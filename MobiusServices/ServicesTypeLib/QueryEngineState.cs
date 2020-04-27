using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum QueryEngineState
    {
        [EnumMember] New = 0,           // no query executed yet
        [EnumMember] Searching = 1,     // executing search step
        [EnumMember] Retrieving = 2,    // retrieving data rows
        [EnumMember] Closed = 3         // all rows retrieved or explicitly closed
    }
}
