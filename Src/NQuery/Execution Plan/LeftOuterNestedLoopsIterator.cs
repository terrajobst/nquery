using System;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class LeftOuterNestedLoopsIterator : NestedLoopsIterator
	{
		public override bool Read()
		{
			bool matchingRowFound = false;

			while (!matchingRowFound)
			{
				if (_advanceOuter)
				{
					_advanceOuter = false;
					_outerRowHadMatchingInnerRow = false;

					if (!Left.Read())
						return false;

					if (CheckPassthruPredicate())
					{
						WriteLeftWithNullRightToRowBuffer();
						_advanceOuter = true;
						return true;
					}

					Right.Open();
				}

				// If we are bof or the inner is eof, reset the inner and
				// advance both cursors.
				if (_bof || !Right.Read())
				{
					bool shouldReturnRow = !_bof && !_outerRowHadMatchingInnerRow;

					_bof = false;
					_advanceOuter = true;

					if (shouldReturnRow)
					{
						// We haven't returned the outer row yet since we couldn't find any matching inner 
						// row. Set the values of the inner row to null and return the combined row.
						WriteLeftWithNullRightToRowBuffer();
						return true;
					}

					continue;
				}

				matchingRowFound = CheckPredicate();
			}

			WriteLeftAndRightToRowBuffer();
			_outerRowHadMatchingInnerRow = true;
			return true;			
		}
	}
}