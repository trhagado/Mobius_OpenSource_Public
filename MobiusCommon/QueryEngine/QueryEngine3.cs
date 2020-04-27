using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mobius.QueryEngineLibrary
{

	/// <summary>
	/// Utility methods associated with executing queries against the databases and returning results
	/// File: QueryEngine3.cs
	/// </summary>

	public partial class QueryEngine
	{

		/// <summary>
		/// Get query to select all data. 
		/// It consists of a single QueryTable with the CID and key table name
		/// This keeps the initial query sent to the client small which helps 
		/// response time for users not on the same local network as the server.
		/// </summary>
		/// <param name="keyMtName"></param>
		/// <param name="cn"></param>
		/// <returns></returns>

		public static Query GetSelectAllDataQuery(
			string keyMtName,
			string cn)
		{
			MetaTable mt = MetaTableCollection.GetWithException(MetaTable.AllDataQueryTable);

			Query q = new Query();
			QueryTable qt = new QueryTable(mt);
			q.AddQueryTable(qt);

			MetaTable keyMt = null;
			if (Lex.IsDefined(keyMtName)) keyMt = MetaTableCollection.Get(keyMtName);

			if (keyMt != null && keyMt.Root.IsUserDatabaseStructureTable) // if root metatable is user database then normalize based on key
			{
				keyMt = keyMt.Root; // be sure we have root
				cn = CompoundId.Normalize(cn, keyMt);
			}

			else
			{
				cn = CompoundId.Normalize(cn);
				keyMt = CompoundId.GetRootMetaTableFromCid(cn, keyMt);
				keyMt = keyMt.Root; // be sure we have root (may not be structure table)
			}

			if (keyMt == null) throw new Exception("Failed to identify key MetaTable");

			q.KeyCriteria = q.KeyCriteriaDisplay = " = " + cn;

			QueryColumn qc = qt.GetQueryColumnByNameWithException("root_table");
			qc.Criteria = qc.CriteriaDisplay = qc.MetaColumn.Name + " = " + keyMt.Name;

			return q;
		}

		/// <summary>
		/// Transform basic query to select all data for a compound number
		/// </summary>
		/// <param name="keyMt">Key metatable. If null then try to determine from key value</param>
		/// <param name="cn"></param>
		/// <returns></returns>

		public static Query TransformSelectAllDataQuery(
			Query originalQuery,
			QueryTable qt0,
			Query newQuery)
		{
			Query q2 = null;
			MetaTable mt;
			MetaColumn mc;
			QueryTable qt;
			QueryColumn qc;
			MetaTreeNode mtn, tn;

			qc = qt0.GetQueryColumnByNameWithException("root_table");
			ParsedSingleCriteria psc = MqlUtil.ParseQueryColumnCriteria(qc);
			if (psc == null || Lex.IsUndefined(psc.Value)) throw new UserQueryException("Root table not defined");
			string keyMtName = psc.Value;

			psc = MqlUtil.ParseSingleCriteria("cid " + originalQuery.KeyCriteria);
			if (psc == null || Lex.IsUndefined(psc.Value)) throw new UserQueryException("Compound Id not defined");
			string cn = psc.Value;

			MetaTable keyMt = null;
			if (Lex.IsDefined(keyMtName)) keyMt = MetaTableCollection.Get(keyMtName);

			if (keyMt != null && keyMt.Root.IsUserDatabaseStructureTable) // if root metatable is user database then normalize based on key
			{
				keyMt = keyMt.Root; // be sure we have root
				cn = CompoundId.Normalize(cn, keyMt);
			}

			else
			{
				cn = CompoundId.Normalize(cn);
				keyMt = CompoundId.GetRootMetaTableFromCid(cn, keyMt);
				keyMt = keyMt.Root; // be sure we have root (may not be structure table)
			}

			if (keyMt == null) throw new Exception("Failed to identify key MetaTable");

			string allTableName = keyMt.Name + "_AllData"; // see if specific all-data tree node
			mtn = MetaTree.GetNode(allTableName);
			if (mtn == null) // no special "_AllData" node, lookup in menu
			{
				foreach (MetaTreeNode parent in MetaTree.Nodes.Values)
				{
					foreach (MetaTreeNode child in parent.Nodes)
					{
						if (Lex.Eq(child.Target, keyMt.Name))
						{
							mtn = parent;
							break;
						}
					}
				}

				IUserObjectTree iuot = InterfaceRefs.IUserObjectTree;
				if (mtn == null && keyMt.IsUserDatabaseStructureTable && iuot != null) // see if user structure table & db
				{
					int userObjId = UserObject.ParseObjectIdFromInternalName(keyMt.Name);
					string nodeItemId = "ANNOTATION_" + userObjId;
					MetaTreeNode childMtn = iuot.GetUserObjectNodeBytarget(nodeItemId);
					if (childMtn != null && childMtn.Parent.Type == MetaTreeNodeType.Database)
						mtn = childMtn.Parent;
				}
			}

			if (mtn == null) return null;

			Query q = newQuery;

			for (int i1 = 0; i1 < mtn.Nodes.Count; i1++)
			{
				tn = (MetaTreeNode)mtn.Nodes[i1];
				if (!tn.IsDataTableType) continue;

				mt = MetaTableCollection.Get(tn.Target);
				if (mt == null) continue;
				if (mt.Root.Name != keyMt.Name) continue; // must have same root

				if (mt.MultiPivot && !mt.UseSummarizedData && mt.SummarizedExists)
				{
					MetaTable mt2 = MetaTableCollection.Get(mt.Name + MetaTable.SummarySuffix);
					if (mt2 != null) mt = mt2;
				}

				//if (mt.RemapForRetrieval && mt.SummarizedExists) mt.UseSummarizedData = true; // get summarized multipivot data (not good, permanently changes the metatable)

				qt = new QueryTable(mt);
				//				if (Lex.Eq(mt.Name, "all_star_pivoted") || Lex.Eq(mt.Name, "all_annotation_pivoted")) mt = mt // debug;

				if (qt.SelectedCount > 0) // be sure something is selected
					q.AddQueryTable(qt);
			}

			// See if a model query exists & use it or append to what we have already

			string fileName = ServicesDirs.ModelQueriesDir + @"\" + allTableName + ".qry";
			if (!ServerFile.GetLastWriteTime(fileName).Equals(DateTime.MinValue)) // model query file exist?
				try
				{
					string query2String = FileUtil.ReadFile(fileName);
					q2 = Query.Deserialize(query2String);
					q.MergeSubqueries(q2); // just use subquery
				}
				catch (Exception ex) { ex = ex; }

			q.SetupQueryPagesAndViews(ResultsViewType.HtmlTable); // be sure we have a default page & HTML view

			// Set key criteria

			q.KeyCriteria = " = " + cn;

			// Review tables (debug)

			//int tCnt = q.Tables.Count;
			//string tls = q.TableListString;
			//q.Tables.RemoveRange(23, 1);
			//q.Tables.RemoveRange(27, q.Tables.Count - 27);
			//q.Tables.RemoveRange(1, 25);

			// Get list of any inaccessible tables & remove from query

			q.InaccessableData = CheckDataSourceAccessibility(q);

			if (q.InaccessableData != null)
			{
				foreach (string schema in q.InaccessableData.Keys)
				{
					foreach (string tName in q.InaccessableData[schema])
					{
						qt = q.GetQueryTableByName(tName);
						if (qt != null && !qt.MetaTable.IsRootTable && q.Tables.Contains(qt))
							q.RemoveQueryTable(qt);
					}
				}

				//ShowUnavailableDataMessage(q);
				q.InaccessableData = null;
			}

			UsageDao.LogEvent("QueryAllData", "");

			//string mql = MqlUtil.ConvertQueryToMql(q); // debug

			return q;
		}


		/// <summary>
		/// Set global QE parameter
		/// </summary>
		/// <param name="cidListString"></param>

		public static void SetParameter(
			string parm,
			string value)
		{
			if (Lex.Eq(parm, "DatabaseSubset"))
			{
				if (Lex.IsNullOrEmpty(value))
					DatabaseSubset = null;
				else DatabaseSubset = CidList.ToStringList(value);
			}

			else if (Lex.Eq(parm, "AllowNetezzaUse"))
			{
				bool.TryParse(value, out AllowNetezzaUse);
			}

			else if (Lex.Eq(parm, "AllowMultiTablePivot"))
			{
				bool.TryParse(value, out AllowMultiTablePivot);
			}

			else if (Lex.Eq(parm, "DefaultToSingleStepQueryExecution"))
			{
				bool.TryParse(value, out MqlUtil.DefaultToSingleStepQueryExecution);
			}

			return;
		}

		/// <summary>
		/// Analyze key criteria in token list.
		/// A saved list reference is returned as a set of key values &
		/// blanked out in the token list.
		/// Other key references are returned as a set of indexes in a list.
		/// </summary>
		/// <param name="q"></param>
		/// <param name="Qtd"></param>
		/// <param name="tokens"></param>
		/// <param name="keyCriteriaPositions">Token indexes for key criteria column names</param>
		/// <param name="keyCriteriaSavedListKeys">Keys in any referenced saved list</param>
		/// <param name="keyCriteriaInListKeys">Keys in any literal list </param>
		/// <param name="keyCriteriaConstantPositions">Positions of key values for literal lists</param>

		public static void AnalyzeKeyCriteria(
			Query q,
			QueryTableData[] Qtd,
			List<MqlToken> tokens,
			out CompareOp keyCriteriaOp,
			out List<int> keyCriteriaPositions,
			out List<string> keyCriteriaSavedListKeys,
			out List<string> keyCriteriaInListKeys,
			out List<int> keyCriteriaConstantPositions)
		{
			string tok1, tok2, tok3, tok4, tok5;
			keyCriteriaOp = CompareOp.Unknown;
			keyCriteriaPositions = new List<int>();
			keyCriteriaSavedListKeys = null;
			keyCriteriaInListKeys = null;
			keyCriteriaConstantPositions = new List<int>();

			for (int tki = 0; tki < tokens.Count; tki++)
			{

				QueryColumn qc = tokens[tki].Qc;
				if (qc == null) continue;
				if (!qc.IsKey) continue;

				keyCriteriaPositions.Add(tki); // remember position of key col reference

				if (tokens.Count < tki + 2)
					throw new QueryException("Incomplete compound id criteria at end of statement");

				bool notLogic = false;

				tok1 = GetTokenListString(tokens, tki + 1);

				if (Lex.Eq(tok1, "Not"))
				{
					notLogic = true;
					tki++;
					if (tokens.Count < tki + 2)
						throw new QueryException("Incomplete compound id criteria at end of statement");
					tok1 = GetTokenListString(tokens, tki + 1);
				}

				tok2 = GetTokenListString(tokens, tki + 2);
				tok3 = GetTokenListString(tokens, tki + 3);
				tok4 = GetTokenListString(tokens, tki + 4);
				tok5 = GetTokenListString(tokens, tki + 5);

				// Saved List

				if (tokens.Count > tki + 3 && Lex.Eq(tok1, "In") &&
					Lex.Eq(tok2, "List"))
				{ // have a saved list
					keyCriteriaOp = CompareOp.InList;

					if (keyCriteriaSavedListKeys != null) // already have it? 
						throw new UserQueryException("Only one condition is allowed for the " + qc.ActiveLabel + " field");

					if (notLogic) // not currently allowed for saved lists
						throw new UserQueryException("\"Not\" logic is not allowed for " + qc.ActiveLabel + " saved lists");

					string listName = tokens[tki + 3].Tok.Text;
					UserObject uo = ResolveCidListReference(listName);
					if (uo == null) throw new UserQueryException("Key list " + listName + " not found");
					listName = uo.InternalName;
					CidList keyCriteriaSavedList = CidListDao.Read(listName, QueryEngine.GetRootTable(q));
					if (keyCriteriaSavedList == null) throw new UserQueryException("Key list " + listName + " not found");
					keyCriteriaSavedListKeys = keyCriteriaSavedList.ToStringList();
					tokens[tki].Qc = null;
					tokens[tki].Tok.Text = "";
					for (int tki2 = tki + 1; tki2 <= tki + 3; tki2++)
						tokens[tki2].Tok.Text = "";
					if (!MqlUtil.DisableAdjacentAndLogic(tokens, tki, tki + 3))
						throw new UserQueryException("Only \"And\" logic is allowed with " +
							qc.ActiveLabel + " saved lists");

					int ti = q.GetQueryTableIndexByAlias(qc.QueryTable.Alias);
					Qtd[ti].CriteriaCount--;
					keyCriteriaPositions.RemoveAt(keyCriteriaPositions.Count - 1);
				}

				// Explicit list of allowed keys

				else if (tokens.Count > tki + 2 && Lex.Eq(tok1, "In") &&
					Lex.Eq(tok2, "("))
				{
					keyCriteriaOp = CompareOp.In;

					int listKeyCount = 0; // count keys in this list
					for (int tki2 = tki + 3; tki2 < tokens.Count; tki2++)
					{
						tok1 = tokens[tki2].Tok.Text;
						if (tok1 == ",") continue;
						else if (tok1 == ")") break;
						keyCriteriaConstantPositions.Add(tki2);
						if (keyCriteriaInListKeys == null)
							keyCriteriaInListKeys = new List<string>();

						MetaTable rootTable = QueryEngine.GetRootTable(q);
						string normKey = CompoundId.Normalize(tok1, rootTable);
						if (normKey != null && normKey != "")
						{
							keyCriteriaInListKeys.Add(normKey);
							listKeyCount++;
						}
					}

					if (listKeyCount == 0) throw new UserQueryException("The query contains an invalid empty list of " + qc.ActiveLabel + "s");
				}

				// Between a range of key values

				else if (tokens.Count > tki + 4 && Lex.Eq(tok1, "Between") &&
					Lex.Eq(tok3, "And"))
				{
					keyCriteriaOp = CompareOp.Between;

					keyCriteriaConstantPositions.Add(tki + 2);
					keyCriteriaConstantPositions.Add(tki + 4);
				}

				// Single key value, treat like a list with a single value

				else if (Lex.Eq(tok1, "="))
				{
					keyCriteriaOp = CompareOp.Eq;

					keyCriteriaConstantPositions.Add(tki + 2);
					if (keyCriteriaInListKeys == null)
						keyCriteriaInListKeys = new List<string>();

					MetaTable rootTable = QueryEngine.GetRootTable(q);
					string normKey = CompoundId.Normalize(tok2, rootTable);
					if (Lex.IsDefined(normKey))
					{
						keyCriteriaInListKeys.Add(normKey);
					}
				}

				// Other binary operator

				else if (MqlUtil.IsBasicComparisonOperator(tok1))
				{
					keyCriteriaOp = CompareOpString.ToCompareOp(tok1);
					keyCriteriaConstantPositions.Add(tki + 2);
				}

				// Unary operator "is null"

				else if (Lex.Eq(tok1, "Is") && Lex.Eq(tok2, "Null"))
				{
					keyCriteriaOp = CompareOp.IsNull;
				}

				// Unary operator "is not null"

				else if (Lex.Eq(tok1, "Is") && Lex.Eq(tok2, "Not") && Lex.Eq(tok3, "Null"))
				{
					keyCriteriaOp = CompareOp.IsNotNull;
				}

				else throw new QueryException("Unrecognized compound id condition " + tok1);
			}

			return;
		}

		/// <summary>
		/// Get a the text of a token from a MqlTokenList returning "" if index is out of range
		/// </summary>
		/// <param name="list"></param>
		/// <param name="index"></param>
		/// <returns></returns>

		public static string GetTokenListString(
			List<MqlToken> list,
			int index)
		{
			if (list != null && index >= 0 && index < list.Count) return list[index].Tok.Text;
			else return "";
		}

		/// <summary>
		/// Examine a text reference to a cnList & try to resolve to an existing cnList
		/// </summary>
		/// <param name="tok"></param>
		/// <returns></returns>

		public static UserObject ResolveCidListReference(
			string tok)
		{
			UserObject uo, uo2;

			tok = Lex.RemoveAllQuotes(tok).ToUpper(); // remove any quotes

			// New form: <objectType>_<objectId>

			if (UserObject.IsCompoundIdListName(tok))
			{
				try
				{
					int objectId = int.Parse(tok.Substring(7));
					if (objectId == 0) // current list
						uo = new UserObject(UserObjectType.CnList, Security.UserName, "Current");
					else uo = UserObjectDao.ReadHeader(objectId); // other list
					return uo;
				}
				catch (Exception ex) { return null; }
			}

			// May be older form of identifier
			// 1. Current
			// 2. Owner.Folder.Name
			// 3. Folder.Name
			// 4. Name

			else
			{
				if (Lex.Eq(tok, "Current")) // if "current" then fully qualify
					tok = UserObject.TempFolderNameQualified + tok;

				if (tok.Contains(".") || tok.Contains("_"))
					uo = UserObject.ParseInternalUserObjectName(tok, Security.UserName);

				else // if simple unqualified name try default folder for user
				{
					uo = new UserObject();
					uo.Name = tok;
					uo.Owner = Security.UserName;

					string preferredProject = UserObjectDao.GetUserParameter(Security.UserName, "PreferredProject");
					if (Lex.IsNullOrEmpty(preferredProject)) return null;
					uo.ParentFolder = preferredProject;
				}

				uo.Type = UserObjectType.CnList;
				UserObject listUo = UserObjectDao.ReadHeader(uo);
				if (listUo != null) return listUo;

				// See if there is a "Lists" folder one level down that contains this

				uo2 = new UserObject();
				uo2.Type = UserObjectType.Folder;
				uo2.Owner = uo.Owner;
				uo2.ParentFolder = uo.ParentFolder;
				uo2.Name = "Lists";
				uo2 = UserObjectDao.ReadHeader(uo2);
				if (uo2 == null) return null;

				uo.ParentFolder = "FOLDER_" + uo2.Id.ToString();
				listUo = UserObjectDao.ReadHeader(uo);
				return listUo;
			}
		}

		/// <summary>
		/// Check that the specified calculated field expression is valid
		/// </summary>
		/// <param name="advExpr"></param>
		/// <returns></returns>

		public static string ValidateCalculatedFieldExpression(
			string advExpr)
		{
			try
			{
				CalcFieldMetaBroker mb = new CalcFieldMetaBroker();
				string sql = mb.BuildAdvancedExpressionSql(advExpr);
				sql = "select * from (" + sql + ") where 1=2"; // wrap in some sql for rapid evaluation
				DbCommandMx dao = new DbCommandMx();
				DateTime t0 = DateTime.Now;
				dao.Prepare(sql);
				dao.ExecuteReader();
				dao.Dispose();
				double ms = TimeOfDay.Delta(t0);
				return "";
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}


		/// Build an unparameterized key list SQL predicate
		/// </summary>
		/// <param name="Qt"></param>
		/// <param name="keyName">Key column name qualified by table name/alias</param>

		/// <summary>
		/// Build an unparameterized key list SQL predicate
		/// </summary>
		/// <param name="eqp"></param>
		/// <param name="baseSql"></param>
		/// <param name="keyName">Key column name qualified by table name/alias</param>
		/// <param name="keyList"></param>
		/// <param name="firstKeyIdx"></param>
		/// <param name="keyCount"></param>
		/// <param name="keyCriteria">Full criteria including col name operator and list</param>
		/// <param name="keyListString">Just the list of keys</param>

		public void BuildUnparameterizedKeyListPredicate(
			ExecuteQueryParms eqp,
			ref string baseSql,
			string keyName,
			List<string> keyList,
			int firstKeyIdx,
			int keyCount,
			out string keyCriteria,
			out string keyListString)
		{
			StringBuilder sb;
			int i1;

			QueryTable qt = eqp.QueryTable;
			MetaTable mt = qt.MetaTable;
			bool integerKey = mt.IsIntegerKey();

			List<StringBuilder> sublists = new List<StringBuilder>();
			sb = new StringBuilder();
			sublists.Add(sb);

			int sublistKeyCount = 0; // current keys in predicate

			keyCriteria = keyListString = null;

			for (i1 = 0; i1 < keyCount; i1++)
			{
				if (sublistKeyCount >= DbCommandMx.MaxOracleInListItemCount)
				{
					sb = new StringBuilder();
					sublists.Add(sb);
					sublistKeyCount = 0;
				}

				if (sb.Length > 0) sb.Append(",");
				string key = CompoundId.NormalizeForDatabase((string)keyList[firstKeyIdx + i1], qt.MetaTable);
				if (key == null) key = NullValue.NullNumber.ToString(); // if fails supply a "null" numeric value

				if (!integerKey || !Lex.IsInteger(key)) // quote it if not integer column or value
					key = Lex.AddSingleQuotes(key); // (note: quoted integers can cause mismatches for some database systems, e.g. Denodo)

				sb.Append(key);
				sublistKeyCount++;
			}

			sb = new StringBuilder();
			if (sublists.Count >= 2) sb.Append("("); // wrap in parens if multiple sublists
			for (int sli = 0; sli < sublists.Count; sli++)
			{
				if (sli > 0) sb.Append(" or ");
				sb.Append(keyName + " in (" + sublists[sli] + ")");
			}
			if (sublists.Count >= 2) sb.Append(")");
			keyCriteria = sb.ToString();

			keyListString = sublists[0].ToString(); // return just the first sublist (adjust later for larger lists)
			return;
		}

		/// <summary>
		/// Build in list predicate using temporary database table
		/// </summary>
		/// <param name="eqp"></param>
		/// <param name="baseSql"></param>
		/// <param name="keyName"></param>
		/// <param name="keyList"></param>
		/// <param name="firstKeyIdx"></param>
		/// <param name="keyCount"></param>
		/// <returns></returns>

		public string BuildTempDbTableKeyListPredicate(
			ExecuteQueryParms eqp,
			ref string baseSql,
			string keyName,
			List<string> keyList,
			int firstKeyIdx,
			int keyCount)
		{
			bool intKey = true;

			if (QueryEngine.AllowNetezzaUse && eqp.AllowNetezzaUse) // && keyCount > MaxNetzzaInListItemCount)
				return DbCommandMx.BuildNetezzaTempDbTableKeyListPredicate(keyName, keyList, firstKeyIdx, keyCount);

			else // if (keyCount > MaxOracleInListItemCount)
			{
				return DbCommandMx.BuildOracleTempDbTableKeyListPredicate(ref baseSql, keyName, intKey, keyList, firstKeyIdx, keyCount);
			}
		}

		/// <summary>
		/// Build a single-parameter in-list by converting a key list string into a table using string operations and the Dual table
		/// (From Tom Kyte: Varying in lists..., 2-June-2006)
		/// (Note that this only works for strings up to 4000 chars and not currently used.)
		/// </summary>
		/// <param name="eqp"></param>
		/// <param name="keyName"></param>
		/// <param name="keyList"></param>
		/// <param name="firstKeyIdx"></param>
		/// <param name="keyCount"></param>
		/// <returns></returns>

		public string BuildSingleParameterOracleKeyListPredicate(
			ExecuteQueryParms eqp,
			string keyName,
			List<string> keyList,
			int firstKeyIdx,
			int keyCount,
			out string keyListString)
		{
			string sql =
				keyName + @" in (
					select
					trim( substr (txt,
								instr (txt, ',', 1, level  ) + 1,
								instr (txt, ',', 1, level+1)
										- instr (txt, ',', 1, level) -1 ) )
						as token
					from (select ','||:0||',' txt
									from dual)
					connect by level <=
						length(:0)-length(replace(:0,',','')) + 1
					)";

			StringBuilder sb = new StringBuilder();

			for (int i1 = 0; i1 < keyCount; i1++)
			{
				string sKey = keyList[firstKeyIdx + i1];
				if (Lex.IsNullOrEmpty(sKey)) continue;
				if (sb.Length > 0) sb.Append(",");
				sb.Append(sKey);
			}

			keyListString = sb.ToString();

			return sql;
		}

		/// <summary>
		/// Return true if all tables allow temp db tables to be used for key lists
		/// </summary>
		/// <param name="tables"></param>
		/// <returns></returns>

		bool AllowTempDbTableKeyListsForAllTables(
			List<QueryTable> tables)
		{
			if (DbCommandMx.DefaultKeyListPredType != KeyListPredTypeEnum.DbList) return false;

			foreach (QueryTable qt in tables)
			{
				if (!MetaBrokerUtil.AllowTempDbTableKeyLists(qt)) return false;
			}

			return true;
		}


		/// <summary>
		/// Return true if all of the supplied QueryTables support Netezza use
		/// </summary>
		/// <param name="tables"></param>
		/// <returns></returns>

		bool AllowNetezzaForAllTables(
			List<QueryTable> tables)
		{

			if (!QueryEngine.AllowNetezzaUse || !Query.AllowNetezzaUse) return false;

			foreach (QueryTable qt in tables)
			{
				MetaTable mt;
				if (!qt.AllowNetezzaUse) return false;
			}

			return true;
		}

		/// <summary>
		/// Return true if this query executes against oracle
		/// </summary>

		bool IsOracleQuery
		{
			get
			{
				foreach (QueryTable qt in Query.Tables)
				{
					MetaTable mt = qt.MetaTable;
					if (!IsOracleMetatable(mt)) return false;
				}

				return true;
			}
		}

		/// <summary>
		/// Return true if this metatable maps to an Oracle source 
		/// </summary>

		public static bool IsOracleMetatable(MetaTable mt)
		{
			bool isOracleTable = (MetaBrokerUtil.IsSqlMetaBroker(mt.MetaBrokerType) &&
													 !IsOdbcMetatable(mt));

			return isOracleTable;
		}

		/// <summary>
		/// Return true if this metatable maps to an ODBC source
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static bool IsOdbcMetatable(MetaTable mt)
		{
			if (DbConnectionMx.IsSqlFromOdbcSource(mt.TableMap))
				return true;

			else return false;

			//if (Lex.Contains(mt.TableMap, "dcsgtr") || Lex.Contains(mt.TableMap, "knimeivdr")) return true; // hack for now
			//   else return false;
		}

		/// <summary>
		/// Build the SQL for a query without executing it
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		public string BuildSqlStatements(Query q)
		{
			List<List<string>> sqlList = BuildSqlStatementList(q);

			StringBuilder sb = new StringBuilder();

			sb.Append("=============================================\r\n");
			sb.Append("SQL Statements for query ");
			UserObject uo = q.UserObject;
			if (uo != null && uo.Id > 0)
				sb.Append(uo.Id.ToString() + " (" + uo.Owner + "), " + uo.Name + "\r\n");

			else sb.Append(q.Name + "\r\n");
			sb.Append("=============================================\r\n");

			for (int li = 0; li < sqlList.Count; li++)
			{
				List<string> i = sqlList[li];
				if (i == null || i.Count == 0) continue;

				sb.Append("=============================================\r\n");
				sb.Append(i[0]);
				sb.Append("\r\n");

				if (i.Count < 2) continue;
				sb.Append("=============================================\r\n");
				sb.Append(i[1]);
				sb.Append("\r\n");
				sb.Append("\r\n");
			}

			return sb.ToString();
		}

		/// <summary>
		/// Build the SQL for a query without executing it
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>

		public List<List<string>> BuildSqlStatementList(
			Query query)
		{
			MetaTable rootTable = null;

			SqlList = new List<List<string>>();
			ExecuteQuery(query, true, false); // build SQL for search step without actual execution

			if (query.SingleStepExecution && DrivingTable != null && DrivingTable.MetaTable != null)
				rootTable = DrivingTable.MetaTable;

			else rootTable = RootTables[0]; // just a single root for now

			bool result = PrepareForRetrieval(rootTable, out KeysForRoot); // get sql for other tables to be retrieved from

			Dictionary<string, MultiTablePivotBrokerTypeData> mbsi = MetaBrokerStateInfo;

			for (int ti = 0; ti < Qtd.Length; ti++)
			{
				GenericMetaBroker mb = Qtd[ti].Broker as GenericMetaBroker;
				QueryTable qt = mb.Qt;
				MetaTable mt = qt.MetaTable;

				// Create broker label to appear in SqlList

				if (mb.PivotInCode)
				{
					if (!mb.IsKeyMultipivotBrokerInGroup) continue;

					string tableList = "";
					for (int ti2 = 0; ti2 < Qtd.Length; ti2++) // get list of tables
					{
						GenericMetaBroker mb2 = Qtd[ti2].Broker as GenericMetaBroker;
						if (!mb2.PivotInCode || mb2.MpGroupKey != mb.MpGroupKey) continue;
						if (tableList != "") tableList += ", ";
						tableList += mb2.Qt.MetaTable.Name;
					}

					mb.Label = "Retrieval SQL (Mobius-pivot-in-code) for metatable";
					if (tableList.Contains(",")) mb.Label += "s";
					mb.Label += ": " + tableList;
				}

				else
				{
					if (mt.MetaBrokerType == MetaBrokerType.Generic)
						mb.Label = "Retrieval SQL for metatable: " + mt.Name;

					else mb.Label = "Retrieval SQL for metatable: " + mt.Name;
				}

				// Create SQL for broker and store in SqlList via call to AddSqlToSqlStatementList

				mb.ExecuteQuery(Qtd[ti].Eqp);
			}

			return SqlList;
		}

		/// <summary>
		/// Add sql for query to list of total sql for query 
		/// </summary>
		/// <param name="mb"></param>
		/// <param name="sql"></param>

		internal void AddSqlToSqlStatementList(
			GenericMetaBroker mb,
			string sql)
		{
			string sqlWithDbLinks = sql;
			DbConnectionMx mxConn = DbConnectionMx.MapSqlToConnection(ref sqlWithDbLinks); // add any dblinks as necessary

			if (SqlList == null) SqlList = new List<List<string>>();
			List<string> sli = new List<string>();

			if (Lex.IsDefined(mb.Label))
				sli.Add(mb.Label);
			else sli.Add(mb.Qt.ActiveLabel + " (" + mb.Qt.MetaTable.Name + ")");

			string formattedSql = OracleDao.FormatSql(sqlWithDbLinks);
			sli.Add(formattedSql);

			SqlList.Add(sli);
			return;
		}

		/// <summary>
		/// Read the initial version of the base version of the specified Spotfire SQL
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static string ReadSpotfireSql(
			string name)
		{
			string spotfireSql = SpotfireDao.ReadSqlStatement(name, 0);
			return spotfireSql;
		}

		/// <summary>
		/// Read the initial version of the specified version of the named Spotfire SQL stmt
		/// </summary>
		/// <param name="name"></param>
		/// <param name="version"></param>
		/// <returns></returns>

		public static string ReadSpotfireSql(
			string name,
			int version)
		{
			string spotfireSql = SpotfireDao.ReadSqlStatement(name, version);
			return spotfireSql;
		}

		/// <summary>
		/// Build and save SQL for use in Spotfire information link
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>

		public static int SaveSpotfireSql(
			string sqlStmtName,
			Query query)
		{
			DbConnectionMx conn = null;

			string qtSql = "", sql, expr;
			string keys = null;
			string keyColName = "";
			string t1KeyColExpr = "", keyColExpr;

			string rootDataSource = "DEV857";
			string rootSchema = "MBS_OWNER";

			//if (query.Tables.Count != 1) throw new Exception("Can only save Spotfire Sql for single-table queries");

			Query q = query.Clone();

			// Clean up query before generating SQL

			q.KeyCriteria = q.KeyCriteriaDisplay = "";

			List<QueryTable> qtToRemove = new List<QueryTable>();
			foreach (QueryTable qt0 in q.Tables)
			{
				if (qt0.SelectedCount <= 1 ||
					!qt0.MetaTable.RetrievesDataFromQueryEngine)
				{
					qtToRemove.Add(qt0);
					continue;
				}

				foreach (QueryColumn qc0 in qt0.QueryColumns) // clear any criteria
				{
					if (Lex.IsDefined(qc0.Criteria))
						qc0.Criteria = qc0.CriteriaDisplay = "";
				}
			}

			foreach (QueryTable qt0 in qtToRemove)
			{
				q.RemoveQueryTable(qt0);
			}

			string selectList = ""; // top-level list of selected columns
			string fromList = ""; // sql for each QueryTable
			string joinCriteria = ""; // join criteria between QueryTables
			int remapCount = 0;

			q.AssignUndefinedAliases();

			q.MarkDuplicateNamesAndLabels();

			for (int ti = 0; ti < q.Tables.Count; ti++)
			{
				QueryTable qt = q.Tables[ti];
				if (ti == 0)
					keyColName = qt.MetaTable.KeyMetaColumn.Name;

				QueryEngine qe = new QueryEngine();
				ExecuteQueryParms eqp = new ExecuteQueryParms(qe, qt);
				eqp.ReturnQNsInFullDetail = false;

				qtSql = qe.BuildSqlForSingleTable(eqp);
				qtSql = Lex.Replace(qtSql, "/*+ hint */ ", "");

				conn = DbConnectionMx.MapSqlToConnection(ref qtSql, rootDataSource, rootSchema); // convert SQL to use dblinks from root source/schema
				if (conn == null)
					throw new Exception("Connection not found for: " + rootDataSource);

				// Recast numeric cols that are integers as integers for Spotfire

				List<DbColumnMetadata> cmdList = OracleDao.GetColumnMetadataFromSql(qtSql, conn);

				string qtSelectList = "";
				remapCount = 0; // number of cols remapped
				int sci = -1;

				foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (!qc.Selected) continue;
					sci++; // synch with cmdList

					MetaColumn mc = qc.MetaColumn;

					string mcName = mc.Name;
					if (q.Tables.Count > 1) // if more than one table qualify by table name
						mcName = qt.Alias + "." + mcName;

					string colName = qc.UniqueName;

					//if (mc.Name == "CORP_SBMSN_ID") mc = mc; // debug
					//if (mc.DataType == MetaColumnType.CompoundId) mc = mc; // debug

					if (mc.IsNumeric && (mc.DataType == MetaColumnType.Integer || mc.DataType == MetaColumnType.CompoundId))
					{
						DbColumnMetadata md = cmdList[sci];
						expr = "cast (" + mcName + " as integer) " + colName;  //  integer same as number(22,0)--- " as number(38, 0)) " + expr;
					}

					else if (mcName != colName)
					{
						expr = mcName + " " + colName;
						remapCount++;
					}

					else expr = mc.Name;

					if (qtSelectList != "") qtSelectList += ", ";
					qtSelectList += expr;
				}

				if (selectList != "") selectList += ", ";
				selectList += qtSelectList;

				if (fromList != "") fromList += ", ";
				fromList += "(" + qtSql + ") " + qt.Alias;

				keyColExpr = qt.Alias + "." + qt.KeyQueryColumn.MetaColumn.Name;

				if (ti == 0)
					t1KeyColExpr = keyColExpr;

				else
				{
					if (joinCriteria != "") joinCriteria += " and ";
					joinCriteria += keyColExpr + " (+) = " + t1KeyColExpr;
				}
			}

			selectList += " "; // be sure last col name in list is delimited with a space

			if (q.Tables.Count == 1 && remapCount == 0)
				sql = qtSql; // simple single table with no remapping of cols

			else // combine list of elements
			{
				sql =
					"select " + selectList +
					" from " + fromList;

				if (joinCriteria != "")
					sql += " where " + joinCriteria;

				sql = "select * from (" + sql + ")";  // encapsulate the SQL
			}

			int v2 = SpotfireDao.InsertSpotfireSql(sqlStmtName, 0, sql, keyColName, null, Security.UserName);
			return v2;
		}

		/// <summary>
		/// Save a list of keys for later inclusion in SQL statements
		/// </summary>
		/// <param name="listType">CIDLIST, ASSAYLIST</param>
		/// <param name="keyList"></param>
		/// <returns></returns>

		public static string SaveSpotfireKeyList(
			string keyColName,
			string listType,
			string keyList)
		{
			string subList;
			string listName = listType + "_"; // start building name
			int v1 = -2; // modify name with next version

			while (true) // break into 4000 char chunks for later use by Oracle function
			{
				if (keyList.Length < 4000)
				{
					subList = keyList;
					keyList = "";
				}
				else
				{
					subList = keyList.Substring(0, 4000);
					int i1 = subList.LastIndexOf(",");
					subList = subList.Substring(0, i1);
					keyList = keyList.Substring(i1 + 1);
				}

				int v2 = SpotfireDao.InsertSpotfireSql(listName, v1, null, keyColName, subList, Security.UserName);
				if (v1 == -2) // first time?
				{
					listName += v2; // all sublists will have this list name
					v1 = -1;
				}

				if (keyList.Length == 0) break;
			} 

			return listName;
		}
		
	} // end of QueryEngine class

} // end of namespace Mobius.QueryEngineLibrary

