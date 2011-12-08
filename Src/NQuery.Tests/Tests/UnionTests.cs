using System;
using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	[TestClass]
	public class UnionTests : AutomatedTestFixtureBase
	{
		[TestMethod]
		public void UnionWithJoin()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void UnionAllWithJoin()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void ExceptWithJoin()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void IntersectWithJoin()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void UnionWithSelf()
		{
			RunTestOfCallingMethod();			
		}
		
		[TestMethod]
		public void UnionAllWithSelf()
		{
			RunTestOfCallingMethod();			
		}
		
		[TestMethod]
		public void ExceptWithSelf()
		{
			RunTestOfCallingMethod();			
		}
		
		[TestMethod]
		public void IntersectWithSelf()
		{
			RunTestOfCallingMethod();			
		}
		
		[TestMethod]
		public void UnionDoesImplicitCasts()
		{
			string sql1 = @"
SELECT	od.OrderID,
		od.ProductID,
		od.Quantity,
		od.UnitPrice,
		od.Discount
FROM	[Order Details] od
UNION
SELECT	1, 2, CAST(3 AS Int32), CAST(4 AS Decimal), CAST(5 AS Single)
";

			string sql2 = @"
SELECT	od.OrderID,
		od.ProductID,
		od.Quantity,
		od.UnitPrice,
		od.Discount
FROM	[Order Details] od
UNION
SELECT	1, 2, 3, 4, 5
";

			Query query = QueryFactory.CreateQuery();
			query.Text = sql1;
			DataTable dt1 = query.ExecuteDataTable();
			
			query.Text = sql2;
			DataTable dt2 = query.ExecuteDataTable();

            AssertHelpers.AreEqual(dt1, dt2);
		}
	}
}
