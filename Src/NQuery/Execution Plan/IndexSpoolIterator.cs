using System;
using System.Collections.Generic;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class IndexSpoolIterator : UnaryIterator
	{
		public IteratorInput IndexEntry;
		public RuntimeExpression ProbeExpression;

		private Dictionary<object, List<object[]>> _indexSpool;
		private List<object[]> _spoolEntries;
		private int _spoolEntryIndex;

		public override void Open()
		{
			if (_indexSpool == null)
			{
				Input.Open();

				_indexSpool = new Dictionary<object, List<object[]>>();

				while (Input.Read())
				{
					object indexValue = Input.RowBuffer[IndexEntry.SourceIndex];
					if (indexValue != null)
					{
						List<object[]> spoolEntries;
						if (!_indexSpool.TryGetValue(indexValue, out spoolEntries))
						{
							spoolEntries = new List<object[]>();
							_indexSpool.Add(indexValue, spoolEntries);
						}

						object[] spoolEntry = new object[InputOutput.Length];
						for (int i = 0; i < InputOutput.Length; i++)
							spoolEntry[i] = Input.RowBuffer[InputOutput[i].SourceIndex];

						spoolEntries.Add(spoolEntry);
					}
				}
			}

			_spoolEntries = null;
			_spoolEntryIndex = 0;

			object probeValue = ProbeExpression.GetValue();
			if (probeValue != null)
				_indexSpool.TryGetValue(probeValue, out _spoolEntries);
		}

		public override bool Read()
		{
			if (_spoolEntries == null || _spoolEntryIndex >= _spoolEntries.Count)
				return false;

			object[] spoolEntry = _spoolEntries[_spoolEntryIndex];
			Array.Copy(spoolEntry, RowBuffer, spoolEntry.Length);
			_spoolEntryIndex++;
			return true;
		}
	}
}