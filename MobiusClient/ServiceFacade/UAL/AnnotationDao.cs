using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections.Generic;
using System.Text;

using ServiceTypes = Mobius.Services.Types;

namespace Mobius.ServiceFacade
{
	public class AnnotationDao
	{
		private ServiceTypes.Session session; //note the session so that instances know if their session dies
		private int instanceId = -1;

		static Queue<long> SeqQueue = new Queue<long>();
		static int SeqQueueRequestSize = 0;

		Mobius.UAL.AnnotationDao Instance;

		/// <summary>
		/// Constructor
		/// </summary>

		public AnnotationDao()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusAnnotationService,
								(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.CreateInstance,
								null);
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				instanceId = (int)resultObject.Value;
			}
			else Instance = new Mobius.UAL.AnnotationDao();
			return;
		}

		/// <summary>
		/// Select all rows for a method and compoundId
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <param name="cmpndId"></param>
		/// <returns></returns>

		public List<AnnotationVo> Select(
			int mthdVrsnId,
			string cmpndId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusAnnotationService,
								(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.Select,
								new Services.Native.NativeMethodTransportObject(new object[] { instanceId, mthdVrsnId, cmpndId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				List<ServiceTypes.AnnotationVo> serviceAnnotationVos =
						(List<ServiceTypes.AnnotationVo>)resultObject.Value;
				List<AnnotationVo> annotationVos =
						ServiceFacade.TypeConversionHelper.Convert<List<ServiceTypes.AnnotationVo>, List<AnnotationVo>>(serviceAnnotationVos);
				return annotationVos;
			}
			else return Instance.Select(mthdVrsnId, cmpndId);
		}

		/// <summary>
		/// Select row for specified result id
		/// </summary>
		/// <param name="rsltId"></param>
		/// <returns></returns>

		public AnnotationVo Select(
			long rsltId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusAnnotationService,
								(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.SelectByResultId,
								new Services.Native.NativeMethodTransportObject(new object[] { instanceId, rsltId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				if (resultObject == null) return null;
				ServiceTypes.AnnotationVo serviceAnnotationVo =
						(ServiceTypes.AnnotationVo)resultObject.Value;
				AnnotationVo annotationVo =
						ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.AnnotationVo, AnnotationVo>(serviceAnnotationVo);
				return annotationVo;
			}
			else return Instance.Select(rsltId);
		}

		/// <summary>
		/// Get compound count for method
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <returns></returns>

		public int SelectCompoundCount(
			int mthdVrsnId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusAnnotationService,
								(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.SelectCompoundCount,
								new Services.Native.NativeMethodTransportObject(new object[] { instanceId, mthdVrsnId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				int count = (int)resultObject.Value;
				return count;
			}
			else return Instance.SelectCompoundCount(mthdVrsnId);
		}

		/// <summary>
		///	Get row count for method
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <returns></returns>

		public int SelectRowCount(
			int mthdVrsnId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusAnnotationService,
								(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.SelectRowCount,
								new Services.Native.NativeMethodTransportObject(new object[] { instanceId, mthdVrsnId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				int count = (int)resultObject.Value;
				return count;
			}
			else return Instance.SelectRowCount(mthdVrsnId);
		}

        /// <summary>
        ///	Get row count for method
        /// </summary>
        /// <param name="mthdVrsnId"></param>
        /// <returns></returns>

        public int GetNonNullRsltCnt(
            int mthdVrsnId)
        {
            if (ServiceFacade.UseRemoteServices)
            {
                Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
                Services.Native.NativeMethodTransportObject resultObject =
                        ServiceFacade.InvokeNativeMethod(nativeClient,
                                (int)Services.Native.ServiceCodes.MobiusAnnotationService,
                                (int)Services.Native.ServiceOpCodes.MobiusAnnotationService.GetNonNullRsltCnt,
                                new Services.Native.NativeMethodTransportObject(new object[] { instanceId, mthdVrsnId }));
                ((System.ServiceModel.IClientChannel)nativeClient).Close();
                int count = (int)resultObject.Value;
                return count;
            }
            else
                return Instance.GetNonNullRsltCnt(mthdVrsnId);
        }

        /// <summary>
        /// Get next identifier from sequence
        /// </summary>
        /// <returns></returns>

        public long GetNextIdLong()
		{ // buffer a queue of ids on the client to reduce trips to server
			// http://social.msdn.microsoft.com/Forums/en/wcf/thread/0c71eedb-6c71-4b94-97c7-332195a7eb5c
			// annotation op invoker on services side modified to add one to avoid 512 byte problem
			if (SeqQueue.Count == 0) // get more ids from server if needed
			{
				if (SeqQueueRequestSize == 0)
					SeqQueueRequestSize = 1;
				else if (SeqQueueRequestSize < 1024)
					SeqQueueRequestSize *= 2; // double size on each call until we get to 1k

				if (ServiceFacade.UseRemoteServices)
				{
					Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
					Services.Native.NativeMethodTransportObject resultObject =
							ServiceFacade.InvokeNativeMethod(nativeClient,
									(int)Services.Native.ServiceCodes.MobiusAnnotationService,
									(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.GetNextIdsLong,
									new Services.Native.NativeMethodTransportObject(new object[] { instanceId, SeqQueueRequestSize }));
					((System.ServiceModel.IClientChannel)nativeClient).Close();
					long[] ids = (long[])resultObject.Value;
					SeqQueue = new Queue<long>(ids);
				}
				else
				{
					long[] ids = Instance.GetNextIdsLong(SeqQueueRequestSize);
					SeqQueue = new Queue<long>(ids);
				}
			}

			long nextVal = SeqQueue.Dequeue(); // may overflow 
			return nextVal;
		}

		/// <summary>
		/// Set archive status for a single result row
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <param name="cmpndId"></param>
		/// <returns></returns>

		public int SetArchiveStatus(
			long rsltId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusAnnotationService,
								(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.SetArchiveStatus,
								new Services.Native.NativeMethodTransportObject(new object[] { instanceId, rsltId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				int result = (int)resultObject.Value;
				return result;
			}
			else return Instance.SetArchiveStatus(rsltId);
		}

		/// <summary>
		/// Update a compoundId for a resultGroup
		/// </summary>
		/// <param name="rsltGrpId"></param>
		/// <param name="cmpndId"></param>
		/// <returns></returns>

		public int UpdateResultGroupCid(
			long rsltGrpId,
			string oldCid,
			string newCid)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusAnnotationService,
								(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.UpdateResultGroupCid,
								new Services.Native.NativeMethodTransportObject(new object[] { instanceId, rsltGrpId, oldCid, newCid }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				int result = (int)resultObject.Value;
				return result;
			}

			else return Instance.UpdateResultGroupCid(rsltGrpId, oldCid, newCid);
		}

		/// <summary>
		/// Set archive status for all rows in a result group
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <param name="cmpndId"></param>
		/// <returns></returns>

		public int SetResultGroupArchiveStatus(long rsltGrpId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusAnnotationService,
								(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.SetResultGroupArchiveStatus,
								new Services.Native.NativeMethodTransportObject(new object[] { instanceId, rsltGrpId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				int result = (int)resultObject.Value;
				return result;
			}
			else return Instance.SetArchiveStatus(rsltGrpId);
		}

		/// <summary>
		/// Delete all rows for a result group
		/// </summary>
		/// <param name="rsltId"></param>
		/// <returns></returns>

		public int DeleteResultGroup(long rsltGrpId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusAnnotationService,
								(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.DeleteResultGroup,
								new Services.Native.NativeMethodTransportObject(new object[] { instanceId, rsltGrpId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				int result = (int)resultObject.Value;
				return result;
			}
			else return Instance.DeleteResultGroup(rsltGrpId);
		}

		/// <summary>
		/// Permanently delete all data for a warehouse method table
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <returns></returns>

		public int DeleteTable(
			int mthdVrsnId)
		{
			return DeleteTable(mthdVrsnId, true);
		}

		/// <summary>
		/// Permanently delete all data for a warehouse method table
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <param name="useTransactions"></param>
		/// <param name="showProgressMsg"></param>
		/// <returns></returns>

		public int DeleteTable(
			int mthdVrsnId,
			bool useTransactions)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusAnnotationService,
								(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.DeleteTable,
								new Services.Native.NativeMethodTransportObject(new object[] { instanceId, mthdVrsnId, useTransactions }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				int count = (int)resultObject.Value;
				return count;
			}
			else return Instance.DeleteTable(mthdVrsnId, useTransactions);
		}

		/// <summary>
		/// Delete data in a metatable for a compound
		/// </summary>
		/// <param name="cmpndId"></param>
		/// <param name="mthdVrsnId"></param>
		/// <returns></returns>

		public int DeleteCompound(
			int mthdVrsnId,
			string cmpndId)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusAnnotationService,
								(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.DeleteCompound,
								new Services.Native.NativeMethodTransportObject(new object[] { instanceId, mthdVrsnId, cmpndId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				int count = (int)resultObject.Value;
				return count;
			}
			else return Instance.DeleteCompound(mthdVrsnId, cmpndId);
		}

		/// <summary>
		/// Insert a list of rows
		/// </summary>
		/// <param name="vo"></param>
		/// <returns></returns>

		public bool Insert(List<AnnotationVo> voList)
		{
			if (voList == null || voList.Count == 0) return true; // nothing to do

			if (ServiceFacade.UseRemoteServices)
			{
				List<ServiceTypes.AnnotationVo> serviceVoList =
						ServiceFacade.TypeConversionHelper.Convert<List<AnnotationVo>, List<ServiceTypes.AnnotationVo>>(voList);
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
					ServiceFacade.InvokeNativeMethod(nativeClient,
					(int)Services.Native.ServiceCodes.MobiusAnnotationService,
					(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.Insert,
					new Services.Native.NativeMethodTransportObject(new object[] { instanceId, serviceVoList }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				bool succeeded = (bool)resultObject.Value;
				return succeeded;
			}
			else return Instance.Insert(voList);
		}

		/// <summary>
		/// Update a single value immediately with a single service call for fast response time
		/// </summary>
		/// <param name="vo"></param>

		public long InsertUpdateRow(AnnotationVo vo)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				List<AnnotationVo> voList = new List<AnnotationVo>();
				voList.Add(vo);
				List<ServiceTypes.AnnotationVo> serviceVoList =
					ServiceFacade.TypeConversionHelper.Convert<List<AnnotationVo>, List<ServiceTypes.AnnotationVo>>(voList);
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
					ServiceFacade.InvokeNativeMethod(nativeClient,
					(int)Services.Native.ServiceCodes.MobiusAnnotationService,
					(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.InsertUpdateRow,
					new Services.Native.NativeMethodTransportObject(new object[] { instanceId, serviceVoList }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				long newRsltId = (long)resultObject.Value;
				return newRsltId;
			}

			else return Instance.InsertUpdateRow(vo);
		}

		/// <summary>
		/// Fast insert/update of a single row including creation of the AnnotationDao object, transaction and Header update
		/// </summary>
		/// <param name="vo"></param>
		/// <returns></returns>

		public static long InsertUpdateRowAndUserObjectHeader(AnnotationVo vo)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				List<AnnotationVo> voList = new List<AnnotationVo>();
				voList.Add(vo);
				List<ServiceTypes.AnnotationVo> serviceVoList =
					ServiceFacade.TypeConversionHelper.Convert<List<AnnotationVo>, List<ServiceTypes.AnnotationVo>>(voList);
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
					ServiceFacade.InvokeNativeMethod(nativeClient,
					(int)Services.Native.ServiceCodes.MobiusAnnotationService,
					(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.InsertUpdateRowAndUserObjectHeader,
					new Services.Native.NativeMethodTransportObject(new object[] { serviceVoList }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				long newRsltId = (long)resultObject.Value;
				return newRsltId;
			}

			else return UAL.AnnotationDao.InsertUpdateRowAndUserObjectHeader(vo);
		}

		/// <summary>
		/// Perform a deep clone of an annotation table UserObject including the underlying data
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static UserObject DeepClone(UserObject uo)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				ServiceTypes.UserObjectNode serviceUONode =
							ServiceFacade.TypeConversionHelper.Convert<UserObject, ServiceTypes.UserObjectNode>(uo);
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
					ServiceFacade.InvokeNativeMethod(nativeClient,
					(int)Services.Native.ServiceCodes.MobiusAnnotationService,
					(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.DeepClone,
					new Services.Native.NativeMethodTransportObject(new object[] { serviceUONode }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();

				if (resultObject == null) return null;

				ServiceTypes.UserObjectNode uoNode = (ServiceTypes.UserObjectNode)resultObject.Value;
				UserObject uo2 = ServiceFacade.TypeConversionHelper.Convert<ServiceTypes.UserObjectNode, UserObject>(uoNode);
				return uo2;
			}

			else return UAL.AnnotationDao.DeepClone(uo);
		}

		/// <summary>
		/// Update a compoundId for a table
		/// </summary>
		/// <param name="mthdVrsnId"></param>
		/// <param name="cmpndId"></param>
		/// <returns></returns>

		public int UpdateCid(
			int mthdVrsnId,
			string oldCid,
			string newCid)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusAnnotationService,
								(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.UpdateCid,
								new Services.Native.NativeMethodTransportObject(new object[] { instanceId, mthdVrsnId, oldCid, newCid }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				int count = (int)resultObject.Value;
				return count;
			}
			else return Instance.UpdateCid(mthdVrsnId, oldCid, newCid);
		}

		/// <summary>
		/// Begin a transaction
		/// </summary>

		public void BeginTransaction()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusAnnotationService,
								(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.BeginTransaction,
								new Services.Native.NativeMethodTransportObject(new object[] { instanceId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				return;
			}
			else Instance.BeginTransaction();
		}

		/// <summary>
		/// Commit transaction
		/// </summary>

		public void Commit()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusAnnotationService,
								(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.Commit,
								new Services.Native.NativeMethodTransportObject(new object[] { instanceId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				return;
			}
			else Instance.Commit();
		}

		/// <summary>
		/// Dispose of any associated DataReaderDao
		/// </summary>
		/// 
		public void Dispose()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)Services.Native.ServiceCodes.MobiusAnnotationService,
								(int)Services.Native.ServiceOpCodes.MobiusAnnotationService.DisposeInstance,
								new Services.Native.NativeMethodTransportObject(new object[] { instanceId }));
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				return;
			}
			else Instance.Dispose();
		}

	}
}
