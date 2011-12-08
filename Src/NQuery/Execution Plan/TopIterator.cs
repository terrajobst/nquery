using System;

namespace NQuery.Runtime.ExecutionPlan
{
	internal class TopIterator : UnaryIterator
	{
		public int Limit;

		private int _rowCount;

		public TopIterator()
		{
		}

		public override void Open()
		{
			Input.Open();
			_rowCount = 0;
		}

		public override bool Read()
		{
			if (!Input.Read())
				return false;

			if (_rowCount == Limit)
				return false;

			WriteInputToRowBuffer();
			_rowCount++;
			return true;
		}
	}
}