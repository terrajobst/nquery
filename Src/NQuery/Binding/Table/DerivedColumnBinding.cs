using System;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class DerivedColumnBinding : ColumnBinding
	{
		private string _name;
		private Type _dataType;

		public DerivedColumnBinding(TableBinding derivedTable, string name, Type dataType)
			: base(derivedTable)
		{
			_name = name;
			_dataType = dataType;
		}

		public override Type DataType
		{
			get { return _dataType; }
		}

		public override object GetValue(object row)
		{
			throw new NotImplementedException();
		}

		public override string Name
		{
			get { return _name; }
		}
	}
}