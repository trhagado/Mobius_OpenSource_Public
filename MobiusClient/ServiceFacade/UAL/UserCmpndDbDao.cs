using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections.Generic;
using System.Text;
using ServiceTypes = Mobius.Services.Types;


namespace Mobius.ServiceFacade
{
	/// <summary>
	/// Ucdb selection, update, delete
	/// </summary>

	public class UserCmpndDbDao
	{
		Mobius.UAL.UserCmpndDbDao Instance;


		/// <summary>
		/// Default constructor
		/// </summary>

		public UserCmpndDbDao()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				//nothing to do
			}
			else Instance = new Mobius.UAL.UserCmpndDbDao();
			return;
		}


		/// <summary>
		/// Check if user if allowed to modify a database
		/// </summary>
		/// <param name="databaseId"></param>
		/// <param name="userName"></param>
		/// <returns></returns>

		public bool CanModifyDatabase(
			long databaseId,
			string userName)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.CanModifyDatabase,
								new Services.Native.NativeMethodTransportObject(new object[] { databaseId, userName }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				bool canModifyDatabase = (bool)resultObject.Value;
				return canModifyDatabase;
			}

			else return Instance.CanModifyDatabase(databaseId, userName);
		}

		/// <summary>
		/// Read database header for a database id
		/// </summary>
		/// <param name="databaseId"></param>
		/// <returns></returns>

		public UcdbDatabase SelectDatabaseHeader(
			long databaseId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.SelectDatabaseHeaderByDatabaseId,
								new Services.Native.NativeMethodTransportObject(new object[] { databaseId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				ServiceTypes.UcdbDatabase serviceUcdbDatabase =
						(ServiceTypes.UcdbDatabase)resultObject.Value;
				UcdbDatabase ucdbDatabase =
						ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.UcdbDatabase, UcdbDatabase>(serviceUcdbDatabase);
				return ucdbDatabase;
			}
			else return Instance.SelectDatabaseHeader(databaseId);
		}

		/// <summary>
		/// Select header for database based on owner and database name
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="databaseNameSpace"></param>
		/// <param name="databaseName"></param>
		/// <returns></returns>

		public UcdbDatabase SelectDatabaseHeader(
			string userName,
			string databaseNameSpace,
			string databaseName)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.SelectDatabaseHeader,
								new Services.Native.NativeMethodTransportObject(new object[] { userName, databaseNameSpace, databaseName }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				ServiceTypes.UcdbDatabase serviceUcdbDatabase =
						(ServiceTypes.UcdbDatabase)resultObject.Value;
				UcdbDatabase ucdbDatabase =
						ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.UcdbDatabase, UcdbDatabase>(serviceUcdbDatabase);
				return ucdbDatabase;
			}
			else return Instance.SelectDatabaseHeader(userName, databaseNameSpace, databaseName);
		}

		/// <summary>
		/// Select all database headers
		/// </summary>
		/// <returns></returns>

		public UcdbDatabase[] SelectDatabaseHeaders()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.SelectDatabaseHeaders,
								null);
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				ServiceTypes.UcdbDatabase[] serviceUcdbDatabases =
						(ServiceTypes.UcdbDatabase[])resultObject.Value;
				UcdbDatabase[] ucdbDatabases =
						ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.UcdbDatabase[], UcdbDatabase[]>(serviceUcdbDatabases);
				return ucdbDatabases;
			}
			else return Instance.SelectDatabaseHeaders();
		}

		/// <summary>
		/// Select the header information for all of the databases owned by a user
		/// </summary>
		/// <param name="ownerUserName"></param>
		/// <returns></returns>

		public UcdbDatabase[] SelectDatabaseHeaders(
			string ownerUserName)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.SelectDatabaseHeadersByOwner,
								new Services.Native.NativeMethodTransportObject(new object[] { ownerUserName }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				ServiceTypes.UcdbDatabase[] serviceUcdbDatabases =
						(ServiceTypes.UcdbDatabase[])resultObject.Value;
				UcdbDatabase[] ucdbDatabases =
						ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.UcdbDatabase[], UcdbDatabase[]>(serviceUcdbDatabases);
				return ucdbDatabases;
			}
			else return Instance.SelectDatabaseHeaders(ownerUserName);
		}

		/// <summary>
		/// Select model/annotation information for a database
		/// </summary>
		/// <param name="databaseId"></param>
		/// <returns></returns>

		public UcdbModel[] SelectDatabaseModels(
			long databaseId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.SelectDatabaseModels,
								new Services.Native.NativeMethodTransportObject(new object[] { databaseId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				ServiceTypes.UcdbModel[] serviceUcdbModels =
						(ServiceTypes.UcdbModel[])resultObject.Value;
				UcdbModel[] ucdbModels =
						ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.UcdbModel[], UcdbModel[]>(serviceUcdbModels);
				return ucdbModels;
			}
			else return Instance.SelectDatabaseModels(databaseId);
		}

		/// <summary>
		/// Get list of database string compound ids
		/// </summary>
		/// <param name="databaseId"></param>
		/// <returns></returns>

		public string[] SelectDatabaseExtStringCids(
			long databaseId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				string[] sa;
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.SelectDatabaseExtStringCids,
								new Services.Native.NativeMethodTransportObject(new object[] { databaseId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				string[] extStringIds = resultObject.ToStringArray();
				return extStringIds;
			}
			else return Instance.SelectDatabaseExtStringCids(databaseId);
		}

		/// <summary>
		/// Select database compound data for a compoundId
		/// </summary>
		/// <param name="compoundId"></param>
		/// <returns></returns>

		public UcdbCompound SelectDatabaseCompound(
			long compoundId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.SelectDatabaseCompoundByCompoundId,
								new Services.Native.NativeMethodTransportObject(new object[] { compoundId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				ServiceTypes.UcdbCompound serviceUcdbCompound =
						(ServiceTypes.UcdbCompound)resultObject.Value;
				UcdbCompound ucdbCompound =
						ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.UcdbCompound, UcdbCompound>(serviceUcdbCompound);
				return ucdbCompound;
			}
			else return Instance.SelectDatabaseCompound(compoundId);
		}


		/// <summary>
		/// Select database compound data for a databaseId and expCmpndIdTxt 
		/// </summary>
		/// <param name="databaseId"></param>
		/// <param name="extCmpndIdTxt"></param>
		/// <returns></returns>

		public UcdbCompound SelectDatabaseCompound(
			long databaseId,
			string extCmpndIdTxt)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.SelectDatabaseCompound,
								new Services.Native.NativeMethodTransportObject(new object[] { databaseId, extCmpndIdTxt }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				ServiceTypes.UcdbCompound serviceUcdbCompound =
						(ServiceTypes.UcdbCompound)resultObject.Value;
				UcdbCompound ucdbCompound =
						ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.UcdbCompound, UcdbCompound>(serviceUcdbCompound);
				return ucdbCompound;
			}
			else return Instance.SelectDatabaseCompound(databaseId, extCmpndIdTxt);
		}

		/// <summary>
		/// Select compound information for a database 
		/// </summary>
		/// <param name="databaseId"></param>
		/// <returns></returns>

		public UcdbCompound[] SelectDatabaseCompounds(
			long databaseId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.SelectDatabaseCompounds,
								new Services.Native.NativeMethodTransportObject(new object[] { databaseId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				ServiceTypes.UcdbCompound[] serviceUcdbCompounds =
						(ServiceTypes.UcdbCompound[])resultObject.Value;
				UcdbCompound[] ucdbCompounds =
						ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.UcdbCompound[], UcdbCompound[]>(serviceUcdbCompounds);
				return ucdbCompounds;
			}
			else return Instance.SelectDatabaseCompounds(databaseId);
		}

		/// <summary>
		/// Return count of compounds for database
		/// </summary>
		/// <param name="databaseId"></param>
		/// <returns></returns>

		public int SelectDatabaseCompoundCount(
			long databaseId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.SelectDatabaseCompoundCount,
								new Services.Native.NativeMethodTransportObject(new object[] { databaseId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				int compoundCount = (int)resultObject.Value;
				return compoundCount;
			}
			else return Instance.SelectDatabaseCompoundCount(databaseId);
		}

		/// <summary>
		/// Get the largest numeric compound id assigned to a database
		/// </summary>
		/// <param name="databaseId"></param>
		/// <returns></returns>

		public long GetMaxCompoundId(
			long databaseId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.GetMaxCompoundId,
								new Services.Native.NativeMethodTransportObject(new object[] { databaseId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				long maxCompoundId = (long)resultObject.Value;
				return maxCompoundId;
			}
			else return Instance.GetMaxCompoundId(databaseId);
		}

		/// <summary>
		/// Insert a new database header
		/// </summary>
		/// <param name="ucdb"></param>

		public void InsertDatabaseHeader(
			UcdbDatabase ucdb)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				ServiceTypes.UcdbDatabase serviceUcdbDatabase =
						ServiceFacade.TypeConversionHelper.Convert<UcdbDatabase, ServiceTypes.UcdbDatabase>(ucdb);

				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.InsertDatabaseHeader,
								new Services.Native.NativeMethodTransportObject(new object[] { serviceUcdbDatabase }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();

				UpdateUcdb(resultObject, ucdb);
			}

			else Instance.InsertDatabaseHeader(ucdb);
		}

		/// <summary>
		/// Update an existing database header
		/// </summary>
		/// <param name="ucdb"></param>

		public void UpdateDatabaseHeader(
			UcdbDatabase ucdb)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				ServiceTypes.UcdbDatabase serviceUcdbDatabase =
						ServiceFacade.TypeConversionHelper.Convert<UcdbDatabase, ServiceTypes.UcdbDatabase>(ucdb);

				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.UpdateDatabaseHeader,
								new Services.Native.NativeMethodTransportObject(new object[] { serviceUcdbDatabase }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();

				UpdateUcdb(resultObject, ucdb);
			}

			else Instance.UpdateDatabaseHeader(ucdb);
		}

		/// <summary>
		/// Update a database model association
		/// </summary>
		/// <param name="lda"></param>

		public void UpdateDatabaseModelAssoc(
			UcdbDatabase ucdb,
			UcdbModel[] dbModels)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				ServiceTypes.UcdbDatabase serviceUcdbDatabase =
						ServiceFacade.TypeConversionHelper.Convert<UcdbDatabase, ServiceTypes.UcdbDatabase>(ucdb);
				ServiceTypes.UcdbModel[] serviceUcdbModels =
						ServiceFacade.TypeConversionHelper.Convert<UcdbModel[], ServiceTypes.UcdbModel[]>(dbModels);

				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.UpdateDatabaseModelAssoc,
								new Services.Native.NativeMethodTransportObject(new object[] { serviceUcdbDatabase, serviceUcdbModels }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				UpdateUcdb(resultObject, ucdb);
			}
			else Instance.UpdateDatabaseModelAssoc(ucdb, dbModels);
		}

#if false
		/// <summary>
		/// Update all model results for the specified db
		/// </summary>
		/// <param name="databaseId"></param>

		public int UpdateDatabaseModelResults(
			long databaseId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUcdbService,
								(int)Services.Native.ServiceOpCodes.MobiusUcdbService.UpdateDatabaseModelResultsByDatabaseId,
								new Services.Native.NativeMethodTransportObject(new object[] { databaseId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				int updateCount = (int)resultObject.Value;
				return updateCount;
			}
			else return Instance.UpdateDatabaseModelResults(databaseId);
		}

		/// <summary>
		/// Update results for specified db & model
		/// </summary>
		/// <param name="databaseId"></param>
		/// <param name="modelId"></param>
		/// <returns></returns>

		public int UpdateDatabaseModelResults(
			long databaseId,
			long modelId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUcdbService,
								(int)Services.Native.ServiceOpCodes.MobiusUcdbService.UpdateDatabaseModelResultsByDatabaseIdAndModelId,
								new Services.Native.NativeMethodTransportObject(new object[] { databaseId, modelId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				int updateCount = (int)resultObject.Value;
				return updateCount;
			}
			else return Instance.UpdateDatabaseModelResults(databaseId, modelId);
		}

		/// <summary>
		/// Update all model results for a given compound
		/// </summary>
		/// <param name="cmpndId"></param>
		/// <returns></returns>

		public int UpdateCompoundModelResults(
			long compoundId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUcdbService,
								(int)Services.Native.ServiceOpCodes.MobiusUcdbService.UpdateCompoundModelResultsByCompoundId,
								new Services.Native.NativeMethodTransportObject(new object[] { compoundId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				int updateCount = (int)resultObject.Value;
				return updateCount;
			}
			else return Instance.UpdateCompoundModelResults(compoundId);
		}

		/// <summary>
		/// Update for a single compound & model
		/// </summary>
		/// <param name="compoundId"></param>
		/// <param name="modelId"></param>
		/// <returns></returns>

		public int UpdateCompoundModelResults(
			long compoundId,
			long modelId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUcdbService,
								(int)Services.Native.ServiceOpCodes.MobiusUcdbService.UpdateCompoundModelResultsByCompoundIdAndModelId,
								new Services.Native.NativeMethodTransportObject(new object[] { compoundId, modelId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				int updateCount = (int)resultObject.Value;
				return updateCount;
			}
			else return Instance.UpdateCompoundModelResults(compoundId, modelId);
		}
#endif

		/// <summary>
		/// Update the compounds for a database
		/// </summary>
		/// <param name="ucdb"></param>
		/// <param name="cpds"></param>
		/// <returns>New number of compounds in database</returns>

		public void UpdateDatabaseCompounds(
			UcdbDatabase ucdb,
			UcdbCompound[] cpds)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				ServiceTypes.UcdbDatabase serviceUcdbDatabase =
						ServiceFacade.TypeConversionHelper.Convert<UcdbDatabase, ServiceTypes.UcdbDatabase>(ucdb);
				ServiceTypes.UcdbCompound[] serviceUcdbCompounds =
						ServiceFacade.TypeConversionHelper.Convert<UcdbCompound[], ServiceTypes.UcdbCompound[]>(cpds);

				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.UpdateDatabaseCompounds,
								new Services.Native.NativeMethodTransportObject(new object[] { serviceUcdbDatabase, serviceUcdbCompounds }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				UpdateUcdb(resultObject, ucdb);
			}

			else Instance.UpdateDatabaseCompounds(ucdb, cpds);
		}

		/// <summary>
		/// Update pending model results for the specified database
		/// </summary>
		/// <param name="databaseId"></param>

#if false

		public int UpdatePendingModelResults(
			long databaseId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUcdbService,
								(int)Services.Native.ServiceOpCodes.MobiusUcdbService.UpdatePendingModelResults,
								new Services.Native.NativeMethodTransportObject(new object[] { databaseId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				int pendingCount = (int)resultObject.Value;
				return pendingCount;
			}
			else return Instance.UpdatePendingModelResults(databaseId);
		}
#endif

		/// <summary>
		/// Delete all compound data for a database 
		/// </summary>
		/// <param name="databaseId"></param>
		/// <returns></returns>

		public long DeleteDatabaseCompounds(
			long databaseId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.DeleteDatabaseCompounds,
								new Services.Native.NativeMethodTransportObject(new object[] { databaseId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				long deleteCount = (long)resultObject.Value;
				return deleteCount;
			}
			else return Instance.DeleteDatabaseCompounds(databaseId);
		}

		/// <summary>
		/// Update an existing database
		/// </summary>
		/// <param name="ucdb"></param>

		public void UpdateDatabase(
			UcdbDatabase ucdb)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				ServiceTypes.UcdbDatabase serviceUcdbDatabase =
						ServiceFacade.TypeConversionHelper.Convert<UcdbDatabase, ServiceTypes.UcdbDatabase>(ucdb);

				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.UpdateDatabase,
								new Services.Native.NativeMethodTransportObject(new object[] { serviceUcdbDatabase }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				UpdateUcdb(resultObject, ucdb);
			}
			else Instance.UpdateDatabase(ucdb);
		}

		/// <summary>
		/// Update the local copy of the Ucdb object with returned values keeping compounds & models if returned as null
		/// </summary>
		/// <param name="resultObject"></param>
		/// <param name="ucdb"></param>

		void UpdateUcdb(
			Services.Native.NativeMethodTransportObject resultObject,
			UcdbDatabase ucdb)
		{
			if (resultObject == null || resultObject.Value == null || ucdb == null) return;

			ServiceTypes.UcdbDatabase serviceUcdbDatabase =
				 (ServiceTypes.UcdbDatabase)resultObject.Value;
			UcdbDatabase ucdbDatabase =
					ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.UcdbDatabase, UcdbDatabase>(serviceUcdbDatabase);

			UcdbCompound[] compounds = ucdb.Compounds;
			UcdbModel[] models = ucdb.Models;
			ObjectEx.MemberwiseCopy(ucdbDatabase, ucdb); // copy values back
			if (ucdb.Compounds == null) ucdb.Compounds = compounds;
			if (ucdb.Models == null) ucdb.Models = models;
			return;
		}

		/// <summary>
		/// Write message to log with exclusive access
		/// </summary>
		/// <param name="message"></param>

		public void LogMessage(
			string message)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserCmpndDbService,
								(int)Services.Native.ServiceOpCodes.MobiusUserCmpndDbService.LogMessage,
								new Services.Native.NativeMethodTransportObject(new object[] { message }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
			}
			else Instance.LogMessage(message);
		}

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				//nothing to do
			}
			//else Instance.Dispose();

			return;
		}

	}
}
