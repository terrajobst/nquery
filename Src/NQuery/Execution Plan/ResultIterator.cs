using System;
using System.Data;
using System.Globalization;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class ResultIterator : UnaryIterator
	{
		public string[] ColumnNames;
		public Type[] ColumnTypes;

		public override void Open()
		{
			Input.Open();
		}

		public override bool Read()
		{
			if (!Input.Read())
				return false;

			WriteInputToRowBuffer();
			return true;
		}

		private static string GenerateUniqueTableName(DataTable dataTable, string name)
		{
			if (name == null)
				name = "Column";

			string result = name;
			int count = 0;
			while (dataTable.Columns.Contains(result))
				result = String.Concat(name, ++count);

			return result;
		}

		public DataTable CreateSchemaTable()
		{
			DataTable schemaTable = new DataTable();
			schemaTable.Locale = CultureInfo.CurrentCulture;

			for (int i = 0; i < ColumnNames.Length; i++)
			{
				string columnCaption = ColumnNames[i];
				string columnName = GenerateUniqueTableName(schemaTable, ColumnNames[i]);
				Type columnType = ColumnTypes[i];

				if (columnType == typeof(DBNull))
				{
					// Special handling for DBNull, which is an illegal storage type
					// for a DataTable column. We will treat this value as System.Object.
					columnType = typeof(object);
				}

				DataColumn dataColumn = schemaTable.Columns.Add(columnName, columnType);
				dataColumn.Caption = columnCaption;
			}

			return schemaTable;
		}
	}
}