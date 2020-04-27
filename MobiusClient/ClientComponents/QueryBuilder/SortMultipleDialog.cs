using Mobius.ComOps;
using Mobius.Data;

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
/// <summary>
/// Get input for sort of multiple fields
/// </summary>

	public partial class SortMultipleDialog : DevExpress.XtraEditors.XtraForm
	{
		static SortMultipleDialog Instance;

		Query Query;
		List<SortColumn> Columns = new List<SortColumn>();
		int SelectedCol = 0;

		public SortMultipleDialog()
		{
			InitializeComponent();
		}

		public static List<SortColumn> ShowDialog(Query query)
		{
			if (Instance == null) Instance = new SortMultipleDialog();
			Instance.Query = query;
			Instance.Setup();
			Instance.SelectCol(0);
			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return null;
			else return Instance.Columns;
		}

		void Setup()
		{
			for (int ci = 0; ci < 6; ci++)
			{
				SetupColumn(0, Column1, Ascending1, Descending1);
				SetupColumn(1, Column2, Ascending2, Descending2);
				SetupColumn(2, Column3, Ascending3, Descending3);
				SetupColumn(3, Column4, Ascending4, Descending4);
				SetupColumn(4, Column5, Ascending5, Descending5);
				SetupColumn(5, Column6, Ascending6, Descending6);
			}
		}

		void SetupColumn(int ci, FieldSelectorControl fs, CheckEdit ascending, CheckEdit descending)
		{ 
			fs.Query = Query;
			if (ci >= Columns.Count)
			{
				fs.MetaColumn = null;
				ascending.Checked = true;
				return;
			}

			QueryColumn qc = Columns[ci].QueryColumn;
			if (qc != null)
			{ // be sure any existing cols are still in query
				QueryTable qt2 = Query.GetTableByName(qc.QueryTable.MetaTable.Name);
				if (qt2 != null)
				{
					QueryColumn qc2 = qt2.GetQueryColumnByName(qc.MetaColumn.Name);
					if (qc2 == qc)
					{
						fs.MetaColumn = qc.MetaColumn;
						if (Columns[ci].Direction == SortOrder.Ascending)
							ascending.Checked = true;
						else descending.Checked = true;
						return;
					}
				}
			}

			fs.MetaColumn = null;
			ascending.Checked = true;
		}

		private void SortButton_Click(object sender, EventArgs e)
		{
			GetColumns();

			if (Columns.Count == 0)
				MessageBoxMx.ShowError("At least one sort column must be defined");

			else DialogResult = DialogResult.OK;
		}

		private void GetColumns()
		{
			Columns = new List<SortColumn>();
			AddColumn(Column1, Ascending1);
			AddColumn(Column2, Ascending2);
			AddColumn(Column3, Ascending3);
			AddColumn(Column4, Ascending4);
			AddColumn(Column5, Ascending5);
			AddColumn(Column6, Ascending6);
		}

		void AddColumn(FieldSelectorControl fs, CheckEdit ascending)
		{
			if (fs.QueryColumn == null) return;
			SortColumn sc = new SortColumn();
			sc.QueryColumn = fs.QueryColumn;
			if (ascending.Checked) sc.Direction = SortOrder.Ascending;
			else sc.Direction = SortOrder.Descending;
			Columns.Add(sc);
		}

		private void Clear_Click(object sender, EventArgs e)
		{
			Column1.MetaColumn = null;
			Column2.MetaColumn = null;
			Column3.MetaColumn = null;
			Column4.MetaColumn = null;
			Column5.MetaColumn = null;
			Column6.MetaColumn = null;
		}

		private void Up_Click(object sender, EventArgs e)
		{
			GetColumns();
			int ci = SelectedCol;
			if (ci <= 0 || ci>= Columns.Count) return;
			SortColumn sc = Columns[ci];
			Columns.RemoveAt(ci);
			Columns.Insert(ci - 1, sc);
			Setup();
			SelectCol(ci - 1);
		}

		private void Down_Click(object sender, EventArgs e)
		{
			GetColumns();
			int ci = SelectedCol;
			if (ci < 0 || ci >= Columns.Count - 1) return;
			SortColumn sc = Columns[ci];
			Columns.RemoveAt(ci);
			Columns.Insert(ci + 1, sc);
			Setup();
			SelectCol(ci + 1);
		}

		private void Delete_Click(object sender, EventArgs e)
		{
			GetColumns();
			int ci = SelectedCol;
			if (ci < 0 || ci >= Columns.Count) return;
			SortColumn sc = Columns[ci];
			Columns.RemoveAt(ci);
			Setup();
			SelectCol(ci);
		}

		void SelectCol(int ci)
		{
			GetColumns();
			if (ci >= Columns.Count) ci--;
			if (ci < 0) ci = 0;
			if (ci == 0) Column1.Focus();
			else if (ci == 1) Column2.Focus();
			else if (ci == 2) Column3.Focus();
			else if (ci == 3) Column4.Focus();
			else if (ci == 4) Column5.Focus();
			else if (ci == 5) Column6.Focus();

			SelectedCol = ci;
		}

		private void Column1_Click(object sender, EventArgs e)
		{
			SelectedCol = 0;
		}

		private void Column2_Click(object sender, EventArgs e)
		{
			SelectedCol = 1;
		}

		private void Column3_Click(object sender, EventArgs e)
		{
			SelectedCol = 2;
		}

		private void Column4_Click(object sender, EventArgs e)
		{
			SelectedCol = 3;
		}

		private void Column5_Click(object sender, EventArgs e)
		{
			SelectedCol = 4;
		}

		private void Column6_Click(object sender, EventArgs e)
		{
			SelectedCol = 5;
		}

	}
}