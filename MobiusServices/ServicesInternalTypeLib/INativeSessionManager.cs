using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ServiceModel;
using Mobius.Services.Types;


namespace Mobius.Services.Types.Internal
{

/// <summary>
/// Service normally hosted by the MobiusServiceHost and called by MobiusNativeServices processes
/// </summary>

	[ServiceContract(Namespace = "Mobius.Services.Internal.Native")]
    public interface INativeSessionManager
    {
        [OperationContract]
        void CreateNativeSession(Internal.InternalSession session);

				// Called by native session hosts to tell service host that native session has started
				[OperationContract]
				Types.Internal.InternalSession RegisterNativeSessionHost(int sessionId);

				// Called by native session hosts to tell service host that native session is active
				[OperationContract]
				Types.Internal.InternalSession FreshenNativeSession(Types.Internal.InternalSession session);

				[OperationContract]
        Types.SessionInfo GetSessionInfo(Types.Internal.InternalSession session);

        [OperationContract]
        Dictionary<InternalSession, SessionInfo> GetSessionInfoForAllSessions();

        [OperationContract]
        void DisposeNativeSessions(List<Types.Internal.InternalSession> staleSessions);

        [OperationContract]
        bool ApplyUserObjectUpdate(Types.Internal.UserObjectUpdate uoUpdate);

        [OperationContract]
        bool DisposeNativeSession(Types.Internal.InternalSession session);

				[OperationContract]
				List<int> ReacquireNativeSessions(Dictionary<int, Internal.InternalSession> sessionById);
    }
}
