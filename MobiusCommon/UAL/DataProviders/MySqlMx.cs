using Mobius.ComOps;
using Mobius.Data;

using MySql.Data.MySqlClient;

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

	public class MySqlMx
	{

		/// <summary>
		/// Get a metatable from MySql catalog
		/// </summary>
		/// <param name="tableName">schema.table</param>
		/// <returns></returns>

		public static MetaTable GetMetaTableFromDatabaseDictionary(
			DbConnectionMx conn,
			string schema,
			string tableName)
		{
			int t0 = TimeOfDay.Milliseconds();

			MetaTable mt = new MetaTable();
			mt.MetaBrokerType = MetaBrokerType.Generic;
			mt.Name = tableName;
			mt.Label = MetaTable.IdToLabel(tableName);
			mt.TableMap = schema + "." + tableName;

			List<DbColumnMetadata> cmdList = GetTableMetadataFromMySqlDictionary(conn, schema, tableName);
			for (int ci = 0; ci < cmdList.Count; ci++)
			{
				DbColumnMetadata cmd = cmdList[ci];

				MetaColumn mc = new MetaColumn();
				mc.Name = cmd.Name;
				mc.ColumnMap = mc.Name;
				mc.Label = MetaTable.IdToLabel(cmd.Name);

				if (Lex.Contains(cmd.Type, "CHAR") ||
				 Lex.Contains(cmd.Type, "TEXT"))
					mc.DataType = MetaColumnType.String;

				else if (Lex.Contains(cmd.Type, "INT") ||
					Lex.Contains(cmd.Type, "ENUM"))
					mc.DataType = MetaColumnType.Integer;

				else if (cmd.Type == "FLOAT" || 
					cmd.Type == "REAL" ||
					cmd.Type == "DOUBLE" ||
					cmd.Type == "DECIMAL" ||
					cmd.Type == "NUMERIC")
				{
					mc.DataType = MetaColumnType.Number;
					mc.Format = ColumnFormatEnum.Decimal;
					mc.Decimals = cmd.Scale;
				}

				else if (cmd.Type == "DATE" ||
				 cmd.Type == "DATETIME" ||
				 cmd.Type == "TIMESTAMP") 
					mc.DataType = MetaColumnType.Date;

				else continue; // unrecognized

				mc.InitialSelection = ColumnSelectionEnum.Selected;
				mc.Width = 12;
				mc.MetaTable = mt;
				mc.Description = cmd.Comment;

				mt.AddMetaColumn(mc);
			}

			t0 = TimeOfDay.Milliseconds() - t0;
			return mt;
		}

		public static List<DbColumnMetadata> GetTableMetadataFromMySqlDictionary(
			DbConnectionMx conn,
			string schema,
			string tableName)
		{
			string sql = String.Format(
				@"SELECT *
			  FROM INFORMATION_SCHEMA.COLUMNS
				WHERE 
           table_schema = '{0}' 
           AND table_name = '{1}'
				ORDER BY ORDINAL_POSITION",
				schema, tableName);

			DbCommandMx drd = new DbCommandMx();
			drd.MxConn = conn;
			drd.PrepareUsingDefinedConnection(sql);
			DbDataReader rdr = drd.ExecuteReader();

			List<DbColumnMetadata> md = new List<DbColumnMetadata>();

      while (rdr.Read())
      {
        DbColumnMetadata cmd = new DbColumnMetadata();
        cmd.Name = drd.GetStringByName("column_name");
        cmd.Type = drd.GetStringByName("data_type");
        cmd.Length = drd.GetLongByName("character_maximum_length");
        cmd.Precision = drd.GetIntByName("numeric_precision");
        cmd.Scale = drd.GetIntByName("numeric_scale");
        cmd.Nullable = drd.GetStringByName("is_nullable");
        cmd.Comment = drd.GetStringByName("column_comment");

        md.Add(cmd);
      }

      rdr.Close();
			return md;
		}

	}


}
