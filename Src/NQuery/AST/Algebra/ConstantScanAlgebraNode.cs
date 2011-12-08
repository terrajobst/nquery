using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class ConstantScanAlgebraNode : AlgebraNode
	{
		private ComputedValueDefinition[] _definedValues;

		public ConstantScanAlgebraNode()
		{
		}

		public ComputedValueDefinition[] DefinedValues
		{
			get { return _definedValues; }
			set { _definedValues = value; }
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.ConstantScanAlgebraNode; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			ConstantScanAlgebraNode result = new ConstantScanAlgebraNode();
			result.StatisticsIterator = StatisticsIterator;
			result.OutputList = ArrayHelpers.Clone(OutputList);
			result.DefinedValues = ArrayHelpers.CreateDeepCopyOfAstElementArray(_definedValues, alreadyClonedElements);
			return result;
		}
	}
}