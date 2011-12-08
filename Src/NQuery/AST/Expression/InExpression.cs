using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class InExpression : ExpressionNode
	{
		private ExpressionNode _left;
		private ExpressionNode[] _rightExpressions;

		public ExpressionNode Left
		{
			get { return _left; }
			set { _left = value; }
		}

		public ExpressionNode[] RightExpressions
		{
			get { return _rightExpressions; }
			set { _rightExpressions = value; }
		}

		public override object GetValue()
		{
			throw ExceptionBuilder.InternalErrorGetValueNotSupported(GetType());
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.InExpression; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			InExpression result = new InExpression();
			result._left = (ExpressionNode)_left.Clone(alreadyClonedElements);
			result._rightExpressions = new ExpressionNode[_rightExpressions.Length];
			
			for (int i = 0; i < _rightExpressions.Length; i++)
				result._rightExpressions[i] = (ExpressionNode)_rightExpressions[i].Clone(alreadyClonedElements);
			
			return result;
		}
		
		public override Type ExpressionType
		{
			get { return typeof(bool); }
		}
	}
}