using System;
using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Runtime;

namespace NQuery.Tests
{
    [TestClass]
    public class ConstantFoldingTests
    {
    	[TestMethod]
        public void ConstantFoldingDoesPreserveType()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("NonNullInt", typeof (int));
            dataTable.Columns.Add("NullInt", typeof (int));

            DataRow dataRow = dataTable.NewRow();
            dataRow["NonNullInt"] = 1;
            dataTable.Rows.Add(dataRow);
            
            DataColumnPropertyBinding[] dataRowProperties = DataRowPropertyProvider.GetProperties(dataTable);
            
            DataContext dataContext = new DataContext();
            dataContext.Constants.Add("DataRow", dataRow, dataRowProperties);

			Expression<object> exprWithNonNullInt = new Expression<object>();
            exprWithNonNullInt.DataContext = dataContext;
            exprWithNonNullInt.Text = "DataRow.NonNullInt";
            Assert.AreEqual(typeof(int), exprWithNonNullInt.Resolve());
            Assert.AreEqual(1, exprWithNonNullInt.Evaluate());

			Expression<object> exprWithNullInt = new Expression<object>();
            exprWithNullInt.DataContext = dataContext;
            exprWithNullInt.Text = "DataRow.NullInt";
            Assert.AreEqual(typeof(int), exprWithNullInt.Resolve());
            Assert.AreEqual(null, exprWithNullInt.Evaluate());
        }
    }
}
