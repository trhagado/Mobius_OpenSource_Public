using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;
using Mobius.ServiceFacade;
using Mobius.ClientComponents;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Web;
using System.Threading;
using System.Windows.Forms;

namespace Mobius.SpotfireClient
{

	/// <summary>
	/// General SpotfireView properties and operations
	/// </summary>

	public class SpotfireViewManager : ViewManager
	{
		public SpotfireViewProps SVP => SpotfireViewProps; // alias from SpotfireViewProps

		Query Query => SVP?.Query; // associated query

		DataTableMapsMsx DataTableMaps => SVP?.DataTableMaps; // associated DataMap
		DataTableMapMsx CurrentMap => SVP?.DataTableMaps?.CurrentMap; // current DataTableMap

		public SpotfireApiClient SpotfireApiClient // API link to Spotfire session to interact with
		{ 
			get => SpotfireSession.SpotfireApiClient;
			set => SpotfireSession.SpotfireApiClient = value;
		}

		public AnalysisApplicationMsx Analysis => SpotfireApiClient?.Analysis; // link to current analysis
		public DocumentMsx Document => Analysis?.Document; // current document
		public DataTableCollectionMsx DataTables => Document?.DataManager?.TableCollection;

		internal ColumnInfo LabelColInfo; // additional col info for label series
		internal bool CheckedServerForBackgroundImage = false;

		public SpotfirePageControl SpotfirePageControl; // the SpotfirePageControl containing the SpotfirePanel, persists when switchhed to another view
		public SpotfirePanel SpotfirePanel { get { return SpotfirePageControl?.SpotfirePanel; } }
		internal WebBrowser WebBrowser { get { return SpotfirePanel?.WebBrowser; } }
		internal MemoEdit StatusTextBox { get { return SpotfirePanel?.SpotfireApiLog; } } // for displaying & updating status of interaction with WebPlayer Document

		public override bool DisposeOfPageControlsWhenSwitchingToOtherPage() { return false; } // don't dispose of existing WebBrowser containing the visualization when switching away

		public string FileName = ""; // name of file containing data to view

		bool Configuring = false;

		public void ValidateSpotfireViewPropsInitialization()
		{
			if (SpotfireViewProps == null)
				SpotfireViewProps = new SpotfireViewProps(this);

			if (SVP?.DataTableMaps == null)
				SVP.DataTableMaps = new DataTableMapsMsx(SVP);

			DataTableMapMsx CurrentMap = SVP?.DataTableMaps?.CurrentMap; // current DataTableMap

			if (CurrentMap == null)
				CurrentMap = SVP.DataTableMaps.Add();

			if (CurrentMap.ColumnMapCollection == null || CurrentMap.ColumnMapCollection.Count == 0)
				CurrentMap.ColumnMapCollection = new ColumnMapCollection();

			return;
		}

		/// <summary>
		/// TestWebPlayer
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		/// 

		public static string TestWebPlayer(
		string url)
		{
			/*** Options Parameter ***
			 
				Example to hide the status bar and page navigation: 

				Header	1
				Status bar	2
				Toolbar	3
				Page navigation	4
				Filter panel	5
				Details on demand	6
				Undo\Redo	7
				Export visualization image	8
				Analysis information	9
				Download as DXP file	10
				Help	11
				About	12
				Close	13
				Logout	14
				Edit button	15

			 */


			if (!Lex.IsDefined(url))
				url = "<webPlayerUrl>";

			PopupHtml bp = new PopupHtml();
			UIMisc.PositionPopupForm(bp);

			bp.Text = "Spotfire WebPlayer";
			bp.Show();

			bp.WebBrowser.Navigate(url);

			return "";
		}

		/// <summary>
		/// Constructor from ResultsViewItem
		/// </summary>

		public SpotfireViewManager(
			ResultsViewModel rvi)
		{
			ViewType = ResultsViewType.Spotfire;
			Title = "Spotfire Visualization";
			SpotfireViewProps = new SpotfireViewProps(this);
			SpotfireViewProps.AnalysisPath = rvi.ViewSubtype;
			return;
		}

		/// <summary>
		/// Show dialog for initial view setup
		/// </summary>
		/// <returns></returns>

		public override DialogResult ShowInitialViewSetupDialog()
		{
			if (!Lex.Contains(ViewSubtype, "FromLibraryTemplate")) // just return OK unless need to select view first
				return DialogResult.OK; // main work happens in ConfigureRenderingControl

			// Prompt the user for the path to the analysis to use as a template

			string title = "Select Spotfire Analysis";
			string prompt = "Enter the name of the analysis to use as a template for this view (e.g. /Mobius/Visualizations/MyAnalysisTemplate";
			string defaultText = "/Mobius/Visualizations/";

			string path = InputBoxMx.Show(prompt, title, defaultText);
			if (Lex.IsDefined(path))
			{
				ViewSubtype = path; // subtype is the path to the template
				CustomViewTypeImageName = ""; // just use the spotfire bitmap for the view initially
				SpotfireViewProps.AnalysisPath = path; // also need to set here
				string[] sa = Lex.Split(path, "/");
				if (sa.Length > 0) // use last part of path as initial title
					Title = sa[sa.Length - 1];
				return DialogResult.OK;
			}

			else return DialogResult.Cancel;
		}

		/// <summary>
		/// Show the properties for the view after the initial setup and rendering of the view
		/// </summary>
		/// <returns></returns>

		public override DialogResult ShowInitialViewPropertiesDialog()
		{
			return DialogResult.OK; // just return OK, main work happens in ConfigureRenderingControl
		}

		/// <summary>
		/// Create the view display controls and link them into the QueryResultsControl and the ResultsView object
		/// </summary>
		/// <returns></returns>

		public override Control AllocateRenderingControl()
		{
			if (SpotfirePageControl == null) // allocate if not done yet
			{
				SpotfirePageControl = new SpotfirePageControl();
				ConfigureCount = 0;
			}

			SpotfirePanel.SVP = SVP;

			RenderingControl = SpotfirePanel;

			return RenderingControl;
		}

		/// <summary>
		/// Insert the rendering control and associated tools into the display panel 
		/// </summary>

		public override void InsertRenderingControlIntoDisplayPanel(
			XtraPanel viewPanel)
		{
			viewPanel.Controls.Clear(); // remove anything in there
			viewPanel.Controls.Add(SpotfirePanel); // add it to the display panel
			SpotfirePanel.Dock = DockStyle.Fill;
			SpotfirePanel.Location = new System.Drawing.Point(0, 0);
			SpotfirePanel.Size = viewPanel.Size;

			InsertToolsIntoDisplayPanel();

			SpotfirePageControl.EditQueryBut.Enabled =
				ViewManager.IsControlContainedInQueriesControl(viewPanel) ||
				ViewManager.IsCustomExitingQueryResultsCallbackDefined(viewPanel);

			if (Qrc == null || BaseQuery == null) return;

			//SpotfirePageControl.ToolPanel  SpotfirePageControl.ToolPanel
			//Qrc.SetToolBarTools(SpotfirePageControl.ToolPanel, BaseQuery.ViewScale); // show the proper tools and zoom

			//SpotfireToolbar sftb = SpotfirePageControl.SpotfireToolbar;
			//sftb.SVM = this; // link SpotfireToolbar to us
			return;
		}

		/// <summary>
		/// Insert tools into display panel
		/// </summary>

		public override void InsertToolsIntoDisplayPanel()
		{
			if (Qrc == null) return;

			Qrc.SetToolBarTools(SpotfirePageControl.ToolPanel, -1); // show the proper tools

			return;
		}

		/// <summary>
		/// Return true if the view has been defined
		/// </summary>

		public override bool IsDefined
		{
			get
			{
				return (Lex.IsDefined(SpotfireViewProps?.AnalysisPath));
			}
		}

		/// <summary>
		/// Configure and Render the spotfire visualization
		/// </summary>

		public override void ConfigureRenderingControl()
		{
			try
			{
				Configuring = true;
				//DebugLog.Message("Configure started");

				if (ConfigureCount > 0) return; // already configured?

				if (SpotfireApiClient.UseWebPlayerClient)
				{
					SpotfireApiClient = SpotfireApiClient.GetWebPlayerApiClient(WebBrowser, SpotfirePanel, StatusTextBox);
				}

				else // DXP Com client
				{
					SpotfireApiClient = SpotfireApiClient.DxpApiClient;

					if (SpotfireApiClient == null) // need to start
					{
						SpotfireApiClient = SpotfireApiClient.GetDxpApiClient();
						MessageBoxMx.Show("SpotfireDXP process started."); // put up message to allow option of connecting to DXP process vis Visual Studio
					}
				}

				ConfigureCount++;

				DelayedCallback.Schedule(ConfigureRenderingControl_DelayedCallback); // schedule callback
			}
			finally

			{
				//DebugLog.Message("Configure complete");
				Configuring = false;
			}
		}

		/// <summary>
		/// ConfigureRenderingControl_DelayedCallback
		/// </summary>
		/// <param name="state"></param>

		public void ConfigureRenderingControl_DelayedCallback(object state)
		{
			try
			{
				CompleteRetrievalAndExportDataToSpotfire();

				if (Debug) DebugLog.Message("Starting OpenAnalysis: " + SVP.AnalysisPath);
				AnalysisApplicationMsx app = SpotfireApiClient.OpenAnalysis(SVP.AnalysisPath);
				if (app == null)
				{
					MessageBoxMx.ShowError("Could not open analysis: " + SVP.AnalysisPath);
					return;
				}

				app.Container = this;

				if (Lex.IsUndefined(Title)) Title = app.Name; // get title from analysis if not already set
				SVP.AnalysisApp = app;

				bool mappingExists = (DataTableMaps != null && DataTableMaps.Count > 0); // does a mapping currently exist?

				if (Debug) DebugLog.Message("Starting ValidateMapsAgainstAnalysisDocument");
				SVP.ValidateMapsAgainstAnalysisDocument(); // be sure valid mapping is defined

				if (Debug) DebugLog.Message("Starting ImportMobiusDataFiles");
				ImportMobiusDataFiles(); // reimport the data with the new mapping
				if (Debug) DebugLog.Message("Completed ImportMobiusDataFiles");

				//else MergeMobiusDataFilesAndRemapDataTables(); // if some columns have been remapped then must do a remapped remplacement

				if (!mappingExists) // if this is a new spotfire view onto the dataset with no previous mapping then show the mapping dialog
					DataMapDialog.ShowDialog(this);

				return;
			}

			catch (Exception ex)
			{
				string msg = "Error: Unable to create Spotfire view of data\r\n\r\n" + DebugLog.FormatExceptionMessage(ex, true);
				DebugLog.Message(msg);
				MessageBoxMx.ShowError(msg);
				return;
			}
		}

		/// <summary>
		/// Start thread to retrieve data if multithreading
		/// </summary>
		/// <returns></returns>

		void TestThreadedRetrieval()
		{
			Exception threadException = null;

			Thread getDataThread = StartCompleteRetrievalAndExportDataToSpotfire();

			try { getDataThread.Join(); }
			catch (Exception ex) { threadException = ex; }

			return;
		}

		Thread StartCompleteRetrievalAndExportDataToSpotfire()
		{
				ThreadStart ts = new ThreadStart(CompleteRetrievalAndExportDataToSpotfire);
				Thread newThread = new Thread(ts);
				newThread.Name = "CompleteRetrievalAndExportDataToSpotfireThread";
				newThread.IsBackground = true;
				newThread.SetApartmentState(ApartmentState.STA);
				newThread.Start();
				return newThread;
		}

		/// <summary>
		/// Complete data retrieval and export to Spotfire files
		/// </summary>

		void CompleteRetrievalAndExportDataToSpotfire()
		{
			try
			{

				if (Debug) DebugLog.Message("Starting CompleteQueryEngineRowRetrieval");
				DialogResult dr = Qm.DataTableManager.CompleteQueryEngineRowRetrieval();
				if (dr != DialogResult.OK) throw new NotImplementedException("Todo: handle cancel");

				if (Debug) DebugLog.Message("Starting ExportDataToSpotfireFilesIfNeeded");
				dr = Qm.DataTableManager.ExportDataToSpotfireFilesIfNeeded();
				if (dr != DialogResult.OK) throw new NotImplementedException("Todo: handle cancel");
			}

			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}

			SessionManager.DisplayStatusMessage("");
			return;
		}

		/// <summary>
		/// Import multiple Mobius-written Spotfire data files into the data tables of an analyis and remap them as specified
		/// </summary>
		/// <returns></returns>

		//public string MergeMobiusDataFilesAndRemapDataTables()
		//{
		//	List<RemapDataTableParms> mapParmsList = new List<DataTableMapParms>();

		//	foreach (SpotfireDataTableMap dtm in DataTableMaps)
		//	{
		//		DataTableMapParms mapParms = DtmToRdp(mapParms);
		//		mapParmsList.Add(mapParms);
		//	}

		//	string result = SpotfireApiClient.MergeMobiusDataFilesAndRemapDataTables(mapParmsList);

		//	return result;
		//}

		/// <summary>
		/// Import a single Mobius-written Spotfire data file into a single Spotfire data table and remap it as specified
		/// </summary>
		/// <param name="dtm"></param>
		/// <returns></returns>

		//public string MergeMobiusDataFileAndRemapDataTable(
		//	SpotfireDataTableMap dtm)
		//{
		//	DataTableMapParms mapParms = DtmToRdp(dtm);
		//	string result = SpotfireApiClient.MergeMobiusDataFileAndRemapDataTable(mapParms);
		//	return result;
		//}

		/// <summary>
		/// Replace data for all data tables
		/// </summary>
		/// <returns></returns>

		public string ImportMobiusDataFiles()
		{
			List<DataTableMapParms> mapParmsList = new List<DataTableMapParms>();

			foreach (DataTableMapMsx dtm in DataTableMaps)
			{
				DataTableMapParms mapParms = SpotfireDataTableMapToRemapParms(dtm);

				if (Lex.IsDefined(mapParms.FileUrl)) // only include if mapped to a file
					mapParmsList.Add(mapParms);
			}

			if (mapParmsList.Count == 0) mapParmsList = mapParmsList; // allowed?

			string result = SpotfireApiClient.ImportMobiusDataFiles(mapParmsList);

			DataTableMaps.UpdateSpotfireDataTablesAndMapsAfterImport(result);

			return result;
		}

		/// <summary>
		/// Import a single Mobius-written Spotfire data file into a single Spotfire data table and remap it as specified
		/// </summary>
		/// <param name="dtm"></param>
		/// <returns></returns>

		public string ImportMobiusDataFile(
			DataTableMapMsx dtm)
		{
			DataTableMapParms mapParms = SpotfireDataTableMapToRemapParms(dtm);

			string result = SpotfireApiClient.ImportMobiusDataFile(mapParms); // Do the import and get back new set of tables and other info

			DataTableMaps.UpdateSpotfireDataTablesAndMapsAfterImport(result);

			return result;
		}

		/// <summary>
		/// Convert a DataTableMap to a ReplaceDataTableDataParms instance
		/// </summary>
		/// <returns></returns>

		public static DataTableMapParms SpotfireDataTableMapToRemapParms(DataTableMapMsx dtm)
		{
			DataTableMapParms mapParms = new DataTableMapParms(true);

			mapParms.SpotfireTableName = dtm.SpotfireDataTable?.Name;
			mapParms.SpotfireTableId = dtm.SpotfireDataTable?.Id;
			mapParms.FileUrl = dtm.FileUrl; 

			List<ColumnMapParms> cmList = new List<ColumnMapParms>();

			foreach (ColumnMapMsx cm in dtm.ColumnMapList) // convert column maps to data columns that can be serialized for calls to Spotfire API 
			{
				ColumnMapParms cmp = ColumnMapToParms(cm);
				mapParms.ColumnMapParmsList.Add(cmp);
			}

			return mapParms;
		}

		/// <summary>
		/// Convert column maps to parameterized form that can be serialized for calls to Spotfire API 
		/// </summary>
		/// <param name="cm"></param>
		/// <returns></returns>

		public static ColumnMapParms ColumnMapToParms(
			ColumnMapMsx cm)
		{
			DataColumnMsx dcMsx = cm.SpotfireColumn;

			ColumnMapParms cp = new ColumnMapParms();
			cp.SpotfireColumnName = cm.SpotfireColumnName;

			if (Lex.IsDefined(cm.NewSpotfireColumnName)) // explicit column rename by user
			{
				cp.NewSpotfireColumnName = cm.NewSpotfireColumnName;
			}

			else
			{
				QueryColumn qc = cm.QueryColumn;
				if (qc != null && Lex.Ne(qc.ActiveLabel, qc.MetaColumn.Label)) // if different QC label assigned then use it for the Spotfire col name (e.g. for SasMap column renaming)
					cp.NewSpotfireColumnName = qc.ActiveLabel;

				else cp.NewSpotfireColumnName = cm.SpotfireColumnName;
			}

			cp.MobiusRole = cm.Role;
			cp.MobiusFileColumnName = cm.MobiusFileColumnName;
			cp.ExternalName = cm.SpotfireExternalName;

			if (dcMsx != null)
			{
				cp.DataType = dcMsx.DataType;
				cp.ContentType = dcMsx.ContentType;
			}

			return cp;
		}

	/// <summary>
	/// Remap a Spotfire DataTable to the underlying Mobius-generated data file(s)
	/// the current mapping.
	/// </summary>
	/// <returns></returns>

	public string RemapDataTable(
			DataTableMapMsx map)
		{
			foreach (ColumnMapMsx cm in map.ColumnMapList)
			{
				cm.NewSpotfireColumnName = cm.SpotfireColumnName; // make the new name the current name
				cm.SpotfireColumnName = cm.Role; // and the current name the role
			}

// Remove map items without a role

			List<ColumnMapMsx> newColumnMapList = new List<ColumnMapMsx>(); 
			foreach (ColumnMapMsx cm in map.ColumnMapList)
			{
				if (Lex.IsDefined(cm.Role)) newColumnMapList.Add(cm);
			}
			map.ColumnMapList = newColumnMapList;

			map.AddUnassignedMobiusQueryColumnsToMap();
			map.AssignUniqueSpotfireNamesFromAssociatedMobiusColumnNames();

			string result = ImportMobiusDataFile(map); // reimport the file with the new mapping 
			return result;
		}

		/// <summary>
		/// Reset the state of the view to allow execution of a new or modified source query
		/// </summary>

		public override void ResetStateForNewQueryExecution()
		{
			SpotfirePageControl = null;
			RenderingControl = null;
			ConfigureCount = 0;

			return;
		}

		/// <summary>
		/// Write the html that will open the visualization with the specified data file
		/// </summary>
		/// <param name="exportFile"></param>
		/// <returns></returns>

		void OpenSpotfireUrlDrivenVisualization_Obsolete(
			WebBrowser webBrowser)
		{
			QueryTable qt;
			QueryColumn qc;
			SpotfireViewManager v = this;

			if (webBrowser == null) DebugMx.ArgException("WebBrowser is null");

			Progress.Show("Opening visualization...");

			// Build the URL to open the analysis from the Webplayer library and pass in any required parameters.
			// Examples: 
			// https://<server>/SpotfireWeb/ViewAnalysis.aspx?file=/Mobius/Visualizations/MdbAssay_MOBIUS&configurationBlock=CorpId_LIST={1,3,5};
			// https://<server>/SpotfireWeb/ViewAnalysis.aspx?file=/Mobius/Visualizations/MdbAssay_MOBIUS&configurationBlock=CorpId_LIST={2784544};
			// https://<server>/SpotfireWeb/ViewAnalysis.aspx?file=/Mobius/Visualizations/SF_UNPIV_ASSY_RSLTS&configurationBlock=CorpId_LIST={2784544};
			// https://<server>/SpotfireWeb/ViewAnalysis.aspx?file=/Mobius/Visualizations/SF_UNPIV_ASSY_RSLTS_VW_WITH_CorpId_PARM&configurationBlock=CorpId_LIST={2784544};
			// https://<server>/SpotfireWeb/ViewAnalysis.aspx?file=/Mobius/Visualizations/SF_UNPIV_ASSY_RSLTS_VW_WITH_CorpId_PARM&configurationBlock=CorpId_LIST={2784544, 2817431};
			// https://<server>/SpotfireWeb/ViewAnalysis.aspx?file=/Mobius/Visualizations/MobiusTableViews&configurationBlock=CorpId_LIST={2784544, 2817431};

			//https://<server>/SpotfireWeb/ViewAnalysis.aspx?file=/Mobius/Visualizations/SELECT_MOBIUS_DATA_Test&configurationBlock=SQLPARMS="2784544, 2817431"; // single-parm form

			string tableName = Name; // e.g. SPOTFIRELINK_613430
			qt = Qm.Query.GetQueryTableByName(tableName);
			if (qt == null) throw new Exception("SpotfireView QueryTable not found: " + tableName);

			MetaTable mt = qt.MetaTable;
			if (mt.MetaBrokerType != MetaBrokerType.SpotfireLink) throw new Exception("MetaTable not a SpotfireLink: " + tableName);

			SpotfireViewProps sl = SpotfireViewProps.DeserializeSqlProcedureVisualization(mt);

			string url = sl.GetWebplayerUrlOfAnalysis();

			// Build configuration block to pass parameter values to Spotfire
			// Block consists of one or more parameters separated by semicolons.
			// Parameter values containing spaces need to be quoted.
			// Example: ConfigurationBlock=SQLPARMS="CIDLIST_393 ASSAYLIST_394 RESULTTYPE_395 MODELLIST_396";CIDLIST=CIDLIST_393;ASSAYLIST=ASSAYLIST_394;RESULTTYPE=RESULTTYPE_395;MODELLIST=MODELLIST_396;


			string vals = "";
			string cidListName = "";

			List<KeyValuePair<string, string>> parmList = new List<KeyValuePair<string, string>>();

			// If list is not empty write it to a table for later retrieval and pass list name (e.g. CIDLIST_123) id rather than the keys themselves
			// The stored list can be directly accessed via the Spotfire InfoLink SQL (modified) or 
			// via the the Mobius SELECT_MOBIUS_DATA(sqlName, sqlParms) Oracle function 

			if (Dtm.ResultsKeys.Count > 0)
			{
				StringBuilder sb = new StringBuilder(); // build list of keys
				for (int ki = 0; ki < Dtm.ResultsKeys.Count; ki++)
				{
					if (sb.Length > 0) sb.Append(",");
					sb.Append(Dtm.ResultsKeys[ki]);
				}
				vals = sb.ToString();

				cidListName = QueryEngine.SaveSpotfireKeyList("CIDLIST", vals);
				parmList.Add(new KeyValuePair<string, string>("CIDLIST", cidListName));
			}

			// Handle other criteria parameters specified for the Spotfire view

			BuildConfigBlockParameter("ASSAYLIST", "invitro_assays", qt, parmList);
			BuildConfigBlockParameter("RSLTTYPNM", "invitro_rslt_typ_nm", qt, parmList);
			BuildConfigBlockParameter("RSLTTYP", "invitro_rslt_typ", qt, parmList);
			BuildConfigBlockParameter("TOPLVLRSLT", "top_lvl_rslt", qt, parmList);

			BuildConfigBlockParameter("ASSAYLIST2", "invitro_assays_2", qt, parmList);
			BuildConfigBlockParameter("RSLTTYPNM2", "invitro_rslt_typ_nm_2", qt, parmList);
			BuildConfigBlockParameter("RSLTTYP2", "invitro_rslt_typ_2", qt, parmList);
			BuildConfigBlockParameter("TOPLVLRSLT2", "top_lvl_rslt_2", qt, parmList);

			BuildConfigBlockParameter("MODELLIST", "insilico_models", qt, parmList);

			// Assemble parameters/subparameters

			if (parmList.Count > 0) // any parms defined?
			{
				string p1 = ""; // accumulate bundled SQLPARMS subparameters here
				string p2 = ""; // accumulate bundled SQLPARMS2 subparameters here
				bool sqlParms2Defined = false;
				string pu = ""; // accumulate individual unbundled parms
				for (int pi = 0; pi < parmList.Count; pi++)
				{
					KeyValuePair<string, string> kv = parmList[pi];

					if (!Lex.EndsWith(kv.Key, "2")) // SQLPARMS1
					{
						if (p1 != "") p1 += " ";
						p1 += kv.Value;
					}

					if (Lex.EndsWith(kv.Key, "2") || Lex.Eq(kv.Key, "CIDLIST")) // SQLPARMS2
					{
						if (Lex.EndsWith(kv.Key, "2")) sqlParms2Defined = true;
						if (p2 != "") p2 += " ";
						p2 += kv.Value;
					}

					pu += kv.Key + "=" + kv.Value + ";"; // include semicolon at end of each parameter
				}

				p1 = "SQLPARMS=\"" + p1 + "\";";

				if (sqlParms2Defined)
					p2 = "SQLPARMS2=\"" + p2 + "\";";
				else p2 = "";

				url += "&configurationBlock=" + p1 + p2 + pu; // add the parms in the config block
			}

			// Convert url to ASCII and encode as necessary

			Encoding sourceEncoding = Encoding.GetEncoding(28591); // ISO-8859-1
			byte[] asciiBytes = Encoding.Convert(sourceEncoding, Encoding.ASCII, sourceEncoding.GetBytes(url));
			String asciiString = Encoding.UTF8.GetString(asciiBytes);

			url = asciiString;
			url = url.Replace(" ", "%20"); // replace spaces with escaped character
			url = url.Replace("\"", "%22"); // replace double quote with escaped character (seems to be needed for IE)

			//url = HttpUtility.UrlPathEncode(asciiString);
			//webBrowser.Navigate("https://www.google.com/webhp"); // debug
			//webBrowser.Navigate(sl.GetWebplayerUrl()); // debug

			webBrowser.Navigate(url);
			Progress.Hide();

			return;
		}

		/// <summary>
		/// Build a config block filter parameter for specified SPOTFIRELINK QueryTable column
		/// </summary>
		/// <param name="parmName">Spotfire Parameter Name</param>
		/// <param name="mtColName">Metacolumn name in SPOTFIRELINK_xxx metatable</param>
		/// <param name="qt"></param>
		/// <param name="parmList"></param>

		void BuildConfigBlockParameter(
			string parmName,
			 string mtColName,
			QueryTable qt,
			List<KeyValuePair<string, string>> parmList)
		{
			QueryColumn qc = qt.GetQueryColumnByName(mtColName);
			if (qc == null || Lex.IsUndefined(qc.Criteria)) return;

			string criteriaSql = ConvertMobiusCriteriaToSqlCriteria(qc);
			string listName = QueryEngine.SaveSpotfireKeyList(parmName, criteriaSql);

			parmList.Add(new KeyValuePair<string, string>(parmName, listName));

			return;
		}

		/// <summary>
		/// ConvertMobiusCriteriaToSqlCriteria
		/// </summary>
		/// <param name="qc">QueryColumn in SpotfireLink with criteria</param>
		/// <returns></returns>

		string ConvertMobiusCriteriaToSqlCriteria(
			QueryColumn qc)
		{
			bool summary;
			int tableId, pi;
			string tok;

			if (Lex.IsUndefined(qc.Criteria)) return "";

			MetaColumn mc = qc.MetaColumn;
			ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(qc.Criteria);

			string listString = "";
			bool quoteItems = !mc.IsNumeric;
			for (int li = 0; li < psc.ValueList.Count; li++)
			{
				tok = psc.ValueList[li];
				MetaTable mt = MetaTableCollection.Get(tok); // see if a metatable name
				if (mt != null)
				{
					if (mt.MetaBrokerType == MetaBrokerType.SpotfireLink && mt.MultiPivot)
					{ // remap to list of metatables
						string[] prefixes = mt.Code.Split(','); // get prefixes of tables to be included
						Query q = qc.QueryTable.Query; // query to check
						foreach (QueryTable qt0 in q.Tables)
						{
							string mtName = qt0.MetaTable.Name;
							for (pi = 0; pi < prefixes.Length; pi++) // see if metatable starts with a prefix that is to be included
							{
								if (Lex.StartsWith(mtName, prefixes[pi]))
								{
									psc.ValueList.Add(mtName);
									break;
								}
							}

						}
					}

					else
					{
						MetaTable.ParseMetaTableName(mt.Name, out tableId, out summary);
						if (listString != "") listString += ",";
						listString += tableId.ToString();
					}
				}

				else // simple value
				{
					if (!mc.IsNumeric)
						tok = Lex.AddSingleQuotes(tok);
					if (listString != "") listString += ",";
					listString += tok;
				}
			}

			return listString;

			//string sqlColName = mc.ResultCode; // SQL col name is in result code 
			//string criteriaSql = sqlColName + " in (" + listString + ")";
			//return criteriaSql;
		}

		/// <summary>
		/// Serialize
		/// </summary>
		/// <param name="tw"></param>
		/// <returns></returns>

		public override void Serialize(XmlTextWriter tw)
		{
			BeginSerialization(tw);
			EndSerialization(tw);
			return;
		}

		/// <summary>
		/// BeginSerialization
		/// </summary>
		/// <param name="tw"></param>

		public override void BeginSerialization(XmlTextWriter tw)
		{
			base.BeginSerialization(tw);

			////SpotfireLink p = SpotfireProperties;
			////if (p == null) return;

			////tw.WriteStartElement("SpotfireLink");
			////tw.WriteAttributeString("AnalysisPath", p.AnalysisPath);
			////tw.WriteAttributeString("KeyParmName", p.KeyParmName);
			////tw.WriteAttributeString("DataTableSqlId", p.DataTableSqlId.ToString());
			////tw.WriteEndElement();

			return;
		}


		/// <summary>
		/// EndSerialization
		/// </summary>
		/// <param name="tw"></param>

		public override void EndSerialization(XmlTextWriter tw)
		{
			base.EndSerialization(tw);
			return;
		}

		/// <summary>
		/// Clear any pointers to UI resources (e.g. forms, controls)
		/// </summary>

		public new void FreeResources()
		{
			RenderingControl = null;
			SpotfirePageControl = null;
			return;
		}


		MetaTable GetTemplate(string templateName)
		{
			string trellisCardTemplate = @"
	<metatable tablename='TrellisCardVisualizationTemplate' l='Trellis Card'>
		<c name='id' l='Compound Id' type='CompoundId' width='6'/>
		<c name='Structure' l='Structure' type='structure' width='40' />
            
		<c name='ColorCodeTable1' l='Horizontal Bar 1' type='Number' />
		<c name='ColorCodeTable2' l='Horizontal Bar 2' type='Number' />
		<c name='ColorCodeTable3' l='Horizontal Bar 3' type='Number' />
		<c name='ColorCodeTable4' l='Horizontal Bar 4' type='Number' />
		<c name='ColorCodeTable5' l='Horizontal Bar 5' type='Number' />
		<c name='ColorCodeTable6' l='Horizontal Bar 6' type='Number' />
            
    <c name='BarChartColumn1' l='Vertical Bar 1' type='Number' />
    <c name='BarChartColumn2' l='Vertical Bar 2' type='Number' />
    <c name='BarChartColumn3' l='Vertical Bar 3' type='Number' />
    <c name='BarChartColumn4' l='Vertical Bar 4' type='Number' />
    <c name='BarChartColumn5' l='Vertical Bar 5' type='Number' />
    <c name='BarChartColumn6' l='Vertical Bar 6' type='Number' />
	</metatable>
	";

			string mtString = trellisCardTemplate.Replace("\'", "\"");
			MetaTable mt = MetaTable.Deserialize(mtString);
			return mt;
		}

	}
}