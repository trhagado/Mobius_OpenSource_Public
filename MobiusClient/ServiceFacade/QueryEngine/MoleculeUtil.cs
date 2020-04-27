using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;
using Mobius.QueryEngineLibrary;
using QEL = Mobius.QueryEngineLibrary;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ServiceTypes = Mobius.Services.Types;


namespace Mobius.ServiceFacade
{
	public class MoleculeUtil : IMoleculeMxUtil
	{

		/// <summary>
		/// Select a MoleculeMx object for a compound id
		/// </summary>
		/// <param name="cid"></param>
		/// <param name="mt"></param>
		/// <returns></returns>

		public MoleculeMx SelectMoleculeFromCid( // required IChemicalStructureUtil interface method definition
			string cid,
			MetaTable mt = null)
		{ // required interface method definition
			return SelectMoleculeForCid(cid, mt);
		}

		/// <summary>
		/// Select a MoleculeMx object for a compound id
		/// </summary>
		/// <param name="cid"></param>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static MoleculeMx SelectMoleculeForCid(
			string cid,
			MetaTable mt = null)
		{
			MoleculeMx mol = null;
			Stopwatch sw = Stopwatch.StartNew();

			mol = MoleculeCache.Get(cid); // see if molecule in cache
			if (mol != null) return mol;

			if (ServiceFacade.UseRemoteServices)
			{
				string mtName = mt?.Name;

				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusCompoundUtilService,
								(int)Services.Native.ServiceOpCodes.MobiusCompoundUtilService.SelectMoleculeFromCid,
								new Services.Native.NativeMethodTransportObject(new object[] { cid, mtName })); // , setStereoChemistryComments
				((System.ServiceModel.IClientChannel)nativeClient).Close();

				if (resultObject == null) return null;
				byte[] ba = resultObject.Value as byte[];
				if (ba != null && ba.Length > 0)
				{
					MobiusDataType mdt = MobiusDataType.DeserializeBinarySingle(ba);
					mol = mdt as MoleculeMx;
				}
			}

			else mol = QEL.MoleculeUtil.SelectMoleculeForCid(cid, mt);

			if (MoleculeMx.IsDefined(mol))
			{
				bool isUcdb = (mt != null && mt.Root.IsUserDatabaseStructureTable); // user compound database
				if (!isUcdb) MoleculeCache.AddMolecule(cid, mol); // add to cache
			}

			long ms = sw.ElapsedMilliseconds;
			return mol;
		}

		/// <summary>
		/// Select a list of molecule strings MoleculeMx objects for a compound id
		/// </summary>
		/// <param name="cid"></param>
		/// <param name="mt">MetaTable used to get root table to select structures from</param>
		/// <returns></returns>

		public static Dictionary<string, MoleculeMx> SelectChemicalStructuresForCidList(
			List<string> cidList,
			MetaTable mt = null)
		{
			Dictionary<string, MoleculeMx> csDict = null;
			Stopwatch sw = Stopwatch.StartNew();

			if (ServiceFacade.UseRemoteServices)
			{
				string mtName = mt?.Name;
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusCompoundUtilService,
								(int)Services.Native.ServiceOpCodes.MobiusCompoundUtilService.SelectMoleculesForCidList,
								new Services.Native.NativeMethodTransportObject(new object[] { cidList, mtName }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();

				csDict = (resultObject != null) ? (Dictionary<string, MoleculeMx>)resultObject.Value : null;
			}

			else // local
			{
				csDict = QEL.MoleculeUtil.SelectMoleculesForCidList(cidList, mt);
			}

			long ms = sw.ElapsedMilliseconds;
			return csDict;
		}

		/// <summary>
		/// Lookup all of the salt forms for a compound
		/// </summary>
		/// <param name="cid></param>
		/// <returns></returns>

		public static List<string> GetAllSaltForms(
			string cid)
		{
			if (cid == null || cid == "") return null;

			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusCompoundUtilService,
								(int)Services.Native.ServiceOpCodes.MobiusCompoundUtilService.GetAllSaltForms,
								new Services.Native.NativeMethodTransportObject(new object[] { cid }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				List<string> result = (resultObject != null) ? (List<string>)resultObject.Value : null;
				return result;
			}
			else
			{
				cid = CompoundId.Normalize(cid);
				List<string> list = new List<string>();
				list.Add(cid);
				Dictionary<string, List<string>> cidDict = GetAllSaltForms(list);
				if (cidDict.ContainsKey(cid)) return (List<string>)cidDict[cid];
				else return null;
			}
		}

/// <summary>
/// Get list of related structures using the supplied compound id for the query structure
/// </summary>
/// <param name="cid"></param>
/// <param name="type"></param>
/// <returns></returns>

		public static string GetRelatedMatchCounts(
			string cid,
			string mtName,
			string chime,
			StructureSearchType searchTypes,
			int searchId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusCompoundUtilService,
								(int)Services.Native.ServiceOpCodes.MobiusCompoundUtilService.GetRelatedCompoundIds,
								//new Services.Native.NativeMethodTransportObject(new object[] { cid, mtName, chime, (int)searchTypes, searchId }));
							new Services.Native.NativeMethodTransportObject(new object[] { cid, mtName, chime, searchId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				string result = (resultObject != null) ? (string)resultObject.Value : null;
				return result;
			}

			else return QEL.MoleculeUtil.GetRelatedMatchCounts(cid, mtName, chime, searchTypes, searchId);
		}

		/// <summary>
		/// Get related structures of the specified types using the supplied compound id for the query structure
		/// </summary>
		/// <param name="queryCid"></param>
		/// <param name="mtName"></param>
		/// <returns></returns>

		public static List<StructSearchMatch> GetRelatedMatchRows(
			string queryCid,
			string mtName)
		{
			List<StructSearchMatch> ssmList;
			string serializedResult = "";

			if (queryCid == null || queryCid == "") return null;

			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusCompoundUtilService,
								(int)Services.Native.ServiceOpCodes.MobiusCompoundUtilService.GetRelatedStructures,
								new Services.Native.NativeMethodTransportObject(new object[] { queryCid, mtName }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				serializedResult = (resultObject != null) ? (string)resultObject.Value : null;
			}

			else serializedResult = QEL.MoleculeUtil.GetRelatedMatchRowsSerialized(queryCid, mtName);

			if (Lex.IsUndefined(serializedResult)) return new List<StructSearchMatch>();

			try
			{
				ssmList = new List<StructSearchMatch>();

				string[] sa = serializedResult.Split('\n');
				foreach (string s in sa)
				{
					if (Lex.IsUndefined(s)) continue;

					StructSearchMatch ssm = StructSearchMatch.Deserialize(s);
					ssmList.Add(ssm);
				}

				return ssmList;
			}

			catch (Exception ex)
			{
				string msg = DebugLog.FormatExceptionMessage(ex);
				ServicesLogFile.Message(msg, CommonConfigInfo.ServicesLogFileName); // try to log on server

				return new List<StructSearchMatch>(); // return empty result
			}

		}

		/// <summary>
		/// Execute a SmallWorld preview query
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>

		public static void ExecuteSmallWorldPreviewQuery(
			Query query, 
			out QueryEngine qe,
			out List<object[]> rows)
		{
			byte[] ba;
			qe = null;
			rows = null;

			string serializedQuery = query.Serialize();

			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusCompoundUtilService,
								(int)Services.Native.ServiceOpCodes.MobiusCompoundUtilService.ExecuteSmallWorldPreviewQuery,
								new Services.Native.NativeMethodTransportObject(new object[] { serializedQuery }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();

				if (resultObject.Value == null) return;

				object[] oa = resultObject.Value as object[];
				int qeId = (int)oa[0];
				qe = new QueryEngine(qeId, null); // instantiate facade QE

				ba = (byte[])oa[1];
				rows = VoArray.DeserializeByteArrayToVoArrayList(ba);
				return;
			}

			else
			{
				QEL.QueryEngine qelQe;
				Query q2 = Query.Deserialize(serializedQuery); // need to make separate copy of query
				QEL.MoleculeUtil.ExecuteSmallWorldPreviewQuery(q2, out qelQe, out rows);
				if (rows == null) return;

				qe = new QueryEngine(-1, qelQe);

				ba = VoArray.SerializeBinaryVoArrayListToByteArray(rows);  // serialize & deserialize to simulate service
				rows = VoArray.DeserializeByteArrayToVoArrayList(ba);
				return;
			}

		}

		/// <summary>
		/// Get list of compounds whose fragments match those of the compounds in the list.
		/// </summary>
		/// <param name="cidList"></param>
		/// <returns></returns>

		public static Dictionary<string, List<string>> GetAllSaltForms(
			List<string> cidList)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusCompoundUtilService,
								(int)Services.Native.ServiceOpCodes.MobiusCompoundUtilService.GetAllSaltFormsForList,
								new Services.Native.NativeMethodTransportObject(new object[] { cidList }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				Dictionary<string, List<string>> allSaltIds = (resultObject != null) ? (Dictionary<string, List<string>>)resultObject.Value : null;
				return allSaltIds;
			}

			else return QEL.MoleculeUtil.GetAllSaltForms(cidList);
		}

		/// <summary>
		/// Insert salts into list, grouping together
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>

		public static List<string> InsertSalts(
			List<string> list)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusCompoundUtilService,
								(int)Services.Native.ServiceOpCodes.MobiusCompoundUtilService.InsertSalts,
								new Services.Native.NativeMethodTransportObject(new object[] { list }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				List<string> result = (resultObject != null) ? (List<string>)resultObject.Value : null;
				return result;
			}

			else return QEL.MoleculeUtil.InsertSalts(list);
		}

		/// <summary>
		/// Group together numbers with same parent structure
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>

		public static List<string> GroupSalts(
			List<string> list)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusCompoundUtilService,
								(int)Services.Native.ServiceOpCodes.MobiusCompoundUtilService.GroupSalts,
								new Services.Native.NativeMethodTransportObject(new object[] { list }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				List<string> result = (resultObject != null) ? (List<string>)resultObject.Value : null;
				return result;
			}

			else return QEL.MoleculeUtil.GroupSalts(list);
		}

/// <summary>
/// Convert an Inchi string to a molfile
/// </summary>
/// <param name="inchiString"></param>
/// <returns></returns>

		public string InChIStringToMolfileString(string inchiString)
		{
			return QEL.MoleculeUtil.InChIStringToMolfileStringStatic(inchiString);
		}

	}
}
