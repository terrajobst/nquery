using System;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class TableSpoolRefIterator : Iterator
	{
		public TableSpoolIterator PrimarySpool;

		public override void Open()
		{
		}

		public override bool Read()
		{
			if (PrimarySpool.TableSpool.Count == 0)
				return false;

			object[] spoolEntry = PrimarySpool.TableSpool.Pop();
			Array.Copy(spoolEntry, RowBuffer, spoolEntry.Length);
			return true;
		}
	}
}