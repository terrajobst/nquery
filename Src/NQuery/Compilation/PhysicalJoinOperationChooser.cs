using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class PhysicalJoinOperationChooser : StandardVisitor
	{
		private sealed class EqualPredicatesExtractor : StandardVisitor
		{
			private RowBufferEntry[] _leftDefinedEntries;
			private RowBufferEntry[] _rightDefinedEntries;
			private List<BinaryExpression> _equalPredicates = new List<BinaryExpression>();

			public EqualPredicatesExtractor(RowBufferEntry[] leftDefinedEntries, RowBufferEntry[] rightDefinedEntries)
			{
				_leftDefinedEntries = leftDefinedEntries;
				_rightDefinedEntries = rightDefinedEntries;
			}

			public BinaryExpression[] GetEqualPredicates()
			{
				return _equalPredicates.ToArray();
			}

			public override ExpressionNode VisitBinaryExpression(BinaryExpression expression)
			{
				if (expression.Op == BinaryOperator.LogicalAnd)
				{
					return base.VisitBinaryExpression(expression);
				}
				else if (expression.Op == BinaryOperator.Equal)
				{
					RowBufferEntry[] leftRowBufferEntries = AstUtil.GetRowBufferEntryReferences(expression.Left);
					RowBufferEntry[] rightRowBufferEntries = AstUtil.GetRowBufferEntryReferences(expression.Right);

					if (leftRowBufferEntries.Length == 1 && rightRowBufferEntries.Length == 1)
					{
						RowBufferEntry leftRowBufferEntry = leftRowBufferEntries[0];
						RowBufferEntry rightRowBufferEntry = rightRowBufferEntries[0];

						if (leftRowBufferEntry != rightRowBufferEntry)
						{
							// Both expressions depend on extactly one row buffer entry but
							// they are not refering to the same row buffer entry.

							bool leftDependsOnLeft= ArrayHelpers.Contains(_leftDefinedEntries, leftRowBufferEntry);
							bool rightDependsOnRight = ArrayHelpers.Contains(_rightDefinedEntries, rightRowBufferEntry);

							bool leftDependsOnRight = ArrayHelpers.Contains(_rightDefinedEntries, leftRowBufferEntry);
							bool rightDependsOnLeft = ArrayHelpers.Contains(_leftDefinedEntries, rightRowBufferEntry);

							if (leftDependsOnRight && rightDependsOnLeft)
							{
								ExpressionNode oldLeft = expression.Left;
								expression.Left = expression.Right;
								expression.Right = oldLeft;
								leftDependsOnLeft = true;
								rightDependsOnRight = true;
							}

							if (leftDependsOnLeft && rightDependsOnRight)
							{
								_equalPredicates.Add(expression);
								return LiteralExpression.FromBoolean(true);
							}
						}
					}
				}

				return expression;
			}
		}

		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			node.Left = VisitAlgebraNode(node.Left);
			node.Right = VisitAlgebraNode(node.Right);

			if (node.Predicate != null && 
				(node.OuterReferences == null || node.OuterReferences.Length == 0) &&
				(
					node.Op == JoinAlgebraNode.JoinOperator.InnerJoin || 
					node.Op == JoinAlgebraNode.JoinOperator.LeftOuterJoin ||
					node.Op == JoinAlgebraNode.JoinOperator.RightOuterJoin ||
					node.Op == JoinAlgebraNode.JoinOperator.FullOuterJoin)
				)
			{
				RowBufferEntry[] leftDefinedEntries = AstUtil.GetDefinedValueEntries(node.Left);
				RowBufferEntry[] rightDefinedEntries = AstUtil.GetDefinedValueEntries(node.Right);

				EqualPredicatesExtractor equalPredicatesExtractor = new EqualPredicatesExtractor(leftDefinedEntries, rightDefinedEntries);
				ExpressionNode probeResidual = equalPredicatesExtractor.VisitExpression(node.Predicate);
				BinaryExpression[] equalPredicates = equalPredicatesExtractor.GetEqualPredicates();

				if (equalPredicates.Length > 0)
				{
					BinaryExpression equalPredicate = equalPredicates[0];

					ExpressionBuilder expressionBuilder = new ExpressionBuilder();
					expressionBuilder.Push(probeResidual);

					if (equalPredicates.Length > 1)
					{
						for (int i = 1; i < equalPredicates.Length; i++)
							expressionBuilder.Push(equalPredicates[i]);
						expressionBuilder.PushNAry(LogicalOperator.And);
					}

					probeResidual = expressionBuilder.Pop();
					if (probeResidual is ConstantExpression)
						probeResidual = null;

					AlgebraNode leftInput = node.Left;
					AlgebraNode rightInput = node.Right;

					if (node.Op == JoinAlgebraNode.JoinOperator.LeftOuterJoin)
					{
						node.Op = JoinAlgebraNode.JoinOperator.RightOuterJoin;
						leftInput = node.Right;
						rightInput = node.Left;
						ExpressionNode oldLeft = equalPredicate.Left;
						equalPredicate.Left = equalPredicate.Right;
						equalPredicate.Right = oldLeft;
					}

					RowBufferEntry leftEntry;
					RowBufferEntryExpression leftAsRowBufferEntryExpression = equalPredicate.Left as RowBufferEntryExpression;
					if (leftAsRowBufferEntryExpression != null)
					{
						leftEntry = leftAsRowBufferEntryExpression.RowBufferEntry;
					}
					else
					{
						leftEntry = new RowBufferEntry(equalPredicate.Left.ExpressionType);
						ComputedValueDefinition definedValue = new ComputedValueDefinition();
						definedValue.Target = leftEntry;
						definedValue.Expression = equalPredicate.Left;

						ComputeScalarAlgebraNode computeScalarAlgebraNode = new ComputeScalarAlgebraNode();
						computeScalarAlgebraNode.Input = leftInput;
						computeScalarAlgebraNode.DefinedValues = new ComputedValueDefinition[] {definedValue};
						leftInput = computeScalarAlgebraNode;
					}

					RowBufferEntry rightEntry;
					RowBufferEntryExpression rightAsRowBufferEntryExpression = equalPredicate.Right as RowBufferEntryExpression;
					if (rightAsRowBufferEntryExpression != null)
					{
						rightEntry = rightAsRowBufferEntryExpression.RowBufferEntry;
					}
					else
					{
						rightEntry = new RowBufferEntry(equalPredicate.Right.ExpressionType);
						ComputedValueDefinition definedValue = new ComputedValueDefinition();
						definedValue.Target = rightEntry;
						definedValue.Expression = equalPredicate.Right;

						ComputeScalarAlgebraNode computeScalarAlgebraNode = new ComputeScalarAlgebraNode();
						computeScalarAlgebraNode.Input = rightInput;
						computeScalarAlgebraNode.DefinedValues = new ComputedValueDefinition[] {definedValue};
						rightInput = computeScalarAlgebraNode;
					}

					HashMatchAlgebraNode hashMatchAlgebraNode = new HashMatchAlgebraNode();
					hashMatchAlgebraNode.Op = node.Op;
					hashMatchAlgebraNode.Left = leftInput;
					hashMatchAlgebraNode.Right = rightInput;
					hashMatchAlgebraNode.BuildKeyEntry = leftEntry;
					hashMatchAlgebraNode.ProbeEntry = rightEntry;
					hashMatchAlgebraNode.ProbeResidual = probeResidual;
					return hashMatchAlgebraNode;
				}
			}

			return node;
		}
	}
}