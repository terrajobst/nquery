using System;

namespace NQuery.Demo
{
	partial class MainForm
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
			TD.SandDock.DockContainer dockContainer1;
			this.documentBrowserDockableWindow = new TD.SandDock.DockableWindow();
			this.noBrowserLabel = new System.Windows.Forms.Label();
			this.propertiesDockableWindow = new TD.SandDock.DockableWindow();
			this.propertyGrid = new VisualStudioPropertyGrid();
			this.sandDockManager = new TD.SandDock.SandDockManager();
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newQueryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.importTestDefinitionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exportTestDefinitionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.documentBrowserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.welcomePageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.queryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.executeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.explainToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
			this.listMembersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.parameterInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.completeWordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.addinsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip = new System.Windows.Forms.ToolStrip();
			this.newQueryToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.cutToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.copyToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.pasteToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
			this.undoToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.redoToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.propertiesToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.documentBrowserToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.welcomePageToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
			this.executeToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.explainQueryToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.saveResultsToolStripButton = new System.Windows.Forms.ToolStripButton();
			dockContainer1 = new TD.SandDock.DockContainer();
			dockContainer1.SuspendLayout();
			this.documentBrowserDockableWindow.SuspendLayout();
			this.propertiesDockableWindow.SuspendLayout();
			this.menuStrip.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// dockContainer1
			// 
			dockContainer1.Controls.Add(this.documentBrowserDockableWindow);
			dockContainer1.Controls.Add(this.propertiesDockableWindow);
			dockContainer1.Dock = System.Windows.Forms.DockStyle.Right;
			dockContainer1.LayoutSystem = new TD.SandDock.SplitLayoutSystem(new System.Drawing.SizeF(250, 517), System.Windows.Forms.Orientation.Horizontal, new TD.SandDock.LayoutSystemBase[] {
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250, 243), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this.documentBrowserDockableWindow))}, this.documentBrowserDockableWindow))),
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250, 271), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this.propertiesDockableWindow))}, this.propertiesDockableWindow)))});
			dockContainer1.Location = new System.Drawing.Point(538, 49);
			dockContainer1.Manager = this.sandDockManager;
			dockContainer1.Name = "dockContainer1";
			dockContainer1.Size = new System.Drawing.Size(254, 517);
			dockContainer1.TabIndex = 24;
			// 
			// documentBrowserDockableWindow
			// 
			this.documentBrowserDockableWindow.BorderStyle = TD.SandDock.Rendering.BorderStyle.Flat;
			this.documentBrowserDockableWindow.Controls.Add(this.noBrowserLabel);
			this.documentBrowserDockableWindow.Guid = new System.Guid("b4875f5d-da25-4046-a14a-239ae8ab87b2");
			this.documentBrowserDockableWindow.Location = new System.Drawing.Point(4, 18);
			this.documentBrowserDockableWindow.Name = "documentBrowserDockableWindow";
			this.documentBrowserDockableWindow.Size = new System.Drawing.Size(250, 201);
			this.documentBrowserDockableWindow.TabImage = global::NQuery.Demo.Properties.Resources.Browser;
			this.documentBrowserDockableWindow.TabIndex = 0;
			this.documentBrowserDockableWindow.Text = "Document Browser";
			// 
			// noBrowserLabel
			// 
			this.noBrowserLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.noBrowserLabel.Location = new System.Drawing.Point(16, 21);
			this.noBrowserLabel.Name = "noBrowserLabel";
			this.noBrowserLabel.Size = new System.Drawing.Size(221, 162);
			this.noBrowserLabel.TabIndex = 0;
			this.noBrowserLabel.Text = "There are no items to show for the selected document.";
			this.noBrowserLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// propertiesDockableWindow
			// 
			this.propertiesDockableWindow.Controls.Add(this.propertyGrid);
			this.propertiesDockableWindow.Guid = new System.Guid("86a69ab1-973d-4313-b8e8-f72667a53005");
			this.propertiesDockableWindow.Location = new System.Drawing.Point(4, 265);
			this.propertiesDockableWindow.Name = "propertiesDockableWindow";
			this.propertiesDockableWindow.Size = new System.Drawing.Size(250, 228);
			this.propertiesDockableWindow.TabImage = global::NQuery.Demo.Properties.Resources.Properties;
			this.propertiesDockableWindow.TabIndex = 1;
			this.propertiesDockableWindow.Text = "Properties";
			// 
			// propertyGrid
			// 
			this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyGrid.HelpVisible = false;
			this.propertyGrid.Location = new System.Drawing.Point(0, 0);
			this.propertyGrid.Name = "propertyGrid";
			this.propertyGrid.Size = new System.Drawing.Size(250, 228);
			this.propertyGrid.TabIndex = 6;
			// 
			// sandDockManager
			// 
			this.sandDockManager.DockSystemContainer = this;
			this.sandDockManager.OwnerForm = this;
			this.sandDockManager.ActiveTabbedDocumentChanged += new System.EventHandler(this.sandDockManager_ActiveTabbedDocumentChanged);
			// 
			// menuStrip
			// 
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.queryToolStripMenuItem,
            this.toolsToolStripMenuItem});
			this.menuStrip.Location = new System.Drawing.Point(0, 0);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size(792, 24);
			this.menuStrip.TabIndex = 5;
			this.menuStrip.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newQueryToolStripMenuItem,
            this.toolStripMenuItem1,
            this.closeToolStripMenuItem,
            this.toolStripSeparator2,
            this.importTestDefinitionToolStripMenuItem,
            this.exportTestDefinitionToolStripMenuItem,
            this.toolStripMenuItem5,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// newQueryToolStripMenuItem
			// 
			this.newQueryToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.New;
			this.newQueryToolStripMenuItem.Name = "newQueryToolStripMenuItem";
			this.newQueryToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.newQueryToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			this.newQueryToolStripMenuItem.Text = "New Query...";
			this.newQueryToolStripMenuItem.Click += new System.EventHandler(this.newQueryToolStripMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(198, 6);
			// 
			// closeToolStripMenuItem
			// 
			this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
			this.closeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F4)));
			this.closeToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			this.closeToolStripMenuItem.Text = "&Close";
			this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(198, 6);
			// 
			// importTestDefinitionToolStripMenuItem
			// 
			this.importTestDefinitionToolStripMenuItem.Name = "importTestDefinitionToolStripMenuItem";
			this.importTestDefinitionToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			this.importTestDefinitionToolStripMenuItem.Text = "Import Test Definition...";
			this.importTestDefinitionToolStripMenuItem.Click += new System.EventHandler(this.importTestDefinitionToolStripMenuItem_Click);
			// 
			// exportTestDefinitionToolStripMenuItem
			// 
			this.exportTestDefinitionToolStripMenuItem.Name = "exportTestDefinitionToolStripMenuItem";
			this.exportTestDefinitionToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			this.exportTestDefinitionToolStripMenuItem.Text = "Export Test Definition...";
			this.exportTestDefinitionToolStripMenuItem.Click += new System.EventHandler(this.exportTestDefinitionToolStripMenuItem_Click);
			// 
			// toolStripMenuItem5
			// 
			this.toolStripMenuItem5.Name = "toolStripMenuItem5";
			this.toolStripMenuItem5.Size = new System.Drawing.Size(198, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripSeparator4,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripSeparator5,
            this.selectAllToolStripMenuItem});
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			this.editToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.editToolStripMenuItem.Text = "&Edit";
			// 
			// undoToolStripMenuItem
			// 
			this.undoToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.Undo;
			this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
			this.undoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
			this.undoToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
			this.undoToolStripMenuItem.Text = "&Undo";
			this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
			// 
			// redoToolStripMenuItem
			// 
			this.redoToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.Redo;
			this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
			this.redoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
			this.redoToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
			this.redoToolStripMenuItem.Text = "&Redo";
			this.redoToolStripMenuItem.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(164, 6);
			// 
			// cutToolStripMenuItem
			// 
			this.cutToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.Cut;
			this.cutToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
			this.cutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
			this.cutToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
			this.cutToolStripMenuItem.Text = "Cu&t";
			this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
			// 
			// copyToolStripMenuItem
			// 
			this.copyToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.Copy;
			this.copyToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
			this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.copyToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
			this.copyToolStripMenuItem.Text = "&Copy";
			this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
			// 
			// pasteToolStripMenuItem
			// 
			this.pasteToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.Paste;
			this.pasteToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
			this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.pasteToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
			this.pasteToolStripMenuItem.Text = "&Paste";
			this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
			// 
			// deleteToolStripMenuItem
			// 
			this.deleteToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.Delete;
			this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			this.deleteToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
			this.deleteToolStripMenuItem.Text = "&Delete";
			this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(164, 6);
			// 
			// selectAllToolStripMenuItem
			// 
			this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
			this.selectAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
			this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
			this.selectAllToolStripMenuItem.Text = "Select &All";
			this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
			// 
			// viewToolStripMenuItem
			// 
			this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.documentBrowserToolStripMenuItem,
            this.propertiesToolStripMenuItem,
            this.toolStripMenuItem2,
            this.welcomePageToolStripMenuItem});
			this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
			this.viewToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
			this.viewToolStripMenuItem.Text = "&View";
			// 
			// documentBrowserToolStripMenuItem
			// 
			this.documentBrowserToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.Browser;
			this.documentBrowserToolStripMenuItem.Name = "documentBrowserToolStripMenuItem";
			this.documentBrowserToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
			this.documentBrowserToolStripMenuItem.Text = "Document Browser";
			this.documentBrowserToolStripMenuItem.Click += new System.EventHandler(this.documentBrowserToolStripMenuItem_Click);
			// 
			// propertiesToolStripMenuItem
			// 
			this.propertiesToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.Properties;
			this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
			this.propertiesToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F4;
			this.propertiesToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
			this.propertiesToolStripMenuItem.Text = "Properties";
			this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.propertiesToolStripMenuItem_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(172, 6);
			// 
			// welcomePageToolStripMenuItem
			// 
			this.welcomePageToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.WelcomeDocument;
			this.welcomePageToolStripMenuItem.Name = "welcomePageToolStripMenuItem";
			this.welcomePageToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
			this.welcomePageToolStripMenuItem.Text = "Welcome Page";
			this.welcomePageToolStripMenuItem.Click += new System.EventHandler(this.welcomePageToolStripMenuItem_Click);
			// 
			// queryToolStripMenuItem
			// 
			this.queryToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.executeToolStripMenuItem,
            this.explainToolStripMenuItem,
            this.toolStripMenuItem7,
            this.listMembersToolStripMenuItem,
            this.parameterInfoToolStripMenuItem,
            this.completeWordToolStripMenuItem});
			this.queryToolStripMenuItem.Name = "queryToolStripMenuItem";
			this.queryToolStripMenuItem.Size = new System.Drawing.Size(49, 20);
			this.queryToolStripMenuItem.Text = "&Query";
			// 
			// executeToolStripMenuItem
			// 
			this.executeToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.Execute;
			this.executeToolStripMenuItem.Name = "executeToolStripMenuItem";
			this.executeToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
			this.executeToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
			this.executeToolStripMenuItem.Text = "&Execute";
			this.executeToolStripMenuItem.Click += new System.EventHandler(this.executeToolStripMenuItem_Click);
			// 
			// explainToolStripMenuItem
			// 
			this.explainToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.EstimatePlan;
			this.explainToolStripMenuItem.Name = "explainToolStripMenuItem";
			this.explainToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
			this.explainToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
			this.explainToolStripMenuItem.Text = "Explain";
			this.explainToolStripMenuItem.Click += new System.EventHandler(this.explainToolStripMenuItem_Click);
			// 
			// toolStripMenuItem7
			// 
			this.toolStripMenuItem7.Name = "toolStripMenuItem7";
			this.toolStripMenuItem7.Size = new System.Drawing.Size(246, 6);
			// 
			// listMembersToolStripMenuItem
			// 
			this.listMembersToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.ListMembers;
			this.listMembersToolStripMenuItem.Name = "listMembersToolStripMenuItem";
			this.listMembersToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+J";
			this.listMembersToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
			this.listMembersToolStripMenuItem.Text = "List Members";
			this.listMembersToolStripMenuItem.Click += new System.EventHandler(this.listMembersToolStripMenuItem_Click);
			// 
			// parameterInfoToolStripMenuItem
			// 
			this.parameterInfoToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.ParameterInfo;
			this.parameterInfoToolStripMenuItem.Name = "parameterInfoToolStripMenuItem";
			this.parameterInfoToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Shift+Space";
			this.parameterInfoToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
			this.parameterInfoToolStripMenuItem.Text = "Parameter Info";
			this.parameterInfoToolStripMenuItem.Click += new System.EventHandler(this.parameterInfoToolStripMenuItem_Click);
			// completeWordToolStripMenuItem
			// 
			this.completeWordToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.CompleteWord;
			this.completeWordToolStripMenuItem.Name = "completeWordToolStripMenuItem";
			this.completeWordToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Space";
			this.completeWordToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
			this.completeWordToolStripMenuItem.Text = "Complete Word";
			this.completeWordToolStripMenuItem.Click += new System.EventHandler(this.completeWordToolStripMenuItem_Click);
			// 
			// toolsToolStripMenuItem
			// 
			this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addinsToolStripMenuItem});
			this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
			this.toolsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.toolsToolStripMenuItem.Text = "&Tools";
			// 
			// addinsToolStripMenuItem
			// 
			this.addinsToolStripMenuItem.Name = "addinsToolStripMenuItem";
			this.addinsToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
			this.addinsToolStripMenuItem.Text = "Add-ins...";
			this.addinsToolStripMenuItem.Click += new System.EventHandler(this.addinsToolStripMenuItem_Click);
			// 
			// toolStrip
			// 
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newQueryToolStripButton,
            this.toolStripSeparator3,
            this.cutToolStripButton,
            this.copyToolStripButton,
            this.pasteToolStripButton,
            this.toolStripSeparator8,
            this.undoToolStripButton,
            this.redoToolStripButton,
            this.toolStripSeparator6,
            this.propertiesToolStripButton,
            this.documentBrowserToolStripButton,
            this.welcomePageToolStripButton,
            this.toolStripSeparator7,
            this.executeToolStripButton,
            this.explainQueryToolStripButton,
            this.toolStripSeparator1,
            this.saveResultsToolStripButton});
			this.toolStrip.Location = new System.Drawing.Point(0, 24);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = new System.Drawing.Size(792, 25);
			this.toolStrip.TabIndex = 23;
			this.toolStrip.Text = "toolStrip1";
			// 
			// newQueryToolStripButton
			// 
			this.newQueryToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.newQueryToolStripButton.Image = global::NQuery.Demo.Properties.Resources.New;
			this.newQueryToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.newQueryToolStripButton.Name = "newQueryToolStripButton";
			this.newQueryToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.newQueryToolStripButton.Text = "New Query";
			this.newQueryToolStripButton.Click += new System.EventHandler(this.newQueryToolStripButton_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// cutToolStripButton
			// 
			this.cutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.cutToolStripButton.Image = global::NQuery.Demo.Properties.Resources.Cut;
			this.cutToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.cutToolStripButton.Name = "cutToolStripButton";
			this.cutToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.cutToolStripButton.Text = "Cut";
			this.cutToolStripButton.Click += new System.EventHandler(this.cutToolStripButton_Click);
			// 
			// copyToolStripButton
			// 
			this.copyToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.copyToolStripButton.Image = global::NQuery.Demo.Properties.Resources.Copy;
			this.copyToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.copyToolStripButton.Name = "copyToolStripButton";
			this.copyToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.copyToolStripButton.Text = "Copy";
			this.copyToolStripButton.Click += new System.EventHandler(this.copyToolStripButton_Click);
			// 
			// pasteToolStripButton
			// 
			this.pasteToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.pasteToolStripButton.Image = global::NQuery.Demo.Properties.Resources.Paste;
			this.pasteToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.pasteToolStripButton.Name = "pasteToolStripButton";
			this.pasteToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.pasteToolStripButton.Text = "Paste";
			this.pasteToolStripButton.Click += new System.EventHandler(this.pasteToolStripButton_Click);
			// 
			// toolStripSeparator8
			// 
			this.toolStripSeparator8.Name = "toolStripSeparator8";
			this.toolStripSeparator8.Size = new System.Drawing.Size(6, 25);
			// 
			// undoToolStripButton
			// 
			this.undoToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.undoToolStripButton.Image = global::NQuery.Demo.Properties.Resources.Undo;
			this.undoToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.undoToolStripButton.Name = "undoToolStripButton";
			this.undoToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.undoToolStripButton.Text = "Undo";
			this.undoToolStripButton.Click += new System.EventHandler(this.undoToolStripButton_Click);
			// 
			// redoToolStripButton
			// 
			this.redoToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.redoToolStripButton.Image = global::NQuery.Demo.Properties.Resources.Redo;
			this.redoToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.redoToolStripButton.Name = "redoToolStripButton";
			this.redoToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.redoToolStripButton.Text = "Redo";
			this.redoToolStripButton.Click += new System.EventHandler(this.redoToolStripButton_Click);
			// 
			// toolStripSeparator6
			// 
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
			// 
			// propertiesToolStripButton
			// 
			this.propertiesToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.propertiesToolStripButton.Image = global::NQuery.Demo.Properties.Resources.Properties;
			this.propertiesToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.propertiesToolStripButton.Name = "propertiesToolStripButton";
			this.propertiesToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.propertiesToolStripButton.Text = "Properties";
			this.propertiesToolStripButton.Click += new System.EventHandler(this.propertiesToolStripButton_Click);
			// 
			// documentBrowserToolStripButton
			// 
			this.documentBrowserToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.documentBrowserToolStripButton.Image = global::NQuery.Demo.Properties.Resources.Browser;
			this.documentBrowserToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.documentBrowserToolStripButton.Name = "documentBrowserToolStripButton";
			this.documentBrowserToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.documentBrowserToolStripButton.Text = "Document Browser";
			this.documentBrowserToolStripButton.Click += new System.EventHandler(this.documentBrowserToolStripButton_Click);
			// 
			// welcomePageToolStripButton
			// 
			this.welcomePageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.welcomePageToolStripButton.Image = global::NQuery.Demo.Properties.Resources.WelcomeDocument;
			this.welcomePageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.welcomePageToolStripButton.Name = "welcomePageToolStripButton";
			this.welcomePageToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.welcomePageToolStripButton.Text = "Welcome Page";
			this.welcomePageToolStripButton.Click += new System.EventHandler(this.welcomePageToolStripButton_Click);
			// 
			// toolStripSeparator7
			// 
			this.toolStripSeparator7.Name = "toolStripSeparator7";
			this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
			// 
			// executeToolStripButton
			// 
			this.executeToolStripButton.Image = global::NQuery.Demo.Properties.Resources.Execute;
			this.executeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.executeToolStripButton.Name = "executeToolStripButton";
			this.executeToolStripButton.Size = new System.Drawing.Size(66, 22);
			this.executeToolStripButton.Text = "Execute";
			this.executeToolStripButton.Click += new System.EventHandler(this.executeToolStripButton_Click);
			// 
			// explainQueryToolStripButton
			// 
			this.explainQueryToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.explainQueryToolStripButton.Image = global::NQuery.Demo.Properties.Resources.EstimatePlan;
			this.explainQueryToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.explainQueryToolStripButton.Name = "explainQueryToolStripButton";
			this.explainQueryToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.explainQueryToolStripButton.Text = "Show execution plan";
			this.explainQueryToolStripButton.Click += new System.EventHandler(this.explainQueryToolStripButton_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// saveResultsToolStripButton
			// 
			this.saveResultsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.saveResultsToolStripButton.Image = global::NQuery.Demo.Properties.Resources.SaveGrid;
			this.saveResultsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.saveResultsToolStripButton.Name = "saveResultsToolStripButton";
			this.saveResultsToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.saveResultsToolStripButton.Text = "Save Results to File";
			this.saveResultsToolStripButton.Click += new System.EventHandler(this.saveResultsToolStripButton_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.ClientSize = new System.Drawing.Size(792, 566);
			this.Controls.Add(dockContainer1);
			this.Controls.Add(this.toolStrip);
			this.Controls.Add(this.menuStrip);
			this.MainMenuStrip = this.menuStrip;
			this.Name = "MainForm";
			this.Text = "NQuery Hosting Demo";
			dockContainer1.ResumeLayout(false);
			this.documentBrowserDockableWindow.ResumeLayout(false);
			this.propertiesDockableWindow.ResumeLayout(false);
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip;
		private VisualStudioPropertyGrid propertyGrid;
		private System.Windows.Forms.ToolStripMenuItem queryToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem executeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem explainToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem7;
		private System.Windows.Forms.ToolStripMenuItem listMembersToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem parameterInfoToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem completeWordToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem newQueryToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem importTestDefinitionToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportTestDefinitionToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
		private System.Windows.Forms.ToolStrip toolStrip;
		private System.Windows.Forms.ToolStripButton executeToolStripButton;
		private System.Windows.Forms.ToolStripButton explainQueryToolStripButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton saveResultsToolStripButton;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton newQueryToolStripButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripButton cutToolStripButton;
		private System.Windows.Forms.ToolStripButton copyToolStripButton;
		private System.Windows.Forms.ToolStripButton pasteToolStripButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
		private System.Windows.Forms.ToolStripButton undoToolStripButton;
		private System.Windows.Forms.ToolStripButton redoToolStripButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
		private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addinsToolStripMenuItem;
		private System.Windows.Forms.Label noBrowserLabel;
		private TD.SandDock.SandDockManager sandDockManager;
		private TD.SandDock.DockableWindow documentBrowserDockableWindow;
		private TD.SandDock.DockableWindow propertiesDockableWindow;
		private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem documentBrowserToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem welcomePageToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton propertiesToolStripButton;
		private System.Windows.Forms.ToolStripButton documentBrowserToolStripButton;
		private System.Windows.Forms.ToolStripButton welcomePageToolStripButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
	}
}