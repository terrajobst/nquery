using System;
using System.Collections.Generic;

using NQuery.Compilation;
using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class QueryScope
	{
		private QueryScope _parentScope;
		private List<CommonTableBinding> _commonTables = new List<CommonTableBinding>();
		private List<TableRefBinding> _tableRefs = new List<TableRefBinding>();
		private List<ColumnRefBinding> _columnRefs = new List<ColumnRefBinding>();
		private List<ColumnRefBinding> _rowColumnRefs = new List<ColumnRefBinding>();

		public QueryScope(QueryScope parentScope)
		{
			_parentScope = parentScope;
		}

		public QueryScope ParentScope
		{
			get { return _parentScope; }
		}

		private static T[] FindBindingsByName<T>(IEnumerable<T> bindingList, Identifier identifier) where T : Binding
		{
			List<T> result = new List<T>();

			foreach (T binding in bindingList)
			{
				if (identifier.Matches(binding.Name))
					result.Add(binding);
			}

			return result.ToArray();
		}

		public CommonTableBinding DeclareCommonTableExpression(Identifier identifier, QueryNode anchorMember)
		{
			CommonTableBinding derivedTableBinding = new CommonTableBinding(identifier.Text, anchorMember);
			_commonTables.Add(derivedTableBinding);
			return derivedTableBinding;
		}

		public CommonTableBinding[] FindCommonTable(Identifier identifier)
		{
			return FindBindingsByName(_commonTables, identifier);
		}

		public TableRefBinding DeclareTableRef(TableBinding tableBinding, Identifier identifier)
		{
			TableRefBinding tableRefBinding = new TableRefBinding(this, tableBinding, identifier.Text);
			_tableRefs.Add(tableRefBinding);

			foreach (ColumnRefBinding columnRefBinding in tableRefBinding.ColumnRefs)
			{
				if (columnRefBinding.ColumnBinding is RowColumnBinding)
					_rowColumnRefs.Add(columnRefBinding);
				else
					_columnRefs.Add(columnRefBinding);
			}

			return tableRefBinding;
		}

		public TableRefBinding[] FindTableRef(Identifier identifier)
		{
			return FindBindingsByName(_tableRefs, identifier);
		}

		public TableRefBinding[] GetAllTableRefBindings()
		{
			return _tableRefs.ToArray();
		}

		public ColumnRefBinding DeclareRowColumnRef(TableRefBinding tableRefBinding)
		{
			foreach (ColumnRefBinding existingColumnRefBinding in _rowColumnRefs)
			{
				if (existingColumnRefBinding.TableRefBinding == tableRefBinding)
					return existingColumnRefBinding;
			}

			return null;
		}

		public ColumnRefBinding[] FindColumnRef(Identifier identifier)
		{
			return FindBindingsByName(_columnRefs, identifier);
		}

		internal ColumnRefBinding[] FindColumnRef(TableRefBinding table, Identifier identifier)
		{
			List<ColumnRefBinding> result = new List<ColumnRefBinding>();

			foreach (ColumnRefBinding columnBinding in _columnRefs)
			{
				if (columnBinding.TableRefBinding == table && identifier.Matches(columnBinding.Name))
					result.Add(columnBinding);
			}

			return result.ToArray();
		}

		public ColumnRefBinding GetColumnRef(TableRefBinding table, ColumnBinding columnBinding)
		{
			foreach (ColumnRefBinding columnRefBinding in _columnRefs)
			{
				if (columnRefBinding.TableRefBinding == table && columnRefBinding.ColumnBinding == columnBinding)
					return columnRefBinding;
			}

			return null;
		}

		public ColumnRefBinding[] GetAllColumnRefBindings()
		{
			return _columnRefs.ToArray();
		}
	}
}
