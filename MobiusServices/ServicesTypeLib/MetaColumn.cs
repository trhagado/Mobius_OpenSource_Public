using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;
using System.Drawing;


namespace Mobius.Services.Types
{
    [DataContract(IsReference = true)]
    [Serializable]
    public class MetaColumn
    {
        [DataMember] public int Id; // id of column
        [DataMember] public string Name = ""; // name of column
        [DataMember] public int MetaTableId; // id of assoc metatable
        [DataMember] public MetaTable MetaTable; // ref to assoc metatable
        [DataMember] public string Label = ""; // label 
        [DataMember] public string ShortLabel = ""; // shorter label
        [DataMember] public string LabelImage = ""; // structure or ref to image to put in label
        [DataMember] public MetaColumnType DataType; // Mobius data type
        [DataMember] public MetaColumnStorageType StorageType; // associated type in underlying database
        [DataMember] public List<string> PivotValues = null; // values that pivot column must have to pivot this column from source table
        [DataMember] public string ResultCode = ""; // older code identifying specific measurement, endpoint etc
        [DataMember] public string SummaryResultCode = ""; // older code for associated summary column
        [DataMember] public bool UnsummarizedExists = true; // true if exists in database in unsummarized form
        [DataMember] public bool SummarizedExists = false; // true if exists in database in summarized form
        [DataMember] public SummarizationRole SummarizationRole; // summarization role - SummarizationRole.Independent, DEPENDENT, etc. 
        [DataMember] public bool PrimaryResult = false; // if true this is a/the primary result for the metatable
        [DataMember] public bool SecondaryResult = false; // if true this is a/the secondary result for the metatable
        [DataMember] public bool SinglePoint = false; // true single point (SP) result
        [DataMember] public bool MultiPoint = false; // true multi point (CRC) result
        [DataMember] public bool DetailsAvailable = false; // can drill down on this column (e.g. IC50 -> %Inh)
        [DataMember] public string Dictionary = ""; // dictionary values come from
        [DataMember] public bool DictionaryMultipleSelect = false; // if true allow selection of multiple dictionary items
        [DataMember] public string Description = ""; // column description
        [DataMember] public int UnitId; // code for units
        [DataMember] public string Units = ""; // string for units
        [DataMember] public int Position; // position for column
        [DataMember] public ColumnSelectionEnum Selection = ColumnSelectionEnum.Selected; // initial selection/visibility state for column
        [DataMember] public ColumnFormatEnum Format = ColumnFormatEnum.Default; // output format
        [DataMember] public float Width; // output width in "characters"
        [DataMember] public int Decimals; // number of decimal positions to display
        [DataMember] public CondFormat CondFormat = null; // any associated conditional formatting
        [DataMember] public ColumnTextCaseEnum TextCase; // case character data is stored in see CASE_xxx
        [DataMember] public bool Searchable = true; // if true column is searchable
        [DataMember] public bool BrokerFiltering = false; // if true specific broker does filtering of data
        [DataMember] public string TableMap = ""; // source table for column
        [DataMember] public string ColumnMap = ""; // source table column
        [DataMember] public string DataTransform = ""; // transformation to be applied to data before formatting
        [DataMember] public string Creator = ""; // creator of metadata
        [DataMember] public MetaBrokerType MetaBrokerType = MetaBrokerType.Unknown; // metabroker type specific to this column differing from table broker
        [DataMember] public string ClickFunction = ""; // function to execute when item clicked in grid
        [DataMember] public string ImportFilePosition = ""; // position in file for loading (ordinal or field name)
        [DataMember] public string UpdateDateTime = ""; // when last updated

        [DataMember] public const int MaxIdentifierLength = 30; // maximum identifier length for name
        [DataMember] public int KeyPosition;
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum MetaColumnType 
	{
		[EnumMember] Unknown      = 0, // no type assigned
		[EnumMember] Integer      = 1, // integer
		[EnumMember] Number       = 2, // double floating point number
		[EnumMember] QualifiedNo  = 3, // qualified number (e.g. > 50)
		[EnumMember] String       = 4, // string
		[EnumMember] Date         = 5, // date

		[EnumMember] CompoundId   = 10, // compound id
		[EnumMember] Structure    = 11, // chemical structure
		[EnumMember] MolFormula   = 12, // mol formula
		[EnumMember] Image        = 13, // image
		[EnumMember] DictionaryId = 14, // numeric dictionary id
		[EnumMember] Hyperlink    = 15, // link to web content
		[EnumMember] Html         = 16, // html text
		[EnumMember] DbSet        = 17  // set of databases to search
	}

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
	public enum ColumnTextCaseEnum
	{
		[EnumMember] Unknown = 0,
		[EnumMember] Lower = 1,
		[EnumMember] Upper = 2,
		[EnumMember] Mixed = 3
	}

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
	public enum MetaColumnStorageType
	{
		[EnumMember] Unknown  = 0,
		[EnumMember] Integer  = 1,
		[EnumMember] Decimal  = 2,
		[EnumMember] String   = 3,
		[EnumMember] DateTime = 4,
		[EnumMember] Clob     = 5,
		[EnumMember] Blob     = 6,
		[EnumMember] BFile    = 7,
		[EnumMember] Object   = 8
	}

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
	public enum SummarizationRole
	{
		[EnumMember] Unknown     = 0,
		[EnumMember] Independent = 1,
		[EnumMember] Dependent   = 2,
		[EnumMember] Normalized  = 2,
		[EnumMember] Derived     = 3,
		[EnumMember] Summarized  = 4,
		[EnumMember] Misc        = 5
	}

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum ColumnSelectionEnum
	{
		[EnumMember] Unknown           = 0,
		[EnumMember] Selected          = 1,
		[EnumMember] Unselected        = 2,
		[EnumMember] Unselectable      = 3,
		[EnumMember] Hidden            = 4,
		[EnumMember] Comment           = 5 // this item is a comment, not selectable or searchable
	}

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum ColumnFormatEnum
    {
        [EnumMember] Unknown = 0,
        [EnumMember] Default = 1,
        [EnumMember] Decimal = 2, // fixed number of decimal places
        [EnumMember] SigDigits = 3, // display number of significant digits
        [EnumMember] Scientific = 4
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public class CondFormat
    {
        [DataMember] public MetaColumnType ColumnType = MetaColumnType.Unknown; // type for metacolumn used in cases where no explicit table, column
        [DataMember] public string Name = ""; // assigned name, allows sharing
        [DataMember] public bool Option1 = false; // first dataType specific formatting option
        [DataMember] public bool Option2 = false; // second dataType specific formatting option
        [DataMember] public CondFormatRules Rules = new CondFormatRules(); // associated rules
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public class CondFormatRules
    {
        [DataMember] public bool ColorContinuously = false;
        [DataMember] public List<CondFormatRule> rulesList = new List<CondFormatRule>();
    }

    /// <summary>
    /// A single conditional formatting rule
    /// </summary>
    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public class CondFormatRule
    {
        [DataMember] public string Name = ""; // name of condition
        [DataMember] public string Op = ""; // operation
        [DataMember] public CondFormatOpCode OpCode;
        [DataMember] public string Value = "";
        [DataMember] public string ValueNormalized = null; // for holding normalized values (e.g. normalized dates)
        [DataMember] public double ValueNumber; // number form of value
        [DataMember] public Dictionary<string, object> ValueDict = null; // dictionay of list values
        [DataMember] public string Value2 = "";
        [DataMember] public string Value2Normalized = null;
        [DataMember] public double Value2Number;
        [DataMember] public double Epsilon = 0.0;
        [DataMember] public Font Font = null;
        [DataMember] public Color ForeColor = Color.Black;
        [DataMember] public Color BackColor = Color.White;
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public class Font
    {
        [DataMember] public string Family;
        [DataMember] public float Size;
        [DataMember] public string Style;

        public Font()
        {
        }

        public Font(System.Drawing.Font font)
        {
            if (font != null)
            {
                Family = font.FontFamily.Name;
                Size = font.Size;
                Style = font.Style.ToString();
            }
        }

        public static explicit operator System.Drawing.Font(Font serviceFont)
        {
            System.Drawing.Font font = null;
            if (serviceFont.Family != null && serviceFont.Size > 0.0)
            {
                try
                {
                    font =
                        new System.Drawing.Font(serviceFont.Family, serviceFont.Size,
                                                (FontStyle)Enum.Parse(typeof(System.Drawing.FontStyle), serviceFont.Style));
                }
                catch (Exception)
                {
                    //do nothing -- let NULL be returned
                }
            }
            return font;
        }
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public class Color
    {
        public static Color Black = new Color(System.Drawing.Color.Black);
        public static Color White = new Color(System.Drawing.Color.White);

        [DataMember] public int A;
        [DataMember] public int R;
        [DataMember] public int G;
        [DataMember] public int B;

        public Color()
        {
        }

        public Color(System.Drawing.Color color)
        {
            //color is a non-nullable type
            A = color.A;
            R = color.R;
            G = color.G;
            B = color.B;
        }

        public static explicit operator System.Drawing.Color(Color serviceColor)
        {
            System.Drawing.Color color = System.Drawing.Color.Empty;
            if (serviceColor != null)
            {
                color = System.Drawing.Color.FromArgb(
                    serviceColor.A, serviceColor.R, serviceColor.G, serviceColor.B);
            }
            return color;
        }
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public enum CondFormatOpCode
    {
        [EnumMember] Unknown    = 0,
        [EnumMember] Between    = 1,
        [EnumMember] NotBetween = 2,
        [EnumMember] Eq         = 3,
        [EnumMember] NotEq      = 4,
        [EnumMember] Gt         = 5,
        [EnumMember] Lt         = 6,
        [EnumMember] Ge         = 7,
        [EnumMember] Le         = 8,
        [EnumMember] Null       = 9,
        [EnumMember] NotNull    = 10,
        [EnumMember] Exists     = 11,
        [EnumMember] Substring  = 12,
        [EnumMember] Within     = 13,
        [EnumMember] SSS        = 14,
        [EnumMember] NotExists  = 15
    }
}
