using System;

namespace NQuery.Runtime.ExecutionPlan
{
	internal abstract class Iterator
	{
		public virtual void Initialize()
		{
		}

		public abstract void Open();
		public abstract bool Read();

		public object[] RowBuffer;
	}
}