using Mobius.ComOps;
using Mobius.Data;
using Mobius.ToolServices;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mobius.ServiceFacade
{
	/// <summary>
	/// TargetResultsViewerService
	/// </summary>

	public class TargetResultsViewerService
	{

		/// <summary>
		/// Create a Spotfire Analysis Document displaying data for the specified
		/// query using the template and other formatting info in the SpotfireParms.
		/// </summary>
		/// <param name="q"></param>
		/// <param name="stp"></param>
		/// <returns></returns>

		public static string CreateSpotfireAnalysisDocument(
			Query q,
			TargetSummaryOptions trvp)
		{
			string serializedQuery = q.Serialize();
			string serializedTrvp = trvp.Serialize();

			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusTargetResultsViewerService,
								(int)Services.Native.ServiceOpCodes.MobiusTargetResultsViewerService.CreateSpotfireAnalysisDocument,
								new Services.Native.NativeMethodTransportObject(new object[] { serializedQuery, serializedTrvp }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null || resultObject.Value == null) return null;
				string path = resultObject.Value.ToString();
				return path;
			}

			else
			{
				q = Query.Deserialize(serializedQuery);
				trvp = TargetSummaryOptions.Deserialize(serializedTrvp);
				string path = Mobius.ToolServices.TargetResultsViewerService.CreateSpotfireAnalysisDocument(q, trvp);
				return path;
			}
		}
	}
}
