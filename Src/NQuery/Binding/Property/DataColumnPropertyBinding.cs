using System;
using System.Data;

namespace NQuery.Runtime
{
	/// <summary>
	/// Represents a property that is bound to a particular <see cref="DataColumn"/>. This class
	/// is used by <see cref="DataRowPropertyProvider"/> to represent the properties of an instance
	/// of <see cref="DataRow"/>.
	/// </summary>
    public class DataColumnPropertyBinding : PropertyBinding
	{
		private DataColumn _dataColumn;

		public DataColumnPropertyBinding(DataColumn dataColumn)
		{
			_dataColumn = dataColumn;	
		}

		public override Type DataType
		{
			get { return _dataColumn.DataType; }
		}

		public override object GetValue(object instance)
		{
			DataRow dataRow = instance as DataRow;

			if (dataRow == null)
				return null;

			return dataRow[_dataColumn];
		}

		public override string Name
		{
			get { return _dataColumn.ColumnName; }
		}
	}
}