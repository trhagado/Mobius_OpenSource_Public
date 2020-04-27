using Mobius.ComOps;
using Mobius.Data;

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
/// Build click functions and hyperlinks
/// </summary>

	public partial class ResultsFormatter
	{

/// <summary>
/// Format the hyperlink for a MobiusDataType object
/// </summary>
/// <param name="qc"></param>
/// <param name="mdt"></param>
/// <returns></returns>

		public string FormatHyperlink (
			CellInfo ci)
		{
			MobiusDataType mdt = ci.DataValue as MobiusDataType;
			int dri = ci.DataRowIndex;
			DataTableMx dt = Qm.DataTable;
			DataRowMx dr = null;
			if (dri >= 0 && dri < dt.Rows.Count)
				dr = dt.Rows[dri];
			FormattedFieldInfo ffi = FormatField(ci.Rt, ci.TableIndex, ci.Rfld, ci.FieldIndex, dr, dri, mdt, -1, false);
			string hyperlink = ffi.Hyperlink;
			return hyperlink;
		}

/// <summary>
/// Delegate instance for formatting hyperlink
/// </summary>
/// <param name="qc"></param>
/// <param name="mdt"></param>
/// <returns></returns>

		public string FormatHyperlink(
			QueryColumn qc,
			MobiusDataType mdt)
		{
			return FormatDetailsAvailableHyperlink(qc, mdt);
		}

/// <summary>
/// Format a hyperlink for a metacolumn where details are available
/// </summary>
/// <param name="qc"></param>
/// <param name="mdt"></param>
/// <returns></returns>

		public static string FormatDetailsAvailableHyperlink (
			QueryColumn qc,
			MobiusDataType mdt)
		{
			string hyperlink, uri = "";
			int drilldownLevel = 1;

			if (qc == null || qc.MetaColumn == null || qc.MetaColumn.MetaTable == null)
				return "";

			MetaColumn mc = qc.MetaColumn;
			MetaTable mt = mc.MetaTable;
			MetaColumnType mcType = mc.DataType;

			MetaBrokerType mbt = qc.MetaColumn.MetaTable.MetaBrokerType;

			// Annotation table broker

			if (mbt == MetaBrokerType.Annotation)
			{
				return mdt.Hyperlink; // just return already formatted hyperlink value
			}

// Target-Assay broker

			else if (mbt == MetaBrokerType.TargetAssay)
			{
				if (Lex.Eq(qc.MetaColumn.Name, "assy_nm"))
				{
					if (mdt.DbLink != "")
					{
						string[] sa = mdt.DbLink.Split(',');
						string mtName = sa[0].Trim() + "_" + sa[1].Trim();
						uri = "http://Mobius/command?" +
							"ClickFunction ShowTableDescription " + mtName;
					}
				}

				else if (qc.MetaColumn.DetailsAvailable)
				{
					if (Lex.Eq(qc.MetaColumn.MetaTable.Name, MultiDbAssayDataNames.CombinedNonSumTableName))
						drilldownLevel = 2; // level 2 goes from unsummarized unpivoted to warehouse details
					else drilldownLevel = 1; // level 1 goes from summarized to unsummarized (UNPIVOTED_ASSAY_RESULTS)

					uri = "http://Mobius/command?" +
						"ClickFunction DisplayDrilldownDetail " +
						qc.MetaColumn.MetaTable.Name + " " + qc.MetaColumn.Name +
						" " + drilldownLevel + " " + Lex.AddSingleQuotes(mdt.DbLink);
				}

			}

// Pivot broker

			else if (mbt == MetaBrokerType.Pivot)
			{
				string[] sa = mdt.DbLink.Split(',');
				if (sa.Length < 2 || sa[1] == "") return "";
				uri = sa[1];
				if (uri.ToLower().StartsWith("www.")) uri = "http://" + uri; // fix shortcut for proper linking
			}

// All other broker types

			else
			{
				int nValue = -1;
				if (mdt is QualifiedNumber) nValue = ((QualifiedNumber)mdt).NValue;
				if ((qc.QueryTable.MetaTable.UseSummarizedData && nValue >= 0) || // link if there is a non-null n-value associated with number
					qc.MetaColumn.DetailsAvailable) // or we explicitly know that details are available
				{
					uri = "http://Mobius/command?" +
						"ClickFunction DisplayDrilldownDetail " +
						qc.QueryTable.MetaTable.Name + " " + qc.MetaColumn.Name +
						(qc.QueryTable.MetaTable.UseSummarizedData ? " 1 " : " 2 ") + Lex.AddSingleQuotes(mdt.DbLink);
				}
			}

			return uri;
		}

		/// <summary>
		/// Build click function and field value text string for output
		/// ClickFunction arguments may be defined in the clickfunction definition including col values
		/// indicated by fieldName.Value in the metacolumn clickfunction definition.
		/// If no args are defined in the clickfunction definition then a field value
		/// argument will be added by default, the keyValue if [keyvalue] or
		/// [rowcol] as a grid row and column to be returned if these appear in the 
		/// ClickFunction definition.
		/// </summary>
		/// <param name="rf">Results field to display link for</param>
		/// <param name="vo">Vo contain tuple values</param>
		/// <param name="displayValue">Formatted display value for field</param>
		/// <returns></returns>

		public static string BuildClickFunctionText(
			ResultsField rf,
			DataRowMx dr,
			int dri,
			string displayValue)
		{
			ResultsTable rt;
			MetaColumn mc, mc2;
			MetaTable mt;
			string arg, arg2, argsString;
			int ai, rfi, voi;

			if (rf == null || dr == null || dri < 0) return "";

			rt = rf.ResultsTable;
			mc = rf.MetaColumn;
			mt = mc.MetaTable;
			object[] vo = dr.ItemArray;

			if (String.IsNullOrEmpty(mc.ClickFunction)) return "";

			List<string> args = Lex.ParseAllExcludingDelimiters(mc.ClickFunction, "( , )", false);
			string funcName = args[0]; // click function name

			int fieldRefs = 0;
			for (ai = 1; ai < args.Count; ai++)
			{ // see how many mcName.Value references there are
				arg = args[ai];
				string suffix = ".Value";
				if (!Lex.EndsWith(arg, suffix)) continue;

				arg = arg.Substring(0, arg.Length - suffix.Length);
				if (mt.GetMetaColumnByName(arg) != null)
					fieldRefs++;
			}

			if (fieldRefs == 0) // if no field references add either the field value, key value or grid row and column
			{
				if (Lex.Eq(funcName, "RunHtmlQuery") || Lex.Eq(funcName, "RunGridQuery") ||
					Lex.Eq(funcName, "DisplayWebPage")) // fixups for old functions
				{
					args.Add(Lex.AddSingleQuotes(mt.Name)); // add metatable name
					args.Add(Lex.AddSingleQuotes(mc.Name)); // add metacolumn name
				}

				if (Lex.Contains(mc.ClickFunction, "[TableColumnValue]")) // convert to metatable, metacolumns, &value refDataTable row & col
				{
					args.RemoveAt(1); // remove [TableColumnValue] arg
					args.Add(Lex.AddSingleQuotes(mt.Name));
					args.Add(Lex.AddSingleQuotes(mc.Name));
					args.Add(mc.Name + ".Value");
				}

				else if (Lex.Contains(mc.ClickFunction, "[keyvalue]"))
					args.Add(mt.KeyMetaColumn.Name + ".value"); // pass key value rather than this col value

				else args.Add(mc.Name + ".value"); // pass column value
			}

			argsString = "";
			for (ai = 1; ai < args.Count; ai++)
			{
				arg = args[ai];

				string suffix = ".Value";
				if (!arg.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) continue;
				arg2 = arg.Substring(0, arg.Length - suffix.Length);
				mc2 = mt.GetMetaColumnByName(arg2);

				if (mc2 == null) continue; // assume unquoted string constant & pass as is

				else if (mc2.IsKey && rt.Fields[0].MetaColumn.Name != mc2.Name)
				{ // see if undisplayed key value 
					voi = rt.Fields[0].VoPosition - 1; // position in vo
				}

				else // find the field name in the list of report fields
				{
					for (rfi = 0; rfi < rt.Fields.Count; rfi++)
					{
						if (Lex.Eq(mc2.Name, rt.Fields[rfi].MetaColumn.Name)) break;
					}

					if (rfi >= rt.Fields.Count)
						throw new Exception("Column name not selected in query: " + mc2.Name);
					voi = rt.Fields[rfi].VoPosition;
				}

				if (vo[voi] == null) args[ai] = "";
				else
				{
					arg = vo[voi].ToString();
					if (vo[voi] is MobiusDataType)
					{ // use dblink if defined
						MobiusDataType mdt = vo[voi] as MobiusDataType;
						if (!String.IsNullOrEmpty(mdt.DbLink))
							arg = mdt.DbLink;
					}

					args[ai] = arg;
				}
			}

			for (ai = 1; ai < args.Count; ai++)
			{
				arg = args[ai];
				if (!Lex.IsDouble(arg) && !arg.StartsWith("'") && !arg.StartsWith("[")) // quote if string
					arg = Lex.AddSingleQuotes(arg);
				if (argsString != "") argsString += ",";
				argsString += arg;
			}

			string txt = // build full string including link & display value
				"<a href=\"http:////Mobius/command?ClickFunction " +
				funcName + "(" + argsString + ")\">" + displayValue + "</a>";

			return txt;
		}

	} // ResultsFormatter
}
