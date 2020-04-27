using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using Mobius.ComOps;

using DevExpress.XtraEditors;

namespace Mobius.Data

{
    public class ResultsViewTypeBitmaps
    {

        /// <summary>
        /// Get image associated with view type
				/// Note: The image list needs to be stored in QueryResultsControl to work properly from
				///  that control. (See DexExpress SharedImageCollection for another option)
        /// </summary>
        /// <param name="viewType"></param>
        /// <returns></returns>

				//public static Image GetImage(ViewTypeMx viewType)
				//{
				//    Image image = Bitmaps.ViewTypeBitmaps.Images[(int)GetImageIndex(viewType)];
				//    return image;
				//}

        /// <summary>
        /// Get image index for view type within ViewTypeSelectionMenu.ViewTypeBitmaps16x16
        /// </summary>
        /// <param name="viewType"></param>
        /// <returns></returns>

        public static ViewTypeImageEnum GetImageIndex(ViewTypeMx viewType)
        {
            switch (viewType)
            {
                case ViewTypeMx.Table: return ViewTypeImageEnum.Table;
                case ViewTypeMx.HtmlTable: return ViewTypeImageEnum.Table;

                case ViewTypeMx.QueryBased: return ViewTypeImageEnum.CfSummary;
                //case ViewTypeMx.TargetSummaryUnpivoted: return ViewTypeImageEnum.TargetSummaryUnpivoted;
                //case ViewTypeMx.TargetSummaryPivoted: return ViewTypeImageEnum.TargetSummaryPivoted;
                //case ViewTypeMx.TargetTargetTable: return ViewTypeImageEnum.TargetTargetTable;
                //case ViewTypeMx.TargetTargetNetwork: return ViewTypeImageEnum.TargetTargetNetwork;
                case ViewTypeMx.PivotGrid: return ViewTypeImageEnum.PivotTable;
                //case ViewTypeMx.Network: return ViewTypeImageEnum.Network;

                //case ViewTypeMx.TargetSummaryHeatmap: return ViewTypeImageEnum.HeatMap;
                //case ViewTypeMx.TargetSummaryImageMap: return ViewTypeImageEnum.TargetMap;

                case ViewTypeMx.Spotfire: return ViewTypeImageEnum.Spotfire;
                //case ViewTypeMx.ScatterPlot: return ViewTypeImageEnum.BubbleChart;
                //case ViewTypeMx.BarChart: return ViewTypeImageEnum.BarChart;
                //case ViewTypeMx.RadarPlot: return ViewTypeImageEnum.RadarPlot;
                //case ViewTypeMx.Heatmap: return ViewTypeImageEnum.HeatMap;
                //case ViewTypeMx.SasMap: return ViewTypeImageEnum.SasMap;

                default: return ViewTypeImageEnum.Unknown;
            }
        }
    }

    /// <summary>
    /// Index of bitmaps for each view type in 
    /// </summary>

    public enum ViewTypeImageEnum
    {
        Unknown = 0, // generic/unknown view
        AddNew = 1, // add a new page or view (used in tab header)
        PageLayout = 2, // layout of views on page (used in tab header)
        BlankPage = 3, // newly added blank page
        LayoutStacked = 4, // stacked views
        LayoutSideBySide = 5, // side by side views
        LayoutRowsAndCols = 6, // even views
        LayoutTabbed = 7, // tabbed views

        Table = 8, // basic data table
        PivotTable = 9,
        BubbleChart = 10,
        CfSummary = 11,
        HeatMap = 12,
        Network = 13,
        TargetSummaryUnpivoted = 14,
        TargetMap = 15, // dendogram, pathway map
        TargetSummaryPivoted = 16,
        TargetTargetNetwork = 17,
        TargetTargetTable = 18,

        Unused = 19,
        BarChart = 20,
        RadarPlot = 21,
        Summary = 22,
        SasMap = 23,

				Spotfire = 24,
				FromLibraryTemplate = 25,
				TrellisCard = 26,
				SpillTheBeans = 27
	}

}
