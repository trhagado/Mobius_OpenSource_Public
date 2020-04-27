using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Drawing;

namespace Mobius.QueryEngineLibrary
{
	/// <summary>
	/// Generic pivoting broker
	/// </summary>
	public class PivotMetaBroker : GenericMetaBroker
	{
		public static int MultipivotExecuteQueryCount, MultipivotExecuteQueryTime, MultipivotExecuteQueryAvgTime; // performance data

		string[] PivotKeys = null; // catenated pivot values for each selected pivoted column
		int PivotedColCount = 0; // number of pivoted columns returned

		/// <summary>
		/// Constructor
		/// </summary>

		public PivotMetaBroker()
		{
			return;
		}

		/// <summary>
		/// Build the sql for a query doing preparation for separate fetch of data.
		/// </summary>
		/// <param name="parms"></param>

		public override string PrepareQuery(
			ExecuteQueryParms eqp)
		{
			MultiTablePivotBrokerTypeData mpd; // multipivot data for this broker type
			List<GenericMetaBroker> mbList;
			string tableFilterKey;

			Eqp = eqp;
			Qt = eqp.QueryTable;
			MetaTable mt = Qt.MetaTable;

			if (!eqp.ReturnQNsInFullDetail || // no multipivot if part of calc field
			eqp.Qe == null) // need to be able to access queryengine info
											//!QueryEngine.AllowMultiTablePivot) // is multipivot even allowed
				return base.PrepareQuery(eqp);

			PivotedColCount = 0;
			foreach (QueryColumn qc1 in Qt.QueryColumns)
			{ // if any criteria then pivot individually rather than via multipivot
				if (qc1.Criteria != "" && !qc1.IsKey) return base.PrepareQuery(eqp);
				if (qc1.MetaColumn.PivotValues != null) PivotedColCount++;
			}

			if (PivotedColCount == 0) return base.PrepareQuery(eqp); // must have at least one column to pivot

// Store pivot info for queryTable

			PivotInCode = true;
			BuildSql(eqp); // setup SelectList (don't really need sql)

			MpGroupKey = mt.TableMap + "," + Csv.JoinCsvString(mt.TableFilterColumns); // grouping based on source sql

			if (!QueryEngine.AllowMultiTablePivot)
				MpGroupKey += "_" + Qt.MetaTable.Name;

			if (eqp.Qe.MetaBrokerStateInfo == null)
				eqp.Qe.MetaBrokerStateInfo = new Dictionary<string, MultiTablePivotBrokerTypeData>();

			Dictionary<string, MultiTablePivotBrokerTypeData> mbsi = eqp.Qe.MetaBrokerStateInfo;
			if (!mbsi.ContainsKey(MpGroupKey))
			{ // create entry for data source
				mpd = new MultiTablePivotBrokerTypeData();
				mbsi[MpGroupKey] = mpd;
				mpd.FirstTableName = mt.Name;
				mpd.TableFilterValuesToMetaTableDict = new Dictionary<string, MetaTable>();
				mpd.TableCodeCsvList = new StringBuilder();
				mpd.TableCodeDict = new Dictionary<string, MpdResultTypeData>();
				mpd.MbInstances = new Dictionary<string, object>();
			}
			else mpd = (MultiTablePivotBrokerTypeData)mbsi[MpGroupKey]; // get existing entry

			string pms = Csv.JoinCsvString(mt.TableFilterValues); // values to identify this metatable

			mpd.AddMetaBroker(Qt.MetaTable.Name, this);

			// Store TableFilter values associated with metatable

			if (mt.TableFilterValues.Count == 1)
			{ // handle case with list of values allowed for single filter column (e.g. pivot values)
				List<string> sl = Csv.SplitCsvString(mt.TableFilterValues[0]);
				foreach (string s in sl)
					mpd.TableFilterValuesToMetaTableDict[s.ToLower()] = mt;
			}

			else
			{
				tableFilterKey = "";
				foreach (string s in mt.TableFilterValues)
				{
					if (tableFilterKey != "") tableFilterKey += ", ";
					tableFilterKey += s;
				}
				mpd.TableFilterValuesToMetaTableDict[tableFilterKey.ToLower()] = mt;
			}

#if false // todo: store codes for quick row identification
				if (!mpd.CodeHash.ContainsKey(Qt.MetaTable.Code)) // include assay code
				{
					mpd.CodeHash[pms] = null; // add key to hash list
					if (mpd.Codes.Length > 0) mpd.Codes.Append(",");
					mpd.Codes.Append(pms);
				}
#endif

			return null; // no sql generated here
		}

		/// <summary>
		/// Read in data, pivot & buffer for supplied set of rows.
		/// This is called for retrieval only, not for search
		/// </summary>
		/// <param name="eqp"></param>

		public override void ExecuteQuery(
			ExecuteQueryParms eqp)
		{
			DbCommandMx drd;
			int rowsFetched = 0, vosCreated = 0;
			MetaTable mt;
			MetaColumn mc = null;
			DateTime dt;
			PivotMetaBroker mb;
			List<GenericMetaBroker> mbList;

			string cid, pivotKey, tableFilter, s, txt, tok;
			int fci, mci, pvi, pci, si, i1;
			object[] vo = null;
			object o;

			if (!PivotInCode) // let Oracle do the pivoting?
			{
				base.ExecuteQuery(eqp);
				return;
			}

			// Self-pivot. Read & buffer data for all query tables from same Source/TableFilterColumns for key set if we are the first table for Source

			int t0 = TimeOfDay.Milliseconds();
			Dictionary<string, MultiTablePivotBrokerTypeData> mbsi = eqp.Qe.MetaBrokerStateInfo;
			mt = eqp.QueryTable.MetaTable;
			string sourceKey = mt.TableMap + "," + Csv.JoinCsvString(mt.TableFilterColumns); // grouping based on source sql
			MultiTablePivotBrokerTypeData mpd = (MultiTablePivotBrokerTypeData)mbsi[sourceKey];
			if (mpd.FirstTableName != mt.Name) return; // retrieve data for all tables when we see first table

			mpd.ClearBuffers();

			// Build sql

			StringBuilder sb = new StringBuilder(); // build filter to select for desired metatable
			tableFilter = "";
			if (mt.TableFilterColumns.Count == 1)
			{ // build single in list if single filter column
				foreach (string mtName in mpd.MbInstances.Keys)
				{
					mt = MetaTableCollection.Get(mtName);
					if (sb.Length > 0) sb.Append(",");
					sb.Append(mt.TableFilterValues[0]);
					tableFilter = mt.TableFilterColumns[0] + " in (" + sb.ToString() + ")";
				}
			}

			else // multiple table filter columns, build and/or expressions
			{
				foreach (string mtName in mpd.MbInstances.Keys)
				{
					mt = MetaTableCollection.Get(mtName);
					if (sb.Length > 0) sb.Append(" or ");
					tableFilter = "(" + GetTableFilterCriteria(mt) + ")";
					sb.Append(tableFilter);
				}
				tableFilter = "(" + sb.ToString() + ")";
			}

			string sql = "select * from " + mt.TableMap + " where ";
			if (tableFilter != "") sql += tableFilter + " and ";
			sql += mt.KeyMetaColumn.ColumnMap + " in (<list>) ";

			// Read unpivoted data, merge/pivot & buffer pivoted rows

			List<string> keySubset = eqp.SearchKeySubset;
			if (keySubset == null) keySubset = GetPreviewSubset(); // assume previewing of single table if no subset

			List<string> parmList = new List<string>();
			for (i1 = 0; i1 < keySubset.Count; i1++) // copy keys to parameter array properly normalized
			{
				string key = CompoundId.NormalizeForDatabase((string)keySubset[i1], Qt.MetaTable);
				if (key != null) parmList.Add(key);
			}

			drd = new DbCommandMx();
			drd.PrepareListReader(sql, DbType.String);
			drd.ExecuteListReader(parmList);
			while (drd.Read())
			{
				rowsFetched++;

				string tableFilterKey = ""; // get column values to identify table
				for (fci = 0; fci < mt.TableFilterColumns.Count; fci++)
				{
					o = drd.GetObjectByName(mt.TableFilterColumns[fci]);
					if (o == null) s = "";
					else s = o.ToString();
					if (tableFilterKey != "") tableFilterKey += ", ";
					tableFilterKey += s;
				}
				mt = mpd.TableFilterValuesToMetaTableDict[tableFilterKey];
				if (mt == null) continue; // continue if don't know about this table

				if (!mpd.MbInstances.ContainsKey(mt.Name)) continue; // have row hash for broker?

				int mbIdx = 0;
				mb = (PivotMetaBroker)mpd.GetFirstBroker(mt.Name, out mbList);

				while (true) // copy out for each metabroker
				{
					mt = mb.Qt.MetaTable;
					if (mt == null) continue;

					if (mb.MultipivotRowDict == null)
						mb.MultipivotRowDict = new Dictionary<string, object[]>();

					string rowKey = "";
					for (mci = 0; mci < mt.PivotMergeColumns.Count; mci++)
					{
						o = drd.GetObjectByName(mt.PivotMergeColumns[mci]);
						if (o == null) s = "<null>";
						else s = o.ToString();
						rowKey += "<" + s + ">";
					}

					if (mb.MultipivotRowDict.ContainsKey(rowKey)) // have entry for row?
						vo = (object[])mb.MultipivotRowDict[rowKey];

					else // new row, create vo for it & fill in merged column values
					{
						vo = new Object[mb.SelectList.Count];
						for (si = 0; si < mb.SelectList.Count; si++) // transfer non-pivoted values
						{
							mc = mb.SelectList[si];
							if (mc.PivotValues != null) continue; // skip pivoted cols for now
							for (mci = 0; mci < mt.PivotMergeColumns.Count; mci++)
							{
								if (Lex.Eq(mc.ColumnMap, mt.PivotMergeColumns[mci]))
								{
									o = drd.GetObjectByName(mt.PivotMergeColumns[mci]);
									if (mc.IsKey) // normalize cid adding prefix as needed
										o = CompoundId.Normalize(o.ToString(), mt);
									vo[si] = o;
									break;
								}
							}
						}

						mb.MultipivotRowDict[rowKey] = vo;
						vosCreated++;
					}

					// Pivot out data based on pivot column values

					if (mb.PivotKeys == null)
					{ // build set of pivot keys for the pivoted columns in the table if not done yet
						mb.PivotKeys = new string[mb.SelectList.Count];
						for (si = 0; si < mb.SelectList.Count; si++)
						{
							mc = mb.SelectList[si];
							if (mc.PivotValues == null) continue; // skip non-pivoted cols
							pivotKey = "";
							for (pvi = 0; pvi < mc.PivotValues.Count; pvi++)
								pivotKey += "<" + mc.PivotValues[pvi].ToLower() + ">";
							mb.PivotKeys[si] = pivotKey;
						}
					}

					pivotKey = "";
					for (pci = 0; pci < mt.PivotColumns.Count; pci++)
					{ // build pivot key for this unpivoted row
						o = drd.GetObjectByName(mt.PivotColumns[pci]);
						if (o == null) s = "<null>";
						else s = o.ToString().ToLower();
						pivotKey += "<" + s + ">";
					}

					for (si = 0; si < mb.SelectList.Count; si++) // transfer pivoted values
					{
						if (mb.PivotKeys[si] == null || // skip non-pivoted cols
						pivotKey != mb.PivotKeys[si]) continue; // and non-matches
						mc = mb.SelectList[si];
						int ci = drd.Rdr.GetOrdinal(mc.ColumnMap);

						if (mc.DataType == MetaColumnType.Integer)
						{
							if (!mc.DetailsAvailable) // simple scalar value
								vo[si] = drd.GetInt(ci);

							else // value with possible resultId, linking information
							{
								txt = drd.GetString(ci); // todo: fix for annotation
								vo[si] = QueryEngine.ParseScalarValue(txt, Qt, mc);
							}
						}

						else if (mc.DataType == MetaColumnType.Number)
						{
							if (!mc.DetailsAvailable) // simple scalar value
								vo[si] = drd.GetDouble(ci);

							else // value with possible resultId, linking information
							{
								txt = Dr.GetString(ci); // todo: fix for annotation
								vo[si] = QueryEngine.ParseScalarValue(txt, Qt, mc);
							}
						}

						else if (mc.DataType == MetaColumnType.QualifiedNo)
						{
							// todo
						}

						else if (mc.DataType == MetaColumnType.String)
						{
							if (!mc.DetailsAvailable)
								vo[si] = drd.GetString(ci);

							else // value with possible resultId, linking information
							{
								txt = Dr.GetString(ci); // todo: fix for annotation
								vo[si] = QueryEngine.ParseScalarValue(txt, Qt, mc);
							}
						}

						else if (mc.DataType == MetaColumnType.Date)
						{
							if (!mc.DetailsAvailable) // simple scalar value
								vo[si] = drd.GetDateTime(ci);

							else // value with possible resultId, linking information
							{
								txt = Dr.GetString(ci); // todo: fix for annotation
								vo[si] = QueryEngine.ParseScalarValue(txt, Qt, mc);
							}
						}

						else if (mc.DataType == MetaColumnType.Structure)
						{ // structures come in as compound identifiers (todo: fix for annotation)
							tok = Dr.GetValue(si).ToString();
							cid = CompoundId.Normalize(tok, Qt.MetaTable);
							vo[si] = cid;
						}

						else if (mc.DataType == MetaColumnType.MolFormula)
							vo[si] = drd.GetString(ci);

						else if (mc.DataType == MetaColumnType.DictionaryId)
							try // Id may be string or integer value
							{ vo[si] = drd.GetString(ci); }
							catch (Exception ex)
							{ vo[si] = drd.GetInt(ci); }

						else if (mc.DataType == MetaColumnType.Image)
						{
							try // Id may be string or integer value
							{ vo[si] = drd.GetString(ci); }
							catch (Exception ex)
							{ vo[si] = drd.GetInt(ci); }
						}

						else if (mc.DataType == MetaColumnType.Hyperlink)
						{
							txt = drd.GetString(ci);
							Hyperlink hlink = new Hyperlink();
							vo[si] = hlink;
							hlink.Text = txt;
						}
					}
					if (mbList == null) break; // single broker
					mbIdx++; // go to next broker
					if (mbIdx >= mbList.Count) break; // at end of brokers?
					mb = (PivotMetaBroker)mbList[mbIdx];
				} // end of broker loop
			} // end of read loop

			drd.Dispose();
			return;
		}

		/// <summary>
		/// Get next row of data from buffer
		/// </summary>
		/// <returns></returns>

		public override Object[] NextRow()
		{
			if (!PivotInCode) // if not multipivot then call generic broker 
				return base.NextRow();

			object[] vo = GetNextMultipivotRowDictVo();
			return vo;
		}

		/// <summary>
		/// Generate the sql for retrieval from a querytable
		/// If this query returns qualified numbers (i.e. is not search part of query or part of calc field)
		/// then don't retrieve any data other than the compound id & group id.
		/// </summary>
		/// <param name="qt"></param>

		public override string BuildSql(
			ExecuteQueryParms eqp)
		{
			List<MetaColumn> mcl;
			List<QueryColumn> qcl;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			Hashtable resultKeys;
			string tableAlias, txt, tok;
			string pivotExprs, pivotTables = "", pivotCriteria;
			int ci, qci, i1;

			Eqp = eqp;
			Qt = eqp.QueryTable;

			Exprs = FromClause = OrderBy = ""; // outer sql elements

			qcl = Qt.QueryColumns;
			mt = Qt.MetaTable;
			mcl = mt.MetaColumns;
			KeyMc = mt.KeyMetaColumn;
			resultKeys = new Hashtable();
			SelectList = new List<MetaColumn>(); // list of selected metacolumns

			int selectCnt = 0; // counting key
			int criteriaCnt = 0; // not counting key

			QueryColumn firstCriteriaPivotedCol = null; // first pivoted column with criteria
			QueryColumn firstSelectedPivotedCol = null; // first pivoted column that's selected

			StringBuilder pivotValueList = new StringBuilder(); // build list of selected result codes

			for (qci = 0; qci < qcl.Count; qci++)
			{
				qc = qcl[qci];
				mc = qc.MetaColumn;

				if (mc.PivotValues != null && mc.PivotValues.Count == 1 && (qc.Is_Selected_or_Criteria_or_GroupBy_or_Sorted))
				{
					if (pivotValueList.Length > 0) pivotValueList.Append(",");
					pivotValueList.Append(mc.PivotValues[0]);
				}

				if (qc.Is_Selected_or_GroupBy_or_Sorted)
				{
					selectCnt++;
					if (mc.PivotValues != null && firstSelectedPivotedCol == null &&
						!eqp.ReturnQNsInFullDetail) // don't count if getting qualified numbers since handled differently
						firstSelectedPivotedCol = qc;
				}

				if (qc.Criteria != "")
				{
					criteriaCnt++;
					if (mc.PivotValues != null && firstCriteriaPivotedCol == null)
						firstCriteriaPivotedCol = qc;
				}
			}

			if ((eqp.Qe.MqlLogicType != QueryLogicType.And && criteriaCnt > 1))
				firstSelectedPivotedCol = firstCriteriaPivotedCol = null; // don't use this optimization unless "and" logic

			// Add key column to selected list if not already selected

			qc = Qt.KeyQueryColumn;
			if (qc == null)
				throw new Exception("Key column not found for MetaTable " + mt.Name);

			if (!qc.Selected) // put key at start of list if not already selected
			{
				SelectList.Add(mt.KeyMetaColumn);
				selectCnt++;
			}

			////////////////////////////////////////////////////////////////////////////////
			// Build sql
			////////////////////////////////////////////////////////////////////////////////
			//
			// An inner query in the from clause pivots out the columns that are
			// selected or have criteria
			//
			////////////////////////////////////////////////////////////////////////////////

			pivotExprs = // key field with native col name mapped to mc name 
				"t0." + mt.KeyMetaColumn.ColumnMap + " " + mt.KeyMetaColumn.Name; // key field with native col name mapped to mc name 

			pivotCriteria = "";
			int pivotCount = 0;

			QueryColumn t0Col = null; // column assigned to t0

			if (firstCriteriaPivotedCol != null)
				t0Col = firstCriteriaPivotedCol; // if there is a pivoted column with criteria assign to t0

			else if (selectCnt == 2 && firstSelectedPivotedCol != null)
				t0Col = firstSelectedPivotedCol; // single selected pivoted col assigned to t0

			else
			{ // multiple (or zero) selected pivoted cols, build "select unique" to join all pivoted values to
				pivotTables = "(select unique ";
				for (int mci = 0; mci < mt.PivotMergeColumns.Count; mci++)
				{
					if (mci > 0) pivotTables += ", ";
					pivotTables += mt.PivotMergeColumns[mci];
				}

				pivotTables += " from " + mt.TableMap + " ";

				string tableSelectionExpr = GetTableFilterCriteria(mt);

				if (pivotValueList.Length > 0) // if we have any pivot value then limit to those
				{
					if (tableSelectionExpr != "") tableSelectionExpr += " and ";
					tableSelectionExpr += mt.PivotColumns[0] + " in (" + pivotValueList + ")";
				}
				pivotTables += " where " + tableSelectionExpr + ") t0 ";
			}

			// Process query column objects

			for (qci = 0; qci < qcl.Count; qci++)
			{
				qc = qcl[qci];
				mc = qc.MetaColumn;

				// Pivot out the result

				if (!qc.Is_Selected_or_Criteria_or_GroupBy_or_Sorted) continue;

				if (mt.KeyMetaColumn == mc) ; // already have key column

				else if (mc.PivotValues != null) // pivot out of results table
				{
					if (qc == t0Col) tableAlias = "t0"; // col to be used as t0
					else
					{
						pivotCount++;
						tableAlias = "t" + (pivotCount).ToString();
					}

					pivotExprs += ", "; // add one or more expressions for this column
					string colName = tableAlias + "." + mc.ColumnMap; // name for this col qualified by table alias
					if (Eqp.ReturnQNsInFullDetail && // return in qn format?
						(mc.DataType == MetaColumnType.QualifiedNo || mc.DetailsAvailable))
					{
						if (qc.Is_Selected_or_GroupBy_or_Sorted)
						{ // build expression to retrieve catenated col value & linking info
							string colExpr = colName;
							if (mc.DataType == MetaColumnType.Date) // return normalized, sortable date string (annotation tables)
								colExpr = "to_char(" + colName +
									",'YYYYMMDD HH24MISS')";

							string qnExpr =
							 AppendQnElement(mt.QnQualifier) + " chr(11) || " +
							 AppendQnElement(mt.QnNumberValue) + " chr(11) || " +
							 AppendQnElement(mt.QnTextValue) + " chr(11) || " +
							 AppendQnElement(mt.QnNValue) + " chr(11) || " +
							 AppendQnElement(mt.QnNValueTested) + " chr(11) || " +
							 AppendQnElement(mt.QnStandardDeviation) + " chr(11) || " +
							 AppendQnElement(mt.QnStandardError) + " chr(11) || " +
							 AppendQnElement(mt.QnLinkValue);

							qnExpr = qnExpr.Replace("<TA>", tableAlias);

							pivotExprs += qnExpr + " " + mc.Name + ", ";
						}

						pivotExprs += // native column value for comparison
							colName + " " + mc.Name + "_val ";
					}

					else // not a qualified number or not returning qualified numbers
					{
						//if (qc.Criteria.ToLower().EndsWith(" is not null")) // if is-not-null criteria then any value will do not just number
						//pivotExprs += tableAlias + ".assay_rslt_typ_id " + 	mc.Name + "_val "; // todo: fix to work with complex criteria
						//else

						pivotExprs += // just return native value
							colName + " " + mc.Name + ", ";
						pivotExprs += // native column value for comparison
							colName + " " + mc.Name + "_val ";
					}

					if (pivotTables != "") pivotTables += ", ";
					pivotTables += GetSourceWithTableFilterCriteria(mt) + " " + tableAlias + " ";

					txt = "";
					for (int pci = 0; pci < mt.PivotColumns.Count; pci++)
					{ // filter unpivoted source for just this column
						if (txt != "") txt += " and ";
						txt += "<TA>." + mt.PivotColumns[pci] + " (+) = " + mc.PivotValues[pci] + " ";
					}

					if (qc != t0Col)  // joint to t0 on merge columns unless we are t0
					{
						foreach (string mergeCol in mt.PivotMergeColumns)
							txt += "and <TA>." + mergeCol + " (+) = t0." + mergeCol + " ";
					}

					txt = txt.Replace("<TA>", tableAlias); // plug in proper table alias
					if (qc.Criteria != "" || selectCnt <= 2) txt = txt.Replace("(+)", ""); // remove outer join if criteria on this column
					if (pivotCriteria != "") pivotCriteria += " and ";
					pivotCriteria += txt + " ";
				}

				else // nulls for other non-result fields
				{
					pivotExprs += ", null " + mc.Name + " ";
				}

				// Add to select list if selected

				if (qc.Is_Selected_or_GroupBy_or_Sorted) // if selected add to expression list
				{
					if (mt.KeyMetaColumn == mc) // save index of compound id value object
						KeyVoi = SelectList.Count;

					if (Exprs.Length > 0) Exprs += ",";

					Exprs += mc.Name;

					SelectList.Add(mc);
				}
			} // end of column loop

			FromClause = "(select " + pivotExprs + " " +
				"from " + pivotTables;
			if (pivotCriteria != "") FromClause += " where " + pivotCriteria;
			FromClause += ")";

			Sql = "select /*+ first_rows */ " + Exprs +
			 " from " + FromClause;

			if (Qt.Alias != "") Sql += " " + Qt.Alias;
			if (eqp.CallerSuppliedCriteria != "")
				Sql += " where " + eqp.CallerSuppliedCriteria;

			return Sql;
		}

		/// <summary>
		/// Get source sql with table filter criteria included
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		string GetSourceWithTableFilterCriteria(
			MetaTable mt)
		{
			string filterCriteria = GetTableFilterCriteria(mt);
			string sql = mt.TableMap.Trim();
			if (sql.ToLower().StartsWith("select  "))
				sql = "(" + sql + ")";
			if (!sql.ToLower().StartsWith("(select ")) // just table name?
				sql = "(select * from " + sql + ")"; // if so build select within parens
			sql = sql.Substring(0, sql.Length - 1); // remove trailing paren
			int wi = sql.ToLower().IndexOf(" where ");
			if (wi < 0) sql += " where "; // need where clause
			else // put exising criteria within parens
				sql = sql.Substring(0, wi + 7) + "(" + sql.Substring(wi + 7) + " and ";
			sql += filterCriteria + ")";
			return sql;
		}

		/// <summary>
		/// Get Oracle criteria for filtering for a single table
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		string GetTableFilterCriteria(
			MetaTable mt)
		{
			string criteria = "";
			if (mt.TableFilterColumns.Count == 1 && mt.TableFilterValues[0].IndexOf(",") > 0) // single filter col with multiple allowed values
				criteria = mt.TableFilterColumns[0] + " in (" + mt.TableFilterValues[0] + ")";

			else
			{
				for (int fci = 0; fci < mt.TableFilterColumns.Count; fci++)
				{ // filter unpivoted source for just this metatable
					if (criteria != "") criteria += " and ";
					criteria += mt.TableFilterColumns[fci] + " = " + mt.TableFilterValues[fci];
				}
			}

			return criteria;
		}

		/// <summary>
		/// Append a Qualified Number expression element
		/// </summary>
		/// <param name="elementExpr"></param>
		/// <returns></returns>

		string AppendQnElement(
			string element)
		{
			if (element == "") return "";
			if (element.IndexOf("<TA>.") < 0) // prepend table alias if not already present
				element = "<TA>." + element;
			return element + " chr(11) || ";
		}

		/// <summary>
		/// Read a sample of a table for previewing
		/// </summary>
		/// <returns></returns>

		List<string> GetPreviewSubset()
		{
			Dictionary<string, object> PreviewSubset = new Dictionary<string, object>();
			MetaTable mt = Eqp.QueryTable.MetaTable;
			string sql = "select /*+ first_rows */" + mt.KeyMetaColumn.ColumnMap + " from " +
				GetSourceWithTableFilterCriteria(mt);
			DbCommandMx drd = new DbCommandMx();
			drd.Prepare(sql);
			drd.ExecuteReader();
			while (drd.Read())
			{
				if (drd.IsNull(0)) continue;
				string id = drd.GetObject(0).ToString();
				string cid = CompoundId.Normalize(id); // normalize cid adding prefix as needed
				PreviewSubset[id] = null;
				if (PreviewSubset.Count >= 100) break;
			}
			drd.Dispose();

			return new List<string>(PreviewSubset.Keys);
		}

		/// <summary>
		/// Convert a multipivot table into a set of tables where data exists for
		/// one or more of the compound identifiers in the list.
		/// </summary>
		/// <param name="qt">Current form of query table</param>
		/// <param name="q">Query to add transformed tables to</param>
		/// <param name="ResultKeys">Keys data will be retrieved for</param>

		public override void ExpandToMultipleTables(
			QueryTable qt,
			Query q,
			List<string> resultKeys)
		{
			MetaTable mt2;
			QueryTable qt2;
			string sql;
			int methodId, i1;

			int t0 = TimeOfDay.Milliseconds();

			List<string> normalizedResultKeys = new List<string>();
			for (i1 = 0; i1 < resultKeys.Count; i1++) // copy keys to parameter array properly normalized
			{
				string key = CompoundId.NormalizeForDatabase(resultKeys[i1], qt.MetaTable);
				if (key == null) key = NullValue.NullNumber.ToString(); // if fails supply a "null" numeric value
				normalizedResultKeys.Add(key);
			}

			sql = // todo: Make to work in general case (PubChem only now)
				"select mthd_vrsn_id " +
				"from " + "mbs_owner.mbs_pbchm_rslt" + " " +
				"where ext_cmpnd_id_nbr in (<list>) " +
				" and sts_id = 1 " + // active records only
				"group by mthd_vrsn_id";

			DbCommandMx drd = new DbCommandMx();
			drd.PrepareListReader(sql, DbType.Int32);
			drd.ExecuteListReader(normalizedResultKeys);
			if (drd.Cancelled)
			{
				// todo qe.Cancelled = true;
				drd.Dispose();
				return;
			}

			Hashtable mtHash = new Hashtable();
			int methodIdCount = 0;
			while (true) // convert list of methods to set of metatable names
			{
				if (!drd.ListRead()) break;
				methodId = drd.GetInt(0);
				string mtName = "pubchem_aid_" + methodId.ToString(); // todo: Make to work in general case (PubChem only now)
				if (QueryEngine.FilterAllDataQueriesByDatabaseContents &&
					!MetaTableCollection.IsMetaTableInContents(mtName)) continue; // metatable must be in contents

				if (qt.MetaTable.UseSummarizedData) mtName += MetaTable.SummarySuffix;
				mtHash[mtName] = null;
				methodIdCount++;
			}

			drd.Dispose();
			if (drd.Cancelled)
			{
				// todo qe.Cancelled = true;
				return;
			}

			ArrayList mtList = new ArrayList();
			foreach (string mtName2 in mtHash.Keys)
			{ // put metatable labels & names into a list for sorting
				mt2 = MetaTableCollection.Get(mtName2);
				if (mt2 == null) continue;
				if (mt2.Parent == null) continue; // skip if no parent
				mtList.Add(mt2.Label.ToLower().PadRight(64) + "\t" + mt2.Name);
			}

			mtList.Sort();

			foreach (string mts in mtList)
			{ // add new querytables/metatables to query
				string[] sa = mts.Split('\t');
				mt2 = MetaTableCollection.Get(sa[1]);
				if (mt2 == null) continue;
				qt2 = new QueryTable(q, mt2);
				if (qt.HeaderBackgroundColor != Color.Empty)
					qt2.HeaderBackgroundColor = qt.HeaderBackgroundColor;
			}

			t0 = TimeOfDay.Milliseconds() - t0;
			return;
		}
	}
}
