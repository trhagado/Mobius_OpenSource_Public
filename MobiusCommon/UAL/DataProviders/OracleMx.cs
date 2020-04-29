using Mobius.ComOps;
using Mobius.Data;

using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Data;
using System.Data.Common;

namespace Mobius.UAL
{
/// <summary>
/// Oracle-specific general database operations
/// </summary>

	public class OracleMx
	{

		/// <summary>
		/// Get a metatable from Oracle catalog
		/// </summary>
		/// <param name="tableName">schema.table</param>
		/// <returns></returns>

		public static MetaTable GetMetaTableFromDatabaseDictionary(
			string tableName)
		{
			int t0 = TimeOfDay.Milliseconds();

			string[] sa = tableName.Split('.');
			if (sa.Length != 2) return null;

			string creator = sa[0];
			string tname = sa[1];

			MetaTable mt = new MetaTable();
			mt.MetaBrokerType = MetaBrokerType.Generic;
			mt.Name = tname;
			mt.Label = MetaTable.IdToLabel(tname);
			mt.TableMap = tableName;

			List<DbColumnMetadata> cmdList = GetTableMetadataFromOracleDictionary(tableName);
			for (int ci = 0; ci <cmdList.Count; ci++)
			{
				DbColumnMetadata cmd = cmdList[ci];

				MetaColumn mc = new MetaColumn();
				mc.Name = cmd.column_name;
				mc.ColumnMap = mc.Name;
				mc.Label = MetaTable.IdToLabel(cmd.column_name);

				if (cmd.data_type == "VARCHAR" ||
					cmd.data_type == "VARCHAR2" ||
					cmd.data_type == "NVARCHAR2" ||
					cmd.data_type == "CHAR" ||
					cmd.data_type == "CHARACTER" ||
					cmd.data_type == "LONG") mc.DataType = MetaColumnType.String;
				else if (cmd.data_type == "INTEGER")
					mc.DataType = MetaColumnType.Integer;

				else if (cmd.data_type == "NUMBER" ||
					cmd.data_type == "FLOAT")
				{
					mc.DataType = MetaColumnType.Number;
					mc.Format = ColumnFormatEnum.SigDigits; // display with 3 sig figures by default
					mc.Decimals = 3;
				}

				else if (cmd.data_type == "DATE" || cmd.data_type.StartsWith("TIMESTAMP")) mc.DataType = MetaColumnType.Date;

				else continue; // unrecognized

				mc.InitialSelection = ColumnSelectionEnum.Selected;
				mc.Width = 12;
				mc.MetaTable = mt;

				mt.AddMetaColumn(mc);
			}

			t0 = TimeOfDay.Milliseconds() - t0;
			return mt;
		}

/// <summary>
/// GetTableMetadataFromOracleDictionary
/// </summary>
/// <param name="tableName"></param>
/// <returns></returns>

		public static List<DbColumnMetadata> GetTableMetadataFromOracleDictionary(
			string tableName)
		{
			int t0 = TimeOfDay.Milliseconds();

			DbColumnMetadata cmd;
			List<DbColumnMetadata> cmdList = new List<DbColumnMetadata>();

			string sql = "select column_name,data_type,data_length,data_precision,data_scale,nullable " +
				"from sys.all_tab_columns where owner=:0 " +
				"and table_name=:1 order by column_id";

			DbConnectionMx conn = DbConnectionMx.MapSqlToConnection(ref tableName); // get proper connection
			if (conn == null)
				throw new Exception("Connection not found for tableName: " + tableName);

			DbCommandMx drd = new DbCommandMx();
			drd.MxConn = conn;
			int parmCount = 2;
			drd.PrepareMultipleParameter(sql, parmCount);

			string[] sa = tableName.Split('.');
			if (sa.Length != 2) throw new Exception("TableName not in owner.tableName form: " + tableName);
			string creator = sa[0];
			string tname = sa[1];

			if (Lex.Eq(creator, "mbs_user")) // workaround to allow tables owned by dev mbs_owner
				creator = "mbs_owner"; // to be accessed via synonyms defined on dev mbs_user

			object[] p = new object[2];
			p[0] = creator.ToUpper();
			p[1] = tname.ToUpper();

			drd.ExecuteReader(p);
			while (drd.Read())
			{
				cmd = new DbColumnMetadata();
				cmd.column_name = drd.GetStringByName("column_name");
				cmd.data_type = drd.GetStringByName("data_type");
				cmd.data_length = drd.GetIntByName("data_length");
				cmd.data_precision = drd.GetIntByName("data_precision");
				cmd.data_scale = drd.GetIntByName("data_scale");
				cmd.nullable = drd.GetStringByName("nullable");

				cmdList.Add(cmd);
			}

			drd.Dispose();

			t0 = TimeOfDay.Milliseconds() - t0;
			return cmdList;
		}

/// <summary>
/// 
/// </summary>
/// <param name="sql"></param>
/// <param name="sourceName"></param>
/// <param name="schemaName"></param>
/// <returns></returns>

		public static List<DbColumnMetadata> GetColumnMetadataFromSql(
			string sql,
			DbConnectionMx conn)
		{
			int t0 = TimeOfDay.Milliseconds();

			DbColumnMetadata md;
			List<DbColumnMetadata> mdList = new List<DbColumnMetadata>();

			string sql2 = sql + " where 1=2"; // make execution fast
			DbCommandMx cmd = new DbCommandMx();
			cmd.MxConn = conn;
			cmd.PrepareUsingDefinedConnection(sql2);

			OracleDataReader rdr = cmd.ExecuteReader() as OracleDataReader;

			for (int fi = 0; fi < rdr.FieldCount; fi++)
			{
				md = new DbColumnMetadata();
				md.column_name = rdr.GetName(fi);
				md.data_type = rdr.GetDataTypeName(fi); // 

				mdList.Add(md);
			}

			rdr.Dispose();

			t0 = TimeOfDay.Milliseconds() - t0;
			return mdList;
		}

/// <summary>

		/// <summary>
		/// TruncateStringIfExceedsMaxStringSize 
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string TruncateStringIfExceedsMaxStringSize(string s)
		{
			if (s == null || s.Length <= DbCommandMx.MaxOracleStringSize)
				return s;

			else return (s.Substring(0, DbCommandMx.MaxOracleStringSize));
		} 
		/// <summary>
		/// ClearStringIfExceedsMaxStringSize
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static string ClearStringIfExceedsMaxStringSize(string s)
		{
			if (s != null && s.Length <= DbCommandMx.MaxOracleStringSize)
				return s;

			else return "";
		}

		/// <summary>
		/// Format a SQL statement
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>

		public static string FormatSql(string sql)
		{
			bool errorEncountered = false;

			if (Lex.IsUndefined(sql)) return "";

			PoorMansTSqlFormatterLib.SqlFormattingManager mgr = new PoorMansTSqlFormatterLib.SqlFormattingManager();

			PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatter fmtr = mgr.Formatter as PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatter;
			if (fmtr != null) 
				fmtr.Options.TrailingCommas = true;

			string formattedSql = mgr.Format(sql, ref errorEncountered);

			formattedSql = Lex.Replace(formattedSql, "--WARNING! ERRORS ENCOUNTERED DURING SQL PARSING!\r\n", "");
			return formattedSql;
		}

	}

/// <summary>
/// Catalog metadata for a database column
/// </summary>

	public class DbColumnMetadata
	{
		public string column_name = "";
		public string data_type = "";
		public int data_length = -1;
		public int data_precision = -1; // number of significant digits
		public int data_scale = -1; // number of places to move the decimal point
		public string nullable = "";
	}

}
