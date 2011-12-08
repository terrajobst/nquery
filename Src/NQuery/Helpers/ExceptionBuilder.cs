using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

using NQuery.Compilation;
using NQuery.Runtime;

namespace NQuery
{
	internal static class ExceptionBuilder
	{
		#region Internal CompilationErrors

		public static NQueryException InternalError(string format, params object[] args)
		{
			if (format == null)
				throw new ArgumentNullException("format");

			string formattedDetails = String.Format(CultureInfo.CurrentCulture, format, args);
			string message = String.Format(CultureInfo.CurrentCulture, Resources.InternalError, formattedDetails);
			return new NQueryException(message);
		}

		public static NQueryException InternalErrorGetValueNotSupported(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			return InternalError(Resources.InternalErrorGetValueNotSupported, type.FullName);
		}

		public static NQueryException UnhandledCaseLabel(object value)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.UnhandledCaseLabel, value);
			return new NQueryException(message);
		}

		#endregion

		#region Argument Errors

		public static ArgumentException ArgumentNull(string paramName)
		{
			if (paramName == null)
				throw new ArgumentNullException("paramName");

			return new ArgumentNullException(paramName);
		}

		public static ArgumentException ArgumentArrayMustNotBeEmpty(string paramName)
		{
			if (paramName == null)
				throw new ArgumentNullException("paramName");

			return new ArgumentException(Resources.ArgumentArrayMustNotBeEmpty, paramName);
		}

		public static ArgumentException ArgumentArrayMustHaveSameSize(string paramName, string paramNameOfReferenceArray)
		{
			if (paramName == null)
				throw new ArgumentNullException("paramName");

			if (paramNameOfReferenceArray == null)
				throw new ArgumentNullException("paramNameOfReferenceArray");

			string message = String.Format(CultureInfo.CurrentCulture, Resources.ArgumentArrayMustHaveSameSize, paramName, paramNameOfReferenceArray);
			throw new ArgumentException(message, paramName);
		}

		public static ArgumentException ArgumentOutOfRange(string paramName, object actualValue, object minValue, object maxValue)
		{
			if (paramName == null)
				throw new ArgumentNullException("paramName");

			if (actualValue == null)
				throw new ArgumentNullException("actualValue");

			if (minValue == null)
				throw new ArgumentNullException("minValue");

			if (maxValue == null)
				throw new ArgumentNullException("maxValue");

			string message = String.Format(CultureInfo.CurrentCulture, Resources.ArgumentOutOfRange, paramName, actualValue, minValue, maxValue);
			return new ArgumentOutOfRangeException(paramName, actualValue, message);
		}

		public static ArgumentException ArgumentMustBeOfType(string paramName, Type type)
		{
			if (paramName == null)
				throw new ArgumentNullException("paramName");

			if (type == null)
				throw new ArgumentNullException("type");

			string message = String.Format(CultureInfo.CurrentCulture, Resources.ArgMustBeOfType, type);
			return new ArgumentException(message, paramName);
		}

		public static ArgumentException ArgumentInvalidIdentifier(string paramName, string message)
		{
			if (paramName == null)
				throw new ArgumentNullException("paramName");

			if (message == null)
				throw new ArgumentNullException("message");

			return new ArgumentException(message, paramName);
		}

		#endregion

		#region Specific Argument Errors

		public static ArgumentException BindingWithSameNameAlreadyInCollection(string paramName, Binding binding)
		{
			if (paramName == null)
				throw new ArgumentNullException("paramName");

			if (binding == null)
				throw new ArgumentNullException("binding");

			string message = String.Format(CultureInfo.CurrentCulture, Resources.BindingWithSameNameAlreadyInCollection, binding.Name);
			return new ArgumentException(message, paramName);
		}

		public static ArgumentException FunctionWithSameNameAndSignatureAlreadyInCollection(string paramName, FunctionBinding functionBinding)
		{
			if (paramName == null)
				throw new ArgumentNullException("paramName");

			if (functionBinding == null)
				throw new ArgumentNullException("functionBinding");

			StringBuilder sb = new StringBuilder();
			sb.Append(functionBinding.Name);
			sb.Append("(");
			FormattingHelpers.FormatTypeList(functionBinding.GetParameterTypes());
			sb.Append(")");

			string signature = sb.ToString();
			string message = String.Format(CultureInfo.CurrentCulture, Resources.FunctionWithSameNameAndSignatureAlreadyInCollection, signature);
			return new ArgumentException(message, paramName);
		}

		public static ArgumentException TypeAlreadyRegistered(string paramName, Type type)
		{
			if (paramName == null)
				throw new ArgumentNullException("paramName");

			if (type == null)
				throw new ArgumentNullException("type");

			return new ArgumentException(Resources.TypeAlreadyRegistered, paramName);
		}

		public static ArgumentException ParentTableMustExistInDataContext(string paramName)
		{
			if (paramName == null)
				throw new ArgumentNullException("paramName");

			return new ArgumentException(Resources.ParentTableMustExistInDataContext, paramName);
		}

		public static ArgumentException ParentColumnNotFound(string paramName, TableBinding parentTable, string parentColumn)
		{
			if (paramName == null)
				throw new ArgumentNullException("paramName");

			if (parentTable == null)
				throw new ArgumentNullException("parentTable");

			if (parentColumn == null)
				throw new ArgumentNullException("parentColumn");

			string message = String.Format(CultureInfo.CurrentCulture, Resources.ParentColumnNotFound, parentColumn, parentTable);
			return new ArgumentException(message, paramName);
		}

		public static ArgumentException ChildTableMustExistInDataContext(string paramName)
		{
			if (paramName == null)
				throw new ArgumentNullException("paramName");

			return new ArgumentException(Resources.ChildTableMustExistInDataContext, paramName);
		}

		public static ArgumentException ChildColumnNotFound(string paramName, TableBinding childTable, string childColumn)
		{
			if (paramName == null)
				throw new ArgumentNullException("paramName");

			if (childTable == null)
				throw new ArgumentNullException("childTable");

			if (childColumn == null)
				throw new ArgumentNullException("childColumn");

			string message = String.Format(CultureInfo.CurrentCulture, Resources.ChildColumnNotFound, childColumn, childTable);
			return new ArgumentException(message, paramName);
		}

		public static ArgumentException TableMustHaveAtLeastOneColumn(string paramName)
		{
			if (paramName == null)
				throw new ArgumentNullException("paramName");

			return new ArgumentException(Resources.TableMustHaveAtLeastOneColumn, paramName);
		}

		public static ArgumentException AllColumnsMustBelongToSameTable(string paramName)
		{
			if (paramName == null)
				throw new ArgumentNullException("paramName");

			string message = String.Format(CultureInfo.CurrentCulture, Resources.AllColumnsMustBelongToSameTable, paramName);
			return new ArgumentException(message, paramName);
		}

		public static ArgumentException ParameterValueTypeMismatch(string paramName)
		{
			if (paramName == null)
				throw new ArgumentNullException("paramName");

			return new ArgumentException(Resources.ParameterValueTypeMismatch, paramName);
		}

		public static ArgumentException TargetTypeMismatch(string paramName, Type genericType, Type targetType)
		{
			if (paramName == null)
				throw new ArgumentNullException("paramName");

			if (genericType == null)
				throw new ArgumentNullException("genericType");

			if (targetType == null)
				throw new ArgumentNullException("targetType");

			string message = String.Format(CultureInfo.CurrentCulture, Resources.TargetTypeMismatch, genericType, targetType);
			return new ArgumentException(message, paramName);
		}

		#endregion

		#region Specific API Misusage

		public static NQueryException FunctionMustNotBeVoid(Delegate function)
		{
			if (function == null)
				throw new ArgumentNullException("function");

			return new NQueryException(Resources.FunctionMustNotBeVoid);
		}

		public static NQueryException FunctionMustNotHaveRefOrOutParams(Delegate function, ParameterInfo parameter)
		{
			if (function == null)
				throw new ArgumentNullException("function");

			if (parameter == null)
				throw new ArgumentNullException("parameter");

			return new NQueryException(Resources.FunctionMustNotHaveRefOrOutParams);
		}

		public static NQueryException FunctionMustNotHaveOptionalParams(Delegate function, ParameterInfo parameter)
		{
			if (function == null)
				throw new ArgumentNullException("function");

			if (parameter == null)
				throw new ArgumentNullException("parameter");

			return new NQueryException(Resources.FunctionMustNotHaveOptionalParams);
		}

		public static NQueryException FunctionMustNotHaveArrayParams(Delegate function, ParameterInfo parameter)
		{
			if (function == null)
				throw new ArgumentNullException("function");

			if (parameter == null)
				throw new ArgumentNullException("parameter");

			return new NQueryException(Resources.FunctionMustNotHaveArrayParams);
		}

		public static NQueryException NoPropertyProviderRegisteredAndDefaultProviderIsMissing(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			string message = String.Format(CultureInfo.CurrentCulture, Resources.NoPropertyProviderRegisteredAndDefaultProviderIsMissing, type.FullName);
			return new NQueryException(message);
		}

		public static InvalidOperationException PropertyNotInitialized(string propertyName)
		{
			if (propertyName == null)
				throw new ArgumentNullException("propertyName");

			string message = String.Format(CultureInfo.CurrentCulture, Resources.PropertyNotInitialized, propertyName);
			return new InvalidOperationException(message);
		}

		public static InvalidOperationException InvalidAttemptToRead()
		{
			return new InvalidOperationException(Resources.InvalidAttemptToRead);
		}

		#endregion

		#region Compilation Errors

		private static string GetAdditionalInformation(ICollection<CompilationError> errors)
		{
			if (errors == null || errors.Count == 0)
				return String.Empty;

			StringBuilder sb = new StringBuilder();

			foreach (CompilationError error in errors)
			{
				// Since we also want a new line at the very beginning
				// we always add a new line.
				sb.Append(Environment.NewLine);
				sb.Append(error.Text);
			}

			return sb.ToString();
		}

		public static Exception ExpressionCompilationFailed(IList<CompilationError> errors)
		{
			if (errors == null)
				throw new ArgumentNullException("errors");

			string message = String.Format(CultureInfo.CurrentCulture, Resources.ExpressionCompilationFailed, GetAdditionalInformation(errors));
			return new CompilationException(message, errors);
		}

		public static Exception QueryCompilationFailed(IList<CompilationError> errors)
		{
			if (errors == null)
				throw new ArgumentNullException("errors");

			string message = String.Format(CultureInfo.CurrentCulture, Resources.QueryCompilationFailed, GetAdditionalInformation(errors));
			return new CompilationException(message, errors);
		}

		public static Exception CodeAssistanceFailed(IList<CompilationError> errors)
		{
			if (errors == null)
				throw new ArgumentNullException("errors");

			string message = String.Format(CultureInfo.CurrentCulture, Resources.CodeAssistanceFailed, GetAdditionalInformation(errors));
			return new CompilationException(message, errors);
		}

		#endregion

		#region Runtime Errors

		public static RuntimeException RuntimeError(Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException("exception");

			return new RuntimeException(exception.Message, exception);
		}

		public static RuntimeException ConversionFailed(Type targetType, object value, Exception exception)
		{
			if (targetType == null)
				throw new ArgumentNullException("targetType");

			if (exception == null)
				throw new ArgumentNullException("exception");

			string valueDescriptor;

			if (value == null)
				valueDescriptor = "NULL";
			else
				valueDescriptor = String.Format(CultureInfo.CurrentCulture, "'{0}' : {1}", Convert.ToString(value, CultureInfo.InvariantCulture), value.GetType().FullName);

			string message = String.Format(CultureInfo.CurrentCulture, Resources.ConversionFailed, valueDescriptor, targetType.FullName, exception.Message);
			return new RuntimeException(message, exception);
		}

		public static RuntimeException AssertionFailed(string message)
		{
			if (message == null)
				throw new ArgumentNullException("message");

			return new RuntimeException(message);
		}

		public static RuntimeException IAggregatorInitFailed(Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException("exception");

			return new RuntimeException(Resources.IAggregatorInitFailed, exception);
		}

		public static RuntimeException IAggregatorAccumulateFailed(Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException("exception");

			return new RuntimeException(Resources.IAggregatorAccumulateFailed, exception);
		}

		public static RuntimeException IAggregatorTerminateFailed(Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException("exception");

			return new RuntimeException(Resources.IAggregatorTerminateFailed, exception);
		}

		public static RuntimeException BinaryOperatorFailed(BinaryOperator binaryOperator, MethodInfo operatorMethod, Type leftOperandType, Type rightOperandType, object left, object right, Exception exception)
		{
			if (binaryOperator == null)
				throw new ArgumentNullException("binaryOperator");

			if (operatorMethod == null)
				throw new ArgumentNullException("operatorMethod");

			if (leftOperandType == null)
				throw new ArgumentNullException("leftOperandType");

			if (rightOperandType == null)
				throw new ArgumentNullException("rightOperandType");

			if (exception == null)
				throw new ArgumentNullException("exception");

			string message = String.Format(
				CultureInfo.CurrentCulture,
				Resources.BinaryOperatorFailed,
				FormattingHelpers.FormatType(leftOperandType),
				binaryOperator.TokenText,
				FormattingHelpers.FormatType(rightOperandType),
				FormattingHelpers.FormatMethodInfo(operatorMethod),
				left,
				right,
				exception.Message,
				Environment.NewLine
			);

			return new RuntimeException(message, exception);
		}

		public static RuntimeException UnaryOperatorFailed(UnaryOperator unaryOperator, MethodInfo operatorMethod, Type expressionType, object value, Exception exception)
		{
			if (unaryOperator == null)
				throw new ArgumentNullException("unaryOperator");

			if (operatorMethod == null)
				throw new ArgumentNullException("operatorMethod");

			if (expressionType == null)
				throw new ArgumentNullException("expressionType");

			if (exception == null)
				throw new ArgumentNullException("exception");

			string message = String.Format(
				CultureInfo.CurrentCulture,
				Resources.UnaryOperatorFailed,
				unaryOperator.TokenText,
				FormattingHelpers.FormatType(expressionType),
				FormattingHelpers.FormatMethodInfo(operatorMethod),
				exception.Message,
				value,
				Environment.NewLine
			);

			return new RuntimeException(message, exception);
		}

		public static RuntimeException CastingOperatorFailed(Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException("exception");

			return new RuntimeException(Resources.CastingOperatorFailed, exception);
		}

		public static RuntimeException FunctionBindingInvokeFailed(Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException("exception");

			return new RuntimeException(Resources.FunctionBindingInvokeFailed, exception);
		}

		public static RuntimeException MethodBindingInvokeFailed(Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException("exception");

			return new RuntimeException(Resources.MethodBindingInvokeFailed, exception);
		}

		public static RuntimeException PropertyBindingGetValueFailed(Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException("exception");

			return new RuntimeException(Resources.PropertyBindingGetValueFailed, exception);
		}

		public static RuntimeException ColumnBindingGetValueFailed(Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException("exception");

			return new RuntimeException(Resources.ColumnBindingGetValueFailed, exception);
		}

		public static RuntimeException ConstantBindingGetValueFailed(Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException("exception");

			return new RuntimeException(Resources.ConstantBindingGetValueFailed, exception);
		}

		public static RuntimeException ParameterBindingGetValueFailed(Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException("exception");

			return new RuntimeException(Resources.ParameterBindingGetValueFailed, exception);
		}

		public static RuntimeException IPropertyProviderGetPropertiesFailed(Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException("exception");

			return new RuntimeException(Resources.IPropertyProviderGetPropertiesFailed, exception);
		}

		public static RuntimeException IMethodProviderGetMethodsFailed(Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException("exception");

			return new RuntimeException(Resources.IMethodProviderGetMethodsFailed, exception);
		}

		#endregion
	}
}