using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;
using Mobius.QueryEngineLibrary;
using Qel = Mobius.QueryEngineLibrary;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceTypes = Mobius.Services.Types;


namespace Mobius.ServiceFacade
{
	public class CompoundIdUtil
	{

		/// <summary>
		/// See if a compound id exists
		/// </summary>
		/// <param name="cid"></param>
		/// <returns></returns>

		public static bool Exists(
			string cid)
		{
			return Exists(cid, null);
		}

		/// <summary>
		/// See if a compound id exists
		/// </summary>
		/// <param name="cid"></param>
		/// <param name="mt">MetaTable to check in or if null check based on prefix</param>
		/// <returns></returns>

		public static bool Exists(
			string cid,
			MetaTable mt)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				string mtName = (mt == null) ? null : mt.Name;
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusCompoundUtilService,
								(int)Services.Native.ServiceOpCodes.MobiusCompoundUtilService.DoesCidExist,
								new Services.Native.NativeMethodTransportObject(new object[] { cid, mtName }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				bool cidExists = (resultObject != null) ? (bool)resultObject.Value : false;
				return cidExists;
			}
			else return Qel.CompoundIdUtil.Exists(cid, mt);
		}

		/// <summary>
		/// Check that each of the numbers in the list exist in the database
		/// </summary>
		/// <param name="listText">String form of list as entered by user</param>
		/// <param name="rootTableName">Root table to check against</param>
		/// <returns></returns>

		public static string ValidateList(
			string listText,
			string rootTableName)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusCompoundUtilService,
								(int)Services.Native.ServiceOpCodes.MobiusCompoundUtilService.ValidateList,
								new Services.Native.NativeMethodTransportObject(new object[] { listText, rootTableName }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				string result = (string)resultObject.Value;
				return result;
			}

			else return Qel.CompoundIdUtil.ValidateList(listText, rootTableName);
		}
	}
}
