using Mobius.ComOps;

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace Mobius.UAL 
{

/// <summary>
/// ClientUI
/// </summary>

	public class ClientUI
	{

/// <summary>
/// Return true if running interactively with real user
/// </summary>

		public static bool Attended
		{
			get {
				if (UalUtil.IClient != null) return UalUtil.IClient.Attended;
				else return false;
			}
		}
	}

/// <summary>
/// Proxy for Progress class for showing task progress dialog
/// </summary>

	public class Progress
	{
		public static int LastUpdateTime = 0; // time progress last updated
		public static int UpdateInterval = 1000; // update interval in milliseconds

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
			Show(caption, UmlautMobius.String, true);
		}

		public static void Show(
			string caption,
			string title,
			bool allowCancel)
		{
			LastUpdateTime = TimeOfDay.Milliseconds();

			if (UalUtil.IClient != null)
				UalUtil.IClient.Progress_Show(caption, title, allowCancel);
		}

		/// <summary>
		/// Return true if a cancel of the operation has been requested by the user
		/// </summary>

		public static bool CancelRequested // see if cancel button clicked
		{
			get
			{
				if (UalUtil.IClient != null)
					return UalUtil.IClient.Progress_CancelRequested;
				else return false;
			}
		}

		/// <summary>
		/// Hide the progress dialog
		/// </summary>

		public static void Hide()
		{
			if (UalUtil.IClient != null)
				UalUtil.IClient.Progress_Hide();
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
/// Send message back to client
/// 
/// </summary>

	public class MessageBoxEx
	{
		public static DialogResult ShowError(
			string text)
		{
			if (UalUtil.IClient != null)
				return UalUtil.IClient.MessageBoxEx_ShowError(text);

			else return DialogResult.OK;
		}
	}

}
