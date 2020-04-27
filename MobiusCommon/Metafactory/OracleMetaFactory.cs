using Mobius.UAL;
using Mobius.ComOps;
using Mobius.Data;
using Mobius.QueryEngineLibrary;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Mobius.MetaFactoryNamespace

{
	/// <summary>
	/// Build Metadata for an Oracle table
	/// </summary>
	
	public class OracleMetaFactory : IMetaFactory
	{
		public MetaTable GetMetaTable(
			string mtName)
		{
			MetaTable mt = null;
			MetaColumn mc, mc2;
			int methodId;
			string txt, tok;

			mtName = mtName.Trim().ToUpper();
			string name2 = mtName;
			string prefix = "ORACLE";
			if (mtName.StartsWith(prefix + "_") || mtName.StartsWith(prefix + "."))
				name2 = mtName.Substring(prefix.Length + 1);
			string[] sa = name2.Split('.');
			if (sa.Length < 2) return null;

			name2 = sa[0] + "." + sa[1];
			mt = GetMetaTableFromDatabaseDictionary(name2);
			if (mt == null || mt.MetaColumns.Count == 0) return null;
			mt.Name = mtName; // assign fully qualified name

			if (sa.Length >= 3) // key col name included
			{
				mc = mt.GetMetaColumnByName(sa[2]);
				if (mc!=null) 
				 mc.DataType = MetaColumnType.CompoundId;
			}

			if (sa.Length >= 4) // assign parent if parent table name included
			{
				mt.Parent = MetaTableCollection.Get(sa[3]);
			}

			return mt;
		}

/// <summary>
/// Get a metatable from Oracle catalog
/// </summary>
/// <param name="mtName">schema.table</param>
/// <returns></returns>

		public static MetaTable GetMetaTableFromDatabaseDictionary(
			string mtName)
		{
			return OracleDao.GetMetaTableFromDatabaseDictionary(mtName);
		}

		/// <summary>
		/// Calculate & persist metatable stats for tables belonging to broker
		/// </summary>
		/// <returns></returns>

		public int UpdateMetaTableStatistics()
		{
			return 0;
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
