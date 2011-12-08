using System;
using System.Globalization;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class SourceGenerator : StandardVisitor
	{
		private SqlTextWriter _writer;

		private const string SELECT     = "   SELECT";
		private const string FROM       = "     FROM";
		private const string WHERE      = "    WHERE";
		private const string GROUPBY    = " GROUP BY";
		private const string HAVING     = "   HAVING";
		private const string ORDERBY    = " ORDER BY";
		private const string UNION      = "    UNION";
		private const string UNIONALL   = "UNION ALL";
		private const string EXCEPT     = "   EXCEPT";
		private const string INTERSECT  = "INTERSECT";

		private const int SELECT_INDENT = 11;
		private const int QUERY_INDENT  = 2;

		public SourceGenerator()
		{
			_writer = new SqlTextWriter();
		}

		#region Helpers

		private static bool NeedParentheses(ExpressionNode childExpression, int parentPrecedence, bool isRightOperand)
		{
			// If childExpression is also an operator expression we may need parentheses.
			//
			// This depends on the precedence and associativity of the child operator.
			// Since we have unary, binary and ternary operators we extract theses pieces
			// of information using casts.

			int childPrecedence;
			bool isRightAssociative;

			OperatorExpression expressionOperatorExpression = childExpression as OperatorExpression;

			if (expressionOperatorExpression != null)
			{
				// It is a binary or unary operator.

				childPrecedence = expressionOperatorExpression.Op.Precedence;
				isRightAssociative = expressionOperatorExpression.Op.IsRightAssociative;
			}
			else
			{
				// Special handling for the only ternary operator.

				BetweenExpression expressionAsBetweenExpression = childExpression as BetweenExpression;

				if (expressionAsBetweenExpression != null)
				{
					childPrecedence = Operator.BETWEEN_PRECEDENCE;
					isRightAssociative = false;
				}
				else
				{
					return false;
				}
			}

			// Analyze precedences.

			if (childPrecedence < parentPrecedence)
				return true;

			if (childPrecedence == parentPrecedence)
			{
				if (isRightOperand && !isRightAssociative)
					return true;

				if (!isRightOperand && isRightAssociative)
					return true;
			}

			return false;
		}

		#endregion

		#region Expressions

		#region Compound Expressions

		public override ExpressionNode VisitUnaryExpression(UnaryExpression expression)
		{
			_writer.Write(expression.Op.TokenText);
			_writer.Write(" ");

			bool parenthesesNeeded = NeedParentheses(expression.Operand, expression.Op.Precedence, false);

			if (parenthesesNeeded)
				_writer.Write("(");

			Visit(expression.Operand);

			if (parenthesesNeeded)
				_writer.Write(")");

			return expression;
		}

		public override ExpressionNode VisitBinaryExpression(BinaryExpression expression)
		{
			// Emit left operand

			bool parenthesesNedded = NeedParentheses(expression.Left, expression.Op.Precedence, false);

			if (parenthesesNedded)
				_writer.Write("(");

			Visit(expression.Left);

			if (parenthesesNedded)
				_writer.Write(")");

			// Emit operator

			_writer.Write(" ");
			_writer.Write(expression.Op.TokenText);
			_writer.Write(" ");

			// Emit right operand

			parenthesesNedded = NeedParentheses(expression.Right, expression.Op.Precedence, true);

			if (parenthesesNedded)
				_writer.Write("(");

			Visit(expression.Right);

			if (parenthesesNedded)
				_writer.Write(")");

			return expression;
		}

		public override ExpressionNode VisitBetweenExpression(BetweenExpression expression)
		{
			Visit(expression.Expression);
			_writer.Write(" BETWEEN ");

			// Emit lower bound

			bool parenthesesNedded = expression.LowerBound is BinaryExpression;

			if (parenthesesNedded)
				_writer.Write("(");

			Visit(expression.LowerBound);

			if (parenthesesNedded)
				_writer.Write(")");

			// Emit and

			_writer.Write(" AND ");

			// Emit upper bound

			parenthesesNedded = expression.UpperBound is BinaryExpression;

			if (parenthesesNedded)
				_writer.Write("(");

			Visit(expression.UpperBound);

			if (parenthesesNedded)
				_writer.Write(")");

			return expression;
		}

		public override ExpressionNode VisitCastExpression(CastExpression expression)
		{
			_writer.Write("CAST(");
			Visit(expression.Expression);
			_writer.Write(" AS ");

			bool shortHandPrinted = false;

			if (expression.TypeReference.ResolvedType != null)
			{
				PrimitiveType pt = Binder.GetPrimitiveType(expression.TypeReference.ResolvedType);

				if (pt != PrimitiveType.None && pt != PrimitiveType.Null && pt != PrimitiveType.Object)
				{
					_writer.Write(pt.ToString().ToUpper(CultureInfo.InvariantCulture));
					shortHandPrinted = true;
				}
			}

			if (!shortHandPrinted)
			{
				if (expression.TypeReference.CaseSensitve)
					_writer.Write("'");

				_writer.Write(expression.TypeReference.TypeName);

				if (expression.TypeReference.CaseSensitve)
					_writer.Write("'");
			}

			_writer.Write(")");

			return expression;
		}

		public override ExpressionNode VisitCaseExpression(CaseExpression expression)
		{
			if (expression.InputExpression == null)
			{
				_writer.Write("CASE");
			}
			else
			{
				_writer.Write("CASE ");
				Visit(expression.InputExpression);
			}

			for (int i = 0; i < expression.WhenExpressions.Length && i < expression.ThenExpressions.Length; i++)
			{
				_writer.Write(" WHEN ");
				Visit(expression.WhenExpressions[i]);
				_writer.Write(" THEN ");
				Visit(expression.ThenExpressions[i]);
			}

			if (expression.ElseExpression != null)
			{
				_writer.Write(" ELSE ");
				Visit(expression.ElseExpression);
			}

			_writer.Write(" END");

			return expression;
		}

		public override ExpressionNode VisitIsNullExpression(IsNullExpression expression)
		{
			Visit(expression.Expression);

			_writer.Write(" IS ");
			if (expression.Negated)
				_writer.Write("NOT ");
			_writer.Write("NULL");
			return expression;
		}

		public override ExpressionNode VisitInExpression(InExpression expression)
		{
			Visit(expression.Left);
			_writer.Write(" IN (");

			for (int i = 0; i < expression.RightExpressions.Length; i++)
			{
				if (i > 0)
					_writer.Write(", ");

				Visit(expression.RightExpressions[i]);
			}

			_writer.Write(")");

			return expression;
		}

		#endregion

		#region Referencing Expression

		public override ExpressionNode VisitNamedConstantExpression(NamedConstantExpression expression)
		{
			Identifier identifier = Identifier.CreateVerbatim(expression.Constant.Name);
			_writer.WriteIdentifier(identifier);
			return expression;
		}

		public override ExpressionNode VisitParameterExpression(ParameterExpression expression)
		{
			_writer.Write("@");
			_writer.WriteIdentifier(expression.Name);
			return expression;
		}

		public override ExpressionNode VisitNameExpression(NameExpression expression)
		{
			_writer.WriteIdentifier(expression.Name);
			return expression;
		}

		public override ExpressionNode VisitPropertyAccessExpression(PropertyAccessExpression expression)
		{
			VisitExpression(expression.Target);
			_writer.Write(".");
			_writer.WriteIdentifier(expression.Name);
			return expression;
		}

		public override ExpressionNode VisitFunctionInvocationExpression(FunctionInvocationExpression expression)
		{
			_writer.WriteIdentifier(expression.Name);

			if (expression.HasAsteriskModifier)
			{
				_writer.Write("(*)");
			}
			else
			{
				_writer.Write("(");

				for (int i = 0; i < expression.Arguments.Length; i++)
				{
					if (i > 0)
						_writer.Write(",");

					Visit(expression.Arguments[i]);
				}

				_writer.Write(")");
			}

			return expression;
		}

		public override ExpressionNode VisitMethodInvocationExpression(MethodInvocationExpression expression)
		{
			bool parenthesesNeeded =
				expression.Target is OperatorExpression ||
				expression.Target is CaseExpression ||
				expression.Target is IsNullExpression;

			if (parenthesesNeeded)
				_writer.Write("(");

			Visit(expression.Target);

			if (parenthesesNeeded)
				_writer.Write(")");

			_writer.Write(".");
			_writer.WriteIdentifier(expression.Name);
			_writer.Write("(");

			for (int i = 0; i < expression.Arguments.Length; i++)
			{
				if (i > 0)
					_writer.Write(",");

				Visit(expression.Arguments[i]);
			}

			_writer.Write(")");

			return expression;
		}


		#endregion

		#region Query

		public override ExpressionNode VisitColumnExpression(ColumnExpression expression)
		{
			if (expression.Column.ColumnBinding is RowColumnBinding)
			{
				_writer.WriteIdentifier(expression.Column.TableRefBinding.Name);
			}
			else
			{
				_writer.WriteIdentifier(expression.Column.TableRefBinding.Name);
				_writer.Write(".");
				_writer.WriteIdentifier(expression.Column.Name);
			}

			return expression;
		}

		public override ExpressionNode VisitRowBufferEntryExpression(RowBufferEntryExpression expression)
		{
			_writer.Write(expression.RowBufferEntry.Name);
			return base.VisitRowBufferEntryExpression(expression);
		}

		public override ExpressionNode VisitAggregagateExpression(AggregateExpression expression)
		{
			_writer.WriteIdentifier(expression.Aggregate.Name);

			if (expression.HasAsteriskModifier)
			{
				_writer.Write("(*)");
			}
			else
			{
				_writer.Write("(");
				Visit(expression.Argument);
				_writer.Write(")");
			}

			return expression;
		}

		public override ExpressionNode VisitSingleRowSubselect(SingleRowSubselect expression)
		{
			_writer.Write("(");
			Visit(expression.Query);
			_writer.Write(")");

			return expression;
		}

		public override ExpressionNode VisitExistsSubselect(ExistsSubselect expression)
		{
			_writer.Write("EXISTS (");
			Visit(expression.Query);
			_writer.Write(")");

			return expression;
		}

		public override ExpressionNode VisitAllAnySubselect(AllAnySubselect expression)
		{
			Visit(expression.Left);

			_writer.Write(" ");
			_writer.Write(expression.Op.TokenText);
			_writer.Write(" ");

			if (expression.Type == AllAnySubselect.AllAnyType.All)
				_writer.Write("ALL (");
			else
				_writer.Write("ANY (");

			Visit(expression.Query);
			_writer.Write(")");

			return expression;
		}

		#endregion

		#endregion

		#region Query

		public override TableReference VisitNamedTableReference(NamedTableReference node)
		{
			_writer.WriteIdentifier(node.TableName);

			if (node.CorrelationName != null)
			{
				_writer.Write(" AS ");
				_writer.WriteIdentifier(node.CorrelationName);
			}

			return node;
		}

		public override TableReference VisitJoinedTableReference(JoinedTableReference node)
		{
			if (node.Left is NamedTableReference)
			{
				Visit(node.Left);
			}
			else
			{
				_writer.Write("(");
				Visit(node.Left);
				_writer.Write(")");
			}

			_writer.Write(" ");

			switch (node.JoinType)
			{
				case JoinType.Inner:
					if (node.Condition == null)
						_writer.Write("CROSS JOIN");
					else
						_writer.Write("INNER JOIN");
					break;
				case JoinType.LeftOuter:
					_writer.Write("LEFT OUTER JOIN");
					break;
				case JoinType.RightOuter:
					_writer.Write("RIGHT OUTER JOIN");
					break;
				case JoinType.FullOuter:
					_writer.Write("FULL OUTER JOIN");
					break;
			}

			_writer.Write(" ");

			if (node.Right is NamedTableReference)
			{
				Visit(node.Right);
			}
			else
			{
				_writer.Write("(");
				Visit(node.Right);
				_writer.Write(")");
			}

			if (node.Condition != null)
			{
				_writer.Write(" ON (");
				Visit(node.Condition);
				_writer.Write(")");
			}

			return node;
		}

		public override TableReference VisitDerivedTableReference(DerivedTableReference node)
		{
			_writer.Write("(");
			_writer.WriteLine();
			_writer.Indent += 2;
			Visit(node.Query);
			_writer.Indent -= 2;
			_writer.WriteLine();
			_writer.Write(") AS ");
			_writer.Write(node.CorrelationName.ToString());

			return node;
		}

		public override QueryNode VisitSelectQuery(SelectQuery query)
		{
			_writer.Write(SELECT);
			_writer.Indent += SELECT_INDENT;

			if (!query.IsDistinct && query.TopClause == null)
			{
				_writer.Write(" ");
			}
			else
			{
				// DISTINCT

				if (query.IsDistinct)
					_writer.Write(" DISTINCT");

				// TOP

				if (query.TopClause != null)
				{
					_writer.Write(" TOP ");
					_writer.WriteLiteral(query.TopClause.Value);
				}

				_writer.WriteLine();
			}

			// Columns

			bool isFirst = true;
			foreach (SelectColumn columnSource in query.SelectColumns)
			{
				if (isFirst)
				{
					isFirst = false;
				}
				else
				{
					_writer.Write(",");
					_writer.WriteLine();
				}

				if (columnSource.IsAsterisk)
				{
					if (columnSource.Alias != null)
					{
						_writer.Write(columnSource.Alias.ToString());
						_writer.Write(".");
					}

					_writer.Write("*");
				}
				else
				{
					Visit(columnSource.Expression);

					if (columnSource.Alias != null)
					{
						_writer.Write(" AS ");
						_writer.Write(columnSource.Alias.ToString());
					}
				}
			}

			_writer.Indent -= SELECT_INDENT;

			// FROM

			if (query.TableReferences != null)
			{
				_writer.WriteLine();
				_writer.Write(FROM);
				_writer.Write(" ");
				_writer.Indent += SELECT_INDENT;
				Visit(query.TableReferences);
				_writer.Indent -= SELECT_INDENT;
			}

			// WHERE

			if (query.WhereClause != null)
			{
				_writer.WriteLine();
				_writer.Write(WHERE);
				_writer.Write(" ");
				_writer.Indent += SELECT_INDENT;
				Visit(query.WhereClause);
				_writer.Indent -= SELECT_INDENT;
			}

			// GROUP BY

			if (query.GroupByColumns != null)
			{
				_writer.WriteLine();
				_writer.Write(GROUPBY);
				_writer.Write(" ");
				_writer.Indent += SELECT_INDENT;

				isFirst = true;
				foreach (ExpressionNode groupyByExpression in query.GroupByColumns)
				{
					if (isFirst)
					{
						isFirst = false;
					}
					else
					{
						_writer.Write(",");
						_writer.WriteLine();
					}

					Visit(groupyByExpression);
				}

				_writer.Indent -= SELECT_INDENT;
			}

			// HAVING

			if (query.HavingClause != null)
			{
				_writer.WriteLine();
				_writer.Write(HAVING);
				_writer.Write(" ");
				_writer.Indent += SELECT_INDENT;
				Visit(query.HavingClause);
				_writer.Indent -= SELECT_INDENT;
			}

			return query;
		}

		public override QueryNode VisitBinaryQuery(BinaryQuery query)
		{
			bool leftNeedsParentheses = query.Left is BinaryQuery;
			bool rightNeedsParentheses = query.Right is BinaryQuery;

			if (leftNeedsParentheses)
			{
				_writer.Write("(");
				_writer.Indent += QUERY_INDENT;
				_writer.WriteLine();
			}

			Visit(query.Left);

			if (leftNeedsParentheses)
			{
				_writer.Indent -= QUERY_INDENT;
				_writer.WriteLine();
				_writer.Write(")");
			}

			_writer.WriteLine();

			switch (query.Op)
			{
				case BinaryQueryOperator.Union:
					_writer.Write(UNION);
					break;
				case BinaryQueryOperator.UnionAll:
					_writer.Write(UNIONALL);
					break;
				case BinaryQueryOperator.Intersect:
					_writer.Write(INTERSECT);
					break;
				case BinaryQueryOperator.Except:
					_writer.Write(EXCEPT);
					break;

				default:
					ExceptionBuilder.UnhandledCaseLabel(query.Op);
					break;
			}

			_writer.WriteLine();

			if (rightNeedsParentheses)
			{
				_writer.Write("(");
				_writer.Indent += QUERY_INDENT;
				_writer.WriteLine();
			}

			Visit(query.Right);

			if (rightNeedsParentheses)
			{
				_writer.Indent -= QUERY_INDENT;
				_writer.WriteLine();
				_writer.Write(")");
			}

			return query;
		}

		public override QueryNode VisitSortedQuery(SortedQuery query)
		{
			Visit(query.Input);

			_writer.WriteLine();
			_writer.Write(ORDERBY);
			_writer.Write(" ");
			_writer.Indent += SELECT_INDENT;

			bool isFirst = true;
			foreach (OrderByColumn orderByColumn in query.OrderByColumns)
			{
				if (isFirst)
				{
					isFirst = false;
				}
				else
				{
					_writer.Write(",");
					_writer.WriteLine();
				}

				Visit(orderByColumn.Expression);

				if (orderByColumn.SortOrder == SortOrder.Descending)
					_writer.Write(" DESC");
			}

			_writer.Indent -= SELECT_INDENT;

			return query;
		}

		public override QueryNode VisitCommonTableExpressionQuery(CommonTableExpressionQuery query)
		{
			_writer.Write("WITH ");

			bool isFirst = true;					
			foreach (CommonTableExpression commonTableExpression in query.CommonTableExpressions)
			{
				if (isFirst)
				{
					isFirst = false;
				}
				else
				{
					_writer.Write(",");
					_writer.WriteLine();
				}

				_writer.Write(commonTableExpression.TableName.ToString());
				_writer.Write(" AS (");
				_writer.Indent += QUERY_INDENT;
				_writer.WriteLine();
				Visit(commonTableExpression.QueryDeclaration);
				_writer.Indent -= QUERY_INDENT;
				_writer.WriteLine();
				_writer.Write(")");
			}

			_writer.WriteLine();
			Visit(query.Input);

			return query;
		}

		#endregion

		public override LiteralExpression VisitLiteralValue(LiteralExpression expression)
		{
			if (expression.GetValue() == null)
				_writer.WriteNull();
			else
			{
				switch (expression.PrimitiveType)
				{
					case PrimitiveType.None:
						if (expression.ExpressionType != typeof(DateTime))
							goto case PrimitiveType.Object;
						_writer.WriteLiteral(expression.AsDateTime);
						break;

					case PrimitiveType.Object:
						_writer.WriteLiteral(expression.GetValue());
						break;

					case PrimitiveType.Boolean:
						_writer.WriteLiteral(expression.AsBoolean);
						break;

					case PrimitiveType.Byte:
					case PrimitiveType.SByte:
					case PrimitiveType.Int16:
					case PrimitiveType.UInt16:
					case PrimitiveType.UInt32:
					case PrimitiveType.UInt64:
						_writer.WriteLiteral(expression.AsInt64, expression.ExpressionType);
						break;

					case PrimitiveType.Int32:
					case PrimitiveType.Int64:
						_writer.WriteLiteral(expression.AsInt64);
						break;

					case PrimitiveType.Char:
						_writer.WriteLiteral(expression.AsString[0]);
						break;

					case PrimitiveType.String:
						_writer.WriteLiteral(expression.AsString);
						break;

					case PrimitiveType.Single:
					case PrimitiveType.Decimal:
						_writer.WriteLiteral(expression.AsDouble, expression.ExpressionType);
						break;

					case PrimitiveType.Double:
						_writer.WriteLiteral(expression.AsDouble);
						break;

					default:
						throw ExceptionBuilder.UnhandledCaseLabel(expression.PrimitiveType);
				}
			}

			return expression;
		}

		public override string ToString()
		{
			return _writer.ToString();
		}
	}
}
