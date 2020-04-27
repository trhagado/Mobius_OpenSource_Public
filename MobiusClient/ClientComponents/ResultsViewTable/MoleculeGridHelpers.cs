using Mobius.ComOps;
using Mobius.Data;
using Mobius.Helm;
using Mobius.MolLib1;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraEditors.Helpers;
using DevExpress.Utils;

using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Dragging;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mobius.ClientComponents

{
	/// <summary>
	/// Context menus & other helper controls used by MoleculeGrid
	/// </summary>

	public partial class MoleculeGridHelpers : Form
	{
		internal int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		internal MoleculeGridControl Grid; // link to associated Molecule grid control
		internal QueryManager Qm { get { return Grid.QueryManager; } }
		internal ResultsFormat Rf { get { return Grid.QueryManager.ResultsFormat; } }
		internal DataTableMx DataTable { get { return Grid.DataTable; } }
		internal DataTableManager Dtm { get { return Grid.Dtm; } }
		internal QueryEngine Qe { get { return Grid.Qe; } }

		internal QueryResultsControl Qrc { get { return QueryResultsControl.GetQrcThatContainsControl(Grid); } }

		internal DataRowMx NewRow = null; // new row being created
		internal bool SettingStructure = false;

		internal bool InCustomUnboundColumnData = false;

		internal int DragStartIdx; // position of drag band or col at start of drag
		internal static DateTime WaitForMoreDataStartTime = DateTime.Now;

		public delegate void EventDelegate(object sender, EventArgs e);
		internal EventDelegate ED1, ED2, ED3, ED4;

		static int ExtraColumns = 1; // extra column making ColWidth a bit bigger than FieldWidth

		public static MoleculeRendererType DefaultEditor = MoleculeRendererType.Chemistry;

		public static int UnboundCalls = 0;
		public static int UnboundGets = 0;
		public static int UnboundSets = 0;
		public static int FormatFieldCalls = 0;
		public static int FormatStructureFieldCalls = 0;
		public static int FormatImageFieldCalls = 0;
		public static bool DebugDetails { get { return DataTableManager.DebugDetails; } }

		/// <summary>
		/// Basic constructor
		/// </summary>

		public MoleculeGridHelpers()
		{
			InitializeComponent();

			return;
		}

		/// <summary>
		/// Build and display the context menu for a click on a CID cell
		/// </summary>
		/// <param name="ci"></param>

		internal void ShowCidClickContextMenu(CellInfo ci)
		{
			CidViewStructureInNewWindow.Visible = CanRetrieveStructureForCurrentCell(); // true
			CidRetrieveRelatedCompoundData.Visible = true;
			CidSelectAllCompoundData.Visible = CanRetrieveAllDataForCurrentCell(); // true;

			bool isPrimaryCid = DoesCurrentCellContainPrimaryCompoundId();
			CidShowSpecialSeparator.Visible = isPrimaryCid;
			CidShowStbViewForCompound.Visible = isPrimaryCid && QbUtil.IsMdbAssayDataViewAvailable();
			CidShowCorpIdChangeHistory.Visible = isPrimaryCid;

			int x = Grid.LastMouseDownEventArgs.X;
			int y = Grid.LastMouseDownEventArgs.Y;
			CidContextMenu.Show(Grid, new System.Drawing.Point(x, y));
		}

		/// <summary>
		/// Build & display the rt-click context menu for a data cell
		/// based on the data type of the column and other contextual
		/// information.
		/// </summary>
		/// <param name="ci"></param>

		internal void ShowCellRightClickContextMenu(CellInfo ci, MouseEventArgs e)
		{
			QueryColumn qc;
			MetaColumn mc;
			MetaColumnType colType;

			int gri = ci.GridRowHandle;
			int gci = ci.GridColAbsoluteIndex;
			if (ci.Qc != null)
			{
				qc = ci.Qc;
				mc = ci.Mc;
				colType = mc.DataType;
			}
			else
			{
				qc = null;
				mc = null;
				colType = MetaColumnType.Unknown; // indicator or checkmark column
			}



			EditNonTextMenuItem.Visible = false;
			EditNonTextMenuItem1.Visible = false;
			EditNonTextMenuItem2.Visible = false;

			toolStripSeparator1.Visible = false;
			CopyGridRangeMenuItem.Visible = false;
			CutGridRangeMenuItem.Visible = false;
			PasteGridRangeMenuItem.Visible = false;

			toolStripSeparator2.Visible = false;
			InsertRowMenuItem.Visible = false;
			DeleteRowMenuItem.Visible = false;

			if (gci < 0) // clicked in indicator column
			{
				if (Grid.Mode != MoleculeGridMode.DataSetView && Grid.Mode != MoleculeGridMode.LocalView) return; // only allow row add/delete in dsviewer for now
				InsertRowMenuItem.Visible = true;
				DeleteRowMenuItem.Visible = true;
				CellRtClickContextMenu.Show(Grid,
						new System.Drawing.Point(e.X, e.Y));
				return;
			}

			if (colType == MetaColumnType.Unknown) return; // ignore if unknown column type

			CopyGridRangeMenuItem.Visible = true; // show items that are visible for all cases below

			toolStripSeparator3.Visible = false;
			OpenHyperlinkMenuItem.Visible = false;
			EditHyperlinkMenuItem.Visible = false;

			toolStripSeparator4.Visible = false;

			ViewStructureInNewWindowMenuItem.Visible = false;
			RetrieveRelatedCompoundData.Visible = false;
			SelectAllCompoundDataMenuItem.Visible = false;
			ShowSpecialSeparator.Visible = false;
			ShowStbViewForCompoundMenuItem.Visible = false;
			ShowCorpIdRegistrationHistoryMenuItem.Visible = false;

			GridColumn gc = Grid.V.Columns[gci];
			bool allowEdit = gc.OptionsColumn.AllowEdit;

			string dataTypeName = "";

			if (colType == MetaColumnType.Structure)
			{ // setup for structure 
				dataTypeName = "Structure";
				if (allowEdit)
				{
					MoleculeMx cs = Grid.GetCellMolecule(gri, gci);

					EditNonTextMenuItem.Visible = true;

					MoleculeRendererType editor = DefaultEditor;

					if (cs != null && cs.IsChemStructureFormat)
						editor = MoleculeRendererType.Chemistry;

					if (cs != null && cs.IsBiopolymerFormat)
						editor = MoleculeRendererType.Helm;

					EditNonTextMenuItem1.Visible = true;
					EditNonTextMenuItem1.Text = "Use Chemistry Editor";
					EditNonTextMenuItem1.Checked = (editor == MoleculeRendererType.Chemistry);

					EditNonTextMenuItem2.Visible = true;
					EditNonTextMenuItem2.Text = "Use Helm Editor";
					EditNonTextMenuItem2.Checked = (editor == MoleculeRendererType.Helm);
				}

				toolStripSeparator4.Visible = true;
				ViewStructureInNewWindowMenuItem.Visible = CanRetrieveStructureForCurrentCell(); // true;
				RetrieveRelatedCompoundData.Visible = true; //isRootTableStructure;
			}

			else if (colType == MetaColumnType.Image)
			{ // setup for image
				dataTypeName = "Image";
				if (allowEdit)
				{
					EditNonTextMenuItem.Visible = true;
					toolStripSeparator1.Visible = true;
				}
			}

			else if (colType == MetaColumnType.CompoundId) // && mc.IsKey)
			{
				dataTypeName = "Text/Number(s)";
				toolStripSeparator4.Visible = true;
				ViewStructureInNewWindowMenuItem.Visible = CanRetrieveStructureForCurrentCell(); // true
				RetrieveRelatedCompoundData.Visible = true;
				SelectAllCompoundDataMenuItem.Visible = CanRetrieveAllDataForCurrentCell(); // true;

				bool isPrimaryCid = DoesCurrentCellContainPrimaryCompoundId();
				ShowSpecialSeparator.Visible = isPrimaryCid;
				ShowStbViewForCompoundMenuItem.Visible = isPrimaryCid && QbUtil.IsMdbAssayDataViewAvailable();
				ShowCorpIdRegistrationHistoryMenuItem.Visible = isPrimaryCid;
			}

			else
			{
				dataTypeName = "Text/Number(s)";

				//bool hasAssocMol = false;
				//if (Grid.GetCellMolecule(gri, gci) != null) hasAssocMol = true; // if structure in cell then allow copy operation
				//else if (colType == MetaColumnType.CompoundId && Rf.CombineStructureWithCompoundId) hasAssocMol = true;
			}

			CopyGridRangeMenuItem.Text = "Copy " + dataTypeName;
			CutGridRangeMenuItem.Text = "Cut " + dataTypeName;
			PasteGridRangeMenuItem.Text = "Paste " + dataTypeName;
			EditNonTextMenuItem.Text = "Edit " + dataTypeName + "...";

			if (!String.IsNullOrEmpty(Grid.GetCellHyperlink(gri, gci)) && // show open hyperlink item if link exists and not a cid, structure
					colType != MetaColumnType.CompoundId && colType != MetaColumnType.Structure && !qc.IsHtmlContent)
			{
				toolStripSeparator3.Visible = true;
				OpenHyperlinkMenuItem.Visible = true;
			}

			if (allowEdit)
			{
				CutGridRangeMenuItem.Visible = true;
				PasteGridRangeMenuItem.Visible = true;

				if (colType == MetaColumnType.CompoundId && mc.IsKey) { } // no editing of hyperlinks for key cids
				else if (colType == MetaColumnType.Structure) { } // no editing of hyperlink for structures

				else if (qc.IsHtmlContent)
				{
					EditNonTextMenuItem.Visible = true;
					toolStripSeparator1.Visible = true;
				}

				else // allow hyperlink edit for others
				{
					EditHyperlinkMenuItem.Visible = true;
				}

				if (Grid.Mode == MoleculeGridMode.DataSetView) // only allow row add/delete in dsviewer for now
				{
					toolStripSeparator2.Visible = true;
					InsertRowMenuItem.Visible = true;
					DeleteRowMenuItem.Visible = true;
				}
			}

			CellRtClickContextMenu.Show(Grid,
					new System.Drawing.Point(e.X, e.Y));

			return;
		}

		public bool CanRetrieveStructureForCurrentCell()
		{
			string cid;
			RootTable rt;

			if (!GetCidAndRootTableForCurrentCell(out cid, out rt))
				return false;

			else if (!rt.IsStructureTable) return false;

			else if (IsSmallWorldOnly(rt)) return false;

			else return true;
		}

		public bool CanRetrieveAllDataForCurrentCell()
		{
			string cid;
			RootTable rt;

			if (!GetCidAndRootTableForCurrentCell(out cid, out rt))
				return false;

			else if (IsSmallWorldOnly(rt)) return false;

			else return true;
		}

		public bool DoesCurrentCellContainPrimaryCompoundId()
		{
			string cid;
			RootTable rt;

			if (!GetCidAndRootTableForCurrentCell(out cid, out rt))
				return false;

			else if (Lex.Eq(rt.MetaTableName, MetaTable.PrimaryRootTable))
				return true;

			else return false;
		}

		public bool GetCidAndRootTableForCurrentCell(
				out string cid,
				out RootTable rt)
		{
			cid = null;
			rt = null;

			CellInfo ci = Grid.LastMouseDownCellInfo;
			if (ci == null || ci.Rfld == null) return false;

			cid = GetCidForSelectedCell();
			if (Lex.IsUndefined(cid)) return false;

			MetaTable mt = CompoundId.GetRootMetaTableFromCid(cid);
			if (mt == null) return false;

			rt = RootTable.GetFromTableName(mt.Name);
			if (rt == null) return false;
			else return true;
		}

		public static bool IsSmallWorldOnly(RootTable rt)
		{
			if (rt.SmallWorldSearchable && !rt.CartridgeSearchable) return true;
			else return false;
		}

		// Event handlers for popup menu click events

		/// <summary>
		/// Copy a selected range from worksheet to clipboard
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		public void CopyGridRangeMenuItem_Click(object sender, System.EventArgs e)
		{
			SessionManager.LogCommandUsage("GridCopy");

			int r = Grid.LastMouseDownRowIdx;
			int c = Grid.LastMouseDownCol;
			Grid.CopyCommand(r, c);
		}

		/// <summary>
		/// Cut a selected range from worksheet to clipboard
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		public void CutGridRangeMenuItem_Click(object sender, System.EventArgs e)
		{
			SessionManager.LogCommandUsage("GridCut");

			int r = Grid.LastMouseDownRowIdx;
			int c = Grid.LastMouseDownCol;
			Grid.CutCommand(r, c);
		}

		/// <summary>
		/// Paste a selected range into worksheet
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		public void PasteGridRangeMenuItem_Click(object sender, System.EventArgs e)
		{
			SessionManager.LogCommandUsage("GridPaste");

			int r = Grid.LastMouseDownRowIdx;
			int c = Grid.LastMouseDownCol;
			Grid.PasteCommand(r, c);
		}

		private void InsertRowMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("GridInsertRow");

			int r = Grid.LastMouseDownRowIdx;
			if (r == GridControl.NewItemRowHandle) return;
			int c = Grid.LastMouseDownCol;
			CellInfo ci = Grid.GetCellInfo(r, c);

			DataRowMx dr = Grid.DataTable.NewRow();
			Dtm.InitializeRowAttributes(dr);
			if (Grid.Editor != null) Dtm.DataModified = true;

			Grid.DataTable.Rows.InsertAt(dr, ci.DataRowIndex);
		}

		private void DeleteRowMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("GridDeleteRow");

			int r = Grid.LastMouseDownRowIdx;
			if (r == GridControl.NewItemRowHandle) return;
			int c = Grid.LastMouseDownCol;
			CellInfo ci = Grid.GetCellInfo(r, c);

			Grid.BeginUpdate(); // do delete within update to assure proper redisplay of grid

			Dtm.DeleteRow(ci.DataRowIndex);

			Grid.DataSource = Dtm.DataTableMx;
			Grid.EndUpdate();
			Grid.RefreshDataSource();

			if (Grid.Editor != null)
				Dtm.DataModified = true;
		}

		public void EditNonTextMenuItem_Click(object sender, EventArgs e)
		{
			int r = Grid.LastMouseDownRowIdx;
			int c = Grid.LastMouseDownCol;
			CellInfo ci = Grid.LastMouseDownCellInfo;
			QueryColumn qc = ci?.Qc;

			if (Grid.GetMetaColumnDataType(c) == MetaColumnType.Structure &&
			 Grid.Mode != MoleculeGridMode.IgnoreEvents)
			{
				SessionManager.LogCommandUsage("GridEditMolecule");
				EditMolecule(r, c, MoleculeRendererType.Unknown);
			}

			else if (qc != null && qc.IsHtmlContent)
			{
				SessionManager.LogCommandUsage("GridEditHtml");
				Grid.EditHtmlTextCellValue(ci);
			}

			else return; 
		}

/// <summary>
/// Edit chem structure
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void EditNonTextMenuItem1_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("GridEditStructure");

			int r = Grid.LastMouseDownRowIdx;
			int c = Grid.LastMouseDownCol;
			CellInfo ci = Grid.LastMouseDownCellInfo;
			QueryColumn qc = ci?.Qc;

			if (Grid.GetMetaColumnDataType(c) == MetaColumnType.Structure &&
			 Grid.Mode != MoleculeGridMode.IgnoreEvents)
			{
				DefaultEditor = MoleculeRendererType.Chemistry;
				EditMolecule(r, c, MoleculeRendererType.Chemistry);
			}
		}

		private void EditNonTextMenuItem2_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("GridEditHelm");

			int r = Grid.LastMouseDownRowIdx;
			int c = Grid.LastMouseDownCol;
			CellInfo ci = Grid.LastMouseDownCellInfo;
			QueryColumn qc = ci?.Qc;

			if (Grid.GetMetaColumnDataType(c) == MetaColumnType.Structure &&
			 Grid.Mode != MoleculeGridMode.IgnoreEvents)
			{
				EditMolecule(r, c, MoleculeRendererType.Helm);
			}
		}


		/*************** Old stuff restored **********************/

		public void EditMoleculeOld(
	int r,
	int c,
	MoleculeRendererType editorType)
		{
			MoleculeMx mol = null, mol2 = null;
			try
			{
				r = MakeGridRowReal(r);

				mol = Grid.GetCellMolecule(r, c);

				Grid.EditStructureRow = r; // location of molecule we are editing
				Grid.EditStructureColumn = c;

				if (editorType != MoleculeRendererType.Unknown)
					DefaultEditor = editorType;

				if (mol == null || mol.PrimaryFormat == MoleculeFormat.Unknown || Lex.IsUndefined(mol.PrimaryValue)) // undefined structure?
				{ // create empty structure
					if (DefaultEditor == MoleculeRendererType.Chemistry)
						mol = new MoleculeMx(MoleculeFormat.Molfile);

					else if (DefaultEditor == MoleculeRendererType.Helm)
						mol = new MoleculeMx(MoleculeFormat.Helm);

					else mol = new MoleculeMx(MoleculeFormat.Molfile);
				}

				mol2 = MoleculeEditor.Edit(mol, editorType);
				if (mol2 != null)
					SetEditedCellMolecule(mol2);

				Grid.AddNewRowAsNeeded();
				return;
			}

			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message, "Error editing molecule");
				return;
			}
		}

		/// <summary>
		/// Edit molecule in specified cell 
		/// </summary>
		/// <param name="r"></param>
		/// <param name="c"></param>

		public void EditMolecule(
				int r,
				int c,
				MoleculeRendererType editorType)
		{
			MoleculeMx mol = null, mol2 = null;
			try
			{
				r = MakeGridRowReal(r);

				mol = Grid.GetCellMolecule(r, c);

				Grid.EditStructureRow = r; // location of molecule we are editing
				Grid.EditStructureColumn = c;

				if (editorType != MoleculeRendererType.Unknown)
						DefaultEditor = editorType;

				if (mol == null || mol.PrimaryFormat == MoleculeFormat.Unknown || Lex.IsUndefined(mol.PrimaryValue)) // undefined structure?
				{ // create empty structure
					if (DefaultEditor == MoleculeRendererType.Chemistry)
						mol = new MoleculeMx(MoleculeFormat.Molfile);

					else if (DefaultEditor == MoleculeRendererType.Helm)
						mol = new MoleculeMx(MoleculeFormat.Helm);

					else mol = new MoleculeMx(MoleculeFormat.Molfile);
				}

				mol2 = MoleculeEditor.Edit(mol, editorType);
				if (mol2 != null)
					SetEditedCellMolecule(mol2);

				Grid.AddNewRowAsNeeded();
				return;
			}

			catch (Exception ex)
			{
				XtraMessageBox.Show(ex.Message, "Error editing molecule");
				return;
			}
		}

		/// <summary>
		/// Make sure the specified grid row is a real row rather than a new row
		/// </summary>
		/// <param name="gr"></param>
		/// <returns></returns>

		public int MakeGridRowReal(
				int gr)
		{
			if (gr >= 0) return gr; // ok as is

			else if (gr == GridControl.NewItemRowHandle) // if updating new row make it real before starting edit
			{
				gr = DataTable.Rows.Count; // where new row will be
				if (!Grid.V.FocusedRowModified) Grid.V.AddNewRow(); // need to add row if no mods for this row
				bool success = Grid.V.UpdateCurrentRow(); // this causes row to be added to dataset
				Grid.V.FocusedRowHandle = gr;
				CellInfo ci = Grid.LastMouseDownCellInfo;
				if (ci != null && ci.GridRowHandle == GridControl.NewItemRowHandle)
				{ // if mouse down began on new item adjust row indexes to real row that should exist now
					ci.GridRowHandle = gr;
					ci.DataRowIndex = gr;
				}

				DataRowMx dr = DataTable.Rows[gr]; // init attributes for row if table has an attributes column
				if (Dtm.RowAttributesVoPos >= 0 && dr[Dtm.RowAttributesVoPos] == null)
					Dtm.InitializeRowAttributes(dr);

				return gr;
			}

			else throw new Exception("MakeGridRowReal unrecognized RowHandle: " + gr);
		}

/// <summary>
/// Update cell value with modified molecule
/// </summary>
/// <param name="cs"></param>

		private void SetEditedCellMolecule (
			MoleculeMx cs)
		{
			int r = Grid.EditStructureRow;
			int c = Grid.EditStructureColumn;

			if (r < 0 || c < 0) return;
			CellInfo ci = Grid.GetCellInfo(r, c);

			Grid.GetEditor().SetCellValue(ci, cs); // store in DataTable & any assoc Oracle table
			Grid.RefreshDataSource(); // force redraw of grid to set proper row height for structure
		}

		private void menuCopyNonText_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("GridCopyStructure");

			int r = Grid.LastMouseDownRowIdx;
			int c = Grid.LastMouseDownCol;

			if (Grid.GetMetaColumnDataType(c) == MetaColumnType.Structure ||
					Grid.GetCellMolecule(r, c) != null)
			{ // copy structure to clipboard
				MoleculeMx mol = Grid.GetCellMolecule(r, c);
				if (mol == null) return;

				MoleculeControl.CopyMoleculeToClipboard(mol);
			}

			else if (Grid.GetMetaColumnDataType(c) == MetaColumnType.Image ||
					Grid.GetCellImage(r, c) != null)
			{ // copy image
				Image i = Grid.GetCellImage(r, c);
				Clipboard.SetImage(i);
			}

			return;
		}

		/// <summary>
		/// Open a hyperlinked menu item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void OpenHyperlinkMenuItem_Click(object sender, System.EventArgs e)
		{ // User rt-clicked Open Hyperlink menu item on cell with link
			SessionManager.LogCommandUsage("GridOpenHyperlink");

			Grid.ProcessHyperlinkClick(Grid.LastMouseDownCellInfo, true);
		}

		/// <summary>
		/// Edit hyperlink for an annotation table cell
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void EditHyperlinkMenuItem_Click(object sender, System.EventArgs e)
		{
			string hyperlink;

			SessionManager.LogCommandUsage("GridEditHyperlink");

			int gr = Grid.Row;
			int gc = Grid.Col;

			if (Grid.GetMetaColumnDataType(gc) == MetaColumnType.CompoundId)  // if cid, edit in place
			{
				Grid.V.ShowEditor();
				return;
			}

			EditHyperlink ehf = new EditHyperlink();

			CellInfo ci = Grid.GetCellInfo(gr, gc);
			if (!ci.IsValidDataCell || !(ci.DataValue is MobiusDataType)) return;

			ehf.DisplayText.Text = FormatFieldText(ci);
			ehf.Address.Text = Grid.GetCellHyperlink(ci);
			while (true)
			{
				DialogResult dlgRes = ehf.ShowDialog(SessionManager.ActiveForm);
				if (dlgRes != DialogResult.OK) return;

				string address = ehf.Address.Text;
				string text = ehf.DisplayText.Text;

				try { Grid.GetEditor().SetCellValueFromText(ci, text, address); }
				catch (Exception ex)
				{
					MessageBoxMx.ShowError(ex.Message);
				}

				Grid.RefreshRowCell(gr, ci.GridColumn);
				return;
			}
		}

		private void ViewStructureInNewWindowMenuItem_Click(object sender, EventArgs e)
		{
			string cid = GetCidForSelectedCell();
			if (Lex.IsUndefined(cid)) return;
			CommandExec.ExecuteCommandAsynch("SelectCompoundStructure " + cid);
			return;
		}

		/// <summary>
		/// Get the CID for the selected cell or the row containing the selected cell
		/// </summary>

		public string GetCidForSelectedCell()
		{
			CellInfo ci = Grid.LastMouseDownCellInfo;
			if (ci == null || ci.DataValue == null) return null;
			if (ci.Mc.DataType == MetaColumnType.CompoundId) ci.DataValue.ToString();

			DataTableManager dtm = Grid.Dtm;
			DataTableMx dt = dtm.Grid.DataTable;
			if (dtm == null || dt == null) return null;

			int ri = ci.DataRowIndex;
			if (ri < 0 || ri >= dt.Rows.Count) return null;

			DataRowMx dr = dt[ri] as DataRowMx;

			string cid = GetAssociatedCidFromDataRowForQueryColumn(dtm, dr, ci);

			return cid;
		}

		/// <summary>
		/// Scan backwards in the datarow for the first cid or key value to associate with the specified column
		/// </summary>
		/// <param name="dtm"></param>
		/// <param name="dr"></param>
		/// <param name="ci"></param>
		/// <returns></returns>

		public string GetAssociatedCidFromDataRowForQueryColumn(
			DataTableManager dtm,
			DataRowMx dr,
			CellInfo ci)
		{
			string cid = "";
			ResultsTable rt = ci.Rt;
			ResultsField rfld = ci.Rfld;

			object[] voa = dr.ItemArrayRef;

			int fi = rt.Fields.IndexOf(rfld);

			if (fi >= 0)
			{
				for (fi = fi; fi >= 0; fi--)
				{
					rfld = rt.Fields[fi];
					MetaColumn mc = rfld.MetaColumn;
					if (mc.DataType == MetaColumnType.CompoundId)
					{
						object vo = voa[rfld.VoPosition];

						object o = MobiusDataType.ConvertToPrimitiveValue(vo, mc);
						if (o == null) return "";

						cid = o.ToString();
						cid = CompoundId.Normalize(cid);
						return cid;
					}
				}
			}

			cid = dtm.GetRowKey(dr);
			return cid;
		}

		/// <summary>
		/// Show all data for key
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void SelectAllCompoundDataMenuItem_Click(object sender, EventArgs e)
		{
			SelectAllCompoundData_Click(sender, e);
		}

		/// <summary>
		/// Show MultiDb view for key as appropriate
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ShowMultiDbViewForCompoundMenuItem_Click(object sender, EventArgs e)
		{
			ShowStbViewForCompound_Click(sender, e);
		}

		/// <summary>
		/// Pick up a cid and execute rt-click command against it
		/// </summary>
		/// <param name="command"></param>
		/// <param name="logEntry"></param>

		void GetCidAndExecute(
				string command,
				string logEntry)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Process extra generic command
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ShowStbViewForCompound2_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("GridShowStbViewForCompound");
			CommandExec.ExecuteCommandAsynch("ShowStbViewForCompound " + Grid.SelectedCid);
		}


		/// <summary>
		/// Format the text of a field value
		/// </summary>
		/// <param name="ci"></param>
		/// <returns></returns>

		internal string FormatFieldText(CellInfo ci)
		{
			if (ci.DataValue == null) return "";

			else if (ci.DataValue is MobiusDataType) // see if already have formatted value
			{
				MobiusDataType mdt = ci.DataValue as MobiusDataType;
				if (mdt.FormattedText != null) return mdt.FormattedText;
			}

			FormattedFieldInfo ffi = Qm.ResultsFormatter.FormatField(ci.Rt, ci.TableIndex, ci.Rfld, ci.FieldIndex,
					DataTable.Rows[ci.DataRowIndex], ci.DataRowIndex, ci.DataValue, -1, false);

			return ffi.FormattedText;
		}

		/// <summary>
		/// Show table description
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ShowTableDescriptionMenuItem_Click(object sender, System.EventArgs e)
		{
			SessionManager.LogCommandUsage("GridShowTableDescription");
			QbUtil.ShowTableDescription(Grid.LastMouseDownQueryTable.MetaTable.Name);
		}

		/// <summary>
		/// Add table to query
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void AddTableToQueryMenuItem_Click(object sender, System.EventArgs e)
		{
			SessionManager.LogCommandUsage("GridAddTableToQuery");
			QbUtil.AddTablesToQuery(Grid.LastMouseDownQueryTable.MetaTable.Name);
		}

		/// <summary>
		/// Rename table
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void RenameTableMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("GridRenameTable");

			QueryTable qt = Grid.LastMouseDownQueryTable;
			if (qt == null) return;

			DialogResult dr = QueryTableControl.QueryTableRenameDialog_Show(qt);
			if (dr == DialogResult.Cancel) return;

			BaseQueryQt(qt).Label = qt.Label; // be sure base query is in synch

			RestartDisplayWithNewHeaders();
			return;
		}

		/////////////////////////////////////////////////
		// Event handlers for WsGridColumnHeaderPopupMenu
		/////////////////////////////////////////////////

		/// <summary>
		/// Sort ascending
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void SortAscendingMenuItem_Click(object sender, System.EventArgs e)
		{
			SessionManager.LogCommandUsage("GridColumnSortAscending");
			SortOnCurrentColumn(SortOrder.Ascending);
		}

		internal void SortOnCurrentColumn(SortOrder direction)
		{
			if (Dtm.DataModified)
			{
				MessageBoxMx.ShowError("You must save the current changes to the dataset before you can sort");
				return;
			}

			CellInfo ci = Grid.LastMouseDownCellInfo;
			if (ci == null || ci.Qc == null)
			{
				MessageBoxMx.ShowError("You must select a valid data column before you can sort");
				return;
			}

			List<SortColumn> sCols = new List<SortColumn>();
			SortColumn sc = new SortColumn();
			sc.QueryColumn = ci.Qc;
			sc.Direction = direction;
			sCols.Add(sc);

			SortGrid(sCols);
			return;
		}

		/// <summary>
		/// Sort descending
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void SortDescendingMenuItem_Click(object sender, System.EventArgs e)
		{
			SessionManager.LogCommandUsage("GridColumnSortDescending");
			SortOnCurrentColumn(SortOrder.Descending); // clicked from grid header
		}

		/// <summary>
		/// Sort on multiple columns
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void SortMultipleMenuItem_Click(object sender, System.EventArgs e)
		{
			SessionManager.LogCommandUsage("GridColumnSortMultiple");
			SortMultiple();
		}

		internal void SortMultiple()
		{
			if (Dtm.DataModified)
			{
				MessageBoxMx.ShowError("You must save the current changes to the dataset before you can sort");
				return;
			}

			List<SortColumn> sCols = SortMultipleDialog.ShowDialog(Grid.Query);
			if (sCols == null) return;
			SortGrid(sCols);
			return;
		}

		/// <summary>
		/// Rename column
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void RenameColumnMenuItem_Click(object sender, System.EventArgs e)
		{
			SessionManager.LogCommandUsage("GridColumnRename");
			RenameColumn();
		}

		internal void RenameColumn()
		{
			CellInfo ci = Grid.LastMouseDownCellInfo;
			if (ci == null || ci.Rfld == null)
			{
				MessageBoxMx.Show(
						"You must first select the column that you\r\n" +
						"want to rename.",
						UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			QueryColumn qc = ci.Qc;
			DialogResult dr = QueryTableControl.QueryColumnRenameDialog_Show(qc);
			if (dr == DialogResult.Cancel) return;

			BaseQueryQc(qc).Label = qc.Label; // be sure base query is in synch

			RestartDisplayWithNewHeaders();
			return;
		}

		/// <summary>
		/// Rename columns for all users
		/// </summary>

		internal void RenameColumnAllUsers()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Define conditional formatting
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DefineCondFormatMenuItem_Click(object sender, System.EventArgs e)
		{
			SessionManager.LogCommandUsage("GridColumnConditionalFormatting");
			DefineCondFormat("");
		}

		/// <summary>
		/// Allow user to create/edit conditional formatting then update display
		/// </summary>

		internal void DefineCondFormat(string cfToUse)
		{
			QueryColumn qc;
			CondFormat cf, oldCf;

			CellInfo ci = Grid.LastMouseDownCellInfo;
			if (ci == null || ci.Rfld == null)
			{
				MessageBoxMx.Show(
						"You must first select the column that you\r\n" +
						"want to define conditional formatting on.",
						UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			qc = ci.Qc;
			if (!CondFormat.CondFormattingAllowed(qc.MetaColumn.DataType))
			{
				MessageBoxMx.Show(
						"Conditional formatting isn't supported for the " + qc.ActiveLabel + " field.",
						UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			if (qc.MetaColumn.DataType == MetaColumnType.CompoundId && qc.QueryTable.MetaTable.IsRootTable && Rf.CombineStructureWithCompoundId)
				qc = Rf.CombinedStructureField.QueryColumn; // use structure formatting here

			oldCf = qc.CondFormat; // save old format
			string serializedOldCf = "";
			if (oldCf != null) serializedOldCf = oldCf.Serialize();
			if (!String.IsNullOrEmpty(cfToUse))
			{ // tentatively put 
				cf = Qm.Query.GetCondFormatByName(cfToUse);
				qc.CondFormat = cf; // tentatively plug in new format
			}

			DialogResult dr = CondFormatEditor.Edit(qc);
			if (dr == DialogResult.Cancel)
			{
				qc.CondFormat = oldCf; // restore any old format
				return;
			}

			BaseQueryQc(qc).CondFormat = qc.CondFormat; // be sure base query is in synch

			Qm.DataTableManager.ResetFormattedValues();
			RestartDisplayWithNewHeaders();
			return;
		}

		/// <summary>
		/// Restart display with new headers
		/// </summary>

		internal void RestartDisplayWithNewHeaders()
		{
			ResultsFormatFactory rff = new ResultsFormatFactory(Qm, Rf);
			rff.FormatSegmentHeaders(null); // rebuild headers
			Qm.ResultsFormatter.FirstSegmentHeaderSize = Rf.Segments[0].Header.Length;
			Grid.FormatGridHeaders(Rf); // format the grid to receive the header content
			Grid.Refresh();
			return;
		}

		/// <summary>
		/// Use the existing named conditional formatting
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void UseNamedCfMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("GridColumnUseNamedCf");
			ToolStripMenuItem mi = (ToolStripMenuItem)sender;
			DefineCondFormat(mi.Tag.ToString());
		}

		/// <summary>
		/// Handle resizing of column via dialog
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ColumnWidthMenuItem_Click(object sender, System.EventArgs e)
		{
			SessionManager.LogCommandUsage("GridColumnWidth");
			SetColumnWidth();
		}

		/// <summary>
		/// Set new column width in the query and results field columns
		/// </summary>

		internal void SetColumnWidth()
		{
			CellInfo ci = Grid.LastMouseDownCellInfo;
			QueryColumn qc = ci.Qc;
			GridColumn gc = ci.GridColumn;
			float originalWidth = qc.DisplayWidth;
			DialogResult dr = QueryColumnWidthDialog.Show(qc);
			if (dr == DialogResult.Cancel) return;

			BaseQueryQc(qc).DisplayWidth = qc.DisplayWidth; // be sure base query is in synch
			ci.Rfld.ColumnWidth = ResultsFormatFactory.QcWidthInCharsToDisplayColWidthInMilliinches(qc.DisplayWidth, Rf); // and ResultsField
			ci.Rfld.FieldWidth = ci.Rfld.ColumnWidth - (ExtraColumns * Rf.CharWidth);

			Grid.InSetup = true;
			gc.Width = Grid.ResultsFieldToGridColumnWidth(ci.Rfld, Rf.PageScale / 100.0, 0);
			Grid.InSetup = false;

			if (qc.MetaColumn.IsGraphical) // cause image redraw at new size
			{
				Dtm.ResetFormattedValues(qc);
				Grid.RefreshDataSource();
			}

			if (Grid.Editor != null) // if editing definition then update underlying metacolumn
			{
				Grid.Editor.UpdateMetaColumnFormatting(qc);
			}

			return;
		}

		/// <summary>
		/// Handle user resizing of column
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void PrototypeGridView_ColumnWidthChanged(object sender, ColumnEventArgs e)
		{
			if (Grid.InSetup) return;

			ColumnInfo ci = Grid.GetColumnInfo(e.Column);
			QueryColumn qc = ci.Qc;
			if (qc == null) return;

			int cWidth = Grid.GridToResultsFieldColumnWidth(e.Column.Width, ci.Rfld, Rf.PageScale / 100.0);

			ci.Rfld.ColumnWidth = cWidth;
			ci.Rfld.FieldWidth = cWidth - ExtraColumns * Rf.CharWidth; // col width is narrower than field width

			qc.DisplayWidth = DisplayWidthInMilliinchesToColumns(cWidth, Rf);
			BaseQueryQc(qc).DisplayWidth = qc.DisplayWidth; // be sure base query is in synch

			if (qc.MetaColumn.IsGraphical) // cause image redraw at new size
			{
				Dtm.ResetFormattedValues(qc);
				Grid.RefreshDataSource();
			}

			if (Grid.Editor != null)
				Grid.Editor.UpdateMetaColumnFormatting(qc);

			return;
		}

		/// <summary>
		/// Convert from display width in milliinches to display width in cols
		/// Essentially the reverse of ResultsFormatFactory.QcWidthInCharsToDisplayColWidthInMilliinches
		/// </summary>
		/// <param name="miWidth"></param>
		/// <param name="rf"></param>
		/// <returns></returns>

		public static float DisplayWidthInMilliinchesToColumns(int miWidth, ResultsFormat rf)
		{
			double w2 = miWidth * (100.0 / SS.I.GridScaleAdjustment); // grid width scale adjustment

			w2 = w2 * (100.0 / rf.PageScale); // adjust scale for page

			int extraColumns = 3; // extra spacing including 2 margin spaces & vertical col divider;
			extraColumns += 1; // need a bit of extra space for grid

			double cols = w2 / rf.CharWidth - extraColumns;

			return (float)cols;
		}

		/// <summary>
		/// Width of band has changed, proportionally adjust each of the columns associated with the band
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void BandedGridView_BandWidthChanged(object sender, DevExpress.XtraGrid.Views.BandedGrid.BandEventArgs e)
		{
			if (Grid.InSetup) return;

			for (int bci = 0; bci < e.Band.Columns.Count; bci++)
			{ // adjust for each column in band
				ColumnInfo ci = Grid.GetColumnInfo(e.Band.Columns[bci]);
				QueryColumn qc = ci.Qc;
				if (qc == null) return;

				int cWidth = Grid.GridToResultsFieldColumnWidth(e.Band.Columns[bci].Width, ci.Rfld, Rf.PageScale / 100.0);

				ci.Rfld.ColumnWidth = cWidth;
				ci.Rfld.FieldWidth = cWidth - ExtraColumns * Rf.CharWidth; // col width is narrower than field width

				qc.DisplayWidth = DisplayWidthInMilliinchesToColumns(cWidth, Rf);
				BaseQueryQc(qc).DisplayWidth = qc.DisplayWidth; // be sure base query is in synch

				if (qc.MetaColumn.IsGraphical) // cause image redraw at new size
				{
					Dtm.ResetFormattedValues(qc);
					Grid.RefreshDataSource();
				}

				if (Grid.Editor != null)
					Grid.Editor.UpdateMetaColumnFormatting(qc);
			}
			return;
		}

		/// <summary>
		/// Hide tables
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void HideTableMenuItem_Click(object sender, EventArgs e)
		{
			if (Grid.BGV == null) return;

			//CellInfo ci = Grid.LastMouseDownCellInfo;
			//if (ci == null || ci.Rfld == null)
			//{
			//  MessageBoxMx.Show(
			//    "You must first select columns for the tables that you\r\n" +
			//    "want to hide.",
			//    UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			//  return;
			//}

			GridCell[] cells = Grid.GetSelectedCells();
			foreach (GridCell c in cells)
			{
				BandedGridColumn bgc = c.Column as BandedGridColumn;
				GridBand gb = bgc.OwnerBand;

				if (!gb.Visible) { }

				gb.Visible = false;
				foreach (BandedGridColumn bgc0 in gb.Columns)
				{
					bgc0.Visible = false;
					SetQcVisible(bgc0, false);
				}
			}

			Grid.Refresh();
			return;
		}

		/// <summary>
		/// Hide columns
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void HideColumnsMenuItem_Click(object sender, EventArgs e)
		{
			HideColumns();
		}

		internal void HideColumns()
		{
			int ci;

			if (Grid.BGV == null) return; // can only hide if GridView

			GridCell[] cells = Grid.GetSelectedCells();
			if (cells.Length == 0)
			{
				MessageBoxMx.Show(
						"You must first select the column(s) that you\r\n" +
						"want to hide.",
						UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			foreach (GridCell c in cells)
			{
				BandedGridColumn bgc = c.Column as BandedGridColumn;

				if (!bgc.Visible) continue;

				bgc.Visible = false;
				SetQcVisible(bgc, false);

				GridBand gb = bgc.OwnerBand;
				if (gb != null)
				{ // see if any visible cells left in band,  hide band if not
					for (ci = 0; ci < gb.Columns.Count; ci++)
					{
						if (gb.Columns[ci].Visible) break;
					}
					if (ci >= gb.Columns.Count) gb.Visible = false;
				}

			}
		}

		/// <summary>
		/// Unhide tables
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void UnhideTableMenuItem_Click(object sender, EventArgs e)
		{
			GridCell[] cells;
			int topRow, bottomRow, ai0, ai1;
			GridColumn leftCol, rightCol;

			if (Grid.BGV == null) return;

			Grid.GetSelectedCells(out cells, out topRow, out bottomRow, out leftCol, out rightCol);
			if (leftCol == null) return;

			for (int ci = leftCol.AbsoluteIndex; ci <= rightCol.AbsoluteIndex; ci++)
			{ // unhide all columns for any band that has a column selected
				BandedGridColumn bgc = Grid.BGV.Columns[ci];
				GridBand gb = bgc.OwnerBand;
				if (gb == null) continue;
				gb.Visible = true;
				foreach (BandedGridColumn bgc0 in gb.Columns)
				{
					if (bgc0.Visible == false)
					{
						bgc0.Visible = true;
						SetQcVisible(bgc0, true);
					}
				}
			}

		}

		/// <summary>
		/// Unhide columns
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void UnhideColumnsMenuItem_Click(object sender, EventArgs e)
		{
			//UnhideColumns(); // (not obvious that you must select a range of cols containing the hidden column first)
			UnhideAllColumns();
		}

		/// <summary>
		/// Unhide any hidden columns between the leftmost and rightmost selected columns
		/// </summary>

		internal void UnhideColumns()
		{
			GridCell[] cells;
			int topRow, bottomRow, ai0, ai1;
			GridColumn leftCol, rightCol;

			if (Grid.BGV == null) return;

			Grid.GetSelectedCells(out cells, out topRow, out bottomRow, out leftCol, out rightCol);
			if (leftCol == null) return;

			for (int ci = leftCol.AbsoluteIndex; ci <= rightCol.AbsoluteIndex; ci++)
			{
				BandedGridColumn bgc = Grid.BGV.Columns[ci];
				if (bgc.OwnerBand != null && bgc.OwnerBand.Visible == false)
					bgc.OwnerBand.Visible = true; // be sure band is visible

				if (bgc.Visible == false)
				{
					bgc.Visible = true;
					SetQcVisible(bgc, true);
				}
			}
		}

		/// <summary>
		/// Set the visiblility for a QueryColumn (and it's base column) for a grid column
		/// </summary>
		/// <param name="gc"></param>
		/// <param name="visible"></param>

		internal void SetQcVisible(GridColumn gc, bool visible)
		{
			ColumnInfo cInf = Grid.GetColumnInfo(gc);
			if (cInf.Qc == null) return;
			cInf.Qc.Hidden = false;
			QueryColumn qc2 = BaseQueryQc(cInf.Qc);
			if (qc2 != null) qc2.Hidden = false;
			return;
		}

		internal void UnhideAllColumns()
		{
			if (Grid.BGV == null) return;
			BandedGridView bgv = Grid.BGV;
			foreach (GridBand gb in bgv.Bands)
			{
				gb.Visible = true;
			}

			foreach (BandedGridColumn bgc in bgv.Columns)
			{
				bgc.Visible = true;
				SetQcVisible(bgc, true);
			}
		}

		/// <summary>
		/// Hide rows
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void HideRowsMenuItem_Click(object sender, EventArgs e)
		{
			if (Dtm is null)
				return;

			if (Dtm.HasMarkedRow())
			{
				HideRows();
			}
			else
			{
				MessageBoxMx.Show(
				"You must mark at least one row before hiding marked rows.",
				UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		internal void HideRows()
		{
			Grid.ShowUnmarkedRowsOnly = true;
			Grid.V.RefreshData();  //Triggers PrototypeGridView_CustomRowFilter()
		}

		/// <summary>
		/// Unhide rows
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void UnhideRowsMenuItem_Click(object sender, EventArgs e)
		{
			UnhideRows();
		}

		internal void UnhideRows()
		{
			Grid.ShowUnmarkedRowsOnly = false;
			Grid.V.RefreshData();  //Triggers PrototypeGridView_CustomRowFilter()
		}

		internal void UnhideAllRows()
		{
			// todo
		}

		/// <summary>
		/// Insert row
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void RowMenuInsertRowMenuItem_Click(object sender, EventArgs e)
		{
			InsertRowMenuItem_Click(sender, e);
		}

		/// <summary>
		/// Delete row
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void RowMenuDeleteRowMenuItem_Click(object sender, EventArgs e)
		{
			DeleteRowMenuItem_Click(sender, e);
		}

		/// <summary>
		/// Start drag of a column or band header has completed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void BandedGridView_DragObjectStart(object sender, DragObjectStartEventArgs e)
		{
			if (e.DragObject is GridBand)
			{ // moving a table
				GridBand gb = e.DragObject as GridBand;
				DragStartIdx = gb.Index;
				if (DragStartIdx <= 1) e.Allow = false; // can't move 1st band
			}

			else if (e.DragObject is BandedGridColumn)
			{ // moving a column within a table
				BandedGridColumn gc = e.DragObject as BandedGridColumn;
				DragStartIdx = gc.ColIndex;
				if (gc.OwnerBand.Index <= 1 && DragStartIdx == 0) e.Allow = false; // can't move 1st band in 1st col
			}

			return;
		}

		/// <summary>
		/// Continue drag of a column or band header has completed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void BandedGridView_DragObjectOver(object sender, DragObjectOverEventArgs e)
		{
			int i2;
			if (e.DragObject is GridBand && e.DropInfo is BandPositionInfo)
			{ // moving a table
				GridBand gb = e.DragObject as GridBand;
				i2 = gb.Index;

				BandPositionInfo pi = e.DropInfo as BandPositionInfo;
				if (pi.Index <= 1) pi.Valid = false;
			}

			else if (e.DragObject is BandedGridColumn && e.DropInfo is BandedColumnPositionInfo)
			{ // moving a column within a table
				BandedGridColumn gc = e.DragObject as BandedGridColumn;
				i2 = gc.ColIndex;
				BandedColumnPositionInfo pi = e.DropInfo as BandedColumnPositionInfo;
				if (pi.Index < 0 || pi.Band == null) pi.Valid = false;
				else if (pi.Band.Index <= 1 && pi.Index == 0) pi.Valid = false;
			}

			return;
		}

		/// <summary>
		/// Drag of a column or band header has completed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void BandedGridView_DragObjectDrop(object sender, DragObjectDropEventArgs e)
		{

			// Moving a table

			if (e.DragObject is GridBand && e.DropInfo is BandPositionInfo)
			{
				GridBand gb = e.DragObject as GridBand; // get QueryTable being dragged
				QueryTable qt = gb.Tag as QueryTable;
				if (qt == null) return;

				int ci1 = DragStartIdx; // initial band position
				int ci2 = gb.Index; // new band position
				if (ci1 == ci2) return; // no move
				if (ci2 > ci1) ci2--; // moving to right, index of col we want to move to right of
				else ci2++; // moving col to left, index of col we want to move to the left of 

				GridBand gb2 = Grid.BGV.Bands[ci2];
				QueryTable qt2 = gb2.Tag as QueryTable; // and assoc query table
				if (qt2 == null) return;

				Query q0 = BaseQuery; // map to base query position which may differ
				int q0ti1 = q0.GetQueryTableIndexByName(qt.MetaTable.Name);
				int q0ti2 = q0.GetQueryTableIndexByName(qt2.MetaTable.Name);
				if (q0ti1 < 0 || q0ti2 < 0) return;

				q0.RemoveQueryTableAt(q0ti1); // remove from original location
				q0.InsertQueryTable(q0ti2, qt); // reinsert at new position
				return;
			}

			// Moving a column within a table

			else if (e.DragObject is BandedGridColumn && e.DropInfo is BandedColumnPositionInfo)
			{ // grid columns are already rearranged at this point
				BandedColumnPositionInfo cp = e.DropInfo as BandedColumnPositionInfo;
				if (cp == null || cp.Band == null || cp.Index < 0) return;

				BandedGridColumn gc = e.DragObject as BandedGridColumn;
				GridBand band = gc.OwnerBand;

				CellInfo cinf = new CellInfo();
				Grid.GetColumnInfo(gc, cinf);
				QueryTable qt = cinf.Qt;
				int qci1 = qt.GetQueryColumnIndexByName(cinf.Mc.Name); // assoc qc index
				if (qci1 <= 0 || qci1 >= qt.QueryColumns.Count) return; // must be within range

				int ci1 = DragStartIdx; // where column started
				int ci2 = gc.ColIndex; // where we're going in grid
				if (ci1 == ci2) return; // no move
				if (ci2 > ci1) ci2--; // moving to right, index of col we want to move to right of
				else ci2++; // moving col to left, index of col we want to move to the left of 

				// Move the QueryColumn to new position in QueryTable

				BandedGridColumn gc2 = band.Columns[ci2]; // column we're going to
				CellInfo cinf2 = new CellInfo();
				Grid.GetColumnInfo(gc2, cinf2);
				int qci2 = qt.GetQueryColumnIndexByName(cinf2.Mc.Name); // assoc qc index
				if (qci2 <= 0 || qci2 >= qt.QueryColumns.Count) return; // must be within range

				QueryColumn qc = qt.QueryColumns[qci1]; // get the QueryColumn that is moving
				qt.QueryColumns.RemoveAt(qci1); // move it within the QueryTable
				qt.QueryColumns.Insert(qci2, qc);

				// Move the ResultsField to new position in ResultsTable (really need to do?)

				ResultsTable rt = cinf.Rt;
				int rfi1 = rt.GetResultsFieldIndexByName(cinf.Mc.Name); // assoc rf index
				int rfi2 = rt.GetResultsFieldIndexByName(cinf2.Mc.Name); // index of col going to

				ResultsField rf = rt.Fields[rfi1]; // get the ResultsField that is moving
				rt.Fields.RemoveAt(rfi1); // move it within the ResultsTable
				rt.Fields.Insert(rfi2, rf);

				return;
			}

			return;
		}

		private void ColumnFormatMenuItem_Click(object sender, System.EventArgs e)
		{
			SessionManager.LogCommandUsage("GridColumnFormat");
			SetColumnFormat();
		}

		internal void SetColumnFormat()
		{
			CellInfo ci = Grid.LastMouseDownCellInfo;
			if (ci == null || ci.Rfld == null)
			{
				MessageBoxMx.Show(
						"You must first select the column that you\r\n" +
						"want to define the format for.",
						UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			QueryColumn qc = ci.Qc;
			DialogResult dr = ColumnFormatDialog.Show(qc);
			if (dr == DialogResult.Cancel) return;

			MetaColumn mc = qc.MetaColumn;
			if (mc != null)
			{
				if (mc.IsNumeric)
				{
					BaseQueryQc(qc).Decimals = qc.Decimals; // be sure base query is in synch
					BaseQueryQc(qc).DisplayFormat = qc.DisplayFormat;
				}

				else if (mc.DataType == MetaColumnType.String)
				{
					Grid.SetColumnEditRepositoryItem(ci.GridColumn, qc); // update grid column repository item to correct type
				}

				else if (mc.DataType == MetaColumnType.Structure)
				{
					ResultsFormatter fmtr = Qm.ResultsFormatter;
					if (fmtr != null)
						fmtr.StructureHighlightingInitialized = false; // reset structure hilighting
				}
			}

			if (Grid.Editor != null) // if editing definition then update underlying metacolumn
				Grid.Editor.UpdateMetaColumnFormatting(qc);

			Qm.DataTableManager.ResetFormattedValues(qc);
			Grid.V.RefreshData(); // just a Refresh() isn't enough in some cases (Grid.RefreshDataSource() also works)
			return;
		}

		private void TextAlignmentMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("GridColumnTextAlignment");
			SetTextAlignment();
		}

		internal void SetTextAlignment()
		{
			CellInfo ci = Grid.LastMouseDownCellInfo;
			if (ci == null || ci.Rfld == null)
			{
				MessageBoxMx.Show(
						"You must first select the column that you\r\n" +
						"want to define the text alignment for.",
						UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			QueryColumn qc = ci.Qc;
			DialogResult dr = TextAlignmentDialog.Show(qc);
			if (dr == DialogResult.Cancel) return;

			BaseQueryQc(qc).HorizontalAlignment = qc.HorizontalAlignment; // be sure base query is in synch
			BaseQueryQc(qc).VerticalAlignment = qc.VerticalAlignment;

			Grid.InSetup = true;

			Grid.SetColumnAlignment(ci.GridColumn, ci.Qc);

			if (TextAlignmentDialog.Instance.ApplyToAllColumns.Checked)
			{ // apply to all QueryColumns in query
				foreach (QueryTable qt0 in Dtm.Query.Tables)
				{
					foreach (QueryColumn qc0 in qt0.QueryColumns)
					{
						if (qc0.VoPosition >= 0 && qc0.VoPosition < Grid.DataTableToGridColumnMap.Length)
						{
							int dataColIndex = qc0.VoPosition;
							int gridColAbsoluteIndex = Grid.DataTableToGridColumnMap[dataColIndex];
							GridColumn gc = Grid.V.Columns[gridColAbsoluteIndex];
							Grid.SetColumnAlignment(gc, qc0);
						}

						BaseQueryQc(qc0).HorizontalAlignment = qc0.HorizontalAlignment; // be sure base query is in synch
						BaseQueryQc(qc0).VerticalAlignment = qc0.VerticalAlignment;
					}
				}
			}

			Grid.InSetup = false;

			return;
		}

		/// <summary>
		/// Attempt to get the current untransformed base query
		/// </summary>
		/// <returns></returns>

		internal Query BaseQuery
		{
			get
			{
				return QueriesControl.BaseQuery;
			}
		}

		/// <summary>
		/// Attempt to get the corresponding QueryTable in the base, untransformed query
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		internal QueryTable BaseQueryQt(QueryTable qt)
		{
			return QueriesControl.BaseQueryQt(qt);
		}

		/// <summary>
		/// Attempt to get the corresponding QueryColumn in the base, untransformed query
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		internal QueryColumn BaseQueryQc(QueryColumn qc)
		{
			return QueriesControl.BaseQueryQc(qc);
		}

		private void TableHeaderBackgroundColorMenuItem_Click(object sender, System.EventArgs e)
		{ // (currently disabled)
			SessionManager.LogCommandUsage("GridTableHeaderBackgroundColor");
			SetTableHeaderBackgroundColor();
		}

		internal void SetTableHeaderBackgroundColor()
		{
			CellInfo ci = Grid.LastMouseDownCellInfo;
			if (ci == null || ci.Rfld == null)
			{
				MessageBoxMx.Show(
						"You must first select the column that you\r\n" +
						"want to define a background color.",
						UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			DialogResult dr = QueryTableControl.TableHeaderBackgroundColorDialog_Show(ci.Qc.QueryTable);
			if (dr == DialogResult.OK) RestartDisplayWithNewHeaders();
			return;
		}


		private void SortAscendingMenuItem2_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("GridColumnSortAscending");
			SortOnCurrentColumn(SortOrder.Ascending);
		}

		private void SortDescendingMenuItem2_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("GridColumnSortDescending");
			SortOnCurrentColumn(SortOrder.Descending);
		}

		private void SortMultipleMenuItem2_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("GridColumnSortMultiple");
			SortOnCurrentColumn(SortOrder.Ascending);
		}

		private void ColumnWidthMenuItem2_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("GridColumnWidth");
			SetColumnWidth();
		}

		private void ColumnFormatMenuItem2_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("GridColumnFormat");
			SetColumnFormat();
		}

		private void CidViewStructureInNewWindow_Click(object sender, System.EventArgs e)
		{
			//string cid = GetCidFromSelectedCidCell();
			//if (Lex.IsUndefined(cid)) return;

			SessionManager.LogCommandUsage("CidViewStructureInNewWindow");
			CommandExec.ExecuteCommandAsynch("SelectCompoundStructure " + Grid.SelectedCid);
		}
		private void SelectAllCompoundData_Click(object sender, System.EventArgs e)
		{
			SessionManager.LogCommandUsage("GridCidSelectAllData");
			//			if (SelectAllCompoundData.DropDownItems.Count == 0) // send command if no submenu
			CommandExec.ExecuteCommandAsynch("SelectAllCompoundData " + Grid.SelectedCid);
		}

		private void ShowStbViewForCompound_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("GridShowStbViewForCompound");
			CommandLine.Execute("TargetResultsViewer ShowPopup " + Grid.SelectedCid);
		}

		private void ShowTargetDescriptionMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("GridTargetShowDescription");
			CommandExec.ExecuteCommandAsynch("ShowTargetDescription " + Grid.SelectedTarget);
		}

		private void ShowTargetAssayListMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("GridTargetShowAssayList");
			CommandExec.ExecuteCommandAsynch("ShowTargetAssayList " + Grid.SelectedTarget);
		}

		private void AllDataForTargetMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ShowAllTargetUnsummarizedAssayData");
			CommandExec.ExecuteCommandAsynch("ShowAllTargetUnsummarizedAssayData " + Grid.SelectedTarget);
		}

		//private void ShowTargetDataByAssayMenuItem_Click(object sender, EventArgs e)
		//{
		//  SessionManager.LogCommandUsage("GridShowTargetDataByAssay");
		//  CommandExec.Execute("ShowTargetDataByAssay " + Grid.SelectedTarget);
		//}

		//private void ShowTargetDataByTargetMenuItem_Click(object sender, EventArgs e)
		//{
		//  SessionManager.LogCommandUsage("GridShowTargetDataByTarget");
		//  CommandExec.Execute("ShowTargetDataByTarget " + Grid.SelectedTarget);
		//}

		/// <summary>
		/// Grid received focus
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		public void MoleculeGridControl_GotFocus(object sender, EventArgs e)
		{
			return;
		}

		/// <summary>
		/// Grid lost focus
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		public void MoleculeGridControl_LostFocus(object sender, EventArgs e)
		{
			return;
		}

		/// <summary>
		/// Set any cell-specific appearance attributes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void PrototypeGridView_RowCellStyle(object sender, RowCellStyleEventArgs e)
		{
			DataRowMx dr = null;
			object fieldValue = null;

			//if (Grid.V.Columns.Count > 5) e = e; // debug

			if (Grid.DataSource == null) return;

			CellInfo ci = Grid.GetGridCellInfo(e.RowHandle, e.Column);
			QueryManager qm = Grid.QueryManager;

			if (e.RowHandle < 0 || ci.DataRowIndex < 0) return;

			//			if (e.RowHandle >= 0) // row exist in DataTable (i.e. not new row)

			AdjustDataRowToRender(ci);

			if (!Grid.Focused && e.RowHandle == Grid.NotFocusedRowToHighlight && e.Column.AbsoluteIndex == Grid.NotFocusedColToHighlight)
			{ // show focused row when grid doesn't have the focus
				GridCell[] cells = Grid.GetSelectedCells();
				int rh = Grid.V.FocusedRowHandle;
				e.Appearance.BackColor = Color.CornflowerBlue;
				return;
			}

			if (ci.DataValue is MobiusDataType)
			{
				MobiusDataType mdt = (MobiusDataType)ci.DataValue;
				e.Appearance.ForeColor = mdt.ForeColor;

				CondFormatStyle cfcs = (ci.Qc != null ? ci.Qc.CondFormatColoringStyle : CondFormatStyle.Undefined);

				if (mdt.BackColor != Color.Empty && cfcs != CondFormatStyle.DataBar) // back color defined and not data bar (e.g. non-zero alpha)
					e.Appearance.BackColor = mdt.BackColor;
				if (mdt.Hyperlinked) ////!String.IsNullOrEmpty(mdt.Hyperlink))
				{
					if (mdt.ForeColor == Color.Black)
						e.Appearance.ForeColor = Color.Blue;
					else e.Appearance.ForeColor = mdt.ForeColor;

					e.Appearance.Font = new Font(e.Appearance.Font, FontStyle.Underline);
				}
			}

			else if (ci.DataValue is Color) // simple color-containing cells
			{
				e.Appearance.BackColor = (Color)ci.DataValue;
			}

			else return; // null or other type
		}

		/// <summary>
		/// Adjust the datarow to render
		/// </summary>
		/// <param name="ci"></param>

		internal void AdjustDataRowToRender(
						CellInfo ci)
		{
			int dri2 = -1;

			//ClientLog.Message("AdjustDataRowToRender " + ci.GridRowHandle + ", " + ci.DataRowIndex);

			//			if (Rf.Tables.Count <= 1 || ci.TableIndex > 0 || // must be column in master table
			if (ci.Rfld == null || Dtm.RowAttributesVoPos < 0) // must have necessary meta info
				return;

			if (ci.DataRowIndex >= DataTable.Rows.Count) return;
			DataRowMx dr = DataTable.Rows[ci.DataRowIndex];
			DataRowAttributes dra = Dtm.GetRowAttributes(dr);
			if (dra == null) return;

			// Normal case just returning rows that are not filtered out

			if (!Grid.ShowMarkedRowsOnly && !Grid.ShowSelectedRowsOnly) // just returning unfiltered rows
			{
				if (ci.Mc.IsKey && // key column value is normally available & comes from master row
						dra.FirstRowForKey >= 0 && Dtm.Query.Tables.Count > 1) // don't use if row not defined or only one table (e.g. annotation table editor)
					dri2 = dra.FirstRowForKey;

				else
				{
					if (dra.SubRowPos == null) return; // just return if no indexes for match
					int ti = ci.TableIndex;
					if (ti < 0 || ti >= dra.SubRowPos.Length) return; // out of range
					dri2 = dra.SubRowPos[ti];
					//ClientLog.Message("AdjustDataRowToRender Table: " + ci.TableIndex + ", Row " + ci.DataRowIndex + " -> " + dri2);
					if (dri2 == ci.DataRowIndex) return; // on the same row
				}
			}

			// Consider selected or marked rows only (e.g. displaying selected rows in a chart view)

			else
			{
				int ri = ci.DataRowIndex;

				if (ci.Mt.IsRootTable) // if column is from the root table scan back to see if this row should be paired with the root table row
				{
					string curKeyVal = dr[Dtm.KeyValueVoPos] as string;

					//if (ci.DataRowIndex == 2) ci = ci; // debug

					for (int dri0 = ri; dri0 >= 0; dri0--)
					{
						DataRowMx dr2 = DataTable.Rows[dri0];
						DataRowAttributes dra2 = Dtm.GetRowAttributes(dr2);
						string keyVal = dr2[Dtm.KeyValueVoPos] as string;
						if (keyVal != curKeyVal) break; // no luck if out of rows for key
						if (Grid.ShowMarkedRowsOnly && dri0 < ci.DataRowIndex && Dtm.RowIsMarked(dri0)) break; // no luck if earlier row was marked
						if (Grid.ShowSelectedRowsOnly && dri0 < ci.DataRowIndex && dra2.Selected) break; // no luck if earlier row was selected
						if (dri0 == dra.FirstRowForKey) // if at 1st row for key then it's ours
						{
							dri2 = dri0;
							break;
						}
					}
				}

				else // not root see if other row should be used instead
				{
					int ti = ci.Qc.QueryTable.TableIndex; // get table corresponding to specified column
					dri2 = Dtm.AdjustDataRowToCurrentDataForTable(ri, ti, true); // get the actual row for this table associated with this base row
				}
			}

			if (dri2 == DataTableManager.NullRowIndex) // null row, return null value
			{
				ci.DataRowIndex = dri2;
				ci.DataValue = null;
			}

			else if (dri2 >= 0) // if master-table row found use its value
			{
				ci.DataRowIndex = dri2;
				dr = DataTable.Rows[dri2];
				if (dr != null && ci.DataColIndex < dr.Length)
					ci.DataValue = dr[ci.DataColIndex];

				else DebugLog.Message("Invalid DataRow or Column Index");
			}

			//ClientLog.Message("AdjustDataRowToRender, ci.DataRowIndex = " + ci.DataRowIndex + ", dri2 = " + dri2); // debug
			//if (dri2 == 1) dri2 = dri2; // debug
			//if (dri2 != ci.DataRowIndex) dri2 = dri2; // debug

			return;
		}

		/// <summary>
		/// Draw coloring in cond format band headers if needed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void BandedGridView_CustomDrawBandHeader(object sender, DevExpress.XtraGrid.Views.BandedGrid.BandHeaderCustomDrawEventArgs e)
		{
			if (e.Band == null || !e.Band.AppearanceHeader.Options.UseBackColor)
				return;

			Rectangle r = new Rectangle(e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);

			Color backColor = e.Band.AppearanceHeader.BackColor;
			Brush brush = new SolidBrush(e.Band.AppearanceHeader.BackColor);
			e.Graphics.FillRectangle(brush, r);

			r.X--; // draw a border
			if (e.Band.ParentBand == null) r.Height--; // show top, bottom & rt line
			else r.Y--; // show botton & rt line
			Pen pen = new Pen(Color.CornflowerBlue);
			e.Graphics.DrawRectangle(pen, r);

			StringFormat sf = new StringFormat(); // center text
			sf.Alignment = StringAlignment.Center;
			sf.LineAlignment = StringAlignment.Far;
			Brush foreBrush = new SolidBrush(e.Band.AppearanceHeader.ForeColor);
			r.Height -= 3; // move up a bit from bottom
			e.Appearance.DrawString(e.Cache, e.Band.Caption, r, /* font, */ foreBrush, sf);
			e.Handled = true;

			return;
		}

		/// <summary>
		/// Custom drawing of column headers containing structures
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void BandedGridView_CustomDrawColumnHeader(object sender, ColumnHeaderCustomDrawEventArgs e)
		{
			if (e.Column == null) return;
			ColumnInfo ci = Grid.GetColumnInfo(e.Column);
			if (ci == null || ci.Qc == null) return;
			if (!(ci.Rfld.Header is MoleculeMx)) return;

			e.Painter.DrawObject(e.Info); // draw the standard header
			e.Handled = true;

			MoleculeMx mol = ci.Rfld.Header as MoleculeMx;
			if (MoleculeMx.IsUndefined(mol)) return;

			MolLib1.MoleculeControl hr = new MolLib1.MoleculeControl();
			hr.Preferences = mol.GetDisplayPreferences();
			hr.Preferences.BackColor = Color.Transparent;
			hr.Preferences.HydrogenDisplayMode = HydrogenDisplayMode.Off;

			Size size = new Size(e.Bounds.Width, e.Bounds.Height);
			double sbl = hr.Preferences.StandardBondLength;
			int width = e.Column.Width;
			if (width < 166) // get better separation of atoms for small structures in narrow columns
				hr.Preferences.StandardBondLength = sbl * 166 * 1.5 / width;
			Point location = e.Info.TopLeft;
			Bitmap bm = hr.PaintMolecule(mol.GetMolfileString(), StructureType.MolFile, size.Width, size.Height);

			e.Graphics.DrawImage(bm, e.Bounds);

			return;
		}

		/// <summary>
		/// CustomDrawRowIndicator prototype event, gets copied to actual view at initialization
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void PrototypeGridView_CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
		{
			if (!Grid.AutoNumberRows) return;
			if (!e.Info.IsRowIndicator) return;
			if (e.RowHandle < 0)
				return; // may be NewItemRow indicator at initialization

			e.Info.DisplayText = (e.RowHandle + 1).ToString();
			e.Info.ImageIndex = -1; // remove any image that would overlay the row number
		}

		/// <summary>
		/// Set cell text just before display
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void PrototypeGridView_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
		{
			//if (e.Column.VisibleIndex != 2) return;
			//e.DisplayText = "Custom!";
			return; // not currently used
		}

		/// <summary>
		/// Do custom drawing of cell
		/// Currently to display data bar conditional formatting
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void PrototypeGridView_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
		{
			MobiusDataType mdt = null;
			Color c = Color.LightGreen;
			int bandWidthPct = 0;

			if (DebugMx.False) return; // turn off for now

			//if (Grid.V.Columns.Count > 5) e = e; // debug

			if (Grid.DataSource == null) return;

			CellInfo ci = Grid.GetGridCellInfo(e.RowHandle, e.Column);
			if (ci == null || ci.Qc == null) return;

			if (ci.Qc.CondFormatColoringStyle != CondFormatStyle.DataBar &&
					ci.Qc.CondFormatColoringStyle != CondFormatStyle.IconSet) return;

			//if (Lex.Eq(ci.Mc.Name, "MLCLR_WGT")) ci = ci; // debug
			if (e.RowHandle < 0 || ci.DataRowIndex < 0) return;

			AdjustDataRowToRender(ci);

			if (!(ci.DataValue is MobiusDataType)) return;

			mdt = (MobiusDataType)ci.DataValue;
			c = mdt.BackColor;
			if (c.IsEmpty) return;

			// Draw with data bar

			if (ci.Qc.CondFormatColoringStyle == CondFormatStyle.DataBar)
			{
				bandWidthPct = c.A; // percent of band size

				if (bandWidthPct == 0 || bandWidthPct > 100) return; // cond format range from 0 - 100 % for data band cond formatting
																														 //if (bandWidthPct > 100) bandWidthPct = 100; // cond format range from 0 - 100 % for data band cond formatting

				// Adjust bounding rectange to properly show data band

				Rectangle r = e.Bounds;
				int bw = (int)(r.Width * (bandWidthPct / 100.0));

				Rectangle r2 = new Rectangle(r.X, r.Y, bw, r.Height);

				c = Color.FromArgb(c.R, c.G, c.B); // build opaque color
				e.Cache.FillRectangle(c, r2); // draw the band

				if (mdt != null && mdt.Hyperlinked) ////!String.IsNullOrEmpty(mdt.Hyperlink))
				{ // todo: fix this for custom draw
					if (mdt.ForeColor == Color.Black)
						e.Appearance.ForeColor = Color.Blue;
					else e.Appearance.ForeColor = mdt.ForeColor;

					e.Appearance.Font = new Font(e.Appearance.Font, FontStyle.Underline);
				}

				e.Appearance.DrawString(e.Cache, e.DisplayText, e.Bounds);
				e.Handled = true;
			}

			// Draw with icon

			else if (ci.Qc.CondFormatColoringStyle == CondFormatStyle.IconSet)
			{
				GridCellInfo cellInfo = e.Cell as GridCellInfo;

				TextEditViewInfo info = cellInfo.ViewInfo as TextEditViewInfo;
				if (info == null) return;

				c = mdt.BackColor;
				int imgIdx = c.A; // alpha contains image index
				ImageCollection il = Bitmaps.I.IconSetImages;
				if (imgIdx >= il.Images.Count) return;
				info.ContextImage = il.Images[imgIdx];
				info.CalcViewInfo();
				return;
			}

			return;
		}

		/// <summary>
		/// Showing editor, if new row then make it a real row before starting the editing.
		/// If structure column then invoke structure editor
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void PrototypeGridView_ShowingEditor(object sender, CancelEventArgs e)
		{
			int gr;
			if (Grid.BGV != null && Grid.BGV.FocusedRowHandle == GridControl.NewItemRowHandle)
				gr = Grid.Helpers.MakeGridRowReal(GridControl.NewItemRowHandle);

			CellInfo ci = Grid.LastMouseDownCellInfo;
			QueryColumn qc = ci?.Qc;

			MetaColumnType colType = Grid.GetMetaColumnDataType(Grid.Col);
			if (colType == MetaColumnType.Structure && Grid.V.Columns[Grid.Col].OptionsColumn.AllowEdit)
			{ // edit structure on double click if allowed
				e.Cancel = true;
				EditMolecule(Grid.Row, Grid.Col, MoleculeRendererType.Unknown);
				return;
			}

			//else if (qc != null && qc.IsHtmlContent) // (Can't click on hyperlinks if we do this)
			//{
			//	e.Cancel = true;
			//	Grid.EditHtmlTextCellValue(ci);
			//}

		}

		/// <summary>
		/// Show the filter 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void PrototypeGridView_ShowFilterPopupListBox(object sender, FilterPopupListBoxEventArgs e)
		{ // Mobius handles this in the MouseDown event
			e.ComboBox.Items.Clear(); // clear items so standard XtraGrid dialog not displayed
		}

		/// <summary>
		/// Apply filters to row to determine visibility 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void PrototypeGridView_CustomRowFilter(object sender, RowFilterEventArgs e)
		{
			//			SystemUtil.Beep();

			//if (MoleculeTable.FilteredColumnList == null || MoleculeTable.FilteredColumnList.Count == 0 ||
			//  !MoleculeTable.FiltersEnabled)
			//{ // just return if no column filters in effect
			//  e.Visible = true;
			//  e.Handled = true;
			//  return;
			//}            

			if (Dtm == null || DataTable == null) return;

			if (Dtm.RowAttributesVoPos < 0) return; // can't do if no attributes in table
			 
			//if (e.ListSourceRow == 0) e = e; // debug

			DataRowMx dr = DataTable.Rows[e.ListSourceRow];
			DataRowAttributes dra = Dtm.GetRowAttributes(dr);
			if (dra == null) e.Visible = true;

			else if (dra.Filtered) // hide if filtered
				e.Visible = false;

			else if (Grid.ShowUnmarkedRowsOnly && Dtm.RowIsMarked(dr) && // show unmarked only hiding
			 Dtm.HasUnmarkedRow()) // At least one row must remain unmarked when hiding marked rows.                    
				e.Visible = false;

			else if (Grid.ShowMarkedRowsOnly && !Dtm.RowIsMarked(dr)) // show marked only hiding (not active)
				e.Visible = false;

			else if (Grid.ShowSelectedRowsOnly && !dra.Selected) // show selected only hiding (not active)
				e.Visible = false;

			else e.Visible = true; // if none of the above is true show the row

			//ClientLog.Message("CustomRowFilter Row=" + e.ListSourceRow + ", Visible=" + e.Visible); // debug

			e.Handled = true;
			return;
		}

		/// <summary>
		/// Clicked in cell, if new row make it real
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void PrototypeGridView_RowCellClick(object sender, RowCellClickEventArgs e)
		{
			if (e.RowHandle == GridControl.NewItemRowHandle)
			{
				MakeGridRowReal(e.RowHandle);
			}
		}

		/// <summary>
		/// Show filter dialog
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void FilterMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("GridColumnFilter");

			GridColumn gc = Grid.V.Columns[Grid.LastMouseDownCol];
			DialogResult dr = Dtm.CompleteRetrieval(); // be sure we have full data before filtering
			if (dr == DialogResult.Cancel) return;

			FilterDialog.Edit(gc);
		}

		/// <summary>
		/// Sort the grid & redisplay
		/// </summary>
		/// <param name="sCols"></param>

		internal void SortGrid(List<SortColumn> sCols)
		{
			if (Qe != null && Qe.State == QueryEngineState.Retrieving)
				Dtm.CompleteRetrieval(); // Complete retrieval of rows

			Grid.BeginUpdate(); // set the sort info in the 
			Grid.Dtm.Sort(sCols); // sort it

			Rf.ClearSortOrder(); // store sort col info Rf
			for (int sci = 0; sci < sCols.Count; sci++)
			{
				foreach (ResultsTable rt in Rf.Tables)
				{
					foreach (ResultsField rf in rt.Fields)
					{
						if (rf.QueryColumn == sCols[sci].QueryColumn)
						{
							if (sCols[sci].Direction == SortOrder.Ascending)
								rf.SortOrder = sci + 1;
							else rf.SortOrder = -(sci + 1);
						}
					}
				}

			}

			Grid.EndUpdate();

			Grid.V.ClearSelection();
			Grid.SetHeaderGlyphs();
			Grid.ForceRefilter(); // must refilter

			Qrc.UpdateFilteredViews(); // update all views to reflect new order of rows and associated filtering

			return;
		}

#if false
		/// <summary>
		/// Process QuickDisplay command & update control with associated structure
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="command"></param>
		/// <returns></returns>
		/// 
		public bool QuickDisplay(
				MetaTable mt,
				string command)
		{
			string cid, extCid, tok, tok2;
			MoleculeMx cs = null;
			int relatedCount = 0; // add related lookup later

			string[] toks = command.Split(' ');
			if (toks.Length < 2) return false;
			tok = toks[1];
			if (toks.Length >= 3) tok2 = toks[2]; // FromList
			else tok2 = "";

			cid = CompoundId.Normalize(tok, mt);
			extCid = CompoundId.Format(cid, mt);

			cs = MoleculeUtil.SelectMoleculeForCid(cid, mt);
			if (cs != null)
			{
				MoleculeMx.SetRenderStructure(BrowseStructurePopup, cs);
				return true;
			}

			else return false;
		}
#endif

		// MarkedDataContextMenu Clicks

		internal void ShowMarkedDataContextMenu(
				Control control,
				Point position)
		{
			ShowMarkedDataContextMenu(control, position, null, null, null, null);
		}

		internal void ShowMarkedDataContextMenu(
				Control control,
				Point position,
				EventDelegate addSelectedToMarkedMenuItemDelegate,
				EventDelegate removeSelectedFromMarkedMenuItemDelegate,
				EventDelegate addMarkedToSelectedMenuItemDelegate,
				EventDelegate removeMarkedFromSelectedMenuItemDelegate)
		{
			bool s1 = SetDelegate(addSelectedToMarkedMenuItemDelegate, out ED1, AddSelectedToMarkedMenuItem);
			bool s2 = SetDelegate(removeSelectedFromMarkedMenuItemDelegate, out ED2, RemoveSelectedFromMarkedMenuItem);
			bool s3 = SetDelegate(addMarkedToSelectedMenuItemDelegate, out ED3, AddMarkedToSelectedMenuItem);
			bool s4 = SetDelegate(removeMarkedFromSelectedMenuItemDelegate, out ED4, RemoveMarkedFromSelectedMenuItem);
			SelectedAndMarkedDividerMenuItem.Visible = s1 | s2 | s3 | s4;
			MarkedDataContextMenu.Show(control, position);
		}

		bool SetDelegate(EventDelegate d1, out EventDelegate d2, ToolStripMenuItem mi)
		{
			d2 = d1;
			bool defined = (d1 != null);
			mi.Visible = defined;
			return defined;
		}

		internal void MarkAllMenuItem_Click(object sender, EventArgs e)
		{
			Grid.MarkAllItems(true, true);
			ResultsViewProps view = Qrc.CurrentView; // get any other view that might need refreshing
			if (view != null) view.Refresh();
		}

		internal void MarkNoneMenuItem_Click(object sender, EventArgs e)
		{
			Grid.MarkAllItems(true, false);
			ResultsViewProps view = Qrc.CurrentView; // get any other view that might need refreshing
			if (view != null) view.Refresh();
		}

		private void AddSelectedToMarkedMenuItem_Click(object sender, EventArgs e)
		{
			if (ED1 != null) ED1(sender, e);
		}

		private void RemoveSelectedFromMarkedMenuItem_Click(object sender, EventArgs e)
		{
			if (ED2 != null) ED2(sender, e);
		}

		private void AddMarkedToSelectedMenuItem_Click(object sender, EventArgs e)
		{
			if (ED3 != null) ED3(sender, e);
		}

		private void RemoveMarkedFromSelectedMenuItem_Click(object sender, EventArgs e)
		{
			if (ED4 != null) ED4(sender, e);
		}

		private void SaveMarkedMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			MainMenuControl.SetupTempListMenu(SaveMarkedMenuItem.DropDownItems, SaveMarkedCurrentMenuItem_Click);
		}

		private void SavedListMenuItem_Click(object sender, EventArgs e)
		{ // save to persisted list
			Grid.SaveMarkedGridItems(false, null);
		}

		private void SaveMarkedCurrentMenuItem_Click(object sender, EventArgs e)
		{ // save to existing temp list
			ToolStripMenuItem mi = sender as ToolStripMenuItem;
			TempCidList tcl = mi.Tag as TempCidList;
			Grid.SaveMarkedGridItems(true, tcl.Name);
		}

		private void SaveMarkedNewTempListMenuItem_Click(object sender, EventArgs e)
		{ // save to new temp list
			Grid.SaveMarkedGridItems(true, null);
		}

		private void MarkedItemsAddToListMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			MainMenuControl.SetupTempListMenu(MarkedItemsAddToListMenuItem.DropDownItems, AddMarkedCurrentMenuItem_Click);
		}

		private void AddMarkedToSavedListMenuItem_Click(object sender, EventArgs e)
		{ // add to saved list
			Grid.AddMarkedGridItemsToList(null);
		}

		private void AddMarkedCurrentMenuItem_Click(object sender, EventArgs e)
		{ // add to temp list
			ToolStripMenuItem mi = sender as ToolStripMenuItem;
			TempCidList tcl = mi.Tag as TempCidList;
			Grid.AddMarkedGridItemsToList(tcl.Name);
		}

		private void AddMarkedToNewTemporaryListMenuItem_Click(object sender, EventArgs e)
		{ // add to new temp list
			SaveMarkedNewTempListMenuItem_Click(sender, e);
		}

		private void MarkedItemsRemoveFromListMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			MainMenuControl.SetupTempListMenu(MarkedItemsRemoveFromListMenuItem.DropDownItems, RemoveMarkedCurrentMenuItem_Click);
		}

		private void RemoveMarkedCurrentMenuItem_Click(object sender, EventArgs e)
		{ // remove from temp list
			ToolStripMenuItem mi = sender as ToolStripMenuItem;
			TempCidList tcl = mi.Tag as TempCidList;
			Grid.RemoveMarkedGridItemsFromList(tcl.Name);
		}

		private void RemoveMarkedMenuItem_Click(object sender, EventArgs e)
		{ // remove from saved list
			Grid.RemoveMarkedGridItemsFromList(null);
		}

		private void CidViewRelatedStructures_Click(object sender, EventArgs e)
		{
			ViewRelatedCompoundData();
		}

		private void ViewRelatedStructuresMenuItem_Click(object sender, EventArgs e)
		{
			ViewRelatedCompoundData();
		}

		private void ViewRelatedCompoundData()
		{
			string cid;
			RootTable rt;

			if (!GetCidAndRootTableForCurrentCell(out cid, out rt)) return;

			bool showRelatedCompoundsDialog = !IsSmallWorldOnly(rt);

			if (showRelatedCompoundsDialog)
			{ 
				//Query q = QbUtil.Query;
				QueryManager qm = QbUtil.QueryManager; // pass the current base query
				//if (q == null || q.Mode != QueryMode.Browse)
				//	qm = null;

				RelatedCompoundsDialog.Show(cid, qm);
			}

			else
				SmallWorldDepictions.OpenUrlFromSmallWorldCid(cid);

			return;
		}

		private void ShowRegistrationHistoryCorpIdChangesForThisCompoundMenuItem_Click(object sender, EventArgs e)
		{
			ShowCorpIdChangeHistory_Click(sender, e);
		}

		private void ShowCorpIdChangeHistory_Click(object sender, EventArgs e)
		{
			int r = Grid.LastMouseDownRowIdx;
			if (r == GridControl.NewItemRowHandle) return;
			int c = Grid.LastMouseDownCol;

			//CellInfo ci = Grid.GetCellInfo(r, c);
			string corpIdString = Grid.GetCellValue(r, c).ToString();
			corpIdString = corpIdString.TrimStart('0');

			string cmd = "ShowCorpIdRegistrationHistory " + corpIdString;
			ClickFunctions.Process(cmd, Qm);
			return;
		}

		/// <summary>
		/// Free resources linked to this instance
		/// </summary>

		public new void Dispose()
		{
			try
			{
				Grid = null;
				ED1 = ED2 = ED3 = ED4 = null;

				Parent = null;

				base.Dispose();

				return;
			}

			catch (Exception ex) { DebugLog.Message(ex); }
		}

	}

	/// <summary>
	/// BitMaps5X8 for display in grid headers to show sorting etc
	/// </summary>

	public enum SmallHeaderGlyphs
	{
		Ascending = 0,
		Ascending1 = 1,
		Ascending2 = 2,
		Ascending3 = 3,
		Descending = 4,
		Descending1 = 5,
		Descending2 = 6,
		Descending3 = 7,
		CheckMark = 8,
		Filter = 9
	}
}