using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;
using Mobius.ServiceFacade;
using Mobius.ClientComponents;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Web;
using System.Threading;
using System.Windows.Forms;

namespace Mobius.SpotfireClient
{

/// <summary>
/// This class handles Spotfire analysis interaction at the level of individual visualizations and their associated properties
/// </summary>

	public class SpotfireViewManager2 : ViewManager
	{
		/// <summary>
		/// Constructor from ResultsViewItem
		/// </summary>

		public SpotfireViewManager2(
			ResultsViewModel rvi)
		{
			ViewType = ResultsViewType.Spotfire;
			Title = "Spotfire Visualization";
			SpotfireViewProps = new SpotfireViewProps(this);
			SpotfireViewProps.AnalysisPath = rvi.ViewSubtype;
			return;
		}


	}
}
