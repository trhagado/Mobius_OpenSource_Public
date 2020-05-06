using Mobius.ComOps;
using Mobius.Data;
using Mobius.Helm;
using Mobius.ServiceFacade;

using Svg;

using System;
using System.IO;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Net;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Results Generator Executive
	/// </summary>

	public partial class ResultsFormatter
	{

		/*
 * This class formats the data results from a query, sends them to the output
 * destination and interacts with the user. The data is processed in the 
 * following series of steps.
 *
 * 1. FetchNextDataRow - Retrieves the next row from the query engine 
 * 
 * 2. FormatTuple - Formats the tuple into the TupleBuffer Tb. All of the
 *    segments for the tuple go into a single TupleBuffer with number of lines
 *    filled in each segment stored in ResultsSegment.
 * 
 * 3. MoveTupleBufferToIntermediateBuffer - The intermediate buffer (Ib) contains
 *    up to a page of tuples. For each segment ResultsSegment points to a
 *    chain of TupleBuffers. Each TupleBuffer in the chain contains the lines
 *    for a single tuple for that segment.
 * 
 * 4. MoveIntermediateBufferToPageBuffer - When the Ib can contain no more
 *    tuples it is moved to the PageBuffer Pb. Pb contains a single set of
 *    lines that are ready to output. 
 * 
 * 5. OutputPageBuffer - Pb is sent to the output destination. Pb may contain
 *    more than a single physical page in some cases. If so this method breaks 
 *    the logical page into multiple physical pages
 * 
 * 6. OutputRecord - Processes a "record" containing a single or multiple
 *    lines and sends it to the output device.
 */

		internal int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		// Inputs to formatter

		internal QueryManager QueryManager = null; // links together elements of query
		internal QueryManager Qm { get { return QueryManager; } }
		internal Query Query { get { return Qm.Query; } }
		internal ResultsFormat ResultsFormat
		{
			get { return Qm.ResultsFormat; }
			set { Qm.ResultsFormat = value; }
		}
		internal ResultsFormat Rf { get { return Qm.ResultsFormat; } } // ResultsFormat alias
		internal DataTableManager DtMgr { get { return Qm.DataTableManager; } } // DataTableManager alias
		internal DataTableMx DataTable { get { return Qm.DataTable; } }
		internal QueryEngine Qe { get { return Qm.QueryEngine; } }
		internal List<string> ResultsKeys { get { return DtMgr.ResultsKeys; } }

		// Public state information

		public bool FormattingStarted = false;
		public bool BrowseExistingResults = false;
		public DataRowMx CurrentDataRow;
		public int OutputRowCount; // count of rows sent to output destination

		internal int FirstSegmentHeaderSize = 0;

		// Fundamental results state data

		MoleculeGridControl Grid
		{ get { return ResultsFormat.MolGrid; } }

		bool CheckMarkInitially { get { return Grid == null ? false : Grid.CheckMarkInitially; } }

		internal bool StructureHighlightingInitialized = false;
		internal QueryColumn StructureHighlightQc; // query column containing any structure to hilight
		internal ParsedStructureCriteria StructureHighlightPssc; // parsed structure hilight criteria
		internal bool HighlightStructureMatches = true; // if true hilight structure matches
		internal bool AlignStructureToQuery = false;
		internal SmallWorldDepictions SmallWorldDepictions;
		internal static StructureMatcher StrMatcher; // structure matcher

		string CurrentKey = ""; // current compound number
		string PreviousKey = "";
		string FirstKey = "";

		string FirstPassExportOutputFile = ""; // first-stage output file if multiple stages (e.g. .csv file)
		string TempExcelFile = ""; // temp file used for building excel data
		StreamWriter TextStream;

		//static ResultsFormat ExportExcelRf = null; // most-recent export report formats
		//static ResultsFormat ExportWordRf = null;
		//static ResultsFormat ExportTextFileRf = null;
		//static ResultsFormat ExportSdFileRf = null;
		//static ResultsFormat ExportSpotfireRf = null;

		Hashtable TableRowCountMap = new Hashtable(); // count of rows for each table by name
		ArrayList TableRowCountList = new ArrayList();

		TupleBuffer Tb; // current tuple buffer row information
		int TbFree; // next free line in tb
		int TbCsFirstLine; // first line for current segment in tb
		int TbCsLineCount; // total line count for current segment in tb
		int TbCsLineCountFirst; // first table line count for current segment 
		int TbCsLineCountOther; // other table line count for current segment 

		PageBuffer Pb; // current page buffer
		int PageCount;
		int CurrentPage;
		int IbRowCount; // count of tuples in intermediate buffer
		int RepeatIdx;
		int RepeatColumnOffset;
		string RepeatColumnOffsetText; //  text of command to produce column offset 
		bool OutputBufferPending; // if true write buffer following this segment
		bool OutputHeader;
		bool OutputFooter;
		string SdfLine; // record being built for sdfile
		const int MAX_REPEAT_COLUMNS = 32;
		StringBuilder[] Html = new StringBuilder[MAX_REPEAT_COLUMNS]; // html indexed by column repeat (note: StringBuider is much faster here than String)
		string PopupHtml; // html for subquery
		bool GenerateToc = false; // generate table of contents
		string HtmlToc; // Html table of contents for all data page
		List<CellGraphic> CellGraphics = new List<CellGraphic>(); // current cell graphics array
		string PendingGraphicsCommand; // graphics image command built here
		int ImageCount; // numbering of graphs sent to client 
		int GraphLines; // height in lines for pending graph
		int MolCount; // number of structures sent
		int MolFailureCount; // number of molecules that failed formatting
		int TextLines; // height in lines for pending text

		bool RestartDisplay; // restart display if true
		int DisplayInterruptedPage; // page to restart display at
		bool OutputCancelled; // user has cancelled report output

		DisplayBuffer FirstDisplayBuffer; // cached display buffers
		DisplayBuffer LastDisplayBuffer;
		DisplayBuffer CurrentDisplayBuffer;

		const int VERT_LINE_MAX = 128;
		int[] VertLineX = new int[VERT_LINE_MAX]; // Column framing data
		int[] VertLineY = new int[VERT_LINE_MAX];
		int VertLineCount;

		string ReturnMsg = "";

		/// <summary>
		/// Constructor
		/// </summary>

		public ResultsFormatter()
		{
			MobiusDataType.FormatHyperlinkDelegateInstance = FormatHyperlink; // callback for hyperlink formatting

			return;
		}

		/// <summary>
		/// Constructor that assigns a QueryManager
		/// </summary>
		/// <param name="qm"></param>

		public ResultsFormatter(QueryManager qm)
		{
			if (qm == null) return;

			qm.ResultsFormatter = this;
			this.QueryManager = qm;

			MobiusDataType.FormatHyperlinkDelegateInstance = FormatHyperlink; // callback for hyperlink formatting
		}

		/// <summary>
		/// Format results
		/// </summary>

		public string BeginFormatting()
		{
			return BeginFormatting(false);
		}

		/// <summary>
		/// Format results
		/// </summary>
		/// <param name="reuseExistingResults">If true browse existing results</param>
		/// <returns></returns>

		public string BeginFormatting(
				bool browseExistingResults)
		{
			int ti;
			int rc;
			string tok, response, responseMixedCase, newResponse = "";
			QueryTable qt;
			QueryColumn qc;
			ResultsTable rt;
			ResultsField rfld;
			MetaTable mt;
			MetaColumn mc;
			string txt;
			DialogResult dr;
			bool success;
			CellGraphic cg;
			Lex lex = new Lex();
			int gi, i1, i2;

			if (Rf == null)
				throw new Exception("QueryExec.Run - Null ResultsFormat");

			if (Rf.Segments == null)
				throw new Exception("QueryExec.Run - ResultsFormat.Segments");

			if (Query == null)
				throw new Exception("QueryExec.Run - Null Rf.Query");

			if (Query.Tables == null || Query.Tables.Count <= 0)
				throw new Exception("QueryExec.Run - No Query Tables");

			if (Rf.Segments.Count > 0 && Rf.Segments[0].Header != null)
				FirstSegmentHeaderSize = Rf.Segments[0].Header.Length;

			BrowseExistingResults = browseExistingResults;

			if (browseExistingResults)
			{
				goto ProcessTuples;
			}

			Rf.TablesExtra = new List<ResultsTable>(); // extra tables added by pivoting

			//			AllocateBuffers(); // allocate tuple & vo buffers
			if (!Rf.Grid) // get 1st tuple unless grid
			{
				Progress.Show("Retrieving data...");
				DtMgr.StartRowRetrieval();
				CurrentDataRow = Qm.DataTableManager.FetchNextDataRow(); // get the first tuple
				Progress.Hide();

				if (CurrentDataRow == null) return ""; // no data
			}
			if (Rf.Search) return ""; // no formatting to do;

			PreviousKey = FirstKey = "";
			OutputCancelled = false;

			if (Rf.Html && Query.Tables.Count > 3) GenerateToc = true; // generate table of contents if HTML output
			HtmlToc = "";

			// Initialize for each type of output

			CurrentDisplayBuffer = FirstDisplayBuffer = LastDisplayBuffer = null;
			//Graphics.SetFont(Rf.FontName, Rf.FontSize);
			OutputHeader = true;
			PendingGraphicsCommand = "";
			OutputRowCount = 0;
			PageCount = 0;

			CellGraphics = new List<CellGraphic>(); // current cell graphics array
			MolCount = MolFailureCount = 0;
			ImageCount = 0; // numbering of graphs sent to client 

			// Grid

			if (Rf.Grid) { } // nothing more to do here

			// MS Word init

			else if (Rf.Word)
			{
				AdjustOutputFileName(".docx");

				tok = WordOp.Call("CreateObject");
				if (tok != "")
				{
					Progress.Hide();
					throw new Exception("Error Connecting to MS Word: " + tok);
				}

				FormatWordColumnsAndHeaders();
				UsageDao.LogEvent("QueryMsWord");
			}

			// Excel 

			else if (Rf.Excel)
			{
				string firstXlsFile = AdjustOutputFileName(".xlsx");

				if (File.Exists(firstXlsFile)) File.Delete(firstXlsFile); // delete so we don't get overwrite prompt on save

				FirstPassExportOutputFile = // write .csv file in first pass
						TempFile.GetTempFileName(ClientDirs.TempDir, ".csv");
				TextStream = new StreamWriter(FirstPassExportOutputFile);
				if (TextStream == null) throw new Exception("Unable to open Excel temp file: " + FirstPassExportOutputFile);
				UsageDao.LogEvent("QueryExcel", "");
			}

			// Text file

			else if (Rf.TextFile)
			{
				string firstFile = AdjustOutputFileName(".txt");
				TextStream = new StreamWriter(firstFile);
				if (TextStream == null) throw new Exception("Error opening output file");
				if (SS.I.QueryTestMode)
				{
					/*
					//prepend Query version-related info
					TextStream.WriteLine("FolderId: "+Query.UserObject.FolderId);
					TextStream.WriteLine("Id: "+Query.UserObject.Id);
					TextStream.WriteLine("Name: "+Query.UserObject.Name);
					TextStream.WriteLine("Content: "+Query.UserObject.Content);
					TextStream.WriteLine("UpdateDateTime: "+Query.UserObject.UpdateDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
					//TextStream.WriteLine("SQL: "+SS.I.QueryTestSQL);
					//TextStream.WriteLine("CRC32 Checksum: ABCDEFGH"); //dummy value to hold the value in hex once it's calculated -- probably not worth the trouble
					TextStream.WriteLine("-=+=-=+=-=+=-=+=-=+=-=+=-=+=-=+=-=+=-=+=-=+=-=+=-=+=-=+=-=+=-");
*/
					//prepend one line of meta-colmn/meta-table info
					//assume that Rf.CollumnCount > 0
					//JES -- TextStream.WriteLine();
				}
				UsageDao.LogEvent("QueryTextFile", "");
			}

			// SDfile 

			else if (Rf.SdFile)
			{
				string firstFile = AdjustOutputFileName(".sdf"); // was "txt"
				TextStream = new StreamWriter(firstFile);
				if (TextStream == null) throw new Exception("Error opening output file");
				SdfLine = "";
				UsageDao.LogEvent("QuerySdFile", "");
			}

			// Html

			else if (Rf.Html)
			{
				UsageDao.LogEvent("QueryHtml", "");
			}

			else throw new Exception("Unexpected output destination: " + Rf.OutputDestination);

			// Allocate & set up tuple buffers

			Tb = new TupleBuffer(); // initial building gets done here
			Tb.Lines = new string[10];
			TbFree = 0;

			// Allocate & set up page buffer

			Pb = new PageBuffer();
			Pb.Lines = new string[10];
			Pb.Free = 0;
			for (i1 = 0; i1 < Rf.TitleBuffer.Count; i1++) // copy title to top of buffer
			{
				IncrementPbFree();
				Pb.Lines[i1] = (string)Rf.TitleBuffer[i1];
			}
			Pb.LineCount = Rf.TitleBuffer.Count;
			Pb.RowCount = 0;

			RepeatIdx = 0;
			RepeatColumnOffsetText = "";

			InitializeIntermediateBuffer();

			//			int rti = 0; // report table index
			//			rt = Rf.Tables[rti];
			//			qt = rt.QueryTable;
			//			mt = rt.MetaTable;

			CurrentKey = "";

		ProcessTuples:

			FormattingStarted = true; // indicate that formatting has started

			// Loop through the tuples retrieved by the query. Tables within the tuple are
			// processed one at a time across the segments of the report.

			if (Rf.OutputDestination == OutputDest.WinForms && Rf.Tables.Count > 0) // outputting table data to grid?
			{
				if (!browseExistingResults && Grid != null)
					Grid.StartDisplay(); // need to start display if not redisplaying existing data

				return "";
			}

			////////////////////////////////////////////////////////////////////////////////
			// Output to all other devices
			////////////////////////////////////////////////////////////////////////////////

			else
			{

				// Loop through rows of data
				int rows = 0;
				while (CurrentDataRow != null)
				{
					CurrentKey = DtMgr.GetRowKey(CurrentDataRow); // get current compound number

					if (PreviousKey != "" && CurrentKey != PreviousKey && GenerateToc) // generate toc if requested
					{
						HtmlToc = GetHtmlToc();
						TableRowCountMap = new Hashtable();
						TableRowCountList = new ArrayList();
					}

					FormatRow(DtMgr.DataTableFetchPosition);
					if (!MoveTupleBufferToIntermediateBuffer())
					{
						MoveIntermediateBufferToPageBuffer();
						if (OutputCancelled) goto OutputComplete;
						InitializeIntermediateBuffer();
						TbFree = 0;
						FormatRow(DtMgr.DataTableFetchPosition);
						if (!MoveTupleBufferToIntermediateBuffer())
							throw new Exception("MoveTupleBufferToIntermediateBuffer failed on 2nd try");
					}

					if (Rf.Word || Rf.Excel || Rf.TextFile || Rf.SdFile) // output rows single
					{
						MoveIntermediateBufferToPageBuffer();
						if (OutputCancelled) goto OutputComplete;
						InitializeIntermediateBuffer();
					}

					if (Rf.SdFile && SdfLine != "") // output sdfile record
																					// && (!DtMgr.MoreDataRowsAvailable || CurrentKey != PreviousKey || !SS.I.CheckCnUniqueness))
					{
						TextStream.WriteLine(SdfLine + "$$$$");
						SdfLine = "";
					}

					if (FirstKey == "") FirstKey = CurrentKey;
					PreviousKey = CurrentKey;

					rows++;
					CurrentDataRow = DtMgr.FetchNextDataRow();
					if (CurrentDataRow == null)
						break;

				}

				// Search complete (or cancelled), output any partial page

				if (GenerateToc)
					HtmlToc = GetHtmlToc();

				MoveIntermediateBufferToPageBuffer();

			OutputComplete:

				//if (QueryManager.MoleculeGrid != null) 
				//QueryManager.MoleculeGrid.EndUpdate2();

				// Html to popup window

				if (Rf.Html && Rf.PopupOutputFormContext)
				{
					PopupHtml = // build full html
							GetHtmlHeader() + // header
							GetHtmlInaccesableDataMessage() + // message about any unavailable data
							HtmlToc + // any table of contents
							PopupHtml + // data content
							GetHtmlEnd(); // footer

					if (Query.ResultsPages.Pages.Count > 1 && // show as full PopupResults form
							Security.IsAdministrator(SS.I.UserName) ||
							ServicesIniFile.ReadBool("PopupResultsFormEnabled", true))
					{
						PopupResults.ShowHtml(Query.QueryManager as QueryManager, PopupHtml, Rf.Title);
					}

					else // show as simple PopupHtml form
					{
						UIMisc.ShowHtmlPopupFormDocument(PopupHtml, Rf.Title);
					}
				}

				// MS Word 

				else if (Rf.Word)
				{
					FormatWordColumnsAndHeadersPart2();
					Progress.Show("Saving Word file...");
					WordOp.Call("SaveAs", Rf.FirstOutputFileName);
					WordOp.Call("Close");
					WordOp.Call("Quit");
					WordOp.Call("DeleteObject");

					UploadExportFileAsNeeded();

					if (!Rf.RunInBackground)
					{
						Progress.Hide();

						//dr = MessageBoxMx.Show(StringMx.FormatIntegerWithCommas(OutputRowCount) + " rows were written to the Word file.\n" +
						//	"Open " + Rf.OutputFileName + "?", UmlautMobius.String,
						//	MessageBoxButtons.YesNo, MessageBoxIcon.Question);
						dr = MessageBoxMx.Show(StringMx.FormatIntegerWithCommas(rows) + " rows were written to the Word file.\n" +
								"Open " + Rf.OutputFileName + "?", UmlautMobius.String,
								MessageBoxButtons.YesNo, MessageBoxIcon.Question);
						if (dr == DialogResult.Yes)
						{
							SystemUtil.StartProcess(Rf.OutputFileName);
						}
					}
				}

				// Excel

				else if (Rf.Excel)
					do
					{
						string tempSketchFile = null, templateOriginal = null, tempFileExt = null;
						double sleft = 0, stop = 0, sright = 0, sbottom = 0;
						int dx, dy;
						bool debugExcelOp = ExcelOp.LogCalls;

						TextStream.Close(); // close initial 
						if (OutputCancelled) // free any cell graphics data
						{
							CellGraphics = new List<CellGraphic>();
							break;
						}

						Progress.Show("Starting Excel...");
						try { ExcelOp.CreateObject(); }
						catch (Exception ex)
						{
							Progress.Hide();
							MessageBoxMx.Show("Error Connecting to Excel: " + ex.Message, UmlautMobius.String);
							break;
						}

						Progress.Show("Formatting Excel worksheet...");

						templateOriginal = // default Excel template file
								CommonConfigInfo.MiscConfigDir + @"\MobiusExcelTemplate.xlsm";

						tempFileExt = ".xlsm"; // default to temp file with macros extension

						if (CellGraphics.Count > 0 && // need to insert Insight structures
								 Rf.ExportStructureFormat == ExportStructureFormat.Insight)
						{
							if (ExcelOp.IsInsightForExcelAvailable)
							{
								templateOriginal = // use empty template file (with no macros)
										CommonConfigInfo.MiscConfigDir + @"\MobiusInsight4XLTemplate.xlsx";
								tempFileExt = ".xlsx"; // no macro extension
							}

							else Rf.ExportStructureFormat = ExportStructureFormat.Metafiles; // just export images otherwise
						}

						TempExcelFile = TempFile.GetTempFileName(tempFileExt);
						File.Copy(templateOriginal, TempExcelFile, true);

						if (debugExcelOp)
						{
							DebugLog.Message("IsInsightForExcelAvailable: " + ExcelOp.IsInsightForExcelAvailable + ", TemplateOriginal: " + templateOriginal +
								", TemplateTempExcelFile: " + TempExcelFile + ", Exists: " + File.Exists(TempExcelFile));
						}

						try // open the copy of the template file
						{
							System.Threading.Thread.Sleep(3000); // debug - see if a bit of sleep helps on new PRD server
							ExcelOp.Open(TempExcelFile);
						}

						catch (Exception ex)
						{
							Progress.Hide();
							string msg = "Error opening copy of Excel template: ";
							MessageBoxMx.Show(msg + ex.Message, UmlautMobius.String);
							ServicesLog.Message(msg + DebugLog.FormatExceptionMessage(ex));
							break;
						}

						if (debugExcelOp)
							MessageBoxMx.Show("After ExcelOp.Open");

						try { ExcelOp.ImportCsv(FirstPassExportOutputFile); }
						catch (Exception ex)
						{
							Progress.Hide();
							string msg = "Error importing FirstPassExportOutputFile: ";
							MessageBoxMx.Show(msg + ex.Message, UmlautMobius.String);
							ServicesLog.Message(msg + DebugLog.FormatExceptionMessage(ex));
							break;
						}

						if (debugExcelOp)
							MessageBoxMx.Show("After ExcelOp.ImportCsv");

						FormatExcelColumnsAndHeaders();
						MergeExcelTableHeaders();

						if (debugExcelOp)
							MessageBoxMx.Show("After FormatExcelColumnsAndHeaders");

						//ExcelOp.PrintGridlines(true); 

						if (CellGraphics.Count > 0) // structures/graphics to insert
						{
							//ClientLog.Message("CellGraphics.Count: " + CellGraphics.Count); // debug

							for (gi = 0; gi < CellGraphics.Count; gi++)
							{
								cg = CellGraphics[gi];
								//ClientLog.Message("CellGraphics " + gi + ": " + cg.Row + ", " + cg.Col); // debug
								if (Progress.IsTimeToUpdate)
								{
									Progress.Show("Inserting Excel Cell Formatting: " + gi.ToString());
									if (Progress.CancelRequested) break;
									//ExcelOp.ScreenUpdating(false);
								}

								ExcelOp.RangeSelect(ColumnNumberToColumnLetter(cg.Col) + cg.Row); // select cell

								txt = String.Format("{0:F2}", cg.RowHeight);
								ExcelOp.RowHeight(cg.RowHeight); // set proper row height

								// Insert molecule as object, image or string as appropriate

								MoleculeMx mol = cg.Molecule;

								if (MoleculeMx.IsDefined(mol))
								{
									MolCount++;

									// Insert chem structure as object or image

									if (mol.IsChemStructureFormat)
									{

										if (Rf.ExportStructureFormat == ExportStructureFormat.Insight)
											try // export in Insight format
											{
												ExcelOp.SetCellMolecule(cg.Row, cg.Col, cg.Molecule);
											}
											catch (Exception ex)
											{
												MolFailureCount++;
												if (ClientState.IsDeveloper)
													ClientLog.Message("MolFailureCount: " + MolFailureCount + "\n\n" + DebugLog.FormatExceptionMessage(ex)); // debug
											}

										else // insert molecule as metafile image
										{
											try
											{
												int width = cg.Width; // GraphicsMx.PointsToPixels(cg.Width);
												int height = 10000; // GraphicsMx.PointsToPixels(10000);
												Metafile mf = mol.CdkMol.GetMetafile(width, height);
												if (mf == null) continue;

												string tempFileName = TempFile.GetTempFileName(".wmf");
												mf.Save(tempFileName);

												width = mf.Width;
												height = mf.Height;

												ExcelOp.InsertPicture(tempFileName, 0, 0, width, height, 1.0F);
												try { File.Delete(tempFileName); } catch { }
												//ExcelOp.ShapeRangeHeight(cg.Height);
												//ExcelOp.ShapeRangeWidth(cg.Width);

												//ExcelOp.Call("FillVisible", false);
												//ExcelOp.Call("LineVisible", false);
											}

											catch (Exception ex)
											{
												MolFailureCount++;
												if (ClientState.IsDeveloper)
													ClientLog.Message("MolFailureCount: " + MolFailureCount + "\n\n" + DebugLog.FormatExceptionMessage(ex)); // debug
											}
										}

									}

									// Format & insert Helm depiction

									else if (mol.PrimaryFormat == MoleculeFormat.Helm)
									{
										SvgDocument doc;
										RectangleF bb;
										int width, width2, width3, height, height2, height3;

										int ptWidth = (int)cg.Width; // desired width in points (not GraphicsMx.PointsToPixels(cg.Width);)

										if (Lex.IsUndefined(mol?.HelmString)) continue;

										string svg = HelmControl.GetSvg(mol.HelmString);
										if (Lex.IsUndefined(svg)) continue;

										string svg2 = SvgUtil.AdjustSvgToFitContent(svg, ptWidth, SvgUnitType.Point, out doc);
										if (Lex.IsUndefined(svg2)) continue;

										dx = 0;
										dy = 0;

										width = -1;
										width2 = cg.Width; 
										width3 = (int)doc.Width;

										height = -1; 
										height2 = (int)cg.RowHeight;
										height3 = (int)doc.Height;

										float scale = (float)WindowsHelper.GetMonitorHighDpiScalingFactor(); // proper handle scaling for high DPI monitor settings

										string tempFileName = TempFile.GetTempFileName(".svg");
										FileUtil.WriteFile(tempFileName, svg2);

										ExcelOp.InsertPicture(tempFileName, dx, dy, width2, height2, scale);
										try { File.Delete(tempFileName); } catch { }
									}

									// Other format, just output molString value as text into the cell

									else
									{
										ExcelOp.SetCellText(cg.Row, cg.Col, mol.PrimaryValue);
									}

								}

								// Insert graphic image

								else if (cg.GraphicsFileName != "")
								{
									cg = CellGraphics[gi];
									dx = 3; // move 3 pts right so grid line is visible
									dy = (int)(cg.RowHeight - cg.Height); // shift down to provide space for text
									dy -= 3; // move 3 pts down so grid line is visible
									ExcelOp.InsertPicture(cg.GraphicsFileName, dx, dy, cg.Width, cg.Height);
									//ExcelOp.ShapeRangeHeight(cg.Height);
									//ExcelOp.ShapeRangeWidth(cg.Width);
									//ExcelOp.ShapeRangeMoveRelative(dx, dy);
								}

								// Insert cell style

								else if (cg.CellStyle != null)
								{
									cg = CellGraphics[gi];
									CellStyleMx cs = cg.CellStyle;
									Font f = cs.Font;

									ExcelOp.FontAttributes(f);

									if (cs.ForeColor != Color.White)
										ExcelOp.FontColor(cs.ForeColor);

									if (cs.BackColor != Color.White)
										ExcelOp.BackColor(cs.BackColor);
								}
							}
						}

						ExcelOp.RemoveModules(); // remove modules to avoid security warning

						Progress.Show("Saving Excel file...");

						ExcelOp.SaveAs(Rf.FirstOutputFileName);
						ExcelOp.Close();
						//ExcelOp.ScreenUpdating(true); // reactivate screen updating for user
						ExcelOp.Quit();
						ExcelOp.DeleteObject();

						File.Delete(FirstPassExportOutputFile);

						UploadExportFileAsNeeded();

						if (!Rf.RunInBackground)
						{
							Progress.Hide();

							var rowCount = OutputRowCount - Rf.HeaderLines;
							//string msg = OutputRowCount + " rows were written to the Excel worksheet.\n";
							string msg = rows + " rows were written to the Excel worksheet.\n";
							if (MolFailureCount > 0)
								msg += "\nERROR: " + MolFailureCount + " chemical structures could not be written to Excel.\n\n";
							msg += "Open " + Rf.OutputFileName + "?";
							dr = MessageBoxMx.Show(msg, UmlautMobius.String,
									MessageBoxButtons.YesNo, MessageBoxIcon.Question);
							if (dr == DialogResult.Yes)
							{
								SystemUtil.StartProcess(Rf.OutputFileName);
							}
						}

					} while (false);

				// Text file

				else if (Rf.TextFile)
				{
					TextStream.Close();

					UploadExportFileAsNeeded();

					if (!Rf.RunInBackground)
					{
						//ReturnMsg = StringMx.FormatIntegerWithCommas(OutputRowCount - 1) + " data records have been written to " + Rf.OutputFileName;
						ReturnMsg = StringMx.FormatIntegerWithCommas(rows) + " data records have been written to " + Rf.OutputFileName;
					}
				}

				// SdFile or Smiles

				else if (Rf.SdFile)
				{
					if (!String.IsNullOrEmpty(SdfLine)) TextStream.WriteLine(SdfLine + "$$$$");
					TextStream.Close();

					UploadExportFileAsNeeded();

					if (!Rf.RunInBackground)
					{
						//ReturnMsg = StringMx.FormatIntegerWithCommas(OutputRowCount) + " data records have been written to " + Rf.OutputFileName;
						ReturnMsg = StringMx.FormatIntegerWithCommas(rows) + " data records have been written to " + Rf.OutputFileName;
					}
				}

				UpdateCurrentCountDisplay();
				if (Qm.StatusBarManager != null)
					Qm.StatusBarManager.DisplayStatusMessage("");
				Progress.Hide();

				return ReturnMsg;
			} // end of processing for output to other than grid
		} // End of Run

		/// <summary>
		/// Adjust the name of the file to write to initially
		/// </summary>
		/// <param name="ext"></param>

		string AdjustOutputFileName(string ext)
		{
			if (Rf.OutputFileName.StartsWith("http") && !Rf.RunInBackground) // write to temp file first if going to SharePoint & not background
			{
				Rf.OutputFileName2 = TempFile.GetTempFileName(ClientDirs.TempDir, ext); // put in secondary file name
				return Rf.OutputFileName2;
			}

			else if (Rf.OutputFileName == "") // get temp file name
				Rf.OutputFileName = TempFile.GetTempFileName(ClientDirs.TempDir, ext);

			else if (!Rf.OutputFileName.Contains(@"\")) // add directory if needed for background export
				Rf.OutputFileName = ServicesIniFile.Read("BackgroundExportDirectory") + @"\" + Rf.OutputFileName;

			Rf.OutputFileName2 = "";
			return Rf.OutputFileName;
		}

		/// <summary>
		/// Upload file to SharePoint as needed
		/// </summary>

		void UploadExportFileAsNeeded()
		{
			if (Lex.IsNullOrEmpty(Rf.OutputFileName2) || !Lex.StartsWith(Rf.OutputFileName, "http"))
				return;

			SharePointUtil.CopyToSharePoint(Rf.OutputFileName2, Rf.OutputFileName);
			return;
		}

		/// <summary>
		/// Export data to spotfire 
		/// </summary>
		/// <returns></returns>

		QueryEngineStats ExportToSpotfire()
		{
			ExportParms ep = Rf.CopyToExportParms();
			QueryEngineStats qeStats = Qm.QueryEngine.ExportDataToSpotfireFiles(ep);
			return qeStats;
		}

		/// <summary>
		/// Format a DataRow into the initial formatted tuple buffer
		/// </summary>
		/// <param name="tuple"></param>

		void FormatRow(
				int dri)
		{
			int si, ti, fi, dri2;
			int csFieldCount, saveCsFieldCount = 0, saveSi = 0;
			bool segmentIsEmpty, saveSegmentIsEmpty = false;
			DataRowMx dr, saveT = null;
			bool summaryTable;
			int r, x;
			object fieldValue;
			ResultsTable rt, saveRt;
			ResultsField rfld;
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			ResultsSegment seg;
			object obj;
			bool sameKey;
			bool suppress, mergedValues;
			string delim, txt, tok;

			int i1, i2;

			// Initialize segments

			for (si = 0; si < Rf.Segments.Count; si++)
			{
				seg = Rf.Segments[si];
				seg.TempLines = seg.TempLinesFirst = seg.TempLinesOther = 0;
			}

			// Start at first segment

			si = 0;
			seg = Rf.Segments[si];
			csFieldCount = 0; // current segment field count
			TbFree = 0;
			TbCsFirstLine = seg.TempBegin = TbFree;
			r = TbFree; // first line for segment
			TbCsLineCount = TbCsLineCountFirst = TbCsLineCountOther = 0;
			segmentIsEmpty = true;
			RowCollectionMx dtRows = Qm.DataTable.Rows;

			// Process each table

			for (ti = 0; ti < Rf.Tables.Count; ti++)
			{
				dri2 = DtMgr.AdjustDataRowToCurrentDataForTable(dri, ti, false);

				if (dri2 < 0)
					dr = null;
				else if (dri2 < dtRows.Count)
					dr = dtRows[dri2];
				else throw new Exception("Index out of range: rows = " + dtRows.Count + ", dri=" + dri + ", dri2=" + dri2 + ", ti=" + ti + "/" + Rf.Tables.Count);

				rt = Rf.Tables[ti];
				qt = rt.QueryTable;
				mt = rt.MetaTable;

				// Do special setup for summary tables

				summaryTable = false;
#if false
				if (mt.MultiPivot && t.Fresh(ti)) 
				{
					ResultsTable newRt = null;
					FlatRow newFlatRow = null;
					int newSi = 0;

					if (!SetupSummaryTableRow(rt, t, ref newRt, ref newFlatRow, ref newSi))
					{
						t.Fresh(ti) = false; // ignore the data
						continue;
					}

					seg.TempLines=TbCsLineCount; // save current state
					seg.TempLinesFirst=TbCsLineCountFirst; 
					seg.TempLinesOther=TbCsLineCountOther; 
					saveSegmentIsEmpty=segmentIsEmpty;
					saveCsFieldCount=csFieldCount;
					saveRt = rt;
					saveSi = si;
					saveT = t;

					rt = newRt; // setup new table, tuple & seg
					t = newFlatRow;
					si = newSi;
					qt = rt.QueryTable;
					mt = rt.MetaTable;
					summaryTable = true;
					seg = Rf.Segments[si]; // set segment
					r = seg.TempBegin = TbFree; // treat as new segment
					TbCsLineCount = TbCsLineCountFirst = TbCsLineCountOther = 0;
					csFieldCount=0; 
					TbCsLineCount=0;
					segmentIsEmpty=true;
				}
#endif

				// Count row for table if retrieving all data & creating table of contents

				if (DataTableManager.RowHasDataForTable(dr, ti) && GenerateToc)
				{
					obj = TableRowCountMap[mt.Name];
					if (obj == null)
					{
						TableRowCountMap.Add(mt.Name, 1);
						TableRowCountList.Add(mt.Name);
					}
					else TableRowCountMap[mt.Name] = (int)obj + 1;
				}

				// Process each field for the table

				for (fi = 0; fi < rt.Fields.Count; fi++)
				{
					rfld = rt.Fields[fi]; // get field info
					x = rfld.FieldX;
					qc = rfld.QueryColumn;
					mc = rfld.MetaColumn;

					if (dr != null) fieldValue = dr[rfld.VoPosition];
					else fieldValue = DBNull.Value;

					if (SS.I.DebugFlags[2] != null) // dump state
					{
						if (fieldValue == null) tok = "<null>";
						else tok = fieldValue.ToString();
						txt = String.Format("si={0}, csFieldCount={1}, ti={2}" +
								" t.Fresh(ti)={3}, fi={4}, mc={5}, value={6}, r={7}",
								si, csFieldCount, ti, DataTableManager.RowHasDataForTable(dr, ti), fi, mc.Name, tok, r);
						ClientLog.Message(txt);
					}

					// Special case output of cmpdNo for first key field in seg when no fresh data for table

					if (!Rf.CompoundOriented && // not compound oriented
																			//						ti>0 && // beyond first table (do for all tables now)
							!DataTableManager.RowHasDataForTable(dr, ti) && // but no data for this table
							fi == 0 && // first field for table
							mc.IsKey && // key field
							csFieldCount == 0) // segment empty
					{
						fieldValue = CurrentKey;
					}

					// Duplicate structures (handled better elsewhere now)

					//if (/* Rf.DuplicateKeyValues &&  */ // request to duplicate key field values
					//  ti == 0 && // first (key) table
					//  // mc.DataType == MetaColumnType.Structure && // (dup all key fields)
					//  NullValue.IsNull(fieldValue))
					//{ // get value from first row for key
					//  DataRowAttributes dra = DtMgr.GetRowAttributes(dr);
					//  int keyRowIndx = dra.FirstRowForKey;
					//  //if (keyRowIndx >= DtMgr.DataTable.Rows.Count) keyRowIndx = keyRowIndx; // debug
					//  DataRowMx dr2 = DtMgr.DataTable.Rows[keyRowIndx];
					//  fieldValue = dr2[rfld.VoPosition];
					//}

					// Append any merged fields to current field

					txt = "";
					txt = AppendMergedFields(rt, fi, dr, txt);
					if (txt != "") mergedValues = true;
					else mergedValues = false;

					// Supress values in the following cases:
					//   1. If this is a duplicate key value and not the 1st tuple on page.
					//   2. Data is not fresh and not the first key value on a new page.
					//   3. Compound oriented report & data not fresh & not 1st tuple on the page.

					if (csFieldCount == 0 && // nothing in segment
							Rf.TableOriented &&
							Rf.RowFraming &&
							IbRowCount > 0) // something in intermediate buffer
					{
						sameKey = false; // say not the same to start

						if (mc.IsKey && // key field
								!Rf.Html) // every row gets framed for html
						{
							if (fieldValue == null) txt = "";
							else txt = (string)fieldValue;
							if (txt == PreviousKey) // same key value as last time
								sameKey = true;
						}

						if (ti == 0 && !DataTableManager.RowHasDataForTable(dr, ti)) sameKey = true; // additional row for root table

						if (sameKey && SS.I.CheckCnUniqueness && !mergedValues) goto FieldDone; // skip if same key value and normal unique rn mode

						else  // new value, add frame to temp buffer
						{
							if (seg.IbLines > 0) // existing lines for segment?
							{
								IncrementTbFree(0); // starting line counts towards all tables
								Tb.Lines[r] = seg.HorizontalLine; // row framing line
							}
							r = TbFree; // next row to fill
							IncrementTbFree(ti); // be sure available
						}
					}

					// Ignore fields after key field if data for table is not fresh 

					if ((csFieldCount > 0 || fi > 1) && !DataTableManager.RowHasDataForTable(dr, ti) && !Rf.CompoundOriented && !mergedValues)
						while (true) // execute the following at most once
						{
							//if (mt.IsRootTable && Rf.DuplicateKeyValues) break; // ok if duplicat of key table

							if (Rf.Word || Rf.Excel || Rf.Grid || Rf.TextFile) // insert place holders for these
							{
								AssureTbFree(ti, r);
								Tb.Lines[r] += "t " + x.ToString() + "  \t";
							}

							else if (Rf.Html && !segmentIsEmpty) // need html spacers if non-null
							{
								AssureTbFree(ti, r);
								Tb.Lines[r] += "t 0 <br>\t";
							}

							goto FieldDone;
						}

					// If the report is compound oriented and this is the only table for the segment then
					// suppress all non-null fields except first key field for table 

					if (Rf.CompoundOriented && !DataTableManager.RowHasDataForTable(dr, ti) &&
							rt.Fields.Count >= seg.FieldCount) goto FieldDone;

					if (Rf.SdFile && ((Rf.StructureFlags & MoleculeTransformationFlags.StructuresOnly) != 0) && (ti > 0 || fi > 0))
						goto FieldDone; // skip SDfile fields other than structure and cmpdNo

					// Allocate the first buffer line if needed

					if (TbCsLineCount == 0 && csFieldCount == 0)
					{
						r = TbCsFirstLine = seg.TempBegin = TbFree;
						AssureTbFree(ti, r);
					}

					// Adding first data to segment?

					if (segmentIsEmpty && (DataTableManager.RowHasDataForTable(dr, ti) || ti == 0))
					{
						segmentIsEmpty = false;
						AssureTbFree(ti, r); // be sure first row is free

						if (Rf.Html) // add blank cell place holders
						{
							i1 = csFieldCount;
							if (!Rf.CompoundOriented && csFieldCount > 1 && Tb.Lines[r] != "") i1--; // adjust for formatted (tentatively) compound id
							for (i2 = 1; i2 <= i1; i2++)
								Tb.Lines[r] += "t 0 <br>\t"; // blank
						}
					}

					// See if we have a null value or a non-fresh non-key value

					suppress = false;
					if (NullValue.IsNull(fieldValue)) suppress = true;
					if (!DataTableManager.RowHasDataForTable(dr, ti) && fi > 1) suppress = true; // nonfresh value but not key
					if (!DataTableManager.RowHasDataForTable(dr, ti) && ti > 1 && Rf.CompoundOriented) suppress = true; // object report value not fresh in secondary table (no key value)

					//if (mt.IsRootTable && Rf.DuplicateKeyValues && fieldValue != null)
					//  suppress = false; // keep if key table & duplication requested & not null

					if (suppress && !mergedValues) // null value 
					{
						if (Rf.SdFile) goto FieldDone; // don't put anything in SDFile
						if (Rf.Html && segmentIsEmpty) goto FieldDone; // fields in null segs added later for html

						tok = SS.I.NullValueString;
						if (Rf.Html && Lex.IsNullOrEmpty(SS.I.NullValueString))
							tok = "<br>"; // html blank

						AssureTbFree(ti, r);
						Tb.Lines[r] += "t " + x.ToString() + " " + tok + "\t"; // plug in
						goto FieldDone; // go process next field
					}

					FormatField(rt, ti, rfld, fi, dr, dri, fieldValue, r, true); // format the field & store in buffer

				FieldDone: // finished inserting current field value

					csFieldCount++; // increment current seg field count

					if (csFieldCount == seg.FieldCount) // full segment
					{
						if (Rf.Html) // add html to enclose with row
						{
							AssureTbFree(ti, r);
							Tb.Lines[r] = "E <tr align=\"left\" valign=\"top\">\t" +
									Tb.Lines[r] + "E </tr>\t";
						}

						if (segmentIsEmpty) TbCsLineCount = TbCsLineCountFirst = TbCsLineCountOther = 0; // null segment
						seg.TempLines = TbCsLineCount; // save current seg line count
						seg.TempLinesFirst = TbCsLineCountFirst;
						seg.TempLinesOther = TbCsLineCountOther;

						if (si < Rf.Segments.Count - 1) // move to next segment if not at end 
						{
							si++;
							seg = Rf.Segments[si];
							TbCsFirstLine = seg.TempBegin = TbFree; // beg of this section in tbuf
							r = TbFree; // first row for section
							TbCsLineCount = TbCsLineCountFirst = TbCsLineCountOther = 0; // clear current segment row count
							csFieldCount = 0; // current segment field count
							segmentIsEmpty = true; // reset null seg flag
						}
					}

				} // end of field loop

				if (summaryTable) // restore state if data from summary 
				{
					if (segmentIsEmpty) TbCsLineCount = TbCsLineCountFirst = TbCsLineCountOther = 0; // empty segment?
					seg.TempLines = TbCsLineCount; // save counts for last seg
					seg.TempLinesFirst = TbCsLineCountFirst;
					seg.TempLinesOther = TbCsLineCountOther;

					si = saveSi; // restore current segment
					seg = Rf.Segments[si];

					TbCsLineCount = seg.TempLines; // save counts for last seg
					TbCsLineCountFirst = seg.TempLinesFirst;
					TbCsLineCountOther = seg.TempLinesOther;
					segmentIsEmpty = saveSegmentIsEmpty;
					csFieldCount = saveCsFieldCount;
				}

				else // store data for segment
				{
					if (segmentIsEmpty) TbCsLineCount = TbCsLineCountFirst = TbCsLineCountOther = 0; // empty segment?
					seg.TempLines = TbCsLineCount; // save counts for last seg
					seg.TempLinesFirst = TbCsLineCountFirst;
					seg.TempLinesOther = TbCsLineCountOther;
				}

			} // end of table loop
		} // end of FormatTuple

		/// <summary>
		/// Make a http request and return the result as a file
		/// </summary>
		/// <param name="url"></param>
		/// <param name="outputFile"></param>
		/// <returns></returns>

		bool GetHttpFile(
				string url,
				string outputFile)
		{
			return false; // todo
		}

		/// <summary>
		/// Initialize for a new page in the intermediate buffer
		/// </summary>

		void InitializeIntermediateBuffer()
		{
			int si;
			ResultsSegment seg;

			for (si = 0; si < Rf.Segments.Count; si++)
			{
				seg = Rf.Segments[si];
				seg.IbRows = 0;
				seg.IbLines = 0;
				seg.IbFirst = seg.IbLast = null;
			}

			IbRowCount = 0; // rest count of rows in Ib (not accurate if repeated columns)

			if (RepeatIdx > 0) return; // done if this is a repeat

			RepeatColumnOffset = 0;
			Pb.MaxLine = 0;
		}

		/// <summary>
		/// Add a line to the tuple buffer
		/// </summary>
		/// <param name="ti"></param>

		void IncrementTbFree(
				int ti)
		{
			AssureTbFree(ti, TbFree);
		}

		/// <summary>
		/// Check that a line is allocated in the tuple buffer & keep track of limits
		/// </summary>
		/// <param name="ti">Table to count toward</param>
		/// <param name="lineNeeded">Line number</param>

		void AssureTbFree(
				int ti,
				int lineNeeded)
		{
			int newAlloc, lineCount;

			lineCount = lineNeeded - TbCsFirstLine + 1; // count of lines needed for segment

			if (ti < 0) // associate with all tables 
			{
				if (lineCount > TbCsLineCount) TbCsLineCountFirst = TbCsLineCountOther = lineCount; // extend all
			}
			else if (ti == 0) // associate with first table
			{
				if (lineCount > TbCsLineCountFirst) TbCsLineCountFirst = lineCount;
			}
			else // associate with tables after first 
			{
				if (lineCount > TbCsLineCountOther) TbCsLineCountOther = lineCount;
			}

			if (lineCount > TbCsLineCount) TbCsLineCount = lineCount; // maintain overall linecount

			while (TbCsFirstLine + lineCount - 1 >= TbFree)
			{
				if (TbFree >= Tb.Lines.Length) // need new buffer
				{
					newAlloc = Tb.Lines.Length + 10;
					if (newAlloc > 10000) // reasonable?
						throw new Exception("AssureTbFree - buffer too large");
					string[] lines2 = new string[newAlloc];
					Array.Copy(Tb.Lines, lines2, Tb.Lines.Length);
					Tb.Lines = lines2;
				}

				Tb.Lines[TbFree] = "";
				TbFree++;
			}
		}

		/// <summary>
		/// Move tuple buffer data to intermediate buffer
		/// </summary>
		/// <returns>True if successful, false if not enough space in intermediate buffer</returns>

		bool MoveTupleBufferToIntermediateBuffer()
		{
			int lincnt;
			int si, i1;
			int rowlc;
			TupleBuffer tb2;
			ResultsSegment seg;

			// Calculate the total number of lines on the page if this row is added

			lincnt = Rf.TitleBuffer.Count;

			for (si = 0; si < Rf.Segments.Count; si++)
			{
				seg = Rf.Segments[si];
				if (seg.IbLines > 0 || seg.TempLines > 0 || Rf.OutputDestination == OutputDest.WinForms || SS.I.OutputEmptyTables)
				{
					lincnt += seg.Header.Length + seg.FooterLineCount +
							seg.IbLines + seg.TempLines;
				}
			}

			if (IbRowCount > 0) // anything in buffer now?
			{
				if ((lincnt > Rf.PageLines && !Rf.CompoundOriented) || // page full?
						(Rf.CompoundOriented && CurrentKey != PreviousKey)) // end of compound?
				{

					if (SS.I.DebugFlags[3] != null)
					{
						seg = Rf.Segments[0];
						string msg = String.Format
								("MoveTupleBufferToIntermediateBuffer lincnt={0} Rf.PageLines={1} Rf.TitleBuffer.Count={2} " +
								"FirstSegmentHeaderSize={3} seglines(1)={4} segtlines(1)={5}",
								lincnt, Rf.PageLines, Rf.TitleBuffer.Count, FirstSegmentHeaderSize,
								seg.IbLines, seg.TempLines);
						ClientLog.Message(msg);
					}
					return (false); // break on end of cmpd?
				}
			}

			// Append tuple buffer to the intermediate buffer 

			for (si = 0; si < Rf.Segments.Count; si++)
			{
				seg = Rf.Segments[si];
				if (SS.I.DebugFlags[4] != null)
				{
					string msg = String.Format("si={0} seg.TempBegin={1} seg.TempLines={2}",
							si, seg.TempBegin, seg.TempLines);
					//					ClientLog.Message(msg);
				}

				/* 
	* Allocate buf to contain this row for this segment, even if empty? 
	* If segment 1 and no data for table 1 then try see if room for
	* other this data in previous row buffer. This is especially helpful
	* when a segment contains a structure followed by multiple lines
	* of biology data.
	*/

				rowlc = seg.TempLines; // number of lines for row
				if (rowlc > 0)
				{
					tb2 = null; // buffer to add to, assume we will need a new one
					if (seg.TempLinesFirst == 0 && // must have nothing in temp for table 1 
							seg.IbRows > 0 && seg.IbLast != null) // and something already in ibuf
					{
						tb2 = seg.IbLast; // address prev buffer
						if (tb2.LineCountOther + seg.TempLinesOther > tb2.LineCount) // must fit
							tb2 = null;
					}

					if (tb2 != null) // append it
					{
						for (i1 = 0; i1 < rowlc; i1++)
						{
							tb2.Lines[tb2.LineCountOther + i1] += Tb.Lines[seg.TempBegin + i1];
						}
						tb2.LineCountOther += rowlc; // update lines for tables past first
					}

					else // need new buffer
					{
						tb2 = new TupleBuffer();
						tb2.LineCount = rowlc;
						tb2.LineCountFirst = seg.TempLinesFirst;
						tb2.LineCountOther = seg.TempLinesOther;
						tb2.Lines = new string[rowlc]; // allocate lines
						tb2.NextSegmentBuffer = null;
						if (seg.IbRows == 0) seg.IbFirst = tb2; // first time
						else seg.IbLast.NextSegmentBuffer = tb2; // link to end
						seg.IbLast = tb2;
						seg.IbRows++; // increment row count for seg
						for (i1 = 0; i1 < rowlc; i1++)
						{
							tb2.Lines[i1] = Tb.Lines[seg.TempBegin + i1]; // copy it
						}

						seg.IbLines += seg.TempLines; // upd # lines in seg
					}
				}
			}

			// Increment buffer tuple count, reset tuple buf & return success

			IbRowCount++; // update number of tuples in intermediate buffer
			TbFree = 0; // tuple buffer is now empty
			return true; // return success
		}

		/// <summary>
		/// Move intermediate buffer data to the page buffer
		/// </summary>

		void MoveIntermediateBufferToPageBuffer()

		/*
For object oriented reports there may be more than 1 physical page to
be output. In this case, data is output for one segment at a time.
In the case of tables than span segments, the segments are kept
together to keep the related segments together.
*/
		{
			string tok;
			int sb, se; // beginning & ending segments
			int lincnt; // number of lines in use in buffer
			int si, ri; // segment, row index                      
			int reps, repi, i1, i2;
			TupleBuffer tb2, tb3;
			ResultsSegment seg;

			TupleBuffer[] segcurr = new TupleBuffer[Rf.Segments.Count]; // pointer array to current seg rows

			// Now output data for each segment if appropriate

			si = 0; // start at first segment

			while (si < Rf.Segments.Count)
			{
				sb = si; // get range of segments to be processed

				for (se = si; se < Rf.Segments.Count; se++) // end segment for the current table 
				{
					if (Segment(se).Table1 != Segment(si).Table1) break;
				}
				se--;

				if (SS.I.DebugFlags[3] != null)
					ClientLog.Message(String.Format("sb={0} se={1}", sb, se));

				outputSegments: // start to output rows for the selected group of segments

				OutputBufferPending = false; // clear buffer output flag
				if (SS.I.DebugFlags[3] != null)
					ClientLog.Message(String.Format("MoveIntermediateBufferToPageBuffer - si={0}", si));

				// Calc size of headers & footers for this segment group

				lincnt = 0; // init # of lines used
				if (OutputHeader && (Segment(si).IbLines > 0 || Rf.OutputDestination == OutputDest.WinForms || SS.I.OutputEmptyTables))
				{ // hdr?
					for (si = sb; si <= se; si++)
					{
						lincnt = lincnt + Segment(si).Header.Length + Segment(si).FooterLineCount;
						// Size of hdr & footer
					}

					if (lincnt + Pb.Free - 1 > Rf.PageLines && Pb.RowCount > 0)
					{ // room hdr?
						OutputBufferPending = true;
						goto segmentComplete;
					}
				}

				// Start adding in the size of each row

				for (si = sb; si <= se; si++) // init current row pointers for each seg
				{
					//					ClientLog.Message(String.Format("si={0} Segment(si).IbRows={1}",si,Segment(si).IbRows));
					segcurr[si] = Segment(si).IbFirst; // init pointer to 1st row
				}

				if (Segment(sb).IbRows == 0) ri = 0; // no data

				else
					for (ri = 1; ri <= Segment(sb).IbRows; ri++)
					{
						for (si = sb; si <= se; si++) // add size of row for each segment 
						{
							tb2 = segcurr[si]; // get pointer to row
							if (tb2 != null)
							{
								lincnt = lincnt + tb2.LineCount;
								segcurr[si] = tb2.NextSegmentBuffer; // advance pointer
							}
						}
						if (lincnt + Pb.Free - 1 > Rf.PageLines) // page full?
						{
							if (Pb.RowCount > 0 || ri > 1) break; // break if something on page already
							if (sb != se) // empty page, try again with fewer segments 
							{
								se--;
								si = sb;
								goto outputSegments;
							}
						}
					}

				if (ri == 0) ; // no data, nothing to do
				else if (ri == 1) // space for one row?
				{
					OutputBufferPending = true;
					goto segmentComplete;
				}

				else // partial or full data?
				{
					if (ri <= Segment(sb).IbRows) OutputBufferPending = true; // partial data
					ri--; // all rows will fit
				}
				//				ClientLog.Message(String.Format("ri={0}",ri)); 

				// Fill buffer, headers first, then data, then footers

				for (si = sb; si <= se; si++)
				{
					seg = Rf.Segments[si];

					OutputFooter = false; // clear header & footer flag
					if (OutputHeader && (seg.IbLines > 0 || Rf.OutputDestination == OutputDest.WinForms || SS.I.OutputEmptyTables)) // header?
					{
						if (Rf.Word || Rf.Excel || Rf.Grid || Rf.TextFile) { } // no footer for these
						else OutputFooter = true; // footer for other headers

						if (Rf.Word) // imsert repeated word headers
							reps = Rf.RepeatCount + 1;
						else reps = 1;

						for (i1 = 0; i1 < seg.Header.Length; i1++) // copy header
						{
							IncrementPbFree(); // move free pointer
							for (repi = 1; repi <= reps; repi++)
							{
								Pb.Lines[Pb.Free - 1] += seg.Header[i1];
							}
						}
					}

					// Output data one row at a time

					tb2 = seg.IbFirst; // point to first row for seg
					if (tb2 != null)
						for (i1 = 0; i1 < ri; i1++)
						{
							// ClientLog.Message(String.Format("data for row {0} of {1}, rowlines={2}",i,ri,tb2.LineCount));

							for (i2 = 0; i2 < tb2.LineCount; i2++) // move each row in line
							{
								// ClientLog.Message(String.Format("{0}",tb2.Lines[i2])); 
								IncrementPbFree(); // move free pointer
								Pb.Lines[Pb.Free - 1] += tb2.Lines[i2];
							}

							seg.IbLines = seg.IbLines - tb2.LineCount; // reduce line count
							seg.IbRows = seg.IbRows - 1; // reduce row count
							tb3 = tb2; // save pointer to this one
							tb2 = tb2.NextSegmentBuffer; // advance pointer to next row
							seg.IbFirst = tb2; // point to new beginning
							Pb.RowCount = Pb.RowCount + 1; // count the row in the buffer
							if (tb2 == null) break; // exit if no more rows for seg
						}

					// Add end of segment stuff if including footer

					if (OutputFooter)
					{
						IncrementPbFree();
						if (Rf.Html)
						{ // end table & insert blank line
							Pb.Lines[Pb.Free - 1] += "E </table> <br>\t"; // "E" indicates native html
						}

						else if (seg.HorizontalLine != "")
						{
							i1 = Pb.Lines[Pb.Free - 1].Length; // where H will go
							Pb.Lines[Pb.Free - 1] += // end line with H . h to complete vertical lines
									"h" + seg.HorizontalLine.Substring(1); // ending line
																												 // if (SS.I.BlankLineBetweenSegments) IncrementPbFree(); // insert blank line following segment
						}
					}

				} // end of segment loop

			segmentComplete: // done with this seg, see if we need to output page
				if (OutputBufferPending)  // output the buffer with partial seg
				{
					OutputPageBuffer();
					if (OutputCancelled) return;
					IncrementPbFree();
					if (Rf.NotPopupOutputFormContext) // insert compound id at top of next page if not in subquery
					{
						tok = CompoundId.Format(PreviousKey);
						Pb.Lines[Pb.Free - 1] = "T 0 " + tok + " continued. . .\t";
					}
					si = sb; // continue with current seg
				}
				else si = se + 1; // advance to next segment

			} // end of loop for segment group

			// Save the max page size & size of this page

			if (Pb.Free > Pb.MaxLine) Pb.MaxLine = Pb.Free; // max page size
			Pb.LineCount = Pb.MaxLine; // save line count

			// If report is repeated then see if another repeat is possible

			if (Rf.RepeatCount > 0 && !Rf.Word)  // Word repeats handled by Word
			{

				if (RepeatIdx < Rf.RepeatCount && DtMgr.MoreDataRowsAvailable) // more repeats to do 
				{
					RepeatIdx = RepeatIdx + 1; // advance the report index
					RepeatColumnOffset = RepeatColumnOffset + Segment(0).Width + Rf.InterColumnSpacing; // set the report offset
					RepeatColumnOffsetText = "O " + RepeatColumnOffset.ToString() + "\t"; // text command to offset
					for (i1 = 0; i1 < Pb.MaxLine; i1++) // offset existing buffer lines
						Pb.Lines[i1] += RepeatColumnOffsetText;

					Pb.LineCount = Rf.TitleBuffer.Count; // init counter for new physical page
					Pb.Free = Pb.LineCount;
					return; // put some more stuff in this report
				}

				else // page complete
				{
					RepeatIdx = 0; // reset the repeat index
				}
			}

			OutputPageBuffer(); // write out the buffer to the device

			if (Rf.Grid && OutputHeader) // do header just once
			{
				OutputHeader = false;
			}

			else if (Rf.Word || Rf.Excel || Rf.Grid || Rf.TextFile) // do header just once
			{
				OutputHeader = false;
			}

			return;
		}

		/// <summary>
		/// Method to allow shorthand syntax for getting segment objects from segment indexes
		/// </summary>
		/// <param name="si"></param>
		/// <returns></returns>

		ResultsSegment Segment(
				int si)
		{
			return Rf.Segments[si];
		}

		/// <summary>
		/// Increment the page buffer free pointer and reallocate as needed
		/// </summary>

		void IncrementPbFree()
		{
			string[] newLines;
			int newAlloc, i1;

			if (Pb.Free >= Pb.Lines.Length) // need to extend allocation?
			{
				newAlloc = Pb.Lines.Length + 10;
				newLines = new string[newAlloc];
				if (Pb.Lines.Length > 0)
					Array.Copy(Pb.Lines, newLines, Pb.Lines.Length);
				Pb.Lines = newLines;
			}

			if (RepeatColumnOffset == 0) Pb.Lines[Pb.Free] = ""; // clear line
			else // clear line only if past prev max, do offset
			{
				if (Pb.Free >= Pb.MaxLine) // extending beyone previous max?
				{
					Pb.Lines[Pb.Free] = "";
					for (i1 = 0; i1 < RepeatIdx; i1++) // offset properly for html
						Pb.Lines[Pb.Free] += RepeatColumnOffsetText;
				}
			}
			Pb.Free++; // advance the free pointer
			Pb.LineCount++; // increment the line count

		}

		/// <summary>
		/// Output page buffer to physical device
		/// </summary>

		void OutputPageBuffer()
		{
			string[] curbuf;
			int curlinec;
			int newPage;
			bool previousEnabled = false, nextEnabled = false;
			StreamWriter sw;
			string fileName, msg, txt, tok;
			string hb = "";
			string response = "";
			Rectangle rt;
			float r1, r2, r3;
			long t2;
			int left, top, right, bottom, x, y, i1, i2;

			Lex lex = new Lex();

			PageCount++;
			if (SS.I.DebugFlags[3] != null)
				ClientLog.Message(String.Format("OutputPageBuffer: PageCount={0} Pb.LineCount={1}",
						PageCount, Pb.LineCount));

			// Fill in page number for page

			if (Rf.TitleBuffer.Count > 0)
			{
				txt = String.Format("{0,4}", PageCount); // page number
				i1 = Pb.Lines[0].IndexOf("Page");
				if (i1 >= 0)
					Pb.Lines[0] = Pb.Lines[0].Substring(0, i1 + 4) + txt + Pb.Lines[0].Substring(i1 + 8); // set page number
			}

		OutputPageBuffer: // Setup for page output

			Pb.PhysicalLineCount = 0; // clear number of lines output to physical page
			VertLineCount = 0; // clear # of vertical lines

			if (CurrentDisplayBuffer == null)
			{
				curbuf = Pb.Lines;
				curlinec = Pb.LineCount;
				CurrentPage = PageCount;
			}
			else
			{
				curbuf = CurrentDisplayBuffer.Lines;
				curlinec = CurrentDisplayBuffer.LineCount;
				CurrentPage = CurrentDisplayBuffer.Page;
			}

			if (Rf.Html)
			{
				for (i1 = 0; i1 <= Rf.RepeatCount; i1++)
					Html[i1] = new StringBuilder(); // init html string for each report repetition
			}

			// Output page

			for (i1 = 0; i1 < curlinec; i1++) // output each line
			{
				OutputRecord(curbuf[i1]); // output a line
			}

			// Print preview & Html end of page processing

			if (Rf.Html)
			{

				if (/* Rf.Html && */ DisplayInterruptedPage > 0)
				{ // goto interrupted page?
					if (DisplayInterruptedPage == CurrentPage || // back where display interrupted?
							(!DtMgr.MoreDataRowsAvailable && !OutputBufferPending))
					{ // or no more data
						DisplayInterruptedPage = 0; // interrupt is done
					}

					else // continue to next page, checking for interrupt & updating title
					{
						if (Progress.IsTimeToUpdate) // time to update title
						{
							if (Progress.CancelRequested) DisplayInterruptedPage = 0; // see if cancelled
							else // just update title
							{
								txt = "Page " + CurrentPage.ToString();
								Progress.Show(txt);
							}
						}

						if (DisplayInterruptedPage > 0) // still continuing to page forward ?
						{
							Pb.Structures = new MoleculeMx();
							List<CellGraphic> cgl = null;
							//Graphics.SetFont(Rf.FontName, Rf.FontSize);
							nextEnabled = true;
							response = "PageNext";
							goto ProcessDisplayResponse;
						}
					}
				}

				// Setup button & status display

				previousEnabled = false; // enable back button properly
				nextEnabled = true; // enable next button properly
				if (CurrentDisplayBuffer == null) // on current page
				{
					if (FirstDisplayBuffer != null) previousEnabled = true;
					if (!DtMgr.MoreDataRowsAvailable && !OutputBufferPending) nextEnabled = false;
				}

				else // on cached page
				{
					if (CurrentDisplayBuffer != FirstDisplayBuffer) previousEnabled = true;
				}

				//				SessionManager.DisplayRowCounts(); // todo

				// HTML

				if (Rf.Html)
				{

					if (Rf.NotPopupOutputFormContext)
					{
						hb = GetHtmlHeader(); // get html header information
						if (GenerateToc) hb += HtmlToc; // add table of contents if requested
					}

					hb += // format repeated columns within a surrounding single row table with no border
							"<table>\r\n" +
							"<tr valign=top>\r\n"; // vertically align to top

					for (i1 = 0; i1 <= Rf.RepeatCount; i1++) // each column is inserted in a cell 
					{ // set proper width if multicolumn
						if (Rf.RepeatCount > 0)
						{
							i2 = GraphicsMx.MilliinchesToPixels(Segment(0).Width + Rf.InterColumnSpacing); // pixel units
							txt = "width=" + i2.ToString();
						}
						else txt = "";
						hb +=
								"<td " + txt + ">\r\n" +
								Html[i1].ToString() +
								"</td>\r\n";
					}

					hb += // end tags for row and table
							"</tr>\r\n" +
							"</table>\r\n";

					if (Rf.NotPopupOutputFormContext)
					{
						hb += GetHtmlEnd(); // end tags for body &
					}

					// Display the html 

					if (Rf.NotPopupOutputFormContext && Math.Sqrt(2) == 0) // (deactivated) 
					{ // requerying after export to word/excel
						fileName = ClientDirs.TempDir + @"\page" + CurrentPage.ToString() + ".htm";
						sw = new StreamWriter(fileName);
						if (sw != null) throw new Exception("Error opening temp file on client " + fileName);
						sw.Write(hb);
						sw.Close();
						// todo if reactivated: 
						// 1. Navigate to fileName
						// 2. Set page caption
					}

					else if (Rf.PopupOutputFormContext) // we are in a subquery, append html and continue
					{
						PopupHtml += hb;
						goto exit;
					}

				}

			GetDisplayResponse:
				Progress.Hide();

				if (!SS.I.AutoPage && !SS.I.QueryTestMode)
				{ } // v3					response = SS.I.MainForm.GetWindowMessage();

				else if (SS.I.QueryTestMode) // if test mode cancel after first display
				{
					ReturnMsg = "";
					OutputCancelled = true;
					goto exit;
				}

				else if (SS.I.AutoPage) // do automatic page ahead unless cancel
				{
					if (Progress.CancelRequested || !DtMgr.MoreDataRowsAvailable)
					{
						SS.I.AutoPage = false;
						goto GetDisplayResponse;
					}
					else response = "PageNext";
				}

			ProcessDisplayResponse:

				lex.OpenString(response);
				tok = lex.Get();

				if (ProcessCommonDisplayResponses(response))
					goto GetDisplayResponse;

				else if (Lex.Eq(tok, "Before_Navigate")) // ignore here, let browser do own navigates
					goto GetDisplayResponse;

				else if (Lex.Eq(tok, "List"))
				{
					CloseSearch(); // close search since list may be changed
												 // v3					CnListCommand.Process(response); // do the command
					OutputCancelled = true;
					ReturnMsg = "COMMAND REQUERY"; // redo the query
					goto exit;
				}


				else if (Lex.Eq(tok, "PageTo")) // jump to specified page
				{
					tok = lex.Get(); // get page number
					if (tok == "")
					{
						tok = InputBoxMx.Show("Enter the page number to go to", "Goto Page");
						if (tok == "") goto GetDisplayResponse;
					}

					try
					{
						newPage = Convert.ToInt32(tok);
					}
					catch (Exception ex)
					{
						MessageBoxMx.Show(tok + " is an invalid page number");
						goto GetDisplayResponse;
					}

					DisplayInterruptedPage = 0;
					if (!DtMgr.MoreDataRowsAvailable && !OutputBufferPending && newPage > PageCount)
						newPage = PageCount; // at end already?
					if (newPage == CurrentPage)
						goto OutputPageBuffer; // same page again
					else if (newPage > PageCount) // go to a page not previously created
					{
						DisplayInterruptedPage = newPage; // cause display to skip ahead if necessary
						CurrentDisplayBuffer = null; // set current buffer to new one
						response = "PageNext";
						goto ProcessDisplayResponse;
					}
					else // switch to proper previous display buffer
					{
						CurrentDisplayBuffer = FirstDisplayBuffer; // start at beginning
						for (i1 = 1; i1 < newPage; i1++)
							CurrentDisplayBuffer = CurrentDisplayBuffer.Next;
						goto OutputPageBuffer; // output it
					}
				}

				else if (Lex.Eq(tok, "PagePrevious"))
				{
					if (!previousEnabled)
					{
						MessageBoxMx.Show("You are on the first page of data", UmlautMobius.String);
						goto GetDisplayResponse;
					}

					if (CurrentDisplayBuffer == null) CurrentDisplayBuffer = LastDisplayBuffer;
					else CurrentDisplayBuffer = CurrentDisplayBuffer.Previous;
					goto OutputPageBuffer;
				}

				else if (Lex.Eq(tok, "PageNext"))
				{
					if (!nextEnabled)
					{
						MessageBoxMx.Show("You have reached the last page of data", UmlautMobius.String);
						goto GetDisplayResponse;
					}

					if (CurrentDisplayBuffer == null) // cache buffer and return
					{
						CurrentDisplayBuffer = new DisplayBuffer();
						CurrentDisplayBuffer.Lines = new string[Pb.LineCount];
						Array.Copy(Pb.Lines, CurrentDisplayBuffer.Lines, Pb.LineCount);
						CurrentDisplayBuffer.LineCount = Pb.LineCount; // save line count
						CurrentDisplayBuffer.Page = PageCount; // page number
						CurrentDisplayBuffer.Previous = LastDisplayBuffer;
						CurrentDisplayBuffer.Next = null;
						if (FirstDisplayBuffer == null) FirstDisplayBuffer = CurrentDisplayBuffer; // first one
						else LastDisplayBuffer.Next = CurrentDisplayBuffer; // link end to this
						LastDisplayBuffer = CurrentDisplayBuffer; // new end
						CurrentDisplayBuffer = null;
						goto exit;
					}
					else if (CurrentDisplayBuffer == LastDisplayBuffer)
					{ // move to active buffer
						CurrentDisplayBuffer = null;
						goto OutputPageBuffer;
					}
					else
					{
						CurrentDisplayBuffer = CurrentDisplayBuffer.Next;
						goto OutputPageBuffer;
					}
				}

				else if (Lex.Eq(tok, "PageFirst"))
				{
					CurrentDisplayBuffer = FirstDisplayBuffer; // start at beginning
					goto OutputPageBuffer; // output it
				}

				else if (Lex.Eq(tok, "PageLast"))
				{
					if (!DtMgr.MoreDataRowsAvailable && !OutputBufferPending) newPage = PageCount; // at end now?
					else newPage = 1000000; // something beyond end
					response = "PageTo " + newPage.ToString();
					goto ProcessDisplayResponse;
				}

				else if (Lex.Eq(tok, "RunQuery") || Lex.Eq(tok, "Export") ||
						Lex.Eq(tok, "Word") ||
						Lex.Eq(tok, "Excel") ||
						Lex.Eq(tok, "Textfile") ||
						Lex.Eq(tok, "Spotfire") ||
						Lex.Eq(tok, "Sdfile"))
				{
					if (Lex.Eq(tok, "RunQuery") || Lex.Eq(tok, "Export")) tok = lex.Get(); // query command name
					OutputCancelled = true;
					if (Rf.Html) // if display remember where we left off
						DisplayInterruptedPage = CurrentPage;
					ReturnMsg = "COMMAND " + tok; // return the interrupting command 
					goto exit;
				}

				else if (Response.IsCancel(response))
				{
					ReturnMsg = "";
					OutputCancelled = true;
					goto exit;
				}

				else if (Lex.Eq(tok, "EXIT"))
				{
					// todo			quit(); 
					ReturnMsg = "COMMAND EXIT";
					OutputCancelled = true;
					goto exit;
				}

				else
				{
					OutputCancelled = true;
					ReturnMsg = "COMMAND " + response;
					goto exit;
				}

			}

			// Word, Excel

			else if (Rf.Word || Rf.Excel)
			{
				if (Progress.IsTimeToUpdate)
				{ // time to update status
					Progress.Show(StringMx.FormatIntegerWithCommas(OutputRowCount) + " data records have been formatted.\n" +
							"Formatting is continuing, wait please...", UmlautMobius.String, true, "Cancelling Export...");
				}
			}

			// Console

			else if (Rf.Console)
			{
				Console.WriteLine("");
				Console.WriteLine("Press enter to continue or type Cancel to stop display");
				txt = Console.ReadLine();
				if (Response.IsCancel(txt)) OutputCancelled = true;
			}

			// TextFile

			else if (Rf.TextFile)
			{
				if (Progress.IsTimeToUpdate)
				{
					Progress.Show(StringMx.FormatIntegerWithCommas(OutputRowCount) + " data records have been formatted.\n" +
							"Formatting is continuing, wait please . . .", UmlautMobius.String, true, "Cancelling Export...");
					//if (QueryManager.DataTableManager.DataTableFetchCount + 1 != OutputRowCount) OutputRowCount = OutputRowCount;
				}
			}

			// SdFile

			else if (Rf.SdFile)
			{
				{ // time to update status
					Progress.Show(StringMx.FormatIntegerWithCommas(OutputRowCount) + " data records have been formatted.\n" +
							"Formatting is continuing, wait please . . .", UmlautMobius.String, true, "Cancelling Export...");
				}
			}

		exit: // Cleanup and reset physical page buffer

			Pb.LineCount = Rf.TitleBuffer.Count; // init counter for new physical page
			Pb.Free = Pb.LineCount;
			Pb.RowCount = 0; // no rows in buffer

			if (Progress.IsTimeToUpdate) UpdateCurrentCountDisplay();
			return;
		}

		/// <summary>
		/// Update current count display if single step query execution
		/// </summary>

		public void UpdateCurrentCountDisplay()
		{
			if (MqlUtil.SingleStepExecution(Query))
			{
				SessionManager.DisplayFilterCountsAndString();
			}
			return;
		}

		/// <summary>
		/// Process commom display responses
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>

		public static bool ProcessCommonDisplayResponses(
				string commandMixedCase)
		{
			string extCid, cid;

			Lex lex = new Lex();
			lex.SetDelimiters("( ) ,");
			string commandUpperCase = commandMixedCase.ToUpper();
			lex.OpenString(commandUpperCase);
			string commandName = lex.Get();
			string arg1 = lex.Get();
			if (arg1 != "") lex.Backup();

			if (Lex.Eq(commandName, "HttpRequest"))
			{ // process link click from html display web page
				return false;
			}

			else if (Lex.Eq(commandName, "ShowTableDescription"))
			{ // put up page of description on table/assay
				string mtName = lex.Get();
				QbUtil.ShowTableDescription(mtName);
				return true;
			}

			else if (Lex.Eq(commandName, "AddTableToQuery"))
			{ // add table to underlying query
				string mtName = lex.Get();
				QbUtil.AddTablesToQuery(mtName);
				return true;
			}

			else if (Lex.Eq(commandName, "ShowTableDescriptionFromAssayTitle"))
			{ // lookup assay desc from assay title
				return false;
			}

			else if (Lex.Eq(commandName, "SelectCompoundStructure"))
			{
				extCid = lex.Get();
				if (extCid == "") return true;
				Query q = QbUtil.GetSelectStructureQuery(QbUtil.CurrentQueryRoot, extCid);
				if (q == null) return true;
				cid = CompoundId.Normalize(extCid);
				extCid = CompoundId.Format(cid);

				if (MoleculeMx.HelmEnabled != true)
				{
					QbUtil.RunPopupQuery(q, MetaTable.PrimaryKeyColumnLabel + " " + extCid, OutputDest.WinForms);
				}

				else
				{
					MoleculeMx mol = MoleculeUtil.SelectMoleculeForCid(cid, q.RootMetaTable);
					MoleculeViewer.ShowMolecule(mol, MetaTable.PrimaryKeyColumnLabel + " " + extCid);
				}

				return true;
			}

			else if (Lex.Eq(commandName, "CopyCompoundIdStructure"))
			{
				string extCn = lex.Get();
				string cn = CompoundId.BestMatch(extCn, QbUtil.CurrentQueryRoot);
				StructureUtil.CopyCompoundIdStructureToClipBoard(cn, QbUtil.CurrentQueryRoot);
				return true;
			}

			else if (Lex.Eq(commandName, "SelectBasicCompoundData"))
			{
				MessageBoxMx.Show("Not implemented", UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return true;
			}

			else if (Lex.Eq(commandName, "SelectAllCompoundData")) // show all data
			{
				extCid = arg1;
				if (extCid == "") return true;
				string mtName = "";
				if (QbUtil.CurrentQueryRoot != null)
					mtName = QbUtil.CurrentQueryRoot.Name;

				Progress.Show("Building Query...");
				Query q = QueryEngine.GetSelectAllDataQuery(mtName, extCid);
				if (q == null) return true;
				Progress.Show("Retrieving data..."); // put up progress dialog since this may take a while
				QbUtil.RunPopupQuery(q, MetaTable.PrimaryKeyColumnLabel + " " + extCid);
				return true;
			}

			else if (Lex.Eq(commandName, "SelectAllCompoundDataByTarget"))
			{ // all data for compound by target
				CommandLine.Execute("TargetResultsViewer ShowDialog " + arg1 + " popup");
				return true;
			}

			else if (Lex.Eq(commandName, "SelectAllCompoundDataExtra"))
			{ // show all data including extra data fields
				MessageBoxMx.Show("Not implemented", UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return true;
			}

			else if (Lex.Eq(commandName, "SelectCidRelated"))
			{ // show related compounds
				return false;
			}

			else if (Lex.Eq(commandName, "SelectCidFromMenu"))
			{ // popup menu for selection
				return false;
			}

			else if (Lex.Eq(commandName, "SelectCidMru"))
			{ // show using a recent query
				return false;
			}

			else if (Lex.Eq(commandName, "ShowTargetDescription"))
			{
				ResultsFormatter.ShowNcbiEntrezGeneWebPage(arg1);
				return true;
			}

			else if (Lex.Eq(commandName, "ShowTargetAssayList"))
			{
				Query q = MultiDbAssayMetaBroker.BuildTargetAssayListQuery(arg1);
				QbUtil.RunPopupQuery(q, arg1 + " Assays");
				return true;
			}

			else if (Lex.Eq(commandName, "ShowTargetDataByAssay") || Lex.Eq(commandName, "ShowTargetDataByTarget"))
			{
				return false; // pass back to QueryBuilder for processing
			}

			else if (Lex.Eq(commandName, "Print"))
			{
				Query q = QbUtil.CurrentSetModeQuery;
				if (q == null) throw new Exception("Can't print - CurrentSetModeQuery not defined");
				if (q.Mode != QueryMode.Browse)
				{
					DialogResult dr = MessageBoxMx.Show(
							"Printing can only be performed while browsing data.\n" +
							"Do you want to run the current query so you can print the results?",
							"Mobius", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

					if (dr == DialogResult.Yes)
						CommandExec.ExecuteCommandAsynch("RunQuery");
					return true;
				}

				QueryManager qm = q.QueryManager as QueryManager;
				if (qm == null) throw new Exception("Can't print - QueryManager not defined");
				int cpi = qm.QueryResultsControl.CurrentPageIndex;
				if (cpi == 0) // print grid
					Mobius.ClientComponents.PrintPreviewDialog.Show(qm, qm.MoleculeGrid);

				else if (cpi > 0) // other view
				{
					ResultsViewProps rv = q.ResultsPages[cpi - 1].Views[0];
					if (rv is PivotGridView)
						Mobius.ClientComponents.PrintPreviewDialog.Show(qm, ((PivotGridView)rv).PivotGridCtl);

					else throw new Exception("Can't print view: " + rv.GetType());
				}

				return true;
			}

			else if (Lex.Eq(commandName, "RunQuery") || Lex.Eq(commandName, "Export") ||
					Lex.Eq(commandName, "Word") ||
					Lex.Eq(commandName, "Excel") ||
					Lex.Eq(commandName, "Textfile") ||
					Lex.Eq(commandName, "Spotfire") ||
					Lex.Eq(commandName, "Sdfile") ||
					Lex.Eq(commandName, "Smiles"))
			{
				if (Lex.Eq(commandName, "RunQuery") || Lex.Eq(commandName, "Export"))
					commandName = lex.Get();

				ExecuteExportCommand(commandName, null);
				return true;
			}

			else return false;
		}

		/// <summary>
		/// Process export command
		/// </summary>
		/// <param name="commandName"></param>
		/// <returns></returns>

		public static void ExecuteExportCommand(
				string commandName,
				QueryManager parentQm)
		{
			QueryManager qm;
			DataTableManager dtm;
			Query q;
			ResultsFormat rf0, rf = null;
			DialogResult dr = DialogResult.OK;
			string msg;

			if (parentQm == null)
			{
				parentQm = SessionManager.Instance.QueryResultsQueryManager; // get displayed results QueryManager
				if (parentQm == null) parentQm = QbUtil.QueryManager; // if none use current query builder QueryManager
				if (parentQm == null) throw new Exception("Base query not defined");
			}

			qm = parentQm.Clone(); // QueryManager for export
			q = parentQm.Query;
			dtm = qm.DataTableManager;

			rf0 = qm.ResultsFormat; // save original ResultsFormat

			rf = new ResultsFormat(); // new format
			qm.LinkMember(rf);
			if (q.DuplicateKeyValues) rf.DuplicateKeyTableValues = true; // if query duplicates then assume export will also

			if (Lex.Eq(commandName, "Excel"))
				dr = SetupExcel.ShowDialog(rf, false);

			else if (Lex.Eq(commandName, "Word"))
				dr = SetupWord.ShowDialog(rf, false);

			else if (Lex.Eq(commandName, "Textfile"))
				dr = SetupTextFile.ShowDialog(rf, false);

			else if (Lex.Eq(commandName, "Spotfire"))
			{
				dr = SetupSpotfire.ShowDialog(rf);
				if (dr == DialogResult.Cancel) return;
			}

			else if (Lex.Eq(commandName, "Sdfile"))
				dr = SetupSdFile.ShowDialog(rf, false);

			if (dr == DialogResult.Cancel) return;

			// Setup & run subquery

			if (!rf.RunInBackground)
			{
				while (dtm.HasRetrievingImageRow())
				{
					if (Progress.IsTimeToUpdate)
					{
						Progress.Show("Retrieving images...", UmlautMobius.String, true, "Cancel");
						if (Progress.CancelRequested) return;
					}

					System.Threading.Thread.Sleep(1000);
				}

				Progress.Hide();

				string response = ExecuteExport(qm, parentQm, true);
				if (response != "") MessageBoxMx.Show(response);

				qm.ResultsFormat = rf0; // restore original ResultsFormat
				return;
			}

			// Start background export 
			// Share area test file: \\L1ymc2ua5170f01\c$\download\test.csv

			else
			{
				if (!QbUtil.CanRunQueryInBackground(q)) return;

				UserObject uo = new UserObject(UserObjectType.BackgroundExport);
				uo.Id = UserObjectDao.GetNextId(); // Get UserObject id for the export
				uo.Name = "BackgroundExport " + uo.Id.ToString(); // assign unique name
				uo.Owner = SS.I.UserName;

				//string ext = Path.GetExtension(rf.ClientOutputFileName);
				//rf.ServerOutputFileName = uo.Id + ext;// where to store on server
				rf.Query = q; // same query, use original with UserObject id

				uo.Content = rf.Serialize();
				UserObjectDao.Write(uo, uo.Id); // write with supplied id

				msg = BackgroundQuery.SpawnBackgroundExport(uo.Id);
				MessageBoxMx.Show(msg);
				return;
			}
		}

		/// <summary>
		/// Do setup of ResultsFormat for export 
		/// </summary>
		/// <param name="outputDest"></param>
		/// <param name="qm"></param>
		/// <param name="rf"></param>
		/// <returns></returns>

		//internal static ResultsFormat SetupExportRf(
		//  OutputDest outputDest,
		//  QueryManager qm,
		//  Query q,
		//  ref ResultsFormat rf)
		//{
		//  if (rf == null)
		//  {
		//    rf = new ResultsFormat(qm, outputDest);
		//    rf.QualifiedNumberSplit = // default qualified number formatting
		//      QnfEnum.Split | QnfEnum.Qualifier | QnfEnum.NumericValue;
		//    rf.ExportFileFormat = ExportFileFormat.Csv;
		//  }

		//  else
		//  {
		//    rf.QueryManager = qm;
		//    rf.OutputDestination = outputDest;
		//    qm.ResultsFormat = rf;
		//  }

		//  if (q.DuplicateKeyValues) rf.DuplicateKeyValues = true; // if query duplicates then assume export will also

		//  return rf;
		//}

		/// <summary>
		/// Execute export based on an existing QueryManager using new results format
		/// </summary>
		/// <param name="parentQm"></param>
		/// <param name="rf"></param>
		/// <param name="allowCaching"></param>
		/// <returns></returns>

		internal static string ExecuteExport(
				QueryManager parentQm,
				ResultsFormat rf,
				bool allowCaching)
		{
			//ResultsFormat rf0 = parentQm.ResultsFormat; // save original ResultsFormat

			QueryManager qm = parentQm.Clone(); // create QueryManager for export
			qm.LinkMember(rf); // use new format

			Query q = parentQm.Query;
			DataTableManager dtm = qm.DataTableManager;

			string msg = ExecuteExport(qm, parentQm, allowCaching);
			// parentQm.ResultsFormat = rf0; // restore originalResultsFormat (need this?)

			if (qm.DataTableManager.RowRetrievalCancelled)
				return "Cancelled";
			else return msg;
		}

		/// <summary>
		/// Execute export
		/// </summary>
		/// <param name="qm"></param>
		/// <param name="parentQm"></param>
		/// <param name="allowTruncation"></param>

		internal static string ExecuteExport(
				QueryManager qm,
				QueryManager parentQm,
				bool allowCaching)
		{
			string response;

			DataTableManager dtm = qm.DataTableManager;
			ResultsFormat rf = qm.ResultsFormat;
			rf.ParentQe = qm.QueryEngine; // use existing query results and Vo positions (in case cols rearranged in grid
			if (rf.ParentQe == null) rf.ParentQe = new QueryEngine(); // if no QueryEngine then assign place holder so OpenSearch isn't called

			ResultsFormatFactory rff = new ResultsFormatFactory(rf);
			rff.Build();

			qm.LinkMembers();
			ResultsFormatter fmtr = qm.ResultsFormatter; // use existing formatter
			int savedDtp = dtm.DataTableFetchPosition; // save current formatter position
			dtm.DataTableFetchPosition = -1; // start fetch at beginning again
			dtm.ResetFormattedValues(); // reset formatted values for export
			if (allowCaching) dtm.BeginCaching(); // allow truncation of DataTable

			QueryExec qx = new QueryExec(qm);

			bool saveEH = qm.DataTable.EnableDataChangedEventHandlers(false); // disable data changed events to grid

			//if (parentQm.MoleculeGrid != null) // don't update grid while exporting
			//  parentQm.MoleculeGrid.BeginUpdate2();

			dtm.UpdateSubRowPos(); // update subrow positions to reflect DuplicateKeyValues setting for export to other than grid

			if (rf.Spotfire) // handle Spotfire export here directly
			{
				QueryEngineStats qeStats = qm.ResultsFormatter.ExportToSpotfire();
				response = qeStats.Message;
			}

			else response = qx.RunQuery3(rf, false, false); // run export query for other export types

			parentQm.LinkMembers(); // restore parent QueryManager

			dtm.UpdateSubRowPos(); // update subrow positions to reflect DuplicateKeyValues setting for grid
			dtm.ResetFormattedValues(); // may need to reformat
			if (dtm.RowRetrievalState == RowRetrievalState.Cancelled) // if export was cancelled, set state to paused to allow continuation
			{
				dtm.RowRetrievalState = RowRetrievalState.Paused;
				dtm.StartRowRetrieval(); // restart retrieval
			}

			if (allowCaching) dtm.EndCaching(); // restart retrieval if needed
			dtm.ResetFormattedValues(); // reset formatted values for display
			qm.DataTable.EnableDataChangedEventHandlers(saveEH); // reenable events

			if (parentQm.MoleculeGrid != null)
			{
				//parentQm.MoleculeGrid.EndUpdate2();
				parentQm.MoleculeGrid.Refresh();
				Application.DoEvents();
			}

			dtm.DataTableFetchPosition = savedDtp;
			return response;
		}

		/// <summary>
		/// Show NCBI web page for target gene symbol or id number
		/// </summary>
		/// <param name="geneIdSymbol"></param>

		public static void ShowNcbiEntrezGeneWebPage(
				string targetIdSymbol)
		{
			string url = MultiDbAssayMetaBroker.GetTargetDescriptionUrl(targetIdSymbol);
			if (String.IsNullOrEmpty(url)) return;
			string title = targetIdSymbol + " target gene description";
			UIMisc.ShowHtmlPopupFormDocument(url, title);
		}

		/// <summary>
		/// Interpret and output a "record" to the appropriate device
		/// </summary>
		/// <param name="rec"></param>

		void OutputRecord(
				string rec)

		/*
 * A record consists of a list of commands separated by tabs that are translated
 * to the proper commands for the current output device. 
 * The commands are the following:
 *
 * T xcoord text - text string beginning at xcoord
 * H xbegin xend - horizontal line from xbegin to xend
 * V xcoord - vertical line extending downward from xcoord
 * v - end all vertical lines started via the 'V' command
 * S skid - structure with molecule pointer in hex
 * O xoffset - xoffset for remainder of line (used for multicolumn elements)  
 * E text - HTML & other native code
 * G xcoord width height filename - graphics file or area to fill
*/
		{
			int xoffset;
			string[] commands;
			string command;
			char commandChar, delim;
			string tok;
			StringBuilder txtbuf = new StringBuilder();
			MoleculeMx cs = null;
			int x = 0, x1 = 0, x2 = 0, y, y1, y2, dy;
			int left = 0, top = 0, right = 0, bottom = 0;
			int width = 0, height = 0;
			int colcnt, gridcol;
			int ri, ci, rc, i1, i2, i3;
			int gridRowLines;
			double r1, r2;
			//			MatchCollection matches;
			string[] args;
			CellGraphic cg;
			CellStyleMx cellStyle;
			string serverFile, clientFile, msg, txt = "", link, txt2, prevtxt;
			//			object hmutex;
			long t1;

			//	ClientLog.Message(String.Format("OutputRecord {0} {1}", OutputRowCount+1,rec);

			if (Rf.Html) xoffset = 0; // only count repeats for html
			else xoffset = Rf.PageMargins.Left; // start with left margin offset
			y = (Rf.PageMargins.Bottom + Rf.PageHeight) - // y location, consider margin
					((Pb.PhysicalLineCount + 1) * Rf.LineHeight); // y position is baseline coord
			ri = 0; // position in record
			colcnt = 0;
			//			CurrentGridRowHeight = 0;
			gridRowLines = 0; // init max # of lines of text seen in cell
			txtbuf.Length = 0;
			prevtxt = "";

			commands = rec.Split('\t'); // split out commands
			int commandCount = commands.Length;
			if (commandCount > 0 && commands[commandCount - 1] == "")
				commandCount--; // remove any empty command

			for (ci = 0; ci < commandCount; ci++)
			{
				command = commands[ci];
				if (command.Length == 0) continue;
				commandChar = command[0];

				switch (commandChar)
				{

					case 'T': // text
					case 't':
						args = SplitCommand(command, 3);
						x = int.Parse(args[1]);
						x += xoffset;
						txt = args[2];
						break;

					case 'H': // horizontal line 
					case 'h': // horizontal line meeting vertical at end
						args = command.Split(' ');
						x = int.Parse(args[1]);
						x2 = int.Parse(args[2]);
						x += xoffset;
						x2 += xoffset;
						break;

					case 'V': // vertical line start
						args = command.Split(' ');
						x = int.Parse(args[1]);
						x += xoffset;
						y2 = y + Rf.LineHeight / 3; // center
						for (i1 = 0; i1 < VertLineCount; i1++)
							if (VertLineX[i1] == x && VertLineY[i1] == y2) break;
						if (i1 >= VertLineCount && VertLineCount < VERT_LINE_MAX)
						{
							VertLineX[VertLineCount] = x;
							VertLineY[VertLineCount] = y2;
							VertLineCount++;
						}
						break;

					case 'S': // structure
					case 's': // structure in cell with text
						args = SplitCommand(command, 9);
						x = int.Parse(args[1]);
						width = int.Parse(args[2]);
						left = int.Parse(args[3]);
						top = int.Parse(args[4]);
						right = int.Parse(args[5]);
						bottom = int.Parse(args[6]);
						cs = new MoleculeMx(args[7]);
						txt = args[8]; // any style for cell
						break;

					case 'O': // x offset
						if (Rf.Html) xoffset++; // just count # of offsets if html
						else
						{
							args = command.Split(' ');
							xoffset = int.Parse(args[1]);
							xoffset += Rf.PageMargins.Left;
						}
						break;

					case 'E': // HTML & other native codes
						if (command.Length > 2) txt = command.Substring(2);
						else txt = "";
						break;

					case 'G': // image
					case 'g': // image in cell as text
						args = SplitCommand(command, 5);
						x = int.Parse(args[1]);
						width = int.Parse(args[2]);
						height = int.Parse(args[3]);
						txt = args[4];
						break;

					default:
						MessageBoxMx.Show("OutputRecord invalid command " + commandChar + " in " + rec);
						return;
				}

				// MS Word

				if (Rf.Word)
					do
					{

						if (commandChar == 'O' || commandChar == 'H' || commandChar == 'h') break;

						if ((colcnt > 0 || OutputRowCount > 0) && commandChar != 's' && commandChar != 'g')
						{
							msg = WordOp.Call("MoveRight", WordOp.Cell);
							msg = WordOp.Call("SetDefaultCellStyle"); // reset any inherited cell style attributes
						}

						switch (commandChar)
						{

							case 'T': // text, todo:handle symbols, sub & superscript
							case 't':
								CellStyleMx style = CellStyleMx.ExtractTag(ref txt);
								if (style != null) // any cell style info for cell?
								{
									Font f = style.Font;

									WordOp.Call("Font.Name", f.Name);
									WordOp.Call("Font.Size", (int)f.SizeInPoints);

									if (f.Bold) WordOp.Call("Font.Bold", f.Bold);
									if (f.Italic) WordOp.Call("Font.Italic", f.Italic);

									if (style.ForeColor != Color.White)
										WordOp.Call("Font.Color", style.ForeColor.ToArgb());

									if (style.BackColor != Color.White)
										WordOp.Call("BackColor", style.BackColor.ToArgb());
								}

								while (txt != "")
								{
									if (txt.Length <= 250)
									{
										txt2 = txt;
										txt = "";
									}
									else
									{
										txt2 = txt.Substring(0, 250);
										txt = txt.Substring(250);
									}
									msg = WordOp.Call("TypeText", txt2);
								}

								break;

							case 'S': // structure
								goto WordStructureCommon;

							case 's': // structure following text in a table cell
								colcnt--; // backup the column counter
								msg = WordOp.Call("TypeParagraph"); // separate by line

							WordStructureCommon:
								msg = WordOp.Call("ParagraphFormat.Alignment", "", WordOp.AlignParagraphCenter); // center structure

								//Mutex mutex = null;

								//string workAround = "false"; //  UalUtil.IniFile.Read("WordPasteWorkAround"); - // force picture paste as workaround for fail of object paste on TST & PRD servers
								//if (!Lex.Eq(workAround, "true"))
								//{
								//  mutex = new Mutex(false, "MobiusClipboard");
								//  mutex.WaitOne(); // get exclusive access to clipboard
								//}

								MoleculeControl molCtl = MoleculeEditor.Instance.MoleculeCtl;
								molCtl.SetupAndRenderMolecule(cs);
								if (molCtl.MoleculeCtl.CanCopy)
								{
									molCtl.CopyMoleculeToClipboard();
									if (Rf.ExportStructureFormat == ExportStructureFormat.Insight)
										// && !Lex.Eq(workAround, "true"))
										msg = WordOp.Call("Paste"); // paste structure object
									else msg = WordOp.Call("PasteSpecial", WordOp.PasteMetafilePicture); // paste image

									//if (!Lex.Eq(workAround, "true"))
									//  mutex.ReleaseMutex();
								}

								break;

							case 'G': // graphics
								goto wordGraphicsCommon;

							case 'g': // graphics following text in a table cell 
								colcnt--;
								msg = WordOp.Call("TypeParagraph"); // move to next line

							wordGraphicsCommon:
								msg = WordOp.Call("InlineShapes.AddPicture", txt, // insert picture
										(int)(width / 1000.0 * 72), (int)(height / 1000.0 * 72)); // set size (points)
								break;
						}

						colcnt++;
					} while (false);

				// Excel

				else if (Rf.Excel)
				{
					switch (commandChar)
					{

						case 'T': // text
						case 't':
							cellStyle = CellStyleMx.ExtractTag(ref txt); // see if any cell style tag
							if (cellStyle != null)
							{
								cg = new CellGraphic();
								cg.Col = colcnt + 1;
								cg.Row = OutputRowCount + 1;
								cg.CellStyle = cellStyle;
								CellGraphics.Add(cg);
								//ClientLog.Message("CellGraphics.Add Text: " + rec); // debug
							}

							delim = ','; // separate with comma
							if (colcnt > 0) txtbuf.Append(delim); // insert delimiter
							txt = txt.Trim();
							prevtxt = txt;
							if (txt.IndexOf("\"") >= 0 || txt.IndexOf(delim) >= 0 || // need to quote?
									txt.IndexOf("\n") >= 0) txt = Lex.Dq(txt);
							if (txt.IndexOf("ID") == 0) txt = Lex.Dq(txt); // "ID bug" workaround

							//if (txt.IndexOf("-") > 0)
							//{ // hack to avoid having XL convert a month/day (e.g. 10-30) to a string (better to do in ImportCsv)
							//  string[] sa = txt.Split('-');
							//  if (sa.Length == 2 && int.TryParse(sa[0], out i1) && int.TryParse(sa[1], out i2))
							//  {
							//    if (i1 >= 1 && i1 <=12 && i2 >= 1 && i2 <= 31) // valid month/day range?
							//      txt = Lex.Dq(" " + txt); // preceeding space avoids date conversion
							//  }
							//}

							txtbuf.Append(txt);
							break;

						case 'S': // structure
							r1 = 0; // height of non-structure text p
							height = bottom - top;
							r2 = (height / 1000.0) * 72; // struct height in pts
							if (colcnt > 0) txtbuf.Append(","); // include place holder for graphic in csv string buffer
							prevtxt = null; // no text
							goto excelStructureCommon;

						case 's': // structure with text also in the cell (must put in struct since Insight overwrites cell text?)
							txt2 = prevtxt.Replace("\n", ""); // count lines in prev text (allow just one for now)
							i1 = prevtxt.Length - txt2.Length;
							r1 = (12.75 * (i1 + 1)) + 2; // text height in pts

							height = bottom - top;
							r2 = (height / 1000.0) * 72 + r1; // total row height (text and structure)
							colcnt--; // backup a col to place structure with text in same cell

						excelStructureCommon:

							cg = new CellGraphic();
							cg.RowHeight = (float)r2; // height of text + structure
							cg.Height = (int)(height / 1000.0 * 72.0); // save size of structure in points
							cg.Width = (int)(width / 1000.0 * 72.0);
							cg.Col = colcnt + 1;
							cg.Row = OutputRowCount + 1;
							if (prevtxt != null)
								try { cs.CreateStructureCaption(prevtxt); } catch (Exception ex) { ex = ex; } // ignore exception (Issue #216)
							cg.Molecule = cs;
							cg.GraphicsFileName = "";
							//ClientLog.Message("CellGraphics.Add Struct: " + rec); // debug

							CellGraphics.Add(cg);
							break;

						case 'G': // graphics
							r1 = 0; // height of text part
							r2 = height / 1000.0 * 72 + 6; // height in points, add a bit so grid lines show
							if (colcnt > 0) txtbuf.Append(","); // include place holder for graphic in csv string buffer
							goto excelGraphicsCommon;

						case 'g': // graphics following text in a cell 
							txt2 = prevtxt.Replace("\n", ""); // count lines in prev text
							i1 = prevtxt.Length - txt2.Length;
							r1 = (12.75 * (i1 + 1)) + 6; // space for text in points
							r2 = height / 1000.0 * 72 + r1; // row height (text and image)
							colcnt--; // backup a col to place graph with text in same cell

						excelGraphicsCommon:

							if (r1 > 0)
							{ // put some blank space in graphic above structure where text will go
								x1 = x2 = 0;
								y1 = y2 = (int)(-r1 * (1000.0 / 72.0)); // space above in milliinches 
							}

							cg = new CellGraphic();
							cg.RowHeight = (float)r2; // height of text + structure
							cg.Col = colcnt + 1; // save cell
							cg.Row = OutputRowCount + 1;
							cg.GraphicsFileName = txt; // copy name of graphics file
							cg.Height = (int)(height / 1000.0 * 72.0); // save size of image in points
							cg.Width = (int)(width / 1000.0 * 72.0);

							//ClientLog.Message("CellGraphics.Add Graphics: " + rec); // debug

							CellGraphics.Add(cg);
							break;
					}

					colcnt++; // advance column number
				}

				// Textfile or console

				else if (Rf.TextFile || Rf.Console)
				{
					switch (commandChar)
					{

						case 'T':
						case 't':
							break;

						case 'S':
						case 's':
							args = SplitCommand(command, 9);
							txt = args[7]; // Chime or Smiles string
							break;

						default:
							continue; // ignore any others
					}

					if (Rf.TextFile)
					{
						tok = txt.Trim();
						if (Rf.ExportFileFormat == ExportFileFormat.Csv) // comma separated
						{
							if (txtbuf.Length > 0) txtbuf.Append(",");
							if (txt.Contains("\"") || txt.Contains(",") || txt.Contains("\n"))
								tok = Lex.DqIfNeeded(tok);
						}

						else if (Rf.ExportFileFormat == ExportFileFormat.Tsv) // tab separated
						{
							if (txtbuf.Length > 0) txtbuf.Append("\t");
							if (txt.Contains("\t")) txt = txt.Replace("\t", " ");
							if (txt.Contains("\n")) txt = txt.Replace("\n", " ");
						}

						//if (tok == "") tok = " "; // insert space place holder (Don't do this because a space is different than a null string)
						txtbuf.Append(tok);
					}
				}

				// SdFile

				else if (Rf.SdFile)
				{
					if (rec.StartsWith("h ")) return; // ignore these records

					switch (commandChar)
					{

						case 'T': // text
						case 't':
							SdfLine += txt;
							break;
					}
				}

				// Html

				else if (Rf.Html)
				{
					StringBuilder sb = Html[xoffset]; // append to Html StringBuilder

					switch (commandChar)
					{

						case 'T':
						case 't':
							sb.Append("<td>");
							sb.Append(txt.Trim());
							sb.Append("</td>");
							sb.Append("\r\n");
							break;

						case 'E': // native HTML
							sb.Append(txt.Trim());
							sb.Append("\r\n");
							break;

						case 'S': // structure
							break;

						case 's': // structure with text in cell
							break;
					}
				}

				// Grid

				else if (Rf.Grid)
					throw new NotImplementedException();

			} // end of command loop 

			// Final record processing

			if (Rf.TextFile || Rf.Excel)
			{
				if (txtbuf.Length == 0) return;
				/*
if (SS.I.QueryTestMode)
{
	//add the line to the CRC calculator to update the data CRC
	//--doesn't look worth the investment of time/effort right now
	//--save this thought for later.
}
*/
				TextStream.WriteLine(txtbuf);

				if (OutputRowCount == 0 && Rf.IncludeDataTypes)
					OutputSpotfireDataTypeHeader(); // insert spotfire type header
			}

			else if (Rf.Console) Console.WriteLine(txtbuf);

			else if (Rf.Grid)
			{
				throw new NotImplementedException();
			}

			OutputRowCount++;

			Pb.PhysicalLineCount++;
			return;
		}

		/// <summary>
		/// Split a record command. The last token returned will be the rest of the string
		/// which may contain spaces.
		/// </summary>
		/// <param name="tokCount"></param>
		/// <param name="s"></param>
		/// <returns></returns>

		string[] SplitCommand(
				string s,
				int tokCount)
		{
			string[] a;
			int i1;

			string[] sa = s.Split(' ');
			int sal = sa.Length;
			if (sal == tokCount) return sa;

			if (sal > tokCount) // too long is ok
			{
				a = sa;
				int spaces = 0; // get remainder as single string
				for (i1 = 0; i1 < s.Length; i1++)
				{
					if (s[i1] != ' ') continue;

					spaces++;
					if (spaces == tokCount - 1) break;
				}

				if (spaces == tokCount - 1 && s.Length - 1 > i1)
					a[tokCount - 1] = s.Substring(i1 + 1);
			}

			else // copy to new array & blank the rest if sa is not long enough
			{
				a = new string[tokCount];

				for (i1 = 0; i1 < tokCount; i1++)
				{
					if (i1 < sal) a[i1] = sa[i1];
					else a[i1] = ""; // past end of text
				}
			}

			return a;
		}

		void HighlightStructureMatch() // highlight structure match if turned on
		{
			if (!Rf.HighlightStructureMatches) return; // do highlighting of match?

			if (PageCount > 10 && DisplayInterruptedPage > PageCount + 10 && DtMgr.MoreDataRowsAvailable)
				return; // if formatting over many pages then skip highlight for speed
		}

		void FormatWordColumnsAndHeaders() // setup MS Word columns and headers

		/*
 * This routine creates the MS Word table and formats the table headers.
 * The header labels are output as a normal part of the OutputRecord code.
 * Column widths are set and justification is set for both the header and
 * data rows. This code uses normal MS Word commands to implement its functions
 * The command sequence was derived through a series of experimental word macros
 * that tested various functions. The Word Developers Kit documentation was also
 * used.
 */
		{
			int cols, rows;
			ResultsTable rt;
			MetaTable mt;
			ResultsField rfld;
			QueryColumn qc;
			MetaColumn mc;
			string msg;
			int ci, ri, ti, fi, currentField, align, rep, totcols;

			// Create table with header rows

			InitializeWord();

			//			msg = WordOp.Call("Visible",true); // show updating for debug
			//			msg = WordOp.Call("ScreenUpdating",true);
			//			SS.I.DebugFlags[5] = "1";

			cols = Segment(0).FieldCount; // # of columns
			rows = FirstSegmentHeaderSize + 1; // # of rows in header + 1st data row
			msg = WordOp.Call("Tables.Add", "", rows, cols * (Rf.RepeatCount + 1)); // create initial table

			// Set overall table parameters

			msg = WordOp.Call("TableSelect", ""); // select complete table
			msg = WordOp.Call("Font.Size", "", Rf.FontSize); // set font size
			msg = WordOp.Call("Rows.AllowBreakAcrossPages", false); // keep cell contents together
			msg = WordOp.Call("HomeKey", "", WordOp.Row); // move to upper left corner

			// Set width and alignment on each column

			msg = WordOp.Call("HomeKey", "", WordOp.Row); // move to upper left corner
			msg = WordOp.Call("HomeKey", "", WordOp.Column);
			totcols = 0; // total number of columns
			ci = 1; // start with first column

			for (rep = 0; rep <= Rf.RepeatCount; rep++)
			{ // do each repeat

				for (ti = 0; ti < Rf.Tables.Count; ti++)
				{

					rt = Rf.Tables[ti];
					mt = rt.MetaTable;

					// Process each field

					for (fi = 0; fi < rt.Fields.Count; fi++)
					{
						rfld = rt.Fields[fi];
						qc = rfld.QueryColumn;
						mc = rfld.MetaColumn;

						msg = WordOp.Call("SelectColumn");
						msg = WordOp.Call("Cells.SetWidth", "", (int)(rfld.ColumnWidth / 1000.0 * 72 + 1)); // width in points

						align = WordOp.AlignParagraphLeft;
						currentField = rfld.FieldX;
						switch (mc.DataType)
						{

							case MetaColumnType.Number:
							case MetaColumnType.Integer:
							case MetaColumnType.QualifiedNo:
								align = WordOp.AlignParagraphRight; break;
						}

						// Set specific horizontal alignment

						if (qc.ActiveHorizontalAlignment != HorizontalAlignmentEx.Default)
						{
							if (qc.ActiveHorizontalAlignment == HorizontalAlignmentEx.Left)
								align = WordOp.AlignParagraphLeft;

							else if (qc.ActiveHorizontalAlignment == HorizontalAlignmentEx.Center)
								align = WordOp.AlignParagraphCenter;

							else if (qc.ActiveHorizontalAlignment == HorizontalAlignmentEx.Right)
								align = WordOp.AlignParagraphRight;
						}

						//if (rfld.MergedFieldList != null) // if multiple merged fields then left align
						//  align = WordOp.AlignParagraphLeft;

						WordOp.Call("ParagraphFormat.Alignment", align);

						// Set specific vertical alignment

						if (qc.ActiveVerticalAlignment != VerticalAlignmentEx.Default)
						{
							if (qc.ActiveVerticalAlignment == VerticalAlignmentEx.Top)
								align = WordOp.CellAlignVerticalTop;

							else if (qc.ActiveVerticalAlignment == VerticalAlignmentEx.Middle)
								align = WordOp.CellAlignVerticalCenter;

							else if (qc.ActiveVerticalAlignment == VerticalAlignmentEx.Bottom)
								align = WordOp.CellAlignVerticalBottom;

							WordOp.Call("Cells.VerticalAlignment", align);
						}

						if (ci < Segment(0).FieldCount * (Rf.RepeatCount + 1) && rows > 1) // clear selection if not at end
							WordOp.Call("MoveRight", WordOp.Cell); // only need to do if >1 rows
						WordOp.Call("MoveRight", WordOp.Cell); // move to next cell
						ci++; // next column
						totcols++; // count column
					} // end of field loop

				} // end of table loop
			} // end of repeat report loop

			// Mark and center headings

			for (ri = 1; ri <= FirstSegmentHeaderSize; ri++)
			{ // mark table header rows	
				msg = WordOp.Call("HomeKey", "", WordOp.Row); // move to upper left corner
				msg = WordOp.Call("HomeKey", "", WordOp.Column);
				if (ri > 1) // move to start of row (movedown doesn't seem to work)
					msg = WordOp.Call("MoveRight", "", WordOp.Cell, (ri - 1) * totcols);
				msg = WordOp.Call("SelectRow", "");
				msg = WordOp.Call("Rows.HeadingFormat", true);
				msg = WordOp.Call("ParagraphFormat.Alignment", "", WordOp.AlignParagraphCenter);
			}

			// Position cursor in upper left corner in preparation for data insertion

			msg = WordOp.Call("HomeKey", "", WordOp.Row); // move to upper left corner
			msg = WordOp.Call("HomeKey", "", WordOp.Column);
			return;
		}

		void FormatWordColumnsAndHeadersPart2() // merge any table headers
		{
			ResultsTable rt;
			MetaTable mt;
			string msg;
			int ti, rep;

			// Process each table

			msg = WordOp.Call("HomeKey", "", WordOp.Row); // move to upper left corner
			msg = WordOp.Call("HomeKey", "", WordOp.Column);

			if (FirstSegmentHeaderSize < 2) return; // just return if no table headers

			for (rep = 0; rep <= Rf.RepeatCount; rep++)
			{ // do each repeat

				for (ti = 0; ti < Rf.Tables.Count; ti++)
				{

					rt = Rf.Tables[ti];
					mt = rt.MetaTable;

					if (rt.Fields.Count > 1) // do we need to merge cells 
					{
						WordOp.Call("MoveRight", "", WordOp.Cell); // this selects the first cell for table header
						WordOp.Call("MoveLeft", "", WordOp.Cell);
						WordOp.Call("MoveRight", "", WordOp.Character, rt.Fields.Count, WordOp.Extend); // extend to all cells for table
						WordOp.Call("Cells.Merge"); // merge the cells
					}
					WordOp.Call("MoveRight", "", WordOp.Cell); // move to first cell for next table

				}
			}

			msg = WordOp.Call("HomeKey", "", WordOp.Row); // move to upper left corner
			msg = WordOp.Call("HomeKey", "", WordOp.Column);
			return;
		}

		void InitializeWord() // initialize for output to Word
		{
			string msg;
			int i1, i2, i3, i4;

			msg = WordOp.Call("Documents.Add", ""); // add new word document

			if ((Rf.PageOrientation == Orientation.Horizontal && SS.I.WordPageWidth < SS.I.WordPageHeight) || // new orient?
					(Rf.PageOrientation == Orientation.Vertical && SS.I.WordPageWidth > SS.I.WordPageHeight))
			{
				if (Rf.PageOrientation == Orientation.Vertical) i1 = WordOp.OrientPortrait;
				else i1 = WordOp.OrientLandscape; // orient parm for Word
				msg = WordOp.Call("PageSetup.Orientation", "", i1); // orient page
			}

			i1 = (int)(Rf.PageWidth / 1000.0 * 72); // set page size in points
			i2 = (int)(Rf.PageHeight / 1000.0 * 72);
			//	msg=WordOp.Call("PageSetup.PageSize","",i,i2);

			i1 = (int)(Rf.PageMargins.Top / 1000.0 * 72); // set page margins in points
			i2 = (int)(Rf.PageMargins.Bottom / 1000.0 * 72);
			i3 = (int)(Rf.PageMargins.Left / 1000.0 * 72);
			i4 = (int)(Rf.PageMargins.Right / 1000.0 * 72);
			//msg = WordOp.Call("PageSetup.Margins", "", i1, i2, i3, i4);

			return;
		}

		/// <summary>
		/// This routine sets Excel column widths and justification for both the header
		/// and data rows. The header labels are output as a normal part of the 
		/// OutputRecord code. 
		/// </summary>

		void FormatExcelColumnsAndHeaders()
		{
			ResultsTable rt;
			ResultsField rfld;
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			ColumnFormatEnum displayFormat;
			string txt;
			int ci, ri, ti, fi, align, decimals;
			float r1;

			// Process each table

			ci = 1; // start with first column
			for (ti = 0; ti < Rf.Tables.Count; ti++)
			{
				rt = Rf.Tables[ti];
				mt = rt.MetaTable;

				// Process each field

				for (fi = 0; fi < rt.Fields.Count; fi++)
				{
					rfld = rt.Fields[fi];
					qc = rfld.QueryColumn;
					mc = rfld.MetaColumn;

					ExcelOp.ColumnsSelect(ci);
					r1 = (float)(rfld.ColumnWidth / 1000.0 * 12.82); // col width in "standard chars"
					ExcelOp.ColumnWidth(r1);
					ExcelOp.WrapText(true);
					ExcelOp.VerticalAlignment(ExcelOp.Top);

					switch (mc.DataType)
					{

						case MetaColumnType.CompoundId:
							align = ExcelOp.Left;
							ExcelOp.NumberFormat("@"); // format as text (keep leading 0's)
							break;

						// Numeric fields can be formatted as fixed decimal, sig. figs or scientific.
						// If decimal then we set it explicitly. Scientific is properly detected and displayed.
						// SigFig truncates zero digits to the right of the decimal. Setting column type to text
						// does not appear to work. Setting decimals on each cell may be the only way to get an 
						// exact representation for decimal sig figs with trailing zeros.

						case MetaColumnType.Number: // numeric fields
						case MetaColumnType.Integer:
						case MetaColumnType.QualifiedNo:
							displayFormat = qc.ActiveDisplayFormat;
							if (displayFormat == ColumnFormatEnum.Decimal) // set decimals if decimal format
							{
								decimals = qc.ActiveDecimals;
								if (decimals > 0) // set number of decimals if specified
								{
									txt = "0.";
									txt = txt.PadRight(decimals + 2, '0');
									ExcelOp.NumberFormat(txt);
								}
							}

							align = ExcelOp.Right;
							break;

						case MetaColumnType.String:
							ExcelOp.NumberFormat("@"); // format as text to keep exact representation
							align = ExcelOp.Left;
							break;

						case MetaColumnType.Date:
							ExcelOp.NumberFormat("d-mmm-yy"); // format as date
							align = ExcelOp.Left;
							break;

						case MetaColumnType.Structure: // center structures and images
						case MetaColumnType.Image:
							align = ExcelOp.Center;
							break;

						default: // all other stuff
							align = ExcelOp.Left;
							break;
					}

					// Set specific horizontal alignment

					if (qc.ActiveHorizontalAlignment != HorizontalAlignmentEx.Default)
					{
						if (qc.ActiveHorizontalAlignment == HorizontalAlignmentEx.Left)
							align = ExcelOp.Left;

						else if (qc.ActiveHorizontalAlignment == HorizontalAlignmentEx.Center)
							align = ExcelOp.Center;

						else if (qc.ActiveHorizontalAlignment == HorizontalAlignmentEx.Right)
							align = ExcelOp.Right;
					}

					//if (rfld.MergedFieldList != null) // if multiple merged fields then left align (obsolete)
					//  align = ExcelOp.Left;

					ExcelOp.HorizontalAlignment(align); // set alignment

					// Set specific vertical alignment

					if (qc.ActiveVerticalAlignment != VerticalAlignmentEx.Default)
					{
						if (qc.ActiveVerticalAlignment == VerticalAlignmentEx.Top)
							align = ExcelOp.Top;

						else if (qc.ActiveVerticalAlignment == VerticalAlignmentEx.Middle)
							align = ExcelOp.Center;

						else if (qc.ActiveVerticalAlignment == VerticalAlignmentEx.Bottom)
							align = ExcelOp.Bottom;

						ExcelOp.VerticalAlignment(align); // set alignment
					}

					// Header alignment

					for (ri = 1; ri <= FirstSegmentHeaderSize; ri++)
					{ // center and bold header lines
						ExcelOp.RangeSelect(ColumnNumberToColumnLetter(ci) + ri); // select header row cell
						ExcelOp.SetFontStyle(FontStyle.Bold);
						ExcelOp.HorizontalAlignment(ExcelOp.Center);
						ExcelOp.VerticalAlignment(ExcelOp.Bottom);
					}

					ci++;
				} // end of field loop

			} // end of table loop

			return;
		}


		/// <summary>
		/// Merge Excel table headers as necessary
		/// </summary>

		void MergeExcelTableHeaders()
		{
			ResultsTable rt;
			MetaTable mt;
			int ci, ti, i1;

			ci = 1; // start with first column
			for (ti = 0; ti < Rf.Tables.Count; ti++)
			{
				rt = Rf.Tables[ti];
				mt = rt.MetaTable;

				ci += rt.Fields.Count; // 1 past last field for table
				if (Rf.HeaderLines == 2 && rt.Fields.Count > 1) // must have two non-CF header lines, table & col headers
				{ // merge table header columns
					i1 = ci - rt.Fields.Count; // first col in range
					ExcelOp.RangeSelect(ColumnNumberToColumnLetter(i1) + "1:" + ColumnNumberToColumnLetter(ci - 1) + "1");
					ExcelOp.Merge();
					ExcelOp.RowsAutofit(); // reset height
				}
			}

			ExcelOp.RangeSelect("A1"); // leave upper left cell selected
			return;
		}

		/// <summary>
		/// Convert an excel column number to one or more column letters
		/// </summary>
		/// <param name="columnNumber"></param>
		/// <returns></returns>

		string ColumnNumberToColumnLetter(
				int columnNumber)
		{
			char c;
			string s = "";
			int i1, i2;

			while (true)
			{
				i1 = (columnNumber - 1) % 26;
				i2 = Convert.ToInt32('A') + i1;
				c = Convert.ToChar(i2);
				s = c + s;
				columnNumber = (columnNumber - 1) / 26;
				if (columnNumber <= 0) break;
			}
			return s;
		}

		/// <summary>
		/// Get HTML header info
		/// </summary>
		/// <param name="numberPage"></param>
		/// <returns></returns>
		string GetHtmlHeader()
		{
			return
				"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>\r\n" +
				"<html xmlns='http://www.w3.org/1999/xhtml' >\r\n" +
					"<head>\r\n" +
					"<meta http-equiv='x-ua-compatible' content='IE=11'>\r\n" +
					"<meta http-equiv='Content-Type' content='text/html; charset=utf-8'>\r\n" +
					"<title>Mobius</title>\r\n" +
					"<style type=\"text/css\"></style>\r\n" +

							//"<style><a:link{color:#0000FF} a:visited{color:#0000FF} a:hover{color:#FF0000}></style>\r\n" +
							//"<style type=\"text/css\"> table{border:1px solid;}</style>\r\n" +

						"<script language='JavaScript'>\r\n" + // this seems to avoid display crud for some reason
						"</script>\r\n" +
						"<script language='JScript'>\r\n" + // add right-click context popup suppression
							"function Init()\r\n" +
							"{\r\n" +
							"    if (document.readyState == 'complete')\r\n" +
							"    {\r\n" +
							"        document.oncontextmenu = new Function('return false;');\r\n" +  //suppress the right-click at the page level
							"    }\r\n" +
							"    else\r\n" +
							"    {\r\n" +
							"        setTimeout(Init,100);\r\n" +
							"    }\r\n" +
							"}\r\n" +
						"</script>\r\n" +
					"</head>\r\n" +

				"<body topmargin=\"0\" leftmargin=\"0\" onload=\"Init();\">\r\n"; // includes start of body
		}

		/// <summary>
		/// Get any message describing unavailable data
		/// </summary>
		/// <returns></returns>

		string GetHtmlInaccesableDataMessage()
		{
			Dictionary<string, List<string>> inaccessableData = Query.InaccessableData;
			if (inaccessableData == null || inaccessableData.Count == 0) return "";

			string message =
					"<table style='font: 9pt Arial; color: #FF0000'> <tr valign='top'> <td>" + // enclose in a table to get a bit of surrounding blank space
							"In the display below the following data will be missing because<br>" +
							"the associated data sources are currently unavailable:<br>";

			foreach (string s in inaccessableData.Keys)
			{
				message += "<br>" + s + ":"; // datasource name or message
				foreach (string mtName in inaccessableData[s]) // list each table in source
				{
					MetaTable mt = MetaTableCollection.Get(mtName);
					if (mtName != null)
						message += "<br>&nbsp;&nbsp;&nbsp;" + mt.Label + "&nbsp;(" + mt.Name + ")";
				}
			}

			message += "<br><br></td></tr></table>";
			return message;
		}

		/// <summary>
		/// Create Html contents for display all data
		/// </summary>
		/// <returns></returns>

		string GetHtmlToc()
		{
			string html;
			MetaTable mt;

			if (!GenerateToc) return "";
			if (TableRowCountMap.Count == 0) return "";

			html = // start with javascript to expand/collapse contents
					"<script language='JavaScript'>\r\n" +
					"function ToggleContentsVisibility()\r\n" +
					"{\r\n" +
					"    if (Contents.style.display =='block')\r\n" +
					"    {\r\n" +
					"     	 Contents.style.display = 'none';\r\n" +
					"     	 ExpandCollapse.src = 'expand.jpg';\r\n" +
					"    }\r\n" +
					"    else\r\n" +
					"    {\r\n" +
					"       Contents.style.display = 'block';\r\n" +
					"     	 ExpandCollapse.src = 'collapse.jpg';\r\n" +
					"    }\r\n" +
					"}\r\n" +
					"</script>\r\n";

			html += // build table of contents
					"<table> <tr valign=\"top\"> <td>\r\n" + // enclose in a table to get a bit of surrounding blank space
					"<table onclick=\"ToggleContentsVisibility()\" \r\n" +
					 "style=\"font: 9pt Arial; text-decoration: underline; color: #0000FF\" " +
					 "border=\"1\" bordercolor=\"#A2BAE9\" borderColorLight=\"#E3EFFF\" " + // light blue border
					 "cellpadding=\"1\" cellspacing=\"0\" bgcolor=\"#FFFFFF\">\r\n" +
					"<tr align=\"center\" valign=\"bottom\">\r\n" +
					"<td bgcolor=\"#E3EFFF\">" +
					"<img id=\"ExpandCollapse\" border=\"0\" src=\"expand.jpg\" width=\"16\" height=\"16\" align=absmiddle>" +
					"&nbsp;Table of Contents for this Page (Click to view)</td></tr></table>\r\n";
			html += "<ol id=\"Contents\" style=\"font: 9pt Arial; display: none\">"; // contents header

			foreach (string tableName in TableRowCountList)
			{
				mt = MetaTableCollection.Get(tableName);
				if (mt == null) continue;
				html += "<li><a href=\"#" + mt.Name + "\">" + mt.Label;
				if ((int)TableRowCountMap[tableName] > 1) // add count if greater than 1
					html += " (" + TableRowCountMap[tableName].ToString() + ")";
				html += "</a></li>\r\n";
			}

			html += "</ol>\r\n"; // end list
			html += "</td></tr></table>\r\n"; // end table enclosing list
			html += "<a name=\"" + "todo - SS.I.KeyMetaTable.Name" + "\"></a>\r\n"; // anchor for first table

			return html;
		}

		/// <summary>
		/// Get end of page html
		/// </summary>
		/// <returns></returns>

		string GetHtmlEnd()  // get html ending code
		{
			return
					"</body>\r\n" +
					"</html>\r\n";
		}

		/// <summary>
		/// Close search
		/// </summary>

		void CloseSearch()
		{
			//			MoreDataRowsAvailable = false;
			return;
		}


#if false
		/// <summary>
		/// Do setup to process a multi-assay summary table row
		/// </summary>
		/// <param name="rt"></param>
		/// <param name="FlatRow"></param>
		/// <param name="NewRt"></param>
		/// <param name="FlatRow2"></param>
		/// <param name="newSi"></param>
		/// <returns></returns>

		bool SetupSummaryTableRow (
			ResultsTable rt,
			FlatRow FlatRow,
			ref ResultsTable rt2,
			ref FlatRow FlatRow2,
			ref int si2)
		{
			ResultsField rfld;
			QueryTable qt, qt2=null;
			QueryColumn qc;
			MetaTable mt, mt2=null;
			MetaColumn mc;
			object [] vo2 = null;
			int ci,i1;

			qt = rt.QueryTable;
			mt = qt.MetaTable;
			if (rt.Fields.Count <= 0) return false;
			rfld = rt.Fields[0];

			if (!Qe.GetPivotedResult(mt,FlatRow.Vo,rfld.VoPosition,ref mt2, ref vo2)) // get pivoted metatable & vo
				return false; 

			// See if we have already built info for this table

			for (i1=0; i1<Rf.TablesExtra.Count; i1++)
			{
				rt2 = Rf.TablesExtra[i1];
				qt2=rt2.QueryTable;
				if (qt2.MetaTable.Name == mt2.Name) break;
			}

			// Build new ResultsTable & QueryTable for this MetaTable

			if (i1 >= Rf.TablesExtra.Count)
			{
				rt2 = new ResultsTable();
				rt2.ResultsFormat = Rf;
				Rf.TablesExtra.Add(rt2);
				qt2 = new QueryTable(mt2);
				rt2.QueryTable = qt2;
				rt2.MetaTable = mt2;

				for (ci=0; ci<mt2.MetaColumns.Count; ci++)
				{
					mc = (MetaColumn)mt2.MetaColumns[ci];
					if (mc.DisplayLevel != DisplayLevel.Default) continue;
					qc = new QueryColumn();
					qc.QueryTable = qt2;
					qc.MetaColumn = mc;
					qc.Selected = true;
					qt2.QueryColumns.Add(qc);

					rfld = new ResultsField();
					rfld.MetaColumn = mc;
					rfld.QueryColumn = qc;
					rfld.ResultsTable = rt2;
					rfld.VoPosition = rt2.Fields.Count;
					rt2.Fields.Add(rfld);
				}
			
				// Finish setting up table & segment info

				QueryFormatter qFmtr = new QueryFormatter();
				qFmtr.Rf = Rf;

				i1=0;
				qFmtr.CalculateColumnWidths(rt2,ref i1);

				int freeColumn=Rf.PageWidth + 1; // force 1st segment to be allocated
				qFmtr.MapTableToSegments(rt2,1,ref freeColumn); // assign segments for table
				qFmtr.FormatSegmentHeaders(rt2); // setup segment headers, footers
			}

			// Set up a new raw tuple with just the data for this table

			FlatRow2 = new FlatRow();
			FlatRow2.Vo = vo2;
			FlatRow2.Cn = FlatRow.Cn;
			FlatRow2.Fresh = FlatRow.Fresh;

			si2 = rt2.FirstSegment;

			return true;
		}
#endif

		/// <summary>
		/// Format & output line of spotfire data type header
		/// </summary>

		void OutputSpotfireDataTypeHeader() // output headers with Spotfire data types
		{
			ResultsTable rt;
			ResultsField rfld;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			char delim;
			int ti, fi;

			string types = "";
			if (Rf.ExportFileFormat == ExportFileFormat.Tsv) delim = '\t';
			else delim = ',';

			for (ti = 0; ti < Rf.Tables.Count; ti++) // each table
			{
				rt = Rf.Tables[ti];
				mt = rt.MetaTable;

				for (fi = 0; fi < rt.Fields.Count; fi++) // each field
				{
					rfld = rt.Fields[fi];
					qc = rfld.QueryColumn;
					mc = rfld.MetaColumn;

					if (types != "") types += delim;

					switch (mc.DataType)
					{
						case MetaColumnType.CompoundId:
							if (mc.IsNumeric) types += "INT";
							else types += "STRING";
							break;

						case MetaColumnType.Integer:
							types += "INT";
							break;

						case MetaColumnType.Number:
							types += "REAL";
							break;

						case MetaColumnType.QualifiedNo:
							if (rfld.QualifiedNumberSplit == QnfEnum.Qualifier || // qualifier piece of qualified number
									rfld.QualifiedNumberSplit == QnfEnum.Combined) // combined qualified number
								types += "STRING";
							else types += "REAL"; // some real numeric piece of a qualified number
							break;

						case MetaColumnType.Date:
							types += "DATE";
							break;

						default: // strings & other field types
							types += "STRING";
							break;
					}
				}
			}

			TextStream.WriteLine(types);
			return;
		}

		/// <summary>
		/// Get field value for a tuple
		/// </summary>
		/// <param name="tupleIndex"></param>
		/// <param name="resultsField"></param>
		/// <returns></returns>

		public object GetTupleFieldValue(
				int tupleIndex,
				ResultsField resultsField)
		{
			DataRowMx dr = DataTable.Rows[tupleIndex];
			return dr[resultsField.VoPosition];
		}

		/// <summary>
		/// Get count of marked rows. 
		/// Note that this value may not be accurate if not all rows have been read in.
		/// </summary>

		public int MarkedRowCount
		{

			get
			{
				if (Rf.CheckMarkVoPos < 0) return 0;

				int count = 0;
				foreach (DataRowMx dr in DataTable.Rows)
				{
					if (dr[Rf.CheckMarkVoPos] is bool && (bool)dr[Rf.CheckMarkVoPos] == true)
						count++;
				}
				return count;
			}
		}

		/// <summary>
		/// Get list of indexes of marked rows. 
		/// Note that this value may not be accurate if not all rows have been read in.
		/// </summary>

		public List<int> MarkedRowIndexes
		{
			get
			{
				List<int> list = new List<int>();
				if (Rf.CheckMarkVoPos < 0) return list;

				for (int ri = 0; ri < DataTable.Rows.Count; ri++)
				{
					DataRowMx dr = DataTable.Rows[ri];

					if (dr[Rf.CheckMarkVoPos] is bool && (bool)dr[Rf.CheckMarkVoPos] == true)
						list.Add(ri);
				}
				return list;
			}
		}

		/// <summary>
		/// Get count of marked rows. 
		/// Note that this value may not be accurate if not all rows have been read in.
		/// </summary>

		public int UnmarkedRowCount
		{
			get
			{
				if (Rf.CheckMarkVoPos < 0) return DataTable.Rows.Count;

				int count = 0;
				foreach (DataRowMx dr in DataTable.Rows)
				{
					if (dr[Rf.CheckMarkVoPos] is bool && (bool)dr[Rf.CheckMarkVoPos] == false)
						count++;
				}
				return count;
			}
		}

		/// <summary>
		/// Get list of indexes of unmarked rows. 
		/// Note that this value may not be accurate if not all rows have been read in.
		/// </summary>

		public List<int> UnmarkedRowIndexes
		{
			get
			{
				List<int> list = new List<int>();

				for (int ri = 0; ri < DataTable.Rows.Count; ri++)
				{
					DataRowMx dr = DataTable.Rows[ri];

					if (Rf.CheckMarkVoPos < 0 || (dr[Rf.CheckMarkVoPos] is bool && (bool)dr[Rf.CheckMarkVoPos] == false))
						list.Add(ri);
				}
				return list;
			}
		}

		/// <summary>
		/// This class checks the client for user cancel
		/// </summary>

		class CheckForCancel // : ICheckForCancel
		{

			public bool IsCancelled
			{
				get
				{
					return Progress.CancelRequested;
				}
			}
		}


		/// <summary>
		/// Create a valid ChemDB field name
		/// </summary>
		/// <param name="fieldName"></param>
		/// <returns></returns>

		public static string BuildValidChemDBFieldName(
				string fieldName)
		{
			Dictionary<string, object> nameDict = new Dictionary<string, object>(); // dictionary of unique names
			return BuildValidAndUniqueChemDBFieldName(nameDict, fieldName, false);
		}

		/// <summary>
		/// Create a valid and unique ChemDB field name
		/// </summary>
		/// <param name="nameDict"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>

		public static string BuildValidAndUniqueChemDBFieldName(
				Dictionary<string, object> nameDict,
				string fieldName,
				bool allowExtraLengthFieldNames)
		{
			// Rules for ChemDB names:
			//
			// Unique name up to 30 characters long. It makes no difference if field names are entered in uppercase, lowercase, or initial capitals. 
			// You can use the following characters in field names:

			// *	Alphabetic characters. For example, A ... Z and a   z. 
			// *	Numeric characters except for the first character of the name. For example, 0,1,...9.

			// *	Other characters such as _ (underscore), , (comma), . (period), / (forward slash), # (pound sign), $ (dollar sign), 
			// * (asterisk), and + (plus sign). 

			// Field names cannot have spaces or these characters:
			// !  @  %  ^  &  (  )  -  =  \  |  {  }  [  ]  <  >  ?  ;  :  '  "  ~

			char[] invalidChars1 = {
								'!', '@', '%', '^', '&', '(', ')', '-', '=', '\\', '|',
								'{', '}', '[', ']', '<', '>', '?', ';', ':', '\'', '"', '~', '\r', '\n'};

			char[] invalidChars2 = { '<', '>' };
			char[] invalidChars;

			//if (Rf.AllowExtraLengthFieldNames)
			//{
			//  label = label.Replace("<", "_"); // still can't allow these chars
			//  label = label.Replace(">", "_");
			//  rfld.MergeLabel = label;
			//}


			fieldName = MetaTable.RemoveSuffixesFromName(fieldName);

			if (!allowExtraLengthFieldNames)
			{
				fieldName = fieldName.Replace("%", " Pct."); // special case
				invalidChars = invalidChars1;
			}
			else invalidChars = invalidChars2;

			foreach (char c in invalidChars)
			{
				if (fieldName.Contains(c.ToString()))
					fieldName = fieldName.Replace(c, ' ');
			}

			fieldName = fieldName.Trim();
			while (fieldName.Contains("  ")) // reduce double spaces
				fieldName = fieldName.Replace("  ", " ");

			if (!allowExtraLengthFieldNames) // replace any blanks with underscores
				fieldName = fieldName.Replace(" ", "_");

			if (!allowExtraLengthFieldNames && fieldName.Length > 30)
				fieldName = fieldName.Substring(0, 30);
			if (!nameDict.ContainsKey(fieldName.ToUpper()))
			{
				nameDict[fieldName.ToUpper()] = null;
				return fieldName;
			}

			if (!allowExtraLengthFieldNames && fieldName.Length > 28)
				fieldName = fieldName.Substring(0, 28);

			for (int i = 2; ; i++) // be sure unique
			{
				string fn2 = fieldName;
				fn2 += i;
				if (!nameDict.ContainsKey(fn2.ToUpper()))
				{
					nameDict[fn2.ToUpper()] = null;
					return fn2;
				}
			}
		}
	} // end of ResultsFormatter

} // end of namespace
