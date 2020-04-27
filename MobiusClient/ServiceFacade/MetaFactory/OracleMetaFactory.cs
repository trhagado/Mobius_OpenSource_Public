using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.ServiceFacade
{
	public class OracleMetaFactory
	{
		/// <summary>
		/// Get a metatable from Oracle catalog
		/// </summary>
		/// <param name="mtName">schema.table</param>
		/// <returns></returns>

		public static MetaTable GetMetaTableFromDatabaseDictionary(
			string mtName)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				//use the MetaTable service for this...  Call into ServiceFacade.MetaTableFactory to keep the call origins all in one place.
				MetaTable metaTable = MetaTableFactory.GetMetaTableFromDatabaseDictionary(mtName);
				return metaTable;
			}

			else return Mobius.MetaFactoryNamespace.OracleMetaFactory.GetMetaTableFromDatabaseDictionary(mtName);
		}
	}
}
