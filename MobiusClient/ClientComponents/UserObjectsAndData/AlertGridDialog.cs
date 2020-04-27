using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class AlertGridDialog : DevExpress.XtraEditors.XtraForm
	{
		DataTable DataTable = null; // DataTable of grid information

		GridView V { get { return GridView; } }

		public AlertGridDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Display the grid of alerts & let user add, edit and cancel them.
		/// </summary>
		/// <returns></returns>
		public static string Show(string args)
		{
			List<UserObject> alerts;
			Alert alert = null;
			int alertId, queryId, row;
			string txt;

			string userid = SS.I.UserName;

			if (args == null || args == "")
			{
				Progress.Show("Retrieving alerts...", UmlautMobius.String, false);
				alerts = UserObjectDao.ReadMultiple(UserObjectType.Alert, SS.I.UserName, false, false);
			}

			else
			{
				if (!Security.IsAdministrator(SS.I.UserName)) return "Only administrators can execute this command";
				Progress.Show("Retrieving alerts...", UmlautMobius.String, false);
				if (Lex.Eq(args, "All")) // all users
					alerts = UserObjectDao.ReadMultiple(UserObjectType.Alert, false);
				else // some other user
					alerts = UserObjectDao.ReadMultiple(UserObjectType.Alert, args, false, false);
			}

			AlertGridDialog Instance = new AlertGridDialog();
			Instance.SetupGrid(alerts);
			Progress.Hide();

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			return "";
		}

		void SetupGrid(List<UserObject> alerts)
		{
		    string txt, tok;

			DataTable dt = CreateDataTable();
			foreach (UserObject uo in alerts) // fill the grid
			{
				UserObject uo2 = uo;
                Alert alert = Alert.GetAlertFromUserObject(uo2,false);

				UserObject quo = UserObjectDao.ReadHeader(alert.QueryObjId); // get query header for name
				if (quo == null) continue; // associated query missing?
				alert.QueryName = quo.Name;
				alert.LastQueryUpdate = quo.UpdateDateTime;

				DataRow dr = dt.NewRow();
				SetDataRow(dr, alert);
				dt.Rows.Add(dr);
			}

			Grid.DataSource = dt;
			Grid.Refresh();

			if (ServicesIniFile.Read("AlertHelpUrl") != "")
				Help.Enabled = true;

			return;
		}

		DataTable CreateDataTable()
		{
			DataTable dt = new DataTable();
			DataTable = dt; // save ref to table
			dt.Columns.Add("AlertId", typeof(int));
			dt.Columns.Add("QueryName", typeof(string));
			dt.Columns.Add("QueryLastModified", typeof(DateTime));
			dt.Columns.Add("CheckInterval", typeof(int));
			dt.Columns.Add("AlertLastChecked", typeof(DateTime));
			dt.Columns.Add("LastCheckThatFoundNewData", typeof(DateTime));
			dt.Columns.Add("NewCompounds", typeof(int));
            dt.Columns.Add("ChangedCompounds", typeof(int));
			dt.Columns.Add("TotalCompounds", typeof(int));
			dt.Columns.Add("NewChangedRows", typeof(int));
			dt.Columns.Add("TotalRows", typeof(int));
			dt.Columns.Add("LastCheckExecutionTime", typeof(TimeSpan));
			dt.Columns.Add("SendTo", typeof(string));
			dt.Columns.Add("Owner", typeof(string));
			dt.Columns.Add("QueryId", typeof(int));

			return dt;
		}

		void SetDataRow(DataRow dr, Alert alert)
		{
			string txt;

			dr["AlertId"] = alert.Id;

			dr["QueryName"] = alert.QueryName;
			if (alert.LastQueryUpdate != DateTime.MinValue)
				dr["QueryLastModified"] = alert.LastQueryUpdate;
			else dr["QueryLastModified"] = DBNull.Value;

			dr["CheckInterval"] = alert.Interval;
			dr["AlertLastChecked"] = alert.LastCheck;
			dr["LastCheckThatFoundNewData"] = alert.LastNewData;

			if (alert.LastCheckExecutionTime >= 0)
				dr["LastCheckExecutionTime"] = new TimeSpan(0, 0, alert.LastCheckExecutionTime);
			else dr["LastCheckExecutionTime"] = DBNull.Value;

			dr["NewCompounds"] = alert.NewCompounds;
            dr["ChangedCompounds"] = alert.ChangedCompounds;
		    dr["TotalCompounds"] = alert.TotalCompounds;    //alert.ChangedCompounds;
			dr["NewChangedRows"] = alert.NewRows;
			dr["TotalRows"] = alert.TotalRows;
			txt = alert.MailTo.Replace("\r\n", ", "); // remove newlines
			dr["SendTo"] = txt;
			txt = alert.Owner;
			try { txt = SecurityUtil.GetShortPersonNameReversed(alert.Owner); }
			catch (Exception ex) { }
			dr["Owner"] = txt;
			dr["QueryId"] = alert.QueryObjId;

			return;
		}

		private void NewAlert_Click(object sender, EventArgs e)
		{
			Alert alert;
			int queryId, alertId;

			queryId = 0;
			if (QbUtil.Query != null && QbUtil.Query.UserObject.Id > 0 &&
				Alert.GetAlertByQueryId(QbUtil.Query.UserObject.Id) == null)
				queryId = QbUtil.Query.UserObject.Id;
			alertId = AlertProperties.Edit(queryId, out alert); // new alert
			if (alertId > 0)
			{
				DataRow dr = DataTable.NewRow();
				SetDataRow(dr, alert);
				DataTable.Rows.Add(dr);
				Grid.Refresh();
			}

			return;
		}

		private void EditAlert_Click(object sender, EventArgs e)
		{
			Alert alert;
			int row, alertId;

			row = V.GetFocusedDataSourceRowIndex();
			if (row < 0) return;
			alertId = (int)DataTable.Rows[row]["AlertId"];
			alertId = AlertProperties.Edit(alertId, out alert); // may cancel also
			if (alertId > 0)
			{
				SetDataRow(DataTable.Rows[row], alert);
			}

			else if (alertId < 0) // cancelled alert
				DataTable.Rows.RemoveAt(row);

			Grid.Refresh();
			return;
		}

/// <summary>
/// Run alert in background
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void RunAlert_Click(object sender, EventArgs e)
		{
			int row, alertId;
			string msg = "";

			row = V.GetFocusedDataSourceRowIndex();
			if (row < 0) return;
			alertId = (int)DataTable.Rows[row]["AlertId"];

			string command = "Check Alert " + alertId + " forceEmail";
			try 
			{
				CommandLine.StartBackgroundSession(command);
				msg =
					"The Alert has been started in the background.\n" +
					"You will receive an email when it completes.";
			}
			catch (Exception ex)
			{
				msg = "Failed to start background alert: " + ex.Message;
			}

			MessageBoxMx.Show(msg);

			return;
		}

		private void DeleteAlert_Click(object sender, EventArgs e)
		{
			int row, alertId;

			row = V.GetFocusedDataSourceRowIndex();
			if (row < 0) return;
			alertId = (int)DataTable.Rows[row]["AlertId"];
			UserObjectDao.Delete(alertId);
			DataTable.Rows.RemoveAt(row);
			Grid.Refresh();
			return;
		}

		private void OpenQuery_Click(object sender, EventArgs e)
		{
			int row, queryId;

			row = V.GetFocusedDataSourceRowIndex();
			if (row < 0) return;
			queryId = (int)DataTable.Rows[row]["QueryId"];
			UserObject quo = UserObjectDao.ReadHeader(queryId);
			string path = quo.InternalName;
			QbUtil.OpenQuery(path, true, false, false);
			DialogResult = DialogResult.OK; // close dialog
		}

		private void Help_Click(object sender, EventArgs e)
		{
			QbUtil.ShowConfigParameterDocument("AlertHelpUrl", "Alert Help");

		}

	}
}