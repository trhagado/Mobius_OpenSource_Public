using System;
using System.Collections.Generic;
using System.Text;

using Mobius.Data;
using ServiceTypes = Mobius.Services.Types;


namespace Mobius.ServiceFacade
{
	public class CidListDao
	{
		/// <summary>
		/// Read a compoundId list given the listId
		/// </summary>
		/// <param name="listId"></param>
		/// <param name="mt"></param>
		/// <returns></returns>

				public static CidList Read(
					int listId,
					MetaTable mt)
				{
					if (ServiceFacade.UseRemoteServices)
					{
							Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
							Services.Native.NativeMethodTransportObject resultObject =
								ServiceFacade.InvokeNativeMethod(nativeClient,
								 (int)Services.Native.ServiceCodes.MobiusCidListService,
								 (int)Services.Native.ServiceOpCodes.MobiusCidListService.Read,
								 new Services.Native.NativeMethodTransportObject(new object[] { listId, mt.Name }));
							((System.ServiceModel.IClientChannel)nativeClient).Close();
							if (resultObject == null || resultObject.Value == null) return null;
							UserObject uo = UserObject.DeserializeBinary((byte[])resultObject.Value);
							CidList cidList = CidList.Deserialize(uo, mt);
							return cidList;
					}
					else return UAL.CidListDao.Read(listId, mt);
				}

				/// <summary>
				/// Read the list of ids for a compound library
				/// </summary>
				/// <param name="libId"></param>
				/// <returns></returns>

				public static CidList ReadLibrary(
					int libId)
				{
					if (ServiceFacade.UseRemoteServices)
					{
							Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
							Services.Native.NativeMethodTransportObject resultObject =
								ServiceFacade.InvokeNativeMethod(nativeClient,
								 (int)Services.Native.ServiceCodes.MobiusCidListService,
								 (int)Services.Native.ServiceOpCodes.MobiusCidListService.ReadLibrary,
								 new Services.Native.NativeMethodTransportObject(new object[] { libId }));
							((System.ServiceModel.IClientChannel)nativeClient).Close();
							if (resultObject == null || resultObject.Value == null) return null;
							UserObject uo = UserObject.DeserializeBinary((byte[])resultObject.Value);
							MetaTable mt = MetaTableCollection.Get(MetaTable.PrimaryRootTable);
							CidList cidList = CidList.Deserialize(uo, mt);
							return cidList;
					}

					else return UAL.CidListDao.ReadLibrary(libId);
				}

		/// <summary>
		/// Execute list logic on a pair of persisted list objects & store results in current list for user
		/// </summary>
		/// <param name="list1InternalName"></param>
		/// <param name="list2InternalName"></param>
		/// <param name="op"></param>
		/// <returns>Number of results from logic operation</returns>

				public static int ExecuteListLogic(
					string list1InternalName,
					string list2InternalName,
					ListLogicType op)
				{
					if (ServiceFacade.UseRemoteServices)
					{
							Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
							Services.Native.NativeMethodTransportObject resultObject =
								ServiceFacade.InvokeNativeMethod(nativeClient,
									(int)Services.Native.ServiceCodes.MobiusCidListService,
									(int)Services.Native.ServiceOpCodes.MobiusCidListService.ExecuteListLogic,
									new Services.Native.NativeMethodTransportObject(new object[] { list1InternalName, list2InternalName, (int)op }));
							((System.ServiceModel.IClientChannel)nativeClient).Close();
							int result = (int)resultObject.Value;
							return result;
					}
					else return UAL.CidListDao.ExecuteListLogic(list1InternalName, list2InternalName, op);
				}

		/// <summary>
		/// Copy a list to another list
		/// </summary>
		/// <param name="sourceList"></param>
		/// <param name="destList"></param>
		/// <returns></returns>

		public static int CopyList(
			UserObject sourceList,
			UserObject destList)
		{
            if (ServiceFacade.UseRemoteServices)
            {
                    ServiceTypes.UserObjectNode destListNode =
                        ServiceFacade.TypeConversionHelper.Convert<UserObject, ServiceTypes.UserObjectNode>(destList);
                    Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
                    Services.Native.NativeMethodTransportObject resultObject =
                        ServiceFacade.InvokeNativeMethod(nativeClient,
                            (int)Services.Native.ServiceCodes.MobiusCidListService,
                            (int)Services.Native.ServiceOpCodes.MobiusCidListService.CopyList,
                            new Services.Native.NativeMethodTransportObject(new object[] { sourceList.InternalName, destListNode }));
                    ((System.ServiceModel.IClientChannel)nativeClient).Close();
                    
                    //update the destList uo in place
                    destListNode = (ServiceTypes.UserObjectNode)resultObject.Value;
                    UserObject updatedDestList =
                        ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.UserObjectNode,UserObject>(destListNode);
                    destList.Id = updatedDestList.Id;
                    destList.Name = updatedDestList.Name;
                    destList.Type = updatedDestList.Type;
                    destList.Owner = updatedDestList.Owner;
                    destList.AccessLevel = updatedDestList.AccessLevel;
                    destList.ParentFolder = updatedDestList.ParentFolder;
                    destList.ParentFolderType = updatedDestList.ParentFolderType;
                    destList.Description = updatedDestList.Description;
                    destList.CreationDateTime = updatedDestList.CreationDateTime;
                    destList.UpdateDateTime = updatedDestList.UpdateDateTime;
                    destList.Content = updatedDestList.Content;
                    destList.Count = updatedDestList.Count;
                    
                    return destList.Count;
            }
            else return UAL.CidListDao.CopyList(sourceList, destList);
		}

	}
}
