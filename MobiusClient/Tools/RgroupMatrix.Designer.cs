namespace Mobius.Tools
{
	partial class RgroupMatrix
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RgroupMatrix));
			Mobius.Data.MoleculeMx moleculeMx1 = new Mobius.Data.MoleculeMx();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.Help = new DevExpress.XtraEditors.SimpleButton();
			this.ShowCoreStructure = new DevExpress.XtraEditors.CheckEdit();
			this.HideRepeatingSubstituents = new DevExpress.XtraEditors.CheckEdit();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.Index = new DevExpress.XtraEditors.CheckEdit();
			this.Formula = new DevExpress.XtraEditors.CheckEdit();
			this.Weight = new DevExpress.XtraEditors.CheckEdit();
			this.Smiles = new DevExpress.XtraEditors.CheckEdit();
			this.Structure = new DevExpress.XtraEditors.CheckEdit();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.TwoD = new DevExpress.XtraEditors.CheckEdit();
			this.OneD = new DevExpress.XtraEditors.CheckEdit();
			this.BrowseMatrices = new DevExpress.XtraEditors.SimpleButton();
			this.label7 = new DevExpress.XtraEditors.LabelControl();
			this.MatrixName = new DevExpress.XtraEditors.TextEdit();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label2 = new DevExpress.XtraEditors.LabelControl();
			this.Frame1 = new System.Windows.Forms.GroupBox();
			this.EditStructure = new DevExpress.XtraEditors.SimpleButton();
			this.RetrieveModel = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.SQuery = new Mobius.ClientComponents.MoleculeControl();
			((System.ComponentModel.ISupportInitialize)(this.ShowCoreStructure.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HideRepeatingSubstituents.Properties)).BeginInit();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Index.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Formula.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Weight.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Smiles.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Structure.Properties)).BeginInit();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TwoD.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OneD.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MatrixName.Properties)).BeginInit();
			this.Frame1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox4
			// 
			this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox4.BackColor = System.Drawing.SystemColors.Control;
			this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBox4.ForeColor = System.Drawing.SystemColors.ControlText;
			this.groupBox4.Location = new System.Drawing.Point(1, 428);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.groupBox4.Size = new System.Drawing.Size(643, 2);
			this.groupBox4.TabIndex = 69;
			this.groupBox4.TabStop = false;
			// 
			// Help
			// 
			this.Help.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Help.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Help.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Help.Appearance.Options.UseFont = true;
			this.Help.Appearance.Options.UseForeColor = true;
			this.Help.Cursor = System.Windows.Forms.Cursors.Default;
			this.Help.Location = new System.Drawing.Point(579, 435);
			this.Help.Name = "Help";
			this.Help.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Help.Size = new System.Drawing.Size(60, 23);
			this.Help.TabIndex = 79;
			this.Help.Tag = "Cancel";
			this.Help.Text = "Help";
			this.Help.Click += new System.EventHandler(this.Help_Click);
			// 
			// ShowCoreStructure
			// 
			this.ShowCoreStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ShowCoreStructure.EditValue = true;
			this.ShowCoreStructure.Location = new System.Drawing.Point(450, 328);
			this.ShowCoreStructure.Name = "ShowCoreStructure";
			this.ShowCoreStructure.Properties.Caption = "Show Core Structure";
			this.ShowCoreStructure.Size = new System.Drawing.Size(126, 19);
			this.ShowCoreStructure.TabIndex = 78;
			// 
			// HideRepeatingSubstituents
			// 
			this.HideRepeatingSubstituents.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.HideRepeatingSubstituents.EditValue = true;
			this.HideRepeatingSubstituents.Location = new System.Drawing.Point(450, 303);
			this.HideRepeatingSubstituents.Name = "HideRepeatingSubstituents";
			this.HideRepeatingSubstituents.Properties.Caption = "Hide Repeating Substituents";
			this.HideRepeatingSubstituents.Size = new System.Drawing.Size(162, 19);
			this.HideRepeatingSubstituents.TabIndex = 77;
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox3.Controls.Add(this.Index);
			this.groupBox3.Controls.Add(this.Formula);
			this.groupBox3.Controls.Add(this.Weight);
			this.groupBox3.Controls.Add(this.Smiles);
			this.groupBox3.Controls.Add(this.Structure);
			this.groupBox3.Location = new System.Drawing.Point(445, 89);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(194, 121);
			this.groupBox3.TabIndex = 76;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Substituent Display";
			// 
			// Index
			// 
			this.Index.Location = new System.Drawing.Point(10, 95);
			this.Index.Name = "Index";
			this.Index.Properties.Caption = "Substituent Index";
			this.Index.Size = new System.Drawing.Size(112, 19);
			this.Index.TabIndex = 50;
			// 
			// Formula
			// 
			this.Formula.Location = new System.Drawing.Point(10, 57);
			this.Formula.Name = "Formula";
			this.Formula.Properties.Caption = "Mol. Formula";
			this.Formula.Size = new System.Drawing.Size(87, 19);
			this.Formula.TabIndex = 49;
			// 
			// Weight
			// 
			this.Weight.Location = new System.Drawing.Point(10, 76);
			this.Weight.Name = "Weight";
			this.Weight.Properties.Caption = "Mol. Weight";
			this.Weight.Size = new System.Drawing.Size(83, 19);
			this.Weight.TabIndex = 48;
			// 
			// Smiles
			// 
			this.Smiles.Location = new System.Drawing.Point(10, 38);
			this.Smiles.Name = "Smiles";
			this.Smiles.Properties.Caption = "Smiles";
			this.Smiles.Size = new System.Drawing.Size(55, 19);
			this.Smiles.TabIndex = 46;
			// 
			// Structure
			// 
			this.Structure.EditValue = true;
			this.Structure.Enabled = false;
			this.Structure.Location = new System.Drawing.Point(10, 19);
			this.Structure.Name = "Structure";
			this.Structure.Properties.Caption = "Structure";
			this.Structure.Size = new System.Drawing.Size(71, 19);
			this.Structure.TabIndex = 45;
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.TwoD);
			this.groupBox2.Controls.Add(this.OneD);
			this.groupBox2.Location = new System.Drawing.Point(445, 227);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(194, 70);
			this.groupBox2.TabIndex = 75;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Matrix Dimensionality";
			// 
			// TwoD
			// 
			this.TwoD.EditValue = true;
			this.TwoD.Location = new System.Drawing.Point(10, 42);
			this.TwoD.Name = "TwoD";
			this.TwoD.Properties.Caption = "2D";
			this.TwoD.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.TwoD.Properties.RadioGroupIndex = 1;
			this.TwoD.Size = new System.Drawing.Size(38, 19);
			this.TwoD.TabIndex = 1;
			// 
			// OneD
			// 
			this.OneD.Location = new System.Drawing.Point(10, 19);
			this.OneD.Name = "OneD";
			this.OneD.Properties.Caption = "1D";
			this.OneD.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.OneD.Properties.RadioGroupIndex = 1;
			this.OneD.Size = new System.Drawing.Size(38, 19);
			this.OneD.TabIndex = 0;
			this.OneD.TabStop = false;
			// 
			// BrowseMatrices
			// 
			this.BrowseMatrices.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.BrowseMatrices.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.BrowseMatrices.Appearance.Options.UseFont = true;
			this.BrowseMatrices.Appearance.Options.UseForeColor = true;
			this.BrowseMatrices.Cursor = System.Windows.Forms.Cursors.Default;
			this.BrowseMatrices.Location = new System.Drawing.Point(212, 458);
			this.BrowseMatrices.Name = "BrowseMatrices";
			this.BrowseMatrices.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.BrowseMatrices.Size = new System.Drawing.Size(59, 20);
			this.BrowseMatrices.TabIndex = 73;
			this.BrowseMatrices.Tag = "OK";
			this.BrowseMatrices.Text = "Browse...";
			this.BrowseMatrices.Visible = false;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(4, 461);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(31, 13);
			this.label7.TabIndex = 72;
			this.label7.Text = "Name:";
			this.label7.Visible = false;
			// 
			// MatrixName
			// 
			this.MatrixName.EditValue = "R-group Matrix 1";
			this.MatrixName.Location = new System.Drawing.Point(48, 458);
			this.MatrixName.Name = "MatrixName";
			this.MatrixName.Size = new System.Drawing.Size(158, 20);
			this.MatrixName.TabIndex = 71;
			this.MatrixName.Visible = false;
			// 
			// groupBox1
			// 
			this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
			this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBox1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.groupBox1.Location = new System.Drawing.Point(0, 79);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.groupBox1.Size = new System.Drawing.Size(480, 2);
			this.groupBox1.TabIndex = 68;
			this.groupBox1.TabStop = false;
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
			this.label2.Padding = new System.Windows.Forms.Padding(4);
			this.label2.Size = new System.Drawing.Size(643, 80);
			this.label2.TabIndex = 70;
			this.label2.Text = resources.GetString("label2.Text");
			// 
			// Frame1
			// 
			this.Frame1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Frame1.BackColor = System.Drawing.Color.Transparent;
			this.Frame1.Controls.Add(this.SQuery);
			this.Frame1.Controls.Add(this.EditStructure);
			this.Frame1.Controls.Add(this.RetrieveModel);
			this.Frame1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Frame1.Location = new System.Drawing.Point(7, 89);
			this.Frame1.Name = "Frame1";
			this.Frame1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Frame1.Size = new System.Drawing.Size(429, 331);
			this.Frame1.TabIndex = 67;
			this.Frame1.TabStop = false;
			this.Frame1.Text = "Core Structure with R-groups";
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
			this.EditStructure.Location = new System.Drawing.Point(317, 301);
			this.EditStructure.Name = "EditStructure";
			this.EditStructure.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.EditStructure.Size = new System.Drawing.Size(104, 24);
			this.EditStructure.TabIndex = 33;
			this.EditStructure.Tag = "EditStructure";
			this.EditStructure.Text = "&Edit Structure";
			this.EditStructure.Click += new System.EventHandler(this.EditStructure_Click);
			// 
			// RetrieveModel
			// 
			this.RetrieveModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RetrieveModel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RetrieveModel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.RetrieveModel.Appearance.Options.UseFont = true;
			this.RetrieveModel.Appearance.Options.UseForeColor = true;
			this.RetrieveModel.Cursor = System.Windows.Forms.Cursors.Default;
			this.RetrieveModel.Location = new System.Drawing.Point(7, 299);
			this.RetrieveModel.Name = "RetrieveModel";
			this.RetrieveModel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RetrieveModel.Size = new System.Drawing.Size(112, 24);
			this.RetrieveModel.TabIndex = 32;
			this.RetrieveModel.Tag = "RetrieveModel";
			this.RetrieveModel.Text = "&Retrieve Structure";
			this.RetrieveModel.Click += new System.EventHandler(this.RetrieveModel_Click);
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
			this.Cancel.Location = new System.Drawing.Point(512, 435);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(60, 23);
			this.Cancel.TabIndex = 66;
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
			this.OK.Location = new System.Drawing.Point(445, 435);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(60, 23);
			this.OK.TabIndex = 65;
			this.OK.Tag = "OK";
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// SQuery
			// 
			this.SQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SQuery.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.SQuery.Location = new System.Drawing.Point(7, 18);
			this.SQuery.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
			this.SQuery.Molecule = moleculeMx1;
			this.SQuery.Name = "SQuery";
			this.SQuery.Size = new System.Drawing.Size(414, 276);
			this.SQuery.TabIndex = 34;
			// 
			// RgroupMatrix
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(643, 462);
			this.Controls.Add(this.Help);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.ShowCoreStructure);
			this.Controls.Add(this.HideRepeatingSubstituents);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.BrowseMatrices);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.MatrixName);
			this.Controls.Add(this.Frame1);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.OK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RgroupMatrix";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "R-group Matrix";
			((System.ComponentModel.ISupportInitialize)(this.ShowCoreStructure.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HideRepeatingSubstituents.Properties)).EndInit();
			this.groupBox3.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Index.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Formula.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Weight.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Smiles.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Structure.Properties)).EndInit();
			this.groupBox2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TwoD.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OneD.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MatrixName.Properties)).EndInit();
			this.Frame1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public System.Windows.Forms.GroupBox groupBox4;
		public DevExpress.XtraEditors.SimpleButton Help;
		public DevExpress.XtraEditors.CheckEdit ShowCoreStructure;
		public DevExpress.XtraEditors.CheckEdit HideRepeatingSubstituents;
		public System.Windows.Forms.GroupBox groupBox3;
		public DevExpress.XtraEditors.CheckEdit Index;
		public DevExpress.XtraEditors.CheckEdit Formula;
		public DevExpress.XtraEditors.CheckEdit Weight;
		public DevExpress.XtraEditors.CheckEdit Smiles;
		public DevExpress.XtraEditors.CheckEdit Structure;
		public System.Windows.Forms.GroupBox groupBox2;
		public DevExpress.XtraEditors.CheckEdit TwoD;
		public DevExpress.XtraEditors.CheckEdit OneD;
		public DevExpress.XtraEditors.SimpleButton BrowseMatrices;
		public DevExpress.XtraEditors.LabelControl label7;
		public DevExpress.XtraEditors.TextEdit MatrixName;
		public System.Windows.Forms.GroupBox groupBox1;
		public DevExpress.XtraEditors.LabelControl label2;
		public System.Windows.Forms.GroupBox Frame1;
		public DevExpress.XtraEditors.SimpleButton EditStructure;
		public DevExpress.XtraEditors.SimpleButton RetrieveModel;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.SimpleButton OK;
		private ClientComponents.MoleculeControl SQuery;
	}
}