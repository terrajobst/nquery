using System;
using System.Collections.Generic;

using NQuery.Compilation;
using NQuery.Runtime;

namespace NQuery.CodeAssistance
{
	internal sealed class CodeAssistanceContextProvider : ICodeAssistanceContextProvider
	{
		private Scope _scope;
		private ErrorCollector _errorReporter = new ErrorCollector();
		private CompletionParser _completionParser;
        private Normalizer _normalizer;
		private Resolver _resolver;

		public CodeAssistanceContextProvider(Scope scope, string source)
		{
			_scope = scope;
			_completionParser = new CompletionParser(_errorReporter, source);
			_resolver = new Resolver(_errorReporter, _scope);
		    _normalizer = new Normalizer();
		}

		private void ThrowErrors()
		{
			if (_errorReporter.ErrorsSeen)
				throw ExceptionBuilder.CodeAssistanceFailed(_errorReporter.GetErrors());
		}

		private void DeclareTableRefs(SourceLocation sourceLocation)
		{
			NamedTableReference[] namedTableReferences = _completionParser.GetTableReferencesOfQuery(sourceLocation);
			DeclareTableRefs(namedTableReferences);
		}

		private void DeclareTableRefs(IEnumerable<NamedTableReference> namedTableReferences)
		{
			foreach (NamedTableReference namedTableReference in namedTableReferences)
			{
				TableBinding[] tables = _scope.DataContext.Tables.Find(namedTableReference.TableName);

				if (tables != null && tables.Length == 1)
				{
					TableBinding tableBinding = tables[0];
					_resolver.CurrentScope.DeclareTableRef(tableBinding, namedTableReference.CorrelationName);
				}
			}
		}

	    private ExpressionNode ResolveExpression(ExpressionNode expressionBeforeDot)
	    {
	        ExpressionNode resolvedExpression = _normalizer.VisitExpression(expressionBeforeDot);
	        return _resolver.VisitExpression(resolvedExpression);
	    }

	    public IMemberCompletionContext ProvideMemberCompletionContext(SourceLocation sourceLocation)
		{
			if (_scope.DataContext == null || _scope.Parameters == null)
				return new NullMemberCompletionContext(sourceLocation);
			
			_errorReporter.Reset();
			
			Token token = _completionParser.GetTokenBeforeSourcePos(sourceLocation);

			Identifier remainingPart;

			if (token.Id != TokenId.Identifier)
				remainingPart = null;
			else
			{
				remainingPart = Identifier.InternalFromSource(token.Text);
				SourceLocation oneBefore = token.Range.StartLocation;
				oneBefore--;
				token = _completionParser.GetTokenBeforeSourcePos(oneBefore);
			}

			if (token.Id == TokenId.ON)
			{
				NamedTableReference[] joinedTables = _completionParser.GetTableReferencesOfJoin(sourceLocation);
				DeclareTableRefs(joinedTables);
				return new TableRelationMemberContext(sourceLocation, remainingPart, _scope, _resolver.CurrentScope);
			}
			else if (token.Id == TokenId.JOIN)
			{
				NamedTableReference[] joinedTables = _completionParser.GetTableReferencesOfJoin(sourceLocation);
				DeclareTableRefs(joinedTables);
				return new JoinTableMemberContext(sourceLocation, remainingPart, _scope, _resolver.CurrentScope);
			}
			else
			{
				ExpressionNode expressionBeforeDot = _completionParser.ParseExpressionBeforeDot(sourceLocation, out remainingPart);

				ThrowErrors();

				DeclareTableRefs(sourceLocation);

				if (expressionBeforeDot == null)
				{
					return new GlobalScopeMemberContext(sourceLocation, remainingPart, _scope, _resolver.CurrentScope);
				}
				else
				{			
					NameExpression expressionAsName = expressionBeforeDot as NameExpression;

					if (expressionAsName != null)
					{
						// Try to resolve a table name.

						foreach (NamedTableReference namedTableReference in _completionParser.GetTableReferencesOfQuery(sourceLocation))
						{
							if (expressionAsName.Name.Matches(namedTableReference.CorrelationName))
							{
								TableBinding[] tables = _scope.DataContext.Tables.Find(namedTableReference.TableName);

								if (tables != null && tables.Length == 1)
								{
									TableBinding tableBinding = tables[0];
									return new TableContext(sourceLocation, remainingPart, _scope, tableBinding, namedTableReference.CorrelationName.Text);
								}

								break;
							}
						}
					}

					expressionBeforeDot = ResolveExpression(expressionBeforeDot);

					if (expressionBeforeDot != null && expressionBeforeDot.ExpressionType != null)
						return new MemberContext(sourceLocation, remainingPart, _scope, expressionBeforeDot);
				}
			}

			ThrowErrors();
			
			return new NullMemberCompletionContext(sourceLocation);
		}

	    public IParameterInfoContext ProvideParameterInfoContext(SourceLocation sourceLocation)
		{
			if (_scope.DataContext == null || _scope.Parameters == null)
				return new NullParameterInfoContext(sourceLocation);
			
			int parameterIndex;
			ExpressionNode expressionBeforeDot;
			Identifier functionName = _completionParser.GetInvocation(sourceLocation, out parameterIndex, out expressionBeforeDot);

			ThrowErrors();
			
			if (functionName != null)
			{
				if (expressionBeforeDot == null)
				{
					return new FunctionOrAggregateParameterInfoContext(sourceLocation, parameterIndex, _scope, functionName);
				}
				else
				{
					DeclareTableRefs(sourceLocation);
                    expressionBeforeDot = ResolveExpression(expressionBeforeDot);

					if (expressionBeforeDot != null && expressionBeforeDot.ExpressionType != null)
						return new MethodParameterInfoContext(sourceLocation, parameterIndex, _scope, expressionBeforeDot.ExpressionType, functionName);
				}
			}
			
			ThrowErrors();
			
			return new NullParameterInfoContext(sourceLocation);
		}
	}
}