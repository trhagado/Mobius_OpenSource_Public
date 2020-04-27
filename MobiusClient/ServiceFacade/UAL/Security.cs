using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using Mobius.Services.Native;
using Mobius.Services.Native.ServiceOpCodes;
using Mobius.NativeSessionClient;
using NCNS = Mobius.NativeSessionClient;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mobius.ServiceFacade
{
	public class Security
	{
		public static string UserName; // current username

		/// <summary>
		/// Authenticate user
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="password"></param>
		/// <returns></returns>

		public static bool Authenticate(
			string userName,
			string password)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusSecurityService,
					MobiusSecurityService.Authenticate,
					new object[] { userName, password });
				bool result = (bool)resultObject.Value;
				return result;
			}
			else if (!UAL.Security.Authenticate(userName, password))
				return false;

			UserName = userName;
			return true;
		}

		/// <summary>
		/// See if user authorized & logon as current user if so
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="domainName"></param>
		/// <param name="clientName"></param>
		/// <returns></returns>

		public static bool Logon(
			string userName,
			string domainName,
			string clientName)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusSecurityService,
					MobiusSecurityService.Logon,
					new object[] { userName, domainName, clientName });
				bool succeeded = (bool)resultObject.Value;
				if (succeeded)
				{
					UserName = userName;
				}
				return succeeded;
			}

			else if (!UAL.Security.Logon(userName, domainName, clientName))
				return false;

			UserName = userName;
			return true;
		}

		/// <summary>
		/// Set the username, use must be already authenticated elsewhere
		/// (When using the services, authentication was required to obtain a session)
		/// </summary>

		public static void SetUser(
			string userName)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusSecurityService,
					MobiusSecurityService.SetUser,
					new object[] { userName });
			}
			else UAL.Security.SetUser(userName);

			UserName = userName;
			return;
		}

		/// <summary>
		/// See the user is the Mobius account
		/// </summary>

		public static bool IsMobius
		{ get { return Lex.Eq(UserName, "Mobius"); } }

		/// <summary>
		/// See if a user is an administrator
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static bool IsAdministrator(
			string userName)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusSecurityService,
					MobiusSecurityService.IsAdministrator,
					new object[] { userName });
				bool isAdmin = (bool)resultObject.Value;
				return isAdmin;
			}
			else return UAL.Security.IsAdministrator(userName);
		}

		/// <summary>
		/// Grant a privilege to a user
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="privilege"></param>
		/// <returns></returns>

		public static void GrantPrivilege(
			string userName,
			string privilege)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusSecurityService,
					MobiusSecurityService.GrantPrivilege,
					new object[] { userName, privilege });
			}
			else UAL.Security.GrantPrivilege(userName, privilege);
		}

		/// <summary>
		/// Revoke a privilege from a user
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="privilege"></param>
		/// <returns></returns>
		public static void RevokePrivilege(
			string userName,
			string privilege)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusSecurityService,
					MobiusSecurityService.RevokePrivilege,
					new object[] { userName, privilege });
			}
			else UAL.Security.RevokePrivilege(userName, privilege);
		}

		/// <summary>
		/// See if a user has a given privilege
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="privilege"></param>
		/// <returns></returns>

		public static bool HasPrivilege(
			string userName,
			string privilege)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusSecurityService,
					MobiusSecurityService.HasPrivilege,
					new object[] { userName, privilege });
				bool hasPriv = (bool)resultObject.Value;
				return hasPriv;
			}
			else return UAL.Security.HasPrivilege(userName, privilege);
		}

		/// <summary>
		/// Get user information for a user 
		/// Note: Will not get email address (avoids slowdown)
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>

		public static UserInfo GetUserInfo(
			string userName)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusSecurityService,
					MobiusSecurityService.GetUserInfo,
					new object[] { userName });

				if (resultObject == null) return null;
				string serializedUserInfo = (string)resultObject.Value;
				UserInfo userInfo = UserInfo.Deserialize(serializedUserInfo);
				return userInfo;
			}
			else return UAL.Security.GetUserInfo(userName);
		}

		/// <summary>
		/// Get user info for a single user from user object table
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static UserInfo ReadUserInfo(
			string userName)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
						ServiceCodes.MobiusSecurityService,
						MobiusSecurityService.ReadUserInfo,
						new object[] { userName });
				if (resultObject == null) return null;
				UserInfo userInfo = UserInfo.Deserialize((string)resultObject.Value);
				return userInfo;
			}

			else return UAL.Security.ReadUserInfo(userName);
		}

		/// <summary>
		/// Get email address for a user
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static string GetUserEmailAddress(
			string userName)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusSecurityService,
					MobiusSecurityService.GetUserEmailAddress,
					new object[] { userName });
				if (resultObject == null) return null;
				string emailAddress = (string)resultObject.Value;
				return emailAddress;
			}
			else return UAL.Security.GetUserEmailAddress(userName);
		}

		/// <summary>
		/// Create a new user entry
		/// </summary>
		/// <param name="userInfo"></param>
		/// <returns></returns>

		public static void CreateUser(
			UserInfo userInfo)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusSecurityService,
					MobiusSecurityService.CreateUser,
					new object[] { userInfo.Serialize() });
			}
			else UAL.Security.CreateUser(userInfo);
		}

		/// <summary>
		/// Create a new user entry
		/// </summary>
		/// <param name="userInfo"></param>
		/// <returns></returns>

		public static bool DeleteUser(
			string userName)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusSecurityService,
					MobiusSecurityService.DeleteUser,
					new object[] { userName });
				bool result = (bool)resultObject.Value;
				return result;
			}

			else return UAL.Security.DeleteUser(userName);
		}

		/// <summary>
		/// See if a user is allowed to log on
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static bool CanLogon( // return true if logon is authorized for user
			string userName)
		{
			return HasPrivilege(userName, "Logon"); // todo: change for Active Directory
		}

	}
}
