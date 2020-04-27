using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Drawing;
using System.Collections;

namespace Mobius.QueryEngineLibrary
{

	/// <summary>
	/// Handle MultiTables which are single metatables that point to and substitute
	/// for a set of tables defined within a query.
	/// </summary>

	public class MultiTableMetaBroker : GenericMetaBroker
	{

/// <summary>
/// We will transform table at presearch time
/// </summary>
/// <param name="qt"></param>
/// <returns></returns>

		public override bool ShouldPresearchCheckAndTransform(
			MetaTable mt)
		{
			return true;
		}

/// <summary>
/// Read the original query associated with this multitable metatable
/// and substitute in the query along with any criteria, selections
/// for the common fields.
/// </summary>
/// <param name="qt"></param>
/// <param name="q"></param>
/// <param name="resultKeys"></param>

		public override void DoPreSearchTransformation(
			Query originalQuery,
			QueryTable qt,
			Query newQuery)
		{
			MetaTable mt2;
			UserObject uo;
			int objectId;

			string name = qt.MetaTable.Name;

			if (Lex.Eq(name, MetaTable.AllDataQueryTable)) // special case transform for QuickSearch all data query
			{
				QueryEngine.TransformSelectAllDataQuery(originalQuery, qt, newQuery);
				return;
			}

			string prefix = "multitable_"; // multitable metatable names begin with "multitable_"
			if (name.ToLower().IndexOf(prefix) != 0) return;
			string tok = name.Substring(prefix.Length); // get the id of the associated query
			try { objectId = Int32.Parse(tok); }
			catch (Exception ex) { return; }

			uo = UserObjectDao.Read(objectId); // read query
			if (uo==null) return;

			Query q2 = Query.Deserialize(uo.Content);
			foreach (QueryTable qt2 in q2.Tables)
			{
				mt2 = qt2.MetaTable;
				if (mt2.Parent == null) continue; // ignore root table
				QueryTable qt3 = qt2.Clone(); // make copy to modify & add to query
				qt3.Alias = ""; // clear alias to avoid possible conflicts with existing aliases

				if (qt.HeaderBackgroundColor != Color.Empty)
					qt2.HeaderBackgroundColor = qt.HeaderBackgroundColor;

				MetaColumn keyMc = qt3.MetaTable.KeyMetaColumn; // clear any key criteria
				if (keyMc != null)
				{
					QueryColumn qc3 = qt3.GetQueryColumnByName(keyMc.Name);
					qc3.Criteria = qc3.CriteriaDisplay = "";
				}

				foreach (QueryColumn qc in qt.QueryColumns)
				{ // pass any criteria, selections from multitable to underlying tables
					int qci3 = qt3.GetQueryColumnIndexByName(qc.MetaColumn.Name);
					if (qci3 < 0) continue; // ignore if doesn't match by name

					QueryColumn qc3 = qt3.QueryColumns[qci3]; // cloned column
					if (qc.Criteria != "")
					{ // if criteria then clone model query col for simpler copying of criteria
						QueryColumn qc4 = qc.Clone();
						qc4.MetaColumn = qc3.MetaColumn;
						qc4.QueryTable = qt3;
						qt3.QueryColumns[qci3] = qc4;
						qc3 = qc4;
					}

					qc3.Selected = qc.Selected;

					continue;
				}

				newQuery.AddQueryTable(qt3);
			}

			return;
		}
	}
}
