namespace NQuery
{
	/// <remarks>
	/// For a conceptual overview of code assistance see <a href="/Overview/CodeAssistance.html">Code Assistance</a>.
	/// </remarks>
	public interface IParameterInfoContext
	{
		/// <summary>
		/// Gets the the source code location where this parameter info context has been requested for.
		/// </summary>
		SourceLocation SourceLocation { get; }
		
		/// <summary>
		/// Gets the parameter index of the invocation in the current context. If there is no invocation the
		/// return value is <c>-1</c>.
		/// </summary>
		int ParameterIndex { get; }
		
		/// <summary>
		/// Enumerates all methods, functions or aggregates that are invocable from the current context.
		/// </summary>
		/// <param name="acceptor">The acceptor to call for each method, function or aggregate in the current context.</param>
		void Enumerate(IParameterInfoAcceptor acceptor);
	}
}