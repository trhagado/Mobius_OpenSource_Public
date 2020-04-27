using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class EditFind : DevExpress.XtraEditors.XtraForm
	{
		internal static EditFind Instance;
		internal MoleculeGridControl Grid; // grid we are operating on
		internal bool InFindNext_Click = false;

		internal QueryManager Qm { get { return Grid.QueryManager; } }
		internal DataTableMx DataTable { get { return Qm.DataTable; } }
		internal DataTableManager Dtm { get { return Qm.DataTableManager; } }
		internal Query Query { get { return Qm.Query; } } // associated query
		internal ResultsFormat ResultsFormat { get { return Qm.ResultsFormat; } } // associated results format
		internal ResultsFormat Rf { get { return Qm.ResultsFormat; } } // alias
		internal ResultsFormatter Formatter { get { return Qm.ResultsFormatter; } } // associated formatter

		public EditFind()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Static method to show the grid
		/// </summary>
		/// <param name="grid"></param>

		public static void Show(MoleculeGridControl grid)
		{
			if (Instance == null) Instance = new EditFind();
			Instance.Grid = grid;
			if (Instance.Visible) return;

			Instance.FindText.Text = "";
			Instance.Progress.Text = "";
			Instance.Show(SessionManager.ActiveForm);
		}

		private void EditFind_Activated(object sender, System.EventArgs e)
		{
			FindText.Focus();
		}

		private void EditFind_Deactivate(object sender, EventArgs e)
		{
			Grid.NotFocusedRowToHighlight = -1;
			Grid.NotFocusedColToHighlight = -1;
		}

		private void FindText_KeyUp(object sender, KeyEventArgs e)
		{ // make enter key do the next find
			if (e.KeyValue == (int)Keys.Return) // do the Find if enter key pressed
				FindNext_Click(null, null);
		}

		private void FindNext_Click(object sender, EventArgs e)
		{
			string findText, cellText;
			bool matches;

			string s = FindText.Text.Trim();
			if (s == "") return;

			if (InFindNext_Click) return; // avoid recursion
			InFindNext_Click = true;

			if (MatchCase.Checked) findText = s;
			else findText = s.ToLower();

			int startRow = Grid.Row; // where we are now
			if (startRow < 0) startRow = 0;
			int startCol = Grid.Col;
			if (startCol < 0) startCol = 0;

			int row = startRow; // where we will start to search
			int col = startCol;

			Progress.Text = "Searching row " + row.ToString() + "...";
			Progress.Visible = true;
			FindNext.Enabled = false;
			CloseButton.Text = "Cancel";

			int t0 = TimeOfDay.Milliseconds();

			bool [] checkColumn = new bool[Grid.V.Columns.Count]; // get list of columns to check
			for (int ci = 0; ci < Grid.V.Columns.Count; ci++)
			{
				GridColumn gc = Grid.V.Columns[ci];
				ColumnInfo cInf = Grid.GetColumnInfo(gc);
				if (cInf == null || cInf.Mc == null || cInf.Mc.IsGraphical)
					checkColumn[ci] = false;
				else checkColumn[ci] = true;
			}

			int iteration = -1;

// Loop through grid checking cells

			while(true)
			{ 
				iteration++;
				if (iteration > 0 && row == startRow && col == startCol)
				{ // if back at start then not found
					MessageBoxMx.Show(UmlautMobius.String + " cannot find the data you're searching for.", UmlautMobius.String,
						MessageBoxButtons.OK, MessageBoxIcon.Information);

					break;
				}

				if (!Visible) break; // cancelled by user

				col++; // go to next column
				if (col >= Grid.V.Columns.Count)
				{
					col = 1; // start at 1st col past check mark col
					row++;
					if (TimeOfDay.Milliseconds() - t0 > 500) // update display periodically
					{
						Progress.Text = "Searching row " + row.ToString() + "...";
						this.Refresh();
						Application.DoEvents();
						t0 = TimeOfDay.Milliseconds();
					}
				}


				if (row >= Grid.V.RowCount) // past end of grid?
				{
					if (Dtm.RowRetrievalComplete) // end of grid & have all data
						row = col = 0; // cycle back to top

					else // need to read more data 
					{
						int chunkSize = 100;
						Dtm.StartRowRetrieval(chunkSize);
						Progress.Text = "Retrieving data...";

						while (true) // loop until requested rows have been read, no more rows or cancel requested
						{
							Thread.Sleep(250);
							Application.DoEvents();

							if (row < Grid.V.RowCount) // have data for next row?
								break;

							else if (Dtm.RowRetrievalState != RowRetrievalState.Running) // must be at end of query
							{
								row = 0;
								break;
							}

							else if (!Visible) // cancelled by user
							{
								Progress.Text = "";
								FindNext.Enabled = true;
								CloseButton.Text = "Close";

								InFindNext_Click = false;
								return;
							}
						}

						Progress.Text = "Searching row " + row.ToString() + "..."; // restore search message
					}
				}

				if (!checkColumn[col]) continue;

				object vo = Grid[row, col];

				if (NullValue.IsNull(vo)) continue;

				else if (vo is MobiusDataType && (vo as MobiusDataType).FormattedText != null)
					cellText = (vo as MobiusDataType).FormattedText; // use existing formatted value

				else // need to format value
				{
					CellInfo cInf = Grid.GetCellInfo(row, col);
					cellText = Grid.Helpers.FormatFieldText(cInf);
				}

				if (String.IsNullOrEmpty(cellText)) continue;

				if (!MatchCase.Checked) cellText = cellText.ToLower();

				if (MatchEntireCell.Checked)
					matches = (cellText == findText);
				else matches = (cellText.IndexOf(findText) >= 0);

				if (matches)
				{
					//Grid.Focus();
					Grid.SelectCell(row, col);
					Grid.FocusCell(row, col); // put focus on cell found, may cause scroll event
					Grid.NotFocusedRowToHighlight = row;
					Grid.NotFocusedColToHighlight = col;

					// Reposition search box out of the way if necessary (above current cell)

					Rectangle findRect = // rectangle for find dialog
						new Rectangle(Instance.Left, Instance.Top, Instance.Width, Instance.Height);

					Rectangle cellRect = Grid.GetCellRect(row, col); // get rectangle relative to control
					Point p = Grid.PointToScreen(new Point(cellRect.Left, cellRect.Top)); // get coords relative to screen
					cellRect = new Rectangle(p.X, p.Y, cellRect.Width, cellRect.Height);

					if (findRect.IntersectsWith(cellRect))
						Instance.Top = cellRect.Top - Instance.Height - 4;

					break;
				}

			}

			Progress.Text = "";
			FindNext.Enabled = true;
			CloseButton.Text = "Close";

			InFindNext_Click = false;
			return;
		}

		/// <summary>
		/// Hide if visible
		/// </summary>

		public static void HideDialog()
		{
			if (Instance == null || !Instance.Visible) return;
			Instance.Visible = false;
		}

		private void CloseButton_Click(object sender, EventArgs e)
		{
			Visible = false;
		}

		private void EditFind_FormClosing(object sender, FormClosingEventArgs e)
		{
			Visible = false;
			e.Cancel = true;
		}


	}
}