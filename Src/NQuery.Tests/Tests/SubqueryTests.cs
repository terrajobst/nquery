using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	[TestClass]
	public class SubqueryTests : AutomatedTestFixtureBase
	{
		[TestMethod]
		public void ExistsInCasePartiallyExecutedScalarSubqueries()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void ExistsInNestedCaseWithNeverExecutedScalarSubqueries()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void ExistsInSelectFromDerivedTable()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void ExistsInWhen()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void NotExistsInWhen()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void NotExistsWithOr()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void SingleRowSubselectInCaseInput()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void SingleRowSubselectInDerivedTableInWhere()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void SingleRowSubselectInSelect()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void SingleRowSubselectInOrderBy()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void SingleRowSubselectInSelectWithOtherExpressions()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void SingleRowSubselectInWhere()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void SingleRowSubselectReturnsMoreThanOneRow()
		{
			RunTestOfCallingMethod();
		}
	}
}