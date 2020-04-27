#define USE_SYSTEM_DATA
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;
#if (USE_SYSTEM_DATA)
using System.Data;
#endif

namespace Mobius.Services.Types 
{
    [DataContract(IsReference=true)]
    [Serializable]
    public class Query //from query.cs
    {
        [DataMember] public int Id = -1; // query identifier
        [DataMember] public QueryMode Mode = QueryMode.Unknown; // current mode
        [DataMember] public QueryMode OldMode;
        [DataMember] public List<QueryTable> Tables = new List<QueryTable>(); // list of tables in query
        [DataMember] public QueryTable CurrentTable; // current query table in builder
        [DataMember] public string KeyCriteriaDisplay = ""; // display form of criteria on key
        [DataMember] public string KeyCriteria = ""; // sql form of criteria on key
        [DataMember] public List<string> DatabaseSubset = null; // keys of subset of database to be considered
        [DataMember] public UserObjectNode UserObjectNode = new UserObjectNode(UserObjectType.Query); // name, etc of query
        [DataMember] public QueryLogicType LogicType = QueryLogicType.And; // QueryBuilder type of logic for query, default is And
        [DataMember] public string ComplexCriteria = ""; // full Oracle-style criteria expression 
        [DataMember] public int SmallSsqMaxAtoms = -1; // if >=0 then upper atom count limit for imprecise structs
        [DataMember] public bool MinimizeDbLinkUse = true; // if true use database links only when necessary
        [DataMember] public bool FilterResults = true; // if true filter results by search criteria 
        [DataMember] public bool FilterNullRows = true; // if true filter null rows keeping at least 1 row per hit
        [DataMember] public bool FilterNullRowsStrong = false; // if true filter null rows possibly removing hits from results
        [DataMember] public bool RunQueryWhenOpened = false; // if true automatically run query when opened
        [DataMember] public bool BrowseExistingResultsWhenOpened = false; // if true go to browse of previous results when opened
        [DataMember] public bool Multitable = false; // allow query to be treated as a single table within other queries
        [DataMember] public int KeySortOrder = -1; // initial sort direction is descending
        [DataMember] public bool GroupSalts = true; // group by default (if feature enabled)
        [DataMember] public bool AllowColumnMerging = false; // true; // allow query columns to be merged
        [DataMember] public bool SingleStepExecution = false; // if true then combine search and retrieval into a single step
        [DataMember] public bool Preview = false; // if true then just preview a single table
        [DataMember] public bool Mobile;
        //[DataMember] public bool MobileDefault;

        [DataMember] public bool UseResultKeys = false; // flag saying that any existing ResultKeys should be used
        [DataMember] public List<string> ResultKeys = null; // if this keylist is set use it instead of other criteria
        //Per Tom, this is only used in the client (and never needs to pass between client and server)
        //[DataMember] public DataTable ResultsTable = null; // table of flattened results
        [DataMember] public bool SerializeMetaTables = false; // if true serialize MetaTables with query

        [DataMember] public bool RepeatReport = true; // repeat report multiple times across grid if possible
        [DataMember] public bool ShowCondFormatLabels = true; // if true show labels for cond formatting
        [DataMember] public bool ShowGridCheckBoxes = true; // if output to grid show checkboxes
        [DataMember] public bool PrefetchImages = true; // if output to grid allow filtering

        [DataMember] public int ViewScale = 100; // view zooming of the grid as a percentage
        [DataMember] public int PrintScale = 100; // print scale of the grid as a percentage, if negative then number of pages wide
        [DataMember] public int PrintMargins = -1; // Print margins in milliinches
        [DataMember] public Orientation PrintOrientation = Orientation.Vertical;

        [DataMember] public bool ShowStereoComments = true; // if true show any stereochemistry comments with structure (currently overridden by global var)
        [DataMember] public int AlertInterval = -1; // number of days between each check for new data (remove after transition to 2.0)
        [DataMember] public string AlertQueryState = ""; // subset of query used to determine if alert needs resetting (not saved)

        [DataMember] public int Timeout = 0; // timeout for query in seconds

        [DataMember] public string SpotfireTemplateName = ""; // template to use when exporting this query to spotfire
        [DataMember] public string SpotfireTemplateContent = ""; // template to use when exporting this query to spotfire
        [DataMember] public int StatDisplayFormat; // content & format for stat display
        [DataMember] public Dictionary<string, List<string>> InaccessibleData = null; // data that was removed during query processing because if inaccessible data sources
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum QueryMode //from query.cs
    {
        [EnumMember] Unknown = 0,
        [EnumMember] Build = 1,
        [EnumMember] Browse = 3,
        [EnumMember] PrintPreview = 4
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum QueryLogicType //from query.cs
    {
        [EnumMember] Unknown = 0,
        [EnumMember] And = 1,
        [EnumMember] Or = 2,
        [EnumMember] Complex = 3
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum Orientation //System.Windows.Forms.Orientation
    {
        [EnumMember] Horizontal = 0,
        [EnumMember] Vertical = 1
    }

    [DataContract(IsReference=true)]
    [Serializable]
    public class QueryTable
    {
        [DataMember] public MetaTable MetaTable = null; // associated metatable
        [DataMember] public Query Query; // associated query
        //public bool Summarized = false; // if true query summarized data
        //public string SummarizationType = ""; // type of data summarization (not currently used)
        [DataMember] public List<QueryColumn> QueryColumns = new List<QueryColumn>(); // list of query column information
        [DataMember] public string Label = ""; // table label if renamed by user
        [DataMember] public bool AllowColumnMerging = true;
        [DataMember] public string Alias = ""; // table alias used in complex criteria
        [DataMember] public Color HeaderBackgroundColor = null; // background color for table header line
        [DataMember] public int VoPosition = -1; // position within queryengine row where results for this QueryTable begin
    }

    [DataContract(IsReference=true)]
    [Serializable]
    public class QueryColumn
    {
        [DataMember] public MetaColumn MetaColumn; // associated metacolumn
        [DataMember] public int MetaColumnIdx = -1; // index of associated metacolumn
        [DataMember] public QueryTable QueryTable; // associated query table
        [DataMember] public bool Selected = false; // true if selected for output
        [DataMember] public string Criteria = ""; // sql form of criteria
        [DataMember] public string CriteriaDisplay = ""; // display form of criteria
        [DataMember] public string MolFile = ""; // molfile for structure search
        [DataMember] public bool ShowOnCriteriaForm = false; // if true display this column on criteria form
        [DataMember] public bool FilterSearch = true; // if true apply any criteria during initial search
        [DataMember] public bool FilterRetrieval = true; // if true apply any criteria during data retrieval
        [DataMember] public int SortOrder = 0; // sort order for column, pos for ascending, neg for descending
        [DataMember] public bool Merge = false; // if true then merge with previous column
        [DataMember] public string Aggregation = ""; // summarization & other functions for column
        [DataMember] public int VoPosition = -1; // position within queryengine row value object for column
        [DataMember] public CondFormat CondFormat = null; // any associated conditional formatting

        [DataMember] public string SecondaryCriteria = ""; // pseudo-sql form of secondary criteria
        [DataMember] public string SecondaryCriteriaDisplay = ""; // display form of secondary criteria
        [DataMember] public FilterType SecondaryFilterType = FilterType.Unknown;
        [DataMember] public bool ShowInFilterPanel = false; // if true display this column in the filter panel even if no criteria value

        [DataMember] public string Label = ""; // column label (if redefined by user)
        [DataMember] public string LabelImage = ""; // structure or ref to image to put in label
        [DataMember] public ColumnFormatEnum DisplayFormat = ColumnFormatEnum.Unknown; // output format (if redefined by user)
        [DataMember] public float DisplayWidth = -1; // output width in characters (if redefined by user)
        [DataMember] public int Decimals = -1; // number of decimal positions to display (if redefined by user)
        [DataMember] public HorizontalAlignmentEx HorizontalAlignment = HorizontalAlignmentEx.Default; // horizontal alignment within cell
        [DataMember] public VerticalAlignmentEx VerticalAlignment = VerticalAlignmentEx.Top; // vertical alignment within cell
        [DataMember] public bool Hidden = false; // if true column is currently hidden

        [DataMember] public static HorizontalAlignmentEx SessionDefaultHAlignment = HorizontalAlignmentEx.Default; // default for this session/user
        [DataMember] public static VerticalAlignmentEx SessionDefaultVAlignment = VerticalAlignmentEx.Top;
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum FilterType
    {
        [EnumMember] Unknown = 0, // undefined
        [EnumMember] BasicCriteria = 1, // simple text entry for basic filtering operations with 0 - 2 values  
        [EnumMember] CheckBoxList = 2, // select from a list of check boxes
        [EnumMember] ItemSlider = 3, // single item from a trackbar
        [EnumMember] RangeSlider = 4, // range of values from an range trackbar
        [EnumMember] StructureSearch = 5 // substructure search
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum HorizontalAlignmentEx
	{
		[EnumMember] Default   = 0,
		[EnumMember] Left      = 1,
		[EnumMember] Center    = 2,
		[EnumMember] Right     = 3,
		[EnumMember] Justified = 4
	}

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum VerticalAlignmentEx
	{
		[EnumMember] Default = 0,
		[EnumMember] Top     = 1,
		[EnumMember] Middle  = 2,
		[EnumMember] Bottom  = 3
	}
}
