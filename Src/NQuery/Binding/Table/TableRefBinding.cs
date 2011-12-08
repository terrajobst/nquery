using System;
using System.Collections.Generic;

using NQuery.Compilation;

namespace NQuery.Runtime
{
	public sealed class TableRefBinding : Binding 
	{
		private QueryScope _scope;
		private string _name;
		private TableBinding _definition;
		private ColumnRefBinding[] _columnRefs;

		internal TableRefBinding(QueryScope queryScope, TableBinding tableBinding, string name)
		{
			_scope = queryScope;
			_name = name;
			_definition = tableBinding;
			CreateColumnRefs();
		}

		internal QueryScope Scope
		{
			get { return _scope; }
		}

		public override string Name
		{
			get { return _name; }
		}

		public override BindingCategory Category
		{
			get { return BindingCategory.TableRef; }
		}

		public TableBinding TableBinding
		{
			get { return _definition; }
		}

		private void CreateColumnRefs()
		{
			List<ColumnRefBinding> tableColumnRefs = new List<ColumnRefBinding>();

			if (!(_definition is DerivedTableBinding) && !(_definition is CommonTableBinding))
			{
				// Create special row column ref

				ColumnRefBinding rowColumnRefBinding = CreateRowColumnRefBinding(this);
				tableColumnRefs.Add(rowColumnRefBinding);
			}

			// Create all column refs.

			foreach (ColumnBinding columnDefinition in _definition.Columns)
			{
				ColumnRefBinding columnRefBinding = new ColumnRefBinding(this, columnDefinition);

				RowBufferEntry rowBufferEntry = new RowBufferEntry(columnRefBinding.ColumnBinding.DataType);
				rowBufferEntry.Name = columnRefBinding.GetFullName();

				ColumnValueDefinition columnValueDefinition = new ColumnValueDefinition();
				columnValueDefinition.Target = rowBufferEntry;
				columnValueDefinition.ColumnRefBinding = columnRefBinding;

				columnRefBinding.ValueDefinition = columnValueDefinition;
				tableColumnRefs.Add(columnRefBinding);
			}

			// Assign column refs to table ref.

			_columnRefs = tableColumnRefs.ToArray();
		}

		private static ColumnRefBinding CreateRowColumnRefBinding(TableRefBinding tableRefBinding)
		{
			RowColumnBinding rowColumnBinding = new RowColumnBinding(tableRefBinding.TableBinding);
			ColumnRefBinding rowColumnRefBinding = new ColumnRefBinding(tableRefBinding, rowColumnBinding);

			RowBufferEntry rowColumnBufferEntry = new RowBufferEntry(rowColumnRefBinding.ColumnBinding.DataType);
			rowColumnBufferEntry.Name = rowColumnRefBinding.TableRefBinding.Name;

			ColumnValueDefinition rowColumnValueDefinition = new ColumnValueDefinition();
			rowColumnValueDefinition.Target = rowColumnBufferEntry;
			rowColumnValueDefinition.ColumnRefBinding = rowColumnRefBinding;

			rowColumnRefBinding.ValueDefinition = rowColumnValueDefinition;
			return rowColumnRefBinding;
		}

		internal ColumnRefBinding[] ColumnRefs
		{
			get { return _columnRefs; }
		}
	}
}