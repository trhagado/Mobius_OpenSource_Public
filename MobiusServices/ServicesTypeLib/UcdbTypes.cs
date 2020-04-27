using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public class UcdbDatabase
    {
        [DataMember] public long DatabaseId; // database id (database key)
        [DataMember] public string OwnerUserName = ""; // username of db owner
        [DataMember] public string NameSpace = ""; // containing namespace
        [DataMember] public string Name = ""; // db name (max length = 80)
        [DataMember] public bool Public; // true if shared publically
        [DataMember] public CompoundIdTypeEnum CompoundIdType; // type of external compound id
        [DataMember] public string Description = ""; // db description
        [DataMember] public int CompoundCount; // number of compounds in db
        [DataMember] public int ModelCount; // number of models associated with db
        [DataMember] public UcdbCompound[] Compounds; // array of associated compounds
        [DataMember] public UcdbModel[] Models; // array of associated models
        [DataMember] public bool StructureSearchSupported = true;
        [DataMember] public bool AllowDuplicateStructures = true;
        [DataMember] public UcdbWaitState PendingStatus; // status of db as updated by background process
        [DataMember] public int PendingCompoundCount; // number of compounds pending
        [DataMember] public long PendingCompoundId; // current compound id position for updating for added models
        [DataMember] public DateTime PendingUpdateDate; // date pending data most recently updated
        [DataMember] public DateTime CreationDate; // date db created
        [DataMember] public DateTime UpdateDate; // date db most recently updated
        [DataMember] public UcdbRowState RowState; // in-memory insert, update, delete state of row
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum CompoundIdTypeEnum
    {
        [EnumMember] Integer = 1,
        [EnumMember] String = 2
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum UcdbWaitState
    {
        [EnumMember] Unknown = 0,
        [EnumMember] DatabaseStorage = 1,
        [EnumMember] StrSearchUpdate = 2,
        [EnumMember] ModelPredictions = 3,
        [EnumMember] Deletion = 4,
        [EnumMember] Complete = 5
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum UcdbRowState
    {
        [EnumMember] Unknown = 0,
        [EnumMember] Added = 1,
        [EnumMember] Unchanged = 2,
        [EnumMember] Modified = 3,
        [EnumMember] Deleted = 4
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public class UcdbModel
    {
        [DataMember] public long DbModelId; // database primary key associating model/build with database
        [DataMember] public long DatabaseId; // database id
        [DataMember] public long ModelId; // numeric identifer for data (e.g. model id)
        [DataMember] public long BuildId; // secondary numeric identifer for data (e.g. model build id)
        [DataMember] public UcdbWaitState PendingStatus; // status of database model as updated by background process
        [DataMember] public DateTime CreationDate; // date created
        [DataMember] public DateTime UpdateDate; // date updated
        [DataMember] public UcdbRowState RowState; // insert, update, delete state of row
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public class UcdbCompound
    {
        [DataMember] public long CompoundId; // compound id (database key)
        [DataMember] public long DatabaseId; // database id - Stored in UCDB_DB_CMPND
        [DataMember] public long ExtCmpndIdNbr; 
        [DataMember] public string ExtCmpndIdTxt; 
        [DataMember] public MolStructureFormatEnum MolStructureFormat; // format of mol structure
        [DataMember] public string MolStructure = ""; // string representation of structure
        [DataMember] public string MolFormula = "";
        [DataMember] public double MolWeight;
        [DataMember] public byte[] MolKeys; // substructure/similarity search keys
        [DataMember] public string Comment = "";
        [DataMember] public string CreatorUserName = ""; // original creator, may be referenced by other user's databases
        [DataMember] public UcdbAlias[] Aliases; // aliases for molecule
        [DataMember] public UcdbWaitState PendingStatus; // status of compound registration as updated by background process
        [DataMember] public DateTime CreationDate; // date db created
        [DataMember] public DateTime UpdateDate; // date db most recently updated
        [DataMember] public UcdbRowState RowState; // in-memory insert, update, delete state of row
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum MolStructureFormatEnum
    {
        [EnumMember] Unknown = 0,
        [EnumMember] MolFile = 1, // molfile
        [EnumMember] Chime = 2, // chime string
        [EnumMember] Smiles = 3, // Smiles
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public class UcdbAlias
    {
        [DataMember] public long AliasId; // alias id (primary database key)
        [DataMember] public long CompoundId; // associated compound id
        [DataMember] public string Name; // the alias name
        [DataMember] public int Type; // type of alias
        [DataMember] public DateTime CreationDate; // date created
        [DataMember] public DateTime UpdateDate; // date most recently updated
        [DataMember] public UcdbRowState RowState; // insert, update, delete state of row
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public class UcdbTestModelServiceResult
    {
        [DataMember] public int ResultInt;
        [DataMember] public string ResultsText;
        [DataMember] public int ErrorCount;
        [DataMember] public Exception Exception;
    }
}
