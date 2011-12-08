using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

using NQuery.Demo.AddIns;

using TD.SandDock;

namespace NQuery.Demo
{
	internal sealed partial class MainForm : Form,
		IHandlerRegistry,
		IObjectSelectionHandler
	{
		private Control _currentDocumentBrowser;
		private WelcomeDocument _welcomeDocument;

		public MainForm()
		{
			InitializeComponent();

			_welcomeDocument = new WelcomeDocument();
			_welcomeDocument.Manager = sandDockManager;
			_welcomeDocument.Open(WindowOpenMethod.OnScreenActivate);

			sandDockManager.MaximumDockContainerSize = Int32.MaxValue;

			OpenQuery(DataSetManager.DefaultDataSet, DataSetManager.DefaultQueryText, false);

			UpdateMenuStates();
			Application.Idle += Application_Idle;
		}

		private void Application_Idle(object sender, EventArgs e)
		{
			UpdateMenuStates();
		}

		object IObjectSelectionHandler.SelectedObject
		{
			get { return propertyGrid.SelectedObject; }
			set { propertyGrid.SelectedObject = value; }
		}

		public T GetHandler<T>() where T : class
		{
			T result = GetHandlerFromObject<T>(sandDockManager.ActiveTabbedDocument);

			if (result != null)
				return result;

			return GetHandlerFromObject<T>(this);
		}

		private static T GetHandlerFromObject<T>(object obj) where T : class
		{
			IHandlerProvider<T> handlerProvider = obj as IHandlerProvider<T>;

			if (handlerProvider != null)
				return handlerProvider.Handler;

			return obj as T;
		}

		private void UpdateMenuStates()
		{
			closeToolStripMenuItem.Enabled = sandDockManager.ActiveTabbedDocument != null;

			IUndoRedoHandler undoRedoHandler = GetHandler<IUndoRedoHandler>();
			undoToolStripMenuItem.Enabled = undoRedoHandler != null && undoRedoHandler.CanUndo();
			redoToolStripMenuItem.Enabled = undoRedoHandler != null && undoRedoHandler.CanRedo();
			undoToolStripButton.Enabled = undoRedoHandler != null && undoRedoHandler.CanUndo();
			redoToolStripButton.Enabled = undoRedoHandler != null && undoRedoHandler.CanRedo();

			IClipboardHandler clipboardHandler = GetHandler<IClipboardHandler>();
			cutToolStripMenuItem.Enabled = clipboardHandler != null && clipboardHandler.HasSelection && !clipboardHandler.IsReadOnly;
			copyToolStripMenuItem.Enabled = clipboardHandler != null && clipboardHandler.HasSelection;
			pasteToolStripMenuItem.Enabled = clipboardHandler != null && !clipboardHandler.IsReadOnly;
			deleteToolStripMenuItem.Enabled = clipboardHandler != null && clipboardHandler.HasSelection && !clipboardHandler.IsReadOnly;
			selectAllToolStripMenuItem.Enabled = clipboardHandler != null;
			cutToolStripButton.Enabled = clipboardHandler != null && clipboardHandler.HasSelection && !clipboardHandler.IsReadOnly;
			copyToolStripButton.Enabled = clipboardHandler != null && clipboardHandler.HasSelection;
			pasteToolStripButton.Enabled = clipboardHandler != null && !clipboardHandler.IsReadOnly;

			IQueryHandler queryHandler = GetHandler<IQueryHandler>();
			importTestDefinitionToolStripMenuItem.Enabled = queryHandler != null;
			exportTestDefinitionToolStripMenuItem.Enabled = queryHandler != null;
			executeToolStripMenuItem.Enabled = queryHandler != null;
			explainToolStripMenuItem.Enabled = queryHandler != null;
			executeToolStripButton.Enabled = queryHandler != null;
			explainQueryToolStripButton.Enabled = queryHandler != null;
			saveResultsToolStripButton.Enabled = queryHandler != null;

			IIntelliSenseHandler intelliSenseHandler = GetHandler<IIntelliSenseHandler>();
			listMembersToolStripMenuItem.Enabled = intelliSenseHandler != null;
			parameterInfoToolStripMenuItem.Enabled = intelliSenseHandler != null;
			completeWordToolStripMenuItem.Enabled = intelliSenseHandler != null;
		}

		private void OpenQuery(string dataSetPath, string queryText, bool autoFocus)
		{
			string queryTitlePrefix = Path.GetFileNameWithoutExtension(dataSetPath);
			DataSet dataSet = DataSetManager.GetDataSet(dataSetPath);

			Query query = new Query();
			query.DataContext.AddTablesAndRelations(dataSet);
			query.Text = queryText;

			OpenQuery(query, autoFocus, queryTitlePrefix);
		}

		private void OpenQuery(Query query, bool autoFocus, string queryTitlePrefix)
		{
			string queryTitle;
			bool unique;
			int counter = 1;
			do
			{
				queryTitle = String.Format("{0} {1}", queryTitlePrefix, counter++);
				unique = true;

				foreach (DockControl documentDockControl in sandDockManager.GetDockControls(DockSituation.Docked))
				{
					if (documentDockControl is QueryDocument)
					{
						unique = String.Compare(documentDockControl.Text, queryTitle, StringComparison.InvariantCultureIgnoreCase) != 0;
						if (!unique)
							break;
					}
				}
			} while (!unique);

			QueryDocument queryDocument = new QueryDocument(this, query);
			queryDocument.Manager = sandDockManager;
			queryDocument.Text = queryTitle;
			queryDocument.Open(autoFocus ? WindowOpenMethod.OnScreenActivate : WindowOpenMethod.OnScreen);
		}

		private void sandDockManager_ActiveTabbedDocumentChanged(object sender, EventArgs e)
		{
			Control oldDocumentBrowser = _currentDocumentBrowser;
			_currentDocumentBrowser = null;

			IDocumentBrowserProviderHandler documentBrowserProviderHandler = GetHandler<IDocumentBrowserProviderHandler>();
			if (documentBrowserProviderHandler != null)
			{
				_currentDocumentBrowser = documentBrowserProviderHandler.DocumentBrowser;
				if (_currentDocumentBrowser.Parent == null)
				{
					_currentDocumentBrowser.Dock = DockStyle.Fill;
					_currentDocumentBrowser.Parent = documentBrowserDockableWindow;
				}
				_currentDocumentBrowser.Show();
			}

			if (oldDocumentBrowser != null)
				oldDocumentBrowser.Hide();

			noBrowserLabel.Visible = (_currentDocumentBrowser == null);
		}

		private void newQueryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (NewQueryForm newQueryForm = new NewQueryForm())
			{
				if (newQueryForm.ShowDialog() == DialogResult.OK && newQueryForm.SelectedItem != null)
				{
					if (newQueryForm.SelectedItem.AddInDefinition != null)
					{
						QueryContext queryContext = newQueryForm.SelectedItem.AddInDefinition.Instance.CreateQueryContext();
						if (queryContext != null)
							OpenQuery(queryContext.Query, true, queryContext.QueryName);
					}
					else
					{
						OpenQuery(newQueryForm.SelectedItem.DataSetPath, String.Empty, true);
					}
				}
			}
		}

		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (sandDockManager.ActiveTabbedDocument != null)
				sandDockManager.ActiveTabbedDocument.Close();
		}

		private void importTestDefinitionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IQueryHandler queryHandler = GetHandler<IQueryHandler>();
			if (queryHandler != null)
				queryHandler.ImportTestDefinition();

		}

		private void exportTestDefinitionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IQueryHandler queryHandler = GetHandler<IQueryHandler>();
			if (queryHandler != null)
				queryHandler.ExportTestDefinition();
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void undoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IUndoRedoHandler undoRedoHandler = GetHandler<IUndoRedoHandler>();
			if (undoRedoHandler != null)
				undoRedoHandler.Undo();
		}

		private void redoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IUndoRedoHandler undoRedoHandler = GetHandler<IUndoRedoHandler>();
			if (undoRedoHandler != null)
				undoRedoHandler.Redo();
		}

		private void cutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IClipboardHandler clipboardHandler = GetHandler<IClipboardHandler>();
			if (clipboardHandler != null)
				clipboardHandler.Cut();
		}

		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IClipboardHandler clipboardHandler = GetHandler<IClipboardHandler>();
			if (clipboardHandler != null)
				clipboardHandler.Copy();
		}

		private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IClipboardHandler clipboardHandler = GetHandler<IClipboardHandler>();
			if (clipboardHandler != null)
				clipboardHandler.Paste();
		}

		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IClipboardHandler clipboardHandler = GetHandler<IClipboardHandler>();
			if (clipboardHandler != null)
				clipboardHandler.Delete();
		}

		private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IClipboardHandler clipboardHandler = GetHandler<IClipboardHandler>();
			if (clipboardHandler != null)
				clipboardHandler.SelectAll();
		}

		private void documentBrowserToolStripMenuItem_Click(object sender, EventArgs e)
		{
			documentBrowserDockableWindow.Open(WindowOpenMethod.OnScreenActivate);
		}

		private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			propertiesDockableWindow.Open(WindowOpenMethod.OnScreenActivate);
		}

		private void welcomePageToolStripMenuItem_Click(object sender, EventArgs e)
		{
			_welcomeDocument.Open(WindowOpenMethod.OnScreenActivate);
		}

		private void executeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IQueryHandler queryHandler = GetHandler<IQueryHandler>();
			if (queryHandler != null)
				queryHandler.Execute();
		}

		private void explainToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IQueryHandler queryHandler = GetHandler<IQueryHandler>();
			if (queryHandler != null)
				queryHandler.ShowPlan();
		}

		private void listMembersToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IIntelliSenseHandler intelliSenseHandler = GetHandler<IIntelliSenseHandler>();
			if (intelliSenseHandler != null)
				intelliSenseHandler.ListMembers();
		}

		private void parameterInfoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IIntelliSenseHandler intelliSenseHandler = GetHandler<IIntelliSenseHandler>();
			if (intelliSenseHandler != null)
				intelliSenseHandler.ParameterInfo();
		}

		private void completeWordToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IIntelliSenseHandler intelliSenseHandler = GetHandler<IIntelliSenseHandler>();
			if (intelliSenseHandler != null)
				intelliSenseHandler.CompleteWord();
		}

		private void newQueryToolStripButton_Click(object sender, EventArgs e)
		{
			newQueryToolStripMenuItem.PerformClick();
		}

		private void cutToolStripButton_Click(object sender, EventArgs e)
		{
			cutToolStripMenuItem.PerformClick();
		}

		private void copyToolStripButton_Click(object sender, EventArgs e)
		{
			copyToolStripMenuItem.PerformClick();
		}

		private void pasteToolStripButton_Click(object sender, EventArgs e)
		{
			pasteToolStripMenuItem.PerformClick();
		}

		private void undoToolStripButton_Click(object sender, EventArgs e)
		{
			undoToolStripMenuItem.PerformClick();
		}

		private void redoToolStripButton_Click(object sender, EventArgs e)
		{
			redoToolStripMenuItem.PerformClick();
		}

		private void propertiesToolStripButton_Click(object sender, EventArgs e)
		{
			propertiesDockableWindow.Open(WindowOpenMethod.OnScreenActivate);

		}

		private void documentBrowserToolStripButton_Click(object sender, EventArgs e)
		{
			documentBrowserDockableWindow.Open(WindowOpenMethod.OnScreenActivate);

		}

		private void welcomePageToolStripButton_Click(object sender, EventArgs e)
		{
			_welcomeDocument.Open(WindowOpenMethod.OnScreenActivate);
		}

		private void executeToolStripButton_Click(object sender, EventArgs e)
		{
			IQueryHandler queryHandler = GetHandler<IQueryHandler>();
			if (queryHandler != null)
				queryHandler.Execute();
		}

		private void explainQueryToolStripButton_Click(object sender, EventArgs e)
		{
			IQueryHandler queryHandler = GetHandler<IQueryHandler>();
			if (queryHandler != null)
				queryHandler.ShowPlan();
		}

		private void saveResultsToolStripButton_Click(object sender, EventArgs e)
		{
			IQueryHandler queryHandler = GetHandler<IQueryHandler>();
			if (queryHandler != null)
				queryHandler.SaveData();
		}

		private void addinsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (AddInForm addInForm = new AddInForm())
				addInForm.ShowDialog();
		}
	}
}