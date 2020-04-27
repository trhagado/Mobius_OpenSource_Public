using Mobius.ComOps;
using Mobius.Data;

using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace Mobius.UAL
{
	/// <summary>
	/// Ucdb selection, update, delete
	/// </summary>

	public partial class UserCmpndDbDao
	{

		/// <summary>
		/// Datatables containing column defs for each table
		/// </summary>

		static DataTable UcdbDatabaseModelTable = null;
		static DataTable UcdbDatabaseCompoundDataTable = null;
		static DataTable UcdbCompoundDataTable = null;
		static DataTable UcdbAliasDataTable = null;
		static DataTable UcdbDatabaseModelDataTable = null;
		static DataTable SpmResultTable = null;
		static DataTable SpmCompoundTable = null;

		LogFile Log; // used for logging messages

		/// <summary>
		/// Default constructor
		/// </summary>

		public UserCmpndDbDao()
		{
			string logFileName = ServicesDirs.LogDir + @"\UserDataLog.log";
			Log = new Mobius.ComOps.LogFile(logFileName);

			//SetModelCalcParameters();
		}

		/// <summary>
		/// Create with log file name
		/// </summary>
		/// <param name="logFileName"></param>

		public UserCmpndDbDao(
			string logFileName)
		{
			if (String.IsNullOrEmpty(Path.GetDirectoryName(logFileName)))
				logFileName = ServicesDirs.LogDir + @"\" + logFileName;

			Log = new Mobius.ComOps.LogFile(logFileName);

			//SetModelCalcParameters();
		}

		/// <summary>
		/// Set any paramters from .ini file for model building
		/// </summary>
		/// <param name="udbs"></param>
#if false
		public void SetModelCalcParameters()
		{
			string tok = ServicesIniFile.Read("ModelCalcCmpndChunkSize"); // see if number of compounds to run is specified
			if (tok != "") ModelCalcCmpndChunkSize = Int32.Parse(tok);

			tok = ServicesIniFile.Read("ModelUpdateIsRunningTimeLimit"); // time limit to check if update is still running
			if (tok != "") ModelUpdateIsRunningTimeLimit = Int32.Parse(tok);
	}
#endif

		/// <summary>
		/// Select all database headers
		/// </summary>
		/// <returns></returns>

		public UcdbDatabase[] SelectDatabaseHeaders()
		{
			DataTable dt = GetUcdbDatabaseDataTable();
			string sql =
				"select " + DbCommandMx.GetFieldListString(dt) + " " +
				"from ucdb_owner.ucdb_db " +
				"order by lower(crtr_usr_nm), lower(db_nm)";

			DbCommandMx drd = new DbCommandMx();
			drd.Prepare(sql);
			drd.ExecuteReader();

			List<UcdbDatabase> dbList = new List<UcdbDatabase>();
			while (drd.Read())
			{
				UcdbDatabase ucdb = ReadDatabaseRow(drd);
				dbList.Add(ucdb);
			}

			drd.Dispose();
			UcdbDatabase[] dbArray = dbList.ToArray();
			return dbArray;
		}

		/// <summary>
		/// Select the header information for all of the databases owned by a user
		/// </summary>
		/// <param name="ownerUserName"></param>
		/// <returns></returns>

		public UcdbDatabase[] SelectDatabaseHeaders(
			string ownerUserName)
		{
			DataTable dt = GetUcdbDatabaseDataTable();
			string sql =
				"select " + DbCommandMx.GetFieldListString(dt) + " " +
				"from ucdb_owner.ucdb_db " +
				"where lower(crtr_usr_nm) = lower(:0) " +
				"order by lower(db_nm)";

			DbCommandMx drd = new DbCommandMx();
			drd.PrepareMultipleParameter(sql, 1);
			drd.ExecuteReader(ownerUserName);

			List<UcdbDatabase> dbList = new List<UcdbDatabase>();
			while (drd.Read())
			{
				UcdbDatabase ucdb = ReadDatabaseRow(drd);
				dbList.Add(ucdb);
			}

			drd.Dispose();
			UcdbDatabase[] dbArray = dbList.ToArray();
			return dbArray;
		}

		/// <summary>
		/// Select all data for a database
		/// </summary>
		/// <param name="databaseId"></param>
		/// <returns></returns>

		public UcdbDatabase SelectDatabase(
			long databaseId)
		{
			UcdbDatabase ucdb = SelectDatabaseHeader(databaseId);
			ucdb.Compounds = SelectDatabaseCompounds(databaseId);
			ucdb.Models = SelectDatabaseModels(databaseId);
			return ucdb;
		}

		/// <summary>
		/// Read database header for a database id
		/// </summary>
		/// <param name="databaseId"></param>
		/// <returns></returns>

		public UcdbDatabase SelectDatabaseHeader(
			long databaseId)
		{
			DataTable dt = GetUcdbDatabaseDataTable();
			string sql =
				"select " + DbCommandMx.GetFieldListString(dt) + " " +
				"from ucdb_owner.ucdb_db " +
				"where db_id = :0";

			DbCommandMx drd = new DbCommandMx();
			drd.PrepareMultipleParameter(sql, 1);
			drd.ExecuteReader(databaseId.ToString());

			if (!drd.Read())
			{
				drd.Dispose();
				throw new Exception("Database not found");
			}

			UcdbDatabase ucdb = ReadDatabaseRow(drd);
			drd.Dispose();
			return ucdb;
		}

		/// <summary>
		/// Select header for database based on owner and database name
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="databaseNameSpace"></param>
		/// <param name="databaseName"></param>
		/// <returns></returns>

		public UcdbDatabase SelectDatabaseHeader(
			string userName,
			string databaseNameSpace,
			string databaseName)
		{
			DataTable dt = GetUcdbDatabaseDataTable();
			string sql =
				"select " + DbCommandMx.GetFieldListString(dt) + " " +
				"from ucdb_owner.ucdb_db " +
				"where lower(crtr_usr_nm) = lower(:0) and " +
				"lower(db_nm_space) = lower(:1) and " +
				"lower(db_nm) = lower(:2)";

			DbCommandMx drd = new DbCommandMx();
			drd.PrepareMultipleParameter(sql, 3);
			object[] oa = new object[3];
			oa[0] = userName;
			oa[1] = databaseNameSpace;
			oa[2] = databaseName;

			drd.ExecuteReader(oa);

			if (!drd.Read())
			{
				drd.Dispose();
				return null;
			}

			UcdbDatabase ucdb = ReadDatabaseRow(drd);
			drd.Dispose();
			return ucdb;
		}

		/// <summary>
		/// Read database header row into UcdbDatabase instance for current DataReaderDao position
		/// </summary>
		/// <param name="drd"></param>
		/// <returns></returns>

		public UcdbDatabase ReadDatabaseRow(
			DbCommandMx drd)
		{
			UcdbDatabase ucdb = new UcdbDatabase();
			ucdb.DatabaseId = drd.GetLong(0);
			ucdb.NameSpace = drd.GetString(1);
			ucdb.Name = drd.GetString(2);
			ucdb.Description = drd.GetString(3);
			ucdb.OwnerUserName = drd.GetString(4);
			ucdb.Public = drd.GetInt(5) == 2 ? true : false;
			ucdb.CompoundCount = drd.GetInt(6);
			ucdb.ModelCount = drd.GetInt(7);
			ucdb.StructureSearchSupported = drd.GetString(8) == "T" ? true : false;
			ucdb.AllowDuplicateStructures = drd.GetString(9) == "T" ? true : false;
			ucdb.CompoundIdType = (CompoundIdTypeEnum)drd.GetInt(10);
			ucdb.PendingStatus = (UcdbWaitState)drd.GetInt(11);
			ucdb.PendingCompoundCount = drd.GetInt(12);
			ucdb.PendingCompoundId = drd.GetLong(13);
			ucdb.PendingUpdateDate = drd.GetDateTime(14);
			ucdb.CreationDate = drd.GetDateTime(15);
			ucdb.UpdateDate = drd.GetDateTime(16);
			return ucdb;
		}

		/// <summary>
		/// Select database compound data for a compoundId
		/// </summary>
		/// <param name="compoundId"></param>
		/// <returns></returns>

		public UcdbCompound SelectDatabaseCompound(
			long compoundId)
		{
			string criteria = "c.cmpnd_id = :0";
			DbType[] pa = new DbType[1];
			pa[0] = DbType.Int64;
			object[] va = new object[1];
			va[0] = compoundId;
			UcdbCompound[] cmpnds = SelectDatabaseCompounds(criteria, pa, va);
			if (cmpnds.Length == 0) throw new Exception("Database CompoundId not found");
			return cmpnds[0];
		}

		/// <summary>
		/// Select database compound data for a databaseId and expCmpndIdTxt 
		/// </summary>
		/// <param name="databaseId"></param>
		/// <param name="extCmpndIdTxt"></param>
		/// <returns></returns>

		public UcdbCompound SelectDatabaseCompound(
			long databaseId,
			string extCmpndIdTxt)
		{
			string criteria = "lc.db_id = :0 and lc.ext_cmpnd_id_txt = :1";
			DbType[] pa = new DbType[2];
			pa[0] = DbType.Int64;
			pa[1] = DbType.String;
			object[] va = new object[2];
			va[0] = databaseId;
			va[1] = extCmpndIdTxt;
			UcdbCompound[] cmpnds = SelectDatabaseCompounds(criteria, pa, va);
			if (cmpnds.Length == 0) throw new Exception("DatabaseId, ExtCmpndIdTxt not found");
			return cmpnds[0];
		}

		/// <summary>
		/// Select compound information for a database 
		/// </summary>
		/// <param name="databaseId"></param>
		/// <returns></returns>

		public UcdbCompound[] SelectDatabaseCompounds(
			long databaseId)
		{
			string criteria = "lc.db_id = :0";
			DbType[] pa = new DbType[1];
			pa[0] = DbType.Int64;
			object[] va = new object[1];
			va[0] = databaseId;
			UcdbCompound[] cmpnds = SelectDatabaseCompounds(criteria, pa, va);
			return cmpnds;
		}

		/// <summary>
		/// Select compound information for given criteria
		/// </summary>
		/// <param name="databaseId"></param>
		/// <returns></returns>

		UcdbCompound[] SelectDatabaseCompounds(
			string criteria,
			DbType[] pa,
			object[] va)
		{
			string sql =
				"SELECT " +
				" c.cmpnd_id, c.mlcl_struct_typ_id, c.mlcl_struct, c.mlcl_frml, c.mlcl_wgt, c.mlcl_keys, " +
				" c.cmnt_txt, c.crtr_usr_nm, c.pndng_sts, c.crtn_date, c.updt_date," +
				" lc.db_id, lc.ext_cmpnd_id_nbr, lc.ext_cmpnd_id_txt, " +
				" a.alias_nm, a.crtn_date, a.updt_date " +
				"FROM " +
				" ucdb_owner.ucdb_db_cmpnd lc, " +
				" ucdb_owner.ucdb_cmpnd c, " +
				" ucdb_owner.ucdb_ALIAS a " +
				"WHERE " +
				" c.cmpnd_id = lc.cmpnd_id " +
				" AND a.cmpnd_id (+)= c.cmpnd_id and " + criteria + " " +
				"ORDER BY c.cmpnd_id";

			DbCommandMx drd = new DbCommandMx();
			drd.PrepareParameterized(sql, pa);
			drd.ExecuteReader(va);

			List<UcdbCompound> cmpndList = new List<UcdbCompound>();
			List<UcdbAlias> aliasList = null;
			UcdbCompound cmpnd = null;
			while (drd.Read())
			{
				if (cmpnd == null || cmpnd.CompoundId != drd.GetLong(0))
				{
					if (aliasList != null) cmpnd.Aliases = aliasList.ToArray();
					aliasList = null;

					cmpnd = new UcdbCompound();
					cmpnd.CompoundId = drd.GetLong(0);
					cmpnd.MolStructureFormat = (MolStructureFormatEnum)drd.GetInt(1);
					if (cmpnd.MolStructureFormat < 0) cmpnd.MolStructureFormat = MolStructureFormatEnum.Unknown;
					cmpnd.MolStructure = drd.GetString(2);
					cmpnd.MolFormula = drd.GetString(3);
					cmpnd.MolWeight = drd.GetDouble(4);
					if (cmpnd.MolWeight < 0) cmpnd.MolWeight = 0;
					if (!drd.IsNull(5))
						cmpnd.MolKeys = drd.GetBinary(5);
					cmpnd.Comment = drd.GetString(6);
					cmpnd.CreatorUserName = drd.GetString(7);
					cmpnd.PendingStatus = (UcdbWaitState)drd.GetInt(8);
					cmpnd.CreationDate = drd.GetDateTime(9);
					cmpnd.UpdateDate = drd.GetDateTime(10);
					cmpnd.DatabaseId = drd.GetLong(11);
					cmpnd.ExtCmpndIdNbr = drd.GetLong(12);
					cmpnd.ExtCmpndIdTxt = drd.GetString(13);

					cmpndList.Add(cmpnd);
				}

				if (!drd.IsNull(14)) // add any alias
				{
					if (aliasList == null) aliasList = new List<UcdbAlias>();
					UcdbAlias alias = new UcdbAlias();
					alias.CompoundId = cmpnd.CompoundId;
					alias.Name = drd.GetString(14);
					alias.CreationDate = drd.GetDateTime(15);
					alias.UpdateDate = drd.GetDateTime(16);
					aliasList.Add(alias);
				}
			}

			drd.Dispose();
			UcdbCompound[] cmpndArray = cmpndList.ToArray();
			return cmpndArray;
		}

		/// <summary>
		/// Get list of database compound ids
		/// </summary>
		/// <param name="databaseId"></param>
		/// <returns></returns>

		public long[] SelectDatabaseCompoundIds(
			long databaseId)
		{
			string sql =
				"SELECT cmpnd_id " +
				"FROM  ucdb_owner.ucdb_db_cmpnd " +
				"WHERE db_id = :0 " +
				"ORDER BY cmpnd_id";

			DbCommandMx drd = new DbCommandMx();
			drd.PrepareParameterized(sql, DbType.Int64);
			drd.ExecuteReader(databaseId);

			List<long> list = new List<long>();
			while (drd.Read())
			{
				if (drd.IsNull(0)) continue;
				list.Add(drd.GetLong(0));
			}
			drd.Dispose();

			long[] la = list.ToArray();
			return la;
		}

		/// <summary>
		/// Get list of database string compound ids
		/// </summary>
		/// <param name="databaseId"></param>
		/// <returns></returns>

		public string[] SelectDatabaseExtStringCids(
			long databaseId)
		{
			string sql =
				"SELECT ext_cmpnd_id_txt " +
				"FROM ucdb_owner.ucdb_db_cmpnd " +
				"WHERE db_id = :0 " +
				" AND ext_cmpnd_id_txt NOT LIKE '-%' " + // filter out compounds pending deletion that contain negative cmpnd_id
				"ORDER BY ext_cmpnd_id_txt";

			DbCommandMx drd = new DbCommandMx();
			drd.PrepareParameterized(sql, DbType.Int64);
			drd.ExecuteReader(databaseId);

			List<string> list = new List<string>();
			while (drd.Read())
			{
				if (drd.IsNull(0)) continue;
				list.Add(drd.GetString(0));
			}
			drd.Dispose();

			string[] sa = list.ToArray();
			return sa;
		}

		/// <summary>
		/// Get list of database numeric compound ids
		/// </summary>
		/// <param name="databaseId"></param>
		/// <returns></returns>

		public long[] SelectDatabaseExtIntegerCids(
			long databaseId)
		{
			string sql =
				"SELECT ext_cmpnd_id_nbr " +
				"FROM  ucdb_owner.ucdb_db_cmpnd " +
				"WHERE db_id = :0 " +
				" AND ext_cmpnd_id_nbr >= 0 " + // filter out compounds pending deletion that contain negative cmpnd_id
				"ORDER BY ext_cmpnd_id_nbr";

			DbCommandMx drd = new DbCommandMx();
			drd.PrepareParameterized(sql, DbType.Int64);
			drd.ExecuteReader(databaseId);

			List<long> list = new List<long>();
			while (drd.Read())
			{
				if (drd.IsNull(0)) continue;
				list.Add(drd.GetLong(0));
			}
			drd.Dispose();

			long[] la = list.ToArray();
			return la;
		}

		/// <summary>
		/// Return count of compounds for database
		/// </summary>
		/// <param name="databaseId"></param>
		/// <returns></returns>

		public int SelectDatabaseCompoundCount(
			long databaseId)
		{
			string sql =
				"SELECT count(*) " +
				"FROM  ucdb_owner.ucdb_db_cmpnd " +
				"WHERE db_id = :0 " +
				" AND ext_cmpnd_id_txt NOT LIKE '-%'"; // filter out compounds pending deletion that contain negative cmpnd_id

			DbCommandMx drd = new DbCommandMx();
			drd.PrepareParameterized(sql, DbType.Int64);
			drd.ExecuteReader(databaseId);

			drd.Read();
			int cmpndCnt = drd.GetInt(0);
			drd.Dispose();
			return cmpndCnt;
		}

		/// <summary>
		/// Select model/annotation information for a database
		/// </summary>
		/// <param name="databaseId"></param>
		/// <returns></returns>

		public UcdbModel[] SelectDatabaseModels(
			long databaseId)
		{
			string sql =
				"select " +
				" db_model_id, db_id, model_id, model_build_id, " +
				" pndng_sts, crtn_date, updt_date " +
				"from ucdb_owner.ucdb_db_model " +
				"where db_id = :0";

			DbCommandMx drd = new DbCommandMx();
			drd.PrepareMultipleParameter(sql, 1);
			drd.ExecuteReader(databaseId.ToString());

			List<UcdbModel> modelList = new List<UcdbModel>();
			while (drd.Read())
			{
				UcdbModel d = new UcdbModel();
				d.DbModelId = drd.GetLong(0);
				d.DatabaseId = drd.GetLong(1);
				d.ModelId = drd.GetLong(2);
				d.BuildId = drd.GetLong(3);
				d.PendingStatus = (UcdbWaitState)drd.GetInt(4);
				d.CreationDate = drd.GetDateTime(5);
				d.UpdateDate = drd.GetDateTime(6);

				modelList.Add(d);
			}

			drd.Dispose();
			UcdbModel[] modelArray = modelList.ToArray();
			return modelArray;
		}

		/// <summary>
		/// Create a new database
		/// </summary>
		/// <param name="ucdb"></param>

		public void CreateDatabase(
			UcdbDatabase ucdb)
		{
			InsertDatabaseHeader(ucdb);

			if (ucdb.Models != null) // insert an initial models
				UpdateDatabaseModelAssoc(ucdb, ucdb.Models);

			if (ucdb.Compounds != null) // insert any initial compounds
				UpdateDatabaseCompounds(ucdb, ucdb.Compounds);

			return;
		}

		/// <summary>
		/// Update an existing database
		/// </summary>
		/// <param name="ucdb"></param>

		public void UpdateDatabase(
			UcdbDatabase ucdb)
		{

			// This methods scans the changes in the db object,
			// determines the elements that need updating
			// and queues up the update.
			// Updating of the Oracle database is done immediately except for model calculations
			// and deletes which are performed by a background process.

			if (ucdb.RowState != UcdbRowState.Deleted)
			{
				if (ucdb.Models != null) // update database data associations
					UpdateDatabaseModelAssoc(ucdb, ucdb.Models);

				if (ucdb.Compounds != null) // update database compounds
					UpdateDatabaseCompounds(ucdb, ucdb.Compounds);
			}

			UpdateDatabaseHeader(ucdb); // update header last to set proper counts

			return;
		}

		/// <summary>
		/// Insert a new database header
		/// </summary>
		/// <param name="ucdb"></param>

		public void InsertDatabaseHeader(
			UcdbDatabase ucdb)
		{
			if (ucdb == null) DebugMx.ArgException("Database is null");
			if (ucdb.Name == null || ucdb.Name == "") DebugMx.ArgException("Database name not defined");
			if (ucdb.OwnerUserName == null || ucdb.OwnerUserName == "") DebugMx.ArgException("Database owner not defined");
			if (ucdb.DatabaseId > 0) DebugMx.ArgException("DatabaseId must be <=0 for new database");

			ucdb.DatabaseId = SequenceDao.NextVal("ucdb_owner.ucdb_db_id_seq");
			ucdb.CompoundCount = 0;
			ucdb.ModelCount = 0;

			ucdb.PendingStatus = UcdbWaitState.Complete; // complete initially

			ucdb.CreationDate = DateTime.Now;
			ucdb.UpdateDate = DateTime.Now;

			DataTable dt = GetUcdbDatabaseDataTable();
			string sql = DbCommandMx.BuildInsertSql(dt);

			DbType[] pta = DbCommandMx.GetParameterTypes(dt);

			DbCommandMx dwd = new DbCommandMx();
			dwd.PrepareParameterized(sql, pta);
			dwd.BeginTransaction(); // be sure we have a transaction going

			object[] pva = SetDatabaseParameterValues(ucdb);

			int count = dwd.ExecuteNonReader(pva); // do insert
			dwd.Commit();
			dwd.Dispose();

			return;
		}

		/// <summary>
		/// Update an existing database header
		/// </summary>
		/// <param name="ucdb"></param>

		public void UpdateDatabaseHeader(
			UcdbDatabase ucdb)
		{
			if (ucdb == null) DebugMx.ArgException("Database is null");
			if (ucdb.Name == null || ucdb.Name == "") DebugMx.ArgException("Database name not defined");
			if (ucdb.OwnerUserName == null || ucdb.OwnerUserName == "") DebugMx.ArgException("Database owner not defined");
			if (ucdb.DatabaseId <= 0) DebugMx.ArgException("DatabaseId must be > 0 for database update");

			ucdb.UpdateDate = DateTime.Now;

			DataTable dt = GetUcdbDatabaseDataTable();
			string sql = DbCommandMx.BuildUpdateSql(dt) + " where db_id=:0";

			DbType[] pta = DbCommandMx.GetParameterTypes(dt);

			DbCommandMx dwd = new DbCommandMx();
			dwd.PrepareParameterized(sql, pta);
			dwd.BeginTransaction();

			object[] pva = SetDatabaseParameterValues(ucdb);
			int count = dwd.ExecuteNonReader(pva); // do update
			dwd.Commit();
			dwd.Dispose();

			return;
		}

		DataTable GetUcdbDatabaseDataTable()
		{
			if (UcdbDatabaseModelTable != null) return UcdbDatabaseModelTable;
			DataTable dt = UcdbDatabaseModelTable =
				new DataTable("ucdb_owner.ucdb_db");

			DbCommandMx.ADC(dt, "db_id", DbType.Int64);
			DbCommandMx.ADC(dt, "db_nm_space", DbType.String);
			DbCommandMx.ADC(dt, "db_nm", DbType.String);
			DbCommandMx.ADC(dt, "cmnt_txt", DbType.String);
			DbCommandMx.ADC(dt, "crtr_usr_nm", DbType.String);
			DbCommandMx.ADC(dt, "acl_id", DbType.Int32);
			DbCommandMx.ADC(dt, "cmpnd_cnt", DbType.Int32);
			DbCommandMx.ADC(dt, "assoc_data_cnt", DbType.Int32);
			DbCommandMx.ADC(dt, "sprt_struct_srch", DbType.StringFixedLength);
			DbCommandMx.ADC(dt, "allow_dup_struct", DbType.StringFixedLength);
			DbCommandMx.ADC(dt, "ext_cmpnd_id_typ_id", DbType.Int32);
			DbCommandMx.ADC(dt, "pndng_sts", DbType.Int32);
			DbCommandMx.ADC(dt, "pndng_cmpnd_cnt", DbType.Int32);
			DbCommandMx.ADC(dt, "pndng_cmpnd_id", DbType.Int64);
			DbCommandMx.ADC(dt, "pndng_updt_date", DbType.Date);
			DbCommandMx.ADC(dt, "crtn_date", DbType.Date);
			DbCommandMx.ADC(dt, "updt_date", DbType.Date);
			return dt;
		}

		object[] SetDatabaseParameterValues(
			UcdbDatabase ucdb)
		{
			object[] pva = new object[17];
			pva[0] = ucdb.DatabaseId;
			pva[1] = ucdb.NameSpace;
			pva[2] = ucdb.Name;
			pva[3] = ucdb.Description;
			pva[4] = ucdb.OwnerUserName;
			pva[5] = !ucdb.Public ? 1 : 2;
			pva[6] = ucdb.CompoundCount;
			pva[7] = ucdb.ModelCount;
			pva[8] = ucdb.StructureSearchSupported == true ? "T" : "F";
			pva[9] = ucdb.AllowDuplicateStructures == true ? "T" : "F";
			pva[10] = (int)ucdb.CompoundIdType;
			pva[11] = (int)ucdb.PendingStatus;
			pva[12] = ucdb.PendingCompoundCount;
			pva[13] = ucdb.PendingCompoundId;
			pva[14] = (ucdb.PendingUpdateDate != DateTime.MinValue ? (object)ucdb.PendingUpdateDate : null);
			pva[15] = ucdb.CreationDate;
			pva[16] = ucdb.UpdateDate;
			return pva;
		}

		/// <summary>
		/// Insert compounds
		/// </summary>
		/// <param name="ucdb"></param>

		public void InsertDatabaseCompounds(
			UcdbDatabase ucdb,
			UcdbCompound[] compounds)
		{
			string sql;
			DbType[] pta;
			object[][] pva; // 2-d array of buffered insert/update rows
			int pvaCount, insCount;

			if (ucdb == null) throw new Exception("Database not defined");
			if (ucdb.DatabaseId <= 0) throw new Exception("DatabaseId not defined");
			if (compounds == null) throw new Exception("Compounds not defined");

			DbCommandMx dwd = new DbCommandMx();

			// Insert compound rows

			DataTable dt = GetUcdbCompoundDataTable();

			sql = DbCommandMx.BuildInsertSql(dt);
			pta = DbCommandMx.GetParameterTypes(dt);

			dwd.PrepareParameterized(sql, pta);
			dwd.BeginTransaction(); // be sure we have a transaction going

			int seqCacheSize = (compounds.Length < 100) ? compounds.Length : 100;
			SequenceDao.SetCacheSize("ucdb_owner.ucdb_cmpnd_id_seq", seqCacheSize);

			pva = DbCommandMx.NewObjectArrayArray(dt.Columns.Count, 100); // alloc insert row array
			pvaCount = 0;
			int aliasCnt = 0; // also count aliases for later use
			for (int ci = 0; ci < compounds.Length; ci++)
			{
				UcdbCompound c = compounds[ci];
				if (c.RowState != UcdbRowState.Unknown && c.RowState != UcdbRowState.Added)
					continue; // only handle new rows here

				c.CompoundId = SequenceDao.NextVal("ucdb_owner.ucdb_cmpnd_id_seq");
				c.CreationDate = DateTime.Now;
				c.UpdateDate = DateTime.Now;

				pvaCount = SetCompoundParameterValues(pva, pvaCount, c);
				if (pvaCount >= pva.Length)
					insCount = dwd.ExecuteArrayNonReader(pva, ref pvaCount);

				if (ci % 1000 == 0)
				{
					dwd.Commit();
					dwd.BeginTransaction();
				}

				if (c.Aliases != null) aliasCnt += c.Aliases.Length;
			}

			if (pvaCount > 0) // insert any remaining compound rows
				insCount = dwd.ExecuteArrayNonReader(pva, ref pvaCount);

			// Insert database-compound rows

			dt = GetDatabaseCompoundDataTable();
			sql = DbCommandMx.BuildInsertSql(dt);

			pta = DbCommandMx.GetParameterTypes(dt);
			dwd.PrepareParameterized(sql, pta);

			pva = DbCommandMx.NewObjectArrayArray(dt.Columns.Count, 100); // alloc insert row array
			pvaCount = 0;
			for (int ci = 0; ci < compounds.Length; ci++)
			{
				UcdbCompound c = compounds[ci];

				if (c.RowState != UcdbRowState.Unknown && c.RowState != UcdbRowState.Added)
					continue; // ignore if not a new row

				c.DatabaseId = ucdb.DatabaseId; // be sure databaseId is set

				pvaCount = SetDatabaseCompoundParameterValues(pva, pvaCount, c);
				if (pvaCount % 100 == 0)
					insCount = dwd.ExecuteArrayNonReader(pva, ref pvaCount);

				if (ci % 1000 == 0)
				{
					dwd.Commit();
					dwd.BeginTransaction();
				}

			}

			if (pvaCount > 0) // insert any remaining database-compound rows
				insCount = dwd.ExecuteArrayNonReader(pva, ref pvaCount);

			// Insert any aliases

			if (aliasCnt > 0)
			{
				dt = GetUcdbAliasDataTable();
				sql = DbCommandMx.BuildInsertSql(dt);

				pta = DbCommandMx.GetParameterTypes(dt);
				dwd.PrepareParameterized(sql, pta);

				seqCacheSize = (aliasCnt < 100) ? aliasCnt : 100;
				SequenceDao.SetCacheSize("ucdb_owner.ucdb_alias_id_seq", seqCacheSize);

				pva = DbCommandMx.NewObjectArrayArray(dt.Columns.Count, 100); // alloc insert row array
				pvaCount = 0;
				for (int ci = 0; ci < compounds.Length; ci++)
				{
					UcdbCompound c = compounds[ci];
					if (c.Aliases == null || c.Aliases.Length == 0) continue;

					for (int ai = 0; ai < c.Aliases.Length; ai++)
					{

						UcdbAlias a = c.Aliases[ai];

						if (a.RowState != UcdbRowState.Unknown && a.RowState != UcdbRowState.Added)
							continue; // must be a new row

						if (a.AliasId <= 0)
							a.AliasId = SequenceDao.NextVal("ucdb_owner.ucdb_alias_id_seq");
						a.CompoundId = c.CompoundId;
						a.CreationDate = DateTime.Now;
						a.UpdateDate = DateTime.Now;

						pvaCount = SetAliasParameterValues(pva, pvaCount, a);
						if (pvaCount >= pva.Length)
							dwd.ExecuteArrayNonReader(pva, ref pvaCount);
					}
					if (ci % 1000 == 0)
					{
						dwd.Commit();
						dwd.BeginTransaction(); // be sure we have a transaction going
					}

				}

				if (pvaCount > 0) // insert any remaining alias rows
					dwd.ExecuteArrayNonReader(pva, ref pvaCount);
			} // end for alias insert

			dwd.Commit();
			dwd.Dispose();

			return;
		}

		DataTable GetUcdbCompoundDataTable()
		{
			if (UcdbCompoundDataTable != null) return UcdbCompoundDataTable;
			DataTable dt = UcdbCompoundDataTable =
				new DataTable("ucdb_owner.ucdb_cmpnd");

			DbCommandMx.ADC(dt, "cmpnd_id", DbType.Int64);
			DbCommandMx.ADC(dt, "mlcl_struct_typ_id", DbType.Int32);
			DbCommandMx.ADC(dt, "mlcl_struct", DbType.String);
			DbCommandMx.ADC(dt, "mlcl_frml", DbType.String);
			DbCommandMx.ADC(dt, "mlcl_wgt", DbType.Double);
			DbCommandMx.ADC(dt, "mlcl_keys", DbType.Binary);
			DbCommandMx.ADC(dt, "cmnt_txt", DbType.String);
			DbCommandMx.ADC(dt, "crtr_usr_nm", DbType.String);
			DbCommandMx.ADC(dt, "pndng_sts", DbType.Int32);
			DbCommandMx.ADC(dt, "crtn_date", DbType.Date);
			DbCommandMx.ADC(dt, "updt_date", DbType.Date);
			return dt;
		}

		/// <summary>
		/// Set Compound parameters in an array of value arrays for bulk operations
		/// </summary>
		/// <param name="pva"></param>
		/// <param name="pvaCount"></param>
		/// <param name="c"></param>
		/// <returns></returns>

		int SetCompoundParameterValues(
			object[][] pva,
			int pvaCount,
			UcdbCompound c)
		{
			object[] pva1 = SetCompoundParameterValues(c, 0);
			for (int pi = 0; pi < pva1.Length; pi++)
				pva[pi][pvaCount] = pva1[pi];
			return pvaCount + 1;
		}

		/// <summary>
		/// Set Compound parameters in a single value array
		/// </summary>
		/// <param name="pva"></param>
		/// <param name="c"></param>
		/// <returns></returns>

		object[] SetCompoundParameterValues(
			UcdbCompound c,
			int extraParameters)
		{
			object[] pva = new object[11 + extraParameters];
			pva[0] = c.CompoundId;
			pva[1] = (int)c.MolStructureFormat;
			pva[2] = c.MolStructure;
			pva[3] = c.MolFormula;
			if (c.MolWeight > 0)
				pva[4] = c.MolWeight;
			pva[5] = c.MolKeys;
			pva[6] = c.Comment;
			pva[7] = c.CreatorUserName;
			pva[8] = c.PendingStatus;
			pva[9] = c.CreationDate;
			pva[10] = c.UpdateDate;
			return pva;
		}

		DataTable GetDatabaseCompoundDataTable()
		{
			if (UcdbDatabaseCompoundDataTable != null) return UcdbDatabaseCompoundDataTable;
			DataTable dt = UcdbDatabaseCompoundDataTable =
				new DataTable("ucdb_owner.ucdb_db_cmpnd");

			DbCommandMx.ADC(dt, "cmpnd_id", DbType.Int64); // put cmpnd_id first
			DbCommandMx.ADC(dt, "db_id", DbType.Int64);
			DbCommandMx.ADC(dt, "ext_cmpnd_id_nbr", DbType.Int64);
			DbCommandMx.ADC(dt, "ext_cmpnd_id_txt", DbType.String);
			DbCommandMx.ADC(dt, "crtn_date", DbType.Date);
			DbCommandMx.ADC(dt, "updt_date", DbType.Date);
			return dt;
		}

		int SetDatabaseCompoundParameterValues(
			object[][] pva,
			int pvaCount,
			UcdbCompound c)
		{
			object[] pva1 = SetDatabaseCompoundParameterValues(c, 0);
			for (int pi = 0; pi < pva1.Length; pi++)
				pva[pi][pvaCount] = pva1[pi];
			return pvaCount + 1;
		}

		/// <summary>
		/// Set DatabaseCompound parameters in a single value array
		/// </summary>
		/// <param name="pva"></param>
		/// <param name="c"></param>
		/// <returns></returns>

		object[] SetDatabaseCompoundParameterValues(
			UcdbCompound c,
			int extraParameters)
		{
			object[] pva = new object[6 + extraParameters];

			pva[0] = c.CompoundId;
			pva[1] = c.DatabaseId;
			pva[2] = c.ExtCmpndIdNbr;
			pva[3] = c.ExtCmpndIdTxt;
			pva[4] = c.CreationDate;
			pva[5] = c.UpdateDate;
			return pva;
		}

		DataTable GetUcdbAliasDataTable()
		{
			if (UcdbAliasDataTable != null) return UcdbAliasDataTable;
			DataTable dt = UcdbAliasDataTable =
				new DataTable("ucdb_owner.ucdb_alias");

			DbCommandMx.ADC(dt, "alias_id", DbType.Int64);
			DbCommandMx.ADC(dt, "cmpnd_id", DbType.Int64);
			DbCommandMx.ADC(dt, "alias_nm", DbType.String);
			DbCommandMx.ADC(dt, "alias_typ_id", DbType.Int32);
			DbCommandMx.ADC(dt, "crtn_date", DbType.Date);
			DbCommandMx.ADC(dt, "updt_date", DbType.Date);
			return dt;
		}

		/// <summary>
		/// Set Alias parameter values in an array of value arrays for bulk operations
		/// </summary>
		/// <param name="pva"></param>
		/// <param name="pvaCount"></param>
		/// <param name="a"></param>
		/// <returns></returns>

		int SetAliasParameterValues(
			object[][] pva,
			int pvaCount,
			UcdbAlias a)
		{
			object[] pva1 = SetAliasParameterValues(a, 0);
			for (int pi = 0; pi < pva1.Length; pi++)
				pva[pi][pvaCount] = pva1[pi];
			return pvaCount + 1;
		}

		/// <summary>
		/// Set alias parameter values in a single value array
		/// </summary>
		/// <param name="a"></param>
		/// <param name="extraParameters"></param>
		/// <returns></returns>

		object[] SetAliasParameterValues(
			UcdbAlias a,
			int extraParameters)
		{
			object[] pva = new object[6 + extraParameters];
			pva[0] = a.AliasId;
			pva[1] = a.CompoundId;
			pva[2] = a.Name;
			pva[3] = a.Type;
			pva[4] = a.CreationDate;
			pva[5] = a.UpdateDate;
			return pva;
		}

		/// <summary>
		/// Insert definitions of models associated with database
		/// </summary>
		/// <param name="ucdb"></param>
		/// <returns></returns>

		public int InserDatabaseModel(
			UcdbDatabase ucdb,
			UcdbModel[] dataAssoc)
		{
			string sql;
			DbType[] pta;
			object[][] pva; // 2-d array of buffered insert/update rows
			int pvaCount, insCnt;

			if (ucdb == null) throw new Exception("Database not defined");
			if (ucdb.DatabaseId <= 0) throw new Exception("DatabaseId not defined");
			if (dataAssoc == null) throw new Exception("Associated data not defined");

			long databaseId = ucdb.DatabaseId;
			DbCommandMx dwd = new DbCommandMx();

			DataTable dt = GetDatabaseModelDataTable();

			sql = DbCommandMx.BuildInsertSql(dt);
			pta = DbCommandMx.GetParameterTypes(dt);
			dwd.PrepareParameterized(sql, pta);
			dwd.BeginTransaction(); // be sure we have a transaction going

			pva = DbCommandMx.NewObjectArrayArray(dt.Columns.Count, 100); // alloc insert row array
			pvaCount = 0;
			for (int di = 0; di < dataAssoc.Length; di++)
			{
				UcdbModel ld = dataAssoc[di];

				if (ld.RowState != UcdbRowState.Unknown && ld.RowState != UcdbRowState.Added)
					continue; // must be a new row

				ld.DbModelId = SequenceDao.NextVal("ucdb_owner.ucdb_db_model_id_seq");
				ld.DatabaseId = databaseId; // be sure databaseId is set
				ld.CreationDate = DateTime.Now;
				ld.UpdateDate = DateTime.Now;

				pvaCount = SetDatabaseModelParameterValues(pva, pvaCount, ld);
				if (pvaCount % 100 == 0)
					dwd.ExecuteArrayNonReader(pva, ref pvaCount);
			}

			if (pvaCount > 0)
				insCnt = dwd.ExecuteArrayNonReader(pva, ref pvaCount);

			dwd.Commit();
			dwd.Dispose();
			return dataAssoc.Length;
		}

		/// <summary>
		/// Update a database model association
		/// </summary>
		/// <param name="lda"></param>

		public void UpdateDatabaseModelAssoc(
			UcdbDatabase ucdb,
			UcdbModel[] dbModels)
		{
			List<UcdbModel> adds = new List<UcdbModel>();
			int addCnt = 0, updateCnt = 0, deleteCnt = 0;

			DbCommandMx dwd = new DbCommandMx();
			foreach (UcdbModel m in dbModels)
			{
				if (m.DbModelId <= 0) // new row
				{
					m.PendingStatus = UcdbWaitState.ModelPredictions;
					adds.Add(m);
					addCnt++;
				}

				else if (m.RowState == UcdbRowState.Modified)
				{
					if (m.PendingStatus == UcdbWaitState.Deletion)  // if marked for deletion then keep that state
						deleteCnt++; // count as deletion now to display correct model count

					else updateCnt++;

					UpdateDatabaseModel(ucdb, m, dwd);
				}

				else if (m.RowState == UcdbRowState.Deleted) { } // actual deletions handled by UpdatePendingModelResults
			}
			dwd.Dispose();

			if (adds.Count > 0) // do any adds
				InserDatabaseModel(ucdb, adds.ToArray());

			foreach (UcdbModel da in dbModels)
			{
				if (da.RowState != UcdbRowState.Deleted)
					da.RowState = UcdbRowState.Unchanged; // reset row state for adds/updates
			}

			int delta = addCnt - deleteCnt;
			ucdb.ModelCount += delta; // new count of associations

			// Update prediction pending status unless waiting for database storage

			if (ucdb.PendingStatus != UcdbWaitState.DatabaseStorage)
			{
				if (addCnt > 0)
				{ // if adding models then set
					ucdb.PendingStatus = UcdbWaitState.ModelPredictions;
					ucdb.PendingCompoundCount = ucdb.CompoundCount;
					ucdb.PendingCompoundId = 0; // back to beginning
				}

				else if (deleteCnt > 0) // if just deleting models then set status so they get deleted 
				{
					ucdb.PendingStatus = UcdbWaitState.ModelPredictions;
				}

				else if (ucdb.ModelCount == 0) // if no models then nothing to do
				{
					ucdb.PendingStatus = UcdbWaitState.Complete;
					ucdb.PendingCompoundCount = 0;
					ucdb.PendingCompoundId = 0;
				}
			}

			return;
		}

		/// <summary>
		/// Update a single database data association (internal method)
		/// </summary>
		/// <param name="lda"></param>

		void UpdateDatabaseModel(
			UcdbDatabase ucdb,
			UcdbModel dbModel,
			DbCommandMx dwd)
		{

			// For updates this method updates both the basic compound data
			// and the pending status of the row. For deletes it just updates the
			// pending status. Once the associated model/annotation data is deleted
			// the row is finally deleted by DeleteDatabaseModel.

			DataTable ldaDt = GetDatabaseModelDataTable();
			string sql = DbCommandMx.BuildUpdateSql(ldaDt) +
				" where db_model_id = :0";

			DbType[] pta = DbCommandMx.GetParameterTypes(ldaDt);
			dwd.PrepareParameterized(sql, pta);

			object[] pva = SetDatabaseModelParameterValues(dbModel, 0);
			int count = dwd.ExecuteNonReader(pva); // do update
			return;
		}

		/// <summary>
		/// Check if user if allowed to modify a database
		/// </summary>
		/// <param name="databaseId"></param>
		/// <param name="userName"></param>
		/// <returns></returns>

		public bool CanModifyDatabase(
			long databaseId,
			string userName)
		{
			UcdbDatabase ucdb = SelectDatabaseHeader(databaseId);
			if (userName.Equals(ucdb.OwnerUserName, StringComparison.OrdinalIgnoreCase))
				return true; // simple ownership check for now
			else return false;
		}

		/// <summary>
		/// Update the compounds for a database
		/// </summary>
		/// <param name="ucdb"></param>
		/// <param name="cpds"></param>
		/// <returns>New number of compounds in database</returns>

		public void UpdateDatabaseCompounds(
			UcdbDatabase ucdb,
			UcdbCompound[] cpds)
		{
			long databaseId = ucdb.DatabaseId;
			List<UcdbCompound> adds = new List<UcdbCompound>();
			List<UcdbCompound> updates = new List<UcdbCompound>();
			int addCnt = 0, updateCnt = 0, deleteCnt = 0;

			foreach (UcdbCompound c in cpds)
			{
				if (c.RowState == UcdbRowState.Added || c.RowState == UcdbRowState.Unknown)
				{
					adds.Add(c);
					addCnt++;
				}

				else if (c.RowState == UcdbRowState.Modified)
				{
					updates.Add(c);
					updateCnt++;
				}

				else if (c.RowState == UcdbRowState.Deleted)
				{
					updates.Add(c);
					deleteCnt++;
				}


				else if (c.RowState == UcdbRowState.Unchanged)
					continue;
			}

			if (adds.Count > 0) // do any adds
				InsertDatabaseCompounds(ucdb, adds.ToArray());

			if (updates.Count > 0) // do any updates
			{
				DbCommandMx dwd = new DbCommandMx();
				dwd.SetConnection("ucdb_owner.ucdb_db");
				dwd.BeginTransaction();
				foreach (UcdbCompound cpd in updates)
				{
					UpdateDatabaseCompound(ucdb, cpd, dwd);
				}
				dwd.Commit();
				dwd.Dispose();
			}

			int delta = addCnt - deleteCnt;
			ucdb.CompoundCount += delta; // set new number of compounds

			if (ucdb.ModelCount > 0) // if models then set model prediction pending status
			{
				ucdb.PendingCompoundCount += delta; // update number of compounds with pending prediction updates
				ucdb.PendingStatus = UcdbWaitState.ModelPredictions;
			}

			return;
		}

		/// <summary>
		/// Update a single compound
		/// </summary>
		/// <param name="ucdb"></param>
		/// <param name="cmpnd"></param>

		void UpdateDatabaseCompound(
			UcdbDatabase ucdb,
			UcdbCompound cmpnd,
			DbCommandMx dwd)
		{

			// For updates this method updates both the basic compound data
			// and the pending status of the row. For deletes it just updates the
			// pending status. Once the associated model/annotation data is deleted
			// the row is finally deleted by DeleteDatabaseModel.

			int count;

			DataTable dbCmpndDt = GetDatabaseCompoundDataTable();
			DataTable cmpndDt = GetUcdbCompoundDataTable();
			DataTable aliasDt = GetUcdbAliasDataTable();

			UcdbCompound oldCmpnd = SelectDatabaseCompound(cmpnd.CompoundId); // get existing data

			long compoundId = oldCmpnd.CompoundId; // copy existing values that won't have changed
			cmpnd.CompoundId = compoundId;
			cmpnd.CreationDate = oldCmpnd.CreationDate;
			cmpnd.UpdateDate = DateTime.Now;

			if (cmpnd.RowState == UcdbRowState.Modified)
			{
				if (ucdb.ModelCount > 0) // if any models then waiting for predictions
					cmpnd.PendingStatus = UcdbWaitState.ModelPredictions;
				else cmpnd.PendingStatus = UcdbWaitState.Complete;
			}

			else if (cmpnd.RowState == UcdbRowState.Deleted)
			{
				cmpnd.ExtCmpndIdNbr = -cmpnd.CompoundId; // reset external cids so cid being deleted could 
				cmpnd.ExtCmpndIdTxt = (-cmpnd.CompoundId).ToString(); // be reassigned before delete completes
				cmpnd.PendingStatus = UcdbWaitState.Deletion;
			}

			else throw new Exception("Unexpected compound row state");

			// Update UCDB_CMPND

			string sql = DbCommandMx.BuildUpdateSql(cmpndDt) +
				" where cmpnd_id = :0";

			DbType[] pta = DbCommandMx.GetParameterTypes(cmpndDt);
			dwd.PrepareParameterized(sql, pta);

			object[] pva = SetCompoundParameterValues(cmpnd, 0);
			count = dwd.ExecuteNonReader(pva); // do update

			// Update UCDB_DB_CMPND

			if (cmpnd.ExtCmpndIdTxt != oldCmpnd.ExtCmpndIdTxt)
			{
				sql = DbCommandMx.BuildUpdateSql(dbCmpndDt) +
					" where cmpnd_id = :0";

				pta = DbCommandMx.GetParameterTypes(dbCmpndDt);
				dwd.PrepareParameterized(sql, pta);

				pva = SetDatabaseCompoundParameterValues(cmpnd, 0);
				count = dwd.ExecuteNonReader(pva); // do update
			}

			// Update UCDB_ALIAS

			if (cmpnd.Aliases != null) // update aliases if array is not-null
			{

				sql = "delete " + /* aliases */
				"from ucdb_owner.ucdb_alias " +
				"where cmpnd_id = :0";

				dwd.PrepareParameterized(sql, DbType.Int64);
				count = dwd.ExecuteNonReader(compoundId);

				sql = DbCommandMx.BuildInsertSql(aliasDt);
				pta = DbCommandMx.GetParameterTypes(aliasDt);
				dwd.PrepareParameterized(sql, pta);
				for (int ai = 0; ai < cmpnd.Aliases.Length; ai++)
				{

					UcdbAlias a = cmpnd.Aliases[ai];

					if (a.RowState == UcdbRowState.Deleted) continue;
					if (a.AliasId <= 0)
						a.AliasId = SequenceDao.NextVal("ucdb_owner.ucdb_alias_id_seq");
					a.CompoundId = compoundId;
					a.CreationDate = DateTime.Now;
					a.UpdateDate = DateTime.Now;

					pva = SetAliasParameterValues(a, 0);
					count = dwd.ExecuteNonReader(pva); // do insert
				}
			}

			return;
		}

		/// <summary>
		/// Delete a single compound
		/// </summary>
		/// <param name="compoundId"></param>

		long DeleteDatabaseCompound(
			long compoundId)
		{
			// Note that this method should only be called internally after 
			// deletetion of associated data is complete.

			string criteria = "cmpnd_id = :0";
			long delCount = DeleteDatabaseCompounds(criteria, compoundId.ToString());
			return delCount;
		}

		/// <summary>
		/// Delete all compound data for a database 
		/// </summary>
		/// <param name="databaseId"></param>
		/// <returns></returns>

		public long DeleteDatabaseCompounds(
			long databaseId)
		{
			string criteria = "db_id = :0";
			long delCount = DeleteDatabaseCompounds(criteria, databaseId.ToString());
			return delCount;
		}

		/// <summary>
		/// Delete one or more database compounds
		/// </summary>
		/// <param name="criteria"></param>
		/// <param name="parmValue"></param>
		/// <returns></returns>

		long DeleteDatabaseCompounds(
			string criteria,
			string parmValue)
		{
			string[] deleteStmts = {

			"delete " + /* model results */
			"from ucdb_owner.spm_rslt " +
			"where cmpnd_id in " +
			" (select cmpnd_id from ucdb_owner.ucdb_db_cmpnd where " + criteria + ")",

			"delete " + /* aliases */ 
			"from ucdb_owner.ucdb_alias " +
			"where cmpnd_id in " +
			" (select cmpnd_id " +
			"  from ucdb_owner.ucdb_db_cmpnd " +
			"  where " + criteria + " )",

			"delete " + /* compound */ 
			"from ucdb_owner.ucdb_cmpnd " +
			"where cmpnd_id in " +
			" (select cmpnd_id " +
			"  from ucdb_owner.ucdb_db_cmpnd " +
			"  where " + criteria + " )",

			"delete " + /* db-compound assoc */ 
			"from ucdb_owner.ucdb_db_cmpnd " +
			"where " + criteria + " "
			};

			long deletes = ExecuteDeleteStatements(deleteStmts, parmValue);
			return deletes;
		}

		/// <summary>
		/// Get DataTable for ucdb_db_model
		/// </summary>
		/// <returns></returns>

		DataTable GetDatabaseModelDataTable()
		{
			if (UcdbDatabaseModelDataTable != null) return UcdbDatabaseModelDataTable;
			DataTable dt = UcdbDatabaseModelDataTable =
				new DataTable("ucdb_owner.ucdb_db_model");

			DbCommandMx.ADC(dt, "db_model_id", DbType.Int64);
			DbCommandMx.ADC(dt, "db_id", DbType.Int64);
			DbCommandMx.ADC(dt, "model_id", DbType.Int64);
			DbCommandMx.ADC(dt, "model_build_id", DbType.Int64);
			DbCommandMx.ADC(dt, "pndng_sts", DbType.Int32);
			DbCommandMx.ADC(dt, "crtn_date", DbType.Date);
			DbCommandMx.ADC(dt, "updt_date", DbType.Date);
			return dt;
		}

		/// <summary>
		/// Set Database Model parameters for bulk operations
		/// </summary>
		/// <param name="pva"></param>
		/// <param name="pvaCount"></param>
		/// <param name="lad"></param>
		/// <returns></returns>

		int SetDatabaseModelParameterValues(
			object[][] pva,
			int pvaCount,
			UcdbModel lda)
		{
			object[] pva1 = SetDatabaseModelParameterValues(lda, 0);
			for (int pi = 0; pi < pva1.Length; pi++)
				pva[pi][pvaCount] = pva1[pi];
			return pvaCount + 1;
		}

		/// <summary>
		/// Set Compound parameters in a single value array
		/// </summary>
		/// <param name="pva"></param>
		/// <param name="c"></param>
		/// <returns></returns>

		object[] SetDatabaseModelParameterValues(
			UcdbModel lda,
			int extraParameters)
		{
			object[] pva = new object[7 + extraParameters];
			pva[0] = lda.DbModelId;
			pva[1] = lda.DatabaseId;
			pva[2] = lda.ModelId;
			pva[3] = lda.BuildId;
			pva[4] = lda.PendingStatus;
			pva[5] = lda.CreationDate;
			pva[6] = lda.UpdateDate;
			return pva;
		}

		/// <summary>
		/// Execute a series of delete statements with transaction control
		/// </summary>
		/// <param name="drd"></param>
		/// <returns></returns>

		long ExecuteDeleteStatements(
			string[] deleteStmts,
			string criteriaValue)
		{
			int delCnt, totalDeletes = 0;

			DbCommandMx dwd = new DbCommandMx();
			dwd.SetConnection("ucdb_owner.ucdb_db");
			dwd.BeginTransaction();

			foreach (string sql in deleteStmts)
			{
				string sql2 = sql + " and rownum <= 1000"; // keep transactions reasonable size

				if (criteriaValue == null) dwd.Prepare(sql2);
				else dwd.PrepareMultipleParameter(sql2, 1);

				while (true)
				{
					if (criteriaValue == null) delCnt = dwd.ExecuteNonReader();
					else delCnt = dwd.ExecuteNonReader(criteriaValue);
					if (delCnt == 0) break;
					dwd.Commit();
					totalDeletes += delCnt;

					int t1 = TimeOfDay.Milliseconds();
				}
			}

			dwd.Dispose();
			return totalDeletes;
		}

		/// <summary>
		/// Get the largest numeric compound id assigned to a database
		/// </summary>
		/// <param name="databaseId"></param>
		/// <returns></returns>

		public long GetMaxCompoundId(
			long databaseId)
		{
			UcdbDatabase ucdb = SelectDatabaseHeader(databaseId);

			if (ucdb.CompoundIdType == CompoundIdTypeEnum.Integer)
			{ // numeric compound id
				string sql =
					"select max(ext_cmpnd_id_nbr) " +
					"from ucdb_owner.ucdb_db_cmpnd " +
					"where db_id = :0";
				DbCommandMx drd = new DbCommandMx();
				drd.PrepareParameterized(sql, DbType.Int64);
				drd.ExecuteReader(databaseId);
				if (!drd.Read() || drd.IsNull(0) || drd.GetLong(0) < 0)
				{
					drd.Dispose();
					return 0;
				}

				long maxId = drd.GetLong(0);
				drd.Dispose();
				return maxId;
			}

			else if (ucdb.CompoundIdType == CompoundIdTypeEnum.String)
			{ // string id
				string sql =
					"select max(to_number(ext_cmpnd_id_txt)) " +
					"from ucdb_owner.ucdb_db_cmpnd " +
					"where db_id = :0 and " +
					"translate(trim(ext_cmpnd_id_nbr),'x.0123456789','x') is null";
				DbCommandMx drd = new DbCommandMx();
				drd.PrepareParameterized(sql, DbType.Int64);
				drd.ExecuteReader(databaseId);
				if (!drd.Read() || drd.IsNull(0) || drd.GetLong(0) < 0)
				{
					drd.Dispose();
					return 0;
				}

				long maxId = drd.GetLong(0);
				drd.Dispose();
				return maxId;
			}

			else throw new Exception("Invalid CompoundIdType");
		}

		/// <summary>
		/// Write message to log with exclusive access
		/// </summary>
		/// <param name="message"></param>

		public void LogMessage(
			string message)
		{
			if (Log == null) return;

			Mutex mutex = new Mutex(false, "UserDataLog");
			mutex.WaitOne(); // get exclusive access
			Log.Message(message);
			mutex.ReleaseMutex();
		}

	}

}
