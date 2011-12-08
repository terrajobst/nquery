using System;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class LeftAntiSemiNestedLoopsIterator : NestedLoopsIterator
	{
		public override bool Read()
		{
			while (true)
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

				if (!Right.Read())
				{
					_advanceOuter = true;
					WriteLeftAndRightToRowBuffer();
					return true;
				}

				// Check predicate.
				bool matchingRowFound = CheckPredicate();

				if (matchingRowFound)
					_advanceOuter = true;
			}
		}
	}
}