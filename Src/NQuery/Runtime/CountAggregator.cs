using System;

namespace NQuery.Runtime
{
	public sealed class CountAggregator : IAggregator
	{
		private int _count;

		public Type ReturnType
		{
			get { return typeof (int); }
		}

		public void Init()
		{
			_count = 0;
		}

		public void Accumulate(object value)
		{
			if (value != null)
				_count++;
		}

		public object Terminate()
		{
			return _count;
		}
	}
}