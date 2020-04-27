using Mobius.ComOps;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Mobius.Data
{

/// <summary>
/// Result set sorting
/// </summary>

	public class ResultsSorter
	{

/// <summary>
/// Sort a dataset
/// </summary>
/// <param name="results">Data to sort</param>
/// <param name="sortColumns">Columns to sort on</param>
/// <param name="query">Query information with vo positions of sort cols</param>
/// <param name="keyValueVoPos">Index of added key value</param>
		/// <param name="ResultKeys">Ordered keys are returned here</param>
/// <returns></returns>
/// 
		public List<object[]> Sort(
			List<object[]> results,
			List<SortColumn> sortColumns,
			Query query,
			int keyValueVoPos,
			out List<string> ResultKeys)
		{
			object [] vo, vo2;
			int qti, ri, ti, sci, i1, i2, i3;

			SortItem currentVal = null; // current top composite value (high or low) for current key
			object value;
			object [] sourceVo, destVo; 

			string key;
			SortItem sItem = null, sItem2;
			int keyOffset=0; // pk is stored in first element of vo and must be offset

			ValidateSortColumns(sortColumns);

// Get array of sort columns associated with each metatable. This will be
// used in the sort routine to do the local sorts in the context of
// each table

			bool[][] mtSortCols = new bool[query.Tables.Count][];

			MetaTableSortItems mtsi;
			int mtsiIdx;
			int firstQtWithSorting = -1;
			for (sci=0; sci<sortColumns.Count; sci++)
			{ 
				QueryColumn qc = sortColumns[sci].QueryColumn;
				for (qti=0; qti<query.Tables.Count; qti++)
				{
//					if (Query.Tables[qti] == qc.QueryTable) break; // (fails for preview)
					if (Lex.Eq((query.Tables[qti]).MetaTable.Name, qc.QueryTable.MetaTable.Name)) 
						break; // todo: properly handle when same metatable in query multiple times
				}
				if (qti >= query.Tables.Count) throw new Exception("Sort column not found in table");

				if (mtSortCols[qti] == null) // allocate array first time
					mtSortCols[qti] = new bool[sortColumns.Count];

				mtSortCols[qti][sci] = true;

				if (firstQtWithSorting<0) firstQtWithSorting = qti;
			}

			MultiColumnSortComparer sortComp = // sort including context of 1st table with sorting
				new MultiColumnSortComparer(sortColumns,mtSortCols[firstQtWithSorting]); 

// Build the array of values to be sorted. Each element of the array consists of
// one or more primary values (i.e. the max or min value for each compound id
// depending on the SortDirection for the column), the compound id and 
// one or more secondary values (i.e. the actual value for the sort key for that row)

			string currentKey = "";
			int firstRowForKey = -1, rowsForKey;
			List<object> sArray = new List<object>();
			
			object[] voResults = null;

			for (ri=0; ri<results.Count; ri++)
			{
				vo = (object [])results[ri];
				key = (string)vo[keyValueVoPos];
				sItem = new SortItem();
				sItem.SortValue = new IComparable[sortColumns.Count]; 
				sItem.SortValueB = new IComparable[sortColumns.Count]; 
				sArray.Add(sItem);
				sItem.Key = key;
				sItem.TupleIndex = ri;

				for (sci=0; sci<sortColumns.Count; sci++)
				{ // copy the primary & secondary values from result buffer into sort value array
					QueryColumn qc = sortColumns[sci].QueryColumn;

					SortOrder direction = sortColumns[sci].Direction;

					if (qc.IsKey) // if key be sure to get non-null value
						sItem.SortValue[sci] = sItem.SortValueB[sci] = key;

					else // copy non-key values 
					{
						object o = null;

						if (qc.VoPosition >= 0 && qc.VoPosition < vo.Length) // be sure in range
							o = vo[qc.VoPosition]; // get primitive or Mobius data type

						else if (NullValue.IsNull(o)) o = null;

						else if (o is MobiusDataType) // convert Mobius types to a comparable primitive type
						{
							if (o is NumberMx)
								o = (o as NumberMx).Value;
							else if (o is QualifiedNumber)
							{
								QualifiedNumber qn = o as QualifiedNumber;
								if (MetaColumn.IsNumericMetaColumnType(qc.MetaColumn.DataType))
									o = qn.NumberValue;
								else if (qc.MetaColumn.DataType == MetaColumnType.String)
									o = qn.TextValue;
								else o = qn.NumberValue; // shouldn't happen
							}
							else if (o is StringMx)
								o = (o as StringMx).Value;
							else if (o is DateTimeMx)
								o = (o as DateTimeMx).Value;
							else if (o is CompoundId)
								o = (o as CompoundId).Value;
						}

						else if (o is byte || o is sbyte || // convert all numbers to doubles so they compare properly (e.g. int that has cond formatting NumberMx values for some rows)
						 o is Int16 || o is Int32 || o is Int64 ||
						 o is float || o is decimal)
							o =  Convert.ToDouble(o);

						if (!(o is IComparable)) o = null; // be sure it's a IComparable
						sItem.SortValue[sci] = sItem.SortValueB[sci] = (IComparable)o;
					}
				}

				if (key!=null && key!=currentKey) // new key value
				{
					firstRowForKey = ri;
					currentKey = key;
					currentVal = sItem; 
				}

				else // another tuple for same key
				{
					if (sortComp.CompareSortItems(currentVal,sItem) > 0) // new primary value?
					{
						currentVal = sItem;
						for (i1=firstRowForKey; i1<ri; i1++) 
						{ // reset primary value for preceeding rows for this key to the value of this row
							sItem2 = (SortItem)sArray[i1];
							for (sci=0; sci<sortColumns.Count; sci++)
								sItem2.SortValue[sci] = currentVal.SortValue[sci];
						}
					}

					else // copy current primary values to this sort item 
						for (sci=0; sci<sortColumns.Count; sci++)
							sItem.SortValue[sci] = currentVal.SortValue[sci];
				}
			}

// Do the first sort which is overall for key order & relative for
// first table with sort columns
			
			sArray.Sort(sortComp);

// Reorder arraylist of raw tuples according to initial search results

			List<object[]> newResults = new List<object[]>(results.Count);
			for (ri=0; ri<sArray.Count; ri++)
			{
				sItem = (SortItem)sArray[ri];
				newResults.Add(results[sItem.TupleIndex]);
			}

// Setup range in Vo for each query table and its key

			List<SortTableData> std = new List<SortTableData>();
			foreach (QueryTable qt0 in query.Tables)
			{
				SortTableData st = new SortTableData();
				std.Add(st);
				foreach (QueryColumn qc0 in qt0.QueryColumns)
				{
					MetaColumn mc = qc0.MetaColumn;
					if (mc.IsKey)
						st.KeyColPos = qc0.VoPosition;

					if (qc0.VoPosition >= 0 && st.FirstColumn < 0) 
						st.FirstColumn = qc0.VoPosition;

					if (qc0.Selected) st.SelectCount++;
				}
			}

// Do table-relative sort for other tables with sort fields

			ArrayList CopyBuf = new ArrayList(); // used for copying
			int voLen = 0; // length of vo
			if (results != null && results.Count > 0 && results[0] != null)
				voLen = ((object [])results[0]).Length; 

			for (ti=0; ti<std.Count; ti++) // check all tables
			{
				int voTablePos = std[ti].FirstColumn; // +std[ti].KeyColPos + keyOffset; // position of first element of vo for table
				int voTableLen = std[ti].SelectCount;

				if (ti == firstQtWithSorting) continue; // did this table first
				else if (mtSortCols[ti] == null) continue; // no sorting on this one

				sortComp = // proper sort comparer for this table
					new MultiColumnSortComparer(sortColumns, mtSortCols[ti]); 
				sArray.Sort(sortComp); // re-sort original results for this table

				currentKey = "";
				firstRowForKey = -1;
				rowsForKey=0;

				for (ri=0; ri<=sArray.Count; ri++) // reorder table vo entries for new results
				{

					if (ri<sArray.Count)
					{
						sItem = (SortItem)sArray[ri];
						vo = (object [])results[sItem.TupleIndex]; // address the vo
						key = (string)vo[keyValueVoPos];
					}

					else key = "<end>"; // past end of data, force copy back of last chunk

					if (key!=null && key!=currentKey) // new key value
					{
						if (firstRowForKey >= 0) // anything to copy back
						{
							for (i1=0; i1 < rowsForKey; i1++)
							{ // copy back to results buffer in proper order
								sourceVo = (Object[])CopyBuf[i1];
								destVo = (Object[])newResults[firstRowForKey + i1];
								Array.Copy(sourceVo,voTablePos,destVo,voTablePos,voTableLen);
							}
						}

						if (ri >= sArray.Count) break; // really done?

						currentKey = key;
						firstRowForKey = ri; // first row in newResults for the key
						rowsForKey = 0;
					}

					if (CopyBuf.Count < rowsForKey+1) // be sure copy buffer big enough
						CopyBuf.Add(new object[voLen]);

					sourceVo = (Object[])results[sItem.TupleIndex]; // copy from original results row to buffer
					destVo = (Object[])CopyBuf[rowsForKey];
					Array.Copy(sourceVo,voTablePos,destVo,voTablePos,voTableLen);
					rowsForKey++;

				} // end of loop on results entries
			} // end of loop on query tables

// For each key shift non-null metatable data to top of section for key

			ResultKeys = new List<string>(); // reorder result keys also
			currentKey = "";
			for (ri=0; ri<newResults.Count; ri++)
			{
				vo = (object [])newResults[ri];
				key = (string)vo[keyValueVoPos];

				if (key!=null && key!=currentKey) // new key value
				{
					firstRowForKey = ri;
					currentKey = key;
					ResultKeys.Add(key);
				}

				//else // another row for key, shift up data as needed to fill gaps
				{
					if (query.Tables.Count <= 1) continue; // only need to do if more than one table
					for (ti=0; ti<std.Count; ti++) // check all tables (sorted tables should be ok except for root table is sorted on key
					{
						int voTablePos = std[ti].FirstColumn; // +std[ti].KeyColPos + keyOffset; // position of first element of vo for table
						int voTableLen = std[ti].SelectCount;

						if (NullValue.IsNull(vo[voTablePos])) continue; // skip if null data for this table (i.e. key is null)
						for (i2=firstRowForKey; i2<ri; i2++) // look for empty slot
						{
							vo2 = (object [])newResults[i2];
							object o = vo2[voTablePos];
							if (!NullValue.IsNull(o)) continue; // already full

							Array.Copy(vo,voTablePos,vo2,voTablePos,voTableLen);
							Array.Clear(vo,voTablePos,voTableLen);
							break;
						}
					}
				}
			}

			return newResults; // substitute new ordered set of results
		} 

/// <summary>
/// Check to see that columns are sortable
/// </summary>
/// <param name="sortColumns"></param>

		public void ValidateSortColumns (
			List<SortColumn> sortColumns)
		{
					for (int sci = 0; sci < sortColumns.Count; sci++)
			{ // be sure all columns are sortable
				QueryColumn qc = sortColumns[sci].QueryColumn;
				MetaColumnType t = qc.MetaColumn.DataType;
				if (t == MetaColumnType.Image || t == MetaColumnType.Structure)
					throw new Exception("Can't sort on column " + Lex.Dq(qc.MetaColumn.Label) +
						" because it is of type " + t.ToString());
			}
			return;
		}

		/// <summary>
		/// Sort keyset in specified direction
		/// </summary>
		/// <param name="keySet"></param>
		/// <param name="sortAscendingInitially"></param>

		public static void SortKeySet(
			List<string> keySet,
			SortOrder sortDirection)
		{
			if (sortDirection == SortOrder.None) return;
			CidSortComparer csc = new CidSortComparer(sortDirection);
			keySet.Sort(csc); // put key set in proper sorted order
		}

	} // ResultsSorter

/// <summary>
/// Sort column information
/// </summary>

	public class SortColumn
	{
		public QueryColumn QueryColumn;
		public SortOrder Direction;
		public int Position;
	}

/// <summary>
/// Region in Vo for each table in query
/// </summary>

	public class SortTableData
	{
		public QueryTable Table = null; // query table this data is for
		public int KeyColPos = -1; // position of key column in vo
		public int FirstColumn = -1; // position of the first column for the table in the vo
		public int SelectCount = 0; // count of columns selected for table
	}

/// <summary>
/// List of SortItems for a metatable
/// </summary>

		public class MetaTableSortItems
		{
			public MetaTable MetaTable; // Metatable items are for
			public MultiColumnSortComparer SortComp; 
			public ArrayList SortItems; // List of sortitems for table
		}

/// <summary>
/// Sort information for a single column
/// </summary>

		public class SortItem
		{
			public string Key;
			public IComparable [] SortValue; // sort all tuples for key on this value (min or max value)
			public IComparable [] SortValueB; // sort tuples within a key on this value
			public int TupleIndex; // tuple position in the original array
		}

/// <summary>
/// Describes the range of tuple rows for a single key value (key stored in hash)
/// </summary>

		public class KeySection
		{
			public int Index; // index where section for key begins
			public int Count; // number of tuples for key
		}

		/// <summary>
		/// Class for doing sort compares of field data
		/// </summary>

		public class MultiColumnSortComparer : IComparer<object> 
		{

			SortOrder [] DirectionArray;
			bool [] ActiveArray;

/// <summary>
/// This constructor passes in the sort direction for each
/// element of the array to be sorted whether it should be actively used
/// </summary>
/// <param name="columns"></param>
/// <param name="sActiveList"></param>

			public MultiColumnSortComparer(
				List<SortColumn> columns,
				bool [] sActive) 
			{
				DirectionArray = new SortOrder[columns.Count];
				for (int i1=0; i1<columns.Count; i1++)
					DirectionArray[i1] = columns[i1].Direction;

				ActiveArray = (bool [])sActive.Clone();
			}
		
			int IComparer<object>.Compare(
				object x, 
				object y) 
			{
				SortItem sx = (SortItem)x;
				SortItem sy = (SortItem)y;

				return CompareSortItems(sx,sy);
			}

			public int CompareSortItems (
				SortItem sx,
				SortItem sy)
			{
				int ci, result;

				for (ci=0; ci<DirectionArray.Length; ci++)
				{ // compare max,min value for sort column for each key
					result = SingleColumnSortComparer.CompareValues(sx.SortValue[ci],sy.SortValue[ci],DirectionArray[ci]);
					if (result!=0) return result;
				}

				result = SingleColumnSortComparer.CompareValues(sx.Key, sy.Key, SortOrder.Ascending); // see if same key
				if (result!=0) return result;

				for (ci=0; ci<DirectionArray.Length; ci++)
				{ // compare specific values
					if (!ActiveArray[ci]) continue;	
					
					result = SingleColumnSortComparer.CompareValues(sx.SortValueB[ci],sy.SortValueB[ci],DirectionArray[ci]);
					if (result!=0) return result;
				}
				return result;
			}
		}

	public class SingleColumnSortComparer : IComparable, IComparer<object>
	{
		public SortOrder Direction = SortOrder.Ascending;

/// <summary>
/// Compare two vo field values (IComparable.CompareTo method)
/// </summary>
/// <param name="o"></param>
/// <returns></returns>

		public virtual int CompareTo(
			object o)
		{
			return CompareValues(this, o, Direction);
		}

/// <summary>
/// Compare two vo field values (IComparer.Compare method)
/// </summary>
/// <param name="x"></param>
/// <param name="y"></param>
/// <returns></returns>

		int IComparer<object>.Compare(
			object x,
			object y)
		{
			return CompareValues(x, y, Direction);
		}

		/// <summary>
		/// Compare two vo field values
		/// </summary>
		/// <param name="direction"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>

		public static int CompareValues(
			object x,
			object y,
			SortOrder direction)
		{
			int result;
			bool reverseResult = (direction == SortOrder.Descending);

			if (NullValue.IsNull(x)) x = null; // set any MobiusDataType null to real null

			if (NullValue.IsNull(y)) y = null;

			if (x != null && y != null) // both not null 
			{
				result = ((IComparable)x).CompareTo((IComparable)y);
				if (reverseResult) result = -result;
				return result;
			}

			else if (x == null && y == null) // both null, say equal 
			{
				return 0;
			}

			else if (x != null) // x not null, y is null, put x first
			{
				return -1; // say x < y, non-null always comes before null
			}

			else // x is null, y not null, put y first
			{
				return 1; // x > y, non-null always comes before null
			}
		}


	}

	/// <summary>
	/// Class for doing simple compound id sort compares
	/// </summary>

	public class CidSortComparer : IComparer<string> 
		{
			SortOrder Direction;

			public CidSortComparer(SortOrder direction) 
			{
				this.Direction = direction;
			}

			int IComparer<string>.Compare(string s1, string s2) 
			{
				int result = String.Compare(s1, s2, true);
				if (Direction == SortOrder.Descending) result = -result;
				return result;
			}
		}

}
