using System;
using System.Collections;
using System.Globalization;

using NQuery.Compilation;

namespace NQuery.Runtime.ExecutionPlan
{
	// TODO: Implement Right Semi Join, Right Anti Semi Join

	internal sealed class HashMatchIterator : BinaryIterator
	{
		private sealed class Entry
		{
			public object[] RowValues;
			public Entry Next;
			public bool Matched;
		}

		private enum Phase
		{
			ProduceMatch,
			ReturnUnmatchedRowsFromBuildInput
		}

		public IteratorInput BuildKeyEntry;
		public IteratorInput ProbeEntry;
		public RuntimeExpression ProbeResidual;
		public JoinType LogicalOp;

		private Hashtable _hashTable;
		private object[] _keys;
		private Entry _entry;
		private int _currentKeyIndex;
		private Phase _currentPhase;
		private bool _rightMatched;

		public override void Open()
		{
			Left.Open();
			Right.Open();
			BuildHashtable();

			_entry = null;
			_currentKeyIndex = -1;
			_currentPhase = Phase.ProduceMatch;
			_rightMatched = true;
		}

		private void BuildHashtable()
		{
			_hashTable = new Hashtable();

			while (Left.Read())
			{
				object keyValue = Left.RowBuffer[BuildKeyEntry.SourceIndex];
				if (keyValue != null)
				{
					object[] rowValues = new object[Left.RowBuffer.Length];
					Array.Copy(Left.RowBuffer, rowValues, rowValues.Length);
					AddToHashtable(keyValue, rowValues);
				}
			}

			_keys = new object[_hashTable.Keys.Count];
			_hashTable.Keys.CopyTo(_keys, 0);
		}

		private void AddToHashtable(object keyValue, object[] values)
		{
			Entry entry = _hashTable[keyValue] as Entry;

			if (entry == null)
			{
				entry = new Entry();
			}
			else
			{
				Entry newEntry = new Entry();
				newEntry.Next = entry;
				entry = newEntry;
			}

			entry.RowValues = values;
			_hashTable[keyValue] = entry;
		}

		private bool CheckIfProbeResidualIsTrue()
		{
			if (ProbeResidual == null)
				return true;

			object result = ProbeResidual.GetValue();
			return (result != null && Convert.ToBoolean(result, CultureInfo.InvariantCulture));
		}

		public override bool Read()
		{
			switch (_currentPhase)
			{
				case Phase.ProduceMatch:
				{
					bool matchFound = false;

					while (!matchFound)
					{
						if (_entry != null)
							_entry = _entry.Next;

						if (_entry == null)
						{
							// All rows having the same key value are exhausted.

							if (!_rightMatched && (LogicalOp == JoinType.FullOuter || LogicalOp == JoinType.RightOuter))
							{
								_rightMatched = true;
								WriteRightWithNullLeftToRowBuffer();
								return true;
							}

							// Read next row from probe input.

							if (!Right.Read())
							{
								// The probe input is exhausted. If we have a full outer or left outer
								// join we are not finished. We have to return all rows from the build
								// input that have not been matched with the probe input.

								if (LogicalOp == JoinType.FullOuter || LogicalOp == JoinType.LeftOuter)
								{
									_currentPhase = Phase.ReturnUnmatchedRowsFromBuildInput;
									_entry = null;
									goto case Phase.ReturnUnmatchedRowsFromBuildInput;
								}

								return false;
							}

							// Get probe value

							_rightMatched = false;
							object probeValue = Right.RowBuffer[ProbeEntry.SourceIndex];

							// Seek first occurence of probe value

							if (probeValue != null)
								_entry = (Entry) _hashTable[probeValue];
						}

						if (_entry != null)
						{
							Array.Copy(_entry.RowValues, Left.RowBuffer, Left.RowBuffer.Length);

							if (CheckIfProbeResidualIsTrue())
							{
								_entry.Matched = true;
								WriteLeftAndRightToRowBuffer();
								matchFound = true;
								_rightMatched = true;
							}
						}
					}

					return true;
				}

				case Phase.ReturnUnmatchedRowsFromBuildInput:
				{
					bool unmatchedFound = false;

					while (!unmatchedFound)
					{
						if (_entry != null)
							_entry = _entry.Next;

						if (_entry == null)
						{
							// All rows having the same key value are exhausted.
							// Read next key from build input.

							_currentKeyIndex++;

							if (_currentKeyIndex >= _keys.Length)
							{
								// We have read all keys. So we are finished.
								return false;
							}

							_entry = (Entry)_hashTable[_keys[_currentKeyIndex]];
						}

						unmatchedFound = !_entry.Matched;
					}

					Array.Copy(_entry.RowValues, Left.RowBuffer, Left.RowBuffer.Length);
					WriteLeftWithNullRightToRowBuffer();
					return true;
				}

				default:
					throw ExceptionBuilder.UnhandledCaseLabel(_currentPhase);
			}
		}
	}
}