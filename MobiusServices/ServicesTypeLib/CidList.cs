using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public class CidList
    {
        public UserObjectNode UserObjectNode = new UserObjectNode(UserObjectType.CnList); // name, etc of list
        public List<CidListElement> List = new List<CidListElement>(); // List of CnListElements
        public Dictionary<string, CidListElement> Dict = new Dictionary<string, CidListElement>(); // dictionary of ids in list
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public class CidListElement // implements Comparable 
    {
        public string Cid; // compound number
        public double Tag; // numeric tag info
        public string StringTag; // text tag info
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum ListLogicType
    {
        [EnumMember] Unknown    = 0,
        [EnumMember] Intersect  = 1,
        [EnumMember] Union      = 2,
        [EnumMember] Difference = 3
    }
}
