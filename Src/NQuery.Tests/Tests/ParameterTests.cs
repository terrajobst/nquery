using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	[TestClass]
	public class ParameterTests
	{
		private Query _query;

		public ParameterTests()
		{
		}

		[TestInitialize]
		public void SetUp()
		{
			_query = QueryFactory.CreateQuery();
		}
		
		[TestMethod]
		public void OptionalAtIsIgnoredInCollection()
		{
			_query.Parameters.Add("@ID", typeof (int), 5);
			_query.Text = "SELECT FirstName FROM Employees WHERE EmployeeId = @ID";
			
			Assert.IsNotNull(_query.Parameters["@ID"]);
			Assert.AreEqual("Steven", _query.ExecuteScalar());
		}

		[TestMethod]
		public void OmittingOptionalAtWorksInCollection()
		{
			_query.Parameters.Add("ID", typeof (int), 5);
			_query.Text = "SELECT FirstName FROM Employees WHERE EmployeeId = @ID";
			
			Assert.IsNotNull(_query.Parameters["ID"]);
			Assert.AreEqual("Steven", _query.ExecuteScalar());
		}
		
		[TestMethod]
		public void OmittingOptionalAtWorksInExpression()
		{
			Expression<object> expression = new Expression<object>("MyParam1 + ' + ' + @MyParam2");
			expression.Parameters.Add("@MyParam1", typeof (string));
			expression.Parameters.Add("MyParam2", typeof (string));
			expression.Parameters["MyParam1"].Value = "MyParam1Value";
			expression.Parameters["@MyParam2"].Value = "MyParam2Value";
			object value = expression.Evaluate();
			Assert.AreEqual(value, "MyParam1Value + MyParam2Value");
		}

		[TestMethod]
		public void OmittingOptionalAtWorksInQuery()
		{
			Query query = new Query("SELECT MyParam1 + ' + ' + @MyParam2");
			query.Parameters.Add("@MyParam1", typeof (string));
			query.Parameters.Add("MyParam2", typeof (string));
			query.Parameters["MyParam1"].Value = "MyParam1Value";
			query.Parameters["@MyParam2"].Value = "MyParam2Value";
			object value = query.ExecuteScalar();
			Assert.AreEqual(value, "MyParam1Value + MyParam2Value");
		}
		
		[TestMethod]
		public void AmbigousReferenceBetweenParameterAndConstantIsCaught()
		{
			Expression<object> expression = new Expression<object>("MyParamOrConstant + ' result'");
			expression.Parameters.Add("@MyParamOrConstant", typeof (string));
			expression.Parameters["@MyParamOrConstant"].Value = "ParamValue";			
			expression.DataContext.Constants.Add("MyParamOrConstant", "ConstantValue");
			
			try
			{
				expression.Evaluate();
				Assert.Fail("Ambigous reference between parameters and constants must be detected.");
			}
			catch (CompilationException ex)
			{
				
				Assert.AreEqual(1, ex.CompilationErrors.Count);
				Assert.AreEqual(ErrorId.AmbiguousReference, ex.CompilationErrors[0].Id);
				Assert.AreEqual("Identifier 'MyParamOrConstant' is ambiguous between 'MyParamOrConstant: Constant, @MyParamOrConstant: Parameter'.", ex.CompilationErrors[0].Text);
			}
		}

		[TestMethod]
		public void AmbigousReferenceBetweenParameterAndColumnIsCaught()
		{
			Query query = QueryFactory.CreateQuery();
			query.Text = "SELECT OrderId FROM Orders";
			query.Parameters.Add("@OrderId", typeof (string));
			query.Parameters["@OrderId"].Value = "ParamValue";			
			
			try
			{
				query.ExecuteDataTable();
				Assert.Fail("Ambigous reference between parameters and columns must be detected.");
			}
			catch (CompilationException ex)
			{
				Assert.AreEqual(1, ex.CompilationErrors.Count);
				Assert.AreEqual(ErrorId.AmbiguousReference, ex.CompilationErrors[0].Id);
				Assert.AreEqual("Identifier 'OrderId' is ambiguous between 'Orders.OrderID: ColumnRef, @OrderId: Parameter'.", ex.CompilationErrors[0].Text);
			}
		}

		[TestMethod]
		public void AmbigousReferenceBetweenParameterAndTableAliasIsCaught()
		{
			Query query = QueryFactory.CreateQuery();
			query.Text = "SELECT o FROM Orders AS o";
			query.Parameters.Add("@o", typeof (string));
			query.Parameters["@o"].Value = "ParamValue";			
			
			try
			{
				query.ExecuteDataTable();
				Assert.Fail("Ambigous reference between parameters and table alias must be detected.");
			}
			catch (CompilationException ex)
			{
				Assert.AreEqual(1, ex.CompilationErrors.Count);
				Assert.AreEqual(ErrorId.AmbiguousReference, ex.CompilationErrors[0].Id);
				Assert.AreEqual("Identifier 'o' is ambiguous between '@o: Parameter, o: TableRef'.", ex.CompilationErrors[0].Text);
			}
		}
		
		[TestMethod]
		public void ExpressionParameterChangeTriggersRecompilation()
		{
			Expression<object> expression = new Expression<object>("@Param");
			expression.Parameters.Add("@Param", typeof(string));
			expression.Parameters["@Param"].Value = "Test";
			Assert.AreEqual("Test", expression.Evaluate());

			expression.Parameters.Remove("@Param");
			expression.Parameters.Add("@Param", typeof(string));
			expression.Parameters["@Param"].Value = "XXX";			
			Assert.AreEqual("XXX", expression.Evaluate());
		}
		
		[TestMethod]
		public void QueryParameterChangeTriggersRecompilation()
		{
			Query query = new Query("SELECT @Param");
			query.Parameters.Add("@Param", typeof(string));
			query.Parameters["@Param"].Value = "Test";
			Assert.AreEqual("Test", query.ExecuteScalar());

			query.Parameters.Remove("@Param");
			query.Parameters.Add("@Param", typeof(string));
			query.Parameters["@Param"].Value = "XXX";			
			Assert.AreEqual("XXX", query.ExecuteScalar());
		}
	}
}
