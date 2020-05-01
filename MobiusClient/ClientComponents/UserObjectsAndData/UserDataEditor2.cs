using Mobius.ComOps;
using Mobius.Data;
using Mobius.CdkMx;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Mail;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using DevExpress.XtraSplashScreen;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Class that does editing of user data definition
	/// </summary>

	public partial class UserDataEditor
	{
		public bool InEditor = false; // true if in UserDataEditor form

		public bool AnnotationTable = false; // creating/editing an annotation table
		public bool UserDatabase = false; // creating/editing a user database
		string UserDataTypeLabel { get { return (UserDatabase ? "database" : "annotation table"); } }

		UserObject Uo; // user object for annotation table or user compound database
		UserObject SUo; // structure table user object for user compound database
		UserObject AUo; // first annotation user object for user compound database
		UserObject MUo; // user object for selected models 

		public MetaTable SMt; // structure table metatable
		public MetaTable AMt; // annotation table metatable
		public MetaTable CMt; // combined structure & annotation tables

		UserCmpndDbDao Udbs; // User database service instance for accessing Ucdb
		UcdbDatabase Udb; // user database being edited
		List<UcdbModel> Models; // list of models assoc with db

		internal QueryManager QueryManager; // query manager for display of data
		QueryManager Qm { get { return QueryManager; } } // query manager for display of data
		MoleculeGridControl Grid { get { return Qm.MoleculeGrid; } } // manager for table containing data
		ResultsFormat Rf { get { return Qm.ResultsFormat; } } // format associated with query
		QueryEngine Qe { get { return Qm.QueryEngine; } } // query engine instance used as datasource retrieving existing data

		DataTableMx DataTable { get { return Qm.DataTable; } } // table containing data
		string DataTableColumnsString { get { return Qm.DataTable.ColumnsString; } } // for debug
		string DataTableRowsString { get { return Qm.DataTable.RowsString; } } // for debug

		DataTableManager Dtm { get { return Qm.DataTableManager; } } // manager for table containing data

		DataTable ColGridDataTable; // DataTable for ColGrid
		int ColGridDataTablePos(string colName) { return ColGridDataTable.Columns[colName].Ordinal; }

		DataTableMx RowGridDataTable { get { return DataTable; } } // DataTable of the data itself
		DataTable ModelsDataTable; // DataTable of selected models

		int AttrVoPos { get { return Dtm.RowAttributesVoPos; } }
		int KeyVoPos { get { return Dtm.KeyValueVoPos; } }
		UserDataImportParms ImportParms = null; // import parameters
		string ActualImportFileName = ""; // real file being imported, can be original file, file cached on server and/or .csv file created from .xlsx file
		bool TurnedCheckForFileUpdatesOff = false;
		CidList ExistingCids; // dictionary of persisted cids for ucdb/annotation table
		DataTable SDt; // structure data table
		DataTable ADt; // annotation data table
		long MaxCid = -1; // max numeric compoundId assigned to ucdb

		bool DisplayDataTabSelected; // true if Display Data tab is currently selected
		bool DataFormatterOpen; // data currently displayed in data tab

		bool ColumnsModified; // column definitions modified since last save
		bool ModelsModified; // annotation/ucdb modified since last save
		bool Modified // union of modification flags
		{
			get { return ColumnsModified || Dtm.DataModified || ModelsModified; }
			set { ColumnsModified = Dtm.DataModified = ModelsModified = value; }
		}

		bool InteractiveMode = true; // true if in interactive mode with the UserDataEditor visible
		bool Saved; // annotation/ucdb saved in this session
		UserObject ImportStateUo = null; // userobject containing import state
		DateTime ImportStateLastCheckPoint; // last checkpoint update
		string ImportMessages = ""; // messages accumulated during import/save

		static int TempNameIndex; // used for assigning unique temp names

		static MainMenuControl MainMenuControl { get { return SessionManager.Instance.MainMenuControl; } }

		/// <summary>
		/// Edit a new or existing user database or annotation table
		/// </summary>
		/// <param name="internalName"></param>
		/// <returns></returns>

		public static UserObject Edit(string internalName)
		{
			UserObject uo = UserObject.ParseInternalUserObjectName(internalName, "");
			if (uo != null) uo = UserObjectDao.Read(uo.Id);
			if (uo == null) throw new Exception("User object not found " + internalName);
			UserDataEditor ude = new UserDataEditor();
			UserObject uo2 = ude.Edit(uo);
			return uo2;
		}

		/// <summary>
		/// Edit a new or existing user database or annotation table
		/// </summary>
		/// <param name="userObj"></param>
		/// <returns></returns>

		public UserObject Edit(
				UserObject userObj)
		{
			UcdbCompound cpd;
			UcdbAlias alias;
			UcdbModel model;
			UcdbModel annotation;
			UserObject uo, uo2;
			MetaTable rootMt;
			MetaColumn mc;
			string extIdTxt;
			string name, txt, prefix;
			int objectId, i1, i2;
			int cci, ci; // current molecule index

			try
			{
				Uo = userObj.Clone(); // make copy

				CreateColDefDataTable();
				ColGrid.DataSource = ColGridDataTable;
				CreateModelsDataTable();
				SelectedModelsGrid.DataSource = ModelsDataTable;

				string modelsRoot = ServicesIniFile.Read("InsilicoModelsRoot", "insilico_data");
				MetaTreeNode mtn = MetaTree.GetNode(modelsRoot);
				if (mtn != null)
				{
					ModelTree.Nodes = new Dictionary<string, MetaTreeNode>();
					ModelTree.Nodes.Add(mtn.Name, mtn); // add full set?
					MetaTreeNodeType mtnFilter =
					 MetaTreeNodeType.Project | // show project folders and above
					 MetaTreeNodeType.MetaTable; // and personal folders
					try { ModelTree.FillTree(modelsRoot, mtnFilter, null, null, null, false, false); }
					catch (Exception ex) { string message = ex.Message; }
				}

				QueryManager = new QueryManager();
				Qm.MoleculeGrid = RowGrid;
				RowGrid.QueryManager = Qm;
				Qm.MoleculeGrid.Editor = this; // set us as the current editor for the grid
				Qm.MoleculeGrid.Mode = MoleculeGridMode.DataSetView; // set mode for use
				Qm.MoleculeGrid.UpdateImmediately = false; // hold updated until explicitly saved

				if (userObj == null || (userObj.Type != UserObjectType.Annotation && userObj.Type != UserObjectType.UserDatabase))
					throw new Exception("Expected Annotation or UserDatabase user object");

				if (Uo.Id > 0 && MainMenuControl != null) // update MruList if object defined
					MainMenuControl.UpdateMruList(Uo.InternalName);

				// Annotation table cases:
				// 1. New table for an existing user database - set containing folder to ucdb folder
				// 2. New table for a general non-user database - 
				// 3. Existing annotation table 
				// 4. Existing user database structure table - switch to edit of ucdb

				if (Uo.Type == UserObjectType.Annotation)
				{
					if (Uo.Id <= 0) // new annotation
					{
						rootMt = QbUtil.CurrentQueryRoot;
						if (rootMt != null) // use root of first table in current query
						{
							if (rootMt.IsUserDatabaseStructureTable)
							{
								objectId = UserObject.ParseObjectIdFromInternalName(rootMt.Name);
								uo2 = UserObjectDao.Read(objectId); // read in user structure metatable object & proceed as if editing this
								if (uo2 != null) Uo.ParentFolder = uo2.ParentFolder; // so this annotation is saved to the proper folder by default
							}
						}
					}

					else // if editing existing structure table then edit as if editing associated user compound database
					{
						MetaTable mt = MetaTableCollection.Get("USERDATABASE_STRUCTURE_" + Uo.Id);
						MetaTreeNode folderNode = UserObjectTree.FindFolderNode(Uo.ParentFolder);
						if (mt != null && folderNode != null && mt.IsUserDatabaseStructureTable)
						{
							objectId = UserObject.ParseObjectIdFromInternalName(folderNode.Name);
							Uo = UserObjectDao.Read(objectId); // read in higher level user database object
						}
					}
				}

				// Prep for editing depending on type

				if (Uo.Type == UserObjectType.Annotation)
					PrepareForAnnotationTableEdit();

				else PrepareForUserDatabaseEdit();

				BuildCMtFromMetaTables();
				FillColDefDataTable(CMt);

				Tabs.SelectedTabPageIndex = 0; // select 1st tab
				DisplayDataTabSelected = false; // data tab not selected
				DataFormatterOpen = false; // indicate data needs to be displayed

				FillModelsGrid(Models); // show selected models if ucdb

				InEditor = true;
				ShowDialog(SessionManager.ActiveForm);
				InEditor = false;

				if (Saved) return Uo; // return saved user object
				else return null; // nothing saved
			} // end of try

			catch (Exception ex)
			{
				MessageBoxMx.ShowError(DebugLog.FormatExceptionMessage(ex));
				return null;
			}
		}

		/// <summary>
		/// Create the table of column information
		/// </summary>

		void CreateColDefDataTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn("Label", typeof(string)));
			dt.Columns.Add(new DataColumn("DataType", typeof(string)));
			dt.Columns.Add(new DataColumn("DisplayByDefault", typeof(bool)));

			dt.Columns.Add(new DataColumn("CustomFormat", typeof(Bitmap)));
			dt.Columns.Add(new DataColumn("PositionInImportFile", typeof(string)));
			dt.Columns.Add(new DataColumn("Name", typeof(string)));
			dt.Columns.Add(new DataColumn("ResultCode", typeof(string)));
			dt.Columns.Add(new DataColumn("MetaColumn", typeof(MetaColumn)));

			dt.RowChanged += new DataRowChangeEventHandler(ColDefDataTable_RowChanged);

			ColGridDataTable = dt;
		}

		/// <summary>
		/// If row added set DisplayByDefault to true if null
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void ColDefDataTable_RowChanged(object sender, DataRowChangeEventArgs e)
		{
			if (InSetup) return;

			if (e.Action == DataRowAction.Add && e.Row["DisplayByDefault"] == DBNull.Value)
			{
				e.Row["DisplayByDefault"] = true;
				SetCustomFormattingGridImage(e.Row);
			}

			ColumnsModified = true;
			DataFormatterOpen = false; // display no longer valid
		}

		/// <summary>
		/// Create table of models info
		/// </summary>

		void CreateModelsDataTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn("Model", typeof(string)));
			dt.Columns.Add(new DataColumn("ModelId", typeof(long)));

			ModelsDataTable = dt;
		}

		/// <summary>
		/// Prepare to edit a single annotation table
		/// </summary>

		void PrepareForAnnotationTableEdit()
		{
			string name;

			AnnotationTable = true;
			UserDatabase = false;

			if (Uo.Name == "") // new annotation table
			{
				AMt = new MetaTable();
				AMt.MetaBrokerType = MetaBrokerType.Annotation;
				AMt.Code = "0"; // assigne zero code so queries will work but return no data

				Query q = QbUtil.Query;
				if (q != null && q.Tables.Count > 0) // use root of first table in current query as root for annotation
					ParentMt = q.Tables[0].MetaTable.Root;

				else // if can't get from query, use first structure table in structure_database dict
					ParentMt = MetaTableCollection.Get(RootTable.GetList()[0].MetaTableName);

				AMt.Parent = ParentMt;
				MetaColumn mc = AMt.AddMetaColumn(ParentMt.KeyMetaColumn.Name, ParentMt.KeyMetaColumn.Label, ParentMt.KeyMetaColumn.DataType);
				mc.Format = ColumnFormatEnum.Decimal; // set format for integer keys
				mc.Decimals = 0;
			}

			else
			{
				Uo = UserObjectDao.Read(Uo);
				AMt = MetaTable.Deserialize(Uo.Content);
			}

			SetupForAnnotationTable();

			Description.Text = AMt.Description;

			List<string> rootLabels = MetaTableFactory.GetRootTableLabels();
			ToolHelper.SetListControlItemsFromDictionary // setup dropdown of root tables
					(ParentCompoundCollection, "Root_Tables", true);

			string parentDb = "";
			if (AMt.Parent != null)
			{
				ParentMt = AMt.Parent;
				RootTable rti = RootTable.GetFromTableName(AMt.Parent.Name);
				if (rti != null) parentDb = rti.Label;
				else if (AMt.Parent.IsUserDatabaseStructureTable)
				{ // lookup user database name
					UserCmpndDbDao udbs = new UserCmpndDbDao();
					UcdbDatabase ucdb = udbs.SelectDatabaseHeader(long.Parse(AMt.Parent.Code));
					if (ucdb != null) parentDb = "UserDatabase: " + ucdb.Name;
				}
				else throw new Exception("Can't identify database associated with parent table: " + AMt.Parent.Name);
			}

			InSetup = true;
			ParentCompoundCollection.Text = parentDb;
			InSetup = false;

			if (Uo.Name == "") name = "New";
			else name = Uo.Name;
			Text = "Edit Annotation Table - " + name;
			return;
		}

		void SetupForAnnotationTable()
		{
			if (Tabs.TabPages.Contains(ModelsTab)) // be sure model page is not visible
				Tabs.TabPages.Remove(ModelsTab);

			//HideStructures.Visible = false;
			//AllowDuplicateStructures.Visible = false;
			ImportFileContextMenu.Tag = null; // null to not show menu

			CompoundCollectionLabel.Visible = true;
			ParentCompoundCollection.Visible = true;
			AnnotationTable = true;
			UserDatabase = false;
		}

		void SetupForUserDatabase()
		{
			//if (!Tabs.TabPages.Contains(ModelsTab)) // be sure model page is there
			//  Tabs.TabPages.Add(ModelsTab);

			if (Tabs.TabPages.Contains(ModelsTab)) // models disabled for now
				Tabs.TabPages.Remove(ModelsTab);

			//HideStructures.Visible = true;
			//AllowDuplicateStructures.Visible = true;
			ImportFileContextMenu.Tag = true; // mark to show

			CompoundCollectionLabel.Visible = false;
			ParentCompoundCollection.Visible = false;
			UserDatabase = true;
			AnnotationTable = false;
		}


		/// <summary>
		/// Setup for user compound database editing
		/// </summary>

		void PrepareForUserDatabaseEdit()
		{
			string prefix, name;
			int id;

			UserDatabase = true;
			AnnotationTable = false;

			Udbs = new UserCmpndDbDao(); // user compound database database service instance
			AMt = null;

			if (Uo.Name == "") // new database
			{
				SMt = GetStructureTablePrototype();
				Udb = new UcdbDatabase();
				Udb.CompoundIdType = CompoundIdTypeEnum.String; // store all ids as strings
				Models = new List<UcdbModel>();
			}

			else // existing database
			{
				Uo = UserObjectDao.Read(Uo); // get db user object that contains the databaseId
				InitializeMembersForExistingUserDatabase(Uo);
			}

			SetupForUserDatabase();
			Description.Text = SMt.Description;

			if (Uo.Name == "") name = "New";
			else name = Uo.Name;
			Text = "Edit User Database - " + name;
			return;
		}

		/// <summary>
		/// Initialize instance vars for existing user database
		/// </summary>
		/// <param name="Uo"></param>
		/// 
		void InitializeMembersForExistingUserDatabase(
				UserObject udbUo)
		{
			Uo = udbUo; // set top level to the user database object
			long databaseId = Int64.Parse(udbUo.Content);
			Udb = Udbs.SelectDatabaseHeader(databaseId); // get headers & models now 
			Udb.Models = Udbs.SelectDatabaseModels(databaseId);

			List<UserObject> luo = GetUcdbUserObjects(udbUo.Id); // get user objects under database in tree
			if (luo != null)
				foreach (UserObject uo2 in luo)
				{
					if (uo2.Type == UserObjectType.Annotation) // structure or annotation table
					{
						MetaTable mt2 = MetaTable.Deserialize(uo2.Content);
						if (mt2.IsUserDatabaseStructureTable) // db structure table
						{
							SUo = uo2;
							SMt = mt2;
						}

						else if (AMt == null) // 1st regular annotation table
						{ // first annotation table
							AUo = uo2;
							AMt = mt2;
						}
					}
				}

			if (SMt == null) // failed to find ucdb structure table, make up a reasonable version
				SMt = GetStructureTablePrototype();

			MetaTableCollection.UpdateGlobally(SMt); // make this metatable available for querying
			if (AMt != null) MetaTableCollection.UpdateGlobally(AMt); // also add any annotation table

			Models = new List<UcdbModel>(Udb.Models); // copy models to list
			return;
		}

		/// <summary>
		/// Get a list of all of the user objects within the a ucdb folder
		/// </summary>
		/// <param name="dbFolderUserObjectId"></param>
		/// <returns></returns>

		public static List<UserObject> GetUcdbUserObjects(
				int dbFolderUserObjectId)
		{
			List<UserObject> luo = new List<UserObject>();

			MetaTreeNode mtn = UserObjectTree.FindFolderNode("USERDATABASE_" + dbFolderUserObjectId);
			if (mtn == null) return null;

			foreach (MetaTreeNode mtn2 in mtn.Nodes)
			{
				try
				{
					string itemId = mtn2.Target.ToUpper();
					int i1 = itemId.LastIndexOf("_");
					int objId = Int32.Parse(itemId.Substring(i1 + 1));
					UserObject uo2 = UserObjectDao.Read(objId);
					if (uo2 != null) luo.Add(uo2);
				}
				catch (Exception ex) { string msg = ex.Message; }
			}

			return luo;
		}


		/// <summary>
		/// Save annotation table
		/// </summary>
		/// <param name="aMt"></param>
		/// <returns></returns>

		bool SaveAnnotationTableDialog()
		{
			UserObject uo, uo2;
			MetaColumn mc;
			bool cancelSaveUserData;
			int i1;

			if (Uo == null || Uo.Name == "")
			{
				uo = new UserObject(UserObjectType.Annotation);
				uo.ParentFolder = Uo.ParentFolder; // default to folder for database
				uo = UserObjectSaveDialog.Show("Save Annotation", uo);
				if (uo == null) return false;
				Uo = uo;
				Uo.Id = UserObjectDao.GetNextId();
			}

			else if (!UserObjectUtil.UserHasWriteAccess(Uo))
			{
				MessageBoxMx.ShowError("You are not authorized to save " + Lex.Dq(Uo.Name));
				return false;
			}

			Progress.Show("Saving annotation table...");
			AMt = CMt; // annotation is same as "combined" table
			AMt.Description = Description.Text;
			SaveAnnotationTableUserObject(Uo, AMt);

			SaveImportState(out cancelSaveUserData);

			if (!cancelSaveUserData)
				SaveUserDataWithImportCheck();

			SaveComplete();
			Progress.Hide();

			Text = "Edit Annotation Table - " + Uo.Name;

			//if (SS.I.Attended) MessageBoxMx.Show("You have successfully saved: " + Uo.Name); // (not needed)

			return true;
		}

		/// <summary>
		/// Save database
		/// </summary>
		/// <returns></returns>

		bool SaveDatabaseDialog()
		{
			UserObject uo, uo2;
			bool cancelSaveUserData;

			UserCmpndDbDao udbs = new UserCmpndDbDao();

			List<UserObject> existingUcdbUos = null; // list of existing objects for user database

			if (Uo == null || Uo.Name == "") // prompt for name & do initial save if new db
			{
				uo = new UserObject(UserObjectType.UserDatabase);
				uo = UserObjectSaveDialog.Show("Save User Compound Database", uo);
				if (uo == null) return false;

				Uo = uo;
				Uo.Id = UserObjectDao.GetNextId(); // assign id so other user objects can reference us as parent folder

				Udb.OwnerUserName = Uo.Owner;
				Udb.Name = Uo.Name;
				Udb.NameSpace = Uo.ParentFolder;
				if (Uo.AccessLevel == UserObjectAccess.Public)
					Udb.Public = true;
				else Udb.Public = false;

				UcdbDatabase db2 = udbs.SelectDatabaseHeader(Udb.OwnerUserName, Udb.NameSpace, Udb.Name);
				if (db2 != null)
				{ // queue any existing version for deletion, new version gets new databaseId
					db2.Name += " - pending deletion " + TimeOfDay.Milliseconds().ToString(); // give unique name to avoid name clash
					db2.PendingStatus = UcdbWaitState.Deletion; // mark for deletion
					udbs.UpdateDatabaseHeader(db2);
					CommandLine.StartBackgroundSession( // start background delete
							"DeleteUserDatabaseData " + db2.DatabaseId);
				}

				udbs.InsertDatabaseHeader(Udb); // initial creation of db
			}

			else // get existing objects for db including all existing annotation tables
				existingUcdbUos = UserDataEditor.GetUcdbUserObjects(Uo.Id);

			Progress.Show("Saving user compound database...");

			Uo.Content = Udb.DatabaseId.ToString(); // save/update db UserObject content is just the db Id
			UserObjectDao.Write(Uo, Uo.Id);
			MainMenuControl.UpdateMruList(Uo.InternalName);

			Udb.Description = SMt.Description = CMt.Description = Description.Text;

			SaveDatabaseStructureMetaData(); // save db structure table definition UserObject

			SaveDatabaseAnnotationMetaData(existingUcdbUos); // save any associated annotation table

			SaveModelsMetaData(); // save MultiTable user object for models & parallel UCDB database entries

			SaveImportState(out cancelSaveUserData);
			if (!cancelSaveUserData)
				SaveUserDataWithImportCheck();  // save any structure/annotation data  (if ImportInBackground just start background process)

			udbs.UpdateDatabaseHeader(Udb); // Update db header with latest counts & status

			//if (Udb.CompoundCount != SUo.Count) // be sure count is up to date in structure table user object
			//  UserObjectDao.UpdateCount(SUo.Id, Udb.CompoundCount);

			SaveComplete();

			Progress.Hide();

			Text = "Edit User Database - " + Udb.Name;
			return true;
		}

		/// <summary>
		/// Save is complete, reset flags & data display
		/// </summary>

		void SaveComplete()
		{
			Saved = true;
			Modified = false; // clear modified flags

			if (ImportParms == null || ImportParms.ImportInBackground) return;

			ImportParms = null;
			DataFormatterOpen = false; // for redisplay of data from database rather than import file

			if (DisplayDataTabSelected) // reset for full display rather than just import display
				OpenDataFormatter();

			return;
		}

		/// <summary>
		/// Save db structure table definition UserObject
		/// </summary>
		/// <param name="uo">Structure table user object</param>
		/// <param name="mt"></param>

		void SaveDatabaseStructureMetaData()
		{

			// The structure table is an annotation user object with a name prefix of
			// USERDATABASE_STRUCTURE_ followed by the object id. 
			// It uses the generic Metabroker rather than the annotation Metabroker
			// In the user object tree its parent is the UserDatabase user object. 

			if (SUo == null)
			{
				SUo = new UserObject(UserObjectType.Annotation); // save as annotation object 
				SUo.Id = UserObjectDao.GetNextId();
				SUo.Name = "Structures";
				SUo.Owner = SS.I.UserName;
				SUo.ParentFolderType = FolderTypeEnum.User; // user object in user folder
				SUo.ParentFolder = "USERDATABASE_" + Uo.Id; // link to parent db
				SUo.AccessLevel = Uo.AccessLevel;

				SMt.Name = "USERDATABASE_STRUCTURE_" + SUo.Id;
				SMt.Code = Udb.DatabaseId.ToString(); // user database id for code
				SMt.TableMap = // select statement
						"(select " +
						" lc.ext_cmpnd_id_txt, " +
						" c.mlcl_struct, " +
						" c.mlcl_frml, " +
						" c.mlcl_wgt, " +
						" c.updt_date molecule_date " +
						"from " +
						" ucdb_owner.ucdb_db_cmpnd lc, " +
						" ucdb_owner.ucdb_cmpnd c " +
						"where " +
						" lc.db_id = " + SMt.Code + " and " +
						" c.cmpnd_id = lc.cmpnd_id and " +
						" c.pndng_sts <> " + (int)UcdbWaitState.Deletion + ")"; // don't select compounds waiting for deletion
				MetaTableCollection.UpdateGlobally(SMt); // update map with modified metatable
			}

			else // read existing user object
			{
				string mtName = "USERDATABASE_STRUCTURE_" + SUo.Id;
				SMt = MetaTableCollection.Get(mtName);
				if (SMt == null) throw new Exception("Metatable " + mtName + " not found");
			}

			if (ImportParms != null) SMt.ImportParms = ImportParms.Clone();
			SUo.Content = SMt.Serialize();

			UserObjectDao.Write(SUo, SUo.Id);
			return;
		}

		/// <summary>
		/// Save annotation table metadata if table exists
		/// </summary>

		void SaveDatabaseAnnotationMetaData( // save any associated annotation table
				List<UserObject> existingUcdbUos)
		{
			SyncCMtWithColGrid();
			SyncAMtWithCMt();
			if (AMt == null) return; // if no annotation table then just return

			if (AUo == null) // create user object if new table
			{
				AUo = new UserObject(UserObjectType.Annotation);
				AUo.Id = UserObjectDao.GetNextId();
				AUo.Name = AMt.Label;
				AUo.Owner = Uo.Owner;
				AUo.ParentFolderType = FolderTypeEnum.User; // user object in user folder
				AUo.ParentFolder = "USERDATABASE_" + Uo.Id; // link to parent db
				AUo.AccessLevel = Uo.AccessLevel;
			}

			if (ImportParms != null) AMt.ImportParms = ImportParms.Clone();
			SaveAnnotationTableUserObject(AUo, AMt);

			// Rewrite any additional annotation tables so they appear in the contents

			if (existingUcdbUos != null)
			{
				foreach (UserObject uo2 in existingUcdbUos)
				{
					if (uo2.Type != UserObjectType.Annotation) continue;
					if (uo2.Id == SUo.Id) continue; // already saved structure table?
					if (uo2.Id == AUo.Id) continue; // already saved primary annotation table?

					UserObjectDao.Write(uo2, uo2.Id); // rewrite other annotation table to get back into tree
				}
			}
		}

		/// <summary>
		/// Save user object containing annotation table definition
		/// </summary>
		/// <param name="uo"></param>
		/// <param name="mt"></param>
		/// <returns></returns>

		void SaveAnnotationTableUserObject(
				UserObject uo,
				MetaTable mt)
		{
			UserObject uo2;

			mt.Name = "ANNOTATION_" + uo.Id; // create metatable name from Id
			AnnotationDao aDao = new AnnotationDao();

			if (mt.Code == "" || mt.Code == "0") // if new see if overwriting existing table
			{
				mt.Code = uo.Id.ToString();
				uo2 = UserObjectDao.ReadHeader(uo);
				if (uo2 != null && uo2.Type == UserObjectType.Annotation) // if creating new table and table with same name exists
				{ // then delete any data associated with the old table
					CommandLine.StartBackgroundSession( // start background delete
							"DeleteAnnotationTableData " + uo2.Id);
				}
			}

			CompleteAnnotationTableMetaColumnDefinitions(mt);

			aDao.Dispose();

			mt.Label = uo.Name; // metatable label same as object name
			uo.Content = mt.Serialize();

			if (AnnotationTable) // if annotation table then store in an annotations folder rather than under the user database folder
				UserObjectTree.GetValidUserObjectTypeFolder(uo);

			UserObjectDao.Write(uo, uo.Id);

			MetaTableCollection.UpdateGlobally(mt); // update map with modified metatable

			MainMenuControl.UpdateMruList(uo.InternalName);

			return;
		}

		/// <summary>
		/// Complete the definitions of annotation table metacolumns in preparation for saving or querying the table
		/// </summary>
		/// <param name="mt"></param>

		void CompleteAnnotationTableMetaColumnDefinitions(
				MetaTable mt)
		{
			AnnotationDao aDao = new AnnotationDao();

			for (int i1 = 0; i1 < mt.MetaColumns.Count; i1++) // map each col to a result code
			{
				MetaColumn mc = (MetaColumn)mt.MetaColumns[i1];
				if (mc.IsKey)
				{
					if (mc.IsNumeric) mc.ColumnMap = "EXT_CMPND_ID_NBR"; // numeric database column
					else mc.ColumnMap = "EXT_CMPND_ID_TXT"; // string database column
					continue;
				}

				if (mc.ResultCode == "" || mc.ResultCode == "0") // assign result code if new col without code
					mc.ResultCode = aDao.GetNextIdLong().ToString();

				mc.Name = "R_" + mc.ResultCode;
				mc.DetailsAvailable = true; // for hyperlinking & to get result_id needed for editing in grid
			}

			aDao.Dispose();

			BuildCMtFromMetaTables();
			FillColDefDataTable(CMt); // update editor with any new col names & codes

			return;
		}

		/// <summary>
		/// Save list of models and create a multitable query linking to the models
		/// </summary>
		/// <param name="MUo"></param>

		void SaveModelsMetaData()
		{
			QueryTable qt;
			MetaTable mt;

			Models = GetSelectedModels(Models);
			if (ModelsModified)
				Udbs.UpdateDatabaseModelAssoc(Udb, Models.ToArray()); // update oracle table of data associations

			int modelAddCnt = 0;
			int modelDelCnt = 0;
			if (Models.Count > 0) // create/update models MultiTable
			{
				Query mq = new Query();
				qt = new QueryTable(SMt);
				mq.AddQueryTable(qt); // root structure table first
				foreach (UcdbModel lda in Models)
				{
					if (lda.RowState == UcdbRowState.Deleted ||  // don't include if deleted
							lda.PendingStatus == UcdbWaitState.Deletion) // or pending deletion
					{
						modelDelCnt++;
						continue;
					}

					else if (lda.RowState == UcdbRowState.Added) modelAddCnt++;

					string mtName = "SPM_MODEL_" + lda.ModelId; // build metatable name for this model/db
					if (lda.BuildId > 0) mtName += "_BUILD_" + lda.BuildId;
					mtName += "_PARENT_" + SMt.Name;

					mt = MetaTableCollection.Get(mtName); // be sure it's valid
					if (mt == null)
					{
						ServicesLog.Message("SaveModelsMetadata - Failed to get model metatable " + mtName);
						continue;
					}

					qt = new QueryTable(mt);
					mq.AddQueryTable(qt);
				}

				if (mq.Tables.Count > 1) // have query with models
				{
					if (MUo == null) // first time?
					{
						MUo = new UserObject(UserObjectType.MultiTable);
						MUo.Name = "Predictive Models";
						MUo.Owner = Uo.Owner;
						MUo.ParentFolderType = FolderTypeEnum.User; // user object in user folder
						MUo.ParentFolder = "USERDATABASE_" + Uo.Id; // link to parent db
						MUo.AccessLevel = Uo.AccessLevel;
					}

					MUo.Content = mq.Serialize();
					UserObjectDao.Write(MUo, MUo.Id);
				}

				else if (MUo != null) // if no models now delete any existing user object
					UserObjectDao.Delete(MUo.Id);
			}

			Udb.ModelCount = Models.Count - modelDelCnt;

			return;
		}

		/// <summary>
		/// Save import state
		/// </summary>
		/// <param name="cancelSaveUserData">True to cancel the save of data</param>
		/// <returns>True if saved</returns>

		bool SaveImportState(
				out bool cancelSaveUserData)
		{
			string uoName; // name to assign to UserDataImportState user object
			UserDataImportState udis = null;

			cancelSaveUserData = false;

			if (ImportParms == null) return true; // not importing

			if (UserDatabase) uoName = "USERDATABASE_" + Uo.Id;
			else uoName = "ANNOTATION_" + Uo.Id;
			UserObject udisUo = // get any existing state info
					UserObjectDao.ReadHeader(UserObjectType.ImportState, SS.I.UserName, "", uoName);

			if (udisUo != null) // deserialize & check that import isn't already running
			{
				udis = UserDataImportState.Deserialize(udisUo);
				if (udis.ImportIsRunning)
				{ // if import is already running then don't start again
					MessageBoxMx.ShowError(
							"This data import can't be started at the present time because it appears\n" +
							"that data is currently being imported in the background into this " + UserDataTypeLabel);
					cancelSaveUserData = true;
				}
			}

			if (TurnedCheckForFileUpdatesOff)
			{ // delete any import state object && see if user wants to continue with import
				if (udisUo != null) UserObjectDao.Delete(udisUo.Id);

				string msg =
						"It appears that you have turn off automatic re-import.\n" +
						"Do you also want to cancel the import of the file for this one time?";

				DialogResult dr = MessageBoxMx.Show(msg, "Cancel Import", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (dr != DialogResult.No) cancelSaveUserData = true;
			}

			if (!ImportParms.ImportInBackground && // not importing in background 
			 !ImportParms.CheckForFileUpdates)  // and not checking to updates 
				return true; // don't need to save

			if (udisUo == null) // create new ImportState object
			{
				udisUo = new UserObject(UserObjectType.ImportState, SS.I.UserName, uoName);
				udis = new UserDataImportState();
			}

			udis.UserDatabase = UserDatabase; // store type of user data object
			udis.UserDataObjectId = Uo.Id; // store id of user data object
			udis.ClientFile = ImportParms.FileName;
			udis.CheckForFileUpdates = ImportParms.CheckForFileUpdates;
			udis.ClientFileModified = ImportParms.ClientFileModified;
			udis.FileName = ImportParms.FileName;
			udisUo.Description = udis.Serialize(); // serialize to description

			UserObjectDao.Write(udisUo);

			return true;
		}

		/// <summary>
		/// Save any new or modified data associated with user database/annotation table.
		/// Called from database/annotation creation editor.
		/// </summary>
		/// <returns></returns>

		bool SaveUserDataWithImportCheck()
		{
			return SaveUserDataWithImportCheck(true);
		}
		/// <summary>
		/// Save any new or modified data associated with user database/annotation table.
		/// Called from database/annotation creation editor.
		/// </summary>
		/// <returns></returns>

		bool SaveUserDataWithImportCheck(
				bool allowBackgroundImport)
		{

			if (ImportParms == null && !Dtm.DataModified)
				return true; // nothing to do;

			if (ImportParms != null)
			{
				if (allowBackgroundImport && ImportParms.ImportInBackground)
				{
					string internalUoName = (UserDatabase ? "USERDATABASE_" : "ANNOTATION_") + Uo.Id;
					string exportDir = ServicesIniFile.Read("BackgroundExportDirectory");
					string serverFileName = // location for file on server
							exportDir + @"\" + internalUoName + Path.GetExtension(ImportParms.FileName);
					ServerFile.CopyToServer(ImportParms.FileName, serverFileName);
					string command = "ImportUserData " + serverFileName + ", " + internalUoName;

					CommandLine.StartBackgroundSession(command);
					MessageBoxMx.Show(
							"The import has been started in the background.\n" +
							"You'll receive an email notification when it completes.",
							"Import Started");

					return true;
				}

				if (!CheckImportData())
					return false;

				if (Rf == null) // setup for data retrieval & display if not done yet
					OpenDataFormatter();
			}

			SaveUserData(true); // complete save
			return true;
		}

		/// <summary>
		/// Save any new or modified data associated with user database/annotation table
		/// </summary>
		/// <param name="Rf"></param>
		/// <returns></returns>

		public bool SaveUserData(bool multipleChanges)
		{
			// This saves/updates the data associated with a UserDatabase structure table
			// and/or one or more annotation tables and
			// handles the following cases for these combinations:
			// 1. New rows for a newly created table.
			// 2. New, updated & deleted rows for an existing table.
			// 3. Rows for an import (foreground & background)
			// 4. Manually edited imports (foreground only)

			Query q;
			bool userDatabase = false;
			bool annotationTable = false; // if true updating annotation tables only
			UcdbDatabase ucdb = null;
			MetaTable ucdbSMt = null; // user database structure table
			MetaTable updatedAMt = null; // first annotation table
			List<int> aQtList; // list of indexes of annotation tables within query
			ResultsTable rt;
			ResultsField rfld;
			QueryTable qt;
			MetaTable mt = null;
			MetaColumn mc;
			UserCmpndDbDao udbs = null;
			UcdbCompound cpd;
			AnnotationDao aDao = null;
			AnnotationVo aVo;
			List<AnnotationVo> voBuffer = new List<AnnotationVo>();
			DataRowMx dr;
			object[] vo;
			object o;
			string cid, oldCid, newCid, msg;
			long databaseId = -1, delCount;
			int ti, rfi, molId, dupCids, missingCidCnt;

			int t0 = TimeOfDay.Milliseconds();

			q = QueryManager.Query;
			aQtList = new List<int>();
			for (ti = 0; ti < q.Tables.Count; ti++)
			{ // list of annotation table positions & any user database structure table
				mt = q.Tables[ti].MetaTable;
				if (mt.IsUserDatabaseStructureTable)
					ucdbSMt = mt;

				else if (mt.MetaBrokerType == MetaBrokerType.Annotation)
				{
					aQtList.Add(ti);
				}
			}

			if (ucdbSMt == null && aQtList.Count == 0) return true; // nothing to do

			if (ucdbSMt != null)
			{
				udbs = new UserCmpndDbDao();
				databaseId = Int64.Parse(ucdbSMt.Code);
				if (InEditor) ucdb = Udb; // if in editor reference it's db object
				else ucdb = udbs.SelectDatabaseHeader(databaseId); // otherwise read in
				userDatabase = true;
			}

			else annotationTable = true;

			if (aQtList.Count > 0)
			{
				if (multipleChanges)
				{
					aDao = new AnnotationDao();
					aDao.BeginTransaction(); // and begin transaction
				}
			}

			bool importing = ( /* InEditor && */ImportParms != null);

			if (InEditor && !DataFormatterOpen) OpenDataFormatter(); // open data formatter if not already done

			if (annotationTable) // if annotation only, check for null cids
			{
				for (ti = 0; ti < DataTable.Rows.Count; ti++)
				{
					dr = DataTable.Rows[ti];
					if (Dtm.IsNullAddedRow(dr)) continue; // skip if all-null added row

					if (Dtm.IsNullKey(dr))
					{
						if (InEditor && !DisplayDataTabSelected)
						{ // be sure tab is selected
							Tabs.SelectedTabPageIndex = 1;
							DisplayDataTabSelected = true;
						}
						CellInfo cell = Grid.GetCellInfo(ti, 0);
						Grid.BGV.FocusedColumn = cell.GridColumn;
						Grid.BGV.FocusedRowHandle = cell.GridRowHandle;
						string errMsg = "Missing " + q.Tables[0].MetaTable.KeyMetaColumn.Label;
						MessageBoxMx.Show(errMsg, UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						Grid.BGV.ShowEditor(); // start editing
						if (udbs != null) udbs.Dispose();
						if (aDao != null) aDao.Dispose();
						return false;
					}
				}
			}

			// Insert/Update data

			ResultsFormatter fmtr = QueryManager.ResultsFormatter;
			int saveDataTableFetchPosition = Dtm.DataTableFetchPosition; // save current formatter position
			Dtm.DataTableFetchPosition = -1; // start at beginning again

			List<UcdbCompound> queuedCompoundList = new List<UcdbCompound>(); // list of compounds queued for insert
			int annotationIudCount = 0;
			int annotationInsertCount = 0;
			int annotationDeleteCount = 0;

			dupCids = 0;
			while (true) // loop through all rows if importing or just rows retrieved so far if not importing
			{
				if (!importing && Dtm.DataTableFetchPosition >= DataTable.Rows.Count - 1) // (note: going beyond this point for while editing a cell in the main results grid causes a fault in the editor/selection state)
					break; // all done if not importing & have retrieved all data that user could have modified

				dr = Dtm.FetchNextDataRow();
				if (dr == null) break;
				if (Dtm.IsNullAddedRow(dr)) continue; // skip if all-null added row
				vo = dr.ItemArray;
				DataRowAttributes dra = Dtm.GetRowAttributes(dr);

				if (dra == null) dra = dra; // debug

				int t2 = TimeOfDay.Milliseconds();
				if (importing && Progress.IsTimeToUpdate)
				{

					if (SS.I.Attended) // interactive import
					{
						if (Progress.CancelRequested)
						{
							if (udbs != null) udbs.Dispose();
							if (aDao != null) aDao.Dispose();
							Progress.Hide();
							return false;
						}
						Progress.Show("Storing data in database, " + Dtm.DataTableFetchCount + " rows inserted...");
					}

					else // non-interactive import, update checkpoint data
						UpdateImportStateCheckPoint();
				}

				// Update structure data

				if (ucdbSMt != null) do
					{
						rt = Rf.Tables[0];
						qt = rt.QueryTable;
						mt = ucdbSMt;

						if (dra.TableRowState[0] == RowStateEnum.Added || importing) // new compound
						{
							if (vo[KeyVoPos] != null) // compound id supplied?
								cid = vo[KeyVoPos].ToString();

							else cid = GetNextCid(); // need to assign compound id 

							vo[KeyVoPos] = cid; // store common key
							vo[rt.Fields[0].VoPosition] = cid; // store key for first table

							foreach (int ti2 in aQtList) // store in any annotation tables as well
							{
								vo[Rf.Tables[ti2].Fields[0].VoPosition] = cid;
							}

							if (importing)
							{
								if (ExistingCids.Contains(cid)) // don't try to insert if already exists
								{
									dupCids++;
									break;
								}

								else ExistingCids.Add(cid); // update list of cids
							}

							else if (CompoundIdUtil.Exists(cid, mt)) // if not import lookup the id
							{
								ResultsField rfld2 = Rf.GetResultsField(ucdbSMt.FirstStructureMetaColumn); // get str col results field
								if (aQtList.Count > 0 && rfld2 != null && vo[rfld2.VoPosition] == null)
									break; // if dup cid, no stucture & annotation then break to allow addition of secondary AMt row

								msg = ucdbSMt.KeyMetaColumn.Label + " " + CompoundId.Format(cid, mt) + " can't be added because it already exists in the database";
								MessageBoxMx.ShowError(msg);
								break;
							}

							cpd = new UcdbCompound();
							cpd.ExtCmpndIdTxt = cid;
							SetCmpndMembersFromVo(cpd, rt, vo);

							if (ucdb.ModelCount > 0) // set pending status if any models for db
								cpd.PendingStatus = UcdbWaitState.ModelPredictions;

							queuedCompoundList.Add(cpd);

							int cBlock = 100;
							if (!SS.I.Attended) cBlock = 1000; // bigger chunk if not attended
							if (queuedCompoundList.Count >= cBlock)
								ExecuteQueuedDatabaseCompoundUpdates(ucdb, queuedCompoundList, udbs);
							dra.TableRowState[0] = RowStateEnum.Unchanged; // reset row state
						}

						else if (dra.TableRowState[0] == RowStateEnum.Modified)
						{
							if (dra.OriginalKey != null) oldCid = dra.OriginalKey;
							else oldCid = vo[KeyVoPos].ToString();

							if (vo[KeyVoPos] == null || vo[KeyVoPos].ToString().Trim() == "") // trying to blank key
							{
								msg = ucdbSMt.KeyMetaColumn.Label + " " + CompoundId.Format(oldCid, mt) + " can't be set to blank, use \"Delete Row\"";
								MessageBoxMx.ShowError(msg);
								continue;
							}

							cid = vo[KeyVoPos].ToString();
							if (oldCid != cid && CompoundIdUtil.Exists(cid, mt))
							{ // check for changing to a cid that already exists
								msg = ucdbSMt.KeyMetaColumn.Label + " " + CompoundId.Format(cid, mt) + " can't be used because it already exists in the database";
								MessageBoxMx.ShowError(msg);
								break;
							}

							cpd = udbs.SelectDatabaseCompound(databaseId, oldCid); // get full existing data
							SetCmpndMembersFromVo(cpd, rt, vo); // update members from user
							cpd.RowState = UcdbRowState.Modified;
							dra.TableRowState[0] = RowStateEnum.Unchanged; // reset buffer row state

							if (ucdb.ModelCount > 0) // set pending status if any models for db
								cpd.PendingStatus = UcdbWaitState.ModelPredictions;

							queuedCompoundList.Add(cpd);
							if (queuedCompoundList.Count >= 100)
								ExecuteQueuedDatabaseCompoundUpdates(ucdb, queuedCompoundList, udbs);
						}

						else { } // no change
					} while (false); // allow break

				// Update annotation table data 

				foreach (int ti2 in aQtList)
				{
					ti = ti2;
					long rsltGrpId;
					rt = Rf.Tables[ti];
					qt = rt.QueryTable;
					mt = rt.MetaTable;

					bool forceUpdate = importing; // force the update if this is an import

					if (dra.TableRowState[ti] == RowStateEnum.Added || importing) // new annotation row
					{
						rsltGrpId = aDao.GetNextIdLong(); // id to assign to row
						foreach (ResultsField rfld2 in rt.Fields)
						{
							if (rfld2.MetaColumn.IsKey) continue;
							List<AnnotationVo> vob = importing ? voBuffer : null; // if importing then buffer inserts
							UpdateAnnotationResult(rfld2, dr, rsltGrpId, forceUpdate, vob, aDao); // buffer the update
							annotationIudCount++;
							annotationInsertCount++;
						}
						dra.TableRowState[ti] = RowStateEnum.Unchanged;
						updatedAMt = mt;
					}

					else if (dra.TableRowState[ti] == RowStateEnum.Modified) // modified row
					{
						if (dra.OriginalKey != null) oldCid = dra.OriginalKey;
						else oldCid = vo[KeyVoPos].ToString();

						foreach (ResultsField rfld2 in rt.Fields)
						{
							if (rfld2.MetaColumn.IsKey) continue;
							UpdateAnnotationResult(rfld2, dr, 0, forceUpdate, null, aDao); // do immediate update
							annotationIudCount++;
						}

						if (dra.OriginalKey != null && dra.OriginalKey != vo[KeyVoPos].ToString())
						{
							oldCid = dra.OriginalKey;
							newCid = vo[KeyVoPos].ToString();
							aDao.UpdateCid(Int32.Parse(mt.Code), oldCid, newCid); // note: this should really just update for the rslt_grp_id not the cid
						}
						updatedAMt = mt;
					}

					int aBlock = 1000;
					if (!SS.I.Attended) aBlock = 5000; // bigger chunk if not attended
					if (annotationIudCount >= aBlock)
					{
						aDao.Insert(voBuffer);
						voBuffer.Clear();
						if (multipleChanges) aDao.Commit();
						annotationIudCount = 0;
					}

				} // end for annnotation update

			} // end of row update

			// Process delete requests

			if (Dtm.DeletedRows != null)
			{
				for (int dti = 0; dti < Dtm.DeletedRows.Count; dti++)
				{
					dr = Dtm.DeletedRows[dti];
					vo = dr.ItemArray;
					DataRowAttributes dra = Dtm.GetRowAttributes(dr);
					if (dra.OriginalKey != null) oldCid = dra.OriginalKey;
					else oldCid = vo[KeyVoPos].ToString();

					// Delete User Database structure data

					mt = fmtr.Query.Tables[0].MetaTable;
					if (mt.IsUserDatabaseStructureTable && dra.TableRowState[0] == RowStateEnum.Deleted)
					{
						rt = Rf.Tables[0];
						qt = rt.QueryTable;

						cpd = udbs.SelectDatabaseCompound(databaseId, oldCid); // get full existing data
						cpd.RowState = UcdbRowState.Deleted; // mark for deletion
						queuedCompoundList.Add(cpd);
						if (queuedCompoundList.Count >= 100)
							ExecuteQueuedDatabaseCompoundUpdates(ucdb, queuedCompoundList, udbs);
					}

					// Delete annotation table data 

					for (ti = 0; ti < fmtr.Query.Tables.Count; ti++)
					{
						mt = Rf.Tables[ti].MetaTable;
						if (mt.MetaBrokerType != MetaBrokerType.Annotation ||
								dra.TableRowState[ti] != RowStateEnum.Deleted) continue;

						aDao.DeleteCompound(Int32.Parse(mt.Code), oldCid); // note: should delete for rslt_grp_id not cid
						annotationDeleteCount++;
						updatedAMt = mt;
					} // end for annnotation update

				} // end of row delete

				Dtm.DeletedRows.Clear(); // remove list of deleted tuples
			} // end of deletion

			ExecuteQueuedDatabaseCompoundUpdates(ucdb, queuedCompoundList, udbs); // finish updating any queued user database structure changes

			if (aDao != null) // finish any annotation updates
			{
				aDao.Insert(voBuffer);
				voBuffer.Clear();
				if (multipleChanges) aDao.Commit();
			}

			if (importing) // update row counts & build message
			{
				int rowCount = 0;

				if (ucdbSMt != null) // update count for structure table user object
				{
					rowCount = udbs.SelectDatabaseCompoundCount(databaseId);
					int objectId = UserObject.ParseObjectIdFromInternalName(ucdbSMt.Name);
					UserObjectDao.UpdateUpdateDateAndCount(objectId, DateTime.Now, (int)rowCount);
				}

				if (updatedAMt != null) // update row count for annotation table user object
				{
					rowCount = aDao.SelectRowCount(int.Parse(mt.Code));
					int objectId = UserObject.ParseObjectIdFromInternalName(updatedAMt.Name);
					UserObjectDao.UpdateUpdateDateAndCount(objectId, DateTime.Now, (int)rowCount);
				}

				// Build messge for user

				string clientFileName = "";
				int totalRows = 0;
				int compoundCount = 0;

				//if (InEditor)
				{
					clientFileName = ImportParms.FileName;
					msg = "Import of file " + Lex.Dq(clientFileName) + " into ";
					if (annotationTable)
					{
						msg += "annotation table " + mt.Label;
						totalRows = rowCount;
					}
					else
					{
						msg += "user database " + ucdb.Name;
						totalRows = rowCount;
					}
					msg += " is complete.\n";
					msg += "Data rows imported: " + Dtm.RowCount + "\n";
					msg += "Total rows in table: " + totalRows + "\n";
					if (annotationTable)
					{
						compoundCount = aDao.SelectCompoundCount(int.Parse(mt.Code));
						msg += "Total compounds: " + compoundCount + "\n";
					}
					t0 = (TimeOfDay.Milliseconds() - t0) / 1000;
					msg += "Time (sec.): " + t0;
					ImportMessages += msg + "\n";
				}

				// Build message for log

				if (udbs == null) udbs = new UserCmpndDbDao();
				msg = "Import complete ";
				if (ucdbSMt != null) msg += ucdbSMt.Name + ", ";
				if (updatedAMt != null) msg += updatedAMt.Name + ", ";
				msg += "User = " + SS.I.UserName +
				 ", File =  " + Lex.Dq(clientFileName) +
				 ", Attended = " + SS.I.Attended +
				 ", Rows imported = " + Dtm.RowCount +
				 ", Total rows = " + totalRows +
				 (annotationTable ? ", Total compounds = " + compoundCount : "") +
				 ", Time (sec.) = " + t0;
				udbs.LogMessage(msg);
			}

			else if (multipleChanges) // not importing, update table stats if not just a single change (just annotation for now) 
				try
				{
					if (updatedAMt != null) // update row count for annotation table user object
					{
						int objectId = UserObject.ParseObjectIdFromInternalName(updatedAMt.Name);
						UserObject uo = UserObjectDao.ReadHeader(objectId);
						uo.UpdateDateTime = DateTime.Now;
						uo.Count = uo.Count + annotationInsertCount - annotationDeleteCount;
						bool updateContentsTree = true;
						if (!InEditor) updateContentsTree = false; // don't update tree for each change in query results data grid
						UserObjectDao.UpdateHeader(uo, true, updateContentsTree);
					}
				}
				catch (Exception ex) { }

			if (udbs != null) udbs.Dispose();
			if (aDao != null) aDao.Dispose();

			Progress.Hide();
			return true;
		}

		/// <summary>
		/// Perform a non-interactive import for an existing user database/annotation.
		/// This method normally runs unattended in a separate Mobius session
		/// as the result of a background import request.
		/// 		command	"ImportUserData c:\\MobiusServerData\\BackgroundExports\\UserDatabase_234439.sdf, UserDatabase_234439"	string
		/// /// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public string ImportFile(
				string args)
		{
			string msg = "", loadLabel = "", typeName_UoId = "";
			bool badArg = false;
			UserDataImportState udis = null;
			int uoId = 0; // id of db or annotation user object

			Udbs = new UserCmpndDbDao(); // user compound database database service instance
			if (SS.I.Attended) Udbs.LogMessage("Non-interactive import started: " + args);
			else Udbs.LogMessage("Background import started: " + args);

			try
			{
				InteractiveMode = false;

				string[] sa = args.Split(',');
				if (sa.Length < 2) badArg = true;

				if (!badArg) // parse out the user data object type & id
				{
					typeName_UoId = sa[1].Trim(); // get the user object type & Id (e.g. USERDATABASE_123 OR ANNOTATION_456)
					string[] sa2 = typeName_UoId.Split('_');
					if (sa2.Length != 2) badArg = true;
					else
					{
						if (Lex.Eq(sa2[0], "UserDatabase")) UserDatabase = true;
						else if (Lex.Eq(sa2[0], "Annotation")) AnnotationTable = true;
						else badArg = true;

						if (!badArg)
						{
							if (!int.TryParse(sa2[1], out uoId)) badArg = true;
						}
					}
				}

				if (badArg)
				{
					msg = "Invalid ImportUserData command: " + args;
					Udbs.LogMessage(msg);
					msg = "Command syntax: Import User Data fileName, UserDatabase_123 | Annotation_456";
					return msg;
				}

				string fileName = sa[0].Trim();
				if (!File.Exists(fileName))
				{
					msg = "Import file doesn't exist on server for " + typeName_UoId + ": " + fileName;
					Udbs.LogMessage(msg);
					return msg;
				}

				ImportStateUo = // get any import state user object
						UserObjectDao.Read(UserObjectType.ImportState, SS.I.UserName, "", typeName_UoId);

				Uo = UserObjectDao.Read(uoId); // get db or annot uo
				if (Uo == null)
				{ // not found, delete any associated importState info
					if (ImportStateUo == null) return "User object " + typeName_UoId + " not found";

					UserObjectDao.Delete(ImportStateUo.Id);
					msg = "User object not found, deleted ImportState for " + typeName_UoId;
					Udbs.LogMessage(msg);
					return msg;
				}

				if (UserDatabase)
				{
					InitializeMembersForExistingUserDatabase(Uo);
					loadLabel = "user database " + Uo.Name;
				}

				else
				{
					AMt = MetaTable.Deserialize(Uo.Content);
					loadLabel = "annotation table " + AMt.Label;
					if (loadLabel.EndsWith(" (Note)", StringComparison.OrdinalIgnoreCase))
						loadLabel = loadLabel.Substring(0, loadLabel.Length - 7);
				}

				BuildCMtFromMetaTables();

				if (CMt.ImportParms == null)
				{ // import parameters must be defined
					msg = "Import parameters not defined for " + typeName_UoId;
					Udbs.LogMessage(msg);
					return msg;
				}

				ImportParms = CMt.ImportParms.Clone(); // get cloned copy of import parms

				string ext = Path.GetExtension(fileName);
				if (Lex.StartsWith(ext, ".xls")) // if .xls file, need to convert to .csv
				{
					string xlsFile = fileName;
					string csvFile = TempFile.GetTempFileName(ClientDirs.TempDir, ".csv", false);
					try { File.Delete(csvFile); }
					catch (Exception ex) { } // delete to avoid excel prompt
					ExcelOp.XlsToCsv(xlsFile, csvFile);
					try { File.Delete(xlsFile); } // don't need .xls file any longer
					catch (Exception ex) { msg = ex.Message; }

					ActualImportFileName = csvFile; // csv file is the one to work on now
				}

				else ActualImportFileName = fileName; // user copy of file on server for source as is (.csv, .txt)

				SyncFc2McWithCMt(); // be sure mapping is up to date

				if (ImportStateUo != null) // update import state
				{
					udis = UserDataImportState.Deserialize(ImportStateUo);
					//udis.FileName = fileName; // (this is the server file name, don't update)
					udis.LastStarted = DateTime.Now;
					udis.LastCheckpoint = DateTime.Now;
					ImportStateLastCheckPoint = DateTime.Now;
					udis.StartedCount++;
					ImportStateUo.Description = udis.Serialize(); // serialize into description
					UserObjectDao.Write(ImportStateUo);
				}

				CreateColDefDataTable();
				FillColDefDataTable(CMt);

				CreateModelsDataTable();
				FillModelsDataTable(Models);

				QueryManager = new QueryManager(); // allocate QueryManager that gets filled-in later

				SaveUserDataWithImportCheck(false); // do the import in this process

				if (ImportStateUo != null) // update import state
				{
					if (ImportParms.CheckForFileUpdates) // 
					{ // update state info if need to keep checking for updates
						ImportStateUo = UserObjectDao.Read(ImportStateUo.Id);
						if (ImportStateUo != null) // update import state if state obj still exists
						{
							udis = UserDataImportState.Deserialize(ImportStateUo);
							udis.LastCheckpoint = DateTime.MinValue; // complete now
							udis.CompletedCount++;
							ImportStateUo.Description = udis.Serialize();
							UserObjectDao.Write(ImportStateUo);
						}
					}

					else UserObjectDao.Delete(ImportStateUo.Id); // delete state info if one time only
				}

				try { File.Delete(ActualImportFileName); } // don't need server file any longer
				catch (Exception ex) { msg = ex.Message; }

				if (!SS.I.Attended) // send mail
				{
					MailMessage mail = new MailMessage();
					mail.From = new MailAddress("Mobius@[server]", UmlautMobius.String);
					mail.Subject = "Mobius file import for " + loadLabel;
					mail.To.Add(SS.I.EmailAddress);
					mail.Body = ImportMessages;
					try { Email.Send(mail); }
					catch (Exception ex)
					{
						Udbs.LogMessage("SendMail failed for: " + SS.I.UserName + ", " + SS.I.EmailAddress);
					}
				}

				msg = ImportMessages; // return message

				// Do any model updates (obsolete)

				//if (UserDatabase && Udb.PendingStatus == UcdbWaitState.ModelPredictions)
				//	msg += UserData.UpdatePendingModelResults(Udb.DatabaseId);

				return msg;
			}

			catch (Exception ex)
			{
				Udbs.LogMessage("Exception in ImportUserData " + args + "\r\n" +
						DebugLog.FormatExceptionMessage(ex));

				throw new Exception(ex.Message, ex);
			}

			finally
			{
				InteractiveMode = true;
			}
		}

		/// <summary>
		/// Update the checkpoint time for an import in the associated user object
		/// </summary>

		void UpdateImportStateCheckPoint()
		{
			if (ImportStateUo == null) return;

			TimeSpan ts = DateTime.Now.Subtract(ImportStateLastCheckPoint);
			if (ts.TotalSeconds < 60) return;

			ImportStateUo = UserObjectDao.Read(ImportStateUo.Id);
			if (ImportStateUo != null) // update import state if state obj still exists
			{
				UserDataImportState udis = UserDataImportState.Deserialize(ImportStateUo);
				udis.LastCheckpoint = DateTime.Now;
				ImportStateLastCheckPoint = udis.LastCheckpoint;
				ImportStateUo.Description = udis.Serialize();
				UserObjectDao.Write(ImportStateUo);
			}
		}

		/// <summary>
		/// Prepare for saving import data by reading into dataset checking for errors
		/// and deleting any existing data if requested.
		/// </summary>
		/// <returns></returns>

		bool CheckImportData()
		{
			AnnotationDao aDao = null;
			string cid, msg;
			long databaseId = 0, delCount;
			int dupCids, missingCidCnt;

			UserCmpndDbDao udbs = new UserCmpndDbDao();

			//Progress prevProgress = Progress.GetAndHideCurrentVisibleInstance();

			if (SMt != null)
			{
				databaseId = Int64.Parse(SMt.Code);
			}

			if (AMt != null)
			{
				aDao = new AnnotationDao();
				aDao.BeginTransaction(); // and begin transaction
			}

			if (DataTable == null || DataTable.Rows.Count == 0) // read file into DataTable if not already done
			{
				if (Qe != null) Qe.Close(); // be sure any query engine is closed
				if (OpenDataFormatter() == false) // read in allowing cancel on errors
				{
					if (udbs != null) udbs.Dispose();
					if (aDao != null) aDao.Dispose();
					return false;
				}
			}

			if (UserDatabase && Uo.Id > 0 && !ImportParms.DeleteExisting)
			{ // User database import, get list of existing cids so we can check for dups
				Progress.Show("Checking for duplicate " + SMt.KeyMetaColumn.Label + "s...");
				ExistingCids = new CidList(udbs.SelectDatabaseExtStringCids(Udb.DatabaseId));
				DataTableMx dt = DataTable;

				dupCids = 0;
				foreach (DataRowMx dr in dt.Rows)
				{ // see how many cids already exist in the database
					cid = dr[KeyVoPos].ToString();
					cid = NormalizeCompoundIdForDatabase(cid, SMt);
					if (ExistingCids.Contains(cid)) dupCids++;
				}
				//Progress.Show(prevProgress);

				if (dupCids > 0)
				{
					msg =
							"File " + Lex.Dq(ImportParms.FileName) + " contains\n" +
							dupCids + " of " + dt.Rows.Count + " records with " +
							 SMt.KeyMetaColumn.Label + "s that are already present in the database.\n" +
							"If these are duplicate database records that you want to replace you\n" +
							"should use the \"Delete Existing\" option in the import dialog box.";

					if (!ShowImportMessage("Duplicate Data", msg, true))
					{
						if (udbs != null) udbs.Dispose();
						if (aDao != null) aDao.Dispose();
						return false;
					}
				}
			}

			else if (AnnotationTable) // do prechecks for annotation-only import
			{
				int dri;
				int mthdVrsnId = Int32.Parse(AMt.Code);
				DataTableMx dt = DataTable;

				Progress.Show("Checking for non-existant " + AMt.KeyMetaColumn.Label + "s...");
				missingCidCnt = 0;
				string missingCids = "";
				for (dri = 0; dri < dt.Rows.Count; dri++)
				{ // see how many cids already exist in the database
					DataRowMx dr = dt.Rows[dri];
					cid = dr[KeyVoPos].ToString();
					cid = NormalizeCompoundIdForDatabase(cid, AMt.Root);
					if (!CompoundIdUtil.Exists(cid, AMt.Root))
					{
						missingCidCnt++;
						if (missingCidCnt <= 10)
						{
							if (missingCids != "") missingCids += ", ";
							missingCids += CompoundId.Format(cid, AMt.Root);
						}
					}

					if (dri >= 100) break; // just check 1st 100
				}
				//Progress.Show(prevProgress);

				if (missingCidCnt > 0)
				{
					msg = "File: " + ImportParms.FileName + "\ncontains non-existant " +
							AMt.KeyMetaColumn.Label + "s: " + missingCids;
					if (missingCidCnt > 10) msg += "...";
					msg += "\nfor the selected parent database.";

					if (!ShowImportMessage("Non-existant " + AMt.KeyMetaColumn.Label + "s", msg, true))
					{
						if (udbs != null) udbs.Dispose();
						if (aDao != null) aDao.Dispose();
						return false;
					}
				}

				// Ensure the annotation table won't change drastically during a nightly update.
				if (Security.IsMobius)
				{
					if (Uo != null && Uo.Id > 0 && ImportParms.DeleteExisting)
					{
						List<int> dataColumnIdxs = new List<int>();
						List<string> excludedColumnNames = new List<string> { "RowAttributes", "CheckMark", "BaseKey" };
						for (int i = 0; i < DataTable.Columns.Count; i++)
						{
							if (!excludedColumnNames.Contains(DataTable.Columns[i].ColumnName) && !DataTable.Columns[i].ColumnName.Contains("CORP_NBR"))
								dataColumnIdxs.Add(i);
						}

						int existingValCnt = aDao.GetNonNullRsltCnt(int.Parse(AMt.Code));
						double newValCnt = 0;
						foreach (DataRowMx dr in DataTable.Rows)
						{
							foreach (int idx in dataColumnIdxs)
							{
								if (dr.ItemArray[idx] != null)
									newValCnt += 1;
							}
						}

						double percentChange = 0;
						if (existingValCnt > 0)
						{
							percentChange = ((newValCnt - existingValCnt) / existingValCnt) * 100;
						}

						int maxPercentDecrease = ServicesIniFile.ReadInt("NightlyAnnotationTableUpdateMaxPercentDecrease", -20);
						if ((percentChange < maxPercentDecrease && ((existingValCnt - newValCnt) > 10)) || newValCnt == 0)
						{
							msg = "Annotation Table " + AMt.Name + " update will delete too many rows, aborting update.";
							udbs.LogMessage(msg);
							//msg = "Max % Decrease " + maxPercentDecrease + " Percent Change " + percentChange + " Existing Val Count " + existingValCnt + " New Val Count " + newValCnt;
							//udbs.LogMessage(msg);
							return false;
						}
					}
				}

				if (Uo != null && Uo.Id > 0 && !ImportParms.DeleteExisting)
				{ // check for duplicate cids
					Progress.Show("Checking for duplicate " + AMt.KeyMetaColumn.Label + "s...");
					dupCids = 0;
					for (dri = 0; dri < dt.Rows.Count; dri++)
					{ // see how many cids already exist in the database
						DataRowMx dr = dt.Rows[dri];
						cid = dr[KeyVoPos].ToString();
						cid = NormalizeCompoundIdForDatabase(cid, AMt.Root);
						if (aDao.Select(mthdVrsnId, cid) != null) dupCids++;
						if (dri + 1 >= 10) break;
					}
					//Progress.Show(prevProgress);

					if (dupCids > 0)
					{
						msg =
								"File: " + ImportParms.FileName + "\n" +
								"contains " + dupCids + " duplicate " + AMt.Root.KeyMetaColumn.Label + "s in the first " + dri + " records of the file\n" +
								"which may be a duplication of data already stored.";
						if (!ShowImportMessage("Possible Duplicate Data", msg, true))
						{
							if (udbs != null) udbs.Dispose();
							if (aDao != null) aDao.Dispose();
							return false;
						}

					}
				}
			}

			else ExistingCids = new CidList(); // no existing cids if new db or deleting old db

			// Delete existing data if requested

			if (ImportParms.DeleteExisting)
			{
				string progressMsg = null;
				if (SMt != null)
				{
					Progress.Show("Deleting existing database compounds...");
					delCount = udbs.DeleteDatabaseCompounds(databaseId);
					Progress.Hide();
					if (Progress.CancelRequested)
					{
						if (udbs != null) udbs.Dispose();
						if (aDao != null) aDao.Dispose();
						return false;
					}

				}

				if (AMt != null)
				{
					Progress.Show("Deleting existing annotation data...");
					delCount = aDao.DeleteTable(Int32.Parse(AMt.Code), true);
					Progress.Hide();
					if (Progress.CancelRequested)
					{
						if (udbs != null) udbs.Dispose();
						if (aDao != null) aDao.Dispose();
						return false;
					}

				}
			}

			Progress.Show("Storing data in database...");

			msg = "Import started  ";
			if (SMt != null) msg += SMt.Name + ", ";
			if (AMt != null) msg += AMt.Name + ", ";
			msg += "User = " + SS.I.UserName +
			 ", File =  " + Lex.Dq(ImportParms.FileName) +
			 ", Attended = " + SS.I.Attended;
			udbs.LogMessage(msg);

			if (udbs != null) udbs.Dispose();
			if (aDao != null)
			{
				aDao.Commit();
				aDao.Dispose();
			}

			return true;
		}

		/// <summary>
		/// Execute any pending compound updates
		/// </summary>
		/// <param name="queuedCompoundList"></param>
		/// <param name="uls"></param>

		static void ExecuteQueuedDatabaseCompoundUpdates(
				UcdbDatabase ucdb,
				List<UcdbCompound> queuedCompoundList,
				UserCmpndDbDao udbs)
		{
			if (queuedCompoundList.Count <= 0) return;
			UcdbCompound[] ca = queuedCompoundList.ToArray();
			udbs.UpdateDatabaseCompounds(ucdb, ca);
			queuedCompoundList.Clear();
		}

		/// <summary>
		/// Set UcdbCompound members (cid, structure, mf, mw) from results value object
		/// </summary>
		/// <param name="cpd"></param>
		/// <param name="rt"></param>
		/// <param name="vo"></param>
		/// <returns></returns>

		void SetCmpndMembersFromVo(
				UcdbCompound cpd,
				ResultsTable rt,
				object[] vo)
		{
			cpd.ExtCmpndIdTxt = vo[KeyVoPos].ToString(); // new compound id

			cpd.MolStructure = "";
			cpd.MolFormula = "";
			cpd.MolWeight = NullValue.NullNumber;

			for (int rfi = 0; rfi < rt.Fields.Count; rfi++)
			{
				ResultsField rf = rt.Fields[rfi];
				if (rf.MetaColumn.DataType != MetaColumnType.Structure) continue;
				object o = vo[rf.VoPosition];
				cpd.MolStructure = ObjectToMolString(o);
			}

			if (cpd.MolStructure == null || cpd.MolStructure == "") return;
			MoleculeMx cs = new MoleculeMx(cpd.MolStructure);
			if (cs != null)
			{
				cpd.MolFormula = cs.MolFormula;
				cpd.MolWeight = cs.MolWeight;
			}

			return;
		}

		/// <summary>
		/// Convert an object containing a molecule to a string form of the molecule
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>

		public static string ObjectToMolString(
				object o)
		{
			MoleculeMx mol = null;
			string molString = null;

			if (o == null) return null;
			else if (o is string) molString = (string)o;
			else if (o is MoleculeMx)
			{
				mol = o as MoleculeMx;
				molString = mol.GetPrimaryTypeAndValueString();
			}

			else if (o is QualifiedNumber)
				molString = ((QualifiedNumber)o).TextValue;
			else throw new Exception("Invalid structure value object");

			return molString;
		}

		/// <summary>
		/// Fill the grid of col defs 
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		void FillColDefDataTable(
				MetaTable mt)
		{
			InSetup = true;
			string txt = "", tok;
			ColGridDataTable.Rows.Clear();

			for (int i1 = 0; i1 < mt.MetaColumns.Count; i1++)
			{
				MetaColumn mc = mt.MetaColumns[i1];
				if (mc == null || Lex.IsUndefined(mc.Name) || Lex.IsNullOrEmpty(mc.Label)) continue;

				DataRow dr = ColGridDataTable.NewRow();
				dr["MetaColumn"] = mc.Clone();
				dr["Label"] = mc.Label;

				if (mc.DataType == MetaColumnType.CompoundId) tok = "Compound Identifier";
				else if (mc.DataType == MetaColumnType.String) tok = "Text";
				else if (
						mc.DataType == MetaColumnType.Integer ||
						mc.DataType == MetaColumnType.Number ||
						mc.DataType == MetaColumnType.QualifiedNo) tok = "Number";
				else if (mc.DataType == MetaColumnType.Date) tok = "Date";
				else if (mc.DataType == MetaColumnType.Structure) tok = "Structure";
				else tok = "Text"; // shouldn't happen
				dr["DataType"] = tok;
				dr["DisplayByDefault"] = mc.InitialSelection == ColumnSelectionEnum.Selected ? true : false;
				dr["PositionInImportFile"] = mc.ImportFilePosition;
				dr["Name"] = mc.Name;
				dr["ResultCode"] = mc.ResultCode;

				SetCustomFormattingGridImage(dr);

				ColGridDataTable.Rows.Add(dr);
			}

			InSetup = false;
			return;
		}

		void SetCustomFormattingGridImage(
				DataRow dr)
		{
			Image bitmap;

			MetaColumn mc = GetMetaColumnFromColGridDataTableRow(dr);

			bool customFormattingExists = mc != null && (!mc.IsDefaultFormat || !mc.IsDefaultWidth);

			if (!customFormattingExists) bitmap = Mobius.Data.Bitmaps.Bitmaps16x16.Images[(int)Bitmaps16x16Enum.DropdownBlack];
			else bitmap = Mobius.Data.Bitmaps.Bitmaps16x16.Images[(int)Bitmaps16x16Enum.DropdownGreen];

			bool b = InSetup;
			InSetup = true; // avoid processing ColDefDataTable_RowChanged event that invalidates the current data display
			dr["CustomFormat"] = bitmap;
			InSetup = b;

			return;
		}


		/// <summary>
		/// Scan file & analyze for data types
		/// </summary>
		/// <param name="fileName">Actual import file to use</param>
		/// <param name="cMt"></param>
		/// <param name="importParms"></param>
		/// <returns></returns>

		bool AnalyzeTextFile(
				string fileName,
				MetaTable cMt,
				UserDataImportParms importParms)
		{
			//			FileStream fs = null;
			StreamReader sr = null;
			List<string> al;
			string rec, tok;
			int recCount, t1, strConvertFails, i1;

			Progress.Show("2. Analyzing file...");
			Thread.Sleep(100);

			try
			{
				sr = OpenImportFileStream(fileName);
				if (sr == null) return false;

				List<MetaColumnType> types = new List<MetaColumnType>(); // column types
				List<string> labels = new List<string>(); // column headers
				recCount = 0;
				t1 = 0;
				strConvertFails = 0;

				while (true)
				{
					int t2 = TimeOfDay.Milliseconds();
					if (t2 - t1 > 1000)
					{
						if (Progress.CancelRequested)
						{
							sr.Close();
							//fs.Close();
							Progress.Hide();
							return false;
						}
						Progress.Show("3. Analyzing record (" + recCount.ToString() + ") ...");
						Thread.Sleep(100);
						t1 = t2;
					}

					rec = ReadImportRecord(sr, importParms.TextQualifier);
					if (rec == null) break;
					if (rec == "") continue; // ignore blank lines
					al = SplitImportRecord(rec, importParms.Delim, importParms.MultDelimsAsSingle, importParms.TextQualifier);

					if (importParms.FirstLineHeaders && recCount == 0)
					{
						labels = al;
						for (i1 = 0; i1 < labels.Count; i1++)
						{ // set any blank labels to default value
							if (labels[i1] == "") labels[i1] = "Column " + (i1 + 1).ToString();
						}
						recCount++;
						continue;
					}

					for (i1 = 0; i1 < al.Count; i1++) // look at each data column for type
					{
						while (i1 + 1 > types.Count)
							types.Add(MetaColumnType.Unknown); // start out with type undefined

						while (i1 + 1 > labels.Count) // add labels as needed
							labels.Add("Column " + (i1 + 1).ToString());

						tok = (string)al[i1];

						MetaColumnType currentType = types[i1];
						if (currentType == MetaColumnType.Structure && recCount > 500)
							continue; // if seen many good structures then don't need to check all of them

						MetaColumnType newType = SetImportDataTypeBasedOnFileValue(currentType, tok);

						if (currentType == MetaColumnType.Structure && newType != MetaColumnType.Structure)
						{ // allow a 10% failure of structure conversions
							strConvertFails++;
							if (recCount > 10 && strConvertFails / ((double)recCount) > .1)
								types[i1] = newType;
						}

						else types[i1] = newType;
					}

					recCount++;
				} // end of record loop

				sr.Close();
				//fs.Close();
				Progress.Hide();

				if (types.Count == 0 || recCount == 0)
				{
					MessageBoxMx.ShowError("File contains no data");
					return false;
				}

				else if (types.Count == 1 && AnnotationTable)
				{
					MessageBoxMx.ShowError("File must contain data records with at least two columns");
					return false;
				}

				Dictionary<string, MetaColumn> fc2Mc = MapMetaTableToFile(cMt, importParms, types, labels, null);
				if (fc2Mc == null) return false;

				importParms.Types = types;
				importParms.Labels = labels;
				importParms.Fc2Mc = fc2Mc;

				CMt.ImportParms = importParms.Clone();
				return true;
			}

			catch (Exception ex)
			{
				try { sr.Close(); /* fs.Close(); */ } catch { }
				Progress.Hide();
				string msg = DebugLog.FormatExceptionMessage(ex);
				MessageBoxMx.ShowError(msg);
				ServicesLog.Message(msg);
				return false;
			}
		}

		/// <summary>
		/// Open stream & handle and exceptions
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		StreamReader OpenImportFileStream(string fileName)
		{
			StreamReader sr = null;

			try { sr = new StreamReader(fileName); }
			catch (Exception ex) // probably open in another application or doesn't exist
			{
				MessageBoxMx.ShowError(ex.Message);
				return null;
			}

			return sr;

			//fs = // open even if open by Excel or another program
			//  File.Open(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
			//sr = new StreamReader(fs);
		}

		/// <summary>
		/// Read file into DataTable
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="importParms"></param>
		/// <param name="allowCancel"></param>
		/// <returns></returns>

		DataTableMx ReadImportFileIntoDataTable(
				string fileName,
				UserDataImportParms importParms,
				bool allowCancel)
		{
			MetaColumn mc, keyMc;
			CompoundId key;
			MoleculeMx cs;
			QualifiedNumber qn;
			DateTime dtTime;
			DateTimeMx dtEx;
			CompoundId cid;
			List<string> recFields = null;
			List<SdFileField> sdfFields = null;
			DataRowMx dr, dr2;
			object value;
			string dbKeyString, dbCidString, cidString2, lastCidString = "", molString, rec, msg, fPos = null, fValue = null;
			int fi, sfi, dri, nonNullValCount, sMtDti = -1, aMtDti = -1;

			//Progress prevProgress = Progress.GetAndHideCurrentVisibleInstance();
			Progress.Show("Formatting data...");

			try
			{
				SyncCMtWithColGrid();
				SyncAMtWithCMt();
				SyncFc2McWithCMt();

				DataTableMx dt = DataTable;

				if (SMt != null) sMtDti = 0; // set the data table offsets for maintaining row status for each table
				if (AMt != null) aMtDti = sMtDti + 1;

				bool sdFile = false;
				if (importParms.FileName.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase))
					sdFile = true;

				StreamReader sr = OpenImportFileStream(fileName);
				if (sr == null) return dt;

				int recCount = 0;
				int t1 = 0;

				int invalidCidCnt = 0;
				string invalidCids = "";
				int textTooLongCnt = 0;

				while (true)
				{
					int t2 = TimeOfDay.Milliseconds();
					if (t2 - t1 > 1000 && InteractiveMode)
					{
						if (Progress.CancelRequested)
						{
							sr.Close();
							//fs.Close();
							Progress.Hide();
							return null;
						}
						Progress.Show("Formatting data (" + recCount.ToString() + ") ...");
						t1 = t2;
					}

					//if (recCount > 100 && Rf.MolGrid != null && Rf.MolGrid.UpdateDepth2 == 0) 
					//  Rf.MolGrid.BeginUpdate2();

					if (!sdFile) // read next .smi, .csv record
					{
						rec = ReadImportRecord(sr, importParms.TextQualifier);
						if (rec == null) break;
						if (rec == "") continue; // ignore blank lines
						recFields = SplitImportRecord(rec, importParms.Delim, importParms.MultDelimsAsSingle, importParms.TextQualifier);

						if (importParms.FirstLineHeaders && recCount == 0)
						{
							recCount++;
							continue;
						}

						if (recFields.Count == 0) continue; // nothing on line
					}

					else // read next .sdf record
					{
						sdfFields = SdFileDao.Read(sr);
						if (sdfFields == null) break;
					}

					dr = dt.NewRow(); // row to hold data
					DataRowAttributes dra = Dtm.InitializeRowAttributes(dr);

					dbKeyString = null; // database form of key string
					key = null; // CompoundId form of cid
					cs = null; // ChemicalStructure
					nonNullValCount = 0;

					fi = 0;
					sfi = 0;
					while (true)
					{

						// Get next .smi, .csv field value

						if (!sdFile)
						{
							if (fi >= recFields.Count) break;
							fValue = recFields[fi];
							fPos = (fi + 1).ToString();
							fi++;
						}

						// Get next .sdf field value

						else
						{
							if (fi >= sdfFields.Count) break;

							SdFileField f = sdfFields[fi];
							if (f.Header == "") // molfile
							{
								switch (sfi)
								{
									case 0:
										fPos = "MolName";
										fValue = MoleculeMx.GetMolfileMolName(f.Data);
										break;

									case 1:
										fPos = "MolComment";
										fValue = MoleculeMx.GetMolfileComments(f.Data);
										break;

									case 2:
										fPos = "MolStructure";
										fValue = f.Data;
										break;
								}
								sfi++;
								if (sfi > 2) fi++;
							}

							else // non-header field
							{
								fPos = NormalizeSdfFieldName(f.Header);
								int ci;
								for (ci = f.Data.Length - 1; ci >= 0; ci--)
								{ // trim trailing newlines & spaces
									char c = f.Data[ci];
									if (c != '\n' && c != '\r' && c != ' ') break;
								}

								fValue = f.Data.Substring(0, ci + 1);
								fi++;
							}
						}

						// Store value based on metacolumn type

						if (!importParms.Fc2Mc.ContainsKey(fPos)) continue;
						mc = importParms.Fc2Mc[fPos];

						if (fValue.Trim() == "" || // null value
						 (mc.DataType == MetaColumnType.QualifiedNo && fValue == ".")) // null JMP numeric value
							continue;

						if (mc.DataType == MetaColumnType.CompoundId)
						{
							dbCidString = NormalizeCompoundIdForDatabase(fValue, CMt);
							cid = new CompoundId(dbCidString);
							value = cid;
						}

						else if (mc.DataType == MetaColumnType.Structure)
						{
							cs = new MoleculeMx(fValue); // verify valid format
							if (cs == null || cs.IsNull) continue;
							value = cs;
						}

						else if (mc.DataType == MetaColumnType.String)
						{
							value = new StringMx(fValue);
							if (fValue.ToString().Length > 4000)
								textTooLongCnt++;
						}

						else if (mc.DataType == MetaColumnType.QualifiedNo)
						{
							if (!QualifiedNumber.TryParse(fValue, out qn)) continue;
							value = qn;
						}

						else if (mc.DataType == MetaColumnType.Date)
						{
							if (!DateTimeMx.TryParse(fValue, out dtEx)) continue;
							value = dtEx;
						}

						else continue;

						dr[DataColName(mc)] = value; // store value

						if (mc.IsKey) // be sure we store the key
						{
							dbKeyString = NormalizeCompoundIdForDatabase(fValue, CMt); // be sure we have database string form of key
							keyMc = mc.MetaTable.KeyMetaColumn;
							dr[DataColName(keyMc)] = value; // store key value
							dr[DataTableManager.BaseKeyColumnName] = value.ToString(); // store common key value
						}

						if (mc.MetaTable == SMt) dra.TableRowState[sMtDti] = RowStateEnum.Added;
						else if (mc.MetaTable == AMt) dra.TableRowState[aMtDti] = RowStateEnum.Added;

						nonNullValCount++;

					} // end of field loop

					if (nonNullValCount == 0) continue; // just continue if no real values

					// Check for valid structure metatable key

					if (SMt != null && (AMt == null || cs != null)) // have structure table data?
					{
						if (dbKeyString != null) // see if cid is valid and new or duplicate
						{
							if (dbKeyString.Length > 32) // check for too long
							{
								invalidCidCnt++;
								if (invalidCidCnt <= 10 && !invalidCids.Contains(dbKeyString))
								{
									if (invalidCids != "") invalidCids += ", ";
									invalidCids += (recCount + 1) + " (" + dbKeyString + ")";
								}
								recCount++;
								continue;
							}

							else // see if already exists in DataTable
							{
								for (dri = 0; dri < dt.Rows.Count; dri++)
								{
									dr2 = dt.Rows[dri];
									cidString2 = dr2[DataTableManager.BaseKeyColumnName] as string;
									if (String.IsNullOrEmpty(cidString2)) continue;

									if (Lex.Eq(dbKeyString, cidString2)) break;
								}

								if (dri < dt.Rows.Count) // if found then ignore this structure data
								{
									if (AMt == null || dra.TableRowState[aMtDti] != RowStateEnum.Added)
										continue; // just skip if no annotation data
									try
									{ // clear structure data
										mc = SMt.KeyMetaColumn;
										dr[DataColName(mc)] = DBNull.Value;
										mc = SMt.FirstStructureMetaColumn;
										dr[DataColName(mc)] = DBNull.Value;
										dra.TableRowState[sMtDti] = RowStateEnum.Undefined;
									}
									catch (Exception ex) { ex = ex; }
								}
							}
						}

						else // assign & store cid
						{
							dbKeyString = GetNextCid();
							key = new CompoundId(dbKeyString);
							dr[DataTableManager.BaseKeyColumnName] = dbKeyString; // store common value
							dr[DataColName(SMt.KeyMetaColumn)] = key;
						}
					}

					if (AMt != null) // check for valid key if AMt data
					{
						if (!AMt.IsRootTable && String.IsNullOrEmpty(dbKeyString))  // ignore null cid for non-root annotation table
						{
							invalidCidCnt++;
							if (invalidCidCnt <= 10)
							{
								if (invalidCids != "") invalidCids += ", ";
								invalidCids += (recCount + 1).ToString();
							}
							recCount++;
							continue;
						}
					}

					dt.Rows.Add(dr); // add the row
					recCount++;
					lastCidString = dbKeyString;
				} // end of record loop

				//if (Rf.MolGrid != null && Rf.MolGrid.UpdateDepth2 > 0) 
				//  Rf.MolGrid.EndUpdate2(); // end update if in progress

				sr.Close();
				//fs.Close();
				//Progress.Show(prevProgress);

				if (invalidCidCnt > 0)
				{
					msg = "File: " + importParms.FileName + "\ncontains " + invalidCidCnt + " missing/invalid " + AMt.KeyMetaColumn.Label + "s in the following removed lines: "
					+ invalidCids;
					if (invalidCidCnt > 10) msg += "...";
					if (!ShowImportMessage("Invalid " + AMt.KeyMetaColumn.Label + "s", msg, allowCancel)) return null;
				}

				if (textTooLongCnt > 0)
				{
					msg = "File: " + importParms.FileName + " contains " + textTooLongCnt + " text values\n" +
							"longer than the 4000 character limit. These value will be truncated.";
					if (!ShowImportMessage("Long Text", msg, allowCancel)) return null;
				}

				Dtm.InitializeRowAttributes(false);

				if (InteractiveMode) // show the data if attended
				{
					Qm.ResultsFormatter = new ResultsFormatter(Qm);
					Qm.MoleculeGrid.InitializeRowState = false; // don't make any changes to rowstate
					Qm.ResultsFormatter.BeginFormatting();
					Qm.MoleculeGrid.InitializeRowState = true;
				}
				return dt;
			}

			catch (Exception ex)
			{
				Progress.Hide();
				msg = DebugLog.FormatExceptionMessage(ex);
				MessageBoxMx.ShowError(msg);
				ServicesLog.Message(msg);
				return null;
			}

		}

		/// <summary>
		/// Normalize compound id for database with simple normalization if exception occurs
		/// </summary>
		/// <param name="unnormalizedCid"></param>
		/// <param name="mt"></param>
		/// <returns></returns>

		static string NormalizeCompoundIdForDatabase(
				string unnormalizedCid,
				MetaTable mt)
		{
			string dbCidString = "";

			try { dbCidString = CompoundId.NormalizeForDatabase(unnormalizedCid, mt); }
			catch (Exception ex)
			{ dbCidString = unnormalizedCid.Replace(' ', '_').ToUpper(); }
			return dbCidString;
		}

		/// <summary>
		/// Return DataTable DataColumn name associated with a MetaColumn
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>

		string DataColName(MetaColumn mc)
		{
			Query q = Dtm.Query;
			QueryTable qt = q.GetQueryTableByName(mc.MetaTable.Name);
			string colName = qt.Alias + "." + mc.Name;
			return colName;

			//return mc.MetaTable.Name + "." + mc.Name; (old form)
		}

		/// <summary>
		/// Show/log error message & prompt for import continue
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="allowCancel"></param>
		/// <returns></returns>

		bool ShowImportMessage(
				string title,
				string msg,
				bool allowCancel)
		{
			if (SS.I.Attended)
			{
				if (!allowCancel)
					MessageBoxMx.ShowError(msg); // just inform
				else // allow user to cancel here
				{
					msg += "\nDo you want to continue with the import?";
					DialogResult dr =
							MessageBoxMx.Show(msg, title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
					if (dr != DialogResult.Yes) return false;
				}
			}
			else Udbs.LogMessage(msg);

			return true;
		}

		/// <summary>
		/// Prompt for annotation text file or ucdb smiles file, then analyze data
		/// </summary>

		bool ImportTextFileDialog()
		{
		GetInput:
			DialogResult dr = UserDataImportTextFile.ShowDialog(CMt, Uo, UserDatabase);
			if (dr == DialogResult.Cancel) return false;
			string fileName = SharePointUtil.CacheLocalCopyIfSharePointFile(CMt.ImportParms.FileName);
			ActualImportFileName = fileName;

			if (!AnalyzeTextFile(fileName, CMt, CMt.ImportParms)) goto GetInput;

			if (ImportParms != null && ImportParms.CheckForFileUpdates && !CMt.ImportParms.CheckForFileUpdates)
				TurnedCheckForFileUpdatesOff = true;

			ImportParms = CMt.ImportParms.Clone();

			ResetViewsAfterImport();
			return true;
		}

		/// <summary>
		/// Prompt for Excel file & analyze data
		/// </summary>

		bool ImportExcelDialog()
		{
			MetaColumn mc;
			StreamReader sr = null;
			List<string> al;
			string csvFile, exProp, headerOpt, rec, fValue, tok;
			QualifiedNumber qn;
			int i1;

			UserDataImportParms importParms = CMt.ImportParms;
			if (importParms == null)
			{
				importParms = new UserDataImportParms();
				importParms.FirstLineHeaders = true; // normally has headers
			}

			bool setForm = true;
		GetInput:
			string filter = "Microsoft Excel Files (*.xls; *.xlsx)|*.xls; *.xlsx";
			string prompt = "Import annotation table data from an Excel file";
			DialogResult dr = UserDataImportFile.ShowDialog(Uo, importParms, prompt, filter, ".xls", setForm);
			if (dr == DialogResult.Cancel) return false;

			setForm = false;
			importParms.Delim = ','; // set other .csv file parameters
			importParms.MultDelimsAsSingle = false;
			importParms.TextQualifier = '"';

			string ext = Path.GetExtension(importParms.FileName);
			if (Lex.Ne(ext, ".xlsx") && Lex.Ne(ext, ".xls"))
			{
				MessageBoxMx.ShowError("File type must be .xls or xlsx");
				goto GetInput;
			}

			try // convert .xls to .csv
			{
				//SplashScreenManager.ShowDefaultWaitForm("Please Wait...","Preparing File for Analysis");
				Progress.Show("1. Preparing file for analysis...");
				Thread.Sleep(100);
				csvFile = TempFile.GetTempFileName(ClientDirs.TempDir, ".csv", false);
				try { File.Delete(csvFile); } catch (Exception ex) { } // delete to avoid excel prompt
				ExcelOp.XlsToCsv(importParms.FileName, csvFile);
				ActualImportFileName = csvFile; // really importing the .csv file
			}
			catch (Exception ex)
			{
				Progress.Hide();
				MessageBoxMx.ShowError("Error converting Excel file to .csv\n\n" +
						ex.Message);

				string msg = "Error converting Excel file to .csv" + importParms.FileName + "\r\n" +
						DebugLog.FormatExceptionMessage(ex);
				ServicesLog.Message(msg);
				return false;
			}

			if (!AnalyzeTextFile(csvFile, CMt, importParms)) goto GetInput;

			CMt.ImportParms = importParms.Clone();
			ImportParms = importParms.Clone();

			ResetViewsAfterImport();
			return true;
		}

		/// <summary>
		/// Prompt for SDfile & analyze data
		/// </summary>

		bool ImportSDfileDialog()
		{
			MetaColumn mc;
			//FileStream fs = null;
			StreamReader sr = null;
			string rec, fValue;
			QualifiedNumber qn;
			int i1;

			UserDataImportParms importParms = CMt.ImportParms;
			if (importParms == null)
			{
				importParms = new UserDataImportParms();
				importParms.FirstLineHeaders = false; // no headers for sdf
			}

			bool setForm = true;
		GetInput:
			string filter = "SDfile (*.sdf)|*.sdf";
			if (UserDataImportFile.ShowDialog(Uo, importParms, "Import SDfile", filter, ".sdf", setForm) != DialogResult.OK)
				return false;
			setForm = false;

			Progress.Show("Analyzing file...");
			try
			{
				string fileName = importParms.FileName;
				ActualImportFileName = fileName;

				sr = OpenImportFileStream(fileName);
				if (sr == null) return false;

				List<string> names = new List<string>(); // name of sdfile field
				Dictionary<string, int> nameToPosition = new Dictionary<string, int>(); // maps name to position
				List<MetaColumnType> types = new List<MetaColumnType>(); // column types
				List<string> labels = new List<string>(); // column headers

				int recCount = 0;
				int dupCidCount = 0;
				int t1 = 0;
				while (true)
				{
					int t2 = TimeOfDay.Milliseconds();
					if (t2 - t1 > 1000)
					{
						if (Progress.CancelRequested)
						{
							sr.Close();
							//fs.Close();
							Progress.Hide();
							return false;
						}
						Progress.Show("Analyzing file (" + recCount.ToString() + ") ...");
						t1 = t2;
					}

					List<SdFileField> flds = SdFileDao.Read(sr);
					if (flds == null) break;

					for (i1 = 0; i1 < flds.Count; i1++)
					{
						SdFileField f = flds[i1];
						if (f.Header == "") // structure part
						{
							fValue = MoleculeMx.GetMolfileMolName(f.Data);
							if (fValue != "")
								SetSdfImportDataTypeBasedOnFileValue("MolName", "Compound Id", fValue, nameToPosition, names, labels, types);

							fValue = MoleculeMx.GetMolfileComments(f.Data);
							if (fValue != "")
								SetSdfImportDataTypeBasedOnFileValue("MolComment", "Comments", fValue, nameToPosition, names, labels, types);

							fValue = "MolFile=" + f.Data; // get structure
							SetSdfImportDataTypeBasedOnFileValue("MolStructure", "Structure", fValue, nameToPosition, names, labels, types);
						}

						else // other non-structure field
						{
							string fPos = NormalizeSdfFieldName(f.Header);
							string fLabel = fPos.Replace("_", " ");
							fLabel = Lex.CapitalizeFirstLetters(fLabel);
							SetSdfImportDataTypeBasedOnFileValue(fPos, fLabel, f.Data, nameToPosition, names, labels, types);
						}
					}

					recCount++;
				} // end of record loop

				sr.Close();
				//fs.Close();
				Progress.Hide();

				if (types.Count == 0 || recCount == 0)
				{
					MessageBoxMx.ShowError("File contains no data");
					goto GetInput;
				}

				else if (types.Count == 1 && AnnotationTable)
				{
					MessageBoxMx.ShowError("File must contain data records with at least two columns");
					goto GetInput;
				}

				Dictionary<string, MetaColumn> fc2Mc = MapMetaTableToFile(CMt, importParms, types, labels, names);
				if (fc2Mc == null) return false;

				importParms.Types = types;
				importParms.Labels = labels;
				importParms.Fc2Mc = fc2Mc;

				CMt.ImportParms = importParms.Clone();
				ImportParms = importParms.Clone();

				ResetViewsAfterImport();

				return true;
			}

			catch (Exception ex)
			{
				try { sr.Close(); /* fs.Close(); */ } catch { }
				Progress.Hide();
				string msg = DebugLog.FormatExceptionMessage(ex);
				MessageBoxMx.ShowError(msg);
				ServicesLog.Message(msg);
				return false;
			}
		}

		void SetValidImportParms() // need?
		{
			if (ImportParms == null && UserDatabase && SMt != null && SMt.ImportParms != null)
				ImportParms = SMt.ImportParms.Clone(); // get any existing import parms for database

			else if (ImportParms == null && AnnotationTable && AMt != null && AMt.ImportParms != null)
				ImportParms = AMt.ImportParms.Clone(); // get any existing import parms for annotation
		}

		/// <summary>
		/// Reset the column grid & display grid after import
		/// </summary>

		void ResetViewsAfterImport()
		{
			FillColDefDataTable(CMt); // update editor with new cols
			if (Qe != null) Qe.Close(); // close any open data source so we get new input data
			if (Qm.ResultsFormatter != null) Qm.ResultsFormatter = null; // any existing formatter is now invalid
			DataFormatterOpen = false; // not yet displayed
		}

		/// <summary>
		/// Convert ">  <FIELD_NAME>" -> "FIELD_NAME"
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		string NormalizeSdfFieldName(
				string name)
		{
			int i1 = name.IndexOf("<");
			if (i1 < 0) return name;
			string name2 = name.Substring(i1 + 1);
			i1 = name2.IndexOf(">");
			if (i1 < 0) return name;
			name2 = name2.Substring(0, i1);
			return name2;
		}

		void SetSdfImportDataTypeBasedOnFileValue(
				string name,
				string label,
				string tok,
				Dictionary<string, int> nameToPosition,
				List<string> names,
				List<string> labels,
				List<MetaColumnType> types)
		{
			if (!nameToPosition.ContainsKey(name))
			{
				nameToPosition[name] = types.Count;
				names.Add(name);
				labels.Add(label);
				types.Add(MetaColumnType.Unknown);
			}

			int pos = nameToPosition[name];

			types[pos] = SetImportDataTypeBasedOnFileValue(types[pos], tok);
			return;
		}

		/// <summary>
		/// This method sets the type of a file field based on the values seen in the file
		/// </summary>
		/// <param name="type"></param>
		/// <param name="tok"></param>

		MetaColumnType SetImportDataTypeBasedOnFileValue(
				MetaColumnType type,
				string tok)
		{
			MoleculeFormat structureFormat = MoleculeFormat.Unknown;

			// Assign type based on previous types seen and type of this value.
			// The following types are assigned from most specific to least specific.
			// 1. Integer, Number, Qualified Number, String
			// 2. Date, String
			// 3. Structure, String

			if (type == MetaColumnType.String) return type; // already have most general string type

			//			if (type == MetaColumnType.Structure && !IsSmilesString(tok)) type = type; // debug

			tok = tok.Trim();
			if (tok.EndsWith("\n")) tok = tok.Substring(0, tok.Length - 1);
			if (tok == "") return type; // skip if blank

			if (tok == ".") tok = "1.23";  // JMP null numeric value is treated as number

			if ((type == MetaColumnType.Integer || // check for integer
			 type == MetaColumnType.Unknown) && // not yet known
			 Lex.IsInteger(tok))
				return MetaColumnType.Integer;

			else if ((type == MetaColumnType.Number || // check for number (double)
			 type == MetaColumnType.Integer || // prev integer is ok
			 type == MetaColumnType.Unknown) && // not yet known
			 Lex.IsDouble(tok))
				return MetaColumnType.Number;

			else if ((type == MetaColumnType.QualifiedNo || // check for qualified number
			 type == MetaColumnType.Number || // prev number is ok
			 type == MetaColumnType.Integer || // prev integer is ok
			 type == MetaColumnType.Unknown) && // not yet known
			 QualifiedNumber.IsQualifiedNumber(tok))
				return MetaColumnType.QualifiedNo;

			else if ((type == MetaColumnType.Date || // check for date
			 type == MetaColumnType.Unknown) && // not yet known
			 DateTimeMx.Normalize(tok) != null)
				return MetaColumnType.Date;

			else if ((type == MetaColumnType.Structure || // check for structure
			 type == MetaColumnType.Unknown) && // not yet known
				IsMoleculeString(tok, out structureFormat))
			{
				if (type == MetaColumnType.Structure ||
						structureFormat != MoleculeFormat.Smiles ||
				 (structureFormat == MoleculeFormat.Smiles && IsNonTrivialSmiles(tok))) // if smiles, be sure we see at least one non-trivial smiles string
					return MetaColumnType.Structure;
				else return MetaColumnType.Unknown; // don't commit to structure type until seen nontrivial smiles
			}

			else return MetaColumnType.String;
		}

		/// <summary>
		/// See if value is a molecule string (Chime, Smiles, Molfile, Helm)
		/// </summary>
		/// <param name="s"></param>
		/// <param name="sf"></param>
		/// <returns></returns>

		bool IsMoleculeString(string s, out MoleculeFormat sf)
		{
			if (String.IsNullOrEmpty(s))
			{
				sf = MoleculeFormat.Unknown;
				return false;
			}

			sf = MoleculeMx.GetMoleculeFormatType(s);
			if (sf == MoleculeFormat.Chime || sf == MoleculeFormat.Molfile || sf == MoleculeFormat.Smiles || // recognize these types only for now
			 sf == MoleculeFormat.Helm)
				return true;

			else
			{
				sf = MoleculeFormat.Unknown;
				return false;
			}
		}

		/// <summary>
		/// Return true if nontrivial Smiles (i.e. string of length 8 with branch or ring
		/// </summary>
		/// <param name="tok"></param>
		/// <returns></returns>

		bool IsNonTrivialSmiles(
				string tok)
		{
			if (tok.Length >= 8) // minimum length
													 //tok.Contains("(") || // or includes branch
													 //tok.Contains("1")) // or includes ring
				return true;
			else return false;
		}

		/// <summary>
		/// Create a DataTable and DataTableManager to match the current user data
		/// </summary>
		/// <param name="aMt">Annotation MetaTable (or null)</param>
		/// <param name="sMt">Structure MetaTable (or null)</param>
		/// <returns></returns>

		DataTableMx BuildDataTable(
				MetaTable sMt,
				MetaTable aMt)
		{
			DataTableMx dt = null;
			Query q = new Query();
			if (sMt != null) q.AddQueryTable(new QueryTable(sMt));
			if (aMt != null) q.AddQueryTable(new QueryTable(aMt));

			dt = DataTableManager.BuildDataTable(q);
			return dt;
		}

		/// <summary>
		/// Check to see if definition is valid
		/// </summary>
		/// <param name="cMt"></param>
		/// <returns></returns>

		bool IsValidDefinition()
		{
			MetaColumn mc;

			if (CMt.MetaColumns.Count < 2)
			{
				ShowColGridError(
						"An annotation table must contain at least two data columns", -1, -1);
				return false;
			}

			if (CMt.MetaColumns.Count > MetaTable.MaxColumns)
			{
				ShowColGridError(
						"An annotation table can contain a maximum of " + MetaTable.MaxColumns + " columns.\n" +
						"The current table contains " + CMt.MetaColumns.Count + " columns.", -1, -1);
				return false;
			}

			for (int ci = 0; ci < CMt.MetaColumns.Count; ci++)
			{
				mc = CMt.MetaColumns[ci];
				if (mc == null)
				{
					ShowColGridError(
							"Column not fully defined", ci + 1, 1);
					return false;
				}

				if (mc.Label == "")
				{
					ShowColGridError(
							"Missing column name", ci + 1, 1);
					return false;
				}

				else if (mc.DataType == MetaColumnType.Unknown)
				{
					ShowColGridError(
							"Missing data type", ci + 1, 2);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Display error in column grid & start editing at that position
		/// </summary>
		/// <param name="errMsg"></param>
		/// <param name="row"></param>
		/// <param name="col"></param>

		void ShowColGridError(
				string errMsg,
				int row,
				int col)
		{
			Tabs.SelectedTabPageIndex = 0; // put on col grid tab
			if (row >= 0)
			{
				View.FocusedRowHandle = row;
				View.FocusedColumn = View.Columns[col];
			}
			MessageBoxMx.ShowError(errMsg);
			if (row >= 0) ColGrid.MainView.ShowEditor();
			return;
		}

		/// <summary>
		/// Open a data formatter for the current user data
		/// </summary>
		/// <param name="cMt"></param>

		bool OpenDataFormatter()
		{

			// This method builds a query for the user data, creates a results format and 
			// sets DataFormatterOpen = true. If we are in interactive graphics mode then
			// data is displayed in the data table tab.
			// There are three possible sources for the data and the values of Qe and ImportDs 
			// determine the source:
			//
			// 1. New Db/Annotation - Qe = ImportDs = null
			// 2. Existing Db/Annotation - Qe != null
			// 3. Preview of new or existing Db/Annotation - ImportDs != null

			ResultsFormatFactory rff;
			QueryTable qt = null;

			SyncCMtWithColGrid();
			SyncAMtWithCMt();

			Query q = new Query();
			q.AllowColumnMerging = false; // keep cols separate to allow editing
			q.ShowGridCheckBoxes = false; // don't want check boxes here
			q.RepeatReport = false;
			q.Mode = QueryMode.Browse;

			if (SMt != null)
			{
				qt = new QueryTable(SMt);
				q.AddQueryTable(qt);
			}

			if (AMt != null)
			{
				qt = new QueryTable(AMt);
				q.AddQueryTable(qt);
				foreach (QueryColumn qc in qt.QueryColumns) // select all selectable fields so they get into database
				{
					if (qc.MetaColumn.InitialSelection == ColumnSelectionEnum.Selected ||
							qc.MetaColumn.InitialSelection == ColumnSelectionEnum.Unselected)
						qc.Selected = true;
				}

				if (AMt.Code != "" && AMt.Code != "0")
				{
					CompleteAnnotationTableMetaColumnDefinitions(AMt); // be sure ready for querying
				}
			}

			Qm.LinkMember(q);
			Qm.DataTableManager = new DataTableManager(Qm);
			DataTableMx dt2 = DataTableManager.BuildDataTable(Qm.Query); // 
			if (!PersistedDataExists && Qm.DataTable != null && Qm.DataTable.Count > 0) // existing unpersisted data to copy over to new DataTable?
				CopyDataTableRows(Qm.DataTable, ref dt2);

			Qm.DataTable = dt2;

			if (InteractiveMode) // setup normal interactive retrieval
			{
				SetupDataGridFormatting();

				RowGridBandedView.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.True;
				RowGridBandedView.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.True;
				RowGridBandedView.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;
				RowGridBandedView.Bands["CheckMark"].Visible = false; // hide checkmark col

				if (ImportParms == null) // if not importing then show the data now.
				{
					RowGrid.DataSource = Qm.DataTable; // set grid to proper data table
					Qm.ResultsFormatter = new ResultsFormatter(Qm);

					if (!PersistedDataExists) // just exists in memory
					{
						Qm.ResultsFormatter.BeginFormatting(true);
						DataFormatterOpen = true; // set flag indicating we are displaying data
						return true;
					}
				}

				else { } // if importing don't show data in grid now to avoid thrashing and out of memory conditions during load of large files
			}

			else
			{ // setup retrieval without sending data to grid (non-interactive)
				rff = new ResultsFormatFactory(Qm, OutputDest.TextFile); // setup for background load
				rff.Rf.ShowHeaderStyling = false; // no styling on headers
				rff.Build(); // complete build of formatting 
				Rf.OutputDestination = OutputDest.Search; // switch output dest to avoid formatting

				Qm.DataTable = DataTableManager.BuildDataTable(q); // build table to hold data
				RowGrid.SetupUnboundColumns(DataTable); // bind to data table

				Qm.ResultsFormatter = new ResultsFormatter(Qm);
				Rf.Formatter.BeginFormatting(); // initialize formatter
			}

			if (PersistedDataExists && ImportParms == null) // if data exists & not importing then open query engine on dataset
			{
				if (SMt != null) // get compound ids & use in query
				{
					string[] cids = Udbs.SelectDatabaseExtStringCids(Udb.DatabaseId);
					ExistingCids = new CidList(cids);
					q.ResultKeys = ExistingCids.ToStringList();
					q.UseResultKeys = true;
				}

				else // do preview on single annotation table
					q.Preview = true;

				if (Uo != null && Uo.Count > 0)
					Progress.Show("Retrieving data...");

				Qm.QueryEngine = new QueryEngine();
				Qm.QueryEngine.ExecuteQuery(q);
				//if (keys == null || keys.Count == 0) Progress.Hide();

				Qm.ResultsFormatter.BeginFormatting(); // // start retrieving rows
																							 //Dtm.StartRowRetrieval(); // start retrieving rows
			}

			else if (ImportParms != null)
			{
				if (DataTable.Rows.Count == 0) // read in import data
				{
					if (Qm.QueryEngine != null) // close any open Qe datasource
					{
						Qm.QueryEngine.Close();
						Qm.QueryEngine = null;
					}

					DataTableMx importDt = ReadImportFileIntoDataTable(ActualImportFileName, ImportParms, false);
					if (importDt == null) return false; // import cancelled
				}

				else
				{
					if (Qm.ResultsFormatter == null)
						Qm.ResultsFormatter = new ResultsFormatter(Qm);

					Qm.ResultsFormatter.BeginFormatting();
				}
			}

			DataFormatterOpen = true; // set flag indicating we are displaying data
			return true;
		}

		/// <summary>
		/// Setup Results format and data grid headers
		/// </summary>
		void SetupDataGridFormatting()
		{
			ResultsFormatFactory rff = new ResultsFormatFactory(Qm, OutputDest.WinForms);
			rff.Rf.ShowHeaderStyling = false; // no styling on headers
			rff.Build(); // complete build of formatting 

			DataTableMx dt = Qm.DataTable; // save ref to data table
			RowGrid.DataSource = null; // clear source for header build
			Qm.DataTable = dt; // restore data table

			RowGrid.FormatGridHeaders(rff.Rf); // create grid headers
			RowGrid.AddStandardEventHandlers(); // setup to handle events
		}

		/// <summary>
		/// Return true if a persisted userobject (and possibly data) exists for the user data object currently being edited.
		/// Assumes that SMt and AMt are up to date
		/// </summary>

		public bool PersistedDataExists
		{
			get
			{
				if (SMt != null)
				{
					if (SMt.Code != "" && SMt.Code != "0")
						return true;
				}

				if (AMt != null)
				{
					if (AMt.Code != "" && AMt.Code != "0")
						return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Copy rows from old to new datatable allowing for any additions or deletions
		/// For Non-persisted tables only.
		/// </summary>
		/// <param name="src"></param>
		/// <param name="dst"></param>

		void CopyDataTableRows(
				DataTableMx src,
				ref DataTableMx dst)
		{

			// Map src to dst cols

			int[] map = new int[dst.Columns.Count];
			int sameCnt = 0;

			for (int di = 0; di < dst.Columns.Count; di++)
			{
				map[di] = -1;

				DataColumn dCol = dst.Columns[di];
				string dName = dCol.ColumnName;
				if (src.Columns.Contains(dName))
				{
					DataColumn sCol = src.Columns[dName];
					int si = sCol.Ordinal;

					if (sCol.DataType.Equals(dCol.DataType))
					{
						map[di] = si;
						if (si == di) sameCnt++;
					}
				}
			}

			if (sameCnt == dst.Columns.Count) // done if same col structure
			{
				dst = src;
				RemoveDataRowsWithUndefinedKeyValue(dst);
				return;
			}

			// Copy the data using map

			foreach (DataRowMx sRow in src.Rows)
			{
				object[] sItemArray = sRow.ItemArray;

				DataRowMx dRow = dst.NewRow();
				object[] dItemArray = dRow.ItemArray;

				for (int di = 0; di < dst.Columns.Count; di++)
				{
					if (map[di] >= 0) dItemArray[di] = sItemArray[map[di]];
					else dItemArray[di] = DBNull.Value;
				}
				dRow.ItemArray = dItemArray;
				dst.Rows.Add(dRow);
			}

			RemoveDataRowsWithUndefinedKeyValue(dst);
		}

		void RemoveDataRowsWithUndefinedKeyValue(DataTableMx dt)
		{
			int ri = 0;
			while (ri < dt.Rows.Count)
			{
				DataRowMx dr = dt.Rows[ri];
				object o = dr[KeyVoPos + 1];
				if (!NullValue.IsNull(o)) ri++;
				else // null, remove
				{
					dt.Rows.RemoveAt(ri);
				}
			}
		}

		/// <summary>
		/// Build combined metatable CMt from SMt and AMt. CMt points to the key
		/// and structure metacolumn from SMt and gets other metacolumns from AMt.
		/// </summary>

		void BuildCMtFromMetaTables()
		{
			if (UserDatabase)
			{
				CMt = SMt.Clone();
				CMt.MetaColumns = new List<MetaColumn>();
				CMt.MetaColumns.Add(SMt.MetaColumns[0]); // add compound id
				CMt.MetaColumns.Add(SMt.MetaColumns[1]); // add structure
				if (AMt != null) // add any annotation metacolumns
					for (int ci = 1; ci < AMt.MetaColumns.Count; ci++)
						CMt.MetaColumns.Add(AMt.MetaColumns[ci]);

				if ((CMt.ImportParms = SMt.ImportParms) != null)
					CMt.ImportParms = SMt.ImportParms.Clone();
			}

			else CMt = AMt; // reference same metatable if a single annotation table
		}

		/// <summary>
		/// Synchronize AMt and CMt with the current contents of the grid
		/// </summary>

		void SyncAMtWithCMt()
		{
			MetaColumn mc;

			if (UserDatabase)
			{
				if (CMt.MetaColumns.Count <= 2) // no annotation table
				{
					AMt = null;
					return;
				}

				if (AMt == null) // create annotation metatable prototype
				{
					AMt = new MetaTable();
					AMt.Name = "ANNOTATION_"; // name stub, id added later
					AMt.Label = "Data";
					AMt.Parent = SMt;
					AMt.MetaBrokerType = MetaBrokerType.Annotation;
					AMt.Code = "0"; // so queries will execute but return no data
					mc = SMt.MetaColumns[0].Clone();
					if (mc.IsNumeric) mc.ColumnMap = "EXT_CMPND_ID_NBR"; // numeric database column
					else mc.ColumnMap = "EXT_CMPND_ID_TXT"; // string database column
					mc.MetaTable = AMt;
					AMt.MetaColumns.Add(mc);
				}

				AMt.Description = CMt.Description; // copy description

				int colCnt = AMt.MetaColumns.Count;
				if (colCnt > 1) AMt.MetaColumns.RemoveRange(1, colCnt - 1);
				for (int ci = 2; ci < CMt.MetaColumns.Count; ci++) // copy other cols
				{
					mc = CMt.MetaColumns[ci];
					mc.MetaTable = AMt;
					AMt.MetaColumns.Add(mc);
				}
			}

			else // annotation table
			{
				if (CMt.Name == "") CMt.Name = "ANNOTATION_"; // name stub, id added later
				AMt = CMt;  // if just annotation table then CMt and AMt are the same object
			}
		}

		/// <summary>
		/// Update CMt from the column DataTable
		/// </summary>

		void SyncCMtWithColGrid()
		{
			MetaColumn mc;
			List<MetaColumn> mcList = new List<MetaColumn>();
			bool updateGrid = false, newMc = false;

			foreach (DataRow dr0 in ColGridDataTable.Rows)
			{
				mc = GetMetaColumnFromColGridDataTableRow(dr0, out newMc);
				if (newMc) updateGrid = true;

				mcList.Add(mc);
			}

			CMt.MetaColumns = mcList;

			if (updateGrid) FillColDefDataTable(CMt); // update grid with any new names
			return;
		}


		/// <summary>
		/// Get or build a MetaColumn from a ColGridDataTable row
		/// </summary>
		/// <param name="dr"></param>
		/// <param name="newMc"></param>
		/// <returns></returns>

		MetaColumn GetMetaColumnFromColGridDataTableRow(
				DataRow dr)
		{
			bool newMc;
			return GetMetaColumnFromColGridDataTableRow(dr, out newMc);
		}

		/// <summary>
		/// Get or build a MetaColumn from a ColGridDataTable row
		/// </summary>
		/// <param name="dr"></param>
		/// <param name="mcNotInCMt"></param>
		/// <returns></returns>

		MetaColumn GetMetaColumnFromColGridDataTableRow(
				DataRow dr,
				out bool mcNotInCMt)
		{
			MetaColumn mc0 = null, mc = null;
			mcNotInCMt = false;

			mc0 = dr["MetaColumn"] as MetaColumn;
			string colLabel = RowUtil.GetString(dr, "Label");
			string colType = RowUtil.GetString(dr, "DataType");

			if (String.IsNullOrEmpty(colLabel) || // need label & type
			String.IsNullOrEmpty(colType)) return null;

			bool displayByDefault = RowUtil.GetBool(dr, "DisplayByDefault", true);
			string posInFile = RowUtil.GetString(dr, "PositionInImportFile");
			string colName = RowUtil.GetString(dr, "Name");
			string resultCode = RowUtil.GetString(dr, "ResultCode");

			if (String.IsNullOrEmpty(colName)) // new row, assign a name
			{
				if (mc0 != null) // if already have a temp mc for this col use it
				{
					mc = mc0;
				}

				else
				{
					mc = new MetaColumn();
					mcNotInCMt = true;
					mc.MetaTable = CMt;
					TempNameIndex++;
					mc.Name = "Temp_" + TempNameIndex;
					mc.ResultCode = "0";
					dr["MetaColumn"] = mc; // save temp mc
				}
			}

			else
			{
				mc = CMt.GetMetaColumnByName(colName);
				if (mc == null)
				{
					mc = new MetaColumn();
					mcNotInCMt = true;
					mc.MetaTable = CMt;
					mc.Name = colName;
					mc.ResultCode = resultCode;
				}
			}

			mc.Label = colLabel;

			if (Lex.Eq(colType, "Compound Identifier")) mc.DataType = MetaColumnType.CompoundId;
			else if (Lex.Eq(colType, "Text")) mc.DataType = MetaColumnType.String;
			else if (Lex.Eq(colType, "Number")) mc.DataType = MetaColumnType.QualifiedNo;
			else if (Lex.Eq(colType, "Date")) mc.DataType = MetaColumnType.Date;
			else if (Lex.Eq(colType, "Structure")) mc.DataType = MetaColumnType.Structure;
			else throw new Exception("Unexpected column type: " + colType);
			if (mcNotInCMt) mc.Width = MetaColumn.GetDefaultDisplayWidth(mc.DataType);

			if (displayByDefault) mc.InitialSelection = ColumnSelectionEnum.Selected;
			else mc.InitialSelection = ColumnSelectionEnum.Unselected;

			if (!String.IsNullOrEmpty(posInFile)) mc.ImportFilePosition = posInFile;
			if (!String.IsNullOrEmpty(resultCode)) mc.ResultCode = resultCode;

			if (mc0 != null)
			{
				mc.Format = mc0.Format; // copy format info
				mc.Decimals = mc0.Decimals;
				mc.Width = mc0.Width;
			}

			dr.ItemArray[ColGridDataTablePos("MetaColumn")] = mc; // store back in DataRow (avoid firing change event that invalidates the current data display)

			return mc;
		}

		/// <summary>
		/// Update MetaColumn formatting for both the column grid and the data display grid.
		/// Copy from qc to mc and restore qc defaults.
		/// 
		/// </summary>
		/// <param name="qc"></param>

		public void UpdateMetaColumnFormatting(
				QueryColumn qc)
		{
			if (CMt == null) return;

			string mcName = qc.MetaColumn.Name;
			QueryColumn qc0 = new QueryColumn();

			MetaColumn cmtMc = CMt.GetMetaColumnByName(mcName); // get metacolumn from temp metatable if exists
			MetaColumn cgMc = GetColGridMetaColumnByName(mcName); // get assoc metacolumn in col grid if exists

			if (qc.DisplayFormat != ColumnFormatEnum.Unknown)
			{
				if (cmtMc != null) cmtMc.Format = qc.DisplayFormat;

				if (cgMc != null)
				{
					cgMc.Format = qc.DisplayFormat;
					qc.DisplayFormat = qc0.DisplayFormat;
				}
			}

			if (qc.Decimals >= 0)
			{
				if (cmtMc != null) cmtMc.Decimals = qc.Decimals;

				if (cgMc != null)
				{
					cgMc.Decimals = qc.Decimals;
					qc.Decimals = qc0.Decimals;
				}
			}

			if (qc.DisplayWidth > 0)
			{
				if (cmtMc != null) cmtMc.Width = qc.DisplayWidth;

				if (cgMc != null)
				{
					cgMc.Width = qc.DisplayWidth;
					qc.DisplayWidth = qc0.DisplayWidth;
				}
			}

			//ColumnsModified = true;  // (Don't mark as modified to allow unhindered format changes and data view switching)
			return;
		}

		/// <summary>
		/// Get the metacolumn from the ColGrid for the specified internal MetaColumn name if it exists
		/// </summary>
		/// <param name="mcName"></param>
		/// <returns></returns>

		MetaColumn GetColGridMetaColumnByName(string mcName)
		{
			foreach (DataRow dr in ColGridDataTable.Rows)
			{
				MetaColumn mc = dr["MetaColumn"] as MetaColumn;
				if (mc == null) continue;

				if (Lex.Eq(mc.Name, mcName))
					return mc;
			}

			return null; // not found
		}

		/// <summary>
		/// Synchronize ImportParms.Fc2Mc with CMt
		/// </summary>
		/// 
		void SyncFc2McWithCMt()
		{

			ImportParms.Fc2Mc = new Dictionary<string, MetaColumn>(); // build current mapping of field columns to metacolumns

			foreach (MetaColumn mc2 in CMt.MetaColumns) // build mapping info
			{
				if (!String.IsNullOrEmpty(mc2.ImportFilePosition))
					ImportParms.Fc2Mc[mc2.ImportFilePosition] = mc2;
			}
		}

		/// <summary>
		/// Get prototype for user database structure table
		/// </summary>
		/// <returns></returns>

		MetaTable GetStructureTablePrototype()
		{
			MetaTable mt;
			MetaColumn mc;

			mt = MetaTableCollection.Get("StructureTableTemplate");
			if (mt == null) throw new Exception("StructureTableTemplate not found");
			mt = mt.Clone(); // make copy we can modify
			mt.Name = "USERDATABASE_STRUCTURE_"; // name stub, id added later
			mt.Label = "Structures";
			mt.MetaBrokerType = MetaBrokerType.Generic;

			mc = mt.KeyMetaColumn;
			if (mc.IsNumeric) mc.ColumnMap = "EXT_CMPND_ID_NBR"; // numeric database column
			else mc.ColumnMap = "EXT_CMPND_ID_TXT"; // string database column

			mc = mt.FirstStructureMetaColumn;
			mc.ColumnMap = "MLCL_STRUCT";

			if ((mc = mt.GetMetaColumnByName("molformula")) != null)
				mc.ColumnMap = "MLCL_FRML";
			if ((mc = mt.GetMetaColumnByName("molweight")) != null)
				mc.ColumnMap = "MLCL_WGT";
			if ((mc = mt.GetMetaColumnByName("molecule_date")) != null)
				mc.ColumnMap = "MOLECULE_DATE";

			mc = mt.DatabaseListMetaColumn;
			if (mc != null) mc.InitialSelection = ColumnSelectionEnum.Hidden; // hide database column
			return mt;
		}

		/// <summary>
		/// Map import file columns to metatable columns.
		/// </summary>
		/// <param name="mt">Metatable to map to</param>
		/// <param name="types">Mobius metacolumn type</param>
		/// <param name="labels">Label for field</param>
		/// <param name="names">Associated field name in SDfile</param>
		/// <returns></returns>

		Dictionary<string, MetaColumn> MapMetaTableToFile(
				MetaTable mt,
				UserDataImportParms importParms,
				List<MetaColumnType> types,
				List<string> labels,
				List<string> names)
		{

			// Three cases are addressed:
			//  1. NewMetaTable - no mapping & not yet saved, create metacolumns for all file columns
			//  2. UnmappedExistingMetaTable - map in file order up to size of metatable (may be old premap table)
			//  3. MappedExistingMetaTable - use existing mapping to file columns
			// This routine is called to do initial mapping and a 2nd time just before load to be sure
			// the mapping is still valid.  

			MetaColumn mc;
			string msg, mcCode;
			int i1;

			// Get current column mapping info

			MetaTable mt0 = mt.Clone(); // clone original for later reference
			List<MetaColumn> mcs0 = mt0.MetaColumns;

			Dictionary<string, MetaColumn> filePosToMc = new Dictionary<string, MetaColumn>(); // file column id to metacolumn
			Dictionary<string, MetaColumn> existingLabelToMc = new Dictionary<string, MetaColumn>(); // map from existing labels to their metacolumn
			HashSet<string> codesReassigned = new HashSet<string>(); // codes that have been reassigned to new table
			bool newMetaTable = true; // if only metacolumn is an unmapped key then this is a new metatable
			bool unmappedExistingMetaTable = false;
			bool mappedExistingMetaTable = false;
			bool askedKeepExistingTypes = false;
			bool keepExistingTypes = false;

			if (mt.Code != "" && mt.Code != "0") newMetaTable = false; // if code assigned then metatable has been saved
			else
			{
				foreach (MetaColumn mc2 in mt.MetaColumns) // if file position assigned then not new
				{
					if (mc2.ImportFilePosition == "") continue;
					newMetaTable = false;
					break;
				}
			}

			if (!newMetaTable)
			{
				foreach (MetaColumn mc2 in mt.MetaColumns)
				{
					if (mc2.ImportFilePosition == "") continue;
					if (filePosToMc.ContainsKey(mc2.ImportFilePosition))
					{ // doubly assigned
						MessageBoxMx.ShowError("File column " + mc2.ImportFilePosition +
								" is assigned to more than one database column");
						return null;
					}
					else if (Lex.IsInteger(mc2.ImportFilePosition) &&
							Int32.Parse(mc2.ImportFilePosition) > types.Count)
					{ // col beyond those in file
						msg = "Column " + Lex.Dq(mc2.Label) +
								" is assigned to file position " + mc2.ImportFilePosition + "\n" +
								"which larger than the " + types.Count.ToString() + " columns in the file.\n" +
								"No data will be loaded into this column.";
						if (!ShowImportMessage(UmlautMobius.String, msg, true)) return null;
					}
					filePosToMc[mc2.ImportFilePosition] = mc2; // save mapping of file position to metacolumn
					existingLabelToMc[mc2.Label] = mc2; // save mapping of mc label to mc
				}

				if (filePosToMc.Count == 0) unmappedExistingMetaTable = true;
				else
				{
					mappedExistingMetaTable = true;

					if (AnnotationTable && mt.KeyMetaColumn.ImportFilePosition == "")
					{
						MessageBoxMx.ShowError("A column in the file must be assigned to the " +
								mt.KeyMetaColumn.Label + " column");
						return null;
					}
				}
			}

			if (!newMetaTable && importParms.DeleteExisting && !importParms.DeleteDataOnly)
			{ // if deleting existing data, including table definition, then treat as new table 
				int cnt = mt.MetaColumns.Count;
				if (AnnotationTable) mt.MetaColumns.RemoveRange(1, cnt - 1); // keep key col for annotation
				else mt.MetaColumns.RemoveRange(2, cnt - 2); // keep key & str for user database
				foreach (MetaColumn mc2 in mt.MetaColumns) // clear import position
					mc2.ImportFilePosition = "";
				newMetaTable = true; // if only metacolumn is an unmapped key then this is a new metatable
				unmappedExistingMetaTable = false;
				mappedExistingMetaTable = false;
			}

			// Create and/or assign columns checking for matching types

			for (i1 = 0; i1 < types.Count; i1++)
			{
				MetaColumnType type = types[i1];

				if (type == MetaColumnType.Unknown) // assign string type if no defined values for column
					type = MetaColumnType.String;

				else if (type == MetaColumnType.Integer || // store ints & numbers in qualified numbers
						type == MetaColumnType.Number)
					type = MetaColumnType.QualifiedNo;

				if (newMetaTable) // add new metacolumn if needed
				{
					mc = null; // assign structure & key columns first
					if (type == MetaColumnType.Structure)
						mc = mt.FirstStructureMetaColumn;
					else mc = mt.KeyMetaColumn;
					if (mc != null && mc.ImportFilePosition != "") mc = null;
					if (mc != null && labels[i1].ToLower().Contains("comment"))
						mc = null; // don't assign "comment" column to key

					if (mc == null) // create column if no match
					{
						mc = new MetaColumn();
						mc.Name = "Column_" + (i1 + 1).ToString();
						if (labels != null && labels.Count > i1) mc.Label = labels[i1];
						else mc.Label = "Column " + (i1 + 1).ToString();
						mc.DataType = type;
						mc.InitialSelection = ColumnSelectionEnum.Selected;
						mc.SetDefaultFormating();
						mc.MetaTable = mt;
						if (importParms.DeleteExisting && // try to use existing codes matched by label if DeleteExisting 
								!mc.Label.StartsWith("Column ") && // don't match generically named labels
								existingLabelToMc.ContainsKey(mc.Label)) // but matches other col name
						{
							//if (mc.Label == "Dose") mc = mc; // debug
							if (mcs0.Count > i1 && mcs0[i1].Label == mc.Label) // use existing code in same position if label matches 
								mcCode = mcs0[i1].ResultCode; // this handles the case where the same label is used multiple times
							else mcCode = existingLabelToMc[mc.Label].ResultCode; // otherwise assign existing code

							if (!codesReassigned.Contains(mcCode)) // be sure we  haven't already used this code
							{
								mc.ResultCode = mcCode;
								codesReassigned.Add(mcCode);
							}
						}

						mt.MetaColumns.Add(mc);
					}

					string posString = (i1 + 1).ToString();
					if (names != null) posString = names[i1]; // use sdfile name instead
					mc.ImportFilePosition = posString;
					filePosToMc[mc.ImportFilePosition] = mc;
				}

				else
				{
					if (unmappedExistingMetaTable)
					{ // map to positionally corresponding metacolumn
						if (i1 >= mt.MetaColumns.Count) continue; // ignore if file column beyond metatable size
						mc = (MetaColumn)mt.MetaColumns[i1];
						mc.ImportFilePosition = (i1 + 1).ToString();
					}

					else
					{ // get any metacolumn corresponding to this file col
						string posString = (i1 + 1).ToString();
						if (names != null) posString = names[i1]; // use sdfile name instead
						if (!filePosToMc.ContainsKey(posString)) continue;
						mc = filePosToMc[posString];
					}

					if (mc.DataType == MetaColumnType.String ||
							mc.DataType == MetaColumnType.CompoundId ||
							mc.DataType == MetaColumnType.Structure) continue;

					if (mc.DataType != type && InteractiveMode && !importParms.DeleteDataOnly)
					{
						if (!askedKeepExistingTypes)
						{
							msg =
									"Data type in file does not match existing annotation data type.\n" +
									" Column: " + (i1 + 1).ToString() +
									", Existing Type: " + mc.DataType.ToString() +
									", File Type: " + type.ToString() + "\n\n" +
									"Do you want Mobius to keep the existing types?\n" +
									"If you answer no Mobius will attempt to use the new types.\n" +
									"Selecting Cancel will cancel the import.";

							DialogResult dr = MessageBoxMx.Show(msg, "Type Mismatch",
									MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);

							if (dr == DialogResult.Yes) keepExistingTypes = true;
							else if (dr == DialogResult.No) keepExistingTypes = false;
							else return null;

							askedKeepExistingTypes = true;
						}

						if (!keepExistingTypes) mc.DataType = type; // use new type
					}
				}
			}

			return filePosToMc;
		}

		/// <summary>
		/// Import a record joining multiple lines in cases where newlines are contained in "quoted" strings
		/// </summary>
		/// <param name="sr"></param>
		/// <param name="textQualifier"></param>
		/// <returns></returns>

		public static string ReadImportRecord(
				StreamReader sr,
				char textQualifier)
		{
			string rec = null;
			while (true)
			{
				string line = sr.ReadLine();
				if (line == null) return rec; // end of file
				if (textQualifier == ' ') return line;

				if (rec == null) rec = line; // first line
				else rec += " " + line; // replace break with space (do better later)
				int cnt = Lex.CountCharacter(rec, textQualifier);
				if (cnt % 2 == 0) return rec; // all done if even number of qualifier chars
			}
		}

		/// <summary>
		/// Excel text file import-like split of a line into multiple tokens
		/// </summary>
		/// <param name="line"></param>
		/// <param name="delimiter"></param>
		/// <param name="multDelimsAsSingle"></param>
		/// <param name="qualifier"></param>
		/// <returns></returns>

		public static List<string> SplitImportRecord(
				string line,
				char delimiter,
				bool multDelimsAsSingle,
				char qualifier)
		{
			string tok;
			char c;
			bool inQual = false;
			List<string> al = new List<string>();
			int p1 = 0; // first char in current token
			for (int p = 0; p <= line.Length; p++)
			{
				if (p < line.Length) c = line[p];
				else
				{
					c = delimiter; // at end
					inQual = false; // force it
				}

				if (c == delimiter && !inQual) // at end of token?
				{
					if (multDelimsAsSingle && p > 0 && line[p - 1] == delimiter) // ignore this delim?
					{
						p1 = p + 1;
						continue;
					}

					tok = line.Substring(p1, p - p1).Trim();
					if (tok.Length > 0 && tok[0] == qualifier)
					{
						if (qualifier == '\"') tok = Lex.RemoveDoubleQuotes(tok);
						else if (qualifier == '\'') tok = Lex.RemoveSingleQuotes(tok);
					}
					al.Add(tok);
					p1 = p + 1;
				}

				else if (line[p] == qualifier && // entering, leaving or double qualifier
						qualifier != ' ') // if qualifier is space (none) then ignore
				{
					if (!inQual) // entering
						inQual = true;

					else // end of qual or double qual char
					{
						if (p == line.Length - 1 || // at end of line
								line[p + 1] != qualifier) inQual = false; // leaving qualified string
						else p++; // skip looking at next qualifier
					}
				}
			}

			return al;
		}

		/////////////////////////////////////////////////////////////////////
		// Models-tab related methods                                      //
		/////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Set list of selected models in form
		/// </summary>
		/// <param name="dataAssoc"></param>

		void FillModelsGrid(List<UcdbModel> models)
		{
			if (models == null) return;

			Dictionary<long, object> d = FillModelsDataTable(models);

			ModelTree.ExpandAllNodes();

			foreach (TreeListNode n in ModelTree.TreeList.Nodes)
				SetTreeNodeCheckIfSelected(n, d);

			if (ModelTree.Nodes.Count > 0)
				ModelTree.TreeList.Nodes[0].Expanded = true; // be sure top node is expanded
		}

		/// <summary>
		/// Fill DataTable of selected models
		/// </summary>
		/// <param name="models"></param>

		Dictionary<long, object> FillModelsDataTable(List<UcdbModel> models)
		{
			if (models == null) return null;

			ModelsDataTable.Rows.Clear();
			Dictionary<long, object> d = new Dictionary<long, object>();
			for (int mi = 0; mi < models.Count; mi++)
			{
				if (models[mi].PendingStatus == UcdbWaitState.Deletion)
					continue; // skip if pending deletion

				d[models[mi].ModelId] = null;
			}
			// todo: put data in DataTable

			return d;
		}

		public static bool SetTreeNodeCheckIfSelected(
				TreeListNode node,
				Dictionary<long, object> selectedModels)
		{
			if (node.Nodes.Count == 0) // leaf node
			{
				MetaTreeNode mtn = node.Tag as MetaTreeNode;
				if (mtn == null) return false;
				long id = ExtractModelIdFromNodeTarget(mtn);
				if (id >= 0 && selectedModels.ContainsKey(id))
				{
					node.Checked = true;
				}
				else node.Checked = false;
			}

			else // recursively check children
			{
				int setCnt = 0;
				int unSetCnt = 0;
				for (int ni = 0; ni < node.Nodes.Count; ni++) // recursively check children
				{
					TreeListNode n2 = node.Nodes[ni];
					if (n2.GetValue(0).ToString().IndexOf("Select all of the ") >= 0)
					{
						node.Nodes.RemoveAt(ni);
						ni--;
						continue;
					}

					if (SetTreeNodeCheckIfSelected(n2, selectedModels)) setCnt++;
					else unSetCnt++;
				}

				if (setCnt > 0 && unSetCnt == 0)
					node.Checked = true;
				else node.Checked = false;
			}

			return node.Checked;
		}

		/// <summary>
		/// Extract modelId from a node target
		/// Target should have format: "TABLE IRW_12[_BUILD_34]"
		/// </summary>
		/// <param name="node"></param>

		static long ExtractModelIdFromNodeTarget(
				MetaTreeNode node)
		{
			string id = node.Target;
			int i1 = id.ToLower().IndexOf("spm_"); // find beginning of table name
			if (i1 < 0) i1 = id.ToLower().IndexOf("irw_"); // old start of name
			if (i1 < 0) return -1;
			if (id.IndexOf(",") >= 0) return -1; // ignore "all of the above..."

			id = id.Substring(i1 + 4);
			i1 = id.ToLower().IndexOf("_"); // if any build
			if (i1 > 0) id = id.Substring(0, i1);
			return long.Parse(id);
		}

		/// <summary>
		/// Get list of selected models from form updating RowState
		/// </summary>
		/// <returns></returns>

		List<UcdbModel> GetSelectedModels(
				List<UcdbModel> models)
		{
			UcdbModel lda;

			List<UcdbModel> models2 = new List<UcdbModel>();
			Dictionary<long, UcdbModel> mDict = new Dictionary<long, UcdbModel>();

			foreach (UcdbModel m2 in models) // get dict of existing Insilico models
				mDict[m2.ModelId] = m2;

			DataTable dt = ModelsDataTable;
			for (int ri = 1; ri < dt.Rows.Count; ri++)
			{
				DataRow dr = dt.Rows[ri];

				long modelId = (long)dr["ModelId"];

				if (mDict.ContainsKey(modelId)) // existing model
				{
					lda = mDict[modelId]; // get existing model
					if (lda.PendingStatus == UcdbWaitState.Deletion) // re-adding model posted for deletion
					{
						lda.RowState = UcdbRowState.Modified;
						lda.PendingStatus = UcdbWaitState.ModelPredictions;
					}
					models2.Add(lda);
					mDict.Remove(modelId);
				}

				else // new model
				{
					lda = new UcdbModel();
					lda.ModelId = modelId;
					lda.PendingStatus = UcdbWaitState.ModelPredictions;
					lda.RowState = UcdbRowState.Added;
					models2.Add(lda);
				}
			}

			foreach (long modelId2 in mDict.Keys)
			{ // mark remaining models not now selected for deletion
				lda = mDict[modelId2];
				lda.PendingStatus = UcdbWaitState.Deletion;
				lda.RowState = UcdbRowState.Modified;
				models2.Add(lda);
			}

			return models2;
		}

		/// <summary>
		/// Grid cell has been edited 
		/// </summary>
		/// <param name="e"></param>
		/// <param name="ci"></param>
		/// <param name="dr"></param>

		internal void CellTextEdited(CustomColumnDataEventArgs e, CellInfo ci)
		{
			try { SetCellValueFromText(ci, e.Value as string, ""); }
			catch (Exception ex)
			{
				Qm.MoleculeGrid.FocusCell(ci.GridRowHandle, ci.GridColAbsoluteIndex);
				MessageBoxMx.ShowError(ex.Message);
				if (ex.Message == "Not a valid data row") { }
				else
				{
					Grid.FocusCell(ci.GridRowHandle, ci.GridColAbsoluteIndex);
					Grid.BGV.ShowEditor();
				}
			}
		}

		/// <summary>
		/// Update a field in a DataRow due to a user change
		/// </summary>
		/// <param name="e"></param>
		/// <param name="ci"></param>
		/// <param name="dr"></param>
		/// <returns></returns>

		public bool SetCellValueFromText(
				CellInfo ci,
				string content,
				string hyperLink)
		{
			MobiusDataType newValue = null;
			QualifiedNumber newQn;
			string[] sa;

			if (content.Contains("\v") && Lex.IsNullOrEmpty(hyperLink))
			{ // split text containing hyperlink
				sa = content.Split('\v');
				content = sa[0];
				hyperLink = sa[1];
			}

			ResultsFormat rf = Rf;
			int gRow = ci.GridRowHandle;
			int gCol = ci.GridColAbsoluteIndex;
			MoleculeGridControl grid = rf.MolGrid;
			ResultsFormatter formatter = rf.Formatter;
			ResultsField rfld = ci.Rfld;
			if (rfld == null) return false;
			ResultsTable rt = rfld.ResultsTable;
			QueryTable qt = rt.QueryTable;
			QueryColumn qc = rfld.QueryColumn;
			MetaTable mt = rt.MetaTable;
			MetaColumn mc = rfld.MetaColumn;

			if (mt.MetaBrokerType != MetaBrokerType.Annotation &&
					!Lex.StartsWith(mt.Name, "USERDATABASE_"))
			{
				return false;
			}

			DataRowMx dr = DataTable.Rows[ci.DataRowIndex];

			if (mc.IsKey) // special handling for key field
				return UpdateTupleCidField(rf, rfld, dr, content);

			if (content == null) content = "";
			content = content.Trim();

			if (content == "" && dr[ci.DataColIndex] is DBNull) return false; // nothing to do

			if (mc.DataType == MetaColumnType.Structure)
			{
				if (content == "") newValue = new MoleculeMx();
				else newValue = new MoleculeMx(content); // structure for user database structure table
			}

			else if (mc.DataType == MetaColumnType.Integer ||
			 mc.DataType == MetaColumnType.Number)
			{
				if (content == "") newValue = new NumberMx();
				else
				{
					if (!Lex.IsDouble(content))
						throw new Exception(content + " is not a valid number");

					newValue = new NumberMx(content);
				}
			}

			else if (mc.DataType == MetaColumnType.QualifiedNo)
			{
				QualifiedNumber parsedQn;
				newValue = newQn = new QualifiedNumber();
				if (content != "")
				{
					if (QualifiedNumber.TryParse(content, out parsedQn))
						newValue = parsedQn;
					else newQn.TextValue = content; // if not a numeric qualified number store text as is
				}
			}

			else if (mc.DataType == MetaColumnType.Date)
			{
				if (content == "") newValue = new DateTimeMx();
				else
				{
					string tok = DateTimeMx.Normalize(content);
					if (tok == null)
						throw new Exception(content + " is not a valid date");

					DateTime dt = DateTimeMx.NormalizedToDateTime(tok);
					newValue = new DateTimeMx(dt);
				}
			}

			else if (mc.DataType == MetaColumnType.String)
			{
				if (content == "") newValue = new StringMx();
				else newValue = new StringMx(content);
			}

			else
			{
				MessageBoxMx.ShowError("Not updated, unexpected data type: " + mc.DataType);
				return false;
			}

			newValue.Hyperlink = hyperLink;
			SetCellValue(ci, newValue);
			return true;
		}

		/// <summary>
		/// Update a field in a DataRow due to a user change and handle update of any underlying user data
		/// </summary>
		/// <param name="ci"></param>
		/// <param name="newValue"></param>
		/// <returns></returns>

		public void SetCellValue(
				CellInfo ci,
				MobiusDataType newValue)
		{
			long oldDbLink;
			DataRowMx dr = DataTable.Rows[ci.DataRowIndex];
			MobiusDataType oldValue = dr[ci.DataColIndex] as MobiusDataType; // get any existing value
			dr[ci.DataColIndex] = newValue; // store the value

			//if (ci.Mt.MetaBrokerType != MetaBrokerType.Annotation &&
			// !Lex.StartsWith(ci.Mt.Name, "USERDATABASE_"))
			//  return;

			if (NullValue.IsNull(oldValue) && NullValue.IsNull(newValue)) return;

			if (newValue == null)
			{ // if newValue is reall null then create a null MobiusDataType object of the proper type to hold the old DbLink info
				newValue = MobiusDataType.New(ci.Mc.DataType);
				dr[ci.DataColIndex] = newValue; // store the value
			}

			if (oldValue != null) // get any old result id
				oldDbLink = ParseDbLink(oldValue.DbLink);
			else oldDbLink = 0;
			newValue.DbLink = oldDbLink.ToString();

			newValue.Modified = true; // indicate modified

			// Update row state & do any immediate update

			if (AttrVoPos >= 0)
			{
				DataRowAttributes dra = Dtm.GetRowAttributes(dr);

				int qti = Qm.Query.GetQueryTableIndex(ci.Qt);
				if (dra != null && dra.TableRowState[qti] != RowStateEnum.Added)
					dra.TableRowState[qti] = RowStateEnum.Modified; // say modified if not an added row

				if (Grid.UpdateImmediately) // if in normal results viewer (i.e. not separate editor) do db update immediately
					SaveUserData(false);
				else if (Grid.Editor != null) Dtm.DataModified = true; // mark data as modified
			}

			// Refresh grid with new value

			newValue.FormattedText = null; // clear any formatted values
			newValue.FormattedBitmap = null;
			Grid.RefreshRowCell(ci.GridRowHandle, ci.GridColumn);

			return;
		}

		/// <summary>
		/// Insert/update single annotation value with a single call to the service
		/// </summary>
		/// <param name="rfld"></param>
		/// <param name="dr"></param>
		/// <param name="rsltGrpId"></param>
		/// <param name="forceUpdate"></param>
		/// <param name="voBuffer">If not null put row in this buffer</param>
		/// <param name="aDao"></param>

		void UpdateAnnotationResult(
				ResultsField rfld,
				DataRowMx dr,
				long rsltGrpId,
				bool forceUpdate,
				List<AnnotationVo> voBuffer,
				AnnotationDao aDao)
		{
			AnnotationVo awVo, awVo2;
			string[] sa;
			MobiusDataType mdt, mdt2;
			ResultsField rfld2 = null;
			QualifiedNumber qn;
			NumberMx nx;
			StringMx sx;
			DateTimeMx dtx;
			long rsltId, rsltId2, newRsltId;
			int i1;

			try
			{
				ResultsTable rt = rfld.ResultsTable;
				MetaTable mt = rt.MetaTable;
				MetaColumn mc = rfld.MetaColumn;
				int voPos = rfld.VoPosition;
				object o = dr[voPos];

				mdt = o as MobiusDataType;
				if (mdt == null) return; // nothing to do

				if (!mdt.Modified && !forceUpdate) return; // no change to value

				rsltId = ParseDbLink(mdt.DbLink);
				if (rsltId > 0)
				{ // if already exists, archive the old value before storing new value
					if (mdt.IsNull)
					{
						if (aDao == null) aDao = new AnnotationDao();
						aDao.SetArchiveStatus(rsltId); // archive it
						aDao.Dispose();
						dr[voPos] = DBNull.Value;
						return; // done if deleting
					}
					rsltGrpId = -1; // rsltId is for the value being replaced
				}

				else if (rsltGrpId <= 0) // see if any other values for this row so we can get the group Id
				{
					for (i1 = 0; i1 < rt.Fields.Count; i1++) // look for existing rsltGrpId
					{
						rfld2 = rt.Fields[i1];
						if (rfld2.MetaColumn.IsKey) continue;
						if (dr[rfld2.VoPosition] != null && dr[rfld2.VoPosition] != DBNull.Value)
						{
							mdt2 = dr[rfld2.VoPosition] as MobiusDataType;
							rsltId2 = ParseDbLink(mdt2.DbLink);
							if (rsltId2 <= 0) continue; // no result id assigned
							rsltId = rsltId2; // pass resultId of other result that will have same group id
							rsltGrpId = -2; // rsltId is for another result that has the groupid we want to use
							break;
						}
					}

					if (rsltGrpId <= 0) // new results row: copy key value  & get new result group id
					{
						rfld2 = rfld.ResultsTable.Fields[0]; // get position for key
						if (rfld2.MetaColumn.IsKey) // if first results field is a key then copy keye here
							i1 = rfld2.VoPosition;
						else i1 = rfld2.VoPosition - 1; // otherwise assume key is in vo just before first results field
						dr[i1] = dr[KeyVoPos]; // copy key value since may not exist
					}
				}

				// Finish setting up & storing new value

				awVo = new AnnotationVo();
				awVo.rslt_grp_id = rsltGrpId; // set id to group results together in row
				awVo.rslt_id = rsltId; // existing result id
				mdt.DbLink = awVo.rslt_id.ToString();
				mdt.Modified = false; // reset modified flag

				string qualifier = "";
				string text = "";
				double number = NullValue.NullNumber;
				DateTime date = DateTime.MinValue;
				string hyperLink = mdt.Hyperlink;

				if (mc.DataType == MetaColumnType.Structure)
				{
					MoleculeMx mol = mdt as MoleculeMx;
					if (mol.IsChemStructureFormat)
						text = "chime=" + mol.GetChimeString();

					else if (mol.IsBiopolymerFormat)
						text = "helm=" + mol.GetHelmString();

					else text = "";
				}

				else if (mc.DataType == MetaColumnType.Integer ||
				 mc.DataType == MetaColumnType.Number)
				{
					nx = mdt as NumberMx;
					if (nx == null && mdt != null) throw new Exception("Expected NumberEx but saw " + mdt.GetType());
					number = nx.Value;
				}

				else if (mc.DataType == MetaColumnType.QualifiedNo)
				{
					qn = mdt as QualifiedNumber;
					if (qn == null && mdt != null) throw new Exception("Expected QualifiedNumber but saw " + mdt.GetType());
					qualifier = qn.Qualifier;
					number = qn.NumberValue;
					text = qn.TextValue;
				}

				else if (mc.DataType == MetaColumnType.Date)
				{
					dtx = mdt as DateTimeMx;
					if (dtx == null && mdt != null) throw new Exception("Expected DateTimeEx but saw " + mdt.GetType());
					date = dtx.Value;
				}

				else if (mc.DataType == MetaColumnType.String)
				{
					sx = mdt as StringMx;
					if (sx == null && mdt != null) throw new Exception("Expected StringEx but saw " + mdt.GetType());
					text = sx.Value;

					if (QualifiedNumber.TryParse(text, out qn))
					{ // if numeric store a number also so type change to number works properly
						qualifier = qn.Qualifier;
						number = qn.NumberValue;
					}
				}

				else throw new Exception("Unexpected data type: " + mc.DataType);

				awVo.ext_cmpnd_id_txt = NormalizeCompoundIdForDatabase((string)dr[KeyVoPos], mt);
				Int32.TryParse((string)dr[KeyVoPos], out awVo.ext_cmpnd_id_nbr); // set ext_cmpnd_id_nbr if numeric
				awVo.mthd_vrsn_id = Int32.Parse(mt.Code);
				awVo.rslt_typ_id = long.Parse(mc.ResultCode);
				awVo.rslt_val_prfx_txt = qualifier;
				awVo.rslt_val_nbr = number;
				awVo.rslt_val_txt = text;
				awVo.rslt_val_dt = date;
				awVo.dc_lnk = hyperLink;
				awVo.chng_op_cd = "I";
				awVo.chng_usr_id = SS.I.UserName;

				if (voBuffer != null) // just add to buffer
				{
					voBuffer.Add(awVo);
					return;
				}

				else if (aDao != null) // update for existing DAO
					newRsltId = aDao.InsertUpdateRow(awVo);

				else newRsltId = AnnotationDao.InsertUpdateRowAndUserObjectHeader(awVo); // fast encapsulated update

				mdt.DbLink = newRsltId.ToString();
				mdt.Modified = false; // reset modified flag

				return;
			}

			catch (Exception ex)
			{
				MessageBoxMx.ShowError("Not updated: " + ex.Message);
				return;
			}

		}

		/// <summary>
		/// Update compound id cell data
		/// </summary>
		/// <param name="rf"></param>
		/// <param name="rfld"></param>
		/// <param name="dr"></param>
		/// <param name="newCid"></param>
		/// <returns></returns>

		public bool UpdateTupleCidField(
				ResultsFormat rf,
				ResultsField rfld,
				DataRowMx dr,
				string newCid)
		{

			// This code checks the validity of the cid for several cases
			// 1. Cid change
			//   A. New
			//   B. Modified
			//   C. Deleted - not allowed
			// 2. Associated query type
			//   A. Root & possibly lower level tables
			//   B. Single non-root table
			// 3. Database update mode
			//   A. In Editor & doing transactions 
			//   B. Immediate update

			MoleculeGridControl grid = rf.MolGrid;
			UserDataEditor editor = grid.Editor;
			ResultsFormatter formatter = rf.Formatter;
			ResultsTable rt = rfld.ResultsTable;
			QueryTable qt = rt.QueryTable;
			MetaTable mt = qt.MetaTable;
			string oldCid;
			int qti = rf.Query.GetQueryTableIndex(qt);

			DataRowAttributes dra = Dtm.GetRowAttributes(dr);

			object oldCidObj = dr[rfld.VoPosition]; // get any existing value
			if (NullValue.IsNull(oldCidObj)) oldCid = null;
			else if (oldCidObj is CompoundId) oldCid = ((CompoundId)oldCidObj).Value;
			else oldCid = oldCidObj as string;

			if (newCid != null && newCid != "")
			{
				string normalizedNewCid = CompoundId.Normalize(newCid, mt); // do 2-step normalize since this step will remove bad suffixes, e.g. 123xyz
				string dbNewCid = NormalizeCompoundIdForDatabase(normalizedNewCid, mt);
				if (String.IsNullOrEmpty(dbNewCid))
					throw new Exception(rfld.MetaColumn.Label + " " + CompoundId.Format(newCid, mt) + " is not in a recognized format for this database.");

				newCid = dbNewCid;
			}

			if ((String.IsNullOrEmpty(newCid) && String.IsNullOrEmpty(oldCid)) ||
					newCid == oldCid) return true; // no change

			else if (String.IsNullOrEmpty(newCid))  // trying to null cid
				throw new Exception("An existing compound id can't be blanked");

			else if (mt.IsRootTable && CompoundIdUtil.Exists(newCid, mt))
			{ // if root table change then be sure newcid doesn't match an existing database cid
				foreach (ResultsField rfld2 in rfld.ResultsTable.Fields)
				{ // may be adding additional row for non-root table so check for other non-null root values
					if (rfld == rfld2) continue;
					if (dr[rfld2.VoPosition] != null)
						throw new Exception(rfld.MetaColumn.Label + " " + CompoundId.Format(newCid, mt) + " can't be used since it already exists in the database.");
				}
			}

			else if (!mt.IsRootTable && !CompoundIdUtil.Exists(newCid, mt.Root))
				throw new Exception( // if not root table change then be sure newcid exists in the associated root table
				 rfld.MetaColumn.Label + " " + CompoundId.Format(newCid, mt) + " doesn't exist in the underlying database");

			dr[rfld.VoPosition] = newCid; // store for this table
			dr[KeyVoPos] = newCid; // also in first position

			if (dra != null) // mark for update unless new row
			{
				RowStateEnum rs = dra.TableRowState[qti]; // change rowstate
				if (rs == RowStateEnum.Undefined) dra.TableRowState[qti] = RowStateEnum.Added;
				else if (rs != RowStateEnum.Added) dra.TableRowState[qti] = RowStateEnum.Modified;

				if (oldCid != null && dra.OriginalKey == null)
					dra.OriginalKey = oldCid; // save any old key value so we can do update
			}

			if (Grid.UpdateImmediately) // if in normal results viewer (i.e. not separate editor) do db update immediately
				SaveUserData(true);

			else if (Grid.Editor != null) Dtm.DataModified = true; // mark data as modified

			return true;
		}

		/// <summary>
		/// Assign & set cell cid if null
		/// </summary>
		/// <param name="cell"></param>

		public void AssignAndSetCidIfNull(
				CellInfo cell) // set the cid if needed
		{
			string cid, fCid;

			QueryTable qt = cell.Rfld.ResultsTable.QueryTable;
			MetaTable mt = qt.MetaTable;

			DataRowMx dr = DataTable.Rows[cell.DataRowIndex];
			if (dr[KeyVoPos] != null) cid = dr[KeyVoPos].ToString();

			else // assign new cid, store in tuple buffer & display in grid
			{
				cid = GetNextCid();
				cid = NormalizeCompoundIdForDatabase(cid, mt.Root);
				dr[KeyVoPos] = cid; // put cid in buffer

				fCid = CompoundId.Format(cid, mt.Root); // user format
				int gridCol = 1; // grid col for cid
			}

			int voPos = qt.KeyQueryColumn.VoPosition; // be sure cid stored for this table as well
			dr[voPos] = new CompoundId(cid);

			return;
		}

		/// <summary>
		/// Get the next sequential numeric compound id for db
		/// </summary>
		/// <returns></returns>

		public string GetNextCid()
		{
			if (MaxCid <= 0)
			{
				if (Udb.DatabaseId > 0)
				{
					MaxCid = Udbs.GetMaxCompoundId(Udb.DatabaseId);
					if (MaxCid <= 0) // if no numeric ids then use compound count + 1
						MaxCid = Udbs.SelectDatabaseCompoundCount(Udb.DatabaseId);
				}
				else MaxCid = 0; // new db
			}
			MaxCid++;

			string cid = NormalizeCompoundIdForDatabase(MaxCid.ToString(), SMt);
			return cid;
		}

		/// <summary>
		/// Parse a dblink
		/// </summary>
		/// <param name="dbLink"></param>
		/// <returns></returns>

		long ParseDbLink(string dbLink)
		{
			if (String.IsNullOrEmpty(dbLink)) return 0;
			else return long.Parse(dbLink);
		}
	}

}
