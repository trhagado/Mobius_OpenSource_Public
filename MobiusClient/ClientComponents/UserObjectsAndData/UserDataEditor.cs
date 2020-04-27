using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraTab;
using DevExpress.Utils;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

	public partial class UserDataEditor : DevExpress.XtraEditors.XtraForm
	{
		MetaTable ParentMt;
		bool InSetup = false;

		internal int CurrentRow; // index of last grid row selected

		/// <summary>
		/// Default constructor
		/// </summary>

		public UserDataEditor()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Construct setting type of edit
		/// </summary>
		/// <param name="type"></param>

		public UserDataEditor(UserObjectType type)
		{
			if (type == UserObjectType.UserDatabase)
			{
				UserDatabase = true;
				AnnotationTable = false;
			}

			else if (type == UserObjectType.Annotation)
			{
				AnnotationTable = true;
				UserDatabase = false;
			}

			return;
		}

/// <summary>
/// Add column
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void AddColumn_Click(object sender, EventArgs e)
		{
			if (!AllowColumnModifications()) return;

			int r = View.FocusedRowHandle;
			if (r < 0) return;
			if (UserDatabase && r == 0) r = 1; // keep key & structure as 1st 2 cols
			r++; // index of new row

			DataRow dr = ColGridDataTable.NewRow();
			//dr["CustomFormat"] = new Bitmap(1, 1);
			dr["DisplayByDefault"] = true;
			ColGridDataTable.Rows.InsertAt(dr, r);
			View.FocusedRowHandle = r;
			View.FocusedColumn = View.Columns[0];
			View.ShowEditor();
		}

/// <summary>
/// Columns can be modified only if data has not been modified since last save
/// </summary>
/// <returns></returns>
		bool AllowColumnModifications()
		{
			if (!Dtm.DataModified || !PersistedDataExists) return true;
			else
			{
				MessageBoxMx.Show("You must save the current changes before you can modify the table definition");
				return false;
			}
		}

		bool AllowDataViewing()
		{
			if (!ColumnsModified || !PersistedDataExists) return true;
			else
			{
				MessageBoxMx.Show("You must save the current changes to the table definition before you can view data");
				return false;
			}
		}

		private void MoveColumnUp_Click(object sender, EventArgs e)
		{
			if (!AllowColumnModifications()) return;

			int r = View.FocusedRowHandle;
			if ((AnnotationTable && r <= 1) || (UserDatabase && r <= 2)) // keep initial col position(s) as is
			{
				MessageBoxMx.Show("This column can't be moved up.");
				return;
			}

			DataRow dr = ColGridDataTable.NewRow();
			dr.ItemArray = ColGridDataTable.Rows[r].ItemArray;
			ColGridDataTable.Rows.RemoveAt(r);
			ColGridDataTable.Rows.InsertAt(dr, r-1);
			View.FocusedRowHandle = r - 1;
			return;
		}

		private void MoveColumnDown_Click(object sender, EventArgs e)
		{
			if (!AllowColumnModifications()) return;

			int r = View.FocusedRowHandle;
			if (r < 0) return;
			if ((AnnotationTable && r <= 0) || (UserDatabase && r <= 1) || // keep initial col position(s) as is
			 r >= View.RowCount - 1)
			{
				MessageBoxMx.Show("This column can't be moved down.");
				return;
			}

			DataRow dr = ColGridDataTable.NewRow();
			dr.ItemArray = ColGridDataTable.Rows[r].ItemArray;
			ColGridDataTable.Rows.RemoveAt(r);
			ColGridDataTable.Rows.InsertAt(dr, r + 1);
			View.FocusedRowHandle = r + 1;
			return;
		}

		private void RemoveColumn_Click(object sender, EventArgs e)
		{
			if (!AllowColumnModifications()) return;

			int r = View.FocusedRowHandle;
			if (r < 0) return;
			if ((AnnotationTable && r <= 0) || (UserDatabase && r <= 1))
			{
				MessageBoxMx.Show("This column can't be removed.");
				return;
			}

			ColGridDataTable.Rows.RemoveAt(r);
		}

/// <summary>
/// Show import menu
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void ImportFileButton_Click(object sender, EventArgs e)
		{
			if (!AllowColumnModifications()) return;

			if (Dtm.DataModified)
			{
				MessageBoxMx.ShowError("You must save the current changes to the data before you can import a new file.");
				return;
			}

			if (ImportFileContextMenu.Tag == null)
				ImportFileContextMenu.Show(ImportFileButton,
				 new System.Drawing.Point(0, ImportFileButton.Size.Height));

			else
				ImportUcdbContextMenu.Show(ImportFileButton,
				 new System.Drawing.Point(0, ImportFileButton.Size.Height));

			return;
		}

		/// <summary>
		/// Import annotation from text file
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ImportTextFileMenuItem_Click(object sender, EventArgs e)
		{
			ImportTextFileDialog();
		}

		/// <summary>
		/// Import annotation from Excel file
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ImportExcelMenuItem_Click(object sender, EventArgs e)
		{
			ImportExcelDialog();
		}

		/// <summary>
		/// Import annotation from Smiles/csv file
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ImportSmiCsvMenuItem_Click(object sender, EventArgs e)
		{
			ImportTextFileDialog();
		}

		/// <summary>
		/// Import annotation from Sdfile
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ImportSdfileMenuItem_Click(object sender, EventArgs e)
		{
			ImportSDfileDialog();
		}

		private void Save_Click(object sender, EventArgs e)
		{
			bool saved;
			SyncCMtWithColGrid();
			SyncAMtWithCMt();
			if (!IsValidDefinition()) return;
			if (AnnotationTable)
				saved = SaveAnnotationTableDialog();

			else saved = SaveDatabaseDialog();

			if (saved) ColumnsModified = false;

			Saved |= saved;

			if (saved && ImportParms != null && ImportParms.ImportInBackground)
			{ // exit out of editor if importing in background
				if (RowGrid != null && DataFormatterOpen && Qm.QueryEngine != null)
					Qm.QueryEngine.Close(); // close any assoc QueryEngine
				DialogResult = DialogResult.OK;
			}
		}

		private void UserDataEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			string name, command;

			if (AnnotationTable && Modified)
			{ // see if want to save changes to annotation table
				name = Uo.Name;
				if (name == "") name = "this new annotation table";
				DialogResult dr = MessageBoxMx.Show(
					"Do you want to save the changes to " + name + "?",
					UmlautMobius.String, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

				if (dr == DialogResult.Yes) // yes try to save them
				{
					if (!IsValidDefinition()) return;
					if (!SaveAnnotationTableDialog()) // save cancelled
					{
						e.Cancel = true;
						return; 
					}
				}

				else if (dr == DialogResult.Cancel) // cancel the close
				{
					e.Cancel = true;
					return;
				}

				else { } // must be DialogResult.No, fall through to hide form without saving
			}

			else if (UserDatabase && Modified)
			{ // see if want to save changes to user database
				name = Uo.Name;
				if (name == "") name = "this new user compound database";
				DialogResult dr = MessageBoxMx.Show(
					"Do you want to save the changes to " + name + "?",
					UmlautMobius.String, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

				if (dr == DialogResult.Yes) // yes try to save them
				{
					if (!SaveDatabaseDialog()) // save cancelled
					{ 
						e.Cancel = true;
						return;
					}

				}

				else if (dr == DialogResult.Cancel) // cancel the close
				{
					e.Cancel = true;
					return;
				}
				else { } // must be DialogResult.No, fall through to hide form without saving
			}

			if (RowGrid != null && DataFormatterOpen) RowGrid.StopGridDisplay(); // close display of any molecule grid

			//if (UserDatabase && Saved && // need to start process to update model data?
			//	Udb.PendingStatus == UcdbWaitState.ModelPredictions && !Udbs.UpdateIsRunning(Udb.DatabaseId))
			//{
			//	command = "UpdateUcdbModelResults Pending " + Udb.DatabaseId;
			//	CommandLine.StartBackgroundSession(command);
			//}

			return;
		}

		private void Help_Click(object sender, EventArgs e)
		{
			if (UserDatabase)
				QbUtil.ShowConfigParameterDocument("EditUserDatabaseHelpUrl", "Edit User Database Help");
			else
				QbUtil.ShowConfigParameterDocument("EditAnnotationTableHelpUrl", "Edit Annotation Table Help");
		}

/// <summary>
/// New parent database selected
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void ParentCompoundCollection_TextChanged(object sender, EventArgs e)
		{
			string colType = "";

			if (InSetup) return;

			string parentDb = ParentCompoundCollection.Text;
			string parentMtName = null;
			RootTable rti = RootTable.GetFromTableLabel(parentDb);
			if (rti == null) return;

			if (!Lex.Eq(parentDb, "UserDatabase"))
				parentMtName = rti.MetaTableName;

			else // prompt user for user database
			{
				UserObject ubdbUo = UserObjectOpenDialog.ShowDialog(UserObjectType.UserDatabase, "Select Parent User Database");
				if (ubdbUo == null) return;

				List<UserObject> luo = GetUcdbUserObjects(ubdbUo.Id);
				if (luo == null) return;

				foreach (UserObject uo3 in luo)
				{
					if (uo3.Type != UserObjectType.Annotation) return; // structure or annotation table

					MetaTable mt2 = MetaTable.Deserialize(uo3.Content);
					if (!mt2.IsUserDatabaseStructureTable) return; // ucdb structure table

					InSetup = true;
					ParentCompoundCollection.Text = "UserDatabase: " + ubdbUo.Name;
					InSetup = false;
					parentMtName = mt2.Name;
					break;
				}
			}

			ParentMt = MetaTableCollection.Get(parentMtName);
			if (ParentMt == null) return;
			MetaColumn mc = ParentMt.KeyMetaColumn;

			if (AMt != null)
			{ // if annotation, link in parent & key metacolumn info
				AMt.Parent = ParentMt;
				if (AMt.MetaColumns.Count > 0)
				{
					AMt.MetaColumns[0] = mc.Clone(); // clone key
					AMt.MetaColumns[0].MetaTable = AMt; // set proper parent
				}
			}

			ColGridDataTable.Rows[0][0] = mc.Label; // show label in grid also
			if (mc.DataType == MetaColumnType.CompoundId) colType = "Compound Identifier";
			else if (mc.IsNumeric) colType = "Number";
			else colType = "Text";

			ColGridDataTable.Rows[0][1] = colType;
			return;
		}

/// <summary>
/// if going to Data Preview tab start data display as needed
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Tabs_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
		{
			if (Tabs.SelectedTabPageIndex == 1)
			{
				SyncCMtWithColGrid();
				SyncAMtWithCMt();
				if (!IsValidDefinition() || !AllowDataViewing())
				{
					Tabs.SelectedTabPageIndex = 0; // back to col def tab
					return;
				}

				if (!DataFormatterOpen || ColumnsModified) OpenDataFormatter();
				DisplayDataTabSelected = true;
			}

			else DisplayDataTabSelected = false;
		}

		private void ColGridView_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
		{
			CustomDrawRowIndicator(e);
		}

		private void SelectedModelsGridView_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
		{
			CustomDrawRowIndicator(e);
		}

/// <summary>
/// Number the rows in the indicator column
/// </summary>
/// <param name="e"></param>

		private void CustomDrawRowIndicator(DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
		{
			try
			{
				if (!e.Info.IsRowIndicator) return;
				if (e.RowHandle < 0)
					return; // may be NewItemRow indicator at initialization

				e.Info.DisplayText = (e.RowHandle + 1).ToString();
				e.Info.ImageIndex = -1; // remove any image that would overlay the row number
			}
			catch  {}
		}

		private void RowGridBandedView_CustomRowCellEdit(object sender, DevExpress.XtraGrid.Views.Grid.CustomRowCellEditEventArgs e)
		{
			if (e.RowHandle == GridControl.NewItemRowHandle && // avoid "No image data" text in new item row
				e.RepositoryItem is RepositoryItemPictureEdit)
				e.RepositoryItem = new RepositoryItemTextEdit();
		}

		private void RowGridBandedView_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
		{
			if (e.Column.ColumnEdit is RepositoryItemPictureEdit) // avoid "No image data" text
				e.DisplayText = "";
			return;
		}

		private void ColGridView_MouseDown(object sender, MouseEventArgs e)
		{
			Point p;
			Rectangle rect;
			string txt;
			int qtCi, ri, ci;

			if (View == null) return;
			p = new Point(e.X, e.Y);
			GridHitInfo ghi = View.CalcHitInfo(p);
			if (ghi == null) return;
			ri = ghi.RowHandle;

			if (ghi.Column == null) return;
			GridColumn gc = ghi.Column;

			GridViewInfo viewInfo = View.GetViewInfo() as GridViewInfo;
			GridCellInfo gci = viewInfo.GetGridCellInfo(ghi);
			if (gci == null) return;

			ri = ghi.RowHandle;
			if (ri == GridControl.NewItemRowHandle) // click in virtual new row?
			{
				DataRow dr = ColGridDataTable.NewRow();
				ColGridDataTable.Rows.Add(dr);

				DelayedCallback.Schedule(ClickedInNewRow, gc); // schedule callback for after grid rendered with new row
				return;
			}

			if (ri >= ColGridDataTable.Rows.Count) return;

			CurrentRow = ri; 

			if (Lex.Eq(gc.Name, "CustomFormat")) // Show format col
			{
				MetaColumn mc = GetMetaColumnFromColGridDataTableRow(ColGridDataTable.Rows[ri]);
				if (mc == null) return;

				ColumnFormattingContextMenu.Show(ColGrid, p);
				this.Refresh();
				return;
			}

			return;
		}

		/// <summary>
		/// Clicked in a cell in the virtual "NewRow" in the grid
		/// </summary>
		/// <param name="state"></param>

		void ClickedInNewRow(object state)
		{
			GridColumn gc = state as GridColumn;

			View.FocusedRowHandle = ColGridDataTable.Rows.Count - 1;
			View.FocusedColumn = gc;
			View.ShowEditor();

			//ColGrid.RefreshDataSource();
		}

		private void menuColWidthFieldCol_Click(object sender, EventArgs e)
		{
			bool mcNotInCMt;

			MetaColumn mc = GetMetaColumnFromColGridDataTableRow(ColGridDataTable.Rows[CurrentRow], out mcNotInCMt);
			if (mc == null) return;
			if (mcNotInCMt) mcNotInCMt = mcNotInCMt; // debug
			QueryColumn qc = new QueryColumn(mc);
			DialogResult dr = QueryColumnWidthDialog.Show(qc);
			if (dr == DialogResult.Cancel) return;

			UpdateColumnFormatting(qc);
			return;
		}

		private void NumberFormatMenuItem_Click(object sender, EventArgs e)
		{
			bool mcNotInCMt;

			MetaColumn mc = GetMetaColumnFromColGridDataTableRow(ColGridDataTable.Rows[CurrentRow], out mcNotInCMt);
			if (mc == null) return;
			if (mcNotInCMt) mcNotInCMt = mcNotInCMt; // debug

			QueryColumn qc = new QueryColumn(mc);
			DialogResult dr = ColumnFormatDialog.Show(qc);
			if (dr == DialogResult.Cancel) return;

			UpdateColumnFormatting(qc);
			return;
		}

		/// <summary>
		/// Update the formatting in the column grid, the metatable and any displayed data grid
		/// </summary>
		/// <param name="qc"></param>

		void UpdateColumnFormatting(QueryColumn qc)
		{
			SetCustomFormattingGridImage(ColGridDataTable.Rows[CurrentRow]); // update image in column grid
			ColGrid.RefreshDataSource(); // redraw column grid

			UpdateMetaColumnFormatting(qc); // update formatting info in metacolumn

			if (DataFormatterOpen && Qm != null) // update query and grid headers
			{
				SetupDataGridFormatting();
				RowGrid.DataSource = Qm.DataTable; // restore grid datasource to proper data table
			}
			return;
		}
	}
}
