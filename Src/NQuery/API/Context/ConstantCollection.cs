using System;

using NQuery.Runtime;

namespace NQuery
{
	public sealed class ConstantCollection : BindingCollection<ConstantBinding>
	{
		internal ConstantCollection()
		{
		}

		public ConstantBinding Add(string constantName, object value)
		{
			if (constantName == null)
				throw ExceptionBuilder.ArgumentNull("constantName");

			if (value == null)
				throw ExceptionBuilder.ArgumentNull("value");

			ConstantBinding constantBinding = new ConstantBinding(constantName, value);
			Add(constantBinding);
			return constantBinding;
		}

		public ConstantBinding Add(string constantName, object value, PropertyBinding[] customProperties)
		{
			if (constantName == null)
				throw ExceptionBuilder.ArgumentNull("constantName");

			if (value == null)
				throw ExceptionBuilder.ArgumentNull("value");

			if (customProperties == null)
				throw ExceptionBuilder.ArgumentNull("customProperties");

			ConstantBinding constantBinding = new ConstantBinding(constantName, value, customProperties);
			Add(constantBinding);
			return constantBinding;
		}
	}
}
