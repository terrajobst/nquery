using System;
using System.Text;

namespace NQuery.Runtime
{
	public abstract class MethodBinding : InvocableBinding
	{
		protected MethodBinding()
		{
		}
		
		public override string GetFullName()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(DeclaringType.Name);
			sb.Append(".");
			sb.Append(Name);
			sb.Append("(");
			sb.Append(FormattingHelpers.FormatTypeList(GetParameterTypes()));
			sb.Append(")");
			return sb.ToString();
		}
				
		public sealed override BindingCategory Category
		{
			get { return BindingCategory.Method; }
		}
		
		public abstract Type DeclaringType { get; }
		public abstract object Invoke(object target, object[] arguments);
	}
}