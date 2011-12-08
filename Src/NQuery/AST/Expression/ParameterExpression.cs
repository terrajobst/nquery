using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class ParameterExpression : ExpressionNode
	{
		private Identifier _name;
		private SourceRange _nameSourceRange;
		private ParameterBinding _parameter;

		public ParameterExpression()
		{
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.ParameterExpression; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			ParameterExpression result = new ParameterExpression();
			result.Name = _name;
			result.NameSourceRange = _nameSourceRange;
			result.Parameter = _parameter;
			return result;
		}

		public override Type ExpressionType
		{
			get
			{
				if (_parameter == null)
					return null;

				return _parameter.DataType;
			}
		}

		public override object GetValue()
		{
			if (_parameter == null)
				return null;

			object result;

			try
			{
				result = _parameter.Value;
			}
			catch (NQueryException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.ParameterBindingGetValueFailed(ex);
			}

			return NullHelper.UnifyNullRepresentation(result);
		}

		public Identifier Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public SourceRange NameSourceRange
		{
			get { return _nameSourceRange; }
			set { _nameSourceRange = value; }
		}

		public ParameterBinding Parameter
		{
			get { return _parameter; }
			set { _parameter = value; }
		}
	}
}