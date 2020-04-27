namespace Mobius.ClientComponents
{
	partial class AssayResultsHeatMapDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssayResultsHeatMapDialog));
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
			this.MainResultsForAll = new DevExpress.XtraEditors.CheckEdit();
			this.ColsWithCfOnly = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
			this.MinMaxSummarization = new DevExpress.XtraEditors.CheckEdit();
			this.MeanSummarization = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
			this.SummarizeByTarget = new DevExpress.XtraEditors.CheckEdit();
			this.SummarizeByAssay = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.ByAssay = new DevExpress.XtraEditors.CheckEdit();
			this.ByGeneFamily = new DevExpress.XtraEditors.CheckEdit();
			this.ByTargetGene = new DevExpress.XtraEditors.CheckEdit();
			this.ByQuery = new DevExpress.XtraEditors.CheckEdit();
			this.ApplyButton = new DevExpress.XtraEditors.SimpleButton();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			((System.ComponentModel.ISupportInitialize)(this.MainResultsForAll.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ColsWithCfOnly.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MinMaxSummarization.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MeanSummarization.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SummarizeByTarget.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SummarizeByAssay.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ByAssay.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ByGeneFamily.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ByTargetGene.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ByQuery.Properties)).BeginInit();
			this.SuspendLayout();
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
			this.labelControl2.Location = new System.Drawing.Point(-3, -3);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Padding = new System.Windows.Forms.Padding(6);
			this.labelControl2.Size = new System.Drawing.Size(477, 53);
			this.labelControl2.TabIndex = 88;
			this.labelControl2.Text = resources.GetString("labelControl2.Text");
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(274, 413);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(60, 24);
			this.OK.TabIndex = 229;
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
			this.Cancel.Location = new System.Drawing.Point(340, 413);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(60, 24);
			this.Cancel.TabIndex = 228;
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
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
			this.labelControl5.Location = new System.Drawing.Point(-3, 401);
			this.labelControl5.Name = "labelControl5";
			this.labelControl5.Size = new System.Drawing.Size(475, 10);
			this.labelControl5.TabIndex = 230;
			// 
			// MainResultsForAll
			// 
			this.MainResultsForAll.Location = new System.Drawing.Point(12, 104);
			this.MainResultsForAll.Name = "MainResultsForAll";
			this.MainResultsForAll.Properties.Caption = "Main result for each assay/data table using Mobius-assigned coloring";
			this.MainResultsForAll.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.MainResultsForAll.Properties.RadioGroupIndex = 1;
			this.MainResultsForAll.Size = new System.Drawing.Size(367, 19);
			this.MainResultsForAll.TabIndex = 232;
			this.MainResultsForAll.TabStop = false;
			this.MainResultsForAll.Tag = " ";
			// 
			// ColsWithCfOnly
			// 
			this.ColsWithCfOnly.EditValue = true;
			this.ColsWithCfOnly.Location = new System.Drawing.Point(12, 81);
			this.ColsWithCfOnly.Name = "ColsWithCfOnly";
			this.ColsWithCfOnly.Properties.Caption = "Only those columns with already defined conditional formatting";
			this.ColsWithCfOnly.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ColsWithCfOnly.Properties.RadioGroupIndex = 1;
			this.ColsWithCfOnly.Size = new System.Drawing.Size(329, 19);
			this.ColsWithCfOnly.TabIndex = 231;
			this.ColsWithCfOnly.Tag = " ";
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
			this.labelControl3.Location = new System.Drawing.Point(7, 60);
			this.labelControl3.Name = "labelControl3";
			this.labelControl3.Size = new System.Drawing.Size(458, 15);
			this.labelControl3.TabIndex = 233;
			this.labelControl3.Text = "Data to include in the heat map";
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
			this.labelControl4.Location = new System.Drawing.Point(7, 130);
			this.labelControl4.Name = "labelControl4";
			this.labelControl4.Size = new System.Drawing.Size(458, 25);
			this.labelControl4.TabIndex = 240;
			this.labelControl4.Text = "Summarization of multiple activity values for a compound for the same assay/targe" +
					"t";
			// 
			// MinMaxSummarization
			// 
			this.MinMaxSummarization.Location = new System.Drawing.Point(12, 181);
			this.MinMaxSummarization.Name = "MinMaxSummarization";
			this.MinMaxSummarization.Properties.Caption = "Most potent (min CRC, max SP)";
			this.MinMaxSummarization.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.MinMaxSummarization.Properties.RadioGroupIndex = 3;
			this.MinMaxSummarization.Size = new System.Drawing.Size(173, 19);
			this.MinMaxSummarization.TabIndex = 242;
			this.MinMaxSummarization.TabStop = false;
			// 
			// MeanSummarization
			// 
			this.MeanSummarization.EditValue = true;
			this.MeanSummarization.Location = new System.Drawing.Point(12, 158);
			this.MeanSummarization.Name = "MeanSummarization";
			this.MeanSummarization.Properties.Caption = "Calculate means (CRC - geometric, SP - arithmetic)";
			this.MeanSummarization.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.MeanSummarization.Properties.RadioGroupIndex = 3;
			this.MeanSummarization.Size = new System.Drawing.Size(274, 19);
			this.MeanSummarization.TabIndex = 241;
			// 
			// labelControl6
			// 
			this.labelControl6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl6.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl6.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl6.Appearance.ForeColor = System.Drawing.SystemColors.ActiveCaption;
			this.labelControl6.Appearance.Options.UseBackColor = true;
			this.labelControl6.Appearance.Options.UseFont = true;
			this.labelControl6.Appearance.Options.UseForeColor = true;
			this.labelControl6.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl6.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl6.LineVisible = true;
			this.labelControl6.Location = new System.Drawing.Point(7, 209);
			this.labelControl6.Name = "labelControl6";
			this.labelControl6.Size = new System.Drawing.Size(458, 15);
			this.labelControl6.TabIndex = 245;
			this.labelControl6.Text = "Summarization level";
			// 
			// SummarizeByTarget
			// 
			this.SummarizeByTarget.Location = new System.Drawing.Point(12, 253);
			this.SummarizeByTarget.Name = "SummarizeByTarget";
			this.SummarizeByTarget.Properties.Caption = "By target (i.e. all assays for a target are summarized together)";
			this.SummarizeByTarget.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.SummarizeByTarget.Properties.RadioGroupIndex = 2;
			this.SummarizeByTarget.Size = new System.Drawing.Size(367, 19);
			this.SummarizeByTarget.TabIndex = 244;
			this.SummarizeByTarget.TabStop = false;
			this.SummarizeByTarget.Tag = " ";
			// 
			// SummarizeByAssay
			// 
			this.SummarizeByAssay.EditValue = true;
			this.SummarizeByAssay.Location = new System.Drawing.Point(12, 230);
			this.SummarizeByAssay.Name = "SummarizeByAssay";
			this.SummarizeByAssay.Properties.Caption = "By assay / data table";
			this.SummarizeByAssay.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.SummarizeByAssay.Properties.RadioGroupIndex = 2;
			this.SummarizeByAssay.Size = new System.Drawing.Size(329, 19);
			this.SummarizeByAssay.TabIndex = 243;
			this.SummarizeByAssay.Tag = " ";
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
			this.labelControl1.Location = new System.Drawing.Point(7, 280);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(458, 15);
			this.labelControl1.TabIndex = 248;
			this.labelControl1.Text = "Order of assays/targets on the X-axis";
			// 
			// ByAssay
			// 
			this.ByAssay.Location = new System.Drawing.Point(12, 326);
			this.ByAssay.Name = "ByAssay";
			this.ByAssay.Properties.Caption = "By assay / data table name";
			this.ByAssay.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ByAssay.Properties.RadioGroupIndex = 4;
			this.ByAssay.Size = new System.Drawing.Size(367, 19);
			this.ByAssay.TabIndex = 247;
			this.ByAssay.TabStop = false;
			this.ByAssay.Tag = " ";
			// 
			// ByGeneFamily
			// 
			this.ByGeneFamily.Location = new System.Drawing.Point(12, 376);
			this.ByGeneFamily.Name = "ByGeneFamily";
			this.ByGeneFamily.Properties.Caption = "By gene family name";
			this.ByGeneFamily.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ByGeneFamily.Properties.RadioGroupIndex = 4;
			this.ByGeneFamily.Size = new System.Drawing.Size(367, 19);
			this.ByGeneFamily.TabIndex = 249;
			this.ByGeneFamily.TabStop = false;
			this.ByGeneFamily.Tag = " ";
			// 
			// ByTargetGene
			// 
			this.ByTargetGene.Location = new System.Drawing.Point(12, 351);
			this.ByTargetGene.Name = "ByTargetGene";
			this.ByTargetGene.Properties.Caption = "By target gene symbol";
			this.ByTargetGene.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ByTargetGene.Properties.RadioGroupIndex = 4;
			this.ByTargetGene.Size = new System.Drawing.Size(367, 19);
			this.ByTargetGene.TabIndex = 250;
			this.ByTargetGene.TabStop = false;
			this.ByTargetGene.Tag = " ";
			// 
			// ByQuery
			// 
			this.ByQuery.EditValue = true;
			this.ByQuery.Location = new System.Drawing.Point(12, 301);
			this.ByQuery.Name = "ByQuery";
			this.ByQuery.Properties.Caption = "As ordered in the query";
			this.ByQuery.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ByQuery.Properties.RadioGroupIndex = 4;
			this.ByQuery.Size = new System.Drawing.Size(367, 19);
			this.ByQuery.TabIndex = 251;
			this.ByQuery.Tag = " ";
			// 
			// ApplyButton
			// 
			this.ApplyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ApplyButton.Location = new System.Drawing.Point(405, 413);
			this.ApplyButton.Name = "ApplyButton";
			this.ApplyButton.Size = new System.Drawing.Size(60, 24);
			this.ApplyButton.TabIndex = 252;
			this.ApplyButton.Text = "Apply";
			this.ApplyButton.Click += new System.EventHandler(this.ApplyButton_Click);
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "Properties.bmp");
			// 
			// AssayResultsHeatMapDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(472, 441);
			this.Controls.Add(this.ByQuery);
			this.Controls.Add(this.ByTargetGene);
			this.Controls.Add(this.ByGeneFamily);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.ByAssay);
			this.Controls.Add(this.labelControl6);
			this.Controls.Add(this.SummarizeByTarget);
			this.Controls.Add(this.SummarizeByAssay);
			this.Controls.Add(this.MinMaxSummarization);
			this.Controls.Add(this.MeanSummarization);
			this.Controls.Add(this.labelControl4);
			this.Controls.Add(this.labelControl3);
			this.Controls.Add(this.MainResultsForAll);
			this.Controls.Add(this.ColsWithCfOnly);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.labelControl5);
			this.Controls.Add(this.labelControl2);
			this.Controls.Add(this.ApplyButton);
			this.Controls.Add(this.OK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AssayResultsHeatMapDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Assay Results Heat Map";
			((System.ComponentModel.ISupportInitialize)(this.MainResultsForAll.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ColsWithCfOnly.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MinMaxSummarization.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MeanSummarization.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SummarizeByTarget.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SummarizeByAssay.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ByAssay.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ByGeneFamily.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ByTargetGene.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ByQuery.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.LabelControl labelControl2;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.LabelControl labelControl5;
		public DevExpress.XtraEditors.CheckEdit MainResultsForAll;
		public DevExpress.XtraEditors.CheckEdit ColsWithCfOnly;
		public DevExpress.XtraEditors.LabelControl labelControl3;
		public DevExpress.XtraEditors.LabelControl labelControl4;
		public DevExpress.XtraEditors.CheckEdit MinMaxSummarization;
		public DevExpress.XtraEditors.CheckEdit MeanSummarization;
		public DevExpress.XtraEditors.LabelControl labelControl6;
		public DevExpress.XtraEditors.CheckEdit SummarizeByTarget;
		public DevExpress.XtraEditors.CheckEdit SummarizeByAssay;
		public DevExpress.XtraEditors.LabelControl labelControl1;
		public DevExpress.XtraEditors.CheckEdit ByAssay;
		public DevExpress.XtraEditors.CheckEdit ByGeneFamily;
		public DevExpress.XtraEditors.CheckEdit ByTargetGene;
		public DevExpress.XtraEditors.CheckEdit ByQuery;
		public DevExpress.XtraEditors.SimpleButton ApplyButton;
		public System.Windows.Forms.ImageList Bitmaps16x16;
	}
}