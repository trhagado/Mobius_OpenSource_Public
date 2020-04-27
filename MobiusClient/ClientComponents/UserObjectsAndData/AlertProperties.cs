using Mobius.Data;
using Mobius.ComOps;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class AlertProperties : DevExpress.XtraEditors.XtraForm
	{
		Alert Alert; // the alert itself
		UserObject QueryUo = null; // user object for query
		ResultsFormat ExportParms; // Export parameters if exporting full data set as part of alert
		private AlertPropertiesDaily _alertPropertiesDailyCtl;
		private AlertPropertiesWeekly _alertPropertiesWeeklyCtl;
		private AlertPropertiesMonthly _alertPropertiesMonthlyCtl;

		int ExpandedHeight = 0; // height in advanced mode
		int ContractedHeight = 0; // height in non-advanced mode
		bool WindowExpanded = true; // if true form is expanded to advanced form
		bool Constructing = true;

		static AlertProperties Instance;
		public AlertProperties()
		{
			InitializeComponent();
			labelTimeZone.Text = "(" + TimeZone.CurrentTimeZone.StandardName + ")";

			_alertPropertiesDailyCtl = AlertPropertiesDaily.Instance;
			_alertPropertiesWeeklyCtl = AlertPropertiesWeekly.Instance;
			_alertPropertiesMonthlyCtl = AlertPropertiesMonthly.Instance;

			Rectangle screenRect = Screen.PrimaryScreen.Bounds;
			Location = new // Center the Location of the form before contracting
				Point(screenRect.Width / 2 - Width / 2,
				screenRect.Height / 2 - Height / 2);

			ExpandedHeight = Height;
			ContractedHeight = AddressNote.Top + 125;
			ContractWindow(); // contract to basic form initially

			ExportFileFormat.SelectedIndex = ExportFileFormat.Properties.Items.Count - 1; // select last item (None) by default

			Constructing = false;
		}

		/// <summary>
		/// Edit an alert
		/// </summary>
		/// <param name="objectId">AlertId or QueryId to edit alert for</param>
		/// <param name="alert">New alert content</param>
		/// <returns>-1 if alert deleted, 0 if edit cancelled, alertId if successfully edited</returns>

		public static int Edit(
			int objectId,
			out Alert alert)
		{
			alert = null;
			int alertId = 0;
			UserObject auo = null; // alert user object
			UserObject quo = null;

			if (objectId > 0) // read existing alert/query header
			{
				UserObject uo = UserObjectDao.ReadHeader(objectId);
				if (uo == null) throw new Exception("UserObject not found " + objectId.ToString());
				if (uo.Type == UserObjectType.Alert) // editing existing alert
				{
					auo = uo;
					alertId = objectId;
					alert = Alert.GetAlertFromUserObject(auo, false);
					quo = UserObjectDao.ReadHeader(alert.QueryObjId);
					alert.QueryName = quo.Name;
					alert.LastQueryUpdate = quo.UpdateDateTime;
				}

				else if (uo.Type == UserObjectType.Query) // edit any alert associated with existing query
				{
					quo = uo;
					alert = Alert.GetAlertByQueryId(uo.Id);
					if (alert != null)
					{
						alertId = alert.Id;
						alert.QueryName = quo.Name;
						alert.LastQueryUpdate = quo.UpdateDateTime;
					}
				}

				else throw new Exception("UserObject not alert or query " + objectId.ToString());
			}

			if (alertId == 0) // create basic new alert object
			{
				alert = new Alert(quo);
			}

			if (Instance == null) Instance = new AlertProperties();
			Instance.Setup(alert, quo);

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Abort) // alert deleted
			{
				alert = null;
				return -1;
			}
			else if (dr == DialogResult.Cancel) return 0; // edit cancelled
			else return Instance.Alert.Id; // successfully created/edited, return the userobject id
		}

		/// <summary>
		/// Setup the form
		/// </summary>
		/// <param name="alert"></param>

		void Setup(
			Alert alert,
			UserObject quo)
		{
			Alert = alert;
			QueryUo = quo;
			QueryName.Text = alert.QueryName;

			if (alert.Id == 0) // if new alert default to Daily Control
			{
				radioButtonDaily.Checked = true;
			}
			dateTimePicker1.Text = alert.StartTime.ToShortTimeString();

			SetControlData(alert);

			var txt = alert.MailTo;
			if (txt == "") txt = SS.I.UserInfo.EmailAddress;
			Recipients.Text = txt;

			if (alert.CheckTablesWithCriteriaOnly)
				CheckTablesWithCriteriaOnly.Checked = true;
			else CheckAllTables.Checked = true;

			ExportParms = Alert.ExportParms; // store ref for later access
			string exportFileFormat = "None";
			if (alert.ExportParms != null)
			{
				if (alert.ExportParms.TextFile) exportFileFormat = "CSV / Text File";
				else if (alert.ExportParms.Excel) exportFileFormat = "Excel Worksheet";
				else if (alert.ExportParms.Word) exportFileFormat = "MS Word Table";
				else if (alert.ExportParms.SdFile) exportFileFormat = "SDFile";
				else if (alert.ExportParms.Grid) exportFileFormat = "Mobius - Results displayed when query is opened";
			}
			ExportFileFormat.Text = exportFileFormat;

			HighlightChangedCompounds.Checked = alert.HighlightChangedCompounds;

			if (!alert.CheckTablesWithCriteriaOnly || // show expanded window if expanded option selected
			 alert.ExportParms != null)
				ExpandWindow();

			RunNow.Checked = false;

			if (ServicesIniFile.Read("AlertHelpUrl") != "")
				Help.Enabled = true;
		}

		private void AlertProperties_Activated(object sender, EventArgs e)
		{
			//Interval.Focus();
		}

		private void AlertProperties_Enter(object sender, EventArgs e)
		{
			//Interval.Focus();
		}

		private void ToggleWindowSize_Click(object sender, EventArgs e)
		{
			if (!WindowExpanded) ExpandWindow();
			else ContractWindow();
		}

		public void ExpandWindow()
		{
			Height = ExpandedHeight;
			TableToCheckGroupBox.Visible = true;
			ToggleWindowSize.ImageIndex = 0;
			WindowExpanded = true;

			if (!Visible) UIMisc.CenterFormOnScreen(this);
		}

		public void ContractWindow()
		{
			Height = ContractedHeight;
			TableToCheckGroupBox.Visible = false;
			ToggleWindowSize.ImageIndex = 1;
			WindowExpanded = false;

			if (!Visible) UIMisc.CenterFormOnScreen(this);
		}

		private void OK_Click(object sender, EventArgs e)
		{
			if (panel1.Controls.Contains(_alertPropertiesDailyCtl))
			{
				if (!_alertPropertiesDailyCtl.ValidateData()) return;
			}

			if (panel1.Controls.Contains(_alertPropertiesWeeklyCtl))
			{
				if (!_alertPropertiesWeeklyCtl.ValidateData()) return;
				Alert.LastCheck = DateTime.MinValue;
			}

			if (panel1.Controls.Contains(_alertPropertiesMonthlyCtl))
			{
				if (!_alertPropertiesMonthlyCtl.ValidateData()) return;
				Alert.LastCheck = DateTime.MinValue;
			}

			if (QueryName.Text.Trim() == "")
			{
				MessageBoxMx.Show("A query name must be supplied.");
				//Interval.Focus();
				return;
			}

			if (Alert.Id > 0) // get previous results from existing alert
			{
				UserObject auo = UserObjectDao.Read(Alert.Id); // read full alert including any results
				Alert.LastResults = Alert.DeserializeResults(auo.Content);
				if (Alert.QueryObjId != QueryUo.Id) // if query changed reset lastCheck date
					Alert.LastCheck = DateTime.MinValue;
			}

			Alert.QueryObjId = QueryUo.Id; // reference query
			Alert.QueryName = QueryUo.Name; // pass back query name (not persisted)
			Alert.LastQueryUpdate = QueryUo.UpdateDateTime;

			//try
			//{
			//if (!int.TryParse(Interval.Text.Trim(), out days))
			//	throw new Exception("Invalid number of days, try again.");

			//	if (days <= 0) throw new Exception("Days value too small");
			//	Alert.Interval = days;
			//}
			//catch (Exception ex)
			//{
			//	MessageBoxMx.Show(ex.Message);
			//	Interval.Focus();
			//	return;
			//}

			if (radioButtonDaily.Checked)
			{
				Alert.Pattern = Alert.PatternEnum.Daily.ToString();
				Dictionary<string, string> dataValues = _alertPropertiesDailyCtl.GetData();
				string interval = dataValues["Interval"];
				string intervalDays = dataValues["Days"];
				if (string.IsNullOrEmpty(intervalDays))
				{
					Alert.Interval = int.Parse(interval);
					Alert.Days = "";
				}
				else
				{
					//Alert.Interval = 1;
					Alert.Days = intervalDays;
				}
			}

			if (radioButtonWeekly.Checked)
			{
				Alert.Pattern = Alert.PatternEnum.Weekly.ToString();
				Dictionary<string, string> dataValues = _alertPropertiesWeeklyCtl.GetData();
				string interval = dataValues["Interval"];
				string intervalDays = dataValues["Days"];
				Alert.Interval = int.Parse(interval);
				Alert.Days = string.IsNullOrEmpty(intervalDays) ? "" : intervalDays;
			}

			if (radioButtonMonthly.Checked)
			{
				Alert.Pattern = Alert.PatternEnum.Monthly.ToString();
				Dictionary<string, string> dataValues = _alertPropertiesMonthlyCtl.GetData();
				string interval = dataValues["Interval"];
				string dayInterval = dataValues["DayInterval"];
				string days = dataValues["Days"];

				if (interval != null) Alert.Interval = int.Parse(interval);
				if (dayInterval != null) Alert.DayInterval = int.Parse(dayInterval);
				if (days != null) Alert.Days = days;
			}

			//TimeZoneInfo currentTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZone.CurrentTimeZone.StandardName);
			//TimeZoneInfo easternStandardTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
			//DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(dateTimePicker1.Value, currentTimeZone);
			//DateTime easternStandarDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, easternStandardTimeZone);



			Alert.StartTime = dateTimePicker1.Value;


			Alert.MailTo = Recipients.Text;
			Alert.CheckTablesWithCriteriaOnly = CheckTablesWithCriteriaOnly.Checked;

			if (ExportParms == null) ExportParms = new ResultsFormat();
			Alert.ExportParms = ExportParms;
			var txt = ExportFileFormat.Text;
			if (txt == "CSV / Text File") ExportParms.OutputDestination = OutputDest.TextFile;
			else if (txt == "Excel Worksheet") ExportParms.OutputDestination = OutputDest.Excel;
			else if (txt == "MS Word Table") ExportParms.OutputDestination = OutputDest.Word;
			else if (txt == "SDFile") ExportParms.OutputDestination = OutputDest.SdFile;
			else if (Lex.StartsWith(txt, "Mobius"))
			{
				ExportParms.OutputDestination = OutputDest.WinForms;
				SetBrowseSavedResultsUponOpenForQuery(Alert.QueryObjId, true);
			}
			else Alert.ExportParms = null; // no export

			Alert.HighlightChangedCompounds = HighlightChangedCompounds.Checked;
			if (RunNow.Checked) Alert.RunImmediately = true; // set flag for this-time-only run now
			Alert.Write();

			if (RunNow.Checked) // start the alert now?
			{
				string command = "Check Alert " + Alert.Id;
				CommandLine.StartBackgroundSession(command);
				MessageBoxMx.Show("Checking of the alert has been started in the background");
			}

			DialogResult = DialogResult.OK;
			return;
		}

		/// <summary>
		/// Set flag to try to browse results upon open in both the saved query and any currently open query
		/// </summary>
		/// <param name="qid"></param>

		void SetBrowseSavedResultsUponOpenForQuery(
			int qid,
			bool value)
		{
			if (qid <= 0) return;

			UserObject uo = UserObjectDao.Read(qid); // update any saved version of query
			if (uo == null || uo.Type != UserObjectType.Query) return;
			Query q = Query.Deserialize(uo.Content);
			q.BrowseSavedResultsUponOpen = value;
			uo.Content = q.Serialize();
			UserObjectDao.Write(uo, uo.Id);

			int di = QueriesControl.Instance.GetQueryIndexByUserObjectId(qid); // update any in-memory version of query
			if (di < 0) return;

			q = QueriesControl.Instance.GetQuery(di);
			q.BrowseSavedResultsUponOpen = value;

			return; // todo
		}

		private void DeleteAlert_Click(object sender, EventArgs e)
		{
			if (QueryName.Text != "")
			{
				DialogResult dr = MessageBoxMx.Show("Are you sure you want to delete the alert on query: \"" +
					QueryName.Text + "\"?", UmlautMobius.String, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (dr != DialogResult.Yes) return;
			}

			if (Alert.Id > 0)
			{
				UserObjectDao.Delete(Alert.Id);
				Alert = null;
			}

			DialogResult = DialogResult.Abort; // indicate alert deleted
		}

		private void BrowseQueries_Click(object sender, EventArgs e)
		{
			string prompt = "Select Query";
			UserObject uo = UserObjectOpenDialog.ShowDialog(UserObjectType.Query, prompt, QueryUo);
			if (uo == null) return;
			Alert existingAlert = Alert.GetAlertByQueryId(uo.Id);
			if (existingAlert != null) // only 1 alert per query per user is allowed
			{
				if (Alert.Id <= 0 || // if this is a new alert or
					(Alert.Id > 0 && Alert.Id != existingAlert.Id)) // another alert exists for the query
				{
					MessageBoxMx.Show("You already an existing alert for query " + Lex.Dq(uo.Name),
						"Mobius", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
			}

			Query q = QbUtil.ReadQuery(uo.Id);
			if (!Alert.QueryValidForAlert(q)) return;

			QueryUo = uo;
			QueryName.Text = QueryUo.Name;
		}

		private void QueryName_MouseDown(object sender, MouseEventArgs e)
		{
			BrowseQueries_Click(sender, e);
		}

		private void ExportFileFormat_SelectedIndexChanged(object sender, EventArgs e)
		{
			return;
		}

		private void SetControlData(Alert alert)
		{
			if (alert.Pattern == Alert.PatternEnum.Daily.ToString())
			{
				_alertPropertiesDailyCtl.SetData(alert);
				radioButtonDaily.Checked = true;
				return;
			}
			if (alert.Pattern == Alert.PatternEnum.Weekly.ToString())
			{
				_alertPropertiesWeeklyCtl.SetData(alert);
				radioButtonWeekly.Checked = true;
				return;
			}
			if (alert.Pattern == Alert.PatternEnum.Monthly.ToString())
			{
				_alertPropertiesMonthlyCtl.SetData(alert);
				radioButtonMonthly.Checked = true;

			}
		}

		private void EditExportSetup_Click(object sender, EventArgs e)
		{
			DialogResult dr;

			string exportFormat = ExportFileFormat.Text;

			if (Lex.Eq(exportFormat, "None"))
			{
				MessageBoxMx.Show("You must select a format first", UmlautMobius.String);
				return;
			}

			bool useExistingOptionValues = true;
			if (ExportParms == null)
			{
				ExportParms = new ResultsFormat();
				useExistingOptionValues = false;
			}

			ExportParms.RunInBackground = true; // alerts are always run in the background

			if (Lex.Eq(exportFormat, "CSV / Text File"))
			{
				dr = SetupTextFile.ShowDialog(ExportParms, useExistingOptionValues);
			}

			else if (Lex.Eq(exportFormat, "Excel Worksheet"))
			{
				dr = SetupExcel.ShowDialog(ExportParms, useExistingOptionValues);
			}

			else if (Lex.Eq(exportFormat, "MS Word Table"))
			{
				dr = SetupWord.ShowDialog(ExportParms, useExistingOptionValues);
			}

			else if (Lex.Eq(exportFormat, "SDFile"))
			{
				dr = SetupSdFile.ShowDialog(ExportParms, useExistingOptionValues);
			}

			else if (Lex.StartsWith(exportFormat, "Mobius"))
			{
				MessageBoxMx.Show( // just give a message
					"When a full result set is exported in the Native Mobius Format\n" +
					"the exported data will be automatically displayed when the\n" +
					"associated query is opened. There is no need to supply any\n" +
					"additional export parameters.");
			}

			ExportParms.RunInBackground = true; // reset in case cleared in dialog
			return;
		}

		private void Cancel_Click(object sender, EventArgs e)
		{
			return;
		}

		private void Help_Click(object sender, EventArgs e)
		{
			QbUtil.ShowConfigParameterDocument("AlertHelpUrl", "Alert Definition Help");
		}

		private void radioButtonDaily_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonDaily.Checked)
			{
				AddControl(_alertPropertiesDailyCtl);
				//_currentPattern = Pattern.Daily;
			}
		}

		private void radioButtonWeekly_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonWeekly.Checked)
			{
				AddControl(_alertPropertiesWeeklyCtl);
				//_currentPattern = Pattern.Weekly;
			}
		}

		private void radioButtonMonthly_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonMonthly.Checked)
			{
				AddControl(_alertPropertiesMonthlyCtl);
				//_currentPattern = Pattern.Monthly;
			}
		}
		private void AddControl(UserControl userControl)
		{
			if (!panel1.Controls.Contains(userControl))
			{
				panel1.Controls.Clear();
				panel1.Controls.Add(userControl);
				userControl.Dock = DockStyle.Fill;
				userControl.BringToFront();
			}
		}

	}
}