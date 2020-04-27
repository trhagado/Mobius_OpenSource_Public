using Mobius.UAL;
using Mobius.Data;

using System;
using System.Collections.Generic;

namespace Mobius.MetaFactoryNamespace
{
	/// <summary>
	/// Summary description for MultiTableMetaFactory.
	/// </summary>
	public class MultiTableMetaFactory : IMetaFactory
	{
		int RecursionDepth = 0;
		public MultiTableMetaFactory()
		{
			return;
		}

/// <summary>
/// Synthesize metatable associated with a multitable
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public MetaTable GetMetaTable (
			string name)
		{
			UserObject uo;
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt = null;
			MetaColumn mc;
			bool [] hasCol;
			int objectId, qti, qci;

			string prefix = "multitable_"; // multitable metatable names begin with "multitable_"
			if (name.ToLower().IndexOf(prefix) != 0) return null;
			string tok = name.Substring(prefix.Length); // get the id of the associated query
			try { objectId = Int32.Parse(tok); }
			catch (Exception ex) { return null; }

			uo = UserObjectDao.Read(objectId); // read query
			if (uo==null) return null;

			if (RecursionDepth > 3) return null; // avoid recursion loop

			RecursionDepth++;

			Query q = Query.Deserialize(uo.Content);
			q.UserObject = uo; // set associated user object
			qt = null; // set of common fields built here
			hasCol = null; 
			bool summarizedExists = true; // say summarized exists unless not seen on at least one table

			foreach (QueryTable qt2 in q.Tables)
			{
				if (qt2.MetaTable.Parent == null && q.Tables.Count>1) continue; // don't include root unless only table
				if (!qt2.MetaTable.SummarizedExists) summarizedExists = false;

				if (qt==null)	// duplicate first non-root table as model
				{
					qt = qt2.Clone(); 
					hasCol = new bool[qt.QueryColumns.Count];
					continue;
				}

				hasCol.Initialize();

				foreach (QueryColumn qc2 in qt2.QueryColumns)
				{
					qci = qt.GetQueryColumnIndexByName(qc2.MetaColumn.Name);
					if (qci<0) continue;
					hasCol[qci] = true;
				}
				
				qci = 0; // remove columns not in table other than key
				while (qci < qt.QueryColumns.Count)
				{
					qc = qt.QueryColumns[qci];
					if (hasCol[qci] || qc.IsKey) qci++;
					else qt.QueryColumns.RemoveAt(qci);
				}
			}

			RecursionDepth--;

			if (qt == null) return null; // no tables or only root table

// Build metatable with common columns

			mt = new MetaTable();
			mt.Name = name.ToUpper();
			mt.Label = q.UserObject.Name; // label same as query name
			mt.MetaBrokerType = MetaBrokerType.MultiTable;
			mt.Parent = qt.MetaTable.Parent;
			mt.SummarizedExists = summarizedExists;
			mt.MultiPivot = true; // cause pivot at runtime

			for (qci = 0; qci < qt.QueryColumns.Count; qci++)
			{
				qc = qt.QueryColumns[qci];
				if (qc.MetaColumn == null) continue;

				mc = qc.MetaColumn.Clone();
				mc.MetaTable = mt;
				mt.MetaColumns.Add(mc);

				if (qci == 0 && Math.Sqrt(4) == 2) // add column listing included tables after key (disabled)
				{
					string tables = "";
					int tableCount = 0;
					foreach (QueryTable qt2 in q.Tables)
					{
						if (qt2.MetaTable.Parent == null && q.Tables.Count > 1) continue; // don't include root unless only table
						if (tables.Length > 0) tables += ", ";
						tables += qt2.ActiveLabel;
						tableCount++;
					}
					mc = mt.AddMetaColumn("DataTables", "Data Tables(" + tableCount + "): " + tables, MetaColumnType.String, ColumnSelectionEnum.Selected,10);
					mc.IsSearchable = false;
					break; // include just key & table list cols
				}
			}

			return mt;
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



