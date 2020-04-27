using Mobius.Data;
using Mobius.ComOps;
using Mobius.ServiceFacade;
using Mobius.ClientComponents.Dialogs;

using DevExpress.XtraTab;
using DevExpress.XtraSplashScreen;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

	/// <summary>
	/// Command execution
	/// </summary>
	/// 
	public class CommandExec
	{
		public static QueriesControl QueriesControl // control containing queries & results
		{ // get reference to QueriesControl
			get { return QueriesControl.Instance; }
			set { QueriesControl.Instance = value; }
		}
		public static QueriesControl Qrc { get { return QueriesControl.Instance; } } // alias 

		public static XtraTabControl QueriesControlTabs { get { return QueriesControl.Tabs; } } // tabs in Queries control
		public static XtraTabPage QueriesControlTabPage { get { return QueriesControlTabs.SelectedTabPage; } } // current query tab
		public static int QueriesControlTabIndex { get { return QueriesControlTabs.SelectedTabPageIndex; } } // current query tab index

		public static QbControl QueryBuilderControl
		{ // get reference to current QueryBuilderControl
			get { return SessionManager.Instance.QueriesControl.QueryBuilderControl; }
		}

		public static QbContentsTree QbContentsTree // ref to main contents tree control
		{ get { return QueriesControl != null && QueriesControl.QueryBuilderControl != null ? QueriesControl.QueryBuilderControl.QbContentsCtl : null; } }

		public static List<Document> DocumentList // ordered list of documents, one per tab
		{ // get reference to QueriesControl
			get { return QueriesControl.Instance.DocumentList; }
			set { QueriesControl.Instance.DocumentList = value; }
		}

		public static int CurrentQueryIndex // index of current query
		{ // get reference to QueriesControl
			get { return QueriesControl.Instance.CurrentQueryIndex; }
			set { QueriesControl.Instance.CurrentQueryIndex = value; }
		}

		public static Query Query // current query
		{ // get reference to QueriesControl
			get { return QueriesControl.Instance.CurrentQuery; }
			set { QueriesControl.Instance.CurrentQuery = value; }
		}

		public static QueryTable Qt // current query table
		{ // get reference to QueriesControl
			get { return QueriesControl.Instance.QueryBuilderControl.QueryTablesControl.CurrentQt; }
			set { QueriesControl.Instance.QueryBuilderControl.QueryTablesControl.CurrentQt = value; }
		}

		static MainMenuControl MainMenuControl { get { return SessionManager.Instance.MainMenuControl; } }

		static CommandExec Instance;
		static string PendingCommand; // command queued for execution
		static Timer PendingCommandTimer;
		static bool InPendingCommandTimer_Tick = false;

/// <summary>
/// Execute a command asynchronously
/// </summary>
/// <param name="commandMixed"></param>

		public static void ExecuteCommandAsynch(string commandMixed)
		{
			DelayedCallback.Schedule(DelayedExecuteCommand, commandMixed);
			return;
		}

		static void DelayedExecuteCommand(object parm)
		{
			string commandMixed = parm as string;
			string result = ExecuteCommand(commandMixed);
		}

		/// <summary>
		/// Execute a single command
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>

		public static string ExecuteCommand(string commandMixed)
		{
			MetaTable mt;
			UserObject uo, uo2;
			MetaTreeNode mtn;
			MetaTreeNode[] mtnArray;
			string nodeTypeName, command, commandName, commandArgs, commandArg1, nextCommand, response = null, target, tok;
			int libId, ti, i1;
			UserObject[] uoArray;

			try
			{
				Lex lex = new Lex();
				nextCommand = commandMixed;

				while (true)
				{

					if (String.IsNullOrEmpty(nextCommand))
					{
						if (SS.I.ScriptLevel >= 0) // reading from script?
						{
							while (SS.I.ScriptLevel >= 0 && String.IsNullOrEmpty(nextCommand))
								nextCommand = CommandLine.ReadScriptFile();

							if (SS.I.ScriptLevel >= 0) ScriptLog.Message("< " + nextCommand);
						}

						if (String.IsNullOrEmpty(nextCommand)) break;
					}

					commandMixed = nextCommand;
					nextCommand = "";

					command = commandMixed.ToUpper();
					lex.OpenString(command);
					commandName = lex.Get(); // get command name

					commandArgs = lex.GetRestOfLine();
					if (commandArgs != "") lex.Backup();

					//string[] result;
					//string[] stringSeparators = new string[] { ";" };
					//result = commandArgs.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

					commandArg1 = lex.Get();
					if (commandArg1 != "") lex.Backup();

					response = ""; // response from command

					//******************************************************************************
					// Home window commands
					//******************************************************************************

					if (Lex.Eq(commandName, "Command "))
					{
						nextCommand = commandMixed.Substring(8); // extract real command
						continue;
					}

					else if (Lex.Eq(commandName, "CommandLine")) // something entered on command line
					{
						string command2 = commandMixed.Trim(); // strip out command from full string mixed case
						string cl = "commandline";
						if (command2.Length > cl.Length) command2 = command2.Substring(cl.Length).Trim();
						command2 = Lex.RemoveAllQuotes(command2).Trim(); // command may be quoted if coming from URL

						if (command2 != "")
						{ // See if compound id
							nextCommand = QbUtil.CheckIfQuickSearchArgIsCid(command2);
							if (nextCommand != null) continue; // continue to process if arg was a cid

							// Try to process as a commandline

							response = CommandLine.Execute(command2);

							// See if we can find it in the contents

							if (response == null)
							{
								bool foundInContents = QbContentsTree.QbContentsTreeCtl.FindInContents(command2);
								if (!foundInContents) response = Lex.Dq(command2) + " was not found";
							}
						}
					}

					else if (Lex.Eq(commandName, "SelectAllCompoundData"))
					{ // all data for compound
						string cid = CompoundId.Normalize(commandArg1);
						string extCid = CompoundId.Format(cid);
						if (!CompoundIdUtil.Exists(cid))
						{
							MessageBoxMx.ShowError("Compound Id doesn't exist: " + commandArg1);
							ServicesLog.Message("Compound Id doesn't exist: " + commandArg1 + ", " + cid + ", " + extCid);
							continue;
						}

						Progress.Show("Building Query...");
						Query q = QueryEngine.GetSelectAllDataQuery(null, cid);
						if (q == null || q.Tables.Count == 0)
						{
							MessageBoxMx.ShowError("Unable to build query");
							continue;
						}

						Progress.Show("Retrieving data..."); // put up progress dialog since this may take a while
						QbUtil.RunPopupQuery(q, MetaTable.PrimaryKeyColumnLabel + " " + extCid);
						continue;
					}

					else if (Lex.Eq(commandName, "ShowStbViewForCompound"))
					{
						response = CommandLine.Execute("TargetResultsViewer ShowPopup " + commandArg1);
					}

					else if (Lex.Eq(commandName, "BringShellFormToFront"))
					{
						if (SessionManager.Instance.ShellForm != null)
							SessionManager.Instance.ShellForm.BringToFront();
					}

					//******************************************************************************
					// Target commands
					//******************************************************************************

					else if (Lex.Eq(commandName, "ShowTargetDescription"))
					{
						string targetIdSymbol = lex.Get();
						targetIdSymbol = Lex.Replace(targetIdSymbol, "TGT_ASSYS_", ""); // remove any target node prefix

						ResultsFormatter.ShowNcbiEntrezGeneWebPage(targetIdSymbol);
					}

					else if (Lex.Eq(commandName, "ShowTargetAssayList")) // list of assays associated with the target
					{ // browser popup command to add table
						string targetIdSymbol = lex.Get();
						targetIdSymbol = Lex.Replace(targetIdSymbol, "TGT_ASSYS_", ""); // remove any target node prefix

						Query q = MultiDbAssayMetaBroker.BuildTargetAssayListQuery(targetIdSymbol);
						QbUtil.RunPopupQuery(q, targetIdSymbol + " Assays", OutputDest.WinForms);
					}

					else if (Lex.Eq(commandName, "ShowAllTargetUnsummarizedAssayData")) // all unsummarized assay data for a target
					{
						QbUtil.SetMode(QueryMode.Build); // be sure in build mode

						string targetIdSymbol = lex.Get();
						targetIdSymbol = Lex.Replace(targetIdSymbol, "TGT_ASSYS_", ""); // remove any target node prefix

						Query q = UnpivotedAssayView.BuildTargetAssayUnsummarizedDataQuery(targetIdSymbol);
						q.Name = targetIdSymbol + " All Unsummarized Assay Data";
						QbUtil.AddQuery(q);
						QbUtil.SetCurrentQueryInstance(q); // set to new query
						QbUtil.RenderQuery();
						nextCommand = "RunQuery";
					}

					else if (Lex.Eq(commandName, "ShowAllTargetSummarizedAssayData")) // all summarized assay data for a target (need better data)
					{
						QbUtil.SetMode(QueryMode.Build); // be sure in build mode

						string targetIdSymbol = lex.Get();
						targetIdSymbol = Lex.Replace(targetIdSymbol, "TGT_ASSYS_", ""); // remove any target node prefix

						Query q = MultiDbAssayMetaBroker.BuildTargetAssaySummarizedDataQuery(targetIdSymbol);
						q.Name = targetIdSymbol + " All Unsummarized Assay Data";
						QbUtil.AddQuery(q);
						QbUtil.SetCurrentQueryInstance(q); // set to new query
						QbUtil.RenderQuery();
						nextCommand = "RunQuery";
					}


					else if (Lex.Eq(commandName, "ShowTargetDataByTarget")) // show all assay data for target summarized by target (need better data summary)
					{
						QbUtil.SetMode(QueryMode.Build); // be sure in build mode

						string targetIdSymbol = lex.Get();
						targetIdSymbol = Lex.Replace(targetIdSymbol, "TGT_ASSYS_", ""); // remove any target node prefix

						Query q = MultiDbAssayMetaBroker.BuildTargetAssaySummarizedDataQuery(targetIdSymbol);
						q.Name = targetIdSymbol + " All Data Summarized by Target";
						QbUtil.AddQuery(q);
						QbUtil.SetCurrentQueryInstance(q); // set to new query
						QbUtil.RenderQuery();
						nextCommand = "RunQuery";
					}

					//******************************************************************************
					// MetaTable commands
					//******************************************************************************

					else if (Lex.Eq(commandName, "ShowTableDescription"))
					{
						string mtName = lex.Get(); // may be other than current table if coming from QuickSearch html
						QbUtil.ShowTableDescription(mtName);
					}

					else if (Lex.Eq(commandName, "AddTableToQuery"))
					{ // browser popup command to add table
						string mtName = lex.Get();
						QbUtil.AddAndRenderTables(mtName);
					}

					else if (Lex.Eq(commandName, "AddMetaTables"))
					{
						QbUtil.AddAndRenderTables(commandArgs);
					}

					else if (Lex.Eq(commandName, "AddSummarizedTable"))
					{
						string mtName = lex.Get(); // table name
																			 //if (Lex.Eq(tableName, "TABLE") || Lex.Eq(tableName, "METATABLE")) tableName = lex.Get(); // table name
						mtName += MetaTable.SummarySuffix; // indicate summary table
						QbUtil.AddAndRenderTables(mtName);
					}

					else if (Lex.Eq(commandName, "ShowResultInTree"))
					{
						SplashScreenManager.ShowDefaultWaitForm();
						string currentMetaNodeName = lex.Get();
						if (string.IsNullOrEmpty(currentMetaNodeName))
						{
							SplashScreenManager.CloseDefaultWaitForm();
							MessageBoxMx.Show("No node selected. Unable to expand tree.");
						}
						else
						{
							SessionManager.Instance.MainContentsControl.ShowNormal(currentMetaNodeName);
							SplashScreenManager.CloseDefaultWaitForm();
						}
					}

					//******************************************************************************
					// Quick structure searches
					//******************************************************************************

					else if (Lex.Eq(commandName, "QuickStructureSearchInclusive"))
						response = QbUtil.QuickStructureSearch(true);

					else if (Lex.Eq(commandName, "QuickStructureSearchExclusive"))
						response = QbUtil.QuickStructureSearch(false);


					//******************************************************************************
					// New, open, close, save & clear query
					//******************************************************************************

					else if (Lex.Eq(commandName, "NewQuery"))
					{
						QbUtil.NewQuery();
					}

					else if (Lex.Eq(commandName, "OpenQuery")) // open saved query
					{
						nextCommand = QbUtil.OpenQuery(null);
						continue;
					}

					else if (Lex.Eq(commandName, "FileClose")) // generic close 
					{
						QbUtil.SetMode(QueryMode.Build);
						QbUtil.CloseFile(true);
						if (QbUtil.DocumentList.Count == 0) QbUtil.NewQuery(); // keep one query document open
						continue;
					}

					else if (Lex.Eq(commandName, "FileCloseAll")) // close all open documents
					{
						QbUtil.CloseFileAll();

						if (QbUtil.DocumentList.Count == 0) QbUtil.NewQuery(); // keep one query window open
						continue;
					}

					else if (Lex.Eq(commandName, "FileSave")) // generic save
					{
						QbUtil.SaveFile();
						continue;
					}

					else if (Lex.Eq(commandName, "FileSaveAs")) // generic saveas
					{
						if (QbUtil.CurrentQueryIndex < 0) continue;

						QbUtil.SaveQueryDialog(Query);
						continue;
					}

					else if (Lex.Eq(commandName, "FileSaveAll")) // generic saveall
					{
						MessageBoxMx.Show("Not Implemented");
					}

					else if (Lex.Eq(commandName, "PreviousQuery")) // open any previous query
					{
						if (Query.Tables.Count > 0)
						{
							DialogResult dr = MessageBoxMx.Show(
								"Do you want to replace the current query with the previous query?", UmlautMobius.String,
								MessageBoxButtons.YesNo, MessageBoxIcon.Question);

							if (dr != DialogResult.Yes) continue;
						}

						nextCommand = "OpenQuery Previous";
						continue;
					}

					else if (Lex.Eq(commandName, "NewAnnotation"))
					{ // create new annotation & add to query
						mt = UserData.CreateNewAnnotationTable(); // let user define table
						if (mt == null) continue;
						QbUtil.AddAndRenderTable(mt); // add it & render it
						continue;
					}

					else if (Lex.Eq(commandName, "ExistingAnnotation"))
					{ // add existing annotation to query
						mt = UserData.SelectAnnotationTable("Select Annotation Table");
						if (mt == null) continue;
						QbUtil.AddAndRenderTable(mt); // add it & render it
						continue;
					}

					else if (Lex.Eq(commandName, "OpenAnnotation"))
					{ // edit existing annotation
						UserData.OpenExistingAnnotationTable(null);
						continue;
					}

					else if (Lex.Eq(commandName, "NewCalcField"))
					{ // create new calc field & add to query
						mt = CalcFieldEditor.CreateNew(); // let user define table
						if (mt == null) continue;
						QbUtil.AddAndRenderTable(mt); // add it & render it
						continue;
					}

					else if (Lex.Eq(commandName, "ExistingCalcField"))
					{ // add existing calc field to query
						mt = CalcFieldEditor.SelectExisting();
						if (mt == null) continue;
						QbUtil.AddAndRenderTable(mt); // add it & render it
						continue;
					}

					else if (Lex.Eq(commandName, "OpenCalcField"))
					{ // edit existing calc field
						CalcFieldEditor.Edit();
						continue;
					}

					else if (Lex.Eq(commandName, "NewCondFormat"))
					{ // create new cond format user object
						CondFormat cf = CondFormatEditor.CreateNewUserObject();
						continue;
					}

					else if (Lex.Eq(commandName, "ExistingCondFormat"))
					{ // edit existing cond format
						CondFormat cf = CondFormatEditor.EditUserObject();
						continue;
					}

					else if (Lex.Eq(commandName, "NewSpotfireLink"))
					{ // create a new spotfire link
						SpotfireLinkUI.CreateNew();
						continue;
					}

					else if (Lex.Eq(commandName, "EditSpotfireLink"))
					{ // edit existing spotfire link
						string linkName = lex.Get();
						SpotfireLinkUI.Edit(linkName);
						continue;
					}

					else if (Lex.Eq(commandName, "OpenSpotfireLink"))
					{ // open a spotfire link
						string arg = lex.Get();
						SpotfireLinkUI.OpenLink(arg);
						continue;
					}

					else if (Lex.Eq(commandName, "NewUserDatabase"))
					{ // edit existing annotation
						UserData.CreateNewUserDatabase();
						continue;
					}

					else if (Lex.Eq(commandName, "OpenUserDatabase"))
					{ // edit existing annotation
						nextCommand = UserDatabasesExplorer.Explore(SS.I.UserName);
						continue;
					}

					//******************************************************************************
					// Query execution commands
					//******************************************************************************

					else if (Lex.Eq(commandName, "RunQuery"))
					{
						nextCommand = QueryExec.RunQuery(Query, SS.I.DefaultQueryDest);
						continue;
					}

					else if (Lex.Eq(commandName, "RunQuerySingleTable")) // run query for current table only
					{
						string mtName = lex.Get();
						QueryTable qt = Query.GetTableByName(mtName);
						if (qt == null) throw new Exception("Table not in query: " + mtName);
						QueryTable qt2 = qt.Clone(); // make copy that we can modify (i.e. qt2.Query modified when added to q2)
						Query q2 = Query.Clone();
						q2.Tables = new List<QueryTable>();
						q2.AddQueryTable(qt2);

						QueriesControl qctl = SessionManager.Instance.QueriesControl;
						Query saveQuery = qctl.CurrentQuery;

						qctl.CurrentQuery = q2;
						nextCommand = QueryExec.RunQuery(q2, SS.I.DefaultQueryDest);
						qctl.CurrentQuery = saveQuery; // restore query
						continue;
					}

					else if (Lex.Eq(commandName, "RunQuerySingleTablePreview"))
					{
						QueryTable qt, qt2;
						QueryColumn qc, qc2;
						string mtName = lex.Get(); // probably node type
						if (lex.Peek() != "") mtName = lex.Peek(); // get table name

						if (!Permissions.UserHasReadAccess(SS.I.UserName, mtName))
						{
							MessageBoxMx.ShowError("You are not authorized to view this data");
							return "";
						}

						qt = Query.GetTableByName(mtName); // may or may not be in query
						if (qt == null)
						{
							mt = MetaTableCollection.GetWithException(mtName);
							qt = new QueryTable(mt);
						}

						qt2 = qt.Clone(); // make copy that we can modify
						for (i1 = 0; i1 < qt2.QueryColumns.Count; i1++) // clear any criteria
						{
							qc2 = qt2.QueryColumns[i1];
							qc2.Criteria = "";
						}

						Query q2 = new Query();
						q2.Preview = true;
						q2.AddQueryTable(qt2);
						q2.KeySortOrder = 0; // no sorting on preview

						QueriesControl qctl = SessionManager.Instance.QueriesControl;
						Query saveQuery = qctl.CurrentQuery;

						qctl.CurrentQuery = q2;
						nextCommand = QueryExec.RunQuery(q2, SS.I.DefaultQueryDest);
						qctl.CurrentQuery = saveQuery; // restore query

						//					nextCommand = "commandline Run Query 133448"; // for demo only
						continue;
					}

					else if (Lex.Eq(commandName, "SpawnQueryInBackground")) // start a new session to run the query in the background
					{
						if (!QbUtil.CanRunQueryInBackground(Query)) continue;
						response = BackgroundQuery.SpawnBackgroundQuery(Query.UserObject.Id, true);
					}

					else if (Lex.Eq(commandName, "RunQueryInBackground"))
					{
						response = BackgroundQuery.RunBackgroundQuery(commandArgs);
					}

					else if (Lex.Eq(commandName, "Browse")) // Reenter browse mode
					{
						Query.UseResultKeys = true;
						nextCommand = QueryExec.RunQuery(Query, SS.I.DefaultQueryDest, OutputFormContext.Session, null, browseExistingResults: true);
						Query.UseResultKeys = false;
						continue;
					}

					else if (Lex.Eq(commandName, "EditQuery")) // back to query editor
					{
						QbUtil.EditQuery();
						continue;
					}

					//******************************************************************************
					// List commands
					//******************************************************************************

					else if (Lex.Eq(commandName, "List") || // list manipulation
						Lex.Eq(commandName, "Subset")) // database subset
					{
						response = CidListCommand.Process(command);
					}

					//******************************************************************************
					// Favorites commands
					//******************************************************************************

					else if (Lex.Eq(commandName, "ContentsAddToFavorites")) // add from contents menu
					{
						// adding items to favorites menu
						ParseContentsTreeCommand(commandMixed, out commandName, out mtnArray);

						DialogResult nodeFound = DialogResult.No;
						if (mtnArray.Length > 1)
						{
							nodeFound = MessageBox.Show("Add the selected " + mtnArray.Length + " items to Favorites?", "Add to Fovorites", MessageBoxButtons.YesNo);
						}
						else if (mtnArray.Length == 1)
						{
							nodeFound = MessageBox.Show("Add " + mtnArray[0].Label + " to Favorites?", "Add to Fovorites", MessageBoxButtons.YesNo);
						}
						else
						{
							MessageBox.Show("No object selected.");
						}

						if (nodeFound.Equals(DialogResult.Yes))
						{
							foreach (MetaTreeNode mtn2 in mtnArray)
							{
								MetaTreeNode node = MainMenuControl.GetMetaTreeNode(mtn2.Target);
								if (node == null) continue; // not found
								SessionManager.Instance.MainMenuControl.AddToFavorites(node);
							}
						}

					}

					//******************************************************************************
					// Common popup and drilldown/weblink commands
					//******************************************************************************

					else if (ResultsFormatter.ProcessCommonDisplayResponses(commandMixed))
						continue;

					//******************************************************************************
					// Commands specific to query builder contents menu
					//******************************************************************************

					else if (Lex.Eq(commandName, "Open") || Lex.Eq(commandName, "ContentsDoubleClick")) // open item (if item double-clicked, also do Open action)
					{
						ParseContentsTreeCommand(commandMixed, out commandName, out mtnArray);

						//mtn = mtnArray[0];

						//string itemType = lex.Get(); // type of item
						//string itemName = lex.Get(); // name of item
						//lex.Backup();
						//lex.Backup();

						foreach (MetaTreeNode mtn2 in mtnArray)
						{
							QbContentsTree.OpenMetaTreeNode(mtn2);
						}
					}

					else if (Lex.Eq(commandName, "SelectDefaultProject"))
					{
						tok = lex.Get(); // node name
						mtn = MetaTree.GetNode(tok);
						if (mtn != null)
						{
							SS.I.PreferredProjectId = tok;
							UserObjectDao.SetUserParameter(SS.I.UserName, "PreferredProject", mtn.Target);
							SessionManager.Instance.MainContentsControl.ShowNormal(); // redisplay main tree with new selected project open
							MessageBoxMx.Show(mtn.Label + " is now your default project");
						}
						else MessageBoxMx.ShowError("Tree node not found for " + tok);
					}

					else if (Lex.Eq(commandName, "ContentsEdit"))
					{ // edit contents tree
						tok = lex.Get(); // node name
						bool edited = EditContentsTree(tok);
					}

#if false // old AFS projecteditor
					else if (Lex.Eq(commandName, "EditProjectDefinition"))
					{
						tok = lex.Get(); // node name
						mtn = MetaTree.GetNode(tok);
						if (mtn != null)
						{
							string label = mtn.Label;
							DialogResult dr = ProjectEditor.EditProject(mtn.Name);
							if (dr == DialogResult.OK) // edit complete
							{
								QbContentsTree.ContentsTree.RefreshAfsSubtree(mtn.Name);
								MetaTree.MetaTreeFactory.MarkCacheForRebuild();
							}

							else if (dr == DialogResult.Abort) // project deleted
							{
								List<MetaTreeNode> parents = MetaTree.GetParents(mtn);
								foreach (MetaTreeNode parent in parents)
								{
									int ni = 0;
									while (ni < parent.Nodes.Count)
									{
										MetaTreeNode child = parent.Nodes[ni];
										if (Lex.Eq(mtn.Name, child.Name)) // compare on name not address
											parent.Nodes.RemoveAt(ni);
										else ni++;
									}

									QbContentsTree.ContentsTree.RefreshSubtree(parent.Name); // refresh the subtree
								}

								MetaTree.MetaTreeFactory.MarkCacheForRebuild();
							}

						}
						else MessageBoxMx.ShowError("Tree node not found for " + tok);
					}
#endif

					else if (Lex.Eq(commandName, "ContentsShowTableXml"))
					{ // show the Xml associated with a metatable
						string objType = lex.Get(); // get type of node
						string objId = lex.Get(); // item id

						mt = QbUtil.GetMetaTable(objId);
						if (mt == null) continue;

						string xml = mt.Serialize();
						string cFile = ClientDirs.TempDir + @"\Source.xml";
						FileUtil.WriteAndOpenTextDocument(cFile, xml);
					}

#if false // rename for all users
				else if (Lex.Eq(commandName, "RenameContentsItemAllUsers")) // item selected from contents
				{
					string objType = lex.Get(); // get type of node
					string objId = lex.Get(); // item id
					MetaTreeNode mtn = MetaTreeGetNode(objId);
					if (mtn == null)
					{
						MessageBoxMx.ShowError("Tree node not found for " + objId);
						continue;
					}

					string objName = mtn.Name;
					string originalLabel = mtn.Label; // save current label
					string newLabel = RenameMetaDataItem(objName, originalLabel);
					if (newLabel == null) continue;
					mtn.Label = newLabel;

					TreeNodeTransferFormat tntf = ContentsTree.FormatNodeForTransfer(mtn, "", 0);

					Client.Call("Mobius.Client.TreeViewControl.UpdateNode(MainForm.ContentsTree," +
						Lex.Dq(objType + " " + objId) + "," + // tag for folder to add to
						Lex.Dq(tntf.Format()) + ")");

					if (Lex.Eq(objType, "Table")) // if metatable rename that also
					{
						mt = GetMetaTable(mtn.Name);
						if (mt != null)
							mt.Label = newLabel;
					}
				}
#endif

					// Rt-Click Query commands

					else if (Lex.Eq(commandName, "ContentsOpenQuery"))
					{ // Not called, sent as DoubleClick command
					}

					else if (Lex.Eq(commandName, "ContentsAddQueryToCurrentQuery"))
					{ // add selected query to the current query
						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;
						Query addQuery = QbUtil.ReadQuery(uoArray[0].Id);
						if (addQuery == null) continue;

						QueryTable matchingQt;

						foreach (QueryTable qt0 in addQuery.Tables)
						{
							if (QbUtil.TableShouldBeAdded(Query, qt0.MetaTable, out matchingQt))
								Query.AddQueryTable(qt0);
							else Qt = matchingQt;
						}

						QbUtil.RenderQuery(Qt);
					}

					else if (Lex.Eq(commandName, "ContentsAddQueryToCurrentQuerySingleTab"))
					{ // add selected query to the current query keeping under a single tab
						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;
						string mtName =  // metatable name with "multitable" prefix and query object id suffix
							"multitable_" + uoArray[0].Id.ToString();
						QbUtil.AddAndRenderTables(mtName);
					}

					else if (Lex.Eq(commandName, "ContentsOpenAndRunQuery"))
					{
						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;

						tok = uoArray[0].InternalName;
						nextCommand = QbUtil.OpenQuery(tok);
						nextCommand = QueryExec.RunQuery(Query, OutputDest.WinForms);
					}

					// Rt-Click User Database

					else if (Lex.Eq(commandName, "ContentsOpenUserDatabase"))
					{
						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;
						UserData.OpenExistingUserDatabase(uoArray[0]);
					}

					// Rt-Click List commands

					else if (Lex.Eq(commandName, "ContentsOpenList"))
					{ // Not called, sent as DoubleClick command
					}

					else if (Lex.Eq(commandName, "ContentsCopyListToCurrent"))
					{
						string listName = lex.Get(); // get list name
						if (!Permissions.UserHasReadAccess(SS.I.UserName, listName))
						{
							MessageBoxMx.ShowError(
								"Unable to find list: " + listName + "\r\n\r\n" +
								"The list may not exist or you may not be authorized for access.");
							return "";
						}
						uo = UserObjectUtil.ParseInternalUserObjectName(listName);
						uo2 = UserObjectUtil.ParseInternalUserObjectName(CidList.CurrentListInternalName);
						CidListDao.CopyList(uo, uo2);
						SessionManager.CurrentResultKeys = CidListCommand.ReadCurrentListRemote().ToStringList();
						CidListCommand.UpdateTempListCollection(uo2);
						SessionManager.DisplayCurrentCount();
						response = "";
					}

					else if (Lex.Eq(commandName, "ContentsAddList"))
					{
						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;
						response = QbUtil.QuickSetListCriteria(uoArray[0]);
					}

					else if (Lex.Eq(commandName, "ContentsRunQueryList"))
					{
						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;
						response = QbUtil.QuickListSearch(uoArray[0]);
					}

					// Rt-Click CalcField commands

					else if (Lex.Eq(commandName, "ContentsOpenCalcField"))
					{
						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;
						uo = uoArray[0];
						uo = CalcFieldEditor.Edit(uo);
						if (uo != null && uo.Id > 0)
						{
							string mtName = "CALCFIELD_" + uo.Id.ToString();
							mt = MetaTableCollection.Get(mtName); // get possibly-modified metatable
							if (mt != null && Qt != null && Lex.Eq(mt.Name, Qt.MetaTable.Name))
							{
								Qt.RemapToMetaTable(); // adjust Qt for any changes in MetaTable
								QbUtil.RenderQueryTable(Qt);
							}
						}
					}

					// Rt-Click CondFormat commands

					else if (Lex.Eq(commandName, "ContentsCondFormatEdit"))
					{
						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;
						uo = uoArray[0];
						CondFormat cf = CondFormatEditor.EditUserObject(uo);
					}


					// Rt-Click Annotation commands

					else if (Lex.Eq(commandName, "ContentsAddList"))
					{
						if (Query.LogicType == QueryLogicType.Complex)
						{
							MessageBoxMx.ShowError("This operation is not allowed for queries with advanced criteria.");
							continue;
						}

						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;
						uo = uoArray[0];
						response = QbUtil.QuickSetListCriteria(uo);
					}

					else if (Lex.Eq(commandName, "ContentsRunQueryList"))
					{
						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;
						uo = uoArray[0];
						response = QbUtil.QuickListSearch(uo);
					}


					else if (Lex.Eq(commandName, "ContentsOpenAnnotation"))
					{
						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;
						uo = uoArray[0];
						UserData.OpenExistingAnnotationTable(uo);
					}

					// Common Rt-Click object commands

					else if (Lex.Eq(commandName, "ContentsPermissions"))
					{
						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;
						UserObjectUtil.ProcessCommonRightClickObjectMenuCommands("Permissions", mtn, uoArray, null);
					}

					else if (Lex.Eq(commandName, "ContentsMakePublic"))
					{
						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;
						UserObjectUtil.ProcessCommonRightClickObjectMenuCommands("MakePublic", mtn, uoArray, null);
					}

					else if (Lex.Eq(commandName, "ContentsMakePrivate"))
					{
						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;
						UserObjectUtil.ProcessCommonRightClickObjectMenuCommands("MakePrivate", mtn, uoArray, null);
					}

					else if (Lex.Eq(commandName, "ContentsChangeOwner"))
					{
						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;
						UserObjectUtil.ProcessCommonRightClickObjectMenuCommands("ChangeOwner", mtn, uoArray, null);
					}

					else if (Lex.Eq(commandName, "ContentsCut"))
					{
						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;
						UserObjectUtil.ProcessCommonRightClickObjectMenuCommands("Cut", mtn, uoArray, null);
					}

					else if (Lex.Eq(commandName, "ContentsCopy"))
					{
						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;
						UserObjectUtil.ProcessCommonRightClickObjectMenuCommands("Copy", mtn, uoArray, null);
					}

					else if (Lex.Eq(commandName, "ContentsPaste"))
					{
						ParseContentsTreeCommand(command, out commandName, out mtnArray);
						mtn = mtnArray[0];
						UserObjectUtil.ProcessCommonRightClickObjectMenuCommands("Paste", mtn, null, null);
					}

					else if (Lex.Eq(commandName, "ContentsDelete"))
					{
						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;
						UserObjectUtil.ProcessCommonRightClickObjectMenuCommands("Delete", mtn, uoArray, null);
					}

					else if (Lex.Eq(commandName, "ContentsRename"))
					{
						if (!ParseContentsTreeCommandAndReadUserObject(command, out mtn, out uoArray)) continue;
						UserObjectUtil.ProcessCommonRightClickObjectMenuCommands("Rename", mtn, uoArray, null);
					}

					else if (Lex.Eq(commandName, "CreateUserFolder")) // create new user folder
					{ // supplied parent folder may be a system or user folder
						ParseContentsTreeCommand(command, out commandName, out mtnArray);
						mtn = mtnArray[0];
						string parentNodeName = mtn.Name; // selected node is parent
						if (String.IsNullOrEmpty(parentNodeName)) continue;
						QbUtil.CreateUserFolder(parentNodeName, QbContentsTree.QbContentsTreeCtl);
						continue;
					}

					else if (Lex.Eq(commandName, "ContentsViewLibAsList")) // open library in list editor
					{
						tok = lex.Get(); // node name
						mtn = MetaTree.GetNode(tok);
						if (mtn != null)
						{
							MetaTreeNode.ParseTypeNameAndId(tok, out nodeTypeName, out libId);
							CidList cidList = CidListDao.ReadLibrary(libId);
							MetaTable rootTable = MetaTableCollection.Get(MetaTable.PrimaryRootTable);
							CidListEditor.Edit(cidList, rootTable);
						}
						else MessageBoxMx.ShowError("Tree node not found for " + tok);
					}

					else if (Lex.Eq(commandName, "ContentsAddLibToCriteria")) // add library to criteria
					{
						ParsedSingleCriteria psc = null;

						tok = lex.Get(); // node name
						mtn = MetaTree.GetNode(tok);
						if (mtn == null) throw new Exception("Tree node not found for " + tok);
						MetaTreeNode.ParseTypeNameAndId(mtn.Name, out nodeTypeName, out libId);

						if (Query == null) throw new Exception("No current query");
						Query q = Query;
						mt = MetaTableCollection.Get(MetaTable.PrimaryCompoundLibraryTable);
						if (mt == null) throw new Exception("Library table not found: " + MetaTable.PrimaryCompoundLibraryTable);
						QueryTable qt = q.GetQueryTableByName(MetaTable.PrimaryCompoundLibraryTable);
						if (qt == null)
						{
							qt = new QueryTable(mt);
							qt.DeselectAll();
							qt.KeyQueryColumn.Selected = true; // just key column is selected
							q.AddQueryTable(qt);
						}

						string libIdCol = "corp_lbrry_id";
						QueryColumn qc = qt.GetQueryColumnByName(libIdCol);
						if (qc == null) throw new Exception("Expected column not found: " + libIdCol);
						MetaColumn mc = qc.MetaColumn;

						if (Lex.IsNullOrEmpty(qc.Criteria))
						{
							psc = new ParsedSingleCriteria();
							psc.Op = "in";
							psc.ValueList = new List<string>();
						}

						else
						{
							psc = MqlUtil.ParseSingleCriteria(qc.Criteria);
							if (Lex.Eq(psc.Op, "in")) { } // already a list
							else
							{
								psc.Op = "in";
								psc.ValueList = new List<string>();
								if (psc.OpEnum == CompareOp.Eq) // add lib id if current equality
									psc.ValueList.Add(psc.Value);
							}
						}

						tok = libId.ToString();
						for (i1 = 0; i1 < psc.ValueList.Count; i1++) // see if in list
							if (Lex.Eq(psc.ValueList[i1], tok)) break;

						if (i1 >= psc.ValueList.Count)
							psc.ValueList.Add(tok);
						MqlUtil.ConvertParsedSingleCriteriaToQueryColumnCriteria(psc, qc);

						QueryBuilderControl.QueryTablesControl.Render(qt); // render it
					}

					//******************************************************************************
					// Query builder commands
					//******************************************************************************

					else if (Lex.Eq(commandName, "QueryOptions"))
					{
						ti = QueryBuilderControl.Qti;

						if (QueryOptionsDialog.Show(Query, commandArg1, ref ti) != DialogResult.Cancel)
						{ // redraw if table positions adjusted
							QbUtil.RenderQuery(ti); // show new current tab
						}
						if (ti >= 0 && ti < Query.Tables.Count)
							Qt = Query.Tables[ti];
					}

					else if (Lex.Eq(commandName, "DuplicateQuery"))
					{
						QbUtil.DuplicateQuery();
						continue;
					}

					else if (Lex.Eq(commandName, "RemoveAllCriteria"))
					{
						DialogResult dr = MessageBoxMx.Show(
							"Are you sure you want to clear all criteria in the current query?", UmlautMobius.String,
							MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

						if (dr != DialogResult.Yes) return "";

						if (DocumentList.Count <= 0 || Query.Tables.Count <= 0) return "";

						foreach (QueryTable qt2 in Query.Tables)
						{

							foreach (QueryColumn qc2 in qt2.QueryColumns)
								qc2.CriteriaDisplay = qc2.Criteria = "";
						}

						Query.KeyCriteriaDisplay = Query.KeyCriteria = "";

						QbUtil.RenderQuery(Qt);
					}

					else if (Lex.Eq(commandName, "ClearQuery"))
					{
						QbUtil.ResetQuery(true);
						continue;
					}

					else if (Lex.Eq(commandName, "RenameQuery"))
					{
						QbUtil.RenameQuery();
						continue;
					}


					//******************************************************************************
					// Exit app after prompting for save of any files
					//******************************************************************************

					else if (Lex.Eq(commandName, "Exit"))
					{
						if (SessionManager.Instance != null)
							SessionManager.Instance.ShutDown();
					}

					//******************************************************************************
					// See if command line command from script or initial command
					//******************************************************************************

					else
					{
						response = CommandLine.Execute(command);
					}

					Progress.Hide();

					// Display or log any response message

					if (!String.IsNullOrEmpty(response) && !Lex.StartsWith(response, "Command "))
					{
						MessageBoxMx.Show(response);
					}
				} // end of command loop

				return response;
			}

// Catch any unhandled exceptions

			catch (Exception ex)
			{
				string msg = "Unexpected error in ExecuteCommand: \"" + commandMixed + "\" \r\n\r\n" +
					DebugLog.FormatExceptionMessage(ex) + "\r\n=========\r\n" +
					new StackTrace(true); // add local context as well

				ServicesLog.Message(msg);
				MessageBoxMx.ShowError(msg);

				if (!Lex.IsNullOrEmpty(ScriptLog.FileName))
					ScriptLog.Message("> " + msg);

				return "";
			}
		}

		/// <summary>
		/// Parse out the next token for a userobject right-click command & return the user object
		/// </summary>
		/// <param name="command"></param>
		/// <param name="mtn"></param>
		/// <param name="uoArray"></param>
		/// <returns></returns>
		public static bool ParseContentsTreeCommandAndReadUserObject(
			string command,
			out MetaTreeNode mtn,
			out UserObject[] uoArray)
		{
			string commandName;
			UserObject uo;
			bool notFound = false;
			uoArray = null;
			mtn = null;
			MetaTreeNode[] mtnArray;

			ParseContentsTreeCommand(command, out commandName, out mtnArray);

			List<UserObject> uoList = new List<UserObject>();

			foreach (var mtn2 in mtnArray)
			{
				// mtn is only needed if it is a parent.  For example, if we are pasting multiple nodes into a parent folder.
				// So to avoid confusion, only return the meteNode when there is one metanode (e.g. parent), this is the only time
				// it is needed. However, continue to validate each object if there are more than one.
				if (mtnArray.Length == 1) mtn = mtn2;

				if (mtn2 == null)
				{
					MessageBoxMx.ShowError("Unable to find user object for command:\r\n" + command);
					return false;
				}

				int id = UserObject.ParseObjectIdFromInternalName(mtn2.Target);
				uo = UserObjectDao.ReadHeader(id);
				if (!Permissions.UserHasReadAccess(SS.I.UserName, uo))
				{
					MessageBoxMx.ShowError(
							"Unable to retrieve user object.\r\n" +
							GetUserObjectReadAccessErrorMessage(id, "object"));
					return false;
				}
				uoList.Add(uo);
			}
			uoArray = uoList.ToArray();

			return true;
		}

		/// <summary>
		/// Get error message associated with failed read access (i.e. no object or not authorized for read)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="objectTypeName"></param>
		/// <returns></returns>

		public static string GetUserObjectReadAccessErrorMessage(
			int id,
			string objectTypeName)
		{
			if (!Permissions.UserObjectExists(id))
				return "The " + objectTypeName + " does not exist.";

			if (!Permissions.UserHasReadAccess(SS.I.UserName, id))
				return "Your account is not authorized for read access to this " + objectTypeName + ".";

			return "You should have access to this " + objectTypeName + ".";
		}

		/// <summary>
		/// Parse out a command on the contents tree of the form: command nodeName
		/// </summary>
		/// <param name="command"></param>
		/// <param name="commandName"></param>
		/// <param name="mtnArray"></param>

		public static void ParseContentsTreeCommand(
			string command,
			out string commandName,
			out MetaTreeNode[] mtnArray)
		{
			MetaTreeNode mtn;
			Lex lex = new Lex();
			lex.OpenString(command);
			commandName = lex.Get();
			if (String.IsNullOrEmpty(commandName)) throw new Exception("Invalid contents tree command: " + command);

			string args = lex.GetRestOfLine().Trim();
			args = args.Split(' ').Last();
			//if (args != "") lex.Backup();
			//string mtnName = lex.Get(); // node name (or type)
			//if (!Lex.IsNullOrEmpty(lex.Peek())) mtnName = lex.Get(); // get node name
			//if (String.IsNullOrEmpty(mtnName)) throw new Exception("Invalid contents tree command: " + command);

			string[] result;
			string[] stringSeparators = new string[] { ";" };
			result = args.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

			List<MetaTreeNode> mtnList = new List<MetaTreeNode>();


			foreach (string mtnName2 in result)
			{
				mtn = MetaTreeNodeCollection.GetNode(mtnName2);

				if (mtn == null) // not in tree, see if MetaTable name
				{
					MetaTable mt = MetaTableCollection.Get(mtnName2);
					if (mt != null) // if found create a temporary MTN that can be used
					{
						mtn = new MetaTreeNode(MetaTreeNodeType.MetaTable);
						mtn.Name = mtn.Target = mt.Name;
						mtn.Label = mt.Label;
					}
				}

				if (mtn != null)
				{
					mtnList.Add(mtn);
				}
				else
				{
					throw new Exception("MetaTree node not found: " + mtnName2);
				}
			}

			mtnArray = mtnList.Count > 0 ? mtnList.ToArray() : null;

			//mtn = MetaTree.GetNode(mtnName); 
			//if (mtn == null) // check user object tree if not in main tree
			//    mtn = UserObjectTree.GetNode(mtnName);

			//if (mtn == null) // try target using single token
			//    mtn = MetaTree.GetNodeByTarget(mtnName);
			//if (mtn == null) // try target using rest of line
			//    mtn = MetaTree.GetNodeByTarget(args);
			//if (mtn == null) // try UserObject target
			//    mtn = UserObjectTree.GetNodeByTarget(mtnName);

			//if (mtn == null) throw new Exception("MetaTree node not found: " + mtnName)   ; // (catch later)


		}

		/// <summary>
		/// Initialize PendingCommand timer
		/// </summary>

		public static void Initialize()
		{
			if (PendingCommandTimer != null) return;
			CommandExec.Instance = new CommandExec();
			PendingCommandTimer = new Timer();
			PendingCommandTimer.Tick += new System.EventHandler(CommandExec.Instance.PendingCommandTimer_Tick);
			PendingCommandTimer.Start();
			return;
		}

		/// <summary>
		/// Post a command for execution (from non UI thread)
		/// </summary>
		/// <param name="command"></param>

		public static void PostCommand(string command)
		{
			if (Instance == null || PendingCommandTimer == null) return;

			PendingCommand = command;

			Form form = SessionManager.ActiveForm;

			if (form != null) // start the timer from the UI thread
				form.Invoke(new SimpleDelegate(CommandExec.Instance.StartCommandTimer));
			return;
		}

		/// <summary>
		/// Method called under UI thread to start the timer
		/// </summary>

		void StartCommandTimer()
		{
			PendingCommandTimer.Start();
		}

		/// <summary>
		/// Timer has ticked, process any pending command
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void PendingCommandTimer_Tick(object sender, EventArgs e)
		{
			PendingCommandTimer.Stop();
			if (String.IsNullOrEmpty(PendingCommand)) return;

			if (InPendingCommandTimer_Tick) return;
			InPendingCommandTimer_Tick = true;

			SessionManager.ActivateShell();
			string result = ExecuteCommand(PendingCommand);
			PendingCommand = null;
			SessionManager.ActivateShell();

			InPendingCommandTimer_Tick = false;
			return;
		}

		/// <summary>
		/// EditContentsTree
		/// </summary>
		/// <param name="nodeName"></param>
		/// <returns></returns>
		/// 
		public static bool EditContentsTree(string nodeName)
		{
			ContentsTreeEditorDialog cte = null;
			DialogResult dr = DialogResult.Cancel;
			MetaTreeNode mtn = null;
			string topNodeTarget = "", expandNodeTarget = "";

			if (Lex.IsDefined(nodeName))
			{
				string[] sa = nodeName.Split('.'); // split qualified node name
				string nodeName2 = sa[sa.Length - 1];
				mtn = MetaTree.GetNode(nodeName2);
			}

			try
			{
				cte = new ContentsTreeEditorDialog(nodeName);
				dr = cte.ShowDialog();
				if (dr == DialogResult.Cancel) return false;
				MetaTree.MetaTreeFactory.MarkCacheForRebuild();
			}
			finally { MetaTreeFactory.ReleaseMetaTreeXml(); } // be sure Root.xml is released

			string rootXml = cte.SavedRootXml;

			string tempFile = TempFile.GetTempFileName("xml");
			StreamWriter sw = new StreamWriter(tempFile);
			sw.Write(rootXml);
			sw.Close();

			MetaTree.Nodes = MetaTree.MetaTreeFactory.BuildQuickMetaTree(tempFile); // rebuild & replace nodes

			try { File.Delete(tempFile); }
			catch { }

			if (mtn != null)
			{
				topNodeTarget = mtn.Target;
				expandNodeTarget = mtn.Target;
			}

			MetaTreeNodeType typeFilter = MetaTreeNodeType.All;
			bool numberItems = true;
			bool showAllObjTypeFolderNames = true;
			CommandExec.QbContentsTree.QbContentsTreeCtl.FillTree("root", typeFilter, topNodeTarget, expandNodeTarget, "", numberItems, showAllObjTypeFolderNames);

			return true;
		}

	}
}
