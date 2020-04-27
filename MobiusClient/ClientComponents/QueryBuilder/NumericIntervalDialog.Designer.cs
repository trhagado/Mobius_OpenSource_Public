namespace Mobius.ClientComponents
{
	partial class NumericIntervalDialog
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
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.NoInterval = new DevExpress.XtraEditors.CheckEdit();
			this.NumericIntervalPanelOK = new DevExpress.XtraEditors.SimpleButton();
			this.NumericIntervalPanelCancel = new DevExpress.XtraEditors.SimpleButton();
			this.IntervalSize = new DevExpress.XtraEditors.TextEdit();
			this.FixedIntervalSizeOption = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.NumericIntervalPanel)).BeginInit();
			this.NumericIntervalPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NoInterval.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IntervalSize.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FixedIntervalSizeOption.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// NumericIntervalPanel
			// 
			this.NumericIntervalPanel.Controls.Add(this.labelControl1);
			this.NumericIntervalPanel.Controls.Add(this.NoInterval);
			this.NumericIntervalPanel.Controls.Add(this.NumericIntervalPanelOK);
			this.NumericIntervalPanel.Controls.Add(this.NumericIntervalPanelCancel);
			this.NumericIntervalPanel.Controls.Add(this.IntervalSize);
			this.NumericIntervalPanel.Controls.Add(this.FixedIntervalSizeOption);
			this.NumericIntervalPanel.Controls.Add(this.labelControl8);
			this.NumericIntervalPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NumericIntervalPanel.Location = new System.Drawing.Point(0, 0);
			this.NumericIntervalPanel.Name = "NumericIntervalPanel";
			this.NumericIntervalPanel.Size = new System.Drawing.Size(249, 74);
			this.NumericIntervalPanel.TabIndex = 270;
			// 
			// labelControl1
			// 
			this.labelControl1.Location = new System.Drawing.Point(12, 12);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(122, 13);
			this.labelControl1.TabIndex = 244;
			this.labelControl1.Text = "Numeric Interval Bin Size:";
			// 
			// NoInterval
			// 
			this.NoInterval.Location = new System.Drawing.Point(268, 244);
			this.NoInterval.Name = "NoInterval";
			this.NoInterval.Properties.Caption = "None";
			this.NoInterval.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.NoInterval.Properties.RadioGroupIndex = 0;
			this.NoInterval.Size = new System.Drawing.Size(82, 19);
			this.NoInterval.TabIndex = 242;
			this.NoInterval.TabStop = false;
			// 
			// NumericIntervalPanelOK
			// 
			this.NumericIntervalPanelOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NumericIntervalPanelOK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NumericIntervalPanelOK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.NumericIntervalPanelOK.Appearance.Options.UseFont = true;
			this.NumericIntervalPanelOK.Appearance.Options.UseForeColor = true;
			this.NumericIntervalPanelOK.Cursor = System.Windows.Forms.Cursors.Default;
			this.NumericIntervalPanelOK.Location = new System.Drawing.Point(118, 46);
			this.NumericIntervalPanelOK.Name = "NumericIntervalPanelOK";
			this.NumericIntervalPanelOK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.NumericIntervalPanelOK.Size = new System.Drawing.Size(60, 23);
			this.NumericIntervalPanelOK.TabIndex = 0;
			this.NumericIntervalPanelOK.Text = "&OK";
			this.NumericIntervalPanelOK.Click += new System.EventHandler(this.NumericIntervalDialog_OK_Click);
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
			this.NumericIntervalPanelCancel.Location = new System.Drawing.Point(184, 46);
			this.NumericIntervalPanelCancel.Name = "NumericIntervalPanelCancel";
			this.NumericIntervalPanelCancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.NumericIntervalPanelCancel.Size = new System.Drawing.Size(60, 23);
			this.NumericIntervalPanelCancel.TabIndex = 240;
			this.NumericIntervalPanelCancel.Text = "Cancel";
			this.NumericIntervalPanelCancel.Click += new System.EventHandler(this.NumericIntervalDialog_Cancel_Click);
			// 
			// IntervalSize
			// 
			this.IntervalSize.Location = new System.Drawing.Point(140, 9);
			this.IntervalSize.Name = "IntervalSize";
			this.IntervalSize.Size = new System.Drawing.Size(97, 20);
			this.IntervalSize.TabIndex = 0;
			// 
			// FixedIntervalSizeOption
			// 
			this.FixedIntervalSizeOption.Location = new System.Drawing.Point(109, 306);
			this.FixedIntervalSizeOption.Name = "FixedIntervalSizeOption";
			this.FixedIntervalSizeOption.Properties.Caption = "Numeric Bin Width:";
			this.FixedIntervalSizeOption.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.FixedIntervalSizeOption.Properties.RadioGroupIndex = 0;
			this.FixedIntervalSizeOption.Size = new System.Drawing.Size(113, 19);
			this.FixedIntervalSizeOption.TabIndex = 35;
			this.FixedIntervalSizeOption.TabStop = false;
			// 
			// labelControl8
			// 
			this.labelControl8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl8.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl8.LineVisible = true;
			this.labelControl8.Location = new System.Drawing.Point(0, 33);
			this.labelControl8.Name = "labelControl8";
			this.labelControl8.Size = new System.Drawing.Size(249, 10);
			this.labelControl8.TabIndex = 243;
			// 
			// NumericIntervalDialog
			// 
			this.AcceptButton = this.NumericIntervalPanelOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.NumericIntervalPanelCancel;
			this.ClientSize = new System.Drawing.Size(249, 74);
			this.Controls.Add(this.NumericIntervalPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "NumericIntervalDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Numeric Value Binning";
			this.Enter += new System.EventHandler(this.PivotGridDialogNumericIntervalPopup_Enter);
			((System.ComponentModel.ISupportInitialize)(this.NumericIntervalPanel)).EndInit();
			this.NumericIntervalPanel.ResumeLayout(false);
			this.NumericIntervalPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NoInterval.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IntervalSize.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FixedIntervalSizeOption.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraEditors.PanelControl NumericIntervalPanel;
		private DevExpress.XtraEditors.CheckEdit NoInterval;
		public DevExpress.XtraEditors.SimpleButton NumericIntervalPanelOK;
		public DevExpress.XtraEditors.SimpleButton NumericIntervalPanelCancel;
		private DevExpress.XtraEditors.CheckEdit FixedIntervalSizeOption;
		private DevExpress.XtraEditors.LabelControl labelControl8;
		private DevExpress.XtraEditors.LabelControl labelControl1;
		private DevExpress.XtraEditors.TextEdit IntervalSize;
	}
}