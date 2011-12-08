
namespace NQuery.Demo
{
	partial class QueryDocument
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QueryDocument));
			ActiproSoftware.SyntaxEditor.Document document1 = new ActiproSoftware.SyntaxEditor.Document();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.errorListTabPage = new System.Windows.Forms.TabPage();
			this.errorsListView = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.errorImages = new System.Windows.Forms.ImageList(this.components);
			this.resultsTabPage = new System.Windows.Forms.TabPage();
			this.resultsDataGridView = new System.Windows.Forms.DataGridView();
			this.gridContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.copyDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.explainPlanTabPage = new System.Windows.Forms.TabPage();
			this.showPlanControl = new NQuery.UI.ShowPlanControl();
			this.planContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.loadFromFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveToFileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.copyPlanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tabImageList = new System.Windows.Forms.ImageList(this.components);
			this.saveToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.syntaxEditor = new ActiproSoftware.SyntaxEditor.SyntaxEditor();
			this.editorContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveGridDataFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.saveTestDefinitionFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.statusBarImages = new System.Windows.Forms.ImageList(this.components);
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.queryResultStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			this.editLocationToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.queryTimeStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.queryRowsStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.savePlanFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.openPlanFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.openTestDefinitionFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.tabControl.SuspendLayout();
			this.errorListTabPage.SuspendLayout();
			this.resultsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.resultsDataGridView)).BeginInit();
			this.gridContextMenuStrip.SuspendLayout();
			this.explainPlanTabPage.SuspendLayout();
			this.planContextMenuStrip.SuspendLayout();
			this.editorContextMenuStrip.SuspendLayout();
			this.statusStrip.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.errorListTabPage);
			this.tabControl.Controls.Add(this.resultsTabPage);
			this.tabControl.Controls.Add(this.explainPlanTabPage);
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.ImageList = this.tabImageList;
			this.tabControl.Location = new System.Drawing.Point(0, 0);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(790, 300);
			this.tabControl.TabIndex = 4;
			this.tabControl.TabStop = false;
			// 
			// errorListTabPage
			// 
			this.errorListTabPage.Controls.Add(this.errorsListView);
			this.errorListTabPage.ImageIndex = 0;
			this.errorListTabPage.Location = new System.Drawing.Point(4, 23);
			this.errorListTabPage.Name = "errorListTabPage";
			this.errorListTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.errorListTabPage.Size = new System.Drawing.Size(782, 273);
			this.errorListTabPage.TabIndex = 2;
			this.errorListTabPage.Text = "Error List";
			this.errorListTabPage.UseVisualStyleBackColor = true;
			// 
			// errorsListView
			// 
			this.errorsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
			this.errorsListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.errorsListView.FullRowSelect = true;
			this.errorsListView.GridLines = true;
			this.errorsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.errorsListView.Location = new System.Drawing.Point(3, 3);
			this.errorsListView.Name = "errorsListView";
			this.errorsListView.Size = new System.Drawing.Size(776, 267);
			this.errorsListView.SmallImageList = this.errorImages;
			this.errorsListView.TabIndex = 0;
			this.errorsListView.UseCompatibleStateImageBehavior = false;
			this.errorsListView.View = System.Windows.Forms.View.Details;
			this.errorsListView.DoubleClick += new System.EventHandler(this.errorListView_DoubleClick);
			this.errorsListView.Resize += new System.EventHandler(this.errorListView_Resize);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Error";
			this.columnHeader1.Width = 650;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Line";
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Column";
			// 
			// errorImages
			// 
			this.errorImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("errorImages.ImageStream")));
			this.errorImages.TransparentColor = System.Drawing.Color.Transparent;
			this.errorImages.Images.SetKeyName(0, "Error");
			// 
			// resultsTabPage
			// 
			this.resultsTabPage.Controls.Add(this.resultsDataGridView);
			this.resultsTabPage.ImageIndex = 1;
			this.resultsTabPage.Location = new System.Drawing.Point(4, 23);
			this.resultsTabPage.Name = "resultsTabPage";
			this.resultsTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.resultsTabPage.Size = new System.Drawing.Size(782, 274);
			this.resultsTabPage.TabIndex = 0;
			this.resultsTabPage.Text = "Results";
			this.resultsTabPage.UseVisualStyleBackColor = true;
			// 
			// resultsDataGridView
			// 
			this.resultsDataGridView.AllowUserToAddRows = false;
			this.resultsDataGridView.AllowUserToDeleteRows = false;
			this.resultsDataGridView.ColumnHeadersHeight = 40;
			this.resultsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.resultsDataGridView.ContextMenuStrip = this.gridContextMenuStrip;
			this.resultsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.resultsDataGridView.Location = new System.Drawing.Point(3, 3);
			this.resultsDataGridView.Name = "resultsDataGridView";
			this.resultsDataGridView.ReadOnly = true;
			this.resultsDataGridView.RowHeadersWidth = 20;
			this.resultsDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			this.resultsDataGridView.Size = new System.Drawing.Size(776, 268);
			this.resultsDataGridView.TabIndex = 0;
			// 
			// gridContextMenuStrip
			// 
			this.gridContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyDataToolStripMenuItem,
            this.saveDataToolStripMenuItem});
			this.gridContextMenuStrip.Name = "contextMenuStrip1";
			this.gridContextMenuStrip.Size = new System.Drawing.Size(135, 48);
			// 
			// copyDataToolStripMenuItem
			// 
			this.copyDataToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.Copy;
			this.copyDataToolStripMenuItem.Name = "copyDataToolStripMenuItem";
			this.copyDataToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
			this.copyDataToolStripMenuItem.Text = "Copy";
			this.copyDataToolStripMenuItem.Click += new System.EventHandler(this.copyDataToolStripMenuItem_Click);
			// 
			// saveDataToolStripMenuItem
			// 
			this.saveDataToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.SaveGrid;
			this.saveDataToolStripMenuItem.Name = "saveDataToolStripMenuItem";
			this.saveDataToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
			this.saveDataToolStripMenuItem.Text = "Save Data...";
			this.saveDataToolStripMenuItem.Click += new System.EventHandler(this.saveToFileToolStripMenuItem_Click);
			// 
			// explainPlanTabPage
			// 
			this.explainPlanTabPage.Controls.Add(this.showPlanControl);
			this.explainPlanTabPage.ImageIndex = 2;
			this.explainPlanTabPage.Location = new System.Drawing.Point(4, 23);
			this.explainPlanTabPage.Name = "explainPlanTabPage";
			this.explainPlanTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.explainPlanTabPage.Size = new System.Drawing.Size(782, 274);
			this.explainPlanTabPage.TabIndex = 1;
			this.explainPlanTabPage.Text = "Execution Plan";
			this.explainPlanTabPage.UseVisualStyleBackColor = true;
			// 
			// showPlanControl
			// 
			this.showPlanControl.ContextMenuStrip = this.planContextMenuStrip;
			this.showPlanControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.showPlanControl.Location = new System.Drawing.Point(3, 3);
			this.showPlanControl.Name = "showPlanControl";
			this.showPlanControl.ShowPlan = null;
			this.showPlanControl.Size = new System.Drawing.Size(776, 268);
			this.showPlanControl.TabIndex = 7;
			this.showPlanControl.SelectedElementChanged += new System.EventHandler<System.EventArgs>(this.showPlanControl_SelectedElementChanged);
			// 
			// planContextMenuStrip
			// 
			this.planContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadFromFileToolStripMenuItem,
            this.saveToFileToolStripMenuItem1,
            this.copyPlanToolStripMenuItem});
			this.planContextMenuStrip.Name = "planContextMenuStrip";
			this.planContextMenuStrip.Size = new System.Drawing.Size(160, 70);
			// 
			// loadFromFileToolStripMenuItem
			// 
			this.loadFromFileToolStripMenuItem.Name = "loadFromFileToolStripMenuItem";
			this.loadFromFileToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
			this.loadFromFileToolStripMenuItem.Text = "Load from File...";
			this.loadFromFileToolStripMenuItem.Click += new System.EventHandler(this.loadFromFileToolStripMenuItem_Click);
			// 
			// saveToFileToolStripMenuItem1
			// 
			this.saveToFileToolStripMenuItem1.Name = "saveToFileToolStripMenuItem1";
			this.saveToFileToolStripMenuItem1.Size = new System.Drawing.Size(159, 22);
			this.saveToFileToolStripMenuItem1.Text = "Save to File...";
			this.saveToFileToolStripMenuItem1.Click += new System.EventHandler(this.saveToFileToolStripMenuItem1_Click);
			// 
			// copyPlanToolStripMenuItem
			// 
			this.copyPlanToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.Copy;
			this.copyPlanToolStripMenuItem.Name = "copyPlanToolStripMenuItem";
			this.copyPlanToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
			this.copyPlanToolStripMenuItem.Text = "Copy";
			this.copyPlanToolStripMenuItem.Click += new System.EventHandler(this.copyPlanToolStripMenuItem_Click);
			// 
			// tabImageList
			// 
			this.tabImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("tabImageList.ImageStream")));
			this.tabImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.tabImageList.Images.SetKeyName(0, "ErrorList");
			this.tabImageList.Images.SetKeyName(1, "Results");
			this.tabImageList.Images.SetKeyName(2, "ExecutionPlan");
			// 
			// saveToFileToolStripMenuItem
			// 
			this.saveToFileToolStripMenuItem.Name = "saveToFileToolStripMenuItem";
			this.saveToFileToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
			this.saveToFileToolStripMenuItem.Text = "Save to File...";
			this.saveToFileToolStripMenuItem.Click += new System.EventHandler(this.saveToFileToolStripMenuItem_Click);
			// 
			// syntaxEditor
			// 
			this.syntaxEditor.AllowDrop = true;
			this.syntaxEditor.ContextMenuStrip = this.editorContextMenuStrip;
			this.syntaxEditor.DefaultContextMenuEnabled = false;
			this.syntaxEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			document1.Outlining.Mode = ActiproSoftware.SyntaxEditor.OutliningMode.Automatic;
			this.syntaxEditor.Document = document1;
			this.syntaxEditor.Location = new System.Drawing.Point(0, 0);
			this.syntaxEditor.Name = "syntaxEditor";
			this.syntaxEditor.Size = new System.Drawing.Size(790, 235);
			this.syntaxEditor.TabIndex = 0;
			this.syntaxEditor.SelectionChanged += new ActiproSoftware.SyntaxEditor.SelectionEventHandler(this.syntaxEditor_SelectionChanged);
			// 
			// editorContextMenuStrip
			// 
			this.editorContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripMenuItem1,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripMenuItem2,
            this.selectAllToolStripMenuItem});
			this.editorContextMenuStrip.Name = "editorContextMenuStrip";
			this.editorContextMenuStrip.Size = new System.Drawing.Size(123, 170);
			this.editorContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.editorContextMenuStrip_Opening);
			// 
			// undoToolStripMenuItem
			// 
			this.undoToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.Undo;
			this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
			this.undoToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
			this.undoToolStripMenuItem.Text = "Undo";
			this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
			// 
			// redoToolStripMenuItem
			// 
			this.redoToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.Redo;
			this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
			this.redoToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
			this.redoToolStripMenuItem.Text = "Redo";
			this.redoToolStripMenuItem.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(119, 6);
			// 
			// cutToolStripMenuItem
			// 
			this.cutToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.Cut;
			this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
			this.cutToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
			this.cutToolStripMenuItem.Text = "Cut";
			this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
			// 
			// copyToolStripMenuItem
			// 
			this.copyToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.Copy;
			this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
			this.copyToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
			this.copyToolStripMenuItem.Text = "Copy";
			this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
			// 
			// pasteToolStripMenuItem
			// 
			this.pasteToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.Paste;
			this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
			this.pasteToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
			this.pasteToolStripMenuItem.Text = "Paste";
			this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
			// 
			// deleteToolStripMenuItem
			// 
			this.deleteToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.Delete;
			this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			this.deleteToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
			this.deleteToolStripMenuItem.Text = "Delete";
			this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(119, 6);
			// 
			// selectAllToolStripMenuItem
			// 
			this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
			this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
			this.selectAllToolStripMenuItem.Text = "Select All";
			this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
			// 
			// saveGridDataFileDialog
			// 
			this.saveGridDataFileDialog.Filter = "CSV Files (*.csv)|*.csv|DataSet File (*.xml)|*.xml";
			// 
			// saveTestDefinitionFileDialog
			// 
			this.saveTestDefinitionFileDialog.DefaultExt = "xml";
			this.saveTestDefinitionFileDialog.Filter = "XML Files (*.xml)|*.xml";
			this.saveTestDefinitionFileDialog.Title = "Export Test Definition";
			// 
			// statusBarImages
			// 
			this.statusBarImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("statusBarImages.ImageStream")));
			this.statusBarImages.TransparentColor = System.Drawing.Color.Transparent;
			this.statusBarImages.Images.SetKeyName(0, "");
			this.statusBarImages.Images.SetKeyName(1, "");
			// 
			// statusStrip
			// 
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.queryResultStatusLabel,
            this.toolStripStatusLabel1,
            this.editLocationToolStripStatusLabel,
            this.queryTimeStatusLabel,
            this.queryRowsStatusLabel});
			this.statusStrip.Location = new System.Drawing.Point(1, 541);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Size = new System.Drawing.Size(790, 24);
			this.statusStrip.SizingGrip = false;
			this.statusStrip.TabIndex = 23;
			this.statusStrip.Text = "statusStrip";
			// 
			// queryResultStatusLabel
			// 
			this.queryResultStatusLabel.Image = global::NQuery.Demo.Properties.Resources.StatusBarSuccess;
			this.queryResultStatusLabel.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.queryResultStatusLabel.Name = "queryResultStatusLabel";
			this.queryResultStatusLabel.Size = new System.Drawing.Size(98, 19);
			this.queryResultStatusLabel.Text = "[Query Status]";
			this.queryResultStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripStatusLabel1
			// 
			this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			this.toolStripStatusLabel1.Size = new System.Drawing.Size(430, 19);
			this.toolStripStatusLabel1.Spring = true;
			// 
			// editLocationToolStripStatusLabel
			// 
			this.editLocationToolStripStatusLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
			this.editLocationToolStripStatusLabel.Name = "editLocationToolStripStatusLabel";
			this.editLocationToolStripStatusLabel.Size = new System.Drawing.Size(88, 19);
			this.editLocationToolStripStatusLabel.Text = "[Edit Location]";
			// 
			// queryTimeStatusLabel
			// 
			this.queryTimeStatusLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)));
			this.queryTimeStatusLabel.Name = "queryTimeStatusLabel";
			this.queryTimeStatusLabel.Size = new System.Drawing.Size(81, 19);
			this.queryTimeStatusLabel.Text = "[Query Time]";
			// 
			// queryRowsStatusLabel
			// 
			this.queryRowsStatusLabel.Name = "queryRowsStatusLabel";
			this.queryRowsStatusLabel.Size = new System.Drawing.Size(78, 19);
			this.queryRowsStatusLabel.Text = "[Query Rows]";
			this.queryRowsStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(1, 2);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.syntaxEditor);
			this.splitContainer1.Panel1MinSize = 0;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.tabControl);
			this.splitContainer1.Panel2MinSize = 0;
			this.splitContainer1.Size = new System.Drawing.Size(790, 539);
			this.splitContainer1.SplitterDistance = 235;
			this.splitContainer1.TabIndex = 24;
			// 
			// savePlanFileDialog
			// 
			this.savePlanFileDialog.DefaultExt = "xml";
			this.savePlanFileDialog.Filter = "XML Files (*.xml)|*.xml|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
			this.savePlanFileDialog.Title = "Save Execution Plan";
			// 
			// openPlanFileDialog
			// 
			this.openPlanFileDialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
			this.openPlanFileDialog.Title = "Load Execution Plan";
			// 
			// openTestDefinitionFileDialog
			// 
			this.openTestDefinitionFileDialog.Filter = "XML Files (*.xml)|*.xml";
			this.openTestDefinitionFileDialog.Title = "Import Test Definition";
			// 
			// QueryDocument
			// 
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.statusStrip);
			this.Name = "QueryDocument";
			this.Padding = new System.Windows.Forms.Padding(1, 2, 1, 1);
			this.Size = new System.Drawing.Size(792, 566);
			this.Text = "NQuery Editor";
			this.tabControl.ResumeLayout(false);
			this.errorListTabPage.ResumeLayout(false);
			this.resultsTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.resultsDataGridView)).EndInit();
			this.gridContextMenuStrip.ResumeLayout(false);
			this.explainPlanTabPage.ResumeLayout(false);
			this.planContextMenuStrip.ResumeLayout(false);
			this.editorContextMenuStrip.ResumeLayout(false);
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStripStatusLabel queryResultStatusLabel;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
		private System.Windows.Forms.ToolStripStatusLabel queryTimeStatusLabel;
		private System.Windows.Forms.ToolStripStatusLabel queryRowsStatusLabel;
		private System.Windows.Forms.TabPage explainPlanTabPage;
		private System.Windows.Forms.ToolStripMenuItem copyDataToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveDataToolStripMenuItem;
		private System.Windows.Forms.TabPage errorListTabPage;
		private System.Windows.Forms.ListView errorsListView;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ImageList errorImages;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage resultsTabPage;
		private System.Windows.Forms.DataGridView resultsDataGridView;
		private System.Windows.Forms.ContextMenuStrip gridContextMenuStrip;
		private System.Windows.Forms.ImageList tabImageList;
		private System.Windows.Forms.ImageList statusBarImages;
		private System.Windows.Forms.SaveFileDialog saveGridDataFileDialog;
		private System.Windows.Forms.SaveFileDialog saveTestDefinitionFileDialog;
		private System.Windows.Forms.ToolStripMenuItem saveToFileToolStripMenuItem;
		private ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ToolStripStatusLabel editLocationToolStripStatusLabel;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ContextMenuStrip planContextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem saveToFileToolStripMenuItem1;
		private System.Windows.Forms.SaveFileDialog savePlanFileDialog;
		private System.Windows.Forms.ToolStripMenuItem copyPlanToolStripMenuItem;
		private System.Windows.Forms.OpenFileDialog openPlanFileDialog;
		private System.Windows.Forms.ToolStripMenuItem loadFromFileToolStripMenuItem;
		private System.Windows.Forms.OpenFileDialog openTestDefinitionFileDialog;
		private NQuery.UI.ShowPlanControl showPlanControl;
		private System.Windows.Forms.ContextMenuStrip editorContextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
	}
}