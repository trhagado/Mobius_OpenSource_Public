using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.Services.Native.ServiceOpCodes
{

	public enum MobiusAnnotationService
	{
		CreateInstance,
		DisposeInstance,
		Select,
		SelectByResultId,
		SelectCompoundCount,
		SelectRowCount,
		GetNextIdsLong,
		SetArchiveStatus,
		SetResultGroupArchiveStatus,
		DeleteCompound,
		DeleteTable,
		DeleteResultGroup,
		Insert,
		UpdateCid,
		UpdateResultGroupCid,
		InsertUpdateRow,
		InsertUpdateRowAndUserObjectHeader,
		DeepClone,
		BeginTransaction,
		Commit,
        GetNonNullRsltCnt
    }

	public enum MobiusCidListService
	{
		Read,
		ReadLibrary,
		ExecuteListLogic,
		CopyList,
	}

	public enum MobiusCompoundUtilService
	{
		DoesCidExist,
		SelectMoleculeFromCid,
		GetAllSaltForms,
		GetAllSaltFormsForList,
		InsertSalts,
		GroupSalts,
		ValidateList,
		GetRelatedCompoundIds,
		GetRelatedStructures,
		ExecuteSmallWorldPreviewQuery,
		SelectMoleculesForCidList
	}

	public enum MobiusMetaDataService
	{
		Build,
		GetFullTree,
		MarkCacheForRebuild,
		GetAfsSubtree,
		GetMetaTreeXml,
		GetMetaTreeXmlCompressed,
		LockAndReadMetatreeXml,
		SaveMetaTreeXml,
		ReleaseMetaTreeXml
	}

	public enum MobiusDataConnectionAdminService
	{
		DefineDataSource,
		AssociateSchema,
		LoadMetaData,
		GetOracleConnections,
		TestConnections,
		UpdateDatabaseLinks,
		CheckForConnectionLeaks,
	}

	public enum MobiusDictionaryService
	{
		GetBaseDictionaries,
		GetDictionaryByName,
	}

	public enum MobiusFileService
	{
		CopyToServer,
		CopyToClient,
		GetLastWriteTime,
		GetTempFileName,
		CanWriteFileFromServiceAccount,
	}

	public enum MobiusMetaTableService
	{
		Initialize,
		GetMetaTable,
		GetMetaTableDescription,
		GetRootTableLabels,
		GetMetaTableFromDatabaseDictionary,
		BuildFromFile,
		AddServiceMetaTable,
		RemoveServiceMetaTable,
		UpdateStats
	}

	public enum MobiusQueryEngineService
	{
		Initialize,
		CreateInstance,
		DisposeInstance,
		SetParameter,
		GetSummarizationDetailQuery,
		GetDrilldownDetailQuery,
		GetImage,
		ResolveCidListReference,
		GetRootTable,
		DoPresearchChecksAndTransforms,
		GetKeys,
		ExecuteQuery,
		RemapTablesForRetrieval, // i.e. DoPreRetrievalTransformation
		NextRowsSerialized,
		NextRow,
		Close,
		Cancel,
		GetQueryEngineState,
		TransformAndExecuteQuery,
		LogExceptionAndSerializedQuery,
		LogQueryExecutionStatistics,
		ValidateCalculatedFieldExpression,
		CreateQueryFromMQL,
		ExecuteMQLQuery,
		NextRows,
		BuildSqlStatements,
		SaveSpotfireSql,
		SaveSpotfireKeyList,
		ReadSpotfireSql,
		GetSelectAllDataQuery,
		GetStandardMobileQueries,
		GetMobileQueriesByOwner,
		GetAdditionalData,
		ExportDataToSpotfireFiles,
		CompleteRowRetrieval
	}

	public enum MobiusSecurityService
	{
		CanLogon,
		IsAdministrator,
		Authenticate,
		CreateUser,
		GetUserEmailAddress,
		GetUserInfo,
		GrantPrivilege,
		HasPrivilege,
		IsValidPrivilege,
		Logon,
		ReadUserInfo,
		RevokePrivilege,
		SetUser,
		DeleteUser,
		LoadMetadata,
	}

	public enum MobiusServerLogFileService
	{
		ResetFile,
		LogMessage,
	}

	public enum MobiusTargetAssayService
	{
		BuildTargetAssayListQuery,
		BuildTargetAssayUnsummarizedDataQuery,
		BuildTargetAssaySummarizedDataQuery,
		GetTargetDescriptionUrl,
		UpdateCommonAssayAttributes,
	}

	public enum MobiusTargetMapService
	{
		GetKeggPathway,
		GetCommonMapNames,
		GetTargetNamesAndLabels,
		GetMap,
		GetMapWithCoords,
		GetMapImage,
	}

	public enum MobiusTaskSchedulerService
	{
		Execute,
		SubmitJob,
		CheckJobStatus,
	}

	public enum MobiusUalUtilService
	{
		GetServicesIniFile,
		SelectOracleBlob,
		SelectOracleClob,
	}

	public enum MobiusUserCmpndDbService // was MobiusUcdbService, model calculation methods no longer supported
	{
		CanModifyDatabase,
		SelectDatabaseHeaderByDatabaseId,
		SelectDatabaseHeader,
		SelectDatabaseHeaders,
		SelectDatabaseHeadersByOwner,
		SelectDatabaseModels,
		SelectDatabaseExtStringCids,
		SelectDatabaseCompoundByCompoundId,
		SelectDatabaseCompound,
		SelectDatabaseCompounds,
		SelectDatabaseCompoundCount,
		GetMaxCompoundId,
		InsertDatabaseHeader,
		UpdateDatabaseHeader,
		UpdateDatabaseModelAssoc,
		UpdateDatabaseModelResultsByDatabaseId,
		UpdateDatabaseModelResultsByDatabaseIdAndModelId,
		UpdateCompoundModelResultsByCompoundId,
		UpdateCompoundModelResultsByCompoundIdAndModelId,
		UpdateDatabaseCompounds,
		UpdatePendingModelResults,
		DeleteDatabaseCompounds,
		DeleteDatabase,
		SelectUsersWithFailedUpdates,
		SelectAllFailedUpdates,
		SelectFailedUpdates,
		UpdateIsRunningByDatabaseId,
		UpdateIsRunning,
		UpdateIsPendingByDatabaseId,
		UpdateIsPending,
		UpdateDatabase,
		TestModelService,
		LogMessage,
	}

	public enum MobiusUsageService
	{
		LogEvent,
		AnalyzeUsageData,
		GetCurrentSessionCount,
		GetLastBootUpTime
	}

	public enum MobiusUserObjectService
	{
		Ping,
		ReadMultiple1,
		ReadMultiple2,
		ReadMultiple3,
		ReadMultiple4,
		ReadMultiple5,
		GetUserParameter,
		SetUserParameter,
		Exists,
		ReadHeader1,
		ReadHeader2,
		UpdateHeader,
		UpdateUpdateDateAndCount,
		GetNextId,
		Write,
		Read1,
		Read2,
		Read3,
		Delete,
		FindQueryByNameSubstring,
		ReadQueryWithMetaTables1,
		ReadQueryWithMetaTables2,
		AssureAccessToSharedQueryUserObjects,
		GetSavedQueryMQL,
		AvailableEnum24,
		SetMobileDefaultQueryForOwner,
		GetDefaultQuery
	}

	public enum MobiusTargetResultsViewerService
	{
		CreateSpotfireAnalysisDocument
	}

}
