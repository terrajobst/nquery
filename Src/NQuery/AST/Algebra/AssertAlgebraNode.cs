using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal enum AssertionType
	{
		MaxOneRow,
		BelowRecursionLimit
	}

	internal sealed class AssertAlgebraNode : UnaryAlgebraNode
	{
		private ExpressionNode _predicate;
		private AssertionType _assertionType;

		public AssertAlgebraNode()
		{
		}

		public ExpressionNode Predicate
		{
			get { return _predicate; }
			set { _predicate = value; }
		}

		public AssertionType AssertionType
		{
			get { return _assertionType; }
			set { _assertionType = value; }
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.AssertAlgebraNode; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			AssertAlgebraNode result = new AssertAlgebraNode();
			result.StatisticsIterator = StatisticsIterator;
			result.OutputList = ArrayHelpers.Clone(OutputList);
			result.Input = (AlgebraNode)Input.Clone(alreadyClonedElements);
			result.Predicate = (ExpressionNode)_predicate.Clone(alreadyClonedElements);
			result.AssertionType = _assertionType;
			return result;
		}
	}
}