using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;
using Mobius.MolLib1;
using Mobius.CdkMx;

using Lucene.Net.Util;

using cdk = org.openscience.cdk;
using org.openscience.cdk;
using org.openscience.cdk.inchi;
using org.openscience.cdk.interfaces;
using org.openscience.cdk.fingerprint;
using org.openscience.cdk.smiles;
using org.openscience.cdk.tools.manipulator;
using org.openscience.cdk.graph;
using org.openscience.cdk.qsar.result;
using org.openscience.cdk.io;
using org.openscience.cdk.io.iterator;
using org.openscience.cdk.tools;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text;

namespace Mobius.CdkSearchMx
{
	public class CdkSimSearchMx 
	{
		//////////////// Public Members ///////////////////////

		public FingerprintType FingerprintType = FingerprintType.Undefined; // type of fingerprint to use

		public double MinimumSimilarity = MoleculeMx.DefaultMinSimScore;
		public int MaxHits = MoleculeMx.DefaultMaxSimHits;

		public HashSet<string> SearchKeySubset = null; // keys to subset the search on
		public HashSet<string> KeysToExclude = null; // keys to be excluded from search results

		public bool GetCorpSim = true;
		public bool GetChemblSim = true;

		public IAtomContainer QueryMol; // Query molecule object
		public FingerprintMx QueryFp; // Query fingerprint object

		public long[] QueryFpLongArray;
		public int QueryFpCardinality;

		///////////////// Private Members //////////////////////

		static bool SearchCorpSimEnabled = true;
		static bool SearchChemblSimEnabled = true;

		static bool UseMultipleThreads = true;
		Exception ThreadException = null; // any exception seen within a thread stored here
		static bool Debug = false;

		static ASCIIEncoding AsciiEncodingInstance = new ASCIIEncoding();

		CountdownEvent CountDownLatch;
		FingerprintDao FpDao;
		FileStream[] FileStreamReaders;

		List<StructSearchMatch>[] FileMatchLists; // List of matches for each reader
		public List<StructSearchMatch> MatchList; // merged list of matches

		/// <summary>
		/// InitializeForSession
		/// </summary>

		public static void InitializeForSession()
		{
			SearchCorpSimEnabled = ServicesIniFile.ReadBool("CdkSimSearchCorp", true);
			SearchChemblSimEnabled = ServicesIniFile.ReadBool("CdkSimSearchChembl", true);
			UseMultipleThreads = ServicesIniFile.ReadBool("CdkSimSearchUseMultipleThreads", true);
			Debug = ServicesIniFile.ReadBool("CdkSimSearchDebug", false);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public CdkSimSearchMx()
		{
			return;
		}

		/// <summary>
		/// Check if searching available
		/// </summary>
		/// <returns></returns>

		public static bool IsSearchingAvailable()
		{
			return SearchCorpSimEnabled; // just consider Corp DB
		}

		/// <summary>
		/// ExecuteSearch
		/// </summary>
		/// <param name="queryMolfile"></param>
		/// <param name="databases"></param>
		/// <param name="minimumSimilarity"></param>
		/// <param name="maxHits"></param>
		/// <returns></returns>

		public List<StructSearchMatch> ExecuteSearch(
			string queryMolfile,
			string databases,
			FingerprintType fpType,
			double minimumSimilarity,
			int maxHits)
		{
			int fragCnt;

			IAtomContainer query = CdkUtil.MolfileToAtomContainer(queryMolfile);
			IAtomContainer largestFrag = CdkUtil.GetLargestMoleculeFragment(query, out fragCnt);

			GetCorpSim = Lex.Contains(databases, "corp");

			GetChemblSim = Lex.Contains(databases, "chembl");

			if (!GetCorpSim && !GetChemblSim) throw new Exception("Invalid database: " + databases);

			FingerprintType = fpType;

			if (FingerprintType != FingerprintType.MACCS && FingerprintType != FingerprintType.Circular)
				throw new Exception("Invalid FingerprintType: " + FingerprintType);

			MinimumSimilarity = minimumSimilarity;
			MaxHits = maxHits;

			List<StructSearchMatch> matches = ExecuteSearch(largestFrag);
			return matches;
		}

		/// <summary>
		/// ExecuteSearch
		/// </summary>
		/// <param name="queryMol"></param>

		public List<StructSearchMatch> ExecuteSearch(
			IAtomContainer queryMol)
		{
			AssertMx.IsTrue(FingerprintType == FingerprintType.MACCS || FingerprintType == FingerprintType.Circular, 
				"Invalid FingerprintType: " + FingerprintType);

			QueryMol = queryMol;

			BitSetFingerprint fp = // generate a fingerprint
				CdkUtil.BuildBitSetFingerprintForLargestFragment(queryMol, FingerprintType);

			QueryFpCardinality = fp.cardinality();
			QueryFpLongArray = fp.asBitSet().toLongArray();

			MatchList = new List<StructSearchMatch>();
			ThreadException = null;

			foreach (string databaseName in FingerprintDbMx.Databases) // loop on all databases
			{
				int srcId = -1;
				if (Lex.Contains(databaseName, "corp"))
				{
					if (!GetCorpSim) continue;
					srcId = StructSearchMatch.CorpDbId;
				}

				else if (Lex.Contains(databaseName, "chembl"))
				{
					if (!GetChemblSim) continue;
					srcId = StructSearchMatch.ChemblDbId;
				}

				if (Debug) DebugLog.Message("Starting sim search on " + databaseName + " database");

				FpDao = new FingerprintDao(databaseName, FingerprintType);

				if (!FpDao.DataFilesExist()) continue; // no files for this database

				FileStreamReaders = FpDao.OpenReaders();
				FileMatchLists = new List<StructSearchMatch>[FileStreamReaders.Length];
				for (int i1 = 0; i1 < FileMatchLists.Length; i1++)
					FileMatchLists[i1] = new List<StructSearchMatch>();

				DateTime t0 = DateTime.Now;

				if (UseMultipleThreads) ExecuteMultiThreadSearch();
				else ExecuteSingleThreadSearch();

				double et = TimeOfDay.Delta(ref t0);

				FpDao.CloseReaders();

				List<StructSearchMatch> matchList = MergeIndividualFileMatchLists();

				if (KeysToExclude != null || SearchKeySubset != null) // filter by any allowed/disallowed keys
				{
					List<StructSearchMatch> matchList2 = new List<StructSearchMatch>(); 

					foreach (StructSearchMatch m0 in matchList)
					{
						if (KeysToExclude != null && KeysToExclude.Contains(m0.SrcCid))
							continue;

						if (SearchKeySubset != null && !SearchKeySubset.Contains(m0.SrcCid))
							continue;

						matchList2.Add(m0);
					}

					matchList = matchList2;
				}

				matchList.Sort(StructSearchMatch.CompareByMatchQuality);

				//int removeCount = matchList.Count - MaxHits; // limit to maxhits per database
				//if (removeCount > 0) 
				//	matchList.RemoveRange(MaxHits, removeCount);

				//foreach (StructSearchMatch ssm0 in matchList)
				//	if (ssm0.SrcId != srcId) ssm0.SrcId = srcId; // debug

				MatchList.AddRange(matchList);

				double et2 = TimeOfDay.Delta(ref t0);

				string msg =
					string.Format("Search complete (" + databaseName + ").Time : {0:0.00} ", et) +
					string.Format("{0} Hits: ", FileMatchLists[0].Count);

				if (Debug) DebugLog.Message(msg);

				for (int hi = 0; hi < 5 && hi < FileMatchLists[0].Count; hi++)
				{
					StructSearchMatch sm = FileMatchLists[0][hi];
					msg += sm.SrcCid + string.Format(" = {0:0.00}, ", sm.MatchScore);
				}
			} // database loop

			if (ThreadException != null) throw new Exception(ThreadException.Message, ThreadException);

			MatchList.Sort( // sort by decreasing sim value
				delegate (StructSearchMatch p1, StructSearchMatch p2)
				{ return p2.MatchScore.CompareTo(p1.MatchScore); });

			if (MaxHits > 0 && MatchList.Count > MaxHits) // remove hits beyond maximum if defined
				MatchList.RemoveRange(MaxHits, MatchList.Count - MaxHits);

			//ShowProgress(msg);
			//Thread.Sleep(10000000);
			return MatchList;
		}

		/// <summary>
		/// ShowProgress
		/// </summary>
		/// <param name="msg"></param>

		public static void ShowProgress(string msg)
		{
			if (WindowsHelper.InvokeShowProgressDelegateInstance != null)
				WindowsHelper.InvokeShowProgressDelegateInstance(msg);
		}

		/// <summary>
		/// Merge MatchLists for each file that was searched taking best sim score for each compound
		/// and then sort by decreasing sim score
		/// </summary>
		List<StructSearchMatch> MergeIndividualFileMatchLists()
		{
			Dictionary<string, StructSearchMatch> matchDict = new Dictionary<string, StructSearchMatch>();

			for (int smlIdx = 0; smlIdx < FileMatchLists.Length; smlIdx++)
			{
				List<StructSearchMatch> sml = FileMatchLists[smlIdx];
				if (sml == null) continue;

				for (int smIdx = 0; smIdx < sml.Count; smIdx++)
				{
					StructSearchMatch sm = sml[smIdx];
					if (sm == null) continue;

					string key = sm.SrcDbId.ToString() + "." + sm.SrcCid;
					if (matchDict.ContainsKey(key))
					{
						if (sm.MatchScore > matchDict[key].MatchScore) // take largest similarity value
							matchDict[key] = sm;
					}

					else matchDict[key] = sm;
				}
			}

			List<StructSearchMatch> matchList = new List<StructSearchMatch>(matchDict.Values);

			return matchList;
		}

		public void ExecuteSingleThreadSearch()
		{
			for (int ti = 0; ti < FileStreamReaders.Length; ti++)
			{
				SearchSingleFile(ti);
			}

			return;
		}

		/// <summary>
		/// Start one thread for each file and search in parallel
		/// </summary>

		void ExecuteMultiThreadSearch()
		{
			ThreadStart ts = null;
			Thread newThread = null;

			CountDownLatch = new CountdownEvent(FileStreamReaders.Length);

			for (int bri = 0; bri < FileStreamReaders.Length; bri++)
			{
				Thread t = new Thread(new ParameterizedThreadStart(SearchFileThreadMethod));
				t.Name = "SimSearchThread";
				t.IsBackground = true;
				t.SetApartmentState(ApartmentState.STA);
				t.Start(bri);
			}

			CountDownLatch.Wait(); // wait until all threads are complete

			return;
		}

		/// <summary>
		/// SearchFileThreadMethod
		/// </summary>
		/// <param name="fileIndexArg"></param>
		void SearchFileThreadMethod(object fileIndexArg)
		{
			try
			{
				int fi = (int)fileIndexArg;
				SearchSingleFile(fi);
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
		/// Search a single file
		/// </summary>

		void SearchSingleFile(int fi)
		{
			StructSearchMatch sm = null;

			AssertMx.IsNotNull(FpDao, "FpDao");

			List<StructSearchMatch> matchList = FileMatchLists[fi];
			AssertMx.IsNotNull(matchList, "matchList");

			OpenBitSet queryObs = new OpenBitSet(QueryFpLongArray, QueryFpLongArray.Length);
			AssertMx.IsNotNull(queryObs, "queryObs");

			OpenBitSet dbObs = new OpenBitSet(QueryFpLongArray, QueryFpLongArray.Length); // gets set to DB fp for intersect
			AssertMx.IsNotNull(dbObs, "dbObs");

			FileStream fs = FileStreamReaders[fi];
			AssertMx.IsNotNull(fs, "fs");

			ReadFingerprintRecArgs a = new ReadFingerprintRecArgs();
			a.Initialize(fs, QueryFpLongArray.Length);

			try
			{
				while (true)
				{
					bool readOk = FpDao.ReadRawFingerprintRec(a);
					if (!readOk) break;

					//if (IsSrcCidMatch("03435269", a)) a = a;  // debug

					dbObs.Bits = a.fingerprint;
					dbObs.Intersect(queryObs);
					int commonCnt = (int)dbObs.Cardinality();
					float simScore = commonCnt / (float)(a.cardinality + QueryFpCardinality - commonCnt);

					if (simScore >= MinimumSimilarity)
					{
						sm = ReadFingerprintRec_To_StructSearchMatch(a);
						sm.SearchType = StructureSearchType.MolSim;
						sm.MatchScore = simScore;

						matchList.Add(sm);
					}
				}
			}

			catch (Exception ex)
			{
				string msg = ex.Message;
				msg += string.Format("\r\nfi: {0}, fs.Name: {1}, sm: {2}", fi, fs.Name, sm!= null ? sm.Serialize() : "");
				DebugLog.Message(DebugLog.FormatExceptionMessage(ex, msg));
				throw new Exception(msg, ex);
			}

			return;
		}

		/// <summary>
		/// Find Cid for debug purposes
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>

		bool IsSrcCidMatch(
			string cid,
			ReadFingerprintRecArgs a)
		{
			StructSearchMatch sm = ReadFingerprintRec_To_StructSearchMatch(a);
			if (Lex.Eq(sm.SrcCid, cid)) return true;
			else return false;
		}

		StructSearchMatch ReadFingerprintRec_To_StructSearchMatch(
			ReadFingerprintRecArgs a)
		{
			StructSearchMatch sm = new StructSearchMatch();
			sm.SearchType = StructureSearchType.MolSim;
			sm.SrcDbId = a.src;
			sm.SrcCid = AsciiEncodingInstance.GetString(a.cidBytes, 0, a.cidLength);
			if (sm.SrcDbId == 0)
				sm.SrcCid = CompoundId.NormalizeForDatabase(sm.SrcCid); // be sure CorpIds are proper length
			return sm;
		}


		/// <summary>
		/// Calculate similarity score between a pair of FingerprintMxs
		/// </summary>
		/// <param name="fp1"></param>
		/// <param name="fp2"></param>
		/// <returns></returns>

		public static float CalculateFingerprintPairSimilarityScore(
			FingerprintMx fp1,
			FingerprintMx fp2)
		{
			long[] fp1Array = fp1.ToLongArray();
			OpenBitSet fp1BitSet = new OpenBitSet(fp1Array, fp1Array.Length);
			int fp1Card = (int)fp1BitSet.Cardinality();

			long[] fp2Array = fp2.ToLongArray();
			OpenBitSet fp2BitSet = new OpenBitSet(fp2Array, fp2Array.Length);
			int fp2Card = (int)fp2BitSet.Cardinality();

			fp2BitSet.Intersect(fp1BitSet);
			int commonCnt = (int)fp2BitSet.Cardinality();
			float simScore = commonCnt / (float)(fp1Card + fp2Card - commonCnt);
			return simScore;
		}

		/// <summary>
		/// PruneFingerprint
		/// </summary>
		/// <param name="qfp"></param>
		/// <param name="dbfp"></param>
		/// <param name="threshold"></param>
		public void PruneFingerprint(
			FingerprintMx qfp,
			FingerprintMx dbfp,
			double threshold)
		{
			int[] qSetBits = qfp.OnBits;
			int qn = qfp.Cardinality; // # Number of bits in query fingerprint
			int qmin = (int)(Math.Ceiling(qn * threshold)); // # Minimum number of bits in results fingerprints
			int qmax = (int)(qn / threshold); // # Maximum number of bits in results fingerprints
			int ncommon = qn - qmin + 1; // # Number of fingerprint bits in which at least one must be in common

			// Get list of bits where at least one must be in result fp. Use least popular bits if possible.

			//if (db.mfp_counts)
			//{
			//	reqbits = [count['_id'] for count in db.mfp_counts.find({ '_id': { '$in': qfp} }).sort('count', 1).limit(ncommon)];
			//}
			//else
			//{
			//	reqbits = qfp[:ncommon];
			//}
			//results = [];
			//for (fp in db.molecules.find({ 'mfp.bits': { '$in': reqbits}, 'mfp.count': { '$gte': qmin, '$lte': qmax} }))
			// {
			//	intersection = len(set(qfp) & set(fp['mfp']['bits']));
			//	pn = fp['mfp']['count'];
			//	tanimoto = float(intersection) / (pn + qn - intersection);
			//	if (tanimoto >= threshold)
			//	{
			//		results.append((tanimoto, fp['chembl_id'], fp['smiles']));
			//	}
			//}
			//return results;
		}

		//    # Get list of bits where at least one must be in result fp. Use least popular bits if possible.
		//    if db.mfp_counts:
		//        reqbits = [count['_id'] for count in db.mfp_counts.find({'_id': {'$in': qfp
		//	}
		//}).sort('count', 1).limit(ncommon)]
		//    else:
		//        reqbits = qfp[:ncommon]
		//		results = []
		//    for fp in db.molecules.find({'mfp.bits': {'$in': reqbits}, 'mfp.count': {'$gte': qmin, '$lte': qmax}}):
		//        intersection = len(set(qfp) & set(fp['mfp']['bits']))
		//        pn = fp['mfp']['count']
		//				tanimoto = float(intersection) / (pn + qn - intersection)
		//        if tanimoto >= threshold:
		//            results.append((tanimoto, fp['chembl_id'], fp['smiles']))
		//    return results


		/// <summary>
		/// Test to calculate similarity between pairs of structures (Alt syntax: Call SimSearchMx.Test [cid1] [cid2]
		///  Tautomers: 
		///  Isotopes: 
		///  Neg Counterion, (Cl-) with 2 quatternary N+ with H attached in main frag:
		///  Neg Counterion, (I-) with quatternary N+ (no attached H) in main frag:
		///  Pos Counterion (Li+) with O- in main frag:
		///  Benzene, cyclohexane: 
		///  StereoIsomers: 
		///  StereoIsomers: 
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public static string Test(string args)
		{
			string[] sa = args.Split(' ');
			string cid1 = CompoundId.Normalize(sa[0]);
			string cid2 = CompoundId.Normalize(sa[1]);

			int smiLen = 40;

			MoleculeMx s1 = MoleculeMx.SelectMoleculeForCid(cid1);
			IAtomContainer mol = CdkUtil.MolfileToAtomContainer(s1.GetMolfileString());
			UniChemData ucd1 = UniChemUtil.BuildUniChemData(mol);
			List<FingerprintMx> fps1 = UniChemDataToFingerprintMxList(ucd1);

			MoleculeMx s2 = MoleculeMx.SelectMoleculeForCid(cid2);
			IAtomContainer mol2 = CdkUtil.MolfileToAtomContainer(s2.GetMolfileString());
			UniChemData ucd2 = UniChemUtil.BuildUniChemData(mol2);
			List<FingerprintMx> fps2 = UniChemDataToFingerprintMxList(ucd2);

			string fps2Smiles = (sa[0] + " / " + sa[1]).PadRight(smiLen);
			string scores = "";
			for (int i1 = 0; i1 < fps1.Count; i1++)
			{
				FingerprintMx fp1 = fps1[i1];
				for (int i2 = 0; i2 < fps2.Count; i2++)
				{
					FingerprintMx fp2 = fps2[i2];

					if (i1 == 0) // build smiles headers of cid2 frags if first cid1 frag
						fps2Smiles += "\t" + fp2.CanonSmiles.PadRight(smiLen);

					if (i2 == 0)
						scores += "\r\n" + fp1.CanonSmiles.PadRight(smiLen); // include smiles at start of each line

					float simScore = CalculateFingerprintPairSimilarityScore(fp1, fp2);
					scores += "\t" + string.Format("{0:0.00}", simScore).PadRight(smiLen);
				}
			}

			scores = fps2Smiles + scores;

			FileUtil.WriteAndOpenTextDocument("SimilarityScores", scores);

			return "";
		}

		/// <summary>
		///  UniChemDataToFingerprintMxList
		/// </summary>
		/// <param name="ucd"></param>
		/// <returns>FingerprintMx list </returns>

		public static List<FingerprintMx> UniChemDataToFingerprintMxList(UniChemData ucd)
		{
			List<FingerprintMx> fps = new List<FingerprintMx>();
			FingerprintMx fp = new FingerprintMx();
			fp.CdkFp = ucd.Fingerprint;
			fp.CanonSmiles = ucd.CanonSmiles;
			fps.Add(fp);

			foreach (UniChemFIKHBHierarchy child in ucd.Children)
			{
				fp = new FingerprintMx();
				fp.CdkFp = child.Fingerprint;
				fp.CanonSmiles = child.CanonSmiles;
				fps.Add(fp);
			}

			return fps;
		}


	} // class SimSearchMx

}
