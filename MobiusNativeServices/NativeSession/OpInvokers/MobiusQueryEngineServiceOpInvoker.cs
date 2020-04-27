using System;
using System.Collections.Generic;
using Mobius.Data;
using Mobius.ServiceFacade;
using Mobius.Services.Types;
using Mobius.Services.Util.TypeConversionUtil;
using Qel = Mobius.QueryEngineLibrary;
using Mobius.Services.Native.ServiceOpCodes;
using MetaColumn = Mobius.Services.Types.MetaColumn;
using Query = Mobius.Services.Types.Query;
using QueryEngineState = Mobius.Services.Types.QueryEngineState;

namespace Mobius.Services.Native.OpInvokers
{
	public class MobiusQueryEngineServiceOpInvoker : IInvokeServiceOps
	{
		private TypeConversionHelper _transHelper = new TypeConversionHelper(TypeConversionHelper.ContextRetentionMode.NoRetention);

		object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
		{
			MobiusQueryEngineService op = (MobiusQueryEngineService)opCode;
			switch (op)
			{
				case MobiusQueryEngineService.Initialize:
					{
						Qel.QueryEngine.InitializeForSession();
						return null;
					}

				case MobiusQueryEngineService.CreateInstance:
					{
						Qel.QueryEngine instance = new Mobius.QueryEngineLibrary.QueryEngine();
						return instance.Id;
					}

				case MobiusQueryEngineService.DisposeInstance:
					{
						int instanceId = (int)args[0];
						bool disposed = Qel.QueryEngine.Dispose(instanceId);
						return disposed;
					}

				case MobiusQueryEngineService.SetParameter:
					{
						string parm = "", value = "";

						if (args.Length == 2)
						{
							parm = args[0].ToString();
							value = args[1].ToString();
						}

						else // old form (remove when all old clients are updated)
						{
							parm = "DatabaseSubset";
							value = args[0].ToString();
						}

						Qel.QueryEngine.SetParameter(parm, value);
						return null;
					}

				case MobiusQueryEngineService.GetSummarizationDetailQuery:
					{
						int instanceId = (int)args[0];
						string metaTableName = (string)args[1];
						string metaColumnName = (string)args[2];
						int level = (int)args[3];
						string resultId = (string)args[4];

						//Qel.QueryEngine qe = null;
						//lock (Qel.QueryEngine.IdToInstanceDict)
						//{
						//	if (Qel.QueryEngine.IdToInstanceDict.ContainsKey(instanceId))
						//	{
						//		qe = Qel.QueryEngine.IdToInstanceDict[instanceId];
						//	}
						//}
						//if (qe != null)
						//{

						string queryXml = "";
						Data.Query query = QueryEngine.GetSummarizationDetailQuery(metaTableName, metaColumnName, level, resultId);
						if (query != null) queryXml = query.Serialize(false);
						return queryXml;

						//}
						//else
						//{
						//	throw new ArgumentException("Not a valid query engine instance id!");
						//}
					}
				case MobiusQueryEngineService.GetDrilldownDetailQuery:
					{
						int instanceId = (int)args[0];
						string metaTableName = (string)args[1];
						string metaColumnName = (string)args[2];
						int level = (int)args[3];
						string resultId = (string)args[4];

						//Qel.QueryEngine qe = null;
						//lock (Qel.QueryEngine.IdToInstanceDict)
						//{
						//	if (Qel.QueryEngine.IdToInstanceDict.ContainsKey(instanceId))
						//	{
						//		qe = Qel.QueryEngine.IdToInstanceDict[instanceId];
						//	}
						//}
						//if (qe != null)
						//{

						Mobius.Data.MetaTable metaTable = Mobius.Data.MetaTableCollection.GetExisting(metaTableName);
						Mobius.Data.MetaColumn metaColumn = metaTable.GetMetaColumnByName(metaColumnName);
						Data.Query query = QueryEngine.GetDrilldownDetailQuery(metaTable, metaColumn, level, resultId);
						string queryXml = query.Serialize(false);
						return queryXml;

						//}
						//else
						//{
						//	throw new ArgumentException("Not a valid query engine instance id!");
						//}
					}

				case MobiusQueryEngineService.GetImage:
					{
						string mtMcName = null;

						int instanceId = (int)args[0];

						if (args[1] is string) // New "mtName.mcName" format for arg (Post Client 5.0)
						{
							mtMcName = (string)args[1];
						}

						else if (args[1] is MetaColumn) // old Mobius.Services.Types.MetaColumn format 
						{
							MetaColumn mc = (MetaColumn)args[1];
							mtMcName = mc.MetaTable.Name + "." + mc.Name;
						}

						else return null; // error

						Mobius.Data.MetaColumn mobiusMC = Mobius.Data.MetaColumn.ParseMetaTableMetaColumnName(mtMcName);

						string graphicsIdString = (string)args[2];
						int desiredWidth = (int)args[3];

						Qel.QueryEngine qe = GetQueryEngineInstance(instanceId);

						//Mobius.Data.MetaColumn mobiusMC = _transHelper.Convert<MetaColumn, Mobius.Data.MetaColumn>(metaColumn);
						System.Drawing.Bitmap bitmap = qe.GetImage(mobiusMC, graphicsIdString, desiredWidth);
						Types.Bitmap result = new Types.Bitmap(bitmap);
						return result;
					}

				case MobiusQueryEngineService.ResolveCidListReference:
					{
						string tok = (string)args[0];
						Mobius.Data.UserObject uo = Qel.QueryEngine.ResolveCidListReference(tok);
						UserObjectNode result = _transHelper.Convert<Mobius.Data.UserObject, UserObjectNode>(uo);
						return result;
					}

				case MobiusQueryEngineService.GetRootTable:
					{
						string queryString = (string)args[0];
						Mobius.Data.Query mobiusQuery = Data.Query.Deserialize(queryString);
						Mobius.Data.MetaTable mobiusMT = Qel.QueryEngine.GetRootTable(mobiusQuery);
						string mtString = null;
						if (mobiusMT != null)
							mtString = mobiusMT.Serialize();
						return mtString;
					}

				case MobiusQueryEngineService.DoPresearchChecksAndTransforms:
					{
						string serializedQuery = (string)args[0];
						Data.Query q = Data.Query.Deserialize(serializedQuery);
						q = Qel.QueryEngine.DoPreSearchTransformations(q);
						if (q == null) return null;
						string serializedQuery2 = q.Serialize(true);
						return serializedQuery2;
					}

				case MobiusQueryEngineService.GetKeys:
					{
						int instanceId = (int)args[0];
						Qel.QueryEngine qe = GetQueryEngineInstance(instanceId);
						List<string> keys = qe.GetKeys();
						return keys;
					}

				case MobiusQueryEngineService.ExecuteQuery:
					{
						int instanceId = (int)args[0];
						string serializedQuery = (string)args[1];
						Qel.QueryEngine qe = GetQueryEngineInstance(instanceId);
						Data.Query q = Data.Query.Deserialize(serializedQuery);
						List<string> result = qe.ExecuteQuery(q, false, false);
						return result;
					}

				case MobiusQueryEngineService.TransformAndExecuteQuery:
					{
						int instanceId = (int)args[0];
						string serializedQuery = (string)args[1];

						Qel.QueryEngine qe = GetQueryEngineInstance(instanceId);
						Data.Query q2;
						Data.Query q = Data.Query.Deserialize(serializedQuery);
						List<string> keyList = qe.TransformAndExecuteQuery(q, out q2);
						object[] sa = new object[2];
						sa[0] = keyList;
						if (q2 != null) // any transformed query?
							sa[1] = q2.Serialize(true, true); // serialize including metatables not previously send to client
						return sa;
					}

				case MobiusQueryEngineService.BuildSqlStatements:
					{
						int instanceId = (int)args[0];
						string serializedQuery = (string)args[1];

						Qel.QueryEngine qe = GetQueryEngineInstance(instanceId);
						Data.Query q = Data.Query.Deserialize(serializedQuery);
						string sqlStatements = qe.BuildSqlStatements(q);
						return sqlStatements;
					}

				case MobiusQueryEngineService.SaveSpotfireSql:
					{
						string sqlStmtName = (string)args[0];
						string serializedQuery = (string)args[1];

						Data.Query q = Data.Query.Deserialize(serializedQuery);
						int stmtId = Mobius.QueryEngineLibrary.QueryEngine.SaveSpotfireSql(sqlStmtName, q);
						return stmtId;
					}

				case MobiusQueryEngineService.SaveSpotfireKeyList:
					{
						string keyColName, listType, keyList;
						if (args.Length >= 3)
						{
							keyColName = (string)args[0];
							listType = (string)args[1];
							keyList = (string)args[2];
						}

						else if (args.Length == 2) // old form
						{
							keyColName = "CORP_ID";
							listType = (string)args[0];
							keyList = (string)args[1];
						}

						else // old form for cid list
						{
							keyColName = "CORP_ID";
							listType = "CIDLIST";
							keyList = (string)args[0];
						}

						string keyListName = Mobius.QueryEngineLibrary.QueryEngine.SaveSpotfireKeyList(keyColName, listType, keyList);
						return keyListName;
					}

				case MobiusQueryEngineService.ReadSpotfireSql:
					{
						string sqlStmtName = args[0] as string;
						int version = (int)args[1];
						string sql = Mobius.QueryEngineLibrary.QueryEngine.ReadSpotfireSql(sqlStmtName, version);
						return sql;
					}

				case MobiusQueryEngineService.RemapTablesForRetrieval: // DoPreRetrievalTransformation
					{
						int instanceId = (int)args[0];

						Qel.QueryEngine qe = GetQueryEngineInstance(instanceId);
						Mobius.Data.Query q = qe.DoPreRetrievalTableExpansions();
						if (q == null) return null;

						string serializedQuery = q.Serialize(true); // serialize including metatables not previously included
						return serializedQuery;
					}

				case MobiusQueryEngineService.NextRowsSerialized:
					{
						int minRows = 1;
						bool returnQeStats = false;
						List<Mobius.Data.MetaBrokerStats> mbStats = null;

						int instanceId = (int)args[0];
						int ai = 1;
						if (args.Length >= 4) // newer version with minRows arg
							minRows = (int)args[ai++];

						int maxRows = (int)args[ai++];
						int maxTime = (int)args[ai++];

						if (ai < args.Length) // newer version with returnQeStats arg
							returnQeStats = (bool)args[ai++];

						Qel.QueryEngine qe = GetQueryEngineInstance(instanceId);

						if (returnQeStats) mbStats = new List<Mobius.Data.MetaBrokerStats>();

						byte[] serializedRows = qe.NextRowsSerialized(minRows, maxRows, maxTime, mbStats);

						if (!returnQeStats) return serializedRows;

						else // return serialized rows and stats in a two-element object array
						{
							object[] oa = new object[2];
							oa[0] = serializedRows;
							oa[1] = Mobius.Data.MetaBrokerStats.SerializeList(mbStats);
							return oa;
						}
					}

				case MobiusQueryEngineService.NextRows:
					{
						int minRows = 1;

						int instanceId = (int)args[0];
						int ai = 1;
						if (args.Length >= 4) // newer version with minRows arg
							minRows = (int)args[ai++];

						int maxRows = (int)args[ai++];
						int maxTime = (int)args[ai++];

						Qel.QueryEngine qe = GetQueryEngineInstance(instanceId);

						List<object[]> rows = qe.NextRows(minRows, maxRows, maxTime);

						DataRow dataRow = null;
						List<DataRow> dataRows = new List<DataRow>();
						if (rows == null) return dataRows;

						for (int ri = 0; ri < rows.Count; ri++) // convert each row
						{
							object[] row = rows[ri];
							if (row != null)
							{
								dataRow = new DataRow();
								object[] convertedRow = new object[row.Length];
								for (int i = 0; i < row.Length; i++)
								{
									convertedRow[i] = _transHelper.ConvertObject(row[i]);
								}
								dataRow.Data = convertedRow;
							}

							dataRows.Add(dataRow);
						}
						return dataRows;
					}

				case MobiusQueryEngineService.NextRow:
					{
						int instanceId = (int)args[0];
						Qel.QueryEngine qe = GetQueryEngineInstance(instanceId);

						object[] row = qe.NextRow();
						DataRow dataRow = null;
						if (row != null)
						{
							dataRow = new DataRow();
							object[] convertedRow = new object[row.Length];
							for (int i = 0; i < row.Length; i++)
							{
								convertedRow[i] = _transHelper.ConvertObject(row[i]);
							}
							dataRow.Data = convertedRow;
						}

						return dataRow;
					}

				case MobiusQueryEngineService.Close:
					{
						int instanceId = (int)args[0];
						Qel.QueryEngine qe = GetQueryEngineInstance(instanceId);
						qe.Close();
						return true;
					}

				case MobiusQueryEngineService.Cancel:
					{
						int instanceId = (int)args[0];
						Qel.QueryEngine qe = GetQueryEngineInstance(instanceId);
						qe.Cancel(false);
						return true;
					}

				case MobiusQueryEngineService.GetQueryEngineState:
					{
						int instanceId = (int)args[0];

						Qel.QueryEngine qe = GetQueryEngineInstance(instanceId);
						Mobius.Data.QueryEngineState state = qe.State;
						QueryEngineState result = _transHelper.Convert<Mobius.Data.QueryEngineState, QueryEngineState>(state);
						return result;
					}

				case MobiusQueryEngineService.LogExceptionAndSerializedQuery:
					{
						string exMsg = (string)args[0];
						string serializedQuery = (string)args[1];
						Data.Query query = Data.Query.Deserialize(serializedQuery);
						return Qel.QueryEngine.LogExceptionAndSerializedQuery(exMsg, query);
					}

				case MobiusQueryEngineService.LogQueryExecutionStatistics:
					{
						Qel.QueryEngine.LogQueryExecutionStatistics();
						return null;
					}

				case MobiusQueryEngineService.ValidateCalculatedFieldExpression:
					{
						string advExpr = (string)args[0];
						return Qel.QueryEngine.ValidateCalculatedFieldExpression(advExpr);
					}

				case MobiusQueryEngineService.CreateQueryFromMQL:
					{
						string mql = (string)args[0];
						Mobius.Data.Query mobiusQuery = Mobius.Data.MqlUtil.ConvertMqlToQuery(mql);

						Query result = _transHelper.Convert<Mobius.Data.Query, Query>(mobiusQuery); //translate the result
						return result;
					}

				case MobiusQueryEngineService.ExecuteMQLQuery:
					{
						Mobius.Data.Query q2;

						int instanceId = (int)args[0];
						string mql = (string)args[1];

						Qel.QueryEngine qe = GetQueryEngineInstance(instanceId);

						Mobius.Data.Query q = Mobius.Data.MqlUtil.ConvertMqlToQuery(mql);
						List<string> result = qe.TransformAndExecuteQuery(q, out q2);
						return result;
					}

				case MobiusQueryEngineService.GetSelectAllDataQuery:
					{
						string mtName = (string)args[0];
						string cn = (string)args[1];
						string queryXml = "";

						Mobius.Data.Query q = QueryEngineLibrary.QueryEngine.GetSelectAllDataQuery(mtName, cn);
						if (q != null) queryXml = q.Serialize(false);
						return queryXml;
					}

				case MobiusQueryEngineService.GetStandardMobileQueries:
					{
						Data.Query[] mobileQueries = QueryEngineLibrary.QueryEngine.GetStandardMobileQueries();

						List<Query> queries = new List<Query>();

						foreach (Data.Query query in mobileQueries)
						{
							Query typesQuery = _transHelper.Convert<Mobius.Data.Query, Query>(query);
							queries.Add(typesQuery);
						}

						return queries.ToArray();
					}

				case MobiusQueryEngineService.GetMobileQueriesByOwner:
					{
						string user = (string)args[0];
						Data.Query[] mobileQueries = QueryEngineLibrary.QueryEngine.GetMobileQueriesByOwner(user);

						List<Query> queries = new List<Query>();

						foreach (Data.Query query in mobileQueries)
						{
							Query typesQuery = _transHelper.Convert<Mobius.Data.Query, Query>(query);
							queries.Add(typesQuery);
						}

						return queries.ToArray();
					}

				case MobiusQueryEngineService.GetAdditionalData:
					{
						int instanceId = (int)args[0];
						string command = (string)args[1];

						Qel.QueryEngine qe = GetQueryEngineInstance(instanceId);

						object o = qe.GetAdditionalData(command);
						return o;
					}

				case MobiusQueryEngineService.ExportDataToSpotfireFiles:
					{
						int instanceId = (int)args[0];
						Qel.QueryEngine qe = GetQueryEngineInstance(instanceId);

						ExportParms ep = ExportParms.Deserialize(args[1] as string);

						QueryEngineStats qeStats = qe.ExportDataToSpotfireFiles(ep);

						return (qeStats != null ? qeStats.Serialize() : null); 
					}

				case MobiusQueryEngineService.CompleteRowRetrieval:
					{
						int instanceId = (int)args[0];
						Qel.QueryEngine qe = GetQueryEngineInstance(instanceId);

						QueryEngineStats qeStats = qe.CompleteRowRetrieval();

						return (qeStats != null ? qeStats.Serialize() : null);
					}

				default:
					throw new InvalidOperationException("Unrecognized operation: " + (int)op);

			}

		}

		/// <summary>
		/// Get the QueryEngine with the supplied ID
		/// </summary>
		/// <param name="qeId"></param>
		/// <returns></returns>

		Qel.QueryEngine GetQueryEngineInstance(int qeId)
		{
			lock (Qel.QueryEngine.IdToInstanceDict)
			{
				if (Qel.QueryEngine.IdToInstanceDict.ContainsKey(qeId))
				{
					Qel.QueryEngine qe = Qel.QueryEngine.IdToInstanceDict[qeId];
					return qe;
				}

				else throw new ArgumentException("Invalid query engine id: " + qeId);
			}
		}


	}
}
