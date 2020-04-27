using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Windows.Forms;

namespace Mobius.SpotfireClient
{

	/// <summary>
	/// Interface code for Spotfire API calls that manipute a Spotfire AnalysisAppplication instance
	/// </summary>

	public class SpotfireApiClient
	{
		string AnalysisPath = ""; // path to analysis: either a file path or a Spotfire server libaray path

		public AnalysisApplicationMsx Analysis = null; // link to current analysis

		public DocumentMsx Document => Analysis?.Document; // current document

		public DataTableCollectionMsx DataTables => Document?.DataManager?.TableCollection;

		public static SpotfireApiClient DxpApiClient = null; // use single static instance if using COM/DXP
		public SpotfireComClientWrapper CCW = null; // wrapper for COM client if using COM 

		public SpotfireWebPlayerClient SWPC = null; // client that controls interaction with WebPlayer and the containing WebBrowser control

		//public static string DefaultAnalysisName = "TestAnalysis";
		//public static string DefaultAnalysisName = "TestAnalysis.TablePlot";
		//public static string DefaultAnalysisName = "TestAnalysis.ScatterPlot";
		//public static string DefaultAnalysisName = "MST_TrellisCard";
		//public static string DefaultAnalysisName = "MST_SpillTheBeans"; 
		public static string DefaultAnalysisName = "MST_Scatterplot";

		//public static string DefaultSerializedTestQuery = @"C:\Downloads\MST_TrellisCardTestQuery.xml";
		public static string DefaultSerializedTestQuery = @"C:\Downloads\MST_SpillTheBeansTestQuery.xml";

		public static string DefaultDataFileUrl = "http://[server]/MobiusQueryExecutorApp/SpotfireData/SfStbQuery_CORP_STRUCTURE.stdf";


		/////////////////////////////////////////////////////////////
		/// <summary>
		/// Use Web Player client (true for normal PRD case)
		/// </summary>
		/////////////////////////////////////////////////////////////

		public static bool UseWebPlayerClient = true; ///****** Normally true, set to false for dev/debug ******///

		/// <summary>
		/// Use Analyst full PC client (Dev/Debug case)
		/// </summary>

		public static bool UseAnalystClient { get => !UseWebPlayerClient; set => UseWebPlayerClient = !value; }

		/////////////////////////////////////////////////////////////

		public static bool PerformNormalCombinedOperations = true; // normally true, set to false for dev/debug work to break up operations into smaller individual suboperations
		public static bool SplitupOperationsForDevPurposes => !PerformNormalCombinedOperations; // true for dev/debug work, false for prd

		public static bool Debug = false;

		/// <summary>
		/// Default constructor
		/// </summary>

		public SpotfireApiClient()
		{
			return;
		}

		public static SpotfireApiClient GetWebPlayerApiClient(
			WebBrowser webBrowser,
			Control webBrowserContainer,
			TextEdit statusTextBox)
		{
			SpotfireApiClient apiClient = new SpotfireApiClient();
			apiClient.InitializeWebBrowserControl(webBrowser, webBrowserContainer, statusTextBox);
			return apiClient;
		}

		/// <summary>
		/// Initialize the Winforms WebBrowser control
		/// </summary>
		/// 
		public void InitializeWebBrowserControl(
			WebBrowser webBrowser,
			Control webBrowserContainer,
			TextEdit statusTextBox)
		{
			SWPC = new SpotfireWebPlayerClient();
			SWPC.InitializeWebBrowserControl(webBrowser, webBrowserContainer, statusTextBox);

			return;
		}

		/// <summary>
		/// Get the DXP/COM api client starting a single instance of DXP Analyst if not yet done
		/// </summary>
		/// <returns></returns>

		public static SpotfireApiClient GetDxpApiClient()
		{
			if (DxpApiClient == null)
			{
				DxpApiClient = new SpotfireApiClient();
				DxpApiClient.StartDxpProcess();
			}

			return DxpApiClient;
		}

		/// <summary>
		/// Launch DXP analyst client locally
		/// </summary>
		public void StartDxpProcess()
		{
			UseAnalystClient = true;
			CCW = new SpotfireComClientWrapper();
			CCW.StartDxpProcess();

			return;
		}

		/// <summary>
		/// Get contextual information on this MobiusSpotfireApiServer .Net assembly instance
		/// </summary>
		/// <returns></returns>

		public string GetApiServerInstanceContextInfo()
		{
			string result = CallSpotfireApi("GetApiServerInstanceContextInfo");
			return result;
		}

		/// <summary>
		/// Open the specified Spotfire do initial display of visualization optionally clearing the data rows in the analysis
		/// </summary>
		/// <param name="analysisPath"></param>
		/// <param name="initializeAnalysisForMobiusInteraction"></param>
		/// <returns></returns>

		public AnalysisApplicationMsx OpenAnalysis(
			string analysisPath,
			bool initializeAnalysisForMobiusInteraction = true)
		{
			if (Lex.IsUndefined(analysisPath)) throw new NullReferenceException("AnalysisPath is not defined");

			bool opened = false;

			if (UseWebPlayerClient)
			{
				if (Lex.Contains(analysisPath, @"\")) // if drive path get just the file name
					analysisPath = Path.GetFileName(analysisPath);

				if (!Lex.Contains(analysisPath, @"/")) // include library folder if not done
					analysisPath = "/Mobius/Visualizations/" + analysisPath;

				AnalysisPath = analysisPath;

				opened = OpenAnalysisInWebPlayer(analysisPath);
			}

			else
			{
				if (CCW == null)
					CCW = new SpotfireComClientWrapper();

				if (Lex.Contains(analysisPath, "/")) // if http get just the file name
					analysisPath = Path.GetFileName(analysisPath);

				if (!Lex.Contains(analysisPath, @"\")) // include default disk folder if not done
					analysisPath = @"C:\Downloads\" + analysisPath + ".dxp";

				AnalysisPath = analysisPath;

				opened = CCW.OpenAnalysis(analysisPath);
			}

			if (!opened) return null;


			if (initializeAnalysisForMobiusInteraction) // Init the just-opened analysis 
				InitializeAnalysisForMobiusInteraction();

			Analysis = new AnalysisApplicationMsx();
			DocumentMsx doc = GetDocument(); // retrieve the document structure
			Analysis.Path = analysisPath;

			return Analysis;
		}

		/// <summary>
		/// Open webplayer analysis
		/// </summary>
		/// <param name="webBrowser"></param>

		bool OpenAnalysisInWebPlayer(
			string analysisPath)
		{
			if (SWPC == null) DebugMx.ArgException("SpotfireWebPlayerClient is null");

			//Progress.Show("Opening visualization...");

			bool opened = SWPC.OpenAnalysis(analysisPath);

			//Progress.Hide();

			return opened;
		}

		/// <summary>
		/// CloseAnalysis
		/// </summary>

		public void CloseAnalysis()
		{
			if (UseWebPlayerClient)
			{
				if (SWPC == null) return;
				SWPC.CloseAnalysis();
			}

			else
			{
				if (CCW == null) return;
				CCW.CloseAnalysis();
			}
		}

		/// <summary>
		/// Get Document and underlying doc nodes (DataTables, Pages, etc.)
		/// </summary>
		/// <returns></returns>

		public DocumentMsx GetDocument()
		{
			string serializedDoc = null;
			DocumentMsx doc = null;
			try
			{
				serializedDoc = CallSpotfireApi("GetDocument");
				int docLength = serializedDoc.Length;

				if (Analysis == null) Analysis = new AnalysisApplicationMsx();

				doc = DocumentMsx.Deserialize(serializedDoc, Analysis);

				doc.Owner = Analysis;

				doc.ValidateNode();

				return doc;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// Get the definitions of the DataTables in the analysis
		/// </summary>
		/// <returns></returns>

		public DataTableCollectionMsx GetDataTableCollection()
		{
			string serializedDtc = null;
			DataTableCollectionMsx dtc = null;
			try
			{
				serializedDtc = CallSpotfireApi("GetDataTableCollection");
				dtc = (DataTableCollectionMsx)SerializeMsx.Deserialize(serializedDtc, typeof(DataTableCollectionMsx));

				if (Analysis == null) Analysis = new AnalysisApplicationMsx();
				if (Analysis.Document == null) Analysis.Document = new DocumentMsx();
				if (Analysis.Document.DataManager == null) Analysis.Document.DataManager = new DataManagerMsx();

				Analysis.Document.DataManager.TableCollection = dtc;
				dtc.UpdatePostDeserializationSecondaryReferences(Analysis.Document.DataManager); // update secondary references including owner and Visualization references to DataColumns etc

				return dtc;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// Get a string Document property
		/// </summary>
		/// <param name="propName"></param>
		/// <returns></returns>

		public string GetDocumentProperty(
			string propName)
		{
			string value = CallSpotfireApi("GetDocumentProperty", propName);
			return value;
		}

		/// <summary>
		/// Set a string DocumentProperty
		/// </summary>
		/// <param name="propName"></param>
		/// <param name="value"></param>

		public string SetDocumentProperty(
			string propName,
			string value)
		{
			string oldVal = CallSpotfireApi("SetDocumentProperty", propName, value);
			return oldVal;
		}

		/// <summary>
		/// Get properties of active visual
		/// </summary>
		/// <returns></returns>
		public VisualMsx GetActiveVisual()
		{
			VisualMsx sVis, cVis = null;

			sVis = GetVisual("<activeVisual>"); // get active visual from server
			if (sVis != null)
			{
				Document.TryGetVisual(sVis.Id, out cVis); // get object in document on client with same id
			}

			Document.ActiveVisualReference = cVis; // update active vis if changed
			return cVis;
		}


		/// <summary>
		/// Initialize Analysis For Mobius Interaction
		/// - Remove rows from all DataTables
		/// - Save the initial Spotfire column names into the MobiusRole DataColumn property
		/// </summary>
		/// <returns></returns>

		public string InitializeAnalysisForMobiusInteraction()
		{
			string response = CallSpotfireApi("InitializeAnalysisForMobiusInteraction");
			return response;
		}

		/// <summary>
		/// Get a visual from the server by id
		/// Note that this method returns a new copy of the visual
		/// that should already be in the document
		/// </summary>
		/// <param name="visualName"></param>
		/// <returns></returns>

		public VisualMsx GetVisual(
			string visualName)
		{
			string serializedVisual = null;
			VisualMsx vis = null;

			try
			{
				serializedVisual = GetVisualProperties(visualName);
				if (Lex.IsUndefined(serializedVisual)) return null;

				vis = VisualMsx.Deserialize(serializedVisual, Analysis);
				return vis;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// GetVisualProperties
		/// </summary>
		/// <param name="visualname"></param>
		/// <returns></returns>

		public string GetVisualProperties(  
			string visualname)
		{
			string serializedProps = CallSpotfireApi("GetVisualProperties", visualname);
			return serializedProps;
		}

		/// <summary>
		/// SetVisualProperties
		/// </summary>
		/// <param name="visualname"></param>
		/// <param name="serializedProps"></param>
		/// <returns></returns>

		public string SetVisualProperties(
			string visualname,
			string serializedProps)
		{
			//if (Lex.Contains(serializedProps, "ude]")) serializedProps = serializedProps; // debug
			string result = CallSpotfireApi("SetVisualProperties", visualname, serializedProps);
			return result;
		}

		/// <summary>
		/// Reset the columns for a datatable to the original set of columns and remove all rows
		/// </summary>
		/// <param name="dtName"></param>
		/// <returns></returns>

		public DataTableMsx ResetDataTable(
			string dtName)
		{
			DataTableMsx dtMsx = Document.DataManager.TableCollection.GetTableByNameWithException(dtName);

			return ResetDataTable(dtMsx);
		}

		/// <summary>
		/// Reset the columns for a datatable to the original set of columns and remove all rows
		/// </summary>
		/// <param name="dtMsx"></param>
		/// <returns></returns>

		public DataTableMsx ResetDataTable(
			DataTableMsx dtMsx)
		{
			if (dtMsx == null) throw new ArgumentNullException("DataTableMsx");

			string id = dtMsx.Id;
			if (Lex.IsUndefined(id)) id = dtMsx.Name;

			if (Lex.IsUndefined(id)) throw new ArgumentOutOfRangeException("DataTableMsx");

			string serializedDtMsx = CallSpotfireApi("ResetDataTable", id);
			DataTableMsx dtMsx2 = (DataTableMsx)SerializeMsx.Deserialize(serializedDtMsx, typeof(DataTableMsx));
			Document.DataManager.TableCollection.ReplaceTableWithUpdatedVersion(dtMsx2);
			return dtMsx2;
		}

		/// <summary>
		/// RemoveRowsFromDataTable
		/// </summary>
		/// <param name="dtName"></param>
		/// <returns></returns>

		public string RemoveRowsFromDataTable(
			string dtName)
		{
			DataTableMsx dtMsx = Document.DataManager.TableCollection.GetTableByNameWithException(dtName);

			string result = RemoveRowsFromDataTable(dtMsx);
			return result;
		}

		/// <summary>
		/// RemoveRowsFromDataTable
		/// </summary>
		/// <param name="dtMsx"></param>
		/// <returns></returns>

		public string RemoveRowsFromDataTable(
			DataTableMsx dtMsx)
		{
			if (dtMsx == null) throw new ArgumentNullException("DataTableMsx");

			string id = dtMsx.Id;
			if (Lex.IsUndefined(id)) id = dtMsx.Name;

			if (Lex.IsUndefined(id)) throw new ArgumentOutOfRangeException("DataTableMsx");

			string result = CallSpotfireApi("RemoveRowsFromDataTable", id);
			return result;
		}

		/// <summary>
		/// RemoveRowsFromAllDataTables
		/// </summary>
		/// <returns></returns>

		public string RemoveRowsFromAllDataTables()
		{
			string result = CallSpotfireApi("RemoveRowsFromAllDataTables");
			return result;
		}

		/// <summary>
		/// Import multiple Mobius-written Spotfire data files into the data tables of an analyis and remap them as specified
		/// Note that all data tables are cleared before loading any files
		/// </summary>
		/// <param name="mapParmsList"></param>
		/// <returns></returns>

		public string MergeMobiusDataFilesAndRemapDataTables_Obsolete(
			List<DataTableMapParms> mapParmsList)
		{
			string result = "";
			if (PerformNormalCombinedOperations) // normal, single-step operation
			{
				string serializedMapParmsList = SerializeMsx.Serialize(mapParmsList);
				result = CallSpotfireApi("MergeMobiusDataFilesAndRemapDataTables", serializedMapParmsList);
			}

			else // break into individual steps for debug
			{
				foreach (DataTableMapParms mapParms in mapParmsList)
				{
					result += MergeMobiusDataFileAndRemapDataTable_Obsolete(mapParms);
				}
			}

			// Get modified set of data tables

			DataTableCollectionMsx dtc = GetDataTableCollection(); // get modified set of data tables

			return result;
		}

		/// <summary>
		/// Import a single Mobius-written Spotfire data file into a single Spotfire data table and remap it as specified
		/// </summary>
		/// <param name="mapParms"></param>
		/// <returns></returns>

		public string MergeMobiusDataFileAndRemapDataTable_Obsolete(
			DataTableMapParms mapParms)
		{
			string result = "";

			if (Lex.IsUndefined(mapParms.FileUrl)) // if no data then remove rows from datatable and restore original columns
			{
				DataTableMsx dt2 = ResetDataTable(mapParms.SpotfireTableName);
				result = "Data table reset";
			}

			else if (PerformNormalCombinedOperations) // normal, single-step operation
			{
				string serializedMapParms = SerializeMsx.Serialize(mapParms);
				result = CallSpotfireApi("MergeMobiusDataFileAndRemapDataTable", serializedMapParms);
			}

			else // debug, two individual steps
			{
				string importResult = MergeMobiusDataFile_Obsolete(mapParms);

				string remapResult = RemapDataTable(mapParms);
				result += importResult + "\r\n" + remapResult;
			}

			DataTableCollectionMsx dtc = GetDataTableCollection(); // get modified set of data tables (could just get single table modified)

			return result;
		}

		/// <summary>
		/// Import a mobius data file into an analysis template
		/// </summary>
		/// <param name="mapParms"></param>
		/// <returns></returns>

		public string MergeMobiusDataFile_Obsolete(
			DataTableMapParms mapParms)
		{
			string serializedMapParms = SerializeMsx.Serialize(mapParms);
			string result = CallSpotfireApi("MergeMobiusDataFile", serializedMapParms);

			DataTableCollectionMsx dtc = GetDataTableCollection(); // get modified set of data tables

			return result;
		}

		/// <summary>
		/// Remap the analysis parameter values
		/// </summary>
		/// <param name="mapParms"></param>
		/// <returns></returns>

		public string RemapDataTable(
			DataTableMapParms mapParms)
		{
			string serializedMapParms = SerializeMsx.Serialize(mapParms);
			string result = CallSpotfireApi("RemapDataTable", serializedMapParms);

			DataTableCollectionMsx dtc = GetDataTableCollection(); // get modified set of data tables

			return result;
		}

		/// <summary>
		/// Replace the data for a list of DataTables assuming no remaping has been done between
		/// the analysis and the exported data since the analysis was set up.
		/// </summary>
		/// <param name="mapParmsList"></param>
		/// <returns></returns>

		public string ImportMobiusDataFiles(
			List<DataTableMapParms> mapParmsList)
		{
			string result = "";
			if (PerformNormalCombinedOperations) // normal, single-step operation
			{
				string serializedMapParmsList = SerializeMsx.Serialize(mapParmsList);
				result = CallSpotfireApi("ImportMobiusDataFiles", serializedMapParmsList);
			}

			else // break into individual steps for debug
			{
				foreach (DataTableMapParms mapParms in mapParmsList)
				{
					result += ImportMobiusDataFile(mapParms);
				}
			}

			return result;
		}

		/// <summary>
		/// Replace the data in a DataTable with the contents of a 
		/// SpotfireTextDataFile retrieved via a URL to the Mobius IIS server 
		/// Note that file extension is important
		/// For text .stdf files use .txt extension (MIME type text/plain)
		/// For binary .sbdf files use .bin extension (MIME type application/octet-stream)
		/// </summary>
		/// <param name="mapParms"></param>
		/// <returns></returns>

		public string ImportMobiusDataFile(
			DataTableMapParms mapParms)
		{
			string serializedMapParms = SerializeMsx.Serialize(mapParms);
			string result = CallSpotfireApi("ImportMobiusDataFile", serializedMapParms);
			return result;
		}

		/// <summary>
		/// Rename the columns in the datatables to the default names for the associated Mobius metacolumns
		/// </summary>
		/// <returns></returns>

		public string RenameColumnsToMobiusNames()
		{
			MetaColumn mc = null;

			GetDataTableCollection(); // be sure we have the current datatables into

			List<DataTableMapParms> mapParmsList = new List<DataTableMapParms>();

			foreach (DataTableMsx dt in DataTables)
			{
				DataTableMapParms mapParms = new DataTableMapParms(true);

				mapParms.SpotfireTableName = dt.Name;
				mapParms.ColumnMapParmsList = new List<ColumnMapParms>();

				foreach (DataColumnMsx dc in dt.Columns)
				{
					mc = GetMetaColumnAssociatedWithSpotfireDataColumn(dc);
					if (mc == null) continue;

					string mcDotMtName = mc.MetaTableDotMetaColumnName;

					ColumnMapParms dc2 = new ColumnMapParms();

					dc2.SpotfireColumnName = dc.Name; // existing name
					dc2.NewSpotfireColumnName = mc.Label; // new name
					dc2.MobiusRole = dc.MobiusRole;
					dc2.MobiusFileColumnName = mcDotMtName;

					mapParms.ColumnMapParmsList.Add(dc2);
				}

				if (mapParms.ColumnMapParmsList.Count > 0)
					mapParmsList.Add(mapParms);
			}

			string serializedMapParmsList = SerializeMsx.Serialize(mapParmsList);
			string result = CallSpotfireApi("RenameColumns", serializedMapParmsList);
			return result;
		}

		/// <summary>
		/// Examine the attributes of a Spotfire DataColumn to try to discover any 
		/// associated MetaColumn
		/// </summary>
		/// <param name="dc"></param>
		/// <returns></returns>

		public static MetaColumn GetMetaColumnAssociatedWithSpotfireDataColumn(
			DataColumnMsx dc)
		{
			MetaTable mt = null;
			MetaColumn mc = null;

			if (Lex.IsDefined(dc.MobiusFileColumnName)) // check for MetaTableName.MetaColumnName
			{
				mc = MetaColumn.ParseMetaTableMetaColumnName(dc.MobiusFileColumnName);
				if (mc != null) return mc;
			}

			if (Lex.IsDefined(dc.ExternalName)) // check for MetaTableName.MetaColumnName
			{
				mc = MetaColumn.ParseMetaTableMetaColumnName(dc.ExternalName);
				if (mc != null) return mc;
			}

			if (Lex.IsDefined(dc.Origin) && Lex.IsDefined(dc.ExternalName)) // origin file name may contain metatable name, try to parse it out
			{
				string mtName = Path.GetFileNameWithoutExtension(dc.Origin);
				while (true) // loop truncating the potential metatable name on the left until hit or we run out of name
				{
					mt = MetaTableCollection.Get(mtName);
					if (mt != null)
					{
						mc = mt.GetMetaColumnByName(dc.Name); // see if col name is a metacolumn name
						if (mc != null) return mc;

						mc = mt.GetMetaColumnByLabel(dc.Name); // see if col name is a metacolumn label
						if (mc != null) return mc;

						mc = mt.GetMetaColumnByName(dc.ExternalName); // see if external name is a metacolumn name
						if (mc != null) return mc;

						mc = mt.GetMetaColumnByLabel(dc.ExternalName); // see if col name is a metacolumn label
						if (mc != null) return mc;
					}

					int i1 = mtName.IndexOf("_");
					if (i1 < 0 || (i1 + 1) == mtName.Length) break;
					mtName = mtName.Substring(i1 + 1);
				}
			}

			return mc;
		}

		/// <summary>
		/// Rename a column
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="currentColName"></param>
		/// <param name="newColName"></param>

		public void RenameColumn(
			string tableName,
			string currentColName,
			string newColName)
		{
			string result = CallSpotfireApi("RenameColumn", tableName, currentColName, newColName);
			return;
		}

		/// <summary>
		/// ConfigureAnalysisForMobiusUse
		/// </summary>
		/// <param name="args"></param>

		public void ConfigureAnalysisForMobiusUse(
			string args)
		{
			if (UseWebPlayerClient) // not currently supported
			{
				throw new Exception("This command is only available when running in DXP mode");
			}

			else // using analysis client via COM interface
			{
				string result = CCW.ConfigureAnalysisForMobiusUse(args);

				return;
			}
		}

		/// <summary>
		/// Call the Spotfire API
		/// </summary>
		/// <param name="method"></param>
		/// <param name="args"></param>
		/// <returns></returns>

		public string CallSpotfireApi(
			string method,
			params string[] args)
		{
			string result = null;

			if (UseWebPlayerClient)
			{
				result = SWPC.CallSpotfireApi(method, args);
			}

			else // using analysis client via COM interface
			{
				result = CCW.CallSpotfireApi(method, args);
			}

			if (Lex.Contains(result, "<MsxException>:")) // some expection being passed back
				throw new Exception(result);

			return result;
		}
	}
}

