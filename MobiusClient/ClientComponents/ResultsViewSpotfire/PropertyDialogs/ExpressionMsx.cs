using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Mobius.SpotfireClient
{

	/// <summary>
	/// AxisExpressionMsx - Expression associated with an axis dimension
	/// Can be parsed into one or more ColumnExpressionMsx's containing details on
	/// the associated column names, method names, etc.
	/// </summary>

	public class AxisExpressionListMsx
	{
		public string Expression = ""; // the text of the expression
		public List<ParsedColumnExpressionMsx> ColExprList = new List<ParsedColumnExpressionMsx>(); // list of parsed column expressions in axis expression

		/// <summary>
		/// Parse an axis expression
		/// </summary>
		/// <param name="axisExpr"></param>
		/// <returns></returns>

		public static AxisExpressionListMsx Parse(string axisExpr)
		{
			AxisExpressionListMsx ax = new AxisExpressionListMsx();
			ax.Expression = axisExpr;

			List<string> colExprs = Split(axisExpr);

			foreach (string colExpr in colExprs)
			{
				ParsedColumnExpressionMsx cx = ParsedColumnExpressionMsx.Parse(colExpr);
				ax.ColExprList.Add(cx);
				cx.AxisExpression = ax;
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
				if (ei > 0) expr += ", ";

				ParsedColumnExpressionMsx ce = ColExprList[ei];

				string colExpr = ce.Format();
				expr += colExpr;
			}

			return expr;
		}

	}

	public class ParsedColumnExpressionMsx
	{
		public AxisExpressionListMsx AxisExpression; // parent that this expression belongs to
		public string Expression = ""; // the text of the expression
		public List<string> MethodNames = new List<string>();
		public string Alias = "";
		public List<string> HierarchyNames = new List<string>();
		public List<string> ColumnNames = new List<string>();

		public static ParsedColumnExpressionMsx Parse(string expr)
		{
			Lex lex = new Lex();
			lex.SetDelimiters(" , ; ( ) [ ]");
			lex.OpenString(expr);
			StringBuilder sb = new StringBuilder();
			PositionedToken lastTok = null, leftBracket = null;

			ParsedColumnExpressionMsx colExpr = new ParsedColumnExpressionMsx();
			colExpr.Expression = expr;
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
					leftBracket = pTok;

				else if (tok == "]")
				{
					if (leftBracket != null)
					{
						int p = leftBracket.Position + 1;
						int l = pTok.Position - p;
						string colName = expr.Substring(p, l);
						colExpr.ColumnNames.Add(colName);

						leftBracket = null;
					}
				}

				else if (Lex.Eq(tok, "as"))
				{
					pTok = lex.GetPositionedToken();
					if (pTok == null) break;
					tokens.Add(pTok);

					colExpr.Alias = expr.Substring(pTok.Position + 2).Trim();
				}

				lastTok = pTok;
			}

			return colExpr;
		}

		/// <summary>
		/// Format an expression from the parsed pieces
		/// </summary>
		/// <returns></returns>

		public string Format()
		{
			string expr = "";
			if (MethodNames.Count > 0)
				expr += MethodNames[0] + "(";

			if (ColumnNames.Count > 0)
			{
				string colName = ColumnNames[0];
				colName = ExpressionUtilities.EscapeIdentifier(colName);
				expr += colName;
			}

			if (MethodNames.Count > 0)
				expr += ")";

			if (Lex.IsDefined(Alias))
				expr += " as " + Alias;

			return expr;
		}

	}

	public class ExpressionUtilities // Spotfire.Dxp.Data.Expressions.ExpressionUtilities
	{
		public static string EscapeIdentifier(string identifier)
		{
			//Robustness.ValidateArgumentNotNull("identifier", identifier);
			if (identifier.StartsWith("[")) return identifier; // already escaped

			string text = Preprocessor.EscapeExpression(identifier);
			return "[" + text.Replace("]", "]]") + "]";
		}

		public static string UnescapeIdentifier(string identifier)
		{
			//Robustness.ValidateArgumentNotNull("identifier", identifier);
			if (identifier.Length < 2 || !identifier.StartsWith("[") || !identifier.EndsWith("]"))
			{
				throw new System.ArgumentException("The identifier '{0}' was not escaped. " + identifier);
			}
			string expression = identifier.Substring(1, identifier.Length - 2).Replace("]]", "]");
			return Preprocessor.UnescapeExpression(expression);
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

	}

}
