using System;
using System.Collections.Generic;

namespace NQuery.Runtime
{
	public sealed class ConstantBinding : Binding
	{
		private object _value;
		private Type _valueType;
		private string _name;
		private PropertyBindingCollection _customProperties;

		public ConstantBinding(string name, object value)
			: this(name, value, null)
		{
		}

		public ConstantBinding(string name, object value, IList<PropertyBinding> customProperties)
		{
			if (name == null)
				throw ExceptionBuilder.ArgumentNull("name");
			
			if (value == null)
				throw ExceptionBuilder.ArgumentNull("value");

			_name = name;
			_value = value;
			_valueType = value.GetType();
			_customProperties = (customProperties == null) ? null : new PropertyBindingCollection(customProperties);
		}

		public override BindingCategory Category
		{
			get { return BindingCategory.Constant; }
		}

		public override string Name
		{
			get { return _name; }
		}

		public Type DataType
		{
			get { return _valueType; }
		}

		public object Value
		{
			get { return _value; }
		}

		public PropertyBindingCollection CustomProperties
		{
			get { return _customProperties; }
		}
	}
}