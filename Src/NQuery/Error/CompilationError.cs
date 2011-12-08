using System;

namespace NQuery
{
	/// <summary>
	/// Represents a compile-time error contained in <see cref="CompilationException"/>.
	/// </summary>
	[Serializable]
	public sealed class CompilationError
	{
		private SourceRange _sourceRange;
		private ErrorId _id;
		private string _text;

		public CompilationError(SourceRange sourceRange, ErrorId id, string text)
		{
			if (text == null)
				throw ExceptionBuilder.ArgumentNull("text");

			_sourceRange = sourceRange;
			_id = id;
			_text = text;
		}

		/// <summary>
		/// Gets the <see cref="SourceRange"/> related to this error.
		/// </summary>
		public SourceRange SourceRange
		{
			get { return _sourceRange; }
		}

		/// <summary>
		/// Gets the <see cref="ErrorId"/> of this error.
		/// </summary>
		public ErrorId Id
		{
			get { return _id; }
		}

		/// <summary>
		/// Gets the message of this error.
		/// </summary>
		public string Text
		{
			get { return _text; }
		}
	}
}