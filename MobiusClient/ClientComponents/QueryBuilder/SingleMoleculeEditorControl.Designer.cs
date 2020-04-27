namespace Mobius.ClientComponents
{
	partial class SingleMoleculeEditorControl
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
			this.components = new System.ComponentModel.Container();
			Mobius.Data.MoleculeMx moleculeMx1 = new Mobius.Data.MoleculeMx();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SingleMoleculeEditorControl));
			this.MoleculeControl = new Mobius.ClientComponents.MoleculeControl();
			this.AddToFavoritesButton = new DevExpress.XtraEditors.SimpleButton();
			this.RetrieveFavoritesButton = new DevExpress.XtraEditors.SimpleButton();
			this.RetrieveRecentButton = new DevExpress.XtraEditors.SimpleButton();
			this.RetrieveModel = new DevExpress.XtraEditors.SimpleButton();
			this.EditStructure = new DevExpress.XtraEditors.SimpleButton();
			this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.MolDisplayFormatEdit = new DevExpress.XtraEditors.ComboBoxEdit();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.MolDisplayFormatEdit.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// MoleculeControl
			// 
			this.MoleculeControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MoleculeControl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.MoleculeControl.Location = new System.Drawing.Point(0, 0);
			this.MoleculeControl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
			this.MoleculeControl.Molecule = moleculeMx1;
			this.MoleculeControl.Name = "MoleculeControl";
			this.MoleculeControl.Size = new System.Drawing.Size(500, 337);
			this.MoleculeControl.TabIndex = 0;
			// 
			// AddToFavoritesButton
			// 
			this.AddToFavoritesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AddToFavoritesButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AddToFavoritesButton.Appearance.ForeColor = System.Drawing.Color.Green;
			this.AddToFavoritesButton.Appearance.Options.UseFont = true;
			this.AddToFavoritesButton.Appearance.Options.UseForeColor = true;
			this.AddToFavoritesButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.AddToFavoritesButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.AddToFavoritesButton.Location = new System.Drawing.Point(172, 340);
			this.AddToFavoritesButton.Name = "AddToFavoritesButton";
			this.AddToFavoritesButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.AddToFavoritesButton.Size = new System.Drawing.Size(16, 22);
			this.AddToFavoritesButton.TabIndex = 43;
			this.AddToFavoritesButton.Tag = "";
			this.AddToFavoritesButton.Text = "+";
			this.AddToFavoritesButton.Click += new System.EventHandler(this.AddToFavoritesButton_Click);
			// 
			// RetrieveFavoritesButton
			// 
			this.RetrieveFavoritesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RetrieveFavoritesButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RetrieveFavoritesButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.RetrieveFavoritesButton.Appearance.Options.UseFont = true;
			this.RetrieveFavoritesButton.Appearance.Options.UseForeColor = true;
			this.RetrieveFavoritesButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.RetrieveFavoritesButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("RetrieveFavoritesButton.ImageOptions.Image")));
			this.RetrieveFavoritesButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.RetrieveFavoritesButton.Location = new System.Drawing.Point(151, 340);
			this.RetrieveFavoritesButton.Name = "RetrieveFavoritesButton";
			this.RetrieveFavoritesButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RetrieveFavoritesButton.Size = new System.Drawing.Size(22, 22);
			this.RetrieveFavoritesButton.TabIndex = 42;
			this.RetrieveFavoritesButton.Tag = "";
			this.RetrieveFavoritesButton.Click += new System.EventHandler(this.RetrieveFavoritesButton_Click);
			// 
			// RetrieveRecentButton
			// 
			this.RetrieveRecentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RetrieveRecentButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RetrieveRecentButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.RetrieveRecentButton.Appearance.Options.UseFont = true;
			this.RetrieveRecentButton.Appearance.Options.UseForeColor = true;
			this.RetrieveRecentButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.RetrieveRecentButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("RetrieveRecentButton.ImageOptions.Image")));
			this.RetrieveRecentButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.RetrieveRecentButton.Location = new System.Drawing.Point(123, 340);
			this.RetrieveRecentButton.Name = "RetrieveRecentButton";
			this.RetrieveRecentButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RetrieveRecentButton.Size = new System.Drawing.Size(22, 22);
			this.RetrieveRecentButton.TabIndex = 41;
			this.RetrieveRecentButton.Tag = "";
			this.RetrieveRecentButton.Click += new System.EventHandler(this.RetrieveRecentButton_Click);
			// 
			// RetrieveModel
			// 
			this.RetrieveModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RetrieveModel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RetrieveModel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.RetrieveModel.Appearance.Options.UseFont = true;
			this.RetrieveModel.Appearance.Options.UseForeColor = true;
			this.RetrieveModel.Cursor = System.Windows.Forms.Cursors.Default;
			this.RetrieveModel.Location = new System.Drawing.Point(3, 340);
			this.RetrieveModel.Name = "RetrieveModel";
			this.RetrieveModel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RetrieveModel.Size = new System.Drawing.Size(114, 22);
			this.RetrieveModel.TabIndex = 39;
			this.RetrieveModel.Tag = "RetrieveModel";
			this.RetrieveModel.Text = "Retrieve Structure";
			this.RetrieveModel.Click += new System.EventHandler(this.RetrieveModel_Click);
			// 
			// EditStructure
			// 
			this.EditStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.EditStructure.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EditStructure.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.EditStructure.Appearance.Options.UseFont = true;
			this.EditStructure.Appearance.Options.UseForeColor = true;
			this.EditStructure.Cursor = System.Windows.Forms.Cursors.Default;
			this.EditStructure.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("EditStructure.ImageOptions.Image")));
			this.EditStructure.Location = new System.Drawing.Point(428, 340);
			this.EditStructure.Name = "EditStructure";
			this.EditStructure.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.EditStructure.Size = new System.Drawing.Size(72, 22);
			this.EditStructure.TabIndex = 40;
			this.EditStructure.Tag = "EditStructure";
			this.EditStructure.Text = "Edit";
			this.EditStructure.Click += new System.EventHandler(this.EditStructure_Click);
			// 
			// MolDisplayFormatEdit
			// 
			this.MolDisplayFormatEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.MolDisplayFormatEdit.EditValue = "Structure";
			this.MolDisplayFormatEdit.Location = new System.Drawing.Point(298, 340);
			this.MolDisplayFormatEdit.Margin = new System.Windows.Forms.Padding(2);
			this.MolDisplayFormatEdit.Name = "MolDisplayFormatEdit";
			this.MolDisplayFormatEdit.Properties.AutoHeight = false;
			this.MolDisplayFormatEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.MolDisplayFormatEdit.Properties.Items.AddRange(new object[] {
            "Structure",
            "Biopolymer"});
			this.MolDisplayFormatEdit.Properties.Padding = new System.Windows.Forms.Padding(1, 0, 0, 0);
			this.MolDisplayFormatEdit.Size = new System.Drawing.Size(74, 22);
			this.MolDisplayFormatEdit.TabIndex = 44;
			this.MolDisplayFormatEdit.ToolTip = "View/edit as full atom/bond Structure or Biopolymer";
			// 
			// labelControl1
			// 
			this.labelControl1.Location = new System.Drawing.Point(253, 344);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(40, 13);
			this.labelControl1.TabIndex = 46;
			this.labelControl1.Text = "View as:";
			// 
			// SingleMoleculeEditorControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.MolDisplayFormatEdit);
			this.Controls.Add(this.AddToFavoritesButton);
			this.Controls.Add(this.RetrieveFavoritesButton);
			this.Controls.Add(this.RetrieveRecentButton);
			this.Controls.Add(this.RetrieveModel);
			this.Controls.Add(this.EditStructure);
			this.Controls.Add(this.MoleculeControl);
			this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.Name = "SingleMoleculeEditorControl";
			this.Size = new System.Drawing.Size(502, 366);
			((System.ComponentModel.ISupportInitialize)(this.MolDisplayFormatEdit.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public MoleculeControl MoleculeControl;
		public DevExpress.XtraEditors.SimpleButton AddToFavoritesButton;
		public DevExpress.XtraEditors.SimpleButton RetrieveFavoritesButton;
		public DevExpress.XtraEditors.SimpleButton RetrieveRecentButton;
		public DevExpress.XtraEditors.SimpleButton RetrieveModel;
		public DevExpress.XtraEditors.SimpleButton EditStructure;
		public System.Windows.Forms.ToolTip ToolTip1;
		private DevExpress.XtraEditors.ComboBoxEdit MolDisplayFormatEdit;
		private DevExpress.XtraEditors.LabelControl labelControl1;
	}
}
