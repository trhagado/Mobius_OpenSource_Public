using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Mobius.QueryEngineLibrary
{
    /* Richard E Rathmell problem calc field 11/17/11 calcfield_275779
	<CalcField CalcType="Basic" SourceColumnType="QualifiedNo" Table1="ASSAY_5184" Column1="R_3" Function1="-Log molar concentration" Operation="/ (Division)" Table2="IRW_202" Column2="R_242" Function2="None"  />
	 */

    /// <summary>
    /// Handles brokering of calculated field queries. For simple calculated fields the generic broker provides the 
    /// necessary services. If this field is mapped to class names then this broker does the mapping. 
    /// If the retrieved data is selected from the buffer for other data tables in the query then 
    /// the broker retrieves the data and calculates the value.
    /// </summary>

    public class CalcFieldMetaBroker : GenericMetaBroker
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

            string hint = "/*+ first_rows */";

            Sql = Lex.Replace(Sql, "/*+ hint */", hint);

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
            string sql;

            Eqp = eqp;
            AssertMx.IsNotNull(Eqp, "Eqp parameter is null");

            Qt = eqp.QueryTable;
            AssertMx.IsNotNull(Qt, "Eqp.QueryTable is null");

            CalcFieldMetaTable mt = GetCalcFieldMetaTable(Qt.MetaTable);
            if (mt == null) DebugMx.DataException("Metatable not defined for CalcField Query Table");

            CalcField cf = mt.CalcField;
            if (cf == null) DebugMx.DataException("Calculated field not defined for MetaTable: " + mt.Name);

            _calcField = cf;
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

            _qt2 = CloneQueryTableAndMetaTable(Qt);
            _mt2 = _qt2.MetaTable;

            CfValColSelectListPos = CfValColMetaTablePos = -1;

            SelectList = new List<MetaColumn>(); // list of selected metacolumns gets built here
            for (int qci = 0; qci < Qt.QueryColumns.Count; qci++)
            {
                qc = Qt.QueryColumns[qci];
                if (qc.MetaColumn == null) continue;

                mc = qc.MetaColumn;
                if (Lex.Eq(mc.Name, "CALC_FIELD"))
                    CfValColMetaTablePos = mc.GetPositionInMetaTable();

                if (qc.Is_Selected_or_GroupBy_or_Sorted)
                {
                    if (Lex.Eq(mc.Name, "CALC_FIELD")) CfValColSelectListPos = SelectList.Count;
                    SelectList.Add(mc);
                }
            }

            _dataMap = MapInputColsToOtherRetrievedData(cf, q, qtd); // map of CF input cols to other retrieved cols

            if (eqp.Qe.State == QueryEngineState.Searching || !eqp.ReturnQNsInFullDetail)
            { // if searching then return values from Oracle for comparison
                sql = BuildFullSql(cf);
            }

            else if (cf.CalcType == CalcTypeEnum.Basic) // if basic calc field see what data we can get from other buffers
            {
                //int colCnt = cf.GetInputColumnCount();
                //int inputRetrievalCnt = InputColsRetrievedByCfTableCount(Qt);

                sql = "";
                if (_dataMap.UnmappedCount == 0 && !_dataMap.SingleUnmapped) return sql; // can map all data, return empty sql

                else if (_dataMap.SingleUnmapped) // build sql for single col to be retrieved into calc_field
                {
                    sql = BuildBasicSingleColumnSql(_dataMap.FirstUnmappedInputMc);
                    sql = "select * from (" + sql + ")";
                }

                else sql = BuildFullSql(cf);
            }

            else sql = BuildFullSql(cf); // advanced calc always builds full sql

            // Complete the  Sql

            Exprs = Criteria = OrderBy = "";
            string innerExprs = ""; // mapping of selected & criteria columns

            for (int qci = 0; qci < Qt.QueryColumns.Count; qci++)
            {
                qc = Qt.QueryColumns[qci];
                if (qc.MetaColumn == null) continue;

                mc = qc.MetaColumn;

                if (!qc.Is_Selected_or_Criteria_or_GroupBy_or_Sorted) continue;

                if (qc.Is_Selected_or_GroupBy_or_Sorted)
                {
                    if (Exprs.Length > 0) Exprs += ",";
                    Exprs += mc.Name;
                }

                string innerExpr = GetColumnSelectionExpression(mc);
                if (innerExprs.Length > 0) innerExprs += ", ";
                innerExprs += innerExpr;
            }

            if (!_dataMap.SingleUnmapped) // build full sql if not already done
            {
                FromClause = "(" + sql + ")";
                sql = "select /*+ hint */ " + Exprs +
                    " from (select " + innerExprs + " from " + FromClause + ")";
            }

            if (Qt.Alias != "") sql += " " + Qt.Alias;
            if (eqp.CallerSuppliedCriteria != "")
                sql += " where " + eqp.CallerSuppliedCriteria;

            return sql;
        }

        /// <summary>
        /// Count number of input columns selected, with criteria or sorted
        /// </summary>
        /// <param name="qt"></param>
        /// <returns></returns>

        //int InputColsRetrievedByCfTableCount(QueryTable qt)
        //{
        //    string sourceTable = "", sourceColumn = "";
        //    int count = 0;

        //    for (int qci = 0; qci < qt.QueryColumns.Count; qci++)
        //    {
        //        QueryColumn qc = qt.QueryColumns[qci];
        //        if (qc.MetaColumn == null) continue;
        //        if (!qc.Selected_or_Criteria_or_Sorted) continue;

        //        if (!ParseCfInputMcSource(qc.MetaColumn, ref sourceTable, ref sourceColumn)) continue;
        //        count++;
        //    }

        //    return count;
        //}

        /// <summary>
        /// Parse the source table and column name from a CF input MetaColumn
        /// </summary>
        /// <param name="cfInputMc"></param>
        /// <returns></returns>

        bool ParseCfInputMcSource(
            MetaColumn cfInputMc,
            ref string sourceTable,
            ref string sourceColumn)
        {
            if (!IsCfInputMc(cfInputMc)) return false;

            string[] sa = cfInputMc.TableMap.Split('.'); // get source table.column
            if (sa.Length != 2) return false;

            sourceTable = sa[0];
            sourceColumn = sa[1];
            return true;
        }


        /// <summary>
        /// Return true if column is a CF table col that is one of the CF input cols 
        /// I.e. Not the key col or the calculated field output column
        /// </summary>
        /// <param name="cfInputMc"></param>
        /// <returns></returns>

        bool IsCfInputMc(MetaColumn cfInputMc)
        {
            if (cfInputMc.IsKey || Lex.Eq(cfInputMc.Name, "CALC_FIELD")) return false;

            else return true;
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
        /// Build full sql from advanced expression
        /// </summary>
        /// <param name="advExpr"></param>
        /// <returns></returns>

        internal string BuildAdvancedExpressionSql(
            string advExpr)
        {
            CalcField cf = new CalcField();
            cf.CalcType = CalcTypeEnum.Advanced;
            cf.AdvancedExpr = advExpr;
            Qt = null; // no query table
            Eqp = null; // no execute query parms
            return BuildFullSql(cf);
        }

        /// <summary>
        /// Build sql to do full calculation
        /// </summary>
        /// <param name="cf"></param>
        /// <returns></returns>

        internal string BuildFullSql(
            CalcField cf)
        {
            MetaTable mt;
            string advExpr, sql = "", tableAlias, outerMcName, innerMcExpr = "", keyExpr = "", valueExpr = "", exprs = "", tableSqlList = "", joinCriteria = "";
            string sourceTable = null, sourceColumn = null, tok;

            Dictionary<string, string> tableDict; // MetaTable name to alias
            Dictionary<string, Dictionary<string, MetaColumn>> tableMcDict; // MetaTable name to dict of cols for table referenced in calc field

            if (cf.CalcType == CalcTypeEnum.Basic)
                advExpr = cf.ConvertBasicToAdvanced();
            else if (cf.CalcType == CalcTypeEnum.Advanced)
                advExpr = cf.AdvancedExpr;
            else throw new ArgumentException();

            ExpressionConverter ec = new ExpressionConverter();
            valueExpr = ec.Convert(advExpr); // convert to sql

            string outerJoinRoot = "";
            if (cf.CalcType == CalcTypeEnum.Advanced && Lex.IsDefined(cf.OuterJoinRoot) &&
                MetaTableCollection.Get(cf.OuterJoinRoot) != null && ec.TableDict.ContainsKey(cf.OuterJoinRoot))
            {
                outerJoinRoot = cf.OuterJoinRoot;
            }

            MetaTable mt1 = null;
            string mt1Alias = "";

            List<string> withClauseList = new List<string>(); // list of with clauses to appear at top of sql

            Dictionary<MetaBrokerType, int> mbTypeCounts = new Dictionary<MetaBrokerType, int>();
            Dictionary<MetaBrokerType, int> mbSummarizedCounts = new Dictionary<MetaBrokerType, int>();
            Dictionary<MetaBrokerType, int> mbUnsummarizedCounts = new Dictionary<MetaBrokerType, int>();

            foreach (string mtName in ec.TableDict.Keys) // count occurances of each metabroker type in query
            {
                mt = MetaTableCollection.Get(mtName);
                MetaBrokerType mbType = mt.MetaBrokerType;
                mbTypeCounts[mbType] = GetMbTypeCount(mbTypeCounts, mbType) + 1;
                if (mt.UseSummarizedData) mbSummarizedCounts[mbType] = GetMbTypeCount(mbSummarizedCounts, mbType) + 1;
                else mbUnsummarizedCounts[mbType] = GetMbTypeCount(mbUnsummarizedCounts, mbType) + 1;
            }

            bool nested = GetMbTypeCount(mbTypeCounts, MetaBrokerType.CalcField) > 0; // nested calc field?

            foreach (string mtName in ec.TableDict.Keys)
            {
                tableAlias = ec.TableDict[mtName];
                mt = MetaTableCollection.Get(mtName);

                if ((Lex.IsUndefined(outerJoinRoot) && tableAlias == "T1") ||
                 (Lex.IsDefined(outerJoinRoot) && Lex.Eq(mtName, outerJoinRoot)))
                {
                    mt1 = mt;
                    mt1Alias = tableAlias;
                }

                Dictionary<string, MetaColumn> mcDict = ec.TableMcDict[mtName];
                string sqlForTable = BuildUnderlyingColumnsSql(mtName, mcDict);

                int buildDepth = GetBuildSqlDepth();
                if (buildDepth == 1) // top-level calc field, extract with clauses
                {
                    ExtractInnerWithClauses(ref sqlForTable, withClauseList); // bring any inner with clauses up to the top

                    if (mbTypeCounts.Count == 1 && mbUnsummarizedCounts.Count == 1 && mbUnsummarizedCounts.ContainsKey(MetaBrokerType.Assay))
                        sqlForTable = InsertMaterializeHint(sqlForTable);

                    withClauseList.Add(tableAlias + " as (" + sqlForTable + ")"); // add with clause for remaining sql for table

                    if (tableSqlList != "") tableSqlList += ", "; // accumulate list of aliases
                    tableSqlList += tableAlias;
                }

                else // sub-level calc field
                {
                    if (tableSqlList != "") tableSqlList += ", ";
                    tableSqlList += "(" + sqlForTable + ") " + tableAlias;
                }
            }

            innerMcExpr = "NULL"; // name of metacolumn containing key value in the calculated field expression
            outerMcName = "COMPOUNDID"; // name of the key metacolumn cf metatable def

            if (Eqp != null)
            {
                KeyMc = Eqp.QueryTable.MetaTable.KeyMetaColumn;
                outerMcName = KeyMc.Name;
            }

            if (mt1 != null) // the inner mt1 table supplies the key value
                innerMcExpr = mt1Alias + "." + mt1.KeyMetaColumn.Name;

            exprs = // start list of select expressions with key
                innerMcExpr + " " + outerMcName;

            if (Qt != null) // include any input cols selected, with criteria or sorted if Qt defined
                for (int qci = 0; qci < Qt.QueryColumns.Count; qci++)
                {
                    QueryColumn qc = Qt.QueryColumns[qci];
                    var mc = qc.MetaColumn;

                    if (mc == null) continue;
                    if (!qc.Is_Selected_or_Criteria_or_GroupBy_or_Sorted) continue;
                    if (!ParseCfInputMcSource(mc, ref sourceTable, ref sourceColumn)) continue;

                    tableAlias = ec.TableDict[sourceTable];
                    innerMcExpr = tableAlias + "." + sourceColumn;
                    exprs += ", " + innerMcExpr + " " + mc.Name;
                }

            exprs += ", " + valueExpr + " calc_field"; // assign main expression to calc_field

            sql = "";
            for (int i0 = 0; i0 < withClauseList.Count; i0++) // prepend "with" clauses
            {
                string withClause = withClauseList[i0];
                withClause = OracleDao.FormatSql(withClause); // debug

                if (i0 == 0) sql += "with ";
                else sql += ", ";
                sql += withClause;

                if (i0 == withClauseList.Count - 1)
                    sql += " ";
            }

            sql += "\r\nselect " + exprs +
                " from " + tableSqlList;

            if (ec.TableDict.Keys.Count > 1) // build criteria to join tables if more than one table
            {
                foreach (string mtName in ec.TableDict.Keys)
                {
                    tableAlias = ec.TableDict[mtName];
                    if (tableAlias == mt1Alias) continue;
                    mt = MetaTableCollection.Get(mtName);

                    string joinOp = " = "; // default to equijoin
                    if (Lex.IsDefined(outerJoinRoot)) joinOp = " (+) = "; // do outer joins against joinRoot

                    if (joinCriteria != "") joinCriteria += " and ";
                    if (mt1 != null)
                        joinCriteria += tableAlias + "." + mt.KeyMetaColumn.Name + joinOp + mt1Alias + "." + mt1.KeyMetaColumn.Name;
                }

                sql += " where " + joinCriteria;
            }

            return sql;
        }

        /// <summary>
        /// GetMbTypeCount
        /// </summary>
        /// <param name="mbDict"></param>
        /// <param name="mbType"></param>
        /// <returns></returns>
        int GetMbTypeCount(
            Dictionary<MetaBrokerType, int> mbDict,
            MetaBrokerType mbType)
        {
            if (!mbDict.ContainsKey(mbType)) return 0;
            else return mbDict[mbType];
        }

        /// <summary>
        /// Extract any inner with clauses from sql for table
        /// This is done so that all with clauses can appear before the main
        /// select statement body to avoid nested with clauses and 
        /// the resulting "ORA-32034: unsupported use of WITH clause" errors.
        /// </summary>
        /// <param name="sqlForTable"></param>
        /// <param name="withClauses"></param>

        void ExtractInnerWithClauses(
            ref string sqlForTable,
            List<string> withClauses)
        {
            string beginWith = "/* begin-with */";
            string endWith = "/* end-with */";
            int i1, i2;

            while ((i1 = Lex.IndexOf(sqlForTable, beginWith)) >= 0)
            {
                i2 = Lex.IndexOf(sqlForTable, endWith);
                if (i2 < 0) throw new Exception("Unmatched " + beginWith);
                int len = (i2 + endWith.Length) - i1;
                string with = sqlForTable.Substring(i1, len);
                with = Lex.Replace(with, beginWith, "");
                with = Lex.Replace(with, endWith, "").Trim();
                if (Lex.StartsWith(with, "with ")) with = with.Substring(5).Trim();
                withClauses.Add(with); // add to list

                sqlForTable = sqlForTable.Substring(0, i1) + " " + sqlForTable.Substring(i1 + len); // remove from main sql
            }

            return;
        }

        /// <summary>
        /// Add hint to materialize the table for better performance as appropriate
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        string InsertMaterializeHint(string sqlForTable)
        {
            string selectTxt = "select ";
            int i1 = Lex.IndexOf(sqlForTable, selectTxt);
            if (i1 >= 0 && Eqp.SearchKeySubset != null && Eqp.SearchKeySubset.Count > 0)
            { // do only if keyset is provided that will result in a relatively small materialized table (i.e. not the full metatable)
                i1 += selectTxt.Length;
                sqlForTable = sqlForTable.Substring(0, i1) + " /*+ materialize */ " + sqlForTable.Substring(i1);
                sqlForTable = Lex.Replace(sqlForTable, "/*+ hint */", ""); // remove any hint placeholder
            }

            return sqlForTable;
        }

        /// <summary>
        /// Build sql for a single column
        /// </summary>
        /// <param name="mc"></param>
        /// <returns></returns>

        string BuildBasicSingleColumnSql(
            MetaColumn mc)
        {
            //throw new NotImplementedException();

            MetaTable mt = null;
            string fName, exprSql;

            string criteria = "";

            var exprs = "t1." + mc.MetaTable.KeyMetaColumn.Name + " " + Qt.MetaTable.KeyMetaColumn.Name + ", " + // select key
                                         "t1." + mc.Name + " CALC_FIELD";

            var fromSql = BuildUnderlyingColumnSql(mc);
            var tables = "(" + fromSql + ") t1";

            // Put together full Sql

            string sql = "select " + exprs +
                " from " + tables;
            if (criteria != "") sql += " where " + criteria;

            return sql;
        }

        ///// <summary>
        ///// Build sql for a single calculated field column
        ///// </summary>
        ///// <param name="mc"></param>
        ///// <param name="tableAlias"></param>
        ///// <param name="exprSql"></param>
        ///// <param name="fromSql"></param>

        //void BuildColumnSql(
        //    MetaColumn mc,
        //    string function,
        //    string constant,
        //    string tableAlias,
        //    out string exprSql,
        //    out string fromSql)
        //{
        //    exprSql = BuildBasicFunctionExpression(mc, function, constant, tableAlias); // expression for field

        //    fromSql = BuildUnderlyingColumnSql(mc);

        //    return;
        //}

        /// <summary>
        /// Build the sql to retrieve the value of an underlying table and the column that go into the calculation
        /// </summary>
        /// <param name="mc"></param>
        /// <returns></returns>

        string BuildUnderlyingColumnSql(MetaColumn mc)
        {
            Dictionary<string, MetaColumn> mcDict = new Dictionary<string, MetaColumn>();
            mcDict[mc.Name] = mc;
            return BuildUnderlyingColumnsSql(mc.MetaTable.Name, mcDict);
        }


        //string BuildUnderlyingColumnsSql(Dictionary<string, string> tableDict)
        //{
        //    string tableSql = "";
        //    foreach (string mtName in tableDict.Keys)
        //    {
        //        string tableAlias = tableDict[mtName];
        //        MetaTable mt = MetaTableCollection.Get(mtName);

        //        if ((Lex.IsUndefined(outerJoinRoot) && tableAlias == "T1") ||
        //         (Lex.IsDefined(outerJoinRoot) && Lex.Eq(mtName, outerJoinRoot)))
        //        {
        //            MetaTable mt1 = mt;
        //            string mt1Alias = tableAlias;
        //        }

        //        Dictionary<string, MetaColumn> mcDict = tableMcDict[mtName];
        //        string sqlForTable = BuildUnderlyingColumnsSql(mtName, mcDict);
        //        if (tableSql != "") tableSql += ", ";
        //        tableSql += "(" + sqlForTable + ") " + tableAlias;
        //    }
        //}


        /// <summary>
        /// Build the sql to retrieve the value of the underlying table and the columns that go into the calculation
        /// </summary>
        /// <param name="mtName"></param>
        /// <param name="mcDict"></param>
        /// <returns></returns>
        string BuildUnderlyingColumnsSql(string mtName, Dictionary<string, MetaColumn> mcDict)
        {
            Query q = new Query(); // build temp query
            MetaTable mt = MetaTableCollection.Get(mtName);
            QueryTable qt = new QueryTable(q, mt);
            for (int ci = 0; ci < qt.QueryColumns.Count; ci++)
            {
                QueryColumn qc = qt.QueryColumns[ci];
                qc.Selected = false;
                if (qc.MetaColumn == null) continue;

                if (qc.IsKey)
                    qc.Selected = true;
                else if (mcDict.ContainsKey(qc.MetaColumn.Name))
                {
                    qc.Selected = true;
                    qc.Criteria = qc.MetaColumn.Name + " is not null"; // specifying criteria makes search faster
                }
            }

            QueryEngine qe = new QueryEngine();
            qe.Query = q;
            ExecuteQueryParms eqp = new ExecuteQueryParms(qe, qt);
            eqp.ReturnQNsInFullDetail = false; // get qualified numbers as doubles
            string sql = qe.BuildSqlForSingleTable(eqp); // have the associated broker build the sql
            return sql;
        }

        ///// <summary>
        ///// Translate from UI function code string to Oracle expression containing column name
        ///// </summary>
        ///// <param name="mc"></param>
        ///// <param name="funcFromUI"></param>
        ///// <param name="constant"></param>
        ///// <param name="tableAlias"></param>
        ///// <returns></returns>
        //private string BuildBasicFunctionExpression(
        //    MetaColumn mc,
        //    string funcFromUI,
        //    string constant,
        //    string tableAlias)
        //{
        //    string exp;

        //    CalcFuncEnum cfe = CalcField.ConvertCalcFuncStringToEnum(funcFromUI);

        //    if (cfe == CalcFuncEnum.None)
        //    {
        //        if (mc.DataType == MetaColumnType.Date)
        //            exp = "trunc(<v>)"; // truncate all dates
        //        else exp = "<v>";
        //    }
        //    else if (cfe == CalcFuncEnum.Abs)
        //        exp = "abs(<v>)";
        //    else if (cfe == CalcFuncEnum.Neg)
        //        exp = "-(<v>)";
        //    else if (cfe == CalcFuncEnum.Log)
        //        exp = "decode(<v>,0,to_number(null),log(10,<v>))";
        //    else if (cfe == CalcFuncEnum.NegLog)
        //        exp = "decode(<v>,0,to_number(null),-log(10,<v>))";
        //    else if (cfe == CalcFuncEnum.Ln)
        //        exp = "decode(<v>,0,to_number(null),ln(<v>))";
        //    else if (cfe == CalcFuncEnum.NegLn)
        //        exp = "decode(<v>,0,to_number(null),-ln(<v>))";
        //    else if (cfe == CalcFuncEnum.Sqrt)
        //        exp = "sqrt(<v>)";
        //    else if (cfe == CalcFuncEnum.Square)
        //        exp = "power(<v>,2)";
        //    else if (cfe == CalcFuncEnum.AddConst)
        //        exp = "(<v> +  " + constant + ")";
        //    else if (cfe == CalcFuncEnum.MultConst)
        //        exp = "(<v> *  " + constant + ")";

        //    else if (cfe == CalcFuncEnum.NegLogMolConc)
        //        exp = GetNegLogConcExpr(mc, 1);

        //    //else if (cfe == CalcFuncEnum.NegLogUmConc)
        //    //  exp = GetNegLogConcExpr(mc, 1000000);

        //    //else if (cfe == CalcFuncEnum.NegLogNmConc)
        //    //  exp = GetNegLogConcExpr(mc, 1000000000);

        //    else if (cfe == CalcFuncEnum.DaysSince)
        //        exp = "(trunc(sysdate) - trunc(<v>))"; // truncate to whole days

        //    else throw new Exception("BuildBasicFunctionExpression unexpected function " + funcFromUI);

        //    exp = exp.Replace("<v>", tableAlias + "." + mc.Name);
        //    return exp;
        //}

        ///// <summary>
        ///// Get -Log of concentration
        ///// </summary>
        ///// <param name="mc"></param>
        ///// <param name="desiredUnitsFactor">1=molar, 1000000 = uM, 1000000000 = nM</param>
        ///// <returns></returns>

        //string GetNegLogConcExpr(
        //    MetaColumn mc,
        //    double desiredUnitsFactor)
        //{
        //    double molFactor = AssayAttributes.GetMolarConcFactor(mc);
        //    string factorString = (desiredUnitsFactor * molFactor).ToString();
        //    string exp = "(decode(<v>,0,to_number(null),-log(10,<v> * " + factorString + ")))";
        //    return exp;
        //}

        /// <summary>
        /// Execute query
        /// </summary>
        /// <param name="eqp"></param>

        public override void ExecuteQuery(
            ExecuteQueryParms eqp)
        {
            Eqp = eqp;
            Qt = eqp.QueryTable;
            CalcFieldMetaTable mt = GetCalcFieldMetaTable(Qt.MetaTable);
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
                while (true)
                {
                    if (!_calcField.IsClassificationDefined || _dataMap.IsMappable)
                    { // call base broker if no classification or calc done later
                        if (_calcField.IsClassificationDefined)
                            SwitchToPreclassificationType();

                        row = base.NextRow();

                        if (_calcField.IsClassificationDefined)
                            SwitchToPostclassificationType();

                        if (row == null) return null;
                        if (CfValColSelectListPos < 0) continue; // just return if calc field value not selected
                        v = row[CfValColSelectListPos]; // calculated value
                        if (NullValue.IsNull(v)) continue; // ignore row if value is null

                        return row;
                    }

                    else // get value and then classify
                    {
                        SwitchToPreclassificationType();
                        row = base.NextRow();
                        SwitchToPostclassificationType();

                        if (row == null) return null;
                        if (row.Length < 2) continue; // just return if calc field value not selected

                        int voi = CfValColSelectListPos;
                        v = row[voi]; // calculated value
                        QualifiedNumber qn = ApplyClassification(v);
                        //if (qn != null && Lex.Contains(qn.TextValue, "Type=Sketch")) qn = qn; // debug
                        row[voi] = qn;
                        return row;
                    }
                }

            else return null; // filled in later from other buffer values
        }

        /// <summary>
        /// SwitchToPreclassificationType
        /// </summary>

        void SwitchToPreclassificationType()
        {
            if (_qt2 == null || _mt2 == null || CfValColMetaTablePos < 0 || CfValColSelectListPos < 0) return; // just in case

            _qt0 = Qt;
            Qt = _qt2; // substitute modifyable copy
            Eqp.QueryTable = _qt2;
            SelectList[CfValColSelectListPos] = _mt2.MetaColumns[CfValColMetaTablePos];
            _mt2.MetaColumns[CfValColMetaTablePos].DataType = _calcField.PreclassificationlResultType; // temporarily set col type to match data retrieved
        }

        /// <summary>
        /// SwitchToPostclassificationType
        /// </summary>

        void SwitchToPostclassificationType()
        {
            if (_qt2 == null || CfValColMetaTablePos < 0 || CfValColSelectListPos < 0) return; // just in case

            Qt = _qt0;
            Eqp.QueryTable = Qt;
            SelectList[CfValColSelectListPos] = Qt.MetaTable.MetaColumns[CfValColMetaTablePos];
            Qt.MetaTable.MetaColumns[CfValColMetaTablePos].DataType = _calcField.FinalResultType; // reset col type (need to do?)
        }

        /// <summary>
        /// Close broker & release resources
        /// </summary>

        public override void Close()
        {
            CalcFieldMetaTable mt = GetCalcFieldMetaTable(Qt.MetaTable);
            if (!mt.CalcField.IsClassificationDefined)
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
                if (ri >= results.Count)
                    throw new Exception(String.Format("Index [{0}] out of range 0 - {1}", ri, results.Count - 1));

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
        }

        /// <summary>
        /// Calculate value for field
        /// </summary>
        /// <param name="cf"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        object CalculateValue(
            CalcField cf,
            object v1,
            CalcFieldColumn cfc2,
            object v2)
        {
            object v = null;
            double dv = NullValue.NullNumber, d1 = 0, d2 = 0;
            string qualifier = "";
            QualifiedNumber qn;

            if (v1 == null || v2 == null) return null;

            if (cf.OpEnum == CalcOpEnum.Sub && // difference
             cfc2.MetaColumn.DataType == MetaColumnType.Date && // in dates?
             cfc2.FunctionEnum == CalcFuncEnum.None) // with no function on individual dates
            { // take difference between dates
                DateTime dt1 = (DateTime)v1;
                DateTime dt2 = (DateTime)v2;
                TimeSpan ts = dt1.Date.Subtract(dt2.Date);
                dv = ts.TotalDays;
                v = dv;
            }

            else if (cfc2.MetaColumn.DataType == MetaColumnType.Image)
            {
                GenericMetaBroker mb = QueryEngine.GetGlobalBroker(cfc2.MetaColumn.MetaTable);
                string sv1 = "", sv2 = "";
                if (v1 != null) sv1 = mb.ConvertValueToImageReference(v1);
                if (v2 != null) sv2 = mb.ConvertValueToImageReference(v2);

                v = sv1 + CalcField.OverlayOpPadded + sv2; // build overlay expression
            }

            else // numeric function
            {
                if (!QualifiedNumber.TryConvertToQualifiedDouble(v1, out d1, out qualifier)) return null;
                if (d1 == NullValue.NullNumber) return null;
                v1 = d1;

                if (!QualifiedNumber.TryConvertToQualifiedDouble(v2, out d2, out qualifier)) return null;
                if (d2 == NullValue.NullNumber) return null;
                v2 = d2;

                if (cf.OpEnum == CalcOpEnum.Add) // numeric function
                    dv = d1 + d2;
                else if (cf.OpEnum == CalcOpEnum.Sub)
                    dv = d1 - d2;
                else if (cf.OpEnum == CalcOpEnum.Mul)
                    dv = d1 * d2;
                else if (cf.OpEnum == CalcOpEnum.Div)
                {
                    if (d2 != 0) dv = d1 / d2;
                    else dv = NullValue.NullNumber;  // say null if attempt to divide by zero
                }

                v = dv;
            }

            return v;
        }

        /// <summary>
        /// Apply classification
        /// </summary>
        /// <param name="cf"></param>
        /// <param name="v"></param>
        /// <returns></returns>

        object ApplyClassification(
            CalcField cf,
            object v)
        {
            double dv = NullValue.NullNumber;
            if (v is double) dv = (double)v;

            if (!cf.IsClassificationDefined)
            { // if no classification then just return numeric value in desired type
                if (cf.FinalResultType == MetaColumnType.Number)
                    return dv;

                else if (cf.FinalResultType == MetaColumnType.Integer)
                    return (int)dv;

                else if (cf.FinalResultType == MetaColumnType.QualifiedNo)
                    return new QualifiedNumber(dv);

                else if (cf.FinalResultType == MetaColumnType.Date)
                    return (DateTime)v;

                else if (cf.FinalResultType == MetaColumnType.Image)
                    return (string)v; // return string describing the image

                else throw new Exception("Unexpected result type: " + cf.FinalResultType);
            }

            QualifiedNumber qn = ApplyClassification(v);
            return qn;
        }

        /// <summary>
        /// Apply classification/conditional formatting rules to value
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>

        QualifiedNumber ApplyClassification(
            object v)
        {
            CalcField cf = _calcField;
            double d;

			if (cf.PreclassificationlResultType == MetaColumnType.Structure && !(v is MoleculeMx))
			{ // if structure be sure formatted as structure object
				MoleculeMx cs = MoleculeUtil.ConvertObjectToChemicalStructure(v);
				v = cs;
			}

            CondFormatRule rule = CondFormatMatcher.Match(cf.Classification, v);
            if (rule == null) return null;

            QualifiedNumber qn = new QualifiedNumber();
            qn.BackColor = rule.BackColor1; // set any color

            if (MetaColumn.IsNumericMetaColumnType(cf.FinalResultType))
            { // if numeric final result type then convert rule name to number

                if (double.TryParse(rule.Name, out d))
                    qn.NumberValue = d;
                else return null;
            }
            else qn.TextValue = rule.Name;

            //if (Lex.Contains(qn.TextValue, "Type=Sketch")) qn = qn; // debug

            return qn;
        }

        /// <summary>
        /// Apply function to a argument in a basic calc function 
        /// </summary>
        /// <param name="cfc"></param>
        /// <param name="v"></param>
        /// <returns></returns>

        object ApplyFunction(
            CalcFieldColumn cfc,
            object v)
        {
            double d;
            string qualifier = "";

            if (cfc.MetaColumn == null) // just return null if metacolumn not defined
                return null;

            else if (cfc.FunctionEnum == CalcFuncEnum.None) // no function
            {
                if (MetaColumn.IsNumericMetaColumnType(cfc.MetaColumn.DataType))
                { // convert numeric values to double
                    if (!QualifiedNumber.TryConvertToQualifiedDouble(v, out d, out qualifier)) return null;
                    if (d == NullValue.NullNumber) return null;
                    v = d;
                }
            }

            else // apply function, all functions return numeric value
            {
                d = ApplyNumericFunction(cfc.MetaColumn, v, cfc.FunctionEnum, cfc.ConstantDouble);
                if (d == NullValue.NullNumber) return null;
                v = d;
            }

            return v;
        }


        /// <summary>
        /// Apply numeric function to value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="func"></param>
        /// <returns></returns>

        double ApplyNumericFunction(
            MetaColumn mc,
            object o,
            CalcFuncEnum func,
            double constant)
        {
            double dVal = NullValue.NullNumber;
            DateTime dtVal = DateTime.MinValue;
            string qualifier = "";

            if (o is DateTime)
            {
                dtVal = (DateTime)o;
                if (dtVal.Equals(DateTime.MinValue)) return NullValue.NullNumber;
            }
            else
            {
                QualifiedNumber.TryConvertToQualifiedDouble(o, out dVal, out qualifier);
                if (dVal == NullValue.NullNumber) return NullValue.NullNumber;
            }

            if (func == CalcFuncEnum.None)
                return dVal;
            else if (func == CalcFuncEnum.Abs)
                return Math.Abs(dVal);
            else if (func == CalcFuncEnum.Neg)
                return -dVal;
            else if (func == CalcFuncEnum.Log)
                return dVal == 0 ? NullValue.NullNumber : Math.Log10(dVal);
            else if (func == CalcFuncEnum.NegLog)
                return dVal == 0 ? NullValue.NullNumber : -Math.Log10(dVal);
            else if (func == CalcFuncEnum.Ln)
                return dVal == 0 ? NullValue.NullNumber : Math.Log(dVal);
            else if (func == CalcFuncEnum.NegLn)
                return dVal == 0 ? NullValue.NullNumber : -Math.Log(dVal);
            else if (func == CalcFuncEnum.Sqrt)
                return Math.Sqrt(dVal);
            else if (func == CalcFuncEnum.Square)
                return Math.Pow(dVal, 2);
            else if (func == CalcFuncEnum.AddConst)
                return dVal + constant;
            else if (func == CalcFuncEnum.MultConst)
                return dVal * constant;

            else if (func == CalcFuncEnum.NegLogMolConc)
                return GetNegLogConcValue(dVal, mc, 1);

            //else if (func == CalcFuncEnum.NegLogUmConc)
            //  return GetNegLogConcValue(dVal, mc, 1000000);

            //else if (func == CalcFuncEnum.NegLogNmConc)
            //  return GetNegLogConcValue(dVal, mc, 1000000000);

            else if (func == CalcFuncEnum.DaysSince)
            {
                TimeSpan ts = DateTime.Now.Date.Subtract(dtVal.Date);
                return ts.TotalDays;
            }

            else throw new Exception("Invalid function");
        }

        /// <summary>
        /// Get -Log of concentration
        /// </summary>
        /// <param name="dVal"></param>
        /// <param name="mc"></param>
        /// <param name="desiredUnitsFactor">1=molar, 1000000 = uM, 1000000000 = nM</param>
        /// <returns></returns>

        double GetNegLogConcValue(
            double dVal,
            MetaColumn mc,
            double desiredUnitsFactor)
        {
            double molFactor = AssayAttributes.GetMolarConcFactor(mc);
            double factor = desiredUnitsFactor * molFactor;
            double val = -Math.Log10(dVal * factor);
            return val;
        }

        /// <summary>
        /// Map CF input columns to other retrieved data 
        /// </summary>

        OtherDataMap MapInputColsToOtherRetrievedData(
         CalcField cf,
         Query q,
         QueryTableData[] qtd)
        {
            MetaColumn mc;
            QueryColumn qc, sourceQc, cfQc = null;
            int ti, ci, voi, i1;

            OtherDataMap cm = new OtherDataMap();

            List<MetaColumn> inputMcList = cf.GetInputMetaColumnList();
            cm.InputColMap = new QueryColumn[inputMcList.Count];

            cm.SelectedColMap = new QueryColumn[SelectList.Count];

            cm.MappedCount = 0;
            cm.UnmappedCount = 0;
            cm.FirstUnmappedPos = -1;

            // Try to map the list of CF inputs to other cols selected in other query tables

            for (i1 = 0; i1 < inputMcList.Count; i1++)
            {
                mc = inputMcList[i1];

                sourceQc = MapInputMcToOtherRetrievedData(mc);
                if (sourceQc != null)
                {
                    cm.InputColMap[i1] = sourceQc;
                    cm.MappedCount++;
                }

                else
                {
                    cm.InputColMap[i1] = null;
                    cm.UnmappedCount++;
                    if (cm.FirstUnmappedInputMc == null)
                    {
                        cm.FirstUnmappedInputMc = mc; // Metacolumn from outside of CF
                        cm.FirstUnmappedCfMc = Qt.MetaTable.GetMetaColumnByName(mc.MetaTable.Name + "_" + mc.Name); // MetaColumn from inside CF
                        cm.FirstUnmappedPos = i1;
                    }
                }
            }

            if (cm.MappedCount > 0 && cm.UnmappedCount == 1) // if all but one input mapped then retrieve that col value into the CF col position
            {
                cfQc = Qt.GetQueryColumnByNameWithException("CALC_FIELD"); // calculated value QueryColumn
                cm.InputColMap[cm.FirstUnmappedPos] = cfQc;
                cm.MappedCount++;
                cm.UnmappedCount--;
                cm.SingleUnmapped = true;
            }

            for (int sci = 0; sci < SelectList.Count; sci++) // map selected input cols as well
            {
                mc = SelectList[sci];

                qc = MapCfInputColToOtherRetrievedData(mc);

                if (qc == null && cm.SingleUnmapped && mc == cm.FirstUnmappedCfMc) // is this the only unmapped col?
                    qc = cfQc;

                if (qc != null)
                    cm.SelectedColMap[sci] = qc;
            }

            // Determine if table data can be retrieved from other data

            cm.IsMappable =
             (cm.MappedCount > 0 && // must have at least one input col mapped
                cm.UnmappedCount == 0); // and none that aren't mapped

            if (cf.CalcType != CalcTypeEnum.Basic)
                cm.IsMappable = false; // only basic CFs are mappable

            if (Qt.GetCriteriaCount(false, false) > 0) // if criteria on any non-key CF cols then can't map
                cm.IsMappable = false;

            if (q != null && q.SingleStepExecution) cm.IsMappable = false; // can't map single step queries

            //cm.IsMappable = xxxfalse; // debug

            if (!cm.IsMappable) // if not mappable then set other members accordingly to reflect this
            {
                cm.InputColMap = new QueryColumn[inputMcList.Count];
                cm.SelectedColMap = new QueryColumn[SelectList.Count];
                cm.MappedCount = 0;
                cm.UnmappedCount = inputMcList.Count;
                cm.SingleUnmapped = false;
            }

            return cm;
        }

        /// <summary>
        /// Try to map a CF input column to another column retrieved in the query
        /// </summary>
        /// <param name="cfInputMc">MetaColumn from the CF Metatable that we're looking for the associated</param>

        QueryColumn MapCfInputColToOtherRetrievedData(
            MetaColumn cfInputMc)
        {
            string sourceTable = "", sourceColumn = "";

            if (!ParseCfInputMcSource(cfInputMc, ref sourceTable, ref sourceColumn))
                return null;

            QueryColumn qc = MapMcToOtherRetrievedData(sourceTable, sourceColumn);
            return qc;
        }

        /// <summary>
        /// Try to map a Metacolumn needed by the CF to other retrieved data
        /// </summary>
        /// <param name="mtName">Column we are looking for</param>
        /// <param name="mcName"></param>
        /// <returns></returns>

        QueryColumn MapMcToOtherRetrievedData(
            string mtName,
            string mcName)
        {
            MetaTable mt = MetaTableCollection.Get(mtName);
            if (mt == null) return null;
            MetaColumn mc = mt.GetMetaColumnByName(mcName);
            if (mc == null) return null;

            QueryColumn qc = MapInputMcToOtherRetrievedData(mc);
            return qc;
        }

        /// <summary>
        /// Try to map a Metacolumn needed by the CF to other retrieved data
        /// </summary>
        /// <param name="mcToFine">The metacolumn we want to find elsewhere</param>
        /// <returns></returns>

        QueryColumn MapInputMcToOtherRetrievedData(
            MetaColumn mcToFind)
        {
            QueryColumn qc = null;
            int ti=-1, ci;
            const int keyOffset = 1;


            List<int> metaTableIndexes = Qe.Query.GetAllTableIndexesByName(mcToFind.MetaTable.Name); 

            if (metaTableIndexes == null)
                return null;

            if (metaTableIndexes.Count > 1)
            {
                int qti = Qe.Query.GetQueryTableIndex(Qt); //current query table index
                List<int> calcFieldIndexes = Qe.Query.GetAllCalcFieldTableIndexesWithMetaColumn(mcToFind);

                //Assume the metatables and calcfields appear in the same order in the query calcfield 1 to metatable 1 and calcfield 2 to metatable 2 
                if (calcFieldIndexes.Contains(qti))
                {
                    int i = calcFieldIndexes.IndexOf(qti);

                    if (i < metaTableIndexes.Count)
                        ti = metaTableIndexes[i];
                    else
                        ti = metaTableIndexes[metaTableIndexes.Count - 1];
                }
                else
                {
                    return null; //this shouldn't happen...
                }
            }
            else
            {
                ti = metaTableIndexes[0]; 
            }            

            if (ti < 0) return null;

            QueryTableData qtd = Qe.Qtd[ti];
            QueryTable qt = qtd.Table;
            MetaTable mt = qt.MetaTable;

            for (ci = 0; ci < qtd.SelectedColumns.Count; ci++)
            {
                qc = qtd.SelectedColumns[ci];
                MetaColumn mc = qc.MetaColumn;
                if (QueryEngine.GetGlobalBroker(mt).ColumnValuesAreEquivalent(mc, mcToFind))
                    break;
            }

            if (ci < qtd.SelectedColumns.Count) return qc;
            else return null;
        }

        /// <summary>
        /// Do presearch initialization for a QueryTable
        /// </summary>
        /// <param name="qt"></param>

        public override void DoPresearchInitialization(
            QueryTable qt)
        {
            CalcFieldMetaTable mt = GetCalcFieldMetaTable(qt.MetaTable);

            if (mt.CalcField.IsClassificationDefined)
            { // clear value dictionary since underlying list may have changed
                foreach (CondFormatRule rule in mt.CalcField.Classification.Rules)
                    rule.ValueDict = null;
            }

            return;
        }

        /// <summary>
        /// Return number of columns referenced in the calculated field
        /// </summary>
        /// <param name="cf"></param>
        /// <returns></returns>

        public int ColsInCalcField(
            CalcField cf)
        {
            int colsInCalcField = 0;
            foreach (CalcFieldColumn cfc in cf.CfCols)
            {
                if (cfc.MetaColumn != null) colsInCalcField++;
            }
            return colsInCalcField;
        }

        /// <summary>
        /// Count the number of columns referenced in the calculated field that are being retrieved by other query tables
        /// </summary>
        /// <param name="cf"></param>
        /// <param name="q"></param>
        /// <returns></returns>

        public int InputColsRetrievedByOtherTablesCount(
            CalcField cf,
            Query q)
        {
            int colCount = 0;

            if (cf.OpEnum == CalcOpEnum.None)
            {
                if (ColRetrievedByOtherTable(cf.MetaColumn1, q))
                    colCount++;
            }

            else foreach (CalcFieldColumn cfc in cf.CfCols)
                {
                    if (cfc.MetaColumn == null) continue;

                    if (ColRetrievedByOtherTable(cfc.MetaColumn, q))
                        colCount++;
                }

            return colCount;
        }

        /// <summary>
        /// Check to see if a column referenced in the calculated field is being retrieved by other query tables
        /// </summary>
        /// <param name="cfLabel"></param>
        /// <param name="refMc"></param>
        /// <param name="q"></param>
        /// <param name="msg"></param>
        /// <returns></returns>

        public bool ColRetrievedByOtherTable(
            MetaColumn refMc,
            Query q)
        {
            QueryTable qt = q.GetTableByName(refMc.MetaTable.Name);
            if (qt == null) return false;

            QueryColumn qc = qt.GetQueryColumnByName(refMc.Name);
            if (qc == null || !qc.Selected) return false;

            return true;
        }

        /// <summary>
        /// Get a CalcFieldMetaTable from either a CalcFieldMetaTable or a plain MetaTable
        /// </summary>
        /// <param name="mt"></param>
        /// <returns></returns>

        CalcFieldMetaTable GetCalcFieldMetaTable(MetaTable mt)
        {
            if (mt == null) return null;
            CalcFieldMetaTable cfMt = mt as CalcFieldMetaTable;
            if (cfMt != null && cfMt.CalcField != null) return cfMt;

            MetaTableCollection.Remove(mt.Name); // if not a CalcFieldMetaTable remove and rebuild it
            cfMt = MetaTableCollection.Get(mt.Name) as CalcFieldMetaTable;
            return cfMt;
        }

        /// <summary>
        /// Build query that returns detailed drilldown data on a result
        /// </summary>
        /// <param name="mt"></param>
        /// <param name="metaColumnId"></param>
        /// <param name="level"></param>
        /// <param name="linkInfo"></param>
        /// <returns></returns>

        public override Query GetDrilldownDetailQuery(
            MetaTable mt,
            string metaColumnId,
            int level,
            string linkInfo)
        {
            foreach (MetaColumn mc in mt.MetaColumns) // look for matching code (summarized or unsummarized)
            {
                if (mc.ResultCode == metaColumnId)
                    return GetDrilldownDetailQuery(mt, mc, level, linkInfo);
            }

            return null;
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
    } // CalcFieldMetaBroker

    /// <summary>
    /// Expression conversion utilitiess 
    /// </summary>

    public class ExpressionConverter
    {
        public Dictionary<string, string> TableDict = // MetaTable name to alias
            new Dictionary<string, string>();

        public Dictionary<string, Dictionary<string, MetaColumn>> TableMcDict = // MetaTable name to dict of cols for table referenced in calc field
            new Dictionary<string, Dictionary<string, MetaColumn>>(); // the Metacolumn dict is keyed on MetaColumn name

        Lex Lex;

        public string Convert(
            string advExpr)
        {
            TableDict = new Dictionary<string, string>();
            TableMcDict = new Dictionary<string, Dictionary<string, MetaColumn>>();

            Lex = new Lex();
            Lex.SetDelimiters(", ( ) + - * /");
            Lex.OpenString(advExpr);

            List<string> list = ParseList("");
            string exprs = AppendList("", list);
            return exprs;
        }

        /// <summary>
        /// Parse a list of expressions up to the specified end token or end of input
        /// Sets of tokens between commas are converted into a single list item
        /// </summary>
        /// <param name="endToken"></param>
        /// <returns></returns>

        List<string> ParseList(
            string endToken)
        {
            string expr = "";

            List<string> toks = new List<string>();

            while (true)
            {
                string tok = ConvertNextExpression();
                if (tok == "," || Lex.Eq(tok, endToken) || tok == "")
                {
                    if (expr != "") toks.Add(expr);
                    if (tok != "") toks.Add(tok);
                    if (Lex.Eq(tok, endToken) || tok == "") return toks;
                    expr = "";
                }

                else toks.Add(tok); // just add normal token
            }
        }

        /// <summary>
        /// Convert the next expression recursively
        /// </summary>

        string ConvertNextExpression()
        {
            string expr, expr2, tok, tok2;

            tok = GetToken(Lex);
            if (tok == "") return "";

            if (Lex.IsQuoted(tok, '"')) // column name, convert to proper sql table & column reference
            {
                tok = Lex.RemoveDoubleQuotes(tok);
                MetaColumn mc = MetaColumn.ParseMetaTableMetaColumnName(tok);
                if (mc == null) return "null";

                string tableName = mc.MetaTable.Name;
                if (!TableDict.ContainsKey(tableName))
                {
                    TableDict[tableName] = "T" + (TableDict.Count + 1);
                    TableMcDict[tableName] = new Dictionary<string, MetaColumn>();
                }

                TableMcDict[tableName][mc.Name] = mc; // include the col in the list of cols referenced for this table

                return TableDict[tableName] + "." + mc.Name;
            }

            else if (tok == "(") // parenthesized expression, scan to closing paren
            { // loop til we get closing paren
                List<string> toks = ParseList(")");
                expr = AppendList(tok, toks);
                return expr;
            }

            else if (tok == "/") // division, wrap in function to avoid divide by zero
            {
                expr = tok + " ";
                expr2 = ConvertNextExpression();
                if (expr2 != null)
                {
                    expr += "nullif(" + expr2 + ", 0)"; // return null if zero
                }

                else expr += "null"; // shouldn't happen

                return expr;
            }

            else if (Lex.Eq(tok, "LN")) return ConvertFuncProperlyHandlingZeroValues(tok, 0);
            else if (Lex.Eq(tok, "EXP")) return ConvertFuncProperlyHandlingZeroValues(tok, 0);
            else if (Lex.Eq(tok, "LOG")) return ConvertFuncProperlyHandlingZeroValues(tok, 2);
            else if (Lex.Eq(tok, "POWER")) return ConvertFuncProperlyHandlingZeroValues(tok, 0);

            else return tok; // function, operator or constant
        }

        /// <summary>
        /// Convert function inserting nullif functions to properly handle args with a value of zero
        /// </summary>
        /// <param name="tok"></param>
        /// <param name="position"></param>
        /// <returns></returns>

        string ConvertFuncProperlyHandlingZeroValues(
            string tok,
            int position)
        {
            string tok2 = GetToken(Lex); // get the left paren
            tok = AppendToken(tok, tok2);
            List<string> toks = ParseList(")");

            if (toks.Count > position)
                toks[position] = "nullif(" + toks[position] + ", 0)"; // return null if zero

            return AppendList(tok, toks);
        }

        /// <summary>
        /// Get next token properly handling minus chars in numbers including scientific notation
        /// </summary>
        /// <param name="lex"></param>
        /// <returns></returns>

        string GetToken(Lex lex)
        {
            PositionedToken pt = lex.GetPositionedToken();
            if (pt == null || pt.Text == "") return "";

            if (pt.Text == "-") // check for simple negative number
            {
                PositionedToken pt2 = lex.GetPositionedToken();
                if (pt2 == null || pt2.Text == "") return pt.Text;

                else if (pt2 != null &&
                    pt2.Position == pt.Position + pt.Length &&
                    Lex.IsDouble(pt2.Text))
                {
                    return pt.Text + pt2.Text;
                }

                else
                {
                    lex.Backup();
                    return pt.Text;
                }
            }

            else // check for scientific notation (e.g. 1E-3)
            {
                PositionedToken pt2 = lex.GetPositionedToken();
                if (pt2 == null || pt2.Text == "") return pt.Text;

                else if (pt2.Text != "-" || pt2.Position != pt.Position + pt.Length)
                {
                    lex.Backup();
                    return pt.Text;
                }

                PositionedToken pt3 = lex.GetPositionedToken();
                if (pt3 == null || pt3.Text == "")
                {
                    lex.Backup();
                    return pt.Text;
                }

                else if (pt3.Position == pt2.Position + pt2.Length &&
                    Lex.IsDouble(pt.Text + pt2.Text + pt3.Text))
                {
                    return pt.Text + pt2.Text + pt3.Text;
                }

                else
                {
                    lex.Backup(); // for pt3
                    lex.Backup(); // for pt2;
                    return pt.Text;
                }
            }
        }

        /// <summary>
        /// Append a list of tokens or expressions
        /// </summary>
        /// <param name="toks"></param>
        /// <returns></returns>

        string AppendList(
            string expr,
            List<string> toks)
        {
            foreach (string s in toks)
                expr = AppendToken(expr, s);
            return expr;
        }

        /// <summary>
        /// Append a token avoiding the breakup of negative numbers (doesn't work for scientific notation)
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="tok"></param>
        /// <returns></returns>

        string AppendToken(
                string expr,
                string tok)
        {
            if (tok == "") return expr;

            //bool lastTokenWasMinus =
            //  expr != "" && expr[expr.Length - 1] == '-';

            //if (lastTokenWasMinus && Char.IsDigit(tok[0])) { } // don't split negative numbers
            //else 
            if (expr != "") expr += " ";
            expr += tok;
            return expr;
        }

    }

    /// <summary>
    /// Internal class of mapping of CF input cols to data from other tables in the query
    /// </summary>

    internal class OtherDataMap
    {
        public int MappedCount = 0;
        public int UnmappedCount = 0;
        public MetaColumn FirstUnmappedInputMc = null; // first unmapped MetaColumn from input MetaColumns in other tables
        public MetaColumn FirstUnmappedCfMc = null; // first unmapped MetaColumn from input MetaColumns in CF table
        public int FirstUnmappedPos = -1; // index of first unmapped column in list of inputs
        public bool SingleUnmapped = false; // true if only a single input column could not be mapped to other table
        public QueryColumn[] InputColMap = null; // map from input col index to corresponding data col in other table
        public QueryColumn[] SelectedColMap = null; // map from selected col index to corresponding data col in other table
        public bool IsMappable = false; // if true table's data can be mapped from data retrieved by other tables in the query
    }

}
