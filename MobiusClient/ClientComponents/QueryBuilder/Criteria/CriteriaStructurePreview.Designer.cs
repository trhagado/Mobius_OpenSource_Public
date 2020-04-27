namespace Mobius.ClientComponents
{
	partial class CriteriaStructurePreview 
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
			this.SearchStatusLabel = new DevExpress.XtraEditors.LabelControl();
			this.MoleculeGridPageControl = new Mobius.ClientComponents.MoleculeGridPageControl();
			this.SuspendLayout();
			// 
			// SearchStatusLabel
			// 
			this.SearchStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SearchStatusLabel.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.SearchStatusLabel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SearchStatusLabel.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.SearchStatusLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.SearchStatusLabel.Cursor = System.Windows.Forms.Cursors.Default;
			this.SearchStatusLabel.LineVisible = true;
			this.SearchStatusLabel.Location = new System.Drawing.Point(6, 5);
			this.SearchStatusLabel.Name = "SearchStatusLabel";
			this.SearchStatusLabel.Size = new System.Drawing.Size(415, 18);
			this.SearchStatusLabel.TabIndex = 253;
			this.SearchStatusLabel.Text = "Search Results";
			// 
			// MoleculeGridPageControl
			// 
			this.MoleculeGridPageControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MoleculeGridPageControl.Location = new System.Drawing.Point(3, 3);
			this.MoleculeGridPageControl.Name = "MoleculeGridPageControl";
			this.MoleculeGridPageControl.Size = new System.Drawing.Size(714, 732);
			this.MoleculeGridPageControl.TabIndex = 252;
			// 
			// CriteriaStructurePreview
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.SearchStatusLabel);
			this.Controls.Add(this.MoleculeGridPageControl);
			this.Name = "CriteriaStructurePreview";
			this.Size = new System.Drawing.Size(734, 752);
			this.ResumeLayout(false);

		}

		#endregion
		public MoleculeGridPageControl MoleculeGridPageControl;
		public DevExpress.XtraEditors.LabelControl SearchStatusLabel;
	}
}
