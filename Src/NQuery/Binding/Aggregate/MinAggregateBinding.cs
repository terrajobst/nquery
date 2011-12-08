using System;

namespace NQuery.Runtime
{
	public sealed class MinAggregateBinding : AggregateBinding
	{
		public MinAggregateBinding(string name)
			: base(name)
		{
		}
		
		public override IAggregator CreateAggregator(Type inputType)
		{
			if (inputType == null)
				throw ExceptionBuilder.ArgumentNull("inputType");

			if (typeof (IComparable).IsAssignableFrom(inputType))
				return new MinMaxAggregator(inputType, true);

			return null;
		}
	}
}