using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class IndexSpoolAlgebraNode : UnaryAlgebraNode
	{
		private RowBufferEntry _indexEntry;
		private ExpressionNode _probeExpression;

		public RowBufferEntry IndexEntry
		{
			get { return _indexEntry; }
			set { _indexEntry = value; }
		}

		public ExpressionNode ProbeExpression
		{
			get { return _probeExpression; }
			set { _probeExpression = value; }
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.IndexSpoolAlgebraNode; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			IndexSpoolAlgebraNode result = new IndexSpoolAlgebraNode();
			result.StatisticsIterator = StatisticsIterator;
			result.OutputList = ArrayHelpers.Clone(OutputList);
			result.Input = (AlgebraNode)Input.Clone(alreadyClonedElements);
			result.IndexEntry = _indexEntry;
			result.ProbeExpression = (ExpressionNode)_probeExpression.Clone(alreadyClonedElements);
			return result;
		}
	}
}