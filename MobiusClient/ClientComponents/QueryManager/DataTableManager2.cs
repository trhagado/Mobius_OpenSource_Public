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
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Mobius.ClientComponents
{
	public partial class DataTableManager
	{

		/// <summary>
		/// Begin cache table to disk 
		/// Caching is used to avoid running out of memory, usually for large exports
		/// </summary>

		public void BeginCaching()
		{
			if (!AllowCaching) return;

			if (CacheStartPosition >= 0) return; // return if already caching
			if (CacheReader != null) return; // return if reading an existing cache file
			if (RowRetrievalState == RowRetrievalState.Complete) return; // return if already have complete data set

			CacheStartPosition = DataTableMx.Rows.Count;
			//if (CacheStartPosition < MinCacheStartPosition)
			//  CacheStartPosition = MinCacheStartPosition;

			CacheFile = TempFile.GetTempFileName(ClientDirs.TempDir, "mds", true);
			CacheWriter = new StreamWriter(CacheFile);
			RowsRemovedFromDataTable = 0;
			RowsWrittenToCache = 0;
			CacheStartRowCount = RowCount;
			CacheStartKeyCount = KeyCount;

			if (DebugCaching) ClientLog.Message("Begincaching, CachingStartPosition: " + DataTableMx.Rows.Count + ", CachingStartRowCount: " + CacheStartRowCount);
			return;
		}

		/// <summary>
		/// Write out rows from table if caching has been activated
		/// </summary>

		internal void WriteRowsToCache(bool keepLastKeyValue)
		{
			DataRowMx dr;
			string firstKey = "", lastKey = "";
			int rowsRemovedInThisCall = 0, ri, ri2, srpi;

			if (!AllowCaching) return;

			if (CacheStartPosition < 0 || CacheWriter == null) return; // just return if caching not active

			if (DataTableMx.Rows.Count < CacheMiminumRowsRequiredForWriting) return;

			lock (DataTransferLock) // lock the DataTable while changing
			{
				if (DataTableFetchPosition < CacheStartPosition) return; // only cache if fetching beyond start position 
				if (DataTableFetchPosition < DataTableMx.Rows.Count - 1) return; // can only cache out if at end of available rows

				dr = DataTableMx.Rows[CacheStartPosition];
				firstKey = dr[KeyValueVoPos] as string;
				ri = DataTableMx.Rows.Count - 1;
				dr = DataTableMx.Rows[ri];
				lastKey = dr[KeyValueVoPos] as string; // key we want to keep

				if (QueryManager.MoleculeGrid != null)
					QueryManager.MoleculeGrid.BeginUpdate();

				ri = CacheStartPosition; // start deleting here
				while (ri < DataTableMx.Rows.Count)
				{ // delete anything with key other than end value
					dr = DataTableMx.Rows[ri];
					string key2 = dr[KeyValueVoPos] as string; // end key with possibly partial data that we want to keep
					if (keepLastKeyValue && key2 == lastKey) break;

					bool doCacheIO = !PurgeDataTableWithoutWritingToCacheFile;
					if (doCacheIO)
					{
						object[] oa = dr.ItemArray;
						StringBuilder sb = VoArray.SerializeToText(oa, KeyValueVoPos, oa.Length - KeyValueVoPos);
						string lenStr = String.Format("{0,8:00000000}", sb.Length);
						CacheWriter.Write(lenStr); // write length
						CacheWriter.Write(sb); // write record
						RowsWrittenToCache++;
					}

					DataTableMx.Rows.Remove(dr);
					RowsRemovedFromDataTable++;
					rowsRemovedInThisCall++;
				}

				DataTableFetchPosition -= rowsRemovedInThisCall; // adjust fetch position

				for (ri2 = CacheStartPosition; ri2 < DataTableMx.Rows.Count; ri2++)
				{ // adjust row indexes held the row attributes in rows below those paged out
					DataRowAttributes dra = GetRowAttributes(ri2);
					if (dra == null) continue;

					dra.FirstRowForKey -= rowsRemovedInThisCall;
					if (dra.SubRowPos == null) continue;
					for (srpi = 0; srpi < dra.SubRowPos.Length; srpi++)
					{
						dra.SubRowPos[srpi] -= rowsRemovedInThisCall;
					}
				}
			} // end of locked section

			if (QueryManager.MoleculeGrid != null)
			{
				QueryManager.MoleculeGrid.EndUpdate();
				QueryManager.MoleculeGrid.Refresh();
				Application.DoEvents();
			}

			if (DebugCaching) ClientLog.Message(
				"CachedRows - DataTable.Rows.Count: " + DataTableMx.Rows.Count +
				", FirstKey: " + firstKey + ", LastKey: " + lastKey +
				", RowsRemovedFromDataTable (This Call): " + rowsRemovedInThisCall +
				", RowsRemovedFromDataTable (Total): " + RowsRemovedFromDataTable +
				", RowsWrittenToCache (Total): " + RowsWrittenToCache +
				", DataTableFetchPosition: " + DataTableFetchPosition);

			return;
		}

		/// <summary>
		/// End table caching by writing out remaining records & opening file reader
		/// </summary>

		public void EndCaching()
		{
			if (!AllowCaching) return;

			if (CacheStartPosition < 0 || CacheWriter == null) return; // caching active

			if (RowsRemovedFromDataTable == 0)
			{
				if (CacheWriter != null)
				{
					CacheWriter.Close();
					CacheWriter = null;
				}
				CacheStartPosition = -1;
				return;
			}

			if (RowRetrievalState == RowRetrievalState.Running || RowRetrievalState == RowRetrievalState.Paused)
				CancelRowRetrieval();  // cancel if not complete

			WriteRowsToCache(false); // write out remaining rows
			CacheWriter.Close();
			CacheWriter = null;

			CacheReader = new StreamReader(CacheFile);
			CacheStartPosition = -1;

			//RowCount = CacheStartRowCount; // restore counts (todo: fix this: gives zero for export count with this code)
			//KeyCount = CacheStartKeyCount;

			RowRetrievalState = RowRetrievalState.Paused;
			if (DebugDetails) ClientLog.Message("RowRetrievalState.Paused 3");

			if (QueryManager.MoleculeGrid != null)
			{
				QueryManager.MoleculeGrid.EndUpdate();
				QueryManager.MoleculeGrid.Refresh();
				Application.DoEvents();
			}

			if (StatusBarManager != null)
			{ // update status bar, may process an event that generates a request for more rows
				StatusBarManager.DisplayRetrievalProgressState(RowRetrievalState);
				StatusBarManager.DisplayFilterCounts(); // update count display
			}

			if (DebugCaching) ClientLog.Message("EndCaching: " + DataTableMx.Rows.Count);

			return;
		}

		/// <summary>
		/// Write result set to a binary file for later retrieval
		/// </summary>

		public void WriteBinaryResultsFile(string fileName)
		{
			int id = Query.UserObject.Id;
			if (id <= 0) throw new Exception("UserObject id not defined, Query not saved");

			BinaryWriter bw = BinaryFile.OpenWriter(fileName + ".new"); // open ".new" file
			string sq = Query.Serialize(); // serialize & write the query
			bw.Write(sq);
			int voArrayLen = DataTableMx.Columns.Count - KeyValueVoPos; // don't write values before the key
			bw.Write(voArrayLen); // write the array length

			for (int dri = 0; dri < DataTableMx.Rows.Count; dri++)
			{
				DataRowMx dr = DataTableMx.Rows[dri];
				VoArray.WriteBinaryVoArray(dr.ItemArrayRef, bw, KeyValueVoPos);
			}

			bw.Close();
			FileUtil.BackupAndReplaceFile(fileName, fileName + ".bak", fileName + ".new");

			return;
		}

		/// <summary>
		/// Attempt to read existing results file into the query DataTable
		/// </summary>
		/// <param name="qm"></param>

		public void ReadBinaryResultsFile(string fileName)
		{
			QueryTable qt;
			QueryColumn qc;
			BinaryReader br = null;

			Stopwatch sw = Stopwatch.StartNew();

			try
			{
				bool saveHandlersEnabled = Qm.DataTable.EnableDataChangedEventHandlers(false); // disable for faster load

				bool saveUpdateMaxRowsPerKey = UpdateMaxRowsPerKeyEnabled;
				UpdateMaxRowsPerKeyEnabled = false; // disable for faster load

				int id = Query.UserObject.Id;
				if (id <= 0) throw new Exception("Query not saved");
				if (DataTableMx == null || DataTableMx.Columns.Count == 0)
					throw new Exception("DataTable not defined");

				br = BinaryFile.OpenReader(fileName);
				string sq = br.ReadString();
				Query q0 = Query.Deserialize(sq); // deserialize the saved query
				QueryManager qm0 = new QueryManager();
				qm0.LinkMember(q0);
				ResultsFormat rf0 = new ResultsFormat(qm0, OutputDest.WinForms);
				ResultsFormatFactory rff0 = new ResultsFormatFactory(qm0, OutputDest.WinForms);
				rff0.Build(); // build format with vo positions

				// The cached query cols should match those of the current query: however,
				// we'll create a mapping just in case they don't

				int voArrayLen0 = br.ReadInt32(); // cached vo array len
				int voArrayLen = DataTableMx.Columns.Count - KeyValueVoPos; // current query vo array len

				List<int> q0VoMap = new List<int>(); // vo position in cached query data
				List<int> qVoMap = new List<int>(); // vo position in current version of query

				q0VoMap.Add(0); // first position is the common key value
				qVoMap.Add(0);

				foreach (QueryTable qt0 in q0.Tables) // scan each table in cached data
				{
					foreach (QueryColumn qc0 in qt0.QueryColumns) // and each column
					{
						if (qc0.VoPosition < 0) continue; // skip if not mapped to the vo in cached data
						int q0VoPos = qc0.VoPosition - KeyValueVoPos; // where it is in cache

						int qvoPos = -1; // where it will go
						qt = Query.GetTableByName(qt0.MetaTable.Name);
						if (qt != null)
						{
							qc = qt.GetQueryColumnByName(qc0.MetaColumn.Name);
							if (qc != null)
								qvoPos = qc.VoPosition - KeyValueVoPos;
						}

						q0VoMap.Add(q0VoPos); // where it is in saved data
						qVoMap.Add(qvoPos); // where it will go (not including attributes & check cols)
					}
				}

				if (q0VoMap.Count != voArrayLen0) throw new Exception("Cached Vo length doesn't match list of selected columns");

				DataTableMx.Clear(); // clear the rows
				CidList cidList = new CidList();
				object[] voa = new object[voArrayLen]; // array to fill

				while (!BinaryFile.ReaderEof(br)) // process each row
				{
					for (int mi = 0; mi < q0VoMap.Count; mi++) // each col 
					{
						object o = VoArray.ReadBinaryItem(br);
						if (mi == 0 && o != null) // add to key list if key
							cidList.Add(o.ToString(), false);

						if (qVoMap[mi] >= 0) // save in new buf if mapped
							voa[qVoMap[mi]] = o;
					}

					DataRowMx dr = AddDataRow(voa);
				}

				br.Close();
				Qm.DataTable.EnableDataChangedEventHandlers(saveHandlersEnabled);

				UpdateMaxRowsPerKeyEnabled = saveUpdateMaxRowsPerKey;
				InitializeRowAttributes(false);

				ResultsKeys = cidList.ToStringList(); // include keys in DTM as well

				double ms = sw.Elapsed.TotalMilliseconds;
				return;
			}
			catch (Exception ex)
			{
				if (br != null) br.Close();
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// Fill the DataTable for the query from a file
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		public static bool LoadDataTableFromFile(
					Query q)
		{
			string fileName = q.ResultsDataTableFileName;
			return LoadDataTableFromFile(q, fileName);
		}

		/// <summary>
		/// Fill the DataTable for the query from a file
		/// </summary>
		/// <param name="q"></param>
		/// <param name="fileName">basic file name without directory</param>
		/// <returns></returns>

		public static bool LoadDataTableFromFile(
			Query q,
			string fileName)
		{
			string clientFileName;
			DateTime clientFileDate;

			if (!GetSavedDataTableFile(fileName, out clientFileName, out clientFileDate))
				return false;

			QueryManager qm = new QueryManager();
			qm.LinkMember(q);
			ResultsFormat rf = new ResultsFormat(qm, OutputDest.WinForms);
			ResultsFormatFactory rff = new ResultsFormatFactory(qm, OutputDest.WinForms);
			rff.Build(); // build format with vo positions
			DataTableManager dtm = new DataTableManager(qm);
			DataTableMx dt = BuildDataTable(q);
			qm.LinkMember(dt);

			try
			{
				qm.DataTableManager.ReadBinaryResultsFile(clientFileName);
				q.ResultsDataTable = dt; // link query to DataTable (redundant?)
			}
			catch (Exception ex)
			{ return false; }

			// Map "old" datatable names based on metatable names to use alias instead

			foreach (System.Data.DataColumn dc in dt.Columns)
			{
				string colName = dc.ColumnName;
				int i1 = colName.IndexOf(".");
				if (i1 < 0) continue;

				string tName = colName.Substring(0, i1);
				string cName = colName.Substring(i1 + 1);
				QueryTable qt = q.GetQueryTableByName(tName);
				if (qt != null) dc.ColumnName = qt.Alias + "." + cName;
			}

			// todo - complete adjustment to get match between query and DataTable

			return true;
		}

		/// <summary>
		/// Get the latest version of a saved DataTable file
		/// </summary>
		/// <param name="fileName">Basic file name without directory</param>
		/// <param name="clientFileName">Full path to file on client</param>
		/// <param name="clientFileDate">Last write date for file</param>
		/// <returns></returns>

		public static bool GetSavedDataTableFile(
			string fileName,
			out string clientFileName,
			out DateTime clientFileDate)
		{
			clientFileDate = DateTime.MinValue;

			string serverFileName = @"<BackgroundExportDir>\" + fileName;
			clientFileName = ClientDirs.CacheDir + @"\" + fileName;
			ServerFile.GetIfChanged(serverFileName, clientFileName); // get file from server if client file doesn't exist or is older
			if (!File.Exists(clientFileName)) return false;

			clientFileDate = FileUtil.GetFileLastWriteTime(clientFileName);
			return true;
		}

		/// <summary>
		/// Serialize DataTable to an xml text file
		/// </summary>
		/// <param name="includeMetaTables"></param>
		/// <returns></returns>

		public void SerializeXml(string fileName)
		{
			XmlTextWriter tw = new XmlTextWriter(fileName, System.Text.Encoding.ASCII);
			tw.Formatting = Formatting.Indented;
			tw.WriteStartDocument();
			SerializeDataTable(tw, this.QueryManager);
			tw.WriteEndDocument();

			tw.Flush();
			tw.Close();
			return;
		}

		/// <summary>
		/// Serialize DataTable to string
		/// </summary>
		/// <param name="includeMetaTables"></param>
		/// <returns></returns>

		public string SerializeDataTable()
		{
			MemoryStream ms = new MemoryStream();
			XmlTextWriter tw = new XmlTextWriter(ms, null);
			tw.Formatting = Formatting.Indented;
			tw.WriteStartDocument();
			SerializeDataTable(tw, this.QueryManager);
			tw.WriteEndDocument();

			tw.Flush();

			byte[] buffer = new byte[ms.Length];
			ms.Seek(0, 0);
			ms.Read(buffer, 0, (int)ms.Length);
			string content = System.Text.Encoding.ASCII.GetString(buffer, 0, (int)ms.Length);
			tw.Close(); // must close after read
			return content;
		}

		/// <summary>
		/// Serialize a DataTable to an XmlTextWriter
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>

		public void SerializeDataTable(
			XmlTextWriter tw,
			IQueryManager iqm)
		{
			QueryManager qm = iqm as QueryManager;
			if (qm == null) throw new Exception("QueryManager not defined");

			DataTableMx dt = qm.DataTable;
			if (dt == null) return; // ignore if no data table

			SerializeDataTable(tw, dt);
			return;
		}
		/// <summary>
		/// Serialize a DataTable to an XmlTextWriter
		/// </summary>
		/// <param name="tw"></param>
		/// <param name="dt"></param>

		public void SerializeDataTable(
			XmlTextWriter tw,
			IDataTableMx iDt)
		{
			DataTableMx dt = iDt as DataTableMx;
			if (dt == null) return; // ignore if no data table

			tw.WriteStartElement("DataTable");
			tw.WriteAttributeString("TableName", dt.TableName);
			tw.WriteStartElement("DataColumns");
			foreach (System.Data.DataColumn dc in dt.Columns)
			{
				if (Lex.Eq(dc.ColumnName, RowAttributesColumnName) || // don't include these in output
				Lex.Eq(dc.ColumnName, CheckMarkColumnName)) continue;

				tw.WriteStartElement("DataColumn");
				tw.WriteAttributeString("ColumnName", dc.ColumnName);
				tw.WriteAttributeString("DataType", dc.DataType.FullName);
				tw.WriteEndElement();
			}

			tw.WriteEndElement(); // end of DataColumns

			tw.WriteStartElement("DataRows");

			foreach (DataRowMx dr in dt.Rows)
			{ // put each row in a CData element
				object[] oa = dr.ItemArray;
				StringBuilder sb = VoArray.SerializeToText(oa, 2, oa.Length - 2);
				tw.WriteCData(sb.ToString());
			}
			tw.WriteEndElement(); // end of DataRows
			tw.WriteEndElement(); // end of DataTable

			return;
		}


		/// <summary>
		/// Serialize a DataRow to a byte array
		/// </summary>
		/// <param name="row"></param>
		/// <returns></returns>

		public static byte[] SerializeDataRowToByteArray(System.Data.DataRow row)
		{
			MemoryStream memStream = new MemoryStream();
			XmlSerializer serializer = new XmlSerializer(typeof(object[]));
			serializer.Serialize(memStream, row.ItemArray);

			return memStream.ToArray();
		}

		/// <summary>
		/// Deserialize a DataTable from a file
		/// </summary>
		/// <param name="tr"></param>
		/// <returns></returns>

		public IDataTableMx DeserializeXml(
			string fileName)
		{
			StreamReader sr = null;
			string tempFile = null;

			if (fileName.StartsWith("<")) // get from server if server file name
			{
				string serverFile = fileName; // must be a server file name
				tempFile = TempFile.GetTempFileName();
				ServerFile.CopyToClient(serverFile, tempFile);
				sr = new StreamReader(tempFile);
			}

			else
			{
				if (!File.Exists(fileName)) throw new FileNotFoundException("File not found: " + fileName);
				sr = new StreamReader(fileName);
			}

			XmlTextReader tr = new XmlTextReader(sr);
			DataTableMx dt = Deserialize(tr) as DataTableMx;
			sr.Close();

			if (tempFile != null)
				try { File.Delete(tempFile); }
				catch { }

			return dt;
		}

		/// <summary>
		/// Deserialize a DataTable from an XmlTextReader
		/// </summary>
		/// <param name="tr"></param>
		/// <returns></returns>

		public IDataTableMx Deserialize(
			XmlTextReader tr)
		{
			DataTableMx dt = new DataTableMx();
			dt.Columns.Add(RowAttributesColumnName, typeof(DataRowAttributes));
			dt.Columns.Add(CheckMarkColumnName, typeof(bool));

			tr.MoveToContent();

			if (!Lex.Eq(tr.Name, "DataTable"))
				throw new Exception("No \"DataTable\" element found");

			tr.Read();
			tr.MoveToContent();

			// Read DataColumns

			if (!Lex.Eq(tr.Name, "DataColumns"))
				throw new Exception("No \"DataColumns\" element found");

			tr.Read();
			tr.MoveToContent();

			while (true)
			{
				if (Lex.Eq(tr.Name, "DataColumn"))
				{
					string colName = tr.GetAttribute("ColumnName");
					string typeName = tr.GetAttribute("DataType");
					//					Type type = Type.GetType(typeName);
					Type type = typeof(object); // store all as object types
					System.Data.DataColumn dc = dt.Columns.Add(colName, type);
					tr.Read();
					tr.MoveToContent();
				}

				else if (tr.NodeType == XmlNodeType.EndElement && tr.Name == "DataColumns")
				{
					tr.Read();
					tr.MoveToContent();
					break;
				}

				else throw new Exception("Expected DataColumn or DataColumns end element but saw " + tr.Name);
			}

			// Read DataRows

			if (tr.Name != "DataRows")
				throw new Exception("No \"DataRows\" element found");

			object[] rowData = new object[dt.Columns.Count];

			bool isEmptyElement = tr.IsEmptyElement;

			tr.Read();
			tr.MoveToContent();

			if (!isEmptyElement)
			{
				while (true)
				{
					if (tr.NodeType == XmlNodeType.CDATA)
					{
						object[] oa = VoArray.DeserializeText(tr.Value);
						Array.Copy(oa, 0, rowData, 2, oa.Length);
						dt.Rows.Add(rowData);
						tr.Read();
						tr.MoveToContent();
					}

					else if (tr.NodeType == XmlNodeType.EndElement && tr.Name == "DataRows")
					{
						tr.Read();
						tr.MoveToContent();
						break;
					}

					else if (tr.Name == "ArrayOfAnyType") // ignore old form data
					{
						while (true)
						{
							tr.Read();
							tr.MoveToContent();
							if (tr.NodeType == XmlNodeType.EndElement && tr.Name == "ArrayOfAnyType")
								break;
						}

						tr.Read();
						tr.MoveToContent();
					}

					else throw new Exception("Expected ArrayOfAnyType or DataRows end element but saw " + tr.Name);
				}
			}

			if (tr.NodeType == XmlNodeType.EndElement && tr.Name == "DataTable")
			{
				return dt;
			}

			else throw new Exception("Expected DataTable end element but saw " + tr.Name);
		}

#if false // Flatten dataset row & add to list value objects
		/// <summary>
		/// Flatten dataset row & add to list value objects
		/// </summary>
		/// <param name="dr"></param>
		/// <param name="rf"></param>
		/// <returns></returns>

		public static List<object[]> FlattenDataRow(
			DataRowMx dr,
			ResultsFormat rf)
		{
			ResultsTable rt;
			QueryTable qt;
			QueryColumn qc;
			DataSet ds;
			DataTableMx dt;
			DataRow[] dRows = null;
			DataRelation dRel;
			object[] vo = null;
			int ti, ri, ci;

			Query q = rf.Query;
			ds = dr.Table.DataSet; // assoc data set

			List<object[]> voBuffer = new List<object[]>();

			object keyValue = dr[0]; // assume key value is in 1st position
			if (keyValue is CompoundId) keyValue = ((CompoundId)keyValue).Value; // get cid as string

			for (ti = 0; ti < q.Tables.Count; ti++) // process each table
			{
				rt = rf.Tables[ti];
				qt = q.Tables[ti];
				dt = ds.Tables[ti];
				if (ti == 0)
				{
					dRows = new DataRow[1];
					dRows[0] = dr;
				}
				else
				{
					dRel = ds.Relations[ti - 1]; // assume relations in same order as tables
					dt = dRel.ChildTable;
					dRows = dr.GetChildRows(dRel);
				}

				for (ri = 0; ri < dRows.Length; ri++)
				{
					dr = dRows[ri];
					if (ri >= voBuffer.Count)
					{
						vo = new Object[rf.VoLength]; // vo to be filled 
						vo[0] = keyValue; // copy key to common first element
						voBuffer.Add(vo); // add to array of flattened tuples
					}

					for (ci = 0; ci < qt.QueryColumns.Count; ci++)
					{
						qc = qt.QueryColumns[ci];
						if (!qc.Selected) continue;
						object value = dr[ci];
						if (value is DBNull) value = null;
						else if (value is CompoundId) value = ((CompoundId)value).Value; // get cid as string
						//						else if (value is ChemicalStructure) value = ((ChemicalStructure)value).Value;

						vo[qc.VoPosition] = value;
					}
				}
			} // end for result merge

			return voBuffer;
		}
#endif

	}
}
