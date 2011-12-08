namespace NQuery
{
	/// <remarks>
	/// For a conceptual overview of code assistance see <a href="/Overview/CodeAssistance.html">Code Assistance</a>.
	/// </remarks>
	public interface IMemberCompletionContext
	{
		/// <summary>
		/// Gets the the source code location where this member completion context has been requested for.
		/// </summary>
		SourceLocation SourceLocation { get; }
		
		/// <summary>
		/// Gets the identifier that has been written directly after the dot. If there was no dot or the
		/// member completion has been requested directly after the dot the return value is <see langword="null"/>.
		/// </summary>
		Identifier RemainingPart { get; }
		
		/// <summary>
		/// Enumerates all members that are accessible form this context.
		/// </summary>
		/// <param name="acceptor">The acceptor to call for each accessible element in the current context.</param>
		void Enumerate(IMemberCompletionAcceptor acceptor);
	}
}