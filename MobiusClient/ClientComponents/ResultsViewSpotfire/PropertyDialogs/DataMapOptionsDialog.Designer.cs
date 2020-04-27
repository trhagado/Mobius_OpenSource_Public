namespace Mobius.SpotfireClient
{
	partial class DataMapOptionsDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataMapOptionsDialog));
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList();
			this.OKButton = new DevExpress.XtraEditors.SimpleButton();
			this.SummarizationAsIsCheckEdit = new DevExpress.XtraEditors.CheckEdit();
			this.SummarizationLabel = new DevExpress.XtraEditors.LabelControl();
			this.SummarizationOneRowPerKeyCheckEdit = new DevExpress.XtraEditors.CheckEdit();
			this.ExportMultipleFilesCheckEdit = new DevExpress.XtraEditors.CheckEdit();
			this.ExportSingleFileCheckEdit = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.CancelBut = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
			this.QtSelectorControl = new Mobius.SpotfireClient.QueryTableSelectorControl();
			((System.ComponentModel.ISupportInitialize)(this.SummarizationAsIsCheckEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SummarizationOneRowPerKeyCheckEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExportMultipleFilesCheckEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExportSingleFileCheckEdit.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "Add.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "Edit.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "ScrollDown16x16.bmp");
			this.Bitmaps16x16.Images.SetKeyName(3, "Delete.bmp");
			this.Bitmaps16x16.Images.SetKeyName(4, "up2.bmp");
			this.Bitmaps16x16.Images.SetKeyName(5, "down2.bmp");
			this.Bitmaps16x16.Images.SetKeyName(6, "Copy.bmp");
			this.Bitmaps16x16.Images.SetKeyName(7, "Paste.bmp");
			this.Bitmaps16x16.Images.SetKeyName(8, "Properties.bmp");
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OKButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OKButton.Appearance.Options.UseFont = true;
			this.OKButton.Appearance.Options.UseForeColor = true;
			this.OKButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.OKButton.Location = new System.Drawing.Point(274, 188);
			this.OKButton.Name = "OKButton";
			this.OKButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OKButton.Size = new System.Drawing.Size(74, 24);
			this.OKButton.TabIndex = 204;
			this.OKButton.Text = "OK";
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// SummarizationAsIsCheckEdit
			// 
			this.SummarizationAsIsCheckEdit.Location = new System.Drawing.Point(30, 147);
			this.SummarizationAsIsCheckEdit.Name = "SummarizationAsIsCheckEdit";
			this.SummarizationAsIsCheckEdit.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
			this.SummarizationAsIsCheckEdit.Properties.Appearance.Options.UseForeColor = true;
			this.SummarizationAsIsCheckEdit.Properties.Caption = "None (use as-is)";
			this.SummarizationAsIsCheckEdit.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.SummarizationAsIsCheckEdit.Properties.RadioGroupIndex = 2;
			this.SummarizationAsIsCheckEdit.Size = new System.Drawing.Size(128, 19);
			this.SummarizationAsIsCheckEdit.TabIndex = 226;
			this.SummarizationAsIsCheckEdit.TabStop = false;
			// 
			// SummarizationLabel
			// 
			this.SummarizationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SummarizationLabel.Appearance.ForeColor = System.Drawing.Color.Black;
			this.SummarizationLabel.Appearance.Options.UseForeColor = true;
			this.SummarizationLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.SummarizationLabel.LineVisible = true;
			this.SummarizationLabel.Location = new System.Drawing.Point(6, 103);
			this.SummarizationLabel.Name = "SummarizationLabel";
			this.SummarizationLabel.Size = new System.Drawing.Size(420, 16);
			this.SummarizationLabel.TabIndex = 225;
			this.SummarizationLabel.Text = "Summarization of Mobius data:";
			// 
			// SummarizationOneRowPerKeyCheckEdit
			// 
			this.SummarizationOneRowPerKeyCheckEdit.EditValue = true;
			this.SummarizationOneRowPerKeyCheckEdit.Location = new System.Drawing.Point(30, 122);
			this.SummarizationOneRowPerKeyCheckEdit.Name = "SummarizationOneRowPerKeyCheckEdit";
			this.SummarizationOneRowPerKeyCheckEdit.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
			this.SummarizationOneRowPerKeyCheckEdit.Properties.Appearance.Options.UseForeColor = true;
			this.SummarizationOneRowPerKeyCheckEdit.Properties.Caption = "One row per CorpId (where possible)";
			this.SummarizationOneRowPerKeyCheckEdit.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.SummarizationOneRowPerKeyCheckEdit.Properties.RadioGroupIndex = 2;
			this.SummarizationOneRowPerKeyCheckEdit.Size = new System.Drawing.Size(188, 19);
			this.SummarizationOneRowPerKeyCheckEdit.TabIndex = 227;
			// 
			// ExportMultipleFilesCheckEdit
			// 
			this.ExportMultipleFilesCheckEdit.Location = new System.Drawing.Point(30, 32);
			this.ExportMultipleFilesCheckEdit.Name = "ExportMultipleFilesCheckEdit";
			this.ExportMultipleFilesCheckEdit.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
			this.ExportMultipleFilesCheckEdit.Properties.Appearance.Options.UseForeColor = true;
			this.ExportMultipleFilesCheckEdit.Properties.Caption = "Single table:";
			this.ExportMultipleFilesCheckEdit.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ExportMultipleFilesCheckEdit.Properties.RadioGroupIndex = 1;
			this.ExportMultipleFilesCheckEdit.Size = new System.Drawing.Size(82, 19);
			this.ExportMultipleFilesCheckEdit.TabIndex = 229;
			this.ExportMultipleFilesCheckEdit.TabStop = false;
			// 
			// ExportSingleFileCheckEdit
			// 
			this.ExportSingleFileCheckEdit.EditValue = true;
			this.ExportSingleFileCheckEdit.Location = new System.Drawing.Point(30, 62);
			this.ExportSingleFileCheckEdit.Name = "ExportSingleFileCheckEdit";
			this.ExportSingleFileCheckEdit.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
			this.ExportSingleFileCheckEdit.Properties.Appearance.Options.UseForeColor = true;
			this.ExportSingleFileCheckEdit.Properties.Caption = "All tables - Merge all tables in the query into a single data source";
			this.ExportSingleFileCheckEdit.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ExportSingleFileCheckEdit.Properties.RadioGroupIndex = 1;
			this.ExportSingleFileCheckEdit.Size = new System.Drawing.Size(337, 19);
			this.ExportSingleFileCheckEdit.TabIndex = 230;
			// 
			// labelControl2
			// 
			this.labelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl2.Appearance.ForeColor = System.Drawing.Color.Black;
			this.labelControl2.Appearance.Options.UseForeColor = true;
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.LineVisible = true;
			this.labelControl2.Location = new System.Drawing.Point(-4, 173);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(445, 11);
			this.labelControl2.TabIndex = 231;
			// 
			// CancelBut
			// 
			this.CancelBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelBut.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CancelBut.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.CancelBut.Appearance.Options.UseFont = true;
			this.CancelBut.Appearance.Options.UseForeColor = true;
			this.CancelBut.Cursor = System.Windows.Forms.Cursors.Default;
			this.CancelBut.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBut.Location = new System.Drawing.Point(357, 188);
			this.CancelBut.Name = "CancelBut";
			this.CancelBut.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CancelBut.Size = new System.Drawing.Size(74, 24);
			this.CancelBut.TabIndex = 232;
			this.CancelBut.Text = "Cancel";
			// 
			// labelControl3
			// 
			this.labelControl3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl3.Appearance.ForeColor = System.Drawing.Color.Black;
			this.labelControl3.Appearance.Options.UseForeColor = true;
			this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl3.LineVisible = true;
			this.labelControl3.Location = new System.Drawing.Point(11, 12);
			this.labelControl3.Name = "labelControl3";
			this.labelControl3.Size = new System.Drawing.Size(420, 16);
			this.labelControl3.TabIndex = 233;
			this.labelControl3.Text = "Mobius query table(s) to use as a data source for this Spotfire data table";
			// 
			// QtSelectorControl
			// 
			this.QtSelectorControl.Location = new System.Drawing.Point(119, 35);
			this.QtSelectorControl.Name = "QtSelectorControl";
			this.QtSelectorControl.Size = new System.Drawing.Size(246, 21);
			this.QtSelectorControl.TabIndex = 234;
			// 
			// DataMapOptionsDialog
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(438, 218);
			this.Controls.Add(this.QtSelectorControl);
			this.Controls.Add(this.labelControl3);
			this.Controls.Add(this.CancelBut);
			this.Controls.Add(this.labelControl2);
			this.Controls.Add(this.ExportMultipleFilesCheckEdit);
			this.Controls.Add(this.ExportSingleFileCheckEdit);
			this.Controls.Add(this.SummarizationAsIsCheckEdit);
			this.Controls.Add(this.SummarizationLabel);
			this.Controls.Add(this.SummarizationOneRowPerKeyCheckEdit);
			this.Controls.Add(this.OKButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DataMapOptionsDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Mapping Options";
			((System.ComponentModel.ISupportInitialize)(this.SummarizationAsIsCheckEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SummarizationOneRowPerKeyCheckEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ExportMultipleFilesCheckEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ExportSingleFileCheckEdit.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		public System.Windows.Forms.ImageList Bitmaps16x16;
		public DevExpress.XtraEditors.SimpleButton OKButton;
		private DevExpress.XtraEditors.CheckEdit SummarizationAsIsCheckEdit;
		private DevExpress.XtraEditors.LabelControl SummarizationLabel;
		private DevExpress.XtraEditors.CheckEdit SummarizationOneRowPerKeyCheckEdit;
		private DevExpress.XtraEditors.CheckEdit ExportMultipleFilesCheckEdit;
		private DevExpress.XtraEditors.CheckEdit ExportSingleFileCheckEdit;
		private DevExpress.XtraEditors.LabelControl labelControl2;
		public DevExpress.XtraEditors.SimpleButton CancelBut;
		private DevExpress.XtraEditors.LabelControl labelControl3;
		private QueryTableSelectorControl QtSelectorControl;
	}
}