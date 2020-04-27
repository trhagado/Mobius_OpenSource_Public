using Mobius.ComOps;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Mobius.Data
{
	/// <summary>
	/// Summary description for QueryColumn.
	/// </summary>
	public class QueryColumn
	{
		internal int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		public MetaColumn MetaColumn; // associated metacolumn
		public QueryTable QueryTable; // associated query table
		public bool Selected = false; // true if selected for output
		public bool Hidden = false; // true if temporarily hidden by user
		public string Criteria = ""; // sql form of criteria
		public string CriteriaDisplay = ""; // display form of criteria
		public string MolString = null; // Molecule string for any structure search criteria, molfile or type-qualified type
		public bool ShowOnCriteriaForm = false; // if true display this column on criteria form
		public bool FilterSearch = true; // if true apply any criteria during initial search
		public bool FilterRetrieval = true; // if true apply any criteria during data retrieval
		public int SortOrder = 0; // sort order for column, pos for ascending, neg for descending
		public bool Merge = false; // if true then merge with previous column
		public AggregationDef Aggregation = null;
		//public SummaryTypeAttrs AggregationType = null; // summarization type for this column if summarized
		//public AggregationProps BinningDef = null; // binning info if this col is bined
		public int VoPosition = -1; // position within queryengine row value object for column
		//public int VoPosition { // debug
		//	get { return _voPosition; }
		//	set {
		//		_voPosition = value;
		//		if (MetaColumn != null && MetaColumn.Name == "SUBSTITUTIONS" && value == 12) _voPosition = _voPosition;
		//	}
		//} // = -1; // position within queryengine row value object for column
		public int _voPosition = -1; // position within queryengine row value object for column
		public int SpotfireExportPos = -1; // index assinged to this col when exported to Spotfire
		public object SpotfireExportType; // Spotfire type for export)
		public CondFormat CondFormat = null; // any associated conditional formatting

		public string SecondaryCriteria = ""; // pseudo-sql form of secondary criteria
		public string SecondaryCriteriaDisplay = ""; // display form of secondary criteria
		public FilterType SecondaryFilterType = FilterType.Unknown;
		public bool ShowInFilterPanel = false; // if true display this column in the filter panel even if no criteria value

		public string Label = ""; // column label (if redefined by user)
		public string LabelImage = ""; // structure or ref to image to put in label

		public ColumnFormatEnum DisplayFormat = ColumnFormatEnum.Unknown; // output format (if redefined by user)
		public string DisplayFormatString = ""; // output format string (if redefined by user)
		public float DisplayWidth = -1; // output width in characters (if redefined by user)
		public int Decimals = -1; // number of decimal positions to display (if redefined by user from metacolumn)

		public VerticalAlignmentEx VerticalAlignment = VerticalAlignmentEx.Default; // vertical alignment within cell
		public HorizontalAlignmentEx HorizontalAlignment = HorizontalAlignmentEx.Default; // horizontal alignment within cell

		public const HorizontalAlignmentEx DefaultHAlignment = HorizontalAlignmentEx.Default;
		public const VerticalAlignmentEx DefaultVAlignment = VerticalAlignmentEx.Top;

		public int TempInt = -1; // temporary integer for short-term scratch-pad use (not serialized)

		public MetaTable MetaTable => QueryTable?.MetaTable; // associated metatable

		public Query Query => QueryTable?.Query; // associated Query

		/// <summary>
		/// Active Horizontal Alignment - Get from Metacolumn if QueryColumn is default value 
		/// else get non-default value from QueryColumn
		/// </summary>
		/// <returns></returns>

		public HorizontalAlignmentEx ActiveHorizontalAlignment // 
		{
			get
			{
				if (MetaColumn != null && HorizontalAlignment == HorizontalAlignmentEx.Default)
					return MetaColumn.HorizontalAlignment;
				else return HorizontalAlignment;
			}
		}

		/// <summary>
		/// Active Vertical Alignment - Get from Metacolumn if QueryColumn is default value
		/// else get non-default value from QueryColumn
		/// </summary>
		/// <returns></returns>

		public VerticalAlignmentEx ActiveVerticalAlignment 
		{
			get
			{
				if (MetaColumn != null && VerticalAlignment == VerticalAlignmentEx.Default)
					return MetaColumn.VerticalAlignment;
				else return VerticalAlignment;
			}
		}

		/// <summary>
		/// Default constructor
		/// </summary>

		public QueryColumn()
		{
			return;
		}

		/// <summary>
		/// Construct & set metacolumn
		/// </summary>
		/// <param name="mc"></param>

		public QueryColumn(MetaColumn mc)
		{
			MetaColumn = mc;
			return;
		}

		/// <summary>
		/// Memberwise clone
		/// </summary>
		/// <returns></returns>

		public QueryColumn Clone()
		{
			QueryColumn qc = (QueryColumn)this.MemberwiseClone();
			qc.Id = InstanceCount++; // assign unique id to clone
			return qc;
		}

		/// <summary>
		/// Get label from QueryColumn or from MetaColumn if no query column label 
		/// Include any associated aggreation attributes
		/// </summary>

		public string ActiveLabel 
		{
			get
			{
				return GetActiveLabel(includeAggregationType: false);
			}
		}

		/// <summary>
		/// Get label from QueryColumn or from MetaColumn if no query column label
		/// </summary>
		/// <param name="includeAggregationType"></param>
		/// <returns></returns>

		public string GetActiveLabel(
			bool includeAggregationType)
		{
			string label = "";
			if (!Lex.IsNullOrEmpty(Label)) label = Label;
			else if (MetaColumn != null)
			{
				MetaColumn mc = MetaColumn;
				if (!Lex.IsNullOrEmpty(mc.Label))
					label = mc.Label;
				else label = Lex.InternalNameToLabel(mc.Name);

				if (!Lex.IsNullOrEmpty(mc.Units) && !Lex.Contains(label, mc.Units))
				{ // add any units
					string unitsString = "(" + mc.Units + ")";
					label += " " + unitsString;
				}
			}
			int i1 = label.IndexOf("\t"); // if tab-separated just return the first part
			if (i1 >= 0) label = label.Substring(0, i1);

			if (Aggregation != null && Aggregation.RoleIsDefined && includeAggregationType &&  // If aggregated col add type of aggregation to label if not a grouping col
				QueryTable != null && QueryTable.AggregationEnabled)
			{
				if (!IsAggregationGroupBy) label += " " + Aggregation.TypeLabel;
			}

			return label;
		}

		/// <summary>
		/// Return true if content is Html and should be formatted as is
		/// </summary>

		public bool IsHtmlContent
		{
			get
			{
				bool isHtml = (DisplayFormat == ColumnFormatEnum.HtmlText || MetaColumn.DataType == MetaColumnType.Html || MetaColumn.Format == ColumnFormatEnum.HtmlText);
				return isHtml;
			}
		}


		/// <summary>
		/// Active display format
		/// </summary>

		public ColumnFormatEnum ActiveDisplayFormat
		{
			get
			{
				if (DisplayFormat != ColumnFormatEnum.Unknown) return DisplayFormat;
				else return MetaColumn.Format;
			}
		}

/// <summary>
/// Active number of decimals
/// </summary>

		public int ActiveDecimals
		{
			get
			{
				if (DisplayFormat != ColumnFormatEnum.Unknown) // if format is defined then use QC decimals
					return Decimals;
				else return MetaColumn.Decimals;
			}
		}

/// <summary>
/// Get name for col that is unique within the query
/// </summary>

		public string UniqueName
		{
			get
			{
				QueryTable qt = QueryTable;
				if (qt == null || qt.Query == null) return MetaColumn.Name;

				Query q = qt.Query;

				if (!q.DuplicateNamesAndLabelsMarked)
					q.MarkDuplicateNamesAndLabels();

				string name = MetaColumn.Name;
				if (qt.HasDuplicateNames)
					name = qt.Alias + "_" + name;

				return name;
			}
		}

/// <summary>
/// Get label for col that is unique within the query
/// </summary>

		public string UniqueLabel
		{
			get
			{
				QueryTable qt = QueryTable;
				if (qt == null || qt.Query == null) return MetaColumn.Name;

				Query q = qt.Query;

				if (!q.DuplicateNamesAndLabelsMarked)
					q.MarkDuplicateNamesAndLabels();

				string label = ActiveLabel;
				if (qt.HasDuplicateNames)
					label = qt.Alias + "." + label;

				return label;
			}
		}

		/// <summary>
		/// Get QueryColumn reference string in the form [Alias.]MetatableName.MetaColumn name
		/// </summary>
		/// <returns></returns>

		public string QcRefString
		{
			get
			{
				string s = "";

				if (MetaColumn == null) return "";

				if (QueryTable?.MetaTable == null) return "";

				if (Lex.IsDefined(QueryTable.Alias))
					s += QueryTable.Alias + ".";

				s += MetaTableDotMetaColumnName;

				return s;
			}
		}

		/// <summary>
		/// Get a QueryColumn for a query from a QC reference string
		/// </summary>
		/// <param name="q"></param>
		/// <param name="qcRef"></param>
		/// <returns></returns>

		public static QueryColumn GetQueryColumnFromRefString(
			Query q,
			string qcRef)
		{
			QueryTable qt = null;
			QueryColumn qc = null;
			string alias, mtName, mcName;
			

			if (Lex.IsUndefined(qcRef)) return null;

			string[] sa = qcRef.Split('.');

			if (sa.Length == 3)
			{
				sa = Lex.Split(qcRef, ".");
				alias = sa[0];
				mtName = sa[1];
				mcName = sa[2];

				qt = q.GetQueryTableByAlias(alias);
			}

			else if (sa.Length == 2)
			{
				mtName = sa[0];
				mcName = sa[1];
			}

			else return null;

			if (qt == null) qt = q.GetQueryTableByName(mtName);
			if (qt == null) return null;

			qc = qt.GetQueryColumnByName(mcName);
			if (qc != null) return qc;
			else return null;
		}
		
		/// <summary>
		/// Get MetatableName.MetaColumn name
		/// </summary>

		public string MetaTableDotMetaColumnName
		{
			get
			{
				if (MetaColumn == null) return "";
				else return MetaColumn.MetaTableDotMetaColumnName;
			}
		}

		public string MetaColumnName
		{
			get
			{
				if (MetaColumn != null) return MetaColumn.Name;
				else return "";
			}
		}


		public MetaColumnType DataType
		{
			get
			{
				if (MetaColumn == null) return MetaColumnType.Unknown;
				else return MetaColumn.DataType;
			}
		}

		/// <summary>
		/// Return true if underlying metacolumn is a key column
		/// </summary>

		public bool IsKey
		{
			get
			{
				if (MetaColumn == null) return false;
				else return MetaColumn.IsKey;
			}
		}

/// <summary>
/// Return true if column is selected, aggregated or sorted
/// </summary>

		public bool Is_Selected_or_GroupBy_or_Sorted
		{
			get
			{
				return Selected || IsAggregationGroupBy || SortOrder != 0;
			}
		}

		/// <summary>
		/// Return true if column is selected, has criteria, is aggregated or is sorted
		/// </summary>

		public bool Is_Selected_or_Criteria_or_GroupBy_or_Sorted
		{
			get
			{
				return Selected || !Lex.IsNullOrEmpty(Criteria) || IsAggregationGroupBy || SortOrder != 0;
			}
		}

#if false // not currently used, see HasNotExistsCriteria below

		/// <summary>
		/// Return true if row with a column value of null exists
		/// </summary>

		public bool HasIsNullCriteria
		{
			get
			{
				if (DebugMx.True) throw new NotImplementedException(); 

				if (Lex.Contains(Criteria, "is null"))
					return true;

				else return false;
			}
		}
#endif

		/// <summary>
		/// Return true if QueryColumn has "not exists" criteria
		/// This is actually represented as "is null" in mql but is executed as a separate
		/// subquery with input and output key lists
		/// </summary>

		public bool HasNotExistsCriteria
		{
			get
			{
				if (Lex.Contains(Criteria, "is null") && !Lex.Contains(Criteria, "not null"))
					return true;

				else return false;
			}
		}

		/// <summary>
		/// Return true if custom formatting exists for QueryColumn
		/// </summary>
		/// <returns></returns>

		public bool CustomFormattingExists
		{
			get
			{

				if (
					ActiveCondFormat != null ||
					Label != "" || // any formatting by user?
					DisplayFormat != ColumnFormatEnum.Unknown ||
					DisplayWidth >= 0 ||
					Decimals >= 0)
					return true;

				MetaColumn mc = MetaColumn;
				HorizontalAlignmentEx ha = ActiveHorizontalAlignment;
				VerticalAlignmentEx va = ActiveVerticalAlignment;

				if (ha != QueryColumn.DefaultHAlignment && mc != null && ha != mc.HorizontalAlignment)
					return true;

				if (va != QueryColumn.DefaultVAlignment && mc != null && va != mc.VerticalAlignment)
					return true;

				return false;
			}
		}

		/// <summary>
		/// Return the coloring style of any conditional formatting
		/// </summary>

		public CondFormatStyle CondFormatColoringStyle
		{
			get
			{
				if (ActiveCondFormat == null || ActiveCondFormat.Rules == null)
					return CondFormatStyle.Undefined;

				else return ActiveCondFormat.Rules.ColoringStyle;
			}
		}

		/// <summary>
		/// CopyCriteriaToKeyQueryCritera in current query  (Query.KeyCriteria has precedence over QC.Critera)
		/// </summary>

		public void CopyCriteriaToQueryKeyCritera()
		{
			if (Query == null) return;

			CopyCriteriaToQueryKeyCritera(Query); // use this query
			return;
		}

		/// CopyCriteriaToKeyQueryCritera  (Query.KeyCriteria has precedence over QC.Critera)

		public void CopyCriteriaToQueryKeyCritera(
			Query query)
		{
			AssertMx.IsNotNull(query, "Query");

			string criteria = Criteria;
			string colName = MetaColumnName + " "; // prepend metacolumn name if not done yet
			if (Lex.StartsWith(criteria, colName)) // remove col name
				criteria = criteria.Substring(colName.Length);
			query.KeyCriteria = criteria;

			query.KeyCriteriaDisplay = CriteriaDisplay;
			return;
		}

		/// <summary>
		/// CopyCriteriaFromKeyQueryCritera in current query - Query.KeyCriteria has precedence over QC.Critera
		/// </summary>

		public void CopyCriteriaFromQueryKeyCriteria()
		{
			if (Query == null) return;

			CopyCriteriaFromQueryKeyCriteria(Query);
			return;
		}

		/// <summary>
		/// CopyCriteriaFromKeyQueryCritera - Query.KeyCriteria has precedence over QC.Critera
		/// </summary>

		public void CopyCriteriaFromQueryKeyCriteria(
			Query query)
		{
			AssertMx.IsNotNull(query, "Query");

			Criteria = query.KeyCriteria;
			CriteriaDisplay = query.KeyCriteriaDisplay;

			if (Lex.IsUndefined(Criteria)) return; // undefined

			string colName = MetaColumnName + " "; // prepend metacolumn name if not done yet
			if (!Lex.StartsWith(Criteria, colName))
				Criteria = colName + Criteria;

			return;
		}

		/// <summary>
		/// Reset formatting to default values
		/// </summary>

		public void ResetFormatting()
		{
			QueryColumn qc0 = new QueryColumn();
			CondFormat = qc0.CondFormat;
			Label = qc0.Label;
			DisplayFormat = qc0.DisplayFormat;
			DisplayWidth = qc0.DisplayWidth;
			Decimals = qc0.Decimals;
			HorizontalAlignment = qc0.HorizontalAlignment;
			VerticalAlignment = qc0.VerticalAlignment;

			return;
		}

		/// <summary>
		/// Get CondFormat from QueryColumn or from MetaColumn if no query column label
		/// </summary>

		public CondFormat ActiveCondFormat
		{
			get
			{
				if (CondFormat != null) return CondFormat;
				else if (MetaColumn != null && MetaColumn.CondFormat != null) return MetaColumn.CondFormat;
				else return null;
			}
		}

		/// <summary>
		/// Check if QueryColumn cond format is the same as the associated MetaColumn cf
		/// </summary>
		/// <returns></returns>

		public bool CondFormatMatchesMetacolumnCf()
		{
			if (CondFormat == null || MetaColumn == null || MetaColumn.CondFormat == null) return false;

			string qcCf = CondFormat.Serialize();
			string mcCf = MetaColumn.CondFormat.Serialize();

			if (qcCf == mcCf) return true;
			else return false;
		}

		/// <summary>
		/// Get epsilon value for fuzzy numeric comparisons = .5 * 10**(-decimals)
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public double GetNumericValueEpsilon()
		{
			if (MetaColumn == null || !MetaColumn.IsDecimal) return 0;

			int decimals = MetaColumn.Decimals; // default to metacolumn decimals
			if (Decimals >= 0) decimals = Decimals; // use QC decimals if defined

			double epsilon = MobiusDataType.GetEpsilon(decimals);
			return epsilon;
		}

		/// <summary>
		/// MapSortOrderToImageIndex
		/// </summary>
		/// <param name="sortOrder"></param>
		/// <returns></returns>

		public static Bitmaps16x16Enum MapSortOrderToImageIndex(
			int sortOrder)
		{
			if (sortOrder == 0) return Bitmaps16x16Enum.None; // set sort image
			else if (sortOrder == 1) return Bitmaps16x16Enum.SortAsc1;
			else if (sortOrder == 2) return Bitmaps16x16Enum.SortAsc2;
			else if (sortOrder == 3) return Bitmaps16x16Enum.SortAsc3;
			else if (sortOrder == -1) return Bitmaps16x16Enum.SortDesc1;
			else if (sortOrder == -2) return Bitmaps16x16Enum.SortDesc2;
			else if (sortOrder == -3) return Bitmaps16x16Enum.SortDesc3;
			else return Bitmaps16x16Enum.None;
		}

/// <summary>
/// AggregationRole
/// </summary>

		public AggregationRole AggregationRole
		{
			get
			{
				if (Aggregation != null) return Aggregation.Role;
				else return AggregationRole.Undefined;
			}

			set
			{
				GetAggregation().Role = value;
			}

		}

		public SummaryTypeEnum SummaryType = SummaryTypeEnum.Undefined;

		/// <summary>
		/// AggregationSummaryType
		/// </summary>

		public SummaryTypeEnum AggregationSummaryType
		{
			get
			{
				if (Aggregation != null) return Aggregation.SummaryType;
				else return SummaryTypeEnum.Undefined;
			}

			set
			{
				GetAggregation().SummaryType = value;
			}

		}

/// <summary>
/// AggregationGroupingType
/// </summary>

		public GroupingTypeEnum AggregationGroupingType
		{
			get
			{
				if (Aggregation != null) return Aggregation.GroupingType;
				else return GroupingTypeEnum.EqualValues;
			}

			set
			{
				GetAggregation().GroupingType = value;
			}

		}

		/// <summary>
		/// AggregationNumericIntervalSize
		/// </summary>

		public Decimal AggregationNumericIntervalSize
		{
			get
			{
				if (Aggregation != null) return Aggregation.NumericIntervalSize;
				else return -1;
			}

			set
			{
				GetAggregation().NumericIntervalSize = value;
			}

		}

		/// <summary>
		/// Set Aggregation by AggregationTypeDetail name
		/// </summary>

		public AggregationTypeDetail SetAggregation(string typeName)
		{
			if (Lex.IsUndefined(typeName) || Lex.Eq(typeName, "none")) // clear?
			{
				Aggregation = null;
				return null;
			}

			AggregationTypeDetail atd = AggregationTypeDetail.GetByTypeName(typeName);
			if (Aggregation != null && Lex.Eq(atd.TypeName, Aggregation.TypeName)) return atd; // same as before

			Aggregation = new AggregationDef(typeName);

			return atd;
		}

		/// <summary>
		/// Get AggregationType Name
		/// </summary>

		public string GetAggregationTypeName()
		{
			if (Aggregation == null) return "";
			else return Aggregation.TypeName;
		}

/// <summary>
/// Get image index
/// </summary>
/// <returns></returns>
		public Bitmaps16x16Enum GetAggregationImage()
		{
			if (Aggregation == null) return Bitmaps16x16Enum.None;
			else return Aggregation.TypeImageIndex;
		}

		/// <summary>
		/// Get AggregationType Label
		/// </summary>

		public string GetAggregationTypeLabel()
		{
			if (Aggregation == null) return "";

			string label = Aggregation.TypeLabel;

			return label;
		}

/// <summary>
/// Get AggregationDef object allocating if null
/// </summary>
/// <returns></returns>

		public AggregationDef GetAggregation()
		{
			if (Aggregation == null) Aggregation = new AggregationDef();
			return Aggregation;
		}

		/// <summary>
		/// Return true if column value is an aggregated value
		/// </summary>

		public bool IsAggregated
		{
			get
			{
				//if (QueryTable == null || !QueryTable.AggregationEnabled) return false;

				return (Aggregation != null && Aggregation.RoleIsDefined);
			}
		}

		/// <summary>
		/// Return true if column is a GroupBy aggregation column and aggregation is turned on for the corresponding table
		/// </summary>

		public bool IsAggregationGroupBy
		{
			get
			{
				//if (QueryTable == null || !QueryTable.AggregationEnabled) return false;

				return (Aggregation != null && Aggregation.IsGroupingType);
			}
		}

		/// <summary>
		/// Return true if this column is a potential number qualifer column
		/// (i.e. a text column followed by a numeric column)
		/// </summary>

		public bool IsPotentialNumberQualifier
		{
			get
			{
				if (MetaColumn.DataType != MetaColumnType.String) return false;

				List<QueryColumn> qcList = QueryTable.QueryColumns;
				for (int qci = 0; qci < qcList.Count; qci++)
				{
					QueryColumn qc2 = QueryTable.QueryColumns[qci];
					if (MetaColumn.Name == qc2.MetaColumn.Name)
					{
						if (qci < qcList.Count - 1 && qcList[qci + 1].MetaColumn.IsNumeric)
							return true;
						else return false;
					}
				}

				return false;
			}
		}

	} // QueryColumn

/// <summary>
/// Format string for Chemical structure formatting
/// </summary>
	public class StructureDisplayFormat
	{
		public bool Highlight = false; // highlight differences via coloring of atoms/bonds
		public bool Align = false; // align orientation of matching structures

		public string Serialize()
		{
			string txt = "Highlight=" + Highlight + ";Align=" + Align;
			return txt;
		}

		public static StructureDisplayFormat Deserialize(string serializedForm)
		{
			StructureDisplayFormat sdf = new StructureDisplayFormat();
			if (Lex.IsUndefined(serializedForm)) return sdf;

			sdf.Highlight = Lex.Contains(serializedForm, "Highlight=true");
			sdf.Align = Lex.Contains(serializedForm, "Align=true");
			return sdf;
		}

	}


	/// <summary>
	/// Interactive filter types
	/// </summary>

	public enum FilterType
		{
			Unknown = 0, // undefined
			BasicCriteria = 1, // simple text entry for basic filtering operations with 0 - 2 values  
			CheckBoxList = 2, // select from a list of check boxes
			ItemSlider = 3, // single item from a trackbar
			RangeSlider = 4, // range of values from an range trackbar
			StructureSearch = 5 // substructure search

			//			RadioButtonListFilter = xxx, // select a single entry from a list of radio buttons
			//			ListBoxFilter = yyy, // select from a scrolling list of values
			//			Custom = zzz // select from "Custom" dialog
		}

	/// <summary>
	/// Specifies the horizontal alignment of an object or text
	/// </summary>
	
	public enum HorizontalAlignmentEx
	{
		Default   = 0,
		Left      = 1,
		Center    = 2,
		Right     = 3,
		Justified = 4
	}

	/// <summary>
	/// Specifies the horizontal alignment of an object or text
	/// </summary>

	public enum VerticalAlignmentEx
	{
		Default = 0,
		Top     = 1,
		Middle  = 2,
		Bottom  = 3
	}


}
