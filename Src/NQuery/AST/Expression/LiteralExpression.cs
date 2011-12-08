using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class LiteralExpression : ConstantExpression
	{
		private object _value;
		private Type _type;

		private LiteralExpression(object value)
		{
			if (value == null)
			{
				// _value = null, but this fires an FX Cop warning.
				_type = typeof(DBNull);
			}
			else
			{
				_value = value;
				_type = value.GetType();
			}
		}

		private LiteralExpression(object value, Type type)
		{
			_value = value;
			_type = type;
		}
		
		public override AstNodeType NodeType
		{
			get { return AstNodeType.Literal; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			return new LiteralExpression(_value);
		}

		public static LiteralExpression FromNull()
		{
			return new LiteralExpression(null);
		}

		public static LiteralExpression FromTypedNull(Type type)
		{
			return new LiteralExpression(null, type);
		}

		public static LiteralExpression FromTypedValue(object value, Type type)
		{
			return new LiteralExpression(value, type);
		}

		public static LiteralExpression FromBoolean(bool value)
		{
			return new LiteralExpression(value);
		}

		public static LiteralExpression FromInt32(int value)
		{
			return new LiteralExpression(value);
		}

		public static LiteralExpression FromInt64(long value)
		{
			return new LiteralExpression(value);
		}

		public static LiteralExpression FromDouble(double value)
		{
			return new LiteralExpression(value);
		}

		public static LiteralExpression FromString(string value)
		{
			return new LiteralExpression(value);
		}

		public static LiteralExpression FromDateTime(DateTime value)
		{
			return new LiteralExpression(value);
		}

		public override Type ExpressionType
		{
			get { return _type; }
		}

		public override object GetValue()
		{
			return _value;
		}
	}
}