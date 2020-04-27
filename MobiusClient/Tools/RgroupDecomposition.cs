using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;
using Mobius.MolLib1;

using DevExpress.XtraEditors;
using DevExpress.Data;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.Tools
{
	/// <summary>
	/// Summary description for RgroupDecomposition.
	/// 
	/// </summary>
	
	public partial class RgroupDecomposition : DevExpress.XtraEditors.XtraForm
	{
		public RgroupDecomposition()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Main tool entry point
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public string Run(
			string args)
		{
			QbUtil.SetMode(QueryMode.Build); // be sure in build mode

			if (ServicesIniFile.Read("RgroupDecompositionHelpUrl") != "")
				Help.Enabled = true;

			this.ShowDialog();
			return "";
		}

		private void RetrieveModel_Click(object sender, EventArgs e)
		{
			Query q = SessionManager.Instance.QueryBuilderQuery;
			QueryColumn qc = null;

			if (q != null && q.Tables.Count > 0)
				qc = q.Tables[0].KeyQueryColumn;

			Point p = RetrieveModel.PointToScreen(new System.Drawing.Point(0, RetrieveModel.Height));
			MoleculeSelectorControl.ShowModelSelectionMenu(p, QueryMolCtl, StructureSearchType.Substructure, qc);
		}

		private void RetrieveRecentButton_Click(object sender, EventArgs e)
		{
			MoleculeSelectorControl.SelectMruMolecule(QueryMolCtl);
			QueryMolCtl.Focus(); // move focus away
		}

		private void RetrieveFavoritesButton_Click(object sender, EventArgs e)
		{
			MoleculeSelectorControl.SelectFavoriteMolecule(QueryMolCtl);
			QueryMolCtl.Focus(); // move focus away
		}

		private void AddToFavoritesButton_Click(object sender, EventArgs e)
		{
			Point p = RetrieveModel.PointToScreen(new System.Drawing.Point(0, AddToFavoritesButton.Height));
			MoleculeSelectorControl.AddToFavoritesList(QueryMolCtl, StructureSearchType.Substructure);
			QueryMolCtl.Focus(); // move focus away
		}

		private void EditStructure_Click(object sender, EventArgs e)
		{
			DelayedCallback.Schedule(QueryMolCtl.EditMolecule);
		}

		private void OK_Click(object sender, EventArgs e)
		{
			DialogResult = ProcessInput();
		}

		private void Cancel_Click(object sender, EventArgs e)
		{
			return;
		}

		private void Help_Click(object sender, EventArgs e)
		{
			QbUtil.ShowConfigParameterDocument("RgroupDecompositionHelpUrl", "Rgroup Decomposition Help");
		}

/// <summary>
/// Build query table for decomposition & add to query
/// </summary>
/// <returns></returns>

		DialogResult ProcessInput()
		{
			Query q;
			MetaTable mt;
			MetaColumn mc;
			QueryTable qt;
			QueryColumn qc;

			q = QbUtil.Query;
			if (q == null || q.Tables.Count == 0)
			{
				MessageBoxMx.ShowError("No current query.");
				return DialogResult.None;
			}

			qt = q.GetQueryTableByName("Rgroup_Decomposition");
			bool newTable = false;
			if (qt == null)
			{
				mt = MetaTableCollection.GetWithException("Rgroup_Decomposition");
				qt = new QueryTable(mt);
				newTable = true;
			}

			qc = qt.GetQueryColumnByNameWithException("Core");

			MoleculeMx core = new MoleculeMx(MoleculeFormat.Molfile, QueryMolCtl.MolfileString);
			if (core.AtomCount == 0)
			{
				MessageBoxMx.ShowError("A Core structure with R-groups must be defined.");
				return DialogResult.None;
			}

			qc.MolString = core.GetMolfileString(); // put core structure into table criteria
			qc.CriteriaDisplay = "Substructure search (SSS)";
			qc.Criteria = "CORE SSS SQUERY";

			if (!Structure.Checked && !Smiles.Checked && !Formula.Checked &&
				!Weight.Checked && !Index.Checked)
			{
				MessageBoxMx.ShowError("At least one substituent display format must be selected.");
				return DialogResult.None;
			}

			qc = qt.GetQueryColumnByName("R1_Structure");
			if (ShowCoreStructure.Checked)
			{
				qc.Label = "R-group, Core\tChime=" + core.GetChimeString(); // reference core in query col header label
				qc.MetaColumn.Width = 25;
			}

			SetSelected(qt, "R1_Structure", Structure.Checked);
			SetSelected(qt, "R1_Smiles", Smiles.Checked);
			SetSelected(qt, "R1_Formula", Formula.Checked);
			SetSelected(qt, "R1_Weight", Weight.Checked);
			SetSelected(qt, "R1_SubstNo", Index.Checked);

			string terminateOption = TerminateOption.Text;
			qc = qt.GetQueryColumnByName("Terminate_Option");
			if (qc != null && Lex.IsDefined(terminateOption))
			{
				qc.Criteria = qt.MetaTable.Name + " = " + Lex.AddSingleQuotes(terminateOption);
				qc.CriteriaDisplay = "= " + Lex.AddSingleQuotes(terminateOption);
			}

			else qc.Criteria = qc.CriteriaDisplay = ""; // not defined 

			if (newTable) q.AddQueryTable(qt); // add to query if new
			QbUtil.RenderQuery(); // show it
			UsageDao.LogEvent("RgroupDecomposition");
			return DialogResult.OK;
		}

		/// <summary>
		/// Set querycolumn selection
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="colName"></param>
		/// <param name="selected"></param>

		public static void SetSelected(
			QueryTable qt,
			string colName,
			bool selected)
		{
			QueryColumn qc = qt.GetQueryColumnByName(colName);
			if (qc != null) qc.Selected = selected;
			return;
		}
	
	}
}