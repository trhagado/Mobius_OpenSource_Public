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
	/// Target/Assay association and attributes
	/// </summary>

	[Serializable]
	public class AssayAttributes
	{
		public int Id;

		public string AssayDatabase;
		public int AssayIdNbr = NullValue.NullNumber; // numeric form of database specific ssay/table identifier
		public string AssayIdTxt; // text form of assay/table identifier
		public int AssayId2 = NullValue.NullNumber; // Secondary assay Id
		public string AssayMetaTableName; // name of the metatable containing the data
		public string AssayName;
		public string AssayLink; // link info for assay
		public string AssayDesc;
		public string AssaySource;
		public string AssayType;
		public string AssayMode;
		public string AssayStatus;

		public string ResultName; // result label
		public int ResultTypeId2 = NullValue.NullNumber;
		public int ResultTypeIdNbr = NullValue.NullNumber; // numeric form of result type code;
		public string ResultTypeIdTxt; // text form of result type code
		public string ResultTypeUnits; // units of the result
		public string ResultTypeConcType; // SP or CRC
		public string ResultTypeConcUnits; // units of the test concentration
		public string TopLevelResult; // Y if this is a CRC or if a SP with no associated CRC
		public bool SummarizedAvailable = false; // true if summarized data is available in the database

		public string Remapped;
		public string Multiplexed;
		public string Reviewed;
		public string ProfilingAssay;
		public int CompoundsAssayed = NullValue.NullNumber;

		public int ResultCount = NullValue.NullNumber;
		public DateTime AssayUpdateDate = DateTime.MinValue; // date of most recent assay result
		public string AssociationSource; // source of assay to gene association
		public string AssociationConflict; // any conflicting source

		public int GeneCount = 0; // positive for 1st gene negative for others
		public string GeneSymbol;
		public int GeneId = NullValue.NullNumber; // Entrez Gene id
		public string GeneDescription;
		public string GeneFamily;
		public string GeneFamilyTargetSymbol; // catenation of target family & gene symbol

		public double TargetMapX = NullValue.NullNumber;
		public double TargetMapY = NullValue.NullNumber;
		public double TargetMapZ = NullValue.NullNumber;

		/// <summary>
		/// Convert TargetAssayAttributes values into an value object array
		/// </summary>
		/// <param name="voLength"></param>
		/// <param name="voi"></param>
		/// <returns></returns>

		public object[] ToValueObject(
			int voLength,
			TargetAssayAttrsFieldPositionMap voi)
		{
			object[] vo = new object[voLength];

			ToValueObject(vo, voi);
			return vo;
		}

		public void ToValueObject(
			object[] vo,
			TargetAssayAttrsFieldPositionMap voi)
		{
			AssayAttributes row = this;

			SetVo(vo, voi.Id.Voi, row.Id);

			SetVo(vo, voi.TargetSymbol.Voi, row.GeneSymbol);
			SetVo(vo, voi.TargetId.Voi, row.GeneId);
			SetVo(vo, voi.TargetDescription.Voi, row.GeneDescription);
			SetVo(vo, voi.GeneFamily.Voi, row.GeneFamily);
			SetVo(vo, voi.GeneFamilyTargetSymbol.Voi, row.GeneFamilyTargetSymbol);

			SetVo(vo, voi.AssayType.Voi, row.AssayType);
			SetVo(vo, voi.AssayMode.Voi, row.AssayMode);

			if (Lex.IsNullOrEmpty(row.AssayLink)) // if no link info use simple string value
				SetVo(vo, voi.AssayName.Voi, row.AssayName);
			else
			{
				StringMx sx = new StringMx(row.AssayName);
				sx.Hyperlink = row.AssayLink;
				SetVo(vo, voi.AssayName.Voi, sx);
			}

			SetVo(vo, voi.AssayDesc.Voi, row.AssayDesc);
			SetVo(vo, voi.AssayDatabase.Voi, row.AssayDatabase);
			SetVo(vo, voi.AssayLocation.Voi, row.AssaySource);
			SetVo(vo, voi.AssayId2.Voi, row.AssayId2);
			SetVo(vo, voi.AssayIdNbr.Voi, row.AssayIdNbr);
			SetVo(vo, voi.AssayIdTxt.Voi, row.AssayIdTxt);
			SetVo(vo, voi.ResultTypeUnits.Voi, row.ResultTypeUnits);
			SetVo(vo, voi.ResultTypeConcUnits.Voi, row.ResultTypeConcUnits);

			SetVo(vo, voi.AssayMetaTableName.Voi, row.AssayMetaTableName);
			SetVo(vo, voi.AssayStatus.Voi, row.AssayStatus);
			SetVo(vo, voi.AssayGeneCount.Voi, row.GeneCount);

			SetVo(vo, voi.ResultTypeConcType.Voi, row.ResultTypeConcType);
			SetVo(vo, voi.ResultName.Voi, row.ResultName);
			SetVo(vo, voi.ResultTypeId2.Voi, row.ResultTypeId2);
			SetVo(vo, voi.ResultTypeIdNbr.Voi, row.ResultTypeIdNbr);
			SetVo(vo, voi.ResultTypeIdTxt.Voi, row.ResultTypeIdTxt);
			SetVo(vo, voi.TopLevelResult.Voi, row.TopLevelResult);

			SetVo(vo, voi.Remapped.Voi, row.Remapped);
			SetVo(vo, voi.Multiplexed.Voi, row.Multiplexed);
			SetVo(vo, voi.Reviewed.Voi, row.Reviewed);
			SetVo(vo, voi.ProfilingAssay.Voi, row.ProfilingAssay);
			SetVo(vo, voi.CompoundsAssayed.Voi, row.CompoundsAssayed);

			SetVo(vo, voi.ResultCount.Voi, row.ResultCount);
			SetVo(vo, voi.AssayUpdateDate.Voi, row.AssayUpdateDate);
			SetVo(vo, voi.AssociationSource.Voi, row.AssociationSource);
			SetVo(vo, voi.AssociationConflict.Voi, row.AssociationConflict);

			SetVo(vo, voi.TargetMapX.Voi, row.TargetMapX);
			SetVo(vo, voi.TargetMapY.Voi, row.TargetMapY);
			SetVo(vo, voi.TargetMapZ.Voi, row.TargetMapZ);
			return;
		}

		static void SetVo(
			object[] vo,
			int voi,
			object value)
		{
			VoArray.SetVo(vo, voi, value);
			return;
		}

		/// <summary>
		/// Convert a value objectarray into a new TargetAssayAttributes
		/// </summary>
		/// <param name="vo"></param>
		/// <param name="voMap"></param>
		/// <returns></returns>

		public static AssayAttributes FromValueObjectNew(
		object[] vo,
		TargetAssayAttrsFieldPositionMap voMap)
		{
			AssayAttributes row = new AssayAttributes();
			row.FromValueObject(vo, voMap);
			return row;
		}

		/// <summary>
		/// Convert a value objectarray into a TargetAssayAttributes
		/// </summary>
		/// <param name="vo"></param>
		/// <param name="voMap"></param>
		/// <returns></returns>

		public void FromValueObject(
			object[] vo,
			TargetAssayAttrsFieldPositionMap voMap)
		{
			string key;

			AssayAttributes row = this;
			row.Id = GetIntVo(vo, voMap.Id.Voi);

			row.GeneSymbol = GetStringVo(vo, voMap.TargetSymbol.Voi);
			row.GeneId = GetIntVo(vo, voMap.TargetId.Voi);
			row.GeneDescription = GetStringVo(vo, voMap.TargetDescription.Voi);
			row.GeneFamily = GetStringVo(vo, voMap.GeneFamily.Voi);
			row.GeneFamilyTargetSymbol = GetStringVo(vo, voMap.GeneFamilyTargetSymbol.Voi);

			row.AssayType = GetStringVo(vo, voMap.AssayType.Voi);
			row.AssayMode = GetStringVo(vo, voMap.AssayMode.Voi);

			object obj = GetObjectVo(vo, voMap.AssayName.Voi);
			if (obj != null)
			{
				if (obj is string) row.AssayName = (string)obj;
				else if (obj is StringMx)
				{
					StringMx sx = obj as StringMx;
					row.AssayName = sx.Value;

					if (sx.DbLink.Contains(","))
					{
						string mtName = sx.DbLink.Replace(",", "_").ToUpper();
						row.AssayMetaTableName = mtName;
					}
				}
			}

			row.AssayDesc = GetStringVo(vo, voMap.AssayDesc.Voi);
			row.AssayDatabase = GetStringVo(vo, voMap.AssayDatabase.Voi);
			row.AssaySource = GetStringVo(vo, voMap.AssayLocation.Voi);
			row.AssayId2 = GetIntVo(vo, voMap.AssayId2.Voi);
			row.AssayIdNbr = GetIntVo(vo, voMap.AssayIdNbr.Voi);
			row.AssayIdTxt = GetStringVo(vo, voMap.AssayIdTxt.Voi);

			string txt = GetStringVo(vo, voMap.AssayMetaTableName.Voi);
			if (txt != null) row.AssayMetaTableName = txt;

			row.AssayStatus = GetStringVo(vo, voMap.AssayStatus.Voi);
			row.GeneCount = GetIntVo(vo, voMap.AssayGeneCount.Voi);

			row.ResultName = GetStringVo(vo, voMap.ResultName.Voi);
			row.ResultTypeId2 = GetIntVo(vo, voMap.ResultTypeId2.Voi);
			row.ResultTypeIdNbr = GetIntVo(vo, voMap.ResultTypeIdNbr.Voi);
			row.ResultTypeIdTxt = GetStringVo(vo, voMap.ResultTypeIdTxt.Voi);
			row.ResultTypeConcType = GetStringVo(vo, voMap.ResultTypeConcType.Voi);
			row.ResultTypeUnits = GetStringVo(vo, voMap.ResultTypeUnits.Voi);
			row.ResultTypeConcUnits = GetStringVo(vo, voMap.ResultTypeConcUnits.Voi);
			row.TopLevelResult = GetStringVo(vo, voMap.TopLevelResult.Voi);

			row.Remapped = GetStringVo(vo, voMap.Remapped.Voi);
			row.Multiplexed = GetStringVo(vo, voMap.Multiplexed.Voi);
			row.Reviewed = GetStringVo(vo, voMap.Reviewed.Voi);
			row.ProfilingAssay = GetStringVo(vo, voMap.ProfilingAssay.Voi);
			row.CompoundsAssayed = GetIntVo(vo, voMap.CompoundsAssayed.Voi);

			row.ResultCount = GetIntVo(vo, voMap.ResultCount.Voi);
			row.AssayUpdateDate = GetDateVo(vo, voMap.AssayUpdateDate.Voi);
			row.AssociationSource = GetStringVo(vo, voMap.AssociationSource.Voi);
			row.AssociationConflict = GetStringVo(vo, voMap.AssociationConflict.Voi);

			row.TargetMapX = GetDoubleVo(vo, voMap.TargetMapX.Voi);
			row.TargetMapY = GetDoubleVo(vo, voMap.TargetMapY.Voi);
			row.TargetMapZ = GetDoubleVo(vo, voMap.TargetMapZ.Voi);

			// Complete assignment any missing assay database and/or assay id information

			AssayDict tad = AssayDict.Instance; // dict used for various lookups

			if (Lex.IsNullOrEmpty(row.AssayIdTxt) && row.AssayId2 == NullValue.NullNumber)
			{ // try to assign from assay name
				if (!Lex.IsNullOrEmpty(row.AssayName) && tad != null)
				{
					key = row.AssayName.Trim().ToUpper();
					if (tad.AssayNameMap.ContainsKey(key))
					{
						AssayAttributes taa = tad.AssayNameMap[key];
						row.AssayDatabase = taa.AssayDatabase;
						row.AssayId2 = taa.AssayId2;
						row.AssayIdNbr = taa.AssayIdNbr;
						row.AssayIdTxt = taa.AssayIdTxt;
					}
				}
			}

			else if (Lex.IsNullOrEmpty(row.AssayIdTxt))
			{
				row.AssayIdTxt = row.AssayId2.ToString();
			}

			else if (row.AssayId2 == NullValue.NullNumber)
			{
				int.TryParse(row.AssayIdTxt, out row.AssayId2);
			}

			// Complete any missing target information if target dict is available

			if (tad != null)
			{
				if (Lex.IsNullOrEmpty(row.GeneSymbol) && row.GeneId == NullValue.NullNumber) // assign target from assay info
				{
					if (!Lex.IsNullOrEmpty(row.AssayDatabase) && !Lex.IsNullOrEmpty(row.AssayIdTxt))
					{
						key = row.AssayDatabase.ToUpper() + "-" + row.AssayIdTxt.ToUpper();

						if (tad.DatabaseAssayIdMap.ContainsKey(key))
						{
							row.GeneSymbol = tad.DatabaseAssayIdMap[key].GeneSymbol;
							row.GeneId = tad.DatabaseAssayIdMap[key].GeneId;
						}
					}
				}

				else if (Lex.IsNullOrEmpty(row.GeneSymbol)) // assign symbol from targetid
				{
					if (tad.TargetIdMap.ContainsKey(row.GeneId))
						row.GeneSymbol = tad.TargetIdMap[row.GeneId].GeneSymbol;
				}

				else if (row.GeneId == NullValue.NullNumber) // assign targetId from symbol
				{
					string geneSymbol = row.GeneSymbol.ToUpper();
					if (tad.TargetSymbolMap.ContainsKey(geneSymbol))
					{
						//geneSymbol = "EPHB1"; // debug
						//geneSymbol = "AKT3"; // debug
						row.GeneId = tad.TargetSymbolMap[geneSymbol].GeneId;
					}
				}
			}

			return;
		}

		public static object GetObjectVo(
			object[] vo,
			int voi)
		{
			if (vo == null || voi < 0 || voi >= vo.Length) return null;
			else return vo[voi];
		}

		public static int GetIntVo(
			object[] vo,
			int voi)
		{
			if (vo == null || voi < 0 || voi >= vo.Length) return NullValue.NullNumber;

			object obj = vo[voi];
			if (NullValue.IsNull(obj)) return NullValue.NullNumber;
			else if (obj is decimal) return decimal.ToInt32((decimal)obj);
			else if (obj is int) return (int)obj;
			else if (obj is MobiusDataType) return (int)(obj as MobiusDataType).NumericValue;
			else throw new Exception("Unexpected type: " + obj.GetType().ToString());
		}

		public static long GetLongVo(
			object[] vo,
			int voi)
		{
			if (vo == null || voi < 0 || voi >= vo.Length) return NullValue.NullNumber;

			object obj = vo[voi];
			if (obj is decimal) return decimal.ToInt32((decimal)obj);
			else if (obj is int) return (int)obj;
			else if (obj is long) return (long)obj;
			else if (obj is MobiusDataType) return (long)(obj as MobiusDataType).NumericValue;
			else throw new Exception("Unexpected type: " + obj.GetType().ToString());
		}

		public static double GetDoubleVo(
			object[] vo,
			int voi)
		{
			if (vo == null || voi < 0 || voi >= vo.Length) return NullValue.NullNumber;

			object obj = vo[voi];
			if (NullValue.IsNull(obj)) return NullValue.NullNumber;
			else if (obj is decimal) return decimal.ToInt32((decimal)obj);
			else if (obj is int) return (int)obj;
			else if (obj is long) return (long)obj;
			else if (obj is double) return (double)obj;
			else if (obj is MobiusDataType) return (obj as MobiusDataType).NumericValue;
			else throw new Exception("Unexpected type: " + obj.GetType().ToString());
		}

		public static string GetStringVo(
			object[] vo,
			int voi)
		{
			if (vo == null || voi < 0 || voi >= vo.Length) return null;

			object obj = vo[voi];
			if (NullValue.IsNull(obj)) return null;
			else return obj.ToString();
		}

		public static MoleculeMx GetStructureVo(
			object[] vo,
			int voi)
		{
			if (vo == null || voi < 0 || voi >= vo.Length) return null;
			else return vo[voi] as MoleculeMx;
		}

		public static DateTime GetDateVo(
			object[] vo,
			int voi)
		{
			if (vo == null || voi < 0 || voi >= vo.Length) return DateTime.MinValue;

			object obj = vo[voi];
			if (NullValue.IsNull(obj)) return DateTime.MinValue;
			else if (obj is DateTime) return (DateTime)obj;
			else if (obj is DateTimeMx) return (obj as DateTimeMx).Value;
			else throw new Exception("Unexpected type: " + obj.GetType().ToString());
		}

		public static StringMx GetSxVo(
			object[] vo,
			int voi)
		{
			if (vo == null || voi < 0 || voi >= vo.Length) return null;
			else return vo[voi] as StringMx;
		}

		public static NumberMx GetNxVo(
			object[] vo,
			int voi)
		{
			if (vo == null || voi < 0 || voi >= vo.Length) return null;
			else return vo[voi] as NumberMx;
		}

		public static QualifiedNumber GetQnVo(
			object[] vo,
			int voi)
		{
			if (vo == null || voi < 0 || voi >= vo.Length) return null;
			else return vo[voi] as QualifiedNumber;
		}

		public static Hyperlink GetHyperlinkVo(
			object[] vo,
			int voi)
		{
			if (vo == null || voi < 0 || voi >= vo.Length) return null;
			else return vo[voi] as Hyperlink;
		}

		/// <summary>
		/// Get factor to multiply value by to get molar concentration
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>

		public static double GetMolarConcFactor(
			MetaColumn mc)
		{
			string exp;
			double molFactor = .000001; // assume micromolar if no other indication
			if (mc.Units.ToLower().IndexOf("mm") >= 0) molFactor = .001;
			else if (mc.Units.ToLower().IndexOf("um") >= 0) molFactor = .000001;
			else if (mc.Units.ToLower().IndexOf("nm") >= 0) molFactor = .000000001;
			else if (mc.Label.ToLower().IndexOf("mm") >= 0) molFactor = .001;
			else if (mc.Label.ToLower().IndexOf("um") >= 0) molFactor = .000001;
			else if (mc.Label.ToLower().IndexOf("nm") >= 0) molFactor = .000000001;

			return molFactor;
		}

		/// <summary>
		/// Clone
		/// </summary>
		/// <returns></returns>

		public AssayAttributes Clone()
		{
			return (AssayAttributes)this.MemberwiseClone();
		}

	}

	/// <summary>
	/// Mapping between TargetAssayAttrs and a value object array
	/// </summary>

	public class TargetAssayAttrsFieldPositionMap
	{
		public Dictionary<string, AssayAttributePosition> ColNameToFieldPosition = // map a col name to field position info
			new Dictionary<string, AssayAttributePosition>();

		public TarColEnum[] VoiToTarColEnum; // map a vo position to a TarColEnum

		public AssayAttributePosition Id = new AssayAttributePosition(TarColEnum.Id);
		public AssayAttributePosition TargetSymbol = new AssayAttributePosition(TarColEnum.TargetSymbol);
		public AssayAttributePosition TargetId = new AssayAttributePosition(TarColEnum.TargetId);
		public AssayAttributePosition TargetDescription = new AssayAttributePosition(TarColEnum.TargetDescription);
		public AssayAttributePosition GeneFamily = new AssayAttributePosition(TarColEnum.GeneFamily);
		public AssayAttributePosition GeneFamilyTargetSymbol = new AssayAttributePosition(TarColEnum.GeneFamilyTargetSymbol); // catenation of target family & gene symbol

		public AssayAttributePosition AssayDatabase = new AssayAttributePosition(TarColEnum.AssayDatabase);
		public AssayAttributePosition AssayId2 = new AssayAttributePosition(TarColEnum.AssayId2);
		public AssayAttributePosition AssayIdNbr = new AssayAttributePosition(TarColEnum.AssayIdNbr); // numeric form of assay/table identifier
		public AssayAttributePosition AssayIdTxt = new AssayAttributePosition(TarColEnum.AssayIdTxt); // text form of assay/table identifier
		public AssayAttributePosition AssayMetaTableName = new AssayAttributePosition(TarColEnum.AssayMetaTableName); // name of the metatable containing the data
		public AssayAttributePosition AssayName = new AssayAttributePosition(TarColEnum.AssayName);
		public AssayAttributePosition AssayNameSx = new AssayAttributePosition(TarColEnum.AssayNameSx); // contains name & link when deserialized from StringEx
		public AssayAttributePosition AssayDesc = new AssayAttributePosition(TarColEnum.AssayDesc);
		public AssayAttributePosition AssayLocation = new AssayAttributePosition(TarColEnum.AssayLocation);
		public AssayAttributePosition AssayType = new AssayAttributePosition(TarColEnum.AssayType);
		public AssayAttributePosition AssayMode = new AssayAttributePosition(TarColEnum.AssayMode);
		public AssayAttributePosition AssayStatus = new AssayAttributePosition(TarColEnum.AssayStatus);
		public AssayAttributePosition AssayGeneCount = new AssayAttributePosition(TarColEnum.AssayGeneCount);

		public AssayAttributePosition ResultName = new AssayAttributePosition(TarColEnum.ResultName); // result label
		public AssayAttributePosition ResultTypeIdNbr = new AssayAttributePosition(TarColEnum.ResultTypeId2);
		public AssayAttributePosition ResultTypeId2 = new AssayAttributePosition(TarColEnum.ResultTypeIdNbr); // numeric form of result type code
		public AssayAttributePosition ResultTypeIdTxt = new AssayAttributePosition(TarColEnum.ResultTypeIdTxt); // text form of result type code
		public AssayAttributePosition ResultTypeUnits = new AssayAttributePosition(TarColEnum.ResultTypeUnits);
		public AssayAttributePosition ResultTypeConcType = new AssayAttributePosition(TarColEnum.ResultTypeConcType); // SP or CRC
		public AssayAttributePosition ResultTypeConcUnits = new AssayAttributePosition(TarColEnum.ConcUnits);
		public AssayAttributePosition TopLevelResult = new AssayAttributePosition(TarColEnum.TopLevelResult); // Y if this is a CRC or if a SP with no associated CRC

		public AssayAttributePosition Remapped = new AssayAttributePosition(TarColEnum.Remapped);
		public AssayAttributePosition Multiplexed = new AssayAttributePosition(TarColEnum.Multiplexed);
		public AssayAttributePosition Reviewed = new AssayAttributePosition(TarColEnum.Reviewed);
		public AssayAttributePosition ProfilingAssay = new AssayAttributePosition(TarColEnum.ProfilingAssay);
		public AssayAttributePosition CompoundsAssayed = new AssayAttributePosition(TarColEnum.CompoundsAssayed);

		public AssayAttributePosition ResultCount = new AssayAttributePosition(TarColEnum.ResultCount);
		public AssayAttributePosition AssayUpdateDate = new AssayAttributePosition(TarColEnum.AssayUpdateDate); // date of most recent assay result
		public AssayAttributePosition AssociationSource = new AssayAttributePosition(TarColEnum.AssociationSource); // ASSAY2EG, ASSAY2EG2
		public AssayAttributePosition AssociationConflict = new AssayAttributePosition(TarColEnum.AssociationConflict); // conflicting source & it's assignment

		public AssayAttributePosition TargetMapX = new AssayAttributePosition(TarColEnum.TargetMapX);
		public AssayAttributePosition TargetMapY = new AssayAttributePosition(TarColEnum.TargetMapY);
		public AssayAttributePosition TargetMapZ = new AssayAttributePosition(TarColEnum.TargetMapZ);

		/// <summary>
		/// Init map from col name to col position instance var
		/// </summary>

		public TargetAssayAttrsFieldPositionMap()
		{
			ColNameToFieldPosition["ID"] = Id;
			ColNameToFieldPosition["GENE_SYMBL"] = TargetSymbol;
			ColNameToFieldPosition["GENE_ID"] = TargetId;
			ColNameToFieldPosition["GENE_DESC"] = TargetDescription;
			ColNameToFieldPosition["GENE_FMLY"] = GeneFamily;
			ColNameToFieldPosition["GENE_FMLY_GENE_SYMBL"] = GeneFamilyTargetSymbol;
			ColNameToFieldPosition["ASSY_TYP"] = AssayType;
			ColNameToFieldPosition["ASSY_MODE"] = AssayMode;
			ColNameToFieldPosition["ASSY_NM"] = AssayName;
			ColNameToFieldPosition["ASSY_NM_SX"] = AssayName; // needed?
			ColNameToFieldPosition["ASSY_DESC"] = AssayDesc;
			ColNameToFieldPosition["ASSY_DB"] = AssayDatabase;
			ColNameToFieldPosition["ASSY_SRC"] = AssayLocation;
			ColNameToFieldPosition["ASSY_ID_2"] = AssayId2;
			ColNameToFieldPosition["ASSY_ID_NBR"] = AssayIdNbr;
			ColNameToFieldPosition["ASSY_ID_TXT"] = AssayIdTxt;
			ColNameToFieldPosition["ASSY_MT_NM"] = AssayMetaTableName;
			ColNameToFieldPosition["ASSY_STS"] = AssayStatus;
			ColNameToFieldPosition["ASSY_GENE_CNT"] = AssayGeneCount;

			ColNameToFieldPosition["RSLT_TYP"] = ResultTypeConcType;
			ColNameToFieldPosition["RSLT_TYP_NM"] = ResultName;
			ColNameToFieldPosition["RSLT_TYP_ID_NBR"] = ResultTypeId2;
			ColNameToFieldPosition["RSLT_TYP_ID_TXT"] = ResultTypeIdTxt;

			ColNameToFieldPosition["RSLT_UOM"] = ResultTypeUnits;
			ColNameToFieldPosition["CONC_UOM"] = ResultTypeConcUnits;

			ColNameToFieldPosition["TOP_LVL_RSLT"] = TopLevelResult;
			ColNameToFieldPosition["REMPPD"] = Remapped;
			ColNameToFieldPosition["MLTPLXD"] = Multiplexed;
			ColNameToFieldPosition["RVWD"] = Reviewed;
			ColNameToFieldPosition["PRFLNG_ASSY"] = ProfilingAssay;
			ColNameToFieldPosition["CMPDS_ASSYD"] = CompoundsAssayed;
			ColNameToFieldPosition["RSLT_CNT"] = ResultCount;
			ColNameToFieldPosition["ASSY_UPDT_DT"] = AssayUpdateDate;
			ColNameToFieldPosition["ASSN_SRC"] = AssociationSource;
			ColNameToFieldPosition["ASSN_CNFLCT"] = AssociationConflict;

			ColNameToFieldPosition["X"] = TargetMapX;
			ColNameToFieldPosition["Y"] = TargetMapY;
			ColNameToFieldPosition["Z"] = TargetMapZ;

			return;
		}

		/// <summary>
		/// Initialize vo positions based on order of selected columns
		/// </summary>
		/// <param name="qt"></param>

		public int InitializeForQueryTable(QueryTable qt)
		{
			ClearMap();
			int voi = -1;
			int failCount = 0;
			foreach (QueryColumn qc in qt.QueryColumns)
			{
				if (!qc.Selected) continue;
				voi++;
				if (!TrySetVoi(qc.MetaColumn.Name, voi)) failCount++; //  voi--; (decementing is an error)
			}

			InitializeVoiToTarColEnum();
			return voi; // return the number of vo positions assigned
		}

		/// <summary>
		/// Initialize vo positions based on existing QueryColumn VoPosition
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="offset"></param>

		public void InitializeFromQueryTableVoPositions(
			QueryTable qt,
			int offset)
		{
			ClearMap();
			int voi = -1;
			foreach (QueryColumn qc in qt.QueryColumns)
			{
				if (!qc.Selected || qc.VoPosition < 0) continue;
				TrySetVoi(qc.MetaColumn.ColumnMap, qc.VoPosition + offset);
			}

			InitializeVoiToTarColEnum();
			return;
		}

		/// <summary>
		/// Clear Voi members of map
		/// </summary>

		void ClearMap()
		{
			foreach (AssayAttributePosition tafp in ColNameToFieldPosition.Values)
			{
				if (tafp == null) continue;
				tafp.Voi = -1;
			}
		}

		/// <summary>
		/// Setup the position maps between the columns and the value object array
		/// </summary>
		/// <param name="qt"></param>

		public void InitializeVoiToTarColEnum()
		{
			int maxVoi = 0;
			foreach (AssayAttributePosition tafp in ColNameToFieldPosition.Values)
			{
				if (tafp.Voi > maxVoi) maxVoi = tafp.Voi;
			}

			VoiToTarColEnum = new TarColEnum[maxVoi + 1]; // setup voi to TarColEnum map
			foreach (AssayAttributePosition tafp in ColNameToFieldPosition.Values)
			{
				if (tafp == null || tafp.Voi < 0) continue;
				if (tafp.Voi >= VoiToTarColEnum.Length) continue;
				VoiToTarColEnum[tafp.Voi] = tafp.TarColEnum;
			}
		}

		/// <summary>
		/// Set the value object index for the specified column name
		/// </summary>
		/// <param name="colName"></param>
		/// <param name="voi"></param>

		public bool TrySetVoi(
			string colName,
			int voi)
		{
			colName = colName.ToUpper();
			if (!ColNameToFieldPosition.ContainsKey(colName)) return false;
			AssayAttributePosition taaVoi = ColNameToFieldPosition[colName];
			//if (taaVoi == null) taaVoi.Voi = taaVoi.Voi; // debug				
			//if (Lex.Eq(colName, "ACTIVITY_BIN")) voi = voi; // debug;
			taaVoi.Voi = voi;
			return true;
		}

		/// <summary>
		/// Get the value object index for the specified column name
		/// </summary>
		/// <param name="colName"></param>
		/// <returns></returns>

		public bool TryGetVoi(
			string colName,
			out int voi)
		{
			voi = -1;
			colName = colName.ToUpper();
			if (!ColNameToFieldPosition.ContainsKey(colName)) return false;

			voi = ColNameToFieldPosition[colName].Voi;
			return true;
		}

		/// <summary>
		/// Return the vo position for a named QueryColumn
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="colName"></param>
		/// <returns></returns>

		public int GetQcVoi(
			QueryTable qt,
			string colName)
		{
			QueryColumn qc = qt.GetQueryColumnByName(colName);
			if (qc == null) return NullValue.NullNumber;
			else return qc.VoPosition;
		}

		/// <summary>
		/// Setup the position maps between the columns and the value object array
		/// </summary>
		/// <param name="qt"></param>

		public void SetupPositionMaps(QueryTable qt) // (obsolete)
		{
			int maxVoi = 0;
			foreach (string colName in ColNameToFieldPosition.Keys)
			{
				QueryColumn qc = qt.GetQueryColumnByName(colName);
				if (qc == null || !ColNameToFieldPosition.ContainsKey(colName))
					throw new Exception("Column name not found in QueryTable: " + colName);

				AssayAttributePosition tafp = ColNameToFieldPosition[colName];
				tafp.Voi = qc.VoPosition;
				if (qc.VoPosition > maxVoi) maxVoi = qc.VoPosition;
			}

			VoiToTarColEnum = new TarColEnum[maxVoi + 1]; // setup voi to TarColEnum map
			foreach (AssayAttributePosition tafp in ColNameToFieldPosition.Values)
			{
				if (tafp == null || tafp.Voi < 0) continue;
				VoiToTarColEnum[tafp.Voi] = tafp.TarColEnum;
			}
		}
	}

	/// <summary>
	/// Positions of target assay result in vo and in TarColEnum
	/// </summary>

	public class AssayAttributePosition
	{
		public int Voi = -1;
		public TarColEnum TarColEnum = TarColEnum.Unknown;

		/// <summary>
		/// Default constructor
		/// </summary>

		public AssayAttributePosition()
		{
			return;
		}

		/// <summary>
		/// Set TarColEnum 
		/// </summary>
		/// <param name="tarColEnum"></param>

		public AssayAttributePosition(TarColEnum tarColEnum)
		{
			TarColEnum = tarColEnum;
		}
	}

	/// <summary>
	/// Basic gene information
	/// </summary>

	public class GeneInfo
	{
		public string Symbol = "";
		public bool PrimarySymbol = true; // true if primary symbol
		public int Id; // Entrez Gene Id
		public string Description = "";
		public string Family = "";
		public int Order;
	}

	/// <summary>
	/// Target Map
	/// </summary>

	public class TargetMap
	{
		public string Name = ""; // map name
		public string Label = ""; // map label displayed to user
		public string Source = ""; // source of map
		public string Type = ""; // dendogram, pathway...
		public string ImageType; // extension type of image for map
		public string ImageFile = ""; // image file (without directory)
		public Rectangle Bounds; // image bounds
		public bool MarkBounds = false; // if true put dummy points in rectangle boundaries  
		public string CoordsFile = ""; // coords file (without directory)
		public Dictionary<int, List<TargetMapCoords>> Coords; // list of coordinates indexed by symbol
	}

	public class TargetMapCoords
	{
		public int TargetId; // Entrez gene id for target
		public int X;
		public int Y;
		public int X2;
		public int Y2;
	}

	/// <summary>
	/// Class containing assay metadata
	/// </summary>

	public class AssayMetaData
	{
		public int AssayId;
		public string Name = ""; // assay name
		public string MdTxt = ""; // Conc./Dose Response Curve (111) or Single Point (222)
		public bool SP = false; // Single Point assay
		public bool CRC = false; // Conc./Dose Response Curve assay

		public List<AssayTarget> Targets = new List<AssayTarget>();

		public static bool IsAssayMetaTableName(string mtName)
		{
			string tableNamePrefix;
			bool isSummary;
			int tableId;

			MetaTable.ParseMetaTableName(mtName, out tableNamePrefix, out tableId, out isSummary);
			if (tableId > 0) return true;
			else return false;
		}

		/// <summary>
		/// Get the source of the assay results
		/// </summary>
		/// <param name="assayId"></param>
		/// <returns></returns>

		public static string GetAssaySource(int assayId)
		{
			throw new NotImplementedException();
		}

/// <summary>
/// Get metadata for assay
/// </summary>
/// <param name="assayId"></param>
/// <returns></returns>

		public static AssayMetaData GetById(int assayId)
		{
			throw new NotImplementedException();
		}

	}

	/// <summary>
	/// Target information
	/// </summary>

	public class AssayTarget
	{
		public int TargetId;
		public string TargetName = "";
		public string TargetDesc = "";
		public string TargetTypeName = ""; // e.g. G protein coupled receptor
		public string TargetTypeShortName = ""; // e.g. GPCR
		public List<AssayGene> Genes = new List<AssayGene>(); // list of genes
	}

	/// <summary>
	/// Gene associated with biological target
	/// </summary>

	public class AssayGene
	{
		public string GeneId = "";
		public string GeneSymbol = "";
	}

	/// <summary>
	/// Assay result type key fields
	/// </summary>

	public class AssayResultType
	{
		public int RsltTypeId = -1;
		public string Name = ""; // full name
		public string SumMdTxt = ""; // Linear (1207), Log Normal (100), None (1717), None (textual result) (38)
		public string RsltLvl = ""; // Raw (), Normalized (), Derived (), Summarized ()
	}

	/// <summary>
	/// Result type codes
	/// </summary>

	public enum AssayResultTypeEnum
	{
		Inhibition = 1,
		IC50 = 2,
		Potency = 4,
		Ki = 5,
		Kb = 6,
		Stimulation = 8,
		EC50 = 9,
		Ka = 14,
		Kd = 11157
	}

}
