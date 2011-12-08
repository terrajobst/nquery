using System;

namespace NQuery.Runtime
{
	public sealed class LastAggregateBinding : AggregateBinding
	{
		public LastAggregateBinding(string name)
			: base(name)
		{
		}

		public override IAggregator CreateAggregator(Type inputType)
		{
			if (inputType == null)
				throw ExceptionBuilder.ArgumentNull("inputType");

			return new LastAggregator(inputType);
		}
	}
}