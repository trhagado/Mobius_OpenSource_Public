using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraPivotGrid;
using DevExpress.XtraPivotGrid.Customization;
using DevExpress.XtraPivotGrid.Data;
using DevExpress.XtraPivotGrid.FilterDropDown;
using DevExpress.XtraPivotGrid.Frames;
using DevExpress.XtraPivotGrid.Printing;
using DevExpress.XtraPivotGrid.Selection;
using DevExpress.XtraPivotGrid.TypeConverters;
using DevExpress.XtraPivotGrid.Utils;
using DevExpress.XtraPivotGrid.ViewInfo;

using DevExpress.XtraEditors;
using DevExpress.Data;
using DevExpress.Data.PivotGrid;
//using DevExpress.XtraCharts;
using DevExpress.Utils;

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

/// <summary>
/// Create a pivot grid summarizing data by conditional formatting categories
/// </summary>
 
	public class CfSummaryView : PivotGridView 
	{
		GroupingTypeEnum DateGroupInterval = GroupingTypeEnum.EqualValues; // preferred date grouping

/// <summary>
/// Basic constructor
/// </summary>

		public CfSummaryView()
		{
			ViewType = ResultsViewType.SecondaryQuery;
			Title = "Summary View";
		}

		/// <summary>
		/// Add conditional formatting summary view to list of views if one doesn't exist & query contains conditional formatting
		/// </summary>
		/// <param name="q"></param>

		public static void AddCfViewAsNeeded(Query query)
		{
			ResultsPages pages;
			ResultsPage page;
			ResultsViewProps view;
			CfSummaryView cfView;

			if (!query.ContainsSelectedCondFormatting) return;

			pages = query.Root.ResultsPages;
			if (pages == null || pages.Pages == null) return;
			for (int ci = 0; ci < pages.Pages.Count; ci++)
			{
				view = pages[ci].FirstView;
				if (view == null) continue;
				if (view.ViewType == ResultsViewType.SecondaryQuery) return;
			}

			ResultsPages.AddNewPageAndView(query, ResultsViewType.SecondaryQuery, null, out page, out view);
			cfView = page.FirstView as CfSummaryView;
		
			return;
		}

		/// <summary>
		/// Render the view into the specified results control
		/// </summary>

		public override void ConfigureRenderingControl()
		{
			Query q;
			ResultsFormat rf = ResultsFormat;

			ResultsTable rt;
			QueryTable qt;
			MetaTable mt;

			ResultsField rfld;
			Mobius.Data.QueryColumn qc;
			MetaColumn mc;

			PivotGridField pgf;

			bool includeActivityClass = true;
			if (!BuildUnpivotedResults(includeActivityClass)) return;

			PivotGridPropertiesMx p = PivotGridPropertiesMx;
			if (p == null) return;

			if (p.PivotFields == null) // if not defined then configure 
			{
				PivotGridCtl.Fields.Clear(); // clear any prev display
				PivotGridCtl.Groups.Clear();

				if (DataIncludesDates)
				{
					DialogResult dr = DateGroupingDialog.ShowDialog(ref DateGroupInterval);
				}

				p.PivotFields = new List<PivotGridFieldMx>();

				foreach (ResultsTable rt0 in rf.Tables) // determine visible fields
				{
					rt = rt0;
					qt = rt.QueryTable;
					mt = qt.MetaTable;
					if (Lex.Eq(mt.Name, MultiDbAssayDataNames.CombinedNonSumTableName))
					{
						if ((rfld = rt.GetResultsFieldByName("activity_bin")) != null)
							pgf = PivotGridView.AddField(rfld, p.PivotFields, PivotArea.RowArea, true, null, GroupingTypeEnum.EqualValues);

						if ((rfld = rt.GetResultsFieldByName("run_dt")) != null &&
							DataIncludesDates && DateGroupInterval != GroupingTypeEnum.EqualValues)
						{
							GroupingTypeEnum pgi = DateGroupInterval;

							int intervalCnt = 0;
							bool week = false, month = false, year = false; // include larger date units up to year
							if (pgi == GroupingTypeEnum.DateDayOfWeek)
							{
								week = month = year = true;
								intervalCnt = 3;
							}

							else if (pgi == GroupingTypeEnum.DateWeekOfMonth)
							{
								intervalCnt = 3;

								month = year = true;
							}

							else if (pgi == GroupingTypeEnum.DateMonth ||
							 pgi == GroupingTypeEnum.DateQuarter)
								year = true;

							PivotGridGroup g = null;

							if (intervalCnt > 1)
							{
								g = new PivotGridGroup();
								PivotGridCtl.Groups.Add(g);
							}

							if (year) PivotGridView.AddField(rfld, p.PivotFields, PivotArea.ColumnArea, true, g, GroupingTypeEnum.DateYear);
							if (month) PivotGridView.AddField(rfld, p.PivotFields, PivotArea.ColumnArea, true, g, GroupingTypeEnum.DateMonth);
							if (week) PivotGridView.AddField(rfld, p.PivotFields, PivotArea.ColumnArea, true, g, GroupingTypeEnum.DateWeekOfMonth);
							PivotGridView.AddField(rfld, p.PivotFields, PivotArea.ColumnArea, true, g, pgi);
						}

						if ((rfld = rt.GetResultsFieldByName("assy_nm")) != null)
							pgf = PivotGridView.AddField(rfld, p.PivotFields, PivotArea.ColumnArea, true, null, GroupingTypeEnum.EqualValues);

						if ((rfld = rt.GetResultsFieldByName("rslt_val")) != null)
							pgf = PivotGridView.AddField(rfld, p.PivotFields, PivotArea.DataArea, true, null, GroupingTypeEnum.EqualValues);

					}

					else // tables other than the standard pivoted table
					{

						foreach (ResultsField rfld0 in rt.Fields)
						{
							rfld = rfld0;
							qc = rfld.QueryColumn;
							mc = qc.MetaColumn;
						}

					}
				}
			} // done defining initial set of fields

			SyncMxFieldListWithSourceQuery(BaseQuery, ref p.PivotFields); // be sure main grid & pivot view fields are in synch

			ConfigurePivotGridControl(); // configure the control to display the data

			PivotGridCtl.DataSource = Qm.DataTableManager.DataSource; // set the data table to start rendering

			return;
		}

	}
}
