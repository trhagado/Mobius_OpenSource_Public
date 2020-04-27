using Mobius.Data;
using Mobius.ComOps;
using Mobius.ServiceFacade;

using DevExpress.XtraGrid.Columns;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Drawing;
using System.Windows.Forms;


namespace Mobius.ClientComponents
{

	/// <summary>
	/// This class contains formatting information on each table in the results.
	/// </summary>

	public class ResultsTable
	{
		public MetaTable MetaTable; // associated metatable
		public QueryTable QueryTable; // associated querytable
		public ResultsFormat ResultsFormat; // associated results format
		public MobiusDataType Header; // table header
		public int Position = -1; // position of this table within the ResultsTables list
		public int Width; // width for table if formatted as a single segment
		public int FirstSegment = -1; // first segment the table is a part of
		public List<ResultsField> Fields = new List<ResultsField>(); // field information

		public ResultsTable()
		{
			return;
		}

		/// <summary>
		/// Lookup a results column by the underlying metacolumn name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public ResultsField GetResultsFieldByName(
			string name)
		{
			int fi = GetResultsFieldIndexByName(name);
			if (fi >= 0) return Fields[fi];
			else return null;
		}

		/// <summary>
		/// Lookup a results column by the underlying metacolumn name and return the column index
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public int GetResultsFieldIndexByName(
				string name)
		{
			for (int fi = 0; fi < Fields.Count; fi++)
			{
				ResultsField rfld = Fields[fi];
				MetaColumn mc = rfld.MetaColumn;
				if (Lex.Eq(mc.Name, name))
					return fi;
			}

			return -1;
		}

	} // ResultsTable

}
