using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    //[KnownType(typeof(System.Int32))]
    //[KnownType(typeof(System.Double))]
    //[KnownType(typeof(System.String))]
    //[KnownType(typeof(System.DateTime))]
    [KnownType(typeof(Mobius.Services.Types.QualifiedNumber))]
    [KnownType(typeof(Mobius.Services.Types.ChemicalStructure))]
    [KnownType(typeof(Mobius.Services.Types.CompoundId))]
    [KnownType(typeof(Mobius.Services.Types.DateTimeEx))]
    [KnownType(typeof(Mobius.Services.Types.StringEx))]
    [KnownType(typeof(Mobius.Services.Types.ImageEx))]
    [KnownType(typeof(Mobius.Services.Types.NumberEx))]
    [Serializable]
    public class DataRow
    {
        [DataMember] public object[] Data;
    }
}
