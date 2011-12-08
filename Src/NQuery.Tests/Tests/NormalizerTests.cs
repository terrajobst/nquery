using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	[TestClass]
	public class NormalizerTests : AutomatedTestFixtureBase
	{
		[TestMethod]
		public void InExpression()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void NegatedInExpression1()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void NegatedInExpression2()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void NegatedOredInExpression()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void OredInExpression()
		{
			RunTestOfCallingMethod();
		}
	}
}
