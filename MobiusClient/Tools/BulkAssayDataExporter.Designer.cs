namespace Mobius.Tools
{
	partial class BulkAssayDataExporter
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BulkAssayDataExporter));
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.label8 = new DevExpress.XtraEditors.LabelControl();
			this.ExportFileFormat = new DevExpress.XtraEditors.ComboBoxEdit();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.IncludeTargetAndFamily = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.SelectAssays = new DevExpress.XtraEditors.SimpleButton();
			this.IncludeAssayName = new DevExpress.XtraEditors.CheckEdit();
			this.IncludeRunDate = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
			this.Summarized = new DevExpress.XtraEditors.CheckEdit();
			this.Unsummarized = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
			this.Assays = new DevExpress.XtraEditors.ListBoxControl();
			this.ExportFileName = new DevExpress.XtraEditors.TextEdit();
			this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
			this.EditExportSetup = new DevExpress.XtraEditors.SimpleButton();
			((System.ComponentModel.ISupportInitialize)(this.ExportFileFormat.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IncludeTargetAndFamily.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IncludeAssayName.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IncludeRunDate.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Summarized.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Unsummarized.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Assays)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExportFileName.Properties)).BeginInit();
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
			this.OK.Location = new System.Drawing.Point(329, 525);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(60, 23);
			this.OK.TabIndex = 72;
			this.OK.Text = "&OK";
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
			this.Cancel.Location = new System.Drawing.Point(395, 525);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(60, 23);
			this.Cancel.TabIndex = 71;
			this.Cancel.Text = "Cancel";
			// 
			// label8
			// 
			this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.label8.Appearance.BackColor = System.Drawing.Color.LightSteelBlue;
			this.label8.Appearance.Options.UseBackColor = true;
			this.label8.Appearance.Options.UseTextOptions = true;
			this.label8.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.label8.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.label8.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.label8.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.label8.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
			this.label8.Location = new System.Drawing.Point(-1, -1);
			this.label8.Name = "label8";
			this.label8.Padding = new System.Windows.Forms.Padding(6);
			this.label8.Size = new System.Drawing.Size(463, 107);
			this.label8.TabIndex = 85;
			this.label8.Text = resources.GetString("label8.Text");
			// 
			// ExportFileFormat
			// 
			this.ExportFileFormat.Location = new System.Drawing.Point(76, 284);
			this.ExportFileFormat.Name = "ExportFileFormat";
			this.ExportFileFormat.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.ExportFileFormat.Properties.Items.AddRange(new object[] {
            "CSV / Text File",
            "Excel Worksheet",
            "MS Word Table",
            "SDFile",
            "Mobius - Results displayed when query is opened",
            "Grid"});
			this.ExportFileFormat.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
			this.ExportFileFormat.Size = new System.Drawing.Size(250, 20);
			this.ExportFileFormat.TabIndex = 207;
			this.ExportFileFormat.SelectedIndexChanged += new System.EventHandler(this.ExportFileFormat_SelectedIndexChanged);
			// 
			// labelControl1
			// 
			this.labelControl1.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl1.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl1.Appearance.ForeColor = System.Drawing.SystemColors.ActiveCaption;
			this.labelControl1.Appearance.Options.UseBackColor = true;
			this.labelControl1.Appearance.Options.UseFont = true;
			this.labelControl1.Appearance.Options.UseForeColor = true;
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(9, 263);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(443, 17);
			this.labelControl1.TabIndex = 208;
			this.labelControl1.Text = "Export to";
			// 
			// IncludeTargetAndFamily
			// 
			this.IncludeTargetAndFamily.Location = new System.Drawing.Point(17, 491);
			this.IncludeTargetAndFamily.Name = "IncludeTargetAndFamily";
			this.IncludeTargetAndFamily.Properties.Caption = "Target and target family names where available";
			this.IncludeTargetAndFamily.Size = new System.Drawing.Size(300, 19);
			this.IncludeTargetAndFamily.TabIndex = 209;
			// 
			// labelControl2
			// 
			this.labelControl2.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl2.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl2.Appearance.ForeColor = System.Drawing.SystemColors.ActiveCaption;
			this.labelControl2.Appearance.Options.UseBackColor = true;
			this.labelControl2.Appearance.Options.UseFont = true;
			this.labelControl2.Appearance.Options.UseForeColor = true;
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl2.LineVisible = true;
			this.labelControl2.Location = new System.Drawing.Point(9, 114);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(443, 17);
			this.labelControl2.TabIndex = 210;
			this.labelControl2.Text = "Export all data for the following assays";
			// 
			// SelectAssays
			// 
			this.SelectAssays.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SelectAssays.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SelectAssays.Appearance.Options.UseFont = true;
			this.SelectAssays.Appearance.Options.UseForeColor = true;
			this.SelectAssays.Cursor = System.Windows.Forms.Cursors.Default;
			this.SelectAssays.Location = new System.Drawing.Point(395, 137);
			this.SelectAssays.Name = "SelectAssays";
			this.SelectAssays.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.SelectAssays.Size = new System.Drawing.Size(57, 21);
			this.SelectAssays.TabIndex = 212;
			this.SelectAssays.Tag = "OK";
			this.SelectAssays.Text = "Edit...";
			this.SelectAssays.Click += new System.EventHandler(this.SelectAssays_Click);
			// 
			// IncludeAssayName
			// 
			this.IncludeAssayName.EditValue = true;
			this.IncludeAssayName.Location = new System.Drawing.Point(17, 441);
			this.IncludeAssayName.Name = "IncludeAssayName";
			this.IncludeAssayName.Properties.Caption = "Assay and result type names (assay and result type codes are exported by default)" +
					"";
			this.IncludeAssayName.Size = new System.Drawing.Size(435, 19);
			this.IncludeAssayName.TabIndex = 216;
			// 
			// IncludeRunDate
			// 
			this.IncludeRunDate.EditValue = true;
			this.IncludeRunDate.Location = new System.Drawing.Point(17, 466);
			this.IncludeRunDate.Name = "IncludeRunDate";
			this.IncludeRunDate.Properties.Caption = "Experiment run date and any run comments";
			this.IncludeRunDate.Size = new System.Drawing.Size(300, 19);
			this.IncludeRunDate.TabIndex = 217;
			// 
			// labelControl4
			// 
			this.labelControl4.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl4.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl4.Appearance.ForeColor = System.Drawing.SystemColors.ActiveCaption;
			this.labelControl4.Appearance.Options.UseBackColor = true;
			this.labelControl4.Appearance.Options.UseFont = true;
			this.labelControl4.Appearance.Options.UseForeColor = true;
			this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl4.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl4.LineVisible = true;
			this.labelControl4.Location = new System.Drawing.Point(9, 420);
			this.labelControl4.Name = "labelControl4";
			this.labelControl4.Size = new System.Drawing.Size(443, 17);
			this.labelControl4.TabIndex = 218;
			this.labelControl4.Text = "Optional data to include in the export";
			// 
			// Summarized
			// 
			this.Summarized.Location = new System.Drawing.Point(17, 390);
			this.Summarized.Name = "Summarized";
			this.Summarized.Properties.Caption = "Summarized data (where available)";
			this.Summarized.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.Summarized.Properties.RadioGroupIndex = 1;
			this.Summarized.Size = new System.Drawing.Size(207, 19);
			this.Summarized.TabIndex = 221;
			this.Summarized.TabStop = false;
			this.Summarized.Tag = " ";
			// 
			// Unsummarized
			// 
			this.Unsummarized.EditValue = true;
			this.Unsummarized.Location = new System.Drawing.Point(17, 367);
			this.Unsummarized.Name = "Unsummarized";
			this.Unsummarized.Properties.Caption = "Unsummarized data";
			this.Unsummarized.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.Unsummarized.Properties.RadioGroupIndex = 1;
			this.Unsummarized.Size = new System.Drawing.Size(150, 19);
			this.Unsummarized.TabIndex = 220;
			this.Unsummarized.Tag = " ";
			// 
			// labelControl3
			// 
			this.labelControl3.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl3.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl3.Appearance.ForeColor = System.Drawing.SystemColors.ActiveCaption;
			this.labelControl3.Appearance.Options.UseBackColor = true;
			this.labelControl3.Appearance.Options.UseFont = true;
			this.labelControl3.Appearance.Options.UseForeColor = true;
			this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl3.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl3.LineVisible = true;
			this.labelControl3.Location = new System.Drawing.Point(9, 346);
			this.labelControl3.Name = "labelControl3";
			this.labelControl3.Size = new System.Drawing.Size(443, 17);
			this.labelControl3.TabIndex = 219;
			this.labelControl3.Text = "Summarization level";
			// 
			// Assays
			// 
			this.Assays.Location = new System.Drawing.Point(19, 137);
			this.Assays.Name = "Assays";
			this.Assays.Size = new System.Drawing.Size(370, 118);
			this.Assays.TabIndex = 222;
			this.Assays.Click += new System.EventHandler(this.Assays_Click);
			// 
			// ExportFileName
			// 
			this.ExportFileName.Location = new System.Drawing.Point(76, 310);
			this.ExportFileName.Name = "ExportFileName";
			this.ExportFileName.Size = new System.Drawing.Size(250, 20);
			this.ExportFileName.TabIndex = 223;
			this.ExportFileName.Click += new System.EventHandler(this.ExportFileName_Click);
			// 
			// labelControl5
			// 
			this.labelControl5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl5.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl5.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl5.Appearance.ForeColor = System.Drawing.SystemColors.ActiveCaption;
			this.labelControl5.Appearance.Options.UseBackColor = true;
			this.labelControl5.Appearance.Options.UseFont = true;
			this.labelControl5.Appearance.Options.UseForeColor = true;
			this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl5.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl5.LineVisible = true;
			this.labelControl5.Location = new System.Drawing.Point(-1, 512);
			this.labelControl5.Name = "labelControl5";
			this.labelControl5.Size = new System.Drawing.Size(463, 10);
			this.labelControl5.TabIndex = 224;
			// 
			// labelControl6
			// 
			this.labelControl6.Location = new System.Drawing.Point(21, 286);
			this.labelControl6.Name = "labelControl6";
			this.labelControl6.Size = new System.Drawing.Size(38, 13);
			this.labelControl6.TabIndex = 225;
			this.labelControl6.Text = "Format:";
			// 
			// labelControl7
			// 
			this.labelControl7.Location = new System.Drawing.Point(21, 313);
			this.labelControl7.Name = "labelControl7";
			this.labelControl7.Size = new System.Drawing.Size(49, 13);
			this.labelControl7.TabIndex = 226;
			this.labelControl7.Text = "File name:";
			// 
			// EditExportSetup
			// 
			this.EditExportSetup.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EditExportSetup.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.EditExportSetup.Appearance.Options.UseFont = true;
			this.EditExportSetup.Appearance.Options.UseForeColor = true;
			this.EditExportSetup.Cursor = System.Windows.Forms.Cursors.Default;
			this.EditExportSetup.Location = new System.Drawing.Point(332, 282);
			this.EditExportSetup.Name = "EditExportSetup";
			this.EditExportSetup.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.EditExportSetup.Size = new System.Drawing.Size(57, 48);
			this.EditExportSetup.TabIndex = 227;
			this.EditExportSetup.Text = "Edit...";
			this.EditExportSetup.Click += new System.EventHandler(this.EditExportSetup_Click);
			// 
			// BulkAssayDataExporter
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(460, 553);
			this.Controls.Add(this.EditExportSetup);
			this.Controls.Add(this.labelControl7);
			this.Controls.Add(this.labelControl6);
			this.Controls.Add(this.ExportFileName);
			this.Controls.Add(this.Assays);
			this.Controls.Add(this.Summarized);
			this.Controls.Add(this.Unsummarized);
			this.Controls.Add(this.labelControl3);
			this.Controls.Add(this.labelControl4);
			this.Controls.Add(this.IncludeRunDate);
			this.Controls.Add(this.IncludeAssayName);
			this.Controls.Add(this.SelectAssays);
			this.Controls.Add(this.labelControl2);
			this.Controls.Add(this.IncludeTargetAndFamily);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.ExportFileFormat);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.labelControl5);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "BulkAssayDataExporter";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Bulk Bioassay Data Exporter";
			((System.ComponentModel.ISupportInitialize)(this.ExportFileFormat.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IncludeTargetAndFamily.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IncludeAssayName.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IncludeRunDate.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Summarized.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Unsummarized.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Assays)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ExportFileName.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.LabelControl label8;
		private DevExpress.XtraEditors.ComboBoxEdit ExportFileFormat;
		public DevExpress.XtraEditors.LabelControl labelControl1;
		public DevExpress.XtraEditors.CheckEdit IncludeTargetAndFamily;
		public DevExpress.XtraEditors.LabelControl labelControl2;
		public DevExpress.XtraEditors.SimpleButton SelectAssays;
		public DevExpress.XtraEditors.CheckEdit IncludeAssayName;
		public DevExpress.XtraEditors.CheckEdit IncludeRunDate;
		public DevExpress.XtraEditors.LabelControl labelControl4;
		public DevExpress.XtraEditors.CheckEdit Summarized;
		public DevExpress.XtraEditors.CheckEdit Unsummarized;
		public DevExpress.XtraEditors.LabelControl labelControl3;
		private DevExpress.XtraEditors.ListBoxControl Assays;
		private DevExpress.XtraEditors.TextEdit ExportFileName;
		public DevExpress.XtraEditors.LabelControl labelControl5;
		private DevExpress.XtraEditors.LabelControl labelControl6;
		private DevExpress.XtraEditors.LabelControl labelControl7;
		public DevExpress.XtraEditors.SimpleButton EditExportSetup;
	}
}