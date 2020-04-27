using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mobius.Services.Util.TypeConversionUtil;
using Mobius.Services.Types;
using Mobius.Services.Native.ServiceOpCodes;


namespace Mobius.Services.Native.OpInvokers
{
    public class MobiusUcdbServiceOpInvoker : IInvokeServiceOps
    {
        private static TypeConversionHelper _transHelper = new TypeConversionHelper(TypeConversionHelper.ContextRetentionMode.NoRetention);

        #region IInvokeServiceOps Members

        object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
        {
            MobiusUcdbService op = (MobiusUcdbService)opCode;
            switch (op)
            {
                case MobiusUcdbService.CanModifyDatabase:
                    {
                        long databaseId = (long)args[0];
                        string userName = (string)args[1];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        bool canModify = instance.CanModifyDatabase(databaseId, userName);
                        return canModify;
                    }
                case MobiusUcdbService.SelectDatabaseHeaderByDatabaseId:
                    {
                        long databaseId = (long)args[0];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        Mobius.Data.UcdbDatabase mobiusUcdbDatabase = instance.SelectDatabaseHeader(databaseId);
                        UcdbDatabase ucdbDatabase = _transHelper.Convert<Mobius.Data.UcdbDatabase, UcdbDatabase>(mobiusUcdbDatabase);
                        return ucdbDatabase;
                    }
                case MobiusUcdbService.SelectDatabaseHeader:
                    {
                        string userName = (string)args[0];
                        string databaseNameSpace = (string)args[1];
                        string databaseName = (string)args[2];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        Mobius.Data.UcdbDatabase mobiusUcdbDatabase = instance.SelectDatabaseHeader(userName, databaseNameSpace, databaseName);
                        UcdbDatabase ucdbDatabase = _transHelper.Convert<Mobius.Data.UcdbDatabase, UcdbDatabase>(mobiusUcdbDatabase);
                        return ucdbDatabase;
                    }
                case MobiusUcdbService.SelectDatabaseHeaders:
                    {
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        Mobius.Data.UcdbDatabase[] mobiusUcdbDatabases = instance.SelectDatabaseHeaders();
                        UcdbDatabase[] ucdbDatabases = _transHelper.Convert<Mobius.Data.UcdbDatabase[], UcdbDatabase[]>(mobiusUcdbDatabases);
                        return ucdbDatabases;
                    }
                case MobiusUcdbService.SelectDatabaseHeadersByOwner:
                    {
                        string ownerUserName = (string)args[0];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        Mobius.Data.UcdbDatabase[] mobiusUcdbDatabases = instance.SelectDatabaseHeaders(ownerUserName);
                        UcdbDatabase[] ucdbDatabases = _transHelper.Convert<Mobius.Data.UcdbDatabase[], UcdbDatabase[]>(mobiusUcdbDatabases);
                        return ucdbDatabases;
                    }
                case MobiusUcdbService.SelectDatabaseModels:
                    {
                        long databaseId = (long)args[0];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        Mobius.Data.UcdbModel[] mobiusUcdbModels = instance.SelectDatabaseModels(databaseId);
                        UcdbModel[] ucdbModels = _transHelper.Convert<Mobius.Data.UcdbModel[], UcdbModel[]>(mobiusUcdbModels);
                        return ucdbModels;
                    }
                case MobiusUcdbService.SelectDatabaseExtStringCids:
                    {
                        long databaseId = (long)args[0];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        string[] extStringCids = instance.SelectDatabaseExtStringCids(databaseId);
												return extStringCids;
                    }
                case MobiusUcdbService.SelectDatabaseCompoundByCompoundId:
                    {
                        long compoundId = (long)args[0];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        Mobius.Data.UcdbCompound mobiusUcdbCompound = instance.SelectDatabaseCompound(compoundId);
                        UcdbCompound ucdbCompound = _transHelper.Convert<Mobius.Data.UcdbCompound, UcdbCompound>(mobiusUcdbCompound);
                        return ucdbCompound;
                    }
                case MobiusUcdbService.SelectDatabaseCompound:
                    {
                        long databaseId = (long)args[0];
                        string extCmpndIdTxt = (string)args[1];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        Mobius.Data.UcdbCompound mobiusUcdbCompound = instance.SelectDatabaseCompound(databaseId, extCmpndIdTxt);
                        UcdbCompound ucdbCompound = _transHelper.Convert<Mobius.Data.UcdbCompound, UcdbCompound>(mobiusUcdbCompound);
                        return ucdbCompound;
                    }
                case MobiusUcdbService.SelectDatabaseCompounds:
                    {
                        long databaseId = (long)args[0];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        Mobius.Data.UcdbCompound[] mobiusUcdbCompounds = instance.SelectDatabaseCompounds(databaseId);
                        UcdbCompound[] ucdbCompounds = _transHelper.Convert<Mobius.Data.UcdbCompound[], UcdbCompound[]>(mobiusUcdbCompounds);
                        return ucdbCompounds;
                    }
                case MobiusUcdbService.SelectDatabaseCompoundCount:
                    {
                        long databaseId = (long)args[0];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        int compoundCount = instance.SelectDatabaseCompoundCount(databaseId);
                        return compoundCount;
                    }
                case MobiusUcdbService.GetMaxCompoundId:
                    {
                        long databaseId = (long)args[0];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        long maxCompoundId = instance.GetMaxCompoundId(databaseId);
                        return maxCompoundId;
                    }
                case MobiusUcdbService.InsertDatabaseHeader:
                    {
                        UcdbDatabase ucdb = (UcdbDatabase)args[0];
                        Mobius.Data.UcdbDatabase mobiusUcdbDatabase =
                            _transHelper.Convert<UcdbDatabase, Mobius.Data.UcdbDatabase>(ucdb);
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        instance.InsertDatabaseHeader(mobiusUcdbDatabase);

												UcdbDatabase ucdbDatabase = ConvertUcdbDatabaseForReturn(mobiusUcdbDatabase);
												return ucdbDatabase;
                    }
                case MobiusUcdbService.UpdateDatabaseHeader:
                    {
                        UcdbDatabase ucdb = (UcdbDatabase)args[0];
                        Mobius.Data.UcdbDatabase mobiusUcdbDatabase =
                            _transHelper.Convert<UcdbDatabase, Mobius.Data.UcdbDatabase>(ucdb);
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        instance.UpdateDatabaseHeader(mobiusUcdbDatabase);

												UcdbDatabase ucdbDatabase = ConvertUcdbDatabaseForReturn(mobiusUcdbDatabase);
												return ucdbDatabase;
										}
                case MobiusUcdbService.UpdateDatabaseModelAssoc:
                    {
                        UcdbDatabase ucdb = (UcdbDatabase)args[0];
                        UcdbModel[] dbModels = (UcdbModel[])args[1];
                        Mobius.Data.UcdbDatabase mobiusUcdbDatabase =
                            _transHelper.Convert<UcdbDatabase, Mobius.Data.UcdbDatabase>(ucdb);
                        Mobius.Data.UcdbModel[] mobiusUcdbModels =
                            _transHelper.Convert<UcdbModel[], Mobius.Data.UcdbModel[]>(dbModels);
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        instance.UpdateDatabaseModelAssoc(mobiusUcdbDatabase, mobiusUcdbModels);

												UcdbDatabase ucdbDatabase = ConvertUcdbDatabaseForReturn(mobiusUcdbDatabase);
												return ucdbDatabase;
                    }
                case MobiusUcdbService.UpdateDatabaseModelResultsByDatabaseId:
                    {
                        long databaseId = (long)args[0];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        int updateCount = instance.UpdateDatabaseModelResults(databaseId);
                        return updateCount;
                    }
                case MobiusUcdbService.UpdateDatabaseModelResultsByDatabaseIdAndModelId:
                    {
                        long databaseId = (long)args[0];
                        long modelId = (long)args[1];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        int updateCount = instance.UpdateDatabaseModelResults(databaseId, modelId);
                        return updateCount;
                    }
                case MobiusUcdbService.UpdateCompoundModelResultsByCompoundId:
                    {
                        long compoundId = (long)args[0];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        int updateCount = instance.UpdateDatabaseModelResults(compoundId);
                        return updateCount;
                    }
                case MobiusUcdbService.UpdateCompoundModelResultsByCompoundIdAndModelId:
                    {
                        long compoundId = (long)args[0];
                        long modelId = (long)args[1];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        int updateCount = instance.UpdateDatabaseModelResults(compoundId, modelId);
                        return updateCount;
                    }
                case MobiusUcdbService.UpdateDatabaseCompounds:
                    {
                        UcdbDatabase ucdb = (UcdbDatabase)args[0];
                        UcdbCompound[] cpds = (UcdbCompound[])args[1];
                        Mobius.Data.UcdbDatabase mobiusUcdbDatabase =
                             _transHelper.Convert<UcdbDatabase, Mobius.Data.UcdbDatabase>(ucdb);
                        Mobius.Data.UcdbCompound[] mobiusUcdbCompounds =
                            _transHelper.Convert<UcdbCompound[], Mobius.Data.UcdbCompound[]>(cpds);
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        instance.UpdateDatabaseCompounds(mobiusUcdbDatabase, mobiusUcdbCompounds);

												UcdbDatabase ucdbDatabase = ConvertUcdbDatabaseForReturn(mobiusUcdbDatabase);
												return ucdbDatabase;
										}
                case MobiusUcdbService.UpdatePendingModelResults:
                    {
                        long databaseId = (long)args[0];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        int updateCount = instance.UpdatePendingModelResults(databaseId);
                        return updateCount;
                    }
                case MobiusUcdbService.DeleteDatabaseCompounds:
                    {
                        long databaseId = (long)args[0];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        long deleteCount = instance.DeleteDatabaseCompounds(databaseId);
                        return deleteCount;
                    }
                case MobiusUcdbService.DeleteDatabase:
                    {
                        long databaseId = (long)args[0];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        long deleteCount = instance.DeleteDatabase(databaseId);
                        return deleteCount;
                    }
                case MobiusUcdbService.SelectUsersWithFailedUpdates:
                    {
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        string[] usersWithFailedUpdates = instance.SelectUsersWithFailedUpdates();
                        return usersWithFailedUpdates;
                    }
                case MobiusUcdbService.SelectAllFailedUpdates:
                    {
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        Mobius.Data.UcdbDatabase[] mobiusUcdbDatabases = instance.SelectFailedUpdates();
                        UcdbDatabase[] ucdbDatabases =
                            _transHelper.Convert<Mobius.Data.UcdbDatabase[], UcdbDatabase[]>(mobiusUcdbDatabases);
                        return ucdbDatabases;
                    }
                case MobiusUcdbService.SelectFailedUpdates:
                    {
                        string ownerUserName = (string)args[0];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        Mobius.Data.UcdbDatabase[] mobiusUcdbDatabases = instance.SelectFailedUpdates(ownerUserName);
                        UcdbDatabase[] ucdbDatabases =
                            _transHelper.Convert<Mobius.Data.UcdbDatabase[], UcdbDatabase[]>(mobiusUcdbDatabases);
                        return ucdbDatabases;
                    }
                case MobiusUcdbService.UpdateIsRunningByDatabaseId:
                    {
                        long databaseId = (long)args[0];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        bool status = instance.UpdateIsRunning(databaseId);
                        return status;
                    }
                case MobiusUcdbService.UpdateIsRunning:
                    {
                        UcdbDatabase ucdb = (UcdbDatabase)args[0];
                        Mobius.Data.UcdbDatabase mobiusUcdbDatabase =
                            _transHelper.Convert<UcdbDatabase, Mobius.Data.UcdbDatabase>(ucdb);
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        bool status = instance.UpdateIsRunning(mobiusUcdbDatabase);
                        return status;
                    }
                case MobiusUcdbService.UpdateIsPendingByDatabaseId:
                    {
                        long databaseId = (long)args[0];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        bool status = instance.UpdateIsPending(databaseId);
                        return status;
                    }
                case MobiusUcdbService.UpdateIsPending:
                    {
                        UcdbDatabase ucdb = (UcdbDatabase)args[0];
                        Mobius.Data.UcdbDatabase mobiusUcdbDatabase =
                            _transHelper.Convert<UcdbDatabase, Mobius.Data.UcdbDatabase>(ucdb);
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        bool status = instance.UpdateIsPending(mobiusUcdbDatabase);
                        return status;
                    }
                case MobiusUcdbService.UpdateDatabase:
                    {
                        UcdbDatabase ucdb = (UcdbDatabase)args[0];
                        Mobius.Data.UcdbDatabase mobiusUcdbDatabase =
                            _transHelper.Convert<UcdbDatabase, Mobius.Data.UcdbDatabase>(ucdb);
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        instance.UpdateDatabase(mobiusUcdbDatabase);

												UcdbDatabase ucdbDatabase = ConvertUcdbDatabaseForReturn(mobiusUcdbDatabase);
												return ucdbDatabase;
										}
                case MobiusUcdbService.TestModelService:
                    {
                        UcdbCompound[] cmpnds=(UcdbCompound[])args[0];
                        UcdbModel[] models=(UcdbModel[])args[1];
                        bool testModelsSingly=(bool)args[2];
                        Mobius.Data.UcdbCompound[] mobiusUcdbCompounds =
                            _transHelper.Convert<UcdbCompound[], Mobius.Data.UcdbCompound[]>(cmpnds);
                        Mobius.Data.UcdbModel[] mobiusUcdbModels =
                            _transHelper.Convert<UcdbModel[], Mobius.Data.UcdbModel[]>(models);
                        UcdbTestModelServiceResult result = new UcdbTestModelServiceResult();
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        result.ResultInt = instance.TestModelService(
                            mobiusUcdbCompounds, mobiusUcdbModels, testModelsSingly,
                            out result.ResultsText, out result.ErrorCount, out result.Exception);
                        return result;
                    }
                case MobiusUcdbService.LogMessage:
                    {
                        string message = (string)args[0];
                        Mobius.UcdbServiceProj.UcdbService instance = new Mobius.UcdbServiceProj.UcdbService();
                        instance.LogMessage(message);
                        return true;
                    }
            }
            return null;
        }

/// <summary>
/// Convert Ucdb database for return removing any lists of models & compounds
/// </summary>
/// <param name="mobiusUcdbDatabase"></param>
/// <returns></returns>

			UcdbDatabase ConvertUcdbDatabaseForReturn(Mobius.Data.UcdbDatabase mobiusUcdbDatabase)
				{
					UcdbDatabase ucdbDatabase = _transHelper.Convert<Mobius.Data.UcdbDatabase, UcdbDatabase>(mobiusUcdbDatabase);
					ucdbDatabase.Compounds = null;
					ucdbDatabase.Models = null;
					return ucdbDatabase;
				}

        #endregion
    }
}
