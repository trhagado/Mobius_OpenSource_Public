using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;
using Mobius.CdkMx;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

	/// <summary>
	/// Dialog for defining related-compound information to retrieve
	/// Note: The 16x16 icon for this form was created from an initial bitmap that was then
	/// converted to a .png file and then an .ico file using http://www.xiconeditor.com/
	/// </summary>

	public partial class RelatedCompoundsDialog : DevExpress.XtraEditors.XtraForm
	{
		QueryManager Qm; // QueryManager for query we were called from
		Query SourceQuery = null; // the query that this request was initiated from
		Query RelatedCompoundsQuery = null; // query to run to get related compounds
		MetaTable RootTable = null; // root table for Q0 or supplied CID
		String SelectedCid = "";
		List<string> MarkedCidsList, CurrentBaseQueryCidHitList; // Marked and all cids in BaseQuery results
		MainMenuControl MainMenu { get { return SessionManager.Instance.MainMenuControl; } }
		CheckEdit[] CheckmarkCtls; // Array of checkmark controls
		HashSet<string> SelectedNodes = new HashSet<string>(); // set of most-recently selected nodes

		string PreviousCid = ""; // most recently seen number
		bool InTimer_Tick = false;

		static int QueryCount = 0; // number of queries built and run

		/// <summary>
		/// Basic constructor
		/// </summary>

		public RelatedCompoundsDialog()
		{
			InitializeComponent();

			if (SystemUtil.InDesignMode) return;

			CheckmarkCtls = new CheckEdit[] {
					CurrentCidCheckEdit, RelatedStrsCheckEdit, MarkedCidsCheckEdit, AllCidsCheckEdit,
					AltForms, MatchedPairs, SmallWorld, SimilarSearch, Substructure, AlignMatches,
					ExcludeCurrentResultsCids, IncludeStructures, ApplyExistingCriteria};

			return;
		}

		/// <summary>
		/// Show the form 
		/// </summary>
		/// <param name="qm"></param>
		/// <param name="cid"></param>

		public static void Show(
			string cid,
			QueryManager qm = null)
		{
			RelatedCompoundsDialog i = new RelatedCompoundsDialog();
			i.MruListCtl.ImageList = Bitmaps.Bitmaps16x16;

			if (!i.SetupForm(cid, qm)) return; // just return if some error

			i.Show();

			return;
		}

		/// <summary>
		/// Setup the form for display
		/// </summary>
		/// <param name="cid"></param>
		/// <param name="qm"></param>
		/// <returns></returns>

		bool SetupForm(
			string cid,
			QueryManager qm)
		{
			AssertMx.IsDefined(cid, "Compound Id (CorpId)");

			SelectedCid = cid;
			PreviousCid = cid;

			RootTable = CompoundId.GetRootMetaTableFromCid(cid);

			Qm = null; // no query context
			SourceQuery = null;
			CurrentBaseQueryCidHitList = null;

			if (qm != null) // if querymanager defined then base the related data query on the "current" query
			{
				Qm = qm; // setup query context
				SourceQuery = qm.Query;
				RootTable = qm.Query.RootMetaTable;

				if (qm.Query != null && qm.Query.Mode == QueryMode.Browse) // if browsing current base query results then get that cid list
					CurrentBaseQueryCidHitList = Qm.DataTableManager.GetMostCompleteResultsKeyList();
			}

			//else throw new Exception("Parameters not defined");

			//if (Lex.IsUndefined(SelectedCid) &&
			// (Qm == null || Qm.MoleculeGrid == null || Qm.MoleculeGrid.Helpers == null))
			//	return false;

			SetupCheckmarks();

			// Current Cid

			//if (Qm != null)
			//	SelectedCid = Qm.MoleculeGrid.Helpers.GetCidForSelectedCell();

			SelectedCid = CompoundId.Format(SelectedCid);
			CidCtl.Text = SelectedCid;
			//CidCtl.Focus();

			// Marked cid count

			MarkedCidsList = null;
			if (Qm?.MoleculeGrid != null)
			{
				CidList cl = Qm.MoleculeGrid.GetMarkedList();
				if (cl != null) MarkedCidsList = cl.ToStringList();
			}

			int selCnt = (MarkedCidsList != null ? MarkedCidsList.Count : 0);
			MarkedCidsCheckEdit.Text = "Selected compound Ids (" + FormatCidListForDisplay(MarkedCidsList) + ")";
			MarkedCidsCheckEdit.Enabled = (selCnt > 0);
			if (selCnt == 0 && MarkedCidsCheckEdit.Checked)
				CurrentCidCheckEdit.Checked = true;

			// All Cid count


			int allCnt = (CurrentBaseQueryCidHitList != null ? CurrentBaseQueryCidHitList.Count : 0);
			AllCidsCheckEdit.Text = "All Ids in the current result set (" + FormatCidListForDisplay(CurrentBaseQueryCidHitList) + ")";
			AllCidsCheckEdit.Enabled = (allCnt > 0);
			if (selCnt == 0 && AllCidsCheckEdit.Checked)
				CurrentCidCheckEdit.Checked = true;

			// Structure

			MoleculeMx cs = new MoleculeMx();

			if (Lex.IsDefined(SelectedCid))
			{
				cs = MoleculeUtil.SelectMoleculeForCid(SelectedCid);
			}

			QueryMolCtl.SetupAndRenderMolecule(cs);

			// MRU list

			RenderMruList();

			return true;
		}

		string FormatCidListForDisplay(List<string> cidList)
		{
			string txt;
			QueryColumn qc = null;

			int count = 0;

			if (cidList != null && cidList.Count > 0)
			count = cidList.Count;
			if (count > 0 && SourceQuery.Tables.Count > 0 && SourceQuery.Tables[0].KeyQueryColumn.MetaColumn.DataType == MetaColumnType.CompoundId)
			{
				qc = SourceQuery.Tables[0].KeyQueryColumn;
				string cidListString = MqlUtil.FormatValueListString(cidList, false);
				txt = CidList.FormatAbbreviatedCidListForDisplay(qc, cidListString);
				return txt;
			}

			else return count.ToString();
		}

		void UpdateAndRenderMruList(MetaTreeNode node)
		{
			if (node == null) return;

			MainMenu.UpdateMruList(node.Target, false);
			UpdateSelectedNodesSet();
			SelectedNodes.Add(node.Target);
			RenderMruList();
		}

		void RenderMruList()
		{
			if (MainMenu.MruList == null) return;

			List<MetaTreeNode> nodes = BuildMetaTreeNodeList(MainMenu.MruList);

			if (Qm?.Query != null && Lex.Ne(Qm.Query.Name, "RelatedStructures"))
			{ // allow current query if not defined and not from QuickSearch Cid "RelatedStrctures" query)
				MetaTreeNode ccn = new MetaTreeNode(MetaTreeNodeType.Query);
				ccn.Label = "Current Query";
				ccn.Name = ccn.Target = "RelatedCompoundsSourceQuery";
				nodes.Insert(0, ccn); // put at start
			}

			MruListCtl.Items.Clear();

			for (int ni = 0; ni < nodes.Count; ni++)
			{
				MetaTreeNode node = nodes[ni];
				ImageListBoxItem li = new ImageListBoxItem();
				li.ImageIndex = node.GetImageIndex();
				li.Value = (ni + 1).ToString() + ". " + node.Label;
				li.Tag = node;
				MruListCtl.Items.Add(li);

				MruListCtl.SetSelected(ni, SelectedNodes.Contains(node.Target));
			}

			if (MruListCtl.SelectedItems.Count == 0 && MruListCtl.Items.Count > 0)
				MruListCtl.SetSelected(0, true); // select first item if nothing selected

			return;
		}

		private void FavoritesButton_Click(object sender, EventArgs e)
		{
			BuildFavoritesMenu();

			FavoritesMenu.Show(FavoritesButton,
				new System.Drawing.Point(0, FavoritesButton.Height));
		}

		public void BuildFavoritesMenu()
		{
			List<MetaTreeNode> nodes = BuildMetaTreeNodeList(MainMenu.Favorites);

			FavoritesMenu.Items.Clear();

			foreach (MetaTreeNode node in nodes) 
			{
				ToolStripMenuItem mi = new ToolStripMenuItem();
				mi.Image = Bitmaps.Bitmaps16x16.Images[node.GetImageIndex()];
				mi.Text = node.Label;
				//mi.ForeColor = NewMenuItem.ForeColor;
				mi.Tag = node;
				mi.MouseDown += new MouseEventHandler(FavoritesMenuItem_MouseDown);
				FavoritesMenu.Items.Add(mi);
			}

		}

/// <summary>
/// Build a MetaTreeNodeList containing just queries or data table objects from a list of node names (MruList or Favorites)
/// </summary>
/// <param name="nodeNameList"></param>
/// <returns></returns>

		List<MetaTreeNode> BuildMetaTreeNodeList(List<string> nodeNameList)
		{

			List<MetaTreeNode> nodes = new List<MetaTreeNode>();
			int li = 0;
			while (li < nodeNameList.Count)
			{
				string nodeName = nodeNameList[li];
				if (String.IsNullOrEmpty(nodeName))
				{
					nodeNameList.RemoveAt(li);
					continue;
				}

				MetaTreeNode node = MainMenu.GetMetaTreeNode(nodeName);
				if (node != null)
				{
					li++;
					if (node.Type == MetaTreeNodeType.Query || MetaTreeNode.IsDataTableNodeType(node.Type))
						nodes.Add(node);

					else continue;
				}

				else // remove from list if can't find
					nodeNameList.RemoveAt(li); // remove from list if not in db
			}

			return nodes;
		}

		private void FavoritesMenuItem_MouseDown(object sender, MouseEventArgs e)
		{
			ToolStripMenuItem mi = (ToolStripMenuItem)sender;
			MetaTreeNode node = mi.Tag as MetaTreeNode;

			UpdateAndRenderMruList(node);
			return;
		}

		private void SelectFromDatabaseContentsButton_Click(object sender, EventArgs e)
		{
			MetaTreeNode node = SelectFromContents.SelectSingleItem(
				"Retrieve Related Compound Data",
				"Select a Query or Data Table to use to retrieve related data",
				MetaTreeNodeType.DataTableTypes,
				null,
				false);

			if (node == null) return;

			UpdateAndRenderMruList(node);

			return;
		}

		private void MruListCtl_DoubleClick(object sender, EventArgs e)
		{
			RunQueryButton_Click(null, null);
			return;
		}

		private void RunQueryButton_Click(object sender, EventArgs e)
		{
			if (!ValidateFormAndBuildQuery()) return;

			PreserveRelatedCheckmarkPreferences();

			MoleculeSelectorControl.AddToMruList(QueryMolCtl, StructureSearchType.Unknown);

			Query q = RelatedCompoundsQuery;

			this.Hide();
			bool success = QbUtil.RunPopupQuery(q, q.Name, OutputDest.WinForms, ExitingQueryResultsCallBack);
			if (!success) this.Show();

			return;
		}

		/// <summary>
		/// This code gets called when the EditQuery button is clicked on the associated PopupResults form
		/// </summary>

		public void ExitingQueryResultsCallBack(
			ExitingQueryResultsParms p)
		{
			if (p.ExitType == ExitingQueryResultsType.EditQuery)
			{
				try // close the results popup
				{
					p.Qrc.ParentForm.Dispose();
				}
				catch (Exception ex) { string msg = ex.Message; }

				this.Visible = true;
				return;
			}

			//else if (p.ExitType == ExitingQueryResultsType.ClosingWindow) // if results closed then close this as well
			//	try
			//	{
			//		this.Dispose();
			//		return;
			//	}
			//	catch (Exception ex) { string msg = ex.Message; }

			else // just make us visible again
			{
				this.Visible = true;
				return;
			}

		}

		/// <summary>
		/// Validate the form and build and validate the query
		/// </summary>
		/// <returns></returns>

		bool ValidateFormAndBuildQuery()
		{
			try
			{
				// Retrieve data for a single Cid

				if (CurrentCidCheckEdit.Checked)
				{
					string cid = CidCtl.Text;
					cid = CompoundId.Normalize(cid);
					if (Lex.IsUndefined(cid) || !CompoundIdUtil.Exists(cid))
						throw new Exception("Invalid or non-existant :" + RootTable.KeyMetaColumn.Label + ": " + CidCtl.Text);
				}

				// Retrieve data for related structures

				else if (RelatedStrsCheckEdit.Checked)
				{
					if (AltForms.Checked || MatchedPairs.Checked ||
						SmallWorld.Checked || SimilarSearch.Checked || Substructure.Checked) ;
					else throw new Exception("At least one type of structure search must be specified to find structurally-related compounds");
				}

				// Retrieve for marked Cids

				else if (MarkedCidsCheckEdit.Checked)
				{ 
					// OK
				}

				// Retrieve for all Cids in originating query

				else if (AllCidsCheckEdit.Checked)
				{
					// OK
				}

				else throw new Exception("A compound retrieval type must be specified");

				int mi = MruListCtl.SelectedIndex;
				if (mi < 0 || mi >= MruListCtl.ItemCount)
					throw new Exception("You must select a query or data table to use to retrieve related data");

				UpdateSelectedNodesSet();
				Query q = BuildQuery();
				MqlUtil.CheckTableCompatibility(q);
				RelatedCompoundsQuery = q;

				return true;
			}

			catch (Exception ex)
			{
				MessageBoxMx.ShowError(ex.Message);
				CidCtl.Focus();
				return false;
			}
		}

/// <summary>
/// Build the query
/// </summary>
		
		private Query BuildQuery()
		{
			Query q, q2;
			QueryTable qt;
			QueryColumn keyQc;
			MetaTreeNode mtn;
			string title = "";

			q = null;
			foreach (ImageListBoxItem item in MruListCtl.SelectedItems)
			{
				mtn = item.Tag as MetaTreeNode;

				if (mtn == null || // use base query
					(mtn.Type == MetaTreeNodeType.Query && Lex.Eq(mtn.Target, "RelatedCompoundsSourceQuery")))
				{
					if (q == null)
						q = SourceQuery.Clone(); // base new query on query we originated from

					else
						q.MergeQuery(SourceQuery); 
				}

				else if (mtn.Type == MetaTreeNodeType.Query) // selected query item
				{
					UserObjectType objType;
					int objId = -1;

					UserObject.ParseObjectTypeAndIdFromInternalName(mtn.Target, out objType, out objId);
					q2 = QbUtil.QueriesControl.GetQueryByUserObjectId(objId); // if this query is open, get the current, possibly edited, version

					if (q2 == null)
						q2 = QbUtil.ReadQuery(mtn.Target); // read in existing query

					if (q2 != null)
					{
						if (q == null)
							q = q2.Clone();

						else
							q.MergeQuery(q2);
					}
				}

				else if (mtn.IsDataTableType) // single data table
				{
					MetaTable mt = MetaTableCollection.Get(mtn.Target);
					if (mt == null) throw new Exception("Can't get metatable " + mtn.Target);

					if (q == null) q = new Query();
					if (Lex.IsUndefined(q.Name)) q.Name = mt.Label;
					if (q.GetQueryTableByName(mt.Name) == null) // add table if not already included
						qt = new QueryTable(q, mt);
				}

				else
				{
					throw new Exception("Invalid node type: " + mtn.Type);
				}
			}

			List<string> cidList = new List<string>();
			bool cidBasedQuery = true; // assume query is based on a list of cids

			string cid = CidCtl.Text;
			cid = CompoundId.Normalize(cid);

			// Retrieve data for a single Cid

			if (CurrentCidCheckEdit.Checked)
			{
				cidList.Add(cid);
				title = q.Name + " for " + RootTable.KeyMetaColumn.Label + " " + CidCtl.Text;
			}

// Retrieve data for related structures

			else if (RelatedStrsCheckEdit.Checked)
			{
				//	AltForms, MatchedPairs, SmallWorld, SimilarSearch, Substructure

				string queryStrName = QueryMolCtl.GetTemporaryStructureTag();
				if (Lex.IsUndefined(queryStrName)) queryStrName = "query structure";
				title = q.Name +  " compounds related to " + queryStrName;
				cidBasedQuery = false; // need to do a related structure search
			}

// Retrieve for marked Cids

			else if (MarkedCidsCheckEdit.Checked)
			{
				cidList = MarkedCidsList;
				title = q.Name + " for checked " + RootTable.KeyMetaColumn.Label + "s";
			}

// Retrieve for all Cids in originating query

			else if (AllCidsCheckEdit.Checked)
			{
				cidList = CurrentBaseQueryCidHitList;
				title = q.Name + " for current hitlist " + RootTable.KeyMetaColumn.Label + "s " + CidCtl.Text;
			}

			q.Name = title; // name query same as title ( + " (Q" + (++QueryCount) + ")"; // with query sequence id appended)

			// Remove existing criteria if requested

			if (!ApplyExistingCriteria.Checked)
				q.ClearAllQueryColumnCriteria();

// Modify the derived query to do either a Cid or Related-structure search

			q.IncludeRootTableAsNeeded();
			if (IncludeStructures.Checked) AddStructureTableToQuery(q);

			if (cidBasedQuery) ModifyQueryForCidSearch(q, cidList);
			else ModifyQueryForRelatedStructureSearch(q, QueryMolCtl);

			q.SingleStepExecution = false; // Single step fails for RelatedSS search (Done in QE now)

			return q;
		}

/// <summary>
/// Get the set of selected node targets
/// </summary>
/// <returns></returns>

		private void UpdateSelectedNodesSet()
		{
			Query q, q2;
			QueryTable qt;
			QueryColumn keyQc;
			MetaTreeNode mtn;
			string title = "";

			SelectedNodes = new HashSet<string>();
			foreach (ImageListBoxItem item in MruListCtl.SelectedItems)
			{
				mtn = item.Tag as MetaTreeNode;
				if (mtn != null) SelectedNodes.Add(mtn.Target);
			}

			return;
		}

		/// <summary>
		/// Add cid criteria to the derived query to get set of compounds desired
		/// </summary>
		/// <param name="q"></param>
		/// <param name="cidList"></param>

		void ModifyQueryForCidSearch(
			Query q,
			List<string> cidList)
		{
			QueryTable qt = q.Tables[0];
			QueryColumn keyQc = qt.KeyQueryColumn;

			ParsedSingleCriteria psc = new ParsedSingleCriteria();
			psc.QueryColumn = keyQc;

			if (cidList.Count == 1)
			{
				psc.Op = "=";
				psc.Value = cidList[0];
			}

			else
			{
				psc.Op = "in";
				psc.ValueList = cidList;
			}

			MqlUtil.ConvertParsedSingleCriteriaToQueryColumnCriteria(psc, keyQc, false);
			keyQc.CopyCriteriaToQueryKeyCritera(q);
			return;
		}

/// <summary>
/// Add criteria to query to do a related structure search
/// </summary>
/// <param name="q"></param>

		void ModifyQueryForRelatedStructureSearch(
			Query q,
			MoleculeControl queryMolCtl)
		{
			q.KeyCriteria = q.KeyCriteriaDisplay = ""; // always remove any existing key criteria

			QueryTable sqt = AddStructureTableToQuery(q); // be sure we have a structure table in the query
			QueryColumn sqc = sqt.FirstStructureQueryColumn;

			sqc.Selected = true;

			sqc.DisplayFormatString = "Highlight=true";
			if (AlignMatches.Checked) sqc.DisplayFormatString += ";Align=true";

			QueryColumn qc = sqt.GetQueryColumnByName("molSrchType"); // include search type
			if (qc != null) qc.Selected = true;

			qc = sqt.GetSimilarityScoreQueryColumn(); // and match score
			if (qc != null) qc.Selected = true;

			ParsedStructureCriteria pssc = new ParsedStructureCriteria();
			pssc.SearchType = StructureSearchType.Related;

			//string mfText = sqt.KeyQueryColumn.ActiveLabel + ": " + queryCid; // (queryCid no longer be accurate)
			//pssc.Structure = // just store a comment with the CID in the structure
			//	new ChemicalStructureMx(StructureFormat.MolFile, ChemicalStructureMx.GetTextMessageMolFile(mfText)); 

			MoleculeMx m = QueryMolCtl.Molecule;
			pssc.Molecule = new MoleculeMx(m.PrimaryFormat, m.PrimaryValue);

			if (AltForms.Checked) pssc.SearchTypeUnion |= StructureSearchType.FullStructure;
			if (MatchedPairs.Checked) pssc.SearchTypeUnion |= StructureSearchType.MatchedPairs;
			if (SmallWorld.Checked) pssc.SearchTypeUnion |= StructureSearchType.SmallWorld;
			if (SimilarSearch.Checked) pssc.SearchTypeUnion |= StructureSearchType.MolSim;
			if (Substructure.Checked) pssc.SearchTypeUnion |= StructureSearchType.Substructure;

			pssc.MinimumSimilarity = .75;
			pssc.MaxSimHits = 100;
			pssc.Highlight = true;
			pssc.Align = AlignMatches.Checked;
			pssc.ConvertToQueryColumnCriteria(sqc);

			if (ExcludeCurrentResultsCids.Checked && CurrentBaseQueryCidHitList != null) // add not in list criteria
			{
				string queryStrName = queryMolCtl.GetTemporaryStructureTag(); // see if we can get the cid

				q.KeysToExclude = new HashSet<string>(CurrentBaseQueryCidHitList);
				if (q.KeysToExclude.Contains(queryStrName)) // keep query id
					q.KeysToExclude.Remove(queryStrName);
			}

			else
				q.KeyCriteria = q.KeyCriteriaDisplay = ""; // no key criteria

			return;
		}

/// <summary>
/// Add structure table if not included and put first in query
/// </summary>
/// <param name="q"></param>
/// <returns></returns>

		QueryTable AddStructureTableToQuery(
			Query q)
		{
 			QueryTable sqt = q.GetQueryTableByName(q.RootMetaTable.Name); // already have root structure table?
			if (sqt == null)
			{
				sqt = new QueryTable(q.RootMetaTable);
				sqt.Query = q;
				q.InsertQueryTable(0, sqt); // put as first table
			}

			QueryColumn sqc = sqt.FirstStructureQueryColumn;
			if (sqc == null) throw new Exception("Structure column not found in " + sqt.MetaTable.Name);

			sqc.Selected = true; // be sure structure selected for display
			return sqt;
		}

		public void SetupCheckmarks()
		{
			string checkmarks = // get user preferences
				SS.I.UserIniFile.Read("RelatedCompoundsCheckmarks");

			if (Lex.IsDefined(checkmarks))
				DeserializeCheckmarks(checkmarks);

// Hide SmallWorld option if not permitted for this user

			if (!CriteriaStructure.SmallWorldIsAvailableForUser)
			{
				SmallWorld.Visible = false;
				SmallWorld.Checked = false;
				SimilarSearch.Location = SmallWorld.Location;
			}

			return;
		}

		string SerializeCheckmarks()
		{
			string txt = "";

			for (int ci = 0; ci < CheckmarkCtls.Length; ci++)
			{
				CheckEdit ce = CheckmarkCtls[ci];
				string ceName = ce.Name;
				if (ce.Checked) txt += ceName + ",";
			}

			if (Lex.IsUndefined(txt)) txt = "<none>"; // indicate that nothing is checked

			return txt;
		}

		void DeserializeCheckmarks(string txt)
		{
			for (int ci = 0; ci < CheckmarkCtls.Length; ci++)
			{
				CheckEdit ce = CheckmarkCtls[ci];
				bool check = Lex.Contains(txt, ce.Name + ",");
				if (ce.Checked != check) ce.Checked = check;
			}

			return;
		}

		/// <summary>
		/// Save preferences for use in later sessions
		/// </summary>
		internal void PreserveRelatedCheckmarkPreferences()
		{
			try
			{
				string checkmarks = SerializeCheckmarks();
				SS.I.UserIniFile.Write("RelatedCompoundsCheckmarks", checkmarks);
				return;
			}

			catch (Exception ex)
			{
				return;
			}

		}

		private void CidCtl_Click(object sender, EventArgs e)
		{
			if (!CurrentCidCheckEdit.Checked) // be sure this item is checked
				CurrentCidCheckEdit.Checked = true;
		}

		private void RelatedCompoundsDialog_Shown(object sender, EventArgs e)
		{
			if (CurrentCidCheckEdit.Checked) CidCtl.Focus();
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

		private void RetrieveModel_Click(object sender, EventArgs e)
		{
			Point p = RetrieveModel.PointToScreen(new System.Drawing.Point(0, RetrieveModel.Height));
			MoleculeSelectorControl.ShowModelSelectionMenu(p, QueryMolCtl, StructureSearchType.Related);
		}

		private void CloseBut_Click(object sender, EventArgs e)
		{
			this.Close();
			this.Dispose();
		}

		private void Timer1_Tick(object sender, EventArgs e)
		{
			if (ServiceFacade.ServiceFacade.InServiceCall) return; // avoid multiplexing service calls that could cause problems
			if (InTimer_Tick) return;
			InTimer_Tick = true;

			string currentCid = CidCtl.Text;
			if (currentCid == PreviousCid)
			{
				InTimer_Tick = false;
				return;
			}

			PreviousCid = currentCid;

			string cid = CompoundId.Normalize(currentCid);
			MetaTable mt = CompoundId.GetRootMetaTableFromCid(cid);
			MoleculeMx mol = MoleculeUtil.SelectMoleculeForCid(cid, mt);
			QueryMolCtl.SetupAndRenderMolecule(mol);

			InTimer_Tick = false;
			return;
		}

		private void CidCtl_Enter(object sender, EventArgs e)
		{
			Timer1.Enabled = true;
			//CidCtl.Focus();
		}

		private void CidCtl_Leave(object sender, EventArgs e)
		{
			Timer1.Enabled = false;
		}

	}
}