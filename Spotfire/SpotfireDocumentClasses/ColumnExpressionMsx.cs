using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Mobius.SpotfireDocument
{

/// <summary>
/// AxisExpressionMsx - Expression associated with an axis dimension
/// Can be parsed into one or more ColumnExpressionMsx's containing details on
/// the associated column names, method names, etc.
/// </summary>

	public class AxisExpressionMsx
	{
		public string Expression = ""; // the text of the expression
		public List<ColumnExpressionMsx> ColExprList = new List<ColumnExpressionMsx>(); // list of parsed column expressions in axis expression

		/// <summary>
		/// Parse an axis expression
		/// </summary>
		/// <param name="axisExpr"></param>
		/// <returns></returns>

		public static AxisExpressionMsx	Parse(string axisExpr)
		{
			AxisExpressionMsx ax = new AxisExpressionMsx();
			ax.Expression = axisExpr;

			List<string> colExprs = Split(axisExpr);

			foreach (string colExpr in colExprs)
			{
				ax.ColExprList.Add(ColumnExpressionMsx.Parse(colExpr));
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
	}

	public class ColumnExpressionMsx
	{
		public string Expression = ""; // the text of the expression
		public List<string> MethodNames = new List<string>();
		public string Alias = "";
		public List<string> HierarchyNames = new List<string>();
		public List<string> ColumnNames = new List<string>();


		public static ColumnExpressionMsx Parse(string expr)
		{
			Lex lex = new Lex();
			lex.SetDelimiters(" , ; ( ) [ ]");
			lex.OpenString(expr);
			StringBuilder sb = new StringBuilder();
			PositionedToken lastTok = null, leftBracket = null;

			ColumnExpressionMsx colExpr = new ColumnExpressionMsx();
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
			}

			return colExpr;
		}

	}
}
