using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace Mobius.ComOps
{
	/// <summary>
	/// Interface to client operations that can be called from service-side code
	/// </summary>

	public interface IClient
	{

		/// <summary>
		/// Return true if running interactively with real user
		/// </summary>

		bool Attended
		{
			get;
		}

		/// <summary>
		/// Show MessageBox
		/// </summary>
		/// <param name="text"></param>
		/// <param name="caption"></param>
		/// <param name="buttons"></param>
		/// <param name="icon"></param>
		/// <returns></returns>

		DialogResult MessageBoxEx_Show(
			string text,
			string caption,
			MessageBoxButtons buttons,
			MessageBoxIcon icon);

		/// <summary>
		/// Show error message in MessageBox 
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>

		DialogResult MessageBoxEx_ShowError(
			string text);

		/// <summary>
		/// Show progress dialog
		/// </summary>
		/// <param name="msg"></param>

		void Progress_Show(
			string caption,
			string title,
			bool allowCancel);

		/// <summary>
		/// See if cancel requested
		/// </summary>

		bool Progress_CancelRequested
		{
			get;
		}

		/// <summary>
		/// Hide progrss dialog
		/// </summary>

		void Progress_Hide();

	}
}
