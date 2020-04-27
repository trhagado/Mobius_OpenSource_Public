using Mobius.ComOps;

using DevExpress.XtraEditors;

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Progress form
	/// </summary>

	public partial class Progress : XtraForm
	{

		// Instance members

		int Id = 0; // instance id
		DateTime StartTime; // time started time display counter
		string LastTimeText; // text of last time displayed
		Size OriginalFormSize;
		Size OriginalCaptionSize;
		int OriginalLines = 3;
		int OriginalCharsPerLine = 45;
		object InstanceLock = new object();
		bool Discarded = false; // "discarded" may or may not be visible and destroyed

		// Static members

		public static Progress Instance; // current instance
		public static Form OwnerFormToUse; // owner form to use for the progress form

		static string PendingCaption = null;
		static string PendingTitle = null;
		static bool PendingAllowCancel = false;
		static string PendingCancellingMessage = null;
		static bool PendingShow = false;
		static bool PendingHide = false;

		static int LastUpdateTime = 0; // time progress last updated
		static int UpdateInterval = 1000; // update interval in milliseconds

		static bool Showing = false;
		static bool PerShowCancelRequestedFlag = false; // reset for each Progress.Show.
		static bool GlobalCancelRequestedFlag = false; // not reset for each Progress.Show.
		static int InstanceCount = 0;
		static int DiscardCount = 0;
		static int DestructorCallCount = 0;

		public static bool Debug = false; // => ClientState.IsDeveloper;

		public static Progress GetInstance()
		{
			if (Instance == null)
			{
				Instance = new Progress();
				//Instance.TopMost = true;
			}

			return Instance;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		private Progress()
		{
			InitializeComponent();

			Id = ++InstanceCount;

			OriginalFormSize = Size;
			OriginalCaptionSize = Caption.Size;

			if (Debug) ClientLog.Message("Progress - Creating instance: " + Id);
		}

		/// <summary>
		/// Finalizer / Destructor
		/// </summary>

		~Progress()
		{
			DestructorCallCount++;
			if (Debug) ClientLog.Message("Progress - Destroying instance: " + Id + ", Caption: " + Caption.Text);
			return;
		}

		/// <summary>
		/// Verify instance exists & create if not
		/// </summary>

		static void VerifyFormExistence()
		{
			if (GetInstance() == null) // || !Showing) // create a new instance if one isn't already showing
			{
				//Instance = new Progress();
				Showing = false;
			}
		}

		/// <summary>
		/// Show progress from thread other than the UI thread
		/// </summary>
		/// <param name="caption"></param>
		/// <param name="title"></param>
		/// <param name="allowCancel"></param>

		public static void InvokeShow(
		string caption,
		string title,
		bool allowCancel)
		{
			Progress.Show(caption, title, allowCancel);

			//if (Instance != null)
			//{
			//	Instance.Invoke(new ProgressDelegates.ShowDelegate(Show), caption, title, allowCancel);
			//}
			//else
			//{ //want the Instance to have been created IN THE UI THREAD!!!
			//	SessionManager.Instance.ShellForm.Invoke(new ProgressDelegates.ShowDelegate(Show), caption, title, allowCancel);
			//}
		}

		/// <summary>
		/// Hide progress from thread other than the UI thread
		/// </summary>

		public static void InvokeHide()
		{
			Progress.Hide();

			//if (Instance != null)
			//{
			//	Instance.Invoke(new ProgressDelegates.HideDelegate(Hide));
			//}
		}

		/// <summary>
		/// Show Progress message
		/// </summary>
		/// <param name="caption"></param>

		public static void Show(
				string caption)
		{
			Show(caption, UmlautMobius.String);
		}

		public static void Show(
				string caption,
				string title)
		{
			Show(caption, title, true);
		}

		public static void Show(
				string caption,
				string title,
				bool allowCancel)
		{
			Show(caption, title, allowCancel, "");
		}

		public static void Show(
	Progress msg)
		{
			if (msg == null || Lex.IsNullOrEmpty(msg.Text)) Hide(); // hide it 
			else Progress.Show(msg.Caption.Text, msg.Text, msg.Cancel.Enabled, msg.CancellingMessage.Text);
		}

		/// <summary>
		/// Post Progress message for display at next timer tick
		/// </summary>
		/// <param name="caption"></param>
		/// <param name="title"></param>
		/// <param name="allowCancel"></param>
		/// <param name="cancellingMessage"></param>

		public static void Show(
			string caption,
			string title,
			bool allowCancel,
			string cancellingMessage)
		{

			VerifyFormExistence();

			PendingCaption = caption;
			PendingTitle = title;
			PendingAllowCancel = allowCancel;
			PendingCancellingMessage = cancellingMessage;
			PendingShow = true;
			PendingHide = false;
			//if (Instance != null) Instance.ProgressTimer.Enabled = true;

			PerShowCancelRequestedFlag = false; 

			if (Debug) ClientLog.Message("Progress - Posting Show: " + caption);
			//if (Lex.Contains(caption, "Retrieving data")) caption = caption; // debug

			if (SS.I.LocalServiceMode) // if local service mode try to show message now since timer ticks may not be processed if in CPU intensive task
			{
				//while (PendingShow)
				{
					Thread.Sleep(10);
					UIMisc.DoEvents();

					string stackTrace = new System.Diagnostics.StackTrace(true).ToString();
					if (Lex.Contains(stackTrace, "_tick")) // if in a timer tick process immediately
						Instance.ProgressTimer_Tick(null, null);
				}
			}

			return;
		}


		/// <summary>
		/// Show progress message
		/// </summary>
		/// <param name="caption"></param>
		/// <param name="title"></param>
		/// <param name="allowCancel"></param>
		/// <param name="cancellingMessage"></param>

		void ShowInstance(
				string caption,
				string title,
				bool allowCancel,
				string cancellingMessage)
		{

			LastUpdateTime = TimeOfDay.Milliseconds();

			//SystemUtil.Beep(); // debug
			if (Debug) ClientLog.Message("Progress - Show: " + caption);
			//bool disabled = true; // debug
			//if (disabled) return;

			if (!SS.I.Attended)
			{
				ClientLog.Message("Show: " + caption);
				return;
			}

			if (!Lex.IsNullOrEmpty(ScriptLog.FileName))
				ScriptLog.Message("Progress.Show: " + caption);

			if (Lex.IsNullOrEmpty(caption)) // hide if no caption
			{
				Progress.Hide();
				return;
			}

			bool sameMessage = Visible &&
				Text == title && Caption.Text == caption &&
				Cancel.Enabled == allowCancel &&
				CancellingMessage.Text == cancellingMessage;

			//if (sameMessage) return;

			if (Debug) ClientLog.Message("Progress - Show 2");

			// Adjust size of form to fit number of lines

			Size s2 = new Size(OriginalFormSize.Width, OriginalFormSize.Height);
			LexLineMetrics llm = Lex.AnalyzeLines(caption);

			int lineCnt = (llm.LineCnt > OriginalLines) ? llm.LineCnt : OriginalLines; // lines to display?

			int newCaptionHeight = (int)(lineCnt * (OriginalCaptionSize.Height / (OriginalLines * 1.0) + .5));
			s2.Height = OriginalFormSize.Height + (newCaptionHeight - OriginalCaptionSize.Height);

			int lineLength = (llm.LongestLine > OriginalCharsPerLine) ? llm.LongestLine : OriginalCharsPerLine; // longer lines?
			int newCaptionWidth = (int)(llm.LongestLine * (OriginalCaptionSize.Width / (OriginalCharsPerLine * 1.0) + .5));
			s2.Width = OriginalFormSize.Width + (newCaptionWidth - OriginalCaptionSize.Width);

			if (Height < s2.Height) Height = s2.Height; // enlarge if necessary
			if (Width < s2.Width) Width = s2.Width;

			// Fill in the form

			Text = title;
			Caption.Text = caption;
			Cancel.Enabled = allowCancel;
			CancellingMessage.Text = cancellingMessage;
			Refresh(); // force update of dialog

			if (SessionManager.ActiveForm == GetInstance()) // if this progress form is the active form then be sure it's visible
			{
				try
				{
					BringToFront();
					Activate();
					Focus();
					//Application.DoEvents();
					Showing = true;
				}
				catch (Exception ex)
				{ ClientLog.Message("Show exception: " + DebugLog.FormatExceptionMessage(ex)); }
			}

			else if (!Showing)
			{
				try
				{
					//CenterToScreen();
					Form owner = OwnerFormToUse;
					if (owner == null) owner = SessionManager.ActiveForm; // if not defined get from session manager
					Show(owner); // show the form, note: non-modal so we can return immediately to caller
					Showing = true;
				}

				catch (InvalidOperationException opEx)
				{
					ClientLog.Message("Show exception 1: " + DebugLog.FormatExceptionMessage(opEx));

					// apparently something strange happened with focus...  Try to recover gracefully
					Form shellForm = null;
					foreach (Form form in Application.OpenForms)
					{
						Type type = form.GetType();
						if (type.FullName == "Mobius.Client.Shell")
						{
							shellForm = form;
							break;
						}
					}
					if (shellForm != null && !shellForm.Visible)
					{
						try
						{
							//Instance.CenterToScreen();
							GetInstance().Show(shellForm);
						}
						catch (Exception ex)
						{ ClientLog.Message("Instance.Show exception: " + DebugLog.FormatExceptionMessage(ex)); }
					}
				}

				catch (Exception ex) // some other error
				{
					string msg = "Show exception 2: ";
					if (SessionManager.ActiveForm != null) msg += "ActiveForm: " + SessionManager.ActiveForm.Text + " ";
					msg += DebugLog.FormatExceptionMessage(ex);
					ClientLog.Message(msg);
				}

				PerShowCancelRequestedFlag = false;
			}

			return;
		}

		/// <summary>
		/// Hide the progress dialog
		/// </summary>

		public static new void Hide()
		{
			if (Instance == null || !Instance.Visible) //  just return if nothing is showing (|| !Showing)
			{
				PendingShow = false; // cancel anything that is pending
				return;
			}

			PendingHide = true;
			return;
		}

		void HideInstance()
		{

			// Note: Using Hide() or Visible = false seems to fail in some cases (e.g. Show alerts) or
			// when control is switched to another app.

			lock (InstanceLock)
			{

				try
				{
					if (Debug) ClientLog.Message("Progress - Hide");
					if (!SS.I.Attended) return;

					if (Instance == null) return;
					if (!Showing) return;

					Showing = false;

					((Form)Instance).Hide(); // call base form method to hide the form
					Instance.Visible = false; // hide doesn't always work if user has switched to another app

					if (Debug) ClientLog.Message("Progress - Instance.Visible: " + Instance.Visible);
				}
				catch (Exception ex)
				{
					ClientLog.Message("Hide exception: " + DebugLog.FormatExceptionMessage(ex));
				}
				return;
			}
		}

		/// <summary>
		/// Return true if progress control is visible
		/// </summary>

		public static bool IsVisible
		{
			get
			{
				if (GetInstance() == null) return false;
				return Showing;
			}
		}

		/// <summary>
		/// Return true if a cancel of the operation has been requested by the user
		/// Reset for each Progress.Show call.
		/// </summary>

		public static bool CancelRequested // see if cancel button clicked
		{
			get
			{
				if (SS.I.Attended)
				{
					VerifyFormExistence();

					if (PerShowCancelRequestedFlag == true)
						return true;

					else return false;
				}

				else return false; // cancel not supported for non-graphics mode for now
			}
		}

		/// <summary>
		/// Return true if a cancel of the operation has been requested by the user
		/// NOT reset for each Progress.Show call.
		/// </summary>

		public static bool GlobalCancelRequested // see if cancel button clicked
		{
			get
			{
				if (SS.I.Attended)
				{
					VerifyFormExistence();
					if (GlobalCancelRequestedFlag == true)
						return true;

					else return false;
				}

				else return false; // cancel not supported for non-graphics mode for now
			}

			set { GlobalCancelRequestedFlag = value; }
		}

		private void Cancel_Click(object sender, System.EventArgs e)
		{
			bool cancelAlreadyRequested = PerShowCancelRequestedFlag;
			PerShowCancelRequestedFlag = true;
			GlobalCancelRequestedFlag = true;

			if (CancellingMessage.Text == "")
			{
				DialogResult = DialogResult.Cancel;
				Hide(); // be sure hidden
			}

			else
			{
				if (cancelAlreadyRequested)
				{ // already cancelling?
					DialogResult dr = XtraMessageBox.Show(
						"Mobius is already in the process of trying to cancel this operation.\n" +
						"You can either continue to wait for the cancellation to complete\n" +
						"or you can exit this Mobius session.\n" +
						"Do you want to continue to wait?", UmlautMobius.String,
						MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (dr == DialogResult.No)
						Environment.Exit(0); // bomb out
				}

				Caption.Text = CancellingMessage.Text; // set cancelling message 
				DialogResult = DialogResult.None; // don't close dialog now
			}
		}

		/// <summary>
		/// When timer ticks perform any pending operations
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ProgressTimer_Tick(object sender, System.EventArgs e)
		{

			//SystemUtil.Beep(); // debug

			lock (InstanceLock)
			{
				try
				{

					if (PendingShow) // show new progress information
					{
						//if (Lex.Contains(PendingCaption, "retrieving")) PendingCaption = PendingCaption; // debug

						if (Debug) ClientLog.Message("Progress - Show: " + PendingCaption);
						ShowInstance(PendingCaption, PendingTitle, PendingAllowCancel, PendingCancellingMessage);
						PendingCaption = PendingTitle = PendingCancellingMessage = "";
						PendingShow = PendingAllowCancel = false;
					}

					else if (PendingHide) // hide the progress form
					{
						if (Debug) ClientLog.Message("Progress - Hide");
						HideInstance();
						Size = OriginalFormSize; // restore size to original
						PendingHide = false;
						if (!PendingShow)
						{
							Owner = null;
							Parent = null;
							Location = new Point(-4096, -4096); // move off screen
							Discarded = true;
							Instance = null; // if no new show pending then don't use this instance again
							ProgressTimer.Enabled = false;

							//SessionManager.ActivateShell(); // really needed, this can hide a popup window that has just been shown

							if (Debug) ClientLog.Message("Progress - Discarding instance: " + Id + ", Caption: " + Caption.Text);
						}
						return;
					}

					if (!Visible) return;

					//if (ProgressBar.Value >= ProgressBar.Maximum)
					//  ProgressBar.Value = ProgressBar.Minimum;
					//else ProgressBar.Value++;

					if (Caption.Text.Contains(".:..")) // continue with timer?
						Caption.Text = Caption.Text.Replace(".:..", LastTimeText);

					if (!String.IsNullOrEmpty(LastTimeText) && Caption.Text.Contains(LastTimeText))
					{ // update time display
						TimeSpan ts = DateTime.Now.Subtract(StartTime);
						string timeText = String.Format("{0}:{1,2:00}", ts.Minutes, ts.Seconds);
						if (timeText != LastTimeText)
						{
							Caption.Text = Caption.Text.Replace(LastTimeText, timeText);
							LastTimeText = timeText;
							UIMisc.DoEvents();
						}
					}

					else if (Caption.Text.Contains("0:00"))
					{ // start new time display
						StartTime = DateTime.Now;
						LastTimeText = "0:00";
					}

					else LastTimeText = "";
				}
				catch (Exception ex)
				{ ClientLog.Message("ProgressTimer_Tick exception: " + DebugLog.FormatExceptionMessage(ex)); }
			}
		}

		private void Progress_Deactivate(object sender, System.EventArgs e)
		{
		}

		private void Progress_VisibleChanged(object sender, EventArgs e)
		{
			//try
			//{
			//  if (this.Visible)
			//  {
			//    CancelRequestedFlag = false; // clear cancel requested flag
			//    //				ProgressBar.Value = 0;
			//    ProgressTimer.Enabled = true;
			//  }

			//  else
			//    ProgressTimer.Enabled = false;
			//}
			//catch (Exception ex) { ex = ex; } // occasionally throws a can't create window exception
		}

		/// <summary>
		/// Return the currently displayed message if any
		/// </summary>
		/// <returns></returns>

		public static string GetCaption()
		{
			if (GetInstance() == null || !Showing) return "";
			return GetInstance().Caption.Text;
		}

		/// <summary>
		/// Get and hide the currently visible instance if any
		/// </summary>
		/// <returns></returns>

		//public static Progress GetAndHideCurrentVisibleInstance()
		//{
		//	if (Instance == null || !Showing) return null;

		//	Progress inst = Instance;
		//	Progress.Hide();
		//	Progress.Instance = null;
		//	return inst;
		//}

		private void Progress_Shown(object sender, EventArgs e)
		{
			ProgressTimer.Enabled = true;
		}

		private void Progress_FormClosing(object sender, FormClosingEventArgs e)
		{
			return;
		}

		private void Progress_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				ProgressTimer.Enabled = false;
			}
			catch (Exception ex) // occasionally throws a can't create window exception
			{ ClientLog.Message("Progress_FormClosed exception: " + DebugLog.FormatExceptionMessage(ex)); }

			Showing = false;
		}

		/// <summary>
		/// Return true if the update interval has passed since the last update
		/// </summary>

		public static bool IsTimeToUpdate
		{
			get
			{
				int t2 = TimeOfDay.Milliseconds();
				if (t2 - LastUpdateTime >= UpdateInterval) return true;
				else return false;
			}
		}
	}

	/// <summary>
	/// ProgressMessage details
	/// </summary>

	public class ProgressMessage
	{
		public string Caption;
		public string Title;
		public bool AllowCancel;
		public string CancellingMessage;
	}
}
