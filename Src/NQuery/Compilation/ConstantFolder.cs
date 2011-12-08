using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class ConstantFolder : StandardVisitor
	{
		private IErrorReporter _errorReporter;

		public ConstantFolder(IErrorReporter errorReporter) 
		{
			_errorReporter = errorReporter;
		}

	    public override ExpressionNode VisitUnaryExpression(UnaryExpression expression)
		{
			base.VisitUnaryExpression (expression);

			ConstantExpression operandAsConstant = expression.Operand as ConstantExpression;

			if (operandAsConstant != null)
			{
				// Ok, lets compute the result

				if (operandAsConstant.IsNullValue)
					return LiteralExpression.FromTypedNull(expression.ExpressionType);
				
				try
				{
					return LiteralExpression.FromTypedValue(expression.GetValue(), expression.ExpressionType);
				}
				catch (RuntimeException ex)
				{
					_errorReporter.CannotFoldConstants(ex);
				}
			}

			// If we getting here we return the orginal one.

			return expression;
		}

		public override ExpressionNode VisitBinaryExpression(BinaryExpression expression)
		{
			base.VisitBinaryExpression(expression);

			ConstantExpression leftConstant = expression.Left as ConstantExpression;
			ConstantExpression rightConstant = expression.Right as ConstantExpression;

			if (leftConstant != null && rightConstant != null)
			{
				// Both operands are constants, compute the result and return a constant node.

				try
				{
                    return LiteralExpression.FromTypedValue(expression.GetValue(), expression.ExpressionType);
				}
				catch (RuntimeException ex)
				{
					_errorReporter.CannotFoldConstants(ex);
				}
			}
			else if ((leftConstant != null || rightConstant != null) && (expression.Op == BinaryOperator.LogicalAnd || expression.Op == BinaryOperator.LogicalOr))
			{
				// We have a boolean AND or OR expression where one operand is a constant. Check if we
				// already know the result.

				if (expression.Op == BinaryOperator.LogicalAnd)
				{
					// Check if one operand is false

					if (leftConstant != null && !leftConstant.IsNullValue && !leftConstant.AsBoolean ||
						rightConstant != null && !rightConstant.IsNullValue && !rightConstant.AsBoolean)
						return LiteralExpression.FromBoolean(false);
				}
				else
				{
					// Check if one operand is true

					if (leftConstant != null && !leftConstant.IsNullValue && leftConstant.AsBoolean ||
						rightConstant != null && !rightConstant.IsNullValue && rightConstant.AsBoolean)
						return LiteralExpression.FromBoolean(true);
				}

				// We don't know the result but we can throw away the and/or expression
				// by replacing it with the unknown part.

				if (leftConstant != null && !leftConstant.IsNullValue)
					return expression.Right;
				else if (rightConstant != null && !rightConstant.IsNullValue)
					return expression.Left;

				return expression;
			}

			// If we getting here we return the orginal one.

			return expression;
		}

		public override ExpressionNode VisitIsNullExpression(IsNullExpression expression)
		{
			base.VisitIsNullExpression(expression);

			ConstantExpression constantExpression = expression.Expression as ConstantExpression;

			if (constantExpression != null)
			{
				if (expression.Negated)
					return LiteralExpression.FromBoolean(constantExpression.GetValue() != null);
				else
					return LiteralExpression.FromBoolean(constantExpression.GetValue() == null);
			}

			return expression;
		}

		public override ExpressionNode VisitCastExpression(CastExpression expression)
		{
			base.VisitCastExpression(expression);

			if (!Binder.ConversionNeeded(expression.Expression.ExpressionType, expression.ExpressionType))
				return expression.Expression;

			ConstantExpression expressionAsConstant = expression.Expression as ConstantExpression;

			if (expressionAsConstant != null)
			{
				if (expressionAsConstant.IsNullValue)
                    return LiteralExpression.FromTypedNull(expression.ExpressionType);

				try
				{
                    return LiteralExpression.FromTypedValue(expression.GetValue(), expression.ExpressionType);
				}
				catch (RuntimeException ex)
				{
					_errorReporter.CannotFoldConstants(ex);
				}
			}

			return expression;
		}

		public override ExpressionNode VisitCaseExpression(CaseExpression expression)
		{
			base.VisitCaseExpression(expression);

			// NOTE: It must be a searched CASE. The normalizer should have normalized it already.
			//
			// Remove all WHEN expressions that are allays FALSE or NULL.
			// AND
			// Cut off all WHENs trailing an expression that is always true.

			List<ExpressionNode> whenExpressions = new List<ExpressionNode>();
			List<ExpressionNode> thenExpressions = new List<ExpressionNode>();

			for (int i = 0; i < expression.WhenExpressions.Length; i++)
			{
				if (AstUtil.IsNull(expression.WhenExpressions[i]))
				{
					// A WHEN expression is always null.
					continue;
				}

				ConstantExpression whenAsBooleanConstant = expression.WhenExpressions[i] as ConstantExpression;

				if (whenAsBooleanConstant != null)
				{
					if (!whenAsBooleanConstant.AsBoolean)
					{
						// A WHEN expression is always false.
						//
						// We remove this part from the case expression by not adding to 
						// whenExpressions and thenExpressions.
						continue;
					}
					else
					{
						// A WHEN expression is always true.							
						//
						// We replace the ELSE expression by the THEN expression and
						// cut off the rest.

						expression.ElseExpression = expression.ThenExpressions[i];
						break;
					}
				}

				whenExpressions.Add(expression.WhenExpressions[i]);
				thenExpressions.Add(expression.ThenExpressions[i]);
			}

			if (whenExpressions.Count == 0)
			{
				// This means the first WHEN expression was always false
				// or all WHEN expressions are either FALSE or NULL.
				//
				// We replace the case expression by the else expression
				// or by NULL.

				if (expression.ElseExpression != null)
					return expression.ElseExpression;

				return LiteralExpression.FromTypedNull(expression.ExpressionType);
			}

			expression.WhenExpressions = whenExpressions.ToArray();
			expression.ThenExpressions = thenExpressions.ToArray();

			return expression;
		}

		public override ExpressionNode VisitNamedConstantExpression(NamedConstantExpression expression)
		{
			// From now on there are no named constants in the tree. A constant value is always indicated
			// by a literal. This simplifies later phases (e.g. IL generation).
			return LiteralExpression.FromTypedValue(expression.Constant.Value, expression.ExpressionType);
		}

		public override ExpressionNode VisitPropertyAccessExpression(PropertyAccessExpression expression)
		{
			base.VisitPropertyAccessExpression(expression);

			if (expression.Target is ConstantExpression)
			{
				try
				{
                    return LiteralExpression.FromTypedValue(expression.GetValue(), expression.ExpressionType);
				}
				catch (RuntimeException ex)
				{
					_errorReporter.CannotFoldConstants(ex);
				}
			}

			return expression;
		}

		public override ExpressionNode VisitFunctionInvocationExpression(FunctionInvocationExpression expression)
		{
			base.VisitFunctionInvocationExpression(expression);

			// Constant folding must not be applied if the function is non-deterministic.
			if (!expression.Function.IsDeterministic)
				return expression;
			
			// Check if all arguments are constants or at least one argument
			// is null.

			ConstantExpression[] constantArguments = new ConstantExpression[expression.Arguments.Length];
			bool allArgumentsAreConstants = true;

			for (int i = 0; i < constantArguments.Length; i++)
			{
				constantArguments[i] = expression.Arguments[i] as ConstantExpression;

				if (constantArguments[i] == null)
				{
					// Ok, one argument is not a constant
					// But don't stop: If an argument is null we can still calculate the result!
					allArgumentsAreConstants = false;
				}
				else if (constantArguments[i].IsNullValue)
				{
					// We found a null. That means the invocation will also yield null.
                    return LiteralExpression.FromTypedNull(expression.ExpressionType);
				}
			}

			if (allArgumentsAreConstants)
			{
				try
				{
                    return LiteralExpression.FromTypedValue(expression.GetValue(), expression.ExpressionType);
				}
				catch (RuntimeException ex)
				{
					_errorReporter.CannotFoldConstants(ex);
				}
			}

			return expression;
		}
		
		public override ExpressionNode VisitMethodInvocationExpression(MethodInvocationExpression expression)
		{
			base.VisitMethodInvocationExpression(expression);

			// Constant folding must not be applied if the method is non-deterministic.
			if (!expression.Method.IsDeterministic)
				return expression;
			
			// Check if target is a constant or even null.
			
			ConstantExpression targetAsConstant = expression.Target as ConstantExpression;
			
			if (targetAsConstant == null)
				return expression;
			
			if (targetAsConstant.IsNullValue)
                return LiteralExpression.FromTypedNull(expression.ExpressionType);
			
			// Check if all arguments are constants or at least one argument
			// is null.

			ConstantExpression[] constantArguments = new ConstantExpression[expression.Arguments.Length];
			bool allArgumentsAreConstants = true;

			for (int i = 0; i < constantArguments.Length; i++)
			{
				constantArguments[i] = expression.Arguments[i] as ConstantExpression;

				if (constantArguments[i] == null)
				{
					// Ok, one argument is not a constant
					// But don't stop: If an argument is null we can still calculate the result!
					allArgumentsAreConstants = false;
				}
				else if (constantArguments[i].IsNullValue)
				{
					// We found a null. That means the invocation will also yield null.
                    return LiteralExpression.FromTypedNull(expression.ExpressionType);
				}
			}

			if (allArgumentsAreConstants)
			{
				try
				{
                    return LiteralExpression.FromTypedValue(expression.GetValue(), expression.ExpressionType);
				}
				catch (RuntimeException ex)
				{
					_errorReporter.CannotFoldConstants(ex);
				}
			}

			return expression;
		}
		
		public override QueryNode VisitSelectQuery(SelectQuery query)
		{
			base.VisitSelectQuery(query);

            if (query.OrderByColumns != null)
            {
                foreach (OrderByColumn orderByColumn in query.OrderByColumns)
                {
                    if (orderByColumn.Expression is ConstantExpression)
                        _errorReporter.ConstantExpressionInOrderBy();
                }
            }
		    			
			return query;
		}
	}
}