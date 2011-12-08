namespace NQuery.Demo
{
	partial class WelcomeDocument
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
			this.richTextBox = new System.Windows.Forms.RichTextBox();
			this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// richTextBox
			// 
			this.richTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox.ContextMenuStrip = this.contextMenuStrip;
			this.richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBox.Location = new System.Drawing.Point(10, 10);
			this.richTextBox.Name = "richTextBox";
			this.richTextBox.ReadOnly = true;
			this.richTextBox.Size = new System.Drawing.Size(272, 246);
			this.richTextBox.TabIndex = 0;
			this.richTextBox.Text = "";
			this.richTextBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richTextBox_LinkClicked);
			// 
			// contextMenuStrip
			// 
			this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.toolStripMenuItem1,
            this.selectAllToolStripMenuItem});
			this.contextMenuStrip.Name = "contextMenuStrip";
			this.contextMenuStrip.Size = new System.Drawing.Size(129, 54);
			this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
			// 
			// copyToolStripMenuItem
			// 
			this.copyToolStripMenuItem.Image = global::NQuery.Demo.Properties.Resources.Copy;
			this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
			this.copyToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
			this.copyToolStripMenuItem.Text = "Copy";
			this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(125, 6);
			// 
			// selectAllToolStripMenuItem
			// 
			this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
			this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
			this.selectAllToolStripMenuItem.Text = "Select All";
			this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
			// 
			// WelcomeDocument
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.CloseAction = TD.SandDock.DockControlCloseAction.HideOnly;
			this.ContextMenuStrip = this.contextMenuStrip;
			this.Controls.Add(this.richTextBox);
			this.Name = "WelcomeDocument";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.PrimaryControl = this.richTextBox;
			this.Size = new System.Drawing.Size(292, 266);
			this.Text = "Welcome!";
			this.contextMenuStrip.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox richTextBox;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
	}
}