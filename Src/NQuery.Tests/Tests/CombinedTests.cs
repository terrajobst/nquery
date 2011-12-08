using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	[TestClass]
	public class CombinedTests : AutomatedTestFixtureBase
	{
		[TestMethod]
		public void Test1()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void Test2()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void Distinct()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void ConstantScanUnionJoin()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void ConstantScanUnionAllJoin()
		{
			RunTestOfCallingMethod();
		}		

		[TestMethod]
		public void JoinUnionConstantScan()
		{
			RunTestOfCallingMethod();
		}		

		[TestMethod]
		public void JoinUnionAllConstantScan()
		{
			RunTestOfCallingMethod();
		}		

		[TestMethod]
		public void GroupyByUnionAllMultipleConstantScans()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void BetweenLikeSoundslikeSimilarToNegations()
		{
			RunTestOfCallingMethod();			
		}

		[TestMethod]
		public void UnionNull()
		{
			RunTestOfCallingMethod();			
		}

		[TestMethod]
		public void UnionAllNull()
		{
			RunTestOfCallingMethod();			
		}
	}
}
