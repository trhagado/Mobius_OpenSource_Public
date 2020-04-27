using Mobius.Data;
using Mobius.ComOps;
using Mobius.ServiceFacade;

using DevExpress.XtraGrid.Columns;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

	/// <summary>
	/// This class contains formatting information on each results field.
	/// Positions & widths are in milliinches
	/// </summary>
	
	public class ResultsField
	{
		public QueryManager QueryManager { get { return ResultsTable.ResultsFormat.QueryManager; } }
		public MetaColumn MetaColumn;
		public QueryColumn QueryColumn;
		public ResultsTable ResultsTable;
		public MobiusDataType Header;
		public List<MobiusDataType> CondFmtHeaders; // conditional formatting headers
		public int FieldPosition; // position in segment for field
		public int FieldX; // x location of field
		public int FieldWidth; // width for field in milliinches
		public int ColumnX; // x location of column containing field
		public int ColumnWidth; // width of column including column divider in milliinches
		public int SortOrder = 0; // sort order, pos for ascending, neg for descending, modified when sheet sorted
		public GridColumn GridColumn; // grid column assigned to this field
		public int VoPosition; // position in Query Engine value object buffer
		public bool Merge; // if true merge with previous field
		public string MergeLabel = ""; // label to prefix values with for merged columns
		public List<MergedField> MergedFieldList; // list of fields that are merged with this field
		public QnfEnum QualifiedNumberSplit = QnfEnum.Combined; // splitting for qualified number
			//QnfEnum.Split | QnfEnum.NumericValue | QnfEnum.Qualifier | QnfEnum.TextValue; // debug
		public StringDictionary StringDict; // store string values from a controlled vocabulary field here

		private ColumnStatistics _stats; // statistics on column (see GetStats)

/// <summary>
/// Constructor
/// </summary>

		public ResultsField()
		{
			return;
		}

		/// <summary>
		/// Get statistics for column, calculating if necessary
		/// </summary>
		/// <returns></returns>

		public ColumnStatistics GetStats() // statistics on column
		{
			if (ResultsTable == null ||
			 ResultsTable.ResultsFormat == null ||
			 ResultsTable.ResultsFormat.QueryManager == null ||
			 ResultsTable.ResultsFormat.QueryManager.DataTableManager == null)
				throw new Exception("DataTableManager not defined");

			if (_stats == null)
			{
				ColumnInfo ci = ResultsTable.ResultsFormat.GetColumnInfo(QueryColumn);
				_stats = QueryManager.DataTableManager.CalcColumnStatistics(ci);

				if (_stats.DistinctValueDict.Count == 0) _stats = _stats; // debug
			}

			// Convert list to a csv string
			//StringBuilder sb = new StringBuilder();
			//foreach (MobiusDataType mdt in _stats.DistinctValueList)
			//{
			//  string s = mdt.ToString();
			//  sb.Append(s);
			//  sb.Append(", ");
			//}
			//string slist = sb.ToString();

			return _stats;
		}

		/// <summary>
		/// Return true if rfld is the basic value of a split or unsplit field
		/// </summary>

		public bool IsMainValue
		{
			get
			{
				if (MetaColumn == null) return true;
				if (MetaColumn.DataType != MetaColumnType.QualifiedNo) return true;
				if (MetaColumn.DataType != MetaColumnType.QualifiedNo) return true;
				if ((QualifiedNumberSplit & QnfEnum.Combined) != 0) return true;
				if ((QualifiedNumberSplit & QnfEnum.NumericValue) != 0) return true;

				return false;
			}
		}

		/// <summary>
		/// Clone
		/// </summary>
		/// <returns></returns>

		public ResultsField Clone()
		{
			ResultsField rf = (ResultsField)this.MemberwiseClone();
			return rf;
		}

	} // ResultsField

	/// <summary>
	/// Statistics on a data column
	/// </summary>

	public class ColumnStatistics
	{
		public MobiusDataType MinValue; // minimum value in current dataset
		public MobiusDataType MaxValue; // max value in current dataset
		public bool NullsExist = false; // one or more null values exists in the dataset
		public bool SingleRowPerKey = false; // if true then there is at most a single row per key

		public List<MobiusDataType> DistinctValueList = // list of distinct values in text form ordered by underlying native data type
			new List<MobiusDataType>();

		public Dictionary<string, DistinctValueAndPosition> DistinctValueDict = // dictionary of distinct values keyed on uppercase text form
			new Dictionary<string, DistinctValueAndPosition>();

		public int Cardinality // number of distinct values
		{
			get { return DistinctValueList != null ? DistinctValueList.Count : 0; }
		}

	} // ColumnStatistics

	public class DistinctValueAndPosition
	{
		public MobiusDataType Value; // sample distinct value as a MobiusDataType
		public int Ordinal; // position of value within set of distinct values

	} // DistinctValueAndPosition

	/// <summary>
	/// This class contains information on merged fields.
	/// </summary>
	public class MergedField
	{
		public int TableIndex; // index of merged table in table list
		public QueryColumn QueryColumn; // associated query column
		public MetaTable MetaTable; // metatable containing field
		public MetaColumn MetaColumn; // associated metacolumn
		public string Label; // label to prefix values with

		public MergedField()
		{
		}

	} // MergedFieldList

}
