using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class SourceRangeRecorder
	{
		private Stack<SourceLocation> _startLocationStack = new Stack<SourceLocation>();
		private SourceLocation _lastStartLocation = SourceLocation.Empty;
		private SourceLocation _lastEndLocation = SourceLocation.Empty;

		public void RecordEnter(Token token)
		{
			_lastStartLocation = token.Range.StartLocation;
		}

		public void RecordLeave(Token token)
		{
			_lastEndLocation = token.Range.EndLocation;
		}

		public void Begin()
		{
			_startLocationStack.Push(_lastStartLocation);
		}

		public SourceRange End()
		{
			SourceLocation startLocation = _startLocationStack.Pop();
			return new SourceRange(startLocation, _lastEndLocation);
		}
	}
}