using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;
using Mobius.CdkMx;
using Mobius.CdkSearchMx;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Drawing;

namespace Mobius.QueryEngineLibrary
{

/// <summary>
/// Broker to handle special structure searches
/// </summary>

	public class MoleculeMetaBroker : GenericMetaBroker
	{
		QueryColumn KeyCriteriaQc; // key column that has criteria
		ParsedSingleCriteria Pkc; // parsed key criteria

		QueryColumn StructCriteriaQc; // structure column that has criteria
		ParsedStructureCriteria Pssc; // parsed structure criteria

		CdkSimSearchMx Ecfp4Dao = null; // data access object for CDK ECFP4 similarity search
		SmallWorldDao SwDao = null; // data access object for SmallWorld Search
		RelatedStructureSearch RSS = null; // manages related structure searches

		List<object[]> Results; // retrieved results
		int CursorPos = -1; // index of current row
		int VoLength = -1; // length of vos for results

		static MoleculeMetaBroker LastSwNonSqlBroker = null; // keep previous dao to properly handle two-step search & retrieve
		public static bool CacheBitmaps = false; // turn caching on or off
		public static Dictionary<string, Bitmap> BitmapCache = null; // dict of retrieved bitmaps

		public static bool Debug = false;

		/// <summary>
		/// Check if criteria doesn't fit into standard single Oracle select statement
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="tokens"></param>
		/// <param name="tki"></param>
		/// <returns></returns>

		public static bool IsNonCartridgeStructureSearchCriteria(
			QueryColumn qc)
		{
			if (CanExecuteInternalStructureSearch(qc)) // do structure search via something other than Oracle?
				return true;

			//else if (!QueryEngine.IsOracleMetatable(qc.QueryTable.MetaTable)) // non-Oracle table (e.g. ODBC)
			//	return true;

			else return false;
		}

/// <summary>
/// Return true if associated query is a valid non-Sql search
/// </summary>
/// <param name="eqp"></param>
/// <returns></returns>

		public static bool IsNonSqlStructureSearchQueryTable(
			ExecuteQueryParms eqp)
		{
			if (Lex.IsDefined(eqp.CallerSuppliedCriteria)) return false; // return false if retrieval step of query

			QueryColumn strQc = eqp.QueryTable.FirstStructureQueryColumn;
			if (strQc == null || strQc.Criteria == "") return false;

			bool result = CanExecuteInternalStructureSearch(strQc);
			return result;
		}

		/// <summary>
		/// Return true if structure column can be searched by retrieving structure and matching with method call
		/// </summary>
		/// <param name="strQc"></param>
		/// <returns></returns>

		public static bool CanExecuteInternalStructureSearch(
			QueryColumn strQc)
		{
			ParsedStructureCriteria psc = null;

			MetaColumn strMc = strQc.MetaColumn;
			if (strMc.DataType != MetaColumnType.Structure)
				return false; // only some structure searches are non-oracle now

			MetaTable mt = strMc.MetaTable;

			bool parseOk = ParsedStructureCriteria.TryParse(strQc, out psc);

			if (parseOk && psc.SearchType == StructureSearchType.MolSim && psc.SimilarityType == SimilaritySearchType.ECFP4) // ECFP4 search
				return true; 

			else if (parseOk && psc.SearchType == StructureSearchType.SmallWorld) // smallworld executed via service calls
				return true;

			else if (parseOk && psc.SearchType == StructureSearchType.Related) // related structure search
				return true;

			else if (mt.MetaBrokerType == MetaBrokerType.Annotation) // annotation table
				return true;

			else if (Lex.StartsWith(mt.Name, "USERDATABASE_STRUCTURE_")) // structure database
				return true;

			else if (Lex.Eq(strMc.DataTransform, "FromMolFile") || // other table with structure stored in usable format
				Lex.Eq(strMc.DataTransform, "FromChime") ||
				Lex.Eq(strMc.DataTransform, "FromSmiles"))
				return true;

			else return false;
		}

		/// <summary>
		/// Prep non-Oracle query
		/// </summary>
		/// <param name="eqp"></param>
		/// <returns></returns>

		public override string PrepareQuery(
			ExecuteQueryParms eqp)
		{
			int cti;

			Eqp = eqp;
			QueryEngine qe = eqp.Qe;
			Query q = qe.Query;
			QueryTable qt = eqp.QueryTable;
			QueryColumn qc;
			MetaTable mt = qt.MetaTable;
			MetaColumn mc = null;

			StructCriteriaQc = KeyCriteriaQc = null;

			QueryColumn strQc = qt.FirstStructureQueryColumn;
			if (strQc != null && Lex.IsDefined(strQc.Criteria))
				StructCriteriaQc = strQc;
			else throw new Exception("Structure criteria not defined");

			Pssc = ParsedStructureCriteria.Parse(StructCriteriaQc);

			QueryColumn keyQc = qt.KeyQueryColumn;
			if (keyQc != null && Lex.IsDefined(q.KeyCriteria)) //keyQc.Criteria))
				KeyCriteriaQc = keyQc;

			if (StructCriteriaQc==null &&
				KeyCriteriaQc == null &&
				eqp.SearchKeySubset==null)
				throw new Exception("NonSqlBroker - No criteria specified");

			SelectList = new List<MetaColumn>(); // list of selected metacolumns
			foreach (QueryColumn qc2 in qt.QueryColumns)
			{
				if (qc2.MetaColumn == null) continue; // in case metacolumn not defined
				if (qc2.IsKey) qc2.Selected = true;
				if (qc2.Selected || qc2.SortOrder != 0) SelectList.Add(qc2.MetaColumn);
			}

			// Setup for ECFP4 similarity search

			if (Pssc.SearchType == StructureSearchType.MolSim && Pssc.SimilarityType == SimilaritySearchType.ECFP4)
			{
				Ecfp4Dao = null; // reset Dao to initiate new search
				return "";
			}

			else if (Pssc.SearchType == StructureSearchType.SmallWorld) // SmallWorld search
			{
				SwDao = null; // reset Dao to initiate new search
				return "";
			}

			else if (Pssc.SearchType == StructureSearchType.Related) // Related structure search
			{
				RSS = null; // reset to initiate new search
				return "";
			}

			else if (!MqlUtil.IsCartridgeMetaTable(mt)) // must be non chemical cartridge table (e.g. User structure DB or structure in Annotation table
				return ""; // everything looks ok, query criteria stored here, no sql returned

			else throw new Exception("Unsupported NonSqlBroker search for table: " + eqp.QueryTable.MetaTable.Label);
		}

/// <summary>
/// Execute non-Oracle query
/// </summary>
/// <param name="eqp"></param>

public override void ExecuteQuery(
			ExecuteQueryParms eqp)
		{
			QueryTable qt = Eqp.QueryTable;
			MetaTable mt = qt.MetaTable;

			if (Pssc.SearchType == StructureSearchType.MolSim && Pssc.SimilarityType == SimilaritySearchType.ECFP4)
				ExecuteECFP4Search(eqp);

			else if (Pssc.SearchType == StructureSearchType.SmallWorld)
				ExecuteSmallWorldSearch(eqp);

			else if (Pssc.SearchType == StructureSearchType.Related)
				ExecuteRelatedStructureSearch(eqp);

			else if (mt.MetaBrokerType == MetaBrokerType.Annotation)
				ExecuteAnnotationStructureSearch();

			else if (Lex.StartsWith(mt.Name, "USERDATABASE_STRUCTURE_"))
				ExecuteUserDatabaseStructureSearch();

			else ExecuteInternalOracleStructureColumnSearch(); // internal search of structures stored in general oracle table column

			return;
		}

		/// <summary>
		/// Execute CDK ECFP4 search
		/// </summary>
		/// <param name="eqp"></param>

		void ExecuteECFP4Search(
			ExecuteQueryParms eqp)
		{
			CdkSimSearchMx dao = Ecfp4Dao;

			if (!CdkSimSearchMx.IsSearchingAvailable())
				throw new UserQueryException("ECFP4 searching is not currently available.");

			Query q = eqp.QueryTable.Query;
			QueryTable qt = eqp.QueryTable;
			MetaTable mt = qt.MetaTable;

			if (dao == null) // if Dao not allocated then allocate Dao & execute the search
			{
				dao = Ecfp4Dao = new CdkSimSearchMx();
				dao.KeysToExclude = q.KeysToExclude;

				if (eqp.SearchKeySubset != null)
					dao.SearchKeySubset = new HashSet<string>(eqp.SearchKeySubset);

				string databases = mt.Name + "," +  mt.Label; // quick/dirty DB Id

				List<StructSearchMatch> hits = dao.ExecuteSearch(Pssc.Molecule.GetMolfileString(), databases, FingerprintType.Circular, Pssc.MinimumSimilarity, Pssc.MaxSimHits);
				VoLength = 1;
				if (qt.GetSelectedMolsimQueryColumn() != null)
					VoLength = 2;

			}

			BuildECFP4QueryEngineRows(eqp); // build the QE row set (filtered) 

			return;
		}

		/// <summary>
		/// Build the set of results rows based on the current key set
		/// </summary>
		/// <returns></returns>

		void BuildECFP4QueryEngineRows(
			ExecuteQueryParms eqp)
		{
			HashSet<string> keySet = new HashSet<string>();
			if (eqp.SearchKeySubset != null) // subsetting
			{
				foreach (string cid0 in eqp.SearchKeySubset)
				{
					keySet.Add(cid0);
				}
			}

			List<StructSearchMatch> recs = Ecfp4Dao.MatchList; // start with full match list

			Results = new List<object[]>(); // destination results list (may or may not be subsetted)

			for (int ri = 0; ri < recs.Count; ri++)
			{
				StructSearchMatch m = recs[ri];

				if (keySet != null && keySet.Count > 0 && Lex.IsDefined(m.SrcCid))
				{
					if (!keySet.Contains(m.SrcCid)) continue;
				}

				object[] voa = new object[VoLength];
				voa[0] = m.SrcCid;
				if (VoLength > 1)
					voa[1] = m.MatchScore;

				Results.Add(voa);
			}

			CursorPos = -1;
			return;
		}


		/// <summary>
		/// Execute SmallWorld search
		/// </summary>
		/// <param name="eqp"></param>

		void ExecuteSmallWorldSearch(
			ExecuteQueryParms eqp)
		{
			if (!SmallWorldDao.IsSmallWorldAvailable())
				throw new UserQueryException("SmallWorld searching is not currently available.");

			Query q = eqp.QueryTable.Query;
			QueryTable qt = eqp.QueryTable;

			SmallWorldPredefinedParameters swp = Pssc.SmallWorldParameters;
			string smiles = Pssc.Molecule.GetSmilesString();
			swp.Smiles = smiles;
			//swp.Database = "Corp"; 

			LastSwNonSqlBroker = null; // debug - force new broker

			if (SwDao == null) // if Dao not allocated then allocate & execute the search
			{
				if (LastSwNonSqlBroker != null && LastSwNonSqlBroker.Pssc != null && LastSwNonSqlBroker.Pssc.SmallWorldParameters != null &&
					LastSwNonSqlBroker.Pssc.SmallWorldParameters.Serialize() == swp.Serialize())
				{
					SwDao = LastSwNonSqlBroker.SwDao; // get rows from previous search
					SwDao.BuildSwToQeColumnMap(SelectList); // reset the column map for this broker instance
				}

				else
				{
					SwDao = new SmallWorldDao();
					SwDao.KeysToExclude = q.KeysToExclude;
					SwDao.ExecuteSearch(swp);
					SwDao.BuildSwToQeColumnMap(SelectList); // 
					LastSwNonSqlBroker = this; // remember for future use
				}

			}

			BuildSmallWorldQueryEngineRows(eqp); // build the QE row set (filtered) from the SW rowset

			return;
		}

		/// <summary>
		/// Build the set of results rows based on the current key set
		/// </summary>
		/// <returns></returns>

		void BuildSmallWorldQueryEngineRows(
			ExecuteQueryParms eqp)
		{
			HashSet<string> keySet = new HashSet<string>();
			if (eqp.SearchKeySubset != null) // subsetting
			{
				foreach (string cid0 in eqp.SearchKeySubset)
				{
					keySet.Add(cid0);
				}
			}

			List<SmallWorldMatch> recs = SwDao.MatchList; // start with full match list

			Results = new List<object[]>(); // destination results list (may or may not be subsetted)

			for (int ri = 0; ri < recs.Count; ri++)
			{
				SmallWorldMatch m = recs[ri];

				if (keySet != null && keySet.Count > 0 && Lex.IsDefined(m.Cid))
				{
					if (!keySet.Contains(m.Cid)) continue;
				}

				object[] voa = m.BuildVoa(SwDao, ri);
				Results.Add(voa);
			}

			CursorPos = -1;
			return;
		}

		/// <summary>
		/// Execute Related Structure Search
		/// </summary>
		/// <param name="eqp"></param>

		void ExecuteRelatedStructureSearch(
			ExecuteQueryParms eqp)
		{
			if (RSS == null)
			{
				RSS = new RelatedStructureSearch();

				Query q = StructCriteriaQc.QueryTable.Query;
				QueryTable qt = StructCriteriaQc.QueryTable;
				MetaTable mt = qt.MetaTable;
				QueryColumn qc = StructCriteriaQc;
				MetaColumn mc = qc.MetaColumn;

				RSS.QueryMtName = mt.Name;

				RSS.QueryChimeString = Pssc.Molecule.GetChimeString(); // if no cid in options use passed structure

				RSS.IncludeQueryStructure = true;

				RSS.SearchCorp = (Lex.Contains(mt.Name, "Corp"));
				RSS.SearchChembl = (Lex.Contains(mt.Name, "ChEMBL"));

				RSS.SearchFSS = SST.IsFull(Pssc.SearchTypeUnion);
				RSS.SearchMmp = SST.IsMmp(Pssc.SearchTypeUnion);
				RSS.SearchSmallWorld = SST.IsSw(Pssc.SearchTypeUnion);
				RSS.SearchSim = SST.IsSim(Pssc.SearchTypeUnion);
				RSS.SearchSSS = SST.IsSSS(Pssc.SearchTypeUnion);

				if (RSS.SearchMmp && Lex.IsUndefined(RSS.QueryCid)) // try to get a cid from the structure if we are searching MMP and don't already have one
				{
					int corpId = Pssc.Molecule.GetCompoundId();
					if (corpId > 0) RSS.QueryCid = corpId.ToString();
				}

				RSS.KeysToExclude = q.KeysToExclude;

				RSS.ExecuteSearch();
			}

			BuildRelatedStructureSearchQueryEngineRows(eqp);

			return;
		}

/// <summary>
/// Build the set of results rows based on the current key set
/// </summary>
/// <param name="eqp"></param>

		void BuildRelatedStructureSearchQueryEngineRows(
			ExecuteQueryParms eqp)
		{
			HashSet<string> keySet = new HashSet<string>();
			if (eqp.SearchKeySubset != null) // subsetting
			{
				foreach (string cid0 in eqp.SearchKeySubset)
				{
					keySet.Add(cid0);
				}
			}

			List<StructSearchMatch> recs = RSS.AllMatches; // start with full match list

			Results = new List<object[]>(); // destination results list (may or may not be subsetted)

			for (int ri = 0; ri < recs.Count; ri++)
			{
				StructSearchMatch m = recs[ri];

				if (keySet != null && keySet.Count > 0 && Lex.IsDefined(m.SrcCid))
				{
					if (!keySet.Contains(m.SrcCid)) continue;
				}

				if (RestrictedDatabaseView.KeyIsRetricted(m.SrcCid)) continue;

				object[] voa = new object[2];
				voa[0] = m.SrcCid;
				voa[1] = m;
				Results.Add(voa);
			}

			CursorPos = -1;
			return;
		}

		/// <summary>
		/// Execute structure search on user database table
		/// </summary>
		/// <param name="eqp"></param>

		void ExecuteUserDatabaseStructureSearch()
		{
			SelectList = new List<MetaColumn>();
			foreach (QueryColumn qc2 in Eqp.QueryTable.QueryColumns)
				if (qc2.Selected || qc2.SortOrder != 0) SelectList.Add(qc2.MetaColumn);

			MetaColumn keyMc = Eqp.QueryTable.MetaTable.KeyMetaColumn;
			MetaColumn strMc = Eqp.QueryTable.MetaTable.FirstStructureMetaColumn;

			string sql =
				"select " + keyMc.Name + ", " + strMc.Name + " " + // select key & struct col by name
			  "from (" + // inner select to get key & structure
				" select ext_cmpnd_id_txt " + keyMc.Name + ", mlcl_struct " + strMc.Name +
				" from ucdb_owner.ucdb_db_cmpnd lc, ucdb_owner.ucdb_cmpnd c " +
				" where db_id = " + Eqp.QueryTable.MetaTable.Code +
				"  and lc.cmpnd_id = c.cmpnd_id " +
				"  and c.pndng_sts <> 4)"; // skip compounds pending deletion (UcdbWaitState.Deletion)

			ExecuteInternalOracleStructureColumnSearch(sql, MoleculeFormat.Unknown);
			return;
		}

/// <summary>
/// Execute structure search on annotation table
/// </summary>
/// <param name="eqp"></param>

		void ExecuteAnnotationStructureSearch()
		{
			SelectList = new List<MetaColumn>();
			foreach (QueryColumn qc2 in Eqp.QueryTable.QueryColumns)
				if (qc2.Selected || qc2.SortOrder != 0) SelectList.Add(qc2.MetaColumn);

			MetaColumn keyMc = Eqp.QueryTable.MetaTable.KeyMetaColumn;
			MetaColumn strMc = Eqp.QueryTable.MetaTable.FirstStructureMetaColumn;

			string sql = // select key column & col containing structure
				"select " + keyMc.Name + ", " + strMc.Name + " " + // select key & struct col by name
				"from (" + // inner select to get key & structure
				" select " + keyMc.ColumnMap + " " + keyMc.Name + ", rslt_val_txt " + strMc.Name +
				" from mbs_owner.mbs_adw_rslt " +
				" where mthd_vrsn_id = " + Eqp.QueryTable.MetaTable.Code +
				"  and rslt_typ_id = " + Pssc.QueryColumn.MetaColumn.ResultCode +
				"  and sts_id = 1)";

			ExecuteInternalOracleStructureColumnSearch(sql, MoleculeFormat.Unknown);
			return;
		}

/// <summary>
/// Execute structure search for structures stored in an general Oracle table text column
/// </summary>

		void ExecuteInternalOracleStructureColumnSearch()
		{
			SelectList = new List<MetaColumn>();
			QueryColumn scQc = null;
			MoleculeFormat structureFormat = MoleculeFormat.Unknown;

			MetaColumn keyMc = Eqp.QueryTable.MetaTable.KeyMetaColumn;
			MetaColumn strMc = null;

			foreach (QueryColumn qc2 in Eqp.QueryTable.QueryColumns)
			{
				if (qc2.Selected || qc2.SortOrder != 0) SelectList.Add(qc2.MetaColumn);
				if (qc2.Criteria == "") continue;
				if (qc2.MetaColumn.DataType == MetaColumnType.Structure)
				{
					strMc = qc2.MetaColumn;
					if (Lex.Eq(strMc.DataTransform, "FromMolFile"))
						structureFormat = MoleculeFormat.Molfile;
					else if (Lex.Eq(strMc.DataTransform, "FromChime"))
						structureFormat = MoleculeFormat.Chime;
					else if (Lex.Eq(strMc.DataTransform, "FromSmiles"))
						structureFormat = MoleculeFormat.Smiles;
					break;
				}
				else {} // allow criteria on other cols?
			}

			if (strMc == null) throw new Exception("Structure criteria not found");

			string sql = 
				"select " + keyMc.Name +	", " + strMc.Name + " " +
				"from " + Eqp.QueryTable.MetaTable.TableMap;

			ExecuteInternalOracleStructureColumnSearch(sql, structureFormat);
			return;
		}

/// <summary>
/// Execute structure search for structures stored in an Oracle text column
/// </summary>
/// <param name="sql">Basic sql to select key & structure</param>

		public void ExecuteInternalOracleStructureColumnSearch(
			string sql,
			MoleculeFormat structureFormat)
		{
			string cid, molString;
			object[] vo;
			MoleculeMx cs;
			bool match = false;

			QueryColumn molsimQc = Eqp.QueryTable.GetSelectedMolsimQueryColumn(); // get any column to return similarity score in

			DbCommandMx drd = new DbCommandMx();
			if (Eqp.SearchKeySubset == null)
			{
				drd.Prepare(sql);
				drd.ExecuteReader();
			}

			else // limit to list
			{
				sql += " where " + Eqp.QueryTable.MetaTable.KeyMetaColumn.Name + " in (<list>)";
				drd.PrepareListReader(sql, DbType.String);

				List<string> dbKeySubset = new List<string>();
				foreach (string key in Eqp.SearchKeySubset)
				{
					string dbKey = CompoundId.NormalizeForDatabase(key, Eqp.QueryTable.MetaTable);
					dbKeySubset.Add(dbKey);
				}
				drd.ExecuteListReader(dbKeySubset);
			}

			drd.CheckForCancel = Eqp.CheckForCancel; // set cancel check flag

			StructureMatcher matcher = new StructureMatcher(); // allocate structure matcher
			List<object[]> results = new List<object[]>();

			int matchCount = 0;
			while (drd.Read())
			{
				cid = drd.GetObject(0).ToString();
				cid = CompoundId.Normalize(cid, Eqp.QueryTable.MetaTable); // normalize adding prefix as needed

				molString = drd.GetString(1);
				if (String.IsNullOrEmpty(molString)) continue;

				cs = new MoleculeMx(structureFormat, molString);
				if (Pssc.SearchType == StructureSearchType.Substructure)
				{
					if (matcher.IsSSSMatch(Pssc.Molecule, cs))
					{
						vo = new object[SelectList.Count];
						vo[0] = CompoundId.Normalize(cid, Eqp.QueryTable.MetaTable); // normalize adding prefix
						results.Add(vo);
					}
				}

				else if (Pssc.SearchType == StructureSearchType.MolSim)
				{
					double score = matcher.Molsim(Pssc.Molecule, cs, Pssc.SimilarityType);
					if (score >= Pssc.MinimumSimilarity)
					{
						vo = new object[SelectList.Count];
						vo[0] = CompoundId.Normalize(cid, Eqp.QueryTable.MetaTable); // normalize adding prefix
						if (vo.Length >= 2)
							vo[1] = (float)score;

						results.Add(vo);
					}
				}

				else if (Pssc.SearchType == StructureSearchType.FullStructure)
				{
					if (matcher.FullStructureMatch(Pssc.Molecule, cs, Pssc.Options))
					{
						vo = new object[SelectList.Count];
						vo[0] = CompoundId.Normalize(cid, Eqp.QueryTable.MetaTable); // normalize adding prefix
						results.Add(vo);
					}
				}
				matchCount++;

//				if (matchCount >100) break; // debug

				if (drd.Cancelled)
				{
					Eqp.Qe.Cancelled = true;
					drd.Dispose();
					Results = null;
					return;
				}
			}

			drd.Dispose();

			Results = results;
			return;
		}

/// <summary>
/// Get next row for non-Oracle query
/// </summary>
/// <returns></returns>

		public override Object[] NextRow()
		{
			while (true)
			{
				if (CursorPos >= Results.Count - 1) return null;
				CursorPos++;

				object[] voa = Results[CursorPos];

				HashSet<string> keysToExclude = null;
				if (Eqp != null && Eqp.QueryTable != null && Eqp.QueryTable.Query != null)
					keysToExclude = Eqp.QueryTable.Query.KeysToExclude;

				if (keysToExclude != null && keysToExclude.Count > 0 && // remove any keys to be excluded from keySubset
					voa != null && voa.Length > 0)
				{
					string key = voa[0] as string; // assume key is in first position
					if (Lex.IsDefined(key) && keysToExclude.Contains(key))
					{
						ReadRowsFilteredByKeyExclusionList++;
						continue;
					}
				}

				return voa;
			}
		}

		/// <summary>
		/// Get additional data from the broker
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>

		public override object GetAdditionalData(
			string command)
		{
			if (!SmallWorldDao.SmallWorldDepictionsAvailable) return null;

			if (SwDao != null && SwDao.Swp != null)
			{

				string cmdName, mcName, colorArg, alignArg;
				bool color, align;
				SmallWorldPredefinedParameters swp = SwDao.Swp;

				Lex.Split(command, " ", out cmdName, out mcName, out colorArg, out alignArg);

				if (Lex.Ne(cmdName, "GetDepictions") ||
				 !bool.TryParse(colorArg, out color) ||
				 !bool.TryParse(alignArg, out align))
					return null;

				string dls = SwDao.GetDepictionsAsMultiRecordText(color, align);
				return dls;
			}

			else throw new Exception("GetAdditionalData failed for: " + command);
		}

		/// <summary>
		/// Get structure image with hilighting from a result link
		/// </summary>
		/// <param name="mc"></param>
		/// <param name="resultLink"></param>
		/// <param name="width"></param>
		/// <returns></returns>

		public override Bitmap GetImage(
			MetaColumn mc,
			string resultLink,
			int width)
		{
			string hls, rs;
			int hitListId, rowId;
			Lex.Split(resultLink, ",",  out hls, out rs);
			int.TryParse(hls, out hitListId);
			int.TryParse(rs, out rowId);

			DateTime t0 = DateTime.Now;

			if (SwDao != null && SwDao.Swp != null)
			{
				bool color = SwDao.Swp.Highlight;
				bool align = SwDao.Swp.Align;

				string svgXml = SwDao.GetDepiction(hitListId, rowId, color, align);
				Bitmap bm = SvgUtil.GetBitmapFromSvgXml(svgXml, width);

				if (bm != null && BitmapCache != null)
					BitmapCache[resultLink] = bm; // cache the bitmap

				return bm;
			}

			else throw new Exception("Failed to get image for " + mc.MetaTable.Name + "." + mc.Name);
		}

		/// <summary>
		/// Check cache for image
		/// </summary>
		/// <param name="matchKey"></param>
		/// <returns></returns>

		Bitmap CheckCache(string matchKey)
		{
			if (!CacheBitmaps) return null;

			if (BitmapCache == null) BitmapCache = new Dictionary<string, Bitmap>();
			if (BitmapCache.ContainsKey(matchKey))
			{
				//DebugLog.Message("Returning cached bitmap: " + resultId);
				return BitmapCache[matchKey];
			}

			else return null;
		}

		/// <summary>
		/// Try to find a search-step non SQL broker associated with the specified metatable
		/// </summary>
		/// <param name="rootTableName"></param>
		/// <returns></returns>

		public static MoleculeMetaBroker GetSearchStepMoleculeBroker(
			QueryEngine qe,
			string rootTableName)
		{
			if (qe.SearchBrokerList == null) return null;

			foreach (GenericMetaBroker mb0 in qe.SearchBrokerList)
			{
				if (Lex.Eq(mb0.Qt.MetaTable.Name, rootTableName) && mb0?.MoleculeMetaBroker != null)
					return mb0?.MoleculeMetaBroker;
			}

			return null;
		}

		/// <summary>
		/// Close non-Oracle query
		/// </summary>

		public override void Close()
		{
			return; // nothing to do here yet
		}


	}
}
