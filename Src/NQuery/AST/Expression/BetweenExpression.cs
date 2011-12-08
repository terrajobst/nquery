using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class BetweenExpression : ExpressionNode
	{
		private ExpressionNode _expression;
		private ExpressionNode _lowerBound;
		private ExpressionNode _upperBound;

		public BetweenExpression(ExpressionNode expression, ExpressionNode lowerBound, ExpressionNode upperBound) 
		{
			_expression = expression;
			_lowerBound = lowerBound;
			_upperBound = upperBound;
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			return new BetweenExpression(
				(ExpressionNode)_expression.Clone(alreadyClonedElements),
				(ExpressionNode)_lowerBound.Clone(alreadyClonedElements),
				(ExpressionNode)_upperBound.Clone(alreadyClonedElements)
			);
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.BetweenExpression; }
		}

		public ExpressionNode Expression
		{
			get { return _expression; }	
			set { _expression = value; }	
		}

		public ExpressionNode LowerBound
		{
			get { return _lowerBound; }	
			set { _lowerBound = value; }	
		}

		public ExpressionNode UpperBound
		{
			get { return _upperBound; }	
			set { _upperBound = value; }	
		}

		public override Type ExpressionType
		{
			get { return typeof(bool); }
		}

		public override object GetValue()
		{
			// Is replaced by binary operators.

			throw ExceptionBuilder.InternalErrorGetValueNotSupported(GetType());
		}
	}
}