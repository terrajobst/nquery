using System;

namespace NQuery.Compilation
{
	internal sealed class Normalizer : StandardVisitor
	{
		public override ExpressionNode VisitUnaryExpression(UnaryExpression expression)
		{
			// First visit arguments
			base.VisitUnaryExpression(expression);
			
			if (expression.Op == UnaryOperator.LogicalNot)
			{				
				// Replace "NOT NOT expr" by "expr"

				UnaryExpression unOp = expression.Operand as UnaryExpression;
				if (unOp != null)
				{
					if (unOp.Op == UnaryOperator.LogicalNot)
						return VisitExpression(unOp.Operand);
				}
				
				// Replace "NOT expr IS NULL" and "NOT expr IS NOT NULL" by
				//         "expr IS NOT NULL" and "expr IS NULL" resp.
				
				IsNullExpression isNull = expression.Operand as IsNullExpression;
				if (isNull != null)
				{
					isNull.Negated = !isNull.Negated;
					return VisitExpression(isNull);
				}
				
				// Apply negation on EXISTS
				
				ExistsSubselect existsSubselect = expression.Operand as ExistsSubselect;
				if (existsSubselect != null)
				{
					existsSubselect.Negated = !existsSubselect.Negated;
					return existsSubselect;
				}

			    // Apply negation on ALL/ANY subquery
			    
                AllAnySubselect allAnySubselect = expression.Operand as AllAnySubselect;
			    if (allAnySubselect != null)
			    {
                    allAnySubselect.Op = AstUtil.NegateBinaryOp(allAnySubselect.Op);
			        allAnySubselect.Type = (allAnySubselect.Type == AllAnySubselect.AllAnyType.All) ? AllAnySubselect.AllAnyType.Any : AllAnySubselect.AllAnyType.All;
                    return allAnySubselect;
			    }
			    
				// Apply De Morgan's law
				
				BinaryExpression binOp = expression.Operand as BinaryExpression;
					
				if (binOp != null)
				{
				    BinaryOperator negatedOp = AstUtil.NegateBinaryOp(binOp.Op);

				    if (negatedOp != null)
					{
						ExpressionNode newLeft;
						ExpressionNode newRight;
						
						if (binOp.Op == BinaryOperator.LogicalAnd || binOp.Op == BinaryOperator.LogicalOr)
						{
							newLeft = new UnaryExpression(expression.Op, binOp.Left);
							newRight = new UnaryExpression(expression.Op, binOp.Right);
						}
						else
						{
							newLeft = binOp.Left;
							newRight = binOp.Right;
						}
							
						binOp.Op = negatedOp;
						binOp.Left = newLeft;
						binOp.Right = newRight;
						return VisitExpression(binOp);
					}
				}					
			}
				
			return expression;
		}

	    public override ExpressionNode VisitBetweenExpression(BetweenExpression expression)
		{
			// First visit all expressions
			base.VisitBetweenExpression(expression);

			// Since a BETWEEN expression can be expressed using an AND expression and two
			// <= binary operator nodes we convert them to simplify the join and optimization engine.

			BinaryExpression lowerBound = new BinaryExpression(BinaryOperator.LessOrEqual, expression.LowerBound, expression.Expression);
			BinaryExpression upperBound = new BinaryExpression(BinaryOperator.LessOrEqual, expression.Expression, expression.UpperBound);
			BinaryExpression and = new BinaryExpression(BinaryOperator.LogicalAnd, lowerBound, upperBound);

			// Make sure we also resolve the result.
			return VisitExpression(and);
		}

		public override ExpressionNode VisitInExpression(InExpression expression)
		{
			// First visit right expressions
			base.VisitInExpression(expression);
			
			// Check if the IN expression should have been expr = ANY (suquery)
			
			if (expression.RightExpressions.Length == 1)
			{
				SingleRowSubselect firstElemenAsSubselect = expression.RightExpressions[0] as SingleRowSubselect;
				
				if (firstElemenAsSubselect != null)
				{
                    AllAnySubselect result = new AllAnySubselect();
                    result.Type = AllAnySubselect.AllAnyType.Any;
                    result.Left = expression.Left;
                    result.Op = BinaryOperator.Equal;
                    result.Query = firstElemenAsSubselect.Query;
					return result;
				}
			}
			
			// IN-expressions can be written as a chain of OR and = expressions:
			//
			// leftExpr IN (rightExpr1 , ... , rightExprN)
			//
			// (leftExpr = rightExpr1) OR (...) OR (leftExpr = rightExprN)

			ExpressionNode resultNode = null;

			foreach (ExpressionNode rightExpression in expression.RightExpressions)
			{
				ExpressionNode clonedLeft = (ExpressionNode) expression.Left.Clone();
				BinaryExpression comparisionNode = new BinaryExpression(BinaryOperator.Equal, clonedLeft, rightExpression);
				
				if (resultNode == null)
					resultNode = comparisionNode;
				else
				{
					BinaryExpression orNode = new BinaryExpression(BinaryOperator.LogicalOr, resultNode, comparisionNode);
					resultNode = orNode;
				}
			}

			// Visit resulting expression.
			return VisitExpression(resultNode);
		}

		public override ExpressionNode VisitCoalesceExpression(CoalesceExpression expression)
		{
			// First visit all expressions
			base.VisitCoalesceExpression(expression);

			// Since a COALESCE expression can be expressed using a CASE expression we convert
			// them to simplify the evaluation and optimization engine.
			//
			// COALESCE(expression1,...,expressionN) is equivalent to this CASE expression:
			//
			// CASE
			//   WHEN (expression1 IS NOT NULL) THEN expression1
			//   ...
			//   WHEN (expressionN IS NOT NULL) THEN expressionN
			// END

			CaseExpression caseExpression = new CaseExpression();
			caseExpression.WhenExpressions = new ExpressionNode[expression.Expressions.Length];
			caseExpression.ThenExpressions = new ExpressionNode[expression.Expressions.Length];
			
			for (int i = 0; i < expression.Expressions.Length; i++)
			{
				ExpressionNode whenPart = expression.Expressions[i];
				ExpressionNode thenPart = (ExpressionNode) whenPart.Clone();
				caseExpression.WhenExpressions[i] = new IsNullExpression(true, whenPart);
				caseExpression.ThenExpressions[i] = thenPart;
			}

			return VisitExpression(caseExpression);
		}
		
		public override ExpressionNode VisitNullIfExpression(NullIfExpression expression)
		{
			// First visit all expressions
			base.VisitNullIfExpression (expression);
			
			// Since a NULLIF expression can be expressed using a CASE expression we convert
			// them to simplify the evaluation and optimization engine.
			//
			// NULLIF(expression1, expression1) is equivalent to this CASE expression:
			//
			// CASE
			//   WHEN expression1 = expression2 THEN NULL
			//	 ELSE expression1 
			// END
			
			CaseExpression caseExpression = new CaseExpression();
			caseExpression.WhenExpressions = new ExpressionNode[1];
			caseExpression.ThenExpressions = new ExpressionNode[1];
			caseExpression.WhenExpressions[0] = new BinaryExpression(BinaryOperator.Equal, expression.LeftExpression, expression.RightExpression);
			caseExpression.ThenExpressions[0] = LiteralExpression.FromNull();
			caseExpression.ElseExpression = expression.LeftExpression;

			return VisitExpression(caseExpression);
		}

		public override ExpressionNode VisitCaseExpression(CaseExpression expression)
		{
			// Replace simple CASE by searched CASE, i.e.
			//
			// replace
			//
			//       CASE InputExpr
			//          WHEN Expr1 THEN ThenExpr1 ...
			//          WHEN ExprN THEN ThenExprN
			//          [ELSE ElseExpr]
			//       END
			//
			// by
			//
			//       CASE
			//          WHEN InputExpr = Expr1 THEN ThenExpr1 ...
			//          WHEN InputExpr = ExprN THEN ThenExprN
			//          [ELSE ElseExpr]
			//       END

			if (expression.InputExpression != null)
			{
				for (int i = 0; i < expression.WhenExpressions.Length; i++)
				{
					BinaryExpression binaryExpression = new BinaryExpression();
					binaryExpression.Op = BinaryOperator.Equal;
					binaryExpression.Left = (ExpressionNode) expression.InputExpression.Clone();
					binaryExpression.Right = expression.WhenExpressions[i];
					expression.WhenExpressions[i] = binaryExpression;
				}

				expression.InputExpression = null;
			}

			return base.VisitCaseExpression(expression);
		}

		public override QueryNode VisitSortedQuery(SortedQuery query)
		{
			// Check if the input is select query. In this case we push the ORDER BY clause into the select query.
			// This is needed because the semantic of ORDER BY is slightly different for SELECT / UNION SELECT queries.

			SelectQuery inputAsSelectQuery = query.Input as SelectQuery;

			if (inputAsSelectQuery != null)
			{
				inputAsSelectQuery.OrderByColumns = query.OrderByColumns;
				return VisitQuery(inputAsSelectQuery);
			} 
			
			return base.VisitSortedQuery(query);
		}
	}
}