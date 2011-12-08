using System;

namespace NQuery.Runtime
{
	public sealed class LastAggregator : IAggregator
	{
		private Type _inputType;
		private object _last;

		public LastAggregator(Type inputType)
		{
			if (inputType == null)
				throw ExceptionBuilder.ArgumentNull("inputType");

			_inputType = inputType;
		}

		public Type ReturnType
		{
			get { return _inputType; }
		}

		public void Init()
		{
			_last = null;
		}

		public void Accumulate(object value)
		{
			_last = value;
		}

		public object Terminate()
		{
			return _last;
		}
	}
}