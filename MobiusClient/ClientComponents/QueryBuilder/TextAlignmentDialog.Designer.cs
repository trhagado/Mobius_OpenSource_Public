namespace Mobius.ClientComponents
{
	partial class TextAlignmentDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextAlignmentDialog));
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.ApplyToAllColumns = new DevExpress.XtraEditors.CheckEdit();
			this.SetAsDefault = new DevExpress.XtraEditors.CheckEdit();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.CenterLabel = new DevExpress.XtraEditors.LabelControl();
			this.HaCenter = new DevExpress.XtraEditors.CheckButton();
			this.HAlignBitmaps20 = new System.Windows.Forms.ImageList(this.components);
			this.RightLabel = new DevExpress.XtraEditors.LabelControl();
			this.HaRight = new DevExpress.XtraEditors.CheckButton();
			this.LeftLabel = new DevExpress.XtraEditors.LabelControl();
			this.HaLeft = new DevExpress.XtraEditors.CheckButton();
			this.GeneralLabel = new DevExpress.XtraEditors.LabelControl();
			this.HaGeneral = new DevExpress.XtraEditors.CheckButton();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.BottomLabel = new DevExpress.XtraEditors.LabelControl();
			this.VaBottom = new DevExpress.XtraEditors.CheckButton();
			this.VAlignBitmaps20 = new System.Windows.Forms.ImageList(this.components);
			this.MiddleLabel = new DevExpress.XtraEditors.LabelControl();
			this.VaMiddle = new DevExpress.XtraEditors.CheckButton();
			this.TopLabel = new DevExpress.XtraEditors.LabelControl();
			this.VaTop = new DevExpress.XtraEditors.CheckButton();
			((System.ComponentModel.ISupportInitialize)(this.ApplyToAllColumns.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SetAsDefault.Properties)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Location = new System.Drawing.Point(0, 216);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(363, 2);
			this.groupBox2.TabIndex = 88;
			this.groupBox2.TabStop = false;
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.Location = new System.Drawing.Point(226, 224);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(60, 23);
			this.OK.TabIndex = 87;
			this.OK.Text = "OK";
			// 
			// Cancel
			// 
			this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Cancel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Cancel.Appearance.Options.UseFont = true;
			this.Cancel.Appearance.Options.UseForeColor = true;
			this.Cancel.Cursor = System.Windows.Forms.Cursors.Default;
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = new System.Drawing.Point(294, 224);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(60, 23);
			this.Cancel.TabIndex = 86;
			this.Cancel.Text = "Cancel";
			// 
			// ApplyToAllColumns
			// 
			this.ApplyToAllColumns.Location = new System.Drawing.Point(10, 161);
			this.ApplyToAllColumns.Name = "ApplyToAllColumns";
			this.ApplyToAllColumns.Properties.AutoWidth = true;
			this.ApplyToAllColumns.Properties.Caption = "Apply to all columns in this query";
			this.ApplyToAllColumns.Size = new System.Drawing.Size(177, 18);
			this.ApplyToAllColumns.TabIndex = 89;
			// 
			// SetAsDefault
			// 
			this.SetAsDefault.Location = new System.Drawing.Point(10, 186);
			this.SetAsDefault.Name = "SetAsDefault";
			this.SetAsDefault.Properties.AutoWidth = true;
			this.SetAsDefault.Properties.Caption = "Use these values as my defaults for new and modified queries";
			this.SetAsDefault.Size = new System.Drawing.Size(318, 18);
			this.SetAsDefault.TabIndex = 90;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.CenterLabel);
			this.groupBox1.Controls.Add(this.HaCenter);
			this.groupBox1.Controls.Add(this.RightLabel);
			this.groupBox1.Controls.Add(this.HaRight);
			this.groupBox1.Controls.Add(this.LeftLabel);
			this.groupBox1.Controls.Add(this.HaLeft);
			this.groupBox1.Controls.Add(this.GeneralLabel);
			this.groupBox1.Controls.Add(this.HaGeneral);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(156, 138);
			this.groupBox1.TabIndex = 92;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Horizontal alignment";
			// 
			// CenterLabel
			// 
			this.CenterLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Horizontal;
			this.CenterLabel.Location = new System.Drawing.Point(40, 80);
			this.CenterLabel.Name = "CenterLabel";
			this.CenterLabel.Size = new System.Drawing.Size(33, 13);
			this.CenterLabel.TabIndex = 9;
			this.CenterLabel.Text = "Center";
			this.CenterLabel.Click += new System.EventHandler(this.CenterLabel_Click);
			// 
			// HaCenter
			// 
			this.HaCenter.AllowAllUnchecked = true;
			this.HaCenter.GroupIndex = 1;
			this.HaCenter.ImageIndex = 4;
			this.HaCenter.ImageList = this.HAlignBitmaps20;
			this.HaCenter.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.HaCenter.Location = new System.Drawing.Point(13, 76);
			this.HaCenter.Name = "HaCenter";
			this.HaCenter.Size = new System.Drawing.Size(22, 22);
			this.HaCenter.TabIndex = 8;
			this.HaCenter.TabStop = false;
			this.HaCenter.CheckedChanged += new System.EventHandler(this.HaCenter_CheckedChanged);
			// 
			// HAlignBitmaps20
			// 
			this.HAlignBitmaps20.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("HAlignBitmaps20.ImageStream")));
			this.HAlignBitmaps20.TransparentColor = System.Drawing.Color.Cyan;
			this.HAlignBitmaps20.Images.SetKeyName(0, "AlignGeneral20.bmp");
			this.HAlignBitmaps20.Images.SetKeyName(1, "AlignGeneral20H.bmp");
			this.HAlignBitmaps20.Images.SetKeyName(2, "AlignLeft20.bmp");
			this.HAlignBitmaps20.Images.SetKeyName(3, "AlignLeft20H.bmp");
			this.HAlignBitmaps20.Images.SetKeyName(4, "AlignCenter20.bmp");
			this.HAlignBitmaps20.Images.SetKeyName(5, "AlignCenter20H.bmp");
			this.HAlignBitmaps20.Images.SetKeyName(6, "AlignRight20.bmp");
			this.HAlignBitmaps20.Images.SetKeyName(7, "AlignRight20H.bmp");
			this.HAlignBitmaps20.Images.SetKeyName(8, "AlignJustify20.bmp");
			this.HAlignBitmaps20.Images.SetKeyName(9, "AlignJustify20H.bmp");
			// 
			// RightLabel
			// 
			this.RightLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Horizontal;
			this.RightLabel.Location = new System.Drawing.Point(40, 109);
			this.RightLabel.Name = "RightLabel";
			this.RightLabel.Size = new System.Drawing.Size(25, 13);
			this.RightLabel.TabIndex = 7;
			this.RightLabel.Text = "Right";
			this.RightLabel.Click += new System.EventHandler(this.RightLabel_Click);
			// 
			// HaRight
			// 
			this.HaRight.AllowAllUnchecked = true;
			this.HaRight.GroupIndex = 1;
			this.HaRight.ImageIndex = 6;
			this.HaRight.ImageList = this.HAlignBitmaps20;
			this.HaRight.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.HaRight.Location = new System.Drawing.Point(13, 105);
			this.HaRight.Name = "HaRight";
			this.HaRight.Size = new System.Drawing.Size(22, 22);
			this.HaRight.TabIndex = 6;
			this.HaRight.TabStop = false;
			this.HaRight.CheckedChanged += new System.EventHandler(this.HaRight_CheckedChanged);
			// 
			// LeftLabel
			// 
			this.LeftLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Horizontal;
			this.LeftLabel.Location = new System.Drawing.Point(40, 52);
			this.LeftLabel.Name = "LeftLabel";
			this.LeftLabel.Size = new System.Drawing.Size(19, 13);
			this.LeftLabel.TabIndex = 3;
			this.LeftLabel.Text = "Left";
			this.LeftLabel.Click += new System.EventHandler(this.LeftLabel_Click);
			// 
			// HaLeft
			// 
			this.HaLeft.AllowAllUnchecked = true;
			this.HaLeft.GroupIndex = 1;
			this.HaLeft.ImageIndex = 2;
			this.HaLeft.ImageList = this.HAlignBitmaps20;
			this.HaLeft.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.HaLeft.Location = new System.Drawing.Point(13, 48);
			this.HaLeft.Name = "HaLeft";
			this.HaLeft.Size = new System.Drawing.Size(22, 22);
			this.HaLeft.TabIndex = 2;
			this.HaLeft.TabStop = false;
			this.HaLeft.CheckedChanged += new System.EventHandler(this.HaLeft_CheckedChanged);
			// 
			// GeneralLabel
			// 
			this.GeneralLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Horizontal;
			this.GeneralLabel.Location = new System.Drawing.Point(40, 25);
			this.GeneralLabel.Name = "GeneralLabel";
			this.GeneralLabel.Size = new System.Drawing.Size(37, 13);
			this.GeneralLabel.TabIndex = 1;
			this.GeneralLabel.Text = "General";
			this.GeneralLabel.Click += new System.EventHandler(this.GeneralLabel_Click);
			// 
			// HaGeneral
			// 
			this.HaGeneral.AllowAllUnchecked = true;
			this.HaGeneral.GroupIndex = 1;
			this.HaGeneral.ImageIndex = 0;
			this.HaGeneral.ImageList = this.HAlignBitmaps20;
			this.HaGeneral.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.HaGeneral.Location = new System.Drawing.Point(13, 21);
			this.HaGeneral.Name = "HaGeneral";
			this.HaGeneral.Size = new System.Drawing.Size(22, 22);
			this.HaGeneral.TabIndex = 0;
			this.HaGeneral.TabStop = false;
			this.HaGeneral.CheckedChanged += new System.EventHandler(this.HaGeneral_CheckedChanged);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.BottomLabel);
			this.groupBox3.Controls.Add(this.VaBottom);
			this.groupBox3.Controls.Add(this.MiddleLabel);
			this.groupBox3.Controls.Add(this.VaMiddle);
			this.groupBox3.Controls.Add(this.TopLabel);
			this.groupBox3.Controls.Add(this.VaTop);
			this.groupBox3.Location = new System.Drawing.Point(190, 12);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(156, 108);
			this.groupBox3.TabIndex = 93;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Vertical alignment";
			// 
			// BottomLabel
			// 
			this.BottomLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Horizontal;
			this.BottomLabel.Location = new System.Drawing.Point(40, 80);
			this.BottomLabel.Name = "BottomLabel";
			this.BottomLabel.Size = new System.Drawing.Size(34, 13);
			this.BottomLabel.TabIndex = 9;
			this.BottomLabel.Text = "Bottom";
			this.BottomLabel.Click += new System.EventHandler(this.BottomLabel_Click);
			// 
			// VaBottom
			// 
			this.VaBottom.AllowAllUnchecked = true;
			this.VaBottom.GroupIndex = 2;
			this.VaBottom.ImageIndex = 4;
			this.VaBottom.ImageList = this.VAlignBitmaps20;
			this.VaBottom.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.VaBottom.Location = new System.Drawing.Point(13, 76);
			this.VaBottom.Name = "VaBottom";
			this.VaBottom.Size = new System.Drawing.Size(22, 22);
			this.VaBottom.TabIndex = 8;
			this.VaBottom.TabStop = false;
			this.VaBottom.CheckedChanged += new System.EventHandler(this.VaBottom_CheckedChanged);
			// 
			// VAlignBitmaps20
			// 
			this.VAlignBitmaps20.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("VAlignBitmaps20.ImageStream")));
			this.VAlignBitmaps20.TransparentColor = System.Drawing.Color.Cyan;
			this.VAlignBitmaps20.Images.SetKeyName(0, "AlignTop20.bmp");
			this.VAlignBitmaps20.Images.SetKeyName(1, "AlignTop20H.bmp");
			this.VAlignBitmaps20.Images.SetKeyName(2, "AlignMiddle20.bmp");
			this.VAlignBitmaps20.Images.SetKeyName(3, "AlignMiddle20H.bmp");
			this.VAlignBitmaps20.Images.SetKeyName(4, "AlignBottom20.bmp");
			this.VAlignBitmaps20.Images.SetKeyName(5, "AlignBottom20H.bmp");
			// 
			// MiddleLabel
			// 
			this.MiddleLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Horizontal;
			this.MiddleLabel.Location = new System.Drawing.Point(40, 52);
			this.MiddleLabel.Name = "MiddleLabel";
			this.MiddleLabel.Size = new System.Drawing.Size(30, 13);
			this.MiddleLabel.TabIndex = 3;
			this.MiddleLabel.Text = "Middle";
			this.MiddleLabel.Click += new System.EventHandler(this.MiddleLabel_Click);
			// 
			// VaMiddle
			// 
			this.VaMiddle.AllowAllUnchecked = true;
			this.VaMiddle.GroupIndex = 2;
			this.VaMiddle.ImageIndex = 2;
			this.VaMiddle.ImageList = this.VAlignBitmaps20;
			this.VaMiddle.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.VaMiddle.Location = new System.Drawing.Point(13, 48);
			this.VaMiddle.Name = "VaMiddle";
			this.VaMiddle.Size = new System.Drawing.Size(22, 22);
			this.VaMiddle.TabIndex = 2;
			this.VaMiddle.TabStop = false;
			this.VaMiddle.CheckedChanged += new System.EventHandler(this.VaMiddle_CheckedChanged);
			// 
			// TopLabel
			// 
			this.TopLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Horizontal;
			this.TopLabel.Location = new System.Drawing.Point(40, 25);
			this.TopLabel.Name = "TopLabel";
			this.TopLabel.Size = new System.Drawing.Size(18, 13);
			this.TopLabel.TabIndex = 1;
			this.TopLabel.Text = "Top";
			this.TopLabel.Click += new System.EventHandler(this.TopLabel_Click);
			// 
			// VaTop
			// 
			this.VaTop.AllowAllUnchecked = true;
			this.VaTop.GroupIndex = 2;
			this.VaTop.ImageIndex = 0;
			this.VaTop.ImageList = this.VAlignBitmaps20;
			this.VaTop.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.VaTop.Location = new System.Drawing.Point(13, 21);
			this.VaTop.Name = "VaTop";
			this.VaTop.Size = new System.Drawing.Size(22, 22);
			this.VaTop.TabIndex = 0;
			this.VaTop.TabStop = false;
			this.VaTop.CheckedChanged += new System.EventHandler(this.VaTop_CheckedChanged);
			// 
			// TextAlignmentDialog
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(360, 253);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.SetAsDefault);
			this.Controls.Add(this.ApplyToAllColumns);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TextAlignmentDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Text Alignment";
			((System.ComponentModel.ISupportInitialize)(this.ApplyToAllColumns.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SetAsDefault.Properties)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox2;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		private System.Windows.Forms.GroupBox groupBox1;
		private DevExpress.XtraEditors.LabelControl CenterLabel;
		private DevExpress.XtraEditors.CheckButton HaCenter;
		private DevExpress.XtraEditors.LabelControl RightLabel;
		private DevExpress.XtraEditors.CheckButton HaRight;
		private DevExpress.XtraEditors.LabelControl LeftLabel;
		private DevExpress.XtraEditors.CheckButton HaLeft;
		private DevExpress.XtraEditors.LabelControl GeneralLabel;
		private DevExpress.XtraEditors.CheckButton HaGeneral;
		private System.Windows.Forms.GroupBox groupBox3;
		private DevExpress.XtraEditors.LabelControl BottomLabel;
		private DevExpress.XtraEditors.CheckButton VaBottom;
		private DevExpress.XtraEditors.LabelControl MiddleLabel;
		private DevExpress.XtraEditors.CheckButton VaMiddle;
		private DevExpress.XtraEditors.LabelControl TopLabel;
		private DevExpress.XtraEditors.CheckButton VaTop;
		public System.Windows.Forms.ImageList VAlignBitmaps20;
		public System.Windows.Forms.ImageList HAlignBitmaps20;
		public DevExpress.XtraEditors.CheckEdit ApplyToAllColumns;
		public DevExpress.XtraEditors.CheckEdit SetAsDefault;
	}
}