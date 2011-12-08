using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class AggregateAlgebraNode : UnaryAlgebraNode
	{
		private RowBufferEntry[] _groups;
		private AggregatedValueDefinition[] _definedValues;

		public AggregateAlgebraNode()
		{
		}

		public RowBufferEntry[] Groups
		{
			get { return _groups; }
			set { _groups = value; }
		}

		public AggregatedValueDefinition[] DefinedValues
		{
			get { return _definedValues; }
			set { _definedValues = value; }
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.AggregateAlgebraNode; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			AggregateAlgebraNode result = new AggregateAlgebraNode();
			result.StatisticsIterator = StatisticsIterator;
			result.OutputList = ArrayHelpers.Clone(OutputList);
			result.Input = (AlgebraNode)Input.Clone(alreadyClonedElements);
			result.Groups = ArrayHelpers.Clone(_groups);
			result.DefinedValues = ArrayHelpers.CreateDeepCopyOfAstElementArray(_definedValues, alreadyClonedElements);
			return result;
		}
	}
}