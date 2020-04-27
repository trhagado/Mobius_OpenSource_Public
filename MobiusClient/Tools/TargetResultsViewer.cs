using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.Data;

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.Tools
{
	/// <summary>
	/// Summarized target results viewer (Spill the Beans)
	/// </summary>

	public partial class TargetResultsViewer : XtraForm, IActionDelegate
	{
		static TargetResultsViewer Instance; // single instance for display

		QueryColumn KeyQc = null; // key criteria query column
		//TargetSummaryOptions Tso; // current target summary options, either default or from existing query
		Query Query; // the query that is built & executed
		bool QueryRendered = false; // if true query has been rendered in QueryBuilder
		int QueryCount; // number of queries executed
		bool Cancelled = false; // set to true if processing cancelled

/// <summary>
/// Main entrypoint for plugin
/// </summary>
/// <param name="args"></param>
/// <returns></returns>

		public string Run(
			string args)
		{
			DialogResult dr;
			try
			{
				Lex lex = new Lex();
				if (args == null) args = "";
				lex.OpenString(args);
				string cmd = lex.Get();
				string args2 = lex.GetRestOfLine().Trim();

				if (cmd == "" || Lex.Eq(cmd, "ShowDialog"))
				{
					QbUtil.SetMode(QueryMode.Build); // be sure in build mode
					dr = Instance.ShowDialog(args2);
					if (dr == DialogResult.OK)
					{
						cmd = "Command RunQuery"; // pass command back to main level
						return cmd;
					}
					else return "";
				}

				else if (Lex.Eq(cmd, "ShowPopup"))
				{
					if (Lex.IsUndefined(args2)) throw new Exception("Syntax: TargetResultsViewer ShowPopup <CorpId>");
					ShowPopup(args2);
					return "";
				}

				else throw new Exception("Invalid command: " + cmd);
			}

			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
		}

/// <summary>
/// Constructor
/// </summary>

		public TargetResultsViewer()
		{
			InitializeComponent();
			InitializeForm();
		}

		/// <summary>
		/// Setup the form
		/// </summary>

		void InitializeForm()
		{
			if (Instance != null) return;

			Instance = this;

			//if (UalUtil.IniFile.Read("TargetResultsViewerHelpUrl") != "")
			//  Help.Enabled = true;

			return;
		}

/// <summary>
/// Show dialog 
/// </summary>
/// <returns></returns>

		DialogResult ShowDialog (
			string args)
		{
			if (Lex.Contains(args, "EditCriteria")) // editing existing query
			{
				Query = QueriesControl.Instance.CurrentQuery;
				QueryTable qt = GetUnpivotedSummaryTable(Query);
				if (qt != null)
					KeyQc = qt.KeyQueryColumn;

				//Tso = TargetSummaryOptions.GetFromQueryTable(qt);
				QueryRendered = true; // should already be rendered
			}

			else // new tool query
			{
				Query = null; // no query defined yet
				KeyQc = null;
				QueryRendered = false; // not yet rendered
			}


			KeyCriteriaCtl.Setup(KeyQc);

			//bool allowDbChange = (Query == null);
			//KeyCriteriaCtl.SetFormValues(Tso, allowDbChange);

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);

			return dr;
		}

/// <summary>
/// Get the unpivoted summary table from the query
/// </summary>
/// <param name="q"></param>
/// <returns></returns>

		QueryTable GetUnpivotedSummaryTable (
			Query q)
		{
			QueryTable qt = q.GetQueryTableByName(MultiDbAssayDataNames.BaseTableName);
			if (qt == null)
				qt = q.GetQueryTableByName(MultiDbAssayDataNames.CombinedTableName);

			return qt;
		}

/// <summary>
/// Build the query that is executed to retrieve the summarized target data and optional structure data
/// </summary>
/// <returns></returns>

		void BuildQuery(string title)
		{
			int qid = SS.I.ServicesIniFile.ReadInt("SpillTheBeansToolModelQuery", -1);
			if (qid < 0) throw new Exception("SpillTheBeansToolModelQuery not defined");

			Query q = QbUtil.ReadQuery(qid);
			if (q == null) throw new Exception("Failed to read SpillTheBeansToolModelQuery: " + qid);

			AssertMx.IsDefined(KeyQc?.Criteria, "KeyQc.Criteria");

			ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(KeyQc?.Criteria); // parse key criteria

			KeyQc.CopyCriteriaToQueryKeyCritera(q);
			Query = q; // save for later use

			return;
		}


		/// <summary>
		/// Run the data query & return the data via a query manager
		/// </summary>
		/// <param name="dataQuery"></param>
		/// <returns>The displayQuery which may be the original dataQuery or a modified version of it</returns>

		Query RunQuery(
			Query dataQuery)
		{
			DataRowMx dr;
			int rowCount = 0;

			List<string> keyList;
			QueryManager qm = new QueryManager();
			Query displayQuery = dataQuery;
			qm.ExecuteQuery(ref displayQuery, out keyList);
			DataTableManager dtm = qm.DataTableManager;

			while (true)
			{
				dr = dtm.FetchNextDataRow(false);
				if (dr == null) break;
				if (dtm.RowRetrievalCancelled)
				{
					Cancelled = true;
					return null;
				}
				rowCount++;
			}

			return displayQuery;
		}

/// <summary>
/// TargetResultsViewer_Activated
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void TargetResultsViewer_Activated(object sender, EventArgs e)
		{
			KeyCriteriaCtl.Cids.Focus(); // focus on compound ids

			//if (OptionsCtl.Cids.Text == "")
			//  OptionsCtl.Cids.Focus(); // focus on compound id if not defined
			//else OptionsCtl.Targets.Focus();
		}

		private void Help_Click(object sender, EventArgs e)
		{
			QbUtil.ShowConfigParameterDocument("TargetResultsViewerHelpUrl", "All Bioassay Data by Target (Spill the Beans) Help");
		}

/// <summary>
/// OK clicked - Update the query in the query builder and run if a new query
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void OK_Click(object sender, EventArgs e)
		{
			string title = "";

			KeyQc = KeyCriteriaCtl.QueryColumn;


			if (Lex.IsUndefined(KeyQc?.Criteria))
			{
				MessageBoxMx.Show("You must supply one or more compound ids.", UmlautMobius.String);
				KeyCriteriaCtl.Cids.Focus();
				return;
			}

			//TargetSummaryOptions tso = Tso; // copy to force update of Query fields
			//if (!KeyCriteriaCtl.GetFormValues(tso)) return;
			//Tso = tso;
			//TargetSummaryOptionsControl.SavePreferences(Tso);

			BuildQuery("");

			if (!QueryRendered)
			{
				QueryCount++;
				Query.UserObject.Name = "Summarized Target Results Analysis " + QueryCount;
				QbUtil.AddQueryAndRender(Query, false); // add query if necessary
				QueryRendered = true;
			}

			else
			{
				//QbUtil.NewQuery(Query.UserObject.Name); // show in query builder
				//QbUtil.SetCurrentQueryInstance(Query);
				QueryTable qt = GetUnpivotedSummaryTable(Query);
				QbUtil.RenderQuery(qt);
			}

			DialogResult = DialogResult.OK;
			return;
		}

/// <summary>
/// ShowImmediate
/// </summary>
/// <param name="singleCorpId"></param>

		void ShowPopup(string singleCorpId)
		{
			KeyQc = QueryTable.GetDefaultRootQueryTable()?.KeyQueryColumn;
			KeyQc.Criteria = KeyQc.MetaColumnName + " = " + singleCorpId;

			//Tso = new TargetSummaryOptions();
			//Tso.CidCriteria = "= " + singleCorpId;

			string title = "Spotfire View of Data for Compound Id " + CompoundId.Format(singleCorpId);
			BuildQuery(title);
			QbUtil.RunPopupQuery(Query, title, OutputDest.WinForms);
			return;
		}

/// <summary>
/// Cancelled
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Cancel_Click(object sender, EventArgs e)
		{
			return;
		}

	}
}