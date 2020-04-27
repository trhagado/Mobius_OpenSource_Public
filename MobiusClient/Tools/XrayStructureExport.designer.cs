namespace Mobius.Tools
{
	partial class XrayStructureExport
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;


		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XrayStructureExport));
            this.CancelBut = new DevExpress.XtraEditors.SimpleButton();
            this.OKButton = new DevExpress.XtraEditors.SimpleButton();
            this.Browse = new DevExpress.XtraEditors.SimpleButton();
            this.FileName = new DevExpress.XtraEditors.TextEdit();
            this.FileFolderLabel = new DevExpress.XtraEditors.LabelControl();
            this.Frame1 = new System.Windows.Forms.GroupBox();
            this.ExportToFile = new DevExpress.XtraEditors.CheckEdit();
            this.ExportToMOE = new DevExpress.XtraEditors.CheckEdit();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ExportMarked = new DevExpress.XtraEditors.CheckEdit();
            this.ExportSingle = new DevExpress.XtraEditors.CheckEdit();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.UseExistingMoeInstance = new DevExpress.XtraEditors.CheckEdit();
            this.SetMoePath = new DevExpress.XtraEditors.SimpleButton();
            this.ExportToPyMol = new DevExpress.XtraEditors.CheckEdit();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.IncludeElectronDensityPyMol = new DevExpress.XtraEditors.CheckEdit();
            ((System.ComponentModel.ISupportInitialize)(this.FileName.Properties)).BeginInit();
            this.Frame1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ExportToFile.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ExportToMOE.Properties)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ExportMarked.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ExportSingle.Properties)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.UseExistingMoeInstance.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ExportToPyMol.Properties)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IncludeElectronDensityPyMol.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // CancelBut
            // 
            this.CancelBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelBut.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBut.Location = new System.Drawing.Point(383, 318);
            this.CancelBut.Name = "CancelBut";
            this.CancelBut.Size = new System.Drawing.Size(70, 24);
            this.CancelBut.TabIndex = 174;
            this.CancelBut.Text = "Cancel";
            this.CancelBut.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.Location = new System.Drawing.Point(307, 318);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(70, 24);
            this.OKButton.TabIndex = 175;
            this.OKButton.Text = "&OK";
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // Browse
            // 
            this.Browse.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Browse.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Browse.Appearance.Options.UseFont = true;
            this.Browse.Appearance.Options.UseForeColor = true;
            this.Browse.Cursor = System.Windows.Forms.Cursors.Default;
            this.Browse.Location = new System.Drawing.Point(348, 24);
            this.Browse.Name = "Browse";
            this.Browse.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Browse.Size = new System.Drawing.Size(68, 19);
            this.Browse.TabIndex = 194;
            this.Browse.Text = "&Browse...";
            this.Browse.Click += new System.EventHandler(this.Browse_Click);
            // 
            // FileName
            // 
            this.FileName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.FileName.Location = new System.Drawing.Point(72, 24);
            this.FileName.Name = "FileName";
            this.FileName.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
            this.FileName.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileName.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
            this.FileName.Properties.Appearance.Options.UseBackColor = true;
            this.FileName.Properties.Appearance.Options.UseFont = true;
            this.FileName.Properties.Appearance.Options.UseForeColor = true;
            this.FileName.Properties.ReadOnly = true;
            this.FileName.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.FileName.Size = new System.Drawing.Size(271, 20);
            this.FileName.TabIndex = 193;
            this.FileName.Tag = "Title";
            // 
            // FileFolderLabel
            // 
            this.FileFolderLabel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileFolderLabel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FileFolderLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.FileFolderLabel.Location = new System.Drawing.Point(7, 25);
            this.FileFolderLabel.Name = "FileFolderLabel";
            this.FileFolderLabel.Size = new System.Drawing.Size(57, 13);
            this.FileFolderLabel.TabIndex = 195;
            this.FileFolderLabel.Text = "   File name:";
            this.FileFolderLabel.Click += new System.EventHandler(this.label3_Click);
            // 
            // Frame1
            // 
            this.Frame1.Controls.Add(this.Browse);
            this.Frame1.Controls.Add(this.FileName);
            this.Frame1.Controls.Add(this.FileFolderLabel);
            this.Frame1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Frame1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Frame1.Location = new System.Drawing.Point(10, 152);
            this.Frame1.Name = "Frame1";
            this.Frame1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Frame1.Size = new System.Drawing.Size(426, 59);
            this.Frame1.TabIndex = 196;
            this.Frame1.TabStop = false;
            this.Frame1.Text = "                                       ";
            // 
            // ExportToFile
            // 
            this.ExportToFile.Cursor = System.Windows.Forms.Cursors.Default;
            this.ExportToFile.Location = new System.Drawing.Point(18, 149);
            this.ExportToFile.Name = "ExportToFile";
            this.ExportToFile.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExportToFile.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ExportToFile.Properties.Appearance.Options.UseFont = true;
            this.ExportToFile.Properties.Appearance.Options.UseForeColor = true;
            this.ExportToFile.Properties.Caption = "Individual PC file(s)";
            this.ExportToFile.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.ExportToFile.Properties.RadioGroupIndex = 1;
            this.ExportToFile.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ExportToFile.Size = new System.Drawing.Size(114, 19);
            this.ExportToFile.TabIndex = 8;
            this.ExportToFile.TabStop = false;
            this.ExportToFile.CheckedChanged += new System.EventHandler(this.ExportToFile_CheckedChanged);
            // 
            // ExportToMOE
            // 
            this.ExportToMOE.Cursor = System.Windows.Forms.Cursors.Default;
            this.ExportToMOE.Location = new System.Drawing.Point(19, 84);
            this.ExportToMOE.Name = "ExportToMOE";
            this.ExportToMOE.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExportToMOE.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ExportToMOE.Properties.Appearance.Options.UseFont = true;
            this.ExportToMOE.Properties.Appearance.Options.UseForeColor = true;
            this.ExportToMOE.Properties.Caption = "MOE (Molecular Operating Environment)";
            this.ExportToMOE.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.ExportToMOE.Properties.RadioGroupIndex = 1;
            this.ExportToMOE.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ExportToMOE.Size = new System.Drawing.Size(214, 19);
            this.ExportToMOE.TabIndex = 8;
            this.ExportToMOE.TabStop = false;
            this.ExportToMOE.CheckedChanged += new System.EventHandler(this.ExportToVida_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox3.Controls.Add(this.ExportMarked);
            this.groupBox3.Controls.Add(this.ExportSingle);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.ForeColor = System.Drawing.SystemColors.ControlText;
            this.groupBox3.Location = new System.Drawing.Point(9, 242);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.groupBox3.Size = new System.Drawing.Size(444, 71);
            this.groupBox3.TabIndex = 198;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Structures to export";
            // 
            // ExportMarked
            // 
            this.ExportMarked.Cursor = System.Windows.Forms.Cursors.Default;
            this.ExportMarked.Location = new System.Drawing.Point(10, 42);
            this.ExportMarked.Name = "ExportMarked";
            this.ExportMarked.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.ExportMarked.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExportMarked.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ExportMarked.Properties.Appearance.Options.UseBackColor = true;
            this.ExportMarked.Properties.Appearance.Options.UseFont = true;
            this.ExportMarked.Properties.Appearance.Options.UseForeColor = true;
            this.ExportMarked.Properties.Caption = "All marked structures";
            this.ExportMarked.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.ExportMarked.Properties.RadioGroupIndex = 2;
            this.ExportMarked.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ExportMarked.Size = new System.Drawing.Size(123, 19);
            this.ExportMarked.TabIndex = 8;
            this.ExportMarked.TabStop = false;
            // 
            // ExportSingle
            // 
            this.ExportSingle.Cursor = System.Windows.Forms.Cursors.Default;
            this.ExportSingle.EditValue = true;
            this.ExportSingle.Location = new System.Drawing.Point(10, 19);
            this.ExportSingle.Name = "ExportSingle";
            this.ExportSingle.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.ExportSingle.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExportSingle.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ExportSingle.Properties.Appearance.Options.UseBackColor = true;
            this.ExportSingle.Properties.Appearance.Options.UseFont = true;
            this.ExportSingle.Properties.Appearance.Options.UseForeColor = true;
            this.ExportSingle.Properties.Caption = "Single selected structure";
            this.ExportSingle.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.ExportSingle.Properties.RadioGroupIndex = 2;
            this.ExportSingle.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ExportSingle.Size = new System.Drawing.Size(141, 19);
            this.ExportSingle.TabIndex = 9;
            this.ExportSingle.CheckedChanged += new System.EventHandler(this.ExportSingle_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ExportToMOE);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.ExportToFile);
            this.groupBox1.Controls.Add(this.ExportToPyMol);
            this.groupBox1.Controls.Add(this.groupBox4);
            this.groupBox1.Controls.Add(this.Frame1);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.groupBox1.Location = new System.Drawing.Point(9, 9);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.groupBox1.Size = new System.Drawing.Size(444, 219);
            this.groupBox1.TabIndex = 197;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Export to:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.UseExistingMoeInstance);
            this.groupBox2.Controls.Add(this.SetMoePath);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.groupBox2.Location = new System.Drawing.Point(12, 87);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.groupBox2.Size = new System.Drawing.Size(426, 51);
            this.groupBox2.TabIndex = 199;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "                    ";
            // 
            // UseExistingMoeInstance
            // 
            this.UseExistingMoeInstance.AllowDrop = true;
            this.UseExistingMoeInstance.Cursor = System.Windows.Forms.Cursors.Default;
            this.UseExistingMoeInstance.EditValue = true;
            this.UseExistingMoeInstance.Location = new System.Drawing.Point(24, 22);
            this.UseExistingMoeInstance.Name = "UseExistingMoeInstance";
            this.UseExistingMoeInstance.Properties.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UseExistingMoeInstance.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.UseExistingMoeInstance.Properties.Appearance.Options.UseFont = true;
            this.UseExistingMoeInstance.Properties.Appearance.Options.UseForeColor = true;
            this.UseExistingMoeInstance.Properties.Caption = "Load into current MOE window (if one exists)";
            this.UseExistingMoeInstance.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.UseExistingMoeInstance.Size = new System.Drawing.Size(262, 19);
            this.UseExistingMoeInstance.TabIndex = 191;
            // 
            // SetMoePath
            // 
            this.SetMoePath.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SetMoePath.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.SetMoePath.Appearance.Options.UseFont = true;
            this.SetMoePath.Appearance.Options.UseForeColor = true;
            this.SetMoePath.Cursor = System.Windows.Forms.Cursors.Default;
            this.SetMoePath.Image = ((System.Drawing.Image)(resources.GetObject("SetMoePath.Image")));
            this.SetMoePath.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.SetMoePath.Location = new System.Drawing.Point(392, 18);
            this.SetMoePath.Name = "SetMoePath";
            this.SetMoePath.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.SetMoePath.Size = new System.Drawing.Size(22, 22);
            this.SetMoePath.TabIndex = 196;
            this.SetMoePath.Text = "...";
            this.SetMoePath.ToolTip = "Set location of MOE executable";
            this.SetMoePath.Click += new System.EventHandler(this.SetMoeExecutable_Click);
            // 
            // ExportToPyMol
            // 
            this.ExportToPyMol.Cursor = System.Windows.Forms.Cursors.Default;
            this.ExportToPyMol.EditValue = true;
            this.ExportToPyMol.Location = new System.Drawing.Point(18, 21);
            this.ExportToPyMol.Name = "ExportToPyMol";
            this.ExportToPyMol.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExportToPyMol.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ExportToPyMol.Properties.Appearance.Options.UseFont = true;
            this.ExportToPyMol.Properties.Appearance.Options.UseForeColor = true;
            this.ExportToPyMol.Properties.Caption = "PyMOL structure visualizer (aligned and overlaid structures)";
            this.ExportToPyMol.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.ExportToPyMol.Properties.RadioGroupIndex = 1;
            this.ExportToPyMol.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ExportToPyMol.Size = new System.Drawing.Size(303, 19);
            this.ExportToPyMol.TabIndex = 8;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.IncludeElectronDensityPyMol);
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.ForeColor = System.Drawing.SystemColors.ControlText;
            this.groupBox4.Location = new System.Drawing.Point(12, 24);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.groupBox4.Size = new System.Drawing.Size(426, 51);
            this.groupBox4.TabIndex = 198;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "                                                                                 " +
    "                   ";
            // 
            // IncludeElectronDensityPyMol
            // 
            this.IncludeElectronDensityPyMol.AllowDrop = true;
            this.IncludeElectronDensityPyMol.Cursor = System.Windows.Forms.Cursors.Default;
            this.IncludeElectronDensityPyMol.Location = new System.Drawing.Point(24, 22);
            this.IncludeElectronDensityPyMol.Name = "IncludeElectronDensityPyMol";
            this.IncludeElectronDensityPyMol.Properties.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IncludeElectronDensityPyMol.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.IncludeElectronDensityPyMol.Properties.Appearance.Options.UseFont = true;
            this.IncludeElectronDensityPyMol.Properties.Appearance.Options.UseForeColor = true;
            this.IncludeElectronDensityPyMol.Properties.Caption = "Include electron density map";
            this.IncludeElectronDensityPyMol.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.IncludeElectronDensityPyMol.Size = new System.Drawing.Size(163, 19);
            this.IncludeElectronDensityPyMol.TabIndex = 191;
            // 
            // XrayStructureExport
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelBut;
            this.ClientSize = new System.Drawing.Size(457, 347);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.CancelBut);
            this.Controls.Add(this.OKButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "XrayStructureExport";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Export X-Ray structures";
            this.Activated += new System.EventHandler(this.XrayStructureExportForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SetupXrayStructureExport_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.FileName.Properties)).EndInit();
            this.Frame1.ResumeLayout(false);
            this.Frame1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ExportToFile.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ExportToMOE.Properties)).EndInit();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ExportMarked.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ExportSingle.Properties)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.UseExistingMoeInstance.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ExportToPyMol.Properties)).EndInit();
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.IncludeElectronDensityPyMol.Properties)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.SimpleButton CancelBut;
		public DevExpress.XtraEditors.SimpleButton OKButton;
		public DevExpress.XtraEditors.SimpleButton Browse;
		public DevExpress.XtraEditors.TextEdit FileName;
		public DevExpress.XtraEditors.LabelControl FileFolderLabel;
		public System.Windows.Forms.GroupBox Frame1;
		public DevExpress.XtraEditors.CheckEdit ExportToFile;
		public DevExpress.XtraEditors.CheckEdit ExportToMOE;
		public System.Windows.Forms.GroupBox groupBox3;
		public DevExpress.XtraEditors.CheckEdit ExportSingle;
		public DevExpress.XtraEditors.CheckEdit ExportMarked;
		public System.Windows.Forms.GroupBox groupBox1;
		public System.Windows.Forms.GroupBox groupBox4;
		public DevExpress.XtraEditors.CheckEdit ExportToPyMol;
		public DevExpress.XtraEditors.CheckEdit IncludeElectronDensityPyMol;
		public DevExpress.XtraEditors.SimpleButton SetMoePath;
		public System.Windows.Forms.GroupBox groupBox2;
		public DevExpress.XtraEditors.CheckEdit UseExistingMoeInstance;


	}
}