using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	[TestClass]
	public class RegressionTests : AutomatedTestFixtureBase
	{
		[TestMethod]
		public void WorkItem7288()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem7290()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem7293()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem7293_1()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem7293_2()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem7293_3()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem7294()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem7296()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem7297()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem7301()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem7302()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem7303_1()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem7303_2()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem7303_3()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem7303_4()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem7305()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem7325_1()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem7325_2()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem8980()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem8982()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem9293()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem9294()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem9417()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem11721()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem13555()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void WorkItem14823()
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("ID", typeof(int));
			dataTable.Columns.Add("Name", typeof(string));
			dataTable.Rows.Add(1, "Name1");
			dataTable.Rows.Add(2, "Name2");
			dataTable.Rows.Add(3, "Name3");
			dataTable.AcceptChanges();
			dataTable.Rows[1].Delete();
			dataTable.TableName = "Test";

			Query query = new Query();
			query.Text = "SELECT * FROM Test";
			query.DataContext.Tables.Add(dataTable);

			DataTable resultDataTable = query.ExecuteDataTable();
			Assert.AreEqual(resultDataTable.Rows.Count, 2);
			Assert.AreEqual(resultDataTable.Rows[0][0], 1);
			Assert.AreEqual(resultDataTable.Rows[0][1], "Name1");
			Assert.AreEqual(resultDataTable.Rows[1][0], 3);
			Assert.AreEqual(resultDataTable.Rows[1][1], "Name3");
		}

		[TestMethod]
		public void TestSampleQuery()
		{
			// Get a lists with all types and all members

			List<Type> typeList = new List<Type>();
			List<MemberInfo> memberList = new List<MemberInfo>();
			foreach (Assembly assembly in new Assembly[] { typeof(int).Assembly, typeof(Uri).Assembly, typeof(XmlDocument).Assembly })
			{
				Type[] types = assembly.GetTypes();
				typeList.AddRange(types);

				foreach (Type t in types)
					memberList.AddRange(t.GetMembers());
			}

			// Create data context containing the tables "Types" and "Members"

			DataContext dataContext = new DataContext();
			dataContext.Tables.Add(typeList.ToArray(), "Types");
			dataContext.Tables.Add(memberList.ToArray(), "Members");

			Query query = new Query(dataContext);
			query.Text = @"
                SELECT  t.Assembly.ManifestModule.Name AS AssemblyName,
                        m.MemberType.ToString() AS MemberType,
                        COUNT(*) AS MemberCount
                FROM    Types t
                            INNER JOIN Members m ON m.DeclaringType.AssemblyQualifiedName = t.AssemblyQualifiedName
                GROUP   BY t.Assembly.ManifestModule.Name, m.MemberType
            ";

			query.ExecuteDataTable();
		}
		
		[TestMethod]
		public void TestSampleExpression()
		{
			// Get a lists with all types
			List<Type> typeList = new List<Type>();
			foreach (Assembly assembly in new Assembly[] { typeof(int).Assembly, typeof(Uri).Assembly, typeof(XmlDocument).Assembly })
				typeList.AddRange(assembly.GetTypes());

			// Create an empty data context
			DataContext dataContext = new DataContext();

			// Create expression to filter types
			Expression<bool> filterExpr = new Expression<bool>();
			filterExpr.DataContext = dataContext;
			filterExpr.Text = "(@Type.IsAbstract OR NOT @Type.IsClass) AND @Type.GetMembers().Length > 50";
			filterExpr.Parameters.Add("@Type", typeof(Type));

			// Create expression to create a value from a type
			Expression<string> valueExpr = new Expression<string>();
			valueExpr.DataContext = dataContext;
			valueExpr.Text = "@Type.FullName + COALESCE(' : ' + @Type.BaseType.FullName, '')";
			valueExpr.Parameters.Add("@Type", typeof(Type));

			List<string> actualTypeStrings = new List<string>();
			foreach (Type type in typeList)
			{
				filterExpr.Parameters["@Type"].Value = type;
				valueExpr.Parameters["@Type"].Value = type;

				if (filterExpr.Evaluate())
					actualTypeStrings.Add(valueExpr.Evaluate());
			}

			List<string> expectedTypeStrings = new List<string>();
			expectedTypeStrings.Add("System.Array : System.Object");
			expectedTypeStrings.Add("System.ExceptionResource : System.Enum");
			expectedTypeStrings.Add("System.DateTime : System.ValueType");
			expectedTypeStrings.Add("System._AppDomain");
			expectedTypeStrings.Add("System.Char : System.ValueType");
			expectedTypeStrings.Add("System.Console : System.Object");
			expectedTypeStrings.Add("System.ConsoleKey : System.Enum");
			expectedTypeStrings.Add("System.Convert : System.Object");
			expectedTypeStrings.Add("System.Decimal : System.ValueType");
			expectedTypeStrings.Add("System.Environment : System.Object");
			expectedTypeStrings.Add("System.Math : System.Object");
			expectedTypeStrings.Add("System.Runtime.InteropServices._Type");
			expectedTypeStrings.Add("System.Type : System.Reflection.MemberInfo");
			expectedTypeStrings.Add("System.TimeSpan : System.ValueType");
			expectedTypeStrings.Add("System.Runtime.InteropServices._Assembly");
			expectedTypeStrings.Add("System.Runtime.InteropServices._MethodBase");
			expectedTypeStrings.Add("System.Runtime.InteropServices._MethodInfo");
			expectedTypeStrings.Add("System.Runtime.InteropServices._ConstructorInfo");
			expectedTypeStrings.Add("System.Runtime.InteropServices._FieldInfo");
			expectedTypeStrings.Add("System.Reflection.MetadataTable : System.Enum");
			expectedTypeStrings.Add("System.Reflection.MetadataColumnType : System.Enum");
			expectedTypeStrings.Add("System.Reflection.MetadataColumn : System.Enum");
			expectedTypeStrings.Add("System.Reflection.MethodBase : System.Reflection.MemberInfo");
			expectedTypeStrings.Add("System.Reflection.ConstructorInfo : System.Reflection.MethodBase");
			expectedTypeStrings.Add("System.Reflection.MethodInfo : System.Reflection.MethodBase");
			expectedTypeStrings.Add("System.Reflection.FieldInfo : System.Reflection.MemberInfo");
			expectedTypeStrings.Add("System.Reflection.RuntimeFieldInfo : System.Reflection.FieldInfo");
			expectedTypeStrings.Add("System.Globalization.Calendar : System.Object");
			expectedTypeStrings.Add("System.Globalization.EastAsianLunisolarCalendar : System.Globalization.Calendar");
			expectedTypeStrings.Add("System.Text.Encoding : System.Object");
			expectedTypeStrings.Add("System.Text.EncodingNLS : System.Text.Encoding");
			expectedTypeStrings.Add("System.Text.BaseCodePageEncoding : System.Text.EncodingNLS");
			expectedTypeStrings.Add("System.Runtime.InteropServices.VarEnum : System.Enum");
			expectedTypeStrings.Add("System.Runtime.InteropServices.Marshal : System.Object");
			expectedTypeStrings.Add("System.IO.File : System.Object");
			expectedTypeStrings.Add("System.IO.TextWriter : System.MarshalByRefObject");
			expectedTypeStrings.Add("System.Security.Principal.WellKnownSidType : System.Enum");
			expectedTypeStrings.Add("Microsoft.Win32.UnsafeNativeMethods : System.Object");
			expectedTypeStrings.Add("Microsoft.Win32.NativeMethods : System.Object");
			expectedTypeStrings.Add("System.Uri+Flags : System.Enum");
			expectedTypeStrings.Add("System.Net.WebRequest : System.MarshalByRefObject");
			expectedTypeStrings.Add("System.Net.HttpRequestHeader : System.Enum");
			expectedTypeStrings.Add("System.Net.HttpStatusCode : System.Enum");
			expectedTypeStrings.Add("System.Net.HttpKnownHeaderNames : System.Object");
			expectedTypeStrings.Add("System.Net.Security.AuthenticatedStream : System.IO.Stream");
			expectedTypeStrings.Add("System.Net.Sockets.SocketError : System.Enum");
			expectedTypeStrings.Add("System.Net.Sockets.SocketOptionName : System.Enum");
			expectedTypeStrings.Add("System.Net.NetworkInformation.IcmpV4Statistics : System.Object");
			expectedTypeStrings.Add("System.Net.NetworkInformation.IcmpV6Statistics : System.Object");
			expectedTypeStrings.Add("System.Xml.XmlWriter : System.Object");
			expectedTypeStrings.Add("System.Xml.XmlRawWriter : System.Xml.XmlWriter");
			expectedTypeStrings.Add("System.Xml.XmlReader : System.Object");
			expectedTypeStrings.Add("System.Xml.XPath.XPathNavigator : System.Xml.XPath.XPathItem");
			expectedTypeStrings.Add("System.Xml.XmlNode : System.Object");
			expectedTypeStrings.Add("System.Xml.XmlLinkedNode : System.Xml.XmlNode");
			expectedTypeStrings.Add("System.Xml.XmlCharacterData : System.Xml.XmlLinkedNode");
			expectedTypeStrings.Add("System.Xml.BinXmlToken : System.Enum");
			expectedTypeStrings.Add("MS.Internal.Xml.Cache.XPathNode : System.ValueType");
			expectedTypeStrings.Add("System.Xml.DtdParser+Token : System.Enum");
			expectedTypeStrings.Add("System.Xml.Schema.SchemaNames+Token : System.Enum");
			expectedTypeStrings.Add("System.Xml.Schema.XmlTypeCode : System.Enum");
			expectedTypeStrings.Add("System.Xml.Schema.XmlValueConverter : System.Object");
			expectedTypeStrings.Add("System.Xml.Schema.XmlBaseConverter : System.Xml.Schema.XmlValueConverter");
			expectedTypeStrings.Add("System.Xml.Schema.XsdBuilder+State : System.Enum");
			expectedTypeStrings.Add("System.DateTimeOffset : System.ValueType");

			expectedTypeStrings.Sort();
			actualTypeStrings.Sort();
			Assert.AreEqual(expectedTypeStrings.Count, actualTypeStrings.Count);
			for (int i = 0; i < expectedTypeStrings.Count; i++)
				Assert.AreEqual(expectedTypeStrings[i], actualTypeStrings[i]);
		}
	}
}
