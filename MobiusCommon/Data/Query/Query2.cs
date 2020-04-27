using Mobius.ComOps;

using System;
using System.IO;
using System.Text;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace Mobius.Data
{

/// <summary>
/// Utility methods on Query object 
/// </summary>

	public partial class Query
	{

/// <summary>
/// For non-complex queries move any key criteria the first table in the query and remove from any other tables
/// </summary>

		public void NormalizeKeyCriteria()
		{
			QueryColumn qc = null;

			if (LogicType == QueryLogicType.Complex) return;

			if (Tables.Count == 0) return;

			foreach (QueryTable qt in Tables)
			{
				qc = qt.KeyQueryColumn;
				if (qc != null) qc.Criteria = qc.CriteriaDisplay = "";
			}

			if (Lex.IsUndefined(KeyCriteria)) return;

			qc = Tables[0].KeyQueryColumn;
			if (qc == null) return;

			qc.Criteria = qc.MetaColumnName + " " + KeyCriteria;
			qc.CriteriaDisplay = KeyCriteriaDisplay;

			return;
		}

		/// <summary>
		/// Set the table index values
		/// </summary>

		public void SetTableIndexes()
		{
			QueryTable qt;
			int ti, mti = -1;

			for (ti = 0; ti < Tables.Count; ti++)
			{
				qt = Tables[ti];
				qt._tableIndex = ti;
			}

			IndexedTableCount = Tables.Count;
			return;
		}

		/// <summary>
		/// Define aliases for each table for simple criteria queries.
		/// This method should not be called for queries with complex criteria that
		/// already reference the aliases since the remap will create errors.
		/// </summary>

		public void ResetTableAliases()
		{
			QueryTable qt;

			for (int qti = 0; qti < Tables.Count; qti++)
			{ // build map from old table aliases to new
				qt = Tables[qti];
				qt.Alias = "T" + (qti + 1).ToString();
			}

			return;
		}

		/// <summary>
		/// Be sure each query table has a unique alias
		/// </summary>

		public int AssignUndefinedAliases()
		{
			QueryTable qt;
			int dupCnt = 0, assignCnt = 0;
			int qid = InstanceId; // for debug

			Dictionary<string, string> aliasMap = new Dictionary<string, string>();
			for (int qti = 0; qti < Tables.Count; qti++)
			{ // build map from old table aliases to new
				qt = Tables[qti];
				if (qt.Alias == "") continue;
				if (aliasMap.ContainsKey(qt.Alias.ToUpper())) // already assigned, reassign if so
				{
					if (this.LogicType == QueryLogicType.Complex) // if this is a complex query then can't reassign alias
						throw new Exception("Non-unique query table alias " + qt.Alias);
					qt.Alias = ""; // clear now & reassign below
					dupCnt++;
					continue;
				}

				aliasMap[qt.Alias.ToUpper()] = null;
			}

			int ai = 1; // start with this alias
			for (int qti = 0; qti < Tables.Count; qti++)
			{ // build map from old table aliases to new
				qt = Tables[qti];
				if (qt.Alias != "") continue;

				string alias = null;
				while (true)
				{
					alias = "T" + ai.ToString();
					ai++;
					if (!aliasMap.ContainsKey(alias)) break;
				}

				qt.Alias = alias;
				assignCnt++;
			}

			if (dupCnt > 0 || assignCnt > 0) qid = InstanceId; // debug
			return assignCnt;
		}

		/// <summary>
		/// Get label for col that is unique within the query
		/// </summary>

		public bool HasDuplicateNames
		{
			get
			{
				if (!DuplicateNamesAndLabelsMarked)
					MarkDuplicateNamesAndLabels();

				foreach (QueryTable qt in Tables)
				{
					if (qt.HasDuplicateNames) return true;
				}

				return false;
			}
		}

		public bool HasDuplicateLabels
		{
			get
			{
				if (!DuplicateNamesAndLabelsMarked)
					MarkDuplicateNamesAndLabels();

				foreach (QueryTable qt in Tables)
				{
					if (qt.HasDuplicateNames) return true;
				}

				return false;
			}
		}

/// <summary>
///  Mark duplicate table/column names and labels
/// </summary>

		public void MarkDuplicateNamesAndLabels()
		{
			Query q = this;
			QueryColumn qc;
			string tableAlias, aliasOrdinal;
			string name, label, newName, newNames = "";
			bool keyIsDup;

			HashSet<string> tableNames = new HashSet<string>(); // internal metatable names seen
			HashSet<string> tableLabels = new HashSet<string>();
			HashSet<string> columnNames = new HashSet<string>(); // internal metacolumn names seen
			HashSet<string> columnLabels = new HashSet<string>();

			AssignUndefinedAliases(); // be sure aliases are defined for all tables

			for (int ti = 0; ti < Tables.Count; ti++)
			{
				QueryTable qt = q.Tables[ti];
				qt.HasDuplicateNames = qt.HasDuplicateLabels = false;

				MetaTable mt = qt.MetaTable;
				name = mt.Name.ToUpper();
				label = qt.ActiveLabel.ToUpper();


				if (tableNames.Contains(name))
					qt.HasDuplicateNames = true;

				if (tableLabels.Contains(label))
					qt.HasDuplicateLabels = true;

				List<QueryColumn> uniqueNameCols, dupNameCols, uniqueLabelCols, dupLabelCols;

				CountDuplicateNames(qt, columnNames, columnLabels, 
					out uniqueNameCols, out dupNameCols, out uniqueLabelCols, out dupLabelCols);

				if (dupNameCols.Count > 0)
					qt.HasDuplicateNames = true;

				if (dupLabelCols.Count > 0)
					qt.HasDuplicateLabels = true;
			}

			DuplicateNamesAndLabelsMarked = true; // indicate that this has been done
			return;
		}

/// <summary>
/// Count how many names in this table match names already seen
/// </summary>
/// <param name="qt"></param>
/// <param name="suffix"></param>
/// <param name="columnNames"></param>
/// <param name="columnLabels"></param>
/// <param name="keyIsDup"></param>
/// <returns></returns>

		void CountDuplicateNames(
			QueryTable qt,
			HashSet<string> columnNames,
			HashSet<string> columnLabels,
			out List<QueryColumn> uniqueNameCols, 
			out List<QueryColumn> dupNameCols,
			out List<QueryColumn> uniqueLabelCols,
			out List<QueryColumn> dupLabelCols)
		{
			uniqueNameCols = new List<QueryColumn>();
			dupNameCols = new List<QueryColumn>();
			uniqueLabelCols = new List<QueryColumn>();
			dupLabelCols = new List<QueryColumn>();

			for (int ci = 0; ci < qt.QueryColumns.Count; ci++) // see how many cols are duplicates
			{
				QueryColumn qc = qt.QueryColumns[ci];
				//if (!qc.Selected) continue;

				MetaColumn mc = qc.MetaColumn;
				string name = mc.Name.ToUpper();
				if (!columnNames.Contains(name))
				{
					uniqueNameCols.Add(qc);
					columnNames.Add(name);
				}
				else dupNameCols.Add(qc);

				string label = qc.ActiveLabel.ToUpper();
				if (!columnLabels.Contains(label))
				{
					uniqueLabelCols.Add(qc);
					columnLabels.Add(label);
				}
				else dupLabelCols.Add(qc);
			}

			return;
		}

		/// <summary>
		/// AssignNewUniqueName
		/// </summary>
		/// <param name="ti"></param>
		/// <param name="qc"></param>
		/// <param name="columnNames"></param>
		/// <returns></returns>

		static string AssignNewUniqueName(
			int ti,
			QueryColumn qc,
			HashSet<string> columnNames)
		{
			string name = qc.MetaColumn.Name;
			name += "_" + (ti + 1).ToString(); // base name on table index
			if (columnNames.Contains(name))
			{
				for (int i0 = 1; ; i0++)
				{
					if (!columnNames.Contains(name + i0))
					{
						name = name + i0;
						break;
					}
				}
			}

			columnNames.Add(name);
			return name;
		}

		/// <summary>
		/// Set simple vo positions for selected QueryColumns starting at offset value
		/// </summary>
		/// <param name="voOffset"></param>
		/// <returns></returns>

		public int SetSimpleVoPositions(
			int voOffset = 0)
		{
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			int ti, fi;

			int voLength = 0;

			for (ti = 0; ti < Tables.Count; ti++)
			{
				qt = Tables[ti];
				qt.VoPosition = -1;

				for (fi = 0; fi < qt.QueryColumns.Count; fi++)
				{
					//if (qt.Alias == "T24") qt = qt; // debug

					qc = qt.QueryColumns[fi];
					qc.VoPosition = -1;

					if (!qc.Is_Selected_or_GroupBy_or_Sorted) continue; // only selected, grouped & sorted fields
					if (qc.MetaColumn == null) continue; // must have metacolumn

					voLength++; // accumulate length of Vo

					int oldVop = qc.VoPosition;
					qc.VoPosition = voOffset + voLength;
					if (qt.VoPosition < 0) qt.VoPosition = qc.VoPosition; // position for first vo element for table
				}
			}

			return voLength;
		}


		/// <summary>
		/// ResetVoPositions
		/// </summary>

		public void ResetVoPositions()
		{
			foreach(QueryTable qt in Tables)
			{
				qt.VoPosition = -1;
				foreach (QueryColumn qc in qt.QueryColumns)
				{
					qc.VoPosition = -1;
				}
			}

			return;
		}

	}
}
