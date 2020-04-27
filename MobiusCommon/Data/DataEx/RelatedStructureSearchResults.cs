using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mobius.Data
{
	/// <summary>
	///  RelatedStructureSearchResult
	/// </summary>

	public class RelatedStructureSearchResults
	{
		public int Id = -1;
		public string Cid = ""; // compound id
		public string CidMtName = ""; // associated root metatable 
		public bool SearchCorp = true;
		public bool SearchChembl = false;

		public int AltCorpDbCnt = -1;
		public int MmpCorpDbCnt = -1;
		public int SwCorpDbCnt = -1;
		public int SimCorpDbCnt = -1;
		public int SSSCorpCnt = -1;

		public int AltChemblCnt = -1;
		public int MmpChemblCnt = -1;
		public int SwChemblCnt = -1;
		public int SimChemblCnt = -1;
		public int SSSChemblCnt = -1;

		public int CorpDbCnt = -1; 
		public int ChemblCnt = -1;

		public int DupCnt = -1;

		public List<StructSearchMatch> MatchList = null; // list of matches
		public static List<RelatedStructureSearchResults> History = new List<RelatedStructureSearchResults>(); // History of searches
		public static int InstanceCount = 0; // instance count

		/// <summary>
		/// Constructor
		/// </summary>
		public RelatedStructureSearchResults()
		{
			Id = InstanceCount++; // id of this instance
			return;
		}

		/// <summary>
		/// Look for existing results
		/// </summary>
		/// <param name="cid"></param>
		/// <param name="mtName"></param>
		/// <returns></returns>

		public static RelatedStructureSearchResults SearchHistory(
			string cid,
			string mtName)
		{
			foreach (RelatedStructureSearchResults rss0 in History)
			{
				if (rss0.Cid == cid && rss0.CidMtName == mtName)
				{
					return rss0;
				}
			}

			return null; // not found
		}

/// <summary>
/// ResetSearchTypeCounts
/// </summary>

		public void ResetBaseSearchTypeCounts()
		{
			AltCorpDbCnt = 0;
			MmpCorpDbCnt = 0;
			SwCorpDbCnt = 0;
			SimCorpDbCnt = 0;

			AltChemblCnt = 0;
			MmpChemblCnt = 0;
			SwChemblCnt = 0;
			SimChemblCnt = 0;

			DupCnt = 0;
		}

	} // RelatedStructureSearchResults

	public class RSSConfig
	{
		// Set RelatedStructureSearch.SearchFss true | false

		public static bool SearchFssEnabled = true;
		public static bool SearchCorpDbFssEnabled = true;
		public static bool SearchChemblFssEnabled = true;

		// Set RelatedStructureSearch.SearchMmp true | false

		public static bool SearchMmpEnabled = true;
		public static bool SearchCorpDbMmpEnabled = true;
		public static bool SearchChemblMmpEnabled = false; // chembl mmp data doesn't exist

		public static bool KeepHighestCidPairScoreOnlyEnabled = true; // if more than one row per pair of cids, keep the best one only

		// Set RelatedStructureSearch.SearchSmallWorld true | false

		public static bool SearchSmallWorldEnabled = true;
		public static bool SearchCorpDbSmallWorldEnabled = true;
		public static bool SearchChemblSmallWorldEnabled = true;

		// Set RelatedStructureSearch.SearchSim true | false

		public static bool SearchSimEnabled = true;
		public static bool SearchCorpSimEnabled = true;
		public static bool SearchChemblSimEnabled = true;

		// Set RelatedStructureSearch.SearchSSSS true | false

		public static bool SearchSssEnabled = true;
		public static bool SearchCorpSssEnabled = true;
		public static bool SearchChemblSssEnabled = true;

		public static bool UseMultipleThreads = true; // use multiple threads for structure searches where multithreading is supported
		public static bool UseCachedResults = true; // use cached search results to improve performance
		public static bool Debug = false; // log debug info	

		public static bool ConfigSettingsHaveBeenRead = false;

		/// <summary>
		/// Read and set the config settings
		/// </summary>

		public static void ReadConfigSettings()
		{
			if (ConfigSettingsHaveBeenRead) return;

			SearchFssEnabled = ServicesIniFile.ReadBool("RssSearchFss", true);
			SearchCorpDbFssEnabled = ServicesIniFile.ReadBool("RssSearchCorpFss", true);
			SearchChemblFssEnabled = ServicesIniFile.ReadBool("RssSearchChemblFss", true);

			SearchMmpEnabled = ServicesIniFile.ReadBool("RssSearchMmp", true);
			SearchCorpDbMmpEnabled = ServicesIniFile.ReadBool("RssSearchCorpMmp", true);
			SearchChemblMmpEnabled = ServicesIniFile.ReadBool("RssSearchChemblMmp", false);
			KeepHighestCidPairScoreOnlyEnabled = ServicesIniFile.ReadBool("RssKeepHighestCidPairScoreOnly", true);

			SearchSmallWorldEnabled = ServicesIniFile.ReadBool("RssSearchSmallWorld", true);
			SearchCorpDbSmallWorldEnabled = ServicesIniFile.ReadBool("RssSearchCorpSmallWorld", true);
			SearchChemblSmallWorldEnabled = ServicesIniFile.ReadBool("RssSearchChemblSmallWorld", true);

			//SearchSmallWorldEnabled = SearchCorpSmallWorldEnabled = SearchChemblSmallWorldEnabled = true; // for testing

			SearchSimEnabled = ServicesIniFile.ReadBool("RssSearchSim", true);
			SearchCorpSimEnabled = ServicesIniFile.ReadBool("RssSearchCorpSim", true);
			SearchChemblSimEnabled = ServicesIniFile.ReadBool("RssSearchChemblSim", true);

			SearchSssEnabled = ServicesIniFile.ReadBool("RssSearchSSS", true);
			SearchCorpSssEnabled = ServicesIniFile.ReadBool("RssSearchCorpSSS", true);
			SearchChemblSssEnabled = ServicesIniFile.ReadBool("RssSearchChemblSSS", true);

			UseMultipleThreads = ServicesIniFile.ReadBool("RssUseMultipleThreads", true);
			UseCachedResults = ServicesIniFile.ReadBool("RssUseCachedResults", true);
			Debug = ServicesIniFile.ReadBool("RssDebug", false);

			ConfigSettingsHaveBeenRead = true;
			return;
		}
	} // RSSConfig



}
