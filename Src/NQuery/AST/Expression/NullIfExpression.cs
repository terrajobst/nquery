using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class NullIfExpression : ExpressionNode
	{
		private ExpressionNode _leftExpression;
		private ExpressionNode _rightExpression;
		
		public NullIfExpression()
		{
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.NullIfExpression; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			NullIfExpression result = new NullIfExpression();
			result.LeftExpression = (ExpressionNode)_leftExpression.Clone(alreadyClonedElements);
			result.RightExpression = (ExpressionNode)_rightExpression.Clone(alreadyClonedElements);
			return result;
		}

		public override Type ExpressionType
		{
			get { return null; }
		}

		public ExpressionNode LeftExpression
		{
			get { return _leftExpression; }
			set { _leftExpression = value; }
		}

		public ExpressionNode RightExpression
		{
			get { return _rightExpression; }
			set { _rightExpression = value; }
		}

		public override object GetValue()
		{
			// NullIfExpression entries in the AST are replaced by CASE expressions.

			throw ExceptionBuilder.InternalErrorGetValueNotSupported(GetType());
		}
	}
}
