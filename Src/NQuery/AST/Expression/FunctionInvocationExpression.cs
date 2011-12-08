using System;
using System.Collections.Generic;
using System.Reflection;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class FunctionInvocationExpression : ExpressionNode
	{
		private Identifier _name;
		private SourceRange _nameSourceRange;
		private ExpressionNode[] _arguments;
		private FunctionBinding _function;
		private bool _hasAsteriskModifier;

		public FunctionInvocationExpression()
		{
		}

		public FunctionInvocationExpression(Identifier identifier, ExpressionNode[] arguments, bool hasStarModifier)
			: this()
		{
			_name = identifier;
			_arguments = arguments;
			_hasAsteriskModifier = hasStarModifier;
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.FunctionInvocationExpression; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			FunctionInvocationExpression result = new FunctionInvocationExpression();
			result.Name = _name;
			result.NameSourceRange = _nameSourceRange;
			result.HasAsteriskModifier = _hasAsteriskModifier;
			result.Arguments = ArrayHelpers.CreateDeepCopyOfAstElementArray(_arguments, alreadyClonedElements);
			result.Function = _function;
			return result;
		}

		public override Type ExpressionType
		{
			get
			{
				if (_function == null)
					return null;

				return _function.ReturnType;
			}
		}

		public override object GetValue()
		{
			if (_function == null)
				return null;

			// BUG: Shouln't we ensure that all arguments are non-null?
			object[] argumentValues = new object[_arguments.Length];
			for (int i = 0; i < argumentValues.Length; i++)
				argumentValues[i] = _arguments[i].GetValue();

			object result;

			try
			{
				result = _function.Invoke(argumentValues);
			}
			catch (TargetInvocationException ex)
			{
				// Special handling for target invocation since we are only
				// interested in the inner one.
				
				if (ex.InnerException is NQueryException)
					throw ExceptionBuilder.RuntimeError(ex.InnerException);
				
				throw ExceptionBuilder.FunctionBindingInvokeFailed(ex.InnerException);
			}
			catch (NQueryException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.FunctionBindingInvokeFailed(ex);
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

		public ExpressionNode[] Arguments
		{
			get { return _arguments; }
			set { _arguments = value; }
		}

		public bool HasAsteriskModifier
		{
			get { return _hasAsteriskModifier; }
			set { _hasAsteriskModifier = value; }
		}

		public FunctionBinding Function
		{
			get { return _function; }
			set { _function = value; }
		}
	}
}