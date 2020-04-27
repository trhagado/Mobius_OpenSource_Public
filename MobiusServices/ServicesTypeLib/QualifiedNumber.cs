using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public class QualifiedNumber : FormattedDataType
    {
        [DataMember] public const int NullNumber = -4194303;
        [DataMember] public double NumberValue;
        [DataMember] public double NumberValueHigh;
        [DataMember] public int NValue;
        [DataMember] public int NValueTested;
        [DataMember] public string Qualifier;
        [DataMember] public double StandardDeviation;
        [DataMember] public double StandardError;
        [DataMember] public string TextValue;
        [DataMember] public string Units;
    }
}
