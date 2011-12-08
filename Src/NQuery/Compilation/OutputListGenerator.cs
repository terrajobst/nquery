using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class OutputListGenerator : StandardVisitor
	{
		private bool _isFirstResultNode = true;

		#region Helpers

		private static RowBufferEntry[] GetRowBufferEntries(IEnumerable<ValueDefinition> definedValues)
		{
			List<RowBufferEntry> result = new List<RowBufferEntry>();
			foreach (ValueDefinition value in definedValues)
				result.Add(value.Target);
			return result.ToArray();
		}

		#endregion

		public override AlgebraNode VisitResultAlgebraNode(ResultAlgebraNode node)
		{
			bool wasFirstResultNode = _isFirstResultNode;
			if (_isFirstResultNode)
				_isFirstResultNode = false;

			node.Input = VisitAlgebraNode(node.Input);

			if (wasFirstResultNode)
				return node;

			return node.Input;
		}

		public override AlgebraNode VisitTableAlgebraNode(TableAlgebraNode node)
		{
			node.OutputList = GetRowBufferEntries(node.DefinedValues);
			return node;
		}

		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			node.Left = VisitAlgebraNode(node.Left);
			node.Right = VisitAlgebraNode(node.Right);

			List<RowBufferEntry> outputList = new List<RowBufferEntry>();
			outputList.AddRange(node.Left.OutputList);
			outputList.AddRange(node.Right.OutputList);
			if (node.ProbeBufferEntry != null)
				outputList.Add(node.ProbeBufferEntry);
			node.OutputList = outputList.ToArray();

			return node;
		}

		public override AlgebraNode VisitConstantScanAlgebraNode(ConstantScanAlgebraNode node)
		{
			node.OutputList = GetRowBufferEntries(node.DefinedValues);
			return node;
		}

		public override AlgebraNode VisitNullScanAlgebraNode(NullScanAlgebraNode node)
		{
			node.OutputList = new RowBufferEntry[0];
			return node;
		}

		public override AlgebraNode VisitConcatAlgebraNode(ConcatAlgebraNode node)
		{
			for (int i = 0; i < node.Inputs.Length; i++)
				node.Inputs[i] = VisitAlgebraNode(node.Inputs[i]);

			node.OutputList = GetRowBufferEntries(node.DefinedValues);
			return node;
		}

		public override AlgebraNode VisitSortAlgebraNode(SortAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);
			node.OutputList = node.Input.OutputList;
			return node;
		}

		public override AlgebraNode VisitAggregateAlgebraNode(AggregateAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);

			List<RowBufferEntry> outputList = new List<RowBufferEntry>();
			outputList.AddRange(node.Input.OutputList);
			outputList.AddRange(GetRowBufferEntries(node.DefinedValues));
			node.OutputList = outputList.ToArray();

			return node;
		}

		public override AlgebraNode VisitTopAlgebraNode(TopAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);
			node.OutputList = node.Input.OutputList;
			return node;
		}

		public override AlgebraNode VisitFilterAlgebraNode(FilterAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);
			node.OutputList = node.Input.OutputList;
			return node;
		}

		public override AlgebraNode VisitComputeScalarAlgebraNode(ComputeScalarAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);

			List<RowBufferEntry> outputList = new List<RowBufferEntry>();
			outputList.AddRange(node.Input.OutputList);
			outputList.AddRange(GetRowBufferEntries(node.DefinedValues));
			node.OutputList = outputList.ToArray();

			return node;
		}

		public override AlgebraNode VisitAssertAlgebraNode(AssertAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);
			node.OutputList = node.Input.OutputList;
			return node;
		}

		public override AlgebraNode VisitIndexSpoolAlgebraNode(IndexSpoolAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);
			node.OutputList = node.Input.OutputList;
			return node;
		}

		public override AstNode VisitStackedTableSpoolAlgebraNode(StackedTableSpoolAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);
			node.OutputList = node.Input.OutputList;
			return node;
		}

		public override AstNode VisitTableSpoolRefAlgebraNode(StackedTableSpoolRefAlgebraNode node)
		{
			node.OutputList = node.DefinedValues;
			return node;
		}

		public override AlgebraNode VisitHashMatchAlgebraNode(HashMatchAlgebraNode node)
		{
			node.Left = VisitAlgebraNode(node.Left);
			node.Right = VisitAlgebraNode(node.Right);

			List<RowBufferEntry> outputList = new List<RowBufferEntry>();
			outputList.AddRange(node.Left.OutputList);
			outputList.AddRange(node.Right.OutputList);
			node.OutputList = outputList.ToArray();

			return node;
		}
	}
}