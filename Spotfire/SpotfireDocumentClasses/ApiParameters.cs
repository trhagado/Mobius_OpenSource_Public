using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace Mobius.SpotfireDocument
{

	/// <summary>
	/// Parameters for remapping a DataTable
	/// </summary>

	public class DataTableMapParms
	{
		public string SpotfireTableName = ""; // Spotfire table name
		public string SpotfireTableId = ""; // Guid of table
		public string FileUrl = ""; // URL of file to get

		public List<ColumnMapParms> ColumnMapParmsList;

		public DataTableMapParms()
		{
			return;
		}

		public DataTableMapParms(bool allocateList)
		{
			if (allocateList)
			{
				ColumnMapParmsList = new List<ColumnMapParms>();
			}

			return;
		}

		public override string ToString()
		{
			string txt = "DataTableMapParms - Table: " + SpotfireTableName + ", FilePath: " + FileUrl + "\r\n" +
				"Idx, Current_Name, New_Name, DataType, MobiusRole, MobiusFileColumnName, ExternalName, ContentType\r\n";

			for (int dci = 0; dci < ColumnMapParmsList.Count; dci++)
			{
				ColumnMapParms dc = ColumnMapParmsList[dci];

				txt += String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}\r\n",
					dci, dc.SpotfireColumnName, dc.NewSpotfireColumnName, dc.DataType, dc.MobiusRole, dc.MobiusFileColumnName, dc.ExternalName, dc.ContentType);
			}

			return txt;
		}
	}

	/// <summary>
	/// ColumnMapParms
	/// </summary>

	public class ColumnMapParms
	{
		public string SpotfireColumnName = ""; // Current name for Spotfire column.

		public string NewSpotfireColumnName = ""; // Resulting name for Spotfire column

		public string MobiusRole = ""; // Role of column as defined by original name of column in template (Mobius-added property)

		public string MobiusFileColumnName = ""; // Name of column in source file. Either MetaTable.MetaColumn name or "None 1", "None 2" ...

		public string ExternalName = ""; // Spotfire-controlled property that is name of the column as imported from a data source.

		public string ContentType = ""; // See coments below

		public DataTypeMsxEnum DataType = DataTypeMsxEnum.Undefined;

		[XmlIgnore]
		public static string SpotfireExportExtraColNameSuffix = "_"; // suffix added to name to keep separate

	}

	/// <summary>
	/// Result of methods that make changes to data tables
	/// </summary>

	public class ColumnsChangedResultMsx
	{
		public bool PrimaryKeyMismatch;
		public bool ColumnMismatch;
		public bool NoMatchingColumns;
		public List<string> AddedColumns;
		public List<string> RemovedColumns;
		public List<string> InvalidatedColumns;
		public List<string> InvalidatedHierarchies;
		public List<string[]> NameChanges;
		public List<string[]> TypeChanges;
	}

}
