using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Mobius.Services.Types.ServiceInterfaces
{
    [ServiceContract(Namespace = "http://server/MobiusServices/v1.0")]
    public interface IMobiusSecurityService
    {
        #region Preferred Operations

        [OperationContract]
        string GetCurrentVersionNumber();

        [OperationContract]
        string GetCurrentUserId();

        [OperationContract]
        string GetCurrentSessionUserId(Session session);

        [OperationContract]
        bool CanLogon(Session session);

        [OperationContract]
        bool IsAdministrator(Session session);

        #endregion Preferred Operations
        
        
        #region Legacy Operations

        [OperationContract]
        bool Authenticate(Session session, string userName, string password);

        [OperationContract]
        bool CreateUser(Session session, UserInfo userInfo);

        [OperationContract]
        string GetUserEmailAddress(Session session, string userName);

        [OperationContract]
        UserInfo GetUserInfo(Session session, string userName);

        [OperationContract]
        bool GrantPrivilege(Session session, string userName, string privilege);

        [OperationContract]
        bool HasPrivilege(Session session, string userName, string privilege);

        [OperationContract]
        bool IsValidPrivilege(Session session, string privilege);

        [OperationContract]
        UserInfo ReadUserInfo(Session session, string userName);

        [OperationContract]
        bool RevokePrivilege(Session session, string userName, string privilege);

        #endregion  Legacy Operations

    }
}
