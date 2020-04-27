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
	/// Identifier operations (Table & column names...)
	/// </summary>

	public class IdentifierMx
	{
		public const int MaxOracleIdentiferLength = 30; // maximum identifier length for name (i.e. max oracle identifier length)

		/// <summary>
		/// Convert a name to a valid Oracle name
		/// </summary>
		/// <param name="arg"></param>
		/// <returns></returns>

		public static string TextToValidOracleName( // convert a text string to a valid Oracle name
			string arg,
			int maxLen = MaxOracleIdentiferLength)
		{
			StringBuilder name = new StringBuilder(), name2;
			char c;
			int i1;

			for (i1 = 0; i1 < arg.Length; i1++)
			{
				c = Char.ToLower(arg[i1]);
				if (Char.IsLetterOrDigit(c) || c == '$' || c == '#' || c == '_') name.Append(c); // ok as is
				else if (c == ' ' || c == '-' || c == ',' || c == '.' || c == ';' || c == ':')
					name.Append('_'); // convert to underscore
				else if (c == '%') name.Append("_pct_");
				else if (c == '=') name.Append("_eq_");
				else if (c == '>') name.Append("_gt_");
				else if (c == '<') name.Append("_lt_");
				else if (c == '+') name.Append("_pls_");
				else if (c == '/') name.Append("_div_");
				else if (c == '*') name.Append("_ast_");
				else if (c == '(') name.Append("_lp_");
				else if (c == ')') name.Append("_rp_");
				else if (c == '[') name.Append("_lb_");
				else if (c == ']') name.Append("_rb_");

				else name.Append("_"); // convert other chars to underscore
			}

			while (true) // remove double underlines
			{
				name2 = name.Replace("__", "_");
				if (name2.Length == name.Length) break;
				name = name2;
			}

			if (name[0] == '_') name[0] = ' ';
			string sName = name.ToString().Trim().ToUpper();
			if (!Char.IsLetter(sName[0])) sName = "V" + sName; // be sure name starts with letter

			if (sName.Length > maxLen) // remove underscores & vowels
			{
				name = new StringBuilder();
				for (i1 = 0; i1 < sName.Length; i1++)
				{
					c = sName[i1];
					if (c == '_' ||
						c == 'a' ||
						c == 'e' ||
						c == 'i' ||
						c == 'o' ||
						c == 'u') continue;
					name.Append(c);
				}
			}

			if (sName.Length > maxLen) // remove some chars in center as last resort
			{
				int lenToRemove = sName.Length - maxLen;
				int l1 = sName.Length / 2 - lenToRemove / 2;
				sName = sName.Substring(0, l1) + sName.Substring(l1 + lenToRemove);
			}

			// todo: check for reserved words

			return sName;
		}


	}
}
