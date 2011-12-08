using System;
using System.Collections;
using System.Collections.Generic;

namespace NQuery.Runtime 
{
	public abstract class TableBinding : Binding
	{
		private ColumnBindingCollection _columns;

		protected TableBinding()
		{
		}

		public ColumnBindingCollection Columns
		{
			get
			{
				if (_columns == null)
					_columns = new ColumnBindingCollection(BuildColumns());

				return _columns;
			}
		}

		public ColumnBinding GetColumn(string name)
		{
			foreach (ColumnBinding columnBinding in Columns)
			{
				if (columnBinding.Name == name)
					return columnBinding;
			}
			
			return null;
		}

		protected abstract IList<ColumnBinding> BuildColumns();
		public abstract Type RowType { get; }
		public abstract IEnumerable GetRows(ColumnRefBinding[] neededColumns);

		public sealed override BindingCategory Category
		{
			get { return BindingCategory.Table; }
		}		
	}
}