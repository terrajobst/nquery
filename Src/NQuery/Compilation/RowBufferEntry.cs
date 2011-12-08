using System;
using System.Globalization;

namespace NQuery.Compilation
{
	internal sealed class RowBufferEntry
	{
		private string _name;
		private Type _dataType;

		public RowBufferEntry(Type dataType)
		{
#if DEBUG
			if (dataType == null)
				throw ExceptionBuilder.ArgumentNull("dataType");
#endif

			_dataType = dataType;
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public Type DataType
		{
			get { return _dataType; }
		}

		public override string ToString()
		{
			return String.Format(CultureInfo.InvariantCulture, "{0} : {1}", _name, _dataType);
		}
	}
}