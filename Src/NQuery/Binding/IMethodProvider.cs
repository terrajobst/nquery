using System;

namespace NQuery.Runtime
{
	/// <summary>
	/// Interface to be implemented by clients that whish to contribute their own
	/// method provider on specific types.
	/// </summary>
	public interface IMethodProvider
	{
		/// <summary>
		/// Returns the list of methods for the given type.
		/// </summary>
		/// <remarks>
		/// The query engine does not cache the result.
		/// </remarks>
		/// <param name="type">The <see cref="Type"/> to get the methods for.</param>
		/// <returns>A list of <see cref="MethodBinding"/> for the given type.</returns>
		MethodBinding[] GetMethods(Type type);
	}
}