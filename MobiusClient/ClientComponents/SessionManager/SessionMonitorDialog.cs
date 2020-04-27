using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;
using Mobius.NativeSessionClient;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace Mobius.ClientComponents
{
	public partial class SessionMonitorDialog : DevExpress.XtraEditors.XtraForm
	{
		DataTable DataTable;
		bool InUpdateGrid = false;

		public SessionMonitorDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Show current sessions
		/// </summary>

		public static new void ShowDialog()
		{
			SessionMonitorDialog smg = new SessionMonitorDialog();
			smg.Show2();
		}

		void Show2()
		{
			DataTable dt = DataTable = new DataTable();
			dt.Columns.Add("SessionIdCol", typeof(int));
			dt.Columns.Add("IsNonNativeCol", typeof(string));
			dt.Columns.Add("UserIdCol", typeof(string));
			dt.Columns.Add("UserNameCol", typeof(string));
			dt.Columns.Add("CreationDtCol", typeof(string));
			dt.Columns.Add("IdleTimeCol", typeof(string));

			dt.Columns.Add("ProcessIdCol", typeof(int));
			dt.Columns.Add("CpuTimeCol", typeof(int));
			dt.Columns.Add("MemoryCol", typeof(int));
			dt.Columns.Add("ThreadsCol", typeof(int));
			dt.Columns.Add("HandlesCol", typeof(int));

			UpdateGrid();

			Timer.Enabled = true;
			DialogResult r = ShowDialog(SessionManager.ActiveForm);
			Timer.Enabled = false;
			return;
		}

/// <summary>
/// Update the grid on a timer tick
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Timer_Tick(object sender, EventArgs e)
		{
			UpdateGrid();
		}

/// <summary>
/// Get new data & update grid
/// </summary>

		void UpdateGrid()
		{
			InUpdateGrid = true;

			ServiceHostInfo svcInf = ServiceFacade.ServiceFacade.GetServiceHostInfo();
			if (svcInf == null) return;

			List<Mobius.NativeSessionClient.SessionInfo> sil = new Mobius.NativeSessionClient.NativeSessionClient().GetSessionInfoForAllSessions();
			if (sil == null) sil = new List<SessionInfo>();

			Label.Text =
				"Server: " + svcInf.ServerName +
				", Version: " + VersionMx.FormatVersion(svcInf.Version) +
				", Count: " + sil.Count;

			DataTable.Rows.Clear();
			DateTime now = DateTime.Now;

			foreach (SessionInfo si in sil)
			{
				DataRow dr = DataTable.NewRow();
				dr["SessionIdCol"] = si.Id;
				dr["IsNonNativeCol"] = si.Native ? "" : "Y";
				dr["UserIdCol"] = si.UserId;
				try {	dr["UserNameCol"] = SecurityUtil.GetShortPersonNameReversed(si.UserId); }
				catch { }

				dr["CreationDtCol"] = CommandLine.FormatTimeSpan(now.Subtract(si.CreationDT)); 

				if (!si.ExpirationDT.Equals(DateTime.MinValue)) // calc idle time
					dr["IdleTimeCol"] = CommandLine.FormatTimeSpan(now.Subtract(si.ExpirationDT));

				if (si.ProcessId > 0)
					dr["ProcessIdCol"] = si.ProcessId;

				if (si.CpuTimeSecs > 0)
					dr["CpuTimeCol"] = (int)si.CpuTimeSecs;

				if (si.MemoryMb > 0)
					dr["MemoryCol"] = si.MemoryMb;

				if (si.Threads > 0)
					dr["ThreadsCol"] = si.Threads;

				if (si.Handles > 0)
					dr["HandlesCol"] = si.Handles;

				DataTable.Rows.Add(dr);
			}

			Grid.DataSource = DataTable;
			Grid.Refresh();
			Application.DoEvents();

			InUpdateGrid = false;
			return;
		}


	}
}