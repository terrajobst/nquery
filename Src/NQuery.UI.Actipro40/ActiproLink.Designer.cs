namespace NQuery.UI
{
    partial class ActiproLink
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
                Unbind();                
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ActiproLink));
            this.memberImageList = new System.Windows.Forms.ImageList(this.components);
            // 
            // memberImageList
            // 
            this.memberImageList.ImageSize = new System.Drawing.Size(16, 16);
            this.memberImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("memberImageList.ImageStream")));
            this.memberImageList.TransparentColor = System.Drawing.Color.Transparent;
        }

        #endregion

        private System.Windows.Forms.ImageList memberImageList;
    }
}
