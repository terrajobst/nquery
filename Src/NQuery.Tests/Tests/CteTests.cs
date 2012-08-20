using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	[TestClass]
	public class CteTests : AutomatedTestFixtureBase
	{
		[TestMethod]
		public void AdvancedRecursive()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void ComplexRecursiveWithMultipleAnchors()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void ComplexRecursiveWithMultipleAnchorsAndRecursiveMembers()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void ComplexRecursiveWithMultipleRecursiveMembers()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void ComplexRecursiveWithUnionAnchor()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		[Ignore] // Doens't work right now. We've filed 14418 for it.
		public void JoinBetweenSameCte()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void NonRecursive()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void NonRecursiveMultipleRefs()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void NonRecursiveNested()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void SimpleRecursive()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void SimpleRecursiveExceedingMaxRecursion()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void SimpleRecursiveImplicitJoin()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void SimpleRecursiveMultipleRecursiveMembers()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void SimpleRecursiveSingleRowSubselect()
		{
			RunTestOfCallingMethod();
		}
	}
}
