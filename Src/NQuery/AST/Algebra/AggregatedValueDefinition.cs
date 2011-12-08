using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class AggregatedValueDefinition : ValueDefinition
	{
		private AggregateBinding _aggregate;
		private IAggregator _aggregator;
		private ExpressionNode _argument;

		public AggregateBinding Aggregate
		{
			get { return _aggregate; }
			set { _aggregate = value; }
		}

		public IAggregator Aggregator
		{
			get { return _aggregator; }
			set { _aggregator = value; }
		}

		public ExpressionNode Argument
		{
			get { return _argument; }
			set { _argument = value; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			AggregatedValueDefinition result = new AggregatedValueDefinition();
			result.Target = Target;
			result.Aggregate = _aggregate;
			result.Aggregator = _aggregator;
			result.Argument = (ExpressionNode)_argument.Clone(alreadyClonedElements);
			return result;
		}
	}
}