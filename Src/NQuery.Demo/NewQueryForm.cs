using System;
using System.IO;
using System.Windows.Forms;

namespace NQuery.Demo
{
	internal sealed partial class NewQueryForm : Form
	{
		public NewQueryForm()
		{
			InitializeComponent();
			LoadDatabaseFiles();
		}

		public sealed class ItemDefinition
		{
			private string _dataSetPath;
			private AddInDefinition _addInDefinition;

			private ItemDefinition(string dataSetPath, AddInDefinition addInDefinition)
			{
				_dataSetPath = dataSetPath;
				_addInDefinition = addInDefinition;
			}

			public static ItemDefinition FromDataSet(string dataSetPath)
			{
				return new ItemDefinition(dataSetPath, null);
			}

			public static ItemDefinition FromAddInDefinition(AddInDefinition addInDefinition)
			{
				return new ItemDefinition(null, addInDefinition);
			}

			public string DataSetPath
			{
				get { return _dataSetPath; }
			}

			public AddInDefinition AddInDefinition
			{
				get { return _addInDefinition; }
			}
		}

		private void LoadDatabaseFiles()
		{
			databasesListView.BeginUpdate();
			try
			{
				databasesListView.Items.Clear();

				foreach (string databaseFile in DataSetManager.GetAllDatabaseFiles())
				{
					ListViewItem item = new ListViewItem();
					item.Text = Path.GetFileNameWithoutExtension(databaseFile);
					item.Tag = ItemDefinition.FromDataSet(databaseFile);
					item.ImageIndex = 0;
					databasesListView.Items.Add(item);
				}

				foreach (AddInDefinition addInDefinition in AddInManager.AddInDefinitions)
				{
					if (!addInDefinition.HasErrors)
					{
						ListViewItem item = new ListViewItem();
						item.Text = addInDefinition.Instance.Name;
						item.Tag = ItemDefinition.FromAddInDefinition(addInDefinition);
						item.ImageIndex = 1;
						databasesListView.Items.Add(item);
					}
				}

				if (databasesListView.Items.Count > 0)
				{
					databasesListView.Items[0].Selected = true;
					databasesListView.Items[0].Focused = true;
				}
			}
			finally
			{
				databasesListView.EndUpdate();
			}
		}

		public ItemDefinition SelectedItem
		{
			get
			{
				if (databasesListView.SelectedItems.Count == 0)
					return null;

				return databasesListView.SelectedItems[0].Tag as ItemDefinition;
			}
		}

		private void LoadDataSetForm_Shown(object sender, EventArgs e)
		{
			databasesListView.Focus();
		}

		private void databasesListView_DoubleClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void databasesListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			okButton.Enabled = databasesListView.SelectedItems.Count > 0;
		}
	}
}