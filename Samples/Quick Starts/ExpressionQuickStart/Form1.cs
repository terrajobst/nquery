using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace NQuery.Samples.ExpressionQuickStart
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		#region Customer

		public class Customer
		{
			private int _id;
			private string _firstName;
			private string _lastName;
			private string _city;

			public Customer(int id, string firstName, string lastName, string city)
			{
				_id = id;
				_firstName = firstName;
				_lastName = lastName;
				_city = city;
			}

			public int Id
			{
				get { return _id; }
				set { _id = value; }
			}

			public string FirstName
			{
				get { return _firstName; }
				set { _firstName = value; }
			}

			public string LastName
			{
				get { return _lastName; }
				set { _lastName = value; }
			}

			public string City
			{
				get { return _city; }
				set { _city = value; }
			}
		}

		#endregion

		private void runButton_Click(object sender, EventArgs e)
		{
			#region Usage

			// Create some sample data.

			List<Customer> customers = new List<Customer>();
			customers.Add(new Customer(1, "Nancy", "Davolio", "Seattle"));
			customers.Add(new Customer(2, "Andrew", "Fuller", "Tacoma"));
			customers.Add(new Customer(3, "Janet", "Leverling", "Kirkland"));
			customers.Add(new Customer(4, "Margaret", "Peacock", "Redmond"));
			customers.Add(new Customer(5, "Steven", "Buchanan", "London"));
			customers.Add(new Customer(6, "Michael", "Suyama", "London"));
			customers.Add(new Customer(7, "Robert", "King", "London"));
			customers.Add(new Customer(8, "Laura", "Callahan", "Seattle"));
			customers.Add(new Customer(9, "Anne", "Dodsworth", "London"));

			// Create expression to filter all customers that live in London

			Expression<bool> filterExpr = new Expression<bool>();
			filterExpr.Parameters.Add("c", typeof (Customer));
			filterExpr.Text = "c.City = 'London'";

			// Create expression to create the full name of a customer

			Expression<string> fullNameExpr = new Expression<string>();
			fullNameExpr.Parameters.Add("c", typeof(Customer));
			fullNameExpr.Text = "c.FirstName + ' ' + c.LastName";

			// Now we iterate over all customers and check if the filter
			// evaluates to true and if does we will evaluate the
			// full name expression and add the result to a data table.

			DataTable result = new DataTable();
			result.Columns.Add("FullName", typeof (string));

			foreach (Customer customer in customers)
			{
				filterExpr.Parameters["c"].Value = customer;
				fullNameExpr.Parameters["c"].Value = customer;

				if (filterExpr.Evaluate())
				{
					string fullname = fullNameExpr.Evaluate();
					result.Rows.Add(fullname);
				}
			}

			dataGridView1.DataSource = result;

			#endregion
		}
	}
}