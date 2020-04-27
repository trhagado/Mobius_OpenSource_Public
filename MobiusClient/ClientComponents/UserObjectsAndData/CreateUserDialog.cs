using Mobius.Data;
using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace Mobius.ClientComponents
{
	public partial class CreateUserDialog : DevExpress.XtraEditors.XtraForm
	{
		public CreateUserDialog()
		{
			InitializeComponent();
		}

		public static DialogResult ShowDialog(
			UserInfo ui,
			string caption)
		{
			CreateUserDialog i = new CreateUserDialog();
			i.Text = caption;
			i.UserName.Text = ui.UserName;
			i.Domain.Text = ui.UserDomainName;
			i.FirstName.Text = ui.FirstName;
			i.MiddleInitial.Text = ui.MiddleInitial;
			i.LastName.Text = ui.LastName;
			i.EmailAddress.Text = ui.EmailAddress;

			DialogResult dr = i.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.OK)
			{
				ui.UserName = i.UserName.Text.Trim();
				ui.UserDomainName = i.Domain.Text.Trim();
				ui.FirstName = i.FirstName.Text.Trim();
				ui.MiddleInitial = i.MiddleInitial.Text.Trim();
				ui.LastName = i.LastName.Text.Trim();
				ui.EmailAddress = i.EmailAddress.Text.Trim();
			}

			return dr;
		}

		private void CreateUser_Activated(object sender, EventArgs e)
		{
			UserName.Focus();
		}

		private void OKButton_Click(object sender, EventArgs e)
		{
			if (IsBlank(UserName) || IsBlank(FirstName) || IsBlank(LastName))
				return;
			DialogResult = DialogResult.OK;
		}

		bool IsBlank(TextEdit ctl)
		{
			if (!Lex.IsNullOrEmpty(ctl.Text)) return false;

			ctl.Focus();
			MessageBoxMx.ShowError("Missing value");
			return true;
		}
	}
}