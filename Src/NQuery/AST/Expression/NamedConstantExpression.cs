using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class NamedConstantExpression : ConstantExpression
	{
		private ConstantBinding _constantBinding;

		public NamedConstantExpression(ConstantBinding constantBinding)
		{
			_constantBinding = constantBinding;
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.NamedConstantExpression; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			return new NamedConstantExpression(_constantBinding);
		}

		public ConstantBinding Constant
		{
			get { return _constantBinding; }
		}

		public override Type ExpressionType
		{
			get { return _constantBinding.DataType; }
		}

		public override object GetValue()
		{
			object result;

			try
			{
				result = _constantBinding.Value;
			}
			catch (NQueryException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.ConstantBindingGetValueFailed(ex);
			}

			return NullHelper.UnifyNullRepresentation(result);
		}
	}
}