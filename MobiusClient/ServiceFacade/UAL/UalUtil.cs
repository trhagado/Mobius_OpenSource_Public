using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections.Generic;
using System.Text;

using ServiceTypes = Mobius.Services.Types;

namespace Mobius.ServiceFacade
{
	public class UalUtil
	{

		public static void Initialize(string iniFileName)
		{
			UAL.UalUtil.Initialize(iniFileName);
		}

		/// <summary>
		/// Get the content of the services inifile
		/// </summary>
		/// <returns></returns>

		public static string GetServicesIniFile()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUalUtilService,
								(int)Services.Native.ServiceOpCodes.MobiusUalUtilService.GetServicesIniFile,
								new Services.Native.NativeMethodTransportObject(null));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null || resultObject.Value == null) return null;
				string iniFileString = resultObject.Value.ToString();
				return iniFileString;
			}

			else return UAL.UalUtil.GetServicesIniFile();
		}

		public static bool TryGetMobiusSystemWindowsAccount(
			out string userName,
			out string pw)
		{
			if (ServiceFacade.UseRemoteServices)
				throw new Exception("Account information is only available when Mobius is being run in \"Services - Integrated\" mode");

			return AccountAccessMx.TryGetMobiusSystemWindowsAccount(out userName, out pw);
		}

		public static bool NoDatabaseAccessIsAvailable {
			get => UAL.DbConnectionMx.NoDatabaseAccessIsAvailable;
			set => UAL.DbConnectionMx.NoDatabaseAccessIsAvailable = value;
		}

		/// <summary>
		/// Select an Oracle Blob value
		/// </summary>
		/// <param name="table"></param>
		/// <param name="matchCol"></param>
		/// <param name="typeCol"></param>
		/// <param name="contentCol"></param>
		/// <param name="matchVal"></param>
		/// <returns></returns>

		public static void SelectOracleBlob(
				string table,
				string matchCol,
				string typeCol,
				string contentCol,
				string matchVal,
				out string typeVal,
				out byte[] ba)
		{
			if (ServiceFacade.UseRemoteServices)
			{
					Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
					Services.Native.NativeMethodTransportObject resultObject =
							ServiceFacade.InvokeNativeMethod(nativeClient,
									(int)Services.Native.ServiceCodes.MobiusUalUtilService,
									(int)Services.Native.ServiceOpCodes.MobiusUalUtilService.SelectOracleBlob,
									new Services.Native.NativeMethodTransportObject(new object[] { table, matchCol, typeCol, contentCol, matchVal }));
					((System.ServiceModel.IClientChannel)nativeClient).Close();
					ServiceTypes.TypedBlob typedBlob = (ServiceTypes.TypedBlob)resultObject.Value;
					typeVal = typedBlob.Type;
					ba = typedBlob.Data;
			}
			else UAL.UalUtil.SelectOracleBlob(table, matchCol, typeCol, contentCol, matchVal, out typeVal, out ba);
		}

		/// <summary>
		/// Select an Oracle Clob value
		/// </summary>
		/// <param name="table"></param>
		/// <param name="matchCol"></param>
		/// <param name="typeCol"></param>
		/// <param name="contentCol"></param>
		/// <param name="matchVal"></param>
		/// <returns></returns>

		public static void SelectOracleClob(
				string table,
				string matchCol,
				string typeCol,
				string contentCol,
				string matchVal,
				out string typeVal,
				out string clobString)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUalUtilService,
								(int)Services.Native.ServiceOpCodes.MobiusUalUtilService.SelectOracleClob,
								new Services.Native.NativeMethodTransportObject(new object[] { table, matchCol, typeCol, contentCol, matchVal }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				ServiceTypes.TypedClob typedClob = (ServiceTypes.TypedClob)resultObject.Value;
				typeVal = typedClob.Type;
				clobString = typedClob.Data;
			}
			else UAL.UalUtil.SelectOracleClob(table, matchCol, typeCol, contentCol, matchVal, out typeVal, out clobString);
		}


	}
}
