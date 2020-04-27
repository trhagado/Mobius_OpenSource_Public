using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Text;

namespace Mobius.ToolServices
{
/// <summary>
/// Server-side methods for the TargetResultsViewer
/// </summary>

	public class TargetResultsViewerService
	{

/// <summary>
/// Write out the data and return the html/js to open the template .dxp file with the exported data file
/// </summary>
/// <param name="q"></param>
/// <param name="trvp"></param>
/// <returns></returns>

		public static string CreateSpotfireAnalysisDocument( 
			Query q,
			TargetSummaryOptions trvp)
		{
			ExportParms ep = new ExportParms();
			ep.OutputDestination = OutputDest.TextFile;
			ep.ExportFileFormat = ExportFileFormat.Csv;
			ep.OutputFileName = TempFile.GetTempFileName(".csv"); // temp .csv file to export to
			ep.IncludeDataTypes = true;

			string result = UalUtil.IQueryExec.RunQuery(q, ep); // do the export
			if (result == null) return null; // just return if cancelled

			string templateLibFolder = // unc name of the lib folder (not currently used)
				ServicesIniFile.Read("SpotfireLibraryFolder");

			string templateLibUrl = // url of the library containing the Mobius templates
				ServicesIniFile.Read("SpotfireLibraryUrl");
			templateLibUrl = @"http://[server]/SpotfireWeb/Library.aspx?folder=DCRT%20(chemistry)%20users/Mobius/Templates";
			
			string templateUrl = templateLibUrl + "/MultiDbView";

			string html = templateUrl; // todo, build html/js to open the template with the specified file
			return html;
		}

	}
}
