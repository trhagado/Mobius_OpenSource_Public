using Mobius.ComOps;

using LumenWorks.Framework.IO.Csv;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mobius.Data
{
	public class Csv
	{
		/// <summary>
		/// Split a .Csv format string into an ArrayList of strings
		/// </summary>
		/// <param name="csvString"></param>
		/// <returns></returns>

		public static List<string> SplitCsvString(
			string csvString)
		{
			return SplitCsvString(csvString, false);
		}

		/// <summary>
		/// Split a .Csv format string into an ArrayList of strings
		/// optionally allowing spaces to be used as delimiters
		/// Uses Sébastien Lorion's LumenWorks.Framework.IO.Csv class for parsing.
		///  
		///The MIT License
		///
		///Permission is hereby granted, free of charge, to any person obtaining a copy
		///of this software and associated documentation files (the "Software"), to deal
		///in the Software without restriction, including without limitation the rights
		///to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
		///copies of the Software, and to permit persons to whom the Software is
		///furnished to do so, subject to the following conditions:

		///The above copyright notice and this permission notice shall be included in
		///all copies or substantial portions of the Software.

		///THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
		///IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
		///FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
		///AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
		///LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
		///OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
		///THE SOFTWARE.
		/// </summary>
		/// <param name="csvString"></param>
		/// <returns></returns>

		public static List<string> SplitCsvString(
			string csvString,
			bool allowSpaceDelimiters)
		{
			char delim;

			int t0 = TimeOfDay.Milliseconds();
			List<string> items = new List<string>();
			try
			{
				TextReader tr = new StringReader(csvString);
				bool hasHeaders = false;
				char delimiter = ',';
				if (csvString.IndexOf(",") < 0 && allowSpaceDelimiters)
					delimiter = ' ';
				CsvReader csv = new CsvReader(tr, hasHeaders, delimiter);

				if (!csv.ReadNextRecord()) return items;

				for (int i1 = 0; i1 < csv.FieldCount; i1++)
					items.Add(csv[i1].Trim());

				t0 = TimeOfDay.Milliseconds() - t0;
			}
			catch (Exception ex) { string msg = ex.Message; }
			return items;
		}

		/// <summary>
		/// Join an arraylist of values into a .Csv format string
		/// </summary>
		/// <param name="arrayList"></param>
		/// <returns></returns>

		public static string JoinCsvString(
			List<string> arrayList)
		{
			StringBuilder sb = new StringBuilder();
			for (int si = 0; si < arrayList.Count; si++)
			{
				string s = arrayList[si];
				if (s == null) s = "";

				if (sb.Length > 0) sb.Append(", ");
				if (s.IndexOf(",") > 0 || s.IndexOf(" ") > 0 ||
					s.IndexOf("\'") > 0 || s.IndexOf("\"") > 0)
					sb.Append(Lex.Dq(s));

				else sb.Append(s); // keep as is
			}

			return sb.ToString();
		}

	}
}
