using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Mobius.UAL
{
/// <summary>
/// Temporary database list operations
/// </summary>

	public class TempDbList
	{
		public DataSourceMx DataSource;
		public string Name = "";
		public int ItemCount = 0;
		public bool Numeric = true; // numeric or string
		public DateTime DateTimeLastWritten;

		public static Dictionary<string, TempDbList> Lists = new Dictionary<string, TempDbList>(); // info on lists written in this session
		public static bool Debug = false;

/// <summary>
/// Write temporary list
/// </summary>
/// <param name="dsName">Name of datasource</param>
/// <param name="listName">Name of list</param>
/// <param name="listItems">The list of items to write</param>
/// <param name="numeric">Store list items in numeric column if true, text otherwise</param>

		public static string Write (
			string dsName,
			string listName,
			List<string> listItems,
			int firstKeyIdx,
			int keyCount,
			bool numeric)
		{
			DbCommandMx cmd = null;
			Oracle.DataAccess.Client.OracleDbType parmType;
			string fullTableName, sKey;
			int iKey, i1;
			object o;

			DataSourceMx ds = GetDataSourceInfo(ref dsName, ref listName, out fullTableName);

			TruncateListTable(dsName, listName);
			if (keyCount == 0) return null; // nothing to write

			string sql = "insert into <schema.name>  (<cols>) values(:0)";
			sql = sql.Replace("<schema.name>", fullTableName);

			if (numeric) sql = sql.Replace("<cols>", "intKey");
			else sql = sql.Replace("<cols>", "stringKey");

			object[][] pva = DbCommandMx.NewObjectArrayArray(1, keyCount); // alloc insert row array

			if (numeric)
			{

				for (i1 = 0; i1 < keyCount; i1++)
				{
					if (int.TryParse(listItems[firstKeyIdx + i1], out iKey))
						pva[0][i1] = iKey;
					else pva[0][i1] = DBNull.Value;
				}

				parmType = Oracle.DataAccess.Client.OracleDbType.Int32;
			}

			else // string keys
			{
				for (i1 = 0; i1 < keyCount; i1++)
				{
					sKey = listItems[firstKeyIdx + i1];
					if (!Lex.IsNullOrEmpty(sKey))
						pva[0][i1] = sKey;
					else pva[0][i1] = DBNull.Value;
				}

				parmType = Oracle.DataAccess.Client.OracleDbType.Varchar2;
			}

			try
			{
				DateTime t0 = DateTime.Now;
				cmd = new DbCommandMx();
				DbConnectionMx dbc = DbConnectionMx.Get(dsName);
				cmd.MxConn = dbc;
				cmd.Prepare(sql, parmType);
				int insCount = cmd.ExecuteArrayNonReader(pva, ref keyCount);
				int tDelta = (int)TimeOfDay.Delta(t0);
				if (Debug)
					DebugLog.Message("Write TempDbList for KeyCount: " + insCount + ", Time: " + tDelta);
			}

			catch (Exception ex)
			{
				throw ex;
			}

			finally { cmd.Dispose(); }

			return fullTableName;
		}

/// <summary>
/// Be sure the table is truncated
/// </summary>
/// <param name="schema"></param>
/// <param name="listName"></param>

		public static void TruncateListTable(
			string dsName,
			string listName)
		{
			DbCommandMx cmd = null;
			string fullTableName;
			
			DataSourceMx ds = GetDataSourceInfo(ref dsName, ref listName, out fullTableName);

			string sql = "truncate table " + fullTableName;
			try
			{
				DateTime t0 = DateTime.Now;
				cmd = new DbCommandMx();
				DbConnectionMx dbc = DbConnectionMx.Get(dsName);
				cmd.MxConn = dbc;
				cmd.Prepare(sql);
				cmd.ExecuteNonReader();
				int tDelta = (int)TimeOfDay.Delta(t0);
				//DebugLog.Message("Truncate Table time: " + tDelta);
			}

			catch (Exception ex) // if truncate failed see if need to create
			{
				if (!ex.Message.Contains("ORA-00942")) throw ex; // if other than already exists then throw exception
				bool created = CreateListTableIfNeeded(dsName, listName);
			}

			finally { if (cmd != null) cmd.Dispose(); }

			return;
		}

/// <summary>
/// Create the temp table for the list if needed
/// </summary>
/// <param name="schema"></param>
/// <param name="name"></param>

		public static bool CreateListTableIfNeeded(
			string dsName,
			string listName)
		{
			DbCommandMx cmd = null;
			bool created = true;
			string fullTableName;
			
			DataSourceMx ds = GetDataSourceInfo(ref dsName, ref listName, out fullTableName);

			if (Lists.ContainsKey(fullTableName)) return false;

			string sql = @"
				create global temporary table <schema.name> (
						rowPos integer,
						intKey integer,
						stringKey varchar2(256))
					on commit preserve rows";

			sql = sql.Replace("<schema.name>", fullTableName);

			try
			{
				DateTime t0 = DateTime.Now;
				cmd = new DbCommandMx();
				DbConnectionMx dbc = DbConnectionMx.Get(dsName);
				cmd.MxConn = dbc;
				cmd.Prepare(sql);
				cmd.ExecuteNonReader();
				created = true;
				int tDelta = (int)TimeOfDay.Delta(t0);
				//DebugLog.Message("Create Table time: " + tDelta);
			}

			catch (Exception ex)
			{
				if (!ex.Message.Contains("ORA-xxx")) throw ex; // if other than already exists then throw exception
				created = false; // already exists
			}

			finally { if (cmd != null) cmd.Dispose(); }

			TempDbList list = new TempDbList();
			list.DataSource = ds;
			list.Name = listName;
			Lists[fullTableName] = list;
			return created;
		}

		/// <summary>
		/// Get the data source info and full name for the temp table
		/// </summary>
		/// <param name="dsName"></param>
		/// <param name="listName"></param>
		/// <param name="fullTableName"></param>
		/// <returns></returns>

		static DataSourceMx GetDataSourceInfo(
			ref string dsName,
			ref string listName,
			out string fullTableName)
		{
			dsName = dsName.ToUpper();
			listName = listName.ToUpper();
			DataSourceMx ds = DataSourceMx.DataSources[dsName];
			fullTableName = ds.UserName.ToUpper() + "." + listName;
			return ds;
		}

	}
}
