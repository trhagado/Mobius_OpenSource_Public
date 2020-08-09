using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;

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
using DevExpress.Utils;
using DevExpress.Data;

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace Mobius.SpotfireClient
{

	/// <summary>
	/// TrellisCardBarChartControl - Based on CondFormatRulesControl
	/// </summary>

	public partial class TrellisCardBarChartControl : DevExpress.XtraEditors.XtraUserControl
	{
		internal int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		private SpotfireViewProps SVP = null; // Spotfire View props

		DataTableMapsMsx DataTableMaps => SVP?.DataTableMaps; // associated DataMap
		DataTableMapMsx CurrentMap => SVP?.DataTableMaps?.CurrentMap; // current DataTableMap
		private Query Query { get { return CurrentMap.Query; } } // associated query
		private VisualMsx Visual { get { return SVP.ActiveVisual; } } // associated Visual

		CondFormatRules Rules = null; // List of "Rules" that define the columns to chart

		internal CondFormatStyle ColoringStyle = CondFormatStyle.Undefined;
		internal MetaColumnType ColumnType = MetaColumnType.Number; // only number cols allowed for trellis bar chart cols
		public ImageCollection ColumnImageCollection = null; // set of allowed images for current format color style 
		public DataTable DataTable = null; // DataTable of grid information

		public bool LabelsRequired = false; // true if labels are required
		int ValColWidth = 0, ValCol2Width = 0; // save initial widths here
		bool InGridChanged = false;
		int LastIndicatorRowClicked = -1;

		RowCellClickEventArgs LastRowCellClickEventArgs = null;
		MouseEventArgs LastMouseDownEventArgs;

		bool InSetup = false;
		public event EventHandler EditValueChangedEventHandler; // event to fire when edit value changes

		BandedGridView V { get { return BandedGridView; } }

		const int LabelCol = 0; // position in data table and grid
		const int OpCol = 1;
		const int ValCol = 2;
		const int ValCol2 = 3;
		const int BackColorCol1 = 4;
		const int IconImageCol = 5;

		public TrellisCardBarChartControl()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);

			//RulesGrid.Mode = MoleculeGridMode.LocalView; // local editing of structure
			//			Grid.CompleteInitialization();
			//RulesGrid.AutoNumberRows = true;

			ValColWidth = V.Columns[ValCol].Width;
			ValCol2Width = V.Columns[ValCol2].Width;

			//RulesGrid.AddStandardEventHandlers();

			//RulesGrid.KeyUp -= // remove handlers we are replacing
			//	new System.Windows.Forms.KeyEventHandler(RulesGrid.MoleculeGridControl_KeyUp);
		}

		/// <summary>
		/// Setup the rules control
		/// </summary>
		/// <param name="columnType"></param>
		/// <param name="rules"></param>

		public void Setup(
			SpotfireViewProps svp,
			CondFormatRules rules,
			EventHandler editValueChangedEventHandler)
		{
			InSetup = true;

			SetupDataTableAndGrid(svp, rules);

			LoadDataTableFromCondFormatRules(rules);

			EditValueChangedEventHandler = editValueChangedEventHandler;

			InSetup = false;

			return;
		}


		/// <summary>
		/// Setup the rules control
		/// </summary>
		/// <param name="columnType"></param>
		/// <param name="rules"></param>

		public void Setup(
			SpotfireViewProps svp,
			CondFormatRules rules)
		{
			SetupDataTableAndGrid(svp, rules);

			LoadDataTableFromCondFormatRules(rules);

			return;
		}

		/// <summary>
		/// Setup the control for the specified column and format style 
		/// (Doesn't set the list of specific rules)
		/// </summary>
		/// <param name="columnType">MetaColumnType for column</param>

		public void SetupDataTableAndGrid(
			SpotfireViewProps svp,
			CondFormatRules rules)
		{
			SVP = svp;
			Rules = rules;

			ColoringStyle = rules.ColoringStyle;
			CondFormatStyle cfStyle = rules.ColoringStyle;

			DataTable dt = new DataTable();
			DataTable = dt; // save ref to table
			dt.Columns.Add("RuleName", typeof(string)); // column id
			dt.Columns.Add("Operator", typeof(string));


			dt.Columns.Add("Value", typeof(string));
			dt.Columns.Add("Value2", typeof(string));

			V.Columns[OpCol].Visible = false; // hide operator

			V.Columns[ValCol].Caption = "Comparison Value";
			V.Columns[ValCol].FieldName = "Value"; // restore name
			V.Columns[ValCol].UnboundType = UnboundColumnType.Bound; // column is bound

			V.Columns[ValCol2].Visible = true; // show high val

			V.Columns[ValCol].Width = ValColWidth;
			V.Columns[ValCol2].Width = ValCol2Width;
			EnableCfStyleOptions(colorSets: true, colorScales: false, dataBars: false, iconSets: true); // don't allow non-discrete styles

			string ops =
				"Equal to|" +
				"Between|" +
				">|" +
				"<|" +
				">=|" +
				"<=|" +
				"Not Equal to|" +
				"Not Between|" +
				"Any other value|" +
				"Missing";

			if (cfStyle == CondFormatStyle.ColorScale)
			{
				ops = "Between"; // only allow between rule
				V.Columns[ValCol].Caption = "Bottom Color Data Value";
				V.Columns[ValCol2].Caption = "Top Color Data Value";
			}

			else // single color (cfStyle == CondFormatStyle.ColorSet
			{
				V.Columns[ValCol].Visible = false;
				V.Columns[ValCol2].Visible = false;
			}

			string[] list = ops.Split('|');
			RepositoryItemComboBox cb = V.Columns[OpCol].ColumnEdit as RepositoryItemComboBox;
			if (cb != null)
			{
				cb.Items.Clear();
				cb.Items.AddRange(list);
			}

			dt.Columns.Add("BackColor1", typeof(Color));
			dt.Columns.Add("IconImageIdx", typeof(int));


			SetColumnVisibilityForColoringStyle(cfStyle);

			dt.RowChanged += new DataRowChangeEventHandler(DataRowChangeEventHandler);

			dt.RowDeleted += new DataRowChangeEventHandler(DataRowDeletedEventHandler);

			return;
		}

		/// <summary>
		/// Set the color and icon columns visibility
		/// </summary>
		/// <param name="showColorCol"></param>

		void SetColumnVisibilityForColoringStyle(CondFormatStyle coloringStyle)
		{

			RepositoryItemImageComboBox riie = IconImageRepositoryColumnEdit as RepositoryItemImageComboBox;

			bool allowMultipleRules = true;

			if (coloringStyle == CondFormatStyle.ColorSet)
			{
				BackColor1.Visible = true;
				IconImage.Visible = false;
			}

			else if (coloringStyle == CondFormatStyle.ColorScale)
			{
				BackColor1.Visible = false;
				IconImage.Visible = true;
				IconImage.Caption = "Color Scale";
				SetupImageComboBox(Bitmaps.I.ColorScaleImages);
				riie.LargeImages = Bitmaps.I.ColorScaleImages;
				riie.SmallImages = null;

				allowMultipleRules = false;
			}

			else if (coloringStyle == CondFormatStyle.DataBar)
			{
				BackColor1.Visible = false;
				IconImage.Visible = true;
				IconImage.Caption = "Color";
				SetupImageComboBox(Bitmaps.I.DataBarsImages);
				riie.LargeImages = Bitmaps.I.DataBarsImages;
				riie.SmallImages = null;

				allowMultipleRules = false;
			}

			else if (coloringStyle == CondFormatStyle.IconSet)
			{
				BackColor1.Visible = false;
				IconImage.Visible = true;
				IconImage.Caption = "Icon";
				SetupImageComboBox(Bitmaps.I.IconSetImages);
				riie.SmallImages = Bitmaps.I.IconSetImages;
				riie.LargeImages = null;
			}

			//V.OptionsView.NewItemRowPosition = // allow new rows?
			//	(allowMultipleRules ? NewItemRowPosition.Bottom : NewItemRowPosition.None);

			return;
		}

		void SetupImageComboBox(ImageCollection imageCollection)
		{
			RepositoryItemImageComboBox riie = IconImageRepositoryColumnEdit;
			riie.Items.Clear();

			Images images = imageCollection.Images;
			for (int ii = 0; ii < images.Count; ii++)
			{
				riie.Items.Add(new ImageComboBoxItem(ii));
			}
		}

		/// <summary>
		/// Enable/disable cf style options
		/// </summary>

		void EnableCfStyleOptions(
			bool colorSets = true,
			bool colorScales = true,
			bool dataBars = true,
			bool iconSets = true)
		{
			ColorSetBarItem.Enabled = colorSets;
			ColorScalesBarItem.Enabled = colorScales;
			DataBarsBarItem.Enabled = dataBars;
			IconSetsBarItem.Enabled = iconSets;
			return;
		}

		/// <summary>
		/// Get list of varied colors corresponding roughly to the basic set of custom colors in the ColorDialog
		/// </summary>
		/// <returns></returns>

		public static Color[] GetColors()
		{
			Color[] colors = {
				Color.FromArgb(255,192,192), // light (pastel)
				Color.FromArgb(255,224,192),
				Color.FromArgb(255,255,192),
				Color.FromArgb(192,255,192),
				Color.FromArgb(192,255,255),
				Color.FromArgb(192,192,255),
				Color.FromArgb(255,192,255),

				Color.FromArgb(255,128,128), // medium
				Color.FromArgb(255,192,128),
				Color.FromArgb(255,255,128),
				Color.FromArgb(128,255,128),
				Color.FromArgb(128,255,255),
				Color.FromArgb(128,128,255),
				Color.FromArgb(255,128,255),

				Color.FromArgb(255,000,000), // bold
				Color.FromArgb(255,128,000),
				Color.FromArgb(255,255,000),
				Color.FromArgb(000,255,000),
				Color.FromArgb(000,255,255),
				Color.FromArgb(000,000,255),
				Color.FromArgb(255,000,255),

				Color.FromArgb(192,000,000), // dark
				Color.FromArgb(192,064,000),
				Color.FromArgb(192,192,000),
				Color.FromArgb(000,192,000),
				Color.FromArgb(000,192,192),
				Color.FromArgb(000,000,192),
				Color.FromArgb(192,000,192) };

			//string[] colorNames = Enum.GetNames(typeof(KnownColor));
			//foreach (string colorName in colorNames)
			//{
			//  KnownColor knownColor = (KnownColor)Enum.Parse(typeof(KnownColor), colorName);
			//  if (knownColor > KnownColor.Transparent)
			//    colors.Add(Color.FromKnownColor(knownColor));
			//}

			return colors;
		}

		/// <summary>
		/// Add to the content of the rules control
		/// </summary>
		/// <param name="rules"></param>

		public void LoadDataTableFromCondFormatRules(
			CondFormatRules rules)
		{
			DataTable dt = DataTable;
			DataRow dr;

			InSetup = true;
			//bool saveEnabled = dt.EnableDataChangedEventHandlers(false); // turn off change events while filling

			// Setup for CF coloring style 

			CondFormatStyle colorStyle = rules.ColoringStyle;

			if (colorStyle == CondFormatStyle.ColorScale)
			{
				//ColorStyleDropDown.Text = "Continuous Colors";
				ColumnImageCollection = Bitmaps.I.ColorScaleImages;
			}

			else if (colorStyle == CondFormatStyle.DataBar)
			{
				//ColorStyleDropDown.Text = "Data Bars";
				ColumnImageCollection = Bitmaps.I.DataBarsImages;
			}

			else if (colorStyle == CondFormatStyle.IconSet)
			{
				//ColorStyleDropDown.Text = "Icon Set";
				ColumnImageCollection = Bitmaps.I.IconSetImages;
			}

			else
			{
				//ColorStyleDropDown.Text = "Discrete Colors";
				ColumnImageCollection = null;
			}

			// Setup rules for color style

			if (rules == null) { InSetup = false; return; }

			for (int ri = 0; ri < rules.Count; ri++) // fill in the grid
			{
				CondFormatRule r = rules[ri];

				dr = dt.NewRow();
				dt.Rows.InsertAt(dr, ri);

				dr[LabelCol] = r.Name;
				dr[OpCol] = r.Op;
				dr[ValCol] = r.Value;
				dr[ValCol2] = r.Value2;
				dr[BackColorCol1] = r.BackColor1;

				if (Lex.IsDefined(r.ImageName))
				{
					dr[IconImageCol] = Bitmaps.GetImageIndexFromName(ColumnImageCollection, r.ImageName);
				}

				else dr[IconImageCol] = DBNull.Value;
			}

			RulesGrid.DataSource = dt; // make the data visible
																 //RulesGrid.AddNewRowAsNeeded();

			//dt.EnableDataChangedEventHandlers(saveEnabled);
			InSetup = false;
			return;
		}

		/// <summary>
		/// Turn editing on
		/// </summary>

		public void StartEditing()
		{
			try { V.ShowEditor(); }
			catch (Exception Exception) { return; }
		}

		/// <summary>
		/// Get the rules from the form
		/// </summary>
		/// <returns></returns>

		public CondFormatRules GetCondFormatRulesFromDataTable()
		{

			CondFormatRules rules = new CondFormatRules();
			CondFormatRule rule = null;

			rules.ColoringStyle = ColoringStyle;

			for (int r = 0; r < DataTable.Rows.Count; r++)
			{
				string nameText = GetCellText(r, LabelCol);
				string opText = GetCellText(r, OpCol);
				CondFormatOpCode opCode = CondFormatRule.ConvertOpNameToCode(opText);
				string valText = GetCellText(r, ValCol);
				string val2Text = GetCellText(r, ValCol2);

				if (Lex.IsUndefined(nameText)) continue;

				bool valueRequired = (ColoringStyle == CondFormatStyle.ColorScale);

				if (valueRequired && Lex.IsUndefined(valText)) // skip if no value && a value is needed
					continue;

				rule = new CondFormatRule();
				rules.Add(rule);

				rule.Name = nameText;
				rule.Op = opText;
				rule.OpCode = opCode;

				rule.Value = valText;
				if (!String.IsNullOrEmpty(rule.Value))
					double.TryParse(rule.Value, out rule.ValueNumber);

				rule.Value2 = val2Text;
				if (!String.IsNullOrEmpty(rule.Value2))
					double.TryParse(rule.Value2, out rule.Value2Number);

				rule.BackColor1 = GetCellColor(r, BackColorCol1);

				int ii = GetCellInt(r, IconImageCol);
				if (ii >= 0) rule.ImageName = Bitmaps.GetImageNameFromIndex(ColumnImageCollection, ii);
				else rule.ImageName = "";
			}

			return rules;
		}

		public string GetCellText(
			int gr,
			int gc)
		{
			object vo = DataTable.Rows[gr].ItemArray[gc];
			if (vo != null)
				return vo.ToString();

			else return null;
		}

		public void SetCellText(
			int gr,
			int gc,
			string text)
		{
			DataTable.Rows[gr].ItemArray[gc] = text;
			//V.RefreshRowCell(gr, col);
		}

		public int GetCellInt(
			int gr,
			int gc)
		{
			object vo = DataTable.Rows[gr].ItemArray[gc];
			if (vo is int) return (int)vo;
			else return NullValue.NullNumber;
		}

		public Color GetCellColor(
			int gr,
			int gc)
		{
			object vo = DataTable.Rows[gr].ItemArray[gc];

			if (vo is Color)
				return (Color)vo;

			else return Color.Empty;
		}

		/// <summary>
		/// Check for valid format
		/// </summary>
		/// <returns></returns>

		public bool AreValid()
		{
			string txt;
			int count = 0;
			DataRow dr;

			for (int r = 0; r < DataTable.Rows.Count; r++)
			{
				dr = DataTable.Rows[r];

				string label = dr[LabelCol] as string;

				string op = dr[OpCol] as string;
				if (op == null) op = "";

				CondFormatOpCode opCode = CondFormatRule.ConvertOpNameToCode(op);
				string value = dr[ValCol] as string;
				string value2 = dr[ValCol2] as string;

				if (String.IsNullOrEmpty(op) && String.IsNullOrEmpty(value)) continue; // nothing on line

				if (Lex.IsNullOrEmpty(label) && LabelsRequired)
				{
					XtraMessageBox.Show("A label must be supplied", UmlautMobius.String);
					//RulesGrid.EditCell(r, LabelCol);
					return false;
				}

				if (opCode == CondFormatOpCode.Null || opCode == CondFormatOpCode.NotNull || opCode == CondFormatOpCode.Exists)
					continue; // no need to check value

				if (op == "")
				{
					XtraMessageBox.Show("A comparison type must be supplied", UmlautMobius.String);
					//RulesGrid.EditCell(r, OpCol);
					return false;
				}

				if (value == "")
				{
					XtraMessageBox.Show("A value must be supplied", UmlautMobius.String);
					//RulesGrid.EditCell(r, ValCol);
					return false;
				}

				if (opCode == CondFormatOpCode.Within) // check within date value as integer
				{
					if (!IsValidValue(value, MetaColumnType.Integer, r, ValCol)) return false;
					if (value2 == "")
					{
						XtraMessageBox.Show("A date unit must be supplied", UmlautMobius.String);
						//RulesGrid.EditCell(r, ValCol2);
						return false;
					}
				}

				else if (!IsValidValue(value, ColumnType, r, ValCol)) return false;

				if (opCode == CondFormatOpCode.Between || opCode == CondFormatOpCode.NotBetween)
				{
					if (value2 == "")
					{
						XtraMessageBox.Show("An \"Between\" high value must be supplied", UmlautMobius.String);
						//RulesGrid.EditCell(r, ValCol2);
						return false;
					}

					else if (!IsValidValue(value2, ColumnType, r, ValCol2)) return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Check for valid low or high value
		/// </summary>
		/// <param name="val"></param>
		/// <param name="colType"></param>
		/// <param name="r"></param>
		/// <param name="c"></param>
		/// <returns></returns>

		private bool IsValidValue(
			string val,
			MetaColumnType colType,
			int r,
			int c)
		{
			double d1;

			if (MetaColumn.IsNumericMetaColumnType(colType))
			{
				if (!double.TryParse(val, out d1))
				{
					XtraMessageBox.Show("Invalid numeric value", UmlautMobius.String);
					//RulesGrid.EditCell(r, c);
					return false;
				}
			}

			else if (colType == MetaColumnType.Date)
			{
				if (DateTimeMx.Normalize(val) == null)
				{
					XtraMessageBox.Show("Invalid date", UmlautMobius.String);
					//RulesGrid.EditCell(r, c);
					return false;
				}
			}

			return true;
		}

		private void BandedGridView_MouseDown(object sender, MouseEventArgs e)
		{
			//ClientLog.Message("BandedGridView_MouseDown");
			LastMouseDownEventArgs = e; // just save for click event to avoid need for extra mouse click
		}

		private void BandedGridView_MouseUp(object sender, MouseEventArgs e)
		{
			//ClientLog.Message("BandedGridView_MouseUp");
			DelayedCallback.Schedule(ShowColumnSelectorMenu); // handle event in timer to avoid strange mouse behavior
		}

		private void ShowColumnSelectorMenu()
		{

			throw new NotImplementedException(); // todo...
#if false
			QueryColumn qc;
			DataRow dr;
			ColumnMapMsx colMap;
			int ri, ci;

			MouseEventArgs e = LastMouseDownEventArgs;
			Point p = new Point(e.X, e.Y);
			BandedGridHitInfo ghi = V.CalcHitInfo(p);

			ri = ghi.RowHandle;
			//if (ri < 0) return;
			BandedGridColumn col = ghi.Column;
			if (col == null) return;
			ci = col.AbsoluteIndex;

			if (ci != LabelCol) return;

			qc = null; // existing assignment
			if (ri != GridControl.NewItemRowHandle)
			{
				dr = DataTable.Rows[ri];
				string spotfireName = dr[LabelCol] as string;
				qc = CurrentMap.ColumnMapCollection.GetQueryColumnForSpotfireColumnName(spotfireName);
			}

			ColumnSelectorFromQueryMsx csc = new ColumnSelectorFromQueryMsx();
			csc.Setup(SVP, qc);
			csc.Flags.ExcludeNonNumericTypes = true;

			Point screenLoc = this.PointToScreen(p);
			QueryColumn qc2 = csc.ShowMenu(screenLoc);
			if (qc2 == null) return;

			if (ri == GridControl.NewItemRowHandle) // adding row
			{
				dr = DataTable.NewRow();
				DataTable.Rows.Add(dr);

				if (IconImage.Visible)
				{
					dr[ValCol] = "(Min)";
					dr[ValCol2] = "(Max)";
					dr[IconImageCol] = Bitmaps.GetImageIndexFromName(ColumnImageCollection, CondFormat.DefaultColorScale);
				}

				else
				{
					Color[] ca = Bitmaps.GetColorSetByName(Bitmaps.ColorSetImageColors, CondFormat.DefaultColorSet);
					if (ca != null && ca.Length > 0)
						dr[BackColorCol1] = ca[DataTable.Rows.Count % ca.Length]; 
				}
			}

			else dr = DataTable.Rows[ri];

			colMap = CurrentMap.UpdateColumnMapList(qc2);
			dr[LabelCol] = colMap.SpotfireColumnName;

			return;
#endif
		}

		/// <summary>
		/// Return true if r, c is in the range of selected rows
		/// </summary>
		/// <param name="r"></param>
		/// <param name="c"></param>
		/// <param name="topRow"></param>
		/// <param name="bottomRow"></param>
		/// <returns></returns>

		private bool RowColInSelectedRows(
			int r,
			int c,
			out int topRow,
			out int bottomRow)
		{
			if (RowsSelected(out topRow, out bottomRow) &&
			 r >= topRow && r <= bottomRow)
				return true;

			else return false;
		}

		/// <summary>
		/// Return true if one or more complete rows are selected
		/// </summary>
		/// <param name="topRow"></param>
		/// <param name="bottomRow"></param>
		/// <returns></returns>

		private bool RowsSelected(
			out int topRow,
			out int bottomRow)
		{
			topRow = bottomRow = -1;

			BandedGridView bgv = RulesGrid.MainView as BandedGridView;
			GridCell[] cells = bgv.GetSelectedCells();
			if (cells.Length <= 1) return false;

			foreach (GridCell cell in cells)
			{
				if (topRow < 0 || cell.RowHandle < topRow) topRow = cell.RowHandle;
				if (cell.RowHandle > bottomRow) bottomRow = cell.RowHandle;
			}

			return true;
		}

		/// <summary>
		/// Activate editor for cell
		/// </summary>
		/// <param name="r"></param>
		/// <param name="c"></param>

		private void EditCellValue(
			int r,
			int c)
		{
			BandedGridView bgv = RulesGrid.MainView as BandedGridView;
			bgv.ClearSelection();
			bgv.SelectCell(r, bgv.Columns[c]);
			bgv.FocusedRowHandle = r;
			bgv.FocusedColumn = bgv.Columns[c];
			bgv.ShowEditor(); // doesn't seem to open dropdown
												//Application.DoEvents();

			//ColorDialog cd = new ColorDialog();
			//cd.Color = RulesGrid.GetCellBackColor(r, BackColorCol);
			//DialogResult dr = cd.ShowDialog(this);
			//if (dr != DialogResult.OK) return;

			//CellInfo ci = RulesGrid.GetCellInfo(r, c);
			//RulesGrid.DataTable.Rows[ci.DataRowIndex][ci.DataColIndex] = cd.Color;
			//bgv.RefreshRowCell(r, ci.GridColumn);

			//RulesGrid.AddNewRowAsNeeded();
			return;
		}

		private void AddColumn_Click(object sender, EventArgs e)
		{
			int r = V.FocusedRowHandle;
			if (r == GridControl.NewItemRowHandle) // just return if on new row
			{
				RulesGrid.Focus(); // put focus back
													 //				V.ShowEditor();
				return;
			}

			if (r < 0) r = 0;
			else r = r + 1;

			DataRow dr = DataTable.NewRow();
			DataTable.Rows.InsertAt(dr, r);
			//RulesGrid.InsertRowAt(r);
			//			RulesGrid.EditCell(r, LabelCol);

			RulesGrid.Refresh();
			//int rowCount = DataTable.Rows.Count;
			return;
		}

		/// <summary>
		/// Start editing specified cell
		/// </summary>
		/// <param name="rowHandle"></param>
		/// <param name="colHandle"></param>

		public void EditCell(
		int rowHandle,
		int colHandle)
		{
			//V.SelectCell(rowHandle, V.Columns[colHandle]);
			V.FocusedRowHandle = rowHandle;
			V.FocusedColumn = V.Columns[colHandle];
			V.ShowEditor();
		}


		/// <summary>
		/// Import a list of structures into the condformat rule list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void RemoveButton_Click(object sender, EventArgs e)
		{
			int[] rows = V.GetSelectedRows();

			if (rows.Length > 0)
				DataTable.Rows.RemoveAt(rows[0]);
		}

		private void RemoveAllButton_Click(object sender, EventArgs e)
		{
			while (DataTable.Rows.Count > 0)
			{
				DataTable.Rows.RemoveAt(0);
			}
		}

		private void MoveRuleUp_Click(object sender, EventArgs e)
		{
			int r = GetSelectedRow();
			if (r < 0) return;

			if (r == 0) return;
			DataRow dr = DataTable.Rows[r];

			DataRow newRow = DataTable.NewRow();
			newRow.ItemArray = dr.ItemArray; // copy data
			DataTable.Rows.RemoveAt(r);
			DataTable.Rows.InsertAt(newRow, r - 1);
			return;
		}

		private void MoveRuleDown_Click(object sender, EventArgs e)
		{
			int r = GetSelectedRow();
			if (r < 0) return;

			DataRow dr = DataTable.Rows[r];
			if (r == DataTable.Rows.Count - 1) return;

			DataRow newRow = DataTable.NewRow();
			newRow.ItemArray = dr.ItemArray; // copy data
			DataTable.Rows.RemoveAt(r);
			DataTable.Rows.InsertAt(newRow, r + 1);
			return;
		}

		int GetSelectedRow()
		{
			int[] rows = V.GetSelectedRows();
			if (rows.Length > 0) return rows[0];
			else return -1; // no rows to select
		}

		/// <summary>
		/// Setup ColorSet rules and grid
		/// </summary>
		/// <param name="setName"></param>

		private void InitializeColorSetCondFormatRulesAndGrid(
			string setName)
		{
			if (InSetup) return;

			SetColumnVisibilityForColoringStyle(CondFormatStyle.ColorSet);

			CondFormatRules rules = InitializeColorSetCondFormatRules(setName);

			Setup(SVP, rules);

			EditValueChanged();
			return;
		}

		/// <summary>
		/// Setup color set conditional formatting rules for column
		/// </summary>
		/// <param name="setName"></param>
		/// <returns></returns>

		private CondFormatRules InitializeColorSetCondFormatRules(string setName)
		{
			CondFormatRule rule = null;

			CondFormatRules rules = GetCondFormatRulesFromDataTable();

			bool compatible = (rules.ColoringStyle == CondFormatStyle.ColorSet ||
				rules.ColoringStyle == CondFormatStyle.IconSet);

			bool setExists = (Bitmaps.ColorSetImageColors.ContainsKey(setName));

			if (!compatible || !setExists) // need new rules set?
			{
				if (!setExists || Lex.Eq(setName, CondFormat.DefaultColorSet))
				{
					CondFormat cf = CondFormat.BuildDefaultConditionalFormatting();
					rules = cf.Rules;
					return rules;
				}

				rules = new CondFormatRules(CondFormatStyle.ColorSet);
			}

			rules.ColoringStyle = CondFormatStyle.ColorSet;
			Color[] colors = Bitmaps.ColorSetImageColors[setName];

			for (int ri = 0; ri < colors.Length; ri++)
			{
				if (ri < rules.Count) rule = rules[ri];

				else
				{
					rule = new CondFormatRule();
					rule.Name = "Rule " + (ri + 1);
					rule.Op = "<=";
					rule.OpCode = CondFormatRule.ConvertOpNameToCode(rule.Op);
					rules.Add(rule);
				}

				rule.BackColor1 = colors[ri];
			}

			return rules;
		}

		/// <summary>
		/// Setup ColorScale rules and grid
		/// </summary>
		/// <param name="setName"></param>

		private void InitializeColorScaleCondFormatRulesAndGrid(
			string setName)
		{
			if (InSetup) return;

			SetColumnVisibilityForColoringStyle(CondFormatStyle.ColorScale);

			EditValueChanged();
			return;
		}

		/// <summary>
		/// Setup DataBar rules and grid
		/// </summary>
		/// <param name="barsName"></param>

		private void InitializeDataBarCondFormatRulesAndGrid(
				string barsName)
		{
			CondFormatRules rules;

			if (InSetup) return;

			SetColumnVisibilityForColoringStyle(CondFormatStyle.DataBar);

			EditValueChanged();
			return;
		}

		/// <summary>
		/// Setup icon set rules and grid
		/// </summary>
		/// <param name="imageNames"></param>

		private void InitializeIconSetCondFormatRulesAndGrid(
				params string[] imageNames)
		{
			CondFormatRules rules;

			if (InSetup) return;

			SetColumnVisibilityForColoringStyle(CondFormatStyle.IconSet);

			EditValueChanged();
			return;
		}

		/// <summary>
		/// If editing a compoundId or background color column call proper editor
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void bandedGridView1_ShowingEditor(object sender, CancelEventArgs e)
		{
			//if (ColumnType == MetaColumnType.CompoundId && RulesGrid.Col == ValCol)
			//{ // if saved list name invoke editor
			//	e.Cancel = true;
			//	//EditSavedListName(RulesGrid.Row, RulesGrid.Col);
			//	return;
			//}

			//if (ColumnType == MetaColumnType.Structure && RulesGrid.Col == ValCol)
			//{
			//	e.Cancel = true;
			//	return;
			//}

			return;
		}

		private void bandedGridView1_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
		{
			if (Lex.StartsWith(e.Column.Name, "BackColor") || // avoid showing text of color in cell
				e.Column.ColumnEdit is RepositoryItemPictureEdit) // avoid "No image data" text
				e.DisplayText = "";
			return;
		}

		private void bandedGridView1_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
		{
			if (e.RowHandle == GridControl.NewItemRowHandle && // avoid "No image data" text in new item row
				e.RepositoryItem is RepositoryItemPictureEdit)
				e.RepositoryItem = new RepositoryItemTextEdit();
		}

		/// <summary>
		/// Put row number in indicator
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void BandedGridView_CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
		{
			if (e.Info.IsRowIndicator && e.RowHandle >= 0)
			{
				e.Info.DisplayText = (e.RowHandle + 1).ToString();
				e.Info.ImageIndex = -1; // no image
			}
		}

		/// <summary>
		/// Retrieve Spotfire name for mapped col for display in grid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void BandedGridView_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
		{
			if (!(e.Value is ColumnMapMsx)) return;

			ColumnMapMsx map = e.Value as ColumnMapMsx;

			e.DisplayText = map.SpotfireColumnName;
			return;
		}


		private void ColorSetItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeColorSetCondFormatRulesAndGrid("ColorSet1");
		}

		private void ColorSetItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeColorSetCondFormatRulesAndGrid("ColorSet2");
		}

		private void ColorSetItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeColorSetCondFormatRulesAndGrid("ColorSet3");
		}
		private void ColorSetItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeColorSetCondFormatRulesAndGrid("ColorSet4");
		}

		private void ColorSetItem5_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeColorSetCondFormatRulesAndGrid("ColorSet5");
		}

		private void ColorSetItem6_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeColorSetCondFormatRulesAndGrid("ColorSet6");
		}

		// Initialize Color Scales Rules

		private void ColorScaleItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeColorScaleCondFormatRulesAndGrid("ColorScale1");
		}

		private void ColorScaleItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeColorScaleCondFormatRulesAndGrid("ColorScale2");
		}

		private void ColorScaleItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeColorScaleCondFormatRulesAndGrid("ColorScale3");
		}

		private void ColorScaleItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeColorScaleCondFormatRulesAndGrid("ColorScale4");
		}

		private void ColorScaleItem5_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeColorScaleCondFormatRulesAndGrid("ColorScale5");
		}

		private void ColorScaleItem6_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeColorScaleCondFormatRulesAndGrid("ColorScale6");
		}

		// Initialize Data Bars Rules

		private void DataBarsBlueBarItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeDataBarCondFormatRulesAndGrid("DataBarsBlue");
		}

		private void DataBarsGreenBarItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeDataBarCondFormatRulesAndGrid("DataBarsGreen");
		}

		private void DataBarsRedBarItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeDataBarCondFormatRulesAndGrid("DataBarsRed");
		}

		private void DataBarsCyanBarItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeDataBarCondFormatRulesAndGrid("DataBarsCyan"); // not really cyan
		}

		private void DataBarsYellowBarItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeDataBarCondFormatRulesAndGrid("DataBarsYellow");
		}

		private void DataBarsMagentaBarItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeDataBarCondFormatRulesAndGrid("DataBarsMagenta");
		}

		// Initialize Icon Rules and grid

		private void IconPiesBarItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeIconSetCondFormatRulesAndGrid("Pie1", "Pie2", "Pie3", "Pie4", "Pie5");
		}

		private void IconBarsBarItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeIconSetCondFormatRulesAndGrid("Bars1", "Bars2", "Bars3", "Bars4", "Bars5");
		}

		private void IconCirclesBarItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeIconSetCondFormatRulesAndGrid("Circle1", "Circle2", "Circle3", "Circle4");
		}

		private void IconShapesBarItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeIconSetCondFormatRulesAndGrid("Shape1", "Shape2", "Shape3");
		}

		private void IconArrowsBarItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeIconSetCondFormatRulesAndGrid("Arrow1", "Arrow3", "Arrow5");
		}

		private void IconTrianglesBarItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeIconSetCondFormatRulesAndGrid("UpDown1", "UpDown2", "UpDown3");
		}

		private void BandedGridView_RowCellClick(object sender, RowCellClickEventArgs e)
		{
			LastRowCellClickEventArgs = e;
			return;
		}

		private void BandedGridView_ShowingEditor(object sender, CancelEventArgs e)
		{
			return;
		}

		private void BandedGridView_ShownEditor(object sender, EventArgs e)
		{
			return;
		}

		private void BandedGridView_CellValueChanging(object sender, CellValueChangedEventArgs e)
		{

			if (InSetup) return;

			int ri = e.RowHandle;
			int ci = e.Column.AbsoluteIndex;
			if (e.Column == BackColor1 || e.Column == IconImage) // force completion of edit
			{
				object value = e.Value;
				BandedGridView.CloseEditor();  // Closes editor
				this.BeginInvoke((Action)(() => // safe way to execute lines below
				{
					BandedGridView.SetRowCellValue(e.RowHandle, e.Column, value); 
					BandedGridView.ShowEditor(); // open editor again
				}));
				return;
			}

			else if (ci == ValCol || ci == ValCol2)
			{

				if (e.Value == null || Lex.Ne(e.Value.ToString(), "User-defined")) return; // blank out if user-defined

				//ComboBoxEdit cb = BandedGridView.ActiveEditor as ComboBoxEdit;

				BandedGridView.CloseEditor();  // Closes editor

				this.BeginInvoke((Action)(() => // safe way to execute lines below
				{
					BandedGridView.SetRowCellValue(e.RowHandle, e.Column, ""); // blank value
					BandedGridView.ShowEditor(); // open editor again
				}));
			}

			return;
		}

		private void BandedGridView_CellValueChanged(object sender, CellValueChangedEventArgs e)
		{
			EditValueChanged();
			return;
		}

		/// <summary>
		/// Grid changed, pass along event if requested
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void DataRowChangeEventHandler(object sender, DataRowChangeEventArgs e)
		{
			//EditValueChanged(); // (don't need, duplicates CellValueChanged event)
			return;
		}

		void DataRowDeletedEventHandler(object sender, DataRowChangeEventArgs e)
		{
			EditValueChanged();
			return;
		}

		/// <summary>
		/// Call any EditValueChanged event
		/// </summary>

		void EditValueChanged(
			object sender = null,
			EventArgs e = null)
		{
			if (InSetup) return;

			if (EditValueChangedEventHandler != null) // fire EditValueChanged event if handlers present
				EditValueChangedEventHandler(this, EventArgs.Empty);
		}

	}
}
