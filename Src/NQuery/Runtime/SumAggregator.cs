using System;

namespace NQuery.Runtime
{
	public sealed class SumAggregator : IAggregator
	{
		private Expression<object> _addExpression;
		private Type _returnType;
		private ParameterBinding _leftParameter;
		private ParameterBinding _rightParameter;
		private Expression<object> _convertInputToSumExpression;
		private object _sum;

		public SumAggregator(Expression<object> addExpression, ParameterBinding leftParameter, ParameterBinding rightParameter, Expression<object> convertInputToSumExpression)
		{
			if (addExpression == null)
				throw ExceptionBuilder.ArgumentNull("addExpr");

			if (leftParameter == null)
				throw ExceptionBuilder.ArgumentNull("leftParam");

			if (rightParameter == null)
				throw ExceptionBuilder.ArgumentNull("rightParam");

			if (convertInputToSumExpression == null)
				throw ExceptionBuilder.ArgumentNull("convertInputToSumExpr)");

			_addExpression = addExpression;
			_returnType = addExpression.Resolve();
			_leftParameter = leftParameter;
			_rightParameter = rightParameter;
			_convertInputToSumExpression = convertInputToSumExpression;
		}

		public Type ReturnType
		{
			get { return _returnType; }
		}

		public void Init()
		{
			_sum = null;
		}

		public void Accumulate(object value)
		{
			if (value != null)
			{
				if (_sum == null)
				{
					_convertInputToSumExpression.Parameters[0].Value = value;
					_sum = _convertInputToSumExpression.Evaluate();
				}
				else
				{
					_leftParameter.Value = _sum;
					_rightParameter.Value = value;
					_sum = _addExpression.Evaluate();
				}
			}
		}

		public object Terminate()
		{
			return _sum;
		}
	}
}