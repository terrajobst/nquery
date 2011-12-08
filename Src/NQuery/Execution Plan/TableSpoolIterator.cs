using System;
using System.Collections.Generic;

using NQuery.Runtime.ExecutionPlan;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class TableSpoolIterator : UnaryIterator
	{
		public Stack<object[]> TableSpool = new Stack<object[]>();

		public override void Open()
		{
			Input.Open();
		}

		public override bool Read()
		{
			object[] spoolEntry;

			if (!Input.Read())
				return false;

			// Read row from input and store it.
			spoolEntry = new object[Input.RowBuffer.Length];
			Array.Copy(Input.RowBuffer, spoolEntry, spoolEntry.Length);
			TableSpool.Push(spoolEntry);

			WriteInputToRowBuffer();
			return true;
		}
	}
}
