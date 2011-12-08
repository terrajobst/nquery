using System;

namespace NQuery.Runtime
{
	public sealed class FirstAggregateBinding : AggregateBinding
	{
		public FirstAggregateBinding(string name)
			: base(name)
		{
		}

		public override IAggregator CreateAggregator(Type inputType)
		{
			if (inputType == null)
				throw ExceptionBuilder.ArgumentNull("inputType");

			return new FirstAggregator(inputType);
		}
	}
}