using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace Mobius.ClientComponents
{
	public class IClientClass : IClient
	{

/// <summary>
/// Return true if running interactively with real user
/// </summary>

		public bool Attended
		{
			get { return SS.I.Attended; }
		}

		/// <summary>
		/// Show MessageBox
		/// </summary>
		/// <param name="text"></param>
		/// <param name="caption"></param>
		/// <param name="buttons"></param>
		/// <param name="icon"></param>
		/// <returns></returns>

		public DialogResult MessageBoxEx_Show(
			string text,
			string caption,
			MessageBoxButtons buttons,
			MessageBoxIcon icon)
		{
			return MessageBoxMx.Show(text, caption, buttons, icon);
		}

		/// <summary>
		/// Show error message in MessageBox 
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>

		public DialogResult MessageBoxEx_ShowError(
			string text)
		{
			return MessageBoxMx.ShowError(text);
		}

		/// <summary>
		/// Show progress dialog
		/// </summary>
		/// <param name="msg"></param>

		public void Progress_Show(
			string caption,
			string title,
			bool allowCancel)
		{
			Progress.InvokeShow(caption, title, allowCancel);
		}

		/// <summary>
		/// See if cancel requested
		/// </summary>

		public bool Progress_CancelRequested
		{
			get
			{
				return Progress.CancelRequested;
			}
		}

		/// <summary>
		/// Hide progrss dialog
		/// </summary>

		public void Progress_Hide()
		{
			Progress.InvokeHide();
		}

	}
}
