using Mobius.UAL;
using Mobius.ComOps;
using Mobius.Data;
using Mobius.QueryEngineLibrary;

using System;
using System.IO;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Mobius.MetaFactoryNamespace
{
	/// <summary>
	/// Build metatables for PubChem
	/// </summary>

	public class PubChemMetaFactory : IMetaFactory
	{
	/// <summary>
	/// Basic information on an assay
	/// </summary>

		public static string PubChemAssayDirectory; // folder with XML for pubchem
		public static string PubChemWarehouseTable = "<pubChemWarehouseTable>";
		public static string PubChemWarehouseSeq = "pubChemWarehouseSeq";
		public static int GetMetaTableCount, GetMetaTableTime, GetMetaTableAvgTime; // performance data

		public PubChemMetaFactory()
		{ }

/// <summary>
/// Create MetaTable for a PubChem assay
/// </summary>

		public MetaTable GetMetaTable (
			string name)
		{
			MetaTable mt = null, mt2 = null, mt3 = null;
			MetaColumn mc,mcc;
			string txt,tok;
			string aid;

			string prefix = "pubchem_aid_";
			if (!name.ToLower().StartsWith(prefix)) 
				return null;

			int t0 = TimeOfDay.Milliseconds();

			aid = name.Substring(prefix.Length);

			XmlNode node;
			XmlNodeList nodes;

			TextReader rdr = GetAssayXml(aid);
			if (rdr == null) return null;
			XmlDocument doc = new XmlDocument();
			doc.Load(rdr);

			mt = new MetaTable();
			mt.Name = "PUBCHEM_AID_" + aid.ToString();
			mt.TableMap = 
				"(select * " +
				"from mbs_owner.mbs_pbchm_rslt)";
			mt.MetaBrokerType = MetaBrokerType.Pivot;
			mt.Parent = MetaTableCollection.Get("pubchem_structure");
			mt.TableFilterColumns = Csv.SplitCsvString("mthd_vrsn_id");
			mt.TableFilterValues = Csv.SplitCsvString(aid);
			mt.PivotMergeColumns = Csv.SplitCsvString("ext_cmpnd_id_nbr, rslt_grp_id");
			mt.PivotColumns = Csv.SplitCsvString("rslt_typ_id");

			XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
			nsmgr.AddNamespace("ab", "http://www.ncbi.nlm.nih.gov"); // must look within this namespace

			MetaTreeNode mtn = MetaTreeFactory.GetNode(mt.Name); // try to get label from contents tree
			mtn = null;
			if (mtn != null) mt.Label = mtn.Label;
			else
			{
				string dataSource = SelectNodeValue(doc, nsmgr, "//ab:PC-DBTracking_name");
				string assayName = SelectNodeValue(doc, nsmgr, "//ab:PC-AssayDescription_name");
				string outcomeMethod = SelectNodeValue(doc, nsmgr, "//ab:PC-AssayDescription_activity-outcome-method");

				mt.Label = assayName;
				if (outcomeMethod == "1") outcomeMethod = "Primary";
				else if (outcomeMethod == "2") outcomeMethod = "Dose-Response";
				else if (outcomeMethod == "3") outcomeMethod = "Probe Summary";
				else outcomeMethod = "";
				if (outcomeMethod.Length > 0) mt.Label += " (" + outcomeMethod + ")";
			}
			if (mt.Label=="") mt.Label = mt.Name;

			mt.Description = // reference PubChem description page
				"http://pubchem.ncbi.nlm.nih.gov/assay/assay.cgi?aid=" + aid; 

			nodes = doc.SelectNodes("//ab:PC-ResultType", nsmgr); // result types

// Build common metacolumns

			mc = mt.AddMetaColumn("PUBCHEM_CID","CID",MetaColumnType.CompoundId,ColumnSelectionEnum.Selected,14);
			mc.ColumnMap = "ext_cmpnd_id_nbr";
			mc = mt.AddMetaColumn("PUBCHEM_SID","SID",MetaColumnType.Integer,ColumnSelectionEnum.Selected,7);
			mc.ColumnMap = "rslt_val_nbr";
			mc.PivotValues = Csv.SplitCsvString("1001"); // assign codes that won't interfere with assay-specific codes
			mc = mt.AddMetaColumn("PUBCHEM_ACTIVITY_SCORE","Activity Score",MetaColumnType.Integer,ColumnSelectionEnum.Selected,7);
			mc.ColumnMap = "rslt_val_nbr";
			mc.PivotValues = Csv.SplitCsvString("1002");
			mc = mt.AddMetaColumn("PUBCHEM_ACTIVITY_OUTCOME","Activity Outcome",MetaColumnType.DictionaryId,ColumnSelectionEnum.Selected,7);
			mc.ColumnMap = "rslt_val_nbr";
			mc.Dictionary = "PubChemActivityOutcome"; // code stored in db must be translated via dictionary
			mc.PivotValues = Csv.SplitCsvString("1003");
			mc = mt.AddMetaColumn("PUBCHEM_ACTIVITY_URL","Activity URL",MetaColumnType.Hyperlink,ColumnSelectionEnum.Selected,7);
			mc.ColumnMap = "rslt_val_txt";
			mc.PivotValues = Csv.SplitCsvString("1004");

// Build assay-specific metacolumns

			for (int ci = 0; ci < nodes.Count; ci++)
			{
				node = nodes[ci];
				string typeId = SelectNodeValue(node, nsmgr, ".//ab:PC-ResultType_tid"); // get values relative to node (need "." in xpath string)
				string typeName = SelectNodeValue(node, nsmgr, ".//ab:PC-ResultType_type");
				string colName = SelectNodeValue(node, nsmgr, ".//ab:PC-ResultType_name");

				string desc = SelectNodeValue(node, nsmgr, ".//ab:PC-ResultType_description_E");
				if (desc.IndexOf(", unit: ") > 0) // shorten label a bit
				{
					desc = desc.Replace(", unit: ", " (");
					if (desc.EndsWith(".")) desc = desc.Substring(0, desc.Length - 1);
					desc += ")";
				}

				string unit = SelectNodeValue(node, nsmgr, ".//ab:PC-ResultType_unit");
				if (unit=="") unit = SelectNodeValue(node, nsmgr, ".//ab:PC-ResultType_sunit");
				if (Lex.Eq(unit, "none") || Lex.Eq(unit, "unspecified")) unit = "";
				if (unit.EndsWith("ml")) unit = unit.Substring(0, unit.Length - 2) + "/mL";

				string transform = SelectNodeValue(node, nsmgr, ".//ab:PC-ResultType_transform");

				mc = new MetaColumn();
				mc.PivotValues = Csv.SplitCsvString(typeId);
				mc.Name = "R_" + typeId;

				mc.Label = colName;
				if (unit != "") mc.Label += " (" + unit + ")";
				if (mc.Label == "") mc.Label = mc.Name;

				//mc.Label = desc; // user longer description for label
				//if (mc.Label == "") mc.Label = mc.Label;

				mc.Format = ColumnFormatEnum.Default;
				mc.Decimals = -1;
				mc.InitialSelection = ColumnSelectionEnum.Selected;

				if (Lex.Eq(typeName, "float"))
				{
					mc.DataType = MetaColumnType.Number;
					mc.Width = 8;
					mc.ColumnMap = "rslt_val_nbr";
				}
				else if (Lex.Eq(typeName, "int"))
				{
					mc.DataType = MetaColumnType.Number;
					mc.Width = 8;
					mc.Format = ColumnFormatEnum.Decimal;
					mc.Decimals = 0;
					mc.ColumnMap = "rslt_val_nbr";
				}
				else if (Lex.Eq(typeName, "bool"))
				{
					mc.DataType = MetaColumnType.Number;
					mc.Width = 8;
					mc.ColumnMap = "rslt_val_nbr";
				}
				else if (Lex.Eq(typeName, "string"))
				{
					mc.DataType = MetaColumnType.String;
					mc.Width = 8;
					mc.ColumnMap = "rslt_val_txt";
				}
				else throw new Exception("Invalid type" + typeName);

				mc.MetaTable = mt;
				mt.AddMetaColumn(mc);
			}

			mc = mt.AddMetaColumn("PUBCHEM_ASSAYDATA_COMMENT","Comment",MetaColumnType.String,ColumnSelectionEnum.Unselected,7);
			mc.ColumnMap = "rslt_val_txt";
			mc.PivotValues = Csv.SplitCsvString("1005");
			mc = mt.AddMetaColumn("PUBCHEM_EXT_DATASOURCE_REG","Ext. Source. Reg. Id",MetaColumnType.String,ColumnSelectionEnum.Unselected,7);
			mc.ColumnMap = "rslt_val_txt";
			mc.PivotValues = Csv.SplitCsvString("1006");
			mc = mt.AddMetaColumn("PUBCHEM_ASSAYDATA_REVOKE","Revoked",MetaColumnType.String,ColumnSelectionEnum.Hidden,7);
			mc.ColumnMap = "rslt_val_txt";
			mc.PivotValues = Csv.SplitCsvString("1007");

			MetaTableFactory.SetAnyNewMetaTableLabel(mt); // if table has been renamed the set new label
			foreach (MetaColumn mc2 in mt.MetaColumns)
				MetaTableFactory.SetAnyNewMetaColumnLabel(mc2); // if col has been renamed then set new label

			GetMetaTableCount++;
			t0 = TimeOfDay.Milliseconds() - t0;
			GetMetaTableTime += t0;
			GetMetaTableAvgTime = GetMetaTableTime / GetMetaTableCount;

			return mt;
		}

		TextReader GetAssayXml(
			string aid)
		{
			TextReader rdr = null;
			DbCommandMx dao = null;
			string xml;

// Try to get from file first

			if (PubChemAssayDirectory == null)
				PubChemAssayDirectory = ServicesIniFile.Read("PubChemAssayDirectory");
			if (PubChemAssayDirectory != "")
				try
				{
					string fileName = PubChemAssayDirectory + @"\CSV\Description\" + aid + ".descr.xml";
					StreamReader sr = new StreamReader(fileName);
					xml = sr.ReadToEnd();
					sr.Close();
					if (String.IsNullOrEmpty(xml)) return null;
					return new StringReader(xml);
				}
				catch (Exception ex) { }

// Try to get from table (new method)

			string sql =
				"select xml " +
				"from mbs_owner.mbs_pbchm_assy_xml " +
				"where aid = :0";

			try
			{
				dao = new DbCommandMx();
				dao.PrepareParameterized(sql, DbType.String);
				dao.ExecuteReader(aid);
				if (!dao.Read()) throw new Exception("Not found");
				xml = dao.GetString(0);
				dao.Dispose();
			}
			catch (Exception ex)
			{
				dao.Dispose();
				return null;
			}

			if (String.IsNullOrEmpty(xml)) return null;
			return new StringReader(xml);
		}

/// <summary>
/// Do xpath search for specified node & return inner xml or blank
/// </summary>
/// <param name="baseNode"></param>
/// <param name="nsmgr"></param>
/// <param name="xpath"></param>
/// <returns></returns>
/// 
		string SelectNodeValue(
			XmlNode baseNode,
			XmlNamespaceManager nsmgr,
			string xpath)
		{
			XmlNode node = baseNode.SelectSingleNode(xpath, nsmgr);
			if (node == null) return "";
			XmlNode attr = node.Attributes.GetNamedItem("value"); // if value attribute then get it
			if (attr != null) return attr.Value;
			string value = node.InnerXml;
			if (value == null) return "";
			else return value;
		}

/// <summary>
/// Load data into PubChem database
/// PubChem assay data files are downloaded from the PubChem site:
/// http://pubchem.ncbi.nlm.nih.gov/ using a program like SmartFTP.
/// The files are in GNU Zip (.gz) format and can be unzipped with 
/// the following gzip commands:
///  c:\gzip\gzip -d c:\pubchem\bioassay\csv\description\*.gz
///  c:\gzip\gzip -d c:\pubchem\bioassay\csv\data\*.gz
/// After downloading and decompression this method can be called on the files.
/// </summary>
/// <param name="args"></param>
/// <returns></returns>

		public static string LoadData(
			string aid)
		{
			int recCount = 0;

			string mtName = "PubChem_aid_" + aid;
			MetaTable mt = MetaTableCollection.Get(mtName);
			if (mt == null) return "Failed to get metatable";

//			if (Math.Sqrt(4) == 2) goto UpdateCids;

			string fileName = PubChemAssayDirectory + @"\CSV\Data\" + aid + ".csv";
			StreamReader sr;
			try { sr = new StreamReader(fileName); }
			catch (Exception ex) { return "File not found: " + fileName; }

			string header = sr.ReadLine(); // read headers line
			List<string> headers = Csv.SplitCsvString(header);
			int cidIdx = -1;
			for (cidIdx = 0; cidIdx < headers.Count; cidIdx++)
			{
				if (headers[cidIdx].ToUpper() == "PUBCHEM_CID") break;
			}
			if (cidIdx >= headers.Count)
			{
				sr.Close();
				return "PUBCHEM_CID column not found in data headers";
			}

			Dictionary<string, MetaColumn> mcd = new Dictionary<string, MetaColumn>();
			foreach (MetaColumn mc2 in mt.MetaColumns)
				mcd[mc2.Name.ToUpper()] = mc2; // build dict for quick metacolumn lookup

			DbConnectionMx conn = DbConnectionMx.MapSqlToConnection(ref PubChemWarehouseTable);
			conn.BeginTransaction(); // do multiple updates per transaction

			GenericDwDao dao = new GenericDwDao(
			 PubChemWarehouseTable, // table for results
			 PubChemWarehouseSeq); // sequence to use
			dao.BufferInserts(true); // buffer inserts for better speed

			SequenceDao.SetCacheSize(PubChemWarehouseSeq, 100); // number of ids to cache locally from sequence

			//string progressMsg = "Deleting existing data...";
			int i1 = dao.DeleteTable(Int32.Parse(mt.TableFilterValues[0]), true);
			//if (Progress.CancelRequested())
			//{
			//  dao.Dispose();
			//  return "Cancelled during data delete";
			//}

			//Progress.Show("Loading file...");

			recCount = 0;
			int t1 = 0;

			while (true)
			{
				int t2 = TimeOfDay.Milliseconds();
				if (t2 - t1 > 1000)
				{
					if (Progress.CancelRequested)
					{
						dao.ExecuteBufferedInserts();
						conn.Commit();
						conn.Close();
						sr.Close();
						Progress.Hide();
						return recCount.ToString() + " rows loaded";
					}
					Progress.Show("Loading file (" + recCount.ToString() + ") ...");
					t1 = t2;
				}

				string rec = sr.ReadLine();
				if (rec == null) break;
				List<string> vals = Csv.SplitCsvString(rec);
				int cid;
				try { cid = Int32.Parse(vals[cidIdx]); } // get compound id
				catch (Exception ex) 
				{ 
					string txtCid = vals[cidIdx];
					if (txtCid==null) txtCid = "";
					DebugLog.Message("Load PubChem bad CID " + txtCid + ", AID = " + aid);
					continue; 
				} 

				long rslt_grp_id = dao.GetNextIdLong(); // id to hold row together
				for (int vi = 0; vi < vals.Count; vi++)
				{
					string s = vals[vi];
					if (s == "") continue;
					string[] sa = rec.Split(',');
					if (vi >= headers.Count) continue;
					string mcName = headers[vi].ToUpper();
					if (mcName.Length > 26) mcName = mcName.Substring(0, 26); // limit length to 26 

					if (mcName == "PUBCHEM_CID") continue;

					if (Lex.IsInteger(mcName)) mcName = "R_" + mcName; // result number
					MetaColumn mc = mcd[mcName];
					if (mc == null) continue;

					AnnotationVo vo = new AnnotationVo();
					vo.rslt_grp_id = rslt_grp_id;

					if (mc.DataType == MetaColumnType.String)
					{
						vo.rslt_val_txt = s;
					}

					else if (mc.DataType == MetaColumnType.Number || mc.DataType == MetaColumnType.Integer)
					{
						try
						{
							vo.rslt_val_nbr = Convert.ToDouble(s);
						}
						catch (Exception e) { continue; } // just continue if bad
					}

					else if (mc.DataType == MetaColumnType.Date)
					{
						s = DateTimeMx.Normalize(s);
						if (s == null) continue;
						vo.rslt_val_dt = DateTimeMx.NormalizedToDateTime(s);
					}

					else if (mc.Name == "PUBCHEM_ACTIVITY_OUTCOME") // activity outcome is a dict value stored as an integer
					{
						try
						{
							vo.rslt_val_nbr = Convert.ToInt32(s);
						}
						catch (Exception e) { continue; } // just continue if bad
					}

					else if (mc.DataType == MetaColumnType.Hyperlink || 
						mc.DataType == MetaColumnType.DictionaryId)
					{
						vo.rslt_val_txt = s;
					}

					else continue;

					vo.ext_cmpnd_id_nbr = cid;
					vo.ext_cmpnd_id_txt = cid.ToString();
					vo.mthd_vrsn_id = Int32.Parse(mt.TableFilterValues[0]);
					vo.rslt_typ_id = Int32.Parse(mc.PivotValues[0]);
					vo.chng_op_cd = "I";
                    vo.chng_usr_id = Security.UserInfo.UserName;

					dao.Insert(vo);
				} // end of field loop

				recCount++;
				if (recCount % 100 == 0)
				{ // commit after group of updates
					dao.ExecuteBufferedInserts();
					conn.Commit();
					conn.BeginTransaction(); // do multiple updates per transaction
				}

			} // end of record loop

			dao.ExecuteBufferedInserts();
			conn.Commit();
			conn.Close();
			dao.Dispose();
			sr.Close();

//UpdateCids: // Add any missing CIDs under method 1000000

			Progress.Show("Updating CID table...");

			string sql =
			"INSERT INTO " + PubChemWarehouseTable + "(ext_cmpnd_id_nbr,rslt_id,mthd_vrsn_id,rslt_typ_id,rslt_grp_id) " +
			"SELECT ext_cmpnd_id_nbr, " + PubChemWarehouseSeq + ".NEXTVAL,1000000,0,0 " +
			"FROM ( " +
			"SELECT UNIQUE ext_cmpnd_id_nbr " +
			"FROM " + PubChemWarehouseTable + " r1 " +
			"WHERE mthd_vrsn_id = " + aid + " " +
			"AND NOT EXISTS ( " +
			" SELECT * " +
			"FROM " + PubChemWarehouseTable + " r2 " +
			"WHERE mthd_vrsn_id = 1000000 " +
			"AND r2.ext_cmpnd_id_nbr = r1.ext_cmpnd_id_nbr) " +
			"and rownum <= 10000)";

			DbCommandMx drd = new DbCommandMx();
			drd.Prepare(sql);
			drd.BeginTransaction();

			int newCids = 0;
			while (true)
			{
				int addedCids = drd.ExecuteNonReader();
				if (addedCids == 0) break;
				newCids += addedCids;
				drd.Commit();
				drd.BeginTransaction(); // do multiple updates per transaction
				Progress.Show("Updating CID table (" + newCids.ToString() + ")...");
			}

			drd.Dispose();

			Progress.Hide();
			return recCount.ToString() + " rows loaded for AID " + aid + " plus " + newCids.ToString() + " new CIDs";
		}

		/// <summary>
		/// Calculate & persist metatable stats for tables belonging to broker
		/// </summary>
		/// <returns></returns>

		public int UpdateMetaTableStatistics()
		{
			return 0;
		}

		/// <summary>
		/// Load persisted metatable stats
		/// </summary>
		/// <param name="metaTableStats"></param>
		/// <returns></returns>

		public int LoadMetaTableStats(
			Dictionary<string, MetaTableStats> metaTableStats)
		{
			return 0;
		}

	}
}


