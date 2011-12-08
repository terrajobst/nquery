using System;
using System.Collections.Generic;
using System.Globalization;

namespace NQuery.Compilation
{
	internal sealed class RowBufferEntryNamer : StandardVisitor
	{
		private const string EXPRESSION_NAME_FMT_STR = "Expr{0}";
		private const string UNION_NAME_FMT_STR = "Union{0}";

		private List<RowBufferEntry> _rowBufferEntries = new List<RowBufferEntry>();

		private void NameEntry(RowBufferEntry target, string formatString)
		{
			if (!_rowBufferEntries.Contains(target))
			{
				if (target.Name == null)
					target.Name = String.Format(CultureInfo.InvariantCulture, formatString, 1000 + _rowBufferEntries.Count);

				_rowBufferEntries.Add(target);
			}
		}

		public override AlgebraNode VisitConstantScanAlgebraNode(ConstantScanAlgebraNode node)
		{
			base.VisitConstantScanAlgebraNode(node);

			foreach (ComputedValueDefinition definedValue in node.DefinedValues)
				NameEntry(definedValue.Target, EXPRESSION_NAME_FMT_STR);

			return node;
		}

		public override AlgebraNode VisitNullScanAlgebraNode(NullScanAlgebraNode node)
		{
			foreach (RowBufferEntry entry in node.OutputList)
				NameEntry(entry, EXPRESSION_NAME_FMT_STR);

			return node;
		}

		public override AlgebraNode VisitTableAlgebraNode(TableAlgebraNode node)
		{
			base.VisitTableAlgebraNode(node);

			foreach (ColumnValueDefinition definedValue in node.DefinedValues)
				_rowBufferEntries.Add(definedValue.Target);

			return node;
		}

		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			base.VisitJoinAlgebraNode(node);

			if (node.ProbeBufferEntry != null)
				NameEntry(node.ProbeBufferEntry, EXPRESSION_NAME_FMT_STR);

			return node;
		}

		public override AlgebraNode VisitConcatAlgebraNode(ConcatAlgebraNode node)
		{
			base.VisitConcatAlgebraNode(node);

			foreach (UnitedValueDefinition definedValue in node.DefinedValues)
				NameEntry(definedValue.Target, UNION_NAME_FMT_STR);

			return node;
		}

		public override AlgebraNode VisitAggregateAlgebraNode(AggregateAlgebraNode node)
		{
			base.VisitAggregateAlgebraNode(node);

			foreach (AggregatedValueDefinition definedValue in node.DefinedValues)
				NameEntry(definedValue.Target, EXPRESSION_NAME_FMT_STR);

			return node;
		}

		public override AlgebraNode VisitComputeScalarAlgebraNode(ComputeScalarAlgebraNode node)
		{
			base.VisitComputeScalarAlgebraNode(node);

			foreach (ComputedValueDefinition definedValue in node.DefinedValues)
				NameEntry(definedValue.Target, EXPRESSION_NAME_FMT_STR);

			return node;
		}

		public override AstNode VisitTableSpoolRefAlgebraNode(StackedTableSpoolRefAlgebraNode node)
		{
			foreach (RowBufferEntry definedValue in node.DefinedValues)
				NameEntry(definedValue, EXPRESSION_NAME_FMT_STR);

			return node;
		}
	}
}