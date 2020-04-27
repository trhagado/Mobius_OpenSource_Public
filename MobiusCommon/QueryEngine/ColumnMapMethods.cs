using Mobius.UAL;
using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// ColumnMap methods for transforming values
/// </summary>

namespace Mobius.QueryEngineLibrary
{
/// <summary>
/// Perform Cassper Calculations
/// </summary>

	public class CassperCalc
	{

/// <summary>
/// Retrieve theoretical elemental composition value from Cassper database
/// </summary>
/// <param name="args"></param>
/// <returns></returns>

		public object GetElementalAnalysisPct (
			string cassperId,
			string aSymbol)
		{
			DbCommandMx drd = new DbCommandMx();
			string sql =
				"select comp_pct " +
				"from cas_owner.cas_structure " +
				"where cassper_id = :0";

			drd.PrepareMultipleParameter(sql, 1);
			drd.ExecuteReader(cassperId);
			if (!drd.Read() || drd.IsNull(0))
			{
				drd.Dispose();
				return null;
			}

			string s = drd.GetString(0);
			drd.Dispose();

			int i1 = s.ToLower().IndexOf(aSymbol.ToLower()); // find atom symbol
			if (i1 < 0) return null;
			s = s.Substring(i1 + 2); // get following number
			i1 = s.IndexOf(" "); // and any following space
			if (i1 >= 0) s = s.Substring(0,i1);
			return Double.Parse(s);
		}
	}
}
