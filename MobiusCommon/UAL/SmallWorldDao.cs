using Mobius.ComOps;
using Mobius.Data;
using Mobius.CdkMx;

using org.openscience.cdk;
using org.openscience.cdk.interfaces;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Security;
using System.Diagnostics;
using System.Runtime.Serialization.Json;

namespace Mobius.UAL
{

	/// <summary>
	/// Access to NextMove's SmallWorld Graph Edit Distance Search functionality
	/// </summary>

	public partial class SmallWorldDao
	{
		internal int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		public List<RootTable> DbRootTableList = null; // list of root tables to search
		Dictionary<string, List<SmallWorldMatch>> DbMatchLists = null; // Dict of matches for each database keyed by dbName
		public List<SmallWorldMatch> MatchList = null; // final merged match list

		public SmallWorldPredefinedParameters Swp = null; // Search parameters
		public SmallWorldMatchColumnMap ColumnMap = null;
		public HashSet<string> KeysToExclude = null; // keys to be excluded from search results

		double SearchMapsTime = 0;
		double SearchSubmitTime = 0;
		double SearchViewTime = 0;
		double SearchExportTime = 0;

		static bool UseMultipleThreads = true;
		Exception ThreadException = null; // any exception seen within a thread stored here
		CountdownEvent CountDownLatch = null;

		static StreamWriter CwHit, CwHitCorpId, CwMiss, CwMissCorpId, CwTooBig, CwEx; // check database writers

		public static string SmallWorldUrl = null;
		public static bool SmallWorldAvailable = false;
		public static bool SmallWorldDepictionsAvailable = false;

		public static int SvgDepictionCount = 0; // number of GetDepiction calls for the current query

		static IMolLib MolLibUtil => StaticMolLib.I; // static molecule shortcut for utility methods

		internal static bool Debug = false;

		/// <summary>
		/// Basic constructor
		/// </summary>

		public SmallWorldDao()
		{
			return;
		}

		/// <summary>
		/// InitializeForSession
		/// </summary>

		public static void InitializeForSession()
		{
			SmallWorldUrl = ServicesIniFile.IniFile.Read("SmallWorldUrl", "http://[server]:8081");
			SmallWorldAvailable = ServicesIniFile.IniFile.ReadBool("SmallWorldAvailable", false); // check inifile setting
			SmallWorldDepictionsAvailable = ServicesIniFile.IniFile.ReadBool("SmallWorldDepictionsAvailable", true);
			UseMultipleThreads = ServicesIniFile.ReadBool("SmallWorldUseMultipleThreads", true);
			Debug = ServicesIniFile.IniFile.ReadBool("SmallWorldDebug", false);

			return;
		}

		/// <summary>
		/// Execute a search and cache the results for later retrieval
		/// </summary>
		/// <param name="swp"></param>

		public List<SmallWorldMatch> ExecuteSearch(
			SmallWorldPredefinedParameters swp)
		{
			Swp = swp;

			string dbs = swp.Database;
			if (Lex.IsUndefined(dbs)) dbs = "Corp"; // default

			string[] dbNameList = Lex.Split(dbs, ",");  // list of external database names to search
			DbRootTableList = new List<RootTable>(); // get db info for each

			for (int dbi = 0; dbi < dbNameList.Length; dbi++) // replace any external database names with internal names
			{
				string dbName = dbNameList[dbi].ToLower();

				RootTable rt = RootTable.GetFromTableId(dbName);
				if (rt != null) DbRootTableList.Add(rt);
				else continue; // don't include if not found
			}

			DbMatchLists = new Dictionary<string, List<SmallWorldMatch>>();
			SvgDepictionCount = 0;

			//StructureSearchExecutionFlags.Debug = true; // debug
			//StructureSearchExecutionFlags.UseMultipleThreads = false; // debug

			if (UseMultipleThreads) ExecuteMultiThreadSearch();
			else ExecuteSingleThreadSearch();

			MatchList = new List<SmallWorldMatch>();

			foreach (List<SmallWorldMatch> list in DbMatchLists.Values) // catenate lists together
			{
				if (list == null) continue;
				foreach (SmallWorldMatch m in list)
				{
					if (KeysToExclude != null && KeysToExclude.Contains(m.Cid)) continue; // filter any exclusion list

					MatchList.Add(m);
				}
			}

			//MatchList.Sort(StructSearchMatch.CompareByMatchQuality);
			SmallWorldMatch.SortListByDistance(MatchList);

			if (Swp.MaxHits >= 0 && MatchList.Count >= Swp.MaxHits) // remove any extra records
				MatchList.RemoveRange(Swp.MaxHits, MatchList.Count - Swp.MaxHits);


			return MatchList;
		}

		/// <summary>
		/// Setup mapping between SmallWorld columns and the output Query Engine Vo array
		/// </summary>
		/// <param name="selectList"></param>

		public void BuildSwToQeColumnMap(
			List<MetaColumn> selectList)
		{
			ColumnMap = new SmallWorldMatchColumnMap();
			ColumnMap.Setup(selectList);
			return;
		}

		/// <summary>
		/// ExecuteSingleThreadSearch
		/// </summary>

		void ExecuteSingleThreadSearch()
		{
			SmallWorldPredefinedParameters swp;

			for (int dbi = 0; dbi < DbRootTableList.Count; dbi++) // loop on each db
			{
				RootTable rt = DbRootTableList[dbi];

				swp = Swp.Clone();
				swp.RootTable = rt;
				swp.Database = rt.SmallWorldDbName;
				SmallWorldSingleDbSearch sdbs = new SmallWorldSingleDbSearch();
				List<SmallWorldMatch> matchList = sdbs.ExecuteSearch(swp);
				DbMatchLists[swp.Database] = matchList;
			}
			return;
		}

		/// <summary>
		/// ExecuteMultiThreadSearch
		/// </summary>
		/// <param name="swp"></param>
		/// <param name="recordsTotal"></param>
		/// <param name="recordsFiltered"></param>

		void ExecuteMultiThreadSearch()
		{
			SmallWorldPredefinedParameters swp;
			ThreadStart ts = null;
			Thread newThread = null;

			CountDownLatch = new CountdownEvent(DbRootTableList.Count);

			for (int dbi = 0; dbi < DbRootTableList.Count; dbi++) // replace any external database names with internal names
			{
				RootTable rt = DbRootTableList[dbi];

				swp = Swp.Clone();
				swp.RootTable = rt;
				swp.Database = rt.SmallWorldDbName;

				Thread t = new Thread(new ParameterizedThreadStart(ExecuteSingleDbSearchThreadMethod));
				t.Name = "SmallWorldSearchThread " + swp.Database;
				t.IsBackground = true;
				t.SetApartmentState(ApartmentState.STA);
				t.Start(swp);
			}

			CountDownLatch.Wait(); // wait until all threads are complete

			return;
		}

		void ExecuteSingleDbSearchThreadMethod(object swpArg)
		{
			try
			{
				SmallWorldPredefinedParameters swp = swpArg as SmallWorldPredefinedParameters;
				SmallWorldSingleDbSearch sdbs = new SmallWorldSingleDbSearch();
				List<SmallWorldMatch> matchList = sdbs.ExecuteSearch(swp);

				lock (DbMatchLists)
				{
					DbMatchLists[swp.Database] = matchList;
				}

				CountDownLatch.Signal(); // signal that we're done
				return;
			}

			catch (Exception ex)
			{
				DebugLog.Message(ex);
				ThreadException = ex;
				CountDownLatch.Signal(); // signal that we're done (but with error)
			}
		}

		/// <summary>
		/// Generate (if necessary) and serialize a list of all depictions for the current match list in the specified style
		/// </summary>
		/// <param name="color"></param>
		/// <param name="align"></param>
		/// <returns></returns>

		public string GetDepictionsAsMultiRecordText(
			bool color,
			bool align)
		{
			DateTime t0 = DateTime.Now;

			GetDepictions(color, align); // Generate depictions in paralled as needed

			StringBuilder sb = new StringBuilder();
			foreach (SmallWorldMatch m in MatchList)
			{
				string svg = GetDepiction(m, color, align);
				if (svg == null) continue; // don't store if couldn't depict
				if (svg.Contains("\t")) svg = svg.Replace("\t", " "); // remove any existing tabs and new lines
				if (svg.Contains("\n")) svg = svg.Replace("\n", " ");

				// Add line containing Cid, HitListId, RowId and Depiction Svg to string buffer
				sb.Append(m.Cid).Append('\t').Append(m.HitListId).Append('\t').Append(m.RowId).Append('\t').Append(svg).Append('\n');
			}

			if (Debug)
			{
				int count = MatchList != null ? MatchList.Count : -1;
				string msg = "Search Id: " + Id + ", MatchList count: " + count + ", Time: ";
				DebugLog.TimeMessage(msg, t0);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Generate depictions of specified type for StructSearchMatchList (MultiThreaded)
		/// </summary>
		/// <param name="matches"></param>
		/// <param name="color"></param>
		/// <param name="align"></param>

		public void GetDepictions(
			bool color,
			bool align)
		{
			int maxThreads = 2; // Timing for row count = 100: Threads: 1, Time:  7400 ms. ; Threads: 2, Time:  3891 ms. ; No faster with more threads

			Stopwatch sw = Stopwatch.StartNew();

			List<Thread> threads = new List<Thread>();

			GetDepictionsThreadParms parms = new GetDepictionsThreadParms(); // common parameters object
			parms.Matches = MatchList;
			parms.MatchPos = -1;
			parms.Color = color;
			parms.Align = align;

			for (int ti = 0; ti < maxThreads; ti++)
			{
				if (ti >= MatchList.Count) break;

				GetDepictionsThreadClass tc = new GetDepictionsThreadClass();
				tc.Parms = parms;

				Thread t = new Thread(new ThreadStart(tc.GetDepictionsThreadMethod));
				t.IsBackground = true;
				t.SetApartmentState(ApartmentState.STA);
				t.Start();
				threads.Add(t);
			}

			foreach (Thread t0 in threads)
			{
				t0.Join();
			}

			if (Debug)
			{
				string msg = "GetDepictions - Count: " + MatchList.Count + ", Threads: " + maxThreads;
				DebugLog.StopwatchMessage(msg + ", Time: ", sw);
			}

			return;
		}

		/// <summary>
		/// Parameters for GetDepiction
		/// </summary>
		class GetDepictionsThreadParms
		{
			public List<SmallWorldMatch> Matches;
			public int MatchPos = -1;
			public bool Color;
			public bool Align;
		}

		/// <summary>
		/// Class with Parms and GetDepictionThreadMethod
		/// Loops through all depictions processing one at a time until none left
		/// </summary>

		class GetDepictionsThreadClass
		{
			public GetDepictionsThreadParms Parms;

			/// <summary>
			/// Loop through list of matches getting depictions until none left
			/// </summary>

			public void GetDepictionsThreadMethod()
			{
				SmallWorldMatch match;

				while (true)
				{
					lock (Parms) // lock parms while getting next entry
					{
						if ((Parms.MatchPos + 1) >= Parms.Matches.Count) return; // all done if nothing left
						Parms.MatchPos++;
						match = Parms.Matches[Parms.MatchPos];
					}

					 GetDepiction(match, Parms.Color, Parms.Align);
				}
			}
		}

		/// <summary>
		/// Get a depiction from position in hitlist
		/// </summary>
		/// <param name="hitListId"></param>
		/// <param name="rowId"></param>
		/// <param name="color"></param>
		/// <param name="align"></param>
		/// <returns></returns>
		public string GetDepiction(
			int hitListId,
			int rowId,
			bool color,
			bool align)
		{
			if (MatchList == null) return "";

			foreach (SmallWorldMatch m in MatchList)
			{
				if (m.HitListId == hitListId && m.RowId == rowId)
				{
					string svg = GetDepiction(m, color, align);
					return svg;
				}
			}

			return ""; // not found
		}

		/// <summary>
		/// Get existing or generate hilighted/aligned SVG structure graphics
		/// </summary>
		/// <param name="m"></param>
		/// <param name="color"></param>
		/// <param name="align"></param>
		/// <returns></returns>

		public static string GetDepiction(
			SmallWorldMatch m,
			bool color,
			bool align)
		{
			int i = SmallWorldData.GetSvgOptionsIndex(color, align); // get color/align combo index

			if (m.DepictSvg == null) m.DepictSvg = new string[4];
			if (m.DepictSvg[i] == null)
			{
				string svg = GenerateDepiction(m, color, align);
				m.DepictSvg[i] = svg;
			}

			return m.DepictSvg[i];
		}

		/// <summary>
		/// Generate hilighted/aligned SVG structure graphics
		/// </summary>
		/// <param name="match"></param>
		/// <param name="color"></param>
		/// <param name="align"></param>

		public static string GenerateDepiction(
			SmallWorldMatch match,
			bool color,
			bool align)
		{
			string hitSmiles = match.HitSmiles;
			string qrySmiles = match.QrySmiles;
			string colorMap = match.AtomScore;
			string colors = "2cbe84,adffab,adffab,fff277,ffb67e,ffbec4"; // need %2C for commas in url?

			if (!color) colorMap = colors = ""; // turn off coloring

			if (!align) // turn off alignment by not passing QryMappedSmiles
				qrySmiles = "";

			Stopwatch sw = Stopwatch.StartNew();
			SmallWorldDao.SvgDepictionCount++;

			string msg = String.Format("GetDepiction {0}, HitlistId: {1},  RowId: {2}, Distance: {3}, Cid: {4}, Smiles: {5}",
				SmallWorldDao.SvgDepictionCount, match.HitListId, match.RowId, match.Distance, match.Cid, hitSmiles);

			HttpWebRequest request = null;
			WebResponse response = null;
			Stream reader = null;
			StreamReader sReader = null;

			string smiles, cid;
			int width = 256;

			try
			{
				//align = false; // debug
				//lock (DepictLock) // just one at a time (really?)
				{
					string url = SmallWorldDao.SmallWorldUrl + "/depict/svg" +
						"?w=" + width +
						"&h=" + width +
						"&smi=" + Uri.EscapeDataString(hitSmiles) +
						"&qry=" + Uri.EscapeDataString(qrySmiles) +
						"&cols=" + Uri.EscapeDataString(colors) +
						"&cmap=" + Uri.EscapeDataString(colorMap) +
						"&bgcolor=clear" +
						"&hgstyle=outerglow";

					//if (Debug) DebugLog.Message("SwDepict " + hitListId + " Depict started. url: " + url);

					request = SmallWorldSingleDbSearch.CreateWebRequest(url);

					response = request.GetResponse();

					reader = response.GetResponseStream();
					string svgXml = "";
					if (reader == null) throw new Exception("Null response");

					sReader = new StreamReader(reader);
					svgXml = sReader.ReadToEnd(); // get full XML

					sReader.Close();
					reader.Close();
					response.Close();

					if (Debug)
						DebugLog.StopwatchMessage(msg + ", Time: ", sw);

					//Thread.Sleep(100);

					return svgXml;
				} // lock
			} // try

			catch (Exception ex)
			{
				if (request != null) request.Abort();

				//string msg2 = msg + "\r\n" + DebugLog.FormatExceptionMessage(ex);
				//DebugLog.Message(msg2);
				return null;

				//throw new System.Exception(msg2, ex);
			}

			finally
			{
				// nothing for now
			}
		}

		/// <summary>
		/// See if SmallWorld service is available
		/// </summary>
		/// <returns></returns>

		public static bool IsSmallWorldAvailable()
		{
			try // do a test call to SmallWorld to see that things are working
			{
				CheckSmallWorld(); // do check, no exception if ok
				return true;
			}

			catch (Exception ex)
			{
				DebugLog.Message("SmallWorld unavailable: " + ex.Message);
				return false;
			}
		}

		/// <summary>
		/// Check SmallWorld, throw exception on error
		/// </summary>

		public static string CheckSmallWorld()
		{
			if (!SmallWorldAvailable) throw new Exception("SmallWorld disabled in .ini file setting");

			return ""; // ok
		}

		/// <summary>
		/// Get parsed version of database list
		/// </summary>
		/// <param name="arg"></param>
		/// <returns></returns>

		public string GetDatabaseList(string arg)
		{
			string json = SearchMaps();
			JObject jo = JObject.Parse(json);

			if (Lex.Eq(arg, "detail"))
				return jo.ToString();

			string dbList = "";

			foreach (JProperty db in jo.Children())
			{
				string name = db.Name;
				if (Lex.Contains(name, ".anon"))
					name = Lex.SubstringBefore(name, ".anon");

				dbList += name + "\r\n";
			}

			return dbList;
		}

		/// <summary>
		/// Retrieve the JSON for the list of available data sets (i.e. maps)
		/// </summary>

		public string SearchMaps()
		{
			HttpWebRequest request = null;
			WebResponse response = null;
			Stream reader = null;
			StreamReader sReader = null;

			try
			{
				string url = SmallWorldUrl + "/search/maps";

				DateTime t0 = DateTime.Now;
				request = SmallWorldSingleDbSearch.CreateWebRequest(url);
				response = request.GetResponse();

				reader = response.GetResponseStream();
				if (reader == null) throw new Exception("Null response");

				sReader = new StreamReader(reader);
				string json = sReader.ReadToEnd();
				sReader.Close();
				SearchMapsTime = TimeOfDay.Delta(t0);

				return json;
			}

			catch (Exception ex)
			{
				DebugLog.Message(ex);
				throw new System.Exception(ex.Message, ex);
			}

			finally
			{
				if (request != null) request.Abort();
			}

		}

		/// <summary>
		/// Analyze unmapped compounds by "heavy" bond count
		/// </summary>
		/// <returns></returns>

		public static string CheckUnmappedCompoundsByBondCount()
		{
			string line, s1, s2, smilesAndCorpId = "", smiles = "", CorpId, msg;
			int readCnt = 0, okCnt = 0, tooBigCnt = 0, exCnt = 0, haCnt, hbCnt;

			StreamReader sr = new StreamReader(@"c:\SmallWorld\Nomap.smi");
			StreamWriter swOk = new StreamWriter(@"c:\SmallWorld\NomapSizeOk.smi");
			StreamWriter swOkCorpId = new StreamWriter(@"c:\SmallWorld\NomapSizeOkCorpId.txt");
			StreamWriter swTooBig = new StreamWriter(@"c:\SmallWorld\NomapTooBig.smi");
			StreamWriter swEx = new StreamWriter(@"c:\SmallWorld\NomapException.smi");

			while (true)
			{
				if (readCnt % 10 == 0) // status
				{
					msg = string.Format("Reads: {0}, OK: {1}, Too Big: {2}, Exception {3}", readCnt, okCnt, tooBigCnt, exCnt);
					Mobius.UAL.Progress.Show(msg);
				}

				line = sr.ReadLine();
				if (line == null) break;
				readCnt++;

				try
				{
					Lex.Split(line, "\t", out s1, out s2, out smilesAndCorpId);
					Lex.Split(smilesAndCorpId, " ", out smiles, out CorpId);

					MolLibUtil.GetHeavyAtomBondCounts(smiles, out haCnt, out hbCnt);
					if (hbCnt <= 0) throw new Exception("No bonds");
					if (hbCnt <= 99)
					{
						okCnt++;
						swOk.WriteLine(smilesAndCorpId);
						swOkCorpId.WriteLine(CorpId);
					}

					else
					{
						tooBigCnt++;
						swTooBig.WriteLine(smilesAndCorpId + "\t" + hbCnt);
					}
				}

				catch (Exception ex)
				{
					exCnt++;
					swEx.WriteLine(smilesAndCorpId + "\t" + ex.Message);
				}


			}

			sr.Close();
			swOk.Close();
			swOkCorpId.Close();
			swTooBig.Close();
			swEx.Close();

			Progress.Hide();

			msg = string.Format("Reads: {0}, OK: {1}, Too Big: {2}, Exception {3}", readCnt, okCnt, tooBigCnt, exCnt);
			return msg;
		}

		public static string CheckUnmappedCompoundsSearchSubmit()
		{
			string line, s1, s2, smilesAndCorpId = "", smiles = "", CorpId, msg;
			int readCnt = 0, hitCnt = 0, missCnt = 0, tooBigCnt = 0, exCnt = 0, haCnt, hbCnt;

			StreamReader sr = new StreamReader(@"c:\SmallWorld\Nomap.smi");
			
			OpenCheckWriters();

			//string url = "http://[server]:8081/search/submit?dist=4&smi=c2ccc1ccccc1c2&db=chembl_21.anon";
			string url = "http://[server]:8081/search/submit?dist=0&smi=<smiles>&db=<db>&matrix=none";

			while (true)
			{
				if (readCnt % 10 == 0) // update status
				{
					CloseCheckWriters();
					OpenCheckWriters(true); // reopen in append mode

					msg = string.Format("Reads: {0}, Hit: {1}, Miss: {2}, Too Big: {3}, Exception {4}", readCnt, hitCnt, missCnt, tooBigCnt, exCnt);
					Mobius.UAL.Progress.Show(msg);
				}

				line = sr.ReadLine();
				if (line == null) break;
				readCnt++;

				try
				{
					Lex.Split(line, "\t", out s1, out s2, out smilesAndCorpId);
					Lex.Split(smilesAndCorpId, " ", out smiles, out CorpId);

					MolLibUtil.GetHeavyAtomBondCounts(smiles, out haCnt, out hbCnt);
					if (hbCnt <= 0) throw new Exception("No bonds");

					else if (hbCnt <= 99)
					{

						string url2 = Lex.Replace(url, "<smiles>", smiles);
						url2 = Lex.Replace(url2, "<db>", "pubchem.anon");

						SmallWorldSingleDbSearch sws = new SmallWorldSingleDbSearch();
						int hlid = sws.SearchSubmit(url2);
						if (hlid > 0)
						{
							hitCnt++;
							CwHit.WriteLine(smilesAndCorpId);
							CwHitCorpId.WriteLine(CorpId);
						}

						else
						{
							missCnt++;
							CwMiss.WriteLine(smilesAndCorpId);
							CwMissCorpId.WriteLine(CorpId);
						}
					}

					else
					{
						tooBigCnt++;
						CwTooBig.WriteLine(smilesAndCorpId + "\t" + hbCnt);
					}
				}

				catch (Exception ex)
				{
					exCnt++;
					CwEx.WriteLine(smilesAndCorpId + "\t" + ex.Message);
				}


			}

			sr.Close();

			CloseCheckWriters();

			Progress.Hide();

			msg = string.Format("Reads: {0}, Hit: {1}, Miss: {2}, Too Big: {3}, Exception {4}", readCnt, hitCnt, missCnt, tooBigCnt, exCnt);
			return msg;
		}

		static void OpenCheckWriters(bool append = false)
		{

			CwHit = new StreamWriter(@"c:\SmallWorld\NomapHit.smi", append);
			CwHitCorpId = new StreamWriter(@"c:\SmallWorld\NomapHitCorpId.txt", append);

			CwMiss = new StreamWriter(@"c:\SmallWorld\NomapMiss.smi", append);
			CwMissCorpId = new StreamWriter(@"c:\SmallWorld\NomapMissCorpId.txt", append);

			CwTooBig = new StreamWriter(@"c:\SmallWorld\NomapTooBig.smi", append);
			CwEx = new StreamWriter(@"c:\SmallWorld\NomapException.smi", append);

			return;
		}

		static void CloseCheckWriters()
		{
			CwHit.Close();
			CwHitCorpId.Close();

			CwMiss.Close();
			CwMissCorpId.Close();

			CwTooBig.Close();
			CwEx.Close();

			return;
		}

	} // SmallWorldDao

	/// <summary>
	/// Execute a single database search
	/// </summary>

	public class SmallWorldSingleDbSearch
	{
		internal int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		public int HitListId = NullValue.NullNumber;
		public SmallWorldPredefinedParameters Swp = null; // Search parameters
		public RootTable RootTable = null; // database info on the database being searched
		public int HeavyAtomCount = 0; // used to calc sim score
		public int HeavyBondCount = 0;

		public int RecordsTotal = -1;
		public int RecordsFiltered = -1;
		public List<SmallWorldMatch> MatchList = null; // match records from SearchView or SearchExport

		double SearchMapsTime = 0;
		double SearchSubmitTime = 0;
		double SearchViewTime = 0;
		double SearchExportTime = 0;

		static bool LastCheckComplete; // check completed
		static string LastCheckResponse; // response from check
		static Exception LastCheckException; // any exception that appears during check

		static IMolLib MolLibUtil => StaticMolLib.I; // static molecule shortcut for utility methods

		static object DepictLock = new object();

		static bool Debug => SmallWorldDao.Debug;

		/// <summary>
		/// Basic constructor
		/// </summary>

		public SmallWorldSingleDbSearch()
		{
			return;
		}

		public List<SmallWorldMatch> ExecuteSearch(
			SmallWorldPredefinedParameters swp)
		{
			HitListId = SearchSubmit(swp); // execute the search
			if (HitListId < 0) // query not found and cannot start search
			{
				MatchList = new List<SmallWorldMatch>();
				return MatchList;
			}

			int rows = 1000;
			if (swp.MaxHits > 0) rows = swp.MaxHits;

			SearchView(0, rows);
			return MatchList;

#if false

// First Export and then get RowIds via Search/View and merge
// Needed until version 3.1 which returns Smiles in SearchView

			SearchExport();

			List<SmallWorldMatch> ml0 = MatchList;
			MatchList = null;
			SearchView(0, 1000);
			List<SmallWorldMatch> ml1 = MatchList;

			Dictionary<string, SmallWorldMatch> d = new Dictionary<string, SmallWorldMatch>();
			foreach (SmallWorldMatch m in ml0)
			{
				if (d.ContainsKey(m.Cid)) continue;
				d[m.Cid] = m;
			}
			
			foreach (SmallWorldMatch m in ml1)
			{
				if (!d.ContainsKey(m.Cid)) continue;
				d[m.Cid].RowId = m.RowId;
			}

			MatchList = ml0;

			return MatchList;
#endif
		}

		/// <summary>
		/// Submit search
		/// </summary>
		/// <param name="swp"></param>
		/// <param name="keySubset"></param>
		/// <returns></returns>

		public int SearchSubmit(
			SmallWorldPredefinedParameters swp)
		{

			try
			{
				Swp = swp;

				if (Lex.IsUndefined(swp.Smiles)) throw new Exception("SmallWorld Smiles structure not defined");

				if (swp.Smiles.Contains(".")) // possible multiple fragments?
				{
					swp.Smiles = MolLibUtil.GetLargestSmilesMoleculeFragment(swp.Smiles);
				}

				MolLibUtil.GetHeavyAtomBondCounts(swp.Smiles, out HeavyAtomCount, out HeavyBondCount);

				if (Lex.IsUndefined(swp.Database)) throw new Exception("SmallWorld Database not defined");
				if (swp.RootTable != null)
					RootTable = swp.RootTable;
				else RootTable = RootTable.GetFromSwName(swp.Database);

				AdjustParameterValues(swp);

				string url = SmallWorldDao.SmallWorldUrl + "/search/submit" +
					"?smi=" + Uri.EscapeDataString(swp.Smiles) +
					"&db=" + Uri.EscapeDataString(swp.Database) +
					AddHighParm("dist", swp.Distance) +
					AddHighParm("tdn", swp.TerminalDown) +
					AddHighParm("tup", swp.TerminalUp) +
					AddHighParm("rdn", swp.RingDown) +
					AddHighParm("rup", swp.RingUp) +
					AddHighParm("ldn", swp.LinkerDown) +
					AddHighParm("lup", swp.LinkerUp) +
					"&matrix=" + Uri.EscapeDataString(swp.TransmutationMatrix) +
					"&route=" + Uri.EscapeDataString(swp.Route);

				SearchSubmit(url);

				return HitListId;
			}

			catch (Exception ex)
			{
				DebugLog.Message(ex);
				throw new System.Exception(ex.Message, ex);
			}

		}

		public int SearchSubmit(
			string url)
		{
			HttpWebRequest request = null;
			WebResponse response = null;
			Stream reader = null;
			StreamReader sReader = null;
			DateTime t0 = DateTime.Now;

			try
			{
				if (url.Contains('#')) 
					url = url.Replace("#", "%23"); // excape any triple bonds

				request = SmallWorldSingleDbSearch.CreateWebRequest(url);
				response = request.GetResponse();

				reader = response.GetResponseStream();
				string results = "", resultLine = "";
				if (reader == null) throw new Exception("Null response");

				sReader = new StreamReader(reader);

				while (true)
				{
					resultLine = sReader.ReadLine();
					if (resultLine == null) break;
					results += resultLine + "\r\n";
					if (Lex.IsUndefined(resultLine)) continue;

					HitListId = Lex.ExtractIntBetween(resultLine, "\"hlid\":", ",");

					if (Debug) DebugLog.Message("SwSrch " + Id + ", Status: " + resultLine);

					if (Lex.Contains(resultLine, "\"status\":\"FIRST\"")) continue; // query found and search started
					else if (Lex.Contains(resultLine, "\"status\":\"MORE\"")) break; // break after first group of results
					else if (Lex.Contains(resultLine, "\"status\":\"END\"")) break; // all done
					else if (Lex.Contains(resultLine, "\"status\":\"Ground Control to Major Tom\"")) continue; // server is just checking to see if we're still there

					else if (Lex.Contains(resultLine, "\"status\":\"MISS\"")) // query anon graph not found in index and cannot start search, return code -1
					{
						// data:{"status":"FIRST","hlid":38,"elap":"0.0 s","numEdges":0,"numNodes":0,"numWaveFront":0,"numEdgesPerSec":"NaN"}

						sReader.Close();
						//DebugLog.Message("SmallWorld \"MISS\" search failure for request: " + url); // some misses are expected
						HitListId = -1;
						return HitListId;
					}

					else
					{
						DebugLog.Message("SmallWorld unexpected status message for request: " + url + "\r\n" + resultLine);
						continue; // just log && ignore for now
					}
				}

				//sReader.Close(); // seems to be slow on some servers

				return HitListId;
			}


			catch (Exception ex)
			{
				//DebugLog.Message(ex);
				throw new Exception(ex.Message, ex);
			}

			finally
			{
				if (request != null) request.Abort();
			}

		}

		/// <summary>
		/// Validatte/adjust parameter values for proper operation and better performance
		/// </summary>
		/// <param name="swp"></param>

		void AdjustParameterValues(SmallWorldPredefinedParameters swp)
		{
			RangeParm d = swp.Distance;

			int min = swp.Distance.Low;
			int max = swp.Distance.High;

			AdjustBounds(d, swp.TerminalUp);
			AdjustBounds(d, swp.TerminalDown);
			AdjustBounds(d, swp.RingUp);
			AdjustBounds(d, swp.RingDown);

			if (swp.MatchAtomTypes) // passed linker parms MUST BE zero if matching atom types
			{
				swp.LinkerDown.Low = swp.LinkerDown.High = swp.LinkerUp.Low = swp.LinkerUp.High = 0; // linker values must me zero

				AdjustBounds(d, swp.MutationMajor); // adjust "Color" parameters since these are applied
				AdjustBounds(d, swp.MutationMinor);
				AdjustBounds(d, swp.SubstitutionRange);
				AdjustBounds(d, swp.HybridisationChange);
			}

			else // Linker can be defined an adjusted if not matching atom types
			{
				AdjustBounds(d, swp.LinkerUp);
				AdjustBounds(d, swp.LinkerDown);
			}
		}

		/// <summary>
		/// Adjust bounds for parameter so that the p2 upper limit don't exceed the overall distance
		/// </summary>
		/// <param name="dist"></param>
		/// <param name="p2"></param>

		void AdjustBounds(
			RangeParm dist,
			RangeParm p2)
		{
			int h = dist.High;
			if (h < 0) return;

			if (p2.High < 0 || p2.High > h) p2.High = h; // p2 high value can't exceed p1 hight value

			if (p2.Low < 0) p2.Low = 0; // define minimum low value also

			return;
		}

		/// <summary>
		/// Get hit counts
		/// </summary>
		/// <param name="recordsTotal"></param>
		/// <param name="recordsFiltered"></param>

		public void SearchHitCount(
			out int recordsTotal,
			out int recordsFiltered)
		{
			recordsTotal = recordsFiltered = -1;

			SearchView(0, 0);

			recordsTotal = RecordsTotal;
			recordsFiltered = RecordsFiltered;
			return;
		}

		/// <summary>
		/// Retrieve a subset of results
		/// </summary>
		/// <param name="start"></param>
		/// <param name="length"></param>
		/// 

		public void SearchView(
			int start,
			int length)
		{

			/*
			 ******************
			 *** Sample URL ***
			 ******************

				http://[server]:8081/search/view?hlid=XXX

			// update...
			
			 ****************************
			 *** Sample Returned JSON ***
			 ****************************
				
			// update...

			*/

			//if (RecordsTotal == -1) return; // disable

			HttpWebRequest request = null;
			WebResponse response = null;
			string json = "";
			Stream reader = null;
			StreamReader sReader = null;
			string cid;

			try
			{
				AdjustParameterValues(Swp);

				string url = SmallWorldDao.SmallWorldUrl + "/search/view" +
				"?hlid=" + HitListId +
				"&columns[0][name]=compound" + // contains hit Id (Cid), hitlist id, rowid, mf, mw & smiles
				"&columns[1][name]=alignment" + // contains hit Id (Cid), hit smiles, query smiles and mapping info
				AddFilteredColumnParm(2, "dist", Swp.Distance) +
				AddFilteredColumnParm(3, "tdn", Swp.TerminalDown) +
				AddFilteredColumnParm(4, "tup", Swp.TerminalUp) +
				AddFilteredColumnParm(5, "rdn", Swp.RingDown) +
				AddFilteredColumnParm(6, "rup", Swp.RingUp);

				if (!Swp.MatchAtomTypes) url += // linkers allowed only if not matching atom types
					AddFilteredColumnParm(7, "ldn", Swp.LinkerDown) +
					AddFilteredColumnParm(8, "lup", Swp.LinkerUp);

				else url += // apply any atom type change filters and get values
					AddFilteredColumnParm(7, "maj", Swp.MutationMajor) +
					AddFilteredColumnParm(8, "min", Swp.MutationMinor) +
					AddFilteredColumnParm(9, "hyb", Swp.HybridisationChange) +
					AddFilteredColumnParm(10, "sub", Swp.SubstitutionRange);

				url +=
				"&order[0][column]=3" + // order from nearest to furthest (currently fails for base matrix?)
				"&order[0][dir]=asc" +
				"&start=" + start +
				"&length=" + length;

				DateTime t0 = DateTime.Now;
				if (Debug) DebugLog.Message("SwSrch " + Id + " View started for Db: " + Swp.Database + ", url: " + url);

				request = SmallWorldSingleDbSearch.CreateWebRequest(url);
				response = request.GetResponse();

				reader = response.GetResponseStream();
				if (reader == null) throw new Exception("Null response");

				sReader = new StreamReader(reader);
				json = sReader.ReadToEnd(); // get full data set
				sReader.Close();

				JObject jo = JObject.Parse(json);
				RecordsTotal = (int)jo["recordsTotal"];
				RecordsFiltered = (int)jo["recordsFiltered"];
				if (start == 0 || MatchList == null) MatchList = new List<SmallWorldMatch>();

				JArray data = (JArray)jo["data"];
				Dictionary<string, object> propDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

				for (int ri = 0; ri < data.Count; ri++)
				{
					SmallWorldMatch m = new SmallWorldMatch();
					m.SwSearch = this;
					m.Database = RootTable.Label; // database is constant for search
					m.DatabaseId = RootTable.DatabaseId;

					int i = 0; // index for data item within row
					JArray row = (JArray)data[ri]; // data for row/hit

					// First item is "compound"

					JArray cpd = (JArray)data[ri][i++]; // "compound" info is in an array
					m.HitListId = (int)cpd[0]; // hitlist id is first
					m.RowId = (int)cpd[1]; // index of hitlist row from topological search step (needed for search/depict)
					cid = (string)cpd[2]; // cid is third
					m.Cid = NormalizeCid(cid);

					// Next item is the alignment info

					JEnumerable<JToken> align = ((JObject)row[i++]).Children<JToken>(); // "alignment" info
					JTokenListToDict(align, propDict);

					if (TryGetStringValue("hitMappedSmiles", propDict, out m.HitSmiles))
					TryGetStringValue("qryMappedSmiles", propDict, out m.QrySmiles);

					TryGetIntStringList("atomMap", propDict, out m.AtomMap);
					TryGetIntStringList("atomScore", propDict, out m.AtomScore);

					// Other properties in "align" element
					//  qrySmiles
					//  qryMappedSmiles
					//  hitMappedSmiles
					//  anonIdx, e.g.B13R1.452
					//  mf
					//  mw

					// Get additional "simple" items

					m.Distance = (Int16)data[ri][i++]; // dist (overall distance)
					m.SimScore = CalcSimScore(m);

					// Topological Edits

					m.TerminalDown = (Int16)data[ri][i++]; // tdn
					m.TerminalUp = (Int16)data[ri][i++]; // tup
					m.RingDown = (Int16)data[ri][i++]; // rdn
					m.RingUp = (Int16)data[ri][i++]; // rup
					if (!Swp.MatchAtomTypes) // linkers allowed only if not matching atom types
					{
						m.LinkerDown = (Int16)data[ri][i++]; // ldn
						m.LinkerUp = (Int16)data[ri][i++]; // lup
					}

					else // Atom type scoring ("Color" Edits)
					{
						m.MajorMutations = (Int16)data[ri][i++]; // Major Transmutation (maj)
						m.MinorMutations = (Int16)data[ri][i++]; // Minor Transmutation (min)
						m.Hybridisation = (Int16)data[ri][i++]; // Hybridisation Change (hyb)
						m.Substitutions = (Int16)data[ri][i++]; // Hydroden Substitution (sub)
					}

					MatchList.Add(m);
				}

				SearchViewTime = TimeOfDay.Delta(t0);

				if (Debug) DebugLog.TimeMessage("SwSrch " + Id + " View complete, HitListId:" + HitListId +
					", RecordsTotal: " + RecordsTotal + ", RecordsFiltered: " + RecordsFiltered + ", Start: " + start + ", Length: " + length + ", Time: ", t0);

				return;
			}
			catch (Exception ex)
			{
				string msg = ex.Message;
				if (Lex.IsDefined(json)) msg += "\r\nJson: " + json;
				Exception ex2 = new System.Exception(msg, ex);
				DebugLog.Message(ex2);
				throw ex2;
			}

			finally
			{
				if (request != null) request.Abort();
			}

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceList"></param>
		/// <param name="dict"></param>

		public static void JTokenListToDict(
			JEnumerable<JToken> sourceList,
			Dictionary<string, object> dict)
		{
			JToken nonPropertyChild = null;
			dict.Clear();

			foreach (JToken child in sourceList)
			{
				if (child.Type.ToString().Equals("Property")) // all children should be properties
				{
					JProperty prop = child as JProperty;
					if (prop == null) continue;
					string k = prop.Name;

					if (prop.Value == null) continue;
					if (prop.Value.Type == JTokenType.String)
					{
						JValue v = prop.Value as JValue;
						dict[k] = v.ToString();
					}

					else dict[k] = prop.Value; // other JSON JTokenType (e.g. Array, Integer...)
				}

				else nonPropertyChild = child; // shouldn't happen
			}

			return;
		}

		/// <summary>
		/// Try to get a string value from the dict
		/// </summary>
		/// <param name="key"></param>
		/// <param name="dict"></param>
		/// <param name="value"></param>
		/// <returns></returns>

		static bool TryGetStringValue(
			string key,
			Dictionary<string, object> dict,
			out string value)
		{
			value = null;

			if (!dict.ContainsKey(key)) return false;

			value = dict[key] as string;
			return true;
		}

		/// <summary>
		/// Get value from dict and convert to an int[]
		/// </summary>
		/// <param name="key"></param>
		/// <param name="dict"></param>
		/// <param name="ia"></param>
		/// <returns></returns>

		static bool TryGetIntStringList(
			string key,
			Dictionary<string, object> dict,
			out string sl)
		{
			sl = null;

			if (!dict.ContainsKey(key)) return false;

			JArray ja = dict[key] as JArray;
			if (ja == null) return false;

			int [] ia = ja.ToObject<int[]>();

			StringBuilder sb = new StringBuilder();
			foreach (int i in ia)
			{
				if (sb.Length > 0)
					sb.Append(",");
				sb.Append(i.ToString());
			}

			sl = sb.ToString();

			return true;
		}


		/// <summary>
		/// Get value from dict and convert to an int[]
		/// </summary>
		/// <param name="key"></param>
		/// <param name="dict"></param>
		/// <param name="ia"></param>
		/// <returns></returns>

		static bool TryGetIntArray(
			string key,
			Dictionary<string, object> dict,
			out int[] ia)
		{
			ia = null;

			if (!dict.ContainsKey(key)) return false;

			JArray ja = dict[key] as JArray;
			if (ja == null) return false;

			ia = ja.ToObject<int[]>();
			return true;
		}

		/// <summary>
		/// Export results of search
		/// </summary>

		public void SearchExport()
		{
			HttpWebRequest request = null;
			WebResponse response = null;
			Stream reader = null;
			StreamReader sReader = null;

			string smiles, cid;

			try
			{
				AdjustParameterValues(Swp);

				string url = SmallWorldDao.SmallWorldUrl + "/search/export" +
					"?hlid=" + HitListId +
					"&columns[0][name]=compound" +
					AddFilteredColumnParm(1, "dist", Swp.Distance) +
					AddFilteredColumnParm(2, "tdn", Swp.TerminalDown) +
					AddFilteredColumnParm(3, "tup", Swp.TerminalUp) +
					AddFilteredColumnParm(4, "rdn", Swp.RingDown) +
					AddFilteredColumnParm(5, "rup", Swp.RingUp);

				if (!Swp.MatchAtomTypes) url += // linkers allowed only if not matching atom types
					AddFilteredColumnParm(6, "ldn", Swp.LinkerDown) +
					AddFilteredColumnParm(7, "lup", Swp.LinkerUp);

				else url += // apply any atom type change filters and get values
					AddFilteredColumnParm(6, "maj", Swp.MutationMajor) +
					AddFilteredColumnParm(7, "min", Swp.MutationMinor) +
					AddFilteredColumnParm(8, "hyb", Swp.HybridisationChange) +
					AddFilteredColumnParm(9, "sub", Swp.SubstitutionRange);

				url +=
					"&order[0][column]=1" + // order by increasing distance
					"&order[0][dir]=asc";

				DateTime t0 = DateTime.Now;
				if (Debug) DebugLog.Message("SwSrch " + Id + " Export started. url: " + url);

				request = SmallWorldSingleDbSearch.CreateWebRequest(url);
				response = request.GetResponse();

				reader = response.GetResponseStream();
				string results = "";
				if (reader == null) throw new Exception("Null response");

				sReader = new StreamReader(reader);
				results = sReader.ReadToEnd(); // get full export data set
				sReader.Close();

				string[] resultLines = results.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
				MatchList = new List<SmallWorldMatch>();

				if (resultLines.Length == 0) return;

				string headerLine = resultLines[0]; // first line is header
				string[] headers = headerLine.Split('\t');

				for (int ri = 1; ri < resultLines.Length; ri++)
				{
					SmallWorldMatch m = new SmallWorldMatch();
					m.SwSearch = this;
					m.Database = RootTable.Label; // database is constant for search
					m.DatabaseId = RootTable.DatabaseId;

					m.HitListId = HitListId;
					//m.RowId = xxx; // not available in Export, only in View (needed for Depict)

					string line = resultLines[ri];
					if (Lex.IsUndefined(line)) continue;
					string[] va = line.Split('\t');

					int i = 0;
					string cpd = va[i++]; // smiles and CID info is in one value
					Lex.Split(cpd, " ", out smiles, out cid);
					m.Cid = NormalizeCid(cid);
					m.HitSmiles = smiles;

					m.Distance = Int16.Parse(va[i++]); // dist (overall distance)
					m.SimScore = CalcSimScore(m);

					// Topological Edits

					m.TerminalDown = Int16.Parse(va[i++]); // tdn
					m.TerminalUp = Int16.Parse(va[i++]); // tup
					m.RingDown = Int16.Parse(va[i++]); // rdn
					m.RingUp = Int16.Parse(va[i++]); // rup
					if (!Swp.MatchAtomTypes) // linkers allowed only if not matching atom types
					{
						m.LinkerDown = Int16.Parse(va[i++]); // ldn
						m.LinkerUp = Int16.Parse(va[i++]); // lup
					}

					else // Atom type scoring ("Color" Edits)
					{
						m.MajorMutations = Int16.Parse(va[i++]); // Major Transmutation (maj)
						m.MinorMutations = Int16.Parse(va[i++]); // Minor Transmutation (min)
						m.Hybridisation = Int16.Parse(va[i++]); // Hybridisation Change (hyb)
						m.Substitutions = Int16.Parse(va[i++]); // Hydroden Substitution (sub)
					}

					MatchList.Add(m);
				}

				//SmallWorldMatch.SortListByDistance(ExportRecords);

				int erc = MatchList.Count;

				SearchExportTime = TimeOfDay.Delta(t0);
				if (Debug) DebugLog.TimeMessage("SwSrch " + Id + " Export complete, HitListId:" + HitListId +
					", ExportRecords: " + MatchList.Count + ", Time: ", t0);

				return;
			}

			catch (Exception ex)
			{
				DebugLog.Message(ex);
				throw new Exception("SmallWorld Search/Export error: " + ex.Message, ex);
			}

			finally
			{
				if (request != null) request.Abort();
			}

		}

		/// <summary>
		/// Calc sim score as (heavyAB - distance) / heavyAB
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>

		float CalcSimScore(SmallWorldMatch m)
		{
			int habCnt = HeavyAtomCount + HeavyAtomCount;
			float f = habCnt - m.Distance;
			if (f < 0) f = 0;
			f = f / habCnt;
			f = (float)Math.Pow(f, 1.5); // scale downward a bit
			return f;
		}
					
		string AddFilteredColumnParm(int ci, string colName, RangeParm r)
		{
			string parm =
			 "&columns[" + ci + "][name]=" + colName;

			if (r != null && r.Low >= 0 && r.High >= 0 && r.Active) // add criteria if active
				parm += "&columns[" + ci + "][search][value]=" + r.Low + "-" + r.High;

			return parm;
		}

		/// <summary>
		/// Add parameter with high range value only for use in search step
		/// </summary>
		/// <param name="parmName"></param>
		/// <param name="r"></param>
		/// <returns></returns>

		string AddHighParm(string parmName, RangeParm r)
		{
			string parm = "";
			if (r == null || r.High < 0 || !r.Active) // if no active value then set to default high value
				parm = "&" + parmName + "=10";

			else parm = "&" + parmName + "=" + r.High;
			return parm;
		}

		/// <summary>
		/// Normalize Cid
		/// </summary>
		/// <param name="cid"></param>
		/// <returns></returns>

		string NormalizeCid(string cid)
		{
			if (Lex.Contains(RootTable.MetaTableName, "Corp") && Lex.IsInteger(cid)) // hack for corp database to get proper length CorpId string
				cid = CompoundId.NormalizeForDatabase(cid);

			else if (!RootTable.PrefixIsStored && Lex.IsDefined(RootTable.Prefix)) // add prefix if needed
				cid = RootTable.Prefix + cid;

			return cid;
		}

		/// <summary>
		/// Create a web request with set of common settings
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>

		public static HttpWebRequest CreateWebRequest(string url)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
			request.AuthenticationLevel = AuthenticationLevel.MutualAuthRequested;
			request.Credentials = CredentialCache.DefaultCredentials;
			request.AutomaticDecompression = DecompressionMethods.GZip;

			request.KeepAlive = false;
			request.Timeout = 60000; // 60 sec timeout
			request.Proxy = null;

			request.ServicePoint.ConnectionLeaseTimeout = 5000;
			request.ServicePoint.MaxIdleTime = 5000;

			return request;

			//request.ServicePoint.CloseConnectionGroup(request.ConnectionGroupName); // help to close out?
		}
	}

	/// <summary>
	/// Enum of fields available from search
	/// Used to index into ColumnMap for each search result field retrieved from SmallWorld
	/// </summary>

	public enum SwFieldEnum
	{
		Cid = 0,
		Smiles = 1,
		Database = 2,
		DataLink = 3,

		SimScore = 4,
		Distance = 5,

		// Topological Edits

		TerminalDown = 6, // tdn
		TerminalUp = 7, // tup
		RingDown = 8, // rdn
		RingUp = 9, // rup
		LinkerDown = 10, // ldn
		LinkerUp = 11,// lup

		// Atom type scoring ("Color" Edits)

		MajorMutations = 12, // Major Transmutation (maj)
		MinorMutations = 13, // Minor Transmutation (min)
		Hybridisation = 14, // Hybridisation Change (hyb)
		Substitutions = 15, // Hydrogen Substitution (sub)
	}

	/// <summary>
	/// Match data
	/// </summary>

	public class SmallWorldMatch
	{
		public SmallWorldSingleDbSearch SwSearch = null; // search/submit that created us
		public int HitListId = -1; // associated hit list
		public int RowId = -1; // index of hitlist row from topological search step (i.e. before secondary filtering) (needed for search/depict)

		public string Cid = null;
		public string HitSmiles = null; // Smiles for hit, normally with atom-mapping information included
		public string QrySmiles = null; // Smiles for query, normally with atom-mapping information included
		public string AtomMap = null;
		public string AtomScore = null;
		public string[] DepictSvg = null; // Depiction Svgs 0 - 4 depending on hilight/align selections
		public string Database = null;
		public int DatabaseId = -1;

		public float SimScore = -1; // similarity score 0 - 1
		public Int16 Distance = -1; // edit distance

		// Topological Edits

		public Int16 TerminalDown = -1; // tdn
		public Int16 TerminalUp = -1; // tup
		public Int16 RingDown = -1; // rdn
		public Int16 RingUp = -1; // rup
		public Int16 LinkerDown = -1; // ldn
		public Int16 LinkerUp = -1; // lup

		// Atom type scoring ("Color" Edits)

		public Int16 MajorMutations = -1; // Major Transmutation (maj)
		public Int16 MinorMutations = -1; // Minor Transmutation (min)
		public Int16 Hybridisation = -1; // Hybridisation Change (hyb)
		public Int16 Substitutions = -1; // Hydrogen Substitution (sub)

		/// <summary>
		/// Build QE Value Object Array (Voa) of values from SmallWorldMatch object
		/// </summary>

		public object[] BuildVoa(
			SmallWorldDao swDao,
			int ri)
		{
			SmallWorldMatchColumnMap columnMap = swDao.ColumnMap;
			int len = columnMap.VoiToFieldEnum.Count;
			object[] voa = new object[len];
			for (int voi = 0; voi < len; voi++)
			{
				voa[voi] = GetVal(swDao, ri, columnMap.VoiToFieldEnum[voi]);
			}

			return voa;
		}

		/// <summary>
		/// Get a match field as an object
		/// </summary>
		/// <param name="fe"></param>
		/// <returns></returns>

		object GetVal(
			SmallWorldDao swDao,
			int ri,
			SwFieldEnum fe)
		{
			switch (fe)
			{
				case SwFieldEnum.Cid: return Cid;
				case SwFieldEnum.Smiles:
					{
						MoleculeMx cs = new MoleculeMx(MoleculeFormat.Smiles, HitSmiles);
						cs.DbLink = HitListId.ToString() + "," + RowId;
						return cs;
					}

				case SwFieldEnum.Database: return Database;
				case SwFieldEnum.DataLink: return Cid;

				case SwFieldEnum.SimScore: return ToObj(SimScore);
				case SwFieldEnum.Distance: return ToObj(Distance);

				case SwFieldEnum.TerminalDown: return ToObj(TerminalDown);
				case SwFieldEnum.TerminalUp: return ToObj(TerminalUp);
				case SwFieldEnum.RingDown: return ToObj(RingDown);
				case SwFieldEnum.RingUp: return ToObj(RingUp);
				case SwFieldEnum.LinkerDown: return ToObj(LinkerDown);
				case SwFieldEnum.LinkerUp: return ToObj(LinkerUp);

				case SwFieldEnum.MajorMutations: return ToObj(MajorMutations);
				case SwFieldEnum.MinorMutations: return ToObj(MinorMutations);
				case SwFieldEnum.Hybridisation: return ToObj(Hybridisation);
				case SwFieldEnum.Substitutions: return ToObj(Substitutions);

				default: return null;
			}
		}

		object ToObj(Int16 val)
		{
			return val >= 0 ? (object)val : null;
		}

		object ToObj(float val)
		{
			return val >= 0 ? (object)val : null;
		}

		/// <summary>
		/// Sort a List<SmallWorldMatch by smallest to largest distance
		/// </summary>
		/// <param name="list"></param>

		public static void SortListByDistance(List<SmallWorldMatch> list)
		{
			if (list == null || list.Count == 0) return;

			IComparer<SmallWorldMatch> comparer = new SwmDistanceComparer();
			list.Sort(comparer);
			return;
		}

		/// <summary>
		/// SmallWorldMatch distance comparer
		/// </summary>
		class SwmDistanceComparer : IComparer<SmallWorldMatch>
		{
			public int Compare(SmallWorldMatch m1, SmallWorldMatch m2)
			{
				// Compare first on score

				int cv = m1.Distance.CompareTo(m2.Distance);
				if (cv != 0) return cv;

				// Then on database Id (asc)

				cv = m1.DatabaseId.CompareTo(m2.DatabaseId);
				if (cv != 0) return cv;

				// Then on database name (in case Id not defined) (asc) 

				cv = m1.Database.CompareTo(m2.Database);
				return cv;
			}
		}

	} // SmallWorldMatch

	/// <summary>
	/// Mapping of SmallWorld result columns to Voa positions
	/// </summary>

	public class SmallWorldMatchColumnMap
	{
		public int[] FieldEnumToVoi = null; // SwField enum value to associated vo position for each possible field
		public List<SwFieldEnum> VoiToFieldEnum = null; // vo position to associated SwField enum value


		/// <summary>
		/// Return true if the specified SwFieldEnum field in mapped
		/// </summary>
		/// <param name="fieldId"></param>
		/// <returns></returns>

		public bool SwFieldIsMapped(SwFieldEnum fieldId)
		{
			int fi = (int)fieldId;
			if (FieldEnumToVoi[fi] >= 0) return true;
			else return false;
		}

		public void Set(
			object[] voa,
			SwFieldEnum swf,
			object value)
		{
			if (FieldEnumToVoi[(int)swf] < 0) return;

			else voa[FieldEnumToVoi[(int)swf]] = value;
		}

		public object Get(
			object[] voa,
			SwFieldEnum swf)
		{
			if (FieldEnumToVoi[(int)swf] < 0) return null;

			else return voa[FieldEnumToVoi[(int)swf]];
		}

		/// <summary>
		/// Setup mapping between SmallWorld columns and selected output Vo array
		/// </summary>
		/// <param name="mcList"></param>

		public void Setup(
			List<MetaColumn> mcList)
		{
			SwFieldEnum swf;

			int enumValCnt = Enum.GetValues(typeof(SwFieldEnum)).Length;
			FieldEnumToVoi = new int[enumValCnt]; // SwField enum value to associated vo position for each possible field
			ArrayMx.Fill(FieldEnumToVoi, -1);

			VoiToFieldEnum = new List<SwFieldEnum>(); // vo position to associated SwField enum value
			for (int voi = 0; voi < mcList.Count; voi++)
			{
				MetaColumn mc = mcList[voi];
				string colMap = mc.ColumnMap;
				if (mc.IsKey) colMap = SwFieldEnum.Cid.ToString(); // map columns from metatables other than SmallWorld to the correct fields
				else if (mc.DataType == MetaColumnType.Structure) colMap = SwFieldEnum.Smiles.ToString();
				else if (mc.IsDatabaseSetColumn) colMap = SwFieldEnum.Database.ToString();
				else if (Lex.Eq(mc.DataTransform, "GetMolSim")) colMap = SwFieldEnum.SimScore.ToString();

				if (!Enum.TryParse(colMap, true, out swf))
					throw new Exception("SwField not found: " + colMap);

				FieldEnumToVoi[(int)swf] = voi;
				VoiToFieldEnum.Add(swf);
			}

			return;
		}

	} // SmallWorldMatchMap

}


