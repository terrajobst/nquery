using System;
using System.IO;
using System.Windows.Forms;

namespace NQuery.Demo
{
	public partial class AddInForm : Form
	{
		private void LoadListView()
		{
			addinsListView.BeginUpdate();
			try
			{
				addinsListView.Items.Clear();

				foreach (AddInDefinition addInDefinition in AddInManager.AddInDefinitions)
				{
					string addInAssemblyPath = addInDefinition.AddInType.Assembly.ManifestModule.FullyQualifiedName;
					string addInAssemblyName = Path.GetFileNameWithoutExtension(addInAssemblyPath);

					ListViewItem item = new ListViewItem();

					if (!addInDefinition.HasErrors)
					{
						item.Text = addInDefinition.Instance.Name;
					}
					else
					{
						item.Text = addInAssemblyName;
					}

					item.SubItems.Add(Path.GetFileName(addInAssemblyPath));
					item.SubItems.Add(addInDefinition.AddInType.FullName);
					item.SubItems.Add(Path.GetDirectoryName(addInAssemblyPath));
					item.ImageIndex = addInDefinition.HasErrors ? 1 : 0;
					item.Tag = addInDefinition;
					addinsListView.Items.Add(item);
				}
			}
			finally
			{
				addinsListView.EndUpdate();
			}
		}

		public AddInForm()
		{
			InitializeComponent();
		}

		private void UpdateErrorInfo()
		{
			AddInDefinition addInDefinition = (AddInDefinition) addinsListView.FocusedItem.Tag;
			if (addInDefinition.HasErrors)
				errorsTextBox.Text = addInDefinition.Error;
			else
				errorsTextBox.Text = String.Format("The add-in '{0}' loaded successfully.", addInDefinition.Instance.Name);
		}

		private void AddInForm_Load(object sender, EventArgs e)
		{
			LoadListView();

			if (addinsListView.Items.Count > 0)
			{
				addinsListView.Items[0].Selected = true;
				addinsListView.FocusedItem = addinsListView.Items[0];
			}

			UpdateErrorInfo();
		}

		private void addinsListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (addinsListView.FocusedItem != null)
				UpdateErrorInfo();
		}
	}
}