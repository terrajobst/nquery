using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	[TestClass]
	public class ValidatorTests : AutomatedTestFixtureBase
	{
		[TestMethod]
		public void EnsureOuterReferencesExcludedFromRuleSelectExpressionNotAggregatedOrGrouped()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void EnsureOuterReferencesExcludedFromRuleSelectExpressionNotAggregatedAndNoGroupBy()
		{
			RunTestOfCallingMethod();	
		}
	}
}
