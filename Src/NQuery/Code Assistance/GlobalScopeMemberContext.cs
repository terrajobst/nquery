using System;
using System.Collections.Generic;

using NQuery.Compilation;
using NQuery.Runtime;

namespace NQuery.CodeAssistance
{
	internal sealed class GlobalScopeMemberContext : MemberCompletionContext
	{
		private Scope _scope;
		private QueryScope _queryScope;

		public GlobalScopeMemberContext(SourceLocation sourceLocation, Identifier remainingPart, Scope scope, QueryScope queryScope)
			: base(sourceLocation, remainingPart)
		{
			_scope = scope;
			_queryScope = queryScope;
		}

		public override void Enumerate(IMemberCompletionAcceptor acceptor)
		{
			if (acceptor == null)
				throw ExceptionBuilder.ArgumentNull("acceptor");
			
			// Enumerate the complete global scope
			//
			// 1.	Enumerate all table refs and column refs as they are part
			//		of the query scope.

			foreach (TableRefBinding tableRefBinding in _queryScope.GetAllTableRefBindings())
				acceptor.AcceptTableRef(tableRefBinding);

			foreach (ColumnRefBinding columnRefBinding in _queryScope.GetAllColumnRefBindings())
				acceptor.AcceptColumnRef(columnRefBinding);

			// 2. Enumerate all tables, constants, aggregates and functions

			foreach (TableBinding table in _scope.DataContext.Tables)
				acceptor.AcceptTable(table);

			foreach (ConstantBinding constant in _scope.DataContext.Constants)
				acceptor.AcceptConstant(constant);

			foreach (AggregateBinding aggregateBinding in _scope.DataContext.Aggregates)
				acceptor.AcceptAggregate(aggregateBinding);

			foreach (FunctionBinding functionBinding in _scope.DataContext.Functions)
				acceptor.AcceptFunction(functionBinding);

			// 3. Enumerate parameters
			
			foreach (ParameterBinding parameter in _scope.Parameters)
				acceptor.AcceptParameter(parameter);

			// 4. Enumerate keywords

			TokenId[] tokenIDs = (TokenId[]) Enum.GetValues(typeof (TokenId));

			foreach (TokenId tokenID in tokenIDs)
			{
				TokenInfo info = TokenInfo.FromTokenId(tokenID);

				if (info.IsKeyword)
					acceptor.AcceptKeyword(info.Text);
			}
			
			// 5. Enumerate relations
			
			TableRefBinding[] tableRefBindings = _queryScope.GetAllTableRefBindings();
			
			foreach (TableRefBinding parentTableRef in tableRefBindings)
			{
                IList<TableRelation> relations = _scope.DataContext.TableRelations.GetChildRelations(parentTableRef.TableBinding);
				
				foreach (TableRelation relation in relations)
				{
					TableBinding childTable = (relation.ParentTable == parentTableRef.TableBinding) ? relation.ChildTable : relation.ParentTable;
					TableRefBinding childTableRef = null;
					
					foreach (TableRefBinding tableRefBinding in tableRefBindings)
					{
						if (tableRefBinding.TableBinding == childTable)
						{
							childTableRef = tableRefBinding;
							break;
						}
					}
					
					if (childTableRef != null)
					{
						acceptor.AcceptRelation(parentTableRef, childTableRef, relation);
					}
				}
			}
		}
	}
}