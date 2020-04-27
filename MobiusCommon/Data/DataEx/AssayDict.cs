using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Drawing;

namespace Mobius.Data
{
	/// <summary>
	/// Common assay and target attributes from table CMN_ASSY_ATRBTS
	/// </summary>

	public class AssayDict
	{
		public static AssayDict Instance; // target assay attributes dictionary instance

		public List<AssayAttributes> // List of assays ordered by family, gene symbol, assay name
			AssayList = new List<AssayAttributes>();

		public Dictionary<string, AssayAttributes> // Map from target gene symbol to associated gene data
			TargetSymbolMap = new Dictionary<string, AssayAttributes>();

		public Dictionary<int, AssayAttributes> // Map from target gene id to associated gene data
			TargetIdMap = new Dictionary<int, AssayAttributes>();

		public Dictionary<int, List<AssayAttributes>> // Map from target gene id to list of assays
			TargetIdAssayMap = new Dictionary<int, List<AssayAttributes>>();

		public Dictionary<string, AssayAttributes> // Map from SourceDatabase-AssayIdTxt to assay-target info
			DatabaseAssayIdMap = new Dictionary<string, AssayAttributes>();

		public Dictionary<string, AssayAttributes> // Map from assay name to assay-Target info
			AssayNameMap = new Dictionary<string, AssayAttributes>();

		public Dictionary<int, AssayAttributes> // Map from assay Id to assay-target info
			AssayIdMap = new Dictionary<int, AssayAttributes>();

		public Dictionary<string, AssayAttributes> // Map from metatable name to assay-target info
			MetaTableNameMap = new Dictionary<string, AssayAttributes>();

		/// <summary>
		/// Deserialize from a binary file
		/// </summary>

		public static AssayDict DeserializeFromFile(
			string fileName)
		{
			FileStream fs = null;

			try
			{
				int t0 = TimeOfDay.Milliseconds();
				AssayDict tad = new AssayDict();

				if (File.Exists(fileName))
				{
					fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					BinaryFormatter bf = new BinaryFormatter();

					List<AssayAttributes> taaList = bf.Deserialize(fs) as List<AssayAttributes>;
					fs.Close();
					fs = null;

					foreach (AssayAttributes taa in taaList)
						tad.SetMaps(taa);
				}
				t0 = TimeOfDay.Milliseconds() - t0;
				return tad;
			}

			catch (Exception ex)
			{ // be sure file is closed
				if (fs != null)
				{
					fs.Close();
					fs = null;
				}

				throw new Exception(ex.Message, ex);
			}

		}

/// <summary>
		/// Setup the mapping information for the TargetAssayAttributes
/// </summary>
/// <param name="row"></param>

		public void SetMaps(AssayAttributes taa)
		{
			if (!String.IsNullOrEmpty(taa.GeneSymbol))
				TargetSymbolMap[taa.GeneSymbol.ToUpper()] = taa;

			if (taa.GeneId > 0)
			{
				TargetIdMap[taa.GeneId] = taa;
				if (!String.IsNullOrEmpty(taa.AssayDatabase) && !String.IsNullOrEmpty(taa.AssayIdTxt))
				{
					if (!TargetIdAssayMap.ContainsKey(taa.GeneId))
						TargetIdAssayMap[taa.GeneId] = new List<AssayAttributes>();
					TargetIdAssayMap[taa.GeneId].Add(taa);

					string key = taa.AssayDatabase.ToUpper() + "-" + taa.AssayIdTxt.ToUpper();
					DatabaseAssayIdMap[key] = taa;

					if (taa.AssayDatabase == "ASSAY") AssayIdMap[taa.AssayId2] = taa;

					key = taa.AssayName.Trim().ToUpper();
					AssayNameMap[key] = taa;
				}
			}

			string mtName = taa.AssayDatabase.ToUpper();
			mtName += "_" + taa.AssayIdTxt; // build lookup by metatable name
			MetaTableNameMap[mtName] = taa;
			return;
		}

		/// <summary>
		/// Write out the Target Assay Attribute dictionary
		/// </summary>
		/// <param name="tad"></param>
		/// <param name="fileName"></param>

		public void SerializeToFile (
			string fileName)
		{
			//			string fileName = ServerCacheDir + "AssayAttributes.bin";
			Stream fs = File.Open(fileName, FileMode.Create);
			BinaryFormatter bf = new BinaryFormatter();

			bf.Serialize(fs, AssayList);
			fs.Close();
			return;
		}
	}
}
