namespace Mobius.Tools
{
	partial class SarLandscape
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SarLandscape));
			this.MinSimCtl = new DevExpress.XtraEditors.TextEdit();
			this.label5 = new DevExpress.XtraEditors.LabelControl();
			this.Help = new DevExpress.XtraEditors.SimpleButton();
			this.PairCountCtl = new DevExpress.XtraEditors.TextEdit();
			this.label4 = new DevExpress.XtraEditors.LabelControl();
			this.Ratio = new DevExpress.XtraEditors.CheckEdit();
			this.SimpleDifference = new DevExpress.XtraEditors.CheckEdit();
			this.label8 = new DevExpress.XtraEditors.LabelControl();
			this.label6 = new DevExpress.XtraEditors.LabelControl();
			this.Prompt = new DevExpress.XtraEditors.LabelControl();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.Hbar = new DevExpress.XtraEditors.LabelControl();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.NegativeLog = new DevExpress.XtraEditors.CheckEdit();
			this.MolarNegativeLog = new DevExpress.XtraEditors.CheckEdit();
			this.CalcAbsoluteValue = new DevExpress.XtraEditors.CheckEdit();
			this.StandardSimilarity = new DevExpress.XtraEditors.CheckEdit();
			this.ECFP4Similarity = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
			this.CompoundIdLabel = new DevExpress.XtraEditors.LabelControl();
			this.KeyCriteriaCtl = new Mobius.ClientComponents.CriteriaCompoundIdControl();
			this.EndPointCtl = new Mobius.ClientComponents.FieldSelectorControl();
			((System.ComponentModel.ISupportInitialize)(this.MinSimCtl.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PairCountCtl.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Ratio.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SimpleDifference.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NegativeLog.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MolarNegativeLog.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CalcAbsoluteValue.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StandardSimilarity.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ECFP4Similarity.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// MinSimCtl
			// 
			this.MinSimCtl.EditValue = ".8";
			this.MinSimCtl.Location = new System.Drawing.Point(265, 469);
			this.MinSimCtl.Name = "MinSimCtl";
			this.MinSimCtl.Size = new System.Drawing.Size(47, 20);
			this.MinSimCtl.TabIndex = 46;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(69, 471);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(190, 13);
			this.label5.TabIndex = 44;
			this.label5.Text = "Mimimum Acceptable Similarity (0 - 1.0):";
			// 
			// Help
			// 
			this.Help.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Help.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Help.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Help.Appearance.Options.UseFont = true;
			this.Help.Appearance.Options.UseForeColor = true;
			this.Help.Cursor = System.Windows.Forms.Cursors.Default;
			this.Help.Location = new System.Drawing.Point(641, 553);
			this.Help.Name = "Help";
			this.Help.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Help.Size = new System.Drawing.Size(60, 23);
			this.Help.TabIndex = 80;
			this.Help.Tag = "Cancel";
			this.Help.Text = "Help";
			this.Help.Click += new System.EventHandler(this.Help_Click);
			// 
			// PairCountCtl
			// 
			this.PairCountCtl.EditValue = "100";
			this.PairCountCtl.Location = new System.Drawing.Point(224, 515);
			this.PairCountCtl.Name = "PairCountCtl";
			this.PairCountCtl.Size = new System.Drawing.Size(47, 20);
			this.PairCountCtl.TabIndex = 75;
			// 
			// label4
			// 
			this.label4.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.label4.Appearance.Options.UseFont = true;
			this.label4.Location = new System.Drawing.Point(12, 518);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(195, 13);
			this.label4.TabIndex = 74;
			this.label4.Text = "Maximum number pairs to display:";
			// 
			// Ratio
			// 
			this.Ratio.Location = new System.Drawing.Point(67, 300);
			this.Ratio.Name = "Ratio";
			this.Ratio.Properties.Caption = "Ratio: (v1/ v2)";
			this.Ratio.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.Ratio.Properties.RadioGroupIndex = 1;
			this.Ratio.Size = new System.Drawing.Size(257, 19);
			this.Ratio.TabIndex = 1;
			this.Ratio.TabStop = false;
			// 
			// SimpleDifference
			// 
			this.SimpleDifference.EditValue = true;
			this.SimpleDifference.Location = new System.Drawing.Point(67, 225);
			this.SimpleDifference.Name = "SimpleDifference";
			this.SimpleDifference.Properties.Caption = "Activity difference: (v1 - v1)";
			this.SimpleDifference.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.SimpleDifference.Properties.RadioGroupIndex = 1;
			this.SimpleDifference.Size = new System.Drawing.Size(227, 19);
			this.SimpleDifference.TabIndex = 0;
			// 
			// label8
			// 
			this.label8.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.label8.Appearance.Options.UseBackColor = true;
			this.label8.Location = new System.Drawing.Point(69, 138);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(63, 13);
			this.label8.TabIndex = 41;
			this.label8.Text = "Assay Name:";
			// 
			// label6
			// 
			this.label6.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.label6.Appearance.Options.UseBackColor = true;
			this.label6.Location = new System.Drawing.Point(67, 156);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(65, 13);
			this.label6.TabIndex = 40;
			this.label6.Text = "Activity Field:";
			// 
			// Prompt
			// 
			this.Prompt.AllowHtmlString = true;
			this.Prompt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Prompt.Appearance.BackColor = System.Drawing.Color.LightSteelBlue;
			this.Prompt.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Prompt.Appearance.Options.UseBackColor = true;
			this.Prompt.Appearance.Options.UseFont = true;
			this.Prompt.Appearance.Options.UseTextOptions = true;
			this.Prompt.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.Prompt.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.Prompt.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.Prompt.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.Prompt.Cursor = System.Windows.Forms.Cursors.Hand;
			this.Prompt.Location = new System.Drawing.Point(0, 0);
			this.Prompt.Name = "Prompt";
			this.Prompt.Padding = new System.Windows.Forms.Padding(4);
			this.Prompt.Size = new System.Drawing.Size(707, 64);
			this.Prompt.TabIndex = 70;
			this.Prompt.Text = resources.GetString("Prompt.Text");
			this.Prompt.HyperlinkClick += new DevExpress.Utils.HyperlinkClickEventHandler(this.Prompt_HyperlinkClick);
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(509, 553);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(60, 23);
			this.OK.TabIndex = 69;
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
			this.Cancel.Location = new System.Drawing.Point(575, 553);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(60, 23);
			this.Cancel.TabIndex = 68;
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// Hbar
			// 
			this.Hbar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Hbar.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.Hbar.Appearance.Options.UseFont = true;
			this.Hbar.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
			this.Hbar.LineVisible = true;
			this.Hbar.Location = new System.Drawing.Point(12, 111);
			this.Hbar.Name = "Hbar";
			this.Hbar.Size = new System.Drawing.Size(697, 13);
			this.Hbar.TabIndex = 82;
			this.Hbar.Text = "Activity endpoint to be analyzed";
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelControl1.Appearance.Options.UseFont = true;
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(7, 377);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(694, 13);
			this.labelControl1.TabIndex = 83;
			this.labelControl1.Text = "Molecular similarity measure";
			// 
			// labelControl2
			// 
			this.labelControl2.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelControl2.Appearance.Options.UseFont = true;
			this.labelControl2.Location = new System.Drawing.Point(12, 200);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(218, 13);
			this.labelControl2.TabIndex = 84;
			this.labelControl2.Text = "Activity difference calculation method:";
			// 
			// NegativeLog
			// 
			this.NegativeLog.Location = new System.Drawing.Point(67, 250);
			this.NegativeLog.Name = "NegativeLog";
			this.NegativeLog.Properties.Caption = "Negative log difference: (- log10(v1))  -  (- log10(v2))";
			this.NegativeLog.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.NegativeLog.Properties.RadioGroupIndex = 1;
			this.NegativeLog.Size = new System.Drawing.Size(290, 19);
			this.NegativeLog.TabIndex = 85;
			this.NegativeLog.TabStop = false;
			// 
			// MolarNegativeLog
			// 
			this.MolarNegativeLog.Location = new System.Drawing.Point(67, 275);
			this.MolarNegativeLog.Name = "MolarNegativeLog";
			this.MolarNegativeLog.Properties.Caption = "Molar-normalized negative log difference (e.g pIC50): (- log10(Molar(v1)))  -  (-" +
    " log10(Molar(v2)))";
			this.MolarNegativeLog.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.MolarNegativeLog.Properties.RadioGroupIndex = 1;
			this.MolarNegativeLog.Size = new System.Drawing.Size(585, 19);
			this.MolarNegativeLog.TabIndex = 86;
			this.MolarNegativeLog.TabStop = false;
			// 
			// CalcAbsoluteValue
			// 
			this.CalcAbsoluteValue.EditValue = true;
			this.CalcAbsoluteValue.Location = new System.Drawing.Point(84, 333);
			this.CalcAbsoluteValue.Name = "CalcAbsoluteValue";
			this.CalcAbsoluteValue.Properties.Caption = "Convert differences to absolute values (max value if ratio)";
			this.CalcAbsoluteValue.Size = new System.Drawing.Size(331, 19);
			this.CalcAbsoluteValue.TabIndex = 87;
			this.CalcAbsoluteValue.TabStop = false;
			// 
			// Similarity
			// 
			this.StandardSimilarity.EditValue = true;
			this.StandardSimilarity.Location = new System.Drawing.Point(69, 403);
			this.StandardSimilarity.Name = "Similarity";
			this.StandardSimilarity.Properties.Caption = "Fingerprint / Tanimoto Similarity";
			this.StandardSimilarity.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.StandardSimilarity.Properties.RadioGroupIndex = 2;
			this.StandardSimilarity.Size = new System.Drawing.Size(255, 19);
			this.StandardSimilarity.TabIndex = 88;
			// 
			// ECFP4Similarity
			// 
			this.ECFP4Similarity.Location = new System.Drawing.Point(69, 428);
			this.ECFP4Similarity.Name = "ECFP4Similarity";
			this.ECFP4Similarity.Properties.Caption = "ECFP4 fingerprint / Tanimoto Similarity";
			this.ECFP4Similarity.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ECFP4Similarity.Properties.RadioGroupIndex = 2;
			this.ECFP4Similarity.Size = new System.Drawing.Size(227, 19);
			this.ECFP4Similarity.TabIndex = 89;
			this.ECFP4Similarity.TabStop = false;
			// 
			// labelControl3
			// 
			this.labelControl3.Location = new System.Drawing.Point(286, 518);
			this.labelControl3.Name = "labelControl3";
			this.labelControl3.Size = new System.Drawing.Size(406, 13);
			this.labelControl3.TabIndex = 90;
			this.labelControl3.Text = "(Values displayed will be the largest  (Activity-difference / (1 - Mol-similarity" +
    ")) values)";
			// 
			// labelControl4
			// 
			this.labelControl4.AllowDrop = true;
			this.labelControl4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl4.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelControl4.Appearance.Options.UseFont = true;
			this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl4.LineVisible = true;
			this.labelControl4.Location = new System.Drawing.Point(0, 540);
			this.labelControl4.Name = "labelControl4";
			this.labelControl4.Size = new System.Drawing.Size(709, 10);
			this.labelControl4.TabIndex = 91;
			// 
			// CompoundIdLabel
			// 
			this.CompoundIdLabel.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.CompoundIdLabel.Appearance.Options.UseFont = true;
			this.CompoundIdLabel.Location = new System.Drawing.Point(12, 80);
			this.CompoundIdLabel.Name = "CompoundIdLabel";
			this.CompoundIdLabel.Size = new System.Drawing.Size(118, 13);
			this.CompoundIdLabel.TabIndex = 193;
			this.CompoundIdLabel.Text = "CorpIds to analyze:  . . . .";
			// 
			// KeyCriteriaCtl
			// 
			this.KeyCriteriaCtl.Location = new System.Drawing.Point(142, 74);
			this.KeyCriteriaCtl.Name = "KeyCriteriaCtl";
			this.KeyCriteriaCtl.Size = new System.Drawing.Size(471, 28);
			this.KeyCriteriaCtl.TabIndex = 192;
			// 
			// EndPointCtl
			// 
			this.EndPointCtl.Location = new System.Drawing.Point(142, 136);
			this.EndPointCtl.MetaColumn = null;
			this.EndPointCtl.Name = "EndPointCtl";
			this.EndPointCtl.OptionExcludeKeys = true;
			this.EndPointCtl.OptionExcludeNonNumericTypes = true;
			this.EndPointCtl.OptionIncludeAllSelectableColumns = true;
			this.EndPointCtl.OptionIncludeNoneItem = false;
			this.EndPointCtl.OptionShowTableAndFieldLabels = false;
			this.EndPointCtl.QueryColumn = null;
			this.EndPointCtl.Size = new System.Drawing.Size(427, 46);
			this.EndPointCtl.TabIndex = 39;
			// 
			// SarLandscape
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(707, 580);
			this.Controls.Add(this.CompoundIdLabel);
			this.Controls.Add(this.KeyCriteriaCtl);
			this.Controls.Add(this.labelControl4);
			this.Controls.Add(this.labelControl3);
			this.Controls.Add(this.ECFP4Similarity);
			this.Controls.Add(this.StandardSimilarity);
			this.Controls.Add(this.CalcAbsoluteValue);
			this.Controls.Add(this.MolarNegativeLog);
			this.Controls.Add(this.NegativeLog);
			this.Controls.Add(this.Ratio);
			this.Controls.Add(this.labelControl2);
			this.Controls.Add(this.SimpleDifference);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.Hbar);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.MinSimCtl);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.EndPointCtl);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.Help);
			this.Controls.Add(this.PairCountCtl);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.Prompt);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SarLandscape";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Structure-Activity Similarity (SAS) Map";
			((System.ComponentModel.ISupportInitialize)(this.MinSimCtl.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PairCountCtl.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Ratio.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SimpleDifference.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NegativeLog.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MolarNegativeLog.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CalcAbsoluteValue.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StandardSimilarity.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ECFP4Similarity.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public DevExpress.XtraEditors.TextEdit MinSimCtl;
		public DevExpress.XtraEditors.LabelControl label5;
		public DevExpress.XtraEditors.SimpleButton Help;
		public DevExpress.XtraEditors.TextEdit PairCountCtl;
		public DevExpress.XtraEditors.LabelControl label4;
		public DevExpress.XtraEditors.CheckEdit Ratio;
		public DevExpress.XtraEditors.CheckEdit SimpleDifference;
		public DevExpress.XtraEditors.LabelControl Prompt;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		private Mobius.ClientComponents.FieldSelectorControl EndPointCtl;
		public DevExpress.XtraEditors.LabelControl label8;
		public DevExpress.XtraEditors.LabelControl label6;
		private DevExpress.XtraEditors.LabelControl Hbar;
		private DevExpress.XtraEditors.LabelControl labelControl1;
		public DevExpress.XtraEditors.LabelControl labelControl2;
		public DevExpress.XtraEditors.CheckEdit NegativeLog;
		public DevExpress.XtraEditors.CheckEdit MolarNegativeLog;
		public DevExpress.XtraEditors.CheckEdit CalcAbsoluteValue;
		public DevExpress.XtraEditors.CheckEdit StandardSimilarity;
		public DevExpress.XtraEditors.CheckEdit ECFP4Similarity;
		public DevExpress.XtraEditors.LabelControl labelControl3;
		private DevExpress.XtraEditors.LabelControl labelControl4;
		private ClientComponents.CriteriaCompoundIdControl KeyCriteriaCtl;
		public DevExpress.XtraEditors.LabelControl CompoundIdLabel;
	}
}