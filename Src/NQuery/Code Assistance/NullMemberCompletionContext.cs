namespace NQuery.CodeAssistance
{
	internal sealed class NullMemberCompletionContext : IMemberCompletionContext
	{
		private SourceLocation _sourceLocation;

		public NullMemberCompletionContext(SourceLocation sourceLocation)
		{
			_sourceLocation = sourceLocation;
		}

		public SourceLocation SourceLocation
		{
			get { return _sourceLocation; }
		}

		public Identifier RemainingPart
		{
			get { return null; }
		}

		public void Enumerate(IMemberCompletionAcceptor acceptor)
		{
		}
	}
}