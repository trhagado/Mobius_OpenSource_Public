namespace Mobius.ClientComponents
{
	partial class QnfSplitDialog
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
			this.NValueTested = new DevExpress.XtraEditors.CheckEdit();
			this.NValue = new DevExpress.XtraEditors.CheckEdit();
			this.StdErr = new DevExpress.XtraEditors.CheckEdit();
			this.StdDev = new DevExpress.XtraEditors.CheckEdit();
			this.NumericValue = new DevExpress.XtraEditors.CheckEdit();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Qualifier = new DevExpress.XtraEditors.CheckEdit();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.TextValue = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.NValueTested.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NValue.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StdErr.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StdDev.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NumericValue.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Qualifier.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TextValue.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// NValueTested
			// 
			this.NValueTested.Cursor = System.Windows.Forms.Cursors.Default;
			this.NValueTested.Location = new System.Drawing.Point(13, 147);
			this.NValueTested.Name = "NValueTested";
			this.NValueTested.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.NValueTested.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NValueTested.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.NValueTested.Properties.Appearance.Options.UseBackColor = true;
			this.NValueTested.Properties.Appearance.Options.UseFont = true;
			this.NValueTested.Properties.Appearance.Options.UseForeColor = true;
			this.NValueTested.Properties.Caption = "Number of times tested";
			this.NValueTested.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.NValueTested.Size = new System.Drawing.Size(344, 19);
			this.NValueTested.TabIndex = 20;
			// 
			// NValue
			// 
			this.NValue.Cursor = System.Windows.Forms.Cursors.Default;
			this.NValue.Location = new System.Drawing.Point(13, 125);
			this.NValue.Name = "NValue";
			this.NValue.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.NValue.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NValue.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.NValue.Properties.Appearance.Options.UseBackColor = true;
			this.NValue.Properties.Appearance.Options.UseFont = true;
			this.NValue.Properties.Appearance.Options.UseForeColor = true;
			this.NValue.Properties.Caption = "N (number of values contributing to the mean)";
			this.NValue.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.NValue.Size = new System.Drawing.Size(344, 19);
			this.NValue.TabIndex = 19;
			// 
			// StdErr
			// 
			this.StdErr.Cursor = System.Windows.Forms.Cursors.Default;
			this.StdErr.Location = new System.Drawing.Point(13, 103);
			this.StdErr.Name = "StdErr";
			this.StdErr.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.StdErr.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StdErr.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.StdErr.Properties.Appearance.Options.UseBackColor = true;
			this.StdErr.Properties.Appearance.Options.UseFont = true;
			this.StdErr.Properties.Appearance.Options.UseForeColor = true;
			this.StdErr.Properties.Caption = "Standard error";
			this.StdErr.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.StdErr.Size = new System.Drawing.Size(344, 19);
			this.StdErr.TabIndex = 18;
			// 
			// StdDev
			// 
			this.StdDev.Cursor = System.Windows.Forms.Cursors.Default;
			this.StdDev.Location = new System.Drawing.Point(13, 81);
			this.StdDev.Name = "StdDev";
			this.StdDev.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.StdDev.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StdDev.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.StdDev.Properties.Appearance.Options.UseBackColor = true;
			this.StdDev.Properties.Appearance.Options.UseFont = true;
			this.StdDev.Properties.Appearance.Options.UseForeColor = true;
			this.StdDev.Properties.Caption = "Standard deviation";
			this.StdDev.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.StdDev.Size = new System.Drawing.Size(344, 19);
			this.StdDev.TabIndex = 17;
			// 
			// NumericValue
			// 
			this.NumericValue.Cursor = System.Windows.Forms.Cursors.Default;
			this.NumericValue.EditValue = true;
			this.NumericValue.Enabled = false;
			this.NumericValue.Location = new System.Drawing.Point(13, 59);
			this.NumericValue.Name = "NumericValue";
			this.NumericValue.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.NumericValue.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NumericValue.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.NumericValue.Properties.Appearance.Options.UseBackColor = true;
			this.NumericValue.Properties.Appearance.Options.UseFont = true;
			this.NumericValue.Properties.Appearance.Options.UseForeColor = true;
			this.NumericValue.Properties.Caption = "Numeric value (single value or mean)";
			this.NumericValue.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.NumericValue.Size = new System.Drawing.Size(344, 19);
			this.NumericValue.TabIndex = 16;
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(238, 210);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(60, 22);
			this.OK.TabIndex = 42;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// Qualifier
			// 
			this.Qualifier.Cursor = System.Windows.Forms.Cursors.Default;
			this.Qualifier.EditValue = true;
			this.Qualifier.Enabled = false;
			this.Qualifier.Location = new System.Drawing.Point(13, 37);
			this.Qualifier.Name = "Qualifier";
			this.Qualifier.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Qualifier.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Qualifier.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Qualifier.Properties.Appearance.Options.UseBackColor = true;
			this.Qualifier.Properties.Appearance.Options.UseFont = true;
			this.Qualifier.Properties.Appearance.Options.UseForeColor = true;
			this.Qualifier.Properties.Caption = "Qualifier (e.g. >)";
			this.Qualifier.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Qualifier.Size = new System.Drawing.Size(344, 19);
			this.Qualifier.TabIndex = 15;
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
			this.Cancel.Location = new System.Drawing.Point(304, 210);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(60, 22);
			this.Cancel.TabIndex = 41;
			this.Cancel.Text = "Cancel";
			// 
			// TextValue
			// 
			this.TextValue.Cursor = System.Windows.Forms.Cursors.Default;
			this.TextValue.Location = new System.Drawing.Point(13, 169);
			this.TextValue.Name = "TextValue";
			this.TextValue.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.TextValue.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TextValue.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.TextValue.Properties.Appearance.Options.UseBackColor = true;
			this.TextValue.Properties.Appearance.Options.UseFont = true;
			this.TextValue.Properties.Appearance.Options.UseForeColor = true;
			this.TextValue.Properties.Caption = "Comments";
			this.TextValue.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.TextValue.Size = new System.Drawing.Size(344, 19);
			this.TextValue.TabIndex = 21;
			// 
			// labelControl1
			// 
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(6, 9);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(359, 16);
			this.labelControl1.TabIndex = 44;
			this.labelControl1.Text = "Split into the following individual columns";
			// 
			// labelControl2
			// 
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.LineVisible = true;
			this.labelControl2.Location = new System.Drawing.Point(-1, 197);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(366, 6);
			this.labelControl2.TabIndex = 45;
			// 
			// QnfSplitDialog
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(368, 237);
			this.Controls.Add(this.labelControl2);
			this.Controls.Add(this.TextValue);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.NValueTested);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.NValue);
			this.Controls.Add(this.StdErr);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.StdDev);
			this.Controls.Add(this.NumericValue);
			this.Controls.Add(this.Qualifier);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "QnfSplitDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Splitting of \"Qualified\" Numeric Values ";
			((System.ComponentModel.ISupportInitialize)(this.NValueTested.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NValue.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StdErr.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StdDev.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NumericValue.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Qualifier.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TextValue.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.CheckEdit NValueTested;
		public DevExpress.XtraEditors.CheckEdit NValue;
		public DevExpress.XtraEditors.CheckEdit StdErr;
		public DevExpress.XtraEditors.CheckEdit StdDev;
		public DevExpress.XtraEditors.CheckEdit NumericValue;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.CheckEdit Qualifier;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.CheckEdit TextValue;
		private DevExpress.XtraEditors.LabelControl labelControl1;
		private DevExpress.XtraEditors.LabelControl labelControl2;
	}
}