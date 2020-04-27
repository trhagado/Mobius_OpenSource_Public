
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Mobius.ComOps
{
	/// <summary>
	/// Parse a list of parameters of standard types parameter list
	/// </summary>
	
	public class LexParmParser
	{
		public string ParmString = ""; // original string
		public string ParmDelim = ""; // original delimiter

		public Dictionary<string, string> V1Dict = new Dictionary<string, string>(); // low or only value
		public Dictionary<string, string> V2Dict = new Dictionary<string, string>(); // high value if range

/// <summary>
/// Constructor
/// </summary>
/// <param name="parmString">Input parameter string</param>
/// <param name="parmDelim">Delimiter used between parameters</param>

		public LexParmParser(
			string parmString,
			string parmDelim)
		{
			string pn, v1, v2, tok;
			int i0, i1, i2;

			ParmString = parmString;
			ParmDelim = parmDelim;

			string[] sa = Lex.Split(parmString, parmDelim);
			foreach (string s2 in sa)
			{
				i0 = s2.IndexOf("=");
				if (i0 < 0)
				{
					pn = s2.Trim().ToLower();
					V1Dict[pn] = "";
					continue;
				}

				pn = s2.Substring(0, i0).Trim().ToLower();

				//if (Lex.Eq(pn, "sub")) pn = pn; // debug
				v1 = s2.Substring(i0 + 1).Trim();
				v2 = "";
				i1 = i2 = -1;
				if (v1.Contains("-")) // if range value, break it down
				{
					string[] sa3 = v1.Split('-');
					if (Lex.IsInteger(sa3[0]) && Lex.IsInteger(sa3[1])) // integer range?
					{
						v1 = sa3[0].Trim();
						v2 = sa3[1].Trim();
					}
				}

				V1Dict[pn] = v1;
				if (v2 != "") V2Dict[pn] = v2;
			}
		}

/// <summary>
/// Get string parameter value
/// </summary>
/// <param name="parmName"></param>
/// <param name="s1"></param>
/// <returns></returns>

		public bool GetParm(
			string parmName,
			out string s1)
		{
			string s2;

			return GetParm(parmName, out s1, out s2);
		}

/// <summary>
/// Parse a range
/// </summary>
/// <param name="parmName"></param>
/// <param name="s1"></param>
/// <param name="s2"></param>
/// <returns></returns>
		
public bool GetParm(
			string parmName,
			out string s1,
			out string s2)
		{
			s1 = s2 = null;

			parmName = parmName.ToLower();
			if (!V1Dict.ContainsKey(parmName)) return false;

			s1 = V1Dict[parmName];
			if (V2Dict.ContainsKey(parmName))
				s2 = V2Dict[parmName];

			return true;
		}

		public bool GetIntParm(
			string parmName,
			out int i1)
		{
			string s1;

			i1 = NumberEx.NullNumber;

			if (!GetParm(parmName, out s1)) return false;
			return int.TryParse(s1, out i1);
		}

		/// <summary>
		/// Get an integer range parm
		/// </summary>
		/// <param name="parmName"></param>
		/// <param name="i1"></param>
		/// <param name="i2"></param>
		/// <returns></returns>

		public bool GetIntRangeParm(
			string parmName,
			out int i1,
			out int i2)
		{
			string s1, s2;
			i1 = i2 = NumberEx.NullNumber;

			if (!GetParm(parmName, out s1, out s2)) return false;
			if (!int.TryParse(s1, out i1) ||
			 !int.TryParse(s2, out i2)) return false;
			else return true;
		}

		public bool GetFloatParm(
			string parmName,
			out float f1)
		{
			string s1;

			f1 = NumberEx.NullNumber;


			if (!GetParm(parmName, out s1)) return false;
			return float.TryParse(s1, out f1);
		}

		public bool GetDoubleParm(
			string parmName,
			out double d1)
		{
			string s1;

			d1 = NumberEx.NullNumber;

			if (!GetParm(parmName, out s1)) return false;
			return double.TryParse(s1, out d1);
		}

		public bool GetBoolParm(
			string parmName,
			out bool b1)
		{
			string s1;
			b1 = true;

			if (!GetParm(parmName, out s1)) return false;

			if (Lex.IsUndefined(s1)) return true; // if no definition then assume true
			else return bool.TryParse(s1, out b1);
		}

	}
}
