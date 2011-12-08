using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NQuery.Compilation
{
	internal sealed class OuterJoinRemover : StandardVisitor
	{
		private List<RowBufferEntry> _nullRejectedRowBufferEntries = new List<RowBufferEntry>();

		#region Helpers

		private void AddNullRejectedTable(RowBufferEntry rowBufferEntries)
		{
			if (!_nullRejectedRowBufferEntries.Contains(rowBufferEntries))
				_nullRejectedRowBufferEntries.Add(rowBufferEntries);
		}

		private bool IsAnyNullRejected(IEnumerable<RowBufferEntry> rowBufferEntries)
		{
			foreach (RowBufferEntry rowBufferEntry in rowBufferEntries)
			{
				if (_nullRejectedRowBufferEntries.Contains(rowBufferEntry))
					return true;
			}

			return false;
		}

		private static AlgebraNode WrapWithFilter(AlgebraNode input, ExpressionNode predicate)
		{
			if (predicate == null)
				return input;

			FilterAlgebraNode filterAlgebraNode = new FilterAlgebraNode();
			filterAlgebraNode.Input = input;
			filterAlgebraNode.Predicate = predicate;
			return filterAlgebraNode;
		}

		#endregion

		public override AlgebraNode VisitFilterAlgebraNode(FilterAlgebraNode node)
		{
			// Check for null rejecting conditions.

			foreach (ExpressionNode andPart in AstUtil.SplitCondition(LogicalOperator.And, node.Predicate))
			{
				RowBufferEntry[] rowBufferEntries = AstUtil.GetRowBufferEntryReferences(andPart);

				foreach (RowBufferEntry rowBufferEntry in rowBufferEntries)
				{
					if (AstUtil.ExpressionYieldsNullOrFalseIfRowBufferEntryNull(andPart, rowBufferEntry))
						AddNullRejectedTable(rowBufferEntry);
				}
			}

			return base.VisitFilterAlgebraNode(node);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			// Get declared tables of left and right

			RowBufferEntry[] leftDefinedValues = AstUtil.GetDefinedValueEntries(node.Left);
			RowBufferEntry[] rightDefinedValues = AstUtil.GetDefinedValueEntries(node.Right);

			// Replace outer joins by left-/right-/inner joins

			if (node.Op == JoinAlgebraNode.JoinOperator.RightOuterJoin || 
			    node.Op == JoinAlgebraNode.JoinOperator.FullOuterJoin)
			{
				if (IsAnyNullRejected(leftDefinedValues))
				{
					if (node.Op == JoinAlgebraNode.JoinOperator.RightOuterJoin)
						node.Op = JoinAlgebraNode.JoinOperator.InnerJoin;
					else
						node.Op = JoinAlgebraNode.JoinOperator.LeftOuterJoin;
				}
			}

			if (node.Op == JoinAlgebraNode.JoinOperator.LeftOuterJoin ||
			    node.Op == JoinAlgebraNode.JoinOperator.FullOuterJoin)
			{
				if (IsAnyNullRejected(rightDefinedValues))
				{
					if (node.Op == JoinAlgebraNode.JoinOperator.LeftOuterJoin)
						node.Op = JoinAlgebraNode.JoinOperator.InnerJoin;
					else
						node.Op = JoinAlgebraNode.JoinOperator.RightOuterJoin;
				}
			}

			// After converting an outer join to an inner one we can
			// sometimes eliminate the whole join.

			if (node.Op == JoinAlgebraNode.JoinOperator.InnerJoin)
			{
				// TODO: There is a problem. If the constant scan defines values this does not work. Acutally, 
				//       this is currently no problem as the only way to create such a plan is using derived 
				//       tables and in this phase the child will be a ResultNode.

				if (node.Left is ConstantScanAlgebraNode)
					return VisitAlgebraNode(WrapWithFilter(node.Right, node.Predicate));

				if (node.Right is ConstantScanAlgebraNode)
					return VisitAlgebraNode(WrapWithFilter(node.Left, node.Predicate));
			}

			// Analyze AND-parts of Condition

			if (node.Predicate == null)
			{
				// TODO: This does not work as the children are not yet rearranged.
				if (node.Op == JoinAlgebraNode.JoinOperator.LeftOuterJoin ||
				    node.Op == JoinAlgebraNode.JoinOperator.RightOuterJoin)
				{
					bool hasOuterReferences = AstUtil.GetOuterReferences(node).Length == 0;
					if (!hasOuterReferences)
					{
						if (node.Op == JoinAlgebraNode.JoinOperator.LeftOuterJoin && AstUtil.WillProduceAtLeastOneRow(node.Right) ||
						    node.Op == JoinAlgebraNode.JoinOperator.RightOuterJoin && AstUtil.WillProduceAtLeastOneRow(node.Left))
						{
							node.Op = JoinAlgebraNode.JoinOperator.InnerJoin;
							return VisitAlgebraNode(node);
						}
					}
				}
			}
			else
			{
				foreach (ExpressionNode andPart in AstUtil.SplitCondition(LogicalOperator.And, node.Predicate))
				{
					if (node.Op != JoinAlgebraNode.JoinOperator.FullOuterJoin)
					{
						// Check if we can derive from this AND-part that a table it depends on
						// is null-rejected.

						RowBufferEntry[] rowBufferEntries = AstUtil.GetRowBufferEntryReferences(andPart);
						foreach (RowBufferEntry rowBufferEntry in rowBufferEntries)
						{
							if (AstUtil.ExpressionYieldsNullOrFalseIfRowBufferEntryNull(andPart, rowBufferEntry))
							{
								if (ArrayHelpers.Contains(leftDefinedValues, rowBufferEntry) &&
								    node.Op != JoinAlgebraNode.JoinOperator.LeftOuterJoin)
								{
									AddNullRejectedTable(rowBufferEntry);
								}
								else if (ArrayHelpers.Contains(rightDefinedValues, rowBufferEntry) &&
								         node.Op != JoinAlgebraNode.JoinOperator.RightOuterJoin)
								{
									AddNullRejectedTable(rowBufferEntry);
								}
							}
						}
					}
				}
			}

			// Visit children

			node.Left = VisitAlgebraNode(node.Left);
			node.Right = VisitAlgebraNode(node.Right);

			return node;
		}
	}
}