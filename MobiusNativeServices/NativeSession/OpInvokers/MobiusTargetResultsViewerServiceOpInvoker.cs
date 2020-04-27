using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mobius.Services.Util.TypeConversionUtil;
using Mobius.Services.Types;
using Mobius.Services.Native.ServiceOpCodes;

namespace Mobius.Services.Native.OpInvokers
{
	public class MobiusTargetResultsViewerServiceOpInvoker : IInvokeServiceOps
	{
		private static TypeConversionHelper _transHelper = new TypeConversionHelper(TypeConversionHelper.ContextRetentionMode.NoRetention);

		object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
		{
			MobiusTargetResultsViewerService op = (MobiusTargetResultsViewerService)opCode;
			switch (op)
			{
				case MobiusTargetResultsViewerService.CreateSpotfireAnalysisDocument:
					{
						string serializedQuery = (string)args[0];
						Mobius.Data.Query q = Mobius.Data.Query.Deserialize(serializedQuery);

						string serializedTrvp = (string)args[1];
						TargetSummaryOptions trvp = TargetSummaryOptions.Deserialize(serializedTrvp);

						string path = Mobius.ToolServices.TargetResultsViewerService.CreateSpotfireAnalysisDocument(q, trvp);
						return path;
					}
			}

			return null;
		}
	}
}
