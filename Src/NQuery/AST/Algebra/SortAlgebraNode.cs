using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class SortAlgebraNode : UnaryAlgebraNode
	{
		private bool _distinct;
		private RowBufferEntry[] _sortEntries;
		private SortOrder[] _sortOrders;

		public SortAlgebraNode()
		{
		}

		public bool Distinct
		{
			get { return _distinct; }
			set { _distinct = value; }
		}

		public RowBufferEntry[] SortEntries
		{
			get { return _sortEntries; }
			set { _sortEntries = value; }
		}

		public SortOrder[] SortOrders
		{
			get { return _sortOrders; }
			set { _sortOrders = value; }
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.SortAlgebraNode; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			SortAlgebraNode result = new SortAlgebraNode();
			result.StatisticsIterator = StatisticsIterator;
			result.OutputList = ArrayHelpers.Clone(OutputList);
			result.Input = (AlgebraNode)Input.Clone(alreadyClonedElements);
			result.Distinct = _distinct;
			result.SortEntries = ArrayHelpers.Clone(_sortEntries);
			result.SortOrders = ArrayHelpers.Clone(_sortOrders);
			return result;
		}
	}
}