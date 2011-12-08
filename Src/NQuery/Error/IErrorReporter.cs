using System;
using System.Reflection;

using NQuery.Compilation;
using NQuery.Runtime;

namespace NQuery
{
	internal interface IErrorReporter
	{
		bool ErrorsSeen { get; }

#if DEBUG || ALLOW_DEBUG_ERROR
#if !DEBUG
		[Obsolete("This method should not be called in release builds.")]
#endif
		void DebugError(string format, params object[] args);
#endif

		// Lexer

		void IllegalInputCharacter(SourceLocation location, char character);
		void UnterminatedComment(SourceLocation location, string tokenText);
		void UnterminatedString(SourceLocation location, string tokenText);
		void UnterminatedQuotedIdentifier(SourceLocation location, string tokenText);
		void UnterminatedParenthesizedIdentifier(SourceLocation location, string tokenText);
		void UnterminatedDate(SourceLocation location, string tokenText);

		// Parser

		void InvalidDate(SourceRange sourceRange, string tokenText);
		void NumberTooLarge(SourceRange sourceRange, string tokenText);
		void InvalidInteger(SourceRange sourceRange, string tokenText);
		void InvalidReal(SourceRange sourceRange, string tokenText);
		void InvalidBinary(SourceRange sourceRange, string tokenText);
		void InvalidOctal(SourceRange sourceRange, string tokenText);
		void InvalidHex(SourceRange sourceRange, string tokenText);
		void InvalidTypeReference(SourceRange sourceRange, string tokenText);
		void TokenExpected(SourceRange sourceRange, string foundTokenText, TokenId expected);
		void SimpleExpressionExpected(SourceRange sourceRange, string tokenText);
		void TableReferenceExpected(SourceRange sourceRange, string foundTokenText);
		void InvalidOperatorForAllAny(SourceRange sourceRange, BinaryOperator foundOp);

		// Resolving/Evaluation

		void UndeclaredTable(SourceRange sourceRange, Identifier identifier);
		void UndeclaredParameter(SourceRange sourceRange, Identifier identifier);
		void UndeclaredFunction(SourceRange sourceRange, Identifier identifier, Type[] parameterTypes);
		void UndeclaredMethod(SourceRange sourceRange, Type declaringType, Identifier identifier, Type[] parameterTypes);
		void UndeclaredColumn(SourceRange sourceRange, TableRefBinding tableRefBinding, Identifier identifier);
		void UndeclaredProperty(SourceRange sourceRange, Type type, Identifier identifier);
		void UndeclaredType(SourceRange sourceRange, string typeName);
		void UndeclaredEntity(SourceRange sourceRange, Identifier identifier);

		void AmbiguousReference(SourceRange sourceRange, Identifier identifier, Binding[] candidates);
		void AmbiguousTableRef(SourceRange sourceRange, Identifier identifier, TableRefBinding[] candidates);
		void AmbiguousColumnRef(SourceRange sourceRange, Identifier identifier, ColumnRefBinding[] candidates);
		void AmbiguousTable(SourceRange sourceRange, Identifier identifier, TableBinding[] candidates);
		void AmbiguousConstant(SourceRange sourceRange, Identifier identifier, ConstantBinding[] candidates);
		void AmbiguousParameter(SourceRange sourceRange, Identifier identifier, ParameterBinding[] candidates);
		void AmbiguousAggregate(SourceRange sourceRange, Identifier identifier, AggregateBinding[] candidates);
		void AmbiguousProperty(SourceRange sourceRange, Identifier identifier, PropertyBinding[] candidates);
		void AmbiguousType(string typeReference, Type[] candidates);
		void AmbiguousInvocation(InvocableBinding function1, InvocableBinding function2, Type[] argumentTypes);

		void InvocationRequiresParentheses(SourceRange sourceRange, InvocableBinding[] invocableGroup);
		void CannotApplyOperator(UnaryOperator op, Type type);
		void AmbiguousOperator(UnaryOperator op, Type type, MethodInfo opMethod1, MethodInfo opMethod2);
		void CannotApplyOperator(BinaryOperator op, Type leftType, Type rightType);
		void AmbiguousOperatorOverloading(BinaryOperator op, Type leftType, Type rightType);
		void AmbiguousOperator(BinaryOperator op, Type leftType, Type rightType, MethodInfo opMethod1, MethodInfo opMethod2);
		void AmbiguousOperator(CastingOperatorType castingOperatorType, MethodInfo targetFromSource, MethodInfo sourceToTarget);
		void AsteriskModifierNotAllowed(SourceRange sourceRange, ExpressionNode functionInvocation);
		void WhenMustEvaluateToBoolIfCaseInputIsOmitted(ExpressionNode whenExpression);
		void CannotLoadTypeAssembly(string assemblyName, Exception exception);
		void CannotFoldConstants(RuntimeException exception);		
		void CannotCast(ExpressionNode expression, Type targetType);
		
		// Query

		void MustSpecifyTableToSelectFrom();
		void AggregateCannotContainAggregate(AggregateExpression expression, AggregateBinding parent, AggregateBinding nested);
		void AggregateCannotContainSubquery(AggregateExpression expression);
		void AggregateDoesNotSupportType(AggregateBinding aggregateBinding, Type argumentType);
		void AggregateInWhere();
		void AggregateInOn();
		void AggregateInGroupBy();
		void AggregateContainsColumnsFromDifferentQueries(ExpressionNode aggregateArgument);
		void AggregateInvalidInCurrentContext(AggregateExpression aggregateExpression);
		void DuplicateTableRefInFrom(Identifier identifier);
		void TableRefInaccessible(TableRefBinding tableRefBinding);
		void TopWithTiesRequiresOrderBy();
		void OrderByColumnPositionIsOutOfRange(long index);
		void WhereClauseMustEvaluateToBool();
		void HavingClauseMustEvaluateToBool();
		void SelectExpressionNotAggregatedAndNoGroupBy(ColumnRefBinding columnRefBinding);
		void SelectExpressionNotAggregatedOrGrouped(ColumnRefBinding columnRefBinding);
		void HavingExpressionNotAggregatedOrGrouped(ColumnRefBinding columnRefBinding);
		void OrderByExpressionNotAggregatedAndNoGroupBy(ColumnRefBinding columnRefBinding);
		void OrderByExpressionNotAggregatedOrGrouped(ColumnRefBinding columnRefBinding);
		void OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified();
		void InvalidDataTypeInSelectDistinct(Type expressionType);
		void InvalidDataTypeInGroupBy(Type expressionType);		
		void InvalidDataTypeInOrderBy(Type expressionType);
		void InvalidDataTypeInUnion(Type expressionType, BinaryQueryOperator unionOperator);
		void DifferentExpressionCountInBinaryQuery();
		void OrderByItemsMustBeInSelectListIfUnionSpecified();
		void OrderByItemsMustBeInSelectListIfDistinctSpecified();
		void GroupByItemDoesNotReferenceAnyColumns();
		void ConstantExpressionInOrderBy();
		void TooManyExpressionsInSelectListOfSubquery();
		void InvalidRowReference(SourceRange sourceRange, TableRefBinding derivedTableRef);
		void NoColumnAliasSpecified(SourceRange sourceRange, int columnIndex, Identifier derivedTableName);
		void CteHasMoreColumnsThanSpecified(Identifier cteTableName);
		void CteHasFewerColumnsThanSpecified(Identifier cteTableName);
		void CteHasDuplicateColumnName(Identifier columnName, Identifier cteTableName);
		void CteHasDuplicateTableName(Identifier cteTableName);
		void CteDoesNotHaveUnionAll(Identifier cteTableName);
		void CteDoesNotHaveAnchorMember(Identifier cteTableName);
		void CteContainsRecursiveReferenceInSubquery(Identifier cteTableName);
		void CteContainsUnexpectedAnchorMember(Identifier cteTableName);
		void CteContainsMultipleRecursiveReferences(Identifier cteTableName);
		void CteContainsUnion(Identifier cteTableName);
		void CteContainsDistinct(Identifier cteTableName);
		void CteContainsTop(Identifier cteTableName);
		void CteContainsOuterJoin(Identifier cteTableName);
		void CteContainsGroupByHavingOrAggregate(Identifier cteTableName);
		void CteHasTypeMismatchBetweenAnchorAndRecursivePart(Identifier columnName, Identifier cteTableName);
	}
}