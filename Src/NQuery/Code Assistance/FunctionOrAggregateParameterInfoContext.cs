using System;

using NQuery.Runtime;

namespace NQuery.CodeAssistance
{
	internal sealed class FunctionOrAggregateParameterInfoContext : ParameterInfoContext
	{
		private Scope _scope;
		private Identifier _functionName;

		public FunctionOrAggregateParameterInfoContext(SourceLocation sourceLocation, int parameterIndex, Scope scope, Identifier functionName)
			: base(sourceLocation, parameterIndex)
		{
			_scope = scope;
			_functionName = functionName;
		}

		public override void Enumerate(IParameterInfoAcceptor acceptor)
		{
			if (acceptor == null)
				throw ExceptionBuilder.ArgumentNull("acceptor");
			
			FunctionBinding[] functions = _scope.DataContext.Functions.Find(_functionName);

			if (functions != null && functions.Length > 0)
			{
				foreach (FunctionBinding function in functions)
					acceptor.AcceptFunction(function);
			}
			else
			{
				// Not a function, try an aggregate.

				AggregateBinding[] aggregates = _scope.DataContext.Aggregates.Find(_functionName);

				if (aggregates != null && aggregates.Length > 0)
				{
					foreach (AggregateBinding aggregate in aggregates)
						acceptor.AcceptAggregate(aggregate);
				}
			}
		}
	}
}