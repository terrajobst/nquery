using System;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class ILParameterRegisterer : StandardVisitor
	{
		private ILEmitContext _ilEmitContext;

		public ILParameterRegisterer(ILEmitContext ilEmitContext)
		{
			_ilEmitContext = ilEmitContext;
		}

		public override AstNode Visit(AstNode node)
		{
			switch (node.NodeType)
			{
				case AstNodeType.Literal:
				case AstNodeType.UnaryExpression:
				case AstNodeType.BinaryExpression:
				case AstNodeType.IsNullExpression:
				case AstNodeType.CastExpression:
				case AstNodeType.CaseExpression:
				case AstNodeType.NullIfExpression:
				case AstNodeType.ParameterExpression:
				case AstNodeType.PropertyAccessExpression:
				case AstNodeType.FunctionInvocationExpression:
				case AstNodeType.MethodInvocationExpression:
				case AstNodeType.RowBufferEntryExpression:
					return base.Visit(node);

				default:
					throw ExceptionBuilder.UnhandledCaseLabel(node.NodeType);
			}
		}

		public override LiteralExpression VisitLiteralValue(LiteralExpression expression)
		{
			_ilEmitContext.AddParameter(expression, expression.GetValue(), expression.ExpressionType);
			return expression;
		}

		public override ExpressionNode VisitParameterExpression(ParameterExpression expression)
		{
			_ilEmitContext.AddParameter(expression, expression.Parameter, typeof(ParameterBinding));
			return expression;
		}

		public override ExpressionNode VisitPropertyAccessExpression(PropertyAccessExpression expression)
		{
			base.VisitPropertyAccessExpression(expression);

			bool isReflectionBinding = expression.Property is ReflectionPropertyBinding ||
			                           expression.Property is ReflectionFieldBinding;
			if (!isReflectionBinding)
				_ilEmitContext.AddParameter(expression, expression.Property, typeof(PropertyBinding));

			return expression;
		}

		public override ExpressionNode VisitFunctionInvocationExpression(FunctionInvocationExpression expression)
		{
			base.VisitFunctionInvocationExpression(expression);

			ReflectionFunctionBinding reflectionFunctionBinding = expression.Function as ReflectionFunctionBinding;
			if (reflectionFunctionBinding != null)
			{
				if (reflectionFunctionBinding.Instance != null)
					_ilEmitContext.AddParameter(expression, reflectionFunctionBinding.Instance, reflectionFunctionBinding.Instance.GetType());
			}
			else
			{
				_ilEmitContext.AddParameter(expression, expression.Function, typeof (FunctionBinding));
				object[] args = new object[expression.Arguments.Length];
				_ilEmitContext.AddParameter(expression, args, typeof(object[]));
			}
			return expression;
		}

		public override ExpressionNode VisitMethodInvocationExpression(MethodInvocationExpression expression)
		{
			base.VisitMethodInvocationExpression(expression);

			bool isReflectionBinding = expression.Method is ReflectionMethodBinding;
			if (!isReflectionBinding)
			{
				_ilEmitContext.AddParameter(expression, expression.Method, typeof (MethodBinding));
				object[] args = new object[expression.Arguments.Length];
				_ilEmitContext.AddParameter(expression, args, typeof(object[]));
			}

			return expression;
		}

		public override ExpressionNode VisitRowBufferEntryExpression(RowBufferEntryExpression expression)
		{
			_ilEmitContext.AddParameter(expression, expression.RowBuffer, typeof(object[]));
			return expression;
		}
	}
}