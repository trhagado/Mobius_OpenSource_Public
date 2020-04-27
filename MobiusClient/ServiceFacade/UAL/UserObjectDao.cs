using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using ServiceTypes = Mobius.Services.Types;

namespace Mobius.ServiceFacade
{

	/// <summary>
	/// Read & write user objects to db (lists, queries, structs, ...)
	/// </summary>

	public class UserObjectDao
	{
		private static UserObject _uo;
		private static byte[] _sUo;
		private static List<UserObject> _uoList;
		private static byte[] _sUoList;

		public static IUserObjectIUD IUserObjectIUD // used to notify others of changes to user objects
		{ get { return InterfaceRefs.IUserObjectIUD; } }

		/// <summary>
		/// Be sure user object table is there and is readable
		/// </summary>

		public static void Ping()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				ServiceFacade.InvokeNativeMethod(nativeClient,
						(int)Services.Native.ServiceCodes.MobiusUserObjectService,
						(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.Ping,
						null);
				((System.ServiceModel.IClientChannel)nativeClient).Close();
			}
			else UAL.UserObjectDao.Ping();
		}

		/// <summary>
		/// Fetch all objects of a given type regardless of who owns it or where it is located.
		/// </summary>
		/// <param name="type"></param>
		/// <returns>Set of user objects ordered by obj_id</returns>

		public static List<UserObject> ReadMultiple(
				UserObjectType type,
				bool includeContent)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserObjectService,
								(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.ReadMultiple1,
								new Services.Native.NativeMethodTransportObject(new object[] { (int)type, includeContent, false }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null || resultObject.Value == null) return null;
				_uoList = UserObject.DeserializeListBinary((byte[])resultObject.Value);
				return _uoList;
			}

			else return UAL.UserObjectDao.ReadMultiple(type, includeContent);
		}

		/// <summary>
		/// Fetch info for the set of objects of a given object type visible to the specified user, optionally includes publicly visible objects as well.
		/// </summary>
		/// <param name="type">Object type</param>
		/// <param name="userId"></param>
		/// <param name="includeShared"></param>
		/// <returns>Set of user objects ordered by folderType, folderId, & (case insensitively) obj name</returns>

		public static List<UserObject> ReadMultiple(
				UserObjectType type,
				string userId,
				bool includeShared,
				bool includeContent)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserObjectService,
								(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.ReadMultiple2,
								new Services.Native.NativeMethodTransportObject(
								new object[] { (int)type, userId, includeShared, includeContent, false }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null || resultObject.Value == null) return null;
				_uoList = UserObject.DeserializeListBinary((byte[])resultObject.Value);
				return _uoList;
			}
			else return UAL.UserObjectDao.ReadMultiple(type, userId, includeShared, includeContent);
		}

		/// <summary>
		/// Fetch user object info on set of objects based what's visible to the given user, optionally including shared objects.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="includeShared"></param>
		/// <returns>Set of user objects ordered by folderType, folderId, and (case insensitively) obj name</returns>

		public static List<UserObject> ReadMultiple(
				String userId,
				bool includeShared,
				bool includeContent)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserObjectService,
								(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.ReadMultiple3,
								new Services.Native.NativeMethodTransportObject(new object[] { userId, includeShared, includeContent }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null || resultObject.Value == null) return null;
				_uoList = UserObject.DeserializeListBinary((byte[])resultObject.Value);
				return _uoList;
			}
			else return UAL.UserObjectDao.ReadMultiple(userId, includeShared, includeContent);
		}

		/// <summary>
		/// Fetch user object info on set of objects under a specified folder (to full de based
		/// on what's visible to the given user, optionally including shared objects.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="includeShared"></param>
		/// <returns>Set of user objects ordered by folderType, parentFolder, and (case insensitively) obj name</returns>

		public static List<UserObject> ReadMultiple(
				string folderName,
				string userId,
				bool includeShared,
				bool includeContent)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserObjectService,
								(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.ReadMultiple4,
								new Services.Native.NativeMethodTransportObject(
								new object[] { folderName, userId, includeShared, includeContent, false }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null || resultObject.Value == null) return null;
				_uoList = UserObject.DeserializeListBinary((byte[])resultObject.Value);
				return _uoList;
			}

			else return UAL.UserObjectDao.ReadMultiple(folderName, userId, includeShared, includeContent);
		}

		/// <summary>
		/// Fetch UserObject info for a list of object ids
		/// </summary>
		/// <param name="includeContent"></param>
		/// <returns>Set of user objects ordered by folderType, parentFolder, and (case insensitively) obj name</returns>

		public static List<UserObject> ReadMultiple(
				List<int> uoIds,
				bool includeContent)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();

				int[] uoIdsArray = uoIds.ToArray(); // convert ids to array for serialization
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
						(int)Services.Native.ServiceCodes.MobiusUserObjectService,
						(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.ReadMultiple5,
						new Services.Native.NativeMethodTransportObject(
						new object[] { uoIdsArray, includeContent }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null || resultObject.Value == null) return null;
				_uoList = UserObject.DeserializeListBinary((byte[])resultObject.Value);
				return _uoList;
			}

			else return UAL.UserObjectDao.ReadMultiple(uoIds, includeContent);
		}

		/// <summary>
		/// Get all user parameters for a user
		/// </summary>
		/// <param name="userid"></param>
		/// <returns></returns>

		public static Dictionary<string, string> GetUserParameters(
			string userid)
		{
			string desc = "", datetime = "";
			int objid = 0;
			string share_with = "";
			int count = 0;

			Dictionary<string, string> upDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			List<UserObject> uoList = ReadMultiple(UserObjectType.UserParameter, userid, false, true);
			foreach (UserObject uo0 in uoList)
			{
				//if (Lex.Contains(uo0.Name, "FavoriteStructures")) uo0.Name = uo0.Name; // debug

				string value = GetProperUserParameterValue(uo0);
				upDict[uo0.Name.ToUpper()] = value;
			}

			return upDict;
		}

		/// <summary>
		/// GetUserParameterValue
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		static string GetProperUserParameterValue(UserObject uo)
		{
			if (uo == null) return "";

			string value = "";
			if (Lex.IsDefined(uo.Description)) // normal value (<4000 chars)
				value = uo.Description;

			else if (Lex.IsDefined(uo.Content)) // long value
				value = uo.Content;

			return value;
		}

		/// <summary>
		/// Get a user profile parameter 
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="parm"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>

		public static string GetUserParameter( // 
				string userId,
				string parm,
				string defaultValue)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserObjectService,
								(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.GetUserParameter,
								new Services.Native.NativeMethodTransportObject(new object[] { userId, parm, defaultValue }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null || resultObject.Value == null) return null;
				string result = (string)resultObject.Value;
				return result;
			}

			else return UAL.UserObjectDao.GetUserParameter(userId, parm, defaultValue);
		}

		/// <summary>
		/// Set a user profile parameter 
		/// </summary>
		/// <param name="userId">User name</param>
		/// <param name="parmName">Parameter</param>
		/// <param name="value">Value to set to</param>

		public static void SetUserParameter( // set a user profile parameter 
			string userId, 
			string parmName, 
			string value)
		{
			bool asynch = true; // if true start call and then return immediately without waiting for completion of update 

			DateTime t0 = DateTime.Now;

			object parmArray = new object[] { userId, parmName, value };

			if (!asynch)
				SetUserParameterThreadMethod(parmArray);

			else
			{
				Thread t = new Thread(new ParameterizedThreadStart(SetUserParameterThreadMethod));
				t.Name = "SetUserParameter " + parmName;
				t.IsBackground = true;
				t.SetApartmentState(ApartmentState.STA);
				t.Start(parmArray);
			}
			//DebugLog.TimeMessage("SetUserParameter " + parmName + " = " + value + ", Time(ms):", t0);
		}

		static void SetUserParameterThreadMethod(object parmArray)
		{
			try
			{
				DateTime t0 = DateTime.Now;

				object[] parms = parmArray as object[];
				string userId = parms[0] as string;
				string parmName = parms[1] as string;
				string value = parms[2] as string;

				if (ServiceFacade.UseRemoteServices)
				{
					Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
					Services.Native.NativeMethodTransportObject resultObject =
							ServiceFacade.InvokeNativeMethod(nativeClient,
									(int)Services.Native.ServiceCodes.MobiusUserObjectService,
									(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.SetUserParameter,
									new Services.Native.NativeMethodTransportObject(new object[] { userId, parmName, value }));
					((System.ServiceModel.IClientChannel)nativeClient).Close();
				}
				else
					UAL.UserObjectDao.SetUserParameter(userId, parmName, value);

				//DebugLog.TimeMessage("SetUserParameterThreadMethod " + parmName + " = " + value + ", Time(ms):", t0);
			}

			catch (Exception ex) // log any exception since any thrown exception for thread can't be caught
			{
				DebugLog.Message(ex);
			}
		}

		/// <summary>
		/// Return true if user object exists
		/// </summary>

		public static bool Exists(
						UserObject uo)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				_sUo = uo.SerializeBinary();
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserObjectService,
								(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.Exists,
								new Services.Native.NativeMethodTransportObject(new object[] { _sUo }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				bool result = (bool)resultObject.Value;
				return result;
			}

			else return UAL.UserObjectDao.Exists(uo);
		}

		/// <summary>
		/// Get header info for object
		/// </summary>
		/// <param name="objectId"></param>
		/// <returns></returns>

		public static UserObject ReadHeader(
				int objectId) // id of item to read
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserObjectService,
								(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.ReadHeader1,
								new Services.Native.NativeMethodTransportObject(new object[] { objectId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null || resultObject.Value == null) return null;
				_uo = UserObject.DeserializeBinary((byte[])resultObject.Value);
				return _uo;
			}

			else return UAL.UserObjectDao.ReadHeader(objectId);
		}

		/// <summary>
		/// Fetch user object header by the type, owner, folder id, and name of the specified user object
		/// </summary>
		/// <param name="uo">User object</param>
		/// <returns>The requested user object or null if no matching user object is found.</returns>

		public static UserObject ReadHeader(
				UserObject uo)
		{
			if (uo.Type != UserObjectType.Unknown && !String.IsNullOrEmpty(uo.Owner) &&
			 !String.IsNullOrEmpty(uo.Name)) // use path if defined (folder may be undefined)
				return ReadHeader(uo.Type, uo.Owner, uo.ParentFolder, uo.Name);

			if (uo.Id > 0) return ReadHeader(uo.Id);

			throw new Exception("Neither the UserObject path or Id is defined");
		}

		/// <summary>
		/// Fetch header for user object by the specified type, owner, folder id, and name
		/// </summary>
		/// <param name="type">type of object</param>
		/// <param name="owner">userid of object owner</param>
		/// <param name="folderId">folder that it's in</param>
		/// <param name="name">name of object</param>
		/// <returns>The requested user object or null if no matching user object is found.</returns>

		public static UserObject ReadHeader(
				UserObjectType type,
				string owner,
				string folderId,
				string name)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserObjectService,
								(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.ReadHeader2,
								new Services.Native.NativeMethodTransportObject(
								new object[] { (int)type, owner, folderId, name }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null || resultObject.Value == null) return null;
				_uo = UserObject.DeserializeBinary((byte[])resultObject.Value);
				return _uo;
			}

			else return UAL.UserObjectDao.ReadHeader(type, owner, folderId, name);
		}

		/// <summary>
		/// Update header for user object
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static void UpdateHeader( // update header info for object
				UserObject uo)
		{
			UpdateHeader(uo, true, true);
		}

		/// <summary>
		/// Update header for user object
		/// </summary>
		/// <param name="uo"></param>
		/// <param name="setUpdateDateToCurrentDate"></param>
		/// <param name="updateUi"></param>

		public static void UpdateHeader( // update header info for object
				UserObject uo,
				bool setUpdateDateToCurrentDate,
				bool updateUi)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				_sUo = uo.SerializeBinary();
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
						(int)Services.Native.ServiceCodes.MobiusUserObjectService,
						(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.UpdateHeader,
						new Services.Native.NativeMethodTransportObject(new object[] { _sUo, setUpdateDateToCurrentDate }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				bool updateSucceeded = (bool)resultObject.Value;

				if (!updateSucceeded)
				{
					string msg = "UpdateHeader failed for user object " + uo.Id;
					string msg2 = msg + "\r\n\r\n" + new StackTrace(true);
					Email.SendCriticalEventNotificationToMobiusAdmin(msg, msg2);

					throw new Exception("UpdateHeader failed.");
				}

				if (updateUi && IUserObjectIUD != null && uo.ParentFolderType != 0) // notify other objects of change 
					IUserObjectIUD.UserObjectUpdated(uo);
			}

			else
			{
				UAL.UserObjectDao.UpdateHeader(uo, setUpdateDateToCurrentDate, updateUi);
				if (updateUi && IUserObjectIUD != null && uo.ParentFolderType != 0) // notify other objects of change 
					IUserObjectIUD.UserObjectUpdated(uo);
			}
		}

		/// <summary>
		/// Update the update date & count columns
		/// </summary>
		/// <param name="objectId"></param>
		/// <param name="updateDateTime"></param>
		/// <param name="count"></param>

		public static void UpdateUpdateDateAndCount(
				int objectId,
				DateTime updateDateTime,
				int count)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserObjectService,
								(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.UpdateUpdateDateAndCount,
								new Services.Native.NativeMethodTransportObject(new object[] { objectId, updateDateTime, count }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				bool updateSucceeded = (bool)resultObject.Value;

				if (!updateSucceeded)
				{ throw new Exception("UpdateUpdateDateAndCount failed."); }
			}

			else UAL.UserObjectDao.UpdateUpdateDateAndCount(objectId, updateDateTime, count);
		}

		//public static bool SetMobileDefaultQueryForOwner(UserObject uo, string ownerId)
		//{
		//    if (ServiceFacade.UseRemoteServices)
		//       {
		//           sUo = uo.SerializeBinary();
		//           Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
		//           Services.Native.NativeMethodTransportObject resultObject =
		//               ServiceFacade.InvokeNativeMethod(nativeClient,
		//                   (int)Services.Native.ServiceCodes.MobiusUserObjectService,
		//                   (int)Services.Native.ServiceOpCodes.MobiusUserObjectService.SetMobileDefaultQueryForOwner,
		//                   new Services.Native.NativeMethodTransportObject(new object[] { sUo }));
		//           ((System.ServiceModel.IClientChannel)nativeClient).Close();
		//           bool updateSucceeded = (bool)resultObject.Value;

		//           return updateSucceeded;
		//       }

		//       return UAL.UserObjectDao.SetMobileDefaultQueryForOwner(uo, ownerId);
		//}

		/// <summary>
		/// Get next available object id
		/// </summary>
		/// <returns></returns>

		public static int GetNextId()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserObjectService,
								(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.GetNextId,
								null);
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				int nextId = (int)resultObject.Value;
				return nextId;
			}
			else return UAL.UserObjectDao.GetNextId();
		}


		// Removed the 2 following methods and made the id and updateUi paramters optional on the actual method.

		//public static void Write(UserObject uo)
		//{
		//	Write(uo, -1);
		//	return;
		//}

		//public static void Write( // write user object
		//	UserObject uo,
		//	int id)
		//{
		//	Write(uo, id, true);
		//}

		/// <summary>
		/// Write user object to database
		/// </summary>
		/// <param name="userObject"></param>
		/// <param name="id"></param>
		/// <param name="updateUi"></param>
		public static void Write( // write user object
				UserObject userObject,
				int id = -1,
				bool updateUi = true)
		{
			bool updated = false; // used for proper updating of contents tree
			UserObject existingUo = null;

			bool tempObject = Lex.Eq(userObject.ParentFolder, "TEMP_FOLDER");

			if (!tempObject) // if not a temp object then see if UI needs updating
			{
				if (userObject.Id > 0) // if id defined see if object with same id exists
				{
					existingUo = ReadHeader(userObject.Id);
					if (existingUo != null) updated = true;
				}

				if (existingUo == null) // see if object with same name exists
				{
					existingUo = ReadHeader(userObject.Type, userObject.Owner, userObject.ParentFolder, userObject.Name);
					if (existingUo != null)
						Delete(existingUo.Id, true); // delete any existing object with id & update UI
				}
			}

			if (ServiceFacade.UseRemoteServices)
			{
				_sUo = userObject.SerializeBinary();
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserObjectService,
								(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.Write,
								new Services.Native.NativeMethodTransportObject(
								new object[] { _sUo, id }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();

				if (resultObject != null && resultObject.Value != null)
				{ // Id/UpdateDT may have been changed during the write operation so update the UO in place
					UserObject uo2 = UserObject.DeserializeBinary((byte[])resultObject.Value);
					userObject.Id = uo2.Id;
					userObject.UpdateDateTime = uo2.UpdateDateTime;
				}

				else // failed
				{
					string msg = "Failed to write user object:\r\n";
					try { msg += userObject.Serialize(); }
					catch (Exception ex) { msg += ex.Message; }
					throw new Exception(msg);
				}

			}

			else
			{
				UAL.UserObjectDao.Write(userObject, id);
			}

			if (updateUi && IUserObjectIUD != null) // notify UI of change 
			{
				//if (uo.ParentFolderType != FolderTypeEnum.None) // don't update the content tree for items not in a folder
				//{
				if (updated)
					IUserObjectIUD.UserObjectUpdated(userObject);
				else IUserObjectIUD.UserObjectInserted(userObject);
				//}
			}

		}

		/// <summary>
		/// Read object matching the type, folder, owner, name of the given user object
		/// </summary>
		/// <param name="uo"></param>
		/// <returns>The requested user object or null if no matching user object is found.</returns>

		public static UserObject Read(UserObject uo)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				_sUo = uo.SerializeBinary();
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserObjectService,
								(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.Read2,
								new Services.Native.NativeMethodTransportObject(new object[] { _sUo }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null || resultObject.Value == null) return null;
				uo = UserObject.DeserializeBinary((byte[])resultObject.Value);
				return uo;
			}

			else return UAL.UserObjectDao.Read(uo);
		}

		/// <summary>
		/// Read user object given the object id
		/// </summary>
		/// <param name="objectId">id of item to read</param>
		/// <returns>The requested user object or null if no matching user object is found.</returns>

		public static UserObject Read(int objectId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
								ServiceFacade.InvokeNativeMethod(nativeClient,
												(int)Services.Native.ServiceCodes.MobiusUserObjectService,
												(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.Read1,
												new Services.Native.NativeMethodTransportObject(new object[] { objectId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null || resultObject.Value == null) return null;
				_uo = UserObject.DeserializeBinary((byte[])resultObject.Value);
				return _uo;
			}

			else return UAL.UserObjectDao.Read(objectId);
		}

		/// <summary>
		/// Read object matching the type, folder, owner, name
		/// </summary>
		/// <param name="type">type of object</param>
		/// <param name="owner">userid of object owner</param>
		/// <param name="folderId">folder that it's in</param>
		/// <param name="name">name of object</param>
		/// <returns></returns>

		public static UserObject Read(
				UserObjectType type,
				string owner,
				string folderId,
				string name)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
								ServiceFacade.InvokeNativeMethod(nativeClient,
												(int)Services.Native.ServiceCodes.MobiusUserObjectService,
												(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.Read3,
												new Services.Native.NativeMethodTransportObject(new object[] { (int)type, owner, folderId, name }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null || resultObject.Value == null) return null;
				_uo = UserObject.DeserializeBinary((byte[])resultObject.Value);
				return _uo;
			}

			else return UAL.UserObjectDao.Read(type, owner, folderId, name);
		}

		/// <summary>
		/// This method assures that user objects (i.e. annotations, calc fields, lists) associated with a shared query are shared at the same level
		/// </summary>
		/// <param name="queryId"></param>
		/// <returns></returns>

		public static void AssureAccessToSharedQueryUserObjects(int queryId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserObjectService,
								(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.AssureAccessToSharedQueryUserObjects,
								new Services.Native.NativeMethodTransportObject(new object[] { queryId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
			}

			else Mobius.UAL.UserObjectDao.AssureAccessToSharedQueryUserObjects(queryId);
		}

		/// <summary>
		/// Delete user object
		/// </summary>
		/// <param name="objectId"></param>
		/// <returns></returns>

		public static bool Delete(int objectId)
		{
			return Delete(objectId, true);
		}

		/// <summary>
		/// Delete a user object by id.
		/// Optionally calls IUDUserObject.UserObjectDeleted to trigger content tree updates
		/// </summary>
		/// <param name="objectId">id of user object</param>
		/// <param name="notifyUi">if true notify any associated UI of change</param>
		/// <returns>True if successful, false otherwise.</returns>

		public static bool Delete(
				int objectId,
				bool notifyUi)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				bool deleteSucceeded = false;
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusUserObjectService,
								(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.ReadHeader1,
								new Services.Native.NativeMethodTransportObject(new object[] { objectId }));
				_sUo = (byte[])resultObject.Value;
				if (_sUo != null)
				{
					_uo = UserObject.DeserializeBinary(_sUo);
					if (_uo != null)
					{
						resultObject =
								ServiceFacade.InvokeNativeMethod(nativeClient,
										(int)Services.Native.ServiceCodes.MobiusUserObjectService,
										(int)Services.Native.ServiceOpCodes.MobiusUserObjectService.Delete,
										new Services.Native.NativeMethodTransportObject(new object[] { objectId }));
						deleteSucceeded = (bool)resultObject.Value;
					}

					if (deleteSucceeded && notifyUi && IUserObjectIUD != null && _uo.ParentFolderType != 0) // notify other object of change 
						IUserObjectIUD.UserObjectDeleted(_uo);
				}
					((System.ServiceModel.IClientChannel)nativeClient).Close();
				return deleteSucceeded;

			}

			else // non-service mode
			{
				UserObject uo = ReadHeader(objectId);
				if (uo == null) return false;

				bool result = UAL.UserObjectDao.Delete(objectId);
				if (notifyUi && IUserObjectIUD != null && uo.ParentFolderType != 0) // notify other object of change 
					IUserObjectIUD.UserObjectDeleted(uo);

				return result;
			}
		}

		/// <summary>
		/// Check if user has read access
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="objectId"></param>
		/// <returns></returns>

		public static bool UserHasReadAccess(
				string userName,
				int objectId)
		{
			return false; // todo
		}

		/// <summary>
		/// Check if user has write access
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="objectId"></param>
		/// <returns></returns>

		public static bool UserHasWriteAccess(
				string userName,
				int objectId)
		{
			return false; // todo
		}

	}

	/// <summary>
	/// An instance of this class is created to bridge between an IUserObjectDao interface reference and the static methods in UserObjectDao
	/// </summary>

	public class IUserObjectDaoMethods : IUserObjectDao
	{
		/// <summary>
		/// Read header for specified objectId
		/// </summary>
		/// <param name="objectId"></param>
		/// <returns></returns>
		public UserObject ReadHeader(
				int objectId)
		{
			return UserObjectDao.ReadHeader(objectId);
		}

		/// <summary>
		/// Read header for specified objectId
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>

		public UserObject Read(
				int objectId)
		{
			return UserObjectDao.Read(objectId);
		}

		/// <summary>
		/// Fetch all objects of a given type regardless of who owns it or where it is located.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="includeContent"></param>
		/// <returns>Set of user objects ordered by obj_id</returns>

		public List<UserObject> ReadMultiple(
				UserObjectType type,
				bool includeContent)
		{
			return UserObjectDao.ReadMultiple(type, includeContent);
		}

	}
}

