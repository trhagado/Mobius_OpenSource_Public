using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public class ChemicalStructure : FormattedDataType
    {
        [DataMember] public string Id;
        [DataMember] public string Caption;
        //force the format to always be something that can travel on the wire -- prefer compoundId, chime, smiles, or molfile
        [DataMember] public StructureFormat Type;
        [DataMember] public string Value;
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public enum StructureFormat
	{
		[EnumMember] Unknown     = 0,
		[EnumMember] MolFile     = 1, // Molfile
		[EnumMember] MolFileName = 2, // Name of file containing a molfile
		[EnumMember] Sketch      = 3, // Internal sketch object (don't transfer via WCF)
		[EnumMember] SkcFileName = 4, // Name of sketch file
		[EnumMember] Chime       = 5, // chime string
		[EnumMember] Smiles      = 6, // Smiles
		[EnumMember] CompoundId  = 7  // Compound identifier
	}

}
