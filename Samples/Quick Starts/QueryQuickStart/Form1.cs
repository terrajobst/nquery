using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NQuery.Samples.QueryQuickStart
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
			private int _id;
			private string _firstName;
			private string _lastName;
			private int _companyId;

			public Employee(int id, string firstName, string lastName, int companyId)
			{
				_id = id;
				_companyId = companyId;
				_firstName = firstName;
				_lastName = lastName;
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

			public int CompanyId
			{
				get { return _companyId; }
				set { _companyId = value; }
			}
		}

		#endregion

		#region Company
		
		public class Company
		{
			private int _id;
			private string _name;

			public Company(int id, string name)
			{
				_id = id;
				_name = name;
			}

			public int Id
			{
				get { return _id; }
				set { _id = value; }
			}

			public string Name
			{
				get { return _name; }
				set { _name = value; }
			}
		}

		#endregion

		private void runButton_Click(object sender, EventArgs e)
		{
			#region Usage

			// Create some sample data.

			List<Company> companies = new List<Company>();
			companies.Add(new Company(1, "Great Lakes Food Market"));
			companies.Add(new Company(2, "Hungry Coyote Import Store"));
			companies.Add(new Company(3, "Lazy K Kountry Store"));
			companies.Add(new Company(4, "Let's Stop N Shop"));
			companies.Add(new Company(5, "Lonesome Pine Restaurant"));
			companies.Add(new Company(6, "Old World Delicatessen"));
			companies.Add(new Company(7, "Rattlesnake Canyon Grocery"));
			companies.Add(new Company(8, "Save-a-lot Markets"));
			companies.Add(new Company(9, "Split Rail Beer & Ale"));

			List<Employee> employees = new List<Employee>();
			employees.Add(new Employee(1, "Nancy", "Davolio", 4));
			employees.Add(new Employee(2, "Andrew", "Fuller", 3));
			employees.Add(new Employee(3, "Janet", "Leverling", 1));
			employees.Add(new Employee(4, "Margaret", "Peacock", 6));
			employees.Add(new Employee(5, "Steven", "Buchanan", 8));
			employees.Add(new Employee(6, "Michael", "Suyama", 9));
			employees.Add(new Employee(7, "Robert", "King", 2));
			employees.Add(new Employee(8, "Laura", "Callahan", 5));
			employees.Add(new Employee(9, "Anne", "Dodsworth", 7));

			// Create data context containing the tables "Employees" and "Companies"

			DataContext dataContext = new DataContext();
			dataContext.Tables.Add(employees, "Employees");
			dataContext.Tables.Add(companies, "Companies");

			// Create a query that joins each employee with its company
			// and returns the name of the company and the full name
			// of the employee.

			Query query = new Query(dataContext);
			query.Text = @"
                SELECT  c.Name AS Company,
						e.FirstName + ' ' + e.LastName AS Employee
                FROM    Employees e
                            INNER JOIN Companies c ON c.Id = e.CompanyId
            ";

			// Execute the query and get a DataTable containing the result

			dataGridView1.DataSource = query.ExecuteDataTable();
			#endregion
		}
	}
}