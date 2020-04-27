using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

/// <summary>
/// CalcFieldColumnControl
/// </summary>

	public partial class CalcFieldColumnControl : DevExpress.XtraEditors.XtraUserControl
	{
		[DefaultValue("Data Field")]
		public string FieldLabel { get { return fieldLabel.Text; } set { fieldLabel.Text = value; } }

		public event EventHandler ColumnChanged; // event to fire when column changes

/// <summary>
/// Constructor
/// </summary>

		public CalcFieldColumnControl()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Adjust for function change
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Function_SelectedValueChanged(object sender, EventArgs e)
		{
			if (Function.Text.ToLower().IndexOf("constant") >= 0)
			{
				Constant.Enabled = true;
				Constant.Focus();
			}
			else Constant.Enabled = false;

			//if (Function.Text.ToLower().IndexOf("lean number") >= 0)
			//  Operation.Text = "None (Use first data field only)";

			return;
		}

		private void Column_EditValueChanged(object sender, EventArgs e)
		{
			if (ColumnChanged != null) // fire ColumnChanged event if handlers present
				ColumnChanged(this, EventArgs.Empty);

		}
	}
}
