namespace Mobius.ClientComponents
{
	partial class DateFormatDialog
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
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.M_d_yy = new DevExpress.XtraEditors.CheckEdit();
			this.d_MMM_yy = new DevExpress.XtraEditors.CheckEdit();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.d_MMM_yyyy = new DevExpress.XtraEditors.CheckEdit();
			this.M_d_yyyy = new DevExpress.XtraEditors.CheckEdit();
			this.DateNone = new DevExpress.XtraEditors.CheckEdit();
			this.TimeNone = new DevExpress.XtraEditors.CheckEdit();
			this.H_mm_ss = new DevExpress.XtraEditors.CheckEdit();
			this.h_mm_ss_tt = new DevExpress.XtraEditors.CheckEdit();
			this.H_mm = new DevExpress.XtraEditors.CheckEdit();
			this.h_mm_tt = new DevExpress.XtraEditors.CheckEdit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.M_d_yy.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.d_MMM_yy.Properties)).BeginInit();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.d_MMM_yyyy.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.M_d_yyyy.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DateNone.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TimeNone.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.H_mm_ss.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.h_mm_ss_tt.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.H_mm.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.h_mm_tt.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(-1, 164);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(336, 10);
			this.labelControl1.TabIndex = 92;
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(200, 176);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(60, 23);
			this.OK.TabIndex = 91;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
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
			this.Cancel.Location = new System.Drawing.Point(268, 176);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(60, 23);
			this.Cancel.TabIndex = 90;
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.DateNone);
			this.groupBox1.Controls.Add(this.M_d_yyyy);
			this.groupBox1.Controls.Add(this.M_d_yy);
			this.groupBox1.Controls.Add(this.d_MMM_yy);
			this.groupBox1.Controls.Add(this.d_MMM_yyyy);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(144, 146);
			this.groupBox1.TabIndex = 88;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Date Format";
			// 
			// M_d_yy
			// 
			this.M_d_yy.Location = new System.Drawing.Point(14, 89);
			this.M_d_yy.Name = "M_d_yy";
			this.M_d_yy.Properties.Caption = "3/14/01";
			this.M_d_yy.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.M_d_yy.Properties.RadioGroupIndex = 1;
			this.M_d_yy.Size = new System.Drawing.Size(103, 19);
			this.M_d_yy.TabIndex = 1;
			this.M_d_yy.TabStop = false;
			// 
			// d_MMM_yy
			// 
			this.d_MMM_yy.Location = new System.Drawing.Point(14, 42);
			this.d_MMM_yy.Name = "d_MMM_yy";
			this.d_MMM_yy.Properties.Caption = "14-Mar-01";
			this.d_MMM_yy.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.d_MMM_yy.Properties.RadioGroupIndex = 1;
			this.d_MMM_yy.Size = new System.Drawing.Size(92, 19);
			this.d_MMM_yy.TabIndex = 2;
			this.d_MMM_yy.TabStop = false;
			this.d_MMM_yy.CheckedChanged += new System.EventHandler(this.Scientific_CheckedChanged);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.TimeNone);
			this.groupBox2.Controls.Add(this.H_mm_ss);
			this.groupBox2.Controls.Add(this.H_mm);
			this.groupBox2.Controls.Add(this.h_mm_ss_tt);
			this.groupBox2.Controls.Add(this.h_mm_tt);
			this.groupBox2.Location = new System.Drawing.Point(178, 12);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(144, 146);
			this.groupBox2.TabIndex = 89;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Time Format";
			// 
			// d_MMM_yyyy
			// 
			this.d_MMM_yyyy.EditValue = true;
			this.d_MMM_yyyy.Location = new System.Drawing.Point(14, 19);
			this.d_MMM_yyyy.Name = "d_MMM_yyyy";
			this.d_MMM_yyyy.Properties.Caption = "14-Mar-2001";
			this.d_MMM_yyyy.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.d_MMM_yyyy.Properties.RadioGroupIndex = 1;
			this.d_MMM_yyyy.Size = new System.Drawing.Size(92, 19);
			this.d_MMM_yyyy.TabIndex = 4;
			// 
			// M_d_yyyy
			// 
			this.M_d_yyyy.Location = new System.Drawing.Point(14, 64);
			this.M_d_yyyy.Name = "M_d_yyyy";
			this.M_d_yyyy.Properties.Caption = "3/14/2001";
			this.M_d_yyyy.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.M_d_yyyy.Properties.RadioGroupIndex = 1;
			this.M_d_yyyy.Size = new System.Drawing.Size(103, 19);
			this.M_d_yyyy.TabIndex = 5;
			this.M_d_yyyy.TabStop = false;
			// 
			// DateNone
			// 
			this.DateNone.Location = new System.Drawing.Point(14, 114);
			this.DateNone.Name = "DateNone";
			this.DateNone.Properties.Caption = "None";
			this.DateNone.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.DateNone.Properties.RadioGroupIndex = 1;
			this.DateNone.Size = new System.Drawing.Size(103, 19);
			this.DateNone.TabIndex = 6;
			this.DateNone.TabStop = false;
			// 
			// TimeNone
			// 
			this.TimeNone.Location = new System.Drawing.Point(15, 114);
			this.TimeNone.Name = "TimeNone";
			this.TimeNone.Properties.Caption = "None";
			this.TimeNone.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.TimeNone.Properties.RadioGroupIndex = 1;
			this.TimeNone.Size = new System.Drawing.Size(103, 19);
			this.TimeNone.TabIndex = 12;
			this.TimeNone.TabStop = false;
			// 
			// H_m_s
			// 
			this.H_mm_ss.Location = new System.Drawing.Point(15, 39);
			this.H_mm_ss.Name = "H_m_s";
			this.H_mm_ss.Properties.Caption = "13:30:55";
			this.H_mm_ss.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.H_mm_ss.Properties.RadioGroupIndex = 1;
			this.H_mm_ss.Size = new System.Drawing.Size(103, 19);
			this.H_mm_ss.TabIndex = 11;
			this.H_mm_ss.TabStop = false;
			// 
			// h_m_s_tt
			// 
			this.h_mm_ss_tt.Location = new System.Drawing.Point(15, 89);
			this.h_mm_ss_tt.Name = "h_m_s_tt";
			this.h_mm_ss_tt.Properties.Caption = "1:30:55 PM";
			this.h_mm_ss_tt.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.h_mm_ss_tt.Properties.RadioGroupIndex = 1;
			this.h_mm_ss_tt.Size = new System.Drawing.Size(103, 19);
			this.h_mm_ss_tt.TabIndex = 7;
			this.h_mm_ss_tt.TabStop = false;
			// 
			// H_m
			// 
			this.H_mm.EditValue = true;
			this.H_mm.Location = new System.Drawing.Point(15, 17);
			this.H_mm.Name = "H_m";
			this.H_mm.Properties.Caption = "13:30";
			this.H_mm.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.H_mm.Properties.RadioGroupIndex = 1;
			this.H_mm.Size = new System.Drawing.Size(92, 19);
			this.H_mm.TabIndex = 8;
			// 
			// h_m_tt
			// 
			this.h_mm_tt.Location = new System.Drawing.Point(15, 64);
			this.h_mm_tt.Name = "h_m_tt";
			this.h_mm_tt.Properties.Caption = "1:30 PM";
			this.h_mm_tt.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.h_mm_tt.Properties.RadioGroupIndex = 1;
			this.h_mm_tt.Size = new System.Drawing.Size(92, 19);
			this.h_mm_tt.TabIndex = 10;
			this.h_mm_tt.TabStop = false;
			// 
			// DateFormatDialog
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(334, 205);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.labelControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DateFormatDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Date/Time Format ";
			this.groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.M_d_yy.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.d_MMM_yy.Properties)).EndInit();
			this.groupBox2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.d_MMM_yyyy.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.M_d_yyyy.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DateNone.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TimeNone.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.H_mm_ss.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.h_mm_ss_tt.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.H_mm.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.h_mm_tt.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.LabelControl labelControl1;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public System.Windows.Forms.GroupBox groupBox1;
		public DevExpress.XtraEditors.CheckEdit M_d_yy;
		public DevExpress.XtraEditors.CheckEdit d_MMM_yy;
		public System.Windows.Forms.GroupBox groupBox2;
		public DevExpress.XtraEditors.CheckEdit M_d_yyyy;
		public DevExpress.XtraEditors.CheckEdit d_MMM_yyyy;
		public DevExpress.XtraEditors.CheckEdit DateNone;
		public DevExpress.XtraEditors.CheckEdit TimeNone;
		public DevExpress.XtraEditors.CheckEdit h_mm_tt;
		public DevExpress.XtraEditors.CheckEdit H_mm_ss;
		public DevExpress.XtraEditors.CheckEdit H_mm;
		public DevExpress.XtraEditors.CheckEdit h_mm_ss_tt;
	}
}