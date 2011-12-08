using System;
using System.Collections;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class DerivedTableBinding : TableBinding
	{
		private string _name;
		private QueryNode _query;

		public DerivedTableBinding(string name, QueryNode query)
		{
			_name = name;
			_query = query;
		}

		protected override IList<ColumnBinding> BuildColumns()
		{
			List<ColumnBinding> result = new List<ColumnBinding>();
			foreach (SelectColumn selectColumn in _query.GetColumns())
			{
				DerivedColumnBinding derivedColumnBinding = new DerivedColumnBinding(this, selectColumn.Alias.Text, selectColumn.Expression.ExpressionType);
				result.Add(derivedColumnBinding);
			}
			return result;
		}

		public override Type RowType
		{
			get { throw new NotImplementedException(); }
		}

		public override IEnumerable GetRows(ColumnRefBinding[] neededColumns)
		{
			throw new NotImplementedException();
		}

		public override string Name
		{
			get { return _name; }
		}
	}
}