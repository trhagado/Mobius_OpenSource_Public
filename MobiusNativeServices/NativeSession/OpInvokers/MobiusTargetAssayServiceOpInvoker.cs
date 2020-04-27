using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qel = Mobius.QueryEngineLibrary;
using Mobius.Services.Native.ServiceOpCodes;


namespace Mobius.Services.Native.OpInvokers
{
	public class MobiusTargetAssayServiceOpInvoker : IInvokeServiceOps
	{
		#region IInvokeServiceOps Members

		object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
		{
			MobiusTargetAssayService op = (MobiusTargetAssayService)opCode;

			switch (op)
			{
				case MobiusTargetAssayService.GetTargetDescriptionUrl:
					{
						string url = "";
						string targetIdSymbol = (string)args[0];
						url = Qel.TargetAssayMetaBroker.GetTargetDescriptionUrl(targetIdSymbol);
						return url;
					}

				case MobiusTargetAssayService.BuildTargetAssayListQuery:
					{
						string geneSymbol = (string)args[0];
						Data.Query query = Qel.TargetAssayMetaBroker.BuildTargetAssayListQuery(geneSymbol);
						string serializedQuery = query.Serialize();
						return serializedQuery;
					}

				default: // other methods below not implemented
					throw new NotImplementedException();
			}

			//  case MobiusTargetAssayService.BuildTargetAssayUnsummarizedDataQuery:
			//    {
			//      string geneSymbol = (string)args[0];
			//      Data.Query query = Qel.TargetAssayMetaBroker.BuildTargetAssayUnsummarizedDataQuery(geneSymbol);
			//      string serializedQuery = query.Serialize();
			//      return serializedQuery;
			//    }
			//  case MobiusTargetAssayService.BuildTargetAssaySummarizedDataQuery:
			//    {
			//      string geneSymbol = (string)args[0];
			//      Data.Query query = Qel.TargetAssayMetaBroker.BuildTargetAssaySummarizedDataQuery(geneSymbol);
			//      string serializedQuery = query.Serialize();
			//      return serializedQuery;
			//    }
			//  case MobiusTargetAssayService.UpdateCommonAssayAttributes:
			//    {
			//      string dir = (string)args[0];
			//      MetaFactoryNamespace.TargetAssayMetaFactory metaFactory =
			//          new Mobius.MetaFactoryNamespace.TargetAssayMetaFactory();
			//      string response = metaFactory.UpdateCommonAssayAttributes(dir);
			//      return response;
			//    }

			//return null;
		}

		#endregion
	}
}
