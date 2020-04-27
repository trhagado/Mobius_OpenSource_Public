using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [KnownType(typeof(Mobius.Services.Types.QualifiedNumber))]
    [KnownType(typeof(Mobius.Services.Types.ChemicalStructure))]
    [KnownType(typeof(Mobius.Services.Types.CompoundId))]
    [KnownType(typeof(Mobius.Services.Types.DateTimeEx))]
    [KnownType(typeof(Mobius.Services.Types.StringEx))]
    [KnownType(typeof(Mobius.Services.Types.ImageEx))]
    [KnownType(typeof(Mobius.Services.Types.NumberEx))]
    [Serializable]
    public class FormattedDataType
    {
        [DataMember] public bool Filtered;
        [DataMember] public Bitmap FormattedBitmap;
        [DataMember] public string FormattedText;
        [DataMember] public bool Modified;
        [DataMember] public int RootRow;
        [DataMember] public Color ForeColor;
        [DataMember] public Color BackColor;
        [DataMember] public bool IsNull;
        [DataMember] public string Hyperlink;
        [DataMember] public string DbLink;
    }
}
