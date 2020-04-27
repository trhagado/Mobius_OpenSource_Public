using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Mobius.QueryEngineLibrary
{

	/// <summary>
	/// Handles brokering of calculated table queries which is a table of multiple calculated fields.
	/// </summary>

	public class CalcTableMetaBroker : GenericMetaBroker
	{
		int CfValColSelectListPos = -1, CfValColMetaTablePos = -1;

		CalcField _calcField; // associated CalcField
		OtherDataMap _dataMap; // map of CF input cols to cols retrieved by other tables in the query

		QueryTable _qt0; // Preclassification query table
		QueryTable _qt2; // Query table set up to point to post-classification metatable
		MetaTable _mt2; // MetaTable with post-classification column type

		/// <summary>
		/// Get the name of the SQL statement group that this table should be included in for the purposes of executing searches
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>
		public override string GetTableCriteriaGroupName(
			QueryTable qt)
		{
			return ""; // not known, could generate and examine sql
		}

		/// <summary>
		/// PrepareQuery
		/// </summary>
		/// <param name="eqp"></param>
		/// <returns></returns>
		public override string PrepareQuery(
			ExecuteQueryParms eqp)
		{
			Sql = BuildSql(eqp);

			return Sql;
		}

		/// <summary>
		/// Build the sql for the query
		/// </summary>
		/// <param name="eqp"></param>
		/// <returns></returns>

		public override string BuildSql(
			ExecuteQueryParms eqp)
		{
			string sql = "";

			Eqp = eqp;
			if (Eqp == null) DebugMx.DataException("Eqp parameter is null");

			Qt = eqp.QueryTable;
			if (Qt == null) DebugMx.DataException("Eqp.QueryTable is null");

			MetaTable mt = Qt.MetaTable;
			if (mt == null) DebugMx.DataException("Metatable not defined for CalcField Query Table");

			Query q = eqp.Qe.Query;
			QueryTableData[] qtd = eqp.Qe.Qtd; // query table data
			QueryColumn qc;
			MetaColumn mc;

			KeyMc = mt.KeyMetaColumn;
			if (KeyMc == null)
				throw new Exception("Key (compound number) column not found for MetaTable " + mt.Name);
			KeyQci = Qt.GetQueryColumnIndexByName(KeyMc.Name);
			KeyQc = Qt.QueryColumns[KeyQci];
			KeyQc.Selected = true; // be sure key is selected

			Qt.MetaTable.KeyMetaColumn.ColumnMap = ""; // reset key column mapping

			Query q2 = InitializeSubQuery(Qt);

			foreach (QueryTable qtx in q2.Tables)
			{
				// todo...
			}

			return sql;
		}

		/// <summary>
		/// Initialize subquery based on which columns are selected, have criteria, etc in the 
		/// top-level qt 
		/// and
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		Query InitializeSubQuery(
			QueryTable qt)
		{
			QueryColumn qc, qc2;

			Query q2 = GetUnderlyingTables(qt); // initialize subquery

			foreach (QueryColumn qc0 in qt.QueryColumns)
			{
				if (qc0.IsKey) continue;

				if (!qc0.Is_Selected_or_Criteria_or_GroupBy_or_Sorted)
					continue;

				string colMap = qc0.MetaColumn.ColumnMap;
				if (Lex.IsUndefined(colMap)) throw new Exception("ColumnMap not defined for: " + qc0.MetaTable.Name + "." + qc0.MetaColumnName);

				List<MqlToken> toks = MqlUtil.ParseComplexCriteria(colMap, q2);
				
				for (int ti = 0; ti < toks.Count; ti++)
				{
					MqlToken tok = toks[ti];
					qc = tok.Qc;
					if (qc != null) qc.Selected = true;
				}
			}

			List<QueryTable> qtList2 = new List<QueryTable>();
			foreach (QueryTable qt0 in q2.Tables)
			{
				if (qt0.SelectedCount > 1) qtList2.Add(qt0);
			}

			q2.Tables = qtList2;

			return q2;
		}


		/// <summary>
		/// Get list of Tables that underlie the CalcTable in the form of a prototype Query
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		Query GetUnderlyingTables(QueryTable qt)
		{
			QueryTable qt2;
			MetaTable mt2;
			string mtName, alias, tok;

			Query q2 = new Query();

			string[] l1 = qt.MetaTable.TableMap.Split(',');
			foreach(string s in l1)
			{
				tok = s.Trim();
				if (tok.Contains(" "))
				{
					string[] sa = tok.Split(' ');
					mtName = sa[0].Trim();
					alias = sa[1].Trim();
				}

				else
				{
					mtName = tok;
					alias = tok;
				}

				mt2 = MetaTableCollection.Get(mtName);
				if (mt2 == null)
					throw new Exception("Metatable not found: " + mtName);

				qt2 = new QueryTable(mt2);
				qt2.SelectKeyOnly();
				qt2.Alias = alias;
				q2.AddQueryTable(qt2);
			}

			return q2;
		}

		/// <summary>
		/// Create clone query and metatables that can be modified later if necessary
		/// </summary>

		QueryTable CloneQueryTableAndMetaTable(QueryTable qt)
		{
			QueryTable qt2 = qt.Clone();
			MetaTable mt2 = qt.MetaTable.Clone();
			qt2.MetaTable = mt2;
			foreach (QueryColumn qc2 in qt2.QueryColumns)
			{
				MetaColumn mc = qc2.MetaColumn;
				MetaColumn mc2 = mt2.GetMetaColumnByName(mc.Name); // get clone metacolumn
				qc2.MetaColumn = mc2;
			}

			return qt2;
		}

		/// <summary>
		/// Build sql to do full calculation
		/// </summary>
		/// <param name="cf"></param>
		/// <returns></returns>

		internal string BuildFullSql()
		{
			MetaTable mt;
			string advExpr, sql = "", tableAlias, outerMcName, innerMcExpr = "", keyExpr = "", valueExpr = "", exprs = "", tableSqlList = "", joinCriteria = "";
			string sourceTable = null, sourceColumn = null, tok;

			Dictionary<string, string> tableDict; // MetaTable name to alias
			Dictionary<string, Dictionary<string, MetaColumn>> tableMcDict; // MetaTable name to dict of cols for table referenced in calc field

			return "";
		}

		/// <summary>
		/// Execute query
		/// </summary>
		/// <param name="eqp"></param>

		public override void ExecuteQuery(
			ExecuteQueryParms eqp)
		{
			Eqp = eqp;
			Qt = eqp.QueryTable;
			MetaTable mt = Qt.MetaTable;
			if (string.IsNullOrEmpty(Sql)) return; // nothing to do here
			base.ExecuteQuery(eqp);
		}

		/// <summary>
		/// Return the next matching row value object
		/// </summary>
		/// <returns></returns>

		public override Object[] NextRow()
		{
			Object[] row;
			object v;
			QueryTable qt0;

			if (Lex.IsDefined(Sql))
			{
				while (true)
				{
						row = base.NextRow();

						if (row == null) return null;
						if (CfValColSelectListPos < 0) continue; // just return if calc field value not selected
						v = row[CfValColSelectListPos]; // calculated value
						if (NullValue.IsNull(v)) continue; // ignore row if value is null

						return row;
					}

			}

			else return null; // filled in later from other buffer values
		}

		/// <summary>
		/// Close broker & release resources
		/// </summary>

		public override void Close()
		{
			base.Close();
		}

		/// <summary>
		/// Return true if this table's results can be built from other retrieved data
		/// </summary>
		/// <param name="qe"></param>
		/// <param name="ti"></param>
		/// <returns></returns>

		public override bool CanBuildDataFromOtherRetrievedData(
			QueryEngine qe,
			int ti)
		{
			if (Qt == null || Lex.Ne(qe.Qtd[ti].Table.MetaTable.Name, Qt.MetaTable.Name))
				throw new Exception("Broker MetaTable name mismatch on table: " + qe.Qtd[ti].Table.MetaTable.Name);

			if (_dataMap == null) throw new Exception("DataMap not defined");

			return _dataMap.IsMappable;
		}

		/// <summary>
		/// Build a set of rows from data in the buffer
		/// </summary>

		public override void BuildDataFromOtherRetrievedData(
			QueryEngine qe,
			int ti,
			List<object[]> results,
			int firstResult,
			int resultCount)
		{

			// This routine takes a set of results in the buffer, computes the calculated field value
			// and stores the results back in the buffer. For a calc field derived from a single column
			// the process is simple; however, for two column calculations there are two cases to consider.
			// 1. If no values then nothing to compute.
			// 2. If both values are present in the buffer row then just use them.
			// 3. If a single value is present then pick up the most recent other value that
			//    exists for the same key.
			// Note that if the query only retrieves one of the two fields the the other field
			// will be retrieved into the calc field buffer.

			QueryColumn qc;
			MetaColumn mc;
			CalcFieldColumn cfc, cfc2;
			int voiCfKey, voiCf, voi;
			object v, v1, v2, vn;
			object[] vo, mergedVo;
			const int keyOffset = 1;

			throw new NotImplementedException();

#if false
			Query q = qe.Query;
			CalcFieldMetaTable cfMt = GetCalcFieldMetaTable(q.Tables[ti].MetaTable);
			CalcField cf = cfMt.CalcField;

			// Setup

			if (resultCount <= 0) return;
			if (SelectList.Count <= 1) return; // just return if only key selected

			voiCfKey = Qt.KeyQueryColumn.VoPosition; // calc field key value buffer index
			voiCf = Qt.GetQueryColumnByNameWithException("CALC_FIELD").VoPosition; // calculated value buffer index

			mergedVo = new object[results[0].Length]; // merged vo containing latest values for each column

			// Process each result row

			for (int ri = firstResult; ri < firstResult + resultCount; ri++) // result loop
			{
				vo = results[ri];
				if (vo[0] == null) continue;

				if (mergedVo[0] == null || Lex.Ne(vo[0].ToString(), mergedVo[0].ToString()))
				{ // if new key init voVals
					Array.Clear(mergedVo, 0, mergedVo.Length);
					mergedVo[0] = vo[0];
				}

				int nonNullCount = 0;
				int nullCount = 0;
				QueryColumn[] colMap = _dataMap.InputColMap;
				for (int mi = 0; mi < colMap.Length; mi++)
				{
					if (colMap[mi] == null) continue; // 
					voi = colMap[mi].VoPosition;
					v = vo[voi];
					if (!NullValue.IsNull(v))
					{
						mergedVo[voi] = v;
						nonNullCount++;
					}

					else nullCount++;
				}

				if (nonNullCount == 0) continue; // if all inputs are null then all done (assume all outputs are null also)

				vo[voiCfKey] = vo[0]; // copy key value

				QueryTableData qtd = qe.Qtd[ti];

				// Pick up each retrieved value in the from other data fields

				for (int cfci = 0; cfci < qtd.SelectedColumns.Count; cfci++) // columns to retrieve/calculate loop
				{
					qc = qtd.SelectedColumns[cfci];
					mc = qc.MetaColumn;
					voi = qc.VoPosition;

					bool calcFieldCol = Lex.Eq(mc.Name, "CALC_FIELD");
					bool selectedCol = !calcFieldCol;

					// Input value column

					if (selectedCol) // retrieved input value
					{
						QueryColumn cmi = _dataMap.SelectedColMap[cfci];
						if (cmi != null && cmi.VoPosition > 0)
						{
							v = mergedVo[cmi.VoPosition];
							if (mc.DataType == MetaColumnType.Image) // convert to proper image reference
							{
								MetaTable sourceMt = cmi.QueryTable.MetaTable;
								GenericMetaBroker gmb = QueryEngine.GetGlobalBroker(cmi.QueryTable.MetaTable);
								v = gmb.ConvertValueToImageReference(v);
							}

							vo[voi] = v;
						}
					}

					// Calculate the CF value

					else
					{
						cfc = cf.CfCols[0];
						vn = vo[voiCf]; // get possible single retrieved value
						vo[voiCf] = null; // clear calc value (may hold one of two values)

						v = mergedVo[colMap[0].VoPosition]; // get first value
						if (NullValue.IsNull(v)) continue; // if null then result must be null

						v = ApplyFunction(cfc, v);

						for (int mi = 1; mi < colMap.Length; mi++) // combine with 
						{
							v1 = v; // reference previous value
							if (colMap[mi] == null) continue;
							cfc2 = cf.CfCols[mi];
							voi = colMap[mi].VoPosition;
							v2 = mergedVo[voi];

							v2 = ApplyFunction(cfc2, v2);

							v = CalculateValue(cf, v1, cfc2, v2);
							if (v == null) break;
						}

						v = ApplyClassification(cf, v);

						vo[voiCf] = v; // store calculated value
					}

				} // column loop

				vo = vo; // debug to check row loop at end
			} // row loop

			return;
#endif
		}

		/// <summary>
		/// Pass drilldown along to first metacolumn in calc field for processing
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="mc"></param>
		/// <param name="level"></param>
		/// <param name="resultId"></param>
		/// <returns></returns>

		public override Query GetDrilldownDetailQuery(
			MetaTable mt,
			MetaColumn mc,
			int level,
			string linkInfoString)
		{
			bool summary;
			int tableId;

			MetaTable.ParseMetaTableName(mt.Name, out tableId, out summary);
			UserObject uo = UserObjectDao.Read(tableId);
			if (uo == null) return null;

			CalcField cf = CalcField.Deserialize(uo.Content);
			if (cf == null) return null;

			if (cf.MetaColumn1 == null || cf.MetaColumn1.MetaTable == null) return null;

			Query q = QueryEngine.GetDrilldownDetailQuery(cf.MetaColumn1.MetaTable, cf.MetaColumn1, level, linkInfoString);
			return q;
		}
	} // CalcTableMetaBroker

}
