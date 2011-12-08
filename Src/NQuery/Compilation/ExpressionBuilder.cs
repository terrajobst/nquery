using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
    internal sealed class ExpressionBuilder
    {
        private Stack<ExpressionNode> _expressionStack = new Stack<ExpressionNode>();

        public int Count
        {
            get { return _expressionStack.Count;  }
        }
        
        public void Push(ExpressionNode expression)
        {
            _expressionStack.Push(expression);
        }

        public void PushUnary(UnaryOperator unaryOperator)
        {
            ExpressionNode operand = _expressionStack.Pop();
            UnaryExpression unaryExpression = new UnaryExpression(unaryOperator, operand);
            Push(unaryExpression);
        }

        public void PushBinary(BinaryOperator binaryOperator)
        {
            ExpressionNode rightExpressionNode = _expressionStack.Pop();
            ExpressionNode leftExpressionNode = _expressionStack.Pop();
            BinaryExpression binaryExpression = new BinaryExpression(binaryOperator, leftExpressionNode, rightExpressionNode);
            _expressionStack.Push(binaryExpression);
        }
        
        public void PushIsNull()
        {
            ExpressionNode operand = _expressionStack.Pop();
            IsNullExpression isNullExpression = new IsNullExpression(false, operand);
            _expressionStack.Push(isNullExpression);
        }

        public void PushNAry(LogicalOperator logicalOperator)
        {
            BinaryOperator binaryOperator;
            if (logicalOperator == LogicalOperator.And)
                binaryOperator = BinaryOperator.LogicalAnd;
            else
                binaryOperator = BinaryOperator.LogicalOr;
            
            List<ExpressionNode> arguments = new List<ExpressionNode>();
            while (_expressionStack.Count > 0)
                arguments.Add(_expressionStack.Pop());

            foreach (ExpressionNode argument in arguments)
            {
                Push(argument);
                if (Count > 1)
                    PushBinary(binaryOperator);
            }
        }
        
        public ExpressionNode Pop()
        {
            ExpressionNode result = _expressionStack.Pop();
            result = new Normalizer().VisitExpression(result);
			result = new OperatorTypeResolver(ExceptionErrorProvider.Instance).VisitExpression(result);
			result = new ConstantFolder(ExceptionErrorProvider.Instance).VisitExpression(result);
			return result;
        }
    }
}