using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Mobius.ComOps
{
	/// <summary>
	/// Schedule a method to be called after a delay to allow exiting conditions such as being within a menu item click
	/// </summary>

	public class DelayedCallback
	{
		SimpleDelegate DelayedCallbackMethod;
		ObjectParmDelegate DelayedCallbackMethodWithParms;
		object Parms;
		StackTrace RequestStack; // stack for requestor
		System.Windows.Forms.Timer DelayTimer;

		/// <summary>
		/// Basic constructor (private)
		/// </summary>

		private DelayedCallback()
		{
			return;
		}

		/// <summary>
		/// Schedule a delayed callback that includes a parameter object
		/// </summary>
		/// <param name="delayedCallbackMethod"></param>
		/// <param name="parms"></param>
		/// <param name="waitingMilliSeconds"></param>

		public static void Schedule(
			ObjectParmDelegate delayedCallbackMethod,
			object parms = null,
			int waitingMilliSeconds = 100)
		{
			DelayedCallback dcb = new DelayedCallback();
			dcb.DelayedCallbackMethodWithParms = delayedCallbackMethod;
			dcb.Parms = parms;
			dcb.RequestStack = new StackTrace(true);

			dcb.Schedule(waitingMilliSeconds);
			return;
		}
		/// <summary>
		/// Schedule a delayed callback with no parameters
		/// </summary>
		/// <param name="simpleDelayedCallbackMethod"></param>
		/// <param name="waitingMilliSeconds"></param>

		public static void Schedule(
			SimpleDelegate simpleDelayedCallbackMethod,
			int waitingMilliSeconds = 100)
		{
			DelayedCallback dcb = new DelayedCallback();
			dcb.DelayedCallbackMethod = simpleDelayedCallbackMethod;
			dcb.RequestStack = new StackTrace(true);

			dcb.Schedule(waitingMilliSeconds);
			return;
		}

		void Schedule
				(int waitingMilliSeconds = 100)
		{
			DelayTimer = new System.Windows.Forms.Timer();
			DelayTimer.Tick += new System.EventHandler(DelayTimer_Tick);
			DelayTimer.Interval = waitingMilliSeconds;
			DelayTimer.Enabled = true; // start timer
			return;
		}

		/// <summary>
		/// Delay over, call the method associated with the callback
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void DelayTimer_Tick(object sender, EventArgs e)
		{
			DelayTimer.Enabled = false;
			if (DelayedCallbackMethod != null)
				DelayedCallbackMethod();

			else if (DelayedCallbackMethodWithParms != null)
				DelayedCallbackMethodWithParms(Parms);

			else throw new Exception("DelayedCallbackMethod not defined");
		}
	}
}
