using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid;
using DevExpress.XtraTab;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;

using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.IO;

namespace Mobius.ClientComponents
{
/// <summary>
/// Define calculated field form
/// </summary>

	public partial class CalcFieldEditor : XtraForm
	{
		static CalcFieldEditor Instance;

		CalcField CalcField; // content for calc field associated with dialog
		List<CalcFieldColumnControl> CfColCtls; // list of the CalcFieldColumnControls
		UserObject UoIn; // user object being edited
		string SelectFieldMenuCommand = ""; // command prefix for selected item
		bool InSetup = false;
		MetaColumnType SourceColumnType = MetaColumnType.Unknown; // type the form is currently set up for
		Query AdvancedEditorPseudoQuery = null; // query containing all of the tables referenced in the advanced calc field

		static List<CalcField> StandardCalculatedFields; // list of standard calculated fields

		static MainMenuControl MainMenuControl { get { return SessionManager.Instance.MainMenuControl; } }

		public CalcFieldEditor()
		{
			InitializeComponent();

			CfColCtls = AddAdditionalCfColCtls(); // build list of col controls

			BuildMenu(AdvancedOperatorPopup, CalcField.AdvancedOps, new EventHandler(AdvancedOperatorMenuItem_Click));
			BuildMenu(AdvancedFunctionPopup, CalcField.AdvancedFuncs, new EventHandler(AdvancedFunctionMenuItem_Click));

			FieldSelectorControl.OptionSelectFromQueryOnly = false;
			FieldSelectorControl.OptionSearchableFieldsOnly = true;
			FieldSelectorControl.OptionNongraphicFieldsOnly = true;
			FieldSelectorControl.OptionIncludeAllSelectableColumns = true;
			FieldSelectorControl.OptionIncludeNoneItem = false;
			FieldSelectorControl.OptionCheckMarkDefaultColumn = false;
			FieldSelectorControl.OptionDisplayInternalMetaTableName = true;
			FieldSelectorControl.OptionSelectSummarizedDataByDefaultIfExists = true; // prefer summarized data for calc fields if exists

			return;
		}

		/// <summary>
		/// Init the column control list to contain 32 controls with CfColCtl1 & CfColCtl2 first 
		/// </summary>
		/// <returns></returns>

		List<CalcFieldColumnControl> AddAdditionalCfColCtls()
		{
			List<CalcFieldColumnControl> cfcc = new List<CalcFieldColumnControl>();
			cfcc.Add(CfColCtl1);
			cfcc.Add(CfColCtl2);

			//ScrollableColumnList.Controls.Clear(); // (any reason we need to do this?)
			//ScrollableColumnList.Controls.Add(CfColCtl1);
			//if (label1.Parent == null) // be sure operation control and parent are present
			//	ScrollableColumnList.Controls.Add(label1);
			//if (Operation.Parent == null)
			//	ScrollableColumnList.Controls.Add(Operation);
			//ScrollableColumnList.Controls.Add(CfColCtl2);

			CalcFieldColumnControl prevCtl = CfColCtl2;
			for (int ci = 2; ci < 32; ci++)
			{
				CalcFieldColumnControl cc = new Mobius.ClientComponents.CalcFieldColumnControl();
				string cis = (ci + 1).ToString();
				cc.FieldLabel = "Data Field " + cis;
				int dy = prevCtl.Size.Height + 6;
				cc.Size = new System.Drawing.Size(prevCtl.Size.Width, prevCtl.Size.Height);
				cc.Location = new System.Drawing.Point(prevCtl.Location.X, prevCtl.Location.Y + dy);
				cc.Name = "CfColCtl" + cis;
				//cc.TabIndex = 21;

				cfcc.Add(cc);
				ScrollableColumnList.Controls.Add(cc);
				prevCtl = cc;
			}

			return cfcc;
		}

		void BuildMenu(
			ContextMenuStrip menu, 
			string[] items,
			EventHandler onClick)
	{
		menu.Items.Clear();
		foreach (string itemText in items)
		{
			ToolStripItem item = menu.Items.Add(itemText);
			item.Click += onClick;
		}
		return;
	}

		/// <summary>
		/// Create a new calculated field
		/// </summary>
		/// <returns></returns>
		public static MetaTable CreateNew()
		{
			CalcField cf = new CalcField();
			UserObject uo = Edit(cf, null);
			if (uo == null) return null;

			string tName = "CALCFIELD_" + uo.Id.ToString();
			MetaTable mt = MetaTableCollection.Get(tName);
			return mt;
		}

		/// <summary>
		/// Select an existing calculated field definition
		/// </summary>
		/// <returns></returns>

		public static MetaTable SelectExisting()
		{
			UserObject uo = UserObjectOpenDialog.ShowDialog(UserObjectType.CalcField, "Select Calculated Field");
			if (uo == null) return null;

			string tName = "CALCFIELD_" + uo.Id.ToString();
			MetaTable mt = MetaTableCollection.Get(tName);
			return mt;
		}

		public static UserObject Edit()
		{
			UserObject uo = null;
			return Edit(uo);
		}

/// <summary>
/// Edit and existing calculated field
/// </summary>
/// <param name="internalName"></param>
/// <returns></returns>

		public static UserObject Edit(string internalName)
		{
			UserObject uo = UserObject.ParseInternalUserObjectName(internalName, "");
			if (uo != null) uo = UserObjectDao.Read(uo.Id);
			if (uo == null) throw new Exception("User object not found " + internalName);
			UserObject uo2 = Edit(uo);
			return uo2;
		}

/// <summary>
/// Edit a new or existing calculated field
/// </summary>
/// <param name="uo">Existing UserObject with content or null to create a new CF</param>
/// <returns></returns>

		public static UserObject Edit(UserObject uo)
		{
			if (uo == null) // prompt if not supplied
				uo = UserObjectOpenDialog.ShowDialog(UserObjectType.CalcField, "Open Calculated Field");
			if (uo == null) return null;

			uo = UserObjectDao.Read(uo);
			if (uo == null) return null;

			CalcField cf = CalcField.Deserialize(uo.Content);
			if (cf == null) return null;

			uo = Edit(cf, uo);
			return uo;
		}

		/// <summary>
		/// Edit a new or existing calculated field
		/// </summary>
		/// <param name="cf"></param>
		/// <param name="uoIn"></param>
		/// <returns></returns>

		public static UserObject Edit(
			CalcField cf,
			UserObject uoIn)
		{
			QueryTable qt;
			MetaTable mt = null;
			MetaColumn mc, field;
			UserObject uo2;
			CalcField cf2 = null;
			string msg;
			int i1;

			CalcFieldEditor lastInstance = Instance;

			CalcFieldEditor i = Instance = new CalcFieldEditor();
			i.AdvancedPanel.BorderStyle = BorderStyle.None;
			i.AdvancedPanel.Visible = false;
			i.AdvancedPanel.Dock = DockStyle.Fill;

			i.BasicPanel.BorderStyle = BorderStyle.None;
			i.BasicPanel.Visible = true;
			i.BasicPanel.Dock = DockStyle.Fill;
			i.SetupStandardCalculatedFields();
			if (ServicesIniFile.Read("CalcFieldHelpUrl") != "")
				i.Help.Enabled = true;

			if (lastInstance != null)
			{
				i.StartPosition = FormStartPosition.Manual;
				i.Location = lastInstance.Location;
				i.Size = lastInstance.Size;
				lastInstance = null;
			}

			if (uoIn == null) uoIn = new UserObject(UserObjectType.CalcField);

			else if (uoIn.Id > 0) // get existing metatable for calc field for exclusion in BuildQuickSelectTableMenu
			{
				string tName = "CALCFIELD_" + uoIn.Id.ToString();
				mt = MetaTableCollection.GetExisting(tName);
				if (MainMenuControl != null) MainMenuControl.UpdateMruList(tName);
			}

			Instance.UoIn = uoIn;

			string title = "Edit Calculated Field";
			if (!String.IsNullOrEmpty(uoIn.Name))
				title += " - " + uoIn.Name;
			Instance.Text = title;

			Instance.CalcField = cf; // what we're editing
			Instance.SetupForm(); // set up the form 

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return null;

			return Instance.UoIn; // return saved object
		}

		/// <summary>
		/// Set contents of calculated field form
		/// </summary>
		/// <param name="cf"></param>

		void SetupForm()
		{
			InSetup = true;
			CalcField cf = CalcField;
			CalcFieldColumn cfc;
			CalcFieldColumnControl cfcc;
			CondFormatRules rules;
			int ci, cci;

			MetaColumnType mcType = MetaColumnType.Number; // be sure source column type is set
			MetaColumn cfMc = null;

			Tabs.SelectedTabPageIndex = 0; // be sure first tab is selected
			SetupControlVisibility();

			foreach (CalcFieldColumnControl cfc0 in CfColCtls) // clear field selector controls on form
			{
				cfc0.FieldSelectorControl.Query = QueriesControl.Instance.CurrentQuery;  // cfc0.CfCol.Query = QueriesControl.Instance.CurrentQuery; 
				cfc0.FieldSelectorControl.MetaColumn = null;
			}

			AdvancedExpr.Text = "";
			FormatAdvancedExpr(cf.AdvancedExpr, out AdvancedEditorPseudoQuery); // setup advanced field menu

			if (cf.CalcType == CalcTypeEnum.Basic)
			{
				cfMc = cf.MetaColumn1;

				BasicOptionButton.Checked = true;

				if (cf.Operation != "")
					Operation.Text = cf.Operation;
				else Operation.Text = "/ (Division)";

				cf.SetDerivedValues();
			}

			else // advanced type
			{
				AdvancedOptionButton.Checked = true;
				AdvancedExpr.Text = FormatAdvancedExpr(cf.AdvancedExpr, out AdvancedEditorPseudoQuery);

				List<MetaColumn> mcList = cf.GetInputMetaColumnList();
				if (mcList.Count > 0)	cfMc = mcList[0];

				// Setup result type

				if (cf.PreclassificationlResultType == MetaColumnType.Unknown)
					cf.PreclassificationlResultType = MetaColumnType.Number;
				string txt = cf.PreclassificationlResultType.ToString();
				if (Lex.Eq(txt, "String")) txt = "Text"; // fixup
				ResultDataType.Text = txt;

				// Setup outer join table

				JoinAnchorComboBox.Text = "";

				if (Lex.IsDefined(cf.OuterJoinRoot))
				{
					for (ci = 0; ci < cf.CfCols.Count; ci++)
					{
						cfc = cf.CfCols[ci];
						if (cfc.MetaColumn == null) continue;
						MetaTable mt = cfc.MetaColumn.MetaTable;

						if (Lex.Eq(cf.OuterJoinRoot, cfc.MetaColumn.MetaTable.Name))
						{
							JoinAnchorComboBox.Text = cfc.MetaColumn.MetaTable.Label;
							break;
						}
					}

				}

			}

// Setup column controls for source column type

			if (cfMc != null) cf.SourceColumnType = cfMc.DataType;
			else cf.SourceColumnType = MetaColumnType.Number;

			SetupFormForColumnType(cf.SourceColumnType);

// Add column controls MetaColumn references and function info

			cci = -1; // col control index

			for (ci = 0; ci < cf.CfCols.Count; ci++)
			{
				cfc = cf.CfCols[ci];

				cci++;
				if (cci >= CfColCtls.Count)
					throw new Exception("Number of calculated field columns exceeds limit of: " + CfColCtls.Count);
				cfcc = CfColCtls[cci];

				cfcc.FieldSelectorControl.MetaColumn = cfc.MetaColumn;

				if (Lex.IsDefined(cfc.Function))
					cfcc.Function.Text = cfc.Function;
				else cfcc.Function.Text = "None";

				cfcc.Constant.Text = cfc.Constant;
				cfcc.Constant.Enabled = Lex.Contains(cfc.Function, "constant");
			}


			CalcField.ColumnLabel = FieldColumnName.Text;
			Description.Text = cf.Description;

			if (cf.Operation != "")
				Operation.SelectedItem = cf.Operation;

			if (cf.Classification != null) rules = cf.Classification.Rules;
			else rules = new CondFormatRules();

			if (rules.Count ==0)
				rules.Add(new CondFormatRule()); // include initial rule

			CondFormatRulesCtl.SetupControl(CalcField.PreclassificationlResultType, rules.ColoringStyle);
			CondFormatRulesCtl.SetupControl(CalcField.PreclassificationlResultType, rules);

			InSetup = false;
			return;
		}

/// <summary>
/// SetupControlVisibility
/// </summary>

		void SetupControlVisibility()
		{
			if (CalcField.CalcType == CalcTypeEnum.Basic)
			{
				AdvancedPanel.Visible = false;
				BasicPanel.Visible = true;

				AdvancedPanel.Visible = FieldsButton.Visible = OperatorsButton.Visible = FunctionsButton.Visible = false;
			}

			else // advanced
			{
				BasicPanel.Visible = false;
				AdvancedPanel.Visible = true;

				AdvancedPanel.Visible = FieldsButton.Visible = OperatorsButton.Visible = FunctionsButton.Visible = true;
			}

			return;
		}

		/// <summary>
		/// Get other values from calc field form checking for errors
		/// </summary>
		/// <returns></returns>

		bool GetCalcFieldForm()
		{
			List<MetaColumn> mcList;

			CalcField cf = CalcField;
			CalcFieldColumn cfc = null;

			if (cf.CalcType == CalcTypeEnum.Basic) // basic CF
			{
				cf.Operation = Operation.Text;

				cf.CfCols.Clear(); // update
				foreach (CalcFieldColumnControl cfcc in CfColCtls)
				{
					if (cfcc.FieldSelectorControl.MetaColumn == null) continue;

					cfc = new CalcFieldColumn();
					cfc.MetaColumn = cfcc.FieldSelectorControl.MetaColumn;
					cfc.Function = cfcc.Function.Text;
					cfc.Constant = cfcc.Constant.Text;
					cf.CfCols.Add(cfc);
				}
			}

			else // advanced CF
			{
				cf.AdvancedExpr = ParseAdvancedExpr(AdvancedExpr.Text, out mcList);

// Get preclassification result type
				
				string txt = ResultDataType.Text;
				if (Lex.IsDefined(txt))
					cf.PreclassificationlResultType = MetaColumn.ParseMetaColumnTypeString(txt);

				else cf.PreclassificationlResultType = MetaColumnType.Number;

// Get anchor table for joins

				cf.CfCols.Clear();
				foreach (MetaColumn mc0 in mcList)
				{
					cfc = new CalcFieldColumn();
					cfc.MetaColumn = mc0;
					cf.CfCols.Add(cfc);
				}

				cf.OuterJoinRoot = "";
				txt = JoinAnchorComboBox.Text;
				if (Lex.Eq(txt, "(None)")) txt = "";

				if (Lex.IsDefined(txt))
				{
					for (int ci = 0; ci < cf.CfCols.Count; ci++)
					{
						cfc = cf.CfCols[ci];
						if (Lex.Eq(txt, cfc.MetaColumn.MetaTable.Label))
						{
							cf.OuterJoinRoot = cfc.MetaColumn.MetaTable.Name;
							break;
						}
					}

				}

			}

			cf.Description = Description.Text;

			CondFormatRules rules = CondFormatRulesCtl.GetRules();
			if (rules.Count == 0) cf.Classification = null;
			else
			{
				cf.Classification = new CondFormat();
				cf.Classification.Rules = rules;
			}

			try { cf.SetDerivedValuesWithException(); } // set derived values checking for errors
			catch (Exception ex)
			{
				MessageBoxMx.ShowError(ex.Message);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Read in the set of standard calculated fields & send to dropdown menu
		/// </summary>

		void SetupStandardCalculatedFields()
		{
			try
			{
				if (StandardCalculatedFields == null)
				{
					StandardCalculatedFields = new List<CalcField>();

					string fileName = "StandardCalculatedFields.xml";
					string serverFile = @"<MetaDataDir>\" + fileName;
					string clientFile = ClientDirs.TempDir + @"\" + fileName;
					ServerFile.CopyToClient(serverFile, clientFile);
					StreamReader sr = new StreamReader(clientFile);
					XmlTextReader tr = new XmlTextReader(sr);

					tr.MoveToContent();
					if (!Lex.Eq(tr.Name, "StandardCalculatedFields"))
						throw new Exception("SetupStandardCalculatedFields - \"StandardCalculatedFields\" element not found");

					while (true)
					{
						tr.Read(); // get CalcField element
						tr.MoveToContent();
						if (Lex.Eq(tr.Name, "CalcField"))
						{
							CalcField cf = CalcField.Deserialize(tr);
							StandardCalculatedFields.Add(cf);
						}
						else if (Lex.Eq(tr.Name, "StandardCalculatedFields")) break;

						else throw new Exception("SetupStandardCalculatedFields unexpected element: " + tr.Name);
					}

					tr.Close();
					sr.Close();
				}

				ToolStripItemCollection items = StandardCalcContextMenu.Items;
				items.Clear();

				foreach (CalcField cf2 in StandardCalculatedFields)
				{
					ToolStripMenuItem mi = new System.Windows.Forms.ToolStripMenuItem();
					mi.Image = StandardCalcMenuItem.Image;
					mi.ImageTransparentColor = System.Drawing.Color.Cyan;
					mi.Text = cf2.UserObject.Name;
					mi.Click += new System.EventHandler(this.StandardCalcMenuItem_Click);
					items.Add(mi);
				}

				return;
			}

			catch (Exception ex)
			{
				ServicesLog.Message("Error reading StandardCalculatedFields.xml: " + ex.Message);
			}

			return;
		}


		private void CloseButton_Click(object sender, EventArgs e)
		{
			return;
		}

		private void Help_Click(object sender, EventArgs e)
		{
			QbUtil.ShowConfigParameterDocument("CalcFieldHelpUrl", "Calculated Field Help");
		}

/// <summary>
/// See if valid calculated field
/// </summary>
/// <returns></returns>

		private bool IsValidCalcField()
		{
			if (CalcField.CalcType == CalcTypeEnum.Basic) 
				return IsValidBasicCalcField();

			else return IsValidAdvancedCalcField();
		}

/// <summary>
/// See if valid basic calculated field
/// </summary>
/// <returns></returns>

		private bool IsValidBasicCalcField()
		{
			for (int cfcci = 0; cfcci < CfColCtls.Count; cfcci++)
			{
				CalcFieldColumnControl cfcc = CfColCtls[cfcci];

				bool validate = false; // decide if we should validate this field

				if (cfcci == 0) validate = true; // always validate 1st field
				else if (cfcci == 1 && !Lex.StartsWith(Operation.Text, "none")) validate = true; // validate 2nd field if operator is not "none"
				else if (cfcc.FieldSelectorControl.MetaColumn != null) validate = true;

				if (validate && !ValidateField(cfcc))
					return false;
			}

			if (!CondFormatRulesCtl.AreValid()) // are any rules valid
			{
				Tabs.SelectedTabPageIndex = 1;
				return false;
			}

			CondFormatRules rules = CondFormatRulesCtl.GetRules();

			if (CalcField.SourceColumnType == MetaColumnType.Structure && rules.Count == 0)
			{
				Tabs.SelectedTabPageIndex = 1;
				MessageBoxMx.Show(
					"Calculated fields on chemical structures must use classification\r\n" +
					"for the calculated field value", UmlautMobius.String);
				return false;
			}

			if (rules.Count > 0) // must have at least one rule

			foreach (CondFormatRule rule in rules) // must have name for each rule
			{
				if (String.IsNullOrEmpty(rule.Name))
				{
					Tabs.SelectedTabPageIndex = 1;
					MessageBoxMx.Show("A name must be defined for each rule if using rule names the as calculated value", UmlautMobius.String);
					return false;
				}
			}

			return true;
		}

/// <summary>
/// See if valid advanced calculated field
/// </summary>
/// <returns></returns>

		private bool IsValidAdvancedCalcField()
		{
			List<MetaColumn> mcList;

			string advExpr;

			if (Lex.IsNullOrEmpty(AdvancedExpr.Text))
			{
				MessageBoxMx.Show(
					"The calculated field expression is blank", UmlautMobius.String);
				return false;
			}

			try { advExpr = ParseAdvancedExpr(AdvancedExpr.Text, out mcList); }
			catch (Exception ex)
			{
				MessageBoxMx.Show(ex.Message, UmlautMobius.String);
				return false;
			}

			string errmsg = QueryEngine.ValidateCalculatedFieldExpression(advExpr);
			if (!Lex.IsNullOrEmpty(errmsg))
			{
				MessageBoxMx.Show(
					"Invalid calculated field expression:\r\n\r\n" + errmsg, UmlautMobius.String);
				return false;
			}

			return true;
		}

/// <summary>
/// Check column control for validity
/// </summary>
/// <param name="cfcc"></param>
/// <returns></returns>

		private bool ValidateField (
			CalcFieldColumnControl cfcc)
		{
			string table = cfcc.FieldSelectorControl.TableName.Text; //cfcc.CfCol.TableName.Text;
			string field = cfcc.FieldSelectorControl.FieldName.Text; // cfcc.CfCol.FieldName.Text;
			string function = cfcc.Function.Text;
			string constant = cfcc.Constant.Text;
			string label = cfcc.FieldLabel;

			if (table=="" || field=="")
			{
				MessageBoxMx.Show("You must define the " + label + " data field");
				return false;
			}

			if (function.ToLower().IndexOf("constant") >= 0) // do we have a constant?
			{
				if (constant=="")
				{
					MessageBoxMx.Show("You must define the " + label + " data field");
					return false;
				}
				
				try { Double.Parse(constant); }
				catch (Exception ex)
				{
					MessageBoxMx.Show("The constant " + constant + " for the " + label + " data field is not valid");
					return false;
				}
			}

			if (function=="" || function.ToLower().StartsWith("---")) // valid function?
			{
				MessageBoxMx.Show("Invalid function for the " + label + " data field");
				return false;
			}

			return true;
		}

/// <summary>
/// Reset form for change in source column type if needed
/// </summary>
/// <param name="sourceColumnType"></param>

		public void SetSourceColumnType(
			int sourceColumnTypeInt)
		{
			MetaColumnType sourceColumnType = (MetaColumnType)sourceColumnTypeInt;
			SetupFormForColumnType(sourceColumnType); // setup form
			CalcField.SourceColumnType = sourceColumnType;
			CalcField.SetDerivedValues();

			CondFormatRules rules = new CondFormatRules();

			CondFormatRulesCtl.SetupControl(CalcField.PreclassificationlResultType, rules.ColoringStyle);

			rules.Add(new CondFormatRule()); // include initial rule
			CondFormatRulesCtl.SetupControl(CalcField.PreclassificationlResultType, rules);
			return;
		}

		/// <summary>
		/// Set the column type for the source data
		/// </summary>

		public void SetupFormForColumnType(
			MetaColumnType mcType)
		{
			if (MetaColumn.AreCompatibleMetaColumnTypes(mcType, SourceColumnType)) return;

			SourceColumnType = mcType;

			string[] funcs = null;
			string[] ops = null;

			if (MetaColumn.IsNumericMetaColumnType(mcType))
			{
				funcs = CalcField.NumericFuncs;
				ops = CalcField.NumericOps;
			}

			else if (mcType == MetaColumnType.Date)
			{
				funcs = CalcField.DateFuncs;
				ops = CalcField.DateOps;
			}

			else if (mcType == MetaColumnType.Image) // allow overlay of NGR CRC curves
			{
				funcs = CalcField.CrcOverlayFuncs;
				ops = CalcField.CrcOverlayOps;
			}

			else
			{
				funcs = CalcField.NoFuncs;
				ops = CalcField.NoOps;
			}

			foreach (CalcFieldColumnControl cfcc in CfColCtls)
			{
				cfcc.Function.Properties.Items.Clear();
				cfcc.Function.Properties.Items.AddRange(funcs);
				cfcc.Function.SelectedIndex = 0;
			}

			Operation.Properties.Items.Clear();
			Operation.Properties.Items.AddRange(ops);
			Operation.SelectedIndex = 0;

			return;
		}

/// <summary>
/// Serialize conditional formatting, classification rules
/// </summary>
/// <returns></returns>

		public string SerializeRules()
		{
			CondFormatRules rules = CondFormatRulesCtl.GetRules();
			string serializedForm = rules.Serialize();
			return serializedForm;
		}

		private void CalcFieldDialog2_FormClosing(object sender, FormClosingEventArgs e)
		{
			return;
		}

		private void Operation_SelectedValueChanged(object sender, EventArgs e)
		{
			bool enable = !Lex.StartsWith(Operation.Text, "none");
			EnableSecondaryFields(enable);
			return;
		}

		void EnableSecondaryFields(bool enable)
		{
			for (int cci = 1; cci < CfColCtls.Count; cci++)
			{
				CalcFieldColumnControl cc = CfColCtls[cci];
				cc.Enabled = enable;
			}

			return;
		}

		/// <summary>
		/// Select a standard calculated field 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void StandardCalcMenuItem_Click(object sender, EventArgs e)
		{
			CalcField cf2 = null;
			string title;
			int i1;

			ToolStripMenuItem ci = (ToolStripMenuItem)sender; // item clicked on
			if (String.IsNullOrEmpty(ci.Text)) return;

			string standardCalcName = ci.Text;

			for (i1 = 0; i1 < StandardCalculatedFields.Count; i1++)
			{
				cf2 = StandardCalculatedFields[i1];
				if (Lex.Eq(cf2.UserObject.Name, standardCalcName)) break;
			}

			if (i1 >= StandardCalculatedFields.Count)
				throw new Exception("Unrecognized standard calculation: " + standardCalcName);

			CalcField = cf2.Clone();

			SetupForm(); // display new content

			title = "Edit Calculated Field";
			string cfName = cf2.UserObject.Name;
			i1 = cfName.IndexOf(" (");
			if (i1 > 0) cfName = cfName.Substring(0, i1);
			title += " - " + cfName;
			Text = title;

			if (UoIn.Id <= 0) UoIn.Name = cfName; // substitute name if not saved yet

			if (!String.IsNullOrEmpty(cf2.Prompt)) // prompt the user
			{
				MetaColumn mc = FieldSelectorDialog.Show(cf2.Prompt, "Input Parameter", 
					QueriesControl.Instance.CurrentQuery, null);

				if (mc == null) return;

				else if (CalcField.CalcType == CalcTypeEnum.Basic)
				{
					CalcField.Column1.MetaColumn = mc;
				}

				else CalcField.AdvancedExpr =
					Lex.Replace(CalcField.AdvancedExpr, "user_input_metatable.metacolumn", mc.MetaTable.Name + "." + mc.Name);

				SetupForm();
			}
		}

		private void StandardCalculationsButton_Click(object sender, EventArgs e)
		{
			StandardCalcContextMenu.Show(StandardCalculationsButton,
				new System.Drawing.Point(0, StandardCalculationsButton.Height));
		}

/// <summary>
/// Setup the menu of standard calc fields
/// </summary>
/// <param name="menuText"></param>

		public void SetupStandardCalcFieldsContextMenu(
			string menuText)
		{
			string[] sa = menuText.Split('\t');

			ToolStripItemCollection items = StandardCalcContextMenu.Items;
			items.Clear();
			foreach (string s in sa)
			{
				ToolStripMenuItem mi = new System.Windows.Forms.ToolStripMenuItem();
				mi.Image = global::Mobius.ClientComponents.Properties.Resources.CalcField;
				mi.ImageTransparentColor = System.Drawing.Color.Cyan;
				mi.Text = s;
				mi.Click += new System.EventHandler(this.StandardCalcMenuItem_Click);
				items.Add(mi);
				// Use classification to map calculated field values to class/rule names as defined by a set of rules.
			}

			return;
		}

		private void SaveAndClose_Click(object sender, EventArgs e)
		{
			bool prompt = true;
			if (UoIn.Id > 0) prompt = false; // normal save & have existing object?
			if (Save(prompt)) DialogResult = DialogResult.OK;
		}

		private void SaveAndClose_ShowDropDownControl(object sender, ShowDropDownControlEventArgs e)
		{
			SaveAndCloseContextMenu.Show(SaveAndClose,
				new System.Drawing.Point(0, SaveAndClose.Size.Height));
		}
		
		private void SaveMenuItem_Click(object sender, EventArgs e)
		{
			bool prompt = true;
			if (UoIn.Id > 0) prompt = false; // normal save & have existing object?
			Save(prompt);
		}

		private void SaveAsMenuItem_Click(object sender, EventArgs e)
		{
			Save(true);
		}

		bool Save(bool prompt)
		{
			UserObject uo2;

			if (!IsValidCalcField()) return false;
			if (!GetCalcFieldForm()) return false;
			if (prompt)
			{
				uo2 = UserObjectSaveDialog.Show("Save Calculated Field Definition", UoIn);
				if (uo2 == null) return false;
			}
			else uo2 = UoIn.Clone();

			if (!UserObjectUtil.UserHasWriteAccess(uo2))
			{ // is the user authorized to save this?
				MessageBoxMx.ShowError("You are not authorized to save this calculated field");
				return false;
			}

			SessionManager.DisplayStatusMessage("Saving calculated field...");

			string content = CalcField.Serialize();

			uo2.Content = content;

			//need the name of the folder to which the object will be saved
			MetaTreeNode targetFolder = UserObjectTree.GetValidUserObjectTypeFolder(uo2);
			if (targetFolder == null)
			{
				MessageBoxMx.ShowError("Failed to save your calculated field.");
				return false;
			}

			UserObjectDao.Write(uo2);

			string tName = "CALCFIELD_" + uo2.Id.ToString();
			QbUtil.UpdateMetaTableCollection(tName);
			MainMenuControl.UpdateMruList(tName);

			string title = "Edit Calculated Field - " + uo2.Name;
			Text = title;
			UoIn = uo2.Clone(); // now becomes input object
			SessionManager.DisplayStatusMessage("");
			return true;
		}

/// <summary>
/// First field changed, adjust source type
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void CfColCtl1_ColumnChanged(object sender, EventArgs e)
		{
			CalcFieldColumnControl cfcc = sender as CalcFieldColumnControl;
			if (cfcc == null || cfcc.FieldSelectorControl == null || cfcc.FieldSelectorControl.MetaColumn == null) return;

			MetaColumn mc = cfcc.FieldSelectorControl.MetaColumn;
			if (mc != null) // reset form for type
			  SetSourceColumnType((int)mc.DataType);
		}

		private void Basic_EditValueChanged(object sender, EventArgs e)
		{
			if (!BasicOptionButton.Checked || InSetup) return;

			CalcField.CalcType = CalcTypeEnum.Basic;
			SetupControlVisibility();
			return;
		}

		private void Advanced_EditValueChanged(object sender, EventArgs e)
		{
			if (!AdvancedOptionButton.Checked || InSetup) return;

			GetCalcFieldForm(); // get current values
			string advExp = CalcField.ConvertBasicToAdvanced();
			CalcField.AdvancedExpr = advExp;
			if (!Lex.IsNullOrEmpty(advExp))	AdvancedExpr.Text = FormatAdvancedExpr(advExp, out AdvancedEditorPseudoQuery);

			CalcField.CalcType = CalcTypeEnum.Advanced;
			SetupControlVisibility();
			AdvancedExpr.Focus();
			return;
		}

		private void CalcFieldEditor_Activated(object sender, EventArgs e)
		{
			if (CalcField.CalcType == CalcTypeEnum.Advanced)
				AdvancedExpr.Focus();
		}

		private void FieldsButton_Click(object sender, EventArgs e)
		{
			Query q;
			QueryTable qt, qt2;

			string advExprExt;
			Point p = FieldsButton.PointToScreen(new Point(0, FieldsButton.Height));
			FieldSelectorControl.Query = AdvancedEditorPseudoQuery;
			FieldSelectorControl.MetaColumn = null; // no current selection

			int p1 = AdvancedExpr.SelectionStart; // save position/selection since may change
			int p2 = AdvancedExpr.SelectionLength;

			Mobius.Data.QueryColumn qc = FieldSelectorControl.ShowMenu(p);
			if (qc == null) return;
			MetaColumn mc = qc.MetaColumn;

			string tok = '"' + mc.MetaTable.Name + "." + mc.Label + '"';
			InsertText(tok, p1, p2);

			qt2 = AdvancedEditorPseudoQuery.GetQueryTableByName(mc.MetaTable.Name);
			if (qt2 == null) AdvancedEditorPseudoQuery.AddQueryTable(qc.QueryTable);
		}

/// <summary>
/// Format internal form of advanced expression 
/// </summary>
/// <param name="advExpr"></param>
/// <param name="q"></param>

		string  FormatAdvancedExpr(
			string advExpr, 
			out Query q)
		{
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;

			string extExpr = advExpr; // copy internal form to external
			q = new Query();

			Lex lex = new Lex();
			lex.OpenString(advExpr);
			while (true)
			{
				string tok = lex.Get();
				if (tok == "") break;
				if (Lex.IsQuoted(tok, '"'))
				{
					tok = tok.Substring(1, tok.Length - 2).Trim();
					mc = MetaColumn.ParseMetaTableMetaColumnName(tok);
					if (mc == null) continue; // not found, may be typo, ignore for now

					mt = mc.MetaTable;
					if (q.GetQueryTableByName(mt.Name) == null)
						q.AddQueryTable(new QueryTable(mt));

					string tok2 = mt.Name + "." + mc.Label;
					extExpr = extExpr.Replace(tok, tok2);
				}
			}

// Add any other tables from base query for convenience

			Query q2 = QueriesControl.BaseQuery;
			if (q2 != null) 
			{
				foreach (QueryTable qt0 in q2.Tables)
				{
					qt = q.GetQueryTableByName(qt0.MetaTable.Name);
					if (qt == null) q.AddQueryTable(qt0.Clone());
				}
			}

			return extExpr; // return external form
		}

/// <summary>
/// Parse external format advanced expression into internal form
/// </summary>
/// <param name="advExprExt"></param>
/// <param name="mcList"></param>
/// <returns></returns>

		string ParseAdvancedExpr(
			string advExprExt,
			out List<MetaColumn> mcList)
		{
			MetaTable mt;
			MetaColumn mc;

			string advExpr = advExprExt; // copy external to internal
			mcList = new List<MetaColumn>();

			Lex lex = new Lex();
			lex.OpenString(advExprExt);
			while (true)
			{
				string tok = lex.Get();
				if (tok == "") break;
				if (Lex.IsQuoted(tok, '"'))
				{
					tok = tok.Substring(1, tok.Length - 2).Trim(); // remove quotes
					int i1 = tok.IndexOf('.');
					if (i1 < 0) throw new Exception("Invalid table.field name: " + tok);
					string table = tok.Substring(0, i1);
					string column = tok.Substring(i1 + 1);
					mt = MetaTableCollection.Get(table);
					if (mt == null) throw new Exception("Unknown table name: " + table);

					mc = mt.GetMetaColumnByLabel(column);
					if (mc == null) mc = mt.GetMetaColumnByName(column);
					if (mc == null) throw new Exception("Unknown column name: " + column);

					string tok2 = mt.Name + "." + mc.Name; // store as internal name
					advExpr = advExpr.Replace(tok, tok2);

					if (!mcList.Contains(mc)) mcList.Add(mc);
				}
			}

			return advExpr; // return internal form
		}

/// <summary>
/// OperatorsButton_Click
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void OperatorsButton_Click(object sender, EventArgs e)
		{
			AdvancedOperatorPopup.Show(OperatorsButton,
			new Point(0, OperatorsButton.Height));
		}

		private void AdvancedOperatorMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem mi = sender as ToolStripMenuItem;
			string tok = mi.Text;
			int i1 = tok.IndexOf(" ");
			if (i1 > 0) tok = tok.Substring(0, i1);

			i1 = AdvancedExpr.SelectionStart + 2; // where to place cursor
			InsertText(tok);
			AdvancedExpr.Focus();
			AdvancedExpr.SelectionStart = i1;
			AdvancedExpr.SelectionLength = 0;
			return;
		}

		private void FunctionsButton_Click(object sender, EventArgs e)
		{
			AdvancedFunctionPopup.Show(FunctionsButton,
			new Point(0, FunctionsButton.Height));
		}

		private void AdvancedFunctionMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem mi = sender as ToolStripMenuItem;
			string tok = mi.Text;
			int i1 = tok.IndexOf(" ");
			if (i1 > 0) tok = tok.Substring(0, i1);
			i1 = tok.IndexOf("m");
			if (i1 < 0) i1 = tok.IndexOf("n");
			i1 += AdvancedExpr.SelectionStart; // where to place cursor
			tok = tok.Replace("n", ""); // remove place holders
			tok = tok.Replace("m", "");

			InsertText(tok);

			AdvancedExpr.Focus();
			AdvancedExpr.SelectionStart = i1;
			AdvancedExpr.SelectionLength = 0;
			return;
		}

		void InsertText(string insTxt)
		{
			int p1 = AdvancedExpr.SelectionStart; // save position/selection since may change
			int p2 = AdvancedExpr.SelectionLength;
			InsertText(insTxt, p1, p2);
		}

		void InsertText(string insTxt, int p1, int p2)
		{ // note a one char string has two possible SelectionStart positions, 0 and 1
			//if (p2 > 0) p2 = p2; // debug
			int p3 = p1 + p2;
			string txt = AdvancedExpr.Text;

			if (p1 == 0 || txt[p1 - 1] != ' ') // space before
				insTxt = " " + insTxt;

			if (p3 >= txt.Length - 1 || txt[p3] != ' ') // space after
				insTxt = insTxt + " ";

			txt = txt.Substring(0, p1) + insTxt + txt.Substring(p3);
			AdvancedExpr.Text = txt;
			AdvancedExpr.SelectionStart = p1 + insTxt.Length;
			AdvancedExpr.SelectionLength = 0;
			return;
		}

		private void FieldColumnName_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			CalcField.ColumnLabel = FieldColumnName.Text;
		}

		private void JoinAnchorComboBox_Properties_Enter(object sender, EventArgs e)
		{
			GetCalcFieldForm(); // get latest values

			CalcField cf = CalcField;
			CalcFieldColumn cfc;

			HashSet<MetaTable> tableSet = new HashSet<MetaTable>();

			ComboBoxItemCollection items = JoinAnchorComboBox.Properties.Items;
			items.Clear();
			items.Add("(none)");

			for (int ci = 0; ci < cf.CfCols.Count; ci++)
			{
				CalcFieldColumn cfCol = cf.CfCols[ci];
				if (cfCol.MetaColumn == null) continue;
				MetaTable mt = cfCol.MetaColumn.MetaTable;
				if (tableSet.Contains(mt)) continue;

				tableSet.Add(mt);
				items.Add(mt.Label);
			}

			return;
		}

	}
}