using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NQuery.Runtime
{
	public static class BuiltInFunctions 
	{
		#region Math

		#region ABS

		[FunctionBinding("ABS", IsDeterministic = true)]
		public static decimal Abs(decimal value)
		{
			return Math.Abs(value);
		}

		[FunctionBinding("ABS", IsDeterministic = true)]
		public static double Abs(double value)
		{
			return Math.Abs(value);
		}

		[FunctionBinding("ABS", IsDeterministic = true)]
		public static float Abs(float value)
		{
			return Math.Abs(value);
		}

		[FunctionBinding("ABS", IsDeterministic = true)]
		public static long Abs(long value)
		{
			return Math.Abs(value);
		}

		[FunctionBinding("ABS", IsDeterministic = true)]
		public static int Abs(int value)
		{
			return Math.Abs(value);
		}

		[FunctionBinding("ABS", IsDeterministic = true)]
		public static short Abs(short value)
		{
			return Math.Abs(value);
		}

		[FunctionBinding("ABS", IsDeterministic = true)]
		public static sbyte Abs(sbyte value)
		{
			return Math.Abs(value);
		}

		#endregion

		[FunctionBinding("ACOS", IsDeterministic = true)]
		public static double Acos(double value)
		{
			return Math.Acos(value);
		}

		[FunctionBinding("ASIN", IsDeterministic = true)]
		public static double Asin(double value)
		{
			return Math.Asin(value);
		}

		[FunctionBinding("ATAN", IsDeterministic = true)]
		public static double Atan(double value)
		{
			return Math.Atan(value);
		}

		[FunctionBinding("CEILING", IsDeterministic = true)]
		public static double Ceiling(double value)
		{
			return Math.Ceiling(value);
		}

		[FunctionBinding("COS", IsDeterministic = true)]
		public static double Cos(double value)
		{
			return Math.Cos(value);
		}

		[FunctionBinding("COSH", IsDeterministic = true)]
		public static double Cosh(double value)
		{
			return Math.Cosh(value);
		}

		[FunctionBinding("EXP", IsDeterministic = true)]
		public static double Exp(double value)
		{
			return Math.Exp(value);
		}

		[FunctionBinding("FLOOR", IsDeterministic = true)]
		public static double Floor(double value)
		{
			return Math.Floor(value);
		}

		#region Round

		[FunctionBinding("ROUND", IsDeterministic = true)]
		public static double Round(double value)
		{
			return Math.Round(value, MidpointRounding.AwayFromZero);
		}

		[FunctionBinding("ROUND", IsDeterministic = true)]
		public static double Round(double value, int digits)
		{
			return Math.Round(value, digits, MidpointRounding.AwayFromZero);
		}

		[FunctionBinding("ROUND", IsDeterministic = true)]
		public static decimal Round(decimal value)
		{
			return Math.Round(value, MidpointRounding.AwayFromZero);
		}

		[FunctionBinding("ROUND", IsDeterministic = true)]
		public static decimal Round(decimal value, int digits)
		{
			return Math.Round(value, digits, MidpointRounding.AwayFromZero);
		}

		#endregion

		#region Log

		[FunctionBinding("LOG", IsDeterministic = true)]
		public static double Log(double value)
		{
			return Math.Log(value);
		}

		[FunctionBinding("LOG", IsDeterministic = true)]
		public static double Log(double value, double newBase)
		{
			return Math.Log(value, newBase);
		}

		#endregion

		[FunctionBinding("LOG10", IsDeterministic = true)]
		public static double Log10(double value)
		{
			return Math.Log10(value);
		}

		[FunctionBinding("SIN", IsDeterministic = true)]
		public static double Sin(double value)
		{
			return Math.Sin(value);
		}

		[FunctionBinding("SINH", IsDeterministic = true)]
		public static double Sinh(double value)
		{
			return Math.Sinh(value);
		}

		[FunctionBinding("SQRT", IsDeterministic = true)]
		public static double Sqrt(double value)
		{
			return Math.Sqrt(value);
		}

		[FunctionBinding("TAN", IsDeterministic = true)]
		public static double Tan(double value)
		{
			return Math.Tan(value);
		}

		[FunctionBinding("TANH", IsDeterministic = true)]
		public static double Tanh(double value)
		{
			return Math.Tanh(value);
		}

		[FunctionBinding("POW", IsDeterministic = true)]
		public static double Pow(double basis, double exponent)
		{
			return Math.Pow(basis, exponent);
		}

		#region SIGN

		[FunctionBinding("SIGN", IsDeterministic = true)]
		public static int Sign(decimal value)
		{
			return Math.Sign(value);
		}

		[FunctionBinding("SIGN", IsDeterministic = true)]
		public static int Sign(double value)
		{
			return Math.Sign(value);
		}

		[FunctionBinding("SIGN", IsDeterministic = true)]
		public static int Sign(float value)
		{
			return Math.Sign(value);
		}

		[FunctionBinding("SIGN", IsDeterministic = true)]
		public static int Sign(long value)
		{
			return Math.Sign(value);
		}

		[FunctionBinding("SIGN", IsDeterministic = true)]
		public static int Sign(int value)
		{
			return Math.Sign(value);
		}

		[FunctionBinding("SIGN", IsDeterministic = true)]
		public static int Sign(short value)
		{
			return Math.Sign(value);
		}

		[FunctionBinding("SIGN", IsDeterministic = true)]
		public static int Sign(sbyte value)
		{
			return Math.Sign(value);
		}

		#endregion

		#endregion

		#region Conversion

		[FunctionBinding("TO_BOOLEAN", IsDeterministic=true)]
		public static bool ToBoolean(object value)
		{
			if (value == null)
				return false;

			try
			{
				return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.ConversionFailed(typeof(bool), value, ex);
			}
		}

		[FunctionBinding("TO_BYTE", IsDeterministic=true)]
		public static byte ToByte(object value)
		{
			if (value == null)
				return 0;

			try
			{
				return Convert.ToByte(value, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.ConversionFailed(typeof(byte), value, ex);
			}
		}

		[FunctionBinding("TO_CHAR", IsDeterministic=true)]
		public static char ToChar(object value)
		{
			if (value == null)
				return (char) 0;

			try
			{
				return Convert.ToChar(value, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.ConversionFailed(typeof(char), value, ex);
			}
		}

		[FunctionBinding("TO_DATETIME", IsDeterministic=true)]
		public static DateTime ToDateTime(object value)
		{
			if (value == null)
				return DateTime.MinValue;

			try
			{
				return Convert.ToDateTime(value, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.ConversionFailed(typeof(DateTime), value, ex);
			}
		}

		[FunctionBinding("TO_DECIMAL", IsDeterministic=true)]
		public static decimal ToDecimal(object value)
		{
			if (value == null)
				return 0;

			try
			{
				return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.ConversionFailed(typeof(decimal), value, ex);
			}
		}

		[FunctionBinding("TO_DOUBLE", IsDeterministic=true)]
		public static double ToDouble(object value)
		{
			if (value == null)
				return 0;

			try
			{
				return Convert.ToDouble(value, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.ConversionFailed(typeof(double), value, ex);
			}
		}

		[FunctionBinding("TO_INT16", IsDeterministic=true)]
		public static short ToInt16(object value)
		{
			if (value == null)
				return 0;

			try
			{
				return Convert.ToInt16(value, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.ConversionFailed(typeof(short), value, ex);
			}
		}

		[FunctionBinding("TO_INT32", IsDeterministic=true)]
		public static int ToInt32(object value)
		{
			if (value == null)
				return 0;

			try
			{
				return Convert.ToInt32(value, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.ConversionFailed(typeof(int), value, ex);
			}
		}

		[FunctionBinding("TO_INT64", IsDeterministic=true)]
		public static long ToInt64(object value)
		{
			if (value == null)
				return 0;

			try
			{
				return Convert.ToInt64(value, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.ConversionFailed(typeof(long), value, ex);
			}
		}

		[FunctionBinding("TO_SBYTE", IsDeterministic=true)]
		public static sbyte ToSByte(object value)
		{
			if (value == null)
				return 0;

			try
			{
				return Convert.ToSByte(value, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.ConversionFailed(typeof(sbyte), value, ex);
			}
		}

		[FunctionBinding("TO_SINGLE", IsDeterministic=true)]
		public static float ToSingle(object value)
		{
			if (value == null)
				return 0;

			try
			{
				return Convert.ToSingle(value, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.ConversionFailed(typeof(float), value, ex);
			}
		}

		[FunctionBinding("TO_STRING", IsDeterministic=true)]
		public static string ToString(object value)
		{
			if (value == null)
				return null;

			try
			{
				return Convert.ToString(value, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.ConversionFailed(typeof(string), value, ex);
			}
		}

		[FunctionBinding("TO_UINT16", IsDeterministic=true)]
		public static ushort ToUInt16(object value)
		{
			if (value == null)
				return 0;

			try
			{
				return Convert.ToUInt16(value, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.ConversionFailed(typeof(ushort), value, ex);
			}
		}

		[FunctionBinding("TO_UINT32", IsDeterministic=true)]
		public static uint ToUInt32(object value)
		{
			if (value == null)
				return 0;

			try
			{
				return Convert.ToUInt32(value, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.ConversionFailed(typeof(uint), value, ex);
			}
		}

		[FunctionBinding("TO_UINT64", IsDeterministic=true)]
		public static ulong ToUInt64(object value)
		{
			if (value == null)
				return 0;

			try
			{
				return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.ConversionFailed(typeof(ulong), value, ex);
			}
		}

		#endregion

		#region String

		[FunctionBinding("SOUNDEX", IsDeterministic = true)]
		public static string GetSoundexCode(string text)
		{
			if (text == null)
				return null;

			try
			{
				return Soundex.GetCode(text);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}

		[FunctionBinding("LEN", IsDeterministic = true)]
		public static int StringLength(string text)
		{
			if (text == null)
				return 0;

			return text.Length;
		}

		[FunctionBinding("CHARINDEX", IsDeterministic = true)]
		public static int CharIndex(string chars, string text)
		{
			if (chars == null || text == null)
				return 0;

			if (chars.Length == 0 || text.Length == 0)
				return 0;

			try
			{
				return text.IndexOf(chars, StringComparison.CurrentCulture) + 1;
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}

		[FunctionBinding("SUBSTRING", IsDeterministic = true)]
		public static string Substring(string text, int start, int length)
		{
			if (length < 0)
				throw ExceptionBuilder.ArgumentOutOfRange("length", length, 0, Int32.MaxValue);

			if (text == null)
				return null;

			if (start == 0 || text.Length == 0)
				return String.Empty;

			if (start > text.Length)
				return String.Empty;

			if (start + length - 1 > text.Length)
				length = text.Length - start + 1;

			try
			{
				return text.Substring(start - 1, length);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}

		[FunctionBinding("SUBSTRING", IsDeterministic = true)]
		public static string Substring(string text, int start)
		{
			if (text == null)
				return null;

			try
			{
				return Substring(text, start, text.Length);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}

		[FunctionBinding("UPPER", IsDeterministic = true)]
		public static string Upper(string text)
		{
			if (text == null)
				return null;

			try
			{
				return text.ToUpper(CultureInfo.CurrentCulture);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}

		[FunctionBinding("LOWER", IsDeterministic = true)]
		public static string Lower(string text)
		{
			if (text == null)
				return null;

			try
			{
				return text.ToLower(CultureInfo.CurrentCulture);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}

		[FunctionBinding("TRIM", IsDeterministic = true)]
		public static string Trim(string text)
		{
			if (text == null)
				return null;

			try
			{
				return text.Trim();
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}

		[FunctionBinding("LTRIM", IsDeterministic = true)]
		public static string LTrim(string text)
		{
			if (text == null)
				return null;

			try
			{
				return text.TrimStart(' ', '\t');
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}

		[FunctionBinding("RTRIM", IsDeterministic = true)]
		public static string RTrim(string text)
		{
			if (text == null)
				return null;

			try
			{
				return text.TrimEnd(' ', '\t');
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}
		
		[FunctionBinding("REPLACE")]
		public static string Replace(string text, string oldValue, string newValue)
		{
			if (text == null || oldValue == null || newValue == null)
				return null;

			try
			{
				return text.Replace(oldValue, newValue);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}
		
		[FunctionBinding("REGEX_REPLACE")]
		public static string RegexReplace(string text, string pattern, string replacementPattern)
		{
			if (text == null || pattern == null || replacementPattern == null)
				return null;

			Regex regex;
			try
			{
				regex = new Regex(pattern);
			}
			catch (ArgumentException ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
			
			return regex.Replace(text, replacementPattern);
		}
		
		[FunctionBinding("REGEX_MATCH")]
		public static bool RegexMatch(string text, string pattern)
		{
			if (text == null || pattern == null)
				return false;

			try
			{
				Regex regex = new Regex(pattern);
				return regex.IsMatch(text);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}
		
		[FunctionBinding("REGEX_ESCAPE")]
		public static string RegexEscape(string text)
		{
			if (text == null)
				return null;

			try
			{
				return Regex.Escape(text);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}
		
		[FunctionBinding("REGEX_UNESCAPE")]
		public static string RegexUnescape(string text)
		{
			if (text == null)
				return null;

			try
			{
				return Regex.Unescape(text);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}

		[FunctionBinding("FORMAT")]
		[SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "1#")]
		public static string Format(object value, string format)
		{
			try
			{
				string embeddedFormatString = String.Format(CultureInfo.InvariantCulture, "{{0:{0}}}", format);
				return String.Format(CultureInfo.CurrentCulture, embeddedFormatString, value);
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}

		[FunctionBinding("REPLICATE")]
		public static string Replicate(string text, int count)
		{
			if (text == null)
				return null;
			
			StringBuilder sb = new StringBuilder(text.Length * count);
			for (int i = 0; i < count; i++)
				sb.Append(text);			
			return sb.ToString();
		}

		[FunctionBinding("REVERSE")]
		public static string Reverse(string text)
		{
			if (text == null)
				return null;
			
			StringBuilder sb = new StringBuilder(text.Length);
			for (int i = text.Length - 1; i >= 0; i--)
				sb.Append(text[i]);			
			return sb.ToString();
		}

		[FunctionBinding("LEFT")]
		public static string Left(string text, int numberOfChars)
		{
			if (text == null)
				return null;

			if (numberOfChars > text.Length)
				numberOfChars = text.Length;

			return text.Substring(0, numberOfChars);
		}

		[FunctionBinding("RIGHT")]
		public static string Right(string text, int numberOfChars)
		{
			if (text == null)
				return null;

			if (numberOfChars > text.Length)
				numberOfChars = text.Length;
			
			return text.Substring(text.Length - numberOfChars, numberOfChars);
		}

		[FunctionBinding("SPACE")]
		public static string Space(int numberOfSpaces)
		{
			if (numberOfSpaces <= 0)
				return String.Empty;

			return Replicate(" ", numberOfSpaces);
		}

		[FunctionBinding("LPAD")]
		public static string LPad(string text, int totalWidth)
		{
			if (text == null)
				return null;

			return text.PadLeft(totalWidth);
		}

		[FunctionBinding("RPAD")]
		public static string RPad(string text, int totalWidth)
		{
			if (text == null)
				return null;

			return text.PadRight(totalWidth);
		}
		
		#endregion
		
		#region Date
		
		[FunctionBinding("GETDATE")]
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public static DateTime GetDate()
		{
			try
			{
				return DateTime.Now;
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}

		[FunctionBinding("GETUTCDATE")]
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public static DateTime GetUtcDate()
		{
			try
			{
				return DateTime.Now.ToUniversalTime();
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}
		
		[FunctionBinding("DAY")]
		public static int GetDay(DateTime dateTime)
		{
			try
			{
				return dateTime.Day;
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}
		
		[FunctionBinding("MONTH")]
		public static int GetMonth(DateTime dateTime)
		{
			try
			{
				return dateTime.Month;
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}

		[FunctionBinding("YEAR")]
		public static int GetYear(DateTime dateTime)
		{
			try
			{
				return dateTime.Year;
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.RuntimeError(ex);
			}
		}

		#endregion
	}
}
