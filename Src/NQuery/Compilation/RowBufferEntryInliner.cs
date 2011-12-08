using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class RowBufferEntryInliner : StandardVisitor
	{
		private Dictionary<RowBufferEntry, RowBufferEntry> _inliningDictionary = new Dictionary<RowBufferEntry, RowBufferEntry>();

		private RowBufferEntry ReplaceRowBufferEntry(RowBufferEntry rowBufferEntry)
		{
			RowBufferEntry replacementEntry;
			if (_inliningDictionary.TryGetValue(rowBufferEntry, out replacementEntry))
				return replacementEntry;

			return rowBufferEntry;
		}

		private void ReplaceRowBufferEntries(RowBufferEntry[] rowBufferEntries)
		{
			for (int i = 0; i < rowBufferEntries.Length; i++)
				rowBufferEntries[i] = ReplaceRowBufferEntry(rowBufferEntries[i]);
		}

		public override ExpressionNode VisitRowBufferEntryExpression(RowBufferEntryExpression expression)
		{
			expression.RowBufferEntry = ReplaceRowBufferEntry(expression.RowBufferEntry);

			return expression;
		}

		public override AlgebraNode VisitResultAlgebraNode(ResultAlgebraNode node)
		{
			base.VisitResultAlgebraNode(node);
			ReplaceRowBufferEntries(node.OutputList);

			return node;
		}

		public override AlgebraNode VisitComputeScalarAlgebraNode(ComputeScalarAlgebraNode node)
		{
			base.VisitComputeScalarAlgebraNode(node);

			List<ComputedValueDefinition> remainingDefinedValues = new List<ComputedValueDefinition>();

			foreach (ComputedValueDefinition definedValue in node.DefinedValues)
			{
				RowBufferEntryExpression rowBufferEntryExpression = definedValue.Expression as RowBufferEntryExpression;
				if (rowBufferEntryExpression != null)
					_inliningDictionary[definedValue.Target] = rowBufferEntryExpression.RowBufferEntry;
				else
					remainingDefinedValues.Add(definedValue);
			}

			if (remainingDefinedValues.Count == 0)
				return node.Input;

			node.DefinedValues = remainingDefinedValues.ToArray();
			return node;
		}

		public override AlgebraNode VisitConcatAlgebraNode(ConcatAlgebraNode node)
		{
			base.VisitConcatAlgebraNode(node);

			foreach (UnitedValueDefinition definedValue in node.DefinedValues)
				ReplaceRowBufferEntries(definedValue.DependendEntries);

			return node;
		}

		public override AlgebraNode VisitSortAlgebraNode(SortAlgebraNode node)
		{
			base.VisitSortAlgebraNode(node);
			ReplaceRowBufferEntries(node.SortEntries);
			return node;
		}

		public override AlgebraNode VisitAggregateAlgebraNode(AggregateAlgebraNode node)
		{
			base.VisitAggregateAlgebraNode(node);

			if (node.Groups != null)
				ReplaceRowBufferEntries(node.Groups);

			return node;
		}

		public override AlgebraNode VisitTopAlgebraNode(TopAlgebraNode node)
		{
			base.VisitTopAlgebraNode(node);

			if (node.TieEntries != null)
				ReplaceRowBufferEntries(node.TieEntries);

			return node;
		}

		public override AlgebraNode VisitIndexSpoolAlgebraNode(IndexSpoolAlgebraNode node)
		{
			base.VisitIndexSpoolAlgebraNode(node);

			node.IndexEntry = ReplaceRowBufferEntry(node.IndexEntry);

			return node;
		}
	}
}