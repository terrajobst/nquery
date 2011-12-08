using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	[TestClass]
	public class OrderByTests : AutomatedTestFixtureBase
	{
		[TestMethod]
		public void DistinctSort()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void DistinctSortEmpty()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void OrderByFieldAlias()	
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void OrderByFieldAliasEmpty()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void OrderByFieldRef()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void OrderByFieldRefEmpty()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void OrderByPos()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void OrderByPosAsc()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void OrderByPosAscDesc()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void OrderByPosDesc()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void OrderByPosDescAsc()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void OrderByPosEmpty()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void DistinctSortIsAfterComputeScalar1()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void DistinctSortIsAfterComputeScalar2()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void DistinctSortIsAfterComputeScalarWithOrderBy()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void DistinctSortIsAfterComputeScalarWithOrderByDesc()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void OrderByWithTop3()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void OrderByWithTop3WithTies()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void OrderByWithUnionAllAndInvalidAggregate()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void OrderByWithUnionAllAndValidAggregate()
		{
			RunTestOfCallingMethod();
		}
	}
}
