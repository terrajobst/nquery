using System;
using System.Collections.Generic;

namespace NQuery
{
	public class CompilationFailedEventArgs : EventArgs
	{
		private CompilationErrorCollection _compilationErrors;

		public CompilationFailedEventArgs(IList<CompilationError> errors)
		{
			if (errors == null)
				throw ExceptionBuilder.ArgumentNull("errors");

			_compilationErrors = new CompilationErrorCollection(errors);
		}

		public CompilationErrorCollection CompilationErrors
		{
			get { return _compilationErrors; }
		}
	}
}