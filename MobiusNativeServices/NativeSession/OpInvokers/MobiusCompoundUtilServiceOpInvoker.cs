using Mobius.ComOps;
using Mobius.Data;
using Mobius.Services.Util.TypeConversionUtil;
using Mobius.Services.Types;
using Mobius.Services.Native.ServiceOpCodes;
using Qel = Mobius.QueryEngineLibrary;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.Services.Native.OpInvokers
{
	public class MobiusCompoundUtilServiceOpInvoker : IInvokeServiceOps
	{
		private static TypeConversionHelper _transHelper = new TypeConversionHelper(TypeConversionHelper.ContextRetentionMode.NoRetention);

		object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
		{
			MobiusCompoundUtilService op = (MobiusCompoundUtilService)opCode;
			switch (op)
			{
				case MobiusCompoundUtilService.DoesCidExist:
					{
						string cid = (string)args[0];
						string mtName = (string)args[1];
						Mobius.Data.MetaTable mt = (mtName == null) ? null : Mobius.Data.MetaTableCollection.Get(mtName);
						bool cidExists = Qel.CompoundIdUtil.Exists(cid, mt);
						return cidExists;
					}

				case MobiusCompoundUtilService.SelectMoleculeFromCid:
					{
						string cid = (string)args[0];
						string mtName = (string)args[1];
						Mobius.Data.MetaTable mt = (mtName == null) ? null : Mobius.Data.MetaTableCollection.Get(mtName);
						Data.MoleculeMx nativeMolecule =
								Qel.MoleculeUtil.SelectMoleculeForCid(cid, mt);

						if (ClientState.MobiusClientVersionIsAtLeast(6, 0)) // Helm version or newer
						{
							byte[] ba = MobiusDataType.SerializeBinarySingle(nativeMolecule);
							return ba;
						}

						else
						{
							ChemicalStructure transferMolecule =
									_transHelper.Convert<Mobius.Data.MoleculeMx, ChemicalStructure>(nativeMolecule);
							return transferMolecule;
						}
					}

				case MobiusCompoundUtilService.SelectMoleculesForCidList:
					{
						List<string> cidList = (List<string>)args[0];
						string mtName = (string)args[1];
						Mobius.Data.MetaTable mt = (mtName == null) ? null : Mobius.Data.MetaTableCollection.Get(mtName);
						Dictionary<string, Data.MoleculeMx> csDict =
								Qel.MoleculeUtil.SelectMoleculesForCidList(cidList, mt);

						return csDict;
					}

				case MobiusCompoundUtilService.GetAllSaltForms:
					{
						string cid = (string)args[0];
						string normalizedCid = Data.CompoundId.Normalize(cid);
						List<string> cidList = new List<string>(new string[] { normalizedCid });
						Dictionary<string, List<string>> cidDict = Qel.MoleculeUtil.GetAllSaltForms(cidList);
						List<string> results = null;
						if (cidDict.ContainsKey(cid))
						{
							results = cidDict[cid] as List<string>;
						}
						return results;
					}

				case MobiusCompoundUtilService.GetAllSaltFormsForList:
					{
						List<string> cidList = (List<string>)args[0];
						Dictionary<string, List<string>> allSaltIds = Qel.MoleculeUtil.GetAllSaltForms(cidList);
						return allSaltIds;
					}

				case MobiusCompoundUtilService.InsertSalts:
					{
						List<string> cidList = (List<string>)args[0];
						List<string> result = Qel.MoleculeUtil.InsertSalts(cidList);
						return result;
					}

				case MobiusCompoundUtilService.GroupSalts:
					{
						List<string> cidList = (List<string>)args[0];
						List<string> result = Qel.MoleculeUtil.GroupSalts(cidList);
						return result;
					}

				case MobiusCompoundUtilService.ValidateList:
					{
						string listText = (string)args[0];
						string rootTableName = (string)args[1];
						string result = Qel.CompoundIdUtil.ValidateList(listText, rootTableName);
						return result;
					}

				case MobiusCompoundUtilService.GetRelatedCompoundIds:
					{
						StructureSearchType searchTypes = // default for old client that doesn't supply this parm
								StructureSearchType.FullStructure |
								StructureSearchType.MolSim |
								StructureSearchType.MatchedPairs |
								StructureSearchType.SmallWorld;

						int argIdx = 0;
						string cid = (string)args[argIdx++];
						string mtName = (string)args[argIdx++];
						string chime = (string)args[argIdx++];
						if (args.Length > 4) searchTypes = (StructureSearchType)args[argIdx++];
						int searchId = (int)args[argIdx++];

						string result = Qel.MoleculeUtil.GetRelatedMatchCounts(cid, mtName, chime, searchTypes, searchId);
						return result;
					}

				case MobiusCompoundUtilService.GetRelatedStructures:
					{
						string cid = (string)args[0];
						string mtName = (string)args[1];

						string result = Qel.MoleculeUtil.GetRelatedMatchRowsSerialized(cid, mtName);
						return result;
					}

				case MobiusCompoundUtilService.ExecuteSmallWorldPreviewQuery:
					{
						string serializedQuery = (string)args[0];
						object[] oa = Qel.MoleculeUtil.ExecuteSmallWorldPreviewQuerySerialized(serializedQuery);
						return oa;
					}

			}
			return null;
		}

	}
}
