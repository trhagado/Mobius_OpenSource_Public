using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Get import parameters for user data
	/// </summary>

	public class UserDataImportTextFile : XtraForm
	{
		static UserDataImportTextFile Instance;

		string InitialName = ""; // initial name for file when dialog entered
		public string FileFilter = "All files (*.*)|*.*";
		public string DefaultFolder = "";
		public string DefaultExt = ".csv"; 

		public DevExpress.XtraEditors.SimpleButton Browse;
		public DevExpress.XtraEditors.TextEdit FileName;
		public DevExpress.XtraEditors.LabelControl Label1;
		public System.Windows.Forms.GroupBox groupBox1;
		public DevExpress.XtraEditors.CheckEdit HeaderLine;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.LabelControl label2;
		public DevExpress.XtraEditors.LabelControl label3;
		public DevExpress.XtraEditors.CheckEdit CommaDelim;
		public DevExpress.XtraEditors.CheckEdit SemiDelim;
		public DevExpress.XtraEditors.CheckEdit TabDelim;
		public DevExpress.XtraEditors.CheckEdit SpaceDelim;
		public DevExpress.XtraEditors.CheckEdit MultDelimsAsSingle;
		public DevExpress.XtraEditors.CheckEdit DeleteExistingData;
		public DevExpress.XtraEditors.CheckEdit ImportInBackground;
		public DevExpress.XtraEditors.CheckEdit CheckForFileUpdates;
        private System.Windows.Forms.ImageList Bitmaps16x16;
		public ComboBoxEdit TextQualifier;
		public LabelControl labelControl1;
		public LabelControl labelControl2;
		private IContainer components;

		public UserDataImportTextFile()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			TextQualifier.Text = "\"";
		}

		/// <summary>
		/// Show the dialog
		/// </summary>
		/// <param name="cMt"></param>
		/// <param name="userDatabase"></param>
		/// <returns></returns>

		public new static DialogResult ShowDialog(MetaTable cMt, UserObject uo, bool userDatabase)
		{
			if (Instance == null) Instance = new UserDataImportTextFile();
			return Instance.ShowDialog2(cMt, uo, userDatabase);
		}

		public DialogResult ShowDialog2(MetaTable cMt, UserObject uo, bool userDatabase)
		{
			MetaColumn mc;
			bool headerLineChecked;
			QualifiedNumber qn;
			string tok;

			UserDataImportParms importParms = cMt.ImportParms;

			if (importParms == null)
			{
				importParms = new UserDataImportParms();
				cMt.ImportParms = importParms;
			}

			else // setup for existing
			{
				FileName.Text = importParms.FileName;
				HeaderLine.Checked = importParms.FirstLineHeaders;
				DeleteExistingData.Checked = importParms.DeleteExisting;
				ImportInBackground.Checked = importParms.ImportInBackground;
				CheckForFileUpdates.Checked = importParms.CheckForFileUpdates;

        TabDelim.Checked = (importParms.Delim == '\t');
        CommaDelim.Checked = (importParms.Delim == ',');
        SemiDelim.Checked = (importParms.Delim == ';');
        SpaceDelim.Checked = (importParms.Delim == ' ');
			}

			if (userDatabase) // setup for db import
			{
				Text = "Import Database from File";
				FileFilter =
					"Smiles, CSV (Comma delimited) (*.smi; *.csv)|*.smi; *.csv|All files (*.*)|*.*";
				DefaultExt = "smi";
			}

			else // setup for anotation import
			{
				Text = "Import annotation table data from a text file";
				FileFilter =
					"CSV (Comma delimited)(*.csv)|*.csv|Text(*.txt)|*.txt|All files (*.*)|*.*";
				DefaultExt = "csv";
			}

		GetInput:
			DialogResult dr = ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return dr;

			importParms.FileName = FileName.Text;
			importParms.ClientFileModified = FileUtil.GetFileLastWriteTime(importParms.FileName);

			if (CommaDelim.Checked) importParms.Delim = ',';
			else if (TabDelim.Checked) importParms.Delim = '\t';
			else if (SemiDelim.Checked) importParms.Delim = ';';
			else if (SpaceDelim.Checked) importParms.Delim = ' ';

			importParms.MultDelimsAsSingle = MultDelimsAsSingle.Checked;
			tok = TextQualifier.Text;
			if (tok == "\"") importParms.TextQualifier = '\"';
			else if (tok == "\'") importParms.TextQualifier = '\'';

			importParms.FirstLineHeaders = HeaderLine.Checked;
			UserObjectDao.SetUserParameter( // save setting for next time
			 SS.I.UserName, "AnnotationHeaderLineChecked", importParms.FirstLineHeaders.ToString());

			importParms.DeleteExisting = DeleteExistingData.Checked;
			importParms.DeleteDataOnly = false;
			if (importParms.DeleteExisting && uo.Id > 0)
			{
				string userDataTypeLabel = 	userDatabase ? "database" : "annotation table";
				int dri = MessageBoxMx.ShowWithCustomButtons(
					"Are you sure you want to delete any existing data for this " + userDataTypeLabel + "?", UmlautMobius.String,
					"Yes", "Keep Cols.", "No", null, MessageBoxIcon.Question);
				if (dri == 1) { } // yes
				else if (dri == 2) importParms.DeleteDataOnly = true;
				else goto GetInput;
			}

			importParms.ImportInBackground = ImportInBackground.Checked;
			importParms.CheckForFileUpdates = CheckForFileUpdates.Checked;

			DialogResult = DialogResult.OK;
			return DialogResult;
		}


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserDataImportTextFile));
            this.Browse = new DevExpress.XtraEditors.SimpleButton();
            this.FileName = new DevExpress.XtraEditors.TextEdit();
            this.Label1 = new DevExpress.XtraEditors.LabelControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.SpaceDelim = new DevExpress.XtraEditors.CheckEdit();
            this.TabDelim = new DevExpress.XtraEditors.CheckEdit();
            this.SemiDelim = new DevExpress.XtraEditors.CheckEdit();
            this.CommaDelim = new DevExpress.XtraEditors.CheckEdit();
            this.HeaderLine = new DevExpress.XtraEditors.CheckEdit();
            this.DeleteExistingData = new DevExpress.XtraEditors.CheckEdit();
            this.Cancel = new DevExpress.XtraEditors.SimpleButton();
            this.OK = new DevExpress.XtraEditors.SimpleButton();
            this.label2 = new DevExpress.XtraEditors.LabelControl();
            this.label3 = new DevExpress.XtraEditors.LabelControl();
            this.MultDelimsAsSingle = new DevExpress.XtraEditors.CheckEdit();
            this.ImportInBackground = new DevExpress.XtraEditors.CheckEdit();
            this.CheckForFileUpdates = new DevExpress.XtraEditors.CheckEdit();
            this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
            this.TextQualifier = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.FileName.Properties)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SpaceDelim.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TabDelim.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SemiDelim.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CommaDelim.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.HeaderLine.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DeleteExistingData.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MultDelimsAsSingle.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImportInBackground.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CheckForFileUpdates.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TextQualifier.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // Browse
            // 
            this.Browse.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Browse.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Browse.Appearance.Options.UseFont = true;
            this.Browse.Appearance.Options.UseForeColor = true;
            this.Browse.Cursor = System.Windows.Forms.Cursors.Default;
            this.Browse.Location = new System.Drawing.Point(324, 76);
            this.Browse.Name = "Browse";
            this.Browse.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Browse.Size = new System.Drawing.Size(85, 22);
            this.Browse.TabIndex = 32;
            this.Browse.Text = "&Browse...";
            this.Browse.Click += new System.EventHandler(this.Browse_Click);
            // 
            // FileName
            // 
            this.FileName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.FileName.Location = new System.Drawing.Point(75, 78);
            this.FileName.Name = "FileName";
            this.FileName.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
            this.FileName.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileName.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
            this.FileName.Properties.Appearance.Options.UseBackColor = true;
            this.FileName.Properties.Appearance.Options.UseFont = true;
            this.FileName.Properties.Appearance.Options.UseForeColor = true;
            this.FileName.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.FileName.Size = new System.Drawing.Size(243, 20);
            this.FileName.TabIndex = 31;
            this.FileName.Tag = "Title";
            // 
            // Label1
            // 
            this.Label1.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.Label1.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label1.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label1.Location = new System.Drawing.Point(11, 78);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(50, 13);
            this.Label1.TabIndex = 33;
            this.Label1.Text = "File Name:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.SpaceDelim);
            this.groupBox1.Controls.Add(this.TabDelim);
            this.groupBox1.Controls.Add(this.SemiDelim);
            this.groupBox1.Controls.Add(this.CommaDelim);
            this.groupBox1.Location = new System.Drawing.Point(11, 107);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(174, 67);
            this.groupBox1.TabIndex = 34;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Delimiter";
            // 
            // SpaceDelim
            // 
            this.SpaceDelim.Location = new System.Drawing.Point(96, 41);
            this.SpaceDelim.Name = "SpaceDelim";
            this.SpaceDelim.Properties.AutoWidth = true;
            this.SpaceDelim.Properties.Caption = "Space";
            this.SpaceDelim.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.SpaceDelim.Properties.RadioGroupIndex = 1;
            this.SpaceDelim.Size = new System.Drawing.Size(51, 19);
            this.SpaceDelim.TabIndex = 3;
            this.SpaceDelim.TabStop = false;
            this.SpaceDelim.CheckedChanged += new System.EventHandler(this.SpaceDelim_CheckedChanged);
            // 
            // TabDelim
            // 
            this.TabDelim.Location = new System.Drawing.Point(96, 19);
            this.TabDelim.Name = "TabDelim";
            this.TabDelim.Properties.AutoWidth = true;
            this.TabDelim.Properties.Caption = "Tab";
            this.TabDelim.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.TabDelim.Properties.RadioGroupIndex = 1;
            this.TabDelim.Size = new System.Drawing.Size(40, 19);
            this.TabDelim.TabIndex = 2;
            this.TabDelim.TabStop = false;
            // 
            // SemiDelim
            // 
            this.SemiDelim.Location = new System.Drawing.Point(10, 41);
            this.SemiDelim.Name = "SemiDelim";
            this.SemiDelim.Properties.AutoWidth = true;
            this.SemiDelim.Properties.Caption = "Semicolon";
            this.SemiDelim.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.SemiDelim.Properties.RadioGroupIndex = 1;
            this.SemiDelim.Size = new System.Drawing.Size(69, 19);
            this.SemiDelim.TabIndex = 1;
            this.SemiDelim.TabStop = false;
            // 
            // CommaDelim
            // 
            this.CommaDelim.EditValue = true;
            this.CommaDelim.Location = new System.Drawing.Point(10, 19);
            this.CommaDelim.Name = "CommaDelim";
            this.CommaDelim.Properties.AutoWidth = true;
            this.CommaDelim.Properties.Caption = "Comma";
            this.CommaDelim.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.CommaDelim.Properties.RadioGroupIndex = 1;
            this.CommaDelim.Size = new System.Drawing.Size(57, 19);
            this.CommaDelim.TabIndex = 0;
            // 
            // HeaderLine
            // 
            this.HeaderLine.EditValue = true;
            this.HeaderLine.Location = new System.Drawing.Point(17, 185);
            this.HeaderLine.Name = "HeaderLine";
            this.HeaderLine.Properties.AutoWidth = true;
            this.HeaderLine.Properties.Caption = "First line of file contains column header labels";
            this.HeaderLine.Size = new System.Drawing.Size(238, 19);
            this.HeaderLine.TabIndex = 35;
            // 
            // DeleteExistingData
            // 
            this.DeleteExistingData.Location = new System.Drawing.Point(17, 209);
            this.DeleteExistingData.Name = "DeleteExistingData";
            this.DeleteExistingData.Properties.AutoWidth = true;
            this.DeleteExistingData.Properties.Caption = "Delete existing data before importing this file";
            this.DeleteExistingData.Size = new System.Drawing.Size(237, 19);
            this.DeleteExistingData.TabIndex = 36;
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
            this.Cancel.Location = new System.Drawing.Point(341, 290);
            this.Cancel.Name = "Cancel";
            this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Cancel.Size = new System.Drawing.Size(68, 23);
            this.Cancel.TabIndex = 38;
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
            this.OK.Location = new System.Drawing.Point(265, 290);
            this.OK.Name = "OK";
            this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.OK.Size = new System.Drawing.Size(68, 23);
            this.OK.TabIndex = 37;
            this.OK.Tag = "OK";
            this.OK.Text = "OK";
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(193, 152);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 40;
            this.label2.Text = "Text qualifier:";
            // 
            // label3
            // 
            this.label3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.label3.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.label3.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.label3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.label3.Location = new System.Drawing.Point(6, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(404, 60);
            this.label3.TabIndex = 41;
            this.label3.Text = resources.GetString("label3.Text");
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // MultDelimsAsSingle
            // 
            this.MultDelimsAsSingle.Location = new System.Drawing.Point(193, 117);
            this.MultDelimsAsSingle.Name = "MultDelimsAsSingle";
            this.MultDelimsAsSingle.Properties.AutoWidth = true;
            this.MultDelimsAsSingle.Properties.Caption = "Treat consecutive delimiters as one";
            this.MultDelimsAsSingle.Size = new System.Drawing.Size(191, 19);
            this.MultDelimsAsSingle.TabIndex = 42;
            // 
            // ImportInBackground
            // 
            this.ImportInBackground.Location = new System.Drawing.Point(17, 233);
            this.ImportInBackground.Name = "ImportInBackground";
            this.ImportInBackground.Properties.AutoWidth = true;
            this.ImportInBackground.Properties.Caption = "Import in the background";
            this.ImportInBackground.Size = new System.Drawing.Size(143, 19);
            this.ImportInBackground.TabIndex = 44;
            // 
            // CheckForFileUpdates
            // 
            this.CheckForFileUpdates.Location = new System.Drawing.Point(17, 257);
            this.CheckForFileUpdates.Name = "CheckForFileUpdates";
            this.CheckForFileUpdates.Properties.AutoWidth = true;
            this.CheckForFileUpdates.Properties.Caption = "Re-import this file in the future whenever it changes";
            this.CheckForFileUpdates.Size = new System.Drawing.Size(272, 19);
            this.CheckForFileUpdates.TabIndex = 45;
            // 
            // Bitmaps16x16
            // 
            this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
            this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
            this.Bitmaps16x16.Images.SetKeyName(0, "SharePoint.bmp");
            // 
            // TextQualifier
            // 
            this.TextQualifier.Location = new System.Drawing.Point(266, 151);
            this.TextQualifier.Name = "TextQualifier";
            this.TextQualifier.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.TextQualifier.Properties.DropDownRows = 3;
            this.TextQualifier.Properties.Items.AddRange(new object[] {
            "\"",
            "\'",
            "None"});
            this.TextQualifier.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.TextQualifier.Size = new System.Drawing.Size(107, 20);
            this.TextQualifier.TabIndex = 48;
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl1.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl1.Cursor = System.Windows.Forms.Cursors.Default;
            this.labelControl1.LineVisible = true;
            this.labelControl1.Location = new System.Drawing.Point(0, 58);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(415, 13);
            this.labelControl1.TabIndex = 53;
            // 
            // labelControl2
            // 
            this.labelControl2.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.labelControl2.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl2.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl2.Cursor = System.Windows.Forms.Cursors.Default;
            this.labelControl2.LineVisible = true;
            this.labelControl2.Location = new System.Drawing.Point(0, 277);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(415, 10);
            this.labelControl2.TabIndex = 54;
            // 
            // UserDataImportTextFile
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(413, 317);
            this.Controls.Add(this.CheckForFileUpdates);
            this.Controls.Add(this.ImportInBackground);
            this.Controls.Add(this.MultDelimsAsSingle);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.DeleteExistingData);
            this.Controls.Add(this.HeaderLine);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Browse);
            this.Controls.Add(this.FileName);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.TextQualifier);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.labelControl2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserDataImportTextFile";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Import Text File";
            this.Activated += new System.EventHandler(this.AnnotationImport_Activated);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.AnnotationImport_Closing);
            this.VisibleChanged += new System.EventHandler(this.UserDataImport_VisibleChanged);
            ((System.ComponentModel.ISupportInitialize)(this.FileName.Properties)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SpaceDelim.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TabDelim.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SemiDelim.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CommaDelim.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.HeaderLine.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DeleteExistingData.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MultDelimsAsSingle.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImportInBackground.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CheckForFileUpdates.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TextQualifier.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void Browse_Click(object sender, System.EventArgs e)
		{
			string fileName = UIMisc.GetOpenFilename(Text, FileName.Text, FileFilter, DefaultExt);
			if (fileName != "") FileName.Text = fileName;

			if (fileName.EndsWith(".smi", StringComparison.OrdinalIgnoreCase) && FileName.Text == "")
				SpaceDelim.Checked = true; // if Smiles file then make space the default delimiter
		}

		private void OK_Click(object sender, System.EventArgs e)
		{
			string fullFileName = UIMisc.CheckFileName(1, FileName, InitialName, ClientDirs.DefaultMobiusUserDocumentsFolder, ".csv");
			if (fullFileName=="") return;
			FileName.Text = fullFileName;
			DialogResult = DialogResult.OK;
		}

		private void Cancel_Click(object sender, System.EventArgs e)
		{
		}

		private void AnnotationImport_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			return;
		}

		private void AnnotationImport_Activated(object sender, System.EventArgs e)
		{
			FileName.Focus();
		}

		private void SpaceDelim_CheckedChanged(object sender, System.EventArgs e)
		{
			if (SpaceDelim.Checked)	MultDelimsAsSingle.Checked = true;
			else MultDelimsAsSingle.Checked = false;
		}

		private void label3_Click(object sender, System.EventArgs e)
		{
		
		}

		private void UserDataImport_VisibleChanged(object sender, EventArgs e)
		{
			if (Visible) InitialName = FileName.Text; // save initial name for later compare
		}

	}
}
