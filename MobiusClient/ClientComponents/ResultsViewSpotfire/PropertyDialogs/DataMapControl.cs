using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;
using Mobius.ClientComponents;
using Mobius.SpotfireDocument;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraGrid.Views.Layout.ViewInfo;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.BandedGrid.ViewInfo;
using DevExpress.Data;
using DevExpress.Utils;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Mobius.SpotfireClient
{
	public partial class DataMapControl : DevExpress.XtraEditors.XtraUserControl
	{
		SpotfireViewManager SVM; // view manager associated with this dialog
		SpotfireViewProps SVP => SVM?.SVP;
		Query Query => SVP?.Query; // Mobius query that supplies data to the Spotfire view

		public DataTableMapsMsx DataTableMaps;  // associated DataMaps, not necessarily same as in SVP
		public DataTableMapMsx CurrentMap => DataTableMaps?.CurrentMap;

		SpotfireApiClient Api => SpotfireSession.SpotfireApiClient;  // API link to Spotfire session to interact with
		
		Point FieldGridP0 = new Point(); // original position of grid

		/// <summary>
		/// If CanEditMapping == true allow full definition of mapping
		/// otherwise just allow renaming of Spotfire columns
		/// </summary>

		[DefaultValue(false)]
		public bool CanEditMapping
		{
			get { return _canEditMapping; }
			set { _canEditMapping = value; }
		}
		bool _canEditMapping = true;

		/// <summary>
		/// Show source query controls
		/// </summary>

		[DefaultValue(true)]
		public bool ShowTableControls
		{
			get { return _showTableControls; }
			set { _showTableControls = value; }
		}
		bool _showTableControls = true;

		[DefaultValue(false)]
		public bool ShowSelectedColumnCheckBoxes { get { return FieldSelectedCol.Visible; } set { SelectedBand.Visible = FieldSelectedCol.Visible = value; } }

		[DefaultValue(true)]
		public bool SelectSingleColumn
		{
			get { return _selectSingleColumn; }
			set { _selectSingleColumn = value; }
		}
		bool _selectSingleColumn = false;

		public GridView FieldGridView { get { return FieldGrid.Views[0] as GridView; } }
		DataTable FieldDataTable;

		bool InSetup = false;
		public bool Changed = false;
		public event EventHandler CallerEditValueChangedHandler; // event to fire when edit value changes

		public DataMapControl()
		{
			InitializeComponent();

			FieldGridP0 = FieldGrid.Location;
		}

		public void Setup(
			SpotfireViewManager svm,
			EventHandler callerEditValueChangedHandler = null)
		{
			DataTableMapMsx dataMap = null;
			Setup(svm, dataMap, callerEditValueChangedHandler);
		}

		/// <summary>
		/// Setup the control for specified view
		/// </summary>
		/// <param name="view"></param>

		public void Setup(
			SpotfireViewManager svm,
			DataTableMapMsx dataMap = null,
			EventHandler callerEditValueChangedHandler = null)
		{
			DataRow dr;
			DataColumn dc;

			InSetup = true;

			SVM = svm;
			DataTableMaps = svm?.SVP.DataTableMaps;

			CallerEditValueChangedHandler = callerEditValueChangedHandler;

			DataTableSelectorControl.Setup(DataTableMaps, DataTableSelectorControl_EditValueChanged);

			QtSelectorControl.Setup(CurrentMap, QueryTableAssignmentChanged);

			GridView.OptionsSelection.MultiSelect = !SelectSingleColumn;
			GridView.OptionsBehavior.Editable = !SelectSingleColumn;

			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn("DataTypeImageField", typeof(Image)));
			dt.Columns.Add(new DataColumn("SpotfireColNameField", typeof(string)));
			dt.Columns.Add(new DataColumn("OriginalSpotfireColNameField", typeof(string)));
			dt.Columns.Add(new DataColumn("SelectedField", typeof(bool)));
			dt.Columns.Add(new DataColumn("MobiusTableNameField", typeof(string)));
			dt.Columns.Add(new DataColumn("MobiusColNameField", typeof(string)));
			dt.Columns.Add(new DataColumn("ColumnMapMsxRefField", typeof(ColumnMapMsx)));

			dt.RowChanged += new DataRowChangeEventHandler(Dt_RowChanged); // Monitor row changes

			//dr = dt.NewRow(); // add blank row at bottom
			//dt.Rows.Add(dr);

			FieldDataTable = dt;
			FieldGrid.DataSource = dt;

			UpdateFieldGridDataTable(); // do initial fill of the grid DataTable

			HeaderPanel.Enabled = CanEditMapping;

			Changed = false;
			InSetup = false;
			return;
		}

		/// <summary>
		/// Fill the field grid with current mappings
		/// </summary>

		internal void UpdateFieldGridDataTable()
		{
			DataRow dr;
			DataColumn dc;
			MetaColumnType mcType = MetaColumnType.Unknown;

			bool excludeNoRoleColumns = false; // if true don't include columns that don't have a role from the initial template analysis file

			InSetup = true;

			DataTable dt = FieldDataTable;
			dt.Clear();

			if (CurrentMap != null)
				foreach (ColumnMapMsx cm in CurrentMap.ColumnMapList)
				{
					if (excludeNoRoleColumns && Lex.IsUndefined(cm.Role)) continue;

					dr = dt.NewRow();

					QueryColumn qc = cm.QueryColumn;

					if (qc != null) // column is mapped
					{
						dr["MobiusTableNameField"] = qc.QueryTable.ActiveLabel;
						dr["MobiusColNameField"] = qc.ActiveLabel;
						mcType = qc.MetaColumn.DataType;
					}

					else // no query column assigned, get type image from spotfire if including these
					{
						mcType = DataTableMapMsx.GetMatchingMobiusDataTypeFromSpotfireColumn(cm.SpotfireColumn);
					}

					int imgIdx = (int)MetaColumn.GetMetaColumnDataTypeImageIndex(mcType);
					if (mcType == MetaColumnType.CompoundId) imgIdx = (int)Bitmaps16x16Enum.Key;
					dr["DataTypeImageField"] = Bitmaps.Bitmaps16x16.Images[imgIdx];

					dr["SpotfireColNameField"] = cm.SpotfireColumnName;
					dr["OriginalSpotfireColNameField"] = cm.Role;

					dr["SelectedField"] = cm.Selected;

					dr["ColumnMapMsxRefField"] = cm; // store Field ref
					dt.Rows.Add(dr);
				}

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
			return;
		}

		private void GridView_MouseDown(object sender, MouseEventArgs e)
		{
			//ClientLog.Message("GridView_MouseDown");

			return;
		}

		private void GridView_MouseUp(object sender, MouseEventArgs e)
		{
			//ClientLog.Message("GridView_MouseUp");
			DelayedCallback.Schedule(GridView_MouseUp_Callback, e); // schedule callback
		}


		private void GridView_MouseUp_Callback(object state)
		{
			ColumnMapMsx cm;
			QueryColumn qc;
			QnfEnum subColumn;
			DialogResult dr;
			string newSpotfireName;

			MouseEventArgs e = state as MouseEventArgs;

			if (e == null) return;

			Point p = new Point(e.X, e.Y);

			GridHitInfo gridHitInfo = FieldGridView.CalcHitInfo(p);
			GridViewInfo gridViewInfo = FieldGridView.GetViewInfo() as GridViewInfo;
			GridCellInfo gridCellInfo = gridViewInfo.GetGridCellInfo(gridHitInfo);

			int ri = gridHitInfo.RowHandle;
			if (ri < 0) return;
			GridColumn gc = gridHitInfo.Column;
			if (gc == null) return;
			int c = gc.AbsoluteIndex;

			if (SelectSingleColumn) // just do a simple single column selection
			{
				UpdateSelectSingleColumnData(ri);
				return;
			}

			DataRow dRow = FieldDataTable.Rows[ri];

			//DataMap.ColumnMapList.Items[ri];

			cm = dRow["ColumnMapMsxRefField"] as ColumnMapMsx; // get ColumnMap for current item

			if (cm == null) throw new Exception("Null ColumnMapMsxRefField");
			//if (cm == null) qc = null;
			//else qc = cm.QueryColumn;

			qc = cm.QueryColumn;
			subColumn = cm.SubColumn;

			// Rename column

			if (gc == SpotfireColNameCol)
			{
				RenameColumn(ri);
				return;
			}

			// Commands checked below are only available if editing of the mapping is allowed

			if (!CanEditMapping) return;

			// Select which Mobius column maps to a Spotfire Column

			if (gc == MobiusTableNameCol || gc == MobiusColNameCol) // select Mobius column to match if defining mapping
			{
				string role = dRow["OriginalSpotfireColNameField"] as string;
				if (Lex.IsUndefined(role)) return; // can only assign cols with "roles" from the original template analysis

				string spotfireName = "";
				FieldSelectorControl fieldSelector = new FieldSelectorControl();
				fieldSelector.QueryColumn = qc;

				SelectColumnOptions flags = new SelectColumnOptions();
				flags.ExcludeImages = true;
				flags.FirstTableKeyOnly = true;
				flags.SelectFromQueryOnly = true;
				flags.QnSubcolsToInclude = QnfEnum.Split | QnfEnum.Qualifier | QnfEnum.NumericValue | QnfEnum.NValue; // split QualifiedNumbers
				flags.IncludeNoneItem = true;

				flags.AllowedDataTypes = DataTableMapMsx.GetMetaColumnTypesCompatibleWithSpotfireColumn(cm.SpotfireColumn, allowCompoundId: (ri == 0));

				p = FieldGrid.PointToScreen(p);
				if (CurrentMap.QueryTable != null) // select from single query table only
					dr = fieldSelector.SelectColumnFromQueryTable(CurrentMap.QueryTable, qc, flags, p.X, p.Y, out qc, out subColumn);

				else // select from any table in the query
					dr = fieldSelector.SelectColumnFromQuery(Query, qc, flags, p.X, p.Y, out qc, out subColumn);

				if (dr != DialogResult.OK) return;

				cm.QueryColumn = qc; // new query column mapped to

				if (qc != null) // assigning a new QueryColumn 
				{
					dRow["MobiusTableNameField"] = qc.QueryTable.ActiveLabel;
					dRow["MobiusColNameField"] = qc.ActiveLabel;

					newSpotfireName = CurrentMap.AssignUniqueSpotfireColumnName(qc.ActiveLabel);
					dRow["SpotfireColNameField"] = newSpotfireName; // update grid

					cm.SpotfireColumnName = newSpotfireName; // store new name

					//if (Lex.Ne(cm.SpotfireColumnName, newSpotfireName)) // changing name?
					//{
					//	cm.NewSpotfireColumnName = newSpotfireName; // store new name
					//}

					cm.MobiusFileColumnName = qc.MetaTableDotMetaColumnName + ColumnMapParms.SpotfireExportExtraColNameSuffix;
				}

				else // set to none (e.g. null col)
				{
					dRow["MobiusTableNameField"] = "";
					dRow["MobiusColNameField"] = "";

					newSpotfireName = CurrentMap.AssignUniqueSpotfireColumnName("None");
					dRow["SpotfireColNameField"] = newSpotfireName; // update grid
					cm.SpotfireColumnName = newSpotfireName; // update map
					//cm.NewSpotfireColumnName = newSpotfireName; // update map

					cm.MobiusFileColumnName = ""; // not mapped to a col
				}

				FieldGrid.RefreshDataSource();

				SVM.RemapDataTable(CurrentMap); // update spotfire view accordingly

				UpdateFieldGridDataTable();

				cm.NewSpotfireColumnName = ""; // clear new name used for rename (needed?)
			}

			return;
		}

		/// <summary>
		/// Select a single column by selecting the associated grid row
		/// and updating the selected value in the underllying datasource and column row
		/// </summary>
		/// <param name="ri"></param>
		void UpdateSelectSingleColumnData(int ri)
		{
			ColumnMapMsx cm;

			DataRow selectedDr = FieldDataTable.Rows[ri];
			bool alreadySelected = (bool)selectedDr["SelectedField"];
			if (alreadySelected) return; // already checked

			foreach (DataRow dr0 in FieldDataTable.Rows) // clear any other selected col
			{
				alreadySelected = (bool)dr0["SelectedField"];
				if (!alreadySelected) continue;

				dr0["SelectedField"] = false; // update DataTable

				cm = dr0["ColumnMapMsxRefField"] as ColumnMapMsx;
				cm.Selected = false; // update column map
			}

			selectedDr["SelectedField"] = true; // update datatable for newly selected col
			cm = selectedDr["ColumnMapMsxRefField"] as ColumnMapMsx;
			cm.Selected = true; // update column map

			FieldGridView.SelectRow(ri);

			EditValueChanged();
			return;
		}

		/// <summary>
		/// Rename a Spotfire Column
		/// </summary>
		/// <param name="ri"></param>

		void RenameColumn(int ri)
		{
			DataRow dRow = FieldDataTable.Rows[ri];
			string currentName = dRow["SpotfireColNameField"] as string;

			string newName = InputBoxMx.Show("Name:", "Rename Column", currentName);
			if (Lex.IsUndefined(newName) || newName == currentName) return;

			for (int ri2 = 0; ri2 < FieldDataTable.Rows.Count; ri2++)
			{
				if (ri2 == ri) continue;
				string name = FieldDataTable.Rows[ri2]["SpotfireColNameField"] as string;
				if (Lex.Eq(name, newName))
				{
					MessageBoxMx.ShowError("Name already in use: " + newName);
					return;
				}
			}

			dRow["SpotfireColNameField"] = newName;

			CurrentMap.ColumnMapCollection[ri].SpotfireColumnName = newName;

			Api.RenameColumn(CurrentMap.SpotfireDataTable?.Name, currentName, newName);

			return;
		}

		/// <summary>
		/// Get the Field definitions from the grid
		/// </summary>

		public DataTableMapMsx GetColumnMapList()
		{
			DataTableMapMsx map = new DataTableMapMsx();
			map.ParentMapList = SVP.DataTableMaps;
			DataTableMaps.CurrentMap = map;

			foreach (DataRow dr in FieldDataTable.Rows)
			{
				ColumnMapMsx fli = dr["ColumnMapMsxRefField"] as ColumnMapMsx;
				if (fli == null) continue;

				QueryColumn qc = fli.QueryColumn;
				if (qc == null) continue;

				fli.QueryColumn = qc;
				fli.SpotfireColumnName = dr["SpotfireColNameField"] as string;
				fli.Selected = (bool)dr["SelectedField"]; // not currently used
				CurrentMap.Add(fli);
			}

			return CurrentMap;
		}

		/// <summary>
		/// Monitor row changes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void Dt_RowChanged(object sender, DataRowChangeEventArgs e)
		{
			//EditValueChanged(); // causes problems with single select
		}

		/// <summary>
		/// Value of some form field has changed
		/// </summary>		
		/// 
		void EditValueChanged()
		{
			if (InSetup) return;

			Changed = true;

			if (CallerEditValueChangedHandler != null) // fire EditValueChanged event if handlers present
				CallerEditValueChangedHandler(this, EventArgs.Empty);
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

		private void GridView_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
		{
			if (e.Column.FieldName == "DataTypeImageField")
			{
				var vi2 = (e.Cell as GridCellInfo).ViewInfo as PictureEditViewInfo;
				if (vi2 == null) return;
				if (vi2.Image == null)
					vi2.Image = Bitmaps.Bitmaps16x16.Images[0];
				return;
			}

		}

		private void SelectAllItems_Click(object sender, EventArgs e)
		{
			for (int fi = 0; fi < CurrentMap.Count; fi++)
			{
				ColumnMapMsx cm = CurrentMap[fi];
				cm.Selected = true;
			}

			UpdateFieldGridDataTable();
		}

		private void DeselectAllItems_Click(object sender, EventArgs e)
		{
			for (int fi = 1; fi < CurrentMap.Count; fi++) // deselect all but first col
			{
				ColumnMapMsx cm = CurrentMap[fi];
				cm.Selected = false;
			}

			UpdateFieldGridDataTable();
		}

		private void SelectDefaultItems_Click(object sender, EventArgs e)
		{
			for (int fi = 0; fi < CurrentMap.Count; fi++)
			{
				ColumnMapMsx cm = CurrentMap[fi];
				MetaColumn mc = cm?.QueryColumn?.MetaColumn;
				if (mc == null) continue;
				cm.Selected = mc.InitialSelection == ColumnSelectionEnum.Selected;
			}

			UpdateFieldGridDataTable();
		}

		private void GridView_CellValueChanging(object sender, CellValueChangedEventArgs e)
		{
			return;
		}

		/// <summary>
		/// Spotfire data table changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DataTableSelectorControl_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			if (CurrentMap == null) return;

			UpdateFieldGridDataTable();

			QtSelectorControl.Setup(CurrentMap, QueryTableAssignmentChanged); // Set associated querytable

			return;
		}

		/// <summary>
		/// New QueryTable to map to selected
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void QueryTableAssignmentChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			CurrentMap.QueryTable = QtSelectorControl.SelectedQueryTable;

			if (QtSelectorControl.IsMapped)
			{
				CurrentMap.AssignInitialMappingForQueryTable(CurrentMap.QueryTable);
				SVM.ImportMobiusDataFile(CurrentMap); // update spotfire view accordingly
				//SVM.MergeMobiusDataFileAndRemapDataTable(CurrentMap); // update spotfire view accordingly
			}

			else // not mapped, restore original table def
			{
				CurrentMap.Clear();
				DataTableMsx dt = SVM.SpotfireApiClient.ResetDataTable(CurrentMap.SpotfireDataTable);
				CurrentMap.InitializeMapForDataTable(dt);
			}

			UpdateFieldGridDataTable();

			return;
		}

		private void MapOptionsButton_Click(object sender, EventArgs e)
		{
			//ShowDataMapOptionsDialog();
		}

#if false // not currently used
		void ShowDataMapOptionsDialog()
		{
			DialogResult dr = DataMapOptionsDialog.ShowDialog(CurrentMap);
			if (dr != DialogResult.OK) return;

			if (DataTableMaps.WriteSingleDataFile != DataMapOptionsDialog.WriteSingleDataFile)
			{
				DataTableMaps.WriteSingleDataFile = DataMapOptionsDialog.WriteSingleDataFile;

				if (DataTableMaps.WriteSingleDataFile)
				{
					// todo
				}

				else
				{
					// todo
				}
			}

			if (DataTableMaps.SummarizationOneRowPerKey != DataMapOptionsDialog.SummarizationOneRowPerKey)
			{
				DataTableMaps.SummarizationOneRowPerKey = DataMapOptionsDialog.SummarizationOneRowPerKey;

				if (DataTableMaps.SummarizationOneRowPerKey)
				{
					// todo
				}
				else
				{
					// todo
				}
			}

			if (CurrentMap.QueryTable != DataMapOptionsDialog.QueryTable)
			{
				CurrentMap.QueryTable = DataMapOptionsDialog.QueryTable;

				CurrentMap.AssignInitialMappingForSpotfireDataTable(CurrentMap.SpotfireDataTable, CurrentMap.QueryTable);

				UpdateFieldGridDataTable();
			}

			return;
		}
#endif

	}
}
