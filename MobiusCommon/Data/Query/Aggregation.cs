using Mobius.ComOps;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Mobius.Data
{

	/// <summary>
	/// Query aggregation utilities
	/// </summary>

	public class Aggregation
	{

		/// <summary>
		/// Get count of tables in the query that include aggregation
		/// </summary>
		/// <returns></returns>

		public static int GetTablesAggregatedCount(
			Query q)
		{
			int count = 0;
			foreach (QueryTable qt in q.Tables)
			{
				if (qt.IsAggregated) count++;
			}

			return count;
		}

/// <summary>
/// Return true if query contains aggregated tables where each table includes aggregation that includes the key column
/// </summary>
/// <param name="q"></param>
/// <returns></returns>
		public static bool IsKeyAggreation(
			Query q)
		{
			List<QueryTable> keyAggregatedTables, nonKeyAggregatedTables;

			AnalyzeAggregation(q, out keyAggregatedTables, out nonKeyAggregatedTables);
			return keyAggregatedTables.Count > 0 && nonKeyAggregatedTables.Count == 0;
		}

		/// <summary>
		/// Check query for a single table that has non-key aggregation
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		public static bool IsNonKeyAggreation(
			Query q)
		{
			List<QueryTable> keyAggregatedTables, nonKeyAggregatedTables;

			AnalyzeAggregation(q, out keyAggregatedTables, out nonKeyAggregatedTables);
			return nonKeyAggregatedTables.Count > 0 && keyAggregatedTables.Count == 0;
		}

		public static bool AnalyzeAggregation(
			Query q,
			out List<QueryTable> keyAggregatedTables,
			out List<QueryTable> nonKeyAggregatedTables)
		{
			keyAggregatedTables = new List<QueryTable>();
			nonKeyAggregatedTables = new List<QueryTable>();

			foreach (QueryTable qt in q.Tables)
			{
				if (qt.IsKeyAggregation) keyAggregatedTables.Add(qt);
				else if (qt.IsNonKeyAggregation) nonKeyAggregatedTables.Add(qt);
			}

			return keyAggregatedTables.Count > 0 || nonKeyAggregatedTables.Count > 0;
		}

		public static void ValidateForQuery(
			Query q)
		{
			List<QueryTable> keyAggregatedTables, nonKeyAggregatedTables;

			AnalyzeAggregation(q, out keyAggregatedTables, out nonKeyAggregatedTables);

			if (q.Tables.Count > 1 && nonKeyAggregatedTables.Count > 0)
				throw new UserQueryException(
					"Any tables that define data aggregation must include grouping of the key column if the query\r\n" +
					"contains more than one table. The following table does not follow this rule:\r\n" + 
					"   " + nonKeyAggregatedTables[0].ActiveLabel);

			foreach (List<QueryTable> qtList in new List<QueryTable>[] { keyAggregatedTables, nonKeyAggregatedTables })
			{
				if (qtList == null) continue;

				foreach (QueryTable qt in qtList)
				{
					foreach (QueryColumn qc in qt.QueryColumns)
					{
						if (qc.Selected)
						{
							AggregationDef ad = qc.Aggregation;
							if (ad == null || !ad.RoleIsDefined)
							{
								throw new UserQueryException(
									"An aggregation role must be defined for the selected column: " + qc.ActiveLabel + "\r\n" +
									"in aggregated table: " + qt.ActiveLabel);
							}
						}
					}
				}
			}

			return;
		}

	} // Aggregation
}
