using System;

namespace NQuery.Compilation
{
	internal sealed class CharReader
	{
		public const char EOF = '\0';
		private const char CR = '\u000d';
		public const char LF = '\u000a';

		private string _source;
		private int _pos;
		private int _charIndex;
		private int _lineIndex;

		public CharReader(string source)
		{
			_source = source;
			Reset();
		}

		public Char Char
		{
			get { return Peek(0); }
		}

		public void Reset()
		{
			_pos = -1;
			_charIndex = -1;
			_lineIndex = 0;
		}

		public int Pos
		{
			get { return _pos; }
			set { Goto(value); }
		}

		public int CharIndex
		{
			get { return _charIndex; }
		}

		public int LineIndex
		{
			get { return _lineIndex; }
		}

		public SourceLocation Location
		{
			get { return new SourceLocation(_charIndex, _lineIndex); }
		}

		public char Next()
		{
			bool lastCharWasLineBreak = (Char == CR || Char == LF);
			_pos++;

			if (InternalPeek(0) == CR && InternalPeek(1) == LF)
			{
				// CR-LF is a single line break
				_pos++;
			}

			if (!lastCharWasLineBreak)
			{
				_charIndex++;
			}
			else
			{
				_charIndex = 0;
				_lineIndex++;
			}

			return Char;
		}

		public void Goto(int absoluteIndex)
		{
			if (absoluteIndex == _pos)
				return;

			// IMPORTANT: To improve performance we only reset the position
			//            to -1 if the new position is before the current one.
			//            Otherwise it is much better to just scan the delta.
			//
			//            This ensures that CharReader.Pos++ and CharReader.Next()
			//            perform equally well.

			if (absoluteIndex < _pos)
				Reset();

			while (_pos < absoluteIndex)
				Next();
		}

		private char InternalPeek(int relativeIndex)
		{
			int absoluteIndex = _pos + relativeIndex;

			if (absoluteIndex < 0 || absoluteIndex >= _source.Length)
				return EOF;

			return _source[absoluteIndex];
		}

		public char Peek()
		{
			return Peek(1);
		}

		public char Peek(int relativeIndex)
		{
			Char result = InternalPeek(relativeIndex);

			if (result == CR)
				return LF;

			return result;
		}
	}
}