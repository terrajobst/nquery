using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NQuery.Samples.CustomMethodProvider
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

			List<MyDataType> myDataType2Values = new List<MyDataType>();
			myDataType2Values.Add(new MyDataType(1, "One"));
			myDataType2Values.Add(new MyDataType(2, "Two"));
			myDataType2Values.Add(new MyDataType(3, "Three"));

			query.DataContext.MetadataContext.MethodProviders.Register(typeof(MyDataType), new MyReflectionProvider());
			query.DataContext.Tables.Add(myDataType2Values, "MyDataTypeValues");
			query.Text = "SELECT t.GetIntValue(), t.GetStringValue(), t.DO_IT(10) FROM MyDataTypeValues t";
			dataGridView1.DataSource = query.ExecuteDataTable();
			
			#endregion
		}
	}
}