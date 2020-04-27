using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraEditors;

namespace Mobius.ClientComponents
{

/// <summary>
/// Form for displaying actively-updated query engine stats
/// </summary>

	public partial class QueryEngineStatsForm : DevExpress.XtraEditors.XtraForm
	{
		static Query Query; // current Query
		static QueryEngine Qe; // current QueryEngine if any

		static DataTable DataTable = null; // DataTable of QE stats used to fill stats grid

		static int SearchKeyCount = 0; 
		static Stopwatch SearchTimer = new Stopwatch();

		static int GridRowCount = 0;
		static int MetatableTotalRows = 0;
		static int OracleTotalRows = 0;
		static Stopwatch RetrievalTimer = new Stopwatch();

		static QueryEngineStatsForm Instance;

		public static bool ShowStats { get { return IsVisible; } }
		public static bool IsVisible { get { return Instance != null && !Instance.IsDisposed && Instance.Visible == true; } }
		public static bool ProcessResets = true;

		/// <summary>
		/// Basic constructor
		/// </summary>

		QueryEngineStatsForm()
		{
			InitializeComponent();
		}

/// <summary>
/// Show the form for the specified query
/// </summary>
/// <param name="qe"></param>

		public static void StartNewQueryExecution(Query q)
		{
			QueryEngineStatsForm i = Instance;

			if (i == null || i.IsDisposed)
			{
				i = Instance = new QueryEngineStatsForm();
				DataTable = i.CreateDataTable();
				i.Grid.DataSource = DataTable;

				Rectangle sr = Screen.GetWorkingArea(new Point(0,0)); //.PrimaryScreen.Bounds;

				Form cf = SessionManager.Instance.ShellForm;
				Rectangle r = cf.Bounds;
				Size s = cf.Size;

				i.Location = new Point(r.Top, r.Left + s.Width / 2);

				cf.Location = new Point(0, 0);
				cf.Size = new Size(sr.Width / 2, sr.Height);
				cf.WindowState = FormWindowState.Normal;

				i.Show();
				i.WindowState = FormWindowState.Normal;
				i.Location = new Point(sr.Width/2, 0);
				i.Size = new Size(sr.Width / 2, sr.Height);
			}

			//if (q != i.Query)

			i.Initialize2(q);

			return;
		}

/// <summary>
/// Show the basic query information
/// </summary>
/// <param name="q"></param>

		public void Initialize2(Query q)
		{
			DataRow dr;

			Query = q;
			Qe = null; // 

			DataTable.Clear();

			if (ProcessResets)
			{
				SearchTimer.Reset();
				RetrievalTimer.Reset();

				SearchKeyCount = GridRowCount = MetatableTotalRows = OracleTotalRows = 0;
			}

			UpdateTotals();

			if (q == null) return;

			//for (int qti = 0; qti < q.Tables.Count; qti++) // fill the grid
			//{
			//	QueryTable qt = q.Tables[qti];
			//	if (qti < DataTable.Rows.Count)
			//		dr = DataTable.Rows[qti];
			//	else
			//	{
			//		dr = DataTable.NewRow();
			//		DataTable.Rows.Add(dr);
			//	}

			//	SetDataRow(dr, qt, null);
			//}

			if (!Visible) Show();
		}

		public static void StartingSearch()
		{
			if (!IsVisible) return;

			//if (ProcessResets)
			{
				SearchTimer.Restart();
				RetrievalTimer.Reset();
			}
		}

		public static void StartingRetrieval()
		{
			if (!IsVisible) return;

			//if (ProcessResets)
			{
				SearchTimer.Stop();
				RetrievalTimer.Restart();
			}
		}

		public static void ExecutionComplete()
		{
			if (!IsVisible) return;

			//if (ProcessResets)
			{
				SearchTimer.Stop();
				RetrievalTimer.Stop();
			}
		}

		/// <summary>
		/// Update display with latest stats
		/// </summary>
		/// <param name="qe"></param>
		/// <param name="mbRowCounts"></param>
		/// <param name="mbTimes"></param>

		public static void UpdateStatsDisplay(
			Query q,
			QueryEngine qe,
			List<MetaBrokerStats> mbStats)
		{
			//if (!IsVisible) return;

			Instance.UpdateStatsDisplay2(q, qe, mbStats);
		}

		/// <summary>
		/// Update display with latest stats
		/// </summary>
		/// <param name="qe"></param>
		/// <param name="mbRowCounts"></param>
		/// <param name="mbTimes"></param>

		public void UpdateStatsDisplay2(
			Query q,
			QueryEngine qe,
			List<MetaBrokerStats> mbStats)
		{
			DataRow dr;

			//if (Query != q) //  || Query.Tables.Count != DataTable.Rows.Count) // if query differs or doesn't match DataTable, show current query
			//	StartNewQueryExecution(q);

			if (mbStats == null) return;

			Query = q;
			Qe = qe; 

			Grid.BeginUpdate();

			DataTable.Clear();

			int sbc = 0; // search step brokers seen
			int qtc = 0; // retrieval step brokers seen

			MetatableTotalRows = OracleTotalRows = 0;

			for (int si = 0; si < mbStats.Count; si++)
			{
				MetaBrokerStats mbs = mbStats[si];
				if (Lex.IsDefined(mbs.Label)) // search step
				{
					dr = DataTable.NewRow();
					SetDataRow(dr, null, mbs);
					DataTable.Rows.Add(dr);
					sbc++;
				}

				else // retrieval step
				{
					if (qtc >= Query.Tables.Count) continue;

					QueryTable qt = Query.Tables[qtc];
					dr = DataTable.NewRow();
					SetDataRow(dr, qt, mbs);
					DataTable.Rows.Add(dr);
					qtc++;
				}
			}

			Grid.EndUpdate();
			Grid.Refresh();

			UpdateTotals();

			return;
		}

		DataTable CreateDataTable()
		{
			DataTable dt = new DataTable();
			DataTable = dt; // save ref to table
			dt.Columns.Add("MetatableLabel", typeof(string));
			dt.Columns.Add("MetatableName", typeof(string));
			dt.Columns.Add("ColumnCount", typeof(int));
			dt.Columns.Add("ColumnsSelected", typeof(int));
			dt.Columns.Add("CriteriaCount", typeof(int));
			dt.Columns.Add("BrokerCol", typeof(string));
			dt.Columns.Add("Multipivot", typeof(string));
			dt.Columns.Add("NextRowCount", typeof(int));
			dt.Columns.Add("ReadRowCount", typeof(int));
			dt.Columns.Add("OracleTime", typeof(double));

			return dt;
		}

		void SetDataRow(
			DataRow dr, 
			QueryTable qt,
			MetaBrokerStats mbs)
		{
			string txt;
			int i1= -1;

			bool searchStep = (qt == null);
			bool retrievalStep = (qt != null);

			if (searchStep) // search step
			{
				try
				{
					string[] sa = mbs.Label.Split(':');

					dr["MetatableLabel"] = sa[0]; // search substep
					dr["MetatableName"] = sa[1]; // one or more tables

					int.TryParse(sa[2], out i1);
					if (i1 >= 0) dr["ColumnCount"] = i1;

					int.TryParse(sa[3], out i1);
					if (i1 >= 0) dr["ColumnsSelected"] = i1;

					int.TryParse(sa[4], out i1);
					if (i1 >= 0) dr["CriteriaCount"] = i1;

					dr["BrokerCol"] = Lex.Replace(sa[5], "Broker", ""); // remove any Broker suffix
				}

				catch (Exception ex) { return; }
			}

			else // retrieval step
			{
				dr["MetatableLabel"] = qt.ActiveLabel;
				dr["MetatableName"] = qt.MetaTable.Name;

				dr["ColumnCount"] = qt.VisibleColumnCount; // includes Selected_or_Criteria_or_GroupBy_or_Sorted
				dr["ColumnsSelected"] = qt.SelectedCount;
				dr["CriteriaCount"] = qt.GetCriteriaCount(true, false);

				txt = qt.MetaTable.MetaBrokerType.ToString();
				dr["BrokerCol"] = Lex.Replace(txt, "Broker", ""); // remove any Broker suffix
			}

			if (mbs != null)
			{
				if (mbs.MultiPivot <= 0) dr["Multipivot"] = "";
				else if (mbs.MultiPivot == 1) dr["Multipivot"] = "1";
				else if (mbs.MultiPivot == 2) dr["Multipivot"] = "2";
				else dr["Multipivot"] = "?";

				if (mbs.MetatableRowCount >= 0)
				{
					dr["NextRowCount"] = mbs.MetatableRowCount;
					if (retrievalStep) MetatableTotalRows += mbs.MetatableRowCount;

				}
				else dr["NextRowCount"] = DBNull.Value;

				if (mbs.OracleRowCount >= 0)
				{
					dr["ReadRowCount"] = mbs.OracleRowCount;
					if (retrievalStep) OracleTotalRows += mbs.OracleRowCount;
				}
				else dr["ReadRowCount"] = DBNull.Value;

				if (mbs.Time > 0)
					dr["OracleTime"] = mbs.Time / 1000.0; // store fractional seconds as double
				else dr["OracleTime"] = DBNull.Value;
			}

			return;
		}

		private void GridView_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
		{
			if (!e.Info.IsRowIndicator) return;
			if (e.RowHandle < 0)
				return; // may be NewItemRow indicator at initialization

			e.Info.DisplayText = (e.RowHandle + 1).ToString();
			e.Info.ImageIndex = -1; // remove any image that would overlay the row number
		}

		private void ShowSqlButton_Click(object sender, EventArgs e)
		{
			QbUtil.ShowQuerySqlStatements(Query);
			return;
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			if (IsVisible) UpdateTotals();
		}

		void UpdateTotals()
		{
			int st = 0, rt = 0;
			if (Qe != null)
			{
				SearchKeyCount = Qe.KeyCount;
				GridRowCount = Qe.RowsRetrievedCount;
			}

			else SearchKeyCount = GridRowCount = 0;

			st = (int)SearchTimer.Elapsed.TotalSeconds;
			SearchTimeCtl.Text = "Search time: " + st;
			SearchHitCountCtl.Text = "Key count: " + SearchKeyCount;

			rt = (int)RetrievalTimer.Elapsed.TotalSeconds;
			RetrievalTimeCtl.Text = "Retrieval time: " + rt;

			GridRowsCtl.Text = "Grid rows: " + GridRowCount;
			MetatableRowsCtl.Text = "Metatable rows: " + MetatableTotalRows;
			OracleRowsCtl.Text = "DB Server rows: " + OracleTotalRows;

			return;
		}

	}

}