using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using Lucene.Net.Util;

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.CdkSearchMx
{
	/// <summary>
	/// FingerprintDao
	/// </summary>
	public class FingerprintDao
	{

		public static int FingerprintFileCount = 16; // number of files of fingerprints to read/write per collection

		public int FpLengthInLongs => // length of FP in 8-byte (64 bit) longs
			FingerprintType == FingerprintType.Circular ? 1024 / 64 : 192 / 64; // 1024 bits for Circular (ECFP4), 192 bits for MACCS

		public string DatabaseName = ""; // database this Dao instance will read/write
		public FingerprintType FingerprintType = FingerprintType.Undefined;
		public string FingerprintFileNameToken => FingerprintType == FingerprintType.Circular ? "ECFP4" : ""; // token added to fine names, blank if MACCS
		public FileStream[] ReadStreams;
		public int Rsi = -1; // read stream index
		ReadFingerprintRecArgs Rfpa = null;

		public BinaryWriter[] BinaryWriters;
		public int Bwi = -1; // binary write index
		public int LastSrcId = 0;
		public string LastCid = "";

		//ASCIIEncoding ASCIIEncoding = new ASCIIEncoding();

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="databaseName"></param>
		public FingerprintDao(
			string databaseName,
			FingerprintType fingerprintType
			)
		{
			DatabaseName = databaseName;
			FingerprintType = fingerprintType;
			return;
		}

		/// <summary>
		/// DataFilesExist
		/// </summary>
		/// <returns></returns>
		public bool DataFilesExist()
		{
			string fileName = GetFpFileName(0) + ".bin";
			return File.Exists(fileName);
		}

		/// <summary>
		/// OpenFingerprintFilesForReading
		/// </summary>
		/// <returns></returns>
		public FileStream[] OpenReaders(
			string fileExtension = "bin")
		{
			if (Lex.IsUndefined(fileExtension)) fileExtension = ".bin";
			if (!fileExtension.StartsWith(".")) fileExtension = "." + fileExtension;

			FileStream[] fsa = new FileStream[FingerprintDao.FingerprintFileCount];
			for (int fi = 0; fi < FingerprintDao.FingerprintFileCount; fi++)
			{
				string fileName = GetFpFileName(fi) + fileExtension;
				int bufferSize = (int)Math.Pow(2, 20); // 2**20 = 1MB = 1048576;
				FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan);
				fsa[fi] = fs;
			}

			ReadStreams = fsa;

			Rfpa = null;
			Rsi = -1;
			return fsa;
		}

		/// <summary>
		/// GetFpFileName
		/// </summary>
		/// <param name="fi"></param>
		/// <returns></returns>
		public string GetFpFileName(int fi)
		{
			AssertMx.IsTrue(FingerprintType == FingerprintType.MACCS || FingerprintType == FingerprintType.Circular);

			return ServicesDirs.BinaryDataDir + @"\Fingerprints\" + DatabaseName + FingerprintFileNameToken + string.Format(@"Fingerprints{0:00}", fi);
		}

		/// <summary>
		/// CloseFingerprintFilesForReading
		/// </summary>
		/// <param name="fsa"></param>
		public void CloseReaders()
		{
			foreach (FileStream br in ReadStreams)
				br.Close();

			return;
		}

		/// <summary>
		/// Open writer files
		/// </summary>
		/// <param name="fileExtension">bin or new</param>
		/// <param name="fileMode">FileMode.Create or FileMode.Append</param>
		/// <returns></returns>

		public BinaryWriter[] OpenWriters(
			string fileExtension,
			FileMode fileMode)
		{
			if (Lex.IsUndefined(fileExtension)) fileExtension = ".bin";
			if (!fileExtension.StartsWith(".")) fileExtension = "." + fileExtension;

			BinaryWriter[] bw = new BinaryWriter[FingerprintDao.FingerprintFileCount];
			for (int fi = 0; fi < FingerprintDao.FingerprintFileCount; fi++)
			{
				string fileName = GetFpFileName(fi) + fileExtension;
				FileStream fs = File.Open(fileName, fileMode); // FileMode.Append);
																											 //FileMode fm = FileMode.Append;
																											 //if (!File.Exists(fileName)) fm = FileMode.Create;
																											 //FileStream fs = File.Open(fileName, fm);
				bw[fi] = new BinaryWriter(fs);
			}

			BinaryWriters = bw;
			return bw;
		}

		/// <summary>
		/// FlushWriters
		/// </summary>
		public void FlushWriters()
		{
			foreach (BinaryWriter bw in BinaryWriters)
				bw.Flush();

			return;
		}

		/// <summary>
		/// Close binary writers
		/// </summary>

		public void CloseWriters()
		{
			for (int bwi = 0; bwi < BinaryWriters.Length; bwi++)
			{
				BinaryWriter bw = BinaryWriters[bwi];
				if (bw == null) continue;

				bw.Close();
				BinaryWriters[bwi] = null;
			}

			return;
		}

		/// <summary>
		/// WriteFingerprintRec
		/// </summary>
		/// <param name="fpr"></param>

		public void WriteFingerprintRec(
			FingerprintRec fpr)
		{
			if (fpr.SrcId != LastSrcId || fpr.Cid != LastCid)
			{ // keep all FPs for a Cid in the same file
				Bwi++;
				if (Bwi >= BinaryWriters.Length) Bwi = 0;
			}

			WriteFingerprintRec(BinaryWriters[Bwi], fpr);

			LastSrcId = fpr.SrcId;
			LastCid = fpr.Cid;

			return;
		}


		/// <summary>
		/// WriteFpRec
		/// </summary>
		/// <param name="bw"></param>
		/// <param name="uci"></param>
		/// <param name="src"></param>
		/// <param name="cidString"></param>
		/// <param name="fingerprint"></param>
		public void WriteFingerprintRec(
			BinaryWriter bw,
			FingerprintRec fpr)
		{
			long l;

			if (bw == null || fpr == null) throw new InvalidDataException();

			bw.Write(fpr.molId); // int - [4]
			bw.Write(fpr.SrcId); // int  - [4]

			string cid = CompoundIdNormalizeForDatabase(fpr.Cid);
			bw.Write((byte)cid.Length); // byte - [1]
			byte[] cidBytes = ASCIIEncoding.ASCII.GetBytes(cid);
			bw.Write(cidBytes); // byte - [n]

			bw.Write(fpr.Cardinality); // number of bits set in fp - int  - [4]

			long[] fp64 = fpr.Fingerprint; // FingerprintMx.ByteArrayToLongrray(fingerprint);
			bw.Write((byte)FpLengthInLongs); // long count  - byte - [1]
			for (int li = 0; li < FpLengthInLongs; li++) // fingerprint long*3 -[24] (MACCS)
			{
				if (li < fp64.Length) l = fp64[li];
				else l = 0; // pad to fixed length
				bw.Write(l);
			}
			return;
		}

		/// <summary>
		/// Normalize CorpIds for proper database storage and retrieval
		/// Faster version of CompoundId.NormalizedForDatabase
		/// </summary>
		/// <param name="cid"></param>
		/// <returns></returns>
		public static string CompoundIdNormalizeForDatabase(string cid)
		{
			int i1;

			if (!int.TryParse(cid, out i1)) return cid;

			cid = string.Format("{0,8:00000000}", i1); // normalized for database CorpId
			return cid;
		}

		public void BackupAndReplaceFingerprintFiles()
		{

			for (int fi = 0; fi < FingerprintDao.FingerprintFileCount; fi++)
			{
				string fBase = GetFpFileName(fi);
				FileUtil.BackupAndReplaceFile(fBase + ".bin", fBase + ".bak", fBase + ".new");
			}
		}

		/// <summary>
		/// ReadFingerprintRec
		/// </summary>
		/// <returns></returns>
		public FingerprintRec ReadFingerprintRec()
		{
			if (Rsi < 0) Rsi = 0; // first file
			FingerprintRec r = ReadFingerprintRec(Rsi);

			while (r == null) // if end of file try going to next
			{
				if (Rsi + 1 >= ReadStreams.Length) return null;

				Rsi++;
				Rfpa = new ReadFingerprintRecArgs();
				Rfpa.Initialize(ReadStreams[Rsi], FpLengthInLongs);

				r = ReadFingerprintRec(Rsi);
			}

			r.molId = Rfpa.uci;
			r.SrcId = Rfpa.src;
			string cid = ASCIIEncoding.ASCII.GetString(Rfpa.cidBytes, 0, Rfpa.cidLength);
			r.Cid = CompoundIdNormalizeForDatabase(cid);

			r.Cardinality = Rfpa.cardinality;
			r.Fingerprint = (long[])Rfpa.fingerprint.Clone();

			return r;
		}

		/// <summary>
		/// Read next fingerprint rec from file
		/// </summary>
		/// <param name="fi"></param>
		/// <returns></returns>

		public FingerprintRec ReadFingerprintRec(
			int fi)
		{
			if (Rfpa == null || Rfpa.reader != ReadStreams[fi]) // start reading file
			{
				Rfpa = new ReadFingerprintRecArgs();
				Rfpa.Initialize(ReadStreams[fi], FpLengthInLongs);
			}

			if (!ReadRawFingerprintRec(Rfpa)) return null; // end of file

			FingerprintRec r = new FingerprintRec();
			r.molId = Rfpa.uci;
			r.SrcId = Rfpa.src;
			string cid = ASCIIEncoding.ASCII.GetString(Rfpa.cidBytes, 0, Rfpa.cidLength);
			r.Cid = CompoundIdNormalizeForDatabase(cid);
			r.Cardinality = Rfpa.cardinality;
			r.Fingerprint = (long[])Rfpa.fingerprint.Clone();

			return r;
		}

		/// <summary>
		/// ReadFingerprintRec
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="fileLength"></param>
		/// <param name="uci"></param>
		/// <param name="src"></param>
		/// <param name="cidLength"></param>
		/// <param name="cidBytes"></param>
		/// <param name="cardinality"></param>
		/// <param name="fingerprint"></param>
		/// <returns></returns>

		public bool ReadRawFingerprintRec(
			ReadFingerprintRecArgs a)
		{
			unsafe // unsafe code, must be compiled with unsafe switch
			{
				int cidLength = 0, cidLongs, li, bi;
				long* lp1, lp2;

				byte[] buffer = a.buffer;
				if (a.reader.Position >= a.fileLength) return false;

				fixed (byte* bp = &(buffer[0])) // fix the buffer for low-level access
				{
					int readLen1 = 4 + 4 + 1; // UCI (4), src(4) and cid length(1)
					a.reader.Read(buffer, 0, readLen1); // read UCI, src and cid length
					cidLength = buffer[8];

					int readLen2 = cidLength + 4 + 1 + a.fingerprint.Length * 8;
					a.reader.Read(buffer, readLen1, readLen2); // read cid, card(4), fpLen(1) & fp

					a.uci = *((int*)&(bp[0]));
					a.src = *((int*)&(bp[4]));

					a.cidLength = cidLength;
					cidLongs = (cidLength + 7) / 8; // copy the cid in 8-byte chunks
					fixed (byte* bp2 = &(a.cidBytes[0])) // fix the dest cid byte array
					{
						lp1 = (long*)&(bp[readLen1]); // start of cid in buffer
						lp2 = (long*)&(bp2[0]); // start of a.cidLength
						for (li = 0; li < cidLongs; li++) // copy the cid
							*(&lp2[li]) = *(&lp1[li]);
					}

					bi = readLen1 + cidLength; // start of cardinality
					a.cardinality = *((int*)&(bp[bi]));

					byte fpLongs = buffer[bi + 4]; // number of longs in fp
					lp1 = (long*)&(bp[bi + 5]); // position of fp in buffer
					fixed (long* lpFp = &(a.fingerprint[0])) // fix the dest fingerprint
					{
						for (li = 0; li < fpLongs; li++) // copy it
							*(&lpFp[li]) = *(&lp1[li]);
					}
				}
			}
			return true;

		}

		/// <summary>
		/// Get a set of the existing Cids
		/// </summary>
		/// <returns></returns>

		public HashSet<string> GetExistingCidSet()
		{
			HashSet<string> cidSet = new HashSet<string>();
			OpenReaders();

			while (true)
			{
				FingerprintRec r = ReadFingerprintRec();
				if (r == null) break;

				cidSet.Add(r.Cid);
			}

			CloseReaders();
			return cidSet;
		}

		/// <summary>
		/// ReadUndefinedStructuresCids
		/// </summary>
		/// <returns></returns>
		public HashSet<string> ReadUndefinedStructuresCids()
		{
			HashSet<string> undefCids = new HashSet<string>();
			string fileName = ServicesDirs.BinaryDataDir + @"\Fingerprints\" + DatabaseName + FingerprintFileNameToken + "UndefinedStructureCids.txt";
			if (!FileUtil.Exists(fileName)) return undefCids;
			StreamReader sr = new StreamReader(fileName);
			while (true)
			{
				string cid = sr.ReadLine();
				if (cid == null) break;

				cid = CompoundIdNormalizeForDatabase(cid);
				undefCids.Add(cid.Trim());
			}
			sr.Close();

			return undefCids;
		}

		/// <summary>
		/// WriteUndefinedStructuresCids
		/// </summary>
		/// <param name="undefCids"></param>
		public void WriteUndefinedStructuresCids(HashSet<string> undefCids)
		{
			string fileName = ServicesDirs.BinaryDataDir + @"\Fingerprints\" + DatabaseName + FingerprintFileNameToken + "UndefinedStructureCids.txt";
			StreamWriter sw = new StreamWriter(fileName);
			foreach (string cid in undefCids)
				sw.WriteLine(cid);
			sw.Close();
			return;
		}

		/// <summary>
		/// WriteFingerPrintSimMxDataUpdateCheckpointDate
		/// </summary>
		/// <param name="checkPointDate"></param>
		public void WriteFingerPrintSimMxDataUpdateCheckpointDate(string checkPointDate)
		{
			StreamWriter sw = null;
			try
			{
				string fileName = ServicesDirs.BinaryDataDir + @"\Fingerprints\" + DatabaseName + FingerprintFileNameToken + "UpdateCheckpointDate.txt";
				sw = new StreamWriter(fileName);
				sw.WriteLine(checkPointDate);
				sw.Close();
			}

			catch (Exception ex)
			{
				if (sw != null) sw.Close();
				throw new Exception(ex.Message, ex);
			}

			return;
		}

		/// <summary>
		/// ReadFingerPrintSimMxDataUpdateCheckpointDate
		/// </summary>
		/// <returns></returns>
		public string ReadFingerPrintSimMxDataUpdateCheckpointDate()
		{
			StreamReader sr = null;

			try
			{
				string fileName = ServicesDirs.BinaryDataDir + @"\Fingerprints\" + DatabaseName + FingerprintFileNameToken + "UpdateCheckpointDate.txt";
				sr = new StreamReader(fileName);
				string checkPointDate = sr.ReadLine().Trim();
				sr.Close();
				return checkPointDate;
			}

			catch (Exception ex)
			{
				if (sr != null) sr.Close();
				throw new Exception(ex.Message, ex);
			}
		}


	} // FingerprintDao

	public class FingerprintRec
	{
		public int molId;
		public int SrcId;
		public string Cid = "";
		public int Cardinality;
		public long[] Fingerprint;
	}

	public class ReadFingerprintRecArgs
	{
		public FileStream reader;
		public long fileLength;
		public byte[] buffer; // byte[] buffer large enough to hold any fingerprint record
		public int uci;
		public int src;
		public int cidLength; // length of cid in bytes
		public byte[] cidBytes; // ASCII byte[] buffer large enough to hold any expected cid string
		public int cardinality;
		public long[] fingerprint;


		public void Initialize(FileStream fs, int fpLengthInLongs)
		{
			reader = fs;
			fileLength = fs.Length;
			cidBytes = new byte[256]; // byte[] buffer large enough to hold any expected cid string
			buffer = new byte[4096]; // byte[] buffer large enough to hold any fingerprint record
			fingerprint = new long[fpLengthInLongs];
			return;
		}

	}

}
