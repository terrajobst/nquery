using System;
using System.Globalization;
using System.Reflection;

using NQuery.Compilation;
using NQuery.Runtime;

namespace NQuery
{
	internal abstract class ErrorProvider : IErrorReporter
	{
		private bool _errorsSeen;

		protected ErrorProvider()
		{
		}

		#region Public API

		public bool ErrorsSeen
		{
			get { return _errorsSeen; }
		}

		public virtual void Reset()
		{
			_errorsSeen = false;
		}

		protected virtual void OnError(CompilationError compilationError)
		{
			_errorsSeen = true;
		}

		#endregion

		#region Error Creation Helpers

		private void HandleError(ErrorId errorId, string message)
		{
			HandleError(SourceRange.None, errorId, message);
		}

		private void HandleError(SourceRange sourceRange, ErrorId errorId, string message)
		{
			OnError(new CompilationError(sourceRange, errorId, message));
		}

		private static SourceRange CreateSourceRange(SourceLocation location)
		{
			return new SourceRange(location.Column, location.Line, location.Column + 1, location.Line);
		}

		#endregion

		#region Debug Error

#if DEBUG || ALLOW_DEBUG_ERROR
		void IErrorReporter.DebugError(string format, params object[] args)
		{
			HandleError(ErrorId.InternalError, String.Format(CultureInfo.CurrentCulture, format, args));
		}
#endif

		#endregion

		#region Lexer CompilationErrors

		void IErrorReporter.IllegalInputCharacter(SourceLocation location, char character)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.IllegalInputCharacter, character);
			HandleError(CreateSourceRange(location), ErrorId.IllegalInputCharacter, message);
		}

		void IErrorReporter.UnterminatedComment(SourceLocation location, string tokenText)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.UnterminatedComment, FormattingHelpers.GetFirstLine(tokenText));
			HandleError(CreateSourceRange(location), ErrorId.UnterminatedComment, message);
		}

		void IErrorReporter.UnterminatedString(SourceLocation location, string tokenText)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.UnterminatedString, FormattingHelpers.GetFirstLine(tokenText));
			HandleError(CreateSourceRange(location), ErrorId.UnterminatedString, message);
		}

		void IErrorReporter.UnterminatedQuotedIdentifier(SourceLocation location, string tokenText)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.UnterminatedQuotedIdentifier, FormattingHelpers.GetFirstLine(tokenText));
			HandleError(CreateSourceRange(location), ErrorId.UnterminatedQuotedIdentifier, message);
		}

		void IErrorReporter.UnterminatedParenthesizedIdentifier(SourceLocation location, string tokenText)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.UnterminatedParenthesizedIdentifier, FormattingHelpers.GetFirstLine(tokenText));
			HandleError(CreateSourceRange(location), ErrorId.UnterminatedParenthesizedIdentifier, message);
		}

		void IErrorReporter.UnterminatedDate(SourceLocation location, string tokenText)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.UnterminatedDate, FormattingHelpers.GetFirstLine(tokenText));
			HandleError(CreateSourceRange(location), ErrorId.UnterminatedDate, message);
		}

		#endregion

		#region Parser Errors

		void IErrorReporter.InvalidDate(SourceRange sourceRange, string tokenText)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidDate, tokenText);
			HandleError(sourceRange, ErrorId.InvalidDate, message);
		}

		void IErrorReporter.InvalidInteger(SourceRange sourceRange, string tokenText)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidInteger, tokenText);
			HandleError(sourceRange, ErrorId.InvalidInteger, message);
		}

		void IErrorReporter.InvalidReal(SourceRange sourceRange, string tokenText)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidDecimal, tokenText);
			HandleError(sourceRange, ErrorId.InvalidReal, message);
		}

		void IErrorReporter.InvalidBinary(SourceRange sourceRange, string tokenText)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidBinary, tokenText);
			HandleError(sourceRange, ErrorId.InvalidBinary, message);
		}

		void IErrorReporter.InvalidOctal(SourceRange sourceRange, string tokenText)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidOctal, tokenText);
			HandleError(sourceRange, ErrorId.InvalidOctal, message);
		}

		void IErrorReporter.InvalidHex(SourceRange sourceRange, string tokenText)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidHex, tokenText);
			HandleError(sourceRange, ErrorId.InvalidHex, message);
		}

		void IErrorReporter.InvalidTypeReference(SourceRange sourceRange, string tokenText)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidTypeReference, tokenText);
			HandleError(sourceRange, ErrorId.InvalidTypeReference, message);
		}

		void IErrorReporter.NumberTooLarge(SourceRange sourceRange, string tokenText)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.NumberTooLarge, tokenText);
			HandleError(sourceRange, ErrorId.NumberTooLarge, message);
		}

		void IErrorReporter.TokenExpected(SourceRange sourceRange, string foundTokenText, TokenId expected)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.TokenExpected, foundTokenText, TokenInfo.FromTokenId(expected).Text);
			HandleError(sourceRange, ErrorId.TokenExpected, message);
		}

		void IErrorReporter.SimpleExpressionExpected(SourceRange sourceRange, string tokenText)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.SimpleExpressionExpected, tokenText);
			HandleError(sourceRange, ErrorId.SimpleExpressionExpected, message);
		}

		void IErrorReporter.TableReferenceExpected(SourceRange sourceRange, string foundTokenText)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.TableReferenceExpected, foundTokenText);
			HandleError(sourceRange, ErrorId.TableReferenceExpected, message);
		}

		void IErrorReporter.InvalidOperatorForAllAny(SourceRange sourceRange, BinaryOperator foundOp)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidOperatorForAllAny, foundOp.TokenText);
			HandleError(sourceRange, ErrorId.InvalidOperatorForAllAny, message);
		}

		#endregion

		#region Resolving/Evaluation Errors

		void IErrorReporter.UndeclaredTable(SourceRange sourceRange, Identifier identifier)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredTable, identifier);
			HandleError(sourceRange, ErrorId.UndeclaredTable, message);
		}

		void IErrorReporter.UndeclaredParameter(SourceRange sourceRange, Identifier identifier)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredParameter, identifier);
			HandleError(sourceRange, ErrorId.UndeclaredParameter, message);
		}

		void IErrorReporter.UndeclaredFunction(SourceRange sourceRange, Identifier identifier, Type[] parameterTypes)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredFunction, identifier, FormattingHelpers.FormatTypeList(parameterTypes));
			HandleError(sourceRange, ErrorId.UndeclaredFunction, message);
		}

		void IErrorReporter.UndeclaredMethod(SourceRange sourceRange, Type declaringType, Identifier identifier, Type[] parameterTypes)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredMethod, declaringType.Name, identifier, FormattingHelpers.FormatTypeList(parameterTypes));
			HandleError(sourceRange, ErrorId.UndeclaredMethod, message);
		}

		void IErrorReporter.UndeclaredColumn(SourceRange sourceRange, TableRefBinding tableRefBinding, Identifier identifier)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredColumn, tableRefBinding.Name, identifier);
			HandleError(sourceRange, ErrorId.UndeclaredColumn, message);
		}

		void IErrorReporter.UndeclaredProperty(SourceRange sourceRange, Type type, Identifier identifier)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredProperty, FormattingHelpers.FormatType(type), identifier);
			HandleError(sourceRange, ErrorId.UndeclaredProperty, message);
		}

		void IErrorReporter.UndeclaredType(SourceRange sourceRange, string typeName)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredType, typeName);
			HandleError(sourceRange, ErrorId.UndeclaredType, message);
		}

		void IErrorReporter.UndeclaredEntity(SourceRange sourceRange, Identifier identifier)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredEntity, identifier);
			HandleError(sourceRange, ErrorId.UndeclaredEntity, message);
		}

		void IErrorReporter.AmbiguousReference(SourceRange sourceRange, Identifier identifier, Binding[] candidates)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousReference, identifier, FormattingHelpers.FormatBindingListWithCategory(candidates));
			HandleError(sourceRange, ErrorId.AmbiguousReference, message);
		}

		void IErrorReporter.AmbiguousTableRef(SourceRange sourceRange, Identifier identifier, TableRefBinding[] candidates)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousTableRef, identifier, FormattingHelpers.FormatBindingList(candidates));
			HandleError(sourceRange, ErrorId.AmbiguousTableRef, message);
		}

		void IErrorReporter.AmbiguousColumnRef(SourceRange sourceRange, Identifier identifier, ColumnRefBinding[] candidates)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousColumnRef, identifier, FormattingHelpers.FormatBindingList(candidates));
			HandleError(sourceRange, ErrorId.AmbiguousColumnRef, message);
		}

		void IErrorReporter.AmbiguousTable(SourceRange sourceRange, Identifier identifier, TableBinding[] candidates)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousTable, identifier, FormattingHelpers.FormatBindingList(candidates));
			HandleError(sourceRange, ErrorId.AmbiguousTable, message);
		}

		void IErrorReporter.AmbiguousConstant(SourceRange sourceRange, Identifier identifier, ConstantBinding[] candidates)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousConstant, identifier, FormattingHelpers.FormatBindingList(candidates));
			HandleError(sourceRange, ErrorId.AmbiguousConstant, message);
		}

		void IErrorReporter.AmbiguousParameter(SourceRange sourceRange, Identifier identifier, ParameterBinding[] candidates)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousParameter, identifier, FormattingHelpers.FormatBindingList(candidates));
			HandleError(sourceRange, ErrorId.AmbiguousParameter, message);
		}

		void IErrorReporter.AmbiguousAggregate(SourceRange sourceRange, Identifier identifier, AggregateBinding[] candidates)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousAggregate, identifier, FormattingHelpers.FormatBindingList(candidates));
			HandleError(sourceRange, ErrorId.AmbiguousAggregate, message);
		}

		void IErrorReporter.AmbiguousProperty(SourceRange sourceRange, Identifier identifier, PropertyBinding[] candidates)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousProperty, identifier, FormattingHelpers.FormatBindingList(candidates));
			HandleError(sourceRange, ErrorId.AmbiguousProperty, message);
		}

		void IErrorReporter.AmbiguousType(string typeReference, Type[] candidates)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousType, typeReference, FormattingHelpers.FormatFullyQualifiedTypeList(candidates));
			HandleError(ErrorId.AmbiguousType, message);
		}

		void IErrorReporter.AmbiguousInvocation(InvocableBinding function1, InvocableBinding function2, Type[] argumentTypes)
		{
			if (argumentTypes.Length == 0)
			{
				string message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousInvocationNoArgs, function1.GetFullName(), function2.GetFullName());
				HandleError(ErrorId.AmbiguousInvocation, message);
			}
			else
			{
				string message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousInvocation, function1.GetFullName(), function2.GetFullName(), FormattingHelpers.FormatTypeList(argumentTypes));
				HandleError(ErrorId.AmbiguousInvocation, message);
			}
		}

		void IErrorReporter.InvocationRequiresParentheses(SourceRange sourceRange, InvocableBinding[] invocableGroup)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.InvocationRequiresParentheses, invocableGroup[0].GetFullName());
			HandleError(sourceRange, ErrorId.InvocationRequiresParentheses, message);
		}

		void IErrorReporter.CannotApplyOperator(UnaryOperator op, Type type)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CannotApplyUnaryOp, op.TokenText, FormattingHelpers.FormatType(type));
			HandleError(ErrorId.CannotApplyUnaryOperator, message);
		}

		void IErrorReporter.AmbiguousOperator(UnaryOperator op, Type type, MethodInfo opMethod1, MethodInfo opMethod2)
		{
			string message = String.Format(
				CultureInfo.CurrentCulture,
				Resources.AmbiguousUnaryOp,
				op.TokenText,
				FormattingHelpers.FormatType(type),
				FormattingHelpers.FormatMethodInfo(opMethod1),
				FormattingHelpers.FormatMethodInfo(opMethod2)
			);

			HandleError(ErrorId.AmbiguousUnaryOperator, message);
		}

		void IErrorReporter.CannotApplyOperator(BinaryOperator op, Type leftType, Type rightType)
		{
			string message = String.Format(
				CultureInfo.CurrentCulture,
				Resources.CannotApplyBinaryOp,
				op.TokenText,
				FormattingHelpers.FormatType(leftType),
				FormattingHelpers.FormatType(rightType)
			);

			HandleError(ErrorId.CannotApplyBinaryOperator, message);
		}

		void IErrorReporter.AmbiguousOperatorOverloading(BinaryOperator op, Type leftType, Type rightType)
		{
			string message = String.Format(
				CultureInfo.CurrentCulture,
				Resources.AmbiguousOperatorOverloading,
				op.TokenText,
				FormattingHelpers.FormatType(leftType),
				FormattingHelpers.FormatType(rightType)
			);

			HandleError(ErrorId.AmbiguousOperatorOverloading, message);
		}

		void IErrorReporter.AmbiguousOperator(BinaryOperator op, Type leftType, Type rightType, MethodInfo opMethod1, MethodInfo opMethod2)
		{
			string message = String.Format(
				CultureInfo.CurrentCulture,
				Resources.AmbiguousBinaryOperator,
				op.TokenText,
				FormattingHelpers.FormatType(leftType),
				FormattingHelpers.FormatType(rightType),
				FormattingHelpers.FormatMethodInfo(opMethod1),
				FormattingHelpers.FormatMethodInfo(opMethod2)
			);

			HandleError(ErrorId.AmbiguousBinaryOperator, message);
		}

		void IErrorReporter.AmbiguousOperator(CastingOperatorType castingOperatorType, MethodInfo targetFromSource, MethodInfo sourceToTarget)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousCastingOperator, castingOperatorType, FormattingHelpers.FormatMethodInfo(targetFromSource), FormattingHelpers.FormatMethodInfo(sourceToTarget));
			HandleError(ErrorId.AmbiguousCastingOperator, message);
		}

		void IErrorReporter.AsteriskModifierNotAllowed(SourceRange sourceRange, ExpressionNode functionInvocation)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.AsteriskModifierNotAllowed, functionInvocation.GenerateSource());
			HandleError(sourceRange, ErrorId.AsteriskModifierNotAllowed, message);
		}

		void IErrorReporter.WhenMustEvaluateToBoolIfCaseInputIsOmitted(ExpressionNode whenExpression)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.WhenMustEvaluateToBoolIfCaseInputIsOmitted, whenExpression.GenerateSource(), FormattingHelpers.FormatType(typeof(bool)));
			HandleError(ErrorId.WhenMustEvaluateToBoolIfCaseInputIsOmitted, message);
		}

		void IErrorReporter.CannotLoadTypeAssembly(string assemblyName, Exception exception)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CannotLoadTypeAssembly, assemblyName, exception.Message);
			HandleError(ErrorId.CannotLoadTypeAssembly, message);
		}

		void IErrorReporter.CannotFoldConstants(RuntimeException exception)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CannotFoldConstants, exception.Message);
			HandleError(ErrorId.CannotFoldConstants, message);
		}

		void IErrorReporter.CannotCast(ExpressionNode expression, Type targetType)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CannotCast, expression.GenerateSource(), FormattingHelpers.FormatType(expression.ExpressionType), FormattingHelpers.FormatType(targetType));
			HandleError(ErrorId.CannotCast, message);
		}

		#endregion

		#region Query Errors

		void IErrorReporter.MustSpecifyTableToSelectFrom()
		{
			HandleError(ErrorId.MustSpecifyTableToSelectFrom, Resources.MustSpecifyTableToSelectFrom);
		}

		void IErrorReporter.AggregateCannotContainAggregate(AggregateExpression expression, AggregateBinding parent, AggregateBinding nested)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.AggregateCannotContainAggregate, expression.GenerateSource(), parent.Name, nested.Name);
			HandleError(ErrorId.AggregateCannotContainAggregate, message);
		}

		void IErrorReporter.AggregateCannotContainSubquery(AggregateExpression expression)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.AggregateCannotContainSubquery, expression.GenerateSource());
			HandleError(ErrorId.AggregateCannotContainAggregate, message);
		}

		void IErrorReporter.AggregateDoesNotSupportType(AggregateBinding aggregateBinding, Type argumentType)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.AggregateDoesNotSupportType, aggregateBinding.Name, FormattingHelpers.FormatType(argumentType));
			HandleError(ErrorId.AggregateDoesNotSupportType, message);
		}

		void IErrorReporter.AggregateInWhere()
		{
			HandleError(ErrorId.AggregateInWhere, Resources.AggregateInWhere);
		}

		void IErrorReporter.AggregateInOn()
		{
			HandleError(ErrorId.AggregateInOn, Resources.AggregateInOn);
		}

		void IErrorReporter.AggregateInGroupBy()
		{
			HandleError(ErrorId.AggregateInGroupBy, Resources.AggregateInGroupBy);
		}

		void IErrorReporter.AggregateContainsColumnsFromDifferentQueries(ExpressionNode aggregateArgument)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.AggregateContainsColumnsFromDifferentQueries, aggregateArgument.GenerateSource());
			HandleError(ErrorId.AggregateContainsColumnsFromDifferentQueries, message);
		}

		void IErrorReporter.AggregateInvalidInCurrentContext(AggregateExpression aggregateExpression)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.AggregateInvalidInCurrentContext, aggregateExpression.GenerateSource());
			HandleError(ErrorId.AggregateInvalidInCurrentContext, message);
		}

		void IErrorReporter.DuplicateTableRefInFrom(Identifier identifier)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.DuplicateTableRefInFrom, identifier);
			HandleError(ErrorId.DuplicateTableRefInFrom, message);
		}

		void IErrorReporter.TableRefInaccessible(TableRefBinding tableRefBinding)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.TableRefInaccessible, tableRefBinding.Name, tableRefBinding.TableBinding.Name);
			HandleError(ErrorId.TableRefInaccessible, message);
		}

		void IErrorReporter.TopWithTiesRequiresOrderBy()
		{
			HandleError(ErrorId.TopWithTiesRequiresOrderBy, Resources.TopWithTiesRequiresOrderBy);
		}

		void IErrorReporter.OrderByColumnPositionIsOutOfRange(long index)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.OrderByColumnPositionIsOutOfRange, index);
			HandleError(ErrorId.OrderByColumnPositionIsOutOfRange, message);
		}

		void IErrorReporter.WhereClauseMustEvaluateToBool()
		{
			HandleError(ErrorId.WhereClauseMustEvaluateToBool, Resources.WhereClauseMustEvaluateToBool);
		}

		void IErrorReporter.HavingClauseMustEvaluateToBool()
		{
			HandleError(ErrorId.HavingClauseMustEvaluateToBool, Resources.HavingClauseMustEvaluateToBool);
		}

		void IErrorReporter.SelectExpressionNotAggregatedAndNoGroupBy(ColumnRefBinding columnRefBinding)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.SelectExpressionNotAggregatedAndNoGroupBy, columnRefBinding.GetFullName());
			HandleError(ErrorId.SelectExpressionNotAggregatedAndNoGroupBy, message);
		}

		void IErrorReporter.SelectExpressionNotAggregatedOrGrouped(ColumnRefBinding columnRefBinding)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.SelectExpressionNotAggregatedOrGrouped, columnRefBinding.GetFullName());
			HandleError(ErrorId.SelectExpressionNotAggregatedOrGrouped, message);
		}

		void IErrorReporter.HavingExpressionNotAggregatedOrGrouped(ColumnRefBinding columnRefBinding)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.HavingExpressionNotAggregatedOrGrouped, columnRefBinding.GetFullName());
			HandleError(ErrorId.HavingExpressionNotAggregatedOrGrouped, message);
		}

		void IErrorReporter.OrderByExpressionNotAggregatedAndNoGroupBy(ColumnRefBinding columnRefBinding)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.OrderByExpressionNotAggregatedAndNoGroupBy, columnRefBinding.GetFullName());
			HandleError(ErrorId.OrderByExpressionNotAggregatedAndNoGroupBy, message);
		}

		void IErrorReporter.OrderByExpressionNotAggregatedOrGrouped(ColumnRefBinding columnRefBinding)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.OrderByExpressionNotAggregatedOrGrouped, columnRefBinding.GetFullName());
			HandleError(ErrorId.OrderByExpressionNotAggregatedOrGrouped, message);
		}

		void IErrorReporter.OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified()
		{
			HandleError(ErrorId.OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified, Resources.OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified);
		}

		void IErrorReporter.InvalidDataTypeInSelectDistinct(Type expressionType)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidDataTypeInSelectDistinct, FormattingHelpers.FormatType(expressionType));
			HandleError(ErrorId.InvalidDataTypeInSelectDistinct, message);
		}

		void IErrorReporter.InvalidDataTypeInGroupBy(Type expressionType)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidDataTypeInGroupBy, FormattingHelpers.FormatType(expressionType));
			HandleError(ErrorId.InvalidDataTypeInGroupBy, message);
		}

		void IErrorReporter.InvalidDataTypeInOrderBy(Type expressionType)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidDataTypeInOrderBy, FormattingHelpers.FormatType(expressionType));
			HandleError(ErrorId.InvalidDataTypeInOrderBy, message);
		}

		void IErrorReporter.InvalidDataTypeInUnion(Type expressionType, BinaryQueryOperator unionOperator)
		{
			string unionOperatorString = unionOperator.ToString().ToUpper(CultureInfo.CurrentCulture);
			string message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidDataTypeInUnion, FormattingHelpers.FormatType(expressionType), unionOperatorString);
			HandleError(ErrorId.InvalidDataTypeInUnion, message);
		}

		void IErrorReporter.DifferentExpressionCountInBinaryQuery()
		{
			HandleError(ErrorId.DifferentExpressionCountInBinaryQuery, Resources.DifferentExpressionCountInBinaryQuery);
		}

		void IErrorReporter.OrderByItemsMustBeInSelectListIfUnionSpecified()
		{
			HandleError(ErrorId.OrderByItemsMustBeInSelectListIfUnionSpecified, Resources.OrderByItemsMustBeInSelectListIfUnionSpecified);
		}

		void IErrorReporter.OrderByItemsMustBeInSelectListIfDistinctSpecified()
		{
			HandleError(ErrorId.OrderByItemsMustBeInSelectListIfDistinctSpecified, Resources.OrderByItemsMustBeInSelectListIfDistinctSpecified);
		}

		void IErrorReporter.GroupByItemDoesNotReferenceAnyColumns()
		{
			HandleError(ErrorId.GroupByItemDoesNotReferenceAnyColumns, Resources.GroupByItemDoesNotReferenceAnyColumns);
		}

		void IErrorReporter.ConstantExpressionInOrderBy()
		{
			HandleError(ErrorId.ConstantExpressionInOrderBy, Resources.ConstantExpressionInOrderBy);
		}

		void IErrorReporter.TooManyExpressionsInSelectListOfSubquery()
		{
			HandleError(ErrorId.TooManyExpressionsInSelectListOfSubquery, Resources.TooManyExpressionsInSelectListOfSubquery);
		}

		void IErrorReporter.InvalidRowReference(SourceRange sourceRange, TableRefBinding derivedTableRef)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidRowReference, derivedTableRef.Name);
			HandleError(sourceRange, ErrorId.InvalidRowReference, message);
		}

		void IErrorReporter.NoColumnAliasSpecified(SourceRange sourceRange, int columnIndex, Identifier derivedTableName)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.NoColumnAliasSpecified, columnIndex + 1, derivedTableName.ToSource());
			HandleError(sourceRange, ErrorId.NoColumnAliasSpecified, message);
		}

		void IErrorReporter.CteHasMoreColumnsThanSpecified(Identifier cteTableName)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CteHasMoreColumnsThanSpecified, cteTableName.Text);
			HandleError(ErrorId.CteHasMoreColumnsThanSpecified, message);
		}

		void IErrorReporter.CteHasFewerColumnsThanSpecified(Identifier cteTableName)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CteHasFewerColumnsThanSpecified, cteTableName.Text);
			HandleError(ErrorId.CteHasFewerColumnsThanSpecified, message);
		}

		void IErrorReporter.CteHasDuplicateColumnName(Identifier columnName, Identifier cteTableName)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CteHasDuplicateColumnName, columnName.Text, cteTableName.Text);
			HandleError(ErrorId.CteHasDuplicateColumnName, message);
		}

		void IErrorReporter.CteHasDuplicateTableName(Identifier cteTableName)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CteHasDuplicateTableName, cteTableName.Text);
			HandleError(ErrorId.CteHasDuplicateTableName, message);
		}

		void IErrorReporter.CteDoesNotHaveUnionAll(Identifier cteTableName)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CteDoesNotHaveUnionAll, cteTableName.Text);
			HandleError(ErrorId.CteDoesNotHaveUnionAll, message);
		}

		void IErrorReporter.CteDoesNotHaveAnchorMember(Identifier cteTableName)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CteDoesNotHaveAnchorMember, cteTableName.Text);
			HandleError(ErrorId.CteDoesNotHaveAnchorMember, message);
		}

		void IErrorReporter.CteContainsRecursiveReferenceInSubquery(Identifier cteTableName)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CteContainsRecursiveReferenceInSubquery, cteTableName.Text);
			HandleError(ErrorId.CteContainsRecursiveReferenceInSubquery, message);
		}

		void IErrorReporter.CteContainsUnexpectedAnchorMember(Identifier cteTableName)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CteContainsUnexpectedAnchorMember, cteTableName.Text);
			HandleError(ErrorId.CteContainsUnexpectedAnchorMember, message);
		}

		void IErrorReporter.CteContainsMultipleRecursiveReferences(Identifier cteTableName)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CteContainsMultipleRecursiveReferences, cteTableName.Text);
			HandleError(ErrorId.CteContainsMultipleRecursiveReferences, message);
		}

		void IErrorReporter.CteContainsUnion(Identifier cteTableName)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CteContainsUnion, cteTableName.Text);
			HandleError(ErrorId.CteContainsUnion, message);
		}

		void IErrorReporter.CteContainsDistinct(Identifier cteTableName)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CteContainsDistinct, cteTableName.Text);
			HandleError(ErrorId.CteContainsDistinct, message);
		}

		void IErrorReporter.CteContainsTop(Identifier cteTableName)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CteContainsTop, cteTableName.Text);
			HandleError(ErrorId.CteContainsTop, message);
		}

		void IErrorReporter.CteContainsOuterJoin(Identifier cteTableName)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CteContainsOuterJoin, cteTableName.Text);
			HandleError(ErrorId.CteContainsOuterJoin, message);
		}

		void IErrorReporter.CteContainsGroupByHavingOrAggregate(Identifier cteTableName)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CteContainsGroupByHavingOrAggregate, cteTableName.Text);
			HandleError(ErrorId.CteContainsGroupByHavingOrAggregate, message);
		}

		void IErrorReporter.CteHasTypeMismatchBetweenAnchorAndRecursivePart(Identifier columnName, Identifier cteTableName)
		{
			string message = String.Format(CultureInfo.CurrentCulture, Resources.CteHasTypeMismatchBetweenAnchorAndRecursivePart, columnName.Text, cteTableName);
			HandleError(ErrorId.CteHasTypeMismatchBetweenAnchorAndRecursivePart, message);
		}

		#endregion
	}
}
