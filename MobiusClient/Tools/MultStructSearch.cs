using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.Data;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.Tools
{
	public partial class MultStructSearch : XtraForm
	{
		internal ParsedStructureCriteria Psc = new ParsedStructureCriteria();
		internal UserObject SavedListUo; // user object for selected saved list
		internal string StructureFile; // name of SDfile or Smiles file
		internal string keyField;
		internal UserObject ResultsUo; // results annotation table
		internal bool RetrieveStructures;
		internal StreamWriter LogStream;

		internal static int SearchCount = 0;

		public MultStructSearch()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Main tool entry point
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public string Run(
			string args)
		{
			QbUtil.SetMode(QueryMode.Build); // be sure in build mode

			//List<StructureDbInfo> dbInfo = StructureDbInfo.GetList(); // get list of root structure tables
			//MetaTable mt = MetaTableCollection.Get(dbInfo[0].StructureTable); // use first structure table

			if (DatabasesToSearch.Text == "")
			{ // default to first structure table
				List<RootTable> dbInfo = RootTable.GetList(); // get list of root structure tables
				if (dbInfo != null && dbInfo.Count > 0)
					DatabasesToSearch.Text = dbInfo[0].Label;
			}

			if (ServicesIniFile.Read("MultStructSearchHelpUrl") != "")
				Help.Enabled = true;

			DialogResult dr = ShowDialog(SessionManager.ActiveForm);

			return "";
		}

// Databases

		private void DatabasesToSearch_Click(object sender, EventArgs e)
		{
			DatabasesToSearch_KeyDown(null, null);
		}

		private void DatabasesToSearch_KeyDown(object sender, KeyEventArgs e)
		{
			BrowseDatabases.Focus();
			BrowseDatabases_Click(null, null);
		}

		private void BrowseDatabases_Click(object sender, EventArgs e)
		{
			string prompt = "Databases to search";
			string response = ToolHelper.GetCheckedListBoxDialog("Structure_Databases", DatabasesToSearch.Text, prompt);
			if (response != null)
			{
				DatabasesToSearch.Text = response; // get new list
			}
		}

// Search Types

		private void SubStruct_CheckedChanged(object sender, EventArgs e)
		{
			return;
		}

		private void Full_CheckedChanged(object sender, EventArgs e)
		{
			return;
		}

		private void FSOptions_Click(object sender, EventArgs e)
		{
			Full.Checked = true;
			CriteriaStructureFSOptions.Edit(Psc);
		}

		private void Similarity_CheckedChanged(object sender, EventArgs e)
		{
			return;
		}

		private void SimOptions_Click(object sender, EventArgs e)
		{
			Similarity.Checked = true;
			CriteriaStructureSimOptions.Edit(Psc);
			return;
		}

		// ListName

		private void ListName_Click(object sender, EventArgs e)
		{
			ListName_KeyDown(null, null);
		}

		private void ListName_KeyDown(object sender, KeyEventArgs e)
		{
			BrowseList.Focus();
			BrowseList_Click(null, null);
		}

		private void BrowseList_Click(object sender, EventArgs e)
		{
			FromList.Checked = true;
			UserObject uo = UserObjectOpenDialog.ShowDialog(UserObjectType.CnList, "Browse Lists", SavedListUo);
			if (uo != null)
			{
				ListName.Text = uo.Name;
				string txt = uo.InternalName;
				SavedListUo = uo;
			}
		}

		// FileName

		private void FileName_MouseDown(object sender, MouseEventArgs e)
		{
			FromFile.Checked = true;
		}

		private void BrowseSDFile_Click(object sender, EventArgs e)
		{
			FromFile.Checked = true;
			string filter = "Files (*.sdf; *.smi)|*.sdf; *.smi|All files (*.*)|*.*";
			string fileName = UIMisc.GetOpenFilename("File Name", FileName.Text, filter, ".sdf");
			if (fileName == "") return;
			FileName.Text = fileName;
		}

		private void KeyField_MouseDown(object sender, MouseEventArgs e)
		{
			FromFile.Checked = true;
		}

		private void BrowseResults_Click(object sender, EventArgs e)
		{
			string tok = ResultsName.Text.Trim();
			if (tok != "")
			{
				if (ResultsUo != null) ResultsUo.Name = tok;
				else ResultsUo = UserObjectUtil.ParseInternalUserObjectName(tok, UserObjectType.Annotation);
			}
			else ResultsUo = new UserObject(UserObjectType.Annotation);

			UserObject uo = UserObjectSaveDialog.Show("Enter Analysis Name", ResultsUo);
			if (uo != null)
			{
				ResultsName.Text = uo.Name;
				string txt = uo.InternalName;
				ResultsUo = uo;
			}
			ResultsName.Focus();
			return;
		}

		private void Search_Click(object sender, EventArgs e)
		{
			try
			{
				DialogResult dr = ProcessInput();
				if (dr == DialogResult.OK) DialogResult = dr;
				return;
			}

			catch (Exception ex)
			{
				string msg = "Unexpected error: " + ex.Message + "\r\n" + DebugLog.FormatExceptionMessage(ex);
				ServicesLog.Message(msg);
				MessageBoxMx.ShowError(msg);
			}
		}

		private void Cancel_Click(object sender, EventArgs e)
		{
			return;
		}

		private void Help_Click(object sender, EventArgs e)
		{
			QbUtil.ShowConfigParameterDocument("MultStructSearchHelpUrl", "Multiple Structure Search Help");
		}

		/// <summary>
		/// Search button clicked, process input
		/// </summary>
		/// <returns></returns>

		DialogResult ProcessInput()
		{
			CidList cidList = null;
			StreamReader structureFileReader = null;
			string qid; // query identifier, compoundId, file name or sdFile key value
			QueryManager qm;
			DataTableMx dt;
			DataColumn dc;
			DataRowMx dr;
			//object[] dr; // if using Qe
			DialogResult dlgRslt = DialogResult.OK;
			Query q = null;
			QueryTable qt = null;
			QueryColumn simScoreQc, structQc; // query column containing latest query settings
			MetaTable mt = null;
			MetaColumn keyMc = null, structMc = null, dbSetMc = null, simScoreMc = null, mc;
			MetaColumnType storageType;
			string txt, tok;

			if (DatabasesToSearch.Text == "")
			{
				MessageBoxMx.ShowError("Databases to search must be defined.");
				DatabasesToSearch.Focus();
				return DialogResult.Cancel;
			}

			// Get list of databases

			string[] dba = DatabasesToSearch.Text.Split(',');
			List<MetaTable> tables = new List<MetaTable>();

			foreach (string dbLabel0 in dba)
			{
				string dbLabel = dbLabel0.Trim();

				RootTable dbInfo = RootTable.GetFromTableLabel(dbLabel);
				if (dbInfo == null)
				{
					MessageBoxMx.ShowError("Can't find database " + DatabasesToSearch.Text);
					DatabasesToSearch.Focus();
					return DialogResult.Cancel;
				}

				mt = MetaTableCollection.Get(dbInfo.MetaTableName);
				if (mt == null)
				{
					MessageBoxMx.ShowError("Unable to locate parent structure table for database: " + DatabasesToSearch.Text);
					DatabasesToSearch.Focus();
					return DialogResult.Cancel;
				}

				if (dbSetMc == null) dbSetMc = mt.DatabaseListMetaColumn;

				tables.Add(mt);
			}

			if (dbSetMc == null) throw new Exception("\"Databases\" metacolumn not found for any of the databases to search");

			// Validate other form values

			RetrieveStructures = RetrieveMatchingStructures.Checked;

			bool fromList = FromList.Checked;
			int listCidsRead = 0;
			int inputQueryCount = -1;
			if (fromList) // using list, validate list name
			{
				if (SavedListUo == null)
				{
					MessageBoxMx.ShowError("Compound list must be defined.");
					ListName.Focus();
					return DialogResult.Cancel;
				}

				cidList = CidListCommand.Read(SavedListUo);
				if (cidList == null)
				{
					MessageBoxMx.ShowError("Error reading list.");
					ListName.Focus();
					return DialogResult.Cancel;
				}

				inputQueryCount = cidList.Count;
			}

			else // Using SdFile, validate SdFile name
			{

				StructureFile = FileName.Text;
				if (StructureFile == "")
				{
					MessageBoxMx.ShowError("File must be defined.");
					FileName.Focus();
					return DialogResult.Cancel;
				}

				try { structureFileReader = new StreamReader(StructureFile); structureFileReader.Close(); }
				catch (Exception ex)
				{
					MessageBoxMx.ShowError("Can't read file: " + Lex.Dq(StructureFile));
					FileName.Focus();
					return DialogResult.Cancel;
				}

				keyField = KeyField.Text; // get key, blank to use name in 1st line

				inputQueryCount = -1; // don't know how many queries unless we read the file (todo?)
			}

			tok = ResultsName.Text.Trim(); // name to store results under
			if (tok == "")
			{
				MessageBoxMx.ShowError("A name for the results must be provided.");
				ResultsName.Focus();
				return DialogResult.Cancel;
			}

			if (SubStruct.Checked) Psc.SearchType = StructureSearchType.Substructure;
			else if (Full.Checked) Psc.SearchType = StructureSearchType.FullStructure;
			else if (Similarity.Checked) Psc.SearchType = StructureSearchType.MolSim;
			else throw new Exception("Unrecognized search type");

			// Write initial log entries

			SearchCount++;
			string logFileName = ClientDirs.DefaultMobiusUserDocumentsFolder + @"\Multistructure Search " + SearchCount + ".txt";
			if (!UIMisc.CanWriteFileToDefaultDir(logFileName)) return DialogResult.Cancel;
			LogStream = new StreamWriter(logFileName);

			if (ResultsUo == null) ResultsUo = new UserObject(UserObjectType.Annotation);
			ResultsUo.Name = tok;
			UserObjectTree.GetValidUserObjectTypeFolder(ResultsUo);

			DateTime startTime = DateTime.Now;

			WriteToLog("Multiple " + Psc.SearchType + " Search");
			WriteToLog("Databases: " + DatabasesToSearch.Text);
			WriteToLog("Date: " + startTime);
			if (fromList) WriteToLog("Input List: " + SavedListUo.Name);
			else WriteToLog("Input Structure File: " + StructureFile);
			WriteToLog("Output List: " + ResultsUo.Name);

			WriteToLog("Log File: " + logFileName);
			WriteToLog("");
			WriteToLog("Query, Match, Score");

			int queryCount = 0;
			int matchAtLeastOneCount = 0;
			MoleculeMx queryStructure = null; // current structure being searched
			CidList matchList = new CidList();
			List<MatchData> matchData = new List<MatchData>();

			if (FromFile.Checked) // open SdFile as required
				structureFileReader = new StreamReader(StructureFile);

			// Search of structures one at a time

			while (true)
			{
				if (fromList) // get next structure from list
				{
					if (listCidsRead >= cidList.Count) break;
					qid = cidList[listCidsRead].Cid;
					listCidsRead++;
					if (qid.Trim() == "") continue;
					if (qid.ToLower().IndexOf(".mol") > 0 || qid.ToLower().IndexOf(".skc") > 0)
					{ // file reference
						if (!File.Exists(qid)) continue;
						if (qid.ToLower().IndexOf(".mol") > 0)
							queryStructure = MoleculeMx.ReadMolfile(qid);
						else queryStructure = MoleculeMx.ReadSketchFile(qid);
					}

					else queryStructure = MoleculeUtil.SelectMoleculeForCid(qid);
					if (queryStructure == null || queryStructure.AtomCount == 0) continue; // oops
				}

				else // get next structure from input file
				{
					qid = null;

					if (StructureFile.ToLower().EndsWith(".sdf"))
					{
						List<SdFileField> fList = SdFileDao.Read(structureFileReader);
						if (fList == null) // end of sdFile
						{
							structureFileReader.Close();
							break;
						}
						if (fList.Count == 0) continue;

						queryStructure = new MoleculeMx(MoleculeFormat.Molfile, fList[0].Data);
						if (queryStructure == null || queryStructure.AtomCount == 0) continue;

						if (keyField != "") // key field specified?
							qid = SdFileDao.GetDataField(fList, keyField);
						else // get name from 1st line of molfile
						{
							string molFile = fList[0].Data;
							int i1 = molFile.IndexOf("\n");
							if (i1 == 0) qid = "";
							else qid = molFile.Substring(0, i1).Trim();
						}
						if (string.IsNullOrEmpty(qid)) qid = SdFileDao.GetDataField(fList, "compound_id");
					}

					else // assume smiles file
					{
						string smiles = structureFileReader.ReadLine();
						if (smiles == null) // end of sdFile
						{
							structureFileReader.Close();
							break;
						}
						smiles = smiles.Trim();
						if (smiles.Length == 0) continue;

						int i1 = smiles.IndexOf(","); // get any preceeding queryId
						if (i1 < 0) i1 = smiles.IndexOf("\t");
						if (i1 >= 0)
						{
							qid = smiles.Substring(0, i1).Trim();
							smiles = smiles.Substring(i1 + 1).Trim();
						}

						queryStructure = new MoleculeMx(MoleculeFormat.Smiles, smiles);
						if (queryStructure == null || queryStructure.AtomCount == 0) continue;
					}

					if (qid == null || qid.Trim() == "") qid = (queryCount + 1).ToString(); // be sure we have a query id
				}

				queryCount++; // count the query
				if (queryStructure == null || queryStructure.AtomCount == 0)
				{
					WriteToLog("Error converting specific structure " + queryCount.ToString() + ", " + qid);
					continue;
				}

				queryStructure.RemoveStructureCaption(); // remove any Mobius-added caption
				Psc.Molecule = queryStructure;

				string msg =
					"Searching Structure: " + queryCount.ToString();
				if (inputQueryCount > 0) msg += " of " + inputQueryCount.ToString();
				msg += "\n" +
					"Structures with one or more matches: " + matchAtLeastOneCount.ToString() + "\n" +
					"Total Matches: " + matchList.Count.ToString();

				Progress.Show(msg);

				// Do the search over the list of databases

				for (int ti = 0; ti < tables.Count; ti++)
				{
					mt = tables[ti];

					q = new Query(); // build basic query
						 //q.SingleStepExecution = true; // do in single step (doesn't currently return sim score)
					q.ShowStereoComments = false;

					qt = new QueryTable(mt);

					q.AddQueryTable(qt);
					qt.SelectKeyOnly(); // start selecting desired cols

					keyMc = mt.KeyMetaColumn;

					structMc = mt.FirstStructureMetaColumn;
					structQc = qt.GetQueryColumnByName(structMc.Name);
					structQc.Selected = RetrieveStructures;

					dbSetMc = mt.DatabaseListMetaColumn;
					if (dbSetMc == null) throw new Exception("\"Databases\" metacolumn not found for table: " + mt.Label);
					QueryColumn dbSetQc = qt.GetQueryColumnByName(dbSetMc.Name);
					dbSetQc.Selected = true; // select the database name
					RootTable root = RootTable.GetFromTableName(mt.Name);
					txt = " in (" + root.Label + ")";
					dbSetQc.Criteria = dbSetMc.Name + txt;
					dbSetQc.CriteriaDisplay = txt;

					simScoreMc = mt.SimilarityScoreMetaColumn;
					simScoreQc = null;
					if (simScoreMc != null) // get sim score if it exists
					{
						simScoreQc = qt.GetQueryColumnByName(simScoreMc.Name);
						simScoreQc.Selected = true; // return sim score
					}

					Psc.QueryColumn = structQc;
					ParsedStructureCriteria psc2 = AdjustSearchForSmallWorldAsNeeded(Psc);
					psc2.ConvertToQueryColumnCriteria(structQc); // format the QC for the structure search

					DateTime t0 = DateTime.Now;

					//QueryEngine qe = new QueryEngine();
					//qe.NextRowsMin = 1000; // minimum number of rows to prefetch
					//qe.NextRowsMax = -1; // maximum number of rows to prefetch
					//qe.NextRowsMaxTime = 10000; // max time in milliseconds for next fetch
					//qe.ExecuteQuery(q);

					qm = new QueryManager();
					try { dlgRslt = qm.ExecuteQuery(ref q); }
					catch (Exception ex)
					{
						WriteToLog("Error searching structure: " + ex.Message + ", " + queryCount.ToString() + ", " + qid);
						continue;
					}

					if (dlgRslt != DialogResult.OK) return dlgRslt;

					double executeTime = TimeOfDay.Delta(ref t0);

					int offset = qm.DataTableManager.KeyValueVoPos + 1;
					//int offset = 0; // for QE
					int keyPos = offset++;
					int strPos = RetrieveStructures ? offset++ : -1;
					int dbPos = offset++;
					int simPos = offset++;

					int fetchCnt = 0;
					while (true)
					{
						dr = qm.NextRow();
						//dr = qe.NextRow(); // for Qe
						if (dr == null) break;

						fetchCnt++;

						if (fetchCnt == 1)
							matchAtLeastOneCount++; // number of queries that have at least one match

						MatchData md = new MatchData();
						md.Qno = queryCount;
						md.Qid = qid;
						if (RetrieveStructures)
							md.Qstr = "Chime=" + queryStructure.GetChimeString();

						CompoundId cid = CompoundId.ConvertTo(dr[keyPos]);
						md.Mid = cid.Value;

						if (RetrieveStructures)
						{
							MoleculeMx ms = MoleculeMx.ConvertTo(dr[strPos]);
							if (!NullValue.IsNull(ms))
								md.Mstr = "Chime=" + ms.GetChimeString();
						}

						StringMx db = StringMx.ConvertTo(dr[dbPos]);
						if (!NullValue.IsNull(db)) md.Db = db.Value;

						if (Psc.SearchType == StructureSearchType.MolSim)
						{
							NumberMx nex = NumberMx.ConvertTo(dr[simPos]);
							if (!NullValue.IsNull(nex)) md.Score = nex.Value;
						}

						if (matchList.Contains(cid.Value)) // already have compound id as match for other query?
						{
							if (Psc.SearchType != StructureSearchType.MolSim) continue; // if similarity search see if more similar
							CidListElement le = matchList.Get(cid.Value); // reference current score
							if (le.Tag > md.Score) continue; // only replace if more similar
							matchList.Remove(le.Cid); // remove from list
							for (int mi = 0; mi < matchData.Count; mi++) // remove from data
							{
								if (matchData[mi].Mid == md.Mid)
								{
									matchData.RemoveAt(mi);
									break;
								}
							}
						}

						matchList.Add(md.Mid);
						matchList.Get(md.Mid).Tag = md.Score; // keep score in list
						matchData.Add(md); // add to results

						txt = md.Qid + ", " + md.Mid + ", " + md.Score.ToString();
						WriteToLog(txt);
					} // Fetch result loop

					double fetchTime = TimeOfDay.Delta(ref t0);

				} // DB loop

				if (Progress.CancelRequested)
				{
					Progress.Hide();
					MessageBoxMx.ShowError("Search cancelled.");
					try { LogStream.Close(); } catch { }
					return DialogResult.Cancel;
				}

			} // key loop

			CidListCommand.WriteCurrentList(matchList); // write the list of numbers

			UsageDao.LogEvent("MultipleStructSearch");

			txt =
			 "=== Multiple structure search complete ===\r\n\r\n" +

			 "Structures Searched: " + queryCount.ToString() + "\r\n";

			txt +=
			 "Structures with one or more matches: " + matchAtLeastOneCount.ToString() + "\r\n" +
			 "Total Matches: " + matchList.Count.ToString() + "\r\n";

			TimeSpan ts = DateTime.Now.Subtract(startTime);
			ts = new TimeSpan(ts.Hours, ts.Minutes, ts.Seconds);
			txt += "Total Time: " + ts + "\r\n\r\n";

			WriteToLog("\r\n" + txt);
			try { LogStream.Close(); } catch { }

			if (matchList.Count == 0)
			{
				Progress.Hide();
				MessageBoxMx.ShowError("No matches have been found.");
				return DialogResult.Cancel;
			}

			tok = "Matching compound ids";
			if (Psc.SearchType == StructureSearchType.MolSim) tok = "Similar compound ids";
			txt += tok + " have been written to the current list: " + ResultsUo.Name + "\n" +
			 "Log file written to: " + logFileName + "\n\n" +
			 "Do you want to view the match results?";

			DialogResult dRslt = MessageBoxMx.Show(txt, "Multiple Structure Search", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

			if (dRslt == DialogResult.Cancel)
				return DialogResult.Cancel;

			else if (dRslt == DialogResult.No) // show log
			{
				SystemUtil.StartProcess(logFileName);
				return DialogResult.Cancel;
			}

			// Display results

			Progress.Show("Formatting results...");

			mt = new MetaTable();
			mt.Name = "MULTSTRUCTSEARCH_" + SearchCount;
			mt.Label = "Multiple Structure Search " + SearchCount;
			mt.TableMap = "Mobius.Tools.MultStructSearch"; // plugin id
			mt.MetaBrokerType = MetaBrokerType.NoSql;

			ColumnSelectionEnum structureColumnSelection = RetrieveStructures ? ColumnSelectionEnum.Selected : ColumnSelectionEnum.Unselected;

			keyMc = keyMc.Clone();
			keyMc.Name = "MatchingCid";
			keyMc.Label = "Matching Compound Id";
			mt.AddMetaColumn(keyMc);

			structMc = structMc.Clone();
			structMc.Name = "MatchingStructure";
			structMc.Label = "Matching Structure";
			structMc.InitialSelection = structureColumnSelection;
			mt.AddMetaColumn(structMc);

			dbSetMc = dbSetMc.Clone();
			dbSetMc.Name = "Database";
			mt.AddMetaColumn(dbSetMc);
			//if (DatabasesToSearch.Text.Contains(","))
			dbSetMc.InitialSelection = ColumnSelectionEnum.Selected;

			mc = mt.AddMetaColumn("Molsimilarity", "Similarity Search Score", MetaColumnType.Number, ColumnSelectionEnum.Unselected, 10);
			if (Psc.SearchType == StructureSearchType.MolSim) mc.InitialSelection = ColumnSelectionEnum.Selected;

			mc = mt.AddMetaColumn("QueryNo", "Query Number", MetaColumnType.Integer);
			//mc = mt.AddMetaColumn("QueryMatchNo", "Query Match Number", MetaColumnType.Integer);

			mc = mt.AddMetaColumn("QueryId", "Query Id", MetaColumnType.String);
			mc = mt.AddMetaColumn("QueryStructure", "Query Structure", MetaColumnType.Structure);
			mc.InitialSelection = structureColumnSelection;

			q = ToolHelper.InitEmbeddedDataToolQuery(mt);
			dt = q.ResultsDataTable as DataTableMx;

			for (int mi = 0; mi < matchData.Count; mi++)
			{
				MatchData md = matchData[mi];
				dr = dt.NewRow();
				dr[qt.Alias + ".MatchingCid"] = new CompoundId(md.Mid);
				if (RetrieveStructures)
					dr[qt.Alias + ".MatchingStructure"] = new MoleculeMx(MoleculeFormat.Chime, md.Mstr);
				dr[qt.Alias + ".Database"] = new StringMx(md.Db);
				if (Psc.SearchType == StructureSearchType.MolSim)
					dr[qt.Alias + ".Molsimilarity"] = new NumberMx(md.Score);
				dr[qt.Alias + ".QueryNo"] = new NumberMx(md.Qno);
				dr[qt.Alias + ".QueryId"] = new StringMx(md.Qid);
				if (RetrieveStructures)
					dr[qt.Alias + ".QueryStructure"] = new MoleculeMx(MoleculeFormat.Chime, md.Qstr);

				dt.Rows.Add(dr);
			}

			ToolHelper.DisplayData(q, dt, true);

			Progress.Hide();
			return DialogResult.OK;
		}

		/// <summary>
		/// Setup a SmallWorld search that most closely matches a full structure, SSS or Similarity search
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="structQc"></param>
		/// <param name="psc"></param>
		/// <returns></returns>

		ParsedStructureCriteria AdjustSearchForSmallWorldAsNeeded(
			ParsedStructureCriteria psc)
		{
			/*
			public static SmallWorldPredefinedParameters
				SmallWorld = ToSwp("preset=SmallWorld; dist=0-4; tup=0-4; tdn=0-4; rup=0-4; rdn=0-4; lup=0-4; ldn=0-4; maj=0-4; min=0-4; sub=0-4; hyb=0-4"),
				Substructure = ToSwp("preset=Substructure; dist=0-4; tup=0-10; tdn=0-0; rup=0-10; rdn=0-0; lup=0-10; ldn=0-0; maj=0-0; min=0-0; sub=0-4; hyb=0-4"),
				SuperStructure = ToSwp("preset=SuperStructure; dist=0-4; tup=0-0; tdn=10-0; rup=0-0; rdn=0-10; lup=0-0; ldn=0-10; maj=0-0; min=0-0; sub=0-4; hyb=0-4"),
				BemisMurckoFramework = ToSwp("preset=Bemis-Murcko Framework; dist=0-4; tup=0-10; tdn=0-10; rup=0-0; rdn=0-0; lup=0-0; ldn=0-0; maj=0-10; min=0-10; sub=0-4; hyb=0-4"),
				NqMCS = ToSwp("preset=Nq MCS; dist=0-4; tup=0-10; tdn=0-10; rup=0-10; rdn=0-10; lup=0-10; ldn=0-0; maj=0-0; min=0-0; sub=0-4; hyb=0-4"),
				ElementGraph = ToSwp("preset=Element Graph; dist=0-4; tup=0-0; tdn=0-0; rup=0-0; rdn=0-0; lup=0-0; ldn=0-0; maj=0-0; min=0-0; sub=0-4; hyb=0-4"),
				CustomSettings = ToSwp("preset=Custom; dist=0-4; tup=0-4; tdn=0-4; rup=0-4; rdn=0-4; lup=0-4; ldn=0-4; maj=0-4; min=0-4; sub=0-4; hyb=0-4"); // defaults to same values as "SmallWorld"
			*/

			SmallWorldPredefinedParameters swp;

			MetaTable mt = psc?.QueryColumn?.MetaTable;
			if (mt == null)
				throw new Exception("Metatable not defined in PSC");

			if (mt.IsCartridgeSearchable) return psc;

			if (!mt.IsSmallWorldSearchable)
				throw new Exception("Data table is not searchable by cartridge or SmallWorld");

			if (psc.SearchType == StructureSearchType.SmallWorld) return psc; // already SmallWorld search?

			ParsedStructureCriteria psc2 = psc.Clone();
			psc2.SearchType = StructureSearchType.SmallWorld;

			if (psc.SearchType == StructureSearchType.FullStructure)
			{
				swp = CriteriaStructureOptionsSW.ElementGraph.Clone();
			}

			else if (psc.SearchType == StructureSearchType.Substructure)
			{
				swp = CriteriaStructureOptionsSW.Substructure.Clone();
			}

			else if (psc.SearchType == StructureSearchType.MolSim)
			{
				swp = CriteriaStructureOptionsSW.SmallWorld.Clone();
			}

			else throw new Exception("Unsupported structure search type for " + mt.Label + ": " + psc.SearchType);

			swp.Database = mt.Name;

			psc2.SmallWorldParameters = swp;
			return psc2;
		}

		/// <summary>
		/// Write to log file avoiding exceptions
		/// </summary>
		/// <param name="txt"></param>

		void WriteToLog(string txt)
		{
			if (LogStream == null) return;

			try { LogStream.WriteLine(txt); }
			catch (Exception ex)
			{
				DebugLog.Message(ex);
				LogStream = LogStream;
				try { LogStream.Close(); } catch { }
				LogStream = null; // avoid further write attempts
			}
		}

		/// <summary>
		/// Match data stored here
		/// </summary>

		class MatchData
		{
			public int Id; // id of match
			public int Qno; // seq number of query
			public string Qid; // query id (registry number, file name, sdfile key, etc)
			public string Qstr; // query structure (chimestring)
			public string Mid; // match database regno
			public string Mstr; // match structure (chimestring)
			public string Db; // database
			public double Score; // match score if sim search
		}

	}
}