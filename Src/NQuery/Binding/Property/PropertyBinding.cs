using System;

namespace NQuery.Runtime
{
	public abstract class PropertyBinding : Binding
	{
		protected PropertyBinding() 
		{
		}

		public abstract Type DataType { get; }

		public override BindingCategory Category
		{
			get { return BindingCategory.Property; }
		}

		public abstract object GetValue(object instance);
	}
}