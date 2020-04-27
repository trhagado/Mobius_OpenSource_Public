using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.Data
{
	/// <summary>
	/// Interface to be implemented by each data source metafactory 
	/// </summary>

	public interface IMetaFactory
	{

		/// <summary>
		/// Get metatable if name belongs to broker
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		MetaTable GetMetaTable(
				string mtName);

		/// <summary>
		/// Calculate & persist metatable stats for tables belonging to broker
		/// </summary>
		/// <returns></returns>

		int UpdateMetaTableStatistics();

		/// <summary>
		/// Load persisted metatable stats
		/// </summary>
		/// <param name="metaTableStats"></param>
		/// <returns></returns>

		int LoadMetaTableStats(
				Dictionary<string, MetaTableStats> metaTableStats);
	}

	/// <summary>
	/// Secondary interface (to be merted with IDataSourceMetaFactory)
	/// </summary>

	public interface IMetaFactory2
	{

		/// <summary>
		/// Return true if this is the metafactory that produces the table
		/// </summary>
		/// <param name="mName"></param>
		/// <returns></returns>

		bool IsMemberMetaTable(
				string mName);
	}
}
