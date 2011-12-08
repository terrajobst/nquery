using System;

namespace NQuery.Runtime
{
	public class ConcatAggregateBinding : AggregateBinding
	{
		public ConcatAggregateBinding(string name)
			: base(name)
		{
		}

		public override IAggregator CreateAggregator(Type inputType)
		{
			return new ConcatAggregator();			
		}
	}
}