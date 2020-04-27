using Mobius.ComOps;

using System;
using System.Text;
using System.Xml;
using System.IO;
using System.Data;
using System.Collections;
using System.Collections.Generic;

namespace Mobius.Data
{

	/// <summary>
	/// Basic class for metatable. 
	/// A metatable is the basic data representation in Mobius
	/// and may correspond to an Oracle table or view
	/// or a view of data provided by a metabroker.
	/// </summary>

	public class MetaTable
	{
		public int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		public MetaBrokerType MetaBrokerType; // metabroker type
		public MetaTable Parent; // parent metatable for this table (null if this is a root table)
		public bool SingleRowPerKeyValue = false; // true if this table has at most one row per root-table key value
		public string TableMap = ""; // source Oracle table, view or select statement
		public string WarehouseMap = ""; // if table exists in warehouse this is the sql for the warehouse
		public bool AllowNetezzaUse { get { return !Lex.IsNullOrEmpty(WarehouseMap); }} // true if the data for this table exists in warehouse

		public string Name	{ // internal metatable name (property allows for setting breakpoints
			get { return _name; }
			set { _name = value; /* if (Lex.Eq(value, "star_41481")) _name = _name; */ }
		}
		private string _name = "";

		public string Label = ""; // table label
		public string ShortLabel = ""; // short label for table
		public bool AllowColumnMerging = true;
		public string Description = ""; // description of table or link to it (use GetDescription to get the full description)
		public string RowCountSql = ""; // sql for selecting row count if generic broker
		public bool SummarizedExists = false; // if true summarized version is available
		public bool UseSummarizedData = false; // if true query summarized data
		public bool UseUnsummarizedData { get { return !UseSummarizedData; } }
		public SubstanceSummarizationLevel SubstanceSummarizationType = SubstanceSummarizationLevel.None; // type of data summarization (not currently used)
		public string Code = ""; // simple code for table/assay used for pivoting

		public string TargetSymbol; // gene symbol for the gene associated with the target
		public int TargetId = NullValue.NullNumber; // Entrez gene id for target
		public string GeneFamily = ""; // Kinase, GPCR, Ion channel, NHR, Phosphatase, Protease...
		public string AssayType = ""; // BINDING, FUNCTIONAL, UNKNOWN
		public string AssayMode = ""; // AG, ANTAG, INH, POT...
		public string AssayLocation; // where the assay is normally run

		public UserDataImportParms ImportParms; // default import parameters for table 
		public string UpdateDateTimeSql = ""; // sql for selecting update date if generic broker

		// Pivoting information for unpivoted sources(optional)

		public List<string> TableFilterColumns = null; // columns in source table to filter on for this metatable
		public List<string> TableFilterValues = null; // values in PivotFilterColumn that identifies this table for pivoting
		public List<string> PivotMergeColumns = null; // columns in source table to use to merge sets of unpivoted rows
		public List<string> PivotColumns = null; // columns in source table to pivot on, values are stored in associated metacolumns
		public bool MultiPivot = false; // if true pivot this table into multiple tables based on hit list & data source

		// Qualified number column set for table (optional)

		public string QnQualifier = ""; // qualified number qualifier column in source table
		public string QnNumberValue = ""; // basic numeric value col
		public string QnNumberValueHigh = ""; // high numeric value for range col
		public string QnNValue = ""; // number of results going into stats col
		public string QnNValueTested = ""; // with number of results tested col
		public string QnStandardDeviation = ""; // QN SD col
		public string QnStandardError = ""; // QN SE col
		public string QnTextValue = ""; // text value col
		public string QnLinkValue = ""; // link value for col

		public List<MetaColumn> MetaColumns = new List<MetaColumn>(); // ordered array of MetaColumn refs for table

		// Static and constant values relating to MetaTables

		public static MetaTable KeyMetaTable; // default key metatable
		public const int MaxColumns = 256; // maximum number of allowed columns
		public const string SummarySuffix = "_SUMMARY"; // suffix for tables that are summary tables

		public static string PrimaryRootTable = "<PrimaryRootTable>";
		public static string PrimaryCompoundLibraryTable = "<PrimaryCompoundLibraryTable>";

		public static string PrimaryKeyColumnName = "COMPOUND_ID";
		public static string PrimaryKeyColumnLabel = "Compound Id";
		
		public static string AllDataQueryTable = "ALL_DATA_QUERY_TABLE"; // table used to define QuickSearch all-data queries

		public static string SmallWorldMetaTableName = "SmallWorld";

		public static string[] NameSuffixes = { " Summary", " (Assay)", " (Note)" }; // possible suffixes added to metatable names

		public const int MaxIdentifierLength = 30; // maximum identifier length for name (i.e. max oracle identifier length)

		/// <summary>
		/// Basic constructor
		/// </summary>

		public MetaTable()
		{
			return;
		}

		/// <summary>
		/// Parse table name for numeric table id and summary table suffix 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="tableId"></param>
		/// <param name="summarized"></param>

		public static void ParseMetaTableName(
				string name,
				out int tableId,
				out bool summary)
		{
			List<int> tableIds;
			ParseMetaTableName(name, out tableIds, out summary);
			if (tableIds.Count == 1) tableId = tableIds[0];
			else tableId = -1;

			return;
		}

/// <summary>
/// Parse table name for numeric table id and summary table suffix 
/// </summary>
/// <param name="name"></param>
/// <param name="tableType"></param>
/// <param name="tableId"></param>
/// <param name="summary"></param>

		public static void ParseMetaTableName(
		string name,
		out string tableType,
		out int tableId,
		out bool summary)
		{
			string tableIdString = "";

			tableType = "";
			tableId = -1;
			summary = false;
			string[] sa = name.Split('_');
			if (sa.Length >= 1) tableType = sa[0];
			if (sa.Length >= 2)
			{
				tableIdString = sa[1];
				int.TryParse(tableIdString, out tableId);
			}

			if (Lex.EndsWith(name, MetaTable.SummarySuffix))
				summary = true;

			return;
		}

		/// <summary>
		/// Parse table name for numeric table id and summary table suffix 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="tableId"></param>
		/// <param name="summarized"></param>

		public static void ParseMetaTableName(
				string name,
				out string tableType,
				out string tableIdString,
				out bool summary)
		{
			tableType = tableIdString = "";
			summary = false;
			string[] sa = name.Split('_');
			if (sa.Length >= 1) tableType = sa[0];
			if (sa.Length >= 2) tableIdString = sa[1];

			if (Lex.EndsWith(name, MetaTable.SummarySuffix))
				summary = true;

			return;
		}

		/// <summary>
		/// Parse metatable name with possible multiple table ids
		/// </summary>
		/// <param name="name"></param>
		/// <param name="tableId"></param>
		/// <param name="summary"></param>

		public static void ParseMetaTableName(
			string name,
			out List<int> tableIds,
			out bool summary)
		{
			int i1, i2;

			tableIds = new List<int>();
			summary = false;

			string[] sa = name.Split('_');

			for (i1 = 1; i1 < sa.Length; i1++)
			{
				if (int.TryParse(sa[i1], out i2))
					tableIds.Add(i2);
			}

			if (Lex.EndsWith(name, MetaTable.SummarySuffix))
				summary = true;

			return;
		}


/// <summary>
/// RemoveSuffixesFromName
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public static string RemoveSuffixesFromName(string name)
		{
			foreach (string sfx in NameSuffixes)
			{
				if (Lex.EndsWith(name, sfx)) // end of name
				{
					name = name.Substring(0, name.Length - sfx.Length);
					name = name.Trim();
				}
				else if (Lex.Contains(name, sfx + ".")) // end of table name part of name
				{
					name = Lex.Replace(name, sfx + ".", ".");
				}

			}

			return name;
		}

		/// <summary>
		/// Get a metacolumn name that is valid & unique for the current table
		/// </summary>
		/// <param name="mcName">Preferred name</param>
		/// <returns></returns>

		public string GetValidMetaColumnName(
			string mcName)
		{
			string suffix;

			string mcName2 = GetValidMetaColumnName(mcName, out suffix);
			return mcName2;
		}

		/// <summary>
		/// Get a metacolumn name that is valid & unique for the current table
		/// </summary>
		/// <param name="mcName">Preferred name</param>
		/// <param name="suffix">Any suffix that was added to the name to make it unique</param>
		/// <returns></returns>
		
		public string GetValidMetaColumnName(
			string mcName,
			out string suffix)
		{
			suffix = "";
			mcName = mcName.ToUpper();
			if (!String.IsNullOrEmpty(mcName) && // see if ok as is
				mcName.Length <= MetaTable.MaxIdentifierLength &&
					GetMetaColumnByName(mcName) == null) return mcName;

			if (String.IsNullOrEmpty(mcName)) mcName = "Column";

			else if (mcName.Length > MetaTable.MaxIdentifierLength)
				mcName = mcName.Substring(0, MetaTable.MaxIdentifierLength - 3);

			// Generate a unique name of acceptable length

			int ci = 1; // suffix to try
			if (GetMetaColumnByName(mcName) != null) ci = 2;
			while (true)
			{
				string mcName2 = mcName + "_" + ci.ToString();
				if (GetMetaColumnByName(mcName2) == null)
				{
					suffix = ci.ToString();
					return mcName2;
				}
				ci++;
			}
		}

/// <summary>
/// Get a unique MetaColumn label 
/// </summary>
/// <param name="mcLabel"></param>
/// <returns></returns>

		public string GetUniqueMetaColumnLabel(
			string mcLabel,
			int startingSuffixIndex)
		{
			if (GetMetaColumnByLabel(mcLabel) == null) return mcLabel; // already unique

			int ci = startingSuffixIndex;
			while (true)
			{
				string mcLabel2 = mcLabel + " " + ci.ToString();
				if (GetMetaColumnByLabel(mcLabel2) == null)
					return mcLabel2;
				ci++;
			}
		}

		/// <summary>
		/// Add a metacolumn
		/// </summary>
		/// <param name="column"></param>

		public MetaColumn AddMetaColumn(
			MetaColumn mc)
		{
			mc.MetaTable = this;
			MetaColumns.Add(mc);
			return mc;
		}

		public MetaColumn AddMetaColumn(
			string name,
			string label,
			MetaColumnType dataType)
		{
			return AddMetaColumn(name, label, dataType, ColumnSelectionEnum.Selected,
				MetaColumn.GetDefaultDisplayWidth(dataType), ColumnFormatEnum.Default, 0);
		}

		/// <summary>
		/// Abbreviated method for adding a metacolumn with a common set of attributes to a metatable
		/// </summary>
		/// <param name="name"></param>
		/// <param name="label"></param>
		/// <param name="dataType"></param>
		/// <param name="displayLevel"></param>
		/// <param name="displayWidth"></param>
		/// <returns></returns>

		public MetaColumn AddMetaColumn(
			string name,
			string label,
			MetaColumnType dataType,
			ColumnSelectionEnum displayLevel,
			float displayWidth)
		{
			return AddMetaColumn(name, label, dataType, displayLevel, displayWidth,
				ColumnFormatEnum.Default, 0);
		}

		/// <summary>
		/// Abbreviated method for adding a metacolumn with a common set of attributes to a metatable
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="name"></param>
		/// <param name="label"></param>
		/// <param name="dataType"></param>
		/// <param name="displayLevel"></param>
		/// <param name="displayWidth"></param>
		/// <param name="decimals"></param>
		/// <returns></returns>

		public MetaColumn AddMetaColumn(
			string name,
			string label,
			MetaColumnType dataType,
			ColumnSelectionEnum displayLevel,
			float displayWidth,
			ColumnFormatEnum displayFormat,
			int decimals)
		{
			return AddMetaColumn(name, label, dataType, displayLevel, displayWidth, displayFormat, decimals, "", "");
		}

/// <summary>
/// Abbreviated method for adding a metacolumn with a common set of attributes to a metatable
/// </summary>
/// <param name="name"></param>
/// <param name="label"></param>
/// <param name="dataType"></param>
/// <param name="displayLevel"></param>
/// <param name="displayWidth"></param>
/// <param name="tableMap"></param>
/// <param name="columnMap"></param>
/// <returns></returns>

		public MetaColumn AddMetaColumn(
			string name,
			string label,
			MetaColumnType dataType,
			ColumnSelectionEnum displayLevel,
			float displayWidth,
			string tableMap,
			string columnMap)
		{
			return AddMetaColumn(name, label, dataType, displayLevel, displayWidth, ColumnFormatEnum.Default, 0, tableMap, columnMap);
		}

/// <summary>
/// Abbreviated method for adding a metacolumn with a common set of attributes to a metatable
/// </summary>
/// <param name="name"></param>
/// <param name="label"></param>
/// <param name="dataType"></param>
/// <param name="displayLevel"></param>
/// <param name="displayWidth"></param>
/// <param name="displayFormat"></param>
/// <param name="decimals"></param>
/// <param name="tableMap"></param>
/// <param name="columnMap"></param>
/// <returns></returns>

		public MetaColumn AddMetaColumn(
			string name,
			string label,
			MetaColumnType dataType,
			ColumnSelectionEnum displayLevel,
			float displayWidth,
			ColumnFormatEnum displayFormat,
			int decimals,
			string tableMap,
			string columnMap)
		{
			MetaColumn mc = new MetaColumn();
			string mcName = name.Trim().ToUpper();
			mc.Name = GetValidMetaColumnName(mcName);
			mc.ColumnMap = mc.Name;
			mc.Label = label;
			mc.DataType = dataType;
			mc.InitialSelection = displayLevel;
			mc.Width = displayWidth;
			mc.Format = displayFormat;
			mc.Decimals = decimals;
			mc.TableMap = tableMap;
			mc.ColumnMap = columnMap;

			mc.MetaTable = this;
			this.AddMetaColumn(mc);
			return mc;
		}

		/// <summary>
		/// Lookup a metacolumn by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public MetaColumn GetMetaColumnByName(
			string name)
		{
			for (int i1 = 0; i1 < MetaColumns.Count; i1++)
			{
				MetaColumn mc = MetaColumns[i1];
				if (mc == null || Lex.IsUndefined(mc.Name)) continue;
				if (String.Compare(mc.Name, name, true) == 0)
					return mc;
			}

			return null;
		}

		/// <summary>
		/// Lookup a metacolumn by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public MetaColumn GetMetaColumnByNameWithException(
			string name)
		{
			MetaColumn mc = GetMetaColumnByName(name);
			if (mc != null) return mc;
			else throw new Exception(
				"Unable to find column: " + name + " in data table " + Label + "(" + Name + ")\r\n\r\n" +
				"The column may have been removed or renamed.");
		}

		/// <summary>
		/// Lookup a metacolumn by name and return it's index
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public int GetMetaColumnIndexByName(
			string name)
		{
			try
			{
				int mci = GetMetaColumnIndexByNameWithException(name);
				return mci;
			}

			catch (Exception ex) { return -1; }
		}

		/// <summary>
		/// Lookup a metacolumn by name and return it's index
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public int GetMetaColumnIndexByNameWithException(
			string name)
		{
			for (int i1 = 0; i1 < MetaColumns.Count; i1++)
			{
				if (String.Compare((MetaColumns[i1]).Name, name, true) == 0)
					return i1;
			}

			throw new Exception("Metacolumn not found: " + name); // no luck
		}

		/// <summary>
		/// Lookup a metacolumn by label
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public MetaColumn GetMetaColumnByLabel(
			string label)
		{
			for (int i1 = 0; i1 < MetaColumns.Count; i1++)
			{
				if (String.Equals(MetaColumns[i1].Label, label, StringComparison.OrdinalIgnoreCase))
					return MetaColumns[i1];
			}

			return null;
		}

		/// <summary>
		/// Lookup a metacolumn the summarization result code
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public MetaColumn GetMetaColumnBySummaryResultCode(
			string code)
		{
			for (int i1 = 0; i1 < MetaColumns.Count; i1++)
			{
				if (String.Compare((MetaColumns[i1]).ResultCode2, code, true) == 0)
					return MetaColumns[i1];
			}

			return null;
		}

		/// <summary>
		/// Return the key metacolumn for this metatable
		/// </summary>

		public MetaColumn KeyMetaColumn
		{
			get
			{
				MetaColumn mc;

				if (_keyMetaColumn != null) return _keyMetaColumn;

				if (MetaColumns.Count == 0) return null;

				for (int ci = 0; ci < MetaColumns.Count; ci++)
				{
					mc = MetaColumns[ci];
					if (mc.KeyPosition >= 0)
					{
						_keyMetaColumn = mc;
						return mc;
					}
				}

				mc = MetaColumns[0]; // assume first column is key & set it as such
				_keyMetaColumn = mc;
				mc.KeyPosition = 0; 
				return mc;
			}
		}

		private MetaColumn _keyMetaColumn = null; // set first time retrieved via KeyMetaColumn for faster subsequent retrieval

/// <summary>
/// Return true if key is an integer value
/// </summary>
/// <returns></returns>

		public bool IsIntegerKey()
		{
			bool integerKey = false;

			MetaColumn mc = KeyMetaColumn;
			if (mc != null &&
			 (mc.DataType == MetaColumnType.Integer ||
				mc.StorageType == MetaColumnStorageType.Integer))
				integerKey = true;

			return integerKey;
		}

		/// <summary>
		/// Return any primary result column
		/// </summary>

		public MetaColumn PrimaryResult
		{
			get
			{
				MetaColumn mc;
				for (int ci = 0; ci < MetaColumns.Count; ci++)
				{
					mc = MetaColumns[ci];
					if (mc.PrimaryResult) return mc;
				}

				return null;
			}
		}

		/// <summary>
		/// Return any secondary result column
		/// </summary>

		public MetaColumn SecondaryResult
		{
			get
			{
				MetaColumn mc;
				for (int ci = 0; ci < MetaColumns.Count; ci++)
				{
					mc = MetaColumns[ci];
					if (mc.SecondaryResult) return mc;
				}

				return null;
			}
		}

		/// <summary>
		/// Get column that contains database set information, if any
		/// </summary>
		/// <returns></returns>

		public MetaColumn DatabaseListMetaColumn
		{
			get 
			{
				for (int ci = 0; ci < this.MetaColumns.Count; ci++)
				{
					MetaColumn mc = MetaColumns[ci];
					if (!mc.IsDatabaseSetColumn) continue;
					if (mc.Dictionary == "") continue;
					return mc;
				}
				return null;
			}
		}

/// <summary>
/// Get column that contains similarity score value, if any
/// </summary>
		public MetaColumn SimilarityScoreMetaColumn
		{
			get
			{
				for (int ci = 0; ci < this.MetaColumns.Count; ci++)
				{
					MetaColumn mc = MetaColumns[ci];
					if (Lex.Eq(mc.Name, "MolSimilarity") ||
 					 Lex.Eq(mc.Name, "SimScore"))
						return mc;
				}
				return null;
			}
		}

		/// <summary>
		/// Return true if specified table is the default root table
		/// </summary>

		public bool IsDefaultRootTable
		{
			get
			{
				if (!Lex.StartsWith(Name, MetaTable.PrimaryRootTable)) return false;

				return true;
			}
		}
	
		/// <summary>
		/// Return true if specified table is a root table
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public bool IsRootTable
		{
			get
			{
				if (this.Root == this) return true;
				else return false;
			}
		}

		/// <summary>
		/// Get root for table
		/// </summary>
		/// <returns></returns>

		public MetaTable Root
		{
			get
			{
				if (this.Parent == null) return this; // if no parent then we must be the root
				else return this.Parent; // other wise our parent is the root
			}
		}

		/// <summary>
		/// Return true if metatable based on a user object
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public bool IsUserObjectTable
		{
			get
			{
				if (Lex.IsNullOrEmpty(Name)) return false;

				else return (UserObject.IsUserObjectTableName(Name));
			}
		}


		/// <summary>
		/// Return true if metatable is an annotation table
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public bool IsAnnotationTable
		{
			get
			{
				if (Lex.IsUndefined(Name)) return false;

				else return (Lex.StartsWith(Name, "ANNOTATION_"));
			}
		}

		/// <summary>
		/// Return true if metatable is a user database structure table
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public bool IsUserDatabaseStructureTable
		{
			get
			{
				if (Lex.IsUndefined(Name)) return false;

				else return (Lex.StartsWith(Name, "USERDATABASE_STRUCTURE_"));
			}
		}

/// <summary>
/// Return true if table contains a cartridge searchable structure column
/// </summary>

		public bool IsCartridgeSearchable
		{
			get
			{
				if (Lex.IsUndefined(Name)) return false;

				MetaColumn smc = FirstStructureMetaColumn;
				if (smc == null) return false;
				if (!smc.IsSearchable) return false;

				if (IsUserDatabaseStructureTable) return true;
				if (MetaBrokerType == MetaBrokerType.Annotation) return true; // structure in annotation table

				RootTable rt = RootTable.GetFromTableName(Name);
				if (rt != null && Lex.Eq(rt.MetaTableName, Name) && rt.CartridgeSearchable)
					return true;

				else return false;
			}
		}

		/// <summary>
		/// Return true if table is SmallWorld searchable
		/// </summary>

		public bool IsSmallWorldSearchable
		{
			get
			{
				if (Lex.IsUndefined(Name)) return false;

				MetaColumn smc = FirstStructureMetaColumn;
				if (smc == null) return false;
				if (!smc.IsSearchable) return false;

				if (Lex.Eq(Name, SmallWorldMetaTableName)) return true;
				
				RootTable rt = RootTable.GetFromTableName(Name);
				if (rt != null && Lex.Eq(rt.MetaTableName, Name) && Lex.IsDefined(rt.SmallWorldDbName))
					return true;

				else return false;
			}
		}

/// <summary>
/// Return true if table is a calculated field
/// </summary>

		public bool IsCalculatedField
		{
			get
			{
				if (Lex.IsUndefined(Name)) return false;

				else return (Lex.StartsWith(Name, "CALCFIELD_"));
			}
		}

		/// <summary>
		/// Return true if table is a SpotfireLink metatable
		/// </summary>

		public bool IsSpotfireLink
		{
			get
			{
				if (Lex.IsUndefined(Name)) return false;

				else return (Lex.StartsWith(Name, "SPOTFIRELINK_"));
			}
		}

/// <summary>
/// Return true if table is an assay table
/// </summary>

		public bool IsAssayTable
		{
			get
			{
				if (Lex.IsUndefined(Name)) return false;

				else return (Name.StartsWith("ASSAY_") || Name.Contains("_ASSAY_"));

			}
		}

		/// <summary>
		/// Return true if metatable name refers to a user database structure table
		/// </summary>
		/// <param name="mtName"></param>
		/// <returns></returns>

		public static bool IsUserDatabaseStructureTableName (string mtName)
		{
			return (Lex.StartsWith(mtName, "USERDATABASE_STRUCTURE_"));
		}

		/// <summary>
		/// Possibly reassign this table based on the current database 
		/// </summary>
		/// <param name="root">Current root table in effect</param>
		/// <returns></returns>

		public MetaTable MapToCurrentDb(
			MetaTable root)
		{
			int di;

			if (root == null) return this;

			MetaTable mt = this;
			if (mt.Root != mt) // is this table it's own root
			{ // non-root table
				if (mt.Parent != null && Lex.Eq(mt.Parent.Name, root.Name))
					return this; // ok as is
				else return null; // doesn't map to this root
			}

			return root; // root table, map to current root
		}

		/// <summary>
		/// Return the database dictionary for a metatable
		/// </summary>
		/// <returns></returns>
		public DictionaryMx GetDbDictionary()
		{
			foreach (MetaColumn mc in this.MetaColumns)
			{
				if (mc.IsDatabaseSetColumn)
					return DictionaryMx.Get(mc.Dictionary);
			}
			return null;
		}

		/// <summary>
		/// Return true if a description of the specified table is available
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public bool DescriptionIsAvailable()
		{
			if (Description != null && Description.Trim().Length > 0)
				return true;
			else return false;
		}

		/// <summary>
		/// HasSearchableColumns
		/// </summary>
		public bool HasSearchableColumns
		{
			get
			{
				foreach (MetaColumn mc in MetaColumns)
				{
					if (mc.IsSearchable) return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Return true if data retrieved and displayed by Mobius rather than outside service (e.g. spotfire WebPlayer)
		/// </summary>

		public bool RetrievesMobiusData
		{
			get
			{
				// Todo: Generalize by storing as a field in the MetaTable or by calling through ServiceFacade
				if (MetaBrokerType == MetaBrokerType.SpotfireLink)
					return false;

				else return true;
			}
		}

		/// <summary>
		/// Return true if data retrieved by QueryEngine
		/// </summary>

		public bool RetrievesDataFromQueryEngine
		{
			get
			{
				// Todo: Generalize by storing as a field in the MetaTable or by calling through ServiceFacade
				if (
					 MetaBrokerType == MetaBrokerType.SpotfireLink)
					return false;

				else return true;
			}
		}

		/// <summary>
		/// Return true if criteria from this table are not used for searching but
		/// are used for parameters & should be ignored.
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public bool IgnoreCriteria
		{
			get
			{
				bool enabled = true;
				if (!enabled) return false;

				// Todo: Generalize by storing as a field in the MetaTable or by calling through ServiceFacade
				if (
				 MetaBrokerType == MetaBrokerType.RgroupDecomp ||
				 MetaBrokerType == MetaBrokerType.SpotfireLink)
					return true;

				else return false;
			}
		}

		/// <summary>
		/// Return true if a structure is associated with this metatable or it's parent
		/// </summary>
		/// <returns></returns>

		public bool HasAssociatedStructure()
		{
			MetaTable mt;
			if (Parent != null) mt = Parent; // check parent for structure
			else mt = this; // check us for structure

			foreach (MetaColumn mc in mt.MetaColumns)
			{
				if (mc.DataType == MetaColumnType.Structure) return true;
			}

			return false;
		}

		/// <summary>
		/// Get first structure column in metatable
		/// </summary>
		/// <returns></returns>

		public MetaColumn FirstStructureMetaColumn
		{
			get
			{
				MetaTable mt = this;

				foreach (MetaColumn mc in mt.MetaColumns)
				{
					if (mc.DataType == MetaColumnType.Structure) return mc;
				}

				return null;
			}
		}

		/// <summary>
		/// Return the name of the associated summarized table
		/// </summary>

		public MetaTable AssociatedSummarizedMetaTable
		{
			get
			{
				MetaTable mt = MetaTableCollection.Get(Name + SummarySuffix);
				if (mt != null) return mt;
				else throw new Exception("Associated summarized table does not exist for: " + Name);
			}
		}

		/// <summary>
		/// Return true if an unsummarized version of the table exists
		/// </summary>

		public bool UnsummarizedExists
		{
			get
			{
				if (Lex.EndsWith(Name, SummarySuffix)) return true;
				else return false;
			}
		}

		/// <summary>
		/// Return the name of the associated unsummarized table
		/// </summary>

		public MetaTable AssociatedUnsummarizedMetaTable
		{
			get
			{
				if (!Lex.EndsWith(Name, SummarySuffix))
					throw new Exception("Associated unsummarized table does not exist for: " + Name);

				string mtName = Name.Substring(0, Name.Length - SummarySuffix.Length);
				MetaTable mt = MetaTableCollection.Get(mtName);
				if (mt != null) return mt;
				else throw new Exception("Associated unsummarized table does not exist for: " + Name);
			}
		}

		/// <summary>
		/// Serialize MetaTable
		/// </summary>
		/// <returns></returns>

		public string Serialize()
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
		/// Serialize MetaTable to XmlTextWriter
		/// </summary>
		/// <param name="tw"></param>

		public void Serialize(
		XmlTextWriter tw)
		{
			tw.WriteStartElement("MetaTable");

			MetaTable mt = this;
			if (mt.Id != 0)
				tw.WriteAttributeString("Id", mt.Id.ToString());
			tw.WriteAttributeString("Name", mt.Name);
			tw.WriteAttributeString("Label", mt.Label);
			tw.WriteAttributeString("MetaBroker", mt.MetaBrokerType.ToString());
			if (mt.Code != "")
				tw.WriteAttributeString("Code", mt.Code);
			if (mt.ShortLabel != "")
				tw.WriteAttributeString("ShortLabel", mt.ShortLabel);
			if (mt.AllowColumnMerging != true)
				tw.WriteAttributeString("AllowColumnMerging", mt.AllowColumnMerging.ToString());
			if (mt.Parent != null)
				tw.WriteAttributeString("Parent", mt.Parent.Name);
			if (mt.SingleRowPerKeyValue)
				tw.WriteAttributeString("SingleRowPerKeyValue", mt.SingleRowPerKeyValue.ToString());
			if (mt.TableMap != "")
				tw.WriteAttributeString("TableMap", mt.TableMap);

			if (mt.WarehouseMap != "")
				tw.WriteAttributeString("WarehouseMap", mt.WarehouseMap);
			if (!String.IsNullOrEmpty(mt.Description))
				tw.WriteAttributeString("Description", mt.Description);

			SerializeList(tw, "TableFilterColumns", mt.TableFilterColumns);
			SerializeList(tw, "TableFilterValues", mt.TableFilterValues);
			SerializeList(tw, "PivotMergeColumns", mt.PivotMergeColumns);
			SerializeList(tw, "PivotColumns", mt.PivotColumns);

			if (mt.MultiPivot != false)
				tw.WriteAttributeString("RemapForRetrieval", mt.MultiPivot.ToString());
			if (mt.SummarizedExists != false)
				tw.WriteAttributeString("SummarizedExists", mt.SummarizedExists.ToString());
			if (mt.UseSummarizedData != false)
				tw.WriteAttributeString("UseSummarizedData", mt.UseSummarizedData.ToString());
			if (mt.SubstanceSummarizationType != SubstanceSummarizationLevel.Unknown && mt.SubstanceSummarizationType != SubstanceSummarizationLevel.None)
				tw.WriteAttributeString("SubstanceSummarizationType", mt.SubstanceSummarizationType.ToString());

			if (!String.IsNullOrEmpty(mt.TargetSymbol))
				tw.WriteAttributeString("TargetSymbol", mt.TargetSymbol);

			if (mt.TargetId > 0)
				tw.WriteAttributeString("TargetId", mt.TargetId.ToString());

			if (!String.IsNullOrEmpty(mt.GeneFamily))
				tw.WriteAttributeString("GeneFamily", mt.GeneFamily);

			if (!String.IsNullOrEmpty(mt.AssayType))
				tw.WriteAttributeString("AssayType", mt.AssayType);

			if (!String.IsNullOrEmpty(mt.AssayMode))
				tw.WriteAttributeString("AssayMode", mt.AssayMode);

			if (!String.IsNullOrEmpty(mt.AssayLocation))
				tw.WriteAttributeString("AssayLocation", mt.AssayLocation);

			if (mt.RowCountSql != "")
				tw.WriteAttributeString("RowCountSql", mt.RowCountSql);
			if (mt.UpdateDateTimeSql != "")
				tw.WriteAttributeString("UpdateDateTimeSql", mt.UpdateDateTimeSql);

			if (mt.ImportParms != null) // save any file import parms
			{
				tw.WriteAttributeString("ImportParms.ClientFile", mt.ImportParms.FileName);
				tw.WriteAttributeString("ImportParms.ClientFileModified", DateTimeUS.ToDateString(mt.ImportParms.ClientFileModified));
				tw.WriteAttributeString("ImportParms.ServerFile", mt.ImportParms.FileName); // keep old name
				tw.WriteAttributeString("ImportParms.Delim", mt.ImportParms.Delim.ToString());
				tw.WriteAttributeString("ImportParms.MultDelimsAsSingle", mt.ImportParms.MultDelimsAsSingle.ToString());
				tw.WriteAttributeString("ImportParms.TextQualifier", mt.ImportParms.TextQualifier.ToString());
				tw.WriteAttributeString("ImportParms.FirstLineHeaders", mt.ImportParms.FirstLineHeaders.ToString());
				tw.WriteAttributeString("ImportParms.DeleteExisting", mt.ImportParms.DeleteExisting.ToString());
				tw.WriteAttributeString("ImportParms.DeleteDataOnly", mt.ImportParms.DeleteDataOnly.ToString());
				tw.WriteAttributeString("ImportParms.ImportInBackground", mt.ImportParms.ImportInBackground.ToString());
				tw.WriteAttributeString("ImportParms.CheckForFileUpdates", mt.ImportParms.CheckForFileUpdates.ToString());
			}

			// Serialize the MetaColumns for MetaTable

			for (int mci = 0; mci < mt.MetaColumns.Count; mci++)
			{
				MetaColumn mc = (MetaColumn)mt.MetaColumns[mci];
				tw.WriteStartElement("MetaColumn");

				if (mc.Id != 0)
					tw.WriteAttributeString("Id", mc.Id.ToString());
				tw.WriteAttributeString("Name", mc.Name);
				tw.WriteAttributeString("Label", mc.Label);
				if (mc.ShortLabel != "")
					tw.WriteAttributeString("ShortLabel", mc.ShortLabel);
				if (mc.LabelImage != "")
					tw.WriteAttributeString("LabelImage", mc.LabelImage);

				if (mc.ResultCode != "")
					tw.WriteAttributeString("ResultCode", mc.ResultCode);

				if (mc.ResultCode2 != "")
					tw.WriteAttributeString("SummaryResultCode", mc.ResultCode2); // i.e. ResultCode2

				if (mc.Units != "")
					tw.WriteAttributeString("Units", mc.Units);
				tw.WriteAttributeString("DataType", mc.DataType.ToString());

				if (mc.DataTypeImageName != "")
					tw.WriteAttributeString("DataTypeImage", mc.DataTypeImageName);

				if (mc.StorageType != MetaColumnStorageType.Unknown)
					tw.WriteAttributeString("StorageType", mc.StorageType.ToString());

				if (mc.KeyPosition == 0)
					tw.WriteAttributeString("Key", "true");
				else if (mc.KeyPosition > 0)
					tw.WriteAttributeString("PositionInKey", mc.KeyPosition.ToString());

				if (mc.SummarizedExists) // write if not default value
					tw.WriteAttributeString("SummarizedExists", mc.SummarizedExists.ToString());
				if (!mc.UnsummarizedExists) // write if not default value
					tw.WriteAttributeString("UnsummarizedExists", mc.UnsummarizedExists.ToString());

				if (mc.SummarizationRole != SummarizationRole.Unknown) // write if not default value
					tw.WriteAttributeString("SummarizationRole", mc.SummarizationRole.ToString());

				if (mc.PrimaryResult) // write if not default value
					tw.WriteAttributeString("PrimaryResult", mc.PrimaryResult.ToString());
				if (mc.SecondaryResult) // write if not default value
					tw.WriteAttributeString("SecondaryResult", mc.SecondaryResult.ToString());

				if (mc.SinglePoint) // write if not default value
					tw.WriteAttributeString("SinglePoint", mc.SinglePoint.ToString());

				if (mc.Concentration) // write if not default value
					tw.WriteAttributeString("Concentration", mc.Concentration.ToString());

				if (mc.MultiPoint) // write if not default value
					tw.WriteAttributeString("MultiPoint", mc.MultiPoint.ToString());

				if (mc.DetailsAvailable) // write if not default value
					tw.WriteAttributeString("DetailsAvailable", mc.DetailsAvailable.ToString());

				if (mc.TextCase != ColumnTextCaseEnum.Unknown)
					tw.WriteAttributeString("Case", mc.TextCase.ToString());
				if (mc.IgnoreCase != true)
					tw.WriteAttributeString("IgnoreCase", mc.IgnoreCase.ToString());
				if (mc.Dictionary != "")
					tw.WriteAttributeString("Dictionary", mc.Dictionary);
				if (mc.DictionaryMultipleSelect != false)
					tw.WriteAttributeString("DictionaryMultipleSelect", mc.DictionaryMultipleSelect.ToString());

				if (!String.IsNullOrEmpty(mc.Description))
					tw.WriteAttributeString("Description", mc.Description);

				tw.WriteAttributeString("DisplayLevel", mc.InitialSelection.ToString());

				if (mc.Format != ColumnFormatEnum.Default)
					tw.WriteAttributeString("DisplayFormat", mc.Format.ToString());

				tw.WriteAttributeString("DisplayWidth", mc.Width.ToString());

				if (mc.Decimals != 0)
					tw.WriteAttributeString("Decimals", mc.Decimals.ToString());

				if (mc.HorizontalAlignment != HorizontalAlignmentEx.Default)
					tw.WriteAttributeString("HorizontalAlignment", mc.HorizontalAlignment.ToString());

				if (mc.VerticalAlignment != VerticalAlignmentEx.Top)
					tw.WriteAttributeString("VerticalAlignment", mc.VerticalAlignment.ToString());

				if (Lex.IsDefined(mc.CondFormatName))
					tw.WriteAttributeString("CondFormat", mc.CondFormatName);

				if (mc.TableMap != "") // && mc.TableMap != mt.TableMap)
					tw.WriteAttributeString("TableMap", mc.TableMap);

				if (mc.ColumnMap != "") // && mc.ColumnMap != mc.Name)
					tw.WriteAttributeString("ColumnMap", mc.ColumnMap);

				if (mc.DataTransform != "")
					tw.WriteAttributeString("DataTransform", mc.DataTransform);

				if (Lex.IsDefined(mc.InitialCriteria))
					tw.WriteAttributeString("InitialCriteria", mc.InitialCriteria);

				if (mc.IsSearchable != true)
					tw.WriteAttributeString("Searchable", mc.IsSearchable.ToString());

				if (FlagHelper.IsSet(mc.SearchMethods, SearchMethods.Cartridge))
					tw.WriteAttributeString("CartridgeSearchable", true.ToString());

				if (FlagHelper.IsSet(mc.SearchMethods, SearchMethods.SmallWorld))
					tw.WriteAttributeString("SmallWorldSearchable", true.ToString());

				if (mc.BrokerFiltering != false)
					tw.WriteAttributeString("BrokerFiltering", mc.BrokerFiltering.ToString());

				if (mc.ClickFunction != "")
					tw.WriteAttributeString("ClickFunction", mc.ClickFunction);

				if (mc.ImportFilePosition != "")
					tw.WriteAttributeString("FilePosition", mc.ImportFilePosition);

				if (mc.MetaBrokerType != MetaBrokerType.Unknown)
					tw.WriteAttributeString("MetaBroker", mc.MetaBrokerType.ToString());

				tw.WriteEndElement(); // end of MetaColumn
			}

			tw.WriteEndElement(); // end of MetaTable
			return;
		}

/// <summary>
/// Write a list of values
/// </summary>
/// <param name="list"></param>
/// <param name="tw"></param>
/// <param name="attrName"></param>

		void SerializeList(
			XmlTextWriter tw,
			string attrName,
			List<string> list)
		{
			if (list == null || list.Count == 0) return;
			string txt = "";
			foreach (string s in list)
			{
				if (txt != "") txt += ",";
				txt += s;
			}

			tw.WriteAttributeString(attrName, txt);
		}

		/// <summary>
		/// Parse xml of a single MetaTable
		/// </summary>
		/// <param name="txt">XML form of metatable</param>
		/// <returns></returns>

		public static MetaTable Deserialize(
			string txt)
		{
			MetaTable[] mta = null;

			if (txt == null || txt == "") return null;

			try
			{
				XmlMemoryStreamTextReader mstr = new XmlMemoryStreamTextReader(txt);
				XmlTextReader tr = mstr.Reader;
				mta = DeserializeMetaTables(tr, null, false);
				mstr.Close();
			}
			catch (Exception ex)
			{
				DebugLog.Message(ex.Message);
				return null;
			}

			if (mta != null && mta.Length > 0 && mta[0] != null)
				return mta[0];
			else return null;
		}

		/// <summary>
		/// Parse metatable XML
		/// </summary>
		/// <param name="textReader">TextReader containing XML</param>
		/// <returns></returns>

		public static MetaTable[] DeserializeMetaTables(
			XmlTextReader tr,
			string tableFolder,
			bool updateMap)
		{
			int i1, mci;
			String tok, txt;
			bool singleMetatableMode, b1 = false;

			// First, turn off unwanted restricted access logging. I.e. allow creation of unrestricted metatables in an
			// XML file while filtering out out restricted tables but not logging them as attempted access violations.
			// Restore setting in the finally block

			bool oldAccessSetting =  RestrictedMetaTables.SetRejectedAccessAttemptLogging(false); 

			try
			{

				if (tr.ReadState == ReadState.Initial) //  read first element if not done yet
				{
					tr.Read();
					tr.MoveToContent();
				}

				if (Lex.Eq(tr.Name, "MetaTables")) // multiple metatables
				{
					tr.Read(); // move to first MetaTable
					tr.MoveToContent();
					singleMetatableMode = false;
				}

				else if (Lex.Eq(tr.Name, "MetaTable")) // single metatable
				{
					singleMetatableMode = true;
				}

				else throw new Exception("Expected MetaTable(s) element but saw " + tr.Name);

				List<MetaTable> mtl = new List<MetaTable>(); // accumulate list of metatables

				while (true) // loop on each metatable & Include reference
				{
					if (Lex.Eq(tr.Name, "MetaTable"))
					{
						string alias = "";

						MetaTable mt = new MetaTable();
						mtl.Add(mt); // add to list

						txt = XmlUtil.GetAttribute(tr, "TableName");
						if (txt == null) txt = XmlUtil.GetAttribute(tr, "Name");
						if (txt == null) txt = XmlUtil.GetAttribute(tr, "N");
						if (String.IsNullOrEmpty(txt)) throw new Exception("Table name missing");
						mt.Name = txt.ToUpper();

						XmlUtil.GetStringAttribute(tr, "Alias", ref alias);

						txt = XmlUtil.GetAttribute(tr, "Label");
						if (txt == null) txt = XmlUtil.GetAttribute(tr, "L");
						if (txt != null) mt.Label = txt;

						txt = XmlUtil.GetAttribute(tr, "MetaBroker");
						if (txt != null)
						{
							if (Lex.Eq(txt, "NonOracle")) mt.MetaBrokerType = MetaBrokerType.NoSql; // fix to handle previous name for NonSql
							else if (!EnumUtil.TryParse(txt, out mt.MetaBrokerType))
								throw new Exception("Unknown MetaBroker type: " + txt);
						}

						txt = XmlUtil.GetAttribute(tr, "ShortLabel");
						if (txt == null) txt = XmlUtil.GetAttribute(tr, "Sl");
						if (txt == null) txt = XmlUtil.GetAttribute(tr, "L2");
						if (txt != null) mt.ShortLabel = txt;

						XmlUtil.GetBoolAttribute(tr, "AllowColumnMerging", ref mt.AllowColumnMerging);

						txt = XmlUtil.GetAttribute(tr, "Parent");
						if (txt != null) mt.Parent = MetaTableCollection.Get(txt);

						XmlUtil.GetBoolAttribute(tr, "SingleRowPerKeyValue", ref mt.SingleRowPerKeyValue);

						txt = XmlUtil.GetAttribute(tr, "TableMap");
						if (txt == null) txt = XmlUtil.GetAttribute(tr, "Map");
						if (!Lex.IsNullOrEmpty(txt))
							mt.TableMap = FixupTableMapSyntax(txt);

						txt = XmlUtil.GetAttribute(tr, "WarehouseMap");
						if (!Lex.IsNullOrEmpty(txt))
						{
							mt.WarehouseMap = FixupTableMapSyntax(txt);
							if (mt.WarehouseMap.EndsWith(")")) mt.WarehouseMap += " " + mt.Name;
						}

						XmlUtil.GetIntAttribute(tr, "MetaTableId", ref mt.Id);

						//txt = XmlUtil.GetAttribute(tr, "MetaTableId");
						//if (txt == null) txt = XmlUtil.GetAttribute(tr, "Id");
						//if (txt != null)
						//{
						//  tok = txt;
						//  i1 = Convert.ToInt32(tok);
						//  mt.Id = i1;
						//}

						XmlUtil.GetStringAttribute(tr, "Code", ref mt.Code);

						if (!XmlUtil.GetStringAttribute(tr, "Description", ref mt.Description))
							XmlUtil.GetStringAttribute(tr, "Desc", ref mt.Description);

						DeserializeList(tr, "TableFilterColumns", ref mt.TableFilterColumns);
						DeserializeList(tr, "TableFilterValues", ref mt.TableFilterValues);
						DeserializeList(tr, "PivotMergeColumns", ref mt.PivotMergeColumns);
						DeserializeList(tr, "PivotColumns", ref mt.PivotColumns);

						if (!XmlUtil.GetBoolAttribute(tr, "RemapForRetrieval", ref mt.MultiPivot))
							XmlUtil.GetBoolAttribute(tr, "MultiPivot", ref mt.MultiPivot);

						if (!XmlUtil.GetBoolAttribute(tr, "SummarizedExists", ref mt.SummarizedExists))
							XmlUtil.GetBoolAttribute(tr, "SummarizedAvailable", ref mt.SummarizedExists); // obsolete attribute name

						XmlUtil.GetBoolAttribute(tr, "UseSummarizedData", ref mt.UseSummarizedData);

						txt = XmlUtil.GetAttribute(tr, "SubstanceSummarizationType");
						if (txt != null) EnumUtil.TryParse(txt, out mt.SubstanceSummarizationType);

						XmlUtil.GetStringAttribute(tr, "TargetSymbol", ref mt.TargetSymbol);
						XmlUtil.GetIntAttribute(tr, "TargetId", ref mt.TargetId);
						XmlUtil.GetStringAttribute(tr, "GeneFamily", ref mt.GeneFamily);
						XmlUtil.GetStringAttribute(tr, "AssayType", ref mt.AssayType);
						XmlUtil.GetStringAttribute(tr, "AssayMode", ref mt.AssayMode);
						XmlUtil.GetStringAttribute(tr, "AssayLocation", ref mt.AssayLocation);

						txt = XmlUtil.GetAttribute(tr, "RowCountSql");
						mt.RowCountSql = txt;

						txt = XmlUtil.GetAttribute(tr, "UpdateDateTimeSql");
						if (txt != null) mt.UpdateDateTimeSql = txt;

						txt = GetImportAttribute(tr, "ImportParms.ClientFile", mt); // import file parameters
						if (txt != null) mt.ImportParms.FileName = txt;

						// Import parameters

						txt = GetImportAttribute(tr, "ImportParms.ClientFileModified", mt);
						if (txt != null) mt.ImportParms.ClientFileModified = DateTimeUS.ParseDate(txt);

						txt = GetImportAttribute(tr, "ImportParms.ServerFile", mt);
						if (txt != null) mt.ImportParms.FileName = txt;

						txt = GetImportAttribute(tr, "ImportParms.Delim", mt);
						if (txt != null && txt.Length == 1) mt.ImportParms.Delim = Convert.ToChar(txt);

						txt = GetImportAttribute(tr, "ImportParms.MultDelimsAsSingle", mt);
						if (txt != null) mt.ImportParms.MultDelimsAsSingle = Convert.ToBoolean(txt);

						txt = GetImportAttribute(tr, "ImportParms.TextQualifier", mt);
						if (txt != null && txt.Length == 1) mt.ImportParms.TextQualifier = Convert.ToChar(txt);

						txt = GetImportAttribute(tr, "ImportParms.FirstLineHeaders", mt);
						if (txt != null) mt.ImportParms.FirstLineHeaders = Convert.ToBoolean(txt);

						txt = GetImportAttribute(tr, "ImportParms.DeleteExisting", mt);
						if (txt != null) mt.ImportParms.DeleteExisting = Convert.ToBoolean(txt);

						txt = GetImportAttribute(tr, "ImportParms.DeleteDataOnly", mt);
						if (txt != null) mt.ImportParms.DeleteDataOnly = Convert.ToBoolean(txt);

						txt = GetImportAttribute(tr, "ImportParms.ImportInBackground", mt);
						if (txt != null) mt.ImportParms.ImportInBackground = Convert.ToBoolean(txt);

						txt = GetImportAttribute(tr, "ImportParms.CheckForFileUpdates", mt);
						if (txt != null) mt.ImportParms.CheckForFileUpdates = Convert.ToBoolean(txt);

						if (mt.Label == "")
							mt.Label = IdToLabel(mt.Name);
						if (mt.MetaBrokerType == 0)
							mt.MetaBrokerType = MetaBrokerType.Generic;

						if (tr.IsEmptyElement) // just a metatable header?
						{
							tr.Read(); // move to next metatable, etc.
							tr.MoveToContent();
							continue;
						}

						// Process the MetaColumns for MetaTable

						tr.Read(); // move to first MetaColumn
						tr.MoveToContent();

						while (true) // loop on MetaColumns
						{
							if (tr.NodeType == XmlNodeType.Element &&
								(Lex.Eq(tr.Name, "MetaColumn") || Lex.Eq(tr.Name, "C")))
							{
								// Get MetaColumn attributes

								MetaColumn mc = new MetaColumn();
								mc.MetaTable = mt;

								txt = XmlUtil.GetAttribute(tr, "Id");
								if (txt != null) mc.Id = Convert.ToInt32(txt);

								txt = XmlUtil.GetAttribute(tr, "ResultCode");
								if (Lex.IsUndefined(txt)) txt = XmlUtil.GetAttribute(tr, "Code");
								if (Lex.IsDefined(txt)) mc.ResultCode = txt;

								txt = XmlUtil.GetAttribute(tr, "ResultCode2");
								if (Lex.IsUndefined(txt)) txt = XmlUtil.GetAttribute(tr, "SummaryResultCode");
								if (Lex.IsDefined(txt)) mc.ResultCode2 = txt;

								txt = XmlUtil.GetAttribute(tr, "ColumnName");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "Name");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "N");
								if (String.IsNullOrEmpty(txt)) throw new Exception("Column name missing");
								mc.Name = txt.Trim().ToUpper();

								if (Lex.Eq(mc.Name, "gene_fmly")) mc = mc; // debug

								txt = XmlUtil.GetAttribute(tr, "Label");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "L");
								if (txt != null)
								{
									i1 = txt.IndexOf("\t");
									if (i1 < 0) mc.Label = txt;
									else // fix old style merged label
									{
										mc.LabelImage = txt.Substring(i1 + 1);
										mc.Label = txt.Substring(0, i1);
									}

									if (mc.Label.Length == 0)
										mc.Label = null; // assign default value later
								}

								txt = XmlUtil.GetAttribute(tr, "ShortLabel");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "Sl");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "l2");
								if (txt != null) mc.ShortLabel = txt;

								XmlUtil.GetStringAttribute(tr, "LabelImage", ref mc.LabelImage);

								txt = XmlUtil.GetAttribute(tr, "DataType");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "Type");
								if (txt == null) throw new Exception("DataType not defined for " + mt.Name + "." + mc.Name);

								mc.DataType = MetaColumn.ParseMetaColumnTypeString(txt);
								if (mc.DataType == MetaColumnType.Unknown)
									throw new Exception("Unexpected DataType for " + mt.Name + "." + mc.Name + ": " + txt);

								if (Lex.Eq(txt, "MolFile")) // structure stored as molfile string
									mc.DataTransform = "FromMolFile";

								else if (Lex.Eq(txt, "Smiles")) // structure stored as smiles string
									mc.DataTransform = "FromSmiles";

								else if (Lex.Eq(txt, "Chime")) // structure stored as chime string
									mc.DataTransform = "FromChime";

								else if (Lex.Eq(txt, "InChI")) // structure stored as InChI string
									mc.DataTransform = "FromInChI";

								if (!XmlUtil.GetStringAttribute(tr, "DataTypeImage", ref mc.DataTypeImageName))
									XmlUtil.GetStringAttribute(tr, "TypeImage", ref mc.DataTypeImageName);

								txt = XmlUtil.GetAttribute(tr, "StorageType");
								if (txt != null)
								{
									try { mc.StorageType = MetaColumn.ParseStorageTypeString(txt); }
									catch (Exception ex)
									{
										throw new Exception
											("Unexpected StorageType for " + mt.Name + "." + mc.Name + ": " + txt);
									}
								}

								txt = XmlUtil.GetAttribute(tr, "Key");
								if (txt != null)
								{
									b1 = ParseBoolAttribute(txt);
									if (b1) mc.KeyPosition = 0;
								}

								txt = XmlUtil.GetAttribute(tr, "PositionInKey");
								if (txt != null) mc.KeyPosition = int.Parse(txt);

								// Other attributes

								txt = XmlUtil.GetAttribute(tr, "SummarizedExists");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "Summarized");
								if (txt != null) mc.SummarizedExists = Convert.ToBoolean(txt);

								txt = XmlUtil.GetAttribute(tr, "UnsummarizedExists");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "Unsummarized");
								if (txt != null) mc.UnsummarizedExists = Convert.ToBoolean(txt);

								txt = XmlUtil.GetAttribute(tr, "SS"); // summarization state: U = unsummarized exists, S = summarized exists or US = both exist
								if (txt != null)
								{
									mc.UnsummarizedExists = Lex.Contains(txt, "U");
									mc.SummarizedExists = Lex.Contains(txt, "S");
								}

								txt = XmlUtil.GetAttribute(tr, "SummarizationRole");
								if (txt != null) try
									{ mc.SummarizationRole = (SummarizationRole)Enum.Parse(typeof(SummarizationRole), txt); }
									catch { }

								XmlUtil.GetBoolAttribute(tr, "PrimaryResult", ref mc.PrimaryResult);
								XmlUtil.GetBoolAttribute(tr, "SecondaryResult", ref mc.SecondaryResult);

								XmlUtil.GetBoolAttribute(tr, "SinglePoint", ref mc.SinglePoint);
								XmlUtil.GetBoolAttribute(tr, "Concentration", ref mc.Concentration);
								XmlUtil.GetBoolAttribute(tr, "MultiPoint", ref mc.MultiPoint);

								XmlUtil.GetBoolAttribute(tr, "DetailsAvailable", ref mc.DetailsAvailable);

								txt = XmlUtil.GetAttribute(tr, "Case");
								if (txt != null)
								{
									if (Lex.Eq(txt, "Unknown"))
										mc.TextCase = ColumnTextCaseEnum.Unknown;
									else if (Lex.Eq(txt, "Lower"))
										mc.TextCase = ColumnTextCaseEnum.Lower;
									else if (Lex.Eq(txt, "Upper"))
										mc.TextCase = ColumnTextCaseEnum.Upper;
									else if (Lex.Eq(txt, "Mixed"))
										mc.TextCase = ColumnTextCaseEnum.Mixed;
									else
										throw new Exception
											("Expected Case lower, upper or mixed but saw " +
											txt);
								}

								XmlUtil.GetBoolAttribute(tr, "IgnoreCase", ref mc.IgnoreCase);

								txt = XmlUtil.GetAttribute(tr, "Dictionary");
								if (txt != null) mc.Dictionary = txt.ToUpper();

								txt = XmlUtil.GetAttribute(tr, "DictionaryMultipleSelect");
								if (txt != null) mc.DictionaryMultipleSelect = ParseBoolAttribute(txt);

								txt = XmlUtil.GetAttribute(tr, "Description");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "Desc");
								if (txt != null) mc.Description = txt;

								txt = XmlUtil.GetAttribute(tr, "InitialSelection");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "DisplaySelection");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "Selection");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "DisplayLevel");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "Level");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "Display");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "D");
								if (txt != null)
								{
									if (Lex.Eq(txt, "Selected") ||
										Lex.Eq(txt, "SelectedByDefault") ||
										Lex.Eq(txt, "Default"))
										mc.InitialSelection = ColumnSelectionEnum.Selected;
									else if (Lex.Eq(txt, "Unselected") ||
									 Lex.Eq(txt, "Selectable"))
										mc.InitialSelection = ColumnSelectionEnum.Unselected;
									else if (Lex.Eq(txt, "Unselectable"))
										mc.InitialSelection = ColumnSelectionEnum.Unselectable;
									else if (Lex.Eq(txt, "Hidden"))
										mc.InitialSelection = ColumnSelectionEnum.Hidden;
									else if (Lex.Eq(txt, "Comment"))
										mc.InitialSelection = ColumnSelectionEnum.Comment;
									else if (Lex.Eq(txt, "Unknown"))
										mc.InitialSelection = ColumnSelectionEnum.Unknown;
									else
										throw new Exception
											("Expected DisplayLevel Default, Selectable or Hidden but saw " +
											txt);
								}

								txt = XmlUtil.GetAttribute(tr, "Displayformat");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "Format");
								if (txt != null)
								{
									if (!EnumUtil.TryParse(txt, out mc.Format))
										throw new Exception("Invalid DisplayFormat " + txt);
								}

								txt = XmlUtil.GetAttribute(tr, "DisplayWidth");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "Width");
								if (txt != null) mc.Width = Convert.ToSingle(txt);

								XmlUtil.GetIntAttribute(tr, "Decimals", ref mc.Decimals);

								txt = tr.GetAttribute("HorizontalAlignment");
								if (txt != null) mc.HorizontalAlignment =
									(HorizontalAlignmentEx)Enum.Parse(typeof(HorizontalAlignmentEx), txt);

								txt = tr.GetAttribute("VerticalAlignment");
								if (txt != null) mc.VerticalAlignment =
									(VerticalAlignmentEx)Enum.Parse(typeof(VerticalAlignmentEx), txt);

								XmlUtil.GetStringAttribute(tr, "TableMap", ref mc.TableMap);

								txt = XmlUtil.GetAttribute(tr, "ColumnMap");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "Map");
								if (txt != null)
								{
									mc.ColumnMap = txt;
									if (Lex.StartsWith(mc.ColumnMap, "InternalMethod") ||
									 Lex.StartsWith(mc.ColumnMap, "PluginMethod") ||
									 Lex.StartsWith(mc.ColumnMap, "ExternalToolMethod")) // old keyword
										mc.IsSearchable = false; // can't search if external tool method
									else mc.ColumnMap = mc.ColumnMap.ToUpper(); // make other mappings uppercase
								}

								XmlUtil.GetStringAttribute(tr, "DataTransform", ref mc.DataTransform);

								txt = XmlUtil.GetAttribute(tr, "UnitCode");
								if (txt == null) txt = XmlUtil.GetAttribute(tr, "Units");
								if (txt != null) mc.Units = txt;

								XmlUtil.GetStringAttribute(tr, "InitialCriteria", ref mc.InitialCriteria);


								if (XmlUtil.GetBoolAttribute(tr, "Searchable", ref b1))
									mc.IsSearchable = b1;

								if (XmlUtil.GetBoolAttribute(tr, "CartridgeSearchable", ref b1))
									mc.IsChemistryCartridgeSearchable = b1;

								if (XmlUtil.GetBoolAttribute(tr, "SmallWorldSearchable", ref b1))
									mc.IsSmallWorldSearchable = b1;

								txt = XmlUtil.GetAttribute(tr, "CondFormat");
								if (txt != null) mc.CondFormatName = txt;

								txt = XmlUtil.GetAttribute(tr, "MetaBroker");
								if (txt != null)
								{
									if (!EnumUtil.TryParse(txt, out mc.MetaBrokerType))
										throw new Exception("Unknown MetaBroker: " + txt);
								}

								XmlUtil.GetBoolAttribute(tr, "BrokerFiltering", ref mc.BrokerFiltering);
								XmlUtil.GetStringAttribute(tr, "ClickFunction", ref mc.ClickFunction);

								txt = XmlUtil.GetAttribute(tr, "FilePosition");
								if (txt != null)
								{
									mc.ImportFilePosition = txt;
									if (mc.ImportFilePosition == "0") mc.ImportFilePosition = ""; // remap undefined numeric value
								}

								if (mc.Label == "")
									mc.Label = IdToLabel(mc.Name);
								if (mc.ColumnMap == "")
									mc.ColumnMap = mc.Name;
								if (mc.InitialSelection == 0)
									mc.InitialSelection = ColumnSelectionEnum.Selected;

								if (mc.Width <= 0) // need to supply default width
								{
									mc.Width = MetaColumn.GetDefaultDisplayWidth(mc.DataType);
									if (mc.Decimals == 0 && (mc.DataType == MetaColumnType.Number ||
										mc.DataType == MetaColumnType.QualifiedNo))
										mc.Decimals = 3;
								}

								if (tr.IsEmptyElement) // is this an empty element?
								{
									if (mt.GetMetaColumnByName(mc.Name) == null) // add to metatable if not already in table
										mt.AddMetaColumn(mc);

									tr.Read(); // move to next element
									tr.MoveToContent();
									continue; // done with MetaColumn
								}

								tr.Read(); // move to next element for MetaColumn
								tr.MoveToContent();

								// Get any qualified number column assingments

								if (Lex.Eq(tr.Name, "QualifiedNumberColumns"))
								{
									txt = XmlUtil.GetAttribute(tr, "Qualifier");
									if (txt != null) mt.QnQualifier = txt;
									txt = XmlUtil.GetAttribute(tr, "NumberValue");
									if (txt != null) mt.QnNumberValue = txt;
									txt = XmlUtil.GetAttribute(tr, "NumberValueHigh");
									if (txt != null) mt.QnNumberValueHigh = txt;
									txt = XmlUtil.GetAttribute(tr, "NValue");
									if (txt != null) mt.QnNValue = txt;
									txt = XmlUtil.GetAttribute(tr, "NValueTested");
									if (txt != null) mt.QnNValueTested = txt;
									txt = XmlUtil.GetAttribute(tr, "StandardDeviation");
									if (txt != null) mt.QnStandardDeviation = txt;
									txt = XmlUtil.GetAttribute(tr, "StandardError");
									if (txt != null) mt.QnStandardError = txt;
									txt = XmlUtil.GetAttribute(tr, "TextValue");
									if (txt != null) mt.QnTextValue = txt;
									txt = XmlUtil.GetAttribute(tr, "LinkValue");
									if (txt != null) mt.QnLinkValue = txt;

									bool empty = (tr.IsEmptyElement);
									tr.Read(); // move to next element
									tr.MoveToContent();

									if (!empty)
									{
										if (Lex.Eq(tr.Name, "QualifiedNumberColumns") &&
											tr.NodeType == XmlNodeType.EndElement)
										{
											tr.Read(); // move to next element
											tr.MoveToContent();
										}

										else throw new Exception("Expected QualifiedNumberColumns end element");
									}
								}

								if (mc.DataType == MetaColumnType.Integer)
								{ // set decimal format for integers
									mc.Format = ColumnFormatEnum.Decimal;
									mc.Decimals = 0;
								}

								if (mt.GetMetaColumnByName(mc.Name) == null) // add to metatable if not already in table
									mt.AddMetaColumn(mc);

								if (tr.NodeType == XmlNodeType.EndElement && // MetaColumn end element?
								(Lex.Eq(tr.Name, "MetaColumn") || Lex.Eq(tr.Name, "C")))
								{
									tr.Read();
									tr.MoveToContent();
									continue;
								}

								else throw new Exception("Expected MetaColumn end element");

							} // end for MetaColumn


							else if (tr.NodeType == XmlNodeType.EndElement && // MetaTable end element?
							Lex.Eq(tr.Name, "MetaTable"))
							{
								if (updateMap) MetaTableCollection.Add(mt);
								break;
							}
							else if (tr.NodeType == XmlNodeType.Comment ||
							 tr.NodeType == XmlNodeType.Text ||
							 tr.NodeType == XmlNodeType.Whitespace)
							{
								//ignore it
								tr.MoveToContent();
								tr.Read();
							}
							else throw new Exception("Expected MetaColumn or MetaTable end element");
						} // end of MetaColumn loop

						if (mt.KeyMetaColumn == null && mt.MetaColumns.Count > 0)
							mt.MetaColumns[0].KeyPosition = 0; // assume first column is key

						if (!Lex.IsNullOrEmpty(alias)) // create alias table if defined
						{
							MetaTable mt2 = mt.Clone();
							mt2.Name = alias.ToUpper();
							mtl.Add(mt2);
						}

						if (singleMetatableMode)
							return mtl.ToArray();

						tr.Read();
						tr.MoveToContent();

						PerformCustomFixups(mt);

					} // end for MetaTable

					// Include other file

					else if (Lex.Eq(tr.Name, "Include"))
					{
						StreamReader sr = null;
						XmlTextReader tr2 = null;

						string fileName = XmlUtil.GetAttribute(tr, "FileName");
						if (fileName == null) throw new Exception("Expected fileName in Include");

						if (fileName.IndexOf("\\") < 0)
						{
							if (tableFolder == null) throw new Exception("TableFolder not defined");
							fileName = tableFolder + "\\" + fileName;
						}

						try // parse the included file 
						{
							int t0 = TimeOfDay.Milliseconds();
							sr = new StreamReader(fileName);
							tr2 = new XmlTextReader(sr);
							DeserializeMetaTables(tr2, tableFolder, updateMap);
							sr.Close();
							//if (MetaTreeFactory.LogStartupTiming)
							//  t0 = DebugLog.TimeMessage("MetaTableFactory include " + fileName + " time", t0);

						}
						catch (Exception ex)
						{
							if (sr != null) sr.Close();
							throw new Exception("Error in file " + fileName + ": " + ex.Message);
						}

						tr.Read();
						tr.MoveToContent();
						continue;
					}

					else if (tr.NodeType == XmlNodeType.EndElement && // done with all tables
						Lex.Eq(tr.Name, "MetaTables"))
					{
						//add new metatables to the list of known tables

						List<MetaTable> mtl2 = new List<MetaTable>(); // accumulate list of metatables that are successfully added to the collection

						for (int i = 0; i < mtl.Count; i++)
						{
							MetaTable mt = mtl[i];

							if (!MetaTableCollection.TableMap.ContainsKey(mt.Name))
							{
								if (MetaTableCollection.Add(mt))
									mtl2.Add(mt); // if added successfully include it in the list of tables returned
							}
						}

						return mtl2.ToArray();
					}

					else throw new Exception("Expected MetaTable or Include element but saw " + tr.Name);
				}
			} // end of try

			finally // restore restricted access logging setting
			{
				RestrictedMetaTables.RestoreRejectedAccessAttemptLogging(oldAccessSetting); 
			}

		}

/// <summary>
/// FixupMapSyntax
/// </summary>
/// <param name="sql"></param>
/// <returns></returns>

		static string FixupTableMapSyntax(string sql)
		{
			sql = sql.Trim();
			sql = sql.Replace("\t\t", " "); // remove newlines, tabs
			sql = sql.Replace("\n\r\t", " ");
			sql = sql.Replace("\n", " ");
			sql = sql.Replace("\r", " ");
			sql = sql.Replace("\t", " ");
			if (sql.ToLower().StartsWith("select "))
				sql = "(" + sql + ")"; // enclose in parens so can be used as subquery if necessary
			return sql;
		}

		/// <summary>
		/// PerformCustomFixups
		/// </summary>
		/// <param name="mt"></param>

		static void PerformCustomFixups(MetaTable mt)
		{
			if (DebugMx.True) return;

			if (Lex.Eq(mt.Name, "corp_structure") && !Lex.Contains(mt.TableMap, "s.helm_txt) molstructure"))
			{
				mt.TableMap = FixupTableMapSyntax(@"
		select
			m.corp_nbr,
			decode (nvl(length(s.helm_txt), 0), 0, chime(m.ctab), s.helm_txt) molstructure,
			m.ctab,
			s.mol_formula molformula,
			s.mol_weight molweight,
			decode (length(s.helm_txt), 0, null, s.helm_txt) helm_txt,
			decode (length(s.sequence_txt), 0, null, s.sequence_txt) sequence_txt,
			m2.molsmiles,
			m.molecule_date
		from
			corp_owner.corp_moltable m,
			corp_owner.corp_substance s,
			mbs_owner.corp_moltable_mx m2
		where
			s.corp_nbr = M.CORP_NBR
			and (s.status_code is null or s.status_code = 'A')	
			and m2.corp_nbr (+) = m.corp_nbr 
		/* AndIncludeKeySubsetOn(m.corp_nbr) */
");
			}

			else if (Lex.Eq(mt.Name, "corp_structure2") && !Lex.Contains(mt.TableMap, "s.helm_txt) molstructure"))
			{
				mt.TableMap = FixupTableMapSyntax(@"
		select 
		 m.corp_nbr,
		 decode (nvl(length(s.helm_txt), 0), 0, chime(m.ctab), s.helm_txt) molstructure,
		 m.ctab, 
		 s.mol_formula molformula,
		 s.mol_weight molweight,
		 m2.molsmiles,
		 decode (length(s.helm_txt), 0, null, s.helm_txt) helm_txt,
 		 decode (length(s.sequence_txt), 0, null, s.sequence_txt) sequence_txt,
		 m.molecule_date
		from
			corp_owner.corp_moltable m,
			corp_owner.corp_substance s,
			mbs_owner.corp_moltable_mx m2
		where 
			s.corp_nbr = M.CORP_NBR 
			and (s.status_code is null or s.status_code = 'A')	 
			and m2.corp_nbr (+) = m.corp_nbr
			/* AndIncludeKeySubsetOn(m.corp_nbr) */	
		 UNION ALL
		select
		  rs.corp_nbr,
		  to_clob('CompoundId=' || corp_nbr) molstructure,
		  null ctab,AccessMx.TryGetMobiusSystemWindowsAccount(out userName, out pw))

		  null molformula,
		  null molweight,
		  null Molsmiles,
		  null helm_txt,
		  null sequence_txt,
		  null molecule_date
		from 
		  corp_owner.strd_sad_reg_lrg_substance_mv rs 
");
			}

		}

		/// <summary>
		/// Get the TableMap with the enclosing parans removed
		/// </summary>

		public string GetTableMapWithEnclosingParensRemoved()
		{
				string tm = Lex.RemoveOuterBrackets(TableMap, "(", ")");
				return tm;
		}

		/// <summary>
		/// Return the tablemap with a SQL subquery alias name appended
		/// Note that is required for some SQL DBs (e.g. Postgres)
		/// </summary>
		/// <param name="mtSuffixToAppend"></param>
		/// <returns></returns>

		public string GetTableMapWithAliasAppendedIfNeeded (string mtSuffixToAppend = null)
		{
			string tm = TableMap;

			if (Lex.IsUndefined(mtSuffixToAppend) && !Lex.Contains(tm, "SELECT") && !Lex.Contains(tm, "FROM"))
				return tm; // if no suffix defined and not a select statement then no alias is needed

			if (Lex.IsUndefined(mtSuffixToAppend))
				mtSuffixToAppend = "_ALIAS";

			string alias = Name + mtSuffixToAppend;
			alias = IdentifierMx.TextToValidOracleName(alias);

			string tm2 = tm + " " + alias;
			return tm2;
		}

		public string GetTableMapWithAliasAppendedObsolete(string alias = null)
		{
			string tm = TableMap;

			if (Lex.IsUndefined(alias) && Lex.Contains(tm, "SELECT") && Lex.Contains(tm, "FROM"))
				alias = Name + "_ALIAS";

			tm += " " + alias;
			return tm;
		}

		/// <summary>
		/// Deserialize a list of strings
		/// </summary>
		/// <param name="tr"></param>
		/// <param name="attrName"></param>
		/// <param name="list"></param>

		static void DeserializeList(
			XmlTextReader tr,
			string attrName,
			ref List<string> list)
		{
			string txt = XmlUtil.GetAttribute(tr, attrName);
			if (Lex.IsNullOrEmpty(txt))
			{
				list = null;
				return;
			}

			list = new List<string>();
			string[] sa = txt.Split(',');
			foreach (string s in sa)
			{
				list.Add(s.Trim());
			}

			return;
		}

/// <summary>
/// Parse a boolean attribute
/// </summary>
/// <param name="txt"></param>
/// <returns></returns>

		static bool ParseBoolAttribute(
			string txt)
		{
			if (Lex.Eq(txt, "true"))
				return true;
			else if (Lex.Eq(txt, "false"))
				return false;
			else
				throw new Exception
					("Expected \"True\" or \"False\" but saw " +
					txt);
		}

		/// <summary>
		/// Get import file parameter & allocate import parms if needed
		/// </summary>
		/// <param name="tr"></param>
		/// <param name="attrName"></param>
		/// <param name="mt"></param>
		/// <returns></returns>

		static string GetImportAttribute(
			XmlTextReader tr,
			string attrName,
			MetaTable mt)
		{
			string txt = XmlUtil.GetAttribute(tr, attrName);
			if (txt == null) return null;

			if (mt.ImportParms == null) mt.ImportParms = new UserDataImportParms();
			return txt;
		}

		/// <summary>
		/// Convert an identifier to a crude label
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		/// 

		public static String IdToLabel(String id)
		{
			StringBuilder label;
			char pc, cc;
			int i1;

			label = new StringBuilder(id);
			pc = ' ';
			for (i1 = 0; i1 < label.Length; i1++)
			{
				cc = label[i1];
				if (Char.IsLetter(cc))
				{
					if (pc == ' ')
						label[i1] = Char.ToUpper(cc);
					else
						label[i1] = Char.ToLower(cc);
				}
				else if (cc == '_')
				{
					label[i1] = ' ';
					cc = ' ';
				}
				pc = cc;
			}
			return label.ToString();
		}

		/// <summary>
		/// Return true if the keys associated with the table support grouping (i.e. by salt)
		/// </summary>

		public bool SupportsKeyGrouping
		{
			get
			{
				if (Lex.Eq(this.Root.Name, MetaTable.PrimaryRootTable)) return true; // todo: generalize
				else return false;
			}
		}

		/// <summary>
		/// See if we should require concentration values to match when joining result rows together
		/// Ex: if % inh type assay with multiple concentrations and other join vars the same
		/// Current implementation makes special use of mt.PivotColumns 
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public bool JoinOnConcentration
		{
			get { return (PivotColumns != null && PivotColumns.Count > 0); }

			set
			{
				if (value == true)
				{
					PivotColumns = new List<string>();
					PivotColumns.Add(Name);
				}

				else PivotColumns = null;

				return;
			}
		}

		/// <summary>
		/// Clone metatable
		/// </summary>
		/// <returns></returns>

		public MetaTable Clone()
		{
			MetaTable mt = (MetaTable)this.MemberwiseClone();
			mt._keyMetaColumn = null; // clear since new col will be key

			if (this.MetaColumns != null) // deeper clone of metacolumns
			{
				mt.MetaColumns = new List<MetaColumn>();
				foreach (MetaColumn mc in this.MetaColumns)
				{
					MetaColumn mc2 = mc.Clone();
					mc2.MetaTable = mt; // point to new metatable
					mt.MetaColumns.Add(mc2);
				}
			}

			if (this.ImportParms != null)
				mt.ImportParms = this.ImportParms.Clone();

			return mt;
		}

		/// <summary>
		/// Display name and label for convenience in debug view
		/// </summary>
		/// <returns></returns>

		public override string ToString()
		{
			string txt = "MetaTable: " + Name + ", Label: " + Label + "\r\n";
			txt += "MetaColumns\r\n";
			for (int mci = 0; mci < MetaColumns.Count; mci++)
			{
				MetaColumn mc = MetaColumns[mci];
				if (UseUnsummarizedData && !mc.UnsummarizedExists) continue; // just show available cols for summarization level
				else if (UseSummarizedData && !mc.SummarizedExists) continue;

				txt += mc.Name + "\t" + mc.Label + "\r\n";
				//txt += mc.Name + "\t" + mc.Label + "\t" + mc.TableMap + "\t" + mc.ColumnMap + "\r\n";
			}

			return txt;
		}

	} // MetaTable

/// <summary>
/// MetaTable name utilities
/// </summary>

	public class MetaTableName
	{
		/// <summary>
		/// Dictionary of associations between MetaBrokerTypes and MetaTable prefixes
		/// </summary>

		public static Dictionary<MetaBrokerType, List<string>> MetaTableNamePrefixToBrokerAssocDict = new Dictionary<MetaBrokerType, List<string>>()
    {
      { MetaBrokerType.Assay, new List<string> {"ASSAY_"}},
			{ MetaBrokerType.Annotation, new List<string> {"ANNOTATION_"}},
			{ MetaBrokerType.CalcField, new List<string> {"CALCFIELD_"}},
			{ MetaBrokerType.SpotfireLink, new List<string> {"SPOTFIRELINK_"}},
		};

		/// <summary>
		/// Get the table name prefix, if any, for specified broker type
		/// </summary>
		/// <param name="mbType"></param>
		/// <returns></returns>

		public static string GetPreferredPrefixForBrokerType(
			MetaBrokerType mbType)
		{
			if (MetaTableNamePrefixToBrokerAssocDict.ContainsKey(mbType))
				return MetaTableNamePrefixToBrokerAssocDict[mbType][0]; // return first prefix as preferred

			else return "";
		}
	}

/// <summary>
/// Supported MetaBroker types
/// </summary>

	public enum MetaBrokerType
	{
		Unknown = 0, // unknown type
		Generic = 1, // basic generic broker
		Assay = 2, // generic assay metabroker
		Annotation = 4, // annotation table data
		CalcField = 5, // calculated field

		Pivot = 11, // generic pivot broker
		MultiTable = 12, // metatable based on multiple underlying tables
		CalcTable = 13, // Table containing multiple calculated fields
		TargetAssay = 14, // retrieves and summarizes data by target and assay
		UnpivotedAssay = 15, // search unpivoted assay results across databases
		RgroupDecomp = 16, // r-group decomposition table
		SpotfireLink = 17, // Link to Spotfire analysis

		NoSql = 18, // broker that doesn't depend on SQL (exclusively) for data search and retrieval
		EmbeddedData = 18, // (replaced by NoSQL, remove when PRD metadata updated)

		MaxBrokerType = 18
	}

	/// <summary>
	/// Summarization level for substance
	/// </summary>

	public enum SubstanceSummarizationLevel
	{
		Unknown = 0,
		None = 1,
		Sample = 2,
		Lot = 3,
		Compound = 4
	}

	/// <summary>
	/// Parameters for importing data from a file
	/// </summary>

	public class UserDataImportParms
	{
		public string FileName = "";
		public char Delim = ' ';
		public bool MultDelimsAsSingle = false;
		public char TextQualifier = ' ';
		public bool FirstLineHeaders = false;
		public bool DeleteExisting = false;
		public bool DeleteDataOnly = false; // if true keep table structure if DeleteExisting is true 
		public bool ImportInBackground = false;
		public bool CheckForFileUpdates = false; // check for updated client file & update associated data 
		public DateTime ClientFileModified; // last time modified, used for autoupdate check
		public List<MetaColumnType> Types; // column types (not persisted)
		public List<string> Labels; // column headers (not persisted)
		public Dictionary<string, MetaColumn> Fc2Mc; // field column position/name to MetaColumn map (not persisted)

		/// <summary>
		/// Clone import parameters
		/// </summary>
		/// <returns></returns>
		
		public UserDataImportParms Clone()
		{
			UserDataImportParms ip = (UserDataImportParms)this.MemberwiseClone();
			return ip;
		}

	}

	/// <summary>
	/// Stats for a metatable
	/// </summary>

	public class MetaTableStats
	{
		public long RowCount; // number of rows in table
		public DateTime UpdateDateTime = DateTime.MinValue; // most-recent update date for table
	}

}
