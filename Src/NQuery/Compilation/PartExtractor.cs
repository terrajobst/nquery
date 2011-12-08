using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class PartExtractor : StandardVisitor
	{
		private BinaryOperator _operatorToBreakAt;
		private List<ExpressionNode> _partList = new List<ExpressionNode>();

		public PartExtractor(LogicalOperator logicalOperator)
		{
            if (logicalOperator == LogicalOperator.And)
                _operatorToBreakAt = BinaryOperator.LogicalAnd;
            else
                _operatorToBreakAt = BinaryOperator.LogicalOr;
		}
		
		public override AstNode Visit(AstNode node)
		{
            ExpressionNode expr = node as ExpressionNode;

            if (expr != null)
            {
                BinaryExpression binaryExpression = expr as BinaryExpression;

                if (binaryExpression != null && binaryExpression.Op == _operatorToBreakAt)
                    return base.Visit(expr);

                _partList.Add(expr);
            }
		    
			return node;
		}

		/* The problem with the code below is that only binary expression children are 
		 * inserted into _partList.
		 * 
		public override ExpressionNode VisitBinaryExpression(BinaryExpression expression)
		{
			if (expression.Op == BinaryOperator.LogicalAnd)
				return base.VisitBinaryExpression (expression);

			_partList.Add(expression);
			return expression;
		}
		*/
	
		public ExpressionNode[] GetParts()
		{
			return _partList.ToArray();
		}
	}
}