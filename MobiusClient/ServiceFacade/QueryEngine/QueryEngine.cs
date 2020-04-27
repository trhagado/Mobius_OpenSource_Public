using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;
using Mobius.QueryEngineLibrary;
using Mobius.ServiceFacade;
using Qel = Mobius.QueryEngineLibrary;
using Mobius.Services.Native;
using Mobius.Services.Native.ServiceOpCodes;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.IO;
using System.ServiceModel;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

using ServiceTypes = Mobius.Services.Types;

namespace Mobius.ServiceFacade
{
	public class QueryEngine
	{
		private int InstanceId = -1; // id for this instance if using remote services
		private Qel.QueryEngine Qe; // ref to real query engine if not using remote services

		public int Id
		{
			get
			{
				if (Qe != null) return Qe.Id;
				else return InstanceId;
			}
		}

		private Query Query; // current query being executed

		private List<object[]> NextRowBuffer;

		public int NextRowsMin = 1; // minimum number of rows to prefetch
		public int NextRowsMax = 1000; // maximum number of rows to prefetch
		public int NextRowsMaxTime = 2000; // max time in milliseconds for next fetch

		public Stopwatch SearchTimer = null;
		public int KeyCount = 0;

		public Stopwatch RetrieveTimer = null;
		public int RowsRetrievedCount = 0;

		public QueryEngine()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusQueryEngineService,
					MobiusQueryEngineService.CreateInstance,
					null);
				InstanceId = (int)resultObject.Value;
			}

			else
			{
				Qe = new Qel.QueryEngine(); // save ref
			}
		}

/// <summary>
/// Create a new Service Facade QueryEngine from and existing Qel.QueryEngine
/// </summary>
/// <param name="QeId"></param>
/// <param name="qelQe"></param>

		public QueryEngine(
			int qeId,
			Qel.QueryEngine qelQe)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				if (qeId < 0) throw new Exception("Undefined qeId");
				InstanceId = qeId; 
			}

			else
			{
				if (qelQe == null) throw new Exception("Undefined qelQe");
				Qe = qelQe; // save ref
			}
		}

		/// <summary>
		/// Initialize for session
		/// </summary>

		public static void InitializeForSession()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusQueryEngineService,
								(int)Services.Native.ServiceOpCodes.MobiusQueryEngineService.Initialize,
								null);
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				return;
			}

			else
			{
				Qel.QueryEngine.InitializeForSession();
			}
		}

		/// <summary>
		/// Build query to select all data for a compound number
		/// </summary>
		/// <param name="keyMt">Key metatable. If null then try to determine from key value</param>
		/// <param name="cn"></param>
		/// <returns></returns>
		
		public static Query GetSelectAllDataQuery(
			string keyMtName,
			string cn)
		{
			Query q;

			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusQueryEngineService,
								(int)Services.Native.ServiceOpCodes.MobiusQueryEngineService.GetSelectAllDataQuery,
								new Services.Native.NativeMethodTransportObject(new object[] { keyMtName, cn }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();

				if (resultObject.Value == null) return null;

				q = Query.Deserialize((string)resultObject.Value);
				return q;
			}

			else 
			{
				q = Qel.QueryEngine.GetSelectAllDataQuery(keyMtName, cn);
				return q;
			}
		}

/// <summary>
/// Set a static QueryEngine parameter
/// </summary>
/// <param name="parm"></param>
/// <param name="value"></param>

		public static void SetParameter(
			string parm,
			string value)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusQueryEngineService,
								(int)Services.Native.ServiceOpCodes.MobiusQueryEngineService.SetParameter,
								new Services.Native.NativeMethodTransportObject(new object[] { parm, value }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				return;
			}

			else Qel.QueryEngine.SetParameter(parm, value);
		}

		/// <summary>
		/// Build query to retrieve detailed data for a summarization
		/// </summary>
		/// <param name="metaTableName"></param>
		/// <param name="metaColumnName"></param>
		/// <param name="resultId"></param>
		/// <returns></returns>

		public static Query GetSummarizationDetailQuery(
				string metaTableName,
				string metaColumnName,
				int level,
				string resultId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				int instanceId = -1; // used for backward compatibility

				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusQueryEngineService,
								(int)Services.Native.ServiceOpCodes.MobiusQueryEngineService.GetSummarizationDetailQuery,
								new Services.Native.NativeMethodTransportObject(new object[] {
                                instanceId, metaTableName, metaColumnName, level, resultId
                            }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject.Value == null) return null;

				Query result = Query.Deserialize((string)resultObject.Value);
				return result;
			}

			else
			{
				Query q = Qel.QueryEngine.GetSummarizationDetailQuery(metaTableName, metaColumnName, level, resultId);
				return q;
			}
		}

		/// <summary>
		/// Build query to retrieve detailed data for a summarization
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="mc"></param>
		/// <param name="resultId"></param>
		/// <returns></returns>

		public static Query GetDrilldownDetailQuery(
				MetaTable mt,
				MetaColumn mc,
				int level,
				string resultId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				int instanceId = -1; // used for backward compatibility
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusQueryEngineService,
								(int)Services.Native.ServiceOpCodes.MobiusQueryEngineService.GetDrilldownDetailQuery,
								new Services.Native.NativeMethodTransportObject(new object[] {
																instanceId, mt.Name, mc.Name, level, resultId
														}));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject.Value == null)
				{
					return null;
				}
				else
				{
					Query result = Query.Deserialize((string)resultObject.Value);
					return result;
				}
			}
			else return Qel.QueryEngine.GetDrilldownDetailQuery(mt, mc, level, resultId);
		}

		/// <summary>
		/// Retrieve an image
		/// </summary>
		/// <param name="metaColumn"></param>
		/// <param name="resultId"></param>
		/// <param name="desiredWidth"></param>
		/// <param name="formattingParm2"></param>
		/// <returns></returns>

		public Bitmap GetImage(
				MetaColumn metaColumn,
				string graphicsIdString,
				int desiredWidth)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				//ServiceTypes.MetaColumn serviceMC = ServiceFacade.TypeConversionHelper.Convert<MetaColumn, ServiceTypes.MetaColumn>(metaColumn);
				string mtMcName = metaColumn.MetaTableDotMetaColumnName; // just pass name string now

				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusQueryEngineService,
								(int)Services.Native.ServiceOpCodes.MobiusQueryEngineService.GetImage,
								new Services.Native.NativeMethodTransportObject(new object[] {
                                InstanceId, mtMcName, graphicsIdString, desiredWidth
                            }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				Bitmap result = (resultObject != null) ? (Bitmap)(resultObject.Value as Mobius.Services.Types.Bitmap) : null;
				return result;
			}
			
			else return Qe.GetImage(metaColumn, graphicsIdString, desiredWidth);
		}

		/// <summary>
		/// Get additional data 
		/// </summary>
		/// <param name="metaColumn"></param>
		/// <param name="command"></param>
		/// <returns></returns>

		public object GetAdditionalData(
			string command)
		{
			object result = null;
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusQueryEngineService,
								(int)Services.Native.ServiceOpCodes.MobiusQueryEngineService.GetAdditionalData,
								new Services.Native.NativeMethodTransportObject(new object[] { InstanceId, command }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				result = (resultObject != null) ? resultObject.Value : null;
				return result;
			}

			else
			{
				result = Qe.GetAdditionalData(command);
				return result;
			}
		}

		/// <summary>
		/// Examine a text reference to a cnList & try to resolve to an existing cnList
		/// </summary>
		/// <param name="tok"></param>
		/// <returns></returns>

		public static UserObject ResolveCnListReference(
				string tok)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusQueryEngineService,
								(int)Services.Native.ServiceOpCodes.MobiusQueryEngineService.ResolveCidListReference,
								new Services.Native.NativeMethodTransportObject(new object[] { tok }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				ServiceTypes.UserObjectNode uoNode = (resultObject != null) ? (ServiceTypes.UserObjectNode)resultObject.Value : null;
				UserObject result = ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.UserObjectNode, UserObject>(uoNode);
				return result;
			}
			else return Qel.QueryEngine.ResolveCidListReference(tok);
		}

		/// <summary>
		/// Get root table for query or null > 1 root table
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>

		public static MetaTable GetRootTable(
				Query query)
		{ // may want to modify this one
			if (ServiceFacade.UseRemoteServices)
			{
				string serializedQuery = query.Serialize();
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusQueryEngineService,
								(int)Services.Native.ServiceOpCodes.MobiusQueryEngineService.GetRootTable,
								new Services.Native.NativeMethodTransportObject(new object[] { serializedQuery }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				string serializedMetaTable = (resultObject != null) ? (string)resultObject.Value : null;
				MetaTable result = MetaTable.Deserialize(serializedMetaTable);
				return result;
			}
			else
			{
				Query qClone = query.Clone();
				return Qel.QueryEngine.GetRootTable(qClone);
			}
		}

		/// <summary>
		/// Examine query & do any required presearch transforms
		/// </summary>
		/// <returns></returns>

		public static Query DoPresearchTransformations( // ExpandMultitableTables(
				Query q)
		{
			Query nq = null;
			if (ServiceFacade.UseRemoteServices)
			{
				string serializedQuery = q.Serialize();
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
					ServiceFacade.InvokeNativeMethod(nativeClient,
					(int)Services.Native.ServiceCodes.MobiusQueryEngineService,
					(int)Services.Native.ServiceOpCodes.MobiusQueryEngineService.DoPresearchChecksAndTransforms,
					new Services.Native.NativeMethodTransportObject(new object[] { serializedQuery }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				serializedQuery = (string)resultObject.Value;
				if (serializedQuery != null)
					nq = Query.Deserialize(serializedQuery);
			}

			else
			{
				string serializedQuery = q.Serialize(); // serialize/deserialize to simulate service
				Query qClone = Query.Deserialize(serializedQuery);
				nq = Qel.QueryEngine.DoPreSearchTransformations(qClone);
			}

			q.PresearchDerivedQuery = nq;
			if (nq != null) // link queries if derived query created
				nq.PresearchBaseQuery = q;

			return nq;
		}

		/// <summary>
		/// Get the keys
		/// </summary>
		/// <returns></returns>

		public List<string> GetKeys()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusQueryEngineService,
								(int)Services.Native.ServiceOpCodes.MobiusQueryEngineService.GetKeys,
								new Services.Native.NativeMethodTransportObject(new object[] { InstanceId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				List<string> result = (resultObject != null) ? (List<string>)resultObject.Value : null;
				return result;
			}
			else return Qe.GetKeys();
		}

		/// <summary>
		/// Run the specified query
		/// </summary>
		/// <param name="query"></param>
		/// <returns>
		/// List of results keys
		/// </returns>
		/// 

		public List<string> ExecuteQuery(
				Query query)
		{
			Query = query;
			KeyCount = RowsRetrievedCount = 0;
			SearchTimer = Stopwatch.StartNew();
			RetrieveTimer = null;

			List<string> keyList = null;

			try
			{
				if (ServiceFacade.UseRemoteServices)
				{
					string serializedQuery = query.Serialize();

					NativeMethodTransportObject resultObject =		ServiceFacade.CallServiceMethod(
						ServiceCodes.MobiusQueryEngineService,
						MobiusQueryEngineService.ExecuteQuery,
						new object[] { InstanceId, serializedQuery });

					if (resultObject == null) return null;
					keyList = (List<string>)resultObject.Value;
				}

				else // local
				{
					string serializedQuery = query.Serialize(); // serialize/deserialize to simulate service
					Query qClone = Query.Deserialize(serializedQuery);
					keyList = Qe.ExecuteQuery(qClone);

				}

				SearchTimer.Stop();
				if (keyList != null) KeyCount = keyList.Count;

				return keyList;
			}

			finally
			{
				query.UseResultKeys = false; // reset UseResultKeys since only applies to a single ExecuteQuery call 
			}
		}


/// <summary>
/// Transform and execute a query
/// </summary>
/// <param name="q">The source query</param>
/// <param name="q2">Transformed query or null if original query not transformed</param>
/// <returns>List of results keys</returns>

		public List<string> TransformAndExecuteQuery(
			Query q,
			out Query nq)
		{
			KeyCount = RowsRetrievedCount = 0;
			SearchTimer = Stopwatch.StartNew();
			RetrieveTimer = null;

			List<string> keyList = null;
			Query = q;
			nq = null;

			try
			{
				if (ServiceFacade.UseRemoteServices)
				{
					string serializedQuery = q.Serialize();

					NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
						ServiceCodes.MobiusQueryEngineService,
						MobiusQueryEngineService.TransformAndExecuteQuery,
						new object[] { InstanceId, serializedQuery });

					if (resultObject == null ||
						!(resultObject.Value is object[])) return null;


					object[] sa = resultObject.Value as object[];
					if (sa == null || sa.Length != 2) return null;

					keyList = sa[0] as List<string>;
					string nqString = sa[1] as string;
					if (Lex.IsDefined(nqString)) // transformed query returned?
						nq = Query.Deserialize(nqString);
				}

				else // local
				{
					string serializedQuery = q.Serialize(); // serialize/deserialize to simulate service
					Query qClone = Query.Deserialize(serializedQuery);
					keyList = Qe.TransformAndExecuteQuery(qClone, out nq);
					if (nq != null)
					{
						serializedQuery = nq.Serialize(true, true); // serialize/deserialize to simulate service
						nq = Query.Deserialize(serializedQuery);
					}

				}

				if (nq != null)
				{
					q.PresearchDerivedQuery = nq;
					Query = nq;
				}
				else q.PresearchDerivedQuery = null;

				SearchTimer.Stop();
				if (keyList != null) KeyCount = keyList.Count;

				return keyList;
			}

			finally
			{
				q.UseResultKeys = false; // reset UseResultKeys since only applies to a single ExecuteQuery call 
			}
		}


/// <summary>
/// Build the SQL for a query - May be several parts
/// </summary>
/// <param name="query"></param>
/// <returns></returns>

		public string BuildSqlStatements(
			Query query)
		{
			string sqlList;

			if (ServiceFacade.UseRemoteServices)
			{
				string serializedQuery = query.Serialize();

				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusQueryEngineService,
					MobiusQueryEngineService.BuildSqlStatements,
					new object[] { InstanceId, serializedQuery });

				if (resultObject == null) return "";

				sqlList = (string)resultObject.Value;
				return sqlList;
			}

			else
			{
				sqlList = Qe.BuildSqlStatements(query);
				return sqlList;
			}
		}

		/// <summary>
		/// Export data retrieved by query to Spotfire files
		/// </summary>
		/// <param name="exportFileFormat"></param>
		/// <param name="fileName"></param>
		/// <param name="fileName2"></param> 
		/// <param name="exportStructureFormat"></param>
		/// <param name="splitQNs"></param>
		/// <returns></returns>

		public QueryEngineStats ExportDataToSpotfireFiles(
			ExportParms ep)
		{
			QueryEngineStats qeStats = null;

			if (ServiceFacade.UseRemoteServices)
			{
				string serializedEp = ep.Serialize();
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusQueryEngineService,
					MobiusQueryEngineService.ExportDataToSpotfireFiles,
					new object[] { InstanceId, serializedEp });

				if (resultObject?.Value == null) return null;
				string txt = resultObject.Value as string;
				qeStats = QueryEngineStats.Deserialize(txt);

				return qeStats;
			}

			else
			{
				qeStats = Qe.ExportDataToSpotfireFiles(ep);
			}

			return qeStats;
		}

		/// <summary>
		/// Build and save SQL for use in Spotfire information link
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>

		public static int SaveSpotfireSql(
			string sqlStmtName,
			Query query)
		{
			int stmtId = SaveSpotfireSql(sqlStmtName, 0, query);
			return stmtId;
		}

		/// <summary>
		/// Build and save SQL for use in Spotfire information link
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>

		public static int SaveSpotfireSql(
			string sqlStmtName,
			int version,
			Query query)
		{
			int stmtId;

			if (ServiceFacade.UseRemoteServices)
			{
				string serializedQuery = query.Serialize();

				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusQueryEngineService,
					MobiusQueryEngineService.SaveSpotfireSql,
					new object[] { sqlStmtName, serializedQuery });

				if (resultObject == null) return -1;

				stmtId = (int)resultObject.Value;
				return stmtId;
			}

			else
			{
				stmtId = Qel.QueryEngine.SaveSpotfireSql(sqlStmtName, query);
				return stmtId;
			}
		}

/// <summary>
/// Save a list of keys for later inclusion in SQL statements
/// </summary>
/// <param name="listNamePrefix"></param>
/// <param name="listString"></param>
/// <returns></returns>

		public static string SaveSpotfireKeyList(
			string listNamePrefix,
			string listString)
		{
			string keyColName = "";
			string listName = SaveSpotfireKeyList(keyColName, listNamePrefix, listString);
			return listName;
		}

		/// <summary>
		/// Save a list of keys for later inclusion in SQL statements
		/// </summary>
		/// <param name="listString"></param>
		/// <returns></returns>

		public static string SaveSpotfireKeyList(
			string keyColName,
			string listNamePrefix,
			string listString)
		{
			string listName;

			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusQueryEngineService,
					MobiusQueryEngineService.SaveSpotfireKeyList,
					new object[] { keyColName, listNamePrefix, listString });

				if (resultObject == null) return null;

				listName = (string)resultObject.Value;
				return listName;
			}

			else
			{
				listName = Qel.QueryEngine.SaveSpotfireKeyList(keyColName, listNamePrefix, listString);
				return listName;
			}
		}

/// <summary>
/// Read saved sql
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public static string ReadSpotfireSql(
			string sqlStmtName)
		{
			string sql = ReadSpotfireSql(sqlStmtName, 0);
			return sql;
		}

/// <summary>
/// Read saved sql
/// </summary>
/// <param name="sqlStmtName"></param>
/// <returns></returns>

		static public string ReadSpotfireSql(
			string sqlStmtName,
			int version)
		{
			int stmtId;

			if (ServiceFacade.UseRemoteServices)
			{

				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusQueryEngineService,
					MobiusQueryEngineService.ReadSpotfireSql,
					new object[] { sqlStmtName, version });

				if (resultObject == null) return null;

				string spotfireSql = resultObject.Value as string;
				return spotfireSql;
			}

			else
			{
				string spotfireSql = Qel.QueryEngine.ReadSpotfireSql(sqlStmtName, version);
				return spotfireSql;
			}
		}

		/// <summary>
		/// Examine query & expand any tables marked for remapping
		/// Existing remap place holder tables are removed & expansion tables
		/// are added at the end of the query in alphabetical order sorted across
		/// all added tables.
		/// </summary>
		/// <returns></returns>

		public Query DoPreRetrievalTransformation()
		{
			Query q;
			string serializedQuery;

			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusQueryEngineService,
								(int)Services.Native.ServiceOpCodes.MobiusQueryEngineService.RemapTablesForRetrieval,
								new Services.Native.NativeMethodTransportObject(new object[] { InstanceId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				serializedQuery = (string)resultObject.Value;
				if (String.IsNullOrEmpty(serializedQuery))
				{
					return null;
				}
				else
				{
					Query result = Query.Deserialize(serializedQuery);
					return result;
				}
			}

			else
			{
				//if (UseSelfSerialization)
				//{
				//    serializedQuery = Qe.RemapTablesForRetrievalSerialized();
				//    if (String.IsNullOrEmpty(serializedQuery)) return null;

				//    q = Query.Deserialize(serializedQuery);
				//}

				//else q = Qe.RemapTablesForRetrieval();

				q = Qe.DoPreRetrievalTableExpansions();
				return q;

				//int t0 = TimeOfDay.Milliseconds(); // timing test
				//q.SerializeMetaTables = true;
				//string qs = q.Serialize();
				//t0 = TimeOfDay.Milliseconds() - t0; // timing test
				//DebugLog.Message("Serialize length = " + qs.Length + ", time = " + t0); // 234 milliseconds for 406 tables

				//t0 = TimeOfDay.Milliseconds(); // timing test
				//Services.Types.Query serviceQuery = ServiceFacade.TypeConversionHelper.Convert<Query, ServiceTypes.Query>(q);
				//t0 = TimeOfDay.Milliseconds() - t0;
				//DebugLog.Message("Serialize time " + t0); // 23 seconds for 406 tables

				//return q;
			}
		}

		/// <summary>
		/// Read a list of rows 
		/// </summary>
		/// <param name="minRows">Minimum number of rows to read</param>
		/// <param name="maxRows">Maximum number of rows to read</param>
		/// <param name="maxTime">Maximum time in milliseconds provided at least one row is returned</param>
		/// <returns>List of value object arrays</returns>

			public List<object[]> NextRows(
			int minRows,
			int maxRows,
			int maxTime)
			{
				List<object[]> rows = NextRows(minRows, maxRows, maxTime, null);
				return rows;
			}

		/// <summary>
		/// Read a list of rows 
		/// </summary>
		/// <param name="minRows">Minimum number of rows to read</param>
		/// <param name="maxRows">Maximum number of rows to read</param>
		/// <param name="maxTime">Maximum time in milliseconds provided at least one row is returned</param>
		/// <param name="returnQeStats">If true additional rows of broker stats are returned</param>
		/// <returns>List of value object arrays</returns>

		public List<object[]> NextRows(
			int minRows,
			int maxRows,
			int maxTime,
			List<MetaBrokerStats> mbStats)
		{
			byte[] ba;
			List<object[]> rows = null;

			if (RowsRetrievedCount == 0) RetrieveTimer = Stopwatch.StartNew();

			try
			{
				if (ServiceFacade.UseRemoteServices)
				{
					bool returnQeStats = (mbStats != null);

					NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
						ServiceCodes.MobiusQueryEngineService,
						MobiusQueryEngineService.NextRowsSerialized,
						new object[] { InstanceId, minRows, maxRows, maxTime, returnQeStats });

					if (resultObject.Value == null) return null;

					else if (!returnQeStats)
						ba = (byte[])resultObject.Value;

					else // return rows & stats
					{
						object[] oa = (object[])resultObject.Value;
						ba = (byte[])oa[0]; // first element is byte array of serialized rows
						string txt = (string)oa[1];
						List<MetaBrokerStats> mbStats2 = MetaBrokerStats.DeserializeList(txt);
						mbStats.Clear();
						mbStats.AddRange(mbStats2);
					}

					rows = VoArray.DeserializeByteArrayToVoArrayList(ba);
				}

				// Local services

				else
				{
					rows = Qe.NextRows(minRows, maxRows, maxTime, mbStats);
					if (rows == null) return null;

					ba = VoArray.SerializeBinaryVoArrayListToByteArray(rows);  // serialize & deserialize to simulate service
					rows = VoArray.DeserializeByteArrayToVoArrayList(ba);

					if (mbStats != null)
					{
						string mbStatsString = MetaBrokerStats.SerializeList(mbStats);
						mbStats = MetaBrokerStats.DeserializeList(mbStatsString);
					}

				}

				if (rows != null)
				{
					RowsRetrievedCount += rows.Count;
					CacheStructures(rows); // do local caching of chemical structures if requested
				}

				return rows;
			}

			catch (Exception ex)
			{
				string msg = DebugLog.FormatExceptionMessage(ex);
				ServicesLogFile.Message(msg, CommonConfigInfo.ServicesLogFileName); // try to log on server

				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// Do local caching of chemical structures if requested
		/// </summary>
		/// <param name="rows"></param>

		void CacheStructures(
			List<object[]> rows)
		{
			CompoundId cidMx;
			MoleculeMx cs;
			object vo;
			string cid;

			if (!MoleculeCache.Enabled) return;

			if (rows == null || rows.Count == 0) return;

			if (Query?.Tables == null || Query.Tables.Count < 1) return;
			QueryTable qt = Query.Tables[0];
			MetaTable mt = qt.MetaTable;
			if (!mt.IsRootTable) return; // 1st table must be root table

			bool isUcdb = (mt != null && mt.Root.IsUserDatabaseStructureTable); // user compound database not added
			if (isUcdb) return; 

			QueryColumn qc = qt.FirstStructureQueryColumn;
			if (qc == null || !qc.Selected) return;

			int strVoi = qc.VoPosition;
			if (strVoi < 0 || strVoi >= rows[0].Length) return;

			foreach (object[] row in rows)
			{
				vo = row[0]; // get cid
				if (vo == null) continue;
				if (vo.GetType() == typeof(CompoundId))
				{
					cidMx = vo as CompoundId;
					cid = cidMx.Value;
				}

				else cid = vo.ToString();
				if (Lex.IsUndefined(cid)) continue;

				vo = row[strVoi]; // get structure
				if (vo == null) continue;
				if (vo.GetType() == typeof(MoleculeMx))
				{
					cs = vo as MoleculeMx;
					MoleculeCache.AddMolecule(cid, cs);
				}
			}
		}

		/// <summary>
		/// Return the next matching row value object
		/// </summary>

		public object[] NextRow()
		{
			if (NextRowBuffer == null || NextRowBuffer.Count == 0)
			{
				NextRowBuffer = NextRows(NextRowsMin, NextRowsMax, NextRowsMaxTime);
				if (NextRowBuffer == null || NextRowBuffer.Count == 0) return null;
                NextRowsMaxTime = 10000; // let's try 10 seconds
            }

			object[] row = NextRowBuffer[0];
			//if (row[113] != null) row = row; // debug

			NextRowBuffer.RemoveAt(0);
			return row;
		}

		/// <summary>
		/// Complete retrieval of all rows and hold within the QE
		/// </summary>
		/// <returns></returns>

		public QueryEngineStats CompleteRowRetrieval()
		{
			QueryEngineStats qeStats = null;

			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusQueryEngineService,
					MobiusQueryEngineService.CompleteRowRetrieval,
					new object[] { InstanceId });

				if (resultObject == null) return new QueryEngineStats();

				string txt = (string)resultObject.Value;
				qeStats = QueryEngineStats.Deserialize(txt);

				return qeStats;
			}

			else
			{
				qeStats = Qe.CompleteRowRetrieval();
			}

			return qeStats;
		}

		/// <summary>
		/// Close the query
		/// </summary>

		public void Close()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusQueryEngineService,
					MobiusQueryEngineService.Close,
					new object[] { InstanceId });
			}

			else
			{
				if (Qe != null)
				{
					Qe.Close();
					Qe = null; // allows memory to be reclaimed for the real QE even if the Facade QE is not Disposed
				}

				InstanceId = -1;
			}
		}

		/// <summary>
		/// Dispose of the query engine instance
		/// </summary>

		public void Dispose()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusQueryEngineService,
								(int)Services.Native.ServiceOpCodes.MobiusQueryEngineService.DisposeInstance,
								new Services.Native.NativeMethodTransportObject(new object[] { InstanceId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
			}
			else
			{
				Qe.Dispose();
			}
		}

		/// <summary>
		/// State of the search
		/// </summary>

		public QueryEngineState State
		{
			get
			{
				if (ServiceFacade.UseRemoteServices)
				{
					Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
					Services.Native.NativeMethodTransportObject resultObject =
							ServiceFacade.InvokeNativeMethod(nativeClient,
									(int)Services.Native.ServiceCodes.MobiusQueryEngineService,
									(int)Services.Native.ServiceOpCodes.MobiusQueryEngineService.GetQueryEngineState,
									new Services.Native.NativeMethodTransportObject(new object[] { InstanceId }));
					((System.ServiceModel.IClientChannel)nativeClient).Close();
					if (resultObject == null) return QueryEngineState.Closed;
					ServiceTypes.QueryEngineState serviceQEState = (ServiceTypes.QueryEngineState)resultObject.Value;
					QueryEngineState result = ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.QueryEngineState, QueryEngineState>(serviceQEState);
					return result;
				}
				else return Qe.State;
			}
		}


		/// <summary>
		/// Cancel execution of the query
		/// </summary>
		/// <returns></returns>

		public void Cancel(bool waitForCompletion)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusQueryEngineService,
								(int)Services.Native.ServiceOpCodes.MobiusQueryEngineService.Cancel,
								new Services.Native.NativeMethodTransportObject(new object[] { InstanceId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
			}
			else Qe.Cancel(waitForCompletion);
		}

		/// <summary>
		/// Check that the specified calculated field expression is valid
		/// </summary>
		/// <param name="advExpr"></param>
		/// <returns></returns>

		public static string ValidateCalculatedFieldExpression(
			string advExpr)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();

				Services.Native.NativeMethodTransportObject resultObject =
					ServiceFacade.InvokeNativeMethod(nativeClient,
					(int)Services.Native.ServiceCodes.MobiusQueryEngineService,
					(int)Services.Native.ServiceOpCodes.MobiusQueryEngineService.ValidateCalculatedFieldExpression,
					new Services.Native.NativeMethodTransportObject(new object[] { advExpr }));

				((System.ServiceModel.IClientChannel)nativeClient).Close();

				string msg = "";
				if (resultObject != null && resultObject.Value != null)
					msg = resultObject.Value.ToString();
				return msg;
			}

			else
			{
				return Qel.QueryEngine.ValidateCalculatedFieldExpression(advExpr);
			}
		}

		/// <summary>
		/// Write exception and associated serialized query to log files
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="query"></param>
		/// <returns></returns>

		public static string LogExceptionAndSerializedQuery(
			string exMsg,
			Query query)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				string serializedQuery = query.Serialize();

				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusQueryEngineService,
								(int)Services.Native.ServiceOpCodes.MobiusQueryEngineService.LogExceptionAndSerializedQuery,
								new Services.Native.NativeMethodTransportObject(new object[] { exMsg, serializedQuery }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();

				string msg = "";
				if (resultObject != null && resultObject.Value != null)
					msg = resultObject.Value.ToString();
				return msg;
			}

			else
			{
				Query qClone = query.Clone();
				return Qel.QueryEngine.LogExceptionAndSerializedQuery(exMsg, qClone);
			}
		}

		/// <summary>
		/// Log query execution stats
		/// </summary>

		public static void LogQueryExecutionStatistics()
		{
			if (ServiceFacade.UseRemoteServices)
			{

				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusQueryEngineService,
								(int)Services.Native.ServiceOpCodes.MobiusQueryEngineService.LogQueryExecutionStatistics,
								new Services.Native.NativeMethodTransportObject(new object[0]));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				return;
			}

			else
			{
				Qel.QueryEngine.LogQueryExecutionStatistics();
			}
		}
	}
}
