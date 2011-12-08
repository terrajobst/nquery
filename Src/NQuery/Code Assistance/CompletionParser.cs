using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using NQuery.Compilation;

namespace NQuery.CodeAssistance 
{
	internal sealed class CompletionParser
	{
		private string _source;
		private TokenStream _tokenStream;
		private IErrorReporter _errorReporter;

		public CompletionParser(IErrorReporter errorReporter, string source)
		{
			_errorReporter = errorReporter;
			_tokenStream = TokenStream.FromSource(errorReporter, source);
			_source = source;
		}

		public Token GetTokenBeforeSourcePos(SourceLocation sourceLocation)
		{
			_tokenStream.ResetToStart();
			_tokenStream.SkipTo(sourceLocation);
			return _tokenStream.Token;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ExpressionNode ParseExpressionBeforeDot(SourceLocation location, out Identifier remainder)
		{
			remainder = null;

			_tokenStream.ResetToStart();
			_tokenStream.SkipTo(location);

			// Check for remainder

			if (_tokenStream.Token.Id == TokenId.Identifier || _tokenStream.Token.Info.IsKeyword)
			{
				remainder = Identifier.InternalFromSource(_tokenStream.Token.Text);
				_tokenStream.ReadPrevious();
			}

			// If the current token is not a dot there is no expression before the dot

			if (_tokenStream.Token.Id != TokenId.Dot)
				return null;

			// We will now read back the whole path until the start of the expression is
			// reached. The path can be compound of identifiers, parenthesized expressions, 
			// and method invocations. The first token can be an identifier, a parameter or 
			// a literal.

			int endPosition = _tokenStream.Token.Pos - 1;
			int startPosition;

			while (_tokenStream.Token.Id == TokenId.Dot)
			{
				// Skip dot.
				
				if (!_tokenStream.ReadPrevious())
				{
					// The dot cannot be the first token
					_errorReporter.SimpleExpressionExpected(_tokenStream.Token.Range, _tokenStream.Token.Text);
					return null;
				}

				// Let's see what is before the dot.
				
				if (_tokenStream.Token.Id == TokenId.Identifier)
				{
					if (_tokenStream.ReadPrevious() && _tokenStream.Token.Id != TokenId.Dot)
					{
						// Check for possible leading parameter marker.

						if (_tokenStream.Token.Id != TokenId.ParameterMarker)
							_tokenStream.ReadNext();
					}
				}
				else if (_tokenStream.Token.Id == TokenId.RightParentheses)
				{
					// We first try to find the matching parentheses

					int closeParenthesesCount = 1;

					while (closeParenthesesCount != 0 && _tokenStream.ReadPrevious())
					{
						if (_tokenStream.Token.Id == TokenId.LeftParentheses)
							closeParenthesesCount--;
						else if (_tokenStream.Token.Id == TokenId.RightParentheses)
							closeParenthesesCount++;
					}
					
					// Did we find the matching one?

					if (closeParenthesesCount == 0)
					{
						// Ok, now lets see if the previous token is an identifer. If
						// so it belongs to the current expression and forms an
						// invocation instead of parenthesized expression.

                        // Ok, now lets look at the previous token.
					    // If it is an identifer it belongs to the current expression and forms an
                        // invocation instead of parenthesized expression.
                        // If it is COALESCE, CAST, NULLIF it also belongs to the current expressions.
                        // and forms a pseudo invocation.
							
						if (_tokenStream.ReadPrevious())
						{
                            bool tokenBelongsToExpression = _tokenStream.Token.Id == TokenId.Identifier ||
                                                            _tokenStream.Token.Id == TokenId.COALESCE ||
                                                            _tokenStream.Token.Id == TokenId.CAST ||
                                                            _tokenStream.Token.Id == TokenId.NULLIF;
                            if (!tokenBelongsToExpression)
							{
								// Token does not belong to expression. So we undo the last read operation.
								_tokenStream.ReadNext();
							}
							else
							{
							    // If the token was an identifier we also look at previous token to see
							    // whether it could be a method invocation.

                                if (_tokenStream.Token.Id == TokenId.Identifier)
                                {
                                    if (_tokenStream.ReadPrevious())
                                    {
                                        // If it is a dot, it is a method invocation.
                                        
                                        if (_tokenStream.Token.Id != TokenId.Dot)
                                        {
                                            // No, it was not. So we undo the last read operation.
                                            _tokenStream.ReadNext();
                                        }
                                    }
                                }
							}
						}
					}
				}
				else if (_tokenStream.Token.Id == TokenId.String ||
				         _tokenStream.Token.Id == TokenId.Date || 
			             _tokenStream.Token.Id == TokenId.Number ||
			             _tokenStream.Token.Id == TokenId.TRUE || 
			             _tokenStream.Token.Id == TokenId.FALSE)
				{
					break;
				}
				else
				{
					// Report error at dot
					
					_tokenStream.ReadNext();
					_errorReporter.SimpleExpressionExpected(_tokenStream.Token.Range, _tokenStream.Token.Text);
					return null;
				}
			}
						
			// Extract source of the expression and parse it

			startPosition = _tokenStream.Token.Pos;
			int length = endPosition - startPosition + 1;

			if (startPosition >= 0 && length <= _source.Length)
			{
				string expressionSource = _source.Substring(startPosition, length);

				Parser p = new Parser(_errorReporter);
				return p.ParseExpression(expressionSource);
			}

			// This indicates an internal error in the logic

			throw ExceptionBuilder.InternalError("CompletionParser.ExpressionBeforeDot(): Illegal code flow.");
		}

		public Identifier GetInvocation(SourceLocation location, out int argumentIndex, out ExpressionNode expressionBeforeDot)
		{
			expressionBeforeDot = null;
			argumentIndex = 0;

			_tokenStream.ResetToStart();
			_tokenStream.SkipTo(location);

			int parenthesesCount = 0;

			while (true)
			{
				switch (_tokenStream.Token.Id)
				{
					case TokenId.Comma:
						if (parenthesesCount == 0)
							argumentIndex++;
						break;

					case TokenId.RightParentheses:
						parenthesesCount++;
						break;

					case TokenId.LeftParentheses:
					{
						if (parenthesesCount > 0)
						{
							parenthesesCount--;
						}
						else
						{
							if (!_tokenStream.ReadPrevious())
								return null;

							if (_tokenStream.Token.Id == TokenId.Identifier)
							{
								// Got it!
								Identifier result;
								expressionBeforeDot = ParseExpressionBeforeDot(_tokenStream.Token.Range.EndLocation, out result);
								return result;
							}
						}
						break;
					}
				}

				if (!_tokenStream.ReadPrevious())
					return null;
			}
		}

		private class TableReferenceScope
		{
            public List<NamedTableReference> TableReferences = new List<NamedTableReference>();
		}

		private NamedTableReference ParseTableReference()
		{
			// Read table name (unlike regular SQL the name cannot be qualified)

			Identifier tableIdentifier = Identifier.InternalFromSource(_tokenStream.Token.Text);

			// The default correlation name is the table name

			Identifier correlationName = tableIdentifier;

			// Override this default by the real correlation name (if present)

			_tokenStream.ReadNext();

			if (_tokenStream.Token.Id != TokenId.AS && _tokenStream.Token.Id != TokenId.Identifier)
			{
				_tokenStream.ReadPrevious();
			}
			else
			{
				if (_tokenStream.Token.Id == TokenId.AS)
					_tokenStream.ReadNext();

				if (_tokenStream.Token.Id == TokenId.Identifier)
					correlationName = Identifier.InternalFromSource(_tokenStream.Token.Text);
			}

			NamedTableReference namedTableReference = new NamedTableReference();
			namedTableReference.CorrelationName = correlationName;
			namedTableReference.TableName = tableIdentifier;
			return namedTableReference;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private TableReferenceScope[] GetTableReferenceScopes(SourceLocation sourceLocation)
		{
			Stack<TableReferenceScope> scopeStack = new Stack<TableReferenceScope>();
			List<NamedTableReference> currentReferences = null;
			int queryDepth = 0;
			int parenthesizedDepth = 0;
			bool endOfCurrentQueryReached = false;
			bool recordTableReferences = false;
			bool needPopScope = false;

			_tokenStream.ResetToStart();

			while (!endOfCurrentQueryReached && _tokenStream.Token.Id != TokenId.Eof)
			{
				switch (_tokenStream.Token.Id)
				{
					case TokenId.LeftParentheses:
						_tokenStream.ReadNext();
						if (_tokenStream.Token.Id != TokenId.SELECT)
						{
							parenthesizedDepth++;
							_tokenStream.ReadPrevious();
						}
						else
						{
							queryDepth++;
							goto case TokenId.SELECT;
						}
						break;

					case TokenId.RightParentheses:
						if (parenthesizedDepth > 0)
							parenthesizedDepth--;
						else if (queryDepth > 0)
						{
							queryDepth--;
							goto case TokenId.UNION;
						}
						break;

					case TokenId.SELECT:
						TableReferenceScope newScope = new TableReferenceScope();
						currentReferences = newScope.TableReferences;
						scopeStack.Push(newScope);
						break;

					case TokenId.UNION:
					case TokenId.INTERSECT:
					case TokenId.EXCEPT:
						if (_tokenStream.Token.Range.StartLocation >= sourceLocation)
							endOfCurrentQueryReached = true;
						needPopScope = true;
						recordTableReferences = false;
						break;

					case TokenId.FROM:
						recordTableReferences = true;
						break;

					case TokenId.WHERE:
					case TokenId.GROUP:
				    case TokenId.HAVING:
					case TokenId.ORDER:
						recordTableReferences = false;
						break;

					case TokenId.Identifier:
						if (recordTableReferences)
						{
							NamedTableReference namedTableReference = ParseTableReference();
							if (currentReferences != null)
								currentReferences.Add(namedTableReference);
						}
						break;
				}

				if (needPopScope && !endOfCurrentQueryReached && scopeStack.Count > 0)
				{
					scopeStack.Pop();

					if (scopeStack.Count == 0)
						currentReferences = null;
					else
						currentReferences = scopeStack.Peek().TableReferences;
				}
				needPopScope = false;

				_tokenStream.ReadNext();
			}

			TableReferenceScope[] result = new TableReferenceScope[scopeStack.Count];
			scopeStack.CopyTo(result, 0);
			return result;
		}

		public NamedTableReference[] GetTableReferencesOfQuery(SourceLocation sourceLocation)
		{
			List<NamedTableReference> namedTableReferences = new List<NamedTableReference>();
			TableReferenceScope[] scopes = GetTableReferenceScopes(sourceLocation);

			foreach (TableReferenceScope scope in scopes)
			{
				int upperBound = namedTableReferences.Count - 1;

				foreach (NamedTableReference tableReference in scope.TableReferences)
				{
					bool referenceIsHidden = false;

					for (int i = 0; i <= upperBound; i++)
					{
						NamedTableReference processedTableReference = namedTableReferences[i];

						if (tableReference.CorrelationName.Matches(processedTableReference.CorrelationName))
						{
							referenceIsHidden = true;
							break;
						}
					}

					if (!referenceIsHidden)
						namedTableReferences.Add(tableReference);
				}
			}
			
			return namedTableReferences.ToArray();
		}

		public NamedTableReference[] GetTableReferencesOfJoin(SourceLocation sourceLocation)
		{
			_tokenStream.ResetToStart();

			// Rewind token stream to the closest FROM

			_tokenStream.SkipTo(sourceLocation);
			_tokenStream.SkipToPrevious(TokenId.FROM);

			// Skip FROM token

			_tokenStream.ReadNext();

			// Read table references

			Stack<TableReferenceScope> scopeStack = new Stack<TableReferenceScope>();
			bool inJoinExpression = false;
			int expressionDepth = 0;

			while (_tokenStream.Token.Range.StartLocation < sourceLocation)
			{
				if (_tokenStream.Token.Id == TokenId.JOIN)
				{
					inJoinExpression = false;
				}
				else if (_tokenStream.Token.Id == TokenId.ON)
				{
					// Merge top two scopes
					if (scopeStack.Count >= 2)
					{
						TableReferenceScope secondScope = scopeStack.Pop();
						TableReferenceScope firstScope = scopeStack.Pop();
						TableReferenceScope newScope = new TableReferenceScope();
						newScope.TableReferences.AddRange(firstScope.TableReferences);
						newScope.TableReferences.AddRange(secondScope.TableReferences);
						scopeStack.Push(newScope);
					}

					inJoinExpression = true;
				}
				else if (_tokenStream.Token.Id == TokenId.Identifier && !inJoinExpression)
				{
					// Push named table on stack
					TableReferenceScope newScope = new TableReferenceScope();
					scopeStack.Push(newScope);
					NamedTableReference namedTableReference = ParseTableReference();
					newScope.TableReferences.Add(namedTableReference);
				}				
				else if (_tokenStream.Token.Id == TokenId.LeftParentheses && inJoinExpression)
				{
					expressionDepth++;
				}
				else if (_tokenStream.Token.Id == TokenId.LeftParentheses && !inJoinExpression)
				{
					// Push empty scope as marker for nested join start
					TableReferenceScope newScope = new TableReferenceScope();
					scopeStack.Push(newScope);
				}
				else if (_tokenStream.Token.Id == TokenId.RightParentheses)
				{
					if (expressionDepth > 0)
					{
						expressionDepth--;
					}
					else
					{
						inJoinExpression = false;

						if (scopeStack.Count > 0)
						{
							TableReferenceScope newScope = new TableReferenceScope();

							while (scopeStack.Count > 0)
							{
								TableReferenceScope scope = scopeStack.Pop();

								if (scope.TableReferences.Count == 0)
									break;

								newScope.TableReferences.AddRange(scope.TableReferences);
							}

							scopeStack.Push(newScope);
						}
					}
				}

				_tokenStream.ReadNext();
			}

			if (scopeStack.Count == 0)
				return new NamedTableReference[0];
			else
			{
				TableReferenceScope scope = scopeStack.Pop();
				return scope.TableReferences.ToArray();
			}
		}
	}
}