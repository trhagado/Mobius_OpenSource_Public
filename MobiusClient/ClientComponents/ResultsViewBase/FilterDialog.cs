using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class FilterDialog : XtraForm, IFilterManager
	{
		public static FilterDialog Instance;
		public static FilterDialog Prototype;

		MoleculeGridControl Grid;
		QueryManager Qm { get { return Grid.QueryManager; } }
		DataTableManager MolTable { get { return Grid.Dtm; } }
		Size ExpandedSize;
		Form CallingActiveForm;
		ColumnInfo ColInfo; // info on column being edited
		QueryColumn InitialQcState; // save initial state here in case of cancel

		int HeightBelowDivider;
		const int TitleBarHeight = 20; // height for title bar if it appeared

		public FilterDialog()
		{
			InitializeComponent();

			Instance = this;
			HeightBelowDivider = Height - Divider.Top;
		}

		/// <summary>
		/// Invoke the editor
		/// </summary>
		/// <param name="qc">QueryColumn to edit</param>
		/// <returns></returns>

		public static void Edit(
			GridColumn gc)
		{
			FilterDialog fltr;
			if (Prototype == null) Prototype = new FilterDialog();

			fltr = new FilterDialog();

			fltr.CallingActiveForm = SessionManager.ActiveForm;

			GridView view = gc.View as GridView;
			fltr.Grid = view.GridControl as MoleculeGridControl;
			fltr.ColInfo = fltr.Grid.GetColumnInfo(gc);
			fltr.InitialQcState = fltr.ColInfo.Qc.Clone();

			fltr.ConfigureDialog();

			if (!fltr.Qm.DataTableManager.FiltersEnabled)
				fltr.Qm.QueryResultsControl.UpdateFiltering(fltr.ColInfo);

			if (fltr.ColInfo.Mc.DataType == MetaColumnType.Structure) // make modal for structure
				fltr.ShowDialog(fltr.CallingActiveForm);

			else // non-modal for others
			{
				fltr.Show(fltr.CallingActiveForm);
				fltr.SetFocusToActiveFilter();
			}

			return;
		}

		static Rectangle GetColumnHeaderBounds(GridColumn column)
		{
			if (column == null) return Rectangle.Empty;
			GridViewInfo viewInfo = column.View.GetViewInfo() as GridViewInfo;
			if (viewInfo.ColumnsInfo[column] != null)
				return viewInfo.ColumnsInfo[column].Bounds;
			return Rectangle.Empty;
		}

/// <summary>
/// Configure dialog for current filter type and value
/// </summary>

		void ConfigureDialog()
		{
			QueryColumn Qc, qc;
			MetaColumn mc;
			XtraUserControl pc;
			int dx, dy;

			qc = ColInfo.Qc;
			mc = qc.MetaColumn;

			FilterType ft = qc.SecondaryFilterType;
			if (qc.MetaColumn.DataType == MetaColumnType.Structure)
				ft = FilterType.StructureSearch;

			else if (ft == FilterType.Unknown) 
			{
				ColumnStatistics stats = null;
				if (Qm != null && Qm.DataTableManager != null) stats = Qm.DataTableManager.GetStats(qc);
				if (stats != null && stats.DistinctValueList.Count <= 5) // if small number of items default to checkbox list
					ft = FilterType.CheckBoxList;
				else if (qc.MetaColumn.IsNumeric)
					ft = FilterType.RangeSlider;
				else ft = FilterType.BasicCriteria;
			}

			FilterBasicControl.Visible = false;
			FilterListControl.Visible = false;
			FilterItemControl.Visible = false;
			FilterRangeControl.Visible = false;
			FilterStructureControl.Visible = false;

			ChangeFilterType.Visible = true;
			EditStructure.Visible = false;

			SetupFilter(ft);

			if (ft == FilterType.BasicCriteria) pc = FilterBasicControl;
			else if (ft == FilterType.CheckBoxList)
			{
				pc = FilterListControl;
				if (pc.Height < 30) pc.Height = Prototype.FilterListControl.Height; // maintain minimum size
				pc.Width = Width - 8;
			}
			else if (ft == FilterType.ItemSlider) pc = FilterItemControl;
			else if (ft == FilterType.RangeSlider) pc = FilterRangeControl;

			else if (ft == FilterType.StructureSearch)
			{
				pc = FilterStructureControl;
				if (pc.Height < 30) pc.Height = Prototype.FilterStructureControl.Height; // maintain minimum size
				pc.Width = Width - 8;
				EditStructure.Location = Prototype.ChangeFilterType.Location; // show edit structure in place of change filter type
				EditStructure.Visible = true;
				ChangeFilterType.Visible = false;
			}

			else throw new Exception("Unexpected SecondaryCriteriaType: " + ft);

			pc.Visible = true;

			Size s0 = pc.Size;
			pc.Location = new Point(0, 0);
			int formHeight = pc.Bottom + HeightBelowDivider;
			if (Text != "") formHeight += TitleBarHeight; // add title bar height if visible
			Height = formHeight;
			pc.Size = s0; // restore filter control size

			Rectangle r = GetColumnHeaderBounds(ColInfo.GridColumn);
			Point p = new Point(r.Left, r.Top - Height); // place above cell header
			p = Grid.PointToScreen(p);
			if (p.Y < 0) p = new Point(p.X, 0); // keep on screen
			int screenWidth = Screen.PrimaryScreen.Bounds.Width;
			if (p.X + Width > screenWidth) p = new Point(screenWidth - Width, p.Y);
			Location = p;

			string crit = qc.SecondaryCriteria;

			return;
		}

/// <summary>
/// Setup filter controls with current criteria values
/// </summary>

		void SetupFilter(FilterType ft)
		{
			if (ft == FilterType.BasicCriteria)	FilterBasicControl.Setup(ColInfo, this);
			else if (ft == FilterType.CheckBoxList) FilterListControl.Setup(ColInfo);
			else if (ft == FilterType.ItemSlider) FilterItemControl.Setup(ColInfo);
			else if (ft == FilterType.RangeSlider) FilterRangeControl.Setup(ColInfo);
			else if (ft == FilterType.StructureSearch) FilterStructureControl.Setup(ColInfo);
		}

		void SetFocusToActiveFilter()
		{
			FilterType ft = ColInfo.Qc.SecondaryFilterType;
			if (ft == FilterType.ItemSlider) FilterItemControl.ItemFilter.Focus();
			else if (ft == FilterType.RangeSlider) FilterRangeControl.RangeFilter.Focus();
			else
			{
				TextEdit v = FilterBasicControl.Value;
				v.Focus(); // focus on text for either single text filter or standard filters
				v.SelectionLength = 0;
				v.SelectionStart = v.Text.Length;
//				ClientLog.Message("Start: " + v.SelectionStart + ", length: " + v.SelectionLength);
			}

		}

/// <summary>
/// Remove filtering on this column
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void ClearFilter_Click(object sender, EventArgs e)
		{
			MolTable.ClearFilter(ColInfo.Qc);
			Qm.QueryResultsControl.UpdateFiltering(ColInfo);
			OK_Click(sender, e);
		}

/// <summary>
/// Close dialog
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void OK_Click(object sender, EventArgs e)
		{
			Hide();

			if (Qm.StatusBarManager != null)
			{ // be sure status is up to date
				Qm.DataTableManager.UpdateFilterState();
				Qm.StatusBarManager.DisplayFilterCountsAndString();
			}

			Grid.SetFilterGlyph(ColInfo);

			////ChartPageControl cpc = Qm.QueryResultsControl.ChartPageControl; // if in chart view update chart filter panel
			////if (cpc != null && cpc.Visible) cpc.ChartPagePanel.FilterPanel.Render();

			if (CallingActiveForm != null) CallingActiveForm.Focus();
		}

		private void Cancel_Click(object sender, EventArgs e)
		{ // restore previous criteria state
			QueryColumn qc = ColInfo.Qc;
			qc.SecondaryCriteria = InitialQcState.SecondaryCriteria;
			qc.SecondaryCriteriaDisplay = InitialQcState.SecondaryCriteriaDisplay;
			qc.SecondaryFilterType = InitialQcState.SecondaryFilterType;

			Qm.QueryResultsControl.UpdateFiltering(ColInfo); // reapply any previous filter
			FilterBasicCriteriaControl.SyncBaseQuerySecondaryCriteria(qc); // sync any base query

			Hide();
			if (CallingActiveForm != null) CallingActiveForm.Focus();
		}

		private void CriteriaInteractive_Deactivate(object sender, EventArgs e)
		{ // close when deactivated unless structure edit
			if (ColInfo.Mc.DataType != MetaColumnType.Structure)
				OK_Click(sender, e);
		}

		private void FilterDialog_Activated(object sender, EventArgs e)
		{
			FilterBasicControl.Value.Focus();
		}

		private void ChangeFilterType_Click(object sender, EventArgs e)
		{
			FilterTypesContextMenu.Show(ChangeFilterType,
				new System.Drawing.Point(0, ChangeFilterType.Height));
		}

		private void ChangeFilterType_ShowDropDownControl(object sender, ShowDropDownControlEventArgs e)
		{
			ChangeFilterType_Click(sender, e);
		}

		private void BasicFilterMenuItem_Click(object sender, EventArgs e)
		{
			FilterTypeSelected(FilterType.BasicCriteria);
		}

/// <summary>
/// Filter type selected, may be the same or different
/// </summary>
/// <param name="ft"></param>

		void FilterTypeSelected(FilterType ft)
		{
			Qm.QueryResultsControl.ChangeFilterType(ColInfo, ft);

			ConfigureDialog();
			SetFocusToActiveFilter();
		}

		private void CheckBoxFilterMenuItem_Click(object sender, EventArgs e)
		{
			FilterTypeSelected(FilterType.CheckBoxList); // show standard if type if listbox or unknown
		}

		private void ItemSliderMenuItem_Click(object sender, EventArgs e)
		{
			FilterTypeSelected(FilterType.ItemSlider);
		}

		private void RangeSliderMenuItem_Click(object sender, EventArgs e)
		{
			FilterTypeSelected(FilterType.RangeSlider);
		}

		/// <summary>
		/// Callback from individual filter (basic) to switch to new filter control (IFilterManager)
		/// </summary>

		public void ChangeFilterTypeCallback(QueryColumn qc, FilterType newType)
		{
			if (newType == FilterType.BasicCriteria || newType == FilterType.Unknown)
				BasicFilterMenuItem_Click(null, null);
			else if (newType == FilterType.CheckBoxList)
				CheckBoxFilterMenuItem_Click(null, null);
			else if (newType == FilterType.ItemSlider)
				ItemSliderMenuItem_Click(null, null);
			else if (newType == FilterType.RangeSlider)
				RangeSliderMenuItem_Click(null, null);
		}

		/// <summary>
		/// Get the associated QueryManager (IFilterManager)
		/// </summary>

		public QueryManager GetQueryManager()
		{
			return Grid.QueryManager;
		}

		private void EditStructure_Click(object sender, EventArgs e)
		{
			CdkMx.CdkMolControl.EditStructure(FilterStructureControl.StructureRenditor);
			return;
		}

	}


}