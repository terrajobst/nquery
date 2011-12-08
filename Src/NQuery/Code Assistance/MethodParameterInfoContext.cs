using System;

using NQuery.Runtime;

namespace NQuery.CodeAssistance
{
	internal sealed class MethodParameterInfoContext : ParameterInfoContext
	{
		private Scope _scope;
		private Type _expressionType;
		private Identifier _methodName;

		public MethodParameterInfoContext(SourceLocation sourceLocation, int parameterIndex, Scope scope, Type expressionType, Identifier methodName)
			: base(sourceLocation, parameterIndex)
		{
			_scope = scope;
			_expressionType = expressionType;
			_methodName = methodName;
		}

		public override void Enumerate(IParameterInfoAcceptor acceptor)
		{
			if (acceptor == null)
				throw ExceptionBuilder.ArgumentNull("acceptor");

			foreach (MethodBinding method in _scope.DataContext.MetadataContext.FindMethod(_expressionType, _methodName))
				acceptor.AcceptMethod(method);
		}
	}
}