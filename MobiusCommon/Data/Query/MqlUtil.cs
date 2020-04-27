using Mobius.ComOps;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Mobius.Data
{
	/// <summary>
	/// Mobius Query Language (MQL) Utilities
	/// </summary>

	public class MqlUtil
	{
		/// <summary>
		/// An array of all the key words available in Oracle.
		/// </summary>

		public static string[] KeyWords = { // MQL key words
			"select", "*", "from", "order", "by",
			"=", "<", "<=", ">", ">=", "<>", "between",
			"is", "not", "null", "not", "in", "saved", "list",
			"like", "escape", "sss", "fss", "molsim", "smallworld", "relatedss",
			"fmla_eq", "fmla_like",
			"and", "or", "not",
			"(", ")", ",",
			"lower", "substr", "upper", "instr", "length",
			"to_char", "to_date", "to_number",
			"sysdate", "months_between",
			"abs", "ceil", "cos", "cosh", "exp", "floor", "ln", "log",
			"mod", "power", "round", "sin", "sinh", "sqrt",
			"tan", "tanh", "trunc", "-" };

		public static string[] Delimiters =
		{",", ";", "(", ")", "<", "=", ">", "<=", ">=", "<>", "!=", "!>", "!<", "+", "-", "*", "/"};

		public static string[] BinaryComparisonOperators =
			{ "=", "<", "<=", ">", ">=", "<>" };

		/// <summary>
		/// Return true if we are executing the query in a single step
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		public static bool SingleStepExecution(
			Query q)
		{
			bool singleStepExecution = q.SingleStepExecution || DefaultToSingleStepQueryExecution;
			return singleStepExecution;
		}

		public static bool DefaultToSingleStepQueryExecution = false; // true to default to single-step execution for all queries

		/// <summary>
		/// Convert complex criteria to QueryColumn criteria of specified type (may lose logic)
		/// </summary>
		/// <param name="q"></param>
		/// <param name="logicType"></param>

		public static void ConvertComplexCriteriaToQueryColumnCriteria(
			Query q,
			QueryLogicType logicType)
		{
			q.LogicType = logicType;

			List<MqlToken> toks = MqlUtil.ParseComplexCriteria(q.ComplexCriteria, q);
			q.ClearAllQueryColumnCriteria();
			for (int ti = 0; ti < toks.Count; ti++)
			{
				MqlToken tok = toks[ti];
				QueryColumn qc = tok.Qc;
				if (qc == null) continue; // only try to convert starting at recognized query columns
				if (qc.Criteria != "") continue; // don't do if already have

				string condition = // get criteria from querycolumn forward
					q.ComplexCriteria.Substring(tok.Tok.Position);

				if (qc.MetaColumn.DataType == MetaColumnType.Structure ||
					qc.MetaColumn.DataType == MetaColumnType.MolFormula)
				{ // structure searches are handled via function syntax
					if (ti < 2) continue; // need to backup 2 tokens
					condition = q.ComplexCriteria.Substring(toks[ti - 2].Tok.Position);
				}

				ParsedSingleCriteria psc = null;
				try
				{ psc = ParseSingleCriteria(condition); }
				catch (Exception ex)
				{ continue; }

				if (psc == null) continue;

				ConvertParsedSingleCriteriaToQueryColumnCriteria(psc, qc);

				if (qc.IsKey)
				{ // if key then move to key criteria
					if (Lex.IsUndefined(q.KeyCriteria)) // do only if don't already have
						qc.CopyCriteriaToQueryKeyCritera(q);

					qc.Criteria = qc.CriteriaDisplay = "";
				}
			}

			return;
		}

		/// <summary>
		/// Format a ParsedSingleCriteria into a QueryColumn criteria string
		/// </summary>

		public static void ConvertParsedSingleCriteriaToQueryColumnCriteria(
			ParsedSingleCriteria psc,
			QueryColumn qc,
			bool includeColNameInCriteria = true)
		{
			string tok;

			if (psc == null || qc == null) return;

			MetaColumn mc = qc.MetaColumn;
			string mcName = "";
			if (includeColNameInCriteria) mcName = mc.Name + " "; // name is normally included except for Query.KeyCriteria

			if (Lex.IsNullOrEmpty(psc.Op))
			{
				qc.Criteria = qc.CriteriaDisplay = "";
				return;
			}

			qc.Criteria = mcName + psc.Op;
			qc.CriteriaDisplay = psc.Op;

			if (Lex.Eq(psc.Op, "is null") || Lex.Eq(psc.Op, "is not null")) return; // no values for null comparison

			qc.Criteria += " ";
			qc.CriteriaDisplay += " ";

			if (Lex.Eq(psc.Op, "in") || Lex.Eq(psc.Op, "not in")) // list
			{
				bool addQuotes = (mc.DataType == MetaColumnType.String);
				string valueListString = FormatValueListString(psc.ValueList, addQuotes);
				qc.Criteria += "(" + valueListString + ")";

				if (mc.DataType == MetaColumnType.CompoundId)
					qc.CriteriaDisplay += CidList.FormatAbbreviatedCidListForDisplay(qc, valueListString);
				else
					qc.CriteriaDisplay += FormatValueListString(psc.ValueList, false);
				return;
			}

			else if (qc.MetaColumn.DataType == MetaColumnType.Structure)
			{ // special conversion for structure criteria
				if (Lex.Eq(psc.Op, "sss"))
					qc.Criteria = mcName + "sss squery";

				else if (Lex.Eq(psc.Op, "fss"))
					qc.Criteria = mcName + "fss squery " +
						Lex.AddSingleQuotes(psc.Value2); // full structure search parameters

				else if (Lex.Eq(psc.Op, "molsim"))
					qc.Criteria = mcName + "msimilar squery " +
						Lex.AddSingleQuotes(psc.Value2); // similarity type & cutoff value

				else if (Lex.Eq(psc.Op, "smallworld"))
					qc.Criteria = mcName + "smallworld squery " +
						Lex.AddSingleQuotes(psc.Value2); // smallworld parameters

				else if (Lex.Eq(psc.Op, "relatedss"))
					qc.Criteria = mcName + "relatedss squery " +
						Lex.AddSingleQuotes(psc.Value2); // related search parameters

				qc.MolString = MoleculeMx.ChimeStringToMolFile(psc.Value);
				return;
			}

			else if (qc.MetaColumn.DataType == MetaColumnType.MolFormula)
			{ // special conversion for molformula criteria
				qc.Criteria = mcName + psc.Op + " " + Lex.AddSingleQuotes(psc.Value);
				qc.CriteriaDisplay = psc.Op + " " + Lex.AddSingleQuotes(psc.Value);
				return;
			}

			tok = psc.Value;
			qc.CriteriaDisplay += tok;
			if (mc.DataType == MetaColumnType.String) tok = Lex.AddSingleQuotes(tok);
			qc.Criteria += tok;

			if (Lex.Ne(psc.Op, "between")) return;

			qc.Criteria += " and ";
			qc.CriteriaDisplay += " and ";

			tok = psc.Value2;
			qc.CriteriaDisplay += tok;
			if (mc.DataType == MetaColumnType.String) tok = Lex.AddSingleQuotes(tok);
			qc.Criteria += tok;

			return;
		}

		/// <summary>
		/// Format a value list into a string with items separated by commas
		/// </summary>
		/// <param name="psc"></param>
		/// <param name="quoteValues"></param>

		public static string FormatValueListString(
			List<string> list,
			bool quoteValues)
		{
			string vls = "";
			for (int i1 = 0; i1 < list.Count; i1++)
			{
				if (vls != "") vls += ", ";
				if (!quoteValues) vls += list[i1];
				else vls += Lex.AddSingleQuotes(list[i1]);
			}

			return vls;
		}

		/// <summary>
		/// Convert a Query object to a Mql statement
		/// </summary>

		public static string ConvertQueryToMql(
			Query q, bool mobileQuery = false)
		{
			MetaTable mt;
			MetaColumn mc;
			string expr, tName, cName, tok;
			int qti;

			if (q == null || q.Tables == null || q.Tables.Count == 0)
				throw new QueryException("Query not defined");

			q.IncludeRootTableAsNeeded(); // be sure we have a root

			q.AssignUndefinedAliases(); // be sure we have an alias for each table

			StringBuilder exprs = new StringBuilder();
			StringBuilder tables = new StringBuilder();
			foreach (QueryTable qt in q.Tables)
			{
				mt = qt.MetaTable;

				if (tables.Length > 0) tables.Append(", ");
				tables.Append(qt.MetaTable.Name);
				//				if (qt.Summarized) tables.Append(MetaTable.SummarySuffix); // summarized table (obsolete)
				tables.Append(" " + qt.Alias);

				foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (qc.Selected)
					{
						if (exprs.Length > 0) exprs.Append(", ");
						exprs.Append(qt.Alias + "." + qc.MetaColumn.Name);
					}
				}

			} // end of querytable loop

			if (exprs.Length == 0) throw new QueryException("No columns selected");
			string mql = "Select " + exprs.ToString() +
				" From " + tables.ToString();

			string criteria = GetCriteriaString(q, mobileQuery);
			if (criteria != "") mql += " Where " + criteria;

			string orderBy = GetOrderByString(q);
			if (orderBy != "") mql += " Order By " + orderBy;

			return mql;
		}


		/// <summary>
		/// Convert all QueryColumn criteria in a query to a single complex criteria
		/// </summary>
		/// <param name="q"></param>

		public static void ConvertQueryColumnCriteriaToComplexCriteria(
			Query q)
		{
			if (q.LogicType == QueryLogicType.Complex) return; // already done
			q.ComplexCriteria = GetCriteriaString(q);
			q.LogicType = QueryLogicType.Complex;
		}

		/// <summary>
		/// Get Mql criteria expression string for query. Build the string
		/// if simple QueryColumn criteria or just retrieve if complex.
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		public static string GetCriteriaString(
			Query q,
			bool mobileQuery = false)
		{
			if (q.LogicType == QueryLogicType.Complex) // if complex then return as is
				return q.ComplexCriteria;

			string criteria = "";
			bool nonBaseCriteria = false; // set to true if see non-key criteria
			bool baseCriteria = false;

			// Insert any base criteria first (key, dbSet)

			if (q.Tables == null || q.Tables.Count == 0) return "";

			QueryTable qt = q.Tables[0]; // look in 1st query table for list/dbset criteria

			// The first table may not allow KeyCriteria because it may have a NOT EXISTS criteria.
			// If so find the first table without "not exists" criteria

			foreach (QueryTable queryTable in q.Tables)
			{
				if (queryTable.HasNormalDatabaseCriteria)
				{
					qt = queryTable;
					break;
				}
			}

			foreach (QueryColumn qc in qt.QueryColumns)
			{
				if (qc.IsKey && q.KeyCriteria != "")
				{
					string saveCriteria = qc.Criteria;
					qc.Criteria = qc.MetaColumn.Name + " " + q.KeyCriteria; // build for temp use

					if (criteria != "") criteria += " and ";
					if (mobileQuery)
					{
						criteria += qc.MetaColumn.Name + " " + @"= '########'"; // CorpId to be filled in by mobile app
					}
					else
					{
						criteria += ConvertSingleQueryColumnCriteriaToMql(qc);
					}
					qc.Criteria = saveCriteria; // restore
					baseCriteria = true;
				}

				else if (qc.IsKey && mobileQuery)
				{
					if (criteria != "") criteria += " and ";
					criteria += qc.MetaColumn.Name + " " + @"= '########'"; // CorpId to be filled in by mobile app 
					baseCriteria = true;
				}

				else if (qc.MetaColumn.IsDatabaseSetColumn && qc.Criteria != "")
				{
					if (criteria != "") criteria += " and ";
					criteria += ConvertSingleQueryColumnCriteriaToMql(qc);
					baseCriteria = true;
				}
			}

			// Parse tables adding non-base criteria

			q.AssignUndefinedAliases(); // be sure we have aliases defined 
			for (int qti = 0; qti < q.Tables.Count; qti++)
			{
				qt = q.Tables[qti];

				MetaTable mt = qt.MetaTable;

				if (mt.IgnoreCriteria) continue; // ignore these criteria?

				foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (qc.Criteria == "" ||
						qc.IsKey || // already processed these
						qc.MetaColumn.IsDatabaseSetColumn) continue;

					if (criteria != "")
					{ // include proper logical operator between criteria 
						if (q.LogicType == QueryLogicType.Or && baseCriteria && !nonBaseCriteria)
						{ // if "or" logic with key criteria then group "Or" criteria within parens
							criteria += " and ("; // and "And" with key criteria
						}

						else if (q.LogicType == QueryLogicType.Or) criteria += " or ";
						else criteria += " and ";

						nonBaseCriteria = true;
					}

					criteria += ConvertSingleQueryColumnCriteriaToMql(qc);

				}
			}

			if (q.LogicType == QueryLogicType.Or && // close out "or" grouping
				baseCriteria && nonBaseCriteria)
			{
				criteria += ")";
			}

			return criteria;
		}

		/// <summary>
		/// Generate Mql for a single QueryColumn criteria
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="mc"></param>
		/// <param name="criteria"></param>

		public static string ConvertSingleQueryColumnCriteriaToMql(
			QueryColumn qc)
		{
			string colCriteria, txt, tok;
			bool normalizeCaseCompare = false; // string conversion flag

			MetaColumn mc = qc.MetaColumn;
			string criteriaColName = qc.QueryTable.Alias + "." + mc.Name;

			if (qc.Criteria.ToLower().IndexOf("is null") >= 0 ||
				qc.Criteria.ToLower().IndexOf("is not null") >= 0)
			{ // is null, is not null criteria ok as is
				colCriteria = qc.Criteria;
				colCriteria = colCriteria.Replace(mc.Name, criteriaColName); // map to proper comparison col as necessary
				return colCriteria;
			}

			else if (qc.MetaColumn.DataType == MetaColumnType.Structure)
			{ // special fixup for structure
				colCriteria = ConvertQueryColumnStructureCriteriaToMql(qc);
				colCriteria = colCriteria.Replace("(ctab,", "(" + criteriaColName + ",");
				return colCriteria;
			}

			colCriteria = criteriaColName; // build criteria for column here starting with column name
			ParsedSingleCriteria sc = ParseSingleCriteria(qc.Criteria);
			if (sc == null) return "";
			string op = sc.Op;
			string v1 = sc.Value;
			string v2 = sc.Value2;

			if (qc.MetaColumn.DataType == MetaColumnType.MolFormula)
			{ // put in more proper sql format
				colCriteria = op + "(" + criteriaColName + ", " + Lex.AddSingleQuotes(v1) + ") = 1";
				return colCriteria;
			}

			//if (mc.IsKey && Lex.Eq(op, "IN LIST"))
			//{
			//  UserObject uo = ResolveCnListReference(v1);
			//  if (uo != null) tok = "CNLIST_" + uo.Id.ToString();
			//  else tok = "Nonexistant list";
			//}

			if (mc.DataType == MetaColumnType.Date)
			{
				if (op == "=" || Lex.Eq(op, "Between"))
				{ // use between on date equality to catch all times during the day
					if (v2.Length == 0) v2 = v1;
					v1 = "to_date('" + DateTimeMx.Format(v1) + "','DD-MON-YYYY')";
					v2 = "to_date('" + DateTimeMx.Format(v2) + " 235959','DD-MON-YYYY HH24MISS')"; // go to end of day
					colCriteria = criteriaColName + " between " + v1 + " and " + v2;
					return colCriteria;
				}

				else if (Lex.Eq(op, "Within"))
				{ // build within expression enclosing in a function so can be properly disabled when necessary
					if (Lex.Contains(v2, "Day"))
						colCriteria = "to_number(trunc(sysdate) - trunc(" + criteriaColName + ")) <= " + v1;

					else if (Lex.Contains(v2, "Week"))
						colCriteria = "to_number((trunc(sysdate) - trunc(" + criteriaColName + "))/7) <= " + v1;

					else if (Lex.Contains(v2, "Month"))
						colCriteria = "months_between(trunc(sysdate), trunc(" + criteriaColName + ")) <= " + v1;

					else if (Lex.Contains(v2, "Year"))
						colCriteria = "to_number(months_between(trunc(sysdate), trunc(" + criteriaColName + "))/12) <= " + v1;

					else throw new QueryException("Invalid Within units: " + v2);

					return colCriteria;
				}
			}

			if (mc.DataType == MetaColumnType.String)
			{ // see if we should normalize case for comparisons (may result in loss of index use)
				if (mc.TextCase == ColumnTextCaseEnum.Unknown ||
				 mc.TextCase == ColumnTextCaseEnum.Mixed)
				{
					normalizeCaseCompare = mc.IgnoreCase;
				}

				if (mc.IsDatabaseSetColumn ||  // avoid application of UPPER function to structure database set columns
				 mc.IsKey) // also for key since causes problem for parsing MQL (but may miss lowercase ids)
					normalizeCaseCompare = false;

				if (normalizeCaseCompare) colCriteria = "upper(" + colCriteria + ")";
			}

			if (Lex.Eq(op, "In") || Lex.Eq(op, "Not In"))
			{
				string sectionLogic = "or";
				if (Lex.Eq(op, "Not In")) sectionLogic = "and";

				int sections = 0;
				StringBuilder sb = new StringBuilder();

				string colNameExpr = colCriteria; // expression for column name

				int sectionSize = 1000; // break into sections of 1000 each to avoid Oracle list size limitation
				if (mc.IsKey) sectionSize = -1; // don't need to break key values into sections since Mobius QE will do it

				for (int vi = 0; vi < sc.ValueList.Count; vi++)
				{
					if (sections == 0 || (sectionSize > 0 && vi % sectionSize == 0))
					{
						sections++;
						if (sections > 1) sb.Append(") " + sectionLogic + " ");
						sb.Append(colNameExpr + " " + op + " (");
					}

					else sb.Append(",");

					string s = sc.ValueList[vi];
					s = ConvertSingleCriteriaToSql(s, mc, normalizeCaseCompare);
					sb.Append(s);
				}

				sb.Append(")"); // close last section
				colCriteria = sb.ToString();
				if (sections > 1) colCriteria = "(" + colCriteria + ")"; // if multiple sections put "ors" in parens
			}

			else if (Lex.Eq(op, "Between"))
			{
				colCriteria +=
					" between " +
					ConvertSingleCriteriaToSql(v1, mc, normalizeCaseCompare) +
					" and " +
					ConvertSingleCriteriaToSql(v2, mc, normalizeCaseCompare);
			}

			else // other binary operators
			{
				colCriteria +=
					" " + op + " " +
					ConvertSingleCriteriaToSql(v1, mc, normalizeCaseCompare);
			}

			return colCriteria;
		}

		/// <summary>
		/// Parse simple single column criteria string into individual fields
		/// </summary>
		/// <param name="condition"></param>
		/// <returns></returns>

		public static ParsedSingleCriteria ParseQueryColumnCriteria(
			QueryColumn qc)
		{
			ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(qc.Criteria);
			if (psc == null) return null;

			psc.QueryColumn = qc; // store associated query column

			if (qc.MetaColumn.DataType == MetaColumnType.Structure)
			{ // copy over structure if structure query
				if (qc.MolString.IndexOf("V2000") >= 0 || qc.MolString.IndexOf("V3000") >= 0) // convert molfile to chime string to avoid length overflow within oracle
					psc.Value = MoleculeMx.MolFileToChimeString(qc.MolString);
				else psc.Value = qc.MolString; // just use value as is
			}
			return psc;
		}

		/// Parse a QueryColumn criteria string into individual fields
		/// Criteria forms:
		/// 1. field unary-op value
		/// 2. field between v1 and v2
		/// 3. field [not] in (v1,v2,v3...)
		/// 4. field is [not] null
		/// 5. (field is not null or field is null)
		/// 6. field [not] in (...)
		/// 7. field in list list-name
		/// 8. field like value
		/// 9. upper(field)..., lower(field)...
		/// 10. field between to_date('4-Oct-1995','DD-MON-YYYY') and to_date('4-Oct-1995 235959','DD-MON-YYYY HH24MISS') 
		/// 11. structCol sss squery (Simple criteria formats)
		/// 12. structCol isomer | tautomer | parent squery
		/// 13. structCol fss squery 'flags'
		/// 14. structCol msimilar squery 'normal | sub | super sim-score'
		/// 15. molsim(structCol,'chime','normal | sub | super') >= sim-score (Mql - Chem cartridge formats)
		/// 17. sss(structCol,'chime') = 1

		public static ParsedSingleCriteria ParseSingleCriteria(
			string criteria)
		{
			ParsedSingleCriteria sc = new ParsedSingleCriteria();
			if (String.IsNullOrEmpty(criteria)) return null;

			if (criteria.ToLower().IndexOf("is not null or") > 0 &&
				 criteria.ToLower().IndexOf("is null") > 0)
			{ // special case for not null or null
				sc.Op = "is not null or is null";
				return sc;
			}

			ArrayList toks = new ArrayList();
			string tok;
			Lex lex = new Lex();
			lex.SetDelimiters(" , ; ( ) < = > <= >= <> != !> !<");
			lex.OpenString(criteria);

			tok = lex.GetLower(); // get col name or function name

			if (tok == "sss" || tok == "fss" || tok == "molsim" || tok == "smallworld" || tok == "relatedss" ||
				tok == "fmla_eq" || tok == "fmla_like")
			{ // mql function-style structure or formula search criteria (i.e. not a QueryColumn criteria)
				sc.Op = tok;
				tok = lex.Get(); // left paren
				tok = lex.Get(); // field name
				tok = lex.Get(); // comma
				sc.Value = Lex.RemoveAllQuotes(lex.Get()); // structure chime string or formula

				if (sc.Op == "sss")
				{
					sc.OpEnum = CompareOp.SSS; // store op, options don't appear here
					tok = lex.Get(); // comma
					sc.Value2 = Lex.RemoveAllQuotes(lex.Get()); // parameters
				}

				else if (sc.Op == "FSS")
				{
					sc.OpEnum = CompareOp.FSS;
					tok = lex.Get(); // comma
					sc.Value2 = Lex.RemoveAllQuotes(lex.Get()); // parameters
				}

				else if (sc.Op == "molsim")
				{
					sc.OpEnum = CompareOp.MolSim;
					tok = lex.Get(); // comma
					sc.Value2 = Lex.RemoveAllQuotes(lex.Get()); // type of similarity
					tok = lex.Get(); // right paren
					tok = lex.Get(); // comparison operator (assumed to be >= )
					tok = lex.Get(); // value
					if (tok != "") sc.Value2 += " " + tok;
				}

				else if (sc.Op == "smallworld")
				{
					sc.OpEnum = CompareOp.SmallWorld;
					tok = lex.Get(); // comma
					sc.Value2 = Lex.RemoveAllQuotes(lex.Get()); // search parameters
					tok = lex.Get(); // right paren
					tok = lex.Get(); // comparison operator (assumed to be = )
					tok = lex.Get(); // value
					if (tok != "") sc.Value2 += " " + tok;
				}

				else if (sc.Op == "relatedss")
				{
					sc.OpEnum = CompareOp.RelatedSS;
					tok = lex.Get(); // comma
					sc.Value2 = Lex.RemoveAllQuotes(lex.Get()); // search parameters
					tok = lex.Get(); // right paren
					tok = lex.Get(); // comparison operator (assumed to be = )
					tok = lex.Get(); // value
					if (tok != "") sc.Value2 += " " + tok;
				}

				else if (tok == "fmla_eq") sc.OpEnum = CompareOp.FormulaEqual;
				else if (tok == "fmla_like") sc.OpEnum = CompareOp.FormulaLike;

				return sc;
			}

			sc.Op = lex.Get().ToLower();
			if (sc.Op == ")") // possible close of function call
				sc.Op = lex.Get().ToLower();

			if (IsBasicComparisonOperator(sc.Op) || sc.Op == "like")
			{
				if (sc.Op == "=") sc.OpEnum = CompareOp.Eq;
				else if (sc.Op == "<>") sc.OpEnum = CompareOp.Ne;
				else if (sc.Op == "<") sc.OpEnum = CompareOp.Lt;
				else if (sc.Op == "<=") sc.OpEnum = CompareOp.Le;
				else if (sc.Op == ">") sc.OpEnum = CompareOp.Gt;
				else if (sc.Op == ">=") sc.OpEnum = CompareOp.Ge;
				else if (sc.Op == "like") sc.OpEnum = CompareOp.Like;

				tok = GetNextNonKeyWord(lex);
				if (tok == "") return null;
				sc.Value = Lex.RemoveAllQuotes(tok);
			}

			else if (Lex.Eq(sc.Op, "in"))
			{ // todo: enhance to handle more complex lists, e.g. (to_date('4-Oct-1995','DD-MON-YYYY'), ...)
				tok = lex.Get(); // left paren or "list"
				if (tok == "(")
				{
					sc.OpEnum = CompareOp.In;
					sc.ValueList = new List<string>();
					while (true)
					{
						tok = lex.Get();
						if (tok == "" || tok == ")") break;
						if (IsKeyWord(tok)) continue; // ignore commas & other keywords
						sc.ValueList.Add(Lex.RemoveAllQuotes(tok));
					}
				}

				else if (Lex.Eq(tok, "list"))
				{
					sc.Op = "in list";
					sc.OpEnum = CompareOp.InList;
					sc.Value = lex.Get();
				}

				return sc;
			}

			else if (Lex.Eq(sc.Op, "is"))
			{
				string tok2 = lex.GetLower();
				if (tok2 == "null")
				{
					sc.Op = "is null";
					sc.OpEnum = CompareOp.IsNull;
					return sc;
				}

				string tok3 = lex.GetLower();
				if (tok2 == "not" && tok3 == "null")
				{
					sc.Op = "is not null";
					sc.OpEnum = CompareOp.IsNotNull;
					return sc;
				}

				return null; // not recognized
			}

			else if (Lex.Eq(sc.Op, "between"))
			{
				sc.OpEnum = CompareOp.Between;
				tok = GetNextNonKeyWord(lex);
				if (tok == "") return null;
				sc.Value = Lex.RemoveAllQuotes(tok);

				while (true)
				{
					tok = lex.Get(); // get "and"
					if (Lex.Eq(tok, "and")) break;
					if (tok == "") return null;
				}

				tok = GetNextNonKeyWord(lex);
				if (tok == "") return null;
				sc.Value2 = Lex.RemoveAllQuotes(tok);
			}

			else if (Lex.Eq(sc.Op, "within"))
			{
				sc.OpEnum = CompareOp.Within;
				sc.Value = Lex.RemoveAllQuotes(lex.Get());
				sc.Value2 = Lex.RemoveAllQuotes(lex.Get());
			}

			else if (Lex.Eq(sc.Op, "sss"))
			{
				sc.OpEnum = CompareOp.SSS;
				sc.Value = Lex.RemoveAllQuotes(lex.Get());
				sc.Value2 = Lex.RemoveAllQuotes(lex.Get()); // get any SS options
			}

			else if (Lex.Eq(sc.Op, "fss") || Lex.Eq(sc.Op, "isomer") ||
				Lex.Eq(sc.Op, "tautomer") || Lex.Eq(sc.Op, "parent"))
			{ // get any structure search options 
				sc.Value = Lex.RemoveAllQuotes(lex.Get());
				sc.Value2 = Lex.RemoveAllQuotes(lex.Get());
				sc.OpEnum = CompareOp.FSS;
			}

			else if (Lex.Eq(sc.Op, "msimilar") || Lex.Eq(sc.Op, "molsim"))
			{ // get any structure search options
				sc.Value = Lex.RemoveAllQuotes(lex.Get());
				sc.Value2 = Lex.RemoveAllQuotes(lex.Get());
				sc.OpEnum = CompareOp.MolSim;
			}

			else if (Lex.Eq(sc.Op, "smallworld"))
			{ // get any structure search options
				sc.Value = Lex.RemoveAllQuotes(lex.Get()); // structure
				sc.Value2 = Lex.RemoveAllQuotes(lex.Get()); // options
				sc.OpEnum = CompareOp.SmallWorld;
			}

			else if (Lex.Eq(sc.Op, "relatedss"))
			{ // get any structure search options
				sc.Value = Lex.RemoveAllQuotes(lex.Get()); // structure
				sc.Value2 = Lex.RemoveAllQuotes(lex.Get()); // options
				sc.OpEnum = CompareOp.RelatedSS;
			}

			else if (Lex.Eq(sc.Op, "fmla_eq"))
			{
				sc.Value = Lex.RemoveAllQuotes(lex.Get());
				sc.OpEnum = CompareOp.FormulaEqual;
			}

			else if (Lex.Eq(sc.Op, "fmla_like"))
			{
				sc.Value = Lex.RemoveAllQuotes(lex.Get());
				sc.OpEnum = CompareOp.FormulaLike;
			}

			else return null;

			return sc;
		}

		/// <summary>
		/// Convert a QueryColumn structure search criteria to Mql
		/// Note that the field name is left as "ctab"
		/// </summary>
		/// <param name="qc"></param>

		public static string ConvertQueryColumnStructureCriteriaToMql(
			QueryColumn qc)
		{
			string criteria = null, chunk;
			int i1;

			ParsedSingleCriteria psc = MqlUtil.ParseQueryColumnCriteria(qc);
			if (psc == null) return null;

			QueryTable qt = qc.QueryTable;
			MetaTable mt = qt.MetaTable;
			MetaColumn mc = qc.MetaColumn;
			ParsedStructureCriteria pssc = ParsedStructureCriteria.ConvertFromPscToPssc(psc);
			MoleculeMx qs = pssc.Molecule; // query structure
			string chimeString = qs.GetChimeString();
			string qss = ConvertStringToValidOracleExpression(chimeString);

			// Set criteria based on operator

			if (pssc.SearchType == StructureSearchType.Substructure)
			{
				criteria = "sss(ctab," + qss + ") = 1";
			}

			else if (pssc.SearchType == StructureSearchType.MolSim)
			{
				if (qs.ContainsQueryFeature)
					throw new UserQueryException("The query structure contains one or more features that are allowed in substructure queries only");

				string simType = "Normal"; // default to normal similarity
				if (pssc.SimilarityType != SimilaritySearchType.Unknown)
					simType = pssc.SimilarityType.ToString();

				double minSim = .75; // default minimum similarity
				if (pssc.MinimumSimilarity > 0) minSim = pssc.MinimumSimilarity;
				if (minSim > 10) minSim = minSim / 100; // convert old 1-100 range to 0-1

				minSim = (int)(minSim * 100 + .5); //  convert internal 0-1 range to cartridge 1-100 range

				criteria = "MolSim(ctab," + qss + ",'" + simType + "') >= " + minSim.ToString();
			}

			else if (pssc.SearchType == StructureSearchType.FullStructure)
			{
				if (qs.ContainsQueryFeature)
					throw new UserQueryException("The query structure contains one or more features that are allowed in substructure queries only");

				if (String.IsNullOrEmpty(pssc.Options)) pssc.Options = "None"; // no parameters turned on (avoid empty string)
				criteria = "F_l_e_x_Match(ctab," + qss + ",'" + pssc.Options + "') = 1";
			}

			else if (pssc.SearchType == StructureSearchType.SmallWorld)
			{
				if (qs.ContainsQueryFeature)
					throw new UserQueryException("The query structure contains one or more features that are allowed in substructure queries only");

				if (pssc.SmallWorldParameters == null) pssc.SmallWorldParameters = new SmallWorldPredefinedParameters(); // shouldn't happen
				string smiles = qs.GetSmilesString(); // include smiles string
				criteria = "SmallWorld(" + qt.Alias + "." + mc.Name + ", '" + smiles + "', '" + pssc.SmallWorldParameters.Serialize() + "') = 1";
			}

			else if (pssc.SearchType == StructureSearchType.Related)
			{
				if (qs.ContainsQueryFeature)
					throw new UserQueryException("The query structure contains one or more features that are allowed in substructure queries only");

				if (pssc.SmallWorldParameters == null) pssc.SmallWorldParameters = new SmallWorldPredefinedParameters(); // shouldn't happen
				string smiles = qs.GetSmilesString(); // include smiles string
				criteria = "RelatedSS(" + qt.Alias + "." + mc.Name + ", '" + smiles + "', '" + pssc.Options + "') = 1";
			}

			else throw new QueryException("Invalid structure search type: " + psc.Op);

			return criteria;
		}

		/// <summary>
		/// Build a n Oracle literal string of any size
		/// If the string length is less or equal to 4000 just return as a
		/// single-quoted string otherwise break into chunks and return as a clob 
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static string ConvertStringToValidOracleExpression(string s)
		{
			string s2, chunk;

			if (s.Length <= 4000) return Lex.AddSingleQuotes(s); // just quote if not longer than 4000

			s2 = "to_clob(" + Lex.AddSingleQuotes(s.Substring(0, 4000)) + ")"; // convert first chunk to clob expression
			s = s.Substring(4000);

			while (s.Length > 0) // use catenate function to add remaining chunks
			{
				if (s.Length > 4000)
				{
					chunk = s.Substring(0, 4000);
					s = s.Substring(4000);
				}
				else // get rest
				{
					chunk = s;
					s = "";
				}

				s2 = "concat(" + s2 + ", " + Lex.AddSingleQuotes(chunk) + ")";
			}

			return s2;
		}

		/// <summary>
		/// Convert a single criteria value for use in SQL statement
		/// </summary>
		/// <param name="val"></param>
		/// <param name="mc"></param>
		/// <returns></returns>

		static string ConvertSingleCriteriaToSql(
			string val,
			MetaColumn mc,
			bool normalizeCaseCompare)
		{

			string fVal = ""; // build formatted value here

			if (mc.DataType == MetaColumnType.CompoundId)
			{
				if (!Lex.IsQuoted(val, '\''))
					fVal = Lex.AddSingleQuotes(val);
				else fVal = val;
			}

			else if (mc.IsDatabaseSetColumn)
			{
				fVal = Lex.AddSingleQuotes(val);
			}

			else if (mc.DataType == MetaColumnType.MolFormula)
			{
				fVal = Lex.AddSingleQuotes(val);
			}

			else if (mc.DataType == MetaColumnType.String)
			{
				fVal = Lex.AddSingleQuotes(val);

				if (mc.TextCase == ColumnTextCaseEnum.Upper) // match upper in db 
					fVal = fVal.ToUpper();

				else if (mc.TextCase == ColumnTextCaseEnum.Lower) // match lower in db
					fVal = fVal.ToLower();

				else if (!normalizeCaseCompare) { } // need to normalize

				else fVal = fVal.ToUpper(); // unknown case, convert to upper for match (also converting db field)
			}

			else if (mc.DataType == MetaColumnType.Integer ||
				mc.DataType == MetaColumnType.Number)
				fVal = val; // no further processing on numbers

			else if (mc.DataType == MetaColumnType.Date)
				fVal = "to_date('" + DateTimeMx.Format(val) + "','DD-MON-YYYY')";

			else if (mc.DataType == MetaColumnType.DictionaryId)
				fVal = Lex.AddSingleQuotes(val); // quote value in case database column is character

			else fVal = val; // other types

			return fVal;
		}

		/// <summary>
		/// Extract the criteria that applies to a single table from full query criteria
		/// </summary>
		/// <param name="q"></param>
		/// <param name="qt"></param>
		/// <returns></returns>

		public static string ExtractSingleTableCriteria(   
			List<MqlToken> critToks, 
			Query query,
			QueryTable qt)
		{
			MetaTable mt = qt.MetaTable;

			if (mt.Parent == null && !SingleStepExecution(query))
				return "(1=1)"; // no need to further filter root tables

			if (!query.FilterResults) // no results filtering
				return "(1=1)";

			List<MqlToken> critToks2 = // disable non-retrieval criteria
			 MqlUtil.DisableCriteria(critToks, query, qt, null, null, null, false, true, true);

			// Remove structure search criteria if root table since not necessary.
			//
			// The following cases of structure searches are possible.
			// 1. Chem cartridge structure in root table - Remove
			// 2. Chem cartridge structure in non-root table - Keep
			// 3. User Compound Database table in root table - Remove
			// 4. Annotation table structure in non-root - Should Keep but currently Removes
			// 5. General Oracle table structure in non-root - Should Keep but currently Removes
			//
			// Note that this currently also removes criteria for cases 5 & 6
			// because keeping the criteria will throw an exception. This can
			// result in the return of mismatching records if multiple structures
			// exist for a cid & needs to be corrected.

			foreach (MqlToken tok2 in critToks2)
			{
				if (tok2.Qc == null) continue;
				if (tok2.Qc.MetaColumn.DataType != MetaColumnType.Structure) continue;
				if (query.SingleStepExecution) continue;

				if (IsCartridgeMetaTable(mt)) continue; // can keep ok if Cartridge table

				// todo: allow structure criteria to be kept for cases 5 & 6 after code fixed to avoid exception

				critToks2 = // disable struct search criteria
				 MqlUtil.DisableCriteria(critToks, query, null, null, null, tok2.Qc, false, true, true);
			}

			string complexCriteria = // build the real sql for the query
			 MqlUtil.CatenateCriteriaTokens(critToks2);

			return complexCriteria;
		}

		/// <summary>
		/// Return true if metatable retrieves structures from a chemical database cartridge table
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static bool IsCartridgeMetaTable(
			MetaTable mt)
		{
			RootTable rt = RootTable.GetFromTableName(mt.Name);
			if ((rt != null && rt.CartridgeSearchable) ||
			 Lex.Contains(mt.TableMap, "chime("))
				return true;

			else return false;
		}

		/// <summary>
		/// Build MQL Order By string from query
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		public static string GetOrderByString(
			Query q)
		{
			string orderBy = "";
			QueryTable qt;
			int ci = 0;

			List<SortColumn> sCols = new List<SortColumn>();

			q.AssignUndefinedAliases(); // be sure we have aliases defined 
			for (int qti = 0; qti < q.Tables.Count; qti++)
			{
				qt = q.Tables[qti];
				MetaTable mt = qt.MetaTable;

				foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (qc.IsKey)
					{
						if (q.KeySortOrder == 0 || sCols.Count > 0) continue; // include key just once for 1st table
						qc.SortOrder = q.KeySortOrder;
					}

					else if (qc.SortOrder == 0) continue;

					MetaColumn mc = qc.MetaColumn;

					SortColumn sc = new SortColumn();
					sc.QueryColumn = qc;
					sc.Position = Math.Abs(qc.SortOrder);
					sc.Direction = (qc.SortOrder > 0) ? SortOrder.Ascending : SortOrder.Descending;
					for (ci = sCols.Count - 1; ci >= 0; ci--)
						if (sc.Position >= sCols[ci].Position) break;
					sCols.Insert(ci + 1, sc);
				}
			}

			foreach (SortColumn sc0 in sCols)
			{
				if (orderBy != "") orderBy += ", ";
				qt = sc0.QueryColumn.QueryTable;
				orderBy += qt.Alias + "." + sc0.QueryColumn.MetaColumn.Name;
				if (sc0.Direction == SortOrder.Descending) orderBy += " desc";
			}

			return orderBy;
		}

		/// <summary>
		/// Parse a Mql statement into a query object
		/// </summary>

		public static Query ConvertMqlToQuery(
			string mql)
		{
			Query q;
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			string expr, tName, cName, tok;
			int qti;

			if (mql == null || mql.Trim().Length == 0)
				throw new QueryException("Mql not defined");

			q = new Query();
			int criteriaCount = 0;

			Lex lex = new Lex();
			lex.SetDelimiters(" , ; ( ) < = > <= >= <> != !> !<");
			lex.OpenString(mql);

			// Parse "Select" clause

			tok = lex.Get();
			if (!Lex.Eq(tok, "select"))
				throw new QueryException("Initial \"Select\" keyword not found");

			ArrayList exprs = new ArrayList();
			expr = "";
			int parenDepth = 0;
			while (true)
			{
				tok = lex.Get();
				if (tok == "") break;
				if (Lex.Eq(tok, "from") ||
					(tok == "," && parenDepth == 0))
				{
					exprs.Add(expr);
					expr = "";
					if (Lex.Eq(tok, "from")) break;
				}

				else
				{
					if (expr.Length > 0) expr += " ";
					expr += tok;
					if (tok == "(") parenDepth++;
					else if (tok == ")") parenDepth--;
				}
			}

			// Parse "From" clause

			if (!Lex.Eq(tok, "from"))
				throw new QueryException("\"From\" keyword not found");

			Dictionary<string, int> aliasMap = new Dictionary<string, int>(); // map table names, aliases to query table position

			while (true)
			{
				tName = lex.Get();
				bool summarized = false;

				mt = MetaTableCollection.Get(tName);
				// obsolete
				//{
				//if (mt == null && Lex.EndsWith(tName, MetaTable.SummarySuffix)) 
				//  string summaryTable = tName.Substring(0, tName.Length - 8);
				//  mt = MetaTableCollection.Get(summaryTable);
				//  summarized = true;
				//}

				//if (mt != null && Lex.EndsWith(tName, MetaTable.SummarySuffix) && !Lex.Eq(tName,mt.Name))
				//  summarized = true; // really a summarized table?

				if (mt == null) throw new QueryException("Table \"" + tName + "\" not found");
				qt = new QueryTable(q, mt);

				for (int i1 = 0; i1 < qt.QueryColumns.Count; i1++)
				{
					qc = qt.QueryColumns[i1];
					mc = qc.MetaColumn;
					if (mc != null && mc.IsKey) qc.Selected = true; // always select key
					else qc.Selected = false;
				}

				string alias = mt.Name.ToLower(); // assign metatable name as default alias
				tok = lex.GetLower();
				if (tok != "" && tok != "where" && tok != "order" && tok != ",")
				{
					alias = tok;
					tok = lex.Get();
				}

				aliasMap[alias] = q.Tables.Count - 1; // map table alias to table position
				qt.Alias = alias;

				if (tok != ",") break; // done unless comma
			}

			// Parse criteria

			if (Lex.Eq(tok, "where"))
			{
				q.LogicType = QueryLogicType.Complex;

				StringBuilder sb = new StringBuilder();

				while (true)
				{
					tok = lex.Get();
					if (tok == "" || Lex.Eq(tok, "Order")) break;
					if (!IsKeyWord(tok) && !IsDelimiter(tok) && !Lex.IsDouble(tok) && !tok.StartsWith("'"))
					{ // lookup col reference
						qc = GetQueryColumn(tok, q);
						if (qc == null)
							throw new QueryException("Unrecognized column in \"From\" clause: " + tok);
						tok = qc.QueryTable.Alias + "." + qc.MetaColumn.Name; // qualify by alias
					}

					if (sb.Length > 0) sb.Append(" ");
					sb.Append(tok);
				}

				q.ComplexCriteria = sb.ToString();
			}

			// Parse order by

			if (Lex.Eq(tok, "order") && Lex.Eq(lex.Peek(), "by"))
			{
				tok = lex.Get(); // get the "by"
				q.ClearSorting();

				int orderCount = 0;
				while (true)
				{
					tok = lex.Get();
					if (tok == "") break;

					qc = GetQueryColumn(tok, q);
					if (qc == null)
						throw new QueryException("Unrecognized column in \"Order By\" clause: " + tok);

					orderCount++;
					qc.SortOrder = orderCount;

					if (Lex.Eq(lex.Peek(), "desc"))
					{
						qc.SortOrder = -qc.SortOrder;
						lex.Get();
					}

					if (qc.IsKey) q.KeySortOrder = qc.SortOrder;

					if (Lex.Eq(lex.Peek(), ",")) lex.Get();
				}
			}

			// Mark selected fields

			foreach (string s in exprs)
			{ // global multiple select
				if (s == "*" || s == "*+" || s == "*-")
				{ // wildcard selector for all tables
					foreach (QueryTable qt2 in q.Tables)
					{
						foreach (QueryColumn qc2 in qt2.QueryColumns)
						{
							mc = qc2.MetaColumn;
							if ((s == "*-" && mc.InitialSelection == ColumnSelectionEnum.Selected) ||
								(s == "*" && (mc.InitialSelection == ColumnSelectionEnum.Selected ||
								mc.InitialSelection == ColumnSelectionEnum.Unselected))
								|| s == "*+") qc2.Selected = true;
						}
					}
					continue;
				}

				qt = null;
				string[] sa = s.Split('.');
				if (sa.Length == 2) // qualified name, get associated query table
				{
					string alias = sa[0].ToLower();
					if (!aliasMap.ContainsKey(alias))
						throw new QueryException("Table/alias not found in \"From\" clause: " + alias);
					qt = q.Tables[aliasMap[alias]];
					expr = sa[1];
				}

				else // must be simple column name, scan tables to find
				{
					qc = GetQueryColumn(sa[0], q);
					if (qc == null)
						throw new QueryException("Column not found in any table in \"From\" clause: " + sa[0]);
					qt = qc.QueryTable;
					expr = sa[0];
				}

				if (expr == "*" || expr == "*+" || expr == "*-")
				{ // wildcard selector

					foreach (QueryColumn qc2 in qt.QueryColumns)
					{
						mc = qc2.MetaColumn;
						if ((expr == "*-" && mc.InitialSelection == ColumnSelectionEnum.Selected) ||
							(expr == "*" && (mc.InitialSelection == ColumnSelectionEnum.Selected ||
							mc.InitialSelection == ColumnSelectionEnum.Unselected))
							|| expr == "*+") qc2.Selected = true;
					}
					continue; // done with expression
				}

				qc = qt.GetQueryColumnByName(expr); // lookup column name
				if (qc != null) qc.Selected = true;
				else throw new QueryException("Select expression \"" + expr + "\" for table \"" +	mt.Name + " is not valid\r\n" +
					"Mql: " + mql);
			}

			return q;
		}

		/// <summary>
		/// Get the column in the query associated with an unqualified column name
		/// or a column name qualified by the table name or an alias
		/// </summary>
		/// <param name="q"></param>
		/// <param name="aliases"></param>
		/// <returns></returns>

		public static QueryColumn GetQueryColumn(
			string expr,
			Query q)
		{
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			string tName, cName;
			int qti;

			ParseColumnIdentifier(expr, out tName, out cName);

			for (qti = 0; qti < q.Tables.Count; qti++)
			{
				qt = q.Tables[qti];
				mt = qt.MetaTable;

				if (tName == "" ||
					Lex.Eq(tName, mt.Name) ||
					Lex.Eq(tName, qt.Alias))
				{
					qc = qt.GetQueryColumnByName(cName);
					if (qc != null) return qc; // got it
					if (tName == "") continue; // if table not defined keep looking
					else return null; // no such column
				}
			}

			return null;
		}

		/// <summary>
		/// Parse a simple expression of the form:
		/// cName or tName.cName or tAlias.cName
		/// </summary>
		/// <param name="expr"></param>
		/// <param name="tName"></param>
		/// <param name="cName"></param>

		public static void ParseColumnIdentifier(
			string expr,
			out string tName,
			out string cName)
		{
			tName = "";
			cName = "";
			int i1 = expr.IndexOf(".");
			if (i1 < 0) cName = expr;
			else
			{
				tName = expr.Substring(0, i1); // may be table name or alias
				cName = expr.Substring(i1 + 1);
			}
			return;
		}

		/// <summary>
		/// Get next token that is a keyword
		/// </summary>
		/// <param name="lex"></param>
		/// <returns></returns>

		public static string GetNextKeyWord(
			Lex lex)
		{
			while (true)
			{
				string tok = lex.Get();
				if (IsKeyWord(tok) ||
				 tok == "") return tok;
			}
		}

		/// <summary>
		/// Get next token that is not a keyword
		/// </summary>
		/// <param name="lex"></param>
		/// <returns></returns>

		public static string GetNextNonKeyWord(
			Lex lex)
		{
			while (true)
			{
				string tok = lex.Get();
				if (!IsKeyWord(tok) ||
				 tok == "") return tok;
			}
		}

		/// <summary>
		/// Return true if token matches a keyword
		/// </summary>
		/// <param name="tok"></param>
		/// <returns></returns>

		public static bool IsKeyWord(
			string tok)
		{
			for (int i1 = 0; i1 < KeyWords.Length; i1++)
			{
				if (Lex.Eq(tok, KeyWords[i1])) return true;
			}

			return false;
		}

		/// <summary>
		/// Return true if token is a delimiter
		/// </summary>
		/// <param name="tok"></param>
		/// <returns></returns>

		public static bool IsDelimiter(
			string tok)
		{
			for (int i1 = 0; i1 < Delimiters.Length; i1++)
			{
				if (Lex.Eq(tok, Delimiters[i1])) return true;
			}

			return false;
		}

		/// <summary>
		/// Return true if token is a basic comparison operator
		/// </summary>
		/// <param name="op"></param>
		/// <returns></returns>
		public static bool IsBasicComparisonOperator(
			string op)
		{
			foreach (string s in BinaryComparisonOperators)
			{
				if (op == s) return true;
			}

			return false;
		}

		/// <summary>
		/// Parse criteria into tokens identifying querycolumns
		/// </summary>
		/// <param name="criteria"></param>
		/// <param name="q"></param>
		/// <returns></returns>

		public static List<MqlToken> ParseComplexCriteria(
			string criteria,
			Query q)
		{
			List<MqlToken> criteriaToks = new List<MqlToken>();
			Lex lex = new Lex();

			StringBuilder sb = new StringBuilder(); // build set of delimiters
			foreach (string s0 in Delimiters)
			{
				if (sb.Length > 0) sb.Append(' ');
				sb.Append(s0);
			}
			lex.SetDelimiters(sb.ToString());

			lex.OpenString(criteria);
			while (true)
			{
				PositionedToken pTok = lex.GetPositionedToken();
				if (pTok == null) break;

				if (criteriaToks.Count > 0 && // see if numeric part of negative number
					criteriaToks[criteriaToks.Count - 1].Tok.Text == "-" && Lex.IsDouble(pTok.Text) &&
					pTok.Position == criteriaToks[criteriaToks.Count - 1].Tok.Position + 1)
				{ // append this tok to "-" tok if so 
					criteriaToks[criteriaToks.Count - 1].Tok.Text += pTok.Text;
					continue;
				} // ok for our limited analysis if merges binary "-" improperly in some cases, e.g. x -5

				MqlToken tok = new MqlToken();
				criteriaToks.Add(tok);
				tok.Tok = pTok;
				QueryColumn qc = MqlUtil.GetQueryColumn(pTok.Text, q);
				tok.Qc = qc;
			}

			SetBasicLogicTokenAttributes(criteriaToks);

			return criteriaToks;
		}

/// <summary>
/// Scan criteria tokens and 
/// </summary>
/// <param name="criteriaToks"></param>
		public static void SetBasicLogicTokenAttributes(
			List<MqlToken> criteriaToks)
		{
			int parenDepth = 0;
			bool inBetween = false;
			int functionParmListDepth = 0;
			MqlToken currentQcTok = null;
			int lastMqlTokpos = -1;

			for (int tki = 0; tki < criteriaToks.Count; tki++)
			{
				MqlToken mqlTok = criteriaToks[tki];
				if (mqlTok == null) continue;

				PositionedToken pTok = mqlTok.Tok;
				if (pTok == null) continue;

				mqlTok.Depth = parenDepth;
				mqlTok.IsLogicToken = false;

				string tok = pTok.Text;

				//if (mqlTok.Qc != null)
				//{
				//	currentQc = mqlTok.Qc; // current query column
				//}

				//if (currentQc != null && currentQc.IsKey) continue; // if in key column criteria continue since long lists may be broken into shorter OR-ed lists

				if (Lex.Eq(tok, "Between")) inBetween = true;
				else if (Lex.Eq(tok, "And") && inBetween) inBetween = false;

				else if (Lex.Eq(tok, "And"))
				{
					mqlTok.IsLogicToken = true;
				}

				else if (Lex.Eq(tok, "Or"))
					mqlTok.IsLogicToken = true;

				else if (tok == "(")
				{
					string t2 = "";
					if (lastMqlTokpos >= 0)
						t2 = criteriaToks[lastMqlTokpos].Tok.Text;

					if (Lex.Eq(t2, "and") || Lex.Eq(t2, "or") || Lex.Eq(t2, "("))
					{
						parenDepth++;
						mqlTok.IsLogicToken = true;
					}

					else functionParmListDepth++;
				}

				else if (tok == ")")
				{
					if (functionParmListDepth <= 0)
					{
						parenDepth--;
						mqlTok.Depth = parenDepth; // say closing paren at new depth
						mqlTok.IsLogicToken = true;
					}

					else functionParmListDepth--;
				}

				lastMqlTokpos = tki;
			}

			return;
		}


		/// <summary>
		/// See if sequence of tokens matches pattern
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="tki"></param>
		/// <param name="pattern">Pattern to match with spaces between tokens</param>
		/// <returns></returns>

		public static bool TokenSequenceMatches(
			List<MqlToken> tokens,
			int firstToken,
			string pattern)
		{
			string toks = null;
			for (int tki = firstToken; tki < tokens.Count; tki++)
			{
				if (tki > firstToken) toks += " ";
				toks += tokens[tki].Tok.Text.ToLower();
				if (Lex.Equals(toks, pattern)) return true;
			}

			return false;
		}

/// <summary>
/// Get a dictionary of query tables containing criteria keyed by table alias
/// </summary>
/// <param name="query"></param>
/// <param name="tokens"></param>
/// <returns></returns>

		public static Dictionary<string, QueryTable> GetCriteriaTables(
			Query query,
			List<MqlToken> tokens,
			bool doFullErrorCheck = false)
		{
			QueryTable qt = null;
			QueryColumn qc = null, keyQc = null;

			string critToksBeforeDebug = // see what we have (debug)
				MqlUtil.CatenateCriteriaTokensForDebug(tokens);

			if (query == null) DebugMx.ArgException("Query is null");
			if (query.Tables == null || query.Tables.Count == 0) DebugMx.ArgException("No QueryTables defined");
			if (tokens == null) DebugMx.ArgException("Tokens are null");

			Dictionary<string, QueryTable> criteriaTables = new Dictionary<string, QueryTable>();
			Dictionary<string, QueryTable> criteriaTablesWithNotExists = new Dictionary<string, QueryTable>();
			Dictionary<string, QueryTable> criteriaTablesWithNormalCriteria = new Dictionary<string, QueryTable>();

			QueryTable firstQt = query.Tables[0];

			foreach (MqlToken tok in tokens)
			{
				qc = tok.Qc;
				if (qc == null) continue;

				//if (qc.IsKey) // ignore key criteria for now
				//{
				//	keyQc = qc; // not currently used
				//	//ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(qc.Criteria); (qc col name not in criteria)
				//	//if (psc == null) continue;

				//	continue; 
				//}

				qt = qc.QueryTable;

				criteriaTables[qt.Alias] = qt;

				int critCount = qt.GetCriteriaCount(includeKey: false, includeDbSet: false);

				if (qt.HasNotExistsCriteria)
					criteriaTablesWithNotExists[qt.Alias] = qt;

				if (qt.HasNormalDatabaseCriteria)
					criteriaTablesWithNormalCriteria[qt.Alias] = qt;

				else if (critCount == 0 && Lex.IsDefined(query.KeyCriteria)) // if table (e.g. root, with not criteria but key criteria exists then count as normal
					criteriaTablesWithNormalCriteria[qt.Alias] = qt;
			}

			if (doFullErrorCheck) // do error check on full query
			{
				if (criteriaTablesWithNotExists.Count > 0 &&  // don't allow only "not exists" criteria unless have a key list
					criteriaTablesWithNormalCriteria.Count == 0 && (
					Lex.IsUndefined(query.KeyCriteria) || KeyCriteriaRequiresDatabaseSearch(query.KeyCriteria)))
				{
					throw new UserQueryException("At least one table in the query must contain criteria other than \"Doesn't Exist\".");
				}
			}

			return criteriaTables;
		}

/// <summary>
/// Do a simple scan of key criteria to see if it will require a database search to evaluate
/// </summary>
/// <param name="keyCriteria"></param>
/// <returns></returns>

		public static bool KeyCriteriaRequiresDatabaseSearch(string keyCriteria)
		{
			if (Lex.IsUndefined(keyCriteria)) return false;

			else if (Lex.Contains(keyCriteria, "in") || Lex.Contains(keyCriteria, "=")) return false; // todo parse criteria & check details

			else return true;
		}

		/// <summary>
		/// Disable conditions from criteria that are inactive according to
		/// the method parameters. It operates by reducing inactive sections of the
		/// criteria between parens, "and" and "or" to "1=1" condition which are 
		/// always true. Only parens for grouping logic and "and" not in between act 
		/// as delimiters. 
		/// This should return the same number of tokens as supplied since existing indexes
		/// into the tokens may be used later on the disabled set of tokens
		/// Todo: Properly handle "not" 
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="q"></param>
		/// <param name="singleTable">Only table to be included</param>
		/// <param name="excludeTable">Exclude this table, include others</param>
		/// <param name="singleCriteriaQc">Only criteria to be included</param>
		/// <param name="excludeCriteriaQc">Exclude this criteria, include others</param>
		/// <param name="useFilterSearch">If true then QC.FilterSearch must be true for col</param>
		/// <param name="useFilterRetrieval">If true then QC.FilterRetrieval must be true for col</param>
		/// <param name="retainKeyCriteria">If true then keep key criteria filters</param>
		/// <returns></returns>

		public static List<MqlToken> DisableCriteria(
			List<MqlToken> tokens,
			Query q,
			QueryTable singleTable,
			QueryTable excludeTable,
			QueryColumn singleCriteriaQc,
			QueryColumn excludeCriteriaQc,
			bool useFilterSearch,
			bool useFilterRetrieval,
			bool retainKeyCriteria)
		{
			string tok = null;
			int i1;

			string critToksBeforeDebug = // see what we have (debug)
				MqlUtil.CatenateCriteriaTokensForDebug(tokens);

			List<MqlToken> newToks = new List<MqlToken>(); // copy of tokens with specified criteria disabled
			List<MqlToken> keyQcToks = new List<MqlToken>(); // output tokens that are enabled and referring to key column
			Dictionary<string, QueryTable> criteriaTables = new Dictionary<string, QueryTable>(); // tables with enabled criteria keyed on alias
			MqlToken firstAddedNonkeyQcToken = null;
			QueryColumn retainedSingleCriteriaKeyQc = null;

			int lastDelim = -1;
			int p = 0; // position in tokens
			int pDepth = 0; // paren depth in section
			int sectionCnt = 0; // number of sections
			int sectionTokCnt = 0; // number of tokens in secion
			int sectionPDepth = 0; // paren depth for section
			bool inBetween = false;

			while (true)
			{
				if (p < tokens.Count) tok = tokens[p].Tok.Text;
				else tok = "";

				if
				 (p >= tokens.Count || // at end of tokens
				 (tok == "(" && sectionTokCnt == 0) || // logical grouping paren not function paren
				 (tok == ")" && pDepth == sectionPDepth) ||
				 (Lex.Eq(tok, "And") && !inBetween) || // and that is not part of a between
					Lex.Eq(tok, "Or"))
				{
					if (sectionTokCnt > 0)
					{ // see if section should be kept or removed

						for (i1 = p - sectionTokCnt; i1 < p; i1++)
						{ // see if any disallowed columns in this section
							QueryColumn qc = tokens[i1].Qc;
							if (qc == null) continue;

							if (singleTable != null && singleTable.Alias != qc.QueryTable.Alias)
								break; // reject section if not single table
							if (excludeTable != null && excludeTable.Alias == qc.QueryTable.Alias)
								break; // reject section if single table

							if (singleCriteriaQc != null && singleCriteriaQc.MetaColumn.Name != qc.MetaColumn.Name)
							{ // reject section if not the specified single criteria
								if (retainKeyCriteria && qc.IsKey) // unless this is key & we want to keep key criteria
									retainedSingleCriteriaKeyQc = qc;
								else break; // reject it
							}

							if (excludeCriteriaQc != null && excludeCriteriaQc.MetaColumn.Name == qc.MetaColumn.Name)
								break; // reject section if single table

							//if (qc.Criteria != "") qc = qc; // debug

							if (useFilterSearch && !qc.FilterSearch) break;
							if (useFilterRetrieval && !qc.FilterRetrieval) break;
							if (qc.MetaColumn.BrokerFiltering) break; // broker will do this filtering
							if (!retainKeyCriteria && qc.IsKey) break;
						}

						if (i1 >= p) // keep section
						{
							for (i1 = p - sectionTokCnt; i1 < p; i1++)
							{
								MqlToken newTok = tokens[i1].Clone();
								newToks.Add(newTok);

								QueryColumn qc = newTok.Qc;
								if (qc != null)
								{
									if (qc.IsKey) keyQcToks.Add(newTok); // list of key QC tokens
									else
									{
										criteriaTables[qc.QueryTable.Alias] = qc.QueryTable; // dict of QueryTable with non-key criteria
										if (firstAddedNonkeyQcToken == null) firstAddedNonkeyQcToken = newTok;
									}
								}
							}
						}

						else
						{ // disable section
							for (i1 = p - sectionTokCnt; i1 < p; i1++)
							{
								MqlToken mqlTok = new MqlToken();
								PositionedToken pTok = new PositionedToken("");
								mqlTok.Tok = pTok;

								if (i1 == p - sectionTokCnt) // if first token then put in placeholder text
									pTok.Text = "(1=1)";

								newToks.Add(mqlTok);
							}
						}

					}

					//else if (newToks.Count == 0) // blank initial set of tokens to be ignored
					//{
					//  for (i1 = 0; i1 < p; i1++)
					//  {
					//    MqlToken mqlTok = new MqlToken();
					//    PositionedToken pTok = new PositionedToken("");
					//    mqlTok.Tok = pTok;

					//    //if (i1 == 0) // if first token then put in placeholder text
					//    //  pTok.Text = "(1=1)";

					//    newToks.Add(mqlTok);
					//  }
					//}

					if (p >= tokens.Count) break; // done

					newToks.Add(tokens[p].Clone()); // add token to output

					if (tok == "(") pDepth++;
					else if (tok == ")") pDepth--;

					sectionCnt++;
					sectionTokCnt = 0; // setup for next section
					sectionPDepth = pDepth;
				}

				else // token at the beginning or within a section including AND or parens that are part of a "Between" or function reference
				{
					if (tok != "" || sectionTokCnt > 0) sectionTokCnt++;

					else if (tok == "" && sectionTokCnt == 0) // leading blank tokens (&& sectionCnt == 0) 
					{
						MqlToken mqlTok = new MqlToken();
						PositionedToken pTok = new PositionedToken("");
						mqlTok.Tok = pTok;
						newToks.Add(mqlTok);
					}

					if (tok == "(") pDepth++;
					else if (tok == ")") pDepth--;

					if (Lex.Eq(tok, "Between")) inBetween = true;
					else if (Lex.Eq(tok, "And")) inBetween = false;
				}

				p++;
			}

			// Be sure any key criteria are mapped to some table whose criteria has been included
			
			bool allowKeyCriteriaRemap = true;
			if (criteriaTables.Count > 1 && q.LogicType == QueryLogicType.Or) // if "OR" logic & multiple tables with criteria then don't remap key criteria to just one of the tables
				allowKeyCriteriaRemap = false;

			if (allowKeyCriteriaRemap)
			{
				foreach (MqlToken mqlTok0 in keyQcToks)
				{
					if (!criteriaTables.ContainsKey(mqlTok0.Qc.QueryTable.Alias) && firstAddedNonkeyQcToken != null)
					{
						QueryTable qt = firstAddedNonkeyQcToken.Qc.QueryTable;
						if (!qt.HasNotExistsCriteria)
						{
							mqlTok0.Qc = qt.KeyQueryColumn;
							mqlTok0.Tok.Text = qt.Alias + "." + mqlTok0.Qc.MetaColumn.Name;
						}
					}
				}
			}

			// If single table then blank out expressions involving "(1=1)"

			if (singleTable != null)
			{
				int changeCount = 1;
				while (changeCount > 0)
				{
					changeCount = 0;
					changeCount += TransformLogicalExpression(newToks, new string[] { "(1=1)", "And" }, "");
					changeCount += TransformLogicalExpression(newToks, new string[] { "(1=1)", "Or" }, "");
					changeCount += TransformLogicalExpression(newToks, new string[] { "And", "(1=1)" }, "");
					changeCount += TransformLogicalExpression(newToks, new string[] { "Or", "(1=1)" }, "");
					changeCount += TransformLogicalExpression(newToks, new string[] { "(", "(1=1)", ")" }, "(1=1)");
					changeCount += TransformLogicalExpression(newToks, new string[] { "Not", "(1=1)" }, "(1=1)");
				}
			}

			critToksBeforeDebug = critToksBeforeDebug; // debug
			string critToksAfterDebug2 = // see what we have (debug)
				MqlUtil.CatenateCriteriaTokensForDebug(newToks);

			if (tokens.Count != newToks.Count) // counts should match
			{
				string msg = "Output token count differs from input count for query " + q.UserObject.Id;
				DebugLog.Message(msg);
				if (ClientState.IsDeveloper)
					throw new Exception(msg);
			}

			return newToks;
		}

/// <summary>
/// DisableCriteriaNotInSet
/// </summary>
/// <param name="tokens"></param>
/// <param name="tokPosSet"></param>
/// <returns></returns>

		public static List<MqlToken> DisableCriteriaNotInSet(
			List<MqlToken> tokens,
			HashSet<int> tokPosSet)
		{
			List<MqlToken> newToks = new List<MqlToken>(); // copy of tokens with specified criteria disabled

			for (int tki = 0; tki < tokens.Count; tki++)
			{
				if (tokPosSet.Contains(tki))
				{
					MqlToken newTok = tokens[tki].Clone();
					newToks.Add(newTok);
				}

				else
				{
					MqlToken mqlTok = new MqlToken();
					newToks.Add(mqlTok);
				}
			}

			return newToks;
		}

	/// <summary>
	/// Transform all instances of the sequence of match tokens into the 
	/// replacement text.
	/// </summary>
	/// <param name="match"></param>
	/// <returns></returns>

	public static int TransformLogicalExpression(
			List<MqlToken> tokens,
			string[] match,
			string replace)
		{
			int changeCount = 0;

			for (int p = 0; p < tokens.Count; p++)
			{
				if (MatchSingleToken(match, 0, tokens, p, p, replace))
					changeCount++;
			}
			return changeCount;
		}

		/// <summary>
		/// Recursively scan forward until we get a full match
		/// ignoring white space tokens
		/// </summary>
		/// <param name="match"></param>
		/// <param name="mi"></param>
		/// <param name="tokens"></param>
		/// <param name="p"></param>
		/// <param name="replace"></param>
		/// <returns></returns>

		public static bool MatchSingleToken(
			string[] match,
			int mi,
			List<MqlToken> tokens,
			int p0,
			int p,
			string replace)
		{
			for (p = p; p < tokens.Count; p++)
			{ // scan the tokens
				if (Lex.Eq(tokens[p].Tok.Text, match[mi]))
				{ // matches at this level
					if (mi < match.Length - 1) // if not at bottom go to next token to match
						return MatchSingleToken(match, mi + 1, tokens, p0, p + 1, replace);

					else
					{ // have a complete match, replace
						tokens[p0].Tok.Text = replace;
						for (int p2 = p0 + 1; p2 <= p; p2++)
							tokens[p2].Tok.Text = "";
						return true;
					}
				}

				else if (mi == 0 || tokens[p].Tok.Text != "") return false;
				else continue; // ignore blanks
			}

			return false;
		}

		/// <summary>
		/// Transform key criteria col names & constant values for current root table
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="rootTable"></param>
		/// <returns></returns>

		public static List<MqlToken> TransformKeyCriteriaForRoot(
		List<MqlToken> tokens,
		MetaTable rootTable,
		List<int> KeyCriteriaPositions,
		List<int> KeyCriteriaConstantPositions)
		{
			string critToksBeforeDebug = // see what we have (debug)
				MqlUtil.CatenateCriteriaTokensForDebug(tokens);

			bool integerKey = rootTable.IsIntegerKey();

			List<MqlToken> newToks = CopyMqlTokenList(tokens);

			foreach (int tki in KeyCriteriaPositions) // if reference to key column then map to current metatable
			{
				QueryColumn qc = tokens[tki].Qc;

				if (qc == null) continue;
				QueryTable qt = qc.QueryTable;
				QueryTable qt2 = qt.MapToCurrentDb(rootTable); // actual query table (modifiable copy always created)
				QueryColumn qc2 = qt2.KeyQueryColumn;
				newToks[tki].Qc = qc2;
				newToks[tki].Tok.Text = qt2.Alias + "." + qc2.MetaColumn.Name;
			}

			string prefix = CompoundId.GetPrefixFromRootTable(rootTable);

			foreach (int tki in KeyCriteriaConstantPositions)
			{
				if (tki >= tokens.Count) DebugMx.DataException("Token index " + tki + " exceeds tokens count " + tokens.Count);
				string key = Lex.RemoveAllQuotes(tokens[tki].Tok.Text);
				if (key == "") continue; // be sure something still there to convert

				key = CompoundId.NormalizeForDatabase(key, rootTable); // convert to database format
				if (!integerKey || !Lex.IsInteger(key)) // quote it if not integer column or value
					key = Lex.AddSingleQuotes(key); // (note: quoted integers can cause mismatches for some database systems, e.g. Denodo)

				newToks[tki].Tok.Text = key;
			}

			string critToksAfterDebug = CatenateCriteriaTokensForDebug(newToks); // debug

			return newToks;
		}

/// <summary>
/// CopyMqlTokenList
/// </summary>
/// <param name="criteriaList"></param>
/// <returns></returns>

		public static List<MqlToken> CopyMqlTokenList(List<MqlToken> criteriaList)
		{
			List<MqlToken> newToks = new List<MqlToken>();
			foreach (MqlToken mqlTok in criteriaList)
			{
				MqlToken newTok = mqlTok.Clone();
				newToks.Add(newTok);
			}

			return newToks;
		}

		/// <summary>
		/// Get list of QueryColumns that are present in the criteria list
		/// </summary>
		/// <param name="criteriaList"></param>
		/// <returns></returns>

		public static List<QueryColumn> GetQueryColumns(
			List<MqlToken> criteriaList)
		{
			List<QueryColumn> qcList = new List<QueryColumn>();

			for (int tki = 0; tki < criteriaList.Count; tki++)
			{
				MqlToken tok = criteriaList[tki];
				if (tok != null && tok.Tok != null && tok.Qc != null)
				{
					if (!qcList.Contains(tok.Qc))
						qcList.Add(tok.Qc);
				}
			}

			return qcList;
		}


		/// <summary>
		/// IndexOfQueryColumnInMqlTokenList
		/// </summary>
		/// <param name="criteriaList"></param>
		/// <param name="qc"></param>
		/// <returns></returns>

		public static int IndexOfQueryColumnInMqlTokenList (
			List<MqlToken> criteriaList, 
			QueryColumn qc)
		{
			for (int tki = 0; tki < criteriaList.Count; tki++)
			{
				MqlToken tok = criteriaList[tki];
				if (tok != null && tok.Tok != null && tok.Qc == qc) return tki;
			}

			return -1;
		}

		/// <summary>
		/// Catenate criteria tokens into string for debug purposes
		/// </summary>
		/// <param name="tokens"></param>
		/// <returns></returns>

		public static string CatenateCriteriaTokensForDebug(
			List<MqlToken> tokens)
		{
			return CatenateCriteriaTokensForDebug(tokens, debug:true);
		}

		/// <summary>
		/// Catenate criteria tokens into string for debug purposes
		/// </summary>
		/// <param name="tokens"></param>
		/// <returns></returns>

		public static string CatenateCriteriaTokensForDebug(
			List<MqlToken> tokens,
			bool debug)
		{
			if (!debug) return "Disabled";

			string tokString = CatenateCriteriaTokens(tokens, 0, debug:true);
			return tokString;
		}

		/// <summary>
		/// Catenate criteria tokens into string
		/// </summary>
		/// <param name="tokens"></param>
		/// <returns></returns>

		public static string CatenateCriteriaTokens(
			List<MqlToken> tokens,
			int firstTokenIndex = 0,
			bool debug = false)
		{
			StringBuilder sb = new StringBuilder();
			for (int tki = firstTokenIndex; tki < tokens.Count; tki++)
			{
				MqlToken mqlTok = tokens[tki];
				string tok = mqlTok.Tok.Text;

				if (Lex.IsUndefined(tok)) continue;
				if (sb.Length > 0) sb.Append(" ");

				sb.Append(tok);

				if (debug && mqlTok.IsLogicToken) // include logic token parse depth info
				{
					if (Lex.Eq(tok, "And") ||
					 Lex.Eq(tok, "Or") ||
					 tok == "(" ||
					 tok == ")")
						sb.Append("[" + mqlTok.Depth + "]");
				}

			}
			return sb.ToString();
		}

		/// <summary>
		/// Parse database set criteria into a list & remove from token list
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="tki"></param>
		/// <param name="dbSet"></param>
		/// <returns></returns>

		public static void ExtractDbSetCriteria(
			List<MqlToken> tokens,
			int qcTki,
			List<MetaTable> rootTables)
		{
			QueryColumn qc = tokens[qcTki].Qc;

			int tki = qcTki;
			tokens[tki].Tok.Text = ""; // replace col name with dummy criteria
			tokens[tki].Qc = null;

			tki++;
			if (tki >= tokens.Count)
				throw new QueryException("Invalid list of databases");
			if (Lex.Eq(tokens[tki].Tok.Text, "Not"))
				throw new QueryException("\"Not\" logic is not allowed for the list of databases");
			if (!Lex.Eq(tokens[tki].Tok.Text, "In"))
				throw new QueryException("Invalid list of databases");
			tokens[tki].Tok.Text = "";

			tki++;
			if (tki >= tokens.Count || !Lex.Eq(tokens[tki].Tok.Text, "("))
				throw new QueryException("Invalid list of databases");
			tokens[tki].Tok.Text = "";

			while (true)
			{
				tki++;
				if (tki >= tokens.Count)
					throw new QueryException("Invalid list of databases");

				RootTable rti = // get info on associated structure database
					RootTable.GetFromTableLabel(tokens[tki].Tok.Text);
				tokens[tki].Tok.Text = "";
				if (rti != null)
				{
					string tname = rti.MetaTableName;
					MetaTable mt = MetaTableCollection.Get(tname);
					if (mt != null) rootTables.Add(mt);
				}

				tki++;
				if (tki >= tokens.Count)
					throw new QueryException("Invalid list of databases");
				string tok = tokens[tki].Tok.Text;
				tokens[tki].Tok.Text = "";
				if (Lex.Eq(tok, ")")) break;
				if (!Lex.Eq(tok, ","))
					throw new QueryException("Invalid list of databases");
			}

			if (!DisableAdjacentAndLogic(tokens, qcTki, tki))
				throw new UserQueryException("Only \"And\" logic is allowed with " +
						qc.ActiveLabel);

			string critToksAfterDebug = CatenateCriteriaTokensForDebug(tokens); // debug

			return;
		}

		/// <summary>
		/// Disable logical operator adjacent to supplied token range
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="tki1"></param>
		/// <param name="tki2"></param>
		/// <returns></returns>

		public static bool DisableAdjacentAndLogic(
			List<MqlToken> tokens,
			int tki1,
			int tki2)
		{
			for (tki1 = tki1 - 1; tki1 >= 0; tki1--)
			{ // try to remove and before criteria tokens
				string tok = tokens[tki1].Tok.Text;
				if (tok == "" || tok == "(" || tok == ")") continue; // ignore blank tokens

				if (!Lex.Eq(tok, "And")) return false;
				tokens[tki1].Tok.Text = "";
				break;
			}

			if (tki1 < 0) // if no success above try to remove and after criteria tokens
				for (tki2 = tki2 + 1; tki2 < tokens.Count; tki2++)
				{
					string tok = tokens[tki2].Tok.Text;
					if (tok == "" || tok == "(" || tok == ")") continue; // ignore blank tokens

					if (!Lex.Eq(tok, "And")) return false;

					tokens[tki2].Tok.Text = "";
					break;
				}

			return true; // success
		}


		/// <summary>
		/// Scan tokens & return any similarity search expression of the form: molsim(ctab,query,simtype)
		/// </summary>
		/// <param name="toks"></param>
		/// <returns></returns>

		public static string GetSimScoreExpressionFromCriteria(
			List<MqlToken> toks)
		{
			for (int ti = 0; ti < toks.Count - 7; ti++)
			{
				if (!Lex.Eq(toks[ti].Tok.Text, "Molsim") ||
					toks[ti + 1].Tok.Text != "(" || toks[ti + 3].Tok.Text != "," ||
					toks[ti + 5].Tok.Text != "," || toks[ti + 7].Tok.Text != ")")
					continue;

				string expr = "";
				for (int ti2 = ti; ti2 <= ti + 7; ti2++)
					expr += toks[ti2].Tok.Text;

				expr += " / 100"; // convert 0-100 cartridge range to 0-1 internal Mobius range
				return expr;
			}

			return null;
		}

		/// <summary>
		/// Verify that the set of tables in the query are compatible
		/// If mtToAdd is null then check the query as it is
		/// otherwise add newMt to a clone of the query & check that and
		/// only report errors for that metatable.
		/// 
		/// </summary>
		/// <param name="q"></param>
		/// <param name="mtToAdd"></param>
		/// 
		public static void CheckTableCompatibility(
			Query q,
			MetaTable mtToAdd = null) // check for compatibility of tables in query
		{
			QueryTable qt;
			MetaTable mt, rootTable = null;
			int ti;

			if (mtToAdd != null)
			{ // add in new metatable if defined & not in query
				if (q.GetQueryTableByName(mtToAdd.Name) != null) return; // if already have assume it's ok
				q = q.Clone(); // clone the query
				qt = new QueryTable(mtToAdd); // and add new metatable to it
				q.AddQueryTable(qt);
			}

			// Check for multiple root tables

			for (ti = 0; ti < q.Tables.Count; ti++)
			{
				qt = q.Tables[ti];
				mt = qt.MetaTable;
				if (mt.IsRootTable)
				{ // if this is a root table check to be sure not more than one root
					if (rootTable == null) rootTable = q.Tables[ti].MetaTable;
					else
					{
						if (mtToAdd == null || mtToAdd.Name == mt.Name)
							throw new UserQueryException("The following two \"root\" tables can't be combined in a single query:\n" +
							"  " + Lex.Dq(rootTable.Label) + "\n" +
							"  " + Lex.Dq(mt.Label));
					}
				}
			}

			// Check that the root is compatible with the non-root tables in the query

			if (q.Tables.Count >= 2)
			{
				if (rootTable == null)
				{
					if (q.RootTableNeedsToBeAdded() && mtToAdd == null) // must be complete as is
						throw new UserQueryException
						 ("A common \"root table\" must be included in queries that contain two or more tables.");

					else rootTable = q.Tables[0].MetaTable.Root;
				}

				RootTable rt = RootTable.GetFromTableName(rootTable.Name); // get root table info, if any

				for (ti = 0; ti < q.Tables.Count; ti++)
				{ // be sure all child tables are compatible with the root table key
					qt = q.Tables[ti];
					mt = qt.MetaTable;
					if (mt.IsRootTable) continue; // ignore root

					if (Lex.Eq(rootTable.Name, mt.Root.Name)) continue; // continue if matches root table by name

					RootTable rt2 = RootTable.GetFromTableName(mt.Root.Name); // both must be structure-based root tables for match
					if (rt != null && rt.IsStructureTable && rt2 != null && rt2.IsStructureTable) continue;

					if (mtToAdd == null || mtToAdd.Name == mt.Name)
						throw new UserQueryException(
						 "Table \"" + mt.Label + "\" can't be included in this query based on " + rootTable.KeyMetaColumn.Label +
						 " because the key fields are not compatible.");
				}
			}

			return;
		}

		/// <summary>
		/// Return true if the message is the result of a user query error
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>

		public static bool IsUserQueryErrorMessage(string msg)
		{
			if ( // catch any user error in query structure thrown from cartridge
			 Lex.Contains(msg, "Molstructure search query is not a valid molecule") ||
			 Lex.Contains(msg, "Invalid FS ") ||
			 Lex.Contains(msg, "Invalid SSS "))
				return true;

			else return false;
		}
	}

	/// <summary>
	/// Parsed single criteria condition
	/// </summary>

	public class ParsedSingleCriteria
	{
		public QueryColumn QueryColumn = null; // associated query column (may be null)
		public string Op = ""; // criteria operator string
		public CompareOp OpEnum = CompareOp.Unknown;
		public string Value = ""; // single or first criteria value
		public string Value2 = ""; // optional 2nd value
		public List<string> ValueList = null; // list of values

		/// <summary>
		/// Parse a QueryColumn criteria string into individual fields
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		public static ParsedSingleCriteria Parse(QueryColumn qc)
		{
			if (qc == null || Lex.IsUndefined(qc.Criteria)) return null;

			ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(qc.Criteria);
			return psc;
		}
	}

	/// <summary>
	/// Parsed token with some additional analysis information
	/// </summary>

	public class MqlToken
	{
		public QueryColumn Qc; // Associated QueryColumn if token is a QueryColumn reference
		public ParsedSingleCriteria Psc; // details on any associated criteria if a QueryColumn
		public PositionedToken Tok = new PositionedToken(""); // the token with positioning information
		public int Depth = -1; // logic depth 
		public bool IsLogicToken = false; // logic operator

// Default constructor

		public MqlToken()
		{
			return;
		}

/// <summary>
/// Construct with specified QueryColumn and/or  text
/// </summary>
/// <param name="qc"></param>
/// <param name="tokText"></param>

		public MqlToken(
			QueryColumn qc,
			string tokText)
		{
			Qc = qc;
			Tok = new PositionedToken(tokText);
			return; 
		}

		public MqlToken Clone()
		{
			MqlToken newTok = (MqlToken)this.MemberwiseClone();
			newTok.Tok = this.Tok.Clone();
			return newTok;
		}

		public override string ToString()
		{
			string txt = "MqlToken: ";

			txt += (Qc != null) ? " QC: " + Qc.QueryTable.Alias + " " + Qc.MetaTable.Name + "." + Qc.MetaColumnName : "QC: null";

			txt += ", Depth: " + Depth + (IsLogicToken ? ", IsLogic" : "");

			txt += (Tok != null) ? ", Tok: " + Tok.Text : ", Tok: null";

			return txt;
		}
	}

	/// <summary>
	/// Comparison operators
	/// </summary>

	public enum CompareOp
	{
		Unknown = 0,
		Eq = 1,
		Ne = 2,
		Lt = 3,
		Le = 4,
		Gt = 5,
		Ge = 6,
		In = 7, // in list that is contained within MQL
		NotIn = 8, // not in list contained within MQL
		InList = 9, // in separately referenced list
		NotInList = 10,
		Between = 11,
		Like = 12,
		NotLike = 13,
		IsNull = 14,
		IsNotNull = 15,
		Within = 16, // DateTime search relative to present DateTime
		SSS = 17, // substructure
		FSS = 18, // fullstructure
		MolSim = 19,
		SmallWorld = 20,
		RelatedSS = 21,
		FormulaEqual = 22,
		FormulaLike = 23
	}

	/// <summary>
	/// String used for each operator within MQL
	/// </summary>

	public class CompareOpString
	{
		public static string[] Values = {
		"unknown", // Unknown = 0,
		"=", // Eq = 1,
		"<>", // Ne = 2,
		"<", // Lt = 3,
		"<=", // Le = 4,
		">", // Gt = 5,
		">=", // Ge = 6,
		"in", // In = 7, // in list within MQL
		"not in", // NotIn = 8, // not in list within MQL (not fully supported)
		"in list", // InList = 9, // in separately referenced list
		"not in list", // NotInList = 10, // not in separately referenced list (not fully supported)
		"between", // Between = 11,
		"like", // Like = 12,
		"not like", // NotLike = 13,
		"is null", // IsNull = 14,
		"is not null", // IsNotNull = 15,
		"within", // Within = 16, // DateTime search relative to present DateTime
		"SSS", // SSS = 17,
		"FSS", // FSS = 18,
		"MolSim", // MolSim = 19,
		"SmallWorld", // 20,
		"MultipleSS", // 21,
		"fmla_eq", // FormulaEqual = 22,
		"fmla_like" }; // FormulaLike = 23

		public static CompareOp ToCompareOp(
			string compareOpString)
		{
			string cos = compareOpString.Trim();
			for (int coi = 0; coi < CompareOpString.Values.Length; coi++)
			{
				if (Lex.Eq(cos, CompareOpString.Values[coi]))
				{
					CompareOp co = (CompareOp)coi;
					return co;
				}
			}

			return CompareOp.Unknown;
		}

	} // CompareOpString

	/// <summary>
	/// String used for each operator for display to user
	/// </summary>

	public class CompareOpDisplayString
	{
		public static string[] Values = {
		"unknown", // Unknown = 0,
		"=", // Eq = 1,
		"<>", // Ne = 2,
		"<", // Lt = 3,
		"<=", // Le = 4,
		">", // Gt = 5,
		">=", // Ge = 6,
		"in", // In = 7, // in list within MQL
		"not in", // In = 8, // not in list within MQL (not fully supported)
		"in list", // InList = 9, // in separately referenced list
		"not in list", // InList = 10, // in separately referenced list (not fully supported)
		"between", // Between = 11,
		"contains", // Like = 12,
		"doesn't contain", // NotLike = 13,
		"is blank", // IsNull = 14,
		"is not blank", // IsNotNull = 15,
		"within", // Within = 16, // DateTime search relative to present DateTime
		"SSS", // SSS = 17,
		"FSS", // FSS = 18,
		"MolSim", // MolSim = 19,
		"=", // FormulaEqual = 20,
		"contains" }; // FormulaLike = 21
	}

	/// <summary>
	/// Phase of query engine execution
	/// </summary>

	public enum QueryEngineState
	{
		New = 0, // no query executed yet
		Searching = 1, // executing search step
		Retrieving = 2, // retrieving data rows
		Closed = 3 // all rows retrieved or explicitly closed
	}

}
