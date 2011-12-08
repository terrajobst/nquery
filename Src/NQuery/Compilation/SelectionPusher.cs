using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class SelectionPusher : StandardVisitor
	{
		#region Helpers

		private static FilterAlgebraNode GetFilterFromAndParts(IList<ExpressionNode> andParts, AlgebraNode input)
		{
			FilterAlgebraNode filterAlgebraNode = new FilterAlgebraNode();
			filterAlgebraNode.Input = input;
			filterAlgebraNode.Predicate = AstUtil.CombineConditions(LogicalOperator.And, andParts);
			return filterAlgebraNode;
		}

		private AlgebraNode PushOverUnary(FilterAlgebraNode node)
		{
			UnaryAlgebraNode inputNode = (UnaryAlgebraNode) node.Input;
			node.Input = inputNode.Input;
			inputNode.Input = VisitAlgebraNode(node);
			return inputNode;
		}

		private AlgebraNode PushOverValueDefininingUnary(IEnumerable<ValueDefinition> definedValues, FilterAlgebraNode node)
		{
			UnaryAlgebraNode inputNode = (UnaryAlgebraNode)node.Input;
			List<ExpressionNode> nonDependingAndParts = new List<ExpressionNode>();
			List<ExpressionNode> dependingAndParts = new List<ExpressionNode>();

			foreach (ExpressionNode andPart in AstUtil.SplitCondition(LogicalOperator.And, node.Predicate))
			{
				RowBufferEntry[] rowBufferEntries = AstUtil.GetRowBufferEntryReferences(andPart);

				bool dependsOnDefinedValue = false;
				foreach (ValueDefinition definedValue in definedValues)
				{
					if (ArrayHelpers.Contains(rowBufferEntries, definedValue.Target))
					{
						dependsOnDefinedValue = true;
						break;
					}
				}

				if (dependsOnDefinedValue)
					dependingAndParts.Add(andPart);
				else
					nonDependingAndParts.Add(andPart);
			}

			if (nonDependingAndParts.Count > 0)
			{
				node.Predicate = AstUtil.CombineConditions(LogicalOperator.And, dependingAndParts);
				inputNode.Input = GetFilterFromAndParts(nonDependingAndParts, inputNode.Input);

				if (node.Predicate == null)
				{
					node.Input = inputNode.Input;
					inputNode.Input = VisitAlgebraNode(node);
					return inputNode;
				}
			}

			node.Input = VisitAlgebraNode(node.Input);
			return node;
		}

		private AlgebraNode MergeWithFilter(FilterAlgebraNode node)
		{
			// The input is also a filter, so we can merge this predicate with
			// the input filter.

			FilterAlgebraNode inputNode = (FilterAlgebraNode) node.Input;
			inputNode.Predicate = AstUtil.CombineConditions(LogicalOperator.And, inputNode.Predicate, node.Predicate);
			return VisitAlgebraNode(inputNode);
		}

		private AlgebraNode PushOverComputeScalar(FilterAlgebraNode node)
		{
			// Predicates can be pushed over a compute scalar if it does not contain any row buffer entries
			// that are defined by the compute scalar node.

			ComputeScalarAlgebraNode inputNode = (ComputeScalarAlgebraNode) node.Input;
			return PushOverValueDefininingUnary(inputNode.DefinedValues, node);
		}

		private AlgebraNode PushOverAggregate(FilterAlgebraNode node)
		{
			// TODO: It is not that easy.
			//       For example this condition can be pushed 'GroupCol = Value' while this can't: 'GroupCol = Value OR Func(const) = const'
			//       Formally, a predicate can be pushed over an aggregate if and only if all disjuncts of the predicate's CNF do reference
			//       at least one grouping column.
			//       Since this requires cloning of the predicate and heavy rewriting this is not done (yet).
			return base.VisitFilterAlgebraNode(node);
/*
			// Predicates can be pushed over an aggregation if it does not contain any aggregate function.
			AggregateAlgebraNode inputNode = (AggregateAlgebraNode)node.Input;
			return PushOverValueDefininingUnary(inputNode.DefinedValues, node);
*/
		}

		private AlgebraNode PushOverJoin(FilterAlgebraNode node)
		{
			JoinAlgebraNode inputNode = (JoinAlgebraNode) node.Input;

			// Get declared tables of left and right

			RowBufferEntry[] leftDefinedValues = AstUtil.GetDefinedValueEntries(inputNode.Left);
			RowBufferEntry[] rightDefinedValues = AstUtil.GetDefinedValueEntries(inputNode.Right);

			// Obviously, we cannot merge the filter with the join if the join is an outer join
			// (since it would change the join's semantics).
			//
			// Another less obvious restriction is that we cannot merge a filter with the join if
			// the join has a passthru predicate. In case the passthru predicte evaluates to true 
			// the filter would not be applied. However, we are allowed to push the filter the over 
			// join.

			bool canMerge = inputNode.Op != JoinAlgebraNode.JoinOperator.FullOuterJoin &&
			                inputNode.Op != JoinAlgebraNode.JoinOperator.LeftOuterJoin &&
			                inputNode.Op != JoinAlgebraNode.JoinOperator.RightOuterJoin &&
			                inputNode.PassthruPredicate == null;

			if (canMerge)
			{
				// We can merge the filter with the condition of the join.
				//
				// However, we have to make sure that the predicate does not reference the probe column.
				// Since not having a probe column is the most common case, we don't always split the 
				// predicate into conjuncts.

				if (inputNode.ProbeBufferEntry == null || !ArrayHelpers.Contains(AstUtil.GetRowBufferEntryReferences(node.Predicate), inputNode.ProbeBufferEntry))
				{
					// Either there is no probe column defined or the filter does not reference it. That means 
					// no splitting necessary, we can just merge the whole predicate with the join predicate.
					inputNode.Predicate = AstUtil.CombineConditions(LogicalOperator.And, inputNode.Predicate, node.Predicate);
					return VisitAlgebraNode(inputNode);
				}
				else
				{
					// Unfortunately, the filter references the probe column. Now let's check whether we can merge
					// conjuncts of the predicate.

					List<ExpressionNode> remainingAndParts = new List<ExpressionNode>();
					List<ExpressionNode> mergableAndParts = new List<ExpressionNode>();

					foreach (ExpressionNode andPart in AstUtil.SplitCondition(LogicalOperator.And, node.Predicate))
					{
						bool andPartReferencesProbeColumn = ArrayHelpers.Contains(AstUtil.GetRowBufferEntryReferences(andPart), inputNode.ProbeBufferEntry);

						if (andPartReferencesProbeColumn)
							remainingAndParts.Add(andPart);
						else
							mergableAndParts.Add(andPart);
					}

					if (mergableAndParts.Count > 0)
					{
						ExpressionNode combinedMergableAndParts = AstUtil.CombineConditions(LogicalOperator.And, mergableAndParts.ToArray());
						inputNode.Predicate = AstUtil.CombineConditions(LogicalOperator.And, inputNode.Predicate, combinedMergableAndParts);
						node.Predicate = AstUtil.CombineConditions(LogicalOperator.And, remainingAndParts);

						if (node.Predicate == null)
							return VisitAlgebraNode(inputNode);
					}
				}
			}
			else
			{
				// The condition cannot be merged. Now we try to push AND-parts over the join.

				List<ExpressionNode> leftAndParts = new List<ExpressionNode>();
				List<ExpressionNode> rightAndParts = new List<ExpressionNode>();
				List<ExpressionNode> remainingAndParts = new List<ExpressionNode>();

				foreach (ExpressionNode andPart in AstUtil.SplitCondition(LogicalOperator.And, node.Predicate))
				{
					bool andPartReferencesProbeColumn = inputNode.ProbeBufferEntry != null &&
					                                    ArrayHelpers.Contains(AstUtil.GetRowBufferEntryReferences(andPart), inputNode.ProbeBufferEntry);

					if (!andPartReferencesProbeColumn && AstUtil.AllowsLeftPushOver(inputNode.Op) && AstUtil.ExpressionDoesNotReference(andPart, rightDefinedValues))
					{
						// The AND-part depends only on the LHS and the join is inner/left. 
						// So we are allowed to push this AND-part down.
						leftAndParts.Add(andPart);
					}
					else if (!andPartReferencesProbeColumn && AstUtil.AllowsRightPushOver(inputNode.Op) && AstUtil.ExpressionDoesNotReference(andPart, leftDefinedValues))
					{
						// The AND-part depends only on the RHS and the join is inner/right. 
						// So we are allowed to push this AND-part down.
						rightAndParts.Add(andPart);
					}
					else
					{
						remainingAndParts.Add(andPart);
					}
				}

				if (leftAndParts.Count > 0)
				{
					inputNode.Left = GetFilterFromAndParts(leftAndParts, inputNode.Left);
				}

				if (rightAndParts.Count > 0)
				{
					inputNode.Right = GetFilterFromAndParts(rightAndParts, inputNode.Right);
				}

				node.Predicate = AstUtil.CombineConditions(LogicalOperator.And, remainingAndParts);
				if (node.Predicate == null)
					return VisitAlgebraNode(inputNode);
			}

			node.Input = VisitAlgebraNode(node.Input);
			return node;
		}

		private AlgebraNode PushOverConcat(FilterAlgebraNode node)
		{
			ConcatAlgebraNode inputNode = (ConcatAlgebraNode) node.Input;

			for (int i = 0; i < inputNode.Inputs.Length; i++)
			{
				ExpressionNode predicate = (ExpressionNode)node.Predicate.Clone();
				ConcatRowBufferEntryReplacer concatRowBufferEntryReplacer = new ConcatRowBufferEntryReplacer(inputNode.DefinedValues, i);
				predicate = concatRowBufferEntryReplacer.VisitExpression(predicate);

				FilterAlgebraNode filterAlgebraNode = new FilterAlgebraNode();
				filterAlgebraNode.Input = inputNode.Inputs[i];
				filterAlgebraNode.Predicate = predicate;
				inputNode.Inputs[i] = VisitAlgebraNode(filterAlgebraNode);
			}

			return inputNode;
		}

		private sealed class ConcatRowBufferEntryReplacer : StandardVisitor
		{
			private UnitedValueDefinition[] _unitedValues;
			private int _inputIndex;

			public ConcatRowBufferEntryReplacer(UnitedValueDefinition[] unitedValues, int inputIndex)
			{
				_unitedValues = unitedValues;
				_inputIndex = inputIndex;
			}

			public override ExpressionNode VisitRowBufferEntryExpression(RowBufferEntryExpression expression)
			{
				foreach (UnitedValueDefinition unitedValue in _unitedValues)
				{
					if (expression.RowBufferEntry == unitedValue.Target)
						expression.RowBufferEntry = unitedValue.DependendEntries[_inputIndex];
				}

				return expression;
			}
		}

		#endregion

		public override AlgebraNode VisitFilterAlgebraNode(FilterAlgebraNode node)
		{
			switch (node.Input.NodeType)
			{
				case AstNodeType.SortAlgebraNode:
				case AstNodeType.ResultAlgebraNode:
				case AstNodeType.TopAlgebraNode:
					return PushOverUnary(node);

				case AstNodeType.FilterAlgebraNode:
					return MergeWithFilter(node);

				case AstNodeType.ComputeScalarAlgebraNode:
					return PushOverComputeScalar(node);

				case AstNodeType.AggregateAlgebraNode:
					return PushOverAggregate(node);

				case AstNodeType.JoinAlgebraNode:
					return PushOverJoin(node);

				case AstNodeType.ConcatAlgebraNode:
					return PushOverConcat(node);

				default:
					return base.VisitFilterAlgebraNode(node);
			}			            
		}

		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			// Get declared tables of left and right

			RowBufferEntry[] leftDefinedValues = AstUtil.GetDefinedValueEntries(node.Left);
			RowBufferEntry[] rightDefinedValues = AstUtil.GetDefinedValueEntries(node.Right);
            
			// Analyze AND-parts of Condition

			if (node.Predicate != null)
			{
				List<ExpressionNode> leftAndParts = new List<ExpressionNode>();
				List<ExpressionNode> rightAndParts = new List<ExpressionNode>();
				List<ExpressionNode> remainingAndParts = new List<ExpressionNode>();

				foreach (ExpressionNode andPart in AstUtil.SplitCondition(LogicalOperator.And, node.Predicate))
				{
					// Check if we can push this AND-part down.

					if (AstUtil.AllowsLeftPushDown(node.Op) && AstUtil.ExpressionDoesNotReference(andPart, rightDefinedValues))
					{
						leftAndParts.Add(andPart);
					}
					else if (AstUtil.AllowsRightPushDown(node.Op) && AstUtil.ExpressionDoesNotReference(andPart, leftDefinedValues))
					{
						rightAndParts.Add(andPart);
					}
					else
					{
						remainingAndParts.Add(andPart);
					}
				}

				if (leftAndParts.Count > 0)
				{
					node.Left = GetFilterFromAndParts(leftAndParts, node.Left);
				}

				if (rightAndParts.Count > 0)
				{
					node.Right = GetFilterFromAndParts(rightAndParts, node.Right);
				}

				node.Predicate = AstUtil.CombineConditions(LogicalOperator.And, remainingAndParts);
			}

			// Visit children

			node.Left = VisitAlgebraNode(node.Left);
			node.Right = VisitAlgebraNode(node.Right);

			return node;
		}
	}
}