using System;

using NQuery.Runtime;

namespace NQuery.UI
{
	internal class ScopeInfoBuilder : IMemberCompletionAcceptor
	{
		private ScopeInfo _scopeInfo = new ScopeInfo();

		public ScopeInfo GetScopeInfo()
		{
			return _scopeInfo;
		}
			
		public void AcceptKeyword(string keyword)
		{
			_scopeInfo.ReservedWords.Add(keyword, null);
		}

		public void AcceptAggregate(AggregateBinding aggregate)
		{
			if (!_scopeInfo.AggregateNames.ContainsKey(aggregate.Name))
				_scopeInfo.AggregateNames.Add(aggregate.Name, null);
		}

		public void AcceptFunction(FunctionBinding function)
		{
			if (!_scopeInfo.FunctionNames.ContainsKey(function.Name))
				_scopeInfo.FunctionNames.Add(function.Name, null);
		}

		public void AcceptMethod(MethodBinding method)
		{
		}

		public void AcceptTable(TableBinding table)
		{
		}

		public void AcceptTableRef(TableRefBinding tableRef)
		{
		}

		public void AcceptColumnRef(ColumnRefBinding column)
		{
		}

		public void AcceptProperty(PropertyBinding property)
		{
		}

		public void AcceptParameter(ParameterBinding parameter)
		{
		}

		public void AcceptConstant(ConstantBinding constant)
		{
		}

		public void AcceptRelation(TableRefBinding parentTableRef, TableRefBinding childTableRef, TableRelation relation)
		{
		}
	}
}