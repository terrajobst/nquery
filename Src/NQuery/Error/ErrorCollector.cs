using System;
using System.Collections.Generic;

namespace NQuery
{
	internal class ErrorCollector : ErrorProvider
	{
		private List<CompilationError> _errorList = new List<CompilationError>();

		protected override void OnError(CompilationError compilationError)
		{
			base.OnError(compilationError);

			_errorList.Add(compilationError);
		}

		public override void Reset()
		{
			base.Reset();

			_errorList.Clear();
		}

		public IList<CompilationError> GetErrors()
		{
			return _errorList;
		}
	}
}