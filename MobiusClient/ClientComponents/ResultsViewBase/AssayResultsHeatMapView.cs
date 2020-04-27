using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.IO;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// AssayResultsHeatMapView
	/// </summary>

	public class AssayResultsHeatMapView : ChartViewBubble
	{

		/// <summary>
		/// Basic constructor
		/// </summary>

		public AssayResultsHeatMapView()
		{
			ViewType = ViewTypeMx.AssayResultsHeatmap;
			Title = "Assay Results Heatmap";
			AssayHeatmapProperties = new AssayHeatmapProperties();
		}

		/// <summary>
		/// Return true if the view has been defined
		/// </summary>

		public override bool IsDefined
		{
			get
			{
				AssayHeatmapProperties p = AssayHeatmapProperties;
				if (p == null) return false;

				if (p.ColsToSum != ColumnsToTransform.Unknown) return true;
				else return false;
			}
		}

		/// <summary>
		/// Build subquery results if needed
		/// </summary>

		public override IQueryManager BuildSubqueryResults()
		{
			QueryManager sqm = BuildAssayResultsHeatMapSubqueryResults();
			return sqm;
		}

		/// <summary>
		/// Render the view
		/// </summary>

		public override void ConfigureRenderingControl()
		{
			AssureQueryManagerIsDefined(BaseQuery);
			DialogResult dr = Qm.DataTableManager.CompleteRetrieval(); // be sure we have all data
			if (dr == DialogResult.Cancel) return;

			SpotfirePageControl spc = new SpotfirePageControl(); // the Spotfire page control
			spc.InitializeLifetimeService();

			if (Math.Sqrt(4) == 2) return; // todo

			////PanelControl panel = Qrc.ResultsDisplayPanel; // the panel we will render in
			////panel.Controls.Add(spc); // add it to the display panel

			Qrc.SetToolBarTools(spc.ToolPanel, GetZoomPct()); // show the proper tools

			return;
		}

		/// <summary>
		/// Serialize
		/// </summary>
		/// <param name="tw"></param>
		/// <returns></returns>

		public override void Serialize(XmlTextWriter tw)
		{
			BeginSerialization(tw);
			EndSerialization(tw);
			return;
		}

/// <summary>
/// BeginSerialization
/// </summary>
/// <param name="tw"></param>
		
		public override void BeginSerialization(XmlTextWriter tw)
		{
			base.BeginSerialization(tw);

			AssayHeatmapProperties p = AssayHeatmapProperties;
			if (p == null) return;

			tw.WriteStartElement("AssayResultsHeatMap");
			tw.WriteAttributeString("ColsToSum", p.ColsToSum.ToString());
			tw.WriteAttributeString("SumLevel", p.SumLevel.ToString());
			tw.WriteAttributeString("SumMethod", p.SumMethod.ToString());
			tw.WriteAttributeString("PivotFormat", p.PivotFormat.ToString());
			tw.WriteEndElement();

			return;
		}

/// <summary>
/// EndSerialization
/// </summary>
/// <param name="tw"></param>

		public override void EndSerialization(XmlTextWriter tw)
		{
			base.EndSerialization(tw);
			return;
		}

		/// <summary>
		/// Build the Qm, DataTable, etc derived from the root query 
		/// </summary>

		internal QueryManager BuildAssayResultsHeatMapSubqueryResults()
		{
			Query rootQuery = BaseQuery.Root; // get root query
			QueryManager rootQm = rootQuery.QueryManager as QueryManager; // and assoc manager

			AssayHeatmapProperties p = AssayHeatmapProperties;

			Query subQuery = null; // the assay results heat map query
			if (BaseQuery.Parent != null) // is Query a subQuery
			{
				subQuery = BaseQuery;
				QueryManager.RemoveSubQuery(subQuery, false, false); // remove any existing subquery but not assoc views
			}

			OutputDest outputDest = OutputDest.Grid; // fix later
			QueryManager sqm = // summarize root data
				rootQm.DataTableManager.Summarize(p.SumLevel, p.SumMethod, p.ColsToSum, outputDest, null); 

			subQuery = sqm.Query; // this is the new query and associated data

			rootQuery.Subqueries.Add(subQuery); // add subquery to root
			subQuery.Parent = rootQuery; // link subquery to parent query
			BaseQuery = subQuery; // link view to subquery
			return sqm;
		}


	}
}
