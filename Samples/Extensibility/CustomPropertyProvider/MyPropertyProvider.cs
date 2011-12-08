using System;

using NQuery.Runtime;

namespace NQuery.Samples.CustomPropertyProvider
{
	#region MyDataType2

	public class MyDataType2
	{
		internal object[] Values;

		public MyDataType2(params object[] values)
		{
			Values = values;
		}
	}

	#endregion

	#region MyPropertyBinding

	public class MyPropertyBinding : PropertyBinding
	{
		private int _index;
		private Type _dataType;

		public MyPropertyBinding(int index, Type dataType)
		{
			_index = index;
			_dataType = dataType;
		}

		public override object GetValue(object instance)
		{
			MyDataType2 myDataType2 = (MyDataType2)instance;
			return myDataType2.Values[_index];
		}

		public override Type DataType
		{
			get { return _dataType; }
		}

		public override string Name
		{
			get { return String.Format("Prop{0}", _index); }
		}
	}

	#endregion

	#region MyPropertyProvider

	public class MyPropertyProvider : IPropertyProvider
	{
		public PropertyBinding[] GetProperties(Type type)
		{
			PropertyBinding[] result = new PropertyBinding[3];
			result[0] = new MyPropertyBinding(0, typeof(int));
			result[1] = new MyPropertyBinding(1, typeof(string));
			result[2] = new MyPropertyBinding(2, typeof(DateTime));
			return result;
		}
	}

	#endregion
}
