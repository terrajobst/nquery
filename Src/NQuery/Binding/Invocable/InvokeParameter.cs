using System;

namespace NQuery.Runtime
{
	public abstract class InvokeParameter
	{
		public abstract string Name { get; }
		public abstract Type DataType { get; }
	}
}