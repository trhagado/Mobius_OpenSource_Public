using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;
using Mobius.Helm;

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
using System.Diagnostics;
using System.Net;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Format structure fields
	/// </summary>

	public partial class ResultsFormatter
	{

		public static HelmOffScreenBrowser HelmRenderer = null;

		/// <summary>
		/// Scale and translate structure and format into buffer
		/// </summary>
		/// <param name="mol">The structure</param>
		/// <param name="cellStyle">Style/conditional formatting to apply to cell</param>
		/// <param name="commandChar">Command character to use in buffer</param>
		/// <param name="x">Coordinate of left side of structure</param>
		/// <param name="width">Width of molecule box in milliinches</param>
		/// <param name="r">Row in buffer to put on. If less than 0 then return formatted data</param>
		/// <param name="heightInLines">number of lines used</param>

		public FormattedFieldInfo FormatStructure(
		MoleculeMx mol,
		CellStyleMx cellStyle,
		char commandChar,
		int x,
		int width,
		int r,
		ResultsField rfld = null,
		DataRowMx dataRow = null)
		{
			Rectangle destRect, boundingRect;
			int height; // formatted  height in milliinches
			bool markBoundaries;
			int fixedHeight;
			Bitmap bm;
			Font font;
			string txt, molfile, molString = "", svg, cid = null;
			int translateType, desiredBondLength = 100;
			int pixWidth = 0, pixHeight = 0;

			bool debug = DataTableManager.DebugDetails;

			if (debug) DebugLog.Message("=============================== FormattingStructure ===============================");
			PerformanceTimer pt = PT.Start("FormatStructure");
			Stopwatch swTotal = Stopwatch.StartNew();
			Stopwatch sw = Stopwatch.StartNew();

			try
			{

				MoleculeFormat initialCsType = mol.PrimaryFormat;
				string initialCsValue = mol.PrimaryValue;

				QueryColumn qc = (rfld != null) ? rfld.QueryColumn : null;

				FormattedFieldInfo ffi = new FormattedFieldInfo();

				if (dataRow != null) // get any cid in row
				{
					int ki = DataTableManager.DefaultKeyValueVoPos;
					if (ki < dataRow.Length)
						cid = dataRow[ki] as string;
				}

				//DebugLog.Message("FormatStructure " + cid);

				//if (!Rf.Grid) x = x; // debug

				///////////////////////////////////
				// Highlight structure
				///////////////////////////////////

				if (StructureHighlightPssc != null)
				{
					ParsedStructureCriteria pssc = StructureHighlightPssc;

					// Hilight substructure search match

					if (pssc.SearchType == StructureSearchType.Substructure || // regular SSS
						pssc.SearchTypeUnion == StructureSearchType.Substructure) // handles related search for just SSS to get hilighting
					{
						if (HighlightStructureMatches)
						{
							try
							{
								mol = StrMatcher.HighlightMatchingSubstructure(mol);
								if (DebugMx.False) // debug
								{
									//string highlightChildren = mol.MolLib.HighlightChildren;
									//Color highlightColor = mol.MolLib.HighlightColor;
									if (debug) DebugLog.StopwatchMessage("tHilight", sw);
								}
							}
							catch (Exception ex) { ex = ex; }
						}

						if (AlignStructureToQuery)
						{
							try
							{
								mol = StrMatcher.AlignToMatchingSubstructure(mol);
								if (debug) DebugLog.StopwatchMessage("tOrient", sw);
							}
							catch (Exception ex) { ex = ex; }
						}
					}

					// Hilight SmallWorld structure match

					else if (pssc.SearchType == StructureSearchType.SmallWorld) // Hilight SmallWorld structure search results
					{
						if (SmallWorldDepictions == null)
							SmallWorldDepictions = new SmallWorldDepictions();

						SmallWorldPredefinedParameters swp = pssc.SmallWorldParameters;

						//DebugLog.Message("Depict " + cid + ", Hilight " + swp.Highlight + ", Align " + swp.Align); // + "\r\n" + new StackTrace(true));

						if ((swp.Highlight || swp.Align) & Lex.IsDefined(cid)) // call depiction for these
						{
							svg = SmallWorldDepictions.GetDepiction(cid, swp.Highlight, swp.Align);

							if (Lex.IsDefined(svg)) // have depiction?
							{
								{
									pixWidth = MoleculeMx.MilliinchesToPixels(width);
									bm = SvgUtil.GetBitmapFromSvgXml(svg, pixWidth);
									ffi.FormattedBitmap = mol.FormattedBitmap = bm; // store in formatting info and chem structure
									return ffi;
								}
							}

							else if (svg == null) // start retrieval of this decpiction type & fall through to get default structure initially
							{
								SmallWorldDepictions.StartDepictionRetrieval(Qm, StructureHighlightQc, swp.Highlight, swp.Align);
							}

							else { } // tried to get it but failed, fall through to get basic structure
						}
					}

					else if (mol.AltFormDefined("Svg")) // svg form exist (e.g. SmallWorld or "related" structure search)?
					{
						svg = mol.SvgString;
						if (Lex.IsDefined(svg))
						{
							pixWidth = MoleculeMx.MilliinchesToPixels(width);
							bm = SvgUtil.GetBitmapFromSvgXml(svg, pixWidth);
							ffi.FormattedBitmap = mol.FormattedBitmap = bm; // store in formatting info and chem structure
							return ffi;
						}
					}
				}

				///////////////////////////////////
				// Handle each output device
				///////////////////////////////////

				ffi.HeightInLines = 1; // min of 1 line

				if (Rf.SdFile)
				{
					FormatSdfileStructure(mol);
					return null;
				}

				int pageHeight = 11000;
				if (Rf.PageMargins != null) pageHeight = Rf.PageMargins.Top + Rf.PageHeight + Rf.PageMargins.Bottom;

				if (Rf.Excel || Rf.Word) translateType = 2;
				else translateType = 0;

				if (!Rf.FixedHeightStructures) fixedHeight = 0; // not fixed height
				else if (Rf.Excel || Rf.Word) fixedHeight = 1; // always fixed height
				else fixedHeight = 2; // fixed height unless need to expand

				if (Rf.Word && Rf.FixedHeightStructures)
					markBoundaries = true;
				else markBoundaries = false;

				destRect = new Rectangle(0, 0, width, width * 4 / 5); // default dest rect

				///////////////////////////////////////////////////////////////////////////
				// Tempory fake generation of HELM & associatedimage for biopolymer testing
				///////////////////////////////////////////////////////////////////////////

				//if (MoleculeMx.HelmEnabled == DebugMx.False) // artificially generate helm molecules
				//	MoleculeMx.SetMoleculeToTestHelmString(mol.GetCorpId().ToString(), mol);

				///////////////////////////////////////////////////////////////////////////
				// End of tempory fake generation of HELM & associatedimage for biopolymer testing
				///////////////////////////////////////////////////////////////////////////

				bool fitStructure = true;

				if (mol.IsChemStructureFormat)
				{
					if (Rf.Grid) // special scale for grid
					{
						double scale = (float)width / MoleculeMx.StandardBoxWidth;
						desiredBondLength = mol.CdkMol.AdjustBondLengthToValidRange((int)(MoleculeMx.StandardBondLength * scale));
						//desiredBondLength = (int)(ChemicalStructure.StandardBondLength * (Rf.PageScale / 100.0));
						desiredBondLength = (int)(desiredBondLength * 90.0 / 100.0); // scale down a bit for grid
						if (debug) DebugLog.StopwatchMessage("tAdjustBondLength1", sw);
					}

					else // set desired bond length based on page scaling
					{
						float scale = (float)width / MoleculeMx.StandardBoxWidth;
						desiredBondLength = mol.CdkMol.AdjustBondLengthToValidRange((int)(MoleculeMx.StandardBondLength * scale));
						//desiredBondLength = (int)(ChemicalStructure.StandardBondLength * (Rf.PageScale / 100.0));
						if (debug) DebugLog.StopwatchMessage("tAdjustBondLength2", sw);
					}

					if (desiredBondLength < 1) desiredBondLength = 1;
					if (debug) DebugLog.StopwatchMessage("tBeforeFit", sw);

					if (fitStructure)
					{
						mol.CdkMol.FitStructureIntoRectangle // scale and translate structure into supplied rectangle.
						(ref destRect, desiredBondLength, translateType, fixedHeight, markBoundaries, pageHeight, out boundingRect);
					}

					if (debug) DebugLog.StopwatchMessage("tFitStructure", sw);

					ffi.HeightInLines = (int)(destRect.Height / Rf.LineHeight + 1); // lines needed
				}

				else if (mol.IsBiopolymerFormat)
				{
					if (mol.PrimaryFormat == MoleculeFormat.Helm && Rf.Excel)
					{
						svg = HelmControl.GetSvg(mol.HelmString);
						float inchWidth = width / 1000.0f; // convert width milliinches to inches
						Svg.SvgDocument svgDoc = SvgUtil.AdjustSvgDocumentToFitContent(svg, inchWidth, Svg.SvgUnitType.Inch);
						RectangleF svgbb = svgDoc.Bounds;
						float ar = svgbb.Width / svgbb.Height; // aspect ratio of svg bounding box

						height = (int)(width / ar); // height in milliinches
						ffi.HeightInLines = (int)(height / Rf.LineHeight) + 1; // lines needed

						destRect = new Rectangle(0, 0, width, height);
					}

				}

				//////////////////////////
				/// Output to Grid
				//////////////////////////

				if (Rf.Grid)
				{
					pixWidth = MoleculeMx.MilliinchesToPixels(destRect.Width);
					pixHeight = MoleculeMx.MilliinchesToPixels(destRect.Height);

					if (cellStyle == null)
					{
						if (Qm == null || Qm.MoleculeGrid == null)
							font = new Font("Tahoma", 8.25f);
						else font = new Font(Qm.MoleculeGrid.Font, FontStyle.Underline);
						cellStyle = new CellStyleMx(font, Color.Blue, Color.Empty);
					}

					if (mol.IsChemStructureFormat) // molfile type molecule
					{
						if (debug) DebugLog.StopwatchMessage("tBeforeGetDisplayPreferences", sw);
						DisplayPreferences dp = mol.GetDisplayPreferences();
						if (debug) DebugLog.StopwatchMessage("tGetDisplayPreferences", sw);

						desiredBondLength = mol.CdkMol.AdjustBondLengthToValidRange(desiredBondLength); // be sure bond len within allowed range 
						if (debug) DebugLog.StopwatchMessage("tAdjustBondLengthToValidRange", sw);
						dp.StandardBondLength = MoleculeMx.MilliinchesToDecipoints(desiredBondLength);
						bm = mol.CdkMol.GetFixedHeightMoleculeBitmap(pixWidth, pixHeight, dp, cellStyle, mol.Caption);
						if (debug) DebugLog.StopwatchMessage("tGetBitmap", sw);
					}

					else if (mol.IsBiopolymerFormat) // Output HELM image for biopolymer
					{
						pixWidth = MoleculeMx.MilliinchesToPixels(width);
						bm = HelmConverter.HelmToBitmap(mol, pixWidth);
					}

					else bm = new Bitmap(1, 1);

					ffi.FormattedBitmap = mol.FormattedBitmap = bm; // store in formatting & structure
					ffi.FormattedText = "Formatted"; // indicate formatted (could save structure string but not needed and avoids possible conversion overhead)
					return ffi;
				}

				//////////////////////////
				/// Output to Html
				//////////////////////////

				else if (Rf.Html)
				{
					if (r >= 0) AssureTbFree(0, r + ffi.HeightInLines - 1);

					if (mol.IsChemStructureFormat)
						FormatChemStructureHtml(mol, destRect, width, r);

					else if (mol.IsBiopolymerFormat)
						FormatBiopolymerStructureHtml(mol, destRect, width, r);

					if (debug) DebugLog.StopwatchMessage("tFormatHtmlStructure", sw);

					ffi.FormattedBitmap = mol.FormattedBitmap;
					ffi.FormattedText = mol.FormattedText;
					return ffi;
				}

				/////////////////////////////////////////////////////////////////
				/// Other format, store Smiles or Helm & any cellStyle in buffer
				/////////////////////////////////////////////////////////////////

				else
				{
					if (mol.IsChemStructureFormat)
					{
						if (Rf.ExportStructureFormat == ExportStructureFormat.Smiles)
							molString = mol.GetSmilesString();
						else molString = mol.GetChimeString(); // use Chime if not smiles
					}

					else if (mol.IsBiopolymerFormat)
					{
						molString = mol.PrimaryValue; // usually Helm but could be sequence
					}

					txt = String.Format("{0} {1} {2} {3} {4} {5} {6}",
						x, width, destRect.Left, destRect.Top, destRect.Right, destRect.Bottom, molString);

					if (cellStyle != null) // apply style to cell?
						txt += " <CellStyle " + cellStyle.Serialize() + ">";

					txt = commandChar + " " + txt + "\t";

					if (r >= 0)
					{
						AssureTbFree(0, r + ffi.HeightInLines - 1);
						Tb.Lines[r] += txt; // put in buffer
					}

					else return new FormattedFieldInfo(txt); // just return formatting
				}

				return null;
			}

			catch (Exception ex)
			{
				DebugLog.Message(DebugLog.FormatExceptionMessage(ex));
				return null;
			}

			finally
			{
				pt.Update();
				int formatCount = pt.Count;
				//ClientLog.Message(pt.ToString() + ", " + cs.GetMolHeader()[2]); // + ", " + new StackTrace(true));
				if (debug) DebugLog.StopwatchMessage("tTotalTime", swTotal);
			}
		}

		/// <summary>
		/// Format a chemical structure for Html output
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="destRect"></param>
		/// <param name="width"></param>
		/// <param name="r"></param>
		/// <returns></returns>

		string FormatChemStructureHtml(
			MoleculeMx mol,
			Rectangle destRect,
			int width,
			int r)
		{
			string source, molfil, html, shtml, fname, txt, txt2, str, hdisp, rntxt, html_end;
			int width2, pixWidth, pixHeight, height, rc, i1;
			bool useChimeString = false; // must use molfile format for .Net 4.0 with Draw 4.0

			MolCount++;
			string objId = "Mol" + MolCount;

			//double scaleUp = (SS.I.GraphicsColumnZoom / 100.0) * 1.2; // do graphics scaleup plus a bit
			double scaleUp = 1.2; // extra scaleup
			pixWidth = (int)(width / 1000.0 * GraphicsMx.LogicalPixelsX * scaleUp);
			height = (int)(width * .67);
			if (destRect.Height > height) height = destRect.Height;
			pixHeight = (int)(height / 1000.0 * GraphicsMx.LogicalPixelsY * scaleUp);

			shtml = // html for structure
			 "<object id=\"" + objId + "\" " +
			 "codebase=\"<editor>.dll#-1,-1,-1,-1\" " +
			 "height=\"<pixHeight>\" width=\"<pixWidth>\" " +
			 "classid=\"CLSID:46803AAE-C327-4002-8CEC-05036E2FDEF4\"> " + // 
			 "<param name=\"<molStringFormat>\" value=\"<molString>\"> " +
			 "</object>";

			shtml = shtml.Replace("<pixWidth>", pixWidth.ToString());
			shtml = shtml.Replace("<pixHeight>", pixHeight.ToString());
			if (useChimeString)
			{
				shtml = shtml.Replace("<molStringFormat>", "ChimeString");
				shtml = shtml.Replace("<molString>", mol.GetChimeString());
			}

			else
			{
				shtml = shtml.Replace("<molStringFormat>", "MolfileString");
				string mfs = mol.GetMolfileString();
				if (mfs.Contains("\t")) // replace any tabs by a space
					mfs = mfs.Replace("\t", " ");
				shtml = shtml.Replace("<molString>", mfs);
			}

			string script = // set mol parameters
				"<script language=\"JavaScript\"> " +
				objId + ".Preferences.WedgeWidth = .3; " + // in .33 inch units
				objId + ".Preferences.StandardBondLength = 144; " +
				objId + ".Preferences.ColorAtomsByType = false; " +
				objId + ".Preferences.HydrogenDisplayMode = 1; " + // 1= Hetero
				objId + ".Preferences.ChemLabelFontString = \"Arial, 16\"; "; // make font larger


			// New stero parameters

			script +=
				objId + ".Preferences.StereoAbsColorString = \"Black\"; " +
				objId + ".Preferences.StereoAndColorString = \"Black\"; " +
				objId + ".Preferences.StereoOrColorString = \"Black\"; " +

				objId + ".Preferences.DisplayChiralStereoLabels = 2; " +  // (On = 2, default is IUPAC = 0)

				objId + ".Preferences.DisplayRS = true; " +  // (default = false)

				objId + ".Preferences.RLabelAtAbsCenter = \"R\"; " + // (default = "R")
				objId + ".Preferences.RLabelAtAndCenter = \"\"; " + // (default = "R*")
				objId + ".Preferences.RLabelAtOrCenter = \"\"; " + // (default = "(R)")

				objId + ".Preferences.SLabelAtAbsCenter = \"S\"; " + // (default = "S")
				objId + ".Preferences.SLabelAtAndCenter = \"\"; " + // (default = "S*")
				objId + ".Preferences.SLabelAtOrCenter = \"\"; " +  // (default = "(S)")

				objId + ".Preferences.AbsStereoLabelText = \"\"; " + // (default = "")
				objId + ".Preferences.AndStereoLabelText = \"\"; " + // (default = "AND Enantiomer")
				objId + ".Preferences.OrStereoLabelText = \"\"; " + // (default = "OR Enantiomer")
				objId + ".Preferences.MixedStereoLabelText = \"\"; "; // (default = "Mixed")

			script += "</script> ";

			shtml += " " + script;

			html = "E <td>";
			html_end = ""; // clear html to add to end
			string label = mol.Caption;
			if (label != null && label != "") // include compound number
			{
				txt = label;
				if (txt.IndexOf("</") >= 0) // label contains already formatted html
				{
					txt2 = txt.Replace("</a>", "");
					if (txt2 != txt) // move anchor close after structure to allow structure click
					{
						txt = txt2;
						html_end = "</a>";
					}
					html += txt;
				}

				else // label contains a registry number possibly followed by other text, make regno an anchor 
				{
					txt.Replace("\n", "<br>"); // change any newlines to breaks
					for (i1 = 0; i1 < txt.Length; i1++) // look for end of regno
					{
						if (!Char.IsLetterOrDigit(txt[i1]) && txt[i1] != '-' && txt[i1] != ' ') break;
					}
					if (i1 < txt.Length)
					{
						rntxt = txt.Substring(0, i1);
						txt = txt.Substring(i1 + 1);
					}
					else
					{
						rntxt = txt;
						txt = "";
					}

					string linkTag = "<a href=\"http://Mobius/command?ShowContextMenu:CompoundIdContextMenu:" + rntxt + "\">";
					txt = // create a link to popup the regno operation menu
						linkTag + rntxt + "</a>" + txt;

					shtml = linkTag + shtml + "</a>"; // also include popup on structure

					html += txt + "<br>";
				}
			}

			html += "<center>" + shtml + "</center>" + html_end + "</td>\t";
			Tb.Lines[r] += html; /* finish up & put in buffer */
			return html;
		}

		/// <summary>
		/// Format Helm-based structure for Html output
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="destRect"></param>
		/// <param name="width"></param>
		/// <param name="r"></param>
		/// <returns></returns>

		string FormatBiopolymerStructureHtml(
			MoleculeMx mol,
			Rectangle destRect,
			int width,
			int r)
		{
			string source, molfil, html, shtml, fname, txt, txt2, str, hdisp, rntxt, html_end;
			int width2, pixWidth, pixHeight, height, rc, i1;
			bool useChimeString = false; // must use molfile format for .Net 4.0 with Draw 4.0

			MolCount++;
			string objId = "Mol" + MolCount;

			//double scaleUp = (SS.I.GraphicsColumnZoom / 100.0) * 1.2; // do graphics scaleup plus a bit
			double scaleUp = 1.2; // extra scaleup
			pixWidth = (int)(width / 1000.0 * GraphicsMx.LogicalPixelsX * scaleUp);
			height = (int)(width * .67);
			if (destRect.Height > height) height = destRect.Height;
			pixHeight = (int)(height / 1000.0 * GraphicsMx.LogicalPixelsY * scaleUp);

			string helm = mol.HelmString;
			string svg = "";

			if (Lex.IsDefined(helm))
				svg = HelmControl.GetSvg(mol.HelmString);

			html = "E <td>" + svg + "</td>\t";

			Tb.Lines[r] += html; /* finish up & put in buffer */
			return html;
		}

		/// <summary>
		/// Format structure for output to SDFile
		/// </summary>
		/// <param name="molid"></param>

		void FormatSdfileStructure(
			MoleculeMx cs)
		{
			string molFile, cid;

			if (SS.I.RemoveLeadingZerosFromCids) // normal formatting
				cid = CompoundId.Format(CurrentKey);
			else cid = CurrentKey; // just use internal formatting

			cs = cs.Convert(Rf.StructureFlags | MoleculeTransformationFlags.RemoveStructureCaption, cid);
			molFile = cs.GetMolfileString();

			if (molFile.IndexOf("\r") < 0) // add carriage returns to molfile if it doesn't contain them
				molFile = molFile.Replace("\n", "\r\n");

			SdfLine = molFile;

			return;
		}

		/// <summary>
		/// Initialize for any structure hilighting
		/// </summary>

		void InitializeStructureHilighting()
		{
			QueryColumn qc = null;

			if (StructureHighlightingInitialized) return;

			// Structure match hilighting / orienting

			try
			{
				do
				{
					qc = Query.GetFirstStructureCriteriaColumn();
					if (qc == null) break;
					if (Lex.IsUndefined(qc.Criteria)) break;

					ParsedSingleCriteria psc = MqlUtil.ParseQueryColumnCriteria(qc);
					if (psc == null || Lex.IsUndefined(psc.Value)) break;

					ParsedStructureCriteria pssc = ParsedStructureCriteria.ConvertFromPscToPssc(psc);

					StructureDisplayFormat sdf = StructureDisplayFormat.Deserialize(qc.DisplayFormatString); // get any hilighting info from format

					if (pssc.SearchType == StructureSearchType.Substructure)
					{
						string chime = MoleculeMx.MolfileStringToChimeString(qc.MolString);
						if (!String.IsNullOrEmpty(chime))
						{
							AlignStructureToQuery = Lex.Contains(psc.Value2, "Orient") || Lex.Contains(psc.Value2, "Align=True");
							HighlightStructureMatches = !(Lex.Contains(psc.Value2, "NoHighlight") || Lex.Contains(psc.Value2, "Highlight=false"));
							MoleculeMx cs = new MoleculeMx(MoleculeFormat.Chime, chime);
							StrMatcher = new StructureMatcher();
							StrMatcher.SetSSSQueryMolecule(cs); // set query used for highlighting
						}
					}

					else if (pssc.SearchType == StructureSearchType.SmallWorld && pssc.SmallWorldParameters != null)
					{
						SmallWorldPredefinedParameters swp = pssc.SmallWorldParameters;
						HighlightStructureMatches = swp.Highlight;
						AlignStructureToQuery = swp.Align;
						//SmallWorldDepictions = null; // depiction cache
					}

					else if (pssc.SearchType == StructureSearchType.Related) // 
					{
						string chime = MoleculeMx.MolfileStringToChimeString(qc.MolString);
						if (!String.IsNullOrEmpty(chime))
						{
							AlignStructureToQuery = Lex.Contains(psc.Value2, "Orient") || Lex.Contains(psc.Value2, "Align=True");
							HighlightStructureMatches = !(Lex.Contains(psc.Value2, "NoHighlight") || Lex.Contains(psc.Value2, "Highlight=false"));
							MoleculeMx cs = new MoleculeMx(MoleculeFormat.Chime, chime);
							StrMatcher = new StructureMatcher();
							StrMatcher.SetSSSQueryMolecule(cs); // set query used for SSS highlighting
						}
					}

					else if (sdf.Highlight || sdf.Align) // other cases 
					{
						HighlightStructureMatches = sdf.Highlight;
						AlignStructureToQuery = sdf.Align;
					}

					else break; // no hilighting / orienting

					StructureHighlightQc = qc;
					StructureHighlightPssc = pssc;

				} while (false);
			}

			catch (Exception ex) // log & ignore any errors
			{
				string msg = DebugLog.FormatExceptionMessage(ex);
				if (qc != null) msg += "\r\n" +
					 "Criteria: " + qc.Criteria + "\r\n" +
					 "Molstring: " + qc.MolString;

				DebugLog.Message(msg);
				return;
			}

			// Atom number display (not currently used)

			SS.I.DisplayAtomNumbers = (int)AtomNumberDisplayMode.None; // see if need to display atom numbers
			for (int ti = 1; ti < Rf.Tables.Count; ti++)
			{
				ResultsTable rt = Rf.Tables[ti];
				MetaTable mt = rt.MetaTable;
				if (mt.Name.IndexOf("todo: table that needs atom number display") >= 0)
				{
					SS.I.DisplayAtomNumbers = (int)AtomNumberDisplayMode.All;
					break;
				}
			}

			StructureHighlightingInitialized = true;
			return;
		}

	}
}
