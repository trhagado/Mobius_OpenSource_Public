using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Summary description for SetupExcel.
	/// </summary>
	public class SetupExcel : XtraForm
	{
		static SetupExcel Instance;

		string InitialName = ""; // initial name for file when dialog entered
		public string DefaultFolder = "";

		public DevExpress.XtraEditors.SimpleButton Browse;
		public DevExpress.XtraEditors.CheckEdit DuplicateKeyValues;
		public DevExpress.XtraEditors.CheckEdit FixedHeightStructs;
		public GroupBox QualifiedNumberFormatting;
		public DevExpress.XtraEditors.CheckEdit Check1;
		public DevExpress.XtraEditors.LabelControl Label3;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.CheckEdit InsightStructure;
		public DevExpress.XtraEditors.LabelControl Label1;
		public DevExpress.XtraEditors.TextEdit FileName;
		public DevExpress.XtraEditors.LabelControl label4;
		public DevExpress.XtraEditors.CheckEdit ExportInBackground;
		private ImageList Bitmaps16x16;
		public DevExpress.XtraEditors.ComboBoxEdit HeaderLines;
		private QnfSplitControl QnfSplitOptions;
		private LabelControl labelControl1;
		public GroupBox groupBox1;
		public LabelControl labelControl2;
		public CheckEdit MetafileStructure;
		public CheckEdit SaveAsDefaultFolderOption;
		private IContainer components;
		private bool _alertExport;

		public SetupExcel()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			HeaderLines.SelectedItem = "2"; // default selection
		}

		/// <summary>
		/// Setup & show the dialog
		/// </summary>
		/// <param name="rf"></param>

		public new static DialogResult ShowDialog(
				ResultsFormat rf,
				bool useExistingOptionValues,
				bool alertExport = false)
		{

			if (Instance == null) Instance = new SetupExcel();
			return Instance.ShowDialog2(rf, useExistingOptionValues, alertExport);
		}

		private DialogResult ShowDialog2(
				ResultsFormat rf,
				bool useExistingOptionValues,
				bool alertExport)
		{
			_alertExport = alertExport;
			rf.OutputDestination = OutputDest.Excel;
			rf.ExportFileFormat = ExportFileFormat.Csv;

			if (useExistingOptionValues) FileName.Text = rf.OutputFileName;
			else GetDefaultExportParameters(rf);

			HeaderLines.SelectedItem = rf.HeaderLines.ToString();

			if (rf.ExportStructureFormat == ExportStructureFormat.Insight)
				InsightStructure.Checked = true;
			else MetafileStructure.Checked = true;

			FixedHeightStructs.Checked = rf.FixedHeightStructures;
			QnfSplitOptions.Setup(rf.QualifiedNumberSplit);
			ExportInBackground.Checked = rf.RunInBackground;
			DuplicateKeyValues.Checked = rf.DuplicateKeyTableValues;

			DialogResult dr = ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return dr;

			rf.OutputFileName = FileName.Text;

			string txt = HeaderLines.SelectedItem.ToString();
			int.TryParse(txt, out rf.HeaderLines);

			if (InsightStructure.Checked)
				rf.ExportStructureFormat = ExportStructureFormat.Insight;

			else if (MetafileStructure.Checked)
				rf.ExportStructureFormat = ExportStructureFormat.Metafiles;

			rf.FixedHeightStructures = FixedHeightStructs.Checked;

			rf.DuplicateKeyTableValues = DuplicateKeyValues.Checked;

			rf.QualifiedNumberSplit = QnfSplitOptions.Get();

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
			string txt = Preferences.Get("ExcelExportDefaults");
			do
			{
				if (!Lex.IsNullOrEmpty(txt))
					try
					{
						string[] sa = txt.Split('\t');
						rf.HeaderLines = int.Parse(sa[0]);
						rf.FixedHeightStructures = bool.Parse(sa[1]);
						rf.QualifiedNumberSplit = QnfSplitControl.DeserializeQualifiedNumberSplit(sa[2]);
						rf.DuplicateKeyTableValues = bool.Parse(sa[3]);
						rf.RunInBackground = bool.Parse(sa[4]);
						rf.ExportStructureFormat = (ExportStructureFormat)Enum.Parse(typeof(ExportStructureFormat), sa[5], true);

						break; // finish up
					}
					catch (Exception ex) { ex = ex; } // fall through to defaults on error

				rf.HeaderLines = 2;
				rf.FixedHeightStructures = false;
				rf.QualifiedNumberSplit = // split by qualifier and numeric value
					QnfEnum.Split | QnfEnum.Qualifier | QnfEnum.NumericValue;
				rf.DuplicateKeyTableValues = false;
				rf.RunInBackground = false;
				rf.ExportStructureFormat = ExportStructureFormat.Insight;
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
				rf.HeaderLines.ToString() + '\t' +
				rf.FixedHeightStructures + '\t' +
				QnfSplitControl.SerializeQualifiedNumberSplit(rf.QualifiedNumberSplit) + '\t' +
				rf.DuplicateKeyTableValues + '\t' +
				rf.RunInBackground + '\t' +
				rf.ExportStructureFormat.ToString();

			Preferences.Set("ExcelExportDefaults", txt);

			if (SaveAsDefaultFolderOption.Checked)
				SaveDefaultFolder(rf.OutputFileName);
			return;
		}

		/// <summary>
		/// Save the default folder
		/// </summary>
		/// <param name="fileName"></param>

		public static void SaveDefaultFolder(string fileName)
		{
			if (Lex.IsNullOrEmpty(fileName) || Lex.StartsWith(fileName, "http"))
				return; // ignore if undefined or SharePoint URL

			string dir = Path.GetDirectoryName(fileName);
			if (!Lex.IsNullOrEmpty(dir))
			{
				ClientDirs.DefaultMobiusUserDocumentsFolder = dir; // update current session value
				Preferences.Set("DefaultExportFolder", dir); // also persist
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupExcel));
			this.Browse = new DevExpress.XtraEditors.SimpleButton();
			this.FileName = new DevExpress.XtraEditors.TextEdit();
			this.DuplicateKeyValues = new DevExpress.XtraEditors.CheckEdit();
			this.FixedHeightStructs = new DevExpress.XtraEditors.CheckEdit();
			this.QualifiedNumberFormatting = new System.Windows.Forms.GroupBox();
			this.QnfSplitOptions = new Mobius.ClientComponents.QnfSplitControl();
			this.Check1 = new DevExpress.XtraEditors.CheckEdit();
			this.Label3 = new DevExpress.XtraEditors.LabelControl();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.InsightStructure = new DevExpress.XtraEditors.CheckEdit();
			this.Label1 = new DevExpress.XtraEditors.LabelControl();
			this.label4 = new DevExpress.XtraEditors.LabelControl();
			this.ExportInBackground = new DevExpress.XtraEditors.CheckEdit();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.HeaderLines = new DevExpress.XtraEditors.ComboBoxEdit();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.MetafileStructure = new DevExpress.XtraEditors.CheckEdit();
			this.SaveAsDefaultFolderOption = new DevExpress.XtraEditors.CheckEdit();
			((System.ComponentModel.ISupportInitialize)(this.FileName.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DuplicateKeyValues.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FixedHeightStructs.Properties)).BeginInit();
			this.QualifiedNumberFormatting.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Check1.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.InsightStructure.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExportInBackground.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HeaderLines.Properties)).BeginInit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MetafileStructure.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SaveAsDefaultFolderOption.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// Browse
			// 
			this.Browse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.Browse.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Browse.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Browse.Appearance.Options.UseFont = true;
			this.Browse.Appearance.Options.UseForeColor = true;
			this.Browse.Cursor = System.Windows.Forms.Cursors.Default;
			this.Browse.Location = new System.Drawing.Point(297, 5);
			this.Browse.Name = "Browse";
			this.Browse.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Browse.Size = new System.Drawing.Size(69, 22);
			this.Browse.TabIndex = 29;
			this.Browse.Text = "&Browse...";
			this.Browse.Click += new System.EventHandler(this.Browse_Click);
			// 
			// FileName
			// 
			this.FileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FileName.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.FileName.Location = new System.Drawing.Point(67, 6);
			this.FileName.Name = "FileName";
			this.FileName.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.FileName.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FileName.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.FileName.Properties.Appearance.Options.UseBackColor = true;
			this.FileName.Properties.Appearance.Options.UseFont = true;
			this.FileName.Properties.Appearance.Options.UseForeColor = true;
			this.FileName.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.FileName.Size = new System.Drawing.Size(223, 20);
			this.FileName.TabIndex = 28;
			this.FileName.Tag = "Title";
			// 
			// DuplicateKeyValues
			// 
			this.DuplicateKeyValues.Cursor = System.Windows.Forms.Cursors.Default;
			this.DuplicateKeyValues.Location = new System.Drawing.Point(24, 157);
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
			this.DuplicateKeyValues.TabIndex = 22;
			this.DuplicateKeyValues.Tag = "FixedHeightStructs";
			// 
			// FixedHeightStructs
			// 
			this.FixedHeightStructs.Cursor = System.Windows.Forms.Cursors.Default;
			this.FixedHeightStructs.Location = new System.Drawing.Point(351, 22);
			this.FixedHeightStructs.Name = "FixedHeightStructs";
			this.FixedHeightStructs.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.FixedHeightStructs.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FixedHeightStructs.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FixedHeightStructs.Properties.Appearance.Options.UseBackColor = true;
			this.FixedHeightStructs.Properties.Appearance.Options.UseFont = true;
			this.FixedHeightStructs.Properties.Appearance.Options.UseForeColor = true;
			this.FixedHeightStructs.Properties.Caption = "Format structures into fixed size boxes";
			this.FixedHeightStructs.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.FixedHeightStructs.Size = new System.Drawing.Size(217, 19);
			this.FixedHeightStructs.TabIndex = 21;
			this.FixedHeightStructs.Tag = "FixedHeightStructs";
			this.FixedHeightStructs.Visible = false;
			// 
			// QualifiedNumberFormatting
			// 
			this.QualifiedNumberFormatting.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.QualifiedNumberFormatting.BackColor = System.Drawing.Color.Transparent;
			this.QualifiedNumberFormatting.Controls.Add(this.QnfSplitOptions);
			this.QualifiedNumberFormatting.Controls.Add(this.Check1);
			this.QualifiedNumberFormatting.Controls.Add(this.Label3);
			this.QualifiedNumberFormatting.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.QualifiedNumberFormatting.ForeColor = System.Drawing.SystemColors.ControlText;
			this.QualifiedNumberFormatting.Location = new System.Drawing.Point(9, 67);
			this.QualifiedNumberFormatting.Name = "QualifiedNumberFormatting";
			this.QualifiedNumberFormatting.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.QualifiedNumberFormatting.Size = new System.Drawing.Size(359, 79);
			this.QualifiedNumberFormatting.TabIndex = 26;
			this.QualifiedNumberFormatting.TabStop = false;
			this.QualifiedNumberFormatting.Text = "Formatting of \"Qualified\" Numeric Values (e.g. >50) ";
			// 
			// QnfSplitOptions
			// 
			this.QnfSplitOptions.Location = new System.Drawing.Point(11, 21);
			this.QnfSplitOptions.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.QnfSplitOptions.Name = "QnfSplitOptions";
			this.QnfSplitOptions.Size = new System.Drawing.Size(359, 49);
			this.QnfSplitOptions.TabIndex = 17;
			// 
			// Check1
			// 
			this.Check1.Cursor = System.Windows.Forms.Cursors.Default;
			this.Check1.Location = new System.Drawing.Point(33, 122);
			this.Check1.Name = "Check1";
			this.Check1.Properties.Appearance.BackColor = System.Drawing.SystemColors.Control;
			this.Check1.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Check1.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Check1.Properties.Appearance.Options.UseBackColor = true;
			this.Check1.Properties.Appearance.Options.UseFont = true;
			this.Check1.Properties.Appearance.Options.UseForeColor = true;
			this.Check1.Properties.Caption = "Open output file with Spotfire (Spotfire must be installed)";
			this.Check1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Check1.Size = new System.Drawing.Size(344, 19);
			this.Check1.TabIndex = 15;
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
			this.Cancel.Location = new System.Drawing.Point(297, 245);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(68, 23);
			this.Cancel.TabIndex = 24;
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
			this.OK.Location = new System.Drawing.Point(221, 245);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(69, 23);
			this.OK.TabIndex = 23;
			this.OK.Tag = "OK";
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// InsightStructure
			// 
			this.InsightStructure.Cursor = System.Windows.Forms.Cursors.Default;
			this.InsightStructure.EditValue = true;
			this.InsightStructure.Location = new System.Drawing.Point(11, 22);
			this.InsightStructure.Name = "InsightStructure";
			this.InsightStructure.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.InsightStructure.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.InsightStructure.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.InsightStructure.Properties.Appearance.Options.UseBackColor = true;
			this.InsightStructure.Properties.Appearance.Options.UseFont = true;
			this.InsightStructure.Properties.Appearance.Options.UseForeColor = true;
			this.InsightStructure.Properties.Caption = "Editable structures (Insight for Excel)";
			this.InsightStructure.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.InsightStructure.Properties.RadioGroupIndex = 1;
			this.InsightStructure.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.InsightStructure.Size = new System.Drawing.Size(304, 19);
			this.InsightStructure.TabIndex = 2;
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
			this.Label1.Location = new System.Drawing.Point(8, 9);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(48, 13);
			this.Label1.TabIndex = 30;
			this.Label1.Text = "File name:";
			// 
			// label4
			// 
			this.label4.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.label4.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.label4.Appearance.Options.UseBackColor = true;
			this.label4.Appearance.Options.UseFont = true;
			this.label4.Appearance.Options.UseForeColor = true;
			this.label4.Cursor = System.Windows.Forms.Cursors.Default;
			this.label4.Location = new System.Drawing.Point(20, 36);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(62, 13);
			this.label4.TabIndex = 32;
			this.label4.Text = "Header lines:";
			// 
			// ExportInBackground
			// 
			this.ExportInBackground.Cursor = System.Windows.Forms.Cursors.Default;
			this.ExportInBackground.Location = new System.Drawing.Point(24, 205);
			this.ExportInBackground.Name = "ExportInBackground";
			this.ExportInBackground.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ExportInBackground.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ExportInBackground.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ExportInBackground.Properties.Appearance.Options.UseBackColor = true;
			this.ExportInBackground.Properties.Appearance.Options.UseFont = true;
			this.ExportInBackground.Properties.Appearance.Options.UseForeColor = true;
			this.ExportInBackground.Properties.Caption = "Export in the background";
			this.ExportInBackground.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ExportInBackground.Size = new System.Drawing.Size(206, 19);
			this.ExportInBackground.TabIndex = 33;
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "SharePoint.bmp");
			// 
			// HeaderLines
			// 
			this.HeaderLines.Location = new System.Drawing.Point(94, 33);
			this.HeaderLines.Name = "HeaderLines";
			this.HeaderLines.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.HeaderLines.Properties.DropDownRows = 3;
			this.HeaderLines.Properties.Items.AddRange(new object[] {
            "0",
            "1",
            "2"});
			this.HeaderLines.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
			this.HeaderLines.Size = new System.Drawing.Size(52, 20);
			this.HeaderLines.TabIndex = 31;
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(-2, 230);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(398, 10);
			this.labelControl1.TabIndex = 38;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.BackColor = System.Drawing.Color.Transparent;
			this.groupBox1.Controls.Add(this.InsightStructure);
			this.groupBox1.Controls.Add(this.FixedHeightStructs);
			this.groupBox1.Controls.Add(this.labelControl2);
			this.groupBox1.Controls.Add(this.MetafileStructure);
			this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBox1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.groupBox1.Location = new System.Drawing.Point(353, 152);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.groupBox1.Size = new System.Drawing.Size(359, 79);
			this.groupBox1.TabIndex = 27;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Chemical Structure Format";
			this.groupBox1.Visible = false;
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
			// MetafileStructure
			// 
			this.MetafileStructure.Cursor = System.Windows.Forms.Cursors.Default;
			this.MetafileStructure.Location = new System.Drawing.Point(11, 47);
			this.MetafileStructure.Name = "MetafileStructure";
			this.MetafileStructure.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.MetafileStructure.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MetafileStructure.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.MetafileStructure.Properties.Appearance.Options.UseBackColor = true;
			this.MetafileStructure.Properties.Appearance.Options.UseFont = true;
			this.MetafileStructure.Properties.Appearance.Options.UseForeColor = true;
			this.MetafileStructure.Properties.Caption = "Uneditable structures (Windows metafile)";
			this.MetafileStructure.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.MetafileStructure.Properties.RadioGroupIndex = 1;
			this.MetafileStructure.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.MetafileStructure.Size = new System.Drawing.Size(344, 19);
			this.MetafileStructure.TabIndex = 3;
			this.MetafileStructure.TabStop = false;
			// 
			// SaveAsDefaultFolderOption
			// 
			this.SaveAsDefaultFolderOption.Cursor = System.Windows.Forms.Cursors.Default;
			this.SaveAsDefaultFolderOption.Location = new System.Drawing.Point(24, 181);
			this.SaveAsDefaultFolderOption.Name = "SaveAsDefaultFolderOption";
			this.SaveAsDefaultFolderOption.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.SaveAsDefaultFolderOption.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SaveAsDefaultFolderOption.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SaveAsDefaultFolderOption.Properties.Appearance.Options.UseBackColor = true;
			this.SaveAsDefaultFolderOption.Properties.Appearance.Options.UseFont = true;
			this.SaveAsDefaultFolderOption.Properties.Appearance.Options.UseForeColor = true;
			this.SaveAsDefaultFolderOption.Properties.Caption = "Use this as the default export folder in the future";
			this.SaveAsDefaultFolderOption.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.SaveAsDefaultFolderOption.Size = new System.Drawing.Size(288, 19);
			this.SaveAsDefaultFolderOption.TabIndex = 47;
			this.SaveAsDefaultFolderOption.Tag = "FixedHeightStructs";
			// 
			// SetupExcel
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(375, 275);
			this.Controls.Add(this.SaveAsDefaultFolderOption);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.FileName);
			this.Controls.Add(this.DuplicateKeyValues);
			this.Controls.Add(this.ExportInBackground);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.HeaderLines);
			this.Controls.Add(this.Browse);
			this.Controls.Add(this.QualifiedNumberFormatting);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.labelControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SetupExcel";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Export to Excel";
			this.Activated += new System.EventHandler(this.SetupExcel_Activated);
			this.VisibleChanged += new System.EventHandler(this.SetupExcel_VisibleChanged);
			((System.ComponentModel.ISupportInitialize)(this.FileName.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DuplicateKeyValues.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FixedHeightStructs.Properties)).EndInit();
			this.QualifiedNumberFormatting.ResumeLayout(false);
			this.QualifiedNumberFormatting.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Check1.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.InsightStructure.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ExportInBackground.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HeaderLines.Properties)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MetafileStructure.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SaveAsDefaultFolderOption.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private void SetupExcel_Activated(object sender, System.EventArgs e)
		{
			FileName.Focus();
		}

		private void OK_Click(object sender, System.EventArgs e)
		{
			string fullFileName = UIMisc.CheckFileName(2, FileName, InitialName, ClientDirs.DefaultMobiusUserDocumentsFolder, ".xlsx");
			if (fullFileName == "") return;
			if (!UIMisc.CanWriteFile(fullFileName, true)) return;
			FileName.Text = fullFileName;

			if (ExportInBackground.Checked)
			{ // if background export of unc file tell user if we can't write directly to the file
				if (UIMisc.CanWriteFileFromServiceAccount(fullFileName) == DialogResult.Cancel) return;
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

			string filter = GetFileFilter();
			fileName = UIMisc.GetSaveAsFilename("Excel File Name", initialFile, filter, ".xlsx");
			if (fileName != "") FileName.Text = fileName;
			return;
		}

		/// <summary>
		/// Get Excel file filter
		/// </summary>
		/// <returns></returns>

		string GetFileFilter()
		{
			return
				"Excel Workbook (*.xlsx)|*.xlsx|" +
				"Excel Macro-Enabled Workbook (*.xlsm)|*.xlsm|" +
				"Excel Binary Workbook (*.xlsb)|*.xlsb|" +
				"Excel 97-2003 Workbook (*.xls)|*.xls|" +
				"All files (*.*)|*.*";
		}

		private void SetupExcel_VisibleChanged(object sender, EventArgs e)
		{
			if (Visible) InitialName = FileName.Text; // save initial name for later compare
		}

	}
}
