using System;
using System.Reflection;

namespace NQuery.Compilation
{
	internal class OperatorTypeResolver : StandardVisitor
	{
		private IErrorReporter _errorReporter;
		private Binder _binder;

		public OperatorTypeResolver(IErrorReporter errorReporter)
		{
			_errorReporter = errorReporter;
			_binder = new Binder(_errorReporter);
		}

		public IErrorReporter ErrorReporter
		{
			get { return _errorReporter; }
		}

		public Binder Binder
		{
			get { return _binder; }
		}

		public override ExpressionNode VisitUnaryExpression(UnaryExpression expression)
		{
			base.VisitUnaryExpression(expression);

			Type operandType = expression.Operand.ExpressionType;

			if (operandType == null)
			{
				expression.OperatorMethod = null;
			}
			else
			{
				expression.OperatorMethod = _binder.BindOperator(expression.Op, operandType);

				if (expression.OperatorMethod == null)
					_errorReporter.CannotApplyOperator(expression.Op, operandType);
			}

			return expression;
		}

		public override ExpressionNode VisitBinaryExpression(BinaryExpression expression)
		{
			base.VisitBinaryExpression(expression);

			Type leftType = expression.Left.ExpressionType;
			Type rightType = expression.Right.ExpressionType;

			if (leftType == null || rightType == null)
			{
				expression.OperatorMethod = null;
			}
			else if (expression.Op == BinaryOperator.LogicalAnd ||expression.Op == BinaryOperator.LogicalOr)
			{
				// If an operand of this expression is of type NULL the whole expression would be of type
				// NULL (because any operator containing NULL will yield NULL). But Boolean conditions
				// are an exception to this rule. This is why there is no operator method in the builtin operators
				// that calculculates OR or AND. Instead, there is custom IL emitted for these operators.
				//
				// This expression will never use the operator method. The binder will return a placeholder.

				if (expression.Left.ExpressionType != typeof(bool) && expression.Left.ExpressionType != typeof(DBNull) ||
					expression.Right.ExpressionType != typeof(bool) && expression.Right.ExpressionType != typeof(DBNull))
				{
					// We only accept Boolean and NULL datatypes as argument to OR and AND.
					_errorReporter.CannotApplyOperator(expression.Op, expression.Left.ExpressionType, expression.Right.ExpressionType);
				}

				expression.OperatorMethod = _binder.BindOperator(expression.Op, typeof(bool), typeof(bool));
			}
			else
			{
				expression.OperatorMethod = _binder.BindOperator(expression.Op, leftType, rightType);

				if (expression.OperatorMethod == null)
				{
					_errorReporter.CannotApplyOperator(expression.Op, leftType, rightType);
				}
				else
				{
					// Convert operands if necessary

					ParameterInfo[] opMethodParams = expression.OperatorMethod.GetParameters();
					expression.Left = _binder.ConvertExpressionIfRequired(expression.Left, opMethodParams[0].ParameterType);
					expression.Right = _binder.ConvertExpressionIfRequired(expression.Right, opMethodParams[1].ParameterType);
				}
			}

			return expression;
		}

		public override ExpressionNode VisitCaseExpression(CaseExpression expression)
		{
			// Ensure all nested expressions are fully resolved.

			base.VisitCaseExpression(expression);

			for (int i = 0; i < expression.WhenExpressions.Length; i++)
			{
				if (expression.WhenExpressions[i].ExpressionType == null || expression.ThenExpressions[i].ExpressionType == null)
					return expression;
			}

			if (expression.ElseExpression != null && expression.ElseExpression.ExpressionType == null)
				return expression;

			// Ok, all nested expressions could be fully resolved. Lets validate the CASE expression.

			// The semantic of CASE says that if no expression incl. ELSE does match the result is NULL.
			// So having an ELSE expression that returns NULL is quite redundant.

			LiteralExpression elseAsLiteral = expression.ElseExpression as LiteralExpression;
			if (elseAsLiteral != null && elseAsLiteral.IsNullValue)
				expression.ElseExpression = null;

			// All WHEN expressions must evaluate to bool.

			foreach (ExpressionNode whenExpression in expression.WhenExpressions)
			{
				if (whenExpression.ExpressionType != typeof(bool) && whenExpression.ExpressionType != typeof(DBNull))
					_errorReporter.WhenMustEvaluateToBoolIfCaseInputIsOmitted(whenExpression);
			}

			// Check that all result expression incl. else share a common type.
			//
			// To do this and to support good error reporting we first try to find
			// the best common type. Any needed conversions or type errors are
			// ignored.

			Type commonResultType = expression.ThenExpressions[0].ExpressionType;

			for (int i = 1; i < expression.ThenExpressions.Length; i++)
			{
				commonResultType = _binder.ChooseBetterTypeConversion(commonResultType, expression.ThenExpressions[i].ExpressionType);
			}

			if (expression.ElseExpression != null)
				commonResultType = _binder.ChooseBetterTypeConversion(expression.ElseExpression.ExpressionType, commonResultType);

			// Now we know that commonResultType is the best type for all result expressions.
			// Insert cast nodes for all expressions that have a different type but are
			// implicit convertible and report errors for all expressions that not convertible.

			if (expression.ElseExpression != null)
			{
				expression.ElseExpression = _binder.ConvertExpressionIfRequired(expression.ElseExpression, commonResultType);
			}

			for (int i = 0; i < expression.ThenExpressions.Length; i++)
			{
				expression.ThenExpressions[i] = _binder.ConvertExpressionIfRequired(expression.ThenExpressions[i], commonResultType);
			}

			expression.ResultType = commonResultType;

			return expression;
		}
	}
}