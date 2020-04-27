using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mobius.Services.Types;
using Mobius.Services.Util.TypeConversionUtil;
using Mobius.Services.Native.ServiceOpCodes;


namespace Mobius.Services.Native.OpInvokers
{
	public class MobiusMetaTableServiceOpInvoker : IInvokeServiceOps
	{
		private static TypeConversionHelper _transHelper = new TypeConversionHelper(TypeConversionHelper.ContextRetentionMode.NoRetention);

		#region IInvokeServiceOps Members

		object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
		{
			MobiusMetaTableService op = (MobiusMetaTableService)opCode;
			switch (op)
			{
				case MobiusMetaTableService.Initialize:
					{
						Mobius.Data.MetaTableCollection.MetaTableFactory = new Mobius.MetaFactoryNamespace.MetaTableFactory();
						Mobius.MetaFactoryNamespace.MetaTableFactory.Initialize();
						return null;
					}

				case MobiusMetaTableService.GetMetaTable:
					{
						string metaTableName = (string)args[0];
						Data.MetaTableCollection.GetDefaultRootMetaTable();
						Data.MetaTable mobiusMetaTable = Data.MetaTableCollection.MetaTableFactory.GetMetaTable(metaTableName);
						if (mobiusMetaTable == null) return null;
						else return mobiusMetaTable.Serialize();
					}
				case MobiusMetaTableService.GetMetaTableDescription:
					{
						string metaTableName = (string)args[0];
						Data.MetaTableCollection.GetDefaultRootMetaTable();
						Data.TableDescription mobiusMetaTableDescription =
								Data.MetaTableCollection.MetaTableFactory.GetDescription(metaTableName);
						MetaTableDescription metaTableDescription =
								_transHelper.Convert<Data.TableDescription, MetaTableDescription>(mobiusMetaTableDescription);
						return metaTableDescription;
					}
				case MobiusMetaTableService.GetMetaTableFromDatabaseDictionary:
					{
						string metaTableName = (string)args[0];
						Data.MetaTable mobiusMetaTable = MetaFactoryNamespace.OracleMetaFactory.GetMetaTableFromDatabaseDictionary(metaTableName);
						if (mobiusMetaTable == null) return null;
						else return mobiusMetaTable.Serialize();
					}
				case MobiusMetaTableService.BuildFromFile:
					{
						string serverFileName = (string)args[0];
						MetaFactoryNamespace.MetaTableFactory.BuildFromXmlFile(serverFileName);
						return true;
					}
				case MobiusMetaTableService.AddServiceMetaTable:
					{
						string serializedMetaTable = args[0].ToString();
						Data.MetaTable mobiusMetaTable = Data.MetaTable.Deserialize(serializedMetaTable);
						Data.MetaTableCollection.Add(mobiusMetaTable);
						return null;
					}
				case MobiusMetaTableService.RemoveServiceMetaTable:
					{
						string metaTableName = (string)args[0];
						Data.MetaTableCollection.Remove(metaTableName);
						return null;

						//Types.MetaTable metaTable = (Types.MetaTable)args[0];
						//Data.MetaTable mobiusMetaTableToUpdate = Data.MetaTableCollection.Get(metaTable.Name);

						////to take advantage of the logic in the TypeConversionHelper to update the MetaTable in place
						////  (No way of knowing in how many places references to the metatable might exist...)
						////  -- create a conversion helper (with retention) and
						////     add this single metatable to its list of known objects
						////     The conversion (if the metatable is "known") updates the Mobius metatable in place
						//TypeConversionHelper transHelper =
						//    new TypeConversionHelper(TypeConversionHelper.ContextRetentionMode.RememberAndUpdateMobiusObjects);
						//transHelper.AddKnownObject<Data.MetaTable>(mobiusMetaTableToUpdate);
						//Data.MetaTable mobiusMetaTable = _transHelper.Convert<MetaTable, Data.MetaTable>(metaTable);
						//return true;
					}

				case MobiusMetaTableService.UpdateStats:
					{
						string factoryName = (string)args[0];
						return Mobius.MetaFactoryNamespace.MetaTableFactory.UpdateStats(factoryName);
					}
			}
			return null;
		}

		#endregion
	}
}
