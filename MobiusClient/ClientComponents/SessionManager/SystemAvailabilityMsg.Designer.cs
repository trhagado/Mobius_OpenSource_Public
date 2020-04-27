namespace Mobius.ClientComponents
{
	partial class SystemAvailabilityMsg
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SystemAvailabilityMsg));
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.SysAvailMsg = new DevExpress.XtraEditors.LabelControl();
			this.ImageList = new System.Windows.Forms.ImageList(this.components);
			this.IconImage = new System.Windows.Forms.Label();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.separatorControl1 = new DevExpress.XtraEditors.SeparatorControl();
			this.DontShowAgainButton = new DevExpress.XtraEditors.SimpleButton();
			((System.ComponentModel.ISupportInitialize)(this.separatorControl1)).BeginInit();
			this.SuspendLayout();
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.Location = new System.Drawing.Point(333, 180);
			this.OK.Name = "OK";
			this.OK.Size = new System.Drawing.Size(74, 24);
			this.OK.TabIndex = 27;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// SysAvailMsg
			// 
			this.SysAvailMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SysAvailMsg.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.SysAvailMsg.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SysAvailMsg.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SysAvailMsg.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.SysAvailMsg.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.SysAvailMsg.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.SysAvailMsg.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.SysAvailMsg.Cursor = System.Windows.Forms.Cursors.Default;
			this.SysAvailMsg.Location = new System.Drawing.Point(56, 12);
			this.SysAvailMsg.Name = "SysAvailMsg";
			this.SysAvailMsg.Size = new System.Drawing.Size(331, 145);
			this.SysAvailMsg.TabIndex = 228;
			this.SysAvailMsg.Text = "Message message message message message message message message message message";
			// 
			// ImageList
			// 
			this.ImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageList.ImageStream")));
			this.ImageList.TransparentColor = System.Drawing.Color.Cyan;
			this.ImageList.Images.SetKeyName(0, "MessageBoxErrorIcon.bmp");
			this.ImageList.Images.SetKeyName(1, "MessageBoxQuestionIcon.bmp");
			this.ImageList.Images.SetKeyName(2, "warningIcon.bmp");
			this.ImageList.Images.SetKeyName(3, "MessageBoxInformationIcon.bmp");
			// 
			// IconImage
			// 
			this.IconImage.BackColor = System.Drawing.Color.Transparent;
			this.IconImage.Cursor = System.Windows.Forms.Cursors.Default;
			this.IconImage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.IconImage.ForeColor = System.Drawing.SystemColors.ControlText;
			this.IconImage.ImageIndex = 0;
			this.IconImage.ImageList = this.ImageList;
			this.IconImage.Location = new System.Drawing.Point(8, 7);
			this.IconImage.Name = "IconImage";
			this.IconImage.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.IconImage.Size = new System.Drawing.Size(38, 38);
			this.IconImage.TabIndex = 229;
			this.IconImage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(256, -9);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(0, 25);
			this.labelControl1.TabIndex = 230;
			// 
			// separatorControl1
			// 
			this.separatorControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.separatorControl1.Location = new System.Drawing.Point(-4, 164);
			this.separatorControl1.Name = "separatorControl1";
			this.separatorControl1.Size = new System.Drawing.Size(422, 20);
			this.separatorControl1.TabIndex = 231;
			// 
			// DontShowAgainButton
			// 
			this.DontShowAgainButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DontShowAgainButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.DontShowAgainButton.Location = new System.Drawing.Point(6, 180);
			this.DontShowAgainButton.Name = "DontShowAgainButton";
			this.DontShowAgainButton.Size = new System.Drawing.Size(171, 24);
			this.DontShowAgainButton.TabIndex = 233;
			this.DontShowAgainButton.Text = "Don\'t show this message again";
			this.DontShowAgainButton.Click += new System.EventHandler(this.DontShowAgainButton_Click);
			// 
			// SystemAvailabilityMsg
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(413, 210);
			this.Controls.Add(this.DontShowAgainButton);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.IconImage);
			this.Controls.Add(this.SysAvailMsg);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.separatorControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SystemAvailabilityMsg";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "System Availability Message";
			((System.ComponentModel.ISupportInitialize)(this.separatorControl1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.LabelControl SysAvailMsg;
		private System.Windows.Forms.ImageList ImageList;
		public System.Windows.Forms.Label IconImage;
		private DevExpress.XtraEditors.LabelControl labelControl1;
		private DevExpress.XtraEditors.SeparatorControl separatorControl1;
		private DevExpress.XtraEditors.SimpleButton DontShowAgainButton;
	}
}