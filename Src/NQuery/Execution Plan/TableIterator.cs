using System;
using System.Collections;

using NQuery.Runtime;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class TableIterator : Iterator
	{
		public TableBinding Table;
		public RuntimeColumnValueOutput[] DefinedValues;

		private IEnumerator _rows;

		public override void Open()
		{
			_rows = Table.GetRows(new ColumnRefBinding[0]).GetEnumerator();
		}

		public override bool Read()
		{
			if (!_rows.MoveNext())
				return false;

			foreach (RuntimeColumnValueOutput definedValue in DefinedValues)
			{
				ColumnBinding columnBinding = definedValue.ColumnBinding;

				object value;

				try
				{
					value = columnBinding.GetValue(_rows.Current);
				}
				catch (NQueryException)
				{
					throw;
				}
				catch (Exception ex)
				{
					throw ExceptionBuilder.ColumnBindingGetValueFailed(ex);
				}

				if (NullHelper.IsNull(value))
					RowBuffer[definedValue.TargetIndex] = null;
				else
					RowBuffer[definedValue.TargetIndex] = value;
			}

			return true;
		}
	}
}