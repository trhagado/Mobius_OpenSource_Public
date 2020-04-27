using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public class UserObjectNode
    {
        [DataMember] public int Id; // database object identifier in MBS_USR_OBJ.OBJ_ID
        [DataMember] public UserObjectType Type; // CnList, Query, Annotation ...
        [DataMember] public string Owner = ""; // userid of object owner
        [DataMember] public string Name = ""; // name of object
        [DataMember] public string Description = ""; // description
        [DataMember] public string ParentFolder = ""; // id of folder this object is stored in (e.g. KINASE_TIER1, FOLDER_123)
        [DataMember] public FolderTypeEnum ParentFolderType = FolderTypeEnum.None; // type of folder object is stored in (System or User)
				[DataMember] public UserObjectAccess AccessLevel = UserObjectAccess.None;
				[DataMember] public string ACL = "";
				[DataMember] public int Count; // number of items in object
        [DataMember] public string Content = ""; // object content
        [DataMember] public DateTime CreationDateTime; // when object was created
        [DataMember] public DateTime UpdateDateTime; // when object last saved

        public UserObjectNode()
        {
        }

        public UserObjectNode(UserObjectType uonType)
        {
            this.Type = uonType;
        }
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum UserObjectType 
	{
		[EnumMember] Unknown       =  0,
		[EnumMember] CnList        =  1,
		[EnumMember] Query         =  2,
		[EnumMember] Structure     =  3,
		[EnumMember] UserParameter =  4,
		[EnumMember] CalcField     =  5,
		[EnumMember] Annotation    =  6,
		[EnumMember] CondFormat    =  7,
		[EnumMember] MetaRename    =  8, // renamed metadata
		[EnumMember] Folder        =  9, // user-created folder
		[EnumMember] Alert         = 10, // new-data alert
		[EnumMember] MultiTable    = 11, // metatable based on a multiple-table query
		[EnumMember] UserDatabase  = 12, // user-created compound library with associated annotation & computed data
		[EnumMember] ImportState   = 13, // import state for user data
		[EnumMember] BackgroundExport = 14,
		[EnumMember] Link = 15, // hyperlink or a link to an existing user object
		[EnumMember] UserGroup = 16,
		[EnumMember] SpotfireLink = 17 // link to Spotfire analysis that retrieves data associated with a query
	}

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum UserObjectAccess 
	{
		[EnumMember] None          = 0,
		[EnumMember] Private       = 1,
		[EnumMember] Public        = 2,
		[EnumMember] ACL           = 3

	}

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
	public enum FolderTypeEnum 
	{
		[EnumMember] None   =   0, // used for current list & other objects not in a specific folder
		[EnumMember] System = 256, // user object is stored in a system folder from basic non-user part of tree (equiv to MetaTreeNodeType.PersonalFolder)
		[EnumMember] User   = 512  // user object is stored in a user created-folder containing user objects (equiv to not MetaTreeNodeType.PersonalFolder)
	}
}
