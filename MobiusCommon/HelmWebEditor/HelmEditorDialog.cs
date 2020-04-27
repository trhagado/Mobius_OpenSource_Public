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

namespace Mobius.Helm
{
	public partial class HelmEditorDialog : DevExpress.XtraEditors.XtraForm
	{
		public string OriginalHelm = null;
		public string EditedHelm = null;

		public HelmEditorDialog()
		{
			InitializeComponent();

			HelmControl.HelmMode = HelmControlMode.BrowserEditor;
			HelmControl.BorderStyle = BorderStyle.None;

			return;
		}


		/// <summary>
		/// Edit the HELM
		/// </summary>
		/// <param name="originalHelm"></param>
		/// <returns></returns>

		public string Edit(
			string originalHelm,
			IWin32Window ownerForm = null)
		{
			OriginalHelm = originalHelm;

			DialogResult dr = ShowDialog(ownerForm);

			if (dr != DialogResult.Cancel)
				return EditedHelm;

			else return null; // cancelled
		}

		/// <summary>
		/// Return HELM if valid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void SaveButton_Click(object sender, EventArgs e)
		{
			EditedHelm = HelmControl.GetHelm();
			// todo: validate HELM
			DialogResult = DialogResult.OK;
		}

		private void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void HelmEditorDialog_Shown(object sender, EventArgs e)
		{
			HelmControl.SetHelmAndRender(OriginalHelm); // display original Helm once dialog is shown

			return;
		}

		private void HelmControl_Scroll(object sender, ScrollEventArgs e)
		{
			return;
		}
	}
}