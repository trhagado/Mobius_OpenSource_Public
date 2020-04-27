using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Mobius.Data
{

	/// <summary>
	/// List of maps for an analysis mapping the Spotfire DataTables in the analsis to the associated Mobius query table(s)/columns
	/// </summary>

	public class DataTableMapsMsx : IEnumerable<DataTableMapMsx>, IEnumerable
	{
		public List<DataTableMapMsx> MapList = new List<DataTableMapMsx>(); // List of mappings from Mobius queries to Spotfire DataTables and DataColumns that this view references

		public SpotfireViewProps SVP; // Spotfire View Properties that this instance of SpotfireDataTableMaps operates on

		public Query Query => SVP?.Query; // Mobius query that supplies data to the Spotfire view

		public DocumentMsx Document => SVP?.AnalysisApp?.Document; // current document

		public DataTableCollectionMsx DataTables => Document?.DataManager?.TableCollection;

		public static bool DevMode = false;

		/// <summary>
		/// Constructor that links to parent SpotfireViewProps
		/// </summary>
		/// <param name="svp"></param>

		public DataTableMapsMsx(SpotfireViewProps svp)
		{
			SVP = svp; // link maps to svp
			if (svp != null) svp.DataTableMaps = this; // link svp to maps
			return;
		}

		/// <summary>
		/// Indexer
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>

		public DataTableMapMsx this[int index]    // Indexer declaration  
		{
			get { return MapList[index]; }
		}
		public int Count => MapList.Count;

		/// <summary>
		/// Returns a SpotfireDataTableMap-typed IEnumerator to interate through MapList
		/// </summary>
		/// <returns></returns>

		IEnumerator<DataTableMapMsx> IEnumerable<DataTableMapMsx>.GetEnumerator()
		{
			return MapList.GetEnumerator();
		}

		/// <summary>
		/// Returns an IEnumerator to interate through MapList
		/// </summary>
		/// <returns></returns>

		IEnumerator IEnumerable.GetEnumerator()
		{
			return MapList.GetEnumerator();
		}

		/// <summary>
		/// The "current" map
		/// If null & maps exist return the first map
		/// </summary>

		public DataTableMapMsx CurrentMap
		{
			get
			{
				if (_currentMap != null) return _currentMap;

				else if (MapList != null && MapList.Count > 0)
				{
					_currentMap = MapList[0];
					return _currentMap;
				}

				else return null; // no current map
			}

			set
			{
				_currentMap = value;
			}
		}
		DataTableMapMsx _currentMap = null;

		/// <summary>
		/// Adjust the mapping so that it is in sync with the associated query and Spotfire analysis document
		/// </summary>
		/// <param name="svp"></param>

		public void ValidateMapsAgainstAnalysisDocument()
		{
			int dti, dci, dmi;
			Query q = Query;

			if (DataTables == null || DataTables.Count == 0) throw new Exception("No Spotfire data tables");
			if (q == null) throw new Exception("No Query");
			if (q.Tables.Count == 0) throw new Exception("No QueryTables");

			if (MapList == null || MapList.Count == 0) // anything defined in this map?
			{
				AssignInitialMapping();
				return;
			}

			// Check map for each Spotfire DataTable to be sure still valid
			// Also, set the DataTable / DataColumn links in the maps to connect to the DataTables / DataColumns in the current document

			List<DataTableMapMsx> newMapList = new List<DataTableMapMsx>(); // build new maplist here

			for (dti = 0; dti < DataTables.Count; dti++)
			{
				DataTableMsx dt = DataTables[dti];

				DataTableMapMsx newMap = new DataTableMapMsx(); // build new map here
				newMap.ParentMapList = this; // existing map as parent
				newMapList.Add(newMap); // add to new list

				newMap.SpotfireDataTable = dt;

				DataTableMapMsx currentMap = GetMapForDataTable(dt);

				if (currentMap == null) // no existing map for Spotfire data table?
				{
					newMap.InitializeMapForDataTable(dt); // init but don't map
					continue;
				}

				// Build new map based on current map

				HashSet<QueryTable> qtSet;
				HashSet<QueryColumn> qcSet;
				newMap.QueryTable = currentMap.QueryTable;
				newMap.SummarizationOneRowPerKey = currentMap.SummarizationOneRowPerKey;

				currentMap.GetQueryTablesAndColumnsMappedTo(out qtSet, out qcSet);

				// Get existing or new mapping for each column in DataTable

				for (dci = 0; dci < dt.Columns.Count; dci++)
				{
					DataColumnMsx dc = dt.Columns[dci];
					ColumnMapMsx cm = currentMap.FindAndUpdateColumnMapForDataColumn(dc);

					if (cm == null) // allocate new map if SF column not included in current map
					{
						cm = ColumnMapMsx.BuildFromSpotfireDataColumn(dc);
						if (dci == 0) // if first col be sure mapped
						{
							if (currentMap?.QueryTable != null)
							{
								cm.QueryColumn = currentMap?.QueryTable?.KeyQueryColumn; // use key for current query table
								if (cm.QueryColumn == null) cm.QueryColumn = Query.Tables[0].KeyQueryColumn; // if not defined use key from first query table
							}
						}
					}

					if (qtSet.Count > 0) // if mapped then set file column name
					{
						QueryColumn qc = cm.QueryColumn;
						if (qc == null) // undefined QC?
						{
							cm = cm; // todo define QC
						}

						if (qc != null) // be sure MobiusFileColumnName is up to date
							cm.MobiusFileColumnName = qc.MetaTableDotMetaColumnName + ColumnMapParms.SpotfireExportExtraColNameSuffix;
					}

// If a col has been renamed previously, then restore the original name and setup to perform the rename

					if (Lex.IsDefined(cm.Role) && Lex.IsDefined(cm.SpotfireColumnName) &&
						Lex.Ne(cm.Role, cm.SpotfireColumnName))
					{
						cm.NewSpotfireColumnName = cm.SpotfireColumnName;
						cm.SpotfireColumnName = cm.Role;
					}

					newMap.Add(cm);
				}

				if (qtSet.Count > 0) // if mapped then be sure all QueryColumns for the mapped QueryTable(s) are mapped
					newMap.AddUnassignedMobiusQueryColumnsToMap();

			} // DataTable loop

			MapList = newMapList;
			return;
		}

		/// <summary>
		/// GetMapForDataTable
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>

		public DataTableMapMsx GetMapForDataTable(
			DataTableMsx dt)
		{
			if (dt == null) throw new NullReferenceException("DataTableMsx is null");

			foreach (DataTableMapMsx dm in MapList)
			{
				if (Lex.Eq(dm.SpotfireTableName, dt.Name) || Lex.Eq(dm.SpotfireTableId, dt.Id))
					return dm;
			}

			return null;
		}


		/// <summary>
		/// Assign initial mapping
		/// </summary>

		public void AssignInitialMapping()
		{
			QueryTable qt = null;
			Query q = Query;

			VerifyThatMainObjectsAreDefined();

			MapList = new List<DataTableMapMsx>();

			if (DataTables.Count == 1)
			{
				//WriteSingleDataFile = true;
				qt = null;
			}

			else // multiple data tables
			{
				//WriteMultipleDataFiles = true;
				qt = q.Tables[0]; // start with map of single table
			}

			for (int dti = 0; dti < DataTables.Count; dti++)
			{
				DataTableMsx dt = DataTables[dti];
				DataTableMapMsx dm = new DataTableMapMsx();
				dm.ParentMapList = this;
				MapList.Add(dm);

				dm.SpotfireDataTable = dt;
				if (dti == 0) // map just the first Spotfire data table for now
				{
					dm.AssignInitialMappingForQueryTable(qt);
				}

				else // just initialize the map for the table, don't assign any QueryTable/QueryColumns
				{
					dm.InitializeMapForDataTable(dt);
				}
			}

			return;
		}

		/// <summary>
		/// VerifyThatMainObjectsAreDefined
		/// </summary>

		void VerifyThatMainObjectsAreDefined()
		{
			try
			{
				bool checkQuery = false;

				if (checkQuery)
				{
					if (Query == null) throw new Exception("No Query");
					if (Query.Tables.Count == 0) throw new Exception("No QueryTables");
				}

				if (SVP == null) throw new Exception("Spotfire View Properties (SVP) not defined.");
				if (Document == null) throw new Exception("Document not defined.");
				if (DataTables == null || DataTables.Count == 0) throw new Exception("Spotfire DataTables not defined");
			}

			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}

			return;
		}

		/// <summary>
		/// Add unassigned QueryColumns from the query to the map for each Spotfire DataTable
		/// </summary>

		public void AddUnassignedMobiusQueryColumns()
		{
			if (Query == null) return;

			foreach (DataTableMapMsx dtm in MapList)
			{
				dtm.AddUnassignedMobiusQueryColumnsToMap();
			}

			return;
		}


		public void UpdateSpotfireDataTablesAndMapsAfterImport(
			string newDataTableXml)
		{
			List<DataTableMsx> updatedDataTables = UpdateSpotfireDataTablesFromXml(newDataTableXml); // update the datatable definitions

			RebuildMapsFromSpotfireDataTables(updatedDataTables);

			//UpdateMapsFromSpotfireDataTables(); 

			return;
		}

		public List<DataTableMsx> UpdateSpotfireDataTablesFromXml(string serializedDataTables)
		{
			DataTableMsx dt;

			List<DataTableMsx> dtList = new List<DataTableMsx>();

			if (serializedDataTables == null) serializedDataTables = "";

			List<string> xmlList = Lex.GetSubstringsBetweenInclusive(serializedDataTables, "<DataTableMsx", "</DataTableMsx>");

			foreach (string dtXml in xmlList)
			{
				dt = (DataTableMsx)SerializeMsx.Deserialize(dtXml, typeof(DataTableMsx));
				if (dt == null) continue;

				dt.SetOwner(DataTables);
				dt.SetApp(DataTables.App);

				dt.UpdatePostDeserializationSecondaryReferences();
				dtList.Add(dt);

				DataTableMsx dt0 = DataTables.GetTableById(dt.Id);
				if (dt0 != null) DataTables.ReplaceTableWithUpdatedVersion(dt);
			}

			return dtList;
		}

		/// <summary>
		/// RebuildDataTableMapsFromSpotfireDataTables
		/// </summary>
		/// <param name="updatedDataTables"></param>

		public void RebuildMapsFromSpotfireDataTables(List<DataTableMsx> updatedDataTables)
		{
			foreach (DataTableMsx dt in updatedDataTables)
			{
				DataTableMapMsx dtm = GetMapForDataTable(dt);
				if (dtm == null) continue;

				dtm.RebuildDataTableMapFromSpotfireDataTable(dt);
			}
		}

		/// <summary>
		/// Adjust the maps as necessary for any changes in the underlying Spotfire data tables
		/// </summary>

		public void UpdateMapsFromSpotfireDataTables()
		{
			UpdateDataTableMapsToUseNewSpotfireColumnNames(); // update maps to use newly assigned column names

			foreach (DataTableMapMsx dtm in this.MapList)
			{
				if (dtm.SpotfireDataTable == null) continue;

				DataTableMsx dt = DataTables.GetTableById(dtm.SpotfireDataTable.Id);
				if (dt == null) continue;

				dtm.SpotfireDataTable = dt; // be sure we're referncing the current instance of the table
				dtm.SynchronizeMapWithQueryAndSpotfireDataTable();
			}
		}

		/// <summary>
		/// Update maps to use newly assigned column names
		/// </summary>

		public void UpdateDataTableMapsToUseNewSpotfireColumnNames()
		{
			foreach (DataTableMapMsx dtm in this)
			{
				dtm.ColumnMapCollection.UpdateColumnMapsToUseNewSpotfireColumnNames();
			}

			return;
		}

		/// <summary>
		/// Allocate and add a new map
		/// </summary>
		/// <returns></returns>

		public DataTableMapMsx Add()
		{
			DataTableMapMsx dm = new DataTableMapMsx(this);
			return dm;
		}

		/// <summary>
		/// Allocate and add a new map
		/// </summary>
		/// <returns></returns>

		public void Add(DataTableMapMsx dm)
		{
			MapList.Add(dm);
			dm.ParentMapList = this;
			return;
		}

		/// <summary>
		/// Serialize
		/// </summary>
		/// <param name="tw"></param>

		public void Serialize(XmlTextWriter tw)
		{
			tw.WriteStartElement("DataTableMaps");

			foreach (DataTableMapMsx dm in MapList)
			{
				dm.Serialize(tw);
			}

			tw.WriteEndElement();
			return;
		}

		/// <summary>
		/// Deserialize
		/// </summary>
		/// <param name="tr"></param>
		/// <param name="query"></param>
		/// <returns></returns>

		static internal DataTableMapsMsx Deserialize(
			Query query,
			XmlTextReader tr,
			SpotfireViewProps svp)
		{
			DataTableMapsMsx maps = new DataTableMapsMsx(svp);

			while (true)
			{
				if (!XmlUtil.MoreSubElements(tr, "DataTableMaps")) break;

				else if (Lex.Eq(tr.Name, "DataTableMap"))
					DataTableMapMsx.Deserialize(query, tr, maps);

				else throw new Exception("Unexpected element: " + tr.Name);
			}

			return maps;
		}
	}

	/// <summary>
	/// Maps a single Spotfire DataTable in the analsis an the associated Mobius query
	/// </summary>

	public class DataTableMapMsx : IEnumerable<ColumnMapMsx>, IEnumerable
	{
		public Query Query => SVP?.Query;

		public DataTableMsx SpotfireDataTable = null; // associated Spotfire data table
		public string SpotfireTableName { get => GetSpotfireTableName(); set => SetSpotfireTableName(value); } // Spotfire table name if SpotfireDataTable is/is-not defined
		public string SpotfireTableId { get => GetSpotfireTableId(); set => SetSpotfireTableId(value); } // Spotfire table id if SpotfireDataTable is/is-not defined

		public QueryTable QueryTable = null; // QueryTable that supplies data to the DataTable. Null if no table or multiple tables. Must check GetQueryTablesCount() to know which

		public ColumnMapCollection ColumnMapCollection = new ColumnMapCollection(); // mapping from query columns to external columns

		public List<ColumnMapMsx> ColumnMapList // abbreviation for list of column maps
		{
			get => ColumnMapCollection.ColumnMapList;
			set => ColumnMapCollection.ColumnMapList = value; 
		}

		public bool IsMapped => (GetQueryTablesCount() > 0);
		public bool IsMappedToSingleQueryTable => (QueryTable != null && GetQueryTablesCount() == 1);
		public bool IsMappedToAllQueryTables => (QueryTable == null && GetQueryTablesCount() > 0);
		public bool IsUnmapped => (QueryTable == null && GetQueryTablesCount() == 0);

		public string FileUrl => GetFileUrl(); // URL of most-recent file exported for the DataTable

		/// <summary>
		/// Summarization of Mobius data when writing files
		/// </summary>

		public bool SummarizationOneRowPerKey = true;
		public bool SummarizationAsIs
		{
			get => !SummarizationOneRowPerKey;
			set => SummarizationOneRowPerKey = !value;
		}

// List of ColumnMaps for the DataTableMap

		public ColumnMapMsx this[int index]    // Indexer declaration  
		{
			get { return ColumnMapList[index]; }
		}

		public int Count => ColumnMapList.Count;

		/// <summary>
		/// Add a ColumnMap to this list
		/// </summary>
		/// <param name="cm"></param>
		/// <returns></returns>

		public ColumnMapMsx Add(ColumnMapMsx cm)
		{
			ColumnMapList.Add(cm);
			return cm;
		}

		public void Clear()
		{
			ColumnMapList.Clear();
		}

		/// <summary>
		/// Returns a ColumnMapMsx-typed IEnumerator  to interate through TableList
		/// </summary>
		/// <returns></returns>

		IEnumerator<ColumnMapMsx> IEnumerable<ColumnMapMsx>.GetEnumerator()
		{
			return ColumnMapList.GetEnumerator();
		}

		/// <summary>
		/// Returns an IEnumerator to interate through ColumnMapList
		/// </summary>
		/// <returns></returns>

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ColumnMapList.GetEnumerator();
		}

		public DataTableMapsMsx ParentMapList  // parent map list this map belongs to
		{
			get => GetParent();
			set => _parent = value;
		}
		private DataTableMapsMsx _parent = null;

		public SpotfireViewProps SVP // associated view properties including the Spotfire analysis document model
		{
			get => ParentMapList?.SVP;
			set => ParentMapList.SVP = value;
		}

		/**************************************************************************/
		/******************************* Methods **********************************/
		/**************************************************************************/

		/// <summary>
		/// Basic constructor
		/// </summary>

		public DataTableMapMsx()
		{
			return;
		}

		/// <summary>
		/// Constructor that links to parent SpotfireDataTableMaps (can be null initially)
		/// </summary>
		/// <param name="maps"></param>

		public DataTableMapMsx(DataTableMapsMsx maps)
		{
			ParentMapList = maps;
			if (maps != null) maps.Add(this);
			return;
		}

		DataTableMapsMsx GetParent() // throws exception on null parent
		{
			if (_parent != null) return _parent;
			else throw new NullReferenceException("Parent is null");
		}

		public string GetSpotfireTableName()
		{
			if (SpotfireDataTable != null) return SpotfireDataTable.Name;
			else return _spotfireTableName;
		}

		public void SetSpotfireTableName(string name)
		{
			_spotfireTableName = name;
			if (SpotfireDataTable != null) SpotfireDataTable.Name = name;
		}
		private string _spotfireTableName = null; // backup Id in case SpotfireDataTable is null

		public string GetSpotfireTableId()
		{
			if (SpotfireDataTable != null) return SpotfireDataTable.Id;
			else return _spotfireTableId;
		}

		public void SetSpotfireTableId(string id)
		{
			_spotfireTableId = id;
			if (SpotfireDataTable != null) SpotfireDataTable.Id = id;
		}
		private string _spotfireTableId = ""; // backup Id in case SpotfireDataTable is null

		/// <summary>
		/// AssignInitialMappingForQueryTable 
		/// </summary>
		/// <param name="qt">Qt to assign to DataTable or null to assign all QTs in query to the DataTable</param>

		public void AssignInitialMappingForQueryTable(
		QueryTable qt)
		{
			InitializeMapForCurrentSpotfireDataTable();
			AssignBestMatchMobiusColumnsToSpotfireColumns(qt);
			AssignUniqueSpotfireNamesFromAssociatedMobiusColumnNames();
			return;
		}

		/// <summary>
		/// Initialize the map for a Spotfire DataTable
		/// </summary>

		public void InitializeMapForDataTable(
			DataTableMsx dt)
		{
			SpotfireDataTable = dt;
			InitializeMapForCurrentSpotfireDataTable();
		}

		/// <summary>__
		/// Initialize the map for the current Spotfire DataTable
		/// </summary>

		public void InitializeMapForCurrentSpotfireDataTable()
		{
			DataTableMsx dt = SpotfireDataTable;
			List<DataColumnMsx> newCols = new List<DataColumnMsx>();

			foreach (DataColumnMsx dc in dt.Columns)
			{
				if (Lex.IsDefined(dc.MobiusRole)) // set role to the initial column name if not yet defined
				{
					dc.Name = dc.MobiusRole; // restore col name
					dc.MobiusFileColumnName = ""; // clear any existing map to a Mobius column
					newCols.Add(dc);
				}
			}

			//public static string ResetDataTable(
			//string id)
			//{
			//	DataTable dt = GetDataTableByNameOrIdWithException(id);

			//	RemoveRowsFromDataTable(dt);

			//	RemovePreviouslyAddedMobiusColumnsFromDataTable(dt);
			//	ResetOriginalSpotfireColumnNamesFromMobiusRoleNames(dt);

			//	DataTableMsx dtMsx = DataTableConverter.S2M(dt);
			//	string serializedDt = MsxUtil.Serialize(dtMsx);
			//	return serializedDt;
			//}

			dt.Columns = newCols;

			ColumnMapCollection = new ColumnMapCollection();

			foreach (DataColumnMsx dc in dt.Columns)
			{
				//if (Lex.IsUndefined(dc.MobiusRole)) // set role to the initial column name if not yet defined
					//dc.MobiusRole = dc.Name; 

				ColumnMapMsx cm = ColumnMapMsx.BuildFromSpotfireDataColumn(dc);
				ColumnMapCollection.Add(cm);
			}

			return;
		}

		/// <summary>
		/// RebuildDataTableMapFromSpotfireDataTable
		/// </summary>
		/// <param name="dt"></param>

		public void RebuildDataTableMapFromSpotfireDataTable(
			DataTableMsx dt)
		{
			ColumnMapMsx cm;
			QueryColumn qc;
			string qcRef;

			Query q = GetParent()?.Query;

			List<ColumnMapMsx> originalColumnMaps = new List<ColumnMapMsx>(ColumnMapList); // make a copy for debug purposes
			List<DataColumnMsx> dataColsWithoutQueryColumns = new List<DataColumnMsx>(); // for debug
			HashSet<string> qcRefsSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // for debug

			SpotfireDataTable = dt; // reference possibly new datatable instance
			ColumnMapList.Clear();

// Add cols with roles first (i.e. cols originally in the table)

			foreach (DataColumnMsx dc in SpotfireDataTable.Columns)
			{
				if (Lex.IsUndefined(dc.MobiusRole)) continue;

				qcRef = ColumnMapMsx.GetQcRefFromDataColumn(dc);
				if (Lex.IsUndefined(qcRef))
				{
					qc = null; // DataColumn not mapped
					dataColsWithoutQueryColumns.Add(dc);
				}
				else
				{
					qc = QueryColumn.GetQueryColumnFromRefString(q, qcRef);
					if (qc == null) // did we get a QC?
					{ // possibly a qualified number subcolumns (e.g. qualifier) that Mobius exports but that are not currently supported in the map
						continue;
					}
				}
				qcRefsSeen.Add(qcRef); // keep set of Qc references seen

				cm = new ColumnMapMsx();
				cm.QueryColumn = qc;
				cm.SetSpotfireFieldsFromDataColumn(dc);

				ColumnMapList.Add(cm);
			}

			// Add cols without roles

			foreach (DataColumnMsx dc in SpotfireDataTable.Columns)
			{
				if (Lex.IsDefined(dc.MobiusRole)) continue;

				qcRef = ColumnMapMsx.GetQcRefFromDataColumn(dc);
				qc = QueryColumn.GetQueryColumnFromRefString(q, qcRef);
				if (qc == null)
				{ // note that this can currently happen for qualified number subcolumns (e.g. qualifier) that Mobius exports but that are not currently supported in the map
					dataColsWithoutQueryColumns.Add(dc);
					continue;
				}
				qcRefsSeen.Add(qcRef); // keep set of Qc references seen

				cm = new ColumnMapMsx();
				cm.QueryColumn = qc;
				cm.SetSpotfireFieldsFromDataColumn(dc);

				ColumnMapList.Add(cm);
			}

			return;
		}

	/// <summary>
	/// After a Spotfire Datatable is loaded with new data make sure the map is in synch with the table
	/// </summary>

	public void SynchronizeMapWithQueryAndSpotfireDataTable()
		{
			ColumnMapMsx cm;
			QueryColumn qc;

			Query q = GetParent()?.Query;

// Assure that all data table columns are in the map

			List<ColumnMapMsx> colMapsWithRoles = new List<ColumnMapMsx>();
			List<ColumnMapMsx> colMapsWithoutRoles = new List<ColumnMapMsx>();
			List<ColumnMapMsx> newColumnMaps = new List<ColumnMapMsx>(); // added maps
			List<DataColumnMsx> dataColsWithoutQueryColumns = new List<DataColumnMsx>();

			foreach (DataColumnMsx dc in SpotfireDataTable.Columns)
			{
				cm = FindColumnMapForDataColumn(dc);
				if (cm != null)
					ColumnMapList.Remove(cm); // remove from existing map so we can check what's left at the end

				else  // no column map, build new one
				{
				string qcRef = ColumnMapMsx.GetQcRefFromDataColumn(dc);
				qc = QueryColumn.GetQueryColumnFromRefString(q, qcRef);
					if (qc == null)
					{ // note that this can currently happen for qualified number subcolumns (e.g. qualifier) that Mobius exports but that are not currently supported in the map
						dataColsWithoutQueryColumns.Add(dc);
						continue;
					}

					cm = new ColumnMapMsx();
					cm.QueryColumn = qc;
					cm.SetSpotfireFieldsFromDataColumn(dc);
					newColumnMaps.Add(cm);
				}

				if (Lex.IsDefined(cm.Role))
					colMapsWithRoles.Add(cm);

				else colMapsWithoutRoles.Add(cm);
			}

			List<ColumnMapMsx> leftOverColumnMaps = new List<ColumnMapMsx>(ColumnMapList);

			ColumnMapList.Clear();
			ColumnMapList.AddRange(colMapsWithRoles);
			ColumnMapList.AddRange(colMapsWithoutRoles);

// Assure that all mappings to QueryColumns are still valid for the query

			// todo...

			return;
		}

		/// <summary>
		/// Get the number of Mobius QueryTables that the Spotfire Data is mapped to
		/// </summary>
		/// <returns></returns>

		public int GetQueryTablesCount()
		{
			HashSet<QueryTable> qtSet;
			HashSet<QueryColumn> qcSet;

			GetQueryTablesAndColumnsMappedTo(out qtSet, out qcSet);
			return qtSet.Count;
		}

		/// <summary>
		/// Get the list of Mobius QueryTables that the Spotfire Data is mapped to
		/// </summary>
		/// <returns></returns>

		public void GetQueryTablesAndColumnsMappedTo(
			out HashSet<QueryTable> qtSet,
			out HashSet<QueryColumn> qcSet)
		{
			qtSet = new HashSet<QueryTable>();
			qcSet = new HashSet<QueryColumn>();

			foreach (ColumnMapMsx cm in ColumnMapList)
			{
				if (cm?.QueryColumn?.QueryTable != null) // mapped
				{
					qtSet.Add(cm.QueryColumn.QueryTable);
					qcSet.Add(cm.QueryColumn);
				}
			}

			return;
		}

		/// <summary>
		/// Look for an existing column map that matches the DataColumn.
		/// if a match is found update it and the datacolumn 
		/// </summary>
		/// <param name="dc"></param>
		/// <returns></returns>

		public ColumnMapMsx FindAndUpdateColumnMapForDataColumn(
			DataColumnMsx dc)
		{
			ColumnMapMsx cm = FindColumnMapForDataColumn(dc);
			if (cm == null) return null;

			if (Lex.IsDefined(cm.Role) && Lex.IsUndefined(dc.MobiusRole)) // keep any name from existing map
				dc.MobiusRole = cm.Role;

			if (Lex.IsDefined(cm.MobiusFileColumnName)) // keep any spotfire file name from existing map
				dc.MobiusFileColumnName = cm.MobiusFileColumnName;

			dc.Name = cm.SpotfireColumnName; // keep any 
			cm.SpotfireColumn = dc; // link column map to column

			return cm;
		}

		/// <summary>
		/// Get any existing map for the specified DataColumn
		/// </summary>
		/// <param name="dc"></param>
		/// <returns></returns>

		public ColumnMapMsx FindColumnMapForDataColumn(
			DataColumnMsx dc)
		{
			foreach (ColumnMapMsx cm in ColumnMapList) // try to match DataColumn objects
			{
				if (cm.SpotfireColumn == dc) return cm;
			}

			foreach (ColumnMapMsx cm in ColumnMapList) // try to match by role 
			{
				if (Lex.Eq(cm.Role, dc.MobiusRole)) return cm;
			}

			foreach (ColumnMapMsx cm in ColumnMapList) // try to match by Spotfire column name
			{
				if (Lex.Eq(cm.SpotfireColumnName, dc.Name))
				{
					return cm;
				}
			}

			foreach (ColumnMapMsx cm in ColumnMapList) // try to match Spotfire document column name to metacolumn name
			{
				if (cm.QueryColumn != null && Lex.Eq(cm.QueryColumn.MetaColumn.Name, dc.Name))
				{
					return cm;
				}
			}

			return null; // no match
		}

		/// <summary>
		/// Get the URL the exported file exported to be mapped to the Spotfire DataTable
		/// </summary>
		/// <returns></returns>

		string GetFileUrl()
		{
			string tn = this.SpotfireTableName; // debug

			if (DataTableMapsMsx.DevMode) return GetFileUrlDev(); // dev/debug

			string baseFileName = SpotfireViewProps.GetBaseExportFileName(Query);

			HashSet<QueryTable> qtSet;
			HashSet<QueryColumn> qcSet;
			GetQueryTablesAndColumnsMappedTo(out qtSet, out qcSet);

			if (qtSet.Count == 0) baseFileName = ""; // table not mapped

			else if (qtSet.Count == 1) // mapped to single table
			{
				string qtName = qtSet.First<QueryTable>().MetaTableName;
				baseFileName = Query.SpotfireDataFileName + "_" + qtName;
			}

			else baseFileName = Query.SpotfireDataFileName; // mapped to all tables

			string url = SpotfireViewProps.GetFullExportFileUrlFromBaseFileName(baseFileName);
			return url;
		}

		/// <summary>
		/// Dev version of GetFileUrl
		/// </summary>
		/// <returns></returns>

		string GetFileUrlDev()
		{
			string fileUrl = "";

			if (this.IsMappedToSingleQueryTable)
			{
				fileUrl = "<fileURL>.<fileName>.stdf";
				throw new NotImplementedException();
			}

			else if (this.IsMappedToAllQueryTables)
			{
				fileUrl = "<fileURL>.<fileName>.stdf";
				throw new NotImplementedException();
			}

			else fileUrl = ""; // table not mapped

			return fileUrl;
		}

		/// <summary>
		/// GetMatchingMobiusDataTypeFromSpotfireColumn
		/// </summary>
		/// <param name="dc"></param>
		/// <returns></returns>

		public static MetaColumnType GetMatchingMobiusDataTypeFromSpotfireColumn(DataColumnMsx dc)
		{
			MetaColumnType mct = MetaColumnType.Unknown;

			if (dc == null) return MetaColumnType.Unknown;

			if (dc.IsStructureColumn) return MetaColumnType.Structure;

			switch (dc.DataType)
			{
				case DataTypeMsxEnum.Integer:
				case DataTypeMsxEnum.LongInteger:
					return MetaColumnType.Integer;

				case DataTypeMsxEnum.Real:
				case DataTypeMsxEnum.SingleReal:
					return MetaColumnType.Number;

				case DataTypeMsxEnum.String:
					return MetaColumnType.String;

				case DataTypeMsxEnum.DateTime:
				case DataTypeMsxEnum.Date:
					return MetaColumnType.Date;

				case DataTypeMsxEnum.Binary:
					return MetaColumnType.Binary;

				case DataTypeMsxEnum.Boolean:
				case DataTypeMsxEnum.Currency:
				case DataTypeMsxEnum.Time:
				case DataTypeMsxEnum.TimeSpan:
				default:
					return MetaColumnType.Unknown;
			}
		}

		public static void SetMatchingSpotfireDataTypeFromMobiusColumn(
			MetaColumn mc,
			DataColumnMsx dc)
		{
			if (mc.DataType == MetaColumnType.CompoundId)
			{
				if (mc.StorageType == MetaColumnStorageType.String)
					dc.DataType = DataTypeMsxEnum.String;

				else dc.DataType = DataTypeMsxEnum.LongInteger;
			}

			else if (mc.DataType == MetaColumnType.Structure)
			{
				dc.DataType = DataTypeMsxEnum.String;
				dc.ContentType = "chemical/x-mdl-molfile";  // chemical/x-mdl-molfile, chemical/x-mdl-chime  or chemical/x-daylight-smiles
			}

			else
			{
				dc.DataType = GetMatchingSpotfireDataTypeForMobiusMetaColumnType(mc.DataType);
				//DataTypeMsx sfType = DataTypeMsx.EnumToDataType(sfTypeEnum);
				//dc.DataType = sfType;
			}

			return;
		}

		public static DataTypeMsxEnum GetMatchingSpotfireDataTypeForMobiusMetaColumnType(
			MetaColumnType mcType)
		{
			switch (mcType)
			{
				case MetaColumnType.Integer:
					return DataTypeMsxEnum.Integer;

				case MetaColumnType.Number:
				case MetaColumnType.QualifiedNo:
					return DataTypeMsxEnum.Real;

				case MetaColumnType.String:
					return DataTypeMsxEnum.String;

				case MetaColumnType.Date:
					return DataTypeMsxEnum.Date;

				case MetaColumnType.Binary:
					return DataTypeMsxEnum.Binary;

				default:
				case MetaColumnType.Hyperlink:
				case MetaColumnType.Html:
				case MetaColumnType.Structure:
				case MetaColumnType.MolFormula:
				case MetaColumnType.Image:
					return DataTypeMsxEnum.String;
			}
		}

		/// <summary>
		/// AssignBestCompatibleMatchOfMobiusColumnsToSpotfireColumns
		/// </summary>

		public void AssignBestMatchMobiusColumnsToSpotfireColumns(
			QueryTable qt)
		{
			if (Query == null) return;

			HashSet<QueryColumn> assignedColumns = new HashSet<QueryColumn>();

			foreach (ColumnMapMsx cm in ColumnMapList)
			{
				QueryColumn qc = GetBestCompatibleQueryColumn(cm, qt, assignedColumns);
				if (qc != null)
				{
					cm.QueryColumn = qc;
					cm.MobiusFileColumnName = qc.MetaTableDotMetaColumnName + ColumnMapParms.SpotfireExportExtraColNameSuffix;
					assignedColumns.Add(qc);
				}
			}

			AddUnassignedMobiusQueryColumnsToMap();

			return;
		}

		/// <summary>
		/// GetBestCompatibleQueryColumn
		/// </summary>
		/// <param name="cm"></param>
		/// <param name="assignedColumns"></param>
		/// <param name="mustBeUnassigned"></param>
		/// <returns></returns>

		QueryColumn GetBestCompatibleQueryColumn(
			ColumnMapMsx cm,
			QueryTable qt,
			HashSet<QueryColumn> assignedColumns,
			bool mustBeUnassigned = true)
		{
			QueryColumn qc;

			List<QueryTable> qts;

			if (qt != null) // map to single query table
			{
				qts = new List<QueryTable>();
				qts.Add(qt);
			}

			else // map to all query tables
			{
				qts = Query.Tables;
			}

			DataColumnMsx dc = cm?.SpotfireColumn;
			if (dc == null) return null; // can't match if no Spotfire col to match against

			MetaColumnType dcMcType = GetMatchingMobiusDataTypeFromSpotfireColumn(dc);

			// If Spotfire and QC names match and types are compatible then use that match
			// Otherwise find the best match in four passes
			// 1. Matching names & compatible types
			// 2. Priorty columns & equivalent types
			// 3. Priorty columns & compatible types
			// 4. Other columns & equivalent types
			// 5. Other columns & compatible types

			for (int step = 1; step <= 5; step++)
			{
				bool MATCHING_NAME_STEP = (step == 1);
				bool PRIORITY_COLUMN_STEP = (step == 2 || step == 3);
				bool EQUIVALENT_TYPE_STEP = (step == 2 || step == 4);

				for (int qti = 0; qti < qts.Count; qti++) // iterate over query tables
				{
					qt = qts[qti];

					for (int qci = 0; qci < qt.QueryColumns.Count; qci++) // iterate over query columns
					{
						qc = qt.QueryColumns[qci];
						MetaColumn mc = qc.MetaColumn;

						if (!qc.Selected) continue; // must be selected

						if (qc.IsKey && qti != 0) continue; // select key only from first table

						if (mustBeUnassigned && assignedColumns.Contains(qc)) continue;

						bool equivalentTypes = // true if types are equivalent
							(mc.DataType == dcMcType) || // exact type
							(mc.IsInteger && dc.IsInteger) ||
							(mc.IsReal && dc.IsReal) || // check float
							(mc.IsString && dc.IsString) ||
							(mc.IsDate && dc.IsDateTime);

						bool compatibleTypes = equivalentTypes || // true if types are compatible
							(mc.IsInteger && dc.IsNumeric); // Mobius integer to Spotfire numeric

						if (!compatibleTypes) continue; // must be compatible types

						if (MATCHING_NAME_STEP)
						{
							int score = GetQcNameToSpotfireNameMatchScore(qc, dc);
							if (score >= 0) return qc;
							else continue;
						}

						if (PRIORITY_COLUMN_STEP) // key type match
						{
							if (mc.IsKey && qti == 0) // assign first key column if types compatible
							{
								{
									if (mc.IsInteger && dc.IsInteger) return qc;
									else if (mc.IsString && dc.IsString) return qc;
								}
							}

							else if (!mc.PrimaryResult && !mc.SecondaryResult) // primary and secondary results also get priority
								continue;
						}

						if (EQUIVALENT_TYPE_STEP && equivalentTypes) // equivalent type match
							return qc;

						else return qc; // compatible types
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Get a set of the MetaColumnTypes that are compatible with and can be exported to the supplied Spotfire column
		/// </summary>
		/// <param name="spotfireColumn"></param>
		/// <returns></returns>

		public static HashSet<MetaColumnType> GetMetaColumnTypesCompatibleWithSpotfireColumn(
			DataColumnMsx spotfireColumn,
			bool allowCompoundId)
		{
			HashSet<MetaColumnType> hs = null;

			if (spotfireColumn == null)
				return new HashSet<MetaColumnType>(MetaColumn.AllMetaColumnTypes);

			switch (spotfireColumn.DataType)
			{
				case DataTypeMsxEnum.Integer:
				case DataTypeMsxEnum.LongInteger:
					hs = new HashSet<MetaColumnType>() { MetaColumnType.Integer };
					if (allowCompoundId) hs.Add(MetaColumnType.CompoundId);
					return hs;

				case DataTypeMsxEnum.Real:
				case DataTypeMsxEnum.SingleReal:
					return new HashSet<MetaColumnType>(MetaColumn.NumericMetaColumnTypes);

				case DataTypeMsxEnum.String:
					if (spotfireColumn.IsStructureColumn)
						return new HashSet<MetaColumnType>() { MetaColumnType.Structure };

					hs = new HashSet<MetaColumnType>(MetaColumn.AllMetaColumnTypes);
					if (allowCompoundId) hs.Add(MetaColumnType.CompoundId);
					return hs;

				case DataTypeMsxEnum.DateTime:
				case DataTypeMsxEnum.Date:
					return new HashSet<MetaColumnType>() { MetaColumnType.Date };

				case DataTypeMsxEnum.Boolean:
				case DataTypeMsxEnum.Binary:
				case DataTypeMsxEnum.Currency:
				case DataTypeMsxEnum.Time:
				case DataTypeMsxEnum.TimeSpan:
					return new HashSet<MetaColumnType>();

				default:
					return new HashSet<MetaColumnType>() { MetaColumnType.Any };
			}
		}

		/// <summary>
		/// Score the quality of a name match between a QueryColumn and a Spotfire DataColumn
		/// -1 = no match otherwise the smaller the better
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="dc"></param>
		/// <returns></returns>

		public static int GetQcNameToSpotfireNameMatchScore(
			QueryColumn qc,
			DataColumnMsx dc)
		{
			string[] qcNames = new string[]
			{
				qc.ActiveLabel,
				qc.Label,
				qc.MetaColumn.Label,
				qc.MetaColumn.Name,
				qc.MetaTableDotMetaColumnName
			};

			string[] dcNames = new string[]
			{
				dc.Name,
				dc.MobiusRole,
				dc.MobiusFileColumnName,
				dc.ExternalName
			};

			int qcnl = qcNames.Length;
			int dcnl = dcNames.Length;

			for (int qci = 0; qci < qcnl; qci++)
			{
				string qcn = qcNames[qci];
				if (string.IsNullOrWhiteSpace(qcn)) continue;

				for (int dci = 0; dci < dcnl; dci++)
				{
					string ecn = dcNames[dci];
					if (string.IsNullOrWhiteSpace(ecn)) continue;

					if (string.Equals(ecn, qcn, StringComparison.OrdinalIgnoreCase))
					{

						int score = (qci * 10) + dci; // allow easy determination of elements
						return score;
					}
				}
			}

			return -1;
		}


		/// <summary>
		/// Add unassigned QueryColumns from the query to the map
		/// </summary>

		public void AddUnassignedMobiusQueryColumnsToMap()
		{
			HashSet<QueryColumn> qcSet;
			HashSet<QueryTable> qtSet;

			if (Query == null) return;

			GetQueryTablesAndColumnsMappedTo(out qtSet, out qcSet);

			//HashSet<QueryColumn> assignedColumns = new HashSet<QueryColumn>();

			//foreach (ColumnMapMsx cm in ColumnMapList) // get list of query columns that are assigned
			//{
			//	QueryColumn qc = cm.QueryColumn;
			//	if (qc != null)
			//		assignedColumns.Add(qc);
			//}

			AddUnassignedMobiusQueryColumnsToMap(qtSet, qcSet);

			return;
		}


		/// <summary>
		/// Add unassigned QueryColumns from the query to the map
		/// </summary>
		/// <param name="qcSet"></param>

		public void AddUnassignedMobiusQueryColumnsToMap(
				HashSet<QueryTable> qtSet,
				HashSet<QueryColumn> qcSet)
		{
			foreach (QueryTable qt in Query.Tables) // consider each table
			{

				if (QueryTable != null && qt != QueryTable) continue; // considering only one QueryTable if defined

				for (int qci = 0; qci < qt.QueryColumns.Count; qci++)
				{
					QueryColumn qc = qt.QueryColumns[qci];
					MetaColumn mc = qc.MetaColumn;

					if (!qc.Selected) continue; // must be selected

					if (qc.IsKey) continue; // don't add other keys

					if (qcSet.Contains(qc)) continue; // don't add if already in assigned set

					ColumnMapMsx cm = new ColumnMapMsx();
					cm.QueryColumn = qc;

					DataColumnMsx dc = CreateDataColumnMsxFromQueryColumn(qc);
					cm.SetSpotfireFieldsFromDataColumn(dc);
					cm.NewSpotfireColumnName = // Final name column will get
						qc.ActiveLabel + ColumnMapParms.SpotfireExportExtraColNameSuffix; 

					ColumnMapList.Add(cm);

					qcSet.Add(qc); // now assigned
				}
			}
		}

		/// <summary>
		/// Create a template DataColumn that is expected to be created when a Spotfire import is done for a column that is not mapped to an existing DataColumn
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>
		DataColumnMsx CreateDataColumnMsxFromQueryColumn(QueryColumn qc)
		{
			DataColumnMsx dc = new DataColumnMsx();
			dc.Name =  // set name to label with appended underscore to indicate that the name is initialized from a query column
			 dc.MobiusFileColumnName = // and not from a Spotfire template column name
			 dc.ExternalName =  // expected external name when this "unassigned" query col is first loaded into Spotfire via AddRows 
				qc.MetaTableDotMetaColumnName + "_";

			SetMatchingSpotfireDataTypeFromMobiusColumn(qc.MetaColumn, dc);

			return dc;
		}

		/// <summary>
		/// Create a Spotfire column name from the root name that is not already in the map
		/// </summary>
		/// <param name="rootName"></param>
		/// <returns></returns>

		public string AssignUniqueSpotfireColumnName(
			string rootName)
		{
			int i1, i2;
			for (i1 = 1; ; i1++)
			{
				string name = rootName;
				if (i1 > 1) name += " " + i1;
				for (i2 = 0; i2 < ColumnMapList.Count; i2++)
				{
					ColumnMapMsx cm = ColumnMapList[i2];
					if (Lex.Eq(cm.SpotfireColumnName, name)) break;
				}

				if (i2 >= ColumnMapList.Count) return name;
			}
		}

		/// <summary>
		/// Assure that the Spotfire column names derived from Mobius names are unique within a table
		/// </summary>

		public void AssignUniqueSpotfireNamesFromAssociatedMobiusColumnNames()
		{
			QueryTable qt;
			int qti;

			if (Query == null || Query.Tables.Count == 0) return;

			//DataMap.InitializeMapFromSpotfireDataTable();

			Dictionary<string, int> colNameDupCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // number of instances of each name
			Dictionary<string, QueryTable> colNameQt = new Dictionary<string, QueryTable>(StringComparer.OrdinalIgnoreCase); // first query table for name or null if same name in multiple QTs

			QueryTable qt0 = Query.Tables[0]; // root or only table

			foreach (ColumnMapMsx cm in ColumnMapList) // count dups for each name
			{
				QueryColumn qc = cm.QueryColumn;
				if (qc == null) continue;

				if (qc.IsKey) continue;

				qt = qc.QueryTable;
				string name = qc.ActiveLabel;
				if (Lex.IsUndefined(cm.Role)) name += "_"; // append underscore for cols without predefined role

				if (!colNameDupCount.ContainsKey(name))
				{
					colNameDupCount[name] = 0;
					colNameQt[name] = qt;
				}

				colNameDupCount[name]++;
				if (colNameQt[name] != qt)
					colNameQt[name] = null; // indicate same name in more than one table
			}

			HashSet<string> namesAssigned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (ColumnMapMsx cm in ColumnMapList) // first, reserve names with no associated querycolumn
			{
				QueryColumn qc = cm.QueryColumn;
				if (qc == null)
				{
					namesAssigned.Add(cm.SpotfireColumnName);
					continue;
				}
			}

			foreach (ColumnMapMsx cm in ColumnMapList) // assign unique names where duplicates exist
			{
				string newName;

				QueryColumn qc = cm.QueryColumn;
				if (qc == null) continue;

				if (qc.IsKey) continue;

				newName = qc.ActiveLabel;
				if (Lex.IsUndefined(cm.Role)) newName += "_"; // append underscore for cols without predefined role

				if (colNameQt[newName] == null) // if name duplicated across query tables, qualify by QueryTable
				{
					qt = qc.QueryTable;
					qti = Query.Tables.IndexOf(qt);
					newName = "T" + (qti + 1).ToString() + "." + qc.ActiveLabel;
					if (Lex.IsUndefined(cm.Role)) newName += "_"; // append underscore for cols without predefined role
				}

				if (namesAssigned.Contains(newName)) // add numeric suffix to newName to get unique name if newName already is assigned
				{
					for (int suffix = 2; ; suffix++)
					{
						if (!namesAssigned.Contains(newName + suffix))
						{
							newName = newName + suffix;
							break;
						}
					}
				}

				cm.NewSpotfireColumnName = newName; // put in new name col, gets copied to normal name after import/remap

				namesAssigned.Add(newName);
			}

			return;
		}


		/// <summary>
		/// GetSpotfireNameFromQueryColumn
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		public string GetSpotfireNameFromQueryColumn(
			QueryColumn qc)
		{
			ColumnMapMsx map = GetColumnMapFromQueryColumn(qc);
			if (map == null) return null;

			return map.SpotfireColumnName;
		}

		/// <summary>
		/// Lookup column map from QueryColumn
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		public ColumnMapMsx GetColumnMapFromQueryColumn(QueryColumn qc)
		{
			ColumnMapMsx mcMap = null;

			if (qc == null) return null;

			QueryTable qt = qc.QueryTable;

			for (int cmi = 0; cmi < ColumnMapCollection.Count; cmi++)
			{
				ColumnMapMsx map = ColumnMapCollection[cmi];
				QueryColumn qc2 = map.QueryColumn;
				if (qc2 == null) continue;
				if (qc2 == qc) return map;

				QueryTable qt2 = qc2.QueryTable;

				if (Lex.Eq(qc2.MetaColumnName, qc.MetaColumnName))
				{
					if (Lex.IsDefined(qt.Alias) && qt.Alias == qt2.Alias) return map; // same Qt alias && mc name
					if (Lex.Eq(qt2.MetaTableName, qt.MetaTableName))
						mcMap = map; // save any matching metatable name in case other matches fail
				}
			}

			if (mcMap != null) return mcMap; // return match on metatable name
			else return null; // not found
		}

		public ColumnMapMsx GetColumnMapFromMetaColumn(MetaColumn mc)
		{
			if (mc == null) return null;

			for (int cmi = 0; cmi < ColumnMapCollection.Count; cmi++)
			{
				ColumnMapMsx map = ColumnMapCollection[cmi];
				QueryColumn qc2 = map.QueryColumn;
				if (qc2 == null || qc2.MetaColumn == null) continue;
				MetaColumn mc2 = qc2.MetaColumn;

				if (Lex.Eq(mc.MetaTable.Name, mc.MetaTable.Name) && Lex.Eq(mc.Name, mc.Name))
					return map;
			}

			return null; // not found
		}

		/// <summary>
		/// BuildFilteredColumnMapList
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="excludedDataTypes"></param>
		/// <param name="defaultMc"></param>
		/// <returns></returns>

		public ColumnMapCollection BuildFilteredColumnMapList(
			SelectColumnOptions flags,
			ColumnMapMsx selectedCm)
		{
			ColumnMapCollection cml = new ColumnMapCollection();

			foreach (ColumnMapMsx cm in ColumnMapList)
			{
				MetaColumn mc = cm?.QueryColumn?.MetaColumn;
				if (mc == null) continue;
				if (flags.ExcludeKeys && mc.IsKey) continue;

				if (flags.ExcludeImages && mc.DataType == MetaColumnType.Image) continue;

				if (flags.ExcludeGraphicTypes && (
					mc.DataType == MetaColumnType.Structure ||
					mc.DataType == MetaColumnType.Image)) continue;

				if (flags.ExcludeNonNumericTypes && !MetaColumn.IsNumericMetaColumnType(mc.DataType))
					continue;

				if (flags.ExcludeNumericTypes && MetaColumn.IsNumericMetaColumnType(mc.DataType))
					continue;

				if (flags.AllowedDataTypes != null && flags.AllowedDataTypes.Count > 0 && !flags.AllowedDataTypes.Contains(mc.DataType))
					continue;

				cml.ColumnMapList.Add(cm);
			}

			return cml;
		}


		/// <summary>
		/// UpdateColumnMapList
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		public ColumnMapMsx UpdateColumnMapList(QueryColumn qc)
		{
			foreach (ColumnMapMsx m0 in ColumnMapList)
			{
				if (m0.QueryColumn == qc) return m0; // already have it just return
			}

			ColumnMapMsx m = new ColumnMapMsx();
			m.QueryColumn = qc;
			QueryTable qt = qc.QueryTable;
			int ti = qt.TableIndex;

			string colName = qc.MetaColumn.Label; // just use label for now, internal name later

			if (Query != null && Query.Tables.Count > 2 && ti > 0)
				colName = "T" + (ti + 1) + "." + colName;
			m.SpotfireColumnName = colName;

			ColumnMapCollection.Add(m);
			return m;
		}

		/// <summary>
		/// Serialize
		/// </summary>
		/// <param name="tw"></param>

		public void Serialize(XmlTextWriter tw)
		{
			tw.WriteStartElement("DataTableMap");

			XmlUtil.WriteAttributeIfDefined(tw, "SpotTableName", SpotfireTableName);

			XmlUtil.WriteAttributeIfDefined(tw, "SpotTableId", SpotfireTableId);

			XmlUtil.WriteAttributeIfDefined(tw, "QtRef", QueryTable?.GetQueryTableRefString());

			foreach (ColumnMapMsx m in ColumnMapList)
			{
				if (m.QueryColumn == null) continue; // don't serialize it not mapped to a query column

				m.Serialize(tw);
			}

			tw.WriteEndElement(); // DataTableMap
			return;
		}

		/// <summary>
		/// Deserialize a DataTableMap
		/// </summary>
		/// <param name="tr"></param>
		/// <returns></returns>

		internal static DataTableMapMsx Deserialize(
			Query q,
			XmlTextReader tr,
			DataTableMapsMsx maps)
		{
			DataTableMapMsx map = new DataTableMapMsx(maps); // create new map and link in

			XmlUtil.GetStringAttribute(tr, "SpotTableName", ref map._spotfireTableName);
			XmlUtil.GetStringAttribute(tr, "SpotTableId", ref map._spotfireTableId);

			string qtRef = XmlUtil.GetAttribute(tr, "QtRef");
			if (Lex.IsDefined(qtRef))
				map.QueryTable = QueryTable.GetQueryTableFromRefString(q, qtRef); // get the query table referenced, could be null if table no longer included in query

			while (true)
			{
				if (!XmlUtil.MoreSubElements(tr, "DataTableMap")) break;

				else if (Lex.Eq(tr.Name, "ColumnMap"))
					ColumnMapMsx.Deserialize(q, tr, map);

				else throw new Exception("Unexpected element: " + tr.Name);
			}

			return map;
		}

		/// <summary>
		/// Just return the Spotfire table name for use in UI controls
		/// </summary>
		/// <returns></returns>

		public override string ToString()
		{
			string txt = String.Format("DataTableMap - SpotTableName: {0}, QtRef: {1}\r\n", SpotfireTableName, QueryTable?.GetQueryTableRefString()) + "\r\n" +
				"Idx, SpotfireName, NewName, Role, ExternalName, MxFileColName";

			for (int cmi = 0; cmi < ColumnMapList.Count; cmi++)
			{
				ColumnMapMsx cm = ColumnMapList[cmi];

				txt += String.Format("{0}, {1}, {2}, {3}, {4}, {5}\r\n",
					cmi, cm.SpotfireColumnName, cm.NewSpotfireColumnName, cm.Role, cm.SpotfireExternalName, cm.MobiusFileColumnName);
			}

			return txt;
		}

	} // SpotfireDataTableMap

	/// <summary>
	/// ColumnMapList
	/// </summary>

	public class ColumnMapCollection : IEnumerable<ColumnMapMsx>, IEnumerable
	{

		/// <summary>
		/// Indexer
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>

		public ColumnMapMsx this[int index]    // Indexer declaration  
		{
			get { return ColumnMapList[index]; }// get accessors  
		}

		public List<ColumnMapMsx> ColumnMapList = new List<ColumnMapMsx>(); // mapping from query columns to external columns

		public void Add(ColumnMapMsx item) => ColumnMapList.Add(item);

		public int Count { get => ColumnMapList.Count; }

		/// <summary>
		/// Build a ColumnMapList from the current list for the columns in
		/// the supplied DataColumnMsx list and mark each as selected
		/// </summary>
		/// <param name="dataCols"></param>
		/// <returns></returns>

		public ColumnMapCollection BuildSelectedColumnMapListFromDataColumnList(
		List<DataColumnMsx> dataCols)
		{
			ColumnMapCollection cmList = Clone(); // make a copy of this list
			foreach (ColumnMapMsx cm in cmList.ColumnMapList)
				cm.Selected = false;

			foreach (DataColumnMsx dc in dataCols)
			{
				ColumnMapMsx map = cmList.GetColumnMapForSpotfireColumn(dc);
				if (map != null)
					map.Selected = true;
			}

			return cmList;
		}

		/// <summary>
		/// Get a list of the selected columns from the ColumnMapList
		/// </summary>
		/// <returns></returns>
		public ColumnMapCollection GetSelectedColumnMapList()
		{
			QueryColumn qc = null;

			ColumnMapCollection cmList = new ColumnMapCollection();
			foreach (ColumnMapMsx cm in ColumnMapList)
			{
				if (cm.Selected) cmList.ColumnMapList.Add(cm);
			}

			return cmList;
		}

		/// <summary>
		/// <summary>
		/// Lookup map from Spotfire column name
		/// </summary>
		/// <param name="spotfireName"></param>
		/// <returns></returns>

		public ColumnMapMsx GetColumnMapForSpotfireColumn(
			DataColumnMsx spotfireCol)
		{
			DataColumnMsx sc = spotfireCol;
			if (sc == null) throw new ArgumentException("DataColumnMsx is null");

			string spotfireColName = sc.Name;
			string spotfireTableName = sc.DataTable?.Name;

			for (int cmi = 0; cmi < ColumnMapList.Count; cmi++)
			{
				ColumnMapMsx map = ColumnMapList[cmi];

				if (map.SpotfireColumn == null) continue;

				if (sc == map.SpotfireColumn) return map;

				if (Lex.Eq(map.SpotfireColumnName, spotfireColName) &&
					Lex.Eq(map.SpotfireColumnName, spotfireColName))
					return map;
			}

			return null; // not found
		}

		/// <summary>
		/// GetQueryColumnForSpotfireColumn
		/// </summary>
		/// <param name="spotfireCol"></param>
		/// <returns></returns>

		public QueryColumn GetQueryColumnForSpotfireColumn(
			DataColumnMsx spotfireCol)
		{
			ColumnMapMsx m = GetColumnMapForSpotfireColumn(spotfireCol);
			if (m != null) return m.QueryColumn;
			else return null;
		}

		/// <summary>
		/// Get QueryColumn from spotfire name
		/// </summary>
		/// <param name="spotfireName"></param>
		/// <returns></returns>

		public QueryColumn GetQueryColumnForSpotfireColumnName(
		string spotfireName)
		{
			ColumnMapMsx m = GetColumnMapForSpotfireColumnName(spotfireName);
			if (m != null) return m.QueryColumn;
			else return null;
		}

		/// <summary>
		/// Lookup map from Spotfire column name
		/// </summary>
		/// <param name="spotfireName"></param>
		/// <returns></returns>

		public ColumnMapMsx GetColumnMapForSpotfireColumnName(
			string spotfireName)
		{
			if (String.IsNullOrWhiteSpace(spotfireName)) return null;

			for (int cmi = 0; cmi < ColumnMapList.Count; cmi++)
			{
				ColumnMapMsx map = ColumnMapList[cmi];

				if (Lex.Eq(map.SpotfireColumnName, spotfireName))
					return map;
			}

			return null; // not found
		}

		/// <summary>
		/// Activate any pending new Spotfire names in the map
		/// </summary>
		/// <returns></returns>

		public int UpdateColumnMapsToUseNewSpotfireColumnNames()
		{
			int activatedCount = 0;

			foreach (ColumnMapMsx cm in ColumnMapList)
			{
				if (Lex.IsDefined(cm.NewSpotfireColumnName) && cm.NewSpotfireColumnName != cm.SpotfireColumnName)
				{
					cm.SpotfireColumnName = cm.NewSpotfireColumnName;
					cm.NewSpotfireColumnName = "";
					activatedCount++;
				}
			}

			return activatedCount;
		}

		/// <summary>
		/// Returns a SpotfireDataTableMap-typed IEnumerator to interate through MapList
		/// </summary>
		/// <returns></returns>

		IEnumerator<ColumnMapMsx> IEnumerable<ColumnMapMsx>.GetEnumerator()
		{
			return ColumnMapList.GetEnumerator();
		}

		/// <summary>
		/// Returns an IEnumerator to interate through MapList
		/// </summary>
		/// <returns></returns>

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ColumnMapList.GetEnumerator();
		}


		/// <summary>
		/// CloneColumnMapList
		/// </summary>
		/// <returns></returns>

		public ColumnMapCollection Clone()
		{
			ColumnMapCollection cmList = new ColumnMapCollection();
			foreach (ColumnMapMsx cm in ColumnMapList)
			{
				ColumnMapMsx cm2 = cm.Clone();
				cmList.ColumnMapList.Add(cm2);
			}

			return cmList;
		}
	}

	/// <summary>
	/// Maps a single column from a Mobius QueryColumn to a Spotfire Column,
	/// This parallels and duplicates many of the members in DataColumnMsx.
	/// </summary>

	public class ColumnMapMsx
	{

// The QueryColumn coming from the associated query

		public QueryColumn QueryColumn = null; // QueryColumn 
		public QnfEnum SubColumn = QnfEnum.Undefined; // SubColumn, e.g. Qualifier, Number, etc

// The associated Spotfire DataColumnMsx

		public DataColumnMsx SpotfireColumn = null; // associated Spotfire column
		public string SpotfireColumnName { get => GetSpotfireColumnName(); set => SetSpotfireColumnName(value); } // Spotfire column name if SpotfireColumn is/is-not defined
		public string Role = ""; // Role, assigned from the name of the column in the template Analysis file
		public string MobiusFileColumnName = ""; // name of column in source file. Either MetaTable.MetaColumn name or "None 1", "None 2" ...
		public DataTypeMsxEnum SpotfireDataType = DataTypeMsxEnum.Undefined;
		public string SpotfireContentType = ""; // e.g chemical/x-mdl-molfile, chemical/x-mdl-chime, chemical/x-daylight-smiles
		public string SpotfireExternalName = ""; // from SpotFire column property. Should match SourceFileColName after ReplaceData operation

// NewSpotfireColumnName contains the new column name if the column is to be renamed

		public string NewSpotfireColumnName = ""; 

		// The following members are for use with parameterized Spotfire views only

		public string ParameterName = ""; // 
		public SpotfireParmDataType SpotfireParmDataType = SpotfireParmDataType.Undefined;
		public MetaColumnType AllowedMetaColumnType = MetaColumnType.Unknown;
		public bool ValueIsRequired = false; // if true then a value must be supplied for the parameter
		public bool Selected = false;

		public string GetSpotfireColumnName()
		{
			if (SpotfireColumn != null) return SpotfireColumn.Name;
			else return _spotfireColumnName;
		}

		public void SetSpotfireColumnName(string name)
		{
			_spotfireColumnName = name;
			if (SpotfireColumn != null) SpotfireColumn.Name = name;
		}
		private string _spotfireColumnName = null; // backup Id in case SpotfireDataColumn is null

		/// <summary>
		/// Default constructor
		/// </summary>

		public ColumnMapMsx()
		{
			return;
		}

/// <summary>
/// Try to extract a QueryColumn/MetaColumn reference from a DataColumnMsx
/// </summary>
/// <param name="dc"></param>
/// <returns></returns>
	public static string GetQcRefFromDataColumn(
		DataColumnMsx dc)
	{
		string qcRef = dc.MobiusFileColumnName; // import source file column name
		//if (Lex.IsUndefined(qcRef)) qcRef = dc.ExternalName; // note that this may be the original source name rather than the latest
		//if (Lex.IsUndefined(qcRef)) qcRef = dc.Name;
		if (Lex.EndsWith(qcRef, ColumnMapParms.SpotfireExportExtraColNameSuffix))
		{
			string s = ColumnMapParms.SpotfireExportExtraColNameSuffix;
			qcRef = qcRef.Substring(0, qcRef.Length - s.Length);
		}

		return qcRef;
	}

/// <summary>
/// Construct a ColumnMap  from a QueryColumn
/// </summary>
/// <param name="qc"></param>

public static ColumnMapMsx BuildFromQueryColumn(QueryColumn qc)
		{
			ColumnMapMsx cm = new ColumnMapMsx();
			cm.QueryColumn = qc;
			return cm;
		}

		/// <summary>
		/// Construct a ColumnMap from a Spotfire DataColumnMsx
		/// </summary>
		/// <param name="dc"></param>

		public static ColumnMapMsx BuildFromSpotfireDataColumn(DataColumnMsx dc)
		{
			if (dc == null) return null;

			ColumnMapMsx cm = new ColumnMapMsx();
			cm.SetSpotfireFieldsFromDataColumn(dc);
			return cm;
		}

		/// <summary>
		/// Copy the values 
		/// </summary>
		/// <param name="dc"></param>

		public void SetSpotfireFieldsFromDataColumn(DataColumnMsx dc)
		{
			SpotfireColumn = dc; 

			Role = dc.MobiusRole;
			SpotfireExternalName = dc.ExternalName;
			SpotfireDataType = dc.DataType;
			SpotfireContentType = dc.ContentType;
			MobiusFileColumnName = dc.MobiusFileColumnName;

			return;
		}

		/// <summary>
		/// Serialize a ColumnMapEx
		/// </summary>
		/// <param name="elementName"></param>
		/// <param name="tw"></param>

		public void Serialize(XmlTextWriter tw)
		{
			tw.WriteStartElement("ColumnMap");

			if (SpotfireColumn != null) // get latest if defined
				SpotfireColumnName = SpotfireColumn.Name;

			XmlUtil.WriteAttributeIfDefined(tw, "SpotColName", SpotfireColumnName);

			XmlUtil.WriteAttributeIfDefined(tw, "OrigSpotColName", Role);

			XmlUtil.WriteAttributeIfDefined(tw, "MobiusFileColumnName", MobiusFileColumnName);

			XmlUtil.WriteAttributeIfDefined(tw, "SpotDataType", SpotfireDataType.ToString());

			XmlUtil.WriteAttributeIfDefined(tw, "SpotContentType", SpotfireContentType);

			XmlUtil.WriteAttributeIfDefined(tw, "QcRef", QueryColumn?.QcRefString);

			tw.WriteEndElement();
		}

		/// <summary>
		/// Deserialize a ColumnMapMsx
		/// </summary>
		/// <param name="tr"></param>
		/// <returns></returns>

		internal static ColumnMapMsx Deserialize(
			Query q,
			XmlTextReader tr,
			DataTableMapMsx map)
		{
			Enum iEnum = null;

			XmlUtil.VerifyElementName(tr, "ColumnMap");

			ColumnMapMsx cm = new ColumnMapMsx();
			map.Add(cm);

			XmlUtil.GetStringAttribute(tr, "SpotColName", ref cm._spotfireColumnName);

			XmlUtil.GetStringAttribute(tr, "OrigSpotColName", ref cm.Role);

			XmlUtil.GetStringAttribute(tr, "MobiusFileColumnName", ref cm.MobiusFileColumnName);

			if (XmlUtil.GetEnumAttribute(tr, "SpotDataType", typeof(DataTypeMsxEnum), ref iEnum))
				cm.SpotfireDataType = (DataTypeMsxEnum)iEnum;

			XmlUtil.GetStringAttribute(tr, "SpotContentType", ref cm.SpotfireContentType);

			string qcRef = XmlUtil.GetAttribute(tr, "QcRef");
			if (Lex.IsDefined(qcRef))
				cm.QueryColumn = QueryColumn.GetQueryColumnFromRefString(q, qcRef); // get the query column referenced, could be null if no longer selected in query

			XmlUtil.VerifyAtEndOfElement(tr, "ColumnMap");

			return cm;
		}

		/// <summary>
		/// Clone
		/// </summary>
		/// <returns></returns>

		public ColumnMapMsx Clone()
		{
			ColumnMapMsx cmCopy = (ColumnMapMsx)this.MemberwiseClone();
			return cmCopy;
		}

		public override string ToString()
		{
			string txt = String.Format("ColumnMap - SpotfireName: {0}, NewName: {1}, Role: {2}, ExternalName: {3}, MxFileColName: {4}",
				SpotfireColumnName, NewSpotfireColumnName, Role, SpotfireExternalName, MobiusFileColumnName);

			return txt;
		}

	} // ColumnMap

	/// <summary>
	/// Flags for column selection 
	/// </summary>

	public class SelectColumnOptions
	{
		public bool SearchableOnly = false; // searchable columns only
		public bool NongraphicOnly = false; // nongraphic columns only
		public bool FirstTableKeyOnly = false; // if key is selected return key for 1st table
		public bool ExcludeKeys = false; // keys not allowed
		public bool ExcludeGraphicTypes = false;
		public bool ExcludeNonNumericTypes = false;
		public bool ExcludeNumericTypes = false;
		public bool ExcludeImages = false; // images not allowed
		public bool IncludeAllSelectableCols = false; // include all selectable cols not just the selected cols
		public bool SelectFromQueryOnly = false; // select from query only, not database
		public bool IncludeNoneItem = false; // allow "none" column to be selected

		public QnfEnum QnSubcolsToInclude = QnfEnum.Combined; // subcolumn treatment 

		public string ExtraItem = ""; // extra item to include at bottom of menu

		public HashSet<MetaColumnType> AllowedDataTypes = new HashSet<MetaColumnType>();
	}

}
