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
	public class MultiDbAssayMetaBroker
	{

		/// <summary>
		/// Build a query to fetch the assays for a target
		/// </summary>
		/// <param name="geneSymbol"></param>
		/// <returns></returns>

		public static Query BuildTargetAssayListQuery(
			string geneSymbol)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusTargetAssayService,
								(int)Services.Native.ServiceOpCodes.MobiusTargetAssayService.BuildTargetAssayListQuery,
								new Services.Native.NativeMethodTransportObject(new object[] { geneSymbol }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				string serializedQuery = (string)resultObject.Value;
				Query query = Query.Deserialize(serializedQuery);
				return query;
			}
			else return Qel.TargetAssayMetaBroker.BuildTargetAssayListQuery(geneSymbol); // Mobius CMN_ASSY_ATRBTS

		}

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

		/// <summary>
		/// Build query to fetch all summarized (by target) assay data for a target
		/// </summary>
		/// <param name="geneSymbol"></param>
		/// <returns></returns>

		public static Query BuildTargetAssaySummarizedDataQuery(
			string geneSymbol)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusTargetAssayService,
								(int)Services.Native.ServiceOpCodes.MobiusTargetAssayService.BuildTargetAssaySummarizedDataQuery,
								new Services.Native.NativeMethodTransportObject(new object[] { geneSymbol }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				string serializedQuery = (string)resultObject.Value;
				Query query = Query.Deserialize(serializedQuery);
				return query;
			}
			else return Qel.MultiDbAssayMetaBroker.BuildTargetAssaySummarizedDataQuery(geneSymbol);
		}

		/// <summary>
		/// Show NCBI Entre Gene web page for supplied geneId and/or symbol
		/// </summary>
		/// <param name="geneIdSymbol"></param>

		public static string GetTargetDescriptionUrl(
			string targetIdSymbol)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusTargetAssayService,
								(int)Services.Native.ServiceOpCodes.MobiusTargetAssayService.GetTargetDescriptionUrl,
								new Services.Native.NativeMethodTransportObject(new object[] { targetIdSymbol }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				string url = (string)resultObject.Value;
				return url;
			}
			else return Qel.MultiDbAssayMetaBroker.GetTargetDescriptionUrl(targetIdSymbol);
		}

	}
}
