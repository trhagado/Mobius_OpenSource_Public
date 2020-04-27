using Mobius.ComOps;
 
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Mobius.Data
{
	/// <summary>
	/// Compound number list manipulation class
	/// </summary>

	public class CidList
	{
		public UserObject UserObject = new UserObject(UserObjectType.CnList); // name, etc of list
		public List<CidListElement> List = new List<CidListElement>(); // List of CnListElements
    public Dictionary<string, CidListElement> Dict = new Dictionary<string, CidListElement>(); // dictionary of ids in list

		public static ICidListDao ICidListDao; // interface to extended list functionality
		public const string CurrentListInternalName = "TEMP_FOLDER.Current"; // internal name for current list including folder

/// <summary>
/// Default constructor
/// </summary>

		public CidList() 
		{
		}

/// <summary>
/// Constructor given a string of numbers
/// </summary>
/// <param name="cnList"></param>

		public CidList(
			string cnList) 
		{
			if (cnList==null) return;

			Lex lex = new Lex();
			lex.SetDelimiters(",");
			lex.OpenString(cnList);

			while (true)
			{
				string tok = lex.Get();
				if (tok == "") break;
				if (tok == ",") continue;
				this.Add(tok);
			}
		}

/// <summary>
/// Constructor given a string array
/// </summary>
/// <param name="cnList"></param>

		public CidList(
			string [] cidArray)
		{
			if (cidArray == null) return;

			foreach (string cid in cidArray)
				this.Add(cid);
		}

		/// <summary>
		/// Constructor given a string List
		/// </summary>
		/// <param name="cidList"></param>
		/// <param name="allowDuplicates"></param>

		public CidList(
			List<string> cidList)
		{
			if (cidList == null) return;

			foreach (string cid in cidList)
				Add(cid);

			return;
		}

/// <summary>
/// Constructor given a string list and allowing duplicates
/// </summary>
/// <param name="cidList"></param>
/// <param name="allowDuplicates"></param>

		public CidList(
			List<string> cidList,
			bool allowDuplicates)
		{
			if (cidList == null) return;

			foreach (string cid in cidList)
				Add(cid, allowDuplicates);

			return;
		}

/// <summary>
/// Constructor given an ArrayList
/// </summary>
/// <param name="cnList"></param>

		public CidList(
			ArrayList cnList) 
		{
			if (cnList==null) return;

			for (int i1=0; i1<cnList.Count; i1++)
			this.Add((string)cnList[i1]);
		}

		/// <summary>
		/// Constructor given a Dictionary
		/// </summary>
		/// <param name="cnList"></param>

		public CidList(
			Dictionary <string, object> cidDict) 
		{
			if (cidDict==null) return;

			foreach (string s in cidDict.Keys)
				this.Add(s);

			this.Sort();
		}

		/// <summary>
		/// Int indexer for CnList
		/// </summary>

		public CidListElement this[int ei] 
		{
			get
			{ 
				return (CidListElement) List[ei];
			}
			set
			{ 
				List[ei] = value;
			}
		}

/// <summary>
/// Lookup by compound number
/// </summary>
/// <param name="rn"></param>
/// <returns></returns>

		public CidListElement Get (
			string cn)
		{
			if (Dict.ContainsKey(cn))
				return (CidListElement)Dict[cn];
			else return null;
		}


		public int Count 
		{
			get 
			{
				return List.Count;
			}
		}

		public bool Add(
			string cn) 
		{ 
			return Add(cn, false); // no duplicates allowed
		}

		public bool Add( // add new compound number to list return true/false
			string cn,
			bool allowDuplicates) 
		{

			if (!allowDuplicates && Dict.ContainsKey(cn))
				return false; // fail if dup and dups not allowed

			CidListElement e = new CidListElement();
			e.Cid = cn;
			if (!Dict.ContainsKey(cn))
				Dict.Add(cn, e); // add to Dictionary for quick lookup

			List.Add(e); // add to list
			return true;
		}

		public void Remove(string cn) 
		{ // remove cn from list
			CidListElement e = new CidListElement(cn); // create list element for proper compare

			Dict.Remove(e.Cid);
			List.Remove(e);
		}

		public bool Contains(
			string cn) 
		{ 
			if (Dict.ContainsKey(cn)) return true; // lookup in dictionary
			else return false;
		}

/// <summary>
/// Apply the specified type of list logic
/// </summary>
/// <param name="l2"></param>
/// <param name="type"></param>
/// <returns></returns>

		public bool ApplyListLogic(
			CidList l2,
			ListLogicType type)
		{
			if (type == ListLogicType.Union) return Union(l2);
			else if (type == ListLogicType.Intersect) return Intersect(l2);
			else if (type == ListLogicType.Difference) return Difference(l2);
			else throw new Exception("Invalid ListLogicType: " + type);
		}

		/// <summary>
		/// Union of compound number lists
		/// </summary>
		/// <param name="l2"></param>
		/// <returns></returns>

		public bool Union(
			CidList l2) 
		{ 
			bool modified = false;

			foreach (CidListElement e in l2.List)
			{
				if (this.Get(e.Cid) != null) continue; // if already in list then don't add
				List.Add(e);
				try {	Dict.Add(e.Cid,e); }
				catch (Exception ex) {}
				modified = true;
			}

			return modified; // todo: sort first?
		}

		/// <summary>
		/// Intersect Compound Number lists
		/// </summary>
		/// <param name="l2"></param>
		/// <returns></returns>
	
		public bool Intersect(
			CidList l2) 
		{
			List<CidListElement> cidList = new List<CidListElement>();
            Dictionary<string, CidListElement> cidDict = new Dictionary<string, CidListElement>();
			bool modified = false;

			foreach (CidListElement e in List)
			{
				if (l2.Get(e.Cid) == null) // if not in 2nd list then don't include
				{
					modified = true;
					continue;
				}
				cidList.Add(e);
				try {	cidDict.Add(e.Cid,e); }
				catch (Exception ex) {}
			}

			if (modified) 
			{
				List = cidList;
				Dict = cidDict;
			}

			return modified;
		}

		/// <summary>
		/// Difference in Compound Number sets
		/// </summary>
		/// <param name="l2"></param>
		/// <returns></returns>
	
		public bool Difference(
			CidList l2) 
		{
			List<CidListElement> cidList = new List<CidListElement>();
            Dictionary<string, CidListElement> cidDict = new Dictionary<string, CidListElement>();
			bool modified = false;

			foreach (CidListElement e in List)
			{
				if (l2.Get(e.Cid) != null) // if in 2nd list then don't include
				{
					modified = true;
					continue;
				}
				cidList.Add(e);
				try {	cidDict.Add(e.Cid,e); }
				catch (Exception ex) {}
			}

			if (modified) 
			{
				List = cidList;
				Dict = cidDict;
			}
			return modified;
		}

		/// <summary>
		/// Sort a compound number list in ascending order
		/// </summary>

		public void Sort()
		{
			Sort(SortOrder.Ascending);
		}

		/// <summary>
		/// Sort a compound number list
		/// </summary>
		/// <param name="sortDirection"></param>

		public void Sort( 
			SortOrder sortDirection) 
		{ 
			IComparer<CidListElement> comparer;
			CidListElement e;
			int i1;

			for (i1=0; i1<List.Count; i1++) 
			{ // convert numbers to sortable form and store in StringTag
				e = (CidListElement)List[i1];
				e.StringTag = CompoundId.Normalize(e.Cid);
			}

			if (sortDirection == SortOrder.Ascending)
				comparer = new CnComparer();
			else
				comparer = new CnReverseComparer();

			List.Sort(comparer);
		}

		public class CnComparer : IComparer<CidListElement>  
		{

			public int Compare(CidListElement e1, CidListElement e2)  
			{
				return String.Compare(e1.StringTag, e2.StringTag, true);
			}

		}

		public class CnReverseComparer : IComparer<CidListElement>  
		{

			public int Compare(CidListElement e1, CidListElement e2)
			{
				return String.Compare(e2.StringTag, e1.StringTag, true);
			}

		}

/// <summary>
/// Return the contents of a list as a string list
/// </summary>
/// <returns></returns>

		public List<string> ToStringList ()
		{
			List<string> keyList = new List<string>();
			for (int i1=0; i1<this.List.Count; i1++)
			{
				CidListElement e = (CidListElement)this.List[i1];
				if (e.Cid!=null && e.Cid!="")
					keyList.Add(e.Cid);
			}

			return keyList;
		}

/// <summary>
/// Convert a string of cids to a list of strings
/// </summary>
/// <param name="cidListString"></param>
/// <returns></returns>

		public static List<string> ToStringList(string cidListString)
		{
			List<string> cidList = new List<string>();
			string[] sa = cidListString.Split('\n');
			for (int i1 = 0; i1 < sa.Length; i1++)
			{
				string cid = sa[i1];
				if (cid.IndexOf("\r") >= 0) cid = cid.Replace("\r", "");
				if (cid.IndexOf(" ") >= 0) cid = cid.Replace(" ", "");
				if (cid == "") continue;
				cidList.Add(cid); // add allowing duplicates
			}

			return cidList;
		}

		/// <summary>
		/// Convert list to string with one list element per row
		/// </summary>
		/// <returns></returns>

		public string ToMultilineString()
		{
			return ToMultilineString(false);
		}

/// <summary>
/// Get string with one list element per row
/// </summary>
/// <param name="removeLeadingZerosFromCids"></param>
/// <returns></returns>

		public string ToMultilineString(
			bool removeLeadingZerosFromCids)
		{
			string cid;
			int cncnt = 0, intCid;
			StringBuilder sb = new StringBuilder();
			foreach (CidListElement e in List)
			{
				if (e.Cid != "")
				{
					if (sb.Length>0) sb.Append("\r\n");
					cid = e.Cid;
					if (removeLeadingZerosFromCids && int.TryParse(cid, out intCid))
						cid = intCid.ToString();
					else cid = CompoundId.Normalize(cid);
					sb.Append(cid);
					cncnt++;
				}
			}

			//string csvForm = ToListString(removeLeadingZerosFromCids, false); // debug
			return sb.ToString();
		}

		/// <summary>
		/// Convert a CnList to a string with elements separated by commas
		/// </summary>
		/// <returns></returns>

		public override string ToString()
		{
			return ToListString(false, false);
		}
		
/// <summary>
/// Convert list to comma separated form
/// </summary>
/// <param name="removeLeadingZerosFromCids"></param>
/// <param name="quoteItems"></param>
/// <returns></returns>

		public string ToListString(
			bool removeLeadingZerosFromCids,
			bool quoteItems)
		{
			string cid;
			int cncnt = 0, intCid;
			StringBuilder sb = new StringBuilder();
			foreach (CidListElement e in List)
			{
				if (e.Cid != "")
				{
					if (sb.Length > 0) sb.Append(",");
					cid = e.Cid;
					if (removeLeadingZerosFromCids && int.TryParse(cid, out intCid))
						cid = intCid.ToString();
					else cid = CompoundId.Normalize(cid);
					if (quoteItems) cid = Lex.AddSingleQuotes(cid);
					sb.Append(cid);
					cncnt++;
				}
			}

			return sb.ToString();
		}

/// <summary>
/// Clear list
/// </summary>

		public void Clear ()
		{
			this.List.Clear();
			this.Dict.Clear();
		}

/// <summary>
/// Clone list
/// </summary>
/// <returns></returns>

		public CidList Clone()
		{
			CidList cnl = (CidList)this.MemberwiseClone();
			cnl.List = new List<CidListElement>(this.List);
            cnl.Dict = new Dictionary<string, CidListElement>(this.Dict);
			return cnl;
		}

		/// <summary>
		/// Read a list from a file
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public static CidList ReadFromFile(
			string fileName)
		{
			CidList cnList = new CidList();

			if (fileName.IndexOf(".") < 0)
				fileName += ".lst"; // append extension if needed

			StreamReader sr = new StreamReader(fileName);
			while (true)
			{
				string cn = ReadCidFromFile(sr);
				if (cn == null) break;
				cnList.Add(cn, true); // add allowing duplicates
			}
			sr.Close();

			return cnList; // return the list
		}

/// <summary>
/// Read a compound id from a list file
/// </summary>
/// <param name="csf"></param>
/// <returns></returns>

		public static string ReadCidFromFile (
			StreamReader csf)
		{
			string cn;

			while (true)
			{
				cn = csf.ReadLine(); 
				if (cn==null) return null;
				if (cn.IndexOf("\r") >= 0) cn = cn.Replace("\r","");
				if (cn.IndexOf(" ") >= 0) cn = cn.Replace(" ","");
				if (cn=="") continue;
				cn = CompoundId.Normalize(cn);
				return cn;
			}
		}

		/// <summary>
		/// Deserialize compound id list
		/// </summary>
		/// <returns></returns>

		public static CidList Deserialize(
			UserObject uo,
			MetaTable mt)
		{
			CidList cidList = new CidList();
			string[] sa = uo.Content.Split('\n');
			for (int i1 = 0; i1 < sa.Length; i1++)
			{
				string cid = sa[i1];
				if (cid.IndexOf("\r") >= 0) cid = cid.Replace("\r", "");
				if (cid.IndexOf(" ") >= 0) cid = cid.Replace(" ", "");
				if (cid == "") continue;
				cid = CompoundId.Normalize(cid, mt);
				cidList.Add(cid, true); // add allowing duplicates
			}

			cidList.UserObject = uo;
			return cidList; // return the list
		}

		/// <summary>
		/// Write a list to a file
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public int WriteToFile(
			string fileName,
			bool removeLeadingZeros)
		{
			if (fileName.IndexOf(".") < 0)
				fileName += ".lst"; // append extension if needed

			StreamWriter sw = new StreamWriter(fileName);
			string content = ToMultilineString(removeLeadingZeros);
			sw.Write(content);
			sw.Close();

			return List.Count;
		}

		/// <summary>
		/// Format a list of Cids for display
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="listText"></param>
		/// <returns></returns>

		public static string FormatAbbreviatedCidListForDisplay(
			QueryColumn qc,
			string listText)
		{
			List<string> al = Csv.SplitCsvString(listText, true); // split up, allowing space delimiters
			listText = BuildListCsvStringOfFormattedCids(qc, al); // and reformat properly
			if (al.Count > 10) // if many elements, just show the first, last and count
				listText = al[0] + " ... " + al[al.Count - 1] + " (" + al.Count + " total)";

			return listText;
		}

		/// <summary>
		/// Build a csv string of formatted compound ids
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="list"></param>
		/// <returns></returns>

		public static string BuildListCsvStringOfFormattedCids(
			QueryColumn qc,
			List<string> list)
		{
			int li, rem;

			List<string> fList = new List<string>();
			for (li = 0; li < list.Count; li++)
			{
				string cid = list[li];
				if (qc != null && qc.QueryTable != null && qc.QueryTable.MetaTable != null)
				{
					string fCid = CompoundId.Format(cid, qc.QueryTable.MetaTable);
					fList.Add(fCid);
					list[li] = fCid; // also store back in supplied List<string>
				}

				else fList.Add(cid);
			}

			string csvString = Csv.JoinCsvString(fList);

			if (DebugMx.False) // debug - create a list of sublists for testing
			{
				StringBuilder sb = new StringBuilder();
				for (li = 0; li < list.Count; li++)
				{
					string cid = list[li];
					Math.DivRem(li, 100, out rem);
					if (rem == 0) sb.Append("\r\n");
					else sb.Append(",");
					sb.Append(cid);
				}

				string s = sb.ToString();
			}

			return csvString;
		}


	} // end of CidList class

/// <summary>
/// Type of list logic
/// </summary>

	public enum ListLogicType
	{
		Unknown    = 0,
		Intersect  = 1,
		Union      = 2,
		Difference = 3
	}

/// <summary>
/// Interface to CidListDao operations called from modules common to both client & server
/// </summary>

	public interface ICidListDao
	{
		CidList VirtualRead(
			int listId,
			MetaTable mt);
	}

} // end of namespace
