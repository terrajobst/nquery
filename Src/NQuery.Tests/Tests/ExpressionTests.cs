using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Runtime;

namespace NQuery.Tests
{
	[TestClass]
	public class ExpressionTests
	{
		public class Base
		{
		}

		public class Derived : Base
		{
		}

		public static class FunctionContainer
		{
			[FunctionBinding("IntIdent")]
			public static int? IntIdent(int? value)
			{
				return value;
			}

			[FunctionBinding("IntNullIf")]
			public static int? IntNullIf(int value, int nullValue)
			{
				if (value == nullValue)
					return null;

				return value;
			}
		}

		[TestMethod]
		public void SimpleExpression()
		{
			Expression<string> expr = new Expression<string>("'Hello' + ' ' + 'World!'");
			string result = expr.Evaluate();

			Assert.AreEqual("Hello World!", result);
		}

		#region Static Tests

		[TestMethod]
		public void SimpleConversion()
		{
			Expression<double> expr = new Expression<double>("2 * 100");
			double result = expr.Evaluate();

			Assert.AreEqual(200.0d, result);
		}

		[TestMethod]
		public void ConversionToLessSpecificType1()
		{
			Derived derived = new Derived();

			Expression<Base> expr = new Expression<Base>("Test");
			expr.Parameters.Add("Test", typeof(Derived), derived);
			Base result = expr.Evaluate();

			Assert.AreSame(derived, result);
		}

		[TestMethod]
		public void ConversionToLessSpecificType2()
		{
			Derived derived = new Derived();

			Expression<Base> expr = new Expression<Base>("Test");
			expr.Parameters.Add("Test", typeof(Base), derived);
			Base result = expr.Evaluate();

			Assert.AreSame(derived, result);
		}

		[TestMethod]
		public void ConversionToLessSpecficWithBoxingToObject()
		{
			Expression<object> expr = new Expression<object>("2 + 2");
			object result = expr.Evaluate();

			Assert.AreEqual(4, result);
		}

		[TestMethod]
		public void ConversionToLessSpecficWithBoxingToValueType()
		{
			Expression<ValueType> expr = new Expression<ValueType>("2 + 2");
			ValueType result = expr.Evaluate();

			Assert.AreEqual(4, result);
		}

		[TestMethod]
		public void ConversionToLessSpecficWithBoxingToEnum()
		{
			Expression<Enum> expr = new Expression<Enum>("MyDayOfWeek");
			expr.Parameters.Add("MyDayOfWeek", typeof(Enum), DayOfWeek.Wednesday);
			Enum result = expr.Evaluate();

			Assert.AreEqual(DayOfWeek.Wednesday, result);
		}

		[TestMethod]
		public void ConversionToMoreSpecificTypeWithoutCast()
		{
			Derived derived = new Derived();

			Expression<Derived> expr = new Expression<Derived>("Test");
			expr.Parameters.Add("Test", typeof(Base), derived);

			try
			{
				expr.Evaluate();
				Assert.Fail("The expression should not be compiled");
			}
			catch (CompilationException ex)
			{
				Assert.AreEqual(1, ex.CompilationErrors.Count);
				Assert.AreEqual("Cannot cast '@Test' from 'Base' to 'Derived'.", ex.CompilationErrors[0].Text);
			}
		}

		[TestMethod]
		public void ConversionToMoreSpecificTypeWithCast()
		{
			Derived derived = new Derived();

			Expression<Derived> expr = new Expression<Derived>("CAST(Test AS 'NQuery.Tests.ExpressionTests+Derived')");
			expr.Parameters.Add("Test", typeof(Base), derived);

			Derived result = expr.Evaluate();
			Assert.AreSame(derived, result);
		}

		[TestMethod]
		public void ConversionToMoreSpecificTypeWithCastAndUnboxing()
		{
			Expression<int> expr = new Expression<int>("CAST(Test AS INT) + CAST(Test AS INT)");
			expr.Parameters.Add("Test", typeof(object), 2);

			int result = expr.Evaluate();
			Assert.AreEqual(4, result);
		}

		[TestMethod]
		public void Int32WithZeroForNull()
		{
			Expression<int> expr = new Expression<int>("NULL");
			int result = expr.Evaluate();
			Assert.AreEqual(0, result);
		}

		[TestMethod]
		public void Int32WithMinusOneForNull()
		{
			Expression<int> expr = new Expression<int>("NULL");
			expr.NullValue = -1;
			int result = expr.Evaluate();
			Assert.AreEqual(-1, result);
		}

		[TestMethod]
		public void NullableInt32WithNullForNull()
		{
			Expression<int?> expr = new Expression<int?>("NULL");
			int? result = expr.Evaluate();
			Assert.AreEqual(null, result);
		}

		[TestMethod]
		public void NullableInt32WithMinusOneForNull()
		{
			Expression<int?> expr = new Expression<int?>("NULL");
			expr.NullValue = -1;
			int? result = expr.Evaluate();
			Assert.AreEqual(-1, result);
		}

		[TestMethod]
		public void BooleanWithFalseForNull()
		{
			Expression<bool> expr = new Expression<bool>("NULL");
			bool result = expr.Evaluate();
			Assert.AreEqual(false, result);
		}

		[TestMethod]
		public void BooleanWithTrueForNull()
		{
			Expression<bool> expr = new Expression<bool>("NULL");
			expr.NullValue = true;
			bool result = expr.Evaluate();
			Assert.AreEqual(true, result);
		}

		[TestMethod]
		public void NullableBooleanWithNullForNull()
		{
			Expression<bool?> expr = new Expression<bool?>("NULL");
			bool? result = expr.Evaluate();
			Assert.AreEqual(null, result);
		}

		[TestMethod]
		public void NullableBooleanWithFalseForNull()
		{
			Expression<bool?> expr = new Expression<bool?>("NULL");
			expr.NullValue = false;
			bool? result = expr.Evaluate();
			Assert.AreEqual(false, result);
		}

		[TestMethod]
		public void NullableBooleanWithTrueForNull()
		{
			Expression<bool?> expr = new Expression<bool?>("NULL");
			expr.NullValue = true;
			bool? result = expr.Evaluate();
			Assert.AreEqual(true, result);
		}

		#endregion

		#region Dynamic Tests

		[TestMethod]
		public void DynamicSimpleConversion()
		{
			Expression<object> expr = new Expression<object>("2 * 100");
			expr.TargetType = typeof (double);
			double result = (double) expr.Evaluate();

			Assert.AreEqual(200.0d, result);
		}

		[TestMethod]
		public void DynamicConversionToLessSpecificType1()
		{
			Derived derived = new Derived();

			Expression<object> expr = new Expression<object>("Test");
			expr.TargetType = typeof (Base);
			expr.Parameters.Add("Test", typeof(Derived), derived);
			Base result = (Base) expr.Evaluate();

			Assert.AreSame(derived, result);
		}

		[TestMethod]
		public void DynamicConversionToLessSpecificType2()
		{
			Derived derived = new Derived();

			Expression<object> expr = new Expression<object>("Test");
			expr.TargetType = typeof (Base);
			expr.Parameters.Add("Test", typeof(Base), derived);
			Base result = (Base) expr.Evaluate();

			Assert.AreSame(derived, result);
		}

		[TestMethod]
		public void DynamicConversionToMoreSpecificTypeWithoutCast()
		{
			Derived derived = new Derived();

			Expression<object> expr = new Expression<object>("Test");
			expr.TargetType = typeof (Derived);
			expr.Parameters.Add("Test", typeof(Base), derived);

			try
			{
				expr.Evaluate();
				Assert.Fail("The expression should not be compiled");
			}
			catch (CompilationException ex)
			{
				Assert.AreEqual(1, ex.CompilationErrors.Count);
				Assert.AreEqual("Cannot cast '@Test' from 'Base' to 'Derived'.", ex.CompilationErrors[0].Text);
			}
		}

		[TestMethod]
		public void DynamicConversionToMoreSpecificTypeWithCast()
		{
			Derived derived = new Derived();

			Expression<object> expr = new Expression<object>("CAST(Test AS 'NQuery.Tests.ExpressionTests+Derived')");
			expr.TargetType = typeof(Derived);
			expr.Parameters.Add("Test", typeof(Base), derived);

			Derived result = (Derived) expr.Evaluate();
			Assert.AreSame(derived, result);
		}

		[TestMethod]
		public void DynamicConversionToMoreSpecificTypeWithCastAndUnboxing()
		{
			Expression<object> expr = new Expression<object>("CAST(Test AS INT) + CAST(Test AS INT)");
			expr.TargetType = typeof (int);
			expr.Parameters.Add("Test", typeof(object), 2);

			int result = (int) expr.Evaluate();
			Assert.AreEqual(4, result);
		}

		[TestMethod]
		public void DynamicInt32WithNullForNull()
		{
			Expression<object> expr = new Expression<object>("NULL");
			expr.TargetType = typeof(int);

			object result = expr.Evaluate();
			Assert.AreEqual(null, result);
		}

		[TestMethod]
		public void DynamicInt32WithZeroForNull()
		{
			Expression<object> expr = new Expression<object>("NULL");
			expr.TargetType = typeof (int);
			expr.NullValue = 0;

			int result = (int) expr.Evaluate();
			Assert.AreEqual(0, result);
		}

		[TestMethod]
		public void DynamicInt32WithMinusOneForNull()
		{
			Expression<object> expr = new Expression<object>("NULL");
			expr.TargetType = typeof (int);
			expr.NullValue = -1;
			int result = (int) expr.Evaluate();
			Assert.AreEqual(-1, result);
		}

		[TestMethod]
		public void DynamicNullableInt32WithNullForNull()
		{
			Expression<object> expr = new Expression<object>("NULL");
			expr.TargetType = typeof (int?);

			object result = expr.Evaluate();
			Assert.AreEqual(null, result);
		}

		[TestMethod]
		public void DynamicNullableInt32WithMinusOneForNull()
		{
			Expression<object> expr = new Expression<object>("NULL");
			expr.TargetType = typeof(int?);
			expr.NullValue = -1;

			int? result = (int?) expr.Evaluate();
			Assert.AreEqual(-1, result);
		}

		[TestMethod]
		public void DynamicBooleanWithNullForNull()
		{
			Expression<object> expr = new Expression<object>("NULL");
			expr.TargetType = typeof(bool);

			object result = expr.Evaluate();
			Assert.AreEqual(null, result);
		}

		[TestMethod]
		public void DynamicBooleanWithFalseForNull()
		{
			Expression<object> expr = new Expression<object>("NULL");
			expr.TargetType = typeof(bool);
			expr.NullValue = false;

			bool result = (bool) expr.Evaluate();
			Assert.AreEqual(false, result);
		}

		[TestMethod]
		public void DynamicBooleanWithTrueForNull()
		{
			Expression<object> expr = new Expression<object>("NULL");
			expr.TargetType = typeof(bool);
			expr.NullValue = true;

			bool result = (bool) expr.Evaluate();
			Assert.AreEqual(true, result);
		}

		[TestMethod]
		public void DynamicNullableBooleanWithNullForNull()
		{
			Expression<object> expr = new Expression<object>("NULL");
			expr.TargetType = typeof(bool?);

			bool? result = (bool?) expr.Evaluate();
			Assert.AreEqual(null, result);
		}

		[TestMethod]
		public void DynamicNullableBooleanWithFalseForNull()
		{
			Expression<object> expr = new Expression<object>("NULL");
			expr.TargetType = typeof(bool?);
			expr.NullValue = false;

			bool? result = (bool?) expr.Evaluate();
			Assert.AreEqual(false, result);
		}

		[TestMethod]
		public void DynamicNullableBooleanWithTrueForNull()
		{
			Expression<object> expr = new Expression<object>("NULL");
			expr.TargetType = typeof(bool?);
			expr.NullValue = true;

			bool? result = (bool?) expr.Evaluate();
			Assert.AreEqual(true, result);
		}

		[TestMethod]
		public void DynamicTargetTypeDetectsConflictWithGenericType()
		{
			Expression<DateTime> expr = new Expression<DateTime>("NULL");
			try
			{
				expr.TargetType = typeof(int);
			}
			catch (ArgumentException ex)
			{
				Assert.AreEqual("Cannot narrow down the target type to 'System.Int32' since the static expression type 'System.DateTime' would not be assignable from the new target type 'System.Int32' anymore.\r\nParameter name: value", ex.Message);
			}
		}
		
		#endregion

		[TestMethod]
		public void NullableIntsInExpression1()
		{
			Expression<int> expr = new Expression<int>();
			expr.Text = "NullableInt1 + NullableInt2";
			expr.Parameters.Add("NullableInt1", typeof (int?), 1);
			expr.Parameters.Add("NullableInt2", typeof (int?), 2);
			int result = expr.Evaluate();
			Assert.AreEqual(3, result);
		}

		[TestMethod]
		public void NullableIntsInExpression2()
		{
			Expression<int?> expr = new Expression<int?>();
			expr.Text = "NullableInt1 + NullableInt2";
			expr.Parameters.Add("NullableInt1", typeof(int?), null);
			expr.Parameters.Add("NullableInt2", typeof(int?), 2);
			int? result = expr.Evaluate();
			Assert.AreEqual(null, result);
		}

		[TestMethod]
		public void CallingFunctionThatReturnsNullableInt1()
		{
			Expression<int?> expr = new Expression<int?>();
			expr.DataContext.Functions.AddFromContainer(typeof(FunctionContainer));
			expr.Text = "IntNullIf(4, 4)";
			int? result = expr.Evaluate();
			Assert.AreEqual(null, result);
		}

		[TestMethod]
		public void CallingFunctionThatReturnsNullableInt2()
		{
			Expression<int?> expr = new Expression<int?>();
			expr.DataContext.Functions.AddFromContainer(typeof(FunctionContainer));
			expr.Text = "IntNullIf(4, 4) + 4";
			int? result = expr.Evaluate();
			Assert.AreEqual(null, result);
		}

		[TestMethod]
		public void CallingFunctionWithNullableInt()
		{
			Expression<int> expr = new Expression<int>();
			expr.DataContext.Functions.AddFromContainer(typeof(FunctionContainer));
			expr.Text = "IntIdent(42)";
			int result = expr.Evaluate();
			Assert.AreEqual(42, result);
		}
	}
}
