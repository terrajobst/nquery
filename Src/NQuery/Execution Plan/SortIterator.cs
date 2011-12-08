using System;
using System.Collections;
using System.Collections.Generic;

using NQuery.Compilation;

namespace NQuery.Runtime.ExecutionPlan
{
	internal class SortIterator : UnaryIterator
	{
		#region Comparer

		internal sealed class RowComparer : IComparer<object[]>
		{
			private IteratorInput[] _sortEntries;
			private SortOrder[] _sortOrders;
			private IComparer[] _comparers;

			public RowComparer(IteratorInput[] sortEntries, SortOrder[] sortOrders, IComparer[] comparers)
			{
				_sortEntries = sortEntries;
				_sortOrders = sortOrders;
				_comparers = comparers;
			}

			public int Compare(object[] x, object[] y)
			{
				if (x == null && y != null)
					return -1;

				if (x != null && y == null)
					return +1;

				if (x == null && y == null)
					return 0;

				// Compare all columns

				int result = 0;
				int index = 0;
				while (index < _sortEntries.Length && result == 0)
				{
					int sign = (_sortOrders[index] == SortOrder.Ascending) ? 1 : -1;

					int valueIndex = _sortEntries[index].SourceIndex;

					object value1 = x[valueIndex];
					object value2 = y[valueIndex];

					if (value1 == null && value2 != null)
						return -1 * sign;

					if (value1 != null && value2 == null)
						return +1 * sign;

					if (value1 != null && value2 != null)
					{
						result = _comparers[index].Compare(value1, value2) * sign;

						if (result != 0)
							return result;
					}

					index++;
				}

				return 0;
			}
		}
		#endregion

		public IteratorInput[] SortEntries;
		public SortOrder[] SortOrders;
		public IComparer[] Comparers;
		protected object[] CurrentSpooledRow;

		private List<object[]> _spooledRowsList;
		private int _spooledRowIndex;

		public SortIterator()
		{
		}

		private void SortInput()
		{
			_spooledRowsList = new List<object[]>();

			while (Input.Read())
			{
				object[] rowValues = new object[Input.RowBuffer.Length];
				Input.RowBuffer.CopyTo(rowValues, 0);
				_spooledRowsList.Add(rowValues);
			}

			int[] sortedColumnIndexes = new int[SortEntries.Length];
			for (int i = 0; i < sortedColumnIndexes.Length; i++)
				sortedColumnIndexes[i] = SortEntries[i].SourceIndex;

			RowComparer rowComparer = new RowComparer(SortEntries, SortOrders, Comparers);
			_spooledRowsList.Sort(rowComparer);
		}

		public override void Open()
		{
			Input.Open();
			_spooledRowsList = null;
			_spooledRowIndex = 0;
		}

		public override bool Read()
		{
			if (_spooledRowsList == null)
				SortInput();

			if (_spooledRowIndex >= _spooledRowsList.Count)
				return false;

			CurrentSpooledRow = _spooledRowsList[_spooledRowIndex];
			foreach (IteratorOutput output in InputOutput)
				RowBuffer[output.TargetIndex] = CurrentSpooledRow[output.SourceIndex];

			_spooledRowIndex++;
			return true;
		}
	}
}