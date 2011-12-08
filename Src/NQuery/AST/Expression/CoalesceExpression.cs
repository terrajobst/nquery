using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class CoalesceExpression : ExpressionNode
	{
		private ExpressionNode[] _expressions;

		public CoalesceExpression() 
		{
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.CoalesceExpression; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			CoalesceExpression result = new CoalesceExpression();
			result.Expressions = new ExpressionNode[_expressions.Length];
			for (int i = 0; i < _expressions.Length; i++)
				result.Expressions[i] = (ExpressionNode)_expressions[i].Clone(alreadyClonedElements);
			return result;
		}

		public ExpressionNode[] Expressions
		{
			get { return _expressions; }
			set { _expressions = value; }
		}

		public override Type ExpressionType
		{
			get { return null; }
		}

		public override object GetValue()
		{
			// CoalesceExpression entries in the AST are replaced by CASE expressions.

			throw ExceptionBuilder.InternalErrorGetValueNotSupported(GetType());
		}
	}
}