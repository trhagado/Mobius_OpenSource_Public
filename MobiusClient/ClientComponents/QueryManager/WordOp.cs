using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using System;
using System.IO;
using System.Drawing;
using System.Reflection;
using Microsoft.Office.Interop.Word;
using Microsoft.Office.Core;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Summary description for WordOp.
	/// </summary>
	public class WordOp
	{
		// MS Word constants

		public const int OrientPortrait = 0;
		public const int OrientLandscape = 1;

		public const int AlignParagraphLeft = 0;
		public const int AlignParagraphCenter = 1;
		public const int AlignParagraphRight = 2;

		public const int CellAlignVerticalCenter = 1;
		public const int CellAlignVerticalTop = 0;
		public const int CellAlignVerticalBottom = 3;

		public const int Character = 1;  // movement units
		public const int Paragraph = 4;
		public const int Line = 5;
		public const int Column = 9;
		public const int Row = 10;
		public const int Cell = 12;

		public const int Move = 0;
		public const int Extend = 1;

		public const int SectionBreakNextPage = 2;
		public const int SectionBreakContinuous = 3;
		public const int SectionBreakEvenPage = 4;
		public const int SectionBreakOddPage = 5;
		public const int LineBreak = 6;
		public const int PageBreak = 7;
		public const int ColumnBreak = 8;

		public const int PasteOLEObject = 0;
		public const int PasteMetafilePicture = 3;

		public const int InLine = 0;
		public const int FloatOverText = 1;

		public const int AdjustNone = 0;

		public const int FormatDocument = 0;
		public const int SaveChanges = -1;


		public WordOp()
		{
			return;
		}

		static Microsoft.Office.Interop.Word._Application WdApp;
		static Microsoft.Office.Interop.Word.Documents WdDocs;
		static Microsoft.Office.Interop.Word.Document WdDoc;
		static Microsoft.Office.Interop.Word.Selection WdSel;
		static Microsoft.Office.Interop.Word.Table WdTable;
		static Microsoft.Office.Interop.Word.Tables WdTables;

		// Shorthand signatures

		public static string Call(
			string command)
		{
			return Call(command, null, -1, -1, -1, -1);
		}

		public static string Call(
			string command,
			string sArg)
		{
			return Call(command, sArg, -1, -1, -1, -1);
		}

		public static string Call(
			string command,
			int nArg1)
		{
			return Call(command, null, nArg1, -1, -1, -1);
		}

		public static string Call(
			string command,
			string sArg,
			int nArg1)
		{
			return Call(command, sArg, nArg1, -1, -1, -1);
		}

		public static string Call(
			string command,
			string sArg,
			int nArg1,
			int nArg2)
		{
			return Call(command, sArg, nArg1, nArg2, -1, -1);
		}

		public static string Call(
			string command,
			string sArg,
			int nArg1,
			int nArg2,
			int nArg3)
		{
			return Call(command, sArg, nArg1, nArg2, nArg3, -1);
		}

		public static string Call(
			string command,
			string sArg,
			int nArg1,
			int nArg2,
			int nArg3,
			int nArg4)
		{
			return Call(command, sArg, nArg1, nArg2, nArg3, nArg4, false);
		}

		public static string Call(
			string command,
			int nArg1,
			int nArg2)
		{
			return Call(command, "", nArg1, nArg2);
		}

		public static string Call(
			string command,
			bool bArg)
		{
			return Call(command, "", -1, -1, -1, -1, bArg);
		}

		/// <summary>
		/// Word operation
		/// </summary>
		/// <param name="command"></param>
		/// <param name="sArg"></param>
		/// <param name="nArg1"></param>
		/// <param name="nArg2"></param>
		/// <param name="nArg3"></param>
		/// <param name="nArg4"></param>
		/// <returns></returns>
		public static string Call(
			string command,
			string sArg,
			int nArg1,
			int nArg2,
			int nArg3,
			int nArg4,
			bool bArg)
		{

			object missingObj = System.Reflection.Missing.Value; // missing object parameter 

			if (SS.I.DebugFlags[5] != null) // dump out command for debug
			{
				ClientLog.Message("Call " + command + " " + sArg);
			}

			try // catch any Word exception
			{

				//******************************************************************************  
				if (Lex.Eq(command, "CreateObject"))  // create the word object
				//******************************************************************************  
				{
					try
					{
						WdApp = new Microsoft.Office.Interop.Word.Application();
					}
					catch (Exception ex)
					{
						return "Word failed to start";
					}

				}

		//******************************************************************************  
				else if (Lex.Eq(command, "Quit"))
				{ // quit application
					//******************************************************************************  

					//Microsoft.Office.Interop.Word.Application.
					WdApp.Quit(ref missingObj, ref missingObj, ref missingObj);

					//	AutoWrap(DISPATCH_METHOD, NULL, WdApp, L"Quit", 0);
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "Cells.SetWidth"))
				{
					//******************************************************************************  
					WdSel.Cells.SetWidth(nArg1, WdRulerStyle.wdAdjustNone);
#if false
	release_obj(WdTemp); // get current font object
	AutoWrap(DISPATCH_PROPERTYGET, &result, WdSel, L"Cells", 0);
	WdTemp = result.pdispVal;

	SETLONG(Arg1,nArg1);  // width in points
	SETLONG(Arg2,wdAdjustNone); // ruler style, required sArg
	AutoWrap(DISPATCH_METHOD, &result, WdTemp, L"Setwidth", 2, Arg2, Arg1);
	VariantClear(&Arg1);
	VariantClear(&Arg2);
#endif
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "DeleteObject"))
				{
					//******************************************************************************  
					WdSel = null;
					WdTable = null;
					WdTables = null;
					WdDoc = null;
					WdDocs = null;
					WdApp = null;
#if false
	// Release references, must do lower to higher level...
	release_obj(WdTemp);
	release_obj(WdSel);
	release_obj(WdTable);
	release_obj(WdTables);
	release_obj(WdDoc);
	release_obj(WdDocs);
	release_obj(WdApp);
#endif
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "Close"))
				{
					//******************************************************************************  
					//      excelobj.ActiveWorkbook.Close False ' no prompt for save
					((_Document)WdDoc).Close(ref missingObj, ref missingObj, ref missingObj);

					//	SETLONG(SaveChanges,wdSaveChanges); // don't prompt
					//	AutoWrap(DISPATCH_METHOD, NULL, WdDoc, L"Close", 1, SaveChanges);
					//	VariantClear(&SaveChanges);
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "Documents.Add"))
				{ // Add a new document
					//******************************************************************************  

					WdDocs = WdApp.Documents;
					WdDoc = WdDocs.Add(ref missingObj, ref missingObj, ref missingObj, ref missingObj);

#if false
	release_obj(WdDocs); // Get active documents
	AutoWrap(DISPATCH_PROPERTYGET, &result, WdApp, L"Documents", 0);   // Get Documents collection
	WdDocs = result.pdispVal;

	release_obj(WdDoc); // Add new document
	AutoWrap(DISPATCH_METHOD, &result, WdDocs, L"Add", 0);
	WdDoc = result.pdispVal;
#endif

					if (WdDoc == null) return ("Error adding document");
					UpdateSelection(); // update selection
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "EndKey"))
				{
					//******************************************************************************  
					Object unit = nArg1;
					WdSel.HomeKey(ref unit, ref missingObj);

					//	SETLONG(Arg,nArg1); 
					//	AutoWrap(DISPATCH_METHOD, &result, WdSel, L"HomeKey", 1, Arg);
					//	VariantClear(&Arg);

					UpdateSelection(); // update selection
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "Font.Name"))
				{
					//******************************************************************************  
					//      WordObj.Selection.Font.Name = sArg
					WdSel.Font.Name = sArg;

#if false
	release_obj(WdTemp); // get current font object
	AutoWrap(DISPATCH_PROPERTYGET, &result, WdSel, L"Font", 0);
	WdTemp = result.pdispVal;

	SETSTR(Arg,sArg);  // set font name
	AutoWrap(DISPATCH_PROPERTYPUT, &result, WdTemp, L"Name", 1, Arg);
	VariantClear(&Arg);
#endif
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "Font.Size"))
				{
					//******************************************************************************  
					//      WordObj.Selection.Font.Size = sArg

					WdSel.Font.Size = nArg1;

#if false
	release_obj(WdTemp); // get current font object
	AutoWrap(DISPATCH_PROPERTYGET, &result, WdSel, L"Font", 0);
	WdTemp = result.pdispVal;

	SETLONG(Arg,nArg1);  // set font size
	AutoWrap(DISPATCH_PROPERTYPUT, &result, WdTemp, L"Size", 1, Arg);
	VariantClear(&Arg);
#endif
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "Font.Bold"))
				//******************************************************************************  
				{
					if (bArg) WdSel.Font.Bold = -1;
					else WdSel.Font.Bold = 0;
				}

	//******************************************************************************  
				else if (Lex.Eq(command, "Font.Italic"))
				//******************************************************************************  
				{
					if (bArg) WdSel.Font.Italic = -1;
					else WdSel.Font.Italic = 0;
				}

		//******************************************************************************  
				else if (Lex.Eq(command, "Font.Subscript"))
				//******************************************************************************  
				{
					WdSel.Font.Subscript = nArg1;

#if false
	release_obj(WdTemp); // get current font object
	AutoWrap(DISPATCH_PROPERTYGET, &result, WdSel, L"Font", 0);
	WdTemp = result.pdispVal;

	SETLONG(Arg,nArg1);  // set font size
	AutoWrap(DISPATCH_PROPERTYPUT, &result, WdTemp, L"Subscript", 1, Arg);
	VariantClear(&Arg);
#endif
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "Font.Superscript"))
				{
					//******************************************************************************  
					WdSel.Font.Superscript = nArg1;

#if false
	release_obj(WdTemp); // get current font object
	AutoWrap(DISPATCH_PROPERTYGET, &result, WdSel, L"Font", 0);
	WdTemp = result.pdispVal;

	SETLONG(Arg,nArg1);  // set font size
	AutoWrap(DISPATCH_PROPERTYPUT, &result, WdTemp, L"Superscript", 1, Arg);
	VariantClear(&Arg);
#endif
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "SetDefaultCellStyle"))
				//******************************************************************************  
				{ // Cell style (e.g. backcolor) automatically carries down do successive
					// rows and must be explicitly reset

					//			int rgbBlack = 0;
					//			if (WdSel.Font.Color != (Microsoft.Office.Interop.Word.WdColor)rgbBlack)
					//				WdSel.Font.Color = (Microsoft.Office.Interop.Word.WdColor)rgbBlack;

					//			int rgbWhite = 255 + 255 * 256 + 255 * 65536;
					//			if (WdSel.Cells.Shading.BackgroundPatternColor != (Microsoft.Office.Interop.Word.WdColor)rgbWhite)
					//				WdSel.Cells.Shading.BackgroundPatternColor = (Microsoft.Office.Interop.Word.WdColor)rgbWhite;

					if (WdSel.Font.Color != Microsoft.Office.Interop.Word.WdColor.wdColorAutomatic)
						WdSel.Font.Color = Microsoft.Office.Interop.Word.WdColor.wdColorAutomatic;

					if (WdSel.Cells.Shading.BackgroundPatternColor != Microsoft.Office.Interop.Word.WdColor.wdColorAutomatic)
						WdSel.Cells.Shading.BackgroundPatternColor = Microsoft.Office.Interop.Word.WdColor.wdColorAutomatic;
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "Font.Color"))
				//******************************************************************************  
				{
					Color c = Color.FromArgb(nArg1);
					int rgb = c.R + c.G * 256 + c.B * 65536;
					WdSel.Font.Color = (Microsoft.Office.Interop.Word.WdColor)rgb;
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "BackColor"))
				//******************************************************************************  
				{
					Color c = Color.FromArgb(nArg1);
					int rgb = c.R + c.G * 256 + c.B * 65536;
					WdSel.Cells.Shading.BackgroundPatternColor = (Microsoft.Office.Interop.Word.WdColor)rgb;
				}

		//******************************************************************************  
				else if (Lex.Eq(command, "HomeKey"))
				{
					//******************************************************************************  
					Object unit = nArg1;
					WdSel.HomeKey(ref unit, ref missingObj);
#if false
	SETLONG(Arg,nArg1); 
	AutoWrap(DISPATCH_METHOD, &result, WdSel, L"HomeKey", 1, Arg);
	VariantClear(&Arg);
#endif
					UpdateSelection(); // update selection
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "InlineShapes.AddPicture"))
				{ // insert image from file
					//******************************************************************************  
					InlineShape ils = WdSel.InlineShapes.AddPicture(sArg, ref missingObj, ref missingObj, ref missingObj);
					ils.Width = nArg1;
					ils.Height = nArg2;
#if false
	release_obj(WdTemp); // get current font object
	AutoWrap(DISPATCH_PROPERTYGET, &result, WdSel, L"InlineShapes", 0);
	WdTemp = result.pdispVal;

	SETSTR(Filename,sArg);  // filename
	SETLONG(Width,nArg1); // in points
	SETLONG(Height,nArg2);

	AutoWrap(DISPATCH_METHOD, &result, WdTemp, L"AddPicture", 1, Filename);
	release_obj(WdTemp);
	WdTemp = result.pdispVal; // new shape object

	AutoWrap(DISPATCH_PROPERTYPUT, NULL, WdTemp, L"Width", 1, Width);

	AutoWrap(DISPATCH_PROPERTYPUT, NULL, WdTemp, L"Height", 1, Height);

	VariantClear(&Filename);
	VariantClear(&Width);
	VariantClear(&Height);
#endif
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "InsertBreak"))
				{
					//******************************************************************************  
					object type = nArg1;
					WdSel.InsertBreak(ref type);

					//	SETLONG(Arg,nArg1); 
					//	AutoWrap(DISPATCH_METHOD, &result, WdSel, L"InsertBreak", 1, Arg);
					//	VariantClear(&Arg);

					UpdateSelection(); // update selection
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "InsertSymbol"))
				{
					//******************************************************************************  
					// InsertSymbol(CharacterNumber as Long, Font as String)
					int characterNumber = nArg1;
					object font = sArg;
					WdSel.InsertSymbol(characterNumber, ref font, ref missingObj, ref missingObj);
#if false
	SETLONG(Arg,nArg1); // get char number
	SETSTR(Arg2,sArg); // get font
	AutoWrap(DISPATCH_METHOD, &result, WdSel, L"InsertSymbol", 2, Arg2, Arg);
	VariantClear(&Arg);
	VariantClear(&Arg2);
#endif
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "Cells.Merge"))
				{ // merge cells together
					//******************************************************************************  
					WdSel.Cells.Merge();
#if false
	release_obj(WdTemp); // get current font object
	AutoWrap(DISPATCH_PROPERTYGET, &result, WdSel, L"Cells", 0);
	WdTemp = result.pdispVal;

	AutoWrap(DISPATCH_METHOD, &result, WdTemp, L"Merge", 0);
#endif
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "MoveLeft"))
				{ // units, count
					//******************************************************************************  
					if (nArg2 <= 0) nArg2 = 1;

					object unit = nArg1;
					object count = nArg2;
					WdSel.MoveLeft(ref unit, ref count, ref missingObj);
#if false
	SETLONG(Arg1,nArg1); 
	SETLONG(Arg2,nArg2); 
	AutoWrap(DISPATCH_METHOD, &result, WdSel, L"MoveLeft", 2, Arg2, Arg1);
	VariantClear(&Arg1);
	VariantClear(&Arg2);
#endif
					UpdateSelection(); // update selection
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "MoveRight"))
				{ // units, count, extend
					//******************************************************************************  
					object extend;
					if (nArg2 <= 0) nArg2 = 1;

					object unit = nArg1;
					object count = nArg2;
					if (nArg3 <= 0) extend = missingObj;
					else extend = nArg3;

					WdSel.MoveRight(ref unit, ref count, ref extend);
#if false
	SETLONG(Arg1,nArg1); 
	SETLONG(Arg2,nArg2); 
	SETLONG(Arg3,nArg3); 
	AutoWrap(DISPATCH_METHOD, &result, WdSel, L"MoveRight", 3, Arg3, Arg2, Arg1);
	VariantClear(&Arg1);
	VariantClear(&Arg2);
	VariantClear(&Arg3);
#endif

					UpdateSelection(); // update selection
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "MoveDown"))
				{ // units, count
					//******************************************************************************  
					if (nArg2 <= 0) nArg2 = 1;
					object unit = nArg1;
					object count = nArg2;
					WdSel.MoveDown(ref unit, ref count, ref missingObj);
#if false
	SETLONG(Arg1,nArg1); 
	SETLONG(Arg2,nArg2); 
	AutoWrap(DISPATCH_METHOD, &result, WdSel, L"MoveDown", 2, Arg2, Arg1);
	VariantClear(&Arg1);
	VariantClear(&Arg2);
#endif
					UpdateSelection(); // update selection
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "MoveUp"))
				{ // units, count
					//******************************************************************************  
					if (nArg2 <= 0) nArg2 = 1;
					object unit = nArg1;
					object count = nArg2;
					WdSel.MoveUp(ref unit, ref count, ref missingObj);
#if false
	SETLONG(Arg1,nArg1); 
	SETLONG(Arg2,nArg2); 
	AutoWrap(DISPATCH_METHOD, &result, WdSel, L"MoveUp", 2, Arg2, Arg1);
	VariantClear(&Arg1);
	VariantClear(&Arg2);
#endif

					UpdateSelection(); // update selection
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "PageSetup.Orientation"))
				{
					//******************************************************************************  

					if (nArg1 == 0)
						WdSel.PageSetup.Orientation = WdOrientation.wdOrientPortrait;
					WdSel.PageSetup.Orientation = WdOrientation.wdOrientLandscape;
#if false
	release_obj(WdTemp);
	AutoWrap(DISPATCH_PROPERTYGET, &result, WdSel, L"PageSetup", 0);
	WdTemp = result.pdispVal;

	SETLONG(Arg,nArg1); 
	AutoWrap(DISPATCH_PROPERTYPUT, &result, WdTemp, L"Orientation", 1, Arg);
	VariantClear(&Arg);
#endif
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "PageSetup.Margins"))
				{
					//******************************************************************************  
					WdSel.PageSetup.TopMargin = nArg1;
					WdSel.PageSetup.BottomMargin = nArg2;
					WdSel.PageSetup.LeftMargin = nArg3;
					WdSel.PageSetup.RightMargin = nArg4;
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "PageSetup.PageSize"))
				{
					//******************************************************************************  
					WdSel.PageSetup.PageWidth = nArg1;
					WdSel.PageSetup.PageHeight = nArg2;
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "ParagraphFormat.Alignment"))
				{
					//******************************************************************************  
					WdSel.ParagraphFormat.Alignment = (WdParagraphAlignment)nArg1;
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "Cells.VerticalAlignment"))
				//******************************************************************************  
				{
					WdSel.Cells.VerticalAlignment = (WdCellVerticalAlignment)nArg1;
				}

		//******************************************************************************  
				else if (Lex.Eq(command, "Paste"))
				//******************************************************************************  
				{
					WdSel.Paste();
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "PasteSpecial"))
				{
					//******************************************************************************  
					//	PasteSpecial(IconIndex, Link, Placement, DisplayAsIcon, DataType)

					object iconIndex = 0;
					object link = false;
					object placement = InLine;
					object displayAsIcon = false;
					object dataType = nArg1; // set type of data to paste

					//	ClientLog.Message("Before PasteSpecial"); // TST & PRD 
					WdSel.PasteSpecial(ref iconIndex, ref link, ref placement, ref displayAsIcon,
						ref dataType, ref missingObj, ref missingObj);
					//	ClientLog.Message("After PasteSpecial");
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "InsertStructure"))
				//******************************************************************************  
				// Selection.InlineShapes.AddOLEObject ClassType:="ISISServer", FileName:= _
				//  "C:\Isis\sketch1-small.skc", LinkToFile:=False, DisplayAsIcon:=False
				// This is significantly slower than a paste
				{
					object classType = "ISISServer";
					object fileName = sArg;

					WdSel.InlineShapes.AddOLEObject(
						ref classType,
						ref fileName,
						ref missingObj,
						ref missingObj,
						ref missingObj,
						ref missingObj,
						ref missingObj,
						ref missingObj);
				}

			//******************************************************************************  
				else if (Lex.Eq(command, "Rows.AllowBreakAcrossPages"))
				{ // keep all contents of row on same page
					//******************************************************************************  

					WdSel.Rows.AllowBreakAcrossPages = nArg1;
				}

		//******************************************************************************  
				else if (Lex.Eq(command, "Rows.HeadingFormat"))
				{ // mark rows as headings
					//******************************************************************************  

					WdSel.Rows.HeadingFormat = nArg1;
				}

		//******************************************************************************  
				else if (Lex.Eq(command, "SaveAs"))
				{ // save file in .doc format
					//******************************************************************************  
					try { File.Delete(sArg); } // delete any existing file
					catch (Exception ex) { };
					object fileName = sArg;
					object fileFormat = WdSaveFormat.wdFormatDocument;

					WdDoc.SaveAs(
						ref fileName, // FileName 
						ref fileFormat, // FileFormat 
						ref missingObj, // LockComments 
						ref missingObj, // Password 
						ref missingObj, // AddToRecentFiles 
						ref missingObj, // WritePassword 
						ref missingObj, // ReadOnlyRecommended 
						ref missingObj, // EmbedTrueTypeFonts 
						ref missingObj, // SaveNativePictureFormat
						ref missingObj,	// SaveFormsData 
						ref missingObj,	// SaveAsAOCELetter
						ref missingObj,	// Encoding
						ref missingObj,	// InsertLineBreaks 
						ref missingObj, // AllowSubstitutions 
						ref missingObj, // LineEnding 
						ref missingObj); // AddBiDiMarks
				}

		//******************************************************************************  
				else if (Lex.Eq(command, "ScreenUpdating"))
				{
					//******************************************************************************  
					WdApp.ScreenUpdating = bArg;
				}

		//******************************************************************************  
				else if (Lex.Eq(command, "SelectColumn"))
				{ // select current column of table
					//******************************************************************************  
					WdSel.SelectColumn();
					//	AutoWrap(DISPATCH_METHOD, NULL, WdSel, L"SelectColumn", 0); 
					UpdateSelection();
				}

				//******************************************************************************  
				else if (Lex.Eq(command, "SelectRow"))
				{ // select current row of table
					//******************************************************************************  
					WdSel.SelectRow();

					UpdateSelection();
				}

				//******************************************************************************  
				else if (Lex.Eq(command, "Tables.Add"))
				{ // number of rows & cols supplied
					//******************************************************************************  

					WdTables = WdDoc.Tables;
					Microsoft.Office.Interop.Word.Range range = WdSel.Range;
					int numRows = nArg1;
					int numCols = nArg2;
					WdTable = WdTables.Add(range, numRows, numCols, ref missingObj, ref missingObj);
					WdTable.Borders.InsideLineStyle = WdLineStyle.wdLineStyleSingle;
					WdTable.Borders.OutsideLineStyle = WdLineStyle.wdLineStyleSingle;

					UpdateSelection();
				}

				//******************************************************************************  
				else if (Lex.Eq(command, "TableSelect"))
				{
					//******************************************************************************  

					WdTable.Select();

					UpdateSelection(); // update selection
				}

				//******************************************************************************  
				else if (Lex.Eq(command, "TypeParagraph"))
				{
					//******************************************************************************  
					WdSel.TypeParagraph();

					UpdateSelection();
				}

				//******************************************************************************  
				else if (Lex.Eq(command, "TypeText"))
				{
					//******************************************************************************  
					WdSel.TypeText(sArg);
					//	SETSTR(Arg,sArg); 
					//	AutoWrap(DISPATCH_METHOD, &result, WdSel, L"TypeText", 1, Arg);
					//	VariantClear(&Arg);

					UpdateSelection();
				}

				//******************************************************************************  
				else if (Lex.Eq(command, "Visible"))
				{
					//******************************************************************************  
					WdApp.Visible = bArg;
#if false
	SETBOOL(True,1);
	SETBOOL(False,0);
	if (strcmpi((CCP)sArg,"true)) 
	AutoWrap(DISPATCH_PROPERTYPUT, NULL, WdApp, L"Visible", 1, True);

	else 	AutoWrap(DISPATCH_PROPERTYPUT, NULL, WdApp, L"Visible", 1, False);
#endif
				}

		//******************************************************************************  
				else throw new Exception("WordOp - Invalid operation " + command);
				//******************************************************************************  

				return ""; // everything must be ok

			} // end of try

			catch (Exception ex)
			{
				ClientLog.Message("Exception on command: " + command + "\n" + ex.Message);
				return ex.Message;
			}

		}
		//******************************************************************************  
		static void UpdateSelection() // get current selection
		//******************************************************************************  
		{
			WdSel = WdApp.Selection;
#if false
VARIANT result;

release_obj(WdSel); // get current selection
AutoWrap(DISPATCH_PROPERTYGET, &result, WdApp, L"Selection",0); 
WdSel = result.pdispVal;
#endif
		}

		/// <summary>
		/// TestWordGetObject
		/// </summary>

		public static void TestWordGetObject()
		{
			Microsoft.Office.Interop.Word._Application app = null;

			//			Word.Application app = null;

			try
			{
				try // see if there is an existing instance we can connect to
				{
					//					object o = Microsoft.VisualBasic.Interaction.GetObject(null,"Word.Application"); 
					object o = System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application");
					app = (Microsoft.Office.Interop.Word.Application)o;
				}
				catch (Exception ex) { app = null; } // fall through & start new instance

				if (app == null) // try to start new instance
					app = new Microsoft.Office.Interop.Word.Application();

				bool b = app.Visible;
			}
			catch (Exception e)
			{
				app = null;
			}

		}

	}
}
