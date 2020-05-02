using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

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
	public partial class CriteriaComplex : DevExpress.XtraEditors.XtraForm
	{
		static CriteriaComplex Instance;
		Query Query;
		int CurrentEditStructureHighlightLength = "[Edit Structure Sx]".Length;
		LabeledCriteria LabeledCriteria;

		public CriteriaComplex()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Complex criteria editor
		/// 
		/// Editing complex criteria involves several steps including the following:
		///  1. ConvertSimpleCriteriaToComplex (MqlUtil) - Initial conversion to set up complex criteria
		///  2. ConvertComplexCriteriaToEditable - Convert raw complex criteria into a more editable form 
		///  3. ConvertEditableCriteriaToComplex - Convert editable format back to raw format checking for basic errors
		///  4. ConvertComplexCriteriaToSimple (MqlUtil) - Convert back to simple format, may be loss of criteria
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		public static bool Edit(
			Query q)
		{
			if (Instance == null) Instance = new CriteriaComplex();
			Instance.Query = q;

			QueryTable qt;
			MoleculeMx cs;

			DataTable dt = BuildTableGridList(q);
			Instance.TableGrid.DataSource = dt;

			Instance.LabeledCriteria = CriteriaEditor.ConvertComplexCriteriaToEditable(q, true);
			RichTextBox coloredCriteria = CriteriaEditor.ColorCodeCriteria(Instance.LabeledCriteria.Text, q);
			string formattedCriteria = coloredCriteria.Rtf;
			Instance.Criteria.Rtf = formattedCriteria;

			if (coloredCriteria.Text.Length == 1)
			{ // if single starter char then position cursor at start
				Instance.Criteria.SelectionStart = 0;
				Instance.Criteria.SelectionLength = 0;
			}

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.OK) return true; 
			else return false;
		}

		/// <summary>
		/// Build list for filling table list grid
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		static DataTable BuildTableGridList(
			Query q)
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("TableName", typeof(string));
			dt.Columns.Add("Criteria", typeof(string));

			for (int ti = 0; ti < q.Tables.Count; ti++)
			{
				DataRow dr = dt.NewRow();
				QueryTable qt = q.Tables[ti];
				dr[0] = qt.ActiveLabel;
				dr[1] =	"Add Field...";
				dt.Rows.Add(dr);
			}

			return dt;
		}

		/// <summary>
		/// Add new criteria element and color appropriately
		/// </summary>
		/// <param name="criteria"></param>

		void InsertCriteria(
			string newCriteria)
		{
			int ss = Criteria.SelectionStart; // where new text will go
			RichTextBox rtb = new RichTextBox(); // make changes in copy
			rtb.Rtf = Criteria.Rtf;
			rtb.SelectionStart = Criteria.SelectionStart;
			rtb.SelectionLength = Criteria.SelectionLength;
			rtb.SelectionFont = Criteria.Font;

			string txt = PadText(newCriteria);
			rtb.SelectedText = txt;
			rtb.SelectionFont = Criteria.Font;
			int newSs = rtb.SelectionStart; // where to put cursor when done
			int newSl = 0; // nothing selected

			bool inSingleQuote = false;
			for (int i1 = 0; i1 < txt.Length; i1++)
			{ // color keywords blue
				if (txt[i1] == ' ') continue;
				rtb.SelectionStart = ss + i1;
				rtb.SelectionLength = 1;
				if (txt[i1] == '\'')
					inSingleQuote = !inSingleQuote;

				if (inSingleQuote) // color text constants red
					rtb.SelectionColor = Color.Red;
				else rtb.SelectionColor = Color.Blue;
			}

			ColorRtfSubstring(rtb, ss, txt.Length, " A ", Color.Black); // color field names black
			ColorRtfSubstring(rtb, ss, txt.Length, " B ", Color.Black);
			ColorRtfSubstring(rtb, ss, txt.Length, "...", Color.Black);
			ColorRtfSubstring(rtb, ss, txt.Length, "structure_field", Color.Black);

			ColorRtfSubstring(rtb, ss, txt.Length, "1", Color.Red); // color constants red
			ColorRtfSubstring(rtb, ss, txt.Length, "90", Color.Red);
			//			ColorRtfSubstring(rtb,ss,txt.Length,"'all'",Color.Red);
			//			ColorRtfSubstring(rtb,ss,txt.Length,"'normal'",Color.Red);

			int sePos = rtb.Text.Substring(ss, txt.Length).IndexOf("[Edit Structure ");
			if (sePos >= 0) // highlight [Edit Structure Sx]
			{
				rtb.SelectionStart = ss + sePos;
				rtb.SelectionLength = CurrentEditStructureHighlightLength;
				rtb.SelectionColor = Color.Red;
			}

			string sfText = "structure_field";
			int htPos = rtb.Text.Substring(ss, txt.Length).IndexOf(sfText);
			if (htPos >= 0) // set to select "structure_field"
			{
				newSs = ss + htPos;
				newSl = sfText.Length;
			}

			htPos = rtb.Text.Substring(ss, txt.Length).IndexOf("...");
			if (htPos >= 0) // select elipsis
			{
				newSs = ss + htPos;
				newSl = 3;
			}

			htPos = rtb.Text.Substring(ss, txt.Length).IndexOf(" A ");
			if (htPos >= 0) // select "A" place holder
			{
				newSs = ss + htPos + 1;
				newSl = 1;
			}

			Criteria.SelectionStart = 0; // replace all rtf text
			Criteria.SelectionLength = Criteria.Text.Length;
			Criteria.SelectedRtf = rtb.Rtf;

			Criteria.SelectionStart = newSs; // restore cursor to proper position
			Criteria.SelectionLength = newSl;
			Criteria.Focus();
			return;
		}

		/// <summary>
		/// Color matching substring with RichTextBox
		/// </summary>
		/// <param name="crt"></param>
		/// <param name="substring"></param>
		/// <param name="color"></param>
		void ColorRtfSubstring(
			RichTextBox rtb,
			int start,
			int length,
			string substring,
			Color color)
		{
			int i1 = rtb.Text.Substring(start, length).IndexOf(substring);
			if (i1 < 0) return;

			int ss = rtb.SelectionStart;
			int sl = rtb.SelectionLength;

			rtb.SelectionStart = start + i1;
			rtb.SelectionLength = substring.Length;
			rtb.SelectionColor = color;

			rtb.SelectionStart = ss;
			rtb.SelectionLength = sl;

			return;
		}

		/// <summary>
		/// Pad Rtf string with spaces relative to text selected in Criteria control
		/// </summary>
		/// <param name="rtf"></param>
		string PadText(
			string txt)
		{

			int ss = Criteria.SelectionStart;
			int sl = Criteria.SelectionLength;
			int se = ss + sl;

			if (ss > 0 && Criteria.Text[ss - 1] != ' ' && Criteria.Text[ss - 1] != '\n')
				txt = " " + txt;

			if ((se < Criteria.Text.Length && Criteria.Text[se] != ' ') ||
				se == Criteria.Text.Length)
				txt += " ";

			return txt;
		}

		private void AddCriteria_Click(object sender, EventArgs e)
		{
			AddCriteriaPopup.Show(AddCriteria,
				new Point(0, AddCriteria.Size.Height));
		}

		private void Criteria_MouseDown(object sender, MouseEventArgs e)
		{
			int ci = GetEditStructureIndex(e.Location);
			if (ci < 0) return;

			DelayedCallback.Schedule(DelayedEditMolecule, ci);
		}

		private void DelayedEditMolecule(object arg)
		{
			try
			{
				int ci = (int)arg;
				string txt = Criteria.Text.Substring(ci + 16); // pick up structure name
				int i1 = txt.IndexOf("]");
				if (i1 < 0) return;
				string sid = txt.Substring(0, i1).Trim().ToUpper();

				string chime = "";
				if (LabeledCriteria.Structures.ContainsKey(sid))
					chime = LabeledCriteria.Structures[sid];
				MoleculeMx mol = new MoleculeMx(MoleculeFormat.Chime, chime);

				MoleculeMx mol2 = MoleculeEditor.Edit(mol, MoleculeRendererType.Unknown, "Edit Structure");
				if (mol2 == null) return;

				if (mol2.IsChemStructureFormat) // store chime for chem structure
					mol2.ConvertTo(MoleculeFormat.Chime);

				LabeledCriteria.Structures[sid] = mol2.GetPrimaryTypeAndValueString();
				return;
			}

			catch (Exception ex)
			{
				return;
			}
		}

		/// <summary>
		/// See if point is over [Edit Structure Sx]
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>

		int GetEditStructureIndex(
			Point p)
		{
			int ci = Criteria.GetCharIndexFromPosition(p);
			if (ci < 0) return -1;
			for (ci = ci - 1; ci >= 0; ci--)
			{
				if (Criteria.Text[ci] == ']') return -1;
				if (Criteria.Text.Substring(ci).ToLower().StartsWith("[edit structure "))
					return ci;
			}
			return -1;
		}

		private void CriteriaComplex_Activated(object sender, EventArgs e)
		{
			Criteria.Focus();
		}

		/// <summary>
		/// Add field to query
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void SelectedField_Click(object sender, System.EventArgs e)
		{
			ToolStripMenuItem mi = (ToolStripMenuItem)sender;
			string txt = Lex.Dq(mi.Tag + "." + mi.Text);
			txt = PadText(txt);
			Criteria.SelectedText = txt;
			Criteria.SelectionFont = Criteria.Font;
			Criteria.Focus();
		}

		private void EQ_Click(object sender, System.EventArgs e)
		{
			InsertCriteria("=");
		}

		private void LT_Click(object sender, EventArgs e)
		{
			InsertCriteria("<");
		}

		private void LE_Click(object sender, EventArgs e)
		{
			InsertCriteria("<=");
		}

		private void GT_Click(object sender, EventArgs e)
		{
			InsertCriteria(">");
		}

		private void GE_Click(object sender, EventArgs e)
		{
			InsertCriteria(">=");
		}

		private void NE_Click(object sender, EventArgs e)
		{
			InsertCriteria("<>");
		}

		private void In_Click(object sender, EventArgs e)
		{
			InsertCriteria("In (...)");
		}

		private void NotIn_Click(object sender, EventArgs e)
		{
			InsertCriteria("Not In (...)");
		}

		private void Between_Click(object sender, EventArgs e)
		{
			InsertCriteria("Between A and B");
		}

		private void NotBetween_Click(object sender, EventArgs e)
		{
			InsertCriteria("Not Between A and B");
		}

		private void Like_Click(object sender, EventArgs e)
		{
			InsertCriteria("Like");
		}

		private void NotLike_Click(object sender, EventArgs e)
		{
			InsertCriteria("Not Like");
		}

		private void IsNull_Click(object sender, EventArgs e)
		{
			InsertCriteria("Is Null");
		}

		private void IsNotNull_Click(object sender, EventArgs e)
		{
			InsertCriteria("Is Not Null");
		}

		private void InSavedList_Click(object sender, EventArgs e)
		{
			Criteria.Focus();
			UserObject uo = CidListCommand.SelectListDialog("In Saved List");
			if (uo == null) return;
			string listName = uo.InternalName;
			string criteria = "In List " + Lex.AddSingleQuotes(listName);
			Instance.InsertCriteria(criteria);
		}

		private void StructureSearch_Click(object sender, EventArgs e)
		{
			QueryColumn qc = new QueryColumn();
			MetaColumn mc = new MetaColumn(); // create minimal metacolumn with structure type to assoc with qc
			mc.DataType = MetaColumnType.Structure;
			qc.MetaColumn = mc;

			bool gotCriteria = CriteriaStructure.Edit(qc);
			if (!gotCriteria || qc.Criteria == "") return;
			string sid = "";
			for (int si = 1; ; si++)
			{
				sid = "S" + si.ToString();
				if (!LabeledCriteria.Structures.ContainsKey(sid.ToUpper())) break;
			}

			string chime = MoleculeMx.MolFileStringToChimeString(qc.MolString); // get chime string
			LabeledCriteria.Structures[sid.ToUpper()] = chime; // store sid & associated chime string
			qc.MolString = chime;
			string sCriteria = MqlUtil.ConvertQueryColumnStructureCriteriaToMql(qc);
			sCriteria = sCriteria.Replace("(ctab,", "(structure_field,"); // required dummy field name
			sCriteria = sCriteria.Replace(chime, "[Edit Structure " + sid + "]"); // substitute surrogate for structure
			sCriteria = sCriteria.Replace("'[", "["); // hack fixup
			sCriteria = sCriteria.Replace("]'", "]");

			Instance.InsertCriteria(sCriteria);
		}

		private void And_Click(object sender, EventArgs e)
		{
			InsertCriteria("And");
		}

		private void Or_Click(object sender, EventArgs e)
		{
			InsertCriteria("Or");
		}

		private void Not_Click(object sender, EventArgs e)
		{
			InsertCriteria("Not");
		}

		private void Parens_Click(object sender, EventArgs e)
		{
			InsertCriteria("(...)");
		}

		private void OK_Click(object sender, EventArgs e)
		{
			LabeledCriteria.Text = Criteria.Text; // get current text
			if (!CriteriaEditor.ConvertEditableCriteriaToComplex(LabeledCriteria, Query, null)) return;
			DialogResult = DialogResult.OK;
			return;
		}

		private void gridView1_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
		{
			if (e.Info.IsRowIndicator)
			{
				e.Info.DisplayText = "T" + (e.RowHandle + 1).ToString();
				e.Info.ImageIndex = -1; // no image
			}
		}

		private void gridView1_MouseDown(object sender, MouseEventArgs e)
		{

			Point p;
			int ri, ci;

			GridHitInfo ghi = GridView.CalcHitInfo(e.Location);

			if (ghi.Column.AbsoluteIndex != 1) return;

			ri = ghi.RowHandle;
			QueryTable qt = Query.Tables[ri];
			ContextMenuStrip colPopup = new ContextMenuStrip();

			bool firstLabel = true;
			foreach (QueryColumn qc in qt.QueryColumns)
			{ // get list of allowed field labels/names for each table
				if (!QueryTableControl.QueryColumnVisible(qc) || !qc.MetaColumn.IsSearchable) continue;
				if (firstLabel) firstLabel = false;
				string label = CriteriaEditor.GetUniqueColumnLabel(qc);
				ToolStripMenuItem mi = new ToolStripMenuItem(label, null, new System.EventHandler(SelectedField_Click));
				mi.Tag = "T" + (ri + 1);
				colPopup.Items.Add(mi);
			}

			p = e.Location;

			GridViewInfo viewInfo = (GridViewInfo)GridView.GetViewInfo();

			GridCellInfo cellInfo = viewInfo.GetGridCellInfo(ghi.RowHandle, ghi.Column);
			if (cellInfo != null)
				p = new Point(cellInfo.Bounds.Left, cellInfo.Bounds.Bottom);

			 colPopup.Show(Instance.TableGrid, p);
		}
	}
}