namespace Mobius.ClientComponents
{
	partial class CriteriaCompoundIdControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Cids = new DevExpress.XtraEditors.TextEdit();
			this.SelectCids = new DevExpress.XtraEditors.SimpleButton();
			((System.ComponentModel.ISupportInitialize)(this.Cids.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// Cids
			// 
			this.Cids.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Cids.Location = new System.Drawing.Point(3, 3);
			this.Cids.Name = "Cids";
			this.Cids.Size = new System.Drawing.Size(260, 20);
			this.Cids.TabIndex = 93;
			this.Cids.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Cids_KeyDown);
			this.Cids.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Cids_KeyPress);
			this.Cids.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Cids_MouseDown);
			// 
			// SelectCids
			// 
			this.SelectCids.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectCids.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SelectCids.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SelectCids.Appearance.Options.UseFont = true;
			this.SelectCids.Appearance.Options.UseForeColor = true;
			this.SelectCids.Cursor = System.Windows.Forms.Cursors.Default;
			this.SelectCids.Location = new System.Drawing.Point(269, 2);
			this.SelectCids.Name = "SelectCids";
			this.SelectCids.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.SelectCids.Size = new System.Drawing.Size(57, 21);
			this.SelectCids.TabIndex = 94;
			this.SelectCids.Tag = "OK";
			this.SelectCids.Text = "Edit...";
			this.SelectCids.Click += new System.EventHandler(this.SelectCids_Click);
			// 
			// CriteriaCompoundIdControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.Cids);
			this.Controls.Add(this.SelectCids);
			this.Name = "CriteriaCompoundIdControl";
			this.Size = new System.Drawing.Size(331, 27);
			((System.ComponentModel.ISupportInitialize)(this.Cids.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.TextEdit Cids;
		public DevExpress.XtraEditors.SimpleButton SelectCids;

	}
}
