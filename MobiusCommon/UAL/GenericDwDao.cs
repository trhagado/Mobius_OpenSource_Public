using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace Mobius.UAL
{
	/// <summary>
	/// Generic Data Warehouse DAO
	/// </summary>

	public class GenericDwDao
	{
		protected string TableName = ""; // owner-qualified name of table containing data
		protected string SeqName = ""; // owner-qualified name of sequence to use
		public DbCommandMx DbCmd; // expose DataReaderDao to allow transaction control
		List<AnnotationVo> InsertBuffer = null; // rows buffered waiting for insert

		public GenericDwDao()
		{
			DbCmd = new DbCommandMx();
			return;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tableName"></param>
		public GenericDwDao(
			string tableName,
			string seqName)
		{
			DbCmd = new DbCommandMx();
			TableName = tableName;
			SeqName = seqName;
		}

		/// <summary>
		/// Establish Oracle connection for the Dao instance
		/// </summary>

		public void Connect()
		{
			DbCommandMx acd = new DbCommandMx();
			this.DbCmd = acd;
			string tName = TableName;
			acd.MxConn = DbConnectionMx.MapSqlToConnection(ref tName);
		}

		/// <summary>
		/// Begin a transaction
		/// </summary>

		public void BeginTransaction()
		{
			if (DbCmd == null || DbCmd.MxConn == null || DbCmd.MxConn.DbConn == null) Connect(); // be sure we have a connection
			DbCmd.BeginTransaction();
			return;
		}

		/// <summary>
		/// Commit transaction
		/// </summary>

		public void Commit()
		{
			if (DbCmd != null) DbCmd.Commit();
		}

		/// <summary>
		/// Select all rows for a method and compoundId
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <param name="cmpndId"></param>
		/// <returns></returns>

		public List<AnnotationVo> Select(
			int mthdVrsnId,
			string cmpndId)
		{
			OpenReader(mthdVrsnId, cmpndId);

			List<AnnotationVo> rows = new List<AnnotationVo>();
			int readCount = 0;
			while (true)
			{
				AnnotationVo vo = Read();
				if (vo == null) break;
				rows.Add(vo);
				readCount++;
			}

			Close();

			if (readCount == 0) return null;
			else return rows;
		}

		/// <summary>
		/// Open reader
		/// </summary>
		/// <param name="mthdVrsnId"></param>

		public void OpenReader(
			int mthdVrsnId)
		{
			OpenReader(mthdVrsnId, null);
		}

		/// <summary>
		/// Open reader
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <param name="cmpndId"></param>

		public void OpenReader(
			int mthdVrsnId,
			string cmpndId)
		{
			OracleDbType[] pa;
			object[] p;

			string sql =
				"select " +
				"rslt_id, rslt_grp_id, ext_cmpnd_id_txt, ext_cmpnd_id_nbr," +
				"src_db_id, mthd_vrsn_id, rslt_typ_id, rslt_val_prfx_txt," +
				"rslt_val_nbr, uom_id, rslt_val_txt, rslt_val_dt," +
				"cmnt_txt, dc_lnk, chng_op_cd, chng_usr_id " +
				"from " + TableName + " ";

			if (cmpndId != null) // method & compound specified
			{
				sql +=
				"where mthd_vrsn_id = :0" +
				" and ext_cmpnd_id_txt = :1 " +
				" and sts_id = 1";

				pa = new OracleDbType[2];
				pa[0] = OracleDbType.Int32;
				pa[1] = OracleDbType.Varchar2;

				p = new object[2];
				p[0] = mthdVrsnId;
				p[1] = cmpndId;
			}

			else // only method specified
			{
				sql +=
				"where mthd_vrsn_id = :0" +
				" and sts_id = 1";

				pa = new OracleDbType[1];
				pa[0] = OracleDbType.Int32;

				p = new object[1];
				p[0] = mthdVrsnId;
			}

			DbCmd.Prepare(sql, pa);
			DbCmd.ExecuteReader(p);
			return;
		}

		/// <summary>
		/// Read next record
		/// </summary>
		/// <returns></returns>

		public AnnotationVo Read()
		{
			if (!DbCmd.Read()) return null;

			AnnotationVo vo = new AnnotationVo();
			vo.rslt_id = DbCmd.GetLong(0);
			vo.rslt_grp_id = DbCmd.GetLong(1);
			vo.ext_cmpnd_id_txt = DbCmd.GetString(2);
			vo.ext_cmpnd_id_nbr = DbCmd.GetInt(3);
			vo.src_db_id = DbCmd.GetInt(4);
			vo.mthd_vrsn_id = DbCmd.GetInt(5);
			vo.rslt_typ_id = DbCmd.GetLong(6);
			vo.rslt_val_prfx_txt = DbCmd.GetString(7);
			vo.rslt_val_nbr = DbCmd.GetDouble(8);
			vo.uom_id = DbCmd.GetInt(9);
			vo.rslt_val_txt = DbCmd.GetString(10);
			vo.rslt_val_dt = DbCmd.GetDateTime(11);
			vo.cmnt_txt = DbCmd.GetString(12);
			vo.dc_lnk = DbCmd.GetString(13);
			vo.chng_op_cd = DbCmd.GetString(14);
			vo.chng_usr_id = DbCmd.GetString(15);
			return vo;
		}

		/// <summary>
		/// Close reader
		/// </summary>

		public void Close()
		{
			DbCmd.CloseReader();
			DbCmd.Dispose();
		}



		/// <summary>
		/// Select row for specified result id
		/// </summary>
		/// <param name="rsltId"></param>
		/// <returns></returns>

		public AnnotationVo Select(
			long rsltId)
		{
			string sql =
				"select " +
				"rslt_id, rslt_grp_id, ext_cmpnd_id_txt, ext_cmpnd_id_nbr," +
				"src_db_id, mthd_vrsn_id, rslt_typ_id, rslt_val_prfx_txt," +
				"rslt_val_nbr, uom_id, rslt_val_txt, rslt_val_dt," +
				"cmnt_txt, dc_lnk, chng_op_cd, chng_usr_id " +
				"from " + TableName + " " +
				"where rslt_id = :0";

			DbCmd.Prepare(sql, OracleDbType.Long);
			DbCmd.ExecuteReader(rsltId);
			if (!DbCmd.Read()) return null;

			AnnotationVo vo = new AnnotationVo();

			vo.rslt_id = DbCmd.GetLong(0);
			vo.rslt_grp_id = DbCmd.GetLong(1);
			vo.ext_cmpnd_id_txt = DbCmd.GetString(2);
			vo.ext_cmpnd_id_nbr = DbCmd.GetInt(3);
			vo.src_db_id = DbCmd.GetInt(4);
			vo.mthd_vrsn_id = DbCmd.GetInt(5);
			vo.rslt_typ_id = DbCmd.GetLong(6);
			vo.rslt_val_prfx_txt = DbCmd.GetString(7);
			vo.rslt_val_nbr = DbCmd.GetDouble(8);
			vo.uom_id = DbCmd.GetInt(9);
			vo.rslt_val_txt = DbCmd.GetString(10);
			vo.rslt_val_dt = DbCmd.GetDateTime(11);
			vo.cmnt_txt = DbCmd.GetString(12);
			vo.dc_lnk = DbCmd.GetString(13);
			vo.chng_op_cd = DbCmd.GetString(14);
			vo.chng_usr_id = DbCmd.GetString(15);

			return vo;
		}

		/// <summary>
		/// Get string list of existing compound ids for method
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <returns></returns>

		public List<string> SelectExtStringCids(
			int mthdVrsnId)
		{
			string sql =
				"select ext_cmpnd_id_txt " +
				"from " + TableName + " " +
				"where mthd_vrsn_id = :0" +
				" and sts_id = 1 " +
				" order by ext_cmpnd_id_txt";

			DbCmd.Prepare(sql, OracleDbType.Int32);
			DbCmd.ExecuteReader(mthdVrsnId);

			List<string> cids = new List<string>();
			while (true)
			{
				if (!DbCmd.Read()) break;
				cids.Add(DbCmd.GetString(0));
			}
			return cids;
		}

		/// <summary>
		/// Get compound count for method
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <returns></returns>

		public int SelectCompoundCount(
			int mthdVrsnId)
		{
			string sql =
				"select count(distinct ext_cmpnd_id_txt) " +
				"from " + TableName + " " +
				"where mthd_vrsn_id = :0" +
				" and sts_id = 1";

			DbCmd.Prepare(sql, OracleDbType.Int32);
			DbCmd.ExecuteReader(mthdVrsnId);

			DbCmd.Read();
			return DbCmd.GetInt(0);
		}

		/// <summary>
		///	Get row count for method
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <returns></returns>

		public int SelectRowCount(
			int mthdVrsnId)
		{
			string sql =
				"select count(distinct rslt_grp_id) " +
				"from " + TableName + " " +
				"where mthd_vrsn_id = :0" +
				" and sts_id = 1";

			DbCmd.Prepare(sql, OracleDbType.Int32);
			DbCmd.ExecuteReader(mthdVrsnId);

			DbCmd.Read();
			return DbCmd.GetInt(0);
		}

		/// <summary>
		/// Turn insert buffering on or off
		/// </summary>
		/// <param name="bufferInserts"></param>
		/// <returns></returns>

		public void BufferInserts(
			bool bufferInserts)
		{
			if (bufferInserts)
			{
				if (InsertBuffer == null) InsertBuffer = new List<AnnotationVo>();
			}

			else
			{
				if (InsertBuffer != null && InsertBuffer.Count > 0)
					throw new Exception("Can't turn off insert buffering with rows in buffer");
				InsertBuffer = null;
			}
		}


		/// <summary>
		/// Execute buffered inserts
		/// </summary>
		/// <returns></returns>

		public int ExecuteBufferedInserts()
		{
			if (InsertBuffer == null || InsertBuffer.Count == 0) return 0;
			int count = InsertBuffer.Count;
			if (!Insert(InsertBuffer)) count = 0;
			InsertBuffer.Clear();
			return count;
		}

		/// <summary>
		/// Insert Mobius warehouse row filling common parameters from var
		/// </summary>
		/// <param name="vo"></param>
		/// <param name="cid"></param>
		/// <param name="mt"></param>
		/// <param name="mc"></param>
		/// <returns></returns>

		public bool Insert(
			AnnotationVo vo,
			string cid,
			int methodVersionId,
			long resultTypeId)
		{
			vo.ext_cmpnd_id_txt = cid; // compound id
			vo.mthd_vrsn_id = methodVersionId;
			vo.rslt_typ_id = resultTypeId;
			vo.chng_op_cd = "I"; // this is an insert
			vo.chng_usr_id = Security.UserInfo.UserName;
			return Insert(vo);
		}

		/// <summary>
		/// Insert row
		/// </summary>
		/// <param name="vo"></param>
		/// <returns></returns>

		public bool Insert(
			AnnotationVo vo)
		{
			if (vo.rslt_val_nbr == NullValue.NullNumber &&
				(vo.rslt_val_txt == null || vo.rslt_val_txt == "") &&
				vo.rslt_val_dt == DateTime.MinValue)
				return false; // don't allow insert of null values (can't update since not retrieved)

			if (InsertBuffer == null) // single insert
			{
				List<AnnotationVo> voList = new List<AnnotationVo>();
				voList.Add(vo);
				return Insert(voList);
			}

			else // buffered batch
			{
				InsertBuffer.Add(vo.Clone());
				return true;
			}
		}

		/// <summary>
		/// Fast insert/update of a single row including creation of the AnnotationDao object, transaction and header update
		/// </summary>
		/// <param name="vo"></param>
		/// <returns></returns>

		public static long InsertUpdateRowAndUserObjectHeader(AnnotationVo vo)
		{
			bool newRow = vo.rslt_id <= 0;
			AnnotationDao dao = new AnnotationDao();
			dao.BeginTransaction();
			long rsltId = dao.InsertUpdateRow(vo);
			dao.Commit();
			dao.Dispose();

			UserObject uo = UserObjectDao.ReadHeader(vo.mthd_vrsn_id);
			if (uo != null)
			{
				uo.UpdateDateTime = DateTime.Now;
				if (newRow) uo.Count++;
				UserObjectDao.UpdateHeader(uo);
			}
			return rsltId;
		}

		/// <summary>
		/// Insert/update a single result row
		/// </summary>
		/// <param name="vo"></param>
		/// <returns></returns>

		public long InsertUpdateRow(AnnotationVo vo)
		{
			bool existingVal = vo.rslt_id > 0 && vo.rslt_grp_id == -1;

			if (existingVal)
				SetArchiveStatus(vo.rslt_id); // archive existing val

			if (vo.rslt_id > 0) // get existing group
			{
				AnnotationVo vo2 = Select(vo.rslt_id);
				if (vo2 != null) vo.rslt_grp_id = vo2.rslt_grp_id;
			}

			if (vo.rslt_grp_id <= 0) vo.rslt_grp_id = GetNextIdLong();

			vo.rslt_id = GetNextIdLong();
			List<AnnotationVo> voList = new List<AnnotationVo>();
			voList.Add(vo);
			if (Insert(voList))
				return vo.rslt_id;
			else return -1;
		}

		/// <summary>
		/// Insert an array of rows into Mobius warehouse table
		/// </summary>
		/// <param name="voList"></param>
		/// <returns></returns>

		public bool Insert(
			List<AnnotationVo> voList)
		{

	// This insert uses the APPEND_VALUES hint if there are 10 or more rows inserted.
	// This means that inserts will go into new blocks and be physically associated with each other
	// which will result in significantly faster retrieval for individual annotation tables since fewer
	// disk reads will be needed. Otherwise tables that are reloaded multiple times will tend to be spread over
	// a larger number of reused blocks resulting in more reads and slower performance.
	//
	// From a web article:
	//  Each of the following is a benefit in some cases
	//  Each of the following is a disaster in other cases
	//  Append does a direct path load (if it can, it is not a promise, you are requesting and we may or may not do it for you - silently)
	//  if you direct path load, the transaction that did the direct path load CANNOT query that segment -but other transactions can, they just cannot see the newly loaded data. 
	//  if you direct path load, you never use any existing free space, it always writes above the high water mark. 
	//  if you direct path load, we bypass UNDO on the table -only the table -modifications
	//  if you direct path load, you'll maintain indexes - we build mini indexes on the newly loaded data and merge them into the 'real' indexes in bulk. A direct path load of large amounts of data will maintain indexes very efficiently. 
	//  if you direct path load you can bypass redo on the TABLE in archivelog mode, if the database is set up to allow nologging and you have the segment set to nologging
	//  direct path loading bypasses the buffer cache, you write directly to the datafiles. 
	//  direct path loading is only appropriate for the first load of a segment or an increment load of lots of data - or an increment load into a table that never has any deletes(so there is no free space to consider)
	// 
	//  transactional systems - you probably won't use it. 
	//  warehouses - a tool you'll use a lot 

			AnnotationVo vo;

			if (voList == null || voList.Count == 0) return false;

			//CheckForDuplicateInsert(voList); // debug

			try
			{
				string sql =
				 "insert /*+ APPEND_VALUES */ into " + TableName + " " +
					"(rslt_id, " +
					"rslt_grp_id, " +
					"ext_cmpnd_id_txt, " +
					"ext_cmpnd_id_nbr, " +
					"src_db_id, " +
					"mthd_vrsn_id, " +
					"rslt_typ_id, " +
					"rslt_val_prfx_txt, " +
					"rslt_val_nbr, " +
					"uom_id, " +
					"rslt_val_txt, " +
					"rslt_val_dt, " +
					"cmnt_txt, " +
					"dc_lnk, " +
					"chng_op_cd, " +
					"chng_usr_id, " +
					"sts_id, " +
					"sts_dt, " +
					"crt_dt, " +
					"updt_dt) " +
				 "values (nvl(:0," + SeqName + ".nextval)" + // if rslt_id not null use it otherwise call nextval locally
					",:1,:2,:3,:4,:5,:6,:7,:8,:9,:10,:11,:12,:13,:14,:15,1,sysdate,sysdate,sysdate)";

				if (voList.Count < 10) // require minimum number of rows to use APPEND_VALUESd
					sql = Lex.Replace(sql, "APPEND_VALUES", "");

				OracleDbType[] pa = new OracleDbType[16];
				pa[0] = OracleDbType.Long; // rslt_id
				pa[1] = OracleDbType.Long; // rslt_grp_id
				pa[2] = OracleDbType.Varchar2;
				pa[3] = OracleDbType.Int32;
				pa[4] = OracleDbType.Int32;
				pa[5] = OracleDbType.Int32; // mthd_vrsn_id
				pa[6] = OracleDbType.Long; // rslt_typ_id
				pa[7] = OracleDbType.Varchar2;
				pa[8] = OracleDbType.Double;
				pa[9] = OracleDbType.Int32;
				pa[10] = OracleDbType.Varchar2;
				pa[11] = OracleDbType.Date;
				pa[12] = OracleDbType.Varchar2;
				pa[13] = OracleDbType.Varchar2;
				pa[14] = OracleDbType.Varchar2;
				pa[15] = OracleDbType.Varchar2;

				DbCmd.Prepare(sql, pa);

				int cnt = voList.Count;

				object[] p = new object[16]; // parameter values
				object[] rslt_idA = new object[cnt]; p[0] = rslt_idA; // allocate arrays to hold values
				long[] rslt_grp_idA = new long[cnt]; p[1] = rslt_grp_idA;
				string[] ext_cmpnd_id_txtA = new string[cnt]; p[2] = ext_cmpnd_id_txtA;
				int[] ext_cmpnd_id_nbrA = new int[cnt]; p[3] = ext_cmpnd_id_nbrA;
				int[] src_db_idA = new int[cnt]; p[4] = src_db_idA;
				int[] mthd_vrsn_idA = new int[cnt]; p[5] = mthd_vrsn_idA;
				long[] rslt_typ_idA = new long[cnt]; p[6] = rslt_typ_idA;
				string[] rslt_val_prfx_txtA = new string[cnt]; p[7] = rslt_val_prfx_txtA;
				OracleDecimal[] rslt_val_nbrA = new OracleDecimal[cnt]; p[8] = rslt_val_nbrA;
				int[] uom_idA = new int[cnt]; p[9] = uom_idA;
				string[] rslt_val_txtA = new string[cnt]; p[10] = rslt_val_txtA;
				OracleDate[] rslt_val_dtA = new OracleDate[cnt]; p[11] = rslt_val_dtA;
				string[] cmnt_txtA = new string[cnt]; p[12] = cmnt_txtA;
				string[] dc_lnkA = new string[cnt]; p[13] = dc_lnkA;
				string[] chng_op_cdA = new string[cnt]; p[14] = chng_op_cdA;
				string[] chng_usr_idA = new string[cnt]; p[15] = chng_usr_idA;

				for (int li = 0; li < cnt; li++)
				{ // copy values to parameter arrays
					vo = voList[li];

					try { vo.ext_cmpnd_id_nbr = Int32.Parse(vo.ext_cmpnd_id_txt); } // try to store text ext_cmpnd_id_txt value also as integer in ext_cmpnd_id_nbr
					catch (Exception ex) { }

					if (vo.rslt_id <= 0 && voList.Count == 1) // assign seq no if not already assigned if inserting only 1 row
						vo.rslt_id = SequenceDao.NextValLong(SeqName);
					if (vo.rslt_id > 0) rslt_idA[li] = vo.rslt_id; // is result_id defined?
					else rslt_idA[li] = DBNull.Value; // if not defined send as null value so sequence is used in nvl function in insert

					rslt_grp_idA[li] = vo.rslt_grp_id;

					string txt = vo.ext_cmpnd_id_txt;
					if (txt != null && txt.Length > 32) // truncate to 32 chars if needed
						txt = txt.Substring(0, 32);
					ext_cmpnd_id_txtA[li] = txt;

					ext_cmpnd_id_nbrA[li] = vo.ext_cmpnd_id_nbr;
					src_db_idA[li] = vo.src_db_id;
					mthd_vrsn_idA[li] = vo.mthd_vrsn_id;
					rslt_typ_idA[li] = vo.rslt_typ_id;
					rslt_val_prfx_txtA[li] = vo.rslt_val_prfx_txt;
					if (vo.rslt_val_nbr != NullValue.NullNumber)
						rslt_val_nbrA[li] = new OracleDecimal(vo.rslt_val_nbr);
					uom_idA[li] = vo.uom_id;

					rslt_val_txtA[li] = vo.rslt_val_txt;
					if (rslt_val_txtA[li] != null && rslt_val_txtA[li].Length > 3900) // avoid overflow error, must leave space for catenating 
						rslt_val_txtA[li] = rslt_val_txtA[li].Substring(0, 3897) + "..."; // link info & keeping total <= 4000

					if (vo.rslt_val_txt.Contains(",")) vo.rslt_val_txt = vo.rslt_val_txt; // debug

					if (vo.rslt_val_dt != DateTime.MinValue) // non-null date?
						rslt_val_dtA[li] = new OracleDate(vo.rslt_val_dt);
					cmnt_txtA[li] = vo.cmnt_txt;
					dc_lnkA[li] = vo.dc_lnk;
					chng_op_cdA[li] = vo.chng_op_cd;
					chng_usr_idA[li] = vo.chng_usr_id;
				}

				int t0 = TimeOfDay.Milliseconds();
				DbCmd.OracleCmd.ArrayBindCount = cnt;
				int count = DbCmd.ExecuteNonReader(p); // do insert

				t0 = TimeOfDay.Milliseconds() - t0;
				//				DebugLog.Message("MobiusDwDao insert rows, count = " + count.ToString() + ", Time(ms) = " + t0.ToString());

				return true;
			}

			catch (Exception e)
			{
				DebugLog.Message("MobiusDwDao.Insert - Error inserting into " + TableName + ": " + e.Message);
				return false;
			}
		}

		/// <summary>
		/// Debug check for disallowed duplicate insert for a given method, cid, result group and result type
		/// </summary>
		/// <param name="voList"></param>
		/// <returns></returns>

		bool CheckForDuplicateInsert(List<AnnotationVo> voList)
		{
			AnnotationVo vo = voList[0];
			string sql0 = // see if this result value already exists 
				"SELECT * " +
				" FROM mbs_owner.MBS_ADW_RSLT " +
				" WHERE mthd_vrsn_id = " + vo.mthd_vrsn_id +
				" and ext_cmpnd_id_nbr = " + vo.ext_cmpnd_id_nbr +
				" and rslt_grp_id = " + vo.rslt_grp_id +
				" and rslt_typ_id = " + vo.rslt_typ_id +
				" and sts_id = 1";

			DbCommandMx d0 = new DbCommandMx();
			d0.Prepare(sql0);
			d0.ExecuteReader();
			bool exists = d0.Read();
			d0.Dispose();
			if (exists) return true;
			else return false;
		}

		/// <summary>
		/// Update a compoundId for a table
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <param name="cmpndId"></param>
		/// <returns></returns>

		public int UpdateCid(
			int mthdVrsnId,
			string oldCid,
			string newCid)
		{
			string sql;

			int sbstncId;
			sql = // Note: paramters must be in increasing order
				"update " + TableName + " " +
				"set ext_cmpnd_id_txt = :0, ext_cmpnd_id_nbr = :1 " +
				"where mthd_vrsn_id = :2" +
				" and ext_cmpnd_id_txt = :3";

			OracleDbType[] pa = new OracleDbType[4];
			pa[0] = OracleDbType.Varchar2;
			pa[1] = OracleDbType.Int32;
			pa[2] = OracleDbType.Int32;
			pa[3] = OracleDbType.Varchar2;

			DbCmd.Prepare(sql, pa);

			object[] p = new object[4];
			p[0] = newCid;
			if (Int32.TryParse(newCid, out sbstncId)) // try to store text ext_cmpnd_id_txt value also as integer in ext_cmpnd_id_nbr
				p[1] = sbstncId;
			p[2] = mthdVrsnId;
			p[3] = oldCid;
			int cnt = DbCmd.ExecuteNonReader(p);
			return cnt;
		}

		/// <summary>
		/// Update a compoundId for a resultGroup
		/// </summary>
		/// <param name="rsltGrpId"></param>
		/// <param name="cmpndId"></param>
		/// <returns></returns>

		public int UpdateResultGroupCid(
			long rsltGrpId,
			string oldCid,
			string newCid)
		{
			string sql;

			int sbstncId;
			sql = // Note: paramters must be in increasing order
				"update " + TableName + " " +
				"set ext_cmpnd_id_txt = :0, ext_cmpnd_id_nbr = :1 " +
				"where rslt_grp_id = :2";

			OracleDbType[] pa = new OracleDbType[3];
			pa[0] = OracleDbType.Varchar2;
			pa[1] = OracleDbType.Int32;
			pa[2] = OracleDbType.Long;

			DbCmd.Prepare(sql, pa);

			object[] p = new object[3];
			p[0] = newCid;
			if (Int32.TryParse(newCid, out sbstncId)) // try to store text ext_cmpnd_id_txt value also as integer in ext_cmpnd_id_nbr
				p[1] = sbstncId;
			p[2] = rsltGrpId;
			int cnt = DbCmd.ExecuteNonReader(p);
			return cnt;
		}

		/// <summary>
		/// Set archive status for a single result row
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <param name="cmpndId"></param>
		/// <returns></returns>

		public int SetArchiveStatus(
			long rsltId)
		{
			string sql = // archive any active data for result
				"update " + TableName + " " +
				"set sts_id = 2, sts_dt = sysdate " +
				"where rslt_id = :0";

			DbCmd.Prepare(sql, OracleDbType.Long);
			int cnt = DbCmd.ExecuteNonReader(rsltId);
			return cnt;
		}

		/// <summary>
		/// Delete a single result row
		/// </summary>
		/// <param name="rsltId"></param>
		/// <returns></returns>

		public int Delete(
			long rsltId)
		{
			string sql =
				"delete " + TableName + " " +
				"where rslt_id = :0";

			DbCmd.Prepare(sql, OracleDbType.Long);
			int cnt = DbCmd.ExecuteNonReader(rsltId);
			return cnt;
		}

		/// <summary>
		/// Set archive status for all rows in a result group
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <param name="cmpndId"></param>
		/// <returns></returns>

		public int SetResultGroupArchiveStatus(
			long rsltGrpId)
		{
			string sql = // archive any active data for result group
				"update " + TableName + " " +
				"set sts_id = 2, sts_dt = sysdate " +
				"where rslt_grp_id = :0";

			DbCmd.Prepare(sql, OracleDbType.Long);
			int cnt = DbCmd.ExecuteNonReader(rsltGrpId);
			return cnt;
		}

		/// <summary>
		/// Delete all rows for a result group
		/// </summary>
		/// <param name="rsltId"></param>
		/// <returns></returns>

		public int DeleteResultGroup(
			long rsltGrpId)
		{
			string sql =
				"delete " + TableName + " " +
				"where rslt_grp_id = :0";

			DbCmd.Prepare(sql, OracleDbType.Long);
			int cnt = DbCmd.ExecuteNonReader(rsltGrpId);
			return cnt;
		}

		/// <summary>
		/// Set archive status for all data for compound
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <param name="cmpndId"></param>
		/// <returns></returns>
		public int SetCompoundArchiveStatus(
			int mthdVrsnId,
			string cmpndId)
		{
			string sql = // archive any active data for cmpd
				"update " + TableName + " " +
				"set sts_id = 2, sts_dt = sysdate " +
				"where ext_cmpnd_id_txt = :0 and mthd_vrsn_id = :1 and sts_id = 1";

			OracleDbType[] pa = new OracleDbType[2];
			pa[0] = OracleDbType.Varchar2;
			pa[1] = OracleDbType.Int32;

			DbCmd.Prepare(sql, pa);

			object[] p = new object[2];
			p[0] = cmpndId;
			p[1] = mthdVrsnId;

			int cnt = DbCmd.ExecuteNonReader(p);
			return cnt;
		}

		/// <summary>
		/// Delete data in a metatable for a compound
		/// </summary>
		/// <param name="cmpndId"></param>
		/// <param name="mthdVrsnId"></param>
		/// <returns></returns>

		public int DeleteCompound(
			int mthdVrsnId,
			string cmpndId)
		{
			string sql =
				"delete " + TableName + " " +
				"where mthd_vrsn_id = :0" +
				" and ext_cmpnd_id_txt = :1";

			OracleDbType[] pa = new OracleDbType[2];
			pa[0] = OracleDbType.Int32;
			pa[1] = OracleDbType.Varchar2;

			DbCmd.Prepare(sql, pa);

			object[] p = new object[2];
			p[0] = mthdVrsnId;
			p[1] = cmpndId;

			int cnt = DbCmd.ExecuteNonReader(p);
			return cnt;
		}

		/// <summary>
		/// Change the archive status for all data for a warehouse method table
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <returns></returns>

		public int SetTableArchiveStatus(
			int mthdVrsnId)
		{
			string sql = // archive any active data for cmpd
				"update " + TableName + " " +
				"set sts_id = 2, sts_dt = sysdate " +
				"where mthd_vrsn_id = :0 and sts_id = 1";

			DbCmd.Prepare(sql, OracleDbType.Int32);

			int cnt = DbCmd.ExecuteNonReader(mthdVrsnId);
			return cnt;
		}

		/// <summary>
		/// Permanently delete all data for a warehouse method table
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <returns></returns>

		public int DeleteTable(
			int mthdVrsnId)
		{
			bool useTransactions = true; // use transactions to avoid rollback segment overflow
			return DeleteTable(mthdVrsnId, useTransactions);
		}

		/// <summary>
		/// Permanently delete all data for a warehouse method table
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <param name="useTransactions"></param>
		/// <param name="showProgressMsg"></param>
		/// <returns></returns>

		public int DeleteTable(
			int mthdVrsnId,
			bool useTransactions)
		{

			int deleteCount = 0;
			string sql =
			"delete /*+ rule */ " + // rule hint seems to work much better than no hint or first_rows hint.
			"from " + TableName + " " +
			"where mthd_vrsn_id = " + mthdVrsnId.ToString();
			if (useTransactions) // do limited number of rows at a time so we can update user & avoid rollback segment overflow
				sql += " and rownum <= 1000";

			DbCmd.Prepare(sql);
			if (useTransactions)
				DbCmd.BeginTransaction(); // be sure we have a transaction going


			int lastCheckTime = 0;
			while (true)
			{
				int t0 = TimeOfDay.Milliseconds();
				int count = DbCmd.ExecuteNonReader();
				int t1 = TimeOfDay.Milliseconds();
				t0 = t1 - t0;
				if (count == 0) break;
				deleteCount += count;

				if (t1 - lastCheckTime >= 1000) // check every sec
				{
					lastCheckTime = t1;
					if (useTransactions) Commit();
				}
			}

			if (useTransactions) Commit();

			return deleteCount;
		}

		/// <summary>
		/// Get a block of new row ids
		/// </summary>
		/// <param name="idCount"></param>
		/// <returns></returns>

		public long[] GetNextIdsLong(int idCount)
		{
			SequenceDao.SetCacheSize(SeqName, idCount);
			long[] ids = new long[idCount];

			for (int i1 = 0; i1 < idCount; i1++)
				ids[i1] = GetNextIdLong();

			SequenceDao.SetCacheSize(SeqName, 1); // reset
			return ids;
		}

		/// <summary>
		/// Set size for cache
		/// </summary>
		/// <param name="cacheSize"></param>

		public void SetSequenceCacheSize(
			int cacheSize)
		{
			SequenceDao.SetCacheSize(SeqName, cacheSize);
		}

		/// <summary>
		/// Get next identifier from sequence
		/// </summary>
		/// <returns></returns>

		public long GetNextIdLong()
		{
			long id = SequenceDao.NextValLong(SeqName);
			return id;
		}

		/// <summary>
		/// Dispose of any associated DataReaderDao
		/// </summary>

		public void Dispose()
		{
			if (DbCmd == null) return;

			DbCmd.Dispose();
			DbCmd = null;
		}
	}
}
