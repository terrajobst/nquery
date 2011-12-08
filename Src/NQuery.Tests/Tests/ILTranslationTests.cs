using System;
using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Runtime;

namespace NQuery.Tests
{
	[TestClass]
	public class ILTranslationTests
	{
		#region Helper Types

		public class BaseFunctionContainer
		{
			protected int _value;

			public BaseFunctionContainer(int value)
			{
				_value = value;
			}

			[FunctionBinding("StaticFunction")]
			public static int StaticFunction(int factor)
			{
				return 42 * factor + 1;
			}

			[FunctionBinding("InstanceFunction")]
			public int InstanceFunction(int factor)
			{
				return _value * factor + 2;
			}

			[FunctionBinding("VirtualFunction")]
			public virtual int VirtualFunction(int factor)
			{
				return -1;
			}
		}

		public class DerivedFunctionContainer : BaseFunctionContainer
		{
			public DerivedFunctionContainer(int value)
				: base(value)
			{
			}

			[FunctionBinding("VirtualFunction")]
			public override int VirtualFunction(int factor)
			{
				return _value * factor + 3;
			}
		}

		public class MyClass
		{
			private int _value;

			public MyClass(int value)
			{
				_value = value;
			}

			public int Value
			{
				get { return _value; }
				set { _value = value; }
			}

			public override string ToString()
			{
				return "Class-" +_value;
			}
		}

		public enum MyEnum
		{
			EnumMember1,
			EnumMember2
		}

		public struct MyStruct
		{
			private int _value;

			public MyStruct(int value)
			{
				_value = value;
			}

			public int Value
			{
				get { return _value; }
			}

			public override string ToString()
			{
				return "Struct-" + _value;
			}
		}

		public static class RaiseErrorContainer
		{
			[FunctionBinding("RaiseError", IsDeterministic = false)]
			public static int RaiseError(string msg)
			{
				throw new RuntimeException(msg);
			}
		}

		public sealed class MyDbNullFunction : FunctionBinding
		{
			public override object Invoke(object[] arguments)
			{
				return DBNull.Value;
			}

			public override InvokeParameter[] GetParameters()
			{
				return new InvokeParameter[0];
			}

			public override Type ReturnType
			{
				get { return typeof(int); }
			}

			public override bool IsDeterministic
			{
				get { return false; }
			}

			public override string Name
			{
				get { return "MyDbNullFunction"; }
			}
		}

		public sealed class MyDbNullMethod : MethodBinding
		{
			public override Type DeclaringType
			{
				get { return typeof (int); }
			}

			public override object Invoke(object target, object[] arguments)
			{
				return DBNull.Value;
			}

			public override InvokeParameter[] GetParameters()
			{
				return new InvokeParameter[0];
			}

			public override Type ReturnType
			{
				get { return typeof(int); }
			}

			public override bool IsDeterministic
			{
				get { return false; }
			}

			public override string Name
			{
				get { return "MyDbNullMethod"; }
			}
		}

		public sealed class MyIntMethodProvider : IMethodProvider
		{
			#region IMethodProvider Members

			public MethodBinding[] GetMethods(Type type)
			{
				return new MethodBinding[] {new MyDbNullMethod()};
			}

			#endregion
		}

		#endregion

		[TestMethod]
		public void CallStaticFunction()
		{
			Expression<int> expression = new Expression<int>();
			expression.DataContext.Functions.AddFromContainer(typeof(BaseFunctionContainer));
			expression.Text = "StaticFunction(10)";

			Assert.AreEqual(421, expression.Evaluate());
		}

		[TestMethod]
		public void CallInstanceFunction()
		{
			Expression<int> expression = new Expression<int>();
			expression.DataContext.Functions.AddFromContainer(new BaseFunctionContainer(42));
			expression.Text = "InstanceFunction(10)";

			Assert.AreEqual(422, expression.Evaluate());
		}

		[TestMethod]
		public void CallVirtualFunction()
		{
			Expression<int> expression = new Expression<int>();
			expression.DataContext.Functions.AddFromContainer(new BaseFunctionContainer(42));
			expression.Text = "VirtualFunction(10)";

			Assert.AreEqual(-1, expression.Evaluate());
		}

		[TestMethod]
		public void CallOverridenVirtualFunction()
		{
			Expression<int> expression = new Expression<int>();
			expression.DataContext.Functions.AddFromContainer(new DerivedFunctionContainer(42));
			expression.Text = "VirtualFunction(10)";

			Assert.AreEqual(423, expression.Evaluate());
		}
	
		[TestMethod]
		public void ToStringOnClass()
		{
			Expression<string> expression = new Expression<string>();
			expression.Parameters.Add("Value", typeof (MyClass), new MyClass(42));
			expression.Text = "Value.ToString()";

			Assert.AreEqual("Class-42", expression.Evaluate());
		}

		[TestMethod]
		public void ToStringOnEnum()
		{
			Expression<string> expression = new Expression<string>();
			expression.Parameters.Add("Value", typeof(MyEnum), MyEnum.EnumMember2);
			expression.Text = "Value.ToString()";

			Assert.AreEqual("EnumMember2", expression.Evaluate());
		}

		[TestMethod]
		public void ToStringOnStruct()
		{
			Expression<string> expression = new Expression<string>();
			expression.Parameters.Add("Value", typeof(MyStruct), new MyStruct(42));
			expression.Text = "Value.ToString()";

			Assert.AreEqual("Struct-42", expression.Evaluate());
		}

		[TestMethod]
		public void LogicalExpressionContainingNulls1()
		{
			Query query = new Query();
			query.Text = @"
				SELECT	FALSE AND (SELECT CAST(NULL AS System.Boolean)) AS ShouldBeFalse,
						(SELECT CAST(NULL AS System.Boolean)) AND FALSE AS ShouldBeFalse,
						(SELECT CAST(NULL AS System.Boolean)) OR TRUE AS ShouldBeTrue,
						TRUE OR (SELECT CAST(NULL AS System.Boolean)) AS ShouldBeTrue
			";

			DataTable result = query.ExecuteDataTable();

			Assert.AreEqual(1, result.Rows.Count);
			Assert.AreEqual(false, result.Rows[0][0]);
			Assert.AreEqual(false, result.Rows[0][1]);
			Assert.AreEqual(true, result.Rows[0][2]);
			Assert.AreEqual(true, result.Rows[0][3]);
		}

		[TestMethod]
		public void LogicalExpressionContainingNulls2()
		{
			Query query = new Query();
			query.Text = @"
				SELECT	FalseTable.Value AND NullTable.Value AS ShouldBeFalse,
						NullTable.Value AND FalseTable.Value AS ShouldBeFalse,
						TrueTable.Value OR NullTable.Value AS ShouldBeTrue,
						NullTable.Value OR TrueTable.Value AS ShouldBeTrue
				FROM	(SELECT TRUE AS Value) AS TrueTable,
						(SELECT FALSE AS Value) AS FalseTable,
						(SELECT CAST(NULL AS System.Boolean) AS Value) AS NullTable
			";

			DataTable result = query.ExecuteDataTable();

			Assert.AreEqual(1, result.Rows.Count);
			Assert.AreEqual(false, result.Rows[0][0]);
			Assert.AreEqual(false, result.Rows[0][1]);
			Assert.AreEqual(true, result.Rows[0][2]);
			Assert.AreEqual(true, result.Rows[0][3]);
		}

		[TestMethod]
		public void LogicalExpressionContainingNulls3()
		{
			Query query = new Query();
			query.Text = @"
				SELECT	CASE WHEN FalseTable.Value = 1 AND NullTable.Value = 1 THEN 'True' ELSE 'False' END AS ShouldBeFalse,
						CASE WHEN NullTable.Value = 1 AND FalseTable.Value = 1 THEN 'True' ELSE 'False' END AS ShouldBeFalse,
						CASE WHEN TrueTable.Value = 1 OR NullTable.Value = 1 THEN 'True' ELSE 'False' END AS ShouldBeTrue,
						CASE WHEN NullTable.Value = 1 OR TrueTable.Value = 1 THEN 'True' ELSE 'False' END AS ShouldBeTrue
				FROM	(SELECT 1 AS Value) AS TrueTable,
						(SELECT 0 AS Value) AS FalseTable,
						(SELECT NULL AS Value) AS NullTable
			";

			DataTable result = query.ExecuteDataTable();

			Assert.AreEqual(1, result.Rows.Count);
			Assert.AreEqual("False", result.Rows[0][0]);
			Assert.AreEqual("False", result.Rows[0][1]);
			Assert.AreEqual("True", result.Rows[0][2]);
			Assert.AreEqual("True", result.Rows[0][3]);
		}

		[TestMethod]
		public void LogicalExpressionContainingNulls4()
		{
			Query query = new Query();
			query.Text = @"
				SELECT	FALSE AND NULL AS ShouldBeFalse,
						NULL AND FALSE AS ShouldBeFalse,
						NULL OR TRUE AS ShouldBeTrue,
						TRUE OR NULL AS ShouldBeTrue			
			";

			DataTable result = query.ExecuteDataTable();

			Assert.AreEqual(1, result.Rows.Count);
			Assert.AreEqual(false, result.Rows[0][0]);
			Assert.AreEqual(false, result.Rows[0][1]);
			Assert.AreEqual(true, result.Rows[0][2]);
			Assert.AreEqual(true, result.Rows[0][3]);
		}

		[TestMethod]
		public void LogicalExpressionContainingNulls5()
		{
			Query query = new Query();
			query.Parameters.Add("FalseParam", typeof (bool), false);
			query.Parameters.Add("TrueParam", typeof (bool), true);
			query.Parameters.Add("NullParam", typeof (bool), null);
			query.Text = @"
				SELECT	FalseParam AND NullParam AS ShouldBeFalse,
						NullParam AND FalseParam AS ShouldBeFalse,
						NullParam OR TrueParam AS ShouldBeTrue,
						TrueParam OR NullParam AS ShouldBeTrue			
			";

			DataTable result = query.ExecuteDataTable();

			Assert.AreEqual(1, result.Rows.Count);
			Assert.AreEqual(false, result.Rows[0][0]);
			Assert.AreEqual(false, result.Rows[0][1]);
			Assert.AreEqual(true, result.Rows[0][2]);
			Assert.AreEqual(true, result.Rows[0][3]);
		}

		[TestMethod]
		public void LogicalExpressionContainingNulls6()
		{
			//	SELECT	t.Value = 'X' AND NULL,
			//			t.Value = 'X' OR NULL
			//	FROM	(SELECT 'X' AS VALUE) t

			Query query = new Query();
			query.Text = @"
				SELECT	t.Value = 'X' AND NULL,
						t.Value = 'X' OR NULL,
						t.Value = 'Y' AND NULL,
						t.Value = 'Y' OR NULL
				FROM	(SELECT 'X' AS VALUE) t
			";

			DataTable result = query.ExecuteDataTable();

			Assert.AreEqual(1, result.Rows.Count);
			Assert.AreEqual(DBNull.Value, result.Rows[0][0]);
			Assert.AreEqual(true, result.Rows[0][1]);
			Assert.AreEqual(false, result.Rows[0][2]);
			Assert.AreEqual(DBNull.Value, result.Rows[0][3]);
		}

		[TestMethod]
		public void ShortCircuitLogicalExpression1()
		{
			Query query = new Query();
			query.DataContext.Functions.AddFromContainer(typeof(RaiseErrorContainer));
			query.Text = @"
				SELECT	FALSE AND RaiseError('Should not be called!') = 1 AS ShouldBeFalse,
						TRUE OR RaiseError('Should not be called!') = 1 AS ShouldBeTrue			
			";

			DataTable result = query.ExecuteDataTable();

			Assert.AreEqual(1, result.Rows.Count);
			Assert.AreEqual(false, result.Rows[0][0]);
			Assert.AreEqual(true, result.Rows[0][1]);
		}

		[TestMethod]
		public void ShortCircuitLogicalExpression2()
		{
			Query query = new Query();
			query.DataContext.Functions.AddFromContainer(typeof(RaiseErrorContainer));
			query.Text = @"
				SELECT	FalseTable.Value AND RaiseError('Should not be called!') = 1 AS ShouldBeFalse,
						TrueTable.Value OR RaiseError('Should not be called!') = 1 AS ShouldBeTrue
				FROM	(SELECT TRUE AS Value) AS TrueTable,
						(SELECT FALSE AS Value) AS FalseTable
			";

			DataTable result = query.ExecuteDataTable();

			Assert.AreEqual(1, result.Rows.Count);
			Assert.AreEqual(false, result.Rows[0][0]);
			Assert.AreEqual(true, result.Rows[0][1]);
		}

		[TestMethod]
		public void ShortCircuitLogicalExpression3()
		{
			Query query = new Query();
			query.DataContext.Functions.AddFromContainer(typeof(RaiseErrorContainer));
			query.Parameters.Add("FalseParam", typeof(bool), false);
			query.Parameters.Add("TrueParam", typeof(bool), true);
			query.Text = @"
				SELECT	FalseParam AND RaiseError('Should not be called!') = 1 AS ShouldBeFalse,
						TrueParam OR RaiseError('Should not be called!') = 1 AS ShouldBeTrue			
			";

			DataTable result = query.ExecuteDataTable();

			Assert.AreEqual(1, result.Rows.Count);
			Assert.AreEqual(false, result.Rows[0][0]);
			Assert.AreEqual(true, result.Rows[0][1]);
		}

		[TestMethod]
		public void FoldedConstantsInExpression()
		{
			Expression<int> expression = new Expression<int>();
			expression.DataContext.Constants.Add("MyStringConstant", "Test");
			expression.DataContext.Constants.Add("MyNumberConstant", 42);
			expression.Text = "MyStringConstant.Length * MyNumberConstant";
			int expectedValue = Convert.ToString(expression.DataContext.Constants["MyStringConstant"].Value).Length *
								Convert.ToInt32(expression.DataContext.Constants["MyNumberConstant"].Value);
			Assert.AreEqual(expectedValue, expression.Evaluate());
		}

		[TestMethod]
		public void NonfoldedConstantsInExpression()
		{
			Expression<DateTime> expression = new Expression<DateTime>();
			expression.Parameters.Add("P1", typeof(DateTime), DateTime.Now);
			expression.DataContext.Constants.Add("MyNumberConstant", 42);
			expression.Text = "P1.AddDays(MyNumberConstant)";

			DateTime p1 = Convert.ToDateTime(expression.Parameters["P1"].Value);
			int myNumberConstant = Convert.ToInt32(expression.DataContext.Constants["MyNumberConstant"].Value);
			DateTime expectedValue = p1.AddDays(myNumberConstant);
			Assert.AreEqual(expectedValue, expression.Evaluate());
		}

		[TestMethod]
		public void FoldedConstantsInQuery()
		{
			Query query = new Query();
			query.DataContext.Constants.Add("MyStringConstant", "Test");
			query.DataContext.Constants.Add("MyNumberConstant", 42);
			query.Text = "SELECT MyStringConstant.Length * MyNumberConstant";
			int expectedValue = Convert.ToString(query.DataContext.Constants["MyStringConstant"].Value).Length *
								Convert.ToInt32(query.DataContext.Constants["MyNumberConstant"].Value);
			Assert.AreEqual(expectedValue, query.ExecuteScalar());
		}

		[TestMethod]
		public void NonfoldedConstantsInQuery()
		{
			Query expression = new Query();
			expression.Parameters.Add("P1", typeof(DateTime), DateTime.Now);
			expression.DataContext.Constants.Add("MyNumberConstant", 42);
			expression.Text = "SELECT P1.AddDays(MyNumberConstant)";

			DateTime p1 = Convert.ToDateTime(expression.Parameters["P1"].Value);
			int myNumberConstant = Convert.ToInt32(expression.DataContext.Constants["MyNumberConstant"].Value);
			DateTime expectedValue = p1.AddDays(myNumberConstant);
			Assert.AreEqual(expectedValue, expression.ExecuteScalar());
		}

		[TestMethod]
		public void DirectConstantInExpression()
		{
			Expression<int> expression = new Expression<int>();
			expression.DataContext.Constants.Add("MyNumberConstant", 42);
			expression.Text = "MyNumberConstant";
			int expectedValue = Convert.ToInt32(expression.DataContext.Constants["MyNumberConstant"].Value);
			Assert.AreEqual(expectedValue, expression.Evaluate());
		}

		[TestMethod]
		public void DirectConstantInQuery()
		{
			Query query = new Query();
			query.DataContext.Constants.Add("MyNumberConstant", 42);
			query.Text = "SELECT MyNumberConstant";
			int expectedValue = Convert.ToInt32(query.DataContext.Constants["MyNumberConstant"].Value);
			Assert.AreEqual(expectedValue, query.ExecuteScalar());
		}

		[TestMethod]
		public void DBNullAsParameterValue()
		{
			Expression<bool> expression = new Expression<bool>();
			expression.Text = "left != right";
			expression.Parameters.Add("left", typeof(int), 42);
			expression.Parameters.Add("right", typeof(int), DBNull.Value);
			Assert.IsFalse(expression.Evaluate());	
		}

		[TestMethod]
		public void DBNullAsPropertyValue()
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("ID1", typeof(int));
			dataTable.Columns.Add("ID2", typeof(int));
			DataRow dataRow = dataTable.Rows.Add(DBNull.Value, DBNull.Value);

			Expression<bool> expression = new Expression<bool>();
			expression.Text = "row.ID1 = row.ID2";
			expression.Parameters.Add("row", typeof(DataRow), dataRow, DataRowPropertyProvider.GetProperties(dataTable));
			Assert.IsFalse(expression.Evaluate());
		}

		[TestMethod]
		public void DBNullAsFunctionResult()
		{
			Expression<bool> expression = new Expression<bool>();
			expression.Text = "IntValue = MyDbNullFunction()";
			expression.DataContext.Functions.Add(new MyDbNullFunction());
			expression.Parameters.Add("IntValue", typeof(int), 42);
			Assert.IsFalse(expression.Evaluate());
		}

		[TestMethod]
		public void DBNullAsMethodResult()
		{
			Expression<bool> expression = new Expression<bool>();
			expression.Text = "IntValue = IntValue.MyDbNullMethod()";
			expression.Parameters.Add("IntValue", typeof(int), 42);
			expression.DataContext.MetadataContext.MethodProviders.Register(typeof(int), new MyIntMethodProvider());
			Assert.IsFalse(expression.Evaluate());
		}
	}
}
