namespace Mobius.ClientComponents
{
	partial class CriteriaStructureOptionsSS
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Align = new DevExpress.XtraEditors.CheckEdit();
			this.Highlight = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.Align.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Highlight.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// Orient
			// 
			this.Align.Cursor = System.Windows.Forms.Cursors.Default;
			this.Align.EditValue = true;
			this.Align.Location = new System.Drawing.Point(2, 50);
			this.Align.Name = "Align";
			this.Align.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Align.Properties.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Align.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Align.Properties.Appearance.Options.UseBackColor = true;
			this.Align.Properties.Appearance.Options.UseFont = true;
			this.Align.Properties.Appearance.Options.UseForeColor = true;
			this.Align.Properties.Caption = "Align structures to the substructure query";
			this.Align.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Align.Size = new System.Drawing.Size(234, 19);
			this.Align.TabIndex = 36;
			// 
			// Highlight
			// 
			this.Highlight.Cursor = System.Windows.Forms.Cursors.Default;
			this.Highlight.EditValue = true;
			this.Highlight.Location = new System.Drawing.Point(3, 25);
			this.Highlight.Name = "Highlight";
			this.Highlight.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Highlight.Properties.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Highlight.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Highlight.Properties.Appearance.Options.UseBackColor = true;
			this.Highlight.Properties.Appearance.Options.UseFont = true;
			this.Highlight.Properties.Appearance.Options.UseForeColor = true;
			this.Highlight.Properties.Caption = "Highlight the matching substructure";
			this.Highlight.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Highlight.Size = new System.Drawing.Size(202, 19);
			this.Highlight.TabIndex = 37;
			// 
			// labelControl2
			// 
			this.labelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl2.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl2.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl2.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl2.LineVisible = true;
			this.labelControl2.Location = new System.Drawing.Point(2, 0);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(316, 19);
			this.labelControl2.TabIndex = 38;
			this.labelControl2.Text = "Substructure search options";
			// 
			// CriteriaStructureOptionsSS
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.labelControl2);
			this.Controls.Add(this.Align);
			this.Controls.Add(this.Highlight);
			this.Name = "CriteriaStructureOptionsSS";
			this.Size = new System.Drawing.Size(321, 81);
			((System.ComponentModel.ISupportInitialize)(this.Align.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Highlight.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.CheckEdit Align;
		public DevExpress.XtraEditors.CheckEdit Highlight;
		public DevExpress.XtraEditors.LabelControl labelControl2;
	}
}
