using Mobius.ComOps;

using System;
using System.Collections;
using System.Collections.Generic;

using System.Xml;
using System.Xml.Schema;

namespace Mobius.Data
{

	/// <summary>
	/// Aggregation defininition for a column
	/// </summary>

	public class AggregationDef
	{
		public AggregationRole Role = AggregationRole.Undefined;
		public SummaryTypeEnum SummaryType = SummaryTypeEnum.Undefined;
		public GroupingTypeEnum GroupingType = GroupingTypeEnum.Undefined;
		public Decimal NumericIntervalSize = 100;

		// Static model with default values used for comparison purposes

		public static AggregationDef ArithmeticMean = new AggregationDef(SummaryTypeEnum.ArithmeticMean); // helper for use in summarizing unpivoted SP data
		public static AggregationDef GeometricMean = new AggregationDef(SummaryTypeEnum.GeometricMean); // helper for use in summarizing unpivoted CRC data
		public static AggregationDef Model = new AggregationDef(); // simple default values for compariston purposes

		/// <summary>
		/// Basic constructor
		/// </summary>

		public AggregationDef()
		{
			return;
		}

		/// <summary>
		/// Construct with an initial summary type selected
		/// </summary>
		/// <param name="summaryTypeId"></param>

		public AggregationDef(SummaryTypeEnum summaryTypeId)
		{
			Role = AggregationRole.DataSummary;
			SummaryType = summaryTypeId;
		}

		/// <summary>
		/// Construct wiith an initial grouping type selected
		/// </summary>
		/// <param name="groupingTypeId"></param>

		public AggregationDef(GroupingTypeEnum groupingTypeId)
		{
			Role = AggregationRole.RowGrouping;
			GroupingType = groupingTypeId;
		}

		/// <summary>
		/// Construct from type name
		/// </summary>
		/// <param name="typeName"></param>

		public AggregationDef(string typeName)
		{
			AggregationTypeDetail atd = AggregationTypeDetail.GetByTypeName(typeName);
			if (atd == null) throw new Exception("Aggregation type not found: " + typeName);

			SetFromDetail(atd);
			return;
		}

/// <summary>
/// Construct from type detail info
/// </summary>
/// <param name="atd"></param>

		public AggregationDef(AggregationTypeDetail atd)
		{
			SetFromDetail(atd);
			return;
		}

		public void SetFromTypeName(string typeName)
		{
			AggregationTypeDetail atd = AggregationTypeDetail.GetByTypeName(typeName);
			SetFromDetail(atd);
			return;
		}

		public void SetFromDetail(AggregationTypeDetail atd)
		{
			if ((Role == AggregationRole.ColumnGrouping && atd.Role == AggregationRole.RowGrouping) ||
			 (Role == AggregationRole.RowGrouping && atd.Role == AggregationRole.ColumnGrouping))
				Role = Role; // don't change role if both are grouping roles
			else Role = atd.Role;

			SummaryType = atd.SummaryTypeId;
			GroupingType = atd.GroupingType;

			return;
		}

/// <summary>
/// Set role and default grouping/summary type based on the metacolumn type
/// </summary>
/// <param name="role"></param>
/// <param name="mct"></param>

		public void SetDefaultTypeIfUndefined(
			MetaColumn mc, 
			bool setIfAlreadyDefined = false)
		{
			MetaColumnType mct = mc.DataType;
			if (IsGroupingType)
			{
				if (mct == MetaColumnType.Date && GroupingType == GroupingTypeEnum.EqualValues)
					GroupingType = GroupingTypeEnum.Date; // fixup for date types

				if (GroupingType == GroupingTypeEnum.Undefined || setIfAlreadyDefined) 
				{
					if (mct == MetaColumnType.Date)
						GroupingType = GroupingTypeEnum.Date;

					else GroupingType = GroupingTypeEnum.EqualValues;
				}
			}

			else if (Role == AggregationRole.DataSummary)
			{
				if (SummaryType == SummaryTypeEnum.Undefined || setIfAlreadyDefined)
				{
					if (MetaColumn.IsNumericMetaColumnType(mct))
					{
						if (mc.SinglePoint && mc.MultiPoint)
							SummaryType = SummaryTypeEnum.ResultMean;

						else if (mc.MultiPoint)
							SummaryType = SummaryTypeEnum.GeometricMean;

						else SummaryType = SummaryTypeEnum.ArithmeticMean;
					}

					else SummaryType = SummaryTypeEnum.Count;
				}
			}

			return;
		}

		/// <summary>
		/// Get the label for the role
		/// </summary>

		public string RoleLabel
		{
			get
			{
				if (Role == AggregationRole.ColumnGrouping) return "Column Area";
				else if (Role == AggregationRole.RowGrouping) return "Row Area";
				else if (Role == AggregationRole.DataSummary) return "Data Area";
				else if (Role == AggregationRole.Filtering) return "Filter Area";
				else return "None";
			}
		}

		/// <summary>
		/// Get the associated detailed aggregation type information
		/// </summary>

		public AggregationTypeDetail TypeDetail
		{
			get
			{
				AggregationTypeDetail a = _aggTypeDetail;
				if (a != null)
				{
					if (a.Role == Role)
					{
						if (Role == AggregationRole.DataSummary && a.SummaryTypeId == SummaryType) return a;
						else if ((Role == AggregationRole.ColumnGrouping || Role == AggregationRole.RowGrouping) && a.GroupingType == GroupingType) return a;
					}
				}

				if (AggregationTypeDetail.TypeKeyDict.ContainsKey(DetailKey))
				{
					a = _aggTypeDetail = AggregationTypeDetail.TypeKeyDict[DetailKey];
					return a;
				}

				else return null; // not found
			}
		}
		private AggregationTypeDetail _aggTypeDetail = null;

		public string DetailKey
		{
			get
			{
				if (Role == AggregationRole.DataSummary) return "Summary." + SummaryType;
				else if (IsGroupingType) return "Grouping." + GroupingType;
				else return ""; // no key throw new Exception("Can't determine key for role: " + Role);
			}
		}

		/// <summary>
		/// Get "name" for aggregation type
		/// </summary>

		public string TypeName
		{
			get
			{
				AggregationTypeDetail ad = AggregationTypeDetail.GetByTypeKey(DetailKey);
				if (ad != null) return ad.TypeName;
				else return "";
			}
		}

		/// <summary>
		/// Get label for aggregation type
		/// </summary>

		public string TypeLabel
		{
			get
			{
				AggregationTypeDetail ad = this.TypeDetail;
				if (ad != null)
				{
					string label = ad.TypeLabel;

					if (ad.IsGroupingType && ad.GroupingType == GroupingTypeEnum.NumericInterval)
						label += " (" + NumericIntervalSize + ")";

					return label;
				}

				else return "";
			}
		}

		public string FormatString
		{
			get
			{
				AggregationTypeDetail ad = AggregationTypeDetail.GetByTypeKey(DetailKey);
				if (ad != null) return ad.FormatString;
				else return "";
			}
		}


		/// <summary>
		/// Get image for aggregation type
		/// </summary>

		public Bitmaps16x16Enum TypeImageIndex
		{
			get
			{
				AggregationTypeDetail ad = AggregationTypeDetail.GetByTypeKey(DetailKey);
				if (ad != null) return ad.TypeImageIndex;

				else return Bitmaps16x16Enum.None;
			}
		}


		/// <summary>
		/// Return true if an aggregation role is defined
		/// </summary>
		/// <returns></returns>
		public bool RoleIsDefined
		{
			get
			{
				if (Role != AggregationRole.Undefined)
					return true;
				else return false;
			}
		}

		/// <summary>
		/// Return true if grouping (row or column) is selected
		/// </summary>
		/// <returns></returns>
		public bool IsGroupingType
		{
			get
			{
				if (Role == AggregationRole.ColumnGrouping || Role == AggregationRole.RowGrouping)
					return true;
				else return false;
			}

		}

/// <summary>
/// Return true if summary type is one of the types of means
/// </summary>

		public bool IsMeanSummaryType
		{
			get
			{
				if (Role != AggregationRole.DataSummary) return false;
				if (SummaryType == SummaryTypeEnum.ArithmeticMean ||
				 SummaryType == SummaryTypeEnum.GeometricMean ||
				 SummaryType == SummaryTypeEnum.ResultMean)
					return true;

				else return false;
			}
		}

		/// <summary>
		/// Serialize
		/// </summary>
		/// <returns></returns>

		public void Serialize(
			XmlTextWriter tw)
		{
			tw.WriteStartElement("Aggregation");

			tw.WriteAttributeString("Role", Role.ToString());

			if (SummaryType != Model.SummaryType)
				tw.WriteAttributeString("SummaryType", SummaryType.ToString());

			if (GroupingType != Model.GroupingType)
				tw.WriteAttributeString("GroupingType", GroupingType.ToString());

			if (NumericIntervalSize != Model.NumericIntervalSize)
				tw.WriteAttributeString("NumericIntervalSize", NumericIntervalSize.ToString());

			tw.WriteEndElement();
			return;
		}

		/// <summary>
		/// Deserialize
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>

		public static AggregationDef Deserialize(
			XmlTextReader tr)
		{
			AggregationDef at = new AggregationDef();

			string txt = tr.GetAttribute("Role");
			if (Lex.IsDefined(txt))
				EnumUtil.TryParse(txt, out at.Role);

			txt = tr.GetAttribute("SummaryType");
			if (Lex.IsDefined(txt))
				EnumUtil.TryParse(txt, out at.SummaryType);

			txt = tr.GetAttribute("GroupingType");
			if (Lex.IsDefined(txt))
				EnumUtil.TryParse(txt, out at.GroupingType);

			XmlUtil.GetDecimalAttribute(tr, "NumericIntervalSize", ref at.NumericIntervalSize);

			if (!tr.IsEmptyElement) // get end element if this is not empty
			{
				tr.Read();
				tr.MoveToContent();

				if (!Lex.Eq(tr.Name, "Aggregation") || tr.NodeType != XmlNodeType.EndElement)
					throw new Exception("Expected Aggregation end element");
			}

			return at;
		}

	} // AggregationDef

	/// <summary>
	/// Detail on AggregationDef Ssmmary amd grouping types
	/// </summary>

	public class AggregationTypeDetail
	{
		public AggregationRole Role = AggregationRole.Undefined;
		public SummaryTypeEnum SummaryTypeId = SummaryTypeEnum.Undefined;
		public GroupingTypeEnum GroupingType = GroupingTypeEnum.Undefined;
		public string TypeName = ""; // internal type name
		public string TypeLabel = ""; // external type label
		public SummarizationMethodDelegate SummarizationMethod; // method to perform summarization of a set of values based on SummaryTypeId
		public GroupingMethodDelegate GroupingMethod; // method to group a value based on GroupingTypeId
		public Bitmaps16x16Enum TypeImageIndex = Bitmaps16x16Enum.None;
		public String FormatString = ""; // default formatting of the item value
		public bool FractionalSummaryResult = false; // summary value may be a fractional number (e.g. mean)

		// Aggregation type dictionary keyed by Type.Id

		public static Dictionary<string, AggregationTypeDetail> TypeKeyDict =
			new Dictionary<string, AggregationTypeDetail>();

		// Aggregation type items dictionary keyed by uppercase TypeName

		public static Dictionary<string, AggregationTypeDetail> TypeNameDict =
			new Dictionary<string, AggregationTypeDetail>();

/// <summary>
/// Detail on Summarization and Grouping types
/// </summary>

		public static AggregationTypeDetail 

			// Summarization Types

			SummaryCount = AddSummaryTypeItem(SummaryTypeEnum.Count, "Count", SMD(SummarizationMethods.CalculateCount)),
			SummaryCountDistinct = AddSummaryTypeItem(SummaryTypeEnum.CountDistinct, "Count (Distinct)", SMD(SummarizationMethods.CalculateCountDistinct)),
			SummarySum = AddSummaryTypeItem(SummaryTypeEnum.Sum, "Sum", SMD(SummarizationMethods.CalculateSum)),
			SummaryMinimum = AddSummaryTypeItem(SummaryTypeEnum.Minimum, "Minimum", SMD(SummarizationMethods.CalculateMinMax)),
			SummaryMaximum = AddSummaryTypeItem(SummaryTypeEnum.Maximum, "Maximum", SMD(SummarizationMethods.CalculateMinMax)),
			SummaryArithmeticMean = AddSummaryTypeItem(SummaryTypeEnum.ArithmeticMean, "Mean", SMD(SummarizationMethods.CalculateMean), "", true),
			SummaryGeometricMean = AddSummaryTypeItem(SummaryTypeEnum.GeometricMean, "Geometric Mean", SMD(SummarizationMethods.CalculateMean), "", true),
			SummaryResultMean = AddSummaryTypeItem(SummaryTypeEnum.ResultMean, "Result Mean", SMD(SummarizationMethods.CalculateMean), "", true),
			SummaryMostPotent = AddSummaryTypeItem(SummaryTypeEnum.MostPotent, "Most Potent", SMD(SummarizationMethods.CalculateMostPotent), "", true),
			SummaryMedian = AddSummaryTypeItem(SummaryTypeEnum.Median, "Median", SMD(SummarizationMethods.CalculateMedian)),
			SummaryMode = AddSummaryTypeItem(SummaryTypeEnum.Mode, "Mode", SMD(SummarizationMethods.CalculateMode)),
			SummaryConcatenate = AddSummaryTypeItem(SummaryTypeEnum.Concatenate, "Concatenate (All values)", SMD(SummarizationMethods.CalculateConcatenate)),
			SummaryConcatenateDistinct = AddSummaryTypeItem(SummaryTypeEnum.ConcatenateDistinct, "Concatenate (Distinct)", SMD(SummarizationMethods.CalculateConcatenate)),
			SummaryNumberQualifier = AddSummaryTypeItem(SummaryTypeEnum.NumberQualifier, "Number Qualifier (< or >)", SMD(SummarizationMethods.CalculateQualifier)), // return single value

			//SingleValue = AddSummaryTypeItem(AggTypeId.SingleValue, "SingleValue", "Single Value (Only value)", AMD(Aggregator2.CalculateSingleValue)), // return single value (not currently used)

			// Grouping Types

			GroupByEqualValues = AddGroupingTypeItem(GroupingTypeEnum.EqualValues, "Matching Values", GMD(GroupingMethods.EqualValues)),

			GroupByFirstLetter = AddGroupingTypeItem(GroupingTypeEnum.FirstLetter, "First Letter", GMD(GroupingMethods.FirstLetter)),
			GroupByNumericInterval = AddGroupingTypeItem(GroupingTypeEnum.NumericInterval, "Numeric Interval", GMD(GroupingMethods.NumericInterval)),

			GroupByDate = AddGroupingTypeItem(GroupingTypeEnum.Date, "Date", GMD(GroupingMethods.Date), "d-MMM-yyyy"), // date only, ignore time of day
			GroupByMonthYear = AddGroupingTypeItem(GroupingTypeEnum.DateMonthYear, "Month/Yr", GMD(GroupingMethods.DateMonthYear), "MMM-yyyy"),
			GroupByQuarterYear = AddGroupingTypeItem(GroupingTypeEnum.DateQuarterYear, "Quarter/Yr", GMD(GroupingMethods.DateQuarterYear), "<Q>-yyyy"),
			GroupByYear = AddGroupingTypeItem(GroupingTypeEnum.DateYear, "Year", GMD(GroupingMethods.DateYear), "yyyy"),

			GroupByQuarter = AddGroupingTypeItem(GroupingTypeEnum.DateQuarter, "Quarter", GMD(GroupingMethods.DateQuarter), "<Q>"),
			GroupByMonth = AddGroupingTypeItem(GroupingTypeEnum.DateMonth, "Month", GMD(GroupingMethods.DateMonth), "MMM"),

		// Undefined Type

			Undefined = AddUndefinedTypeItem();

		/// <summary>
		/// Basic constructor
		/// </summary>

		public AggregationTypeDetail()
			{
				return;
			}

		/// <summary>
		/// Key for dict
		/// </summary>

		public string DetailKey
		{
			get
			{
				if (Role == AggregationRole.DataSummary) return "Summary." + SummaryTypeId;
				else if (Role == AggregationRole.RowGrouping || Role == AggregationRole.ColumnGrouping) return "Grouping." + GroupingType;
				else if (Role == AggregationRole.Filtering) return "Filtering" + GroupingType;
				else if (Role == AggregationRole.Undefined) return "Undefined.Undefined";
				else throw new Exception("Can't determine key for role: " + Role);
			}
		}

		/// <summary>
		/// Construct from existing AggregationType Id
		/// </summary>
		/// <param name="typeId"></param>

		public AggregationTypeDetail(string key)
			{
				if (!AggregationTypeDetail.TypeKeyDict.ContainsKey(key)) throw new Exception("Unrecognized Aggregation type key: " + key);

				AggregationTypeDetail at = AggregationTypeDetail.TypeKeyDict[key];
				at.MemberwiseCopy(this);

				return;
			}

			/// <summary>
			/// Construct from existing AggregationType
			/// </summary>
			/// <param name="aggType"></param>

			public AggregationTypeDetail(AggregationTypeDetail aggType)
			{
				aggType.MemberwiseCopy(this);
				return;
			}

			public AggregationTypeDetail Clone()
			{
				AggregationTypeDetail at = (AggregationTypeDetail)this.MemberwiseClone();
				return at;
			}

			/// <summary>
			/// MemberwiseCopy
			/// </summary>
			/// <param name="dest"></param>

			public void MemberwiseCopy(object dest)
			{
				ObjectEx.MemberwiseCopy(this, dest);
				return;
			}

		/// <summary>
		/// Create SummarizationMethodDelegate instance
		/// </summary>
		/// <param name="smd"></param>
		/// <returns></returns>

		public static SummarizationMethodDelegate SMD(SummarizationMethodDelegate smd)
		{
			return new SummarizationMethodDelegate(smd);
		}

		/// <summary>
		/// Create GroupingMethodDelegate instance
		/// </summary>
		/// <param name="smd"></param>
		/// <returns></returns>

		public static GroupingMethodDelegate GMD(GroupingMethodDelegate gmd)
		{
			return new GroupingMethodDelegate(gmd);
		}

		/// <summary>
		/// Add an item to the type dictionary
		/// </summary>
		/// <param name="summaryType"></param>
		/// <param name="name"></param>
		/// <param name="label"></param>
		/// <param name="format"></param>
		/// <param name="image"></param>
		/// <param name="supportsSubTypes"></param>
		/// <returns></returns>

		public static AggregationTypeDetail AddSummaryTypeItem(
			SummaryTypeEnum summaryType,
			string label,
			SummarizationMethodDelegate summaryDelegate = null,
			string format = "",
			bool fractionalResult = false,
			Bitmaps16x16Enum image = Bitmaps16x16Enum.None)
		{
			AggregationTypeDetail item = new AggregationTypeDetail();
			item.Role = AggregationRole.DataSummary;
			item.SummaryTypeId = summaryType;
			item.TypeName = summaryType.ToString();
			item.FractionalSummaryResult = fractionalResult;
			item.SummarizationMethod = summaryDelegate;

			AddAggTypeDetailItem(item, label, format, image);

			return item;
		}

		public static AggregationTypeDetail AddGroupingTypeItem(
			GroupingTypeEnum groupingType,
			string label,
			GroupingMethodDelegate groupingDelegate = null,
			string format = "",
			Bitmaps16x16Enum image = Bitmaps16x16Enum.None)
		{
			AggregationTypeDetail item = new AggregationTypeDetail();
			item.Role = AggregationRole.RowGrouping;
			item.GroupingType = groupingType;
			item.TypeName = groupingType.ToString();
			item.GroupingMethod = groupingDelegate;

			AddAggTypeDetailItem(item, label, format, image);

			return item;
		}

		public static AggregationTypeDetail AddUndefinedTypeItem()
		{
			AggregationTypeDetail item = new AggregationTypeDetail();
			item.TypeName = "Undefined";
			AddAggTypeDetailItem(item, "None");

			return item;
		}

		public static void AddAggTypeDetailItem(
			AggregationTypeDetail item,
			string label,
			string format = "",
			Bitmaps16x16Enum image = Bitmaps16x16Enum.None)
		{
			item.TypeLabel = label;
			item.FormatString = format;
			item.TypeImageIndex = image;

			string key = item.DetailKey;
			if (TypeKeyDict.ContainsKey(key)) throw new Exception("Type Id already exists: " + key);
			TypeKeyDict[key] = item;

			string typeName = item.TypeName.ToUpper();
			if (TypeNameDict.ContainsKey(typeName)) throw new Exception("TypeName already exists: " + item.TypeName);
			TypeNameDict[typeName] = item;

			return;
		}

		/// <summary>
		/// GetById
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>

		public static AggregationTypeDetail GetByTypeKey(
			string typeKey,
			bool throwException = false)
		{
			if (TypeKeyDict.ContainsKey(typeKey))
				return TypeKeyDict[typeKey];
			else
			{
				if (throwException) throw new Exception("TypeKey not found: " + typeKey);
				else return null;
			}
		}

		/// <summary>
		/// GetByTypeName
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>

		public static AggregationTypeDetail GetByTypeName(
			string typeName,
			bool throwException = false)
		{
			typeName = typeName.ToUpper();

			if (TypeNameDict.ContainsKey(typeName))
				return TypeNameDict[typeName];

			else
			{
				if (throwException) throw new Exception("TypeName not found: " + typeName);
				else return null;
			}
		}

		/// <summary>
		/// Return true if grouping aggregation type (row or column) is selected
		/// </summary>
		/// <returns></returns>
		public bool IsGroupingType
		{
			get
			{
				if (Role == AggregationRole.ColumnGrouping || Role == AggregationRole.RowGrouping)
					return true;
				else return false;
			}
		}

		/// <summary>
		/// Return true if summary aggregation type (row or column) is selected
		/// </summary>

		//public bool IsSummaryType // (uncomment this)
		//{
		//	get
		//	{
		//		if (Role == AggregationRole.DataSummary)
		//			return true;
		//		else return false;
		//	}
		//}


	} // AggregationTypeDetail

	/// <summary>
	/// Aggregation Role
	/// </summary>

	public enum AggregationRole
	{
		Undefined = 0,
		RowGrouping = 1,
		ColumnGrouping = 2,
		Filtering = 3,
		DataSummary = 4
	}

	/// <summary>
	/// Summary type enum
	/// </summary>

	public enum SummaryTypeEnum
	{
		Undefined = 0,
		Count = 2,
		CountDistinct = 3,
		Sum = 4,
		Minimum = 5,
		Maximum = 6,
		ArithmeticMean = 7,
		GeometricMean = 8,
		ResultMean = 9,
		MostPotent = 10,
		Median = 11,
		Mode = 12,
		Concatenate = 14,
		ConcatenateDistinct = 15,
		NumberQualifier = 16,

		StdDev = 17, // not currently used
		StdDevp = 18,
		Var = 19,
		Varp = 20,

		//SingleValue = 13, "SingleValue", "Single Value (Only value)", AMD(Aggregator2.CalculateSingleValue)), // return single value (not currently used)
	}

	/// <summary>
	/// Grouping type enum
	/// </summary>

	public enum GroupingTypeEnum
	{
		Undefined = 0, 
		EqualValues = 1, // Groups combine matching values for all data types

		FirstLetter = 10, // Text values are grouped by the first letter of the string

		NumericInterval = 20, // Values are grouped into intervals as defined by a numeric interval size

		Date = 30, // Values are grouped by the date part. The time part of the values is ignored.
		DateMonth = 31, // Values are grouped by the month part. Examples of groups: January, February, March
		DateQuarter = 32, // Values are grouped by the quarterly intervals of the year, i.e.: 1, 2, 3 and 4. 
		DateYear = 33, // Values are grouped by the year part. Examples of such groups: 2003, 2004, 2005.

		DateMonthYear = 34, // Values are grouped by months and years. Examples of groups: August 2013, September 2014, January 2015, ...
		DateQuarterYear = 35, // Values are grouped by the year and quarter. Examples of groups: Q3 2012, Q4 2012, Q1 2013, Q2 2013, ...

		DateDay = 36,
		DateDayOfWeek = 37,
		DateDayOfYear = 38,
		DateWeekOfMonth = 39, // Values are grouped by the number of the week in which they occur in a month. The following groups can be created: 1, 2, 3, 4 and 5. The first week is the week containing the 1st day of the month.
		DateWeekOfYear = 40

	}
}
