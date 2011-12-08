using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Runtime;

namespace NQuery.Tests
{
	[TestClass]
	public class BinderTests
	{
		private Query _query;
		private Expression<object> _expression;

		#region Custom Types
		
		public struct MyInt
		{
			private int _value;

			public MyInt(int value)
			{
				_value = value;
			}
			
			public int Value
			{
				get { return _value; }
				set { _value = value; }
			}
			
			public static implicit operator MyInt(int value)
			{
				return new MyInt(value);
			}

			public static explicit operator MyInt(MyLong myLong)
			{
				return new MyInt((int) myLong.Value);
			}
			
			public static bool operator ==(MyInt value1, MyInt value2)
			{
				return value1.Value == value2.Value;
			}

			public static bool operator !=(MyInt value1, MyInt value2)
			{
				return value1.Value != value2.Value;
			}

			public override int GetHashCode()
			{
				return _value;
			}

			public override bool Equals(object obj)
			{
				if (!(obj is MyInt)) return false;
				MyInt myInt = (MyInt) obj;
				if (_value != myInt._value) return false;
				return true;
			}
		}
		
		public class MyLong
		{
			private long _value;

			public MyLong(long value)
			{
				_value = value;
			}
			
			public long Value
			{
				get { return _value; }
				set { _value = value; }
			}
			
			public static implicit operator MyLong(long value)
			{
				return new MyLong(value);
			}

			public static implicit operator MyLong(MyInt myInt)
			{
				return new MyLong(myInt.Value);
			}
			
			public static bool operator ==(MyLong value1, MyLong value2)
			{
				return value1.Value == value2.Value;
			}

			public static bool operator !=(MyLong value1, MyLong value2)
			{
				return value1.Value != value2.Value;
			}

			public override int GetHashCode()
			{
				return (int) _value;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(this, obj))
					return true;
				
				MyLong myLong = obj as MyLong;
				if (myLong == null)
					return false;
				
				if (_value != myLong._value)
					return false;
				
				return true;
			}
		}
		
		public class MyBaseOperator
		{
			private int _value;

			public MyBaseOperator(int value)
			{
				_value = value;
			}

			public int Value
			{
				get { return _value; }
			}

			public override string ToString()
			{
				return _value.ToString();
			}

			public static MyBaseOperator operator+(MyBaseOperator left, MyBaseOperator right)
			{
				return new MyBaseOperator(left._value + right._value);
			}

			public static MyBaseOperator operator-(MyBaseOperator left, MyBaseOperator right)
			{
				return new MyBaseOperator(left._value - right._value);
			}
		}

		public class MyDerivedOperator : MyBaseOperator
		{
			public MyDerivedOperator(int value)
				: base(value)
			{
			}
		}

		public class MyDerivedOperatorRedeclared : MyBaseOperator
		{
			public MyDerivedOperatorRedeclared(int value) : base(value)
			{
			}

			public static MyDerivedOperatorRedeclared operator +(MyDerivedOperatorRedeclared left, MyDerivedOperatorRedeclared right)
			{
				return new MyDerivedOperatorRedeclared(left.Value + right.Value);
			}

			public static MyDerivedOperatorRedeclared operator -(MyDerivedOperatorRedeclared left, MyDerivedOperatorRedeclared right)
			{
				return new MyDerivedOperatorRedeclared(left.Value - right.Value);
			}
		}

		[Flags]
		public enum MyByteEnum : byte
		{
			None,
			Value1 = 0x0001,
			Value2 = 0x0002,
			Value3 = 0x0004
		}

		[Flags]
		public enum MySByteEnum : sbyte
		{
			None,
			Value1 = 0x0001,
			Value2 = 0x0002,
			Value3 = 0x0004
		}

		[Flags]
		public enum MyShortEnum : short
		{
			None,
			Value1 = 0x0001,
			Value2 = 0x0002,
			Value3 = 0x0004
		}

		[Flags]
		public enum MyUShortEnum : ushort
		{
			None,
			Value1 = 0x0001,
			Value2 = 0x0002,
			Value3 = 0x0004
		}

		[Flags]
		public enum MyIntEnum : int
		{
			None,
			Value1 = 0x0001,
			Value2 = 0x0002,
			Value3 = 0x0004
		}

		[Flags]
		public enum MyUIntEnum : uint
		{
			None,
			Value1 = 0x0001,
			Value2 = 0x0002,
			Value3 = 0x0004
		}

		[Flags]
		public enum MyLongEnum : long
		{
			None,
			Value1 = 0x0001,
			Value2 = 0x0002,
			Value3 = 0x0004
		}

		[Flags]
		public enum MyULongEnum : ulong
		{
			None,
			Value1 = 0x0001,
			Value2 = 0x0002,
			Value3 = 0x0004
		}

		public class MyClassWithEnumMembers
		{
			private MyByteEnum _myByteEnum;
			private MySByteEnum _mySByteEnum;
			private MyShortEnum _myShortEnum;
			private MyUShortEnum _myUShortEnum;
			private MyIntEnum _myIntEnum;
			private MyUIntEnum _myUIntEnum;
			private MyLongEnum _myLongEnum;
			private MyULongEnum _myULongEnum;

			public MyClassWithEnumMembers(int value)
			{
				_myByteEnum = (MyByteEnum) value;
				_mySByteEnum = (MySByteEnum) value;
				_myShortEnum = (MyShortEnum) value;
				_myUShortEnum = (MyUShortEnum) value;
				_myIntEnum = (MyIntEnum) value;
				_myUIntEnum = (MyUIntEnum) value;
				_myLongEnum = (MyLongEnum) value;
				_myULongEnum = (MyULongEnum) value;
			}

			public MyByteEnum MyByteEnum
			{
				get { return _myByteEnum; }
				set { _myByteEnum = value; }
			}

			public MySByteEnum MySByteEnum
			{
				get { return _mySByteEnum; }
				set { _mySByteEnum = value; }
			}

			public MyShortEnum MyShortEnum
			{
				get { return _myShortEnum; }
				set { _myShortEnum = value; }
			}

			public MyUShortEnum MyUShortEnum
			{
				get { return _myUShortEnum; }
				set { _myUShortEnum = value; }
			}

			public MyIntEnum MyIntEnum
			{
				get { return _myIntEnum; }
				set { _myIntEnum = value; }
			}

			public MyUIntEnum MyUIntEnum
			{
				get { return _myUIntEnum; }
				set { _myUIntEnum = value; }
			}

			public MyLongEnum MyLongEnum
			{
				get { return _myLongEnum; }
				set { _myLongEnum = value; }
			}

			public MyULongEnum MyULongEnum
			{
				get { return _myULongEnum; }
				set { _myULongEnum = value; }
			}
		}

		public struct MyStructWithoutEqualityOperators
		{
			private int _value;

			public MyStructWithoutEqualityOperators(int value)
			{
				_value = value;
			}

			public int Value
			{
				get { return _value; }
			}

			public override bool Equals(object obj)
			{
				MyStructWithoutEqualityOperators? other = obj as MyStructWithoutEqualityOperators?;
				if (other == null)
					return false;

				return _value == other.Value._value;
			}

			public override int GetHashCode()
			{
				// Just to avoid compiler warning.
				return base.GetHashCode();
			}
		}

		public interface Interface0
		{
		}

		public interface Interface1
		{
		}

		public interface Interface2
		{
		}

		public class BaseClass : Interface0
		{
		}

		public class Derived1Class : BaseClass, Interface1
		{
		}

		public class Derived2Class : BaseClass, Interface1, Interface2
		{
		}

		#endregion
		
		#region Table Bindings
		
		public class MyIntRow
		{
			public MyInt Value;

			[IndexerName("Multiply")]
			public int this[int factor]
			{
				get { return Value.Value *factor; }
			}

			public override string ToString()
			{
				return Value.Value.ToString();
			}
		}

		public class MyLongRow
		{
			public MyLong Value;
		}

		public class MyIntLongRow
		{
			public MyInt IntValue;
			public MyLong LongValue;
		}

		private static MyIntRow[] ProduceIntRows(int start, int end)
		{
			List<MyIntRow> rows = new List<MyIntRow>(end - start + 1);
			for (int i = start; i <= end; i++)
			{
				MyIntRow row = new MyIntRow();
				row.Value = new MyInt(i);
				rows.Add(row);
			}
			return rows.ToArray();
		}

		private static MyLongRow[] ProduceLongRows(int start, int end)
		{
            List<MyLongRow> rows = new List<MyLongRow>(end - start + 1);
			for (long i = start; i <= end; i++)
			{
				MyLongRow row = new MyLongRow();
				row.Value = new MyLong(i);
				rows.Add(row);
			}
			return rows.ToArray();
		}

		private static MyIntLongRow[] ProduceIntLongRows(int intStart, int intEnd, int longStart, int longEnd)
		{
			int count = (intEnd - intStart + 1)*(longEnd - longStart + 1);
			List<MyIntLongRow> rows = new List<MyIntLongRow>(count);
			for (int intValue = intStart; intValue <= intEnd; intValue++)
			{
				for (int longValue = longStart; longValue <= longEnd; longValue++)
				{
					MyIntLongRow row = new MyIntLongRow();
					row.IntValue = new MyInt(intValue);
					row.LongValue = new MyLong(longValue);
					rows.Add(row);
				}
			}
			return rows.ToArray();
		}

		#endregion
		
		#region Custom Functions
		
		public class Functions
		{
			[FunctionBinding("Calc", IsDeterministic = true)]
			public static string Calc(MyInt p1, MyInt p2)
			{
				return MethodInfo.GetCurrentMethod().ToString();
			}
			
			[FunctionBinding("Calc", IsDeterministic = true)]
			public static string Calc(MyLong p1, MyLong p2)
			{
				return MethodInfo.GetCurrentMethod().ToString();
			}
			
			[FunctionBinding("Calc", IsDeterministic = true)]
			public static string Calc(MyLong p1, MyInt p2)
			{
				return MethodInfo.GetCurrentMethod().ToString();
			}

			[FunctionBinding("Calc", IsDeterministic = true)]
			public static string Calc(MyInt p1, MyLong p2)
			{
				return MethodInfo.GetCurrentMethod().ToString();
			}

			[FunctionBinding("IntCalc", IsDeterministic = true)]
			public static string IntCalc(MyInt p1, MyInt p2)
			{
				return MethodInfo.GetCurrentMethod().ToString();
			}

			[FunctionBinding("LongCalc", IsDeterministic = true)]
			public static string LongCalc(MyLong p1, MyLong p2)
			{
				return MethodInfo.GetCurrentMethod().ToString();
			}
		}
		
		#endregion
		
		[TestInitialize]
		public void SetUp()
		{
			const int INT_START = 0;
			const int INT_END = 55;
			const int LONG_START = 50;
			const int LONG_END = 100;

			MyIntRow[] intRows = ProduceIntRows(INT_START, INT_END);
			MyLongRow[] longRows = ProduceLongRows(LONG_START, LONG_END);
			MyIntLongRow[] intLongRows = ProduceIntLongRows(INT_START, INT_END, LONG_START, LONG_END);
			
			_query = QueryFactory.CreateQuery();
			_query.DataContext.Tables.Add(intRows, "IntTable");
			_query.DataContext.Tables.Add(longRows, "LongTable");
			_query.DataContext.Tables.Add(intLongRows, "IntLongTable");
			_query.DataContext.Functions.AddFromContainer(typeof(Functions));

			_expression = new Expression<object>();
			_expression.DataContext = _query.DataContext;
			_expression.Parameters.Add("MyInt", typeof(MyInt), new MyInt(1));
			_expression.Parameters.Add("MyLong", typeof(MyLong), new MyLong(2));
		}
		
		[TestMethod]
		public void EqualsResolvesIfImplicitAndExplictAreApplicable()
		{
			_query.Text = "SELECT COUNT(*) FROM IntLongTable WHERE IntValue = LongValue";
			int count = (int) _query.ExecuteScalar();
			Assert.AreEqual(6, count);
		}

		[TestMethod]
		public void ImplictIsCalledWhenNothingElseIsAvailable()
		{
			_expression.Text = "IntCalc(1, 2)";
			string result = (string) _expression.Evaluate();
			Assert.AreEqual("System.String IntCalc(MyInt, MyInt)", result);
		}

		[TestMethod]
		public void ExplictIsCalledWhenNothingElseIsAvailable()
		{
			string result;
			
			_expression.Text = "IntCalc(@MyLong, @MyLong)";
			result = (string) _expression.Evaluate();
			Assert.AreEqual("System.String IntCalc(MyInt, MyInt)", result);

			_expression.Text = "IntCalc(@MyInt, @MyLong)";
			result = (string) _expression.Evaluate();
			Assert.AreEqual("System.String IntCalc(MyInt, MyInt)", result);

			_expression.Text = "IntCalc(@MyLong, @MyInt)";
			result = (string) _expression.Evaluate();
			Assert.AreEqual("System.String IntCalc(MyInt, MyInt)", result);
		}

		[TestMethod]
		public void ImplictIsBetterThanExplicit()
		{
			_expression.Text = "Calc(@MyInt, @MyLong)";
			string result = (string) _expression.Evaluate();
			Assert.AreEqual("System.String Calc(MyInt, MyLong)", result);
		}

		[TestMethod]
		public void ImplicitConversionToIntIsBetterThanLong()
		{
			_expression.Text = "Calc(1, 2)";
			string result = (string) _expression.Evaluate();
			Assert.AreEqual("System.String Calc(MyInt, MyInt)", result);
		}
		
		[TestMethod]
		public void SameTypeIsBetterThanAnyConversion()
		{
			string result;
			
			_expression.Text = "Calc(@MyInt, @MyInt)";
			result = (string) _expression.Evaluate();
			Assert.AreEqual("System.String Calc(MyInt, MyInt)", result);

			_expression.Text = "Calc(@MyLong, @MyLong)";
			result = (string) _expression.Evaluate();
			Assert.AreEqual("System.String Calc(MyLong, MyLong)", result);
		}

		[TestMethod]
		public void OperatorsWithMixedTypesWorkAsExpected()
		{
			MyDerivedOperatorRedeclared left = new MyDerivedOperatorRedeclared(13);
			MyBaseOperator right = new MyBaseOperator(11);
			MyBaseOperator result;

			Expression<MyBaseOperator> expression = new Expression<MyBaseOperator>();
			expression.Parameters.Add("Left", left.GetType(), left);
			expression.Parameters.Add("Right", right.GetType(), right);
			
			expression.Text = "Left + Right";
			result = expression.Evaluate();
			Assert.AreEqual(24, result.Value);

			expression.Text = "Left - Right";
			result = expression.Evaluate();
			Assert.AreEqual(2, result.Value);
		}

		[TestMethod]
		public void OperatorsInBaseClassAreConsidered()
		{
			MyDerivedOperator left = new MyDerivedOperator(11);
			MyDerivedOperator right = new MyDerivedOperator(13);
			MyBaseOperator result;

			Expression<MyBaseOperator> expression = new Expression<MyBaseOperator>();
			expression.Parameters.Add("Left", left.GetType(), left);
			expression.Parameters.Add("Right", right.GetType(), right);

			expression.Text = "Left + Right";
			result = expression.Evaluate();
			Assert.AreEqual(24, result.Value);

			expression.Text = "Left - Right";
			result = expression.Evaluate();
			Assert.AreEqual(-2, result.Value);
		}

		[TestMethod]
		public void OperatorsInBaseClassWorkAsExpected()
		{
			MyBaseOperator left = new MyBaseOperator(13);
			MyBaseOperator right = new MyBaseOperator(11);
			MyBaseOperator result;

			Expression<MyBaseOperator> expression = new Expression<MyBaseOperator>();
			expression.Parameters.Add("Left", left.GetType(), left);
			expression.Parameters.Add("Right", right.GetType(), right);

			expression.Text = "Left + Right";
			result = expression.Evaluate();
			Assert.AreEqual(24, result.Value);

			expression.Text = "Left - Right";
			result = expression.Evaluate();
			Assert.AreEqual(2, result.Value);
		}

		[TestMethod]
		public void OperatorsInDerivedClassWorkAsExpected()
		{
			MyDerivedOperatorRedeclared left = new MyDerivedOperatorRedeclared(11);
			MyDerivedOperatorRedeclared right = new MyDerivedOperatorRedeclared(13);
			MyDerivedOperatorRedeclared result;

			Expression<MyDerivedOperatorRedeclared> expression = new Expression<MyDerivedOperatorRedeclared>();
			expression.Parameters.Add("Left", left.GetType(), left);
			expression.Parameters.Add("Right", right.GetType(), right);

			expression.Text = "Left + Right";
			result = expression.Evaluate();
			Assert.AreEqual(24, result.Value);

			expression.Text = "Left - Right";
			result = expression.Evaluate();
			Assert.AreEqual(-2, result.Value);
		}

		[TestMethod]
		public void EnumBitTesting()
		{
			const int None = 0x0;
			const int Value1 = 0x0001;
			const int Value2 = 0x0002;
			const int Value3 = 0x0004;

			List<MyClassWithEnumMembers> myClasses = new List<MyClassWithEnumMembers>();
			myClasses.Add(new MyClassWithEnumMembers(None));
			myClasses.Add(new MyClassWithEnumMembers(Value1));
			myClasses.Add(new MyClassWithEnumMembers(Value2));
			myClasses.Add(new MyClassWithEnumMembers(Value3));
			myClasses.Add(new MyClassWithEnumMembers(Value1 | Value2));
			myClasses.Add(new MyClassWithEnumMembers(Value3 | Value2));
			myClasses.Add(new MyClassWithEnumMembers(Value1 | Value3));
			myClasses.Add(new MyClassWithEnumMembers(Value2 | Value3));

			DataTable result;

			Query query = new Query();
			query.DataContext.Tables.Add(myClasses, "MyClasses");
			query.Parameters.Add("MyByteEnum", typeof (MyByteEnum), MyByteEnum.Value1);
			query.Parameters.Add("MySByteEnum", typeof (MySByteEnum), MySByteEnum.Value1);
			query.Parameters.Add("MyShortEnum", typeof (MyShortEnum), MyShortEnum.Value1);
			query.Parameters.Add("MyUShortEnum", typeof (MyUShortEnum), MyUShortEnum.Value1);
			query.Parameters.Add("MyIntEnum", typeof (MyIntEnum), MyIntEnum.Value1);
			query.Parameters.Add("MyUIntEnum", typeof (MyUIntEnum), MyUIntEnum.Value1);
			query.Parameters.Add("MyLongEnum", typeof (MyLongEnum), MyLongEnum.Value1);
			query.Parameters.Add("MyULongEnum", typeof (MyULongEnum), MyULongEnum.Value1);

			// Check bitwise-AND

			query.Text = @"
				SELECT	c.MyByteEnum,
                        c.MySByteEnum,
                        c.MyShortEnum,
                        c.MyUShortEnum,
                        c.MyIntEnum,
                        c.MyUIntEnum,
                        c.MyLongEnum,
                        c.MyULongEnum
				  FROM  MyClasses c
				 WHERE  (c.MyByteEnum & @MyByteEnum) != 0
				   AND  (c.MySByteEnum & @MySByteEnum) != 0
				   AND  (c.MyShortEnum & @MyShortEnum) != 0
				   AND  (c.MyUShortEnum & @MyUShortEnum) != 0
				   AND  (c.MyIntEnum & @MyIntEnum) != 0
				   AND  (c.MyUIntEnum & @MyUIntEnum) != 0
				   AND  (c.MyLongEnum & @MyLongEnum) != 0
				   AND  (c.MyULongEnum & @MyULongEnum) != 0
			";

			result = query.ExecuteDataTable();
			Assert.AreEqual(3, result.Rows.Count);
			Assert.AreEqual(MyByteEnum.Value1, (MyByteEnum) result.Rows[0][0]);
			Assert.AreEqual(MySByteEnum.Value1, (MySByteEnum)result.Rows[0][1]);
			Assert.AreEqual(MyShortEnum.Value1, (MyShortEnum)result.Rows[0][2]);
			Assert.AreEqual(MyUShortEnum.Value1, (MyUShortEnum)result.Rows[0][3]);
			Assert.AreEqual(MyIntEnum.Value1, (MyIntEnum)result.Rows[0][4]);
			Assert.AreEqual(MyUIntEnum.Value1, (MyUIntEnum)result.Rows[0][5]);
			Assert.AreEqual(MyLongEnum.Value1, (MyLongEnum)result.Rows[0][6]);
			Assert.AreEqual(MyULongEnum.Value1, (MyULongEnum)result.Rows[0][7]);

			Assert.AreEqual(MyByteEnum.Value1 | MyByteEnum.Value2, (MyByteEnum)result.Rows[1][0]);
			Assert.AreEqual(MySByteEnum.Value1 | MySByteEnum.Value2, (MySByteEnum)result.Rows[1][1]);
			Assert.AreEqual(MyShortEnum.Value1 | MyShortEnum.Value2, (MyShortEnum)result.Rows[1][2]);
			Assert.AreEqual(MyUShortEnum.Value1 | MyUShortEnum.Value2, (MyUShortEnum)result.Rows[1][3]);
			Assert.AreEqual(MyIntEnum.Value1 | MyIntEnum.Value2, (MyIntEnum)result.Rows[1][4]);
			Assert.AreEqual(MyUIntEnum.Value1 | MyUIntEnum.Value2, (MyUIntEnum)result.Rows[1][5]);
			Assert.AreEqual(MyLongEnum.Value1 | MyLongEnum.Value2, (MyLongEnum)result.Rows[1][6]);
			Assert.AreEqual(MyULongEnum.Value1 | MyULongEnum.Value2, (MyULongEnum)result.Rows[1][7]);

			Assert.AreEqual(MyByteEnum.Value1 | MyByteEnum.Value3, (MyByteEnum)result.Rows[2][0]);
			Assert.AreEqual(MySByteEnum.Value1 | MySByteEnum.Value3, (MySByteEnum)result.Rows[2][1]);
			Assert.AreEqual(MyShortEnum.Value1 | MyShortEnum.Value3, (MyShortEnum)result.Rows[2][2]);
			Assert.AreEqual(MyUShortEnum.Value1 | MyUShortEnum.Value3, (MyUShortEnum)result.Rows[2][3]);
			Assert.AreEqual(MyIntEnum.Value1 | MyIntEnum.Value3, (MyIntEnum)result.Rows[2][4]);
			Assert.AreEqual(MyUIntEnum.Value1 | MyUIntEnum.Value3, (MyUIntEnum)result.Rows[2][5]);
			Assert.AreEqual(MyLongEnum.Value1 | MyLongEnum.Value3, (MyLongEnum)result.Rows[2][6]);
			Assert.AreEqual(MyULongEnum.Value1 | MyULongEnum.Value3, (MyULongEnum)result.Rows[2][7]);

			// Check bitwise-OR

			query.Text = @"
				SELECT	c.MyByteEnum | @MyByteEnum,
                        c.MySByteEnum | @MySByteEnum,
                        c.MyShortEnum | @MyShortEnum,
                        c.MyUShortEnum | @MyUShortEnum,
                        c.MyIntEnum | @MyIntEnum,
                        c.MyUIntEnum | @MyUIntEnum,
                        c.MyLongEnum | @MyLongEnum,
                        c.MyULongEnum | @MyULongEnum
                  FROM  MyClasses c
			";
			result = query.ExecuteDataTable();
			Assert.AreEqual(8, result.Rows.Count);

			Assert.AreEqual(MyByteEnum.Value1, (MyByteEnum) result.Rows[0][0]);
			Assert.AreEqual(MyByteEnum.Value1, (MyByteEnum)result.Rows[1][0]);
			Assert.AreEqual(MyByteEnum.Value2 | MyByteEnum.Value1, (MyByteEnum)result.Rows[2][0]);
			Assert.AreEqual(MyByteEnum.Value3 | MyByteEnum.Value1, (MyByteEnum)result.Rows[3][0]);
			Assert.AreEqual(MyByteEnum.Value1 | MyByteEnum.Value2, (MyByteEnum)result.Rows[4][0]);
			Assert.AreEqual(MyByteEnum.Value3 | MyByteEnum.Value2 | MyByteEnum.Value1, (MyByteEnum)result.Rows[5][0]);
			Assert.AreEqual(MyByteEnum.Value1 | MyByteEnum.Value3, (MyByteEnum)result.Rows[6][0]);
			Assert.AreEqual(MyByteEnum.Value2 | MyByteEnum.Value3 | MyByteEnum.Value1, (MyByteEnum)result.Rows[7][0]);

			Assert.AreEqual(MySByteEnum.Value1, (MySByteEnum) result.Rows[0][1]);
			Assert.AreEqual(MySByteEnum.Value1, (MySByteEnum)result.Rows[1][1]);
			Assert.AreEqual(MySByteEnum.Value2 | MySByteEnum.Value1, (MySByteEnum)result.Rows[2][1]);
			Assert.AreEqual(MySByteEnum.Value3 | MySByteEnum.Value1, (MySByteEnum)result.Rows[3][1]);
			Assert.AreEqual(MySByteEnum.Value1 | MySByteEnum.Value2, (MySByteEnum)result.Rows[4][1]);
			Assert.AreEqual(MySByteEnum.Value3 | MySByteEnum.Value2 | MySByteEnum.Value1, (MySByteEnum)result.Rows[5][1]);
			Assert.AreEqual(MySByteEnum.Value1 | MySByteEnum.Value3, (MySByteEnum)result.Rows[6][1]);
			Assert.AreEqual(MySByteEnum.Value2 | MySByteEnum.Value3 | MySByteEnum.Value1, (MySByteEnum)result.Rows[7][1]);

			Assert.AreEqual(MyShortEnum.Value1, (MyShortEnum)result.Rows[0][2]);
			Assert.AreEqual(MyShortEnum.Value1, (MyShortEnum)result.Rows[1][2]);
			Assert.AreEqual(MyShortEnum.Value2 | MyShortEnum.Value1, (MyShortEnum)result.Rows[2][2]);
			Assert.AreEqual(MyShortEnum.Value3 | MyShortEnum.Value1, (MyShortEnum)result.Rows[3][2]);
			Assert.AreEqual(MyShortEnum.Value1 | MyShortEnum.Value2, (MyShortEnum)result.Rows[4][2]);
			Assert.AreEqual(MyShortEnum.Value3 | MyShortEnum.Value2 | MyShortEnum.Value1, (MyShortEnum)result.Rows[5][2]);
			Assert.AreEqual(MyShortEnum.Value1 | MyShortEnum.Value3, (MyShortEnum)result.Rows[6][2]);
			Assert.AreEqual(MyShortEnum.Value2 | MyShortEnum.Value3 | MyShortEnum.Value1, (MyShortEnum)result.Rows[7][2]);

			Assert.AreEqual(MyUShortEnum.Value1, (MyUShortEnum)result.Rows[0][3]);
			Assert.AreEqual(MyUShortEnum.Value1, (MyUShortEnum)result.Rows[1][3]);
			Assert.AreEqual(MyUShortEnum.Value2 | MyUShortEnum.Value1, (MyUShortEnum)result.Rows[2][3]);
			Assert.AreEqual(MyUShortEnum.Value3 | MyUShortEnum.Value1, (MyUShortEnum)result.Rows[3][3]);
			Assert.AreEqual(MyUShortEnum.Value1 | MyUShortEnum.Value2, (MyUShortEnum)result.Rows[4][3]);
			Assert.AreEqual(MyUShortEnum.Value3 | MyUShortEnum.Value2 | MyUShortEnum.Value1, (MyUShortEnum)result.Rows[5][3]);
			Assert.AreEqual(MyUShortEnum.Value1 | MyUShortEnum.Value3, (MyUShortEnum)result.Rows[6][3]);
			Assert.AreEqual(MyUShortEnum.Value2 | MyUShortEnum.Value3 | MyUShortEnum.Value1, (MyUShortEnum)result.Rows[7][3]);

			Assert.AreEqual(MyIntEnum.Value1, (MyIntEnum)result.Rows[0][4]);
			Assert.AreEqual(MyIntEnum.Value1, (MyIntEnum)result.Rows[1][4]);
			Assert.AreEqual(MyIntEnum.Value2 | MyIntEnum.Value1, (MyIntEnum)result.Rows[2][4]);
			Assert.AreEqual(MyIntEnum.Value3 | MyIntEnum.Value1, (MyIntEnum)result.Rows[3][4]);
			Assert.AreEqual(MyIntEnum.Value1 | MyIntEnum.Value2, (MyIntEnum)result.Rows[4][4]);
			Assert.AreEqual(MyIntEnum.Value3 | MyIntEnum.Value2 | MyIntEnum.Value1, (MyIntEnum)result.Rows[5][4]);
			Assert.AreEqual(MyIntEnum.Value1 | MyIntEnum.Value3, (MyIntEnum)result.Rows[6][4]);
			Assert.AreEqual(MyIntEnum.Value2 | MyIntEnum.Value3 | MyIntEnum.Value1, (MyIntEnum)result.Rows[7][4]);

			Assert.AreEqual(MyUIntEnum.Value1, (MyUIntEnum)result.Rows[0][5]);
			Assert.AreEqual(MyUIntEnum.Value1, (MyUIntEnum)result.Rows[1][5]);
			Assert.AreEqual(MyUIntEnum.Value2 | MyUIntEnum.Value1, (MyUIntEnum)result.Rows[2][5]);
			Assert.AreEqual(MyUIntEnum.Value3 | MyUIntEnum.Value1, (MyUIntEnum)result.Rows[3][5]);
			Assert.AreEqual(MyUIntEnum.Value1 | MyUIntEnum.Value2, (MyUIntEnum)result.Rows[4][5]);
			Assert.AreEqual(MyUIntEnum.Value3 | MyUIntEnum.Value2 | MyUIntEnum.Value1, (MyUIntEnum)result.Rows[5][5]);
			Assert.AreEqual(MyUIntEnum.Value1 | MyUIntEnum.Value3, (MyUIntEnum)result.Rows[6][5]);
			Assert.AreEqual(MyUIntEnum.Value2 | MyUIntEnum.Value3 | MyUIntEnum.Value1, (MyUIntEnum)result.Rows[7][5]);

			Assert.AreEqual(MyLongEnum.Value1, (MyLongEnum)result.Rows[0][6]);
			Assert.AreEqual(MyLongEnum.Value1, (MyLongEnum)result.Rows[1][6]);
			Assert.AreEqual(MyLongEnum.Value2 | MyLongEnum.Value1, (MyLongEnum)result.Rows[2][6]);
			Assert.AreEqual(MyLongEnum.Value3 | MyLongEnum.Value1, (MyLongEnum)result.Rows[3][6]);
			Assert.AreEqual(MyLongEnum.Value1 | MyLongEnum.Value2, (MyLongEnum)result.Rows[4][6]);
			Assert.AreEqual(MyLongEnum.Value3 | MyLongEnum.Value2 | MyLongEnum.Value1, (MyLongEnum)result.Rows[5][6]);
			Assert.AreEqual(MyLongEnum.Value1 | MyLongEnum.Value3, (MyLongEnum)result.Rows[6][6]);
			Assert.AreEqual(MyLongEnum.Value2 | MyLongEnum.Value3 | MyLongEnum.Value1, (MyLongEnum)result.Rows[7][6]);

			Assert.AreEqual(MyULongEnum.Value1, (MyULongEnum)result.Rows[0][7]);
			Assert.AreEqual(MyULongEnum.Value1, (MyULongEnum)result.Rows[1][7]);
			Assert.AreEqual(MyULongEnum.Value2 | MyULongEnum.Value1, (MyULongEnum)result.Rows[2][7]);
			Assert.AreEqual(MyULongEnum.Value3 | MyULongEnum.Value1, (MyULongEnum)result.Rows[3][7]);
			Assert.AreEqual(MyULongEnum.Value1 | MyULongEnum.Value2, (MyULongEnum)result.Rows[4][7]);
			Assert.AreEqual(MyULongEnum.Value3 | MyULongEnum.Value2 | MyULongEnum.Value1, (MyULongEnum)result.Rows[5][7]);
			Assert.AreEqual(MyULongEnum.Value1 | MyULongEnum.Value3, (MyULongEnum)result.Rows[6][7]);
			Assert.AreEqual(MyULongEnum.Value2 | MyULongEnum.Value3 | MyULongEnum.Value1, (MyULongEnum)result.Rows[7][7]);
		}

		[TestMethod]
		public void EnumBitTestingBetweenDifferentEnums()
		{
			Expression<bool> expression = new Expression<bool>();
			expression.Parameters.Add("Left", typeof (MyIntEnum), MyIntEnum.Value1);
			expression.Parameters.Add("Right", typeof(MyUIntEnum), MyUIntEnum.Value1);
			
			try
			{
				expression.Text = "(Left | Right) != 0";
				expression.Evaluate();
				Assert.Fail("Bitwise-OR between different enum types should fail.");
			}
			catch (CompilationException ex)
			{
				Assert.AreEqual(1, ex.CompilationErrors.Count);
				Assert.AreEqual(ErrorId.CannotApplyBinaryOperator, ex.CompilationErrors[0].Id);
			}

			try
			{
				expression.Text = "(Left & Right) != 0";
				expression.Evaluate();
				Assert.Fail("Bitwise-AND between different enum types should fail.");
			}
			catch (CompilationException ex)
			{
				Assert.AreEqual(1, ex.CompilationErrors.Count);
				Assert.AreEqual(ErrorId.CannotApplyBinaryOperator, ex.CompilationErrors[0].Id);
			}
		}

		[TestMethod]
		public void EqualityCheckBetweenAbstractObjects()
		{
			object left = Environment.UserName;
			object right = Environment.UserName;
			bool result;

			Expression<bool> expression = new Expression<bool>();
			expression.Parameters.Add("Left", typeof(object), left);
			expression.Parameters.Add("Right", typeof(object), right);

			expression.Text = "Left = Right";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "Left <> Right";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);
		}

		[TestMethod]
		public void EqualityCheckBetweenAbstractEnums()
		{
			Enum left = MyIntEnum.Value1;
			Enum right = MyIntEnum.Value1;
			bool result;

			Expression<bool> expression = new Expression<bool>();
			expression.Parameters.Add("Left", typeof(Enum), left);
			expression.Parameters.Add("Right", typeof(Enum), right);

			expression.Text = "Left = Right";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "Left <> Right";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);
		}

		[TestMethod]
		public void EqualityCheckBetweenConcreteEnums()
		{
			MyIntEnum left = MyIntEnum.Value1;
			MyIntEnum right = MyIntEnum.Value1;
			bool result;

			Expression<bool> expression = new Expression<bool>();
			expression.Parameters.Add("Left", typeof(MyIntEnum), left);
			expression.Parameters.Add("Right", typeof(MyIntEnum), right);

			expression.Text = "Left = Right";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "Left <> Right";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);
		}
	
		[TestMethod]
		public void EqualityCheckBetweenAbstractStructs()
		{
			ValueType left = 42;
			ValueType right = 42;
			bool result;

			Expression<bool> expression = new Expression<bool>();
			expression.Parameters.Add("Left", typeof(ValueType), left);
			expression.Parameters.Add("Right", typeof(ValueType), right);

			expression.Text = "Left = Right";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "Left <> Right";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);
		}
	
		[TestMethod]
		public void EqualityCheckBetweenConcreteStructs()
		{
			MyStructWithoutEqualityOperators left = new MyStructWithoutEqualityOperators(42);
			MyStructWithoutEqualityOperators right = new MyStructWithoutEqualityOperators(42);
			bool result;

			Expression<bool> expression = new Expression<bool>();
			expression.Parameters.Add("Left", typeof(MyStructWithoutEqualityOperators), left);
			expression.Parameters.Add("Right", typeof(MyStructWithoutEqualityOperators), right);

			expression.Text = "Left = Right";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "Left <> Right";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);
		}

		[TestMethod]
		public void EqualityCheckBetweenCompatibleTypes()
		{
			BaseClass baseClass = new BaseClass();
			Derived1Class derived1Class = new Derived1Class();
			bool result;

			Expression<bool> expression = new Expression<bool>();
			expression.Parameters.Add("baseClass", typeof(BaseClass), baseClass);
			expression.Parameters.Add("derived1Class", typeof(Derived1Class), derived1Class);
			expression.Parameters.Add("derived1ClassTypedAsBase", typeof(BaseClass), derived1Class);

			expression.Text = "baseClass = derived1Class";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived1Class = baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived1Class = derived1ClassTypedAsBase";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived1ClassTypedAsBase = derived1Class";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "baseClass <> derived1Class";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived1Class <> baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived1Class <> derived1ClassTypedAsBase";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived1ClassTypedAsBase <> derived1Class";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);
		}

		[TestMethod]
		public void EqualityCheckBetweenCompatibleInterfaceTypes()
		{
			BaseClass baseClass = new BaseClass();
			Derived1Class derived1Class = new Derived1Class();
			Derived2Class derived2Class = new Derived2Class();
			bool result;

			Expression<bool> expression = new Expression<bool>();
			expression.Parameters.Add("baseClass", typeof(BaseClass), baseClass);
			expression.Parameters.Add("derived1Class", typeof(Derived1Class), derived1Class);
			expression.Parameters.Add("derived2Class", typeof(Derived2Class), derived2Class);
			expression.Parameters.Add("baseClassInterface0", typeof(Interface0), baseClass);
			expression.Parameters.Add("derived1ClassInterface0", typeof(Interface0), derived1Class);
			expression.Parameters.Add("derived1ClassInterface1", typeof(Interface1), derived1Class);
			expression.Parameters.Add("derived2ClassInterface0", typeof(Interface0), derived2Class);
			expression.Parameters.Add("derived2ClassInterface1", typeof(Interface1), derived2Class);
			expression.Parameters.Add("derived2ClassInterface2", typeof(Interface2), derived2Class);

			// Check Interface0 against baseClass, derived1Class, and derived2Class.

			expression.Text = "baseClass = baseClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "baseClassInterface0 = baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived1Class = derived1ClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived1ClassInterface0 = derived1Class";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived2Class = derived2ClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived2ClassInterface0 = derived2Class";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "baseClass = derived1ClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived1ClassInterface0 = baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "baseClass = derived2ClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived2ClassInterface0 = baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived1ClassInterface0 = derived2ClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived2ClassInterface0 = derived1ClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			// Check Interface1 against derived1Class and derived2Class.

			expression.Text = "derived1ClassInterface1 = derived1Class";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived1Class = derived1ClassInterface1";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived2ClassInterface1 = derived2Class";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived2Class = derived2ClassInterface1";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived2Class = derived1ClassInterface1";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived1ClassInterface1 = derived2Class";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived1Class = derived2ClassInterface1";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived2ClassInterface1 = derived1Class";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			// Check Interface2 against derived2Class

			expression.Text = "derived2ClassInterface2 = derived2Class";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived2Class = derived2ClassInterface2";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			//-------------------------------
			// Now check with <> version.
			//-------------------------------

			// Check Interface0 against baseClass, derived1Class, and derived2Class.

			expression.Text = "baseClass <> baseClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "baseClassInterface0 <> baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived1Class <> derived1ClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived1ClassInterface0 <> derived1Class";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived2Class <> derived2ClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived2ClassInterface0 <> derived2Class";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "baseClass <> derived1ClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived1ClassInterface0 <> baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "baseClass <> derived2ClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived2ClassInterface0 <> baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived1ClassInterface0 <> derived2ClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived2ClassInterface0 <> derived1ClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			// Check Interface1 against derived1Class and derived2Class.

			expression.Text = "derived1ClassInterface1 <> derived1Class";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived1Class <> derived1ClassInterface1";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived2ClassInterface1 <> derived2Class";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived2Class <> derived2ClassInterface1";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived2Class <> derived1ClassInterface1";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived1ClassInterface1 <> derived2Class";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived1Class <> derived2ClassInterface1";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived2ClassInterface1 <> derived1Class";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			// Check Interface2 against derived2Class

			expression.Text = "derived2ClassInterface2 <> derived2Class";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived2Class <> derived2ClassInterface2";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);
		}

		[TestMethod]
		public void EqualityCheckBetweenArbitraryInterfaceTypes()
		{
			BaseClass baseClass = new BaseClass();
			Derived1Class derived1Class = new Derived1Class();
			Derived2Class derived2Class = new Derived2Class();
			bool result;

			Expression<bool> expression = new Expression<bool>();
			expression.Parameters.Add("baseClass", typeof(BaseClass), baseClass);
			expression.Parameters.Add("baseClassInterface0", typeof(Interface0), baseClass);
			expression.Parameters.Add("derived1ClassInterface0", typeof(Interface0), derived1Class);
			expression.Parameters.Add("derived1ClassInterface1", typeof(Interface1), derived1Class);
			expression.Parameters.Add("derived2ClassInterface0", typeof(Interface0), derived2Class);
			expression.Parameters.Add("derived2ClassInterface1", typeof(Interface1), derived2Class);
			expression.Parameters.Add("derived2ClassInterface2", typeof(Interface2), derived2Class);

			expression.Text = "baseClass = baseClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "baseClassInterface0 = baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "baseClass = derived1ClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived1ClassInterface0 = baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "baseClass = derived1ClassInterface1";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived1ClassInterface1 = baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "baseClass = derived2ClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived2ClassInterface0 = baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "baseClass = derived2ClassInterface1";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived2ClassInterface1 = baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "baseClass = derived2ClassInterface2";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "derived2ClassInterface2 = baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			//-------------------------------
			// Now check with <> version.
			//-------------------------------

			expression.Text = "baseClass <> baseClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "baseClassInterface0 <> baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(false, result);

			expression.Text = "baseClass <> derived1ClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived1ClassInterface0 <> baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "baseClass <> derived1ClassInterface1";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived1ClassInterface1 <> baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "baseClass <> derived2ClassInterface0";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived2ClassInterface0 <> baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "baseClass <> derived2ClassInterface1";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived2ClassInterface1 <> baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "baseClass <> derived2ClassInterface2";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);

			expression.Text = "derived2ClassInterface2 <> baseClass";
			result = expression.Evaluate();
			Assert.AreEqual(true, result);
		}

		[TestMethod]
		public void EqualityCheckBetweenIncompatibleTypesFails()
		{
			Derived1Class derived1Class = new Derived1Class();
			Derived2Class derived2Class = new Derived2Class();

			Expression<bool> expression = new Expression<bool>();
			expression.Parameters.Add("derived1Class", typeof(Derived1Class), derived1Class);
			expression.Parameters.Add("derived2Class", typeof(Derived2Class), derived2Class);

			try
			{
				expression.Text = "derived1Class = derived2Class";
				expression.Evaluate();
				Assert.Fail("Equality check between incompatible types should fail.");
			}
			catch (CompilationException ex)
			{
				Assert.AreEqual(1, ex.CompilationErrors.Count);
				Assert.AreEqual(ErrorId.CannotApplyBinaryOperator, ex.CompilationErrors[0].Id);
			}

			try
			{
				expression.Text = "derived1Class <> derived2Class";
				expression.Evaluate();
				Assert.Fail("Inequality check between incompatible types should fail.");
			}
			catch (CompilationException ex)
			{
				Assert.AreEqual(1, ex.CompilationErrors.Count);
				Assert.AreEqual(ErrorId.CannotApplyBinaryOperator, ex.CompilationErrors[0].Id);
			}
		}
	}
}
