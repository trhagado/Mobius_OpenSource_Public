using Mobius.ComOps;
using Mobius.Data;

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace Mobius.UAL
{

/// <summary>
/// SpotfireDao
/// </summary>

	public class SpotfireDao
	{
		/// <summary>
		/// Store Spotfire SQL and associated fields returning the sequence number of the row
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>

		public static int InsertSpotfireSql(
			string name,
			int version,
			string sqlStmt,
			string keyColName,
			string keys,
			string owner)
		{
			int v0 = version;

			if (version < 0)
			{
				version = SequenceDao.NextVal("dev_mbs_owner.mbs_spotfire_sql_seq");
				if (v0 == -2) // append version to name
					name += version;
			}

			else DeleteSqlStatement(name, version); // delete if matching version

			string sql = @"
				insert into dev_mbs_owner.mbs_spotfire_sql
				(name, version, sql, key_col_name, keys, ownr_id)
				values (:0,:1,:2,:3,:4,:5)";

			DbCommandMx drDao = new DbCommandMx();

			OracleDbType[] pa = new OracleDbType[6];

			pa[0] = OracleDbType.Varchar2;
			pa[1] = OracleDbType.Int32;
			pa[2] = OracleDbType.Clob;
			pa[3] = OracleDbType.Varchar2;
			pa[4] = OracleDbType.Varchar2;
			pa[5] = OracleDbType.Varchar2;

			drDao.Prepare(sql, pa);

			object[] p = new object[6];

			p[0] = name;
			p[1] = version;
			p[2] = sqlStmt;
			p[3] = keyColName;
			p[4] = keys;
			p[5] = owner;

			int count = drDao.ExecuteNonReader(p); // insert the row
			drDao.Dispose();

			return version;
		}

/// <summary>
/// ReadSqlStatement
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public static string ReadSqlStatement(
			string name,
			int version)
		{
			int id;
			string sqlStmt, keyColName, owner;

			if (!ReadSqlStatement(name, version,  out sqlStmt, out keyColName, out owner))
				return null;

			return sqlStmt;
		}

/// <summary>
/// ReadSqlStatement
/// </summary>
/// <param name="name"></param>
/// <param name="id"></param>
/// <param name="sqlStmt"></param>
/// <param name="owner"></param>
/// <returns></returns>

		public static bool ReadSqlStatement(
			string name,
			int version,
			out string sqlStmt,
			out string keyColName,
			out string owner)
		{
			string sql = @"
			select 
				sql, 
				key_col_name,
				ownr_id
			from dev_mbs_owner.mbs_spotfire_sql
			where name = '" + name + "' and " +
			"version = " + version;

			DbCommandMx cmd = new DbCommandMx();
			cmd.Prepare(sql);
			cmd.ExecuteReader();

			sqlStmt = keyColName = owner = null;

			bool exists = cmd.Read();
			if (exists)
			{
				sqlStmt = cmd.GetClob(0);
				keyColName = cmd.GetString(1);
				owner = cmd.GetString(2);
			}

			cmd.CloseReader();
			cmd.Dispose();

			return exists;
		}

/// <summary>
/// DeleteSqlStatement
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public static bool DeleteSqlStatement(
			string name,
			int version)
		{
			string sql = @"
			delete from dev_mbs_owner.mbs_spotfire_sql
			where name = '" + name + "'";

			if (version >= 0) // specific version?
				sql += " and version = " + version;

			DbCommandMx cmd = new DbCommandMx();
			cmd.Prepare(sql);
			int count = cmd.ExecuteNonReader();
			cmd.Dispose();

			return (count > 0);
		}



	}
}
