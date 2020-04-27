using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;
using Mobius.QueryEngineLibrary;
using Qel = Mobius.QueryEngineLibrary;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mobius.Data;
using ServiceTypes = Mobius.Services.Types;

namespace Mobius.ServiceFacade
{
	public class UnpivotedAssayMetaBroker
	{
		/// <summary>
		/// Build query to fetch all unsummarized assay data for a target
		/// </summary>
		/// <param name="geneSymbol"></param>
		/// <returns></returns>

		public static Query BuildTargetAssayUnsummarizedDataQuery(
			string geneSymbol)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusTargetAssayService,
								(int)Services.Native.ServiceOpCodes.MobiusTargetAssayService.BuildTargetAssayUnsummarizedDataQuery,
								new Services.Native.NativeMethodTransportObject(new object[] { geneSymbol }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				string serializedQuery = (string)resultObject.Value;
				Query query = Query.Deserialize(serializedQuery);
				return query;
			}

			else return Qel.MultiDbAssayMetaBroker.BuildTargetAssayUnsummarizedDataQuery(geneSymbol);
		}


	}
}
