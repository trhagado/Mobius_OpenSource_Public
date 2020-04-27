using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
    [DataContract(IsReference=true)]
    [Serializable]
    public class CalcFieldMetaTable : MetaTable
    {
        [DataMember] public CalcField CalcField = null; // full detail on associated calculated field
    }

    [DataContract(IsReference = true)]
    [Serializable]
    public class CalcField
    {
        [DataMember] public CalcTypeEnum CalcType = CalcTypeEnum.Basic; // type of calculation (Basic, Advanced)
        [DataMember] public MetaColumnType SourceColumnType = MetaColumnType.Unknown;
        [DataMember] public MetaColumnType PreclassificationlResultType = MetaColumnType.Unknown; // datatype for result of calc field before classification
        [DataMember] public MetaColumnType FinalResultType = MetaColumnType.Unknown; // final datatype for result of calc field

        [DataMember] public MetaColumn Column1 = null;
        [DataMember] public string Function1 = "";
        [DataMember] public CalcFuncEnum Function1Enum = CalcFuncEnum.Unknown;
        [DataMember] public string Constant1 = "";
        [DataMember] public double Constant1Double = 0;

        [DataMember] public string Operation = "";
        [DataMember] public CalcOpEnum OpEnum = CalcOpEnum.Unknown;

        [DataMember] public MetaColumn Column2 = null;
        [DataMember] public string Function2 = "";
        [DataMember] public CalcFuncEnum Function2Enum = CalcFuncEnum.Unknown;
        [DataMember] public string Constant2 = "";
        [DataMember] public double Constant2Double = 0;

        [DataMember] public CondFormat Classification = null; // defines any classification for calc field

        [DataMember] public string AdvancedExprs = ""; // expression list if in advanced mode
        [DataMember] public string Description = ""; // calc field description
        [DataMember] public UserObjectNode UserObjectNode = new UserObjectNode(UserObjectType.CalcField); // name, etc of CalcField
        [DataMember] public string Prompt = ""; // for standard calc fields this contains the prompt for the data to be supplied by the user
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public enum CalcTypeEnum
    {
        [EnumMember] Unknown  = 0,
        [EnumMember] Basic    = 1,
        [EnumMember] Advanced = 2
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public enum CalcFuncEnum
    {
        [EnumMember] Unknown       = 0,
        [EnumMember] None          = 1,
        [EnumMember] Abs           = 2, 
        [EnumMember] Neg           = 3,
        [EnumMember] Log           = 4,
        [EnumMember] NegLog        = 5,
        [EnumMember] NegLogMolConc = 6,
        [EnumMember] Ln            = 7,
        [EnumMember] NegLn         = 8,
        [EnumMember] Sqrt          = 9,
        [EnumMember] Square        = 10,
        [EnumMember] AddConst      = 11,
        [EnumMember] MultConst     = 12,
        [EnumMember] DaysSince     = 13
    }

    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    [Serializable]
    public enum CalcOpEnum
    {
        [EnumMember] Unknown = 0,
        [EnumMember] None    = 1,
        [EnumMember] Div     = 2,
        [EnumMember] Mul     = 3,
        [EnumMember] Add     = 4,
        [EnumMember] Sub     = 5
    }
}
