using Mobius.Data;
using Mobius.ComOps;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class CriteriaPanel : XtraUserControl
	{
		internal Query Query; // current query
		internal int CriteriaTabYPos; // position for next criteria tab control
		Control CurrentCriteriaControl; // current control
		QueryColumn CurrentQc; // QueryColumn assoc with current control
		bool Rendering = false;

		public CriteriaPanel()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);

			CtStructBox.Height = 141; // restore normal height
			//Clear(); // Remove controls from criteria panel to allow later rebuild
		}

		/// <summary>
		/// Remove controls from criteria tab
		/// </summary>

		internal void Clear()
		{
			while (ScrollablePanel.Controls.Count > 0) // clear criteria tab panel
				ScrollablePanel.Controls.RemoveAt(0);
		}

		/// <summary>
		/// Render
		/// </summary>
		/// <param name="query"></param>

		internal void Render(
			Query query)
		{
			Query = query;  // make this the current query
			Render();
		}

		internal void Render()
		{
			Query q = Query;
			int criteriaCount = 0;

			if (q == null || q.Tables.Count <= 0) return;
			//if (q.Tables.Count <= 0 || !SS.I.ShowCriteriaTab) return;

			Rendering = true;
			Clear();
			CriteriaTabYPos = 0;

			bool shownKeyCriteria = false;
			int summarizable = 0, summarized = 0;

			for (int ti = 0; ti < q.Tables.Count; ti++)
			{
				QueryTable qt = q.Tables[ti];
				string txt;
				bool shownTableLabel = false;

				if (qt.MetaTable.SummarizedExists) summarizable++;
				if (qt.MetaTable.UseSummarizedData) summarized++;

				if (q.LogicType == QueryLogicType.Complex)
					continue; // if complex logic don't show simple criteria

				if (qt.MetaTable.IgnoreCriteria)
					continue; // skip if criteria should be ignored

				foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (qc.IsKey)
					{
						if (shownKeyCriteria) continue; // skip if already shown
						shownKeyCriteria = true;
					}

					else if (qc.Criteria == "" && !qc.ShowOnCriteriaForm) continue;

					if (qc.MetaColumn.IsDatabaseSetColumn) continue; // don't show db list

					if (!shownTableLabel)
					{
						shownTableLabel = true;
						if (!qt.MetaTable.IsRootTable || ti > 0)
							AddCriteriaTabTableLabel(qt);
					}

					if (qc.MetaColumn.DataType == MetaColumnType.Structure)
					{
						AddCriteriaTabStructureItem(qc);
						criteriaCount++;
					}

					else // other column types
					{
						if (qc.IsKey)
							txt = q.KeyCriteriaDisplay;

						else
						{
							txt = qc.CriteriaDisplay;
							if (txt != "")
							{
								if (qc.FilterSearch && !qc.FilterRetrieval) txt += " (Search only)";
								else if (!qc.FilterSearch && qc.FilterRetrieval) txt += " (Retrieve only)";
								else if (!qc.FilterSearch && !qc.FilterRetrieval) txt += " (Disabled)";
							}
						}

						AddCriteriaTabItem(qc, txt);
						if (qc.IsKey)
							criteriaCount++;
					}
				}
			}

			if (q.LogicType == QueryLogicType.Complex)
			{ // convert & show complex logic
				AddComplexCriteriaPanel();
				LabeledCriteria labeledCriteria = CriteriaEditor.ConvertComplexCriteriaToEditable(q, false);
				RichTextBox coloredCriteria = CriteriaEditor.ColorCodeCriteria(labeledCriteria.Text, q);
				CtComplexCriteria.Rtf =  coloredCriteria.Rtf;

				MenuComplexApplyCriteriaToRetrieval.Checked = q.FilterResults;
			}

			AddCriteriaLogicPanel(q.LogicType);  // add criteria type panel
			AddQueryDescription(q.UserObject.Description.TrimEnd()); // add description panel
			Rendering = false;
			return;
		}

		/// <summary>
		/// Add a label to the criteria tab
		/// </summary>
		/// <param name="text"></param>

		void AddCriteriaTabTableLabel(
			QueryTable qt)
		{
			LabelControl label = new LabelControl();

			label.AutoSize = true;
			label.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			label.ForeColor = System.Drawing.Color.Blue;
			label.Location = new System.Drawing.Point(8, CriteriaTabYPos + 8);
			label.Name = "CtTableLabel";
			label.Size = new System.Drawing.Size(69, 17);
			label.TabIndex = 150;
			label.Text = qt.ActiveLabel;
			label.Tag = qt;
			label.MouseDown += new System.Windows.Forms.MouseEventHandler(CtTableLabel_MouseDown);

			ScrollablePanel.Controls.Add(label);

			CriteriaTabYPos += 20;
		}

		/// <summary>
		/// Add an item to the criteria tab
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="criteriaText"></param>

		void AddCriteriaTabItem(
			QueryColumn qc,
			string criteriaText)
		{

			// 
			// CtColumnLabel
			// 
			LabelControl label = AddColumnLabel(qc.ActiveLabel);
			int textBoxX = 142;
			if (label.Left + label.Width > textBoxX) // shift textbox as needed so we see full label
				textBoxX = label.Left + label.Width + 4;
			else label.Text += " . . . . . . . . . . . ."; // connect label to TextBox

			// 
			// CtTextBox
			// 
			TextEdit textBox = new TextEdit();

			textBox.Font = new System.Drawing.Font("Tahoma", 9, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			textBox.Location = new System.Drawing.Point(textBoxX, CriteriaTabYPos + 6);
			textBox.Name = "CtTextBox";
			textBox.Size = new System.Drawing.Size(200, 21);
			textBox.TabIndex = 152;
			textBox.Text = criteriaText;
			textBox.Properties.ContextMenuStrip = this.CriteriaTabRtClickContextMenu;
			textBox.KeyDown += new System.Windows.Forms.KeyEventHandler(CtTextBox_KeyDown);
			textBox.MouseDown += new System.Windows.Forms.MouseEventHandler(CtTextBox_MouseDown);

			textBox.Tag = qc.MetaTableDotMetaColumnName;

			ScrollablePanel.Controls.Add(textBox);
			ScrollablePanel.Controls.Add(label); // add label 2nd so it is behind textbox

			CriteriaTabYPos += 24;
			return;
		}

		/// <summary>
		/// Create label for criteria tab column
		/// </summary>
		/// <param name="labelText"></param>
		/// <returns></returns>

		LabelControl AddColumnLabel(
			string labelText)
		{
			LabelControl label = new LabelControl();
			label.AutoSize = true;
			label.Font = new System.Drawing.Font("Tahoma", 9, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			label.ForeColor = System.Drawing.Color.Black;
			label.Location = new System.Drawing.Point(16, CriteriaTabYPos + 8);
			label.Name = "CtColumnLabel";
			label.Size = new System.Drawing.Size(81, 17);
			label.TabIndex = 151;
			label.Text = labelText;
			return label;
		}

		/// <summary>
		/// Add a structure criteria item to the criteria tab
		/// </summary>
		/// <param name="text"></param>
		/// <param name="tag"></param>
		
		void AddCriteriaTabStructureItem(
			QueryColumn qc)
		{
			LabelControl label = AddColumnLabel(qc.ActiveLabel);
			int structBoxX = 142;
			if (label.Left + label.Width > structBoxX) // shift textbox as needed
				structBoxX = label.Left + label.Width + 4;
			else label.Text += " . . . . . . . . . . . .";

// Build the structure control

			PictureEdit structBox = new DevExpress.XtraEditors.PictureEdit();
			structBox.Location = new System.Drawing.Point(142, CriteriaTabYPos + 6);
			structBox.Name = "CtStructBox";
			structBox.Size = new System.Drawing.Size(200, 136);

			structBox.Tag = qc.QueryTable.MetaTable.Name + "." + qc.MetaColumn.Name;
			structBox.Properties.ContextMenuStrip = this.CriteriaTabRtClickContextMenu;

			int pixWidth = structBox.Width;
			Font font = structBox.Font;
			Bitmap bm = QueryTableControl.GetQueryMoleculeBitmap(qc, pixWidth, font);
			int height = bm.Height;
			if (height < 20) height = 20;
			structBox.Height = height;
			structBox.Image = bm;

			structBox.MouseDown += new System.Windows.Forms.MouseEventHandler(CtStructBox_MouseDown);

			ScrollablePanel.Controls.Add(structBox);
			ScrollablePanel.Controls.Add(label);
			CtStructBox = structBox; // so references to CtStructBox refer to new box

			CriteriaTabYPos += structBox.Height + 4;
			return;
		}

		/// <summary>
		/// Add panel showing complex logic
		/// </summary>
		/// <param name="intLogicType"></param>

		void AddComplexCriteriaPanel()
		{
			CtComplexCriteriaPanel.Location = new System.Drawing.Point(0, CriteriaTabYPos + 0);
			ScrollablePanel.Controls.Add(CtComplexCriteriaPanel);

			CriteriaTabYPos += CtComplexCriteriaPanel.Height;
			return;
		}

		/// <summary>
		/// Add type of logic
		/// </summary>
		/// <param name="logicType"></param>

		void AddCriteriaLogicPanel(
			QueryLogicType logicType)
		{
			if (logicType == QueryLogicType.And) CtUseAndLogic.Checked = true;
			else if (logicType == QueryLogicType.Or) CtUseOrLogic.Checked = true;
			else CtUseComplexLogic.Checked = true;

			CtLogicPanel.Location = new System.Drawing.Point(0, CriteriaTabYPos + 6);
			ScrollablePanel.Controls.Add(CtLogicPanel);

			CriteriaTabYPos += CtLogicPanel.Height;
			return;
		}

		/// <summary>
		/// Add any query description to criteria tab
		/// </summary>
		/// <param name="desc"></param>
		/// <param name="enable"></param>

		void AddQueryDescription(
			string desc)
		{
			QueryDesc.Text = desc;

			QueryDescPanel.Location = new System.Drawing.Point(0, CriteriaTabYPos + 14);
			QueryDescPanel.Width = Width;

			ScrollablePanel.Controls.Add(QueryDescPanel);

			CriteriaTabYPos += QueryDescPanel.Height + 2;
		}

		///////////////////////////////////////////////////////////////
		////////////////////// Event code /////////////////////////
		///////////////////////////////////////////////////////////////

		private void CtUseAndLogic_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("CriteriaPanelUseAndLogic");
			ChangeLogicType(QueryLogicType.And);
		}

		private void CtUseOrLogic_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("CriteriaPanelUseOrLogic");
			ChangeLogicType(QueryLogicType.Or);
		}

		private void CtUseComplexLogic_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("CriteriaPanelUseComplexLogic");
			ChangeLogicType(QueryLogicType.Complex);
		}

		void ChangeLogicType (QueryLogicType newType)
		{
			if (newType == Query.LogicType) return;

			if (Query.LogicType == QueryLogicType.Complex) // turning off complex logic
			{
				string logicTypeString = "And";
				if (newType == QueryLogicType.Or) logicTypeString = "Or";
				if (XtraMessageBox.Show(
					"Switching from \"Advanced\" to \"" + logicTypeString + "\" logic may cause the loss of some criteria logic.\n" +
					"Do you want to continue?",
					UmlautMobius.String, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
					!= DialogResult.Yes)
				{ // set back to advanced
					CtUseComplexLogic.Checked = true;
					return;
				}

				else // switching to And or Or logic
				{
					if (newType == QueryLogicType.And)
						MqlUtil.ConvertComplexCriteriaToQueryColumnCriteria(Query, QueryLogicType.And);

					else MqlUtil.ConvertComplexCriteriaToQueryColumnCriteria(Query, QueryLogicType.Or);
				}
			}

			else if (newType == QueryLogicType.Complex) // turning on complex logic
			{
				MqlUtil.ConvertQueryColumnCriteriaToComplexCriteria(Query);
				Query.ClearAllQueryColumnCriteria();
			}

			Query.LogicType = newType;
			Render();
			return;
		}

		private void CtComplexCriteria_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right) // right button ContextMenu
			{
				Point p = new Point(e.X, e.Y);
				CriteriaComplexRtClickContextMenu.Show(CtComplexCriteria, p);
			}

			else
			{
				if (CriteriaComplex.Edit(Query))
				{
					LabeledCriteria labeledCriteria = CriteriaEditor.ConvertComplexCriteriaToEditable(Query, false);
					RichTextBox coloredCriteria = CriteriaEditor.ColorCodeCriteria(labeledCriteria.Text, Query);
					CtComplexCriteria.Rtf = coloredCriteria.Rtf;
				}
			}
		}

		/// <summary>
		/// Respond to click on criteria tab table label
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void CtTableLabel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			LabelControl label = (LabelControl)sender;
			QueryTable qt = label.Tag as QueryTable;
			if (qt == null) return;

			QueryTableControl.ShowTableLabelDropdown(qt, label);

			return;
		}

/// <summary>
/// MouseDown on TextBox criteria
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void CtTextBox_MouseDown(object sender, MouseEventArgs e)
		{
			TextEdit textBox = (TextEdit)sender;
			string tag = (string)textBox.Tag;
			TextOrStructBox_MouseDown(textBox, e, tag);
		}

		/// <summary>
		/// KeyDown on TextBox criteria
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void CtTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			TextEdit textBox = (TextEdit)sender;
			string tag = (string)textBox.Tag;
			TextOrStructBox_KeyDown(textBox, e, tag);
		}

		/// <summary>
/// Respond to mouse-down on structure
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void CtStructBox_MouseDown(object sender, MouseEventArgs e)
		{
			PictureEdit structBox = (PictureEdit)sender;
			string tag = (string)structBox.Tag;
			TextOrStructBox_MouseDown(structBox, e, tag);
		}

		private void CtStructBox_KeyDown(object sender, KeyEventArgs e)
		{
			PictureEdit structBox = (PictureEdit)sender;
			string tag = (string)structBox.Tag;
			TextOrStructBox_KeyDown(structBox, e, tag);
		}

		private void TextOrStructBox_MouseDown(Control sender, MouseEventArgs e, string tag)
		{
			if (tag == null) return;

			if (Query.LogicType == QueryLogicType.Complex)
			{
				MessageBoxMx.ShowError(
					"Since Advanced Criteria Logic has been selected\n" +
					"for this query you must use the advanced criteria\n" +
					"text box on the Criteria Summary tab to edit criteria.");
				return;
			}

			string[] sa = tag.Split('.');
			if (sa.Length < 2) return;
			QueryTable qt = Query.GetTableByName(sa[0]);
			if (qt == null) return;
			QueryColumn qc = qt.GetQueryColumnByName(sa[1]);
			if (qc == null) return;

			CurrentCriteriaControl = sender;
			CurrentQc = qc;

			if (e.Button == MouseButtons.Left)
				EditCriteriaMenuItem_Click(sender, e);
		}

// CriteriaColRtClickContextMenu events

		private void EditCriteriaMenuItem_Click(object sender, EventArgs e)
		{
			if (CriteriaEditor.EditCriteria(CurrentQc))
			{

				QueryColumn qc = CurrentQc;
				if (qc.MetaColumn.DataType == MetaColumnType.Structure)
					Render(); // rerender since height of box may have changed

				else // simple text value
				{
					TextEdit te = (TextEdit)CurrentCriteriaControl;
					if (qc.CriteriaDisplay != "") te.Text = qc.CriteriaDisplay;
					else te.Text = "";
				}
			}

			this.Parent.Focus(); // move focus away from any current text control
		}

		private void TextOrStructBox_KeyDown(Control sender, KeyEventArgs e, string tag)
		{
			if (tag == null) return;

			if (Query.LogicType == QueryLogicType.Complex)
			{
				MessageBoxMx.ShowError(
					"Since Advanced Criteria Logic has been selected\n" +
					"for this query you must use the advanced criteria\n" +
					"text box on the Criteria Summary tab to edit criteria.");
				return;
			}

			string[] sa = tag.Split('.');
			if (sa.Length < 2) return;
			QueryTable qt = Query.GetTableByName(sa[0]);
			if (qt == null) return;
			QueryColumn qc = qt.GetQueryColumnByName(sa[1]);
			if (qc == null) return;

			CurrentCriteriaControl = sender;
			CurrentQc = qc;

			EditCriteriaMenuItem_Click(sender, e);
		}

		private void RemoveCriteriaMenuItem_Click(object sender, EventArgs e)
		{
			QueryColumn qc = CurrentQc;

			qc.CriteriaDisplay = qc.Criteria = "";

			if (qc.IsKey)
				Query.KeyCriteriaDisplay = Query.KeyCriteria = "";

			Render();
		}

		private void ApplyCriteriaToSearchMenuItem_Click(object sender, EventArgs e)
		{
			CurrentQc.FilterSearch = !CurrentQc.FilterSearch;
			Render();
		}

		private void ApplyCriteriaToRetrievalMenuItem_Click(object sender, EventArgs e)
		{
			CurrentQc.FilterRetrieval = !CurrentQc.FilterRetrieval;
			Render();
		}

		private void ShowCriteriaInSumMenuItem_Click(object sender, EventArgs e)
		{
			CurrentQc.ShowOnCriteriaForm = !CurrentQc.ShowOnCriteriaForm;
		}

		private void RemoveAllCriteriaMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult dr = MessageBoxMx.Show(
				"Are you sure you want to clear all criteria in the current query?", UmlautMobius.String,
				MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			if (dr != DialogResult.Yes) return;

			foreach (QueryTable qt2 in Query.Tables)
			{
				foreach (QueryColumn qc2 in qt2.QueryColumns)
					qc2.CriteriaDisplay = qc2.Criteria = "";
			}

			Query.KeyCriteriaDisplay = Query.KeyCriteria = "";

			Render();
		}

		private void CriteriaTabRtClickContextMenu_Opening(object sender, CancelEventArgs e)
		{
			QueryColumn qc = CurrentQc;
			ApplyCriteriaToSearchMenuItem.Checked = qc.FilterSearch;
			ApplyCriteriaToRetrievalMenuItem.Checked = qc.FilterRetrieval;
			ShowCriteriaInSumMenuItem.Checked = qc.ShowOnCriteriaForm;
		}

		private void QueryDesc_EditValueChanged(object sender, EventArgs e)
		{
			if (!Rendering)
				Query.UserObject.Description = QueryDesc.Text;
		}

	}
}
