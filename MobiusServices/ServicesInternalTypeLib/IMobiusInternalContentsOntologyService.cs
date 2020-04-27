using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;
using System.ServiceModel;


namespace Mobius.Services.Types.Internal
{
    [ServiceContract(Namespace = "Mobius.Services.Types.Internal")]
    public interface IMobiusInternalContentsOntologyService
    {
        [OperationContract]
        List<string> GetAllNodeNames(string userId);
    }
}
