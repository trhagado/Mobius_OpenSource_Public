using Mobius.ComOps;

using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using DevExpress.XtraEditors.Controls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class MessageBoxMx2 : XtraForm
	{
		public delegate DialogResult ShowDelegate(string message);

		public static bool UseDevExpressMessageBox = true;

		public MessageBoxMx2()
		{
			try
			{
				InitializeComponent();
			}
			catch (Exception ex) // seems to sometimes occur when exiting WIN 10
			{
				string msg = ex.Message;
			}
		}

		private void MessageBoxEx_Activated(object sender, EventArgs e)
		{
			YesButton.Focus(); // put initial focus on Yes button
		}

		private void YesButton_Click(object sender, EventArgs e)
		{
			DialogResult = (DialogResult)1;
		}

		private void NoButton_Click(object sender, EventArgs e)
		{
			DialogResult = (DialogResult)2;
		}

		private void NoToAllButton_Click(object sender, EventArgs e)
		{
			DialogResult = (DialogResult)3;
		}

		private void Cancel_Click(object sender, EventArgs e)
		{
			DialogResult = (DialogResult)4;
		}

		private void MessageBoxMx2_SizeChanged(object sender, EventArgs e)
		{
			return;
		}

		private void OK_Click(object sender, EventArgs e)
		{

		}
	}

}
