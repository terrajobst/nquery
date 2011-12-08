using System;
using System.Collections.Generic;

using NQuery.Compilation;

namespace NQuery.CodeAssistance 
{
	internal sealed class TokenStream
	{
		private Token[] _tokens;
		private int _pos;

		private TokenStream()
		{
		}

		public static TokenStream FromSource(IErrorReporter errorReporter, string source)
		{
			Lexer lexer = new Lexer(errorReporter, source);

			List<Token> tokenList = new List<Token>();

			while (lexer.NextToken())
			{
				if (lexer.TokenID == TokenId.SELECT)
					lexer.IsQuery = true;

				Token token = lexer.GetToken();
				tokenList.Add(token);
			}

			TokenStream result = new TokenStream();
			result._tokens = tokenList.ToArray();

			return result;
		}

		public int Pos
		{
			get { return _pos; }
			set { _pos = value; }
		}

		public void ResetToStart()
		{
			_pos = 0;
		}

		public void ResetToEnd()
		{
			_pos = _tokens.Length - 1;
		}

		public bool ReadNext()
		{
			_pos++;

			if (_pos >= _tokens.Length)
			{
				_pos = _tokens.Length - 1;
				return false;
			}

			return true;
		}

		public bool ReadPrevious()
		{
			_pos--;

			if (_pos < 0)
			{
				_pos = 0;
				return false;
			}

			return true;
		}

		public void SkipTo(SourceLocation tokenLocation)
		{
			if (Token.Range.StartLocation > tokenLocation)
			{
				// If the current token is after the requested position we cannot get
				// there so we are finished.
				
				return;
			}
			else if (Token.Range.StartLocation <= tokenLocation && tokenLocation <= Token.Range.EndLocation)
			{
				// In this case the current token is the searched one.

				return;
			}

			// Skip all tokens that end before the requested position

			do
			{
				if (!ReadNext())
					return;
			}
			while (Token.Range.EndLocation < tokenLocation);

			// The current token must contain the token pos. If the token starts after
			// the requested token pos that position is in the middle of two tokens.
			// In this case we return false.

			if (Token.Range.StartLocation >= tokenLocation)
				ReadPrevious();
		}

		public bool SkipToNext(TokenId tokenID)
		{
			while (ReadNext() && Token.Id != tokenID)
			{
				// Skip tokens.
			}

			return Token.Id == tokenID;
		}

		public bool SkipToPrevious(TokenId tokenID)
		{
			while (ReadPrevious() && Token.Id != tokenID)
			{
				// Skip tokens.
			}

			return Token.Id == tokenID;
		}

		public Token Token
		{
			get { return _tokens[_pos]; }
		}
	}
}