using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.Common;

namespace Mobius.UAL
{
  public class OdbcMx
  {
		/// GetMetaTableFromOdbcDictionary
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>

		public static MetaTable GetMetaTableFromDatabaseDictionary(
			string tableName)
		{
			string data_type = null; // delete

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

			string sql = "select * from " + tableName;

			DbCommandMx drd = new DbCommandMx();
			drd.Prepare(sql);
			drd.ExecuteReader();
			DataTable st = drd.Rdr.GetSchemaTable();

			for (int ri = 0; ri < st.Rows.Count; ri++)
			{
				DataRow dr = st.Rows[ri];
				string column_name = dr["ColumnName"] as string;
				Type type = dr["DataType"] as Type;
				int data_length = (int)dr["ColumnSize"];
				int data_precision = (short)dr["NumericPrecision"];
				int data_scale = (short)dr["NumericScale"];
				bool nullable = (bool)dr["AllowDBNull"];

				MetaColumn mc = new MetaColumn();
				mc.Name = column_name;
				mc.ColumnMap = mc.Name;
				mc.Label = MetaTable.IdToLabel(column_name);

				if (Lex.Eq(type.Name, "String"))
					mc.DataType = MetaColumnType.String;

				else if (Lex.StartsWith(type.Name, "Int"))
					mc.DataType = MetaColumnType.Integer;

				else if (Lex.Eq(type.Name, "Single") ||
					Lex.Eq(type.Name, "Double"))
				{
					mc.DataType = MetaColumnType.Number;
					mc.Format = ColumnFormatEnum.SigDigits; // display with 3 sig figures by default
					mc.Decimals = 3;
				}

				else if (Lex.Eq(type.Name, "DateTime")) mc.DataType = MetaColumnType.Date;

				else continue; // unrecognized

				mc.InitialSelection = ColumnSelectionEnum.Selected;
				mc.Width = 12;
				mc.MetaTable = mt;

				mt.AddMetaColumn(mc);
			}

			drd.Dispose();

			t0 = TimeOfDay.Milliseconds() - t0;
			return mt;
		}

	}
}
