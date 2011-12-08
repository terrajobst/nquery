using System;

namespace NQuery.Runtime
{
	public sealed class CountAggregateBinding : AggregateBinding
	{
		public CountAggregateBinding(string name)
			: base(name)
		{
		}

		public override IAggregator CreateAggregator(Type inputType)
		{
			if (inputType == null)
				throw ExceptionBuilder.ArgumentNull("inputType");

			return new CountAggregator();
		}
	}
}