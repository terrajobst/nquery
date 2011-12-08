using System;

namespace NQuery.Runtime
{
	public sealed class FirstAggregator : IAggregator
	{
		private Type _inputType;
		private object _first;

		public FirstAggregator(Type inputType)
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
			_first = null;
		}

		public void Accumulate(object value)
		{
			if (_first == null)
				_first = value;
		}

		public object Terminate()
		{
			return _first;
		}
	}

}