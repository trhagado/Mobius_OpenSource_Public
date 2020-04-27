using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.Data;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.Tools
{
	public partial class BulkAssayDataExporter : XtraForm, IActionDelegate
	{
		BulkAssayDataExporter Instance;
		string ReturnMessage = "";

		public BulkAssayDataExporter()
		{
			InitializeComponent();
			Instance = this;
		}

		/// <summary>
/// Main entrypoint for plugin
/// </summary>
/// <param name="args"></param>
/// <returns></returns>

		public string Run(string args) 
		{
			return Instance.ShowDialog(null);
		}

		/// <summary>
		/// Show dialog & process results
		/// </summary>
		/// <returns></returns>
		/// 
		string ShowDialog(
			string args)
		{
			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return "";

			return ReturnMessage;
		}

		private void Assays_Click(object sender, EventArgs e)
		{

		}

		private void SelectAssays_Click(object sender, EventArgs e)
		{

		}

		private void ExportFileFormat_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void ExportFileName_Click(object sender, EventArgs e)
		{

		}

		private void EditExportSetup_Click(object sender, EventArgs e)
		{

		}

		private void OK_Click(object sender, EventArgs e)
		{

		}

	}
}