using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.Data;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace Mobius.Tools
{
	public partial class SarLandscape : DevExpress.XtraEditors.XtraForm
	{
		internal QueryColumn SasQc = null; // column in sasmap table that contains parameters for analysis
		internal QueryTable SasQt => SasQc?.QueryTable; // SasMap QueryTable we are working on
		internal Query SasQuery => SasQc?.Query;

		internal SasMapParms Parms = new SasMapParms(); // the parameters for the current SasMap

		internal Query BaseQuery = null; // reference query that we will use for source of possible endpoint cols to analyze

		internal static int AnalysisCount = 0;
		internal static SasMapParms LastParms = new SasMapParms();

		public SarLandscape()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Tools menu item click
		/// Create a standalone SasMap analysis based on the current query and then run it
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public string Run(
			string args)
		{
			QueryTable qt;
			MetaTable mt;
			QueryColumn qc;

			int qid = SS.I.ServicesIniFile.ReadInt("SasMapToolModelQuery", 888070);
			if (qid < 0) throw new Exception("SasMapToolModelQuery not defined");

			Query q = QbUtil.ReadQuery(qid);
			if (q == null) throw new Exception("Failed to read SasMapToolModelQuery: " + qid);

			q.UserObject = new UserObject(UserObjectType.Query); // treat this as a new query

			QueryTable sasQt = q.GetQueryTableByNameWithException(SasMapParms.MetaTableName);
			SasQc = sasQt.GetQueryColumnByNameWithException(SasMapParms.ParametersMetaColumnName);

			BaseQuery = GetBaseQuery();
			if (BaseQuery == null || BaseQuery.Tables.Count == 0)
			{
				MessageBoxMx.ShowError("The current query must contain one or more data tables before a SasMap can be defined on the query");
				return "";
			}

			Parms = LastParms;

			qc = BaseQuery?.GetFirstKeyColumn();
			qc = qc.Clone();
			qc.CopyCriteriaFromQueryKeyCriteria(BaseQuery);
			Parms.KeyCriteriaQc = qc; 

			Parms.EndpointMc = BaseQuery?.GetFirstResultColumnByPriority(true)?.MetaColumn; // pick the highest priority result column as the default endpoint

			QbUtil.SetMode(QueryMode.Build); // be sure in build mode

			DialogResult dr = Edit();
			if (dr != DialogResult.OK) return "";

			StoreParmsInQueryColumnCriteria(Parms, SasQc);

			// Add as a new query and run it

			AnalysisCount++;

			q.Name = "Structure-Activity Similarity Analysis " + AnalysisCount;
			q.SingleStepExecution = true;

			// Add the query to the QueryBuilder and run it

			DelayedCallback.Schedule( // add and run the query after exiting this dialog
				delegate (object state)
				{
					QbUtil.AddQuery(q);
					QbUtil.SetCurrentQueryInstance(q); // set to new query
					QbUtil.RenderQuery();
					QueryExec.RunQuery(q, SS.I.DefaultQueryDest);
				});

			Progress.Hide();

			return "";
		}

		/// <summary>
		/// Adding a new SasMap view to an existing query
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public DialogResult ShowInitialViewSetupDialog(
			QueryColumn sasQc,
			Query baseQuery)
		{
			MetaTable mt;
			QueryColumn qc = null;
			DialogResult dr;

			AssertMx.IsNotNull(sasQc, "SasQc");
			AssertMx.IsNotNull(SasQuery, "SasMapQuery");

			SasQc = sasQc;
			BaseQuery = baseQuery;

			Parms = GetParmsFromQueryColumnCriteria(SasQc);

			qc = BaseQuery?.GetFirstKeyColumn();
			if (qc == null)
				qc = QueryTable.GetDefaultRootQueryTable()?.KeyQueryColumn;
			qc = qc.Clone();

			Parms.KeyCriteriaQc = qc; // assign QC with undefined criteria

			QueryManager refQm = (baseQuery.QueryManager as QueryManager);
			List<string> resultsKeys = refQm?.DataTableManager?.ResultsKeys;
			if (resultsKeys != null && resultsKeys.Count > 0)
			{
				string listText = Csv.JoinCsvString(resultsKeys);
				qc.CriteriaDisplay = CidList.FormatAbbreviatedCidListForDisplay(qc, listText);
				qc.Criteria = qc.MetaColumn.Name + " IN (" + listText + ")";
			}

			Parms.EndpointMc = BaseQuery?.GetFirstResultColumnByPriority(true)?.MetaColumn; // pick the highest priority result column as the default endpoint

			dr = Edit();
			if (dr == DialogResult.OK)
				StoreParmsInQueryColumnCriteria(Parms, SasQc);

			return dr;
		}

		/// <summary>
		/// Edit parameters for existing SasMap query/view
		/// </summary>
		/// <param name="sasQc"></param>
		/// <returns></returns>

		public DialogResult EditCriteria(QueryColumn sasQc)
		{
			SasQc = sasQc;
			BaseQuery = GetBaseQuery();
			Parms = GetParmsFromQueryColumnCriteria(sasQc);

			DialogResult dr = Edit();
			if (dr == DialogResult.OK)
				StoreParmsInQueryColumnCriteria(Parms, SasQc);

			return dr;
		}

		/// <summary>
		/// Get base query
		/// </summary>
		/// <returns></returns>

		Query GetBaseQuery()
		{
			Query bq = SessionManager.Instance.QueryBuilderQuery;
			if (bq == null) bq = QueriesControl.Instance.CurrentQuery;
			if (bq == null)
			{
				bq = new Query();
				bq.Tables.Add(QueryTable.GetDefaultRootQueryTable());
			}

			if (bq != null) return bq;
			else throw new Exception("Unable to get base query");
		}

	/// <summary>
	/// Edit the parameters (Private method)
	/// </summary>
	/// <param name="parms"></param>
	/// <returns></returns>

	private DialogResult Edit()
		{
			SetupForm();

			DialogResult dr = ShowDialog(SessionManager.ActiveForm);
			return dr;
		}

		/// <summary>
		/// SetupForm
		/// </summary>

		void SetupForm()
		{

// CorpIds

			if (Parms.KeyCriteriaQc != null)
				KeyCriteriaCtl.Setup(Parms.KeyCriteriaQc);

			else
				KeyCriteriaCtl.Setup(BaseQuery);

			// Endpoint

			EndPointCtl.Setup(BaseQuery, new QueryColumn(Parms.EndpointMc));

// Activity diff calc type

			ActDiffCalcType calcType = Parms.ActDiffCalcType;

			if (calcType == ActDiffCalcType.SimpleDifference)
				SimpleDifference.Checked = true;

			else if (calcType == ActDiffCalcType.NegativeLog)
				NegativeLog.Checked = true;

			else if (calcType == ActDiffCalcType.MolarNegativeLog)
				MolarNegativeLog.Checked = true;

			else if (calcType == ActDiffCalcType.Ratio)
				Ratio.Checked = true;

			 CalcAbsoluteValue.Checked = Parms.UseAbsoluteValue;

			// Molsim parameters

			if (Parms.SimilarityType == SimilaritySearchType.ECFP4)
				ECFP4Similarity.Checked = true;


			else if (Parms.SimilarityType == SimilaritySearchType.Normal)
				StandardSimilarity.Checked = true;


			MinSimCtl.Text = Parms.MinimumSimilarity.ToString();

			PairCountCtl.Text = Parms.MaxPairCount.ToString();

			return;
		}

		static void StoreParmsInQueryColumnCriteria(
			SasMapParms smp,
			QueryColumn qc)
		{
			string parms = smp.Serialize();
			qc.Criteria = qc.MetaColumnName + " = " + Lex.AddDoubleQuotesIfNeeded(parms);
			qc.CriteriaDisplay = "Edit...";
		}

		static SasMapParms GetParmsFromQueryColumnCriteria(
			QueryColumn qc)
		{
			if (Lex.IsUndefined(qc.Criteria)) return new SasMapParms();

			ParsedSingleCriteria psc = ParsedSingleCriteria.Parse(qc);
			if (Lex.IsUndefined(psc.Value)) return new SasMapParms();

			SasMapParms smp = SasMapParms.Deserialize(psc.Value);
			if (smp != null) return smp;

			else return new SasMapParms();
		}

		private void OK_Click(object sender, EventArgs e)
		{
			if (ProcessInput())
				DialogResult = DialogResult.OK;
		}

		private void Cancel_Click(object sender, EventArgs e)
		{
			return;
		}

		private void Help_Click(object sender, EventArgs e)
		{
			QbUtil.ShowConfigParameterDocument("SarLandscapeHelpUrl", "Sar Landscape Help");
		}

		/// <summary>
		/// OK button clicked, process input
		/// </summary>
		/// <returns></returns>

		bool ProcessInput()
		{
			MetaTable mt;
			Query q;
			QueryColumn qc;
			string actDifLabel = "", actSasCoefLabel = "";
			string tok;

			if (Lex.IsDefined(KeyCriteriaCtl.QueryColumn.Criteria))
				Parms.KeyCriteriaQc = KeyCriteriaCtl.QueryColumn;

			else
			{
				MessageBoxMx.ShowError("Compound Id criteria must be defined.");
				return false;
			}

			if (EndPointCtl.MetaColumn == null)
			{
				MessageBoxMx.ShowError("An endpoint assay result must be selected.");
				return false;
			}

			Parms.EndpointMc = EndPointCtl.MetaColumn;
			string endPointLabel = EndPointCtl.MetaColumn.Label;

			qc = SasQt.GetQueryColumnByName("ACTIVITY1");
			if (qc != null) qc.Label = endPointLabel + " 1";

			qc = SasQt.GetQueryColumnByName("ACTIVITY2");
			if (qc != null) qc.Label = endPointLabel + " 2";

			actSasCoefLabel = "Act. Diff. / (1-Sim.)";

			// Get the type of calculation

			ActDiffCalcType calcType = ActDiffCalcType.Undefined;

			if (SimpleDifference.Checked)
			{
				calcType = ActDiffCalcType.SimpleDifference;
				actDifLabel = "Activity Difference";
			}

			else if (NegativeLog.Checked)
			{
				actDifLabel = "Negative Log Difference";
				calcType = ActDiffCalcType.NegativeLog;
			}

			else if (MolarNegativeLog.Checked)
			{
				calcType = ActDiffCalcType.MolarNegativeLog;

				if (Lex.Contains(endPointLabel, "IC50"))
					actDifLabel = "pIC50 Difference (Molar)";

				else if (Lex.Contains(endPointLabel, "EC50"))
					actDifLabel = "pEC50 Difference (Molar)";

				else actDifLabel = "Negative Log Difference (Molar)";
			}

			else if (Ratio.Checked)
			{
				calcType = ActDiffCalcType.Ratio;
				actDifLabel = "Activity Ratio";
				actSasCoefLabel = "Act. Ratio / (1-Sim.)";
			}
						
			qc = SasQt.GetQueryColumnByName("ACTIVITY_DIFF");
			if (qc != null) qc.Label = actDifLabel;

			qc = SasQt.GetQueryColumnByName("ACT_SIM_COEF");
			if (qc != null) qc.Label = actSasCoefLabel;

			Parms.UseAbsoluteValue = CalcAbsoluteValue.Checked;

			Parms.ActDiffCalcType = calcType;

			// Get molsim parameters

			QueryColumn simQc = SasQt.GetQueryColumnByName("SIMILARITY");

			if (ECFP4Similarity.Checked)
			{
				Parms.SimilarityType = SimilaritySearchType.ECFP4;
				if (simQc != null)
					simQc.Label = "Structural Similarity (ECFP4)";
			}

			else if (StandardSimilarity.Checked)
			{
				Parms.SimilarityType = SimilaritySearchType.Normal;
				if (simQc != null)
					simQc.Label = "Structural Similarity";
			}

			tok = MinSimCtl.Text;
			Parms.MinimumSimilarity = -1;
			try { Parms.MinimumSimilarity = Double.Parse(tok); }
			catch (Exception ex) { }
			if (Parms.MinimumSimilarity <= 0 || Parms.MinimumSimilarity > 1)
			{
				MessageBoxMx.ShowError(
				"Minimum acceptable structural similarity\n" +
				"must be between 0 and 1.0");
				MinSimCtl.Focus();
				return false;
			}

			// Get pair count

			tok = PairCountCtl.Text;
			Parms.MaxPairCount = -1;
			try { Parms.MaxPairCount = Int32.Parse(tok); }
			catch (Exception ex) { }
			if (Parms.MaxPairCount <= 0.0)
			{
				MessageBoxMx.ShowError(
				"Pair count must be a positive integer value.");
				PairCountCtl.Focus();
				return false;
			}

			return true;
		}


		private void Prompt_HyperlinkClick(object sender, DevExpress.Utils.HyperlinkClickEventArgs e)
		{
			UIMisc.ShowHtmlPopupFormDocument(e.Link, "SAS Maps");
			return;
		}

	}
}