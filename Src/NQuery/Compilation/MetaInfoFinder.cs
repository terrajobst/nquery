using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal class MetaInfoFinder : StandardVisitor
	{
		private List<TableRefBinding> _tableBindingList = new List<TableRefBinding>();
		private List<ColumnRefBinding> _columnBindingList = new List<ColumnRefBinding>();
		private Stack<QueryScope> _queryScopeStack = new Stack<QueryScope>();
		private int _nestingLevel;
        private bool _containsSingleRowSubselects;
        private bool _containsExistenceSubselects;

		public MetaInfoFinder()
		{
		}

		public MetaInfo GetMetaInfo()
	    {
            MetaInfo result = new MetaInfo(
                _columnBindingList.ToArray(),
                _tableBindingList.ToArray(),
                _containsSingleRowSubselects,
				_containsExistenceSubselects);
            return result;
	    }

		#region Helpers

		private bool IsOuterScope(QueryScope scope)
		{
            return !_queryScopeStack.Contains(scope);
		}

		private void AddTable(TableRefBinding table)
		{
			if (IsOuterScope(table.Scope) && !_tableBindingList.Contains(table))
				_tableBindingList.Add(table);
		}

		private void AddColumn(ColumnRefBinding column)
		{
			if (IsOuterScope(column.Scope) && !_columnBindingList.Contains(column))
				_columnBindingList.Add(column);
		}

		private void VisitQueryExpression(QueryNode query)
		{
			_nestingLevel++;
			base.Visit(query);
			_nestingLevel--;
		}

		#endregion
		
		public override ExpressionNode VisitColumnExpression(ColumnExpression expression)
		{
			AddTable(expression.Column.TableRefBinding);
			AddColumn(expression.Column);

			return expression;
		}

		public override TableReference VisitNamedTableReference(NamedTableReference node)
		{
			AddTable(node.TableRefBinding);
			return node;
		}

		public override QueryNode VisitSelectQuery(SelectQuery query)
		{
			if (_nestingLevel == 0)
				return base.VisitSelectQuery(query);
			
			if (_nestingLevel > 0)
			{
                _queryScopeStack.Push(query.QueryScope);
				base.VisitSelectQuery(query);
                _queryScopeStack.Pop();
			}
			
			return query;
		}

		public override ExpressionNode VisitSingleRowSubselect(SingleRowSubselect expression)
		{
		    _containsSingleRowSubselects = true;
			VisitQueryExpression(expression.Query);
			return expression;
		}

		public override ExpressionNode VisitExistsSubselect(ExistsSubselect expression)
		{
            _containsExistenceSubselects = true;
            VisitQueryExpression(expression.Query);
			return expression;
		}

		public override ExpressionNode VisitAllAnySubselect(AllAnySubselect expression)
		{
            _containsExistenceSubselects = true;
            VisitQueryExpression(expression.Query);
			return expression;
		}
	}
}