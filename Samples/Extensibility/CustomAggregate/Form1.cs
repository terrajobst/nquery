using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NQuery.Samples.CustomAggregate
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		#region Employee

		public class Employee
		{
			private string _firstName;
			private string _lastName;
			private string _city;

			public Employee(string firstName, string lastName, string city)
			{
				_firstName = firstName;
				_lastName = lastName;
				_city = city;
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

			// Now we will create a list of employees grouped
			// by their city.

			// Create some sample data
			List<Employee> employees = new List<Employee>();
			employees.Add(new Employee("Nancy", "Davolio", "Seattle"));
			employees.Add(new Employee("Andrew", "Fuller", "Tacoma"));
			employees.Add(new Employee("Janet", "Leverling", "Kirkland"));
			employees.Add(new Employee("Margaret", "Peacock", "Redmond"));
			employees.Add(new Employee("Steven", "Buchanan", "London"));
			employees.Add(new Employee("Michael", "Suyama", "London"));
			employees.Add(new Employee("Robert", "King", "London"));
			employees.Add(new Employee("Laura", "Callahan", "Seattle"));
			employees.Add(new Employee("Anne", "Dodsworth", "London"));

			Query query = new Query();
			query.DataContext.Tables.Add(employees, "Employees");
			query.Text = "SELECT e.City, MyAggregate(e.FirstName + ' ' + e.LastName) AS Employees FROM Employees e GROUP BY e.City";

			// Now we want to publish our custom aggregate
			CustomAggregate customAggregate = new CustomAggregate("MyAggregate");
			query.DataContext.Aggregates.Add(customAggregate);

			dataGridView1.DataSource = query.ExecuteDataTable();
			#endregion
		}
	}
}