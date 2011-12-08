using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class ComputeScalarAlgebraNode : UnaryAlgebraNode
	{
		private ComputedValueDefinition[] _definedValues;

		public ComputeScalarAlgebraNode()
		{
		}

		public ComputedValueDefinition[] DefinedValues
		{
			get { return _definedValues; }
			set { _definedValues = value; }
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.ComputeScalarAlgebraNode; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			ComputeScalarAlgebraNode result = new ComputeScalarAlgebraNode();
			result.StatisticsIterator = StatisticsIterator;
			result.OutputList = ArrayHelpers.Clone(OutputList);
			result.Input = (AlgebraNode)Input.Clone(alreadyClonedElements);
			result.DefinedValues = ArrayHelpers.CreateDeepCopyOfAstElementArray(_definedValues, alreadyClonedElements);
			return result;
		}
	}
}