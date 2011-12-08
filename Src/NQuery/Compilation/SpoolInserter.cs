using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	// TODO: We must distinguish between Layz Spool and Eager Spool. Otherwise we might insert
	//       a spool though the input is dependent on an outer reference. In this case the spool
	//       would need to be invalidated if the value of the outer reference changes. Otherwise
	//       the spool would return incorrect results.

	internal sealed class SpoolInserter : StandardVisitor
	{
		#region Helpers

		private Stack<RowBufferEntry[]> _outerReferences = new Stack<RowBufferEntry[]>();

		private sealed class SpoolExpression
		{
			public ExpressionNode IndexExpression;
			public ExpressionNode ProbeExpression;
		}

		private sealed class SpoolExpressionExtractor : StandardVisitor
		{
			private List<SpoolExpression> _spoolExpressions = new List<SpoolExpression>();
			private IEnumerable<RowBufferEntry[]> _outerReferences;

			public SpoolExpressionExtractor(IEnumerable<RowBufferEntry[]> outerReferences)
			{
				_outerReferences = outerReferences;
			}

			private bool IsOuterReference(RowBufferEntry entry)
			{
				foreach (RowBufferEntry[] outerReferences in _outerReferences)
				{
					if (ArrayHelpers.Contains(outerReferences, entry))
						return true;
				}

				return false;
			}

			public SpoolExpression[] GetSpoolExpressions()
			{
				return _spoolExpressions.ToArray();
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
						bool leftIsOuter = IsOuterReference(leftRowBufferEntry);
						bool rightIsOuter = IsOuterReference(rightRowBufferEntry);

						if (leftRowBufferEntry != rightRowBufferEntry && leftIsOuter ^ rightIsOuter)
						{
							// Both expressions depend on extactly one row buffer entry but
							// they are not refering to the same row buffer entry and
							// only one is an outer reference.

							SpoolExpression spoolExpression = new SpoolExpression();
							if (leftIsOuter)
							{
								spoolExpression.IndexExpression = expression.Right;
								spoolExpression.ProbeExpression = expression.Left;
							}
							else
							{
								spoolExpression.IndexExpression = expression.Left;
								spoolExpression.ProbeExpression = expression.Right;
							}

							_spoolExpressions.Add(spoolExpression);
							return LiteralExpression.FromBoolean(true);
						}
					}
				}

				return expression;
			}
		}

		private bool CheckIfNodeHasDependenciesToOuterReferences(AlgebraNode node)
		{
			foreach (RowBufferEntry rowBufferEntryReference in AstUtil.GetRowBufferEntryReferences(node))
			{
				foreach (RowBufferEntry[] outerReferences in _outerReferences)
				{
					if (ArrayHelpers.Contains(outerReferences, rowBufferEntryReference))
						return true;
				}
			}
			return false;
		}

		#endregion

		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			node.Left = VisitAlgebraNode(node.Left);

			RowBufferEntry[] outerReferences = AstUtil.GetOuterReferences(node);
			if (outerReferences != null && outerReferences.Length > 0)
				_outerReferences.Push(node.OuterReferences);

			node.Right = VisitAlgebraNode(node.Right);

			if (outerReferences != null && outerReferences.Length > 0)
				_outerReferences.Pop();

			return node;
		}

		public override AlgebraNode VisitFilterAlgebraNode(FilterAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);

			ExpressionNode originalPredicate = (ExpressionNode) node.Predicate.Clone();
			SpoolExpressionExtractor spoolExpressionExtractor = new SpoolExpressionExtractor(_outerReferences);
			// HACK: This hack ensures that TRUE literals introduced by SpoolExpressionExtractor are removed.
			node.Predicate = AstUtil.CombineConditions(LogicalOperator.And, spoolExpressionExtractor.VisitExpression(node.Predicate));
			SpoolExpression[] spoolExpressions = spoolExpressionExtractor.GetSpoolExpressions();

			// Now we must check that the remaining filter incl. input to the filter don't reference any other
			// outer reference.

			bool remainingFilterHasDependenciesToOuterReferences = CheckIfNodeHasDependenciesToOuterReferences(node);

			if (remainingFilterHasDependenciesToOuterReferences)
			{
				// OK; we cannot insert a spool operation here. Undo the expression replacement.
				node.Predicate = originalPredicate;
			}
			else if (spoolExpressions.Length > 0)
			{
				SpoolExpression spoolExpression = spoolExpressions[0];

				AlgebraNode currentInput;

				if (node.Predicate is ConstantExpression)
					currentInput = node.Input;
				else
					currentInput = node;

				RowBufferEntry indexEntry;
				RowBufferEntryExpression indexExpressionAsRowBufferEntryExpression = spoolExpression.IndexExpression as RowBufferEntryExpression;

				if (indexExpressionAsRowBufferEntryExpression != null)
				{
					indexEntry = indexExpressionAsRowBufferEntryExpression.RowBufferEntry;
				}
				else
				{
					indexEntry = new RowBufferEntry(spoolExpression.IndexExpression.ExpressionType);
					ComputedValueDefinition definedValue = new ComputedValueDefinition();
					definedValue.Target = indexEntry;
					definedValue.Expression = spoolExpression.IndexExpression;

					ComputeScalarAlgebraNode computeScalarAlgebraNode = new ComputeScalarAlgebraNode();
					computeScalarAlgebraNode.Input = currentInput;
					computeScalarAlgebraNode.DefinedValues = new ComputedValueDefinition[] { definedValue };
					currentInput = computeScalarAlgebraNode;
				}

				IndexSpoolAlgebraNode indexSpoolAlgebraNode = new IndexSpoolAlgebraNode();
				indexSpoolAlgebraNode.Input = currentInput;
				indexSpoolAlgebraNode.IndexEntry = indexEntry;
				indexSpoolAlgebraNode.ProbeExpression = spoolExpression.ProbeExpression;
				return indexSpoolAlgebraNode;
			}

			return node;
		}
	}
}