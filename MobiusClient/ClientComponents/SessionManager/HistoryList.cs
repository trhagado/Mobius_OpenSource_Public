using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Mobius.ClientComponents 
{
	public partial class HistoryList : DevExpress.XtraEditors.XtraForm
	{
		static HistoryList Instance;
		DataTable Dt;

		public HistoryList()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Show the history list
		/// </summary>
		/// <param name="p">Locatio on screen to display</param>

		public static void Show(Point p)
		{
			DataTable dt;
			if (Instance == null)
			{
				Instance = new HistoryList();

				dt = Instance.Dt = new DataTable();
				DataColumn dc = new DataColumn("QueryCol", typeof(string));
				dt.Columns.Add(dc);
				dc = new DataColumn("HitCountCol", typeof(string));
				dt.Columns.Add(dc);
				dc = new DataColumn("TimeCol", typeof(string));
				dt.Columns.Add(dc);
				Instance.Grid.DataSource = dt;
			}
			else dt = Instance.Dt;
			dt.Rows.Clear();

			StringBuilder sb = new StringBuilder();
			for (int i1 = 0; i1 < SS.I.History.Count; i1++)
			{
				HistoryItem hi = SS.I.History[i1];
				DataRow dr = dt.NewRow();
				dr["QueryCol"] = hi.QueryName;
				dr["HitCountCol"] = hi.ListCount.ToString();
				dr["TimeCol"] = hi.DateTime.ToLongTimeString();
				dt.Rows.Add(dr);
			}

			Instance.Location = p;
			Instance.Show(SessionManager.ActiveForm);
			//Instance.Location = p;
			return;
		}

		private void HistoryList_Deactivate(object sender, EventArgs e)
		{ // hide form when deactivated
			this.Hide();
			SessionManager.ActivateShell();
		}

		private void GridView_CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
		{
			if (!e.Info.IsRowIndicator) return;
			if (e.RowHandle < 0) return;

			e.Info.DisplayText = (e.RowHandle + 1).ToString();
			e.Info.ImageIndex = -1; // remove any image that would overlay the row number
		}

/// <summary>
/// Open the specified query or copy the list
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void GridView_RowCellClick(object sender, RowCellClickEventArgs e)
		{
			StreamReader sr;

			int r = e.RowHandle;
			int c = e.Column.AbsoluteIndex;
			HistoryItem hi = SS.I.History[r]; // get item allowing for header row

			if (c == 0) // open query
			{
				sr = new StreamReader(hi.QueryFileName);
				string queryText = sr.ReadToEnd();
				Query q = Query.Deserialize(queryText);
				q.UserObject.Name = // append time to name
				 q.UserObject.Name + " - " + hi.DateTime.ToLongTimeString();
				q.Mode = QueryMode.Build; // want to be in build mode
				q.UserObject.Content = q.Serialize(); // set content so not prompted for save when closed

				QbUtil.AddQueryAndRender(q, false);
			}

			// Copy list to current list

			sr = new StreamReader(hi.ListFileName);
			string listText = sr.ReadToEnd();
			CidList cidList = new CidList(listText);
			CidListCommand.WriteCurrentList(cidList);
			SessionManager.CurrentResultKeys = cidList.ToStringList();
			SessionManager.DisplayCurrentCount();

			if (c == 1) // return message if just copying list to current
			{
				string msg = "The list has been copied to the current list (" + SessionManager.CurrentResultKeysCount + ")";
				MessageBoxMx.Show(msg);
			}

			HistoryList_Deactivate(null, null);
			return;
		}

	}
}