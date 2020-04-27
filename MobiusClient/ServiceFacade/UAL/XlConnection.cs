using Mobius.ComOps;
using Mobius.UAL;

using System;
using System.Collections.Generic;
using System.Text;

using ServiceTypes = Mobius.Services.Types;


namespace Mobius.ServiceFacade
{
/// <summary>
/// Oracle connection information facade
/// </summary>

	public class DbConnection
	{

		/// <summary>
		/// Load datasource metadata
		/// </summary>

		public static void LoadMetaData()
		{
            if (ServiceFacade.UseRemoteServices)
            {
                    Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
                    Services.Native.NativeMethodTransportObject resultObject =
                        ServiceFacade.InvokeNativeMethod(nativeClient,
                        (int)Services.Native.ServiceCodes.MobiusDataConnectionAdminService,
                        (int)Services.Native.ServiceOpCodes.MobiusDataConnectionAdminService.LoadMetaData,
                        null);
                    ((System.ServiceModel.IClientChannel)nativeClient).Close();
                    return;
            }
            else UAL.DataSourceMx.LoadMetadata();
		}

		/// <summary>
		/// Check for connection leaks and log if found
		/// </summary>
		/// <returns></returns>

		public static string CheckForConnectionLeaks()
		{
			if (ServiceFacade.UseRemoteServices)
			{
					Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
					Services.Native.NativeMethodTransportObject resultObject =
							ServiceFacade.InvokeNativeMethod(nativeClient,
									(int)Services.Native.ServiceCodes.MobiusDataConnectionAdminService,
									(int)Services.Native.ServiceOpCodes.MobiusDataConnectionAdminService.CheckForConnectionLeaks,
									null);
					((System.ServiceModel.IClientChannel)nativeClient).Close();
					if (resultObject == null) return null;
					string result = (string)resultObject.Value;
					return result;
			}
			else return UAL.DbConnectionMx.CheckForConnectionLeaks();
		}

/// <summary>
/// Get list of existing Oracle connections
/// </summary>
/// <returns></returns>

		public static string GetOracleConnections()
		{
            if (ServiceFacade.UseRemoteServices)
            {
                    Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
                    Services.Native.NativeMethodTransportObject resultObject =
                        ServiceFacade.InvokeNativeMethod(nativeClient,
                            (int)Services.Native.ServiceCodes.MobiusDataConnectionAdminService,
                            (int)Services.Native.ServiceOpCodes.MobiusDataConnectionAdminService.GetOracleConnections,
                            null);
                    ((System.ServiceModel.IClientChannel)nativeClient).Close();
										if (resultObject == null) return null;
										string result = (string)resultObject.Value;
                    return result;
            }
            else return UAL.DbConnectionMx.GetActiveDatabaseConnections();
		}

/// <summary>
/// Test each connection & return text of results
/// </summary>
/// <returns></returns>

			public static string TestConnections ()
			{
                if (ServiceFacade.UseRemoteServices)
                {
                        Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
                        Services.Native.NativeMethodTransportObject resultObject =
                            ServiceFacade.InvokeNativeMethod(nativeClient,
                                (int)Services.Native.ServiceCodes.MobiusDataConnectionAdminService,
                                (int)Services.Native.ServiceOpCodes.MobiusDataConnectionAdminService.TestConnections,
                                null);
                        ((System.ServiceModel.IClientChannel)nativeClient).Close();
												if (resultObject == null) return null;
												string result = (string)resultObject.Value;
                        return result;
                }
                else return UAL.DbConnectionMx.TestConnections();
			}

		/// <summary>
/// Update database links from each instance to every other instance
/// </summary>
/// <returns></returns>

			public static string UpdateDatabaseLinks (
				string singleInstance)
			{
                if (ServiceFacade.UseRemoteServices)
                {
                        Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
                        Services.Native.NativeMethodTransportObject resultObject =
                            ServiceFacade.InvokeNativeMethod(nativeClient,
                                (int)Services.Native.ServiceCodes.MobiusDataConnectionAdminService,
                                (int)Services.Native.ServiceOpCodes.MobiusDataConnectionAdminService.UpdateDatabaseLinks,
                                new Services.Native.NativeMethodTransportObject(new object[] { singleInstance }));
                        ((System.ServiceModel.IClientChannel)nativeClient).Close();
												if (resultObject == null) return null;
												string result = (string)resultObject.Value;
                        return result;
                }
                else return UAL.DbConnectionMx.UpdateDatabaseLinks(singleInstance);
			}

            public static string DefineDataSource(string args)
            {
                if (ServiceFacade.UseRemoteServices)
                {
                    Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
                    Services.Native.NativeMethodTransportObject resultObject =
                        ServiceFacade.InvokeNativeMethod(nativeClient,
                            (int)Services.Native.ServiceCodes.MobiusDataConnectionAdminService,
                            (int)Services.Native.ServiceOpCodes.MobiusDataConnectionAdminService.DefineDataSource,
                            new Services.Native.NativeMethodTransportObject(new object[] { args }));
                    ((System.ServiceModel.IClientChannel)nativeClient).Close();
                    if (resultObject == null) return null;
                    string result = (string)resultObject.Value;
                    return result;
                }
                else return UAL.DbConnectionMx.DefineDataSource(args);
            }

            public static string AssociateSchema(string args)
            {
                if (ServiceFacade.UseRemoteServices)
                {
                    Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
                    Services.Native.NativeMethodTransportObject resultObject =
                        ServiceFacade.InvokeNativeMethod(nativeClient,
                            (int)Services.Native.ServiceCodes.MobiusDataConnectionAdminService,
                            (int)Services.Native.ServiceOpCodes.MobiusDataConnectionAdminService.AssociateSchema,
                            new Services.Native.NativeMethodTransportObject(new object[] { args }));
                    ((System.ServiceModel.IClientChannel)nativeClient).Close();
                    if (resultObject == null) return null;
                    string result = (string)resultObject.Value;
                    return result;
                }
                else return UAL.DbConnectionMx.AssociateSchema(args);
            }

    }
}
