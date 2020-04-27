using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.Data;

using System;
using System.Data;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public class ToolHelper 
	{

		public static string ToolsNameSpaceName = "Mobius.Tools";

/// <summary>
/// Init query for use in an Embedded Data Tool
/// </summary>
/// <param name="mt"></param>
/// <returns></returns>
		public static Query InitEmbeddedDataToolQuery(
			MetaTable mt)
		{
			Query q = new Query(); // build single-table query
			q.SerializeResultsWithQuery = true;
			q.BrowseSavedResultsUponOpen = true;
			QueryTable qt = new QueryTable(q, mt);
			DataTableMx dt = DataTableManager.BuildDataTable(q);
			q.ResultsDataTable = dt;
			return q;
		}

		/// <summary>
		/// Return true if the query contains a metatable that is handled by a MobiusToolMetaBroker (Deprecated)
		/// </summary>
		/// <param name="q"></param> 
		/// <returns></returns>

		public static bool IsOldToolQuery(
			Query q)
		{
			QueryTable qt = GetFirstToolQueryMetaTable(q);
			if (qt == null) return false;

			if (Lex.Eq(qt.MetaTable.TableMap, "Mobius.Tools.MultStructSearch")) // check specific cases of old tools
				 return true;

			else return false;
		}

		/// <summary>
		/// Run the current tool query
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		public static string RunOldToolQuery(
			Query q)
		{
			string extensionId = GetExtensionId(q);
			string args = "RunToolQuery";
			string response = Plugins.CallExtensionPointRunMethod(extensionId, args);
			return response;
		}

		/// <summary>
		/// Get the tool extension id associated with a query
		/// Example: "PlugIn.Mobius.Tools.TargetNetworkViewer" 
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		public static string GetExtensionId(
			Query q)
		{
			QueryTable qt = GetFirstToolQueryMetaTable(q);
			if (qt == null) return "";
			else return qt.MetaTable.TableMap;
		}

		public static QueryTable GetFirstToolQueryMetaTable(
			Query q)
		{
			foreach (QueryTable qt in q.Tables)
			{
				MetaTable mt = qt.MetaTable;
				if (mt.MetaBrokerType == MetaBrokerType.NoSql && mt.TableMap.Contains(ToolsNameSpaceName))
					return qt;
			}

			return null;
		}

		/// <summary>
		/// InvokeToolBasedViewSetupDialog
		/// </summary>
		/// <param name="toolQuery">Query for tool that includes a ToolUtil.ToolParametersColumnMapValue column containing parameters</param>
		/// <param name="baseQuery"></param>
		/// <returns></returns>

		public static DialogResult InvokeToolBasedInitialViewSetupDialog(
				Query toolQuery,
				Query baseQuery)
		{
			QueryColumn qc = null;
			foreach (QueryTable qt in toolQuery.Tables)
			{
				qc = qt.GetQueryColumnByName(ToolUtil.ToolParametersColumnMapValue);
				if (qc != null) break;
			}

			if (qc == null) return DialogResult.Cancel;

			string extensionId = qc.MetaTable.TableMap; // e.g. "Mobius.Tools.SarLandscape"
			string methodRef = extensionId + ".ShowInitialViewSetupDialog";
			List<object> args = new List<object>() { qc, baseQuery }; // single QueryColumn criteria
			DialogResult dr = Plugins.CallDialogResultExtensionPointMethod(methodRef, args); // i.e. public string EditCriteria(QueryColumn qc)
			return dr;
		}

		/// <summary>
		/// Invoke the dialog for the associated tool to edit criteria
		/// </summary>
		/// <param name="qc"></param>

		public static DialogResult InvokeToolCriteriaEditor(
			QueryColumn qc)
		{
			string extensionId = qc.MetaTable.TableMap; // e.g. "Mobius.Tools.SarLandscape"
			string methodRef = extensionId + ".EditCriteria";
			List<object> args = new List<object>() { qc }; // single QueryColumn criteria
			DialogResult dr = Plugins.CallDialogResultExtensionPointMethod(methodRef, args); // i.e. public string EditCriteria(QueryColumn qc)
			return dr;
		}

/// <summary>
/// Execute specified query
/// </summary>
/// <param name="q"></param>
/// <returns></returns>

		public static DialogResult ExecuteQuery(ref Query q, out List<string> keys)
		{
			QueryManager qm = new QueryManager();
			DialogResult dr = qm.ExecuteQuery(ref q, out keys);
			return dr;
		}

/// <summary>
/// Retrieve next row for query
/// </summary>
/// <param name="q"></param>
/// <returns></returns>

		public static DataRowMx NextRow(Query q)
		{
			QueryManager qm = q.QueryManager as QueryManager;
			if (qm == null) return null;
			return qm.NextRow();
		}

		/// <summary>
		/// Display the data for the specified Query and matching DataTable
		/// </summary>
		/// <param name="q"></param>
		/// <param name="dt"></param>
		/// <param name="embedDataInQuery"></param>
		
		public static void DisplayData(
			Query q,
			DataTableMx dt,
			bool embedDataInQuery)
		{
			DisplayData(q, dt, null, embedDataInQuery);
			return;
		}


/// <summary>
/// Create a QueryManager and associated objects from the specified Query and DataTable
/// </summary>
/// <param name="q"></param>
/// <param name="dt"></param>
/// <returns></returns>

		public static QueryManager SetupQueryManager(
			Query q,
			DataTableMx dt)
		{
			QueryManager qm = new QueryManager();
			qm.LinkMember(q);
			qm.LinkMember(dt);

			DataTableManager dtm = new DataTableManager(qm);
			ResultsFormatFactory rff = new ResultsFormatFactory(qm, OutputDest.WinForms);
			rff.Build(); // builds format

			ResultsFormatter fmtr = new ResultsFormatter(qm);

			qm.DataTableManager.InitializeRowAttributes();
			qm.DataTableManager.SetRowRetrievalStateComplete();

			return qm;
		}

		/// <summary>
		/// Get the curren tool query and it's parameter string
		/// </summary>
		/// <returns></returns>

		public static void GetCurrentQueryAndParameterString(
			out Query q,
			out string parms)
		{
			parms = "";

			q = QueriesControl.Instance.CurrentQuery;
			if (q == null) return;

			foreach (QueryTable qt in q.Tables)
			{
				foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (Lex.Eq(qc.MetaColumn.ColumnMap, ToolUtil.ToolParametersColumnMapValue))
					{
						parms = qc.Criteria;
						return;
					}
				}
			}

			return;
		}

		/// <summary>
		/// Set the subquery for the root tool query
		/// </summary>
		/// <param name="rootQuery"></param>
		/// <param name="subQuery"></param>

		public static void SetSubquery(
			Query rootQuery,
			Query subQuery)
		{
			rootQuery.Subqueries.Clear();
			rootQuery.Subqueries.Add(subQuery);
			//subQuery.Parent = rootQuery;
		}

/// <summary>
/// Display the tool data
/// </summary>
/// <param name="qm"></param>

		public static void DisplayData(
			QueryManager qm)
		{
			DisplayData(qm, gridPanel:null, embedDataInQuery:true);
			return;
		}

		/// <summary>
		/// DisplayStructureInPopupGrid
		/// </summary>
		/// <param name="title"></param>
		/// <param name="cidLabel"></param>
		/// <param name="structLabel"></param>
		/// <param name="structure"></param>

		public static void DisplayStructureInPopupGrid(
			string title,
			string cidLabel,
			string structLabel,
			MoleculeMx structure)
		{
			List<MoleculeMx> structures = new List<MoleculeMx>();
			structures.Add(structure);

			PopupGrid pg = new PopupGrid();
			bool embedDataInQuery = false;

			DisplayStructures(title, cidLabel, structLabel, structures, pg.MoleculeGridPanel, embedDataInQuery);
			return;
		}

		/// <summary>
		/// Do generic display of a structure
		/// </summary>
		/// <param name="title"></param>
		/// <param name="cidLabel"></param>
		/// <param name="structLabel"></param>
		/// <param name="structure"></param>

		public static void DisplayStructure(
			string title,
			string cidLabel,
			string structLabel,
			MoleculeMx structure,
			MoleculeGridPanel gridPanel,
			bool embedDataInQuery)
		{
			List<MoleculeMx> structures = new List<MoleculeMx>();
			structures.Add(structure);

			DisplayStructures(title, cidLabel, structLabel, structures, gridPanel, embedDataInQuery);
			return;
		}

		/// <summary>
		/// Do Generic display of a list of structures
		/// </summary>
		/// <param name="title"></param>
		/// <param name="cidLabel"></param>
		/// <param name="structLabel"></param>
		/// <param name="structures"></param>

		public static void DisplayStructures(
			string title,
			string cidLabel,
			string structLabel,
			List<MoleculeMx> structures,
			MoleculeGridPanel gridPanel,
			bool embedDataInQuery)
		{
			QueryManager qm = null;

			MetaTable mt = MetaTableCollection.GetWithException("Generic_Structure_Table");
			mt.MetaBrokerType = MetaBrokerType.NoSql;

			Query q = ToolHelper.InitEmbeddedDataToolQuery(mt);
			QueryTable qt = q.Tables[0];
			qt.Label = title;
			qt.GetQueryColumnByName("CompoundId").Label = cidLabel;
			qt.GetQueryColumnByName("Structure").Label = structLabel;

			DataTableMx dt = q.ResultsDataTable as DataTableMx;

			for (int mi = 0; mi < structures.Count; mi++)
			{
				MoleculeMx cs = structures[mi];
				DataRowMx dr = dt.NewRow();
				string cid = cs.Id;
				if (Lex.IsUndefined(cid)) cid = (mi + 1).ToString();
				dr[qt.Alias + ".CompoundId"] = new CompoundId(cid);
				dr[qt.Alias + ".Structure"] = cs;
				dt.Rows.Add(dr);
			}

			DisplayData(q, dt, gridPanel, embedDataInQuery);

			return;
		}

/// <summary>
/// Display Data for Query
/// </summary>
/// <param name="q"></param>
/// <param name="gridPanel"></param>
/// <param name="embedDataInQuery"></param>
/// <param name="fitDataToGridWidth"></param>

		public static void DisplayData(
			Query q,
			MoleculeGridPanel gridPanel,
			bool embedDataInQuery = false,
			bool fitDataToGridWidth = false)
		{
			if (q.Tables.Count == 0) throw new Exception("No tables for query");
			QueryTable qt = q.Tables[0];

			DataTableMx dt = DataTableManager.BuildDataTable(q);
			q.ResultsDataTable = dt;
			q.UserObject.Name = qt.MetaTable.Label;
			QueryManager qm = SetupQueryManager(q, dt);
			DisplayData(qm, gridPanel, embedDataInQuery, fitDataToGridWidth);
			return;
		}

		/// <summary>
		/// Display the data for the specified Query and matching DataTable
		/// </summary>
		/// <param name="q"></param>
		/// <param name="dt"></param>
		/// <param name="persistWithinQuery"></param>

		public static void DisplayData(
			Query q,
			DataTableMx dt,
			MoleculeGridPanel gridPanel,
			bool embedDataInQuery = false,
			bool fitDataToGridWidth = false)
		{
			if (q.Tables.Count == 0) throw new Exception("No tables for query");
			QueryTable qt = q.Tables[0];
			q.UserObject.Name = qt.MetaTable.Label;
			QueryManager qm = SetupQueryManager(q, dt);
			DisplayData(qm, gridPanel, embedDataInQuery, fitDataToGridWidth);
			return;
		}

/// <summary>
/// Display data 
/// - Query and DataTable are already setup in QueryManger
/// - Complete the QM members and start display
/// </summary>
/// <param name="qm"></param>
/// <param name="gridPanel"></param>
/// <param name="embedDataInQuery"></param>
/// <param name="fitDataToGridWidth"></param>

		public static void DisplayData(
			QueryManager qm,
			MoleculeGridPanel gridPanel,
			bool embedDataInQuery = false,
			bool fitDataToGridWidth = false)
		{
			SetupGridPanelForDisplay(qm, gridPanel, embedDataInQuery, fitDataToGridWidth);

			DisplayData(gridPanel, qm);
			return;
		}

/// <summary>
/// Setup/format a MoleculeGridPanel/MoleculeGrid for display of results for the supplied QueryManager
/// </summary>
/// <param name="qm"></param>
/// <param name="gridPanel"></param>
/// <param name="embedDataInQuery"></param>
/// <param name="fitDataToGridWidth"></param>

		public static void SetupGridPanelForDisplay(
			QueryManager qm,
			MoleculeGridPanel gridPanel,
			bool embedDataInQuery = false,
			bool fitDataToGridWidth = false)
		{
			MoleculeGridControl grid = null;

			bool displayAsNormalQueryResults = (gridPanel == null); // if grid panel not defined assume display is in normal results panel
																															//bool displayAsPopupGrid = (gridPanel != null && gridPanel.Parent is PopupGrid); // display in popup grid


			Query q = qm.Query;
			q.SetupQueryPagesAndViews(); // be sure we have a default view page

			if (embedDataInQuery)
			{ // no database behind this table so persist within the query
				MetaTable mt = q.Tables[0].MetaTable; // assume just one metatable
				foreach (MetaColumn mc in mt.MetaColumns) // no criteria allowed
					mc.IsSearchable = false;

				q.SerializeMetaTablesWithQuery = true; // if no broker then save the metatable definition
				q.SerializeResultsWithQuery = true; // also save results when saving the query
				q.BrowseSavedResultsUponOpen = true; // open the results when the query is opened
				q.ResultsDataTable = qm.DataTable; // point the query to this results table
			}

			ResultsFormatFactory rff = new ResultsFormatFactory(qm, OutputDest.WinForms);
			rff.Build(); // build format
			ResultsFormatter fmtr = new ResultsFormatter(qm); // and formatter

			if (displayAsNormalQueryResults)
			{
				MoleculeGridPageControl mgpc = MoleculeGridPanel.AllocateNewMoleculeGridPageControl(qm, SessionManager.Instance.QueryResultsControl);
				gridPanel = mgpc.MoleculeGridPanel;
				//gridPanel = SessionManager.Instance.QueryResultsControl.MoleculeGridPageControl.MoleculeGridPanel; // (old)
				QbUtil.AddQuery(qm.Query); // add to the list of visible queries
				QueriesControl.Instance.CurrentBrowseQuery = qm.Query; // make it current
			}

			grid = gridPanel.SelectBaseGridViewGrid(qm);

			//if (qm.ResultsFormat.UseBandedGridView)
			//	grid = gridPanel.BandedViewGrid;

			//else grid = gridPanel.LayoutViewGrid;

			qm.MoleculeGrid = grid;
			grid.QueryManager = qm;

			DataTableMx dt = qm.DataTable; // save ref to data table
			grid.DataSource = null; // clear source for header build
			if (fitDataToGridWidth && grid.BGV != null) grid.BGV.OptionsView.ColumnAutoWidth = true; // set view for auto width to fit within view

			grid.FormatGridHeaders(qm.ResultsFormat);
			qm.DataTable = dt; // restore data table

			qm.DataTableManager.SetResultsKeysFromDatatable(); // set the results keys

			//if (displayAsNormalQueryResults)
			//	QbUtil.SetMode(QueryMode.Browse); // put into browse mode (need this?)

			//else if (displayAsPopupGrid) // obsolete
			//{
			//	PopupGrid pug = gridPanel.Parent as PopupGrid;
			//	pug.Initialize(qm); // be sure popup is initialized
			//	pug.Show();
			//}

			//gridPanel.Visible = true;
			//grid.Visible = true;
			//grid.DataSource = qm.DataTable; // set the datasource for the grid to the datatable

			//RefreshDataDisplay(qm);

			return;
		}

		/// <summary>
		/// Display the tool data
		/// </summary>
		/// <param name="qm"></param>
		/// <param name="serializeContentInQuery"></param>

		public static void DisplayDataOld( // Old version from 1/4/2017 
			QueryManager qm,
			MoleculeGridPanel gridPanel,
			bool serializeContentInQuery,
			bool fitDataToGridWidth = false)
		{
			MoleculeGridControl grid = null;

			bool displayAsNormalQueryResults = (gridPanel == null); // if grid panel not defined assume display is in normal results panel
			bool displayAsPopupGrid = (gridPanel != null && gridPanel.Parent is PopupGrid); // display in popup grid

			Query q = qm.Query;
			q.SetupQueryPagesAndViews(); // be sure we have a default view page

			if (serializeContentInQuery)
			{ // no database behind this table so persist within the query
				MetaTable mt = q.Tables[0].MetaTable; // assume just one metatable
				foreach (MetaColumn mc in mt.MetaColumns) // no criteria allowed
					mc.IsSearchable = false;

				q.SerializeMetaTablesWithQuery = true; // if no broker then save the metatable definition
				q.SerializeResultsWithQuery = true; // also save results when saving the query
				q.BrowseSavedResultsUponOpen = true; // open the results when the query is opened
				q.ResultsDataTable = qm.DataTable; // point the query to this results table
			}

			ResultsFormatFactory rff = new ResultsFormatFactory(qm, OutputDest.WinForms);
			rff.Build(); // build format
			ResultsFormatter fmtr = new ResultsFormatter(qm); // and formatter

			if (displayAsNormalQueryResults)
			{
				gridPanel = SessionManager.Instance.QueryResultsControl.MoleculeGridPageControl.MoleculeGridPanel;
				QbUtil.AddQuery(qm.Query); // add to the list of visible queries
				QueriesControl.Instance.CurrentBrowseQuery = qm.Query; // make it current
			}

			grid = gridPanel.SelectBaseGridViewGrid(qm);

			//if (qm.ResultsFormat.UseBandedGridView)
			//	grid = gridPanel.BandedViewGrid;

			//else grid = gridPanel.LayoutViewGrid;

			qm.MoleculeGrid = grid;
			grid.QueryManager = qm;

			DataTableMx dt = qm.DataTable; // save ref to data table
			grid.DataSource = null; // clear source for header build
			if (fitDataToGridWidth && grid.BGV != null) grid.BGV.OptionsView.ColumnAutoWidth = true; // set view for auto width to fit within view

			grid.FormatGridHeaders(qm.ResultsFormat);
			qm.DataTable = dt; // restore data table

			qm.DataTableManager.SetResultsKeysFromDatatable(); // set the results keys

			if (displayAsNormalQueryResults)
				QbUtil.SetMode(QueryMode.Browse); // put into browse mode

			else if (displayAsPopupGrid)
			{
				PopupGrid pug = gridPanel.Parent as PopupGrid;
				pug.Initialize(qm); // be sure popup is initialized
				pug.Show();
			}

			gridPanel.Visible = true;
			grid.Visible = true;
			grid.DataSource = qm.DataTable; // set the datasource for the grid to the datatable

			RefreshDataDisplay(qm);

			return;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="gridPanel"></param>
		/// <param name="qm"></param>

		public static void DisplayData(
			MoleculeGridPanel gridPanel,
			QueryManager qm)
		{
			if (gridPanel == null) // if grid panel not defined assume display is in normal results panel
			{
				gridPanel = SessionManager.Instance.QueryResultsControl.MoleculeGridPageControl.MoleculeGridPanel;
				QbUtil.SetMode(QueryMode.Browse); // put into browse mode
			}

			else if (gridPanel.Parent is PopupGrid) // display in popup grid
			{
				PopupGrid pug = gridPanel.Parent as PopupGrid;
				pug.Initialize(qm); // be sure popup is initialized
				WindowsHelper.FitFormOnScreen(pug);
				pug.Show();
			}

			MoleculeGridControl grid = qm.MoleculeGrid;

			gridPanel.Visible = true;
			grid.Visible = true;
			grid.DataSource = qm.DataTable; // set the datasource for the grid to the datatable

			RefreshDataDisplay(qm);
		}


		/// <summary>
		/// Refresh grid display with new data
		/// </summary>
		/// <param name="qm"></param>

		public static void RefreshDataDisplay(
			QueryManager qm)
		{
			if (qm.MoleculeGrid.DataSource == null) // be sure datasource is set
				qm.MoleculeGrid.DataSource = qm.DataTable;

			qm.MoleculeGrid.RefreshDataSource();

			//qm.MoleculeGrid.ForceRefilter();
			//qm.MoleculeGrid.Refresh();

			return;
		}

/// <summary>
/// Add a DataColumn to a DataTable that will be used to create an annotation table
/// </summary>
/// <param name="dataTable">DataTable to add column to</param>
/// <param name="columnName">Name for column</param>
/// <param name="caption">Caption/label for column</param>
/// <param name="dataType">Datatype for column</param>
/// <param name="displayLevel">Initial level of display for this column</param>
/// <param name="displayWidth">Width for column in characters</param>
/// <returns>Added DataColumn</returns>

		public static DataColumn AddDataColumn(
		DataTable dataTable,
		string columnName,
		string caption,
		Type dataType,
		ColumnSelectionEnum displayLevel,
		float displayWidth)
		{
			return AddDataColumn(dataTable, columnName, caption, dataType, displayLevel, displayWidth, ColumnFormatEnum.Default, 0);
		}

/// <summary>
/// Add a DataColumn to a DataTable that will be used to create an annotation table
/// </summary>
/// <param name="dataTable">DataTable to add column to</param>
/// <param name="columnName">Name for column</param>
/// <param name="caption">Caption/label for column</param>
/// <param name="dataType">Datatype for column</param>
/// <param name="displayLevel">Initial level of display for this column</param>
/// <param name="displayWidth">Width for column in characters</param>
/// <param name="displayFormat">Type of display for numeric columns</param>
/// <param name="decimals">Number of decimals/precision for numeric values</param>
/// <returns>Added DataColumn</returns>
 
		public static DataColumn AddDataColumn(
		DataTable dataTable,
		string columnName,
		string caption,
		Type dataType,
		ColumnSelectionEnum displayLevel,
		float displayWidth,
		ColumnFormatEnum displayFormat,
		int decimals)
		{
			DataColumn dc = new DataColumn(columnName, dataType);
			dc.Caption = caption;
			dc.ExtendedProperties.Add("DisplayLevel",displayLevel);
			dc.ExtendedProperties.Add("DisplayWidth",displayWidth);
			dc.ExtendedProperties.Add("DisplayFormat",displayFormat);
			dc.ExtendedProperties.Add("Decimals",decimals);
			dataTable.Columns.Add(dc);
			return dc;
		}

		/// <summary>
		/// Setup continuous conditional formatting for the specified QueryColumn
		/// </summary>
		/// <param name="qm"></param>
		/// <param name="qc"></param>

		public static void SetupContinuousCondFormat(
			QueryManager qm,
			QueryColumn qc)
		{
			ResultsField rfld = qm.ResultsFormat.GetResultsField(qc);
			qc.CondFormat = CondFormatRulesControl.InitializeColorScaleCondFormat(rfld);
			return;
		}

		/// <summary>
		/// Create an annotation table from a DataTable
		/// </summary>
		/// <param name="fullyQualifiedName">Fully qualified name to assign to table</param>
		/// <param name="dataTable">DataTable containing table definition & data</param>
		/// <param name="showProgress">Display dialog box showing progress of creation</param>
		/// <returns>Internal name assigned to annotation table (ANNOTATION_12345)</returns>

		public static MetaTable CreateAnnotationTable(
			string fullyQualifiedName,
			DataTable dataTable,
			bool showProgress)
		{
			List<AnnotationVo> voList = new List<AnnotationVo>();
			AnnotationVo avo;

			if (dataTable == null) DebugMx.ArgException("DataTable is null");
			if (dataTable.Columns.Count == 0) DebugMx.ArgException("No DataColumns are defined");
			if (dataTable.Columns[0].DataType != typeof(CompoundId))
				DebugMx.ArgException("The first column must be of type CompoundId");

			if (showProgress)
				Progress.Show("Creating annotation table...");

			AnnotationDao aDao = new AnnotationDao();
			UserObject auo = UserObjectUtil.ParseInternalUserObjectName(fullyQualifiedName);
			auo.Type = UserObjectType.Annotation;
			UserObjectTree.GetValidUserObjectTypeFolder(auo);
			UserObject auo2 = UserObjectDao.Read(auo); // see if there already
			MetaTable oldMt = null;
			if (auo2 == null) // get new identifier
				auo.Id = UserObjectDao.GetNextId(); // id to store table under

			else // reuse identifier
			{
				auo.Id = auo2.Id; // copy it over
				aDao.DeleteTable(auo.Id); // delete any existing data
				string oldMtName = "ANNOTATION_" + auo2.Id;
				oldMt = MetaTableCollection.Get(oldMtName);
			}

			MetaTable mt = new MetaTable();
			mt.MetaBrokerType = MetaBrokerType.Annotation;
			mt.Name = "ANNOTATION_" + auo.Id; // name table by uo
			mt.Label = auo.Name;
			mt.Code = auo.Id.ToString(); // code for the metatable
			int mtCode = auo.Id;

			if (dataTable.ExtendedProperties.ContainsKey("ParentTableName"))
				mt.Parent = MetaTableCollection.Get((string)dataTable.ExtendedProperties["ParentTableName"]);

			foreach (DataColumn dc in dataTable.Columns)
			{
				MetaColumn mc = new MetaColumn();
				mc.MetaTable = mt;
				mc.Name = dc.ColumnName;
				MetaColumn oldMc = null;
				if (oldMt != null) oldMc = oldMt.GetMetaColumnByName(mc.Name); // see if column name exists
				if (oldMc != null && oldMc.ResultCode != "") // use any existing code
					mc.ResultCode = oldMc.ResultCode;
				else mc.ResultCode = aDao.GetNextIdLong().ToString();

				if (dc.Caption != null) mc.Label = dc.Caption;
				else mc.Label = mc.Name;
				if (dc.DataType == typeof(CompoundId))
				{
					mc.DataType = MetaColumnType.CompoundId;
					if (dc.ExtendedProperties.ContainsKey("StorageType") && dc.ExtendedProperties["StorageType"] is MetaColumnType &&
						((MetaColumnType)dc.ExtendedProperties["StorageType"]) == MetaColumnType.String)
						mc.ColumnMap = "EXT_CMPND_ID_TXT"; // text column
					else mc.ColumnMap = "EXT_CMPND_ID_NBR"; // numeric column otherwise
				}
				else if (dc.DataType == typeof(int) || dc.DataType == typeof(Int16) || dc.DataType == typeof(Int32)) mc.DataType = MetaColumnType.Integer;
				else if (dc.DataType == typeof(float) || dc.DataType == typeof(double)) mc.DataType = MetaColumnType.Number;
				else if (dc.DataType == typeof(QualifiedNumber)) mc.DataType = MetaColumnType.QualifiedNo;
				else if (dc.DataType == typeof(string)) mc.DataType = MetaColumnType.String;
				else if (dc.DataType == typeof(DateTime)) mc.DataType = MetaColumnType.Date;
				else if (dc.DataType == typeof(MoleculeMx)) mc.DataType = MetaColumnType.Structure;
				else throw new Exception("Invalid data type " + dc.DataType.ToString());

				if (dc.ExtendedProperties.ContainsKey("DisplayLevel"))
					mc.InitialSelection = (ColumnSelectionEnum)dc.ExtendedProperties["DisplayLevel"];
				if (dc.ExtendedProperties.ContainsKey("DisplayWidth"))
					mc.Width = (float)dc.ExtendedProperties["DisplayWidth"];
				if (dc.ExtendedProperties.ContainsKey("DisplayFormat"))
					mc.Format = (ColumnFormatEnum)dc.ExtendedProperties["DisplayFormat"];
				if (dc.ExtendedProperties.ContainsKey("Decimals"))
					mc.Decimals = (int)dc.ExtendedProperties["Decimals"];

				mt.MetaColumns.Add(mc);
			}

			ToolHelper.CreateAnnotationTable(mt, auo);

			aDao.BeginTransaction(); // insert all data in single transaction

			if (showProgress)
				Progress.Show("Writing data to annotation table...");
			int t1 = TimeOfDay.Milliseconds();
			int writeCount = 0;

			foreach (DataRow dr in dataTable.Rows)
			{
				if (dr.IsNull(0)) continue; // shouldn't happen
				string key = dr[0].ToString();
				key = CompoundId.NormalizeForDatabase(key, mt.Root);
				long rslt_grp_id = aDao.GetNextIdLong();

				for (int ci = 1; ci < dataTable.Columns.Count; ci++) // do columns after key
				{
					if (dr.IsNull(ci)) continue;
					DataColumn dc = dataTable.Columns[ci];
					MetaColumn mc = mt.MetaColumns[ci];
					int mcCode = Int32.Parse(mc.ResultCode);
					avo = new AnnotationVo();
					avo.rslt_grp_id = rslt_grp_id; // keep row together

					if (dc.DataType == typeof(CompoundId)) // shouldn't happen since key processed already
						avo.rslt_val_txt = dr[ci].ToString();

					else if (dc.DataType == typeof(int) || dc.DataType == typeof(Int16) || dc.DataType == typeof(Int32) ||
						dc.DataType == typeof(float) || dc.DataType == typeof(double))
						avo.rslt_val_nbr = (double)dr[ci];

					else if (dc.DataType == typeof(QualifiedNumber))
					{
						QualifiedNumber qn = (QualifiedNumber)dr[ci];
						avo.rslt_val_nbr = qn.NumberValue;
						avo.rslt_val_prfx_txt = qn.Qualifier;
						avo.rslt_val_txt = qn.TextValue;
						avo.dc_lnk = qn.Hyperlink;
					}

					else if (dc.DataType == typeof(string))
						avo.rslt_val_txt = dr[ci].ToString();

					else if (dc.DataType == typeof(DateTime))
						avo.rslt_val_dt = (DateTime)dr[ci];

					else if (dc.DataType == typeof(MoleculeMx))
						avo.rslt_val_txt = dr[ci].ToString();

					AddAnnotationVoToList(avo, key, mtCode, mcCode, voList);
				}

				writeCount++;

				if (Progress.CancelRequested) // check for cancel
				{
					aDao.Commit();
					aDao.Insert(voList);
					voList.Clear();
					aDao.Commit();
					aDao.Dispose();
					if (showProgress) Progress.Hide();
					MessageBoxMx.ShowError("Writing of annotation table cancelled.");
					return null;
				}

				int t2 = TimeOfDay.Milliseconds();
				if (showProgress && t2 - t1 >= 1000)
				{
					t1 = t2;
					Progress.Show("Writing data to annotation table " + writeCount.ToString() +
						" of " + dataTable.Rows.Count.ToString() + " ...");
					aDao.Insert(voList);
					voList.Clear();
					aDao.Commit();
				}
			}

			aDao.Insert(voList);
			voList.Clear();
			aDao.Commit();
			aDao.Dispose();

			if (showProgress) Progress.Hide();
			return mt; // return metatable name
		}

		/// <summary>
		/// Add an annotation vo to a vo list filling common parameters from args
		/// </summary>
		/// <param name="vo"></param>
		/// <param name="cid"></param>
		/// <param name="mt"></param>
		/// <param name="mc"></param>
		/// <param name="voList"></param>
		/// <returns></returns>

		public static void AddAnnotationVoToList(
			AnnotationVo vo,
			string cid,
			int methodVersionId,
			int resultTypeId,
			List<AnnotationVo> voList)
		{
			vo.ext_cmpnd_id_txt = cid; // compound id
			vo.mthd_vrsn_id = methodVersionId;
			vo.rslt_typ_id = resultTypeId;
			vo.chng_op_cd = "I"; // this is an insert
			vo.chng_usr_id = SS.I.UserName;
			voList.Add(vo);
		}

		/// <summary>
/// Create an annotation table given its metatable and user object
/// </summary>
/// <param name="mt"></param>
/// <param name="uo"></param>

		public static void CreateAnnotationTable (
			MetaTable mt,
			UserObject uo)
		{
			uo.Type = UserObjectType.Annotation; // be sure type is set
			uo.Content = mt.Serialize(); // serialize the metatable to the user object
			UserObjectTree.GetValidUserObjectTypeFolder(uo);
			UserObjectDao.Write(uo); // write the user object

			MetaTableCollection.UpdateGlobally(mt); // update map with modified metatable
			return;
		}

/// <summary>
/// Add a query to the window list, make it current & render
/// </summary>
/// <param name="label"></param>
/// <param name="mql"></param>

		public static void AddQueryToWindowList(
			string label,
			string mql,
			bool convertToSimpleCriteria)
		{
			Query q = MqlUtil.ConvertMqlToQuery(mql);
			if (convertToSimpleCriteria) MqlUtil.ConvertComplexCriteriaToQueryColumnCriteria(q,QueryLogicType.And);
			q.UserObject.Name = label;

			int wi = QbUtil.GetQueryIndex(label);
			if (wi >= 0) // replace existing query
			{
				Document mwt = (Document)QbUtil.DocumentList[wi];
				mwt.Type = DocumentType.Query;
				mwt.Content = q;
				QbUtil.SelectQuery(wi);
			}

			else QbUtil.AddQueryAndRender(q, false); // new query
		}

/// <summary>
/// Get default similarity method & limit
/// </summary>
/// <param name="simType">Normal, Sub, Super or ECFP4</param>
/// <param name="minSimTok">Minimum required similarity (1 - 100)</param>
/// <param name="maxHitsTok">Max sim hits (blank or integer > 0)</param>

		public static void GetDefaultSimMethod (
			out string simType,
			out string minSimTok,
			out string maxHitsTok)
		{
			simType = "Normal";
			minSimTok = "67";
			maxHitsTok = "";

			string simMethod = Preferences.Get("DefaultSimMethod");
			string[] sa = simMethod.Split(' ');
			if (sa.Length >= 1)
			{
				if (!Lex.Contains(sa[0], "gfp")) // default to normal if disabled gpf type
					simType = sa[0];
			}

			if (sa.Length >= 2)
				minSimTok = sa[1];

			if (sa.Length >= 3)
				maxHitsTok = sa[2];

			return;
		}

		/// <summary>
		/// Setup a dropdown control from a dictionary
		/// </summary>
		/// <param name="ctl">Control to set up</param>
		/// <param name="dictName">dictionary name, null to reset control</param>
		/// <param name="reload">reload the list even if previously loaded</param>
		/// <returns></returns>

		public static bool SetListControlItemsFromDictionary(
			object ctlObj,
			string dictName,
			bool reload)
		{
			List<string> dict;
			int begRow, rowSel, i1;

			if (dictName == null || dictName == "")
			{ // no dictionary, clear dropdown
				UIMisc.SetListControlItems(ctlObj, "");
				return true;
			}

			dict = DictionaryMx.GetWords(dictName, true);
			if (dict == null) return false;

			StringBuilder buf = new StringBuilder();

			foreach (string s in dict)
			{
				if (buf.Length > 0) buf.Append("\n");
				buf.Append(s);
			}

			UIMisc.SetListControlItems(ctlObj, buf.ToString());

			return true;
		}


		/// <summary>
		/// Dialog to make multiple dictionary selections from checked list dialog box
		/// </summary>
		/// <param name="dictionaryName"></param>
		/// <param name="selections"></param>
		/// <param name="prompt"></param>
		/// <returns></returns>

		public static string GetCheckedListBoxDialog(
			string dictionaryName,
			string selections,
			string prompt)
		{
			MetaTable mt = new MetaTable(); // create dummy table
			MetaColumn mc = mt.AddMetaColumn(dictionaryName, prompt, MetaColumnType.String);
			mc.Dictionary = dictionaryName;

			//if (selections.Contains(","))  { ... } (still have problem for single item)

			// split and reformat to properly handle unquoted items with spaces in them, e.g. "big cat, little dog"

			List<string> items = Csv.SplitCsvString(selections, false);
			selections = Csv.JoinCsvString(items);

			QueryTable qt = new QueryTable(mt);
			QueryColumn qc = qt.QueryColumns[0];
			qc.Criteria = mc.Name + " in (" + selections + ")";
			bool success = CriteriaDictMultSelect.Edit(qc);
			if (!success) return null;

			ParsedSingleCriteria psc = MqlUtil.ParseQueryColumnCriteria(qc);
			if (psc == null || psc.ValueList == null) selections = "";
			else selections = MqlUtil.FormatValueListString(psc.ValueList, false);
			return selections;
		}

	}
}
