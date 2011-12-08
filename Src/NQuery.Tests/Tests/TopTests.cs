using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	[TestClass]
	public class TopTests : AutomatedTestFixtureBase
	{
		[TestMethod]
		public void LimitAboveTableCount()
		{
			RunTestOfCallingMethod();			
		}

		[TestMethod]
		public void LimitBelowTableCount()
		{
			RunTestOfCallingMethod();			
		}

		[TestMethod]
		public void LimitEqualsTableCount()
		{
			RunTestOfCallingMethod();			
		}

		[TestMethod]
		public void LimitEqualsZero()
		{
			RunTestOfCallingMethod();			
		}
	}
}
