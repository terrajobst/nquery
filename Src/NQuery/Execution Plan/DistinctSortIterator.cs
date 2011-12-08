using System;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class DistinctSortIterator : SortIterator
	{
		private object[] _lastSpooledRow;

		public DistinctSortIterator()
		{
		}

		public override void Open()
		{
			base.Open();
			_lastSpooledRow = null;
		}

		public override bool Read()
		{
			if (_lastSpooledRow == null)
			{
				if (!base.Read())
					return false;

				_lastSpooledRow = CurrentSpooledRow;
				return true;
			}

			bool atLeastOneRecordFound = false;

			while (true)
			{
				if (!base.Read())
					break;

				foreach (IteratorInput sortEntry in SortEntries)
				{
					object valueOfLastRow = _lastSpooledRow[sortEntry.SourceIndex];
					object valueOfThisRow = CurrentSpooledRow[sortEntry.SourceIndex];

					if (valueOfLastRow == valueOfThisRow)
						continue;

					if (Equals(valueOfLastRow, valueOfThisRow))
						continue;

					atLeastOneRecordFound = true;
					break;
				}

				if (atLeastOneRecordFound)
					break;
			}

			_lastSpooledRow = CurrentSpooledRow;
			return atLeastOneRecordFound;
		}
	}
}