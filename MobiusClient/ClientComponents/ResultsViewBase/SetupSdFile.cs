using Mobius.ComOps;
using Mobius.Data;
using Mobius.CdkMx;

using DevExpress.XtraEditors;

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Summary description for SetupSdf.
	/// </summary>
	/// 
	public class SetupSdFile : XtraForm
	{
		static SetupSdFile Instance;

		string InitialName = ""; // initial name for file when dialog entered

		public System.Windows.Forms.GroupBox Frame2;
		public DevExpress.XtraEditors.CheckEdit NoSuperAtom;
		public DevExpress.XtraEditors.CheckEdit LargeFragOnly;
		public DevExpress.XtraEditors.CheckEdit RemoveStereochemistry;
		public DevExpress.XtraEditors.LabelControl Label3;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.SimpleButton OK;
		public System.Windows.Forms.GroupBox Frame1;
		public DevExpress.XtraEditors.CheckEdit StructureOnly;
		public DevExpress.XtraEditors.CheckEdit AllData;
		public DevExpress.XtraEditors.CheckEdit OpenSpotfire;
		public DevExpress.XtraEditors.LabelControl Label2;
		public DevExpress.XtraEditors.SimpleButton Browse;
		public DevExpress.XtraEditors.LabelControl Label1;
		public DevExpress.XtraEditors.TextEdit FileName;
		public DevExpress.XtraEditors.CheckEdit RemoveHydrogens;
		public DevExpress.XtraEditors.CheckEdit ExportInBackground;
		public System.Windows.Forms.GroupBox QualifiedNumberFormatting;
		public DevExpress.XtraEditors.CheckEdit checkBox2;
		public DevExpress.XtraEditors.LabelControl label5;
		private QnfSplitControl QnfSplitOptions;
		private LabelControl labelControl1;
		public CheckEdit AllowExtraLengthFieldNames;
		public CheckEdit DuplicateKeyValues;
		public CheckEdit SaveAsDefaultFolderOption;
	    private bool _alertExport;

		/// <summary>
		/// Required designer variable.
		/// </summary>

		private System.ComponentModel.Container components = null;

		public SetupSdFile()
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

		public static new DialogResult ShowDialog(
			ResultsFormat rf,
			bool useExistingOptionValues,
            bool alertExport = false)
		{
			if (Instance == null) Instance = new SetupSdFile();
			return Instance.ShowDialog2(rf, useExistingOptionValues, alertExport);
		}

		public DialogResult ShowDialog2(
			ResultsFormat rf,
			bool useExistingOptionValues,
            bool alertExport)
		{
		    _alertExport = alertExport;
            rf.OutputDestination = OutputDest.SdFile;

			if (useExistingOptionValues) FileName.Text = rf.OutputFileName;
			else GetDefaultExportParameters(rf);

			bool structuresOnly = (rf.StructureFlags & MoleculeTransformationFlags.StructuresOnly) != 0;
			AllData.Checked = !structuresOnly;
			StructureOnly.Checked = structuresOnly;

			LargeFragOnly.Checked = (rf.StructureFlags & MoleculeTransformationFlags.LargestFragmentOnly) != 0;
			NoSuperAtom.Checked = (rf.StructureFlags & MoleculeTransformationFlags.NoSuperAtomInfo) != 0;
			RemoveHydrogens.Checked = (rf.StructureFlags & MoleculeTransformationFlags.RemoveHydrogens) != 0;
			RemoveStereochemistry.Checked = (rf.StructureFlags & MoleculeTransformationFlags.RemoveStereochemistry) != 0;

			QnfSplitOptions.Setup(rf.QualifiedNumberSplit);

			DuplicateKeyValues.Checked = rf.DuplicateKeyTableValues;
			AllowExtraLengthFieldNames.Checked = rf.AllowExtraLengthFieldNames;
			ExportInBackground.Checked = rf.RunInBackground;

			DialogResult dr = ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return dr;

			rf.OutputFileName = FileName.Text;

			rf.StructureFlags = 0;

			if (StructureOnly.Checked)
				rf.StructureFlags |= MoleculeTransformationFlags.StructuresOnly;

			if (LargeFragOnly.Checked)
				rf.StructureFlags |= MoleculeTransformationFlags.LargestFragmentOnly;

			if (NoSuperAtom.Checked)
				rf.StructureFlags |= MoleculeTransformationFlags.NoSuperAtomInfo;

			if (RemoveHydrogens.Checked)
				rf.StructureFlags |= MoleculeTransformationFlags.RemoveHydrogens;

			if (RemoveStereochemistry.Checked)
				rf.StructureFlags |= MoleculeTransformationFlags.RemoveStereochemistry;

			rf.QualifiedNumberSplit = QnfSplitOptions.Get();

			rf.DuplicateKeyTableValues = DuplicateKeyValues.Checked;
			rf.AllowExtraLengthFieldNames = AllowExtraLengthFieldNames.Checked;
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
			string txt = Preferences.Get("SdFileExportDefaults");
			do
			{
				if (!Lex.IsNullOrEmpty(txt))
					try
					{
						string[] sa = txt.Split('\t');
						rf.StructureFlags = (MoleculeTransformationFlags)int.Parse(sa[0]);
						rf.QualifiedNumberSplit = QnfSplitControl.DeserializeQualifiedNumberSplit(sa[1]);
						rf.DuplicateKeyTableValues = bool.Parse(sa[2]);
						rf.AllowExtraLengthFieldNames = bool.Parse(sa[3]);
						rf.RunInBackground = bool.Parse(sa[4]);
						break; // finish up
					}
					catch (Exception ex) { ex = ex; } // fall through to defaults on error

				rf.StructureFlags = MoleculeTransformationFlags.None;
				rf.QualifiedNumberSplit = // split by qualifier and numeric value
					QnfEnum.Split | QnfEnum.Qualifier | QnfEnum.NumericValue;
				rf.DuplicateKeyTableValues = false;
				rf.AllowExtraLengthFieldNames = false;
				rf.RunInBackground = false;
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
				((int)rf.StructureFlags).ToString() + '\t' +
				QnfSplitControl.SerializeQualifiedNumberSplit(rf.QualifiedNumberSplit) + '\t' +
				rf.DuplicateKeyTableValues + '\t' +
				rf.AllowExtraLengthFieldNames + '\t' +
				rf.RunInBackground;

			Preferences.Set("SdFileExportDefaults", txt);

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
			this.Frame2 = new System.Windows.Forms.GroupBox();
			this.RemoveHydrogens = new DevExpress.XtraEditors.CheckEdit();
			this.NoSuperAtom = new DevExpress.XtraEditors.CheckEdit();
			this.LargeFragOnly = new DevExpress.XtraEditors.CheckEdit();
			this.RemoveStereochemistry = new DevExpress.XtraEditors.CheckEdit();
			this.Label3 = new DevExpress.XtraEditors.LabelControl();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Frame1 = new System.Windows.Forms.GroupBox();
			this.StructureOnly = new DevExpress.XtraEditors.CheckEdit();
			this.AllData = new DevExpress.XtraEditors.CheckEdit();
			this.OpenSpotfire = new DevExpress.XtraEditors.CheckEdit();
			this.Label2 = new DevExpress.XtraEditors.LabelControl();
			this.Browse = new DevExpress.XtraEditors.SimpleButton();
			this.FileName = new DevExpress.XtraEditors.TextEdit();
			this.Label1 = new DevExpress.XtraEditors.LabelControl();
			this.ExportInBackground = new DevExpress.XtraEditors.CheckEdit();
			this.QualifiedNumberFormatting = new System.Windows.Forms.GroupBox();
			this.checkBox2 = new DevExpress.XtraEditors.CheckEdit();
			this.label5 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.AllowExtraLengthFieldNames = new DevExpress.XtraEditors.CheckEdit();
			this.DuplicateKeyValues = new DevExpress.XtraEditors.CheckEdit();
			this.SaveAsDefaultFolderOption = new DevExpress.XtraEditors.CheckEdit();
			this.QnfSplitOptions = new Mobius.ClientComponents.QnfSplitControl();
			this.Frame2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RemoveHydrogens.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NoSuperAtom.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LargeFragOnly.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RemoveStereochemistry.Properties)).BeginInit();
			this.Frame1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.StructureOnly.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AllData.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OpenSpotfire.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FileName.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExportInBackground.Properties)).BeginInit();
			this.QualifiedNumberFormatting.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBox2.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AllowExtraLengthFieldNames.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DuplicateKeyValues.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SaveAsDefaultFolderOption.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// Frame2
			// 
			this.Frame2.BackColor = System.Drawing.Color.Transparent;
			this.Frame2.Controls.Add(this.RemoveHydrogens);
			this.Frame2.Controls.Add(this.NoSuperAtom);
			this.Frame2.Controls.Add(this.LargeFragOnly);
			this.Frame2.Controls.Add(this.RemoveStereochemistry);
			this.Frame2.Controls.Add(this.Label3);
			this.Frame2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Frame2.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Frame2.Location = new System.Drawing.Point(6, 130);
			this.Frame2.Name = "Frame2";
			this.Frame2.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Frame2.Size = new System.Drawing.Size(402, 120);
			this.Frame2.TabIndex = 17;
			this.Frame2.TabStop = false;
			this.Frame2.Text = "Structure Output Options";
			// 
			// RemoveHydrogens
			// 
			this.RemoveHydrogens.Cursor = System.Windows.Forms.Cursors.Default;
			this.RemoveHydrogens.Location = new System.Drawing.Point(16, 42);
			this.RemoveHydrogens.Name = "RemoveHydrogens";
			this.RemoveHydrogens.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.RemoveHydrogens.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RemoveHydrogens.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.RemoveHydrogens.Properties.Appearance.Options.UseBackColor = true;
			this.RemoveHydrogens.Properties.Appearance.Options.UseFont = true;
			this.RemoveHydrogens.Properties.Appearance.Options.UseForeColor = true;
			this.RemoveHydrogens.Properties.Caption = "Remove hydrogen atoms";
			this.RemoveHydrogens.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RemoveHydrogens.Size = new System.Drawing.Size(304, 19);
			this.RemoveHydrogens.TabIndex = 15;
			// 
			// NoSuperAtom
			// 
			this.NoSuperAtom.Cursor = System.Windows.Forms.Cursors.Default;
			this.NoSuperAtom.Location = new System.Drawing.Point(17, 93);
			this.NoSuperAtom.Name = "NoSuperAtom";
			this.NoSuperAtom.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.NoSuperAtom.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NoSuperAtom.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.NoSuperAtom.Properties.Appearance.Options.UseBackColor = true;
			this.NoSuperAtom.Properties.Appearance.Options.UseFont = true;
			this.NoSuperAtom.Properties.Appearance.Options.UseForeColor = true;
			this.NoSuperAtom.Properties.Caption = "Remove any \"Super Atom\" information from the end of each molfile";
			this.NoSuperAtom.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.NoSuperAtom.Size = new System.Drawing.Size(368, 19);
			this.NoSuperAtom.TabIndex = 14;
			// 
			// LargeFragOnly
			// 
			this.LargeFragOnly.Cursor = System.Windows.Forms.Cursors.Default;
			this.LargeFragOnly.Location = new System.Drawing.Point(16, 18);
			this.LargeFragOnly.Name = "LargeFragOnly";
			this.LargeFragOnly.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.LargeFragOnly.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LargeFragOnly.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.LargeFragOnly.Properties.Appearance.Options.UseBackColor = true;
			this.LargeFragOnly.Properties.Appearance.Options.UseFont = true;
			this.LargeFragOnly.Properties.Appearance.Options.UseForeColor = true;
			this.LargeFragOnly.Properties.Caption = "Output only the largest fragment for each structure";
			this.LargeFragOnly.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.LargeFragOnly.Size = new System.Drawing.Size(342, 19);
			this.LargeFragOnly.TabIndex = 13;
			// 
			// RemoveStereochemistry
			// 
			this.RemoveStereochemistry.Cursor = System.Windows.Forms.Cursors.Default;
			this.RemoveStereochemistry.Location = new System.Drawing.Point(17, 68);
			this.RemoveStereochemistry.Name = "RemoveStereochemistry";
			this.RemoveStereochemistry.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.RemoveStereochemistry.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RemoveStereochemistry.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.RemoveStereochemistry.Properties.Appearance.Options.UseBackColor = true;
			this.RemoveStereochemistry.Properties.Appearance.Options.UseFont = true;
			this.RemoveStereochemistry.Properties.Appearance.Options.UseForeColor = true;
			this.RemoveStereochemistry.Properties.Caption = "Remove stereochemistry";
			this.RemoveStereochemistry.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RemoveStereochemistry.Size = new System.Drawing.Size(344, 19);
			this.RemoveStereochemistry.TabIndex = 11;
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
			this.Label3.TabIndex = 12;
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
			this.Cancel.Location = new System.Drawing.Point(340, 463);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(68, 24);
			this.Cancel.TabIndex = 14;
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
			this.OK.Location = new System.Drawing.Point(264, 463);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(68, 24);
			this.OK.TabIndex = 13;
			this.OK.Tag = "OK";
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// Frame1
			// 
			this.Frame1.BackColor = System.Drawing.Color.Transparent;
			this.Frame1.Controls.Add(this.StructureOnly);
			this.Frame1.Controls.Add(this.AllData);
			this.Frame1.Controls.Add(this.OpenSpotfire);
			this.Frame1.Controls.Add(this.Label2);
			this.Frame1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Frame1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Frame1.Location = new System.Drawing.Point(6, 39);
			this.Frame1.Name = "Frame1";
			this.Frame1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Frame1.Size = new System.Drawing.Size(402, 73);
			this.Frame1.TabIndex = 16;
			this.Frame1.TabStop = false;
			this.Frame1.Text = "Data to be written to the SDfile";
			// 
			// StructureOnly
			// 
			this.StructureOnly.Cursor = System.Windows.Forms.Cursors.Default;
			this.StructureOnly.Location = new System.Drawing.Point(17, 45);
			this.StructureOnly.Name = "StructureOnly";
			this.StructureOnly.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.StructureOnly.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StructureOnly.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.StructureOnly.Properties.Appearance.Options.UseBackColor = true;
			this.StructureOnly.Properties.Appearance.Options.UseFont = true;
			this.StructureOnly.Properties.Appearance.Options.UseForeColor = true;
			this.StructureOnly.Properties.Caption = "Structures and compound ids only";
			this.StructureOnly.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.StructureOnly.Properties.RadioGroupIndex = 1;
			this.StructureOnly.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.StructureOnly.Size = new System.Drawing.Size(235, 19);
			this.StructureOnly.TabIndex = 9;
			this.StructureOnly.TabStop = false;
			// 
			// AllData
			// 
			this.AllData.Cursor = System.Windows.Forms.Cursors.Default;
			this.AllData.EditValue = true;
			this.AllData.Location = new System.Drawing.Point(17, 19);
			this.AllData.Name = "AllData";
			this.AllData.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.AllData.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AllData.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.AllData.Properties.Appearance.Options.UseBackColor = true;
			this.AllData.Properties.Appearance.Options.UseFont = true;
			this.AllData.Properties.Appearance.Options.UseForeColor = true;
			this.AllData.Properties.Caption = "&All selected data fields";
			this.AllData.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.AllData.Properties.RadioGroupIndex = 1;
			this.AllData.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.AllData.Size = new System.Drawing.Size(235, 19);
			this.AllData.TabIndex = 8;
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
			this.Browse.Location = new System.Drawing.Point(340, 8);
			this.Browse.Name = "Browse";
			this.Browse.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Browse.Size = new System.Drawing.Size(68, 22);
			this.Browse.TabIndex = 12;
			this.Browse.Text = "&Browse...";
			this.Browse.Click += new System.EventHandler(this.Browse_Click);
			// 
			// FileName
			// 
			this.FileName.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.FileName.Location = new System.Drawing.Point(108, 9);
			this.FileName.Name = "FileName";
			this.FileName.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.FileName.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FileName.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.FileName.Properties.Appearance.Options.UseBackColor = true;
			this.FileName.Properties.Appearance.Options.UseFont = true;
			this.FileName.Properties.Appearance.Options.UseForeColor = true;
			this.FileName.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.FileName.Size = new System.Drawing.Size(227, 20);
			this.FileName.TabIndex = 11;
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
			this.Label1.Location = new System.Drawing.Point(8, 9);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(85, 13);
			this.Label1.TabIndex = 15;
			this.Label1.Text = "Output File Name:";
			// 
			// ExportInBackground
			// 
			this.ExportInBackground.Cursor = System.Windows.Forms.Cursors.Default;
			this.ExportInBackground.Location = new System.Drawing.Point(22, 423);
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
			this.ExportInBackground.TabIndex = 18;
			// 
			// QualifiedNumberFormatting
			// 
			this.QualifiedNumberFormatting.BackColor = System.Drawing.Color.Transparent;
			this.QualifiedNumberFormatting.Controls.Add(this.QnfSplitOptions);
			this.QualifiedNumberFormatting.Controls.Add(this.checkBox2);
			this.QualifiedNumberFormatting.Controls.Add(this.label5);
			this.QualifiedNumberFormatting.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.QualifiedNumberFormatting.ForeColor = System.Drawing.SystemColors.ControlText;
			this.QualifiedNumberFormatting.Location = new System.Drawing.Point(6, 266);
			this.QualifiedNumberFormatting.Name = "QualifiedNumberFormatting";
			this.QualifiedNumberFormatting.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.QualifiedNumberFormatting.Size = new System.Drawing.Size(402, 74);
			this.QualifiedNumberFormatting.TabIndex = 36;
			this.QualifiedNumberFormatting.TabStop = false;
			this.QualifiedNumberFormatting.Text = "Formatting of \"Qualified\" numeric values (e.g. >50) ";
			// 
			// checkBox2
			// 
			this.checkBox2.Cursor = System.Windows.Forms.Cursors.Default;
			this.checkBox2.Location = new System.Drawing.Point(33, 122);
			this.checkBox2.Name = "checkBox2";
			this.checkBox2.Properties.Appearance.BackColor = System.Drawing.SystemColors.Control;
			this.checkBox2.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBox2.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.checkBox2.Properties.Appearance.Options.UseBackColor = true;
			this.checkBox2.Properties.Appearance.Options.UseFont = true;
			this.checkBox2.Properties.Appearance.Options.UseForeColor = true;
			this.checkBox2.Properties.Caption = "Open output file with Spotfire (Spotfire must be installed)";
			this.checkBox2.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.checkBox2.Size = new System.Drawing.Size(344, 19);
			this.checkBox2.TabIndex = 15;
			// 
			// label5
			// 
			this.label5.Appearance.BackColor = System.Drawing.SystemColors.Control;
			this.label5.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.label5.Appearance.Options.UseBackColor = true;
			this.label5.Appearance.Options.UseFont = true;
			this.label5.Appearance.Options.UseForeColor = true;
			this.label5.Cursor = System.Windows.Forms.Cursors.Default;
			this.label5.Location = new System.Drawing.Point(58, 140);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(521, 13);
			this.label5.TabIndex = 16;
			this.label5.Text = "Note: You can open an existing Spotfire file from the main menu with the expert c" +
    "ommand: SPOTFIRE filename";
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(-4, 449);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(425, 10);
			this.labelControl1.TabIndex = 37;
			// 
			// AllowExtraLengthFieldNames
			// 
			this.AllowExtraLengthFieldNames.Cursor = System.Windows.Forms.Cursors.Default;
			this.AllowExtraLengthFieldNames.Location = new System.Drawing.Point(23, 373);
			this.AllowExtraLengthFieldNames.Name = "AllowExtraLengthFieldNames";
			this.AllowExtraLengthFieldNames.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.AllowExtraLengthFieldNames.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AllowExtraLengthFieldNames.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.AllowExtraLengthFieldNames.Properties.Appearance.Options.UseBackColor = true;
			this.AllowExtraLengthFieldNames.Properties.Appearance.Options.UseFont = true;
			this.AllowExtraLengthFieldNames.Properties.Appearance.Options.UseForeColor = true;
			this.AllowExtraLengthFieldNames.Properties.Caption = "Allow field names to exceed the normal 30 character limit";
			this.AllowExtraLengthFieldNames.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.AllowExtraLengthFieldNames.Size = new System.Drawing.Size(342, 19);
			this.AllowExtraLengthFieldNames.TabIndex = 38;
			// 
			// DuplicateKeyValues
			// 
			this.DuplicateKeyValues.Cursor = System.Windows.Forms.Cursors.Default;
			this.DuplicateKeyValues.Location = new System.Drawing.Point(23, 348);
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
			// SaveAsDefaultFolderOption
			// 
			this.SaveAsDefaultFolderOption.Cursor = System.Windows.Forms.Cursors.Default;
			this.SaveAsDefaultFolderOption.Location = new System.Drawing.Point(23, 398);
			this.SaveAsDefaultFolderOption.Name = "SaveAsDefaultFolderOption";
			this.SaveAsDefaultFolderOption.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.SaveAsDefaultFolderOption.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SaveAsDefaultFolderOption.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SaveAsDefaultFolderOption.Properties.Appearance.Options.UseBackColor = true;
			this.SaveAsDefaultFolderOption.Properties.Appearance.Options.UseFont = true;
			this.SaveAsDefaultFolderOption.Properties.Appearance.Options.UseForeColor = true;
			this.SaveAsDefaultFolderOption.Properties.Caption = "Use this as the default export folder in the future";
			this.SaveAsDefaultFolderOption.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.SaveAsDefaultFolderOption.Size = new System.Drawing.Size(256, 19);
			this.SaveAsDefaultFolderOption.TabIndex = 47;
			this.SaveAsDefaultFolderOption.TabStop = false;
			this.SaveAsDefaultFolderOption.Tag = "FixedHeightStructs";
			// 
			// QnfSplitOptions
			// 
			this.QnfSplitOptions.Location = new System.Drawing.Point(12, 18);
			this.QnfSplitOptions.Name = "QnfSplitOptions";
			this.QnfSplitOptions.Size = new System.Drawing.Size(359, 49);
			this.QnfSplitOptions.TabIndex = 38;
			// 
			// SetupSdFile
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(418, 492);
			this.Controls.Add(this.SaveAsDefaultFolderOption);
			this.Controls.Add(this.DuplicateKeyValues);
			this.Controls.Add(this.AllowExtraLengthFieldNames);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.QualifiedNumberFormatting);
			this.Controls.Add(this.ExportInBackground);
			this.Controls.Add(this.Frame2);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Frame1);
			this.Controls.Add(this.Browse);
			this.Controls.Add(this.FileName);
			this.Controls.Add(this.Label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SetupSdFile";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Export SDfile";
			this.Activated += new System.EventHandler(this.SetupSdFile_Activated);
			this.VisibleChanged += new System.EventHandler(this.SetupSdFile_VisibleChanged);
			this.Frame2.ResumeLayout(false);
			this.Frame2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RemoveHydrogens.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NoSuperAtom.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LargeFragOnly.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RemoveStereochemistry.Properties)).EndInit();
			this.Frame1.ResumeLayout(false);
			this.Frame1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.StructureOnly.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AllData.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OpenSpotfire.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FileName.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ExportInBackground.Properties)).EndInit();
			this.QualifiedNumberFormatting.ResumeLayout(false);
			this.QualifiedNumberFormatting.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBox2.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AllowExtraLengthFieldNames.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DuplicateKeyValues.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SaveAsDefaultFolderOption.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private void SetupSdFile_Activated(object sender, System.EventArgs e)
		{
			FileName.Focus();
		}

		private void Browse_Click(object sender, System.EventArgs e)
		{
			string fileName = FileName.Text;
			if (Lex.IsNullOrEmpty(fileName))
				fileName = ClientDirs.DefaultMobiusUserDocumentsFolder;

			string filter = "SDfile (*.sdf)|*.sdf";
			fileName = UIMisc.GetSaveAsFilename("SDfile Name", fileName, filter, ".sdf");
			if (fileName=="") return;
			FileName.Text = fileName;
		}

		private void OK_Click(object sender, System.EventArgs e)
		{
			string fullFileName = UIMisc.CheckFileName(2, FileName, InitialName, ClientDirs.DefaultMobiusUserDocumentsFolder, ".sdf");
			if (fullFileName=="") return;
			if (!UIMisc.CanWriteFile(fullFileName, true)) return;
			FileName.Text = fullFileName;

			if (ExportInBackground.Checked)
			{ // if background export of unc file tell user if we can't write directly to the file
				if (UIMisc.CanWriteFileFromServiceAccount(fullFileName) == DialogResult.Cancel) return;
                if (_alertExport && UIMisc.PathContainsDrive(fullFileName) == DialogResult.Cancel) return;
            }

			DialogResult = DialogResult.OK;
		}

		private void SetupSdFile_VisibleChanged(object sender, EventArgs e)
		{
			if (Visible) InitialName = FileName.Text; // save initial name for later compare
		}
	}
}
