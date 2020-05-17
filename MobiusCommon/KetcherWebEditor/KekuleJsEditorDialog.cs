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

namespace Mobius.KekuleJs
{
	public partial class KekuleJsEditorDialog : DevExpress.XtraEditors.XtraForm
	{
		public string OriginalKekuleJs = null;
		public string EditedKekuleJs = null;

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
		/// <param name="originalKekuleJs"></param>
		/// <returns></returns>

		public string Edit(
			string originalKekuleJs,
			IWin32Window ownerForm = null)
		{
			OriginalKekuleJs = originalKekuleJs;

			DialogResult dr = ShowDialog(ownerForm);

			if (dr != DialogResult.Cancel)
				return EditedKekuleJs;

			else return null; // cancelled
		}

		/// <summary>
		/// Return KekuleJs if valid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void SaveButton_Click(object sender, EventArgs e)
		{
			EditedKekuleJs = KekuleJsControl.GetKekuleJs();
			// todo: validate KekuleJs
			DialogResult = DialogResult.OK;
		}

		private void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void KekuleJsEditorDialog_Shown(object sender, EventArgs e)
		{
			KekuleJsControl.SetKekuleJsAndRender(OriginalKekuleJs); // display original KekuleJs once dialog is shown

			return;
		}

		private void KekuleJsControl_Scroll(object sender, ScrollEventArgs e)
		{
			return;
		}
	}
}