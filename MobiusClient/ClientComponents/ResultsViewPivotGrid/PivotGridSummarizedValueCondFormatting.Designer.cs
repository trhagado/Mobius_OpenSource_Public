namespace Mobius.ClientComponents.ResultsViews
{
	partial class PivotGridSummarizedValueCondFormatting
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
			this.Rules = new Mobius.ClientComponents.CondFormatRulesControl();
			this.CancelBut = new DevExpress.XtraEditors.SimpleButton();
			this.OKButton = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.SuspendLayout();
			// 
			// Rules
			// 
			this.Rules.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.Rules.Location = new System.Drawing.Point(3, 1);
			this.Rules.Name = "Rules";
			this.Rules.Size = new System.Drawing.Size(498, 232);
			this.Rules.TabIndex = 198;
			// 
			// CancelBut
			// 
			this.CancelBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelBut.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBut.Location = new System.Drawing.Point(429, 239);
			this.CancelBut.Name = "CancelBut";
			this.CancelBut.Size = new System.Drawing.Size(70, 24);
			this.CancelBut.TabIndex = 199;
			this.CancelBut.Text = "Cancel";
			this.CancelBut.Click += new System.EventHandler(this.CancelBut_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Location = new System.Drawing.Point(353, 239);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = new System.Drawing.Size(70, 24);
			this.OKButton.TabIndex = 200;
			this.OKButton.Text = "&OK";
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(-1, 228);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(504, 10);
			this.labelControl1.TabIndex = 201;
			// 
			// PivotGridSummarizedValueCondFormatting
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(505, 268);
			this.Controls.Add(this.CancelBut);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.Rules);
			this.Controls.Add(this.labelControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "PivotGridSummarizedValueCondFormatting";
			this.Text = "PivotGridSummarizedValueCondFormatting";
			this.ResumeLayout(false);

		}

		#endregion

		private CondFormatRulesControl Rules;
		public DevExpress.XtraEditors.SimpleButton CancelBut;
		public DevExpress.XtraEditors.SimpleButton OKButton;
		private DevExpress.XtraEditors.LabelControl labelControl1;



	}
}