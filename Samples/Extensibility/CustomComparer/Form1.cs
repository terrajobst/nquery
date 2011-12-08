using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NQuery.Samples.CustomComparer
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void runButton_Click(object sender, EventArgs e)
		{
			#region Usage

			// Create some sample data. Note that there is no specific
			// order with respect to the person's id.
			List<ExternalPersonDataType> persons = new List<ExternalPersonDataType>();
			persons.Add(new ExternalPersonDataType(9, "Anne", "Dodsworth"));
			persons.Add(new ExternalPersonDataType(4, "Margaret", "Peacock"));
			persons.Add(new ExternalPersonDataType(2, "Andrew", "Fuller"));
			persons.Add(new ExternalPersonDataType(5, "Steven", "Buchanan"));
			persons.Add(new ExternalPersonDataType(3, "Janet", "Leverling"));
			persons.Add(new ExternalPersonDataType(8, "Laura", "Callahan"));
			persons.Add(new ExternalPersonDataType(1, "Nancy", "Davolio"));
			persons.Add(new ExternalPersonDataType(6, "Michael", "Suyama"));
			persons.Add(new ExternalPersonDataType(7, "Robert", "King"));
			
			Query query = new Query();
			query.DataContext.Tables.Add(persons, "Persons");

			// Register a comparer for the type ExternalPersonDataType.
			query.DataContext.MetadataContext.Comparers.Register(typeof(ExternalPersonDataType), new MyComparer());

			// Run a query that sorts the persons.
			query.Text = "SELECT * FROM Persons p ORDER BY p";

			dataGridView1.DataSource = query.ExecuteDataTable();

			#endregion
		}
	}
}