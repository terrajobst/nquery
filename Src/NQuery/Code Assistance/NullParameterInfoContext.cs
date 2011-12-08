namespace NQuery.CodeAssistance
{
	internal sealed class NullParameterInfoContext : IParameterInfoContext
	{
		private SourceLocation _sourceLocation;

		public NullParameterInfoContext(SourceLocation sourceLocation)
		{
			_sourceLocation = sourceLocation;
		}

		public SourceLocation SourceLocation
		{
			get { return _sourceLocation; }
		}

		public int ParameterIndex
		{
			get { return -1; }
		}

		public void Enumerate(IParameterInfoAcceptor acceptor)
		{
		}
	}
}