namespace NQuery.Demo
{
	partial class EditParameterForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditParameterForm));
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.nameLabel = new System.Windows.Forms.Label();
			this.nameTextBox = new System.Windows.Forms.TextBox();
			this.typeComboBox = new System.Windows.Forms.ComboBox();
			this.typeLabel = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.valueTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(339, 151);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 6;
			this.okButton.Text = "&OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(420, 151);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 7;
			this.cancelButton.Text = "&Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// nameLabel
			// 
			this.nameLabel.AutoSize = true;
			this.nameLabel.Location = new System.Drawing.Point(12, 75);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(89, 13);
			this.nameLabel.TabIndex = 0;
			this.nameLabel.Text = "&Parameter Name:";
			// 
			// nameTextBox
			// 
			this.nameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.nameTextBox.Location = new System.Drawing.Point(157, 72);
			this.nameTextBox.Name = "nameTextBox";
			this.nameTextBox.Size = new System.Drawing.Size(338, 20);
			this.nameTextBox.TabIndex = 1;
			// 
			// typeComboBox
			// 
			this.typeComboBox.DisplayMember = "Name";
			this.typeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.typeComboBox.FormattingEnabled = true;
			this.typeComboBox.Location = new System.Drawing.Point(157, 98);
			this.typeComboBox.Name = "typeComboBox";
			this.typeComboBox.Size = new System.Drawing.Size(167, 21);
			this.typeComboBox.TabIndex = 3;
			// 
			// typeLabel
			// 
			this.typeLabel.AutoSize = true;
			this.typeLabel.Location = new System.Drawing.Point(12, 101);
			this.typeLabel.Name = "typeLabel";
			this.typeLabel.Size = new System.Drawing.Size(34, 13);
			this.typeLabel.TabIndex = 2;
			this.typeLabel.Text = "&Type:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 128);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(37, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "&Value:";
			// 
			// valueTextBox
			// 
			this.valueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.valueTextBox.Location = new System.Drawing.Point(157, 125);
			this.valueTextBox.Name = "valueTextBox";
			this.valueTextBox.Size = new System.Drawing.Size(338, 20);
			this.valueTextBox.TabIndex = 5;
			// 
			// label1
			// 
			this.label1.ForeColor = System.Drawing.SystemColors.InfoText;
			this.label1.Location = new System.Drawing.Point(49, 11);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(423, 35);
			this.label1.TabIndex = 8;
			this.label1.Text = "Please note that NQuery supports arbitrary parameter types. However, since this d" +
				"ialog allows you to enter values only a set of predefined types are supported he" +
				"re.";
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.SystemColors.Info;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.Add(this.pictureBox1);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Location = new System.Drawing.Point(12, 12);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(8);
			this.panel1.Size = new System.Drawing.Size(483, 54);
			this.panel1.TabIndex = 9;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.InitialImage = null;
			this.pictureBox1.Location = new System.Drawing.Point(11, 11);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(32, 32);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 9;
			this.pictureBox1.TabStop = false;
			// 
			// EditParameterForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(507, 186);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.typeComboBox);
			this.Controls.Add(this.valueTextBox);
			this.Controls.Add(this.nameTextBox);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.typeLabel);
			this.Controls.Add(this.nameLabel);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EditParameterForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Parameter";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditParameterForm_FormClosing);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Label nameLabel;
		private System.Windows.Forms.TextBox nameTextBox;
		private System.Windows.Forms.ComboBox typeComboBox;
		private System.Windows.Forms.Label typeLabel;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox valueTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.PictureBox pictureBox1;
	}
}