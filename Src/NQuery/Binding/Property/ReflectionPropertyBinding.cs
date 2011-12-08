using System;
using System.Reflection;

namespace NQuery.Runtime
{
	public class ReflectionPropertyBinding : PropertyBinding
	{
		private PropertyInfo _propertyInfo;
		private string _name;

		public ReflectionPropertyBinding(PropertyInfo propertyInfo)
			: this(propertyInfo, propertyInfo == null ? null : propertyInfo.Name)
		{
		}

		public ReflectionPropertyBinding(PropertyInfo propertyInfo, string name)
		{
			if (propertyInfo == null)
				throw ExceptionBuilder.ArgumentNull("propertyInfo");

			_propertyInfo = propertyInfo;	

			if (name == null)
				_name = propertyInfo.Name;
			else
				_name = name;
		}

		public override string Name
		{
			get { return _name; }
		}

		public override Type DataType
		{
			get { return _propertyInfo.PropertyType; }
		}

		internal PropertyInfo PropertyInfo
		{
			get { return _propertyInfo; }
		}

		public override object GetValue(object instance)
		{
			return _propertyInfo.GetValue(instance, null);
		}
	}
}