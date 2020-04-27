namespace Mobius.ClientComponents
{
	partial class TargetSummaryImageMapDialog
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
			this.MinMaxSummarization = new DevExpress.XtraEditors.CheckEdit();
			this.MeanSummarization = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
			this.TargetMap = new DevExpress.XtraEditors.ComboBoxEdit();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
			this.MainResultsForAll = new DevExpress.XtraEditors.CheckEdit();
			this.ColsWithCfOnly = new DevExpress.XtraEditors.CheckEdit();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.MinMaxSummarization.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MeanSummarization.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TargetMap.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainResultsForAll.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ColsWithCfOnly.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// MinMaxSummarization
			// 
			this.MinMaxSummarization.Location = new System.Drawing.Point(13, 188);
			this.MinMaxSummarization.Name = "MinMaxSummarization";
			this.MinMaxSummarization.Properties.Caption = "Most potent (min CRC, max SP)";
			this.MinMaxSummarization.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.MinMaxSummarization.Properties.RadioGroupIndex = 3;
			this.MinMaxSummarization.Size = new System.Drawing.Size(173, 19);
			this.MinMaxSummarization.TabIndex = 259;
			this.MinMaxSummarization.TabStop = false;
			// 
			// MeanSummarization
			// 
			this.MeanSummarization.EditValue = true;
			this.MeanSummarization.Location = new System.Drawing.Point(13, 163);
			this.MeanSummarization.Name = "MeanSummarization";
			this.MeanSummarization.Properties.Caption = "Calculate means (CRC - geometric, SP - arithmetic)";
			this.MeanSummarization.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.MeanSummarization.Properties.RadioGroupIndex = 3;
			this.MeanSummarization.Size = new System.Drawing.Size(274, 19);
			this.MeanSummarization.TabIndex = 258;
			// 
			// labelControl4
			// 
			this.labelControl4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl4.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl4.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl4.Appearance.ForeColor = System.Drawing.SystemColors.ActiveCaption;
			this.labelControl4.Appearance.Options.UseBackColor = true;
			this.labelControl4.Appearance.Options.UseFont = true;
			this.labelControl4.Appearance.Options.UseForeColor = true;
			this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl4.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl4.LineVisible = true;
			this.labelControl4.Location = new System.Drawing.Point(8, 142);
			this.labelControl4.Name = "labelControl4";
			this.labelControl4.Size = new System.Drawing.Size(451, 15);
			this.labelControl4.TabIndex = 257;
			this.labelControl4.Text = "How to handle multiple activity values for a compound for the same assay/target";
			// 
			// TargetMap
			// 
			this.TargetMap.Location = new System.Drawing.Point(15, 246);
			this.TargetMap.Name = "TargetMap";
			this.TargetMap.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.TargetMap.Properties.DropDownRows = 10;
			this.TargetMap.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
			this.TargetMap.Size = new System.Drawing.Size(358, 20);
			this.TargetMap.TabIndex = 256;
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl1.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl1.Appearance.ForeColor = System.Drawing.SystemColors.ActiveCaption;
			this.labelControl1.Appearance.Options.UseBackColor = true;
			this.labelControl1.Appearance.Options.UseFont = true;
			this.labelControl1.Appearance.Options.UseForeColor = true;
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(8, 225);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(451, 15);
			this.labelControl1.TabIndex = 255;
			this.labelControl1.Text = "Dendogram or pathway diagram to map data onto";
			// 
			// labelControl3
			// 
			this.labelControl3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl3.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl3.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl3.Appearance.ForeColor = System.Drawing.SystemColors.ActiveCaption;
			this.labelControl3.Appearance.Options.UseBackColor = true;
			this.labelControl3.Appearance.Options.UseFont = true;
			this.labelControl3.Appearance.Options.UseForeColor = true;
			this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl3.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl3.LineVisible = true;
			this.labelControl3.Location = new System.Drawing.Point(8, 63);
			this.labelControl3.Name = "labelControl3";
			this.labelControl3.Size = new System.Drawing.Size(451, 15);
			this.labelControl3.TabIndex = 252;
			this.labelControl3.Text = "Data to include in the chart";
			// 
			// MainResultsForAll
			// 
			this.MainResultsForAll.Location = new System.Drawing.Point(13, 107);
			this.MainResultsForAll.Name = "MainResultsForAll";
			this.MainResultsForAll.Properties.Caption = "Main result for each assay/data table using Mobius-assigned coloring";
			this.MainResultsForAll.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.MainResultsForAll.Properties.RadioGroupIndex = 1;
			this.MainResultsForAll.Size = new System.Drawing.Size(367, 19);
			this.MainResultsForAll.TabIndex = 251;
			this.MainResultsForAll.TabStop = false;
			this.MainResultsForAll.Tag = " ";
			// 
			// ColsWithCfOnly
			// 
			this.ColsWithCfOnly.EditValue = true;
			this.ColsWithCfOnly.Location = new System.Drawing.Point(13, 84);
			this.ColsWithCfOnly.Name = "ColsWithCfOnly";
			this.ColsWithCfOnly.Properties.Caption = "Only those columns with already defined conditional formatting";
			this.ColsWithCfOnly.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ColsWithCfOnly.Properties.RadioGroupIndex = 1;
			this.ColsWithCfOnly.Size = new System.Drawing.Size(329, 19);
			this.ColsWithCfOnly.TabIndex = 250;
			this.ColsWithCfOnly.Tag = " ";
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(333, 284);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(60, 23);
			this.OK.TabIndex = 248;
			this.OK.Text = "&OK";
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
			this.Cancel.Location = new System.Drawing.Point(399, 284);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(60, 23);
			this.Cancel.TabIndex = 247;
			this.Cancel.Text = "Cancel";
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
			this.labelControl5.Location = new System.Drawing.Point(-1, 271);
			this.labelControl5.Name = "labelControl5";
			this.labelControl5.Size = new System.Drawing.Size(467, 10);
			this.labelControl5.TabIndex = 249;
			// 
			// labelControl2
			// 
			this.labelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl2.Appearance.BackColor = System.Drawing.Color.LightSteelBlue;
			this.labelControl2.Appearance.Options.UseBackColor = true;
			this.labelControl2.Appearance.Options.UseTextOptions = true;
			this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.labelControl2.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.labelControl2.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
			this.labelControl2.Location = new System.Drawing.Point(-2, -3);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Padding = new System.Windows.Forms.Padding(6);
			this.labelControl2.Size = new System.Drawing.Size(468, 54);
			this.labelControl2.TabIndex = 246;
			this.labelControl2.Text = "This tool charts the current query results data onto a gene-target dendrogram or " +
					"a target pathway map. Existing conditional formatting from the query or Mobius-a" +
					"ssigned coloring can be used. ";
			// 
			// AssayResultsTargetMapDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(464, 311);
			this.Controls.Add(this.MinMaxSummarization);
			this.Controls.Add(this.MeanSummarization);
			this.Controls.Add(this.labelControl4);
			this.Controls.Add(this.TargetMap);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.labelControl3);
			this.Controls.Add(this.MainResultsForAll);
			this.Controls.Add(this.ColsWithCfOnly);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.labelControl5);
			this.Controls.Add(this.labelControl2);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AssayResultsTargetMapDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Assay Results Gene-Target Dendogram or Pathway Map";
			((System.ComponentModel.ISupportInitialize)(this.MinMaxSummarization.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MeanSummarization.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TargetMap.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MainResultsForAll.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ColsWithCfOnly.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.CheckEdit MinMaxSummarization;
		public DevExpress.XtraEditors.CheckEdit MeanSummarization;
		public DevExpress.XtraEditors.LabelControl labelControl4;
		public DevExpress.XtraEditors.ComboBoxEdit TargetMap;
		public DevExpress.XtraEditors.LabelControl labelControl1;
		public DevExpress.XtraEditors.LabelControl labelControl3;
		public DevExpress.XtraEditors.CheckEdit MainResultsForAll;
		public DevExpress.XtraEditors.CheckEdit ColsWithCfOnly;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.LabelControl labelControl5;
		public DevExpress.XtraEditors.LabelControl labelControl2;
	}
}