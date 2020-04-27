
using Mobius.SpotfireComOps;

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text;

namespace Mobius.SpotfireDocument
{

	/// <summary>
	/// DataColumnMsx
	/// </summary>

	public class DataColumnMsx : NodeMsx
	{
		public string Name = ""; // Spotfire data column name

		public string MobiusRole = ""; // Role of column as defined by original name of column in template (Mobius-added property)

		public string MobiusFileColumnName = ""; // Name of column in source file. Either MetaTable.MetaColumn name or "None 1", "None 2" ...

		public string ExternalName = ""; // Spotfire-controlled property that is name of the column as imported from a data source.
																		 // Used by Spotfire allow column matching for linked data and add/replace data operations
																		 // Once this is set for a column it does not change even if data is replaced from another source using a different external name

		public string Origin = "";      // A text string that describes where the column comes from. Typically set by a data source, or by a tool for a result column.
																		// For columns coming from files this is the name of the file, not including the extension.
																		// For rename operations contains the old column name.

		public string ContentType = ""; // See coments below
																		// Each column may have a specified content type.
																		// Renderers use this property as input to know what to display. 
																		// Use the form toplevel/subtype, for example, text/plain or image/jpg.
																		//
																		// For Geometry columns the content type should be set to application/x-wkb 
																		// if you want to show the geometry information as images.
																		//
																		// If you are using TIBCO Spotfire Lead Discovery to display chemical structures 
																		// from an SDFile then the content type should be set to chemical/x-mdl-molfile for the molfile column.
																		// Other structure formats are: chemical/x-mdl-chime and chemical/x-daylight-smiles

		public DataTypeMsxEnum DataType = DataTypeMsxEnum.Undefined;

		public DataTableMsx DataTable => GetAncestor<DataTableMsx>();

		/// <summary>
		/// Get serializable reference name from instance
		/// </summary>
		/// <param name="dc"></param>
		/// <returns></returns>

		public static string GetReferenceId(DataColumnMsx dc)
		{
			if (dc != null) return dc.ReferenceId;
			else return null;
		}

		public string ReferenceId => DataTable.Id + "." + Name;

		/// <summary>
		/// Get instance from reference id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>

		public static DataColumnMsx GetInstanceFromReferenceId(
			DocumentMsx doc,
			string id)
		{
			DataColumnMsx dc;

			if (String.IsNullOrWhiteSpace(id)) return null;

			int i1 = id.IndexOf(".");
			if (i1 < 0) return null;
			string tableId = id.Substring(0, i1);
			string columnName = id.Substring(i1 + 1);

			DataTableMsx dt = DataTableMsx.GetInstanceFromReferenceId(doc, tableId);
			if (dt == null) return null;

			if (dt.TryGetColumnByName(columnName, out dc))
				return dc;

			else return null; // (throw exception?) 
		}

		/// <summary>
		/// Return true if Spotfire column is a chemical structure
		/// e.g. ContentType = chemical/x-mdl-molfile, chemical/x-mdl-chime or chemical/x-daylight-smiles
		/// </summary>
		/// <returns></returns>

		public bool IsStructureColumn
		{
			get
			{
				if (String.IsNullOrWhiteSpace(ContentType)) return false;

				if (ContentType.IndexOf("chemical/x-", StringComparison.OrdinalIgnoreCase) >= 0) return true;
				else return false;
			}
		}

		public bool IsReal => DataTypeMsx.IsNumeric(DataType);

		public bool IsNumeric => DataTypeMsx.IsNumeric(DataType);

		public bool IsInteger => DataTypeMsx.IsInteger(DataType);

		public bool IsString => DataTypeMsx.IsString(DataType);

		public bool IsDateTime => DataTypeMsx.IsDateTime(DataType);

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			return; // no secondary references
		}

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			return; // no secondary references
		}

		public override string ToString()
		{
			string txt = String.Format("DataColumnMsx - SpotfireName: {0} ({1}), ExtName: {2}, MxRole: {3}, MxName: {4}",
				Name, DataType, ExternalName, MobiusRole, MobiusFileColumnName);
			return txt;
		}

	}

	/// <summary>
	/// DataTypeMsx
	/// </summary>

	public class DataTypeMsx
	{

		public static bool TryGetFromName(string name, out DataTypeMsxEnum dataType)
		{
			if (Enum.TryParse<DataTypeMsxEnum>(name, true, out dataType))
				return true;

			else return false;
		}

		public static bool IsNumeric(DataTypeMsxEnum DataType)
		{
			return DataType == DataTypeMsxEnum.Integer || DataType == DataTypeMsxEnum.LongInteger ||
				DataType == DataTypeMsxEnum.Real || DataType == DataTypeMsxEnum.SingleReal ||
				DataType == DataTypeMsxEnum.Currency;
		}

		public static bool IsReal(DataTypeMsxEnum DataType)
		{
			return DataType == DataTypeMsxEnum.Real || DataType == DataTypeMsxEnum.SingleReal;
		}

		public static bool IsInteger(DataTypeMsxEnum DataType)
		{
			return DataType == DataTypeMsxEnum.Integer || DataType == DataTypeMsxEnum.LongInteger;
		}

		public static bool IsString(DataTypeMsxEnum DataType)
		{
			return DataType == DataTypeMsxEnum.String;
		}

		public static bool IsDateTime(DataTypeMsxEnum DataType)
		{
			return DataType == DataTypeMsxEnum.Date || DataType == DataTypeMsxEnum.DateTime;
		}

		public static bool IsTime(DataTypeMsxEnum DataType)
		{
			return DataType == DataTypeMsxEnum.Date || DataType == DataTypeMsxEnum.Time || DataType == DataTypeMsxEnum.DateTime;
		}

		public static bool IsSimple(DataTypeMsxEnum DataType)
		{
			return DataType != DataTypeMsxEnum.Binary && DataType != DataTypeMsxEnum.Undefined;
		}

		public static object DefaultValue(DataTypeMsxEnum DataType) => NullValue(DataType);

		public static object NullValue(DataTypeMsxEnum DataType)
		{
			switch (DataType)
			{
				case DataTypeMsxEnum.String:
					return null;

				case DataTypeMsxEnum.Real:
					return 0.0;

				case DataTypeMsxEnum.Integer:
					return 0;

				case DataTypeMsxEnum.DateTime:
					return new System.DateTime(0L);

				case DataTypeMsxEnum.Date:
					return new System.DateTime(0L);

				case DataTypeMsxEnum.Time:
					return new System.DateTime(0L);

				case DataTypeMsxEnum.Currency:
					return 0.0m;

				case DataTypeMsxEnum.Boolean:
					return false;

				case DataTypeMsxEnum.Binary:
					return null;

				case DataTypeMsxEnum.LongInteger:
					return 0L;

				case DataTypeMsxEnum.TimeSpan:
					return new System.TimeSpan(0L);

				case DataTypeMsxEnum.SingleReal:
					return 0f;


				default:
					return null;
			}
		}

#if false
		public string Name;
		public DataTypeMsxEnum Id = DataTypeMsxEnum.Undefined;

		public DataTypeMsx()
		{
			return;
		}

		public DataTypeMsx(string name, DataTypeMsxEnum id)
		{
			Name = name;
			Id = id;
		}

		public static readonly DataTypeMsx Undefined = new DataTypeMsx("Undefined", DataTypeMsxEnum.Undefined);
		public static readonly DataTypeMsx String = new DataTypeMsx("String", DataTypeMsxEnum.String);
		public static readonly DataTypeMsx Integer = new DataTypeMsx("Integer", DataTypeMsxEnum.Integer);
		public static readonly DataTypeMsx Real = new DataTypeMsx("Real", DataTypeMsxEnum.Real);
		public static readonly DataTypeMsx Date = new DataTypeMsx("Date", DataTypeMsxEnum.Date);
		public static readonly DataTypeMsx DateTime = new DataTypeMsx("DateTime", DataTypeMsxEnum.DateTime);
		public static readonly DataTypeMsx Time = new DataTypeMsx("Time", DataTypeMsxEnum.Time);
		public static readonly DataTypeMsx Currency = new DataTypeMsx("Currency", DataTypeMsxEnum.Currency);
		public static readonly DataTypeMsx Binary = new DataTypeMsx("Binary", DataTypeMsxEnum.Binary);
		public static readonly DataTypeMsx Boolean = new DataTypeMsx("Boolean", DataTypeMsxEnum.Boolean);
		public static readonly DataTypeMsx LongInteger = new DataTypeMsx("LongInteger", DataTypeMsxEnum.LongInteger);
		public static readonly DataTypeMsx TimeSpan = new DataTypeMsx("TimeSpan", DataTypeMsxEnum.TimeSpan);
		public static readonly DataTypeMsx SingleReal = new DataTypeMsx("SingleReal", DataTypeMsxEnum.SingleReal);

		public static IList<DataTypeMsx> AvailableDataTypes
		{
			get
			{
				return new System.Collections.Generic.List<DataTypeMsx>(new DataTypeMsx[]
				{
					DataTypeMsx.String,
					DataTypeMsx.Integer,
					DataTypeMsx.Real,
					DataTypeMsx.Currency,
					DataTypeMsx.Date,
					DataTypeMsx.Time,
					DataTypeMsx.DateTime,
					DataTypeMsx.TimeSpan,
					DataTypeMsx.LongInteger,
					DataTypeMsx.SingleReal,
					DataTypeMsx.Boolean,
					DataTypeMsx.Binary
				});
			}
		}
#endif

#if false
	public static DataTypeMsx EnumToDataType(DataTypeMsxEnum valueType)
		{
			switch (valueType)
			{
				case DataTypeMsxEnum.Integer:
					return DataTypeMsx.Integer;

				case DataTypeMsxEnum.Real:
					return DataTypeMsx.Real;

				case DataTypeMsxEnum.String:
					return DataTypeMsx.String;

				case DataTypeMsxEnum.Boolean:
					return DataTypeMsx.Boolean;

				case DataTypeMsxEnum.LongInteger:
					return DataTypeMsx.LongInteger;

				case DataTypeMsxEnum.SingleReal:
					return DataTypeMsx.SingleReal;

				case DataTypeMsxEnum.Binary:
					return DataTypeMsx.Binary;

				case DataTypeMsxEnum.Currency:
					return DataTypeMsx.Currency;

				case DataTypeMsxEnum.Time:
					return DataTypeMsx.Time;

				case DataTypeMsxEnum.DateTime:
					return DataTypeMsx.DateTime;

				case DataTypeMsxEnum.Date:
					return DataTypeMsx.Date;

				case DataTypeMsxEnum.TimeSpan:
					return DataTypeMsx.TimeSpan;

				default:
					throw new Exception("Invalid DataType: " + valueType);
			}

		}
#endif
	}

	public enum DataTypeMsxEnum // based on CxxDataType enum
	{
		Undefined = -1,
		Integer = 0,
		Real = 1,
		String = 2,
		Boolean = 3,
		LongInteger = 4,
		SingleReal = 5,
		Binary = 6,
		Currency = 7,
		Time = 8,
		DateTime = 9,
		Date = 10,
		TimeSpan = 11
	}

}
