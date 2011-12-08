using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace NQuery.Runtime
{
	public class DataTableBinding : TableBinding
	{
		private DataTable _dataTable;

		public DataTableBinding(DataTable dataTable)
		{
			_dataTable = dataTable;
		}

		public DataTable DataTable
		{
			get { return _dataTable; }
		}

		public override string Name
		{
			get { return _dataTable.TableName; }
		}

		protected override IList<ColumnBinding> BuildColumns()
		{
			List<ColumnBinding> columnList = new List<ColumnBinding>();

			foreach (DataColumn col in _dataTable.Columns)
				columnList.Add(new DataColumnBinding(this, col));

			return columnList;
		}

		public override Type RowType
		{
			get { return typeof(DataRow); }
		}

		public override IEnumerable GetRows(ColumnRefBinding[] neededColumns)
		{
			foreach (DataRow dataRow in _dataTable.Rows)
			{
				if (dataRow.RowState != DataRowState.Deleted)
					yield return dataRow;
			}
		}
	}
}