using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Mobius.UAL
{

/// <summary>
/// Functionality for creating and maintaining Target/Assay association information
/// </summary>

	public class AssayAttributesDao
	{
		public static string AssayAttrsTableName
		{
			get
			{
				if (_assayAttrsTableName == null)
					_assayAttrsTableName = ServicesIniFile.Read("AssayAttrsTableName", DefaultAssayAttrsTableName );
				return _assayAttrsTableName;
			}
		}
		static string _assayAttrsTableName = null;

		static string DefaultAssayAttrsTableName = "mbs_owner.cmn_assy_atrbts";

		public static string AdjustAssayAttrsTableName(string txt)
		{
			string txt2 = txt; // noop now
			//string txt2 = Lex.Replace(txt, DefaultAssayAttrsTableName, AssayAttrsTableName);
			return txt2;
		}

		/// <summary>
		/// Read in assay attribute information
		/// </summary>
		/// <returns></returns>

		public static AssayDict ReadAssayAttributesTable()
		{
			int t0 = TimeOfDay.Milliseconds();
			MetaTable mt = MetaTableCollection.Get(AssayAttrsTableName);
			if (mt == null) throw new Exception("Can't get table " + AssayAttrsTableName);

			UnpivotedAssayResultFieldPositionMap voMap = new UnpivotedAssayResultFieldPositionMap(); // used for fast indexing of value by col name
			string fList = "";
			for (int mci = 0; mci < mt.MetaColumns.Count; mci++)
			{
				string colMap = mt.MetaColumns[mci].ColumnMap;
				voMap.TrySetVoi(colMap, mci);
				if (fList != "") fList += ", ";
				fList += colMap;
			}

			string sql =
				"select " + fList + " " +
				"from mbs_owner.cmn_assy_atrbts " +
				"order by lower(gene_fmly), lower(gene_symbl), lower(assy_nm)";

//			sql = AdjustAssayAttrsTableName(sql);

			DbCommandMx dao = new DbCommandMx();
			dao.Prepare(sql);
			dao.ExecuteReader();

			AssayDict dict = new AssayDict();

			int readCnt = 0;
			while (true)
			{
				if (!dao.Read()) break;
				//if (readCnt > 100) break; // debug !!!
				readCnt++;
				object[] vo = new object[mt.MetaColumns.Count];
				for (int mci = 0; mci < mt.MetaColumns.Count; mci++)
					vo[mci] = dao.GetObject(mci);

				AssayAttributes row = AssayAttributes.FromValueObjectNew(vo, voMap);
				dict.AssayList.Add(row);
				dict.SetMaps(row);
			}

			dao.CloseReader();
			dao.Dispose();
			t0 = TimeOfDay.Milliseconds() - t0;
			return dict;
		}

		/// <summary>
		/// Insert a row into the target-assay association table
		/// </summary>
		/// <param name="toks"></param>
		/// <param name="dao"></param>

		public static void InsertCommonAssayAttributes(
			AssayAttributes taa,
			DbCommandMx dao)
		{
			string names = "";
			string values = "";
			AddInsertColumn("id", ref names, ref values, taa.Id);
			AddInsertColumn("assy_db", ref names, ref values, taa.AssayDatabase);
			AddInsertColumn("assy_id2", ref names, ref values, taa.AssayId2);
			AddInsertColumn("assy_id_nbr", ref names, ref values, taa.AssayIdNbr);     
			AddInsertColumn("assy_id_txt", ref names, ref values, taa.AssayIdTxt);     
			AddInsertColumn("assy_nm", ref names, ref values, taa.AssayName);         
			AddInsertColumn("assy_desc", ref names, ref values, taa.AssayDesc);       
			AddInsertColumn("assy_src", ref names, ref values, taa.AssaySource);        
			AddInsertColumn("assy_typ", ref names, ref values, taa.AssayType);        
			AddInsertColumn("assy_mode", ref names, ref values, taa.AssayMode);       
			AddInsertColumn("assy_sts", ref names, ref values, taa.AssayStatus);
			AddInsertColumn("assy_sum_lvl", ref names, ref values, taa.SummarizedAvailable ? 1 : 0);
			AddInsertColumn("assy_gene_cnt", ref names, ref values, taa.GeneCount);
			AddInsertColumn("rslt_typ", ref names, ref values, taa.ResultTypeConcType);        
			AddInsertColumn("rslt_typ_nm", ref names, ref values, taa.ResultName);
			AddInsertColumn("rslt_typ_id2", ref names, ref values, taa.ResultTypeId2);
			AddInsertColumn("rslt_typ_id_nbr", ref names, ref values, taa.ResultTypeIdNbr);
			AddInsertColumn("rslt_typ_id_txt", ref names, ref values, taa.ResultTypeIdTxt);
			AddInsertColumn("rslt_uom", ref names, ref values, taa.ResultTypeUnits);
			AddInsertColumn("conc_uom", ref names, ref values, taa.ResultTypeConcUnits);

			AddInsertColumn("top_lvl_rslt", ref names, ref values, taa.TopLevelResult);
			AddInsertColumn("remppd", ref names, ref values, taa.Remapped);          
			AddInsertColumn("mltplxd", ref names, ref values, taa.Multiplexed);         
			AddInsertColumn("rvwd", ref names, ref values, taa.Reviewed);            
			AddInsertColumn("cmpds_assyd", ref names, ref values, taa.CompoundsAssayed);     
			AddInsertColumn("prflng_assy", ref names, ref values, taa.ProfilingAssay);     
			AddInsertColumn("gene_id", ref names, ref values, taa.GeneId);         
			AddInsertColumn("gene_symbl", ref names, ref values, taa.GeneSymbol);      
			AddInsertColumn("gene_desc", ref names, ref values, taa.GeneDescription);
			AddInsertColumn("gene_fmly", ref names, ref values, taa.GeneFamily);

			AddInsertColumn("rslt_cnt", ref names, ref values, taa.ResultCount);
			AddInsertColumn("assy_updt_dt", ref names, ref values, taa.AssayUpdateDate);
			AddInsertColumn("assn_src", ref names, ref values, taa.AssociationSource);
			AddInsertColumn("assn_cnflct", ref names, ref values, taa.AssociationConflict);

			AddInsertColumn("x", ref names, ref values, taa.TargetMapX);
			AddInsertColumn("y", ref names, ref values, taa.TargetMapY);
			AddInsertColumn("z", ref names, ref values, taa.TargetMapZ); 

			string sql = "insert into mbs_owner.cmn_assy_atrbts (" + names + ") " +
			 "values (" + values + ")";

//			sql = AssayAttributesDao.AdjustAssayAttrsTableName(sql);

			dao.Prepare(sql);
			dao.ExecuteNonReader();
			return;
		}

		static void AddInsertColumn (
			string colName,
			ref string names,
			ref string values,
			object obj)
		{
			if (names != "") names += ",";
			names += colName;

			if (values != "") values += ", ";

			if (obj == null) values += "null";

			else if (obj is int && ((int)obj) == NullValue.NullNumber || // store null number values as nulls
			 obj is double && ((double)obj) == NullValue.NullNumber)
				values += "null";

			else if (obj is DateTime)
			{
				DateTime dt = (DateTime)obj;
				if (dt.Equals(DateTime.MinValue)) values += "null";
				else
				{
					string yyyymmdd = DateTimeMx.Normalize(dt);
					values += "to_date('" + yyyymmdd + "','YYYYMMDD')";
				}
			}

			else values += Lex.AddSingleQuotes(obj.ToString());

			return;
		}

	}

}
