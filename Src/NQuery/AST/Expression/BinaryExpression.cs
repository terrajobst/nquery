using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace NQuery.Compilation
{
	internal sealed class BinaryExpression : OperatorExpression
	{
		private BinaryOperator _op;
		private ExpressionNode _left;
		private ExpressionNode _right;

		public BinaryExpression()
		{
		}

		public BinaryExpression(BinaryOperator op, ExpressionNode left, ExpressionNode right)
		{
			_op = op;
			_left = left;
			_right = right;
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.BinaryExpression; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			BinaryExpression result = new BinaryExpression();
			result.Op = _op;
			result.Left = (ExpressionNode) _left.Clone(alreadyClonedElements);
			result.Right = (ExpressionNode) _right.Clone(alreadyClonedElements);
			result.OperatorMethod = OperatorMethod;
			return result;
		}

		public override object GetValue()
		{
			if (OperatorMethod == null)
				return null;

			if (_op != BinaryOperator.LogicalAnd && _op != BinaryOperator.LogicalOr)
			{
				// Normal evaluation.
				//
				// In this case the whole expression is null when
				// left or right is null.

				object left = _left.GetValue();

				if (left == null)
					return null;

				object right = _right.GetValue();

				if (right == null)
					return null;

				try
				{
					return OperatorMethod.Invoke(null, new object[] {left, right});
				}
				catch (TargetInvocationException ex)
				{
					throw ExceptionBuilder.BinaryOperatorFailed(Op, OperatorMethod, _left.ExpressionType, _right.ExpressionType, left, right, ex.InnerException);
				}
			}
			else
			{
				// Operator is either LogicalAnd or LogicalOr.
				//
				// Special handling for three-state boolean logic and short-circuit 
				// boolean evaluation.
				//
				// ATTENTION: All binary operators will return null when any operand is
				//            is null. Logical operations are different in this point.
				//            Sometimes boolean operators will return TRUE or FALSE though
				//            an operand was null.
				//
				//            See tables for details.
				//
				//    AND | F | T | N        OR | F | T | N
				//    ----+---+---+--        ---+---+---+--
				//    F   | F | F | F        F  | F | T | N
				//    T   | F | T | N        T  | T | T | T
				//    N   | F | N | N        N  | N | T | N

				object left = _left.GetValue();

				if (left != null)
				{
					// Special handling to allow short-circuit boolean evaluation.

					bool leftAsBool = Convert.ToBoolean(left, CultureInfo.InvariantCulture);

					if (_op == BinaryOperator.LogicalAnd && !leftAsBool)
						return false;

					if (_op == BinaryOperator.LogicalOr && leftAsBool)
						return true;
				}

				object right = _right.GetValue();

				if (left == null && right == null)
					return null;

				if (left != null && right != null)
				{
					bool leftAsBool = Convert.ToBoolean(left, CultureInfo.InvariantCulture);
					bool rightAsBool = Convert.ToBoolean(right, CultureInfo.InvariantCulture);

					if (_op == BinaryOperator.LogicalAnd)
						return leftAsBool && rightAsBool;
					else
						return leftAsBool || rightAsBool;
				}
				else if (left != null)
				{
					// left != null && right == null

					bool leftAsBool = Convert.ToBoolean(left, CultureInfo.InvariantCulture);

					if (_op == BinaryOperator.LogicalAnd)
					{
						if (leftAsBool)
							return null;
						else
							return false;
					}
					else
					{
						if (leftAsBool)
							return true;
						else
							return null;
					}
				}
				else
				{
					// left == null && right != null

					bool rightAsBool = Convert.ToBoolean(right, CultureInfo.InvariantCulture);

					if (_op == BinaryOperator.LogicalAnd)
					{
						if (rightAsBool)
							return null;
						else
							return false;
					}
					else
					{
						if (rightAsBool)
							return true;
						else
							return null;
					}
				}
			}
		}

		public new BinaryOperator Op
		{
			get { return _op; }
			set { _op = value; }
		}

		public ExpressionNode Left
		{
			get { return _left; }
			set { _left = value; }
		}

		public ExpressionNode Right
		{
			get { return _right; }
			set { _right = value; }
		}

		protected override Operator InternalGetOp()
		{
			return _op;
		}
	}
}