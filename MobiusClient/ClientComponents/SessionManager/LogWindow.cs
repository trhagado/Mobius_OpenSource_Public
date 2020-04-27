using Mobius.ComOps;

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
	public partial class LogWindow : DevExpress.XtraEditors.XtraForm
	{
		public static DateTime T0 = DateTime.MinValue;
		public static DateTime TLast = DateTime.MinValue;
		public static LogWindow Instance;
		public static bool Display = false;

		DataTable DataTable = null; // DataTable of grid information
		List<MessageItem> MessageQueue = new List<MessageItem>();

		public LogWindow()
		{
			InitializeComponent();

			DataTable dt = new DataTable();
			DataTable = dt; // save ref to table
			dt.Columns.Add("TimeField", typeof(string));
			dt.Columns.Add("T0PlusField", typeof(string));
			dt.Columns.Add("TDeltaField", typeof(string));
			dt.Columns.Add("MessageField", typeof(string));
			Grid.DataSource = dt;

			Instance = this;
		}

/// <summary>
/// Add a message to the log
/// </summary>
/// <param name="msg"></param>

		public static void Message(string msg)
		{
			try
			{
				if (!Display) return;

				if (Instance == null)
				{
					Instance = new LogWindow();
					Instance.Show();
				}

				lock (Instance.MessageQueue)
				{ // just queue in case on other thread
					MessageItem mi = new MessageItem();
					mi.Message = msg;
					mi.Time = DateTime.Now;
					Instance.MessageQueue.Add(mi);
				}
				Application.DoEvents();
			}

			catch (Exception ex) { ex = ex; }
			return;
		}

/// <summary>
/// Display any messages in the queue
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Timer_Tick(object sender, EventArgs e)
		{
			if (MessageQueue == null || MessageQueue.Count == 0) return;

			lock (MessageQueue)
			{
				while (MessageQueue.Count > 0)
				{
					MessageItem mi = MessageQueue[0];

					DataRow dr = DataTable.NewRow();
					DateTime dt = mi.Time;
					if (T0 == DateTime.MinValue) T0 = TLast = dt;
					dr[0] = dt.Hour + ":" + dt.Minute + ":" + dt.Second + "." + dt.Millisecond; // current time

					TimeSpan ts = dt.Subtract(T0);
					dr[1] = ((int)ts.TotalSeconds).ToString() + "." + ts.Milliseconds; // Time since first call

					ts = dt.Subtract(TLast);
					dr[2] = ((int)ts.TotalSeconds).ToString() + "." + ts.Milliseconds; // Time since last call
					TLast = dt;

					dr[3] = mi.Message;
					DataTable.Rows.Add(dr);

					MessageQueue.RemoveAt(0);
				}
			}
			Grid.Refresh();
		}

		private void LogWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = true;
			Hide();
		}

		private void CopyAll_Click(object sender, EventArgs e)
		{
			GridView v = gridView1;
			v.SelectAll();
			v.CopyToClipboard();
		}

		private void ClearGrid_Click(object sender, EventArgs e)
		{
			DataTable.Clear();
		}

		private void CloseForm_Click(object sender, EventArgs e)
		{
			Hide();
		}

		class MessageItem
		{
			internal string Message;
			internal DateTime Time;
		}

	}
}