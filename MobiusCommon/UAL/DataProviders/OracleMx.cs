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
			DbConnectionMx conn,
			string schemaName,
			string tableName)
		{
			int t0 = TimeOfDay.Milliseconds();

			MetaTable mt = new MetaTable();
			mt.MetaBrokerType = MetaBrokerType.Generic;
			mt.Name = tableName;
			mt.Label = MetaTable.IdToLabel(tableName);
			mt.TableMap = schemaName + "." + tableName;

			List<DbColumnMetadata> cmdList = GetTableMetadataFromOracleDictionary(conn, schemaName, tableName);
			for (int ci = 0; ci <cmdList.Count; ci++)
			{
				DbColumnMetadata cmd = cmdList[ci];

				MetaColumn mc = new MetaColumn();
				mc.Name = cmd.Name;
				mc.ColumnMap = mc.Name;
				mc.Label = MetaTable.IdToLabel(cmd.Name);

				if (cmd.Type == "VARCHAR" ||
					cmd.Type == "VARCHAR2" ||
					cmd.Type == "NVARCHAR2" ||
					cmd.Type == "CHAR" ||
					cmd.Type == "CHARACTER" ||
					cmd.Type == "LONG") mc.DataType = MetaColumnType.String;
				else if (cmd.Type == "INTEGER")
					mc.DataType = MetaColumnType.Integer;

				else if (cmd.Type == "NUMBER" ||
					cmd.Type == "FLOAT")
				{
					mc.DataType = MetaColumnType.Number;
					mc.Format = ColumnFormatEnum.SigDigits; // display with 3 sig figures by default
					mc.Decimals = 3;
				}

				else if (cmd.Type == "DATE" || cmd.Type.StartsWith("TIMESTAMP")) mc.DataType = MetaColumnType.Date;

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
			DbConnectionMx conn,
			string schemaName,
			string tableName)
		{
			int t0 = TimeOfDay.Milliseconds();

			DbColumnMetadata cmd;
			List<DbColumnMetadata> cmdList = new List<DbColumnMetadata>();

			string sql = "select column_name,data_type,data_length,data_precision,data_scale,nullable " +
				"from sys.all_tab_columns where owner=:0 " +
				"and table_name=:1 order by column_id";

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
				cmd.Name = drd.GetStringByName("column_name");
				cmd.Type = drd.GetStringByName("data_type");
				cmd.Length = drd.GetIntByName("data_length");
				cmd.Precision = drd.GetIntByName("data_precision");
				cmd.Scale = drd.GetIntByName("data_scale");
				cmd.Nullable = drd.GetStringByName("nullable");

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
				md.Name = rdr.GetName(fi);
				md.Type = rdr.GetDataTypeName(fi); // 

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
		public string Name = "";
		public string Type = "";
		public long Length = -1;
		public int Precision = -1; // number of significant digits
		public int Scale = -1; // number of places to move the decimal point
		public string Nullable = "";
		public string Comment = "";
	}

}
