using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraTab;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;

using System;
using System.Data;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.IO;

namespace Mobius.ClientComponents
{
	public partial class SpotfireLinkUI : DevExpress.XtraEditors.XtraForm
	{
		static SpotfireLinkUI Instance;

		SpotfireViewProps SpotfireViewProps; // content for Spotfire link associated with dialog
		UserObject UoIn; // user object being edited

		bool InSetup = false;

		static MainMenuControl MainMenuControl { get { return SessionManager.Instance.MainMenuControl; } }
		static string SpotfireLinkModel = "SPOTFIRELINK_MODEL";

		public SpotfireLinkUI()
		{
			InitializeComponent();
		}

/// <summary>
/// Create a new link
/// </summary>
/// <returns></returns>

		public static SpotfireViewProps CreateNew()
		{
			SpotfireViewProps sl = new SpotfireViewProps(null);
			UserObject uo = Edit(sl, null);
			if (uo == null) return null;

			return sl;
		}

/// <summary>
/// Edit existing link and edit 
/// </summary>
/// <returns></returns>

		public static UserObject Edit()
		{
			UserObject uo = null;
			return Edit(uo);
		}

/// <summary>
/// Edit and existing Spotfire link
/// </summary>
/// <param name="internalName"></param>
/// <returns></returns>

		public static UserObject Edit(string internalName)
		{
			UserObject uo = null;

			if (Lex.IsDefined(internalName)) // get the user object
			{
				uo = UserObject.ParseInternalUserObjectName(internalName, "");
				if (uo != null) uo = UserObjectDao.Read(uo.Id);
				if (uo == null) throw new Exception("User object not found " + internalName);
			}

			else uo = null; // no arg, show open dialog

			UserObject uo2 = Edit(uo);

			if (uo2 != null && uo2.Id > 0)
			{
				QbUtil.UpdateMetaTableCollection(uo2.InternalName);
			}

			return uo2;
		}

/// <summary>
/// Edit a new or existing link
/// </summary>
/// <param name="uo">Existing UserObject with content or null to create a new link</param>
/// <returns></returns>

		public static UserObject Edit(UserObject uo)
		{
			if (uo == null) // prompt if not supplied
				uo = UserObjectOpenDialog.ShowDialog(UserObjectType.SpotfireLink, "Edit Spotfire Link");
			if (uo == null) return null;

			uo = UserObjectDao.Read(uo);
			if (uo == null) throw new Exception("User object not found: " + uo.Id);

			SpotfireViewProps sl = SpotfireViewProps.Deserialize(uo.Content);
			if (sl == null) return null;

			uo = Edit(sl, uo);
			return uo;
		}

		/// <summary>
		/// Edit a new or existing Spotfire link
		/// </summary>
		/// <param name="cf"></param>
		/// <param name="uoIn"></param>
		/// <returns></returns>

		public static UserObject Edit(
			SpotfireViewProps sl,
			UserObject uoIn)
		{
			SpotfireLinkUI lastInstance = Instance;

			Instance = new SpotfireLinkUI();
			SpotfireLinkUI i = Instance;

			//if (UalUtil.IniFile.Read("SpotfireLinkHelpUrl") != "")
			//  Instance.Help.Enabled = true;

			if (lastInstance != null)
			{
				Instance.StartPosition = FormStartPosition.Manual;
				Instance.Location = lastInstance.Location;
				Instance.Size = lastInstance.Size;
				lastInstance = null;
			}

			if (uoIn == null) uoIn = new UserObject(UserObjectType.SpotfireLink);

			else if (uoIn.Id > 0) // get existing metatable for calc field for exclusion in BuildQuickSelectTableMenu
			{
				if (MainMenuControl != null) MainMenuControl.UpdateMruList(uoIn.InternalName);
			}

			Instance.UoIn = uoIn;

			string title = "Edit Spotfire Link";

			if (!String.IsNullOrEmpty(uoIn.Name))
			{
				title += " - " + uoIn.Name;

				if (uoIn.Id > 0) // include internal name?
					title += " - (SPOTFIRELINK_" + uoIn.Id + ")";
			}

			Instance.Text = title;

			Instance.SpotfireViewProps = sl; // what we're editing
			Instance.SetupForm(); // set up the form 

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return null;

			return Instance.UoIn; // return saved object
		}

/// <summary>
/// SetupForm
/// </summary>

		void SetupForm()
		{
			string DefaultLibraryPath = "/Mobius/Visualizations/";
			string DefaultCorpIdParm = "CorpId_LIST";

			InSetup = true;

			SpotfireViewProps svp = SpotfireViewProps;
			string libPath = svp.AnalysisPath;
			if (Lex.IsUndefined(libPath)) libPath = DefaultLibraryPath;
			LibraryPath.Text = libPath;

			MetaTable mt = MetaTableCollection.Get(SpotfireLinkModel);
			if (mt == null) throw new Exception(SpotfireLinkModel + " table not found");
			mt = mt.Clone();

			CriteriaCols.QueryTable = new QueryTable(mt);
			SetupParameters();

			Description.Text = svp.Description;

			InSetup = false;
			return;
		}

/// <summary>
/// Setup Parameters
/// </summary>

		void SetupParameters()
		{
			QueryColumn qc;

			SpotfireViewProps sl = SpotfireViewProps;
			Dictionary<string, SpotfireLinkParameter> p = sl.SpotfireLinkParameters;

			if (p.Count == 0) // if no parameters then include one for CIDLIST checked
			{
				SpotfireLinkParameter p1 = new SpotfireLinkParameter("COMPOUND_ID", "True");
				p[p1.Name] = p1;
			}

			QueryTable qt = CriteriaCols.QueryTable;
			qt.DeselectAll();

			foreach (SpotfireLinkParameter p0 in sl.SpotfireLinkParameters.Values)
			{
				qc = qt.GetQueryColumnByName(p0.Name);
				if (qc == null) continue;

				bool selected = false;
				bool.TryParse(p0.Value, out selected);
				qc.Selected = selected;
			}

			qc = qt.GetQueryColumnByName("visualization"); // hide visualization col here
			if (qc != null)
			{
				qc.MetaColumn.InitialSelection = ColumnSelectionEnum.Hidden; // hide visualization col here
				qc.Selected = false;
			}

			CriteriaCols.TableHeaderLabel.Visible = false;
			CriteriaCols.CanDeselectKeyCol = true;
			CriteriaCols.Render();
			return;
		}

		private void SaveAndClose_Click(object sender, EventArgs e)
		{
			bool prompt = true;
			if (UoIn.Id > 0) prompt = false; // normal save & have existing object?
			if (Save(prompt)) DialogResult = DialogResult.OK;
		}

		private void SaveAndClose_ShowDropDownControl(object sender, ShowDropDownControlEventArgs e)
		{
			SaveAndCloseContextMenu.Show(SaveAndClose,
				new System.Drawing.Point(0, SaveAndClose.Size.Height));
		}

		private void SaveMenuItem_Click(object sender, EventArgs e)
		{
			bool prompt = true;
			if (UoIn.Id > 0) prompt = false; // normal save & have existing object?
			Save(prompt);
		}

		private void SaveAsMenuItem_Click(object sender, EventArgs e)
		{
			Save(true);
		}

/// <summary>
/// Save the SpotfireLink UserObject
/// </summary>
/// <param name="prompt"></param>
/// <returns></returns>

		bool Save(bool prompt)
		{
			UserObject uo2;

			if (!IsValidSpotfireLink()) return false;
			if (!GetSpotfireLinkForm()) return false;
			if (prompt)
			{
				uo2 = UserObjectSaveDialog.Show("Save Spotfire link Definition", UoIn);
				if (uo2 == null) return false;
			}

			else uo2 = UoIn.Clone();

			uo2.Content = SpotfireViewProps.Serialize();

			if (!UserObjectUtil.UserHasWriteAccess(uo2))
			{ // is the user authorized to save this?
				MessageBoxMx.ShowError("You are not authorized to save this Spotfire link");
				return false;
			}

			SessionManager.DisplayStatusMessage("Saving Spotfire link...");

			//need the name of the folder to which the object will be saved
			MetaTreeNode targetFolder = UserObjectTree.GetValidUserObjectTypeFolder(uo2);
			if (targetFolder == null)
			{
				MessageBoxMx.ShowError("Failed to save your Spotfire link.");
				return false;
			}

			UserObjectDao.Write(uo2);

			MainMenuControl.UpdateMruList(uo2.InternalName);

			string title = "Edit Spotfire Link - " + uo2.Name;
			Text = title;
			UoIn = uo2.Clone(); // now becomes input object
			SessionManager.DisplayStatusMessage("");
			return true;
		}

		/// <summary>
		/// See if valid Spotfire link
		/// </summary>
		/// <returns></returns>

		private bool IsValidSpotfireLink()
		{
			if (Lex.IsUndefined(LibraryPath.Text))
			{
				LibraryPath.Focus();
				MessageBoxMx.Show("You must define the Library Path for the linked analysis");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Get values from form checking for errors
		/// </summary>
		/// <returns></returns>


		bool GetSpotfireLinkForm()
		{
			SpotfireViewProps l = SpotfireViewProps;

			l.AnalysisPath = LibraryPath.Text;
			l.Description = Description.Text;

			GetParameters();

			return true;
		}

/// <summary>
/// Setup Parameters
/// </summary>

		void GetParameters()
		{
			SpotfireViewProps sl = SpotfireViewProps;
			sl.SpotfireLinkParameters = new Dictionary<string, SpotfireLinkParameter>();

			QueryTable qt = CriteriaCols.QueryTable;

			foreach (QueryColumn qc in qt.QueryColumns)
			{
				if (!qc.Selected) continue;

				MetaColumn mc = qc.MetaColumn;

				SpotfireLinkParameter p = new SpotfireLinkParameter();
				p.Name = mc.Name;
				p.Value = "True";
				SpotfireViewProps.SpotfireLinkParameters[p.Name] = p;
			}

			return;
		}

/// <summary>
/// Close dialog
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void CloseButton_Click(object sender, EventArgs e)
		{
			return;
		}

		/// <summary>
		/// Open a spotfire link or webplayer url 
		/// </summary>
		/// <param name="link"></param>

		public static void OpenLink(
			string link)
		{
			OpenLink(link, "", null);
			return;
		}

/// <summary>
/// Open a spotfire link or webplayer url 
/// </summary>
/// <param name="link"></param>

		public static void OpenLink(
			string link,
			string title)
		{
			OpenLink(link, title, null);
			return;
		}

		/// <summary>
		/// Open a spotfire link or webplayer url 
		/// Example: https://[server]/SpotfireWeb/ViewAnalysis.aspx?file=/Mobius/Visualizations/MdbAssay_MOBIUS&configurationBlock=CorpId_LIST={1,3,5};
		/// Example: https://[server]/SpotfireWeb/ViewAnalysis.aspx?file=/Mobius/Visualizations/SELECT_MOBIUS_DATA_Test&configurationBlock=SQLPARMS="1,3,5,7,9";
		/// </summary>
		/// <param name="link">Open a spotfire link or Webplayer URL </param>
		/// <param name="browserControl"></param>

		public static void OpenLink(
			string link,
			string title,
			WebBrowser browserControl)
		{
			SpotfireViewProps sl = null;
			UserObject uo = null;
			UserObjectType uoType;
			string url;
			PopupHtml form;
			int uoId;

			if (Lex.IsUndefined(title)) title = link;

			if (Lex.IsUndefined(link)) // prompt for the link to open
			{
				string prompt = "Open Spotfire Link ";
				uo = UserObjectOpenDialog.ShowDialog(UserObjectType.SpotfireLink, prompt);
				if (uo == null) return;
				link = uo.InternalName;
			}

			if (Lex.StartsWith(link, "SPOTFIRELINK_")) // get the link user object
			{
				string internalName = link;
				bool parseOk = UserObject.ParseObjectTypeAndIdFromInternalName(link, out uoType, out uoId);
				uo = UserObjectDao.Read(uoId);
				if (uo == null) throw new Exception("User object not found " + internalName);
				sl = SpotfireViewProps.Deserialize(uo.Content);
				title = uo.Name;
				url = BuildUrl(sl);
				if (Lex.IsUndefined(url)) return; // cancelled
			}

			else url = link; // assume link is a complete url

			if (browserControl == null) // Open the link within a WebBrowser control
			{
				form = new PopupHtml();
				UIMisc.PositionPopupForm(form);
				form.Text = title;
				form.Show();
				browserControl = form.WebBrowser;
			}

			browserControl.Navigate(url);

			return;
		}

/// <summary>
/// Test the link
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Test_Click(object sender, EventArgs e)
		{

			if (!IsValidSpotfireLink()) return;
			if (!GetSpotfireLinkForm()) return;

			string url = BuildUrl(SpotfireViewProps);
			if (Lex.IsUndefined(url)) return; // cancelled

			OpenLink(url);
			return;
		}

		/// <summary>
		/// Build Url for analysis including prompting for any parameters
		/// Example: https://[server></server>/SpotfireWeb/ViewAnalysis.aspx?file=/Mobius/Visualizations/MdbAssay_MOBIUS&configurationBlock=CorpId_LIST={1,3,5};
		/// </summary>
		/// <param name="sl"></param>
		/// <returns></returns>

		static string BuildUrl(SpotfireViewProps sl)
		{
			string url = sl.GetWebplayerUrlOfAnalysis();

			string parms = ""; // parm names & values for config block - 

			//foreach (SpotfireParameter p0 in sl.Parameters.Values)
			//{

			SpotfireLinkParameter p0 = new SpotfireLinkParameter();
			p0.Name = "Compound Id";

				string vals = InputBoxMx.Show(
					"Enter desired values for " + p0.Name + ":\r\n" +
					"Example: 1, 2, 3",
					p0.Name + " Criteria");
				if (vals == null) return null; // cancelled
				if (parms != "") parms += " ";
				parms += "SQLPARMS=\"" + vals + "\"";

				//parms += p0.Name + "={" + vals + "}";
				//parms += p0.Name + "=\"" + vals + "\"";
				//}

			if (Lex.IsDefined(parms))
				url += "&configurationBlock=" + parms + ";"; // add the parms in the config block


			return url;
		}

		private void LibraryPath_Enter(object sender, EventArgs e)
		{
			LibraryPath.Select(LibraryPath.Text.Length, 0); // move cursor to end of text
		}

		/// <summary>
		/// Build a Spotfire view for the QueryTable in the current query
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public static string StoreSpotfireQueryTableSql()
		{
			string sqlStmtName = QbUtil.Query.UserObject.Name;
			if (Lex.StartsWith(sqlStmtName, "SF_")) // trim the sf prefix
				sqlStmtName = sqlStmtName.Substring(3);

			while (true)
			{
				sqlStmtName = InputBoxMx.Show("Name to assign to stored SQL:", "Store Mobius QueryTable SQL for Use by Spotfire", sqlStmtName);
				if (Lex.IsUndefined(sqlStmtName)) return "";

				string existingSql = QueryEngine.ReadSpotfireSql(sqlStmtName, 0);
				if (Lex.IsDefined(existingSql))
				{
					DialogResult dr = MessageBoxMx.Show(
						"Sql statement " + sqlStmtName + " already exists.\r\n" +
						"Do you want to overwrite it?",
						"Confirm Overwrite", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

					if (dr == DialogResult.No) continue; // allow name reentry
					else if (dr == DialogResult.Cancel) return "";
				}

				break; 
			}

			Progress.Show("Saving SQL...");
			int id = QueryEngine.SaveSpotfireSql(sqlStmtName, QbUtil.Query);
			Progress.Hide();

			string msg = "Sql statement " + sqlStmtName + " has been stored.";
			return msg;
		}

		/// <summary>
		/// Build a Spotfire view for the QueryTable in the current query
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public static string BuildSpotfireQueryTableView(string viewName)
		{
			string initialViewName = viewName;

			if (Lex.IsUndefined(initialViewName))
				viewName = QbUtil.Query.UserObject.Name;

			do
			{
				if (Lex.IsUndefined(initialViewName) || viewName.Length > 30)
				{
					viewName = InputBoxMx.Show("Name to assign to stored SQL:", "Build QueryTable SQL for Use by Spotfire", viewName);
					if (Lex.IsUndefined(viewName)) return "";
				}
			}
			while (viewName.Length > 30);

			string serializedQuery = QbUtil.Query.Serialize();

			string command = "BuildQueryTableOracleView " + viewName + " " + serializedQuery;

			Progress.Show("Executing Command...");
			string result = CommandLine.ExecuteServiceCommand(command);
			Progress.Hide();
			return result;

			//QueryTable qt = QbUtil.Query.CurrentTable;
			//if (qt == null) return "No current QueryTable";

			//Query q = new Query();
			//qt = qt.Clone();
			//q.AddQueryTable(qt);

			//string initialViewName = viewName;
			//if (Lex.IsUndefined(initialViewName))
			//  viewName = "SF_" + qt.MetaTable.Name; // default view name has SF prefix
		}

/// <summary>
/// SQL for Oracle function to select Mobius data
/// </summary>

		void SELECT_MOBIUS_DATA_Function()
		{

string sql = @"


";		
		}

	}
}