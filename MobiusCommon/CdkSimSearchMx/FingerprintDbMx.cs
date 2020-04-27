using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;
using Mobius.CdkSearchMx;
using Mobius.CdkMx;

using cdk = org.openscience.cdk;
using org.openscience.cdk;
using org.openscience.cdk.inchi;
using org.openscience.cdk.interfaces;
using org.openscience.cdk.tools.manipulator;
using org.openscience.cdk.graph;
using org.openscience.cdk.qsar.result;
using org.openscience.cdk.io;
using org.openscience.cdk.io.iterator;
using org.openscience.cdk.fingerprint;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Mobius.CdkSearchMx
{

	/// <summary>
	/// Build / maintain Mobius Corp/ChEMBL similarity-search database
	/// </summary>

	public class FingerprintDbMx
	{
		static string SyntaxMsg = 
		"Syntax: Update FingerprintDatabaseMx [Corp | ChEMBL] [MACCS | ECFP4] [Load | ByIdRange | SinceLastCheckpoint | LoadMissing | <singleId>]";

		static List<String> CidList; // working list of cids that need to be added or updated in the database
		static Dictionary<string, DateTime> CidUpdateDateDict; // dictionary of cids with last update date
		static int CidListOriginalCount = -1;
		static HashSet<string> ExistingUndefinedStructureCids; // list of examined but undefined cids 
		static string CheckpointDateTime; // where we are continuing from
		static DateTime MoleculeDateTime = DateTime.MinValue;
		static Dictionary<string, string> Failures;
		static List<string> NewUndefinedStructureCids;
		static string LastFailure = "";
		static int FailureCount = 0;

		static bool ByCheckpoint = false, ByCidRange = false, ByCidList = false, LoadIfMissing = false;

		const int DefaultReadChunkSize = 100000; // number of cids/rows to read per sql statement
		static int ReadChunkSize = DefaultReadChunkSize; 

		const int DefaultWriteChunkSize = 1000; // number or rows to buffer before writing to database files (and closing and merging).
		static int WriteChunkSize = DefaultWriteChunkSize;

		static FingerprintDao FpDao; // used for reading/writing fingerprint files

		public static string[] Databases = new string []{"Corp", "ChEMBL"};
		public static bool CorpDatabase { get { return Lex.Eq(Database, "Corp"); } }
		public static bool ChemblDatabase { get { return Lex.Eq(Database, "Chembl"); } }

		static string Database = ""; // current database

		static FingerprintType FingerprintType = FingerprintType.Undefined;

		public static int SrcDbId
		{
			get
			{
				if (CorpDatabase) return 0;
				else if (ChemblDatabase) return 1;
				else throw new InvalidDataException();
			}
		}

		/// <summary>
		/// UpdateCorpFingerprintDatabaseMx
		/// ///////////////////////////////////////////////////////
		/// Syntax: Update FingerprintDatabaseMx [Corp | ChEMBL] [MACCS | ECFP4] [Load | ByCidRange | SinceLastCheckpoint | LoadMissing | <SingleCorpId>]
		/// 
		/// Corp Examples:
		///    Update FingerprintDatabaseMx Corp MACCS Load
		///    Update FingerprintDatabaseMx Corp MACCS LoadMissing
		///    Update FingerprintDatabaseMx Corp MACCS SinceLastCheckpoint
		///    
		///    Update FingerprintDatabaseMx Corp ECFP4 Load
		///    Update FingerprintDatabaseMx Corp ECFP4 LoadMissing
		///    Update FingerprintDatabaseMx Corp ECFP4 SinceLastCheckpoint
		/// 
		/// ChEMBL Examples:
		///    Update FingerprintDatabaseMx Chembl MACCS Load
		///    Update FingerprintDatabaseMx Chembl MACCS LoadMissing
		///    
		///    Update FingerprintDatabaseMx Chembl ECFP4 Load
		///    Update FingerprintDatabaseMx Chembl ECFP4 LoadMissing
		/// ///////////////////////////////////////////////////////
		/// </summary>
		/// <param name="argString"></param>
		/// <returns></returns>

		static public string Update(
			string argString)
		{
			IAtomContainer mol;
			double mw;
			string chime, smiles, molString, molFile = "";
			string msg = "", sql = "", chemblId, cid = "", maxCorpIdSql, maxIdSql2, mf, missingFixCriteria = "", CorpIdList = "";
			int storeChunkCount = 0, CorpId, molregno, molId, lowId = 0, highId = 0, maxDestId = 0, maxSrcId = 0;
			int readCount = 0, storeCount = 0;

			ByCheckpoint = ByCidRange = ByCidList = LoadIfMissing = false;
			ReadChunkSize = DefaultReadChunkSize;
			WriteChunkSize = DefaultWriteChunkSize;

			Failures = new Dictionary<string, string>();
			NewUndefinedStructureCids = new List<string>();

			LastFailure = "";
			FailureCount = 0;

			// global try loop

			try
			{
				////////////////////////
				/// Parse Parameters ///
				////////////////////////

				// See which database

				argString = argString.Trim();
				if (Lex.StartsWith(argString, "Corp"))
				{
					Database = "Corp";
					argString = argString.Substring(5).Trim();
				}

				else if (Lex.StartsWith(argString, "Chembl"))
				{
					Database = "ChEMBL";
					argString = argString.Substring(6).Trim();
				}

				else return SyntaxMsg;

				// See which fingerprint type

				FingerprintType = FingerprintType.MACCS; // default to MACCS if type not defined

				if (Lex.TryReplace(ref argString, "MACCS", ""))
				{
					FingerprintType = FingerprintType.MACCS;
					argString = argString.Trim();
				}

				else if (Lex.TryReplace(ref argString, "ECFP4", ""))
				{
					FingerprintType = FingerprintType.Circular; // (i.e. ECFP4)
					argString = argString.Trim();
				}

				FpDao = new FingerprintDao(Database, FingerprintType);
				List<FingerprintRec> fpRecList = new List<FingerprintRec>();

				string args = argString.Trim();

				string initialMsg = "Update FingerprintDatabase started: " + args;

				CidList = new List<string>(); // init empty list

				//////////////////////
				/// Corp Database ///
				//////////////////////

				if (CorpDatabase)
				{

					if (Lex.Eq(args, "Load"))
					{
						ByCidRange = true;

						ShowProgress("Getting range of CorpIds to insert...");

						maxCorpIdSql = SelectMaxCorpId; // get highest id in source db
						maxSrcId = SelectSingleValueDao.SelectInt(maxCorpIdSql);
						if (maxSrcId < 0) maxSrcId = 0;

						maxDestId = GetMaxDestMolId();

						//maxIdSql2 = "select max(src_compound_id_nbr) from dev_mbs_owner.corp_uc_xref where src_id = 0"; // get highest id in UniChemDb db
						//highCorpId = SelectSingleValueDao.SelectInt(maxIdSql2);

						if (maxDestId < 0) maxDestId = 0;
					}

					else if (Lex.Eq(args, "SinceLastCheckpoint"))
					{
						ByCheckpoint = true;

						ShowProgress("Getting list of CorpIds updated since last checkpoint...");

						CidList = GetNewAndModifiedCorpIdList(out CidUpdateDateDict);

						//CidUpdateList = new List<string>(); // debug with single cmpd
						//CidUpdateList.Add("03435269");

						if (CidList.Count == 0) return "There have been no updates since the last checkpoint";

						initialMsg += ", CorpIds to add/update: " + CidList.Count;
					}

					else if (Lex.StartsWith(args, "ByCorpIdList"))
					{
						ByCidList = true;
						CorpIdList = args.Substring("ByCorpIdList".Length).Trim();
						if (Lex.IsUndefined(CorpIdList)) throw new Exception("Undefined CorpId list");
					}

					else if (Lex.StartsWith(args, "LoadMissing"))
					{
						LoadIfMissing = true;
						if (args.Contains(" "))
							missingFixCriteria = args.Substring("LoadMissing".Length).Trim();

						ShowProgress("Getting list of missing CorpIds...");
						CidList = GetMissingCidList();
						if (CidList.Count == 0) return "There are no missing CorpIds";
						initialMsg += ", Missing CorpIds: " + CidList.Count;
					}

					else if (int.TryParse(args, out maxSrcId)) // single CorpId
					{
						ByCidRange = true;
						maxDestId = maxSrcId - 1; // say 1 less is the max we have
					}

					else return SyntaxMsg;
				}

				///////////////////////
				/// ChEMBL Database ///
				///////////////////////

				else if (ChemblDatabase)
				{
					if (Lex.Eq(args, "Load"))
					{
						ByCidRange = true;

						ShowProgress("Getting range of MolRegNos to insert...");

						sql = "select max(molregno) from chembl_owner.compound_struct_xxxxxx";
						maxSrcId = SelectSingleValueDao.SelectInt(sql);
						if (maxSrcId < 0) maxSrcId = 0;

						maxDestId = GetMaxDestMolId();
						if (maxDestId < 0) maxDestId = 0;
					}

					else if (Lex.StartsWith(args, "LoadMissing"))
					{
						LoadIfMissing = true;
						ShowProgress("Getting list of missing ChEMBL Ids...");
						CidList = GetMissingCidList();
						if (CidList.Count == 0) return "There are no missing Ids";
						initialMsg += ", Missing Chembl Ids: " + CidList.Count;
					}

					else return SyntaxMsg;
				}

				else return SyntaxMsg;

				CidListOriginalCount = CidList.Count;

				Log(initialMsg);

				/////////////////////////////
				// Loop over chunks of data
				/////////////////////////////

				for (int chunk = 1; ; chunk++)
				{

					//////////////////////
					/// Corp Database ///
					//////////////////////

					if (CorpDatabase)
					{
						if (ByCheckpoint) // single chunk
						{
							string cidList = GetNextListChunk();
							if (Lex.IsUndefined(cidList)) break;

							sql = SelectByCorpIdCriteria;
							sql = Lex.Replace(sql, "<CorpIdCriteria>", "in (" + cidList + ")");
							string matchString = "order by m.corp_nbr";
							if (!Lex.Contains(sql, matchString)) throw new Exception(matchString + " not found");
							sql = Lex.Replace(sql, matchString, "order by m.molecule_date");

							msg = "Processing " + CidListOriginalCount + " updates since " + CheckpointDateTime;
							// + " (" + Mobius.Data.CidList.FormatCidListForDisplay(null, chunkCidList) + ")";
						}

						else if (ByCidRange) // by CorpId range
						{
							if (maxDestId >= maxSrcId) break; // done

							lowId = maxDestId + 1; // start of next chunk
							highId = lowId + ReadChunkSize;
							maxDestId = highId;

							//lowCorpId = highCorpId = 12345; // debug

							if (highId >= maxSrcId) highId = maxSrcId;
							sql = SelectByCorpIdCriteria;
							sql = Lex.Replace(sql, "<CorpIdCriteria>", "between " + lowId + " and " + highId);

							msg = "Processing CorpId range: " + lowId + " to " + highId;
						}

						else if (ByCidList) // by single user-supplied CorpId list
						{
							if (chunk > 1) break; // break 2nd time through

							sql = SelectByCorpIdCriteria;
							sql = Lex.Replace(sql, "<CorpIdCriteria>", "in (" + CorpIdList + ")");
							msg = "Processing CorpId list: " + CorpIdList;
						}

						else if (LoadIfMissing)
						{
							string cidList = GetNextListChunk();
							if (Lex.IsUndefined(cidList)) break; // all done

							sql = SelectByCorpIdCriteria;
							sql = Lex.Replace(sql, "<CorpIdCriteria>", "in (" + cidList + ")");

							msg = "Processing missing CorpId Chunk: " + Mobius.Data.CidList.FormatAbbreviatedCidListForDisplay(null, cidList) +
								", Total Ids: " + CidListOriginalCount;

							Log(msg);
						}

						else return SyntaxMsg;
					}

					///////////////////////
					/// ChEMBL Database ///
					///////////////////////
					
					else if (ChemblDatabase)
					{
						if (ByCidRange) // by CID range
						{
							if (maxDestId >= maxSrcId) break; // done

							lowId = maxDestId + 1; // start of next chunk
							highId = lowId + ReadChunkSize;
							maxDestId = highId;

							//lowId =  highId = 12345; // debug

							if (maxDestId >= maxSrcId) maxDestId = maxSrcId;
							sql = SelectChemblSql;
							sql = Lex.Replace(sql, "<molregnoCriteria>", "between " + lowId + " and " + highId);

							msg = "Processing ChEMBL MolRegNo range: " + lowId + " to " + highId;
						}

						else if (LoadIfMissing)
						{
							string cidList = GetNextListChunk();
							if (Lex.IsUndefined(cidList)) break; // all done

							sql = SelectByCorpIdCriteria;
							sql = Lex.Replace(sql, "<CorpIdCriteria>", "in (" + cidList + ")");

							msg = "Processing missing ChEMBL Id Chunk: " + Mobius.Data.CidList.FormatAbbreviatedCidListForDisplay(null, cidList) +
								", Total Ids: " + CidListOriginalCount;
						}

						else return SyntaxMsg;
					}

					else return SyntaxMsg;

					ShowProgress(msg);

					// Execute the SQL to get the rows for the chunk 

					DbCommandMx rdr = DbCommandMx.PrepareAndExecuteReader(sql);
					DateTime lastShowProgressTime = DateTime.MinValue;

					///////////////////////////////////////////
					/// Loop over rows in the current chunk ///
					///////////////////////////////////////////

					while (true)
					{

						// Update progress display

						if (DateTime.Now.Subtract(lastShowProgressTime).TotalSeconds > 1) // show progress
						{
							int storeTotalCount = storeCount + storeChunkCount;

							string msg2 = msg + "\r\n" +
							"Reads: " + readCount + "\r\n" +
							"Undefined: " + NewUndefinedStructureCids.Count + "\r\n" +
							"Insert/Updates: " + storeTotalCount + "\r\n" +
							"Failures: " + FailureCount + "\r\n" +
							"Failure Types: " + Failures.Count + "\r\n" +
							"Last Failure: " + LastFailure;

							ShowProgress(msg2);
							lastShowProgressTime = DateTime.Now;
						}

						// Read and process next compound

						bool readOk = rdr.Read();

						if (readOk)
						{
							readCount++;

							try
							{
								double t1 = 0, t2 = 0, t3 = 0, t4 = 0;
								DateTime t0 = DateTime.Now;
								mol = null;
								//t2 = TimeOfDay.Delta(ref t0);

								//////////////////////
								/// Corp Database ///
								//////////////////////

								if (CorpDatabase)
								{
									CorpId = rdr.GetInt(0); // corp_nbr
									//Log("CorpId: " + CorpId); // debug
									molId = CorpId;
									cid = CorpId.ToString();
									cid = CompoundId.NormalizeForDatabase(cid);

									if (!rdr.IsNull(1)) // be sure chime field isn't null
									{
										chime = rdr.GetClob(1);
										if (Lex.IsDefined(chime))
										{
											molFile = MolLib1.StructureConverter.ChimeStringToMolfileString(chime); // convert Chime to MolFile
											mol = CdkUtil.MolfileToAtomContainer(molFile);
										}
									}

									MoleculeDateTime = rdr.GetDateTimeByName("Molecule_Date"); // Date molecule was updated in the CorpDB cartridge DB
								}

								///////////////////////
								/// ChEMBL Database ///
								///////////////////////

								else // chembl
								{
									molId = molregno = rdr.GetInt(0);
									cid = chemblId = rdr.GetString(1);
									smiles = rdr.GetString(2);
									if (Lex.IsDefined(smiles))
										mol = CdkUtil.SmilesToAtomContainer(smiles);
								}

								if (mol == null || mol.isEmpty() || mol.getAtomCount() <= 1)
								{
									NewUndefinedStructureCids.Add(cid);
									continue;
									//mol = new AtomContainer(); // write empty structure 
								}

								bool includeOverallFingerprint = true;
								List<BitSetFingerprint> fps = CdkUtil.BuildBitSetFingerprints(mol, includeOverallFingerprint, FingerprintType);

								//t3 = TimeOfDay.Delta(ref t0);

								foreach (BitSetFingerprint fp in fps)
								{
									FingerprintRec fpr = new FingerprintRec();
									fpr.molId = molId;
									fpr.SrcId = SrcDbId;
									fpr.Cid = cid;
									fpr.Cardinality = fp.cardinality();
									fpr.Fingerprint = fp.asBitSet().toLongArray();
									fpRecList.Add(fpr);
								}

								//t4 = TimeOfDay.Delta(ref t0);
								t4 = t4;
							}

							catch (Exception ex)
							{
								if (!Failures.ContainsKey(ex.Message))
									Failures.Add(ex.Message, cid);

								else Failures[ex.Message] += ", " + cid;

								LastFailure = "Cid: " + cid + " - " + ex.Message;

								Log(LastFailure);

								//ShowProgress(ex.Message + "\r\n" + ex.StackTrace.ToString()); // debug

								FailureCount++;

								continue;
							}

							storeChunkCount++;
						}

						bool commitTransaction = (storeChunkCount >= WriteChunkSize || (!readOk && storeChunkCount > 0));
						if (commitTransaction) // end of chunk of data to store?
						{

							// if updating by CheckPoint date range then merge existing data with new/updated data

							if (ByCheckpoint)
							{
								if (readCount > 0 && (storeCount > 0 || FailureCount == 0)) // make sure not everything has failed)
								{
									MergeRecordsIntoFiles(fpRecList);
								}
							}

							// Simple append of records to files

							else
							{
								FpDao.OpenWriters("bin", FileMode.Append); // open bin files for append

								foreach (FingerprintRec fpr in fpRecList) // write out buffered recs
									FpDao.WriteFingerprintRec(fpr);

								FpDao.CloseWriters();

								int cnt = fpRecList.Count;
								if (cnt > 0)
								{
									string cid1 = fpRecList[0].Cid;
									string cid2 = fpRecList[cnt - 1].Cid;
									Log("Records Appended: " + cnt + ", CIDS: " + cid1 + " - " + cid2);
								}
								else Log("Records Appended: 0");
							}

							fpRecList.Clear();

							storeCount += storeChunkCount;
							storeChunkCount = 0;
						}


						if (!readOk) break;

					} // end of read loop for rows in a chunk

					rdr.Dispose();

				} // end for loop of chunks

				DeleteTempFiles();

				if (LoadIfMissing) // update list of cids with missing structures
				{
					ExistingUndefinedStructureCids.UnionWith(NewUndefinedStructureCids);
					FpDao.WriteUndefinedStructuresCids(ExistingUndefinedStructureCids);
				}

				msg = "*** Update Complete ***\r\n\r\n" + msg;
				ShowProgress(msg);
				System.Threading.Thread.Sleep(100);

				string logMsg = "UpdateFingerprintDb - CIDs stored: " + storeCount + ", Undefined structures: " + NewUndefinedStructureCids.Count + ", failures: " + FailureCount + "\r\n";

				foreach (string key in Failures.Keys)
					logMsg += key + " - CIDs: " + Failures[key] + "\r\n";

				Log(logMsg);

				return logMsg;
			} // end of main try loop

			catch (Exception ex)
			{
				Log(DebugLog.FormatExceptionMessage(ex));
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// Merge existing .bin files with new records
		/// 1. Copy .bin files to .mrg files filtering out cids that have been updated
		/// 2. Append new records to .mrg files
		/// 3. Rename .bin files to .bak files and .mrg files to new .bin files
		/// </summary>

		static void MergeRecordsIntoFiles(
			List<FingerprintRec> fpRecList)
		{
			string date1, date2;
			Progress.Show("Merging existing and new files...");

			// Copy existing bin file entries filtering out cids that were updated

			FpDao.OpenReaders("bin"); // open existing .bin files for input
			FpDao.OpenWriters("mrg", FileMode.Create); // open new merge files for output

			HashSet<string> fpRecHash = new HashSet<string>(); // build a hash of cids that shouldn't be copied
			foreach (FingerprintRec fpr in fpRecList)
				fpRecHash.Add(fpr.Cid);

			int replacementCnt = 0;
			for (int fi = 0; fi < FingerprintDao.FingerprintFileCount; fi++) // copy each file
			{
				BinaryWriter bw = FpDao.BinaryWriters[fi];

				while (true)
				{
					FingerprintRec fpr = FpDao.ReadFingerprintRec(fi);
					if (fpr == null) break;

					if (fpRecHash.Contains(fpr.Cid))
					{
						//Log("Removing " + fpr.Cid); // debug
						replacementCnt++;
						continue; // skip if this cid is was updated in the incoming list
					}

					FpDao.WriteFingerprintRec(bw, fpr);
				}
			}

			FpDao.CloseReaders();

			// Append the new records to the merge files

			foreach (FingerprintRec fpr in fpRecList) // write out buffered recs
				FpDao.WriteFingerprintRec(fpr);

			FpDao.CloseWriters();

			// Backup old files and activate new files

			for (int fi = 0; fi < FingerprintDao.FingerprintFileCount; fi++) // check that we can backup all bin files
			{
				string fileName = FpDao.GetFpFileName(fi) + ".bin";
				bool backupOk = FileUtil.CanRename(fileName);
				if (!backupOk) throw new Exception("Unable to rename file: " + fileName + ", aborting update");
			}

			for (int fi = 0; fi < FingerprintDao.FingerprintFileCount; fi++) 
			{
				string fileName = FpDao.GetFpFileName(fi);
				bool backupOk = FileUtil.BackupAndReplaceFile(fileName + ".bin", fileName + ".bak", fileName + ".mrg");
				if (!backupOk) throw new Exception("Error replacing file: " + fileName + ".bin");
			}

			if (ByCheckpoint)
			{
				string checkPointDate = String.Format("{0:dd-MMM-yyyy HHmmss}", MoleculeDateTime); // format last new/updated date time in a format that works with Oracle
				FpDao.WriteFingerPrintSimMxDataUpdateCheckpointDate(checkPointDate); // update checkpoint
			}

			Progress.Show("Merging complete...");

			int cnt = fpRecList.Count;
			string cid1 = fpRecList[0].Cid;
			string cid2 = fpRecList[cnt - 1].Cid;

			if (CidUpdateDateDict.ContainsKey(cid1))
					date1 = CidUpdateDateDict[cid1].ToString();
				else date1 = "Missing";

				if (CidUpdateDateDict.ContainsKey(cid2))
					date2 = CidUpdateDateDict[cid1].ToString();
				else date2 = "Missing";

			Log("Records stored: " + cnt + ", Adds: " + (cnt - replacementCnt) + ", Replacements: " + replacementCnt + 
				", Date range: " + date1 + " - " + date2 + ", CIDs: First = " + cid1 + ", Last = " + cid2);

			return;
		}


/// <summary>
/// DeleteTempFiles
/// </summary>
		static void DeleteTempFiles()
		{
			for (int fi = 0; fi < FingerprintDao.FingerprintFileCount; fi++)
			{
				string fileName = FpDao.GetFpFileName(fi);
				FileUtil.DeleteFile(fileName + ".mrg");
			}
		}

	/// <summary>
	/// GetMaxCorpId
	/// </summary>
	/// <returns></returns>
	public static int GetMaxDestMolId()
		{
			int molId, maxMolId = 0;
			FingerprintDao fpd = new FingerprintDao(Database, FingerprintType);
			List<string> ids = new List<string>();

			if (!fpd.DataFilesExist()) return maxMolId;

			fpd.OpenReaders();

			while (true)
			{
				FingerprintRec rec = fpd.ReadFingerprintRec();
				if (rec == null) break;

				//CorpIds.Add(rec.Cid); // debug

				if (CorpDatabase) int.TryParse(rec.Cid, out molId);
				else molId = rec.molId;

				if (molId > maxMolId) maxMolId = molId;
			}

			fpd.CloseReaders();

			return maxMolId;
		}

		/// <summary>
		/// Get a list of CorpIds that have been added or modified since the checkpoint date
		/// </summary>
		/// <returns></returns>

		static List<string> GetNewAndModifiedCorpIdList(
			out Dictionary<string, DateTime> cidUpdateDateDict)
		{
			int readCnt = 0;

			List<string> cidList = new List<string>();
			cidUpdateDateDict = new Dictionary<string, DateTime>();

			CheckpointDateTime = FpDao.ReadFingerPrintSimMxDataUpdateCheckpointDate(); // , "20-mar-2016 000000");
			if (Lex.IsUndefined(CheckpointDateTime)) throw new Exception("Mobius UpdateSimMxCorpIdDataCheckpointDate is not defined");
			//CheckpointDateTime = "20-mar-2016 000000"; // debug

			string sql = Lex.Replace(SelectCorpIdsByDateRange, "1-jan-1900 000000", CheckpointDateTime);

			DbCommandMx rdr = DbCommandMx.PrepareAndExecuteReader(sql);
			while (rdr.Read())
			{
				readCnt++;
				int corpId = rdr.GetInt(0); // corp_nbr
				string cid = corpId.ToString();
				cid = CompoundId.NormalizeForDatabase(cid);
				cidList.Add(cid);

				DateTime dt = rdr.GetDateTime(1);
				cidUpdateDateDict[cid] = dt;
			}

			rdr.CloseReader();

			return cidList;
		}

		/// <summary>
		/// Get list of CIDS that are in the oracle database but not the Mobius fingerprint files
		/// </summary>
		/// <returns></returns>

		static List<string> GetMissingCidList()
		{
			string sql = "", cid;
			int readCnt = 0;

			HashSet<string> knownCidsSet = FpDao.GetExistingCidSet(); // get list of cids in DB
			ExistingUndefinedStructureCids = FpDao.ReadUndefinedStructuresCids(); // cids that have undefined structures and aren't in DB
			knownCidsSet.UnionWith(ExistingUndefinedStructureCids);

			List<string> cidList = new List<string>();

			if (CorpDatabase)
			{
				sql = SelectAllCorpIds;
				//sql = Lex.Replace(sql, "s.corp_nbr = m.corp_nbr", "s.corp_nbr = m.corp_nbr and s.corp_nbr = 3431641"); // debug
				//sql = Lex.Replace(sql, "s.corp_nbr = m.corp_nbr", "s.corp_nbr = m.corp_nbr and s.corp_nbr between 1000000 and 1100000"); // debug
			}

			else // chembl
			{
				sql = SelectAllChemblIds;
			}

			DbCommandMx rdr = DbCommandMx.PrepareAndExecuteReader(sql);
			while (rdr.Read())
			{
				readCnt++;

				if (CorpDatabase)
				{
					int corpId = rdr.GetInt(0); // corp_nbr
					cid = corpId.ToString();
					cid = CompoundId.NormalizeForDatabase(cid);
				}

				else cid = rdr.GetString(0); // chembl

				if (!knownCidsSet.Contains(cid)) cidList.Add(cid);
			}

			rdr.CloseReader();

			return cidList;
		}

/// <summary>
/// Get next chunk of cids from CidList
/// </summary>
/// <returns></returns>
		static string GetNextListChunk()
		{
			int cidsAdded = 0, li;

			StringBuilder sb = new StringBuilder();

			for (li = 0; li < CidList.Count; li++)
			{
				if (cidsAdded >= DbCommandMx.MaxOracleInListItemCount) break;

				string cid = CidList[li];
				if (Lex.IsUndefined(cid)) continue;

				if (sb.Length > 0) sb.Append(",");
				if (ChemblDatabase) cid = "'" + cid + "'";
				sb.Append(cid);
				cidsAdded++;
			}

			CidList.RemoveRange(0, li);

			return sb.ToString();
		}


		/// <summary>
		/// UpdateChemblFingerprintDatabaseMx
		/// </summary>
		/// <param name="argString"></param>
		/// <returns></returns>

		static public string UpdateChembl(
			string argString)
		{
			throw new NotImplementedException();
		}


#if false
		public static void UpdateFingerprintFiles()
		{
			string msg = "";
			long uci, lastUci = 0;
			int baseFpWriteCnt = 0, fragFpWriteCnt = 0, nullFpCnt = 0, readCnt = 0;
			int fi = 0;


			string sql = @"
				select
					x.uci, 
					x.src_id, 
					x.src_compound_id, 
					x.src_compound_id_nbr,
					s.fingerprint, 
					h.fingerprint  
				from 
					DEV_MBS_OWNER.CORP_UC_XREF x,
					DEV_MBS_OWNER.CORP_UC_STRUCTURE s,
					DEV_MBS_OWNER.CORP_UC_FIKHB_HIERARCHY h
				where 
					s.uci = x.uci
					and H.PARENT (+) = s.fikhb
					/* and s.uci between 1 and 1000 */ /* debug */
				order by s.uci";


			ShowProgress("Executing select fingerprints query...");

			DbCommandMx cmd = DbCommandMx.PrepareExecuteAndRead(sql);
			if (cmd == null) throw new Exception("No rows retrieved");
			bool readOk = true;

			BinaryWriter[] bw = FpDao.OpenFingerprintFilesForWriting();

			fi = 0;
			DateTime lastProgressUpdate = DateTime.Now;
			while (true)
			{
				if (readCnt > 0) // read next row if not first row
					readOk = cmd.Read();

				if (readOk) readCnt++;

				if (DateTime.Now.Subtract(lastProgressUpdate).TotalSeconds > 1 || !readOk) // show progress
				{
					lastProgressUpdate = DateTime.Now;

					int fpWriteCnt = baseFpWriteCnt + fragFpWriteCnt;
					msg =
						"Update Fingerprint Files\r\n" +
						"\r\n" +
						"Reads: " + readCnt + "\r\n" +
						"Fingerprints written: " + fpWriteCnt + "\r\n" +
						"Null FPs: " + nullFpCnt;

					ShowProgress(msg);
				}

				if (!readOk) break;

				uci = cmd.GetLong(0);
				int src = cmd.GetInt(1);
				string cidString = cmd.GetString(2);
				int cidInt = cmd.GetInt(3);
				byte[] fp = cmd.GetBinary(4);
				byte[] fp2 = cmd.GetBinary(5);

				if (fp == null && fp2 == null)
				{
					nullFpCnt++;
					continue;
				}

				if (uci != lastUci)
				{
					fi = (fi + 1) % FpDao.FingerprintFileCount;
					lastUci = uci;
				}

				if (fp != null)
				{
					FpDao.WriteFingerprintRec(bw[fi], uci, src, cidString, fp);
					baseFpWriteCnt++;
				}

				if (fp2 != null)
				{
					FpDao.WriteFingerprintRec(bw[fi], uci, src, cidString, fp2);
					fragFpWriteCnt++;
				}

			} // read loop

			cmd.CloseReader();

			FpDao.CloseFingerprintFilesForWriting();

			FpDao.BackupAndReplaceFingerprintFiles();

			msg = "*** Update Complete ***\r\n\r\n" + msg;
			ShowProgress(msg);

			return;
		}
#endif

		/// <summary>
		/// ShowProgress
		/// </summary>
		/// <param name="msg"></param>

		public static void ShowProgress(string msg)
		{
			Mobius.UAL.Progress.Show(msg, UmlautMobius.String, false); // show with cancel not allowed
		}

		public static void Log(string msg)
		{
			string logFileName = "UpdateFingerprintDb.log";
			logFileName = ServicesDirs.LogDir + @"\" + logFileName;
			DebugLog.MessageDirect(msg, logFileName);
		}

		/// <summary>
		/// Get highest CorpId in Corp DB
		/// </summary>

		public static string SelectMaxCorpId =
			"select max(corp_nbr) from corp_owner.corp_moltable";

		/// <summary>
		/// Select by CorpId criteria
		/// </summary>

		public static string SelectByCorpIdCriteria = @" 
			select 
				m.corp_nbr,
				chime(ctab),
				molecule_date
			from 
				corp_owner.corp_moltable m,
				corp_owner.corp_substance s
			where
				s.corp_nbr = m.corp_nbr
				and (s.status_code is null or s.status_code = 'A')    
				and m.corp_nbr <CorpIdCriteria>
			order by m.corp_nbr";

		// Select data from corp_moltable by date range 

		public static string SelectCorpIdsByDateRange = @"
			select 	m.corp_nbr, m.molecule_date
			from 
				corp_owner.corp_moltable m,
				corp_owner.corp_substance s
			where
				s.corp_nbr = m.corp_nbr
				and (s.status_code is null or s.status_code = 'A')    
				and m.molecule_date > to_date('1-jan-1900 000000','DD-MON-YYYY HH24MISS')
			order by m.molecule_date";

		// Select all CorpIds

		public static string SelectAllCorpIds = @"
            select m.corp_nbr 
            from
                corp_owner.corp_moltable m,
                corp_owner.corp_substance s
            where
                s.corp_nbr = m.corp_nbr
                and (s.status_code is null or s.status_code = 'A')  
                and s.mol_weight > 1
            order by m.corp_nbr";

		/// <summary>
		/// Select from ChEMBL
		/// </summary>
		/// 
		public static string SelectChemblSql = @" 
			select
				md.molregno,
				md.chembl_id,
				cs.canonical_smiles
			from
				chembl_owner.molecule_dictionary md,
				chembl_owner.compound_structures cs
			where
				cs.molregno = md.molregno
				and md.molregno <molregnoCriteria>";

		public static string SelectAllChemblIds = @"
			select chembl_id,
			from
				chembl_owner.molecule_dictionary md,
				chembl_owner.compound_structures cs
			where
				cs.molregno = md.molregno
			order by chembl_id";

	}
}
