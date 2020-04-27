using Mobius.Services.Native.ServiceOpCodes;
using Mobius.ComOps;
using Mobius.Data;
using Mobius.MetaFactoryNamespace;
using Mobius.QueryEngineLibrary;
using Mobius.UAL;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Data;
using System.Data.Common;

namespace Mobius.Services.Native.OpInvokers
{

	public class MobiusTaskSchedulerServiceOpInvoker : IInvokeServiceOps
	{
		private delegate object CommandDelegate(ScheduledTask task);
		private Dictionary<ScheduledTaskTypes, CommandDelegate> _commandMap;

		private object _lockObject = new object();
		private int _nextJobId = 0;
		private Dictionary<int, ScheduledTask> _jobs = new Dictionary<int, ScheduledTask>();

		// Save these values so that they can be restored if needed

		private static string _defaultMetaDataDir;
		private static string _defaultMetaTreeDir;
		private static string _defaultMetaTableXmlDir;

		public MobiusTaskSchedulerServiceOpInvoker()
		{
			//Save these values so that they can be restored

			_defaultMetaDataDir = ServicesDirs.MetaDataDir;
			_defaultMetaTreeDir = MetaTreeFactory.MetaTreeDir;
			_defaultMetaTableXmlDir = MetaTableFactory.MetaTableXmlFolder;

			//Populate the command table

			_commandMap = new Dictionary<ScheduledTaskTypes, CommandDelegate>();
			_commandMap.Add(ScheduledTaskTypes.ExecuteCommand,
					new CommandDelegate(ExecuteCommand));
		}

// This supports two kinds of tasks:
//  1.  Execute -- wait for the result of the request
//  2.  Submit  -- get a job id and poll for completion

/// <summary>
/// 2. Start in-session job
/// </summary>
/// <param name="task"></param>
/// <returns></returns>

		private ScheduledTask StartJob(ScheduledTask task)
		{
			//add the task to the list of running jobs
			task.Status = ScheduledTaskStatus.Running;
			lock (_lockObject)
			{
				task.JobId = _nextJobId++;
				_jobs.Add(task.JobId, task);
			}

			ParameterizedThreadStart jobThreadStart = new ParameterizedThreadStart(ExecuteCommandAsJob);
			Thread jobThread = new Thread(jobThreadStart);
			jobThread.IsBackground = true;
			jobThread.Start(task);

			return task;
		}

		private void ExecuteCommandAsJob(object taskObj)
		{
			ScheduledTask task = taskObj as ScheduledTask;
			task.Status = ScheduledTaskStatus.Running;
			ExecuteCommand(task);
		}

		private ScheduledTask CheckJob(int jobId)
		{
			ScheduledTask task = null;
			lock (_lockObject)
			{
				if (_jobs.ContainsKey(jobId))
				{
					task = _jobs[jobId];
					if (task.Status == ScheduledTaskStatus.Succeeded ||
							task.Status == ScheduledTaskStatus.Failed)
					{
						_jobs.Remove(jobId);
					}
				}
			}
			return task;
		}

		/// <summary>
		/// Call method to process a command line command
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns>Command response or null if not a command</returns>

		private object ExecuteCommand(ScheduledTask task)
		{
			string commandLine = task.CommandArg;
			task.Result = Mobius.ToolServices.CommandLineService.ExecuteCommand(commandLine);
			task.Status = ScheduledTaskStatus.Succeeded;
			return task;
		}

		#region IInvokeServiceOps Members

		object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
		{
			if (_defaultMetaDataDir == null)
			{
				_defaultMetaDataDir = ServicesDirs.MetaDataDir;
			}

			MobiusTaskSchedulerService op = (MobiusTaskSchedulerService)opCode;
			switch (op)
			{
				case MobiusTaskSchedulerService.Execute:
					{
						ScheduledTaskTypes taskType = (ScheduledTaskTypes)args[0];
						string commandArg = (string)args[1];
						ScheduledTask task = new ScheduledTask(taskType, commandArg);
						object response = ExecuteCommand(task);
						return response;
					}
				case MobiusTaskSchedulerService.SubmitJob:
					{
						ScheduledTaskTypes taskType = (ScheduledTaskTypes)args[0];
						string commandArg = (string)args[1];
						ScheduledTask task = new ScheduledTask(taskType, commandArg);
						StartJob(task);
						return task;
					}
				case MobiusTaskSchedulerService.CheckJobStatus:
					{
						int jobId = (int)args[0];
						ScheduledTask task = CheckJob(jobId);
						return task;
					}
			}
			return null;
		}

		#endregion
	}
}
