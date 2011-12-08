using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class NullScanAlgebraNode : AlgebraNode
	{
		public NullScanAlgebraNode()
		{
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.NullScanAlgebraNode; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			NullScanAlgebraNode result = new NullScanAlgebraNode();
			result.StatisticsIterator = StatisticsIterator;
			result.OutputList = ArrayHelpers.Clone(OutputList);
			return result;
		}
	}
}