namespace Mobius.ClientComponents
{
	partial class CriteriaStructureFormatDialog
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
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.AlignStructures = new DevExpress.XtraEditors.CheckEdit();
			this.HilightStructures = new DevExpress.XtraEditors.CheckEdit();
			((System.ComponentModel.ISupportInitialize)(this.AlignStructures.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HilightStructures.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.Location = new System.Drawing.Point(165, 71);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(60, 23);
			this.OK.TabIndex = 94;
			this.OK.Text = "OK";
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
			this.Cancel.Location = new System.Drawing.Point(233, 71);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(60, 23);
			this.Cancel.TabIndex = 93;
			this.Cancel.Text = "Cancel";
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(2, 59);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(298, 10);
			this.labelControl1.TabIndex = 95;
			// 
			// AlignStructs
			// 
			this.AlignStructures.EditValue = true;
			this.AlignStructures.Location = new System.Drawing.Point(12, 37);
			this.AlignStructures.Name = "AlignStructs";
			this.AlignStructures.Properties.Caption = "Align Structure Orientation";
			this.AlignStructures.Properties.GlyphAlignment = DevExpress.Utils.HorzAlignment.Default;
			this.AlignStructures.Size = new System.Drawing.Size(182, 19);
			this.AlignStructures.TabIndex = 132;
			// 
			// ShowColors
			// 
			this.HilightStructures.EditValue = true;
			this.HilightStructures.Location = new System.Drawing.Point(12, 12);
			this.HilightStructures.Name = "ShowColors";
			this.HilightStructures.Properties.Caption = "Highlight Match  (Show Colors)";
			this.HilightStructures.Properties.GlyphAlignment = DevExpress.Utils.HorzAlignment.Default;
			this.HilightStructures.Size = new System.Drawing.Size(201, 19);
			this.HilightStructures.TabIndex = 131;
			// 
			// CriteriaStructureFormatDialog
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(299, 97);
			this.Controls.Add(this.AlignStructures);
			this.Controls.Add(this.HilightStructures);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.labelControl1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CriteriaStructureFormatDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Structure Format";
			((System.ComponentModel.ISupportInitialize)(this.AlignStructures.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HilightStructures.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.LabelControl labelControl1;
		public DevExpress.XtraEditors.CheckEdit AlignStructures;
		public DevExpress.XtraEditors.CheckEdit HilightStructures;
	}
}