using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mobius.KekuleJs
{
	public partial class KekuleJsEditorDialog : DevExpress.XtraEditors.XtraForm
	{
		public string OriginalMolfile = null;
		public string EditedMolfile = null;

		public KekuleJsEditorDialog()
		{
			InitializeComponent();

			KekuleJsControl.KekuleJsMode = KekuleJsControlMode.BrowserEditor;
			KekuleJsControl.BorderStyle = BorderStyle.None;

			return;
		}

		/// <summary>
		/// Edit the KekuleJs
		/// </summary>
		/// <param name="originalMolfile"></param>
		/// <returns></returns>

		public string Edit(
			string originalMolfile,
			IWin32Window ownerForm = null)
		{
			OriginalMolfile = originalMolfile;

			DialogResult dr = ShowDialog(ownerForm);

			if (dr != DialogResult.Cancel)
				return EditedMolfile;

			else return null; // cancelled
		}

		/// <summary>
		/// Return KekuleJs if valid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void SaveButton_Click(object sender, EventArgs e)
		{
			EditedMolfile = KekuleJsControl.GetKekuleJs();
			// todo: validate KekuleJs
			DialogResult = DialogResult.OK;
		}

		private void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void KekuleJsEditorDialog_Shown(object sender, EventArgs e)
		{
			KekuleJsControl.SetMoleculeAndRender(MoleculeFormat.Molfile, OriginalMolfile); // display original KekuleJs once dialog is shown

			return;
		}

		private void KekuleJsControl_Scroll(object sender, ScrollEventArgs e)
		{
			return;
		}
	}
}