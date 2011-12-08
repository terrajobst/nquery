using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	[TestClass]
	public class CalculateScalarTests
	{
		[TestMethod]
		public void TestOrderRemains()
		{
			Query query = QueryFactory.CreateQuery();
			query.Text = @"
SELECT	TRIM(c.CategoryName) as CategoryName,
		FIRST(c.Description) as Description,
		FIRST(c.Picture) as Picture,
		CONCAT(p.ProductName) Products,
		COUNT(*) * 2 ProductCount
		
FROM	Products p
			INNER JOIN Categories c ON c.CategoryID = p.CategoryID
GROUP	BY c.CategoryName
ORDER	BY COUNT(*) DESC
";
			using (QueryDataReader reader = query.ExecuteSchemaReader())
			{
				Assert.AreEqual(0, reader.GetOrdinal("CategoryName"));
				Assert.AreEqual(1, reader.GetOrdinal("Description"));
				Assert.AreEqual(2, reader.GetOrdinal("Picture"));
				Assert.AreEqual(3, reader.GetOrdinal("Products"));
				Assert.AreEqual(4, reader.GetOrdinal("ProductCount"));
			}
		}
	}
}
