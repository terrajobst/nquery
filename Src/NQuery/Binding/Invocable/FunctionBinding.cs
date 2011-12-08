using System;
using System.Text;

namespace NQuery.Runtime
{
	public abstract class FunctionBinding : InvocableBinding
	{
		protected FunctionBinding()
		{
		}

		public override string GetFullName()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Name);
			sb.Append("(");
			sb.Append(FormattingHelpers.FormatTypeList(GetParameterTypes()));
			sb.Append(")");
			return sb.ToString();
		}

		public sealed override BindingCategory Category
		{
			get { return BindingCategory.Function; }
		}

		public abstract object Invoke(object[] arguments);
	}
}