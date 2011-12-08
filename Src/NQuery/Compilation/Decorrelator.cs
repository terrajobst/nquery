using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class Decorrelator : StandardVisitor
	{
		#region Helpers

		private static bool AndPartHasOuterReference(ExpressionNode andPart, RowBufferEntry[] definedValues)
		{
			RowBufferEntry[] rowBufferEntries = AstUtil.GetRowBufferEntryReferences(andPart);
			foreach (RowBufferEntry rowBufferEntry in rowBufferEntries)
			{
				if (!ArrayHelpers.Contains(definedValues, rowBufferEntry))
					return true;
			}

			return false;
		}

		private static bool AndPartHasOuterReference(ExpressionNode andPart, RowBufferEntry[] leftDefinedValues, RowBufferEntry[] rightDefinedValues)
		{
			RowBufferEntry[] rowBufferEntries = AstUtil.GetRowBufferEntryReferences(andPart);
			foreach (RowBufferEntry rowBufferEntry in rowBufferEntries)
			{
				if (!ArrayHelpers.Contains(leftDefinedValues, rowBufferEntry) &&
					!ArrayHelpers.Contains(rightDefinedValues, rowBufferEntry))
					return true;
			}

			return false;
		}

		private static bool SemiJoinDoesNotDependOn(JoinAlgebraNode.JoinOperator op, ExpressionNode part, RowBufferEntry[] leftDefinedValues, RowBufferEntry[] rightDefinedValues)
		{
			if (op == JoinAlgebraNode.JoinOperator.LeftSemiJoin ||
				op == JoinAlgebraNode.JoinOperator.LeftAntiSemiJoin)
			{
				return AstUtil.ExpressionDoesNotReference(part, rightDefinedValues);
			}

			if (op == JoinAlgebraNode.JoinOperator.RightSemiJoin ||
				op == JoinAlgebraNode.JoinOperator.RightAntiSemiJoin)
			{
				return AstUtil.ExpressionDoesNotReference(part, leftDefinedValues);
			}

			return true;
		}

		private AlgebraNode PullFilterUp(UnaryAlgebraNode unaryAlgebraNode)
		{
			unaryAlgebraNode.Input = VisitAlgebraNode(unaryAlgebraNode.Input);

			FilterAlgebraNode filterAlgebraNode = unaryAlgebraNode.Input as FilterAlgebraNode;
			if (filterAlgebraNode == null)
			{
				return unaryAlgebraNode;
			}
			else
			{
				unaryAlgebraNode.Input = filterAlgebraNode.Input;
				filterAlgebraNode.Input = unaryAlgebraNode;
				return filterAlgebraNode;
			}
		}

		#endregion

		public override AlgebraNode VisitSortAlgebraNode(SortAlgebraNode node)
		{
			return PullFilterUp(node);
		}

		public override AlgebraNode VisitComputeScalarAlgebraNode(ComputeScalarAlgebraNode node)
		{
			return PullFilterUp(node);
		}

		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			node.Left = VisitAlgebraNode(node.Left);
			node.Right = VisitAlgebraNode(node.Right);
            
			// Get defined values of left and right

			RowBufferEntry[] leftDefinedValues = AstUtil.GetDefinedValueEntries(node.Left);
			RowBufferEntry[] rightDefinedValues = AstUtil.GetDefinedValueEntries(node.Right);
            
			List<ExpressionNode> andPartsWithinJoin = new List<ExpressionNode>();

			// Try to pull up AND-parts that contain outer references from a left sided filter and combine 
			// them with the join predicate.
			//
			// NOTE: This is only possible if the join is not a LEFT OUTER or FULL OUTER JOIN since this
			// operation would change the join's semantic.

			if (node.Op != JoinAlgebraNode.JoinOperator.LeftOuterJoin &&
			    node.Op != JoinAlgebraNode.JoinOperator.FullOuterJoin)
			{
				FilterAlgebraNode leftAsFilter = node.Left as FilterAlgebraNode;
				if (leftAsFilter != null)
				{
					List<ExpressionNode> remainingAndParts = new List<ExpressionNode>();
					foreach (ExpressionNode andPart in AstUtil.SplitCondition(LogicalOperator.And, leftAsFilter.Predicate))
					{
						if (AndPartHasOuterReference(andPart, leftDefinedValues))
							andPartsWithinJoin.Add(andPart);
						else
							remainingAndParts.Add(andPart);
					}

					leftAsFilter.Predicate = AstUtil.CombineConditions(LogicalOperator.And, remainingAndParts);
					if (leftAsFilter.Predicate == null)
						node.Left = leftAsFilter.Input;
				}
			}

			// Try to pull up AND-parts that contain outer references from a right sided filter and combine 
			// them with the join predicate.
			//
			// NOTE: This is only possible if the join is not a RIGHT OUTER or FULL OUTER JOIN since this
			// operation would change the join's semantic.

			if (node.Op != JoinAlgebraNode.JoinOperator.RightOuterJoin &&
			    node.Op != JoinAlgebraNode.JoinOperator.FullOuterJoin)
			{
				FilterAlgebraNode rightAsFilter = node.Right as FilterAlgebraNode;
				if (rightAsFilter != null)
				{
					List<ExpressionNode> remainingAndParts = new List<ExpressionNode>();
					foreach (ExpressionNode andPart in AstUtil.SplitCondition(LogicalOperator.And, rightAsFilter.Predicate))
					{
						if (AndPartHasOuterReference(andPart, rightDefinedValues))
							andPartsWithinJoin.Add(andPart);
						else 
							remainingAndParts.Add(andPart);
					}

					rightAsFilter.Predicate = AstUtil.CombineConditions(LogicalOperator.And, remainingAndParts);
					if (rightAsFilter.Predicate == null)
						node.Right = rightAsFilter.Input;
				}
			}

			// If we found any AND-parts that could be pulled up, merge them with the join predicate.

			if (andPartsWithinJoin.Count > 0)
				node.Predicate = AstUtil.CombineConditions(LogicalOperator.And, node.Predicate, AstUtil.CombineConditions(LogicalOperator.And, andPartsWithinJoin));

			// Now we try to extract AND-parts that contain outer references from the join predicate itself.
			//
			// NOTE: This is only possible if the node is not an OUTER JOIN. If the node is a SEMI JOIN the
			// operation is only legal if the AND-part does not reference any columns from the side that is
			// is used as filter criteria (i.e. for LSJ this is the right side, for RSJ this is the left
			// side).

			if (node.Op != JoinAlgebraNode.JoinOperator.LeftOuterJoin &&
			    node.Op != JoinAlgebraNode.JoinOperator.RightOuterJoin &&
			    node.Op != JoinAlgebraNode.JoinOperator.FullOuterJoin &&
			    node.Predicate != null)
			{
				List<ExpressionNode> andPartsAboveJoin = new List<ExpressionNode>();
				List<ExpressionNode> remainingAndParts = new List<ExpressionNode>();

				foreach (ExpressionNode andPart in AstUtil.SplitCondition(LogicalOperator.And, node.Predicate))
				{
					if (AndPartHasOuterReference(andPart, leftDefinedValues, rightDefinedValues) && 
						SemiJoinDoesNotDependOn(node.Op, andPart, leftDefinedValues, rightDefinedValues))
						andPartsAboveJoin.Add(andPart);
					else
						remainingAndParts.Add(andPart);
				}

				node.Predicate = AstUtil.CombineConditions(LogicalOperator.And, remainingAndParts);
                
				if (andPartsAboveJoin.Count > 0)
				{
					FilterAlgebraNode filterAlgebraNode = new FilterAlgebraNode();
					filterAlgebraNode.Predicate = AstUtil.CombineConditions(LogicalOperator.And, andPartsAboveJoin);
					filterAlgebraNode.Input = node;
					return filterAlgebraNode;
				}
			}
                                    
			return node;
		}
	}
}