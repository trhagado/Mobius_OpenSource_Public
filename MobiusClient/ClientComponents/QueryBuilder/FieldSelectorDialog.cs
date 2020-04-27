using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

/// <summary>
/// Show dialog that allows user to select a field
/// </summary>

	public partial class FieldSelectorDialog : DevExpress.XtraEditors.XtraForm
	{
		public FieldSelectorDialog()
		{
			InitializeComponent();
		}

		public static MetaColumn Show(
			string prompt,
			string title,
			Query q,
			MetaColumn mc)
		{
			FieldSelectorDialog fsd = new FieldSelectorDialog();

			fsd.Text = title;
			fsd.Prompt.Text = prompt;

			fsd.FSC.Query = q;
			fsd.FSC.MetaColumn = mc;

			DialogResult dr = fsd.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.OK) return fsd.FSC.MetaColumn;
			else return null;
		}
	}
}