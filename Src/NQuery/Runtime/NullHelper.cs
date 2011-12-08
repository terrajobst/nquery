using System;
using System.Data.SqlTypes;

namespace NQuery.Runtime
{
	internal static class NullHelper
	{
		public static bool IsNull(object value)
		{
			if (value == null)
				return true;

			if (value == DBNull.Value)
				return true;

			INullable nullable = value as INullable;

			if (nullable != null)
				return nullable.IsNull;

			return false;
		}

		public static object UnifyNullRepresentation(object value)
		{
			if (IsNull(value))
				return null;

			return value;
		}
	}
}
