using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace NQuery.Runtime
{
	/// <summary>
	/// Utility class for performing soundex algorithm.
	/// 
	/// The Soundex algorithm is used to convert a word to a
	/// code based upon the phonetic sound of the word.
	/// 
	/// The soundex algorithm is outlined below:
	///     Rule 1. Keep the first character of the name.
	///     Rule 2. Perform a transformation on each remaining characters:
	///                 A,E,I,O,U,Y     = A
	///                 H,W             = S
	///                 B,F,P,V         = 1
	///                 C,G,J,K,Q,S,X,Z = 2
	///                 D,T             = 3
	///                 L               = 4
	///                 M,N             = 5
	///                 R               = 6
	///     Rule 3. If a character is the same as the previous, do not include in the code.
	///     Rule 4. If character is "A" or "S" do not include in the code.
	///     Rule 5. If a character is blank, then do not include in the code.
	///     Rule 6. A soundex code must be exactly 4 characters long.  If the
	///             code is too short then pad with zeros, otherwise truncate.
	/// </summary>
	internal static class Soundex
	{
		/// <summary>
		/// Return the soundex code for a given string.
		/// </summary>
		public static string GetCode(string text)
		{
			text = text.ToUpper(CultureInfo.CurrentCulture);
			StringBuilder sb = new StringBuilder(8);

			if (text.Length > 0)
			{
				// Rule 1. Keep the first character of the word
				sb.Append(text[0]);
			}

			// Rule 2. Perform a transformation on each remaining characters
			for (int i = 1; i < text.Length && sb.Length <= 4; i++)
			{
				char transformedChar = Transform(text[i]);

				// Rule 3. If a character is the same as the previous, do not include in code
				if (transformedChar != sb[sb.Length - 1])
				{
					// Rule 4. If character is "A" or "S" do not include in code
					if (transformedChar != 'A' && transformedChar != 'S')
					{
						// Rule 5. If a character is blank, then do not include in code 
						if (transformedChar != ' ')
						{
							sb.Append(transformedChar);
						}
					}
				}
			}

			// Rule 6. A soundex code must be exactly 4 characters long.  If the
			//         code is too short then pad with zeros, otherwise truncate.

			sb.Append("0000");

			return sb.ToString(0, 4);
		}

		/// <summary>
		/// Transform the A-Z alphabetic characters to the appropriate soundex code.
		/// </summary>
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private static char Transform(char c)
		{
			switch (c)
			{
				case 'A':
				case 'E':
				case 'I':
				case 'O':
				case 'U':
				case 'Y':
					return 'A';
				case 'H':
				case 'W':
					return 'S';
				case 'B':
				case 'F':
				case 'P':
				case 'V':
					return '1';
				case 'C':
				case 'G':
				case 'J':
				case 'K':
				case 'Q':
				case 'S':
				case 'X':
				case 'Z':
					return '2';
				case 'D':
				case 'T':
					return '3';
				case 'L':
					return '4';
				case 'M':
				case 'N':
					return '5';
				case 'R':
					return '6';

				default:
					return ' ';
			}
		}
	}
}