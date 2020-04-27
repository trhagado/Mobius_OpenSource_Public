using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections.Generic;

namespace Mobius.QueryEngineLibrary
{
	/// <summary>
	/// Handles brokering of Spotfire Links that use the Spotfire Web Player to display data in a browser control
	/// </summary>

	public class SpotfireLinkMetaBroker : GenericMetaBroker
	{

/// <summary>
/// Constructor
/// </summary>
		
		public SpotfireLinkMetaBroker()
		{
			return;
		}

		/// <summary>
		/// Build the sql for the query
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		public override string BuildSql(
			ExecuteQueryParms eqp)
		{
			string sql = base.BuildSql(eqp);
			return sql;
		}

		/// <summary>
		/// Ignore criteria because they are used as parameters for Spotfire control,
		/// not as search criteria.
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public override bool IgnoreCriteria(
			MetaTable mt)
		{
			return true; // ignore criteria, used as parameters for Spotfire control
		}

	}
}
