using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class UserDatabasesExplorer : DevExpress.XtraEditors.XtraForm
	{
		static UserDatabasesExplorer Instance;

		DataTable DataTable; // data displayed in grid
		UserCmpndDbDao Udbs;
		string OwnerUserName = null;
		bool FailedOnly = false;
		string Response = "";

		public UserDatabasesExplorer()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Explore user databases
		/// </summary>
		/// <returns></returns>

		public static string Explore(
			string args)
		{
			UcdbDatabase ucdb, ucdb2;
			long databaseId;
			UserObject uo;
			UserDataEditor editor;
			UcdbDatabase[] ucdbArray;
			List<UserObject> uoList = null;
			Dictionary<long, UserObject> uoDict = null;
			int li, i1;

			if (Instance == null) // create & setup form 1st time
			{
				Instance = new UserDatabasesExplorer();
				Instance.CreateDataTable();
				Instance.Grid.DataSource = Instance.DataTable;
			}

			Instance.Udbs = new UserCmpndDbDao(); // user database database service instance

			if (args.StartsWith("Failed", StringComparison.OrdinalIgnoreCase)) // only show databases with failed updates
				Instance.FailedOnly = true;

			else Instance.OwnerUserName = args;

			Instance.FillDataTable();

			Instance.Response = "";
			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			return Instance.Response;
		}

		void CreateDataTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn("UserDatabaseName", typeof(string)));
			dt.Columns.Add(new DataColumn("Public", typeof(bool)));
			dt.Columns.Add(new DataColumn("Molecules", typeof(int)));
			dt.Columns.Add(new DataColumn("PredictiveModels", typeof(int)));
			dt.Columns.Add(new DataColumn("PendingUpdates", typeof(int)));
			dt.Columns.Add(new DataColumn("Created", typeof(DateTime)));
			dt.Columns.Add(new DataColumn("Updated", typeof(DateTime)));
			dt.Columns.Add(new DataColumn("OwnerUserName", typeof(string)));
			dt.Columns.Add(new DataColumn("DatabaseId", typeof(int)));
			dt.Columns.Add(new DataColumn("Ucdb", typeof(UcdbDatabase)));

			DataTable = dt;
		}

		void FillDataTable()
		{
			UcdbDatabase ucdb, ucdb2;
			string headerString;
			long databaseId;
			UserObject uo;
			UserDataEditor editor;
			UcdbDatabase[] ucdbArray;
			List<UcdbDatabase> ucdbList = new List<UcdbDatabase>();
			List<UserObject> uoList = null;
			Dictionary<long, UserObject> uoDict = null;
			int li, i1;

			//if (!FailedOnly) // Get list of databases
			{
				if (OwnerUserName != "") // show databases for specified user
					ucdbArray = Udbs.SelectDatabaseHeaders(OwnerUserName);
				else ucdbArray = Udbs.SelectDatabaseHeaders(); // get for all users

			}

			//else ucdbArray = Udbs.SelectFailedUpdates(); // show databases with failed updates

			foreach (UcdbDatabase ucdb0 in ucdbArray)
			{
				uo = UserObjectDao.Read(UserObjectType.UserDatabase, ucdb0.OwnerUserName, ucdb0.NameSpace, ucdb0.Name);
				if (uo != null) ucdbList.Add(ucdb0);
			}

			DataTable.Rows.Clear();
			foreach (UcdbDatabase ucdb0 in ucdbList)
			{
				DataRow dr = DataTable.NewRow();
				SetupDataRow(dr, ucdb0);
				DataTable.Rows.Add(dr);
			}

			return;
		}

		void RenumberRows() // todo
		{
			return;
		}

		void SetupDataRow(DataRow dr, UcdbDatabase ucdb)
		{
			dr["UserDatabaseName"] = ucdb.Name;
			dr["Public"] = ucdb.Public;
			dr["Molecules"] = ucdb.CompoundCount;
			dr["PredictiveModels"] = ucdb.ModelCount;
			dr["PendingUpdates"] = ucdb.PendingCompoundId;
			dr["Created"] = ucdb.CreationDate;
			dr["Updated"] = ucdb.UpdateDate;
			dr["OwnerUserName"] = ucdb.OwnerUserName;
			dr["DatabaseId"] = ucdb.DatabaseId;
			dr["Ucdb"] = ucdb; // ref to the ucdb, not displayed
		}

/// <summary>
/// Create a new database
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void NewDatabase_Click(object sender, EventArgs e)
		{
			UserDataEditor editor;
			Dictionary<long, UserObject> uoDict = null;
			UcdbDatabase ucdb, ucdb2;
			UserObject uo;
			DataRow dr;
			long databaseId;
			string headerString;
			int li;

			editor = new UserDataEditor();
			uo = new UserObject(UserObjectType.UserDatabase);
			uo = editor.Edit(uo);
			if (uo == null) return; // edit cancelled

			databaseId = Int64.Parse(uo.Content); // get the database id
			ucdb = Udbs.SelectDatabaseHeader(databaseId);
			if (ucdb == null) return; // shouldn't happen

			for (int ri = 0; ri<DataTable.Rows.Count; ri++)
			{
				dr = DataTable.Rows[ri];
				ucdb2 = (UcdbDatabase)dr["Ucdb"];

				if (Lex.Eq(ucdb2.NameSpace, ucdb.NameSpace) &&
				 Lex.Eq(ucdb2.Name, ucdb.Name))
				{ // remove any database same name
					DataTable.Rows.RemoveAt(ri);
					RenumberRows();
					break;
				}
			}

			dr = DataTable.NewRow();
			SetupDataRow(dr, ucdb);
			DataTable.Rows.Add(dr);
		}

/// <summary>
/// Edit the database
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void EditDatabaseMenuItem_Click(object sender, EventArgs e)
		{
			EditDatabase_Click(sender, e);
		}

		private void EditDatabase_Click(object sender, EventArgs e)
		{
			UcdbDatabase ucdb, ucdb2;
			UserObject uo;
			DataRow dr;
			UserDataEditor editor;
			long databaseId;
			string headerString;
			int ri, li;

			if (!GetFocusedUo(out ri, out dr, out ucdb, out uo)) return;

			editor = new UserDataEditor();
			uo = editor.Edit(uo);
			if (uo == null) return; // edit cancelled

			ucdb2 = Udbs.SelectDatabaseHeader(ucdb.DatabaseId); // get new form of database
			if (ucdb2 == null) return; // shouldn't happen

			if (ucdb2.PendingStatus == UcdbWaitState.ModelPredictions)
				ucdb2.PendingUpdateDate = DateTime.Now; // assume started

			if (Lex.Eq(uo.ParentFolder, ucdb.NameSpace) && Lex.Eq(uo.Name, ucdb.Name)) // no name change, same database
				SetupDataRow(dr, ucdb2);

			else // saved as new database (not currently supported)
			{
				dr = DataTable.NewRow();
				SetupDataRow(dr, ucdb2);
				DataTable.Rows.Add(dr);
			}
		}

/// <summary>
/// Delete a database
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void DeleteDatabaseMenuItem_Click(object sender, EventArgs e)
		{
			DeleteDatabase_Click(sender, e);
		}

		private void DeleteDatabase_Click(object sender, EventArgs e)
		{
			DataRow dr;
			UcdbDatabase ucdb;
			int ri;

			ri = GridView.FocusedRowHandle;
			if (ri < 0)
			{
				MessageBoxMx.ShowError("You must select the user compound database first");
				return;
			}

			dr = DataTable.Rows[ri];
			ucdb = (UcdbDatabase)dr["Ucdb"];

			DialogResult dR = MessageBoxMx.Show("Are you sure you want to delete user compound database: \"" +
				ucdb.Name + "\"?", UmlautMobius.String, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			if (dR != DialogResult.Yes) return;

			ucdb.RowState = UcdbRowState.Deleted; // mark for deletion
			Udbs.UpdateDatabase(ucdb); // mark data for deletion

			DataTable.Rows.RemoveAt(ri);
			RenumberRows();
		}

/// <summary>
/// Build a query for the database
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void BuildQueryMenuItem_Click(object sender, EventArgs e)
		{
			BuildDatabaseQuery_Click(sender, e);
		}

		private void BuildDatabaseQuery_Click(object sender, EventArgs e)
		{
			DataRow dr;
			UcdbDatabase ucdb;
			UserObject uo;
			int ri;

			if (!GetFocusedUo(out ri, out dr, out ucdb, out uo)) return;

			if (!UserData.BuildDatabaseQuery(uo)) return; // some error

			QbUtil.RenderQuery();
			DialogResult = DialogResult.OK;
		}

/// <summary>
/// Build & run query on database
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void RunQueryMenuItem_Click(object sender, EventArgs e)
		{
			RunDatabaseQuery_Click(sender, e);
		}

		private void RunDatabaseQuery_Click(object sender, EventArgs e)
		{
			DataRow dr;
			UcdbDatabase ucdb;
			UserObject uo;
			int ri;

			if (!GetFocusedUo(out ri, out dr, out ucdb, out uo)) return;

			if (!UserData.BuildDatabaseQuery(uo)) return; // some error

			QbUtil.RenderQuery();

			QueryTable qt = QbUtil.Query.Tables[0]; // root table
			QueryColumn qc = qt.GetQueryColumnByName("MolFormula");
			if (qc == null) throw new Exception("MolFormula not found in root table");
			MetaColumn mc = qc.MetaColumn;

			qc.CriteriaDisplay = "All Data Rows"; // do all data query on molformula
			qc.Criteria = "(" + mc.Name + " IS NOT NULL OR " + mc.Name + " IS NULL)";
			QbUtil.RenderQuery(0);

			Response = "Command RunQuery";
			DialogResult = DialogResult.OK;
		}


		/// <summary>
		/// Recalculate model results
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

#if false
		private void RecalculateModelResultsMenuItem_Click(object sender, EventArgs e)
		{
			DataRow dr;
			UcdbDatabase ucdb, ucdb2;
			UserObject uo;
			int ri;

			if (!GetFocusedUo(out ri, out dr, out ucdb, out uo)) return;

			ucdb2 = Udbs.SelectDatabaseHeader(ucdb.DatabaseId); // get new form of database
			if (ucdb2 == null) return; // shouldn't happen

			if (Udbs.UpdateIsRunning(ucdb2))
			{
				MessageBoxMx.ShowError("Update is currently running");
				return;
			}

			UcdbModel[] models = Udbs.SelectDatabaseModels(ucdb.DatabaseId);
			if (models.Length == 0)
			{
				MessageBoxMx.ShowError("No models for database");
				return;
			}

			ucdb2.PendingCompoundCount = ucdb2.CompoundCount;
			ucdb2.PendingCompoundId = 0;
			ucdb2.PendingUpdateDate = DateTime.MinValue;
			ucdb2.PendingStatus = UcdbWaitState.ModelPredictions;
			ucdb2.RowState = UcdbRowState.Modified;
			Udbs.UpdateDatabase(ucdb2);

			foreach (UcdbModel model in models)
			{
				model.PendingStatus = UcdbWaitState.ModelPredictions;
				model.RowState = UcdbRowState.Modified;
			}
			Udbs.UpdateDatabaseModelAssoc(ucdb2, models);

			string command = "UpdateUcdbModelResults Pending " + ucdb2.DatabaseId;
			CommandLine.StartBackgroundSession(command);

			ucdb2.PendingUpdateDate = DateTime.Now; // set value assuming background process started;
			dr["Ucdb"] = ucdb2;
			SetupDataRow(dr, ucdb2);
		}


/// <summary>
/// Restart any pending updates
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void RestartPendingUpdatesMenuItem_Click(object sender, EventArgs e)
		{
			DataRow dr;
			UcdbDatabase ucdb, ucdb2;
			UserObject uo;
			int ri;

			if (!GetFocusedUo(out ri, out dr, out ucdb, out uo)) return;

			ucdb2 = Udbs.SelectDatabaseHeader(ucdb.DatabaseId); // get new form of database
			if (ucdb2 == null) return; // shouldn't happen

			if (ucdb2.PendingStatus == UcdbWaitState.Complete)
			{
				MessageBoxMx.ShowError("All pending updates are complete");
				return;
			}

			if (Udbs.UpdateIsRunning(ucdb2))
			{
				MessageBoxMx.ShowError("Update is currently running");
				return;
			}

			string command = "UpdateUcdbModelResults Pending " + ucdb2.DatabaseId;
			CommandLine.StartBackgroundSession(command);

			ucdb2.PendingUpdateDate = DateTime.Now; // assume started;
			dr["Ucdb"] = ucdb2;
			SetupDataRow(dr, ucdb2);
		}
#endif


		/// <summary>
		/// Read in focused UserObject
		/// </summary>
		/// <param name="ri"></param>
		/// <param name="dr"></param>
		/// <param name="ucdb"></param>
		/// <param name="uo"></param>
		/// <returns></returns>

		bool GetFocusedUo(out int ri, out DataRow dr, out UcdbDatabase ucdb, out UserObject uo)
		{
			dr = null;
			ucdb = null;
			uo = null;

			ri = GridView.FocusedRowHandle;


			if (ri < 0)
			{
				MessageBoxMx.ShowError("You must select the user compound database first");
				return false;
			}

			dr = DataTable.Rows[ri];
			ucdb = (UcdbDatabase)dr["Ucdb"];

			uo = UserObjectDao.Read(UserObjectType.UserDatabase, ucdb.OwnerUserName, ucdb.NameSpace, ucdb.Name);
			if (uo == null)
			{
				MessageBoxMx.ShowError("Unable to find associated user database \"UserObject\" information");
				return false;
			}

			return true;
		}

		private void GridView_MouseDown(object sender, MouseEventArgs e)
		{
			GridHitInfo hi = GridView.CalcHitInfo(e.Location);

			int gr = hi.RowHandle;
			if (hi.Column == null) return;

			int gc = hi.Column.AbsoluteIndex;

			if (e.Button == MouseButtons.Right)
			{
				GridContextMenu.Show(Grid,
					new System.Drawing.Point(e.X, e.Y));
			}

			else if (e.Clicks == 2)
				EditDatabase_Click(null, null);
		}

	}

}