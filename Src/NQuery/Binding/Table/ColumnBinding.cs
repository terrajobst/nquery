using System;

namespace NQuery.Runtime
{
	public abstract class ColumnBinding : Binding
	{
		private TableBinding _table;

		protected ColumnBinding(TableBinding table)
		{
			_table = table;
		}

		public TableBinding Table
		{
			get { return _table; }
		}

		public override string GetFullName()
		{
			return _table.Name + "." + Name;
		}

		public sealed override BindingCategory Category
		{
			get { return BindingCategory.Column; }
		}

		public abstract Type DataType { get; }
		public abstract object GetValue(object row);
	}
}