using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;
//using Mobius.ClientComponents;

using DevExpress.XtraEditors;
using DevExpress.XtraBars;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Mobius.SpotfireClient
{
	/// <summary>
	/// Select a DataTable column or list of Columns
	/// </summary>

	public partial class ColumnSelectorControl : DevExpress.XtraEditors.XtraUserControl
	{
		public SelectColumnOptions Flags = new SelectColumnOptions();

		[DefaultValue(false)]
		public bool OptionSearchableFieldsOnly { get { return Flags.SearchableOnly; } set { Flags.SearchableOnly = value; } }

		[DefaultValue(false)]
		public bool OptionNongraphicFieldsOnly { get { return Flags.NongraphicOnly; } set { Flags.NongraphicOnly = value; } }

		[DefaultValue(false)]
		public bool OptionExcludeKeys { get { return Flags.ExcludeKeys; } set { Flags.ExcludeKeys = value; } }

		[DefaultValue(false)]
		public bool OptionExcludeImages { get { return Flags.ExcludeImages; } set { Flags.ExcludeImages = value; } }

		[DefaultValue(false)]
		public bool OptionAllowNumericTypesOnly { get { return Flags.ExcludeNonNumericTypes; } set { Flags.ExcludeNonNumericTypes = value; } }

		[DefaultValue(true)]
		public bool OptionIncludeNoneItem { get { return Flags.IncludeNoneItem; } set { Flags.IncludeNoneItem = value; } }


		[DefaultValue(false)]
		public bool MultiExpressionSelectionAllowed { get { return _multiColumnSelectionAllowed; } set { _multiColumnSelectionAllowed = value; } }
		bool _multiColumnSelectionAllowed = false;

		DropDownButton LastDropDownButtonClicked;

		public new event EventHandler Click; // event to fire when control is clicked

		public event EventHandler ValueChangedCallback; // event to fire when edit value changes

		VisualMsx Visual; // visual we are working with
		AxisMsx Axis; // axis we are working with

		ParsedExpressionListMsx ParsedExpressionList; // list of parsed expression (list) we are working on

		internal SpotfireViewProps SVP; // associated Spotfire View Properties
		DataTableMapMsx DataMap => SVP?.DataTableMaps?.CurrentMap; // associated DataMap
		Query DmQuery { get { return DataMap?.Query; } } // query associated with datamap (same as ViewQuery?)
		ColumnMapCollection ColumnMap => DataMap?.ColumnMapCollection;


		public HashSet<MetaColumnType> ExcludedDataTypes { get { return _excludedDataTypes; } } // dict of excluded data types
		HashSet<MetaColumnType> _excludedDataTypes = new HashSet<MetaColumnType>();

		bool InSetup { get => (SetupLevel > 0); set => SetupLevel = (value == true ? SetupLevel + 1 : SetupLevel - 1); }
		int SetupLevel = 0;

		/// <summary>
		/// Basic constructor
		/// </summary>

		public ColumnSelectorControl()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);

			if (SystemUtil.InDesignMode) return;

			ColumnSelectorContainerPanel.Dock = DockStyle.Fill;
		}

		/// <summary>
		/// Allow numeric types only
		/// </summary>

		public void AllowNumericTypesOnly()
		{
			_excludedDataTypes = new HashSet<MetaColumnType>();

			foreach (MetaColumnType mcType in (MetaColumnType[])Enum.GetValues(typeof(MetaColumnType)))
			{
				if (!MetaColumn.IsNumericMetaColumnType(mcType))
					_excludedDataTypes.Add(mcType);
			}

			return;
		}

		/// <summary>
		/// Allow only specified types
		/// </summary>
		/// <param name="allowedTypes"></param>

		public void SetAllowedTypes(params MetaColumnType[] allowedTypes)
		{
			_excludedDataTypes = new HashSet<MetaColumnType>();

			foreach (MetaColumnType mcType in (MetaColumnType[])Enum.GetValues(typeof(MetaColumnType)))
			{
				if (Array.IndexOf(allowedTypes, mcType) < 0)
					_excludedDataTypes.Add(mcType);
			}

			return;
		}

		/// <summary>
		/// Setup
		/// </summary>
		/// <param name="expr"></param>
		/// <param name="svp"></param>
		/// <param name="valueChangedCallback"></param>

		public void Setup(
			AxisMsx axis,
			VisualMsx visual,
			SpotfireViewProps svp,
			EventHandler valueChangedCallback = null)
		{
			InSetup = true;

			try
			{
				Axis = axis;
				ParsedExpressionList = ParsedExpressionListMsx.Parse(axis.Expression);
				MultiExpressionSelectionAllowed = axis.MultiExpressionSelectionAllowed;

				Visual = visual;

				SVP = svp;
				ValueChangedCallback = valueChangedCallback;

				SetupLayoutPanel();

				return;
			}

			finally { InSetup = false; }
		}

		/// <summary>
		/// Setup the LayoutPanel with the list of defined column expressions and button to add an expression as appropriate
		/// </summary>

		public void SetupLayoutPanel()
		{
			InSetup = true;

			int cols = ParsedExpressionList.ColExprList.Count;
			if (cols == 0) cols++;
			if (MultiExpressionSelectionAllowed) cols++;

			LayoutPanel.Controls.Clear();
			LayoutPanel.ColumnCount = cols;

			for (int ci = cols - 1; ci >= 0; ci--) // insert in reverse order
			{
				ParsedColumnExpressionMsx cx = null;

				if (ci < ParsedExpressionList.ColExprList.Count)
					cx = ParsedExpressionList.ColExprList[ci];

				else // the Add-Column control
				{
					cx = new ParsedColumnExpressionMsx(); // column not assigned 
					cx.ParentAxisExpressionList = ParsedExpressionList;
				}

				DropDownButton b = BuildColumnDropDownButton(); // build the basic dropdown button control
				string exprLabel = FormatColumnButtonLabel(cx);
				b.Text = exprLabel;
				b.Tag = cx;
				LayoutPanel.Controls.Add(b, ci, 0);
			}

			InSetup = false;

			return;
		}

		public static string FormatColumnButtonLabel(ParsedColumnExpressionMsx cx)
		{
			string colLabel;

			if (cx != null && cx.EscapedColumnAndTableNames.Count > 0)
			{
				colLabel = cx.GetColumnLabel(); //.FormatExpressionLabel();
			}

			else // add button
			{
				colLabel = "+";
			}

			return colLabel;
		}

		DropDownButton BuildColumnDropDownButton()
		{
			DropDownButton b = new DropDownButton();

			b.Appearance.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			b.Appearance.Options.UseFont = true;
			b.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			b.Size = new System.Drawing.Size(99, 20);
			b.AutoSize = true;
			b.Location = new System.Drawing.Point(2, 2);
			b.Margin = new System.Windows.Forms.Padding(2);

			b.ActAsDropDown = false;
			b.DropDownControl = null;

			b.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ColButton_MouseDown);
			b.Click += new System.EventHandler(this.ColButton_Click);
			b.ArrowButtonClick += new System.EventHandler(this.ColButton_ArrowButtonClick);

			return b;
		}

		/// <summary>
		/// Build the LayoutPanel containing the buttons for each column
		/// </summary>

		public void BuildLayoutPanel()
		{
			DropDownButton b = CreateColumnDropDownButton();
			int r = 0;
			int c = 0;
			LayoutPanel.Controls.Add(b, c, r);
			return;
		}

		/// <summary>
		/// Create a Column DropDown button for a Column
		/// </summary>
		/// <returns></returns>

		public DropDownButton CreateColumnDropDownButton()
		{
			DropDownButton b = new DevExpress.XtraEditors.DropDownButton();

			b.Appearance.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			b.Appearance.Options.UseFont = true;
			b.AutoSize = true;
			b.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			b.Location = new System.Drawing.Point(0, 0);
			b.Margin = new System.Windows.Forms.Padding(0);
			b.Name = "ModelColButton";
			b.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
			b.Size = new System.Drawing.Size(103, 20);
			b.TabIndex = 4;
			b.Text = "Sum(T2.Amw)";

			return b;
		}

		/// <summary>
		/// Get the text of the expression for the axis
		/// </summary>
		/// <returns></returns>

		public string GetAxisExpression()
		{
			return ParsedExpressionList.Format();
		}

		/// <summary>
		/// GetFirstSelectedDataColumn
		/// </summary>
		/// <returns></returns>

		public DataColumnMsx GetFirstSelectedDataColumn()
		{
			if (ParsedExpressionList?.ColExprList == null || ParsedExpressionList.ColExprList.Count == 0) return null;

			ParsedColumnExpressionMsx pce = ParsedExpressionList.ColExprList[0];
			if (pce.EscapedColumnAndTableNames.Count == 0) return null;

			DataColumnMsx dc = GetDataColumnFromName(pce.EscapedColumnAndTableNames[0]);
			return dc;
		}

		/// <summary>
		/// Get datacolumn from a name
		/// If a [tableName].[colName] then parse & lookup by the table name
		/// If [colName] or colName then use the main table from the Visual as the table to lookup the column in
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public DataColumnMsx GetDataColumnFromName(string name)
		{
			string tableName, colName;
			DataColumnMsx dc;

			if (Lex.IsUndefined(name)) return null;

			DataTableMsx dt = Visual.Data.DataTableReference;

			Lex.Split(name, ".", out tableName, out colName);
			if (Lex.IsDefined(tableName) && Lex.IsDefined(colName))
			{
				if (!SVP.AnalysisApp.Doc_DataManager.Doc_Tables.TryGetTableByName(tableName, out dt))
					return null;
			}

			if (dt.TryGetColumnByName(colName, out dc))
				return dc;

			else return null;
		}

		public bool MenuItemSelected = false; // set to true if item is selected as opposed to menu is closed without selection

		//public bool ExtraItemSelected { get { return MetaColumn == null && MenuItemSelected; } } // return true if the extra menu item was selected

		private void ColButton_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right) // right button ContextMenu
			{
				DropDownButton b = sender as DropDownButton;
				LastDropDownButtonClicked = b;
				ParsedColumnExpressionMsx cx = b.Tag as ParsedColumnExpressionMsx;
				if (cx == null || cx.EscapedColumnAndTableNames.Count == 0) return; // probably the add (+) button 

				List<ParsedColumnExpressionMsx> cxl = cx.ParentAxisExpressionList.ColExprList;
				int i = cxl.IndexOf(cx);

				MoveExpressionLeftMenuItem.Enabled = (i > 0);
				MoveExpressionRightMenuItem.Enabled = (i >= 0 && i < cxl.Count - 1);
				Point p = new Point(0, b.Height);
				ExpressionRtClickContextMenu.Show(b, p);
				return;
			}

			else
			{
				//LastDropDownButtonClicked = sender as DropDownButton;
				//DelayedCallback.Schedule(SetupAndShowColumnSelectorDropdown);
			}
		}

		private void ColButton_Click(object sender, EventArgs e)
		{
			LastDropDownButtonClicked = sender as DropDownButton;
			DelayedCallback.Schedule(SetupAndShowColumnSelectorDropdown);
			return;
		}

		private void ColButton_ArrowButtonClick(object sender, EventArgs e)
		{
			LastDropDownButtonClicked = sender as DropDownButton;
			DelayedCallback.Schedule(SetupAndShowColumnSelectorDropdown);
			return;
		}

		void SetupAndShowColumnSelectorDropdown()
		{
			InSetup = true;

			try
			{
				DropDownButton b = LastDropDownButtonClicked;
				ParsedColumnExpressionMsx parsedExpr = b.Tag as ParsedColumnExpressionMsx;
				List<DataTableMsx> allowedDataTables = Axis.GetAllowedDataTables();

				DropdownColumnSelector.Setup(parsedExpr, allowedDataTables, Visual.Data.DataTableReference, b, this, DropdownControlEventCallback);

				Point p = PointToScreen(new Point(b.Location.X, b.Location.Y + b.Height + 2));
				DropdownControlContainer.ShowPopup(p);
				DropdownColumnSelector.ColumnSearchControl.Focus();
				return;
			}

			finally { InSetup = false; }
		}

		private void RemoveExpression_Click(object sender, EventArgs e)
		{
			RemoveLastSelectedExpression();
		}

		void RemoveLastSelectedExpression()
		{
			ParsedColumnExpressionMsx cx = LastDropDownButtonClicked.Tag as ParsedColumnExpressionMsx;
			if (ParsedExpressionList.ColExprList.Contains(cx))
				ParsedExpressionList.ColExprList.Remove(cx);

			ParsedExpressionList.Expression = ParsedExpressionList.Format();
			SetupLayoutPanel();

			ExpressionListChanged();
			return;
		}

		private void RemoveAllMenuItem_Click(object sender, EventArgs e)
		{
			ParsedExpressionList.Expression = "";

			SetupLayoutPanel();

			ExpressionListChanged();
			return;
		}


		private void MoveExpressionLeftMenuItem_Click(object sender, EventArgs e)
		{
			ParsedColumnExpressionMsx cx = LastDropDownButtonClicked.Tag as ParsedColumnExpressionMsx;
			int i1 = ParsedExpressionList.ColExprList.IndexOf(cx);
			if (i1 <= 0) return;

			ParsedExpressionList.ColExprList.Remove(cx);
			ParsedExpressionList.ColExprList.Insert(i1 - 1, cx);
			ParsedExpressionList.Expression = ParsedExpressionList.Format();

			SetupLayoutPanel();

			ExpressionListChanged();
			return;
		}

		private void MoveExpressionRightMenuItem_Click(object sender, EventArgs e)
		{
			ParsedColumnExpressionMsx cx = LastDropDownButtonClicked.Tag as ParsedColumnExpressionMsx;
			int i1 = ParsedExpressionList.ColExprList.IndexOf(cx);
			if (i1 >= (ParsedExpressionList.ColExprList.Count - 1)) return;

			ParsedExpressionList.ColExprList.Remove(cx);
			ParsedExpressionList.ColExprList.Insert(i1 + 1, cx);
			ParsedExpressionList.Expression = ParsedExpressionList.Format();

			SetupLayoutPanel();

			ExpressionListChanged();
			return;
		}


		/// <summary>
		/// Callback from the ColumnSelectorDropDown
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void DropdownControlEventCallback(
			object sender = null,
			string arg = null)
		{
			if (Lex.Eq(arg, "EditValueChanged"))
			{
				DropDownButton b = LastDropDownButtonClicked;

				ParsedColumnExpressionMsx pce = b.Tag as ParsedColumnExpressionMsx;
				if (pce == null) return;

				bool newColumn = (b.Text == "+");

				if (pce.EscapedColumnAndTableNames.Count > 0)
					b.Text = pce.FormatUnescapedExpression();

				else b.Text = "(None)";

				if (newColumn)
				{
					// todo
				}

				ValueChanged();
				return;
			}

			else if (Lex.Eq(arg, "CloseButtonClicked"))
			{
				DropdownControlContainer.HidePopup();
				return;
			}

			else if (Lex.Eq(arg, "RemoveButtonClicked"))
			{
				RemoveLastSelectedExpression();
				return;
			}

			else throw new ArgumentException("Unexpected event: " + arg);
		}

		/// <summary>
		/// The number or order of the items in the expression list changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void ExpressionListChanged(
			object sender = null,
			EventArgs e = null)
		{
			// todo?

			ValueChanged();
			return;
		}

		/// <summary>
		/// Call ValueChangedCallback if defined
		/// </summary>

		void ValueChanged(
			object sender = null,
			EventArgs e = null)
		{
			if (InSetup) return;

			if (ValueChangedCallback != null) // fire EditValueChanged event if handler is defined
				ValueChangedCallback(this, EventArgs.Empty);
			return;
		}

	} // ColumnsSelector	

}
