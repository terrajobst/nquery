using System;
using System.Diagnostics.CodeAnalysis;

namespace NQuery.Compilation
{
	internal sealed class Lexer
	{
		private IErrorReporter _errorReporter;
		private string _source;
		private CharReader _reader;
		private bool _isQuery;

		private TokenId _tokenId;
		private int _tokenStart;
		private int _tokenEnd;
		private SourceRange _tokenRange;

		public Lexer(IErrorReporter errorReporter, string source)
		{
			_errorReporter = errorReporter;
			_source = source;
			_reader = new CharReader(source);
			_tokenRange = SourceRange.Empty;
		}

		public void Reset()
		{
			Reset(0);
		}

		public void Reset(int pos)
		{
			if (pos == 0)
				_reader.Reset();
			else
				_reader.Pos = pos - 1;

			_tokenId = TokenId.Unknown;

			NextToken();
		}

		private TokenId ReadString()
		{
			bool finished = false;

			while (!finished)
			{
				switch (_reader.Peek())
				{
					case CharReader.EOF:
						_errorReporter.UnterminatedString(_tokenRange.StartLocation, _source.Substring(_tokenStart + 1, _reader.Pos - _tokenStart));
						finished = true;
						break;

					case '\'':
						_reader.Pos++;

						if (_reader.Peek() == '\'')
							_reader.Pos++;
						else
							finished = true;
						break;

					default:
						_reader.Pos++;
						break;
				}
			}

			return TokenId.String;
		}

		private TokenId ReadIdentifierOrKeyword()
		{
			// The following characters can be letters, digits the underscore and the dollar sign.

			while (Char.IsLetterOrDigit(_reader.Peek()) || _reader.Peek() == '_' || _reader.Peek() == '$')
				_reader.Pos++;

			string token = _source.Substring(_tokenStart, _reader.Pos - _tokenStart + 1);

			TokenInfo info = TokenInfo.FromText(token);

			if (info.IsQueryKeyword && !_isQuery)
				return TokenId.Identifier;

			return info.TokenId;
		}

		private TokenId ReadQuotedIdentifier()
		{
			bool finished = false;

			while (!finished)
			{
				switch (_reader.Peek())
				{
					case CharReader.EOF:
					case CharReader.LF:
						_errorReporter.UnterminatedQuotedIdentifier(_tokenRange.StartLocation, _source.Substring(_tokenStart + 1, _reader.Pos - _tokenStart));
						finished = true;
						break;

					case '"':
						_reader.Pos++;

						if (_reader.Peek() == '"')
							_reader.Pos++;
						else
							finished = true;
						break;

					default:
						_reader.Pos++;
						break;
				}
			}

			return TokenId.Identifier;
		}

		private TokenId ReadParenthesizedIdentifier()
		{
			bool finished = false;

			while (!finished)
			{
				switch (_reader.Peek())
				{
					case CharReader.EOF:
					case CharReader.LF:
						_errorReporter.UnterminatedParenthesizedIdentifier(_tokenRange.StartLocation, _source.Substring(_tokenStart + 1, _reader.Pos - _tokenStart));
						finished = true;
						break;

					case ']':
						_reader.Pos++;

						if (_reader.Peek() == ']')
							_reader.Pos++;
						else
							finished = true;
						break;

					default:
						_reader.Pos++;
						break;
				}
			}

			return TokenId.Identifier;
		}

		private TokenId ReadNumber()
		{
			// Just read everything that looks like it could be a number
			// numbers are verified by the parser

			bool finished = false;

			while (!finished)
			{
				char c = _reader.Peek();
				switch (c)
				{
					// dot

					case '.':
						break;

					// special handling for e, it could be the exponent indicator 
					// followed by an optional sign

					case 'E':
					case 'e':
						if (_source[_reader.Pos + 2] == '-' || _source[_reader.Pos + 2] == '+')
							_reader.Pos++;
						break;

					default:
						finished = !Char.IsLetterOrDigit(c);
						break;
				}

				if (!finished)
					_reader.Pos++;
			}

			return TokenId.Number;
		}

		private TokenId ReadDate()
		{
			// Just read everything that looks like it could be a date
			// date are verified by the parser

			bool finished = false;

			while (!finished)
			{
				char currentChar = _reader.Peek();

				switch (currentChar)
				{
					case CharReader.EOF:
					case CharReader.LF:
						_errorReporter.UnterminatedDate(_tokenRange.StartLocation, _source.Substring(_tokenStart + 1, _reader.Pos - _tokenStart));
						finished = true;
						break;

					case '#':
						_reader.Pos++;
						finished = true;
						break;

					default:
						_reader.Pos++;
						break;
				}
			}

			return TokenId.Date;
		}

		private void ReadSinglelineComment()
		{
			while (true)
			{
				switch (_reader.Char)
				{
					case CharReader.EOF:
                        _reader.Pos--;
						return;

					case CharReader.LF:
						return;

					default:
						_reader.Pos++;
						break;
				}
			}
		}

		private void ReadMultilineComment()
		{
			_reader.Pos += 2;

			while (true)
			{
				switch (_reader.Char)
				{
					case CharReader.EOF:
						_errorReporter.UnterminatedComment(_tokenRange.StartLocation, _source.Substring(_tokenStart, _reader.Pos - _tokenStart));
						_tokenId = TokenId.Eof;
						return;

					case '*':
						if (_reader.Peek() == '/')
						{
							_reader.Pos++;
							return;
						}
						_reader.Pos++;
						break;

					default:
						_reader.Pos++;
						break;
				}
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public bool NextToken()
		{
			// Ignore calls after eof

			if (_tokenId == TokenId.Eof)
				return false;

			// Set token to unknown to allow rescanning

			_tokenId = TokenId.Unknown;

			while (_tokenId == TokenId.Unknown)
			{
				_tokenStart = ++_reader.Pos;
				_tokenRange.StartColumn = _reader.CharIndex;
				_tokenRange.StartLine = _reader.LineIndex;

				switch (_reader.Char)
				{
					case CharReader.EOF:
						_tokenId = TokenId.Eof;
						break;

					case CharReader.LF:
						break;

					case '~':
						_tokenId = TokenId.BitwiseNot;
						break;

					case '&':
						_tokenId = TokenId.BitwiseAnd;
						break;

					case '|':
						_tokenId = TokenId.BitwiseOr;
						break;

					case '^':
						_tokenId = TokenId.BitwiseXor;
						break;

					case '(':
						_tokenId = TokenId.LeftParentheses;
						break;

					case ')':
						_tokenId = TokenId.RightParentheses;
						break;

					case '.':
						if (Char.IsDigit(_reader.Peek()))
							_tokenId = ReadNumber();
						else
							_tokenId = TokenId.Dot;
						break;

					case '@':
						_tokenId = TokenId.ParameterMarker;
						break;

					case '+':
						_tokenId = TokenId.Plus;
						break;

					case '-':
						if (_reader.Peek() == '-')
							ReadSinglelineComment();
						else
							_tokenId = TokenId.Minus;
						break;

					case '*':
						if (_reader.Peek() == '*')
						{
							_tokenId = TokenId.Power;
							_reader.Pos++;
						}
						else
							_tokenId = TokenId.Multiply;
						break;

					case '/':
						if (_reader.Peek() == '/')
							ReadSinglelineComment();
						else if (_reader.Peek() == '*')
							ReadMultilineComment();
						else
							_tokenId = TokenId.Divide;
						break;
						
					case '%':
						_tokenId = TokenId.Modulus;
						break;
						
					case ',':
						_tokenId = TokenId.Comma;
						break;

					case '=':
						_tokenId = TokenId.Equals;
						break;

					case '!':
						if (_reader.Peek() == '=')
						{
							_tokenId = TokenId.Unequals;
							_reader.Pos++;
						}
						else if (_reader.Peek() == '>')
						{
							_tokenId = TokenId.NotGreater;
							_reader.Pos++;
						}
						else if (_reader.Peek() == '<')
						{
							_tokenId = TokenId.NotLess;
							_reader.Pos++;
						}
						else
							_errorReporter.IllegalInputCharacter(_reader.Location, _reader.Char);
						break;

					case '<':
						if (_reader.Peek() == '<')
						{
							_tokenId = TokenId.LeftShift;
							_reader.Pos++;
						}
						else if (_reader.Peek() == '>')
						{
							_tokenId = TokenId.Unequals;
							_reader.Pos++;
						}
						else if (_reader.Peek() == '=')
						{
							_tokenId = TokenId.LessOrEqual;
							_reader.Pos++;
						}
						else
							_tokenId = TokenId.Less;
						break;

					case '>':
						if (_reader.Peek() == '>')
						{
							_tokenId = TokenId.RightShift;
							_reader.Pos++;
						}
						else if (_reader.Peek() == '=')
						{
							_tokenId = TokenId.GreaterOrEqual;
							_reader.Pos++;
						}
						else
							_tokenId = TokenId.Greater;
						break;

					case '\'':
						_tokenId = ReadString();
						break;

					case '"':
						_tokenId = ReadQuotedIdentifier();
						break;

					case '[':
						_tokenId = ReadParenthesizedIdentifier();
						break;

					case '#':
						_tokenId = ReadDate();
						break;

					default:
						if (Char.IsWhiteSpace(_reader.Char))
						{
							// whitespaces, just eat them. The loop will continue.
						}
						else if (Char.IsLetter(_reader.Char) || _reader.Char == '_')
						{
							_tokenId = ReadIdentifierOrKeyword();
						}
						else if (Char.IsDigit(_reader.Char))
						{
							_tokenId = ReadNumber();
						}
						else
						{
							_errorReporter.IllegalInputCharacter(_reader.Location, _reader.Char);
						}

						break;
				}
			}

			_tokenEnd = _reader.Pos;
			_tokenRange.EndColumn = _reader.CharIndex;
			_tokenRange.EndLine = _reader.LineIndex;

			return true;
		}

		public bool IsQuery
		{
			get { return _isQuery; }
			set { _isQuery = value; }
		}

		public TokenId TokenID
		{
			get { return _tokenId; }
		}

		public string GetTokenText()
		{
			if (_tokenId == TokenId.Eof)
				return TokenInfo.FromTokenId(TokenId.Eof).Text;

			int tokenLength = _tokenEnd - _tokenStart + 1;

			return _source.Substring(_tokenStart, tokenLength);
		}

		public Token GetToken()
		{
			string tokenText;

			TokenInfo info = TokenInfo.FromTokenId(_tokenId);

			if (info.HasDynamicText)
				tokenText = GetTokenText();
			else
				tokenText = info.Text;

			return new Token(tokenText, _tokenId, _tokenStart, _tokenRange);
		}
	}
}
