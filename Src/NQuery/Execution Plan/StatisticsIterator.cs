using System;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class StatisticsIterator : UnaryIterator
	{
		public int OpenCount;
		public int RowCount;

		public override void Initialize()
		{
			base.Initialize();
			OpenCount = 0;
			RowCount = 0;
		}

		public override void Open()
		{
			OpenCount++;
			Input.Open();
		}

		public override bool Read()
		{
			if (Input.Read())
			{
				RowCount++;
				return true;
			}

			return false;
		}
	}
}
