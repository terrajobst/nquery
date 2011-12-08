using System;
using System.Reflection;

namespace NQuery.Runtime
{
	public class ReflectionFieldBinding : PropertyBinding
	{
		private string _name;
		private FieldInfo _fieldInfo;

		public ReflectionFieldBinding(FieldInfo fieldInfo)
			: this(fieldInfo, fieldInfo == null ? null : fieldInfo.Name)
		{
		}

		public ReflectionFieldBinding(FieldInfo fieldInfo, string name)
		{
			if (fieldInfo == null)
				throw ExceptionBuilder.ArgumentNull("fieldInfo");

			_fieldInfo = fieldInfo;	

			if (name == null)
				_name = fieldInfo.Name;
			else
				_name = name;
		}

		public override string Name
		{
			get { return _name; }
		}

		public override Type DataType
		{
			get { return _fieldInfo.FieldType; }
		}

		internal FieldInfo FieldInfo
		{
			get { return _fieldInfo; }
		}

		public override object GetValue(object instance)
		{
			return _fieldInfo.GetValue(instance);
		}
	}
}