namespace NQuery.UI
{
	partial class EvaluatableBrowser
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EvaluatableBrowser));
			this.treeView = new System.Windows.Forms.TreeView();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// treeView
			// 
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.ImageIndex = 0;
			this.treeView.ImageList = this.imageList;
			this.treeView.Location = new System.Drawing.Point(0, 0);
			this.treeView.Name = "treeView";
			this.treeView.SelectedImageIndex = 0;
			this.treeView.Size = new System.Drawing.Size(150, 150);
			this.treeView.TabIndex = 0;
			this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
			this.treeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView_ItemDrag);
			this.treeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView_MouseDown);
			// 
			// imageList
			// 
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList.Images.SetKeyName(0, "Folder");
			this.imageList.Images.SetKeyName(1, "Table");
			this.imageList.Images.SetKeyName(2, "Column");
			this.imageList.Images.SetKeyName(3, "Aggregate");
			this.imageList.Images.SetKeyName(4, "Function");
			this.imageList.Images.SetKeyName(5, "Parameter");
			this.imageList.Images.SetKeyName(6, "Relation");
			// 
			// EvaluatableBrowser
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.treeView);
			this.Name = "EvaluatableBrowser";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TreeView treeView;
		private System.Windows.Forms.ImageList imageList;
	}
}
