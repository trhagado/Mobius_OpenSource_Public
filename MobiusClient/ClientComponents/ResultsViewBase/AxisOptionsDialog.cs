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
	public partial class AxisOptionsDialog : DevExpress.XtraEditors.XtraForm
	{
		static AxisOptionsDialog Instance;

		public AxisOptionsDialog()
		{
			InitializeComponent();
		}

		//public static DialogResult ShowDialog(AxisMx ax)
		//{
		//	if (Instance == null) Instance = new AxisOptionsDialog();

		//	if (ax == null || ax.QueryColumn == null) return DialogResult.Cancel;

		//	Instance.Text = ax.QueryColumn.ActiveLabel + " Axis Properties";

		//	AxisMx ax2 = ax.Clone(); // make copy we can modify

		//	Instance.AxisOptionsControl.Setup(ax2);

		//	DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
		//	if (dr == DialogResult.OK) ObjectEx.MemberwiseCopy(ax2, ax); // copy back changed values

		//	return dr;
		//}

	}
}