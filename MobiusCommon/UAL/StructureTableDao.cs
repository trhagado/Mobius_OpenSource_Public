using Mobius.UAL;
using Mobius.ComOps;
using Mobius.Data;
using Mobius.Helm;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace Mobius.UAL
{
	public class StructureTableDao
	{

		public static bool Debug = false;

/// <summary>
/// Retrieve any existing SVG for the list of supplied molecules
/// The Id column should contain the CorpId
/// </summary>
/// <param name="molList"></param>

		public static int SelectMoleculeListSvg(
			List<MoleculeMx> molList)
		{
			MoleculeMx mol;
				int corpId, molSvgsFetchedCount = 0;
				string corpIdString, molString, svg;

				const string sql = @"
		SELECT 
      corp_nbr,
      molstructure svgString
    FROM 
			mbs_owner.corp_moltable_mx
    WHERE 
      corp_nbr in (<list>)
			and molstructure is not null
		";

				if (!Security.UserInfo.Privileges.CanRetrieveStructures) // structures allowed?
					return 0;

				//if (DebugMx.True) return 0; // debug, don't use existing values

				List<string> lsnList = new List<string>();
				Dictionary<string, MoleculeMx> molDict = new Dictionary<string, MoleculeMx>();

				foreach (MoleculeMx mol0 in molList) // set up a dict keyed by cid with mol values
				{
					if (mol0.PrimaryFormat != MoleculeFormat.Helm || Lex.IsUndefined(mol0.PrimaryValue))
						continue;

					if (int.TryParse(mol0.Id, out corpId))
						molDict[mol0.Id] = mol0;
				}

				if (molDict.Count == 0) return 0;

				DbCommandMx cmd = new DbCommandMx();
				cmd.PrepareListReader(sql, DbType.String);
				cmd.ExecuteListReader(new List<string>(molDict.Keys));

				while (cmd.Read())
				{
					corpId = cmd.GetInt(0); // lilly_nbr

					if (!cmd.IsNull(1)) // molstructure
					{
						molString = cmd.GetClob(1);

						if (!SvgUtil.IsSvgString(molString)) continue; // skip if not SVG

						svg = molString; // should be compressed format SVG

						corpIdString = CompoundId.Normalize(corpId.ToString());

						if (Lex.IsDefined(svg) && molDict.ContainsKey(corpIdString))
						{
							mol = molDict[corpIdString];
							mol.SvgString = svg;
							molSvgsFetchedCount++;
						}
					}
				}

				cmd.CloseReader();

				return molSvgsFetchedCount;
			}

			/// <summary>
			/// Sync the Mobius CorpMoltable replicate used to retrieve Smiles
			/// Syntax: UpdateCorpDbMoltableMx [ ByDateRange | ByCorpIdRange | LoadMissing | <singleCorpId>]
			/// </summary>
			/// <returns></returns>

			public static string UpdateCorpDbMoltableMx(
			string args)
		{
			DateTime moleculeDateTime = DateTime.MinValue;
			double mw;
			string msg = "", sql = "", maxCorpIdSql, mf = "", chime = "", smiles = "", checkPointDate = "", helm = "", sequence = "", svg = "";
			object[][] pva = null;
			int pvaCount = 0, CorpId, lowCorpId = 0, highCorpId = 0, srcMaxCorpId = 0;

			int SelectChunkSize = 20; // small chunks
			int InsertBufferSize = 10; 

			//int SelectChunkSize = 100000; // big chunks
			//int InsertBufferSize = 1000;

			// Select data from corp_moltable by CorpId range

			const string SelectByCorpIdRange = @" 
		SELECT 
        m.corp_nbr,
        chime(m.ctab), 
        m.molformula,
        m.molweight,
        null molsmiles,
        s.helm_txt,
        s.sequence_txt,
        m.molecule_date
    FROM 
        corp_owner.corp_moltable m,
        corp_owner.corp_substance s
    where 
        m.corp_nbr > 0
        and s.corp_nbr = m.corp_nbr
        and (s.status_code is null or s.status_code = 'A')    
    ORDER BY corp_nbr";

		// Select data from corp_moltable by date range comparing to corp_moltable_mx

		const string SelectByDateRange = @"
			select
					m.corp_nbr,
					chime(m.ctab), 
					m.molformula,
					m.molweight,
          null molsmiles,
	        s.helm_txt,
					s.sequence_txt,
					m.molecule_date,
					m2.molecule_date
			from
					corp_owner.corp_moltable m,
					corp_owner.corp_substance s,
					corp_moltable_mx m2
			where
					m.molecule_date > to_date('1-jan-1900 000000','DD-MON-YYYY HH24MISS')
					and s.corp_nbr = M.CORP_NBR
					and (s.status_code is null or s.status_code = 'A')    
					and m2.corp_nbr (+) = m.corp_nbr
					and m2.molecule_date (+) != m.molecule_date
			order by m.molecule_date";

		// Select for missing smiles strings, ex: Update CorpDbMoltableMx LoadMissing mx.molecule_date > '1-jan-2014'

		const string SelectMissingSmilesFix = @"
			select /* check for CorpIds in corp_moltable not in corp_moltable_mx */
					corp_nbr,
					chime(ctab), 
					molformula,
					molweight,
					null molsmiles,
					helm_txt,
          sequence_txt,
          molecule_date
			from 
					(
					select 
						m.*, 
						s.helm_txt,
						s.sequence_txt,
						mx.molsmiles molsmiles_mx
					from
					 corp_owner.corp_moltable m,
					 corp_owner.corp_substance s,
					 corp_moltable_mx mx
					where
					 s.corp_nbr = M.CORP_NBR
					 and (s.status_code is null or s.status_code = 'A')
					 and mx.corp_nbr (+) = m.corp_nbr
					 and 1=1 /* condition to substitute */
					) m
			where molsmiles_mx is null /* extra condition */
			order by corp_nbr";

// Insert missing helm info

			const string SelectMissingHelmFix = @"
			select /* check for CorpIds in corp_moltable not in corp_moltable_mx */
					corp_nbr,
					chime(ctab), 
					molformula,
					molweight,
					null molsmiles,
					helm_txt,
          sequence_txt,
          molecule_date
			from 
					(
					select 
						m.*, 
						s.helm_txt,
						s.sequence_txt,
						mx.molsmiles molsmiles_mx
					from
					 corp_owner.corp_moltable m,
					 corp_owner.corp_substance s,
					 corp_moltable_mx mx
					where
					 s.corp_nbr = M.CORP_NBR
					 and (s.status_code is null or s.status_code = 'A')
					 and mx.corp_nbr (+) = m.corp_nbr
					 and 1=1 /* condition to substitute */
					) m
			where length(helm_txt) > 0 /* extra condition */
			order by corp_nbr";

			// Secondary "large" structure table (~5k mols)

			const string SelectLargeMols = @"
			select 
				corp_nbr, 
				to_clob(molstructure), 
				to_clob(molformula), 
				molweight,
				molsmiles,
				null helm_txt,
				null sequence_txt,
				molecule_date
			from
			(select
				corp_srl_nbr corp_nbr,
				'CompoundId=' || corp_srl_nbr molstructure, 
				null ctab,
				mlclr_frml_txt molformula,
				mlclr_wgt molweight,
				null molsmiles,
				null molecule_date
				from rdm_owner.rdm_sbstnc 
				where rdw_src_cd = 'LRG'";

// Insert statement

		const string InsertSql = @"
			insert into mbs_owner.corp_moltable_mx (
				corp_nbr,
				molstructure,
				molformula,
				molweight,
				molsmiles,
				molecule_date)
			values (:0, :1, :2, :3, :4, :5)";

// Build select sql

			bool byDateRange = false, byCorpIdRange = false, missingFix = true, deleteExisting = true;
			string missingFixCriteria = "";

			if (Lex.IsUndefined(args) || Lex.Eq(args, "ByDateRange"))
			{
				byDateRange = true;
			}

			else if (Lex.Eq(args, "ByCorpIdRange"))
			{
				byCorpIdRange = true;

				Progress.Show("Getting range of CorpIds to insert...");
				maxCorpIdSql = "select max(corp_nbr) from corp_owner.corp_moltable"; // get highest CorpId in source db
				srcMaxCorpId = SelectSingleValueDao.SelectInt(maxCorpIdSql);
				if (srcMaxCorpId < 0) srcMaxCorpId = 0;

				maxCorpIdSql = "select max(corp_nbr) from mbs_owner.corp_moltable_mx"; // get highest CorpId in dest db
				highCorpId = SelectSingleValueDao.SelectInt(maxCorpIdSql);
				if (highCorpId < 0) highCorpId = 0;
			}

			else if (Lex.StartsWith(args, "LoadMissing"))
			{
				missingFix = true;
				if (args.Contains(" "))
					missingFixCriteria = args.Substring(10).Trim();
			}

			else if (int.TryParse(args, out srcMaxCorpId)) // single CorpId
			{
				byCorpIdRange = true;
				highCorpId = srcMaxCorpId - 1; // say 1 less is the max we have
			}

			else return "Syntax: UpdateCorpDbMoltableMx [ ByDateRange | ByCorpIdRange | LoadMissing | <singleCorpId>]";

			Log("UpdateCorpDbMoltableMx started: " + args);

			int readCount = 0, insCount = 0, insertCount = 0, updateCount = 0, undefinedStructures = 0, smilesSuccess = 0, smilesFails = 0, helmStructures = 0;
			List<string> CorpIdList = new List<string>();

			for (int chunk = 1; ; chunk++) // loop over chunks
			{

				if (byDateRange) // single chunk
				{
					if (chunk > 1) break; // break 2nd time through

					checkPointDate = UserObjectDao.GetUserParameter("MOBIUS", "UpdateCorpDbMoltableMxCheckpointDate", "01-sep-2013 000000");

					//UserObjectDao.SetUserParameter("MOBIUS", "UpdateCorpDbMoltableMxCheckpointDate", checkPointDate);

					sql = Lex.Replace(SelectByDateRange, "1-jan-1900 000000", checkPointDate);

					msg = "Reading where date >= " + checkPointDate;
				}

				else if (byCorpIdRange) // by CorpId range
				{
					if (highCorpId >= srcMaxCorpId) break; // done

					lowCorpId = highCorpId + 1; // start of next chunk
					highCorpId = lowCorpId + SelectChunkSize;
					if (highCorpId >= srcMaxCorpId) highCorpId = srcMaxCorpId;
					sql = Lex.Replace(SelectByCorpIdRange, "corp_nbr > 0", "corp_nbr between " + lowCorpId + " and " + highCorpId);

					msg = "Reading: " + lowCorpId + " to " + highCorpId + ", Reads: " + readCount + ", Inserts: " + insertCount ;
				}

				else if (missingFix)
				{
					if (chunk > 1) break; // break 2nd time through

					sql = SelectMissingHelmFix;
					if (Lex.IsDefined(missingFixCriteria)) // substitute any criteria
						sql = Lex.Replace(sql, "1=1", missingFixCriteria);
					msg = "Fixing missing data";
				}

				Progress.Show(msg);

				DbCommandMx readCmd = new DbCommandMx();
				readCmd.MxConn = DbConnectionMx.Get("prd123");
				readCmd.PrepareUsingDefinedConnection(sql, null);
				DbDataReader rdr = readCmd.ExecuteReader();

				DbCommandMx insertCmd = new DbCommandMx();

				OracleDbType[] pta = new OracleDbType[6];
				pta[0] = OracleDbType.Int32;      // corp_nbr
				pta[1] = OracleDbType.Clob; // molstructure
				pta[2] = OracleDbType.Clob; // molformula
				pta[3] = OracleDbType.Double; // molweight
				pta[4] = OracleDbType.Clob; // smiles
				pta[5] = OracleDbType.Date; // molecule_date

				insertCmd.Prepare(InsertSql, pta);
				insertCmd.BeginTransaction(); // be sure we have a transaction going

				pva = DbCommandMx.NewObjectArrayArray(6, InsertBufferSize); // alloc insert row array
				object[] vo = new object[6];

				while (true)
				{
					bool readOk = rdr.Read();

					if (readOk)
					{
						rdr.GetValues(vo);

						CorpId = readCmd.GetInt(0); // corp_nbr
						vo[0] = CorpId;
						CorpIdList.Add(CorpId.ToString());

						if (!readCmd.IsNull(1)) // molstructure
						{
							chime = readCmd.GetClob(1);
							chime = OracleDao.ClearStringIfExceedsMaxStringSize(chime);
							vo[1] = chime;
						}
						else chime = "";

						if (!readCmd.IsNull(2)) // molformula
						{
							mf = readCmd.GetClob(2);
							mf = OracleDao.ClearStringIfExceedsMaxStringSize(mf);
							vo[2] = mf;
						}

						if (!readCmd.IsNull(3)) // molweight
						{
							mw = readCmd.GetDouble(3);
							vo[3] = mw;
						}

						if (Lex.IsDefined(chime)) // molsmiles - calculate from chime string
						{
							MoleculeMx cs = new MoleculeMx(MoleculeFormat.Chime, chime);
							if (cs.AtomCount > 1) // need more than one atom
							{
								MoleculeMx cs2 = cs.ConvertTo(MoleculeFormat.Smiles);
								smiles = cs2.GetSmilesString();
								if (Lex.IsDefined(smiles)) smilesSuccess++;
								else
								{
									Log("Smiles conversion failure for CorpId: " + CorpId);
									smilesFails++;
								}
								smiles = OracleDao.ClearStringIfExceedsMaxStringSize(smiles);

								vo[4] = smiles;
							}
							else undefinedStructures++;
						}
						else undefinedStructures++;

						if (!readCmd.IsNull(5))
						{
							helm = readCmd.GetClob(5);
							if (Lex.IsDefined(helm))
							{
								svg = HelmControl.GetSvg(helm);
								vo[1] = SvgUtil.CompressToBase64String(svg); // store compressed svg in molstructure column for now
								helmStructures++;
							}
						}

						if (!readCmd.IsNull(6))
						{
							sequence = readCmd.GetClob(6);
							if (Lex.IsDefined(sequence))
							{
								// nothing yet
							}
						}

						moleculeDateTime = DateTime.MinValue;
						if (!readCmd.IsNull(7)) // molecule_date
						{
							moleculeDateTime = readCmd.GetDateTime(7);
							vo[5] = moleculeDateTime;
						}

						for (int pi = 0; pi < 6; pi++) // invert for insert
							pva[pi][pvaCount] = vo[pi];

						if (Debug)
						{
							msg = String.Format("CorpId: {0}, mf: {1}, chime: {2}, smiles: {3}", CorpId.ToString(), mf.Length, chime.Length, smiles.Length);
							Log(msg);
						}

						pvaCount++;
					}

					if (pvaCount >= InsertBufferSize || (!readOk && pvaCount > 0)) // write if buffer full or at end
					{
						try
						{
							if (deleteExisting)
							{
								int delCount = DoDeletes(CorpIdList);
								updateCount += delCount; // count deletes as updates
								insertCount -= delCount; // subtract from inserts
							}
							CorpIdList.Clear();

							insCount = insertCmd.ExecuteArrayNonReader(pva, ref pvaCount);
							insertCmd.Commit();
							insertCmd.BeginTransaction();
							insertCount += insCount;
						}

						catch (Exception ex)
						{
							throw new Exception(ex.Message, ex);
						}

						if (byDateRange)
						{
							string checkPointDate2 = String.Format("{0:dd-MMM-yyyy HHmmss}", moleculeDateTime); // format date time that will work with oracle
							UserObjectDao.SetUserParameter("MOBIUS", "UpdateCorpDbMoltableMxCheckpointDate", checkPointDate2);
							msg = "Processing where date >= " + checkPointDate + ", Reads: " + readCount + ", Inserts: " + insertCount + ", Updates: " + updateCount;
						}

						else if (byCorpIdRange) // CorpId range
							msg = "Processing: " + lowCorpId + " to " + highCorpId + ", Reads: " + readCount + ", Inserts: " + insertCount;

						else if (missingFix)
							msg = "Fixing missing smiles, Updates: " + updateCount;

						msg += String.Format(", Undefined structures: {0} , Smiles failures: {1}, Helms: {2}", undefinedStructures, smilesFails, helmStructures);

						Progress.Show(msg);
					}

					if (!readOk) break;

					readCount++;
				}

				readCmd.Dispose();
				insertCmd.Dispose();

			} // end for select chunk

			msg = "UpdateCorpDbMoltableMx - Inserts: " + insertCount + ", Updates: " + updateCount;
			msg += String.Format(", Undefined structures: {0} , Smiles failures: {1}, Helms: {2}", undefinedStructures, smilesFails, helmStructures);
			Log(msg);

			return msg;
		}

		static int DoDeletes(List<string> CorpIdList)
		{
			StringBuilder sb = new StringBuilder();
			int delCount = 0;
			int li = -1;

			while (li + 1 < CorpIdList.Count)
			{
				sb.Length = 0;
				int sbCorpIdCnt = 0;
				for (li = li + 1; li < CorpIdList.Count; li++)
				{
					if (sb.Length > 0) sb.Append(",");
					sb.Append(CorpIdList[li]);
					sbCorpIdCnt++;
					if (sbCorpIdCnt >= 1000) break;
				}

				string sql =
					"delete from mbs_owner.corp_moltable_mx " +
					"where corp_nbr in (" + sb + ")";

				int delCount2 = DbCommandMx.PrepareAndExecuteNonReaderSql(sql);
				delCount += delCount2;
			}

			return delCount;
		}

/// <summary>
/// Log message
/// </summary>
/// <param name="msg"></param>
/// 
		static void Log(string msg)
		{
			string logFileName = "UpdateCorpDbMoltableMx.log";
			logFileName = ServicesDirs.LogDir + @"\" + logFileName;
			DebugLog.MessageDirect(msg, logFileName);
		}

	}
}
