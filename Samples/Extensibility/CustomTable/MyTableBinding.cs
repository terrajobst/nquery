using System;
using System.Collections;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Samples.CustomTable
{
	#region MyTableData

	public class MyTableData
	{
		private string[] _columnNames;
		private Type[] _columnTypes;
		private List<object[]> _rows = new List<object[]>();

		public string[] ColumnNames
		{
			get { return _columnNames; }
			set { _columnNames = value; }
		}

		public Type[] ColumnTypes
		{
			get { return _columnTypes; }
			set { _columnTypes = value; }
		}

		public List<object[]> Rows
		{
			get { return _rows; }
			set { _rows = value; }
		}
	}

	#endregion

	#region MyColumnBinding

	public class MyColumnBinding : ColumnBinding
	{
		private string _name;
		private Type _dataType;
		private int _columnIndex;

		public MyColumnBinding(MyTableBinding table, int columnIndex)
			: base(table)
		{
			_name = table.TableData.ColumnNames[columnIndex];
			_dataType = table.TableData.ColumnTypes[columnIndex];
			_columnIndex = columnIndex;
		}

		public override object GetValue(object row)
		{
			// This method is called by NQuery to produce the column's
			// value in the given row.

			object[] rowObject = (object[]) row;
			return rowObject[_columnIndex];
		}

		public override Type DataType
		{
			// Here we must return data type of the column we represent.
			get { return _dataType; }
		}

		public override string Name
		{
			// Here we must return name of the column we represent.
			get { return _name; }
		}
	}

	#endregion

	#region MyTableBinding

	public class MyTableBinding : TableBinding
	{
		private string _tableName;
		private MyTableData _tableData;

		public MyTableBinding(string tableName, MyTableData tableData)
		{
			_tableName = tableName;
			_tableData = tableData;
		}

		// This property is needed by our column binding MyColumnBinding
		// to access the table's metadata.
		public MyTableData TableData
		{
			get { return _tableData; }
		}

		protected override IList<ColumnBinding> BuildColumns()
		{
			// This method is called to build a list of all columns. NQuery will
			// this method only once so there is no need to cache the result.

			List<ColumnBinding> result = new List<ColumnBinding>();
			for (int i = 0; i < _tableData.ColumnNames.Length; i++)
			{
				MyColumnBinding myColumnBinding = new MyColumnBinding(this, i);
				result.Add(myColumnBinding);
			}
			return result;
		}

		public override IEnumerable GetRows(ColumnRefBinding[] neededColumns)
		{
			// This method is called to produce an enumerable that represents
			// the rows.
			//
			// NQuery will pass the values of this enumerator to method
			// MyColumnBinding.GetValue() to get the value of the column 
			// in a row.
			return _tableData.Rows;
		}

		public override Type RowType
		{
			// Here we return the type of the row object. This value must match
			// the type of the objects returned by GetRows() and is typically the 
			// type the parameter row MyColumnBinding.GetValue() is casted to.
			get { return typeof(object[]); }
		}

		public override string Name
		{
			// Here we return the name of the table.
			get { return _tableName; }
		}
	}

	#endregion
}
