using Mobius.ComOps;

using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;

namespace Mobius.Data
{

	public class UnpivotedAssayView
	{
		public static string UnsummarizedMetaTableName = "UNPIVOTED_ASSAY_RESULTS";
		public static string SummarizedMetaTableName = "UNPIVOTED_ASSAY_RESULTS_SUMMARY";

		public static string AssayAttributesMetaTableName = "CMN_ASSY_ATRBTS";

		public static MetaTable AssayAttrsMetaTable { get { return MetaTableCollection.Get(AssayAttributesMetaTableName); } }

		/// <summary>
		/// Build query to fetch all unsummarized assay data for a target
		/// </summary>
		/// <param name="geneSymbol"></param>
		/// <returns></returns>

		public static Query BuildTargetAssayUnsummarizedDataQuery(
			string geneSymbol)
		{
			Query q = new Query(); 
			MetaTable mt = MetaTableCollection.Get(UnsummarizedMetaTableName);
			QueryTable qt = new QueryTable(mt);
			q.AddQueryTable(qt);
			q.SingleStepExecution = true; // for performance
			q.KeySortOrder = 0; // no sorting
			return q;
		}



	}
}
