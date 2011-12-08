using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

using NQuery.Compilation;

namespace NQuery
{
	/// <summary>
	/// Represents an identifier in the query engine. This class cannot be directly instantiated. Instead use
	///  <see cref="CreateVerbatim"/>, <see cref="CreateNonVerbatim"/>, or <see cref="FromSource"/>.
	/// </summary>
	public sealed class Identifier
	{
		[Flags]
		private enum IdentifierFlags
		{
			None           = 0x0000,
			Verbatim       = 0x0001,
			Parenthesized  = 0x0002
		}

		private string _text;
		private IdentifierFlags _flags;

		private Identifier([SuppressMessage("Microsoft.Globalization", "CA1303")] string text, IdentifierFlags flags)
		{
			if (text == null)
				throw ExceptionBuilder.ArgumentNull("text");

			_text = text;
			_flags = flags;
		}

		internal static readonly Identifier Missing = new Identifier("?", IdentifierFlags.Verbatim);

		/// <summary>
		/// Gets the textual value of this identifier, i.e. the name without any masking characters (e.g. quotes, brackets).
		/// </summary>
		public string Text
		{
			get { return _text; }
		}

		/// <summary>
		/// Gets a value indicating whether the textual value of this identifer is taken verbatim. This is equivalent to
		/// a quoted identifier.
		/// </summary>
		public bool Verbatim
		{
			get { return (_flags & IdentifierFlags.Verbatim) == IdentifierFlags.Verbatim; }
		}

		/// <summary>
		/// Gets a value indicating whether the textucal value can only be represented using brackets.
		/// </summary>
		public bool Parenthesized
		{
			get { return (_flags & IdentifierFlags.Parenthesized) == IdentifierFlags.Parenthesized; }
		}

		/// <summary>
		/// Creates a new verbatim (i.e. quoted) identifier.
		/// </summary>
		/// <param name="text">The textual value without any masking characters.</param>
		public static Identifier CreateVerbatim([SuppressMessage("Microsoft.Globalization", "CA1303")] string text)
		{
			return new Identifier(text, IdentifierFlags.Verbatim);
		}

		/// <summary>
		/// Creates a new non-verbatim (i.e. normal or parenthesized) identifier.
		/// </summary>
		/// <param name="text">The textual value without any masking characters.</param>
		public static Identifier CreateNonVerbatim([SuppressMessage("Microsoft.Globalization", "CA1303")] string text)
		{
			if (MustBeParenthesized(text))
				return new Identifier(text, IdentifierFlags.Parenthesized);
			else
				return new Identifier(text, IdentifierFlags.None);
		}

		private static Identifier InternalFromSource([SuppressMessage("Microsoft.Globalization", "CA1303")] string text, bool allowInvalid)
		{
			if (text == null)
				throw ExceptionBuilder.ArgumentNull("text");

			if (text.Length == 0 || (text[0] != '"' && text[0] != '['))
				return CreateNonVerbatim(text);

			const char EOF = '\0';
			IdentifierFlags flags;
			char endChar;
			string unterminatedMessage;

			if (text[0] == '[')
			{
				endChar = ']';
				unterminatedMessage = Resources.UnterminatedParenthesizedIdentifier;
				flags = IdentifierFlags.Parenthesized;
			}
			else
			{
				endChar = '"';
				unterminatedMessage = Resources.UnterminatedQuotedIdentifier;
				flags = IdentifierFlags.Verbatim;
			}

			StringBuilder sb = new StringBuilder();
			int pos = 1;
			while (true)
			{
				char c = pos < text.Length ? text[pos] : EOF;
				char l = pos < text.Length - 1 ? text[pos + 1] : EOF;

				if (c == EOF)
				{
					if (allowInvalid)
						break;
					else
						throw ExceptionBuilder.ArgumentInvalidIdentifier("text", String.Format(CultureInfo.CurrentCulture, unterminatedMessage, text));
				}
				else if (c == endChar)
				{
					if (l == endChar)
						pos++;
					else
						break;
				}

				sb.Append(c);
				pos++;
			}

			if (!allowInvalid && pos < text.Length - 1)
				throw ExceptionBuilder.ArgumentInvalidIdentifier("text", String.Format(CultureInfo.CurrentCulture, Resources.InvalidIdentifier, text));

			return new Identifier(sb.ToString(), flags);
		}

		/// <summary>
		/// Creates a new identifier from a source-code representation.
		/// </summary>
		/// <remarks>
		/// This method also accepts malformed identifiers. This is crucial to allow lexer and parser to use this class
		/// even if the identifier is not correct. However, in this case the error has already been handled by the lexer.
		/// </remarks>
		/// <param name="text">The textual value including masking characters.</param>
		internal static Identifier InternalFromSource([SuppressMessage("Microsoft.Globalization", "CA1303")] string text)
		{
			return InternalFromSource(text, true);
		}

		/// <summary>
		/// Creates a new identifier from a source-code representation.
		/// </summary>
		/// <param name="text">The textual value including masking characters.</param>
		public static Identifier FromSource([SuppressMessage("Microsoft.Globalization", "CA1303")] string text)
		{
			return InternalFromSource(text, false);
		}

		/// <summary>
		/// Detects whether a given text would not form a valid identifier and therefore must be parenthesized.
		/// </summary>
		/// <param name="text">The text of the identifier</param>
		public static bool MustBeParenthesized(string text)
		{
			if (text == null)
				throw ExceptionBuilder.ArgumentNull("text");

			if (text[0] != '_' && !Char.IsLetter(text[0]))
				return true;

			for (int i = 1; i < text.Length; i++)
			{
				if (!Char.IsLetterOrDigit(text[i]) && text[i] != '_' && text[i] != '$')
					return true;
			}

			TokenInfo tokenInfo = TokenInfo.FromText(text);
			if (tokenInfo.IsKeyword || tokenInfo.IsQueryKeyword)
				return true;

			return false;
		}

		/// <summary>
		/// Returns the source-code representation of this identifier.
		/// </summary>
		public string ToSource()
		{
			if (!Verbatim && !Parenthesized)
				return Text;

			char startChar = Parenthesized ? '[' : '"';
			char endChar = Parenthesized ? ']' : '"';

			StringBuilder sb = new StringBuilder();
			sb.Append(startChar);
			for (int i = 0; i < _text.Length; i++)
			{
				if (_text[i] == endChar)
					sb.Append(endChar);

				sb.Append(_text[i]);
			}
			sb.Append(endChar);
			return sb.ToString();
		}

		public override bool Equals(object obj)
		{
			Identifier identifier = obj as Identifier;
			
			if (identifier == null)
				return false;

			return this == identifier;
		}

		/// <summary>
		/// Compares this instance of an identifier to another instance.
		/// </summary>
		/// <param name="identifier">Other identfier to compare to.</param>
		public bool Equals(Identifier identifier)
		{
			if (identifier == null)
				return false;

			return this == identifier;
		}
		
		public override int GetHashCode()
		{
			return _text.GetHashCode() + 29 * _flags.GetHashCode();
		}

		public static bool operator==(Identifier left, Identifier right)
		{
			if (ReferenceEquals(left, right))
				return true;

			if (ReferenceEquals(left, null))
				return false;

			if (ReferenceEquals(right, null))
				return false;

			return left.Verbatim == right.Verbatim && left._text == right._text;
		}

		public static bool operator!=(Identifier left, Identifier right)
		{
			if (ReferenceEquals(left, right))
				return false;

			if (ReferenceEquals(left, null))
				return true;

			if (ReferenceEquals(right, null))
				return true;

			return left.Verbatim != right.Verbatim || left._text != right._text;
		}

		/// <summary>
		/// Checks whether this identifier matches the given text. If <see cref="Verbatim"/> is <see langword="true"/>
		/// the comparison is done case-sensitive otherwise it is case-insensitive.
		/// </summary>
		/// <param name="text">The text to match against</param>
		public bool Matches([SuppressMessage("Microsoft.Globalization", "CA1303")] string text)
		{
			if (text == null)
				return false;

			if (Verbatim)
			{
				// Compare the string literally

				return _text == text;
			}

			// We are not verbatim, compare the names case insensitive.

			return String.Compare(_text, text, StringComparison.OrdinalIgnoreCase) == 0;
		}

		/// <summary>
		/// Checks whether this identifier matches the given identifier.
		/// To match <paramref name="identifier" /> the following must be true:
		/// <ul>
		///		<li>The value of <see cref="Verbatim"/> of this instance and <paramref name="identifier" /> must be equal.</li>
		///		<li>If <see cref="Verbatim"/> is <see langword="true"/> the value of <see cref="Text"/> of this instance and <paramref name="identifier" /> must be equal (compared case-sensitive).</li>
		///		<li>If <see cref="Verbatim"/> is <see langword="false"/> the value of <see cref="Text"/> of this instance and <paramref name="identifier" /> must be equal (compared case-insensitive).</li>
		/// </ul>
		/// </summary>
		/// <param name="identifier">The identifier to match against</param>
		public bool Matches(Identifier identifier)
		{
			// Identifier a1 = new Identifier("Name", true);
			// Identifier a2 = new Identifier("NAME", true);
			//
			// Identifier b1 = new Identifier("Name", false);
			// Identifier b2 = new Identifier("NAME", false);
			//
			// a1 and a2 are verbatim identifiers, b1 and b2 are a non-verbatim identifiers.
			//
			// -- Every identifier matches itself
			//
			// a1.Matches(a1)  == true
			// a2.Matches(a2)  == true
			// b1.Matches(b1)  == true
			// b2.Matches(b2)  == true
			//
			// -- Verbatim idenfiers are compared case sensitive
			//
			// a1.Matches(a2)  == false
			// a2.Matches(a1)  == false
			//
			// -- Non verbatim identifiers are compared case insensitive
			//
			// b1.Matches(b2)  == true
			// b2.Matches(b1)  == true
			//
			// -- A verbatim identifier compared with a non-verbatim identifier will never produce a match
			//
			// a1.Matches(b1)  == false
			// a2.Matches(b2)  == false
			//
			// -- A non-verbatim identifier and a verbatim identifier are compared case insensitive
			//
			// b1.Matches(a1)  == true
			// b1.Matches(a2)  == true
			// b2.Matches(a1)  == true
			// b2.Matches(a2)  == true

			if (identifier == null)
				return false;

			if (identifier == this)
				return true;

			if (Verbatim && !identifier.Verbatim)
			{
				// They will not match

				return false;
			}

			if (Verbatim && identifier.Verbatim)
			{
				// Both are verbatim, compare the string by chars.

				return Text == identifier.Text;
			}

			// The left identifier is not verbatim, compare the names case insensitive.

			return String.Compare(Text, identifier.Text, StringComparison.OrdinalIgnoreCase) == 0;
		}

		public override string ToString()
		{
			return ToSource();
		}
	}
}