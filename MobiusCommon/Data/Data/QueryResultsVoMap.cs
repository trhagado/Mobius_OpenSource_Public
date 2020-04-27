using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;
using System.Globalization;
using System.IO;

namespace Mobius.Data
{
	/// <summary>
	/// Mapping of the retrieved columns in a query to positions in the query results rows vo object array
	/// </summary>

	public class QueryResultsVoMap
	{
		public List<QueryTableVoMap> Tables = new List<QueryTableVoMap>();

		/// <summary>
		/// Build map from query
		/// </summary>
		/// <param name="query"></param>

		public static QueryResultsVoMap BuildFromQuery(
			Query query,
			bool includeKeyColsForAllTables = false)
		{

			QueryResultsVoMap qMap = new QueryResultsVoMap();
			for (int qti = 0; qti < query.Tables.Count; qti++)
			{
				QueryTable qt = query.Tables[qti];
				QueryTableVoMap tMap = new QueryTableVoMap();
				qMap.Tables.Add(tMap);
				tMap.Table = qt;

				foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (!qc.Is_Selected_or_GroupBy_or_Sorted) continue; // must be selected, grouped or sorted on
					if (!includeKeyColsForAllTables && qti > 0 && qc.IsKey) continue; // don't include keys from tables other than the first

					tMap.SelectedColumns.Add(qc);
				}
			}

			return qMap;
		}
	}

	public class QueryTableVoMap
	{
		public QueryTable Table = null;
		public List<QueryColumn> SelectedColumns = new List<QueryColumn>();
	}

}
