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
/// Format information for a query
/// </summary>

	public class ResultsFormat
	{
		internal int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		public QueryManager QueryManager; 
		public Query Query {
			get { return QueryManager == null ? null : QueryManager.Query; }
			set { if (QueryManager == null) return; QueryManager.Query = value; }
		}
		public ResultsFormatter Formatter { 
			get { return QueryManager == null ? null : QueryManager.ResultsFormatter; } } // associated results formatter
		public MoleculeGridControl MolGrid { 
			get { return QueryManager == null ? null : QueryManager.MoleculeGrid; } } // target molecule grid if grid output

		public int QueryId; // id of query (serialized)
		public OutputDest OutputDestination = OutputDest.Unknown;
		public OutputDest SecondaryOutput = OutputDest.Unknown; // e.g. Printer or PrintPreview
		public OutputFormContext OutputFormContext = OutputFormContext.Session; // Type of (Windows) form in which the results will be displayed
		public Control OutputContainerControl = null; // WinForm control that will contain the results (e.g. QueryResultsControl)
		public ExitingQueryResultsDelegate CustomExitingQueryResultsCallback = null; // override method to call when output container is closed
		public bool SuppressNoDataMessage = false; // there are time we do not want to show the popu message, we may be looking elsewhere for data

		public bool RunInBackground = false; // run query in background mode

		public bool TableOriented = false; // data for a compound may be split
		public bool CompoundOriented = false; // data for a compound is held together

		public bool UseBandedGridView = true; // display data in BandedGridView format 
		public bool UseLayoutView { get { return !UseBandedGridView; } set { UseBandedGridView = !value; } } 

		public bool RowFraming; // framing between rows
		public bool CombineStructureWithCompoundId; // combine in same cell
		public ResultsField CombinedStructureField;
		public ResultsField FirstTableStructureField;
		public bool HighlightStructureMatches;
		public ExportStructureFormat ExportStructureFormat; // how to output structures
		public ColumnNameFormat ColumnNameFormat = ColumnNameFormat.Normal; // format of table labels

		public MoleculeTransformationFlags StructureFlags; // additional structure output flags
		public bool FixedHeightStructures = false; // if true use constant structure box height

		public int PageWidth; // milliinches, unscaled
		public int PageHeight; // milliinches, unscaled
		public Orientation PageOrientation = Orientation.Vertical;
		public PageMargins PageMargins; // millinches, unscaled
		public int DisplayPageWidth;
		public int DisplayPageHeight;
		public int HtmlScreensPerPage = SS.I.HtmlScreensPerPage; // physical screen pages per html page
		public int HtmlPageWidth = SS.I.HtmlPageWidth; // set to a nonzero value to force an HTML page width
		public int LogicalPixelsX; // screen pixels per "logical" inch
		public int LogicalPixelsY;
		public int PageScale = 100; // in percent units
		public int PageLines; // number of lines that fit on page

		public int LineHeight = 111; // text line height in milliinches 
		public int CharWidth = 74; // "standard" character width in milliinches (may be redefined at runtime)
		public int InterColumnSpacing; // milliinch between repeated cols

		public int TotalWidth = 0; // total width of report in milliinches if formatted in a single section

		public String FontName; // e.g. Arial 
		public int FontSize; // in points

		public string Title = ""; // title for results
		public bool ShowCondFormatLabels = true; // override for displaying labels
		public bool ShowConditionalFormatting = true; // if true display any conditional formatting for results
		public int MaxCondFormattingRules = 0; // max # of rules for any conditionally formatted field
		public bool ShowHeaderStyling = true; // show header background color and links if true
		public int HeaderLines = 2; // number of lines in header
		public int RepeatCount = -1; // number of times to repeat data across page (-1 = undefined)
		public QnfEnum QualifiedNumberSplit = QnfEnum.Combined; // qualified number formatting default for export to excel, spotfire, text files
			//QnfEnum.Split | QnfEnum.NumericValue | QnfEnum.Qualifier | QnfEnum.TextValue; // debug
		public bool AllowExtraLengthFieldNames = false;

		public ExportFileFormat ExportFileFormat; // output file format Csv or Tsv
		public string OutputFileName = ""; // output file name
		public string OutputFileName2 = ""; // intermediate output file name
		public string OutputFolder = "";
		public bool DuplicateKeyTableValues; // duplicate key table fields in each row of export output

		// Spotfire

		public bool IncludeDataTypes = false; // if true include spotfire data types in 2nd line of file
		public SpotfireOpenModeEnum OpenMode = SpotfireOpenModeEnum.None; // Spotfire open mode

		// Spotfire DecisionSite (obsolete)

		public string TemplateFileName = ""; // template file (name on client)
		public string VisualizationTitle = ""; // title for first visualization
		public string ImageFileName = ""; // background image file for first visualization (name on client)
		public Rectangle ImageBounds; // image boundaries for first visualization
		public bool ViewStructures = false; // if true start DecisionSite structure viewer

		// Other
		
		public QueryEngine ParentQe; // if non-null then use results from existing query engine instance
		public int VoLength; // length of vo returned by queryengine
		public List<QueryColumn> VoIndexToQueryColumn; // map of vo index to QueryColumn
		public List<ResultsField> VoIndexToResultsField; // map of vo index to ResultsField
		public List<ResultsField> GridColumnIndexToResultsField; // map of overall position of results field in the list of grid columns
		public string ErrorMessage;

		public GraphicsMx GraphicsContext = new GraphicsMx();

		public List<ResultsTable> Tables = new List<ResultsTable>(); // ResultsTables array
		public List<ResultsTable> TablesExtra = new List<ResultsTable>(); // extra tables added by pivoting
		public List<ResultsSegment> Segments = new List<ResultsSegment>(); // ResultsSegments array
		public List<string> TitleBuffer = new List<string>(); // formatted and buffered title

		public int RowAttributesVoPos // position in Vo for row attributes
		{
			get
			{
				if (QueryManager != null && QueryManager.DataTableManager != null)
					return QueryManager.DataTableManager.RowAttributesVoPos;
				else return 0;
			}
		}

		public int CheckMarkVoPos // position in Vo for check mark
		{
			get
			{
				if (QueryManager != null && QueryManager.DataTableManager != null)
					return QueryManager.DataTableManager.CheckMarkVoPos;
				else return 1;
			}
		}

		public int KeyValueVoPos // position in Vo for the common key value
		{
			get
			{
				if (QueryManager != null && QueryManager.DataTableManager != null)
					return QueryManager.DataTableManager.KeyValueVoPos;
				else return 2; // default position
			}
		}

/// <summary>
/// Return the first output file name which may be a temporary file
/// </summary>

		public string FirstOutputFileName
		{
			get
			{
				if (OutputFileName2 != "") return OutputFileName2;
				else return OutputFileName;
			}
		}

		// OutputFormContext type abbreviations
		public bool SessionOutputFormContext => OutputFormContext == OutputFormContext.Session;
		public bool ToolOutputFormContext => OutputFormContext == OutputFormContext.Tool;
		public bool PopupOutputFormContext => OutputFormContext == OutputFormContext.Popup;
		public bool NotPopupOutputFormContext => OutputFormContext != OutputFormContext.Popup;

		/// <summary>
		/// Constructor
		/// </summary>

		public ResultsFormat()
		{
			return;
		}

		/// <summary>
		/// Construct with supplied OutputDest
		/// </summary>

		public ResultsFormat(OutputDest outputDest)
		{
			OutputDestination = outputDest;
			return;
		}

		/// <summary>
		/// Construct with supplied QueryManager & dest
		/// </summary>

		public ResultsFormat(QueryManager qm, OutputDest outputDest)
		{
			QueryManager = qm;
			OutputDestination = outputDest;
			qm.ResultsFormat = this;
			return;
		}

		public bool Search { get { return OutputDestination == OutputDest.Search; } }
		public bool Grid { get { return OutputDestination == OutputDest.WinForms; } }
		public bool Html { get { return OutputDestination == OutputDest.Html; } }
		public bool Console { get { return OutputDestination == OutputDest.Console; } }
		public bool Excel { get { return OutputDestination == OutputDest.Excel; } }
		public bool Word { get { return OutputDestination == OutputDest.Word; } }
		public bool TextFile { get { return OutputDestination == OutputDest.TextFile; } }
		public bool Spotfire { get { return OutputDestination == OutputDest.Spotfire; } }
		public bool SdFile { get { return OutputDestination == OutputDest.SdFile; } }

		public Rectangle PageBounds 
		{ // return rectangle with full page rectangle including margins
			get 
			{
				return new Rectangle(0,0,
					PageMargins.Left + PageWidth + PageMargins.Right,
					PageMargins.Top + PageHeight + PageMargins.Bottom);
			}
		}

/// <summary>
/// Get column count for results format (grid only)
/// Checks last column to get count
/// </summary>
/// <returns></returns>

		public int GridColumnCount ()
		{
			for (int ti = this.Tables.Count-1; ti>=0; ti--) // scan backwards til we find a table with columns
			{
				ResultsTable rt = this.Tables[ti];
				if (rt.Fields.Count==0) continue; // table may have zero cols
				ResultsField rfld = rt.Fields[rt.Fields.Count - 1];
				int colcnt = rfld.FieldPosition + 1; // number of data-containing columns in grid
				if (Query.ShowGridCheckBoxes) colcnt++;
				return colcnt;
			}

			return 0; // no columns (shouldn't happen)
		}

/// <summary>
/// Get column count for results format 
/// Explicitly sums counts for tables
/// </summary>
/// <returns></returns>

		public int ColumnCount ()
		{
			int colcnt = 0;
			for (int ti=0; ti<Tables.Count; ti++)
			{
				ResultsTable rt = this.Tables[ti];
				colcnt += rt.Fields.Count;
			}

			return colcnt; 
		}

/// <summary>
/// Get a ResultsField associated with a QueryColumn
/// </summary>
/// <param name="qc"></param>
/// <returns></returns>

		public ResultsField GetResultsField (
			QueryColumn qc)
		{
			if (qc == null) return null;

			foreach (ResultsTable rt in Tables)
			{
				if (Lex.Ne(qc.MetaColumn.MetaTable.Name,rt.MetaTable.Name))
					continue;

				foreach (ResultsField rfld in rt.Fields)
				{
					if (Lex.Eq(qc.MetaColumn.Name,rfld.MetaColumn.Name))
						return rfld;
				}
			}

			return null;
		}

/// <summary>
/// Find any results column corresponding to a metacolumn
/// </summary>
/// <param name="mc"></param>
/// <returns></returns>

		public ResultsField GetResultsField(
		MetaColumn mc)
		{
			if (mc == null) return null;
			
			foreach (ResultsTable rt in Tables)
			{
				if (Lex.Ne(mc.MetaTable.Name, rt.MetaTable.Name))
					continue;

				foreach (ResultsField rfld in rt.Fields)
				{
					if (Lex.Eq(mc.Name, rfld.MetaColumn.Name))
						return rfld;
				}
			}

			return null;
		}


		/// <summary>
		/// return number of columns that qualified numbers are split into
		/// </summary>
		/// <returns></returns>

		public int QnSplitCount()
		{
			if (!Excel && !TextFile) return 1;
			if (QualifiedNumberSplit == QnfEnum.Combined) return 1;

			int count = 0;
			if ((QualifiedNumberSplit & QnfEnum.Qualifier) != 0) count++;
			if ((QualifiedNumberSplit & QnfEnum.NumericValue) != 0) count++;
			if ((QualifiedNumberSplit & QnfEnum.StdDev) != 0) count++;
			if ((QualifiedNumberSplit & QnfEnum.StdErr) != 0) count++;
			if ((QualifiedNumberSplit & QnfEnum.NValue) != 0) count++;
			if ((QualifiedNumberSplit & QnfEnum.NValueTested) != 0) count++;
			if ((QualifiedNumberSplit & QnfEnum.TextValue) != 0) count++;

			return count;
		}

		/// <summary>
		/// Get ColumnInfo given a QueryColumn
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		public ColumnInfo GetColumnInfo(QueryColumn qc)
		{
			return GetColumnInfo(QueryManager, qc);
		}

		/// <summary>
		/// Get ColumnInfo given a QueryColumn
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		public static ColumnInfo GetColumnInfo(QueryManager qm, QueryColumn qc)
		{
			MetaTable mt;
			MetaColumn mc;
			ResultsTable rt;
			ResultsField rfld;
			int ti, fi;

			ResultsFormat rf = qm.ResultsFormat;

			ColumnInfo ci = new ColumnInfo();
			ci.Qt = qc.QueryTable;
			ci.Qc = qc;
			ci.Mt = qc.MetaColumn.MetaTable;
			ci.Mc = qc.MetaColumn;

			string cName = DataTableManager.DataColName(qc);

			for (ti = 0; ti < rf.Tables.Count; ti++)
			{
				rt = rf.Tables[ti];
				if (Lex.Ne(rt.QueryTable.MetaTable.Name, ci.Qt.MetaTable.Name)) continue;
				if (Lex.Ne(rt.QueryTable.Alias, ci.Qt.Alias)) continue; // alias must match as well
				ci.Rt = rt;
				ci.TableIndex = ti;

				for (fi = 0; fi < rt.Fields.Count; fi++)
				{
					rfld = rt.Fields[fi];
					if (Lex.Ne(rfld.QueryColumn.MetaColumn.Name, qc.MetaColumn.Name)) continue;
					ci.Rfld = rfld;

					int i1 = rfld.VoPosition; // col position in DataTable
					ci.DataColIndex = i1;
					ci.DataColumn = qm.DataTable.Columns[i1];

					if (qm.MoleculeGrid != null && qm.MoleculeGrid.DataTableToGridColumnMap != null) // position in grid if grid is defined
					{
						ci.GridColAbsoluteIndex = qm.MoleculeGrid.DataTableToGridColumnMap[ci.DataColIndex]; // reduce by 1 since grid doesn't have dataset status column
						ci.GridColumn = qm.MoleculeGrid.V.Columns[ci.GridColAbsoluteIndex];
					}

					if (Lex.Ne(ci.DataColumn.ColumnName, cName))
						throw new Exception("Data column name " + ci.DataColumn.ColumnName +
							" doesn't match ResultsField column name " + cName);

					return ci;
				}

			}

			throw new Exception("Column not found: " + cName);
		}

/// <summary>
/// Clear the sort order values
/// </summary>

		public void ClearSortOrder()
		{
			foreach (ResultsTable rt in Tables)
			{
				foreach (ResultsField rf in rt.Fields)
				{
					rf.SortOrder = 0;
				}
			}
		}

		/// <summary>
		/// Serialize input ResultsFormat fields into XML
		/// </summary>
		/// <returns></returns>

		public string Serialize()
		{
			XmlMemoryStreamTextWriter mstw = new XmlMemoryStreamTextWriter();
			Serialize(mstw.Writer);
			return mstw.GetXmlAndClose();
		}

		public void Serialize(
			XmlTextWriter tw)
		{
			tw.WriteStartElement("ResultsFormat");

			if (Query != null && Query.UserObject != null) // write id from query if query exists
				tw.WriteAttributeString("QueryId", Query.UserObject.Id.ToString());
			else if (QueryId > 0) tw.WriteAttributeString("QueryId", QueryId.ToString()); // otherwise write any defined id

			tw.WriteAttributeString("OutputDestination", OutputDestination.ToString());
			tw.WriteAttributeString("AllowExtraLengthFieldNames", AllowExtraLengthFieldNames.ToString());
			tw.WriteAttributeString("RunInBackground", RunInBackground.ToString());
			tw.WriteAttributeString("TableOriented", TableOriented.ToString());
			tw.WriteAttributeString("CompoundOriented", CompoundOriented.ToString());
			tw.WriteAttributeString("RowFraming", RowFraming.ToString());
			tw.WriteAttributeString("CombineStructureWithCompoundId", CombineStructureWithCompoundId.ToString());
			tw.WriteAttributeString("DuplicateKeyValues", DuplicateKeyTableValues.ToString());
			tw.WriteAttributeString("HighlightStructureMatches", HighlightStructureMatches.ToString());
			tw.WriteAttributeString("ExportStructureFormat", ExportStructureFormat.ToString());
			tw.WriteAttributeString("StructureFlags", ((int)StructureFlags).ToString());
			tw.WriteAttributeString("FixedHeightStructures", FixedHeightStructures.ToString());

			tw.WriteAttributeString("PageOrientation", PageOrientation.ToString());
			tw.WriteAttributeString("PageWidth", PageWidth.ToString());
			tw.WriteAttributeString("PageHeight", PageHeight.ToString());
			if (PageMargins != null)
			 tw.WriteAttributeString("PageMargins", PageMargins.Serialize());

			tw.WriteAttributeString("Title", Title);
			tw.WriteAttributeString("ShowConditionalFormatting", ShowConditionalFormatting.ToString());
			tw.WriteAttributeString("ShowHeaderStyling", ShowHeaderStyling.ToString());
			tw.WriteAttributeString("HeaderLines", HeaderLines.ToString());
			tw.WriteAttributeString("TableLabelFormat", ColumnNameFormat.ToString());
			tw.WriteAttributeString("RepeatCount", RepeatCount.ToString());
			tw.WriteAttributeString("QualifiedNumberSplit", ((int)QualifiedNumberSplit).ToString());
			tw.WriteAttributeString("ExportFileFormat", ExportFileFormat.ToString());
			tw.WriteAttributeString("OutputFileName", OutputFileName);
			tw.WriteAttributeString("OutputFileName2", OutputFileName);
			tw.WriteAttributeString("IncludeDataTypes", IncludeDataTypes.ToString());
			tw.WriteAttributeString("OpenMode", OpenMode.ToString());
			tw.WriteAttributeString("ViewStructures", ViewStructures.ToString());

// Spotfire Decision Site (old)

			tw.WriteAttributeString("TemplateFileName", TemplateFileName);
			tw.WriteAttributeString("VisualizationTitle", VisualizationTitle);
			tw.WriteAttributeString("ImageFileName", ImageFileName);
			if (ImageBounds != null)
				tw.WriteAttributeString("ImageBounds", ImageBounds.ToString());

			tw.WriteEndElement(); // end of ResultFormat

			return;
		}

		/// <summary>
		/// Deserialize XML into a ResultsFormat object
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>

		public static ResultsFormat Deserialize(
			string serializedForm)
		{
			XmlMemoryStreamTextReader mstr = new XmlMemoryStreamTextReader(serializedForm);

			XmlTextReader tr = mstr.Reader;
			tr.Read(); // get CondFormatRules element
			tr.MoveToContent();
			if (!Lex.Eq(tr.Name, "ResultsFormat"))
				throw new Exception("ResultsFormat.Deserialize - No \"ResultsFormat\" element");

			ResultsFormat rf = Deserialize(mstr.Reader);
			mstr.Close();
			return rf;
		}

		public static ResultsFormat Deserialize(
			XmlTextReader tr)
		{
			string txt = null;
			int i1 = -1;

			ResultsFormat rf = new ResultsFormat();

			if (XmlUtil.GetStringAttribute(tr, "OutputDestination", ref txt))
				EnumUtil.TryParse(txt, out rf.OutputDestination);

			XmlUtil.GetIntAttribute(tr, "QueryId", ref rf.QueryId);
			XmlUtil.GetBoolAttribute(tr, "AllowExtraLengthFieldNames", ref rf.AllowExtraLengthFieldNames);
			XmlUtil.GetBoolAttribute(tr, "RunInBackground", ref rf.RunInBackground);
			XmlUtil.GetBoolAttribute(tr, "TableOriented", ref rf.TableOriented);
			XmlUtil.GetBoolAttribute(tr, "CompoundOriented", ref rf.CompoundOriented);
			XmlUtil.GetBoolAttribute(tr, "RowFraming", ref rf.RowFraming);
			XmlUtil.GetBoolAttribute(tr, "CombineStructureWithCompoundId", ref rf.CombineStructureWithCompoundId);
			XmlUtil.GetBoolAttribute(tr, "DuplicateKeyValues", ref rf.DuplicateKeyTableValues);
			XmlUtil.GetBoolAttribute(tr, "HighlightStructureMatches", ref rf.HighlightStructureMatches);
			if (XmlUtil.GetStringAttribute(tr, "ExportStructureFormat", ref txt))
				EnumUtil.TryParse(txt, out rf.ExportStructureFormat);

			if (XmlUtil.GetIntAttribute(tr, "StructureFlags", ref i1))
				rf.StructureFlags = (MoleculeTransformationFlags)i1;
			XmlUtil.GetBoolAttribute(tr, "FixedHeightStructures", ref rf.FixedHeightStructures);

			if (XmlUtil.GetStringAttribute(tr, "PageOrientation", ref txt))
				EnumUtil.TryParse(txt, out rf.PageOrientation);

			XmlUtil.GetIntAttribute(tr, "PageWidth", ref rf.PageWidth);
			XmlUtil.GetIntAttribute(tr, "PageHeight", ref rf.PageHeight);
			if (XmlUtil.GetStringAttribute(tr, "PageMargins", ref txt))
				rf.PageMargins = PageMargins.Deserialize(txt);

			XmlUtil.GetStringAttribute(tr, "Title", ref rf.Title);
			XmlUtil.GetBoolAttribute(tr, "ShowConditionalFormatting", ref rf.ShowConditionalFormatting);
			XmlUtil.GetBoolAttribute(tr, "ShowHeaderStyling", ref rf.ShowHeaderStyling);
			XmlUtil.GetIntAttribute(tr, "HeaderLines", ref rf.HeaderLines);

			if (XmlUtil.GetStringAttribute(tr, "TableLabelFormat", ref txt))
				EnumUtil.TryParse(txt, out rf.ColumnNameFormat);

			XmlUtil.GetIntAttribute(tr, "RepeatCount", ref rf.RepeatCount);
			if (XmlUtil.GetIntAttribute(tr, "QualifiedNumberSplit", ref i1))
				rf.QualifiedNumberSplit = (QnfEnum)i1;

			if (XmlUtil.GetStringAttribute(tr, "ExportFileFormat", ref txt))
				EnumUtil.TryParse(txt, out rf.ExportFileFormat);

			XmlUtil.GetStringAttribute(tr, "ClientOutputFileName", ref rf.OutputFileName); // Mobius 2.x name of client file
			XmlUtil.GetStringAttribute(tr, "OutputFileName", ref rf.OutputFileName); // client file name
			XmlUtil.GetStringAttribute(tr, "OutputFileName2", ref rf.OutputFileName2);
			XmlUtil.GetBoolAttribute(tr, "IncludeDataTypes", ref rf.IncludeDataTypes);

			bool a1 = XmlUtil.GetStringAttribute(tr, "OpenMode", ref txt);
			if (a1)	EnumUtil.TryParse(txt, out rf.OpenMode);

			XmlUtil.GetBoolAttribute(tr, "ViewStructures", ref rf.ViewStructures);

			// SpotfireParms (DecisionSite)

			XmlUtil.GetStringAttribute(tr, "TemplateFileName", ref rf.TemplateFileName);
			XmlUtil.GetStringAttribute(tr, "VisualizationTitle", ref rf.VisualizationTitle);
			XmlUtil.GetStringAttribute(tr, "ImageFileName", ref rf.ImageFileName);
			XmlUtil.GetRectAttribute(tr, "ImageBounds", ref rf.ImageBounds);

			return rf;
		}

/// <summary>
/// Get stats for the specified QueryColumn
/// </summary>
/// <param name="qc"></param>
/// <returns></returns>

		internal ColumnStatistics GetStats(QueryColumn qc)
		{
			MetaColumn mc = qc.MetaColumn;
			ColumnInfo colInfo = GetColumnInfo(qc);
			ColumnStatistics stats = colInfo.Rfld.GetStats();
			return stats;
		}

		/// <summary>
		/// Calc totalwidth for all cols in milliinches
		/// </summary>
		/// <returns></returns>

		public int CalcTotalWidth()
		{
			ResultsTable rt;
			ResultsField rf;
			ResultsField rfld;
			int ScrollBarWidth = 22;
			int ti, fi;

			int totalWidth = 0;
			for (ti = 0; ti < Tables.Count; ti++)
			{
				rt = Tables[ti];
				for (fi = 0; fi < rt.Fields.Count; fi++)
				{
					rfld = rt.Fields[fi];
					totalWidth += rfld.ColumnWidth;

				}
			}

			return totalWidth;
		}


		/// <summary>
		/// Copy ResultsFormat members to ExportParms
		/// </summary>
		/// <returns></returns>

		public ExportParms CopyToExportParms()
		{
			ExportParms ep = new ExportParms();

			ep.QueryId = QueryId;
			ep.OutputDestination = OutputDestination;
			ep.RunInBackground = RunInBackground;

			ep.ExportFileFormat = ExportFileFormat;
			ep.OutputFileName = OutputFileName;
			ep.OutputFileName2 = OutputFileName2;
			ep.DuplicateKeyValues = DuplicateKeyTableValues;

			ep.QualifiedNumberSplit = QualifiedNumberSplit;

			ep.ExportStructureFormat = ExportStructureFormat;
			ep.StructureFlags = StructureFlags;
			ep.FixedHeightStructures = FixedHeightStructures;
			ep.ColumnNameFormat = ColumnNameFormat;
			ep.IncludeDataTypes = IncludeDataTypes;
			ep.OpenMode = OpenMode;
			ep.ViewStructures = ViewStructures;

			return ep;
		}

		/// <summary>
		/// Copy ExportParms members to ResultsFormat
		/// </summary>
		/// <param name="ep"></param>

		public void CopyFromExportParms(ExportParms ep)
		{
			QueryId = ep.QueryId;
			OutputDestination = ep.OutputDestination;
			RunInBackground = ep.RunInBackground;

			ExportFileFormat = ep.ExportFileFormat;
			OutputFileName = ep.OutputFileName;
			OutputFileName2 = ep.OutputFileName2;
			DuplicateKeyTableValues = ep.DuplicateKeyValues;

			if (ep.QualifiedNumberSplit != QnfEnum.Undefined)
				QualifiedNumberSplit = ep.QualifiedNumberSplit;

			ExportStructureFormat = ep.ExportStructureFormat;
			StructureFlags = ep.StructureFlags;
			FixedHeightStructures = ep.FixedHeightStructures;
			ColumnNameFormat = ep.ColumnNameFormat;
			IncludeDataTypes = ep.IncludeDataTypes;
			OpenMode = ep.OpenMode;
			ViewStructures = ep.ViewStructures;

			return;
		}

/// <summary>
/// Clone
/// </summary>
/// <returns></returns>

		public ResultsFormat Clone()
		{
			return (ResultsFormat)this.MemberwiseClone();
		}

	} // end of ResultsFormat class

	/// <summary>
	/// This class contains the data on a results "segment" A segment is a horizontal
	/// span across the page and may contain a partial table, a complete table or 
	/// multiple complete tables.
	/// </summary>
	public class ResultsSegment
	{
		public int Width; // include begin & end column dividers;
		public ResultsTable Table1; // table data for first table in segment
		public int FieldCount; 
		public string [] Header; // formatted text of header
		public string HorizontalLine = ""; // horizontal interrow dividing line
		public int FooterLineCount;

		// Data in temporary buffer for the row currently being processed
		public int TempLines; // count of lines in temp buffer
		public int TempLinesFirst; // count of lines in temp buffer for first table
		public int TempLinesOther; // count of lines in temp buffer for other tables
		public int TempBegin; // index of start of segment in temp buffer

		// Data held in intermediate buffer for one or more rows
		public int IbLines; // count of lines in intermediate buffer
		public TupleBuffer IbFirst; // first tuple buffer for segment
		public TupleBuffer IbLast; // last tuple buffer for segment
		public int IbRows; // count of rows in the segment

		public ResultsSegment()
		{
		}

	} // ResultsSegment

}
