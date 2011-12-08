using System;
using System.Windows.Forms;

namespace NQuery.Samples.CustomTable
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

			// Initialize custom table data object
			MyTableData myTableData = new MyTableData();
			myTableData.ColumnNames = new string[] {"ID", "FirstName", "LastName"};
			myTableData.ColumnTypes = new Type[] { typeof(int), typeof(string), typeof(string) };
			myTableData.Rows.Add(new object[] { 1, "Nancy", "Davolio" });
			myTableData.Rows.Add(new object[] { 2, "Andrew", "Fuller" });
			myTableData.Rows.Add(new object[] { 3, "Janet", "Leverling" });
			myTableData.Rows.Add(new object[] { 4, "Margaret", "Peacock" });
			myTableData.Rows.Add(new object[] { 5, "Steven", "Buchanan" });
			myTableData.Rows.Add(new object[] { 6, "Michael", "Suyama" });
			myTableData.Rows.Add(new object[] { 7, "Robert", "King" });
			myTableData.Rows.Add(new object[] { 8, "Laura", "Callahan" });
			myTableData.Rows.Add(new object[] { 9, "Anne", "Dodsworth" });

			// Create the custom table binding for the data object
			MyTableBinding myTableBinding = new MyTableBinding("Persons", myTableData);

			Query query = new Query();

			// Add the custom table binding to the data context.
			query.DataContext.Tables.Add(myTableBinding);

			// Run a query that sorts the persons.
			query.Text = "SELECT * FROM Persons";

			dataGridView1.DataSource = query.ExecuteDataTable();

			#endregion
		}
	}
}