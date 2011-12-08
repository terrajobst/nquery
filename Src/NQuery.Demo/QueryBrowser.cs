using System;
using System.Windows.Forms;

using NQuery.Runtime;

namespace NQuery.Demo
{
	internal sealed partial class QueryBrowser : UserControl
	{
		private IHandlerRegistry _handlerRegistry;
		private Evaluatable _evaluatable;

		public QueryBrowser(IHandlerRegistry handlerRegistry, Evaluatable evaluatable)
		{
			InitializeComponent();

			_handlerRegistry = handlerRegistry;
			_evaluatable = evaluatable;
			evaluatableBrowser.Evaluatable = evaluatable;
			evaluatableBrowser.SelectedItemChanged += evaluatableBrowser_SelectedItemChanged;
			Application.Idle += Application_OnIdle;
		}

		private void Application_OnIdle(object sender, EventArgs e)
		{
			UpdateMenuStates();
		}

		private void UpdateMenuStates()
		{
			deleteParameterToolStripMenuItem.Enabled = evaluatableBrowser.SelectedItem is ParameterBinding;
			deleteParameterToolStripButton.Enabled = deleteParameterToolStripMenuItem.Enabled;
		}

		private void NewParameter()
		{
			string parameterName = "Param" + (_evaluatable.Parameters.Count + 1);
			Type parameterType = typeof(string);
			string parameterValue = null;

			using (EditParameterForm dlg = new EditParameterForm(true, parameterName, parameterType, parameterValue))
			{
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					ParameterBinding newParameter = new ParameterBinding(dlg.ParameterName, dlg.ParameterType);
					newParameter.Value = dlg.ParameterValue;
					_evaluatable.Parameters.Add(newParameter);
				}
			}
		}

		private static void EditParameter(ParameterBinding parameter)
		{
			using (EditParameterForm dlg = new EditParameterForm(false, parameter.Name, parameter.DataType, parameter.Value))
			{
				if (dlg.ShowDialog() == DialogResult.OK)
					parameter.Value = dlg.ParameterValue;
			}
		}

		private void DeleteSelectedParameter()
		{
			ParameterBinding selectedParameter;

			if ((selectedParameter = evaluatableBrowser.SelectedItem as ParameterBinding) != null)
				_evaluatable.Parameters.Remove(selectedParameter);
		}

		private void evaluatableBrowser_SelectedItemChanged(object sender, EventArgs e)
		{
			IObjectSelectionHandler objectSelectionHandler = _handlerRegistry.GetHandler<IObjectSelectionHandler>();
			if (objectSelectionHandler != null)
				objectSelectionHandler.SelectedObject = evaluatableBrowser.SelectedItem;
		}

		private void evaluatableBrowser_DoubleClick(object sender, EventArgs e)
		{
			ParameterBinding selectedParameter = evaluatableBrowser.SelectedItem as ParameterBinding;
			if (selectedParameter != null)
				EditParameter(selectedParameter);
		}

		private void newParameterToolStripButton_Click(object sender, EventArgs e)
		{
			NewParameter();
		}

		private void deleteParameterToolStripButton_Click(object sender, EventArgs e)
		{
			DeleteSelectedParameter();
		}

		private void newParameterToolStripMenuItem_Click(object sender, EventArgs e)
		{
			NewParameter();
		}

		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DeleteSelectedParameter();
		}
	}
}
