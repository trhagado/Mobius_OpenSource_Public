using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using DevExpress.LookAndFeel;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraBars.Ribbon.ViewInfo;

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

	/// <summary>
	/// User preferences form
	/// </summary>

	public partial class SystemAvailabilityMsg : DevExpress.XtraEditors.XtraForm
	{

		public SystemAvailabilityMsg()
		{
			InitializeComponent();
			Text = UmlautMobius.String + " Error";
		}

		/// <summary>
		/// Show Mobius system availability message
		/// </summary>
		/// <param name="messageText"></param>
		/// <param name="messageTitle"></param>
		/// <param name="icon"></param>

		public static void Show(
				string messageText,
				string messageTitle,
				MessageBoxIcon icon,
				Form parentForm)
		{
			if (!Lex.IsNullOrEmpty(ScriptLog.FileName))
				ScriptLog.Message("> " + messageTitle + " - " + messageText);

			if (!SS.I.Attended)
			{
				ClientLog.Message("Show: " + messageTitle + " - " + messageText);
				return;
			}

			bool error = (icon == MessageBoxIcon.Error);

			SystemAvailabilityMsg sam = new SystemAvailabilityMsg();

			sam.Text = messageTitle;
			sam.SysAvailMsg.Text = messageText;
			MessageBoxMx.SetIconImageIndex(sam.IconImage, icon);

			if (!error && SS.I != null && SS.I.UserIniFile != null)
			{
				string dontShowMsg = SS.I.UserIniFile.Read("SystemAvailabilityMsgDontShowMsg");
				if (Lex.Eq(messageText, dontShowMsg)) return; // if msg not to show just return
				sam.DontShowAgainButton.Visible = true;
			}

			else sam.DontShowAgainButton.Visible = false; // hide for error

			if (parentForm!= null && parentForm.Visible) // put in front of supplied form if any
				sam.ShowDialog(parentForm);

			else sam.ShowDialog();

			return;
		}

		private void DontShowAgainButton_Click(object sender, EventArgs e)
		{
			SS.I.UserIniFile.Write("SystemAvailabilityMsgDontShowMsg", SysAvailMsg.Text);

			DialogResult = DialogResult.OK;
		}

		private void OK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			return;
		}

	}
}