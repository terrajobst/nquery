using System;
using System.Data;
using System.Windows.Forms;

using NQuery.Demo.AddIns;

namespace NQuery.Demo.DefaultAddins
{
	public sealed class LoadDataSetFromFileAddIn : IAddIn
	{
		public QueryContext CreateQueryContext()
		{
			using (OpenFileDialog dlg = new OpenFileDialog())
			{
				dlg.Filter = "DataSet Files (*.xml)|*.xml|All Files (*.*)|*.*";

				if (dlg.ShowDialog() != DialogResult.OK)
					return null;

				DataSet dataSet = new DataSet();
				dataSet.ReadXml(dlg.FileName);

				Query query = new Query();
				query.DataContext.AddTablesAndRelations(dataSet);

				QueryContext queryContext = new QueryContext(query, dataSet.DataSetName);
				return queryContext;
			}
		}

		public string Name
		{
			get { return "Load DataSet From File"; }
		}
	}
}
