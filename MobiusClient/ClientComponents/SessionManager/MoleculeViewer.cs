using Mobius.ComOps;
using Mobius.Data;
using Mobius.MolLib1;
//using Mobius.ClientControls; // assign later?
using Mobius.Helm;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraEditors;
using DevExpress.XtraTab;

namespace Mobius.ClientComponents
{
/// <summary>
/// Test structure conversion
/// </summary>

	public partial class MoleculeViewer : DevExpress.XtraEditors.XtraForm
	{

		/// <summary>
		/// Molecule currently being viewed
		/// </summary>

		public MoleculeMx Molecule
		{
			get => MoleculeControl.Molecule;
			set => MoleculeControl.Molecule = value;
		}

		public MoleculeMx OriginalMol = null; // mol upon entry
		public MoleculeMx EditedMol = null; // mol after edit

		MoleculeRendererType DisplayFormat => MoleculeControl.RendererType; // Current molecule display format

		bool IsMolfileView => AtomBondView.Checked;

		bool IsHelmView => HelmView.Checked;

		bool InSetup = false;

		/// <summary>
		/// Constructor
		/// </summary>

		public MoleculeViewer()
		{
			InitializeComponent();
			MoleculeControl.ShowLargerStructureInTooltip = false;
			MoleculeControl.ShowEnlargeStructureButton = false;
			MoleculeControl.AllowEditing = true;
		}

		/// <summary>
		/// Edit the Molecule (modal)
		/// </summary>
		/// <param name="originalMol"></param>
		/// <returns></returns>

		public static MoleculeMx EditMolecule(MoleculeMx originalMol)
		{
			MoleculeViewer mv = new MoleculeViewer();
			MoleculeMx copy = originalMol.Clone();
			mv.SetupForm(copy);

			DialogResult dr = mv.ShowDialog();

			if (dr != DialogResult.Cancel)
				return mv.EditedMol;

			else return null; // cancelled
		}

		/// <summary>
		/// Display the molecule
		/// </summary>

		public static void ShowMolecule(
			MoleculeMx mol,
			string title = null)
		{
			if (MoleculeMx.IsUndefined(mol)) return;

			MoleculeViewer mv = new MoleculeViewer();

			if (Lex.IsUndefined(title)) title = "MoleculeViewer";
			mv.Text = title;

			MoleculeMx copy = mol.Clone();
			mv.SetupForm(copy);
			mv.Show(); // show form and return;
			return; 
		}

		/// <summary>
		/// Setup the form for the current molecule
		/// </summary>
		/// <param name="mol"></param>

		public void SetupForm(MoleculeMx mol)
		{
			if (InSetup) return;
			InSetup = true;

			try
			{
				Molecule = mol;

				ClearViewsTabControls(null);

				if (mol.IsChemStructureFormat)
				{
					string molfile = Lex.AdjustEndOfLineCharacters(mol.GetMolfileString(), "\r\n");
					MolfileStringEdit.Text = molfile;
					ViewsTabControl.SelectedTabPage = MolfileTab;
					AtomBondView.Checked = true;
				}

				else if (mol.PrimaryFormat == MoleculeFormat.Helm)
				{
					string helm = MoleculeControl.Molecule.HelmString;
					HelmStringEdit.Text = helm;
					ViewsTabControl.SelectedTabPage = HelmTab;
					HelmView.Checked = true;
				}

			}
			catch (Exception ex) { ex = ex; }

			finally { InSetup = false; }
		}
		private void RetrieveModel_Click(object sender, EventArgs e)
		{
			Point p = RetrieveModel.PointToScreen(new System.Drawing.Point(0, RetrieveModel.Height));
			MoleculeSelectorControl.ShowModelSelectionMenu(p, MoleculeControl, StructureSearchType.Related);
		}

		private void RetrieveRecentButton_Click(object sender, EventArgs e)
		{
			MoleculeSelectorControl.SelectMruMolecule(MoleculeControl);
			MoleculeControl.Focus(); // move focus away
		}

		private void RetrieveFavoritesButton_Click(object sender, EventArgs e)
		{
			MoleculeSelectorControl.SelectFavoriteMolecule(MoleculeControl);
			MoleculeControl.Focus(); // move focus away
		}

		private void AddToFavoritesButton_Click(object sender, EventArgs e)
		{
			Point p = RetrieveModel.PointToScreen(new System.Drawing.Point(0, AddToFavoritesButton.Height));
			MoleculeSelectorControl.AddToFavoritesList(MoleculeControl, StructureSearchType.Related);
			MoleculeControl.Focus(); // move focus away
		}

		private void EditMolecule_Click(object sender, EventArgs e)
		{
			DelayedCallback.Schedule(EditMolecule);
		}

		void EditMolecule()
		{
			MoleculeControl.EditMolecule();
		}

		private void MoleculeControl_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			SetupForm(MoleculeControl.Molecule);
			return; 
		}

		private void HelmStringEdit_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
		{
			return; 
		}

		private void UpdateMolButton_Click(object sender, EventArgs e)
		{
			DelayedCallback.Schedule(UpdateMolecule);
		}

/// <summary>
/// Update the molecule from the text control on the current tab
/// </summary>

		void UpdateMolecule()
		{
			if (ViewsTabControl.SelectedTabPage == SmilesTab)
			{
				string smiles = SmilesStringEdit.Text;
				string molFile = MoleculeMx.SmilesStringToMolFile(smiles);
				if (Lex.IsUndefined(molFile))
				{
					MessageBoxMx.ShowError("Not a valid Smiles string:\r\n" + smiles);
					return;
				}

				SetupAndRenderMolecule(MoleculeFormat.Smiles, smiles, SmilesTab);
			}

			else if (ViewsTabControl.SelectedTabPage == MolfileTab)
			{
				string molfile = MolfileStringEdit.Text;
				if (!MolLib1.StructureConverter.IsValidMolfile(molfile))
				{
					MessageBoxMx.ShowError("The text entered is not a valid molfile");
					return;
				}

				SetupAndRenderMolecule(MoleculeFormat.Molfile, molfile, MolfileTab);
			}

			else if (ViewsTabControl.SelectedTabPage == ChimeTab)
			{
				string chime = ChimeStringEdit.Text;
				if (!MolLib1.StructureConverter.IsValidChimeString(chime))
				{
					MessageBoxMx.ShowError("The text entered is not a valid Chime string");
					return;
				}

				SetupAndRenderMolecule(MoleculeFormat.Chime, chime, ChimeTab);
			}

			else if (ViewsTabControl.SelectedTabPage == HelmTab)
			{
				string helm = HelmStringEdit.Text;
				if (!MoleculeMx.IsValidHelmString(helm))
				{
					MessageBoxMx.ShowError("The text entered is not valid HELM");
					return;
				}

				SetupAndRenderMolecule(MoleculeFormat.Helm, helm, HelmTab);
			}

			else if (ViewsTabControl.SelectedTabPage == SequenceTab)
			{
				string seq = SequenceStringEdit.Text;
				if (Lex.IsUndefined(seq))
				{
					MessageBoxMx.ShowError("The text entered is not valid sequence");
					return;
				}

				string helm = HelmConverter.SequenceToHelm(seq);
				if (Lex.IsUndefined(helm))
				{
					MessageBoxMx.ShowError("Unable to convert the sequence to HELM");
					return;
				}

				SetupAndRenderMolecule(MoleculeFormat.Helm, helm, SequenceTab);
			}

			else if (ViewsTabControl.SelectedTabPage == FastaTab)
			{
				string fasta = FastaStringEdit.Text;
				if (Lex.IsUndefined(fasta))
				{
					MessageBoxMx.ShowError("The text entered is not valid FASTA string");
					return;
				}

				string helm = HelmConverter.FastaToHelm(fasta);
				if (Lex.IsUndefined(helm))
				{
					MessageBoxMx.ShowError("Unable to convert the FASTA string to HELM");
					return;
				}

				SetupAndRenderMolecule(MoleculeFormat.Helm, helm, FastaTab);
			}

			return;
		}

		/// <summary>
		/// Setup a new molecule and render it
		/// </summary>
		/// <param name="molFormatType"></param>
		/// <param name="molString"></param>
		/// <param name="sourceTab"></param>

		void SetupAndRenderMolecule(
			MoleculeFormat molFormatType,
			string molString,
			XtraTabPage sourceTab)
		{
			InSetup = true;

			MoleculeMx mol = new MoleculeMx(molFormatType, molString);
			MoleculeControl.SetupAndRenderMolecule(mol);
			ClearViewsTabControls(sourceTab);

			if (molFormatType == MoleculeFormat.Helm)
					HelmView.Checked = true;

				else AtomBondView.Checked = true;

			InSetup = false;

			return;
		}

		void ClearViewsTabControls(XtraTabPage exceptedTab)
		{
			if (exceptedTab != SmilesTab)
				ClearTextControl(SmilesStringEdit);

			if (exceptedTab != MolfileTab)
				ClearTextControl(MolfileStringEdit);

			if (exceptedTab != ChimeTab)
				ClearTextControl(ChimeStringEdit);

			if (exceptedTab != HelmTab)
				ClearTextControl(HelmStringEdit);

			if (exceptedTab != SequenceTab)
				ClearTextControl(SequenceStringEdit);

			if (exceptedTab != FastaTab)
				ClearTextControl(FastaStringEdit);

			if (exceptedTab != PropertiesTab)
				ClearPropertiesTabControls();

			return;
		}

		/// <summary>
		/// Switched to a new tab page, update values if needed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ViewsTabControl_SelectedPageChanged(object sender, TabPageChangedEventArgs e)
		{
			DelayedCallback.Schedule(SelectedPageChanged);
		}

		void SelectedPageChanged()
		{
			string txt = null;
			XtraTabPage tab = ViewsTabControl.SelectedTabPage;

			if (tab == SmilesTab && !HasBeenSet(SmilesStringEdit))
			{
				SetTextControl(SmilesStringEdit, GetMolString(MoleculeFormat.Smiles));
			}

			else if (tab == MolfileTab && !HasBeenSet(MolfileStringEdit))
			{
				SetTextControl(MolfileStringEdit, GetMolString(MoleculeFormat.Molfile));
			}

			else if (tab == ChimeTab && !HasBeenSet(ChimeStringEdit))
			{
				SetTextControl(ChimeStringEdit, GetMolString(MoleculeFormat.Chime));
			}

			else if (tab == HelmTab && !HasBeenSet(HelmStringEdit))
			{
				SetTextControl(HelmStringEdit, GetMolString(MoleculeFormat.Helm));
			}

			else if (tab == SequenceTab && !HasBeenSet(SequenceStringEdit))
			{
				SetTextControl(SequenceStringEdit, GetMolString(MoleculeFormat.Sequence));
			}

			else if (tab == FastaTab && !HasBeenSet(FastaStringEdit))
			{
				SetTextControl(FastaStringEdit, GetMolString(MoleculeFormat.Fasta));
			}

			else if (tab == PropertiesTab && !HasBeenSet(FormulaString))
			{
				SetupPropertiesTab();
			}
		}

		/// <summary>
		/// Get the molecule string of the current molecule in the specified format
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>

		string GetMolString(MoleculeFormat format)
		{
			string molString;

			if (!Molecule.TryGetMoleculeString(format, out molString))
				molString = "Unable to convert molecule to this format";

			molString = Lex.AdjustEndOfLineCharacters(molString, "\r\n");
			return molString;
		}

		private void SetupPropertiesTab()
		{
			ClearPropertiesTabControls();

			MoleculeMx mol = Molecule;

			if (mol.IsBiopolymerFormat) // will need molfile for biopolymer to calculate props
				Molecule.GetMolfileString();

			string mf = mol.MolFormula;
			SetTextControl(FormulaString, mf);

			WeightString.Text = mol.MolWeight.ToString();
			HeavyAtomsString.Text = mol.HeavyAtomCount.ToString();
			TotalAtomsString.Text = mol.AtomCount.ToString();
			return;
		}

		void ClearPropertiesTabControls()
		{
			ClearTextControl(WeightString);
			ClearTextControl(FormulaString);
			ClearTextControl(HeavyAtomsString);
			ClearTextControl(TotalAtomsString);
		}

		void SetTextControl(
			TextEdit ctl,
			string txt)
		{
			ctl.Text = txt;
			ctl.Tag = "True"; // indicate value has been set for the current molecule
		}

		void ClearTextControl(
			TextEdit ctl)
		{
			ctl.Text = "";
			ctl.Tag = null; // indicate value has not been set
		}

		bool HasBeenSet(TextEdit ctl)
		{
			return (ctl.Tag != null);
		}

		/// <summary>
		/// Switch to Structure display mode
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void AtomBondView_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			if (!AtomBondView.Checked) return;

			if (MoleculeControl.RendererType == MoleculeRendererType.Chemistry) return;  // already in chemistry format?

			DelayedCallback.Schedule(SwitchToAtomBondView);
		}

		void SwitchToAtomBondView()
		{
			MoleculeMx mol = MoleculeControl.Molecule;
			if (Lex.IsUndefined(mol.MolfileString)) // may need to convert helm to molfile
			{
				string molfile = mol.GetMolfileString(); 
				mol.MolfileString = molfile;
			}

			mol.PrimaryFormat = MoleculeFormat.Molfile;

			MoleculeControl.Molecule = mol; // redisplay in new form
			return;
		}

		/// <summary>
		/// Switch to BioPolymer display mode
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void HelmView_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			if (!HelmView.Checked) return;

			if (MoleculeControl.RendererType == MoleculeRendererType.Helm) return;  // already Helm?

			DelayedCallback.Schedule(SwitchToHelmView);
		}

		void SwitchToHelmView()
		{
			MoleculeMx mol = MoleculeControl.Molecule;
			if (Lex.IsUndefined(mol.HelmString)) 
			{
				string helm = MoleculeControl.Molecule.GetHelmString();
				mol.HelmString = helm;
			}

			mol.PrimaryFormat = MoleculeFormat.Helm;

			MoleculeControl.Molecule = mol; // redisplay in new form
			return;
		}


		private void CloseButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}