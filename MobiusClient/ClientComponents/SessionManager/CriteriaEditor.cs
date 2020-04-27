using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public class CriteriaEditor
	{

/// Edit criteria for a query column
/// </summary>
/// <param name="qc"></param>
/// <returns>True if criteria has been successfully edited</returns>

		public static bool EditCriteria (
			QueryColumn qc)
		{
			MetaColumn mc = qc.MetaColumn;
			Query Query  = qc.QueryTable.Query;
			bool sameQ = Query == QueriesControl.Instance.CurrentQuery; // debug

			if (Lex.Contains(qc.MetaColumn.ColumnMap, ToolUtil.ToolParametersColumnMapValue))
			{
				DialogResult dr = ToolHelper.InvokeToolCriteriaEditor(qc);
				return (dr == DialogResult.OK);
			}

			try
			{
				if (!mc.IsSearchable)
				{
					MessageBoxMx.ShowError("The " + qc.ActiveLabel + " data item is not currently searchable.");
					return false;
				}

				if (mc.IsKey) // edit key criteria
				{
					qc.CopyCriteriaFromQueryKeyCriteria(); // be sure qc is in sync with Query.KeyCriteria
					if (!CriteriaCompoundId.Edit(qc))
						return false;

					qc.CopyCriteriaToQueryKeyCritera(); // update Query.KeyCriteria
					return true;
				}

				switch (mc.DataType)
				{
					// Compound Number criteria

					case MetaColumnType.CompoundId:

						if (!CriteriaCompoundId.Edit(qc))
							return false;
						else break;

					// Structure criteria

					case MetaColumnType.Structure:
						if (!CriteriaStructure.Edit(qc))
							return false;
						break;

					// Mol. formula criteria

					case MetaColumnType.MolFormula:
						if (!CriteriaMolFormula.Edit(qc))
							return false;
						break;


					// General criteria

					case MetaColumnType.Integer:
					case MetaColumnType.Number:
					case MetaColumnType.QualifiedNo:
					case MetaColumnType.String:
					case MetaColumnType.Date:
					case MetaColumnType.DictionaryId:

						if (!Criteria.Edit(qc)) return false;
						else break;

					default:
						MessageBoxMx.ShowError("The " + qc.ActiveLabel + " data item is not currently searchable.");
						return false;
				}

				return true;
			}

			catch (Exception ex)
			{
				string msg = "Unexpected error editing criteria: \r\n\r\n" +
					DebugLog.FormatExceptionMessage(ex);
				ServicesLog.Message(msg);
				MessageBoxMx.ShowError(msg);
				return false;
			}
		}


/// <summary>
/// Edit CompoundId Criteria proxy
/// </summary>
/// <param name="qc"></param>
/// <returns></returns>

		public static bool GetCompoundIdCriteria(
			QueryColumn qc)
		{
			return CriteriaCompoundId.Edit(qc);
		}

		/// <summary>
		/// Edit Structure Criteria proxy
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		public static bool EditStructureCriteria(
			QueryColumn qc)
		{
			return CriteriaStructure.Edit(qc);
		}

		/// <summary>
		/// Edit General Criteria proxy
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		public static bool GetGeneralCriteria(
			QueryColumn qc)
		{
			return Criteria.Edit(qc);
		}

		/// <summary>
		/// Convert complex criteria to labeled form suitable for editing in complex criteria editor
		/// </summary>
		/// <param name="q"></param>
		/// <param name="structures">Dictionary of structure names & connection tables</param>

		public static LabeledCriteria ConvertComplexCriteriaToEditable(
			Query q,
			bool includeEditButtons)
		{
			bool insertBreaks = false;
			if (q.ComplexCriteria.IndexOf("\n") < 0) insertBreaks = true;

			Dictionary<string, string> tAliasMap = GetAliasMap(q);

			if (tAliasMap != null && !includeEditButtons)
			{ // fixup aliases properly first using editable criteria
				ConvertComplexCriteriaToEditable(q, true);
				tAliasMap = null;
			}

			Lex lex = new Lex();
			lex.SetDelimiters(" , ; ( ) < = > <= >= <> != !> !<");
			string criteria = q.ComplexCriteria;
			lex.OpenString(criteria);
			StringBuilder sb = new StringBuilder();
			PositionedToken lastTok = null;

			List<PositionedToken> tokens = new List<PositionedToken>(); // list of tokens seen

			LabeledCriteria lc = new LabeledCriteria();
			lc.Structures = new Dictionary<string, string>();

			while (true)
			{
				PositionedToken tok = lex.GetPositionedToken();
				if (tok == null) break;

				tokens.Add(tok);

				if (lastTok != null)
				{ // include same white space between tokens
					int wsBeg = lastTok.Position + lastTok.Text.Length;
					sb.Append(criteria.Substring(wsBeg, tok.Position - wsBeg));
				}

				QueryColumn qc = MqlUtil.GetQueryColumn(tok.Text, q); // see if token is column ref
				if (qc != null)
				{ //query column, map to labeled columns
					string label = GetUniqueColumnLabel(qc);

					QueryTable qt = qc.QueryTable;
					string tName, cName;
					MqlUtil.ParseColumnIdentifier(tok.Text, out tName, out cName);
					if (tName != null && tName != "") // any table name supplied?
					{
						if (tAliasMap != null && tAliasMap.ContainsKey(tName.ToUpper()))
							tName = tAliasMap[tName.ToUpper()];
						label = tName + "." + label;
					}

					sb.Append(Lex.Dq(label));
				}

				else
				{ // not a query column reference
					string tokText = tok.Text;

					string txt = Lex.RemoveSingleQuotes(tokText).ToUpper();
					if (UserObject.IsCompoundIdListName(txt))
					{
						string listName = null;
						int objectId = int.Parse(txt.Substring(7));
						UserObject uo = UserObjectDao.ReadHeader(objectId);
						if (uo != null) listName = uo.InternalName;
						else listName = "Unknown";
						tokText = Lex.AddSingleQuotes(listName);
					}

					if (tokens.Count >= 5)
					{ // see if this is a chime string
						string sFuncCand = tokens[tokens.Count - 5].Text.ToLower();
						if ((Lex.Eq(sFuncCand, "SSS") || Lex.Eq(sFuncCand, "FSS") || Lex.Eq(sFuncCand, "MolSim")) &&
							tokText.StartsWith("'") && tokText.EndsWith("'")) // single-quoted chime?
						{
							string sAlias = "S" + (lc.Structures.Count + 1).ToString();
							lc.Structures[sAlias] = Lex.RemoveSingleQuotes(tokText); // save structure in dictionary
							if (includeEditButtons)
								tokText = "[Edit Structure " + sAlias + "]"; // use alias in labeled query
							else tokText = Lex.AddSingleQuotes(sAlias);
						}
					}

					if ((Lex.Eq(tokText, "And") || Lex.Eq(tokText, "Or")) &&
						tokens.Count >= 3 && !Lex.Eq(tokens[tokens.Count - 3].Text, "Between") &&
						insertBreaks)
						sb.Append("\n"); // start new line for each and/or
					sb.Append(tokText); // not query column identifier
				}

				lastTok = tok;
			}

			sb.Append(" "); // include final space so additional text is black, also to get correct font
			lc.Text = sb.ToString();

			// If a table alias changes then update aliases & complex criteria but only if going
			// to editable text since ConvertEditableCriteriaToComplex fails otherwise.

			if (tAliasMap != null)
			{
				for (int qti = 0; qti < q.Tables.Count; qti++)
				{ // set new table aliases
					QueryTable qt = q.Tables[qti];
					string alias = "T" + (qti + 1).ToString();
					qt.Alias = alias;
				}

				ConvertEditableCriteriaToComplex(lc, q, null); // update q.ComplexCriteria also
			}

			return lc;
		}

		/// <summary>
		/// Convert labeled criteria to complex checking for errors
		/// </summary>
		/// <param name="criteria"></param>
		/// <param name="q"></param>
		/// <param name="complexCriteriaCtl"></param>

		public static bool ConvertEditableCriteriaToComplex(
			LabeledCriteria labeledCriteria,
			Query q,
			RichTextBox complexCriteriaCtl)
		{
			QueryTable qt;
			QueryColumn qc;

			Lex lex = new Lex();
			lex.SetDelimiters(" , ; ( ) < = > <= >= <> != !> !< [ ]");
			lex.OpenString(labeledCriteria.Text);
			StringBuilder sb = new StringBuilder();
			PositionedToken lastTok = null;

			List<PositionedToken> tokens = new List<PositionedToken>(); // list of tokens seen

			int parenDepth = 0;
			while (true)
			{
				PositionedToken pTok = lex.GetPositionedToken();
				if (pTok == null) break;

				tokens.Add(pTok);

				if (lastTok != null)
				{ // include same white space between tokens
					int wsBeg = lastTok.Position + lastTok.Text.Length;
					sb.Append(labeledCriteria.Text.Substring(wsBeg, pTok.Position - wsBeg));
				}

				string tok = pTok.Text;

				if (MqlUtil.IsKeyWord(pTok.Text)) // ok if keyword, operator, etc
				{
					if (pTok.Text == "(") parenDepth++;
					else if (pTok.Text == ")") parenDepth--;
				}

				else if (pTok.Text.StartsWith("'")) // string constant
				{
					if (tokens.Count >= 3 && Lex.Eq(tokens[tokens.Count - 2].Text, "In")
						&& Lex.Eq(tokens[tokens.Count - 1].Text, "List"))
					{ // saved list reference
						UserObject uo = QueryEngine.ResolveCnListReference(tok);
						if (uo != null)
							tok = "CNLIST_" + uo.Id.ToString();
						else tok = "Nonexistant list";
						tok = Lex.AddSingleQuotes(tok);
					}
				}

				else if (Lex.IsDouble(pTok.Text)) { } // numeric constant

				else if (tok == "[")
				{ // translate editable structure reference
					pTok = lex.GetPositionedToken();
					if (!MatchToken(pTok, "Edit", complexCriteriaCtl)) return false;

					pTok = lex.GetPositionedToken();
					if (!MatchToken(pTok, "Structure", complexCriteriaCtl)) return false;

					pTok = lex.GetPositionedToken();
					tok = Lex.RemoveSingleQuotes(pTok.Text.ToUpper());
					if (!labeledCriteria.Structures.ContainsKey(tok))
					{
						ConvertLabeledCriteriaError("Structure \"" + pTok.Text + "\" not defined", pTok, complexCriteriaCtl);
						return false;
					}
					tok = Lex.AddSingleQuotes(labeledCriteria.Structures[tok]); // replace with chime

					pTok = lex.GetPositionedToken();
					if (!MatchToken(pTok, "]", complexCriteriaCtl)) return false;
				}

				else if (Lex.Eq(pTok.Text, "structure_field"))
				{ // check for user failing to define structure_field in structure search criteria
					ConvertLabeledCriteriaError("\"Structure_field\" must be replaced with a real field name", pTok, complexCriteriaCtl);
					return false;
				}

				else // must be a column reference or invalid token
				{ // translate labeled column name
					tok = TranslateLabeledColumnName(pTok, q, complexCriteriaCtl);
					if (tok == null) return false;
				}

				sb.Append(tok);
				lastTok = pTok;
			}

			tokens = tokens; // debug

			if (parenDepth != 0) // parens balance?
			{
				if (parenDepth > 0)
					MessageBoxMx.ShowError("Unbalanced parentheses: left parentheses exceed right by " + parenDepth.ToString());

				else
					MessageBoxMx.ShowError("Unbalanced parentheses: right parentheses exceed left by " + (-parenDepth).ToString());

				if (complexCriteriaCtl != null) complexCriteriaCtl.Focus();
				return false;
			}

			q.ComplexCriteria = sb.ToString(); // store back in query
			return true;
		}

		/// <summary>
		/// Check that tokens match
		/// </summary>
		/// <param name="pTok"></param>
		/// <param name="matchToken"></param>
		/// <returns></returns>

		static bool MatchToken(
		PositionedToken pTok,
		string matchToken,
		RichTextBox complexCriteriaCtl)
		{
			if (Lex.Eq(pTok.Text, matchToken)) return true;
			ConvertLabeledCriteriaError("Expected keyword \"" + matchToken + "\"", pTok, complexCriteriaCtl);
			return false;
		}

		/// <summary>
		/// Translate a labeled column reference to a normal named reference
		/// </summary>
		/// <param name="pTok"></param>
		/// <returns></returns>

		static string TranslateLabeledColumnName(
			PositionedToken pTok,
			Query q,
			RichTextBox complexCriteriaCtl)
		{
			QueryColumn qc = null;
			int qti, qci;

			string tok = pTok.Text;

			tok = Lex.RemoveDoubleQuotes(tok);
			string tName, cName;
			MqlUtil.ParseColumnIdentifier(tok, out tName, out cName);

			QueryTable qt = q.Tables[0]; // default to first table

			if (tName != "")
			{
				for (qti = 0; qti < q.Tables.Count; qti++)
				{
					qt = q.Tables[qti];
					if (Lex.Eq(qt.Alias, tName)) break;
				}
				if (qti >= q.Tables.Count)
					ConvertLabeledCriteriaError("Invalid table name or alias", pTok, complexCriteriaCtl);
			}

			for (qci = 0; qci < qt.QueryColumns.Count; qci++)
			{
				qc = qt.QueryColumns[qci];
				if (qc.MetaColumn == null) continue;
				if (!qc.MetaColumn.IsSearchable) continue;

				string label = GetUniqueColumnLabel(qc);
				if (Lex.Eq(label, cName)) break;
			}

			if (qci >= qt.QueryColumns.Count)
			{
				qc = MqlUtil.GetQueryColumn(tok, q); // see if metacolumn name
				if (qc == null)
				{
					ConvertLabeledCriteriaError("Unrecognized query element: \"" + pTok.Text + "\"", pTok, complexCriteriaCtl);
					return null;
				}
			}

			string colName = qc.MetaColumn.Name;
			if (tName != "") colName = tName + "." + colName; // include table name or alias if exists
			return colName;
		}

		/// <summary>
		/// Show error message & select token with error
		/// </summary>
		/// <param name="message"></param>
		/// <param name="pTok"></param>

		static void ConvertLabeledCriteriaError(
			string message,
			PositionedToken pTok,
			RichTextBox complexCriteriaCtl)
		{
			Progress.Hide();
			if (complexCriteriaCtl != null)
			{
				complexCriteriaCtl.SelectionStart = pTok.Position;
				complexCriteriaCtl.SelectionLength = pTok.Text.Length;
			}

			MessageBoxMx.ShowError(message);
			if (complexCriteriaCtl != null)
				complexCriteriaCtl.Focus();
			return;
		}

		public static string GetUniqueColumnLabel(
			QueryColumn qc)
		{
			string label = qc.ActiveLabel;
			int matchLabelCount = 0;
			foreach (QueryColumn qc2 in qc.QueryTable.QueryColumns)
			{
				if (Lex.Eq(qc2.ActiveLabel, label)) matchLabelCount++;
			}
			if (matchLabelCount >= 2) label = qc.MetaColumn.Name; // use name if label not unique within query table
			return label;
		}

		/// <summary>
		/// Color criteria to highlight keywords, column names, constants, etc.
		/// </summary>
		/// <param name="criteria"></param>
		/// <param name="q"></param>
		/// <returns></returns>

		public static RichTextBox ColorCodeCriteria(
			string criteria,
			Query q)
		{
			bool inEditStructure = false;
			int i1;

			RichTextBox rtb = new RichTextBox();
			rtb.Text = criteria;
			rtb.SelectionStart = 0;
			rtb.SelectionLength = rtb.Text.Length;
			rtb.SelectionFont = new Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

			PositionedToken lastTok = null;
			Lex lex = new Lex();
			lex.SetDelimiters(" , ; ( ) < = > <= >= <> != !> !< [ ]");
			lex.OpenString(criteria);
			while (true)
			{
				PositionedToken tok = lex.GetPositionedToken();
				if (tok == null) break;

				if (MqlUtil.IsKeyWord(tok.Text))
				{
					rtb.SelectionStart = tok.Position;
					rtb.SelectionLength = tok.Text.Length;
					rtb.SelectionColor = Color.Blue;
					continue;
				}

				QueryColumn qc = MqlUtil.GetQueryColumn(tok.Text, q);
				if (qc != null)
				{ // labeled field name
					rtb.SelectionStart = tok.Position;
					rtb.SelectionLength = tok.Text.Length;
					rtb.SelectionColor = Color.Cyan;
					continue;
				}

				if (tok.Text.StartsWith("/*"))
				{ // comment
					rtb.SelectionStart = tok.Position;
					rtb.SelectionLength = tok.Text.Length;
					rtb.SelectionColor = Color.Green;
					continue;
				}

				if (tok.Text.StartsWith("'") || Lex.IsDouble(tok.Text))
				{ // constant 
					rtb.SelectionStart = tok.Position;
					rtb.SelectionLength = tok.Text.Length;
					rtb.SelectionColor = Color.Red;
					continue;
				}

				if (tok.Text == "[") inEditStructure = true; // color structure placeholder tokens

				if (inEditStructure) // color the tokens in edit structure
				{
					rtb.SelectionStart = tok.Position;
					rtb.SelectionLength = tok.Text.Length;
					rtb.SelectionColor = Color.Red;
					if (tok.Text == "]") inEditStructure = false;
					continue;
				}

			}

			return rtb;
		}

		/// <summary>
		/// Get map of query table alias changes, return null if no alias changes
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		public static Dictionary<string, string> GetAliasMap(
			Query q)
		{
			q.AssignUndefinedAliases(); // be sure everything has an alias

			bool aliasChanged = false;
			Dictionary<string, string> tAliasMap = new Dictionary<string, string>();
			for (int qti = 0; qti < q.Tables.Count; qti++)
			{ // build map from old table aliases to new
				QueryTable qt = q.Tables[qti];
				string alias = "T" + (qti + 1).ToString();
				if (qt.Alias != "") tAliasMap[qt.Alias.ToUpper()] = alias;
				if (!Lex.Eq(qt.Alias, alias)) aliasChanged = true;
			}

			if (aliasChanged) return tAliasMap;
			else return null; // no map if no change
		}

	}

	/// <summary>
	/// Labeled criteria consisting of user viewable text & dictionary of structures
	/// </summary>

	public class LabeledCriteria
	{
		public string Text = ""; // user viewable text of query
		public Dictionary<string, string> Structures = new Dictionary<string, string>(); // associated set of structures in chime format
	}

}
