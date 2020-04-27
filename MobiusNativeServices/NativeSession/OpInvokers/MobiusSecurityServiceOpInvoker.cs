using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mobius.Services.Native.ServiceOpCodes;


namespace Mobius.Services.Native.OpInvokers
{
    public class MobiusSecurityServiceOpInvoker : IInvokeServiceOps
    {
        #region IInvokeServiceOps Members

        object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
        {
            MobiusSecurityService op = (MobiusSecurityService)opCode;
            switch (op)
            {
                case MobiusSecurityService.CanLogon:
                    {
                        string userId = (string)args[0];
                        bool canLogon = UAL.Security.IsAuthorized(userId);
                        return canLogon;
                    }
                case MobiusSecurityService.IsAdministrator:
                    {
                        string userId = (string)args[0];
                        bool isAdmin = UAL.Security.IsAdministrator(userId);
                        return isAdmin;
                    }
                case MobiusSecurityService.Authenticate:
                    {
                        string userId = (string)args[0];
                        string password = (string)args[1];
                        bool succeeded = UAL.Security.Authenticate(userId, password);
                        return succeeded;
                    }
                case MobiusSecurityService.CreateUser:
                    {
                        string serializedUserInfo = (string)args[0];
                        ComOps.UserInfo userInfo = ComOps.UserInfo.Deserialize(serializedUserInfo);
                        UAL.Security.CreateUser(userInfo);
                        return true;
                    }

								case MobiusSecurityService.DeleteUser:
										{
											string userName = (string)args[0];
											bool succeeded = UAL.Security.DeleteUser(userName);
											return succeeded;
										}

                case MobiusSecurityService.GetUserEmailAddress:
                    {
                        string userId = (string)args[0];
                        string emailAddress = UAL.Security.GetUserEmailAddress(userId);
                        return emailAddress;
                    }
                case MobiusSecurityService.GetUserInfo:
                    {
                        string userId = (string)args[0];
                        ComOps.UserInfo userInfo = UAL.Security.GetUserInfo(userId);
                        string serializedUserInfo = (userInfo == null) ? null : userInfo.Serialize();
                        return serializedUserInfo;
                    }
                case MobiusSecurityService.GrantPrivilege:
                    {
                        string userId = (string)args[0];
                        string privilegeName = (string)args[1];
                        UAL.Security.GrantPrivilege(userId, privilegeName);
                        return true;
                    }
                case MobiusSecurityService.HasPrivilege:
                    {
                        string userId = (string)args[0];
                        string privilegeName = (string)args[1];
                        bool hasPrivilege = UAL.Security.HasPrivilege(userId, privilegeName);
                        return hasPrivilege;
                    }
								case MobiusSecurityService.IsValidPrivilege: // (Deprecated, can now call directly in ComOps)
										{
											string privilegeName = (string)args[0];
											bool isValidPrivilege = ComOps.PrivilegesMx.IsValidPrivilegeName(privilegeName);
											return isValidPrivilege;
										}
                case MobiusSecurityService.Logon:
                    {
                        string userId = (string)args[0];
												string domainName = (string)args[1];
												string client = (string)args[2];
												bool canLogon = UAL.Security.Logon(userId, domainName, client);

												if (canLogon) // if logged on init client components with user info, etc
													NativeSessionInitializer.InitializeClientComponents();

                        return canLogon;
                    }
                case MobiusSecurityService.ReadUserInfo:
                    {
                        string userId = (string)args[0];
                        ComOps.UserInfo userInfo = UAL.Security.ReadUserInfo(userId);
                        string serializedUserInfo = (userInfo == null) ? null : userInfo.Serialize();
                        return serializedUserInfo;
                    }
                case MobiusSecurityService.RevokePrivilege:
                    {
                        string userId = (string)args[0];
                        string privilegeName = (string)args[1];
                        UAL.Security.RevokePrivilege(userId, privilegeName);
                        return true;
                    }
                case MobiusSecurityService.SetUser:
                    {
                        string userId = (string)args[0];
                        UAL.Security.SetUser(userId);
                        return true;
                    }

							case MobiusSecurityService.LoadMetadata:
										{
											NativeSessionInitializer.Instance.LoadMetaData(); 
											return true;
										}

            }
            return null;
        }

        #endregion
    }
}
