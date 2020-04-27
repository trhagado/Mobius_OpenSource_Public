using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mobius.Services.Util.TypeConversionUtil;
using Mobius.Services.Types;
using Mobius.Services.Native.ServiceOpCodes;


namespace Mobius.Services.Native.OpInvokers
{
    public class MobiusAnnotationServiceOpInvoker : IInvokeServiceOps
    {
        private static object _lockObject = new object();
        private static int _nextInstanceId = 0;
        private static Dictionary<int, Mobius.UAL.AnnotationDao> _instanceIdToInstance = new Dictionary<int, Mobius.UAL.AnnotationDao>();
        private TypeConversionHelper _transHelper = new TypeConversionHelper(TypeConversionHelper.ContextRetentionMode.NoRetention);

        #region IInvokeServiceOps Members

        object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
        {
            MobiusAnnotationService op = (MobiusAnnotationService)opCode;
            switch (op)
            {
                case MobiusAnnotationService.CreateInstance:
                    {
                        int instanceId = -1;
                        Mobius.UAL.AnnotationDao instance = new Mobius.UAL.AnnotationDao();
                        lock (_lockObject)
                        {
                            instanceId = _nextInstanceId++;
                            _instanceIdToInstance.Add(instanceId, instance);
                        }
                        return instanceId;
                    }
                case MobiusAnnotationService.DisposeInstance:
                    {
                        bool disposed = false;
                        int instanceId = (int)args[0];
                        Mobius.UAL.AnnotationDao instance = null;
                        lock (_lockObject)
                        {
                            if (_instanceIdToInstance.ContainsKey(instanceId))
                            {
                                instance = _instanceIdToInstance[instanceId];
                                _instanceIdToInstance.Remove(instanceId);
                            }
                        }
                        if (instance != null)
                        {
                            instance.Dispose();
                            disposed = true;
                        }
                        return disposed;
                    }
                case MobiusAnnotationService.Select:
                    {
                        int instanceId = (int)args[0];
                        int mthdVrsnId = (int)args[1];
                        string cmpndId = (string)args[2];
                        Mobius.UAL.AnnotationDao instance = null;
                        lock (_lockObject)
                        {
                            if (_instanceIdToInstance.ContainsKey(instanceId))
                            {
                                instance = _instanceIdToInstance[instanceId];
                            }
                        }
                        if (instance != null)
                        {
                            List<Mobius.Data.AnnotationVo> mobiusAnnotationVos =
                                instance.Select(mthdVrsnId, cmpndId);
                            List<AnnotationVo> annotationVos =
                                _transHelper.Convert<List<Mobius.Data.AnnotationVo>, List<AnnotationVo>>(mobiusAnnotationVos);
                            return annotationVos;
                        }
                        return null;
                    }
                case MobiusAnnotationService.SelectByResultId:
                    {
                        int instanceId = (int)args[0];
                        long rsltId = (long)args[1];
                        Mobius.UAL.AnnotationDao instance = null;
                        lock (_lockObject)
                        {
                            if (_instanceIdToInstance.ContainsKey(instanceId))
                            {
                                instance = _instanceIdToInstance[instanceId];
                            }
                        }
                        if (instance != null)
                        {
                            Mobius.Data.AnnotationVo mobiusAnnotationVo =
                                instance.Select(rsltId);
                            AnnotationVo annotationVo =
                                _transHelper.Convert<Mobius.Data.AnnotationVo, AnnotationVo>(mobiusAnnotationVo);
                            return annotationVo;
                        }
                        return null;
                    }
                case MobiusAnnotationService.SelectCompoundCount:
                    {
                        int instanceId = (int)args[0];
                        int mthdVrsnId = (int)args[1];
                        Mobius.UAL.AnnotationDao instance = null;
                        lock (_lockObject)
                        {
                            if (_instanceIdToInstance.ContainsKey(instanceId))
                            {
                                instance = _instanceIdToInstance[instanceId];
                            }
                        }
                        if (instance != null)
                        {
                            int count = instance.SelectCompoundCount(mthdVrsnId);
                            return count;
                        }
                        return null;
                    }

                case MobiusAnnotationService.SelectRowCount:
                    {
                        int instanceId = (int)args[0];
                        int mthdVrsnId = (int)args[1];
                        Mobius.UAL.AnnotationDao instance = null;
                        lock (_lockObject)
                        {
                            if (_instanceIdToInstance.ContainsKey(instanceId))
                            {
                                instance = _instanceIdToInstance[instanceId];
                            }
                        }
                        if (instance != null)
                        {
                            int rowCount = instance.SelectRowCount(mthdVrsnId);
                            return rowCount;
                        }
                        return null;
                    }
                case MobiusAnnotationService.GetNonNullRsltCnt:
                    {
                        int instanceId = (int)args[0];
                        int mthdVrsnId = (int)args[1];
                        Mobius.UAL.AnnotationDao instance = null;
                        lock (_lockObject)
                        {
                            if (_instanceIdToInstance.ContainsKey(instanceId))
                            {
                                instance = _instanceIdToInstance[instanceId];
                            }
                        }
                        if (instance != null)
                        {
                            int rowCount = instance.GetNonNullRsltCnt(mthdVrsnId);
                            return rowCount;
                        }
                        return null;
                    }
                case MobiusAnnotationService.GetNextIdsLong:
                    {
                        int instanceId = (int)args[0];
                        int seqQueueRequestSize = (int)args[1];
                        Mobius.UAL.AnnotationDao instance = null;
                        lock (_lockObject)
                        {
                            if (_instanceIdToInstance.ContainsKey(instanceId))
                            {
                                instance = _instanceIdToInstance[instanceId];
                            }
                        }
                        if (instance != null)
                        {
                            //work-around for .Net bug for writes of multiples of 128
                            // (See: http://social.msdn.microsoft.com/Forums/en/wcf/thread/0c71eedb-6c71-4b94-97c7-332195a7eb5c )
                            if (seqQueueRequestSize % 2 == 0)
                            {
                                seqQueueRequestSize++;
                            }
                            long[] nextIds =
                                instance.GetNextIdsLong(seqQueueRequestSize);
                            return nextIds;
                        }
                        return null;
                    }
                case MobiusAnnotationService.SetArchiveStatus:
                    {
                        int instanceId = (int)args[0];
                        long rsltId = (long)args[1];
                        Mobius.UAL.AnnotationDao instance = null;
                        lock (_lockObject)
                        {
                            if (_instanceIdToInstance.ContainsKey(instanceId))
                            {
                                instance = _instanceIdToInstance[instanceId];
                            }
                        }
                        if (instance != null)
                        {
                            int result =
                                instance.SetArchiveStatus(rsltId);
                            return result;
                        }
                        return null;
                    }
                case MobiusAnnotationService.SetResultGroupArchiveStatus:
                    {
                        int instanceId = (int)args[0];
                        long rsltGrpId = (long)args[1];
                        Mobius.UAL.AnnotationDao instance = null;
                        lock (_lockObject)
                        {
                            if (_instanceIdToInstance.ContainsKey(instanceId))
                            {
                                instance = _instanceIdToInstance[instanceId];
                            }
                        }
                        if (instance != null)
                        {
                            int result =
                                instance.SetResultGroupArchiveStatus(rsltGrpId);
                            return result;
                        }
                        return null;
                    }
                case MobiusAnnotationService.DeleteCompound:
                    {
                        int instanceId = (int)args[0];
                        int mthdVrsnId = (int)args[1];
                        string cmpndId = (string)args[2];
                        Mobius.UAL.AnnotationDao instance = null;
                        lock (_lockObject)
                        {
                            if (_instanceIdToInstance.ContainsKey(instanceId))
                            {
                                instance = _instanceIdToInstance[instanceId];
                            }
                        }
                        if (instance != null)
                        {
                            int result =
                                instance.DeleteCompound(mthdVrsnId, cmpndId);
                            return result;
                        }
                        return null;
                    }
                case MobiusAnnotationService.DeleteTable:
                    {
                        int instanceId = (int)args[0];
                        int mthdVrsnId = (int)args[1];
                        bool useTransactions = (bool)args[2];
                        Mobius.UAL.AnnotationDao instance = null;
                        lock (_lockObject)
                        {
                            if (_instanceIdToInstance.ContainsKey(instanceId))
                            {
                                instance = _instanceIdToInstance[instanceId];
                            }
                        }
                        if (instance != null)
                        {
                            int result =
                                instance.DeleteTable(mthdVrsnId, useTransactions);
                            return result;
                        }
                        return null;
                    }

                case MobiusAnnotationService.DeleteResultGroup:
                    {
                        int instanceId = (int)args[0];
                        long rsltGrpId = (long)args[1];
                        Mobius.UAL.AnnotationDao instance = null;
                        lock (_lockObject)
                        {
                            if (_instanceIdToInstance.ContainsKey(instanceId))
                            {
                                instance = _instanceIdToInstance[instanceId];
                            }
                        }
                        if (instance != null)
                        {
                            int result =
                                instance.DeleteResultGroup(rsltGrpId);
                            return result;
                        }
                        return null;
                    }

                case MobiusAnnotationService.Insert:
                    {
                        int instanceId = (int)args[0];
                        List<AnnotationVo> voList = (List<AnnotationVo>)args[1];
                        Mobius.UAL.AnnotationDao instance = null;
                        lock (_lockObject)
                        {
                            if (_instanceIdToInstance.ContainsKey(instanceId))
                            {
                                instance = _instanceIdToInstance[instanceId];
                            }
                        }
                        if (instance != null)
                        {
                            List<Mobius.Data.AnnotationVo> mobiusAnnotationVos =
                                _transHelper.Convert<List<AnnotationVo>, List<Mobius.Data.AnnotationVo>>(voList);
                            bool result =
                                instance.Insert(mobiusAnnotationVos);
                            return result;
                        }
                        return null;
                    }

								case MobiusAnnotationService.InsertUpdateRow:
										{
											int instanceId = (int)args[0];
											List<AnnotationVo> voList = (List<AnnotationVo>)args[1];
											Mobius.UAL.AnnotationDao instance = null;
											lock (_lockObject)
											{
												if (_instanceIdToInstance.ContainsKey(instanceId))
												{
													instance = _instanceIdToInstance[instanceId];
												}
											}
											if (instance != null)
											{
												List<Mobius.Data.AnnotationVo> mobiusAnnotationVos =
														_transHelper.Convert<List<AnnotationVo>, List<Mobius.Data.AnnotationVo>>(voList);

												long newRsltId = instance.InsertUpdateRow(mobiusAnnotationVos[0]);
												return newRsltId;
											}

											else return null;
										}

								case MobiusAnnotationService.InsertUpdateRowAndUserObjectHeader:
										{
											List<AnnotationVo> voList = (List<AnnotationVo>)args[0];
											List<Mobius.Data.AnnotationVo> mobiusAnnotationVos =
											_transHelper.Convert<List<AnnotationVo>, List<Mobius.Data.AnnotationVo>>(voList);

											long newRsltId = Mobius.UAL.AnnotationDao.InsertUpdateRowAndUserObjectHeader(mobiusAnnotationVos[0]);
											return newRsltId;
										}

								case MobiusAnnotationService.DeepClone:
										{
											UserObjectNode uoNode = (UserObjectNode)args[0];
											Mobius.Data.UserObject mobiusUO = _transHelper.Convert<UserObjectNode, Mobius.Data.UserObject>(uoNode);

											Mobius.Data.UserObject fullMobiusUO = Mobius.UAL.AnnotationDao.DeepClone(mobiusUO);

											UserObjectNode fullUONode = _transHelper.Convert<Mobius.Data.UserObject, UserObjectNode>(fullMobiusUO);
											return fullUONode;
										}

								case MobiusAnnotationService.UpdateCid:
                    {
                        int instanceId = (int)args[0];
                        int mthdVrsnId = (int)args[1];
                        string oldCid = (string)args[2];
                        string newCid = (string)args[3];
                        Mobius.UAL.AnnotationDao instance = null;
                        lock (_lockObject)
                        {
                            if (_instanceIdToInstance.ContainsKey(instanceId))
                            {
                                instance = _instanceIdToInstance[instanceId];
                            }
                        }
                        if (instance != null)
                        {
                            int result =
                                instance.UpdateCid(mthdVrsnId, oldCid, newCid);
                            return result;
                        }
                        return null;
                    }

                case MobiusAnnotationService.UpdateResultGroupCid:
                    {
                        int instanceId = (int)args[0];
                        long rsltGrpId = (long)args[1];
                        string oldCid = (string)args[2];
                        string newCid = (string)args[3];
                        Mobius.UAL.AnnotationDao instance = null;
                        lock (_lockObject)
                        {
                            if (_instanceIdToInstance.ContainsKey(instanceId))
                            {
                                instance = _instanceIdToInstance[instanceId];
                            }
                        }
                        if (instance != null)
                        {
                            int result =
                                instance.UpdateResultGroupCid(rsltGrpId, oldCid, newCid);
                            return result;
                        }
                        return null;
                    }

                case MobiusAnnotationService.BeginTransaction:
                    {
                        int instanceId = (int)args[0];
                        Mobius.UAL.AnnotationDao instance = null;
                        lock (_lockObject)
                        {
                            if (_instanceIdToInstance.ContainsKey(instanceId))
                            {
                                instance = _instanceIdToInstance[instanceId];
                            }
                        }
                        if (instance != null)
                        {
                            instance.BeginTransaction();
                            return true;
                        }
                        return false;
                    }

                case MobiusAnnotationService.Commit:
                    {
                        int instanceId = (int)args[0];
                        Mobius.UAL.AnnotationDao instance = null;
                        lock (_lockObject)
                        {
                            if (_instanceIdToInstance.ContainsKey(instanceId))
                            {
                                instance = _instanceIdToInstance[instanceId];
                            }
                        }
                        if (instance != null)
                        {
                            instance.Commit();
                            return true;
                        }
                        return false;
                    }

            }
            return null;
        }

        #endregion
    }
}
