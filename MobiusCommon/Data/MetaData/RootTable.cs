using Mobius.ComOps;

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace Mobius.Data
{
	/// <summary>
	/// Information on set of root MetaTables and the keys associated with those tables
	/// </summary>

	public class RootTable
	{
		static List<RootTable> TableList = null; // ordered list of structure tables
		static Dictionary<string,RootTable> TableDictionary = null; // dictionary of RootTable entries indexed by metatable name (lowercase)
		static Dictionary<string,RootTable> PrefixDictionary = null; // dictionary of RootTable entries indexed by Prefix (lowercase)
		static Dictionary<string,RootTable> LabelDictionary = null; // dictionary of RootTable entries indexed by structure database label (lowercase) (i.e. external DB name)

		public static IServerFile IServerFile; // interface to method to read in the RootTables.xml file

// Attributes for the root table

		public string Label = ""; // label for table
		public int DatabaseId = -1; // Database Id. Used for sorting structure search results by database. Similar to UniChem database id
		public string MetaTableName = ""; // name of table
		public bool   IsStructureTable = true; // true if this is the root table of a CompoundId/Structure based database
		public bool   CartridgeSearchable = true; // cartridge structure searching is available
		public bool   EcfpSearchable = false; // Extended Connectivity FingerPrint similarity searching is available
		public bool SmallWorldSearchable { get { return Lex.IsDefined(SmallWorldDbName); } }
		public string SmallWorldDbName = ""; // internal name of any associated SmallWorld database (e.g. chembl_21.anon)
		public string CidUrl = ""; // url template that retrieves basic data for a CID, e.g. "https://www.ebi.ac.uk/chembl/compound/inspect/[CID]"

		// Attributes for key values:
		// 1. A key is stored in a numeric or a string database field (e.g. Corp is numeric, ACD is string)
		// 2. Keys for a table may or may not have a prefix (e.g. Corp has no prefix, ACD does)
		// 3. The prefix may or may not be stored in the database (e.g. Stored for ACD, not stored for GeneGo
		// 4. Negative numbers may be allowed (e.g. GeneGo)
		// 5. An internal and external format for the numeric portion of the key may be specified. 

		public bool   IsStringType = false; // if true is stored in a string data type otherwise stored as number
		public string Prefix = ""; // compound identifier prefix (value needed to add to dict)
		public bool   PrefixIsStored = false; // true if prefix is stored in database
		public bool   NegativeNumberAllowed = false; // if true cid can be a negative number
		public string InternalNumberFormat = ""; // C# format for normalizing numeric portion of cid for internal/database use, e.g. "8:00000000"
		public string ExternalNumberFormat = ""; // C# format for formatting numeric portion of cid for display to user

/// <summary>
/// Keys are prefixed for display
/// </summary>

		public bool HasSinglePrefix // return true if a single prefix exists for the root table
		{
			get
			{
				 if (Lex.IsNullOrEmpty(Prefix) || Lex.Eq(Prefix, "none")) // blank, null or "none" prefix
					return false; // then not formatted

				else return true; // otherwise it is
			}
		}

		/// <summary>
		/// Keys in table have multiple prefixes and shouldn't be mixed with other tables
		/// </summary>

		public bool HasMultiplePrefixes
		{
			get
			{
				if (Lex.Eq(Prefix, "multiple")) return true;
				else return false;
			}
		}

		/// <summary>
		/// Return true if in table have multiple prefixes and shouldn't be mixed with other tables
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static bool IsMultiplePrefixTable(MetaTable mt)
		{
			if (mt == null) return false;
			RootTable rti = RootTable.GetFromTableName(mt.Root.Name);
			if (rti == null) return false;

			return rti.HasMultiplePrefixes;
		}

/// <summary>
/// Constructor
/// </summary>

		public RootTable()
		{
		}

		/// <summary>
		/// Retrieve StructureDatabase dictionary info
		/// </summary>
		/// 

		public static List<RootTable> GetList ()
		{
			if (TableList == null) Build();

			return TableList;
		}

		/// <summary>
		/// Get RootTable object for a prefix
		/// </summary>
		/// <param name="prefix"></param>
		/// <returns></returns>

		public static RootTable GetFromPrefix(
			string prefix)
		{
			if (PrefixDictionary == null) Build();

			if (prefix == "") prefix = "none";
			prefix = prefix.Trim().ToLower();
			if (PrefixDictionary.ContainsKey(prefix))
				return PrefixDictionary[prefix];

			else if (PrefixDictionary.ContainsKey(prefix + "-"))
				return PrefixDictionary[prefix + "-"];

			else return null;
		}

		/// <summary>
		/// Try to get root table from table name, label and SmallWorld db name in order
		/// </summary>
		/// <param name="tableId"></param>
		/// <returns></returns>

		public static RootTable GetFromTableId(
			string tableId)
		{
			RootTable rt = GetFromTableName(tableId);
			if (rt == null)
				rt = GetFromTableLabel(tableId);

			if (rt == null)
				rt = GetFromSwName(tableId);

			return rt;
		}

		/// <summary>
		/// Get RootTable object given a structure table name
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>

		public static RootTable GetFromTableName (
			string tableName)
		{
			if (TableDictionary == null) Build();

			tableName = tableName.Trim().ToLower();
			foreach (RootTable rt in TableList) // do ordered scan since same metatable name can appear more than once (e.g. smallworld)
			{
				if (Lex.Eq(rt.MetaTableName, tableName)) return rt;
			}

			return null;
		}

		/// <summary>
		/// Get RootTable object for a criteria item
		/// </summary>
		/// <param name="ValueListItem"></param>
		/// <returns></returns>

		public static RootTable GetFromTableLabel (
			string label)
		{
			const string udbNamePrefix = "USER_DATABASE_";
			if (LabelDictionary == null) Build();

			label = Lex.RemoveAllQuotes(label).Trim().ToLower();
			if (LabelDictionary.ContainsKey(label))
				return LabelDictionary[label];
			else return null;
		}

/// <summary>
/// Get RootTable for a SW database name
/// </summary>
/// <param name="swName"></param>
/// <returns></returns>

		public static RootTable GetFromSwName(
			string swName)
		{
			if (TableList == null) Build();
			foreach (RootTable rt in TableList)
			{
				if (Lex.Eq(rt.SmallWorldDbName, swName)) return rt;
			}

			return null;
		}

		/// <summary>
		/// Serialize RootTable information
		/// </summary>
		/// <returns></returns>

		public static string Serialize()
		{
			MemoryStream ms = new MemoryStream();
			XmlTextWriter tw = new XmlTextWriter(ms, null);
			tw.Formatting = Formatting.Indented;
			tw.WriteStartDocument();

			Serialize(tw);

			tw.WriteEndDocument();
			tw.Flush();

			byte[] buffer = new byte[ms.Length];
			ms.Seek(0, 0);
			ms.Read(buffer, 0, (int)ms.Length);
			string content = System.Text.Encoding.ASCII.GetString(buffer, 0, (int)ms.Length);
			tw.Close(); // must close after read
			return content;
		}

/// <summary>
/// Serialize the DB info
/// </summary>
/// <param name="tw"></param>

		public static void Serialize(
				XmlTextWriter tw)
		{
			tw.WriteStartElement("RootTables");
			foreach (RootTable rti in TableList)
			{
				tw.WriteStartElement("RootTable");
				tw.WriteAttributeString("Label", rti.Label);
				tw.WriteAttributeString("DbId", rti.DatabaseId.ToString());
				tw.WriteAttributeString("MetaTable", rti.MetaTableName);
				tw.WriteAttributeString("IsStructureTable", rti.IsStructureTable.ToString());
				tw.WriteAttributeString("CartridgeSearchable", rti.CartridgeSearchable.ToString());
				tw.WriteAttributeString("EcfpSearchable", rti.EcfpSearchable.ToString());
				tw.WriteAttributeString("SmallWorldDbName", rti.SmallWorldDbName);
				tw.WriteAttributeString("CidUrl", rti.CidUrl);
				tw.WriteAttributeString("Prefix", rti.Prefix.ToString());
				tw.WriteAttributeString("PrefixIsStored", rti.PrefixIsStored.ToString());
				tw.WriteAttributeString("IsStringType", rti.IsStringType.ToString());
				tw.WriteAttributeString("NegativeNumberAllowed", rti.NegativeNumberAllowed.ToString());
				tw.WriteAttributeString("InternalFormat", rti.InternalNumberFormat);
				tw.WriteAttributeString("ExternalFormat", rti.ExternalNumberFormat);

				tw.WriteEndElement();
			}
			tw.WriteEndElement();
			return;
		}

		/// <summary>
		/// Read & deserialize root table information
		/// </summary>

		public static void Build()
		{
			if (IServerFile == null) throw new Exception("ServerFile instance is not defined");

			string fileName = @"<MetaDataDir>\RootTables.xml";
			string content = IServerFile.ReadAll(fileName);
			Deserialize(content);
			return;
		}

/// <summary>
/// Deserialize root table information
/// </summary>
/// <param name="txt"></param>

		public static void Deserialize(string txt)
		{
			if (txt == null || txt == "") return;

			try
			{
				XmlMemoryStreamTextReader mstr = new XmlMemoryStreamTextReader(txt);
				XmlTextReader tr = mstr.Reader;
				Deserialize(tr);
				mstr.Close();
			}
			catch (Exception ex)
			{
				DebugLog.Message(ex.Message);
				return;
			}

			return;
		}

		public static void Deserialize(XmlTextReader tr)
		{

			TableList = new List<RootTable>();
			TableDictionary = new Dictionary<string, RootTable>();
			PrefixDictionary = new Dictionary<string, RootTable>();
			LabelDictionary = new Dictionary<string, RootTable>();

			if (tr.ReadState == ReadState.Initial) //  read first element if not done yet
			{
				tr.Read();
				tr.MoveToContent();
			}

			if (Lex.Eq(tr.Name, "RootTables")) // multiple metatables
			{
				tr.Read(); // move to first MetaTable
				tr.MoveToContent();
			}

			while (true) // loop on tables
			{
				if (tr.NodeType == XmlNodeType.Element &&
					Lex.Eq(tr.Name, "RootTable"))
				{
					RootTable i = new RootTable();

					XmlUtil.GetStringAttribute(tr, "Label", ref i.Label);
					XmlUtil.GetIntAttribute(tr, "DbId", ref i.DatabaseId);
					//if (Lex.Eq(i.Label, "lida")) i = i; // debug
					XmlUtil.GetStringAttribute(tr, "MetaTable", ref i.MetaTableName);

					XmlUtil.GetBoolAttribute(tr, "IsStructureTable", ref i.IsStructureTable);
					if (!i.IsStructureTable) // if not a structure table can't do any structure searches on the table
						i.CartridgeSearchable = false;
					XmlUtil.GetBoolAttribute(tr, "CartridgeSearchable", ref i.CartridgeSearchable);
					XmlUtil.GetBoolAttribute(tr, "EcfpSearchable", ref i.EcfpSearchable);
					XmlUtil.GetStringAttribute(tr, "SmallWorldDbName", ref i.SmallWorldDbName);
					XmlUtil.GetStringAttribute(tr, "CidUrl", ref i.CidUrl);

					XmlUtil.GetStringAttribute(tr, "Prefix", ref i.Prefix);
					XmlUtil.GetBoolAttribute(tr, "PrefixIsStored", ref i.PrefixIsStored);
					XmlUtil.GetBoolAttribute(tr, "IsStringType", ref i.IsStringType);
					XmlUtil.GetBoolAttribute(tr, "NegativeNumberAllowed", ref i.NegativeNumberAllowed);
					XmlUtil.GetStringAttribute(tr, "InternalFormat", ref i.InternalNumberFormat);
					XmlUtil.GetStringAttribute(tr, "ExternalFormat", ref i.ExternalNumberFormat);

					if (i.IsStructureTable && !i.HasMultiplePrefixes)
					{ // include prefixes for structure tables only
						if (i.Prefix == "") i.Prefix = "none"; // non-blank value for lookup
						PrefixDictionary[i.Prefix.ToLower()] = i;
					}

					TableList.Add(i);
					LabelDictionary[i.Label.ToLower()] = i;
					TableDictionary[i.MetaTableName.ToLower()] = i;

					//SetSimSearchability(); 

					if (tr.IsEmptyElement) // is this an empty element?
					{
						tr.Read(); // move to next element
						tr.MoveToContent();
						continue;
					}

					tr.Read(); // move to next element
					tr.MoveToContent();

					if (Lex.Eq(tr.Name, "RootTable") &&	tr.NodeType == XmlNodeType.EndElement) { }
					else throw new Exception("Expected RootTable EndElement");

					tr.Read(); // move to next element
					tr.MoveToContent();
				}

				else
				{
					if (Lex.Eq(tr.Name, "RootTables") &&	tr.NodeType == XmlNodeType.EndElement) { }
					else throw new Exception("Expected RootTables EndElement");
					return;
				}
			}
		}

		static bool GetKeyValuePair(
			Lex lex,
			out string key,
			out string value)
		{
			key = lex.Get();
			value = null;
			if (String.IsNullOrEmpty(key)) return false;
			lex.GetExpected("=");
			value = lex.Get();
			
			key = Lex.RemoveAllQuotes(key).Trim();
			value = Lex.RemoveAllQuotes(value).Trim();
			return true;
		}

/// <summary>
/// Get dictionary associated with a set of root tables
/// </summary>
/// <param name="dictName"></param>
/// 
		public static DictionaryMx GetDictionary(string dictName)
		{
			if (TableList == null) Build();

			bool allRoots = Lex.Eq(dictName, "Root_Tables");
			bool structs = Lex.Eq(dictName, "StructureDatabases");
			bool cartridge = Lex.Eq(dictName, "CartridgeDatabases");
			bool smallWorld = Lex.Eq(dictName, "SmallWorldDatabases");

			DictionaryMx dex = new DictionaryMx();
			dex.Name = dictName;

			foreach (RootTable rt in TableList)
			{
				bool add = false;
				string word = rt.Label;
				string def = rt.MetaTableName;

				if (Lex.Eq(rt.Label, "SmallWorld")) continue; // ignore this

				if (structs)
					add = rt.IsStructureTable;

				else if (cartridge)
					add = rt.CartridgeSearchable;

				else if (smallWorld)
					add = Lex.IsDefined(rt.SmallWorldDbName); // exclude the "SmallWorld" umbrella DB

				else add = true; // if dict not one of the definede subsets then include all 

				if (add)
					dex.Add(rt.Label, rt.MetaTableName);
			}

			return dex;
		}

		/// <summary>
		///  Return true if dict name is the name of the root table dictionary
		/// </summary>
		/// <param name="dictName"></param>
		/// <returns></returns>

		public static bool IsRootTableDictionaryName(string dictName)
		{
			if (Lex.Eq(dictName, "Root_Tables") ||
				Lex.Eq(dictName, "Structure_Databases")) return true;
			// || Lex.Eq(dictName, "SmallWorldDatabases")) return true;
			else return false;
		}

		/// <summary>
		/// Return true if root table is structure based
		/// </summary>
		/// <param name="dictName"></param>
		/// <returns></returns>

		public static bool IsStructureDatabaseRootTableDictionaryName(string dictName)
		{
			if (Lex.Eq(dictName, "Structure_Databases")) return true;
			//|| Lex.Eq(dictName, "SmallWorldDatabases")) return true;
			else return false;
		}

		/// <summary>
		/// Return true if dict name is the full list or a sublist of database names
		/// </summary>
		/// <param name="dictName"></param>
		/// <returns></returns>

		public static bool IsDatabaseListDictionaryName(string dictName)
		{
			if (Lex.Eq(dictName, "Root_Tables") ||
				Lex.Eq(dictName, "Structure_Databases") ||
				Lex.Eq(dictName, "CartridgeDatabases") ||
				Lex.Eq(dictName, "SmallWorldDatabases")) return true;

			else return false;
		}

		/// <summary>
		/// Return true if the specified table is the root of a structure database
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static bool IsStructureDatabaseRootTable(MetaTable mt)
		{
			return IsStructureDatabaseRootTable(mt.Name);
		}

		/// <summary>
		/// Return true if the specified table is the root of a structure database
		/// </summary>
		/// <param name="mtName"></param>
		/// <returns></returns>

		public static bool IsStructureDatabaseRootTable(string tableName)
		{
			if (TableList == null) Build();

			tableName = tableName.Trim().ToLower();
			if (TableDictionary.ContainsKey(tableName))
			{
				RootTable root = TableDictionary[tableName];
				return root.IsStructureTable;
			}

			else if (Mobius.Data.MetaTable.IsUserDatabaseStructureTableName(tableName)) return true;

			else return false;
		}


	}
}
