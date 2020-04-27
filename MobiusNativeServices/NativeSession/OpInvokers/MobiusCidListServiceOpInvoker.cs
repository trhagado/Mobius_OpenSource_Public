using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mobius.Services.Util.TypeConversionUtil;
using Mobius.Services.Types;
using Mobius.Services.Native.ServiceOpCodes;


namespace Mobius.Services.Native.OpInvokers
{
    public class MobiusCidListServiceOpInvoker : IInvokeServiceOps
    {
        private static TypeConversionHelper _transHelper = new TypeConversionHelper(TypeConversionHelper.ContextRetentionMode.NoRetention);

        #region IInvokeServiceOps Members

				object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
				{
					Mobius.Data.UserObject uo;
					byte[] sUo;

					MobiusCidListService op = (MobiusCidListService)opCode;
					switch (op)
					{
						case MobiusCidListService.Read:
							{
								int listId = (int)args[0];
								string mtName = (string)args[1];
								Mobius.Data.MetaTable mt = (mtName == null) ? null : Mobius.Data.MetaTableCollection.Get(mtName);
								Data.CidList mobiusCidList = UAL.CidListDao.Read(listId, mt);
								if (mobiusCidList == null) return null;
								uo = mobiusCidList.UserObject;
								uo.Content = mobiusCidList.ToMultilineString();
								sUo = uo.SerializeBinary();
								return sUo;
							}

						case MobiusCidListService.ExecuteListLogic:
							{
								string list1Name = (string)args[0];
								string list2Name = (string)args[1];
								Data.ListLogicType listLogicType = (Data.ListLogicType)args[2];
								int result =
										UAL.CidListDao.ExecuteListLogic(list1Name, list2Name, listLogicType);
								return result;
							}

						case MobiusCidListService.CopyList:
							{
								string sourceUOInternalName = (string)args[0];
								UserObjectNode targetUONode = (UserObjectNode)args[1];
								Data.UserObject targetUO = _transHelper.Convert<UserObjectNode, Data.UserObject>(targetUONode);
								UAL.CidListDao.CopyList(sourceUOInternalName, targetUO);
								targetUONode = _transHelper.Convert<Data.UserObject, UserObjectNode>(targetUO);
								return targetUONode;
							}

						case MobiusCidListService.ReadLibrary:
							{
								int libId = (int)args[0];
								Data.CidList mobiusCidList = UAL.CidListDao.ReadLibrary(libId);
								if (mobiusCidList == null) return null;
								uo = mobiusCidList.UserObject;
								uo.Content = mobiusCidList.ToMultilineString();
								sUo = uo.SerializeBinary();
								return sUo;
							}
					}
					return null;
				}

        #endregion
    }
}
