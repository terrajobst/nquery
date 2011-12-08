using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class ConcatAlgebraNode : AlgebraNode
	{
		private AlgebraNode[] _inputs;
		private UnitedValueDefinition[] _definedValues;

		public ConcatAlgebraNode()
		{
		}

		public AlgebraNode[] Inputs
		{
			get { return _inputs; }
			set { _inputs = value; }
		}

		public UnitedValueDefinition[] DefinedValues
		{
			get { return _definedValues; }
			set { _definedValues = value; }
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.ConcatAlgebraNode; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			ConcatAlgebraNode result = new ConcatAlgebraNode();
			result.StatisticsIterator = StatisticsIterator;
			result.OutputList = ArrayHelpers.Clone(OutputList);
			result.Inputs = ArrayHelpers.CreateDeepCopyOfAstElementArray(_inputs, alreadyClonedElements);
			result.DefinedValues = ArrayHelpers.CreateDeepCopyOfAstElementArray(_definedValues, alreadyClonedElements);
			return result;
		}
	}
}