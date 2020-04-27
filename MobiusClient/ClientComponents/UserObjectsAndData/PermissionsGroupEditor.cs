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

/// <summary>
/// Edit the list of members in a user group
/// </summary>

	public partial class PermissionsGroupEditor : DevExpress.XtraEditors.XtraForm
	{
		static PermissionsGroupEditor Instance;
		UserObject Uo; // user object currently being edited
		bool Editable = false;

		public PermissionsGroupEditor()
		{
			InitializeComponent();

			PermissionsList.UsersOnly = true;
		}

/// <summary>
/// Edit the specified group name
/// </summary>
/// <param name="groupName"></param>
/// <returns></returns>

		public static DialogResult Show(
			string groupName,
			bool editable)
		{
			if (Instance == null) Instance = new PermissionsGroupEditor();
			Instance.Setup(groupName, editable);
			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			return dr;
		}

		void Setup(
			string groupName,
			bool editable)
		{
			AccessControlList acl;
			AclItem aclItem;

			Text = "Users in the Group: " + groupName;

			Editable = editable;
			PermissionsList.Editable = editable;

			Uo = UserObjectDao.ReadHeader(UserObjectType.UserGroup, "Mobius", "", groupName);
			if (Uo == null)
			{
				Uo = new UserObject();
				Uo.Type = UserObjectType.UserGroup;
				Uo.Name = groupName;
				Uo.Owner = "Mobius";
				Uo.ParentFolder = ""; // no parent folder
				acl = new AccessControlList();
				acl.MakePublic(SS.I.UserName); // write to creator, read to others
			}

			acl = AccessControlList.Deserialize(Uo.ACL); // content is the list of users
			PermissionsList.Setup(acl);
			return;
		}

/// <summary>
/// Update the list
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
			acl.AddReadWriteUserItem(SS.I.UserName); // be sure current user is included

			Uo.ACL = acl.Serialize(); // just an ACL, no content
			Uo.AccessLevel = UserObjectAccess.ACL;
			UserObjectDao.Write(Uo, Uo.Id);

			UserGroups.UpdateInMemoryCollection(Uo.Name, acl);
			DialogResult = DialogResult.OK;
		}

		private void UserGroupEditor_Activated(object sender, EventArgs e)
		{
			PermissionsList.ItemNameComboBox.Focus();
			AcceptButton = PermissionsList.AddItem; // enter key adds the item
		}
	}
}
