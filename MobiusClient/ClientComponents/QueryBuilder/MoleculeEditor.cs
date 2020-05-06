using Mobius.ComOps;
using Mobius.Data;
using Mobius.Helm;

using DevExpress.XtraEditors;

using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class MoleculeEditor : DevExpress.XtraEditors.XtraForm
	{
		static int LogicalPixelsX = -1, LogicalPixelsY = -1;

		bool MoleculeChanged = false;

		public static MoleculeEditor Instance => GetInstance();

		private System.ComponentModel.IContainer Components = null;

		public static string SDFFolder = "";
		public static int FileCount;

		public static int PopupPos = 100; // position for next popup window
		public static int PopupPosDelta = 20; // amount to move for next
		public static int PopupCount = 0; // number of popups shown
		public static int PopupCycleCount = 6; // number of popups to show before returning to initial position

		public static bool DisplayedUncWarningMessage = false;
		public static bool InDoevents = false;

		[DllImport("user32.dll", EntryPoint = "SystemParametersInfo", SetLastError = true)]
		public static extern bool SystemParametersInfoGet(
			uint action, uint param, ref uint vparam, uint init);

		[DllImport("user32.dll", EntryPoint = "SystemParametersInfo", SetLastError = true)]
		static extern bool SystemParametersInfoSet(
			uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

		[DllImport("user32.dll")]
		public static extern IntPtr GetFocus();

		public MoleculeEditor()
		{
			InitializeComponent();

			return;
		}

		public static MoleculeEditor GetInstance()
		{
			if (_instance == null) _instance = new MoleculeEditor();
			return _instance;
		}
		private static MoleculeEditor _instance = null;

		/// <summary>
		/// Modal edit of molecule
		/// </summary>

		public static MoleculeMx Edit(
			MoleculeMx mol,
			MoleculeRendererType editorType = MoleculeRendererType.Unknown,
			string title = "")
		{
			MoleculeMx mol2 = null;

			try
			{
				if (editorType == MoleculeRendererType.Unknown) // need to assign editor type?
				{
					if (mol.IsChemStructureFormat)
						editorType = MoleculeRendererType.Chemistry;

					else if (mol.IsBiopolymerFormat)
						editorType = MoleculeRendererType.Helm;

					else editorType = MoleculeRendererType.Chemistry; // final default
				}

				bool useChemEditor = (editorType != MoleculeRendererType.Helm);

				if (useChemEditor)
				{
					mol2 = mol.ConvertTo(MoleculeFormat.Molfile);
					return EditMolLib1Molecule(mol2, title);
				}

				else // use Helm editor
				{
					return EditHelmMolecule(mol, title);
				}

			}

			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message, "Error editing molecule");
				return null;
			}
		}

		/// <summary>
		/// Edit Molecule
		/// </summary>
		/// <param name="title"></param>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static MoleculeMx EditMolLib1Molecule(
			MoleculeMx mol,
			string title = "")
		{
			MoleculeMx mol2 = null;
			MoleculeFormat molFormat;
			string molString;

			try
			{
				Instance.MoleculeCtl.Molecule = mol;
				ICdkMolControl molCtl = Instance.MoleculeCtl.MoleculeCtl;
				molCtl.EditorReturnedHandler = new MolEditorReturnedHandler(Instance.MolLibEditorReturned);

				Instance.Text = title;
				//molCtl.RenditorName = title; // shows as tooltip on editor Done button
				Instance.MoleculeCtl.InSetup = true;
				MoleculeMx.SetRendererStructure(molCtl, mol);
				Instance.MoleculeCtl.InSetup = false;
				Instance.MoleculeChanged = false;

				DialogResult dr = // Show the form. The shown event invokes Draw & the dialog is ended when drawing is done
					Instance.ShowDialog(SessionManager.ActiveForm);

				if (dr != DialogResult.OK) return null;

				molCtl.GetMolecule(out molFormat, out molString);
				mol2 = new MoleculeMx(molFormat, molString);
				return mol2;
			}

			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message, "Error editing structure");
				Instance.MoleculeCtl.InSetup = false;
				return null;
			}
		}

		/// <summary>
		/// This event occurs when the UIMisc form is shown to do modal structure editing.
		/// We activate Draw & the EditorReturned event ends the dialog
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void MoleculeEditor_Shown(object sender, EventArgs e)
		{
			DelayedCallback.Schedule(Call_MolLib1_EditStructure); // delay so this form has time to render and we exit event
		}

		private void Call_MolLib1_EditStructure()
		{
			CdkMx.CdkMolControl.EditStructure(Instance.MoleculeCtl.MoleculeCtl);
			return;
		}

/// <summary>
/// Method called when editing complete
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void MolLibEditorReturned(object sender, MolEditorReturnedEventArgs e)
		{
			if (MoleculeChanged)
			{
				DialogResult = DialogResult.OK; // hide form
			}

			else DialogResult = DialogResult.Cancel;
		}

/// <summary>
/// EditHelmMolecule
/// </summary>
/// <param name="title"></param>
/// <param name="mol"></param>
/// <returns></returns>

		public static MoleculeMx EditHelmMolecule(
			MoleculeMx mol,
			string title = "")
		{
			MoleculeMx mol2 = mol.ConvertTo(MoleculeFormat.Helm);

			string helm = mol2.HelmString;

			HelmEditorDialog editor = new HelmEditorDialog();
			string newHelm = editor.Edit(helm);
			if (newHelm == null) return null;

			mol2.SetPrimaryTypeAndValue(MoleculeFormat.Helm, newHelm);
			return mol2;
		}


/// <summary>
/// Set flag that molecule has changed
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void MoleculeCtl_EditValueChanged(object sender, EventArgs e)
		{
			MoleculeChanged = true;
		}

	}
}
