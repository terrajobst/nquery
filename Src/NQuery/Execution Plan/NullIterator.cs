using System;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class NullIterator : Iterator
	{
		public override void Open()
		{
		}

		public override bool Read()
		{
			return false;
		}
	}
}