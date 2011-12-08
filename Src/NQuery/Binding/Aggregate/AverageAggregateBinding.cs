using System;

namespace NQuery.Runtime
{
	public sealed class AverageAggregateBinding : AggregateBinding
	{
		public AverageAggregateBinding(string name)
			: base(name)
		{
		}

		public override IAggregator CreateAggregator(Type inputType)
		{
			if (inputType == null)
				throw ExceptionBuilder.ArgumentNull("inputType");

			SumAggregateBinding sumAggregate = new SumAggregateBinding(String.Empty);
			IAggregator sumAggregator = sumAggregate.CreateAggregator(inputType);
			if (sumAggregator == null)
				return null;

			Expression<object> avgExpression = new Expression<object>();
			ParameterBinding sumParam = avgExpression.Parameters.Add("@Sum", sumAggregator.ReturnType);
			ParameterBinding countParam = avgExpression.Parameters.Add("@Count", typeof(int));
			avgExpression.Text = "@Sum / @Count";

			try
			{
				avgExpression.Resolve();
			}
			catch (CompilationException)
			{
				return null;
			}

			return new AverageAggregator(sumAggregator, avgExpression, sumParam, countParam);
		}
	}
}