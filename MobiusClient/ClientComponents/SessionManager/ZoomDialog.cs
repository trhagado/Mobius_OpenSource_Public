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
	public partial class ZoomDialog : DevExpress.XtraEditors.XtraForm
	{
		static ZoomDialog Instance;

		public QueryManager Qm; // QueryManager, set on Show call
		public QueryResultsControl Qrc { get { return Qm != null ? Qm.QueryResultsControl : null; } } // the current queryresults control
		int Pct; // persentage of zoom

		public ZoomDialog()
		{
			InitializeComponent();
		}

/// <summary>
/// Show the dialog
/// </summary>
		
		public static DialogResult Show(
			ref int pct, 
			Point location, 
			QueryManager qm)
		{
			if (Instance == null) Instance = new ZoomDialog();
			ZoomDialog i = Instance;

			i.Qm = qm;

			if (pct == 800) i.Zoom800.Checked = true;
			else if (pct == 400) i.Zoom400.Checked = true;
			else if (pct == 200) i.Zoom200.Checked = true;
			else if (pct == 100) i.Zoom100.Checked = true;
			else if (pct == 75) i.Zoom75.Checked = true;
			else if (pct == 50) i.Zoom50.Checked = true;
			else if (pct == 25) i.Zoom25.Checked = true;
			else i.ZoomCustom.Checked = true;
			
			i.ZoomCustomSpinEdit.Text = pct.ToString();

			i.ZoomFitToWidth.Enabled = (i.Qrc != null);

			if (!location.IsEmpty)
			{
				i.StartPosition = FormStartPosition.Manual;
				i.Location = location;
			}
			else i.StartPosition = FormStartPosition.CenterScreen;

			DialogResult dr = i.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.OK) pct = i.Pct; // return new percentage
			return dr;
		}

		private void OK_Click(object sender, EventArgs e)
		{
			if (Zoom800.Checked) Pct = 800;
			else if (Zoom400.Checked) Pct = 400;
			else if (Zoom200.Checked) Pct = 200;
			else if (Zoom100.Checked) Pct = 100;
			else if (Zoom75.Checked) Pct = 75;
			else if (Zoom50.Checked) Pct = 50;
			else if (Zoom25.Checked) Pct = 25;
			else if (ZoomFitToWidth.Checked || // fit to page width
			 ZoomCustom.Checked) 
				Pct = int.Parse(ZoomCustomSpinEdit.Text);

			if (Pct <= 0) Pct = 1; // be sure not too small

			DialogResult = DialogResult.OK;
			return;
		}

		private void Zoom800_Click(object sender, EventArgs e)
		{
			ZoomCustomSpinEdit.EditValue = "800";
		}

		private void Zoom400_Click(object sender, EventArgs e)
		{
			ZoomCustomSpinEdit.EditValue = "400";
		}

		private void Zoom200_Click(object sender, EventArgs e)
		{
			ZoomCustomSpinEdit.EditValue = "200";
		}

		private void Zoom100_Click(object sender, EventArgs e)
		{
			ZoomCustomSpinEdit.EditValue = "100";
		}

		private void Zoom75_Click(object sender, EventArgs e)
		{
			ZoomCustomSpinEdit.EditValue = "75";
		}

		private void Zoom50_Click(object sender, EventArgs e)
		{
			ZoomCustomSpinEdit.EditValue = "50";
		}

		private void Zoom25_Click(object sender, EventArgs e)
		{
			ZoomCustomSpinEdit.EditValue = "25";
		}

		private void ZoomFitToWidth_Click(object sender, EventArgs e)
		{
			if (Qrc == null) return;

			int pct = Qrc.CurrentView.GetFitPageWidthScale();
			ZoomCustomSpinEdit.EditValue = pct.ToString();
		}

		private void ZoomCustom_Click(object sender, EventArgs e)
		{
			ZoomCustom.Checked = true;
			ZoomCustomSpinEdit.Focus();
		}

		private void ZoomCustomSpinEdit_MouseDown(object sender, MouseEventArgs e)
		{
			ZoomCustom.Checked = true;
		}

		private void ZoomCustomSpinEdit_Click(object sender, EventArgs e)
		{
			ZoomCustom.Checked = true;
		}


	}
}