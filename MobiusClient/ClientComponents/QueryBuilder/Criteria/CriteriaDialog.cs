using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class CriteriaDialog : DevExpress.XtraEditors.XtraForm
	{
		static CriteriaDialog Instance;

		int InitialHeight;
		int InitialNoneTop;
		QueryColumn Qc; // column being edited

		public CriteriaDialog()
		{
			InitializeComponent();

			InitialHeight = this.Height;
			InitialNoneTop = None.Top;
		}

/// <summary>
/// Invoke the editor
/// </summary>
/// <param name="qc">QueryColumn to edit</param>
/// <returns></returns>

		public static bool Edit(
			QueryColumn qc)
		{

			//if (DebugMx.True) return CriteriaYesNo.Edit(qc);

			if (qc.MetaColumn.DictionaryMultipleSelect) return CriteriaDictMultSelect.Edit(qc);

			else if (Lex.Eq(qc.MetaColumn.Dictionary, "yes_no")) return CriteriaYesNo.Edit(qc);

			MetaColumn mc = qc.MetaColumn;
			if (Instance == null) Instance = new CriteriaDialog();
			if (Instance.Visible) return false; // catch 2nd of two quick single-clicks & ignore

			new PlotlyDashConverter().ToDash(Instance, true);

			Instance.Setup(qc);
			Instance.Qc = qc;
			Form activeForm = SessionManager.ActiveForm;
			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			if (activeForm != null) activeForm.BringToFront();

			if (dr == DialogResult.OK) return true;
			else return false;
		}

/// <summary>
/// Setup the form with existing criteria
/// </summary>
/// <param name="qc"></param>

		void Setup(
			QueryColumn qc)
		{
			string op, tok, val;
			CheckEdit option = null;
			CheckEdit subOption = null;
			int i1;

			MetaColumn mc = qc.MetaColumn;

			string prompt = "Select the search criteria that you want to apply to " + qc.ActiveLabel +
				" from the list below and enter the limiting value(s).";

			bool removeDuplicates = mc.IgnoreCase; // remove dups if ignoring case
			UIMisc.SetListControlItemsFromDictionary(Value.Properties.Items, mc.Dictionary, removeDuplicates); // setup dropdown

			switch (mc.DataType)
			{

				case MetaColumnType.Integer:
				case MetaColumnType.Number:
				case MetaColumnType.QualifiedNo:
					Setup(true, true, false, false);
					break;

				case MetaColumnType.String:
					Setup(true, true, true, false);
					break;

				case MetaColumnType.Date:
					prompt += " Dates can be entered in the common standard ways: e.g. 12/31/2004 or 31-Dec-04.";
					Setup(true, true, false, true);
					break;

				case MetaColumnType.DictionaryId:
					Setup(false, false, false, false);
					break;
			}

			// Fill in the form with the current criteria

			Instance.Text = "Search Criteria for " + qc.ActiveLabel;
			Instance.Prompt.Text = prompt;

			ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(qc.Criteria);
			if (psc == null)
			{ // create default values
				psc = new ParsedSingleCriteria();
				psc.Op = "=";
			}

			if (mc.DataType == MetaColumnType.Date && psc.OpEnum != CompareOp.Within) // format dates for user if not within clause
			{
				if (psc.Value != "") psc.Value = DateTimeMx.Format(psc.Value);
				if (psc.Value2 != "") psc.Value2 = DateTimeMx.Format(psc.Value2);
				if (psc.ValueList != null)
					for (i1 = 0; i1 < psc.ValueList.Count; i1++)
					{
						tok = (string)psc.ValueList[i1];
						if (tok != null && tok != "")
							psc.ValueList[i1] = DateTimeMx.Format(tok);
					}
			}

			else if (mc.DataType == MetaColumnType.DictionaryId && psc.Value != "")
			{ // transform database value to dictionary value
				val = DictionaryMx.LookupWordByDefinition(mc.Dictionary, psc.Value);
				if (val != null && val != "") psc.Value = val;
			}

			op = psc.Op;

			// Fill fields based on criteria types

			if (op == "" || op.IndexOf("=") >= 0 || op.IndexOf("<") >= 0 || op.IndexOf(">") >= 0)
			{
				option = BasicOp;
				if (op == "=" || op == "") BasicOpBut.Text = "Equals";
				else if (op == "<") BasicOpBut.Text = "<";
				else if (op == "<=") BasicOpBut.Text = UnicodeString.LessOrEqual;
				else if (op == "<>") BasicOpBut.Text = UnicodeString.NotEqual;
				else if (op == ">") BasicOpBut.Text = ">";
				else if (op == ">=") BasicOpBut.Text = UnicodeString.GreaterOrEqual;

				Value.Text = psc.Value; // put in current value
			}

			else if (Lex.Eq(op, "Between"))
			{
				option = Between;
				Limit1.Text = psc.Value;
				Limit2.Text = psc.Value2;
			}

			else if (Lex.Eq(op, "In"))
			{
				option = InList;
				StringBuilder sb = new StringBuilder();
				for (i1 = 0; i1 < psc.ValueList.Count; i1++)
				{
					if (i1 > 0) sb.Append(", ");
					sb.Append((string)psc.ValueList[i1]);
				}
				ValueList.Text = sb.ToString(); // set value
			}

			else if (Lex.Eq(op, "Like"))
			{
				option = Like;
				tok = psc.Value.Replace("%", ""); // remove sql wildcard characters
				Substring.Text = tok;
			}

			else if (Lex.Eq(op, "Within"))
			{
				option = Within;
				WithinValue.Text = psc.Value;

				string value2 = psc.Value2;

				if (Lex.Contains(value2, "Day")) // translate alternative values
					value2 = "Day(s)";
				else if (Lex.Contains(value2, "Week"))
					value2 = "Week(s)";
				else if (Lex.Contains(value2, "Month"))
					value2 = "Month(s)";
				else if (Lex.Contains(value2, "Year"))
					value2 = "Year(s)";

				WithinUnits.Text = value2;
			}

			else if (Lex.Eq(op, "is not null"))
			{
				option = IsNotNull;
			}

			else if (Lex.Eq(op, "is null"))
			{
				option = IsNull;
			}

			else if (Lex.Eq(op, "is not null or is null"))
			{
				option = All;
			}

			else // oops
			{
				MessageBoxMx.ShowError("Unrecognized criteria type");
				qc.Criteria = qc.CriteriaDisplay = "";
				return;
			}

			option.Checked = true;

			return;
		}

		/// <summary>
		/// Setup for string criteria
		/// </summary>

		public void SetupForString()
		{
			Setup(true, true, true, false);
		}

		/// <summary>
		/// Setup for date criteria
		/// </summary>

		public void SetupForDate()
		{
			Setup(true, true, false, true);
		}

		/// <summary>
		/// Setup for dictionary criteria
		/// </summary>

		public void SetupForDictionary()
		{
			Setup(false, false, false, false);
		}

		public void Setup(
				bool includeList,
				bool includeBetween,
				bool includeLike,
				bool includeWithin)
		{
			int spacing = None.Top - All.Top; // spacing between lines
			int loc = BasicOpBut.Top + spacing; // where next line will go

			Value.Text = "";
			ValueList.Text = "";
			Limit1.Text = "";
			Limit2.Text = "";
			Substring.Text = "";
			WithinValue.Text = "";
			WithinUnits.SelectedIndex = 0;

			if (SyncfusionConverter.Active) // if converting include all controls
				includeList = includeBetween = includeLike = includeWithin = true;

			InList.Visible = ValueList.Visible = ImportList.Visible = EditList.Visible = includeList;
			if (includeList)
			{
				InList.Top = ValueList.Top = ImportList.Top = EditList.Top = loc;
				loc += spacing;
			}

			Between.Visible = Limit1.Visible = BetweenAnd.Visible = Limit2.Visible = includeBetween;
			if (includeBetween)
			{
				Between.Top = Limit1.Top = BetweenAnd.Top = Limit2.Top = loc;
				loc += spacing;
			}

			Like.Visible = Substring.Visible = includeLike;
			if (includeLike)
			{
				Like.Top = Substring.Top = loc;
				loc += spacing;
			}

			Within.Visible = WithinValue.Visible = WithinUnits.Visible = includeWithin;
			if (includeWithin)
			{
				Within.Top = WithinValue.Top = WithinUnits.Top = loc;
				loc += spacing;
			}

			IsNotNull.Top = loc;
			IsNull.Top = loc + spacing;
			All.Top = loc + spacing * 2;
			None.Top = loc + spacing * 3;

			Height = InitialHeight - (InitialNoneTop - None.Top);
			return;
		}


		private void Criteria_Activated(object sender, EventArgs e)
		{
			DisableValueFields();
			if (BasicOp.Checked)
			{
				Value.Enabled = true;
				if (Visible) Value.Focus();
			}

			else if (InList.Checked)
			{
				ValueList.Enabled = true;
				ImportList.Enabled = true;
				EditList.Enabled = true;
				if (Visible) ValueList.Focus();
			}

			else if (Between.Checked)
			{
				Limit1.Enabled = true;
				Limit2.Enabled = true;
				if (Visible) Limit1.Focus();
			}

			else if (Like.Checked)
			{
				Substring.Enabled = true;
				if (Visible) Substring.Focus();
			}

			else if (Within.Checked)
			{
				WithinValue.Enabled = true;
				WithinUnits.Enabled = true;
				if (Visible) WithinValue.Focus();
			}

			else if (All.Checked)
				All.Focus();

			else if (IsNotNull.Checked)
				IsNotNull.Focus();

			else if (IsNull.Checked)
				IsNull.Focus();

			else if (None.Checked)
				None.Focus();
		}

		private void BasicOpBut_Click(object sender, System.EventArgs e)
		{
			BasicOp.Checked = true;
			BasicOpMenu.Show(BasicOpBut, new Point(0, BasicOpBut.Height));
		}

		private void Value_Click(object sender, EventArgs e)
		{
			if (!BasicOp.Checked) BasicOp.Checked = true; // be sure option checked
			return;
		}

		private void Value_KeyDown(object sender, KeyEventArgs e)
		{ // do auto dropdown if nothing entered and list exists
			if (Value.Text.Length == 0 && Value.Properties.Items != null && Value.Properties.Items.Count > 0)
				Value.ShowPopup(); // correct?
		}

		private void BasicOp_CheckedChanged(object sender, EventArgs e)
		{
			DisableValueFields();
			Value.Enabled = true;
			if (Visible) Value.Focus();
		}

		private void InList_CheckedChanged(object sender, EventArgs e)
		{
			DisableValueFields();
			ValueList.Enabled = true;
			ImportList.Enabled = true;
			EditList.Enabled = true;
			if (Visible) ValueList.Focus();
		}

		private void ImportList_Click(object sender, EventArgs e)
		{
			string fileName = UIMisc.GetOpenFilename("List File to Import", "",
				"List Files (*.lst; *.txt)|*.lst; *.txt|All files (*.*)|*.*", "LST");
			if (fileName == "") return;

			StreamReader sr = new StreamReader(fileName);
			List<string> list = new List<string>();
			while (true)
			{
				string item = sr.ReadLine();
				if (item == null) break;
				if (item.IndexOf("\r") >= 0) item = item.Replace("\r", "");
				item = item.Trim();
				if (item == "") continue;
				list.Add(item);
			}
			sr.Close();

			string listText = Csv.JoinCsvString(list);
			ValueList.Text = listText;
			if (listText.Length > 0) // no text selected
				ValueList.Select(0, 0);
			return;
		}

		private void EditList_Click(object sender, EventArgs e)
		{
			string listText = ValueList.Text;
			List<string> list = Csv.SplitCsvString(listText); // only comma delimiters allowed here since some values (e.g. names) may contain spaces
			StringBuilder sb = new StringBuilder();
			foreach (string s in list)
			{
				sb.Append(s);
				sb.Append("\r\n");
			}

			string title = Qc.ActiveLabel + " List";
			listText = CriteriaList.Edit(sb.ToString(), title);
			if (listText != null)
				ValueList.Text = listText;
			return;
		}

		private void Between_CheckedChanged(object sender, EventArgs e)
		{
			DisableValueFields();
			Limit1.Enabled = true;
			Limit2.Enabled = true;
			if (Visible) Limit1.Focus();
		}

		private void Like_CheckedChanged(object sender, EventArgs e)
		{
			DisableValueFields();
			Substring.Enabled = true;
			if (Visible) Substring.Focus();
		}

		private void Within_CheckedChanged(object sender, EventArgs e)
		{
			DisableValueFields();
			WithinValue.Enabled = true;
			WithinUnits.Enabled = true;
			if (Visible) WithinValue.Focus();
		}

		private void IsNotNull_CheckedChanged(object sender, EventArgs e)
		{
			DisableValueFields();
		}

		private void IsNull_CheckedChanged(object sender, EventArgs e)
		{
			DisableValueFields();
		}

		private void All_CheckedChanged(object sender, EventArgs e)
		{
			DisableValueFields();
		}

		private void None_CheckedChanged(object sender, EventArgs e)
		{
			DisableValueFields();
		}

		void DisableValueFields()
		{
			//			Value.Enabled = false; // (don't disable to allow click within field to work)
			ValueList.Enabled = false;
			ImportList.Enabled = false;
			EditList.Enabled = false;
			Limit1.Enabled = false;
			Limit2.Enabled = false;
			Substring.Enabled = false;
			WithinValue.Enabled = false;
			WithinUnits.Enabled = false;
		}

		private void OK_Click(object sender, EventArgs e)
		{
			if (BasicOp.Checked && Value.Text.Trim() == "")
			{
				XtraMessageBox.Show("Criteria value is missing", UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.None);
				Value.Focus();
				return;
			}

			else if (InList.Checked && ValueList.Text.Trim() == "")
			{
				XtraMessageBox.Show("The list must contain one or more items", UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.None);
				ValueList.Focus();
				return;
			}

			else if (Between.Checked && (Limit1.Text.Trim() == "" || Limit2.Text.Trim() == ""))
			{
				XtraMessageBox.Show("Between value is missing", UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.None);
				if (Limit1.Text.Trim() == "") Limit1.Focus();
				else if (Limit2.Text.Trim() == "") Limit2.Focus();
				return;
			}

			else if (Like.Checked && Substring.Text == "")
			{
				XtraMessageBox.Show("Substring value is missing", UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.None);
				Substring.Focus();
				return;
			}

			else if (Within.Checked && WithinValue.Text == "")
			{
				XtraMessageBox.Show("Within value is missing", UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.None);
				WithinValue.Focus();
				return;
			}

			if (!GetCriteria ()) return;
			DialogResult = DialogResult.OK;
			this.Hide();
		}

/// <summary>
/// Get criteria & validate
/// </summary>

		bool GetCriteria()
		{
			string op = null, val, val2, eval, lim1, lim2, elim1, elim2, txt;
			bool allowSpaceDelimiters;

			QueryColumn qc = Qc;
			MetaColumn mc = qc.MetaColumn;

			if (BasicOp.Checked)
			{
				if (BasicOpBut.Text == "Equals") op = "=";
				else if (BasicOpBut.Text == "<") op = "<";
				else if (BasicOpBut.Text == UnicodeString.LessOrEqual) op = "<=";
				else if (BasicOpBut.Text == UnicodeString.NotEqual) op = "<>";
				else if (BasicOpBut.Text == ">") op = ">";
				else if (BasicOpBut.Text == UnicodeString.GreaterOrEqual) op = ">=";

				val = Value.Text;
				if ((eval = CheckCriteriaValue(qc, val)) == null) return false;
				qc.CriteriaDisplay = op + " " + val;
				qc.Criteria = mc.Name + " " + op + " " + eval;
			}

			else if (InList.Checked)
			{
				val = ValueList.Text;
				StringBuilder listSql = new StringBuilder();
				if (qc.MetaColumn.Dictionary == "") // if no dictionary allow spaces to be used as delimiters
					allowSpaceDelimiters = true;
				else allowSpaceDelimiters = false;
				List<string> list = Csv.SplitCsvString(val, allowSpaceDelimiters);

				eval = null;
				foreach (string item in list)
				{
					if ((eval = CheckCriteriaValue(qc, item)) == null) return false;
					if (listSql.Length > 0) listSql.Append(",");
					listSql.Append(eval);
				}

				qc.Criteria = mc.Name + " in (" + listSql.ToString() + ")";

				string listText = Csv.JoinCsvString(list); // and reformat properly for display
				qc.CriteriaDisplay = listText;
			}

			else if (Between.Checked)
			{
				lim1 = Limit1.Text;
				lim2 = Limit2.Text;
				if ((elim1 = CheckCriteriaValue(qc, lim1)) == null) return false;
				if ((elim2 = CheckCriteriaValue(qc, lim2)) == null) return false;

				qc.CriteriaDisplay = "Between " + lim1 + " and " + lim2;
				qc.Criteria = mc.Name + " BETWEEN " + elim1 + " AND " + elim2;
			}

			else if (Like.Checked)
			{
				val = Substring.Text;
				if ((eval = CheckCriteriaValue(qc, val)) == null) return false;

				eval = Lex.RemoveSingleQuotes(eval);
				if (val.IndexOf("%") < 0 && val.IndexOf("_") < 0) eval = "%" + eval + "%"; // need to add wildcards?
				eval = Lex.AddSingleQuotes(eval);

				qc.CriteriaDisplay = "Contains " + val;
				qc.Criteria = mc.Name + " LIKE " + eval;
			}

			else if (Within.Checked)
			{
				val = WithinValue.Text;
				val2 = WithinUnits.Text;
				if (!Lex.IsDouble(val))
				{
					MessageBoxMx.Show(val + " is not a valid number", UmlautMobius.String,
					 MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return false;
				}

				qc.CriteriaDisplay = "Within the last " + val + " " + val2;
				qc.Criteria = mc.Name + " WITHIN " + val + " " + val2;
			}

			else if (IsNotNull.Checked)
			{
				qc.CriteriaDisplay = "Exists";
				qc.Criteria = mc.Name + " IS NOT NULL";
			}

			else if (IsNull.Checked)
			{
				qc.CriteriaDisplay = "Doesn't exist";
				qc.Criteria = mc.Name + " IS NULL";
			}

			else if (All.Checked)
			{
				qc.CriteriaDisplay = "All data rows";
				qc.Criteria = "(" + mc.Name + " IS NOT NULL OR " + mc.Name + " IS NULL)";
			}

			else if (Instance.None.Checked)
			{
				qc.CriteriaDisplay = "";
				qc.Criteria = "";
			}

			else throw new Exception("Unexpected criteria form operator");

			return true; // success if no errors
		}

		/// <summary>
		/// Parse an operator and value
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="op"></param>
		/// <param name="val"></param>
		/// <returns>Expert form of criteria if form is acceptable</returns>

		static string CheckCriteriaValue(
			QueryColumn qc,
			string val)
		{
			string eval, txt, txt2, txt3;
			int i1;
			double d1;

			MetaColumn mc = qc.MetaColumn;

			if (val == null) return null;

			val = eval = val.Trim();

			switch (mc.DataType)
			{

				case MetaColumnType.Date:
					eval = DateTimeMx.Normalize(val);
					if (eval == null)
					{
						MessageBoxMx.Show(val + " is not a valid date", UmlautMobius.String,
							MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						return null;
					}
					break;

				case MetaColumnType.Integer:
				case MetaColumnType.Number:
				case MetaColumnType.QualifiedNo:
					try
					{ d1 = Convert.ToDouble(val); }
					catch (Exception e)
					{
						MessageBoxMx.Show(val + " is not a valid number", UmlautMobius.String,
							MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						return null;
					}
					break;

				case MetaColumnType.String: // quoted form
				case MetaColumnType.MolFormula:
					eval = Lex.AddSingleQuotes(Lex.RemoveSingleQuotes(val));
					break;

				case MetaColumnType.DictionaryId: // translate dictionary value back to database value
					eval = DictionaryMx.LookupDefinition(mc.Dictionary, val); // get database value
					if (eval == null || eval == "")
					{
						MessageBoxMx.Show(Lex.Dq(val) + " is not a valid value.\nYou must select an item from the dropdown box.", UmlautMobius.String,
						 MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						return null;
					}
					eval = Lex.AddSingleQuotes(Lex.RemoveSingleQuotes(eval)); // quote in case string value
					break;

			} // end of datatype case

			return eval;
		} // end of CheckCriteriaValue


		private void Cancel_Click(object sender, EventArgs e)
		{
			this.Hide();
		}

		private void menuEQ_Click(object sender, System.EventArgs e)
		{
			BasicOp.Checked = true;
			BasicOpBut.Text = "Equals";
			Value.Focus();
		}

		private void menuLT_Click(object sender, System.EventArgs e)
		{
			BasicOp.Checked = true;
			BasicOpBut.Text = "<";
			Value.Focus();
		}

		private void menuLE_Click(object sender, System.EventArgs e)
		{
			BasicOp.Checked = true;
			BasicOpBut.Text = UnicodeString.LessOrEqual;
			Value.Focus();
		}

		private void menuNE_Click(object sender, System.EventArgs e)
		{
			BasicOp.Checked = true;
			BasicOpBut.Text = UnicodeString.NotEqual;
			Value.Focus();
		}

		private void menuGT_Click(object sender, System.EventArgs e)
		{
			BasicOp.Checked = true;
			BasicOpBut.Text = ">";
			Value.Focus();
		}

		private void menuGE_Click(object sender, System.EventArgs e)
		{
			BasicOp.Checked = true;
			BasicOpBut.Text = UnicodeString.GreaterOrEqual;
			Value.Focus();
		}

		private void LabelControl_Click(object sender, EventArgs e)
		{

		}
	}
}