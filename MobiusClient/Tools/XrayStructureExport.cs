using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Net;
using Microsoft.Win32;

namespace Mobius.Tools
{
	public partial class XrayStructureExport : XtraForm
	{
		QueryColumn _qcProtein, _qcDensity, _qcTarget;

		bool _xRay2Request; // request is for an XRay2 file
		string _urlFileName = ""; // filename portion of URL
		static string _lastValidatedFileName = "";
		private string _currentMetaTable;
		private string _currentPdbColumnName;

		public XrayStructureExport()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Handle a click on a link to a 3D structure & open in Vida
		/// If clicked from an XtraGrid give the user a range of options for the data
		/// to be exported. If clicked from a HTML page give limited options.
		/// </summary>
		/// <param name="mtName"></param>
		/// <param name="mcName"></param>
		/// <param name="url"></param>

		public void ClickFunction(
				string mtName,
				string mcName,
				string url)
		{

			ResultsFormatter fmtr = null;
			ResultsField rfld = null;
			//DataRowMx dr;
			StringMx sx;
			StreamWriter sw;
			int markedCount = 0;
			string densityMapUrl = null;
			string target = null;

			_currentMetaTable = mtName;
			_currentPdbColumnName = mcName;

			IncludeElectronDensityPyMol.Checked = false;

			MetaTable mt = MetaTableCollection.Get(mtName);
			if (mt == null) return;

			MetaColumn mc = mt.GetMetaColumnByName(mcName);
			if (mc == null) return;

			_xRay2Request = Lex.Contains(mtName, "XRay2"); // newer XRay2 database table?
			_urlFileName = Path.GetFileName(url); // extract file name from url

			QueryManager qm = ClickFunctions.CurrentClickQueryManager;
			if (qm != null)
			{
				Query q = qm.Query;
				ResultsFormat rf = qm.ResultsFormat;
				fmtr = qm.ResultsFormatter;

				rfld = rf.GetResultsField(mc);
				if (rfld == null) return;

				QueryTable qt = q.GetQueryTableByNameWithException(mtName);
				_qcProtein = qt.GetQueryColumnByNameWithException(mcName);

				if (_xRay2Request) // newer XRay2 database
				{
					string mapUrl = "";
					if (mcName == "ALIGNED_SPLIT_COMPLEX_URL") { mapUrl = "ALIGNED_SPLIT_MAP_URL"; }
					else if (mcName == "ALIGNED_FULL_COMPLEX_URL") { mapUrl = "ALIGNED_FULL_MAP_URL"; }
					else if (mcName == "ORIGINAL_PDB_URL") { mapUrl = "ORIGINAL_MAP_URL"; } //{ mapUrl = "ORIGINAL_MAP_URL"; }

					_qcDensity = qt.GetQueryColumnByName(mapUrl); //("ALIGNED_SPLIT_MAP_URL");
					if (_qcDensity != null && !_qcDensity.Selected) _qcDensity = null;

					_qcTarget = qt.GetQueryColumnByName("primary_gene_name_alias");
					if (_qcTarget != null && !_qcTarget.Selected) _qcTarget = null;

				}

				if (_qcDensity != null)
				{
					// if there is a density map url located in the density column, enable the PyMol CheckEdit  
					DataRowMx dr = qm.DataTable.Rows[qm.MoleculeGrid.LastMouseDownCellInfo.DataRowIndex];
					densityMapUrl = Lex.ToString(dr[_qcDensity.VoPosition]);
				}
				else
				{
					// user did not select the map column, try to retrieve the XRay1 or Xray2 density map url fromt he metatable
					densityMapUrl = _xRay2Request ? GetXray2DensityMapUrl(url, qt.MetaTable.Name, mcName) : GetXray1DensityMapUrl(url, qt.MetaTable.Name);
				}

				// default checkbox to false so user does not load electron density maps everytime, since these take
				// extra time to load.
				IncludeElectronDensityPyMol.Checked = false;

				IncludeElectronDensityPyMol.Enabled = !string.IsNullOrEmpty(densityMapUrl);

				if (_qcProtein == null && _qcDensity == null)
					throw new Exception("Neither the PDB nor the MAP column is selected in the query");

				markedCount = fmtr.MarkedRowCount;
				int unmarkedCount = fmtr.UnmarkedRowCount;

				if (markedCount == 0 || unmarkedCount == 0) // if no specific selection assume user wants single structure
				{
					ExportSingle.Checked = true;
					FileName.Text = _urlFileName;
				}
				else ExportMarked.Checked = true; // assume wants marked structures

				ExportMarked.Enabled = true;
			}

			else  // simple setup for click from HTML display
			{
				ExportSingle.Checked = true;
				ExportMarked.Enabled = false;
				FileName.Text = _urlFileName;

				densityMapUrl = GetXray2DensityMapUrl(url, _currentMetaTable, mcName);
				target = GetTarget(url, _currentMetaTable, mcName);


				////IncludeProtein.Enabled = IncludeElectronDensityEnabled = false;

				////if (Lex.Eq(mcName, "bsl_xray_cmplx_url"))
				////  IncludeProtein.Checked = true;

				////else if (Lex.Eq(mcName, "bsl_xray_edensity_url"))
				////  IncludeElectronDensityChecked = true;
			}

			if (mcName == "ALIGNED_SPLIT_MAP_URL" || mcName == "ALIGNED_FULL_MAP_URL" || mcName == "ORIGINAL_MAP_URL")  // not viewable fileds
			{
				DisablePymol();
				DisableMoe();
				ExportToFile.Enabled = ExportToFile.Checked = true;
			}

			else if (mcName == "ALIGNED_FULL_COMPLEX_URL" || mcName == "ORIGINAL_PDB_URL" || mcName == "ALIGNED_SPLIT_COMPLEX_URL") // viewable by PyMol
			{
				EnableMoe();
				EnablePymol();
				ExportToFile.Enabled = true;
				ExportToFile.Checked = false;
			}
			else //everything else should be viewable by MOE
			{
				EnableMoe();
				DisablePymol();
				ExportToFile.Enabled = true;
				ExportToFile.Checked = false;
			}

			DialogResult dlgRslt = ShowDialog(SessionManager.ActiveForm);
			if (dlgRslt == DialogResult.Cancel) return;

			bool exportSingle = ExportSingle.Checked;
			bool exportMarked = !exportSingle;
			if (!IncludeElectronDensityPyMol.Checked) densityMapUrl = null;

			if (exportMarked) // see if reasonable count if exporting marked rows
			{
				string msg;
				if (markedCount == 0)
				{
					msg = "No rows have been marked for export";
					MessageBoxMx.Show(msg, "Mobius", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}

				if (markedCount > 10)
				{
					msg =
							markedCount + " structures have been selected out export.\n" +
							"Are you sure these are the structures you want to export?";
					dlgRslt = MessageBoxMx.Show(msg, "Mobius", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					if (dlgRslt != DialogResult.Yes) return;
				}
			}

			// Export to PyMol (Schrodinger) - Collab with Paul Sprengeler and Ken Schwinn
			// Relavant Files:
			// 1. Plugin: CorpView.py (in Mobius config dir)
			// 2. Script: CorpView.pml - Runs the plugin and then passes in the .csv file (in Mobius config dir)
			//		 Expected content (note: <csv-file-name> gets replaced):
			//			import pymol
			//			pluginLocation = 'C:\Progra~1\PyMOL\PyMOL\modules\pmg_tk\startup\CorpView.py'
			//			cmd.do('run ' + pluginLocation)
			//			cmd.do('lv_create_environment')
			//			cmd.do('lv_import_mobius <csv-file-name>')
			//			cmd.do('lv_destroy_environment')
			// 3. Csv file: PyMolBatchLoad.csv - Csv file in format expected by plugin (in Mobius temp dir)

			if (ExportToPyMol.Checked)
				ExportToPyMolMethod(url, densityMapUrl, target, qm);

			else if (ExportToMOE.Checked) // Export to MOE
				ExportToMOEMethod(url, qm, fmtr, rfld);

			else ExportToFilesMethod(url, qm, fmtr, rfld);

		}

		/// <summary>
		/// ExportToPyMolMethod
		/// </summary>
		/// <param name="url"></param>
		/// <param name="qm"></param>
		void ExportToPyMolMethod(
						string url,
						string densityMapUrl,
						string target,
						QueryManager qm)
		{
			StreamWriter sw;

			string rec, msg, scriptSourceFile, pluginSourceFile, script;

			bool includeProtein = true; // always include protein
																	//bool includeElectronDensity = IncludeElectronDensityPyMol.Checked;
			if (!IncludeElectronDensityPyMol.Checked) _qcDensity = null;

			pluginSourceFile = CommonConfigInfo.MiscConfigDir + @"\CorpView.py"; // the PyMOL plugin
			CopyToReadLockedFile(pluginSourceFile, CommonConfigInfo.MiscConfigDir + @"\CorpView2.py"); // move CorpView2.py to CorpView.py
			if (!File.Exists(pluginSourceFile)) throw new Exception("Plugin file not found: " + pluginSourceFile);



			scriptSourceFile = CommonConfigInfo.MiscConfigDir + @"\CorpView.pml"; // the script that starts pymol & hands it the .csv file
			CopyToReadLockedFile(scriptSourceFile, CommonConfigInfo.MiscConfigDir + @"\CorpView2.pml");
			if (!File.Exists(scriptSourceFile)) throw new Exception("Script file not found: " + scriptSourceFile);

			string csvFile = ClientDirs.TempDir + @"\PyMolBatchLoad.csv"; // the .csv file to write
			WriteCsvFile(csvFile, ExportSingle.Checked, _qcProtein, _qcDensity, _qcTarget, qm, url, densityMapUrl, target);

			//qm.Query.Tables[0].MetaTable.Name

			try // Read the CorpView2.pml that gets edited.
			{
				StreamReader sr = new StreamReader(scriptSourceFile);
				script = sr.ReadToEnd();
				sr.Close();
			}
			catch (Exception ex)
			{ throw new Exception("Error reading " + scriptSourceFile + ": " + ex.Message); }

			// Get the path to the plugin file

			string pluginFile = null;
			Lex lex = new Lex("=");
			lex.OpenString(script);
			while (true)
			{
				string tok = lex.Get();
				if (tok == "") break;
				if (Lex.Ne(tok, "pluginLocation")) continue;

				pluginFile = lex.Get();
				if (pluginFile == "=") pluginFile = lex.Get();
				pluginFile = Lex.RemoveSingleQuotes(pluginFile);
			}

			if (pluginFile == null) throw new Exception("Can't find definition of plugin location");

			// Get the proper plugin directory

			string pluginFolder = Path.GetDirectoryName(pluginFile);
			if (!Directory.Exists(pluginFolder))
			{
				string pluginFile2 = pluginFile;
				pluginFile2 = Lex.Replace(pluginFile2, "Progra~1", "Progra~2"); // Win7 x86 folder using 8.3 names
				pluginFile2 = Lex.Replace(pluginFile2, "Program Files", "Program Files (x86)"); // Win7 x86 folder with regular names
				string pluginFolder2 = Path.GetDirectoryName(pluginFile2);
				if (!Directory.Exists(pluginFolder2))
				{
					DebugLog.Message("Can't find PyMOL plugin folder: " + pluginFolder);
					MessageBox.Show("Can't find PyMol plugin folder " + pluginFolder + ".\rContact Pymol Support team.");
					Progress.Hide();
					return;
				}
				script = Lex.Replace(script, pluginFile, pluginFile2); // substitute proper plugin file name in script
				pluginFile = pluginFile2;
			}

			bool update = false;
			bool pyMolInstalled = File.Exists(pluginFile);
			ClientLog.Message("Checking for PyMol installation...");

			if (pyMolInstalled)
			{
				DateTime dt1 = File.GetLastWriteTime(pluginSourceFile);
				DateTime dt2 = File.GetLastWriteTime(pluginFile);
				if (DateTime.Compare(dt1, dt2) > 0)
				{
					ClientLog.Message("  PyMol file is older than the Mobius version.");
					update = true;
				}
			}
			else
			{
				ClientLog.Message("  Could not find PyMol file: " + pluginFile);
				update = true;
			}

			// Be sure the plugin is up to date

			if (update)
			{
				try
				{
					File.Copy(pluginSourceFile, pluginFile, true);  // copy CorpView.py to Pymol startup dir
				}
				catch (Exception ex)
				{

					ClientLog.Message("   Error copying CorpView.py to: " + pluginFile + ", " + ex.Message);
					if (!pyMolInstalled)
					{
						MessageBox.Show("Unable to find Corp Plugin (CorpView.py) for Pymol.\rWindows 7 will not allow Mobius access to copy the plugin to your machine.\rContact Pymol Support team.");
						Progress.Hide();
						return;
					}
				}
			}

			DebugLog.Message("Error copying CorpView.py to: " + pluginFile);

			// Plug the csv file name into the script

			string csvFileNameParm = "<csv-file-name>";
			if (!Lex.Contains(script, csvFileNameParm)) throw new Exception("<csv-file-name> not found in script");
			string csvFileNameForScript = csvFile.Replace(@"\", "/"); // translate backslashes so they aren't interpreted as escapes
			script = Lex.Replace(script, csvFileNameParm, csvFileNameForScript);

			string scriptFile = ClientDirs.TempDir + @"\CorpView.pml";
			sw = new StreamWriter(scriptFile);
			sw.Write(script);
			sw.Close();

			Progress.Show("Passing data to PyMOL...");
			try
			{
				//DebugLog.Message("startFile: " + startFile);
				//DebugLog.Message("startArgs: " + startArgs);
				Process p = Process.Start(scriptFile);
			}
			catch (Exception ex)
			{
				MessageBoxMx.ShowError("Failed to start PyMOL");
				Progress.Hide();
				return;
			}

			System.Threading.Thread.Sleep(3000); // leave message up a bit while PyMOL is starting/being activated
			Progress.Hide();
		}

		/// <summary>
		/// ExportToMOE
		/// </summary>
		/// <param name="mtName"></param>
		/// <param name="mcName"></param>
		/// <param name="url"></param>

		void ExportToMOEMethod(
				string url,
				QueryManager qm,
				ResultsFormatter fmtr,
				ResultsField rfld)
		{
			List<string> fileList = ExportToFilesMethod(url, qm, fmtr, rfld); // download the files first

			string moeExecutable, moeArgs = "";

			if (!GetMoeExecutable(out moeExecutable, out moeArgs))
				return; // failed

			if (UseExistingMoeInstance.Checked) // -openfiles uses any existing instance, if not included then starts new instance
				moeArgs += " -openfiles ";

			for (int fi = 0; fi < fileList.Count; fi++)
			{
				moeArgs += Lex.AddDoubleQuotes(fileList[fi]) + " ";
			}

			try
			{
				Progress.Show("Passing data to MOE...");
				Process p = Process.Start(moeExecutable, moeArgs);
				Progress.Hide();
				return;
			}

			catch (Exception ex)
			{
				try { Progress.Hide(); } catch { }
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// Get the path to the MoeExecutable and any default args
		/// </summary>
		/// <param name="moeExecutable"></param>
		/// <param name="moeArgs"></param>

		bool GetMoeExecutable(
			out string mxp,
			out string moeArgs)
		{
			string dmxp = "";

			while (true)
			{
				mxp = moeArgs = "";

				string cl = Preferences.Get("MoeCommandLine"); // try personal preference first

				if (Lex.IsDefined(cl))
				{
					ParseMoeCommandLine(cl, out mxp, out moeArgs);
					dmxp = mxp;
					if (File.Exists(mxp)) return true;
				}

				// If no personal preference then cycle on MOE command lines until we find one that works
				// Example: "C:\Users\[userName]\AppData\Roaming\moe2018\bin\moe.exe  -setenv 'MOE=C:\Users\[userName]\AppData\Roaming\moe2018'"

				DictionaryMx md = DictionaryMx.Get("MoeCommandLines");
				if (md != null)
				{
					for (int i1 = 0; i1 < md.Words.Count; i1++)
					{
						cl = md.Words[i1];
						if (Lex.IsUndefined(cl)) continue;
						cl = cl.Replace("'", "\"");
						cl = Lex.Replace(cl, "[userName]", SS.I.UserName);
						ParseMoeCommandLine(cl, out mxp, out moeArgs);
						if (Lex.IsUndefined(dmxp)) dmxp = mxp;
						if (File.Exists(mxp)) return true;
					}
				}

				if (!GetPersonalMoeExecutableLocation(dmxp))
					return false;
			}
		}

		private void SetMoeExecutable_Click(object sender, EventArgs e)
		{
			GetPersonalMoeExecutableLocation(null);
			return;
		}

		private bool GetPersonalMoeExecutableLocation(string mxp)
		{
			string moeArgs, newCl = "";

			while (true) // loop til we get a valid executable name or cancelled
			{
				if (mxp != null)
				{
					string msg =
							"Unable to find MOE executable:\r\n\r\n" +
							"  " + mxp + "\r\n\r\n" +
							"Would you like to define a new location for the MOE executable?";
					DialogResult dr = MessageBoxMx.Show(msg, "Can't Find MOE", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
					if (dr != System.Windows.Forms.DialogResult.Yes) return false;
				}

				string cl = Preferences.Get("MoeCommandLine"); // get any personal preference first

				string txt =
						"Enter your preferred \"non-standard\" personal location for MOE executable file.\r\n" +
						"For example, the Target field from the MOE Desktop shortcut.\r\n" +
						"To use the system default executable set the value to blank.";

				newCl = InputBoxMx.Show(txt, "Set location of MOE executable file", cl);
				if (newCl == null) return false; // cancelled
				else if (newCl.Trim() == "") break; // use system default
				else
				{
					ParseMoeCommandLine(newCl, out mxp, out moeArgs);
					if (File.Exists(mxp)) break;
				}
			}

			Preferences.Set("MoeCommandLine", newCl);
			return true;
		}


		void ParseMoeCommandLine(
				string cl,
				out string mxp,
				out string moeArgs)
		{
			mxp = "";
			moeArgs = "";

			int i1 = Lex.IndexOf(cl, ".exe");
			if (i1 >= 0) i1 += 4;
			else i1 = cl.Length;

			mxp = cl.Substring(0, i1);
			if (i1 < cl.Length) moeArgs = cl.Substring(i1).Trim();
			return;
		}

		/// <summary>
		/// Export to Vida
		/// </summary>
		/// <param name="mtName"></param>
		/// <param name="mcName"></param>
		/// <param name="url"></param>

		//void ExportToVidaMethod(
		//    string url,
		//    QueryManager qm)
		//{
		//    StreamWriter sw;
		//    string scriptSourceFile = "", script;
		//    bool includeProtein = false; // IncludeProtein.Checked;
		//    if (!includeProtein) QcProtein = null;

		//    bool includeElectronDensity = false; // IncludeElectronDensityVida.Checked;
		//    if (!includeElectronDensity) QcDensity = null;

		//    if (!includeProtein && !includeElectronDensity)
		//        throw new Exception("Either the protein or the electron density must be selected for VIDA export");

		//    string csvFile = ClientDirs.TempDir + @"\VidaBatchLoad.csv";

		//    WriteCsvFile(csvFile, ExportSingle.Checked, QcProtein, QcDensity, QcTarget, qm, url);

		//    try
		//    {
		//        scriptSourceFile = CommonConfigInfo.MiscConfigDir + @"\VidaBatchLoad.py";
		//        StreamReader sr = new StreamReader(scriptSourceFile);
		//        script = sr.ReadToEnd();
		//        sr.Close();
		//    }
		//    catch (Exception ex)
		//    { throw new Exception("Error reading " + scriptSourceFile + ": " + ex.Message); }

		//    // Modify script & send to client for opening by VIDA

		//    script += "\r\nLoadMobiusExport(" + Lex.AddDoubleQuotes(csvFile) + ")\r\n";
		//    scriptSourceFile = ClientDirs.TempDir + @"\VidaBatchLoad.py";
		//    sw = new StreamWriter(scriptSourceFile);
		//    sw.Write(script);
		//    sw.Close();

		//    string vidaPaths = SS.I.ServicesIniFile.Read("VidaPaths", @"Software\Openeye\vida");
		//    scriptSourceFile = Lex.AddSingleQuotes(scriptSourceFile); // quote file name since it includes spaces
		//    string args = vidaPaths + "\t" + scriptSourceFile;

		//    Progress.Show("Passing data to VIDA...");
		//    StartVida(args);
		//    System.Threading.Thread.Sleep(3000); // leave message up a bit while VIDA is starting/being activated
		//    Progress.Hide();
		//}

		/// <summary>
		/// ExportFiles
		/// </summary>

		List<string> ExportToFilesMethod(
				string url,
				QueryManager qm,
				ResultsFormatter fmtr,
				ResultsField rfld)
		{
			List<int> markedRows;
			List<string> fileList = new List<string>();
			DataRowMx dr;
			StringMx sx;
			string clientPath, clientFolder, clientFileName;

			clientPath = FileName.Text; // path to file

			clientFolder = Path.GetDirectoryName(clientPath);
			if (Lex.IsUndefined(clientFolder) || !Directory.Exists(clientFolder))
			{
				clientFolder = Preferences.Get("DefaultExportFolder", ClientDirs.DefaultMobiusUserDocumentsFolder);
				if (Lex.IsUndefined(clientFolder) || !Directory.Exists(clientFolder))
					clientFolder = ClientDirs.TempDir;
			}

			clientFileName = Path.GetFileName(clientPath);

			if (ExportSingle.Checked) // put single row to export in table
			{
				if (Lex.IsUndefined(clientFileName)) return fileList;

				Progress.Show("Retrieving " + clientFileName + " ...");
				clientPath = clientFolder + @"\" + clientFileName;
				try
				{
					bool downloadOk = DownloadFile(url, clientPath);

					fileList.Add(clientPath);
				}
				catch (Exception ex)
				{
					throw new Exception(ex.Message, ex);
				}
				finally
				{
					Progress.Hide();
				}
			}

			else // multiple files
			{
				markedRows = fmtr.MarkedRowIndexes;


				foreach (int ri in markedRows)
				{
					if (ri < 0) url = url; // single selected url
					else
					{
						dr = qm.DataTable.Rows[ri];
						sx = dr[rfld.VoPosition] as StringMx;
						if (sx == null || sx.Hyperlink == null) continue;
						url = sx.Value; // link is in value rather than HyperLink currently
					}
					if (String.IsNullOrEmpty(url)) continue;

					clientFileName = Path.GetFileName(url);
					clientPath = clientFolder + @"\" + clientFileName;
					Progress.Show("Retrieving " + clientFileName + " ...");
					try
					{
						bool downloadOk = DownloadFile(url, clientPath);
						fileList.Add(clientPath);
					}
					catch (Exception ex)
					{
						Progress.Hide();
						throw ex;
					}

				}

				Progress.Hide();
				if (markedRows.Count > 0 && fileList.Count == 0) throw new Exception("No files downloaded");
			}

			return fileList;
		}

		/// <summary>
		/// If XRay2 database && URL is a file pointer download the file & return the downloaded file name
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>

		string DownloadFile(string url)
		{
			if (!_xRay2Request || !Lex.IsDefined(url)) return url;

			string fileName = Path.GetFileName(url);
			string clientFile = ClientDirs.TempDir + @"\" + fileName;
			Progress.Show("Retrieving " + Path.GetFileName(clientFile) + " ...");
			try
			{
				bool downloadOk = DownloadFile(url, clientFile);
			}
			catch (Exception ex)
			{
				Progress.Hide();
				throw ex;
			}

			return clientFile;
		}

		/// <summary>
		/// Download a file
		/// </summary>
		/// <param name="mtName"></param>
		/// <param name="url"></param>
		/// <param name="clientFileName"></param>
		/// <returns></returns>

		bool DownloadFile(
				string url,
				string clientFileName)
		{
			return DownloadXRayFile(url, clientFileName);
		}

		/// <summary>
		/// Download an XRay file
		/// </summary>
		/// <param name="url"></param>
		/// <param name="clientFileName"></param>
		/// <returns></returns>

		bool DownloadXRayFile(
				string url,
				string clientFileName)
		{
			byte[] ba = GetXRayFileContent(url);
			if (ba == null) return false;

			FileStream fw = new FileStream(clientFileName, FileMode.Create, FileAccess.Write);
			if (fw == null) return false;
			fw.Write(ba, 0, ba.Length);
			fw.Close();

			return true;
		}

		public static byte[] GetXRayFileContent(string urlOrFilePath)
		{
			return null; // todo
		}

		/// <summary>
		/// Write Csv file of data
		/// </summary>
		/// <param name="csvFile"></param>
		/// <param name="includeProtein"></param>
		/// <param name="includeElectronDensity"></param>

		void WriteCsvFile(
			string csvFile,
			bool exportSingle,
			QueryColumn qcProtein,
			QueryColumn qcDensity,
			QueryColumn qcTarget,
			QueryManager qm,
			string url,
			string densityMapUrl,
			string target)
		{
			string rec;
			DataRowMx dr;
			StringMx sx;
			List<int> markedRows;
			//string finalUrl;

			StreamWriter sw = new StreamWriter(csvFile);
			rec = "BSL_XRAY_CMPLX_URL,BSL_XRAY_EDENSITY_URL,TRGT_LABEL";
			sw.WriteLine(rec); // header

			string filePath = DownloadFile(url);
			string mapFilePath = DownloadFile(densityMapUrl);

			if (qm == null) // export from HTML
			{
				rec = "";
				//if (!Lex.Contains(url, "edensity")) // assume protein/ligand
				rec += filePath;

				rec += ",";

				if (mapFilePath != null)
				{
					rec += mapFilePath;
				}

				rec += ","; // nothing for target for now

				if (target != null) rec += target;

				sw.WriteLine(rec);
			}

			else // export from XtraGrid
			{
				ResultsFormatter fmtr = qm.ResultsFormatter;

				if (exportSingle) // put single row to export in table
				{
					markedRows = new List<int>();
					CellInfo cinf = qm.MoleculeGrid.LastMouseDownCellInfo;
					markedRows.Add(cinf.DataRowIndex); // indicate current row
				}

				else markedRows = fmtr.MarkedRowIndexes;

				foreach (int ri in markedRows)
				{
					dr = qm.DataTable.Rows[ri];
					rec = "";
					if (qcProtein != null)
					{
						url = Lex.ToString(dr[qcProtein.VoPosition]);
						filePath = DownloadFile(url);
						if (String.IsNullOrEmpty(filePath)) continue;
						rec += filePath;
					}

					rec += ",";

					if (qcDensity != null)
					{
						string mapUrl = Lex.ToString(dr[qcDensity.VoPosition]);
						mapFilePath = DownloadFile(mapUrl);
						rec += mapFilePath;
					}
					else if (!IncludeElectronDensityPyMol.Checked)
					{
						// do nothing, user does not want map
					}
					else if (densityMapUrl == null && !exportSingle)
					{
						densityMapUrl = _xRay2Request ? GetXray2DensityMapUrl(url, _currentMetaTable, _currentPdbColumnName) : GetXray1DensityMapUrl(url, _currentMetaTable);
						mapFilePath = DownloadFile(densityMapUrl);
						rec += mapFilePath;
					}
					else if (densityMapUrl != null)
					{
						mapFilePath = DownloadFile(densityMapUrl);
						rec += mapFilePath;
					}

					rec += ",";

					if (qcTarget != null)
					{
						target = Lex.ToString(dr[qcTarget.VoPosition]);
						rec += target;
					}

					densityMapUrl = null; //we only get the map from the first record

					sw.WriteLine(rec);
				}
			}

			sw.Close();
			return;
		}

		/// <summary>
		/// File fix for read-locked files - Delete old sourceFile, move replacement to source
		/// (replace the *2.* files with the *.* files later)
		/// </summary>
		/// <param name="replacementFile"></param>
		/// <param name="sourceFile"></param>

		void CopyToReadLockedFile(string sourceFile, string replacementFile)
		{
			if (!File.Exists(replacementFile)) return;

			if (File.Exists(sourceFile))
				try
				{
					System.IO.File.SetAttributes(sourceFile, FileAttributes.Normal); // set attributes to normal in case file is read only
					System.IO.File.Delete(sourceFile);
				}
				catch (Exception ex) { }

			try { File.Move(replacementFile, sourceFile); }
			catch (Exception ex) { }

			return;
		}

		/// <summary>
		/// Get value for one of the link fields (not used)
		/// </summary>
		/// <param name="tupleRow"></param>
		/// <param name="tupleCol"></param>
		/// <returns></returns>

		string GetLinkFieldVal(
				object val)
		{
			string url;

			if (val == null) return null;
			else if (val is string) url = (string)val;
			else if (val is Hyperlink)
			{
				url = ((Hyperlink)val).URI;
				if (String.IsNullOrEmpty(url)) url = ((Hyperlink)val).Text;
			}

			else throw new Exception("Unexpected XrayStructureExport field type: " + val.GetType());
			return url;
		}

		private void OKButton_Click(object sender, EventArgs e)
		{
			if (ExportToFile.Checked)
			{
				if (ExportSingle.Checked)
				{
					string fileName = FileName.Text;
					if (Directory.Exists(fileName))
					{
						MessageBoxMx.Show("\"" + fileName + "\" appears to be a folder name rather than a file name", "Mobius",
								MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						FileName.Focus();
						return;
					}
					string fullFileName = UIMisc.CheckFileName(2, FileName, _lastValidatedFileName, ClientDirs.DefaultMobiusUserDocumentsFolder, ".pdb");
					if (fullFileName == "") return;
					FileName.Text = fullFileName;
				}

				else
				{
					string folder = FileName.Text;
					if (String.IsNullOrEmpty(folder)) folder = ClientDirs.DefaultMobiusUserDocumentsFolder;
					folder = Path.GetDirectoryName(folder);
					if (!Directory.Exists(folder))
					{
						MessageBoxMx.Show("Folder does not exist: " + folder, "Mobius",
								MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						FileName.Focus();
						return;
					}
				}
			}

			DialogResult = DialogResult.OK;

			//UIMisc.BringActiveFormToFront(); // after showdialog be sure active form is on top
		}

		private void CancelButton_Click(object sender, EventArgs e)
		{
			this.Hide();
			UIMisc.BringActiveFormToFront(); // after showdialog be sure active form is on top
		}

		private void SetupXrayStructureExport_FormClosing(object sender, FormClosingEventArgs e)
		{
			return;
		}

		private void Browse_Click(object sender, EventArgs e)
		{
			string ClientDefaultDir = ClientDirs.DefaultMobiusUserDocumentsFolder;

			ExportToFile.Checked = true;
			if (ExportSingle.Checked) // get filename
			{
				string filter = "Files (*.pdb; *.map)|*.pdb; *.map|All files (*.*)|*.*";
				string fileName = UIMisc.GetSaveAsFilename("File Name", FileName.Text, filter, ".pdb");
				if (fileName == "") return;
				FileName.Text = fileName;
				_lastValidatedFileName = fileName;
			}

			else // get folder
			{
				FolderBrowserDialog fbd = new FolderBrowserDialog();
				fbd.Description = "Folder for downloaded files";
				string folder = FileName.Text;
				if (folder.Contains(".")) folder = Path.GetDirectoryName(folder);
				if (String.IsNullOrEmpty(folder)) folder = ClientDirs.DefaultMobiusUserDocumentsFolder;
				fbd.SelectedPath = folder;
				DialogResult dr = fbd.ShowDialog();
				if (dr == DialogResult.OK)
				{
					string path = fbd.SelectedPath;
					if (!path.EndsWith("\\"))
					{
						path += "\\";
						FileName.Text = path;
					}

				}

			}
		}

		private void ExportToVida_CheckedChanged(object sender, EventArgs e)
		{
			if (ExportToMOE.Checked) ExportToFile.Checked = false;
		}

		private void ExportToFile_CheckedChanged(object sender, EventArgs e)
		{
			if (ExportToFile.Checked) ExportToMOE.Checked = false;
		}

		private void XrayStructureExportForm_Activated(object sender, EventArgs e)
		{
			if (ExportMarked.Checked && (FileName.Text == "" || FileName.Text.Contains(".")))
				FileName.Text = ClientDirs.DefaultMobiusUserDocumentsFolder;

			OKButton.Focus();
		}

		/// <summary>
		/// Startup Vida with supplied arguments
		/// </summary>
		/// <param name="args"></param>

		public void StartVida(
				string args)
		{
			RegistryKey key = null;
			string rootKeyName = null, version = null, installPath = null, msg;

			string[] sa = args.Split('\t');

			string[] vidaPaths = sa[0].Split(';'); // array of paths to try
			string startArgs = sa[1]; // args to pass to Vida

			for (int pi = 0; pi < vidaPaths.Length; pi++)
			{
				string path = vidaPaths[pi].Trim();
				if (path == "") continue;

				else if (!String.IsNullOrEmpty(Path.GetDirectoryName(path))) // directory name
				{
					if (Directory.Exists(path))
					{
						installPath = path;
						//DebugLog.Message("StartVida installPath: " + installPath);
						break;
					}
				}

				else // registry key
				{
					string keyName = path;
					key = Registry.CurrentUser.OpenSubKey(keyName);
					if (key != null)
					{
						rootKeyName = keyName;
						version = (string)key.GetValue("version");
						if (!String.IsNullOrEmpty(version))
						{
							key = Registry.CurrentUser.OpenSubKey(rootKeyName + @"\" + version);
							if (key != null)
							{
								installPath = (string)key.GetValue("InstallPath");
								//DebugLog.Message("StartVida key: " + rootKeyName + @"\" + version + ", installPath: " + installPath);
								break;
							}
						}
						break;
					}
				}

			}

			if (String.IsNullOrEmpty(installPath))
			{
				msg = "Mobius is unable to locate a VIDA installation on this machine.";
				MessageBoxMx.Show(msg, "Mobius", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			string startFile = installPath + @"\vida.bat";

			try
			{
				//DebugLog.Message("startFile: " + startFile);
				//DebugLog.Message("startArgs: " + startArgs);
				Process p = Process.Start(startFile, startArgs);
			}
			catch (Exception ex)
			{
				MessageBoxMx.Show("Failed to start VIDA: " + startFile + " " + startArgs + "\n" + ex.Message, "Mobius");
				return;
			}

			return;
		}

		private void label3_Click(object sender, EventArgs e)
		{

		}

		private void ExportSingle_CheckedChanged(object sender, EventArgs e)
		{
			if (ExportSingle.Checked)
			{
				FileFolderLabel.Text = "   File name:";
				FileName.Text = _urlFileName;
			}

			else // multiple, just folder
			{
				FileFolderLabel.Text = "Folder name:";
			}
		}

		public string GetXray1DensityMapUrl(string pdbUrl, string tableName)
		{
			string mapUrl = null;
			string mql = "select bsl_xray_edensity_url from " + tableName + " where BSL_XRAY_CMPLX_URL = '" + pdbUrl + "'";

			Query query = MqlUtil.ConvertMqlToQuery(mql);
			query.SingleStepExecution = true;
			QueryEngine qe = new QueryEngine();

			qe.ExecuteQuery(query);

			object[] vo = qe.NextRow();

			if (vo != null)
			{
				if (vo[2] != null) mapUrl = vo[2].ToString();
			}
			return mapUrl;
		}

		public string GetXray2DensityMapUrl(string pdbUrl, string tableName, string pdbColumnName)
		{
			string mapUrl = null;
			string mapColumn = null;

			if (pdbColumnName == "ALIGNED_SPLIT_COMPLEX_URL") { mapColumn = "ALIGNED_SPLIT_MAP_URL"; }
			else if (pdbColumnName == "ALIGNED_FULL_COMPLEX_URL") { mapColumn = "ALIGNED_FULL_MAP_URL"; }
			else if (pdbColumnName == "ORIGINAL_PDB_URL") { mapColumn = "ORIGINAL_MAP_URL"; }

			if (mapColumn == null) return null;

			string mql = "select " + mapColumn + " from " + tableName + " where " + pdbColumnName + " = '" + pdbUrl + "'";

			Query query = MqlUtil.ConvertMqlToQuery(mql);
			query.SingleStepExecution = true;
			QueryEngine qe = new QueryEngine();

			qe.ExecuteQuery(query);

			object[] vo = qe.NextRow();

			if (vo != null)
			{
				if (vo[2] != null) mapUrl = vo[2].ToString();
			}
			return mapUrl;
		}

		public string GetTarget(string pdbUrl, string tableName, string pdbColumnName)
		{
			string target = null;

			string mql = "select primary_gene_name_alias from " + tableName + " where " + pdbColumnName + " = '" + pdbUrl + "'";

			Query query = MqlUtil.ConvertMqlToQuery(mql);
			query.SingleStepExecution = true;
			QueryEngine qe = new QueryEngine();

			qe.ExecuteQuery(query);

			object[] vo = qe.NextRow();

			if (vo != null)
			{
				if (vo[2] != null) target = vo[2].ToString();
			}

			return target;
		}

		private void EnableMoe()
		{
			ToggleMoe(true);
		}

		private void DisableMoe()
		{
			ToggleMoe(false);
		}

		private void ToggleMoe(bool b)
		{
			ExportToMOE.Enabled =
			ExportToMOE.Checked =
			UseExistingMoeInstance.Enabled = b;
		}

		private void EnablePymol()
		{
			TogglePyMol(true);
		}

		private void DisablePymol()
		{
			TogglePyMol(false);
		}

		private void TogglePyMol(bool b)
		{
			ExportToPyMol.Enabled =
			ExportToPyMol.Checked =
			IncludeElectronDensityPyMol.Enabled = b;
		}

	}
}
