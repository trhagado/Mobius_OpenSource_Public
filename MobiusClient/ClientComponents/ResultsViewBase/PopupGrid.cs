using Mobius.ComOps;
using Mobius.Data;

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
	public partial class PopupGrid : DevExpress.XtraEditors.XtraForm
	{
		QueryManager Qm;

		static PopupGrid LastGridCreated;

		public PopupGrid()
		{
			InitializeComponent();
			return;
		}

		public PopupGrid(QueryManager qm)
		{
			InitializeComponent();

			Initialize(qm);

			return;
		}

		public void Initialize(QueryManager qm)
		{
			Qm = qm;
			StatusBarManager sbm = new StatusBarManager();
			qm.LinkMember(sbm);

			sbm.SetupViewZoomControls(null, ZoomPctBarItem, ZoomSlider);
			sbm.ZoomSliderPct = 100; // set to 100% initially

			if (LastGridCreated != null && LastGridCreated.WindowState == FormWindowState.Normal)
				try { Size = LastGridCreated.Size; }
				catch (Exception ex) { ex = ex; }

			LastGridCreated = this;
		}

		private void PrintBut_Click(object sender, EventArgs e)
		{
			PrintPreviewDialog.Show(Qm, Qm.MoleculeGrid);
		}

	}
}