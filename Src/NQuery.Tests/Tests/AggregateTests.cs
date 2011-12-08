using System;
using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	[TestClass]
	public class AggregateTests : AutomatedTestFixtureBase
	{
		[TestMethod]
		public void AggregateContainingDirectSingleRowSubselect()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void AggregateContainingDirectSingleRowSubselectWithAggregate()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void AggregateContainingIndirectAllSubquery()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void AggregateContainingIndirectAnySubquery()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void AggregateContainingIndirectExistsSubquery()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void AggregateContainingIndirectSingleRowSubselect()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void AggregateContainingIndirectSingleRowSubselectWithAggregate()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void All()
		{
			RunTestOfCallingMethod();			
		}

        [TestMethod]
        public void GroupedQueryReturnsManyRowsOnNonEmptyInput()
        {
            RunTestOfCallingMethod();
        }
        
	    [TestMethod]
        public void GroupedQueryReturnsZeroRowsOnConstantScan()
        {
            RunTestOfCallingMethod();
        }

	    [TestMethod]
        public void GroupedQueryReturnsZeroRowsOnEmptyInput()
        {
            RunTestOfCallingMethod();
        }
	    
        [TestMethod]
	    public void UngroupedQueryReturnsRowOnEmptyInput()
	    {
	        RunTestOfCallingMethod();
	    }

	    [TestMethod]
        public void UngroupedQueryReturnsRowOnEmptyConstantScan()
        {
            RunTestOfCallingMethod();
        }

        [TestMethod]
        public void UngroupedQueryReturnsRowOnNonEmptyInput()
        {
            RunTestOfCallingMethod();
        }

        private class Dto
		{
			public int ID;
			public short Value;

			public Dto(int id, short value)
			{
				ID = id;
				Value = value;
			}
		}
		
		[TestMethod]
		public void SingleValueGetsConvertedCorrectly()
		{
			Dto[] dtos = new Dto[] {new Dto(1, 4), new Dto(2, 5), new Dto(2, 6)};
			
			Query query = new Query();
			query.Text = "SELECT SUM(ID) Int32Sum, SUM(Value) Int16Sum, SUM(CAST(Value AS INT64)) Int64Sum FROM Dtos GROUP BY ID ORDER BY 1";
			query.DataContext = new DataContext();
			query.DataContext.Tables.Add(dtos, "Dtos");
			
			DataTable dataTable = query.ExecuteDataTable();

			Assert.AreEqual(typeof(int), dataTable.Columns["Int32Sum"].DataType);
			Assert.AreEqual(typeof(int), dataTable.Columns["Int16Sum"].DataType);
			Assert.AreEqual(typeof(long), dataTable.Columns["Int64Sum"].DataType);
			
			Assert.AreEqual(2, dataTable.Rows.Count);
			Assert.AreEqual(1, dataTable.Rows[0]["Int32Sum"]);
			Assert.AreEqual((short)9, dataTable.Rows[0]["Int16Sum"]);
			Assert.AreEqual((long)9, dataTable.Rows[0]["Int64Sum"]);

			Assert.AreEqual(4, dataTable.Rows[0]["Int32Sum"]);
			Assert.AreEqual((short)11, dataTable.Rows[0]["Int16Sum"]);
			Assert.AreEqual((long)11, dataTable.Rows[0]["Int64Sum"]);
		}
	}
}
