using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections.Generic;
using System.Text;

using ServiceTypes = Mobius.Services.Types;

namespace Mobius.ServiceFacade
{
	public class QueryReader
	{
		/// <summary>
		/// Lookup a query by matching the supplied substring against the query name
		/// </summary>
		/// <param name="querySubString"></param>
		/// <param name="msg"></param>
		/// <returns></returns>

		public static int FindQueryByNameSubstring(string querySubString, out string msg)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
					ServiceFacade.InvokeNativeMethod(nativeClient,
						(int)Services.Native.ServiceCodes.MobiusUserObjectService,
						(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.FindQueryByNameSubstring,
						new Services.Native.NativeMethodTransportObject(new object[] { querySubString }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				ServiceTypes.FindQueryByNameSubstringResult result =
					(ServiceTypes.FindQueryByNameSubstringResult)resultObject.Value;
				msg = result.resultMsg;
				return result.resultInt;
			}
			else return Mobius.QueryEngineLibrary.QueryUserObjectReader.FindQueryByNameSubstring(querySubString, out msg);
		}

		/// <summary>
		/// Read user object given the object id
		/// </summary>
		/// <param name="objectId">id of item to read</param>
		/// <returns>The requested user object or null if no matching user object is found.</returns>

		public static UserObject ReadQueryWithMetaTables(int objectId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
					ServiceFacade.InvokeNativeMethod(nativeClient,
						(int)Services.Native.ServiceCodes.MobiusUserObjectService,
						(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.ReadQueryWithMetaTables1,
						new Services.Native.NativeMethodTransportObject(new object[] { objectId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null || resultObject.Value == null) return null;

				UserObject uo = UserObject.DeserializeBinary((byte[])resultObject.Value);
				return uo;
			}

			else return Mobius.QueryEngineLibrary.QueryUserObjectReader.ReadQueryWithMetaTables(objectId);
		}

		/// <summary>
		/// Read object matching the type, folder, owner, name of the given user object
		/// </summary>
		/// <param name="uo"></param>
		/// <returns>The requested user object or null if no matching user object is found.</returns>

		public static UserObject ReadQueryWithMetaTables(UserObject uo)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				byte[] sUo = uo.SerializeBinary();
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
					ServiceFacade.InvokeNativeMethod(nativeClient,
						(int)Services.Native.ServiceCodes.MobiusUserObjectService,
						(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.ReadQueryWithMetaTables2,
						new Services.Native.NativeMethodTransportObject(new object[] { sUo }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null || resultObject.Value == null) return null;
				uo = UserObject.DeserializeBinary((byte[])resultObject.Value);
				return uo;
			}

			else return Mobius.QueryEngineLibrary.QueryUserObjectReader.ReadQueryWithMetaTables(uo);
		}

	}
}
