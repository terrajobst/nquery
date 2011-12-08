using System;
using System.Data;

namespace NQuery.Runtime
{
	public class DataColumnBinding : ColumnBinding
	{
		private DataColumn _dataColumn;

		public DataColumnBinding(DataTableBinding table, DataColumn dataColumn)
			: base(table)
		{
			if (dataColumn == null)
				throw ExceptionBuilder.ArgumentNull("dataColumn");

			_dataColumn = dataColumn;
		}

		public override string Name
		{
			get { return _dataColumn.ColumnName; }
		}

		public override Type DataType
		{
			get { return _dataColumn.DataType; }
		}

		public override object GetValue(object row)
		{
			DataRow dataRow = (DataRow) row;
			return dataRow[_dataColumn, DataRowVersion.Default];
		}
	}
}