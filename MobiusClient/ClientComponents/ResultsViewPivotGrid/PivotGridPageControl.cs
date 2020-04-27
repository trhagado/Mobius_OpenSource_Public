using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraPivotGrid;
using DevExpress.XtraEditors;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraPrinting;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class PivotGridPageControl : DevExpress.XtraEditors.XtraUserControl
	{
		string TextFileName = "";
		string XlsFileName = "";

		internal PivotGridView View; // the view associated with the page control

		public PivotGridPageControl()
		{
			InitializeComponent();

			Controls.Remove(ToolPanel); // hide the controls
			PivotGridPanel.Dock = DockStyle.Fill;
			PivotGridPanel.PageControl = this; 
		}

		private void PivotGridPropertiesButton_Click(object sender, EventArgs e)
		{
			DialogResult dr = View.ShowInitialViewPropertiesDialog();
			return;
		}

		private void PrintBut_Click(object sender, EventArgs e)
		{
			if (View == null || View.Qm == null) return;
			PrintPreviewDialog.Show(View.Qm, PivotGridPanel.PivotGrid);
		}

		private void EditQueryBut_Click(object sender, EventArgs e)
		{
			if (ViewManager.TryCallCustomExitingQueryResultsCallback(PivotGridPanel, ExitingQueryResultsType.EditQuery)) return;

			CommandExec.ExecuteCommandAsynch("EditQuery");
		}

		private void ExportBut_Click(object sender, EventArgs e)
		{
			SimpleButton b = sender as SimpleButton;
			ExportContextMenu.Show(b,
				new System.Drawing.Point(0, b.Height));
		}

/// <summary>
/// Export to Excel
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void ExportExcelMenuItem_Click(object sender, EventArgs e)
		{
			XlsxExportOptions xeo;

			if (Lex.IsNullOrEmpty(TextFileName))
				XlsFileName = ClientDirs.DefaultMobiusUserDocumentsFolder;

			string filter =
				"Excel Workbook (*.xlsx)|*.xlsx|" +
				"Excel 97-2003 Workbook (*.xls)|*.xls";
			string fileName = UIMisc.GetSaveAsFilename("Excel File Name", XlsFileName, filter, ".xlsx");

			if (Lex.IsNullOrEmpty(fileName)) return;
			XlsFileName = fileName;

			if (Lex.EndsWith(fileName, ".xls"))
				PivotGridPanel.PivotGrid.ExportToXls(XlsFileName);

			else PivotGridPanel.PivotGrid.ExportToXlsx(XlsFileName);

			string msg =
				"Export to Excel complete.\n" +
				"Do you want to open " + fileName;
			DialogResult dr = MessageBoxMx.Show(msg, UmlautMobius.String,
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (dr == DialogResult.Yes)
				SystemUtil.StartProcess(fileName);

			return;
		}

/// <summary>
/// Export to csv or text file
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void ExportTextFileMenuItem_Click(object sender, EventArgs e)
		{
			CsvExportOptions ceo; // Can specify separator
			TextExportMode tem; // Value or Text
			PdfExportOptions peo;

			if (Lex.IsNullOrEmpty(TextFileName))
				TextFileName = ClientDirs.DefaultMobiusUserDocumentsFolder;

			string filter =
				"CSV (Comma delimited)(*.csv)|*.csv|" +
				"Text (Tab delimited)(*.txt)|*.txt";
			string fileName = UIMisc.GetSaveAsFilename("CSV / Text File Name", TextFileName, filter, ".csv");
			if (Lex.IsNullOrEmpty(fileName)) return;
			TextFileName = fileName;

			if (Lex.EndsWith(fileName, ".csv"))
				PivotGridPanel.PivotGrid.ExportToCsv(TextFileName);

			else PivotGridPanel.PivotGrid.ExportToText(TextFileName);

			return;
		}

		private void ShowFieldListButton_Click(object sender, EventArgs e)
		{
			DialogResult dr = View.ShowInitialViewPropertiesDialog();
			return;
		}
	}
}
