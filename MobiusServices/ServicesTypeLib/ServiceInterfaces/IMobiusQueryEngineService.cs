using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Mobius.Services.Types;


namespace Mobius.Services.Types.ServiceInterfaces
{
    [ServiceContract(Namespace = "http://server/MobiusServices/v1.0")]
    public interface IMobiusQueryEngineService
    {
        #region Preferred methods

        [OperationContract]
        string GetCurrentVersionNumber();

        [OperationContract]
        string GetCurrentUserId();

        [OperationContract]
        string GetCurrentSessionUserId(Session session);

        //more to come...

        #endregion Preferred methods


        #region Legacy methods

        [OperationContract]
        int CreateInstance(Session session);

        [OperationContract]
        bool DisposeInstance(Session session, int instanceId);

        [OperationContract]
        Query GetSummarizationDetailQuery(
            Session session,
            int queryEngineInstanceId,
            string metaTableName,
            string metaColumnName,
            int level,
            string resultId);

        [OperationContract]
        Query GetDrilldownDetailQuery(
            Session session,
            int queryEngineInstanceId,
            MetaTable mt,
            MetaColumn mc,
            int level,
            string resultId);

        [OperationContract]
        Mobius.Services.Types.Bitmap GetImage(
            Session session,
            int queryEngineInstanceId,
            MetaColumn metaColumn,
            string graphicsIdString,
            int desiredWidth);

        [OperationContract]
        UserObjectNode ResolveCidListReference(
            Session session,
            string cidListToken);

        [OperationContract]
        MetaTable GetRootTable(
            Session session,
            Query query);

        [OperationContract]
        Query DoPresearchChecksAndTransforms(
            Session session,
            Query query);

        [OperationContract]
        List<string> GetKeys(
            Session session,
            int queryEngineInstanceId);

        [OperationContract]
        List<string> ExecuteQuery(
            Session session,
            int queryEngineInstanceId,
            Query query);

        [OperationContract]
        List<string> ExecuteMQLQuery(
            Session session,
            int queryEngineInstanceId,
            string queryInMQLFormat);

        [OperationContract]
        Query CreateQueryFromMQL(
            Session session,
            string queryInMQLFormat);

        [OperationContract]
				string GetSavedQueryMQL(
					Types.Session publicSession,
					int queryId,
                    bool mobileQuery);

        [OperationContract]
        Query RemapTablesForRetrieval(
            Session session,
            int queryEngineInstanceId);

        [OperationContract]
        List<DataRow> NextRows(Session session, int queryEngineInstanceId, int maxRows, int maxTime);

        [OperationContract]
        DataRow NextRow(Session session, int queryEngineInstanceId);

        [OperationContract]
        void Close(Session session, int queryEngineInstanceId);

        [OperationContract]
        void Cancel(Session session, int queryEngineInstanceId);

        [OperationContract]
        QueryEngineState GetQueryEngineState(Session session, int queryEngineInstanceId);

        [OperationContract]
        Dictionary<string, List<string>> CheckDataSourceAccessibility(
            Session session, int queryEngineInstanceId, Query query);

        [OperationContract]
        Query[] GetStandardMobileQueries(
            Session session);

        [OperationContract]
        Query[] GetMobileQueriesByOwner(
    Session session, string ownerId);

        [OperationContract]
        List<string> TransformAndExecuteQuery(Types.Session publicSession, int qeId, Query query, out Query newQuery);

        #endregion Legacy methods
    }
}
