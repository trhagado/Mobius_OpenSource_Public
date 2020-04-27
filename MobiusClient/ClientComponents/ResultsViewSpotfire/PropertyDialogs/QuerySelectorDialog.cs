using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace Mobius.SpotfireClient
{
	public partial class QuerySelectorDialog : DevExpress.XtraEditors.XtraForm
	{
		int ActivationCount = 0;

		public QuerySelectorDialog()
		{
			InitializeComponent();
		}

		public DialogResult ShowDialog(
			int selectedQueryId,
			string selectedQueryName)
		{
			QuerySelectorControl.Setup(selectedQueryId, selectedQueryName);
			DialogResult dr = ShowDialog(SessionManager.ActiveForm);
			return dr;
		}

		private void QuerySelectorDialog_Activated(object sender, EventArgs e)
		{
			ActivationCount++;

			if (ActivationCount == 1) // show menu on first activation only
				DelayedCallback.Schedule( // schedule callback
					delegate (object state)
					{ QuerySelectorControl.ShowSelectQueryMenu(); });
		}

	}
}