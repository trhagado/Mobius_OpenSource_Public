using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Mobius.ComOps;
using Mobius.Services.Util.TypeConversionUtil;
using Mobius.Services.Types;
using Mobius.Services.Native.ServiceOpCodes;

namespace Mobius.Services.Native.OpInvokers
{
	public class MobiusUserObjectServiceOpInvoker : IInvokeServiceOps
	{
		private static TypeConversionHelper _transHelper = new TypeConversionHelper(TypeConversionHelper.ContextRetentionMode.NoRetention);

		object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
		{
			Mobius.Data.UserObject uo;
			byte[] sUo;
			List<Mobius.Data.UserObject> uoList;

			MobiusUserObjectService op = (MobiusUserObjectService)opCode;
			switch (op)
			{
				case MobiusUserObjectService.Ping:
					{
						UAL.UserObjectDao.Ping();
						return true;
					}

				case MobiusUserObjectService.ReadMultiple1:
					{
						Types.UserObjectType type = (Types.UserObjectType)args[0];
						bool includeContent = (bool)args[1];
						uoList = UAL.UserObjectDao.ReadMultiple((Data.UserObjectType)type, includeContent);
						return Mobius.Data.UserObject.SerializeListBinary(uoList);
					}

				case MobiusUserObjectService.ReadMultiple2:
					{
						int type = (int)args[0];
						string userId = (string)args[1];
						bool includeShared = (bool)args[2];
						bool includeContent = (bool)args[3];
						uoList = Mobius.UAL.UserObjectDao.ReadMultiple((Mobius.Data.UserObjectType)type, userId, includeShared, includeContent);
						return Mobius.Data.UserObject.SerializeListBinary(uoList);
					}

				case MobiusUserObjectService.ReadMultiple3:
					{
						string userId = (string)args[0];
						bool includeShared = (bool)args[1];
						bool includeContent = (bool)args[2];
						uoList = Mobius.UAL.UserObjectDao.ReadMultiple(userId, includeShared, includeContent);
						return Mobius.Data.UserObject.SerializeListBinary(uoList);
					}

				case MobiusUserObjectService.ReadMultiple4:
					{
						string folderName = (string)args[0];
						string userId = (string)args[1];
						bool includeShared = (bool)args[2];
						bool includeContent = (bool)args[3];
						uoList = Mobius.UAL.UserObjectDao.ReadMultiple(folderName, userId, includeShared, includeContent);
						return Mobius.Data.UserObject.SerializeListBinary(uoList);
					}

				case MobiusUserObjectService.ReadMultiple5:
					{
						int[] uoIdsArray = args[0] as int[];
						List<int> uoIds = new List<int>(uoIdsArray);
						bool includeContent = (bool)args[1];
						uoList = Mobius.UAL.UserObjectDao.ReadMultiple(uoIds, includeContent);
						return Mobius.Data.UserObject.SerializeListBinary(uoList);
					}

				case MobiusUserObjectService.GetUserParameter:
					{
						string userId = (string)args[0];
						string paramName = (string)args[1];
						string defaultValue = (string)args[2];
						string paramValue = UAL.UserObjectDao.GetUserParameter(userId, paramName, defaultValue);
						return paramValue;
					}

				case MobiusUserObjectService.SetUserParameter:
					{
						string userId = (string)args[0];
						string paramName = (string)args[1];
						string paramValue = (string)args[2];
						bool result = false;
						try
						{
							Mobius.UAL.UserObjectDao.SetUserParameter(userId, paramName, paramValue);
							result = true;
						}
						catch (Exception) { result = false; } // shouldn't happen
						return result;
					}

				case MobiusUserObjectService.Exists:
					{
						sUo = (byte[])args[0];
						uo = Mobius.Data.UserObject.DeserializeBinary(sUo);
						bool exists = Mobius.UAL.UserObjectDao.Exists(uo);
						return exists;
					}

				case MobiusUserObjectService.ReadHeader1:
					{
						int objectId = (int)args[0];
						uo = Mobius.UAL.UserObjectDao.ReadHeader(objectId);
						if (uo == null) return null;
						sUo = uo.SerializeBinary();
						return sUo;
					}

				case MobiusUserObjectService.ReadHeader2:
					{
						Mobius.Services.Types.UserObjectType type = (Mobius.Services.Types.UserObjectType)args[0];
						string owner = (string)args[1];
						string folderId = (string)args[2];
						string name = (string)args[3];
						uo = Mobius.UAL.UserObjectDao.ReadHeader((Data.UserObjectType)type, owner, folderId, name);
						if (uo == null) return null;
						sUo = uo.SerializeBinary();
						return sUo;
					}

				case MobiusUserObjectService.UpdateHeader:
					{
						sUo = (byte[])args[0];
						uo = Mobius.Data.UserObject.DeserializeBinary(sUo);
						bool setUpdateDateToCurrentDate = (bool)args[1];
						bool result = false;
						try
						{
							Mobius.UAL.UserObjectDao.UpdateHeader(uo, setUpdateDateToCurrentDate);
							result = true;
						}
						catch (Exception) { result = false; } // shouldn't happen
						return result;
					}

				case MobiusUserObjectService.UpdateUpdateDateAndCount:
					{
						int objectId = (int)args[0];
						DateTime updateDateTime = (DateTime)args[1];
						int count = (int)args[2];
						bool result = false;
						try
						{
							Mobius.UAL.UserObjectDao.UpdateUpdateDateAndCount(objectId, updateDateTime, count);
							result = true;
						}
						catch (Exception) { result = false; } // shouldn't happen
						return result;
					}
				case MobiusUserObjectService.GetNextId:
					{
						int nextId = UAL.UserObjectDao.GetNextId();
						return nextId;
					}

				case MobiusUserObjectService.Write:
					{
						sUo = (byte[])args[0];
						uo = Mobius.Data.UserObject.DeserializeBinary(sUo);
						int uoId = (int)args[1];
						try
						{
							UAL.UserObjectDao.Write(uo, uoId);
							sUo = uo.SerializeBinary();
							return sUo;
						}
						catch (Exception) { return null; } // shouldn't happen
					}

				case MobiusUserObjectService.Read1:
					{
						int objectId = (int)args[0];
						uo = Mobius.UAL.UserObjectDao.Read(objectId);
						if (uo == null) return null;
						sUo = uo.SerializeBinary();
						return sUo;
					}

				case MobiusUserObjectService.Read2:
					{
						sUo = (byte[])args[0];
						uo = Mobius.Data.UserObject.DeserializeBinary(sUo);
						uo = Mobius.UAL.UserObjectDao.Read(uo);
						if (uo == null) return null;
						sUo = uo.SerializeBinary();
						return sUo;
					}

				case MobiusUserObjectService.Read3:
					{
						Mobius.Services.Types.UserObjectType type = (Mobius.Services.Types.UserObjectType)args[0];
						string owner = (string)args[1];
						string folderId = (string)args[2];
						string name = (string)args[3];
						uo = Mobius.UAL.UserObjectDao.Read((Data.UserObjectType)type, owner, folderId, name);
						if (uo == null) return null;
						sUo = uo.SerializeBinary();
						return sUo;
					}

				case MobiusUserObjectService.Delete:
					{
						int objectId = (int)args[0];
						bool result = UAL.UserObjectDao.Delete(objectId);
						return result;
					}

				case MobiusUserObjectService.FindQueryByNameSubstring:
					{
						string querySubString = (string)args[0];
						FindQueryByNameSubstringResult result = null;
						if (querySubString != null)
						{
							result = new FindQueryByNameSubstringResult();
							result.resultInt = Mobius.QueryEngineLibrary.QueryUserObjectReader.FindQueryByNameSubstring(querySubString, out result.resultMsg);
						}
						return result;
					}

				case MobiusUserObjectService.ReadQueryWithMetaTables1:
					{
						int objectId = (int)args[0];
						uo = Mobius.QueryEngineLibrary.QueryUserObjectReader.ReadQueryWithMetaTables(objectId);
						if (uo == null) return null;
						sUo = uo.SerializeBinary();
						return sUo;
					}

				case MobiusUserObjectService.ReadQueryWithMetaTables2:
					{
						sUo = (byte[])args[0];
						uo = Mobius.Data.UserObject.DeserializeBinary(sUo);
						uo = Mobius.QueryEngineLibrary.QueryUserObjectReader.ReadQueryWithMetaTables(uo);
						if (uo == null) return null;
						sUo = uo.SerializeBinary();
						return sUo;
					}

				case MobiusUserObjectService.AssureAccessToSharedQueryUserObjects:
					{
						int objectId = (int)args[0];
						Mobius.UAL.UserObjectDao.AssureAccessToSharedQueryUserObjects(objectId);
						return null;
					}

				case MobiusUserObjectService.GetSavedQueryMQL:
					{
						int objectId = (int)args[0];

						bool mobileQuery = false;
						try
						{
							if (args.Length > 1 && args[1] != null)
							{
								mobileQuery = (bool)args[1];
							}
						}
						catch (Exception)
						{
							// ignored
						}

						string mql;

						mql = mobileQuery ?
								Mobius.QueryEngineLibrary.QueryUserObjectReader.GetSavedQueryMQL(objectId, true) :
								Mobius.QueryEngineLibrary.QueryUserObjectReader.GetSavedQueryMQL(objectId);
						return mql;
					}

				case MobiusUserObjectService.SetMobileDefaultQueryForOwner:
					{
						try
						{
							string queryIdString = (string)args[0];
							int queryId;

							try
							{
								queryId = Convert.ToInt32(queryIdString);
							}
							catch (Exception ex)
							{
								DebugLog.Message("Unable to parse queryId " + queryIdString + " to an Integer.");
								return false;
							}

							string ownerId = (string)args[1];
							uo = Mobius.QueryEngineLibrary.QueryUserObjectReader.ReadQueryWithMetaTables(queryId);
							if (uo == null)
							{
								DebugLog.Message(queryId + " is an invalid Query ID");
								DebugLog.Message("OwnerId: " + ownerId);
								return false;
							}
							return UAL.UserObjectDao.SetMobileDefaultQueryForOwner(uo, ownerId);
						}
						catch (Exception ex)
						{
							DebugLog.Message(DebugLog.FormatExceptionMessage(ex));
						}
						return false;
					}
				case MobiusUserObjectService.GetDefaultQuery:
					{
						string user = (string)args[0];
						int queryId = QueryEngineLibrary.QueryEngine.GetDefaultQuery(user);
						return queryId;
					}



			}
			return null;
		}

	}
}
