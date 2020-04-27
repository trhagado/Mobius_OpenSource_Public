using Mobius.ComOps;

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Mobius.Data
{

	/// <summary>
	/// MetaColumns for a MetaTable have the members defined below
	/// </summary>

	public class MetaColumn
	{
		public int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		public string Name = ""; // name of column
		public int Position = 0; // position within table
		public MetaTable MetaTable; // ref to assoc metatable
		public string Label = ""; // label 
		public string ShortLabel = ""; // shorter label
		public string LabelImage = ""; // structure or ref to image to put in label
		public MetaColumnType DataType; // Mobius data type
		public string DataTypeImageName = ""; // 16x16 bitmap for type that overrides default
		public MetaColumnStorageType StorageType; // associated type in underlying database
		public int KeyPosition = -1; // position of this column in primary key for the metatable
		public List<string> PivotValues = null; // values that pivot column must have to pivot this column from source table
		public string ResultCode = ""; // pivot value (code) identifying specific measurement, endpoint etc (may be multiple comma-separated codes)
		public string ResultCode2 = ""; // secondary pivot value (code) (e.g. code for associated summary column)
		public bool UnsummarizedExists = true; // true if exists in database in unsummarized form
		public bool SummarizedExists = false; // true if exists in database in summarized form
		public SummarizationRole SummarizationRole; // summarization role - SummarizationRole.Independent, DEPENDENT, etc. 
		public bool PrimaryResult = false; // if true this is a/the primary result for the metatable
		public bool SecondaryResult = false; // if true this is a/the secondary result for the metatable
		public bool SinglePoint = false; // true single point (SP) result
		public bool Concentration = false; // true if concentration column for a single point (concentration) result
		public bool MultiPoint = false; // true if multi point (CRC, IC50 etc.) result
		public bool DetailsAvailable = false; // can drill down on this column (e.g. IC50 -> %Inh)
		public string Dictionary = ""; // dictionary values come from
		public bool DictionaryMultipleSelect = false; // if true allow selection of multiple dictionary items
		public string Description = ""; // column description
		public string Units = ""; // string for units
		public ColumnSelectionEnum InitialSelection = ColumnSelectionEnum.Selected; // initial selection/visibility state for column
		public ColumnFormatEnum Format = ColumnFormatEnum.Default; // output format
		public float Width = 0; // output width in "characters"
		public int Decimals = 0; // number of decimal positions to display
		public HorizontalAlignmentEx HorizontalAlignment = HorizontalAlignmentEx.Default; // horizontal alignment within cell
		public VerticalAlignmentEx VerticalAlignment = VerticalAlignmentEx.Top; // vertical alignment within cell
		public string CondFormatName = ""; // name of any associated default conditional formatting
		public ColumnTextCaseEnum TextCase = ColumnTextCaseEnum.Unknown; // case character data is stored in see CASE_xxx
		public bool IgnoreCase = true; // ignore case if text string
		public bool IsSearchable = true; // searching allowed for column
		public SearchMethods SearchMethods = SearchMethods.Unknown; // search engine methods for the column
		public bool BrokerFiltering = false; // if true specific broker does filtering of data
		public string InitialCriteria = ""; // any initial criteria for column
		public string TableMap = ""; // source table for column
		public string ColumnMap = ""; // source table column
		public string DataTransform = ""; // transformation to be applied to data before formatting
		public MetaBrokerType MetaBrokerType = MetaBrokerType.Unknown; // metabroker type specific to this column differing from table broker
		public string ClickFunction = ""; // function to execute when item clicked in grid
		public string ImportFilePosition = ""; // position in file for loading (ordinal or field name)

		public static HorizontalAlignmentEx SessionDefaultHAlignment = HorizontalAlignmentEx.Default; // default alignment for this session/user
		public static VerticalAlignmentEx SessionDefaultVAlignment = VerticalAlignmentEx.Top;

		/// <summary>
		/// Default constructor
		/// </summary>

		public MetaColumn()
		{
			return;
		}

		/// <summary>
		/// Constructor with metatable supplied
		/// </summary>

		public MetaColumn(
			MetaTable mt)
		{
			MetaTable = mt;
			mt.AddMetaColumn(this);
		}

		/// <summary>
		/// Constructor with initial type
		/// </summary>
		/// <param name="dataType"></param>
		public MetaColumn (
			MetaColumnType dataType)
		{
			DataType = dataType;
			SetDefaultFormating();
			return;
		}

		/// <summary>
		/// Constructor with initial values
		/// </summary>
		/// <param name="name"></param>
		/// <param name="label"></param>
		/// <param name="dataType"></param>
		/// <param name="displayLevel"></param>
		/// <param name="displayWidth"></param>

		public MetaColumn(
			string name,
			string label,
			MetaColumnType dataType,
			ColumnSelectionEnum displayLevel,
			float displayWidth)
		{
			Name = name.Trim().ToUpper();
			Label = label;
			DataType = dataType;

			SetDefaultFormating();

			if (displayWidth > 0)
				Width = displayWidth;

			InitialSelection = displayLevel;

			return;
		}

		/// <summary>
		/// Return true if key column
		/// </summary>

		public bool IsKey
		{
			get
			{
				if (KeyPosition >= 0) return true;
				if (MetaTable == null) return false;
				MetaColumn mc = MetaTable.KeyMetaColumn;
				if (mc == this || mc.Name == this.Name)
					return true;
				else return false;
			}

			set
			{
				KeyPosition = 0;
				return;
			}
		}

		/// <summary>
		/// Return true if column is a "qualitative/categorical" type (i.e. not numeric or a date)
		/// </summary>

		public bool IsQualitative
		{ get { return !IsContinuous; } }

		/// <summary>
		/// Return true if column is a "continuous" type (i.e. numeric or a date)
		/// </summary>
		/// 
		public bool IsContinuous
		{
			get
			{
				if ((IsNumeric && DataType != MetaColumnType.CompoundId) ||
				 DataType == MetaColumnType.Date)
					return true;

				else return false;
			}
		}

		/// <summary>
		/// Return true if column is a numeric data type
		/// </summary>

		public bool IsNumeric
		{
			get
			{
				if (IsNumericMetaColumnType(DataType)) return true;

				switch (this.StorageType)
				{
					case MetaColumnStorageType.Integer:
					case MetaColumnStorageType.Decimal:
						return true;
				}

				if (this.DataType == MetaColumnType.CompoundId)
				{
					MetaTable root = this.MetaTable.Root;
					RootTable rti = RootTable.GetFromTableName(root.Name);

					if (rti != null) return !rti.IsStringType;

					else if (MetaTable.IsUserDatabaseStructureTableName(root.Name))
						return false; // user databases store compound ids in string fields

					else return false; // throw new Exception("Can't identify key column data type");
				}

				else return false;
			}
		}

		/// <summary>
		/// All MetaColumn Types
		/// </summary>

		public static List<MetaColumnType> AllMetaColumnTypes =
			new List<MetaColumnType>((MetaColumnType[])Enum.GetValues(typeof(MetaColumnType)));

		/// <summary>
		/// Return true if numeric metacolumn type.
		/// Note: Can't properly determine for CompoundId type
		/// </summary>
		/// <param name="mcType"></param>
		/// <returns></returns>

		public static bool IsNumericMetaColumnType(
			MetaColumnType mcType)
		{
			return NumericMetaColumnTypes.Contains(mcType);
		}

		/// <summary>
		/// List of numeric MetaColumnTypes
		/// </summary>

		public static List<MetaColumnType> NumericMetaColumnTypes = new List<MetaColumnType>() {
			MetaColumnType.Number,
			MetaColumnType.Integer,
			MetaColumnType.QualifiedNo };

		/// <summary>
		/// Return true if decimal type
		/// </summary>

		public bool IsDecimal
		{
			get { return IsDecimalMetaColumnType(DataType); }
		}

		/// <summary>
		/// Return true if decimal type
		/// </summary>
		/// <param name="mcType"></param>
		/// <returns></returns>

		public static bool IsDecimalMetaColumnType(
			MetaColumnType mcType)
		{
			if (mcType == MetaColumnType.Number ||
				mcType == MetaColumnType.QualifiedNo)
				return true;

			else return false;
		}

		public bool IsReal
		{
			get { return IsRealMetaColumnType(DataType); }
		}

		/// <summary>
		/// Return true if decimal type
		/// </summary>
		/// <param name="mcType"></param>
		/// <returns></returns>

		public static bool IsRealMetaColumnType(
			MetaColumnType mcType)
		{
			if (mcType == MetaColumnType.Number ||
				mcType == MetaColumnType.QualifiedNo)
				return true;

			else return false;
		}

		public bool IsInteger
		{
			get
			{
				if (DataType == MetaColumnType.Integer) return true;

				switch (this.StorageType)
				{
					case MetaColumnStorageType.Integer:
						return true;

					case MetaColumnStorageType.String:
					case MetaColumnStorageType.Clob:
						return false;
				}

				if (this.DataType == MetaColumnType.CompoundId)
				{
					MetaTable root = this.MetaTable.Root;
					if (root == null) return false;

					RootTable rti = RootTable.GetFromTableName(root.Name);

					if (rti != null) return !rti.IsStringType;

					else if (MetaTable.IsUserDatabaseStructureTableName(root.Name))
						return false; // user databases store compound ids in string fields

					else return false; // throw new Exception("Can't identify key column data type");
				}

				else return false;
			}
		}

		/// <summary>
		/// Return true if string type
		/// </summary>

		public bool IsString
		{
			get
			{
				if (DataType == MetaColumnType.String) return true;

				switch (this.StorageType)
				{
					case MetaColumnStorageType.String:
					case MetaColumnStorageType.Clob:
						return true;

					case MetaColumnStorageType.Integer:
						return false;
				}

				if (this.DataType == MetaColumnType.CompoundId)
				{
					MetaTable root = this.MetaTable.Root;
					if (root == null) return false;

					RootTable rti = RootTable.GetFromTableName(root.Name);

					if (rti != null) return rti.IsStringType;

					else if (MetaTable.IsUserDatabaseStructureTableName(root.Name))
						return true; // user databases store compound ids in string fields

					else return false; // throw new Exception("Can't identify key column data type");
				}

				else return false;
			}
		}


		/// <summary>
		/// Return true if date type
		/// </summary>

		public bool IsDate
		{
			get { return DataType == MetaColumnType.Date; }
		}

		/// <summary>
		/// Return true if this is a structure database set column
		/// </summary>

		public bool IsDatabaseSetColumn
		{
			get
			{
				return RootTable.IsDatabaseListDictionaryName(Dictionary);
				//return RootTable.IsRootTableDictionaryName(Dictionary);
			}
		}

		/// <summary>
		/// Return true if column is searchable by Cartridge
		/// </summary>

		public bool IsChemistryCartridgeSearchable
		{
			get
			{
				if (FlagHelper.IsSet(SearchMethods, SearchMethods.Cartridge))
					return true;

				else if (IsMetatableCartridgeSearchable()) return true;

				else return false;
			}

			set
			{
				FlagHelper.Set(ref SearchMethods, SearchMethods.Cartridge, value);
			}
		}

		/// <summary>
		/// Return true if table contains a chemical cartridge searchable structure column
		/// </summary>

		public bool IsMetatableCartridgeSearchable()
		{
			MetaTable mt = MetaTable;
			if (mt == null || Lex.IsUndefined(mt.Name)) return false;

			MetaColumn smc = mt.FirstStructureMetaColumn;
			if (smc != this) return false;
			if (!smc.IsSearchable) return false;

			if (mt.IsUserDatabaseStructureTable) return true;
			if (MetaBrokerType == MetaBrokerType.Annotation) return true; // structure in annotation table

			RootTable rt = RootTable.GetFromTableName(mt.Name);
			if (rt != null && Lex.Eq(rt.MetaTableName, mt.Name) && rt.CartridgeSearchable)
				return true;

			else return false;
		}

		/// <summary>
		/// Return true if column is searchable by SmallWorld
		/// </summary>

		public bool IsSmallWorldSearchable
		{
			get
			{
				if (FlagHelper.IsSet(SearchMethods, SearchMethods.SmallWorld))
					return true;

				else if (IsMetaTableSmallWorldSearchable()) return true;

				else return false;
			}

			set
			{
				FlagHelper.Set(ref SearchMethods, SearchMethods.SmallWorld, value);
			}
		}

		/// <summary>
		/// Return true if table is SmallWorld searchable
		/// </summary>

		public bool IsMetaTableSmallWorldSearchable()
		{
			MetaTable mt = MetaTable;
			if (mt == null || Lex.IsUndefined(mt.Name)) return false;

			MetaColumn smc = mt.FirstStructureMetaColumn;
			if (smc != this) return false;
			if (!smc.IsSearchable) return false;

			if (Lex.Eq(Name, MetaTable.SmallWorldMetaTableName)) return true;

			RootTable rt = RootTable.GetFromTableName(mt.Name);
			if (rt != null && Lex.Eq(rt.MetaTableName, mt.Name) && Lex.IsDefined(rt.SmallWorldDbName))
				return true;

			else return false;
		}

		/// <summary>
		/// IsMetaTableEcfpSimilaritySearchable
		/// </summary>
		/// <returns></returns>

		public bool IsMetaTableEcfpSimilaritySearchable()
		{
			MetaTable mt = MetaTable;
			if (mt == null || Lex.IsUndefined(mt.Name)) return false;

			MetaColumn smc = mt.FirstStructureMetaColumn;
			if (smc != this) return false;
			if (!smc.IsSearchable) return false;

			RootTable rt = RootTable.GetFromTableName(mt.Name);
			if (rt != null && Lex.Eq(rt.MetaTableName, mt.Name) && (
				Lex.Eq(mt.Name, "corp_structure") || Lex.Eq(mt.Name, "chembl_structures"))) // todo: move to RootTable field
					return true;

			else return false;
		}

		/// <summary>
		/// Return true if column is sortable
		/// </summary>

		public bool IsSortable
		{
			get
			{
				if (DataType == MetaColumnType.Image || DataType == MetaColumnType.Structure)
					return false;
				else return true;
			}
		}

		/// <summary>
		/// Return true if column normally has a graphical representation
		/// </summary>

		public bool IsGraphical
		{
			get
			{
				if (DataType == MetaColumnType.Image || DataType == MetaColumnType.Structure)
					return true;
				else return false;
			}
		}


		/// <summary>
		/// Return true if user can define formatting on the column
		/// </summary>

		public bool IsFormattable
		{
			get
			{
				if ((IsNumeric && !IsKey) ||
				 DataType == MetaColumnType.String ||
				 DataType == MetaColumnType.Date ||
				 DataType == MetaColumnType.Structure)
					return true;
				else return false;
			}
		}

/// <summary>
/// Return true if column is default format
/// </summary>
		public bool IsDefaultFormat
		{
			get
			{
				ColumnFormatEnum format;
				float width;
				int decimals;

				if (Format == ColumnFormatEnum.Default) return true;

				if (DataType == MetaColumnType.Integer && Format == ColumnFormatEnum.Decimal) return true;

				if (DataType == MetaColumnType.String && Format == ColumnFormatEnum.NormalText) return true;

				if (DataType == MetaColumnType.CompoundId && Format == ColumnFormatEnum.Decimal) return true;

				return false;
			}
		}

		/// <summary>
		/// Return true if column is default display width
		/// </summary>
		public bool IsDefaultWidth
		{
			get { return (Width == GetDefaultDisplayWidth(DataType)); }
		}

		/// <summary>
		/// Return true if supplied code is a valid code for the column
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>

		public bool IsValidResultCode(string code)
		{
			if (Lex.Eq(code, ResultCode)) return true;

			else if (!ResultCode.Contains(",")) return false;

			else if (Lex.StartsWith(ResultCode, code + ",") ||
			 Lex.EndsWith(ResultCode, "," + code) ||
			 Lex.Contains(ResultCode, "," + code + ","))
				return true;

			else return false;
		}

/// <summary>
/// Return true if metacolumns have the same table and column names
/// </summary>
/// <param name="mc2"></param>
/// <returns></returns>

		public bool IsSameNameAs(MetaColumn mc2)
		{
			if (Lex.Eq(this.Name, mc2.Name) &&
			 Lex.Eq(this.MetaTable.Name, mc2.MetaTable.Name))
				return true;

			else return false;
		}

/// <summary>
/// Get first or only result code
/// </summary>
/// <returns></returns>

		public static string FirstResultCode(string codeString	)
		{
			if (!Lex.IsDefined(codeString) || !codeString.Contains(",")) return codeString;
			string[] sl = codeString.Split(',');
			if (sl.Length > 0) return sl[0];
			else return "";
		}

/// <summary>
/// Get array of result codes
/// </summary>
/// <returns></returns>

		public string[] ResultCodeList()
		{
			if (!Lex.IsDefined(ResultCode) || !ResultCode.Contains(",")) return new string[0];
			string[] sl = ResultCode.Split(',');
			return sl;
		}

/// <summary>
/// Return true if compatible metacolumn types
/// Note: Can't properly determine for CompoundId type
/// </summary>
/// <param name="mcType1"></param>
/// <param name="mcType2"></param>
/// <returns></returns>

		public static bool AreCompatibleMetaColumnTypes(
			MetaColumnType mcType1,
			MetaColumnType mcType2)
		{
			if (mcType1 == mcType2) return true;
			else if (IsNumericMetaColumnType(mcType1) && IsNumericMetaColumnType(mcType2))
				return true;
			else return false;
		}

		/// <summary>
		/// Set default output formatting
		/// </summary>
		/// <param name="mc"></param>

		public void SetDefaultFormating()
		{
			GetDefaultFormatting(DataType, out Format, out Width, out Decimals);
			return;
		}

		/// <summary>
		/// Set default output format
		/// </summary>
		/// <param name="type"></param>
		/// <param name="format"></param>
		/// <param name="width"></param>
		/// <param name="decimals"></param>

		public static void GetDefaultFormatting(
		MetaColumnType type,
		out ColumnFormatEnum format,
		out float width,
		out int decimals)
		{
			
			format = ColumnFormatEnum.Default;

			if (type == MetaColumnType.Integer) // display integers as as typical decimal values
				format = ColumnFormatEnum.Decimal;

			width = GetDefaultDisplayWidth(type);
			decimals = 0;

			return;
		}

		/// Predefined CondFormat object retrieved via CondFormatName

		public CondFormat CondFormat // default conditional formatting object
		{
			get
			{
				if (_condFormat != null) return _condFormat;
				if (Lex.IsUndefined(CondFormatName)) return null;

				_condFormat = CondFormat.GetPredefined(CondFormatName);
				return _condFormat;
			}

			set
			{
				_condFormat = value;
			}
		}
		private CondFormat _condFormat = null;

		/// <summary>
		/// Get default display width for a given column type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>

		public static int GetDefaultDisplayWidth(
			MetaColumnType type)
		{

			if (type == MetaColumnType.CompoundId)
				return 12;

			else if (type == MetaColumnType.String ||
				type == MetaColumnType.DictionaryId ||
				type == MetaColumnType.Html ||
				type == MetaColumnType.Hyperlink ||
				type == MetaColumnType.Binary ||
				type == MetaColumnType.MolFormula)
				return 16;

			else if (type == MetaColumnType.QualifiedNo ||
			type == MetaColumnType.Integer ||
			type == MetaColumnType.Number)
				return 10;

			else if (type == MetaColumnType.Date)
				return 10;

			else if (type == MetaColumnType.Structure)
				return 35;

			else return 10;
		}

/// <summary>
/// Get MetaTableName.MetaColumnName name for metacolumn
/// </summary>
/// <returns></returns>

		public string MetaTableDotMetaColumnName
		{
			get
			{
				string name = Name;
				if (MetaTable != null)
					name = MetaTable.Name + "." + name;

				return name;
			}
		}
		/// <summary>
		/// Parse a metaTableName.metaColumnName string and return the associated MetaColumn object
		/// throwing exception if error
		/// </summary>
		/// <param name="tableDotColumnName"></param>
		/// <returns></returns>

		public static MetaColumn ParseMetaTableMetaColumnName(
			string tableDotColumnName)
		{
			try { return ParseMetaTableMetaColumnNameWithException(tableDotColumnName); }
			catch (Exception ex) { return null; }
		}

/// <summary>
/// Parse a metaTableName.metaColumnName string and return the associated MetaColumn object
/// throwing exception if error
/// </summary>
/// <param name="tableDotColumnName"></param>
/// <returns></returns>

		public static MetaColumn ParseMetaTableMetaColumnNameWithException(
			string tableDotColumnName)
		{
			string[] sa = tableDotColumnName.Split('.');
			if (sa.Length < 2) throw new Exception("Invalid name");
			MetaTable mt = MetaTableCollection.GetWithException(sa[0]);
			MetaColumn mc = mt.GetMetaColumnByNameWithException(sa[1]);
			return mc;
		}

/// <summary>
/// Convert a string to a MetaColumnType
/// </summary>
/// <param name="typeString"></param>
/// <returns></returns>

		public static MetaColumnType ParseMetaColumnTypeString(
			string typeString)
		{
			MetaColumnType mcType;

			if (EnumUtil.TryParse(typeString, out mcType))
				return mcType;

			if (Lex.Eq(typeString, "Integer") ||
				Lex.Eq(typeString, "Int"))
				mcType = MetaColumnType.Integer;
			else if (Lex.Eq(typeString, "Number") ||
			 Lex.Eq(typeString, "Float"))
				mcType = MetaColumnType.Number;
			else if (Lex.Eq(typeString, "String") ||
				Lex.Eq(typeString, "Text"))
				mcType = MetaColumnType.String;
			else if (Lex.Eq(typeString, "Date"))
				mcType = MetaColumnType.Date;
			else if (Lex.Eq(typeString, "CompoundId") ||
				Lex.Eq(typeString, "CmpdId") ||
				Lex.Eq(typeString, "CompoundNumber") ||
				Lex.Eq(typeString, "CmpdNo") ||
				Lex.Eq(typeString, "Key")) // move key to separate type later
				mcType = MetaColumnType.CompoundId;
			else if (Lex.Eq(typeString, "QualifiedNumber") ||
				Lex.Eq(typeString, "QualifiedNo"))
				mcType = MetaColumnType.QualifiedNo;

			else if (Lex.Eq(typeString, "Binary"))
				mcType = MetaColumnType.Binary;

			else if (Lex.Eq(typeString, "Structure"))
				mcType = MetaColumnType.Structure;

			else if (Lex.Eq(typeString, "MolFile")) // structure stored as molfile string
				mcType = MetaColumnType.Structure;

			else if (Lex.Eq(typeString, "Smiles")) // structure stored as smiles string
				mcType = MetaColumnType.Structure;

			else if (Lex.Eq(typeString, "Chime")) // structure stored as chime string
				mcType = MetaColumnType.Structure;

			else if (Lex.Eq(typeString, "MolFormula"))
				mcType = MetaColumnType.MolFormula;

			else if (Lex.Eq(typeString, "ImageId") || Lex.Eq(typeString, "Image") ||
			 Lex.Eq(typeString, "GraphId") || Lex.Eq(typeString, "Graph"))
				mcType = MetaColumnType.Image;

			else if (Lex.Eq(typeString, "DictionaryId") || Lex.Eq(typeString, "Dictionary"))
				mcType = MetaColumnType.DictionaryId;

			else if (Lex.Eq(typeString, "Hyperlink") ||
			 Lex.Eq(typeString, "URL") ||
			 Lex.Eq(typeString, "URI"))
				mcType = MetaColumnType.Hyperlink;

			else if (Lex.Eq(typeString, "HTML"))
				mcType = MetaColumnType.Html;

			else if (Lex.Eq(typeString, "DbSet") || // (Deprecated)
				Lex.Eq(typeString, "DbLink"))
				mcType = MetaColumnType.String;

			else mcType = MetaColumnType.Unknown; // no luck

			return mcType;
		}

/// <summary>
/// Parse a MetaColumn storage type string
/// </summary>
/// <param name="txt"></param>
/// <returns></returns>

		public static MetaColumnStorageType ParseStorageTypeString(string txt)
		{
			if (Lex.Eq(txt, "Integer") ||
				Lex.Eq(txt, "Int") ||
				Lex.Eq(txt, "Int16") ||
				Lex.Eq(txt, "Int32"))
				return MetaColumnStorageType.Integer;

			else if (Lex.Eq(txt, "Int64"))
				return MetaColumnStorageType.Int64;

			else if (Lex.Eq(txt, "Number") ||
			 Lex.Eq(txt, "Float") ||
			 Lex.Eq(txt, "Single") ||
			 Lex.Eq(txt, "Double") ||
			 Lex.Eq(txt, "Byte") ||
			 Lex.Eq(txt, "Decimal"))
				return MetaColumnStorageType.Decimal;

			else if (Lex.Eq(txt, "String") ||
				Lex.Eq(txt, "Char") ||
				Lex.Eq(txt, "VarChar2"))
				return MetaColumnStorageType.String;

			else if (Lex.Eq(txt, "Date") ||
				Lex.Eq(txt, "TimeStamp"))
				return MetaColumnStorageType.DateTime;

			else if (Lex.Eq(txt, "Clob"))
				return MetaColumnStorageType.Clob;

			else if (Lex.Eq(txt, "Blob"))
				return MetaColumnStorageType.Blob;

			else if (Lex.Eq(txt, "BFile"))
				return MetaColumnStorageType.BFile;

			else if (Lex.Eq(txt, "Object"))
				return MetaColumnStorageType.Object;

			else
				throw new Exception("Unexpected MetaColumn Storage Type: " + txt);
		}

		/// <summary>
		/// Get best label for a metacolumn (not currently in use)
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>

		public string GetBestFieldLabel()
		{
			string label;
			int i1;

			MetaColumn mc = this;

			if (mc.Label == null || mc.Label == "") label = IdToLabel(mc.Name);

			else
			{
				label = mc.Label;
				if (mc.Units != "" && !Lex.Contains(label, mc.Units)) label += " (" + mc.Units + ")";
				//label = mc.Label.Replace("- ", ""); // remove explicit hypenation
			}

			return label;
		}

		/// <summary>
		/// Convert an Id to a better looking label
		/// </summary>

		public static string IdToLabel(
			string id)
		{
			char pc, cc;
			int i1;

			StringBuilder label = new StringBuilder(id);
			pc = ' '; // prev char
			for (i1 = 0; i1 < label.Length; i1++)
			{
				cc = label[i1];
				if (Char.IsLetter(cc))
				{
					if (pc == ' ') label[i1] = Char.ToUpper(cc); // cap first char
					else label[i1] = Char.ToLower(cc);
				}
				else if (cc == '_')
				{ // treat underscore like space
					label[i1] = ' ';
					cc = ' ';
				}
				pc = cc; // 
			}
			return label.ToString();
		}

/// <summary>
/// Get the position of a metacolumn in its metatable
/// </summary>
/// <returns></returns>

		public int GetPositionInMetaTable()
		{
			if (MetaTable == null) throw new Exception("MetaTable not defined");
			int pos = MetaTable.GetMetaColumnIndexByName(Name);
			if (pos < 0) throw new Exception("MetaColumn " + Name + " not found in table " + MetaTable.Name);
			return pos;
		}

/// <summary>
/// Get the index of the image for the data type
/// </summary>

		public Bitmaps16x16Enum DataTypeImageIndex
		{
			get
			{
				Bitmaps16x16Enum typeImg = Bitmaps16x16Enum.None;
				if (!Lex.IsNullOrEmpty(DataTypeImageName))
				{
					if (EnumUtil.TryParse(DataTypeImageName, out typeImg))
						return typeImg;
				}

				return GetMetaColumnDataTypeImageIndex(DataType);
			}
		}

		/// <summary>
		/// Get the image index for the specified metacolumn type
		/// </summary>
		/// <param name="mcType"></param>
		/// <returns></returns>

		public static Bitmaps16x16Enum GetMetaColumnDataTypeImageIndex(MetaColumnType mcType)
		{
			if (mcType == MetaColumnType.Integer) return Bitmaps16x16Enum.Number;
			else if (mcType == MetaColumnType.Number) return Bitmaps16x16Enum.Number;
			else if (mcType == MetaColumnType.QualifiedNo) return Bitmaps16x16Enum.QualNumber;
			else if (mcType == MetaColumnType.String) return Bitmaps16x16Enum.Text;
			else if (mcType == MetaColumnType.Date) return Bitmaps16x16Enum.Date;
			else if (mcType == MetaColumnType.Binary) return Bitmaps16x16Enum.Document;
			else if (mcType == MetaColumnType.CompoundId) return Bitmaps16x16Enum.QualNumber;
			else if (mcType == MetaColumnType.Structure) return Bitmaps16x16Enum.Structure;
			else if (mcType == MetaColumnType.MolFormula) return Bitmaps16x16Enum.Text;
			else if (mcType == MetaColumnType.Image) return Bitmaps16x16Enum.Graph;
			else if (mcType == MetaColumnType.DictionaryId) return Bitmaps16x16Enum.Text;
			else if (mcType == MetaColumnType.Hyperlink) return Bitmaps16x16Enum.URL;
			else if (mcType == MetaColumnType.Html) return Bitmaps16x16Enum.URL;
			else if (mcType == MetaColumnType.Unknown) return Bitmaps16x16Enum.Unknown;
			else if (mcType == MetaColumnType.Any) return Bitmaps16x16Enum.CidList;

			else return Bitmaps16x16Enum.Unknown;
		}

/// <summary>
/// Set table and column maps 
/// </summary>
/// <param name="tableMap"></param>
/// <param name="columnMap"></param>
/// 
		public void SetMaps(
			string tableMap,
			string columnMap)
		{
			TableMap = tableMap;
			ColumnMap = columnMap;
		}

/// <summary>
/// Clone metacolumn
/// </summary>
/// <returns></returns>

		public MetaColumn Clone ()
		{
			MetaColumn mc = (MetaColumn)this.MemberwiseClone();
			if (mc.PivotValues != null) mc.PivotValues = new List<string>(this.PivotValues);
			return mc;
		}

/// <summary>
/// Display name and label for convenience in debug view
/// </summary>
/// <returns></returns>

		public override string ToString()
		{
			return Name + ", " + Label;
		}

	}

/// <summary>
/// Enum for DataType
/// </summary>

	public enum MetaColumnType
	{
		Unknown = 0, // no type assigned
		Integer = 1, // integer
		Number = 2, // double floating point number
		QualifiedNo = 3, // qualified number (e.g. > 50)
		String = 4, // string
		Date = 5, // date
		Binary = 6, // variable length binary data

		CompoundId = 10, // compound id
		Structure = 11, // molecule
		MolFormula = 12, // mol formula
		Image = 13, // image
		DictionaryId = 14, // numeric dictionary id
		Hyperlink = 15, // link to web content
		Html = 16, // html text

		Any = 999 // any type allowed 
	}

	/// <summary>
	/// Database storage type for column
	/// </summary>

	public enum MetaColumnStorageType
	{
		Unknown  = 0,
		Integer  = 1,
		Int64    = 2, 
		Decimal  = 3,
		String   = 4,
		DateTime = 5,
		Clob     = 6,
		Blob     = 7,
		BFile    = 8,
		Object   = 9
	}

	/// <summary>
	/// SummarizationRole enum
	/// </summary>

	public enum SummarizationRole
	{
		Unknown = 0,
		Independent = 1,
		Dependent = 2,
		Derived = 3, // e.g. IC50 derived from %inh values
		Summarized = 4,
		Misc = 5
	}

/// <summary>
/// Initial selection state for a MetaColumn
/// </summary>

	public enum ColumnSelectionEnum
	{
		Unknown = 0,
		Selected = 1,
		Unselected = 2,
		Unselectable = 3,
		Hidden = 4,
		Comment = 5 // this item is a comment, not selectable or searchable
	}

/// <summary>
/// Values for textCase
/// </summary>

	public enum ColumnTextCaseEnum
	{
		Unknown = 0,
		Lower = 1,
		Upper = 2,
		Mixed = 3
	}

	// Value formatting enums

	public enum ColumnFormatEnum
	{
		Unknown = 0,
		Default = 1,
		Decimal = 2, // fixed number of decimal places
		SigDigits = 3, // display number of significant digits
		Scientific = 4,

		NormalText = 5,
		HtmlText = 6
	}

	// Qualified number formatting enum


	[Flags]
	public enum QnfEnum
	{
		Undefined = 0, // not defined

		Split = 1, // split into multiple columns as defined below
		Combined = 2, // output in combined format

		Qualifier = 4, // output qualifier
		NumericValue = 8, // output number
		StdDev = 16, // output standard deviation
		StdErr = 32, // output standard error
		NValue = 64, // output n-value
		NValueTested = 128,  // output number tested
		TextValue = 256, // output any text comment

		SubfieldMask = Qualifier | NumericValue | StdDev | StdErr | NValue | NValueTested | TextValue, // mask for subfield bits

		DisplayStdDevLabel = 1024, // display "SD=" label when formatting parenthetically
		DisplayStdErrLabel = 2048, // display "SE=" label when formatting parenthetically
		DisplayNLabel = 4096, // display "n=" label when formatting parenthetically

		FormatMask = DisplayStdDevLabel | DisplayStdErrLabel | DisplayNLabel // mask for formatting bits
	}

	/// <summary>
	/// Selected QueryColumn with or without an indidual subcolumn
	/// </summary>

	public class SelectedQueryColumn
	{
		QueryColumn Column = null;
		QnfEnum Subcolumn = QnfEnum.Combined;
	}

	/// <summary>
	/// Utility methods on Qualified number subcolumns and formatting
	/// </summary>

	public class QnSubcolumns
	{

		/// <summary>
		/// Return the scalar data type of the specified subcolumn of a QualifiedNumber
		/// </summary>
		/// <param name="qnf"></param>
		/// <returns></returns>

		public static MetaColumnType GetMetaColumnType (QnfEnum qnf)
		{
			if (IsCombinedFormat(qnf)) return MetaColumnType.String;

			else if (QualifierIsSet(qnf))
				return MetaColumnType.String;

			else if (NumericValueIsSet(qnf))
			return MetaColumnType.Number;

			else if (StdDevIsSet(qnf))
			return MetaColumnType.Number;

			else if (StdErrIsSet(qnf))
			return MetaColumnType.Number;

			else if (NValueIsSet(qnf))
			return MetaColumnType.Integer;

			else if (NValueTestedIsSet(qnf))
			return MetaColumnType.Integer;

			else return MetaColumnType.String;
		}

		/// <summary>
		/// Return true if a combined format that outputs a string value
		/// </summary>
		/// <param name="qnFormat"></param>
		/// <returns></returns>

		public static bool IsCombinedFormat(QnfEnum qnFormat) 
		{
			return ((qnFormat & QnfEnum.Combined) != 0 || qnFormat == 0);
		}

/// <summary>
/// Return true if a split format that outputs one or more subfields from a QualifiedNumber 
/// </summary>
/// <param name="qnFormat"></param>
/// <returns></returns>
		public static bool IsSplitFormat(QnfEnum qnFormat)
		{
			return ((qnFormat & QnfEnum.Split) != 0);
		}

		public static bool QualifierIsSet(QnfEnum qnFormat)
		{
			return ((qnFormat & QnfEnum.Qualifier) != 0);
		}

		public static bool NumericValueIsSet(QnfEnum qnFormat)
		{
			return ((qnFormat & QnfEnum.NumericValue) != 0);
		}

		public static bool StdDevIsSet(QnfEnum qnFormat)
		{
			return ((qnFormat & QnfEnum.StdDev) != 0);
		}

		public static bool StdErrIsSet(QnfEnum qnFormat)
		{
			return ((qnFormat & QnfEnum.StdErr) != 0);
		}

		public static bool NValueIsSet(QnfEnum qnFormat)
		{
			return ((qnFormat & QnfEnum.NValue) != 0);
		}

		public static bool NValueTestedIsSet(QnfEnum qnFormat)
		{
			return ((qnFormat & QnfEnum.NValueTested) != 0);
		}

		public static bool TextValueIsSet(QnfEnum qnFormat)
		{
			return ((qnFormat & QnfEnum.TextValue) != 0);
		}
	}

	/// <summary>
	/// Search methods for column
	/// </summary>

	[Flags]
	public enum SearchMethods
	{
		Unknown = 0, // not defined

		Oracle = 1, // standard Oracle searching
		ODBC = 2, // ODBC interface
		Cartridge = 4, // chemical cartridge structure searching is available
		SmallWorld = 8 // can search with SmallWorld
	}

}
