using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using Mobius.MolLib1;
using Mobius.MolLib2;
using Mobius.CdkMx;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Mobius.QueryEngineLibrary
{
	public class SasMapMetaBroker : GenericMetaBroker
	{
		SasMapParms SMP = new SasMapParms(); // parameters defining this analysis

		List<object[]> VoList = new List<object[]>();
		int VoListPos = -1; // current position in Vo

		string LastCriteriaString = "";

		bool Debug = false;

		// Constructor

		public SasMapMetaBroker()
		{
			return;
		}

		/// <summary>
		/// We will transform table at presearch time
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		public override bool ShouldPresearchCheckAndTransform(
			MetaTable mt)
		{
			return false;
		}

		/// <summary>
		/// Validate Rgroup decomposition & build proper query table
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="q"></param>
		/// <param name="resultKeys"></param>

		public override void DoPreSearchTransformation(
			Query originalQuery,
			QueryTable originalQt,
			Query newQuery)
		{
			//int ri;

			//QueryTable qt = originalQt.Clone(); // make copy we can modify
			//SetupQueryTableForRGroupDecomposition(qt, newQuery);
			//newQuery.AddQueryTableUnique(qt); // store replacement query table

			return;
		}

		/// <summary>
		/// Prepare for execution
		/// </summary>
		/// <param name="eqp"></param>
		/// <returns></returns>

		public override string PrepareQuery(
		ExecuteQueryParms eqp)
		{
			Eqp = eqp;
			Qt = eqp.QueryTable;

			//PrepareForDecompose(Qt, eqp.Qe.Query); // setup again for this broker instance

			SelectList = new List<MetaColumn>();
			foreach (QueryColumn qc in Qt.QueryColumns)
			{
				if (qc.Selected || qc.IsKey)
					SelectList.Add(qc.MetaColumn);
			}
			return ""; // No SQL returned
		}

		/// <summary>
		/// Execute query
		/// </summary>
		/// <param name="eqp"></param>

		public override void ExecuteQuery(
				ExecuteQueryParms eqp)
		{
			MetaTable mt;
			MetaColumn mc;
			Query q;
			QueryTable qt;
			QueryColumn qc;
			ResultsPage rp;
			ResultsViewProps view;
			CompoundStructureActivityData cd1, cd2;

			bool smallerIsbetter;
			double r1, r2, r3, r4;
			int di, di2, pdi, pdi2, i3;
			string tok;

			qt = eqp.QueryTable;
			qc = qt.GetQueryColumnByNameWithException(SasMapParms.ParametersMetaColumnName);
			AssertMx.IsDefined(qc.Criteria, qc.Label + " criteria not defined");

			if (Lex.Eq(qc.Criteria, LastCriteriaString)) // if same criteria as last time then use existing data
			{
				VoListPos = -1; // init list position
				return;
			}

			VoList = new List<object[]>();
			VoListPos = -1; // init list position

			LastCriteriaString = qc.Criteria;
			ParsedSingleCriteria psc = ParsedSingleCriteria.Parse(qc);
			SMP = SasMapParms.Deserialize(psc.Value);

			mc = SMP.EndpointMc;
			smallerIsbetter = mc.MultiPoint;

			List<CompoundStructureActivityData> ds1 = ReadData(SMP); // read in the data to analyze
			if (ds1 == null || ds1.Count == 0) return; //  throw new QueryException("No data retrieved");

			List<CompoundStructureActivityData> ds2 = ds1; // just one set for now

			// Calculate difference or ratio coefficents for each pair

			List<PairData> pd = new List<PairData>();
			int minCoef = -1; // index of minimum coefficent selected so far
			double molFactor = AssayAttributes.GetMolarConcFactor(SMP.EndpointMc);

			for (di = 0; di < ds1.Count; di++)
			{ // process all compounds in 1st set
				//		if (ds1[di].Nearest == 0) continue; // any data?
				if (ds2 == ds1) di2 = di + 1; // only do lower rt diagonal if one dataset
				else di2 = 0; // must do all compares, check for dups later

				for ( /* start at di2 */; di2 < ds2.Count; di2++)
				{
					//			if (ds2[di2].Nearest == 0) continue; // any data?
					if (ds1[di].Cid == ds2[di2].Cid) continue; // avoid self compare

					double sim = // similarity
					 CalculateSimilarity(ds1[di], ds2[di2]);
					//if (sim==1.0 && !stereo) // eliminate stereo pairs if requested
					// continue; // a more careful check may be needed

					if (sim < SMP.MinimumSimilarity) continue; // below cutoff value?

					double denom = 1 - sim; // denominator is 1 - sim
					if (denom == 0) denom = .00000000001f; // avoid divide by zero

					double actChange = 0;

					if (smallerIsbetter && ds1[di].Activity < ds2[di2].Activity)
					{
						cd1 = ds1[di];
						cd2 = ds2[di2];
					}

					else
					{
						cd1 = ds2[di2];
						cd2 = ds1[di];
					}

					double a1 = cd1.Activity;
					double a2 = cd2.Activity;

					if (a1 == NullValue.NullNumber || a2 == NullValue.NullNumber)
						actChange = 0;

					else switch (SMP.ActDiffCalcType)
						{
							case ActDiffCalcType.SimpleDifference: // activity difference
								{
									actChange = a1 - a2;
									break;
								}

							case ActDiffCalcType.NegativeLog:
								{
									actChange = -Math.Log10(a1) - -Math.Log10(a2);
									break;
								}

							case ActDiffCalcType.MolarNegativeLog:
								{
									actChange = (-Math.Log10(a1 * molFactor)) - (-Math.Log10(a2 * molFactor));
									break;
								}

							case ActDiffCalcType.Ratio: // activity ratio
								{
									r1 = a1;
									if (r1 == 0) r1 = .00000000001f;
									r2 = a2;
									if (r2 == 0) r2 = .00000000001f;
									r3 = r1 / r2;
									r4 = r2 / r1;

									actChange = r3;
									if (SMP.UseAbsoluteValue && r4 > r3) // take the max value
										actChange = r4;

									break;
								}

							case ActDiffCalcType.Advanced:
								{
									throw new InvalidOperationException("SarMapCalcType.Advanced");
								}

							default:
								throw new InvalidOperationException("SarMapCalcType: " + (int)SMP.ActDiffCalcType);
						}

					if (SMP.UseAbsoluteValue && SMP.ActDiffCalcType != ActDiffCalcType.Ratio)
						actChange = Math.Abs(actChange);

					double coef = actChange / denom;

					if (pd.Count < SMP.MaxPairCount)  // just add this pair to end
					{
						pdi = pd.Count;
						pd.Add(new PairData());
					}

					else
					{ // see if this value is greater than anything in list
						if (minCoef < 0)
						{ // find element with minimum coef
							minCoef = 0;
							for (i3 = 1; i3 < pd.Count; i3++)
							{
								if (pd[i3].Coef < pd[minCoef].Coef)
									minCoef = i3;
							}
						}
						if (coef <= pd[minCoef].Coef) continue; // if this one better?

						//if (ds1 != ds2)
						//{ // be sure not a duplicate of what we have in list
						//	for (i3 = 0; i3 < pd.Count; i3++)
						//	{ // check for pair in list already
						//		if ((di == pd[i3].CD1 && di2 == pd[i3].CD2) ||
						//				(di == pd[i3].CD2 && di2 == pd[i3].CD1)) break;
						//	}
						//	if (i3 < pd.Count) continue; // continue to next pair if found
						//}

						pdi = minCoef; // replace this item
						minCoef = -1; // reset to get new minimum next time
					}

					// Save data for the pair

					pd[pdi].CD1 = cd1;
					pd[pdi].CD2 = cd2;
					pd[pdi].Sim = sim;
					pd[pdi].ActChange = actChange;
					pd[pdi].Coef = coef;
				}
			}

			// Build the list of pair Vos

			int voLen = qt.SetSimpleVoPositions();

			PairData pdItem;
			for (pdi = 1; pdi < pd.Count; pdi++) // sort from max to min coef value
			{
				pdItem = pd[pdi];
				for (pdi2 = pdi - 1; pdi2 >= 0; pdi2--)
				{
					if (pdItem.Coef < pd[pdi2].Coef) break;
					pd[pdi2 + 1] = pd[pdi2];
				}
				pd[pdi2 + 1] = pdItem;
			}

			for (pdi = 0; pdi < pd.Count; pdi++)
			{
				pdItem = pd[pdi];
				cd1 = pdItem.CD1;
				cd2 = pdItem.CD2;

				object[] vo = new object[voLen];

				VoArray.SetVo(qt, "PAIR_ID", vo, new NumberMx(pdi + 1));
				VoArray.SetVo(qt, "COMPOUND1", vo, new StringMx(cd1.Cid));
				VoArray.SetVo(qt, "STRUCTURE1", vo, cd1.Structure);
				VoArray.SetVo(qt, "ACTIVITY1", vo, new NumberMx(cd1.Activity));

				VoArray.SetVo(qt, "COMPOUND2", vo, new StringMx(cd2.Cid));
				VoArray.SetVo(qt, "STRUCTURE2", vo, cd2.Structure);
				VoArray.SetVo(qt, "ACTIVITY2", vo, new NumberMx(cd2.Activity));

				VoArray.SetVo(qt, "SIMILARITY", vo, new NumberMx(pdItem.Sim));
				VoArray.SetVo(qt, "ACTIVITY_DIFF", vo, new NumberMx(pdItem.ActChange));
				VoArray.SetVo(qt, "ACT_SIM_COEF", vo, new NumberMx(pdItem.Coef));

				VoList.Add(vo);
			}

			VoListPos = -1; // init list position
			return;
		}

		/// <summary>
		/// NextRow - Return the next matching row value object
		/// </summary>
		/// <returns></returns>

		public override Object[] NextRow()
		{
			if (VoListPos >= (VoList.Count - 1)) return null;

			VoListPos++;

			object[] vo = VoList[VoListPos];
			return vo;
		}

		/// <summary>
		/// Close broker & release resources
		/// </summary>

		public override void Close()
		{
			return; // noop
		}

		/// <summary>
		/// Ignore criteria because they are used as parameters for formatting Rgroup table
		/// not as search criteria.
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public override bool IgnoreCriteria(
			MetaTable mt)
		{
			return true; // ignore criteria, used as parameters for formatting Rgroup table
		}

		/// <summary>
		/// Build query to get summarization detail for RgroupMatrix data element
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="mc"></param>
		/// <param name="resultId">act_code.compound_id</param>
		/// <returns></returns>

		public override Query GetDrilldownDetailQuery(
			MetaTable mt,
			MetaColumn mc,
			int level,
			string resultId)
		{
			QueryColumn qc;

			// ResultId is of the form: queryId, mtName, mcName, sn1, sn2,...snn

			string[] sa = resultId.Split(',');

			int objectId = Int32.Parse(sa[0]);
			UserObject uo = UserObjectDao.Read(objectId);
			if (uo == null) return null; // no longer there
			Query q = Query.Deserialize(uo.Content);
			q.ResultKeys = null; // clear any set of result keys
			if (q.LogicType == QueryLogicType.Complex)
			{ // if complex logic then go simple (todo: fix to handle complex logic)
				q.ClearAllQueryColumnCriteria();
				q.LogicType = QueryLogicType.And;
			}
			q.KeyCriteria = "in (";
			for (int i1 = 3; i1 < sa.Length; i1++)
			{
				q.KeyCriteria += Lex.AddSingleQuotes(sa[i1]);
				if (i1 < sa.Length - 1) q.KeyCriteria += ",";
			}
			q.KeyCriteria += ")";
			return q;
		}

		/// <summary>
		/// Read input data from database
		/// </summary>
		/// <param name="smp">
		/// <returns></returns>

		List<CompoundStructureActivityData> ReadData(
			SasMapParms smp)
		{
			MetaColumn activityMc = smp.EndpointMc;
			QueryColumn keyCriteriaQc = smp.KeyCriteriaQc;

			AssertMx.IsNotNull(activityMc, "mc");
			AssertMx.IsNotNull(keyCriteriaQc, "keyCriteriaQc");

			MetaTable mt, mt2;
			MetaColumn mc2 = null;

			Query q = new Query();
			mt = activityMc.MetaTable;
			QueryTable qt = new QueryTable(mt);
			if (mt.SummarizedExists && !mt.UseSummarizedData)
			{ // retrieve summarized data if exists 
				mt2 = MetaTableCollection.Get(mt.Name + MetaTable.SummarySuffix);
				if (mt2 != null)
				{
					mc2 = mt2.GetMetaColumnByName(activityMc.Name);
					if (mc2 == null) mc2 = mt2.GetMetaColumnByLabel(activityMc.Label);
				}

				if (mc2 != null) // same column available in summarized?
				{
					mt = mt2;
					activityMc = mc2;
				}
			}

			SMP.KeyCriteriaQc.CopyCriteriaToQueryKeyCritera(q);
			q.KeyCriteriaDisplay = SMP.KeyCriteriaQc.CriteriaDisplay;

			qt.SelectKeyOnly();
			QueryColumn qc = qt.GetQueryColumnByName(activityMc.Name);
			qc.Selected = true;
			q.AddQueryTable(qt);

			QueryEngine qe = new QueryEngine();
			List<string> keyList = qe.ExecuteQuery(q); // note that keylist may be empty if single-step query

			HashSet<string> keySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			List<CompoundStructureActivityData> data = new List<CompoundStructureActivityData>();

			int rowCount = 0;
			while (true)
			{
				object[] vo = qe.NextRow();
				if (vo == null) break;
				CompoundStructureActivityData cd = new CompoundStructureActivityData();
				string cid = (string)vo[0];
				cd.Cid = cid;
				keySet.Add(cid); // accumulate keys

				object val = vo[2];
				if (NullValue.IsNull(val)) continue;
				if (val is double)
					cd.Activity = (double)val;
				else if (val is Int32)
					cd.Activity = (Int32)val;

				else if (val is NumberMx)
				{
					NumberMx nex = val as NumberMx;
					cd.Activity = nex.Value;
				}

				else if (val is QualifiedNumber)
				{
					QualifiedNumber qn = val as QualifiedNumber;
					cd.Activity = qn.NumberValue;
					//if (qn.Qualifier != null && qn.Qualifier != "" && qn.Qualifier != "=")
					//	continue; // (don't want to do this since may filter out good data (e.g. IC50 <0.0001))
				}

				else continue;

				if (cd.Activity == NullValue.NullNumber) continue;

				data.Add(cd);
				rowCount++;
			}

			// Retrieve structures

			keyList = new List<string>(keySet);
			Dictionary<string, MoleculeMx> csDict = MoleculeUtil.SelectMoleculesForCidList(keyList, qt.MetaTable); // get the structures in a single step

			// Add structures and build/store fingerprints to data

			DebugLog.Message("========== Fingerprints ============");

			foreach (CompoundStructureActivityData cd in data)
			{

				if (!csDict.ContainsKey(cd.Cid) || csDict[cd.Cid] == null) continue;

				if (cd.Cid == "111" || cd.Cid == "222") csDict = csDict; // debug

				MoleculeMx cs = csDict[cd.Cid];
				cd.Structure = cs;

				FingerprintType fpType = FingerprintType.Circular;
				int fpSubtype = -1;

				if (SMP.SimilarityType == SimilaritySearchType.ECFP4) // some issue with ECFP4?
				{
					fpType = FingerprintType.Circular;
					fpSubtype = CircularFingerprintType.ECFP4;
				}

				else if (SMP.SimilarityType == SimilaritySearchType.Normal)
				{
					fpType = FingerprintType.MACCS;
				}

				cd.BitsetFingerprint = cs.BuildBitSetFingerprint(fpType, fpSubtype);
				if (cd.BitsetFingerprint == null) continue; // couldn't build fingerprint (e.g. no structure)

				if (Debug) DebugLog.Message(cd.Cid + ": " + Lex.Join(StructureConverter.ICdkUtil.GetBitSet(cd.BitsetFingerprint), ", "));
			}

			return data;
		}

		/// <summary>
		/// Calculate Tanimoto similarity between two compounds
		/// </summary>
		/// <param name="fp1"></param>
		/// <param name="rp2"></param>
		/// <returns></returns>

		double CalculateSimilarity(
			CompoundStructureActivityData cd1,
			CompoundStructureActivityData cd2)
		{
			int l1 = -1, l2 = -1;

			if (cd1.BitsetFingerprint == null || cd2.BitsetFingerprint == null)
				return 0;

			if (Debug)
			{
				if ((cd1.Cid == "111" && cd2.Cid == "222") ||
				 (cd2.Cid == "222" && cd1.Cid == "111"))
				{
					DebugLog.Message("Before: " + cd1.Cid + ": " + String.Join(", ", StructureConverter.ICdkUtil.GetBitSet(cd1.BitsetFingerprint)));
					DebugLog.Message("Before: " + cd2.Cid + ": " + String.Join(", ", StructureConverter.ICdkUtil.GetBitSet(cd2.BitsetFingerprint)));
				}
			}

			double sim = MoleculeMx.CalculateBitSetFingerprintSimilarity(cd1.BitsetFingerprint, cd2.BitsetFingerprint);

			if (Debug)
			{
				if ((cd1.Cid == "111" && cd2.Cid == "222") ||
				 (cd2.Cid == "222" && cd1.Cid == "111"))
				{
					DebugLog.Message(cd1.Cid + ": " + String.Join(", ", StructureConverter.ICdkUtil.GetBitSet(cd1.BitsetFingerprint)));
					DebugLog.Message(cd2.Cid + ": " + String.Join(", ", StructureConverter.ICdkUtil.GetBitSet(cd2.BitsetFingerprint)));
					DebugLog.Message(cd1.Cid + ", " + cd2.Cid + " Similarity: " + sim);
				}
			}

			return sim;
		}

		/// <summary>
		/// Data on molecule needed to perform comparisons
		/// </summary>

		class CompoundStructureActivityData
		{
			public string Cid; // compound id
			public MoleculeMx Structure; // structure
			public object BitsetFingerprint; // Fingerprint (e.g. 960 MACCS keys, ECFP4)
			public double Activity; // activity value
			public double Nearest; // index of nearest?
		}

		/// <summary>
		/// Intermediate pair comparison data
		/// </summary>

		class PairData
		{
			public CompoundStructureActivityData CD1; // data for 1st compound ("most-active)
			public CompoundStructureActivityData CD2; // data for 2nd compound
			public double Sim; // pair similarity
			public double ActChange; // change in activity
			public double Coef; // activity/similarity coefficient
		}

	} // SasMapMetaBroker

}
