using System.Xml;
using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using QueryTable = Mobius.Data.QueryTable;

namespace Mobius.ClientComponents
{

	/// <summary>
	/// Methods that respond to clicks on Mobius data links
	/// </summary>

	public class ClickFunctions
	{
		public static QueryManager CurrentClickQueryManager; // QueryManager associated with the current click being processed
		private static Query _query;
		private static string _title, _mql, _cid;


		/// <summary>
		/// See if a ClickFunction command & process if so
		/// </summary>
		/// <param name="command"></param>
		/// <param name="qm"></param>
		/// <param name="cInf"></param>

		public static void Process(
			string command,
			QueryManager qm,
			CellInfo cInf = null)
		{
		  QueryTable rootQt, qt;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			Query q2;
			string dbName = "", mtName = "", mcName = "";
			List<string> args0, args;
			string funcName, arg1, arg2, arg3, arg4, arg5;
			string value = "", keyValue = "";

			int ai;

			try
			{
				// Parse click function arguments stripping all single quotes.
				// Arguments may be defined in the clickfunction definition including col values
				// indicated by field.Value in the metacolumn clickfunction definition.
				// If no args are defined in the clickfunction definition then a field value
				// argument will be added by default or the keyValue if [keyvalue] appears in the 
				// ClickFunction definition

				CurrentClickQueryManager = qm;
				args0 = Lex.ParseAllExcludingDelimiters(command, "( , )", false);
				args = new List<string>();
				for (ai = 0; ai < args0.Count; ai++) // strip all single quotes
				{
					string arg = args0[ai];
					if (arg.StartsWith("'"))
						arg = Lex.RemoveSingleQuotes(arg);

					//if (Lex.Eq(arg, "[rowcol]") && cInf!= null)
					//{ // pass grid row & col
					//  args.Add(cInf.GridRowHandle.ToString());
					//  args.Add(cInf.GridColAbsoluteIndex.ToString());
					//}
					//else

					args.Add(arg);
				}

				funcName = args[0];
				arg1 = (args.Count >= 2 ? args[1] : ""); // get other args
				arg2 = (args.Count >= 3 ? args[2] : "");
				arg3 = (args.Count >= 4 ? args[3] : "");
				arg4 = (args.Count >= 5 ? args[4] : "");
				arg5 = (args.Count >= 6 ? args[5] : "");

				if (Lex.Eq(funcName, "DisplayAllData"))
				{ // do all data display for supplied root table and key, i.e. DisplayAllData(TableName, KeyColName, KeyValue)

					ParseMetaTableMetaColumn(arg1, out mt, arg2, out mc);
					string extKey = arg3;
					string intKey = CompoundId.Normalize(extKey, mt);

					Progress.Show("Building Query...");
					_query = QueryEngine.GetSelectAllDataQuery(mt.Name, intKey);
					Progress.Show("Retrieving data..."); // put up progress dialog since this may take a while
					QbUtil.RunPopupQuery(_query, mt.KeyMetaColumn.Name + " " + extKey);
					Progress.Hide();
					return;
				}

				else if (Lex.Eq(funcName, "DisplayAllDataUsingDbName"))
				{ // display all data for supplied database synonym & key value, i.e. DisplayAllData2(DataBaseSynonym, KeyValue)
					mtName = null;
					dbName = arg1;
					RootTable rti = RootTable.GetFromTableLabel(dbName);
					if (rti != null) mtName = rti.MetaTableName;
					else // try synonyms
					{
						DictionaryMx dict = DictionaryMx.Get("Database_Synonyms");
						if (dict != null) mtName = dict.LookupDefinition(dbName);
					}

					if (String.IsNullOrEmpty(mtName))
					{
						MessageBoxMx.ShowError("Unrecognized database: " + dbName);
						return;
					}

					mt = MetaTableCollection.Get(mtName);
					if (mt == null)
					{
						MessageBoxMx.ShowError("Can't find key metatable " + mtName + " for database " + dbName);
						return;
					}

					string extKey = arg2;
					string intKey = CompoundId.Normalize(extKey, mt);

					Progress.Show("Building Query...");
					_query = QueryEngine.GetSelectAllDataQuery(mt.Name, intKey);
					Progress.Show("Retrieving data..."); // put up progress dialog since this may take a while
					QbUtil.RunPopupQuery(_query, mt.KeyMetaColumn.Name + " " + extKey);
					return;
				}

				// Run a query displaying results to a grid or web page and substituting a parameter value

				else if (Lex.Eq(funcName, "RunHtmlQuery") || Lex.Eq(funcName, "RunGridQuery"))
				{ // command to display to grid or html
					if (arg1.StartsWith("MetaTreeNode=", StringComparison.OrdinalIgnoreCase))
					{ // query based on metatables under a tree node
						string nodeName = arg1.Substring("MetaTreeNode=".Length).Trim();
						_cid = arg2;

						MetaTreeNode mtn = MetaTree.GetNode(nodeName);
						if (mtn == null)
						{
							MessageBoxMx.ShowError("Can't find tree node referenced in ClickFunction: " + nodeName);
							return;
						}

						_query = new Query();
						MetaTable rootMt = null;
						foreach (MetaTreeNode mtn_ in mtn.Nodes)
						{
							if (!mtn_.IsDataTableType) continue;
							mt = MetaTableCollection.Get(mtn_.Target);
							if (mt == null) continue;
							if (rootMt == null)
							{
								rootMt = mt.Root;
								rootQt = new QueryTable(_query, rootMt);
							}

							if (mt == rootMt) continue;
							qt = new QueryTable(_query, mt);
						}

						if (_query.Tables.Count == 0)
						{
							MessageBoxMx.ShowError("No valid data tables found: " + nodeName);
							return;
						}

						_query.KeyCriteria = "= " + _cid;
						_title = mtn.Label + " for " + _query.Tables[0].MetaTable.MetaColumns[0].Label + " " + CompoundId.Format(_cid);
					}

					else if (arg1.StartsWith("Query=", StringComparison.OrdinalIgnoreCase))
					{ // query based on saved query
						string qIdString = arg1.Substring("Query=".Length).Trim();
						if (qIdString.StartsWith("Query_", StringComparison.OrdinalIgnoreCase))
							qIdString = qIdString.Substring("Query_".Length).Trim();
						int qId = int.Parse(qIdString);

						_query = QbUtil.ReadQuery(qId);

						_cid = arg2;
						_query.KeyCriteria = "= " + _cid;
						_title = _query.UserObject.Name + " for " + _query.Tables[0].MetaTable.MetaColumns[0].Label + " " + CompoundId.Format(_cid);
					}

					else // explicit mql string to execute
					{

						_mql = arg1; // mql to execute
						if (Lex.IsUndefined(_mql)) throw new Exception("Expected MQL query not found: " + command);

						mt = null;
						mc = null;

						if (Lex.IsDefined(arg2) && Lex.IsDefined(arg3))
						{
							mtName = arg2;
							mcName = arg3;
							value = arg4; // value to plug in to mql
							keyValue = value;
							ParseMetaTableMetaColumn(arg2, out mt, arg3, out mc);
						}

						else if (cInf != null)
						{
							mt = cInf.Mt;
							mc = cInf.Mc;
							value = cInf?.DataValue?.ToString();
							keyValue = qm?.DataTableManager?.GetRowKey(cInf.DataRowIndex);
						}

						if (mt == null || mc == null) throw new Exception("Invalid MetaTable or MetaColumn name(s): " + command);

						if (!mc.IsNumeric) value = Lex.AddSingleQuotes(value); // quote if not numeric

						int i1 = _mql.ToLower().IndexOf("[value]"); // see if a value parameter
						if (i1 >= 0)
						{
							string value2 = value;
							_mql = _mql.Replace(_mql.Substring(i1, 7), value);
							_title = mc.Label + " " + value;
						}

						i1 = _mql.ToLower().IndexOf("[keyvalue]"); // see if a key value parameter
						if (i1 >= 0)
						{
							_mql = _mql.Replace(_mql.Substring(i1, 10), keyValue);
							_title = mt.KeyMetaColumn.Label + " " + keyValue;
						}

						try { _query = MqlUtil.ConvertMqlToQuery(_mql); }
						catch (Exception ex)
						{
							MessageBoxMx.ShowError("Error converting Mql to query: " + ex.Message);
							return;
						}
					}

					if (Lex.Eq(funcName, "RunHtmlQuery"))
						QbUtil.RunPopupQuery(_query, _title, OutputDest.Html);

					else // output to grid
						QbUtil.RunPopupQuery(_query, _title, OutputDest.WinForms);

					//else // create new grid query & run (will lose results for current query)
					//{
					//	QbUtil.NewQuery(_title); // show in query builder
					//	QbUtil.SetCurrentQueryInstance(_query);
					//	QbUtil.RenderQuery();
					//	string nextCommand = QueryExec.RunQuery(_query, OutputDest.Grid);
					//}

					return;
				}

				// Open a URL, normally substituting parameter value

				else if (Lex.Eq(funcName, "OpenUrl"))
				{
					string url = arg1; // url to execute
					value = arg2; // value to plug in to url

					int i1 = Lex.IndexOf(url, "[value]"); // fill in one of the value place holders
					if (i1 >= 0)
					{
						string value2 = value;
						url = url.Replace(url.Substring(i1, 7), value);
					}

					else // check to see if we are matching on key
					{
						i1 = Lex.IndexOf(url, "[keyvalue]");
						if (i1 >= 0)
						{
							url = url.Replace(url.Substring(i1, 10), value);
						}
					}

					SystemUtil.StartProcess(url);
					return;
				}

				else if (Lex.Eq(funcName, "OpenUrlFromSmallWorldCid"))
				{
					SmallWorldDepictions.OpenUrlFromSmallWorldCid(arg1);
					return;
				}

				else if (Lex.Eq(funcName, "ShowProjectDescription"))
				{
					string projName = arg1;
					QbUtil.ShowProjectDescription(projName);
					return;
				}

				else if (Lex.Eq(funcName, "ShowTableDescription"))
				{
					mtName = arg1;
					QbUtil.ShowTableDescription(mtName);
					return;
				}

				else if (Lex.Eq(funcName, "DisplayDrilldownDetail"))
				{ // drill down into a result value
					mtName = arg1; // table
					mcName = arg2; // column
					int level = Int32.Parse(arg3);
					string resultId = arg4; // quoted resultId to get
					q2 = QueryEngine.GetSummarizationDetailQuery(mtName, mcName, level, resultId);
					if (q2 == null) throw new Exception("Unable to build drill-down query for: " + mtName + "." + mcName);
					bool success = QbUtil.RunPopupQuery(q2, "Result Detail", OutputDest.WinForms);
					return;
				}

				//else if (Lex.Eq(funcName, "PopupSmilesStructure")) // display structure for a Smiles string (still needs some work...)
				//{
				//	string molString = arg1.ToString();
				//	ChemicalStructure cs = new ChemicalStructure(StructureFormat.Smiles, molString);
				//	ToolHelper.DisplayStructureInPopupGrid("Title...", "Smiles", "Structure", cs);
				//}

				//else if (Lex.Eq(funcName, "PopupChimeStructure")) // display structure for a Chime string
				//{
				//	string molString = arg1.ToString();
				//	ChemicalStructure cs = new ChemicalStructure(StructureFormat.Smiles, molString);
				//	ToolHelper.DisplayStructureInPopupGrid("Title...", "Smiles", "Structure", cs);
				//}

				else if (Lex.Eq(funcName, "DisplayWebPage"))
				{ // substitute a field value into a url & display associated web page
					string url = arg1;

					ParseMetaTableMetaColumn(arg2, out mt, arg3, out mc);
					value = arg4; // value to plug in to mql

					//				value = "{6E9C28EF-407E-44A0-9007-5FFB735A5C6C}"; // debug
					//				value = "{0AC17903-E551-445E-BFAA-860023D2884F}"; // debug
					//				value = "{63EE71F9-15BA-42FB-AFDC-C399103707B1}"; // debug
					//				value = "{80591183-B7BA-4669-8C5F-7E7F53D981CE}";

					//lex.OpenString(mc.ClickFunction); // reparse url to get proper case
					//funcName = lex.GetNonDelimiter();
					//url = Lex.RemoveAllQuotes(lex.GetNonDelimiter());

					_title = mc.Label + " " + value;
					int i1 = url.ToLower().IndexOf("[value]"); // fill in one of the value place holders
					if (i1 >= 0)
						url = url.Replace(url.Substring(i1, 7), value);

					else // check to see if we are matching on key
					{
						i1 = url.ToLower().IndexOf("[keyvalue]");
						if (i1 >= 0)
						{
							url = url.Replace(url.Substring(i1, 10), value);
							_title = mt.KeyMetaColumn.Label + " " + value;
						}
					}

					UIMisc.ShowHtmlPopupFormDocument(url, _title);
					return;
				}

				else if (Lex.Eq(funcName, "DisplayOracleBlobDocument")) // display a document contained in an Oracle blob column
				{ // Syntax: DisplayOracleBlobDocument(<table-to-lookup>, <value_match_column>, <file-name-or-type-col>, <content-column>)
					string table = arg1;
					string matchCol = arg2;
					string typeCol = arg3;
					string contentCol = arg4;
					string matchVal = arg5; // value to match

					try
					{
						string typeName;
						byte[] ba;
						UalUtil.SelectOracleBlob(table, matchCol, typeCol, contentCol, matchVal, out typeName, out ba);
						if (ba == null || ba.Length == 0) return;

						UIMisc.SaveAndOpenBinaryDocument(typeName, ba);
					}

					catch (Exception ex)
					{
						MessageBoxMx.ShowError("Error retrieving document: " + ex.Message);
						return;
					}

					return;
				}

				else if (Lex.Eq(funcName, "DisplayOracleClobDocument")) // display a document contained in an Oracle clob column
				{ // Syntax: DisplayOracleBlobDocument(<table-to-lookup>, <value_match_column>, <file-name-or-type-col>, <content-column>)
					string table = arg1;
					string matchCol = arg2;
					string typeCol = arg3;
					string contentCol = arg4;
					string matchVal = arg5; // value to match

					try
					{
						string typeName, clobString;
						UalUtil.SelectOracleClob(table, matchCol, typeCol, contentCol, matchVal, out typeName, out clobString);
						if (Lex.IsUndefined(clobString)) return;

						UIMisc.SaveAndOpenBase64BinaryStringDocument(typeName, clobString); // assume clob string is a Base64Binary string
					}

					catch (Exception ex)
					{
						MessageBoxMx.ShowError("Error retrieving document: " + ex.Message);
						return;
					}

					return;
				}

				else if (Plugins.IsMethodExtensionPoint(funcName))
				{
					List<object> objArgs = new List<object>();
					for (ai = 1; ai < args.Count; ai++) // build list of object arguments
						objArgs.Add(args[ai]);
					Plugins.CallStringExtensionPointMethod(funcName, objArgs);
				}

				else if (Lex.Eq(funcName, "None")) // dummy click function
					return;

				else
				{
					MessageBoxMx.ShowError("Unrecogized click function: " + funcName);
					return;
				}
			}

			catch (Exception ex)
			{
				Exception ex2 = ex;
				if (ex.InnerException != null) ex2 = ex.InnerException;
				string msg = "Error executing ClickFunction: " + command + "\r\n" +
					DebugLog.FormatExceptionMessage(ex);
				MessageBoxMx.ShowError(msg);

				ServicesLog.Message(msg);
			}
		}

		/// <summary>
		/// Parse metatable & metacolumn
		/// </summary>
		/// <param name="mtName"></param>
		/// <param name="mt"></param>
		/// <param name="mcName"></param>
		/// <param name="mc"></param>

		static void ParseMetaTableMetaColumn(
			string mtName,
			out MetaTable mt,
			string mcName,
			out MetaColumn mc)
		{
			mt = null;
			mc = null;

			mt = MetaTableCollection.Get(mtName);
			if (mt == null)
				throw new Exception("Can't find metatable: " + mtName);

			mc = mt.GetMetaColumnByName(mcName);
			if (mc == null)
				throw new Exception("Can't find metacolumn: " + mcName);

			return;
		}

		/// <summary>
		/// Get string value from vo
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="colName"></param>
		/// <param name="vo"></param>
		/// <returns></returns>

		public static string GetVoString(
			QueryTable qt,
			string colName,
			object[] vo)
		{
			QueryColumn qc = qt.GetQueryColumnByName(colName);
			object o = vo[qc.VoPosition + 1];
			if (o == null) return null;
			else return o.ToString();
		}

		/// <summary>
		/// Get int value from vo
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="colName"></param>
		/// <param name="vo"></param>
		/// <returns></returns>

		public static int GetVoInt(
			QueryTable qt,
			string colName,
			object[] vo)
		{
			QueryColumn qc = qt.GetQueryColumnByName(colName);
			object o = vo[qc.VoPosition + 1];
			if (o is int) return (int)o;
			else return NullValue.NullNumber;
		}

		/// <summary>
		/// Get date value from vo
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="colName"></param>
		/// <param name="vo"></param>
		/// <returns></returns>

		public static DateTime GetVoDate(
			QueryTable qt,
			string colName,
			object[] vo)
		{
			QueryColumn qc = qt.GetQueryColumnByName(colName);
			object o = vo[qc.VoPosition + 1];
			if (o is DateTime) return (DateTime)o;
			else return DateTime.MinValue;
		}

	}
}
