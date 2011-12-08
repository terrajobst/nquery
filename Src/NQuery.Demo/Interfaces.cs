using System;
using System.Windows.Forms;

using ActiproSoftware.SyntaxEditor;

namespace NQuery.Demo
{
	internal interface IHandlerRegistry
	{
		T GetHandler<T>() where T : class;
	}

	internal interface IHandlerProvider<T> where T : class
	{
		T Handler { get; }
	}

	internal interface IClipboardHandler
	{
		bool HasSelection { get; }
		bool IsReadOnly { get; }

		void Cut();
		void Copy();
		void Paste();
		void Delete();
		void SelectAll();
	}

	internal interface IUndoRedoHandler
	{
		bool CanUndo();
		bool CanRedo();

		void Undo();
		void Redo();
	}

	internal interface IQueryHandler
	{
		void Execute();
		void ShowPlan();
		void SaveData();
		void ImportTestDefinition();
		void ExportTestDefinition();
	}

	internal interface IIntelliSenseHandler
	{
		void ListMembers();
		void ParameterInfo();
		void CompleteWord();
	}

	internal interface IObjectSelectionHandler
	{
		object SelectedObject { get; set; }
	}

	internal interface IDocumentBrowserProviderHandler
	{
		Control DocumentBrowser { get; }
	}

	internal sealed class SyntaxEditorClipboardHandler : IClipboardHandler
	{
		private SyntaxEditor _syntaxEditor;

		public SyntaxEditorClipboardHandler(SyntaxEditor syntaxEditor)
		{
			_syntaxEditor = syntaxEditor;
		}

		public bool HasSelection
		{
			get { return _syntaxEditor.SelectedView.Selection.Length > 0; }
		}

		public bool IsReadOnly
		{
			get { return _syntaxEditor.Document.ReadOnly; }
		}

		public void Cut()
		{
			_syntaxEditor.SelectedView.CutToClipboard();
		}

		public void Copy()
		{
			_syntaxEditor.SelectedView.CopyToClipboard();
		}

		public void Paste()
		{
			_syntaxEditor.SelectedView.PasteFromClipboard();
		}

		public void Delete()
		{
			_syntaxEditor.SelectedView.Delete();
		}

		public void SelectAll()
		{
			_syntaxEditor.SelectedView.Selection.SelectAll();
		}
	}

	internal sealed class SyntaxEditorUndoRedoHandler : IUndoRedoHandler
	{
		private SyntaxEditor _syntaxEditor;

		public SyntaxEditorUndoRedoHandler(SyntaxEditor syntaxEditor)
		{
			_syntaxEditor = syntaxEditor;
		}

		public bool CanUndo()
		{
			return _syntaxEditor.Document.UndoRedo.CanUndo;
		}

		public bool CanRedo()
		{
			return _syntaxEditor.Document.UndoRedo.CanRedo;
		}

		public void Undo()
		{
			_syntaxEditor.Document.UndoRedo.Undo();
		}

		public void Redo()
		{
			_syntaxEditor.Document.UndoRedo.Redo();
		}
	}
}