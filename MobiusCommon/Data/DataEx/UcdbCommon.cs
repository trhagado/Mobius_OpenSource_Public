using System;
using System.Collections.Generic;
using System.Text;

namespace Mobius.Data
{

/// <summary>
/// User Compound Database
/// </summary>

	public class UcdbDatabase
	{
		public long DatabaseId; // database id (database key)
		public string OwnerUserName = ""; // username of db owner
		public string NameSpace = ""; // containing namespace
		public string Name = ""; // db name (max length = 80)
		public bool Public; // true if shared publically
		public CompoundIdTypeEnum CompoundIdType; // type of external compound id
		public string Description = ""; // db description
		public int CompoundCount; // number of compounds in db
		public int ModelCount; // number of models associated with db
		public UcdbCompound[] Compounds; // array of associated compounds
		public UcdbModel[] Models; // array of associated models
		public bool StructureSearchSupported = true;
		public bool AllowDuplicateStructures = true;
		public UcdbWaitState PendingStatus; // status of db as updated by background process
		public int PendingCompoundCount; // number of compounds pending
		public long PendingCompoundId; // current compound id position for updating for added models
		public DateTime PendingUpdateDate; // date pending data most recently updated
		public DateTime CreationDate; // date db created
		public DateTime UpdateDate; // date db most recently updated
		public UcdbRowState RowState; // in-memory insert, update, delete state of row
	}

	/// <summary>
	/// Database/compound assoc
	/// </summary>

	public class UcdbDatabaseCompound
	{
		public long DatabaseId; // database id (database key)
		public long CompoundId; // compound id (database key)
		public long ExtCmpndIdInteger; // integer compoundId relative to this db
		public string ExtCmpndIdString; // string compoundId relative to this db
		public DateTime CreationDate; // date db created
		public DateTime UpdateDate; // date db most recently updated
	}

	/// <summary>
	/// Compound class
	/// </summary>

	public class UcdbCompound
	{
		public long CompoundId; // compound id (database key)
		public long DatabaseId; // database id - Stored in UCDB_DB_CMPND
		public long ExtCmpndIdNbr; // virtual compound id within current db - Stored in UCDB_DB_CMPND
		public string ExtCmpndIdTxt; // virtual compound id within current db - Stored in UCDB_DB_CMPND
		public MolStructureFormatEnum MolStructureFormat; // format of mol structure
		public string MolStructure = ""; // string representation of structure
		public string MolFormula = "";
		public double MolWeight;
		public byte[] MolKeys; // substructure/similarity search keys
		public string Comment = "";
		public string CreatorUserName = ""; // original creator, may be referenced by other user's databases
		public UcdbAlias[] Aliases; // aliases for molecule
		public UcdbWaitState PendingStatus; // status of compound registration as updated by background process
		public DateTime CreationDate; // date db created
		public DateTime UpdateDate; // date db most recently updated
		public UcdbRowState RowState; // in-memory insert, update, delete state of row
	}

	/// <summary>
	/// Enumeration of structure format types
	/// </summary>

	public enum MolStructureFormatEnum
	{
		Unknown = 0,
		MolFile = 1, // molfile
		Chime = 2, // chime string
		Smiles = 3, // Smiles
	}

	/// <summary>
	/// Alias class
	/// </summary>

	public class UcdbAlias
	{
		public long AliasId; // alias id (primary database key)
		public long CompoundId; // associated compound id
		public string Name; // the alias name
		public int Type; // type of alias
		public DateTime CreationDate; // date created
		public DateTime UpdateDate; // date most recently updated
		public UcdbRowState RowState; // insert, update, delete state of row
	}

	/// <summary>
	/// Models associated with database
	/// </summary>

	public class UcdbModel
	{
		public long DbModelId; // database primary key associating model/build with database
		public long DatabaseId; // database id
		public long ModelId; // numeric identifer for data (e.g. model id)
		public long BuildId; // secondary numeric identifer for data (e.g. model build id)
		public UcdbWaitState PendingStatus; // status of database model as updated by background process
		public DateTime CreationDate; // date created
		public DateTime UpdateDate; // date updated
		public UcdbRowState RowState; // insert, update, delete state of row
	}

	/// <summary>
	/// Data type for database compound id
	/// </summary>

	public enum CompoundIdTypeEnum
	{
		Integer = 1,
		String = 2
	}

	/// <summary>
	/// Insert/update/delete state of row, parallels ADO.Net DataRowState 
	/// </summary>

	public enum UcdbRowState
	{
		Unknown = 0,
		Added = 1,
		Unchanged = 2,
		Modified = 3,
		Deleted = 4
	}

	/// <summary>
	/// Database wait state with respect to background updating processes
	/// </summary>

	public enum UcdbWaitState
	{
		Unknown = 0,
		DatabaseStorage = 1,
		StrSearchUpdate = 2,
		ModelPredictions = 3,
		Deletion = 4,
		Complete = 5
	}


}
