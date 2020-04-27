using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Mobius.Services.Types.ServiceInterfaces
{

  /// <summary>
  /// Contracts for MobiusExternalService
  /// </summary>

  [ServiceContract(Namespace = "http://server/MobiusServices/v1.0")]
    public interface IMobiusExternalService
    {
        /// <summary>
        /// Simple method to get the current user id
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        string GetCurrentUserId();

        /// <summary>
        /// Gets the entire XML Meta Tree
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        Stream GetMetaTreeXml();

        /// <summary>
        /// Retrieves the mql for a given Query ID
        /// </summary>
        /// <param name="queryId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "/GetMqlByQueryId/{queryId}")]
        Stream GetMqlByQueryId(string queryId);

        /// <summary>
        /// Retrievese tab delimited results from mql
        /// </summary>
        /// <param name="mql"></param>
        /// <param name="suppressColumnHeaders"></param>
        /// <param name="showDataTypesRow"></param>
        /// <param name="mergePrefixAndValue"></param>
        /// <param name="forceCompoundIdRepeat"></param>
        /// <returns></returns>
        [OperationContract(Name = "GetMqlQueryResults")]
        [WebInvoke(Method = "POST",
            UriTemplate =
                "GetMqlQueryResults?suppressColumnHeaders={suppressColumnHeaders}&showDataTypesRow={showDataTypesRow}&mergePrefixAndValue={mergePrefixAndValue}&forceCompoundIdRepeat={forceCompoundIdRepeat}"
            )]
        Stream GetMqlQueryResults(Stream mql,
            string suppressColumnHeaders = "false",
            string showDataTypesRow = "false",
            string mergePrefixAndValue = "true",
            string forceCompoundIdRepeat = "false");
    }

}
