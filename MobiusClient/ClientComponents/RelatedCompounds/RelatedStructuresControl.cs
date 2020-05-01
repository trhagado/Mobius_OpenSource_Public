using Mobius.ComOps;
using Mobius.Data;
using Mobius.CdkMx;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class RelatedStructuresControl : UserControl
	{
		public RelatedStructureControlManager RSM; // associated manager
		RelatedStructureSearchResults RelatedStructuresResults => (RSM != null ? RSM.RelatedStructuresResults : null); 

		public SimpleDelegate RenderSearchResultsDelegate;
		public SimpleDelegate RenderSearchResultsCompleteCallBack;

		bool SmallWorldAvailable => CriteriaStructure.SmallWorldIsAvailableForUser; // Small world available for user

		public CheckEdit[] CheckmarkCtls // Array of checkmark controls
		{
			get
			{
				if (_checkmarkCtls == null) _checkmarkCtls = new CheckEdit[] {
					AltForms, MatchedPairs, SmallWorld, SimilarStructure, AllMatches, CorpDB, ChemblDB, IncludeQueryStruct };
				return _checkmarkCtls;
			}

		}
		CheckEdit[] _checkmarkCtls = null;

		static string CheckmarkPrefs = null; // last preference settings saved
		static string Checkmarks = null; // current preferences across all instances

		// Abbreviations for database Ids

		static int CorpDbId = StructSearchMatch.CorpDbId;
		static int ChemblDbId = StructSearchMatch.ChemblDbId;

// Abbreviations for search types

		static StructureSearchType SST_FullStructure = StructureSearchType.FullStructure;
		static StructureSearchType SST_MatchedPairs = StructureSearchType.MatchedPairs;
		static StructureSearchType SST_SmallWorld = StructureSearchType.SmallWorld;
		static StructureSearchType SST_MolSim = StructureSearchType.MolSim;

		public bool InSetup = false;

		public RelatedStructuresControl()
		{
			MoleculeGridPageControl.RemoveToolPanelInConstructor = false; // keep tool panel and properly layout of controls
			InitializeComponent();
			MoleculeGridPageControl.RemoveToolPanelInConstructor = true;

			if (SystemUtil.InDesignMode) return;

			SplitContainer.Panel1.BorderStyle = BorderStyles.NoBorder;
			SplitContainer.Panel2.BorderStyle = BorderStyles.NoBorder;

			this.BorderStyle = BorderStyle.None;
			MoleculeGridPageControl.BorderStyle = BorderStyle.None;

			MoleculeGridPageControl.MoleculeGridPanel.Dock = DockStyle.Fill;
			MoleculeGridPageControl.MoleculeGridPanel.BandedViewGrid.SetColumnAutoWidth(true); // show all cols

			SplitContainer.BorderStyle = BorderStyles.NoBorder;

			return;
		}

		/// <summary>
		/// Display the related counts
		/// </summary>
		/// <param name="rss"></param>
		/// 
		public void DisplayRelatedCidCounts(
			RelatedStructureSearchResults rss)
		{
			int altCnt, mmpCnt, swCnt, simCnt, allMatchCnt;

			//MoleculeGridPageControl.Width = this.Width - 3;
			//MoleculeGridPageControl.Height = this.Height - MoleculeGridPageControl.Top - 3;

			MoleculeGridPageControl mgpc = MoleculeGridPageControl;
			if (mgpc.EditQueryButton.Visible) // adjust tool panel for proper display
			{
				mgpc.EditQueryButton.Visible = false;
				//mgpc.MarkDataButton.Visible = false;
				mgpc.FormattingButton.Visible = false;
				mgpc.AdjustToolPanelButtonSpacing();
			}

			if (rss == null)
			{
				DisplayEmptyCidCountsGrid();
				return;
			}

			altCnt = SetRelatedCidCount(AltForms, SumByDb(rss.AltCorpDbCnt, rss.AltChemblCnt)); // alternate forms

			mmpCnt = SetRelatedCidCount(MatchedPairs, SumByDb(rss.MmpCorpDbCnt, rss.MmpChemblCnt)); // matched pairs, no ChEMBL for now

			if (SmallWorldAvailable)
				swCnt = SetRelatedCidCount(SmallWorld, SumByDb(rss.SwCorpDbCnt, rss.SwChemblCnt)); // SmallWorld

			else
			{
				SmallWorld.Checked = false;
				if (MatchCountsPanel.RowCount == 5) // remove SmallWorld from the MatchCountsPanel
				{
					MatchCountsPanel.SetRow(labelControl3, 2);
					MatchCountsPanel.SetRow(SimilarStructure, 2);
					MatchCountsPanel.SetRow(labelControl6, 3);
					MatchCountsPanel.SetRow(AllMatches, 3);

					MatchCountsPanel.SetRow(SmallWorldLabel, 4);
					MatchCountsPanel.SetRow(SmallWorld, 4);
					SmallWorldLabel.Visible = SmallWorld.Visible = false;
					SmallWorld.Checked = false;
					MatchCountsPanel.RowCount = 4;
				}

				swCnt = 0;
			}

			simCnt = SetRelatedCidCount(SimilarStructure, SumByDb(rss.SimCorpDbCnt, rss.SimChemblCnt)); // similar

			allMatchCnt = SetRelatedCidCount(AllMatches, altCnt + mmpCnt + swCnt + simCnt); // total
			
			SetRelatedCidCount(CorpDB, rss.CorpDbCnt); // these values don't change during life of search results
			SetRelatedCidCount(ChemblDB, rss.ChemblCnt);

			return;
		}

/// <summary>
/// Sum by database checking to see if database is included
/// </summary>
/// <param name="args"></param>
/// <returns></returns>

		int SumByDb(params int[] args)
		{
			int sum = 0, definedValCount = 0;
			for (int dbi = 0; dbi < 2; dbi++)
			{
				int arg = args[dbi];

				if (dbi == StructSearchMatch.CorpDbId && !CorpDB.Checked) arg = -1; // CorpDb first (=0)
				else if (dbi == StructSearchMatch.ChemblDbId && !ChemblDB.Checked) arg = -1; // Chembl second (=1)

				if (arg < 0) continue;

				sum += arg;
				definedValCount++;
			}

			if (definedValCount > 0) return sum;
			else return -1; // nothing defined
		}

		/// <summary>
		/// Add together an array of ints treating negative values and values for DB not selected as missing values
		/// </summary>
		/// <param name="arg"></param>
		/// <returns></returns>

		int Sum(params int[] args)
		{
			int sum = 0, definedValCount = 0;
			foreach (int arg in args)
			{
				if (arg < 0) continue;

				sum += arg;
				definedValCount++;
			}

			if (definedValCount > 0) return sum;
			else return -1; // nothing defined
		}

		public void DisplayEmptyCidCountsGrid()
		{

			RelatedStructureSearchResults rss2 = new RelatedStructureSearchResults();
			DisplayRelatedCidCounts(rss2);

			return;
		}

		int SetRelatedCidCount(
			CheckEdit ctl,
			int count)
		{
			if (count < 0) ctl.Text = ""; // not determined/no data
			else ctl.Text = count.ToString();
			return count;
		}

		void SetRelatedCidCount2(
			CheckEdit ctl,
			int resultCount,
			int cidCount)
		{
			if (resultCount < 0) ctl.Text = ""; // not determined/no data
			else
			{
				string txt = resultCount.ToString();
				if (cidCount > 0 && cidCount != resultCount) txt += " (" + cidCount + ")";
				ctl.Text = txt;
			}
			return;
		}

		private void IncludeQueryStruct_CheckedChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			RSM.DisplayRelatedCidCountsAndStructures(RelatedStructuresResults);
			return;
		}

		private void AltForms_CheckedChanged(object sender, EventArgs e)
		{
			//DebugLog.Message("AltForms.Checked " + AltForms.Checked + new StackTrace(true));
			CheckedChanged(AltForms);
		}

		private void MatchedPairs_CheckedChanged(object sender, EventArgs e)
		{
			CheckedChanged(MatchedPairs);
		}

		private void SmallWorld_CheckedChanged(object sender, EventArgs e)
		{
			CheckedChanged(SmallWorld);
		}

		private void Similar_CheckedChanged(object sender, EventArgs e)
		{
			CheckedChanged(SimilarStructure);
		}

		private void AllMatches_CheckedChanged(object sender, EventArgs e)
		{
			CheckedChanged(AllMatches);
		}

		private void CorpDB_CheckedChanged(object sender, EventArgs e)
		{
			CheckedChanged(CorpDB);
		}

		private void ChemblDB_CheckedChanged(object sender, EventArgs e)
		{
			CheckedChanged(ChemblDB);
		}

		public void ClearAllSearchOptions()
		{
			InSetup = true;

			foreach (CheckEdit ce in CheckmarkCtls) // Array of checkmark controls
			{
				ce.Checked = false;
				ce.Text = "";
			}

			InSetup = false;

			return;
		}

/// <summary>
/// Return true if any search options checked
/// </summary>
/// <returns></returns>

		public bool AnySearchTypesChecked()
		{
			if (AltForms.Checked || MatchedPairs.Checked || SimilarStructure.Checked || SmallWorld.Checked || AllMatches.Checked)
				return true;

			else return false;
		}

		/// <summary>
		/// Process the change in the checked result types and display corresponding data
		/// </summary>
		/// <param name="ctl"></param>

		private void CheckedChanged(
			CheckEdit ctl)
		{
			if (InSetup) return;

			try
			{
				InSetup = true;

				if (ctl == AllMatches)
				{
					bool amc = AllMatches.Checked;
					if (AltForms.Enabled) AltForms.Checked = amc;
					if (MatchedPairs.Enabled) MatchedPairs.Checked = amc;
					if (SimilarStructure.Enabled) SimilarStructure.Checked = amc;
					if (SmallWorld.Enabled) SmallWorld.Checked = amc;

					//if (SmallWorldAvailable) SmallWorld.Checked = AllMatches.Checked;
					//else SmallWorld.Checked = false;
				}

				else
				{
					bool allChecked = 
						(AltForms.Checked || !AltForms.Enabled) && 
						(MatchedPairs.Checked || !MatchedPairs.Enabled) && 
						(SimilarStructure.Checked || !SimilarStructure.Enabled) &&
						(SmallWorld.Checked || !SmallWorld.Enabled);

					//if (SmallWorldAvailable) allChecked = allChecked && SmallWorld.Checked;
					AllMatches.Checked = allChecked;
				}

				Checkmarks = SerializeCheckmarks(); // save current settings

				MatchCountsPanel.Refresh(); // redraw the counts grid

				if (RenderSearchResultsDelegate != null)
					DelayedCallback.Schedule(RenderSearchResultsDelegate);

				return;
			}

			catch (Exception ex) { ex = ex; }

			finally { InSetup = false; }
		}

		/// <summary>
		/// Structure matches are filtered by DB, search type and currentList Membership
		/// Match counts are filtered by currentList Membership.
		/// </summary>
		/// <param name="matchSubsets"></param>
		/// <param name="excludeCurrentHitList"></param>
		/// <param name="rss"></param>
		/// <returns></returns>

		public List<StructSearchMatch> FilterMatchList(
			RelatedStructureSearchResults rss)
		{
			StructSearchMatch ssm;

			List<StructSearchMatch> passList = new List<StructSearchMatch>();
			HashSet<string> cidSet = new HashSet<string>();
			//if (excludeCurrentHitList) cidsToExclude = GetCurrentHitList();

			rss.ResetBaseSearchTypeCounts();

			//if (IncludeQueryStruct.Checked)
			//{
			//	ssm = new StructSearchMatch();
			//	ssm.UCI = 0; // Query structure
			//	ssm.SrcDbId = StructSearchMatch.SrcNameToId(RSM.CidMtName);
			//	ssm.SrcCid = RSM.Cid; // source compound id
			//	ssm.MolString = RenditorControl.ChimeString;
			//	ssm.MolStringFormat = StructureFormat.Chime;
			//	ssm.MatchScore = 1.0f;

			//	passList.Add(ssm);
			//}

			for (int mi = 0; mi < rss.MatchList.Count; mi++) // check the possible db/search type combinations and add to passList if matches and allowed
			{
				ssm = rss.MatchList[mi];
				bool include = false;

				if (Lex.Eq(ssm.SrcCid, RSM.Cid)) // query cid
				{
					if (IncludeQueryStruct.Checked && !cidSet.Contains(ssm.SrcCid))
						include = true; // include Query cid
				}

				else if (ssm.SrcDbId == CorpDbId && CorpDB.Checked)
				{
					if (cidSet.Contains(ssm.SrcCid)) // dup cid?
					{
						rss.DupCnt++;
						//if (RemoveDups.Checked) continue; 
					}

					if (FilterMatch(ssm, CorpDbId, SST_FullStructure, AltForms, ref rss.AltCorpDbCnt, ref include)) ;
					else if (FilterMatch(ssm, CorpDbId, SST_MatchedPairs, MatchedPairs, ref rss.MmpCorpDbCnt, ref include)) ;
					else if (FilterMatch(ssm, CorpDbId, SST_SmallWorld, SmallWorld, ref rss.SwCorpDbCnt, ref include)) ;
					else if (FilterMatch(ssm, CorpDbId, SST_MolSim, SimilarStructure, ref rss.SimCorpDbCnt, ref include)) ;
				}

				else if (ssm.SrcDbId == ChemblDbId && ChemblDB.Checked)
				{
					if (cidSet.Contains(ssm.SrcCid)) // dup cid?
					{
						rss.DupCnt++;
						//if (RemoveDups.Checked) continue;
					}

					if (FilterMatch(ssm, ChemblDbId, SST_MatchedPairs, MatchedPairs, ref rss.MmpChemblCnt, ref include)) ;
					else if (FilterMatch(ssm, ChemblDbId, SST_SmallWorld, SmallWorld, ref rss.SwChemblCnt, ref include)) ;
					else if (FilterMatch(ssm, ChemblDbId, SST_MolSim, SimilarStructure, ref rss.SimChemblCnt, ref include)) ;
				}

				if (include)
				{
					passList.Add(ssm);
					cidSet.Add(ssm.SrcCid);
				}
			}

			return passList;
		}

/// <summary>
/// Determine if match passes the current filter settings
/// </summary>
/// <param name="ssm"></param>
/// <param name="dbId"></param>
/// <param name="searchType"></param>
/// <param name="searchTypeCount"></param>
/// <param name="include"></param>
/// <returns></returns>

		bool FilterMatch(
			StructSearchMatch ssm,
			int dbId,
			StructureSearchType searchType,
			CheckEdit searchTypeCtl,
			ref int searchTypeCount,
			ref bool include)
		{

			bool dbChecked = false;
			if (dbId == CorpDbId) dbChecked = CorpDB.Checked;
			else if (dbId == ChemblDbId) dbChecked = ChemblDB.Checked;

			if (!dbChecked || ssm.SrcDbId != dbId || ssm.SearchType != searchType) return false;

			if (searchTypeCtl.Checked)
				include = true;

			searchTypeCount++;

			return true;
		}

		public void SetupCheckmarks()
		{
			if (InSetup) return;

			if (Checkmarks == null)
			{
				if (CheckmarkPrefs == null)
					CheckmarkPrefs = // get user preferences
						SS.I.UserIniFile.Read("RelatedStructuresCheckmarks", "Altforms,MatchedPairs,SmallWorld,SimilarStructure,AllMatches,CorpDb,");

				Checkmarks = CheckmarkPrefs;
			}

			InSetup = true;

			DeserializeCheckmarks(Checkmarks);

			InSetup = false;
			return;
		}

		string SerializeCheckmarks()
		{
			string txt = "";
			RSSConfig.ReadConfigSettings();

			GetCheckMark(AltForms, RSSConfig.SearchCorpDbFssEnabled, ref txt);
			GetCheckMark(MatchedPairs, RSSConfig.SearchCorpDbMmpEnabled, ref txt);
			GetCheckMark(SmallWorld, RSSConfig.SearchCorpDbSmallWorldEnabled, ref txt);
			GetCheckMark(SimilarStructure, RSSConfig.SearchSimEnabled, ref txt);

			GetCheckMark(AllMatches, true, ref txt);

			GetCheckMark(CorpDB, true, ref txt);
			GetCheckMark(ChemblDB, true, ref txt);

			GetCheckMark(IncludeQueryStruct, true, ref txt);

			if (Lex.IsUndefined(txt)) txt = "<none>"; // indicate that nothing is checked

			return txt;
		}

		void GetCheckMark(
			CheckEdit ce,
			bool enabled,
			ref string settings)
		{
			if (ce.Checked || !enabled)	settings += ce.Name + ",";
		}


		void DeserializeCheckmarks(string txt)
		{
			RSSConfig.ReadConfigSettings();

			if (!SmallWorldAvailable) RSSConfig.SearchSmallWorldEnabled = false;

			SetCheckMark(AltForms, RSSConfig.SearchFssEnabled, txt);
			SetCheckMark(MatchedPairs, RSSConfig.SearchMmpEnabled, txt);
			SetCheckMark(SmallWorld, RSSConfig.SearchSmallWorldEnabled, txt);
			SetCheckMark(SimilarStructure, RSSConfig.SearchSimEnabled, txt);

			SetCheckMark(AllMatches, true, txt);

			SetCheckMark(CorpDB, true, txt);
			SetCheckMark(ChemblDB, true, txt);

			SetCheckMark(IncludeQueryStruct, true, txt);

			return;
		}

		void SetCheckMark(
			CheckEdit ce,
			bool enabled,
			string settings)
		{
			if (enabled)
			{
				bool check = Lex.Contains(settings, ce.Name + ",");
				if (ce.Checked != check) ce.Checked = check;
				if (!ce.Enabled) ce.Enabled = true;
			}

			else
			{
				if (!ce.Enabled) ce.Enabled = true;
				ce.Checked = false;
				ce.Text = "";
				ce.Enabled = false;
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
				//SystemUtil.Beep(); // debug

				//if (CheckmarkPrefs == null || Checkmarks == null) return;

				Checkmarks = SerializeCheckmarks();
				if (Lex.Eq(Checkmarks, CheckmarkPrefs)) return;

				CheckmarkPrefs = Checkmarks;
				SS.I.UserIniFile.Write("RelatedStructuresCheckmarks", Checkmarks);
				return;
			}

			catch (Exception ex)
			{
				return; 
			}

		}

		private void RelatedStructuresControl_SizeChanged(object sender, EventArgs e)
		{
			return;
		}
	}
}
