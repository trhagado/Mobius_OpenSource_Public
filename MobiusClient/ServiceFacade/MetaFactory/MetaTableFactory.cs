using Mobius.ComOps;
using Mobius.Data;
using Mobius.MetaFactoryNamespace;
using Mfn = Mobius.MetaFactoryNamespace;
using Mobius.UAL;

using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;
using ServiceTypes = Mobius.Services.Types;


namespace Mobius.ServiceFacade
{
	public class MetaTableFactory : IMetaTableFactory
	{

		Mfn.MetaTableFactory MetaTableFactoryInstance; // ref to real factory

		/// <summary>
		/// Constructor
		/// </summary>

		public MetaTableFactory()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				//nothing to do
			}
			else MetaTableFactoryInstance = new Mfn.MetaTableFactory();
		}

		/// <summary>
		/// Initialize the MetaTableFactory
		/// </summary>

		public static void Initialize()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						nativeClient.InvokeNativeMethod(
								(int)Services.Native.ServiceCodes.MobiusMetaTableService,
								(int)Services.Native.ServiceOpCodes.MobiusMetaTableService.Initialize,
								new Mobius.Services.Native.NativeMethodTransportObject(new object[] { false }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				return;
			}

			else Mfn.MetaTableFactory.Initialize();
		}

		/// <summary>
		/// Load/reload from specified file
		/// </summary>

		public void BuildFromFile(string fileName)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				MetaTableCollection.Reset(); // reset local collection

				string serverFile = ServerFile.GetTempFileName(".xml", true);
				ServerFile.CopyToServer(fileName, serverFile);
					Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
					Services.Native.NativeMethodTransportObject resultObject =
							nativeClient.InvokeNativeMethod(
									(int)Services.Native.ServiceCodes.MobiusMetaTableService,
									(int)Services.Native.ServiceOpCodes.MobiusMetaTableService.BuildFromFile,
									new Mobius.Services.Native.NativeMethodTransportObject(new object[] { serverFile }));
					((System.ServiceModel.IClientChannel)nativeClient).Close();
			}

			else // just do local build
			{
				Mobius.MetaFactoryNamespace.MetaTableFactory.BuildFromXmlFile(fileName);
			}
		}

		/// <summary>
		/// Build MetaTable
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public MetaTable GetMetaTable(string name)
		{
			MetaTable mt;

			if (ServiceFacade.UseRemoteServices)
			{
					Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
					Services.Native.NativeMethodTransportObject resultObject = ServiceFacade.InvokeNativeMethod(nativeClient,
							(int)Services.Native.ServiceCodes.MobiusMetaTableService,
							(int)Services.Native.ServiceOpCodes.MobiusMetaTableService.GetMetaTable,
							new Services.Native.NativeMethodTransportObject(new object[] { name }));
					((System.ServiceModel.IClientChannel)nativeClient).Close();

					if (resultObject == null || resultObject.Value == null) return null;
					string serializedMt = resultObject.Value.ToString();
					mt = MetaTable.Deserialize(serializedMt);
					return mt;
			}
			else
			{
				int t0 = TimeOfDay.Milliseconds();
				mt = MetaTableFactoryInstance.GetMetaTable(name);
				//DebugLog.TimeMessage("MetaTableFactory.GetMetaTable " + name + ", time = ", t0);
				return mt;
			}
		}

		/// <summary>
		/// Add a metatable to the services side from the client 
		/// </summary>
		/// <param name="mt"></param>

		public void AddServiceMetaTable(MetaTable mt)
		{
			if (ServiceFacade.UseRemoteServices)
			{
					string serializedMt = mt.Serialize();
					Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
					Services.Native.NativeMethodTransportObject resultObject =
							ServiceFacade.InvokeNativeMethod(nativeClient,
									(int)Services.Native.ServiceCodes.MobiusMetaTableService,
									(int)Services.Native.ServiceOpCodes.MobiusMetaTableService.AddServiceMetaTable,
									new Services.Native.NativeMethodTransportObject(new object[] { serializedMt }));
					((System.ServiceModel.IClientChannel)nativeClient).Close();
			}
			else
			{
				return; // noop locally
			}
		}



		/// <summary>
		/// Remove a metatable on the services side from the client 
		/// </summary>
		/// <param name="mt"></param>

		public void RemoveServiceMetaTable(string mtName)
		{
			if (ServiceFacade.UseRemoteServices)
			{
					Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
					Services.Native.NativeMethodTransportObject resultObject =
							ServiceFacade.InvokeNativeMethod(nativeClient,
									(int)Services.Native.ServiceCodes.MobiusMetaTableService,
									(int)Services.Native.ServiceOpCodes.MobiusMetaTableService.RemoveServiceMetaTable,
									new Services.Native.NativeMethodTransportObject(new object[] { mtName }));
					((System.ServiceModel.IClientChannel)nativeClient).Close();
			}
			else
			{
				return; // noop locally
			}
		}

		/// <summary>
		/// Build MetaTable from a dictionary in oracle
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static MetaTable GetMetaTableFromDatabaseDictionary(string mtName)
		{
			MetaTable mt;

			if (ServiceFacade.UseRemoteServices)
			{
					Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
					Services.Native.NativeMethodTransportObject resultObject =
							ServiceFacade.InvokeNativeMethod(nativeClient,
									(int)Services.Native.ServiceCodes.MobiusMetaTableService,
									(int)Services.Native.ServiceOpCodes.MobiusMetaTableService.GetMetaTableFromDatabaseDictionary,
									new Services.Native.NativeMethodTransportObject(new object[] { mtName }));
					((System.ServiceModel.IClientChannel)nativeClient).Close();

					if (resultObject == null || resultObject.Value == null) return null;
					string serializedMt = resultObject.Value.ToString();
					mt = MetaTable.Deserialize(serializedMt);
					return mt;
			}

			else return Mobius.MetaFactoryNamespace.OracleMetaFactory.GetMetaTableFromDatabaseDictionary(mtName);
		}

		/// <summary>
		/// Get the labels of the root set of tables
		/// </summary>
		/// <returns></returns>

		public static List<string> GetRootTableLabels()
		{
			if (ServiceFacade.UseRemoteServices)
			{
					Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
					Services.Native.NativeMethodTransportObject resultObject =
							ServiceFacade.InvokeNativeMethod(nativeClient,
									(int)Services.Native.ServiceCodes.MobiusMetaTableService,
									(int)Services.Native.ServiceOpCodes.MobiusMetaTableService.GetRootTableLabels,
									new Services.Native.NativeMethodTransportObject(new object[] { null }));
					((System.ServiceModel.IClientChannel)nativeClient).Close();
					ServiceTypes.MetaTable serviceMT = (resultObject != null) ? (ServiceTypes.MetaTable)resultObject.Value : null;
					//MetaTable mt = ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.MetaTable, MetaTable>(serviceMT);
					//return mt;
					return null;
			}
			else return Mobius.MetaFactoryNamespace.MetaTableFactory.GetRootTableLabels();
		}

		/// <summary>
		/// Get the description or a link to the description for a table
		/// </summary>
		/// <param name="mtName"></param>
		/// <returns></returns>

		public TableDescription GetDescription(string mtName)
		{
			if (ServiceFacade.UseRemoteServices)
			{
					Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
					Services.Native.NativeMethodTransportObject resultObject =
							ServiceFacade.InvokeNativeMethod(nativeClient,
									(int)Services.Native.ServiceCodes.MobiusMetaTableService,
									(int)Services.Native.ServiceOpCodes.MobiusMetaTableService.GetMetaTableDescription,
									new Services.Native.NativeMethodTransportObject(new object[] { mtName }));
					((System.ServiceModel.IClientChannel)nativeClient).Close();
					ServiceTypes.MetaTableDescription serviceMetaTableDescription = (resultObject != null) ?
						(ServiceTypes.MetaTableDescription)resultObject.Value : null;
					TableDescription tableDescription =
							ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.MetaTableDescription, TableDescription>(serviceMetaTableDescription);
					return tableDescription;
			}
			else return MetaTableFactoryInstance.GetDescription(mtName);
		}


		/// <summary>
		/// Calculate metatable statistics for each broker type
		/// </summary>
		/// <param name="factoryName"></param>
		/// <returns></returns>

		public static int UpdateStats(
				string factoryName)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusMetaTableService,
								(int)Services.Native.ServiceOpCodes.MobiusMetaTableService.UpdateStats,
								new Services.Native.NativeMethodTransportObject(new object[] { factoryName }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();

				if (resultObject == null || resultObject.Value == null) return -1;
				else return (int)resultObject.Value;
			}

			else
			{
				return Mobius.MetaFactoryNamespace.MetaTableFactory.UpdateStats(factoryName);
			}
		}
	}
}
