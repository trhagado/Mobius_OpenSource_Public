using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Mobius.QueryEngineLibrary
{

	/// <summary>
	/// Rgroup Decomposition MetaBroker
	/// </summary>

	public class RgroupMetaBroker : GenericMetaBroker
	{
		ICdkMol CoreMolecule = null; // single core molecule
		MoleculeMx CoreChemicalStructure = null;
		StructureMatcher StrMatcher = null; // structure matcher

		ICdkMol[] Substituents = new ICdkMol[32 + 1]; // substituents for current mapping 0 - 32

		int[] CoreRNumbers;
		int RgTotalCount; // total number of RGroup atoms in the core
		SortedDictionary<int, int> RgCounts; // dictionary keyed by the group number with values contains the count of instances of that group number
		string TerminateOptionString; // how to terminate mapping

		Dictionary<string, MoleculeMx> CidToStructureDict = null; // dictionary of target structures we are matching in this chunk

		int KeyListPos; // position in key subset we are currently at
		int MapCount = -1;
		int MapPos = -1; // position in set of maps for current key

		static ICdkMol CdkMolUtil => StaticCdkMol.I; // static molecule shortcut for utility methods

		// Constructor

		public RgroupMetaBroker()
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
			return true;
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
			int ri;

			QueryTable qt = originalQt.Clone(); // make copy we can modify

			SetupQueryTableForRGroupDecomposition(qt, newQuery);

			newQuery.AddQueryTableUnique(qt); // store replacement query table
			return;
		}

		/// <summary>
		/// SetupQueryTableForRGroupDecomposition
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="q"></param>

		public void SetupQueryTableForRGroupDecomposition(
			QueryTable qt,
			Query q)
		{
			Stopwatch sw = Stopwatch.StartNew();

			MetaTable mt = qt.MetaTable;

			QueryColumn qc = qt.GetQueryColumnByName("core");
			if (qc == null) throw new UserQueryException("Core column not defined in rgroup_decomposition table");
			string molfile = qc.MolString;
			if (molfile == null || molfile == "")
			{ // core not defined in Rgroup decomposition table, try to get from structure search in rest of query
				foreach (QueryTable qt3 in q.Tables)
				{
					if (!qt3.MetaTable.IsRootTable) continue;
					MetaColumn mc3 = qt3.MetaTable.FirstStructureMetaColumn;
					if (mc3 != null)
					{
						QueryColumn qc3 = qt3.GetQueryColumnByName(mc3.Name);
						molfile = qc3.MolString;
						break;
					}
				}
			}

			if (molfile == null || molfile == "")
				throw new UserQueryException("R-group decomposition core structure is not defined");

			MoleculeMx cs = new MoleculeMx(MoleculeFormat.Molfile, molfile);
			cs.GetCoreRGroupInfo(out RgCounts, out RgTotalCount);

			if (RgCounts.Count == 0 || (RgCounts.ContainsKey(0) && RgCounts.Count == 1))
				throw new UserQueryException("R-group decomposition core structure must contain at least one numbered R-group");

			qc = qt.GetQueryColumnByName("R1_Structure"); // update any core structure label
			if (qc == null) throw new UserQueryException("Can't find R1_Structure in " + mt.Label);
			if (qc.Label.IndexOf("\tChime=") > 0)
			{
				qc.Label = "R-group, Core\tChime=" + cs.GetChimeString(); // reference core in query col header label
				qc.MetaColumn.Width = 25;
			}

			SetRnToMatchR1(qt, "Structure"); // reset querycolumn selection & width to match R1
			SetRnToMatchR1(qt, "Smiles");
			SetRnToMatchR1(qt, "Formula");
			SetRnToMatchR1(qt, "Weight");
			SetRnToMatchR1(qt, "SubstNo");

			int msTime = (int)sw.ElapsedMilliseconds;
			//if (RGroupDecomp.Debug) DebugLog.Message("Time(ms): " + msTime);

			return;
		}


		/// <summary>
		/// Reset querycolumn selection & width for rgroups R2 - Rn to match R1
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="name"></param>

		void SetRnToMatchR1(
			QueryTable qt,
			string nameSuffix)
		{
			QueryColumn qc1 = qt.GetQueryColumnByName("R1_" + nameSuffix);
			for (int ri = 1; ri <= 32; ri++)
			{
				if (!RgCounts.ContainsKey(ri)) continue; // ignore if not defined in core
				if (ri > 8) throw new UserQueryException("R" + ri.ToString() + " exceeds the limit of R8");

				QueryColumn qc = qt.GetQueryColumnByName("R" + ri.ToString() + "_" + nameSuffix);
				if (ri == 1) // fix up label for R1
				{
					if (qc.Label == "") // replace generic R-group label with specific R1
						qc.Label = qc.MetaColumn.Label.Replace("R-group", "R1");
					else qc.Label = qc.Label.Replace("R-group", "R1");
				}

				else
				{
					qc.Selected = qc1.Selected;
					qc.DisplayWidth = qc1.DisplayWidth;
				}
			}
		}

		/// <summary>
		/// Do setup in preparation for decompose
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="q"></param>

		public void PrepareForDecompose(
			QueryTable qt,
			Query q)
		{
			throw new NotImplementedException();
#if false
			Stopwatch sw = Stopwatch.StartNew();

			MetaTable mt = qt.MetaTable;

			QueryColumn qc = qt.GetQueryColumnByName("core");
			if (qc == null) throw new UserQueryException("Core column not defined in rgroup_decomposition table");
			string molfile = qc.MolString;
			if (molfile == null || molfile == "")
			{ // core not defined in Rgroup decomposition table, try to get from structure search in rest of query
				foreach (QueryTable qt3 in q.Tables)
				{
					if (!qt3.MetaTable.IsRootTable) continue;
					MetaColumn mc3 = qt3.MetaTable.FirstStructureMetaColumn;
					if (mc3 != null)
					{
						QueryColumn qc3 = qt3.GetQueryColumnByName(mc3.Name);
						molfile = qc3.MolString;
						break;
					}
				}
			}

			if (molfile == null || molfile == "")
				throw new UserQueryException("R-group decomposition core structure is not defined");

			bool allowHydrogenSubstituents = true;
			qc = qt.GetQueryColumnByName("AllowHydrogenSubstituents");
			if (Lex.Contains(qc?.Criteria, "'N'")) allowHydrogenSubstituents = false; // set to false if "no" criteria specified

			TerminateOptionString = "First mapping";
			bool allowMultipleCoreMappings = false;

			qc = qt.GetQueryColumnByName("Terminate_Option");
			if (qc != null && qc.Criteria != "")
			{
				ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(qc.Criteria);
				TerminateOptionString = psc.Value;
				allowMultipleCoreMappings = Lex.Contains(TerminateOptionString, "All");
			}

			MoleculeMx cs = new MoleculeMx(MoleculeFormat.Molfile, molfile);
			cs.GetCoreRGroupInfo(out RgCounts, out RgTotalCount); // need to redefine these

			RGroupDecomp.Initialize();

			RGroupDecomp.SetAlignStructuresToCore(true);
			RGroupDecomp.SetAllowMultipleCoreMappings(allowMultipleCoreMappings);
			RGroupDecomp.SetIncludeHydrogenFragments(allowHydrogenSubstituents);

			if (DebugMx.True) // debug
			{
				String coreSmiles = "[R1]c1cn2c3c(c1= O)cc(c(c3SC(C2)[R4])[R3])[R2]";
				string coreChime = "CYAAFQwAUewQeV58YHemfM^EQqPRfIYlJGEkx6M7e4db95jjK5HrNFVP2e1qHphWPL98KvcvrsF7bP9bRcW4QH$si9PXkkuwre6Q5UkHZjciQqeAKVLSHNAeQTAMlkiXEBD$BePpbNQCPD3BkklFEaQqwokeI$FwUoS5cAixQXkMbLTWDaAK7t7cOkSmt3RhwQedbrba6OHKBq3OF4Dhlz$0BfLeKCUUo8ixle176M2aIzXUccTOU8Xy8ARwE3bTyMfjuI3UunZceJf4iZELvsj3PX2MHZG73baUvQGS4DaxUaBGps81PPiDljfvxwFv8OfdOyIRlOBeuEAvk2rT9SRT6oMZIV6UtLFvmCHdwKnu9WtrfV1rEctydNUVxW4qKVlV0rZtpK1oZXuKcv6WVdl6r2hrjVLxBhblTVKO7w1qGRoziquOnPQkKd9H4EQfV0rP6mzI8Au8ulti2fu3YKB94lPXftPGbwr5yA";
				IMolLib coreMol = MolLibFactory.NewMolLib(MoleculeFormat.Molfile, molfile);
				coreChime = MoleculeMx.MolfileStringToChimeString(molfile);
				molfile = coreMol.MolfileString;
			}

			CoreMolecule = CdkMol.Util.MolfileStringToMolecule(molfile);
			RGroupDecomp.SetCoreStructure(CoreMolecule, false);

			CoreChemicalStructure = new MoleculeMx(MoleculeFormat.Molfile, molfile);
			StrMatcher = null;

			int msTime = (int)sw.ElapsedMilliseconds;
			if (RGroupDecomp.Debug) DebugLog.Message("Time(ms): " + msTime);

			return;
#endif
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

			PrepareForDecompose(Qt, eqp.Qe.Query); // setup again for this broker instance

			SelectList = new List<MetaColumn>();
			foreach (QueryColumn qc in Qt.QueryColumns)
			{
				if (qc.Selected || qc.IsKey)
					SelectList.Add(qc.MetaColumn);
			}
			return ""; // No SQL returned
		}

		/// <summary>
		/// Execute query for new set of keys
		/// </summary>
		/// <param name="eqp"></param>

		public override void ExecuteQuery(
			ExecuteQueryParms eqp)
		{
			CidToStructureDict = null;
			KeyListPos = -1;
			MapPos = -1;
			return;
		}

		/// <summary>
		/// NextRow - Return the next matching row value object
		/// </summary>
		/// <returns></returns>

		public override Object[] NextRow()
		{
			throw new NotImplementedException();
#if false
			string cid = "";
			Molecule substituent = null;

			Stopwatch swTotal = Stopwatch.StartNew();
			Stopwatch sw = Stopwatch.StartNew();

			if (Eqp.SearchKeySubset == null || Eqp.SearchKeySubset.Count == 0) return null;

			// Get the structures for the set of keys

			if (CidToStructureDict == null) 
			{
				CidToStructureDict = MoleculeUtil.SelectMoleculesForCidList(Eqp.SearchKeySubset);
				KeyListPos = -1;
				MapPos = -1;
				int msTime = (int)sw.ElapsedMilliseconds;
				if (RGroupDecomp.Debug) DebugLog.Message("Select " + Eqp.SearchKeySubset.Count + " structures time(ms): " + msTime);
			}

			// Get the next match

			while (true)
			{
				if (KeyListPos < 0 || MapPos + 1 >= MapCount)
				{ // go to next structure & set up mapping

					KeyListPos++;
					if (KeyListPos >= Eqp.SearchKeySubset.Count) return null;
					cid = Eqp.SearchKeySubset[KeyListPos];
					if (!CidToStructureDict.ContainsKey(cid)) continue;

					MoleculeMx cs = CidToStructureDict[cid];
					string molFile = cs.GetMolfileString();
					//string chime = cs.ChimeString; // debug
					if (RGroupDecomp.Debug) DebugLog.StopwatchMessage("t1", sw);

					if (DebugMx.False) // debug
					{
						//molFile = FileUtil.ReadFile(@"C:\Downloads\RGroupTarget.mol"); 
						string targetSmiles = "CC1Cn2cc(c(= O)c3c2c(c(c(c3)F)Cl)S1)C(= O)O";
						string targetChime = "CYAAFQwADfwQ19aXPcZERR45lQkn08$hZNXzeJ2yaAhDnxxJou4Gq9od8VG1ykiO63fQpvM8W4C6MR$O3VaZjQwrGr5weW3y^BeUEezndoIivvAbQN58EEHVMAsdPaF4LIsqsf$OCBUPHI5njBB2LIBy3i2cwbrD8T8kFVBVWkTCfIUFUtblI0G7vYiEL^svUWCT^m6tF18I7ISJUp^7WkuzpT9LrBSJLmMl5hHXog$68Q6YPb0^Xp0ftxmy7FDSF^sWib6^JrUMhrHtLfJ3yVMTm9RIrvvKMRQvKqq4G1Ooze5pdlpSdzp7MFl0K1zx4tdnNFoUO1kRPpyZks61qbBz2tU0L$svvojoU4yUlf$^MFF0nqSfGVW2PKv9TReb$knLXffdAAhqBN310WUfdJDoQBNX1a5L2uj9ybNNRLYpaZN1p6WYp2WI^ntQEVTBaJF1Uu28N4o2xudURpVITTKO7omUtpgLoaoOQHazmZG3k^aHPUOQfE0d27eAbE^uxcQUAB";
						molFile = CdkMol.Util.MoleculeTofMolfileString(CdkMol.Util.ChimeStringToMolecule(targetChime));
					}

					Molecule target = CdkMol.Util.MolfileStringToMolecule(molFile);
					if (RGroupDecomp.Debug) DebugLog.StopwatchMessage("t2", sw);

					MapCount = RGroupDecomp.ProcessTargetMolecule(target); // process & get number of maps
					if (MapCount > 1) MapCount = MapCount; // debug, seems to always be 1
					MapPos = -1;

					if (RGroupDecomp.Debug) DebugLog.StopwatchMessage("t3", sw);
				}

				// Get the substituent for each RGroup for the next mapping

				MapPos++; // get data for next mapping

				for (int si = 0; si < Substituents.Length; si++) // clear substituents
					Substituents[si] = null;

				for (int fi = 0; fi < RgTotalCount; fi++)
				{
					Molecule fragment = RGroupDecomp.GetIthMappingFragment(MapPos, fi);
					if (fragment == null) break; // must have reached the end

					if (RGroupDecomp.Debug) DebugLog.StopwatchMessage("t4", sw);
					
					int ri = CdkMol.Util.GetFragmentRGroupAssignment(fragment);

					if (ri >= 0 && ri < Substituents.Length)
						Substituents[ri] = fragment;
				}

				if (RGroupDecomp.Debug) DebugLog.StopwatchMessage("t5", sw);

				// Fill in the Vo

				object[] vo = new object[Qt.SelectedCount];
				for (int ci = 0; ci < SelectList.Count; ci++)
				{
					MetaColumn mc = SelectList[ci];
					string name = mc.Name.ToLower();

					if (mc.IsKey)
						vo[ci] = Eqp.SearchKeySubset[KeyListPos];

					else if (Lex.Eq(name, "Core"))
					{
						vo[ci] = CoreChemicalStructure;
					}

					else if (Lex.Eq(name, "Terminate_Option"))
						vo[ci] = TerminateOptionString;

					else if (Lex.Eq(name, "Map_Number"))
						vo[ci] = (MapPos + 1).ToString();

					else if (Lex.Eq(name, "Map"))
					{
						if (RGroupDecomp.Debug) DebugLog.StopwatchMessage("t6", sw);

						MoleculeMx cs = null;

						try // hilight core molecule	
						{
							if (StrMatcher == null) // initialize matcher with core structure if not done yet
							{
								StrMatcher = new StructureMatcher();
								string molfile2 = CdkMol.Util.RemoveRGroupAttachmentPointAtoms(CoreChemicalStructure.GetMolfileString());
								if (RGroupDecomp.Debug) DebugLog.StopwatchMessage("t7", sw);

								MoleculeMx cs2 = new MoleculeMx(MoleculeFormat.Molfile, molfile2);
								StrMatcher.SetSSSQueryMolecule(cs2); // set core query used for highlighting
								if (RGroupDecomp.Debug) DebugLog.StopwatchMessage("t8", sw);
							}

							Molecule alignedTarget = RGroupDecomp.GetAlignedTargetForMapping(MapPos);
							if (RGroupDecomp.Debug) DebugLog.StopwatchMessage("t9", sw);

							string chime = CdkMol.Util.MoleculeToChimeString(alignedTarget);
							if (RGroupDecomp.Debug) DebugLog.StopwatchMessage("t10", sw);

							cs = new MoleculeMx(MoleculeFormat.Chime, chime);
							cs = StrMatcher.HighlightMatchingSubstructure(cs);
						}
						catch (Exception ex) { ex = ex; }

						vo[ci] = cs; // store value

						if (RGroupDecomp.Debug) DebugLog.StopwatchMessage("t11", sw);
					}

					else if (Lex.Eq(name, "Is_Map_Complete"))
					{
						vo[ci] = "Yes"; // always complete for PP
														//else vo[ci] = "No";
					}

					else
					{ // must be a substituent
						if (RGroupDecomp.Debug) DebugLog.StopwatchMessage("t12", sw);

						int ri = name.IndexOf("_");
						if (ri < 0 || !int.TryParse(name.Substring(1, ri - 1), out ri))
							continue;  // in case of bogus name

						substituent = Substituents[ri];

						if (substituent == null)
						{
							vo[ci] = null; // no mapping found
							continue;
						}

						if (name.EndsWith("_structure"))
						{
							string chime = CdkMol.Util.MoleculeToChimeString(substituent);
							MoleculeMx cs = new MoleculeMx(MoleculeFormat.Chime, chime);
							vo[ci] = cs;
						}

						else if (name.EndsWith("_smiles"))
						{
							string smiles = CdkMol.Util.MoleculeToSmilesString(substituent);
							vo[ci] = smiles;
						}

						else if (name.EndsWith("_formula"))
						{
							string mf = CdkMol.Util.GetMolFormulaDotDisconnect(substituent);
							vo[ci] = mf;
						}

						else if (name.EndsWith("_weight"))
							vo[ci] = CdkMol.Util.GetMolWeight(substituent);

						else if (name.EndsWith("_substno"))
							vo[ci] = ri;

						if (RGroupDecomp.Debug) DebugLog.StopwatchMessage("t13", sw);

					}
				}

				int msTime = (int)sw.ElapsedMilliseconds;
				if (RGroupDecomp.Debug) DebugLog.StopwatchMessage("Total Time for Cid: " + cid, swTotal);

				return vo;
			}
#endif
		}

		/// <summary>
		/// Close broker & release resources
		/// </summary>

		public override void Close()
		{
			return; // todo
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
			Query q = null;

			//QueryColumn qc;

			//// ResultId is of the form: queryId, mtName, mcName, sn1, sn2,...snn

			//string[] sa = resultId.Split(',');

			//int objectId = Int32.Parse(sa[0]);
			//UserObject uo = UserObjectDao.Read(objectId);
			//if (uo == null) return null; // no longer there
			//Query q = Query.Deserialize(uo.Content);
			//q.ResultKeys = null; // clear any set of result keys
			//if (q.LogicType == QueryLogicType.Complex)
			//{ // if complex logic then go simple (todo: fix to handle complex logic)
			//	q.ClearAllQueryColumnCriteria();
			//	q.LogicType = QueryLogicType.And;
			//}
			//q.KeyCriteria = "in (";
			//for (int i1 = 3; i1 < sa.Length; i1++)
			//{
			//	q.KeyCriteria += Lex.AddSingleQuotes(sa[i1]);
			//	if (i1 < sa.Length - 1) q.KeyCriteria += ",";
			//}
			//q.KeyCriteria += ")";
			return q;
		}
	}

	/// <summary>
	/// Substituent information
	/// </summary>

	public class RgroupSubstituent
	{
		public double Mw; // mol weight, used for sorting & speeding match identity
		ICdkMol fragMol = null; // matching mol

		/// <summary>
		/// Get substituent for a decomposition rgroup & map position & store in list
		/// </summary>
		/// <param name="rgd"></param>
		/// <param name="rgroup"></param>
		/// <param name="mapPos"></param>
		/// <returns>Index of the substituent in rSubs</returns>

		public static int Get(
			int rgroup,
			int mapPos,
			List<RgroupSubstituent>[] rSubs)
		{
			const string X_H_MolFile = // molfile to return when matching implicit hydrogen ([X]-H)
			"\n" +
			"  -PROG-  01040717482D\n" +
			"\n" +
			"  2  1  0  0  0  0  0  0  0  0999 V2000\n" +
			"   -2.5724   -0.0345    0.0000 X   0  0  0  0  0  0  0  0  0  0  0  0\n" +
			"   -1.4681   -0.0319    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0\n" +
			"  1  2  1  0  0  0  0\n" +
			"A    1\n" +
			"^X1\n" +
			"M  END\n";

			throw new NotImplementedException(); // todo

		}

		/// <summary>
		/// Destructor
		/// </summary>

		~RgroupSubstituent()
		{
			Dispose();
		}

		/// <summary>
		/// Dispose of sketch
		/// </summary>

		void Dispose()
		{
			return;
		}
	}

}
