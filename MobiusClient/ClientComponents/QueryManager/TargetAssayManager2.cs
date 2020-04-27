using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DevExpress.Data;

using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Mobius.ClientComponents
{
/// <summary>
/// TargetAssayManager2 DataTableManager partial class - Efficient Target-Assay Results (TAR) data source (for testing only)
/// </summary>

	public partial class DataTableManager
	{

		public bool UseUnpivotedAssayResultsCache = false; // if true use this cache for data access

// Arrays of in-memory data

		public int[] CompoundId;
		public ushort[] AssayId;
		public ushort[] ResultTypeId;
		public byte[] ActivityBin;
		public byte[] ResultQualifier;
		public float[] ResultNumericValue;
		public Dictionary<int, string> ResultTextValue;
		public Dictionary<int, string> Units;
		public Dictionary<int, float> Conc;
		public Dictionary<int, string> ConcUnits;
		public ushort[] RunDate; // days since Jan 1, 1900

		public int ResultsRowCount;

		UnpivotedAssayResultFieldPositionMap TarFieldPositionMap; // map from vo position to TarColumnEnum

		ushort LastAid = 0; // last AssayId seen
		AssayAttributes LastTaa = null; // associated TargetAssayAttributes

// Where cache files are kept

		public static string ServerCacheDir = @"<CachedDataDir>\";
		public static string ClientCacheDir = ClientDirs.CacheDir + @"\";
		public static DateTime BaseDate = new DateTime(1900, 1, 1);
		public static int GetPrimitiveValueFromTarCacheCalls = 0;

/// <summary>
/// Get the list, returns array of AssayIDs
/// </summary>
/// <returns></returns>
/// 
		public IList GetUnpivotedAssayResultsDataSource()
		{
			StreamReader sr;
			bool changed;
			int i1;

			if (AssayId != null) return AssayId;

// Read cache parameters

			changed = ServerFile.GetIfChanged(ServerCacheDir + "CacheParms.txt", ClientCacheDir + "CacheParms.txt");
			sr = new StreamReader(ClientCacheDir + "CacheParms.txt");
			string txt = sr.ReadLine();
			ResultsRowCount = int.Parse(txt);
			sr.Close();

// Init Vo position tables

			TarFieldPositionMap = UnpivotedAssayResultFieldPositionMap.NewOriginalMap(); // used for fast indexing of value by col name
			QueryTable qt = Query.GetQueryTableByName(MultiDbAssayDataNames.CombinedNonSumTableName);
			if (qt != null)
				TarFieldPositionMap.InitializeFromQueryTableVoPositions(qt, 0);

// Read in list of assay Ids

			AssayId = ReadUShortArray("AssayId.bin"); // read in the list of assay ids for starters
			return AssayId;
		}

/// <summary>
/// Get a primitive value from the in-memory Target Assay Results cache
/// </summary>
/// <param name="dri"></param>
/// <param name="rfld"></param>
/// <returns></returns>

		public object GetPrimitiveValueFromTarCache(
			int dri,
			ResultsField rfld)
		{
			AssayAttributes taa = null;
			BinaryReader br;
			long streamLength;
			string s;
			int rowNum;

			GetPrimitiveValueFromTarCacheCalls++;

			ushort aid = AssayId[dri];
			if (aid == LastAid)
				taa = LastTaa;
			else if (TargetAssayDict.AssayIdMap.ContainsKey(aid)) // link to associated target-assay attributes
			{
				LastAid = aid;
				LastTaa = taa = TargetAssayDict.AssayIdMap[aid]; 
			}

			int voi = rfld.VoPosition;
			if (voi < 0 || voi >= TarFieldPositionMap.VoiToTarColEnum.Length) return null; // not in range
			TarColEnum tarCol = TarFieldPositionMap.VoiToTarColEnum[voi]; 

			switch (tarCol)
			{

// Assay Attributes - Get from TargetAssayDictionary

				case TarColEnum.Id:
					if (taa != null) return taa.Id;
					else return null;

				case TarColEnum.TargetSymbol:
					if (taa != null) return taa.GeneSymbol;
					else return null;

				case TarColEnum.TargetId:
					if (taa != null) return taa.GeneId;
					else return null;

				case TarColEnum.TargetDescription:
					if (taa != null) return taa.GeneDescription;
					else return null;

				case TarColEnum.GeneFamily:
					if (taa != null) return taa.GeneFamily;
					else return null;

				case TarColEnum.GeneFamilyTargetSymbol: // catenation of target family & gene symbol
					if (taa != null) return taa.GeneFamilyTargetSymbol;
					else return null;

				case TarColEnum.AssayDatabase: 
					if (taa != null) return taa.AssayDatabase;
					else return null;

				case TarColEnum.AssayMetaTableName: // name of the metatable containing the data
					if (taa != null) return taa.AssayMetaTableName;
					else return null;

				case TarColEnum.AssayName:
					if (taa != null) return taa.AssayName;
					else return null;

				case TarColEnum.AssayNameSx: // contains name & link when deserialized from StringEx
					if (taa != null) return taa.AssayName; // todo: add link
					else return null;

				case TarColEnum.AssayDesc:
					if (taa != null) return taa.AssayDesc;
					else return null;

				case TarColEnum.AssayLocation:
					if (taa != null) return taa.AssaySource;
					else return null;

				case TarColEnum.AssayType:
					if (taa != null) return taa.AssayType;
					else return null;

				case TarColEnum.AssayMode:
					if (taa != null) return taa.AssayMode;
					else return null;

				case TarColEnum.AssayStatus:
					if (taa != null) return taa.AssayStatus;
					else return null;

				case TarColEnum.AssayGeneCount:
					if (taa != null) return taa.GeneCount;
					else return null;

				case TarColEnum.ResultName: // result label
					if (taa != null) return taa.ResultName;
					else return null;

				case TarColEnum.ResultTypeIdTxt: // text form of result type code
					if (taa != null) return taa.ResultTypeIdTxt;
					else return null;

				case TarColEnum.ResultTypeId2: // numeric form of result type code:
					if (taa != null) return taa.ResultTypeId2;
					else return null;

				case TarColEnum.ResultTypeConcType: // SP or CRC
					if (taa != null) return taa.ResultTypeConcType;
					else return null;

				case TarColEnum.TopLevelResult: // Y if this is a CRC or if a SP with no associated CRC
					if (taa != null) return taa.TopLevelResult;
					else return null;

				case TarColEnum.Remapped:
					if (taa != null) return taa.Remapped;
					else return null;

				case TarColEnum.Multiplexed:
					if (taa != null) return taa.Multiplexed;
					else return null;

				case TarColEnum.Reviewed:
					if (taa != null) return taa.Reviewed;
					else return null;

				case TarColEnum.ProfilingAssay:
					if (taa != null) return taa.ProfilingAssay;
					else return null;

				case TarColEnum.CompoundsAssayed:
					if (taa != null) return taa.CompoundsAssayed;
					else return null;

				case TarColEnum.ResultCount:
					if (taa != null) return taa.ResultCount;
					else return null;

				case TarColEnum.AssayUpdateDate: // date of most recent assay result
					if (taa != null) return taa.AssayUpdateDate;
					else return null;

				case TarColEnum.AssociationSource: // ASSAY2EG
					if (taa != null) return taa.AssociationSource;
					else return null;

				case TarColEnum.AssociationConflict: // conflicting source & it's assignment
					if (taa != null) return taa.AssociationConflict;
					else return null;

				case TarColEnum.TargetMapX: 
					if (taa != null) return taa.TargetMapX;
					else return null;

				case TarColEnum.TargetMapY:
					if (taa != null) return taa.TargetMapY;
					else return null;

				case TarColEnum.TargetMapZ:
					if (taa != null) return taa.TargetMapZ;
					else return null;

// Results - Get from in-memory arrays, one per table column

				case TarColEnum.CompoundId:
					if (CompoundId == null)
						CompoundId = ReadIntArray("CompoundId.bin");
					return CompoundId[dri];

				case TarColEnum.AssayId2: // numeric form of assay/table identifier
					return (int)AssayId[dri]; // should already have data AssayId array

				case TarColEnum.AssayIdTxt: // text form of assay/table identifier
					return AssayId.ToString();

				//case TarColEnum.ResultTypeIdTxt: // text form of result type code
				//  if (ResultTypeId == null)
				//    ResultTypeId = ReadUShortArray("ResultTypeId.bin");
				//  return ResultTypeId[dri].ToString(); // get from results data array

				//case TarColEnum.ResultTypeIdNbr: // numeric form of result type identifier
				//  if (ResultTypeId == null)
				//    ResultTypeId = ReadUShortArray("ResultTypeId.bin");
				//  return (int)ResultTypeId[dri]; // get from results data array

				case TarColEnum.ActivityBin:
					if (ActivityBin == null)
						ActivityBin = ReadByteArray("ActivityBin.bin");
					return (int)ActivityBin[dri];

				case TarColEnum.ResultValue: // just return numeric value
					if (ResultNumericValue == null)
						ResultNumericValue = ReadSingleArray("ResultNumericValue.bin");
					return ResultNumericValue[dri];

				case TarColEnum.ResultQualifier:
					if (ResultQualifier == null)
						ResultQualifier = ReadByteArray("ResultQualifier.bin");

					if (ResultQualifier[dri] != ' ') return ResultQualifier[dri].ToString();
					else return "";

				case TarColEnum.ResultNumericValue:
					if (ResultNumericValue == null)
						ResultNumericValue = ReadSingleArray("ResultNumericValue.bin");
					return ResultNumericValue[dri];

				case TarColEnum.ResultTextValue:
					if (ResultTextValue == null)
					{
						ResultTextValue = new Dictionary<int, string>();
						br = OpenBinaryReader("ResultTextValue.bin");
						streamLength = br.BaseStream.Length;
						while (true)
						{
							if (br.BaseStream.Position >= streamLength - 1) break;
							rowNum = br.ReadInt32();
							s = br.ReadString();
							ResultTextValue[rowNum] = s;
						}
						br.Close();
					}

					if (ResultTextValue.ContainsKey(dri))
						return ResultTextValue[dri];
					else return "";

				case TarColEnum.NValue:
					return 0;

				case TarColEnum.NValueTested:
					return 0;

				case TarColEnum.StdDev:
					return 0;

				case TarColEnum.Units:
					if (Units == null)
					{
						Units = new Dictionary<int, string>();
						br = OpenBinaryReader("Units.bin");
						streamLength = br.BaseStream.Length;
						while (true)
						{
							if (br.BaseStream.Position >= streamLength - 1) break;
							rowNum = br.ReadInt32();
							s = br.ReadString();
							Units[rowNum] = s;
						}
						br.Close();
					}

					if (ResultTextValue.ContainsKey(dri))
						return ResultTextValue[dri];
					else return "";

				case TarColEnum.Conc:
					if (Conc == null)
						ReadConcData();

					if (Conc.ContainsKey(dri))
						return Conc[dri];
					else return 0;

				case TarColEnum.ConcUnits:
					if (ConcUnits == null)
						ReadConcData();

					if (ConcUnits.ContainsKey(dri))
						return ConcUnits[dri];
					else return "";

				case TarColEnum.RunDate:
					if (RunDate == null)
						RunDate = ReadUShortArray("RunDate.bin");
					ushort days = RunDate[dri];
					DateTime dt = BaseDate.AddDays(days);
					return dt;

				case TarColEnum.ResultDetail:
					return "";

				default:
					throw new Exception("Unexpected TarColEnum: " + tarCol);
			}
		}

/// <summary>
/// Read concentration values & units
/// </summary>

		void ReadConcData()
		{
			int t0 = TimeOfDay.Milliseconds();
			Conc = new Dictionary<int, float>();
			ConcUnits = new Dictionary<int, string>();

			BinaryReader br = OpenBinaryReader("Conc.bin");
			long streamLength = br.BaseStream.Length;
			while (true)
			{
				if (br.BaseStream.Position >= streamLength - 1) break;
				int rowNum = br.ReadInt32();
				float conc = br.ReadSingle();
				Conc[rowNum] = conc;

				string concUnits = br.ReadString();
				ConcUnits[rowNum] = concUnits;
			}
			br.Close();

			t0 = TimeOfDay.Milliseconds() - t0;
			ClientLog.Message("Read cache file time (" + "Conc.bin" + "): " + t0);
			return;
		}

/// <summary>
/// ReadIntArray
/// </summary>
/// <param name="fileName"></param>
/// <returns></returns>

		int[] ReadIntArray(string fileName)
		{
			// See the following for data on fast binary reading: http://www.codeproject.com/KB/files/fastbinaryfileinput.aspx
			// Also see: Buffer.BlockCopy(src, scrOffset, dst, dstOffset, count);

			int t0 = TimeOfDay.Milliseconds();
			BinaryReader br = OpenBinaryReader(fileName);

			int[] a = new int[ResultsRowCount];
			for (int i1 = 0; i1 < ResultsRowCount; i1++)
			{
				if (i1 >= ResultsRowCount) break;
				a[i1] = br.ReadInt32();
			}

			br.Close();
			t0 = TimeOfDay.Milliseconds() - t0;
			ClientLog.Message("Read cache file time (" + fileName + "): " + t0);
			return a;
		}

/// <summary>
/// ReadUShortArray
/// </summary>
/// <param name="fileName"></param>
/// <returns></returns>

		ushort[] ReadUShortArray(string fileName)
		{
			int t0 = TimeOfDay.Milliseconds();
			BinaryReader br = OpenBinaryReader(fileName);

			ushort[] a = new ushort[ResultsRowCount];
			for (int i1 = 0; i1 < ResultsRowCount; i1++)
			{
				if (i1 >= ResultsRowCount) break;
				a[i1] = br.ReadUInt16();
			}

			br.Close();

			t0 = TimeOfDay.Milliseconds() - t0;
			ClientLog.Message("Read cache file time (" + fileName + "): " + t0);
			return a;
		}

		/// <summary>
		/// ReadByteArray
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		Byte[] ReadByteArray(string fileName)
		{
			int t0 = TimeOfDay.Milliseconds();
			BinaryReader br = OpenBinaryReader(fileName);

			Byte[] a = new Byte[ResultsRowCount];
			for (int i1 = 0; i1 < ResultsRowCount; i1++)
			{
				if (i1 >= ResultsRowCount) break;
				a[i1] = br.ReadByte();
			}

			br.Close();
			t0 = TimeOfDay.Milliseconds() - t0;
			ClientLog.Message("Read cache file time (" + fileName + "): " + t0);
			return a;
		}

		/// <summary>
		/// ReadCharArray
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		Char[] ReadCharArray(string fileName)
		{
			int t0 = TimeOfDay.Milliseconds();
			BinaryReader br = OpenBinaryReader(fileName);

			Char[] a = new Char[ResultsRowCount];
			for (int i1 = 0; i1 < ResultsRowCount; i1++)
			{
				if (i1 >= ResultsRowCount) break;
				a[i1] = br.ReadChar();
			}

			br.Close();
			t0 = TimeOfDay.Milliseconds() - t0;
			ClientLog.Message("Read cache file time (" + fileName + "): " + t0);
			return a;
		}

		/// <summary>
		/// ReadSingleArray
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		Single[] ReadSingleArray(string fileName)
		{
			int t0 = TimeOfDay.Milliseconds();
			BinaryReader br = OpenBinaryReader(fileName);

			Single[] a = new Single[ResultsRowCount];
			for (int i1 = 0; i1 < ResultsRowCount; i1++)
			{
				a[i1] = br.ReadSingle();
				if (i1 >= ResultsRowCount) break;
			}

			br.Close();
			t0 = TimeOfDay.Milliseconds() - t0;
			ClientLog.Message("Read cache file time (" + fileName + "): " + t0);
			return a;
		}

		/// <summary>
		/// ReadDoubleArray
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		Double[] ReadDoubleArray(string fileName)
		{
			int t0 = TimeOfDay.Milliseconds();
			BinaryReader br = OpenBinaryReader(fileName);

			Double[] a = new Double[ResultsRowCount];
			for (int i1 = 0; i1 < ResultsRowCount; i1++)
			{
				if (i1 >= ResultsRowCount) break;
				a[i1] = br.ReadDouble();
			}

			br.Close();
			t0 = TimeOfDay.Milliseconds() - t0;
			ClientLog.Message("Read cache file time (" + fileName + "): " + t0);
			return a;
		}

		/// <summary>
		/// ReadStringArray
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		String[] ReadStringArray(string fileName)
		{
			int t0 = TimeOfDay.Milliseconds();
			BinaryReader br = OpenBinaryReader(fileName);

			String[] a = new String[ResultsRowCount];
			for (int i1 = 0; i1 < ResultsRowCount; i1++)
			{
				if (i1 >= ResultsRowCount) break;
				a[i1] = br.ReadString();
			}

			br.Close();
			t0 = TimeOfDay.Milliseconds() - t0;
			ClientLog.Message("Read cache file time (" + fileName + "): " + t0);
			return a;
		}

		BinaryReader OpenBinaryReader(string fileName)
		{
			bool changed = ServerFile.GetIfChanged(ServerCacheDir + fileName, ClientCacheDir + fileName);

			FileStream fs = File.Open(ClientCacheDir + fileName, FileMode.Open);

			BinaryReader br = new BinaryReader(fs);
			return br;
		}

	} // DataTableManager
}
