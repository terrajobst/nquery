namespace NQuery.UI
{
	partial class ShowPlanControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShowPlanControl));
			this.executionPlanImages = new System.Windows.Forms.ImageList(this.components);
			this.showPlanTreeView = new System.Windows.Forms.TreeView();
			this.SuspendLayout();
			// 
			// executionPlanImages
			// 
			this.executionPlanImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("executionPlanImages.ImageStream")));
			this.executionPlanImages.TransparentColor = System.Drawing.Color.Transparent;
			this.executionPlanImages.Images.SetKeyName(0, "Hash Match");
			this.executionPlanImages.Images.SetKeyName(1, "Nested Loops");
			this.executionPlanImages.Images.SetKeyName(2, "Compute Scalar");
			this.executionPlanImages.Images.SetKeyName(3, "Filter");
			this.executionPlanImages.Images.SetKeyName(4, "Sort");
			this.executionPlanImages.Images.SetKeyName(5, "Stream Aggregate");
			this.executionPlanImages.Images.SetKeyName(6, "Top");
			this.executionPlanImages.Images.SetKeyName(7, "Table Scan");
			this.executionPlanImages.Images.SetKeyName(8, "Select");
			this.executionPlanImages.Images.SetKeyName(9, "Concatenation");
			this.executionPlanImages.Images.SetKeyName(10, "Constant Scan");
			this.executionPlanImages.Images.SetKeyName(11, "Warning Overlay");
			this.executionPlanImages.Images.SetKeyName(12, "Assert");
			this.executionPlanImages.Images.SetKeyName(13, "Index Spool");
			this.executionPlanImages.Images.SetKeyName(14, "Table Spool");
			// 
			// showPlanTreeView
			// 
			this.showPlanTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.showPlanTreeView.ImageIndex = 0;
			this.showPlanTreeView.ImageList = this.executionPlanImages;
			this.showPlanTreeView.Location = new System.Drawing.Point(0, 0);
			this.showPlanTreeView.Name = "showPlanTreeView";
			this.showPlanTreeView.SelectedImageIndex = 0;
			this.showPlanTreeView.Size = new System.Drawing.Size(150, 150);
			this.showPlanTreeView.TabIndex = 4;
			this.showPlanTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.showPlanTreeView_AfterSelect);
			// 
			// ShowPlanControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.showPlanTreeView);
			this.Name = "ShowPlanControl";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ImageList executionPlanImages;
		private System.Windows.Forms.TreeView showPlanTreeView;
	}
}
