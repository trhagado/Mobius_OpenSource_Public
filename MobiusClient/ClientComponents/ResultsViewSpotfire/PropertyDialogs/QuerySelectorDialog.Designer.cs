namespace Mobius.SpotfireClient
{
	partial class QuerySelectorDialog
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
			this.QuerySelectorControl = new Mobius.SpotfireClient.QuerySelectorControl();
			this.CancelBut = new DevExpress.XtraEditors.SimpleButton();
			this.OkButton = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.SuspendLayout();
			// 
			// QuerySelectorControl
			// 
			this.QuerySelectorControl.Location = new System.Drawing.Point(12, 32);
			this.QuerySelectorControl.Name = "QuerySelectorControl";
			this.QuerySelectorControl.Size = new System.Drawing.Size(339, 26);
			this.QuerySelectorControl.TabIndex = 0;
			// 
			// CancelBut
			// 
			this.CancelBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelBut.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBut.Location = new System.Drawing.Point(291, 69);
			this.CancelBut.Name = "CancelBut";
			this.CancelBut.Size = new System.Drawing.Size(60, 24);
			this.CancelBut.TabIndex = 186;
			this.CancelBut.Text = "Cancel";
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OkButton.Location = new System.Drawing.Point(223, 69);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = new System.Drawing.Size(60, 24);
			this.OkButton.TabIndex = 185;
			this.OkButton.Text = "OK";
			// 
			// labelControl1
			// 
			this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl1.Appearance.ForeColor = System.Drawing.Color.Black;
			this.labelControl1.Appearance.Options.UseFont = true;
			this.labelControl1.Appearance.Options.UseForeColor = true;
			this.labelControl1.Location = new System.Drawing.Point(12, 12);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(284, 14);
			this.labelControl1.TabIndex = 199;
			this.labelControl1.Text = "Select the query to be used from the menu below:";
			// 
			// QuerySelectorDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(356, 99);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.CancelBut);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.QuerySelectorControl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "QuerySelectorDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Select Query";
			this.Activated += new System.EventHandler(this.QuerySelectorDialog_Activated);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public DevExpress.XtraEditors.SimpleButton CancelBut;
		public DevExpress.XtraEditors.SimpleButton OkButton;
		public DevExpress.XtraEditors.LabelControl labelControl1;
		public SpotfireClient.QuerySelectorControl QuerySelectorControl;
	}
}