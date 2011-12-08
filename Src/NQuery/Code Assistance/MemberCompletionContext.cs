namespace NQuery.CodeAssistance
{
	internal abstract class MemberCompletionContext : IMemberCompletionContext
	{
		private SourceLocation _sourceLocation;
		private Identifier _remainingPart;
		
		protected MemberCompletionContext(SourceLocation sourceLocation, Identifier remainingPart)
		{
			_sourceLocation = sourceLocation;
			_remainingPart = remainingPart;
		}

		public SourceLocation SourceLocation
		{
			get { return _sourceLocation; }
		}

		public Identifier RemainingPart
		{
			get { return _remainingPart; }
		}
		
		public abstract void Enumerate(IMemberCompletionAcceptor acceptor);
	}
}