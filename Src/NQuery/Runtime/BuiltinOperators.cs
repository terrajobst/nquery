using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NQuery.Runtime
{
	internal static class BuiltInOperators
	{
		#region Null Placeholders

		// NOTE: Don't remove these methods!
		//       They are used in the Binder. The reflected MethodInfo objects of theses
		//       methods is returned to unary and binary operators where one operand is null.

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "a")]
		public static DBNull Placeholder(DBNull a)
		{
			throw ExceptionBuilder.InternalError("Placeholder(DBNull) should never get called.");
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "a")]
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "b")]
		public static DBNull Placeholder(DBNull a, DBNull b)
		{
			throw ExceptionBuilder.InternalError("Placeholder(DBNull,DBNull) should never get called.");
		}

		#endregion

		#region General Equality Checks

		public static bool op_Equality(object a, object b)
		{
			return Equals(a, b);
		}

		public static bool op_Inequality(object a, object b)
		{
			return !Equals(a, b);
		}

		#endregion

		#region Enum Equality

		public static bool op_Equality(Enum a, string b)
		{
			string enumAsString = a.ToString();
			return enumAsString == b;
		}

		public static bool op_Equality(string a, Enum b)
		{
			string enumAsString = b.ToString();
			return a == enumAsString;
		}

		public static bool op_Equality(Enum a, Int32 b)
		{
			return Convert.ToInt32(a, CultureInfo.InvariantCulture) == b;
		}

		public static bool op_Equality(Int32 a, Enum b)
		{
			return a == Convert.ToInt32(b, CultureInfo.InvariantCulture);
		}

		public static bool op_Equality(Enum a, Int64 b)
		{
			return Convert.ToInt64(a, CultureInfo.InvariantCulture) == b;
		}

		public static bool op_Equality(Int64 a, Enum b)
		{
			return a == Convert.ToInt64(b, CultureInfo.InvariantCulture);
		}

		public static bool op_Inequality(Enum a, string b)
		{
			string enumAsString = a.ToString();
			return enumAsString != b;
		}

		public static bool op_Inequality(string a, Enum b)
		{
			string enumAsString = b.ToString();
			return a != enumAsString;
		}

		public static bool op_Inequality(Enum a, Int32 b)
		{
			return Convert.ToInt32(a, CultureInfo.InvariantCulture) != b;
		}

		public static bool op_Inequality(Int32 a, Enum b)
		{
			return a != Convert.ToInt32(b, CultureInfo.InvariantCulture);
		}

		public static bool op_Inequality(Enum a, Int64 b)
		{
			return Convert.ToInt64(a, CultureInfo.InvariantCulture) != b;
		}

		public static bool op_Inequality(Int64 a, Enum b)
		{
			return a != Convert.ToInt64(b, CultureInfo.InvariantCulture);
		}

		#endregion

		#region Enum Bitwise Operators

		public static Enum op_BitwiseOr(Enum a, Enum b)
		{
			return (Enum)Enum.ToObject(a.GetType(), Convert.ToInt64(a, CultureInfo.InvariantCulture) | Convert.ToInt64(b, CultureInfo.InvariantCulture));
		}

		public static Enum op_BitwiseAnd(Enum a, Enum b)
		{
			return (Enum)Enum.ToObject(a.GetType(), Convert.ToInt64(a, CultureInfo.InvariantCulture) & Convert.ToInt64(b, CultureInfo.InvariantCulture));
		}

		#endregion

		#region Boolean

		public static bool op_Logicalnot(bool a)
		{
			return !a;
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "a")]
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "b")]
		public static bool op_LogicalOr(bool a, bool b)
		{
			throw ExceptionBuilder.InternalError("op_LogicalOr(bool, bool) should never get called.");
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "a")]
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "b")]
		public static bool op_LogicalAnd(bool a, bool b)
		{
			throw ExceptionBuilder.InternalError("op_LogicalAnd(bool, bool) should never get called.");
		}

		public static bool op_ExclusiveOr(bool a, bool b)
		{
			return a ^ b;
		}

		public static bool op_Equality(bool a, bool b)
		{
			return a == b;
		}

		public static bool op_Inequality(bool a, bool b)
		{
			return a != b;
		}

		#endregion

		#region String

		public static bool op_Equality(string a, string b)
		{
			return a == b;
		}

		public static bool op_Inequality(string a, string b)
		{
			return a != b;
		}

		public static bool op_LessThan(string a, string b)
		{
			return String.Compare(a, b, StringComparison.CurrentCulture) < 0;
		}

		public static bool op_LessThanOrEqual(string a, string b)
		{
			return String.Compare(a, b, StringComparison.CurrentCulture) <= 0;
		}

		public static bool op_GreaterThan(string a, string b)
		{
			return String.Compare(a, b, StringComparison.CurrentCulture) > 0;
		}

		public static bool op_GreaterThanOrEqual(string a, string b)
		{
			return String.Compare(a, b, StringComparison.CurrentCulture) >= 0;
		}

		public static string op_Addition(string a, string b)
		{
			return String.Concat(a, b);
		}

		public static bool op_SimilarTo(string str, string regex)
		{
			Regex r = new Regex(regex, RegexOptions.Multiline);
			return r.IsMatch(str);
		}

		public static bool op_Like(string str, string expr)
		{
			str = str.ToUpper(CultureInfo.CurrentCulture);

			StringBuilder sb = new StringBuilder();

			sb.Append('^');

			for (int i = 0; i < expr.Length; i++)
			{
				switch (expr[i])
				{
					case '%':
						sb.Append(".*");
						break;

					case '_':
						sb.Append('.');
						break;

					default:
						sb.Append(Char.ToUpper(expr[i], CultureInfo.CurrentCulture));
						break;
				}
			}

			sb.Append('$');

			Regex r = new Regex(sb.ToString(), RegexOptions.Singleline);
			return r.IsMatch(str);
		}

		public static bool op_SoundsLike(string left, string right)
		{
			if (left == null || right == null)
				return false;

			return Soundex.GetCode(left) == Soundex.GetCode(right);
		}

		#endregion

		#region Int32

		public static int op_UnaryPlus(int a)
		{
			return a;
		}

		public static int op_UnaryNegation(int a)
		{
			return -a;
		}

		public static int op_OnesComplement(int a)
		{
			return ~a;
		}

		public static bool op_Equality(int a, int b)
		{
			return a == b;
		}

		public static bool op_Inequality(int a, int b)
		{
			return a != b;
		}

		public static bool op_LessThan(int a, int b)
		{
			return a < b;
		}

		public static bool op_LessThanOrEqual(int a, int b)
		{
			return a <= b;
		}

		public static bool op_GreaterThan(int a, int b)
		{
			return a > b;
		}

		public static bool op_GreaterThanOrEqual(int a, int b)
		{
			return a >= b;
		}

		public static int op_LeftShift(int a, int b)
		{
			return a << b;
		}

		public static int op_RightShift(int a, int b)
		{
			return a >> b;
		}

		public static int op_BitwiseOr(int a, int b)
		{
			return a | b;
		}

		public static int op_BitwiseAnd(int a, int b)
		{
			return a & b;
		}

		public static int op_ExclusiveOr(int a, int b)
		{
			return a ^ b;
		}

		public static int op_Addition(int a, int b)
		{
			return a + b;
		}

		public static int op_Subtraction(int a, int b)
		{
			return a - b;
		}

		public static int op_Multiply(int a, int b)
		{
			return a * b;
		}

		public static int op_Division(int a, int b)
		{
			return a / b;
		}

		public static int op_Modulus(int a, int b)
		{
			return a % b;
		}

		public static int op_Power(int a, int b)
		{
			return (int) Math.Pow(a, b);
		}

		#endregion

		#region UInt32

		public static uint op_UnaryPlus(uint a)
		{
			return a;
		}

		public static uint op_OnesComplement(uint a)
		{
			return ~a;
		}

		public static bool op_Equality(uint a, uint b)
		{
			return a == b;
		}

		public static bool op_Inequality(uint a, uint b)
		{
			return a != b;
		}

		public static bool op_LessThan(uint a, uint b)
		{
			return a < b;
		}

		public static bool op_LessThanOrEqual(uint a, uint b)
		{
			return a <= b;
		}

		public static bool op_GreaterThan(uint a, uint b)
		{
			return a > b;
		}

		public static bool op_GreaterThanOrEqual(uint a, uint b)
		{
			return a >= b;
		}

		public static uint op_LeftShift(uint a, int b)
		{
			return a << b;
		}

		public static uint op_RightShift(uint a, int b)
		{
			return a >> b;
		}
		public static uint op_BitwiseOr(uint a, uint b)
		{
			return a | b;
		}

		public static uint op_BitwiseAnd(uint a, uint b)
		{
			return a & b;
		}

		public static uint op_ExclusiveOr(uint a, uint b)
		{
			return a ^ b;
		}

		public static uint op_Addition(uint a, uint b)
		{
			return a + b;
		}

		public static uint op_Subtraction(uint a, uint b)
		{
			return a - b;
		}

		public static uint op_Multiply(uint a, uint b)
		{
			return a * b;
		}

		public static uint op_Division(uint a, uint b)
		{
			return a / b;
		}

		public static uint op_Modulus(uint a, uint b)
		{
			return a % b;
		}

		public static uint op_Power(uint a, uint b)
		{
			return (uint) Math.Pow(a, b);
		}

		#endregion

		#region Int64

		public static long op_UnaryPlus(long a)
		{
			return a;
		}

		public static long op_UnaryNegation(long a)
		{
			return -a;
		}

		public static long op_OnesComplement(long a)
		{
			return ~a;
		}

		public static bool op_Equality(long a, long b)
		{
			return a == b;
		}

		public static bool op_Inequality(long a, long b)
		{
			return a != b;
		}

		public static bool op_LessThan(long a, long b)
		{
			return a < b;
		}

		public static bool op_LessThanOrEqual(long a, long b)
		{
			return a <= b;
		}

		public static bool op_GreaterThan(long a, long b)
		{
			return a > b;
		}

		public static bool op_GreaterThanOrEqual(long a, long b)
		{
			return a >= b;
		}

		public static long op_LeftShift(long a, int b)
		{
			return a << b;
		}

		public static long op_RightShift(long a, int b)
		{
			return a >> b;
		}

		public static long op_BitwiseOr(long a, long b)
		{
			return a | b;
		}

		public static long op_BitwiseAnd(long a, long b)
		{
			return a & b;
		}

		public static long op_ExclusiveOr(long a, long b)
		{
			return a ^ b;
		}

		public static long op_Addition(long a, long b)
		{
			return a + b;
		}

		public static long op_Subtraction(long a, long b)
		{
			return a - b;
		}

		public static long op_Multiply(long a, long b)
		{
			return a * b;
		}

		public static long op_Division(long a, long b)
		{
			return a / b;
		}

		public static long op_Modulus(long a, long b)
		{
			return a % b;
		}

		public static long op_Power(long a, long b)
		{
			return (long) Math.Pow(a, b);
		}

		#endregion

		#region UInt64

		public static ulong op_UnaryPlus(ulong a)
		{
			return a;
		}

		public static ulong op_OnesComplement(ulong a)
		{
			return ~a;
		}

		public static bool op_Equality(ulong a, ulong b)
		{
			return a == b;
		}

		public static bool op_Inequality(ulong a, ulong b)
		{
			return a != b;
		}

		public static bool op_LessThan(ulong a, ulong b)
		{
			return a < b;
		}

		public static bool op_LessThanOrEqual(ulong a, ulong b)
		{
			return a <= b;
		}

		public static bool op_GreaterThan(ulong a, ulong b)
		{
			return a > b;
		}

		public static bool op_GreaterThanOrEqual(ulong a, ulong b)
		{
			return a >= b;
		}

		public static ulong op_LeftShift(ulong a, int b)
		{
			return a << b;
		}

		public static ulong op_RightShift(ulong a, int b)
		{
			return a >> b;
		}

		public static ulong op_BitwiseOr(ulong a, ulong b)
		{
			return a | b;
		}

		public static ulong op_BitwiseAnd(ulong a, ulong b)
		{
			return a & b;
		}

		public static ulong op_ExclusiveOr(ulong a, ulong b)
		{
			return a ^ b;
		}

		public static ulong op_Addition(ulong a, ulong b)
		{
			return a + b;
		}

		public static ulong op_Subtraction(ulong a, ulong b)
		{
			return a - b;
		}

		public static ulong op_Multiply(ulong a, ulong b)
		{
			return a * b;
		}

		public static ulong op_Division(ulong a, ulong b)
		{
			return a / b;
		}

		public static ulong op_Modulus(ulong a, ulong b)
		{
			return a % b;
		}

		public static ulong op_Power(ulong a, ulong b)
		{
			return (ulong) Math.Pow(a, b);
		}

		#endregion

		#region Single

		public static float op_UnaryPlus(float a)
		{
			return a;
		}

		public static float op_UnaryNegation(float a)
		{
			return -a;
		}

		public static bool op_Equality(float a, float b)
		{
			return a == b;
		}

		public static bool op_Inequality(float a, float b)
		{
			return a != b;
		}

		public static bool op_LessThan(float a, float b)
		{
			return a < b;
		}

		public static bool op_LessThanOrEqual(float a, float b)
		{
			return a <= b;
		}

		public static bool op_GreaterThan(float a, float b)
		{
			return a > b;
		}

		public static bool op_GreaterThanOrEqual(float a, float b)
		{
			return a >= b;
		}

		public static float op_Addition(float a, float b)
		{
			return a + b;
		}

		public static float op_Subtraction(float a, float b)
		{
			return a - b;
		}

		public static float op_Multiply(float a, float b)
		{
			return a * b;
		}

		public static float op_Division(float a, float b)
		{
			return a / b;
		}

		public static float op_Modulus(float a, float b)
		{
			return a % b;
		}

		public static float op_Power(float a, float b)
		{
			return (float) Math.Pow(a, b);
		}

		#endregion

		#region Double

		public static double op_UnaryPlus(double a)
		{
			return a;
		}

		public static double op_UnaryNegation(double a)
		{
			return -a;
		}

		public static bool op_Equality(double a, double b)
		{
			return a == b;
		}

		public static bool op_Inequality(double a, double b)
		{
			return a != b;
		}

		public static bool op_LessThan(double a, double b)
		{
			return a < b;
		}

		public static bool op_LessThanOrEqual(double a, double b)
		{
			return a <= b;
		}

		public static bool op_GreaterThan(double a, double b)
		{
			return a > b;
		}

		public static bool op_GreaterThanOrEqual(double a, double b)
		{
			return a >= b;
		}

		public static double op_Addition(double a, double b)
		{
			return a + b;
		}

		public static double op_Subtraction(double a, double b)
		{
			return a - b;
		}

		public static double op_Multiply(double a, double b)
		{
			return a * b;
		}

		public static double op_Division(double a, double b)
		{
			return a / b;
		}

		public static double op_Modulus(double a, double b)
		{
			return a % b;
		}

		public static double op_Power(double a, double b)
		{
			return Math.Pow(a, b);
		}

		#endregion

		#region DateTime and TimeSpan

		public static TimeSpan op_Division(TimeSpan a, int b)
		{
			return new TimeSpan(a.Ticks / b);
		}

		public static DateTime op_Addition(DateTime a, double b)
		{
			return a.AddDays(b);
		}

		#endregion
	}
}