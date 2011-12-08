using System;
using System.Collections.Generic;
using System.Reflection;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class MethodInvocationExpression : ExpressionNode
	{
		private ExpressionNode _target;
		private Identifier _name;
		private SourceRange _nameSourceRange;
		private ExpressionNode[] _arguments;
		private MethodBinding _method;

		public MethodInvocationExpression()
		{
		}
		
		public override Type ExpressionType
		{
			get
			{
				if (_method == null)
					return null;
				
				return _method.ReturnType;
			}
		}

		public override object GetValue()
		{
			if (_method == null)
				return null;

			object targetValue = _target.GetValue();

			if (NullHelper.IsNull(targetValue))
				return null;
			
			object[] argumentValues = new object[_arguments.Length];

			for (int i = 0; i < argumentValues.Length; i++)
				argumentValues[i] = _arguments[i].GetValue();

			object result;

			try
			{
				result = _method.Invoke(targetValue, argumentValues);
			}
			catch (TargetInvocationException ex)
			{
				// Special handling for target invocation since we are only
				// interested in the inner one.
				throw ExceptionBuilder.MethodBindingInvokeFailed(ex.InnerException);
			}
			catch (NQueryException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.MethodBindingInvokeFailed(ex);
			}

			return NullHelper.UnifyNullRepresentation(result);			
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.MethodInvocationExpression; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			MethodInvocationExpression result = new MethodInvocationExpression();

			result.Target = (ExpressionNode)_target.Clone(alreadyClonedElements);
			result.Name = _name;
			result.NameSourceRange = _nameSourceRange;
			
			result._arguments = new ExpressionNode[_arguments.Length];
			for (int i = 0; i < _arguments.Length; i++)
				result._arguments[i] = (ExpressionNode)_arguments[i].Clone(alreadyClonedElements);
			
			result._method = _method;
			
			return result;
		}

		public ExpressionNode Target
		{
			get { return _target; }
			set { _target = value; }
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

		public MethodBinding Method
		{
			get { return _method; }
			set { _method = value; }
		}
	}
}