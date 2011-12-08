using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class DefinedValuesFinder : StandardVisitor
	{
		private List<ValueDefinition> _definedValues = new List<ValueDefinition>();
		private List<RowBufferEntry> _definedValueEntries = new List<RowBufferEntry>();

		private void AddDefinedValues(IEnumerable<ValueDefinition> definedValues)
		{
			_definedValues.AddRange(definedValues);

			foreach (ValueDefinition definedValue in definedValues)
			{
				if (_definedValues.Contains(definedValue))
					_definedValueEntries.Add(definedValue.Target);
			}
		}

		public RowBufferEntry[] GetDefinedValueEntries()
		{
			return _definedValueEntries.ToArray();
		}

		public override AlgebraNode VisitTableAlgebraNode(TableAlgebraNode node)
		{
			AddDefinedValues(node.DefinedValues);

			return node;
		}

		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			Visit(node.Left);
			Visit(node.Right);
			_definedValueEntries.Add(node.ProbeBufferEntry);

			return node;
		}

		public override AlgebraNode VisitConstantScanAlgebraNode(ConstantScanAlgebraNode node)
		{
			AddDefinedValues(node.DefinedValues);

			return node;
		}

		public override AlgebraNode VisitComputeScalarAlgebraNode(ComputeScalarAlgebraNode node)
		{
			Visit(node.Input);
			AddDefinedValues(node.DefinedValues);

			return node;
		}

		public override AlgebraNode VisitAggregateAlgebraNode(AggregateAlgebraNode node)
		{
			Visit(node.Input);
			AddDefinedValues(node.DefinedValues);

			return node;
		}

		public override AlgebraNode VisitConcatAlgebraNode(ConcatAlgebraNode node)
		{
			foreach (AlgebraNode input in node.Inputs)
				Visit(input);

			if (node.DefinedValues != null)
				AddDefinedValues(node.DefinedValues);

			return node;
		}

		public override AstNode VisitTableSpoolRefAlgebraNode(StackedTableSpoolRefAlgebraNode node)
		{
			_definedValueEntries.AddRange(node.DefinedValues);
			return node;
		}
	}
}