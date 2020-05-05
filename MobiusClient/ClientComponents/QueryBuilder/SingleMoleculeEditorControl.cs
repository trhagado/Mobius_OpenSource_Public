using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

/// <summary>
/// Molecule control with standard editor controls
/// Note: Not currently complete or used
/// </summary>

	public partial class SingleMoleculeEditorControl : DevExpress.XtraEditors.XtraUserControl
	{

		bool InSetup = false;

		public SingleMoleculeEditorControl()
		{
			InitializeComponent();
		}

		public void SetupControl(
			MoleculeMx molecule)
		{
			InSetup = true;

			MoleculeControl.SetupAndRenderMolecule(molecule);

			MolDisplayFormatEdit.Text =
				molecule.PrimaryDisplayFormat == MoleculeRendererType.Helm ? "Biopolymer" : "Structure";

			InSetup = false;
			return;
		}

		private void RetrieveModel_Click(object sender, EventArgs e)
		{
		}

		private void RetrieveRecentButton_Click(object sender, EventArgs e)
		{
		}

		private void RetrieveFavoritesButton_Click(object sender, EventArgs e)
		{
		}

		private void AddToFavoritesButton_Click(object sender, EventArgs e)
		{
		}

		private void EditStructure_Click(object sender, EventArgs e)
		{
		}

		private void MolDisplayFormatPic_Click(object sender, EventArgs e)
		{
			MolDisplayFormatEdit.PerformClick(MolDisplayFormatEdit.Properties.Buttons[0]);
		}
	}
}
