namespace Mobius.Tools
{
	partial class RgroupDecomposition
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RgroupDecomposition));
			Mobius.Data.MoleculeMx moleculeMx1 = new Mobius.Data.MoleculeMx();
			this.Help = new DevExpress.XtraEditors.SimpleButton();
			this.ShowCoreStructure = new DevExpress.XtraEditors.CheckEdit();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.Index = new DevExpress.XtraEditors.CheckEdit();
			this.Formula = new DevExpress.XtraEditors.CheckEdit();
			this.Weight = new DevExpress.XtraEditors.CheckEdit();
			this.Smiles = new DevExpress.XtraEditors.CheckEdit();
			this.Structure = new DevExpress.XtraEditors.CheckEdit();
			this.label1 = new DevExpress.XtraEditors.LabelControl();
			this.TerminateOption = new System.Windows.Forms.ComboBox();
			this.Frame1 = new System.Windows.Forms.GroupBox();
			this.AddToFavoritesButton = new DevExpress.XtraEditors.SimpleButton();
			this.RetrieveFavoritesButton = new DevExpress.XtraEditors.SimpleButton();
			this.RetrieveRecentButton = new DevExpress.XtraEditors.SimpleButton();
			this.RetrieveModel = new DevExpress.XtraEditors.SimpleButton();
			this.EditStructure = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.label2 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.QueryMolCtl = new Mobius.ClientComponents.MoleculeControl();
			((System.ComponentModel.ISupportInitialize)(this.ShowCoreStructure.Properties)).BeginInit();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Index.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Formula.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Weight.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Smiles.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Structure.Properties)).BeginInit();
			this.Frame1.SuspendLayout();
			this.SuspendLayout();
			// 
			// Help
			// 
			this.Help.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Help.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Help.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Help.Appearance.Options.UseFont = true;
			this.Help.Appearance.Options.UseForeColor = true;
			this.Help.Cursor = System.Windows.Forms.Cursors.Default;
			this.Help.Location = new System.Drawing.Point(549, 439);
			this.Help.Name = "Help";
			this.Help.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Help.Size = new System.Drawing.Size(60, 23);
			this.Help.TabIndex = 74;
			this.Help.Tag = "Cancel";
			this.Help.Text = "Help";
			this.Help.Click += new System.EventHandler(this.Help_Click);
			// 
			// ShowCoreStructure
			// 
			this.ShowCoreStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ShowCoreStructure.EditValue = true;
			this.ShowCoreStructure.Location = new System.Drawing.Point(474, 309);
			this.ShowCoreStructure.Name = "ShowCoreStructure";
			this.ShowCoreStructure.Properties.Appearance.BackColor = System.Drawing.Color.Gray;
			this.ShowCoreStructure.Properties.Appearance.Options.UseBackColor = true;
			this.ShowCoreStructure.Properties.Caption = "Show Core Structure";
			this.ShowCoreStructure.Size = new System.Drawing.Size(126, 19);
			this.ShowCoreStructure.TabIndex = 73;
			this.ShowCoreStructure.Visible = false;
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox3.Controls.Add(this.Index);
			this.groupBox3.Controls.Add(this.Formula);
			this.groupBox3.Controls.Add(this.Weight);
			this.groupBox3.Controls.Add(this.Smiles);
			this.groupBox3.Controls.Add(this.Structure);
			this.groupBox3.Location = new System.Drawing.Point(412, 122);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(195, 121);
			this.groupBox3.TabIndex = 72;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Substituent Display";
			// 
			// Index
			// 
			this.Index.Location = new System.Drawing.Point(10, 94);
			this.Index.Name = "Index";
			this.Index.Properties.Caption = "Substituent Index";
			this.Index.Size = new System.Drawing.Size(112, 19);
			this.Index.TabIndex = 50;
			// 
			// Formula
			// 
			this.Formula.Location = new System.Drawing.Point(10, 56);
			this.Formula.Name = "Formula";
			this.Formula.Properties.Caption = "Mol. Formula";
			this.Formula.Size = new System.Drawing.Size(87, 19);
			this.Formula.TabIndex = 49;
			// 
			// Weight
			// 
			this.Weight.Location = new System.Drawing.Point(10, 75);
			this.Weight.Name = "Weight";
			this.Weight.Properties.Caption = "Mol. Weight";
			this.Weight.Size = new System.Drawing.Size(83, 19);
			this.Weight.TabIndex = 48;
			// 
			// Smiles
			// 
			this.Smiles.Location = new System.Drawing.Point(10, 37);
			this.Smiles.Name = "Smiles";
			this.Smiles.Properties.Caption = "Smiles";
			this.Smiles.Size = new System.Drawing.Size(55, 19);
			this.Smiles.TabIndex = 46;
			// 
			// Structure
			// 
			this.Structure.EditValue = true;
			this.Structure.Location = new System.Drawing.Point(10, 18);
			this.Structure.Name = "Structure";
			this.Structure.Properties.Caption = "Structure";
			this.Structure.Size = new System.Drawing.Size(71, 19);
			this.Structure.TabIndex = 45;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.Appearance.BackColor = System.Drawing.SystemColors.ControlDark;
			this.label1.Appearance.Options.UseBackColor = true;
			this.label1.Location = new System.Drawing.Point(455, 334);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(111, 13);
			this.label1.TabIndex = 70;
			this.label1.Text = "Include which mapings:";
			this.label1.Visible = false;
			// 
			// TerminateOption
			// 
			this.TerminateOption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.TerminateOption.BackColor = System.Drawing.SystemColors.ControlDark;
			this.TerminateOption.FormattingEnabled = true;
			this.TerminateOption.Items.AddRange(new object[] {
            "First mapping",
            "All mappings"});
			this.TerminateOption.Location = new System.Drawing.Point(467, 350);
			this.TerminateOption.Name = "TerminateOption";
			this.TerminateOption.Size = new System.Drawing.Size(133, 21);
			this.TerminateOption.TabIndex = 68;
			this.TerminateOption.Visible = false;
			// 
			// Frame1
			// 
			this.Frame1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Frame1.BackColor = System.Drawing.Color.Transparent;
			this.Frame1.Controls.Add(this.QueryMolCtl);
			this.Frame1.Controls.Add(this.AddToFavoritesButton);
			this.Frame1.Controls.Add(this.RetrieveFavoritesButton);
			this.Frame1.Controls.Add(this.RetrieveRecentButton);
			this.Frame1.Controls.Add(this.RetrieveModel);
			this.Frame1.Controls.Add(this.EditStructure);
			this.Frame1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Frame1.Location = new System.Drawing.Point(6, 122);
			this.Frame1.Name = "Frame1";
			this.Frame1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Frame1.Size = new System.Drawing.Size(391, 301);
			this.Frame1.TabIndex = 66;
			this.Frame1.TabStop = false;
			this.Frame1.Text = "Core Structure with R-groups";
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
			this.AddToFavoritesButton.Location = new System.Drawing.Point(175, 268);
			this.AddToFavoritesButton.Name = "AddToFavoritesButton";
			this.AddToFavoritesButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.AddToFavoritesButton.Size = new System.Drawing.Size(16, 22);
			this.AddToFavoritesButton.TabIndex = 298;
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
			this.RetrieveFavoritesButton.Location = new System.Drawing.Point(153, 268);
			this.RetrieveFavoritesButton.Name = "RetrieveFavoritesButton";
			this.RetrieveFavoritesButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RetrieveFavoritesButton.Size = new System.Drawing.Size(22, 22);
			this.RetrieveFavoritesButton.TabIndex = 297;
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
			this.RetrieveRecentButton.Location = new System.Drawing.Point(125, 268);
			this.RetrieveRecentButton.Name = "RetrieveRecentButton";
			this.RetrieveRecentButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RetrieveRecentButton.Size = new System.Drawing.Size(22, 22);
			this.RetrieveRecentButton.TabIndex = 296;
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
			this.RetrieveModel.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("RetrieveModel.ImageOptions.Image")));
			this.RetrieveModel.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.RightCenter;
			this.RetrieveModel.Location = new System.Drawing.Point(9, 268);
			this.RetrieveModel.Name = "RetrieveModel";
			this.RetrieveModel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RetrieveModel.Size = new System.Drawing.Size(110, 22);
			this.RetrieveModel.TabIndex = 295;
			this.RetrieveModel.Tag = "RetrieveModel";
			this.RetrieveModel.Text = "Retrieve Structure";
			this.RetrieveModel.Click += new System.EventHandler(this.RetrieveModel_Click);
			// 
			// EditStructure
			// 
			this.EditStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.EditStructure.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EditStructure.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.EditStructure.Appearance.Options.UseFont = true;
			this.EditStructure.Appearance.Options.UseForeColor = true;
			this.EditStructure.Cursor = System.Windows.Forms.Cursors.Default;
			this.EditStructure.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("EditStructure.ImageOptions.Image")));
			this.EditStructure.Location = new System.Drawing.Point(199, 268);
			this.EditStructure.Name = "EditStructure";
			this.EditStructure.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.EditStructure.Size = new System.Drawing.Size(56, 22);
			this.EditStructure.TabIndex = 33;
			this.EditStructure.Tag = "EditStructure";
			this.EditStructure.Text = "Edit";
			this.EditStructure.Click += new System.EventHandler(this.EditStructure_Click);
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
			this.Cancel.Location = new System.Drawing.Point(483, 439);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(60, 23);
			this.Cancel.TabIndex = 65;
			this.Cancel.Tag = "Cancel";
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(417, 439);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(60, 23);
			this.OK.TabIndex = 64;
			this.OK.Tag = "OK";
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.Appearance.BackColor = System.Drawing.Color.LightSteelBlue;
			this.label2.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Appearance.Options.UseBackColor = true;
			this.label2.Appearance.Options.UseFont = true;
			this.label2.Appearance.Options.UseTextOptions = true;
			this.label2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.label2.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.label2.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.label2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.label2.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
			this.label2.Location = new System.Drawing.Point(0, 0);
			this.label2.Name = "label2";
			this.label2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.label2.Size = new System.Drawing.Size(614, 105);
			this.label2.TabIndex = 71;
			this.label2.Text = resources.GetString("label2.Text");
			// 
			// labelControl2
			// 
			this.labelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.LineVisible = true;
			this.labelControl2.Location = new System.Drawing.Point(0, 428);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(614, 4);
			this.labelControl2.TabIndex = 76;
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(0, 104);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(614, 4);
			this.labelControl1.TabIndex = 75;
			// 
			// QueryMolCtl
			// 
			this.QueryMolCtl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.QueryMolCtl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.QueryMolCtl.Location = new System.Drawing.Point(9, 20);
			this.QueryMolCtl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
			this.QueryMolCtl.Molecule = moleculeMx1;
			this.QueryMolCtl.Name = "QueryMolCtl";
			this.QueryMolCtl.Size = new System.Drawing.Size(374, 243);
			this.QueryMolCtl.TabIndex = 77;
			// 
			// RgroupDecomposition
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(614, 467);
			this.Controls.Add(this.labelControl2);
			this.Controls.Add(this.Help);
			this.Controls.Add(this.ShowCoreStructure);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.TerminateOption);
			this.Controls.Add(this.Frame1);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.labelControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "RgroupDecomposition";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "R-group Decomposition Table";
			((System.ComponentModel.ISupportInitialize)(this.ShowCoreStructure.Properties)).EndInit();
			this.groupBox3.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Index.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Formula.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Weight.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Smiles.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Structure.Properties)).EndInit();
			this.Frame1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public DevExpress.XtraEditors.SimpleButton Help;
		public DevExpress.XtraEditors.CheckEdit ShowCoreStructure;
		public System.Windows.Forms.GroupBox groupBox3;
		public DevExpress.XtraEditors.CheckEdit Index;
		public DevExpress.XtraEditors.CheckEdit Formula;
		public DevExpress.XtraEditors.CheckEdit Weight;
		public DevExpress.XtraEditors.CheckEdit Smiles;
		public DevExpress.XtraEditors.CheckEdit Structure;
		private DevExpress.XtraEditors.LabelControl label1;
		public System.Windows.Forms.ComboBox TerminateOption;
		public System.Windows.Forms.GroupBox Frame1;
		public DevExpress.XtraEditors.SimpleButton EditStructure;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.SimpleButton OK;
		private DevExpress.XtraEditors.LabelControl label2;
		private DevExpress.XtraEditors.LabelControl labelControl2;
		private DevExpress.XtraEditors.LabelControl labelControl1;
		public DevExpress.XtraEditors.SimpleButton AddToFavoritesButton;
		public DevExpress.XtraEditors.SimpleButton RetrieveFavoritesButton;
		public DevExpress.XtraEditors.SimpleButton RetrieveRecentButton;
		public DevExpress.XtraEditors.SimpleButton RetrieveModel;
		private ClientComponents.MoleculeControl QueryMolCtl;
	}
}