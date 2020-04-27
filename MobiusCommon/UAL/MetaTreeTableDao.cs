using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Mobius.UAL
{

	/// <summary>
	/// Functionality for creating and maintaining Target/Assay association information
	/// </summary>

	public class MetaTreeTableDao
	{
		public static string MetatreeNodesTableName
		{
			get
			{
				if (_metatreeNodesTableName == null)
					_metatreeNodesTableName = ServicesIniFile.Read("MetatreeNodesTableName", DefaultMetatreeNodesTableName);
				return _metatreeNodesTableName;
			}
		}
		static string _metatreeNodesTableName = null;

		static string DefaultMetatreeNodesTableName = "dev_mbs_owner.metatree_nodes";

		public static string AdjustMetatreeNodesTableName(string txt)
		{
			string txt2 = txt; // noop now
												 //string txt2 = Lex.Replace(txt, DefaultMetatreeNodesTableName, MetatreeNodesTableName);
			return txt2;
		}


		/// <summary>
		/// Insert the rows for a metatree node including its children into the metatree_nodes table
		/// Called via command: Update MetatreeNodeTable
		/// </summary>
		/// <param name="toks"></param>
		/// <param name="dao"></param>

		public static int InsertMetatreeNodeRows(
			MetaTreeNode mtn,
			DbCommandMx dao)
		{
			int insCnt = 0;

			foreach (MetaTreeNode cn in mtn.Nodes)
			{
				string names = "";
				string values = "";
				AddInsertColumn("parent_name", ref names, ref values, mtn.Name);
				AddInsertColumn("parent_label", ref names, ref values, mtn.Label);
				AddInsertColumn("parent_type", ref names, ref values, mtn.Type);
				AddInsertColumn("child_name", ref names, ref values, cn.Name);
				AddInsertColumn("child_label", ref names, ref values, cn.Label);
				AddInsertColumn("child_type", ref names, ref values, cn.Type);
				AddInsertColumn("child_size", ref names, ref values, cn.Size);
				AddInsertColumn("child_update_dt", ref names, ref values, cn.UpdateDateTime);

				string sql = "insert into " + MetatreeNodesTableName +  " (" + names + ") " +
				 "values (" + values + ")";

				dao.Prepare(sql);
				dao.ExecuteNonReader();
				insCnt++;
			}

			return insCnt;
		}

		static void AddInsertColumn(
			string colName,
			ref string names,
			ref string values,
			object obj)
		{
			if (names != "") names += ",";
			names += colName;

			if (values != "") values += ", ";

			if (obj == null) values += "null";

			else if (obj is int && ((int)obj) == NullValue.NullNumber || // store null number values as nulls
			 obj is double && ((double)obj) == NullValue.NullNumber)
				values += "null";

			else if (obj is DateTime)
			{
				DateTime dt = (DateTime)obj;
				if (dt.Equals(DateTime.MinValue)) values += "null";
				else
				{
					string yyyymmdd = DateTimeMx.Normalize(dt);
					values += "to_date('" + yyyymmdd + "','YYYYMMDD')";
				}
			}

			else values += Lex.AddSingleQuotes(obj.ToString());

			return;
		}

	}

}
