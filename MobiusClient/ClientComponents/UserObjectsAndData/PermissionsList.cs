using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

/// <summary>
/// Edit a list of permissions or users
/// </summary>

	public partial class PermissionsList : DevExpress.XtraEditors.XtraUserControl
	{
		public DataTable DataTable = null;
		public bool UsersOnly = false; // if true then just edit as list of users
		public bool Editable = true; // if true can edit the list

		public PermissionsList()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);
		}

		/// <summary>
		/// Setup with specified list
		/// </summary>
		/// <param name="acl"></param>

		public void Setup(AccessControlList acl)
		{
			if (UsersOnly)
			{
				ExternalNameCol.Caption = "User";
				WriteCol.Caption = "Can Update Group Members";
				GridView.ColumnPanelRowHeight = 48; // for larger caption
				ReadCol.Visible = false;
				AddPrompt.Text = "User:";
			}

			if (Editable)
			{
				GridView.OptionsBehavior.Editable = true;
				AddPrompt.Enabled = ItemNameComboBox.Enabled = AddItem.Enabled = RemoveItem.Enabled = true;
			}

			else
			{
				GridView.OptionsBehavior.Editable = false;
				AddPrompt.Enabled = ItemNameComboBox.Enabled = AddItem.Enabled = RemoveItem.Enabled = false;
			}

			DataTable dt = CreateDataTable(); // build the field grid

			acl.Sort(); // sort before display
			FillDataTable(acl);

			return;
		}

		/// <summary>
		/// CreateDataTable
		/// </summary>
		/// <returns></returns>

		DataTable CreateDataTable()
		{
			DataTable dt = new DataTable();
			DataTable = dt; // save ref to table

			dt.Columns.Add("ExternalNameCol", typeof(string));
			dt.Columns.Add("ReadCol", typeof(bool));
			dt.Columns.Add("WriteCol", typeof(bool));
			dt.Columns.Add("IsGroupCol", typeof(bool));
			dt.Columns.Add("ItemTypeImageCol", typeof(Image));
			dt.Columns.Add("InternalNameCol", typeof(string));

			return dt;
		}

		/// <summary>
		/// Fill the datatable with current permissions
		/// </summary>

		void FillDataTable(AccessControlList acl)
		{
			DataTable dt = DataTable;
			dt.Clear();
			foreach (AclItem p0 in acl.Items)
			{
				DataRow dr = dt.NewRow();
				SetDataRow(dr, p0);
				dt.Rows.Add(dr);
			}

			Grid.DataSource = dt;
			Grid.Refresh();
		}

		/// <summary>
		/// SetDataRow
		/// </summary>
		/// <param name="dr"></param>
		/// <param name="f"></param>

		void SetDataRow(DataRow dr, AclItem p)
		{
			dr["ExternalNameCol"] = p.GetExternalName();
			dr["ReadCol"] = p.ReadIsAllowed;
			dr["WriteCol"] = p.WriteIsAllowed;
			dr["IsGroupCol"] = p.IsGroup;
			dr["ItemTypeImageCol"] = Bitmaps16x16.Images[p.IsGroup ? 1 : 0];
			dr["InternalNameCol"] = p.AssignedTo;

			return;
		}

		/// <summary>
		/// Setup the dropdown if not done yet
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void GroupUserComboBox_Enter(object sender, EventArgs e)
		{
			List<string> names;

			if (ItemNameComboBox.Properties.Items.Count > 0) return;

			if (!UsersOnly)
			{
				ItemNameComboBox.Properties.Items.Add("------------- GROUPS -------------");

				names = new List<string>();
				foreach (UserGroup ug in UserGroups.Items)
					names.Add(ug.ExternalName);

				names.Sort();
				ItemNameComboBox.Properties.Items.AddRange(names);

				ItemNameComboBox.Properties.Items.Add("");
			}

			ItemNameComboBox.Properties.Items.Add("-------------- USERS --------------");

			names = SecurityUtil.GetAllUsers();
			ItemNameComboBox.Properties.Items.AddRange(names);

			return;
		}

		private void AddGroupUser_Click(object sender, EventArgs e)
		{
			string txt = ItemNameComboBox.Text;
			if (Lex.IsNullOrEmpty(txt) || txt.Contains("------")) return;

			if (ItemNameComboBox.SelectedIndex < 0) return;

			foreach (DataRow dr0 in DataTable.Rows) // be sure not already in list
				if (dr0["ExternalNameCol"].ToString() == txt) return;

			DataRow dr = DataTable.NewRow();
			AclItem i = new AclItem();
			i.AssignedTo = ExternalToInternalName(txt);
			i.IsGroup = (LookupGroupItem(txt) != null);
			i.Permissions = PermissionEnum.Read;
			SetDataRow(dr, i);
			DataTable.Rows.Add(dr);
		}

		/// <summary>
		/// Convert an external user or group name to an internal name
		/// </summary>
		/// <param name="extName"></param>
		/// <returns></returns>
		string ExternalToInternalName(string extName)
		{
			if (Lex.IsNullOrEmpty(extName)) return "";
			UserGroup g = LookupGroupItem(extName); // see if group name first
			if (g != null) return g.InternalName;

			DictionaryMx userDict = DictionaryMx.Get("UserName");
			if (userDict == null) return extName;
			foreach (string userName in userDict.Words)
			{
				string userInfoString = userDict.LookupDefinition(userName);
				UserInfo userInfo = UserInfo.Deserialize(userInfoString);
				if (userInfo == null || Lex.IsNullOrEmpty(userInfo.FullName)) continue;
				if (Lex.Eq(userInfo.FullNameReversed, extName)) return userInfo.UserName;
			}

			return extName; // shouldn't happen
		}

		/// <summary>
		/// Lookup a group item by its external name
		/// </summary>
		/// <param name="extName"></param>
		/// <returns></returns>

		UserGroup LookupGroupItem(string extName)
		{
			foreach (UserGroup g in UserGroups.Items)
			{
				if (Lex.Eq(extName, g.ExternalName))
					return g;
			}

			return null;
		}

/// <summary>
/// Get the ACL from the grid
/// </summary>
/// <param name="acl"></param>
/// <param name="seenOwner"></param>

		public AccessControlList GetAcl()
		{
			AccessControlList acl = new AccessControlList();

			foreach (DataRow dr0 in DataTable.Rows) // convert DataRows to an ACL
			{
				AclItem item = new AclItem();
				item.AssignedTo = (string)dr0["InternalNameCol"];
				if (item.AssignedTo == SS.I.UserName)
				{
					item.Permissions = PermissionEnum.Read | PermissionEnum.Write;
				}
				item.IsGroup = (bool)dr0["IsGroupCol"];
				if ((bool)dr0["ReadCol"]) item.Permissions |= PermissionEnum.Read;
				if ((bool)dr0["WriteCol"]) item.Permissions |= PermissionEnum.Write;
				acl.Items.Add(item);
			}

			return acl;
		}

		/// <summary>
		/// Remove item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Remove_Click(object sender, EventArgs e)
		{
			int[] rows = GridView.GetSelectedRows();
			if (rows.Length == 0) return;

			for (int ri = rows.Length - 1; ri >= 0; ri--)
			{
				GridView.DeleteRow(rows[ri]);
			}

			return;
		}

	}
}
