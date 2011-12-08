using System;

namespace NQuery.Runtime
{
	public sealed class StdDevAggregateBinding : AggregateBinding
	{
		public StdDevAggregateBinding(string name) : base(name)
		{
		}

		public override IAggregator CreateAggregator(Type inputType)
		{
			if (inputType == typeof(byte) ||
				inputType == typeof(sbyte) || 
				inputType == typeof(short) ||
				inputType == typeof(ushort) ||
			    inputType == typeof(int) ||
			    inputType == typeof(uint) ||
				inputType == typeof(long) ||
				inputType == typeof(ulong) ||
				inputType == typeof(decimal) ||
			    inputType == typeof(float) ||
			    inputType == typeof(double))
				return new VarAndStdDevAggregator(false);

			return null;
		}
	}
}