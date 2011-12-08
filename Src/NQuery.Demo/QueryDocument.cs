using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using ActiproSoftware.SyntaxEditor;

using NQuery.Demo.Properties;
using NQuery.UI;

using TD.SandDock;

namespace NQuery.Demo
{
	internal sealed partial class QueryDocument : UserTabbedDocument, 
		IHandlerProvider<IClipboardHandler>,
		IHandlerProvider<IUndoRedoHandler>,
		IHandlerProvider<IIntelliSenseHandler>,
		IIntelliSenseHandler,
		IQueryHandler,
		IDocumentBrowserProviderHandler
	{
		private IObjectSelectionHandler _objectSelectionHandler;
		private SyntaxEditorClipboardHandler _syntaxEditorClipboardHandler;
		private SyntaxEditorUndoRedoHandler _syntaxEditorUndoRedoHandler;
		private ResultsDataGridViewClipboardHandler _resultsDataGridViewClipboardHandler;
		private ErrorListClipboardHandler _errorListClipboardHandler;
		private QueryBrowser _queryBrowser;
		private Query _query;
		private ActiproLink _actiproLink;

		public QueryDocument(IHandlerRegistry handlerRegistry, Query query)
		{
			InitializeComponent();

			_objectSelectionHandler = handlerRegistry.GetHandler<IObjectSelectionHandler>();
			_syntaxEditorClipboardHandler = new SyntaxEditorClipboardHandler(syntaxEditor);
			_syntaxEditorUndoRedoHandler = new SyntaxEditorUndoRedoHandler(syntaxEditor);
			_resultsDataGridViewClipboardHandler = new ResultsDataGridViewClipboardHandler(resultsDataGridView);
			_errorListClipboardHandler = new ErrorListClipboardHandler(errorsListView);

			_query = query;

			_actiproLink = new ActiproLink(components);
			_actiproLink.SyntaxEditor = syntaxEditor;
			_actiproLink.Evaluatable = _query;

			_queryBrowser = new QueryBrowser(handlerRegistry, _query);

			resultsDataGridView.AllowUserToResizeRows = false;
			resultsDataGridView.BackgroundColor = System.Drawing.SystemColors.Window;
			resultsDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			resultsDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
			resultsDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			resultsDataGridView.GridColor = System.Drawing.SystemColors.ActiveBorder;
			resultsDataGridView.RowHeadersVisible = false;
			resultsDataGridView.DataError += delegate { };

			syntaxEditor.Text = _query.Text;

			ClearStatusPanel();
		}

		#region Interfaces

		IClipboardHandler IHandlerProvider<IClipboardHandler>.Handler
		{
			get
			{
				if (syntaxEditor.Focused)
					return _syntaxEditorClipboardHandler;

				if (resultsDataGridView.Focused)
					return _resultsDataGridViewClipboardHandler;

				if (errorsListView.Focused)
					return _errorListClipboardHandler;

				return null;
			}
		}

		IIntelliSenseHandler IHandlerProvider<IIntelliSenseHandler>.Handler
		{
			get
			{
				if (syntaxEditor.Focused)
					return this;

				return null;
			}
		}

		IUndoRedoHandler IHandlerProvider<IUndoRedoHandler>.Handler
		{
			get
			{
				if (syntaxEditor.Focused)
					return _syntaxEditorUndoRedoHandler;

				return null;
			}
		}

		void IQueryHandler.Execute()
		{
			Execute();
		}

		void IQueryHandler.ShowPlan()
		{
			ExplainQuery();
		}

		void IQueryHandler.SaveData()
		{
			SaveGrid();
		}

		void IQueryHandler.ImportTestDefinition()
		{
			ImportTestDefinition();
		}
		
		void IQueryHandler.ExportTestDefinition()
		{
			ExportTestDefinition();
		}

		void IIntelliSenseHandler.ListMembers()
		{
			_actiproLink.ListMembers();
		}

		void IIntelliSenseHandler.ParameterInfo()
		{
			_actiproLink.ParameterInfo();
		}

		void IIntelliSenseHandler.CompleteWord()
		{
			_actiproLink.CompleteWord();
		}

		Control IDocumentBrowserProviderHandler.DocumentBrowser
		{
			get { return _queryBrowser; }
		}

		#endregion

		#region Status Panel

		private void ClearStatusPanel()
		{
			queryResultStatusLabel.Text = String.Empty;
			queryResultStatusLabel.Image = null;
			queryTimeStatusLabel.Text = String.Format("{0}", TimeSpan.Zero);
			queryRowsStatusLabel.Text = String.Format("{0:N0} rows", 0);
		}

		private void UpdateStatusPanelEditLocation()
		{
			// For Actipro Syntax Edit 3.1 support comment out the following line...
			editLocationToolStripStatusLabel.Text = String.Format("Ln {0,-10} Col {1,-10} Ch {2,-10}", syntaxEditor.Caret.DisplayDocumentPosition.Line, syntaxEditor.Caret.DisplayCharacterColumn, syntaxEditor.Caret.CharacterColumn);
			// ...and uncomment this one.
			// editLocationToolStripStatusLabel.Text = String.Format("Ln {0,-10} Col {1,-10} Ch {2,-10}", syntaxEditor.Caret.Position.Line + 1, syntaxEditor.Caret.DisplayCharacterColumn, syntaxEditor.Caret.Position.Character + 1);
		}

		#endregion

		#region Execution

		private void Execute()
		{
			ClearStatusPanel();
			ClearErrors();
			try
			{
				resultsDataGridView.DataSource = null;
				showPlanControl.ShowPlan = null;

				_query.Text = syntaxEditor.Text;

				Stopwatch stopwatch = Stopwatch.StartNew();
				DataTable result = _query.ExecuteDataTable();
				stopwatch.Stop();

				showPlanControl.ShowPlan = _query.GetShowPlan();
				LoadResults(result);

				queryResultStatusLabel.Image = Resources.StatusBarSuccess;
				queryResultStatusLabel.Text = "Query executed successfully.";
				queryTimeStatusLabel.Text = String.Format("{0}", stopwatch.Elapsed);
				queryRowsStatusLabel.Text = String.Format("{0:N0} rows", result.Rows.Count);
			}
			catch (CompilationException ex)
			{
				LoadErrors(ex.CompilationErrors);
				SetErrorState();
			}
			catch (RuntimeException ex)
			{
				LoadErrors(ex);
				SetErrorState();
			}
		}

		private void LoadResults(DataTable result)
		{
			tabControl.SelectedTab = resultsTabPage;
			resultsDataGridView.DataSource = result;

			for (int i = 0; i < resultsDataGridView.Columns.Count; i++)
			{
				DataColumn dataColumn = result.Columns[i];
				DataGridViewColumn gridColumn = resultsDataGridView.Columns[i];

				string columnName = dataColumn.Caption;
				string columnType = result.Columns[i].DataType.Name;

				if (String.IsNullOrEmpty(columnName))
					columnName = "(No column name)";

				gridColumn.HeaderText = String.Format("{0}\n({1})", columnName, columnType);
			}

			resultsDataGridView.AutoResizeColumns();
		}

		private void ExplainQuery()
		{
			ClearStatusPanel();
			ClearErrors();
			try
			{
				_query.Text = syntaxEditor.Text;
				ShowPlan showPlan = _query.GetShowPlan();
				showPlanControl.ShowPlan = showPlan;
				tabControl.SelectedTab = explainPlanTabPage;
				showPlanControl.Focus();
			}
			catch (CompilationException ex)
			{
				LoadErrors(ex.CompilationErrors);
				SetErrorState();
			}
		}

		#endregion

		#region Error Handling

		private class ErrorListClipboardHandler : IClipboardHandler
		{
			private ListView _errorListView;

			public ErrorListClipboardHandler(ListView errorListView)
			{
				_errorListView = errorListView;
			}

			public bool HasSelection
			{
				get { return _errorListView.SelectedItems.Count > 0; }
			}

			public bool IsReadOnly
			{
				get { return true; }
			}

			public void Cut()
			{
				throw new NotImplementedException();
			}

			public void Copy()
			{
				CopyErrorMessagesToClipboard(_errorListView);
			}

			public void Paste()
			{
				throw new NotImplementedException();
			}

			public void Delete()
			{
				throw new NotImplementedException();
			}

			public void SelectAll()
			{
				foreach (ListViewItem listViewItem in _errorListView.Items)
					listViewItem.Selected = true;
			}
		}

		private void ClearErrors()
		{
			errorsListView.Items.Clear();
		}

		private void SetErrorState()
		{
			tabControl.SelectedTab = errorListTabPage;
			queryResultStatusLabel.Image = Resources.StatusBarError;
			queryResultStatusLabel.Text = "Query completed with errors.";
		}

		private static void CopyErrorMessagesToClipboard(ListView errorListView)
		{
			StringBuilder sb = new StringBuilder();
			foreach (ListViewItem item in errorListView.Items)
			{
				if (item.Selected)
				{
					sb.Append(item.Text);
					sb.Append(Environment.NewLine);
				}
			}

			Clipboard.SetDataObject(sb.ToString());
		}

		private void LoadErrors(RuntimeException runtimeException)
		{
			LoadErrors(runtimeException.Message);
		}

		private void LoadErrors(string runtimeError)
		{
			ListViewItem item = new ListViewItem();
			item.Text = runtimeError;
			item.ImageIndex = 0;
			errorsListView.Items.Add(item);
		}

		private void LoadErrors(IEnumerable<CompilationError> errors)
		{
			foreach (CompilationError error in errors)
			{
				ListViewItem item = new ListViewItem();
				item.ImageIndex = 0;
				item.Tag = error;
				item.Text = error.Text;
				if (error.SourceRange != SourceRange.None)
				{
					item.SubItems.Add((error.SourceRange.StartLocation.Line + 1).ToString());
					item.SubItems.Add((error.SourceRange.StartLocation.Column + 1).ToString());
				}
				errorsListView.Items.Add(item);
			}
		}

		private void GotoError(CompilationError compilationError)
		{
			if (compilationError.SourceRange != SourceRange.None)
			{
				TextRange offsetAndLength = _actiproLink.GetTextRange(compilationError.SourceRange);
				syntaxEditor.SelectedView.Selection.SelectRange(offsetAndLength.StartOffset, offsetAndLength.Length);
				syntaxEditor.Focus();
			}
		}

		#endregion

		#region Execution Plan Handling

		private void LoadPlan()
		{
			if (openPlanFileDialog.ShowDialog() == DialogResult.OK)
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(openPlanFileDialog.FileName);
				ShowPlan showPlan = ShowPlan.FromXml(xmlDocument);
				showPlanControl.ShowPlan = showPlan;
			}
		}

		private void SavePlan()
		{
			if (savePlanFileDialog.ShowDialog() == DialogResult.OK)
			{
				ShowPlan showPlan = _query.GetShowPlan();

				if (savePlanFileDialog.FilterIndex == 0)
				{
					showPlan.ToXml().Save(savePlanFileDialog.FileName);
				}
				else
				{
					using (StreamWriter sw = new StreamWriter(savePlanFileDialog.FileName))
						showPlan.WriteTo(sw, 2);
				}
			}
		}

		private void CopyPlanToClipboard()
		{
			ShowPlan showPlan = _query.GetShowPlan();

			using (StringWriter sw = new StringWriter())
			{
				XmlTextWriter xmlTextWriter = new XmlTextWriter(sw);
				showPlan.ToXml().WriteContentTo(xmlTextWriter);
				Clipboard.SetText(sw.ToString());
			}
		}

		#endregion

		#region Grid Data Handling

		private class ResultsDataGridViewClipboardHandler : IClipboardHandler
		{
			private DataGridView _dataGridView;

			public ResultsDataGridViewClipboardHandler(DataGridView dataGrid)
			{
				_dataGridView = dataGrid;
			}

			public bool HasSelection
			{
				get { return _dataGridView.SelectedCells.Count > 0; }
			}

			public bool IsReadOnly
			{
				get { return true; }
			}

			public void Cut()
			{
				throw new NotImplementedException();
			}

			public void Copy()
			{
				List<DataGridViewCell> selectedCells = new List<DataGridViewCell>(_dataGridView.SelectedCells.Count);
				foreach (DataGridViewCell selectedCell in _dataGridView.SelectedCells)
					selectedCells.Add(selectedCell);

				selectedCells.Sort(delegate(DataGridViewCell x, DataGridViewCell y)
								   {
									   int comparison = x.RowIndex.CompareTo(y.RowIndex);
									   if (comparison == 0)
										   comparison = x.ColumnIndex.CompareTo(y.ColumnIndex);
									   return comparison;
								   });

				int minColumn = Int32.MaxValue;
				selectedCells.ForEach(delegate(DataGridViewCell obj)
				                      {
				                      	if (minColumn > obj.ColumnIndex)
				                      		minColumn = obj.ColumnIndex;
				                      });

				StringBuilder sb = new StringBuilder();
				int lastRowIndex = -1;
				foreach (DataGridViewCell cell in selectedCells)
				{
					if (sb.Length > 0)
					{
						if (cell.RowIndex == lastRowIndex)
							sb.Append("\t");
						else
							sb.AppendLine();
					}

					if (cell.RowIndex != lastRowIndex)
					{
						for (int i = 0; i < cell.ColumnIndex - minColumn - 1; i++)
							sb.Append("\t");						
					}

					string value = cell.Value == null ? String.Empty : cell.Value.ToString();
					value = value.Replace("\"", "\"\"");

					sb.Append("\"");
					sb.Append(value);
					sb.Append("\"");

					lastRowIndex = cell.RowIndex;
				}

				if (sb.Length > 0)
				{
					// Clipboard.SetText throws an exception if the text has a length
					// of zero.
					Clipboard.SetText(sb.ToString());
				}
			}

			public void Paste()
			{
				throw new NotImplementedException();
			}

			public void Delete()
			{
				throw new NotImplementedException();
			}

			public void SelectAll()
			{
				_dataGridView.SelectAll();
			}
		}

		private static string DataTableToCsv(DataTable gridData)
		{
			bool isFirst;
			StringBuilder sb = new StringBuilder();
			isFirst = true;
			foreach (DataColumn column in gridData.Columns)
			{
				if (isFirst)
					isFirst = false;
				else
					sb.Append("\t");

				sb.Append("\"");
				sb.Append(column.ColumnName);
				sb.Append("\"");
			}
			sb.Append(Environment.NewLine);

			foreach (DataRow dataRow in gridData.Rows)
			{
				isFirst = true;

				foreach (object value in dataRow.ItemArray)
				{
					if (isFirst)
						isFirst = false;
					else
						sb.Append("\t");

					sb.Append("\"");
					sb.Append(value.ToString());
					sb.Append("\"");
				}
				sb.Append(Environment.NewLine);
			}

			return sb.ToString();
		}

		private DataTable GetGridData()
		{
			return resultsDataGridView.DataSource as DataTable;
		}

		private void SaveGrid()
		{
			DataTable gridData = GetGridData();
			if (gridData != null)
			{
				if (saveGridDataFileDialog.ShowDialog() == DialogResult.OK)
				{
					switch (saveGridDataFileDialog.FilterIndex)
					{
						case 1: // CSV
							File.WriteAllText(saveGridDataFileDialog.FileName, DataTableToCsv(gridData));
							break;
						case 2: // DataSet
							DataSet dataSet = new DataSet(Path.GetFileNameWithoutExtension(saveGridDataFileDialog.FileName));
							dataSet.Tables.Add(gridData);
							dataSet.WriteXml(saveGridDataFileDialog.FileName, XmlWriteMode.WriteSchema);
							dataSet.Tables.Remove(gridData);
							break;
					}
				}
			}
		}

		#endregion

		#region Test Definition

		private void ExportTestDefinition()
		{
			if (saveTestDefinitionFileDialog.ShowDialog() == DialogResult.OK)
			{
				Text = String.Format("NQuery - [{0}]", Path.GetFileName(saveTestDefinitionFileDialog.FileName));
				CompilationErrorCollection compilationErrors = null;
				RuntimeException runtimeException = null;
				DataTable result = null;
				ShowPlan showPlan = null;

				Query query = new Query();
				query.DataContext = _query.DataContext;
				query.Text = syntaxEditor.Text;

				try
				{
					result = query.ExecuteDataTable();
					showPlan = query.GetShowPlan();
				}
				catch (RuntimeException ex)
				{
					runtimeException = ex;
				}
				catch (CompilationException ex)
				{
					compilationErrors = ex.CompilationErrors;
				}

				XmlDocument testDefinition = new XmlDocument();
				XmlNode rootNode = testDefinition.CreateElement("test");
				testDefinition.AppendChild(rootNode);

				XmlNode sqlNode = testDefinition.CreateElement("sql");
				sqlNode.InnerText = syntaxEditor.Text;
				rootNode.AppendChild(sqlNode);

				if (runtimeException != null)
				{
					XmlNode expectedRuntimeErrorNode = testDefinition.CreateElement("expectedRuntimeError");
					rootNode.AppendChild(expectedRuntimeErrorNode);
					expectedRuntimeErrorNode.InnerText = runtimeException.Message;
				}
				else if (compilationErrors != null)
				{
					XmlNode expectedErrorsNode = testDefinition.CreateElement("expectedErrors");
					rootNode.AppendChild(expectedErrorsNode);

					foreach (CompilationError error in compilationErrors)
					{
						XmlNode errorNode = testDefinition.CreateElement("expectedError");

						XmlAttribute idAtt = testDefinition.CreateAttribute("id");
						idAtt.Value = error.Id.ToString();
						errorNode.Attributes.Append(idAtt);

						XmlAttribute textAtt = testDefinition.CreateAttribute("text");
						textAtt.Value = error.Text;
						errorNode.Attributes.Append(textAtt);

						expectedErrorsNode.AppendChild(errorNode);
					}
				}

				if (result != null)
				{
					XmlNode resultsNode = testDefinition.CreateElement("expectedResults");
					rootNode.AppendChild(resultsNode);

					StringBuilder sb = new StringBuilder();
					using (StringWriter stringWriter = new StringWriter(sb))
					{
						DataSet dataSet = new DataSet();
						dataSet.Tables.Add(result);
						dataSet.WriteXml(stringWriter, XmlWriteMode.WriteSchema);
					}

					resultsNode.InnerXml = sb.ToString();
				}

				if (showPlan != null)
				{
					XmlNode planNode = testDefinition.CreateElement("expectedPlan");
					rootNode.AppendChild(planNode);

					XmlNode executionPlanNode = testDefinition.ImportNode(showPlan.ToXml().SelectSingleNode("executionPlan"), true);
					planNode.AppendChild(executionPlanNode);
				}

				testDefinition.Save(saveTestDefinitionFileDialog.FileName);
			}
		}

		private void ImportTestDefinition()
		{
			if (openTestDefinitionFileDialog.ShowDialog() == DialogResult.OK)
			{
				Text = String.Format("NQuery - [{0}]", Path.GetFileName(openTestDefinitionFileDialog.FileName));
				saveTestDefinitionFileDialog.FileName = openTestDefinitionFileDialog.FileName;

				string query;
				List<CompilationError> errorList = new List<CompilationError>();
				string runtimeError = null;
				DataSet dataSet = null;
				ShowPlan showPlan = null;

				#region XML Handling

				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(openTestDefinitionFileDialog.FileName);

				query = xmlDocument.SelectSingleNode("/test/sql").InnerText;

				XmlNode expectedRuntimeErrorNode = xmlDocument.SelectSingleNode("/test/expectedRuntimeError");
				if (expectedRuntimeErrorNode != null)
					runtimeError = expectedRuntimeErrorNode.InnerText;

				XmlNode expectedErrorsNode = xmlDocument.SelectSingleNode("/test/expectedErrors");
				if (expectedErrorsNode != null)
				{
					foreach (XmlNode expectedErrorNode in expectedErrorsNode.SelectNodes("expectedError"))
					{
						ErrorId errorId = (ErrorId)Enum.Parse(typeof(ErrorId), expectedErrorNode.Attributes["id"].Value);
						string errorText = expectedErrorNode.Attributes["text"].Value;
						CompilationError compilationError = new CompilationError(SourceRange.Empty, errorId, errorText);
						errorList.Add(compilationError);
					}
				}

				XmlNode expectedResultsNode = xmlDocument.SelectSingleNode("/test/expectedResults");
				if (expectedResultsNode != null)
				{
					dataSet = new DataSet();
					using (StringReader stringReader = new StringReader(expectedResultsNode.InnerXml))
					{
						dataSet.ReadXml(stringReader);
					}
				}

				XmlNode expectedPlanNode = xmlDocument.SelectSingleNode("/test/expectedPlan");
				if (expectedPlanNode != null)
				{
					showPlan = ShowPlan.FromXml(expectedPlanNode);
				}

				#endregion

				ClearErrors();
				resultsDataGridView.DataSource = null;
				showPlanControl.ShowPlan = null;

				syntaxEditor.Text = query;
				if (runtimeError != null)
					LoadErrors(runtimeError);
				LoadErrors(errorList);
				if (dataSet != null)
					LoadResults(dataSet.Tables[0]);
				if (showPlan != null)
					showPlanControl.ShowPlan = showPlan;
			}
		}

		#endregion

		#region Key Processing

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
			{
				case Keys.Control | Keys.J:
					_actiproLink.ListMembers();
					return true;

				case Keys.Control | Keys.Space:
					_actiproLink.CompleteWord();
					return true;

				case Keys.Control | Keys.Shift | Keys.Space:
					_actiproLink.ParameterInfo();
					return true;

				default:
					return base.ProcessCmdKey(ref msg, keyData);
			}
		}

		#endregion

		private void syntaxEditor_SelectionChanged(object sender, SelectionEventArgs e)
		{
			UpdateStatusPanelEditLocation();
		}

		private void errorListView_DoubleClick(object sender, EventArgs e)
		{
			if (errorsListView.FocusedItem != null)
			{
				CompilationError compilationError = errorsListView.FocusedItem.Tag as CompilationError;
				if (compilationError != null)
					GotoError(compilationError);
			}
		}

		private void errorListView_Resize(object sender, EventArgs e)
		{
			int remainingWith = errorsListView.ClientSize.Width;
			for (int i = 1; i < errorsListView.Columns.Count; i++)
				remainingWith -= errorsListView.Columns[i].Width;

			if (remainingWith <= 0)
				remainingWith = 15;

			errorsListView.Columns[0].Width = remainingWith;
		}

		private void showPlanControl_SelectedElementChanged(object sender, EventArgs e)
		{
			_objectSelectionHandler.SelectedObject = showPlanControl.SelectedElement;
		}

		private void editorContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			undoToolStripMenuItem.Enabled = _syntaxEditorUndoRedoHandler.CanUndo();
			redoToolStripMenuItem.Enabled = _syntaxEditorUndoRedoHandler.CanRedo();
			cutToolStripMenuItem.Enabled = !_syntaxEditorClipboardHandler.IsReadOnly && _syntaxEditorClipboardHandler.HasSelection;
			copyToolStripMenuItem.Enabled = _syntaxEditorClipboardHandler.HasSelection;
			pasteToolStripMenuItem.Enabled = !_syntaxEditorClipboardHandler.IsReadOnly;
		}

		private void undoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_syntaxEditorUndoRedoHandler.CanUndo())
				_syntaxEditorUndoRedoHandler.Undo();
		}

		private void redoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_syntaxEditorUndoRedoHandler.CanRedo())
				_syntaxEditorUndoRedoHandler.Redo();
		}

		private void cutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!_syntaxEditorClipboardHandler.IsReadOnly && _syntaxEditorClipboardHandler.HasSelection)
				_syntaxEditorClipboardHandler.Cut();
		}

		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_syntaxEditorClipboardHandler.HasSelection)
				_syntaxEditorClipboardHandler.Copy();
		}

		private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!_syntaxEditorClipboardHandler.IsReadOnly)
				_syntaxEditorClipboardHandler.Paste();
		}

		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!_syntaxEditorClipboardHandler.IsReadOnly)
				_syntaxEditorClipboardHandler.Delete();
		}

		private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			_syntaxEditorClipboardHandler.SelectAll();
		}

		private void copyDataToolStripMenuItem_Click(object sender, EventArgs e)
		{
			_resultsDataGridViewClipboardHandler.Copy();
		}

		private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveGrid();
		}

		private void loadFromFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LoadPlan();
		}

		private void saveToFileToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			SavePlan();
		}

		private void copyPlanToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CopyPlanToClipboard();
		}
	}
}