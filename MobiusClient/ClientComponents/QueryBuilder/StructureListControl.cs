using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Layout.ViewInfo;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.BandedGrid.ViewInfo;
using DevExpress.Data;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class StructureListControl : DevExpress.XtraEditors.XtraUserControl
	{
		MoleculeList StructureList; // associated list
		DataTableMx DataTable; // DataTable created from StructureList and used as grid DataSource

		public ListItemSelectedDelegate ListItemSelectedCallback; // callback for selection of list item
		//MouseEventArgs LastMouseDownEventArgs; // event data for last mousedown event
		//int LastRowSelected = -1;
		bool InSetup = false;

		public StructureListControl()
		{
			InitializeComponent();

			if (SystemUtil.InDesignMode) return;

			MoleculeGrid.MouseDownCallback = new MouseEventHandler(MoleculeGrid_MouseDown);
			MoleculeGrid.MouseUpCallback = new MouseEventHandler(MoleculeGrid_MouseUp);
			MoleculeGrid.MouseClickCallback = new MouseEventHandler(MoleculeGrid_MouseClick);

			MoleculeGrid.AddStandardEventHandlers();

 			return;
		}
		
		public void Setup(
			MoleculeList structureList,
			ListItemSelectedDelegate itemSelectedCallback)
		{
			DataTableMx dt;
			DataRowMx dr;

			InSetup = true;

			StructureList = structureList;
			ListItemSelectedCallback = itemSelectedCallback;

// Create DataTable if not done yet

			if (DataTable == null)
			{
				dt = new DataTableMx();
				DataTable = dt; // save ref to table
				dt.Columns.Add("NameCol", typeof(string));
				dt.Columns.Add("StructureCol", typeof(MoleculeMx));
				dt.Columns.Add("DateCol", typeof(DateTime));
				dt.Columns.Add("StructureTypeCol", typeof(string));

				MoleculeGrid.SetupDefaultQueryManager(dt); // setup underlying QueryManager/QueryTable for current type
				MoleculeGrid.SetupUnboundColumns(dt);
			}

			dt = DataTable;

// Add rows to DataTable

			bool saveEnabled = dt.EnableDataChangedEventHandlers(false); // turn off change events while filling

			if (structureList == null) { InSetup = false; return; }

			for (int ri = 0; ri < structureList.Count; ri++) // fill in the grid
			{
				MoleculeListItem sli = structureList[ri];

				dr = dt.NewRow();

				dr["NameCol"] = sli.Name;
				dr["StructureCol"] = sli.Molecule;
				dr["DateCol"] = sli.UpdateDate;
				dr["StructureTypeCol"] = sli.MoleculeType;

				dt.Rows.Add(dr);
			}

			MoleculeGrid.DataSource = dt; // make the data visible
			BandedGridView.ClearSelection();
			dt.EnableDataChangedEventHandlers(saveEnabled);
			InSetup = false;
			return;
		}

		private void MoleculeGrid_SelectionChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

		}

			/// <summary>
			/// Handle mouse interaction with cells
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="e"></param>

			public void MoleculeGrid_MouseDown(object sender, MouseEventArgs e)
		{
			int gri = MoleculeGrid.LastMouseDownRowIdx;
			if (gri < 0 || gri >= StructureList.Count) return;

			if (e.Button == MouseButtons.Left) // left click
			{
				MoleculeListItem sli = StructureList[gri];
				if (ListItemSelectedCallback != null)  // callback defined?
					ListItemSelectedCallback(sli);
			}

			else if (e.Button == MouseButtons.Right)
			{
				RtClickMenu.Show(WindowsHelper.GetMousePosition()); ;
			}

			return;
		}

		public void MoleculeGrid_MouseUp(object sender, MouseEventArgs e)
		{
			return;
		}

		public void MoleculeGrid_MouseClick(object sender, MouseEventArgs e)
		{
			return;
		}

		private void CopyMenuItem_Click(object sender, EventArgs e)
		{
			int r = MoleculeGrid.LastMouseDownRowIdx;
			int c = MoleculeGrid.LastMouseDownCol;
			MoleculeGrid.CopyCommand(r, c);
		}

		private void RenameMenuItem_Click(object sender, EventArgs e)
		{

			int ri = MoleculeGrid.LastMouseDownRowIdx;
			if (ri < 0) return;

			string newName = InputBoxMx.Show("Enter new name:", "Rename", StructureList.ItemList[ri].Name);
			if (Lex.IsUndefined(newName)) return;

			StructureList.ItemList[ri].Name = newName;
			DataTable.Rows[ri]["NameCol"] = newName;

			SaveStructureListToPreferences(StructureList, StructureList.Name);

			return;
		}

		private void DeleteRowMenuItem_Click(object sender, EventArgs e)
		{
			int ri = MoleculeGrid.LastMouseDownRowIdx;
			if (ri < 0) return;

			StructureList.ItemList.RemoveAt(ri);
			DataTable.Rows.RemoveAt(ri);
			SaveStructureListToPreferences(StructureList, StructureList.Name);

			return;
		}

		private void ViewMoleculeInNewWindowMenuItem_Click(object sender, EventArgs e)
		{
			int gri = MoleculeGrid.LastMouseDownRowIdx;
			if (gri < 0 || gri >= StructureList.Count) return;
			MoleculeListItem sli = StructureList[gri];
			if (!MoleculeMx.IsDefined(sli.Molecule)) return;
			MoleculeViewer.ShowMolecule(sli.Molecule);

			return;
		}

		/// <summary>
		/// Load list from Preferences
		/// </summary>
		/// <param name="listName"></param>
		/// <param name="list"></param>

		public static void LoadStructureListFromPreferences(
			string listName,
			ref MoleculeList list)
		{
			if (list != null) return;

			string txt = Preferences.Get(listName);
			if (String.IsNullOrEmpty(txt))
			{
				list = new MoleculeList();
				return;
			}

			try
			{
				list = MoleculeList.Deserialize(txt);
				if (list != null) list.Name = listName;
			}

			catch (Exception ex)
			{
				DebugLog.Message(ex);
			}

			if (list == null) list = new MoleculeList();

			return;
		}

		/// <summary>
		/// Save list to preferences
		/// </summary>
		/// <param name="list"></param>
		/// <param name="listName"></param>

		public static void SaveStructureListToPreferences(
			MoleculeList list,
			string listName)
		{
			try
			{
				list.Name = listName;
				string serializedList = list.Serialize();
				Preferences.Set(listName, serializedList);
			}

			catch (Exception ex)
			{
				DebugLog.Message(ex);
			}

			return;
		}

	}

	/// <summary>
	/// ListItemSelectedDelegate callback delegate
	/// </summary>
	/// <param name="sli"></param>

	public delegate void ListItemSelectedDelegate(MoleculeListItem sli);

}
