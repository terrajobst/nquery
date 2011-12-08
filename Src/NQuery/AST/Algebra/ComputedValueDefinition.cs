using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class ComputedValueDefinition : ValueDefinition
	{
		private ExpressionNode _expression;

		public ExpressionNode Expression
		{
			get { return _expression; }
			set { _expression = value; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			ComputedValueDefinition result = new ComputedValueDefinition();
			result.Target = Target;
			result.Expression = (ExpressionNode)_expression.Clone(alreadyClonedElements);
			return result;
		}
	}
}