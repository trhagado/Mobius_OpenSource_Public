using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mobius.MetaFactoryNamespace
{
	/// <summary>
	/// Summary description for AnnotationMetaFactory.
	/// </summary>
	public class AnnotationMetaFactory : IMetaFactory
	{
		public AnnotationMetaFactory()
		{
			return;
		}

/// <summary>
/// Get metatable if name belongs to broker
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public MetaTable GetMetaTable (
			string name)
		{
			UserObject uo = null;
			int objectId, i1;
			string tok;
			Stopwatch sw = Stopwatch.StartNew();

			if (!UserObject.IsAnnotationTableName(name) &&
				!UserObject.IsUserDatabaseStructureTableName(name))
				return null;

			try { objectId = UserObject.ParseObjectIdFromInternalName(name); }
			catch (Exception ex) { return null; }

			try { uo = UserObjectDao.Read(objectId); }
			catch (MobiusConnectionOpenException ex)
			{ DbConnectionMx.ThrowSpecificConnectionOpenException(ex, "mbs_owner"); }

			if (uo==null) return null;

			if (!Permissions.UserHasReadAccess(Security.UserName, uo)) return null; 

			MetaTable mt = MetaTable.Deserialize(uo.Content);
			if (mt == null) return null; // something wrong with the annotation table content

			MetaColumn keyMc = mt.KeyMetaColumn;
			MetaColumn rootMc = mt.Root.KeyMetaColumn;
			if (rootMc.IsNumeric) // adjust key col mapping
				keyMc.ColumnMap = "EXT_CMPND_ID_NBR";
			else keyMc.ColumnMap = "EXT_CMPND_ID_TXT";

			while (mt.MetaColumns.Count > MetaTable.MaxColumns) // fixup for table with too many columns
				mt.MetaColumns.RemoveAt(mt.MetaColumns.Count - 1);

			foreach (MetaColumn mc in mt.MetaColumns)
			{ // set DetailsAvailable for existing annotation tables */
				if (mc.ResultCode != "") mc.DetailsAvailable = true; 
			}
			mt.Label = uo.Name; // get latest name (may have changed if renamed after saved)

			if (MetaTableFactory.ShowDataSource) // add source to label if requested
				mt.Label = MetaTableFactory.AddSourceToLabel(mt.Name,mt.Label);

			long ms = sw.ElapsedMilliseconds;

			return mt;
		}

/// <summary>
/// Calculate & persist metatable stats for tables belonging to broker
/// </summary>
/// <returns></returns>

		public int UpdateMetaTableStatistics()
		{
			MetaTableStats mts;
			DateTime dt;
			string sql;
			int mthdId;
			long rowCount;

			Dictionary<int, MetaTableStats> stats = new Dictionary<int, MetaTableStats>();
			DbCommandMx dao = new DbCommandMx();

// Get row count for each table

			sql =
				"select " +
				" mthd_vrsn_id, count(*) " +
				"from ( " +
				" select " +
				"  mthd_vrsn_id, rslt_grp_id" +
				"  from mbs_owner.mbs_adw_rslt " +
				" where " +
				"  sts_id = 1 " +
//				" and mthd_vrsn_id = 148411 " + // debug
				" group by mthd_vrsn_id, rslt_grp_id) " +
				"group by mthd_vrsn_id";

			dao.Prepare(sql);
			dao.ExecuteReader();
			while (dao.Read())
			{
				mthdId = dao.GetInt(0);
				rowCount = dao.GetLong(1);
				if (!stats.ContainsKey(mthdId)) stats[mthdId] = new MetaTableStats();
				stats[mthdId].RowCount = rowCount;
			}

			dao.CloseReader();

// Get latest update date for each table 

			sql = 
				"select " +
				" mthd_vrsn_id, max(updt_dt) " +
				"from mbs_owner.mbs_adw_rslt " +
				"where " +
				" updt_dt <= sysdate and " +
				" sts_id = 1" +
//				" and mthd_vrsn_id = 148411 " + // debug
				"group by mthd_vrsn_id";

			dao.Prepare(sql);
			dao.ExecuteReader();
			while (dao.Read())
			{
				mthdId = dao.GetInt(0);
				dt = dao.GetDateTime(1);
				if (!stats.ContainsKey(mthdId)) stats[mthdId] = new MetaTableStats();
				stats[mthdId].UpdateDateTime = dt;
			}

			dao.CloseReader();
			dao.Dispose();

			int updCnt = 0;
			foreach (int mthdId_ in stats.Keys)
			{
				mts = stats[mthdId_];
				try { UserObjectDao.UpdateUpdateDateAndCount(mthdId_, mts.UpdateDateTime, (int)mts.RowCount); }
				catch (Exception ex) { continue; }
				updCnt++;
			}

			return stats.Count;
		}

		/// <summary>
		/// Load persisted metatable stats
		/// </summary>
		/// <param name="metaTableStats"></param>
		/// <returns></returns>

		public int LoadMetaTableStats(
			Dictionary<string, MetaTableStats> metaTableStats)
		{
			return 0;
		}

	}
}
