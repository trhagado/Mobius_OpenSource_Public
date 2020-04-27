namespace Mobius.ClientComponents
{
	partial class TargetSummaryDialog
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
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.TargetSummaryOptionsControl = new Mobius.ClientComponents.TargetSummaryOptionsControl();
			this.AdvancedButton = new DevExpress.XtraEditors.SimpleButton();
			this.SuspendLayout();
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
			this.Cancel.Location = new System.Drawing.Point(428, 393);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(60, 24);
			this.Cancel.TabIndex = 253;
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
			this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl5.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl5.LineVisible = true;
			this.labelControl5.Location = new System.Drawing.Point(-2, 381);
			this.labelControl5.Name = "labelControl5";
			this.labelControl5.Size = new System.Drawing.Size(495, 10);
			this.labelControl5.TabIndex = 255;
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(362, 393);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(60, 24);
			this.OK.TabIndex = 254;
			this.OK.Text = "&OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// TargetSummaryOptionsControl
			// 
			this.TargetSummaryOptionsControl.AssayTypes = "\'Functional\',\'Binding\'";
			this.TargetSummaryOptionsControl.Location = new System.Drawing.Point(3, 4);
			this.TargetSummaryOptionsControl.Name = "TargetSummaryOptionsControl";
			this.TargetSummaryOptionsControl.Size = new System.Drawing.Size(502, 396);
			this.TargetSummaryOptionsControl.TabIndex = 256;
			// 
			// AdvancedButton
			// 
			this.AdvancedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AdvancedButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AdvancedButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.AdvancedButton.Appearance.Options.UseFont = true;
			this.AdvancedButton.Appearance.Options.UseForeColor = true;
			this.AdvancedButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.AdvancedButton.Enabled = false;
			this.AdvancedButton.Location = new System.Drawing.Point(3, 393);
			this.AdvancedButton.Name = "AdvancedButton";
			this.AdvancedButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.AdvancedButton.Size = new System.Drawing.Size(73, 24);
			this.AdvancedButton.TabIndex = 257;
			this.AdvancedButton.Text = "Advanced...";
			this.AdvancedButton.Click += new System.EventHandler(this.AdvancedButton_Click);
			// 
			// TargetSummaryDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(492, 421);
			this.Controls.Add(this.AdvancedButton);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.labelControl5);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.TargetSummaryOptionsControl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TargetSummaryDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Target Summary Table";
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.LabelControl labelControl5;
		public DevExpress.XtraEditors.SimpleButton OK;
		private TargetSummaryOptionsControl TargetSummaryOptionsControl;
		public DevExpress.XtraEditors.SimpleButton AdvancedButton;
	}
}