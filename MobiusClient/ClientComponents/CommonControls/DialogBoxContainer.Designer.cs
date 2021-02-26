namespace Mobius.ClientComponents
{
	partial class DialogBoxContainer
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DialogBoxContainer));
			this.ContentPanel = new System.Windows.Forms.Panel();
			this.HeaderPanel = new System.Windows.Forms.Panel();
			this.WindowTitle = new System.Windows.Forms.Label();
			this.MaximizeWindowButton = new System.Windows.Forms.Button();
			this.MinimizeWindowButton = new System.Windows.Forms.Button();
			this.CloseWindowButton = new System.Windows.Forms.Button();
			this.WindowIcon = new System.Windows.Forms.PictureBox();
			this.RestoreWindowButton = new System.Windows.Forms.Button();
			this.HeaderPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.WindowIcon)).BeginInit();
			this.SuspendLayout();
			// 
			// ContentPanel
			// 
			this.ContentPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ContentPanel.BackColor = System.Drawing.Color.White;
			this.ContentPanel.Location = new System.Drawing.Point(0, 30);
			this.ContentPanel.Name = "ContentPanel";
			this.ContentPanel.Size = new System.Drawing.Size(446, 240);
			this.ContentPanel.TabIndex = 3;
			// 
			// HeaderPanel
			// 
			this.HeaderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.HeaderPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.HeaderPanel.Controls.Add(this.WindowTitle);
			this.HeaderPanel.Controls.Add(this.MaximizeWindowButton);
			this.HeaderPanel.Controls.Add(this.MinimizeWindowButton);
			this.HeaderPanel.Controls.Add(this.CloseWindowButton);
			this.HeaderPanel.Controls.Add(this.WindowIcon);
			this.HeaderPanel.Controls.Add(this.RestoreWindowButton);
			this.HeaderPanel.Location = new System.Drawing.Point(0, 0);
			this.HeaderPanel.Name = "HeaderPanel";
			this.HeaderPanel.Size = new System.Drawing.Size(446, 30);
			this.HeaderPanel.TabIndex = 2;
			// 
			// WindowTitle
			// 
			this.WindowTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.WindowTitle.Location = new System.Drawing.Point(29, 8);
			this.WindowTitle.Name = "WindowTitle";
			this.WindowTitle.Size = new System.Drawing.Size(327, 18);
			this.WindowTitle.TabIndex = 4;
			this.WindowTitle.Text = "Mobius Window Title";
			// 
			// MaximizeWindowButton
			// 
			this.MaximizeWindowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.MaximizeWindowButton.BackColor = System.Drawing.Color.Transparent;
			this.MaximizeWindowButton.FlatAppearance.BorderSize = 0;
			this.MaximizeWindowButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.MaximizeWindowButton.Image = ((System.Drawing.Image)(resources.GetObject("MaximizeWindowButton.Image")));
			this.MaximizeWindowButton.Location = new System.Drawing.Point(382, 4);
			this.MaximizeWindowButton.Name = "MaximizeWindowButton";
			this.MaximizeWindowButton.Size = new System.Drawing.Size(20, 20);
			this.MaximizeWindowButton.TabIndex = 1;
			this.MaximizeWindowButton.UseVisualStyleBackColor = false;
			// 
			// MinimizeWindowButton
			// 
			this.MinimizeWindowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.MinimizeWindowButton.BackColor = System.Drawing.Color.Transparent;
			this.MinimizeWindowButton.FlatAppearance.BorderSize = 0;
			this.MinimizeWindowButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.MinimizeWindowButton.Image = ((System.Drawing.Image)(resources.GetObject("MinimizeWindowButton.Image")));
			this.MinimizeWindowButton.Location = new System.Drawing.Point(347, 5);
			this.MinimizeWindowButton.Name = "MinimizeWindowButton";
			this.MinimizeWindowButton.Size = new System.Drawing.Size(20, 20);
			this.MinimizeWindowButton.TabIndex = 0;
			this.MinimizeWindowButton.UseVisualStyleBackColor = false;
			// 
			// CloseWindowButton
			// 
			this.CloseWindowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseWindowButton.BackColor = System.Drawing.Color.Transparent;
			this.CloseWindowButton.FlatAppearance.BorderSize = 0;
			this.CloseWindowButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.CloseWindowButton.Image = ((System.Drawing.Image)(resources.GetObject("CloseWindowButton.Image")));
			this.CloseWindowButton.Location = new System.Drawing.Point(417, 4);
			this.CloseWindowButton.Name = "CloseWindowButton";
			this.CloseWindowButton.Size = new System.Drawing.Size(20, 20);
			this.CloseWindowButton.TabIndex = 3;
			this.CloseWindowButton.UseVisualStyleBackColor = false;
			// 
			// WindowIcon
			// 
			this.WindowIcon.Image = ((System.Drawing.Image)(resources.GetObject("WindowIcon.Image")));
			this.WindowIcon.Location = new System.Drawing.Point(7, 7);
			this.WindowIcon.Name = "WindowIcon";
			this.WindowIcon.Size = new System.Drawing.Size(16, 16);
			this.WindowIcon.TabIndex = 0;
			this.WindowIcon.TabStop = false;
			// 
			// RestoreWindowButton
			// 
			this.RestoreWindowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RestoreWindowButton.BackColor = System.Drawing.Color.Transparent;
			this.RestoreWindowButton.FlatAppearance.BorderSize = 0;
			this.RestoreWindowButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.RestoreWindowButton.Image = ((System.Drawing.Image)(resources.GetObject("RestoreWindowButton.Image")));
			this.RestoreWindowButton.Location = new System.Drawing.Point(382, 4);
			this.RestoreWindowButton.Name = "RestoreWindowButton";
			this.RestoreWindowButton.Size = new System.Drawing.Size(20, 20);
			this.RestoreWindowButton.TabIndex = 2;
			this.RestoreWindowButton.UseVisualStyleBackColor = false;
			this.RestoreWindowButton.Visible = false;
			// 
			// DialogBoxContainer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(446, 270);
			this.Controls.Add(this.ContentPanel);
			this.Controls.Add(this.HeaderPanel);
			this.Name = "DialogBoxContainer";
			this.Text = "DialogBoxContainer";
			this.HeaderPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.WindowIcon)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.Panel ContentPanel;
		public System.Windows.Forms.Panel HeaderPanel;
		public System.Windows.Forms.Label WindowTitle;
		public System.Windows.Forms.Button MaximizeWindowButton;
		public System.Windows.Forms.Button MinimizeWindowButton;
		public System.Windows.Forms.Button CloseWindowButton;
		public System.Windows.Forms.PictureBox WindowIcon;
		public System.Windows.Forms.Button RestoreWindowButton;
	}
}