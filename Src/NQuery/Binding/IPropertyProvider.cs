using System;

namespace NQuery.Runtime
{
	/// <summary>
	/// Interface to be implemented by clients that whish to contribute their own 
	/// property provider based on specific types.
	/// </summary>
	public interface IPropertyProvider
	{
		/// <summary>
		/// Returns the list of properties for the given type.
		/// </summary>
		/// <remarks>
		/// The query engine does not cache the result.
		/// </remarks>
		/// <param name="type">The <see cref="Type"/> to get the properties for.</param>
		/// <returns>A list of <see cref="PropertyBinding"/> for the given type.</returns>
		PropertyBinding[] GetProperties(Type type);
	}
}