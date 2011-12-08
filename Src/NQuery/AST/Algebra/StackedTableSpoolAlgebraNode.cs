using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class StackedTableSpoolAlgebraNode : UnaryAlgebraNode
	{
		public override AstNodeType NodeType
		{
			get { return AstNodeType.StackedTableSpoolAlgebraNode; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			StackedTableSpoolAlgebraNode result = new StackedTableSpoolAlgebraNode();
			alreadyClonedElements.Add(this, result);
			result.StatisticsIterator = StatisticsIterator;
			result.Input = (AlgebraNode)Input.Clone(alreadyClonedElements);
			result.OutputList = ArrayHelpers.Clone(OutputList);
			return result;
		}
	}
}