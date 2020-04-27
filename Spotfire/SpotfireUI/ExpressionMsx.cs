using Mobius.ComOps;
using Mobius.SpotfireDocument;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Mobius.SpotfireClient
{

	/// <summary>
	/// ParsedExpressionListMsx - Expression list normally associated with an axis dimension
	/// Can be parsed into one or more ColumnExpressionMsx's containing details on
	/// the associated column names, method names, etc.
	/// </summary>

	public class ParsedExpressionListMsx
	{
		public string Expression = ""; // the text of the expression
		public List<ParsedColumnExpressionMsx> ColExprList = new List<ParsedColumnExpressionMsx>(); // list of parsed column expressions in axis expression

		/// <summary>
		/// Parse an axis expression
		/// </summary>
		/// <param name="axisExpr"></param>
		/// <returns></returns>

		public static ParsedExpressionListMsx Parse(string axisExpr)
		{
			ParsedExpressionListMsx ax = new ParsedExpressionListMsx();
			ax.Expression = axisExpr;

			List<string> colExprs = Split(axisExpr);

			foreach (string colExpr in colExprs)
			{
				ParsedColumnExpressionMsx cx = ParsedColumnExpressionMsx.Parse(colExpr);
				ax.ColExprList.Add(cx);
				cx.ParentAxisExpressionList = ax;
			}

			return ax;
		}

		/// <summary>
		/// Split an axis expression into a list of column expressions
		/// </summary>
		/// <param name="axisExpr"></param>
		/// <returns></returns>

		public static List<string> Split(string axisExpr)
		{
			List<string> colExprs = new List<string>();

			Lex lex = new Lex();
			lex.SetDelimiters(" , ( ) ");
			lex.OpenString(axisExpr);
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
					sb.Append(axisExpr.Substring(wsBeg, pTok.Position - wsBeg));
				}

				string tok = pTok.Text;

				if (pTok.Text == "(") parenDepth++;
				else if (pTok.Text == ")") parenDepth--;

				if (Lex.Eq(pTok.Text, ",") && parenDepth == 0 && sb.Length > 0)
				{
					colExprs.Add(sb.ToString());
					sb.Clear();
				}

				else sb.Append(tok);

				lastTok = pTok;
			}

			if (sb.Length > 0) // final col expr
			{
				colExprs.Add(sb.ToString());
			}

			return colExprs;
		}

		/// <summary>
		/// Format an axis expression by catenating the associated individual 
		/// collumn expressions with comma separators
		/// </summary>
		/// <returns></returns>

		public string Format()
		{
			string expr = "";

			for (int ei = 0; ei < ColExprList.Count; ei++)
			{
				if (Lex.IsDefined(expr)) expr += ", ";

				ParsedColumnExpressionMsx ce = ColExprList[ei];

				string colExpr = ce.FormatEscapedExpression();
				expr += colExpr;
			}

			return expr;
		}

	} //	AxisExpressionListMsx


	/// <summary>
	/// ParsedColumnExpressionMsx
	/// If multiple expressions are defined for an axis then this will be one of those expressions in parsed form
	/// </summary>

	public class ParsedColumnExpressionMsx
	{
		public ParsedExpressionListMsx ParentAxisExpressionList; // parent AxisExpressionList that this expression belongs to
		public string Expression = ""; // the text of the expression

		public string AggregationMethod { get => GetAggregationMethod(); set => SetAggregationMethod(value); }
		public List<string> MethodNames = new List<string>(); // list of method names

		public string ColumnName { get => GetUnescapedColumnName(); set => SetColumnName(value); }
		public string TableName { get => GetUnescapedTableName(); set => SetTableName(value); }
		public string UnescapedColumnAndTableName => GetUnescapedColumnAndTableNameString(); // not escaped table.col or col name
		public string EscapedColumnAndTableName => GetEscapedColumnAndTableNameString(); // single column escaped [table].[col] or [col] name

		public List<string> EscapedColumnAndTableNames = new List<string>(); // source list of escaped table, column names (e.g. [colname] (if from main vis table) or [tableName].[colName])

		public string Alias = ""; // un-escaped alias (escaped in full expression)

		public List<string> HierarchyNames = new List<string>();

/// <summary>
/// Get any single aggregation method name
/// </summary>
/// <returns></returns>

		public string GetAggregationMethod()
		{
			if (MethodNames != null && MethodNames.Count > 0)
				return MethodNames[0];

			else return "";
		}

/// <summary>
/// Set a single aggregation method name
/// </summary>
/// <param name="methodName"></param>

		public void SetAggregationMethod(string methodName)
		{
			MethodNames = new List<string>();

			if (Lex.IsDefined(methodName))
				MethodNames.Add(methodName);

			return;
		}

		/// <summary>
		/// GetUnescapedColumnName
		/// </summary>
		/// <returns></returns>

		public string GetUnescapedColumnName()
		{
			string colName, tableName;
			GetUnescapedColumnAndTableName(out colName, out tableName);
			return colName;
		}

		/// <summary>
		/// GetUnescapedTableName
		/// </summary>
		/// <returns></returns>

		public string GetUnescapedTableName()
		{
			string colName, tableName;
			GetUnescapedColumnAndTableName(out colName, out tableName);
			return tableName;
		}

		/// <summary>
		/// SetColumnName
		/// </summary>
		/// <returns></returns>

		public void SetColumnName(string newColName)
		{
			string colName, tableName;
			GetUnescapedColumnAndTableName(out colName, out tableName);
			SetColumnAndTableName(newColName, tableName);
		}


		/// <summary>
		/// SetTableName
		/// </summary>
		/// <returns></returns>

		public void SetTableName(string newTableName)
		{
			string colName, tableName;
			GetUnescapedColumnAndTableName(out colName, out tableName);
			SetColumnAndTableName(colName, newTableName);
		}

		/// <summary>
		/// Return unescaped columnName or tableName.columnName string
		/// </summary>
		/// <returns></returns>

		public string GetUnescapedColumnAndTableNameString()
		{
			string colName, tableName;

			GetUnescapedColumnAndTableName(out colName, out tableName);

			if (Lex.IsUndefined(colName)) return "";

			else if (Lex.IsUndefined(tableName)) return colName;

			else return tableName + "." + colName;
		}

		/// <summary>
		/// Get unescaped col and table name in two out parameters
		/// </summary>
		/// <param name="colName"></param>
		/// <param name="tableName"></param>

		public void GetUnescapedColumnAndTableName(
			out string colName,
			out string tableName)
		{
			GetEscapedColumnAndTableName(out colName, out tableName);
			colName = UnescapeIdentifier(colName);
			tableName = UnescapeIdentifier(tableName);
			return;
		}

		/// <summary>
		/// GetEscapedColumnAndTableNameString
		/// </summary>
		/// <returns></returns>

		public string GetEscapedColumnAndTableNameString()
		{
			string colName, tableName;

			GetEscapedColumnAndTableName(out colName, out tableName);
			if (Lex.IsDefined(tableName))
				return tableName + "." + colName;

			else return colName;
		}

		/// <summary>
		/// Get unescaped col and table name
		/// </summary>
		/// <param name="colName"></param>
		/// <param name="tableName"></param>

		public void GetEscapedColumnAndTableName(
			out string colName,
			out string tableName)
		{
			colName = "";
			tableName = "";

			if (EscapedColumnAndTableNames.Count == 0) return;

			string ectn = EscapedColumnAndTableNames[0];

			int i = ectn.IndexOf("].[");
			if (i > 0 && !ectn.StartsWith("].[") && !ectn.EndsWith("].["))
			{
				tableName = ectn.Substring(0, i + 1);
				colName = ectn.Substring(i + 2);
				return;
			}

			else colName = ectn; // assume just col name without table name

			return;
		}

		/// <summary>
		/// Set [columnName] or [tableName].[columnName] from column [and table name] input(s)
		/// Can handle escaped or unescaped names
		/// </summary>
		/// <param name="colName"></param>
		/// <param name="tableName"></param>
		/// <returns></returns>

		public void SetColumnAndTableName(
			string colName,
			string tableName = "")
		{
			string ectn = "";

			if (Lex.IsDefined(colName))
			{
				if (Lex.IsDefined(tableName))
					ectn = EscapeIdentifier(tableName) + "." + EscapeIdentifier(colName);

				else ectn = EscapeIdentifier(colName);

				EscapedColumnAndTableNames = new List<string>() { ectn };
			}

			else EscapedColumnAndTableNames = new List<string>(); // not defined

			return;
		}


		/// <summary>
		/// Parse an expression
		/// </summary>
		/// <param name="expr"></param>
		/// <returns>ParsedColumnExpressionMsx</returns>

		public static ParsedColumnExpressionMsx Parse(string expr)
		{
			Lex lex = new Lex();
			lex.SetDelimiters(" , ; ( ) [ ]");
			lex.OpenString(expr);
			StringBuilder sb = new StringBuilder();
			PositionedToken lastTok = null, leftBracket = null;
			bool inTableDotColumnName = false;

			ParsedColumnExpressionMsx pce = new ParsedColumnExpressionMsx();
			pce.Expression = expr;
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
					sb.Append(expr.Substring(wsBeg, pTok.Position - wsBeg));
				}

				string tok = pTok.Text;

				if (pTok.Text == "(") parenDepth++;
				else if (pTok.Text == ")") parenDepth--;

				else if (tok == "[")
				{
					leftBracket = pTok;
					int tc = tokens.Count;
					inTableDotColumnName = // set to true if at the second left bracket in a [tableName].[columnName] string
						(tc >= 3 && tokens[tc - 2].Text == "." && tokens[tc - 3].Text == "]");
				}

				else if (tok == "]")
				{
					if (leftBracket != null)
					{
						int p = leftBracket.Position + 1;
						int l = pTok.Position - p;
						string colName = expr.Substring(p, l);
						if (!inTableDotColumnName)
						{
							colName = EscapeIdentifier(colName);
							pce.EscapedColumnAndTableNames.Add(colName); // store [colName] format name
						}

						else // finishing last part of a [tableName].[columnName] string
						{
							int i = pce.EscapedColumnAndTableNames.Count - 1; // previous name index

							string tableName = pce.EscapedColumnAndTableNames[i]; // really was the table name
							colName = EscapeIdentifier(colName);
							pce.EscapedColumnAndTableNames[i] = tableName + "." + colName; // store [tableName].[colName] format name

							inTableDotColumnName = false;
						}

						leftBracket = null;
					}
				}

				else if (Lex.Eq(tok, "as"))
				{
					pTok = lex.GetPositionedToken();
					if (pTok == null) break;
					tokens.Add(pTok);

					string alias = pTok.Text;
					if (Lex.IsDefined(alias))
						pce.Alias = UnescapeIdentifier(alias);

					//pce.Alias = expr.Substring(pTok.Position + 2).Trim();
				}

				lastTok = pTok;
			}

			return pce;
		}

		/// <summary>
		/// Format a parsed expression into internal escaped format from the parsed pieces
		/// </summary>
		/// <returns></returns>

		public string FormatEscapedExpression()
		{
			if (Lex.IsUndefined(ColumnName)) return ""; // no column selected

			string expr = "";

			if (Lex.IsDefined(AggregationMethod))
				expr += AggregationMethod + "(";

			string colName = GetEscapedColumnAndTableNameString(); // [colName] or [tableName].[colName]
			expr += colName;

			if (Lex.IsDefined(AggregationMethod))
				expr += ")";

			if (Lex.IsDefined(Alias))
				expr += " as " + EscapeIdentifier(Alias);

			return expr;
		}

		/// <summary>
		/// Format a parsed expression into external label format from the parsed pieces
		/// 
		/// </summary>
		/// <returns></returns>

		public string FormatUnescapedExpression()
		{
			string expr = "";

			if (Lex.IsDefined(Alias)) return Alias; // just return the alias as the label

			if (Lex.IsDefined(AggregationMethod))
				expr += AggregationMethod + "(";

			if (EscapedColumnAndTableNames.Count > 0)
			{
				string colName = GetUnescapedColumnAndTableNameString();
				expr += colName;
			}

			if (Lex.IsDefined(AggregationMethod))
				expr += ")";

			return expr;
		}

		/// <summary>
		/// Get the label for the underlying column or pseudocolumn
		/// </summary>
		/// <returns></returns>

		public string GetColumnLabel()
		{
			string tableName, colName;
			GetUnescapedColumnAndTableName(out colName, out tableName);
			string label = colName;
			return label;
		}

		/// <summary>
		/// Get the DataTable associated with the ColSelection
		/// If not in the expression use the main visual table reference
		/// </summary>
		/// <returns></returns>

		public DataColumnMsx GetDataColumn(
			DataTableMsx defaultTable,
			List<DataTableMsx> tableList)
		{
			string tableName, colName;
			DataTableMsx dt = null;
			DataColumnMsx dc = null;

			GetUnescapedColumnAndTableName(out colName, out tableName);
			if (Lex.IsUndefined(tableName))
				dt = defaultTable;

			else dt = DataTableCollectionMsx.GetTableByName(tableName, tableList);

			if (dt == null) return null;

			if (dt.TryGetColumnByName(colName, out dc))
				return dc;

			else return null;
		}

		/// <summary>
		/// Get the index of colName in the ColumnNames list
		/// </summary>
		/// <returns></returns>

		public int GetColumnNameIndex(string colName)
		{
			if (Lex.IsUndefined(colName) || EscapedColumnAndTableNames == null)
				return -1;

			for (int i = 0; i < EscapedColumnAndTableNames.Count; i++)
			{
				if (Lex.Eq(EscapedColumnAndTableNames[i], colName))
					return i;
			}

			return -1;
		}

		public void Clear()
		{
			ParsedColumnExpressionMsx pce2 = new ParsedColumnExpressionMsx();

			ObjectEx.MemberwiseCopy(pce2, this);
			return;
		}

		/// <summary>
		/// EscapeIdentifier
		/// </summary>
		/// <param name="identifier"></param>
		/// <returns></returns>

		public static string EscapeIdentifier(string identifier)
		{
			return ExpressionUtilities.EscapeIdentifier(identifier);
		}

		/// <summary>
		/// UnescapeIdentifier
		/// </summary>
		/// <param name="identifier"></param>
		/// <returns></returns>

		public static string UnescapeIdentifier(string identifier)
		{
			return ExpressionUtilities.UnescapeIdentifier(identifier);
		}

	} //ParsedColumnExpressionMsx

	public class ExpressionUtilities // Spotfire.Dxp.Data.Expressions.ExpressionUtilities
	{
		public static string EscapeIdentifier(string identifier)
		{
			//Robustness.ValidateArgumentNotNull("identifier", identifier);

			if (identifier.StartsWith("[") && identifier.EndsWith("]")) return identifier; // already escaped

			string text = identifier;

			//text = Preprocessor.EscapeExpression(identifier);

			text = text.Replace("[", "[[");
			text = text.Replace("]", "]]");

			return "[" + text + "]";
		}

		public static string UnescapeIdentifier(string identifier)
		{
			//Robustness.ValidateArgumentNotNull("identifier", identifier);

			if (identifier.Length < 2 || !identifier.StartsWith("[") || !identifier.EndsWith("]"))
				return identifier; // already unexcaped

			string text = identifier.Substring(1, identifier.Length - 2);
			text = text.Replace("]]", "]");
			text = text.Replace("[[", "[");
			text = Preprocessor.UnescapeExpression(text);
			return text;
		}

	}

	public class Preprocessor // Spotfire.Dxp.Data.Expressions.Preprocessing.Preprocessor
	{
		internal static string EscapeExpression(string expression)
		{
			int num = expression.IndexOf('$');
			if (num == -1)
			{
				return expression;
			}
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(expression.Length + 10);
			stringBuilder.Append(expression.Substring(0, num));
			for (int i = num; i < expression.Length; i++)
			{
				if (expression[i] != '$' || i == expression.Length - 1)
				{
					stringBuilder.Append(expression[i]);
				}
				else if (expression[i + 1] == '{')
				{
					stringBuilder.Append("\\${");
					i++;
				}
				else if (Preprocessor.Match("map", expression, i + 1))
				{
					stringBuilder.Append("\\$map");
					i += 3;
				}
				else if (Preprocessor.Match("esc", expression, i + 1))
				{
					stringBuilder.Append("\\$esc");
					i += 3;
				}
				else if (Preprocessor.Match("csearch", expression, i + 1))
				{
					stringBuilder.Append("\\$csearch");
					i += 7;
				}
				else
				{
					stringBuilder.Append(expression[i]);
				}
			}
			return stringBuilder.ToString();
		}

		internal static string UnescapeExpression(string expression)
		{
			int num = expression.IndexOf("\\$", System.StringComparison.Ordinal);
			if (num == -1)
			{
				return expression;
			}
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(expression.Length);
			stringBuilder.Append(expression.Substring(0, num));
			for (int i = num; i < expression.Length; i++)
			{
				if (expression[i] == '\\' && i + 2 < expression.Length && expression[i + 1] == '$')
				{
					if (expression[i + 2] == '{')
					{
						stringBuilder.Append("${");
						i += 2;
					}
					else if (Preprocessor.Match("map", expression, i + 2))
					{
						stringBuilder.Append("$map");
						i += 4;
					}
					else if (Preprocessor.Match("esc", expression, i + 2))
					{
						stringBuilder.Append("$esc");
						i += 4;
					}
					else if (Preprocessor.Match("csearch", expression, i + 2))
					{
						stringBuilder.Append("$csearch");
						i += 8;
					}
					else
					{
						stringBuilder.Append("\\$");
						i++;
					}
				}
				else
				{
					stringBuilder.Append(expression[i]);
				}
			}
			return stringBuilder.ToString();
		}



		private static bool Match(string stringToMatch, string str, int startIndex)
		{
			if (startIndex + stringToMatch.Length > str.Length)
			{
				return false;
			}
			for (int i = 0; i < stringToMatch.Length; i++)
			{
				if (str[startIndex + i] != stringToMatch[i])
				{
					return false;
				}
			}
			return true;
		}

	} // Preprocessor

}

