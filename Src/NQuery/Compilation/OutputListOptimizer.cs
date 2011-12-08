using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class OutputListOptimizer : StandardVisitor
	{
		private List<RowBufferEntry> _neededRowBufferColumns = new List<RowBufferEntry>();

		#region Helpers

		private static RowBufferEntry[] GetRowBufferEntries(IEnumerable<ValueDefinition> definedValues)
		{
			List<RowBufferEntry> result = new List<RowBufferEntry>();
			foreach (ValueDefinition value in definedValues)
				result.Add(value.Target);
			return result.ToArray();
		}

		private void AddNeededRowBufferEntry(RowBufferEntry rowBufferEntry)
		{
			if (!IsNeeded(rowBufferEntry))
				_neededRowBufferColumns.Add(rowBufferEntry);
		}

		private void AddNeededRowBufferEntryReferences(ExpressionNode expression)
		{
			RowBufferEntry[] rowBufferEntries = AstUtil.GetRowBufferEntryReferences(expression);
			foreach (RowBufferEntry rowBufferEntry in rowBufferEntries)
				AddNeededRowBufferEntry(rowBufferEntry);
		}

		private bool IsNeeded(RowBufferEntry rowBufferColumn)
		{
			return _neededRowBufferColumns.Contains(rowBufferColumn);
		}

		private RowBufferEntry[] RemovedUnneededRowBufferColumns(IEnumerable<RowBufferEntry> list)
		{
			List<RowBufferEntry> result = new List<RowBufferEntry>();
			foreach (RowBufferEntry rowBufferEntry in list)
			{
				if (_neededRowBufferColumns.Contains(rowBufferEntry))
					result.Add(rowBufferEntry);
			}

			return result.ToArray();
		}

		private T[] RemovedUnneededDefinedValues<T>(IEnumerable<T> definedValues) where T : ValueDefinition
		{
			List<T> result = new List<T>();
			foreach (T definedValue in definedValues)
			{
				if (_neededRowBufferColumns.Contains(definedValue.Target))
					result.Add(definedValue);
			}

			return result.ToArray();
		}

		#endregion

		public override AlgebraNode VisitResultAlgebraNode(ResultAlgebraNode node)
		{
			foreach (RowBufferEntry column in node.OutputList)
				AddNeededRowBufferEntry(column);

			node.Input = VisitAlgebraNode(node.Input);

			return node;
		}

		public override AlgebraNode VisitTableAlgebraNode(TableAlgebraNode node)
		{
			node.DefinedValues = RemovedUnneededDefinedValues(node.DefinedValues);
			node.OutputList = GetRowBufferEntries(node.DefinedValues);
			
			return base.VisitTableAlgebraNode(node);
		}

		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			node.OutputList = RemovedUnneededRowBufferColumns(node.OutputList);
			
			if (node.Predicate != null)
				AddNeededRowBufferEntryReferences(node.Predicate);

			if (node.PassthruPredicate != null)
				AddNeededRowBufferEntryReferences(node.PassthruPredicate);

			if (node.OuterReferences != null)
			{
				foreach (RowBufferEntry outerReference in node.OuterReferences)
					AddNeededRowBufferEntry(outerReference);
			}

			return base.VisitJoinAlgebraNode(node);
		}

		public override AlgebraNode VisitConstantScanAlgebraNode(ConstantScanAlgebraNode node)
		{
			node.DefinedValues = RemovedUnneededDefinedValues(node.DefinedValues);
			node.OutputList = RemovedUnneededRowBufferColumns(node.OutputList);
			return base.VisitConstantScanAlgebraNode(node);
		}

		public override AlgebraNode VisitNullScanAlgebraNode(NullScanAlgebraNode node)
		{
			node.OutputList = RemovedUnneededRowBufferColumns(node.OutputList);
			return base.VisitNullScanAlgebraNode(node);
		}

		public override AlgebraNode VisitConcatAlgebraNode(ConcatAlgebraNode node)
		{
			node.OutputList = RemovedUnneededRowBufferColumns(node.OutputList);

			List<UnitedValueDefinition> definedValues = new List<UnitedValueDefinition>();
			foreach (UnitedValueDefinition definedValue in node.DefinedValues)
			{
				if (IsNeeded(definedValue.Target))
				{
					definedValues.Add(definedValue);
					foreach (RowBufferEntry dependendValue in definedValue.DependendEntries)
						AddNeededRowBufferEntry(dependendValue);
				}
			}
			node.DefinedValues = definedValues.ToArray();
			node.OutputList = GetRowBufferEntries(node.DefinedValues);
			return base.VisitConcatAlgebraNode(node);
		}

		public override AlgebraNode VisitSortAlgebraNode(SortAlgebraNode node)
		{
			node.OutputList = RemovedUnneededRowBufferColumns(node.OutputList);

			foreach (RowBufferEntry sortEntry in node.SortEntries)
				AddNeededRowBufferEntry(sortEntry);

			return base.VisitSortAlgebraNode(node);
		}

		public override AlgebraNode VisitAggregateAlgebraNode(AggregateAlgebraNode node)
		{
			node.OutputList = RemovedUnneededRowBufferColumns(node.OutputList);

			if (node.Groups != null)
			{
				foreach (RowBufferEntry group in node.Groups)
					AddNeededRowBufferEntry(group);
			}

			List<AggregatedValueDefinition> definedValues = new List<AggregatedValueDefinition>();
			foreach (AggregatedValueDefinition definedValue in node.DefinedValues)
			{
				if (IsNeeded(definedValue.Target))
				{
					definedValues.Add(definedValue);
					AddNeededRowBufferEntryReferences(definedValue.Argument);
				}
			}
			node.DefinedValues = definedValues.ToArray();

			return base.VisitAggregateAlgebraNode(node);
		}

		public override AlgebraNode VisitTopAlgebraNode(TopAlgebraNode node)
		{
			node.OutputList = RemovedUnneededRowBufferColumns(node.OutputList);
			
			if (node.TieEntries != null)
				foreach (RowBufferEntry tieColumn in node.TieEntries)
					AddNeededRowBufferEntry(tieColumn);

			return base.VisitTopAlgebraNode(node);
		}

		public override AlgebraNode VisitFilterAlgebraNode(FilterAlgebraNode node)
		{
			node.OutputList = RemovedUnneededRowBufferColumns(node.OutputList);

			AddNeededRowBufferEntryReferences(node.Predicate);

			return base.VisitFilterAlgebraNode(node);
		}

		public override AlgebraNode VisitComputeScalarAlgebraNode(ComputeScalarAlgebraNode node)
		{
			node.OutputList = RemovedUnneededRowBufferColumns(node.OutputList);
			node.DefinedValues = RemovedUnneededDefinedValues(node.DefinedValues);

			if (node.DefinedValues.Length == 0)
				return VisitAlgebraNode(node.Input);

			foreach (ComputedValueDefinition definedValue in node.DefinedValues)
				AddNeededRowBufferEntryReferences(definedValue.Expression);

			return base.VisitComputeScalarAlgebraNode(node);
		}

		public override AlgebraNode VisitAssertAlgebraNode(AssertAlgebraNode node)
		{
			node.OutputList = RemovedUnneededRowBufferColumns(node.OutputList);
			AddNeededRowBufferEntryReferences(node.Predicate);

			return base.VisitAssertAlgebraNode(node);
		}

		public override AlgebraNode VisitIndexSpoolAlgebraNode(IndexSpoolAlgebraNode node)
		{
			node.OutputList = RemovedUnneededRowBufferColumns(node.OutputList);
			AddNeededRowBufferEntry(node.IndexEntry);
			AddNeededRowBufferEntryReferences(node.ProbeExpression);

			return base.VisitIndexSpoolAlgebraNode(node);
		}

		public override AstNode VisitStackedTableSpoolAlgebraNode(StackedTableSpoolAlgebraNode node)
		{
			node.OutputList = RemovedUnneededRowBufferColumns(node.OutputList);
			foreach (RowBufferEntry rowBufferEntry in node.Input.OutputList)
				AddNeededRowBufferEntry(rowBufferEntry);

			return base.VisitStackedTableSpoolAlgebraNode(node);
		}

		public override AlgebraNode VisitHashMatchAlgebraNode(HashMatchAlgebraNode node)
		{
			node.OutputList = RemovedUnneededRowBufferColumns(node.OutputList);

			AddNeededRowBufferEntry(node.BuildKeyEntry);
			AddNeededRowBufferEntry(node.ProbeEntry);

			if (node.ProbeResidual != null)
				AddNeededRowBufferEntryReferences(node.ProbeResidual);

			return base.VisitHashMatchAlgebraNode(node);
		}
	}
}