using System;
using System.Collections.Generic;
using System.Windows.Forms;

using NQuery.Runtime;

namespace NQuery.Samples.CustomPropertyProvider
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void runMyReflectionProviderButton_Click(object sender, EventArgs e)
		{
			#region MyReflectionProvider Usage

			Query query = new Query();

			List<MyDataType1> myDataType2Values = new List<MyDataType1>();
			myDataType2Values.Add(new MyDataType1(1, "One", DateTime.Now));
			myDataType2Values.Add(new MyDataType1(2, "Two", DateTime.Now.AddDays(1)));
			myDataType2Values.Add(new MyDataType1(3, "Three", DateTime.Now.AddDays(2)));

			query.DataContext.MetadataContext.PropertyProviders.Register(typeof(MyDataType1), new MyReflectionProvider());
			query.DataContext.Tables.Add(myDataType2Values, "MyDataType1Values");
			query.Text = "SELECT * FROM MyDataType1Values";
			dataGridView1.DataSource = query.ExecuteDataTable();
			
			#endregion
		}

		private void runMyPropertyProviderButton_Click(object sender, EventArgs e)
		{
			#region MyPropertyProvider Usage

			Query query = new Query();

			List<MyDataType2> myDataType2Values = new List<MyDataType2>();
			myDataType2Values.Add(new MyDataType2(1, "One", DateTime.Now));
			myDataType2Values.Add(new MyDataType2(2, "Two", DateTime.Now.AddDays(1)));
			myDataType2Values.Add(new MyDataType2(3, "Three", DateTime.Now.AddDays(2)));

			query.DataContext.MetadataContext.PropertyProviders.Register(typeof(MyDataType2), new MyPropertyProvider());
			query.DataContext.Tables.Add(myDataType2Values, "MyDataType2Values");
			query.Text = "SELECT * FROM MyDataType2Values";
			dataGridView1.DataSource = query.ExecuteDataTable();

			#endregion
		}

		private void runParameterWithCustomPropertiesButton_Click(object sender, EventArgs e)
		{
			#region Parameter with Custom Properties

			Query query = new Query();

			Dictionary<string, object> myDictionary = new Dictionary<string, object>();
			myDictionary["DateTimeProp"] = DateTime.Now;
			myDictionary["IntProp"] = 42;
			myDictionary["StringProp"] = Environment.UserName;

			query.Parameters.Add("@Param", typeof (Dictionary<string, object>), myDictionary, DictionaryPropertyProvider.GetProperties(myDictionary));
			query.Text = "SELECT @Param.IntProp, @Param.StringProp, @Param.DateTimeProp";
			dataGridView1.DataSource = query.ExecuteDataTable();

			#endregion
		}
	}
}