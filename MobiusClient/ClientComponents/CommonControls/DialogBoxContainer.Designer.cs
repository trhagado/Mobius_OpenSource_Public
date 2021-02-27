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
			this.CloseWindowButton = new DevExpress.XtraEditors.SimpleButton();
			this.MaximizeWindowButton = new DevExpress.XtraEditors.SimpleButton();
			this.WindowTitle = new System.Windows.Forms.Label();
			this.MinimizeWindowButton = new DevExpress.XtraEditors.SimpleButton();
			this.WindowIcon = new System.Windows.Forms.PictureBox();
			this.RestoreWindowButton = new DevExpress.XtraEditors.SimpleButton();
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
			this.HeaderPanel.Controls.Add(this.CloseWindowButton);
			this.HeaderPanel.Controls.Add(this.MaximizeWindowButton);
			this.HeaderPanel.Controls.Add(this.WindowTitle);
			this.HeaderPanel.Controls.Add(this.MinimizeWindowButton);
			this.HeaderPanel.Controls.Add(this.WindowIcon);
			this.HeaderPanel.Controls.Add(this.RestoreWindowButton);
			this.HeaderPanel.Location = new System.Drawing.Point(0, 0);
			this.HeaderPanel.Name = "HeaderPanel";
			this.HeaderPanel.Size = new System.Drawing.Size(446, 30);
			this.HeaderPanel.TabIndex = 2;
			// 
			// CloseWindowButton
			// 
			this.CloseWindowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseWindowButton.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.CloseWindowButton.Appearance.BorderColor = System.Drawing.Color.Transparent;
			this.CloseWindowButton.Appearance.ForeColor = System.Drawing.Color.Transparent;
			this.CloseWindowButton.Appearance.Options.UseBackColor = true;
			this.CloseWindowButton.Appearance.Options.UseBorderColor = true;
			this.CloseWindowButton.Appearance.Options.UseForeColor = true;
			this.CloseWindowButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("CloseWindowButton.ImageOptions.Image")));
			this.CloseWindowButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.CloseWindowButton.Location = new System.Drawing.Point(417, 4);
			this.CloseWindowButton.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.UltraFlat;
			this.CloseWindowButton.LookAndFeel.UseDefaultLookAndFeel = false;
			this.CloseWindowButton.Name = "CloseWindowButton";
			this.CloseWindowButton.Size = new System.Drawing.Size(20, 20);
			this.CloseWindowButton.TabIndex = 3;
			this.CloseWindowButton.TabStop = false;
			this.CloseWindowButton.Tag = "CloseWindow";
			this.CloseWindowButton.ToolTip = "Close";
			// 
			// MaximizeWindowButton
			// 
			this.MaximizeWindowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.MaximizeWindowButton.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.MaximizeWindowButton.Appearance.BorderColor = System.Drawing.Color.Transparent;
			this.MaximizeWindowButton.Appearance.ForeColor = System.Drawing.Color.Transparent;
			this.MaximizeWindowButton.Appearance.Options.UseBackColor = true;
			this.MaximizeWindowButton.Appearance.Options.UseBorderColor = true;
			this.MaximizeWindowButton.Appearance.Options.UseForeColor = true;
			this.MaximizeWindowButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("MaximizeWindowButton.ImageOptions.Image")));
			this.MaximizeWindowButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.MaximizeWindowButton.Location = new System.Drawing.Point(382, 4);
			this.MaximizeWindowButton.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.UltraFlat;
			this.MaximizeWindowButton.LookAndFeel.UseDefaultLookAndFeel = false;
			this.MaximizeWindowButton.Name = "MaximizeWindowButton";
			this.MaximizeWindowButton.Size = new System.Drawing.Size(20, 20);
			this.MaximizeWindowButton.TabIndex = 1;
			this.MaximizeWindowButton.TabStop = false;
			this.MaximizeWindowButton.Tag = "MaximizeWindow";
			this.MaximizeWindowButton.ToolTip = "Maximize";
			// 
			// WindowTitle
			// 
			this.WindowTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.WindowTitle.Location = new System.Drawing.Point(29, 5);
			this.WindowTitle.Name = "WindowTitle";
			this.WindowTitle.Size = new System.Drawing.Size(302, 18);
			this.WindowTitle.TabIndex = 4;
			this.WindowTitle.Text = "Mobius Window Title";
			this.WindowTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// MinimizeWindowButton
			// 
			this.MinimizeWindowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.MinimizeWindowButton.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.MinimizeWindowButton.Appearance.BorderColor = System.Drawing.Color.Transparent;
			this.MinimizeWindowButton.Appearance.ForeColor = System.Drawing.Color.Transparent;
			this.MinimizeWindowButton.Appearance.Options.UseBackColor = true;
			this.MinimizeWindowButton.Appearance.Options.UseBorderColor = true;
			this.MinimizeWindowButton.Appearance.Options.UseForeColor = true;
			this.MinimizeWindowButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("MinimizeWindowButton.ImageOptions.Image")));
			this.MinimizeWindowButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.MinimizeWindowButton.Location = new System.Drawing.Point(347, 5);
			this.MinimizeWindowButton.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.UltraFlat;
			this.MinimizeWindowButton.LookAndFeel.UseDefaultLookAndFeel = false;
			this.MinimizeWindowButton.Name = "MinimizeWindowButton";
			this.MinimizeWindowButton.Size = new System.Drawing.Size(20, 20);
			this.MinimizeWindowButton.TabIndex = 0;
			this.MinimizeWindowButton.TabStop = false;
			this.MinimizeWindowButton.Tag = "MinimizeWindow";
			this.MinimizeWindowButton.ToolTip = "Minimize";
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
			this.RestoreWindowButton.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.RestoreWindowButton.Appearance.BorderColor = System.Drawing.Color.Transparent;
			this.RestoreWindowButton.Appearance.ForeColor = System.Drawing.Color.Transparent;
			this.RestoreWindowButton.Appearance.Options.UseBackColor = true;
			this.RestoreWindowButton.Appearance.Options.UseBorderColor = true;
			this.RestoreWindowButton.Appearance.Options.UseForeColor = true;
			this.RestoreWindowButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("RestoreWindowButton.ImageOptions.Image")));
			this.RestoreWindowButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.RestoreWindowButton.Location = new System.Drawing.Point(382, 4);
			this.RestoreWindowButton.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.UltraFlat;
			this.RestoreWindowButton.LookAndFeel.UseDefaultLookAndFeel = false;
			this.RestoreWindowButton.Name = "RestoreWindowButton";
			this.RestoreWindowButton.Size = new System.Drawing.Size(20, 20);
			this.RestoreWindowButton.TabIndex = 2;
			this.RestoreWindowButton.TabStop = false;
			this.RestoreWindowButton.Tag = "RestoreWindow";
			this.RestoreWindowButton.ToolTip = "Restore";
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
		public System.Windows.Forms.PictureBox WindowIcon;
		public DevExpress.XtraEditors.SimpleButton MinimizeWindowButton;
		public DevExpress.XtraEditors.SimpleButton CloseWindowButton;
		public DevExpress.XtraEditors.SimpleButton RestoreWindowButton;
		public DevExpress.XtraEditors.SimpleButton MaximizeWindowButton;
	}
}