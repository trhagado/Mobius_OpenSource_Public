using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Manage Related Structure Control UIs
	/// Two basic forms are currently managed:
	/// 
	/// 1. RelatedStructureBrowser - All UI elements are on the same form
	/// 
	/// 2. QuickSearchPopup form - The popup containing the CID, its structure and match counts is 
	///    separate from the matching structure display grid.
	/// </summary>

	public class RelatedStructureControlManager
	{
		public bool IntegratedView = true; // All UI elements are on the same form
		public bool SplitView // UI elements split across two forms
		{
			get { return !IntegratedView; }
			set { IntegratedView = !value; }
		}

		public Query StructureDisplayQuery = null; // query that displays similar structure table
		public QueryManager Qm = null; // QueryManger that contains all elements for query definition, execution and display

		public bool RenderResultsRequested = false; // if true re-render the results
		public bool StructureRetrievalCompletedRenderRequest = false;
		public bool HideUndefinedStructureList = false;
		public RelatedStructureSearchResults RelatedStructuresResults = null; // current set of results

		public RelatedStructuresControl RSC; // panel containing match counts

		public string CidMtName = ""; // Name of metatable that contains the cid
		public string Cid = ""; // normalized cid that data is currently displayed for
		public MoleculeMx QueryStruct = null; // query structure
		StructureSearchType SearchTypes = StructureSearchType.Unknown; // ORed list of search types to perform
		public bool ExcludeCurrentHitList = false;
		public bool StructureChanged = false; // true if user modifies the structure directly

		public string PendingStatusMessage = null; // status message posted for display

		public System.Windows.Forms.Timer Timer;
		public bool InTimer_Tick = false;

		public bool InSetup
		{
			get { return RSC.InSetup; }
			set { RSC.InSetup = value; }
		}

		//  ========================================================================================================

		public static int SearchCounter = 0; // Counter used to assign an Id to each search
		public static object RSMLock = new object();

		static bool UseCachedResults => RSSConfig.UseCachedResults;
		static bool Debug => RSSConfig.Debug;

		/// <summary>
		/// Constructor
		/// </summary>
		public RelatedStructureControlManager()
		{
			Timer = new System.Windows.Forms.Timer();
			Timer.Tick += new System.EventHandler(Timer_Tick);
		}

/// <summary>
/// Clear search status
/// </summary>

		public void ClearSearchStatus()
		{
			foreach (CheckEdit ce in RSC.CheckmarkCtls) // Array of checkmark controls
					ce.Text = "";
	
			PendingStatusMessage = "";
			SetSearchStatusLabelText(PendingStatusMessage);
			return;
		}

		/// <summary>
		/// Get the list of related compounds
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="cid"></param>
		/// <param name="str"></param>

		public void StartSearchAndRetrievalOfRelatedStructures(
			string mtName,
			string cid,
			MoleculeMx str,
			StructureSearchType searchTypes)
		{
			RSC.RenderSearchResultsDelegate = RenderSearchResults; // set render callback

			DisplayEmptySearchResults();

			if (Lex.IsUndefined(str.PrimaryValue) || (!str.IsChemStructureFormat && str.IsBiopolymerFormat)) // reasonable structure?
			{
				return;
			}

			Cid = cid;
			CidMtName = mtName;
			QueryStruct = str;
			SearchTypes = searchTypes;

			Timer.Enabled = true;

			Thread t = new Thread(new ParameterizedThreadStart(SearchAndRetrieveRelatedStructuresThreadMethod));
			t.IsBackground = true;
			t.SetApartmentState(ApartmentState.STA);

			object[] parms = null;
			t.Start(parms);

			return;
		}

/// <summary>
/// Get related compound ids & update display
/// </summary>
/// <param name="parm"></param>

		void SearchAndRetrieveRelatedStructuresThreadMethod(object parm)
		{
			try
			{
				int searchId = 0; // id for search associated with this thread

				lock (RSMLock)
				{
					SearchCounter++; // assign id for the search associated with this thread
					searchId = SearchCounter;
				}

				RelatedStructureSearchResults rss = RelatedStructureSearchResults.SearchHistory(Cid, CidMtName);
				if (rss == null || Lex.IsUndefined(Cid) || Lex.IsUndefined(CidMtName) || !UseCachedResults)
				{
					PendingStatusMessage = "Searching...";

					rss = new RelatedStructureSearchResults();
					rss.Cid = Cid;
					rss.CidMtName = CidMtName;

					if (Debug) DebugLog.Message("=== Client === " + searchId + ". Starting search  for CID: " + Cid + " =========");

					string hitString = MoleculeUtil.GetRelatedMatchCounts(Cid, CidMtName, QueryStruct.GetChimeString(), SearchTypes, searchId);

					if (Superseded(searchId)) return; // return now if superceded and no longer needed

					if (Debug) DebugLog.Message("=== Client === " + searchId + ". Search step complete: " + hitString);
					if (Lex.IsUndefined(hitString))
					{
						if (Debug) DebugLog.Message("=== Client === " + searchId + ". Hit string is undefined.");
						return;
					}

					string[] sa = hitString.Split(';');
					if (sa.Length != 10) return;

					rss.AltCorpDbCnt = int.Parse(sa[0]);
					rss.SimCorpDbCnt = int.Parse(sa[1]);
					rss.MmpCorpDbCnt = int.Parse(sa[2]);
					rss.SwCorpDbCnt = int.Parse(sa[3]);
					rss.CorpDbCnt = int.Parse(sa[4]);

					rss.AltChemblCnt = int.Parse(sa[5]);
					rss.SimChemblCnt = int.Parse(sa[6]);
					rss.MmpChemblCnt = int.Parse(sa[7]);
					rss.SwChemblCnt = int.Parse(sa[8]);
					rss.ChemblCnt = int.Parse(sa[9]);

					RelatedStructureSearchResults.History.Add(rss);
				}

				RelatedStructuresResults = rss;
				RenderResultsRequested = true; // request render of initial results

				if (rss.CorpDbCnt <= 0 && rss.ChemblCnt <= 0) // if no hits then assign empty structure list and don't do retrieve
					rss.MatchList = new List<StructSearchMatch>();

				if (rss.MatchList == null) // retrieve structures if structure list doesn't exist
				{
					if (Debug) DebugLog.Message("=== Client === " + searchId + ". Starting structure retrieval...");

					PendingStatusMessage = "Retrieving...";

					rss.Cid = Cid;
					rss.CidMtName = CidMtName;

					List<StructSearchMatch> ssml = MoleculeUtil.GetRelatedMatchRows(Cid, CidMtName);
					if (Superseded(searchId)) return; // return now if superceded and no longer needed

					rss.MatchList = ssml;

					if (Debug) DebugLog.Message("=== Client === " + searchId + ". Structure retrieval complete");
				}

				else
				{
					if (Debug) DebugLog.Message("=== Client === " + searchId + ". Already have structures");
				}

				PendingStatusMessage = "";

				RenderResultsRequested = true; // request render of associated structures
				StructureRetrievalCompletedRenderRequest = true;

				if (Debug) DebugLog.Message("=== Client === " + searchId + ". Search and retrieval complete");
				return;
			}

			catch (Exception ex) // log any exception since any thrown exception for thread can't be caught
			{
				DebugLog.Message(ex);
			}
		}

		/// <summary>
		/// Check if specified search as been superseded
		/// </summary>
		/// <param name="searchId"></param>
		/// <returns></returns>

		bool Superseded(int searchId)
		{
			if (searchId < SearchCounter)
			{
				if (Debug) DebugLog.Message("=== Client === " + searchId + ". Superseded");
				return true; 
			}

			else return false;
		}

		/// <summary>
		/// Update the match counts panel and structure view
		/// </summary>
		/// <param name="excludeCurrentHitList"></param>
		/// <param name="rssrs"></param>

		public void DisplayRelatedCidCountsAndStructures(
			RelatedStructureSearchResults rssrs)
		{
			if (!RSC.Visible) return; // just return if the match count panel is not visible

			List<StructSearchMatch> fml = // get filtered results bases on current checkbox settings
				RSC.FilterMatchList(rssrs); 

			RSC.DisplayRelatedCidCounts(RelatedStructuresResults);

			DisplayRelatedStructures(fml);

			return;
		}

		/// <summary>
		/// Update the match counts panel and structure view based on a set of structures and filter values
		/// </summary>
		/// <param name="excludeCurrentHitList"></param>
		/// <param name="rssrs"></param>

		public void DisplayRelatedStructures(
			List<StructSearchMatch> fml)
		{
			MoleculeMx cs;

			if (fml == null || fml.Count == 0)
			{
				DisplayEmptyStructureGrid();
				return;
			}

			if (StructureDisplayQuery == null) BuildStructureDisplayQuery(); // initial setup

			Query q = StructureDisplayQuery;
			QueryTable qt = q.Tables[0];
			DataTableMx dt = Qm.DataTable;
			MoleculeGridControl grid = Qm.MoleculeGrid;
			grid.BeginUpdate();

			dt.Clear(); // filter table

			HashSet<string> cidSet = new HashSet<string>();

			for (int mi = 0; mi < fml.Count; mi++) // build and add the rows to the datatable of structures
			{
				StructSearchMatch ssm = fml[mi];
				DataRowMx dr = dt.NewRow();
				dr[qt.Alias + ".CompoundId"] = ssm.SrcCid;

				cs = new MoleculeMx(ssm.MolStringFormat, ssm.MolString);
        if (Lex.IsDefined(ssm.SrcCid)) cs.SetMolComments("CorpId=" + ssm.SrcCid); // Attach CorpId to Molfile so it will be rendered correctly

				if (ssm.SearchType == StructureSearchType.SmallWorld && Lex.IsDefined(ssm.GraphicsString))
					cs.SvgString = ssm.GraphicsString;

 				dr[qt.Alias + ".Structure"] = cs;
				dr[qt.Alias + ".MatchType"] = ssm.SearchTypeName;
				dr[qt.Alias + ".MatchScore"] = ssm.MatchScore;
				dr[qt.Alias + ".Database"] = ssm.SrcName;
				dt.Rows.Add(dr);

				cidSet.Add(ssm.SrcCid);
			}

			Qm.DataTableManager.InitializeRowAttributes();

			string title = "Related Structures - Matches: " + fml.Count + ", Compound Ids: " + cidSet.Count;
			Qm.MoleculeGrid.SetTableHeaderCaption(0, title);

			if (!RSC.MoleculeGridPageControl.Visible) RSC.MoleculeGridPageControl.Visible = true;

			grid.EndUpdate();
			ToolHelper.RefreshDataDisplay(Qm);

			return;
		}

		/// <summary>
		/// DisplayEmptyStructureGrid
		/// </summary>

		public void DisplayEmptyStructureGrid()
		{
			if (StructureDisplayQuery == null) // first time
			{
				if (HideUndefinedStructureList)
				{
					RSC.MoleculeGridPageControl.Visible = false;
					return;
				}

				BuildStructureDisplayQuery();
			}

			ClearGrid();
		}

		/// <summary>
		/// Build the Query, QueryManager and DataTable for the matching structure display
		/// </summary>

		public void BuildStructureDisplayQuery()
		{
			MetaTable mt = MetaTableCollection.Get("QuickSearchRelatedStructures");
			if (mt == null)
			{
				DebugLog.Message("QuickSearchRelatedStructures MetaTable not found");
				RSC.MoleculeGridPageControl.Visible = false;
				return;
			}

			Query q = ToolHelper.InitEmbeddedDataToolQuery(mt);
			QueryTable qt = q.Tables[0];
			q.UserObject.Name = qt.MetaTable.Label;
			StructureDisplayQuery = q;

			QueryColumn qc = qt.FirstStructureQueryColumn;
			if (qc != null) // setup dummy criteria
			{
				ParsedStructureCriteria pssc = new ParsedStructureCriteria();
				pssc.SearchType = StructureSearchType.FullStructure; // just full structure search for now
				pssc.Molecule = new MoleculeMx(MoleculeFormat.Smiles, "C"); // placeholder structure
				pssc.ConvertToQueryColumnCriteria(qc);

				qc.DisplayFormatString = "Highlight=true;Align=true";
			}

			string title = "Related Structures";
			qt.Label = title;

			DataTableMx dt = q.ResultsDataTable as DataTableMx;

			Qm = ToolHelper.SetupQueryManager(q, dt);

			MoleculeGridPageControl mgpc = RSC.MoleculeGridPageControl;
			MoleculeGridPanel gp = mgpc.MoleculeGridPanel;

			mgpc.Width = RSC.Width - 3; // be sure we have page control correctly sized
			mgpc.Height = RSC.Height - mgpc.Top - 3;

			ToolHelper.SetupGridPanelForDisplay(Qm, mgpc.MoleculeGridPanel, true, true);

			MoleculeGridControl grid = mgpc.MoleculeGrid;
			if (grid != null && grid.GV != null)
				grid.GV.IndicatorWidth = 40; // narrow indicator a bit

			//ToolHelper.DisplayData // build and display empty grid for query with columns scaled to fit the grid
			//		(q, dt, RSC.MoleculeGridPageControl.MoleculeGridPanel, true, true);

			return;
		}

		public void ClearGrid()
		{
			DataTableMx dt = Qm.DataTable;
			dt.Clear();
			ToolHelper.RefreshDataDisplay(Qm);

			string title = "Related Structures";
			Qm.MoleculeGrid.SetTableHeaderCaption(0, title);

			return;
		}

		/// <summary>
		/// Handle UI operations during tick of timer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Timer_Tick(object sender, EventArgs e)
		{
			//if (ServiceFacade.ServiceFacade.InServiceCall) return; // avoid multiplexing service calls that could cause problems
			if (InTimer_Tick) return;

			try
			{
				InTimer_Tick = true;

				if (PendingStatusMessage != null)
				{
					//if (Lex.IsDefined(PendingStatusMessage)) SystemUtil.Beep(); // debug

					SetSearchStatusLabelText(PendingStatusMessage);
					PendingStatusMessage = null;
				}


				if (RenderResultsRequested)
				{
					RenderResultsRequested = false;
					if (StructureRetrievalCompletedRenderRequest) // first render after structure retrieval is completed
					{
						//SystemUtil.Beep(); // debug
						if (RSC.RenderSearchResultsCompleteCallBack != null) // notify someone we're done if requested
							RSC.RenderSearchResultsCompleteCallBack();
						StructureRetrievalCompletedRenderRequest = false;
					}

					RenderSearchResults();
					return;
				}
			}

			catch (Exception ex) { ex = ex; }

			finally
			{

				InTimer_Tick = false;
			}

		}

		/// <summary>
		/// Render the search results, either just counts or counts and structures
		/// </summary>

		public void RenderSearchResults()
		{
			if (RelatedStructuresResults == null) // no results
			{
				DisplayEmptySearchResults();
			}

			else if (RelatedStructuresResults.MatchList == null) // cid counts only
			{
				RSC.DisplayRelatedCidCounts(RelatedStructuresResults);
			}

			else // display structures 
			{
				DisplayRelatedCidCountsAndStructures(RelatedStructuresResults);
			}

			return;
		}

		void SetSearchStatusLabelText(string txt)
		{
			RSC.SearchStatusLabel.Text = txt;
			RSC.SearchStatusLabel.Visible = Lex.IsDefined(RSC.SearchStatusLabel.Text); // hide text ctl if no text to avoid hiding part of structure
		}

		/// <summary>
		/// DisplayEmptySearchResults
		/// </summary>
		void DisplayEmptySearchResults()
		{
			try
			{
				RSC.DisplayEmptyCidCountsGrid();
				DisplayEmptyStructureGrid();
			}

			catch (Exception ex)
			{
				string msg = ex.Message;  // ignore any error (e.g. in ClearGrid)
			}

			PendingStatusMessage = "";
			return;
		}

	}
}
