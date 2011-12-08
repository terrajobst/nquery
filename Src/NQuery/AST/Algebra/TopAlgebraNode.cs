using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class TopAlgebraNode : UnaryAlgebraNode
	{
		private int _limit;
		private RowBufferEntry[] _tieEntries;

		public TopAlgebraNode()
		{
		}

		public int Limit
		{
			get { return _limit; }
			set { _limit = value; }
		}

		public RowBufferEntry[] TieEntries
		{
			get { return _tieEntries; }
			set { _tieEntries = value; }
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.TopAlgebraNode; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			TopAlgebraNode result = new TopAlgebraNode();
			result.StatisticsIterator = StatisticsIterator;
			result.OutputList = ArrayHelpers.Clone(OutputList);
			result.Input = (AlgebraNode)Input.Clone(alreadyClonedElements);
			result.Limit = _limit;
			result.TieEntries = ArrayHelpers.Clone(_tieEntries);
			return result;
		}
	}
}