using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Select a field from the database or the current query
	/// </summary>

	public partial class FieldSelectorControl : DevExpress.XtraEditors.XtraUserControl
	{

// Options that appear as properties for the control in design mode

		[DefaultValue(false)]
		public bool OptionSelectFromQueryOnly { get => _selectFromQueryOnly; set => _selectFromQueryOnly = value;  }
		bool _selectFromQueryOnly = false; // select from query only

		[DefaultValue(true)]
		public bool OptionShowTableAndFieldLabels { get => _showTableAndFieldLabels; set => _showTableAndFieldLabels = value; }
		bool _showTableAndFieldLabels = true; // show the table and field labels before the names

		[DefaultValue(false)]
		public bool OptionSearchableFieldsOnly { get => _searchableFieldsOnly; set => _searchableFieldsOnly = value; }
		bool _searchableFieldsOnly = false;

		[DefaultValue(false)]
		public bool OptionNongraphicFieldsOnly { get => _nongraphicFieldsOnly; set => _nongraphicFieldsOnly = value; }
		bool _nongraphicFieldsOnly = false;

		[DefaultValue(false)]
		public bool OptionFirstTableKeyOnly { get => _firstTableKeyOnly; set => _firstTableKeyOnly = value; }
		bool _firstTableKeyOnly = false;

		[DefaultValue(false)]
		public bool OptionExcludeKeys { get => _excludeKeys; set => _excludeKeys = value; }
		bool _excludeKeys = false;

		[DefaultValue(false)]
		public bool OptionExcludeImages { get => _excludeImages; set => _excludeImages = value; }
		bool _excludeImages = false;

		[DefaultValue(false)]
		public bool OptionIncludeAllSelectableColumns { get => _includeAllSelectableColumns; set => _includeAllSelectableColumns = value; }
		bool _includeAllSelectableColumns = false; // include all selectable columns for possible display not just selected cols

		[DefaultValue(true)]
		public bool OptionCheckMarkDefaultColumn { get => _checkMarkDefaultColumn; set => _checkMarkDefaultColumn = value; }
		bool _checkMarkDefaultColumn = true; // normally checkmark the currently selected or default column

		[DefaultValue(false)]
		public bool OptionDisplayInternalMetaTableName { get => _displayInternalMetaTableName; set => _displayInternalMetaTableName = value; }
		bool _displayInternalMetaTableName = false; // normally checkmark the currently selected or default column\

		[DefaultValue(false)]
		public bool OptionExcludeNonNumericTypes { get => _excludeNonNumericTypes; set => _excludeNonNumericTypes = value; }
		bool _excludeNonNumericTypes = false;

		[DefaultValue(false)]
		public bool OptionExcludeNumericTypes { get => _excludeNumericTypes; set => _excludeNumericTypes = value; }
		bool _excludeNumericTypes = false;

		public HashSet<MetaColumnType> AllowedDataTypes { get => _allowedDataTypes; } // dict of allowed data types
		HashSet<MetaColumnType> _allowedDataTypes = new HashSet<MetaColumnType>();

		[DefaultValue(false)]
		public bool OptionSelectSummarizedDataByDefaultIfExists { get => _selectSummarizedDataByDefaultIfExists; set => _selectSummarizedDataByDefaultIfExists = value; }
		// if both summarized & unsummarized data exists and
		//  - true: select summarized data by default
		//  - false: show menu to allow user to select which they want
		bool _selectSummarizedDataByDefaultIfExists = false;

		[DefaultValue(true)]
		public bool OptionIncludeNoneItem { get => _includeNoneItem; set => _includeNoneItem = value; }
		bool _includeNoneItem = true; // all selection of nothing

		// Other members

		public new event EventHandler Click; // event to fire when control is clicked

		public event EventHandler CallerEditValueChangedHandler; // event to fire when edit value changes

		public Query Query; // associated query

		/// <summary>
		/// Currently selected QueryColumn
		/// Note that the current QueryColumn is syncronized with the MetaColumn below 
		/// to allow either to be the primary focus of activity
		/// </summary>

		public QueryColumn QueryColumn
		{
			get // return the QueryColumn corresponding to the SelectedMetaColumn
			{
				if (_queryColumn?.MetaColumn == _metaColumn)
					return _queryColumn; // in synch

				if (Query == null || MetaColumn == null) return null;

				QueryColumn qc = Query.GetFirstMatchingQueryColumnByMetaColumn(MetaColumn);
				if (qc == null) // if not in query create a temp query table so we can return a column
				{
					QueryTable qt = new QueryTable(MetaColumn.MetaTable);
					qc = qt.GetQueryColumnByName(MetaColumn.Name);
				}
				_queryColumn = qc; // new cached synched value
				return qc;
			}

			set
			{
				MetaColumn = value?.MetaColumn;
				_queryColumn = value; // must set after setting metacolumn
			}
		}
		QueryColumn _queryColumn = null;

		/// <summary>
		/// Currently selected MetaColumn - Synchronized with QueryColumn above
		/// </summary>

		public MetaColumn MetaColumn
		{
			get => _metaColumn; 

			set
			{
				_metaColumn = value;
				_queryColumn = null; // force new lookup of QC

				if (value != null) // update controls to match the new MC
				{
					TableName.Text = value.MetaTable.Label; // 
					FieldName.Text = value.Label;
				}

				else
				{
					TableName.Text = "";
					FieldName.Text = "";
				}
			}
		}
		MetaColumn _metaColumn = null;

		public QnfEnum Subcolumn = QnfEnum.Undefined; // currently selected subcolumn if subcolumn selection is being used

		public bool MenuItemSelected = false; // set to true if item is selected as opposed to menu is closed without selection

		public bool NoneItemSelected = false; // (None) item was selected

		public bool ExtraItemSelected = false; // added extra item was selected

		static bool SubcolumnUseEnabled = false; // don't allow subcolumn use for now
		
		/// <summary>
		/// Basic constructor
		/// </summary>

		public FieldSelectorControl()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);
		}

		/// <summary>
		/// Setup a FieldSelectorControl
		/// </summary>
		/// <param name="query"></param>
		/// <param name="qc"></param>

		public void Setup(
			Query query,
			QueryColumn qc)
		{
			Query = query;

			QueryColumn = qc;

			OptionSelectFromQueryOnly = true;
			//OptionShowTableAndFieldLabels = true;
			return;
		}

		/// <summary>
		/// Set focus to control
		/// </summary>

		new public bool Focus()
		{
			return SelectField.Focus();
		}

		/// <summary>
		/// Select a QueryColumn from a Query
		/// </summary>
		/// <param name="query"></param>
		/// <param name="currentQc"></param>
		/// <param name="flags"></param>
		/// <param name="screenX"></param>
		/// <param name="screenY"></param>
		/// <param name="selectedQc">Selected QueryColumn or null if none item is selected</param>
		/// <returns></returns>

		public DialogResult SelectColumnFromQuery(
			Query query,
			QueryColumn currentQc,
			SelectColumnOptions flags,
			int screenX,
			int screenY,
			out QueryColumn selectedQc)
		{

			QnfEnum selectedSubColumn;
			return SelectColumnFromQuery(query, currentQc, flags, screenX, screenY, out selectedQc, out selectedSubColumn);
		}

		/// <summary>
		/// Select a QueryColumn and optional subcolumn from a Query
		/// </summary>
		/// <param name="query"></param>
		/// <param name="currentQc"></param>
		/// <param name="flags"></param>
		/// <param name="screenX"></param>
		/// <param name="screenY"></param>
		/// <param name="selectedQc">Selected QueryColumn or null if none item is selected</param>
		/// <param name="selectedSubColumn">Selected subcolumn if defined</param>
		/// <returns></returns>

		public DialogResult SelectColumnFromQuery(
			Query query,
			QueryColumn currentQc,
			SelectColumnOptions flags,
			int screenX,
			int screenY,
			out QueryColumn selectedQc,
			out QnfEnum selectedSubColumn)
		{
			QueryTable qt;
			MetaTable mt;
			QueryColumn qc;
			ToolStripMenuItem tmi = null, fmi = null;

			if (Click != null) // fire Clicked event if handlers present
				Click(this, EventArgs.Empty);

			selectedQc = null;
			selectedSubColumn = QnfEnum.Undefined;

			Query = query;
			QueryColumn = currentQc; // set QueryColumn and MetaColumn

			SelectFieldMenu.Items.Clear();

			List<QueryTable> qts = new List<QueryTable>();

			if (query != null) // get all tables from query to show in menu
			{
				qts = new List<QueryTable>(query.Tables);
			}

			MetaTable selectedMt = QueryColumn?.MetaColumn?.MetaTable; // if selected metatable not included in table list then add it
			if (selectedMt != null && query.GetQueryTableByName(selectedMt.Name) == null)
				qts.Insert(0, new QueryTable(selectedMt));

			for (int qti = 0; qti < qts.Count; qti++)
			{
				qt = qts[qti];
				mt = qt.MetaTable;

				if (qts.Count > 1)
				{
					tmi = new ToolStripMenuItem();
					if (currentQc != null && qt.MetaTable == currentQc.MetaColumn.MetaTable) tmi.Checked = true;
					else
					{
						tmi.Image = global::Mobius.ClientComponents.Properties.Resources.Table;
						tmi.ImageTransparentColor = System.Drawing.Color.Cyan;
					}

					tmi.Text = qt.ActiveLabel;
					if (OptionDisplayInternalMetaTableName) tmi.Text += " (" + qt.MetaTable.Name + ")";
				}

				else tmi = null;

				for (int qci = 0; qci < qt.QueryColumns.Count; qci++)
				{ 
					qc = qt.QueryColumns[qci];
					MetaColumn mc = qc.MetaColumn;
					MetaColumnType mcType = mc.DataType;

					bool isSelectable = (flags.IncludeAllSelectableCols &&
						(qc.MetaColumn.InitialSelection == ColumnSelectionEnum.Selected ||
						 qc.MetaColumn.InitialSelection == ColumnSelectionEnum.Unselected));

					if (!qc.Selected && !isSelectable)
						continue; // must be selected or have default cols requested

					if (flags.ExcludeKeys &&
						mc.IsKey) continue;

					if (flags.FirstTableKeyOnly && mc.IsKey && qti > 0)
						continue;

					if (flags.SearchableOnly &&
						mcType == MetaColumnType.Image) continue;

					if (flags.NongraphicOnly && (
						mcType == MetaColumnType.Structure ||
						mcType == MetaColumnType.Image)) continue;

					if (flags.ExcludeNonNumericTypes && !mc.IsNumeric)
						continue;

					if (flags.ExcludeNumericTypes && mc.IsNumeric)
						continue;

					if (flags.AllowedDataTypes != null && flags.AllowedDataTypes.Count > 0 && !flags.AllowedDataTypes.Contains(mcType))
						continue;

					QnfEnum subcolsAllowed = flags.QnSubcolsToInclude;

					if (mcType != MetaColumnType.QualifiedNo || QnSubcolumns.IsCombinedFormat(subcolsAllowed)) // display as single column with default type
					{
						AddMenuItem(qc, QnfEnum.Combined, qc.ActiveLabel, subcolsAllowed, tmi); 
					}

					else // display just the numeric type of a QualifiedNumer column
					{
						AddMenuItem(qc,QnfEnum.NumericValue, qc.ActiveLabel, subcolsAllowed, tmi);

						if (SubcolumnUseEnabled)
						{
							AddMenuItem(qc, QnfEnum.Qualifier, " " + UnicodeString.Bullet + " Qualifier (i.e.: <, >) (Q)", subcolsAllowed, tmi);
							if (mt.SummarizedExists && mt.UseSummarizedData) // add additional cols available for summarized data
							{
								AddMenuItem(qc, QnfEnum.NValue, " " + UnicodeString.Bullet + " Number Tested (N)", subcolsAllowed, tmi);
							}
						}
					}

				}

				if (tmi != null && tmi.DropDownItems.Count > 0)
					SelectFieldMenu.Items.Add(tmi);
			}

			if (!flags.SelectFromQueryOnly) // allow selection from database
			{
				if (SelectFieldMenu.Items.Count > 0)
					SelectFieldMenu.Items.Add(new ToolStripSeparator());
				SelectFieldMenu.Items.Add(SelectFromDatabaseContentsTreeMenuItem);
			}

			if (flags.IncludeNoneItem)
			{ // added none item
				SelectFieldMenu.Items.Add(new ToolStripSeparator());
				tmi = new ToolStripMenuItem();
				tmi.Text = "(None)";
				tmi.Click += new System.EventHandler(this.FieldMenuItemNone_Click);
				SelectFieldMenu.Items.Add(tmi);
			}

			if (!String.IsNullOrEmpty(flags.ExtraItem))
			{ // added extra item
				SelectFieldMenu.Items.Add(new ToolStripSeparator());
				tmi = new ToolStripMenuItem();
				tmi.Text = flags.ExtraItem;
				tmi.Click += new System.EventHandler(this.FieldMenuItemExtra_Click);
				SelectFieldMenu.Items.Add(tmi);
			}

			MenuItemSelected = NoneItemSelected = ExtraItemSelected = false;

			SelectFieldMenu.Show(screenX, screenY);
			SelectFieldMenu.Focus();

			while (SelectFieldMenu.Visible) // wait til menu closes
			{
				Application.DoEvents();
				Thread.Sleep(100);
			}

			if (!MenuItemSelected) return DialogResult.Cancel; // treat as cancel if nothing selected

			CheckForSummarizedVersionOfMetaColumn();

			if (QueryColumn != null && query != null && query.Tables.Count > 0 &&
				flags.FirstTableKeyOnly && QueryColumn.IsKey)
			{ // if selected key & must be from first table then be sure it is
				qt = query.Tables[0];
				QueryColumn = qt.KeyQueryColumn;
			}

			if (CallerEditValueChangedHandler != null) // fire EditValueChanged event if handlers present
				CallerEditValueChangedHandler(this, EventArgs.Empty);

			selectedQc = QueryColumn;
			selectedSubColumn = Subcolumn; 
			return DialogResult.OK;
		}

		/// <summary>
		/// AddMenuItem
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="subcolType"></param>
		/// <param name="subcolSuffix"></param>
		/// <param name="subcolMcType"></param>
		/// <param name="subcolsAllowed"></param>
		/// <param name="tmi"></param>
		/// <param name="selectedMc"></param>
		/// <returns></returns>

		ToolStripMenuItem AddMenuItem(
			QueryColumn qc,
			QnfEnum subcolType,
			string colLabel,
			QnfEnum subcolsAllowed,
			ToolStripMenuItem tmi)
		{
			Image typeImage = null;

			if (!QnSubcolumns.IsCombinedFormat(subcolType) && (subcolType & subcolsAllowed) == 0) return null;

			MetaColumn mc = qc.MetaColumn;
			MetaColumnType mcType = mc.DataType;
			typeImage = Bitmaps.Bitmaps16x16.Images[(int)mc.DataTypeImageIndex];

			if (mcType == MetaColumnType.QualifiedNo && !QnSubcolumns.IsCombinedFormat(subcolType))
			{
				mcType = QnSubcolumns.GetMetaColumnType(subcolType);
				typeImage = Bitmaps.Bitmaps16x16.Images[(int)MetaColumn.GetMetaColumnDataTypeImageIndex(mcType)];
			}

			ToolStripMenuItem fmi = new ToolStripMenuItem();

			if (qc == QueryColumn && (subcolType == Subcolumn || Subcolumn == QnfEnum.Undefined || !SubcolumnUseEnabled))
				fmi.Checked = true; 

			else
			{
				fmi.Image = typeImage;
				fmi.ImageTransparentColor = System.Drawing.Color.Cyan;
			}

			fmi.Text = colLabel;
			fmi.Tag = new KeyValuePair<QueryColumn, QnfEnum>(qc, subcolType);
			fmi.Click += new System.EventHandler(this.FieldMenuItem_Click);

			if (tmi != null) tmi.DropDownItems.Add(fmi);
			else SelectFieldMenu.Items.Add(fmi);

			return fmi;
		}


		/// <summary>
		///  Select a QueryColumn from a single QueryTable
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="currentQc"></param>
		/// <param name="flags"></param>
		/// <param name="screenX"></param>
		/// <param name="screenY"></param>
		/// <param name="selectedQc"></param>
		/// <returns></returns>

		public DialogResult SelectColumnFromQueryTable(
			QueryTable qt,
			QueryColumn currentQc,
			SelectColumnOptions flags,
			int screenX,
			int screenY,
			out QueryColumn selectedQc,
			out QnfEnum selectedSubcolumn)
		{
			Query q0 = qt.Query;

			Query q = new Query(); // build temp query with just one table so only a menu of fields from that table are shown
			q.AddQueryTable(qt);

			DialogResult dr = SelectColumnFromQuery(q, currentQc, flags, screenX, screenY, out selectedQc, out selectedSubcolumn);

			qt.Query = q0;

			return dr;
		}

		/// <summary>
		/// Selected a field item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void FieldMenuItem_Click(object sender, EventArgs e)
		{
			KeyValuePair<QueryColumn, QnfEnum> kvp = (KeyValuePair<QueryColumn, QnfEnum>)(sender as ToolStripMenuItem).Tag;
			QueryColumn = kvp.Key;
			Subcolumn = kvp.Value;

			MenuItemSelected = true;
		}

		/// <summary>
		/// Selected no item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void FieldMenuItemNone_Click(object sender, EventArgs e)
		{
			QueryColumn = null;
			Subcolumn = QnfEnum.Undefined;
			NoneItemSelected = true;
			MenuItemSelected = true;
		}

		/// <summary>
		/// Selected extra item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void FieldMenuItemExtra_Click(object sender, EventArgs e)
		{
			QueryColumn = null;
			Subcolumn = QnfEnum.Undefined;
			ExtraItemSelected = true;
			MenuItemSelected = true;
		}

		/// <summary>
		/// Go to full contents tree to select item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void SelectFromDatabaseContentsTreeMenuItem_Click(object sender, EventArgs e)
		{
			MetaColumn mc = SelectFieldFromContents.ShowDialog(null, null, QueryColumn?.MetaColumn, OptionSelectSummarizedDataByDefaultIfExists, OptionCheckMarkDefaultColumn);
			if (mc != null && Query != null)
			{
				MetaColumn = mc;
				MenuItemSelected = true;
			}
		}

		private void SelectField_Click(object sender, EventArgs e)
		{
			SelectField.Focus(); // be sure focus is on us rather than one of the text boxes

			Point p = SelectField.PointToScreen(new Point(0, SelectField.Height));
			ShowMenu(p);
		}

		/// <summary>
		/// Direct call to select field
		/// </summary>
		/// <returns></returns>

		public QueryColumn ShowMenu(Point screenLoc)
		{
			QueryColumn selectedQc = null;

			SelectColumnOptions flags = new SelectColumnOptions();
			flags.SearchableOnly = OptionSearchableFieldsOnly;
			flags.NongraphicOnly = OptionNongraphicFieldsOnly;
			flags.FirstTableKeyOnly = OptionFirstTableKeyOnly;
			flags.ExcludeKeys = OptionExcludeKeys;
			flags.ExcludeImages = OptionExcludeImages;
			flags.SelectFromQueryOnly = OptionSelectFromQueryOnly;
			flags.IncludeAllSelectableCols = OptionIncludeAllSelectableColumns;
			flags.IncludeNoneItem = OptionIncludeNoneItem;
			flags.ExcludeNonNumericTypes = OptionExcludeNonNumericTypes;
			flags.ExcludeNumericTypes = OptionExcludeNumericTypes;
			flags.AllowedDataTypes = AllowedDataTypes;

			DialogResult dr = SelectColumnFromQuery(Query, QueryColumn, flags, screenLoc.X, screenLoc.Y, out selectedQc);
			return selectedQc;
		}

		private void TableName_VisibleChanged(object sender, EventArgs e)
		{
			if (!Visible) return;

			if (!OptionShowTableAndFieldLabels) // hide the table and field labels if requested
			{
				TableLabel.Visible = false;
				TableName.Width = TableName.Right;
				TableName.Left = 0;

				FieldLabel.Visible = false;
				FieldName.Width = FieldName.Right;
				FieldName.Left = 0;
			}
		}

		private void TableName_MouseDown(object sender, MouseEventArgs e)
		{
			SelectField_Click(null, null);
		}

		private void TableName_KeyDown(object sender, KeyEventArgs e)
		{
			SelectField_Click(null, null);
			e.Handled = true;
		}

		private void FieldName_MouseDown(object sender, MouseEventArgs e)
		{
			SelectField_Click(null, null);
		}

		private void FieldName_KeyDown(object sender, KeyEventArgs e)
		{
			SelectField_Click(null, null);
			e.Handled = true;
		}

		/// <summary>
		/// If the user selected a field from a non-summarized table, give them the option to replace it with the field from the
		/// summarized table (if available).  Performance is better. This will also avoid extra rows as well as a cartesian product.
		/// </summary>

		private void CheckForSummarizedVersionOfMetaColumn()
		{
			if (QueryColumn == null) return;
			MetaTable mt = QueryColumn.MetaColumn.MetaTable;
			QueryTable qt = new QueryTable(mt);

			bool unsummarrizedVersionSelected = (!mt.UseSummarizedData && mt.SummarizedExists);
			if (!unsummarrizedVersionSelected) return;

			QueryTable summarizedQt = qt.AdjustSummarizationLevel(useSummarized: true);
			QueryColumn summarizedQc = summarizedQt.GetQueryColumnByName(QueryColumn.MetaColumnName);
			if (summarizedQc == null) return;

			string msg =
				"You have selected the un-summarized version of " + QueryColumn.Label + ".\n" +
				"Using the summarized version will result in better performance and avoid unwanted extra rows.\n" +
				"It is recommended that the summarized version be used for calculated fields.\n\n" +
				"Would you like to use the summarized version instead?\n";

			DialogResult dialogResult = MessageBoxMx.Show(msg, "Summarized Data Available!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (dialogResult == DialogResult.Yes)
				QueryColumn = summarizedQc;

			return;
		}

	} // FieldSelectorControl

}
