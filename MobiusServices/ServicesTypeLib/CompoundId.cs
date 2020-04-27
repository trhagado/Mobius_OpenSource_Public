using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public class CompoundId : FormattedDataType
    {
        [DataMember] public string Value;
    }
}
