using System;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class ProbedSemiJoinNestedLoopsIterator : NestedLoopsIterator
	{
		public RuntimeValueOutput ProbeOutput;

		public override bool Read()
		{
			RowBuffer[ProbeOutput.TargetIndex] = false;
			bool matchingRowFound = false;

			while (!matchingRowFound)
			{
				if (_advanceOuter)
				{
					_advanceOuter = false;

					if (!Left.Read())
						return false;

					if (CheckPassthruPredicate())
					{
						_advanceOuter = true;
						WriteLeftAndRightToRowBuffer();
						return true;
					}

					Right.Open();
				}

				if (_bof)
				{
					_bof = false;
					_advanceOuter = true;
					continue;
				}

				// If the inner is eof, reset the inner and advance both cursors.
				if (!Right.Read())
				{
					_advanceOuter = true;
					// We found no matching row. However, since this a probing iterator
					// we must return this row as well.
					WriteLeftAndRightToRowBuffer();
					return true;
				}

				// Check predicate.
				matchingRowFound = CheckPredicate();

				if (matchingRowFound)
					_advanceOuter = true;
			}

			RowBuffer[ProbeOutput.TargetIndex] = true;
			WriteLeftAndRightToRowBuffer();
			return true;
		}
	}
}