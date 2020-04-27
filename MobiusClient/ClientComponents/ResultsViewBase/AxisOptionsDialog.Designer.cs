namespace Mobius.ClientComponents
{
	partial class AxisOptionsDialog
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
			this.AxisOptionsControl = new Mobius.ClientComponents.AxisOptionsControl();
			this.CancelBut = new DevExpress.XtraEditors.SimpleButton();
			this.OKButton = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.SuspendLayout();
			// 
			// AxisOptionsControl
			// 
			this.AxisOptionsControl.Location = new System.Drawing.Point(12, 12);
			this.AxisOptionsControl.Name = "AxisOptionsControl";
			this.AxisOptionsControl.Size = new System.Drawing.Size(457, 237);
			this.AxisOptionsControl.TabIndex = 0;
			// 
			// CancelBut
			// 
			this.CancelBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelBut.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBut.Location = new System.Drawing.Point(370, 257);
			this.CancelBut.Name = "CancelBut";
			this.CancelBut.Size = new System.Drawing.Size(70, 24);
			this.CancelBut.TabIndex = 193;
			this.CancelBut.Text = "Cancel";
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = new System.Drawing.Point(294, 257);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = new System.Drawing.Size(70, 24);
			this.OKButton.TabIndex = 194;
			this.OKButton.Text = "&OK";
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(-2, 246);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(450, 4);
			this.labelControl1.TabIndex = 195;
			// 
			// AxisOptionsDialog
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.CancelBut;
			this.ClientSize = new System.Drawing.Size(448, 289);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.CancelBut);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.AxisOptionsControl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AxisOptionsDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Axis Properties";
			this.ResumeLayout(false);

		}

		#endregion

		private AxisOptionsControl AxisOptionsControl;
		public DevExpress.XtraEditors.SimpleButton CancelBut;
		public DevExpress.XtraEditors.SimpleButton OKButton;
		private DevExpress.XtraEditors.LabelControl labelControl1;
	}
}