using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Summary description for SetupText.
	/// </summary>
	public class SetupTextFile : XtraForm
	{

		static SetupTextFile Instance;

		string InitialName = ""; // initial name for file when dialog entered
		public string DefaultFolder = "";

		public System.Windows.Forms.GroupBox QualifiedNumberFormatting;
		public DevExpress.XtraEditors.LabelControl Label3;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.SimpleButton OK;
		public System.Windows.Forms.GroupBox Frame1;
		public DevExpress.XtraEditors.CheckEdit CommaSeparated;
		public DevExpress.XtraEditors.CheckEdit TabSeparated;
		public DevExpress.XtraEditors.CheckEdit OpenSpotfire;
		public DevExpress.XtraEditors.LabelControl Label2;
		public DevExpress.XtraEditors.SimpleButton Browse;
		public DevExpress.XtraEditors.LabelControl Label1;
		public DevExpress.XtraEditors.TextEdit FileName;
		public DevExpress.XtraEditors.CheckEdit Spotfire;
        public DevExpress.XtraEditors.CheckEdit ExportInBackground;
		private ImageList Bitmaps16x16;
		private QnfSplitControl QnfSplitOptions;
		private LabelControl labelControl1;
		public CheckEdit DuplicateKeyValues;
		public GroupBox groupBox1;
		public CheckEdit checkEdit1;
		public CheckEdit SmilesOrHelmMolFormat;
		public LabelControl labelControl2;
		public CheckEdit ChimeStringStructure;
		public CheckEdit SaveAsDefaultFolderOption;
		public GroupBox groupBox2;
		public CheckEdit NormalTableLabel;
		public CheckEdit NoneTableLabel;
		public CheckEdit OrdinalTableLabel;
		public CheckEdit InternalTableLabel;
		private IContainer components;
	    private bool _alertExport;

		public SetupTextFile()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

/// <summary>
/// Setup & show the dialog
/// </summary>
/// <param name="rf"></param>

		public static new DialogResult ShowDialog (
			ResultsFormat rf,
			bool useExistingOptionValues,
            bool alertExport = false)
		{
			if (Instance == null) Instance = new SetupTextFile();
			return Instance.ShowDialog2(rf, useExistingOptionValues, alertExport);
		}

		private DialogResult ShowDialog2(
			ResultsFormat rf,
			bool useExistingOptionValues,
            bool alertExport)
		{
		    _alertExport = alertExport;
            string fileName;

			rf.OutputDestination = OutputDest.TextFile;

			if (useExistingOptionValues) FileName.Text = rf.OutputFileName;
			else GetDefaultExportParameters(rf);

			if (rf.ExportFileFormat == ExportFileFormat.Csv)
				CommaSeparated.Checked = true;
			else TabSeparated.Checked = true;

			QnfSplitOptions.Setup(rf.QualifiedNumberSplit);

			if (rf.ExportStructureFormat == ExportStructureFormat.Smiles)
				SmilesOrHelmMolFormat.Checked = true;
			else ChimeStringStructure.Checked = true;

			if (rf.ColumnNameFormat == ColumnNameFormat.Normal) NormalTableLabel.Checked = true;
			else if (rf.ColumnNameFormat == ColumnNameFormat.Internal) InternalTableLabel.Checked = true;
			else if (rf.ColumnNameFormat == ColumnNameFormat.Ordinal) OrdinalTableLabel.Checked = true;
			else if (rf.ColumnNameFormat == ColumnNameFormat.None) NoneTableLabel.Checked = true;

			Spotfire.Checked = rf.IncludeDataTypes;
			DuplicateKeyValues.Checked = rf.DuplicateKeyTableValues;
			ExportInBackground.Checked = rf.RunInBackground;

			DialogResult dr = ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return dr;

			rf.OutputFileName = FileName.Text;

			if (CommaSeparated.Checked)
				rf.ExportFileFormat = ExportFileFormat.Csv;
			else if (TabSeparated.Checked)
				rf.ExportFileFormat = ExportFileFormat.Tsv;

			rf.QualifiedNumberSplit = QnfSplitOptions.Get();

			if (ChimeStringStructure.Checked)
				rf.ExportStructureFormat = ExportStructureFormat.Chime;
			else rf.ExportStructureFormat = ExportStructureFormat.Smiles;

			if (NormalTableLabel.Checked) rf.ColumnNameFormat = ColumnNameFormat.Normal;
			else if (InternalTableLabel.Checked) rf.ColumnNameFormat = ColumnNameFormat.Internal;
			else if (OrdinalTableLabel.Checked) rf.ColumnNameFormat = ColumnNameFormat.Ordinal;
			else if (NoneTableLabel.Checked) rf.ColumnNameFormat = ColumnNameFormat.None;

			rf.DuplicateKeyTableValues = DuplicateKeyValues.Checked;

			rf.IncludeDataTypes = Spotfire.Checked;
			rf.RunInBackground = ExportInBackground.Checked;

			SaveDefaultExportParameters(rf);

			return DialogResult.OK;
		}

		/// <summary>
		/// Get default export parameters for user
		/// </summary>
		/// <param name="rf"></param>

		void GetDefaultExportParameters(ResultsFormat rf)
		{
			string txt = Preferences.Get("TextFileExportDefaults");
			do
			{
				if (!Lex.IsNullOrEmpty(txt))
					try
					{
						string[] sa = txt.Split('\t');
						rf.ExportFileFormat = (ExportFileFormat)int.Parse(sa[0]);
						rf.QualifiedNumberSplit = QnfSplitControl.DeserializeQualifiedNumberSplit(sa[1]);
						rf.IncludeDataTypes = bool.Parse(sa[2]);
						rf.DuplicateKeyTableValues = bool.Parse(sa[3]);
						rf.RunInBackground = bool.Parse(sa[4]);
						if (sa.Length >= 6) // structure format
							rf.ExportStructureFormat = (ExportStructureFormat)Enum.Parse(typeof(ExportStructureFormat), sa[5], true);

						if (sa.Length >= 7) // column name format
							rf.ColumnNameFormat = (ColumnNameFormat)Enum.Parse(typeof(ColumnNameFormat), sa[6], true);

						break; // finish up
					}
					catch (Exception ex) { ex = ex; } // fall through to defaults on error

				rf.ExportFileFormat = ExportFileFormat.Csv;
				rf.QualifiedNumberSplit = // split by qualifier and numeric value
					QnfEnum.Split | QnfEnum.Qualifier | QnfEnum.NumericValue;
				rf.IncludeDataTypes = false;
				rf.DuplicateKeyTableValues = false;
				rf.RunInBackground = false;
				rf.ExportStructureFormat = ExportStructureFormat.Chime;
			} while (false);

			if (rf.QueryManager != null && rf.QueryManager.Query != null &&
			 rf.QueryManager.Query.DuplicateKeyValues)
				rf.DuplicateKeyTableValues = true;
			return;
		}

		/// <summary>
		/// Save default export parameters for user
		/// </summary>
		/// <param name="rf"></param>

		void SaveDefaultExportParameters(ResultsFormat rf)
		{
			string txt =
				((int)rf.ExportFileFormat).ToString() + '\t' +
				QnfSplitControl.SerializeQualifiedNumberSplit(rf.QualifiedNumberSplit) + '\t' +
				rf.IncludeDataTypes + '\t' +
				rf.DuplicateKeyTableValues + '\t' +
				rf.RunInBackground + '\t' +
				rf.ExportStructureFormat.ToString() + '\t' +
				rf.ColumnNameFormat.ToString();

			Preferences.Set("TextFileExportDefaults", txt);

			if (SaveAsDefaultFolderOption.Checked)
				SetupExcel.SaveDefaultFolder(rf.OutputFileName);
			return;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupTextFile));
			this.QualifiedNumberFormatting = new System.Windows.Forms.GroupBox();
			this.QnfSplitOptions = new Mobius.ClientComponents.QnfSplitControl();
			this.Label3 = new DevExpress.XtraEditors.LabelControl();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Frame1 = new System.Windows.Forms.GroupBox();
			this.CommaSeparated = new DevExpress.XtraEditors.CheckEdit();
			this.TabSeparated = new DevExpress.XtraEditors.CheckEdit();
			this.OpenSpotfire = new DevExpress.XtraEditors.CheckEdit();
			this.Label2 = new DevExpress.XtraEditors.LabelControl();
			this.Browse = new DevExpress.XtraEditors.SimpleButton();
			this.FileName = new DevExpress.XtraEditors.TextEdit();
			this.Label1 = new DevExpress.XtraEditors.LabelControl();
			this.Spotfire = new DevExpress.XtraEditors.CheckEdit();
			this.ExportInBackground = new DevExpress.XtraEditors.CheckEdit();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.DuplicateKeyValues = new DevExpress.XtraEditors.CheckEdit();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkEdit1 = new DevExpress.XtraEditors.CheckEdit();
			this.SmilesOrHelmMolFormat = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.ChimeStringStructure = new DevExpress.XtraEditors.CheckEdit();
			this.SaveAsDefaultFolderOption = new DevExpress.XtraEditors.CheckEdit();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.InternalTableLabel = new DevExpress.XtraEditors.CheckEdit();
			this.NoneTableLabel = new DevExpress.XtraEditors.CheckEdit();
			this.OrdinalTableLabel = new DevExpress.XtraEditors.CheckEdit();
			this.NormalTableLabel = new DevExpress.XtraEditors.CheckEdit();
			this.QualifiedNumberFormatting.SuspendLayout();
			this.Frame1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommaSeparated.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TabSeparated.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OpenSpotfire.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FileName.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Spotfire.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExportInBackground.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DuplicateKeyValues.Properties)).BeginInit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkEdit1.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SmilesOrHelmMolFormat.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChimeStringStructure.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SaveAsDefaultFolderOption.Properties)).BeginInit();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InternalTableLabel.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NoneTableLabel.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OrdinalTableLabel.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NormalTableLabel.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// QualifiedNumberFormatting
			// 
			this.QualifiedNumberFormatting.BackColor = System.Drawing.Color.Transparent;
			this.QualifiedNumberFormatting.Controls.Add(this.QnfSplitOptions);
			this.QualifiedNumberFormatting.Controls.Add(this.Label3);
			this.QualifiedNumberFormatting.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.QualifiedNumberFormatting.ForeColor = System.Drawing.SystemColors.ControlText;
			this.QualifiedNumberFormatting.Location = new System.Drawing.Point(8, 123);
			this.QualifiedNumberFormatting.Name = "QualifiedNumberFormatting";
			this.QualifiedNumberFormatting.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.QualifiedNumberFormatting.Size = new System.Drawing.Size(468, 73);
			this.QualifiedNumberFormatting.TabIndex = 18;
			this.QualifiedNumberFormatting.TabStop = false;
			this.QualifiedNumberFormatting.Text = "Formatting of \"Qualified\" numeric values (e.g. >50) ";
			// 
			// QnfSplitOptions
			// 
			this.QnfSplitOptions.Location = new System.Drawing.Point(12, 19);
			this.QnfSplitOptions.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.QnfSplitOptions.Name = "QnfSplitOptions";
			this.QnfSplitOptions.Size = new System.Drawing.Size(339, 49);
			this.QnfSplitOptions.TabIndex = 37;
			// 
			// Label3
			// 
			this.Label3.Appearance.BackColor = System.Drawing.SystemColors.Control;
			this.Label3.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label3.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label3.Appearance.Options.UseBackColor = true;
			this.Label3.Appearance.Options.UseFont = true;
			this.Label3.Appearance.Options.UseForeColor = true;
			this.Label3.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label3.Location = new System.Drawing.Point(58, 140);
			this.Label3.Name = "Label3";
			this.Label3.Size = new System.Drawing.Size(521, 13);
			this.Label3.TabIndex = 16;
			this.Label3.Text = "Note: You can open an existing Spotfire file from the main menu with the expert c" +
    "ommand: SPOTFIRE filename";
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
			this.Cancel.Location = new System.Drawing.Point(406, 469);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(69, 23);
			this.Cancel.TabIndex = 15;
			this.Cancel.Tag = "Cancel";
			this.Cancel.Text = "Cancel";
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(326, 469);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(69, 23);
			this.OK.TabIndex = 14;
			this.OK.Tag = "OK";
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// Frame1
			// 
			this.Frame1.BackColor = System.Drawing.Color.Transparent;
			this.Frame1.Controls.Add(this.CommaSeparated);
			this.Frame1.Controls.Add(this.TabSeparated);
			this.Frame1.Controls.Add(this.OpenSpotfire);
			this.Frame1.Controls.Add(this.Label2);
			this.Frame1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Frame1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Frame1.Location = new System.Drawing.Point(8, 37);
			this.Frame1.Name = "Frame1";
			this.Frame1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Frame1.Size = new System.Drawing.Size(468, 78);
			this.Frame1.TabIndex = 17;
			this.Frame1.TabStop = false;
			this.Frame1.Text = "Output File Format";
			// 
			// CommaSeparated
			// 
			this.CommaSeparated.Cursor = System.Windows.Forms.Cursors.Default;
			this.CommaSeparated.EditValue = true;
			this.CommaSeparated.Location = new System.Drawing.Point(16, 22);
			this.CommaSeparated.Name = "CommaSeparated";
			this.CommaSeparated.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.CommaSeparated.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CommaSeparated.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.CommaSeparated.Properties.Appearance.Options.UseBackColor = true;
			this.CommaSeparated.Properties.Appearance.Options.UseFont = true;
			this.CommaSeparated.Properties.Appearance.Options.UseForeColor = true;
			this.CommaSeparated.Properties.Caption = "Comma-separated values (.csv)";
			this.CommaSeparated.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.CommaSeparated.Properties.RadioGroupIndex = 1;
			this.CommaSeparated.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CommaSeparated.Size = new System.Drawing.Size(235, 19);
			this.CommaSeparated.TabIndex = 10;
			// 
			// TabSeparated
			// 
			this.TabSeparated.Cursor = System.Windows.Forms.Cursors.Default;
			this.TabSeparated.Location = new System.Drawing.Point(16, 47);
			this.TabSeparated.Name = "TabSeparated";
			this.TabSeparated.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.TabSeparated.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TabSeparated.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.TabSeparated.Properties.Appearance.Options.UseBackColor = true;
			this.TabSeparated.Properties.Appearance.Options.UseFont = true;
			this.TabSeparated.Properties.Appearance.Options.UseForeColor = true;
			this.TabSeparated.Properties.Caption = "Tab separated values (.txt)";
			this.TabSeparated.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.TabSeparated.Properties.RadioGroupIndex = 1;
			this.TabSeparated.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.TabSeparated.Size = new System.Drawing.Size(235, 19);
			this.TabSeparated.TabIndex = 8;
			this.TabSeparated.TabStop = false;
			// 
			// OpenSpotfire
			// 
			this.OpenSpotfire.Cursor = System.Windows.Forms.Cursors.Default;
			this.OpenSpotfire.Location = new System.Drawing.Point(33, 122);
			this.OpenSpotfire.Name = "OpenSpotfire";
			this.OpenSpotfire.Properties.Appearance.BackColor = System.Drawing.SystemColors.Control;
			this.OpenSpotfire.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OpenSpotfire.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OpenSpotfire.Properties.Appearance.Options.UseBackColor = true;
			this.OpenSpotfire.Properties.Appearance.Options.UseFont = true;
			this.OpenSpotfire.Properties.Appearance.Options.UseForeColor = true;
			this.OpenSpotfire.Properties.Caption = "Open output file with Spotfire (Spotfire must be installed)";
			this.OpenSpotfire.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OpenSpotfire.Size = new System.Drawing.Size(344, 19);
			this.OpenSpotfire.TabIndex = 2;
			// 
			// Label2
			// 
			this.Label2.Appearance.BackColor = System.Drawing.SystemColors.Control;
			this.Label2.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label2.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label2.Appearance.Options.UseBackColor = true;
			this.Label2.Appearance.Options.UseFont = true;
			this.Label2.Appearance.Options.UseForeColor = true;
			this.Label2.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label2.Location = new System.Drawing.Point(58, 140);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(521, 13);
			this.Label2.TabIndex = 7;
			this.Label2.Text = "Note: You can open an existing Spotfire file from the main menu with the expert c" +
    "ommand: SPOTFIRE filename";
			// 
			// Browse
			// 
			this.Browse.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Browse.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Browse.Appearance.Options.UseFont = true;
			this.Browse.Appearance.Options.UseForeColor = true;
			this.Browse.Cursor = System.Windows.Forms.Cursors.Default;
			this.Browse.Location = new System.Drawing.Point(384, 7);
			this.Browse.Name = "Browse";
			this.Browse.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Browse.Size = new System.Drawing.Size(91, 22);
			this.Browse.TabIndex = 13;
			this.Browse.Text = "&Browse...";
			this.Browse.Click += new System.EventHandler(this.Browse_Click);
			// 
			// FileName
			// 
			this.FileName.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.FileName.Location = new System.Drawing.Point(61, 9);
			this.FileName.Name = "FileName";
			this.FileName.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.FileName.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FileName.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.FileName.Properties.Appearance.Options.UseBackColor = true;
			this.FileName.Properties.Appearance.Options.UseFont = true;
			this.FileName.Properties.Appearance.Options.UseForeColor = true;
			this.FileName.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.FileName.Size = new System.Drawing.Size(317, 20);
			this.FileName.TabIndex = 12;
			this.FileName.Tag = "Title";
			// 
			// Label1
			// 
			this.Label1.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Label1.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label1.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label1.Appearance.Options.UseBackColor = true;
			this.Label1.Appearance.Options.UseFont = true;
			this.Label1.Appearance.Options.UseForeColor = true;
			this.Label1.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label1.Location = new System.Drawing.Point(6, 11);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(48, 13);
			this.Label1.TabIndex = 16;
			this.Label1.Text = "File name:";
			// 
			// Spotfire
			// 
			this.Spotfire.Location = new System.Drawing.Point(26, 379);
			this.Spotfire.Name = "Spotfire";
			this.Spotfire.Properties.Caption = "Include Spotfire data type information in second line of file";
			this.Spotfire.Size = new System.Drawing.Size(305, 19);
			this.Spotfire.TabIndex = 19;
			// 
			// ExportInBackground
			// 
			this.ExportInBackground.Cursor = System.Windows.Forms.Cursors.Default;
			this.ExportInBackground.Location = new System.Drawing.Point(26, 427);
			this.ExportInBackground.Name = "ExportInBackground";
			this.ExportInBackground.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ExportInBackground.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ExportInBackground.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ExportInBackground.Properties.Appearance.Options.UseBackColor = true;
			this.ExportInBackground.Properties.Appearance.Options.UseFont = true;
			this.ExportInBackground.Properties.Appearance.Options.UseForeColor = true;
			this.ExportInBackground.Properties.Caption = "Export in the background";
			this.ExportInBackground.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ExportInBackground.Size = new System.Drawing.Size(145, 19);
			this.ExportInBackground.TabIndex = 20;
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "SharePoint.bmp");
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(-3, 454);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(491, 10);
			this.labelControl1.TabIndex = 38;
			// 
			// DuplicateKeyValues
			// 
			this.DuplicateKeyValues.Cursor = System.Windows.Forms.Cursors.Default;
			this.DuplicateKeyValues.Location = new System.Drawing.Point(26, 355);
			this.DuplicateKeyValues.Name = "DuplicateKeyValues";
			this.DuplicateKeyValues.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.DuplicateKeyValues.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DuplicateKeyValues.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.DuplicateKeyValues.Properties.Appearance.Options.UseBackColor = true;
			this.DuplicateKeyValues.Properties.Appearance.Options.UseFont = true;
			this.DuplicateKeyValues.Properties.Appearance.Options.UseForeColor = true;
			this.DuplicateKeyValues.Properties.Caption = "Duplicate data that occurs only once per compound";
			this.DuplicateKeyValues.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.DuplicateKeyValues.Size = new System.Drawing.Size(318, 19);
			this.DuplicateKeyValues.TabIndex = 39;
			this.DuplicateKeyValues.Tag = "FixedHeightStructs";
			// 
			// groupBox1
			// 
			this.groupBox1.BackColor = System.Drawing.Color.Transparent;
			this.groupBox1.Controls.Add(this.checkEdit1);
			this.groupBox1.Controls.Add(this.SmilesOrHelmMolFormat);
			this.groupBox1.Controls.Add(this.labelControl2);
			this.groupBox1.Controls.Add(this.ChimeStringStructure);
			this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBox1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.groupBox1.Location = new System.Drawing.Point(8, 208);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.groupBox1.Size = new System.Drawing.Size(467, 49);
			this.groupBox1.TabIndex = 40;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Chemical Structure Format";
			// 
			// checkEdit1
			// 
			this.checkEdit1.Cursor = System.Windows.Forms.Cursors.Default;
			this.checkEdit1.Location = new System.Drawing.Point(33, 122);
			this.checkEdit1.Name = "checkEdit1";
			this.checkEdit1.Properties.Appearance.BackColor = System.Drawing.SystemColors.Control;
			this.checkEdit1.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkEdit1.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.checkEdit1.Properties.Appearance.Options.UseBackColor = true;
			this.checkEdit1.Properties.Appearance.Options.UseFont = true;
			this.checkEdit1.Properties.Appearance.Options.UseForeColor = true;
			this.checkEdit1.Properties.Caption = "Open output file with Spotfire (Spotfire must be installed)";
			this.checkEdit1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.checkEdit1.Size = new System.Drawing.Size(344, 19);
			this.checkEdit1.TabIndex = 15;
			// 
			// SmilesOrHelmMolFormat
			// 
			this.SmilesOrHelmMolFormat.Cursor = System.Windows.Forms.Cursors.Default;
			this.SmilesOrHelmMolFormat.EditValue = true;
			this.SmilesOrHelmMolFormat.Location = new System.Drawing.Point(16, 19);
			this.SmilesOrHelmMolFormat.Name = "SmilesOrHelmMolFormat";
			this.SmilesOrHelmMolFormat.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.SmilesOrHelmMolFormat.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SmilesOrHelmMolFormat.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SmilesOrHelmMolFormat.Properties.Appearance.Options.UseBackColor = true;
			this.SmilesOrHelmMolFormat.Properties.Appearance.Options.UseFont = true;
			this.SmilesOrHelmMolFormat.Properties.Appearance.Options.UseForeColor = true;
			this.SmilesOrHelmMolFormat.Properties.Caption = "Smiles / Helm";
			this.SmilesOrHelmMolFormat.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.SmilesOrHelmMolFormat.Properties.RadioGroupIndex = 1;
			this.SmilesOrHelmMolFormat.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.SmilesOrHelmMolFormat.Size = new System.Drawing.Size(99, 19);
			this.SmilesOrHelmMolFormat.TabIndex = 2;
			// 
			// labelControl2
			// 
			this.labelControl2.Appearance.BackColor = System.Drawing.SystemColors.Control;
			this.labelControl2.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl2.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.labelControl2.Appearance.Options.UseBackColor = true;
			this.labelControl2.Appearance.Options.UseFont = true;
			this.labelControl2.Appearance.Options.UseForeColor = true;
			this.labelControl2.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl2.Location = new System.Drawing.Point(58, 140);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(521, 13);
			this.labelControl2.TabIndex = 16;
			this.labelControl2.Text = "Note: You can open an existing Spotfire file from the main menu with the expert c" +
    "ommand: SPOTFIRE filename";
			// 
			// ChimeStringStructure
			// 
			this.ChimeStringStructure.Cursor = System.Windows.Forms.Cursors.Default;
			this.ChimeStringStructure.Location = new System.Drawing.Point(125, 19);
			this.ChimeStringStructure.Name = "ChimeStringStructure";
			this.ChimeStringStructure.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ChimeStringStructure.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ChimeStringStructure.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ChimeStringStructure.Properties.Appearance.Options.UseBackColor = true;
			this.ChimeStringStructure.Properties.Appearance.Options.UseFont = true;
			this.ChimeStringStructure.Properties.Appearance.Options.UseForeColor = true;
			this.ChimeStringStructure.Properties.Caption = "Chime string ";
			this.ChimeStringStructure.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ChimeStringStructure.Properties.RadioGroupIndex = 1;
			this.ChimeStringStructure.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ChimeStringStructure.Size = new System.Drawing.Size(98, 19);
			this.ChimeStringStructure.TabIndex = 3;
			this.ChimeStringStructure.TabStop = false;
			// 
			// SaveAsDefaultFolderOption
			// 
			this.SaveAsDefaultFolderOption.Cursor = System.Windows.Forms.Cursors.Default;
			this.SaveAsDefaultFolderOption.Location = new System.Drawing.Point(26, 403);
			this.SaveAsDefaultFolderOption.Name = "SaveAsDefaultFolderOption";
			this.SaveAsDefaultFolderOption.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.SaveAsDefaultFolderOption.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SaveAsDefaultFolderOption.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SaveAsDefaultFolderOption.Properties.Appearance.Options.UseBackColor = true;
			this.SaveAsDefaultFolderOption.Properties.Appearance.Options.UseFont = true;
			this.SaveAsDefaultFolderOption.Properties.Appearance.Options.UseForeColor = true;
			this.SaveAsDefaultFolderOption.Properties.Caption = "Use this as the default export folder in the future";
			this.SaveAsDefaultFolderOption.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.SaveAsDefaultFolderOption.Size = new System.Drawing.Size(296, 19);
			this.SaveAsDefaultFolderOption.TabIndex = 46;
			this.SaveAsDefaultFolderOption.Tag = "FixedHeightStructs";
			// 
			// groupBox2
			// 
			this.groupBox2.BackColor = System.Drawing.Color.Transparent;
			this.groupBox2.Controls.Add(this.InternalTableLabel);
			this.groupBox2.Controls.Add(this.NoneTableLabel);
			this.groupBox2.Controls.Add(this.OrdinalTableLabel);
			this.groupBox2.Controls.Add(this.NormalTableLabel);
			this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBox2.ForeColor = System.Drawing.SystemColors.ControlText;
			this.groupBox2.Location = new System.Drawing.Point(8, 269);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.groupBox2.Size = new System.Drawing.Size(469, 73);
			this.groupBox2.TabIndex = 41;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Format of Table/Assay Names Included in Column Header Labels";
			// 
			// InternalTableLabel
			// 
			this.InternalTableLabel.Cursor = System.Windows.Forms.Cursors.Default;
			this.InternalTableLabel.Location = new System.Drawing.Point(210, 19);
			this.InternalTableLabel.Name = "InternalTableLabel";
			this.InternalTableLabel.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.InternalTableLabel.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.InternalTableLabel.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.InternalTableLabel.Properties.Appearance.Options.UseBackColor = true;
			this.InternalTableLabel.Properties.Appearance.Options.UseFont = true;
			this.InternalTableLabel.Properties.Appearance.Options.UseForeColor = true;
			this.InternalTableLabel.Properties.Caption = "\"Internal\" Mobius table name";
			this.InternalTableLabel.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.InternalTableLabel.Properties.RadioGroupIndex = 1;
			this.InternalTableLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.InternalTableLabel.Size = new System.Drawing.Size(188, 19);
			this.InternalTableLabel.TabIndex = 52;
			this.InternalTableLabel.TabStop = false;
			// 
			// NoneTableLabel
			// 
			this.NoneTableLabel.Cursor = System.Windows.Forms.Cursors.Default;
			this.NoneTableLabel.Location = new System.Drawing.Point(210, 44);
			this.NoneTableLabel.Name = "NoneTableLabel";
			this.NoneTableLabel.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.NoneTableLabel.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NoneTableLabel.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.NoneTableLabel.Properties.Appearance.Options.UseBackColor = true;
			this.NoneTableLabel.Properties.Appearance.Options.UseFont = true;
			this.NoneTableLabel.Properties.Appearance.Options.UseForeColor = true;
			this.NoneTableLabel.Properties.Caption = "None (duplicate column labels may occur)";
			this.NoneTableLabel.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.NoneTableLabel.Properties.RadioGroupIndex = 1;
			this.NoneTableLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.NoneTableLabel.Size = new System.Drawing.Size(253, 19);
			this.NoneTableLabel.TabIndex = 51;
			this.NoneTableLabel.TabStop = false;
			// 
			// OrdinalTableLabel
			// 
			this.OrdinalTableLabel.Cursor = System.Windows.Forms.Cursors.Default;
			this.OrdinalTableLabel.Location = new System.Drawing.Point(18, 44);
			this.OrdinalTableLabel.Name = "OrdinalTableLabel";
			this.OrdinalTableLabel.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.OrdinalTableLabel.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OrdinalTableLabel.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OrdinalTableLabel.Properties.Appearance.Options.UseBackColor = true;
			this.OrdinalTableLabel.Properties.Appearance.Options.UseFont = true;
			this.OrdinalTableLabel.Properties.Appearance.Options.UseForeColor = true;
			this.OrdinalTableLabel.Properties.Caption = "T1, T2, T3...";
			this.OrdinalTableLabel.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.OrdinalTableLabel.Properties.RadioGroupIndex = 1;
			this.OrdinalTableLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OrdinalTableLabel.Size = new System.Drawing.Size(87, 19);
			this.OrdinalTableLabel.TabIndex = 49;
			this.OrdinalTableLabel.TabStop = false;
			// 
			// NormalTableLabel
			// 
			this.NormalTableLabel.Cursor = System.Windows.Forms.Cursors.Default;
			this.NormalTableLabel.EditValue = true;
			this.NormalTableLabel.Location = new System.Drawing.Point(18, 19);
			this.NormalTableLabel.Name = "NormalTableLabel";
			this.NormalTableLabel.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.NormalTableLabel.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NormalTableLabel.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.NormalTableLabel.Properties.Appearance.Options.UseBackColor = true;
			this.NormalTableLabel.Properties.Appearance.Options.UseFont = true;
			this.NormalTableLabel.Properties.Appearance.Options.UseForeColor = true;
			this.NormalTableLabel.Properties.Caption = "Normal (beyond the first table)";
			this.NormalTableLabel.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.NormalTableLabel.Properties.RadioGroupIndex = 1;
			this.NormalTableLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.NormalTableLabel.Size = new System.Drawing.Size(205, 19);
			this.NormalTableLabel.TabIndex = 3;
			// 
			// SetupTextFile
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(484, 499);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.SaveAsDefaultFolderOption);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.DuplicateKeyValues);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.FileName);
			this.Controls.Add(this.ExportInBackground);
			this.Controls.Add(this.Spotfire);
			this.Controls.Add(this.QualifiedNumberFormatting);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Frame1);
			this.Controls.Add(this.Browse);
			this.Controls.Add(this.Label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SetupTextFile";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Export to Text File";
			this.Activated += new System.EventHandler(this.SetupTextFile_Activated);
			this.VisibleChanged += new System.EventHandler(this.SetupTextFile_VisibleChanged);
			this.QualifiedNumberFormatting.ResumeLayout(false);
			this.QualifiedNumberFormatting.PerformLayout();
			this.Frame1.ResumeLayout(false);
			this.Frame1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommaSeparated.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TabSeparated.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OpenSpotfire.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FileName.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Spotfire.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ExportInBackground.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DuplicateKeyValues.Properties)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkEdit1.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SmilesOrHelmMolFormat.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChimeStringStructure.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SaveAsDefaultFolderOption.Properties)).EndInit();
			this.groupBox2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.InternalTableLabel.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NoneTableLabel.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OrdinalTableLabel.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NormalTableLabel.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private void OK_Click(object sender, System.EventArgs e)
		{
			bool fileCheckEnabled = true;

			string fullFileName = UIMisc.CheckFileName(2, FileName, InitialName, ClientDirs.DefaultMobiusUserDocumentsFolder, ".csv");
			if (fullFileName=="") return;
			if (fileCheckEnabled && !UIMisc.CanWriteFile(fullFileName, true)) return;
			FileName.Text = fullFileName;

			if (ExportInBackground.Checked && fileCheckEnabled)
			{ // if background export of unc file tell user if we can't write directly to the file
				if (UIMisc.CanWriteFileFromServiceAccount(fullFileName) == DialogResult.Cancel) return;
                if (_alertExport && UIMisc.PathContainsDrive(fullFileName) == DialogResult.Cancel) return;
            }

			DialogResult = DialogResult.OK;
		}

		/// <summary>
		/// Browse regular file system name
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Browse_Click(object sender, System.EventArgs e)
		{
			string fileName;
			string initialFile = FileName.Text;

			if (Lex.IsNullOrEmpty(initialFile))
				initialFile = ClientDirs.DefaultMobiusUserDocumentsFolder;

			string filter, defaultExt;
			GetFileFilter(out filter, out defaultExt);

			fileName = UIMisc.GetSaveAsFilename(filter, initialFile, filter, defaultExt);
			if (fileName != "") FileName.Text = fileName;
			return;
		}

		/// <summary>
		/// Get file filter
		/// </summary>
		/// <returns></returns>

		void GetFileFilter(out string filter, out string defaultExt)
		{
			string csvFilter = "CSV (Comma delimited)(*.csv)|*.csv";
			string txtFilter = "Text (Tab delimited)(*.txt)|*.txt";

			if (CommaSeparated.Checked)
			{
				filter = csvFilter + "|" + txtFilter;
				defaultExt = ".csv";
			}
			else
			{
				filter = txtFilter + "|" + csvFilter;
				defaultExt = ".txt";
			}

			filter += "|All files (*.*)|*.*";

			return;
		}

		private void SetupTextFile_VisibleChanged(object sender, EventArgs e)
		{
			if (Visible) InitialName = FileName.Text; // save initial name for later compare
		}

		private void SetupTextFile_Activated(object sender, EventArgs e)
		{
			FileName.Focus();
		}

	}
}
