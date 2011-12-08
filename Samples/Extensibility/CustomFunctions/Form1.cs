using System;
using System.Collections.Generic;
using System.Windows.Forms;

using NQuery.Runtime;

namespace NQuery.Samples.CustomFunctions
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void staticFunctionContainerButton_Click(object sender, EventArgs e)
		{
			#region Static Function Container Usage

			Query query = new Query();

			// This adds all static functions of the type StaticFunctionContainer that are 
			// marked with the FunctionBinding attribute to the data context.
			query.DataContext.Functions.AddFromContainer(typeof(StaticFunctionContainer));

			query.Text = "SELECT StaticFunctionFromContainer() AS Result";
			dataGridView1.DataSource = query.ExecuteDataTable();
			
			#endregion
		}

		private void instanceFunctionContainerButton_Click(object sender, EventArgs e)
		{
			#region Instance Function Container Usage

			Query query = new Query();

			// This adds both all instance and static functions of containerInstance that
			// are marked with the FunctionBinding attribute to the data context.
			InstanceFunctionContainer containerInstance = new InstanceFunctionContainer(42);
			query.DataContext.Functions.AddFromContainer(containerInstance);

			query.Text = "SELECT InstanceFunctionFromContainer() AS Result";
			dataGridView1.DataSource = query.ExecuteDataTable();
			
			#endregion
		}

		#region Delegate Declaration

		public delegate string MyFunctionDelegate(int a);

		public static string MyFunctionDelegateImplementation(int a)
		{
			return "The value is: " + a.ToString();
		}
		
		#endregion

		private void delagateFunctionButton_Click(object sender, EventArgs e)
		{
			#region Delegate Function Usage
			
			Query query = new Query();

			// This adds the delegate 'MyFunctionDelegate' as a function to the data context.
			query.DataContext.Functions.Add("FunctionFromDelegate", new MyFunctionDelegate(MyFunctionDelegateImplementation));

			query.Text = "SELECT FunctionFromDelegate(42) AS Result";
			dataGridView1.DataSource = query.ExecuteDataTable();

			#endregion
		}

		private void dynamicFunctionsButton_Click(object sender, EventArgs e)
		{
			#region Dynamic Function Usage

			Query query = new Query();

			Expression<object> functionBody = new Expression<object>();
			functionBody.Parameters.Add("firstName", typeof (string));
			functionBody.Parameters.Add("lastName", typeof(string));
			functionBody.Text = "lastName + ', ' + firstName";

			MyFunctionBinding myFunctionBinding = new MyFunctionBinding("CreateFullname", functionBody);

			// This adds the dynamic function to the data context
			query.DataContext.Functions.Add(myFunctionBinding);

			query.Text = "SELECT CreateFullname('Abraham', 'Lincoln') AS Result";
			dataGridView1.DataSource = query.ExecuteDataTable();

			#endregion
		}
	}
}