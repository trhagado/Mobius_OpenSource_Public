using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;

namespace Mobius.UAL
{
	/// <summary>
	/// Annotation Data Warehouse DAO
	/// </summary>
	public class AnnotationDao : GenericDwDao
	{
		public AnnotationDao()
		{
			TableName = "mbs_owner.mbs_adw_rslt";
			SeqName = "mbs_owner.mbs_adw_rslt_seq";
		}

        /// <summary>
        /// Get the number of non-null text, numeric or date values for a given annotation table.
        /// </summary>
        /// <returns></returns>
        public int GetNonNullRsltCnt(int method_vrsn_id)
        {
            string sql = @"select
                            count(rslt_id) as rslt_cnt
                           from MBS_OWNER.mbs_adw_rslt 
                           where 
                             mthd_vrsn_id = :0
                             and sts_id = 1
                             and (rslt_val_nbr is not null
                             or rslt_val_txt is not null
                             or rslt_val_dt is not null)";
                        
            DbCommandMx cmd = new DbCommandMx();
            cmd.Prepare(sql, Oracle.DataAccess.Client.OracleDbType.Int32);            
            cmd.ExecuteReader(method_vrsn_id);
            cmd.Read();
                        
            return cmd.GetInt(0);
                        
        }
        

		/// <summary>
		/// Reorder the rows in all annotation tables that haven't been reordered since last call
		/// </summary>

		public static string ReorderRows(string singleAnnotTableId)
		{
			long methodId = 0, rowCnt = 0, rowCnt2 = 0, totalRows = 0;
			int failCnt = 0, skipCnt = 0;

			Lex lex = new Lex();
			lex.OpenString(singleAnnotTableId);
			string singleMvId = lex.Get(); // item to set

			string sql = @"
				select mthd_vrsn_id, count from 
				(
				select mthd_vrsn_id, count(mthd_vrsn_id) count 
				 FROM MBS_OWNER.mbs_adw_rslt
				 WHERE mthd_vrsn_id > 0 
				 group by mthd_vrsn_id)
				order by mthd_vrsn_id
				";

			if (Lex.IsDefined(singleMvId)) // single annot table
			{
				if (!Lex.IsLong(singleMvId)) throw new ArgumentException();
				sql = Lex.Replace(sql, "> 0", "= " + singleMvId);
			}

			Mobius.UAL.Progress.Show("Getting list of annotation tables to update...");

			DbCommandMx cmd = DbCommandMx.PrepareAndExecuteReader(sql);

			List<long> methodIds = new List<long>();
			List<long> methodRowCounts = new List<long>();

			while (cmd.Read())
			{
				methodId = cmd.GetLong(0);
				methodIds.Add(methodId);

				rowCnt = cmd.GetLong(1);
				methodRowCounts.Add(rowCnt);
			}
			cmd.Dispose();

			totalRows = 0;
			int vmIdCnt = 0;

			for (int mi = 0; mi < methodIds.Count; mi++)
			{
				methodId = methodIds[mi];

				//if (methodId <= 442290) continue; // continue where left off

				rowCnt = methodRowCounts[mi];
				string msg = "Table Id: " + methodId + " (" + (mi+1) + " / " + methodIds.Count + "), Rows: " + rowCnt + ", Total Rows Reordered: " + totalRows + ", Failed: " + failCnt + ", Skipped: " + skipCnt;

				if (rowCnt > 1000000 && Lex.IsUndefined(singleMvId)) // skip if > 1000000 rows and not a specific single annot table
				{
					skipCnt++;
					DebugLog.Message("Skipping " +  msg);
					continue;
				}

				Mobius.UAL.Progress.Show(msg);
				DebugLog.Message(msg);

				try
				{
					rowCnt2 = ReorderRows(methodId);
				}

				catch (Exception ex)
				{
					DebugLog.Message(DebugLog.FormatExceptionMessage(ex));
					failCnt++;
					continue;
				}

				vmIdCnt++;
				totalRows += rowCnt;
			}

			return "Annotation tables reordered: " + vmIdCnt + ", Failed: " + failCnt + ", Skipped: " + skipCnt + ", Total rows reordered: " + totalRows;
		}

		/// <summary>
		/// Reorder the rows in an annotation table
		/// </summary>
		/// <param name="methodId"></param>
		/// <returns></returns>

		public static long ReorderRows(long methodId)
		{
			AnnotationDao dao = null;
			DbCommandMx cmd = null;
			string sql = null, ttName = null;
			int insCnt = 0;

			try
			{

				dao = new AnnotationDao();
				dao.BeginTransaction();
				cmd = dao.DbCmd;

				ttName = "tt_" + methodId;

				try // delete just in case
				{
					cmd.PrepareAndExecuteNonReader("truncate table " + ttName, logExceptions:false);
					cmd.PrepareAndExecuteNonReader("drop table " + ttName, logExceptions: false);
				}

				catch (Exception ex) { ex = ex; }

				// Create temp table & copy data for method

				sql =
					"create global temporary table " + ttName + @"  
					on commit preserve rows
					as select* from mbs_owner.mbs_adw_rslt
						where mthd_vrsn_id = " + methodId;

				cmd.PrepareAndExecuteNonReader(sql);

				// Delete existing data

				sql =
					@"delete from mbs_owner.mbs_adw_rslt
					where mthd_vrsn_id = " + methodId;
				int delCnt = cmd.PrepareAndExecuteNonReader(sql);

				// Reinsert from temp table in desired order into new blocks (e.g. APPEND hint)

				sql =
					@"insert /*+ APPEND */ into mbs_owner.mbs_adw_rslt
					select* from " + ttName + @"
	 			  order by ext_cmpnd_id_nbr, rslt_grp_id, rslt_id";
				insCnt = cmd.PrepareAndExecuteNonReader(sql);

				cmd.Commit(); // commit

				try // remove temp table
				{
					cmd.PrepareAndExecuteNonReader("truncate table " + ttName, logExceptions: false);
					cmd.PrepareAndExecuteNonReader("drop table " + ttName, logExceptions: false);
				}

				catch (Exception ex) { ex = ex; }

				dao.Dispose();
			}

			catch (Exception ex)
			{
				string msg = "Exception for Annotation Table: " + ttName + " ";
				DebugLog.Message(msg + DebugLog.FormatExceptionMessage(ex));

				try
				{
					cmd.Rollback(); // rollback if failed
					cmd.PrepareAndExecuteNonReader("truncate table " + ttName, logExceptions: false);
					cmd.PrepareAndExecuteNonReader("drop table " + ttName, logExceptions: false);
				}
				catch (Exception ex2) { ex2 = ex2; }

				try
				{
					dao.Dispose();
				}
				catch (Exception ex3) { ex3 = ex3; }

				throw new Exception(msg, ex);
			}

			return insCnt;
		}

		/// <summary>
		/// /// <summary>
		/// Copy PRD Annotation Table data to DEV reording and compressing the data by method
		/// </summary>
		/// <returns></returns>

		public static string CopyPrdAnnotationTableDataToDev()
		{
			string mvidSql = @"
select mthd_vrsn_id, count from 
(
select mthd_vrsn_id, count(mthd_vrsn_id) count 
 FROM MBS_OWNER.mbs_adw_rslt
 WHERE mthd_vrsn_id > 0 /* = 708070 */
 group by mthd_vrsn_id)
order by mthd_vrsn_id
";

			Mobius.UAL.Progress.Show("Getting list of annotation table mtht_vrsn_ids ...");
			DbCommandMx c1 = new DbCommandMx();
			c1.Prepare(mvidSql);
			c1.ExecuteReader();

			DbCommandMx cmd = new DbCommandMx();
			cmd.MxConn = DbConnectionMx.GetConnection("DEV857");
			cmd.BeginTransaction();
			long totalIns = 0;
			int vmIdCnt = 0;

			while (c1.Read())
			{
				long mvId = c1.GetLong(0);

				string sql =
	@"insert /*+ APPEND */ into mbs_owner.mbs_adw_rslt 
  select * from mbs_owner.mbs_adw_rslt
  where mthd_vrsn_id = " + mvId + @" 
  order by ext_cmpnd_id_nbr, rslt_grp_id, rslt_id";
				cmd.PrepareUsingDefinedConnection(sql);
				int insCnt = cmd.ExecuteNonReader();
				cmd.Commit();
				totalIns += insCnt;
				vmIdCnt++;
				string msg = "Mthd_Vrsn_id: " + mvId + ", Vmids: " + vmIdCnt + ", Rows: " + totalIns;
				Mobius.UAL.Progress.Show(msg);
				DebugLog.Message(msg);
			}

			c1.Dispose();

			cmd.Dispose();
			return "Rows copied: " + totalIns;
		}

		/// <summary>
		/// Copy PRD Annotation Table data to DEV by groups of result ids
		/// </summary>
		/// <returns></returns>

		public static string CopyPrdAnnotationTableDataToDevBy_Rslt_Id()
		{
			long minRi = 1;
			//long minRi = 1310251601; // appro half of rows, allows index on all cols to be built
			long maxRi = 4331096190;
			long chunkSize = 100000;

			DbCommandMx cmd = new DbCommandMx();
			cmd.MxConn = DbConnectionMx.GetConnection("DEV857");
			cmd.BeginTransaction();

			long totalIns = 0;
			for (long ri = minRi; ri <= maxRi; ri += chunkSize)
			{
				string range = (ri + 1).ToString() + " and " + (ri + chunkSize);
				string sql =
	@"insert /*+ APPEND */ into mbs_owner.mbs_adw_rslt 
  (select * from mbs_owner.mbs_adw_rslt
  where rslt_id between " + range + ")";
				cmd.PrepareUsingDefinedConnection(sql);
				int insCnt = cmd.ExecuteNonReader();
				cmd.Commit();
				totalIns += insCnt;
				string msg = "Rows copied: " + totalIns + " (" + range + ")";
				Mobius.UAL.Progress.Show(msg);
				DebugLog.Message(msg);
			}

			cmd.Dispose();
			return "Rows copied: " + totalIns;
		}

		/// <summary>
		/// Perform a deep clone of an annotation table UserObject including the underlying data
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static UserObject DeepClone(UserObject uo)
		{
			UserObject uo2 = uo.Clone();
			long oldCode, newCode;

			// Create a copy of the user object including remapping of the codes

			AnnotationDao dao = new AnnotationDao();
			MetaTable mt = MetaTable.Deserialize(uo.Content); // deserialize metatable xml

			int newMethodCode = UserObjectDao.GetNextId();
			uo2.Id = newMethodCode; // store in UserObject id as well
			mt.Code = newMethodCode.ToString();
			mt.Name = "ANNOTATION_" + mt.Code;
			mt.Label = uo.Name;

			Dictionary<long, long> codeMap = new Dictionary<long, long>();
			foreach (MetaColumn mc in mt.MetaColumns)
			{
				if (!Lex.IsNullOrEmpty(mc.ResultCode))
				{
					if (!long.TryParse(mc.ResultCode, out oldCode)) continue;
					newCode = dao.GetNextIdLong();
					codeMap[oldCode] = newCode;
					mc.Name = "R_" + newCode;
					mc.ResultCode = newCode.ToString();
				}
			}

			// Write import state if checking for updates

			if (mt.ImportParms != null && mt.ImportParms.CheckForFileUpdates)
			{
				UserObject udisUo = new UserObject(UserObjectType.ImportState, uo2.Owner, mt.Name);
				UserDataImportState udis = new UserDataImportState();

				udis.UserDatabase = false; // indicate annotation table
				udis.UserDataObjectId = uo2.Id; // store id of annotation table user object
				udis.ClientFile = mt.ImportParms.FileName;
				udis.CheckForFileUpdates = mt.ImportParms.CheckForFileUpdates;
				udis.ClientFileModified = mt.ImportParms.ClientFileModified;
				udis.FileName = mt.ImportParms.FileName;
				udisUo.Description = udis.Serialize(); // serialize to description

				UserObjectDao.Write(udisUo);
			}

			// Copy the data

			dao.BeginTransaction();
			dao.BufferInserts(true);
			dao.OpenReader(uo.Id);

			Dictionary<long, long> groupMap = new Dictionary<long, long>();
			int readCount = 0;
			while (true)
			{
				AnnotationVo vo = dao.Read();
				if (vo == null) break;

				if (!codeMap.ContainsKey(vo.rslt_typ_id)) continue;
				vo.rslt_typ_id = codeMap[vo.rslt_typ_id]; // map the result code

				vo.rslt_id = dao.GetNextIdLong(); // new result id

				vo.mthd_vrsn_id = newMethodCode;

				if (!groupMap.ContainsKey(vo.rslt_grp_id)) // map the group id
					groupMap[vo.rslt_grp_id] = dao.GetNextIdLong();
				vo.rslt_grp_id = groupMap[vo.rslt_grp_id];

				dao.Insert(vo);
				readCount++;
				if (readCount % 1000 == 0)
				{
					dao.ExecuteBufferedInserts();
					dao.Commit();
				}
			}

			dao.ExecuteBufferedInserts();
			dao.Commit();
			dao.Dispose();

			uo2.Count = groupMap.Count; // update the count
			uo2.CreationDateTime = uo2.UpdateDateTime = DateTime.Now;
			uo2.Content = mt.Serialize();
			return uo2;
		}

	}
}
