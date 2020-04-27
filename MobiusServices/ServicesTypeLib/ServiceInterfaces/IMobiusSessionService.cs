using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Mobius.Services.Types.ServiceInterfaces
{
    [ServiceContract(Namespace = "http://server/MobiusServices/v1.0")]
    public interface IMobiusSessionService
    {
        [OperationContract]
        string GetCurrentVersionNumber();

        [OperationContract]
        string GetCurrentUserId();

        [OperationContract]
        string GetCurrentSessionUserId(Session session);

				[OperationContract]
				Session CreateSessionAsOtherUser(string userName, string domainName);

				[OperationContract]
        Session CreateSession(bool webServiceUser); // public Types.Session CreateSession(bool externalUser = false)

        [OperationContract]
        Session CreateNativeSession();

				[OperationContract]
				string GetUserParametersString(Session session);

				[OperationContract]
        Session FreshenSession(Session session);

        [OperationContract]
        bool IsSessionValid(Session session);

        [OperationContract]
        bool IsSessionAlive(Session session);

        [OperationContract]
        List<SessionInfo> GetSessionInfo(Session session);

        [OperationContract]
        int GetCurrentSessionCount(Session session);

        [OperationContract]
        Dictionary<Session, List<SessionInfo>> GetSessionInfoForAllSessions(Session session);

        [OperationContract]
        bool SetSessionParameter(Session session, SessionParameterName parmName, string value);

        [OperationContract]
        void DisposeSession(Session session);

        [OperationContract]
        void DisposeSessionById(Session session, int sessionIdToDispose);

    }
}
