namespace Mobius.ClientComponents
{
	partial class MessageBoxMx2
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
			this.YesButton = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.NoButton = new DevExpress.XtraEditors.SimpleButton();
			this.NoToAllButton = new DevExpress.XtraEditors.SimpleButton();
			this.Message = new DevExpress.XtraEditors.LabelControl();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.MessageBoxImage = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageBoxImage)).BeginInit();
			this.SuspendLayout();
			// 
			// YesButton
			// 
			this.YesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.YesButton.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.YesButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.YesButton.Appearance.Options.UseFont = true;
			this.YesButton.Appearance.Options.UseForeColor = true;
			this.YesButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.YesButton.Location = new System.Drawing.Point(17, 94);
			this.YesButton.Name = "YesButton";
			this.YesButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.YesButton.Size = new System.Drawing.Size(76, 23);
			this.YesButton.TabIndex = 14;
			this.YesButton.Text = "Yes";
			this.YesButton.Click += new System.EventHandler(this.YesButton_Click);
			// 
			// Cancel
			// 
			this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Cancel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Cancel.Appearance.Options.UseFont = true;
			this.Cancel.Appearance.Options.UseForeColor = true;
			this.Cancel.Cursor = System.Windows.Forms.Cursors.Default;
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = new System.Drawing.Point(263, 94);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(76, 23);
			this.Cancel.TabIndex = 15;
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// NoButton
			// 
			this.NoButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NoButton.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NoButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.NoButton.Appearance.Options.UseFont = true;
			this.NoButton.Appearance.Options.UseForeColor = true;
			this.NoButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.NoButton.Location = new System.Drawing.Point(99, 94);
			this.NoButton.Name = "NoButton";
			this.NoButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.NoButton.Size = new System.Drawing.Size(76, 23);
			this.NoButton.TabIndex = 18;
			this.NoButton.Text = "No";
			this.NoButton.Click += new System.EventHandler(this.NoButton_Click);
			// 
			// NoToAllButton
			// 
			this.NoToAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NoToAllButton.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NoToAllButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.NoToAllButton.Appearance.Options.UseFont = true;
			this.NoToAllButton.Appearance.Options.UseForeColor = true;
			this.NoToAllButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.NoToAllButton.Location = new System.Drawing.Point(99, 94);
			this.NoToAllButton.Name = "NoToAllButton";
			this.NoToAllButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.NoToAllButton.Size = new System.Drawing.Size(76, 23);
			this.NoToAllButton.TabIndex = 19;
			this.NoToAllButton.Text = "No to All";
			this.NoToAllButton.Click += new System.EventHandler(this.NoToAllButton_Click);
			// 
			// Message
			// 
			this.Message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Message.Appearance.Options.UseTextOptions = true;
			this.Message.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.Message.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.Message.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.Message.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.Message.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
			this.Message.Location = new System.Drawing.Point(56, 2);
			this.Message.Name = "Message";
			this.Message.Size = new System.Drawing.Size(283, 86);
			this.Message.TabIndex = 20;
			this.Message.Text = "Message...";
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.Location = new System.Drawing.Point(181, 94);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(76, 23);
			this.OK.TabIndex = 21;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// MessageBoxImage
			// 
			this.MessageBoxImage.Image = global::Mobius.ClientComponents.Properties.Resources.MessageBoxErrorIcon32;
			this.MessageBoxImage.Location = new System.Drawing.Point(12, 28);
			this.MessageBoxImage.Name = "MessageBoxImage";
			this.MessageBoxImage.Size = new System.Drawing.Size(32, 32);
			this.MessageBoxImage.TabIndex = 23;
			this.MessageBoxImage.TabStop = false;
			// 
			// MessageBoxMx2
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(351, 122);
			this.Controls.Add(this.MessageBoxImage);
			this.Controls.Add(this.YesButton);
			this.Controls.Add(this.NoButton);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Message);
			this.Controls.Add(this.NoToAllButton);
			this.Controls.Add(this.Cancel);
			this.IconOptions.ShowIcon = false;
			this.MinimizeBox = false;
			this.Name = "MessageBoxMx2";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "MessageBoxEx";
			this.Activated += new System.EventHandler(this.MessageBoxEx_Activated);
			this.SizeChanged += new System.EventHandler(this.MessageBoxMx2_SizeChanged);
			((System.ComponentModel.ISupportInitialize)(this.MessageBoxImage)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.SimpleButton YesButton;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.SimpleButton NoButton;
		public DevExpress.XtraEditors.SimpleButton NoToAllButton;
		private DevExpress.XtraEditors.LabelControl Message;
		public DevExpress.XtraEditors.SimpleButton OK;
		private System.Windows.Forms.PictureBox MessageBoxImage;
	}
}