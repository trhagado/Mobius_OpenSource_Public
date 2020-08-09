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

namespace Mobius.ClientComponents
{
	public partial class CondFormatRulesControl : XtraUserControl
	{
		internal int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		public MetaColumnType ColumnType = MetaColumnType.Unknown;
		public ResultsField ResultsField = null; // optional ResultsField
		public ImageCollection ColumnImageCollection = null; // set of allowed images for current format color style 
		public DataTableMx DataTable = null; // DataTable of grid information

		public bool LabelsRequired = false; // true if labels are required
		bool InSetRules = false;
		public event EventHandler EditValueChanged; // event to fire when edit value changes
		int ValColWidth = 0, ValCol2Width = 0; // save initial widths here
		bool InGridChanged = false;
		int LastIndicatorRowClicked = -1;

		const int LabelCol = 0; // position in data table and grid
		const int OpCol = 1;
		const int ValCol = 2;
		const int ValCol2 = 3;
		const int BackColorCol = 4;
		const int IconImageCol = 5;

		public CondFormatRulesControl()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);

			if (SystemUtil.InDesignMode) return;

			RulesGrid.Mode = MoleculeGridMode.LocalView; // local editing of structure
																									 //			Grid.CompleteInitialization();
			RulesGrid.AutoNumberRows = true;

			ValColWidth = RulesGrid.V.Columns[ValCol].Width;
			ValCol2Width = RulesGrid.V.Columns[ValCol2].Width;

			RulesGrid.AddStandardEventHandlers();

			RulesGrid.KeyUp -= // remove handlers we are replacing
				new System.Windows.Forms.KeyEventHandler(RulesGrid.MoleculeGridControl_KeyUp);
		}

		/// <summary>
		/// Setup the control for the specified column and format style 
		/// (Doesn't set the list of specific rules)
		/// </summary>
		/// <param name="columnType">MetaColumnType for column</param>
		/// <param name="rfld">Optional ResultsField</param>

		public void SetupControl(
			MetaColumnType columnType,
			CondFormatStyle cfStyle,
			ResultsField rfld = null)
		{
			ColumnType = columnType;
			ResultsField = rfld;

			DataTableMx dt = new DataTableMx();
			DataTable = dt; // save ref to table
			dt.Columns.Add("RuleName", typeof(string));
			dt.Columns.Add("Operator", typeof(string));

			if (rfld != null && rfld.MetaColumn.IsKey) { } // todo: handle key properly

			if (ColumnType == MetaColumnType.CompoundId && false) // disabled, treat compound id like string for now
			{
				dt.Columns.Add("Value", typeof(string));
				dt.Columns.Add("Value2", typeof(string));

				RulesGrid.V.Columns[OpCol].Visible = false; // hide operator
				RulesGrid.V.Columns[ValCol2].Visible = false; // hide 2nd value

				RulesGrid.V.Columns[ValCol].Width = 200;
				RulesGrid.V.Columns[ValCol].Caption = "Saved List Name";

				RulesGrid.V.Columns[ValCol2].Visible = false; // hide high val

				Prompt.Text =
					"Enter the name(s) of the saved compound id list(s) " +
					"to be matched against each retrieved compound id.";
			}

			else if (ColumnType == MetaColumnType.Structure)
			{
				dt.Columns.Add("Value", typeof(MoleculeMx));
				dt.Columns.Add("Value2", typeof(string));

				RulesGrid.V.Columns[OpCol].Visible = false; // hide operator
				RulesGrid.V.Columns[ValCol2].Visible = false; // hide 2nd value

				RulesGrid.V.Columns[ValCol].Caption = "Substructure Query";
				RulesGrid.V.Columns[ValCol].FieldName = "Value"; // restore name
				RulesGrid.V.Columns[ValCol].UnboundType = UnboundColumnType.Object;
				RulesGrid.V.Columns[ValCol].ColumnEdit = null; // clear editor for unbound
				RulesGrid.V.Columns[ValCol].Width = 250; // wider for structure

				Prompt.Text =
					"Enter the substructure queries to match, in order, " +
					"against each retrieved database structure until a successful match occurs.";

				EnableCfStyleOptions(colorSets:true, colorScales:false, dataBars:false, iconSets:false); // don't allow non-discrete coloring for structures
			}

			else // other types
			{
				dt.Columns.Add("Value", typeof(string));
				dt.Columns.Add("Value2", typeof(string));

				Prompt.Text =
					"Enter the list of rules to match, in order, " +
					"against each retrieved field value until a successful match occurs.";

				RulesGrid.V.Columns[OpCol].Visible = true; // show operator

				RulesGrid.V.Columns[ValCol].Caption = "Comparison Value";
				RulesGrid.V.Columns[ValCol].FieldName = "Value"; // restore name
				RulesGrid.V.Columns[ValCol].UnboundType = UnboundColumnType.Bound; // column is bound
				RulesGrid.V.Columns[ValCol].ColumnEdit = RuleName.ColumnEdit; // need to set editor to keep bound (was repositoryItemTextEdit1)

				RulesGrid.V.Columns[ValCol2].Visible = true; // show high val

				if (MetaColumn.IsNumericMetaColumnType(ColumnType) || ColumnType == MetaColumnType.Date)
				{
					RulesGrid.V.Columns[ValCol].Width = ValColWidth; // (int)(ValColWidth * .7); // // narrower cols if numeric (no, labels don't look good)
					RulesGrid.V.Columns[ValCol2].Width = ValColWidth; // (int)(ValCol2Width * .7);
					EnableCfStyleOptions(); // allow all style options for numbers & dates
				}
				else
				{
					RulesGrid.V.Columns[ValCol].Width = ValColWidth;
					RulesGrid.V.Columns[ValCol2].Width = ValCol2Width;
					//if (rfld != null) // if we have rfld assume we are viewing results & can get stats
					EnableCfStyleOptions(colorSets: true, colorScales: false, dataBars: false, iconSets: true); // don't allow non-discrete styles
				}

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

				if (ColumnType == MetaColumnType.String)
					ops = ops.Replace("Missing",
					"Contains Substring|" +
					"Missing");

				if (ColumnType == MetaColumnType.Date)
					ops = ops.Replace("Missing",
					"Within the Last|" +
					"Missing");

				if (cfStyle == CondFormatStyle.ColorScale)
				{
					ops = "Between"; // only allow between rule
					RulesGrid.V.Columns[ValCol].Caption = "Bottom Color Data Value";
					RulesGrid.V.Columns[ValCol2].Caption = "Top Color Data Value";
				}

				else if (cfStyle == CondFormatStyle.DataBar)
				{
					ops = "Between"; // only allow between rule
					RulesGrid.V.Columns[ValCol].Caption = "Shortest Bar Data Value";
					RulesGrid.V.Columns[ValCol2].Caption = "Longest Bar Data Value";
				}

				else // normal between
				{
					RulesGrid.V.Columns[ValCol].Caption = "Comparison Value";
					RulesGrid.V.Columns[ValCol2].Caption = "\"Between\" High Value";
				}

				string[] list = ops.Split('|');
				RepositoryItemComboBox cb = RulesGrid.V.Columns[OpCol].ColumnEdit as RepositoryItemComboBox;
				cb.Items.Clear();
				cb.Items.AddRange(list);

				//RepositoryItemLookUpEdit lue = RulesGrid.V.Columns[OpCol].ColumnEdit as RepositoryItemLookUpEdit;
				//lue.DataSource = list;
			}

			dt.Columns.Add("BackColor1", typeof(Color));
			dt.Columns.Add("IconImageIdx", typeof(int));

			dt.RowChanged += new DataRowMxChangeEventHandler(DataRowMxChangeEventHandler);

			//if (rfld != null || true)
			//{
			RulesGrid.SetupDefaultQueryManager(dt); // setup underlying QueryManager/QueryTable for current type
			RulesGrid.SetupUnboundColumns(dt);
			//}

			SetColumnVisibilityForColoringStyle(cfStyle);

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

			AddRule.Enabled = EditRule.Enabled = DeleteRuleBut.Enabled =
				MoveRuleUp.Enabled = MoveRuleDown.Enabled = allowMultipleRules;

			BandedGridView.OptionsView.NewItemRowPosition = // allow new rows?
				(allowMultipleRules ? NewItemRowPosition.Bottom : NewItemRowPosition.None);

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
		/// Grid changed, pass along event if requested
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void DataRowMxChangeEventHandler(object sender, DataRowMxChangeEventArgs e)
		{
			FireEditValueChangedEvent();
		}

		/// <summary>
		/// Call any EditValueChanged event
		/// </summary>

		void FireEditValueChangedEvent()
		{
			if (EditValueChanged != null) // fire EditValueChanged event if handlers present
				EditValueChanged(this, EventArgs.Empty);
		}

		/// <summary>
		/// Setup the rules control for results field
		/// </summary>
		/// <param name="rfld"></param>
		/// <param name="rules"></param>

		public void SetupControl(
			ResultsField rfld,
			CondFormatRules rules)
		{
			MetaColumnType columnType = MetaColumnType.Number; // default type
			if (rfld != null) columnType = rfld.QueryColumn.MetaColumn.DataType;

			SetupControl(columnType, rules);
			return;
		}

		/// <summary>
		/// Setup the rules control
		/// </summary>
		/// <param name="columnType"></param>
		/// <param name="rules"></param>

		public void SetupControl(
			MetaColumnType columnType,
			CondFormatRules rules)
		{
			SetupControl(columnType, rules.ColoringStyle);

			SetupRules(0, rules);

			if (ColumnType != MetaColumnType.Structure) // put on first value unless structure which would invoke structure editor
				RulesGrid.EditCell(0, ValCol);

			return;
		}

		/// <summary>
		/// Add to the content of the rules control
		/// </summary>
		/// <param name="firstRow"></param>
		/// <param name="rules"></param>

		public void SetupRules(
			int firstRow,
			CondFormatRules rules)
		{
			DataTableMx dt = DataTable;
			DataRowMx dr;

			InSetRules = true;
			bool saveEnabled = dt.EnableDataChangedEventHandlers(false); // turn off change events while filling

			// Setup for CF coloring style 

			CondFormatStyle colorStyle = rules.ColoringStyle;

			if (colorStyle == CondFormatStyle.ColorScale)
			{
				ColorStyleDropDown.Text = "Continuous Colors";
				ColumnImageCollection = Bitmaps.I.ColorScaleImages;
			}

			else if (colorStyle == CondFormatStyle.DataBar)
			{
				ColorStyleDropDown.Text = "Data Bars";
				ColumnImageCollection = Bitmaps.I.DataBarsImages;
			}

			else if (colorStyle == CondFormatStyle.IconSet)
			{
				ColorStyleDropDown.Text = "Icon Set";
				ColumnImageCollection = Bitmaps.I.IconSetImages;
			}

			else
			{
				ColorStyleDropDown.Text = "Discrete Colors";
				ColumnImageCollection = null;
			}

			ColorStyleDropDown.ImageOptions.ImageIndex = (int)colorStyle; // save current type

// Setup for column type

			if (ColumnType == MetaColumnType.CompoundId && false) // disabled - treat as string for now
			{
				Prompt.Text = // set prompt at top of control for type
					"Enter the name(s) of the saved compound id list(s) " +
					"to be matched against each retrieved compound id.";

				if (rules == null) { InSetRules = false; return; }

				for (int ri = 0; ri < rules.Count; ri++) // fill in the grid
				{
					CondFormatRule r = rules[ri];
					int row = firstRow + ri;

					dr = dt.NewRow();
					dt.Rows.InsertAt(dr, row);

					dr[LabelCol] = r.Name;
					dr[ValCol] = r.Value;
					dr[ValCol2] = r.Value2; // list id, e.g. CNLIST_123
					dr[BackColorCol] = r.BackColor1;
				}
			}

			else if (ColumnType == MetaColumnType.Structure)
			{
				if (rules == null) { InSetRules = false; return; }

				for (int ri = 0; ri < rules.Count; ri++) // fill in the grid
				{
					CondFormatRule r = rules[ri];
					int row = firstRow + ri;

					dr = dt.NewRow();
					dt.Rows.InsertAt(dr, row);

					dr[LabelCol] = r.Name;
					dr[ValCol] = new MoleculeMx(MoleculeFormat.Chime, r.Value);
					dr[BackColorCol] = r.BackColor1;

				}
			}

			else // other data types
			{
				if (rules == null) { InSetRules = false; return; }

				for (int ri = 0; ri < rules.Count; ri++) // fill in the grid
				{
					CondFormatRule r = rules[ri];
					int row = firstRow + ri;

					dr = dt.NewRow();
					dt.Rows.InsertAt(dr, row);

					dr[LabelCol] = r.Name;
					dr[OpCol] = r.Op;
					dr[ValCol] = r.Value;
					dr[ValCol2] = r.Value2;
					dr[BackColorCol] = r.BackColor1;

					if (Lex.IsDefined(r.ImageName))
					{
						dr[IconImageCol] = Bitmaps.GetImageIndexFromName(ColumnImageCollection, r.ImageName);
					}

					else dr[IconImageCol] = DBNull.Value;
				}

				CheckVal2ColSetup();
			}

			RulesGrid.DataSource = dt; // make the data visible
			RulesGrid.AddNewRowAsNeeded();

			dt.EnableDataChangedEventHandlers(saveEnabled);
			InSetRules = false;
			return;
		}

		/// <summary>
		/// Be sure the Val2 column is setup properly for either Between or Within criteria
		/// </summary>

		private void CheckVal2ColSetup()
		{
			if (ColumnType == MetaColumnType.Structure) return;

			RepositoryItemComboBox cb = RulesGrid.V.Columns[ValCol2].ColumnEdit as RepositoryItemComboBox;
			if (cb == null || cb.Items == null) return;

			for (int r = 0; r < DataTable.Rows.Count; r++)
			{
				string op = DataTable.Rows[r][OpCol] as string;
				if (op.IndexOf("Within", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					string list =
						"Day(s)|" +
						"Week(s)|" +
						"Month(s)|" +
						"Year(s)";
					cb.Items.Clear();
					cb.Items.AddRange(list.Split('|'));

					RulesGrid.V.Columns[ValCol2].Caption = "Date Units";
					return;
				}
			}

//			cb.Items.Clear();
//			RulesGrid.V.Columns[ValCol2].Caption = "\"Between\" 2nd Value";
			return;
		}

		/// <summary>
		/// Turn editing on
		/// </summary>

		public void StartEditing()
		{
			try { RulesGrid.V.ShowEditor(); }
			catch (Exception Exception) { return; }
		}

		/// <summary>
		/// Get the rules from the form
		/// </summary>
		/// <returns></returns>

		public CondFormatRules GetRules()
		{
			DataTableMx dt = RulesGrid.DataSource as DataTableMx;
			return GetRules(0, dt.Rows.Count - 1);
		}

		/// <summary>
		/// Get the rules from the form
		/// </summary>
		/// <returns></returns>

		public CondFormatRules GetRules(
			int topRow,
			int bottomRow)
		{
			CondFormatRules rules = new CondFormatRules();
			CondFormatRule rule = null;

			rules.ColoringStyle = GetColoringStyleFromControl();

			if (ColumnType == MetaColumnType.Structure)
			{
				for (int r = topRow; r <= bottomRow; r++)
				{
					MoleculeMx cs = RulesGrid.GetCellMolecule(r, ValCol);
					if (MoleculeMx.IsUndefined(cs)) continue; 

					rule = new CondFormatRule();
					rules.Add(rule);

					rule.Name = RulesGrid.GetCellText(r, LabelCol);
					rule.Op = "SSS";
					rule.OpCode = CondFormatOpCode.SSS;
					rule.Value = cs.GetChimeString();
					rule.BackColor1 = RulesGrid.GetCellBackColor(r, BackColorCol);
				}
			}

			else // non-structure data type
			{
				for (int r = topRow; r <= bottomRow; r++)
				{
					string nameText = RulesGrid.GetCellText(r, LabelCol);
					string opText = RulesGrid.GetCellText(r, OpCol);
					CondFormatOpCode opCode = CondFormatRule.ConvertOpNameToCode(opText);
					string valText = RulesGrid.GetCellText(r, ValCol);
					string val2Text = RulesGrid.GetCellText(r, ValCol2);

					bool valueRequired =
						 opCode != CondFormatOpCode.Null &&
						 opCode != CondFormatOpCode.NotNull &&
						 opCode != CondFormatOpCode.Exists &&
						 opCode != CondFormatOpCode.NotExists;


					if ((valueRequired && Lex.IsUndefined(valText)) ||  // skip if no value && a value is needed
						(!valueRequired && rules.Count == 0)) // or no value required and no rules yet
						continue; 

					rule = new CondFormatRule();
					rules.Add(rule);

					rule.Name = nameText;
					rule.Op = opText;
					rule.OpCode = opCode;

					rule.Value = valText;
					if (MetaColumn.IsNumericMetaColumnType(ColumnType) && !String.IsNullOrEmpty(rule.Value))
						double.TryParse(rule.Value, out rule.ValueNumber);

					else if (ColumnType == MetaColumnType.Date && !String.IsNullOrEmpty(rule.Value))
						rule.ValueNormalized = DateTimeMx.Normalize(rule.Value);

					else if (ColumnType == MetaColumnType.CompoundId && !String.IsNullOrEmpty(rule.Value))
						rule.ValueNormalized = CompoundId.Normalize(rule.Value);

					rule.Value2 = val2Text;
					if (MetaColumn.IsNumericMetaColumnType(ColumnType) && !String.IsNullOrEmpty(rule.Value2))
						double.TryParse(rule.Value2, out rule.Value2Number);
					else if (ColumnType == MetaColumnType.Date && !String.IsNullOrEmpty(rule.Value2))
						rule.Value2Normalized = DateTimeMx.Normalize(rule.Value2);

					rule.BackColor1 = RulesGrid.GetCellBackColor(r, BackColorCol);

					int ii = RulesGrid.GetCellInt(r, IconImageCol);
					if (ii >= 0) rule.ImageName = Bitmaps.GetImageNameFromIndex(ColumnImageCollection, ii);
					else rule.ImageName = "";
				}
			}

			return rules;
		}

		CondFormatStyle GetColoringStyleFromControl()
		{
			return (CondFormatStyle)ColorStyleDropDown.ImageOptions.ImageIndex;
		}

		/// <summary>
		/// Check for valid format
		/// </summary>
		/// <returns></returns>

		public bool AreValid()
		{
			string txt;
			int count = 0;
			DataRowMx dr;

			for (int r = 0; r < DataTable.Rows.Count; r++)
			{
				dr = DataTable.Rows[r];

				string label = dr[LabelCol] as string;

				if (ColumnType == MetaColumnType.CompoundId)
				{
					string value = dr[ValCol] as string;
					if (String.IsNullOrEmpty(label) && String.IsNullOrEmpty(value)) continue; // nothing on line

					if (Lex.IsNullOrEmpty(label) && LabelsRequired)
					{
						XtraMessageBox.Show("A label must be supplied", UmlautMobius.String);
						RulesGrid.EditCell(r, LabelCol);
						return false;
					}

					if (value == "")
					{
						XtraMessageBox.Show("A list name must be supplied", UmlautMobius.String);
						RulesGrid.EditCell(r, ValCol);
						return false;
					}

				}

				else if (ColumnType == MetaColumnType.Structure)
				{
					MoleculeMx mol = dr[ValCol] as MoleculeMx;
					if (MoleculeMx.IsUndefined(mol))
					{
						if (Lex.IsNullOrEmpty(label)) continue; // blank line
						RulesGrid.SelectCell(r, OpCol);
						XtraMessageBox.Show("A substructure query must be supplied", UmlautMobius.String);
						return false;
					}

					if (Lex.IsNullOrEmpty(label) && LabelsRequired)
					{
						XtraMessageBox.Show("A label must be supplied", UmlautMobius.String);
						RulesGrid.EditCell(r, LabelCol);
						return false;
					}
				}

				else // other types
				{
					string op = dr[OpCol] as string;
					if (op == null) op = "";

					CondFormatOpCode opCode = CondFormatRule.ConvertOpNameToCode(op);
					string value = dr[ValCol] as string;
					string value2 = dr[ValCol2] as string;

					if (String.IsNullOrEmpty(op) && String.IsNullOrEmpty(value)) continue; // nothing on line

					if (Lex.IsNullOrEmpty(label) && LabelsRequired)
					{
						XtraMessageBox.Show("A label must be supplied", UmlautMobius.String);
						RulesGrid.EditCell(r, LabelCol);
						return false;
					}

					if (opCode == CondFormatOpCode.Null || opCode == CondFormatOpCode.NotNull || opCode == CondFormatOpCode.Exists)
						continue; // no need to check value

					if (op == "")
					{
						XtraMessageBox.Show("A comparison type must be supplied", UmlautMobius.String);
						RulesGrid.EditCell(r, OpCol);
						return false;
					}

					if (value == "")
					{
						XtraMessageBox.Show("A value must be supplied", UmlautMobius.String);
						RulesGrid.EditCell(r, ValCol);
						return false;
					}

					if (opCode == CondFormatOpCode.Within) // check within date value as integer
					{
						if (!IsValidValue(value, MetaColumnType.Integer, r, ValCol)) return false;
						if (value2 == "")
						{
							XtraMessageBox.Show("A date unit must be supplied", UmlautMobius.String);
							RulesGrid.EditCell(r, ValCol2);
							return false;
						}
					}

					else if (!IsValidValue(value, ColumnType, r, ValCol)) return false;

					if (opCode == CondFormatOpCode.Between || opCode == CondFormatOpCode.NotBetween)
					{
						if (value2 == "")
						{
							XtraMessageBox.Show("An \"Between\" high value must be supplied", UmlautMobius.String);
							RulesGrid.EditCell(r, ValCol2);
							return false;
						}

						else if (!IsValidValue(value2, ColumnType, r, ValCol2)) return false;
					}
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
					RulesGrid.EditCell(r, c);
					return false;
				}
			}

			else if (colType == MetaColumnType.Date)
			{
				if (DateTimeMx.Normalize(val) == null)
				{
					XtraMessageBox.Show("Invalid date", UmlautMobius.String);
					RulesGrid.EditCell(r, c);
					return false;
				}
			}

			return true;
		}

		private void Grid_MouseDown(object sender, MouseEventArgs e)
		{
			int gr, gc = -1, topRow, bottomRow;
			CellInfo ci = null;

			Point p = new Point(e.X, e.Y);
			GridView bgv = RulesGrid.MainView as GridView;
			GridHitInfo ghi = bgv.CalcHitInfo(p);

			gr = ghi.RowHandle;

			if (ghi.Column != null)
			{
				ci = RulesGrid.GetGridCellInfo(gr, ghi.Column); // click within data cells
				gc = ci.GridColAbsoluteIndex;
			}

			else if (ghi.HitTest == GridHitTest.RowIndicator) // clicked in indicator col
			{
				RulesGrid.V.ClearSelection();
				RulesGrid.V.SelectRow(gr);
				RulesGrid.V.FocusedRowHandle = gr;
				LastIndicatorRowClicked = gr;
				gc = -1;
			}

			else return; // clicked in some other area

			if (e.Button == MouseButtons.Right &&
				(RowColInSelectedRows(gr, gc, out topRow, out bottomRow) || ghi.HitTest == GridHitTest.RowIndicator))
			{
				CellRtClickContextMenu.Show(RulesGrid,
					new System.Drawing.Point(e.X, e.Y));
			}

			//else if (gc == BackColorCol || gc == OpCol) { } // handle in mouse up

			else if (ColumnType == MetaColumnType.CompoundId && gc == ValCol) { } // cidList column, handle in mouse up

			else RulesGrid.MoleculeGridControl_MouseDown(sender, e); // let base handle		
		}

		private void Grid_MouseUp(object sender, MouseEventArgs e)
		{
			int ri, ci, topRow, bottomRow;

			Point p = new Point(e.X, e.Y);
			BandedGridView bgv = RulesGrid.MainView as BandedGridView;
			BandedGridHitInfo ghi = bgv.CalcHitInfo(p);

			ri = ghi.RowHandle;
			if (ghi.Column == null) return;
			ci = ghi.Column.AbsoluteIndex;

			GridCell[] cells = bgv.GetSelectedCells();
			if (cells.Length <= 1) // and not an extended selection
			{
				//if (ci == BackColorCol || ci == OpCol) EditCellValue(ri, ci); 
				if (ColumnType == MetaColumnType.CompoundId && ci == ValCol) EditSavedListName(ri, ci);
			}

			return;
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

			const int LabelCol = 0; // position in data table and grid
			const int OpCol = 1;
			const int ValCol = 2;
			const int ValCol2 = 3;
			const int BackColorCol = 4;
			const int IconImageCol = 5;

			if (c == LabelCol)
			{
				return;
			}

			else if (c == ValCol || c == ValCol2)
			{
				bgv.ClearSelection();
				bgv.SelectCell(r, bgv.Columns[c]);
				bgv.FocusedRowHandle = r;
				bgv.FocusedColumn = bgv.Columns[c];
				bgv.ShowEditor(); // doesn't seem to open dropdown
													//Application.DoEvents();
			}

			else if (c == BackColorCol)
			{
				ColorDialog cd = new ColorDialog();
				cd.Color = RulesGrid.GetCellBackColor(r, BackColorCol);
				DialogResult dr = cd.ShowDialog(this);
				if (dr != DialogResult.OK) return;

				CellInfo ci = RulesGrid.GetCellInfo(r, c);
				RulesGrid.DataTable.Rows[ci.DataRowIndex][ci.DataColIndex] = cd.Color;
				bgv.RefreshRowCell(r, ci.GridColumn);

				RulesGrid.AddNewRowAsNeeded();
			}

			else if (c == IconImageCol)
			{
				return;
			}

			return;
		}

		/// <summary>
		/// Edit saved list name for cell
		/// </summary>
		/// <param name="r"></param>
		/// <param name="c"></param>

		private void EditSavedListName(
			int r,
			int c)
		{
			int uoId = 0;

			UserObject uo = null;
			string uoIdString = DataTable.Rows[r][ValCol2] as string;
			if (!String.IsNullOrEmpty(uoIdString))
			{
				if (int.TryParse(uoIdString, out uoId))
				{
					uo = new UserObject(UserObjectType.CnList);
					uo.Id = uoId;
				}
			}

			uo = CidListCommand.SelectListDialog("Browse Lists", uo);
			if (uo == null) return;

			DataTable.Rows[r][ValCol] = uo.Name;
			DataTable.Rows[r][ValCol2] = uo.Id.ToString(); // store id in 2nd col
																										 //Grid.Row = 0; // turn off edit
																										 //Grid.Row = r;

			return;
		}

		private void AddRule_Click(object sender, EventArgs e)
		{
			if (ColumnType == MetaColumnType.Structure)
			{ // show menu that allows import of sd or smiles file
				NewStructureRuleContextMenu.Show(AddRule,
					new System.Drawing.Point(0, AddRule.Size.Height));
			}

			else AddNewRuleMenuItem_Click(sender, e);
		}

		private void AddNewRuleMenuItem_Click(object sender, EventArgs e)
		{
			int r = RulesGrid.V.FocusedRowHandle;
			if (r == GridControl.NewItemRowHandle) // just return if on new row
			{
				RulesGrid.Focus(); // put focus back
				RulesGrid.V.ShowEditor();
				return;
			}

			//if (r == DataTable.Rows.Count - 1) return; // just return if on new row?

			//else if (r == DataTable.Rows.Count - 2) // if on last real row then move to new row
			//  r = DataTable.Rows.Count - 1;

			//else // add new row after current row
			//{
			if (r < 0) r = 0;
			else r = r + 1;
			RulesGrid.InsertRowAt(r);
			//}

			RulesGrid.EditCell(r, LabelCol);
		}

		/// <summary>
		/// Import a list of structures into the condformat rule list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ImportSDFileOrSmilesFileMenuItem_Click(object sender, EventArgs e)
		{
			List<MoleculeMx> csList;

			string fileFilter =
					"Molfile, SDFile, Smiles (*.mol; *.sdf; *.smi)|*.mol; *.sdf; *.smi|All files (*.*)|*.*";

			string fileName = UIMisc.GetOpenFilename(Text, "", fileFilter, ".sdf");
			if (Lex.IsNullOrEmpty(fileName)) return;

			string ext = Path.GetExtension(fileName);
			if (Lex.Eq(ext, ".mol") || Lex.Eq(ext, ".sdf"))
				csList = MoleculeMx.ReadSDFileStructures(fileName);

			else if (Lex.Eq(ext, ".smi"))
				csList = MoleculeMx.ReadSmilesFileStructures(fileName);

			else
			{
				MessageBoxMx.ShowError("File extension must be .mol, .sdf or .smi");
				return;
			}

			CondFormatRules rules = GetRules();
			Color[] colors = GetColors();

			foreach (MoleculeMx cs in csList)
			{
				string ruleName = cs.Id;
				if (Lex.IsNullOrEmpty(ruleName))
					ruleName = "Rule " + (rules.Count + 1);

				CondFormatRule rule = new CondFormatRule();
				rule.Name = ruleName;

				rule.Op = "SSS";
				rule.OpCode = CondFormatOpCode.SSS;
				rule.Value = cs.GetChimeString();
				rule.BackColor1 = colors[rules.Count % colors.Length];

				rules.Add(rule);
			}

			if (ResultsField != null)
				SetupControl(ResultsField, rules);

			else SetupControl(MetaColumnType.Structure, rules);

			return;
		}

		private void EditRule_Click(object sender, EventArgs e)
		{
			EditMenuStrip.Show(EditRule,
				new System.Drawing.Point(0, EditRule.Size.Height));
		}

		private void DeleteRuleBut_Click(object sender, EventArgs e)
		{
			DeleteRule(RulesGrid.Row);
		}

		private void DeleteRule(int row)
		{
			int r = RulesGrid.Row;
			if (r < 0) return;
			RulesGrid.RemoveRowAt(r);
			RulesGrid.AddNewRowAsNeeded();
			RulesGrid.Refresh();
		}

		private void MoveRuleUp_Click(object sender, EventArgs e)
		{
			DataRowMx dr, dr2;

			int r = RulesGrid.Row;
			if (r < 0) return;
			if (r == 0) return;

			dr = RulesGrid.MoveRowUp(r);
			RulesGrid.V.FocusedRowHandle = r - 1;
		}

		private void MoveRuleDown_Click(object sender, EventArgs e)
		{
			DataRowMx dr, dr2;

			int r = RulesGrid.Row;
			if (r < 0) return;
			if (r >= DataTable.Rows.Count - 1) return;

			dr = RulesGrid.MoveRowDown(r);
			RulesGrid.V.FocusedRowHandle = r + 1;
		}

		private void CutMenuItem_Click(object sender, EventArgs e)
		{
			CopySelectedRows();
			DeleteSelectedRows();
		}

		private void CopyMenuItem_Click(object sender, EventArgs e)
		{
			CopySelectedRows();
		}

		private void CopySelectedRows()
		{
			int[] rows = RulesGrid.V.GetSelectedRows();
			if (rows.Length == 0)
			{
				int h = RulesGrid.V.FocusedRowHandle;
				if (h < 0) return;
				rows = new int[1];
				rows[0] = h;
			}

			CondFormatRules rules = GetRules(rows[0], rows[rows.Length - 1]);
			string serializedForm = rules.Serialize();
			Clipboard.SetDataObject(serializedForm, true);
		}

		private void DeleteSelectedRows()
		{
			RulesGrid.V.DeleteSelectedRows();
		}

		private void PasteMenuItem_Click(object sender, EventArgs e)
		{
			int ri = LastIndicatorRowClicked;
			RulesGrid.V.ClearSelection();
			RulesGrid.V.SelectRow(ri);
			RulesGrid.V.FocusedRowHandle = ri;
			PasteCommon();
		}

		void PasteCommon()
		{
			try
			{
				int r = RulesGrid.Row;
				if (r >= 0) DeleteSelectedRows();
				else r = DataTable.Rows.Count; // put at end;

				IDataObject iData = Clipboard.GetDataObject();
				if (!iData.GetDataPresent(DataFormats.Text)) return;
				string serializedForm = (string)iData.GetData(DataFormats.Text);

				CondFormatRules rules = CondFormatRules.Deserialize(serializedForm);
				SetupRules(r, rules);
				//Grid.Select(r, 0, r + rules.Count - 1, Grid.Cols.Count - 1, true);
				//Grid.Focus();
			}
			catch (Exception ex) { return; }
		}

		private void DeleteMenuItem_Click(object sender, EventArgs e)
		{
			DeleteSelectedRows();
		}

		private void SelectAllMenuItem_Click(object sender, EventArgs e)
		{
			RulesGrid.V.SelectAll();
		}

		private void menuCutGridRange_Click(object sender, EventArgs e)
		{
			CutMenuItem_Click(sender, e);
		}

		private void menuCopyGridRange_Click(object sender, EventArgs e)
		{
			CopyMenuItem_Click(sender, e);
		}

		private void menuPasteGridRange_Click(object sender, EventArgs e)
		{
			PasteCommon();
		}

		private void menuInsertRow_Click(object sender, EventArgs e)
		{
			if (RulesGrid.Row == GridControl.NewItemRowHandle) return;
			RulesGrid.InsertRowAt(RulesGrid.Row);
			RulesGrid.EditCell(RulesGrid.Row, LabelCol);
		}

		private void menuDeleteRow_Click(object sender, EventArgs e)
		{
			DeleteRuleBut_Click(sender, e);
		}

		/// <summary>
		/// Setup ColorSet rules and grid
		/// </summary>
		/// <param name="setName"></param>

		private void InitializeColorSetCondFormatRulesAndGrid(
			string setName)
		{
			if (InSetRules) return;

			SetColumnVisibilityForColoringStyle(CondFormatStyle.ColorSet);

			CondFormatRules rules = InitializeColorSetCondFormatRules(setName);

			if (ResultsField != null)
				SetupControl(ResultsField, rules);
			else SetupControl(ColumnType, rules);

			FireEditValueChangedEvent();
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

			CondFormatRules rules = GetRules();

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
			if (InSetRules) return;

			SetColumnVisibilityForColoringStyle(CondFormatStyle.ColorScale);

			CondFormatRules rules = InitializeColorScaleCondFormatRules(ResultsField, setName);

			if (ResultsField != null)
				SetupControl(ResultsField, rules);
			else SetupControl(ColumnType, rules);

			FireEditValueChangedEvent();
			return;
		}

		/// <summary>
		/// Setup color scale conditional formatting for column
		/// </summary>

		public static CondFormat InitializeColorScaleCondFormat(
			ResultsField rfld,
			string setName = null)
		{
			CondFormatRulesControl cfrc = new CondFormatRulesControl();

			CondFormat cf = new CondFormat();
			cf.ColumnType = rfld.QueryColumn.MetaColumn.DataType;
			cf.Rules = cfrc.InitializeColorScaleCondFormatRules(rfld, setName);
			return cf;
		}

		/// <summary>
		/// Setup color scale conditional formatting rules for column
		/// </summary>

		public CondFormatRules InitializeColorScaleCondFormatRules(
			ResultsField rfld,
			string scalename)
		{
			CondFormatRule rule = null;

			CondFormatRules rules = GetRules();
			bool compatible = (rules.ColoringStyle == CondFormatStyle.ColorScale ||
				rules.ColoringStyle == CondFormatStyle.DataBar);

			if (Lex.IsUndefined(scalename)) scalename = CondFormat.DefaultColorScale;
			bool setExists = (Bitmaps.ColorScaleImageColors.ContainsKey(scalename));

			if (!compatible || !setExists) // need new rules set?
			{
				rules = new CondFormatRules(CondFormatStyle.ColorScale);
				if (!setExists) return rules;
			}

			rules.ColoringStyle = CondFormatStyle.ColorScale;
			ColumnImageCollection = Bitmaps.I.ColorScaleImages;

			int ri = 0; // index of first and only rule
			if (ri < rules.Count) rule = rules[ri];

			else
			{
				rule = new CondFormatRule();
				rule.Name = "Rule 1";
				rule.Op = "Between";
				rule.OpCode = CondFormatOpCode.Between;
				rules.Add(rule);
			}

			rule.ImageName = scalename;

			if (rfld != null)
			{
				QueryColumn qc = rfld.QueryColumn;
				ColumnStatistics stats = rfld.GetStats();
				if (qc.MetaColumn.DataType == MetaColumnType.QualifiedNo)
				{ // get basic number from QualifiedNumber
					QualifiedNumberTextElements qnte = QualifiedNumber.ParseToTextElements(stats.MinValue.FormattedText);
					rule.Value = qnte.NumberValue;
					qnte = QualifiedNumber.ParseToTextElements(stats.MaxValue.FormattedText);
					rule.Value2 = qnte.NumberValue;
				}
				else
				{
					rule.Value = stats.MinValue.FormattedText;
					rule.Value2 = stats.MaxValue.FormattedText;
				}

				//if (qc.MetaColumn.MultiPoint) // for IC50 type values reverse coloring so small values are red
				//{
				//	r1.BackColor1 = Color.Red; // normal red
				//	r1.BackColor1 = Color.FromArgb(0, 255, 0); // normal green
				//}

				rules.InitializeInternalMatchValues(qc.MetaColumn.DataType);

				QueryManager qm = qc.QueryTable.Query.QueryManager as QueryManager;
				if (qm != null && qm.DataTableManager != null) // clear formatted values so they get reformatted with new rules
					qm.DataTableManager.ResetFormattedValues(qc);
			}

			return rules;
		}


		/// <summary>
		/// Setup DataBar rules and grid
		/// </summary>
		/// <param name="barsName"></param>

		private void InitializeDataBarCondFormatRulesAndGrid(
				string barsName)
		{
			CondFormatRules rules;

			if (InSetRules) return;

			SetColumnVisibilityForColoringStyle(CondFormatStyle.DataBar);

			rules = InitializeDataBarCondFormatRules(ResultsField, barsName);

			if (ResultsField != null)
				SetupControl(ResultsField, rules);
			else SetupControl(ColumnType, rules);

			FireEditValueChangedEvent();
			return;
		}

		/// <summary>
		/// Setup data bar conditional formatting rules for column
		/// </summary>
		/// <param name="rfld"></param>
		/// <returns></returns>

		public CondFormatRules InitializeDataBarCondFormatRules(
			ResultsField rfld,
			string barsName)
		{
			CondFormatRule rule = null;

			CondFormatRules rules = GetRules();
			bool compatible = (rules.ColoringStyle == CondFormatStyle.ColorScale ||
				rules.ColoringStyle == CondFormatStyle.DataBar);

			bool setExists = (Bitmaps.DataBarsImageColors.ContainsKey(barsName));

			if (!compatible || !setExists) // need new rules set?
			{
				rules = new CondFormatRules(CondFormatStyle.DataBar);
				if (!setExists) return rules;
			}

			rules.ColoringStyle = CondFormatStyle.DataBar;
			ColumnImageCollection = Bitmaps.I.DataBarsImages;

			int ri = 0; // index of first and only rule
			if (ri < rules.Count) rule = rules[ri];

			else
			{
				rule = new CondFormatRule();
				rule.Name = "Rule 1";
				rule.Op = "Between";
				rule.OpCode = CondFormatOpCode.Between;
				rules.Add(rule);
			}

			rule.ImageName = barsName;

			if (rfld != null)
			{
				QueryColumn qc = rfld.QueryColumn;
				ColumnStatistics stats = rfld.GetStats();
				if (qc.MetaColumn.DataType == MetaColumnType.QualifiedNo)
				{ // get basic number from QualifiedNumber
					QualifiedNumberTextElements qnte = QualifiedNumber.ParseToTextElements(stats.MinValue.FormattedText);
					rule.Value = qnte.NumberValue;

					qnte = QualifiedNumber.ParseToTextElements(stats.MaxValue.FormattedText);
					rule.Value2 = qnte.NumberValue;
				}
				else
				{
					rule.Value = stats.MinValue.FormattedText;
					rule.Value2 = stats.MaxValue.FormattedText;
				}

				rules.InitializeInternalMatchValues(qc.MetaColumn.DataType);
				QueryManager qm = qc.QueryTable.Query.QueryManager as QueryManager;
				if (qm != null && qm.DataTableManager != null) // clear formatted values so they get reformatted with new rules
					qm.DataTableManager.ResetFormattedValues(qc);
			}

			return rules;
		}


		/// <summary>
		/// Setup icon set rules and grid
		/// </summary>
		/// <param name="imageNames"></param>

		private void InitializeIconSetCondFormatRulesAndGrid(
				params string[] imageNames)
		{
			CondFormatRules rules;

			if (InSetRules) return;

			SetColumnVisibilityForColoringStyle(CondFormatStyle.IconSet);

			rules = InitializeIconSetCondFormatRules(imageNames);

			if (ResultsField != null)
				SetupControl(ResultsField, rules);
			else SetupControl(ColumnType, rules);
			FireEditValueChangedEvent();
			return;
		}

		/// <summary>
		/// Setup icon set conditional formatting rules for column
		/// </summary>
		/// <param name="rfld"></param>
		/// <returns></returns>

		public CondFormatRules InitializeIconSetCondFormatRules(
			string[] iconNames)
		{
			CondFormatRule rule;

			CondFormatRules rules = GetRules();

			bool compatible = (rules.ColoringStyle == CondFormatStyle.ColorSet ||
				rules.ColoringStyle == CondFormatStyle.IconSet);

			if (!compatible) // need new rules set?
				rules = new CondFormatRules(CondFormatStyle.IconSet);

			rules.ColoringStyle = CondFormatStyle.IconSet;
			ColumnImageCollection = Bitmaps.I.IconSetImages;

			for (int ri = 0; ri < iconNames.Length; ri++)
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

				rule.ImageName = iconNames[ri];
			}

			return rules;
		}

		private void Grid_KeyUp(object sender, KeyEventArgs e)
		{
			int topRow, bottomRow;

			if (RowsSelected(out topRow, out bottomRow)) // handle here if multiple cells selected
			{
				if (e.Modifiers == Keys.Control)
				{
					if (e.KeyCode == Keys.X)
					{
						CutMenuItem_Click(sender, null);
						e.Handled = true;
					}
					else if (e.KeyCode == Keys.C)
					{
						CopyMenuItem_Click(sender, null);
						e.Handled = true;
					}
					else if (e.KeyCode == Keys.V)
					{
						PasteMenuItem_Click(sender, null);
						e.Handled = true;
					}
				}

				else if (e.KeyCode == Keys.Delete && e.Modifiers == 0)
				{
					return; // delete of row is handled by grid control
				}
			}

			if (e.Handled)
			{
				RulesGrid.AddNewRowAsNeeded();
				return;
			}

			RulesGrid.MoleculeGridControl_KeyUp(sender, e); // if not handled than call underlying mol grid
			return;
		}

		/// <summary>
		/// If editing a compoundId or background color column call proper editor
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void bandedGridView1_ShowingEditor(object sender, CancelEventArgs e)
		{
			if (ColumnType == MetaColumnType.CompoundId && RulesGrid.Col == ValCol)
			{ // if saved list name invoke editor
				e.Cancel = true;
				EditSavedListName(RulesGrid.Row, RulesGrid.Col);
				return;
			}

			if (ColumnType == MetaColumnType.Structure && RulesGrid.Col == ValCol)
			{
				e.Cancel = true;
				return;
			}
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

		private void bandedGridView1_CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
		{
			if (RulesGrid.AutoNumberRows && e.Info.IsRowIndicator && e.RowHandle >= 0)
			{
				e.Info.DisplayText = (e.RowHandle + 1).ToString();
				e.Info.ImageIndex = -1; // no image
			}
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

		private void IconChecksBarItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			InitializeIconSetCondFormatRulesAndGrid("Check1", "Check2", "Check3");
		}

		private void ColorStyleDropDown_Click(object sender, EventArgs e)
		{
			Point p = this.PointToScreen(new Point(ColorStyleDropDown.Right, ColorStyleDropDown.Top));
			CfStyleMenu.ShowPopup(MenuBarManager, p);
		}

		private void ColorStyleDropDown_TextChanged(object sender, EventArgs e)
		{
			return; 
		}

	}
}
