namespace NQuery.Demo
{
	partial class AddInForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddInForm));
			this.addinsListView = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.button1 = new System.Windows.Forms.Button();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.errorsTextBox = new System.Windows.Forms.TextBox();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// addinsListView
			// 
			this.addinsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
			this.addinsListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.addinsListView.FullRowSelect = true;
			this.addinsListView.Location = new System.Drawing.Point(0, 0);
			this.addinsListView.Name = "addinsListView";
			this.addinsListView.Size = new System.Drawing.Size(624, 181);
			this.addinsListView.SmallImageList = this.imageList;
			this.addinsListView.TabIndex = 0;
			this.addinsListView.UseCompatibleStateImageBehavior = false;
			this.addinsListView.View = System.Windows.Forms.View.Details;
			this.addinsListView.SelectedIndexChanged += new System.EventHandler(this.addinsListView_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 283;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Assembly";
			this.columnHeader2.Width = 160;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Type";
			this.columnHeader3.Width = 150;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Path";
			// 
			// imageList
			// 
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList.Images.SetKeyName(0, "Success 16x16.png");
			this.imageList.Images.SetKeyName(1, "Error 16x16.png");
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Location = new System.Drawing.Point(561, 274);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 1;
			this.button1.Text = "&Close";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = new System.Drawing.Point(12, 12);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.addinsListView);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.errorsTextBox);
			this.splitContainer1.Size = new System.Drawing.Size(624, 256);
			this.splitContainer1.SplitterDistance = 181;
			this.splitContainer1.TabIndex = 0;
			// 
			// errorsTextBox
			// 
			this.errorsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.errorsTextBox.Location = new System.Drawing.Point(0, 0);
			this.errorsTextBox.Multiline = true;
			this.errorsTextBox.Name = "errorsTextBox";
			this.errorsTextBox.ReadOnly = true;
			this.errorsTextBox.Size = new System.Drawing.Size(624, 71);
			this.errorsTextBox.TabIndex = 0;
			// 
			// AddInForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(648, 309);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.button1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(656, 343);
			this.Name = "AddInForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Add-ins";
			this.Load += new System.EventHandler(this.AddInForm_Load);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView addinsListView;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.TextBox errorsTextBox;
		private System.Windows.Forms.ImageList imageList;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
	}
}