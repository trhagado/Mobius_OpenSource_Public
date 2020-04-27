using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Mobius.Services.Types;

namespace Mobius.Services.Types.ServiceInterfaces
{
    [ServiceContract(Namespace = "http://server/MobiusServices/v1.0")]
    public interface IMobiusUsageService
    {
        [OperationContract]
        string GetCurrentVersionNumber();

        [OperationContract]
        string GetCurrentUserId();

        [OperationContract]
        string GetCurrentSessionUserId(Session session);

        [OperationContract]
        bool LogEvent(
            Session session,
            string eventName,
            string eventData,
            int eventNumber);

        [OperationContract]
        string AnalyzeUsageData(
            Session session,
            string commandArgs);
    }
}
