using System;

namespace NQuery.Runtime
{
	public sealed class PropertyColumnBinding : ColumnBinding
	{
		private PropertyBinding _propertyDefinition;

		public PropertyColumnBinding(TableBinding tableBinding, PropertyBinding propertyDefinition)
			: base(tableBinding)
		{
			_propertyDefinition = propertyDefinition;
		}

		public override string Name
		{
			get { return _propertyDefinition.Name; }
		}

		public override Type DataType
		{
			get { return _propertyDefinition.DataType; }
		}

		public override object GetValue(object row)
		{
			return _propertyDefinition.GetValue(row);
		}
	}
}