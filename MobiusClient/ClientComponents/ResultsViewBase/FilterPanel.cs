using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
//using DevExpress.XtraCharts;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

/// <summary>
/// Filter panel control containing the set of active filters for the current query
/// </summary>

	public partial class FilterPanel : XtraUserControl, IFilterManager
	{
		internal QueryManager Qm; // QueryManager currently associated with the filter panel

		bool Rendering = false;
		bool InSetup = false;
		int PanelYPos; // position for next criteria tab control
		QueryColumn ActiveQueryColumn;
		static int PaintCount = 0;

		public FilterPanel()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);

			while (ScrollablePanel.Controls.Count > 0)
				ScrollablePanel.Controls.RemoveAt(0);
		}

		private void ChartFilterPanel_Load(object sender, EventArgs e)
		{
			return;
		}

		/// <summary>
		/// Remove controls from scrollable panel
		/// </summary>

		internal void Clear()
		{
			while (ScrollablePanel.Controls.Count > 0) // clear criteria tab panel
				ScrollablePanel.Controls.RemoveAt(0);
		}

		/// <summary>
		/// Render
		/// </summary>
		/// <param name="query"></param>

		internal void Render()
		{
			if (Qm == null) return;
			if (Rendering) return;

			Query q = Qm.Query;
			int criteriaCount = 0;

			//if (q.Tables.Count <= 0 || !SS.I.ShowCriteriaTab) return;

			FiltersEnabled = Qm.DataTableManager.FiltersEnabled;

			Rendering = true;
			Clear();
			PanelYPos = 0;

			bool shownKeyCriteria = false;

			for (int ti = 0; ti < q.Tables.Count; ti++)
			{
				QueryTable qt = q.Tables[ti];
				string txt;
				bool shownTableLabel = false;

				foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (qc.SecondaryFilterType == FilterType.Unknown) continue;

					if (qc.IsKey)
					{
						if (shownKeyCriteria) continue; // skip if already shown
						shownKeyCriteria = true;
					}

					if (!shownTableLabel)
					{
						shownTableLabel = true;
						AddTableLabel(qt.ActiveLabel);
					}

					AddColumnLabel(qc.ActiveLabel);
					AddFilter(qc);
					if (qc.IsKey)
						criteriaCount++;
				} // column loop
			} // table loop

			Rendering = false;
			return;
		}

		/// <summary>
		/// Add a label to the criteria tab
		/// </summary>
		/// <param name="text"></param>

		LabelControl AddTableLabel(
			string text)
		{
			LabelControl label = new LabelControl();

			label.AutoSize = true;
			label.Font = new System.Drawing.Font("Tahoma", 9, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			label.ForeColor = System.Drawing.Color.Black;
			label.Location = new System.Drawing.Point(0, PanelYPos + 4);
			label.Name = "TableLabel";
			label.Size = new System.Drawing.Size(69, 17);
			label.TabIndex = 150;
			label.Text = text;

			ScrollablePanel.Controls.Add(label);
			PanelYPos += 20;
			return label;
		}

		/// <summary>
		/// Create label for criteria tab column
		/// </summary>
		/// <param name="labelText"></param>
		/// <returns></returns>

		LabelControl AddColumnLabel(
			string labelText)
		{
			LabelControl label = new LabelControl();
			label.AutoSize = true;
			label.Font = new System.Drawing.Font("Tahoma", 9, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			label.ForeColor = System.Drawing.Color.Black;
			label.Location = new System.Drawing.Point(4, PanelYPos + 4);
			label.Name = "ColumnLabel";
			label.Size = new System.Drawing.Size(81, 17);
			label.TabIndex = 151;
			label.Text = labelText;

			ScrollablePanel.Controls.Add(label);
			PanelYPos += 20;
			return label;
		}

		/// <summary>
		/// Add a filter item
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="criteriaText"></param>

		void AddFilter(QueryColumn qc)
		{
			Control control = null, focusControl = null;
			ColumnInfo colInfo;


			try { colInfo = ResultsFormat.GetColumnInfo(Qm, qc); }
			catch { return; } // ignore if col no longer in query

			int buttonDy = 4;

			if (qc.SecondaryFilterType == FilterType.BasicCriteria)
			{
				FilterBasicCriteriaControl fbc = new FilterBasicCriteriaControl();
				fbc.Setup(colInfo, this);
				if (qc == ActiveQueryColumn) focusControl = fbc.Value; // put focus on value if this is the active column
				control = fbc;
			}

			else if (qc.SecondaryFilterType == FilterType.CheckBoxList)
			{
				FilterCheckBoxListControl flc = new FilterCheckBoxListControl();
				flc.Setup(colInfo);
				control = flc;
			}

			else if (qc.SecondaryFilterType == FilterType.ItemSlider)
			{
				FilterItemSliderControl fis = new FilterItemSliderControl();
				fis.Setup(colInfo);
				control = fis;
				buttonDy = 16;
			}

			else if (qc.SecondaryFilterType == FilterType.RangeSlider)
			{
				FilterRangeSliderControl frs = new FilterRangeSliderControl();
				frs.Setup(colInfo);
				control = frs;
				buttonDy = 16;
			}

			else if (qc.SecondaryFilterType == FilterType.StructureSearch)
			{
				FilterStructureControl fss = new FilterStructureControl();
				fss.Setup(colInfo);
				control = fss;
			}

			control.Top = PanelYPos;
			control.Left = 0;
			control.Width = ScrollablePanel.Width - 34;
			ScrollablePanel.Controls.Add(control);
			if (focusControl != null) focusControl.Focus(); 

//			if (qc.SecondaryFilterType != FilterType.StructureSearch)
			AddFilterDropDownButton(qc, ScrollablePanel.Width - 32, PanelYPos + buttonDy);

			PanelYPos += control.Height + 2;

			return;
		}

		/// <summary>
		/// Add dropdown button to right of filter
		/// </summary>
		/// <param name="qc"></param>

		void AddFilterDropDownButton(QueryColumn qc, int x, int y)
		{
			SimpleButton b = new SimpleButton();

			b.Appearance.BackColor = System.Drawing.Color.Transparent;
			b.Appearance.Options.UseBackColor = true;
			b.ImageIndex = 0;
			b.ImageList = this.Bitmaps5x5;
			b.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			b.Location = new System.Drawing.Point(282, 66);
			b.Name = "FilterDropDownButton";
			b.Size = new System.Drawing.Size(12, 20);
			b.Click += new System.EventHandler(FilterDropDownButton_Click);

			b.Left = x;
			b.Top = y;
			b.Tag = qc; // link button to QueryColumn

			ScrollablePanel.Controls.Add(b);
			return;
		}

		private void FilterDropDownButton_Click(object sender, EventArgs e)
		{
			bool showOtherFilterTypes;

			SimpleButton b = sender as SimpleButton;
			ActiveQueryColumn = b.Tag as QueryColumn;

			if (ActiveQueryColumn.MetaColumn.DataType == MetaColumnType.Structure) showOtherFilterTypes = false;
			else showOtherFilterTypes = true;

			for (int i1 = 0; i1 < 5; i1++)
				FilterContextMenu.Items[i1].Visible = showOtherFilterTypes;

			FilterContextMenu.Show(b, new System.Drawing.Point(0, b.Size.Height));
		}

/// <summary>
/// Add a new filter
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void AddFilterButton_Click(object sender, EventArgs e)
		{
			QueryColumn qc = null;

			FieldSelectorControl fieldSelector = new FieldSelectorControl();
			Point p = new Point(AddFilterButton.Left, AddFilterButton.Bottom);
			p = this.PointToScreen(p);

			SelectColumnOptions flags = new SelectColumnOptions();
			flags.SearchableOnly = true;
			flags.FirstTableKeyOnly = true;
			flags.SelectFromQueryOnly = true;
			DialogResult dr = fieldSelector.SelectColumnFromQuery(Qm.Query, null, flags, p.X, p.Y, out qc);
			if (dr != DialogResult.OK) return;

			if (qc.SecondaryFilterType != FilterType.Unknown) { } // already have a filter
			if (qc.MetaColumn.DataType == MetaColumnType.Structure)
				qc.SecondaryFilterType = FilterType.StructureSearch;

			else
			{
				ColumnStatistics stats = null;
				if (Qm != null && Qm.DataTableManager != null) stats = Qm.DataTableManager.GetStats(qc);
				if (stats != null && stats.DistinctValueList.Count <= 10)
					qc.SecondaryFilterType = FilterType.CheckBoxList;
				else if (qc.MetaColumn.IsNumeric)
					qc.SecondaryFilterType = FilterType.RangeSlider;
				else qc.SecondaryFilterType = FilterType.BasicCriteria;
			}

			ActiveQueryColumn = qc;

			if (!Qm.DataTableManager.FiltersEnabled) // be sure filters are enabled & view also
			{
				Qm.DataTableManager.FiltersEnabled = true;
				Qm.DataTableManager.ApplyFilters();
				Qm.DataTableManager.UpdateFilterState();
				QueryResultsControl.GetQrcThatContainsControl(this).UpdateFilteredViews();
			}

			Render();
		}

		/// <summary>
		/// Callback from individual filter (basic) to switch to new filter control
		/// </summary>

		public void ChangeFilterTypeCallback(QueryColumn qc, FilterType newType)
		{
			ActiveQueryColumn = qc;
			if (newType == FilterType.BasicCriteria || newType == FilterType.Unknown)
				BasicFilterMenuItem_Click(null, null);
			else if (newType == FilterType.CheckBoxList)
				ListFilterMenuItem_Click(null, null);
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
			return Qm;
		}


		private void BasicFilterMenuItem_Click(object sender, EventArgs e)
		{
			FilterTypeSelected(FilterType.BasicCriteria);
		}

		private void ListFilterMenuItem_Click(object sender, EventArgs e)
		{
			FilterTypeSelected(FilterType.CheckBoxList);
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
		/// Filter type selected, may be the same or different
		/// </summary>
		/// <param name="ft"></param>

		void FilterTypeSelected(FilterType ft)
		{
			ColumnInfo ci = Qm.ResultsFormat.GetColumnInfo(ActiveQueryColumn);
			QueryResultsControl.GetQrcThatContainsControl(this).ChangeFilterType(ci, ft);

			Render();
		}

		private void RemoveFilterMenuItem_Click(object sender, EventArgs e)
		{
			ActiveQueryColumn.SecondaryFilterType = FilterType.Unknown;
			ActiveQueryColumn.SecondaryCriteria = ActiveQueryColumn.SecondaryCriteriaDisplay = "";
			if (QueryResultsControl.GetQrcThatContainsControl(this) != null)
			{
				ColumnInfo colInfo = ResultsFormat.GetColumnInfo(Qm, ActiveQueryColumn);
				QueryResultsControl.GetQrcThatContainsControl(this).UpdateFiltering(colInfo);
			}

			FilterBasicCriteriaControl.SyncBaseQuerySecondaryCriteria(ActiveQueryColumn); // sync any base query

			Render();
		}

		private void ResetAllFiltersMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.Instance.ResetFilters();
			Render();
		}

		private void RemoveAllFiltersMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.Instance.ClearFilters();
			Render();
		}

		private void ScrollablePanel_Resize(object sender, EventArgs e)
		{
			if (Qm == null || Qm.QueryResultsControl == null)
				//// || !QueryResultsControl.GetQrcThatContainsControl(this).InChartView) 
			  return; // just return if not in chart view

			if (Qm != null && Qm.DataTableManager != null && 
			 Qm.DataTableManager.RowRetrievalState != RowRetrievalState.Complete) 
				return;

			Render(); // rerender filters to current size
		}

		private void FiltersEnabled_CheckedChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			SessionManager.Instance.EnableFilters(FiltersEnabledCtl.Checked);
		}

/// <summary>
/// Set FiltersEnabledCtl witout executing event code
/// </summary>

		internal bool FiltersEnabled
		{
			get { return FiltersEnabledCtl.Checked; }
			set
			{
				InSetup = true;
				FiltersEnabledCtl.Checked = value;
				InSetup = false;
				return;
			}
		}

		private void FilterPanel_Paint(object sender, PaintEventArgs e)
		{
			PaintCount++;
			//DebugLog.Message("FilterPanel_Paint " + new StackTrace().ToString());
			return;
		}

	}
}
