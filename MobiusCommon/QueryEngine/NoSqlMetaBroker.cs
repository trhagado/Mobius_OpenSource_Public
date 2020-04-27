using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections.Generic;

namespace Mobius.QueryEngineLibrary
{
	/// <summary>
	/// Handles query execution and data retrieval for table data produced by internal Mobius tools
	/// </summary>

	public class NoSqlMetaBroker : GenericMetaBroker
	{

		public GenericMetaBroker SpecificBroker = null; // actual specific broker that gets called for the associated tool

		/// <summary>
		/// Constructor
		/// </summary>

		public NoSqlMetaBroker()
		{
			return;
		}

		/// <summary>
		/// We will transform table at presearch time
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		public override bool ShouldPresearchCheckAndTransform(
			MetaTable mt)
		{
			return GSP(mt).ShouldPresearchCheckAndTransform(mt);
		}


		/// <summary>
		/// Do presearch initialization for a QueryTable
		/// </summary>
		/// <param name="originalQuery"></param>
		/// <param name="originalQt"></param>
		/// <param name="newQuery"></param>

		public override void DoPreSearchTransformation(
			Query originalQuery,
			QueryTable originalQt,
			Query newQuery)
		{
			MetaTable mt = originalQt.MetaTable;
			GSP(mt).DoPreSearchTransformation(originalQuery, originalQt, newQuery);
			return;
		}

		/// <summary>
		/// Prepare for execution
		/// </summary>
		/// <param name="eqp"></param>
		/// <returns></returns>

		public override string PrepareQuery(
		ExecuteQueryParms eqp)
		{
			Eqp = eqp; // save for possible later reference
			Qt = eqp.QueryTable;

			MetaTable mt = eqp?.QueryTable?.MetaTable;
			return GSP(mt).PrepareQuery(eqp);
		}

		/// <summary>
		/// Build the sql for the query
		/// </summary>
		/// <param name="eqp"></param>
		/// <returns></returns>

		public override string BuildSql(
			ExecuteQueryParms eqp)
		{
			Eqp = eqp; // save for possible later reference

			return "NoSql"; // NonSqlMetaBroker placeholder
		}

		/// <summary>
		/// Execute query for new set of keys
		/// </summary>
		/// <param name="eqp"></param>

		public override void ExecuteQuery(
			ExecuteQueryParms eqp)
		{
			Eqp = eqp; // save for possible later reference

			MetaTable mt = eqp?.QueryTable?.MetaTable;
			GSP(mt).ExecuteQuery(eqp);
			return;
		}

		/// <summary>
		/// NextRow - Return the next matching row value object
		/// </summary>
		/// <returns></returns>

		public override Object[] NextRow()
		{
			AssertMx.IsNotNull(SpecificBroker, "SpecificBroker");

			Object[] row = SpecificBroker.NextRow();
			return row;
		}

		/// <summary>
		/// Close broker & release resources
		/// </summary>

		public override void Close()
		{
			AssertMx.IsNotNull(SpecificBroker, "SpecificBroker");

			SpecificBroker.Close();
		}

		/// <summary>
		/// Ignore criteria because they are used as parameters for formatting Rgroup table
		/// not as search criteria.
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public override bool IgnoreCriteria(
			MetaTable mt)
		{
			AssertMx.IsNotNull(SpecificBroker, "SpecificBroker");

			return SpecificBroker.IgnoreCriteria(mt);
		}

		/// <summary>
		/// Build query to get summarization detail for RgroupMatrix data element
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="mc"></param>
		/// <param name="resultId">act_code.compound_id</param>
		/// <returns></returns>

		public override Query GetDrilldownDetailQuery(
			MetaTable mt,
			MetaColumn mc,
			int level,
			string resultId)
		{
			AssertMx.IsNotNull(SpecificBroker, "SpecificBroker");

			Query q = SpecificBroker.GetDrilldownDetailQuery(mt, mc, level, resultId);
			return q;
		}

		/// <summary>
		/// Get the specific broker, allocating as needed
		/// </summary>
		/// <param name="mt"></param>

		GenericMetaBroker GSP(
			MetaTable mt)
		{
			if (SpecificBroker == null) // need to allocate?
			{
				string sbType = mt.TableMap; // metatable map contains is of specific broker type

				if (Lex.Eq(sbType, "Mobius.Tools.SarLandscape"))
					SpecificBroker = new SasMapMetaBroker();

				else if (Lex.StartsWith(sbType, "SmallWorld"))
					SpecificBroker = new GenericMetaBroker();

				else throw new QueryException("Unrecognized NoSql data source type: " + sbType);
			}

			return SpecificBroker; //  already allocated?
		}
		/* Screen Test Text
		ABCDEFGHIJKLMNOPQRSTUVWXYZ
		abcdefghijklmnopqrstuvwxyz
		1234567890
		*/

	}
}
