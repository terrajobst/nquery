using System;

namespace NQuery
{
	internal sealed class ExceptionErrorProvider : ErrorProvider
	{
		public static readonly ExceptionErrorProvider Instance = new ExceptionErrorProvider();

		private ExceptionErrorProvider()
		{
		}

		protected override void OnError(CompilationError compilationError)
		{
			throw ExceptionBuilder.InternalError(compilationError.Text);
		}
	}
}