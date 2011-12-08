using System;

namespace NQuery.Runtime
{
	public sealed class MinMaxAggregator : IAggregator
	{
		private Type _inputType;
		private bool _isMin;
		private IComparable _currentMaxValue;

		public MinMaxAggregator(Type inputType, bool isMin)
		{
			if (inputType == null)
				throw ExceptionBuilder.ArgumentNull("inputType");

			_inputType = inputType;
			_isMin = isMin;
		}

		public Type ReturnType
		{
			get { return _inputType; }
		}

		public void Init()
		{
			_currentMaxValue = null;
		}

		public void Accumulate(object value)
		{
			IComparable comparable = value as IComparable;

			if (comparable != null)
			{
				if (_currentMaxValue == null)
				{
					_currentMaxValue = comparable;
				}
				else
				{
					int result = _currentMaxValue.CompareTo(comparable);

					if (_isMin)
					{
						if (result > 0)
							_currentMaxValue = comparable;
					}
					else
					{
						if (result < 0)
							_currentMaxValue = comparable;
					}
				}
			}
		}

		public object Terminate()
		{
			return _currentMaxValue;
		}
	}
}