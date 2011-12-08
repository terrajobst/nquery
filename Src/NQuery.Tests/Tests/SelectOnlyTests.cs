using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	[TestClass]
	public class SelectOnlyTests : AutomatedTestFixtureBase
	{
		[TestMethod]
		public void NullQuery()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void NullExpression()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void ConstantExpression()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void EmptyConstantScan()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void EmptyEmployeeScan()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void NullIfExpression()
		{
			RunTestOfCallingMethod();
		}
	}
}
