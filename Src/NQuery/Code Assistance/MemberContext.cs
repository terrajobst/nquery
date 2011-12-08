using System;
using System.Collections.Generic;

using NQuery.Compilation;
using NQuery.Runtime;

namespace NQuery.CodeAssistance
{
	internal sealed class MemberContext : MemberCompletionContext
	{
		private Scope _scope;
		private ExpressionNode _expressionBeforeDot;

		public MemberContext(SourceLocation sourceLocation, Identifier remainingPart, Scope scope, ExpressionNode expressionBeforeDot)
			: base(sourceLocation, remainingPart)
		{
			_scope = scope;
			_expressionBeforeDot = expressionBeforeDot;
		}

		public override void Enumerate(IMemberCompletionAcceptor acceptor)
		{
			if (acceptor == null)
				throw ExceptionBuilder.ArgumentNull("acceptor");
			
			NamedConstantExpression namedConstantExpression = _expressionBeforeDot as NamedConstantExpression;
			ParameterExpression parameterExpression = _expressionBeforeDot as ParameterExpression;

			IList<PropertyBinding> properties = null;

			if (namedConstantExpression == null && parameterExpression == null)
			{
				// The properties must be provided by a regular property provider.

				IPropertyProvider propertyProvider = _scope.DataContext.MetadataContext.PropertyProviders[_expressionBeforeDot.ExpressionType];

				if (propertyProvider != null)
					properties = propertyProvider.GetProperties(_expressionBeforeDot.ExpressionType);
			}
			else
			{
				// The expression before the dot is named constant or a parameter. In both cases, the properties 
				// could be contributed as custom properties.

				if (namedConstantExpression != null)
					properties = namedConstantExpression.Constant.CustomProperties;
				else
					properties = parameterExpression.Parameter.CustomProperties;
			}

			if (properties != null)
				foreach (PropertyBinding propertyBinding in properties)
					acceptor.AcceptProperty(propertyBinding);
				
			// Now contribute any methods

			IMethodProvider methodProvider = _scope.DataContext.MetadataContext.MethodProviders[_expressionBeforeDot.ExpressionType];
				
			if (methodProvider != null)
			{
				MethodBinding[] methods = methodProvider.GetMethods(_expressionBeforeDot.ExpressionType);
				foreach (MethodBinding methodBinding in  methods)
					acceptor.AcceptMethod(methodBinding);
			}
		}
	}
}