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
	public partial class TableList : DevExpress.XtraEditors.XtraForm
	{
		public System.EventHandler SelectedMethod; // callback method for selection

		public TableList()
		{
			InitializeComponent();
		}

		private void TableList_Deactivate(object sender, EventArgs e)
		{
			Close(); // close if something else is clicked
		}

		private void List_SelectedValueChanged(object sender, EventArgs e)
		{
			if (!this.Visible) return;
			this.Close();
			if (SelectedMethod != null) SelectedMethod(sender,e);
		}
	}
}