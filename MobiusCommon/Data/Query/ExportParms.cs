using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;

namespace Mobius.Data
{

	/// <summary>
	/// Export parameters
	/// </summary>

	public class ExportParms
	{
		public int QueryId; // id of query (serialized)
		public OutputDest OutputDestination = OutputDest.Unknown;
		public bool RunInBackground = false; // run query in background mode

		public ExportFileFormat ExportFileFormat; // output file format Csv or Tsv
		public string OutputFileName = ""; // output file name
		public string OutputFileName2 = ""; // secondary output file name
		public bool DuplicateKeyValues; // duplicate key table fields in each row of export output
		public QnfEnum QualifiedNumberSplit = 0; // not defined
		public ExportStructureFormat ExportStructureFormat; // how to output structures
		public MoleculeTransformationFlags StructureFlags; // additional structure output flags
		public bool FixedHeightStructures = false; // if true use constant structure box height
		public ColumnNameFormat ColumnNameFormat = ColumnNameFormat.Normal; // format of table label

		public bool IncludeDataTypes = false; // include Spotfire data types in 2nd line
		public SpotfireOpenModeEnum OpenMode = SpotfireOpenModeEnum.None; // Spotfire open mode
		public bool ViewStructures = false; // if true start structure viewer


		/// <summary>
		/// Serialize
		/// </summary>
		/// <returns></returns>

		public string Serialize()
		{
			string s = XmlUtil.Serialize(this);
			return s;
		}

		/// <summary>
		/// Deserialize
		/// </summary>
		/// <param name="serializedEp"></param>
		/// <returns></returns>

		public static ExportParms Deserialize(
			string serializedEp)
		{
			ExportParms ep = (ExportParms)XmlUtil.Deserialize(serializedEp, typeof(ExportParms));
			return ep;
		}
	}

		/// <summary>
		/// PageOrientation
		/// </summary>

		public enum PageOrientation
	{
		Portrait = 1,
		Landscape = 2
	}

/// <summary>
/// Export file format
/// </summary>

	public enum ExportFileFormat
	{
		Csv = 1, // comma-separated values
		Tsv = 2, // tab-separated value
		Sdf = 3, // SDfile
		Smi = 4,  // Smiles
		Stdf = 5, // Spotfire text data file with col type info
		Sbdf = 6 // Spotfire binary data file with col type info
	}

	/// <summary>
	/// Structure export format
	/// </summary>

	public enum ExportStructureFormat
	{
		Insight = 1, // Insight for Excel
		JChem = 2, // JChem for Excel
		Metafiles = 3, // Metafile
		Molfile = 4,
		Chime = 5,
		Smiles = 6
	}

/// <summary>
/// Mode to open Spotfire Decisionsite
/// </summary>

	public enum SpotfireOpenModeEnum
	{
		None = 0,
		NewInstance = 1,
		ExistingInstance = 2
	}

/// <summary>
/// Format of table labels
/// </summary>

	public enum ColumnNameFormat
	{
		None = 0, // Just the column label without any table label (e.g. "Compound Id")
		Normal = 1, // Column label for 1st table, then tableLabel.columnLabel (e.g. "Corp Structure.Mol. Wt.")
		Internal = 2, // Internal Mobius table name.colLabel (e.g. "CORP_STRUCTURE.Compound Id").
		Ordinal = 3, // Table index.ColumnLabel (e.g. T1.Compound Id.)
		MetaTableDotMetaColumnName = 4,  // MetaTableName.MetaColumnName (e.g. CORP_STRUCTURE.COMPOUND_ID)
		SpotfireApiClient = 5 // special format used by the Mobius SpotfireApiClient
	}

}
