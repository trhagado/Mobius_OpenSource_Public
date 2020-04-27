using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class CondFormatEditor : XtraForm
	{
		static CondFormatEditor Instance = null;

		MetaColumnType ColumnType; // type for current column
		//bool ShowInHeaders;
		static string InitialSerializedForm = "";
		bool ShowEditorWhenActivated = false;
		bool EditForUserObjectStorage = false;

		static MainMenuControl MainMenuControl { get { return SessionManager.Instance.MainMenuControl; } }

		public CondFormatEditor()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Create a new conditional format user object
		/// </summary>
		/// <returns></returns>

		public static CondFormat CreateNewUserObject()
		{
			MetaColumnType mcType = MetaColumnType.Number;
			CondFormat cf = CreateAndInitializeCf(mcType);

			string title = "New Conditional Format";
			CondFormat cf2 = Edit(cf, true, title);
			if (cf2 == null) return null;

			UserObject uo = new UserObject(UserObjectType.CondFormat);

			//  Predefined cond formats by default are owned by a single owner and stored in a single folder

			uo.Owner = SS.I.UserName;
			uo.ParentFolder = CondFormat.PredefinedCondFormatFolderName;
			uo.AccessLevel = UserObjectAccess.ACL;
			uo.ACL = AccessControlList.GetAdministratorGroupRwPublicReadAcl(SS.I.UserName);

			uo = UserObjectSaveDialog.Show("Save As", uo);
			if (uo == null) return null;

			uo.Content = cf2.Serialize();
			UserObjectDao.Write(uo, uo.Id);

			MessageBoxMx.Show("Conditional format " + uo.Name + " saved (" + uo.InternalName + ")"); // show internal name so it may be referenced in a MetaColumn definition

			if (MainMenuControl != null) MainMenuControl.UpdateMruList(uo.InternalName);

			return cf;
		}

		/// <summary>
		/// Edit an existing conditional format user object
		/// </summary>
		/// <param name="internalName"></param>
		/// <returns></returns>

		public static CondFormat EditUserObject(
			string internalName)
		{
			UserObject uo = UserObject.ParseInternalUserObjectName(internalName, "");
			if (uo != null) uo = UserObjectDao.Read(uo.Id);
			if (uo == null) throw new Exception("User object not found " + internalName);
			CondFormat cf = EditUserObject(uo);
			return cf;
		}

		/// <summary>
		/// Edit a new or existing calculated field
		/// </summary>
		/// <param name="uo">Existing UserObject with content or null to create a new CF</param>
		/// <returns></returns>

		public static CondFormat EditUserObject(
			UserObject uo = null)
		{
			if (uo == null) // prompt if not supplied
			{
				UserObject defaultUo = new UserObject(UserObjectType.CondFormat);
				defaultUo.ParentFolder = CondFormat.PredefinedCondFormatFolderName;
				uo = UserObjectOpenDialog.ShowDialog(UserObjectType.CondFormat, "Edit Conditional Formatting", defaultUo);
			}
			if (uo == null) return null;

			uo = UserObjectDao.Read(uo);
			if (uo == null) return null;

			CondFormat cf = CondFormat.Deserialize(uo.Content);
			if (cf == null) return null;

			string title = "Conditional Formatting - " + uo.Name + " (" + uo.InternalName + ")"; 
			CondFormat cf2 = Edit(cf, true, title);
			if (cf2 == null) return null;

			uo.Content = cf2.Serialize();
			UserObjectDao.Write(uo, uo.Id);

			if (MainMenuControl != null) MainMenuControl.UpdateMruList(uo.InternalName);

			CondFormat.UpdatePredefined(uo.InternalName, cf2); // update the dictionary of predefined CFs

			UpdateExistingCfReferences(uo, cf2); // update any references to this cf in the list of open queries

			return cf;
		}

		/// <summary>
		/// Update any references to this CF in open queries
		/// </summary>
		/// <param name="uo"></param>
		/// <param name="cf"></param>
		/// <returns></returns>

		public static int UpdateExistingCfReferences(
			UserObject uo,
			CondFormat cf)
		{
			int updCnt = 0;
			string internalName = uo.InternalName;

			if (QueriesControl.Instance == null) return updCnt;
			List<Document> docList = QueriesControl.Instance.DocumentList;

			for (int di = 0; di < docList.Count; di++)
			{ // update any QueryColumn to MetaColumn references
				Document d = docList[di];
				if (d.Type != DocumentType.Query) continue;
				Query q = (Query)d.Content;
				foreach (QueryTable qt in q.Tables)
				{
					MetaTable mt = qt.MetaTable;
					foreach (MetaColumn mc in mt.MetaColumns)
					{
						if (!Lex.Eq(mc.CondFormatName, internalName)) continue;
						mc.CondFormat = cf;
						updCnt++;
					}
				}
			}

			return updCnt;
		}

		/// <summary>
		/// Edit conditional formatting
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		public static DialogResult Edit(
				QueryColumn qc)
		{
			MetaColumnType mcType = qc.MetaColumn.DataType;
			if (!CondFormat.CondFormattingAllowed(mcType))
			{
				ErrorMessageBox.Show(
					"Conditional formatting isn't supported for the " + qc.ActiveLabel + " field.");
				return DialogResult.Cancel;
			}

			CondFormat cf = qc.ActiveCondFormat; // get current formatting
			if (cf == null) cf = CreateAndInitializeCf(mcType);
			QueryTable qt = qc.QueryTable;
			Query query = qt.Query;

			CondFormat cf2 = Edit(cf);
			if (cf2 == null) return DialogResult.Cancel;

			if (cf2.Rules.Count == 0) // all rules deleted
			{
				qc.CondFormat = null;
				return DialogResult.OK;
			}

			qc.CondFormat = cf2;

			if (query != null && !String.IsNullOrEmpty(cf2.Name)) // update any other formatting that shares this name to use this object
			{
				foreach (QueryTable qt2 in query.Tables)
				{
					foreach (QueryColumn qc2 in qt2.QueryColumns)
					{
						if (qc2.CondFormat != null && Lex.Eq(qc2.CondFormat.Name, cf2.Name))
							qc2.CondFormat = cf2;
					}
				}
			}

			return DialogResult.OK;
		}

		/// <summary>
		///  Edit conditional formatting
		/// </summary>
		/// <param name="cf"></param>
		/// <param name="title"></param>
		/// <returns></returns>

		public static CondFormat Edit(
			CondFormat cf,
			bool editForUserObjectStorage = false,
			string title = "Conditional Formatting Rules")
		{
			InitialSerializedForm = cf.Serialize(); // save for later compare
			//if (Instance == null) // always create new instance because the any changed height of the grid editor controls is maintained otherwise
				Instance = new CondFormatEditor();

			Instance.Text = title;
			Instance.ShowEditorWhenActivated = true;
			Instance.EditForUserObjectStorage = editForUserObjectStorage;
			Instance.CondFormatToForm(cf);
			
			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return null;

			cf = Instance.FormToCondFormat();

			cf.Rules.InitializeInternalMatchValues(cf.ColumnType);

			return cf;
		}

		public static CondFormat CreateAndInitializeCf(
			MetaColumnType mcType)
		{
			CondFormat cf = null;

			if (MetaColumn.IsNumericMetaColumnType(mcType) ||
				mcType == MetaColumnType.String ||
				mcType == MetaColumnType.Date)
				cf = CondFormat.BuildDefaultConditionalFormatting();

			else
			{
				cf = new CondFormat();
				cf.Rules.Add(new CondFormatRule()); // initial rule
			}

			cf.ColumnType = mcType; // be sure type is set
			return cf;
		}

		private void OKButton_Click(object sender, EventArgs e)
		{
			if (!Rules.AreValid()) return;
			this.Visible = false; // hide so not called again by FormClosing event
			DialogResult = DialogResult.OK;
		}

		private void CancelBut_Click(object sender, EventArgs e)
		{
			CondFormat cf = FormToCondFormat();
			string s = cf.Serialize();

			if (s != InitialSerializedForm)
			{
				DialogResult dr = XtraMessageBox.Show(
					"Are you sure you want to discard the\r\n" +
					"changes to this conditional formatting?",
					UmlautMobius.String, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
				if (dr != DialogResult.Yes) return;
			}
			this.Visible = false; // hide so not called again by FormClosing event
			DialogResult = DialogResult.Cancel;
		}


		/// <summary>
		/// Copy CondFormat object onto the form
		/// </summary>
		/// <param name="cf"></param>

		public void CondFormatToForm(
			CondFormat cf)
		{
			ColumnType = cf.ColumnType;
			//ShowInHeaders = cf.ShowInHeaders;

			if (ColumnType == MetaColumnType.Structure)
			{
				Option1.Text = "Highlight matching substructure";
				Option1.Visible = true;

				Option2.Text = "Align to matching substructure query";
				Option2.Visible = true;

				Rules.Height = Option1.Top - Rules.Top - 8;
			}

			else
			{
				Option1.Visible = false;
				Option2.Visible = false;
				Rules.Height = CfName.Top - Rules.Top - 8;
			}

			Option1.Checked = cf.Option1;
			Option2.Checked = cf.Option2;

			CfName.Text = cf.Name;
			ColumnDataType.Text = cf.ColumnType.ToString();

			bool showFormatName = (!EditForUserObjectStorage); // show either the Cf name or the Cf data type (for saved Cfs)

			CfNameLabel.Visible = CfName.Visible = CfNameLabel2.Visible = showFormatName;

			if (ColumnTypeLabel.Top != CfName.Top)
			{
				int dy = CfName.Top - ColumnTypeLabel.Top;
				ColumnDataType.Top += dy;
				ColumnTypeLabel.Top += dy;
			}
			ColumnTypeLabel.Visible = ColumnDataType.Visible = !showFormatName;

			Rules.SetupControl(cf.ColumnType, cf.Rules);

			return;
		}

		/// <summary>
		/// Serialize from form into cf object & then into string
		/// </summary>
		/// <returns></returns>

		public CondFormat FormToCondFormat()
		{
			CondFormat cf = new CondFormat();
			cf.ColumnType = ColumnType;
			//cf.ShowInHeaders = ShowInHeaders;
			cf.Name = CfName.Text;
			cf.ColumnType = MetaColumn.ParseMetaColumnTypeString(ColumnDataType.Text);
			cf.Option1 = Option1.Checked;
			cf.Option2 = Option2.Checked;
			cf.Rules = Rules.GetRules();
			return cf;
		}

		private void CondFormatForm_Activated(object sender, EventArgs e)
		{
			if (ShowEditorWhenActivated)
			{
				Rules.Focus();
				if (ColumnType != MetaColumnType.Structure) // put on first value unless structure which would invoke structure editor
				{
					Rules.RulesGrid.FocusCell(0, 2); // put on first value
					Rules.StartEditing();
				}
				ShowEditorWhenActivated = false;
			}
		}

		private void CondFormatForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!this.Visible) return;
			CancelBut_Click(sender, e); // check for changes
			e.Cancel = true; // cancel this close requiring an explicit this.Visible = false 
		}

		private void JoinAnchorComboBox_Properties_Enter(object sender, EventArgs e)
		{

		}
	}
}
