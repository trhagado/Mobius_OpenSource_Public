using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Mobius.Services.Types.ServiceInterfaces
{
    [ServiceContract(Namespace = "http://server/MobiusServices/v1.0")]
    public interface IMobiusMetaDataService
    {
			[OperationContract]
			string GetMetaTreeXml(
				Session session,
				string rootNodeName,
				bool includeCurrentUsersObjects,
				bool includeAllUsersObjects);

			[OperationContract]
			byte [] GetMetaTreeXmlCompressed(
				Session session,
				string rootNodeName,
				bool includeCurrentUsersObjects,
				bool includeAllUsersObjects);
		}
}
