using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Mobius.Services.Types.Internal
{

/// <summary>
/// INativeSession Interface
/// </summary>

    [ServiceContract(Namespace = "Mobius.Services.Internal.Native")]
    public interface INativeSession
    {
        [OperationContract]
        bool SetSessionManagerProcessId(int sessionManagerProcessId);

        [OperationContract]
        InternalSession GetInternalSession();

        [OperationContract]
        string GetCurrentUserId();

    }
}
