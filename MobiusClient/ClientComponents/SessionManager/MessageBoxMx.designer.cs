namespace Mobius.ClientComponents
{
	partial class MessageBoxMx
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessageBoxMx));
			this.Button1 = new DevExpress.XtraEditors.SimpleButton();
			this.Button4 = new DevExpress.XtraEditors.SimpleButton();
			this.Button2 = new DevExpress.XtraEditors.SimpleButton();
			this.Button3 = new DevExpress.XtraEditors.SimpleButton();
			this.ImageList = new System.Windows.Forms.ImageList(this.components);
			this.label1 = new DevExpress.XtraEditors.LabelControl();
			this.IconImage = new System.Windows.Forms.Label();
			this.HtmlMessage = new System.Windows.Forms.WebBrowser();
			this.Message = new DevExpress.XtraEditors.MemoEdit();
			((System.ComponentModel.ISupportInitialize)(this.Message.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// Button1
			// 
			this.Button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Button1.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Button1.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Button1.Appearance.Options.UseFont = true;
			this.Button1.Appearance.Options.UseForeColor = true;
			this.Button1.Cursor = System.Windows.Forms.Cursors.Default;
			this.Button1.Location = new System.Drawing.Point(15, 69);
			this.Button1.Name = "Button1";
			this.Button1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Button1.Size = new System.Drawing.Size(76, 23);
			this.Button1.TabIndex = 14;
			this.Button1.Text = "&Yes";
			this.Button1.Click += new System.EventHandler(this.Button1_Click);
			// 
			// Button4
			// 
			this.Button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Button4.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Button4.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Button4.Appearance.Options.UseFont = true;
			this.Button4.Appearance.Options.UseForeColor = true;
			this.Button4.Cursor = System.Windows.Forms.Cursors.Default;
			this.Button4.Location = new System.Drawing.Point(261, 69);
			this.Button4.Name = "Button4";
			this.Button4.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Button4.Size = new System.Drawing.Size(76, 23);
			this.Button4.TabIndex = 15;
			this.Button4.Text = "Cancel";
			this.Button4.Click += new System.EventHandler(this.Button4_Click);
			// 
			// Button2
			// 
			this.Button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Button2.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Button2.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Button2.Appearance.Options.UseFont = true;
			this.Button2.Appearance.Options.UseForeColor = true;
			this.Button2.Cursor = System.Windows.Forms.Cursors.Default;
			this.Button2.Location = new System.Drawing.Point(97, 69);
			this.Button2.Name = "Button2";
			this.Button2.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Button2.Size = new System.Drawing.Size(76, 23);
			this.Button2.TabIndex = 18;
			this.Button2.Text = "&No";
			this.Button2.Click += new System.EventHandler(this.Button2_Click);
			// 
			// Button3
			// 
			this.Button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Button3.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Button3.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Button3.Appearance.Options.UseFont = true;
			this.Button3.Appearance.Options.UseForeColor = true;
			this.Button3.Cursor = System.Windows.Forms.Cursors.Default;
			this.Button3.Location = new System.Drawing.Point(179, 69);
			this.Button3.Name = "Button3";
			this.Button3.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Button3.Size = new System.Drawing.Size(76, 23);
			this.Button3.TabIndex = 19;
			this.Button3.Text = "No to All";
			this.Button3.Click += new System.EventHandler(this.Button3_Click);
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
			// label1
			// 
			this.label1.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.label1.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.label1.Cursor = System.Windows.Forms.Cursors.Default;
			this.label1.Location = new System.Drawing.Point(9, 10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(0, 13);
			this.label1.TabIndex = 21;
			// 
			// IconImage
			// 
			this.IconImage.BackColor = System.Drawing.Color.Transparent;
			this.IconImage.Cursor = System.Windows.Forms.Cursors.Default;
			this.IconImage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.IconImage.ForeColor = System.Drawing.SystemColors.ControlText;
			this.IconImage.ImageIndex = 0;
			this.IconImage.ImageList = this.ImageList;
			this.IconImage.Location = new System.Drawing.Point(9, 10);
			this.IconImage.Name = "IconImage";
			this.IconImage.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.IconImage.Size = new System.Drawing.Size(38, 38);
			this.IconImage.TabIndex = 22;
			this.IconImage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// HtmlMessage
			// 
			this.HtmlMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.HtmlMessage.Location = new System.Drawing.Point(61, 3);
			this.HtmlMessage.MinimumSize = new System.Drawing.Size(20, 20);
			this.HtmlMessage.Name = "HtmlMessage";
			this.HtmlMessage.Size = new System.Drawing.Size(285, 53);
			this.HtmlMessage.TabIndex = 23;
			// 
			// Message
			// 
			this.Message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Message.EditValue = "Line 1\r\nLine 2\r\nLine 3\r\nLine 4";
			this.Message.Location = new System.Drawing.Point(61, 7);
			this.Message.Name = "Message";
			this.Message.Properties.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(236)))), ((int)(((byte)(239)))));
			this.Message.Properties.Appearance.Options.UseBackColor = true;
			this.Message.Properties.ReadOnly = true;
			this.Message.Size = new System.Drawing.Size(285, 53);
			this.Message.TabIndex = 24;
			// 
			// MessageBoxMx
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(351, 97);
			this.Controls.Add(this.Message);
			this.Controls.Add(this.IconImage);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.Button3);
			this.Controls.Add(this.Button2);
			this.Controls.Add(this.Button1);
			this.Controls.Add(this.Button4);
			this.Controls.Add(this.HtmlMessage);
			this.MinimizeBox = false;
			this.Name = "MessageBoxMx";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "MessageBoxEx";
			this.Activated += new System.EventHandler(this.MessageBoxEx_Activated);
			this.SizeChanged += new System.EventHandler(this.MessageBoxMx_SizeChanged);
			((System.ComponentModel.ISupportInitialize)(this.Message.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public DevExpress.XtraEditors.SimpleButton Button1;
		public DevExpress.XtraEditors.SimpleButton Button4;
		public DevExpress.XtraEditors.SimpleButton Button2;
		public DevExpress.XtraEditors.SimpleButton Button3;
		private System.Windows.Forms.ImageList ImageList;
		public DevExpress.XtraEditors.LabelControl label1;
		public System.Windows.Forms.Label IconImage;
		private System.Windows.Forms.WebBrowser HtmlMessage;
		private DevExpress.XtraEditors.MemoEdit Message;
	}
}