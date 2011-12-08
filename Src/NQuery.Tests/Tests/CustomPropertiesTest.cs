using System;
using System.Collections;
using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Runtime;

namespace NQuery.Tests
{
	[TestClass]
	public class CustomPropertiesTest
	{
		[TestMethod]
		public void DataRowProperties()
		{
			Query query = QueryFactory.CreateQuery();
			query.Text = @"
SELECT	e.EmployeeID,
		e.FirstName,
		e.LastName
FROM	Employees e
WHERE	e.EmployeeID BETWEEN 1 AND 2
ORDER	BY 1
";
			DataTable result = query.ExecuteDataTable();
			
			Assert.AreEqual(2, result.Rows.Count);

			Expression<object> expr = new Expression<object>();
			expr.DataContext = query.DataContext;

			ParameterBinding param = new ParameterBinding("@ROW", typeof (DataRow), DataRowPropertyProvider.GetProperties(result));
			expr.Parameters.Add(param);
			
			param.Value = result.Rows[0];
			expr.Text = "@ROW.EmployeeID";
			Assert.AreEqual(1, expr.Evaluate());
			expr.Text = "@ROW.FirstName";
			Assert.AreEqual("Nancy", expr.Evaluate());
			expr.Text = "@ROW.LastName";
			Assert.AreEqual("Davolio", expr.Evaluate());
			
			param.Value = result.Rows[1];
			expr.Text = "@ROW.EmployeeID";
			Assert.AreEqual(2, expr.Evaluate());
			expr.Text = "@ROW.FirstName";
			Assert.AreEqual("Andrew", expr.Evaluate());
			expr.Text = "@ROW.LastName";
			Assert.AreEqual("Fuller", expr.Evaluate());
		}
		
		[TestMethod]
		public void DictionaryProperties()
		{
			Query query = QueryFactory.CreateQuery();
			query.Text = @"
SELECT	e.EmployeeID,
		e.FirstName,
		e.LastName
FROM	Employees e
WHERE	e.EmployeeID BETWEEN 1 AND 2
ORDER	BY 1
";
			
			Hashtable hashtable = new Hashtable();
			hashtable["EmployeeID"] = -1;
			hashtable["FirstName"] = "";
			hashtable["LastName"] = "";
			
			ParameterBinding param = new ParameterBinding("@ROW", typeof (IDictionary), DictionaryPropertyProvider.GetProperties(hashtable));
			Expression<object> expr = new Expression<object>();
			expr.DataContext = query.DataContext;
			expr.Parameters.Add(param);

			DataTable dataTable = query.ExecuteDataTable();
			foreach (DataRow row in dataTable.Rows)
			{
				Hashtable rowHashtable = new Hashtable();
				param.Value = rowHashtable;

				foreach (DataColumn col in dataTable.Columns)
					rowHashtable[col.ColumnName] = row[col];
								
				foreach (DataColumn col in dataTable.Columns)
				{
					expr.Text = "@ROW.[" + col.ColumnName + "]";
					Assert.AreEqual(row[col], expr.Evaluate());
				}				
			}
		}
		
		[TestMethod]
		public void DictionaryPropertiesConstantWithNullPropValue()
		{
			Hashtable hashtable = new Hashtable();
			hashtable["EmployeeID"] = -1;
			hashtable["FirstName"] = "Immo";
			hashtable["DOB"] = new DateTime(1981, 10, 19);
			
			PropertyBinding[] customProps = DictionaryPropertyProvider.GetProperties(hashtable);
			hashtable["FirstName"] = null;

			Expression<object> expr = new Expression<object>();	
			expr.DataContext.Constants.Add(new ConstantBinding("Constant", hashtable, customProps));
			expr.Text = "Constant.FirstName = 'Immo'";
			
			Assert.AreEqual(typeof(bool), expr.Resolve());
			Assert.AreEqual(null, expr.Evaluate());
		}

		[TestMethod]
		public void DictionaryPropertiesParameterWithNullPropValue()
		{
			Hashtable hashtable = new Hashtable();
			hashtable["EmployeeID"] = -1;
			hashtable["FirstName"] = "Immo";
			hashtable["DOB"] = new DateTime(1981, 10, 19);
			
			PropertyBinding[] customProps = DictionaryPropertyProvider.GetProperties(hashtable);
			hashtable["FirstName"] = null;

			Expression<object> expr = new Expression<object>();	
			expr.Parameters.Add(new ParameterBinding("@Parameter", typeof(IDictionary), customProps));
			expr.Parameters["@Parameter"].Value = hashtable;
			
			expr.Text = "@Parameter.FirstName = 'Immo'";
			
			Assert.AreEqual(typeof(bool), expr.Resolve());
			Assert.AreEqual(null, expr.Evaluate());
		}
		
		[TestMethod]
		public void DictionaryPropertiesParameterNullValue()
		{
			Hashtable hashtable = new Hashtable();
			hashtable["EmployeeID"] = -1;
			hashtable["FirstName"] = "Immo";
			hashtable["DOB"] = new DateTime(1981, 10, 19);
			
			PropertyBinding[] customProps = DictionaryPropertyProvider.GetProperties(hashtable);

			Expression<object> expr = new Expression<object>();	
			expr.Parameters.Add(new ParameterBinding("@Parameter", typeof(IDictionary), customProps));
			expr.Parameters["@Parameter"].Value = null;
			
			expr.Text = "@Parameter.FirstName = 'Immo'";
			
			Assert.AreEqual(typeof(bool), expr.Resolve());
			Assert.AreEqual(null, expr.Evaluate());
		}
	}
}
