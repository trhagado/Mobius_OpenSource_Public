using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;
using System.Globalization;
using System.Diagnostics;
using System.IO;

namespace Mobius.Data
{

	/// <summary>
	/// Collection of data row value object (vo) arrays 
	/// Example uses:
	///  Client-side DataTableManger rows
	///  Services-side QueryEngine rows (potential)
	/// </summary>

	public class VoArrayList
	{
		public List<object[]> RowBuffer = new List<object[]>(); // in-memory buffer list of rows
		public int RowBufferRowCount => RowBuffer.Count; // number of rows currently in RowBuffer
		public int TotalRowCount => (RowsWrittenToCache + RowBuffer.Count); // total number of rows in collection (cached + in-memory buffer)

		public DataRowCachingMode CachingMode => GetCachingMode(); // determined by WriteCache and RemoveRows values (default = CacheToSecondaryStorage)
		internal bool WriteCache = true; // if true caching out of rows is allowed, if false then no rows will be cached out
		internal bool RemoveRows = true; // if true & caching then cached rows removed, if false then don't remove rows from memory

		internal int MininumRowsRequiredForCaching = 10; // minimum unwritten rows required to do a write to cache operation (keep small for small QE mem usage)
		internal int RowsWrittenToCache = 0; // rows written to cache
		internal int RowsRemovedFromList = 0; // number of rows removed from the list (may or may not be written to cache)

		internal string CacheFilePath; // name of cache file
		internal BinaryWriter CacheWriter; // for writing cache file

		internal BinaryReader CacheReader; // for reading cache file
		internal int CacheReaderPosition = -1; // index of the last row read from the cache

		internal int VoaLength = -1; // length of vos being written

		internal static Exception LastException = null;
		internal static bool DebugCaching = false;

		/// <summary>
		/// Basic constructor
		/// </summary>

		public VoArrayList()
		{
			if (DebugCaching)
				MininumRowsRequiredForCaching = 100;

			return;
		}

		/// <summary>
		/// Indexer
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>

		public object[] this[int index]  
		{
			get { return GetRow(index); }
		}

/// <summary>
/// Adds a VoArray row to the set of rows
/// </summary>
/// <param name="vo"></param>

		public void AddRow(object[] vo)
		{
			RowBuffer.Add(vo);

			if (vo != null) VoaLength = vo.Length;

			if (DebugCaching) // && RowBuffer.Count <= 4)
			{
				String msg = string.Format("Row added Buffer (cacheSize: {0}, bufferSize: {1})",
					RowsWrittenToCache, RowBuffer.Count);

				//if (RowBuffer.Count == 4) msg = "Row added Buffer...";
				DebugLog.Message(msg);
			}

			//if (Rows.Count == 17) vo = vo; // debug

			return;
		}

		/// <summary>
		/// Get current caching mode
		/// </summary>
		/// <returns></returns>

		public DataRowCachingMode GetCachingMode()
		{
			if (WriteCache)
			{
				if (RemoveRows) return DataRowCachingMode.CacheToSecondaryStorage;
				else throw new InvalidDataException("Invalid DataRowCachingMode");
			}

			else if (!RemoveRows) return DataRowCachingMode.LockInMemory;

			else throw new InvalidDataException("Invalid DataRowCachingMode");
		}

		/// <summary>
		/// Set caching state
		/// Todo: Properly handle transitions between modes
		/// </summary>

		public void SetCachingMode(DataRowCachingMode mode)
		{
			DataRowCachingMode curMode = GetCachingMode();

			if (mode == curMode) return; // no change

			if (mode == DataRowCachingMode.CacheToSecondaryStorage)
			{
				// Switch from LockInMemory to CacheToSecondaryStorage

				if (DebugCaching)
					DebugLog.Message("Switching DataRowCachingMode from LockInMemory to CacheToSecondaryStorage");

				WriteCache = true;
				RemoveRows = true;

				MoveRowsToCacheAsAppropriate(); // force all in-memory data to be written out to cache file
				return;
			}

			else if (mode == DataRowCachingMode.LockInMemory)
			{
				// Switch from CacheToSecondaryStorage to LockInMemory

				if (DebugCaching)
					DebugLog.Message("Switching CacheToSecondaryStorage from LockInMemory to DataRowCachingMode");

				TransformFrom_CacheToSecondaryStorageMode_to_LockInMemoryMode();
				return;
			}

			else throw new InvalidDataException("Invalid mode: " + mode);
		}

		/// <summary>
		/// SwitchCacheModeFrom_DataRowCachingMode_to_LockInMemory
		/// </summary>

		void TransformFrom_CacheToSecondaryStorageMode_to_LockInMemoryMode()
		{
			Stopwatch sw = Stopwatch.StartNew();
			int oldCacheCount = RowsWrittenToCache;
			int oldRowBufferCount = RowBuffer.Count;

// If any rows in cache then read them into the beginning of the RowBuffer

			if (RowsWrittenToCache > 0)
			{
				List<object[]> rows = ReadCachedRows(); // read in the cached rows
				rows.AddRange(RowBuffer); // add in any in-memory rows
				RowBuffer.Clear();
				RowBuffer = rows;

				CloseCaching(deleteCacheFile: true); 
			}

			WriteCache = false;
			RemoveRows = false;

			if (DebugCaching)
			{
				string msg = string.Format("Old cacheSize: {0}, Old bufferSize: {1}, (new cacheSize: {2}, new bufferSize: {3})",
					oldCacheCount, oldRowBufferCount, RowsWrittenToCache, RowBuffer.Count);
				DebugLog.StopwatchMessage(msg, sw);
			}
			
			return;
		}

		/// <summary>
		/// Read in all of the rows that have been cached
		/// </summary>
		/// <returns></returns>

		public List<object[]> ReadCachedRows()
		{
			AssertMx.IsTrue(File.Exists(CacheFilePath), "CacheFilePath doesn't exist");
			FileStream fs = File.Open(CacheFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			BinaryReader br = new BinaryReader(fs);

			List<object[]> cachedRows = new List<object[]>(); // in-memory buffer list of rows
			for (int ri = 0; ri < RowsWrittenToCache; ri++)
			{
				object[] vo = VoArray.ReadBinaryVoArray(br, VoaLength);
				cachedRows.Add(vo);
			}

			br.Close(); // close reader and the underlying stream

			return cachedRows;
		}

		/// <summary>
		/// Move rows to cache file if caching is turned on and RowBuffer contains the threshhold number of rows
		/// </summary>
		/// <returns></returns>

		public int MoveRowsToCacheAsAppropriate()
		{
			int rowsCached = 0, rowsRemoved = 0, rowsAffected = 0;
			lock (this)
			{
				Stopwatch sw = Stopwatch.StartNew();

				if (RowBuffer.Count == 0) return 0;

				if (!WriteCache && !RemoveRows) return 0; // leave as is

// Write rows to cache if caching is turned on

				if (WriteCache)
				{
					if (RowBuffer.Count < MininumRowsRequiredForCaching) return 0; // not time to cache yet

					if (RowsWrittenToCache == 0)
					{
						CacheFilePath = TempFile.GetTempFileName(ServicesDirs.TempDir, "voa", true); // Note that we want the temp dir to be local to the current machine
						CacheWriter = new BinaryWriter(File.Open(CacheFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite));
					}

					byte[] ba = VoArray.SerializeBinaryVoArrayListToByteArray(RowBuffer, includeListHeader: false);
					CacheWriter.Write(ba);
					CacheWriter.Flush();

					rowsCached = rowsAffected = RowBuffer.Count;
					RowsWrittenToCache += rowsCached;
				}

// Remove all rows from Rows if rows purging is turned on

				if (RemoveRows)
				{
					if (WriteCache && RowBuffer.Count < MininumRowsRequiredForCaching) return 0; // not time to remove rows yet if caching

					rowsRemoved = rowsAffected = RowBuffer.Count;
					RowBuffer.Clear();

					RowsRemovedFromList += rowsRemoved;
				}

				if (DebugCaching)
				{
					string msg = String.Format("Rows added to cache: {0} , rows removed from buffer: {1}, total rows removed: {2} (cacheSize: {3}, bufferSize: {4}",
						 rowsCached, rowsRemoved, RowsRemovedFromList, RowsWrittenToCache, RowBuffer.Count);
					DebugLog.StopwatchMessage(msg, sw);
				}

				return rowsAffected; 
			} // end of lock
		}

		/// <summary>
		/// Try to get the row at the given index from the buffer or cache
		/// </summary>
		/// <param name="cursor"></param>
		/// <returns></returns>

		public bool TryToGetNextRow(
			VoArrayListCursor cursor)
		{
			object[] vo = null;

			int ri = cursor.Position + 1;

			AssertMx.IsTrue(ri >= 0, "Invalid row index:" + ri);

			if (ri >= TotalRowCount)
			{
				if (DebugCaching)
					DebugLog.Message(string.Format("Row {0} not in cache or buffer (cacheSize: {1}, bufferSize: {2})", ri, RowsWrittenToCache, RowBuffer.Count));

				return false; // not in collection
			}

			vo = GetRow(ri);
			cursor.CurrentRow = vo;
			cursor.Position = ri;

			return true;
		}

		/// <summary>
		/// Try to get the row at the given index from the buffer or cache
		/// </summary>
		/// <param name="ri"></param>
		/// <returns></returns>
		/// 
		public object[] GetRow (
			int ri)
		{
			object[] vo = null;

			if (ri < 0 || ri >= TotalRowCount)
			{
				string msg = string.Format("Row {0} not in cache or buffer (cacheSize: {1}, bufferSize: {2})", ri, RowsWrittenToCache, RowBuffer.Count);
				throw new InvalidDataException(msg);
			}

			// See if in buffer

			if (ri >= RowsWrittenToCache)
			{
				vo = (object[])RowBuffer[ri - RowsWrittenToCache];

				if (DebugCaching)
					DebugLog.Message(string.Format("Row: {0} in Buffer (cacheSize: {1}, bufferSize: {2})", ri, RowsWrittenToCache, RowBuffer.Count));

				return vo;
			}

			// Get from cache

			if (ri < CacheReaderPosition + 1) // if already read past this then close current cursor
				CloseCacheReader();

			if (CacheReader == null) // need to open reader?
			{
				AssertMx.IsNotNull(CacheWriter, "CacheWriter");
				AssertMx.IsTrue(File.Exists(CacheFilePath), "CacheFilePath doesn't exist");

				FileStream fs = File.Open(CacheFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				CacheReader = new BinaryReader(fs);
				CacheReaderPosition = -1;

				if (DebugCaching) DebugLog.Message("Opening cache reader");
			}

			while (CacheReaderPosition + 1 <= ri) // read rows until we get the one we want
			{
				vo = VoArray.ReadBinaryVoArray(CacheReader, VoaLength);
				CacheReaderPosition++;
			}

			CacheReaderPosition = ri;

			if (DebugCaching)
				DebugLog.Message(string.Format("Row: {0} in Cache (cacheSize: {1}, bufferSize: {2})", ri, RowsWrittenToCache, RowBuffer.Count));

			return vo;
		}

		/// <summary>
		/// Reset the VoArrayListState
		/// </summary>

		public void Reset()
		{
			RowBuffer.Clear();
			CloseCaching(deleteCacheFile: true);

			WriteCache = true; // reset caching mode to CacheToSecondaryStorage
			RemoveRows = true;

			return;
		}

		/// <summary>
		/// Close any file open for cache writing
		/// </summary>

		public void CloseCaching(
			bool deleteCacheFile = true)
		{
			CloseCacheWriter(deleteCacheFile);
			CloseCacheReader();
			if (DebugCaching) DebugLog.Message("Caching Closed");
		}

		/// <summary>
		/// CloseCacheWriter
		/// </summary>
		/// <param name="deleteCacheFile"></param>

		public void CloseCacheWriter(
			bool deleteCacheFile = true)
		{
			if (CacheWriter != null)
			{
				try { CacheWriter.Close(); }
				catch (Exception ex) { LastException = ex; }
				CacheWriter = null;
			}

			if (Lex.IsDefined(CacheFilePath))
			{
				FileUtil.DeleteFile(CacheFilePath);

				CacheFilePath = null;
			}

			RowsWrittenToCache = 0;
			RowsRemovedFromList = 0;

			return;
		}

		/// <summary>
		/// CloseCacheReader
		/// </summary>

		public void CloseCacheReader()
		{
			if (CacheReader != null)
			{
				try { CacheReader.Close(); }
				catch (Exception ex) { LastException = ex; }
				CacheReader = null;
			}

			CacheReaderPosition = -1;
		}

	}

	/// <summary>
	/// Row retrieval cursor for a QE instance
	/// Allows multiple cursors to be used on a single result set
	/// (e.g. a client side DataTableManger and a services Spotfire visualization row set
	/// </summary>

	public class VoArrayListCursor
	{
		public int Position = -1; // current position, normally incremented by 1, can be reset to -1 to start reading again
		public object[] CurrentRow = null;

		internal Exception LastException;

		/// <summary>
		/// Clone the current cursor state so it can be restored later
		/// </summary>
		/// <returns></returns>

		public VoArrayListCursor Save()
		{
			VoArrayListCursor cs2 = (VoArrayListCursor)MemberwiseClone(); // do shallow clone
			return cs2;
		}

		/// <summary>
		/// Restore the cursor state to a previous value
		/// </summary>
		/// <param name="savedState"></param>

		public void Restore(
			ref VoArrayListCursor cursorToRestore)
		{
			cursorToRestore = this;
			return;
		}

		/// <summary>
		/// Close the cursor
		/// </summary>

		public void Close()
		{
			Position = -1;
			CurrentRow = null;

			return;
		}
	}

	/// <summary>
	/// Enum for mode of caching query results data rows
	/// </summary>

	public enum DataRowCachingMode
	{
		CacheToSecondaryStorage = 0, // write to secondary storage and remove from memory (data persists only for this VoArrayList object)
		LockInMemory = 1 // keep all rows in memory, don't write to secondary storage
		//Transient = 2 // keep rows just long enough to be read once then remove (don't cache)
		//CacheAndLock = 3, // write to secondary storage and keep all rows in memory
	}

}
