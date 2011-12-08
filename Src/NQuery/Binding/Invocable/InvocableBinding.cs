using System;

namespace NQuery.Runtime
{
	public abstract class InvocableBinding : Binding
	{
		public abstract InvokeParameter[] GetParameters();

		public Type[] GetParameterTypes()
		{
			InvokeParameter[] parameters = GetParameters();

			Type[] result = new Type[parameters.Length];
			for (int i = 0; i < result.Length; i++)
				result[i] = parameters[i].DataType;

			return result;
		}

		public abstract Type ReturnType { get; }
		public abstract bool IsDeterministic { get; }
	}
}