namespace NQuery.CodeAssistance
{
	internal abstract class ParameterInfoContext : IParameterInfoContext
	{
		private SourceLocation _sourceLocation;
		private int _parameterIndex;

		public ParameterInfoContext(SourceLocation sourceLocation, int parameterIndex)
		{
			_sourceLocation = sourceLocation;
			_parameterIndex = parameterIndex;
		}

		public SourceLocation SourceLocation
		{
			get { return _sourceLocation; }
		}

		public int ParameterIndex
		{
			get { return _parameterIndex; }
		}

		public abstract void Enumerate(IParameterInfoAcceptor acceptor);
	}
}