using System;
using System.Globalization;

namespace NQuery.Runtime
{
	public sealed class VarAndStdDevAggregator : IAggregator
	{
		private bool _isVar;
		private decimal _sum;
		private decimal _sumOfSquares;
		private int _count;
		
		public VarAndStdDevAggregator(bool isVar)
		{
			_isVar = isVar;
		}
		
		public void Init()
		{
			_sum = 0;
			_sumOfSquares = 0;
			_count = 0;
		}

		public void Accumulate(object value)
		{
			decimal valueAsDecimal = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
			_sum += valueAsDecimal;
			_sumOfSquares += valueAsDecimal * valueAsDecimal;
			_count++;
		}

		public object Terminate()
		{
			if (_count < 2)
				return null;
			
			decimal e = _sum / _count;
			decimal r = (_sumOfSquares - e * (2 * _sum - e * _count)) / (_count - 1);
			
			if (_isVar)
				return r;
			
			return Math.Sqrt((double) r);
		}

		public Type ReturnType
		{
			get
			{
				if (_isVar)
					return typeof(decimal);
				
				return typeof(double);
			}
		}
	}
}