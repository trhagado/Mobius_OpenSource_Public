using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
    [DataContract(IsReference=true)]
    [KnownType(typeof(Mobius.Services.Types.CalcFieldMetaTable))]
    [Serializable]
    public class MetaTable
    {
        [DataMember] public int Id; //	table id
        [DataMember] public MetaBrokerType MetaBrokerType; // metabroker type
        [DataMember] public MetaTable Parent; // parent metatable to this table
        [DataMember] public string Source = ""; // source Oracle table, view or select statement
        [DataMember] public string Name = ""; // table name
        [DataMember] public string Label = ""; // table label
        [DataMember] public string ShortLabel = ""; // short label for table
        [DataMember] public bool AllowColumnMerging = true;
        [DataMember] public string Description = ""; // description of table or link to it (clients should use GetDescription)
        [DataMember] public string RowCountSql = ""; // sql for selecting row count if generic broker
        [DataMember] public bool SummarizedExists = false; // if true summarized version is available
        [DataMember] public bool UnsummarizedExists = false; // if true unsummarized version is available
        [DataMember] public bool UseSummarizedData = false; // if true query summarized data
        [DataMember] public SummarizationTypeEnum SummarizationType = SummarizationTypeEnum.None; // type of data summarization (not currently used)
        [DataMember] public string Code = ""; // simple code for table/assay used for pivoting
        [DataMember] public UserDataImportParms ImportParms; // default import parameters for table 
        [DataMember] public string Creator = ""; // creator of this table
        [DataMember] public string UpdateDateTimeSql = ""; // sql for selecting update date if generic broker

        // Pivoting information for unpivoted sources(optional)

        [DataMember] public List<string> TableFilterColumns = null; // columns in source table to filter on for this metatable
        [DataMember] public List<string> TableFilterValues = null; // values in PivotFilterColumn that identifies this table for pivoting
        [DataMember] public List<string> PivotMergeColumns = null; // columns in source table to use to merge sets of unpivoted rows
        [DataMember] public List<string> PivotColumns = null; // columns in source table to pivot on, values are stored in associated metacolumns
        [DataMember] public bool RemapForRetrieval = false; // if true pivot this table into multiple tables based on hit list & data source

        // Qualified number column set for table (optional)

        [DataMember] public string QnQualifier = ""; // qualified number qualifier column in source table
        [DataMember] public string QnNumberValue = ""; // basic numeric value col
        [DataMember] public string QnNumberValueHigh = ""; // high numeric value for range col
        [DataMember] public string QnNValue = ""; // number of results going into stats col
        [DataMember] public string QnNValueTested = ""; // with number of results tested col
        [DataMember] public string QnStandardDeviation = ""; // QN SD col
        [DataMember] public string QnStandardError = ""; // QN SE col
        [DataMember] public string QnTextValue = ""; // text value col
        [DataMember] public string QnLinkValue = ""; // link value for col

        [DataMember] public List<MetaColumn> MetaColumns; // ordered array of MetaColumn refs for table

		[DataMember] public MetaTable KeyMetaTable; // default key metatable?
        [DataMember] public const int MAX_COLUMNS = 256; // maximum number of allowed columns
        [DataMember] public const string SummarySuffix = "_SUMMARY"; // suffix for tables that are summary tables
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum SummarizationTypeEnum
    {
        [EnumMember] Unknown  = 0,
        [EnumMember] None     = 1,
        [EnumMember] Sample   = 2,
        [EnumMember] Lot      = 3,
        [EnumMember] Compound = 4
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public class UserDataImportParms
    {
        [DataMember] public string FileName = "";
        [DataMember] public char Delim = ' ';
        [DataMember] public bool MultDelimsAsSingle = false;
        [DataMember] public char TextQualifier = ' ';
        [DataMember] public bool FirstLineHeaders = false;
        [DataMember] public bool DeleteExisting = false;
        [DataMember] public bool ImportInBackground = false;
        [DataMember] public bool CheckForFileUpdates = false; // check for updated client file & update associated data 
        [DataMember] public DateTime ClientFileModified; // last time modified, used for autoupdate check
        [DataMember] public List<MetaColumnType> Types; // column types (not persisted)
        [DataMember] public List<string> Labels; // column headers (not persisted)
        [DataMember] public Dictionary<string, MetaColumn> Fc2Mc; // field column position/name to MetaColumn map (not persisted)
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public class MetaTableDescription
    {
        [DataMember] public string TextDescription; // if the description is text or a URL/UNC link
        [DataMember] public byte[] BinaryDescription; // content of a binary document
        [DataMember] public string TypeName; // document type if binary description
    }

}
