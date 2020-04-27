using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class PermissionsDialog : DevExpress.XtraEditors.XtraForm
	{
		static PermissionsDialog Instance;
		UserObject Uo;
		bool Editable = false;

		public PermissionsDialog()
		{
			InitializeComponent();
		}

		public static DialogResult Show(UserObject uo)
		{
			if (Instance == null) Instance = new PermissionsDialog();
			Instance.Setup(uo);
			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			return dr;
		}

		void Setup(UserObject uo)
		{
			Text = "Permissions for " + uo.Name;

			Uo = uo;

			if (UserObjectUtil.UserHasWriteAccess(uo))
			{
				Editable = true;
				PermissionsList.Editable = true;
				MakePublic.Enabled = MakePrivate.Enabled = AdvancedButton.Enabled = true;
			}

			else
			{
				Editable = false;
				PermissionsList.Editable = false;
				MakePublic.Enabled = MakePrivate.Enabled = AdvancedButton.Enabled = false;
			}

			AccessControlList acl = AccessControlList.Deserialize(uo);
			PermissionsList.Setup(acl);
			return;
		}
		
		private void MakePublic_Click(object sender, EventArgs e)
		{
			AccessControlList acl = new AccessControlList();
			acl.MakePublic(Uo.Owner);
			PermissionsList.Setup(acl);
		}

		private void MakePrivate_Click(object sender, EventArgs e)
		{
			AccessControlList acl = new AccessControlList();
			acl.MakePrivate(Uo.Owner);
			PermissionsList.Setup(acl);
		}

		private void AdvancedButton_Click(object sender, EventArgs e)
		{
			AdvancedMenu.Show(AdvancedButton,
				new System.Drawing.Point(0, AdvancedButton.Size.Height));
		}

/// <summary>
/// Save the changes
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void OkButton_Click(object sender, EventArgs e)
		{
			if (!Editable)
			{
				DialogResult = DialogResult.OK;
				return;
			}

			AccessControlList acl = PermissionsList.GetAcl();
			acl.AddReadWriteUserItem(SS.I.UserName); // be sure owner is included

			if (acl.IsPrivate)
			{
				Uo.AccessLevel = UserObjectAccess.Private;
				Uo.ACL = "";
			}
			else if (acl.IsPublic)
			{
				Uo.AccessLevel = UserObjectAccess.Public;
				Uo.ACL = "";
			}
			else
			{
				Uo.AccessLevel = UserObjectAccess.ACL;
				Uo.ACL = acl.Serialize();
			}

			if (Uo.Id > 0) // update header if object exists already
				UserObjectDao.UpdateHeader(Uo, false, true);

			DialogResult = DialogResult.OK;
			return;
		}

		private void CreateUserGroupMenuItem_Click(object sender, EventArgs e)
		{
			if (!Security.IsAdministrator(SS.I.UserName) && 
				!Security.HasPrivilege(SS.I.UserName, "CreateUserGroup") &&
				!ServicesIniFile.ReadBool("CreateUserGroupByAnyUser", true))
			{
				MessageBoxMx.ShowError("Your account is not authorized to create and/or edit user groups");
				return;
			}

			string groupName = InputBoxMx.Show("Enter the name of the new user group to be created.", "Create User Group");
			if (Lex.IsNullOrEmpty(groupName)) return;

			if (UserGroups.LookupExternalName(groupName) != null)
			{
				MessageBoxMx.ShowError("Group \"" + groupName + "\" already exists");
				return;
			}

			DialogResult dr = PermissionsGroupEditor.Show(groupName, true);
			if (dr == DialogResult.OK)
				PermissionsList.ItemNameComboBox.Properties.Items.Clear(); // rebuild for new group
		}

		private void ViewUserGroupMenuItem_Click(object sender, EventArgs e)
		{
			List<String> groups = UserGroups.GetAllGroups();

			string groupName = InputBoxMx.Show(
				"Select the User Group that you want to view", "View User Group", "", groups, -1, -1);

			if (Lex.IsNullOrEmpty(groupName)) return;

			DialogResult dr = PermissionsGroupEditor.Show(groupName, false);
			return;
		}

		private void EditUserGroupMenuItem_Click(object sender, EventArgs e)
		{

			List<String> groups = UserGroups.GetEditableGroups(SS.I.UserName);
			if (groups.Count == 0)
			{
				MessageBoxMx.ShowError("There are no groups that you are currently authorized to edit");
				return;
			}

			string groupName = InputBoxMx.Show(
				"Select the User Group that you want to edit", "Edit User Group", "", groups, -1, -1);

			if (Lex.IsNullOrEmpty(groupName)) return;

			DialogResult dr = PermissionsGroupEditor.Show(groupName, true);
			return;
		}

/// <summary>
/// Change owner
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void ChangeOwnerMenuItem_Click(object sender, EventArgs e)
		{
			{
				List<string> users = SecurityUtil.GetAllUsers();
				string prompt = "Enter the user name of the person to transfer ownership of " + Lex.AddDoubleQuotes(Uo.Name) + " to:";
				string newOwner = InputBoxMx.Show(prompt, "Change Owner", "", users, -1, -1);
				if (Lex.IsNullOrEmpty(newOwner)) return;

				string newUserName = SecurityUtil.GetInternalUserName(newOwner);
				if (Lex.IsNullOrEmpty(newUserName)) return;

				string result = UserObjectUtil.ChangeOwner(Uo.Id, newUserName);
				if (!Lex.IsNullOrEmpty(result))
				{
					MessageBoxMx.Show(result);
					DialogResult = DialogResult.Cancel; // close the dialog
				}
				return;
			}
		}

		private void PermissionsDialog_Activated(object sender, EventArgs e)
		{
			PermissionsList.ItemNameComboBox.Focus();
			AcceptButton = PermissionsList.AddItem; // enter key adds the item
		}

	}
}