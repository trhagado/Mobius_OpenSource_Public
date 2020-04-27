using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using System;
using System.Runtime.InteropServices;

using System.Reflection;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.IO;

using Microsoft.Win32;

// The following are the Microsoft Office Primary Interop Assemblies (PIAs) used by Mobius (Word also).
// These references are added by selecting from the ExternalResources project. Version 14 (Office 2010) files are used 
// and work with both Office 2010 and Office 2007 (Version 12).
//  1. Microsoft Excel 14.0 Object Library (Microsoft.Office.Interop.Excel.dll)
//     Microsoft Excel 16.0 (Office 2016) (target app, not currently using matching interop)
//  2. Microsoft Word 14.0 Object Library (Microsoft.Office.Interop.Word.dll)
// Selecting these will also add:
//  3. Microsoft.Office.Core (Office.dll)
//  4. VBIDE (Microsoft.Vbe.Interop.dll)
// These files should be copied to the users Mobius\Bin folder
// Also the project for the Startup.exe must include these references as well.
//
// If there is a problem at runtime finding the assemblies or with version mismatches check
// the .config file for unwanted specific version requirements and remove if found.

// --- February 2018 ---- 
// Insight for Excel support added.
//
// For Insight for Excel no Mobius VBA code is included in the MobiusInsight4XLTemplate.xlsx file.
// The InsightChemistry.xlam!IxlPutChemistryInCell macro method is called directly from this module. 
// This simplifies the overall code.
// Note that the InsightChemistry.xlam addin is explicitly loaded after opening the template file but
// before inserting structures into the worksheet.

using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;
using Microsoft.Vbe.Interop;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Summary description for Excel.
	/// </summary>

	public class ExcelOp
	{
		static Process ExcelProcess;

		// Excel objects

		static Microsoft.Office.Interop.Excel._Application XlApp;
		static Microsoft.Office.Interop.Excel.Workbooks XlBooks;
		static Microsoft.Office.Interop.Excel._Workbook XlBook;
		static Microsoft.Office.Interop.Excel._Workbook XlBook2;
		static Microsoft.Office.Interop.Excel._Worksheet XlSheet;
		static Microsoft.Office.Interop.Excel._Worksheet XlSheet2;
		static Microsoft.Office.Interop.Excel.QueryTables XlQueryTables;
		static Microsoft.Office.Interop.Excel.Range XlRange;
		static Microsoft.Office.Interop.Excel.ShapeRange XlShapeRange;
		static Microsoft.Office.Interop.Excel.Shapes XlShapes;
		static Microsoft.Office.Interop.Excel.Shape XlShape;
		static Microsoft.Office.Interop.Excel.Pictures XlPictures;

		public static bool LogCalls
		{
			get
			{
				if (_logCalls == null)
				{
					if (SS.I?.UserIniFile == null) return false;
					_logCalls = SS.I.UserIniFile.ReadBool("ExcelOpLogCalls", false);
				}

				return (bool)_logCalls;
			}
		}
		static bool? _logCalls = null;

		static bool AdjustHighDpiRowHeight = true; // help with aspect ratio of Helm depicictions

		static bool KeepExcelVisibleForDebugging = false;
		static bool DebugFileSaveOperation = false;
		static bool InsightForExcelAddInLoaded = false;
		static int StructureInsertCount = 0; // number of structures inserted 

		static double WindowLeft; // original left coord of Excel window

		// Excel constants

		public const int Left = -4131;
		public const int Center = -4108;
		public const int Right = -4152;
		public const int Top = -4160;
		public const int Bottom = -4107;

		public ExcelOp()
		{
			return;
		}

		/// <summary>
		/// Create the Excel object
		/// </summary>

		public static void CreateObject()
		{
			KeepExcelVisibleForDebugging = SS.I.UserIniFile.ReadBool("ExcelOpKeepExcelVisibleForDebugging", KeepExcelVisibleForDebugging);

			if (LogCalls) DebugLog.Message("ExcelOp CreateObject");

			XlApp = new Microsoft.Office.Interop.Excel.Application();
			MessageFilter.Register(); // filter messages, e.g. RetryRejectedCall ("Call was rejected by callee")
			ExcelProcess = GetNewExcelProcess();

			XlBooks = XlApp.Workbooks;

			try // avoid popups that lockup Excel
			{
				XlApp.AskToUpdateLinks = false; // XlApp.Workbooks.Open Filename:="C:\Book1withLinkToBook2.xlsx", UpdateLinks:=False also works
				XlApp.DisplayAlerts = false; // avoid alert popup dialogs
				//XlApp.AutomationSecurity = // AutomationSecurity normally starts at: msoAutomationSecurityLow
				//  MsoAutomationSecurity.msoAutomationSecurityForceDisable; // setting to ForceDisable won't allow structures to be inserted which is something that we don't want 
			}

			catch (Exception ex) { ex = ex; }

			Version version = ExcelOp.GetExcelVersion();
			bool visible = XlApp.Visible;

			StructureInsertCount = 0;

			if (!KeepExcelVisibleForDebugging)
			{
				ShowExcelApp(false); // hide the app

				//if (version.Major >= 14) // >= Excel 2010 - window visible & no screen updating during sheet build
				//{
				//	ExcelOp.HideExcelApp(true); 
				//	ExcelOp.ScreenUpdating(false);
				//}

				//else // Excel 2007 or earlier - window invisible & no screen updating during sheet build
				//{
				//	ExcelOp.HideExcelApp(false); // hide Excel instance
				//	ExcelOp.ScreenUpdating(false);
				//}
			}

			else // make visible for dev/debug
			{
				XlApp.Visible = true;
				XlApp.WindowState = XlWindowState.xlNormal;
			}

			return;
		}

		/// <summary>
		/// Load the Insight addin so that it is available to call
		/// </summary>

		static bool LoadInsightForExcelAddIn()
		{
			bool installed = false;
			if (!IsInsightForExcelAvailable) return false;

			int addInCount = XlApp.AddIns.Count;
			string addInList = "";

			foreach (Microsoft.Office.Interop.Excel.AddIn addIn in XlApp.AddIns)
			{
				string name = addIn.Name;
				string fullName = addIn.FullName;
				addInList += name + ", " + fullName + ", " + addIn.Installed + "\r\n";
				if (!Lex.Contains(name, "InsightChemistry")) continue;

				try
				{
					installed = addIn.Installed;
					addIn.Installed = false; // toggle value
					addIn.Installed = true;
					installed = addIn.Installed;
					return installed;
				}
				catch (Exception ex)
				{
					DebugLog.Message(ex);
					return false;
				}
			}

			return false;
		}

		/// <summary>
		/// Get Excel version
		/// </summary>
		/// <returns></returns>

		public static Version GetExcelVersion()
		{
			string vs = GetVersionString();
			Version v = new Version(vs);
			return v;
		}

		/// <summary>
		/// Get Excel version string
		/// </summary>
		/// <returns></returns>

		public static string GetVersionString()
		{
			string version = XlApp.Version;
			return version;
		}

		/// <summary>
		/// Open an Excel file
		/// </summary>
		/// <param name="fileName"></param>

		public static void Open(string fileName)
		{
			if (LogCalls) DebugLog.Message("ExcelOp Open " + fileName);

			if (!File.Exists(fileName)) throw new Exception("File doesn't exist: " + fileName);

			ReleaseObject(XlBook);
			XlBook = XlBooks.Open(Filename: fileName);
			//XlBook = XlBooks.Add(); // start new WorkBook rather than opening existing file

			if (XlBook == null) throw new Exception("Error opening workBook: " + fileName);

			ReleaseObject(XlSheet);
			XlSheet = (_Worksheet)XlApp.ActiveSheet;
			if (XlSheet == null) throw new Exception("Error getting active sheet");

			if (Lex.EndsWith(fileName, ".csv")) XlSheet.Name = "Sheet1"; // set normal sheet name if opening .csv file

			return;
		}

		/// <summary>
		/// Import a .csv file
		/// </summary>
		/// <param name="?"></param>

		public static void ImportCsv(string fileName)
		{
			if (LogCalls) DebugLog.Message("ExcelOp ImportCsv " + fileName);

			try
			{
				ReleaseObject(XlQueryTables);
				XlQueryTables = (Microsoft.Office.Interop.Excel.QueryTables)XlSheet.QueryTables;
				QueryTables qt = XlQueryTables;

				CellSelect(1, 1); // set XlRange to ("$A$1")

				// Add a QueryTable for the file

				XlQueryTables.Add(
					Connection: "TEXT;" + fileName,
					Destination: XlRange);

				// Set the other QueryTable properties 

				qt[1].Name = Path.GetFileNameWithoutExtension(fileName);
				//qt[1].FieldNames = true;
				//qt[1].RowNumbers = false;
				//qt[1].FillAdjacentFormulas = false;
				//qt[1].PreserveFormatting = true;
				//qt[1].RefreshOnFileOpen = false;
				//qt[1].RefreshStyle = XlCellInsertionMode.xlInsertDeleteCells;
				//qt[1].SavePassword = false;
				//qt[1].SaveData = true;
				//qt[1].AdjustColumnWidth = true;
				//qt[1].RefreshPeriod = 0;
				qt[1].TextFilePromptOnRefresh = false;
				qt[1].TextFilePlatform = 437; // default = 2
				qt[1].TextFileStartRow = 1;
				qt[1].TextFileParseType = XlTextParsingType.xlDelimited;
				qt[1].TextFileTextQualifier = XlTextQualifier.xlTextQualifierDoubleQuote;
				qt[1].TextFileConsecutiveDelimiter = false;
				qt[1].TextFileTabDelimiter = false; // default: true
				qt[1].TextFileSemicolonDelimiter = false; // default: true
				qt[1].TextFileCommaDelimiter = true; // default: false
				qt[1].TextFileSpaceDelimiter = false;
				qt[1].TextFileTrailingMinusNumbers = true;

				qt[1].Refresh(false);
				qt[1].Delete(); // cleanup

				return;
			}

			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}

		}

/// <summary>
/// Set the text for a cell 
/// </summary>
/// <param name="row"></param>
/// <param name="col"></param>
/// <param name="text"></param>

		public static void SetCellText(int row, int col, string text)
		{
			if (LogCalls) DebugLog.Message("ExcelOp SetCellText " + row + ", " + col + ", " + text);

			XlSheet.Cells[row, col] = text;
			return;
		}

		/// <summary>
		/// Check if InsightForExcel is available on the current PC
		/// </summary>

		public static bool IsInsightForExcelAvailable
		{
			get
			{
				if (_insightAvailable == null)
					_insightAvailable = GetIsInsightForExcelAvailable();

				return (bool)_insightAvailable;
			}
		}
		static bool? _insightAvailable = null;

		static bool GetIsInsightForExcelAvailable()
		{
			return false; 
		}

		/// <summary>
		/// Insert molecule into Excel table cell
		/// </summary>
		/// <param name="cs"></param>
		/// <param name="row"></param>
		/// <param name="col"></param>

		public static void SetCellMolecule(
			int row,
			int col,
			MoleculeMx cs)
		{
			if (IsInsightForExcelAvailable)
				SetInsightForExcelMolecule(row, col, cs);

			return;
		}

		/// Insert Insight for Excel structure

		public static void SetInsightForExcelMolecule(
			int row,
			int col,
			MoleculeMx mol)
		{

			if (LogCalls) DebugLog.Message("ExcelOp SetInsightForExcelMolecule " + row + ", " + col);

			if (StructureInsertCount == 0) // check that AddIn is loaded
			{
				InsightForExcelAddInLoaded = LoadInsightForExcelAddIn();
				if (!InsightForExcelAddInLoaded)
					DebugLog.Message("Insight Chemistry AddIn failed to load");
			}

			StructureInsertCount++;

			try
			{
				if (MoleculeMx.IsUndefined(mol)) return;

				string molFile = mol.GetMolfileString();
				if (!InsightForExcelAddInLoaded) throw new Exception("Insight Chemistry AddIn failed to load");

				CellSelect(row, col); // outputCell
				string errorString = "";
				bool returnStatus = false;
				string textFormat = "Automatic";

				string macroPath = "InsightChemistry.xlam!IxlPutChemistryInCell"; // path to macro (don't use fullPath = XlApp.LibraryPath + @"\" + "InsightChemistry.xlam!IxlPutChemistryInCell")

				string fullPath = XlApp.LibraryPath + @"\" + macroPath; // (do not use full path) 

				//if (StructureInsertCount == 2) throw new Exception("Test Exception"); // debug 

				object ro = XlApp.Run( // call Insight macro directly (i.e. no added VBA macro in sheet)
					Macro: macroPath,
					Arg1: molFile,
					Arg2: XlRange,
					Arg3: errorString,
					Arg4: returnStatus,
					Arg5: textFormat);

				ReleaseObject(ro);

				return;
			}

			catch (Exception ex)
			{
				string msg = ex.Message;
				try
				{
					msg =
						"Insight for Excel Failed\r\n" +
						"Molecule: " + StructureInsertCount + ", Atoms: " + mol.AtomCount;
					XlSheet.Cells[row, col] = msg;
				}
				catch (Exception ex2) { ex2 = ex2; }
				throw new Exception(msg, ex); // throw exception so failure gets counted
			}
		}

		/// <summary>
		/// Show or hide the Excel app
		/// </summary>
		/// <param name="show"></param>

		static void ShowExcelApp(bool show)
		{
			if (KeepExcelVisibleForDebugging) return; // deubg

			if (LogCalls) DebugLog.Message("ExcelOp HideExcelApp " + show);

			try
			{

				if (show) // move Excel app window to upper left of screen
				{
					XlApp.Visible = false; // hide while moving
					XlApp.WindowState = XlWindowState.xlNormal;
					XlApp.Left = 0;
					XlApp.Top = 0;
					XlApp.ScreenUpdating = true;
					XlApp.Visible = true;
				}

				else // move the Excel app window off screen but keep Visible = true, turn off updating
				{
					XlApp.Visible = false; // hide while moving
					XlApp.WindowState = XlWindowState.xlNormal;
					XlApp.Left = 16000; // move off screen
					XlApp.ScreenUpdating = false;
					XlApp.Visible = true;
				}
			}
			catch (Exception ex) { ex = ex; }
		}

		///// <summary>
		///// Turn screen updating on/off 
		///// </summary>
		///// <param name="updateScreen"></param>

		//public static void ScreenUpdating(bool updateScreen)
		//{
		//	if (BlockScreenVisibilityChanges) return; // debug

		//	if (LogCalls) DebugLog.Message("ExcelOp ScreenUpdating");
		//	try
		//	{
		//		XlApp.ScreenUpdating = updateScreen;

		//		//if (!updateScreen)
		//		//{
		//		//	if (XlApp.Left < 16000)
		//		//		WindowLeft = XlApp.Left;
		//		//	XlApp.Left = 16000;  // move off screen
		//		//}

		//		//else XlApp.Left = WindowLeft;
		//	}
		//	catch (Exception ex) { ex = ex; }
		//}


		/// <summary>
		/// Remove modules from excel file to avoid Excel Security warning startup message
		/// </summary>

		public static void RemoveModules()
		{
			if (LogCalls) DebugLog.Message("ExcelOp RemoveModules");

			// Tools > Macro > Security > Security Level must be set to Medium.
			// Also Tools > Macro > Security > Trusted Sources >
			// Trust access to Visual Basic Project must be enabled
			// (See: http://support.microsoft.com/kb/282830/)

			try // remove modules
			{
				if (IsInsightForExcelAvailable) // call method in module if Insight
				{
					//object ro = XlApp.Run("DeleteVbaModule");
					return;
				}

				else
				{
					foreach (VBComponent vbc in XlBook.VBProject.VBComponents)
					{
						string s = vbc.Name;
						if (s.ToLower().StartsWith("module")) // just modules
							XlBook.VBProject.VBComponents.Remove(vbc);
					}

					return;
				}
			}
			catch (Exception ex) { ClientLog.Message("RemoveModules - " + ex.Message); }
		}

		/// <summary>
		/// Save file
		/// </summary>

		public static void Save()
		{
			if (LogCalls) DebugLog.Message("ExcelOp Save");

			try
			{
				XlBook.Save();
				return;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}

		}

		/// <summary>
		/// Save file in .xlsx format
		/// </summary>
		/// <param name="?"></param>

		public static void SaveAs(string fileName)
		{
			XlFileFormat ff = new XlFileFormat();

			if (DebugFileSaveOperation) return; // debug

			//if (DebugMx.True) // debug
			//{
			//	Save();
			//	return;
			//}

			if (LogCalls) DebugLog.Message("ExcelOp SaveAs " + fileName);

			try
			{
				ShowExcelApp(true); // make visible before save so visible when reopened later

				if (Lex.EndsWith(fileName, ".xlsx")) // Excel 2007+ (version 12+) Workbook (.xlsx) open XML format
				{
					ff = XlFileFormat.xlOpenXMLWorkbook; // 51 = xlsx

					//fileName = Lex.Replace(fileName, ".xlsx", ".xlsm"); // debug
					//ff = XlFileFormat.xlOpenXMLWorkbookMacroEnabled; // debug
				}

				else if (Lex.EndsWith(fileName, ".xlsm")) // Excel Macro-Enabled Workbook (.xlsm) open XML format
					ff = XlFileFormat.xlOpenXMLWorkbookMacroEnabled;

				else if (Lex.EndsWith(fileName, ".xlsb")) // Excel Binary Workbook (.xlsx) (Excel 2007, version 12) binary format
					ff = XlFileFormat.xlExcel12;

				else if (Lex.EndsWith(fileName, ".xls")) // Excel 97-2003 (version 8-11) Workbook (*.xls) proprietary binary format
					ff = XlFileFormat.xlExcel8;

				else ff = XlFileFormat.xlWorkbookDefault; // use default for others

				XlBook.SaveAs(
					Filename: fileName,
					FileFormat: ff,
					AccessMode: XlSaveAsAccessMode.xlNoChange);
			}
			catch (Exception ex)
			{
				throw new Exception("File: " + fileName + "\n\n" + ex.Message);
			}

			return;
		}

		/// <summary>
		/// Save file in .csv format
		/// </summary>
		/// <param name="fileName"></param>

		public static void SaveAsCsv(string fileName)
		{
			if (LogCalls) DebugLog.Message("ExcelOp SaveAsCsv " + fileName);

			try
			{
				XlBook.SaveAs(
					fileName, // Filename , 
					XlFileFormat.xlCSV, // FileFormat 
					Type.Missing, // Password 
					Type.Missing, // WriteResPassword 
					Type.Missing, // ReadOnlyRecommended
					false, // CreateBackup 
					Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, // AccessMode 
					Type.Missing, // ConflictResolution 
					Type.Missing, // AddToMru 
					Type.Missing, // TextCodepage 
					Type.Missing, // TextVisualLayout 
					Type.Missing); // Local
			}
			catch (Exception ex)
			{
				throw new Exception("File: " + fileName + "\n\n" + ex.Message);
			}
		}

		/// <summary>
		/// Close main workbook
		/// </summary>

		public static void Close()
		{
			if (DebugFileSaveOperation) return; // debug

			if (LogCalls) DebugLog.Message("ExcelOp Close");

			try
			{
				XlBook.Close(SaveChanges: false); // seems to throw exception
																					//XlBook.Close(false, Type.Missing, Type.Missing);
			}

			catch (Exception ex)
			{
				ex = ex; // ignore error
								 //throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// Close secondary workbook
		/// </summary>

		public static void Close2() // 
		{
			if (DebugFileSaveOperation) return; // debug

			if (LogCalls) DebugLog.Message("ExcelOp Close2");

			XlBook2.Close(SaveChanges: false);
			//XlBook2.Close(false, Type.Missing, Type.Missing);
		}

		/// <summary>
		/// Quit Excel app
		/// </summary>

		public static void Quit()
		{
			if (DebugFileSaveOperation) return; // debug

			if (LogCalls) DebugLog.Message("ExcelOp Quit");

			ReleaseAllObjects();
			XlApp.Quit();

			MessageFilter.Revoke();

			//System.Windows.Forms.Application.DoEvents();
			//Thread.Sleep(5000); // sleep to avoid race
			//System.Windows.Forms.Application.DoEvents();
		}

		/// <summary>
		/// Release all references, must do lower to higher level...
		/// </summary>

		public static void DeleteObject()
		{
			if (LogCalls) DebugLog.Message("ExcelOp DeleteObject");

			ReleaseAllObjects();
			ReleaseObject(XlApp);
			XlApp = null;

			GC.Collect();
			GC.WaitForPendingFinalizers();

			if (DebugFileSaveOperation) return; // debug

			try // hack to kill Excel process until proper close can be done
			{
				if (ExcelProcess != null && !ExcelProcess.HasExited) ExcelProcess.Kill();
			}
			catch (Exception ex)
			{
				throw new Exception("Kill Process Failed: " + ex.Message);
			}
		}

		/// <summary>
		/// Select a cell
		/// </summary>
		/// <param name="rowIdx"></param>
		/// <param name="columnIdx"></param>

		public static void CellSelect(
			int rowIdx,
			int columnIdx)
		{
			if (LogCalls) DebugLog.Message("ExcelOp CellSelect " + rowIdx + ", " + columnIdx);

			ReleaseObject(XlRange);
			XlRange = (Microsoft.Office.Interop.Excel.Range)XlSheet.Cells[rowIdx, columnIdx];
			return;
		}

		/// <summary>
		/// Select column
		/// </summary>
		/// <param name="columnIdx"></param>
		public static void ColumnsSelect(int columnIdx)
		{
			if (LogCalls) DebugLog.Message("ExcelOp ColumnSelect " + columnIdx);

			ReleaseObject(XlRange);
			XlRange = (Microsoft.Office.Interop.Excel.Range)XlApp.Columns[columnIdx, Type.Missing];
			return;
		}

		public static void RangeSelect(string cellString) // select cell
		{
			ReleaseObject(XlRange);
			XlRange = (Microsoft.Office.Interop.Excel.Range)XlApp.get_Range(cellString, Type.Missing);
			XlRange.Select();
		}

		/// <summary>
		/// RowHeight
		/// </summary>
		/// <param name="height"></param>

		public static void RowHeight(double height)
		{
			const int MaxRowHeight = 409; // max Excel 2010 row height in points

			if (LogCalls) DebugLog.Message("ExcelOp RowHeight " + height);

			if (height > MaxRowHeight) height = MaxRowHeight; // keep within limit

			try
			{
				if ((double)XlRange.RowHeight < height)
					XlRange.RowHeight = height;
			}
			catch (Exception ex)
			{ // log Mobius 3.0 issue #200 (T. Richardson) exception
				string msg =
					"ExcelOp.RowHeight (" + XlRange.Row + ", " + XlRange.Column + ") = " + height + "\r\n" +
					DebugLog.FormatExceptionMessage(ex);
				ServicesLog.Message(msg);
				return;
			}
		}

		/// <summary>
		/// InsertPicture
		/// </summary>
		/// <param name="fileName"></param>

		public static void InsertPicture(
			string fileName,
			float dx,
			float dy,
			float width,
			float height,
			float scale = -1)
		{

			if (LogCalls) DebugLog.Message("ExcelOp InsertPicture " + fileName);

			float left = (float)((double)XlApp.ActiveCell.Left) + dx;
			float top = (float)((double)XlApp.ActiveCell.Top) + dy;

			float origWidth = width;
			if (width <= 0) width = (float)((double)XlApp.ActiveCell.Width);

			float origHeight = height;
			if (height <= 0) height = (float)((double)XlApp.ActiveCell.Height);

			bool scaleShapeSize = false;
			if (scale > 0)
			{
				left *= scale;
				top *= scale;
				width *= scale;
				height *= scale;

				float rowHeight = height;

				if (scale > 1 && AdjustHighDpiRowHeight) // fudge to adjust row height better for relatively tall images
				{
					if (height < width)
					{
						float fFactor = ((width - height) / width); // varies 1 for no height to 0 for height == width
						float scale2 = 1 + ((scale - 1) * fFactor);
						rowHeight = origHeight * scale2;
					}

					else rowHeight = origHeight;
				}

				RowHeight(rowHeight); // be sure row is at least this height

				//scaleShapeSize = true; // don't turn on shape scaling, doesn't seem to help
			}

			else scale = 1.0f;

			MsoTriState linkToFile = MsoTriState.msoFalse;
			MsoTriState saveToDocument = MsoTriState.msoTrue;

			XlShape = XlSheet.Shapes.AddPicture(fileName, linkToFile, saveToDocument, left, top, width, height);

			if (scaleShapeSize) // adjust scaling of the shape just created
			{
				MsoTriState scaleRelativeToOriginalSize = MsoTriState.msoTrue;
				MsoScaleFrom scaleFrom = MsoScaleFrom.msoScaleFromTopLeft;
					 
				XlShape.LockAspectRatio = MsoTriState.msoTrue; 
				XlShape.ScaleWidth(scale, scaleRelativeToOriginalSize, scaleFrom); 
				//XlShape.ScaleHeight(scale, scaleRelativeToOriginalSize, scaleFrom);
			}

			ReleaseObject(XlShape);
		}

		public static void ShapeRangeHeight(int height)
		{
			if (LogCalls) DebugLog.Message("ExcelOp ShapeRangeHeight " + height);

			XlShapeRange.Height = height;
		}

		public static void ShapeRangeWidth(int width)
		{
			if (LogCalls) DebugLog.Message("ExcelOp ShapeRangeWidth " + width);

			XlShapeRange.Width = width;
		}

		public static void ShapeRangeMoveRelative(int dx, int dy)
		{
			if (LogCalls) DebugLog.Message("ExcelOp ShapeRangeMoveRelative " + dx + ", " + dy);

			XlShapeRange.Left = XlShapeRange.Left + dx;
			XlShapeRange.Top = XlShapeRange.Top + dy;
		}

		public static void FontAttributes(System.Drawing.Font f)
		{
			if (LogCalls) DebugLog.Message("ExcelOp FontAttributes");

			Microsoft.Office.Interop.Excel.Font font;
			font = XlRange.Font;
			font.Name = f.Name;
			font.Size = f.Size;

			ReleaseObject(font);
			font = null;

			SetFontStyle(f.Style);
		}

		public static void SetFontStyle(FontStyle fs)
		{
			if (LogCalls) DebugLog.Message("ExcelOp SetFontStyle");

			string tok = "";

			Microsoft.Office.Interop.Excel.Font font;
			font = XlRange.Font;

			if (fs == (FontStyle.Bold & FontStyle.Italic)) tok = "Bold Italic";
			else if (fs == FontStyle.Bold) tok = "Bold";
			else if (fs == FontStyle.Italic) tok = "Italic";
			else tok = "Regular";
			font.FontStyle = tok;

			ReleaseObject(font);
			font = null;
		}

		public static void FontColor(Color c)
		{
			if (LogCalls) DebugLog.Message("ExcelOp FontColor");

			Microsoft.Office.Interop.Excel.Font font;
			font = XlRange.Font;
			int rgb = c.R + c.G * 256 + c.B * 65536;
			font.Color = rgb;
			ReleaseObject(font);
			font = null;
		}

		public static void BackColor(Color c)
		{
			if (LogCalls) DebugLog.Message("ExcelOp BackColor");

			int rgb = c.R + c.G * 256 + c.B * 65536;
			XlRange.Interior.Color = rgb;
		}

		public static void ColumnWidth(double width)
		{
			if (LogCalls) DebugLog.Message("ExcelOp ColumnWidth " + width);

			XlRange.ColumnWidth = width;
		}

		public static void WrapText(bool wrap)
		{
			if (LogCalls) DebugLog.Message("ExcelOp Wrap " + wrap);

			XlRange.WrapText = wrap;
		}

		public static void HorizontalAlignment(int align)
		{
			if (LogCalls) DebugLog.Message("ExcelOp HorizontalAlignment " + align);

			XlRange.HorizontalAlignment = align;
		}

		public static void VerticalAlignment(int align)
		{
			if (LogCalls) DebugLog.Message("ExcelOp VerticalAlignment " + align);

			try
			{
				if (XlRange == null) DebugLog.Message("Range object is null");
				XlRange.VerticalAlignment = align;
			}
			catch (Exception ex)
			{
				DebugLog.Message(@"Mobius Warning: Unable to set VerticalAlignment to '" + align + @"'. Skipping Vertical Alignemnt.");
				DebugLog.Message("Mobius Warning: " + ex.Message);
				DebugLog.Message("Mobius Warning: Number of Cells - " + XlRange.Cells.Count);
			}
		}

		public static void NumberFormat(string format) // format as text to keep exact representation
		{
			if (LogCalls) DebugLog.Message("ExcelOp NumberFormat " + format);

			XlRange.NumberFormat = format;
		}

		public static void Merge()
		{
			if (LogCalls) DebugLog.Message("ExcelOp Merge");

			XlRange.Merge(Type.Missing);
		}

		public static void RowsAutofit()
		{
			if (LogCalls) DebugLog.Message("RowsAutoFit");

			XlRange.Rows.AutoFit();
		}

		/// <summary>
		/// Explicitly release all com objects
		/// </summary>

		static void ReleaseAllObjects()
		{
			if (LogCalls) DebugLog.Message("ReleaseAllObjects");

			ReleaseObject(XlRange);
			ReleaseObject(XlShapeRange);
			ReleaseObject(XlShapes);
			//			ReleaseObject(XlSelection);
			//			ReleaseObject(XlTemp);

			ReleaseObject(XlQueryTables);
			ReleaseObject(XlPictures);
			ReleaseObject(XlSheet);
			ReleaseObject(XlBook);
			ReleaseObject(XlSheet2);
			ReleaseObject(XlBook2);
			ReleaseObject(XlBooks);

			XlRange = null;
			XlShapeRange = null;
			XlShapes = null;
			//		XlSelection = null;
			//		XlTemp = null;
			XlQueryTables = null;
			XlPictures = null;
			XlSheet = null;
			XlBook = null;
			XlSheet2 = null;
			XlBook2 = null;
			XlBooks = null;
		}

		/// <summary>
		/// Explicitly release a com object
		/// </summary>
		/// <param name="obj"></param>

		static void ReleaseObject(
			object obj)
		{
			if (obj == null) return;
			int refCnt = Marshal.ReleaseComObject(obj);
			if (refCnt != 0) refCnt = refCnt; // debug
			obj = null;
		}

		static void ShutDown() // immediate shutdown, used for debugging
		{
			if (XlBook != null)
				XlBook.Close(false, Type.Missing, Type.Missing);

			ReleaseAllObjects();
			XlApp.Quit();

			Thread.Sleep(5000); // sleep to avoid race

			ReleaseObject(XlApp);
			XlApp = null;

			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		/// <summary>
		/// Convert an .xls file to a csv file
		/// </summary>
		/// <param name="xlsFile"></param>
		/// <param name="csvFile"></param>

		public static void XlsToCsv(
			string xlsFile,
			string csvFile)
		{
			if (LogCalls) DebugLog.Message("XlsToCsv " + xlsFile + ", " + csvFile);

			string xlsFile2 = "", tok;

			for (int i = 1; i <= 3; i++) // try up to three times since the conversion sometimes fails for unknown reasons
			{
				try
				{
					if (xlsFile.StartsWith(@"\\"))
					{ // copy from share to local file 
						string ext = Path.GetExtension(xlsFile);
						xlsFile2 = TempFile.GetTempFileName(ClientDirs.TempDir, ext, false);
						File.Copy(xlsFile, xlsFile2, true);
					}

					else xlsFile2 = xlsFile;

					CreateObject();
					//ScreenUpdating(false); // no screen updating during sheet build
					Open(xlsFile2); // "The remote procedure call failed" or "Call was rejected by callee (on get_ActiveSheet)" may occur here
					SaveAsCsv(csvFile);
					try { Close(); }
					catch (Exception ex) { ex = ex; } // "Call was rejected by callee" may occur here, ignore if so
					try { Quit(); }
					catch (Exception ex) { ex = ex; }
					try { DeleteObject(); }
					catch (Exception ex) { ex = ex; }

					if (xlsFile2 != xlsFile) try { File.Delete(xlsFile2); }
						catch (Exception ex) { ex = ex; }

					//SystemUtil.Beep(); // debug
					return; // success
				}

				catch (Exception ex)
				{
					if (i < 3) // allow two retries
					{
						try
						{
							Quit();
							DeleteObject();
							Thread.Sleep(500); // sleep a bit before retry
						}
						catch (Exception ex2) { ex2 = ex2; }
					}

					else // log and throw exception if final try fails
					{
						ServicesLog.Message("XlsToCsv failed for file: " + xlsFile + ", " + xlsFile2 + "\r\n\r\n" +
							DebugLog.FormatExceptionMessage(ex));
						throw new Exception(ex.Message, ex);
					}

				}
			}

			return;
		}

		public static void Test()
		{
			Microsoft.Office.Interop.Excel._Application xl = null;
			Microsoft.Office.Interop.Excel.Workbooks wbs = null;
			Microsoft.Office.Interop.Excel._Workbook wb = null;
			Microsoft.Office.Interop.Excel._Worksheet sheet = null;

			int refCnt;

			//			GC.Collect(); 
			xl = new Microsoft.Office.Interop.Excel.Application();
			xl.Visible = true;
			wbs = xl.Workbooks;
			//			wb = wbs.Open(@"C:\Mobius_OpenSource\opentest.xls",Missing.Value,Missing.Value,Missing.Value,Missing.Value,Missing.Value,Missing.Value,Missing.Value,Missing.Value,Missing.Value,Missing.Value,Missing.Value,Missing.Value);
			wb = wbs.Add(Missing.Value);
			//			wb = (Microsoft.Office.Interop.Excel._Workbook)(xl.Workbooks.Add( Missing.Value ));

			sheet = (Microsoft.Office.Interop.Excel._Worksheet)wb.ActiveSheet;

			// Once data is put on the sheet Excel process doesn't go away
			// Ok if no data on sheet
			// Excel 2000 - 9.0 
			// 2002 = 10.0
			// 2003 = 11.0
#if true
			for (int r = 0; r < 20; r++)
			{

				for (int c = 0; c < 10; c++)
				{
					sheet.Cells[r + 1, c + 1] = 125;
				}

			}
#endif

			//			wb.SaveAs(@"C:\Mobius_OpenSource\savetest.xls",Excel.XlFileFormat.xlWorkbookNormal,
			//				null,null,false,false,Excel.XlSaveAsAccessMode.xlShared,
			//				null,null,null,null);

			wb.Close(false, null, null);
			xl.Workbooks.Close();

			xl.Quit();

			Thread.Sleep(5000); // delay to avoid race condition

			if (sheet != null) { refCnt = Marshal.ReleaseComObject(sheet); }
			if (wb != null) { refCnt = Marshal.ReleaseComObject(wb); }
			if (wbs != null) { refCnt = Marshal.ReleaseComObject(wbs); }
			if (xl != null) { refCnt = Marshal.ReleaseComObject(xl); }

			sheet = null;
			wb = null;
			wbs = null;
			xl = null;
			GC.Collect();
			return;
		}

		/// <summary>
		/// Get the latest process as the Excel process
		/// </summary>
		/// <returns></returns>

		static Process GetNewExcelProcess()
		{
			Process[] processList = Process.GetProcessesByName("Excel"); // get Excel processes

			Process p = null, p2 = null;
			int pi = 0;

			for (pi = 0; pi < processList.Length; pi++)
			{
				p2 = processList[pi];
				if (p == null) p = p2;
				else if (p2.StartTime > p.StartTime) p = p2;
			}
			return p;
		}

	}

	/// <summary>
	/// Class containing the IOleMessageFilter thread error-handling functions.
	/// </summary>

	public class MessageFilter : IOleMessageFilter
	{
		//
		// Class containing the IOleMessageFilter
		// thread error-handling functions.

		// Start the filter.
		public static void Register()
		{
			IOleMessageFilter newFilter = new MessageFilter();
			IOleMessageFilter oldFilter = null;
			CoRegisterMessageFilter(newFilter, out oldFilter);
		}

		// Done with the filter, close it.
		public static void Revoke()
		{
			IOleMessageFilter oldFilter = null;
			CoRegisterMessageFilter(null, out oldFilter);
		}

		// IOleMessageFilter functions.
		// Handle incoming thread requests.
		int IOleMessageFilter.HandleInComingCall(int dwCallType,
			System.IntPtr hTaskCaller, int dwTickCount, System.IntPtr
			lpInterfaceInfo)
		{
			//Return the flag SERVERCALL_ISHANDLED.
			return 0;
		}

		// Thread call was rejected, so try again.
		int IOleMessageFilter.RetryRejectedCall(System.IntPtr
			hTaskCallee, int dwTickCount, int dwRejectType)
		{
			if (dwRejectType == 2)
			// flag = SERVERCALL_RETRYLATER.
			{
				// Retry the thread call immediately if return >=0 & 
				// <100.
				return 99;
			}
			// Too busy; cancel call.
			return -1;
		}

		int IOleMessageFilter.MessagePending(System.IntPtr hTaskCallee,
			int dwTickCount, int dwPendingType)
		{
			//Return the flag PENDINGMSG_WAITDEFPROCESS.
			return 2;
		}

		// Implement the IOleMessageFilter interface.
		[DllImport("Ole32.dll")]
		private static extern int
			CoRegisterMessageFilter(IOleMessageFilter newFilter, out
					IOleMessageFilter oldFilter);
	}

	[ComImport(), Guid("00000016-0000-0000-C000-000000000046"),
	InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	interface IOleMessageFilter
	{
		[PreserveSig]
		int HandleInComingCall(
				int dwCallType,
				IntPtr hTaskCaller,
				int dwTickCount,
				IntPtr lpInterfaceInfo);

		[PreserveSig]
		int RetryRejectedCall(
				IntPtr hTaskCallee,
				int dwTickCount,
				int dwRejectType);

		[PreserveSig]
		int MessagePending(
				IntPtr hTaskCallee,
				int dwTickCount,
				int dwPendingType);
	}
}
