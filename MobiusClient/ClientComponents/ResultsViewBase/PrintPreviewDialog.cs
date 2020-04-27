using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Native;
using DevExpress.XtraPrinting.Preview;
using DevExpress.XtraPrinting.Control;
using DevExpress.XtraPrintingLinks;
using DevExpress.LookAndFeel;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Card;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Threading;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class PrintPreviewDialog : DevExpress.XtraBars.Ribbon.RibbonForm
	{
		static PrintPreviewDialog Instance;

		QueryManager Qm;
		IPrintable Control; // the control we're currently printing
		PrintingSystem Ps;
		PrintableComponentLink Pcl;
		int PreviewRowCount = 0;

		public PrintPreviewDialog()
		{
			InitializeComponent();
		}

/// <summary>
/// Setup & show the dialog
/// </summary>
/// <param name="queryManager"></param>
/// <param name="control"></param>
/// <returns></returns>
 
		public new static DialogResult Show(
			QueryManager queryManager,
			IPrintable control)
		{

// When the GridView is active we can mostly the standard DevExpress printing stuff.
// We just remove the Checkmark boxes and make sure all data is read in before starting the print.
// However, when LayoutView is active (Cids & structures only) we must work around a couple of issues.
// 1. Use CardView instead of LayoutView because LayoutView cards get split between pages.
// 2. Scaling does not put the proper number of cards per row. Fix by scaling the cards rather 
//    than the print Document.
//
// Print scaling is persisted in Query.PrintScale

			if (Instance == null) Instance = new PrintPreviewDialog();
			Instance.Qm = queryManager;
			Query query = queryManager.Query;
			Instance.Control = control;

			if (control is MoleculeGridControl)
			{
				int printScalePct = query.PrintScale; // scale the document
				double scale = printScalePct / 100.0;
				queryManager.ResultsFormat.PageScale = printScalePct; // do view scaling based on printScale

				MoleculeGridControl grid = control as MoleculeGridControl;
				ResultsFormat rf = queryManager.ResultsFormat;
				queryManager.DataTableManager.ResetFormattedBitmaps(); // for structures to be redrawn in correct size

				if (rf.UseBandedGridView) // hide check mark band for printing
				{
					BandedGridView bgv = grid.MainView as BandedGridView;
					if (Lex.Eq(bgv.Bands[0].Name, "CheckMark")) // hide checkmark band
						bgv.Bands[0].Visible = false;

					if (printScalePct > 0) grid.ScaleBandedGridView(scale); // scale the BandedGridView
				}

				else // switch to special card view for printing
				{
					CardView cv = grid.GetCardView();
					grid.MainView = cv;
					string keyLabel = queryManager.Query.Tables[0].MetaTable.KeyMetaColumn.Label;
					cv.CardCaptionFormat = keyLabel + ": {2}"; // set proper caption

					if (printScalePct > 0) grid.ScaleCardView(scale); // scale the CardView
				}

				DialogResult dr = ShowDialog(grid);

				// Reset the grid view

				queryManager.DataTableManager.ResetFormattedBitmaps(); // clear struct bitmaps so they get redrawn in correct size
				queryManager.ResultsFormat.PageScale = query.ViewScale; // switch back to doing view scaling based on viewScale

				if (rf.UseBandedGridView) // unhide check mark band
				{
					BandedGridView bgv = grid.MainView as BandedGridView;
					bgv.Bands[0].Visible = true;
					grid.ScaleBandedGridView(query.ViewScale / 100.0); // scale back for viewing
				}

				else // switch back to layout view
				{
					grid.MainView = grid.GetLayoutView();
				}

				grid.Refresh();

				return dr;
			}

			else if (control is PivotGridControlMx)
			{
				PivotGridControlMx grid = control as PivotGridControlMx;
				DialogResult dr = ShowDialog(grid);
				return dr;
			}

			//else if (control is ChartControlMx)
			//{
			//	ChartControlMx chart = control as ChartControlMx;
			//	DialogResult dr = ShowDialog(chart);
			//	return dr;
			//}

			else throw new Exception("Invalid control type: " + control.GetType());
		}

/// <summary>
/// Build & show the Print Preview dialog
/// </summary>
/// <param name="printableDxControl"></param>
/// <returns></returns>

		static DialogResult ShowDialog(
			IPrintable printableDxControl)
		{
			string leftColumn, middleColumn, rightColumn;
			int m;

			Query query = Instance.Qm.Query;
			int printScale = query.PrintScale; // scale the document

			if (printScale < 0 && !(Instance.Qm.MoleculeGrid.MainView is DevExpress.XtraGrid.Views.BandedGrid.BandedGridView))
			{ // if not banded view be sure not fit to width
				printScale = query.PrintScale = 100; 
			}

			Instance.Qm.ResultsFormat.PageScale = printScale; // do view scaling based on printScale
			ResultsFormat rf = Instance.Qm.ResultsFormat;

			PrintingSystem ps = new PrintingSystem();
			Instance.Ps = ps;
			Instance.PrintControl.PrintingSystem = ps;

			PrintableComponentLink pcl = new PrintableComponentLink(ps);
			Instance.Pcl = pcl;
			pcl.Component = printableDxControl;
			ps.Links.Add(pcl);
			pcl.CreateDetailArea += new CreateAreaEventHandler(Instance.PrintableComponentLink_CreateDetailArea);
			pcl.CreateDetailHeaderArea += new CreateAreaEventHandler(Instance.PrintableComponentLink_CreateDetailHeaderArea);
			pcl.CreateInnerPageHeaderArea += new CreateAreaEventHandler(Instance.PrintableComponentLink_InnerPageHeaderArea);
			pcl.CreateMarginalHeaderArea += new CreateAreaEventHandler(Instance.PrintableComponentLink_CreateMarginalHeaderArea);
			pcl.CreateReportHeaderArea += new CreateAreaEventHandler(Instance.PrintableComponentLink_CreateReportHeaderArea);

			if (query.PrintMargins > 0)
			{
				m = query.PrintMargins / 10;
				pcl.Margins = new Margins(m, m, m, m);
			}
			if (query.PrintOrientation == Orientation.Vertical)
				pcl.Landscape = false;
			else pcl.Landscape = true;

			PageHeaderFooter phf = pcl.PageHeaderFooter as PageHeaderFooter;

			phf.Header.Content.Clear();
			middleColumn = rf.Query.UserObject.Name; // use query name for heading
			phf.Header.Content.AddRange(new string[] { "", middleColumn, "" });
			phf.Header.LineAlignment = BrickAlignment.Center;
			phf.Header.Font = new Font("Arial", 10, FontStyle.Bold);

			leftColumn = "Mobius: " + DateTimeMx.Format(DateTimeMx.GetCurrentDate());
			middleColumn = "Confidential";
			rightColumn = "Page [Page #]";
			phf.Footer.Content.Clear();
			phf.Footer.Content.AddRange(new string[] { leftColumn, middleColumn, rightColumn });
			phf.Footer.LineAlignment = BrickAlignment.Center;

			// Todo: If DataTable is big just use a subset for preview? Takes about 5 secs to format 100 structs.

			Instance.PreviewRowCount = Instance.Qm.DataTable.Rows.Count;

			Progress.Show("Formatting preview...", "Mobius", true);
			pcl.CreateDocument();
			Progress.Hide();

			if (printScale > 0) // scale to percentage
			{
				ps.Document.ScaleFactor = 1.0f; // keep document scale at 1.0
				ps.Document.AutoFitToPagesWidth = 0;
			}

			else
			{
				Instance.Qm.MoleculeGrid.ScaleBandedGridView(100.0); // scale grid to full size before setting doc scale
				ps.Document.AutoFitToPagesWidth = (int)-printScale; // fit to 1 or more pages
				int pctScale = (int)(ps.Document.ScaleFactor * 100);
				Instance.Qm.ResultsFormat.PageScale = pctScale;
				Instance.Qm.MoleculeGrid.ScaleBandedGridView(pctScale / 100.0); // scale grid down to get structures correct size
				pcl.CreateDocument(); // recreate the doc
			}

			ps.StartPrint += new PrintDocumentEventHandler(Instance.PrintingSystem_StartPrint);
			ps.PrintProgress += new PrintProgressEventHandler(Instance.PrintingSystem_PrintProgress);
			ps.EndPrint += new EventHandler(Instance.PrintingSystem_EndPrint);
			ps.ShowPrintStatusDialog = true;

			Form shell = SessionManager.Instance.ShellForm;
			Instance.Location = shell.Location;
			Instance.Size = shell.Size;
			Instance.WindowState = shell.WindowState;
			RibbonControl src = SessionManager.Instance.RibbonCtl; // model this ribbon on shell ribbon
			if (src != null)
			{
				Instance.RibbonControl.RibbonStyle = src.RibbonStyle;
				Instance.RibbonControl.ApplicationIcon = src.ApplicationIcon;
			}

			DialogResult dr = Instance.ShowDialog(shell); // show the preview

			query.PrintMargins = pcl.Margins.Left * 10; // keep margins in milliinches

			if (pcl.Landscape) query.PrintOrientation = Orientation.Horizontal;
			else query.PrintOrientation = Orientation.Vertical;

			Progress.Hide(); // just in case
			return dr;
		}

/// <summary>
/// Create document with status update & cancel allowed (gets cross-thread exception)
/// </summary>
		void CreateDocument()
		{
			try
			{
				Progress.Show("Formatting preview...", "Mobius", true);
				ThreadStart ts = new ThreadStart(CreateDocumentThreadMethod);
				Thread newThread = new Thread(ts);
        newThread.IsBackground = true;
				newThread.SetApartmentState(ApartmentState.STA);
				newThread.Start();

				while (true)
				{
					Thread.Sleep(10000);
					if (!Ps.Document.IsCreating) break;
					Progress.Show("Formatting preview page " + (Ps.Document.PageCount + 1).ToString() + "...");
					if (Progress.CancelRequested)
					{
						Ps.ExecCommand(PrintingSystemCommand.StopPageBuilding);
						Thread.Sleep(250);
						break;
					}
				}

				Progress.Hide();
			}
			catch (Exception ex) { ex = ex; }
		}

		void CreateDocumentThreadMethod()
		{
			try
			{
				Pcl.CreateDocument();
				return;
			}
			catch (Exception ex) { ex = ex; } // cross thread exception caught here
		}

/// <summary>
/// Set values for popup
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void ScalePopup_Popup(object sender, EventArgs e)
		{
			if (Qm.Query.PrintScale > 0) // scale to percentage
			{
				ScaleAdjustToOption.CheckState = CheckState.Checked;
				ScaleFitToOption.CheckState = CheckState.Unchecked;
				ScaleAdjustToSpinner.Value = Qm.Query.PrintScale;
				ScaleFitToSpinner.Value = 1;
				ScaleFitToSpinner.Focus();
			}

			else // fit-to a number of pages
			{
				ScaleFitToOption.CheckState = CheckState.Checked;
				ScaleAdjustToOption.CheckState = CheckState.Unchecked;
				ScaleFitToSpinner.Value = -Qm.Query.PrintScale;
				ScaleAdjustToSpinner.Value = (int)(PrintControl.PrintingSystem.Document.ScaleFactor * 100); // show pct also
				ScaleAdjustToSpinner.Focus();
			}
		}

		private void ScaleAdjustToOption_Click(object sender, EventArgs e)
		{
			ScaleAdjustToOption.CheckState = CheckState.Checked;
			ScaleAdjustToSpinner.Focus();
		}

		private void ScaleFitToOption_Click(object sender, EventArgs e)
		{
			ScaleFitToOption.CheckState = CheckState.Checked;
			ScaleFitToSpinner.Focus();
		}

		private void ScaleAdjustToSpinner_Click(object sender, EventArgs e)
		{
			ScaleAdjustToOption.CheckState = CheckState.Checked;
		}

		private void ScaleFitToSpinner_Click(object sender, EventArgs e)
		{
			ScaleFitToOption.CheckState = CheckState.Checked;
		}

		private void ScaleAdjustToSpinner_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter) ScalePopupOk_Click(null, null);
		}

		private void ScaleFitToSpinner_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter) ScalePopupOk_Click(null, null);
		}

		private void ScaleAdjustToOption_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter) ScalePopupOk_Click(null, null);
		}

		private void ScaleFitToOption_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter) ScalePopupOk_Click(null, null);
		}

		/// <summary>
		/// Accept the values
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ScalePopupOk_Click(object sender, EventArgs e)
		{
			PrintingSystem ps = (PrintingSystem)Instance.PrintControl.PrintingSystem;
			PrintableComponentLink pcl = ps.Links[0] as PrintableComponentLink;

			Qm.DataTableManager.ResetFormattedBitmaps(); // reset so structures are drawn correct size

			if (Qm.MoleculeGrid.MainView is BandedGridView)
			{
				BandedGridView v = Qm.MoleculeGrid.MainView as BandedGridView;
				v.OptionsPrint.UsePrintStyles = false;
				if (ScaleAdjustToOption.CheckState == CheckState.Checked)
				{ // Scale the view not the page
					ps.Document.ScaleFactor = 1.0f; // Keep page scale at 100%
					ps.Document.AutoFitToPagesWidth = 0; // no autofit
					Qm.Query.PrintScale = (int)ScaleAdjustToSpinner.Value; // store with query
					Qm.ResultsFormat.PageScale = Qm.Query.PrintScale; // for fieldFormatter use
					Qm.MoleculeGrid.ScaleBandedGridView(Qm.Query.PrintScale / 100.0);
					pcl.CreateDocument(); // recreate the doc
				}

				else
				{
					Qm.MoleculeGrid.ScaleBandedGridView(1.0); // scale grid to full size before setting doc scale
					pcl.CreateDocument(); // create the doc full size
					ps.Document.AutoFitToPagesWidth = (int)(ScaleFitToSpinner.Value);
					Qm.Query.PrintScale = (int)-ScaleFitToSpinner.Value; // store fit-to as negative value
					int pctScale = (int)(ps.Document.ScaleFactor * 100);
					Qm.ResultsFormat.PageScale = pctScale;
					Qm.MoleculeGrid.ScaleBandedGridView(pctScale / 100.0); // scale again to get structures correct size
					pcl.CreateDocument(); // recreate the doc (this takes the most time, could probably refactor to avoid this)
				}
			}

			else if (Qm.MoleculeGrid.MainView is CardView)
			{
				CardView v = Qm.MoleculeGrid.MainView as CardView;

				if (ScaleAdjustToOption.Checked)
				{ // scale the card not the page
					ps.Document.ScaleFactor = 1.0f; // Keep page scale at 100%
					ps.Document.AutoFitToPagesWidth = 0; // no autofit
					Qm.Query.PrintScale = (int)ScaleAdjustToSpinner.Value; // store with query
					Qm.ResultsFormat.PageScale = Qm.Query.PrintScale; // for fieldFormatter use
					Qm.MoleculeGrid.ScaleCardView(Qm.Query.PrintScale / 100.0); // scale the cards
					pcl.CreateDocument(); // recreate the doc
				}

				else
				{
					ps.Document.AutoFitToPagesWidth = (int)(ScaleFitToSpinner.Value);
					Qm.Query.PrintScale = (int)-ScaleFitToSpinner.Value; // store fit-to as negative value
					Qm.ResultsFormat.PageScale = (int)(ps.Document.ScaleFactor * 100);
					Qm.MoleculeGrid.ScaleCardView(1.0); // make the cards normal size
					pcl.CreateDocument(); // recreate the doc
				}
			}

			else throw new Exception("Expected BandedGridView or CardView");

			ScalePopup.HidePopup();
		}

		private void ScalePopupCancel_Click(object sender, EventArgs e)
		{
			ScalePopup.HidePopup();
		}

		/// <summary>
		/// Print button pressed 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void PrintButtonBarItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			//PrintingSystem ps = Instance.PrintControl.PrintingSystem;
			//ps.ExecCommand(PrintingSystemCommand.ExportPdf);
			//return;

			DialogResult dr = CompleteRetrieval();
			if (dr == DialogResult.Cancel) return;

			PrintingSystem ps = (PrintingSystem)Instance.PrintControl.PrintingSystem;
			ps.PrintDlg();
			return;
		}

/// <summary>
/// Complete retrieval and then execute command
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void CompleteRetrievalAndExecCommand(PrintingSystemCommand cmd)
		{
			DialogResult dr = CompleteRetrieval();
			if (dr == DialogResult.Cancel) return;

			PrintingSystem ps = (PrintingSystem)Instance.PrintControl.PrintingSystem;
			ps.ExecCommand(cmd);
		}


/// <summary>
/// Complete data retrieval
/// </summary>
/// <returns></returns>
		DialogResult CompleteRetrieval()
		{
			PrintingSystem ps = (PrintingSystem)Instance.PrintControl.PrintingSystem;
			PrintableComponentLink pcl = ps.Links[0] as PrintableComponentLink;

			DataTableManager dtm = Qm.DataTableManager;
			if (!dtm.RowRetrievalComplete)
			{ // be sure we have the complete set of rows
				MoleculeGridControl mg = Qm.MoleculeGrid;
				Qm.MoleculeGrid = null; // don't update the grid itself
				DialogResult dr = dtm.CompleteRetrieval();
				Qm.MoleculeGrid = mg;

				if (dr == DialogResult.Cancel) return dr;
			}

			if (Qm.DataTable.Rows.Count != PreviewRowCount)
				pcl.CreateDocument(); // recreate the document with full set of rows

			return DialogResult.OK;
		}

/// <summary>
/// Export commands
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void ExportPdf_ItemClick(object sender, ItemClickEventArgs e)
		{
			CompleteRetrievalAndExecCommand(PrintingSystemCommand.ExportPdf);
		}

		private void ExportHtmlBarItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			CompleteRetrievalAndExecCommand(PrintingSystemCommand.ExportHtm);
		}

		private void ExportTextBarItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			CompleteRetrievalAndExecCommand(PrintingSystemCommand.ExportTxt);
		}

		private void ExportCsvBarItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			CompleteRetrievalAndExecCommand(PrintingSystemCommand.ExportHtm);
		}

		private void ExportMhtBarItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			CompleteRetrievalAndExecCommand(PrintingSystemCommand.ExportMht);
		}

		private void ExportXlsBarItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			CompleteRetrievalAndExecCommand(PrintingSystemCommand.ExportXls);
		}

		private void ExportXlsxBarItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			CompleteRetrievalAndExecCommand(PrintingSystemCommand.ExportXlsx);
		}

		private void ExportRtfBarItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			CompleteRetrievalAndExecCommand(PrintingSystemCommand.ExportRtf);
		}

		private void ExportImageBarItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			CompleteRetrievalAndExecCommand(PrintingSystemCommand.ExportGraphic);
		}

		void PrintingSystem_StartPrint(object sender, PrintDocumentEventArgs e)
		{
			return;
		}

		void PrintingSystem_PrintProgress(object sender, PrintProgressEventArgs e)
		{
			return;
		}

		void PrintingSystem_EndPrint(object sender, EventArgs e)
		{
			return;
		}

		void PrintableComponentLink_CreateDetailArea(object sender, CreateAreaEventArgs e)
		{
			return; // not called
		}

		void PrintableComponentLink_CreateDetailHeaderArea(object sender, CreateAreaEventArgs e)
		{
			return; // not called
		}

		void PrintableComponentLink_InnerPageHeaderArea(object sender, CreateAreaEventArgs e)
		{
			return; // not called
		}

		void PrintableComponentLink_CreateMarginalHeaderArea(object sender, CreateAreaEventArgs e)
		{
			return; // called once
		}

		void PrintableComponentLink_CreateReportHeaderArea(object sender, CreateAreaEventArgs e)
		{
			return; // called once
		}

	}

}