using Mobius.ComOps;
using Mobius.Data;
using Mobius.CdkMx;

using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraTab;
using DevExpress.Utils;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class QueryTableControl : XtraUserControl
	{
		static QueryTableControl Instance;

		internal QueryTable Qt = null; // QueryTable object to edit
		internal QueryColumn CurrentQc; // last QueryColumn selected
		internal Point LastGridMouseDownPosition;
		internal int GridRow; // index of last grid row selected, corresponds to CurrentQc
		internal int GridCol; // index of last grid col selected
		internal bool EditCriteriaOnMouseUp = false; // if true then edit criteria on current col on mouse up
		internal DataTableMx GridDt = null; // DataTable associated with grid
		internal RepositoryItemPictureEdit StructureViewerControl; // repository item for viewing structures

		internal int[] QtGridRowToQueryColIdx; // maps query builder row to Query column Index
		internal int[] QueryColIdxToQbGridRow; // maps Query column Index to query builder row

		internal DataTableMx DataTable = null; // DataTable of visible QueryColumns

		internal static bool ShowMqlNames = false; // if true show sql names
		internal static bool ShowHiddenFields = false; // if true show hidden QueryTable fields

		GridHitInfo downHitInfo = null; // used for dragging rows

		// Query table grid column assignments

		internal const int ColNameColIdx = 0; // column name
		internal const int SelectedColIdx = 1; // column selected
		internal const int CriteriaColIdx = 2; // criteria
		internal const int AggregationColIdx = 3; // aggregation attributes
		internal const int SortOrderColIdx = 4; // sort position

		/// <summary>
		/// Constructor
		/// </summary>
		/// 
		public QueryTableControl()
		{
			InitializeComponent();
		}

		/// <summary>
		/// QueryTable object to edit
		/// </summary>
		public QueryTable QueryTable
		{
			get
			{
				return Qt;
			}

			set
			{
				Qt = value;
				Render(); // show it
			}
		}

		/// <summary>
		/// If true just display field names & select column
		/// </summary>

		public bool SelectOnly
		{
			get
			{
				return _selectOnly;
			}
			set
			{
				_selectOnly = value;
			}
		}
		bool _selectOnly = false;

		/// <summary>
		/// If true allow selection of just a single field
		/// </summary>

		public bool SelectSingle
		{
			get
			{
				return _selectSingle;
			}
			set
			{
				_selectSingle = value;
			}
		}
		bool _selectSingle = false;

		/// <summary>
		/// If true allow deselection of the key column
		/// </summary>

		public bool CanDeselectKeyCol
		{
			get
			{
				return _canDeselectKeyCol;
			}
			set
			{
				_canDeselectKeyCol = value;
			}
		}
		bool _canDeselectKeyCol = false;


		/// <summary>
		/// Set current query table control & render it
		/// </summary>
		/// <param name="qt"></param>

		public void Render(
			QueryTable qt)
		{
			QueryTable = qt;
			if (qt.Query != null) qt.Query.CurrentTable = qt; // now the current table for the query
			Render();
			return;
		}

		/// <summary>
		/// Render the query table
		/// </summary>

		public void Render()
		{
			string menuhdr;
			string itemNumberText, criteriaText, formatText;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			GridView v;
			QtGridRow gr;

			int fi, ci, i1;
			string txt, txt2;

			QueryTable qt = QueryTable;
			if (qt == null) return;
			Query query = qt.Query;

			bool showAggCol = (qt.AggregationEnabled || qt.AggregatedCount > 0);
			AggregationColumn.Visible = showAggCol;

			QtGridRowToQueryColIdx = new int[qt.QueryColumns.Count]; // map of qb grid item to query col 
			QueryColIdxToQbGridRow = new int[qt.QueryColumns.Count];
			for (ci = 0; ci < qt.QueryColumns.Count; ci++) // set all values to -1
				QueryColIdxToQbGridRow[ci] = -1;

			if (qt.QueryColumns.Count > 0) qt = qt; // debug

			GridDt = new DataTableMx("QueryTable");
			DataTable = GridDt;
			GridDt.Columns.Add("ColumnLabel", typeof(DataFieldWithIcons));
			GridDt.Columns.Add("Selected", typeof(bool));
			GridDt.Columns.Add("Criteria", typeof(object)); // may be string or structure bitmap
			GridDt.Columns.Add("AggregationRole", typeof(string));
			GridDt.Columns.Add("SortPos", typeof(Bitmap));
			mt = qt.MetaTable;

			menuhdr = qt.ActiveLabel;
			//if (qt.Label != "") menuhdr = qt.Label; // any user-defined label?
			//else if (mt.Label != "" && (mt.Label.Length <= 60 || mt.ShortLabel == "")) 
			//	menuhdr = mt.Label; // use unless blank or long & short label exists
			//else if (mt.ShortLabel != "") menuhdr = mt.ShortLabel;
			//else menuhdr = mt.Name;

			if (ShowMqlNames) // show sql "table names"
				menuhdr += " (" + mt.Name + ")";

			TableHeaderLabel.Text = menuhdr;

			if (SelectOnly)
			{
				SearchCriteriaColumn.Visible = false;
				SortPositionColumn.Visible = false;
				AggregationColumn.Visible = false;
			}

			else
			{
				SetupAggregationHeader();
			}

			//else (always show a hyperlink since commands are always available)
			//{
			//  if (mt.DescriptionIsAvailable()) // set proper highlighting for header label
			//  {
			//    TableHeaderLabel.ForeColor = Color.Blue;
			//    TableHeaderLabel.Font = new Font(TableHeaderLabel.Font, FontStyle.Underline);
			//  }
			//}

			List<QtGridRow> gridData = new List<QtGridRow>();

			bool containsStructure = false;
			int structCriteriaRow = -1;
			QueryColumn structCriteriaQc = null; // column to send out struct for

			int itemNumber = 0;
			for (fi = 0; fi < qt.QueryColumns.Count; fi++)
			{
				qc = qt.QueryColumns[fi];
				if (qc.MetaColumn == null) continue;

				mc = qc.MetaColumn;
				if (!QueryColumnVisible(qc)) continue;

				QtGridRowToQueryColIdx[gridData.Count] = fi; // save mapping
				QueryColIdxToQbGridRow[fi] = gridData.Count;

				gr = new QtGridRow();
				gridData.Add(gr);
				itemNumber++;

				if (mc.IsKey) gr.ColumnImage = Bitmaps16x16Enum.Key;
				else gr.ColumnImage = mc.DataTypeImageIndex;
				if (mc.DataType == MetaColumnType.Structure)
					containsStructure = true;

				if (mc.InitialSelection == ColumnSelectionEnum.Comment) // no image for comments
				{
					gr.ColumnImage = Bitmaps16x16Enum.None;
					itemNumber = 0;
				}

				//if (itemNumber==0) itemNumberText="";
				//else
				//{
				//  itemNumberText = System.Convert.ToString(itemNumber) + ". ";
				//  if (itemNumber < 10) itemNumberText += "  ";
				//}

				//if (Lex.Contains(qc.ActiveLabel, "chime")) qc = qc; // debug
				//if (mc.Units != null) mc = mc; // debug

				txt = qc.ActiveLabel;
				if (txt.IndexOf("\t") >= 0) // remove anything after tab (e.g. chimestring structure label)
					txt = txt.Substring(0, txt.IndexOf("\t"));

				if (ShowMqlNames) txt += " - (" + mc.Name + ")";
				gr.ColumnLabel = txt;

				if (qc.Selected) gr.Selected = true;
				else if (mc.InitialSelection == ColumnSelectionEnum.Unselectable) gr.Selected = null;
				else gr.Selected = false;

				if (!qc.CustomFormattingExists) gr.ColumnDropdownImage = Bitmaps16x16Enum.DropdownBlack;
				else gr.ColumnDropdownImage = Bitmaps16x16Enum.DropdownGreen;

				if (mc.IsKey && QueryTable.Query != null)
					criteriaText = QueryTable.Query.KeyCriteriaDisplay;

				else
				{
					criteriaText = qc.CriteriaDisplay;
					if (criteriaText != "" && mt.RetrievesDataFromQueryEngine)
					{
						if (qc.FilterSearch && !qc.FilterRetrieval) criteriaText += " (Search only)";
						else if (!qc.FilterSearch && qc.FilterRetrieval) criteriaText += " (Retrieve only)";
						else if (!qc.FilterSearch && !qc.FilterRetrieval) criteriaText += " (Disabled)";
					}
				}


				if (criteriaText != "" && mc.DataType == MetaColumnType.Structure)
				{
					v = this.Grid.MainView as GridView;
					GridColumn gridCol = v.Columns[CriteriaColIdx];
					int pixWidth = gridCol.Width; // width to fit into
					gr.Criteria = GetQueryMoleculeBitmap(qc, pixWidth, v.Appearance.Row.Font); // store image for structure
				}

				else
				{
					if (criteriaText == "" && mc.IsSearchable) criteriaText = "Add Criteria...";
					gr.Criteria = criteriaText;
				}

				gr.AggregationImage = qc.GetAggregationImage();

				string aggLabel = qc.GetAggregationTypeLabel();
				if (qc.IsAggregationGroupBy) aggLabel = "Group by " + aggLabel;
				gr.AggregationLabel = aggLabel;

				if (qc.IsKey && query != null) qc.SortOrder = query.KeySortOrder; // be sure up to date
				gr.SortPosImage = QueryColumn.MapSortOrderToImageIndex(qc.SortOrder);

				DataRowMx dr = GridDt.NewRow();

				dr[ColNameColIdx] = new DataFieldWithIcons(gr.ColumnImage, gr.ColumnLabel, gr.ColumnDropdownImage);
				dr[SelectedColIdx] = gr.Selected;
				dr[CriteriaColIdx] = gr.Criteria;
				dr[AggregationColIdx] = new DataFieldWithIcons(gr.AggregationImage, gr.AggregationLabel);

				if (gr.SortPosImage >= 0)
					dr[SortOrderColIdx] = Bitmaps.Bitmaps16x16.Images[(int)gr.SortPosImage];

				GridDt.Rows.Add(dr);
			}

			Grid.DataSource = GridDt;

			if (!SelectOnly)
				DisplayCriteriaCount();

			CurrentQc = null; // clear currently selected column
			GridRow = -1;

			return;
		}

		/// <summary>
		/// Get the image index for the specified metacolumn
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>

		public static Image GetMetaColumnDataTypeImage(MetaColumn mc)
		{
			Bitmaps16x16Enum idx = mc.DataTypeImageIndex;
			if (mc.IsKey) idx = Bitmaps16x16Enum.Key;
			return Bitmaps.Bitmaps16x16.Images[(int)idx];
		}

		/// <summary>
		/// Return true if query column should be visible to user
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		public static bool QueryColumnVisible(
			QueryColumn qc)
		{
			QueryTable qt = qc.QueryTable;
			MetaColumn mc = qc.MetaColumn;

			if (SS.I.DebugFlags[6] != null) // debug
			{
				string msg = "QueryColumnVisible " + SS.I.UserName +
					" " + qc.MetaTableDotMetaColumnName + " " +
					qc.Selected.ToString() + " " +
					qc.Criteria.ToString() + " " +
					qt.MetaTable.UseSummarizedData.ToString() + " " +
					mc.SummarizedExists.ToString() + " " +
					mc.UnsummarizedExists.ToString() + " " +
					ShowHiddenFields.ToString() + " " +
					mc.InitialSelection.ToString();
				ClientLog.Message(msg);
			}

			if (qc.Selected || qc.Criteria != "") return true; // show if selected or criteria defined
																												 //else if (qt.Summarized && !mc.SummarizedExists) return false; // exist in this summary form?
																												 //else if (!qt.Summarized && !mc.UnsummarizedExists) return false;
			else if (ShowHiddenFields) return true; // show everything
			else if (mc.InitialSelection == ColumnSelectionEnum.Hidden) return false; // hidden field
			else return true;
		}

		/// <summary>
		/// Convert an Id to a better looking label
		/// </summary>

		public string IdToLabel(
			string id)
		{
			char pc, cc;
			int i1;

			StringBuilder label = new StringBuilder(id);
			pc = ' '; // prev char
			for (i1 = 0; i1 < label.Length; i1++)
			{
				cc = label[i1];
				if (Char.IsLetter(cc))
				{
					if (pc == ' ') label[i1] = Char.ToUpper(cc); // cap first char
					else label[i1] = Char.ToLower(cc);
				}
				else if (cc == '_')
				{ // treat underscore like space
					label[i1] = ' ';
					cc = ' ';
				}
				pc = cc; // 
			}
			return label.ToString();
		}

		/// <summary>
		/// Set text for Search Criteria column
		/// </summary>
		/// <param name="qt">Current query table</param>

		void DisplayCriteriaCount()
		{
			QueryTable qt, qt2;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			string txt;
			int ti, fi, thisCount, totCount;

			qt = QueryTable;
			Query q = qt.Query;
			if (q == null) return;

			if (q.KeyCriteriaDisplay != "") thisCount = totCount = 1; // count key criteria if exists
			else thisCount = totCount = 0;

			for (ti = 0; ti < q.Tables.Count; ti++)
			{
				qt2 = q.Tables[ti];
				mt = qt2.MetaTable;
				if (mt.IgnoreCriteria) continue;
				for (fi = 0; fi < qt2.QueryColumns.Count; fi++)
				{
					qc = qt2.QueryColumns[fi];
					if (qc.MetaColumn == null) continue;

					mc = qc.MetaColumn;
					if (mc.IsKey) continue; // don't count key
					if (qc.Criteria == "") continue;
					totCount++; // count criteria
					if (qt2 == qt) thisCount++; // also count if this table
				}
			}

			txt = "Search Criteria";
			if (totCount > 0) txt += " (" + thisCount + " of " + totCount + ")";
			SearchCriteriaColumn.Caption = txt;

			return;
		}

		/// <summary>
		/// If this column contains a DataFieldWithIcons object then do custom rendering of it
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void View_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
		{
			DataFieldWithIcons dfi = e.CellValue as DataFieldWithIcons;
			if (dfi != null)
			{
				if (dfi.ColumnLabel == "Group By") dfi = dfi; // debug

				if (dfi.ColumnImage < 0 && dfi.ColumnDropdownImage < 0) // no need for custom if no images
				{
					e.DisplayText = dfi.ColumnLabel;
					e.Handled = false;
					return;
				}

				e.DisplayText = dfi.ColumnImage + " " + dfi.ColumnLabel + " " + dfi.ColumnDropdownImage;
				int top = e.Bounds.Top + 2;
				int bot = e.Bounds.Bottom + 2;
				Point p = new Point(e.Bounds.Left, top);
				if (dfi.ColumnImage >= 0)
					e.Graphics.DrawImageUnscaled(Bitmaps.Bitmaps16x16.Images[(int)dfi.ColumnImage], p);

				Rectangle r =
					new Rectangle(e.Bounds.Left + 18, top, e.Bounds.Width - 28, e.Bounds.Height - 3);
				e.Graphics.DrawString(dfi.ColumnLabel, e.Appearance.Font, Brushes.Black, r);
				if (dfi.ColumnDropdownImage >= 0)
				{
					p = new Point(e.Bounds.Left + e.Bounds.Width - 16, top);
					e.Graphics.DrawImageUnscaled(Bitmaps.Bitmaps16x16.Images[(int)dfi.ColumnDropdownImage], p);
				}

				e.Handled = true;
				return;
			}
		}

		/// <summary>
		/// Put row number in indicator
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void View_CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
		{
			if (e.Info.IsRowIndicator)
			{
				e.Info.DisplayText = (e.RowHandle + 1).ToString();
				e.Info.ImageIndex = -1; // no image
			}
		}

		private void View_MouseDown(object sender, MouseEventArgs e)
		{
			Rectangle rect;
			string txt;
			int qtCi, ri, ci;

			if (View == null) return;
			Point p = LastGridMouseDownPosition = new Point(e.X, e.Y);
			GridHitInfo ghi = View.CalcHitInfo(p);
			if (ghi == null) return;
			ri = ghi.RowHandle;

			if (ghi.HitTest == GridHitTest.RowIndicator) // drag row indicator to reorder QueryColumns
			{
				if (ri == 0) return; // can't move key field
				if (Control.ModifierKeys != Keys.None) return;
				if (e.Button == MouseButtons.Left && ghi.InRow)
					downHitInfo = ghi;
				return;
			}

			if (ghi.Column == null || (!ghi.InColumn && !ghi.InRowCell)) // ignore if in grid "whitespace"
				return;

			ci = ghi.Column.ColumnHandle; // not AbsoluteIndex?

			GridViewInfo viewInfo = View.GetViewInfo() as GridViewInfo;

			// Click in header area

			if (ghi.InColumn)
			{
				if (SelectSingle) return;
				if (viewInfo.ColumnsInfo[ci + 1] == null) return;

				rect = viewInfo.ColumnsInfo[ci + 1].Bounds;
				p = LastGridMouseDownPosition = new Point(rect.Left, rect.Bottom);

				if (ci == ColNameColIdx) // field names
				{
					ShowColumnsContextMenu.Show(Grid, p);
				}
				else if (ci == SelectedColIdx) // selected columns 
				{
					SelectColumnsContextMenu.Show(Grid, p);
				}
				else if (ci == CriteriaColIdx) // criteria 
					CriteriaHeaderContextMenu.Show(Grid, p);

				else if (ci == SortOrderColIdx) // sorting
					SortHeaderContextMenu.Show(Grid, p);

				else if (ci == AggregationColIdx)
				{
					if (e.Button == MouseButtons.Left)
						AggregationHeaderLeftClicked();
					else AggregationHeaderRightClicked(p);
				}
			}

			// Click on item below headers

			else
			{
				GridCellInfo gci = viewInfo.GetGridCellInfo(ghi);
				if (gci == null) return;
				ri = ghi.RowHandle;
				qtCi = QtGridRowToQueryColIdx[ri];
				QueryColumn qc = QueryTable.QueryColumns[qtCi];
				CurrentQc = qc;
				GridRow = ri;
				GridCol = ci;

				if (e.Button == MouseButtons.Left)
				{
					if (ci == ColNameColIdx) // field names
					{
						SetupColumnFormattingContextMenu(ColumnFormattingContextMenu, qc, UseNamedCfMenuItem_Click);
						ColumnFormattingContextMenu.Show(Grid, p);
					}

					else if (ci == SelectedColIdx) // item selected/deselected
					{
						GridView v = this.Grid.MainView as GridView;
						GridColumn gc = v.Columns[ci];

						object cellVal = v.GetRowCellValue(ri, gc);
						if (cellVal == null || !(cellVal is bool)) // unselectable column
						{
							DelayedCallback.Schedule( // schedule reset of value to null for a bit later
								delegate (object state) { v.SetRowCellValue(ri, gc, null); });
							return;
						}

						bool selected = (bool)cellVal;
						selected = !selected; // switch state

						//if (SelectSingle) // be sure just a single col is selected
						//{
						//  qc.Selected = selected;
						//  if (!selected) return;

						//  for (int ri2 = 0; ri2 < GridDt.Rows.Count; ri2++)
						//  {
						//    if (ri != ri2 && (bool)v.GetRowCellValue(ri2, gc) == true)
						//      v.SetRowCellValue(ri2, gc, false);
						//  }

						//  foreach (QueryColumn qc0 in Qt.QueryColumns)
						//  {
						//    if (qc0 != qc) qc0.Selected = false;
						//  }
						//}

						if (!selected && ri == 0 && !SelectSingle && !CanDeselectKeyCol) // only allow deselect of key col for certain conditions
						{
							v.SetRowCellValue(ri, gc, true);
							XtraMessageBox.Show("The \"key\" column cannot be deselected",
								UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							return;
						}

						else qc.Selected = selected; // switch qc value
					}

					else if (ci == CriteriaColIdx) // edit criteria handled in mouse up so the mouse-up event isn't lost
						EditCriteriaOnMouseUp = true;

					else if (ci == AggregationColIdx)
						AggregationDefMenus.ShowTypeMenu(qc, qc.GetAggregation(), Grid.PointToScreen(p), AggregationTypeChanged);

					else if (ci == SortOrderColIdx)
						SortOrderContextMenu.Show(Grid, p);
				}

				if (e.Button == MouseButtons.Right)
				{
					if (ci == ColNameColIdx) // field names
					{
						SetupColumnFormattingContextMenu(ColumnFormattingContextMenu, qc, UseNamedCfMenuItem_Click);
						ColumnFormattingContextMenu.Show(Grid, p);
					}
					else if (ci == SelectedColIdx) { } // item selected/deselected
					else if (ci == CriteriaColIdx) // criteria col
					{
						menuApplyCriteriaToSearch.Checked = qc.FilterSearch;
						menuApplyCriteriaToRetrieval.Checked = qc.FilterRetrieval;
						menuShowCriteriaInSum.Checked = qc.ShowOnCriteriaForm;

						bool enabled = true;
						if (ri == 0) enabled = false; // don't allow change of filter state for key field
						menuApplyCriteriaToSearch.Enabled = enabled;
						menuApplyCriteriaToRetrieval.Enabled = enabled;

						CriteriaColRtClickContextMenu.Show(Grid, p);
					}

					else if (ci == AggregationColIdx)
						AggregationDefMenus.ShowTypeMenu(qc, qc.GetAggregation(), Grid.PointToScreen(p), AggregationTypeChanged);

					else if (ci == SortOrderColIdx) // sort order
						SortOrderContextMenu.Show(Grid, p);
				}
			}

		}

		/// <summary>
		/// Mouse up event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void View_MouseUp(object sender, MouseEventArgs e)
		{
			if (EditCriteriaOnMouseUp)
			{
				EditCriteriaOnMouseUp = false;
				EditCriteria();
				return;
			}
		}

		/// <summary>
		/// Edit currently selected criteria
		/// </summary>

		public void EditCriteria()
		{
			string txt;

			QueryColumn qc = CurrentQc;
			if (qc == null) return;

			if (Qt.Query != null && Qt.Query.LogicType == QueryLogicType.Complex)
			{
				MessageBoxMx.ShowError(
					"Since Advanced Criteria Logic has been selected\n" +
					"for this query you must use the advanced criteria\n" +
					"text box on the Criteria Summary tab to edit criteria.");
				return;
			}

			if (!CriteriaEditor.EditCriteria(qc)) return;

			// Render the full table

			bool renderFullTable = true;
			if (renderFullTable)
			{
				Render();
				return;
			}

			// Render just the specific edited cell

			GridView v = this.Grid.MainView as GridView;
			GridColumn gc = v.Columns[GridCol];
			if (qc.CriteriaDisplay != "")
			{
				if (qc.MetaColumn.DataType == MetaColumnType.Structure)
				{
					v = this.Grid.MainView as GridView;
					GridColumn gridCol = v.Columns[CriteriaColIdx];
					int pixWidth = gridCol.Width; // width to fit into
					Bitmap bm = GetQueryMoleculeBitmap(qc, pixWidth, v.Appearance.Row.Font);
					v.SetRowCellValue(GridRow, gc, bm);
				}

				else // simple text value
				{
					txt = qc.CriteriaDisplay;
					txt = txt.Replace("<>", UnicodeString.NotEqual); // use single unicode char for these
					txt = txt.Replace("<=", UnicodeString.LessOrEqual);
					txt = txt.Replace(">=", UnicodeString.GreaterOrEqual);
					v.SetRowCellValue(GridRow, gc, txt);
				}
			}
			else
			{
				txt = "Add Criteria...";
				v.SetRowCellValue(GridRow, gc, txt);
			}
		}

		/// <summary>
		/// Set the image for a query structure
		/// </summary>
		/// <param name="ri"></param>
		/// <param name="ci"></param>
		/// <param name="qc"></param>

		internal static Bitmap GetQueryMoleculeBitmap(
			QueryColumn qc,
			int pixWidth,
			Font font)
		{
			Rectangle boundingRect;
			Bitmap bm = null;

			MoleculeMx mol = new MoleculeMx(qc.MolString);

			if (mol.IsChemStructureFormat)
			{
				int miWidth = MoleculeMx.PixelsToMilliinches(pixWidth);

				Rectangle destRect = new Rectangle(0, 0, miWidth, 10000);
				CdkMx.CdkMol molLib1Mol = mol.FitStructureIntoRectangle(ref destRect, MoleculeMx.StandardBondLength, 0, 0, false, 11000, out boundingRect);
				int pixHeight = MoleculeMx.MilliinchesToPixels(destRect.Height);
				DisplayPreferences dp = mol.GetDisplayPreferences();
				if (Lex.Contains(qc.Criteria, "SSS")) // no H display if SS query
					dp.HydrogenDisplayMode = HydrogenDisplayMode.Off;
				bm = molLib1Mol.GetMoleculeBitmap(pixWidth, pixHeight + 20, dp); // get image with a bit of extra height for text

				Rectangle r = new Rectangle(2, 0, pixWidth, pixHeight); // show the text part
				Graphics g = Graphics.FromImage(bm);
				g.DrawString(qc.CriteriaDisplay, font, Brushes.Black, r);
				return bm;
			}

			else if (mol.IsBiopolymerFormat)
			{
				bm = HelmConverter.HelmToBitmap(mol, pixWidth);
				return bm;
			}

			else return new Bitmap(1, 1);

		}

		/// <summary>
		/// Initialize a column formatting context menu including named conditional formatting menu items
		/// </summary>
		/// <param name="menu"></param>

		internal static void SetupColumnFormattingContextMenu(
			ContextMenuStrip menu,
			QueryColumn qc,
			EventHandler useNamedCfMenuItemEventHandler)
		{
			ToolStripItem mi;
			int i1, i2;

			for (i1 = 0; i1 < menu.Items.Count; i1++)
			{ // enable/disable custom formatting menu item
				mi = menu.Items[i1];
				if (Lex.Eq(mi.Text, "Remove Custom Formatting..."))
				{
					mi.Enabled = qc.CustomFormattingExists;
					break;
				}
			}

			for (i1 = 0; i1 < menu.Items.Count; i1++) // find location of CF item
			{
				mi = menu.Items[i1];
				if (Lex.Eq(mi.Text, "Conditional Formatting...")) break;
			}
			if (i1 >= menu.Items.Count) return;

			while (i1 + 1 < menu.Items.Count) // remove items up to divider
			{
				mi = menu.Items[i1 + 1];
				if (mi is ToolStripSeparator) break;
				menu.Items.RemoveAt(i1 + 1);
			}

			if (qc != null)
			{
				QueryTable qt = qc.QueryTable;

				Query q = qt.Query;
				if (q == null) // build dummy query to hold query table
				{
					q = new Query();
					q.AddQueryTable(qt);
				}

				SortedDictionary<string, CondFormat> cfList = q.GetSortedCondFormats();

				string thisCfName = "";
				if (qc.CondFormat != null) thisCfName = qc.CondFormat.Name;
				MetaColumnType mcType = qc.MetaColumn.DataType;
				foreach (CondFormat cf2 in cfList.Values)
				{
					if (cf2.Name == thisCfName) continue; // don't add self
					if (MetaColumn.AreCompatibleMetaColumnTypes(mcType, cf2.ColumnType)) // must be compatible type
					{
						mi = new ToolStripMenuItem();
						mi.Text = "Use \"" + cf2.Name + "\" Conditional Formatting...";
						mi.Tag = cf2.Name;
						mi.Click += useNamedCfMenuItemEventHandler;
						menu.Items.Insert(i1 + 1, mi);
						i1++;
					}
				}
			}
		}

		private void ShowMqlNamesMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableShowMqlNames");
			ShowMqlNames = !ShowMqlNames;
			Render();
		}

		private void ShowHiddenFieldsMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableShowHiddenFields");
			ShowHiddenFields = !ShowHiddenFields;
			Render();
		}

		private void RemoveAllTableCriteriaMenuItem2_Click(object sender, EventArgs e)
		{
			RemoveAllTableCriteriaMenuItem_Click(sender, e);
		}

		private void SelectAllItems_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableSelectAllItems");
			QueryTable qt = QueryTable;

			for (int fi = 0; fi < qt.QueryColumns.Count; fi++)
			{
				QueryColumn qc = qt.QueryColumns[fi];
				if (!QueryColumnVisible(qc)) continue; // don't select if not currently visible to user
				if (qc.MetaColumn.InitialSelection == ColumnSelectionEnum.Unselectable) continue;
				qc.Selected = true;
			}
			Render();
		}

		private void DeselectAllItems_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableDeselectAllItems");
			QueryTable qt = QueryTable;

			for (int fi = 0; fi < qt.QueryColumns.Count; fi++)
			{
				QueryColumn qc = qt.QueryColumns[fi];
				if (qc.MetaColumn.InitialSelection == ColumnSelectionEnum.Unselectable) continue;

				if (qc.IsKey)
					qc.Selected = true;
				else qc.Selected = false;
			}
			Render();
		}

		private void SelectDefaultItems_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableSelectDefaultItems");

			QueryTable qt = QueryTable;

			for (int fi = 0; fi < qt.QueryColumns.Count; fi++)
			{
				QueryColumn qc = qt.QueryColumns[fi];
				if (qc.MetaColumn == null) continue;

				MetaColumn mc = qc.MetaColumn;
				if (mc.InitialSelection == ColumnSelectionEnum.Selected)
					qc.Selected = true;
				else qc.Selected = false;
			}
			Render();
		}

		private void RunQueryPreviewMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTablePreview");

			CommandExec.ExecuteCommandAsynch("RunQuerySingleTablePreview " + Qt.MetaTable.Name);
		}

		private void RunQuerySingleTableMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableRunQuerySingleTable");
			CommandExec.ExecuteCommandAsynch("RunQuerySingleTable " + Qt.MetaTable.Name);
		}

		private void ShowDescMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableShowDescription");
			CommandExec.ExecuteCommandAsynch("ShowTableDescription " + Qt.MetaTable.Name);
		}

		private void RetrieveSummarizedMenuItem_Click(object sender, EventArgs e)
		{
			Query q = Qt.Query;
			int pos = q.Tables.IndexOf(Qt);
			if (pos < 0) throw new Exception("QueryTable not found in query");

			QueryTable qt2 = Qt.AdjustSummarizationLevel(!Qt.MetaTable.UseSummarizedData); // flip summarization level
			qt2.Query = q;
			q.Tables[pos] = qt2; // plug in new QueryTable
			Qt = qt2;
			q.CurrentTable = Qt; // set as new current table as well

			Render();

			if (this.Parent is XtraTabPage) // if parent is tab page then set its text to the new label also
				(this.Parent as XtraTabPage).Text = Qt.ActiveLabel;

			return;
		}

		private void CustomSummariationMenuItem_Click(object sender, EventArgs e)
		{
			Qt.AggregationEnabled = !Qt.AggregationEnabled;
			if (Qt.AggregationEnabled && Qt.AggregatedCount == 0) // define default values if nothing defined so far
				SetDefaultAggregationTypes();

			Render();
			return;
		}

		private void EditDefinitionMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableEditDefinition");

			MetaTable mt = Qt.MetaTable;

			if (mt.IsAnnotationTable)
			{
				CommandExec.ExecuteCommandAsynch("ContentsOpenAnnotation Annotation " + mt.Name);
				Qt.RemapToMetaTable(); // associate new metatable
				Render(); // render it
			}

			else if (mt.IsCalculatedField)
			{
				CommandExec.ExecuteCommandAsynch("ContentsOpenCalcField CalcField " + mt.Name);
			}

			else if (mt.IsSpotfireLink)
			{
				CommandExec.ExecuteCommandAsynch("EditSpotfireLink " + mt.Name);
				mt = MetaTableCollection.Get(mt.Name); // get updated metatable
				if (mt != null && Qt != null && Lex.Eq(Qt.MetaTable.Name, mt.Name))
				{
					Render(); // show new form
				}
			}

			else return; // shouldn't happen
		}

		private void DuplicateTableMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("DuplicateQueryTable");
			QbUtil.DuplicateQueryTable(Qt);
			return;
		}

		private void RemoveAllTableCriteriaMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableRemoveAllCriteria");

			if (Qt.GetCriteriaCount(includeKey: true, includeDbSet: true) > 1)
			{
				DialogResult dr = MessageBoxMx.Show(
					"Are you sure you want to clear all criteria in the current table?", UmlautMobius.String,
					MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (dr != DialogResult.Yes) return;
			}

			foreach (QueryColumn qc2 in Qt.QueryColumns) // remove all criteria for this table
				qc2.CriteriaDisplay = qc2.Criteria = "";

			Qt.Query.KeyCriteriaDisplay = Qt.Query.KeyCriteria = "";
			Render();
		}

		private void RenameTableMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableRename");

			DialogResult dr = QueryTableRenameDialog_Show(Qt);
			if (dr == DialogResult.Cancel) return;

			TableHeaderLabel.Text = Qt.ActiveLabel;
			if (this.Parent is XtraTabPage) // if parent is tab page then set its text to the new label also
				(this.Parent as XtraTabPage).Text = Qt.ActiveLabel;

			return;
		}

		/// <summary>
		/// Prompt user to rename query Table
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		public static DialogResult QueryTableRenameDialog_Show(
			QueryTable qt)
		{
			string originalLabel = qt.ActiveLabel;
			string newLabel = InputBoxMx.Show("Enter new name (Clear to restore original name)", "Rename Table", originalLabel);

			if (newLabel == null) return DialogResult.Cancel;
			else if (newLabel.Trim() != "") // new name entered
				qt.Label = newLabel;
			else // restore old name
			{
				qt.Label = ""; // use original label from metacolumn
				newLabel = qt.MetaTable.Label;
			}

			return DialogResult.OK;
		}

		private void TableHeaderLabel_MouseDown(object sender, MouseEventArgs e)
		{
			ShowTableLabelDropdown(TableHeaderLabel);
		}

		/// <summary>
		/// Show a dropdown for a table label (static method) 
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="control"></param>

		internal static void ShowTableLabelDropdown(QueryTable qt, Control control)
		{
			QueryTableControl qtc = new QueryTableControl();
			qtc.QueryTable = qt;
			qtc.ShowTableLabelDropdown(control);
		}

		/// <summary>
		/// Show a dropdown for a table label
		/// </summary>
		/// <param name="control"></param>

		internal void ShowTableLabelDropdown(Control control)
		{
			MetaTable mt = QueryTable.MetaTable;
			ShowDescMenuItem.Enabled = !String.IsNullOrEmpty(mt.Description);

			RetrieveSummarizedMenuItem.Visible = mt.SummarizedExists && (this.Parent != null); // visible only if summarized data and parent control is available
			RetrieveSummarizedMenuItem.Checked = mt.UseSummarizedData;

			CustomSummariationMenuItem.Visible = !mt.IsRootTable && Aggregator.IsAvailableForUser;
			CustomSummariationMenuItem.Checked = QueryTable.AggregationEnabled;

			if (Lex.StartsWith(mt.Name, "annotation_") ||
			 Lex.StartsWith(mt.Name, "calcfield_") ||
			 Lex.StartsWith(mt.Name, "spotfirelink_"))
				EditDefinitionMenuItem.Visible = true;
			else EditDefinitionMenuItem.Visible = false;

			RenameTableMenuItem.Enabled = (this.Parent != null); // enable rename only if parent exists

			QbTableLabelContextMenu.Show(control,
				new System.Drawing.Point(0, control.Height));
			return;
		}

		/// <summary>
		/// Define conditional formatting
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void CondFormatMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableColumnEditConditionalFormatting");

			QueryColumn qc = CurrentQc;
			CondFormatEditor.Edit(qc);
			SetFieldDropdownImage(qc, GridRow);
			this.Refresh();
		}

		/// <summary>
		/// Define cond formatting based on existing formatting
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void UseNamedCfMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableColumnUseNamedCf");

			QueryColumn qc = CurrentQc;
			ToolStripMenuItem mi = (ToolStripMenuItem)sender;
			string cfName = mi.Tag.ToString();

			CondFormat cf = qc.QueryTable.Query.GetCondFormatByName(cfName);
			CondFormat oldCf = qc.CondFormat; // save old format
			qc.CondFormat = cf; // tentatively plug in new format
			if (CondFormatEditor.Edit(qc) != DialogResult.OK) // restore prev cf if cancelled
				qc.CondFormat = oldCf;

			SetFieldDropdownImage(qc, GridRow);
			this.Refresh();
		}

		private void menuColWidthFieldCol_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableColumnWidth");

			QueryColumn qc = CurrentQc;
			QueryColumnWidthDialog.Show(qc);
			SetFieldDropdownImage(qc, GridRow);
			this.Refresh();
		}

		/// <summary>
		/// Set the dropdown image for a field
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="ri"></param>

		void SetFieldDropdownImage(
			QueryColumn qc,
			int ri)
		{
			DataRowMx dr = DataTable.Rows[ri];
			DataFieldWithIcons dfi = dr[ColNameColIdx] as DataFieldWithIcons;

			if (!qc.CustomFormattingExists) dfi.ColumnDropdownImage = Bitmaps16x16Enum.DropdownBlack;
			else dfi.ColumnDropdownImage = Bitmaps16x16Enum.DropdownGreen;
		}

		private void menuNumberFormatFieldCol_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableColumnNumberFormat");

			QueryColumn qc = CurrentQc;
			ColumnFormatDialog.Show(qc);
			SetFieldDropdownImage(qc, GridRow);
			this.Refresh();
		}

		private void TextAlignmentMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableColumnTextAlignment");

			QueryColumn qc = CurrentQc;
			TextAlignmentDialog.Show(qc);
			SetFieldDropdownImage(qc, GridRow);
			this.Refresh();
		}

		private void RemoveCustomFormattingMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableColumnRemoveCustomFormatting");

			QueryColumn qc = CurrentQc;

			if (!qc.CustomFormattingExists) return;

			DialogResult dr = MessageBoxMx.Show(
				"Are you sure you want to clear all custom formatting for the " + qc.ActiveLabel + " column?",
				"Remove Formatting",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (dr != DialogResult.Yes) return;

			qc.ResetFormatting();
			SetFieldDropdownImage(qc, GridRow);
			this.Refresh();
		}

		private void menuTableHeaderBackgroundColorFieldCol_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableHeaderBackgroundColor");

			TableHeaderBackgroundColorDialog_Show(Qt);
		}

		private void menuRenameFieldCol_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableColumnRename");

			QueryColumn qc = CurrentQc;
			if (QueryColumnRenameDialog_Show(qc) == DialogResult.Cancel) return;

			DataRowMx dr = DataTable.Rows[GridRow];
			DataFieldWithIcons dfi = dr[ColNameColIdx] as DataFieldWithIcons;
			dfi.ColumnLabel = qc.ActiveLabel;
			SetFieldDropdownImage(qc, GridRow);
			this.Refresh();
		}

		private void menuRenameFieldColAllUsers_Click(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		private void menuEditCriteria_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableCriteriaEdit");

			EditCriteria();
		}

		private void menuRemoveCriteria_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableCriteriaRemove");
			QueryColumn qc = CurrentQc;

			qc.CriteriaDisplay = qc.Criteria = "";

			if (qc.IsKey)
				Qt.Query.KeyCriteriaDisplay = Qt.Query.KeyCriteria = "";

			Render();
		}

		private void menuApplyCriteriaToSearch_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableCriteriaApplyToSearch");
			CurrentQc.FilterSearch = !CurrentQc.FilterSearch;
			Render();
		}

		private void menuApplyCriteriaToRetrieval_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableCriteriaApplyToRetrieval");
			CurrentQc.FilterRetrieval = !CurrentQc.FilterRetrieval;
			Render();
		}

		private void menuShowCriteriaInSum_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableCriteriaShowInCriteriaSummary");
			CurrentQc.ShowOnCriteriaForm = !CurrentQc.ShowOnCriteriaForm;
		}

		private void menuRemoveAllCriteria_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableCriteriaRemoveAll");

			DialogResult dr = MessageBoxMx.Show(
				"Are you sure you want to clear all criteria in the current query?", UmlautMobius.String,
				MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			if (dr != DialogResult.Yes) return;

			foreach (QueryTable qt2 in Qt.Query.Tables)
			{
				foreach (QueryColumn qc2 in qt2.QueryColumns)
					qc2.CriteriaDisplay = qc2.Criteria = "";
			}

			Qt.Query.KeyCriteriaDisplay = Qt.Query.KeyCriteria = "";

			Render();
		}

		/// <summary>
		/// Prompt user for QueryTable background header color
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		public static DialogResult TableHeaderBackgroundColorDialog_Show(
				QueryTable qt)
		{
			Color c = qt.HeaderBackgroundColor;
			if (c.Equals(Color.Empty))
				c = Color.White; // todo get real back color for header

			ColorDialog cd = new ColorDialog();
			cd.AllowFullOpen = true;

			Color defaultHeaderColor = Color.White; // TODO: get real default header cell color

			if (c.Equals(Color.Empty))
				c = defaultHeaderColor;

			//if (customColor!=0) // custom color supplied?
			//{
			//  c = Color.FromArgb(customColor);
			//  customColor = c.B*65536 + c.G*256 + c.R; // proper form for custom colors
			//  cd.CustomColors = new int[]{ customColor };
			//}

			cd.Color = c;
			DialogResult dr = cd.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.OK)
			{
				c = cd.Color;
				if (c.Equals(defaultHeaderColor)) // if still default header color then set to null
					c = Color.Empty;

				qt.HeaderBackgroundColor = c;
			}
			return dr;
		}

		/// <summary>
		/// Prompt user to rename query column
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		public static DialogResult QueryColumnRenameDialog_Show(
			QueryColumn qc)
		{
			string originalLabel = qc.ActiveLabel;
			string newLabel = InputBoxMx.Show("Enter new name (Clear to restore original name)", "Rename Column", originalLabel);

			if (newLabel == null) return DialogResult.Cancel;
			else if (newLabel.Trim() != "") // new name entered
				qc.Label = newLabel;
			else // restore old name
			{
				qc.Label = ""; // use original label from metacolumn
				newLabel = qc.MetaColumn.Label;
			}

			return DialogResult.OK;
		}

		///////////////////////////////////////
		//// Aggregation Header Operations ////
		///////////////////////////////////////

		/// <summary>
		/// AggregationHeaderLeftClicked
		/// </summary>
		void AggregationHeaderLeftClicked()
		{
			Qt.AggregationEnabled = !Qt.AggregationEnabled;
			//SetupAggregationHeader();

			if (Qt.AggregationEnabled && Qt.AggregatedCount == 0)
			{
				SetDefaultAggregationTypes();
			}

			if (this.Parent is XtraTabPage) // if parent is tab page then set its text to the new label also
				(this.Parent as XtraTabPage).Text = Qt.ActiveLabel;

			Render();

			return;
		}

		/// <summary>
		/// SetupAggregationHeader
		/// </summary>

		void SetupAggregationHeader()
		{
			if (!Qt.AggregationEnabled)
			{
				AggregationColumn.Caption = "Aggregate";
				AggregationColumn.ImageIndex = 2;
			}

			else
			{
				AggregationColumn.Caption = "Aggregation Role";
				AggregationColumn.ImageIndex = 3;
			}

			return;
		}

		/// <summary>
		/// AggregationHeaderRightClicked
		/// </summary>
		/// <param name="p"></param>

		void AggregationHeaderRightClicked(
			Point p)
		{
			ContextMenuStrip menu = SetupAggregationHeaderContextMenu();
			menu.Show(Grid, p);
			return;
		}

		/// <summary>
		/// SetupAggregationHeaderContextMenu
		/// </summary>
		/// <returns></returns>
		ContextMenuStrip SetupAggregationHeaderContextMenu()
		{
			AggregationEnabledMenuItem.Checked = Qt.AggregationEnabled;

			return AggregationHeaderContextMenu;
		}

		private void AggregationEnabledMenuItem_Click(object sender, EventArgs e)
		{
			Qt.AggregationEnabled = !Qt.AggregationEnabled;
			SetupAggregationHeader();
			if (Qt.AggregationEnabled && Qt.AggregatedCount == 0)
			{
				SetDefaultAggregationTypes();
				Render();
			}

			return;
		}

		private void SelectDefaultAggregationMenuItem_Click(object sender, EventArgs e)
		{
			SetDefaultAggregationTypes(selectedColumnsOnly: true, keepExistingAggregationSettings: false);
			Render();
			return;
		}

		private void RemoveAllAggregationMenuItem_Click(object sender, EventArgs e)
		{
			if (Qt.AggregatedCount == 0) // if no cols aggregated then set Aggregate to false
				Qt.AggregationEnabled = false;

			else // otherwise just clear aggreation types and leave Aggregate set as is
				foreach (QueryColumn qc in Qt.QueryColumns)
				{
					qc.Aggregation = null;
				}

			Render();
			return;
		}

		/// <summary>
		/// Process aggregation item click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void AggregationTypeChanged(AggregationDefMenus ats)
		{
			if (!Qt.AggregationEnabled) // turn on aggregation if not on
			{
				Qt.AggregationEnabled = true;
				SetupAggregationHeader();
			}

			string aggLabel = CurrentQc.GetAggregationTypeLabel();
			if (CurrentQc.IsAggregationGroupBy) aggLabel = "Group by " + aggLabel;

			GridDt.Rows[GridRow][AggregationColIdx] = new DataFieldWithIcons(CurrentQc.GetAggregationImage(), aggLabel);
			View.FocusedColumn = DataFieldColumn; // move focus to cause cell to redraw

			return;
		}


		/// <summary>
		/// Initialize aggregation type based on data type
		/// </summary>
		/// <param name="selectedColumnsOnly"></param>

		void SetDefaultAggregationTypes(
			bool selectedColumnsOnly = true,
			bool keepExistingAggregationSettings = true)
		{
			QueryColumn qc;
			int qci;

			for (qci = 0; qci < Qt.QueryColumns.Count; qci++)
			{
				qc = Qt.QueryColumns[qci];

				if (selectedColumnsOnly && !qc.Selected) continue;

				if (qc.Aggregation != null && keepExistingAggregationSettings) continue;

				SetDefaultAggregationType(qc);
			}

			return;
		}

		/// <summary>
		/// Set default aggregation type which may be either a grouping or summary type
		/// </summary>
		/// <param name="qc"></param>

		void SetDefaultAggregationType(QueryColumn qc)
		{
			MetaColumn mc = qc.MetaColumn;
			MetaColumnType dt = mc.DataType;

			if (mc.IsKey)
			{
				qc.Aggregation = new AggregationDef(GroupingTypeEnum.EqualValues);
			}

			else if (dt == MetaColumnType.String)
				qc.Aggregation = new AggregationDef(GroupingTypeEnum.EqualValues);

			else if (dt == MetaColumnType.Date)
			{
				qc.Aggregation = new AggregationDef(GroupingTypeEnum.Date);
			}

			else if (mc.Concentration) // group on concentrations
				qc.Aggregation = new AggregationDef(GroupingTypeEnum.EqualValues);

			else if (mc.IsNumeric)
			{
				if (UnpivotedAssayResult.IsSpAndCrcUnpivotedAssayResultColumn(mc)) // col contains both SP and CRC values
					qc.Aggregation = new AggregationDef(SummaryTypeEnum.ResultMean);

				else if (mc.MultiPoint)
					qc.Aggregation = new AggregationDef(SummaryTypeEnum.GeometricMean);

				else qc.Aggregation = new AggregationDef(SummaryTypeEnum.ArithmeticMean);
			}

			else qc.Aggregation = new AggregationDef(SummaryTypeEnum.Count); // default

			return;
		}

		/////////////////////////////////// 
		///	SortOrderContextMenu Clicks ///
		/////////////////////////////////// 

		private void SortAsc1MenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableSetSortOrderAsc1");
			SetSortOrder(1);
		}

		private void SortAsc2MenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableSetSortOrderAsc2");
			SetSortOrder(2);
		}

		private void SortAsc3MenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableSetSortOrderAsc3");
			SetSortOrder(3);
		}

		private void SortDesc1MenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableSetSortOrderDesc1");
			SetSortOrder(-1);
		}

		private void SortDesc2MenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableSetSortOrderDesc2");
			SetSortOrder(-2);
		}

		private void SortDesc3MenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableSetSortOrderDesc3");
			SetSortOrder(-3);
		}

		private void SortNoneMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableSetSortOrderNone");
			SetSortOrder(0);
		}

		private void SortClearAllMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableSetSortOrderClearAll");

			DialogResult dr = MessageBoxMx.Show(
				"Are you sure you want to clear all sorting criteria in the current query?", UmlautMobius.String,
				MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			if (dr != DialogResult.Yes) return;

			Qt.Query.ClearSorting();

			Render();
		}

		/// <summary>
		/// Clear sorting on this table only
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void RemoveAllSortingForTableMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTableRemoveAllSortingForTable");

			for (int ci = 0; ci < Qt.QueryColumns.Count; ci++)
			{
				Qt.QueryColumns[ci].SortOrder = 0;
			}

			Qt.Query.KeySortOrder = 0;

			Render();
		}


		/// <summary>
		/// Update data & UI for change in sort order
		/// </summary>
		/// <param name="sortOrder"></param>

		void SetSortOrder(
			int sortOrder)
		{
			int gridRow = GridRow;
			int qci = QtGridRowToQueryColIdx[gridRow];
			QueryColumn qc = Qt.QueryColumns[qci];
			if (qc.MetaColumn == null) return;

			if (qc.MetaColumn.DataType == MetaColumnType.Structure ||
				qc.MetaColumn.DataType == MetaColumnType.Image)
			{
				MessageBoxMx.ShowError("Sorting is not supported for this column");
				return;
			}

			Query Query = Qt.Query;
			if (qc.MetaColumn.DataType == MetaColumnType.CompoundId && qc.IsKey &&
			 SS.I.AllowGroupingBySalts && Qt.MetaTable.SupportsKeyGrouping && sortOrder != 0)
			{
				string msg = "Do you want to group the salts of the same parent structure together?";
				if (Query.GroupSalts) msg += " (currently Enabled)";
				else msg += " (currently Disabled)";

				DialogResult dr = MessageBoxMx.Show(msg, "Salt Grouping", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (dr == DialogResult.Cancel) return;
				else if (dr == DialogResult.Yes) Query.GroupSalts = true;
				else Query.GroupSalts = false;
			}

			if (sortOrder != 0)
			{ // see if any other col has this order & clear if so
				for (int ci = 0; ci < Qt.QueryColumns.Count; ci++)
				{
					QueryColumn qc2 = Qt.QueryColumns[ci];
					if (qc2.MetaColumn == null) continue;

					if (QueryColIdxToQbGridRow[ci] < 0) continue; // not current displayed
					if (Math.Abs(qc2.SortOrder) != Math.Abs(sortOrder)) continue;

					qc2.SortOrder = 0;
					if (qc2.IsKey) Query.KeySortOrder = 0;
					int ri = QueryColIdxToQbGridRow[ci];
					GridDt.Rows[ri][SortOrderColIdx] = null;
				}

				Bitmaps16x16Enum imageIndex = QueryColumn.MapSortOrderToImageIndex(sortOrder);
				GridDt.Rows[gridRow][SortOrderColIdx] = Bitmaps.Bitmaps16x16.Images[(int)imageIndex];
			}

			else GridDt.Rows[gridRow][SortOrderColIdx] = null; // clearing

			qc.SortOrder = sortOrder;
			if (qc.IsKey) Query.KeySortOrder = sortOrder; // also key order

			View.FocusedColumn = DataFieldColumn; // move focus to cause sort cell to redraw

			return;
		}

		/// <summary>
		/// Handle rendering of structures stored as images
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void View_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
		{
			if (e.Column.ColumnHandle == CriteriaColIdx && e.CellValue is Bitmap) // structure image?
			{
				if (StructureViewerControl == null)
				{
					StructureViewerControl = new RepositoryItemPictureEdit();
					e.Column.View.GridControl.RepositoryItems.Add(StructureViewerControl);
				}

				e.RepositoryItem = StructureViewerControl;
			}

		}

		/// <summary>
		/// If in SelectSingle be sure only one item is selected
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void RepositoryItemCheckEdit_EditValueChanged(object sender, EventArgs e)
		{
			if (SelectSingle && CurrentQc.Selected) // be sure just a single col is selected
			{
				GridView v = this.Grid.MainView as GridView;
				TableHeaderLabel.Focus(); // move focus away to close editor
				Grid.Focus(); // then bring back
				GridColumn gc = v.Columns[GridCol];

				for (int ri2 = 0; ri2 < GridDt.Rows.Count; ri2++)
				{ // clear other values in grid
					if (GridRow != ri2 && (bool)v.GetRowCellValue(ri2, gc) == true)
						v.SetRowCellValue(ri2, gc, false);
				}

				foreach (QueryColumn qc0 in Qt.QueryColumns)
				{ // clear other QueryColumns
					if (qc0 != CurrentQc) qc0.Selected = false;
				}

				return;
			}

		}

		/// <summary>
		/// Handle mouse move for dragging rows
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void View_MouseMove(object sender, MouseEventArgs e)
		{
			GridView view = sender as GridView;
			if (e.Button == MouseButtons.Left && downHitInfo != null)
			{
				Size dragSize = SystemInformation.DragSize;
				Rectangle dragRect = new Rectangle(new Point(downHitInfo.HitPoint.X - dragSize.Width / 2,
						downHitInfo.HitPoint.Y - dragSize.Height / 2), dragSize);

				if (!dragRect.Contains(new Point(e.X, e.Y)))
				{
					view.GridControl.DoDragDrop(downHitInfo, DragDropEffects.All);
					downHitInfo = null;
				}
			}
		}

		/// <summary>
		/// Handle DragOver event for dragging rows
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Grid_DragOver(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.None;

			if (downHitInfo == null) return;

			GridControl grid = sender as GridControl;
			GridView view = grid.MainView as GridView;
			GridHitInfo hitInfo = view.CalcHitInfo(grid.PointToClient(new Point(e.X, e.Y)));
			if (hitInfo.InRow && hitInfo.RowHandle != 0 && hitInfo.RowHandle != downHitInfo.RowHandle) // moving to row other than start row or first (key) row?
				e.Effect = DragDropEffects.Move;
		}

		/// <summary>
		/// Complete drag of row
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Grid_DragDrop(object sender, DragEventArgs e)
		{
			GridControl grid = sender as GridControl;
			GridView view = grid.MainView as GridView;
			GridHitInfo hitInfo = view.CalcHitInfo(grid.PointToClient(new Point(e.X, e.Y)));
			int qci1 = QtGridRowToQueryColIdx[downHitInfo.RowHandle];
			int qci2 = QtGridRowToQueryColIdx[hitInfo.RowHandle];
			QueryColumn qc = Qt.QueryColumns[qci1];
			Qt.QueryColumns.RemoveAt(qci1);
			//if (qci1 < qci2) qci2--;
			Qt.QueryColumns.Insert(qci2, qc);
			Render();
			return;
		}

	} // QueryTableControl

	/// <summary>
	/// Prompt user for QueryColumn width
	/// </summary>

	public class QueryColumnWidthDialog
	{
		/// <summary>
		/// Show
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		public static DialogResult Show(
			QueryColumn qc)
		{
			float width;

			MetaColumn mc = qc.MetaColumn;
			while (true)
			{
				if (qc.DisplayWidth > 0) width = qc.DisplayWidth;
				else width = mc.Width;
				string tok = String.Format("{0:F2}", width);
				tok = InputBoxMx.Show("Enter new column width (units are characters in standard font)", "Column Width", tok);
				if (tok == null || tok == "") return DialogResult.Cancel;
				try { width = Single.Parse(tok); }
				catch (Exception ex) { continue; }
				if (width <= 0) width = -1; // restore default width
				break;
			}
			qc.DisplayWidth = width; // save width in chars
			return DialogResult.OK;
		}
	}

	/// <summary>
	/// Data structure for QueryTable grid
	/// </summary>

	class QtGridRow
	{
		public Bitmaps16x16Enum ColumnImage = Bitmaps16x16Enum.None;
		public string ColumnLabel = "";
		public Bitmaps16x16Enum ColumnDropdownImage = Bitmaps16x16Enum.None;
		public bool? Selected = null;
		public object Criteria = null; // may be string or structure bitmap
		public Bitmaps16x16Enum SortPosImage = Bitmaps16x16Enum.None;
		public Bitmaps16x16Enum AggregationImage = Bitmaps16x16Enum.None;
		public string AggregationLabel = "";
	}

	class DataFieldWithIcons
	{
		public Bitmaps16x16Enum ColumnImage = Bitmaps16x16Enum.None;
		public string ColumnLabel = "";
		public Bitmaps16x16Enum ColumnDropdownImage = Bitmaps16x16Enum.None;

		public DataFieldWithIcons(Bitmaps16x16Enum colImage, string label, Bitmaps16x16Enum dropDownImage = Bitmaps16x16Enum.None)
		{
			ColumnImage = colImage;
			ColumnLabel = label;
			ColumnDropdownImage = dropDownImage;
			return;
		}

		public override string ToString()
		{
			return ""; // needed to avoid having class name appear as a tooltip within an XtraGrid column that contains DataFieldWithIcons
		}
	}

}
