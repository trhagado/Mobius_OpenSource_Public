using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;
using Mobius.CdkSearchMx;
using Mobius.CdkMx;

using org.openscience.cdk;
using org.openscience.cdk.interfaces;

using System;
using System.IO;
using System.Data;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Mobius.QueryEngineLibrary
{

	/// <summary>
	/// Do fast searches for structures "Related" to a supplied query structure
	/// </summary>

	public class RelatedStructureSearch
	{

// Parameters to be defined before calling ExecuteSearch

		public string QueryMtName = ""; // metatable that QueryCid is from
		public string QueryCid = "";
		public string QueryChimeString = "";
		public int SearchId = -1; // Id of this search (optional)

		// Search Types 

		public bool SearchFSS = true;
		public bool SearchMmp = true;
		public bool SearchSmallWorld = true;
		public bool SearchSim = true;
		public bool SearchSSS = true;

		// Databases

		public bool SearchCorp = true;
		public bool SearchChembl = true;

		// Include the query structure in the results

		public bool IncludeQueryStructure = true;

		public HashSet<string> KeysToExclude = null;

// Internal fields

		string LargestFragmentChimeString = "";
		List<KeyValuePair<string, IAtomContainer>> FragmentList = null; // list of fragments (largest to smallest) with small and common fragments removed

		Dictionary<string, Subsearch> FssSubsearches; // individual FssSubsearches that are combined

		// Dictionary of of cids by type with first result row

		public HashSet<string>
			FssCidSet = new HashSet<string>(),
			CorpFssCidSet = new HashSet<string>(),
			ChemblFssCidSet = new HashSet<string>(),

			SssMatchCidSet = new HashSet<string>(),

			MmpCidSet = new HashSet<string>(),
			SmallWorldCidSet = new HashSet<string>(),
			SimCidSet = new HashSet<string>(),

			CorpMatchCidSet = new HashSet<string>(),
			ChemblMatchCidSet = new HashSet<string>(),
			AllMatchesCidSet = new HashSet<string>();

		// Lists of detailed match results by type 

		public List<StructSearchMatch>
			FssMatches = new List<StructSearchMatch>(),
			CorpFssMatches = new List<StructSearchMatch>(),
			ChemblFssMatches = new List<StructSearchMatch>(),

			SssMatches = new List<StructSearchMatch>(),
			CorpSssMatches = new List<StructSearchMatch>(),
			ChemblSssMatches = new List<StructSearchMatch>(),

			SimMatches = new List<StructSearchMatch>(),
			CorpSimMatches = new List<StructSearchMatch>(),
			ChemblSimMatches = new List<StructSearchMatch>(),

			MmpMatches = new List<StructSearchMatch>(),
			CorpMmpMatches = new List<StructSearchMatch>(),
			ChemblMmpMatches = new List<StructSearchMatch>(),

			SmallWorldMatches = new List<StructSearchMatch>(),
			CorpSmallWorldMatches = new List<StructSearchMatch>(),
			ChemblSmallWorldMatches = new List<StructSearchMatch>(),

			CorpMatches = new List<StructSearchMatch>(),
			ChemblMatches = new List<StructSearchMatch>(),
			AllMatches = new List<StructSearchMatch>();

		bool FssSearchFailed = false;
		bool SssSearchFailed = false;
		bool SimSearchFailed = false;
		bool MmpSearchFailed = false;
		bool SmallWorldSearchFailed = false;
		bool Superseded = false; // usually because the search was superseded by another search

		bool StructureRetrievalStarted = false; // if true structure retrieval has been started
		bool StructureRetrievalCompleted = false; // if true structure retrieval is complete
		Thread CorpRetrieveStructuresThread = null;
		Thread ChemblRetrieveStructuresThread = null;
		object StructureRetrievalWaitLock = new object();

		public int MaxHits = DefaultMaxHits; // maximum number of hits kept for each (search-method, database) combination
		public static int DefaultMaxHits = 25;

		public static int MaxSearchIdReceived = -1; // Max search id received. Allows earlier searches to be cancelled if a larger id is received

		private static object NetezzaOdbcLock = new object(); // used to serialize access to the non-thread-safe Netezza ODBC driver
		private static object PrepareQueryChimeLock = new object(); // thread lock for query chime preparation

		private static Dictionary<string, string> FssLocks = new Dictionary<string, string> {
			{ "CorpExact", "CorpExact" },
			{ "CorpFragment", "CorpFragment" },
			{ "CorpIsomer", "CorpIsomer" },
			{ "CorpTautomer", "CorpTautomer" },

			{ "ChemblExact", "ChemblExact" },
			{ "ChemblFragment", "ChemblFragment" },
			{ "ChemblIsomer", "ChemblIsomer" },
			{ "ChemblTautomer", "ChemblTautomer" },
		};

		static bool UseMultipleThreads => RSSConfig.UseMultipleThreads;
		static bool UseCachedResults => RSSConfig.UseCachedResults;
		static bool Debug => RSSConfig.Debug;

		private static List<RelatedStructureSearch> SearchHistory = new List<RelatedStructureSearch>();

		private static string LogFileName { get { return ServicesDirs.LogDir + @"\RelatedStructureSearch - [Date].log"; } } // Allow logging to RelatedStructureSearch-specific file

		/// <summary>
		/// InitializeForSession
		/// </summary>

		public static void InitializeForSession()
		{
			RSSConfig.ReadConfigSettings();
			return;
		}

		/// <summary>
		/// Execute related-structure search and return resulting hit counts
		/// <param name="cid"></param>
		/// <param name="mtName"></param>
		/// <param name="chime"></param>
		/// <returns></returns>

		public static string GetRelatedMatchCounts(
			string cid,
			string mtName,
			string chime,
			StructureSearchType searchTypes,
			int searchId)
		{
		
			string result = "";

			RelatedStructureSearch rss = ExecuteSearchWithHistoryCheck(cid, mtName, chime, searchTypes, searchId);

			result =
				GetCount(rss.CorpFssMatches).ToString() + ";" +
				GetCount(rss.CorpSimMatches) + ";" +
				GetCount(rss.CorpMmpMatches) + ";" +
				(IsSmallWorldClient ? GetCount(rss.CorpSmallWorldMatches) + ";" : "") + // include in newer version
				GetCount(rss.CorpMatches) + ";" +
				GetCount(rss.ChemblFssMatches) + ";" + 
				GetCount(rss.ChemblSimMatches) + ";" + 
				GetCount(rss.ChemblMmpMatches) + ";" +
				(IsSmallWorldClient ? GetCount(rss.ChemblSmallWorldMatches) + ";" : "") + // include in newer version
				GetCount(rss.ChemblMatches);

			return result;
		}

		/// <summary>
		/// Return true if client supports smallworld
		/// </summary>

		public static bool IsSmallWorldClient
		{
			get
			{
				return ClientState.MobiusClientVersionIsAtLeast(4, 1);
			}
		}

		static int GetCount(List<StructSearchMatch> list)
		{
			if (list == null) return -1;
			else return list.Count;
		}

		/// <summary>
		/// Get serialized form of the list of structures related to the supplied compound id
		/// </summary>
		/// <param name="queryCid"></param>
		/// <param name="mtName"></param>
		/// <param name="chime"></param>
		/// <returns></returns>

		static public string GetRelatedMatchRowsSerialized(
			string queryCid,
			string mtName)
		{
			RelatedStructureSearch rss = GetRelatedMatchRows(queryCid, mtName);

			StringBuilder sb = new StringBuilder();
			foreach (StructSearchMatch ssm in rss.AllMatches)
			{
				string serializedSsm = ssm.Serialize();
				sb.Append(serializedSsm);
				sb.Append("\n");
			}

			return sb.ToString();
		}

		static public RelatedStructureSearch GetRelatedMatchRows(
			string queryCid,
			string mtName)
		{
			RelatedStructureSearch rss = FindInSearchHistory(queryCid, mtName); // Check existing first
			if (rss == null)
			{
				string msg = "Initial search results not found for: " + queryCid + "." + mtName;
				DebugLogMessage(msg);
				throw new Exception(msg);
			}

			//lock (rss.StructureRetrievalWaitLock) // need this lock?
			{
				if (rss.StructureRetrievalCompleted) // already have structures
				{
					if (UseCachedResults) return rss; // if not debugging just return
					else rss.StructureRetrievalCompleted = false; // start retrieval again if debugging
				}

				if (!rss.StructureRetrievalStarted) rss.StartRetrieveRelatedStructures(); // start retrieval

				rss.WaitForRetrieveStructuresCompletion();
			}

			return rss;
		}

		/// <summary>
		/// Get list of structures related to the supplied compound id
		/// </summary>
		/// <param name="cid"></param>
		/// <param name="mtName"></param>
		/// <param name="chime"></param>
		/// <returns></returns>

		public static RelatedStructureSearch ExecuteSearchWithHistoryCheck(
			string cid,
			string mtName,
			string chime,
			StructureSearchType searchTypes,
			int searchId)
		{
			RelatedStructureSearch rss = FindInSearchHistory(cid, mtName); // Check existing first
			if (rss != null && UseCachedResults)
			{
				if (Debug)
					DebugLogMessage("********* Search Service - Found existing results in Search History for " + mtName + " CID " + cid + " **********");

				return rss;
			}

			// If doesn't already exist then execute the search

			rss = new RelatedStructureSearch();
			rss.QueryMtName = mtName; // copy parms so threads can pick up
			rss.QueryCid = cid;
			rss.QueryChimeString = chime;
			rss.SearchId = searchId;

			rss.SearchFSS = SST.IsFull(searchTypes);
			rss.SearchMmp = SST.IsMmp(searchTypes);
			rss.SearchSmallWorld = SST.IsSw(searchTypes);
			rss.SearchSim = SST.IsSim(searchTypes);
			rss.SearchSSS = SST.IsSSS(searchTypes);

			if (searchId > MaxSearchIdReceived) // keep latest request
				MaxSearchIdReceived = searchId;

			rss.ExecuteSearch(); // do search & integrate results into the RelatedStructureSearch just added to the history list
			return rss;
		}

		/// <summary>
		/// Search history to see if we have this already
		/// </summary>
		/// <param name="cid"></param>
		/// <param name="mtName"></param>
		/// <returns></returns>

		static RelatedStructureSearch FindInSearchHistory(
				string cid,
				string mtName)
		{
			foreach (RelatedStructureSearch rss0 in SearchHistory)
			{
				if (Lex.Eq(rss0.QueryCid, cid) && Lex.Eq(rss0.QueryMtName, mtName))
				{
					return rss0;
				}
			}

			return null; // not found
		}

		/// <summary>
		/// Execute search and store resulting keys and scores
		/// </summary>

		public void ExecuteSearch()
		{
			Thread fssThread = null, simSrchThread = null, sssSrchThread = null, mmpSrchThread = null, swSrchThread = null;
			DateTime t0 = DateTime.Now;

			if (Debug)
				DebugLogMessage("********* Search Service - Starting search " + SearchId + " for CID: " + QueryCid + " **********");

			MetaTable mt = MetaTableCollection.Get("rss_corp_structures"); // be sure rss Corp structure table exists and user can access it
			if (mt == null)
			{
				if (Debug)
					DebugLogMessage("Can't perform search. RSS_CORP_STRUCTURES not found or authorized for this user.");

				return; // search not performed
			}

			try
			{
				RSSConfig.ReadConfigSettings();

				if (!IsSmallWorldClient)
					RSSConfig.SearchSmallWorldEnabled = RSSConfig.SearchCorpDbSmallWorldEnabled = RSSConfig.SearchChemblSmallWorldEnabled = false;

				lock (PrepareQueryChimeLock) // single-thread this to avoid structure manipulate errors due to multithreading
				{
					if (Lex.IsUndefined(QueryChimeString)) throw new Exception("QueryChimeString not defined");

					MoleculeMx cs = new MoleculeMx(MoleculeFormat.Chime, QueryChimeString);
					string smiles = cs.GetSmilesString();
					FragmentList = CdkUtil.FragmentAndCanonicalizeSmiles(smiles, true); // get list of fragments (largest to smallest) with small and common fragments removed
					if (FragmentList.Count == 0) return; // nothing to search

					string smilesQuery = FragmentList[0].Key; // just do largest structure
					IAtomContainer mol = FragmentList[0].Value;

					CdkUtil.ConfigureAtomContainer(mol);
					mol = CdkUtil.GenerateCoordinates(mol); // need coords for proper stereo identification
					string molFile = CdkUtil.AtomContainerToMolFile(mol);
					LargestFragmentChimeString = MoleculeMx.MolFileToChimeString(molFile); // get chime string
				}

				if (UseMultipleThreads)
				{
					// Start full structure search thread

					if (SearchFSS && RSSConfig.SearchFssEnabled)
					{
						if (Debug) DebugLogMessage(SearchId.ToString() + ". Starting thread ExecuteFssSearchesThreadMethod");

						fssThread = new Thread(new ThreadStart(ExecuteFssSearchesThreadMethod));
						fssThread.Name = "ExecuteFssSearchesThreadMethod";
						fssThread.IsBackground = true;
						fssThread.SetApartmentState(ApartmentState.STA);
						fssThread.Start();
					}

					else DebugLogMessage("SearchFss = false");

					// Start matched pair structure search thread

					if (SearchMmp && RSSConfig.SearchMmpEnabled)
					{
						if (Debug) DebugLogMessage(SearchId.ToString() + ". Starting thread ExecuteMmpSearchThreadMethod");

						mmpSrchThread = new Thread(new ThreadStart(ExecuteMmpSearchThreadMethod));
						mmpSrchThread.Name = "ExecuteMmpSearchThreadMethod";
						mmpSrchThread.IsBackground = true;
						mmpSrchThread.SetApartmentState(ApartmentState.STA);
						mmpSrchThread.Start();
					}

					else DebugLogMessage("SearchMmp = false");

					// Start SmallWorld search thread

					if (SearchSmallWorld && RSSConfig.SearchSmallWorldEnabled)
					{
						if (Debug) DebugLogMessage(SearchId.ToString() + ". Starting thread ExecuteSmallWorldSearchThreadMethod");

						swSrchThread = new Thread(new ThreadStart(ExecuteSmallWorldSearchThreadMethod));
						swSrchThread.Name = "ExecuteSmallWorldSearchThreadMethod";
						swSrchThread.IsBackground = true;
						swSrchThread.SetApartmentState(ApartmentState.STA);
						swSrchThread.Start();
					}

					else if (Debug) DebugLogMessage("SearchSmallWorld = false");


					// Start sim structure search thread

					if (SearchSim && RSSConfig.SearchSimEnabled)
					{
						if (Debug) DebugLogMessage(SearchId.ToString() + ". Starting thread ExecuteSimSearchesThreadMethod");

						simSrchThread = new Thread(new ThreadStart(ExecuteSimSearchesThreadMethod));
						simSrchThread.Name = "ExecuteSimSearchesThreadMethod";
						simSrchThread.IsBackground = true;
						simSrchThread.SetApartmentState(ApartmentState.STA);
						simSrchThread.Start();
					}

					else if (Debug) DebugLogMessage("SearchSim = false");

					// Start substructre structure search thread

					if (SearchSSS && RSSConfig.SearchSssEnabled)
					{
						if (Debug) DebugLogMessage(SearchId.ToString() + ". Starting thread _");

						sssSrchThread = new Thread(new ThreadStart(ExecuteSssSearchesThreadMethod));
						sssSrchThread.Name = "ExecuteSssSearchesThreadMethod";
						sssSrchThread.IsBackground = true;
						sssSrchThread.SetApartmentState(ApartmentState.STA);
						sssSrchThread.Start();
					}

					else if (Debug) DebugLogMessage("SearchSSS = false");

					// Wait for all searches to finish

					if (Debug) DebugLogMessage(SearchId.ToString() + ". Waiting for search threads to finish...");

					if (SearchFSS && RSSConfig.SearchFssEnabled) fssThread.Join();
					if (SearchSim && RSSConfig.SearchSimEnabled) simSrchThread.Join();
					if (SearchSSS && RSSConfig.SearchSssEnabled) sssSrchThread.Join();
					if (SearchSmallWorld && RSSConfig.SearchSmallWorldEnabled) swSrchThread.Join();
					if (SearchMmp && RSSConfig.SearchMmpEnabled) mmpSrchThread.Join();

					if (Debug) DebugLogMessage(SearchId.ToString() + ". Main Search threads finished");
				}

				else // single threading for debug purposes
				{
					if (SearchFSS && RSSConfig.SearchFssEnabled)
						ExecuteFssSearchesThreadMethod();
					if (SearchSim && RSSConfig.SearchSimEnabled)
						ExecuteSimSearchesThreadMethod();
					if (SearchMmp && RSSConfig.SearchMmpEnabled)
						ExecuteMmpSearchThreadMethod();
					if (SearchSmallWorld && RSSConfig.SearchSmallWorldEnabled)
						ExecuteSmallWorldSearchThreadMethod();
				}

				if (Superseded)
				{
					if (Debug)
						DebugLogTimeMessage(SearchId.ToString() + ". Search superseded, Time: ", t0);
					return;
				}

				if (Debug) DebugLogMessage(SearchId.ToString() + ". Combining results");

// FSS matches

				CombineMatchLists(FssMatches, FssCidSet, CorpFssMatches, null);
				CombineMatchLists(FssMatches, FssCidSet, ChemblFssMatches, null);

// Substructure matches

				CombineMatchLists(SssMatches, SssMatchCidSet, CorpSssMatches, null);
				CombineMatchLists(SssMatches, SssMatchCidSet, ChemblSssMatches, null);

// Matched Pair hits

				CombineMatchLists(MmpMatches, MmpCidSet, CorpMmpMatches, null);
				CombineMatchLists(MmpMatches, MmpCidSet, ChemblMmpMatches, null);

// SmallWorld Matches - split results by source

				if (SmallWorldMatches == null) // be sure exists
					SmallWorldMatches = new List<StructSearchMatch>();

				foreach (StructSearchMatch sm in SmallWorldMatches)
				{
					if (Lex.Eq(sm.SrcCid, QueryCid)) continue; // don't add query Cid

					if (KeysToExclude != null && KeysToExclude.Contains(sm.SrcCid)) continue; // filter any exclusion list

					if (RestrictedDatabaseView.KeyIsRetricted(sm.SrcCid)) continue;

					if (sm.SrcDbId == StructSearchMatch.CorpDbId)
					{
						if (CorpSmallWorldMatches.Count < MaxHits)
							CorpSmallWorldMatches.Add(sm);
					}

					else if (sm.SrcDbId == StructSearchMatch.ChemblDbId)
					{
						if (ChemblSmallWorldMatches.Count < MaxHits)
							ChemblSmallWorldMatches.Add(sm);
					}

					else continue;
				}

				SmallWorldMatches = new List<StructSearchMatch>(); // rebuild sim hits with just those that were accepted
				SmallWorldCidSet = new HashSet<string>();

				CombineMatchLists(SmallWorldMatches, SmallWorldCidSet, CorpSmallWorldMatches, null);
				CombineMatchLists(SmallWorldMatches, SmallWorldCidSet, ChemblSmallWorldMatches, null);

// Similarity Matches - Split similarity results by source 

				if (SimMatches == null) // be sure exists
					SimMatches = new List<StructSearchMatch>();

				foreach (StructSearchMatch sm in SimMatches)
				{
					if (Lex.Eq(sm.SrcCid, QueryCid)) continue; // don't add query Cid

					if (KeysToExclude != null && KeysToExclude.Contains(sm.SrcCid)) continue; // filter any exclusion list

					if (RestrictedDatabaseView.KeyIsRetricted(sm.SrcCid)) continue;

					if (sm.SrcDbId == 0)
					{
						if (CorpSimMatches.Count < MaxHits)
							CorpSimMatches.Add(sm);
					}

					else if (sm.SrcDbId == 1)
					{
						if (ChemblSimMatches.Count < MaxHits)
							ChemblSimMatches.Add(sm);
					}

					else continue;
				}

				SimMatches = new List<StructSearchMatch>(); // rebuild SmallWorld hits with just those that were accepted
				SimCidSet = new HashSet<string>();

				CombineMatchLists(SimMatches, SimCidSet, CorpSimMatches, null);
				CombineMatchLists(SimMatches, SimCidSet, ChemblSimMatches, null);

				// Build full hit list

				AllMatches = new List<StructSearchMatch>();
				AllMatchesCidSet = new HashSet<string>();

				if (IncludeQueryStructure) // include the query structure in the results
				{
					StructSearchMatch ssm = new StructSearchMatch();
					ssm.SearchType = StructureSearchType.FullStructure;
					ssm.SearchSubtype = 0; // Query structure
					ssm.SrcDbId = StructSearchMatch.SrcNameToId(QueryMtName);
					ssm.SrcCid = CompoundId.Normalize(QueryCid); // source compound id
					ssm.MolString = QueryChimeString;
					ssm.MolStringFormat = MoleculeFormat.Chime;
					ssm.MatchScore = 1.0f;

					AllMatches.Add(ssm);
					AllMatchesCidSet.Add(ssm.SrcCid);
				}

				CombineMatchLists(AllMatches, AllMatchesCidSet, FssMatches, null);
				CombineMatchLists(AllMatches, AllMatchesCidSet, SssMatches, null);
				CombineMatchLists(AllMatches, AllMatchesCidSet, MmpMatches, null);
				CombineMatchLists(AllMatches, AllMatchesCidSet, SmallWorldMatches, null);
				CombineMatchLists(AllMatches, AllMatchesCidSet, SimMatches, null);

				// Build match list for each database

				CorpMatches = new List<StructSearchMatch>();
				ChemblMatches = new List<StructSearchMatch>();

				foreach (StructSearchMatch sm in AllMatches)
				{
					if (sm.SrcDbId == StructSearchMatch.CorpDbId)
					{
						CorpMatches.Add(sm);
						CorpMatchCidSet.Add(sm.SrcCid);
					}

					else if (sm.SrcDbId == StructSearchMatch.ChemblDbId)
					{
						ChemblMatches.Add(sm);
						ChemblMatchCidSet.Add(sm.SrcCid);
					}
				}

				SearchHistory.Add(this); // add results to the history list

				if (Debug)
					DebugLogTimeMessage(SearchId.ToString() + ". Search complete for CorpId: " + QueryCid + ", Count: " + AllMatches.Count + ", Time: ", t0);
				return;
			}

			catch (Exception ex)
			{
				DebugLogMessage(ex);
				//throw new Exception(ex.Message, ex); // not caught
			}
		}

		/// <summary>
		/// ExecuteFssSearchesThreadMethod
		/// </summary>

		void ExecuteFssSearchesThreadMethod()
		{
			string mtName;

			DateTime t0 = DateTime.Now;

			try
			{
				if (Debug) DebugLogMessage(SearchId.ToString() + ". Starting FSS searches");

				FssSubsearches = new Dictionary<string, Subsearch>();

				mtName = "rss_corp_fss"; // start corp db searches
				StartFssSearch("CorpExact", FullStructureSearchType.Exact, mtName);
				StartFssSearch("CorpFragment", FullStructureSearchType.Fragment, mtName);
				StartFssSearch("CorpIsomer", FullStructureSearchType.Isomer, mtName);
				StartFssSearch("CorpTautomer", FullStructureSearchType.Tautomer, mtName);

				mtName = "rss_chembl_fss"; // start chembl db searches
				StartFssSearch("ChemblExact", FullStructureSearchType.Exact, mtName);
				StartFssSearch("ChemblFragment", FullStructureSearchType.Fragment, mtName);
				StartFssSearch("ChemblIsomer", FullStructureSearchType.Isomer, mtName);
				StartFssSearch("ChemblTautomer", FullStructureSearchType.Tautomer, mtName);

				if (UseMultipleThreads)
				{
					foreach (Subsearch ss in FssSubsearches.Values)
					{
						ss.Thread.Join(); // wait for completion
						ss.Thread = null;
					}
				}

				if (Superseded)
				{
					if (Debug)
						DebugLogTimeMessage(SearchId.ToString() + ". FSS search superseded, Time: ", t0);
					return;
				}

				// Combine match results using just one result per Cid

				CorpFssMatches = new List<StructSearchMatch>();
				CorpFssCidSet = new HashSet<string>();
				CombineMatchLists(CorpFssMatches, CorpFssCidSet, FssSubsearches["CorpExact"].Matches, null);
				CombineMatchLists(CorpFssMatches, CorpFssCidSet, FssSubsearches["CorpFragment"].Matches, CorpFssCidSet);
				CombineMatchLists(CorpFssMatches, CorpFssCidSet, FssSubsearches["CorpIsomer"].Matches, CorpFssCidSet);
				CombineMatchLists(CorpFssMatches, CorpFssCidSet, FssSubsearches["CorpTautomer"].Matches, CorpFssCidSet);

				ChemblFssMatches = new List<StructSearchMatch>();
				ChemblFssCidSet = new HashSet<string>();
				CombineMatchLists(ChemblFssMatches, ChemblFssCidSet, FssSubsearches["ChemblExact"].Matches, null);
				CombineMatchLists(ChemblFssMatches, ChemblFssCidSet, FssSubsearches["ChemblFragment"].Matches, ChemblFssCidSet);
				CombineMatchLists(ChemblFssMatches, ChemblFssCidSet, FssSubsearches["ChemblIsomer"].Matches, ChemblFssCidSet);
				CombineMatchLists(ChemblFssMatches, ChemblFssCidSet, FssSubsearches["ChemblTautomer"].Matches, ChemblFssCidSet);

				if (Debug)
					DebugLogTimeMessage(SearchId.ToString() + ". FSS searches (All), Count: " + (CorpFssMatches.Count + ChemblFssMatches.Count).ToString() + ", Time: ", t0);

				return;
			}

			catch (Exception ex) // log message for thread since can't throw exception up the stack
			{
				CorpFssMatches = ChemblFssMatches = null; // indicate failure
				FssSearchFailed = true;
				DebugLogMessage(ex);
			}
		}

		void ExecuteSssSearchesThreadMethod()
		{
			string mtName;

			DateTime t0 = DateTime.Now;

			try
			{
				if (Debug) DebugLogMessage(SearchId.ToString() + ". Starting substructure searches");

				mtName = "rss_corp_sss"; // start corp db search
				Thread corpThread = StartSssSearch(mtName);

				mtName = "rss_chembl_sss"; // start chembl db search
				Thread chemblThread = StartSssSearch(mtName);

				if (UseMultipleThreads)
				{
					corpThread.Join(); // wait for completion
					chemblThread.Join(); 
				}

				if (Superseded)
				{
					if (Debug)
						DebugLogTimeMessage(SearchId.ToString() + ". FSS search superseded, Time: ", t0);
					return;
				}

				if (Debug)
					DebugLogTimeMessage(SearchId.ToString() + ". Sssmatch searches (All), Count: " + (CorpSssMatches.Count + ChemblSssMatches.Count).ToString() + ", Time: ", t0);

				return;
			}

			catch (Exception ex) // log message for thread since can't throw exception up the stack
			{
				CorpSssMatches = ChemblSssMatches = null; // indicate failure
				SssSearchFailed = true;
				DebugLogMessage(ex);
			}
		}


		/// <summary>
		/// Combine Match Lists
		/// </summary>
		/// <param name="destMatchList"></param>
		/// <param name="destMatchDict"></param>
		/// <param name="addList"></param>
		/// <param name="exclusionCidSet"></param>

		void CombineMatchLists(
			List<StructSearchMatch> destMatchList,
			HashSet<string> destMatchDict,
			List<StructSearchMatch> addList,
			HashSet<string> exclusionCidSet)
		{
			if (addList == null) return;

			foreach (StructSearchMatch ssm0 in addList)
			{
				string key = ssm0.SrcCid; // qualify with dbId later...

				if (Lex.Eq(key, QueryCid)) continue; // don't include query Cid

				if (exclusionCidSet != null && exclusionCidSet.Contains(key)) continue;

				if (destMatchList != null) destMatchList.Add(ssm0);
				if (destMatchDict != null && !destMatchDict.Contains(key)) // add if not included yet
					destMatchDict.Add(key); // store key
			}

			return;
		}

		/// <summary>
		/// StartFssSearch
		/// </summary>
		/// <param name="searchName"></param>
		/// <param name="flags"></param>
		/// <param name="metaTable"></param>

		void StartFssSearch(
			string searchName,
			string flags,
			string mtName)
		{
			try
			{
				Subsearch ss = new Subsearch();
				ss.Name = searchName;
				ss.FssFlags = flags;
				ss.MtName = mtName;
				ss.Matches = new List<StructSearchMatch>();
				FssSubsearches[searchName] = ss;

				if (UseMultipleThreads)
				{
					if (Debug) DebugLogMessage(SearchId.ToString() + ". Starting Fss " + searchName + " thread");

					Thread t = new Thread(new ParameterizedThreadStart(FssMatchSearchThreadMethod));
					t.Name = "FssSearchThreadMethod";
					ss.Thread = t;
					t.IsBackground = true;
					t.SetApartmentState(ApartmentState.STA);
					t.Start(ss);
				}

				else FssMatchSearchThreadMethod(ss);

				return;
			}
			catch (Exception ex)
			{
				DebugLogMessage(ex); // just log message & exit
			}

		}

/// <summary>
/// FssSearchThreadMethod
/// </summary>
/// <param name="parm"></param>

		void FssMatchSearchThreadMethod(
			object parm)
		{
			string queryChime, sql;

			int srcId = 0, searchSubType = 0;

			bool dbCommandMxLogExceptions = DbCommandMx.LogExceptions; // turn off low level database exception handling
			DbCommandMx.LogExceptions = false;

			try
			{

				DateTime tLock = DateTime.Now;
				Subsearch ss = parm as Subsearch;

				{
					double lockTime = TimeOfDay.Delta(tLock);

					DateTime t0 = DateTime.Now;

					if (Debug) DebugLogMessage(SearchId.ToString() + ". Starting FSS " + ss.Name); // + " search, lock wait time: " + TimeOfDay.Delta(tLock));

					if (SearchId >= 0 && SearchId < MaxSearchIdReceived) // if a later request has occured, don't need to do this search
					{
						this.Superseded = true; // flag this search as aborted
						if (Debug) DebugLogMessage(SearchId.ToString() + ". FSS " + ss.Name + " search superseded by search " + MaxSearchIdReceived);
						return;
					}

					if (Lex.Contains(ss.MtName, "corp"))
					{
						srcId = StructSearchMatch.CorpDbId;
						if (!SearchCorp || !RSSConfig.SearchCorpDbFssEnabled) return;
					}

					else if (Lex.Contains(ss.MtName, "chembl"))
					{
						srcId = StructSearchMatch.ChemblDbId;
						if (!SearchChembl || !RSSConfig.SearchChemblFssEnabled) return;
					}

					else throw new Exception("Invalid source: " + ss.MtName);

					if (Lex.Contains(ss.Name, "exact"))
					{
						searchSubType = 1;
						queryChime = QueryChimeString;
					}

					else
					{
						queryChime = LargestFragmentChimeString;

						if (Lex.Contains(ss.Name, "fragment"))
							searchSubType = 2;

						else if (Lex.Contains(ss.Name, "isomer"))
							searchSubType = 3;

						else if (Lex.Contains(ss.Name, "tautomer"))
							searchSubType = 4;

						else DebugLogMessage("Unrecognized FSS subtype: " + ss.Name);
					}

					MetaTable mt = MetaTableCollection.GetWithException(ss.MtName);
					sql = mt.GetTableMapWithEnclosingParensRemoved();
					queryChime = MqlUtil.ConvertStringToValidOracleExpression(queryChime); // properaly handle chime where length > 4000 

					sql = sql.Replace("':1'", ":1"); // remove single quotes in old metatable sql (remove when metatable updated)
					sql = sql.Replace(":1", queryChime);

					sql = sql.Replace("':2'", ":2"); // remove single quotes in old metatable sql (remove when metatable updated)
					sql = sql.Replace(":2", Lex.AddSingleQuotes(ss.FssFlags));

					DbCommandMx cmd = new DbCommandMx();

					cmd.Prepare(sql);

					cmd.ExecuteReader();

					ss.Matches = new List<StructSearchMatch>();

					while (cmd.Read())
					{
						if (cmd.IsNull(0)) continue;
						string cid = cmd.GetObject(0).ToString();
						cid = CompoundId.NormalizeForDatabase(cid); // be sure CorpIds are proper length
						if (Lex.Eq(cid, QueryCid)) continue; // don't add query Cid

						if (RestrictedDatabaseView.KeyIsRetricted(cid)) continue;

						StructSearchMatch sm = new StructSearchMatch();
						sm.SearchType = StructureSearchType.FullStructure;
						sm.SearchSubtype = searchSubType;
						sm.SrcDbId = srcId;
						sm.SrcCid = cid;
						sm.MatchScore = 1.0f;
						ss.Matches.Add(sm);
					}

					cmd.CloseReader();
					cmd.Dispose();

					if (Debug)
						DebugLogTimeMessage(SearchId.ToString() + ". Completed FSS search: " + ss.Name + ", Hit count: " + ss.Matches.Count + ", Time: ", t0);

					return;
				} // end of locked block
			}

			catch (Exception ex)
			{
				DebugLogMessage(ex); // just log message & exit
			}

			finally
			{
				DbCommandMx.LogExceptions = dbCommandMxLogExceptions;
			}
		}

		/// <summary>
		/// StartSssSearch
		/// </summary>
		/// <param name="metaTable"></param>

		Thread StartSssSearch(
			string mtName)
		{
			try
			{

				if (UseMultipleThreads)
				{
					if (Debug) DebugLogMessage(SearchId.ToString() + ". Starting SSS " + mtName + " thread");

					Thread t = new Thread(new ParameterizedThreadStart(SssSearchThreadMethod));
					t.Name = "ExecuteSssSearchThreadMethod " + mtName;
					t.IsBackground = true;
					t.SetApartmentState(ApartmentState.STA);
					t.Start(mtName);
					return t;
				}

				else
				{
					SssSearchThreadMethod(mtName);
					return null;
				}
			}
			catch (Exception ex)
			{
				DebugLogMessage(ex); // just log message & exit
				return null;
			}

		}

		/// <summary>
		/// SssSearchThreadMethod
		/// </summary>

		void SssSearchThreadMethod(object parm)
		{
			string queryChime, sql;

			int srcId = 0, searchSubType = 0;
			List<StructSearchMatch> matchList = null;

			bool dbCommandMxLogExceptions = DbCommandMx.LogExceptions; // turn off low level database exception handling
			DbCommandMx.LogExceptions = false;

			try
			{

				DateTime tLock = DateTime.Now;
				string mtName = parm as string;

				//lock (SssMatchLocks[ss.Name]) // need lock these to serialize access?
				{
					double lockTime = TimeOfDay.Delta(tLock);

					DateTime t0 = DateTime.Now;

					if (Debug) DebugLogMessage(SearchId.ToString() + ". Starting Substructure search " + mtName); // + " search, lock wait time: " + TimeOfDay.Delta(tLock));

					if (SearchId >= 0 && SearchId < MaxSearchIdReceived) // if a later request has occured, don't need to do this search
					{
						this.Superseded = true; // flag this search as aborted
						if (Debug) DebugLogMessage(SearchId.ToString() + ". Substructure search superseded by search " + MaxSearchIdReceived);
						return;
					}

					if (Lex.Contains(mtName, "corp"))
					{
						if (!SearchCorp || !RSSConfig.SearchCorpSssEnabled) return;

						srcId = StructSearchMatch.CorpDbId;
						matchList = CorpSssMatches = new List<StructSearchMatch>();
					}

					else if (Lex.Contains(mtName, "chembl"))
					{
						if (!SearchChembl || !RSSConfig.SearchChemblSssEnabled) return;

						srcId = StructSearchMatch.ChemblDbId;
						matchList = ChemblSssMatches = new List<StructSearchMatch>();
					}

					else throw new Exception("Invalid source: " + mtName);

					MetaTable mt = MetaTableCollection.GetWithException(mtName);
					sql = mt.GetTableMapWithEnclosingParensRemoved();
					queryChime = MqlUtil.ConvertStringToValidOracleExpression(QueryChimeString); // properaly handle chime where length > 4000 

					sql = sql.Replace("':1'", ":1"); // remove single quotes in old metatable sql (remove when metatable updated)
					sql = sql.Replace(":1", queryChime);

					DbCommandMx cmd = new DbCommandMx();

					//if (Debug) DebugLogMessage(SearchId.ToString() + ". SSS Prepare");
					cmd.Prepare(sql);

					//if (Debug) DebugLogMessage(SearchId.ToString() + ". SSS ExecuteReader");
					cmd.ExecuteReader();

					//if (Debug) DebugLogMessage(SearchId.ToString() + ". SSS Reading");

					while (cmd.Read())
					{
						if (cmd.IsNull(0)) continue;
						string cid = cmd.GetObject(0).ToString();
						cid = CompoundId.NormalizeForDatabase(cid); // be sure CorpIds are proper length
						if (Lex.Eq(cid, QueryCid)) continue; // don't add query Cid

						if (RestrictedDatabaseView.KeyIsRetricted(cid)) continue;

						StructSearchMatch sm = new StructSearchMatch();
						sm.SearchType = StructureSearchType.Substructure;
						sm.SearchSubtype = searchSubType;
						sm.SrcDbId = srcId;
						sm.SrcCid = cid;
						sm.MatchScore = 1.0f;
						matchList.Add(sm);
					}

					//if (Debug) DebugLogMessage(SearchId.ToString() + ". SSS Closing");
					cmd.CloseReader();
					cmd.Dispose();

					if (Debug)
						DebugLogTimeMessage(SearchId.ToString() + ". Completed substructure search: " + mtName + ", Hit count: " + matchList.Count + ", Time: ", t0);

					return;
				} // end of locked block
			}

			catch (Exception ex)
			{
				DebugLogMessage(ex); // just log message & exit
			}

			finally
			{
				DbCommandMx.LogExceptions = dbCommandMxLogExceptions;
			}
		}


		/// <summary>
		/// ExecuteSimSearchThreadMethod
		/// </summary>

		void ExecuteSimSearchesThreadMethod()
		{
			try
			{
				DateTime t0 = DateTime.Now;

				if (Debug) DebugLogMessage(SearchId.ToString() + ". Starting similarity searches");

				if (FragmentList.Count == 0)
				{
					SimMatches = new List<StructSearchMatch>();
					return;
				}

				string smilesQuery = FragmentList[0].Key; // just do largest structure
				IAtomContainer queryMol = CdkUtil.SmilesToAtomContainer(smilesQuery);

				CdkSimSearchMx simSrch = new CdkSimSearchMx();
				simSrch.KeysToExclude = KeysToExclude;
				simSrch.GetCorpSim = (SearchCorp && RSSConfig.SearchCorpSimEnabled);
				simSrch.GetChemblSim = (SearchChembl && RSSConfig.SearchChemblSimEnabled);
				simSrch.FingerprintType = FingerprintType.Circular;
				SimMatches = simSrch.ExecuteSearch(queryMol);

				if (Debug)
					DebugLogTimeMessage(SearchId.ToString() + ". Completed Similarity Searches, Count: " + SimMatches.Count + ", Time: ", t0);

				return;
			}

			catch (Exception ex) // log message for thread since can't throw exception up the stack
			{
				SimMatches = null; // indicate failure
				SimSearchFailed = true;
				DebugLogMessage(ex);
			}

		}

		/// <summary>
		/// ExecuteMmpSearchesThreadMethod
		/// </summary>

		void ExecuteMmpSearchThreadMethod()
		{
			try
			{
				StructSearchMatch sm, sm2;
				DateTime tLock = DateTime.Now;

				lock (NetezzaOdbcLock) // must serialize access to Netezza ODBC driver since multithreading on a connection not supported
				{
					double lockTime = TimeOfDay.Delta(tLock);

					DateTime t0 = DateTime.Now;

					// Cutoff for acceptable pair matches.
					// - Mean context ratio: (cont_ratio_l + cont_ratio_r) / 2.0
					// - Alternatively, inverse mean frag ratio: 1.0 - (frag_ratio_l + frag_ratio_r) / 2.0

					float contextRatioCutoff = 0.80F;

					if (Debug) DebugLogMessage(SearchId.ToString() + ". Starting MMP search, lock wait time: " + TimeOfDay.Delta(tLock));

					CorpMmpMatches = new List<StructSearchMatch>();
					Dictionary<string, StructSearchMatch> mmpMatchDict = new Dictionary<string, StructSearchMatch>();

					if (FragmentList.Count == 0) return; // nothing to search

					if (SearchId >= 0 && SearchId >= 0 && SearchId < MaxSearchIdReceived) // if a later request has occured, don't need to do this search
					{
						this.Superseded = true; // flag this search as aborted
						if (Debug) DebugLogMessage(SearchId.ToString() + ". MMP search superseded by search " + MaxSearchIdReceived);
						return;
					}

					MetaTable mt = MetaTableCollection.GetWithException("rss_corp_mmp");
					string sql = mt.GetTableMapWithEnclosingParensRemoved();
					sql = sql.Replace(":1", QueryCid);
					sql = sql.Replace(":2", contextRatioCutoff.ToString());

					DbCommandMx cmd = new DbCommandMx();

					//if (Debug) DebugLogMessage(SearchId.ToString() + ". MMP Prepare");
					cmd.Prepare(sql);

					//if (Debug) DebugLogMessage(SearchId.ToString() + ". MMP ExecuteReader");
					cmd.ExecuteReader();

					//if (Debug) DebugLogMessage(SearchId.ToString() + ". MMP Reading");
					while (cmd.Read())
					{
						sm = new StructSearchMatch();
						int molIdL = cmd.GetInt(0);
						int molIdR = cmd.GetInt(1);

						if (RestrictedDatabaseView.KeyIsRetricted(molIdL.ToString())) continue;
						if (RestrictedDatabaseView.KeyIsRetricted(molIdR.ToString())) continue;

						float pairScore = (float)cmd.GetDouble(2);
						string structSmiR = cmd.GetString(3);

						if (RSSConfig.KeepHighestCidPairScoreOnlyEnabled) // just keep the highest score per pair and ignore the rest
						{
							string cidKey = molIdL.ToString() + "." + molIdR;

							if (mmpMatchDict.ContainsKey(cidKey)) // already seen this key combo?
							{
								sm2 = mmpMatchDict[cidKey];
								if (pairScore > sm2.MatchScore) // input should be sorted by high to low score but check just in case
									sm2.MatchScore = pairScore;
								continue;
							}

							else mmpMatchDict[cidKey] = sm; // new key pair
						}

						sm.SearchSubtype = molIdL;
						sm.SrcDbId = StructSearchMatch.SrcNameToId("Corp");
						sm.SrcCid = CompoundId.NormalizeForDatabase(molIdR.ToString()); // be sure CorpIds are proper length

						sm.SearchType = StructureSearchType.MatchedPairs;
						sm.MatchScore = pairScore;

						sm.MolString = structSmiR;
						sm.MolStringFormat = MoleculeFormat.Smiles; // smiles (in 3 pieces)

						string molfile = CdkUtil.IntegrateAndHilightMmpStructure(sm.MolString);
						string chime = MoleculeMx.MolFileToChimeString(molfile);
						sm.MolString = chime;
						sm.MolStringFormat = MoleculeFormat.Chime;

						CorpMmpMatches.Add(sm);
						if (CorpMmpMatches.Count > MaxHits) break;
					}

					//if (Debug) DebugLogMessage(SearchId.ToString() + ". MMP Closing");
					cmd.CloseReader();
					cmd.Dispose();

					CorpMmpMatches.Sort(StructSearchMatch.CompareByMatchQuality);

					if (Debug) DebugLogTimeMessage(SearchId.ToString() + ". Completed MMP Search, Count: " + CorpMmpMatches.Count + ", Time: ", t0);

					return;
				} // end of locked block
			}
			catch (Exception ex) // log message for thread since can't throw exception up the stack
			{
				CorpMmpMatches = null; // indicate failed
				MmpSearchFailed = true;
				DebugLogMessage(ex);
			}

		}

		/// <summary>
		/// ExecuteSmallWorldSearcheThreadMethod
		/// </summary>

		void ExecuteSmallWorldSearchThreadMethod()
		{
			try
			{
				StructSearchMatch sm, sm2;
				DateTime tLock = DateTime.Now;

				DateTime t0 = DateTime.Now;

				if (FragmentList.Count == 0)
				{
					SmallWorldMatches = new List<StructSearchMatch>();
					return;
				}

				string smilesQuery = FragmentList[0].Key; // just do largest structure

				SmallWorldPredefinedParameters swp = new SmallWorldPredefinedParameters(); // get default parameters
				swp.Smiles = smilesQuery;

				swp.Database = "";
				if (SearchSmallWorld && RSSConfig.SearchCorpDbSmallWorldEnabled) Lex.AppendItemToString(ref swp.Database, ",", "Corp");
				if (SearchChembl && RSSConfig.SearchChemblSmallWorldEnabled) Lex.AppendItemToString(ref swp.Database, ",", "ChEMBL");

				SmallWorldDao swDao = new SmallWorldDao();
				swDao.KeysToExclude = KeysToExclude;
				List<SmallWorldMatch> swMatches = swDao.ExecuteSearch(swp);

				SmallWorldMatches = new List<StructSearchMatch>(); // init match list

				if (SearchId >= 0 && SearchId < MaxSearchIdReceived) // if a later request has occured, don't need to do this search
				{
					this.Superseded = true; // flag this search as aborted
					if (Debug) DebugLogMessage(SearchId.ToString() + ". SmallWorld search superseded by search " + MaxSearchIdReceived);
					return;
				}

				if (swMatches == null || swMatches.Count == 0) return;

				if (swp.Highlight || swp.Align) // get depictions for these
					swDao.GetDepictions(swp.Highlight, swp.Align);

				foreach (SmallWorldMatch match in swMatches)
				{
					sm = new StructSearchMatch();

					sm.SrcCid = CompoundId.NormalizeForDatabase(match.Cid); // be sure CorpIds are proper length

					if (RestrictedDatabaseView.KeyIsRetricted(sm.SrcCid)) continue;

					sm.SrcDbId = StructSearchMatch.SrcNameToId(match.Database);
					sm.SearchType = StructureSearchType.SmallWorld;
					sm.MatchScore = match.SimScore;
					sm.MolString = match.HitSmiles;
					sm.MolStringFormat = MoleculeFormat.Smiles;
					if (swp.Highlight || swp.Align) // get depiction
						sm.GraphicsString = SmallWorldDao.GetDepiction(match, swp.Highlight, swp.Align);

					SmallWorldMatches.Add(sm);
					if (SmallWorldMatches.Count > MaxHits) break;
				}

				//if (Debug) DebugLogMessage(SearchId.ToString() + ". SmallWorld Closing");

				SmallWorldMatches.Sort(StructSearchMatch.CompareByMatchQuality);

				if (Debug) DebugLogTimeMessage(SearchId.ToString() + ". Completed SmallWorld Search, Count: " + SmallWorldMatches.Count + ", Time: ", t0);

				return;
			}

			catch (Exception ex) // log message for thread since can't throw exception up the stack
			{
				SmallWorldMatches = null; // indicate failed
				SmallWorldSearchFailed = true;
				DebugLogMessage(ex);
			}
		}

		/// <summary>
		/// Get the structures we don't already have (i.e. FSS & Sim search)
		/// </summary>

		void RetrieveRelatedStructures()
		{
			StartRetrieveRelatedStructures();
			WaitForRetrieveStructuresCompletion();
			return;
		}

		void StartRetrieveRelatedStructures()
		{
			if (StructureRetrievalStarted) return; // just return if already started

			List<StructSearchMatch> corpList = new List<StructSearchMatch>();
			List<StructSearchMatch> chemblList = new List<StructSearchMatch>();

			foreach (StructSearchMatch ssm in AllMatches) // get cid and result type subset
			{
				if ((ssm.SearchType == StructureSearchType.FullStructure || ssm.SearchType == StructureSearchType.MolSim))
				{
					if (ssm.SrcDbId == StructSearchMatch.CorpDbId)
						corpList.Add(ssm);

					else if (ssm.SrcDbId == StructSearchMatch.ChemblDbId)
						chemblList.Add(ssm);

					else continue;
				}

				else continue; // don't need to include Matched Pairs or SmallWorld since already have structures
			}

			CorpRetrieveStructuresThread = StartRetrieveStructuresThread("RSS_CORP_STRUCTURES", corpList);
			ChemblRetrieveStructuresThread = StartRetrieveStructuresThread("RSS_CHEMBL_STRUCTURES", chemblList);

			StructureRetrievalStarted = true;
			return;
		}


		/// <summary>
		/// Start retrieval thread
		/// </summary>
		/// <param name="mtName"></param>
		/// <param name="matchList"></param>
		/// <returns></returns>

		Thread StartRetrieveStructuresThread(
			string mtName,
			List<StructSearchMatch> matchList)
		{
			Thread t = null;

			object[] parms = new object[2];
			parms[0] = mtName;
			parms[1] = matchList;

			if (UseMultipleThreads)
			{
				if (Debug) DebugLogMessage(SearchId.ToString() + ". Starting RetrieveStructures " + mtName + " thread");

				t = new Thread(new ParameterizedThreadStart(RetrieveStructuresThreadMethod));
				t.Name = "RetrieveStructuresThreadMethod";
				t.IsBackground = true;
				t.SetApartmentState(ApartmentState.STA);
				t.Start(parms);
				return t;
			}

			else
			{
				RetrieveStructuresThreadMethod(parms);
				return null;
			}
		}

		void RetrieveStructuresThreadMethod(
			object parm)
		{
			try
			{
				DateTime t0 = DateTime.Now;

				object[] parms = (object[])parm;
				string mtName = parms[0] as string;
				List<StructSearchMatch> matchList = parms[1] as List<StructSearchMatch>;

				if (Debug) DebugLogMessage(SearchId.ToString() + ". Starting RetrieveStructures " + mtName);

				HashSet<string> cidSubset = new HashSet<string>(); // get the set of cids
				foreach (StructSearchMatch ssm in matchList) // get cid and result type subset
				{
					cidSubset.Add(ssm.SrcCid);
				}
				List<string> cidList = new List<string>(cidSubset); // convert to list

				if (Lex.Contains(mtName, "corp"))
					RetrieveCorpStructures(cidList, matchList);

				else if (Lex.Contains(mtName, "chembl"))
					RetrieveChemblStructures(cidList, matchList);

				if (Debug) DebugLogTimeMessage(SearchId.ToString() + ". Finished retrieve structures, Table: " + mtName + ", Count: " + matchList.Count + ", Time: ", t0);

				return;
			}
			catch (Exception ex)
			{
				DebugLogMessage(ex); // just log message & exit
			}

		}

/// <summary>
/// Retrieve CorpStructures
/// </summary>
/// <param name="matchList"></param>

		void RetrieveCorpStructures(
			List<string> cidList,
			List<StructSearchMatch> matchList)
		{
			CompactMolecule cm;
			StructSearchMatch ssmNotFound = null;

			if (cidList.Count == 0) return;

			MetaTable mt = MetaTableCollection.GetWithException("rss_corp_structures");
			string sql = mt.GetTableMapWithEnclosingParensRemoved();
			sql = sql.Replace(":1", "<list>");

			DbCommandMx cmd = new DbCommandMx();
			cmd.PrepareListReader(sql, DbType.Int32, KeyListPredTypeEnum.Literal);
			cmd.ExecuteListReader(cidList);

			Dictionary<string, CompactMolecule> structDict = new Dictionary<string, CompactMolecule>();
			while (cmd.Read())
			{
				int cidInt = cmd.GetInt(0);
				//if (cidInt == 3439060 || cidInt == 3439163) cidInt = cidInt; // debug
				string cid = CompoundId.NormalizeForDatabase(cidInt.ToString());
				if (RestrictedDatabaseView.KeyIsRetricted(cid)) continue;
				string molString = cmd.GetClob(1);
				if (HelmConverter.IsHelmString(molString)) // do quick check for Helm
					cm = new CompactMolecule(MoleculeFormat.Helm, molString);

				else cm = new CompactMolecule(MoleculeFormat.Chime, molString); // assume chime otherwise

				structDict[cid] = cm;
			}

			cmd.CloseReader();

			foreach (StructSearchMatch ssm in matchList)
			{
				if (structDict.ContainsKey(ssm.SrcCid))
				{
					cm = structDict[ssm.SrcCid];
					ssm.MolString = cm.Value;
					ssm.MolStringFormat = cm.Format;
				}

				else ssmNotFound = ssm; // shouldn't happen
			}

			return;
		}

		/// <summary>
		/// Retrieve ChemblStructures
		/// </summary>
		/// <param name="matchList"></param>

		void RetrieveChemblStructures(
			List<string> cidList,
			List<StructSearchMatch> matchList)
		{
			StructSearchMatch ssmNotFound = null;
			if (cidList.Count == 0) return;

			MetaTable mt = MetaTableCollection.GetWithException("rss_chembl_structures");
			string sql = mt.GetTableMapWithEnclosingParensRemoved();
			sql = sql.Replace(":1", "<list>");

			DbCommandMx cmd = new DbCommandMx();
			cmd.PrepareListReader(sql, DbType.String, KeyListPredTypeEnum.Literal);
			cmd.ExecuteListReader(cidList);

			Dictionary<string, string> structDict = new Dictionary<string, string>();
			while (cmd.Read())
			{
				string cid = cmd.GetString(0);
				string smiles = cmd.GetString(1);
				structDict[cid] = smiles;
			}
			cmd.CloseReader();

			foreach (StructSearchMatch ssm in matchList)
			{
				if (structDict.ContainsKey(ssm.SrcCid))
				{
					ssm.MolString = structDict[ssm.SrcCid];
					ssm.MolStringFormat = MoleculeFormat.Smiles;
				}

				else ssmNotFound = ssm; // shouldn't happen
			}

			return;
		}

		/// <summary>
		/// WaitForRetrieveRelatedStructuresCompletion
		/// </summary>

		void WaitForRetrieveStructuresCompletion()
		{
			if (UseMultipleThreads)
			{

				//lock (StructureRetrievalWaitLock) // need to lock this
				{
					if (StructureRetrievalCompleted) return;
					if (!StructureRetrievalStarted) throw new Exception("Structure Retrieval Not Started");

					CorpRetrieveStructuresThread.Join(); // wait for Corp db completion
					CorpRetrieveStructuresThread = null;

					ChemblRetrieveStructuresThread.Join(); // wait for ChEMBL db completion
					ChemblRetrieveStructuresThread = null;

					StructureRetrievalCompleted = true;
					StructureRetrievalStarted = false;
				}
			}

			AllMatches.Sort(StructSearchMatch.CompareByMatchQuality);
			return;
		}

		/// <summary>
		/// Allow logging to RelatedStructureSearch-specific file
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="time"></param>
		public static void DebugLogTimeMessage(string msg, DateTime time)
		{
			DebugLog.TimeMessage(msg, time, LogFileName);
		}

		/// <summary>
		/// Allow logging to RelatedStructureSearch-specific file
		/// </summary>
		/// <param name="ex"></param>

		public static void DebugLogMessage(Exception ex)
		{
			DebugLog.Message(ex, LogFileName);
		}

		/// <summary>
		/// Allow logging to RelatedStructureSearch-specific file
		/// </summary>
		/// <param name="msg"></param>

		public static void DebugLogMessage(string msg)
		{
			DebugLog.Message(msg, LogFileName);
		}

	}

	/// <summary>
	/// Subsearch
	/// </summary>

	internal class Subsearch
	{
		public string Name = ""; // subsearch identifier
		public string FssFlags = "";
		public string MtName = ""; // identifies database
		public Thread Thread = null; // thread this search is running on
		public List<StructSearchMatch> Matches = null; // search results
	}

}
