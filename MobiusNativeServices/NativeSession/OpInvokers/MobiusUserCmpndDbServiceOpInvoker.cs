using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mobius.Services.Util.TypeConversionUtil;
using Mobius.Services.Types;
using Mobius.Services.Native.ServiceOpCodes;


namespace Mobius.Services.Native.OpInvokers
{
	public class MobiusUserCmpndDbServiceOpInvoker : IInvokeServiceOps
	{
		private static TypeConversionHelper _transHelper = new TypeConversionHelper(TypeConversionHelper.ContextRetentionMode.NoRetention);

		#region IInvokeServiceOps Members

		object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
		{
			MobiusUserCmpndDbService op = (MobiusUserCmpndDbService)opCode;
			switch (op)
			{
				case MobiusUserCmpndDbService.CanModifyDatabase:
					{
						long databaseId = (long)args[0];
						string userName = (string)args[1];
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						bool canModify = instance.CanModifyDatabase(databaseId, userName);
						return canModify;
					}
				case MobiusUserCmpndDbService.SelectDatabaseHeaderByDatabaseId:
					{
						long databaseId = (long)args[0];
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						Mobius.Data.UcdbDatabase mobiusUcdbDatabase = instance.SelectDatabaseHeader(databaseId);
						UcdbDatabase ucdbDatabase = _transHelper.Convert<Mobius.Data.UcdbDatabase, UcdbDatabase>(mobiusUcdbDatabase);
						return ucdbDatabase;
					}
				case MobiusUserCmpndDbService.SelectDatabaseHeader:
					{
						string userName = (string)args[0];
						string databaseNameSpace = (string)args[1];
						string databaseName = (string)args[2];
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						Mobius.Data.UcdbDatabase mobiusUcdbDatabase = instance.SelectDatabaseHeader(userName, databaseNameSpace, databaseName);
						UcdbDatabase ucdbDatabase = _transHelper.Convert<Mobius.Data.UcdbDatabase, UcdbDatabase>(mobiusUcdbDatabase);
						return ucdbDatabase;
					}
				case MobiusUserCmpndDbService.SelectDatabaseHeaders:
					{
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						Mobius.Data.UcdbDatabase[] mobiusUcdbDatabases = instance.SelectDatabaseHeaders();
						UcdbDatabase[] ucdbDatabases = _transHelper.Convert<Mobius.Data.UcdbDatabase[], UcdbDatabase[]>(mobiusUcdbDatabases);
						return ucdbDatabases;
					}
				case MobiusUserCmpndDbService.SelectDatabaseHeadersByOwner:
					{
						string ownerUserName = (string)args[0];
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						Mobius.Data.UcdbDatabase[] mobiusUcdbDatabases = instance.SelectDatabaseHeaders(ownerUserName);
						UcdbDatabase[] ucdbDatabases = _transHelper.Convert<Mobius.Data.UcdbDatabase[], UcdbDatabase[]>(mobiusUcdbDatabases);
						return ucdbDatabases;
					}
				case MobiusUserCmpndDbService.SelectDatabaseModels:
					{
						long databaseId = (long)args[0];
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						Mobius.Data.UcdbModel[] mobiusUcdbModels = instance.SelectDatabaseModels(databaseId);
						UcdbModel[] ucdbModels = _transHelper.Convert<Mobius.Data.UcdbModel[], UcdbModel[]>(mobiusUcdbModels);
						return ucdbModels;
					}
				case MobiusUserCmpndDbService.SelectDatabaseExtStringCids:
					{
						long databaseId = (long)args[0];
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						string[] extStringCids = instance.SelectDatabaseExtStringCids(databaseId);
						return extStringCids;
					}
				case MobiusUserCmpndDbService.SelectDatabaseCompoundByCompoundId:
					{
						long compoundId = (long)args[0];
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						Mobius.Data.UcdbCompound mobiusUcdbCompound = instance.SelectDatabaseCompound(compoundId);
						UcdbCompound ucdbCompound = _transHelper.Convert<Mobius.Data.UcdbCompound, UcdbCompound>(mobiusUcdbCompound);
						return ucdbCompound;
					}
				case MobiusUserCmpndDbService.SelectDatabaseCompound:
					{
						long databaseId = (long)args[0];
						string extCmpndIdTxt = (string)args[1];
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						Mobius.Data.UcdbCompound mobiusUcdbCompound = instance.SelectDatabaseCompound(databaseId, extCmpndIdTxt);
						UcdbCompound ucdbCompound = _transHelper.Convert<Mobius.Data.UcdbCompound, UcdbCompound>(mobiusUcdbCompound);
						return ucdbCompound;
					}
				case MobiusUserCmpndDbService.SelectDatabaseCompounds:
					{
						long databaseId = (long)args[0];
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						Mobius.Data.UcdbCompound[] mobiusUcdbCompounds = instance.SelectDatabaseCompounds(databaseId);
						UcdbCompound[] ucdbCompounds = _transHelper.Convert<Mobius.Data.UcdbCompound[], UcdbCompound[]>(mobiusUcdbCompounds);
						return ucdbCompounds;
					}
				case MobiusUserCmpndDbService.SelectDatabaseCompoundCount:
					{
						long databaseId = (long)args[0];
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						int compoundCount = instance.SelectDatabaseCompoundCount(databaseId);
						return compoundCount;
					}
				case MobiusUserCmpndDbService.GetMaxCompoundId:
					{
						long databaseId = (long)args[0];
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						long maxCompoundId = instance.GetMaxCompoundId(databaseId);
						return maxCompoundId;
					}
				case MobiusUserCmpndDbService.InsertDatabaseHeader:
					{
						UcdbDatabase ucdb = (UcdbDatabase)args[0];
						Mobius.Data.UcdbDatabase mobiusUcdbDatabase =
								_transHelper.Convert<UcdbDatabase, Mobius.Data.UcdbDatabase>(ucdb);
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						instance.InsertDatabaseHeader(mobiusUcdbDatabase);

						UcdbDatabase ucdbDatabase = ConvertUcdbDatabaseForReturn(mobiusUcdbDatabase);
						return ucdbDatabase;
					}
				case MobiusUserCmpndDbService.UpdateDatabaseHeader:
					{
						UcdbDatabase ucdb = (UcdbDatabase)args[0];
						Mobius.Data.UcdbDatabase mobiusUcdbDatabase =
								_transHelper.Convert<UcdbDatabase, Mobius.Data.UcdbDatabase>(ucdb);
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						instance.UpdateDatabaseHeader(mobiusUcdbDatabase);

						UcdbDatabase ucdbDatabase = ConvertUcdbDatabaseForReturn(mobiusUcdbDatabase);
						return ucdbDatabase;
					}
				case MobiusUserCmpndDbService.UpdateDatabaseModelAssoc:
					{
						UcdbDatabase ucdb = (UcdbDatabase)args[0];
						UcdbModel[] dbModels = (UcdbModel[])args[1];
						Mobius.Data.UcdbDatabase mobiusUcdbDatabase =
								_transHelper.Convert<UcdbDatabase, Mobius.Data.UcdbDatabase>(ucdb);
						Mobius.Data.UcdbModel[] mobiusUcdbModels =
								_transHelper.Convert<UcdbModel[], Mobius.Data.UcdbModel[]>(dbModels);
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						instance.UpdateDatabaseModelAssoc(mobiusUcdbDatabase, mobiusUcdbModels);

						UcdbDatabase ucdbDatabase = ConvertUcdbDatabaseForReturn(mobiusUcdbDatabase);
						return ucdbDatabase;
					}
				case MobiusUserCmpndDbService.UpdateDatabaseCompounds:
					{
						UcdbDatabase ucdb = (UcdbDatabase)args[0];
						UcdbCompound[] cpds = (UcdbCompound[])args[1];
						Mobius.Data.UcdbDatabase mobiusUcdbDatabase =
								 _transHelper.Convert<UcdbDatabase, Mobius.Data.UcdbDatabase>(ucdb);
						Mobius.Data.UcdbCompound[] mobiusUcdbCompounds =
								_transHelper.Convert<UcdbCompound[], Mobius.Data.UcdbCompound[]>(cpds);
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						instance.UpdateDatabaseCompounds(mobiusUcdbDatabase, mobiusUcdbCompounds);

						UcdbDatabase ucdbDatabase = ConvertUcdbDatabaseForReturn(mobiusUcdbDatabase);
						return ucdbDatabase;
					}

				case MobiusUserCmpndDbService.DeleteDatabaseCompounds:
					{
						long databaseId = (long)args[0];
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						long deleteCount = instance.DeleteDatabaseCompounds(databaseId);
						return deleteCount;
					}
				case MobiusUserCmpndDbService.DeleteDatabase:
					{
						throw new NotImplementedException();
					}
				case MobiusUserCmpndDbService.UpdateDatabase:
					{
						UcdbDatabase ucdb = (UcdbDatabase)args[0];
						Mobius.Data.UcdbDatabase mobiusUcdbDatabase =
								_transHelper.Convert<UcdbDatabase, Mobius.Data.UcdbDatabase>(ucdb);
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						instance.UpdateDatabase(mobiusUcdbDatabase);

						UcdbDatabase ucdbDatabase = ConvertUcdbDatabaseForReturn(mobiusUcdbDatabase);
						return ucdbDatabase;
					}
				case MobiusUserCmpndDbService.LogMessage:
					{
						string message = (string)args[0];
						UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						instance.LogMessage(message);
						return true;
					}

				///////////////////////////////////////////////////////////////////////
				// Currently unsupported methods relating to model calculation results
				///////////////////////////////////////////////////////////////////////

				case MobiusUserCmpndDbService.UpdateDatabaseModelResultsByDatabaseId:
					{
						throw new NotImplementedException();
						//long databaseId = (long)args[0];
						//UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						//int updateCount = instance.UpdateDatabaseModelResults(databaseId);
						//return updateCount;
					}
				case MobiusUserCmpndDbService.UpdateDatabaseModelResultsByDatabaseIdAndModelId:
					{
						throw new NotImplementedException();
						//long databaseId = (long)args[0];
						//long modelId = (long)args[1];
						//UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						//int updateCount = instance.UpdateDatabaseModelResults(databaseId, modelId);
						//return updateCount;
					}
				case MobiusUserCmpndDbService.UpdateCompoundModelResultsByCompoundId:
					{
						throw new NotImplementedException();
						//long compoundId = (long)args[0];
						//UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						//int updateCount = instance.UpdateDatabaseModelResults(compoundId);
						//return updateCount;
					}
				case MobiusUserCmpndDbService.UpdateCompoundModelResultsByCompoundIdAndModelId:
					{
						throw new NotImplementedException();
						//long compoundId = (long)args[0];
						//long modelId = (long)args[1];
						//UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						//int updateCount = instance.UpdateDatabaseModelResults(compoundId, modelId);
						//return updateCount;
					}
				case MobiusUserCmpndDbService.UpdatePendingModelResults:
					{
						throw new NotImplementedException();
						//long databaseId = (long)args[0];
						//UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						//int updateCount = instance.UpdatePendingModelResults(databaseId);
						//return updateCount;
					}

				case MobiusUserCmpndDbService.SelectUsersWithFailedUpdates:
					{
						throw new NotImplementedException();
						//UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						//string[] usersWithFailedUpdates = instance.SelectUsersWithFailedUpdates();
						//return usersWithFailedUpdates;
					}
				case MobiusUserCmpndDbService.SelectAllFailedUpdates:
					{
						throw new NotImplementedException();
						//UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						//Mobius.Data.UcdbDatabase[] mobiusUcdbDatabases = instance.SelectFailedUpdates();
						//UcdbDatabase[] ucdbDatabases =
						//		_transHelper.Convert<Mobius.Data.UcdbDatabase[], UcdbDatabase[]>(mobiusUcdbDatabases);
						//return ucdbDatabases;
					}
				case MobiusUserCmpndDbService.SelectFailedUpdates:
					{
						throw new NotImplementedException();
						//string ownerUserName = (string)args[0];
						//UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						//Mobius.Data.UcdbDatabase[] mobiusUcdbDatabases = instance.SelectFailedUpdates(ownerUserName);
						//UcdbDatabase[] ucdbDatabases =
						//		_transHelper.Convert<Mobius.Data.UcdbDatabase[], UcdbDatabase[]>(mobiusUcdbDatabases);
						//return ucdbDatabases;
					}
				case MobiusUserCmpndDbService.UpdateIsRunningByDatabaseId:
					{
						throw new NotImplementedException();
						//long databaseId = (long)args[0];
						//UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						//bool status = instance.UpdateIsRunning(databaseId);
						//return status;
					}
				case MobiusUserCmpndDbService.UpdateIsRunning:
					{
						throw new NotImplementedException();
						//UcdbDatabase ucdb = (UcdbDatabase)args[0];
						//Mobius.Data.UcdbDatabase mobiusUcdbDatabase =
						//		_transHelper.Convert<UcdbDatabase, Mobius.Data.UcdbDatabase>(ucdb);
						//UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						//bool status = instance.UpdateIsRunning(mobiusUcdbDatabase);
						//return status;
					}
				case MobiusUserCmpndDbService.UpdateIsPendingByDatabaseId:
					{
						throw new NotImplementedException();
						//long databaseId = (long)args[0];
						//UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						//bool status = instance.UpdateIsPending(databaseId);
						//return status;
					}
				case MobiusUserCmpndDbService.UpdateIsPending:
					{
						throw new NotImplementedException();
						//UcdbDatabase ucdb = (UcdbDatabase)args[0];
						//Mobius.Data.UcdbDatabase mobiusUcdbDatabase =
						//		_transHelper.Convert<UcdbDatabase, Mobius.Data.UcdbDatabase>(ucdb);
						//UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						//bool status = instance.UpdateIsPending(mobiusUcdbDatabase);
						//return status;
					}

				case MobiusUserCmpndDbService.TestModelService:
					{
						throw new NotImplementedException();
						//UcdbCompound[] cmpnds = (UcdbCompound[])args[0];
						//UcdbModel[] models = (UcdbModel[])args[1];
						//bool testModelsSingly = (bool)args[2];
						//Mobius.Data.UcdbCompound[] mobiusUcdbCompounds =
						//		_transHelper.Convert<UcdbCompound[], Mobius.Data.UcdbCompound[]>(cmpnds);
						//Mobius.Data.UcdbModel[] mobiusUcdbModels =
						//		_transHelper.Convert<UcdbModel[], Mobius.Data.UcdbModel[]>(models);
						//UcdbTestModelServiceResult result = new UcdbTestModelServiceResult();
						//UAL.UserCmpndDbDao instance = new UAL.UserCmpndDbDao();
						//result.ResultInt = instance.TestModelService(
						//		mobiusUcdbCompounds, mobiusUcdbModels, testModelsSingly,
						//		out result.ResultsText, out result.ErrorCount, out result.Exception);
						//return result;
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

