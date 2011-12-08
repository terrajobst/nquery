using System;
using System.Collections.Generic;

namespace NQuery.Runtime
{
	public sealed class ParameterBinding : Binding
	{
		private string _name;
		private Type _parameterType;
		private object _value;
		private PropertyBindingCollection _customProperties;

		public ParameterBinding(string name, Type parameterType)
			: this(name, parameterType, null)
		{
		}

		public ParameterBinding(string name, Type parameterType, IList<PropertyBinding> customProperties)
		{
			if (name == null)
				throw ExceptionBuilder.ArgumentNull("name");

			if (parameterType == null)
				throw ExceptionBuilder.ArgumentNull("parameterType");

			_name = ExtractParameterName(name);
			_parameterType = parameterType;
			_customProperties = (customProperties == null) ? null : new PropertyBindingCollection(customProperties);
		}

		internal static string ExtractParameterName(string parameterName)
		{
			if (parameterName == null || parameterName.Length == 0 || parameterName[0] != '@')
				return parameterName;
			
			return parameterName.Substring(1);
		}

		public override string Name
		{
			get { return _name; }
		}

		public override string GetFullName()
		{
			return "@" + _name;
		}

		public override BindingCategory Category
		{
			get { return BindingCategory.Parameter; }
		}

		public Type DataType
		{
			get { return _parameterType; }
		}

		public object Value
		{
			get { return _value; }
			set
			{
				value = NullHelper.UnifyNullRepresentation(value);

				if (value != null && !_parameterType.IsAssignableFrom(value.GetType()))
					throw ExceptionBuilder.ParameterValueTypeMismatch("value");

				_value = value;
			}
		}

		public PropertyBindingCollection CustomProperties
		{
			get { return _customProperties; }
		}
	}
}