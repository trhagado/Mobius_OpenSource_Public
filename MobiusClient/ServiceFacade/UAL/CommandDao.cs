using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using ServiceTypes = Mobius.Services.Types;
using Mobius.Services.Native;
using Mobius.Services.Native.ServiceOpCodes;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;


namespace Mobius.ServiceFacade
{

	/// <summary>
	/// Start background processes and other general commands
	/// </summary>

	public class CommandDao
	{

		/// <summary>
		/// Execute a command
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>

		public static string Execute(string command)
		{
			return Execute("ExecuteCommand", command);
			//return;
		}

		/// <summary>
		/// Execute a command
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>

		public static string Execute(string command, string commandArgs)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				ScheduledTaskTypes taskType = // get type of scheduled task
						(ScheduledTaskTypes)Enum.Parse(typeof(ScheduledTaskTypes), command);

				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusTaskSchedulerService,
					MobiusTaskSchedulerService.Execute,
					new object[] { (int)taskType, commandArgs });

				if (resultObject == null) return null;

				string result = "";
				ScheduledTask scheduledTask = resultObject.Value as ScheduledTask;
				if (scheduledTask == null)
					throw new Exception("Command failed (null resultObject)");

				else
				{
					if (scheduledTask.Status == ScheduledTaskStatus.Succeeded)
					{
						if (scheduledTask.Result != null)
							result = scheduledTask.Result.ToString();
					}
					else
					{
						result = scheduledTask.Status.ToString() +
								((scheduledTask.Result == null) ? "" : ": " + scheduledTask.Result.ToString());
						throw new Exception(result);
					}
				}
				return result;
			}

			else return Mobius.ToolServices.CommandLineService.ExecuteCommand(commandArgs);
		}

		/// <summary>
		/// ExecuteLongRunningCommand
		/// </summary>
		/// <param name="longRunningCommandObject"></param>

		public static void ExecuteLongRunningCommand(object longRunningCommandObject)
		{
			//Assume that the invocation of this was already outside of the gui thread
			// -- a good thing since we're going to "sleep" between status polls...
			LongRunningCommand longRunningCommand = (LongRunningCommand)longRunningCommandObject;
			if (ServiceFacade.UseRemoteServices)
			{
				//verify the taskType client-side...
				// no reason to make a round-trip to the server if we can fail faster
				Services.Native.ScheduledTaskTypes taskType =
						(ScheduledTaskTypes)Enum.Parse(
								typeof(ScheduledTaskTypes), longRunningCommand.Command);

				INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
				Services.Native.NativeMethodTransportObject resultObject =
						ServiceFacade.InvokeNativeMethod(nativeClient,
								(int)ServiceCodes.MobiusTaskSchedulerService,
								(int)MobiusTaskSchedulerService.SubmitJob,
								new Services.Native.NativeMethodTransportObject(new object[] { (int)taskType, longRunningCommand.CommandArgs }));

				Services.Native.ScheduledTask task = (Services.Native.ScheduledTask)resultObject.Value;
				if (task != null)
				{
					//do a SHORT sleep before the first check in case the task failed or completed very quickly
					task.SuggestedTimeBetweenPolls = TimeSpan.FromSeconds(0.03);
					while (task.Status == ScheduledTaskStatus.Running)
					{
						if (longRunningCommand.UpdateProgress != null && task.Result != null &&
								!String.IsNullOrEmpty(task.Result.ToString()))
						{
							longRunningCommand.UpdateProgress(task.Result.ToString(), ComOps.UmlautMobius.String, longRunningCommand.AllowCancel);
						}
						Thread.Sleep(task.SuggestedTimeBetweenPolls);
						resultObject =
								ServiceFacade.InvokeNativeMethod(nativeClient,
										(int)ServiceCodes.MobiusTaskSchedulerService,
										(int)MobiusTaskSchedulerService.CheckJobStatus,
										new Services.Native.NativeMethodTransportObject(new object[] { task.JobId }));
						task = (Services.Native.ScheduledTask)resultObject.Value;
					}
					longRunningCommand.Result = task.Result;
				}
				((System.ServiceModel.IClientChannel)nativeClient).Close();

				longRunningCommand.IsRunning = false;
				longRunningCommand.SignalCompletion();

				if (longRunningCommand.OnCompletion != null)
				{
					longRunningCommand.OnCompletion();
				}
				if (longRunningCommand.HideProgress != null)
				{
					longRunningCommand.HideProgress();
				}
				if (longRunningCommand.ShowResponse != null && task.Result != null)
				{
					longRunningCommand.ShowResponse(task.Result.ToString());
				}
			}
		}
	}

	public delegate void LongRunningCommandUpdateDelegate(string caption, string title, bool allowCancel);
	public delegate void LongRunningCommandHideProgressDelegate();
	public delegate void LongRunningCommandShowResponseDelegate(string message);
	public delegate void LongRunningCommandOnCompletionDelegate();

	public class LongRunningCommand
	{
		public string Command = null;
		public string CommandArgs = null;
		public bool IsRunning = true;
		public object Result = null;

		public bool AllowCancel = false;
		public string CancelProgressMessage = null;
		public LongRunningCommandUpdateDelegate UpdateProgress = null;
		public LongRunningCommandHideProgressDelegate HideProgress = null;
		public LongRunningCommandShowResponseDelegate ShowResponse = null;
		public LongRunningCommandOnCompletionDelegate OnCompletion = null;

		private Semaphore _completionSemaphore = new Semaphore(0, 1);

		public LongRunningCommand(string command)
		{
			Command = command;
		}

		internal void SignalCompletion()
		{
			_completionSemaphore.Release();
		}

		public void WaitForCompletion()
		{
			_completionSemaphore.WaitOne();
		}
	}
}
