using System;
using System.Data;
using System.Xml;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NQuery.Build
{
	public class RunSampleCode : Task
	{
		private ITaskItem[] _sourceFiles;
		private ITaskItem[] _dataSetFiles;

		[Required]
		public ITaskItem[] SourceFiles
		{
			get { return _sourceFiles; }
			set { _sourceFiles = value; }
		}

		public ITaskItem[] DataSetFiles
		{
			get { return _dataSetFiles; }
			set { _dataSetFiles = value; }
		}

		public override bool Execute()
		{
			Query query = new Query();

			foreach (ITaskItem dataSetFile in _dataSetFiles)
			{
				try
				{
					DataSet dataSet = new DataSet();
					dataSet.ReadXml(dataSetFile.ItemSpec);
					query.DataContext.AddTablesAndRelations(dataSet);
				}
				catch (Exception ex)
				{
					Log.LogError("Cannot load DataSet {0}: {1}", dataSetFile.ItemSpec, ex.Message);				
				}
			}

			foreach (ITaskItem sourceFile in _sourceFiles)
			{
				try
				{
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.Load(sourceFile.ItemSpec);
					xmlDocument.PreserveWhitespace = true;

					foreach (XmlNode sampleCodeNode in xmlDocument.SelectNodes("//sampleCode[@language='NQuery.SQL']"))
					{
						string sampleCode = sampleCodeNode.InnerText;

						query.Text = sampleCode;
						try
						{
							Log.LogMessage(MessageImportance.Normal, "Executing query '{0}'", sampleCode);
							DataTable result = query.ExecuteDataTable();
							XmlNode resultTable = ConvertDataTableToXml(xmlDocument, result);
							sampleCodeNode.ParentNode.InsertAfter(resultTable, sampleCodeNode);
						}
						catch (NQueryException ex)
						{
							LogQueryError(ex, sampleCodeNode, sourceFile.ItemSpec);
						}
					}

					xmlDocument.Save(sourceFile.ItemSpec);
				}
				catch (Exception ex)
				{
					Log.LogError("Cannot run sample code on file {0}: {1}", sourceFile.ItemSpec, ex.Message);
				}
			}

			return !Log.HasLoggedErrors;
		}

		private void LogQueryError(NQueryException ex, XmlNode sampleCodeNode, string fileName)
		{
			string lineNumberInfo;

			IXmlLineInfo lineInfo = sampleCodeNode as IXmlLineInfo;
			if (lineInfo != null && lineInfo.HasLineInfo())
				lineNumberInfo = String.Format("line {0}, col {1}", lineInfo.LineNumber, lineInfo.LinePosition);
			else
				lineNumberInfo = "[unknown location]";

			Log.LogError("Error executing sample code of {0} {1}: {2}", fileName, lineNumberInfo, ex);
		}

		private static XmlNode ConvertDataTableToXml(XmlDocument owner, DataTable result)
		{
			XmlNode tableNode = owner.CreateElement("table");
			XmlNode headerRowNode = owner.CreateElement("tr");
			tableNode.AppendChild(headerRowNode);

			foreach (DataColumn dataColumn in result.Columns)
			{
				XmlNode headerColumnNode = owner.CreateElement("th");
				headerColumnNode.InnerText = dataColumn.ColumnName;
				headerRowNode.AppendChild(headerColumnNode);
			}

			foreach (DataRow dataRow in result.Rows)
			{
				XmlNode rowNode = owner.CreateElement("tr");
				tableNode.AppendChild(rowNode);

				foreach (DataColumn dataColumn in result.Columns)
				{
					XmlNode columnNode = owner.CreateElement("td");
					columnNode.InnerText = dataRow[dataColumn].ToString();
					rowNode.AppendChild(columnNode);
				}
			}

			return tableNode;
		}
	}
}
