namespace Mobius.ClientComponents
{
	partial class NumericBinningDialog
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
			this.NumericIntervalPanel = new DevExpress.XtraEditors.PanelControl();
			this.NoInterval = new DevExpress.XtraEditors.CheckEdit();
			this.VariableIntervals = new DevExpress.XtraEditors.TextEdit();
			this.VariableIntervalsOption = new DevExpress.XtraEditors.CheckEdit();
			this.NumericIntervalPanelOK = new DevExpress.XtraEditors.SimpleButton();
			this.FixedIntervalCountOption = new DevExpress.XtraEditors.CheckEdit();
			this.NumericIntervalPanelCancel = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
			this.IntervalBinCount = new DevExpress.XtraEditors.SpinEdit();
			this.FixedIntervalSizeOption = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
			this.FixedIntervalSize = new DevExpress.XtraEditors.TextEdit();
			this.Field1 = new Mobius.ClientComponents.FieldSelectorControl();
			this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
			this.textEdit1 = new DevExpress.XtraEditors.TextEdit();
			this.BinMethodGroupBox = new System.Windows.Forms.GroupBox();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.NumericIntervalPanel)).BeginInit();
			this.NumericIntervalPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NoInterval.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.VariableIntervals.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.VariableIntervalsOption.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FixedIntervalCountOption.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IntervalBinCount.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FixedIntervalSizeOption.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FixedIntervalSize.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).BeginInit();
			this.BinMethodGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// NumericIntervalPanel
			// 
			this.NumericIntervalPanel.Controls.Add(this.labelControl2);
			this.NumericIntervalPanel.Controls.Add(this.BinMethodGroupBox);
			this.NumericIntervalPanel.Controls.Add(this.labelControl1);
			this.NumericIntervalPanel.Controls.Add(this.textEdit1);
			this.NumericIntervalPanel.Controls.Add(this.labelControl8);
			this.NumericIntervalPanel.Controls.Add(this.Field1);
			this.NumericIntervalPanel.Controls.Add(this.NumericIntervalPanelOK);
			this.NumericIntervalPanel.Controls.Add(this.NumericIntervalPanelCancel);
			this.NumericIntervalPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NumericIntervalPanel.Location = new System.Drawing.Point(0, 0);
			this.NumericIntervalPanel.Name = "NumericIntervalPanel";
			this.NumericIntervalPanel.Size = new System.Drawing.Size(428, 406);
			this.NumericIntervalPanel.TabIndex = 270;
			// 
			// NoInterval
			// 
			this.NoInterval.Location = new System.Drawing.Point(20, 183);
			this.NoInterval.Name = "NoInterval";
			this.NoInterval.Properties.Caption = "None";
			this.NoInterval.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.NoInterval.Properties.RadioGroupIndex = 0;
			this.NoInterval.Size = new System.Drawing.Size(61, 19);
			this.NoInterval.TabIndex = 242;
			this.NoInterval.TabStop = false;
			this.NoInterval.CheckedChanged += new System.EventHandler(this.NoInterval_CheckedChanged);
			// 
			// VariableIntervals
			// 
			this.VariableIntervals.Location = new System.Drawing.Point(39, 153);
			this.VariableIntervals.Name = "VariableIntervals";
			this.VariableIntervals.Size = new System.Drawing.Size(360, 20);
			this.VariableIntervals.TabIndex = 42;
			this.VariableIntervals.EditValueChanged += new System.EventHandler(this.VariableIntervals_EditValueChanged);
			// 
			// VariableIntervalsOption
			// 
			this.VariableIntervalsOption.Location = new System.Drawing.Point(20, 130);
			this.VariableIntervalsOption.Name = "VariableIntervalsOption";
			this.VariableIntervalsOption.Properties.Caption = "List of variable size intervals (e.g.: -1, 5, 10, 50):";
			this.VariableIntervalsOption.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.VariableIntervalsOption.Properties.RadioGroupIndex = 0;
			this.VariableIntervalsOption.Size = new System.Drawing.Size(385, 19);
			this.VariableIntervalsOption.TabIndex = 31;
			this.VariableIntervalsOption.TabStop = false;
			this.VariableIntervalsOption.CheckedChanged += new System.EventHandler(this.VariableIntervalsOption_CheckedChanged);
			// 
			// NumericIntervalPanelOK
			// 
			this.NumericIntervalPanelOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NumericIntervalPanelOK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NumericIntervalPanelOK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.NumericIntervalPanelOK.Appearance.Options.UseFont = true;
			this.NumericIntervalPanelOK.Appearance.Options.UseForeColor = true;
			this.NumericIntervalPanelOK.Cursor = System.Windows.Forms.Cursors.Default;
			this.NumericIntervalPanelOK.Location = new System.Drawing.Point(297, 378);
			this.NumericIntervalPanelOK.Name = "NumericIntervalPanelOK";
			this.NumericIntervalPanelOK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.NumericIntervalPanelOK.Size = new System.Drawing.Size(60, 23);
			this.NumericIntervalPanelOK.TabIndex = 0;
			this.NumericIntervalPanelOK.Text = "&OK";
			this.NumericIntervalPanelOK.Click += new System.EventHandler(this.NumericIntervalPanelOK_Click);
			// 
			// FixedIntervalCountOption
			// 
			this.FixedIntervalCountOption.Location = new System.Drawing.Point(20, 24);
			this.FixedIntervalCountOption.Name = "FixedIntervalCountOption";
			this.FixedIntervalCountOption.Properties.Caption = "Fixed number of intervals from lowest to highest value";
			this.FixedIntervalCountOption.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.FixedIntervalCountOption.Properties.RadioGroupIndex = 0;
			this.FixedIntervalCountOption.Size = new System.Drawing.Size(289, 19);
			this.FixedIntervalCountOption.TabIndex = 32;
			this.FixedIntervalCountOption.TabStop = false;
			this.FixedIntervalCountOption.CheckedChanged += new System.EventHandler(this.FixedIntervalCountOption_CheckedChanged);
			// 
			// NumericIntervalPanelCancel
			// 
			this.NumericIntervalPanelCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NumericIntervalPanelCancel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NumericIntervalPanelCancel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.NumericIntervalPanelCancel.Appearance.Options.UseFont = true;
			this.NumericIntervalPanelCancel.Appearance.Options.UseForeColor = true;
			this.NumericIntervalPanelCancel.Cursor = System.Windows.Forms.Cursors.Default;
			this.NumericIntervalPanelCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.NumericIntervalPanelCancel.Location = new System.Drawing.Point(363, 378);
			this.NumericIntervalPanelCancel.Name = "NumericIntervalPanelCancel";
			this.NumericIntervalPanelCancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.NumericIntervalPanelCancel.Size = new System.Drawing.Size(60, 23);
			this.NumericIntervalPanelCancel.TabIndex = 240;
			this.NumericIntervalPanelCancel.Text = "Cancel";
			this.NumericIntervalPanelCancel.Click += new System.EventHandler(this.NumericIntervalPanelCancel_Click);
			// 
			// labelControl4
			// 
			this.labelControl4.Location = new System.Drawing.Point(40, 47);
			this.labelControl4.Name = "labelControl4";
			this.labelControl4.Size = new System.Drawing.Size(98, 13);
			this.labelControl4.TabIndex = 33;
			this.labelControl4.Text = "Number of intervals:";
			// 
			// IntervalBinCount
			// 
			this.IntervalBinCount.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
			this.IntervalBinCount.Location = new System.Drawing.Point(144, 44);
			this.IntervalBinCount.Name = "IntervalBinCount";
			this.IntervalBinCount.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
			this.IntervalBinCount.Properties.IsFloatValue = false;
			this.IntervalBinCount.Properties.Mask.EditMask = "N00";
			this.IntervalBinCount.Size = new System.Drawing.Size(59, 20);
			this.IntervalBinCount.TabIndex = 34;
			this.IntervalBinCount.EditValueChanged += new System.EventHandler(this.IntervalBinCount_EditValueChanged);
			// 
			// FixedIntervalSizeOption
			// 
			this.FixedIntervalSizeOption.Location = new System.Drawing.Point(20, 77);
			this.FixedIntervalSizeOption.Name = "FixedIntervalSizeOption";
			this.FixedIntervalSizeOption.Properties.Caption = "Fixed size intervals from lowest to highest value";
			this.FixedIntervalSizeOption.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.FixedIntervalSizeOption.Properties.RadioGroupIndex = 0;
			this.FixedIntervalSizeOption.Size = new System.Drawing.Size(258, 19);
			this.FixedIntervalSizeOption.TabIndex = 35;
			this.FixedIntervalSizeOption.TabStop = false;
			this.FixedIntervalSizeOption.CheckedChanged += new System.EventHandler(this.FixedIntervalSizeOption_CheckedChanged);
			// 
			// labelControl6
			// 
			this.labelControl6.Location = new System.Drawing.Point(40, 100);
			this.labelControl6.Name = "labelControl6";
			this.labelControl6.Size = new System.Drawing.Size(63, 13);
			this.labelControl6.TabIndex = 36;
			this.labelControl6.Text = "Interval size:";
			// 
			// FixedIntervalSize
			// 
			this.FixedIntervalSize.Location = new System.Drawing.Point(112, 98);
			this.FixedIntervalSize.Name = "FixedIntervalSize";
			this.FixedIntervalSize.Size = new System.Drawing.Size(59, 20);
			this.FixedIntervalSize.TabIndex = 51;
			this.FixedIntervalSize.EditValueChanged += new System.EventHandler(this.FixedIntervalSize_EditValueChanged);
			// 
			// Field1
			// 
			this.Field1.Location = new System.Drawing.Point(12, 29);
			this.Field1.MetaColumn = null;
			this.Field1.Name = "Field1";
			this.Field1.OptionIncludeAllSelectableColumns = true;
			this.Field1.Size = new System.Drawing.Size(393, 46);
			this.Field1.TabIndex = 243;
			// 
			// labelControl8
			// 
			this.labelControl8.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(70)))), ((int)(((byte)(213)))));
			this.labelControl8.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl8.LineVisible = true;
			this.labelControl8.Location = new System.Drawing.Point(6, 9);
			this.labelControl8.Name = "labelControl8";
			this.labelControl8.Size = new System.Drawing.Size(410, 14);
			this.labelControl8.TabIndex = 244;
			this.labelControl8.Text = "Field to be binned";
			// 
			// textEdit1
			// 
			this.textEdit1.Location = new System.Drawing.Point(45, 334);
			this.textEdit1.Name = "textEdit1";
			this.textEdit1.Size = new System.Drawing.Size(360, 20);
			this.textEdit1.TabIndex = 245;
			// 
			// BinMethodGroupBox
			// 
			this.BinMethodGroupBox.Controls.Add(this.FixedIntervalSize);
			this.BinMethodGroupBox.Controls.Add(this.labelControl6);
			this.BinMethodGroupBox.Controls.Add(this.FixedIntervalSizeOption);
			this.BinMethodGroupBox.Controls.Add(this.IntervalBinCount);
			this.BinMethodGroupBox.Controls.Add(this.labelControl4);
			this.BinMethodGroupBox.Controls.Add(this.NoInterval);
			this.BinMethodGroupBox.Controls.Add(this.FixedIntervalCountOption);
			this.BinMethodGroupBox.Controls.Add(this.VariableIntervals);
			this.BinMethodGroupBox.Controls.Add(this.VariableIntervalsOption);
			this.BinMethodGroupBox.Location = new System.Drawing.Point(6, 81);
			this.BinMethodGroupBox.Name = "BinMethodGroupBox";
			this.BinMethodGroupBox.Size = new System.Drawing.Size(410, 216);
			this.BinMethodGroupBox.TabIndex = 247;
			this.BinMethodGroupBox.TabStop = false;
			this.BinMethodGroupBox.Text = "Bin method";
			// 
			// labelControl1
			// 
			this.labelControl1.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(70)))), ((int)(((byte)(213)))));
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(7, 308);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(409, 20);
			this.labelControl1.TabIndex = 246;
			this.labelControl1.Text = "Binned field name";
			// 
			// labelControl2
			// 
			this.labelControl2.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(70)))), ((int)(((byte)(213)))));
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.LineVisible = true;
			this.labelControl2.Location = new System.Drawing.Point(0, 359);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(428, 12);
			this.labelControl2.TabIndex = 248;
			// 
			// BinnedFieldEditor
			// 
			this.AcceptButton = this.NumericIntervalPanelOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.NumericIntervalPanelCancel;
			this.ClientSize = new System.Drawing.Size(428, 406);
			this.Controls.Add(this.NumericIntervalPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "BinnedFieldEditor";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Edit Binned Field";
			this.Enter += new System.EventHandler(this.PivotGridDialogNumericIntervalPopup_Enter);
			this.Leave += new System.EventHandler(this.PivotGridDialogNumericIntervalPopup_Leave);
			((System.ComponentModel.ISupportInitialize)(this.NumericIntervalPanel)).EndInit();
			this.NumericIntervalPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.NoInterval.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.VariableIntervals.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.VariableIntervalsOption.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FixedIntervalCountOption.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IntervalBinCount.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FixedIntervalSizeOption.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FixedIntervalSize.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).EndInit();
			this.BinMethodGroupBox.ResumeLayout(false);
			this.BinMethodGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraEditors.PanelControl NumericIntervalPanel;
		private DevExpress.XtraEditors.CheckEdit NoInterval;
		private DevExpress.XtraEditors.TextEdit VariableIntervals;
		private DevExpress.XtraEditors.CheckEdit VariableIntervalsOption;
		public DevExpress.XtraEditors.SimpleButton NumericIntervalPanelOK;
		private DevExpress.XtraEditors.CheckEdit FixedIntervalCountOption;
		public DevExpress.XtraEditors.SimpleButton NumericIntervalPanelCancel;
		private DevExpress.XtraEditors.LabelControl labelControl4;
		private DevExpress.XtraEditors.SpinEdit IntervalBinCount;
		private DevExpress.XtraEditors.CheckEdit FixedIntervalSizeOption;
		private DevExpress.XtraEditors.LabelControl labelControl6;
		private DevExpress.XtraEditors.TextEdit FixedIntervalSize;
		private DevExpress.XtraEditors.LabelControl labelControl8;
		private FieldSelectorControl Field1;
		private DevExpress.XtraEditors.TextEdit textEdit1;
		public System.Windows.Forms.GroupBox BinMethodGroupBox;
		private DevExpress.XtraEditors.LabelControl labelControl1;
		private DevExpress.XtraEditors.LabelControl labelControl2;
	}
}