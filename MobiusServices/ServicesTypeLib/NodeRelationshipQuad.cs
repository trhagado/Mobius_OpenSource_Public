using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public class NodeRelationshipQuad
    {
        [DataMember] public DateTime LastUpdateTimestamp = DateTime.MinValue; // used to enable retrieval of deltas
        [DataMember] public GraphElementStatus Status = GraphElementStatus.present; //for support of deltas

        [DataMember] public string SubjectNodeName;
        [DataMember] public NodeRelationshipType RelationshipType;
        [DataMember] public string ObjectNodeName;

        public override string ToString()
        {
            return SubjectNodeName + " --" + RelationshipType.ToString() + "--> " + ObjectNodeName + " (" + LastUpdateTimestamp.ToShortTimeString() + ")";
        }
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum NodeRelationshipType
    {
        [EnumMember] hasChild,
        [EnumMember] hasParent
    }
}
