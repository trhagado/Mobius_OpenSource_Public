using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Mobius.Services.Types
{

/// <summary>
/// Session class - Exchanged with service clients
/// </summary>

	[DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public class Session
    {
        [DataMember] public int Id;
        [DataMember] public string UserId;
        [DataMember] public Dictionary<SessionParameterName, string> SessionParameters;
        [DataMember] public DateTime CreationDT;
        [DataMember] public DateTime ExpirationDT;

				/// <summary>
				/// Access the native process id from SessionParameters[NativeSessionLine]
				/// </summary>

				public int ProcessId
				{
					get
					{
						int processId = -1;
						Dictionary<SessionParameterName, string> sps = SessionParameters;
						SessionParameterName nsl = SessionParameterName.NativeSessionLine;
						if (sps.ContainsKey(nsl)) int.TryParse(sps[nsl], out processId);
						return processId;
					}

					set
					{
						Dictionary<SessionParameterName, string> sps = SessionParameters;
						SessionParameterName nsl = SessionParameterName.NativeSessionLine;
						sps[nsl] = value.ToString();
					}
				}

    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public class SessionInfo
    {
        [DataMember] public string ServiceName;
        [DataMember] public string ServiceEndpointUri;
        [DataMember] public List<SessionInfoDataElement> SessionData;
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public class SessionInfoDataElement
    {
        [DataMember] public string Name;
        [DataMember] public SessionInfoDataElementType ValueType;
        [DataMember] public string Value;
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public enum SessionInfoDataElementType
    {
        [EnumMember] String,
        [EnumMember] Integer,
        [EnumMember] Double,
        [EnumMember] Date,
        [EnumMember] DateTime,
        [EnumMember] TimeSpan,
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public enum SessionParameterName
    {
        [EnumMember] UserNameToImpersonate,
        [EnumMember] IsNativeSession,
        [EnumMember] NativeSessionLine
    }

}
