using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;
using Mobius.ClientComponents;

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

	public partial class ColumnsSelector : DevExpress.XtraEditors.XtraUserControl
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

		public event EventHandler CallerEditValueChangedHandler; // event to fire when edit value changes

		VisualMsx Visual; // visual we are working with
		AxisMsx Axis; // axis we are working with

		AxisExpressionListMsx AxisExpressionList; // parsed expression (list) we are working on

		internal SpotfireViewManager SVM; // view manager associated with this dialog
		internal SpotfireApiClient Api => SVM?.SpotfireApiClient;
		Query ViewQuery => SVM.BaseQuery; // query associated with view (same as DmQuery?)
		Query BaseQuery => SVM.BaseQuery;
		SpotfireViewProps SVP => SVM?.SVP;
		DataTableMapMsx DataMap => SVP?.DataTableMaps?.CurrentMap; // associated DataMap
		Query DmQuery { get { return DataMap?.Query; } } // query associated with datamap (same as ViewQuery?)
		ColumnMapCollection ColumnMap => DataMap?.ColumnMapCollection;

		public HashSet<MetaColumnType> ExcludedDataTypes { get { return _excludedDataTypes; } } // dict of excluded data types
		HashSet<MetaColumnType> _excludedDataTypes = new HashSet<MetaColumnType>();

		/// <summary>
		/// Basic constructor
		/// </summary>

		public ColumnsSelector()
		{
			InitializeComponent();

			if (SystemUtil.InDesignMode) return;

			OuterPanel.Dock = DockStyle.Fill;
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
/// <param name="svm"></param>
/// <param name="editValueChangedEventHandler"></param>
	
		public void Setup(
			AxisMsx axis,
			VisualMsx visual,
			SpotfireViewManager svm,
			EventHandler editValueChangedEventHandler = null)
		{
			Axis = axis;
			AxisExpressionList = AxisExpressionListMsx.Parse(axis.Expression);
			MultiExpressionSelectionAllowed = axis.MultiExpressionSelectionAllowed; // (not correct)

			Visual = visual;

			SVM = svm;
			CallerEditValueChangedHandler = editValueChangedEventHandler;

			SetupLayoutPanel();

			return;
		}

/// <summary>
/// Setup the LayoutPanel with the list of defined column expressions and button to add an expression as appropriate
/// </summary>

		public void SetupLayoutPanel()
		{

			int cols = AxisExpressionList.ColExprList.Count;
			if (cols == 0) cols++;
			if (MultiExpressionSelectionAllowed) cols++;

			LayoutPanel.Controls.Clear();
			LayoutPanel.ColumnCount = cols;

			for (int ci = cols - 1; ci >= 0; ci--) // insert in reverse order
			{
				ParsedColumnExpressionMsx cx = null;

				if (ci < AxisExpressionList.ColExprList.Count)
					cx = AxisExpressionList.ColExprList[ci];

				else
				{
					cx = new ParsedColumnExpressionMsx();
					cx.AxisExpression = AxisExpressionList;
				}


				DropDownButton b = BuildColumnDropDownButton();
				string exprLabel = GetColumnButtonLabel(cx);
				b.Text = exprLabel; // should be formatted expression instead?
				b.Tag = cx;
				LayoutPanel.Controls.Add(b, ci, 0);
			}

			return;
		}

		public static string GetColumnButtonLabel(ParsedColumnExpressionMsx cx)
		{
			string exprLabel;

			if (cx != null && cx.ColumnNames.Count > 0)
			{
				exprLabel = cx.ColumnNames[0];
				if (Lex.Ne(cx.Expression, exprLabel)  && Lex.Ne(cx.Expression, "[" + exprLabel + "]"))
					exprLabel = cx.Expression;
			}

			else // add button
			{
				exprLabel = "+";
			}

			return exprLabel;
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
			//b.Name = "ModelColButton";
			//b.TabIndex = 4;

			b.DropDownControl = DropDownControlContainer;

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
			return AxisExpressionList.Format();
		}

		/// <summary>
		/// Currently selected QueryColumn
		/// </summary>

		public QueryColumn QueryColumn
		{
			get // return the QueryColumn corresponding to the SelectedMetaColumn
			{
				if (DmQuery == null || MetaColumn == null) return null;
				MetaTable mt = MetaColumn.MetaTable;
				QueryTable qt = DmQuery.GetQueryTableByName(mt.Name);
				if (qt == null) // if not in query create a temp query table so we can return a column
					qt = new QueryTable(mt);
				QueryColumn qc = qt.GetQueryColumnByName(MetaColumn.Name);
				return qc;
			}
		}

		/// <summary>
		/// Currently selected MetaColumn
		/// </summary>

		public MetaColumn MetaColumn
		{
			get { return _MetaColumn; }

			set
			{
				_MetaColumn = value;
				if (value == null)
				{
					//ColumnName.Text = "";
				}

				else
				{
					//ColumnName.Text = value.Label;
				}
			}
		}
		MetaColumn _MetaColumn = null;

		public bool MenuItemSelected = false; // set to true if item is selected as opposed to menu is closed without selection

		public bool ExtraItemSelected { get { return MetaColumn == null && MenuItemSelected; } } // return true if the extra menu item was selected

		private void ColButton_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right) // right button ContextMenu
			{
				DropDownButton b = sender as DropDownButton;
				LastDropDownButtonClicked = b;
				ParsedColumnExpressionMsx cx = b.Tag as ParsedColumnExpressionMsx;
				if (cx == null || cx.ColumnNames.Count == 0) return; // probable the add (+) button 

				List<ParsedColumnExpressionMsx> cxl = cx.AxisExpression.ColExprList;
				int i = cxl.IndexOf(cx);

				MoveExpressionLeftMenuItem.Enabled = (i > 0);
				MoveExpressionRightMenuItem.Enabled = (i >= 0 && i < cxl.Count - 1);
				Point p = new Point(0, b.Height);
				ExpressionRtClickContextMenu.Show(b, p);
			}
		}

		private void ColButton_Click(object sender, EventArgs e)
		{
			DropDownButton b = sender as DropDownButton;
			LastDropDownButtonClicked = b;
			SetupColumnsSelectorDropDown(sender, e);
			Point p = PointToScreen(new Point(b.Location.X, b.Location.Y + b.Height));
			DropDownControlContainer.ShowPopup(p);
			return;
		}

		private void ColButton_ArrowButtonClick(object sender, EventArgs e)
		{
			DropDownButton b = sender as DropDownButton;
			LastDropDownButtonClicked = b;
			SetupColumnsSelectorDropDown(sender, e);
			return;
		}

		private void RemoveExpression_Click(object sender, EventArgs e)
		{
			ParsedColumnExpressionMsx cx = LastDropDownButtonClicked.Tag as ParsedColumnExpressionMsx;
			if (AxisExpressionList.ColExprList.Contains(cx))
				AxisExpressionList.ColExprList.Remove(cx);

			AxisExpressionList.Expression = AxisExpressionList.Format();
			SetupLayoutPanel();
			EditValueChanged();
			return;
		}

		private void MoveExpressionLeftMenuItem_Click(object sender, EventArgs e)
		{
			ParsedColumnExpressionMsx cx = LastDropDownButtonClicked.Tag as ParsedColumnExpressionMsx;
			int i1 = AxisExpressionList.ColExprList.IndexOf(cx);
			if (i1 <= 0) return;

			AxisExpressionList.ColExprList.Remove(cx);
			AxisExpressionList.ColExprList.Insert(i1 - 1, cx);
			AxisExpressionList.Expression = AxisExpressionList.Format();

			SetupLayoutPanel();
			EditValueChanged();
			return;
		}

		private void MoveExpressionRightMenuItem_Click(object sender, EventArgs e)
		{
			ParsedColumnExpressionMsx cx = LastDropDownButtonClicked.Tag as ParsedColumnExpressionMsx;
			int i1 = AxisExpressionList.ColExprList.IndexOf(cx);
			if (i1 >= (AxisExpressionList.ColExprList.Count - 1)) return;

			AxisExpressionList.ColExprList.Remove(cx);
			AxisExpressionList.ColExprList.Insert(i1 + 1, cx);
			AxisExpressionList.Expression = AxisExpressionList.Format();

			SetupLayoutPanel();
			EditValueChanged();
			return;
		}


		private void SetupColumnsSelectorDropDown(object sender, EventArgs e)
		{

			DropDownButton b = sender as DropDownButton;
			ParsedColumnExpressionMsx expr = b.Tag as ParsedColumnExpressionMsx;

			ColumnExpressionSelectorDropDown.Setup(expr, Axis, Visual, b, this, EditValueChanged);
		}

		/// <summary>
		/// Closing control
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DropDownControlContainer_CloseUp(object sender, EventArgs e)
		{
			return;
		}

		/// <summary>
		/// Call any EditValueChanged event
		/// </summary>

		void EditValueChanged(
			object sender = null,
			EventArgs e = null)
		{
			if (CallerEditValueChangedHandler != null) // fire EditValueChanged event if handlers present
				CallerEditValueChangedHandler(this, EventArgs.Empty);
		}

		private void DropDownControlContainer_Popup(object sender, EventArgs e)
		{
			try // try to put focus on SearchControl
			{
				//BeginInvoke(new MethodInvoker(delegate {
				//	ColumnExpressionSelectorDropDown.DataMapControl.SearchControl.Focus();
				//}));
			}

			catch (Exception ex)
			{ ex = ex; }

		}

		private void CloseButton_Click(object sender, EventArgs e)
		{

		}
	} // ColumnsSelector	

}
