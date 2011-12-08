using System;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class TopWithTiesIterator : TopIterator
	{
		public IteratorInput[] TieEntries;
		private bool _limitReached;
		private object[] _lastTieEntryValues;

		public TopWithTiesIterator()
		{
		}

		public override void Open()
		{
			base.Open();
			_limitReached = false;
			_lastTieEntryValues = new object[TieEntries.Length];
		}

		public override bool Read()
		{
			if (_limitReached)
			{
				Input.Read();
			}
			else
			{
				if (!base.Read())
				{
					_limitReached = true;
				}
				else
				{
					for (int i = 0; i < TieEntries.Length; i++)
						_lastTieEntryValues[i] = Input.RowBuffer[TieEntries[i].SourceIndex];

					return true;
				}
			}

			// Check if the tie values of this row are equal to the one of last row.

			bool allTieValuesAreEqual = true;

			for (int i = 0; i < TieEntries.Length; i++)
			{
				object lastTieValue = _lastTieEntryValues[i];
				object thisTieValue = Input.RowBuffer[TieEntries[i].SourceIndex];

				if (lastTieValue == thisTieValue)
					continue;

				if (Equals(lastTieValue, thisTieValue))
					continue;

				allTieValuesAreEqual = false;
				break;
			}

			WriteInputToRowBuffer();
			return allTieValuesAreEqual;
		}
	}
}