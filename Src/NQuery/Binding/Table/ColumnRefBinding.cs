using System;

using NQuery.Compilation;

namespace NQuery.Runtime
{
	public sealed class ColumnRefBinding : Binding
	{
		private TableRefBinding _tableRefBinding;
		private ColumnBinding _columnBinding;
		private ColumnValueDefinition _valueDefinition;

		public ColumnRefBinding(TableRefBinding tableRefBinding, ColumnBinding columnBinding)
		{
			_tableRefBinding = tableRefBinding;
			_columnBinding = columnBinding;
		}

		internal QueryScope Scope
		{
			get { return _tableRefBinding.Scope; }
		}

		public override string Name
		{
			get { return _columnBinding.Name; }
		}

		public override string GetFullName()
		{
			if (_columnBinding is RowColumnBinding)
				return _tableRefBinding.Name;
			
			return _tableRefBinding.Name + "." + _columnBinding.Name;
		}

		public override BindingCategory Category
		{
			get { return BindingCategory.ColumnRef; }
		}

		public TableRefBinding TableRefBinding
		{
			get { return _tableRefBinding; }
		}

		public ColumnBinding ColumnBinding
		{
			get { return _columnBinding; }
		}

		internal ColumnValueDefinition ValueDefinition
		{
			get { return _valueDefinition; }
			set { _valueDefinition = value; }
		}
	}
}