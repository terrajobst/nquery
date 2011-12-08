using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class FilterAlgebraNode : UnaryAlgebraNode
	{
		private ExpressionNode _predicate;

		public FilterAlgebraNode()
		{
		}

		public ExpressionNode Predicate
		{
			get { return _predicate; }
			set { _predicate = value; }
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.FilterAlgebraNode; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			FilterAlgebraNode result = new FilterAlgebraNode();
			result.StatisticsIterator = StatisticsIterator;
			result.OutputList = ArrayHelpers.Clone(OutputList);
			result.Input = (AlgebraNode)Input.Clone(alreadyClonedElements);
			result.Predicate = (ExpressionNode)_predicate.Clone(alreadyClonedElements);
			return result;
		}
	}
}