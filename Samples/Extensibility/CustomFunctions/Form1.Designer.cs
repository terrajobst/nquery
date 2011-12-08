namespace NQuery.Samples.CustomFunctions
{
	partial class Form1
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
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.staticFunctionContainerButton = new System.Windows.Forms.Button();
			this.instanceFunctionContainerButton = new System.Windows.Forms.Button();
			this.delagateFunctionButton = new System.Windows.Forms.Button();
			this.dynamicFunctionsButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// dataGridView1
			// 
			this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Location = new System.Drawing.Point(12, 41);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.Size = new System.Drawing.Size(545, 213);
			this.dataGridView1.TabIndex = 0;
			// 
			// staticFunctionContainerButton
			// 
			this.staticFunctionContainerButton.Location = new System.Drawing.Point(13, 13);
			this.staticFunctionContainerButton.Name = "staticFunctionContainerButton";
			this.staticFunctionContainerButton.Size = new System.Drawing.Size(152, 23);
			this.staticFunctionContainerButton.TabIndex = 1;
			this.staticFunctionContainerButton.Text = "Static Function Container";
			this.staticFunctionContainerButton.UseVisualStyleBackColor = true;
			this.staticFunctionContainerButton.Click += new System.EventHandler(this.staticFunctionContainerButton_Click);
			// 
			// instanceFunctionContainerButton
			// 
			this.instanceFunctionContainerButton.Location = new System.Drawing.Point(171, 13);
			this.instanceFunctionContainerButton.Name = "instanceFunctionContainerButton";
			this.instanceFunctionContainerButton.Size = new System.Drawing.Size(152, 23);
			this.instanceFunctionContainerButton.TabIndex = 1;
			this.instanceFunctionContainerButton.Text = "Instance Function Container";
			this.instanceFunctionContainerButton.UseVisualStyleBackColor = true;
			this.instanceFunctionContainerButton.Click += new System.EventHandler(this.instanceFunctionContainerButton_Click);
			// 
			// delagateFunctionButton
			// 
			this.delagateFunctionButton.Location = new System.Drawing.Point(330, 13);
			this.delagateFunctionButton.Name = "delagateFunctionButton";
			this.delagateFunctionButton.Size = new System.Drawing.Size(107, 23);
			this.delagateFunctionButton.TabIndex = 2;
			this.delagateFunctionButton.Text = "Delegate Function";
			this.delagateFunctionButton.UseVisualStyleBackColor = true;
			this.delagateFunctionButton.Click += new System.EventHandler(this.delagateFunctionButton_Click);
			// 
			// dynamicFunctionsButton
			// 
			this.dynamicFunctionsButton.Location = new System.Drawing.Point(443, 13);
			this.dynamicFunctionsButton.Name = "dynamicFunctionsButton";
			this.dynamicFunctionsButton.Size = new System.Drawing.Size(114, 23);
			this.dynamicFunctionsButton.TabIndex = 3;
			this.dynamicFunctionsButton.Text = "Dynamic Functions";
			this.dynamicFunctionsButton.UseVisualStyleBackColor = true;
			this.dynamicFunctionsButton.Click += new System.EventHandler(this.dynamicFunctionsButton_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(569, 266);
			this.Controls.Add(this.dynamicFunctionsButton);
			this.Controls.Add(this.delagateFunctionButton);
			this.Controls.Add(this.instanceFunctionContainerButton);
			this.Controls.Add(this.staticFunctionContainerButton);
			this.Controls.Add(this.dataGridView1);
			this.Name = "Form1";
			this.Text = "Custom Functions Sample";
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.Button staticFunctionContainerButton;
		private System.Windows.Forms.Button instanceFunctionContainerButton;
		private System.Windows.Forms.Button delagateFunctionButton;
		private System.Windows.Forms.Button dynamicFunctionsButton;
	}
}