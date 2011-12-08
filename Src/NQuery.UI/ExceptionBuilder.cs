using System;
using System.Globalization;

namespace NQuery.UI
{
	internal static class ExceptionBuilder
	{
		public static NQueryException UnhandledCaseLabel(object value)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.UnhandledCaseLabel, value);
			return new NQueryException(message);
		}
	}
}
