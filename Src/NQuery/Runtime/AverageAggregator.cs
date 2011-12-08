using System;

namespace NQuery.Runtime
{
	public sealed class AverageAggregator : IAggregator
	{
		private IAggregator _sumAggregator;
		private Expression<object> _avgExpression;
		private Type _returnType;
		private ParameterBinding _sumParameter;
		private ParameterBinding _countParameter;
		private int _count;

		public AverageAggregator(IAggregator sumAggregator, Expression<object> avgExpression, ParameterBinding sumParameter, ParameterBinding countParameter)
		{
			if (sumAggregator == null)
				throw ExceptionBuilder.ArgumentNull("sumAggregator");

			if (avgExpression == null)
				throw ExceptionBuilder.ArgumentNull("avgExpression");

			if (sumParameter == null)
				throw ExceptionBuilder.ArgumentNull("sumParam");

			if (countParameter == null)
				throw ExceptionBuilder.ArgumentNull("countParam");

			_sumAggregator = sumAggregator;
			_avgExpression = avgExpression;
			_returnType = avgExpression.Resolve();
			_sumParameter = sumParameter;
			_countParameter = countParameter;
		}

		public Type ReturnType
		{
			get { return _returnType; }
		}

		public void Init()
		{
			_sumAggregator.Init();
			_count = 0;
		}

		public void Accumulate(object value)
		{
			if (value != null)
			{
				_count++;
				_sumAggregator.Accumulate(value);
			}
		}

		public object Terminate()
		{
			if (_count == 0)
				return null;

			_sumParameter.Value = _sumAggregator.Terminate();
			_countParameter.Value = _count;
			return _avgExpression.Evaluate();
		}
	}
}