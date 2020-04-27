using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;

using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Mobius.ClientComponents
{
	public partial class SetupSpotfire : XtraForm
	{

		static SetupSpotfire Instance;

		string InitialName = ""; // initial name for file when dialog entered
		ResultsFormat Rf = null;

		static internal int SpotfireFileCount = 0; // counter for Spotfire file written
		static Dictionary<string, string> QNameToExpFileName = new Dictionary<string, string>();

		public SetupSpotfire()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Setup & show the dialog
		/// </summary>
		/// <param name="rf"></param>

		public static new DialogResult ShowDialog(ResultsFormat rf)
		{
			if (Instance == null) Instance = new SetupSpotfire();
			return Instance.ShowDialog2(rf);
		}

		public DialogResult ShowDialog2(ResultsFormat rf)
		{

// Setup the dialog

			Rf = rf;
			rf.OutputDestination = OutputDest.Spotfire;

			GetDefaultExportParameters(rf);

			string dir = DirectoryMx.IncludeTerminalBackSlash(ClientDirs.DefaultMobiusUserDocumentsFolder);

			string qName = Rf.Query.UserObject.Name;
			string fName = dir;

			if (Lex.IsDefined(qName))
			{
				if (QNameToExpFileName.ContainsKey(qName))
					fName = QNameToExpFileName[qName];

				else fName = dir + qName + ".stdf";
			}

			FileName.Text = fName;

			if (rf.ExportStructureFormat == ExportStructureFormat.Chime)
				ChimeStringStructure.Checked = true;

			else if (rf.ExportStructureFormat == ExportStructureFormat.Smiles)
				SmilesStructure.Checked = true;

			else MolfileStructure.Checked = true;

			QnfSplitOptions.Setup(rf.QualifiedNumberSplit);

			DuplicateKeyValues.Checked = rf.DuplicateKeyTableValues;

// Show the dialog

			DialogResult dr = ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return dr;

			// Get the dialog values

			SpotfireFileCount++;

			rf.OutputFileName = FileName.Text;

			if (Lex.IsDefined(qName) && Lex.IsDefined(FileName.Text))
				QNameToExpFileName[qName] = FileName.Text;

			if (SingleFileOption.Checked)
				rf.OutputFileName2 = "Single";

			else if (MultipleFilesOptions.Checked)
				rf.OutputFileName2 = "Multiple";

			else rf.OutputFileName2 = "SingleAndMultiple";

			if (StdfCheckEdit.Checked)
				rf.ExportFileFormat = ExportFileFormat.Stdf;

			else rf.ExportFileFormat = ExportFileFormat.Sbdf;

			if (ChimeStringStructure.Checked)
				rf.ExportStructureFormat = ExportStructureFormat.Chime;

			else if (SmilesStructure.Checked)
				rf.ExportStructureFormat = ExportStructureFormat.Smiles;

			else rf.ExportStructureFormat = ExportStructureFormat.Molfile;

			rf.IncludeDataTypes = true;

			rf.DuplicateKeyTableValues = DuplicateKeyValues.Checked;

			rf.QualifiedNumberSplit = QnfSplitOptions.Get();

			SaveDefaultExportParameters(rf);

			return DialogResult.OK;
		}
		/// <summary>
		/// Get default export parameters for user
		/// </summary>
		/// <param name="rf"></param>

		void GetDefaultExportParameters(ResultsFormat rf)
		{
			string txt = Preferences.Get("SpotfireExportDefaults");
			do
			{
				if (!Lex.IsNullOrEmpty(txt))
					try
					{
						string[] sa = txt.Split('\t');
						rf.OpenMode = (SpotfireOpenModeEnum)int.Parse(sa[0]);
						rf.QualifiedNumberSplit = QnfSplitControl.DeserializeQualifiedNumberSplit(sa[1]);
						rf.DuplicateKeyTableValues = bool.Parse(sa[2]);
						rf.ViewStructures = bool.Parse(sa[3]);
						break; // finish up
					}
					catch (Exception ex) { ex = ex; } // fall through to defaults on error

				rf.OpenMode = SpotfireOpenModeEnum.None;
				rf.QualifiedNumberSplit = // split by qualifier and numeric value
					QnfEnum.Split | QnfEnum.Qualifier | QnfEnum.NumericValue;
				rf.DuplicateKeyTableValues = false;
				rf.ViewStructures = false;
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
				((int)rf.OpenMode).ToString() + '\t' +
				QnfSplitControl.SerializeQualifiedNumberSplit(rf.QualifiedNumberSplit) + '\t' +
				rf.DuplicateKeyTableValues + '\t' +
				rf.ViewStructures;

			Preferences.Set("SpotfireExportDefaults", txt);
			return;
		}

		private void OK_Click(object sender, System.EventArgs e)
		{
			bool fileCheckEnabled = true;

			string fullFileName = UIMisc.CheckFileName(2, FileName, InitialName, ClientDirs.DefaultMobiusUserDocumentsFolder, "");
			if (fullFileName == "") return;
			if (StdfCheckEdit.Checked)
			{
				fullFileName = Lex.Replace(fullFileName, ".sbdf", ".stdf");
				if (!Lex.Contains(fullFileName, ".stdf")) fullFileName += ".stdf";
			}
			else
			{
				fullFileName = Lex.Replace(fullFileName, ".stdf", ".sbdf");
				if (!Lex.Contains(fullFileName, ".sbdf")) fullFileName += ".sbtdf";
			}

			if (fileCheckEnabled && !UIMisc.CanWriteFile(fullFileName, true)) return;
			FileName.Text = fullFileName;

			DialogResult = DialogResult.OK;
		}

		private void SetupSpotfire_Activated(object sender, EventArgs e)
		{
			FileName.Focus();
			FileName.Select(FileName.Text.Length, 0);

			//			FolderName.Focus();
			//			FolderName.Select(FolderName.Text.Length, 0);
		}

		private void Browse_Click(object sender, System.EventArgs e)
		{
			string fileName;
			string initialFile = FileName.Text;

			if (Lex.IsNullOrEmpty(initialFile) || Lex.Eq(initialFile, ClientDirs.DefaultMobiusUserDocumentsFolder))
			{
				initialFile = ClientDirs.DefaultMobiusUserDocumentsFolder;
				initialFile += @"\" + Rf.Query.UserObject.Name;
			}

			string defaultExt = ".stdf";
			string filter = GetFileFilter();

			fileName = UIMisc.GetSaveAsFilename("Export Data", initialFile, filter, defaultExt);
			if (fileName != "") FileName.Text = fileName;
			return;
		}

		string GetFileFilter()
		{
			return
				"TIBCO Spotfire Text Data Format (*.stdf)|*.stdf|" +
				"All files (*.*)|*.*";
		}

	}

}
