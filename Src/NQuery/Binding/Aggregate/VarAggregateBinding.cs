using System;

namespace NQuery.Runtime
{
	public sealed class VarAggregateBinding : AggregateBinding
	{
		public VarAggregateBinding(string name) : base(name)
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
				return new VarAndStdDevAggregator(true);

			return null;
		}
	}
}