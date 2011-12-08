using System;

namespace NQuery.Runtime
{
	/// <summary>
	/// This special class is used to represent a reference to a row itself.
	/// </summary>
	internal sealed class RowColumnBinding : ColumnBinding
	{
		// The name will only be shown in a few cases. However, we need a unique name. So we
		// use a naming convention similar to SQL Server where the special row columns
		// $ROWGUID and $IDENTITY exists.
		public const string COLUMN_NAME = "$ROW";
		
		public RowColumnBinding(TableBinding table) : base(table)
		{
		}

		public override Type DataType
		{
			get { return Table.RowType; }
		}

		public override object GetValue(object row)
		{
			// Since this pseudo column represents the row we can
			// return the row directly.
			return row;
		}

		public override string Name
		{
			get { return COLUMN_NAME; }
		}
	}
}