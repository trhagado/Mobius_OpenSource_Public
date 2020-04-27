namespace Mobius.ClientComponents
{
	partial class CriteriaYesNo
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
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.YesCheckEdit = new DevExpress.XtraEditors.CheckEdit();
			this.None = new DevExpress.XtraEditors.CheckEdit();
			this.NoCheckEdit = new DevExpress.XtraEditors.CheckEdit();
			this.Prompt = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.YesCheckEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.None.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NoCheckEdit.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(255, 131);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(68, 22);
			this.OK.TabIndex = 89;
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
			this.Cancel.Location = new System.Drawing.Point(335, 131);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(68, 22);
			this.Cancel.TabIndex = 88;
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl1.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl1.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.labelControl1.Appearance.Options.UseBackColor = true;
			this.labelControl1.Appearance.Options.UseFont = true;
			this.labelControl1.Appearance.Options.UseForeColor = true;
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(-1, 118);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(412, 10);
			this.labelControl1.TabIndex = 90;
			// 
			// YesCheckEdit
			// 
			this.YesCheckEdit.Cursor = System.Windows.Forms.Cursors.Default;
			this.YesCheckEdit.Location = new System.Drawing.Point(21, 42);
			this.YesCheckEdit.Name = "YesCheckEdit";
			this.YesCheckEdit.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.YesCheckEdit.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.YesCheckEdit.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.YesCheckEdit.Properties.Appearance.Options.UseBackColor = true;
			this.YesCheckEdit.Properties.Appearance.Options.UseFont = true;
			this.YesCheckEdit.Properties.Appearance.Options.UseForeColor = true;
			this.YesCheckEdit.Properties.Caption = "Yes";
			this.YesCheckEdit.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.YesCheckEdit.Properties.RadioGroupIndex = 1;
			this.YesCheckEdit.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.YesCheckEdit.Size = new System.Drawing.Size(414, 19);
			this.YesCheckEdit.TabIndex = 92;
			this.YesCheckEdit.TabStop = false;
			// 
			// None
			// 
			this.None.Cursor = System.Windows.Forms.Cursors.Default;
			this.None.Location = new System.Drawing.Point(21, 92);
			this.None.Name = "None";
			this.None.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.None.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.None.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.None.Properties.Appearance.Options.UseBackColor = true;
			this.None.Properties.Appearance.Options.UseFont = true;
			this.None.Properties.Appearance.Options.UseForeColor = true;
			this.None.Properties.Caption = "No criteria";
			this.None.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.None.Properties.RadioGroupIndex = 1;
			this.None.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.None.Size = new System.Drawing.Size(182, 19);
			this.None.TabIndex = 91;
			this.None.TabStop = false;
			// 
			// NoCheckEdit
			// 
			this.NoCheckEdit.Cursor = System.Windows.Forms.Cursors.Default;
			this.NoCheckEdit.Location = new System.Drawing.Point(21, 67);
			this.NoCheckEdit.Name = "NoCheckEdit";
			this.NoCheckEdit.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.NoCheckEdit.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NoCheckEdit.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.NoCheckEdit.Properties.Appearance.Options.UseBackColor = true;
			this.NoCheckEdit.Properties.Appearance.Options.UseFont = true;
			this.NoCheckEdit.Properties.Appearance.Options.UseForeColor = true;
			this.NoCheckEdit.Properties.Caption = "No";
			this.NoCheckEdit.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.NoCheckEdit.Properties.RadioGroupIndex = 1;
			this.NoCheckEdit.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.NoCheckEdit.Size = new System.Drawing.Size(414, 19);
			this.NoCheckEdit.TabIndex = 93;
			this.NoCheckEdit.TabStop = false;
			// 
			// Prompt
			// 
			this.Prompt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Prompt.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Prompt.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Prompt.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Prompt.Appearance.Options.UseBackColor = true;
			this.Prompt.Appearance.Options.UseFont = true;
			this.Prompt.Appearance.Options.UseForeColor = true;
			this.Prompt.Appearance.Options.UseTextOptions = true;
			this.Prompt.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.Prompt.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.Prompt.Cursor = System.Windows.Forms.Cursors.Default;
			this.Prompt.Location = new System.Drawing.Point(12, 12);
			this.Prompt.Name = "Prompt";
			this.Prompt.Size = new System.Drawing.Size(391, 24);
			this.Prompt.TabIndex = 94;
			this.Prompt.Tag = "Prompt";
			this.Prompt.Text = "Prompt Prompt Prompt Prompt Prompt Prompt Prompt Prompt Prompt Prompt Prompt Prom" +
    "pt Prompt Prompt";
			// 
			// CriteriaYesNo
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(415, 156);
			this.Controls.Add(this.Prompt);
			this.Controls.Add(this.NoCheckEdit);
			this.Controls.Add(this.YesCheckEdit);
			this.Controls.Add(this.None);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.labelControl1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CriteriaYesNo";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Criteria";
			((System.ComponentModel.ISupportInitialize)(this.YesCheckEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.None.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NoCheckEdit.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.LabelControl labelControl1;
		public DevExpress.XtraEditors.CheckEdit YesCheckEdit;
		public DevExpress.XtraEditors.CheckEdit None;
		public DevExpress.XtraEditors.CheckEdit NoCheckEdit;
		public DevExpress.XtraEditors.LabelControl Prompt;
	}
}