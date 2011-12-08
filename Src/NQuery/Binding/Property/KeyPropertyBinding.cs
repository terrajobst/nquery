using System;
using System.Collections;

namespace NQuery.Runtime
{
	/// <summary>
	/// Represents a property that is bound to a particular key. This class is used by 
	/// <see cref="DictionaryPropertyProvider"/> to represent the properties of an instance
	/// of <see cref="IDictionary"/>.
	/// </summary>
	public class KeyPropertyBinding : PropertyBinding
	{
		private string _name;
		private object _key;
		private Type _type;

		public KeyPropertyBinding(string name, object key, Type type)
		{
			if (name == null)
				throw ExceptionBuilder.ArgumentNull("name");

			if (type == null)
				throw ExceptionBuilder.ArgumentNull("type");

			if (key == null)
				throw ExceptionBuilder.ArgumentNull("key");

			_name = name;
			_key = key;
			_type = type;
		}

		public override Type DataType
		{
			get { return _type; }
		}

		public override object GetValue(object instance)
		{
			IDictionary dictionary = instance as IDictionary;
				
			if (dictionary == null)
				return null;
				
			return dictionary[_key];
		}

		public override string Name
		{
			get { return _name; }
		}
	}
}