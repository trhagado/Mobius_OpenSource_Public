using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;
using Mobius.MolLib1;

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
	/// FormatField & associated methods of ResultsFormatter
	/// </summary>

	public partial class ResultsFormatter
	{
		static QueryEngine QueryEngine = new QueryEngine(); // single instance for getting images & other utility calls

		// Statics for GetMetaColumnConditionalFormatting

		static Query LastMCCFQuery = null; // last query we retrieved conditional formatting info for
		static int LastMCCFTime = 0; // time last query was used
		static int FormatFieldCount, FormatFieldTotalTime, FormatFieldAvgTime; // performance stats

		/// <summary>
		/// Simple field formatter
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="mdt"></param>
		/// <returns></returns>

		public FormattedFieldInfo FormatField(
			QueryColumn qc,
			MobiusDataType mdt)
		{
			ColumnInfo ci = Rf.GetColumnInfo(qc);
			FormattedFieldInfo ffi = FormatField(ci.Rt, ci.TableIndex, ci.Rfld, ci.FieldIndex, null, -1, mdt, -1, false);
			return ffi;
		}

		/// <summary>
		/// Format a single field
		/// </summary>
		/// <param name="rt">ResultsTable</param>
		/// <param name="ti">ResultsTable index</param>
		/// <param name="rfld">ResultsField</param>
		/// <param name="fi">ResultsField index</param>
		/// <param name="dataRow">DataRow containing field</param>
		/// <param name="dri">DataRow index</param>
		/// <param name="fieldValue">Field value, normally a MobiusDataType</param>
		/// <param name="bri">Row to start filling in buffer</param>
		/// <param name="storeInBuffer">Store in buffer if true</param>
		/// <returns>MobiusDataType with formatted value</returns>

		public FormattedFieldInfo FormatField(
			ResultsTable rt,
			int ti,
			ResultsField rfld,
			int fi,
			DataRowMx dataRow,
			int dri,
			object fieldValue,
			int bri,
			bool storeInBuffer)
		{
			int t0 = TimeOfDay.Milliseconds();
			FormattedFieldInfo ffi = FormatField2(rt, ti, rfld, fi, dataRow, dri, fieldValue, bri, storeInBuffer);
			t0 = TimeOfDay.Milliseconds() - t0;

			FormatFieldCount++;
			FormatFieldTotalTime += t0;
			FormatFieldAvgTime = FormatFieldTotalTime / FormatFieldCount;

			return ffi;
		}

		public FormattedFieldInfo FormatField2(
			ResultsTable rt,
			int ti,
			ResultsField rfld,
			int fi,
			DataRowMx dataRow,
			int dri,
			object fieldValue,
			int bri,
			bool storeInBuffer)
		{
			ColumnFormatEnum displayFormat;
			int fieldPosition, fieldWidth, decimals;
			string cid, formattedCid;
			int fi2, intCid, width, x;
			bool mergedField;
			MobiusDataType mdt;
			CompoundId cidObj = null;
			QualifiedNumber qn;
			NumberMx nex;
			CondFormat condFormat;
			CondFormatRule cfi;
			StringMx sx;
			DateTimeMx dex;
			MoleculeMx cs;
			ImageMx imageEx;
			ResultsField strRf = null;
			bool duplicatedValue = false;
			object strVo = null;
			double d1;

			string[] buf32 = new string[32];

			FormattedFieldInfo ffi = new FormattedFieldInfo(); // build formatted info here
			string hyperlink = "", link = "", linkEnd = "", imageFile = "";
			int imageWidth = 0, imageHeight = 0;
			ResultsField rfld2;
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc, mc2;
			CellStyleMx cellStyle = null;
			string txt, txt2, tok, debugMsg;
			int rc, ci, i1;
			bool hasDataForTable;

			fieldPosition = rfld.FieldX;
			qt = rt.QueryTable;
			qc = rfld.QueryColumn;
			mt = rt.MetaTable;
			mc = rfld.MetaColumn;

			bool isHtmlContent = qc.IsHtmlContent; // column format is html (content is expected to be html as well)

			if (!StructureHighlightingInitialized) InitializeStructureHilighting();

			//debugMsg = "FormatField ti = " + ti + ", fi = " + fi + ", dri = " + dri + ", fieldValue = "; // debug
			//if (fieldValue != null) debugMsg += fieldValue.ToString();
			//ClientLog.Message(debugMsg);

			if (mt.MetaBrokerType == MetaBrokerType.Annotation) fieldValue = fieldValue; // debug

			if (fieldValue is MobiusDataType) // if Mobius type copy any existing formatting info
			{
				mdt = fieldValue as MobiusDataType;
				ffi.ForeColor = mdt.ForeColor;
				ffi.BackColor = mdt.BackColor;
			}

			if (dataRow == null || dataRow[0] is System.DBNull || dataRow[0] == null) hasDataForTable = true;
			else hasDataForTable = DataTableManager.RowHasDataForTable(dataRow, ti);


			fieldPosition = rfld.FieldX; // starting col position for field
			fieldWidth = rfld.FieldWidth; // current field width in milliinches
																		//if (Rf.Grid && Rf.PageScale > 0)
																		//{ // extra grid scaling

			double scale = SS.I.TableColumnZoom / 100.0; // extra column scaling
			if (mc.IsGraphical) scale *= SS.I.GraphicsColumnZoom / 100.0; // extra graphical col scaling
			if (Rf.Grid && Rf.PageScale > 0) // overall page scaling for grid output
				scale *= (Rf.PageScale / 100.0);
			fieldWidth = (int)(fieldWidth * scale);

			//}

			//if (QueryManager.ResultsFormat.OutputDestination == OutputDest.Grid && // why?
			// QueryManager.MoleculeGrid.ViewWidth < fieldWidth)
			//  fieldWidth = QueryManager.MoleculeGrid.ViewWidth;

			GetOutputFormatForQueryColumn(qc, out displayFormat, out decimals);

			//if (qc.Decimals >= 0) decimals = qc.Decimals; // user-specified decimals?
			//else decimals = mc.Decimals;

			//if (qc.DisplayFormat != ColumnFormatEnum.Unknown) displayFormat = qc.DisplayFormat;
			//else displayFormat = mc.Format;

			mergedField = rfld.Merge || (rfld.MergedFieldList != null); // is field part of a merged set?

			// Process null fields

			MetaColumnType mcType = mc.DataType;

			if (mc.DataType == MetaColumnType.Date && fieldValue is DBNull) mc = mc;

			if (NullValue.IsNull(fieldValue) || // null value 
			 (!hasDataForTable && !mc.IsKey)) // nonfresh, nonkey value
				do
				{
					//					if (mt.IsRootTable && Rf.DuplicateKeyValues && fieldValue != null) break; // key field to duplicate

					ffi.FormattedText = SS.I.NullValueString;
					if (!storeInBuffer) return ffi;

				} while (false);

			// Process each data type

			switch (mcType)
			{
				///****************************************************************************
				case MetaColumnType.CompoundId:
					///****************************************************************************

					if (fieldValue is CompoundId)
					{
						cid = ((CompoundId)fieldValue).Value;
						cidObj = (CompoundId)fieldValue;
					}

					else // convert to a CompoundId
					{
						cid = fieldValue.ToString();
						////cidObj = new CompoundId();
						////cidObj.Value = cid;
					}

					////ffi = cidObj;

					cid = cid.Replace("\t", " ");
					cid = cid.Trim();
					//if (cid == "00012345") cid = cid; // debug

					if (mc.IsKey) CurrentKey = cid; // save most recently formatted as current 

					if (SS.I.RemoveLeadingZerosFromCids) // normal formatting
						formattedCid = CompoundId.Format(cid, mt);
					else formattedCid = cid; // just use internal formatting
					ffi.FormattedText = formattedCid; // copy in case we will be doing simple text format

					//if (ffi.FormattedText.Contains("PBCHM0")) ffi = ffi; // debug;

					if (!Rf.CombineStructureWithCompoundId || !mt.IsRootTable) goto textCompoundId; // include struct?
					if (Rf.CombinedStructureField == null) goto textCompoundId;
					if (!hasDataForTable /* && !Rf.DuplicateKeyValues */) goto textCompoundId; // must be fresh data

					for (fi2 = 0; fi2 < fi; fi2++) // is this the first occurance of cmpdno?
					{
						rfld2 = rt.Fields[fi2];
						mc2 = rfld2.MetaColumn;
						if (mc2.Name == mc.Name) goto textCompoundId; // earlier occurance?
					}

					strRf = Rf.CombinedStructureField;
					strVo = dataRow[strRf.VoPosition];
					if (strVo == null) goto textCompoundId;

					try { cs = MoleculeMx.ConvertObjectToChemicalStructure(strVo); }
					catch (Exception ex) { cs = new MoleculeMx(); }
					dataRow[strRf.VoPosition] = cs; // put ChemicalStructure object back into buffer

					if (cellStyle == null && strRf.QueryColumn.CondFormat != null && // get any cell style for structure if not already defined
						cs.BackColor == Color.Empty)
					{
						cellStyle = GetCondFormatCellStyle(strRf.QueryColumn, cs, Rf);
						cs.BackColor = cellStyle.BackColor;
					}

					ffi.FormattedText = formattedCid;

					if (!storeInBuffer && !Rf.Grid)
					{
						return ffi;
						////return new MobiusDataType(fTxt); // done
					}

					ffi.FormattedText = AppendMergedFields(rt, fi, dataRow, ffi.FormattedText);
					//ffi.ForeColor = Color.Green; // debug

					if (Rf.Word)
					{
						AssureTbFree(ti, bri); // count line
						Tb.Lines[bri] += "t " + fieldPosition.ToString() + " " + ffi.FormattedText + "\t"; // plug in
						FormatStructure(cs, cellStyle, 's', fieldPosition, fieldWidth, bri, rfld, dataRow); // format below regno
					}

					else if (Rf.Html) // add cn to structure graphic to avoid splitting them
					{
						ffi.FormattedText = ConvertMfSymbolsToHtml(ffi.FormattedText);
						cs.Caption = ffi.FormattedText;
						FormatStructure(cs, cellStyle, 'S', // add rn
							fieldPosition, (int)(fieldWidth * 1.05), // enlarge struct slightly for better display
							bri, rfld, dataRow);
					}

					else if (Rf.Excel) // include cn with struct
					{
						Tb.Lines[bri] += "t " + fieldPosition.ToString() + " " + ffi.FormattedText + "\t"; // plug in
						FormatStructure(cs, cellStyle, 's', fieldPosition, fieldWidth, bri, rfld, dataRow); // format below regno
					}

					else if (Rf.Grid) // combine cn with struct
					{
						cs.Caption = ffi.FormattedText; // make the CompoundId a caption
						ffi = FormatStructure(cs, cellStyle, 's', fieldPosition, (int)(fieldWidth * 1.0), bri, rfld, dataRow); // format below regno
						ffi.Hyperlink = // link to popup the compound id operation menu
							"http:////Mobius/command?ShowContextMenu:CompoundIdContextMenu:" + formattedCid;
						//if (cidObj != null && String.IsNullOrEmpty(cidObj.Hyperlink)) // store link with object
						//  cidObj.Hyperlink = ffi.Hyperlink;
					}

					else goto textCompoundId; // oops

					break;

				textCompoundId:
					if ((Rf.Grid || (Rf.Html && Rf.NotPopupOutputFormContext)) && ffi.FormattedText != "" && mt.HasAssociatedStructure() &&
						mc.ClickFunction == "") // include additional formatting for cid (hyperlink, cond formatting ...)
					{
						hyperlink = "http:////Mobius/command?ShowContextMenu:CompoundIdContextMenu:" + ffi.FormattedText; // link to popup the compound id operation menu

						if (cellStyle == null)
							cellStyle = GetCondFormatCellStyle(qc, fieldValue, Rf);

						SetSpecialCompoundIdHilighting(mc, ffi, ref cellStyle);

						if (Rf.Grid) break;

						else // html format
						{
							ffi.FormattedText = "<a href=\"" + hyperlink + "\">" + ffi.FormattedText + "</a>";
							goto AppendHtmlField;
						}
					}

					else goto AppendTextField;

				///****************************************************************************
				case MetaColumnType.String:
					///****************************************************************************

					////if (fieldValue is StringEx)
					////{
					////  ffi.FormattedText = ((StringEx)fieldValue).Value;
					////sx = fieldValue as StringEx;
					////ffi = sx;
					//if (!String.IsNullOrEmpty(sx.DbLink)) hyperlink = sx.DbLink; // use dblink if defined (causes bogus hyperlinks for annotations)
					//else hyperlink = sx.Hyperlink;
					////}

					////else if (fieldValue is string)
					////{
					////  ffi.FormattedText = fieldValue.ToString();
					////  ffi = new StringEx(ffi.FormattedText);
					////}

					////else 
					if (fieldValue is QualifiedNumber)
					{
						qn = (QualifiedNumber)fieldValue;
						ffi.FormattedText = FormatQualifiedNumber(qn, rfld, mergedField, displayFormat, decimals, Query.StatDisplayFormat);
					}

					////else if (fieldValue == null)
					////{
					////  ffi.FormattedText = SS.I.NullValueString;
					////}

					else
					{
						ffi.FormattedText = fieldValue.ToString();
						////ffi = new StringEx(ffi.FormattedText);
					}

				//if (ffi.FormattedText != null && Lex.Contains(ffi.FormattedText, "Type=Sketch")) ffi = ffi; // debug

				//if (Rf.Html && // include any qn hyperlink info for these
				// fmtdFld.Hyperlink != "" && // must have result id
				// !fTxt.ToLower().Contains("<a href")) // skip if have link info already in txt
				//{
				//  uri = FormatHyperlink(qc, fmtdFld);
				//}

				AppendTextField:

					ffi.FormattedText = NormalizeText(ffi.FormattedText);

					if (cellStyle == null && !qc.MetaColumn.IsNumeric && rfld.IsMainValue) // if not a number then get any conditional formatting if not already defined
						cellStyle = GetCondFormatCellStyle(qc, fieldValue, Rf); // pass fieldValue to pick up any qn background color

					//					if (txt == "") fieldValue = txt; // avoid link info if QN with blank text

					if (Rf.Word || Rf.Excel) // remove newlines 
					{
						ffi.FormattedText = ffi.FormattedText.Replace("\r", "");
						ffi.FormattedText = ffi.FormattedText.Replace("\n", " ");
					}

					if (!storeInBuffer)
					{
						if (Rf.Grid)
						{
							if (isHtmlContent) { } // if html format then assume just use the anchors in the HTML as the hyperlinks directly

							else if (Lex.Contains(ffi.FormattedText, "<a href=\"")) // if anchor tag in text parse it out
								ParseAnchorTag(ffi.FormattedText, out ffi.FormattedText, out hyperlink);

							else if (mc.ClickFunction != "" && Lex.Ne(mc.ClickFunction, "None")) // add click function for field
								hyperlink = BuildClickFunctionText(rfld, dataRow, dri, "");

							else if (Lex.IsUri(ffi.FormattedText) || ffi.FormattedText.ToLower().StartsWith(@"\\")) // link in text
							{ // make hyperlink match text
								hyperlink = ffi.FormattedText;
								if (hyperlink.ToLower().StartsWith("www.")) hyperlink = "http://" + hyperlink; // fully qualify
							}

							else if (qc.MetaColumn.DetailsAvailable && fieldValue is MobiusDataType) // or we explicitly know that details are available
							{
								mdt = fieldValue as MobiusDataType;
								hyperlink = FormatDetailsAvailableHyperlink(qc, mdt);
								//hyperlink = "http://Mobius/command?" +
								//  "ClickFunction DisplayDrilldownDetail " +
								//  qc.QueryTable.MetaTable.Name + " " + qc.MetaColumn.Name + " 1 " + 
								//  Lex.AddSingleQuotes(fmtdFld.DbLink);
							}

						}

						break;
					}

					ffi.FormattedText = AppendMergedFields(rt, fi, dataRow, ffi.FormattedText);

				StoreTextField:

					////if (ffi == null) ffi = new StringEx(); // allocate string if no existing MobiusDataType

					if (Rf.Grid) throw new NotSupportedException();

					else if (Rf.Word || Rf.Excel) // keep together on one line 
					{
						if (cellStyle != null) // apply style to cell
							ffi.FormattedText = "<CellStyle " + cellStyle.Serialize() + ">" + ffi.FormattedText;
						Tb.Lines[bri] += "t " + fieldPosition.ToString() + " " + ffi.FormattedText + "\t"; // plug in
					}

					else if (Rf.Html) // add table data tags 
					{
						//			if (fTxt.Contains("2011")) fTxt = fTxt; // debug
						hyperlink = ""; // hyperlink reference
						ParseAnchorTag(ffi.FormattedText, out txt, out hyperlink);
						int unlimitedWidth = 1000000000; // don't add any breaks to the string
						rc = FormatTextField(txt, Rf.OutputDestination, Rf.GraphicsContext, fieldPosition, unlimitedWidth, 0, buf32);
						AssureTbFree(ti, bri + rc - 1); // be sure adequate space in buffer
						if (buf32[0] == "") buf32[0] = "<br>"; // html blank
						buf32[0] = buf32[0].Replace("\n", "<br>");

						if (mergedField) hyperlink = ""; // don't use link if merged field

						else if (hyperlink != "") { } // if already have href then don't add another

						else if (Lex.IsUri(ffi.FormattedText) || ffi.FormattedText.ToLower().StartsWith(@"\\")) // link?
						{
							txt2 = ffi.FormattedText;
							if (txt2.ToLower().StartsWith("www.")) txt2 = "http://" + txt2; // fully qualify
							buf32[0] = "<a href=" + Lex.Dq(txt2) +
								" target=\"_blank\">" + buf32[0] + "</a>"; // cause link to open on new page

						}

						else if (mc.ClickFunction != "" && Lex.Ne(mc.ClickFunction, "None")) // add click function for field
							buf32[0] = BuildClickFunctionText(rfld, dataRow, dri, buf32[0]);

						else if (qc.MetaColumn.DetailsAvailable && fieldValue is MobiusDataType) // or we explicitly know that details are available
						{
							mdt = fieldValue as MobiusDataType;
							txt2 = FormatDetailsAvailableHyperlink(qc, mdt);
							if (!Lex.IsNullOrEmpty(txt2)) // build complete anchor tag
								buf32[0] = "<a href=" + Lex.Dq(txt2) +
									" target=\"_blank\">" + buf32[0] + "</a>"; // cause link to open on new page
						}

						if (hyperlink != "") // restore link info
							buf32[0] = hyperlink + buf32[0] + "</a>";

						Tb.Lines[bri] +=
							"E <td>" + buf32[0] + " </td>\t";
					}

					else if (Rf.TextFile) // csv, keep together on one line
					{
						ffi.FormattedText = ffi.FormattedText.Replace("\r", "");
						ffi.FormattedText = ffi.FormattedText.Replace("\n", " ");
						Tb.Lines[bri] += "t " + fieldPosition.ToString() + " " + ffi.FormattedText + "\t"; // plug in
					}

					else if (Rf.SdFile)
					{
						tok = rfld.MergeLabel; // get unique & valid name
						tok = ">  <" + tok + ">\r\n"; // field name line
						ffi.FormattedText = tok + ffi.FormattedText + "\r\n\r\n"; // blank line at end
						Tb.Lines[bri] += "t " + fieldPosition.ToString() + " " + ffi.FormattedText + "\t"; // plug in

						if ((Rf.StructureFlags & MoleculeTransformationFlags.StructuresOnly) != 0)
						{
							cs = MoleculeUtil.SelectMoleculeForCid(CurrentKey, null);
							if (cs != null)
							{
								FormatStructure(cs, null, 'S', fieldPosition, fieldWidth, bri + 1, rfld, dataRow);
							}
						}
					}

					else // other outputs (Rf.TextFile)
					{
						rc = FormatTextField(ffi.FormattedText, Rf.OutputDestination, Rf.GraphicsContext, fieldPosition, fieldWidth, 0, buf32);
						AssureTbFree(ti, bri + rc - 1); // be sure adequate space in buffer
						TextLines = rc; // keep number of lines for possible later use with pending graph
						for (i1 = 0; i1 < rc; i1++)
						{
							Tb.Lines[bri + i1] += buf32[i1]; // append field info
						}
					}

					break;

				///****************************************************************************
				case MetaColumnType.Integer:
					///****************************************************************************

					if (fieldValue is NumberMx)
					{
						nex = (NumberMx)fieldValue;
						ffi.FormattedText = String.Format("{0}", (int)nex.Value);
					}

					else if (fieldValue is QualifiedNumber)
					{
						qn = (QualifiedNumber)fieldValue;
						ffi.FormattedText = FormatQualifiedNumber(qn, rfld, mergedField, displayFormat, decimals, Query.StatDisplayFormat);
					}

					else if (fieldValue is int)
					{
						i1 = (int)fieldValue;
						ffi.FormattedText = String.Format("{0}", i1);
					}

					else if (int.TryParse(fieldValue.ToString(), out i1)) // see if string form of integer
					{
						ffi.FormattedText = String.Format("{0}", i1);
					}

					else // other unexpected type, do conversion to string within qualified number
					{
						ffi.FormattedText = fieldValue.ToString();
						qn = new QualifiedNumber();
						qn.TextValue = ffi.FormattedText;
						////ffi = qn;
						goto AppendTextField;
					}

					goto AppendNumericField;

				///****************************************************************************
				case MetaColumnType.Number:
					///****************************************************************************

					if (fieldValue is double)
					{
						d1 = (double)fieldValue;
						ffi.FormattedText = QualifiedNumber.FormatNumber(d1, displayFormat, decimals);
						////ffi = new NumberEx(d1);
					}

					else if (fieldValue is float)
					{
						d1 = (float)fieldValue;
						ffi.FormattedText = QualifiedNumber.FormatNumber(d1, displayFormat, decimals);
						////ffi = new NumberEx(d1);
					}

					else if (fieldValue is NumberMx)
					{
						d1 = (double)((NumberMx)fieldValue).Value;
						ffi.FormattedText = QualifiedNumber.FormatNumber(d1, displayFormat, decimals);
					}

					else if (fieldValue is QualifiedNumber)
					{
						qn = (QualifiedNumber)fieldValue;
						ffi.FormattedText = FormatQualifiedNumber(qn, rfld, mergedField, displayFormat, decimals, Query.StatDisplayFormat);
					}

					else // other unexpected type, do conversion to string within qualified number
					{
						ffi.FormattedText = fieldValue.ToString();
						qn = new QualifiedNumber();
						qn.TextValue = ffi.FormattedText;
						////ffi = qn;
						goto AppendTextField;
					}

					goto AppendNumericField;

				///****************************************************************************
				case MetaColumnType.QualifiedNo:
					///****************************************************************************

					if (fieldValue is QualifiedNumber)
					{
						qn = (QualifiedNumber)fieldValue;
					}

					else if (Lex.IsDouble(fieldValue.ToString()))
					{
						qn = new QualifiedNumber();
						Double.TryParse(fieldValue.ToString(), out d1);
						qn.NumberValue = d1;
					}

					else // other unexpected type, do conversion to string
					{
						ffi.FormattedText = fieldValue.ToString();
						qn = new QualifiedNumber();
						qn.TextValue = ffi.FormattedText;
						////ffi = qn;
						goto AppendTextField;
					}

					ffi.FormattedText = FormatQualifiedNumber(qn, rfld, mergedField, displayFormat, decimals, Query.StatDisplayFormat);

				// Append the numeric field value to buffer
				// Expected inputs:
				//  txt - text of formatted number
				//  qn - Qualified number containing value
				//  width - width this field occupies in milliinches

				AppendNumericField:

					if (ffi.FormattedText == null) ffi.FormattedText = "";
					ffi.FormattedText = ffi.FormattedText.Trim();

					if (cellStyle == null && rfld.IsMainValue) // get any cell style if not already defined
						cellStyle = GetCondFormatCellStyle(qc, fieldValue, Rf);

					if (!storeInBuffer)
					{
						if (Lex.Contains(ffi.FormattedText, "<a href=\"")) // if anchor tag in text parse it out
							ParseAnchorTag(ffi.FormattedText, out ffi.FormattedText, out hyperlink);

						if (Rf.Grid && mc.ClickFunction != "" && Lex.Ne(mc.ClickFunction, "None")) // add click function for field
							hyperlink = BuildClickFunctionText(rfld, dataRow, dri, ffi.FormattedText);

						if (Lex.Eq(mc.ClickFunction, "ShowSubmissionRegistrationHistory"))
							SetSpecialCompoundSubmissionIdHilighting(mc, ffi, ref cellStyle);

						break;
					}

					ffi.FormattedText = AppendMergedFields(rt, fi, dataRow, ffi.FormattedText);

					txt2 = ParseTextFromAnchorTag(ffi.FormattedText); // get text without any html tags
					width = Rf.GraphicsContext.StringWidth(txt2);
					x = fieldPosition + fieldWidth - width;
					AssureTbFree(ti, bri); // associate line with correct table

					if (Rf.Grid) // ever true?
					{
						if (width > fieldWidth || ffi.FormattedText.IndexOf("\n") >= 0) // break if too long or multiple lines 
							goto AppendTextField;

						if (mc.ClickFunction != "" && Lex.Ne(mc.ClickFunction, "None")) // add click function for field
							hyperlink = BuildClickFunctionText(rfld, dataRow, dri, ffi.FormattedText);
					}

					else if (Rf.Excel || Rf.Word)
					{
						if (width > fieldWidth || ffi.FormattedText.IndexOf("\n") >= 0) // break if too long or multiple lines 
							goto AppendTextField;

						if (cellStyle != null) // apply style to cell
							ffi.FormattedText = "<CellStyle " + cellStyle.Serialize() + ">" + ffi.FormattedText;

						Tb.Lines[bri] += "t " + x.ToString() + " " + ffi.FormattedText + '\t'; // right justify
					}

					else if (Rf.Html) // do proper html alignment
					{
						ffi.FormattedText = ffi.FormattedText.Replace("\n", "<br>"); // replace newlines with breaks

						if (mergedField) { } // can't show links if merged field

						else if (ffi.FormattedText.ToLower().IndexOf("http:") == 0) // web link?
							ffi.FormattedText = "<a href=" + Lex.Dq(ffi.FormattedText) + ">" + ffi.FormattedText + "</a>";

						else if (mc.ClickFunction != "") // add click function for field
						{
							hyperlink = BuildClickFunctionText(rfld, dataRow, dri, ffi.FormattedText);
							ffi.FormattedText = hyperlink;
						}

						if (rfld.MergedFieldList != null) tok = "left"; // left align if multiple merged fields
						else tok = "right";
						Tb.Lines[bri] += "E <td align=\"" + tok + "\">" + ffi.FormattedText + " </td>\t";
					}

					else if (Rf.SdFile || Rf.TextFile)
					{
						goto AppendTextField;
					}

					break;

				///****************************************************************************
				case MetaColumnType.Date:
					///****************************************************************************

					DateTime dt = DateTime.MinValue;

					try
					{
						string formatString = DateTimeMx.GetFormatString(qc);

						if (fieldValue is DateTimeMx)
						{
							dex = (DateTimeMx)fieldValue;
							dt = dex.Value;
							ffi.FormattedText = DateTimeMx.Format(dt, formatString);
						}

						else if (fieldValue is DateTime)
						{
							dt = (DateTime)fieldValue;
							ffi.FormattedText = DateTimeMx.Format(dt, formatString);
							////ffi = new DateTimeEx(dt);
						}

						else if (fieldValue is QualifiedNumber)
						{ // dates within qualified numbers are stored in TextValue

							qn = (QualifiedNumber)fieldValue;
							if (qn.TextValue == "")
								ffi.FormattedText = SS.I.NullValueString;
							else
							{
								dt = DateTimeMx.NormalizedToDateTime(qn.TextValue);
								ffi.FormattedText = DateTimeMx.Format(dt, formatString);
							}
						}

						else if (fieldValue is StringMx && !qc.IsAggregated)
						{
							sx = (StringMx)fieldValue;
							dt = DateTimeMx.NormalizedToDateTime(sx.Value);
							ffi.FormattedText = DateTimeMx.Format(dt, formatString);
						}

						else // other unexpected type, do conversion to string
						{
							ffi.FormattedText = fieldValue.ToString();
							////ffi = new StringEx(ffi.FormattedText);
						}


					}
					catch (Exception ex)
					{
						ffi.FormattedText = "Invalid date";
					}

					goto AppendTextField;

				///****************************************************************************
				case MetaColumnType.Structure:
					///****************************************************************************

					if (NullValue.IsNull(fieldValue)) cs = null;

					if (fieldValue is MoleculeMx) cs = fieldValue as MoleculeMx;

					else
					{
						try { cs = MoleculeMx.ConvertObjectToChemicalStructure(fieldValue); }
						catch (Exception ex) { cs = new MoleculeMx(); }
						if (fieldValue is MobiusDataType)
						{ // keep link info
							mdt = fieldValue as MobiusDataType;
							if (mdt != null)
							{
								cs.Hyperlink = mdt.Hyperlink;
								cs.DbLink = mdt.DbLink;
							}
						}
						dataRow[rfld.VoPosition] = cs; // put ChemicalStructure object back into buffer
					}

					////ffi = cs;

					if (cs != null)
					{

						if (cellStyle == null && qc.CondFormat != null && rfld.IsMainValue) // get any cell style if not already defined
						{ // get any cell style if not already defined
							cellStyle = GetCondFormatCellStyle(qc, cs, Rf);
						}

						string clickFunction = mc.ClickFunction;
						if (clickFunction == "" && mc.MetaTable.IsRootTable && mc == mc.MetaTable.FirstStructureMetaColumn)
							clickFunction = "SelectCompoundStructure"; // default click function for struct in root table

						if (clickFunction == "" || (!Rf.Html && !Rf.Grid)) // no clickfunction
						{
							ffi = FormatStructure(cs, cellStyle, 'S', fieldPosition, fieldWidth, bri, rfld, dataRow);
						}

						else // format for html or grid with a click function
						{
							CurrentKey = DtMgr.GetRowKey(dataRow); // get current compound number

							hyperlink = // create a link to respond to the click
								"<a href=\"http:////Mobius/command?" + clickFunction + " " +
								CurrentKey + " " + mt.Name + " " + mc.Name + " " + CurrentKey + "\"></a>";

							ffi = FormatStructure(cs, cellStyle, 'S', fieldPosition, fieldWidth, bri, rfld, dataRow);
						}

						if (ffi == null) ffi = new FormattedFieldInfo(); // be sure defined
																														 //if (ffi != null && ffi.FormattedText != null)
																														 //  ffi.FormattedText = ffi.FormattedText; // need to return text value
																														 //else ffi.FormattedText = "";
					}

					else // no structure
					{
						if (!Rf.Grid)
						{
							AssureTbFree(ti, bri);
							Tb.Lines[bri] += "t " + fieldPosition.ToString() + "  \t"; // blank
						}
					}
					break;

				///****************************************************************************
				case MetaColumnType.Hyperlink:
				case MetaColumnType.Html:
					///****************************************************************************

					if (fieldValue is StringMx)
					{
						sx = fieldValue as StringMx;
						ffi.FormattedText = sx.Value;
						hyperlink = sx.Hyperlink;
						if (Lex.IsNullOrEmpty(hyperlink)) hyperlink = ffi.FormattedText; // set both the same if one is blank & other not
						else if (Lex.IsNullOrEmpty(ffi.FormattedText)) ffi.FormattedText = hyperlink;
					}
					else ffi.FormattedText = hyperlink = fieldValue.ToString();

					if ((Rf.Grid || Rf.Html) && !Lex.IsNullOrEmpty(hyperlink))
						ffi.FormattedText = "Link"; // just show "Link" for grid & html display

					if ((!Rf.Html && !Rf.Grid) || mergedField)
					{
						i1 = ffi.FormattedText.IndexOf(" >>> ");
						if (i1 >= 0) ffi.FormattedText = ffi.FormattedText.Substring(0, i1); // remove any ref info (could remove tags also)
						goto AppendTextField;
					}

					if (mc.ClickFunction != "") // explicit click function?
					{
						hyperlink = BuildClickFunctionText(rfld, dataRow, dri, "Link");
					}

					else if (Lex.IsUri(ffi.FormattedText))
					{
						ffi.FormattedText = "<a href=" + Lex.Dq(ffi.FormattedText);
						if (Rf.Html) ffi.FormattedText += " target=\"_blank\""; // cause link to open on new browser page
						ffi.FormattedText += ">" +
							"Link" + "</a>";
					}

					else // see if text >>> link_addr shortcut 
					{
						i1 = ffi.FormattedText.IndexOf(">>>");
						if (i1 >= 0)
						{
							ffi.FormattedText = "<a href=" + Lex.Dq(ffi.FormattedText.Substring(i1 + 3).Trim()) + ">" +
								ffi.FormattedText.Substring(0, i1).Trim() + "</a>";
						}
					}

				AppendHtmlField:

					if (storeInBuffer) ffi.FormattedText = AppendMergedFields(rt, fi, dataRow, ffi.FormattedText);

					else break; // done if not storing in buffer ( return new MobiusDataType(fTxt);)

					if (Rf.Grid) // shouldn't be storing in buffer for grid
						throw new NotImplementedException();

					else if (Rf.Html && !Lex.IsNullOrEmpty(hyperlink)) // format hyperlink for html
					{
						string anchorTag = hyperlink;
						if (!Lex.StartsWith(hyperlink, "<a href=")) // link fully defined?
							anchorTag =
								"<a href=" + Lex.Dq(hyperlink) +
								" target=\"_blank\">" + // cause link to open on new browser page
								ffi.FormattedText + "</a>";

						ffi.FormattedText = anchorTag;
					}

					ffi.FormattedText = ConvertMfSymbolsToHtml(ffi.FormattedText); // fix up any special mf symbols
					ffi.FormattedText.Replace("\n", "<br>"); // replace any newlines with breaks

					MatchCollection mc1 = Regex.Matches(ffi.FormattedText, @"\<br\>"); // count # of breaks
					if (mc1 == null || mc1.Count == 0) i1 = 1;
					else i1 = mc1.Count;

					AssureTbFree(ti, bri + i1 - 1); // be sure adequate space in buffer
					if (ffi.FormattedText == "") ffi.FormattedText = "<br>"; // html blank
					Tb.Lines[bri] += "E <td>" + ffi.FormattedText + " </td>\t";
					break;

				///****************************************************************************
				case MetaColumnType.Binary:
					///****************************************************************************

					ffi.FormattedText = fieldValue.ToString(); // simple text for now
					goto AppendTextField;

				///****************************************************************************
				case MetaColumnType.MolFormula:
					///****************************************************************************

					////if (fieldValue is StringEx) { }
					////else ffi = new StringEx(fieldValue.ToString());

					ffi.FormattedText = fieldValue.ToString(); // simple text for now
					goto AppendTextField;

				///****************************************************************************
				case MetaColumnType.DictionaryId:
					///****************************************************************************

					////if (fieldValue is StringEx) { }
					////else ffi = new StringEx(fieldValue.ToString());

					ffi.FormattedText = fieldValue.ToString(); // simple text for now
					txt2 = DictionaryMx.LookupWordByDefinition(mc.Dictionary, ffi.FormattedText);
					if (txt2 != null && txt2 != "") ffi.FormattedText = txt2;
					goto AppendTextField;

				///****************************************************************************
				case MetaColumnType.Image:
					///****************************************************************************

					Bitmap bmp = null;

					if (fieldValue is ImageMx)
					{
						imageEx = fieldValue as ImageMx;
						if (imageEx.Value != null)
							bmp = imageEx.Value;

						else if (!String.IsNullOrEmpty(imageEx.DbLink))
							fieldValue = imageEx.DbLink;

						else // not defined
						{
							ffi.FormattedText = "";
							goto AppendTextField;
						}
					}

					else if (fieldValue is Bitmap)
					{
						bmp = (Bitmap)fieldValue;
						imageEx = new ImageMx();
						imageEx.Value = bmp;
					}

					else // assume field value is information pointing to image that broker can handle
					{
						imageEx = new ImageMx();
						imageEx.DbLink = fieldValue.ToString();
					}

					hyperlink = // reference command to display image full size
						"http://Mobius/command?ClickFunction DisplayDrilldownDetail " +
						mc.MetaTable.Name + " " + mc.Name +
						(qt.MetaTable.UseSummarizedData ? " 1 " : " 2 ") + Lex.AddSingleQuotes(imageEx.DbLink);
					link = "<a href=" + Lex.Dq(hyperlink) + ">"; // link to original
					linkEnd = "</a>";

					width = (int)((fieldWidth / 1000.0) * GraphicsMx.LogicalPixelsX * 1.0); // width in pixels 
					bool saveToFile = Rf.Html || Rf.Excel || Rf.Word;
					try
					{
						FormatGraphic(mc, imageEx, width, saveToFile, out imageFile);
					}
					catch (Exception ex)
					{
						ffi.FormattedText = ex.Message;
						goto AppendTextField;
					}

					bmp = imageEx.FormattedBitmap;
					if (bmp == null)
					{
						ffi.FormattedText = "";
						goto AppendTextField;
					}

					ffi.FormattedBitmap = bmp; // imageEx;

					imageHeight = (int)((bmp.Height * 1000.0) / (GraphicsMx.LogicalPixelsY * 1.0)); // height in milli-inches
					int imageLines = imageHeight / Rf.LineHeight + 1; // lines needed
					imageWidth = (int)((bmp.Width * 1000.0) / (GraphicsMx.LogicalPixelsX * 1.0));

					ImageCount++;

					if (Rf.Grid) break;

					else if (Rf.Html)
					{
						ffi.FormattedText = "<img src=\"" + imageFile + "\" border=0>"; // build tag for image
						if (!mergedField)
							ffi.FormattedText = link + ffi.FormattedText + linkEnd;

						AssureTbFree(ti, bri + imageLines - 1); // reserve lines for image
						goto AppendHtmlField;
					}

					else if (Rf.Excel || Rf.Word)
					{
						ffi.FormattedText = "g " + fieldPosition.ToString() + " " + imageWidth.ToString() + " " + imageHeight.ToString() +
							" " + imageFile + "\t";

						if (storeInBuffer) // need to get additional text first?
						{
							AssureTbFree(ti, bri);
							txt2 = ".";
							txt2 = AppendMergedFields(rt, fi, dataRow, txt2);
							if (txt2 != ".")
							{
								PendingGraphicsCommand = ffi.FormattedText;
								ffi.FormattedText = txt2; // put in proper var
								goto StoreTextField;
							}
							else
							{
								ffi.FormattedText = "G" + ffi.FormattedText.Substring(1);
								ffi.FormattedText.Replace("\t", link + "\t"); // put link at end
								Tb.Lines[bri] += ffi.FormattedText;
								break;
							}
						}

						else
						{
							PendingGraphicsCommand = ffi.FormattedText;
							return new FormattedFieldInfo("."); // placeholder to generate label
						}
					}

					else // nongraphics output device
					{
						ffi.FormattedText = "Image/Graph";
						goto AppendTextField;
					}

				//****************************************************************************
				default: // unknown datatype
				//****************************************************************************

					ffi.FormattedText = "Unknown format for column " + mt.Name + "." + mc.Name;
					////ffi = new StringEx(ffi.FormattedText);
					goto AppendTextField;

			} // end of case on datatype

			///****************************************************************************
			/// Complete processing for field
			///****************************************************************************

			if (!String.IsNullOrEmpty(PendingGraphicsCommand)) // append any pending graphics command
			{
				Tb.Lines[bri] += PendingGraphicsCommand;
				PendingGraphicsCommand = "";
			}

			// Fill in format info & return


			////if (ffi == null) ffi = new StringEx();
			if (Rf.Grid) // save formatting info if formatting for grid only
			{
				//if (fTxt == "1-Jan-1") fTxt = fTxt; // debug
				ffi.FormattedText = ffi.FormattedText;
				ffi.Hyperlink = hyperlink;
				if (cellStyle != null) // apply style to cell?
				{
					ffi.ForeColor = cellStyle.ForeColor;
					ffi.BackColor = cellStyle.BackColor;
				}
			}

			//if (ffi.FormattedText != null && Lex.Contains(ffi.FormattedText, "Type=Sketch")) ffi = ffi; // debug

			//debugMsg = ""; // debug
			//if (ffi.FormattedText != null) debugMsg = ffi.FormattedText; // debug
			//ClientLog.Message("ffi.FormattedText = " + debugMsg); // debug

			return ffi;
		}

		/// <summary>
		/// Get the proper output format for a QueryColumn
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="displayFormat"></param>
		/// <param name="decimals"></param>

		public static void GetOutputFormatForQueryColumn(
			QueryColumn qc,
			out ColumnFormatEnum displayFormat,
			out int decimals)
		{
			displayFormat = qc.ActiveDisplayFormat;
			decimals = qc.ActiveDecimals;

			if (displayFormat == ColumnFormatEnum.Unknown || displayFormat == ColumnFormatEnum.Default)
			{ // default attributes for user are 3rd choice
				displayFormat = SS.I.DefaultNumberFormat;
				decimals = SS.I.DefaultDecimals;
			}

			return;
		}

		/// <summary>
		/// Normalize text before formatting
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>

		public static string NormalizeText(
					string txt)
		{
			if (txt.IndexOf("\t") >= 0) // convert tabs to spaces
				txt = txt.Replace("\t", " ");
			txt = txt.TrimEnd(); // remove trailing blanks
			while (txt.Length > 0) // remove terminal newlines 
			{
				if (txt[txt.Length - 1] == '\n' || txt[txt.Length - 1] == '\r')
					txt = txt.Substring(0, txt.Length - 1); // remove terminal newlines
				else break;
			}
			return txt;
		}

		/// <summary>
		/// Format associated merged fields and append to existing text
		/// </summary>
		/// <param name="rt"></param>
		/// <param name="fi"></param>
		/// <param name="inResult">Existing formated data</param>
		/// <returns>Updated formated data</returns>

		string AppendMergedFields(
			ResultsTable rt,
			int fi,
			DataRowMx dataRow,
			//int dri, // include later if this method is called
			string inResult)
		{
			string label, txt, tok;
			ResultsTable rt2;
			ResultsField rfld, rfld2;
			MetaTable mt, mt2;
			MetaColumn mc, mc2, mcs;
			MergedField mf;
			List<MergedField> mfl;
			float f1, f2;
			int ti2, li, fi2, i1, i2;
			int dri = -1; // remove later if defined as parm
			bool fresh;
			object fieldValue;

			string result = inResult;
			mt = rt.MetaTable;
			rfld = rt.Fields[fi];
			mc = rfld.MetaColumn;

			if (Rf.Html && // convert string to proper html format if needed
				result.ToLower().IndexOf("<a href=") < 0 &&
				result.ToLower().IndexOf("<img ") < 0 &&
				result != "<br>")
				result = ConvertTextToHtml(result);

			mfl = rfld.MergedFieldList;
			if (mfl == null) return result;
			if (mc.Label != "" && result != "") // add label for 1st field if not blank
				result = mc.Label + ": " + result;

			for (li = 0; li < mfl.Count; li++)
			{
				mf = (MergedField)mfl[li];
				ti2 = mf.TableIndex;
				mt2 = mf.MetaTable;
				mc2 = mf.MetaColumn;
				rt2 = null;
				fi2 = 0;
				rfld2 = null; // todo: fix this for FormatField
				fieldValue = null;
				FormattedFieldInfo ffi = FormatField(rt2, ti2, rfld2, fi2, dataRow, dri, fieldValue, 0, false); // format without storing in buffer
				txt = ffi.FormattedText;

				if (Rf.Html &&  // make string acceptable as html
					txt.ToLower().IndexOf("<a href=") < 0 && txt.ToLower().IndexOf("<img ") < 0)
					txt = ConvertTextToHtml(txt);

				if (txt != "")
				{
					label = mc.Label;
					if (label != "") label += ": ";
					txt += label;

					if (result == "") result = txt;
					else
					{
						tok = "\n";
						if (mt.IsRootTable && rfld2.FieldX == 0 &&
							Rf.CombineStructureWithCompoundId) // fit in structure field? 
						{
							mcs = null; // todo: get mc of structure field
							f1 = mcs.Width;
							f2 = label.Length + mc2.Width; // this field's length
							if (result.Length + 3 + f2 <= f1) tok = ",  "; // substitute comma & spaces for return
						}
						result += tok + txt;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Remove html tags from text
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>

		public static string ParseTextFromAnchorTag(
			string tag)
		{
			string txt, href;
			ParseAnchorTag(tag, out txt, out href);
			return txt;
		}

		/// <summary>
		/// Remove html tags from text
		/// </summary>
		/// <param name="txt"></param>
		/// <param name="?"></param>
		/// <returns></returns>

		public static void ParseAnchorTag(
			string tag,
			out string txt,
			out string href)
		{
			txt = tag;
			href = "";
			string prefix = "<a href=\"";
			int i1 = Lex.IndexOf(tag, prefix);
			if (i1 != 0) return; // not an anchor tag
			tag = tag.Substring(prefix.Length);

			i1 = Lex.IndexOf(tag, "\">");
			if (i1 < 0) return; // not an anchor tag

			href = tag.Substring(0, i1); // extract link text
			txt = tag.Substring(i1 + 2).Trim(); // extract display text
			txt = Lex.Replace(txt, "</a>", ""); // remove end tag
			return;
		}

		/// <summary>
		/// Format text into a fixed width column
		/// </summary>
		/// <param name="text">Text to be formatted</param>
		/// <param name="outputDest">Output destination</param>
		/// <param name="graphics">Graphics Context</param>
		/// <param name="colbegin">Left hand x coordinate of column</param>
		/// <param name="colwidth">Width of column</param>
		/// <param name="justification">0 to left justify, 1 to center</param>
		/// <param name="buf">Buffer to format into</param>
		/// <returns>Number of lines filled</returns>

		public static int FormatTextField(
			string text,
			OutputDest outputDest,
			GraphicsMx graphics,
			int colbegin,
			int colwidth,
			int justification,
			string[] buf)
		{
			string s, txt;
			char c;
			int p, l, width, lineCount, i1;

			//			if (text==null || text=="") 
			//			{
			// 				buf[0]="";
			//				lineCount=1;
			//				return lineCount;
			//			}

			s = text.Replace('\t', ' '); // convert tabs to spaces

			p = 0;
			lineCount = 0;
			while (true)
			{

				width = 0;
				for (l = 0; p + l < s.Length; l++)
				{
					c = s[p + l];
					if (c == '\n' || c == '\r') break;
					width += graphics.CharWidth(c);
				}

				if (width <= colwidth) goto locatedBreak;

				// Backscan looking for a space, dash or comma break character

				while (true)
				{
					for (l = l - 1; l > 0; l--)
					{
						c = s[p + l - 1];
						if (c == ' ' || c == '-' || c == ',') break;
					}
					if (l == 0) break; // break out if no luck

					width = 0;
					int p2 = p + l - 1;
					if (p2 >= s.Length)
					{
						ClientLog.Message("p2 too big: " + p + " " + p2 + " " + s); // debug info
						p2 = s.Length - 1;
					}
					for (i1 = p; i1 <= p2; i1++)
						width += graphics.CharWidth(s[i1]);
					if (width <= colwidth) goto locatedBreak;
				}

				// break within a word

				l = 0;
				width = 0;
				while (true)
				{
					if (p + l >= s.Length) break;
					i1 = graphics.CharWidth(s[p + l]);
					if (l > 0 && width + i1 > colwidth) break;
					width += i1;
					l++;
				}

			locatedBreak:

				if (justification == 0) i1 = 0; // left justified
				else i1 = (colwidth - width) / 2; // center
				txt = s.Substring(p, l);
				p += l;
				while (p < s.Length - 1 && (s[p] == '\n' || s[p] == '\r')) p++;

				if (lineCount + 1 == buf.Length && p < s.Length) txt += "..."; // store elipsis if overflowing buffer
				if (colbegin < 0)
				{ // text only without coords
					buf[lineCount] = txt;
					lineCount++;
				}

				else if (outputDest == OutputDest.Html)
				{ // html, keep in single line with <br> inserted
					txt = ConvertMfSymbolsToHtml(txt); // special formula styling
					txt = ConvertGreekSymbolsToHtml(txt);

					if (lineCount == 0) buf[0] = txt;
					else buf[0] += "<br>" + txt;
					lineCount++;
				}

				else if (outputDest == OutputDest.WinForms)
				{ // store in a single line with newline inserted
					if (lineCount == 0) buf[0] = txt;
					//					else buf[0] += "\n" + txt;
					else buf[0] += "\v" + txt; // insert temporary vertical tabs line-break placeholders
					lineCount++;
				}

				else
				{
					buf[lineCount] = "t";
					buf[lineCount] += " " + (colbegin + i1).ToString() + " " + txt + "\t";
					lineCount++;
				}

				if (p >= s.Length)
				{
					break;
				}

				if (lineCount == buf.Length)
				{
					break;
				}

			} // end of text loop

			if (outputDest == OutputDest.WinForms && buf[0].Contains("\v"))
			{ // if grid then just use explicit line breaks
				buf[0] = text;
				if (buf[0].Contains("\r")) // remove returns
					buf[0] = buf[0].Replace("\r", "");

				if (buf[0].Contains("\n")) // replace newlines with vertical chars
					buf[0] = buf[0].Replace("\n", "\v");
			}

			return lineCount;
		}

		/// <summary>
		/// Get conditional formatting related cell style for a value
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="value"></param>
		/// <param name="rf"></param>
		/// <returns></returns>

		public static CellStyleMx GetCondFormatCellStyle(
			QueryColumn qc,
			object value,
			ResultsFormat rf)
		{
			return GetCondFormatCellStyle(qc, null, value, rf);
		}

		/// <summary>
		/// Get conditional formatting related cell style for a value
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="stats"></param>
		/// <param name="value"></param>
		/// <param name="rf"></param>
		/// <returns></returns>

		public static CellStyleMx GetCondFormatCellStyle(
			QueryColumn qc,
			ColumnStatistics stats,
			object value,
			ResultsFormat rf)
		{
			QualifiedNumber qn = null;
			MobiusDataType mdt = null;
			MoleculeMx structure = null, cs;
			CondFormatRule rule = null;
			CellStyleMx cellStyle = null;
			Color foreColor, backColor;
			Font f2;

			if (!SS.I.ShowConditionalFormatting) return null;
			if (rf.OutputDestination == OutputDest.TextFile || rf.OutputDestination == OutputDest.SdFile)
				return null;

			//if (Lex.Eq(qc.MetaColumn.Name, "calc_field")) qc = qc; // debug

			CondFormat condFormat = qc.ActiveCondFormat; // get any QueryColumn/MetaColumn level conditional formatting

			if (value is MobiusDataType)
				mdt = value as MobiusDataType;

			else // try to convert from non-MDT object
				mdt = MobiusDataType.New(qc.MetaColumn.DataType, value);

			if (mdt == null) return null;

			if (condFormat == null && qc.MetaColumn.MetaBrokerType != MetaBrokerType.Unknown)
			{ // get any conditional formatting specific to the underlying query column 
				condFormat = GetMetaColumnConditionalFormatting(qc.MetaColumn, mdt);
			}

			if (condFormat == null || condFormat.Rules == null || condFormat.Rules.Count == 0)
			{
				bool backColorDefined = mdt.BackColor != Color.White && mdt.BackColor != Color.Empty && mdt.BackColor != Color.Transparent;
				if (backColorDefined || mdt.ForeColor != Color.Black)
				{ // are colors available in qualified number
					f2 = new Font(rf.FontName, rf.FontSize, FontStyle.Regular);
					cellStyle = new CellStyleMx(f2, mdt.ForeColor, mdt.BackColor);
					return cellStyle;
				}
				else return null;
			}

			if (NullValue.IsNull(value))
				rule = CondFormatMatcher.MatchNull(condFormat);

			else if (qc.MetaColumn.DataType == MetaColumnType.Structure)
			// || (qc.MetaColumn.DataType == MetaColumnType.CompoundId && Rf.CombineStructureWithCompoundId))
			{
				//if (qc.MetaColumn.DataType == MetaColumnType.CompoundId && Rf.CombineStructureWithCompoundId)
				//  condFormat = Rf.CombinedStructureField.QueryColumn.CondFormat; // get struct formatting

				structure = (MoleculeMx)value;
				rule = CondFormatMatcher.Match(condFormat, structure);
				if (rule == null) return null; // have a match?

				if (condFormat.Option1) // highlight structure with matching rule
				{
					try
					{
						cs = StrMatcher.HighlightMatchingSubstructure(structure);
						structure.SetPrimaryTypeAndValue(cs.PrimaryFormat, cs.PrimaryValue); // copy to existing structure
					}
					catch (Exception ex) { ex = ex; }
				}

				if (condFormat.Option2) // orient structure to matching rule
				{
					try
					{
						cs = StrMatcher.AlignToMatchingSubstructure(structure);
						structure.SetPrimaryTypeAndValue(cs.PrimaryFormat, cs.PrimaryValue); // copy to existing structure
					}
					catch (Exception ex) { ex = ex; }
				}
			}

			else if (mdt is CompoundId) // match as string for now
				rule = MatchStringValue(condFormat, stats, (mdt as CompoundId).Value);

			else if (mdt is NumberMx)
				rule = CondFormatMatcher.Match(condFormat, (mdt as NumberMx).Value);

			else if (mdt is DateTimeMx)
				rule = CondFormatMatcher.Match(condFormat, (mdt as DateTimeMx).Value);

			else if (mdt is StringMx)
				rule = MatchStringValue(condFormat, stats, (mdt as StringMx).Value);

			else // do formatting on numbers, dates & strings stored as native types (really need this?)
			{
				if (mdt is QualifiedNumber) qn = value as QualifiedNumber;
				else if (!QualifiedNumber.TryConvert(value, out qn)) return null; // convert other formats into a qualified number

				if (qn.NumberValue != NullValue.NullNumber)
					rule = CondFormatMatcher.Match(condFormat, qn.NumberValue);

				else if (!String.IsNullOrEmpty(qn.TextValue))
					rule = MatchStringValue(condFormat, stats, qn.TextValue);

				else rule = CondFormatMatcher.MatchNull(condFormat);
			}

			if (rule == null) return null;

			FontStyle fontStyle = FontStyle.Regular; // set other cell style attributes
			if (rule.Font != null) fontStyle = rule.Font.Style;
			f2 = new Font(rf.FontName, rf.FontSize, fontStyle);
			cellStyle = new CellStyleMx(f2, rule.ForeColor, rule.BackColor1);
			return cellStyle;
		}

		/// <summary>
		/// Match a string value using the string matcher or continuous coloring
		/// </summary>
		/// <param name="condFormat"></param>
		/// <param name="stats"></param>
		/// <param name="stringVal"></param>
		/// <returns></returns>

		static CondFormatRule MatchStringValue(
			CondFormat condFormat,
			ColumnStatistics stats,
			string stringVal)
		{
			CondFormatRule rule = null;

			if (condFormat.Rules.ColoringStyle == CondFormatStyle.ColorSet || condFormat.Rules.Count != 2 ||
				stats == null || !stats.DistinctValueDict.ContainsKey(stringVal))
				rule = CondFormatMatcher.Match(condFormat, stringVal);

			else // convert to form so continuous coloring can be applied
			{
				condFormat.Rules[0].ValueNumber = 0;
				condFormat.Rules[1].ValueNumber = stats.DistinctValueList.Count - 1;
				double d = stats.DistinctValueDict[stringVal].Ordinal;
				rule = CondFormatMatcher.Match(condFormat, d);
			}

			return rule;
		}

		/// <summary>
		/// SetSpecialCompoundIdHilighting
		/// i.e. Color background for corp compound ids that have an associated change history
		/// </summary>
		/// <param name="mc"></param>
		/// <param name="ffi"></param>
		/// <param name="cellStyle"></param>

		void SetSpecialCompoundIdHilighting(
			MetaColumn mc,
			FormattedFieldInfo ffi,
			ref CellStyleMx cellStyle)
		{
			int corpIdInt;

			if (!SS.I.HilightCidChanges) return;

			if (mc.DataType != MetaColumnType.CompoundId) return;

			if (!CompoundId.IsCorpCompoundId(mc)) return;

			if (!int.TryParse(ffi.FormattedText, out corpIdInt)) return;

			CellStyleMx cs = new CellStyleMx();

			if (cellStyle != null && cellStyle.BackColor != cs.BackColor) return; // skip if nonstandard back color

			if (cellStyle == null) cellStyle = cs;
			cellStyle.BackColor = Color.LightGoldenrodYellow;
		}

		/// <summary>
		/// SetSpecialSubmissionIdHilighting
		/// i.e. Color background for Submission Ids that have an associated change history with different CorpIds
		/// </summary>
		/// <param name="mc"></param>
		/// <param name="ffi"></param>
		/// <param name="cellStyle"></param>

		void SetSpecialCompoundSubmissionIdHilighting(
			MetaColumn mc,
			FormattedFieldInfo ffi,
			ref CellStyleMx cellStyle)
		{
			int sbmsnId;

			if (!SS.I.HilightCidChanges) return;

			if (!int.TryParse(ffi.FormattedText, out sbmsnId)) return;

			CellStyleMx cs = new CellStyleMx();

			if (cellStyle != null && cellStyle.BackColor != cs.BackColor) return; // skip if nonstandard back color

			if (cellStyle == null) cellStyle = cs;
			cellStyle.BackColor = Color.LightGoldenrodYellow;
		}

		/// <summary>
		/// Format an image, retrieving as necessary
		/// </summary>
		/// <param name="metaColumn"></param>
		/// <param name="imageMx"></param>
		/// <param name="desiredWidth"></param>
		/// <param name="saveToFile"></param>
		/// <param name="imageFile"></param>
		/// <returns></returns>

		bool FormatGraphic(
			MetaColumn metaColumn,
			ImageMx imageMx,
			int desiredWidth,
			bool saveToFile,
			out string imageFile)
		{
			Bitmap bmp = null;

			imageFile = null;
			if (desiredWidth <= 0) return false;
			if (imageMx.Value != null)
			{
				bmp = imageMx.Value; // have image
			}

			else // call QueryEngine to retrieve image
			{
				if (String.IsNullOrEmpty(imageMx.DbLink)) return false;
				bmp = QueryEngine.GetImage(metaColumn, imageMx.DbLink, desiredWidth);
				if (bmp == null) return false;
				imageMx.Value = bmp;
			}

			bmp = BitmapUtil.ScaleBitmap(bmp, desiredWidth);
			imageMx.FormattedBitmap = bmp;

			if (saveToFile)
			{
				imageFile = ClientDirs.TempDir + @"\" + Guid.NewGuid().ToString() + ".bmp";
				bmp.Save(imageFile, ImageFormat.Bmp);
			}

			return true;
		}

		string ConvertTextToHtml(string txt) // convert .sub. . <sub>, etc.
		{
			string txt2;

			txt2 = txt;

			txt2 = txt2.Replace("&", "&amp"); // replace order here is important!
			txt2 = txt2.Replace("<", "&lt;");
			txt2 = txt2.Replace(">", "&gt;");

			return txt2;
		}

		public static string ConvertGreekSymbolsToHtml(
			string txt)
		{
			return txt; // todo
		}

		public static string ConvertMfSymbolsToHtml(
			string txt)
		{
			return txt; // todo
		}

		public static string SetHtmlPairs(
			string s,
			string m,
			string h1,
			string h2)
		{
			return s; // todo
		}
		/// <summary>
		/// TruncateTrailingZeros
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>

		string TruncateTrailingZeros(
			string txt)
		{
			int i1;

			if (txt.IndexOf(".") < 0) return txt;

			for (i1 = txt.Length; i1 > 2; i1--)
			{
				if (txt[i1 - 1] != '0') break;
				if (txt[i1 - 2] == '.') break; // keep one zero following decimal point
			}
			return txt.Substring(0, i1);
		}

		/// <summary>
		/// Format a qualified numeric field
		/// </summary>
		/// <param name="fieldValue"></param>
		/// <param name="decimals"></param>
		/// <param name="mergedField"></param>
		/// <returns></returns>

		string FormatQualifiedNumber(
			QualifiedNumber qn,
			ResultsField rfld,
			bool mergedField,
			ColumnFormatEnum displayFormat,
			int decimals,
			QnfEnum statFormat)
		{
			QueryColumn qc = rfld.QueryColumn;
			QnfEnum qnFormat = rfld.QualifiedNumberSplit;
			if (qnFormat == 0) qnFormat = QnfEnum.Combined; // default value if nothing assigned

			if ((qnFormat & QnfEnum.SubfieldMask) == 0) // if no subfields selected for field take field list from statFormat
				qnFormat |= (statFormat & QnfEnum.SubfieldMask);

			if ((qnFormat & QnfEnum.FormatMask) == 0) // if no formatting selected for field take formatting from statFormat
				qnFormat |= (statFormat & QnfEnum.FormatMask);

			return qn.Format(qc, mergedField, displayFormat, decimals, qnFormat, Rf.OutputDestination);
		}

		/// <summary>
		/// Get metacolumn specific conditional formatting 
		/// Todo: Remove this & specify in MetaTable def
		/// </summary>
		/// <param name="mc"></param>
		/// <param name="qn"></param>
		/// <returns></returns>

		public static CondFormat GetMetaColumnConditionalFormatting(
			MetaColumn mc,
			MobiusDataType mdt)
		{
			if (mc == null || mdt == null) return null;

			if (mc.MetaTable.MetaBrokerType != MetaBrokerType.RgroupDecomp)
				return null; // only for RgroupDecomp currently

			// For Rgroup analysis matrix: ResultId is of the form: queryId, mtName, mcName, sn1, sn2,...snn

			string[] sa = mdt.DbLink.Split(',');
			if (sa.Length < 3) return null;

			int objectId = Int32.Parse(sa[0]);

			Query q; // query that contain conditional formatting we need
			int t1 = TimeOfDay.Milliseconds(); // must be within 10 secs of last time so we get any query changes
			if (t1 - LastMCCFTime < 10000 && LastMCCFQuery != null &&
				LastMCCFQuery.UserObject.Id == objectId)
				q = LastMCCFQuery; // same query as last time
			else // read query slow way
			{
				UserObject uo = UserObjectDao.Read(objectId);
				if (uo == null) return null; // no longer there
				q = Query.Deserialize(uo.Content);
				q.UserObject.Id = objectId;
			}

			LastMCCFQuery = q;
			LastMCCFTime = t1;

			string mtName = sa[1];
			QueryTable qt = q.GetQueryTableByName(mtName);
			if (qt == null) return null;

			string mcName = sa[2];
			QueryColumn qc = qt.GetQueryColumnByName(mcName);
			if (qc == null) return null;

			CondFormat condFormat = (CondFormat)qc.CondFormat;
			return condFormat;
		}

	}

	/// <summary>
	/// Results of formatting a field 
	/// </summary>

	public class FormattedFieldInfo
	{
		public Color BackColor = Color.Empty;
		public Color ForeColor = Color.Black;
		public string FormattedText = null;
		public Bitmap FormattedBitmap = null;
		public string Hyperlink = "";
		public int HeightInLines = 0;

		/// <summary>
		/// Default constructor
		/// </summary>

		public FormattedFieldInfo()
		{
			return;
		}

		/// <summary>
		/// Construct with specified formatted text
		/// </summary>
		/// <param name="txt"></param>

		public FormattedFieldInfo(string txt)
		{
			FormattedText = txt;
		}

	} // FormattedFieldInfo

}