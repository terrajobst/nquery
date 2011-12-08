using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class IsNullExpression : ExpressionNode
	{
		private bool _negated;
		private ExpressionNode _expression;

		public IsNullExpression(bool negated, ExpressionNode expression)
		{
			_negated = negated;
			_expression = expression;
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.IsNullExpression; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			return new IsNullExpression(_negated, (ExpressionNode)_expression.Clone(alreadyClonedElements));
		}

		public override Type ExpressionType
		{
			get { return typeof (bool); }
		}

		public override object GetValue()
		{
			object expressionValue = _expression.GetValue();

			if (_negated)
				return expressionValue != null;
			else
				return expressionValue == null;
		}

		public bool Negated
		{
			get { return _negated; }
			set { _negated = value; }
		}

		public ExpressionNode Expression
		{
			get { return _expression; }

			set { _expression = value; }
		}
	}
}