using System;
using System.Collections.Generic;
using System.Reflection;

namespace NQuery.Compilation
{
	internal sealed class UnaryExpression : OperatorExpression
	{
		private UnaryOperator _op;
		private ExpressionNode _operand;

		public UnaryExpression(UnaryOperator op, ExpressionNode operand)
		{
			_op = op;
			_operand = operand;
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.UnaryExpression; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			UnaryExpression result = new UnaryExpression(_op, (ExpressionNode)_operand.Clone(alreadyClonedElements));
			result.OperatorMethod = OperatorMethod;
			return result;
		}

		public override object GetValue()
		{
			object value = _operand.GetValue();

			if (value == null || OperatorMethod == null)
				return null;

			try
			{
				return OperatorMethod.Invoke(null, new object[] {value});
			}
			catch (TargetInvocationException ex)
			{
				throw ExceptionBuilder.UnaryOperatorFailed(Op, OperatorMethod, _operand.ExpressionType, value, ex.InnerException);
			}
		}

		public new UnaryOperator Op
		{
			get { return _op; }
		}

		public ExpressionNode Operand
		{
			get { return _operand; }
			set { _operand = value; }
		}

		protected override Operator InternalGetOp()
		{
			return _op;
		}
	}
}