using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;
using Mobius.CdkMx;
using Mobius.MolLib1;
using Mobius.Helm;

using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

using LumenWorks.Framework.IO.Csv;

namespace Mobius.QueryEngineLibrary
{

	/// <summary>
	/// Utility methods associated with executing queries against the databases and returning results
	/// File: QueryEngine2.cs
	/// </summary>

	public partial class QueryEngine
	{

		/// <summary>
		/// Do some simple optimizations for searches that are pure "And" logic
		/// </summary>
		/// <param name="query"></param>
		/// <param name="criteriaTableDict"></param>
		/// <param name="criteriaToks"></param>
		/// <param name="criteriaSubsets"></param>
		/// <returns>New list of criteria subsets</returns>

		void OptimizeAndLogic(
				Query query,
				Dictionary<string, QueryTable> criteriaTableDict,
				CriteriaList sourceCritera,
				out List<CriteriaList> criteriaSubsets,
				out List<LogicGroup> logicGroups)
		{
			if (Query.LogicType != QueryLogicType.And) throw new Exception("Expected And logic");

			List<MqlToken> sourceCriteriaToks = sourceCritera.Tokens;
			List<MqlToken> ssCriteriaToks = null;
			List<MqlToken> nonSsCriteriaToks = null;

			criteriaSubsets = new List<CriteriaList>();
			logicGroups = new List<LogicGroup>();
			LogicGroup logicGroup = new LogicGroup(QueryLogicType.And); // single "And" logic group
			logicGroups.Add(logicGroup);

			// Build the criteria groups

			List<CriteriaList> subsets = SplitCriteriaToOptimizePerformance(query, criteriaTableDict, sourceCriteriaToks);

			foreach (CriteriaList subset in subsets) // add all subsets to the logicGroup
			{
				CriteriaList.Add(criteriaSubsets, logicGroup, subset);
			}

			return;
		}

		/// <summary>
		/// For "Or" logic create one criteriaSubset for each table with criteria 
		/// </summary>
		/// <param name="query"></param>
		/// <param name="criteriaTables"></param>
		/// <param name="criteriaToks"></param>
		/// <param name="criteriaSubsets"></param>
		/// <returns>New list of criteria subsets</returns>

		void OptimizeOrLogic(
				Query query,
				Dictionary<string, QueryTable> criteriaTables,
				CriteriaList sourceCritera,
				out List<CriteriaList> criteriaSubsets,
				out List<LogicGroup> logicGroups)
		{
			List<MqlToken> sourceCriteriaToks = sourceCritera.Tokens;

			criteriaSubsets = new List<CriteriaList>();
			logicGroups = new List<LogicGroup>();
			LogicGroup lg = new LogicGroup(QueryLogicType.Or); // single "Or" logic group
			logicGroups.Add(lg);

			foreach (QueryTable qt3 in criteriaTables.Values)
			{
				List<MqlToken> critToksOr = // get criteria for current table only
						MqlUtil.DisableCriteria(CritToks, query, qt3, null, null, null, true, false, true);

				CriteriaList.Add(criteriaSubsets, lg, critToksOr);
			}

			return;
		}

		/// <summary>
		/// Optimize "Complex" Logic Search
		/// </summary>
		/// <param name="query"></param>
		/// <param name="criteriaTables"></param>
		/// <param name="criteriaToks"></param>
		/// <param name="criteriaSubsets"></param>
		/// <param name="logicGroups"></param>

		public void OptimizeComplexLogic(
				Query query,
				Dictionary<string, QueryTable> criteriaTables,
				CriteriaList sourceCriteria,
				out List<CriteriaList> criteriaSubsets,
				out List<LogicGroup> logicGroups)
		{
			LogicGroup rootLg = BuildComplexLogicTree(query, criteriaTables, sourceCriteria, out criteriaSubsets, out logicGroups);
			return;
		}

		/// <summary>
		/// Build a logic tree for complex logic
		/// </summary>
		/// <param name="query"></param>
		/// <param name="criteriaTables"></param>
		/// <param name="criteriaToks"></param>
		/// <returns>Root logic group</returns>

		public static LogicGroup BuildComplexLogicTree(
				Query query,
				Dictionary<string, QueryTable> criteriaTables,
				CriteriaList sourceCriteria,
				out List<CriteriaList> criteriaSubsets,
				out List<LogicGroup> logicGroups)
		{
			QueryTable qt;
			int tki, logicToksDisabled = 0;

			List<MqlToken> sourceCriteriaToks = sourceCriteria.Tokens;
			criteriaSubsets = new List<CriteriaList>();
			logicGroups = new List<LogicGroup>();

			Dictionary<string, int> firstTableTokPos = new Dictionary<string, int>();
			Dictionary<string, int> lastTableTokPos = new Dictionary<string, int>();

			// Scan all tokens setting first and last token position for each QueryTable with criteria

			for (tki = 0; tki < sourceCriteriaToks.Count; tki++)
			{
				MqlToken mqlTok = sourceCriteriaToks[tki];
				if (mqlTok == null || mqlTok.Qc == null) continue;
				qt = mqlTok.Qc.QueryTable;
				string alias = qt.Alias;
				if (!firstTableTokPos.ContainsKey(alias))
					firstTableTokPos[alias] = tki;

				lastTableTokPos[alias] = tki;
			}

			// Unmark any logic tokens as logic tokens between the first and last reference to each table as logic tokens

			foreach (string alias_i in criteriaTables.Keys)
			{
				if (!firstTableTokPos.ContainsKey(alias_i)) continue;
				int firstPos = firstTableTokPos[alias_i];
				int lastPos = lastTableTokPos[alias_i];
				int parenDepth = 0;
				for (tki = firstPos; tki < sourceCriteriaToks.Count; tki++)
				{
					MqlToken t = sourceCriteriaToks[tki];
					if (t.IsLogicToken)
					{
						if (t.Tok.Text == "(") parenDepth++;
						else if (t.Tok.Text == ")") parenDepth++;
						t.IsLogicToken = false;
						logicToksDisabled++;
					}
					if (tki >= lastPos && parenDepth <= 0) break; // must be at or past last token for table and have balanced parens
				}
			}

			tki = 0;
			LogicGroup rootLg = BuildLogicGroup(sourceCriteria, ref criteriaSubsets, ref logicGroups, ref tki);

			return rootLg;
		}

		/// <summary>
		/// Recursively build logic groups
		/// </summary>
		/// <param name="criteriaToks"></param>
		/// <param name="tki"></param>
		/// <returns></returns>

		static LogicGroup BuildLogicGroup(
				CriteriaList sourceCriteria,
				ref List<CriteriaList> criteriaSubsets,
				ref List<LogicGroup> logicGroups,
				ref int tki)
		{
			CriteriaList acs = null;
			MqlToken t = null;

			List<MqlToken> sourceCriteriaToks = sourceCriteria.Tokens;

			int startTokPos = tki;
			int logicTokCnt = 0, qcTokCnt = 0, otherTokCnt = 0;

			LogicGroup lg = new LogicGroup(); // build logic group here
			lg.Id = logicGroups.Count;
			logicGroups.Add(lg);

			HashSet<int> tokPosSet = new HashSet<int>();
			tki--; // backup one place

			while (true)
			{
				tki++;

				if (tki >= sourceCriteriaToks.Count) // at end?
				{
					break;
				}

				t = sourceCriteriaToks[tki];
				if (t == null || t.Tok == null) continue;
				string tok = t.Tok.Text;

				if (t.IsLogicToken) // logic token between tables (not logic within a single table or overlapped tables)
				{
					logicTokCnt++;

					if (Lex.Eq(tok, "and"))
					{
						if (lg.LogicType == QueryLogicType.Unknown || lg.LogicType == QueryLogicType.And)
						{
							lg.LogicType = QueryLogicType.And;
							acs = lg.AddCriteriaSubset(sourceCriteria, tokPosSet, criteriaSubsets);
							tokPosSet = new HashSet<int>(); // start new token list
							continue;
						}

						else
						{
							throw new NotImplementedException(); // end of this logic group
						}
					}

					else if (Lex.Eq(tok, "or"))
					{
						if (lg.LogicType == QueryLogicType.Unknown || lg.LogicType == QueryLogicType.Or)
						{
							lg.LogicType = QueryLogicType.Or;
							acs = lg.AddCriteriaSubset(sourceCriteria, tokPosSet, criteriaSubsets);
							tokPosSet = new HashSet<int>(); // start new token list
							continue;
						}

						else
						{
							throw new NotImplementedException(); // end of this logic group
						}
					}

					else if (Lex.Eq(tok, "(")) // descend into inner expression
					{
						if (lg.ElementList.Count == 0) // ignore initial paren
							continue;

						if (lg.LogicType == QueryLogicType.Unknown) throw new Exception("Left paren seen but logic type not defined");

						if (tokPosSet.Count > 0) // add any accumulated criteria
							acs = lg.AddCriteriaSubset(sourceCriteria, tokPosSet, criteriaSubsets);

						LogicGroup lg2 = // recursively process the rest of the tokens
								BuildLogicGroup(sourceCriteria, ref criteriaSubsets, ref logicGroups, ref tki);

						lg.ElementList.Add(lg2); // add new logic group to element list
						lg2.ParentLogicGroup = lg; // point the new logic group to current group as parent

						startTokPos = tki; // starting new element
						tokPosSet = new HashSet<int>();
						continue;
					}

					else if (Lex.Eq(tok, ")")) // end of this group
					{
						break;
					}

					else throw new Exception("Unexpected logic token: " + tok);
				}

				else // non-logic token
				{
					if (t.Qc != null) qcTokCnt++;
					else otherTokCnt++;
				}

				tokPosSet.Add(tki); // add to list and continue
			}

			if (tokPosSet.Count > 0) // if any tokens then add an additional subset
			{
				acs = lg.AddCriteriaSubset(sourceCriteria, tokPosSet, criteriaSubsets);
			}

			return lg;
		}

		/// <summary>
		/// Identify special structure search criteria that need to be executed by
		/// themselves.
		/// 1. Structure searches against structure columns that don't have associated 
		///    cartride indexes. These are executed in Mobius code.
		/// 2. Small substructure searches that return a large number of hits.
		///    These should be done last with the set of previous matches
		///    limiting the search domain.
		/// </summary>
		/// <param name="query"></param>
		/// <param name="criteriaTableDict"></param>
		/// <param name="sourceCriteriaToks"></param>
		/// <returns></returns>

		List<QueryColumn> BuildSpecialStructureSearchCriteriaList(
				Query query,
				Dictionary<string, QueryTable> criteriaTableDict,
				List<MqlToken> sourceCriteriaToks)
		{
			QueryColumn qc;

			List<QueryColumn> qcList = new List<QueryColumn>();

			if (NonSqlStructCriteriaList != null)
			{
				foreach (int noci in NonSqlStructCriteriaList)
				{
					qc = CritToks[noci].Qc; // QueryColumn criteria is being applied to
					qcList.Add(qc);
				}
			}

			if (StructCriteriaList.Count == 1 && // exactly one structure search
					criteriaTableDict.Count >= 2 && // criteria on at least two tables
					KeyCriteriaSavedListKeys == null && // no saved list
					KeyCriteriaPositions.Count == 0 && // no other criteria on key
					StructureSearchMayBeSlow(query, sourceCriteriaToks, StructCriteriaList[0])) // "imprecise" query (put last since evaluating this may be slowest part of expression)
			{
				qc = CritToks[StructCriteriaList[0]].Qc;
				qcList.Add(qc);
			}

			return qcList;
		}

		/// <summary>
		/// Separate special structure search criteria that need to be executed by themselves.
		/// </summary>
		/// <param name="query"></param>
		/// <param name="specialSsCriteria"></param>
		/// <param name="criteriaSubsets"></param>
		/// <param name="logicGroups"></param>

		void AdjustSpecialStructureSearchCriteria(
				Query query,
				List<QueryColumn> specialSsCriteria,
				List<CriteriaList> criteriaSubsets,
				List<LogicGroup> logicGroups)
		{
			int noci, csi, tki;

			List<CriteriaList> csList2 = new List<CriteriaList>(criteriaSubsets); // make stable copy of lists to look at 

			foreach (QueryColumn qc in specialSsCriteria) // look for each criteria
			{
				int csCnt = csList2.Count;
				for (csi = 0; csi < csCnt; csi++)
				{
					CriteriaList cl = csList2[csi]; // criteria list we are considering
					LogicGroup lg = cl.ParentLogicGroup; // logic group we are working in

					tki = MqlUtil.IndexOfQueryColumnInMqlTokenList(cl.Tokens, qc);
					if (tki < 0) continue;

					CriteriaList cl2 = cl.Clone();
					cl2.Tokens = // get single non-oracle criteria token subset
			 MqlUtil.DisableCriteria(cl.Tokens, query, null, null, qc, null, true, false, true);

					int ilcl = lg.IndexOfLastCriteriaList(); // insert after last criteria list in logicgroup elements
					if (ilcl >= 0)
					{
						CriteriaList cl3 = (CriteriaList)lg.ElementList[ilcl];

						int csiPos = criteriaSubsets.IndexOf(cl3); // where last criteria list is in criteria subsets
						if (csiPos >= 0) // insert the new single structure search criteria list into the criteria subsets last after last list in the current logic group
							criteriaSubsets.Insert(csiPos + 1, cl2);
					}

					lg.ElementList.Add(cl2); // add new criteria list to end of logic group

					cl.Tokens = // remove the str search criteria from the Oracle set
							MqlUtil.DisableCriteria(cl.Tokens, query, null, null, null, qc, true, false, true);

					List<QueryColumn> qcRemainingList = MqlUtil.GetQueryColumns(cl.Tokens);

					if (qcRemainingList.Count == 0) // if nothing left in criteria list then remove it from logic group and criteria subsets
					{
						if (lg.ElementList.Contains(cl)) lg.ElementList.Remove(cl); // remove from logic group
						else cl = cl; // not found

						if (criteriaSubsets.Contains(cl)) criteriaSubsets.Remove(cl); // remove from remove from criteria subsets
						else cl = cl; // not found
					}

				}
			}

			return;
		}

		/// <summary>
		/// Move any "not exists" criteria to separate CriteriaSubsets and
		/// position at end of logic group so key list will be available for 
		/// input to the not-exists sql.
		/// </summary>
		/// <param name="query"></param>
		/// <param name="criteriaSubsets"></param>
		/// <param name="logicGroups"></param>

		void AdjustNotExistsCriteria(
				Query query,
				List<CriteriaList> criteriaSubsets,
				List<LogicGroup> logicGroups)
		{
			CriteriaList cl2 = null;
			QueryColumn qc;
			int noci, csi, tki;

			List<CriteriaList> csList2 = new List<CriteriaList>(criteriaSubsets); // make stable copy of subsets to look at 

			for (csi = 0; csi < csList2.Count; csi++) // examine each list of criteria
			{
				CriteriaList cl = csList2[csi]; // criteria list we are considering
				cl.ParseQueryColumnTokens();
				LogicGroup lg = cl.ParentLogicGroup; // logic group we are working in

				// Move each "not exists" criteria in this CriteriaList to its own criteria list and position at end of its logic group

				for (int qcti = 0; qcti < cl.QcTokList.Count; qcti++)
				{
					MqlToken tok = cl.QcTokList[qcti];
					if (tok.Qc == null || tok.Psc == null || tok.Psc.OpEnum != CompareOp.IsNull) continue;

					qc = tok.Qc;

					if (qc.QueryTable.HasNormalDatabaseCriteria) continue; // if other normal criteria on table then leave as is

					if (cl.QcTokList.Count > 1) // Move the "is null" criteria to new list and insert at end of logic group
					{
						cl2 = cl.Clone(); // make copy to contain single criteria
						cl2.Tokens = // get single "is null" criteria token subset
								MqlUtil.DisableCriteria(cl.Tokens, query, null, null, qc, null, true, false, true);

						cl.Tokens = // remove the "is null" criteria from the set
								MqlUtil.DisableCriteria(cl.Tokens, query, null, null, null, qc, true, false, true);
					}

					else // just move this list to end of logic group
					{
						cl2 = cl;
						if (!lg.ElementList.Contains(cl)) DebugMx.DataException("CriteriaList not found in ElementList");

						if (lg.ElementList.Count > 1)
						{
							lg.ElementList.Remove(cl); // remove from where it's at and insert at end of lg below
							criteriaSubsets.Remove(cl);
						}

						else continue; // no need to move the criteria list if the only one in the logic group
					}

					int lcli = lg.IndexOfLastCriteriaList(); // insert after last criteria list in logicgroup elements and into the overall criteria subsets for the query
					if (lcli >= 0)
					{
						CriteriaList cl3 = (CriteriaList)lg.ElementList[lcli];
						lg.ElementList.Insert(lcli + 1, cl2); // insert the new single "is null" criteria list into the criteria subsets after last list in the current logic group

						int csiPos = criteriaSubsets.IndexOf(cl3); // where last criteria list for this logic group is in the overall criteria subsets for the query
						if (csiPos >= 0) // insert the new single "is null" criteria list into the overall criteria subsets after last list of the current logic group
							criteriaSubsets.Insert(csiPos + 1, cl2);
					}
				}
			}

			return;
		}

		/// <summary>
		/// Comparison method for sorting list of result keys, sim score KeyValuePairs on the sim score
		/// </summary>
		/// <param name="firstPair"></param>
		/// <param name="nextPair"></param>
		/// <returns></returns>

		int CompareSimScoresKeyValuePair(
				KeyValuePair<string, object> firstPair,
				KeyValuePair<string, object> nextPair)
		{
			object v1 = firstPair.Value;
			object v2 = nextPair.Value;
			int cv = StructSearchMatch.CompareByMatchQuality(v1, v2);
			return cv;
		}

		/// <summary>
		/// If a Mol similarity column is selected then get the the query column & associated sql expression
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="simExpr"></param>
		/// <returns></returns>

		QueryColumn GetMolSimScoreExpressionIfSelected(
				QueryTable qt,
				List<MqlToken> subsetToks,
				out string simExpr)
		{
			QueryColumn qc, qc2 = null;
			simExpr = null;

			qc = qt.GetMolSimScoreQueryColumn(); // and structure search score column?
			if (qc == null || !qc.Selected) return null;

			if (Query.LogicType != QueryLogicType.Complex) // simple (non-complex logic case)
				qc2 = qt.GetQueryColumnWithStructureSearchCriteria();

			else // extract any complex structure search criteria
			{
				Query q2 = Query.Clone();
				MqlUtil.ConvertComplexCriteriaToQueryColumnCriteria(q2, QueryLogicType.And); // move the criteria into QueryColumn
				QueryTable qt2 = q2.GetQueryTableByName(qt.MetaTable.Name);
				if (qt2 != null)
					qc2 = qt2.GetQueryColumnWithStructureSearchCriteria();
			}

			if (qc2 == null || Lex.IsUndefined(qc2.Criteria)) return null;

			ParsedStructureCriteria psc = ParsedStructureCriteria.Parse(qc2);
			if (psc.SearchType == StructureSearchType.MolSim) //  && psc.MaxSimHits > 0) // MolSim value comes from Oracle
			{
				simExpr = MqlUtil.GetSimScoreExpressionFromCriteria(subsetToks); // get any score expression from criteria
				if (Lex.IsDefined(simExpr)) return qc;
				else return null;
			}

			else if (psc.SearchType == StructureSearchType.SmallWorld) // SmallWorld data doesn't come from Oracle
			{
				simExpr = qc.MetaColumn.Name;
				return qc;
			}

			else if (psc.SearchType == StructureSearchType.Related) // Related structure search data doesn't come from Oracle
			{
				simExpr = qc.MetaColumn.Name;
				return qc;
			}

			else return null;
		}

		/// <summary>
		/// If a sim search exists for the query return max allowed hits
		/// </summary>
		/// <param name="maxSimHits"></param>
		/// <returns></returns>

		public bool TryParseSimSearchMaxHits(out int maxSimHits)
		{
			maxSimHits = -1;

			ParsedStructureCriteria pssc = GetParsedStructureSearchCriteria();
			if (pssc == null) return false;

			if (pssc.SearchType == StructureSearchType.MolSim)
			{
				maxSimHits = pssc.MaxSimHits;
				return true;
			}

			else if (pssc.SearchType == StructureSearchType.SmallWorld & pssc.SmallWorldParameters != null)
			{
				maxSimHits = pssc.SmallWorldParameters.MaxHits; // copy maxHits to common location
				return true;
			}

			else if (pssc.SearchType == StructureSearchType.Related)
			{
				maxSimHits = pssc.MaxSimHits;
				return true;
			}

			else return false;
		}

		/// <summary>
		/// Return any structure search QueryColumn in the table in parsed format
		/// </summary>
		/// <returns></returns>

		public ParsedStructureCriteria GetParsedStructureSearchCriteria()
		{

			foreach (QueryTable qt in Query.Tables)
			{
				ParsedStructureCriteria pssc = qt.GetParsedStructureSearchCriteria();
				if (pssc != null) return pssc;
			}

			return null;
		}

		/// <summary>
		/// Write exception and associated serialized query to log files
		/// </summary>
		/// <param name="ex"></param>

		public string LogExceptionAndSerializedQuery(
						Exception ex)
		{
			string exMsg = DebugLog.FormatExceptionMessage(ex);
			string msg = LogExceptionAndSerializedQuery(exMsg, Query);
			LoggedQueryException = true;
			return msg;
		}

		/// <summary>
		/// Write exception and associated serialized query to log files
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="query"></param>
		/// <returns></returns>

		public static string LogExceptionAndSerializedQuery(
				string exMsg,
				Query query)
		{
			string logFile, sqLogFile;

			if (query == null) // if no query then just log message
			{
				DebugLog.Message("QE Exception: " + exMsg);
				return "";
			}

			if (LoggedQueries.ContainsKey(query.InstanceId) && LoggedQueries[query.InstanceId] == exMsg) // if exception already logged for this query don't log again
				return "";

			DateTime dt = DateTime.Now;
			string logTimeMsg = "(QueryLogged: " + dt.ToString() + "." + dt.Millisecond + ")";

			int uoId = query.UserObject != null ? query.UserObject.Id : -1;

			DebugLog.Message("QE Exception, UoId: " + uoId + " " + logTimeMsg + ": " + exMsg);
			logFile = sqLogFile = DebugLog.LogFileName;
			int i1 = logFile.LastIndexOf(".");
			if (i1 >= 0)
				sqLogFile = logFile.Substring(0, i1) + ".SerializedQueries" + logFile.Substring(i1);

			string sq = query.SerializeForDebugging();
			LogFile.Message(logTimeMsg + "\n\r" + sq, sqLogFile); // write directly to file

			LoggedQueries[query.InstanceId] = exMsg; // mark as logged

			return logTimeMsg;
		}

		/// <summary>
		/// Log query as the current query for the current process
		/// </summary>
		/// <param name="query"></param>

		public static void LogQueryByProcessId(Query query)
		{
			try
			{
				string pidString = String.Format("{0,8:00000000}", Process.GetCurrentProcess().Id);
				string sqLogFile = ServicesDirs.LogDir + @"\CurrentQueryExecutingByProcessId\Process_" + pidString + ".log";
				if (File.Exists(sqLogFile))
					try { File.Delete(sqLogFile); }
					catch (Exception ex) { }

				string sq = query.SerializeForDebugging();
				string msg = "Process: " + pidString + ", User: " + Security.UserName + ", Query:\r\n" + sq;
				LogFile.Message(msg, sqLogFile); // write directly to file

				return;
			}
			catch (Exception ex) { } // ignore errors
		}

		/// <summary>
		/// Log query execution stats
		/// </summary>

		public static void LogQueryExecutionStatistics()
		{
			string ss = "";

			foreach (QueryEngineStats s in SessionStats)
			{
				if (s.ExecutionCount == 0) continue;
				if (ss != "") ss += "\n";
				ss += s.Serialize();
			}

			UsageDao.LogEvent("QueryEngineStats", ss);
		}

		/// <summary>
		/// Cancel query execution
		/// </summary>

		public void Cancel(bool waitForCompletion)
		{
			if (State != QueryEngineState.Searching &&
					State != QueryEngineState.Searching) return;

			CancelRequested = true;
			if (!waitForCompletion) return;

			while (true)
			{
				if (Cancelled) return;
				Thread.Sleep(250);
				Application.DoEvents();
			}
		}

		/// <summary>
		/// Implementation of ICheckForCancel method to check for cancel
		/// </summary>

		public bool IsCancelRequested
		{
			get { return CancelRequested; }
		}

		/// <summary>
		/// Get the list of schemas and tables in a query that are not accessible
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		public static Dictionary<string, List<string>> CheckDataSourceAccessibility(Query q)
		{
			DateTime t0 = DateTime.Now;
			Dictionary<string, List<string>> inaccessableData = new Dictionary<string, List<string>>();
			HashSet<MetaBrokerType> brokersChecked = new HashSet<MetaBrokerType>();
			int tDelta;

			if (Math.Abs(1) == 1) return inaccessableData; // disable for now, can take several seconds for a QuickSearch

			foreach (QueryTable qt in q.Tables)
			{
				MetaTable mt = qt.MetaTable;
				if (brokersChecked.Contains(mt.MetaBrokerType) && mt.MetaBrokerType != MetaBrokerType.Generic) // already checked?
					continue;

				brokersChecked.Add(mt.MetaBrokerType);

				IMetaBroker imb = GetGlobalBroker(mt);
				string schemaKey = null;
				try
				{
					if (imb == null) throw new Exception("No data available (MetaBroker not defined)");
					DateTime t1 = DateTime.Now;
					DbSchemaMx schema = imb.CheckDataSourceAccessibility(mt);
					tDelta = (int)TimeOfDay.Delta(ref t1);
					if (LogBasics) DebugLog.Message("Table: " + mt.Name + ", time: " + tDelta);

					if (schema == null) continue; // everything ok if no schema returned
					schemaKey = schema.Label;
					if (schemaKey == null || schemaKey == "") schemaKey = schema.DataSourceName;
					else schemaKey += " (" + schema.DataSourceName + ")";
				}
				catch (Exception ex)
				{ schemaKey = ex.Message; } // don't know source name so use message as key

				if (inaccessableData.ContainsKey(schemaKey))
					inaccessableData[schemaKey].Add(qt.MetaTable.Name);

				else
				{
					List<string> mtList = new List<string>();
					mtList.Add(qt.MetaTable.Name);
					inaccessableData[schemaKey] = mtList;
				}

			}

			tDelta = (int)TimeOfDay.Delta(ref t0);
			if (LogBasics) DebugLog.Message("Total Time: " + tDelta);

			return inaccessableData;
		}

		/// <summary>
		/// Do any presearch initialization for the QueryTables in a query
		/// </summary>

		void DoPresearchInitialization()
		{
			for (int ti = 0; ti < Query.Tables.Count; ti++)
			{
				QueryTable qt = Query.Tables[ti];
				if (qt.MetaTable.MetaBrokerType == MetaBrokerType.Unknown) continue;
				IMetaBroker mb = MetaBrokerUtil.GlobalBrokers[(int)qt.MetaTable.MetaBrokerType];
				mb.DoPresearchInitialization(qt);
			}

			return;
		}

		/// <summary>
		/// Get root table for query or null > 1 root table
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>

		public static MetaTable GetRootTable(
				Query query)
		{
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;

			if (query == null || query.Tables.Count == 0) return null;

			//if (query.LogicType != QueryLogicType.Complex)
			{ // do simple check
				qt = query.Tables[0];
				mt = qt.MetaTable;
				mc = mt.DatabaseListMetaColumn;
				if (mc != null) // see if multiple databases for query
				{
					qc = qt.GetQueryColumnByName(mc.Name);
					if (qc != null && qc.Criteria != "" && qc.Criteria.IndexOf(",") > 0)
						mt = null; // just go on cid if multiple root tables for query
				}
				return mt;
			}

			// todo: fix stack overflow problem below
			//else // check complex criteria
			//{ 
			//  QueryEngine qe = new QueryEngine();
			//  List<MetaTable> roots = qe.GetRootTables(query); // this can cause an loop: 1. AnalyzeCriteria, 2. AnalyzeKeyCriteria, 3. GetRootTable, 4. GetRootTables ...
			//  if (roots != null && roots.Count == 1)
			//    return roots[0];
			//  else return null;
			//}
		}

		/// <summary>
		/// Get set of root tables for query
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>

		public List<MetaTable> GetRootTables(
				Query query)
		{
			string errorMsg;
			int errorPosition;

			Qtd = new QueryTableData[query.Tables.Count]; // init qtd structure
			for (int ti = 0; ti < query.Tables.Count; ti++)
			{
				Qtd[ti] = new QueryTableData();
				Qtd[ti].Table = query.Tables[ti];
				Qtd[ti].SelectedColumns = new List<QueryColumn>();
			}

			AnalyzeCriteria(
					query,
					Qtd,
					out MqlLogicType,
					out NotLogic,
					out MustUseEquiJoins,
					out CritToks,
					out TotalCriteria,
					out TablesWithCriteria,
					out StructCriteriaList,
					out NonSqlStructCriteriaList,
					out RootTables,
					out KeyCompareOp,
					out KeyCriteriaPositions,
					out KeyCriteriaSavedListKeys,
					out KeyCriteriaInListKeys,
					out KeyCriteriaConstantPositions,
					out errorMsg,
					out errorPosition);

			return RootTables;
		}

		/// <summary>
		/// Analyze query and setup query table data (Qtd) and criteria data
		/// </summary>

		public void AnalyzeQuery()
		{
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			string errorMsg;
			int errorPosition;
			int ti, ci;

			Qtd = new QueryTableData[Query.Tables.Count];
			ResultsMetaColumns = new List<MetaColumn>();
			ResultsMetaColumns.Add(null); // first metacolumn is common key col whose value comes from any of the tables in the query
			TotalColumnsSelected = 0;
			MetaTable rootTable = null;

			//SetAllowCriteriaProperty(Query); (Obsolete)

			// Inital setup of QueryTableData

			for (ti = 0; ti < Query.Tables.Count; ti++)
			{
				Qtd[ti] = new QueryTableData();
				Qtd[ti].Table = Query.Tables[ti];
				Qtd[ti].SelectedColumns = new List<QueryColumn>();

				qt = Query.Tables[ti];
				mt = qt.MetaTable;
				if (mt.KeyMetaColumn == null)
					throw new QueryException("Key column not defined for metatable: " + mt.Name);

				if (!qt.KeyQueryColumn.Selected)
					throw new QueryException("Key column not selected for metatable: " + mt.Name);
			}

			MqlUtil.CheckTableCompatibility(Query, null); // check for compatibility of tables in query

			// Analyze & partially transform criteria

			AnalyzeCriteria(
					Query,
					Qtd,
					out MqlLogicType,
					out NotLogic,
					out MustUseEquiJoins,
					out CritToks,
					out TotalCriteria,
					out TablesWithCriteria,
					out StructCriteriaList,
					out NonSqlStructCriteriaList,
					out RootTables,
					out KeyCompareOp,
					out KeyCriteriaPositions,
					out KeyCriteriaSavedListKeys,
					out KeyCriteriaInListKeys,
					out KeyCriteriaConstantPositions,
					out errorMsg,
					out errorPosition);

			if (errorMsg != null) throw new QueryException(errorMsg);

			// Scan tables counting selected cols for each

			//StringBuilder colsSelectedNames = new StringBuilder(); // debug

			for (ti = 0; ti < Query.Tables.Count; ti++)
			{
				qt = Query.Tables[ti];
				mt = qt.MetaTable;

				// Store the result VO position of first column for each table

				if (ti == 0)
					Qtd[ti].TableVoPosition = 0;

				else
				{
					Qtd[ti].TableVoPosition = Qtd[ti - 1].TableVoPosition + Qtd[ti - 1].SelectCount;
					if (Qtd[0].ChildTables == null) Qtd[0].ChildTables = new List<QueryTable>();
					Qtd[ti].ParentTable = Query.Tables[0]; // assume first table is parent (generalize later)
					Qtd[ti].ParentVoPos = Qtd[0].SelectCount + Qtd[0].ChildTables.Count; // where in parent vo this table goes
					Qtd[0].ChildTables.Add(qt); // add us as child to our parent
				}

				for (ci = 0; ci < qt.QueryColumns.Count; ci++)
				{
					qc = qt.QueryColumns[ci];
					if (qc.MetaColumn == null) continue; // todo: handle missing metacolumn better?

					mc = qc.MetaColumn;

					if (qc.Is_Selected_or_GroupBy_or_Sorted) // retrieve if selected, grouped or sorted on
					{
						Qtd[ti].SelectedColumns.Add(qc);
						Qtd[ti].SelectCount++;
						TotalColumnsSelected++;
						ResultsMetaColumns.Add(mc);
						qc.VoPosition = ResultsMetaColumns.Count - 1;
						if (mc.IsKey) // keep just one instance of key column
						{
							Qtd[ti].KeyColPos = Qtd[ti].SelectCount - 1;
						}

						//colsSelectedNames.Append((TotalColumnsSelected - 1).ToString() + " " + qc.QueryTable.Alias + "." + mc.Name + "\r\n"); // debug
					}

				} // end column loop

			} // end table loop

			// Adjust key column positions.
			// This is necessary because the metabrokers will add the key column to the beginning of
			// the VO if the key is not selected in the query.

			for (ti = 0; ti < Query.Tables.Count; ti++)
			{
				if (Qtd[ti].KeyColPos < 0)
				{
					Qtd[ti].KeyAdded = true;
					Qtd[ti].KeyColPos = 0;
				}
				else
				{
					Qtd[ti].KeyAdded = false;
				}
			}

			return;
		}

		/// <summary>
		/// Do initial analysis & partial transformation of query criteria
		/// </summary>
		/// <param name="query"></param>
		/// <param name="qtd"></param>
		/// <param name="notLogic"></param>
		/// <param name="tokens"></param>
		/// <param name="totalCriteria"></param>
		/// <param name="tablesWithCriteria"></param>
		/// <param name="structCriteriaList"></param>
		/// <param name="nonCartridgeStructureCriteriaList"></param>
		/// <param name="rootTables"></param>
		/// <param name="keyCriteriaPositions"></param>
		/// <param name="keyCriteriaSavedListKeys"></param>
		/// <param name="keyCriteriaInListKeys"></param>
		/// <param name="keyCriteriaConstantPositions"></param>
		/// <param name="errorMessage"></param>
		/// <param name="errorPosition"></param>
		/// <returns></returns>

		public bool AnalyzeCriteria(
				Query query,
				QueryTableData[] qtd,
				out QueryLogicType mqlLogicType, // logic type seen in criteria, may be complex even if query logic is AND 
				out bool notLogic,
				out bool mustUseEquiJoins,
				out List<MqlToken> tokens,
				out int totalCriteria,
				out List<QueryTable> tablesWithCriteria,
				out List<int> structCriteriaList,
				out List<int> nonCartridgeStructureCriteriaList,
				out List<MetaTable> rootTables,
				out CompareOp keyCompareOp,
				out List<int> keyCriteriaPositions,
				out List<string> keyCriteriaSavedListKeys,
				out List<string> keyCriteriaInListKeys,
				out List<int> keyCriteriaConstantPositions,
				out string errorMessage,
				out int errorPosition)
		{
			QueryTable qt;
			QueryColumn qc, qc2;
			MetaTable mt;
			MetaColumn mc;

			notLogic = false;
			mustUseEquiJoins = true;
			structCriteriaList = new List<int>(); // list of positions of structure queryColumn references
			nonCartridgeStructureCriteriaList = new List<int>();
			rootTables = new List<MetaTable>();
			keyCompareOp = CompareOp.Unknown;
			keyCriteriaPositions = null;
			keyCriteriaSavedListKeys = null;
			keyCriteriaInListKeys = null;
			keyCriteriaConstantPositions = null;
			errorMessage = null;
			errorPosition = -1;

			List<int> dbSetCriteriaList = new List<int>(); // list of positions of structure queryColumn references
			totalCriteria = 0;
			tablesWithCriteria = new List<QueryTable>(); // list of tables with criteria

			if (query.LogicType != QueryLogicType.Complex)
			{
				query.AssignUndefinedAliases(); // simple query be sure every table has an alias
				query.NormalizeKeyCriteria();
			}

			else // if complex then clear simple so we can mark cols with criteria
				query.ClearAllQueryColumnCriteria();

			string complexCriteria = MqlUtil.GetCriteriaString(query);

			tokens = // parse criteria into tokens marking query column references
					MqlUtil.ParseComplexCriteria(complexCriteria, query);

			int tki = 0;
			while (tki < tokens.Count)
			{ // count number of criteria for each QueryTable
				MqlToken tok = tokens[tki];
				if (tok.Tok != null && tok.Tok.Text != null && Lex.Eq(tok.Tok.Text, "not"))
				{ // set not logic for any not expression other than "not null"
					if (tki + 1 < tokens.Count && tokens[tki + 1].Tok != null &&
					 tokens[tki + 1].Tok.Text != null && Lex.Eq(tokens[tki + 1].Tok.Text, "null"))
					{ } // don't count as notLogic

					else notLogic = true; // set flag for "not" logic seen
				}

				if (tok.Qc == null) { tki++; continue; }
				qc = tok.Qc;
				mc = qc.MetaColumn;
				if (!mc.IsSearchable)
					throw new UserQueryException("Searching is not allowed for " + mc.MetaTable.Name + "." + mc.Name);

				qt = qc.QueryTable;
				int ti = query.GetQueryTableIndex(qt);
				if (ti < 0) { tki++; continue; } // index not found for some reason

				if (MoleculeMetaBroker.IsNonCartridgeStructureSearchCriteria(qc))
				{
					nonCartridgeStructureCriteriaList.Add(tki);
				}

				else if (qc.MetaColumn.DataType == MetaColumnType.Structure)
				{
					structCriteriaList.Add(tki);
					tok.Tok.Text = qt.Alias + ".ctab"; // map structure column to ctab
				}

				else if (qc.MetaColumn.DataType == MetaColumnType.MolFormula)
				{
					structCriteriaList.Add(tki);
					tok.Tok.Text = qt.Alias + ".ctab"; // map structure column to ctab
				}

				else if (Lex.Eq(qc.MetaColumn.ColumnMap, "MolWt(ctab)"))
				{
					structCriteriaList.Add(tki);
					tok.Tok.Text = "molwt(" + qt.Alias + ".ctab)"; // properly map molweight column to ctab
				}

				else if (qc.MetaColumn.IsDatabaseSetColumn)
				{

					bool sqlTable = MetaBrokerUtil.IsSqlMetaBroker(qt.MetaTable.MetaBrokerType);
					if (sqlTable)
						dbSetCriteriaList.Add(tki);

					else { } // don't count if non-sql table database subset (e.g. smallworld)

					tki++;
					continue; // doesn't count as criteria
				}

				else if (mc.DataType == MetaColumnType.QualifiedNo || mc.DetailsAvailable)
				{ // append "_val" suffix to get column name to compare to
					tok.Tok.Text += "_val"; // col to compare to
				}

				if (qtd[ti].CriteriaCount == 0) // tables with criteria
					tablesWithCriteria.Add(qt);

				qtd[ti].CriteriaCount++; // criteria for table
				totalCriteria++;
				if (query.LogicType == QueryLogicType.Complex) // set criteria place holder for metabroker to see (needed?)
					qc.Criteria = "Complex";

				tki++; // move to next token
			}

			// Extract database set criteria
			foreach (int tki2 in dbSetCriteriaList)
			{
				MqlUtil.ExtractDbSetCriteria(tokens, tki2, rootTables);
			}

			rootTables = FilterRootTablesForStructureSearchCriteria(rootTables);

			if (rootTables.Count == 0 && query.Tables.Count > 0)
				rootTables.Add(query.Tables[0].MetaTable.Root); // get default root if nothing explicit

			// Analyze/Transform key criteria

			AnalyzeKeyCriteria(
					query,
					qtd,
					tokens,
					out keyCompareOp,
					out keyCriteriaPositions,
					out keyCriteriaSavedListKeys,
					out keyCriteriaInListKeys,
					out keyCriteriaConstantPositions);

			// Set type of logic for query engine

			mqlLogicType = QueryLogicType.Unknown;
			bool inBetween = false;
			MqlToken currentQcTok = null;
			for (int ti = 0; ti < tokens.Count; ti++)
			{
				MqlToken mqlTok = tokens[ti];
				string tok = mqlTok.Tok.Text;

				if (mqlTok.Qc != null) currentQcTok = mqlTok; // keep current query column
				if (currentQcTok != null && currentQcTok.Qc.IsKey) continue; // if in key column criteria continue since long lists may be broken into shorter OR-ed lists

				else if (Lex.Eq(tok, "Between")) inBetween = true;
				else if (Lex.Eq(tok, "And") && inBetween) inBetween = false;

				else if (Lex.Eq(tok, "And"))
				{
					if (mqlLogicType == QueryLogicType.And ||
					mqlLogicType == QueryLogicType.Unknown)
						mqlLogicType = QueryLogicType.And;

					else
					{
						mqlLogicType = QueryLogicType.Complex;
						break;
					}
				}

				else if (Lex.Eq(tok, "Or"))
				{
					if (mqlLogicType == QueryLogicType.Or ||
					mqlLogicType == QueryLogicType.Unknown)
						mqlLogicType = QueryLogicType.Or;

					else
					{
						mqlLogicType = QueryLogicType.Complex;
						break;
					}
				}
			}

			if (mqlLogicType == QueryLogicType.Unknown)
				mqlLogicType = QueryLogicType.And; // if nothing explicit say and

			string critToks = MqlUtil.CatenateCriteriaTokens(tokens);

			string critToksAfterDebug = MqlUtil.CatenateCriteriaTokensForDebug(tokens);

			if (query.LogicType == QueryLogicType.And && !Lex.Contains(critToks, " is null"))
				mustUseEquiJoins = false; // if And logic and no "is null" criteria all use of outer joins

			if (nonCartridgeStructureCriteriaList.Count > 1 && mqlLogicType != QueryLogicType.And && mqlLogicType != QueryLogicType.Or)
			{ // return error if non-Cartridge criteria are beyond what we can handle
				foreach (int tki2 in nonCartridgeStructureCriteriaList)
				{
					if (errorMessage == null)
						errorMessage = "Multiple \"non-Oracle\" criteria are not allowed for advanced query criteria: ";
					else errorMessage += ", ";

					errorMessage += tokens[tki2].Qc.MetaColumn.Label;
				}
				errorPosition = nonCartridgeStructureCriteriaList[1];
				return false;
			}

			critToksAfterDebug = MqlUtil.CatenateCriteriaTokensForDebug(tokens);

			bool DisableKeyListCriteria = true; // disable key lists in main Sql and include at end
			if (DisableKeyListCriteria)
			{
				if ((keyCriteriaInListKeys != null && keyCriteriaInListKeys.Count > 0) ||
				 (keyCriteriaSavedListKeys != null && keyCriteriaSavedListKeys.Count > 0))
					tokens =
					 MqlUtil.DisableCriteria(tokens, query, null, null, null, null, false, false, false);
			}

			return true;
		}

		/// <summary>
		/// FilterRootTablesForStructureSearchCriteria
		/// </summary>
		/// <param name="rootMetaTables"></param>

		public List<MetaTable> FilterRootTablesForStructureSearchCriteria(
				List<MetaTable> rootMetaTables)
		{
			ParsedStructureCriteria pssc = null;
			RootTable rt;

			List<MetaTable> filteredRootMetaTables = new List<MetaTable>();

			if (rootMetaTables == null || rootMetaTables.Count == 0) return rootMetaTables;

			QueryColumn qc = Query.GetFirstStructureCriteriaColumn();
			if (qc == null) return rootMetaTables;

			QueryTable qt = qc.QueryTable;
			MetaTable mt = qt.MetaTable;

			bool parseOk = ParsedStructureCriteria.TryParse(qc, out pssc);
			if (!parseOk) return rootMetaTables;
			StructureSearchType sst = pssc.SearchType;

			foreach (MetaTable rmt in rootMetaTables)
			{
				rt = RootTable.GetFromTableName(rmt.Name);

				if (pssc.IsChemistrySearch)
				{
					if (mt.IsUserDatabaseStructureTable || mt.IsUserObjectTable) { }
					else
					{
						if (rt == null) continue;
						if (!rt.CartridgeSearchable) continue;
					}
				}

				else if (sst == StructureSearchType.SmallWorld)
				{
					if (rt == null) continue;
					if (!rt.SmallWorldSearchable) continue;
				}

				else if (sst == StructureSearchType.Related)
				{
					if (rt == null) continue;
					if (!rt.CartridgeSearchable && !rt.SmallWorldSearchable) continue;
				}

				else if (sst == StructureSearchType.MatchedPairs)
				{
					if (Lex.Ne(mt.Name, MetaTable.PrimaryRootTable)) continue;
				}

				else continue;

				filteredRootMetaTables.Add(rmt);
			}

			return filteredRootMetaTables;
		}

		/// <summary>
		/// Get list of specific keys in query criteria including a saved list, 
		/// inline in-list and key = cid values. Don't include keys in ranges.
		/// </summary>
		/// <returns></returns>

		List<string> GetQueryKeyList()
		{
			List<string> queryKeyList = null;
			if (KeyCriteriaSavedListKeys != null && KeyCriteriaSavedListKeys.Count > 0)
				queryKeyList = KeyCriteriaSavedListKeys;

			else if (KeyCriteriaInListKeys != null && KeyCriteriaInListKeys.Count > 0)
				queryKeyList = KeyCriteriaInListKeys;

			else if (KeyCriteriaPositions != null && KeyCriteriaPositions.Count == 1)
			{ // equality match on key
				int i1 = KeyCriteriaPositions[0];
				if (i1 + 2 < CritToks.Count && CritToks[i1 + 1].Tok.Text == "=")
				{
					queryKeyList = new List<string>();
					string cid = CompoundId.Normalize(CritToks[i1 + 2].Tok.Text, QueryEngine.GetRootTable(Query));
					queryKeyList.Add(cid);
				}
			}

			return queryKeyList;
		}

		/// <summary>
		/// Return true if this looks like an imprecise structure search (e.g. a small fragment SS query)
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		public static bool StructureSearchMayBeSlow(
				Query query,
				List<MqlToken> tokens,
				int position)
		{
			if (position < 2 ||
					position + 2 >= tokens.Count) throw new QueryException("Invalid syntax for structure search");

			QueryColumn qc = tokens[position].Qc;
			if (!Lex.Eq(tokens[position - 2].Tok.Text, "sss")) return false;

			string chime = tokens[position + 2].Tok.Text;
			chime = Lex.RemoveAllQuotes(chime);
			if (String.IsNullOrEmpty(chime)) return false;

			MoleculeMx cs = new MoleculeMx(MoleculeFormat.Chime, chime);
			int atomCnt = cs.AtomCount;

			int smallSsqMaxAtoms = 12; // default top limit for small queries
			if (query.SmallSsqMaxAtoms >= 0)
				smallSsqMaxAtoms = query.SmallSsqMaxAtoms;

			if (atomCnt > smallSsqMaxAtoms) return false;

			// This appears to be a small structure that may return many hits and be slow.
			// To check this, we'll start the search and let it run for a few seconds to verify if it is really "slow" since it may or may not be

			if (!VerifyImpreciseStructureSearches) return true; // assume slow if verification not enabled

			if (qc == null || qc.QueryTable == null) return false;
			QueryTable qt = qc.QueryTable;
			MetaTable mt = qt.MetaTable;
			if (!MqlUtil.IsCartridgeMetaTable(mt)) return false;

			string sql = "select count(*) from " + mt.TableMap;

			string qss = MqlUtil.ConvertStringToValidOracleExpression(chime);
			string criteria = "sss(ctab," + qss + ") = 1";
			sql += " where " + criteria;

			try
			{
				DateTime t0 = DateTime.Now;
				DbCommandMx cmd = new DbCommandMx();
				cmd.Timeout = 5; // 5 sec timeout
				cmd.Prepare(sql);
				cmd.ExecuteReader();
				cmd.Read();
				int count = cmd.GetInt(0);
				cmd.Dispose();
				double ms = MSTime.Delta(t0);
				//DebugLog.Message("ImpreciseStructureSearch: No timeout, " + chime);
				return false;
			}

			catch (Exception ex) // timeout or other exception
			{
				if (ex is TimeoutException)
				{
					//DebugLog.Message("ImpreciseStructureSearch: Timeout, " + chime);
					return true;

				}
				else
				{
					//DebugLog.Message("ImpreciseStructureSearch: Exception, " + chime + "\r\n" + DebugLog.FormatExceptionMessage(ex));
					return false;
				}
			}
		}

		/// <summary>
		/// Reopen result set
		/// </summary>

		public void Reopen()
		{
			CursorPos = -1; // position cursor at beginning to allow reread
			State = QueryEngineState.Retrieving;

			ChunkKeyCount = 0;
			return;
		}

		/// <summary>
		/// Retrieve all rows in a single call
		/// </summary>
		/// <returns></returns>

		public List<object[]> RetrieveAllRows()
		{
			List<object[]> rows = NextRows(-1, -1, -1);
			return rows;
		}

		/// <summary>
		/// Complete retrieval of all rows and hold within the QE
		/// </summary>
		/// <returns></returns>

		public QueryEngineStats CompleteRowRetrieval()
		{

			lock (this) // lock the QE while getting all of the rows
			{
				int rowsRead = 0;

				VoArrayListCursor savedState = RowCursor.Save(); // save current cursor position

				while (NextRow2() != null)
				{
					rowsRead++;
				}

				savedState.Restore(ref RowCursor); // restore cursor position

				Stats.Cancelled = Cancelled;
			}

			return Stats;
		}

		////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Read a list of rows returning result in serialized form
		/// </summary>
		/// <param name="minRows">Minimum number of rows to read</param>
		/// <param name="maxRows">Maximum number of rows to read</param>
		/// <param name="maxTime">Maximum time in milliseconds provided at least one row is returned</param>
		/// <param name="mbStats">If list is non-null return stats in list</param>
		/// <returns>Serialized byte array</returns>
		////////////////////////////////////////////////////////////////////////////////

		public byte[] NextRowsSerialized(
				int minRows,
				int maxRows,
				int maxTime,
				List<MetaBrokerStats> mbStats)
		{
			int[] mbRowCounts, mbTimes;
			string s = null;

			List<object[]> rows = NextRows(minRows, maxRows, maxTime, mbStats);
			if (rows == null) return null;

			byte[] ba = VoArray.SerializeBinaryVoArrayListToByteArray(rows);
			return ba;
		}

		////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Read a list of rows 
		/// </summary>
		/// <param name="minRows">Minimum number of rows to read</param>
		/// <param name="maxRows">Maximum number of rows to read</param>
		/// <param name="maxTime">Maximum time in milliseconds provided at least one row is returned</param>
		/// <param name="mbStats">If list is non-null return stats in list</param>
		/// <returns>List of value object arrays</returns>
		////////////////////////////////////////////////////////////////////////////////

		public List<object[]> NextRows(
				int minRows,
				int maxRows,
				int maxTime,
				List<MetaBrokerStats> mbStats)
		{
			//minRows = maxRows = 256000; // debug
			//maxTime = 5000; // debug

			List<object[]> rows = NextRows(minRows, maxRows, maxTime);

			if (mbStats == null) return rows;

			int qtc = Query.Tables.Count;
			mbStats.Clear();

			if (SearchBrokerList != null) // return information for search step
			{
				foreach (GenericMetaBroker mb0 in SearchBrokerList)
				{
					MetaBrokerStats bs = new MetaBrokerStats();
					bs.Label = mb0.Label;
					bs.MetatableRowCount = mb0.ReadRowCount; // make same as number of key reads
					bs.OracleRowCount = mb0.ReadRowCount;
					bs.Time = (int)(mb0.ReadRowTime + mb0.ExecuteReaderTime); // store as integer milliseconds

					mbStats.Add(bs);
				}
			}

			for (int ti = 0; ti < Query.Tables.Count; ti++)
			{
				MetaBrokerStats bs = new MetaBrokerStats();
				GenericMetaBroker mb = Qtd[ti].Broker;
				if (!mb.PivotInCode) bs.MultiPivot = 0; // no multipivot
				else if (mb.IsKeyMultipivotBrokerInGroup) bs.MultiPivot = 1;
				else bs.MultiPivot = 2;

				bs.MetatableRowCount = mb.NextRowCount;
				bs.OracleRowCount = mb.ReadRowCount;
				bs.Time = (int)(mb.ReadRowTime + mb.ExecuteReaderTime); // store as integer milliseconds

				mbStats.Add(bs);
			}

			return rows;
		}

		////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Read a list of rows 
		/// </summary>
		/// <param name="minRows">Minimum number of rows to read</param>
		/// <param name="maxRows">Maximum number of rows to read</param>
		/// <param name="maxTime">Maximum time in milliseconds provided at least one row is returned</param>
		/// <returns>List of value object arrays</returns>
		////////////////////////////////////////////////////////////////////////////////

		public List<object[]> NextRows(
				int minRows = -1,
				int maxRows = -1,
				int maxTime = -1)
		{
			List<object[]> rows = new List<object[]>(); // build list of rows here

			if (IsAggregationDrillDown()) return GetAggregationDrillDownRows();

			NextRowsMinRows = minRows; // store for rest of QE to check
			NextRowsMaxRows = maxRows;
			NextRowsMaxTime = maxTime;
			NextRowsStopwatch.Restart();

			while (true)
			{
				object[] vo = NextRow(false);
				if (vo == null) // end of data
				{
					if (rows.Count == 0) return null;
					else break;
				}

				rows.Add(vo);
				if (maxRows >= 0 && rows.Count >= maxRows) break;
				if (rows.Count >= minRows && NextRowsMaxTimeExceeded()) break; // at time limit & have at least minimum number of rows?
			}

			List<MoleculeMx> molList = SetMoleculeCids(rows); // set the Cid in any molecule objects in the row set

			if (PrefetchStoredHelmSvgImagesEnabled) // prefetch stored Helm Svg images for faster rendering
				StructureTableDao.SelectMoleculeListSvg(molList); // get svgs & store in mol objects

			if (Query.PrefetchImages && AllowPrefetchOfImages)
				PrefetchImages(rows);

			NextRowsTotalTime += NextRowsStopwatch.Elapsed.TotalMilliseconds;
			return rows;
		}

		////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return the next matching row value object
		/// </summary>
		/// <returns>Value object containing next row or null if no more rows</returns>
		////////////////////////////////////////////////////////////////////////////////

		public object[] NextRow()
		{
			return NextRow(Query.PrefetchImages);
		}

		/// <summary>
		/// Return the next matching row value object
		/// </summary>
		/// <param name="prefetchImages"></param>
		/// <returns>Value object containing next row or null if no more rows</returns>

		public object[] NextRow(bool prefetchImages)
		{
			Object[] vo;

			try
			{

				if (ParentQe != null) // If working off of existing parent query engine then call that engine
				{

					VoArrayListCursor savedCursor = ParentQe.RowCursor.Save();

					ParentQe.RowCursor = RowCursor; // use our cursor for parent retrieval

					vo = ParentQe.NextRow();
					CurrentRow = vo;

					savedCursor.Restore(ref ParentQe.RowCursor); // restore parent
					Cancelled = ParentQe.Cancelled; // pass through any parent cancel

					return vo;
				}

				if (!CanAttemptToFetchRows) return null;

				// See if results need to be fully retrieved & sorted before return

				if (ResultsRows.TotalRowCount == 0) // check upon first row retrieval
				{
					List<SortColumn> sCols = Query.GetSortColumns();
					if (sCols.Count > 1 ||
							(sCols.Count == 1 && !sCols[0].QueryColumn.IsKey))
					{
						CompleteRowRetrieval();
						if (Cancelled) return null;

						ResultsSorter rs = new ResultsSorter();

						int keyValueVoPos = 0;
						ResultsRows.RowBuffer = rs.Sort(ResultsRows.RowBuffer, sCols, Query, keyValueVoPos, out ResultsKeys);

						CursorPos = -1; // reset cursor to start
						State = QueryEngineState.Retrieving; // still retrieving
					}
				}

				vo = NextRow2();

				if (vo != null && prefetchImages)
				{
					List<object[]> rowList = new List<object[]>();
					rowList.Add(vo);
					PrefetchImages(rowList);
				}

				return vo;
			}

			catch (Exception ex)
			{
				string msg = ex.Message;
				if (!LoggedQueryException)
					msg += " " + LogExceptionAndSerializedQuery(ex);

				throw new Exception(msg, ex); // pass it up
			}
		}

		////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return the next matching row value object
		/// This method retrieves the next row of data. It moves through a 3 level hierarchy
		/// 1. The set of roots
		/// 2. The set of keys in the current key set
		/// 3. The set of rows for the current key
		/// </summary>
		/// <returns>Value object containing next row or null if no more rows</returns>
		////////////////////////////////////////////////////////////////////////////////

		public object[] NextRow2()
		{
			int keyOffset;
			bool CurrentKeyDataSeen;
			Object[] vo, vo2;
			QueryTable qt;
			MetaTable mt;
			MetaColumn mc;
			string cn, txt;
			int ti, offset, i1, i2;
			bool moreDataAvailable;

			lock (this) // this method is not reentrant for a QE instance
			{

				// Loop til we get a result or are at end of cursor

				while (true)
				{
					int ri = CursorPos + 1; // index of row we want
					if (ResultsRows.TryToGetNextRow(RowCursor))
					{
						vo = RowCursor.CurrentRow;
						if (vo != null && vo.Length > 0)
						{
							if (RestrictedDatabaseView.KeyIsRetricted(vo[0].ToString()))
								continue; // don't return if restricted key
						}
						return vo;
					}

					// Need to get the next chunk of rows if available
					// First check to see if we need to cache and/or purge the existing rows in ResultsRows

					ResultsRows.MoveRowsToCacheAsAppropriate();

					// Get next chunk of results

					if (QeSingleStepExecution)
						moreDataAvailable = !Qtd[DrivingTableIdx].Closed;

					else // say more data available if more keys are available in the keys for current root
						moreDataAvailable = (CurrentKeyIdx + 1 < KeysForRoot.Count);

					if (moreDataAvailable)
					{
						GetResultsChunk(); // get the data for the next chunk of keys
						if (Cancelled) return null;
						else if (CursorPos + 1 >= ResultsRows.TotalRowCount) return null; // no more results
						continue; // go pick up result from buffer
					}

					// Go to the next root table

					RootTableIdx++; // move to next root table
					if (RootTableIdx >= RootTables.Count) // really done?
					{
						if (LogBasics)
						{
							int count = Stats.TotalRowCount;
							string msg = "!!!!!! Retrieval Complete - Total rows: " + count + ", time: " + Stats.TotalRowFetchTime;
							if (count > 0) msg += ", time/row: " + ((Stats.TotalRowFetchTime * 1000) / count) / 1000.0;
							msg += " !!!!!!";
							DebugLog.Message(msg);
						}

						State = QueryEngineState.Closed;
						return null;
					}

					MetaTable rootTable = RootTables[RootTableIdx];
					if (!PrepareForRetrieval(rootTable, out KeysForRoot))
						continue; // nothing for root
					if (Cancelled) return null;

					CurrentKeyIdx = -1; // start with the first key
					continue;
				}
			} // lock

		}

		/// <summary>
		/// Export data retrieved by query to Spotfire files
		/// </summary>
		/// <param name="exportFileFormat"></param>
		/// <param name="fileName"></param>
		/// <param name="fileName2"></param>
		/// <param name="structureExportFormat"></param>
		/// <param name="splitQNs"></param>
		/// <returns></returns>

		public QueryEngineStats ExportDataToSpotfireFiles(
				ExportParms ep)
		{
			string result = new SpotfireDataExporter().WriteSpotfireDataFiles(Query, ResultsRows, ep);

			Stats.Message = result;
			return Stats;
		}

		/// <summary>
		/// GetStandardMobileQueries
		/// </summary>
		/// <returns></returns>

		public static Query[] GetStandardMobileQueries()
		{
			List<UserObject> mobiusUserObjects = UserObjectDao.ReadMultiple(Data.UserObjectType.Query, "MOBIUS", false, true);
			List<Data.Query> mobileQueries = new List<Data.Query>();
			foreach (UserObject userObject in mobiusUserObjects)
			{
				Data.Query query = Data.Query.Deserialize(userObject.Content);
				if (query.Mobile) mobileQueries.Add(query);
			}
			return mobileQueries.ToArray();
		}

		public static Query[] GetMobileQueriesByOwner(string currentUser)
		{
			DebugLog.Message("currentUser: " + currentUser);
			List<UserObject> mobiusUserObjects = UserObjectDao.ReadMultiple(Data.UserObjectType.Query, currentUser, false, true);
			List<Data.Query> mobileQueries = new List<Data.Query>();
			foreach (UserObject userObject in mobiusUserObjects)
			{
				try
				{
					// I think it is faster to use XML objects and check the mobile attributer
					// rather than deserializing every object and checking the mobile parameter.
					XmlMemoryStreamTextReader mstr = new XmlMemoryStreamTextReader(userObject.Content);
					System.Xml.XmlTextReader tr = mstr.Reader;
					if (tr.ReadState == System.Xml.ReadState.Initial) //  read first element if not done yet
					{
						tr.Read();
						tr.MoveToContent();
						string mobileString = tr.GetAttribute("Mobile");
						bool mobile = Convert.ToBoolean(mobileString);
						if (mobile)
						{
							Data.Query query = Data.Query.Deserialize(userObject.Content);
							if (query.Mobile)
							{
								// the UserObject.Id found in the xml is not always there and cannot be trusted.
								query.UserObject.Id = userObject.Id;
								query.UserObject.Owner = userObject.Owner;
								mobileQueries.Add(query);
							}
						}
					}
				}
				catch (Exception ex)
				{
					DebugLog.Message("Unable to deserialize Query " + userObject.Name);
					continue;
				}
			}
			return mobileQueries.ToArray();
		}

		public static int GetDefaultQuery(string userId)
		{
			int queryId = -1;
			string id = UserObjectDao.GetUserParameter(userId, "MobileDefaultQuery");
			if (!string.IsNullOrEmpty(id)) queryId = Convert.ToInt32(id);
			return queryId;
		}

#if false // PassNullRowFilter not used
/// <summary>
/// See if this is a row with all null values (other than key) that should be filtered out
/// </summary>
/// <param name="vo"></param>
/// <returns></returns>

		bool PassNullRowFilter(
			object [] vo)
		{
			int ti, fi;
			bool seenNonKey = false;

			if (Math.Sqrt(1) == 1) return true; // disabled for now

			if (!Query.FilterNullRows) return true;

			for (ti=0; ti<Query.Tables.Count; ti++)
			{
				QueryTable qt = Query.Tables[ti];
				for (fi=0; fi<qt.QueryColumns.Count; fi++) // look for non-null data for at least one field
				{
					QueryColumn qc = qt.QueryColumns[fi];
					if (!qc.Selected) continue;
					if (qc.IsKeyMetaColumn) continue; // don't count key columns

					seenNonKey = true;

					object voItem = vo[qc.VoPosition];
					if (voItem == null) continue;
					if (voItem is QualifiedNumber && 
						((QualifiedNumber)voItem).IsNull) continue;

					return true; // non-key non-null value
				}
			}

			if (seenNonKey) return false; // filter out if we saw a non-key value
			else return true;
		}
#endif

		/// <summary>
		/// Open query to retrieve data from a single table combined search and retrieval query
		/// </summary>
		/// <returns></returns>

		bool OpenSingleStepQuery()
		{
			QueryTable qt;
			int t0;

			DrivingTable = Query.GetFirstTableWithCriteria();
			if (DrivingTable == null) DrivingTable = Query.Tables[0];
			DrivingTableIdx = Query.GetTableIndexByName(DrivingTable.MetaTable.Name); // index of driving table

			MetaTable mt = DrivingTable.MetaTable;
			MetaTable rootTable = mt.Root;
			if (!PrepareForRetrieval(rootTable, out KeysForRoot)) return false;
			ExecuteQueryParms eqp = Qtd[DrivingTableIdx].Eqp;

			t0 = TimeOfDay.Milliseconds();

			//RootTableIdx = 0; // set to the first (and only) root table

			Qtd[DrivingTableIdx].Broker.ExecuteQuery(eqp);
			if (eqp.Cancelled)
			{
				Cancelled = true;
				return false;
			}
			Qtd[DrivingTableIdx].RowsFetched = 0;

			if (LogBasics)
			{
				t0 = TimeOfDay.Milliseconds() - t0;
				DebugLog.Message("OpenSingleStepQuery" +
						", table: " + eqp.QueryTable.MetaTable.Name +
						", time: " + t0);
			}

			return true;
		}

		/// <summary>
		/// Get the next result for a combined search and retrieval query on a single table
		/// </summary>
		/// <returns></returns>

		//object[] GetNextSingleStepQueryRow()
		//{
		//  object [] vo, vo2;
		//  int ti=0 , keyOffset=1, offset, i1, t0;

		//  t0 = TimeOfDay.Milliseconds();

		//  vo2 = Qtd[ti].Broker.NextRow();
		//  if (Qtd[ti].Broker.GetExecuteQueryParms.Cancelled) 
		//  {
		//    Cancelled = true;
		//    return null;
		//  }

		//  if (DebugLevel > 0 && Qtd[ti].RowsFetched == 0)
		//  {
		//    t0 = TimeOfDay.Milliseconds() - t0;
		//    ExecuteQueryParms eqp = Qtd[ti].Eqp;
		//    DebugLog.Message("QE GetNextSingleStepQueryRow NextRow" +
		//      ", table: " + eqp.QueryTable.MetaTable.Name +
		//      ", time: " + t0 +
		//      ", first key: " + (vo2 != null && vo2[0] != null ? GetKeyString(vo2[0]) : "null"));
		//  }

		//  if (vo2==null) return null;
		//  Qtd[ti].RowsFetched++;

		//  keyOffset=1; // always prepend the current key to the Vo
		//  vo = new Object[TotalColumnsSelected + keyOffset]; // vo to be filled 
		//  vo[0] = vo2[Qtd[ti].KeyColPos]; // copy key to first position
		//  Results.Add(vo); // add to results buffer for later pickup
		//  ApplyDataTransforms(Qtd[ti], keyOffset, vo2); // transform data
		//  CopyTableVoToCompositeVo(Qtd[ti], keyOffset, vo2, vo); // move vo data for this table into composite buffer
		//  return vo2;
		//}

		////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Retrieve data for a chunk of keys in a two step process:
		/// 1. Get the set of data for the set of keys for each table
		/// 2. Merge together and add to Results buffer
		/// 
		/// This process keeps the data in key order in KeysForRoot and calls the
		/// metabrokers in chunkwise style which minimizes the trips to/from Oracle
		/// and improves performance.
		/// </summary>
		/// <param name="CurrentKey"></param>
		////////////////////////////////////////////////////////////////////////////////

		void GetResultsChunk()
		{
			QueryTable qt;
			MetaTable mt;
			MetaColumn mc;
			object[] vo = null, vo2 = null;
			string keyVal, txt;
			int keyOffset = 1, offset, tDelta, ti, ri, i1, i2;
			int firstResult = 0, firstKeyIdx = -1, lastKeyIdx = -1, keyCount, ki;

			SetChunkKeyCount();

			while (true)
			{
				ChunkExecuteTime = ChunkFetchTime = 0; // init overall query execute & fetch time

				// Setup the set of keys for the chunk and data structures for unmerged resulting data

				List<string> chunkKeys = new List<string>(100); // set of keys to get data for goes here

				if (DrivingTable == null)
				{
					firstKeyIdx = CurrentKeyIdx + 1;
					lastKeyIdx = firstKeyIdx + ChunkKeyCount - 1;
					if (lastKeyIdx >= KeysForRoot.Count) // past end of subset?
						lastKeyIdx = KeysForRoot.Count - 1;
					keyCount = lastKeyIdx - firstKeyIdx + 1;
					if (keyCount <= 0) return; // oops, can't get anything

					if (Aggregation.IsNonKeyAggreation(Query))
					{ // If aggregating a single table but aggreation doesn't include the key then get all of the data for the table as a single chunk
						firstKeyIdx = 0;
						lastKeyIdx = KeysForRoot.Count - 1;
					}

					CurrentKeyIdx = lastKeyIdx; // set current index at last key for chunk

					for (ki = firstKeyIdx; ki <= lastKeyIdx; ki++)
					{
						chunkKeys.Add(KeysForRoot[ki].ToString()); // build list of keys to get data for
					}

				}

				// Get the next set of rows & keys from the driving table

				else
				{
					chunkKeys = GetDrivingTableDataChunk();
					if (chunkKeys.Count == 0) return;
				}

				// Set up the structures to receive the data

				ChunkTableData chunkData = new ChunkTableData(chunkKeys, Query.Tables.Count);

				List<object[]>[] tablesForKey = null; // array with one entry for each table for a key
				List<object[]> rowsForTableForKey; // data rows for a key for a single table (object[] is the vo containing a single row's row data)

				if (LogBasics)
				{
					string keyPredType = DbCommandMx.DefaultKeyListPredType.ToString() + "(" + chunkKeys.Count + ")";
					DebugLog.Message("------ Chunk Started - Tables: " + Query.Tables.Count +
							", KeyListPredType: " + keyPredType + " ------");
				}

				RetrieveTableDataForKeyset(chunkKeys, chunkData); // Get Data for each table for the set of keys

				AggregateData(chunkData);

				// Merge the data together by key and store in results (Hierarchical)

				if (ReturnHierarchicalResults)
				{
					firstResult = ResultsRows.RowBufferRowCount; // where first result will go

					for (ki = firstKeyIdx; ki <= lastKeyIdx; ki++)
					{
						keyVal = KeysForRoot[ki].ToString();
						tablesForKey = chunkData.TableDataForKey(keyVal);
						object[] parentVo = null; // parent to be filled in
						for (ti = 0; ti < Query.Tables.Count; ti++) // take data from each table
						{
							rowsForTableForKey = tablesForKey[ti]; // get rows for table
							if (rowsForTableForKey == null) continue;

							for (ri = 0; ri < rowsForTableForKey.Count; ri++)
							{
								vo2 = (object[])rowsForTableForKey[ri];
								ApplyDataTransforms(Qtd[ti], keyOffset, vo2); // transform data
								if (((ti == 0 && Query.Tables.Count == 1 && Qtd[ti].SelectCount > 1) || // if only table with more than one col selected
										ti > 0) && // or beyond 1st table 
										!PassTableNullRowFilter(vo2)) continue; // then filter if all null except key

								i1 = Qtd[ti].SelectCount;
								if (Qtd[ti].ChildTables != null) i1 += Qtd[ti].ChildTables.Count;
								vo = new Object[i1];
								Array.Clear(vo, 0, i1);

								if (ti == 0) // if first table then add to results
								{
									ResultsRows.AddRow(vo);
									Stats.TotalRowCount++;
									parentVo = vo;
								}

								else
								{ // non-first table links into parent vo
									if (parentVo == null) continue; // shouldn't happen
									ArrayList voa = (ArrayList)parentVo[Qtd[ti].ParentVoPos];
									if (voa == null)
									{
										voa = new ArrayList();
										parentVo[Qtd[ti].ParentVoPos] = voa;
									}
									voa.Add(vo);
								}

								Qtd[ti].TableVoPosition = 0; // FirstColumn value is zero for hierarchical results
								keyOffset = 0; // no offset for key
								CopyTableVoToCompositeVo(Qtd[ti], keyOffset, vo2, vo); // move vo data for this table
							} // end for result loop
						} // end for table loop
					} // end for key loop

					if (firstResult == ResultsRows.RowBufferRowCount) continue; // if we didn't get anything then try again
				}

				// Merge the data together by key and store in results (Flattened)

				else
				{
					firstResult = ResultsRows.RowBufferRowCount; // where first result will go

					int[] posForTable = new int[Query.Tables.Count];
					//for (ki = firstKeyIdx; ki <= lastKeyIdx; ki++)
					//{
					//  keyVal = KeysForRoot[ki].ToString();

					foreach (string key0 in chunkData.KeyToTableDataDict.Keys)
					{
						keyVal = key0;
						tablesForKey = chunkData.TableDataForKey(keyVal);
						int voCreateCount = 0; // number of vos created for this key

						Array.Clear(posForTable, 0, posForTable.Length); // reset position for each table
						for (ri = 0; ; ri++) // build set of result rows for this key
						{
							bool voCreated = false;

							for (ti = 0; ti < Query.Tables.Count; ti++) // take a row from each table
							{
								qt = Query.Tables[ti];
								mt = qt.MetaTable;
								//if (!mt.RetrievesDataFromQueryEngine) continue; // todo: include later

								rowsForTableForKey = tablesForKey[ti];
								bool haveDataForTable = false;
								while (true) // try to find a valid row of data for this table
								{
									if (rowsForTableForKey == null || rowsForTableForKey.Count <= posForTable[ti]) break;
									vo2 = (object[])rowsForTableForKey[posForTable[ti]];
									ApplyDataTransforms(Qtd[ti], keyOffset, vo2); // transform data
									posForTable[ti]++;
									if (!PassTableNullRowFilter(vo2)) continue;
									haveDataForTable = true;
									break;
								}
								if (!haveDataForTable) continue;

								if (!voCreated)
								{
									keyOffset = 1; // always prepend the current key to the Vo
									vo = new Object[TotalColumnsSelected + keyOffset]; // vo to be filled 
									vo[0] = keyVal; // copy common key
									if (ri == 0) vo[1] = keyVal; // copy key value for 1st table 1st time (may not pass null filter if only key is selected)

									ResultsRows.AddRow(vo);

									Stats.TotalRowCount++;
									voCreated = true;
									voCreateCount++;
								}

								CopyTableVoToCompositeVo(Qtd[ti], keyOffset, vo2, vo); // move vo data for this table into composite buffer
							}
							if (!voCreated) break; // done if no more data for key
						} // end for result merge

						if (voCreateCount == 0 && !Query.FilterNullRowsStrong) // nothing created for key (everything filtered out)
						{
							keyOffset = 1; // always prepend the current key to the Vo
							vo = new Object[TotalColumnsSelected + keyOffset]; // vo to be filled 
							vo[0] = keyVal; // copy common key
							vo[1] = keyVal; // copy key value for 1st table
							ResultsRows.AddRow(vo);
						}

					} // end for key loop

					if (firstResult == ResultsRows.RowBufferRowCount) continue; // if we didn't get anything then try again

					// Get data for tables whose values are calculated from other retrieved data
					List<int> queryTableOrderOfExecution = getQueryTableOrderOfExecution();

					foreach (int i in queryTableOrderOfExecution)
					{
						qt = Query.Tables[i];
						mt = qt.MetaTable;
						//if (!mt.RetrievesDataFromQueryEngine) continue; // todo: include later

						if (Qtd[i].Broker == null) continue;
						if (!Qtd[i].Broker.CanBuildDataFromOtherRetrievedData(this, i)) continue;
						int resultCount = ResultsRows.RowBufferRowCount - firstResult;
						Qtd[i].Broker.BuildDataFromOtherRetrievedData(this, i, ResultsRows.RowBuffer, firstResult, resultCount);
					}

				}

				break;
			} // loop until we get some data or are at end of keys

			// Output debug info

			int tt = ChunkExecuteTime + ChunkFetchTime;
			Stats.TotalRowFetchTime += tt;
			ChunksRetrievedCount++; // count the chunk

			if (LogBasics) // total time
			{
				DebugLog.Message("Chunk Execute/Fetch - Total time: " + tt);
			}

			if (LogDataRows) // log the data rows retrieved
			{
				for (ri = firstResult; ri < ResultsRows.RowBufferRowCount; ri++)
				{
					vo = (object[])ResultsRows.RowBuffer[ri];
					string d = ""; // build here

					//				d += "Results for " + Query.Tables.Count.ToString() + " tables, " +
					//					"result count = " + Results.Count.ToString() + "\n";

					StringBuilder sb = new StringBuilder();

					for (i1 = 0; i1 < ResultsMetaColumns.Count; i1++)
					{
						mc = (MetaColumn)ResultsMetaColumns[i1];
						if (i1 == 0)
							sb.Append(String.Format("{0,12} ", "Key"));
						else sb.Append(String.Format("{0,12} ", mc.Name));
					}

					d += sb.ToString() + "\n";

					//					if (i1>=100) break;
					sb = new StringBuilder();

					for (i2 = 0; i2 < vo.Length; i2++)
					{
						mc = (MetaColumn)ResultsMetaColumns[i2];
						if (vo[i2] == null)
							txt = "<null>";
						else if (i2 == 0) txt = vo[i2].ToString(); // common key element
																											 //					else if (mc.DataType == MetaColumnType.QualifiedNo )
																											 //						txt = ((QualifiedNumber)vo[i2]).TextValue;
						else
							txt = vo[i2].ToString();
						sb.Append(String.Format("{0,12} ", txt));
					}
					d += sb.ToString() + "\n";
				}
			}

			//			DebugLog.Message("Fill Time: " + GetResultsChunkExecutionTime + ", Size: " + RetrieveKeyBlockSize);

			return;
		}

		/// <summary>
		/// Get the order of query table execution with the calculated fields in dependent order.  For example, calc field 2 might need to be executed before calc field 1.
		/// </summary>
		/// <returns></returns>
		private List<int> getQueryTableOrderOfExecution()
		{
			List<QueryTable> unsortedQts = new List<QueryTable>();
			List<QueryTable> qtsInDependencyOrder = new List<QueryTable>();
			int ti;

			for (ti = 0; ti < Query.Tables.Count; ti++)
			{
				if (!Query.Tables[ti].MetaTable.IsCalculatedField)
				{
					qtsInDependencyOrder.Add(Query.Tables[ti]);
				}
			}

			for (ti = 0; ti < Query.Tables.Count; ti++)
			{
				if (Query.Tables[ti].MetaTable.IsCalculatedField)
				{
					unsortedQts.Add(Query.Tables[ti]);
				}
			}

			Dictionary<string, List<string>> calcFieldDependencyLookup = new Dictionary<string, List<string>>();

			while (unsortedQts.Count > 0)
			{
				QueryTable topUnsortedQt = unsortedQts[0];

				if (!calcFieldDependencyLookup.ContainsKey(topUnsortedQt.MetaTableName))
				{
					calcFieldDependencyLookup[topUnsortedQt.MetaTableName] = GetCalcFieldDependencies(topUnsortedQt.MetaTableName);
				}

				List<string> calcFieldDependencies = calcFieldDependencyLookup[topUnsortedQt.MetaTableName];

				if (calcFieldDependencies == null)
				{
					unsortedQts.RemoveAt(0);
					qtsInDependencyOrder.Add(topUnsortedQt);
					continue;
				}

				bool addQt = true;
				foreach (QueryTable unsortedQt in unsortedQts)
				{
					if (calcFieldDependencies.Contains(unsortedQt.MetaTableName))
					{
						addQt = false;
						break;
					}
				}

				if (addQt)
				{
					unsortedQts.RemoveAt(0);
					qtsInDependencyOrder.Add(topUnsortedQt);
				}
				else
				{
					//move unsorted qt to the bottom of the list...
					unsortedQts.RemoveAt(0);
					unsortedQts.Add(topUnsortedQt);
				}
			}

			List<int> queryTableOrderOfExecution = new List<int>();
			foreach (QueryTable nextQt in qtsInDependencyOrder)
			{
				ti = Query.GetQueryTableIndex(nextQt);
				queryTableOrderOfExecution.Add(ti);
			}

			return queryTableOrderOfExecution;

		}

		/// <summary>
		/// Set ChunkKeyCount for the next retrieval
		/// </summary>

		void SetChunkKeyCount()
		{
			// Decide how big the next chunk should be 
			// Increase chunk size up to max if emptying faster than filling		

			if (NextRowsMaxTime > 0) // time limit specified?
			{
				if (ChunksRetrievedCount == 0) // first chunk
					ChunkKeyCount = ChunkKeyCountInitial;

				else if (ChunkKeyCount < ChunkKeyCountMax) // subsequent chunk
				{
					ChunkKeyCount *= ChunkKeyCountNextMultiplier;
					if (ChunkKeyCount > ChunkKeyCountMax)
						ChunkKeyCount = ChunkKeyCountMax;
				}

				else ChunkKeyCount = ChunkKeyCountMax; // don't exceed max
			}

			else ChunkKeyCount = ChunkKeyCountMax; // if no time limit then start with max

			if (IsOracleQuery) // adjust for Oracle
			{
				if ((DbCommandMx.DefaultKeyListPredType == KeyListPredTypeEnum.Parameterized || // limit max keys if necessary
						DbCommandMx.DefaultKeyListPredType == KeyListPredTypeEnum.Literal) &&
						ChunkKeyCount > DbCommandMx.MaxOracleInListItemCount)
				{
					ChunkKeyCount = DbCommandMx.MaxOracleInListItemCount;
				}
			}

			else // adjust for ODBC
			{
				ChunkKeyCount = ChunkKeyCount; // no adjust for now
			}

			if (ChunkKeyCount < 1) ChunkKeyCount = 1; // be sure > 0

			if (LogBasics) DebugLog.Message("ChunkKeyCount: " + ChunkKeyCount);
			return;
		}

		/// <summary>
		/// Set ChunkKeyCount for the next retrieval
		/// </summary>

#if false
		void SetChunkKeyCountOld()
		{
			// Decide how big the next chunk should be 
			// Increase chunk size up to max if emptying faster than filling		

			if (IsOracleQuery) // set Oracle chunk size
			{

				if (NextRowsMaxTime > 0) // time limit specified?
				{
					if (ChunksRetrievedCount == 0) // first chunk
						ChunkKeyCount = ChunkKeyCountFirst;

					else if (ChunksRetrievedCount == 1) // second chunk
						ChunkKeyCount = ChunkKeyCountSecond;

					else // subsequent chunks
						ChunkKeyCount = ChunkKeyCountRest;
				}

				else ChunkKeyCount = ChunkKeyCountRest; // if no time limit then start with max

				if ((DbCommandMx.DefaultKeyListPredType == KeyListPredTypeEnum.Parameterized || // limit max keys if necessary
					DbCommandMx.DefaultKeyListPredType == KeyListPredTypeEnum.Literal) &&
					ChunkKeyCount > DbCommandMx.MaxOracleInListItemCount)
				{
					ChunkKeyCount = DbCommandMx.MaxOracleInListItemCount;
				}
			}

			else // set ODBC chunk size
			{
				if (ChunksRetrievedCount == 0) // first chunk
					ChunkKeyCount = ChunkKeyCountFirst;
				// ChunkKeyCount = 100;

				else if (ChunksRetrievedCount == 1) // second chunk
					ChunkKeyCount = ChunkKeyCountSecond;
				//ChunkKeyCount = 1000;

				else // subsequent chunks
					ChunkKeyCount = ChunkKeyCountRest;
				//ChunkKeyCount = NetezzaDao.RetrieveMaxChunkSize;
			}

			if (ChunkKeyCount < 1) ChunkKeyCount = 1; // be sure > 0

			if (LogBasics) DebugLog.Message("ChunkKeyCount: " + ChunkKeyCount);
		}
#endif


		/// <summary>
		/// Retrieve data for each table for the keys in the chunk
		/// </summary>
		/// <param name="chunkKeys"></param>
		/// <param name="chunkData"></param>

		void RetrieveTableDataForKeyset(
				List<string> chunkKeys,
				ChunkTableData chunkData)
		{
			List<QueryTableData> qtdList;

			DateTime tForChunk;
			tForChunk = DateTime.Now; // time marker

			RetrieveTableDataForKeysetException = null;

			if (!MultithreadRowRetrieval || Qtd.Length <= 1) // do as single thread if multithreading not enabled or only one table
			{
				qtdList = new List<QueryTableData>(Qtd);
				RetrieveTableDataForKeyset(qtdList, chunkKeys, chunkData);
				//return; // (allow fall through to check for exception)
			}

			else RetrieveTableDataForKeysetMultithreaded(chunkKeys, chunkData);

			if (RetrieveTableDataForKeysetException != null) // throw any exception that occurred
				throw new Exception(RetrieveTableDataForKeysetException.Message, RetrieveTableDataForKeysetException);

			return;
		}

		/// <summary>
		/// Execute retrieval for tables on multiple threads for improved performance
		/// </summary>
		/// <param name="chunkKeys"></param>
		/// <param name="chunkData"></param>

		void RetrieveTableDataForKeysetMultithreaded(
				List<string> chunkKeys,
				ChunkTableData chunkData)
		{
			List<QueryTableData> qtdList;
			QueryTableData qtd;
			ExecuteQueryParms eqp;
			MultiTablePivotBrokerTypeData mpd;
			GenericMetaBroker mb;
			QueryTable qt;
			MetaTable mt;
			string tableGroupKey;
			int ti;

			// Create lists of tables to be executed by each thread

			Dictionary<string, List<QueryTableData>> mbQtdDict = // list of tables for which to retrieve data for each thread
					new Dictionary<string, List<QueryTableData>>();

			for (ti = 0; ti < Qtd.Length; ti++) // get groups of tables by associated broker
			{
				qtd = Qtd[ti];
				mb = qtd.Broker;

				if (mb.PivotInCode && Lex.IsDefined(mb.MpGroupKey))
					tableGroupKey = mb.MpGroupKey;

				else tableGroupKey = "Broker_" + mb.Id;

				if (!mbQtdDict.ContainsKey(tableGroupKey)) // allocate dict entry for group if needed
				{
					qtdList = new List<QueryTableData>();
					mbQtdDict[tableGroupKey] = qtdList;
				}

				mbQtdDict[tableGroupKey].Add(qtd);
			}

			int threadCount = mbQtdDict.Keys.Count;
			RetrieveThreadLatch = new MultithreadLatch(threadCount); // latch to synchronize with threads

			foreach (string tableGroupKey0 in mbQtdDict.Keys) // start a thread for each broker
			{
				qtdList = mbQtdDict[tableGroupKey0];

				object[] parms = new object[3];
				parms[0] = qtdList;
				parms[1] = chunkKeys;
				parms[2] = chunkData;

				Thread t = new Thread(new ParameterizedThreadStart(RetrieveTableDataForKeysetThreadMethod));
				t.Name = "RetrieveTableDataForKeyset";
				t.IsBackground = true;
				t.SetApartmentState(ApartmentState.STA);
				t.Start(parms);
			}

			RetrieveThreadLatch.Wait(); // wait for all threads to finish
			return;
		}

		/// <summary>
		/// Thread method to retrieve data for a selected set of tables
		/// </summary>
		/// <param name="parms"></param>

		void RetrieveTableDataForKeysetThreadMethod(object parm)
		{
			object[] parms = parm as object[];
			List<QueryTableData> qtdList = parms[0] as List<QueryTableData>;
			List<string> chunkKeys = parms[1] as List<string>;
			ChunkTableData chunkData = parms[2] as ChunkTableData;

			RetrieveTableDataForKeyset(qtdList, chunkKeys, chunkData);
			RetrieveThreadLatch.Signal();
		}

		/// <summary>
		/// Retrieve data for selected set of tables
		/// </summary>
		/// <param name="qtdList"></param>
		/// <param name="chunkKeys"></param>
		/// <param name="chunkData"></param>

		void RetrieveTableDataForKeyset(
		List<QueryTableData> qtdList,
		List<string> chunkKeys,
		ChunkTableData chunkData)
		{
			QueryTable qt;
			MetaTable mt;
			GenericMetaBroker mb;
			DateTime tExecute, tForTableSet, tNextRow;
			object[] vo2;
			int qtdi, ti, tDelta;

			tForTableSet = DateTime.Now; // time marker

			//int drivingTables = 0, tablesFromOtherRoots = 0, selectKeyOnlyTables = 0;  // these are never used
			int executeQueryTables = 0;
			try
			{

				for (qtdi = 0; qtdi < qtdList.Count; qtdi++) // process each table in the group
				{
					QueryTableData qtd = qtdList[qtdi];
					mb = qtd.Broker;
					if (qtd.Broker == null) continue;

					ExecuteQueryParms eqp = qtd.Eqp;
					if (eqp == null) continue; // if no Eqp then can't execute query against this

					qt = eqp.QueryTable; // QueryTable
					mt = qt.MetaTable; // metatable
					ti = qt.TableIndex; // table index within query

					if (DrivingTableIdx >= 0 && Query.Tables[DrivingTableIdx].Alias == qt.Alias) // if driving table, we already have data, just copy over
					{
						FillChunkDataForDrivingTable(chunkData);
						//drivingTables++;
						continue; // all done here
					}

					if (RootTableIdx >= RootTables.Count) throw new Exception("RootTableIndex = " + RootTableIdx + " >= " + RootTables.Count);
					if (!mt.IsRootTable && mt.Root != null && Lex.Ne(mt.Root.Name, RootTables[RootTableIdx].Name))
					{ // if table from other root then we can't retrieve any data from it
						//tablesFromOtherRoots++;
						continue;
					}

					if (SkipTablesThatSelectKeyOnly && qtdi > 0 && executeQueryTables > 0 && qtd.SelectCount <= 1)
					{ // if only the key is selected and not the first table then don't retrieve data from the table
						//selectKeyOnlyTables++;
						continue;
					}

					eqp.SearchKeySubset = chunkKeys; // supply key subset
					mb.GetExecuteQueryParms.CheckForCancel =
					 CheckForCancel; // set cancel checking on each call since sort may change it

					tExecute = DateTime.Now;
					executeQueryTables++;

					try { mb.ExecuteQuery(eqp); }
					catch (Exception ex)
					{
						ProcessBrokerException(ex, ti);
						continue; // just continue with nothing added to buffer for this table
					}

					if (mb.GetExecuteQueryParms.Cancelled)
					{
						Cancelled = true;
						return;
					}

					ChunkExecuteTime += // accumulate overall chunk execute time
							(int)TimeOfDay.Delta(ref tForTableSet);

					if (LogBasics)
					{
						tDelta = (int)TimeOfDay.Delta(ref tExecute);
						DebugLog.Message("Chunk Execute - Table: " +
								eqp.QueryTable.MetaTable.Name + ", time: " + tDelta);
						DebugLog.Message("ChunkExecuteTime: " + ChunkExecuteTime);
					}

					qtd.RowsFetched = 0;

					tNextRow = DateTime.Now;

					while (true) // get all rows for this table for the chunk of keys
					{
						vo2 = mb.NextRow();
						if (mb.GetExecuteQueryParms.Cancelled)
						{
							Cancelled = true;
							return;
						}

						if (vo2 == null) break; // all done for this table
						NormalizeNullValues(vo2);

						int fetchCount = qtd.RowsFetched;
						int fetchCountLogLimit = 2; // limit of number of fetches to log

						if (fetchCount < fetchCountLogLimit)
						{
							tDelta = (int)TimeOfDay.Delta(ref tNextRow);
							if (ChunksRetrievedCount == 0 && fetchCount == 0) // accumulate first row time for each table
								Stats.FirstRowFetchTime += tDelta;

							if (LogBasics && fetchCount < fetchCountLogLimit)
							{
								DebugLog.Message("Chunk Fetch - Row " + (fetchCount + 1) +
										" time: " + tDelta);
							}
						}

						AccumulateDataForMerge(chunkData, ti, vo2);
					}

					// Have all rows for current table			

					tDelta = (int)TimeOfDay.Delta(ref tForTableSet); // get fetch time for this table
					ChunkFetchTime += tDelta; // accumulate overall chunk fetch time

					if (LogBasics)
					{
						int count = qtd.RowsFetched;
						string msg = "Chunk Fetch - Total rows: " + qtd.RowsFetched + ", time: " + tDelta;
						if (count > 0) msg += ", time/row: " + ((tDelta * 1000) / count) / 1000.0;
						msg += "\r\n"; // add blank line in debug text between tables
						DebugLog.Message(msg);
					}
				}
			}

			catch (Exception ex)
			{
				DebugLog.Message(DebugLog.FormatExceptionMessage(ex));
				RetrieveTableDataForKeysetException = ex; // save exception
			}
		}

		/// <summary>
		/// Get the next set of rows & keys from the driving table
		/// </summary>
		/// <returns></returns>

		List<string> GetDrivingTableDataChunk()
		{
			object[] vo;

			int ti = DrivingTableIdx;

			List<string> chunkKeys = new List<string>(100); // the set of associated keys
			DrivingTableVos = new List<object[]>(200);

			DateTime t0 = DateTime.Now;

			string currentKey = "";

			while (true) // loop until have all data for chink or time up and have data for at least one key or cancelled
			{
				if (DrivingTableNextRow != null) // already have next row?
				{
					vo = DrivingTableNextRow;
					DrivingTableNextRow = null;
				}

				else // fetch next row
				{
					vo = Qtd[ti].Broker.NextRow();
					if (vo == null) return chunkKeys;
					NormalizeNullValues(vo);

					if (Qtd[ti].Broker.GetExecuteQueryParms.Cancelled)
					{
						Cancelled = true;
						return null;
					}
				}

				string key = GetKeyString(vo[0]);

				if (NextRowsMaxTimeExceeded() && chunkKeys.Count > 0 && key != currentKey)
				{ // if time up & we have something
					DrivingTableNextRow = vo;
					return chunkKeys;
				}

				DrivingTableVos.Add(vo);

				if (key != currentKey) // build list of keys
				{
					chunkKeys.Add(key);
					currentKey = key;
				}
			}
		}

		/// <summary>
		/// Fill in the already-retrieved data for the current chunk of keys
		/// </summary>

		void FillChunkDataForDrivingTable(
				ChunkTableData chunkData)
		{
			foreach (object[] vo in DrivingTableVos)
				AccumulateDataForMerge(chunkData, DrivingTableIdx, vo);
		}

		/// <summary>
		/// Add a row of data for a broker to chunkData prior to merging
		/// </summary>
		/// <param name="chunkData"></param>
		/// <param name="ti"></param>
		/// <param name="vo2"></param>

		void AccumulateDataForMerge(
				ChunkTableData chunkData,
				int ti,
				object[] vo2)
		{
			List<object[]>[] tablesForKey; // array with one entry for each table for a key
			List<object[]> rowsForTableForKey; // data rows for a key for a single table

			string key = GetKeyString(vo2[0]);
			if (Lex.IsNullOrEmpty(key)) throw new ArgumentException("Undefined key");

			if (!chunkData.KeyToTableDataDict.ContainsKey(key)) throw new Exception("ChunkData not initialized for key: " + key);

			tablesForKey = chunkData.TableDataForKey(key);
			AssertMx.IsTrue(ti >= 0 && ti < tablesForKey.Length, "Not in bounds");
			rowsForTableForKey = tablesForKey[ti]; // data for key for table

			if (rowsForTableForKey == null) // if no rows for table for key allocate list to hold them
			{
				rowsForTableForKey = new List<object[]>();
				tablesForKey[ti] = rowsForTableForKey;
			}

			Qtd[ti].RowsFetched++;
			Qtd[ti].Broker.NextRowCount++; // also keep count in broker

			rowsForTableForKey.Add(vo2);
			return;
		}

		/// <summary>
		/// Apply any requested aggregation 
		/// </summary>
		/// <param name="chunkData"></param>

		void AggregateData(ChunkTableData chunkData)
		{
			QueryTableData qtd = null;
			int ti, ri;

			if (Query.GetTablesAggregatedCount() <= 0) return;

			// Do key-oriented 

			if (Query.IsKeyAggreation)
			{
				foreach (string key0 in chunkData.KeyToTableDataDict.Keys)
				{
					List<object[]>[] tableDataForKey = chunkData.TableDataForKey(key0);
					object[] parentVo = null; // parent to be filled in

					for (ti = 0; ti < Qtd.Length; ti++)
					{
						qtd = Qtd[ti];
						QueryTable qt = qtd.Eqp.QueryTable;
						if (!qt.AggregationEnabled) continue;

						List<object[]> rowsForTableForKey = tableDataForKey[ti]; // get rows for table
						if (rowsForTableForKey == null) continue;

						if (qtd.AggregationData == null) qtd.AggregationData = new Aggregator();

						List<object[]> aggregatedRows = qtd.AggregationData.AggregateQueryTable(Id, qt, rowsForTableForKey);
						tableDataForKey[ti] = aggregatedRows;
					}
				}
			}

			// Do non-key-oriented aggregation over full data set

			else if (Query.IsNonKeyAggreation) // key col is not part of aggregation
			{
				for (ti = 0; ti < Qtd.Length; ti++) // (should be just one table)
				{
					qtd = Qtd[ti];
					QueryTable qt = qtd.Eqp.QueryTable;
					if (!qt.AggregationEnabled) continue;

					List<object[]> allRows = new List<object[]>(); // build a list of all rows for all keys to be input into the aggregation

					foreach (List<object[]>[] rowSetsForKeysForTables in chunkData.KeyToTableDataDict.Values)
					{ // append all rows for all keys together for proper aggregation
						List<object[]> rowsSetForTable = rowSetsForKeysForTables[ti];
						if (rowsSetForTable == null) continue;
						allRows.AddRange(rowsSetForTable);
					}

					if (qtd.AggregationData == null) qtd.AggregationData = new Aggregator();

					List<object[]> aggregatedRows = qtd.AggregationData.AggregateQueryTable(Id, qt, allRows);

					List<object[]>[] rowArray = new List<object[]>[1];
					rowArray[0] = aggregatedRows;

					chunkData.KeyToTableDataDict = new Dictionary<string, List<object[]>[]>();
					chunkData.KeyToTableDataDict["AggregatedKey"] = rowArray;
				}
			}

			else throw new Exception("Not a key or non-key aggreagation");

			return;
		}

		/// <summary>
		/// Apply any data transforms to the data for the table
		/// </summary>
		/// <param name="qtd"></param>
		/// <param name="keyOffset"></param>
		/// <param name="vo"></param>

		void ApplyDataTransforms(
			QueryTableData qtd,
			int keyOffset,
			object[] vo)
		{
			MoleculeMx cs;
			string prefixedStr;
			string transform;
			const string chimePrefix = "chime=";

			if (vo[0] == null) return;

			for (int voi = 0; voi < qtd.SelectCount; voi++)
			{
				object o = vo[voi]; // get the value
				if (o == null) continue;

				QueryColumn qc = qtd.SelectedColumns[voi];
				MetaColumn mc = qc.MetaColumn;
				//if (Lex.Eq(mc.Name, "molSrchType")) mc = mc; // debug

				transform = mc.DataTransform;
				if (String.IsNullOrEmpty(transform)) continue;

				string oString = o is string ? o as string : null; // string ref

				if (Lex.Eq(transform, "GetSmiles"))
				{ // transform a chime string or compound id into a smiles string
					string smiles;
					cs = null;
					o = null;
					string keyValue = GetKeyString(vo[0]);

					if (!Security.UserInfo.Privileges.CanRetrieveStructures) { } // return null if can't retrieve structures

					else if (Lex.StartsWith(oString, chimePrefix)) // chime string to convert?
					{
						if (Lex.Eq(oString.Trim(), chimePrefix)) continue; // no chime value
						string chimeString = oString.Substring(chimePrefix.Length);
						cs = new MoleculeMx(MoleculeFormat.Chime, chimeString);
					}

					else if (Lex.Eq(oString, "Smiles")) // if "Smiles" retrieve structure & calc
					{
						if (Lex.IsNullOrEmpty(keyValue)) continue;
						cs = MoleculeUtil.SelectMoleculeForCid(keyValue, qtd.Table.MetaTable);
					}

					else if (!Lex.IsNullOrEmpty(oString)) // assume already is smiles
					{
						o = oString;
					}

					if (cs != null) // extract smiles from ChemicalStructure
					{
						smiles = cs.GetSmilesString();
						o = smiles;
					}
				}

				else if (Lex.Eq(transform, "GetMolFormula"))
				{ // transform a compound id into a molecular formula
					string keyValue = GetKeyString(vo[0]);
					cs = MoleculeUtil.SelectMoleculeForCid(keyValue, qtd.Table.MetaTable);
					if (cs != null)
					{
						string mf = cs.MolFormula;
						o = mf;
					}

					else o = null;
				}

				else if (Lex.Eq(transform, "GetMolWeight"))
				{ // transform a compound id into a molecular weight
					string keyValue = GetKeyString(vo[0]);
					cs = MoleculeUtil.SelectMoleculeForCid(keyValue, qtd.Table.MetaTable);
					if (cs != null)
					{
						double mw = cs.MolWeight;
						o = mw;
					}

					else o = null;
				}

				else if (Lex.Eq(transform, "GetMolSrchType")) // get molecule mol search type
				{
					if (vo[0] != null && ResultsKeysDict != null)
					{
						string keyValue = GetKeyString(vo[0]);
						if (ResultsKeysDict.ContainsKey(keyValue))
						{
							o = ResultsKeysDict[keyValue];
							if (o is double)
								o = "Similar";

							else if (o is StructSearchMatch)
							{
								StructSearchMatch ssm = o as StructSearchMatch;
								o = ssm.SearchTypeName;
							}
						}
					}
				}

				else if (Lex.Eq(transform, "GetMolSim")) // get similarity search score
				{ // get mol similarity or more-detailed search results value buffered in ResultKeyHash
					if (vo[0] != null && ResultsKeysDict != null)
					{
						string keyValue = GetKeyString(vo[0]);
						if (ResultsKeysDict.ContainsKey(keyValue))
						{
							o = ResultsKeysDict[keyValue];
							if (o is double) { o = o; } // ok as is

							else if (o is StructSearchMatch)
							{
								StructSearchMatch ssm = o as StructSearchMatch;
								o = (double)ssm.MatchScore;
								QueryTable qt = qtd.Table; // enhance structure with match information if available
								QueryColumn qcStr = qt.FirstStructureQueryColumn;
								if (qcStr != null && qcStr.Selected && Lex.IsDefined(ssm.MolString))
								{
									int voiStr = qcStr.VoPosition - 1; // vo position for structure (adjust for CID offset)
									cs = vo[voiStr] as MoleculeMx; // any existing structure
									if (cs != null)
									{
										if (ssm.SearchType == StructureSearchType.MatchedPairs)
										{
											cs.SetPrimaryTypeAndValue(ssm.MolStringFormat, ssm.MolString);
										}

										else if (ssm.SearchType == StructureSearchType.SmallWorld && Lex.IsDefined(ssm.GraphicsString))
										{
											cs.SvgString = ssm.GraphicsString;
										}
									}
								}

							}
						}
					}
				}

				else if (Lex.Eq(transform, "IntegrateMmpStructure"))
				{ // integrate MMP difference fragment and context fragments into a single hilighted structure
					string smiles = "";
					if (o is MoleculeMx)
						smiles = (o as MoleculeMx).GetSmilesString();
					else smiles = o.ToString();
					string molfile = CdkUtil.IntegrateAndHilightMmpStructure(smiles);
					o = new MoleculeMx(MoleculeFormat.Molfile, molfile);
				}

				else if (MoleculeMx.TrySetStructureFormatPrefix(ref oString, transform))
				{ // if string format structure be sure proper prefix is added
					o = oString;
				}

				else
				{ // apply broker-specific data transformation function
					IMetaBroker gbm = GetGlobalBroker(mc.MetaTable);
					if (gbm != null) o = gbm.TransformData(qc, o, vo);
				}

				vo[voi] = o;
			}
		}

		/// <summary>
		/// Copy vo data for this table into composite buffer
		/// </summary>
		/// <param name="qtd"></param>
		/// <param name="vo2"></param>
		/// <param name="vo"></param>

		void CopyTableVoToCompositeVo(
				QueryTableData qtd,
				int keyOffset,
				object[] vo2,
				object[] vo)
		{
			int offset = qtd.TableVoPosition + keyOffset;
			if (vo2.Length > vo.Length - offset) throw new Exception("Array copy length too large: (" + vo2.Length + " > " + (vo.Length - offset) + ")");
			vo2.CopyTo(vo, offset);
		}

		/// <summary>
		/// See if there is data to use for this table vo
		/// </summary>
		/// <param name="vo"></param>
		/// <returns></returns>

		bool PassTableNullRowFilter(
				object[] vo)
		{
			int ti, fi;

			if (!Query.FilterNullRows) return true;

			for (int voi = 1; voi < vo.Length; voi++) // start after key
			{
				if (!NullValue.IsNull(vo[voi]))
					return true; // non-key non-null value
			}

			return false;
		}

		/// <summary>
		/// Normalize null values in a vo to a real null object pointer
		/// </summary>
		/// <param name="vo"></param>

		void NormalizeNullValues(
				object[] vo)
		{
			if (vo == null) return;
			for (int voi = 1; voi < vo.Length; voi++) // start after key
			{
				if (vo[voi] == null) continue;

				if (NullValue.IsNull(vo[voi]))
					vo[voi] = null;
			}
		}

		/// <summary>
		/// Do preparation for data retrieval (ExecuteQuery on each table) using supplied root
		/// </summary>
		/// <param name="rootTable"></param>

		bool PrepareForRetrieval(
				MetaTable rootTable,
				out List<string> resultKeysForRoot)
		{
			QueryTable qt, qt2;
			MetaTable mt, mt2;
			GenericMetaBroker mb;
			ExecuteQueryParms eqp;
			int ti, ti2, ci, t0;

			if (ResultsKeys == null || ResultsKeys.Count == 0) // no keys if preview or single table combined search and retrieval
				resultKeysForRoot = null;

			else // filter keys to get just those for this root
			{
				resultKeysForRoot =
						CompoundId.FilterKeysByRoot(ResultsKeys, rootTable);
				if (resultKeysForRoot == null || resultKeysForRoot.Count == 0) return false;
			}

			int tablesMapped = 0;
			for (ti = 0; ti < Query.Tables.Count; ti++)
			{
				qt = Query.Tables[ti];
				qt2 = qt.MapToCurrentDb(rootTable); // make copy we can modify pointing to real metatable
				if (qt2 == null) continue; // not associated with this root
				mt2 = qt2.MetaTable;
				mb = MetaBrokerUtil.Create(mt2.MetaBrokerType);
				if (Qtd == null || ti >= Qtd.Length) Qtd = Qtd; // debug
				Qtd[ti].Broker = mb;

				CritToksForRoot = // plug in proper key col names & constant values
				 MqlUtil.TransformKeyCriteriaForRoot(
						 CritToks,
						 rootTable,
						 KeyCriteriaPositions,
						 KeyCriteriaConstantPositions);

				eqp = new ExecuteQueryParms(this);
				Qtd[ti].Eqp = eqp;

				if (Qtd[ti].CriteriaCount == 0)
					eqp.CallerSuppliedCriteria = "(1=1)"; // dummy criteria that can be appended to

				else // reduce criteria to what's relevant to this table
					eqp.CallerSuppliedCriteria = "(" + MqlUtil.ExtractSingleTableCriteria(CritToksForRoot, Query, qt) + ")";

				eqp.QueryTable = qt2; // modified query table
				eqp.SearchKeySubset = resultKeysForRoot;
				eqp.CheckForCancel = CheckForCancel;

				SortOrder sd = SortOrder.None;
				if (Query.KeySortOrder > 0) sd = SortOrder.Ascending;
				else if (Query.KeySortOrder < 0) sd = SortOrder.Descending;
				eqp.KeySortDirection = sd; // set sorting for this table

				t0 = TimeOfDay.Milliseconds();

				try { mb.PrepareQuery(eqp); }
				catch (Exception ex)
				{
					ProcessBrokerException(ex, ti);
					continue;
				}

				if (eqp.Cancelled)
				{
					Cancelled = true;
					if (resultKeysForRoot == null)
						resultKeysForRoot = new List<string>();
					return true;
				}

				if (LogDetail) // bit higher level of logging
				{
					t0 = TimeOfDay.Milliseconds() - t0;
					DebugLog.Message("PrepareForRetrieval" +
							", table: " + eqp.QueryTable.MetaTable.Name +
							", keySubsetCount: " + (eqp.SearchKeySubset != null ? eqp.SearchKeySubset.Count.ToString() : "0") +
							", time: " + t0 +
							", sql: " + Lex.RemoveLineBreaksAndTabs(eqp.CallerSuppliedSql));
					//", sql: " + OracleMx.FormatSql(eqp.CallerSuppliedSql));
				}

				Qtd[ti].Closed = false;
				tablesMapped++;
			}

			CurrentRow = null;
			if (tablesMapped > 0)
			{
				if (resultKeysForRoot == null)
					resultKeysForRoot = new List<string>();
				return true;
			}
			else return false;
		}

		/// <summary>
		/// Process single table metabroker exception adding to list instead of throwing if list exists
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="ti"></param>

		bool ProcessBrokerException(
				Exception ex,
				int ti)
		{
			int ti2;

			if (Query.Tables == null || ti < 0 || ti >= Query.Tables.Count) throw ex;
			QueryTable qt = Query.Tables[ti];
			List<string> unaccessableMtList;

			if (Query.InaccessableData == null) throw new Exception(ex.Message, ex);

			string key = ex.Message;
			if (key.Length > 32)
			{
				int i1 = Lex.IndexOf(key, "Ora-");
				if (i1 >= 0)
				{
					key = key.Substring(i1);
					i1 = Lex.IndexOf(key, "\n");
					if (i1 > 0) key = key.Substring(0, i1);
				}
				else key = "General metabroker exception";
			}

			if (Query.InaccessableData.ContainsKey(key))
				unaccessableMtList = Query.InaccessableData[key];

			else
			{
				unaccessableMtList = new List<string>();
				Query.InaccessableData[key] = unaccessableMtList;
			}

			for (ti2 = 0; ti2 < unaccessableMtList.Count; ti2++)
			{
				if (Lex.Eq(unaccessableMtList[ti2], qt.MetaTable.Name)) break;
			}

			if (ti2 <= unaccessableMtList.Count) Query.InaccessableData[key].Add(qt.MetaTable.Name);

			Qtd[ti].Closed = true; // no data for the table
			return true;
		}

		/// <summary>
		/// Get a count of the keys
		/// </summary>
		/// <returns></returns>

		public int GetKeyCount()
		{
			if (ResultsKeys == null) return 0;
			else return ResultsKeys.Count;
		}

		/// <summary>
		/// Get the keys
		/// </summary>
		/// <returns></returns>

		public List<string> GetKeys()
		{
			if (ResultsKeys == null) return new List<string>();
			else return ResultsKeys;
		}

		/// <summary>
		/// Build sql for full query
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		/// 
		public string BuildSqlForSingleStepExecution(
				Query query)
		{
			ExecuteQuery(query, true, true);
			return Sql;
		}

		/// <summary>
		/// Build the sql for a single query table
		/// </summary>
		/// <param name="eqp"></param>
		/// <returns></returns>

		public string BuildSqlForSingleTable(
				ExecuteQueryParms eqp)
		{

			IMetaBroker mb = MetaBrokerUtil.Create(eqp.QueryTable.MetaTable.MetaBrokerType);
			string sql = mb.BuildSql(eqp);
			return sql;
		}

		/// <summary>
		/// Sort a result set on a single column and reopen sorted data
		/// </summary>

		public void SortResultSet(
				QueryColumn qc,
				SortOrder direction)
		{
			List<SortColumn> sortColumns = new List<SortColumn>();
			SortColumn sc = new SortColumn();
			sc.QueryColumn = qc;
			sc.Direction = direction;
			sortColumns.Add(sc);

			SortResultSet(sortColumns);
		}

		/// <summary>
		/// Sort a result set on multiple columns and reopen sorted data
		/// </summary>
		/// <param name="sortColumns">ArrayList of ordered QueryColumns</param>

		public void SortResultSet(
				List<SortColumn> sortColumns)
		{
			object[] vo, vo2;
			int qti, ri, ti, sci, i1, i2, i3;

			SortItem currentVal = null; // current top composite value (high or low) for current key
			object value;
			object[] sourceVo, destVo;

			string key;
			SortItem sItem = null, sItem2;
			int keyOffset = 1; // pk is stored in first element of vo and must be offset

			ResultsSorter sorter = new ResultsSorter();

			sorter.ValidateSortColumns(sortColumns);

			ICheckForCancel saveCancel = CheckForCancel; // handle cancel checking here rather than at low-level Oracle
			CheckForCancel = null;

			DataRowCachingMode curMode = ResultsRows.CachingMode;
			ResultsRows.SetCachingMode(DataRowCachingMode.LockInMemory);

			while (NextRow() != null)
			{
				if (saveCancel != null && saveCancel.IsCancelRequested)
				{ // just return if cancelled, leaving query to continue further
					CheckForCancel = saveCancel;
					return;
				}
			}

			CheckForCancel = saveCancel; // restore normal cancel

			if (ResultsRows == null || ResultsRows.TotalRowCount <= 1) return;

			ResultsRows.RowBuffer = sorter.Sort(ResultsRows.RowBuffer, sortColumns, Query, 0, out ResultsKeys);

			ResultsRows.SetCachingMode(curMode);
			return;
		}

		/// <summary>
		/// Build query to retrieve detailed data for a summarization
		/// </summary>
		/// <param name="metaTableName"></param>
		/// <param name="metaColumnName"></param>
		/// <param name="resultId"></param>
		/// <returns></returns>

		public static Query GetSummarizationDetailQuery(
				string metaTableName,
				string metaColumnName,
				int level,
				string resultId)
		{
			MetaTable mt = MetaTableCollection.Get(metaTableName);
			if (mt == null) return null;

			if (Lex.Eq(metaColumnName, "<AggregationGroupRows>"))
				return GetAggregationGroupRowsQuery(queryEngineId: level, mt: mt, groupIdString: resultId);

			MetaColumn mc = mt.GetMetaColumnByName(metaColumnName);
			if (mc == null) return null;

			return GetDrilldownDetailQuery(mt, mc, level, resultId);
		}

		/// <summary>
		/// Build query to retrieve detailed data for a summarization
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="mc"></param>
		/// <param name="resultId"></param>
		/// <returns></returns>

		public static Query GetDrilldownDetailQuery(
				MetaTable mt,
				MetaColumn mc,
				int level,
				string resultId)
		{
			IMetaBroker imb;
			if (mc.MetaBrokerType != MetaBrokerType.Unknown) // column broker?
				imb = MetaBrokerUtil.GlobalBrokers[(int)mc.MetaBrokerType];
			else imb = MetaBrokerUtil.GlobalBrokers[(int)mc.MetaTable.MetaBrokerType];
			return imb.GetDrilldownDetailQuery(mt, mc, level, resultId);
		}

		/// <summary>
		/// Get query that returns rows underlying an aggregation group
		/// </summary>
		/// <param name="queryEngineId"></param>
		/// <param name="mt"></param>
		/// <param name="groupId"></param>
		/// <returns></returns>

		public static Query GetAggregationGroupRowsQuery(
				int queryEngineId,
				MetaTable mt,
				string groupIdString)
		{
			QueryEngine qe;
			Aggregator ag;
			QueryTableData qtd = null;
			int ti, groupId;

			if (!int.TryParse(groupIdString, out groupId)) return null;

			if (!GetAggregationGroupIds(queryEngineId, mt.Name, groupId, out qe, out ag, out qtd)) return null;

			Query q = new Query();
			QueryTable qt = qtd.Eqp.QueryTable.Clone();
			qt.AggregationEnabled = false; // turn off aggregation for drilldown
			q.AddQueryTable(qt);

			q.SingleStepExecution = true;

			q.KeyCriteria = "AggregationDrillDown=" + queryEngineId + "." + mt.Name + "." + groupIdString; // special criteria to indicate where to get drilldown data

			return q;
		}

		/// <summary>
		/// Get internal id info for an aggregation group
		/// </summary>
		/// <param name="qeId"></param>
		/// <param name="mtName"></param>
		/// <param name="groupId"></param>
		/// <returns></returns>

		static bool GetAggregationGroupIds(
				int qeId,
				string mtName,
				int groupId,
				out QueryEngine qe,
				out Aggregator ag,
				out QueryTableData qtd)
		{
			int ti;

			ag = null;
			qtd = null;

			qe = GetInstanceById(qeId);
			if (qe == null) return false;

			for (ti = 0; ti < qe.Qtd.Length; ti++)
			{
				qtd = qe.Qtd[ti];
				if (Lex.Eq(qtd.Eqp.QueryTable.MetaTable.Name, mtName)) break;
			}
			if (ti >= qe.Qtd.Length) return false;

			ag = qtd.AggregationData;
			if (!ag.GroupIdToGroupRows.ContainsKey(groupId)) return false;

			return true;
		}

		/// <summary>
		/// Return true is associated query is an aggregation drilldown query
		/// </summary>
		/// <returns></returns>

		bool IsAggregationDrillDown()
		{
			if (Lex.StartsWith(Query.KeyCriteria, "AggregationDrillDown"))
				return true;
			else return false;
		}

		/// <summary>
		/// Return all of the drilldown rows for an aggregation group in a single call
		/// </summary>
		/// <returns></returns>

		List<object[]> GetAggregationDrillDownRows()
		{
			int qeId = 0, groupId = 0;
			string qeIdString, mtName, groupIdString;
			QueryEngine qe;
			Aggregator ag;
			QueryTableData qtd;

			if (State == QueryEngineState.Closed) return null;

			string args = Lex.SubstringAfter(Query.KeyCriteria, "=");
			if (args == null) throw new Exception("Expected \"AggregationDrillDown=\"  KeyCriteria: " + Query.KeyCriteria);
			Lex.Split(args, ".", out qeIdString, out mtName, out groupIdString);

			int.TryParse(qeIdString, out qeId);
			int.TryParse(groupIdString, out groupId);

			if (!GetAggregationGroupIds(qeId, mtName, groupId, out qe, out ag, out qtd)) return null;

			List<object[]> voListForGroup = ag.GroupIdToGroupRows[groupId];

			List<object[]> voList = new List<object[]>();

			foreach (object[] voa0 in voListForGroup)
			{
				object[] voa = new object[voa0.Length + 1];
				voList.Add(voa);
				voa[0] = voa0[0]; // copy base key
				Array.Copy(voa0, 0, voa, 1, voa0.Length);
			}

			State = QueryEngineState.Closed;
			return voList;
		}

		/// <summary>
		/// Get a pivoted vo given an unpivoted result identifier (obsolete) 
		/// </summary>
		/// <param name="mti"></param>
		/// <param name="voi"></param>
		/// <param name="voOffset"></param>
		/// <param name="mt2"></param>
		/// <param name="vo2"></param>
		/// <returns></returns>

		public bool GetPivotedResult(
				MetaTable mti,
				object[] voi,
				int voOffset,
				ref MetaTable mt2,
				ref object[] vo2)
		{
			IMetaBroker mb = MetaBrokerUtil.Create(mti.MetaBrokerType);
			return mb.GetPivotedResult(mti, voi, voOffset, ref mt2, ref vo2);
		}

		/// <summary>
		/// Format sql for easier reading
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>
		public static string FormatSql(string sql)
		{
			string fSql = sql;
			fSql = fSql.Replace("select", "\r\nselect");
			fSql = fSql.Replace("Select", "\r\nselect");
			fSql = fSql.Replace("SELECT", "\r\nselect");
			fSql = fSql.Replace("from", "\r\nfrom");
			fSql = fSql.Replace("From", "\r\nfrom");
			fSql = fSql.Replace("FROM", "\r\nfrom");
			fSql = fSql.Replace("where", "\r\nwhere");
			fSql = fSql.Replace("Where", "\r\nwhere");
			fSql = fSql.Replace("WHERE", "\r\nwhere");
			fSql = fSql.Replace("order by", "\r\norder by");
			fSql = fSql.Replace("Order By", "\r\norder by");
			fSql = fSql.Replace("ORDER BY", "\r\norder by");
			return fSql;
		}


		/// <summary>
		/// Parse text representation of a qualified number into its constituent 
		/// parts (separated by \v delimiters) which are:
		///  0 - Qualifier 
		///  1 - Basic number
		///  2 - String value
		///  3 - N value used in summarization
		///  4 - N value tested count
		///  5 - Standard deviation
		///  6 - Standard error
		///  7 - DbLink (Result Identifier)
		///  8 - Hyperlink
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>

		public static QualifiedNumber ParseQualifiedNumber(
				string txt)
		{
			if (txt == null) return null;

			QualifiedNumber qn = new QualifiedNumber();
			string[] args = txt.Split('\v'); // elements are separated by vertical tab chars

			if (args.Length > 0) qn.Qualifier = args[0];
			if (qn.Qualifier == "=") qn.Qualifier = "";
			if (args.Length > 1 && args[1] != "") qn.NumberValue = Double.Parse(args[1]);
			if (args.Length > 2) qn.TextValue = args[2];

			if (args.Length > 3 && args[3] != "") qn.NValue = Int32.Parse(args[3]);
			if (args.Length > 4 && args[4] != "") qn.NValueTested = Int32.Parse(args[4]);
			if (args.Length > 5 && args[5] != "") qn.StandardDeviation = Double.Parse(args[5]);
			if (args.Length > 6 && args[6] != "") qn.StandardError = Double.Parse(args[6]);
			if (args.Length > 7 && args[7] != "" && !qn.IsNull) // qn.NumberValue != QualifiedNumber.NullNumber) 
				qn.DbLink = args[7]; // store any link unless undefined numeric value
			if (args.Length > 8 && args[8] != "") qn.Hyperlink = args[8];

			return qn;
		}

		/// <summary>
		/// Split a simple scalar value from it's result identifier information 
		/// </summary>
		/// <param name="txt"></param>
		/// <param name="qt"></param>
		/// <param name="mc"></param>
		/// <returns></returns>

		public static QualifiedNumber ParseScalarValue(
				string txt,
				QueryTable qt,
				MetaColumn mc)
		{
			if (txt == null) return null;

			string[] args = txt.Split('\v'); // value (string, number, date) separated from result id by vertical tab
			if (args.Length == 0 || args[0].Length == 0) return null;
			QualifiedNumber qn = new QualifiedNumber();

			if (mc.DataType == MetaColumnType.Integer ||
					mc.DataType == MetaColumnType.Number)
			{
				qn.NumberValue = Double.Parse(args[0]);
			}

			else if (mc.DataType == MetaColumnType.String)
			{
				qn.TextValue = args[0];
			}

			else if (mc.DataType == MetaColumnType.Date)
			{
				qn.TextValue = args[0];
			}

			else throw new QueryException("Invalid data type for " + mc.MetaTable.Name + "." + mc.Name);

			if (args.Length >= 2)
				qn.DbLink = args[1];

			if (args.Length >= 3)
				qn.Hyperlink = args[2];

			return qn;
		}

		/// <summary>
		/// Get a string value from the reader by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public string GetString(
				string name)
		{
			int pos = GetOrdinal(name); // throws exception if bad name
			return GetString(pos);
		}

		/// <summary>
		/// Get string value by position
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>

		public string GetString(
				int pos)
		{
			if (CurrentRow == null) throw new Exception("No current row");
			if (pos < 0 || pos >= CurrentRow.Length) throw new Exception("Index out of range");
			if (CurrentRow[pos] == null) return "";
			else return Convert.ToString(CurrentRow[pos]);
		}

		/// <summary>
		/// Get an int value from the reader by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public int GetInt(
				string name)
		{
			int pos = GetOrdinal(name); // throws exception if bad name
			return GetInt(pos);
		}

		/// <summary>
		/// Get an int value from the reader by position
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>

		public int GetInt(
				int pos)
		{
			if (CurrentRow == null) throw new Exception("No current row");
			if (pos < 0 || pos >= CurrentRow.Length) throw new Exception("Index out of range");
			if (CurrentRow[pos] == null) return NullValue.NullNumber;
			else return Convert.ToInt32(CurrentRow[pos]);
		}

		/// <summary>
		/// Get a double value from the reader by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public double GetDouble(
				string name)
		{
			int pos = GetOrdinal(name); // throws exception if bad name
			return GetDouble(pos);
		}

		/// <summary>
		/// Get a double value from the reader by position
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		public double GetDouble(
				int pos)
		{
			if (CurrentRow == null) throw new Exception("No current row");
			if (pos < 0 || pos >= CurrentRow.Length) throw new Exception("Index out of range");
			if (CurrentRow[pos] == null) return NullValue.NullNumber;
			else return Convert.ToDouble(CurrentRow[pos]);
		}

		/// <summary>
		/// Get a date value from the reader by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public DateTime GetDate(
				string name)
		{
			int pos = GetOrdinal(name); // throws exception if bad name
			return GetDate(pos);
		}

		/// <summary>
		/// Get a date value from the reader by position
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		public DateTime GetDate(
				int pos)
		{
			if (CurrentRow == null) throw new Exception("No current row");
			if (pos < 0 || pos >= CurrentRow.Length) throw new Exception("Index out of range");
			if (CurrentRow[pos] == null) return DateTime.MinValue;
			else return (DateTime)CurrentRow[pos];
		}

		/// <summary>
		/// Get ordinal position of specified column name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public int GetOrdinal(
				string name)
		{
			List<MetaColumn> rmc = ResultsMetaColumns;
			if (rmc == null) throw new Exception("ResultMetaColumns not defined");
			for (int pos = 0; pos < rmc.Count; pos++)
			{
				MetaColumn mc = rmc[pos];
				if (mc == null) continue;
				if (Lex.Eq(mc.Name, name)) return pos;
			}

			throw new Exception("Column name not found: " + name);
		}

		/// <summary>
		/// Set molecule compound Id fields from other CID values in rowList
		/// </summary>
		/// <param name="rowList"></param>
		/// <returns></returns>

		public List<MoleculeMx> SetMoleculeCids(List<object[]> rowList)
		{
			List<MoleculeMx> molList = new List<MoleculeMx>(); // list of mols with known CIDs

			ColumnsOfSpecifiedType p = GetColumnsOfSpecifiedType(MetaColumnType.Structure, MetaColumnType.CompoundId);
			if (p.QueryColumns != null && p.QueryColumns.Count == 0) return molList;

			p.RowList = new List<object[]>();

			foreach (object[] row in rowList) // build a list of helm mols and set Id to cid
			{
				if (row == null) continue;

				for (int qci = 0; qci < p.QueryColumns.Count; qci++)
				{
					QueryColumn qc = p.QueryColumns[qci];

					if (qc.DataType != MetaColumnType.Structure) continue;

					object molVo = row[qc.VoPosition];
					MoleculeMx mol = molVo as MoleculeMx;
					if (mol == null) continue;

					QueryColumn cidQc = p.AssocQueryColumns[qci];
					if (cidQc == null) continue; // associated cid col?
					int voi = cidQc.VoPosition;
					if (voi < 0 || voi >= row.Length) continue;

					object cidVo = row[voi];
					if (cidVo == null) continue;
					string cid = cidVo.ToString();
					cid = CompoundId.Normalize(cid);
					if (Lex.IsUndefined(cid)) continue;

					mol.Id = cid;
					molList.Add(mol);
				}
			}

			return molList;
		}

		/// <summary>
		/// Fetch and return any images
		/// </summary>
		/// <param name="rowList"></param>

		void PrefetchImages(List<object[]> rowList)
		{
			const int maxThreadCount = 10;

			ColumnsOfSpecifiedType p = GetColumnsOfSpecifiedType(MetaColumnType.Image);
			if (p.QueryColumns != null && p.QueryColumns.Count == 0) return; // no images to get

			p.RowList = new List<object[]>();
			foreach (object[] row in rowList)
			{
				p.RowList.Add(row);
			}

			p.Row = -1;
			p.Col = p.QueryColumns.Count;

			int threadCount = maxThreadCount;
			int imgCount = p.RowList.Count * p.QueryColumns.Count;
			if (imgCount < threadCount) threadCount = imgCount;
			if (imgCount < 1) threadCount = 1;

			//threadCount = 1; // debug

			p.ThreadLatch = new MultithreadLatch(threadCount); // latch to synchronize with threads

			for (int ti = 0; ti < threadCount; ti++) // start threads
			{
				Thread t = new Thread(new ParameterizedThreadStart(GetImagesThreadMethod));
				t.Name = "GetImages";
				t.IsBackground = true;
				t.SetApartmentState(ApartmentState.STA);
				t.Start(p);
			}

			p.ThreadLatch.Wait(); // wait for all threads to finish
		}

		/// <summary>
		/// GetImagesThreadMethod
		/// </summary>

		void GetImagesThreadMethod(object parms)
		{
			ColumnsOfSpecifiedType p = parms as ColumnsOfSpecifiedType;
			object[] voa;
			object vo;
			QueryColumn qc;

			while (true)
			{
				lock (p.RowList)
				{
					if (p.Row >= p.RowList.Count) break;
					else if (p.Col + 1 >= p.QueryColumns.Count)
					{
						p.Row++;
						p.Col = 0;
						if (p.Row >= p.RowList.Count) break;
						voa = p.RowList[p.Row];
						if (voa == null) continue;
					}
					else
					{
						p.Col++;
						voa = p.RowList[p.Row];
					}

					qc = p.QueryColumns[p.Col];
				}

				if (voa == null) continue;
				vo = voa[qc.VoPosition];

				if (NullValue.IsNull(vo)) continue; // skip if null

				if (!(vo is string)) continue;

				ImageMx imageMx = new ImageMx();
				string resultLink = vo as string;
				imageMx.DbLink = resultLink;

				if (qc.Decimals < 0) continue; // approx width defined?
				int desiredWidth = qc.Decimals;

				imageMx.Value = GetImage(qc.MetaColumn, resultLink, desiredWidth);
				voa[qc.VoPosition] = imageMx; // formatted image back to buffer
			}

			p.ThreadLatch.Signal();
		}

		/// <summary>
		/// Parameters for set of rows/threads
		/// </summary>

		class ColumnsOfSpecifiedType
		{
			public MetaColumnType PrimaryType = MetaColumnType.Unknown;
			public MetaColumnType SecondaryType = MetaColumnType.Unknown;

			public List<QueryColumn> QueryColumns; // list of of specified type
			public List<QueryColumn> AssocQueryColumns; // list of associated cols of another type (e.g. CIDS)
			public MultithreadLatch ThreadLatch; // used for synchronizing data retrieval
			public List<object[]> RowList; // list of rows that are being processed
			public int Row = -1; // current row
			public int Col = -1; // current col
		}

		/// <summary>
		/// Setup a list of the columns in a query of the specified type 
		/// </summary>
		/// <param name="primaryType">Desired MC type</param>
		/// <param name="secondaryType">If defined, MC type to associate with main specified type (usually CompoundId). Must proceed mcType QC in selected cols)</param>
		/// <returns></returns>

		ColumnsOfSpecifiedType GetColumnsOfSpecifiedType(
			MetaColumnType primaryType,
			MetaColumnType secondaryType = MetaColumnType.Unknown)
		{
			ColumnsOfSpecifiedType p = new ColumnsOfSpecifiedType();
			p.PrimaryType = primaryType;
			p.SecondaryType = secondaryType;

			p.QueryColumns = new List<QueryColumn>();
			p.AssocQueryColumns = new List<QueryColumn>();

			foreach (QueryTableData qtd in Qtd)
			{
				QueryColumn assocQc = null;

				foreach (QueryColumn qc in qtd.SelectedColumns)
				{
					MetaColumn mc = qc.MetaColumn;

					if (mc.DataType == secondaryType)
					{
						assocQc = qc;
						continue;
					}

					if (mc.DataType != primaryType) continue;

					if (secondaryType != MetaColumnType.Unknown) // looking for associated type also?
					{
						if (assocQc == null) continue; // ignore if assoc type required but we don't have one available
					}

					p.QueryColumns.Add(qc);
					p.AssocQueryColumns.Add(assocQc);
					assocQc = null; // don't use the same assoc QC again
				}
			}

			return p;
		}

		/// <summary>
		/// Retrieve an Image
		/// </summary>
		/// <param name="metaColumn"></param>
		/// <param name="graphicsIdString"></param>
		/// <param name="desiredWidth"></param>
		/// <returns></returns>

		public Bitmap GetImage(
				MetaColumn metaColumn,
				string graphicsIdString,
				int desiredWidth)
		{
			DateTime t0 = DateTime.Now;

			MetaBrokerType mbType = metaColumn.MetaTable.MetaBrokerType;
			if (metaColumn.MetaTable.MetaBrokerType == MetaBrokerType.CalcField) // if calculated fields must call underlying broker
			{
				mbType = MetaBrokerType.Assay; // default broker type
				if (metaColumn.MetaTable is CalcFieldMetaTable) // underlying broker (may be a regular MetaTable instance the first query executed for some reason)
				{
					CalcFieldMetaTable cfMt = metaColumn.MetaTable as CalcFieldMetaTable;
					mbType = cfMt.CalcField.MetaColumn1.MetaTable.MetaBrokerType;
				}
			}

			IMetaBroker mb = MetaBrokerUtil.GlobalBrokers[(int)mbType];
			if (mb == null) return null;

			Bitmap bmp = mb.GetImage(metaColumn, graphicsIdString, desiredWidth);

			if (LogBasics)
				DebugLog.Message(metaColumn.MetaTable.Name + "." + metaColumn.Name + ", Link: " + graphicsIdString + ", Size: " + bmp.Width + "x" + bmp.Height + ", Time: " + TimeOfDay.Delta(t0));

			return bmp;
		}

		/// <summary>
		/// Get additional data 
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>

		public object GetAdditionalData(
				string command)
		{
			string cmdName, mcName;
			DateTime t0 = DateTime.Now;

			Lex.Split(command, " ", out cmdName, out mcName);

			MetaColumn mc = MetaColumn.ParseMetaTableMetaColumnName(mcName);
			if (mc == null) throw new Exception("MetaColumn not found: " + mcName);
			MetaTable mt = mc.MetaTable;

			GenericMetaBroker mb = null;
			foreach (QueryTableData qtd in Qtd)
			{
				if (Lex.Eq(qtd.Table.MetaTable.Name, mt.Name))
				{
					mb = qtd.Broker;
					break;
				}
			}

			if (mb == null) throw new Exception("Broker not found for table: " + mcName);
			object additionalData = mb.GetAdditionalData(command);
			return additionalData;
		}

		/// <summary>
		/// Transform and execute a query
		/// </summary>
		/// <param name="query">The source query</param>
		/// <param name="transformedQuery">Transformed query or null if original query not transformed</param>
		/// <returns>List of results keys</returns>

		public List<string> TransformAndExecuteQuery(
				Query query,
				out Query transformedQuery)
		{
			Query q, q2, q3, q4;
			bool transformed = false;

			q = query;

			q2 = DoPreSearchTransformations(query); // do initial transformation not requiring knowlege of the hit list            
			if (q2 != null)
			{
				q = q2;
				transformed = true;
			}

			List<string> keyList = ExecuteQuery(q);

			q3 = DoPreRetrievalTableExpansions(); // remap any multipivot tables based on the hitlist            
			if (q3 != null)
			{
				Query = q3; // replace query with transformed query for use in retrieval step                
				AnalyzeQuery(); // Rebuild QueryTableData
				transformed = true;
			}

			q4 = AddCalcFieldDependencies(Query); // confirm all calculated field dependencies are included in the query
			if (q4 != null)
			{
				Query = q4; // replace query with transformed query for use in retrieval step                
				AnalyzeQuery(); // Rebuild QueryTableData 
				transformed = true;
			}

			if (transformed) transformedQuery = Query;
			else transformedQuery = null;

			return keyList;
		}


		/// <summary>
		/// Get all of the metatable dependencies for a particular calculated field.
		/// </summary>
		/// <param name="mtName"></param>
		/// <returns></returns>
		private static List<string> GetCalcFieldDependencies(string mtName)
		{
			List<string> calcFieldDependencies = new List<string>();

			foreach (MetaColumn mc in MetaTableCollection.Get(mtName).MetaColumns)
			{
				string referencedMtName = mc.Name.Replace("_CALC_FIELD", "");
				if (!string.IsNullOrEmpty(referencedMtName))
				{
					if (MetaTableCollection.Get(referencedMtName) != null)
					{
						if (!calcFieldDependencies.Contains(referencedMtName))
							calcFieldDependencies.Add(referencedMtName.Trim());

						List<string> _calcFieldDependencies = GetCalcFieldDependencies(referencedMtName);
						if (_calcFieldDependencies != null)
						{
							foreach (string calcFieldDependency in _calcFieldDependencies)
							{
								if (!calcFieldDependencies.Contains(calcFieldDependency))
									calcFieldDependencies.Add(calcFieldDependency.Trim());
							}
						}
					}
				}
			}

			return calcFieldDependencies.Count > 0 ? calcFieldDependencies : null;

		}

		/// <summary>
		/// Add any missing required calculated field dependencies to the query.
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>
		private Query AddCalcFieldDependencies(Query q)
		{
			bool addQt = false;

			for (int ti = 0; ti < q.Tables.Count; ti++)
			{
				MetaTable mt = q.Tables[ti].MetaTable;
				if (mt.IsCalculatedField)
				{
					List<string> calcFieldDependencies = GetCalcFieldDependencies(mt.Name);

					if (calcFieldDependencies != null)
					{
						foreach (string dependency in calcFieldDependencies)
						{
							if (q.GetQueryTableByName(dependency) is null)
							{
								MetaTable newMt = MetaTableCollection.Get(dependency);
								if (newMt != null)
								{
									QueryTable newQt = new QueryTable(newMt);
									newQt.Query = q;
									newQt.AssignUniqueQueryTableLabel();

									foreach (QueryColumn qc in newQt.QueryColumns)
									{
										int sortOrder = 1;
										if (!qc.IsKey && qc.Is_Selected_or_GroupBy_or_Sorted)
										{
											qc.Selected = false;
											qc.SortOrder = sortOrder;
											sortOrder++;
										}
									}

									q.AddQueryTable(newQt);

									addQt = true;
								}
							}
						}
					}
				}
			}

			if (addQt)
			{
				q.IncludeRootTableAsNeeded();
				q.SetTableIndexes();
			}
			else
			{
				return null;
			}

			return q;

		}

		/// <summary>
		/// Examine query & do any required presearch transforms
		/// </summary>
		/// <returns></returns>

		public static Query DoPreSearchTransformations( // ExpandMultitableTables(
				Query q)
		{
			Query nq; // new query
			QueryTable qt, qt2;
			bool modified = false;
			int ti, ti2;

			try
			{
				q.PresearchDerivedQuery = null; // clear any existing derived query

				for (ti = 0; ti < q.Tables.Count; ti++)// see if we have any multitable tables in query
				{
					qt = q.Tables[ti];
					if (qt.MetaTable.MetaBrokerType == MetaBrokerType.Unknown) continue;
					if (MetaBrokerUtil.GlobalBrokers[(int)qt.MetaTable.MetaBrokerType].ShouldPresearchCheckAndTransform(qt.MetaTable)) break;
				}
				if (ti >= q.Tables.Count)
					return null;

				nq = q.Clone(); // clone query to build
				nq.Tables = new List<QueryTable>(); // clear list of tables

				for (ti = 0; ti < q.Tables.Count; ti++)
				{
					qt = q.Tables[ti];

					if (qt.MetaTable.MetaBrokerType == MetaBrokerType.Unknown) continue;
					IMetaBroker mb = MetaBrokerUtil.GlobalBrokers[(int)qt.MetaTable.MetaBrokerType];
					if (!mb.ShouldPresearchCheckAndTransform(qt.MetaTable))
					{ // just copy if no possible transform
						//	qt = qt.Clone(); // don't clone since browse-time formatting changes won't be propagated back to parent query
						nq.AddQueryTable(qt);
						continue;
					}

					mb = MetaBrokerUtil.Create(qt.MetaTable.MetaBrokerType);
					mb.DoPreSearchTransformation(q, qt, nq);
				}

				q.PresearchDerivedQuery = nq;
				if (nq != null) // link queries if derived query created
					nq.PresearchBaseQuery = q;

				nq.SetTableIndexes();

				return nq;
			}

			catch (UserQueryException ex) // standard user query error
			{
				QueryEngine qe = new QueryEngine();
				qe.Query = q;
				//qe.LogExceptionAndSerializedQuery(ex); // temporary
				throw new UserQueryException(ex.Message, ex); // pass it up
			}

			catch (Exception ex) // unexpected query error
			{
				QueryEngine qe = new QueryEngine();
				qe.Query = q;
				string msg = ex.Message;
				msg += " " + qe.LogExceptionAndSerializedQuery(ex);
				throw new Exception(msg, ex); // pass it up
			}
		}

		/// <summary>
		/// Examine query & expand any tables marked for remapping
		/// Existing remap place holder tables are removed & expansion tables
		/// are added at the end of the query in alphabetical order sorted across
		/// all added tables.
		/// </summary>
		/// <returns></returns>

		public Query DoPreRetrievalTableExpansions()
		{
			Query pq; // pivoted query
			QueryTable qt, qt2;
			int ti, ti2, firstMpQtPos = -1, remapCount = 0, tDelta;
			DateTime t0, t1;

			Stopwatch sw0 = Stopwatch.StartNew();

			for (ti = 0; ti < Query.Tables.Count; ti++)// see if we have any remap tables in query
			{
				qt = Query.Tables[ti];
				if (qt.MetaTable.MultiPivot) break;
			}
			if (ti >= Query.Tables.Count) return null; // return null if nothing to remap

			pq = Query.Clone(); // clone query to build
			pq.Tables = new List<QueryTable>(); // clear list of remapped tables

			Query.InaccessableData = new Dictionary<string, List<string>>(); // in case of remap error

			for (ti = 0; ti < Query.Tables.Count; ti++) // expand remap tables
			{
				qt = Query.Tables[ti];
				if (!qt.MetaTable.MultiPivot) // just clone if not remapped
				{
					qt = qt.Clone();
					pq.AddQueryTable(qt);
					continue;
				}

				if (firstMpQtPos < 0) firstMpQtPos = pq.Tables.Count; // remember position of first remap table
				GenericMetaBroker mb = MetaBrokerUtil.Create(qt.MetaTable.MetaBrokerType);
				mb.Qt = qt;

				try
				{
					int cnt0 = pq.Tables.Count;
					Stopwatch sw1 = Stopwatch.StartNew();
					mb.ExpandToMultipleTables(qt, pq, ResultsKeys);
					remapCount++;

					tDelta = (int)sw1.ElapsedMilliseconds;
					int addCnt = pq.Tables.Count - cnt0;

					if (SearchBrokerList == null) SearchBrokerList = new List<GenericMetaBroker>();
					mb.Label = // build a label with values
							"Expand Table Step -> " + addCnt + " Tables:" + qt.MetaTableName + ":-1:-1:-1:" + qt.MetaTable.MetaBrokerType;

					mb.ReadRowCount = -1;
					mb.ReadRowTime = tDelta;
					SearchBrokerList.Add(mb);

					if (LogBasics)
					{
						DebugLog.Message(
								"PreRetrievalTableExpansion: " + qt.MetaTable.Name + ", Remap count: " + addCnt + ", time: " + tDelta);
					}

				}
				catch (Exception ex)
				{
					ProcessBrokerException(ex, ti);
				}
			}

			// Sort list of added tables by metatable label

			for (ti = firstMpQtPos + 1; ti < pq.Tables.Count; ti++)
			{
				qt = pq.Tables[ti];
				for (ti2 = ti - 1; ti2 >= firstMpQtPos; ti2--)
				{
					qt2 = pq.Tables[ti2];
					if (String.Compare(qt2.MetaTable.Label, qt.MetaTable.Label, true) < 0) break;
					pq.Tables[ti2 + 1] = qt2;
				}
				pq.Tables[ti2 + 1] = qt;
			}

			if (Query.InaccessableData != null)
				pq.InaccessableData = Query.InaccessableData;

			if (LogBasics)
			{
				tDelta = (int)sw0.ElapsedMilliseconds;
				DebugLog.Message("PreRetrievalTableExpansions - Tables = " + remapCount +
						", total time: " + tDelta);
			}

			Stats.BrokerTypeDict = pq.CountTablesByBrokerType();
			return pq;
		}


		/// <summary> 
		/// Split a .Csv format string into an ArrayList of strings
		/// </summary>
		/// <param name="csvString"></param>
		/// <returns></returns>

		public static List<string> SplitCsvString(
				string csvString)
		{
			return SplitCsvString(csvString, false);
		}

		/// <summary>
		/// Split a .Csv format string into an ArrayList of strings
		/// optionally allowing spaces to be used as delimiters
		/// Uses Sebastien Lorion's LumenWorks.Framework.IO.Csv class for parsing.
		/// </summary>
		/// <param name="csvString"></param>
		/// <returns></returns>

		public static List<string> SplitCsvString(
				string csvString,
				bool allowSpaceDelimiters)
		{
			char delim;

			int t0 = TimeOfDay.Milliseconds();
			List<string> items = new List<string>();
			try
			{
				TextReader tr = new StringReader(csvString);
				//bool hasHeaders = false;
				char delimiter = ',';
				if (csvString.IndexOf(",") < 0 && allowSpaceDelimiters)
					delimiter = ' ';
				CsvReader csv = new CsvReader(tr, false, delimiter);

				if (!csv.ReadNextRecord()) return items;

				for (int i1 = 0; i1 < csv.FieldCount; i1++)
					items.Add(csv[i1].Trim());

				//t0 = TimeOfDay.Milliseconds() - t0;
			}
			catch (Exception ex)
			{
				// ignored
			}
			return items;
		}

		/// <summary>
		/// Join an arraylist of values into a .Csv format string
		/// </summary>
		/// <param name="arrayList"></param>
		/// <returns></returns>

		public static string JoinCsvString(
				List<string> arrayList)
		{
			return JoinCsvString(arrayList, false);
		}

		/// <summary>
		/// Join an arraylist of values into a .Csv format string
		/// </summary>
		/// <param name="arrayList"></param>
		/// <param name="formatCompoundIds"></param>
		/// <returns></returns>

		public static string JoinCsvString(
				List<string> arrayList,
				bool formatCompoundIds)
		{
			StringBuilder sb = new StringBuilder();
			foreach (string s in arrayList)
			{
				if (sb.Length > 0) sb.Append(", ");
				if (s.IndexOf(",") > 0 || s.IndexOf(" ") > 0 ||
						s.IndexOf("\'") > 0 || s.IndexOf("\"") > 0)
					sb.Append(Lex.Dq(s));

				else if (formatCompoundIds) sb.Append(CompoundId.Format(s)); // format the number for user

				else sb.Append(s); // keep as is
			}

			return sb.ToString();
		}

		/// <summary>
		/// Get the global broker associated with a metatable
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static GenericMetaBroker GetGlobalBroker(
				MetaTable mt)
		{
			return MetaBrokerUtil.GlobalBrokers[(int)mt.MetaBrokerType] as GenericMetaBroker;
		}

		/// <summary>
		/// Convert a key value to a sortable fixed length string
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>

		public static string GetKeyString(object o)
		{
			if (o == null) return null;

			if (o is int || o is Int16 || o is Int32 || o is Int64)
			{
				Int64 i = Convert.ToInt64(o);
				if (i >= 10000000000) throw new Exception("Integer key value >= 10000000000 : " + i);
				string s = String.Format("{0:0000000000}", i);
				//s = i.ToString();
				return s;
			}

			return o.ToString();
		}

		/// <summary>
		/// Return the QueryTableData for associated QueryTable
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		public QueryTableData GetQueryTableData(QueryTable qt)
		{
			if (Qtd == null) DebugMx.DataException("Qtd is null");

			foreach (QueryTableData qtd in Qtd)
			{
				if (Lex.Eq(qtd.Table.MetaTable.Name, qt.MetaTable.Name)) return qtd;
			}

			return null;
		}

		/// <summary>
		/// Split criteria into subsets to improve search performance.
		/// The work is done based on the metabroker GetTableCriteriaGroupName methods.
		/// This is based mainly on the database server each table with criteria run on
		/// which minmizes the use of Oracle DbLinks which tend to have poor performance.
		/// However, each broker makes it's own decision based on performance specifics
		/// that each broker is aware of.
		/// </summary>
		/// <param name="query"></param>
		/// <param name="criteriaTableDict">Dictionary of QueryTables keyed by table alias</param>
		/// <param name="criteriaToks"></param>
		/// <returns></returns>

		public static List<CriteriaList> SplitCriteriaToOptimizePerformance(
				Query query,
				Dictionary<string, QueryTable> criteriaTableDict,
				List<MqlToken> criteriaToks)
		{
			QueryColumn keyQc = null;
			CriteriaList ctg;
			List<MqlToken> tList;
			QueryTable qt;
			MetaTable mt;
			MetaColumn mc;
			int i1;

			CriteriaList cl = new CriteriaList(criteriaToks);
			cl.ParseQueryColumnTokens();
			Dictionary<string, CriteriaList> ctgDbDict = cl.CriteriaTypeGroupDict;

			for (int qcti = 0; qcti < cl.QcTokList.Count; qcti++)
			{
				MqlToken tok = cl.QcTokList[qcti];
				if (tok.Qc == null) continue;
				QueryColumn qc = tok.Qc;

				ParsedSingleCriteria psc = tok.Psc;

				qt = qc.QueryTable;
				mt = qt.MetaTable;
				string groupName = MetaBrokerUtil.GetTableCriteriaGroupName(qt);
				if (Lex.IsUndefined(groupName)) groupName = "Unknown";

				if (!ctgDbDict.ContainsKey(groupName))
				{
					ctg = new CriteriaList();
					ctg.GroupName = groupName;
					ctgDbDict[groupName] = ctg;
				}

				else ctg = ctgDbDict[groupName];

				if (!ctg.QueryTableSet.Contains(qt)) // add to table set
				{
					ctg.QueryTableSet.Add(qt);
				}

				if (psc.OpEnum == CompareOp.Eq) // call equality simple
				{
					ctg.EqCriteria++;
				}

				else if (psc.OpEnum == CompareOp.In) // also lists
				{
					ctg.ListCriteria++;
				}

				else if (psc.OpEnum == CompareOp.FSS) // count FSS like list
				{
					ctg.ListCriteria++;
				}

				else if (psc.OpEnum == CompareOp.MolSim) // count similarity like range
				{
					ctg.RangeCriteria++;
				}

				else if (psc.OpEnum == CompareOp.SSS) // SSS can be fast or slow
				{
					ctg.RangeCriteria++;
				}

				else ctg.RangeCriteria++; // everything else is non-simple
			}

			// Assign priority to each list of tables in the same instance based on the criteria type & sort in priority order

			List<CriteriaList> ctgList = new List<CriteriaList>();

			foreach (CriteriaList ctg0 in ctgDbDict.Values) // order groups
			{
				if (Lex.Eq(ctg0.GroupName, "Unknown")) ctg0.Priority = 4; // give unknown databases low priority
				else if (ctg0.EqCriteria > 0) ctg0.Priority = 1;
				else if (ctg0.ListCriteria > 0) ctg0.Priority = 2;
				else if (ctg0.RangeCriteria > 0) ctg0.Priority = 3;
				else ctg0.Priority = 4; // slow

				for (i1 = ctgList.Count - 1; i1 >= 0; i1--)
				{
					if (ctgList[i1].Priority <= ctg0.Priority) break;
				}

				ctgList.Insert(i1 + 1, ctg0);
			}

			// Adjust included criteria for each group based on the tables in the group

			List<CriteriaList> criteriaSubsets = new List<CriteriaList>();
			if (ctgList.Count > 0)
			{
				foreach (CriteriaList ctg0 in ctgList)
				{
					tList = new List<MqlToken>(criteriaToks); // make copy of original tokens

					foreach (QueryTable qt0 in criteriaTableDict.Values) // remove criteria for any tables not in our group
					{
						if (ctg0.QueryTableSet.Contains(qt0)) continue;

						tList = MqlUtil.DisableCriteria(tList, query, null, qt0, null, null, true, false, true);
					}

					criteriaSubsets.Add(new CriteriaList(tList)); // add list with all disables
				}
			}

			else // if nothing in group list then just return original criteria list
			{
				criteriaSubsets.Add(new CriteriaList(criteriaToks));
			}

			//List<string>lltDebug = MqlUtil.CatenateCriteriaTokensForDebug(llt, true); // debug - get criteria token list in text form

			return criteriaSubsets;
		}

	} // end of QueryEngine class

	/// <summary>
	/// Additional QE data associated with each querytable
	/// </summary>

	public class QueryTableData
	{
		public QueryTable Table; // query table this data is for
		public QueryTable ParentTable = null; // our parent
		public int CriteriaCount = 0; // count of non-key criteria for table
		public int KeyColPos = -1; // position of key column in vo
		public bool KeyAdded; // if true key has been added for table
		public int TableVoPosition = 0; // position of the first column for the table in the vo
		public List<QueryColumn> SelectedColumns = null; // selected QueryColumn objects
		public int SelectCount = 0; // count of columns selected for table
		public int ParentVoPos = -1; // for hierarchical data where link to us goes in parent vo
		public List<QueryTable> ChildTables = null; // list of child tables below this table
		public GenericMetaBroker Broker = null; // metabroker instance for table
		public ExecuteQueryParms Eqp = null; // execute query parms for table
		public Aggregator AggregationData = null; // row groups if aggregated data
		public int RowsFetched = 0; // number of rows fetched
		public bool Closed; // if true at end of cursor for table
	}

	/// <summary>
	/// Parent class for LogicGroup and CriteriaSubset
	/// </summary>
	public class LogicElement
	{
		public LogicGroup ParentLogicGroup = null; // group that this element is a part of
	}

	/// <summary>
	/// A list of one or more CriteriaSubsets and/or sub-LogicGroups to be individually executed and then logically combined together with the specified logic type
	/// </summary>

	public class LogicGroup : LogicElement
	{
		public int Id = 0; // index of logic group within search
		public QueryLogicType LogicType = QueryLogicType.Unknown; // And or Or Logic for this group. If only one item in ElementList then logic type is not relevant since to logic is applied
		public List<LogicElement> ElementList = new List<LogicElement>(); // each element of the group is either a sub-LogicGroup or a CriteriaSubset at the lowest level

		public int ElementsProcessed = 0; // number of elements in the group that have completed processing
		public List<string> SearchKeySubset = null; // subset of keys to limit search to
		public Dictionary<string, object> ResultKeys = new Dictionary<string, object>(); // current list of keys found for this logic group

		public LogicGroup()
		{
			return;
		}

		public LogicGroup(QueryLogicType logicType)
		{
			LogicType = logicType;
			return;
		}

		/// <summary>
		/// IndexOfLastCriteriaList
		/// </summary>
		/// <returns></returns>

		public int IndexOfLastCriteriaList()
		{
			int lastCtl = -1;
			for (int i1 = 0; i1 < ElementList.Count; i1++)
			{
				if (ElementList[i1] is CriteriaList)
					lastCtl = i1;
			}

			return lastCtl;
		}

		/// <summary>
		/// Add a criteria subset to the LogicGroup
		/// </summary>
		/// <param name="tokPosSet"></param>
		/// <param name="criteriaSubsets"></param>
		/// <returns></returns>

		public CriteriaList AddCriteriaSubset(
				CriteriaList sourceCriteria,
				HashSet<int> tokPosSet,
				List<CriteriaList> criteriaSubsets)
		{
			//List<MqlToken> tokList,

			if (tokPosSet == null || tokPosSet.Count == 0) return null;

			int qcTokCnt = 0;
			foreach (int tki0 in tokPosSet) // check to see if at least one QueryColumn token
			{
				if (sourceCriteria.Tokens[tki0].Qc != null) qcTokCnt++;
			}
			if (qcTokCnt == 0) return null;

			List<MqlToken> csToks = MqlUtil.DisableCriteriaNotInSet(sourceCriteria.Tokens, tokPosSet);
			CriteriaList cs = new CriteriaList(csToks);
			criteriaSubsets.Add(cs);

			cs.ParentLogicGroup = this;
			ElementList.Add(cs);
			return cs;
		}

		/// <summary>
		/// ToString
		/// </summary>
		/// <returns></returns>

		public override string ToString()
		{
			return ToString2();
		}

		public string ToString2(int indent = 0)
		{
			string pad = new string(' ', indent);
			string txt2 = "";
			string txt = pad + "Logic Group " + Id + ", LogicType: " + LogicType + "\r\n";
			foreach (LogicElement e in ElementList)
			{
				if (e is LogicGroup)
				{
					LogicGroup lg = (LogicGroup)e;

					txt += pad + "  Logic Group " + lg.Id + "\r\n";
					txt2 += lg.ToString2(indent + 2);
				}

				else txt += pad + "  " + e.ToString() + "\r\n";
			}

			txt += "\r\n" + txt2;

			return txt;
		}

	}

	/// <summary>
	/// Data used in building search part of query with the separated into groups by database "connection" (Oracle instance) or other table criteria
	/// </summary>

	public class CriteriaList : LogicElement
	{
		public List<MqlToken> Tokens = null; // Mql tokens in the list the CriteriaList
		public List<MqlToken> QcTokList = new List<MqlToken>(); // list of QC tokens in the CriteriaList
		public HashSet<QueryTable> QueryTableSet = new HashSet<QueryTable>(); // HashSet of QueryTables included in the CriteriaList

		// Grouping information

		public string GroupName = ""; // database instance or other name for tables in the group
		public Dictionary<string, CriteriaList> CriteriaTypeGroupDict = new Dictionary<string, CriteriaList>(); // dict of criteria keyed by criteria type group

		public int EqCriteria = 0; // simple equality
		public int ListCriteria = 0; // small list criteria
		public int RangeCriteria = 0; // range criteria 
		public int SlowCriteria = 0; // other slow criteria
		public int Priority = 0; // order in which this table group should be executed

		public CriteriaList()
		{
			return;
		}

		public CriteriaList(List<MqlToken> tokList)
		{
			Tokens = tokList;
			return;
		}

		/// <summary>
		/// Create a new criteria subset and add it to the
		/// criteria subset list and associated logic group
		/// </summary>
		/// <param name="criteriaSubsets"></param>
		/// <param name="logicGroup"></param>
		/// <param name="tokList"></param>
		/// <returns></returns>

		public static CriteriaList Add(
				List<CriteriaList> criteriaSubsets,
				LogicGroup logicGroup,
				List<MqlToken> tokList)
		{
			CriteriaList cs = new CriteriaList();
			cs.Tokens = tokList;
			Add(criteriaSubsets, logicGroup, cs);
			return cs;
		}

		/// <summary>
		/// Add CriteriaSubset to criteria subset list and associated logic group
		/// </summary>
		/// <param name="criteriaSubsets"></param>
		/// <param name="logicGroup"></param>
		/// <param name="cs"></param>

		public static void Add(
				List<CriteriaList> criteriaSubsets,
				LogicGroup logicGroup,
				CriteriaList cs)
		{
			criteriaSubsets.Add(cs);
			cs.ParentLogicGroup = logicGroup;
			logicGroup.ElementList.Add(cs);
			return;
		}

		/// <summary>
		/// Perform basic analysis on the QueryColumn tokens in the list
		/// </summary>

		public void ParseQueryColumnTokens()
		{
			CriteriaList ctg;
			QueryTable qt;
			MetaTable mt;
			MetaColumn mc;
			int i1;

			QueryTableSet = new HashSet<QueryTable>();
			QcTokList = new List<MqlToken>();

			CriteriaTypeGroupDict = new Dictionary<string, CriteriaList>();
			Dictionary<string, CriteriaList> ctgDict = CriteriaTypeGroupDict;

			EqCriteria = 0;
			ListCriteria = 0;
			RangeCriteria = 0;
			SlowCriteria = 0;
			Priority = 0;

			for (int ti = 0; ti < Tokens.Count; ti++)
			{
				MqlToken mqlTok = Tokens[ti];
				QueryColumn qc = mqlTok.Qc;
				if (qc == null) continue;
				if (!Lex.IsDefined(qc.Criteria)) continue;


				if (qc.IsKey) // include key criteria only if requires a database search
				{
					if (!MqlUtil.KeyCriteriaRequiresDatabaseSearch(qc.Criteria))
						continue;
				}

				ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(qc.Criteria);
				if (psc == null) continue;
				mqlTok.Psc = psc; // save parse details
				QcTokList.Add(mqlTok);

				qt = qc.QueryTable;
				if (!QueryTableSet.Contains(qt))
					QueryTableSet.Add(qt);
			}

			return;
		}

		/// <summary>
		/// Clone
		/// </summary>
		/// <returns></returns>

		public CriteriaList Clone()
		{
			CriteriaList cl2 = (CriteriaList)this.MemberwiseClone();
			cl2.Tokens = MqlUtil.CopyMqlTokenList(Tokens);
			return cl2;
		}

		/// <summary>
		/// Format a list of lists for debugging
		/// </summary>
		/// <param name="lists"></param>
		/// <returns></returns>

		public static string ListsToString(
				List<CriteriaList> lists)
		{
			string txt = "=== List of CriteriaLists ===\r\n";
			for (int i1 = 0; i1 < lists.Count; i1++)
			{
				txt += i1.ToString() + "\t" + lists[i1].ToString() + "\r\n";
			}

			return txt;
		}

		/// <summary>
		/// ToString
		/// </summary>
		/// <returns></returns>

		public override string ToString()
		{
			string txt = "Criteria List";
			if (Lex.IsDefined(GroupName)) txt += " " + GroupName;
			txt += ": " + MqlUtil.CatenateCriteriaTokensForDebug(Tokens);
			return txt;
		}
	}

	/// <summary>
	/// Intermediate unmerged data for all tables in a query for a set of keys
	/// </summary>

	internal class ChunkTableData
	{

		/// <summary>
		/// Dictionary of array of rows for each table keyed by key (e.g. CID)
		/// An array of data rows is stored under each table under each key
		/// </summary>

		internal Dictionary<string, List<object[]>[]> KeyToTableDataDict = null;

		/// <summary>
		/// Gets list of tables and associated data rows for a specific key
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>

		internal List<object[]>[] TableDataForKey(string key)
		{
			List<object[]>[] tablesForKey = KeyToTableDataDict[key];
			return tablesForKey;
		}

		/// <summary>
		/// Constructor - Creates a key dictionary with the list of tables for each key. Data is added later.
		/// </summary>
		/// <param name="chunkKeys"></param>
		/// <param name="tableCount"></param>

		internal ChunkTableData(List<string> chunkKeys, int tableCount)
		{
			KeyToTableDataDict = new Dictionary<string, List<object[]>[]>();

			foreach (string key in chunkKeys)
			{
				if (key == null) { throw new Exception("Null ChunkData key"); }

				List<object[]>[] tablesForKey = new List<object[]>[tableCount]; // array for key with one entry per querytable
				KeyToTableDataDict[key] = tablesForKey; // map key to data for that key 
			}

		}

	}

	/// <summary>
	/// CountdownLatch used for synchronizing data retrieval threads
	/// </summary>

	public class MultithreadLatch
	{
		private int m_remain;
		private EventWaitHandle m_event;

		public MultithreadLatch(int count)
		{
			m_remain = count;
			m_event = new ManualResetEvent(false);
		}

		public void Signal()
		{
			// The last thread to signal also sets the event.
			if (Interlocked.Decrement(ref m_remain) == 0)
				m_event.Set();
		}

		public void Wait()
		{
			m_event.WaitOne();
		}
	}

} // end of namespace Mobius.QueryEngineLibrary
