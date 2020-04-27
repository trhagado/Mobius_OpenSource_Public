using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public class UserInfo //from ComOps/UserInfo.cs
    {
        [DataMember] public string UserDomainName = ""; // lan domain for client machine
        [DataMember] public string UserName = ""; // Windows NT userid of client
        [DataMember] public string LastName = "";
        [DataMember] public string FirstName = "";
        [DataMember] public string MiddleInitial = "";
        [DataMember] public string EmailAddress = "";
        [DataMember] public string Company = "";
        [DataMember] public string Site = "";
        [DataMember] public string Department = "";
    }
}
