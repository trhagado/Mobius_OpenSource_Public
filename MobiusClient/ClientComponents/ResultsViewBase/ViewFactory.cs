using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireClient;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mobius.ClientComponents
{
	public class ViewFactory : IResultsViewManagerFactory
	{

/// <summary>
/// Create a new view of the specified type
/// </summary>
/// <param name="viewType"></param>
/// <returns></returns>

		public ResultsViewProps NewResultsViewManager(
			ResultsViewModel rvi)
		{
			ResultsViewProps view = null;

			switch (rvi.ViewType)
			{
				case ResultsViewType.Table:
					view = new MoleculeBandedGridViewManager();
					break;

				case ResultsViewType.HtmlTable:
					view = new HtmlTableView();
					break;

				case ResultsViewType.PivotGrid:
					view = new PivotGridView();
					break;

				case ResultsViewType.Spotfire:
					view = new SpotfireViewManager(rvi);
					break;

				case ResultsViewType.ScatterPlot:
					view = new SpotfireViewManager2(rvi);
					break;

				case ResultsViewType.BarChart:
					view = new SpotfireViewManager2(rvi);
					break;


				default:
					DebugLog.Message("Unrecognized view type: " + rvi.ViewType);
					view = new ViewManager();
					break;
					//throw new Exception("Unrecognized view type: " + viewType);

					//case ViewTypeMx.ScatterPlot:
					//	view = new ChartViewBubble();
					//	break;

					//case ViewTypeMx.Heatmap:
					//	view = new ChartViewHeatmap();
					//	break;

					//case ViewTypeMx.TargetSummaryUnpivoted:
					//	view = new TargetSummaryUnpivotedView();
					//	break;

					//case ViewTypeMx.TargetSummaryPivoted:
					//	view = new TargetSummaryPivotedView();
					//	break;

					//case ViewTypeMx.TargetTargetTable:
					//	view = new TargetTargetPivotedView();
					//	break;

					//case ViewTypeMx.TargetSummaryImageMap:
					//	view = new TargetSummaryImageMapView();
					//	break;

					//case ViewTypeMx.TargetSummaryHeatmap:
					//	view = new TargetSummaryHeatmapView();
					//	break;

			}

// Set some key values if not already set by the specifice view manager constructor

			if (view.ViewType == ResultsViewType.Unknown)
				view.ViewType = rvi.ViewType;

			if (Lex.IsUndefined(view.ViewSubtype))
				view.ViewSubtype = rvi.ViewSubtype;

			if (Lex.IsUndefined(view.CustomViewTypeImageName))
				view.CustomViewTypeImageName = rvi.CustomViewTypeImageName;

			if (Lex.IsUndefined(view.Title))
				view.Title = rvi.Title;

			if (view.BaseQuery == null)
				view.BaseQuery = rvi.Query;

			return view;
		}
	}
}
