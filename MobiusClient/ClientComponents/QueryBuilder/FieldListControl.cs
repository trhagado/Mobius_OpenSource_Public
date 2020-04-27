using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Layout.ViewInfo;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.BandedGrid.ViewInfo;
using DevExpress.Data;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Mobius.ClientComponents
{
	public partial class FieldListControl : DevExpress.XtraEditors.XtraUserControl
	{
		Query Query = null; // associated query
		ColumnMapCollection FieldList = null; // field list

		GridView FieldGridView { get { return FieldGrid.Views[0] as GridView; } }
		DataTable FieldDataTable;
		bool InSetup = false;
		public bool Changed = false;
		public event EventHandler EditValueChanged; // event to fire when edit value changes

		MouseEventArgs LastMouseDownEventArgs;

		public FieldListControl()
		{
			InitializeComponent();
		}

		public void Setup(
			Query query,
			ColumnMapCollection fieldList)
		{
			Query = query;
			FieldList = fieldList;
			SetupFieldGrid();
		}

		internal void SetupFieldGrid()
		{
			InSetup = true;
			DataRow dr;
			DataColumn dc;

			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn("FieldSelectedCol", typeof(bool)));
			dt.Columns.Add(new DataColumn("FieldDatabaseTableCol", typeof(string)));
			dt.Columns.Add(new DataColumn("FieldDatabaseColumnCol", typeof(string)));
			dt.Columns.Add(new DataColumn("SpotfireColumnName", typeof(string)));
			dt.Columns.Add(new DataColumn("ColumnMapMsxRefCol", typeof(ColumnMapMsx)));

			dt.RowChanged += new DataRowChangeEventHandler(Dt_RowChanged); // Monitor row changes

			if (FieldList != null)
				foreach (ColumnMapMsx f in FieldList.ColumnMapList)
				{
					if (f.QueryColumn == null) continue;
					dr = dt.NewRow();
					dr["FieldSelectedCol"] = f.Selected;
					dr["FieldDatabaseTableCol"] = f.QueryColumn.QueryTable.ActiveLabel;
					dr["FieldDatabaseColumnCol"] = f.QueryColumn.ActiveLabel;
					dr["SpotfireColumnName"] = f.ParameterName;
					//dr["FieldMinCol"] = String.IsNullOrEmpty(ax.RangeMin) ? "Automatic" : ax.RangeMin;
					//dr["FieldMaxCol"] = String.IsNullOrEmpty(ax.RangeMax) ? "Automatic" : ax.RangeMax;
					dr["ColumnMapMsxRefCol"] = f; // store Field ref
					dt.Rows.Add(dr);
				}

			dr = dt.NewRow(); // add blank row at bottom
			dt.Rows.Add(dr);

			FieldDataTable = dt;
			FieldGrid.DataSource = dt;

			Changed = false;
			InSetup = false;
			return;
		}

		private void GridView_CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
		{
			if (e.Info.IsRowIndicator && e.RowHandle >= 0)
			{
				e.Info.DisplayText = (e.RowHandle + 1).ToString();
				e.Info.ImageIndex = -1; // no image
			}
		}


		private void GridView_RowCellClick(object sender, RowCellClickEventArgs e)
		{
		}

		private void GridView_MouseDown(object sender, MouseEventArgs e)
		{
			//ClientLog.Message("GridView_MouseDown");

			LastMouseDownEventArgs = e; // just save for click event to avoid need for extra mouse click
		}

		private void GridView_MouseUp(object sender, MouseEventArgs e)
		{
			//ClientLog.Message("GridView_MouseUp");
			Timer.Enabled = true; // handle event in timer to avoid strange mouse behavior
		}

		private void GridView_Click(object sender, EventArgs e0)
		{
			//ClientLog.Message("GridView_Click");
		}

		private void GridView_ValidatingEditor(object sender, BaseContainerValidateEditorEventArgs e)
		{
			GridColumn gc = FieldGridView.FocusedColumn;
			if (gc == null) return;

		// no validation here now
		//  if (Lex.Ne(gc.FieldName, "AxisMinCol") && Lex.Ne(gc.FieldName, "AxisMaxCol"))
		//    return;

		//  if (e.Value != null && !AxisOptionsControl.IsValidRangeValue(e.Value.ToString()))
		//  {
		//    e.Valid = false;
		//    e.ErrorText = "Invalid value";
		//  }

		  return;
		}

		private void Timer_Tick(object sender, EventArgs e0)
		{
			QueryColumn qc;

			if (!Timer.Enabled) return;

			Timer.Enabled = false;

			MouseEventArgs e = LastMouseDownEventArgs;
			Point p = new Point(e.X, e.Y);
			GridHitInfo ghi = FieldGridView.CalcHitInfo(p);

			int ri = ghi.RowHandle;
			if (ri < 0) return;
			GridColumn gc = ghi.Column;
			if (gc == null) return;
			int c = gc.AbsoluteIndex;

			DataRow dRow = FieldDataTable.Rows[ri];
			ColumnMapMsx i = dRow["ColumnMapMsxRefCol"] as ColumnMapMsx; // currently item
			if (i == null) qc = null;
			else qc = i.QueryColumn;

			if (Lex.Eq(gc.FieldName, "FieldDatabaseTableCol") || Lex.Eq(gc.FieldName, "FieldDatabaseColumnCol"))
			{
				FieldSelectorControl fieldSelector = new FieldSelectorControl();
				fieldSelector.QueryColumn = qc;

				p = this.PointToScreen(p);

				SelectColumnOptions sco = new SelectColumnOptions();

				sco.ExcludeImages = true;

				sco.FirstTableKeyOnly = true;
				sco.SelectFromQueryOnly = true;
				sco.IncludeNoneItem = true;

				DialogResult dr = fieldSelector.SelectColumnFromQuery(Query, fieldSelector.QueryColumn, sco, p.X, p.Y, out qc);
				if (dr != DialogResult.OK) return;

				if (qc != null)
				{
					if (i == null) i = new ColumnMapMsx();
					i.QueryColumn = qc;
					dRow["FieldSelectedCol"] = true;
					dRow["ColumnMapMsxRefCol"] = i;
					dRow["FieldDatabaseTableCol"] = qc.QueryTable.ActiveLabel;
					dRow["FieldDatabaseColumnCol"] = qc.ActiveLabel;
					FieldGrid.RefreshDataSource();
				}

				else // set to none
				{
					dRow["ColumnMapMsxRefCol"] = null;
					dRow["FieldDatabaseTableCol"] = dRow["FieldDatabaseColumnCol"] = "";
				}

				if (ri == FieldDataTable.Rows.Count - 1)
				{ // add blank row at end if needed
					dRow = FieldDataTable.NewRow();
					FieldDataTable.Rows.Add(dRow);
				}
			}

			//else if (Lex.Eq(gc.FieldName, "EditAxisPropertiesCol"))
			//{
			//  DialogResult dr = AxisOptionsDialog.ShowDialog(ax);
			//  if (dr == DialogResult.OK) Changed = true;
			//}

			return;
		}

/// <summary>
/// Get the Field definitions from the grid
/// </summary>

		public ColumnMapCollection GetFields()
		{
			FieldList = new ColumnMapCollection();
			foreach (DataRow dr in FieldDataTable.Rows)
			{
				ColumnMapMsx fli = dr["ColumnMapMsxRefCol"] as ColumnMapMsx;
				if (fli == null) continue;
				QueryColumn qc = fli.QueryColumn;
				if (qc == null) continue;
				fli.QueryColumn = qc;
				fli.ParameterName = dr["SpotfireColumnName"] as string;
				fli.Selected = (bool)dr["FieldSelectedCol"];
				FieldList.Add(fli);
			}

			return FieldList;
		}

		/// <summary>
		/// Monitor row changes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void Dt_RowChanged(object sender, DataRowChangeEventArgs e)
		{
			if (InSetup) return;

			Changed = true;

			if (EditValueChanged != null) // fire EditValueChanged event if handlers present
				EditValueChanged(this, EventArgs.Empty);
			return;
		}

		private void AddField_Click(object sender, EventArgs e)
		{
			int r = FieldGridView.FocusedRowHandle;
			DataRow dr = FieldDataTable.NewRow();
			if (r < 0 || r + 1 >= FieldDataTable.Rows.Count)
			{
				FieldDataTable.Rows.Add(dr);
				FieldGridView.FocusedRowHandle = FieldDataTable.Rows.Count - 1;
			}
			else
			{
				FieldDataTable.Rows.InsertAt(dr, r + 1);
				FieldGridView.FocusedRowHandle = r + 1;
			}

		}

		private void DeleteFieldBut_Click(object sender, EventArgs e)
		{
			int r = FieldGridView.FocusedRowHandle;
			if (r >= 0 && r < FieldGridView.RowCount)
			{
				FieldGridView.DeleteRow(r);
				Changed = true;
			}
		}

		private void MoveFieldDown_Click(object sender, EventArgs e)
		{
			DataRow dr, dr2;

			int r = FieldGridView.FocusedRowHandle;
			if (r < 0) return;
			if (r >= FieldDataTable.Rows.Count - 1) return;

			dr = MoveFieldGridRowDown(r);
			FieldGridView.SelectRow(r + 1);
			FieldGridView.FocusedRowHandle = r + 1;
			FieldGridView.Focus();
			return;
		}

		/// <summary>
		/// Move a row down one
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="r"></param>
		/// <returns></returns>

		private DataRow MoveFieldGridRowDown(
			int r)
		{
			DataRow dr = FieldDataTable.Rows[r];
			if (r == FieldDataTable.Rows.Count - 1) return dr;

			DataRow newRow = FieldDataTable.NewRow();
			newRow.ItemArray = dr.ItemArray; // copy data
			FieldDataTable.Rows.RemoveAt(r);
			FieldDataTable.Rows.InsertAt(newRow, r + 1);
			return dr;
		}

		private void MoveFieldUp_Click(object sender, EventArgs e)
		{
			DataRow dr, dr2;

			int r = FieldGridView.FocusedRowHandle;
			if (r <= 0) return;

			dr = MoveFieldGridRowUp(r);
			FieldGridView.SelectRow(r - 1);
			FieldGridView.FocusedRowHandle = r - 1;
			FieldGridView.Focus();
			return;

		}

		/// <summary>
		/// Move a row up one
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="r"></param>
		/// <returns></returns>

		private DataRow MoveFieldGridRowUp(
			int r)
		{
			DataRow dr = FieldDataTable.Rows[r];
			if (r == 0) return dr;

			DataRow newRow = FieldDataTable.NewRow();
			newRow.ItemArray = dr.ItemArray; // copy data
			FieldDataTable.Rows.RemoveAt(r);
			FieldDataTable.Rows.InsertAt(newRow, r - 1);
			return dr;
		}

	}

}
