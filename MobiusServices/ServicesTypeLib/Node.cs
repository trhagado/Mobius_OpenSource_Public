using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public class Node
    {
        [DataMember] public string Name = "";   // name of node -- unique identifier
        [DataMember] public string Label = "";  // label for node
        [DataMember] public NodeType Type;      // type of node
        [DataMember] public string Target = ""; // string id of associated object, defaults to node name
        [DataMember] public int Size = -1;      // size of assoc object (also .Count for UserObject objects)
        [DataMember] public DateTime CreationTimestamp = DateTime.MinValue;
        [DataMember] public DateTime LastUpdateTimestamp = DateTime.MinValue; // most-recent update date for the entity (useful for finding deltas)

        [DataMember] public string Owner = "";     // string id of the user (if any) of the object represented by this node, may be changed from id to real name
        [DataMember] public bool Shared = true;    // bool to flag user objects.  Needed so that pub/priv can be cascaded through user folders.
        [DataMember] public string Description = "";

        [DataMember] public GraphElementStatus Status = GraphElementStatus.present; //for support of deltas
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum NodeType
    {
		[EnumMember] Unknown        =        0,
		[EnumMember] Root           =        1,
		[EnumMember] Database       =        2,
		[EnumMember] DatabaseView   =        4,
		[EnumMember] Site           =        8,
		[EnumMember] WorkAreaType   =       16, // e.g. therapeutic area, hts
		[EnumMember] WorkArea       =       32, // e.g. CNS
		[EnumMember] Pathway        =       64,
		[EnumMember] Platform       =      128, // e.g. Kinase
		[EnumMember] Target         =      256, 
		[EnumMember] Project        =      512,
		[EnumMember] SystemFolder   =     4096, // general system folder 
		[EnumMember] UserFolder     =     8192, // user folder containing user objects
		[EnumMember] LastFolderType =     8192, // end of folder types

		[EnumMember] MetaTable      =   131072,
		[EnumMember] Url            =   262144,
		[EnumMember] Query          =   524288,
		[EnumMember] CnList         =  1048576,
		[EnumMember] Action         =  2097152,
		[EnumMember] CalcField      =  4194304,
		[EnumMember] Annotation     =  8388608,
		[EnumMember] CondFormat     = 16777216,

        // ** UserObjectType types **
        [EnumMember] Structure           = (1<<25) + 0,
        [EnumMember] UserParameter       = (1<<25) + 1,
        [EnumMember] MetaRename          = (1<<25) + 2,
        // For UserObjectType.Folder, use NodeType.UserFolder -- mapping enum values by name gets dicey when multiple names exist with the same underlying value
        [EnumMember] Alert               = (1<<25) + 4,
        [EnumMember] MultiTable          = (1<<25) + 5,
        [EnumMember] UserDatabase        = (1<<25) + 6,
        [EnumMember] ImportState         = (1<<25) + 7,
        [EnumMember] BackgroundExport    = (1<<25) + 8
    }
}
