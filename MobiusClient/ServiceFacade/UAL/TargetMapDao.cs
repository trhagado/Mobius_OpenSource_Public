using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections.Generic;
using System.Text;

using ServiceTypes = Mobius.Services.Types;


namespace Mobius.ServiceFacade
{
	public class TargetMapDao
	{
		UAL.TargetMapDao ServerInstance;

    public TargetMapDao()
		{
			ServerInstance = new UAL.TargetMapDao();
		}

		/// <summary>
/// Get image & coordinates file from KEGG FTP
/// </summary>
/// <param name="pathwayId"></param>
/// <returns></returns>

		public static TargetMap GetKeggPathway(
			string pathwayId)
		{
            if (ServiceFacade.UseRemoteServices)
            {
                    Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
                    Services.Native.NativeMethodTransportObject resultObject =
                        ServiceFacade.InvokeNativeMethod(nativeClient,
                            (int)Services.Native.ServiceCodes.MobiusTargetMapService,
                            (int)Services.Native.ServiceOpCodes.MobiusTargetMapService.GetKeggPathway,
                            new Services.Native.NativeMethodTransportObject(new object[] { pathwayId }));
                    ((System.ServiceModel.IClientChannel)nativeClient).Close();
										if (resultObject == null) return null;
										ServiceTypes.TargetMap serviceTargetMap = (ServiceTypes.TargetMap)resultObject.Value;
                    TargetMap targetMap =
                        ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.TargetMap, TargetMap>(serviceTargetMap);
                    return targetMap;
            }
            else return UAL.TargetMapDao.GetKeggPathway(pathwayId);
		}

/// <summary>
/// Get list of common map names
/// </summary>
/// <returns></returns>

		public static List<string> GetCommonMapNames()
		{
            if (ServiceFacade.UseRemoteServices)
            {
                    Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
                    Services.Native.NativeMethodTransportObject resultObject =
                        ServiceFacade.InvokeNativeMethod(nativeClient,
                            (int)Services.Native.ServiceCodes.MobiusTargetMapService,
                            (int)Services.Native.ServiceOpCodes.MobiusTargetMapService.GetCommonMapNames,
                            null);
                    ((System.ServiceModel.IClientChannel)nativeClient).Close();
										if (resultObject == null) return null;
										List<string> mapNames = (List<string>)resultObject.Value;
                    return mapNames;
            }
            else return UAL.TargetMapDao.GetCommonMapNames();
		}

		/// <summary>
		/// Return the dictionary of target map names and labels
		/// </summary>
		/// <returns></returns>

		public static Dictionary<string, string> GetTargetNamesAndLabels()
		{
            if (ServiceFacade.UseRemoteServices)
            {
                    Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
                    Services.Native.NativeMethodTransportObject resultObject =
                        ServiceFacade.InvokeNativeMethod(nativeClient,
                            (int)Services.Native.ServiceCodes.MobiusTargetMapService,
                            (int)Services.Native.ServiceOpCodes.MobiusTargetMapService.GetTargetNamesAndLabels,
                            null);
                    ((System.ServiceModel.IClientChannel)nativeClient).Close();
										if (resultObject == null) return null;
										Dictionary<string, string> dict = (Dictionary<string, string>)resultObject.Value;
                    return dict;
            }
            else return UAL.TargetMapDao.GetTargetNamesAndLabels();
		}

/// <summary>
/// Get a target map by name
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public static TargetMap GetMap(
			string name)
		{
			if (ServiceFacade.UseRemoteServices)
			{
					Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
					Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
						(int)Services.Native.ServiceCodes.MobiusTargetMapService,
						(int)Services.Native.ServiceOpCodes.MobiusTargetMapService.GetMap,
						new Services.Native.NativeMethodTransportObject(new object[] { name }));
					((System.ServiceModel.IClientChannel)nativeClient).Close();
					if (resultObject == null) return null;
					ServiceTypes.TargetMap serviceTargetMap = (ServiceTypes.TargetMap)resultObject.Value;
					TargetMap targetMap =
						ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.TargetMap, TargetMap>(serviceTargetMap);
					return targetMap;
			}
			else return UAL.TargetMapDao.GetMap(name);
		}

/// <summary>
/// Get a map by name with coordinates
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public static TargetMap GetMapWithCoords(
			string name)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
					ServiceFacade.InvokeNativeMethod(nativeClient,
					(int)Services.Native.ServiceCodes.MobiusTargetMapService,
					(int)Services.Native.ServiceOpCodes.MobiusTargetMapService.GetMapWithCoords,
					new Services.Native.NativeMethodTransportObject(new object[] { name }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				ServiceTypes.TargetMap serviceTargetMap = (ServiceTypes.TargetMap)resultObject.Value;
				TargetMap targetMap =
						ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.TargetMap, TargetMap>(serviceTargetMap);
				return targetMap;
			}
			else return UAL.TargetMapDao.GetMapWithCoords(name);
		}

/// <summary>
/// Get a target map image by name
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public static byte[] GetMapImage(
			string name)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
					ServiceFacade.InvokeNativeMethod(nativeClient,
					(int)Services.Native.ServiceCodes.MobiusTargetMapService,
					(int)Services.Native.ServiceOpCodes.MobiusTargetMapService.GetMapImage,
					new Services.Native.NativeMethodTransportObject(new object[] { name }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				byte[] imageBytes = (byte[])resultObject.Value;
				return imageBytes;
			}
			else return UAL.TargetMapDao.GetMapImage(name);
		}

	}
}
