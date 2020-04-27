using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

	/// <summary>
	/// Control for previewing structure search results while defining search criteria
	/// </summary>

	public partial class CriteriaStructurePreview : UserControl
	{
		public Query SwQuery = null; // Query that defines SmallWorld search
		public QueryManager Qm = null; // QueryManger that contains all elements for query definition, execution and display
		public QueryEngine Qe = null; // QueryEngine used to run query and that can be called to pick alternate structure depictions
		public List<object[]> MatchingStructuresList = null; // list of data rows for matches

		public bool RenderResultsRequested = false; // if true re-render the results
		//public bool HideUndefinedStructureList = false;

		public string CidMtName = ""; // Name of metatable that contains the cid
		public string PendingStatusMessage = "";

		public System.Windows.Forms.Timer Timer;
		public bool InTimer_Tick = false;

		ParsedStructureCriteria LastPsc = null;
		string LastQuerySmiles = "";
		DateTime SearchStartTime, LastUpdateTime;

		public static int SearchCounter = 0; // Counter used to assign an Id to each search
		public static object SearchLock = new object();

		public SimpleDelegate RenderSearchResultsDelegate;
		public SimpleDelegate RenderSearchResultsCompleteCallBack;

		static bool Debug = false;

		public CriteriaStructurePreview()
		{

			if (SystemUtil.InDesignMode)
			{
				InitializeComponent();
				return;
			}

			MoleculeGridPageControl.RemoveToolPanelInConstructor = false; // keep tool panel and properly layout of controls
			InitializeComponent();
			MoleculeGridPageControl.RemoveToolPanelInConstructor = true;

			MoleculeGridPageControl mgpc = MoleculeGridPageControl;
			mgpc.EditQueryButton.Visible = false;
			//PreviewCtl.MarkDataButton.Visible = false;
			mgpc.FormattingButton.Visible = false;
			mgpc.AdjustToolPanelButtonSpacing();
			mgpc.Dock = DockStyle.Fill;

			Timer = new System.Windows.Forms.Timer();
			Timer.Tick += new System.EventHandler(Timer_Tick);

			return;
		}

		/// <summary>
		/// Start preview structure search
		/// </summary>
		/// <param name="str"></param>

		public void StartQueryExecution(
			ParsedStructureCriteria psc)
		{
			if (SwQuery == null) InitializeView();

			if (psc == null || psc.Molecule == null || psc.Molecule.AtomCount < 6)
			{
				ClearDataAndGrid();
				return;
			}

			if (Qm != null && Qm.ResultsFormatter != null)
				Qm.ResultsFormatter.SmallWorldDepictions = null; // reset dipictions since any previous depictions are invalid

			string smiles = psc.Molecule.GetSmilesString();
			if (!Lex.Eq(smiles, LastQuerySmiles)) ClearDataAndGrid(); // clear grid if different structure (must still start search since other options may have changed)

			LastPsc = psc;
			LastQuerySmiles = smiles;

			SearchStartTime = LastUpdateTime = DateTime.Now;
			SearchStatusLabel.Appearance.ForeColor = Color.DarkRed;
			SearchStatusLabel.Text = "Searching...";
			//SystemUtil.Beep(); // debug

			QueryColumn qc = SwQuery.GetFirstStructureCriteriaColumn();
			psc.ConvertToQueryColumnCriteria(qc);

			Timer.Enabled = true; // enable timer waiting for query execution to complete
			Thread t = new Thread(new ParameterizedThreadStart(ExecuteQueryThreadMethod));
			t.IsBackground = true;
			t.SetApartmentState(ApartmentState.STA);

			object parm = null;
			t.Start(parm);

			return;
		}

		/// <summary>
		/// Update depiction style 
		/// </summary>
		/// <param name="psc"></param>

		public void UpdateDepictions(ParsedStructureCriteria psc)
		{
			SmallWorldPredefinedParameters swp = psc.SmallWorldParameters; // new values

			if (Qm == null || Qm.ResultsFormatter == null ||
				Qm.ResultsFormatter.StructureHighlightPssc == null ||
				Qm.ResultsFormatter.StructureHighlightPssc.SmallWorldParameters == null) return;

			ResultsFormatter fmtr = Qm.ResultsFormatter;
			SmallWorldPredefinedParameters swp2 = // current view values
				fmtr.StructureHighlightPssc.SmallWorldParameters;

			psc.Highlight = swp2.Highlight = swp.Highlight;
			psc.Align = swp2.Align = swp.Align;

			int i = SmallWorldData.GetSvgOptionsIndex(swp2.Highlight, swp2.Align);
			SmallWorldDepictions d = fmtr.SmallWorldDepictions;

			if ((swp.Highlight || swp.Align) && d != null && i!=0 && d.SvgDict[i] == null) // need to start retrieval for depiction type we don't alread have?
			{
				d.StartDepictionRetrieval(Qm, psc.QueryColumn, swp2.Highlight, swp2.Align);
				return; // leave display as is for now
			}

			else // clear current decpiction so new one shows
			{
				Qm.DataTableManager.ResetFormattedValues(psc.QueryColumn); // clear existing bitmaps
				Qm.MoleculeGrid.RefreshDataSource();
			}

			return;
		}

		/// <summary>
		/// Execute query & get results
		/// </summary>
		/// <param name="parm"></param>

		void ExecuteQueryThreadMethod(object parm)
		{
			try
			{
				List<object[]> matches = null;
				QueryEngine qe = null;
				int searchId = 0; // id for search associated with this thread
				string hitString = "";

				lock (SearchLock)
				{
					SearchCounter++; // assign id for the search associated with this thread
					searchId = SearchCounter;
				}

				if (Debug) DebugLog.Message("=== Client === " + searchId + ". Starting query execution...");

				PendingStatusMessage = "Retrieving...";

				MoleculeUtil.ExecuteSmallWorldPreviewQuery(SwQuery, out qe, out matches);

				if (Superseded(searchId)) return; // return now if superceded and no longer needed

				Qe = qe;
				MatchingStructuresList = matches;

				if (Debug) DebugLog.Message("=== Client === " + searchId + ". Structure retrieval complete");

				PendingStatusMessage = "";
				RenderResultsRequested = true; // request render of associated structures
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

				DateTime now = DateTime.Now;
				TimeSpan ts = now.Subtract(LastUpdateTime);
				if (ts.TotalSeconds >= 1)
				{
					LastUpdateTime = now;
					ts = now.Subtract(SearchStartTime);
					SearchStatusLabel.Appearance.ForeColor = Color.DarkRed;
					SearchStatusLabel.Text = "Searching... (" + String.Format("{0:0.0}", ts.TotalSeconds) + " s.)";
				}


				if (PendingStatusMessage != null)
				{
					//if (Lex.IsDefined(PendingStatusMessage)) SystemUtil.Beep(); // debug

					//SearchStatusLabel.Text = PendingStatusMessage;
					PendingStatusMessage = null;
				}


				if (RenderResultsRequested)
				{
					RenderResultsRequested = false;
					//if (StructureRetrievalCompletedRenderRequest) // first render after structure retrieval is completed
					//{
					//	//SystemUtil.Beep(); // debug
					//	if (RenderSearchResultsCompleteCallBack != null) // notify someone we're done if requested
					//		RSC.RenderSearchResultsCompleteCallBack();
					//	StructureRetrievalCompletedRenderRequest = false;
					//}

					Timer.Enabled = false; // enable timer waiting for query execution to complete
					DisplayMatches();
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
/// Display the matches
/// </summary>

		public void DisplayMatches()
			//bool includeQueryStructure,
			//bool excludeCurrentHitList,
			//List<object[]> matchingStructuresList)
		{
			List<object[]> voaList = MatchingStructuresList;
			Qm.QueryEngine = Qe; // link-in query engine

			if (voaList == null)
			{
				ClearDataAndGrid();
				return;
			}

			SearchStatusLabel.Appearance.ForeColor = Color.Black;
			SearchStatusLabel.Text = "Matching Structures: " + voaList.Count;

			//string title = "Matching Structures: " + voaList.Count;
			//Qm.MoleculeGrid.SetTableHeaderCaption(0, title);

			if (!MoleculeGridPageControl.Visible) MoleculeGridPageControl.Visible = true;

			Qm.DataTableManager.FillDataTable(voaList);

			return;
		}

/// <summary>
/// Initialize the Preview Query and associated QueryManager and Grid View
/// </summary>

		public void InitializeView()
		{
			if (SwQuery != null) return;

			SwQuery = BuildStructureSearchQuery();
			DataTableMx dt = DataTableManager.BuildDataTable(SwQuery);
			Qm = ToolHelper.SetupQueryManager(SwQuery, dt);

			MoleculeGridPageControl mgpc = MoleculeGridPageControl;
			ToolHelper.SetupGridPanelForDisplay(Qm, mgpc.MoleculeGridPanel);

			MoleculeGridControl grid = mgpc.MoleculeGrid;
			if (grid != null && grid.GV != null)
				grid.GV.IndicatorWidth = 40; // narrow indicator col a bit

			ClearDataAndGrid();

			return;
		}

/// <summary>
/// ClearDataAndGrid
/// </summary>

		public void ClearDataAndGrid()
		{
			if (Qm == null || Qm.DataTable == null) return;

			DataTableMx dt = Qm.DataTable;
			dt.Clear();
			ToolHelper.RefreshDataDisplay(Qm);

			SearchStatusLabel.Appearance.ForeColor = Color.Black;
			SearchStatusLabel.Text = "Matching Structures: 0";

			//string title = "Matching Structures";
			//Qm.MoleculeGrid.SetTableHeaderCaption(0, title);
			return;
		}

		public Query BuildStructureSearchQuery()
		{
			MetaTable mt = MetaTableCollection.Get(MetaTable.SmallWorldMetaTableName);
			if (mt == null)
			{
				DebugLog.Message("SmallWorld MetaTable not found");
				MoleculeGridPageControl.Visible = false;
				return null;
			}

			Query q = new Query(); // build single-table query
			q.SingleStepExecution = true;

			QueryTable qt = new QueryTable(q, mt);
			string title = "SmallWorld";
			qt.Label = title;
			q.UserObject.Name = qt.MetaTable.Label; // need?

			return q;
		}

	}
}
