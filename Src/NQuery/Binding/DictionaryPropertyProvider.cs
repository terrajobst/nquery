using System;
using System.Collections;
using System.Collections.Generic;

namespace NQuery.Runtime
{
	/// <summary>
	/// Provides properties for an instance of <see cref="IDictionary"/>.
	/// </summary>
    public static class DictionaryPropertyProvider
	{
		/// <summary>
		/// Returns the list of properties for the given instance.
		/// </summary>
		/// <remarks>
		/// The query engine does not cache the result.
		/// </remarks>
		/// <param name="dictionary">The instance to get the properties for.</param>
		/// <returns>A list of <see cref="PropertyBinding"/> for the given instance.</returns>
		public static KeyPropertyBinding[] GetProperties(IDictionary dictionary)
		{
			if (dictionary == null)
				throw ExceptionBuilder.ArgumentNull("dictionary");

			List<KeyPropertyBinding> propertyList = new List<KeyPropertyBinding>();

			foreach (object key in dictionary.Keys)
			{
				string name = key.ToString();
				object value = dictionary[key];
				Type type = (value == null) ? typeof(object) : value.GetType();

				KeyPropertyBinding propertyBinding = new KeyPropertyBinding(name, key, type);
				propertyList.Add(propertyBinding);
			}

			return propertyList.ToArray();
		}
	}
}