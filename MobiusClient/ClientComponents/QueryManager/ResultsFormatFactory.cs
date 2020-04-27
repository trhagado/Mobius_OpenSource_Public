using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Process a query object & define initial format of query results
	/// </summary>
	
	public class ResultsFormatFactory
	{
// Structure of segment header information while being built. A table
// header is built for each table after the first. Column headers
// are built in all cases. Table & column headers may be from 1 to 10 
// lines each. Headers also contain vertical & horizontal dividers.
// The general format is:
// 
// 0   +--------------------------------+   top horizontal.
// 1   |                                |   1st line of table header.
// ... |                                |   mid lines of table header.
// 10  |                                |   last line of table header.
// 11  +----------+----------+----------+   dividing horizontal.
// 12  |          |          |          |   1st line of column header.
// ... |          |          |          |   mid lines of column header.
// 21  |          |          |          |   last line of column header.
// 22  +----------+----------+----------+   bottom horizontal.
// 23  |          |          |          |   conditional formatting lines 
// 24  |          |          |          |   (moved up to be before col headers)
// 24  |          |          |          |   
// 26  |          |          |          |   

		const int Horizontal1       =  0;
		const int TableHeaderStart  =  1;
		const int TableHeaderEnd    = 10;
		const int TableHeaderMax    = 10;
		const int Horizontal2       = 11;
		const int ColumnHeaderStart = 12;
		const int ColumnHeaderEnd   = 21;
		const int ColumnHeaderMax   = 10;
		const int Horizontal3       = 22;
		const int CfHeaderStart     = 23;
		const int CfHeaderMax       = 10;
		const int SegHeaderAlloc = CfHeaderStart + CfHeaderMax;

		int TableHeaderLines = 0; // temp state of current segment being built
		int	ColumnHeaderLines = 0;
		int	CfHeaderLines = 0;

		int TableHeaderLabelCount = 0;
		int ColumnHeaderLabelCount = 0;
		int CfHeaderLabelCount = 0;

		internal QueryManager QueryManager;
		public Query Query { get { return QueryManager.Query; } }
		public ResultsFormatter ResultsFormatter { get { return QueryManager.ResultsFormatter; } } 
		public MoleculeGridControl MoleculeGridControl { get { return QueryManager.MoleculeGrid; } }

		public ResultsFormat Rf { 
			get { return QueryManager.ResultsFormat; }
			set { QueryManager.ResultsFormat = value; } }
		Query Q { get { return QueryManager.Query; } } // alias
	
/// <summary>
/// Basic constructor
/// </summary>

		public ResultsFormatFactory()
		{
			QueryManager = new QueryManager();
			ResultsFormat rf = new ResultsFormat();
			LinkRfToQm(rf);
			return;
		}

		/// <summary>
		/// Construct with supplied QueryManager & dest
		/// </summary>

		public ResultsFormatFactory(QueryManager qm, OutputDest queryDest)
		{
			QueryManager = qm;
			ResultsFormat rf = new ResultsFormat();
			rf.OutputDestination = queryDest;
			LinkRfToQm(rf);
			return;
		}

		/// <summary>
		/// Construct with supplied QueryManager and ResultsFormat
		/// </summary>

		public ResultsFormatFactory(QueryManager qm, ResultsFormat rf)
		{
			QueryManager = qm;
			LinkRfToQm(rf);
		}

		void LinkRfToQm(ResultsFormat rf)
		{
			rf.QueryManager = QueryManager;
			QueryManager.ResultsFormat = rf;
		}

/// <summary>
/// Construct new query formatter based on existing report format
/// </summary>
/// <param name="rf"></param>

		public ResultsFormatFactory(
			ResultsFormat rf)
		{
			ResultsFormat rf2 = rf.Clone();
			QueryManager = rf2.QueryManager;
			rf2.GraphicsContext = new GraphicsMx();
			rf2.Tables = new List<ResultsTable>();
			rf2.TablesExtra = new List<ResultsTable>();
			rf2.Segments = new List<ResultsSegment>();
			rf2.TitleBuffer = new List<string>();

			rf2.RepeatCount = -1; // need to recalc number of repeats
		}

/// <summary>
/// Construct new query formatter with supplied query & destination
/// </summary>
/// <param name="query"></param>
/// <param name="outputDest"></param>

		public ResultsFormatFactory (
			QueryManager queryManager,
			Query query,
			OutputDest outputDest)
		{
			QueryManager = queryManager;

			ResultsFormat rf = new ResultsFormat();
			rf.QueryManager = QueryManager;
			QueryManager.ResultsFormat = rf;

			QueryManager.Query = query;
			query.QueryManager = queryManager;

			rf.OutputDestination = outputDest;
			return;
		}

/// <summary>
/// Build the report formatting data for a query
/// </summary>
/// <param name="query">The query to be used as a starting point</param>

		public void Build()
		{
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			ResultsTable rt;
			ResultsField rfld;
			int structureColumnWidth = 0; // structure width (milliinches)
			int fi; // field index
			int ti; // table index
			int freeColumn = 0; // next free column position
			int reportWidth;
			int i1, i2, i3;
			string txt, txt2, txt3;
			float r1;
			//			Rf.GraphicsContext = new XlGraphics();

			// See if input is reasonable

			if (Rf == null)
				throw new Exception("Null Report Format (Rf)");

			if (Rf.OutputDestination <= 0)
				throw new Exception( "Output destination not defined");

			if (Q == null || Q.Tables == null || Q.Tables.Count == 0)
				throw new UserQueryException( "The query must contain one or more data tables");

			QueryManager.Query = Q; // be sure query and querymanager are linked
			Q.QueryManager = QueryManager;

		BeginFormatting:

			if (Rf.Excel || Rf.TextFile || Rf.Spotfire || Rf.SdFile || Rf.Html) // no repeats allowed
				Rf.RepeatCount = 0;

			if (Rf.Grid) // use value from query if grid, exports set value themselves
				Rf.DuplicateKeyTableValues = Query.DuplicateKeyValues;

			// Decide if row framing should be done

			if (SS.I.RowFraming > 0) Rf.RowFraming = true;
			else if (SS.I.RowFraming == 0) Rf.RowFraming = false;
			else if (Q.Tables.Count > 1) Rf.RowFraming = true;
			else if (Rf.Html) Rf.RowFraming = true;
			else Rf.RowFraming = false;

			// Create ResultsTables & ResultsFields

			Rf.VoLength = Rf.KeyValueVoPos + 1; // where first value will go
			Rf.VoIndexToQueryColumn = new List<QueryColumn>();
			Rf.VoIndexToResultsField = new List<ResultsField>();

			Rf.Tables.Clear(); // be sure we're starting with no tables

			for (ti = 0; ti < Q.Tables.Count; ti++)
			{
				qt = Q.Tables[ti];
				mt = qt.MetaTable;

				if (Rf.ParentQe == null || !mt.RetrievesMobiusData) // reset value object positions
				{
					qt.VoPosition = -1;
					foreach (QueryColumn qc0 in qt.QueryColumns)
						qc0.VoPosition = -1;
				}

				if (!mt.RetrievesMobiusData) // don't include table in Rf.Tables if it doesn't retrieve any data from Mobius
					continue;

				rt = new ResultsTable();
				rt.ResultsFormat = Rf;
				rt.QueryTable = qt;
				rt.MetaTable = mt;

				Rf.Tables.Add(rt);

				for (fi = 0; fi < qt.QueryColumns.Count; fi++)
				{
					//if (qt.Alias == "T24") qt = qt; // debug

					qc = qt.QueryColumns[fi];
					if (!qc.Is_Selected_or_GroupBy_or_Sorted) continue; // only selected, grouped & sorted fields
					if (qc.MetaColumn == null) continue; // must have metacolumn

					Rf.VoLength++; // accumulate length of Vo

					if (Rf.ParentQe == null || qc.VoPosition < 0) // assign value object position 
					{ // except if ParentQe and VoPosition already defined then use existing positions since reordering by user may have occurred
						qc.VoPosition = Rf.VoLength - 1; // 
						if (qt.VoPosition < 0) qt.VoPosition = qc.VoPosition; // position for first vo element for table
					}

					while (Rf.VoIndexToQueryColumn.Count < Rf.VoLength)
						Rf.VoIndexToQueryColumn.Add(null);
					Rf.VoIndexToQueryColumn[Rf.VoLength - 1] = qc;

					if (!qc.Selected) continue; // only selected fields

					mc = qc.MetaColumn;
					rfld = new ResultsField();
					rfld.MetaColumn = mc;
					rfld.QueryColumn = qc;
					rfld.ResultsTable = rt;
					rfld.VoPosition = qc.VoPosition;
					rfld.SortOrder = qc.SortOrder;
					rt.Fields.Add(rfld);

					while (Rf.VoIndexToResultsField.Count < Rf.VoLength)
						Rf.VoIndexToResultsField.Add(null);
					Rf.VoIndexToResultsField[Rf.VoLength - 1] = rfld;

					if (ti == 0 && mc.DataType == MetaColumnType.Structure && Rf.FirstTableStructureField == null)
						Rf.FirstTableStructureField = rfld;

					if (mc.DataType == MetaColumnType.Image)
					{ // frame rows if outputting images & framing not explicitly defined
						if (SS.I.RowFraming < 0) Rf.RowFraming = true;
					}

					if (mc.DataType == MetaColumnType.QualifiedNo && Rf.QualifiedNumberSplit != QnfEnum.Combined)
					{ // need to split out qualified number subfields?
						rt.Fields.RemoveAt(rt.Fields.Count - 1);
						AddQnsSubfieldIfSelected(rt, rfld, QnfEnum.Qualifier);
						AddQnsSubfieldIfSelected(rt, rfld, QnfEnum.NumericValue);
						AddQnsSubfieldIfSelected(rt, rfld, QnfEnum.StdDev);
						AddQnsSubfieldIfSelected(rt, rfld, QnfEnum.StdErr);
						AddQnsSubfieldIfSelected(rt, rfld, QnfEnum.NValue);
						AddQnsSubfieldIfSelected(rt, rfld, QnfEnum.NValueTested);
						AddQnsSubfieldIfSelected(rt, rfld, QnfEnum.TextValue);
					}

					Q.AllowColumnMerging = false; // column merging no longer allowed
					if (!Q.AllowColumnMerging || !qt.AllowColumnMerging || !mt.AllowColumnMerging ||
						mc.DataType != MetaColumnType.Structure || // need to check for combination of struct with key?
						mt.MetaBrokerType == MetaBrokerType.Annotation || // not for annotation
						mt.Parent != null) continue; // not for non-parent table

					if (Rf.SdFile || Rf.Spotfire) continue;
					if (Rf.TextFile)
					{
						rt.Fields.RemoveAt(rt.Fields.Count - 1); // don't need structure here
						continue;
					}

					if (Rf.Excel) if (!MergingIntoStructure()) continue; // only merge for excel if explicitly requested
					if (!mt.IsRootTable) continue; // do for key table only

					// Merge structure with compoundId field

					Rf.CombineStructureWithCompoundId = true;
					Rf.CombinedStructureField = rfld;
					rt.Fields.RemoveAt(rt.Fields.Count - 1); // don't need structure here

					if (qc.DisplayWidth > 0) r1 = qc.DisplayWidth;
					else r1 = mc.Width;
					structureColumnWidth = QcWidthInCharsToDisplayColWidthInMilliinches(r1, Rf);
					if (Rf.Grid && structureColumnWidth < 2500) structureColumnWidth = 2500;

					if (SS.I.RowFraming < 0) Rf.RowFraming = true;
				}
			}

			// Setup display metrics

			if (Rf.Html)
			{
				Rf.DisplayPageWidth = 10000;
				Rf.DisplayPageHeight = 7500;

				Rf.LogicalPixelsX = GraphicsMx.LogicalPixelsX;
				Rf.LogicalPixelsY = GraphicsMx.LogicalPixelsY;
				Rf.LogicalPixelsX = (int)(Rf.LogicalPixelsX * SS.I.HtmlScaleAdjustment / 100.0);
			}

			else if (Rf.Grid)
			{
				Size s = GetQueryResultsControlSize();
				Rf.DisplayPageWidth = s.Width;
				Rf.DisplayPageHeight = s.Height;
				Rf.PageScale = Q.ViewScale;

				Rf.LogicalPixelsX = GraphicsMx.LogicalPixelsX;
				Rf.LogicalPixelsY = GraphicsMx.LogicalPixelsY;
				Rf.LogicalPixelsX = (int)(Rf.LogicalPixelsX * SS.I.GridScaleAdjustment / 100.0);
			}

			// Define initial page dimensions

			if (Rf.Word)
			{
				if (Rf.PageMargins == null) Rf.PageMargins = new PageMargins(0, 0, 0, 0);
				Rf.PageWidth = // subtract margins from page width
					Rf.PageWidth - (Rf.PageMargins.Left + Rf.PageMargins.Right);
				Rf.PageScale = SS.I.ResultsPageScale;

				// output as graphics image

				Rf.FontName = SS.I.ResultsFontName;
				Rf.FontSize = SS.I.ResultsFontSize;
				Rf.PageHeight = Rf.PageHeight - (Rf.PageMargins.Top + Rf.PageMargins.Bottom);
				Rf.PageLines = (int)((Rf.PageHeight / 1000.0) / (Rf.FontSize / 72.0));
				//Rf.PageMargins = new PageMargins(0, 0, 0, 0); // margins provided by Word
			}

			else if (Rf.Excel)
			{
				Rf.PageOrientation = Orientation.Vertical;
				Rf.PageWidth = 10000000; // something wide 
				Rf.PageHeight = -1; // undefined		
				Rf.PageMargins = new PageMargins(0, 0, 0, 0);
				Rf.PageScale = SS.I.ResultsPageScale;
				Rf.FontName = SS.I.ResultsFontName;
				Rf.FontSize = SS.I.ResultsFontSize;
				Rf.PageLines = 1000000;
			}

			else if (Rf.TextFile || Rf.SdFile || Rf.Spotfire)
			{
				Rf.PageOrientation = Orientation.Vertical;
				Rf.PageWidth = 1000000;
				Rf.PageHeight = -1;
				Rf.PageMargins = new PageMargins(0, 0, 0, 0);
				Rf.PageScale = 100;
				Rf.FontName = "Courier";
				Rf.FontSize = 9;
				Rf.PageLines = -1;
			}

			else if (Rf.Console)
			{
				Rf.PageOrientation = Orientation.Vertical;
				Rf.PageWidth = QcWidthInCharsToDisplayColWidthInMilliinches(80, Rf); // make it 80 columns
				Rf.PageHeight = -1;
				Rf.PageMargins = new PageMargins(0, 0, 0, 0);
				Rf.PageScale = 100;
				Rf.FontName = "Courier";
				Rf.FontSize = 9;
				Rf.PageLines = 20;
			}

			else if (Rf.Html)
			{
				Rf.OutputFormContext = OutputFormContext.Popup; // always format for popup for now
				Rf.PageHeight = 2000000000; // no breaks
				//Rf.PageHeight = (int)(Rf.DisplayPageHeight * Rf.HtmlScreensPerPage * .9);

				if (SS.I.BreakHtmlPopupsAtPageWidth)
					Rf.PageWidth = Rf.HtmlPageWidth = 10000; // do landscape page width
				else
				{
					Rf.PageWidth = Rf.HtmlPageWidth = 1000000000; // no limit on page width
					Query.RepeatReport = false; // don't try to repeat across wide area
				}

				Rf.PageOrientation = Orientation.Vertical;
				Rf.PageMargins = new PageMargins(0, 0, 0, 0);
				Rf.PageScale = SS.I.ResultsPageScale;
				Rf.FontName = SS.I.ResultsFontName;
				Rf.FontSize = SS.I.ResultsFontSize;
				Rf.PageLines = (int)((Rf.PageHeight / 1000.0) / (Rf.FontSize / 72.0) * 1.0); // adjustment
			}

			else if (Rf.Grid)
			{
				//if (Lex.Eq(Query.UserObject.Description, "PopupDisplay = true"))
				//	Rf.PopupDisplay = true; 

				Rf.PageOrientation = Orientation.Vertical;
				Rf.PageWidth = Rf.DisplayPageWidth - 200; // use current size minus scoll bar width
				Rf.PageHeight = -1; // not defined
				Rf.PageMargins = new PageMargins(0, 0, 0, 0);
				//Rf.PageScale = SS.I.ResultsPageScale; // (don't override Q.ViewScale if not 100)
				Rf.FontName = SS.I.ResultsFontName;
				//		Rf.FontSize=repFontSize; 
				Rf.FontSize = 8; // fudge to fill full width of cells better
				Rf.PageLines = 1000000000; // unlimited
			}

			else throw new Exception("Invalid ResultsFormat.Dest");

			Rf.GraphicsContext.SetFont(Rf.FontName, Rf.FontSize);
			Rf.CharWidth = Rf.GraphicsContext.StringWidth("X"); // "average" char width (milliinches)
			Rf.LineHeight = Rf.FontSize * 1000 / 72; // line height (milliinches)

			if (Rf.Html) Rf.InterColumnSpacing = Rf.CharWidth * 1; // spacing between column repeats

			else if (Rf.Grid)
			{
				if (Q.ShowGridCheckBoxes) Rf.InterColumnSpacing = 200; // add space for check boxes
				else Rf.InterColumnSpacing = 0;
			}

			else if (Rf.Word) Rf.InterColumnSpacing = 0; // no space for native word format

			else Rf.InterColumnSpacing = Rf.CharWidth * 2; // default value

			// Start to determine the format for the report 

			Rf.TableOriented = true; // assume a table report for starters
			Rf.CompoundOriented = false;
			ConsolidateMergedFields();

			// Determine the single-segment width of each table. Remove compound number
			// field from all tables but the first.

			if (Rf.Word || Rf.Grid) Rf.TotalWidth = 0; // if Word don't count left column
			else Rf.TotalWidth = Rf.CharWidth; // count left column

			bool multiPivotTableIncluded = false;
			for (ti = 0; ti < Rf.Tables.Count; ti++)
			{
				rt = Rf.Tables[ti];
				CalculateColumnWidths(rt, ref structureColumnWidth);
				Rf.TotalWidth += rt.Width;
				if (rt.MetaTable.MultiPivot) multiPivotTableIncluded = true; // map summary tables at runtime
			}

			// Decide if this should be a compound-oriented or a table-oriented
			// report. If compound-oriented all of the data for a compound is 
			// held together and the compound number appears only once. Data
			// for a single compound may span multiple pages. 
			// If table-oriented then data for as many compounds as possible
			// is fit on a page and the compound number appears in each segment
			// of the report.


			if ((Rf.Html || Rf.Console) &&
				!Rf.CompoundOriented && Rf.RepeatCount <= 0)
			{

				if (Rf.TotalWidth > 7 * Rf.PageWidth || multiPivotTableIncluded) // wide enough to qualify as compound-oriented 
				{
					Rf.CompoundOriented = true;
					Rf.TableOriented = false;
					if (Rf.Html) Rf.PageLines = 1000000; // place all data for a compound on each page
				}
			}

			qt = Q.Tables[0]; // if this is a key & structure grid display then use Card view (LayoutView)
			mc = qt.MetaTable.FirstStructureMetaColumn;
			if (Rf.Grid && Q.RepeatReport && Q.Tables.Count == 1 && qt.SelectedCount == 2 &&
				mc != null && qt.GetQueryColumnByName(mc.Name).Selected)
			{
				Rf.UseLayoutView = true;
				Rf.FixedHeightStructures = true; // also use fixed height structures
			}

			if (Rf.Word && Rf.ColumnCount() > 256)
			{
				throw new Exception(
					"The exported form of this query would contain " + Rf.ColumnCount().ToString() + " columns\n" +
					"which exceeds the Word limit of 256 columns.\n" +
					"Try breaking your query into multiple separate smaller queries\n" +
					"or reduce the level of splitting of qualified numbers.");
			}

			if (Rf.Excel && Rf.ColumnCount() > 16384)
			{
				throw new Exception(
					"The exported form of this query would contain " + Rf.ColumnCount().ToString() + " columns\n" +
					"which exceeds the Excel and Word limit of 16384 columns.\n" +
					"Try breaking your query into multiple separate smaller queries\n" +
					"or reduce the level of splitting of qualified numbers.");
			}

			if (Rf.Excel && Rf.TotalWidth > Rf.PageWidth)
			{
				throw new Exception(
					"The Excel worksheet that would be produced for this query is too wide.\r\n" +
					"Try breaking your query into multiple separate smaller queries.");
			}

			// Scale report for specfied repeats or calc repeats if not specified

			bool noRepeat = !SS.I.RepeatReport || !Q.RepeatReport;

			if (Rf.Grid && noRepeat && // explicit no repeats?
				(Rf.Tables.Count == 0 || Rf.Tables.Count > 1 || Rf.Tables[0].Fields.Count > 1)) // always repeat if only one report field
				Rf.RepeatCount = 0;

			if (Rf.Grid && Rf.TotalWidth >= Rf.PageWidth)
			{
				Rf.RepeatCount = 0; // no repeats
				Rf.PageWidth = 2000000000; // set something wide
				//				if (Rf.TotalWidth>Rf.PageWidth) 
				//				{ // do as html if too wide
				//					Rf.OutputDestination = OutputDest.Html;
				//					goto BeginFormatting;
				//				}
			}

			// Calculate number of repeats of the report across the page

			if (Rf.RepeatCount < 0) // fill page with repeats
			{
				Rf.RepeatCount = 0;
				reportWidth = Rf.TotalWidth;

				if (Rf.Word && reportWidth > Rf.PageWidth) // reduce to fit across page
				{
					r1 = (float)Rf.PageWidth / reportWidth;
					Rf.PageWidth = (int)(Rf.PageWidth * 1.03);  // avoid artificial breaks
					if (!ScaleReportSegments(r1)) goto BeginFormatting;
				}

				while (true) // repeat until width of page is filled
				{
					reportWidth += Rf.InterColumnSpacing + Rf.TotalWidth;
					if (reportWidth > Rf.PageWidth) break;
					Rf.RepeatCount++;
				}
			}

			else  // explicit number of repeats
			{
				reportWidth = Rf.TotalWidth;

				for (i1 = 0; i1 < Rf.RepeatCount; i1++) // sum total width
					reportWidth += Rf.InterColumnSpacing + Rf.TotalWidth;

				if (reportWidth > Rf.PageWidth && !Rf.Grid && !Rf.Html && !Rf.CompoundOriented)
				{ // need to scale segments? (don't do for grid or if compound oriented)
					r1 = (float)Rf.PageWidth / reportWidth;
					if (Rf.Word) // avoid artificial breaks
						Rf.PageWidth = (int)(Rf.PageWidth * 1.03);

					ScaleReportSegments(r1);
				}
			}

			// Page header info

			if (Rf.Title == "") Rf.Title = Query.UserObject.Name; // default title
			Rf.TitleBuffer = new List<string>();

			/*
			 * Map tables to segments using the following rules.
			 * 1. Place one or more compete tables in each segment.
			 * 2. If a table won't fit in a partially full segment then
			 *    start a new segment for the table.
			 * 3. If a table uses multiple segments the the compound number
			 *    is repeated for each segment.
			 * 4. Each segment must contain at least two fields, a compound
			 *    number and one other data column.
			 */

			Rf.Segments = new List<ResultsSegment>();
			freeColumn = Rf.PageWidth + 1; // Cause first segment allocation

			// Process tables

			for (ti = 0; ti < Rf.Tables.Count; ti++)
			{
				rt = Rf.Tables[ti];
				rt.Position = ti;
				mt = rt.MetaTable;
				i1 = Rf.PageWidth - freeColumn; // space remaining in current segment

				if (ti > 0 && // force new segment if table won't fit in remainder
					(rt.Width > i1 ||
					(Rf.CompoundOriented))) // && rt.Fields.Count>1))) 
				{
					freeColumn = Rf.PageWidth + 1;
				}

				MapTableToSegments(rt, ti, ref freeColumn); // assign segments for table
				if (rt.Width > Rf.PageWidth) freeColumn = Rf.PageWidth; // if mult segs for table say full now
			}

			if (Rf.Segments.Count > 0) // format the headers
				FormatSegmentHeaders(null);

			//CondFormat.InitializeInternalMatchValues(Query); // prepare for condformat matching

			return;
		}

/// <summary>
/// Get the size of the QueryResultsControl
/// </summary>
/// <returns></returns>

		public static Size GetQueryResultsControlSize()
		{
			Size s = SessionManager.Instance.QueriesControl.QueryResultsControl.Tabs.Size;
			return s;
		}

/// <summary>
/// Add a field split out of a qualified number
/// </summary>
/// <param name="rt"></param>
/// <param name="rfldModel"></param>
/// <param name="qnfType"></param>

		void AddQnsSubfieldIfSelected (
			ResultsTable rt,
			ResultsField rfldModel,
			QnfEnum qnfType)
		{
			if ((Rf.QualifiedNumberSplit & qnfType) == 0) return;
			ResultsField rfld = rfldModel.Clone();
			rt.Fields.Add(rfld);
			rfld.QualifiedNumberSplit = qnfType;
			return;
		}

/// <summary>
/// Calculate initial column widths
/// </summary>
/// <param name="rt"></param>
/// <param name="structureColumnWidth"></param>
/// <returns></returns>

		public void CalculateColumnWidths ( 
			ResultsTable rt,
			ref int structureColumnWidth) 
		{
			QueryTable qt;
			QueryColumn qc, qc2;
			MetaTable mt, mt2;
			MetaColumn mc, mc2;
			ResultsField rfld;
			MergedField mfld;
			float f2,f3;
			int width,cnColWidth,fi,i1,i2,i3; 

			mt = rt.MetaTable;
			cnColWidth=0; // compound number col width

			qt = rt.QueryTable;
			mt = rt.MetaTable;

			if (Rf.Word) rt.Width=0; // if word don't count left column
			else rt.Width = Rf.CharWidth; // count left column

			RemoveCompoundIdFieldIfNotFirstTable(rt); 

			for (fi=0; fi<rt.Fields.Count; fi++) 
			{ 
				rfld = rt.Fields[fi];
				qc = rfld.QueryColumn;
				mc = rfld.MetaColumn;
				if (qc.DisplayWidth > 0) f2 = qc.DisplayWidth;
				else f2 = mc.Width;
				if (mc.DataType!=MetaColumnType.Structure && // include label in the cell if not graphic field 
					mc.DataType!=MetaColumnType.Image) 
				{ 
					f3=rfld.MergeLabel.Length;
					if (f3>20 && f3>f2) f3=f2+2; // control width
					f2 += f3;
				}

				if (mc.DataType == MetaColumnType.QualifiedNo &&
					rfld.QualifiedNumberSplit == QnfEnum.Qualifier)
					f2 = 1; // split-out qualifier width is one char wide

				width = QcWidthInCharsToDisplayColWidthInMilliinches(f2, Rf);
				if (fi==1) cnColWidth=width; // save compoundNumber width

// If key table compound number & combined with structure set width to structure width

				if (Rf.CombineStructureWithCompoundId && mt.IsRootTable && 
					mc.DataType == MetaColumnType.CompoundId)
				{
					int width2 = QcWidthInCharsToDisplayColWidthInMilliinches(structureColumnWidth, Rf); // structure col width
					if (width2 > width) width = width2; // use if larger
				}

// If merged then consider the merged fields

				if (rfld.MergedFieldList != null) 
				{
					for (i1=0; i1<rfld.MergedFieldList.Count; i1++) 
					{
						mfld = (MergedField)rfld.MergedFieldList[i1];
						qc2 = mfld.QueryColumn;
						mt2 = mfld.MetaTable;
						mc2 = mfld.MetaColumn;
						if (qc2.DisplayWidth > 0) f2 = qc2.DisplayWidth;
						else f2 = mc2.Width;
						if (mc2.DataType!=MetaColumnType.Structure && 
							mc.DataType!=MetaColumnType.Image) 
						{ 
							f3=mfld.Label.Length;
							if (f3>20 && f3>f2) f3=f2+2; // control width
							f2+=f3;
						}
						i2 = QcWidthInCharsToDisplayColWidthInMilliinches(f2, Rf);
						if (i2>width) width=i2;
					}
				}

				rfld.ColumnWidth = width;
				rt.Width += rfld.ColumnWidth; // add to total table width
			}
		}

/// <summary>
/// Convert from column width in chars to full column width in milliinches including extra columns
/// </summary>
/// <param name="cols"></param>
/// <param name="rf"></param>
/// <returns></returns>

		public static int QcWidthInCharsToDisplayColWidthInMilliinches(float cols, ResultsFormat rf)
		{
			int extraColumns = 0;
			if (rf.Grid) extraColumns = 1; // need a bit of extra space for grid

			extraColumns += 3; // extra spacing including 2 margin spaces & vertical col divider;

			int miWidth = // column width including any extraColumns
				(int)((cols + extraColumns) * rf.CharWidth + .5);

			miWidth = (int)(miWidth * rf.PageScale/100.0); // adjust scale for page

			if (rf.PageScale == 100)
			{
				if (rf.Grid)
					miWidth = (int)(miWidth * SS.I.GridScaleAdjustment / 100.0);
				else if (rf.Html)
					miWidth = (int)(miWidth * SS.I.HtmlScaleAdjustment / 100.0);
			}

			return miWidth;
		}

/// <summary>
/// If not the first table remove the compound number field
/// </summary>
/// <param name="rt"></param>

		void RemoveCompoundIdFieldIfNotFirstTable (
			ResultsTable rt)
		{
			MetaTable mt = rt.MetaTable;
			if (rt.Fields.Count <= 0) return;
			ResultsField rfld = rt.Fields[0];
			MetaColumn mc = rfld.MetaColumn;

			if (!ReferenceEquals(rt,Rf.Tables[0]) && rt.Fields.Count>0 && mc.IsKey)
				rt.Fields.RemoveAt(0);
			return;
		}

/// <summary>
/// Map a report table to one or more segments
/// </summary>
/// <param name="rt"></param>
/// <param name="freeColumn"></param>

		public void MapTableToSegments (
			ResultsTable rt,
			int tableIndex,
			ref int freeColumn)
		{
			QueryTable qt;
			MetaTable mt;
			ResultsField rfld;
			ResultsSegment seg = null;
			int si,fi,i1;
			
			qt = rt.QueryTable;
			mt = rt.MetaTable;

			si=Rf.Segments.Count - 1; // current segment
			if (si<0) si=0;
			else seg = (ResultsSegment)Rf.Segments[si];
			rt.FirstSegment=si; 

			fi=0;
			while (fi < rt.Fields.Count) 
			{
				rfld = rt.Fields[fi];
				rfld.ColumnX = freeColumn; 
				freeColumn += rfld.ColumnWidth;

				if (freeColumn>=Rf.PageWidth || seg == null) // go to new segment
					do 
					{
						if (Rf.Segments.Count>0) 
						{
							if (Rf.TableOriented && seg.FieldCount==1 && fi!=0 &&
								(!Rf.CombineStructureWithCompoundId || Rf.Segments.Count>1)) break; // need at least 2 fields in seg?

							if (Rf.Word) break; // only one segment allowed for Word
						}
			 
						seg = new ResultsSegment();
						Rf.Segments.Add(seg);
//						seg.Header= new string[HEADER_ALLOC]; // initial allocation
						si = Rf.Segments.Count - 1;

						freeColumn=Rf.CharWidth; 

// Duplicate key field if table breaks across segments

						if (Rf.TableOriented && !(tableIndex==0 && fi==0)) // ok to break?
						{ 
							rfld = new ResultsField();
							rfld.QueryColumn = qt.QueryColumns[0]; // assume first querycolumn is key col
							rfld.VoPosition = rfld.QueryColumn.VoPosition;
							rfld.MetaColumn = rfld.QueryColumn.MetaColumn;
							rfld.ResultsTable = rt;

							rfld.VoPosition = Rf.KeyValueVoPos;
							rfld.ColumnWidth = // use compound number width
								QcWidthInCharsToDisplayColWidthInMilliinches(mt.MetaColumns[0].Width, Rf);

							rfld.Merge = false; // clear merge info
							rfld.MergeLabel = "";
							rfld.MergedFieldList = null;

							rt.Width=rt.Width + rfld.ColumnWidth; 
							rt.Fields.Insert(fi,rfld); // insert new field
						}

						rfld.ColumnX=Rf.CharWidth; 
						freeColumn = Rf.CharWidth + rfld.ColumnWidth; 
					} while (false);

				// Adjust column width if need to provide at least 1 field per segment if
				// compound number oriented or 2 fields per segment for table oriented.

				if (freeColumn>=Rf.PageWidth) 
				{
					i1=freeColumn - Rf.PageWidth; 
					rt.Width -= i1; // adjust overall table width
					rfld.ColumnWidth = rfld.ColumnWidth - i1; // and column width
					freeColumn = Rf.PageWidth; // this segment is now full
				}

				// Complete column & field data

				rfld.FieldX = rfld.ColumnX + Rf.CharWidth; 
				rfld.FieldWidth = (freeColumn - 2*Rf.CharWidth) - rfld.FieldX; 

				if (Rf.Grid) // remove to adjust spacing chars
				{ 
					rfld.FieldX -= Rf.CharWidth*2;
					rfld.FieldWidth += Rf.CharWidth*2;
				}

				if (rfld.QueryColumn.MetaColumn.DataType == MetaColumnType.Image)
					rfld.QueryColumn.Decimals = // store approximate number of pixel for screen in QC for optional sizing of bitmap by QueryEngine
						(int)((rfld.FieldWidth / 1000.0) * GraphicsMx.LogicalPixelsX * 1.0); // width in pixels 

				if (seg == null) // display debug info if null
					throw new Exception("Seg is null: " + Query.UserObject.Id + ", " + rt.MetaTable.Name + "." + rfld.QueryColumn.MetaColumn.Name + ", " + 
						tableIndex + ", " + freeColumn + ", " + Rf.Segments.Count);

				seg.Width = freeColumn;

				if (seg.FieldCount==0) seg.Table1 = rt; // first table for seg
				rfld.FieldPosition = seg.FieldCount;
				seg.FieldCount++;

				if (fi==0) rt.FirstSegment = si; // first seg for table
				fi++;
			}

			return;
		}

/// <summary>
/// Scale down segment widths & font sizes
/// </summary>
/// <param name="scale"></param>
/// <returns></returns>

		bool ScaleReportSegments ( 
			float scale)
		{
			ResultsTable rt;
			ResultsField rfld;
			String msg;
			int ti,fi,i1;
			const int MINIMUM_FONT_SIZE = 8;

			if (scale==1.0) return true;

			i1=(int)(Rf.FontSize*scale + 1); // scale font size & round up

			if (i1 < MINIMUM_FONT_SIZE && SS.I.Attended) 
			{
				if (i1<=0) i1=1;
				msg =
					"Scaling the data selected in this query down to the size necessary to " +
					"to fit it onto the report page in a single section would result in a " +
					"font size of " + i1.ToString() + " pts which is probably too small to be " +
					"readable. You can increase the scale of the report by selecting fewer " + 
					"data tables and/or data items or by selecting a landscape page format. " +
					"Do you want to proceed to format your report in the small font size?";

				DialogResult dr = MessageBoxMx.Show(msg,"Small Font Size",
				MessageBoxButtons.YesNoCancel,MessageBoxIcon.Exclamation);
				if (dr != DialogResult.Yes) return false; 
			}

			Rf.FontSize=i1;
			Rf.GraphicsContext.SetFont(Rf.FontName,Rf.FontSize); // set in context also
			Rf.CharWidth = Rf.FontSize*1000 / (72*2); // CharWidth('X'); // "average" char width (milliinches)
			Rf.LineHeight=Rf.FontSize*1000 / 72; // line height (milliinches)
			Rf.PageLines=(int)((Rf.PageHeight/1000.0) / (Rf.FontSize/72.0));
			Rf.PageScale=(int)(100 * scale); // page scale (percent)

			for (ti=0; ti<Rf.Tables.Count; ti++) 
			{ 
				rt = Rf.Tables[ti];
				rt.Width = (int)(rt.Width * scale);
				for (fi=0; fi<rt.Fields.Count; fi++) 
				{ 
					rfld = rt.Fields[fi];
					rfld.ColumnX = (int)(rfld.ColumnX*scale);
					rfld.ColumnWidth = (int)(rfld.ColumnWidth*scale);
					rfld.FieldX = (int)(rfld.FieldX*scale);
					rfld.FieldWidth = (int)(rfld.FieldWidth*scale);
				}
			}

			return true;
		}

/// <summary>
/// Format headers for one or more segments
/// </summary>
/// <param name="singleRt"></param>

		public void FormatSegmentHeaders (
			ResultsTable singleResultsTable)
		{
			ResultsTable rt;
			ResultsField rfld, rfld2;
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			ResultsSegment seg=null;
			ArrayList mergedFields = new ArrayList();
			MobiusDataType formattedLabel;
			CondFormat cf;
			string [] labels = new string[3];
			string [] segHeader = null;

			String label, qnSubfieldLabel, unitLabel, txt=null;
			bool allFieldsMerged;
			int fieldCount;
			int x,width,labelCount=0;
			int tableRepetition = 0;
			int ti,ti2,si,ri,fi,fi2,i0,i1,i2=0,i5=0;
			string [] buf = new string[3];

			////if (Rf.Grid) return; // done elsewhere for grid (no, need for rt.Header)

			Dictionary<string, object> nameDict = new Dictionary<string, object>(); // dictionary of unique names

			Rf.MaxCondFormattingRules = 0; // get max # of cf rules so we know how many header lines are needed for cf rows

			if (SS.I.ShowConditionalFormatting && Rf.Query.ShowCondFormatLabels)
			{
				List<QueryColumn> qcCfList = Query.GetSelectedCondFormatColumns();
				foreach (QueryColumn qc0 in qcCfList)
				{
					cf = qc0.CondFormat;
					if (cf == null || cf.Rules == null) continue;
					int cfRulesCount = cf.Rules.Count;
					if (cfRulesCount > CfHeaderMax) cfRulesCount = CfHeaderMax;
					if (cf != null && cf.ShowInHeaders && cfRulesCount > Rf.MaxCondFormattingRules)
						Rf.MaxCondFormattingRules = cfRulesCount;
				}
			}

			if (singleResultsTable==null) si=-1; // start at first segment for first table
			else si = singleResultsTable.FirstSegment - 1; // start at this single table

			List<ResultsTable> resultsTables = new List<ResultsTable>();

			ti=0;
			while (true)
			{
				if (singleResultsTable!=null) rt=singleResultsTable;
				else rt = Rf.Tables[ti];

				if (rt.FirstSegment<0)  // table (summary) with no segments
				{
					if (singleResultsTable!=null || ti >= Rf.Tables.Count-1) break;
					ti++;
					continue;
				}

				resultsTables.Add(rt);
				qt = rt.QueryTable;
				mt = rt.MetaTable;

				if (mt.MetaBrokerType == MetaBrokerType.Annotation && // if annotation table with undefined header color
					qt.HeaderBackgroundColor.Equals(Color.Empty)) // then use default for annotation tables
					qt.HeaderBackgroundColor = Color.FromArgb(255, 255, 128); // default color for annotation tables (light yellow)

				fi=0;
				while (fi<rt.Fields.Count) 
				{
					rfld = rt.Fields[fi];

					if (rfld.ColumnX==Rf.CharWidth) // beginning new segment
					{ 
						if (fi>0 || ti>0) SegmentHeaderComplete(seg); // finish setting up prev seg
						si++;
						seg = (ResultsSegment)Rf.Segments[si];

						segHeader = new string[SegHeaderAlloc];
						seg.Header = segHeader;
						for (ri=0; ri<SegHeaderAlloc; ri++) segHeader[ri]="";
						TableHeaderLines = 0; 
						ColumnHeaderLines = 0;
						CfHeaderLines = 0;

						TableHeaderLabelCount = 0;
						ColumnHeaderLabelCount = 0;
						CfHeaderLabelCount = 0;

						segHeader[Horizontal1] = (Rf.CharWidth/2).ToString() + " VLINE\t";
						x=Rf.CharWidth/2; 
						seg.HorizontalLine = "H " + x.ToString() + " " + (seg.Width - x).ToString() + "\t";
						segHeader[Horizontal1] = segHeader[Horizontal3] = seg.HorizontalLine; 
						segHeader[Horizontal1] += "V " + x.ToString() + "\t"; 
						segHeader[Horizontal2] = segHeader[Horizontal1]; 
					}

// Build table labels

					if (fi==0)
						do 
						{
							tableRepetition = 0;
							string tableLabel = Rf.Tables[ti].QueryTable.ActiveLabel;
							for (ti2 = 0; ti2 < ti; ti2++) // count how many times this table has already been seen
							{
								if (Lex.Eq(tableLabel, Rf.Tables[ti2].QueryTable.ActiveLabel))
									tableRepetition++;
							}

							fieldCount=1; 
							for (fi2=fi+1; fi2<rt.Fields.Count; fi2++) 
							{
								rfld2 = rt.Fields[fi2];
								if (rfld2.ColumnX==Rf.CharWidth) break;
								fieldCount++;
							}

							x=rfld.ColumnX - Rf.CharWidth + rt.Width; 
							if (seg.Width < x) x=seg.Width; 
							width=x - Rf.CharWidth - rfld.ColumnX; 
							x -= Rf.CharWidth/2; 
							segHeader[Horizontal1] += "V " + x.ToString() + "\t"; 

							if (Rf.HeaderLines < 2) break; 

							if (Rf.TextFile || Rf.Spotfire) break; // no table header for these

							allFieldsMerged=true;
							for (fi2=0; fi2<rt.Fields.Count; fi2++) 
							{
								rfld2 = rt.Fields[fi2];
								if (!rfld2.Merge) 
								{
									allFieldsMerged=false;
									break;
								}
							}

							bool buildTableLabel = true;
							if ((mt.IsRootTable || allFieldsMerged) && Q.Tables.Count >= 2)
							{ } // buildTableLabel = false; // suppress table label if this is root and there are other tables

							if (!buildTableLabel) 
							{ // no labels if root table or all fields are merged
								if (Rf.Html || Rf.Grid || Rf.Excel || Rf.Word) 
								{ // need to keep blank labels
									labels[0]="";
									labelCount=1;
								}
								else break;
							}

							else 
							{ // build labels
								labelCount=0; 

								if (qt.ActiveLabel!="") // user-specified label?
								{
									labels[labelCount]=qt.ActiveLabel;
									labelCount++;
								}

								else // try up to 3 labels
								{
									if (mt.Label!="" && !SS.I.UseColumnNameLabels) 
									{ // first choice
										labels[labelCount]=mt.Label;
										labelCount++;
									}
									if (mt.ShortLabel!="" && !SS.I.UseColumnNameLabels) 
									{ // second choice
										labels[labelCount]=mt.ShortLabel;
										labelCount++;
									}
									labels[labelCount]=Lex.InternalNameToLabel(mt.Name); // column name is last choice
									labelCount++; 
								}
								TableHeaderLabelCount++; // count real label
							}

							string link = ""; // hyperlink for label
							if ((mt.DescriptionIsAvailable() || !QbUtil.BaseQueryContainsMetaTable(mt.Name)) &&
								Rf.ShowHeaderStyling)
							{ // build link for table header if either of these exist
								link="http://Mobius/command?ShowContextMenu:MetaTableLabelContextMenu";
								if (mt.DescriptionIsAvailable())
									link += "&ShowDescription=" + mt.Name;
								if (!QbUtil.BaseQueryContainsMetaTable(mt.Name))
									link += "&AddToQuery=" + mt.Name;
							}

							CellStyleMx cellStyle = null;
							if (!qt.HeaderBackgroundColor.Equals(Color.Empty) && Rf.ShowHeaderStyling)
							{
								Font f2 = new Font(Rf.FontName,Rf.FontSize,FontStyle.Regular);
								cellStyle = new CellStyleMx(f2, Color.Black, qt.HeaderBackgroundColor);
							}

							i2 = FormatHeaderLabel(segHeader,TableHeaderStart,TableHeaderMax,rfld.ColumnX,width,fieldCount,
								labels,labelCount,Rf.GraphicsContext,1,link,mt.Name,cellStyle, out formattedLabel);
							if (i2 > TableHeaderLines) TableHeaderLines = i2;

							rt.Header = formattedLabel;
						} while (false);

// Build column labels

					int splitFields = 0;
					MetaColumn lastSplitMc = null;
					for (fi=fi; fi<rt.Fields.Count; fi++) // continue to end of table or segment
					{
						rfld = rt.Fields[fi];
						qc = rfld.QueryColumn;
						mc = rfld.MetaColumn;
						labelCount=0;

						if (rfld.MergedFieldList!=null) 
						{ // no column label for merged fields 
							labels[0]="."; // use . as place holder
							labelCount=1; 
						}

						else // normal, non-merged, build of column labels
						{ 
							if (mc.Units!="" && !Lex.Contains(mc.Label, mc.Units)) unitLabel = " (" + mc.Units + ")";
							else unitLabel="";

							qnSubfieldLabel = "";
							if (mc.DataType == MetaColumnType.QualifiedNo &&
								rfld.QualifiedNumberSplit != QnfEnum.Combined && 
								rfld.QualifiedNumberSplit != QnfEnum.NumericValue)
							{ // labels for pieces of qualified numbers
								if ((rfld.QualifiedNumberSplit & QnfEnum.Qualifier) != 0) qnSubfieldLabel = "Q";
								else if ((rfld.QualifiedNumberSplit & QnfEnum.StdDev) != 0) qnSubfieldLabel = "Std. Dev.";
								else if ((rfld.QualifiedNumberSplit & QnfEnum.StdErr) != 0) qnSubfieldLabel = "Std. Err.";
								else if ((rfld.QualifiedNumberSplit & QnfEnum.NValue) != 0) qnSubfieldLabel = "N";
								else if ((rfld.QualifiedNumberSplit & QnfEnum.NValueTested) != 0) qnSubfieldLabel = "N Tested";
								else if ((rfld.QualifiedNumberSplit & QnfEnum.TextValue) != 0) qnSubfieldLabel = "Text";
								else qnSubfieldLabel = mc.Label; // shouldn't happen

								if (mc != lastSplitMc) // increment splitcount if appropriate
								{
									lastSplitMc = mc;
									splitFields++;
								}

								if (splitFields > 1) // append count to make unique field name as necessary
								{
									if (qnSubfieldLabel.Length>1) qnSubfieldLabel += " ";
									qnSubfieldLabel += splitFields.ToString();
								}

								labels[labelCount]=qnSubfieldLabel;
								labelCount++;
							}

							else if (qc.Label!="") // user-specified label?
							{
								labels[labelCount]=qc.Label;
								labelCount++;
							}

							else // try up to 3 labels
							{
								if (mc.Label!="" && !SS.I.UseColumnNameLabels) // first choice
								{ 
									labels[labelCount]=mc.Label + unitLabel;
									if (fi==0 && Rf.CombineStructureWithCompoundId && mt.IsRootTable && mc.DataType == MetaColumnType.CompoundId) 
										labels[labelCount] += ", Structure"; // include structure label
									if (mc.LabelImage != "") labels[labelCount] += "\t" + mc.LabelImage;
									labelCount++;
								}

								if (mc.ShortLabel!="" && !SS.I.UseColumnNameLabels) // second choice
								{ 
									labels[labelCount]=mc.ShortLabel + unitLabel;
									if (fi==0 && Rf.CombineStructureWithCompoundId && mt.IsRootTable && mc.DataType == MetaColumnType.CompoundId) 
										labels[labelCount] += ", Structure"; // include structure label
									labelCount++;
								}

								if (labelCount==0) 
								{
									labels[labelCount]=Lex.InternalNameToLabel(mc.Name) + unitLabel;
									labelCount++; 
								}
							}

// Qualify the column label by the table name if there is only a single header line (i.e. Rf.Textfile or Rf.HeaderLines==1)

							if (Rf.TextFile || Rf.HeaderLines == 1) do
							{ 
								if (Rf.ColumnNameFormat == ColumnNameFormat.None) break; // no label

								else if (Rf.ColumnNameFormat == ColumnNameFormat.Normal && (mt.IsRootTable || Rf.Tables.Count ==1)) 
									break; // no table label for normal format if root table or single table

								label = "";
								if (Rf.ColumnNameFormat == ColumnNameFormat.Normal || Rf.ColumnNameFormat == ColumnNameFormat.Internal)
									{
									if (Rf.ColumnNameFormat == ColumnNameFormat.Normal)
										label = qt.ActiveLabel;

									else if (Rf.ColumnNameFormat == ColumnNameFormat.Internal)
										label = mt.Name;

										if (tableRepetition >= 1 && label != "") // give unique name to each table
											label += " " + (tableRepetition + 1);
									}

									else if (Rf.ColumnNameFormat == ColumnNameFormat.Ordinal)
										label = "T" + (ti +1).ToString();

								if (label != "")
								{
									if (labelCount >= 1) labels[0] = label + "." + labels[0]; // delimiter was " - " previously
									if (labelCount >= 2) labels[1] = label + "." + labels[1];
								}

							} while (false);

							if (Rf.SdFile) // get unique & valid ChemDB field names
							{
								if (qnSubfieldLabel != "") label = qnSubfieldLabel; // use subfield label if exists
								else
								{
									label = rfld.QueryColumn.ActiveLabel;
									if (!mt.IsRootTable && Rf.Tables.Count >= 3) // qualify with table name if exists
										if (Rf.AllowExtraLengthFieldNames)
											label = qt.ActiveLabel + "." + label;
										else label = mt.Name + "." + label; 
								}

								rfld.MergeLabel = ResultsFormatter.BuildValidAndUniqueChemDBFieldName(nameDict, label, Rf.AllowExtraLengthFieldNames);
							}

							if (mc.DataType == MetaColumnType.Image && (Rf.Html || Rf.Grid)) 
							{ // include directions for viewing images
								for (i1=0; i1<labelCount; i1++) 
									labels[i1] += " (Click on image to open)";
							}

							ColumnHeaderLabelCount++; 
						}

						i5 = FormatHeaderLabel (segHeader,ColumnHeaderStart,ColumnHeaderMax,rfld.ColumnX,
							rfld.ColumnWidth - Rf.CharWidth,1,labels,labelCount,Rf.GraphicsContext,1,"","",null, out formattedLabel);

						rfld.Header = formattedLabel;

						if (i5>ColumnHeaderLines) ColumnHeaderLines=i5; 

						x = rfld.ColumnX + rfld.ColumnWidth - Rf.CharWidth/2; // position for vertical
						segHeader[Horizontal2] += "V " + x.ToString() + "\t";

// Build any conditional formatting labels

						cf = qc.CondFormat; // get any cond formatting for field
						int cfRulesCount = 0;
						if (cf != null)
						{
							cfRulesCount = cf.Rules.Count;
							if (cfRulesCount > CfHeaderMax) cfRulesCount = CfHeaderMax;
						}

						//if (cf != null) cf = cf; // debug

						if (qc.MetaColumn.DataType == MetaColumnType.CompoundId && qt.MetaTable.IsRootTable && Rf.CombineStructureWithCompoundId)
						 cf = Rf.CombinedStructureField.QueryColumn.CondFormat; // use structure formatting here

						if (!SS.I.ShowConditionalFormatting || !Rf.Query.ShowCondFormatLabels || 
							(cf != null && !cf.ShowInHeaders)) cf = null;

						if (Rf.MaxCondFormattingRules == 0) rfld.CondFmtHeaders = null;
						else rfld.CondFmtHeaders = new List<MobiusDataType>();

						if ((Rf.Grid || Rf.Excel || Rf.Word) && Rf.MaxCondFormattingRules > 0)
						{ // only do cf for these

							int blankCfLabels = Rf.MaxCondFormattingRules; // put in the blank cf labels first
							if (cf != null && rfld.IsMainValue) blankCfLabels = Rf.MaxCondFormattingRules - cfRulesCount;
							labels[0] = "";
							for (i0 = 0; i0<blankCfLabels; i0++)
							{
								i1 = CfHeaderStart + (CfHeaderMax - Rf.MaxCondFormattingRules) + i0; // line to append to
								i2 = FormatHeaderLabel (segHeader, i1, 1, rfld.ColumnX,
									rfld.ColumnWidth, 1, labels, 1, Rf.GraphicsContext, 1, "", "", null, out formattedLabel);
								rfld.CondFmtHeaders.Add(formattedLabel);
							}

							if (cf != null && rfld.IsMainValue) do
							{
								int rulei;
								for (rulei = 0; rulei < cfRulesCount; rulei++) // defined items shifted down
								{
									Color bc  = cf.Rules[rulei].BackColor1;
									if (bc.R != Color.White.R || bc.G != Color.White.G || bc.B != Color.White.B) break;
								}
								if (rulei >= cfRulesCount) break; // if all background colors are white then don't include in header

								if (cfRulesCount > CfHeaderLines) 
									CfHeaderLines = cfRulesCount; // keep biggest

								for (rulei=0; rulei<cfRulesCount; rulei++) // defined items shifted down
								{
									CondFormatRule rule = cf.Rules[rulei];

									labels[0] = rule.ToString(mc, rulei);

									FontStyle fontStyle = FontStyle.Regular;
									if (rule.Font != null) fontStyle = rule.Font.Style;
									Font f2 = new Font(Rf.FontName, Rf.FontSize, fontStyle);
									CellStyleMx cellStyle = new CellStyleMx(f2, rule.ForeColor, rule.BackColor1);

									i1 = CfHeaderStart + rulei + (CfHeaderMax - cfRulesCount); // line to append to
									i2 = FormatHeaderLabel(segHeader, i1, 1, rfld.ColumnX,
										rfld.ColumnWidth, 1, labels, 1, Rf.GraphicsContext, 1, "", "", cellStyle, out formattedLabel);
									rfld.CondFmtHeaders.Add(formattedLabel);
								}
							} while (false);
						}

// All done if this is the last field of this segment

						if (fi<rt.Fields.Count-1) 
						{
							rfld2 = rt.Fields[fi+1];
							if (rfld2.ColumnX==Rf.CharWidth)
								break; 
						}
					}

					fi++;
				} // end of field

				if (singleResultsTable!=null || ti >= Rf.Tables.Count-1) break;
				ti++;

			} // end of table

			if (seg!=null) SegmentHeaderComplete(seg); // finish last segment

// Set the number of headers the same for each field

			foreach (ResultsTable rt0 in resultsTables)
			{
				foreach (ResultsField rf0 in rt0.Fields)
				{
					while (rf0.CondFmtHeaders != null && rf0.CondFmtHeaders.Count < CfHeaderLines)
					{
						rf0.CondFmtHeaders.Insert(0, new MobiusDataType());
					}
				}
			}

// Dump header information

//			SS.I.DebugFlags[0] = "1";
			if (SS.I.DebugFlags[0] != null) 
			{
				ClientLog.Message("Dump of Formatting Information");
				txt = String.Format("Tables.Count={0} Segments.Count={1} TitleCount={2} RepeatCount={3}",
					Rf.Tables.Count,Rf.Segments.Count,Rf.TitleBuffer.Count,Rf.RepeatCount);
				ClientLog.Message(txt);

				for (ti=0; ti<Rf.Tables.Count; ti++) 
				{
					rt = Rf.Tables[ti];
					mt = rt.MetaTable;
					txt = String.Format("Table: {0} Width: {1}",mt.Name,rt.Width);
					ClientLog.Message(txt);
					ClientLog.Message("Fi ColPos ColWdth FldPos FldWdth");
					for (fi=0; fi<rt.Fields.Count; fi++) 
					{
						rfld = rt.Fields[fi];
						txt = String.Format("{0,2} {1,6} {2,7} {3,6} {4,7}",
							fi,rfld.ColumnX,rfld.ColumnWidth,rfld.FieldX,rfld.FieldWidth);
						ClientLog.Message(txt);
					}
					ClientLog.Message("");
				}
				ClientLog.Message("Si Width FldCnt");
				for (si=0; si<Rf.Segments.Count; si++) 
				{
					seg = (ResultsSegment)Rf.Segments[si];
					txt = String.Format("{0,2} {1,5} {2,6}",si,seg.Width,seg.FieldCount);
					ClientLog.Message(txt);
					for (i1=0; i1<seg.Header.Length; i1++) 
					{ 
						ClientLog.Message(seg.Header[i1]);
					}
					ClientLog.Message("HorizontalLine: " + seg.HorizontalLine);
				}
			}
		}

/// <summary>
/// Finish setting up information for segment header
/// </summary>
/// <param name="rs"></param>

		void SegmentHeaderComplete(
			ResultsSegment seg)
		{
			////if (Rf.Grid) return; // not needed for grid

			int headerLines,pixels,i1,i2,i3;
			string [] hdrBuild;

			if (TableHeaderLabelCount==0) TableHeaderLines=0; 
			if (ColumnHeaderLabelCount==0) ColumnHeaderLines=0; 

			hdrBuild = new string [ // area to build final header in
				1 + TableHeaderLines + // 1st horizontal + table headers
				1 + ColumnHeaderLines + // 2nd horizontal + columns headers
				1 + CfHeaderLines + // 3rd horizontal + cond. format headers
			  1]; // final horizontal

			if (Rf.Grid)
			{
				headerLines = 2 + CfHeaderLines; // table header + col header + cf header
				//				i=0; 
//				if (tableHeaderLines>0 && (Rf.Tables.Count>1 || // do we want table headers?
//					!IsRootTable(Rf.Tables[0].MetaTable))) 
//				{ 
					hdrBuild[0]= seg.Header[1];
//					i++;
//				}

					if (CfHeaderLines > CfHeaderMax) CfHeaderLines = CfHeaderMax;
					for (i1 = 0; i1 < CfHeaderLines; i1++) // cf headers
						hdrBuild[1 + i1] = seg.Header[CfHeaderStart + i1 + (CfHeaderMax - CfHeaderLines)];

					hdrBuild[1 + CfHeaderLines] = seg.Header[ColumnHeaderStart]; // column header
			}

			else if (Rf.Excel || Rf.Word) 
			{
				headerLines=0; 

				//if (tableHeaderLines>0 && (Rf.Tables.Count>1 || // do we want table headers?
				//  !Rf.Tables[0].MetaTable.IsRootTable)) 
				if (Rf.HeaderLines >= 2) // table headers?
				{ 
					hdrBuild[headerLines]= seg.Header[1];
					headerLines++;
				}

				if (CfHeaderLines > 0) // header lines for conditional formatting
				{
					i2 = CfHeaderMax - CfHeaderLines; // number of lines not used in cf headers
					for (i3=0; i3<CfHeaderLines; i3++) 
					{ 
						hdrBuild[headerLines+i3] = seg.Header[CfHeaderStart+i2+i3];
					}

					headerLines+=CfHeaderLines; 
				}


				if (Rf.HeaderLines >= 1) // column headers?
				{
					hdrBuild[headerLines] = seg.Header[ColumnHeaderStart]; // column header
					headerLines++;
				}
			}

			else if (Rf.Html) 
			{
				i1=0;
				pixels = GraphicsMx.MilliinchesToPixels(seg.Width);
				hdrBuild[i1] = // use html table
					"E <table style=\"font: " + Rf.FontSize.ToString() + "pt " + Rf.FontName + "\" " +
					"width=" + pixels.ToString() + " " +
					"border=\"1\" bordercolor=\"#A2BAE9\" borderColorLight=\"#E3EFFF\" " + // light blue border
					"cellpadding=\"1\" cellspacing=\"0\" bgcolor=\"#FFFFFF\">\t";

				i1++;

				if (seg.Header[TableHeaderStart]!="") // table header exist
				{ 
					hdrBuild[i1] = // wrap HTML in a row
						"E <tr align=\"center\" valign=\"bottom\">" + seg.Header[TableHeaderStart+2] + " </tr>\t"; 
					i1++;
				}

				hdrBuild[i1] = // wrap column labels within a row 
					"E <tr align=\"center\" valign=\"bottom\">" + seg.Header[ColumnHeaderStart + 2] + " </tr>\t"; 
				i1++;

				headerLines=i1;
			}

			else if (Rf.TextFile || Rf.Spotfire) 
			{
				hdrBuild[0]=seg.Header[ColumnHeaderEnd]; 
				headerLines=ColumnHeaderLines; // one or none
			}

			else  // other output destinations
			{ 
				if (TableHeaderLines==0) headerLines=0; // table headers
				else 
				{
					hdrBuild[Horizontal1] = seg.Header[Horizontal1]; // top of table line
					i1 = TableHeaderMax - TableHeaderLines; // number of lines not used in table headers
					for (i3=0; i3<TableHeaderLines; i3++) 
					{ 
						hdrBuild[1+i3] = seg.Header[1+i1+i3];
					}
					headerLines=1 + TableHeaderLines;
				}

				if (CfHeaderLines > 0) // header lines for conditional formatting
				{
					hdrBuild[headerLines] = seg.Header[Horizontal2]; // horizontal divider between table & col headers 
					headerLines++;

					i2 = CfHeaderMax - CfHeaderLines; // number of lines not used in cf headers
					for (i3=0; i3<CfHeaderLines; i3++) 
					{ 
						hdrBuild[headerLines+i3] = seg.Header[CfHeaderStart+i2+i3];
					}

					headerLines+=CfHeaderLines; // total with lines for column headers
				}

				hdrBuild[headerLines] = seg.Header[Horizontal2]; // horizontal divider between table & col headers 
				headerLines++;

				i2 = ColumnHeaderMax - ColumnHeaderLines; // number of lines not used in column headers

				for (i3=0; i3<ColumnHeaderLines; i3++) 
				{ 
					hdrBuild[headerLines+i3] = seg.Header[ColumnHeaderStart+i2+i3];
				}
				headerLines+=ColumnHeaderLines; // total with lines for column headers

				if (TableHeaderLines>0 || ColumnHeaderLines>0) // add horizonal line after headers
				{ 
					hdrBuild[headerLines] = seg.Header[Horizontal3]; 
					headerLines++;
				}

				if ((Rf.TextFile && !SS.I.PrintHeaders) || Rf.Spotfire) 
					headerLines=0; // really dont want any header lines
			}

			// Count footer size for segment

			seg.FooterLineCount=0;
			if (Rf.Console)
				seg.FooterLineCount=1; 

			else if (Rf.Html) seg.FooterLineCount=1; // leave blank line for </table> code 
			else if (Rf.SdFile) headerLines = seg.FooterLineCount = 0;

// Reallocate header to actual size

			seg.Header = new string[headerLines];
			Array.Copy(hdrBuild,seg.Header,headerLines);

			return;
		}

/// <summary>
/// Select and format the "best" table or column header
/// </summary>
/// <param name="segHeader">Segment header array to be filled</param>
/// <param name="row1">Row in header to start filling</param>
/// <param name="rowsAvailable">Number of rows that can be used</param>
/// <param name="col1">Column to start filling</param>
/// <param name="columnWidth">Width available in column</param>
/// <param name="fieldCount">Count of subcolumns under label</param>
/// <param name="labels">Array of candidate labels</param>
/// <param name="labelCount">Count of candidate labels</param>
/// <param name="graphics">Graphics context</param>
/// <param name="justification">Justification: 0=left or 1=centered</param>
/// <param name="hyperlink">Optional link address to associate with label</param>
/// <param name="anchor">Name if this label is an anchor</param>
/// <param name="cellStyle">Any style to be applied to cell</param>
/// <param name="cellValue">Header value with attributes as a qualified number or chemical structure</param>
/// <returns></returns>

		int FormatHeaderLabel ( 
			string [] segHeader, 
			int row1, 
			int rowsAvailable,
			int col1, 
			int columnWidth, 
			int fieldCount, 
			string [] labels, 
			int labelCount, 
			GraphicsMx graphics,
			int justification,
			string hyperlink, 
			string anchor,
			CellStyleMx cellStyle,
			out MobiusDataType formattedLabel)  
		{
			MoleculeMx cs = null;
			int rowsUsed, columnWidth2, pixels, pass, skid, structureRows, i1, i2;
			string [] buf = new string[3];
			string txt;

			rowsUsed=0; 
			columnWidth2=columnWidth; 

			if (Rf.Word || Rf.TextFile || Rf.Spotfire) 
			{ 
				rowsUsed=1; // just one line
				columnWidth2=1000000; // no breaks 
			}

			else if (Rf.Excel) 
			{
				rowsUsed=1; // just one line
				columnWidth2=1000000; // no breaks 
			}

			else if (Rf.Html) 
			{
				rowsUsed=1; // just one line
				columnWidth2=1000000; // no breaks 
				col1=-1; // code to output simple text
			}

			else if (Rf.Grid) 
			{
				rowsUsed=1; // just 1 line 
			}

			else rowsUsed=3; // max of 3 lines 

// See if text structure with optional text label

			i1 = labels[0].IndexOf("\t"); // tab separates text label from structure
			if (i1 >= 0)
			{
				if (Rf.Grid)
					cs = new MoleculeMx(labels[0].Substring(i1 + 1));

				else // remove structure & treat as regular label
				{
					labels[0] = labels[0].Substring(0, i1);
					cs = null;
				}
			}

// Label with structure

			if (cs != null)
			{
				int textRows = ResultsFormatter.FormatTextField(labels[0].Substring(0, i1), Rf.OutputDestination, graphics, col1, columnWidth2, justification, buf);
				int totalRows = textRows;
				string formattedText = buf[0];
				if (formattedText.IndexOf("\t") < 0) // add control chars if not already included
					formattedText = "T 0 " + formattedText + "\t"; // plug in text

				ResultsFormatter fmtr = new ResultsFormatter(QueryManager);
				fmtr.ResultsFormat = Rf;
				FormattedFieldInfo ffi = fmtr.FormatStructure(cs, null, 's', col1, columnWidth2, -1); // add structure in same cell
				string formattedStruct = ""; // todo

				// single row positioned at top

				segHeader[row1] += formattedText; // label text on first row
				segHeader[row1] += formattedStruct; // and structure after it

				formattedLabel = cs;
			}

// Normal text label

			else
			{
				i1 = 0;

				StringMx formattedStringLabel = new StringMx();
				for (pass = 1; pass <= 4; pass++)
				{
					for (i1 = 0; i1 < labelCount; i1++)
					{
						if (pass >= 2) // smaller on 2nd pass
						{
							txt = labels[i1];
							txt.Replace("...", "");
							txt = txt.Substring(0, txt.Length / 2) + "...";
							labels[i1] = txt;
						}

						rowsUsed = ResultsFormatter.FormatTextField(labels[i1], Rf.OutputDestination, graphics, col1, columnWidth2, justification, buf);
						if (rowsUsed > 0) break;
					}
					if (i1 < labelCount) break; // success
				}

				if (i1 < labelCount) formattedStringLabel.Value = labels[i1];
				else // failed
				{
					formattedStringLabel.Value = buf[0] = "...";
					i1 = 1;
				}

				if (Rf.Excel || Rf.Word)
				{

					if (row1 == 1) // if table header include column merge information
					{
						segHeader[row1] += buf[0];

						if (buf[0] == "") i1 = 1;
						else i1 = 2;
						for (i2 = i1; i2 <= fieldCount; i2++)
							segHeader[row1] += "T 0 \t";
					}

					else if (cellStyle != null)
					{
						txt = "<CellStyle " + cellStyle.Serialize() + ">";
						i1 = buf[0].Substring(2).IndexOf(" ");
						if (i1 > 0) // insert formatting info just before text
							segHeader[row1] += buf[0].Substring(0, i1 + 3) + txt + buf[0].Substring(i1 + 3);
					}

					else segHeader[row1] += buf[0]; // just add it in
				}

				else if (Rf.Grid) { } // grid headers are built elsewhere, don't need to do anything here

				////{
				////  if (row1 == 1 && buf[0] == "") buf[0] = "."; // place holder
				////  else if (hyperlink != "") // show heading as hyperlinked
				////    buf[0] = "<a href=\"" + hyperlink + "\">" + buf[0] + "</a>";

				////  if (cellStyle != null) // apply style to cell?
				////    buf[0] = "<CellStyle " + cellStyle.Serialize() + ">" + buf[0];

				////  segHeader[row1] += "T 0 " + buf[0] + "\t"; 
				////  if (row1 == 1) // include duplicate spanning values if table header
				////  {
				////    for (i2 = 2; i2 <= fieldCount; i2++)
				////      segHeader[row1] += "T 0 " + buf[0] + "\t"; // blanks for other columns
				////  }
				////}

				else if (Rf.Html)
				{
					segHeader[row1] += buf[0]; // allow check for blank 
					if (buf[0] == "") buf[0] = "<br>";  // empty cell

					i1 = row1 + 2; // position for html
					if (row1 == 1) // include spanning with table header
						segHeader[i1] += "<td colspan=\"" + fieldCount.ToString() + "\" ";

					else // col header with width
					{
						pixels = GraphicsMx.MilliinchesToPixels(columnWidth + Rf.CharWidth); // use width in pixels
						segHeader[i1] += "<td width=\"" + pixels.ToString() + "\" ";
					}

					if (hyperlink != "") // show heading as hyperlinked
						buf[0] = "<a href=\"" + hyperlink + "\">" + buf[0] + "</a>";

					if (anchor != "") // save anchor if provided
						buf[0] = "<a name=\"" + anchor + "\"></a>" + buf[0];

					string bgColor = "E3EFFF"; // light-blue header background
					if (cellStyle != null) // set background color from cellStyle
						bgColor = String.Format("{0,2:X}{1,2:X}{2,2:X}",
							cellStyle.BackColor.R, cellStyle.BackColor.G, cellStyle.BackColor.B);

					segHeader[i1] += "bgcolor=\"#" + bgColor + "\">" + buf[0] + " </td>";
				}

				else // just append
					for (i1 = 0; i1 < rowsUsed; i1++)
					{
						segHeader[row1 + i1 + (rowsAvailable - rowsUsed)] += buf[i1];
					}

				formattedLabel = formattedStringLabel;
			}

// Set any style and hyperlink attributes

			if (cellStyle != null)
			{
				formattedLabel.ForeColor = cellStyle.ForeColor;
				formattedLabel.BackColor = cellStyle.BackColor;
			}

			formattedLabel.Hyperlink = hyperlink;

			return rowsUsed;
		}

/// <summary>
/// MergingIntoStructure
/// </summary>
/// <returns></returns>
		bool MergingIntoStructure ()
		{
			return false; // todo
		}

/// <summary>
/// ConsolidateMergedFields
/// </summary>
		void ConsolidateMergedFields ()
		{
			return; // todo
		}


	}
}
