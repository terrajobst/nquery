using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace NQuery.Compilation
{
	internal sealed class Parser
	{
		private IErrorReporter _errorReporter;
		private Lexer _lexer;
		private Token _token;
		private Token _lookahead;
		private SourceRangeRecorder _rangeRecorder = new SourceRangeRecorder();

		public Parser(IErrorReporter errorReporter)
		{
			_errorReporter = errorReporter;
		}

		public ExpressionNode ParseExpression(string source)
		{
			_lexer = new Lexer(_errorReporter, source);
			Reset();
			_rangeRecorder.Begin();

			// Start parsing

			ExpressionNode expression = ParseExpression();

			// We are done so no tokens should remain

			if (expression != null)
				Match(TokenId.Eof);

			return expression;
		}

		public QueryNode ParseQuery(string source)
		{
			_lexer = new Lexer(_errorReporter, source);
			Reset();
			_rangeRecorder.Begin();

			// Start parsing

			QueryNode query = ParseQueryWithOptionalCTE();

			// We are done so no tokens should remain

			if (query != null)
				Match(TokenId.Eof);

			return query;
		}

		private void Reset()
		{
			Reset(0);
		}

		private void Reset(int pos)
		{
			_lexer.Reset(pos);

			// Intialize _token and _lookahead

			_token = _lexer.GetToken();
			_rangeRecorder.RecordEnter(_token);

			if (_token.Id == TokenId.Eof)
				_lookahead = _token;
			else
			{
				_lexer.NextToken();
				_lookahead = _lexer.GetToken();
			}
		}

		private void NextToken()
		{
			_rangeRecorder.RecordLeave(_token);
			_rangeRecorder.RecordEnter(_lookahead);
			_token = _lookahead;

			if (_lookahead.Id != TokenId.Eof)
			{
				_lexer.NextToken();
				_lookahead = _lexer.GetToken();
			}
		}

		private bool Match(TokenId tokenID)
		{
			if (_token.Id == tokenID)
			{
				NextToken();
				return true;
			}

			_errorReporter.TokenExpected(_token.Range, _token.Text, tokenID);
			return false;
		}

		private void EnableQueryKeywords()
		{
			if (!_lexer.IsQuery)
			{
				// Ok, we are going to parse a query. Notify the lexer
				// so that query-only-keywords are now recognized.

				_lexer.IsQuery = true;

				// Retokenize the lookahead (it may be a query-only-keyword which was not recognized
				// previously)

				Reset(_token.Pos);
			}
		}

		private QueryNode ParseQueryWithOptionalCTE()
		{
			EnableQueryKeywords();

			if (_token.Id != TokenId.WITH)
			{
				return ParseQuery();
			}
			else
			{
				NextToken();

				CommonTableExpressionQuery commonTableExpressionQuery = new CommonTableExpressionQuery();
				List<CommonTableExpression> commonTableExpressions = new List<CommonTableExpression>();

				while (_token.Id != TokenId.Eof)
				{
					CommonTableExpression commonTableExpression = ParseCommonTableExpression();
					commonTableExpressions.Add(commonTableExpression);

					if (_token.Id != TokenId.Comma)
						break;

					NextToken();
				}

				commonTableExpressionQuery.CommonTableExpressions = commonTableExpressions.ToArray();
				commonTableExpressionQuery.Input = ParseQuery();
				return commonTableExpressionQuery;
			}
		}

		private CommonTableExpression ParseCommonTableExpression()
		{
			CommonTableExpression commonTableExpression = new CommonTableExpression();
			_rangeRecorder.Begin();
			commonTableExpression.TableName = ParseIdentifier();
			commonTableExpression.TableNameSourceRange = _rangeRecorder.End();

			if (_token.Id == TokenId.LeftParentheses)
			{
				NextToken();

				List<Identifier> columnNames = new List<Identifier>();

				while (_token.Id != TokenId.Eof)
				{
					Identifier columnName = ParseIdentifier();
					if (columnName == null)
						break;

					columnNames.Add(columnName);

					if (_token.Id != TokenId.Comma)
						break;
					NextToken();
				}

				commonTableExpression.ColumnNames = columnNames.ToArray();
				Match(TokenId.RightParentheses);
			}
			Match(TokenId.AS);
			Match(TokenId.LeftParentheses);
			commonTableExpression.QueryDeclaration = ParseQuery();
			Match(TokenId.RightParentheses);

			return commonTableExpression;
		}

		private QueryNode ParseQuery()
		{
			EnableQueryKeywords();

			QueryNode result = ParseUnifiedOrExceptionalQuery();

			// ORDER BY

			if (_token.Id == TokenId.ORDER)
			{
				NextToken();
				Match(TokenId.BY);

				SortedQuery sortedQuery = new SortedQuery();
				sortedQuery.Input = result;
				sortedQuery.OrderByColumns = ParseOrderByColumns();
				result = sortedQuery;
			}

			return result;
		}

		private QueryNode ParseUnifiedOrExceptionalQuery()
		{
			QueryNode leftQuery = ParseIntersectionalQuery();

			if (leftQuery == null)
				return null;

			while (_token.Id == TokenId.UNION || _token.Id == TokenId.EXCEPT)
			{
				BinaryQueryOperator op;

				if (_token.Id == TokenId.EXCEPT)
				{
					op = BinaryQueryOperator.Except;
				}
				else
				{
					if (_lookahead.Id != TokenId.ALL)
					{
						op = BinaryQueryOperator.Union;
					}
					else
					{
						op = BinaryQueryOperator.UnionAll;
						NextToken();
					}
				}

				NextToken();

				QueryNode rightQuery = ParseIntersectionalQuery();

				if (rightQuery == null)
					return null;

				BinaryQuery binaryQuery = new BinaryQuery();
				binaryQuery.Op = op;
				binaryQuery.Left = leftQuery;
				binaryQuery.Right = rightQuery;
				leftQuery = binaryQuery;
			}

			return leftQuery;
		}

		private QueryNode ParseIntersectionalQuery()
		{
			QueryNode leftQuery = ParseSelectQuery();

			if (leftQuery == null)
				return null;

			while (_token.Id == TokenId.INTERSECT)
			{
				NextToken();
				QueryNode rightQuery = ParseSelectQuery();

				if (rightQuery == null)
					return null;

				BinaryQuery binaryQuery = new BinaryQuery();
				binaryQuery.Op = BinaryQueryOperator.Intersect;
				binaryQuery.Left = leftQuery;
				binaryQuery.Right = rightQuery;
				leftQuery = binaryQuery;
			}

			return leftQuery;
		}

		private QueryNode ParseSelectQuery()
		{
			if (_token.Id == TokenId.LeftParentheses)
			{
				NextToken();
				QueryNode result = ParseQuery();
				Match(TokenId.RightParentheses);
				return result;
			}

			if (!Match(TokenId.SELECT))
				return null;

			SelectQuery selectQuery = new SelectQuery();

			// DISTINCT

			if (_token.Id == TokenId.DISTINCT)
			{
				selectQuery.IsDistinct = true;
				NextToken();
			}

			// TOP

			if (_token.Id == TokenId.TOP)
			{
				NextToken();

				TopClause topClause = new TopClause();
				topClause.Value = (int)ParseInteger();

				if (_token.Id == TokenId.WITH)
				{
					NextToken();
					Match(TokenId.TIES);
					topClause.WithTies = true;
				}
				selectQuery.TopClause = topClause;
			}

			// SelectColumns

			selectQuery.SelectColumns = ParseColumnSources();

			// FROM

			if (_token.Id == TokenId.FROM)
			{
				NextToken();
				selectQuery.TableReferences = ParseTableReferences();
			}

			// WHERE

			if (_token.Id == TokenId.WHERE)
			{
				NextToken();
				selectQuery.WhereClause = ParseExpression();
			}

			// GROUP BY

			if (_token.Id == TokenId.GROUP)
			{
				NextToken();
				Match(TokenId.BY);
				selectQuery.GroupByColumns = ParseExpressions();
			}

			// HAVING

			if (_token.Id == TokenId.HAVING)
			{
				NextToken();
				selectQuery.HavingClause = ParseExpression();
			}

			return selectQuery;
		}

		private SelectColumn[] ParseColumnSources()
		{
			List<SelectColumn> columns = new List<SelectColumn>();

			if (_token.Id == TokenId.Eof)
				_errorReporter.SimpleExpressionExpected(_token.Range, _token.Text);

			while (_token.Id != TokenId.Eof)
			{
				columns.Add(ParseColumnSource());

				if (_token.Id != TokenId.Comma)
					break;

				NextToken();
			}

			return columns.ToArray();
		}

		private SelectColumn ParseColumnSource()
		{
			if (_token.Id == TokenId.Multiply)
			{
				// Simple all columns indicator

				NextToken();
				return new SelectColumn(null);
			}

			if (_token.Id == TokenId.Identifier && _lookahead.Id == TokenId.Dot)
			{
				// Ok, it could be the qualified all columns indicator (*). Let's see
				// what it is.

				int oldPos = _token.Pos;
				Identifier correlationName = ParseIdentifier();

				if (_lookahead.Id == TokenId.Multiply)
				{
					// Ok, we found <tableAlias>.*

					NextToken(); // Skip dot
					NextToken(); // Skip asterisk

					return new SelectColumn(correlationName);
				}
				else
				{
					// Ok, it is not an asterisk, rewind the lexer
					// and try to parse a normal expression.

					Reset(oldPos);
				}
			}

			// If we get here, it must be an expression.

			ExpressionNode expression = ParseExpression();
			Identifier columnAlias = ParseOptionalAlias();

			return new SelectColumn(expression, columnAlias);
		}

		private TableReference ParseTableReferences()
		{
			TableReference lastTableReference = null;

			do
			{
				TableReference tableReference = ParseTableReference();

				if (lastTableReference == null)
					lastTableReference = tableReference;
				else
				{
					JoinedTableReference crossJoinReference = new JoinedTableReference();
					crossJoinReference.Left = lastTableReference;
					crossJoinReference.Right = tableReference;
					crossJoinReference.JoinType = JoinType.Inner;
					lastTableReference = crossJoinReference;
				}

				if (_token.Id != TokenId.Comma)
					break;

				NextToken();
			} while (_token.Id != TokenId.Eof);

			return lastTableReference;
		}

		private TableReference ParseTableReference()
		{
			TableReference left;

			switch (_token.Id)
			{
				case TokenId.LeftParentheses:
				{
					if (_lookahead.Id == TokenId.SELECT)
						left = ParseDerivedTableReference();
					else
					{
						NextToken();
						left = ParseTableReference();
						Match(TokenId.RightParentheses);
					}

					break;
				}

				case TokenId.Identifier:
				{
					_rangeRecorder.Begin();
					Identifier tableName = ParseIdentifier();
					SourceRange tableNameSourceRange = _rangeRecorder.End();
					_rangeRecorder.Begin();
					Identifier correlationName = ParseOptionalAlias();
					SourceRange correlationNameSourceRange = _rangeRecorder.End();

					NamedTableReference namedTableReference = new NamedTableReference();
					namedTableReference.TableName = tableName;
					namedTableReference.TableNameSourceRange = tableNameSourceRange;
					namedTableReference.CorrelationName = correlationName;
					namedTableReference.CorrelationNameSourceRange = correlationNameSourceRange;
					left = namedTableReference;
					break;
				}

				default:
					_errorReporter.TableReferenceExpected(_token.Range, _token.Text);
					return null;
			}

			while (_token.Id != TokenId.Eof)
			{
				JoinType joinType = JoinType.Inner;
				bool isCrossJoin = false;

				switch (_token.Id)
				{
					case TokenId.CROSS:
						NextToken();
						isCrossJoin = true;
						goto case TokenId.JOIN;

					case TokenId.INNER:
						NextToken();
						goto case TokenId.JOIN;

					case TokenId.LEFT:
						NextToken();
						if (_token.Id == TokenId.OUTER)
							NextToken();
						joinType = JoinType.LeftOuter;
						goto case TokenId.JOIN;

					case TokenId.RIGHT:
						NextToken();
						if (_token.Id == TokenId.OUTER)
							NextToken();
						joinType = JoinType.RightOuter;
						goto case TokenId.JOIN;

					case TokenId.FULL:
						NextToken();
						if (_token.Id == TokenId.OUTER)
							NextToken();
						joinType = JoinType.FullOuter;
						goto case TokenId.JOIN;

					case TokenId.JOIN:
						Match(TokenId.JOIN);
						JoinedTableReference joinedTableReference = new JoinedTableReference();
						joinedTableReference.JoinType = joinType;
						joinedTableReference.Left = left;
						joinedTableReference.Right = ParseTableReference();

						if (!isCrossJoin)
						{
							Match(TokenId.ON);
							joinedTableReference.Condition = ParseExpression();
						}
						left = joinedTableReference;
						break;
					default:
						goto exitLoop;
				}
			}
		exitLoop:
			return left;
		}

		private DerivedTableReference ParseDerivedTableReference()
		{
			Match(TokenId.LeftParentheses);

			DerivedTableReference result = new DerivedTableReference();
			result.Query = ParseQuery();

			Match(TokenId.RightParentheses);

			if (_token.Id == TokenId.AS)
				NextToken();

			_rangeRecorder.Begin();
			Identifier correlationName = ParseIdentifier();
			SourceRange correlationNameSourceRange = _rangeRecorder.End();

			result.CorrelationName = correlationName;
			result.CorrelationNameSourceRange = correlationNameSourceRange;
			return result;
		}

		private OrderByColumn[] ParseOrderByColumns()
		{
			List<OrderByColumn> columns = new List<OrderByColumn>();

			while (_token.Id != TokenId.Eof)
			{
				ExpressionNode expression = ParseExpression();

				if (expression == null)
					break;

				SortOrder sortOrder = SortOrder.Ascending;

				if (_token.Id == TokenId.ASC)
				{
					NextToken();
				}
				else if (_token.Id == TokenId.DESC)
				{
					NextToken();
					sortOrder = SortOrder.Descending;
				}

				columns.Add(new OrderByColumn(expression, sortOrder));

				if (_token.Id != TokenId.Comma)
					break;

				NextToken();
			}

			return columns.ToArray();
		}

		private Identifier ParseOptionalAlias()
		{
			if (_token.Id == TokenId.Identifier)
				return ParseIdentifier();

			if (_token.Id == TokenId.AS)
			{
				NextToken();

				return ParseIdentifier();
			}

			return null;
		}

		private ExpressionNode ParseExpression()
		{
			return ParseSubExpression(null, 0);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private ExpressionNode ParseSubExpression(ExpressionNode left, int precedence)
		{
			if (left == null)
			{
				// No left operand, so we parse one and take care about leading unary operators

				if (_token.Info.UnaryOperator != null)
				{
					UnaryOperator op = _token.Info.UnaryOperator;

					NextToken();

					ExpressionNode expr = ParseSubExpression(null, op.Precedence);
					left = new UnaryExpression(op, expr);
				}
				else
				{
					left = ParseSimpleExpression();
				}
			}

			while (_token.Id != TokenId.Eof)
			{
				// Special handling for NOT BETWEEN, NOT IN, NOT LIKE, NOT SIMILAR TO, and NOT SOUNDSLIKE.

				bool negated = false;

				if (_token.Id == TokenId.NOT)
				{
					if (_lookahead.Id == TokenId.BETWEEN ||
						_lookahead.Id == TokenId.IN ||
						_lookahead.Id == TokenId.LIKE ||
						_lookahead.Id == TokenId.SIMILAR ||
						_lookahead.Id == TokenId.SOUNDSLIKE)
					{
						NextToken();
						negated = true;
					}
				}

				// Special handling for the only ternary operator BETWEEN

				if (_token.Id == TokenId.BETWEEN)
				{
					NextToken();
					ExpressionNode lowerBound = ParseSubExpression(null, Operator.BETWEEN_PRECEDENCE);
					Match(TokenId.AND);
					ExpressionNode upperBound = ParseSubExpression(null, Operator.BETWEEN_PRECEDENCE);

					left = new BetweenExpression(left, lowerBound, upperBound);
				}
				else
				{
					// If there is no binary operator we are finished

					if (_token.Info.BinaryOperator == null)
						break;

					BinaryOperator binaryOp = _token.Info.BinaryOperator;

					// Precedence is lower, parse it later

					if (binaryOp.Precedence < precedence)
						break;

					// Precedence is equal, but operator ist not right associative, parse it later

					if (binaryOp.Precedence == precedence && !binaryOp.IsRightAssociative)
						break;

					// Precedence is higher

					NextToken();

					// Special handling for SIMILAR TO

					if (binaryOp == BinaryOperator.SimilarTo)
						Match(TokenId.TO);

					if (binaryOp == BinaryOperator.In)
					{
						// Special handling for IN

						InExpression inExpression = new InExpression();
						inExpression.Left = left;
						inExpression.RightExpressions = ParseSimpleQueryExpressionList();
						left = inExpression;
					}
					else if (_token.Id == TokenId.ANY || _token.Id == TokenId.SOME || _token.Id == TokenId.ALL)
					{
						// Special handling for ANY (SOME) and ALL

						if (binaryOp != BinaryOperator.Equal &&
							binaryOp != BinaryOperator.NotEqual &&
							binaryOp != BinaryOperator.Less &&
							binaryOp != BinaryOperator.LessOrEqual &&
							binaryOp != BinaryOperator.Greater &&
							binaryOp != BinaryOperator.GreaterOrEqual)
						{
							_errorReporter.InvalidOperatorForAllAny(_token.Range, binaryOp);
						}

						AllAnySubselect allAnySubselect = new AllAnySubselect();
						allAnySubselect.Left = left;
						allAnySubselect.Op = binaryOp;
						allAnySubselect.Type = (_token.Id == TokenId.ALL) ? AllAnySubselect.AllAnyType.All : AllAnySubselect.AllAnyType.Any;
						NextToken();
						Match(TokenId.LeftParentheses);
						allAnySubselect.Query = ParseQuery();
						Match(TokenId.RightParentheses);
						left = allAnySubselect;
					}
					else
					{
						left = new BinaryExpression(binaryOp, left, ParseSubExpression(null, binaryOp.Precedence));
					}
				}

				// Special handling for negated expressions (see above)

				if (negated)
					left = new UnaryExpression(UnaryOperator.LogicalNot, left);
			}

			return left;
		}

		private ExpressionNode[] ParseSimpleQueryExpressionList()
		{
			if (_token.Id == TokenId.LeftParentheses && _lookahead.Id == TokenId.SELECT)
			{
				NextToken();
				SingleRowSubselect singleRowSubselect = new SingleRowSubselect();
				singleRowSubselect.Query = ParseQuery();
				Match(TokenId.RightParentheses);

				return new ExpressionNode[] { singleRowSubselect };
			}

			return ParseExpressionList();
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private ExpressionNode ParsePrimaryExpression()
		{
			switch (_token.Id)
			{
				case TokenId.NULL:
					NextToken();
					return LiteralExpression.FromNull();

				case TokenId.TRUE:
				case TokenId.FALSE:
					return ParseBooleanLiteral();

				case TokenId.Date:
					return ParseDateLiteral();

				case TokenId.Number:
					return ParseNumberLiteral();

				case TokenId.String:
					return LiteralExpression.FromString(ParseString());

				case TokenId.EXISTS:
				{
					ExistsSubselect result = new ExistsSubselect();
					NextToken();
					Match(TokenId.LeftParentheses);
					result.Query = ParseQuery();
					Match(TokenId.RightParentheses);

					return result;
				}

				case TokenId.ParameterMarker:
				{
					_rangeRecorder.Begin();
					NextToken();
					Identifier name = ParseIdentifier();
					SourceRange nameSourceRange = _rangeRecorder.End();

					ParameterExpression result = new ParameterExpression();
					result.Name = name;
					result.NameSourceRange = nameSourceRange;
					return result;
				}

				case TokenId.CAST:
				{
					NextToken();
					CastExpression castExpression = new CastExpression();
					Match(TokenId.LeftParentheses);
					castExpression.Expression = ParseExpression();
					Match(TokenId.AS);
					castExpression.TypeReference = ParseTypeReference();
					Match(TokenId.RightParentheses);

					return castExpression;
				}

				case TokenId.CASE:
				{
					NextToken();

					CaseExpression caseExpression = new CaseExpression();

					if (_token.Id != TokenId.WHEN && _token.Id != TokenId.ELSE && _token.Id != TokenId.END)
						caseExpression.InputExpression = ParseExpression();

					List<ExpressionNode> whenExpressionList = new List<ExpressionNode>();
					List<ExpressionNode> expressionList = new List<ExpressionNode>();

					if (_token.Id != TokenId.WHEN)
					{
						Match(TokenId.WHEN);
					}
					else
					{
						while (_token.Id == TokenId.WHEN)
						{
							NextToken();

							whenExpressionList.Add(ParseExpression());
							Match(TokenId.THEN);
							expressionList.Add(ParseExpression());
						}
					}

					caseExpression.WhenExpressions = whenExpressionList.ToArray();
					caseExpression.ThenExpressions = expressionList.ToArray();

					if (_token.Id == TokenId.ELSE)
					{
						NextToken();
						caseExpression.ElseExpression = ParseExpression();
					}

					Match(TokenId.END);

					return caseExpression;
				}

				case TokenId.COALESCE:
				{
					NextToken();
					CoalesceExpression coalesceExpression = new CoalesceExpression();
					coalesceExpression.Expressions = ParseExpressionList();
					return coalesceExpression;
				}

				case TokenId.NULLIF:
				{
					NextToken();
					NullIfExpression nullIfExpression = new NullIfExpression();
					Match(TokenId.LeftParentheses);
					nullIfExpression.LeftExpression = ParseExpression();
					Match(TokenId.Comma);
					nullIfExpression.RightExpression = ParseExpression();
					Match(TokenId.RightParentheses);
					return nullIfExpression;
				}

				case TokenId.Identifier:
				{
					_rangeRecorder.Begin();
					Identifier name = ParseIdentifier();
					SourceRange nameSourceRange = _rangeRecorder.End();

					if (_token.Id != TokenId.LeftParentheses)
					{
						NameExpression result = new NameExpression();
						result.Name = name;
						result.NameSourceRange = nameSourceRange;
						return result;
					}
					else
					{
						bool hasAsteriskModifier;
						ExpressionNode[] args;

						if (_lookahead.Id != TokenId.Multiply)
						{
							hasAsteriskModifier = false;
							args = ParseExpressionList();
						}
						else
						{
							NextToken();
							NextToken();
							Match(TokenId.RightParentheses);

							hasAsteriskModifier = true;
							args = new ExpressionNode[0];
						}

						FunctionInvocationExpression result = new FunctionInvocationExpression();
						result.Name = name;
						result.NameSourceRange = nameSourceRange;
						result.Arguments = args;
						result.HasAsteriskModifier = hasAsteriskModifier;
						return result;
					}
				}

				case TokenId.LeftParentheses:
				{
					NextToken();

					ExpressionNode expr;

					if (_token.Id != TokenId.SELECT)
					{
						expr = ParseExpression();
					}
					else
					{
						SingleRowSubselect singleRowSubselect = new SingleRowSubselect();
						singleRowSubselect.Query = ParseQuery();
						expr = singleRowSubselect;
					}

					Match(TokenId.RightParentheses);
					return expr;
				}

				default:
				{
					_errorReporter.SimpleExpressionExpected(_token.Range, _token.Text);
					return LiteralExpression.FromNull();
				}
			}
		}

		private ExpressionNode ParseMemberExpression()
		{
			ExpressionNode leftSide = ParsePrimaryExpression();

			while (_token.Id == TokenId.Dot)
			{
				NextToken();

				_rangeRecorder.Begin();
				Identifier memberName = ParseIdentifier();
				SourceRange memberNameSourceRange = _rangeRecorder.End();

				if (_token.Id != TokenId.LeftParentheses)
				{
					PropertyAccessExpression result = new PropertyAccessExpression();
					result.Target = leftSide;
					result.Name = memberName;
					result.NameSourceRange = memberNameSourceRange;
					leftSide = result;
				}
				else
				{
					ExpressionNode[] arguments = ParseExpressionList();
					MethodInvocationExpression result = new MethodInvocationExpression();
					result.Target = leftSide;
					result.Name = memberName;
					result.NameSourceRange = memberNameSourceRange;
					result.Arguments = arguments;
					leftSide = result;
				}
			}

			return leftSide;
		}

		private ExpressionNode ParseIsNullExpression()
		{
			ExpressionNode expr = ParseMemberExpression();

			if (_token.Id == TokenId.IS)
			{
				NextToken();

				bool negated = false;

				if (_token.Id == TokenId.NOT)
				{
					negated = true;
					NextToken();
				}

				Match(TokenId.NULL);

				return new IsNullExpression(negated, expr);
			}

			return expr;
		}

		private ExpressionNode ParseSimpleExpression()
		{
			return ParseIsNullExpression();
		}

		private TypeReference ParseTypeReference()
		{
			TypeReference typeReference = new TypeReference();

			if (_token.Id == TokenId.String)
			{
				_rangeRecorder.Begin();
				typeReference.TypeName = ParseString();
				typeReference.TypeNameSourceRange = _rangeRecorder.End();
				typeReference.CaseSensitve = true;
			}
			else
			{
				_rangeRecorder.Begin();

				StringBuilder sb = new StringBuilder();
				bool wasVerbatim = false;
				while (_token.Id != TokenId.Eof)
				{
					Identifier identifier = ParseIdentifier();

					if (!identifier.Verbatim && !identifier.Parenthesized)
					{
						sb.Append(identifier.Text);
					}
					else
					{
						wasVerbatim = true;

						// Include quotes and brackets for better error reporting.
						sb.Append(identifier.ToString());
					}

					if (_token.Id != TokenId.Dot)
						break;

					sb.Append(".");
					NextToken();
				}

				string typeName = sb.ToString();
				SourceRange typeNameSourceRange = _rangeRecorder.End();

				if (wasVerbatim)
					_errorReporter.InvalidTypeReference(typeNameSourceRange, typeName);

				typeReference.TypeName = typeName;
				typeReference.TypeNameSourceRange = typeNameSourceRange;
				typeReference.CaseSensitve = false;
			}

			return typeReference;
		}

		private Identifier ParseIdentifier()
		{
			if (_token.Id == TokenId.Identifier)
			{
				string text = _token.Text;
				NextToken();

				return Identifier.InternalFromSource(text);
			}

			Match(TokenId.Identifier);
			return Identifier.Missing;
		}

		private LiteralExpression ParseBooleanLiteral()
		{
			bool value = _token.Id == TokenId.TRUE;
			NextToken();

			return LiteralExpression.FromBoolean(value);
		}

		private LiteralExpression ParseDateLiteral()
		{
			string text = _token.Text;
			SourceRange tokenRange = _token.Range;
			NextToken();

			DateTime value;

			if (text.Length < 3 || text[0] != '#' || text[text.Length - 1] != '#')
			{
				_errorReporter.InvalidDate(tokenRange, text);
				value = DateTime.MinValue;
			}
			else
			{
				string textWithoutDelimiters = text.Substring(1, text.Length - 2);
				try
				{
					value = DateTime.Parse(textWithoutDelimiters, CultureInfo.InvariantCulture);
				}
				catch (FormatException)
				{
					_errorReporter.InvalidDate(tokenRange, textWithoutDelimiters);
					value = DateTime.MinValue;
				}
			}

			return LiteralExpression.FromDateTime(value);
		}

		private ExpressionNode ParseNumberLiteral()
		{
			string text = _token.Text;

			bool hasHexModifier = text.EndsWith("h", StringComparison.OrdinalIgnoreCase);
			bool hasExponentialModifier = text.IndexOfAny(new char[] { '.', 'E', 'e' }) != -1;
			if (hasExponentialModifier && !hasHexModifier)
				return LiteralExpression.FromDouble(ParseReal());

			long integer = ParseInteger();

			// If the integer can be represented as Int32 we return
			// an Int32 literal. Otherwise we return an Int64.

			try
			{
				checked
				{
					return LiteralExpression.FromInt32((int)integer);
				}
			}
			catch (OverflowException)
			{
				return LiteralExpression.FromInt64(integer);
			}
		}

		private long ParseBinary(SourceRange tokenRange, string binary)
		{
			long val = 0;

			for (int i = binary.Length - 1, j = 0; i >= 0; i--, j++)
			{
				if (binary[i] == '0')
				{
					// Nothing to add
				}
				else if (binary[i] == '1')
				{
					checked
					{
						// Don't use >> because this implicitly casts the operator to Int32. 
						// Also this operation will never detect an overflow.
						val += (long)Math.Pow(2, j);
					}
				}
				else
				{
					_errorReporter.InvalidBinary(tokenRange, binary);
					return 0;
				}
			}

			return val;
		}

		private long ParseOctal(SourceRange tokenRange, string octal)
		{
			long val = 0;

			for (int i = octal.Length - 1, j = 0; i >= 0; i--, j++)
			{
				int c;

				try
				{
					c = Int32.Parse(new string(octal[i], 1), CultureInfo.InvariantCulture);

					if (c > 7)
					{
						_errorReporter.InvalidOctal(tokenRange, octal);
						return 0;
					}
				}
				catch (FormatException)
				{
					_errorReporter.InvalidOctal(tokenRange, octal);
					return 0;
				}

				checked
				{
					val += (long)(c * Math.Pow(8, j));
				}
			}

			return val;
		}

		private string ParseString()
		{
			string text = _token.Text;
			NextToken();

			StringBuilder sb = new StringBuilder(text.Length);

			// Text includes leading/trailing/masking quotes

			for (int i = 1; i < text.Length - 1; i++)
			{
				if (text[i] == '\'' && text[i + 1] == '\'')
					i++;

				sb.Append(text[i]);
			}

			return sb.ToString();
		}

		private double ParseReal()
		{
			string number = _token.Text;
			SourceRange tokenRange = _token.Range;
			NextToken();

			try
			{
				return Double.Parse(number, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
			}
			catch (OverflowException)
			{
				_errorReporter.NumberTooLarge(tokenRange, number);
			}
			catch (FormatException)
			{
				_errorReporter.InvalidReal(tokenRange, number);
			}

			return 0;
		}

		private long ParseInteger()
		{
			string text = _token.Text;
			SourceRange tokenRange = _token.Range;
			NextToken();

			// Get indicator

			char indicator = text[text.Length - 1];

			// Remove trailing indicator (h, b, or o)

			string textWithoutIndicator = text.Substring(0, text.Length - 1);

			switch (indicator)
			{
				case 'H':
				case 'h':
					try
					{
						return Int64.Parse(textWithoutIndicator, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
					}
					catch (OverflowException)
					{
						_errorReporter.NumberTooLarge(tokenRange, textWithoutIndicator);
					}
					catch (FormatException)
					{
						_errorReporter.InvalidHex(tokenRange, textWithoutIndicator);
					}

					return 0;

				case 'B':
				case 'b':
					try
					{
						return ParseBinary(tokenRange, textWithoutIndicator);
					}
					catch (OverflowException)
					{
						_errorReporter.NumberTooLarge(tokenRange, textWithoutIndicator);
					}

					return 0;

				case 'O':
				case 'o':
					try
					{
						return ParseOctal(tokenRange, textWithoutIndicator);
					}
					catch (OverflowException)
					{
						_errorReporter.NumberTooLarge(tokenRange, textWithoutIndicator);
					}

					return 0;

				default:
					try
					{
						return Int64.Parse(text, CultureInfo.InvariantCulture);
					}
					catch (OverflowException)
					{
						_errorReporter.NumberTooLarge(tokenRange, text);
					}
					catch (FormatException)
					{
						_errorReporter.InvalidInteger(tokenRange, text);
					}

					return 0;
			}
		}

		private ExpressionNode[] ParseExpressionList()
		{
			NextToken();

			if (_token.Id == TokenId.RightParentheses)
			{
				NextToken();
				return new ExpressionNode[0];
			}

			ExpressionNode[] arguments = ParseExpressions();

			Match(TokenId.RightParentheses);

			return arguments;
		}

		private ExpressionNode[] ParseExpressions()
		{
			List<ExpressionNode> arguments = new List<ExpressionNode>();

			while (_token.Id != TokenId.Eof)
			{
				ExpressionNode arg = ParseExpression();

				if (arg == null)
					break;

				arguments.Add(arg);

				if (_token.Id != TokenId.Comma)
					break;

				NextToken();
			}
			return arguments.ToArray();
		}
	}
}