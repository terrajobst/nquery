using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	[TestClass]
	public class ErrorTests : AutomatedTestFixtureBase
	{
		[TestMethod]
		public void AggregateContainsColumnsFromDifferentQueries()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void AggregateDoesNotSupportType()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void AggregateInGroupBy()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void AggregateInOn()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void AggregateInWhere()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void AsteriskModifierNotAllowed()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void CannotLoadTypeAssembly()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void ConstantExpressionInOrderBy()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void CteContainsDistinct()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void CteContainsGroupByHavingOrAggregate()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void CteContainsMultipleRecursiveReferences()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void CteContainsOuterJoin()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void CteContainsRecursiveRefenceInSubquery()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void CteContainsTop()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void CteContainsUnexpectedAnchorMember()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void CteContainsUnion()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void CteDoesNotHaveAnchorMember()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void CteDoesNotHaveUnionAll()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void CteHasDuplicateColumnName()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void CteHasDuplicateTableName()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void CteHasFewerColumnsThanSpecified()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void CteHasMoreColumnsThanSpecified()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void CteHasTypeMismatchBetweenAnchorAndRecursivePart()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void CteMissingColumnName()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void CteUnionAllColumnCountMismatch()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void DuplicateTableRefInFrom1()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void DuplicateTableRefInFrom2()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void DuplicateTableRefInFrom3()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void DuplicateTableRefInFrom4()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void GroupByItemDoesNotReferenceAnyColumns()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void HavingClauseMustEvaluateToBool()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void HavingExpressionNotAggregatedOrGrouped1()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void HavingExpressionNotAggregatedOrGrouped2()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void IllegalInputCharacter()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void InvalidBinary()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void InvalidDataTypeInGroupBy()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void InvalidDataTypeInOrderBy()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void InvalidDataTypeInSelectDistinct()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void InvalidDataTypeInUnion()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void InvalidDate()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void InvalidHex()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void InvalidInteger()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void InvalidOctal()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void InvalidOperatorForAllAny1()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void InvalidOperatorForAllAny2()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void InvalidOperatorForAllAny3()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void InvalidTypeReference()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void MustSpecifyTableToSelectFrom()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void NoColumnAliasSpecifiedForDerivedTable()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void NumberTooLarge()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void OrderByColumnPositionIsOutOfRange()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void OrderByExpressionNotAggregatedAndNoGroupBy()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void OrderByExpressionNotAggregatedOrGrouped()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified1()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified2()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified3()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified4()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified5()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified6()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void OrderByItemsMustBeInSelectListIfUnionSpecified()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void OrderByItemsMustBeInSelectListIfDistinctSpecified()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void RowReferenceToDerivedTableIsInvalid()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void SelectExpressionNotAggregatedAndNoGroupBy()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void SelectExpressionNotAggregatedOrGrouped()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void SimpleExpressionExpected()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void TableReferenceExpected1()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void TableReferenceExpected2()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void TableRefInaccessible1()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void TableRefInaccessible2()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void TokenExpected()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void TooManyExpressionsInSelectListOfSubquery1()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void TooManyExpressionsInSelectListOfSubquery2()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void TooManyExpressionsInSelectListOfSubquery3()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void TopWithTiesRequiresOrderBy()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void UndeclaredColumn()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void UndeclaredFunction()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void UndeclaredMethod()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void UndeclaredParameter()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void UndeclaredProperty()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void UndeclaredTable()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void UndeclaredType()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		[Ignore]
		public void UnterminatedComment()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void UnterminatedDate()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void UnterminatedParenthesizedIdentifier()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void UnterminatedQuotedIdentifier()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		[Ignore]
		public void UnterminatedString()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WhenMustEvaluateToBoolIfCaseInputIsOmitted()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WhereClauseMustEvaluateToBool()
		{
			RunTestOfCallingMethod();
		}
	}
}
