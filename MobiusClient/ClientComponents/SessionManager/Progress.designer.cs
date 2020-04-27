namespace Mobius.ClientComponents
{
	partial class Progress
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
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.Caption = new DevExpress.XtraEditors.LabelControl();
			this.ProgressTimer = new System.Windows.Forms.Timer(this.components);
			this.CancellingMessage = new DevExpress.XtraEditors.LabelControl();
			this.ProgressBar = new DevExpress.XtraEditors.MarqueeProgressBarControl();
			((System.ComponentModel.ISupportInitialize)(this.ProgressBar.Properties)).BeginInit();
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
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Abort;
			this.Cancel.Location = new System.Drawing.Point(225, 65);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(72, 22);
			this.Cancel.TabIndex = 9;
			this.Cancel.Tag = "Cancel";
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// Caption
			// 
			this.Caption.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.Caption.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Caption.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Caption.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Caption.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.Caption.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
			this.Caption.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.Caption.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.Caption.Cursor = System.Windows.Forms.Cursors.Default;
			this.Caption.Location = new System.Drawing.Point(9, 7);
			this.Caption.Name = "Caption";
			this.Caption.Size = new System.Drawing.Size(288, 44);
			this.Caption.TabIndex = 10;
			this.Caption.Text = "123456789012345678901234567890123456789012345678901234567890";
			// 
			// ProgressTimer
			// 
			this.ProgressTimer.Enabled = true;
			this.ProgressTimer.Tick += new System.EventHandler(this.ProgressTimer_Tick);
			// 
			// CancellingMessage
			// 
			this.CancellingMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.CancellingMessage.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.CancellingMessage.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CancellingMessage.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.CancellingMessage.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.CancellingMessage.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
			this.CancellingMessage.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.CancellingMessage.Cursor = System.Windows.Forms.Cursors.Default;
			this.CancellingMessage.Location = new System.Drawing.Point(97, 50);
			this.CancellingMessage.Name = "CancellingMessage";
			this.CancellingMessage.Size = new System.Drawing.Size(112, 13);
			this.CancellingMessage.TabIndex = 11;
			this.CancellingMessage.Text = "Cancelling Search...";
			this.CancellingMessage.Visible = false;
			// 
			// ProgressBar
			// 
			this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressBar.EditValue = 0;
			this.ProgressBar.Location = new System.Drawing.Point(9, 67);
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Properties.ProgressAnimationMode = DevExpress.Utils.Drawing.ProgressAnimationMode.Cycle;
			this.ProgressBar.Properties.ProgressViewStyle = DevExpress.XtraEditors.Controls.ProgressViewStyle.Solid;
			this.ProgressBar.Size = new System.Drawing.Size(200, 20);
			this.ProgressBar.TabIndex = 12;
			// 
			// Progress
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(306, 95);
			this.ControlBox = false;
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.Caption);
			this.Controls.Add(this.CancellingMessage);
			this.Controls.Add(this.ProgressBar);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Progress";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Mobius";
			((System.ComponentModel.ISupportInitialize)(this.ProgressBar.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.LabelControl Caption;
		public System.Windows.Forms.Timer ProgressTimer;
		public DevExpress.XtraEditors.LabelControl CancellingMessage;
		private DevExpress.XtraEditors.MarqueeProgressBarControl ProgressBar;
	}
}