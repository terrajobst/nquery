using System;
using System.Reflection;
using System.Text;

using NQuery.Runtime;

namespace NQuery
{
	internal static class FormattingHelpers
	{
		public static string GetFirstLine(string str)
		{
			if (str == null || str.Length == 0)
				return str;

			int length = 0;

			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] == '\n' || str[i] == '\b')
				{
					length = i;
					break;
				}
			}

			if (length > 0)
				return str.Substring(0, length);

			return str;
		}

		public static string FormatType(Type type)
		{
			return type.Name;
		}

		public static string FormatTypeList(Type[] argTypes)
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < argTypes.Length; i++)
			{
				if (i > 0)
					sb.Append(',');

				sb.Append(FormatType(argTypes[i]));
			}

			return sb.ToString();
		}

		public static string FormatFullyQualifiedTypeList(Type[] candidates)
		{
			StringBuilder sb = new StringBuilder();

			foreach (Type t in candidates)
			{
				if (sb.Length > 0)
					sb.Append(", ");

				sb.Append("'");
				sb.Append(t.AssemblyQualifiedName);
				sb.Append("'");
			}

			return sb.ToString();
		}

		public static string FormatColumnRefList(ColumnRefBinding[] columnRefBindings)
		{
			StringBuilder sb = new StringBuilder();

			foreach (ColumnRefBinding columnRefBinding in columnRefBindings)
			{
				if (sb.Length > 0)
					sb.Append(", ");

				sb.Append("'");
				sb.Append(columnRefBinding.GetFullName());
				sb.Append("'");
			}

			return sb.ToString();
		}

		public static string FormatMethodInfo(MethodInfo methodInfo)
		{
			ParameterInfo[] parameterInfos = methodInfo.GetParameters();

			StringBuilder sb = new StringBuilder();
			sb.Append(FormatType(methodInfo.DeclaringType));
			sb.Append(".");
			sb.Append(methodInfo.Name);
			sb.Append("(");
			for (int i = 0; i < parameterInfos.Length; i++)
			{
				if (i > 0)
					sb.Append(", ");

				sb.Append(FormatType(parameterInfos[i].ParameterType));
			}
			sb.Append(")");
			return sb.ToString();
		}

		public static string FormatBindingList(Binding[] bindings)
		{
			StringBuilder sb = new StringBuilder();
			foreach (Binding binding in bindings)
			{
				if (sb.Length > 0)
					sb.Append(", ");

				sb.Append(binding.GetFullName());
			}
			return sb.ToString();
		}

		public static string FormatBindingListWithCategory(Binding[] bindings)
		{
			StringBuilder sb = new StringBuilder();
			foreach (Binding binding in bindings)
			{
				if (sb.Length > 0)
					sb.Append(", ");

				sb.Append(binding.GetFullName());
				sb.Append(": ");
				sb.Append(binding.Category);
			}
			return sb.ToString();
		}
	}
}