﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public class Graph
    {
        [DataMember] public List<Node> Nodes;
        [DataMember] public List<NodeRelationshipQuad> Relationships;
    }
}