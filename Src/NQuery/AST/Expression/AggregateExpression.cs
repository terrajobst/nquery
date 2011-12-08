using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class AggregateExpression : ExpressionNode
	{
		private AggregateBinding _aggregate;
		private IAggregator _aggregator;
		private ExpressionNode _argument;
		private bool _hasAsteriskModifier;
		private AggregatedValueDefinition _ValueDefinition;

		public AggregateExpression() 
		{
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			AggregateExpression result = new AggregateExpression();
			result.Aggregate = _aggregate;
			result.Aggregator = _aggregator;
			result.Argument = _argument;
			result.HasAsteriskModifier = _hasAsteriskModifier;
			return result;
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.AggregateExpression; }
		}

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

		public bool HasAsteriskModifier
		{
			get { return _hasAsteriskModifier; }
			set { _hasAsteriskModifier = value; }
		}

		public AggregatedValueDefinition ValueDefinition
		{
			get { return _ValueDefinition; }
			set { _ValueDefinition = value; }
		}

		public override Type ExpressionType
		{
			get { return _aggregator.ReturnType; }
		}

		public override object GetValue()
		{
			throw ExceptionBuilder.InternalErrorGetValueNotSupported(GetType());
		}
	}
}