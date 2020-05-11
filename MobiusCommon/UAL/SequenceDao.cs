using Mobius.ComOps;

using System;
using System.Data;
using System.Collections.Generic;
using System.Data.Common;

using Oracle.DataAccess.Client;

namespace Mobius.UAL
{
	/// <summary>
	/// Summary description for SequenceDao.
	/// </summary>
	public class SequenceDao
	{

		static Dictionary<string, SequenceDao> Sequences; // dictionary by sequence name of sequences

		string Name = ""; // name of sequence
		int CacheSize = 32; // number of values to cache locally
		Queue<long> Queue = new Queue<long>(); // queue of cached values

		public SequenceDao()
		{
			return;
		}

		/// <summary>
		/// Set the size of the local cache for the sequence
		/// </summary>
		/// <param name="seqName"></param>
		/// <param name="size"></param>

		public static void SetCacheSize(
			string seqName,
			int size)
		{
			SequenceDao seqDao = Lookup(seqName);
			seqDao.CacheSize = size;
			return;
		}

		public static int NextVal(
			string seqName)
		{
			return (int)NextValLong(seqName); // caution, may overflow
		}

		/// <summary>
		/// Get the next value for the sequence
		/// </summary>
		/// <param name="seqName"></param>
		/// <returns></returns>

		public static long NextValLong(
			string seqName)
		{
			return NextValLongMySQL(seqName);

			//return NextValLongOracle(seqName);
		}

		/// <summary>
		/// Get the next value for the sequence (Oracle)
		/// </summary>
		/// <param name="seqName"></param>
		/// <returns></returns>

		public static long NextValLongOracle(
		string seqName)
		{
			string sql;
			long nextVal;

			SequenceDao seqDao = Lookup(seqName);

			Queue<long> seqQueue = seqDao.Queue;
			if (seqQueue.Count > 0)
			{
				nextVal = seqQueue.Dequeue();
				return nextVal;
			}

			if (seqDao.CacheSize <= 0) sql = "select " + seqName + ".nextval from dual";
			else sql = "select /*+ first_rows */ " + seqName + ".nextval from sys.all_catalog where rownum <= " +
				(seqDao.CacheSize + 1).ToString();

			int t0 = TimeOfDay.Milliseconds();
			DbCommandMx drd = new DbCommandMx();
			drd.Prepare(sql);
			drd.ExecuteReader();
			if (!drd.Read()) throw (new Exception("SequenceDao.NextVal Read failed"));
			nextVal = drd.GetLong(0); // return this one

			while (drd.Read()) // enqueue the rest
				seqQueue.Enqueue(drd.GetLong(0));

			drd.CloseReader();
			drd.Dispose();
			t0 = TimeOfDay.Milliseconds() - t0;
			//			DebugLog.Message("Read sequence, set size = " + seqQueue.Count.ToString() + ", Time(ms) = " + t0.ToString());
			return nextVal;
		}

		/// <summary>
		/// Get the next value for the sequence (MySQL)
		/// </summary>
		/// <param name="seqName"></param>
		/// <returns></returns>

		public static long NextValLongMySQL(
			string seqName)
		{
			string sql;
			long nextVal;

			SequenceDao seqDao = Lookup(seqName);

			Queue<long> seqQueue = seqDao.Queue;
			if (seqQueue.Count > 0)
			{
				nextVal = seqQueue.Dequeue();
				return nextVal;
			}

			int count = (seqDao.CacheSize > 0 ? seqDao.CacheSize : 1);

			int t0 = TimeOfDay.Milliseconds();

			DbCommandMx seqCmd = new DbCommandMx();
			seqCmd.MxConn = DbConnectionMx.GetConnection("MySql_Mobius");  // "MySql_Mobius_Sequences"

			sql = String.Format(
			@"update mbs_owner.mbs_sequences 
			set value = last_insert_id(value) + {0}
			where name = '{1}'", count, seqName.ToUpper());

			seqCmd.PrepareUsingDefinedConnection(sql);
			
			int updCount = seqCmd.ExecuteNonReader();
			if (updCount <= 0) throw new Exception("Error updating sequence (may not exist): " + seqName);

			sql = "select last_insert_id()"; // gets value before update above
			seqCmd.PrepareUsingDefinedConnection(sql, null);
			DbDataReader rdr = seqCmd.ExecuteReader();

			bool readOk = rdr.Read();
			AssertMx.IsTrue(readOk, "readOk");
			long value = rdr.GetInt64(0);
			rdr.Close();
			seqCmd.CloseReader();

			nextVal = value + 1; // return this one now

			long v2 = value + 2; // next value
			long vn = value + count; // last value

			for (long vi = v2; vi <= vn; vi++)
				seqQueue.Enqueue(vi);

			t0 = TimeOfDay.Milliseconds() - t0;
			//			DebugLog.Message("Read sequence, set size = " + seqQueue.Count.ToString() + ", Time(ms) = " + t0.ToString());
			return nextVal;
		}


		/// <summary>
		/// Lookup a sequence by name & create if doesn't already exist
		/// </summary>
		/// <param name="seqName"></param>
		/// <returns></returns>

		static SequenceDao Lookup(
			string seqName)
		{
			if (Sequences == null) Sequences = new Dictionary<string, SequenceDao>();

			if (Sequences.ContainsKey(seqName))
				return Sequences[seqName];

			else
			{
				SequenceDao seqDao = new SequenceDao();
				Sequences.Add(seqName, seqDao);
				return seqDao;
			}
		}
	}
}
