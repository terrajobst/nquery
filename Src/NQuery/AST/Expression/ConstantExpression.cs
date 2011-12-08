using System;
using System.Globalization;

namespace NQuery.Compilation
{
	internal abstract class ConstantExpression : ExpressionNode
	{
		public PrimitiveType PrimitiveType
		{
			get
			{
				if (ExpressionType == null)
					return PrimitiveType.Null;

				return Binder.GetPrimitiveType(ExpressionType);
			}
		}

		public bool IsNullValue
		{
			get { return PrimitiveType == PrimitiveType.Null; }
		}

		public bool IsBooleanValue
		{
			get { return PrimitiveType == PrimitiveType.Boolean; }
		}

		public bool IsInt32Value
		{
			get { return PrimitiveType == PrimitiveType.Int32; }
		}

		public bool IsInt64Value
		{
			get { return PrimitiveType == PrimitiveType.Int64; }
		}

		public bool IsDoubleValue
		{
			get { return PrimitiveType == PrimitiveType.Double; }
		}

		public bool IsStringValue
		{
			get { return PrimitiveType == PrimitiveType.String; }
		}

		public bool IsDateTimeValue
		{
			get { return ExpressionType == typeof(DateTime); }
		}

		public bool AsBoolean
		{
			get { return Convert.ToBoolean(GetValue(), CultureInfo.InvariantCulture); }
		}

		public int AsInt32
		{
			get { return Convert.ToInt32(GetValue(), CultureInfo.InvariantCulture); }
		}

		public long AsInt64
		{
			get { return Convert.ToInt64(GetValue(), CultureInfo.InvariantCulture); }
		}

		public double AsDouble
		{
			get { return Convert.ToDouble(GetValue(), CultureInfo.InvariantCulture); }
		}

		public string AsString
		{
			get { return Convert.ToString(GetValue(), CultureInfo.InvariantCulture); }
		}

		public DateTime AsDateTime
		{
			get { return Convert.ToDateTime(GetValue(), CultureInfo.InvariantCulture); }
		}
	}
}