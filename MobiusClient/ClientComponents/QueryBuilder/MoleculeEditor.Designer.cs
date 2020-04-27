namespace Mobius.ClientComponents
{
	partial class MoleculeEditor
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
			Mobius.Data.MoleculeMx moleculeMx1 = new Mobius.Data.MoleculeMx();
			this.LabelCtl = new DevExpress.XtraEditors.LabelControl();
			this.MoleculeCtl = new Mobius.ClientComponents.MoleculeControl();
			this.SuspendLayout();
			// 
			// LabelCtl
			// 
			this.LabelCtl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LabelCtl.Appearance.BackColor = System.Drawing.Color.White;
			this.LabelCtl.Appearance.Options.UseBackColor = true;
			this.LabelCtl.Appearance.Options.UseTextOptions = true;
			this.LabelCtl.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.LabelCtl.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
			this.LabelCtl.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.LabelCtl.Location = new System.Drawing.Point(0, 0);
			this.LabelCtl.Name = "LabelCtl";
			this.LabelCtl.Size = new System.Drawing.Size(170, 38);
			this.LabelCtl.TabIndex = 2;
			this.LabelCtl.Text = "Editing structure...";
			// 
			// MoleculeCtl
			// 
			this.MoleculeCtl.AllowDrop = true;
			this.MoleculeCtl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MoleculeCtl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.MoleculeCtl.Location = new System.Drawing.Point(0, 0);
			this.MoleculeCtl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			moleculeMx1.BackColor = System.Drawing.Color.Empty;
			moleculeMx1.DateTimeValue = new System.DateTime(((long)(0)));
			moleculeMx1.DbLink = "";
			moleculeMx1.Filtered = false;
			moleculeMx1.ForeColor = System.Drawing.Color.Black;
			moleculeMx1.FormattedBitmap = null;
			moleculeMx1.FormattedText = null;
			moleculeMx1.Hyperlink = "";
			moleculeMx1.Hyperlinked = false;
			moleculeMx1.IsNonExistant = false;
			moleculeMx1.IsRetrievingValue = false;
			moleculeMx1.Modified = false;
			moleculeMx1.NumericValue = -4194303D;
			this.MoleculeCtl.Molecule = moleculeMx1;
			this.MoleculeCtl.Name = "MoleculeCtl";
			this.MoleculeCtl.Size = new System.Drawing.Size(170, 38);
			this.MoleculeCtl.TabIndex = 1;
			this.MoleculeCtl.EditValueChanged += new System.EventHandler(this.MoleculeCtl_EditValueChanged);
			// 
			// MoleculeEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(172, 40);
			this.Controls.Add(this.LabelCtl);
			this.Controls.Add(this.MoleculeCtl);
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MoleculeEditor";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "MoleculeEditor";
			this.Shown += new System.EventHandler(this.MoleculeEditor_Shown);
			this.ResumeLayout(false);

		}

		#endregion

		public MoleculeControl MoleculeCtl;
		private DevExpress.XtraEditors.LabelControl LabelCtl;
	}
}