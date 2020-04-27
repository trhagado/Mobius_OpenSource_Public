using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Mobius.ComOps
{ 

	public class ApplicationRecovery
	{
		// Consts and Externs
		const string APPLICATION_CRASHED = "appCrashed";

		[Flags]
		private enum RestartFlags
		{
			NONE = 0,
			RESTART_NO_CRASH = 1,
			RESTART_NO_HANG = 2,
			RESTART_NO_PATCH = 4,
			RESTART_NO_REBOOT = 8
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern uint RegisterApplicationRestart(string pwsCommandLine, RestartFlags dwFlags);

		[DllImport("kernel32.dll")]
		private static extern uint RegisterApplicationRecoveryCallback(IntPtr pRecoveryCallback, IntPtr pvParameter, int dwPingInterval, int dwFlags);

		[DllImport("kernel32.dll")]
		private static extern uint ApplicationRecoveryInProgress(out bool pbCancelled);

		[DllImport("kernel32.dll")]
		public static extern uint ApplicationRecoveryFinished(bool bSuccess);

		// Delegates & Events
		private delegate int ApplicationRecoveryCallback(IntPtr pvParameter);
		public delegate void ApplicationCrashHandler();

		/// <summary>
		/// Handle this event.  
		/// This is where you will attempt to persist your data.
		/// </summary>
		public static event ApplicationCrashHandler OnApplicationCrash;

		// Register the application for restart notification.
		private static ApplicationRecoveryCallback RecoverApplication;

		/// <summary>
		/// Registers the application for notification by windows of a failure.
		/// </summary>
		/// <returns>true if successfully registered for restart notification</returns>   

		public static bool RegisterForRestart()
		{
			uint i = RegisterApplicationRestart(APPLICATION_CRASHED, RestartFlags.NONE);

			if (i == 0)
			{
				//Hook the callback function.
				RecoverApplication = new ApplicationRecoveryCallback(HandleApplicationCrash);
				IntPtr ptrOnApplicationCrash = Marshal.GetFunctionPointerForDelegate(RecoverApplication);

				i = RegisterApplicationRecoveryCallback(ptrOnApplicationCrash, IntPtr.Zero, 50000, 0);
			}

			return i == 0;
		}

		////////////////////// Data Persistance Methods //////////////////////

		/// <summary>
		/// This is the callback function that is executed in the event of the application crashing.
		/// It calls our event handler for OnPersistData.
		/// </summary>
		/// <param name="pvParameter"></param>
		/// <returns></returns>
		
		private static int HandleApplicationCrash(IntPtr pvParameter)
		{
			//Allow the user to cancel the recovery.  The timer polls for that cancel.
			using (System.Threading.Timer t = new System.Threading.Timer(CheckForRecoveryCancel, null, 1000, 1000))
			{
				//Handle this event in your own code
				if (OnApplicationCrash != null)
				{
					OnApplicationCrash();
					//Note: We will reload the data from persistant storage when the application restarts.
				}

				ApplicationRecoveryFinished(true);
			}

			return 0;
		}

		/// <summary>
		/// Checks to see if the user has cancelled the recovery.
		/// </summary>
		/// <param name="o"></param>

		private static void CheckForRecoveryCancel(object o)
		{
			bool userCancelled;
			ApplicationRecoveryInProgress(out userCancelled);

			if (userCancelled)
			{
				Environment.FailFast("User cancelled application recovery");
			}
		}
	}

}