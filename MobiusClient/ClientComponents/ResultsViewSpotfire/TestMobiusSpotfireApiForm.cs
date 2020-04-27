
using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;
using Mobius.SpotfireClient.ComAutomation;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;


using DevExpress.XtraEditors;
using DevExpress.Data;

using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Mobius.SpotfireClient
{

	/// <summary>
	/// Mobius Spotfire Api test form
	/// </summary>

	public partial class TestMobiusSpotfireApiForm : XtraForm
	{

		Query Query; // test query

		QueryManager Qm; // test qm

		AnalysisApplicationMsx App;
			
		SpotfireViewManager SVM; // view manager associated with this dialog

		SpotfireApiClient Api;  // Spotfire client Api instance

		SpotfireViewProps SVP => SVM?.SVP;

		DataTableMapsMsx DataTableMaps => SVP?.DataTableMaps; // associated DataMap

		DataTableMapMsx CurrentMap => DataTableMaps?.CurrentMap; // current DataTableMap

		AnalysisApplicationMsx Analysis => SVP?.AnalysisApp;

		DocumentMsx Doc => Analysis?.Document;

		VisualMsx ActiveVisual => Doc?.ActiveVisualReference;

		string DefaultAnalyisFilePath = @"C:\Downloads\" + SpotfireApiClient.DefaultAnalysisName + ".dxp";
		string DefaultAnalysisLibraryPath = "/Mobius/Visualizations/" + SpotfireApiClient.DefaultAnalysisName;

		MessageSnatcher MessageSnatcher; // to catch rt-click within webbrowser

		Size InitialFormSize;

		public TestMobiusSpotfireApiForm()
		{
			this.InitializeComponent();

			InitialFormSize = Size;

			PreferencesDialog.SetLookAndFeel("Blue");

			MessageSnatcher = new MessageSnatcher(WebBrowserContainerPanel);
			MessageSnatcher.RightMouseClickOccured += BrowserControlRightMouseClickOccured;

			Mobius.ServiceFacade.ServiceFacade.UseRemoteServices = false;
			Mobius.ServiceFacade.UalUtil.Initialize(ServicesIniFile.IniFilePath);

			DataTableMapsMsx.DevMode = true;
			SessionManager.DoFoundationDependencyInjections();

			return;
		}

		private void StartAnalystButton_Click(object sender, EventArgs e)
		{
			if (Api != null) return;

			SpotfireApiClient.UseAnalystClient = true;

			StatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((
				System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Bottom | 
				System.Windows.Forms.AnchorStyles.Left)	| System.Windows.Forms.AnchorStyles.Right)));

			int newHeight = StatusTextBox.Bottom - WebBrowserContainerPanel.Top; // make status box bigger
			StatusTextBox.Top = WebBrowserContainerPanel.Top; 
			StatusTextBox.Height = newHeight;

			WebBrowserContainerPanel.Visible = false;

			StatusTextBox.Text += "Waiting for TIBCO Spotfire to start...\r\n";
			Api = new SpotfireApiClient();
			Api.StartDxpProcess();
			StatusTextBox.Text += "Spotfire started\r\n";

			CommandPanel.Enabled = true;
			StartAnalystButton.Text = "Using Analyst";
			StartWebPlayerButton.Visible = false;
			return;
		}

		private void StartWebPlayerButton_Click(object sender, EventArgs e)
		{
			if (Api != null) return;

			SpotfireApiClient.UseWebPlayerClient = true;

			Api = new SpotfireApiClient();
			Api.InitializeWebBrowserControl(WebBrowser, this, StatusTextBox);

			CommandPanel.Enabled = true;
			StartWebPlayerButton.Text = "Using WebPlayer";
			StartAnalystButton.Visible = false;

			return;
		}

		private void OpenDefaultDocButton_Click(object sender, EventArgs e)
		{
			string analysisPath = null;

			if (Api == null) throw new Exception("Must start Analyst or WebPlayer first");

			if (SVM == null) SetupViewEnvironment();

			if (SpotfireApiClient.UseWebPlayerClient)
				analysisPath = DefaultAnalysisLibraryPath;

			else
				analysisPath = DefaultAnalyisFilePath;

			OpenDocument(analysisPath);
			return;
		}

		private void OpenDocumentButton_Click(object sender, EventArgs e)
		{
			string analysisPath = null;

			if (Api == null) throw new Exception("Must start Analyst or WebPlayer first");

			if (SVM == null) SetupViewEnvironment();

			if (SpotfireApiClient.UseWebPlayerClient)
			{
				string title = "Select Spotfire Analysis";
				string prompt = "Enter the name of the library analysis to open (e.g. /Mobius/Visualizations/MyAnalysisTemplate";
				string defaultText = SpotfireApiClient.DefaultAnalysisName;

				analysisPath = InputBoxMx.Show(prompt, title, defaultText);
				if (Lex.IsUndefined(analysisPath)) return;
			}

			else
			{
				if (String.IsNullOrWhiteSpace(OpenFileDialog.FileName))
					OpenFileDialog.FileName = DefaultAnalyisFilePath;

				if (this.OpenFileDialog.ShowDialog() != DialogResult.OK) return;

				analysisPath = this.OpenFileDialog.FileName;
			}

			OpenDocument(analysisPath);
			return;
		}


		void SetupViewEnvironment()
		{
			ResultsViewModel rvi = new ResultsViewModel();
			rvi.ViewType = ResultsViewType.Spotfire;

			SVM = new SpotfireViewManager(rvi);
			SVM.SpotfireViewProps.AnalysisPath = SpotfireApiClient.DefaultAnalysisName;
			SVM.SpotfireApiClient = Api;
			VisPropsButton.Enabled = true;

			Query = Query.DeserializeFromFile(SpotfireApiClient.DefaultSerializedTestQuery);
			SVM.BaseQuery = Query;

			Qm = new QueryManager();
			Qm.LinkMember(Query);
			SVM.Qm = Qm;

			return;
		}


		void OpenDocument(string path)
		{
			StatusTextBox.Text += "Opening document: " + path + "\r\n";

			DelayedCallback.Schedule(OpenDocumentDelayed, path); // schedule callback

			return;
		}


	void OpenDocumentDelayed(object state)
		{
			string path = state as string;

			try
			{
				App = Api.OpenAnalysis(path); 
				if (App == null) throw new Exception("Openanalysis " + path + " failed");
				string msg = path;
				StatusTextBox.Text += "Document open: " + msg + "\r\n";

				//CheckForNewWebplayerSession(); // debug

				SVP.AnalysisApp = App;
				App.Container = SVM;

				SVP.DataTableMaps = new DataTableMapsMsx(SVP); // Allocate the maps
				SVP.DataTableMaps.AssignInitialMapping();

				//DoInitialMergeMobiusDataFilesAndRemapDataTables(); 
			}

			catch (Exception ex)
			{
				string errMsg = DebugLog.FormatExceptionMessage(ex);
				StatusTextBox.Text += errMsg + "\r\n";
			}

			return;
		}

		private void BrowserControlRightMouseClickOccured(object sender, EventArgs e)
		{
			DelayedCallback.Schedule(ShowVisContextMenu, e, 200); // schedule callback
			return;
		}

		private void ShowVisContextMenu(object state)
		{
			Point p = WindowsHelper.GetMousePosition();
			WebBrowserRtClickContextMenu.Show(p);

			return;
		}

		/// <summary>
		/// Check to be sure we have a new webplayer session and are not connected to an existing session / document
		/// Note that this problem can be avoided with the following code:
		/// 
		///   if (Document.Properties.AllowWebPlayerResume) // this needs to be false to prevent reusing modified documented cached by the webplayer
		///		 Document.Properties.AllowWebPlayerResume = false;
		///		 
		/// </summary>

		void CheckForNewWebplayerSession()
		{
			string mobiusWebPlayerSession = Api.SetDocumentProperty("MobiusWebPlayerSession", DateTime.Now.ToString());

			if (Lex.IsDefined(mobiusWebPlayerSession))
			{
				string msg = "Existing MobiusWebPlayerSession: " + mobiusWebPlayerSession;
				//MessageBoxMx.ShowError(msg);
				UIMisc.Beep();
			}
		}

	/// <summary>
	/// DoInitialMergeMobiusDataFileAndRemapDataTable
	/// </summary>
	/// <param name="state"></param>

	void DoInitialMergeMobiusDataFilesAndRemapDataTables()
		{
			SVM.ImportMobiusDataFiles(); // do initial load of Mobius data into Spotfire
			//SVM.MergeMobiusDataFilesAndRemapDataTables(); 

			DialogResult dr = DataMapDialog.ShowDialog(SVM);

			return;
		}

		/// <summary>
		/// InitializeMapForDev
		/// </summary>
		/// <param name="dm"></param>

		void InitializeMapForDev(DataTableMapMsx dm)
		{
			QueryTable qt = null;
			QueryColumn qc = null;
			string alias = "";
			string colName = "";

			if (dm.ColumnMapCollection != null && dm.ColumnMapCollection.Count > 0) return;

			dm.ColumnMapCollection = new ColumnMapCollection();

			DataTableMsx dt = SVP.Doc.DataManager.TableCollection[0];
			dm.SpotfireDataTable = dt;
			
			foreach (DataColumnMsx dc in dt.Columns)
			{
				colName = dc.Name;
				int i1 = colName.IndexOf(".");
				if (i1 <= 0 || i1 + 1 >= colName.Length) // name without alias
					alias = "T1";

				else
				{
					alias = colName.Substring(0, i1);
					colName = colName.Substring(i1 + 1);
				}

				int qti = Query.GetQueryTableIndexByAlias(alias);
				if (qti < 0) continue;

				qt = Query.Tables[qti];

				qc = qt.GetQueryColumnByLabel(colName);
				if (qc == null) continue;

				ColumnMapMsx cm = new ColumnMapMsx();
				cm.QueryColumn = qc;

				cm.SpotfireColumn = dc;

				cm.SpotfireColumnName = dc.Name;
				cm.Role = dc.Name;

				dm.ColumnMapCollection.Add(cm);
			}

			return;
		}

		private void RunScriptButton_Click(object sender, EventArgs e)
		{
			Stopwatch sw = Stopwatch.StartNew();

			//for (int i1 = 0; i1 < 1000; i1++)
			//{
			// Set directly with C# code in automation server
			// Note that is is 20X faster than doing with script 
			// i.e. .7ms/direct C# call versus 13 ms/call for IronPython script: 
			//  --- Document.Data.Tables.DefaultTableReference.Name = "New Name";
			//
			// this.viewWrapper.SetDefaultTableName("NoScript Title"); 
			//}

			string script = InputTextBox.Text;
			if (String.IsNullOrWhiteSpace(script))
			{
				MessageBox.Show("A script must be defined");
				return;
			}

			string result = Api.CCW.SCC.RunScript(script);
			long ms = sw.ElapsedMilliseconds;
			StatusTextBox.Text += "Response: " + result + "\r\nTime: " + ms + " ms.";
			return;
		}

		private void CloseDocumentButton_Click(object sender, EventArgs e)
		{
			Api.CloseAnalysis();
			return;
		}

		private void ExitDxpButton_Click(object sender, EventArgs e)
		{
			Api.CCW.SCC.ExitDxp();
		}

		public class SpotfireComCallback : SpotfireComCallbackBase
		{
			private TestMobiusSpotfireApiForm TestForm;

			public SpotfireComCallback(TestMobiusSpotfireApiForm testFormArg)
			{
				this.TestForm = testFormArg;
			}

			public override void Exited(ExitContextWrapper context)
			{
				base.Exited(context);

				// The call to this method is made on a thread that is allocated by  the COM 
				// interop framework. To set a text in the Form we must switch to the Form thread.
				// By doing this with an asynchronous BeginInvoke, this method returns immediately
				// an thus does the COM call from TIBCO Spotfire which allows TIBCO Spotfire to continue execution.                

				TestForm.BeginInvoke(
						new MethodInvoker(
								delegate
								{
									TestForm.StatusTextBox.Text += "TIBCO Spotfire exited.";
									TestForm.StartAnalystButton.Enabled = true;
									TestForm.StartWebPlayerButton.Visible = true;
								}));
			}

			public override void Started(StartContextWrapper context)
			{
				base.Started(context);

				// The call to this method is made on a thread that is allocated by  the COM 
				// interop framework. To set a text in the Form we must switch to the Form thread.
				// By doing this with an asynchronous BeginInvoke, this method returns immediately
				// an thus does the COM call from TIBCO Spotfire which allows TIBCO Spotfire to continue execution.                
				this.TestForm.BeginInvoke(
						new MethodInvoker(delegate { this.TestForm.StatusTextBox.Text += "TIBCO Spotfire started."; }));
			}

			public override void OnStatusChanged(string status)
			{
				// The call to this method is made on a thread that is allocated by  the COM 
				// interop framework. To set a text in the Form we must switch to the Form thread.
				// By doing this with an asynchronous BeginInvoke, this method returns immediately
				// an thus does the COM call from TIBCO Spotfire which allows TIBCO Spotfire to continue execution.                
				this.TestForm.BeginInvoke(
						new MethodInvoker(delegate { this.TestForm.StatusTextBox.Text += status; }));
			}

		}

		/// <summary>
		/// Get active visual properties
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void GetVisPropsButton_Click(object sender, EventArgs e)
		{
			Stopwatch sw = Stopwatch.StartNew();

			string serializedVisual = Api.GetVisualProperties("<activeVisual>");
			if (Lex.IsUndefined(serializedVisual)) return;

			VisualMsx vixualMsx = VisualMsx.Deserialize(serializedVisual, Analysis);

			long ms = sw.ElapsedMilliseconds;
			StatusTextBox.Text += "Response: " + serializedVisual + "\r\nTime: " + ms + " ms.";
			return;
		}

		/// <summary>
		/// Set active visual properties
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void SetVisPropsButton_Click_Unused(object sender, EventArgs e)
		{
			Stopwatch sw = Stopwatch.StartNew();

			string props = InputTextBox.Text;
			if (String.IsNullOrWhiteSpace(props))
			{
				MessageBox.Show("Input must be defined");
				return;
			}

			string result = Api.SetVisualProperties("<activeVisual>", props);
			long ms = sw.ElapsedMilliseconds;
			StatusTextBox.Text += "Response: " + result + "\r\nTime: " + ms + " ms.";
			return;
		}

		/// <summary>
		/// Rename the columns in the data table(s) to those of the associated Mobius MetaColumns names
		/// based on a match of the MetaTable.MetaColumn names stored in the DataColumn ExternalName property
		/// when the DataTable was (re)loaded from a file.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void UseMxColNamesButton_Click(object sender, EventArgs e)
		{
			string result = Api.RenameColumnsToMobiusNames();
			return;
		}

		private void GetDocumentButton_Click(object sender, EventArgs e)
		{
			DocumentMsx doc = Api.GetDocument();
			return;
		}

		private void GetDataTablesButton_Click(object sender, EventArgs e)
		{
			DataTableCollectionMsx dataTableCollection = Api.GetDataTableCollection();
			return;
		}

		private void GetBuildDateTimeButton_Click(object sender, EventArgs e)
		{
			string result = Api.GetApiServerInstanceContextInfo();
			StatusTextBox.Text += "Api server instance context info: " + result + "\r\n";
		}

		private void ClearStatusButton_Click(object sender, EventArgs e)
		{
			StatusTextBox.Text += "";
		}

		private void WebBrowser_Resize(object sender, EventArgs e)
		{
			if (Api?.SWPC != null)
				Api.SWPC.InvokeScriptMethod("resize"); // Resize the WebPlayer div to fill available area
		}

		private void VisPropsButton_Click(object sender, EventArgs e)
		{
			ShowVisualPropertiesDialog();
		}

		private void VisualPropertiesMenuItem_Click(object sender, EventArgs e)
		{
			ShowVisualPropertiesDialog();
		}

		private void ShowVisualPropertiesDialog()
		{
			VisualMsx v = Api.GetActiveVisual();

			SpotfireToolbar.EditVisualProperties(v, SVP);

			return;
		}

		private void DataButton_Click(object sender, EventArgs e)
		{
			ShowDataDialog();
		}

		private void DataMenuItem_Click(object sender, EventArgs e)
		{
			ShowDataDialog();
		}

		private void ShowDataDialog()
		{
			SpotfireToolbar.EditDataProperties(SVP);
			return;
		}

		private void InsertScriptButton_Click(object sender, EventArgs e)
		{
			string fileName = @"<MiscConfigDir>\MobiusSpotfireApi.IronPythonScript.txt";
			string script = new ServerFile().ReadAll(fileName);

			string args = "";
			Api.ConfigureAnalysisForMobiusUse(args);

			Clipboard.SetText(script); // put script on clipboard

			string msg =
@"The necessary Document Properties have been created. 

You must manually create a script named MobiusSpotfireApi, insert the
text currently on the clipboard into the script and save it.

Then, associate this script with the MobiusSpotfireApiCall so that the script
is called whenever the the property is changed.

Finally, save the analysis to the Spotfire Libary. You can then add this Spotfire 
analysis to any Mobius query from the Query Results page using the 
Add View > Select Spotfire analysis from library command.";

			MessageBoxMx.Show(msg);

			return;
		}
	}

}
