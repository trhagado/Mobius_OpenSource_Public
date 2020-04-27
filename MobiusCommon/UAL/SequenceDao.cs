using Mobius.ComOps;

using System;
using System.Data;
using System.Collections.Generic;

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
		int CacheSize = 0; // number of values to cache locally
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
