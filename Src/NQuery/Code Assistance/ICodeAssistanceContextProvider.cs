namespace NQuery
{
	/// <summary>
	/// Defines a mechanism for retrieving a context useful for code assistance; that are, objects that 
	/// provide member completion and parameter info.
	/// </summary>
	/// <remarks>
	/// For a conceptual overview of code assistance see <a href="/Overview/CodeAssistance.html">Code Assistance</a>.
	/// </remarks>
	public interface ICodeAssistanceContextProvider
	{
		/// <summary>
		/// Provides a member completion context for a given location in source code i.e. a list of elements such as
		/// tables and keywords accessible.
		/// </summary>
		/// <param name="sourceLocation">The source code location where member completion is requested for.</param>
		/// <exception cref="CompilationException">Thrown when the current context contained too many errors to provide member completion.</exception>
		IMemberCompletionContext ProvideMemberCompletionContext(SourceLocation sourceLocation);
		
		/// <summary>
		/// Provides parameter info context for a given location in source code i.e. a list of functions and parameters.
		/// </summary>
		/// <param name="sourceLocation">The source code location where parameter info is requested for.</param>
		/// <exception cref="CompilationException">Thrown when the current context contained too many errors to provide parameter info.</exception>
		IParameterInfoContext ProvideParameterInfoContext(SourceLocation sourceLocation);
	}
}