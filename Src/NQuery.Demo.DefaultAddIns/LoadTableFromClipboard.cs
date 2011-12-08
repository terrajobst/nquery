using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;

using NQuery.Demo.AddIns;

namespace NQuery.Demo.DefaultAddins
{
	public sealed class LoadTableFromClipboard : IAddIn
	{
		#region Tab Delimeted Values Parsing

		private enum TokenType
		{
			Eof,
			Linebreak,
			Separator,
			Value
		}

		private sealed class Token
		{
			private TokenType _tokenType;
			private string _text;

			public Token(TokenType tokenType, string text)
			{
				_tokenType = tokenType;
				_text = text;
			}

			public TokenType TokenType
			{
				get { return _tokenType; }
			}

			public string Text
			{
				get { return _text; }
			}
		}

		private sealed class CharReader
		{
			private string _text;
			private int _pos;

			public CharReader(string text)
			{
				_text = text;
			}

			public bool Eof
			{
				get { return _pos >= _text.Length; }
			}

			public char Current
			{
				get { return Peek(0); }
			}

			public char Peek()
			{
				return Peek(1);
			}

			public char Peek(int lookahead)
			{
				int index = _pos + lookahead;
				if (index < 0 || index >= _text.Length)
					return '\0';

				return _text[index];
			}

			public int Pos
			{
				get { return _pos; }
				set { _pos = value; }
			}
		}

		private sealed class Scanner
		{
			private CharReader _charReader;
			private const char EOF = '\0';
			private const char CR = '\r';
			private const char LF = '\n';
			private const char TAB = '\t';
			private const char QUOTE = '"';

			public Scanner(string text)
			{
				_charReader = new CharReader(text);
			}

			public Token Read()
			{
				if (_charReader.Eof)
					return new Token(TokenType.Eof, String.Empty);

				switch(_charReader.Current)
				{
					case CR:
						_charReader.Pos++;
						if (_charReader.Current == LF)
							_charReader.Pos++;
						return new Token(TokenType.Linebreak, Environment.NewLine);

					case LF:
						_charReader.Pos++;
						return new Token(TokenType.Linebreak, Environment.NewLine);

					case TAB:
						_charReader.Pos++;
						return new Token(TokenType.Separator, "\t");

					case QUOTE:
						return ReadMaskedValue();

					default:
						return ReadValue();
				}
			}

			private Token ReadMaskedValue()
			{
				_charReader.Pos++;

				StringBuilder sb = new StringBuilder();
				while (_charReader.Current != EOF)
				{
					if (_charReader.Current == QUOTE)
					{
						_charReader.Pos++;
						if (_charReader.Current != QUOTE)
							break;
					}

					sb.Append(_charReader.Current);
					_charReader.Pos++;
				}

				return new Token(TokenType.Value, sb.ToString());
			}

			private Token ReadValue()
			{
				StringBuilder sb = new StringBuilder();
				while (_charReader.Current != EOF)
				{
					if (_charReader.Current == TAB ||
					    _charReader.Current == CR ||
					    _charReader.Current == LF)
						break;

					sb.Append(_charReader.Current);
					_charReader.Pos++;
				}

				return new Token(TokenType.Value, sb.ToString());
			}
		}

		private sealed class Parser
		{
			private Scanner _scanner;
			private Token _current;
			private Token _lookahead;

			public Parser(string text)
			{
				_scanner = new Scanner(text);
				_current = _scanner.Read();
				_lookahead = _scanner.Read();
			}

			private void Next()
			{
				_current = _lookahead;
				_lookahead = _scanner.Read();
			}

			private void Match(TokenType value)
			{
				if (_current.TokenType != value)
					throw new InvalidOperationException(String.Format("Expected token '{0}' but found '{1}", value, _current.TokenType));

				Next();
			}

			public DataTable ParseTable()
			{
				DataTable result = new DataTable();

				// Build column definitions
				
				string[] headers = ParseRow();
				foreach (string header in headers)
				{
					DataColumn dataColumn = new DataColumn(header, typeof(string));
					result.Columns.Add(dataColumn);
				}

				// Read rows
				while (_current.TokenType != TokenType.Eof)
				{
					string[] values = ParseRow();
					
					if (values.Length > result.Columns.Count)
					{
						// Drop all values beyond column count
						string[] newValues = new string[result.Columns.Count];
						Array.Copy(values, newValues, newValues.Length);
						values = newValues;
					}
					else if (values.Length < result.Columns.Count)
					{
						// Padd with empty strings
						string[] newValues = new string[result.Columns.Count];
						Array.Copy(values, newValues, values.Length);

						for (int i = values.Length; i < newValues.Length; i++)
							newValues[i] = String.Empty;
						values = newValues;
					}

					result.Rows.Add(values);
				}

				return result;
			}

			private string[] ParseRow()
			{
				List<string> values = new List<string>();
				while (_current.TokenType != TokenType.Eof)
				{
					if (_current.TokenType == TokenType.Value)
					{
						values.Add(_current.Text);
						Next();
					}
					else if (_current.TokenType == TokenType.Separator)
					{
						values.Add(null);
					}

					if (_current.TokenType == TokenType.Separator)
						Next();

					if (_current.TokenType == TokenType.Linebreak)
					{
						Next();
						break;
					}
				}

				return values.ToArray();
			}
		}

		#endregion

		public QueryContext CreateQueryContext()
		{
			
			string textFromClipboard = Clipboard.GetText();
			Parser parser = new Parser(textFromClipboard);
			DataTable dataTable = parser.ParseTable();
			dataTable.TableName = "Clipboard";

			Query query = new Query();
			query.DataContext.Tables.Add(dataTable);
			query.Text = "SELECT * FROM Clipboard";

			QueryContext queryContext = new QueryContext(query, "Clipboard");
			return queryContext;
		}

		public string Name
		{
			get { return "Load Table From Clipboard"; }
		}
	}
}
