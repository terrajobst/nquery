namespace NQuery.Demo
{
	partial class QueryBrowser
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QueryBrowser));
			this.evaluatableBrowser = new NQuery.UI.EvaluatableBrowser();
			this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.newParameterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.deleteParameterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip = new System.Windows.Forms.ToolStrip();
			this.newParameterToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.deleteParameterToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.contextMenuStrip.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// evaluatableBrowser
			// 
			this.evaluatableBrowser.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.evaluatableBrowser.ContextMenuStrip = this.contextMenuStrip;
			this.evaluatableBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.evaluatableBrowser.Evaluatable = null;
			this.evaluatableBrowser.Location = new System.Drawing.Point(0, 25);
			this.evaluatableBrowser.Name = "evaluatableBrowser";
			this.evaluatableBrowser.Size = new System.Drawing.Size(150, 125);
			this.evaluatableBrowser.TabIndex = 0;
			this.evaluatableBrowser.DoubleClick += new System.EventHandler(this.evaluatableBrowser_DoubleClick);
			// 
			// contextMenuStrip
			// 
			this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newParameterToolStripMenuItem,
            this.toolStripMenuItem1,
            this.deleteParameterToolStripMenuItem});
			this.contextMenuStrip.Name = "contextMenuStrip1";
			this.contextMenuStrip.Size = new System.Drawing.Size(172, 76);
			// 
			// newParameterToolStripMenuItem
			// 
			this.newParameterToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newParameterToolStripMenuItem.Image")));
			this.newParameterToolStripMenuItem.Name = "newParameterToolStripMenuItem";
			this.newParameterToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
			this.newParameterToolStripMenuItem.Text = "New &Parameter...";
			this.newParameterToolStripMenuItem.Click += new System.EventHandler(this.newParameterToolStripMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(168, 6);
			// 
			// deleteParameterToolStripMenuItem
			// 
			this.deleteParameterToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("deleteParameterToolStripMenuItem.Image")));
			this.deleteParameterToolStripMenuItem.Name = "deleteParameterToolStripMenuItem";
			this.deleteParameterToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
			this.deleteParameterToolStripMenuItem.Text = "&Delete Parameter";
			this.deleteParameterToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
			// 
			// toolStrip
			// 
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newParameterToolStripButton,
            this.toolStripSeparator1,
            this.deleteParameterToolStripButton});
			this.toolStrip.Location = new System.Drawing.Point(0, 0);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = new System.Drawing.Size(150, 25);
			this.toolStrip.TabIndex = 1;
			this.toolStrip.Text = "toolStrip1";
			// 
			// newParameterToolStripButton
			// 
			this.newParameterToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.newParameterToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("newParameterToolStripButton.Image")));
			this.newParameterToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.newParameterToolStripButton.Name = "newParameterToolStripButton";
			this.newParameterToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.newParameterToolStripButton.Text = "New Parameter";
			this.newParameterToolStripButton.Click += new System.EventHandler(this.newParameterToolStripButton_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// deleteParameterToolStripButton
			// 
			this.deleteParameterToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.deleteParameterToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("deleteParameterToolStripButton.Image")));
			this.deleteParameterToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.deleteParameterToolStripButton.Name = "deleteParameterToolStripButton";
			this.deleteParameterToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.deleteParameterToolStripButton.Text = "Delete Parameter";
			this.deleteParameterToolStripButton.Click += new System.EventHandler(this.deleteParameterToolStripButton_Click);
			// 
			// QueryBrowser
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.evaluatableBrowser);
			this.Controls.Add(this.toolStrip);
			this.Name = "QueryBrowser";
			this.contextMenuStrip.ResumeLayout(false);
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private NQuery.UI.EvaluatableBrowser evaluatableBrowser;
		private System.Windows.Forms.ToolStrip toolStrip;
		private System.Windows.Forms.ToolStripButton newParameterToolStripButton;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem newParameterToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem deleteParameterToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton deleteParameterToolStripButton;
	}
}
