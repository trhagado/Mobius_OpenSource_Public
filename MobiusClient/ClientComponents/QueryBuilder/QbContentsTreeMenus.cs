using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Web.UI.Design;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

	/// <summary>
	/// Collection of ContextMenus used by the ContentsTree.
	/// </summary>

	public partial class QbContentsTreeMenus : XtraForm
	{
		internal QbContentsTree QbContentsTree; // link to assoc tree control

		public QbContentsTreeMenus()
		{
			InitializeComponent();
		}

		////////////////////////////////////////////////////////////////////////////
		// Annotation
		////////////////////////////////////////////////////////////////////////////

		private void ContentsAddAnnotation_Click(object sender, System.EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAnnotationAdd");
			ExecuteContentsTreeCommand("ContentsDoubleClick");
		}

		private void ContentsOpenAnnotation_Click(object sender, System.EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAnnotationEdit");
			ExecuteContentsTreeCommand("ContentsOpenAnnotation");
		}

		private void ContentsPreviewAnnotation_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAnnotationPreview");
			ExecuteContentsTreeCommand("RunQuerySingleTablePreview");
		}

		private void ShowAnnotationTableDescriptionMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsTableShowDescription");
			ContentsCommandTargetOnly("ShowTableDescription");
		}

		private void ContentsAddToFavoritesAnnotation_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAnnotationAddToFavorites");
			ExecuteContentsTreeCommand("ContentsAddToFavorites");
		}

		private void ContentsMakePublicAnnotation_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAnnotationMakePublic");
			ExecuteContentsTreeCommand("ContentsMakePublic");
		}

		private void ContentsMakePrivateAnnotation_Click(object sender, System.EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAnnotationMakePrivate");
			ExecuteContentsTreeCommand("ContentsMakePrivate");
		}

		private void ChangeOwnerAnnotation_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAnnotationChangeOwner");
			ExecuteContentsTreeCommand("ContentsChangeOwner");
		}

		private void PermissionsAnnotation_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAnnotationPermissions");
			ExecuteContentsTreeCommand("ContentsPermissions");
		}

		private void ContentsCutAnnotation_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAnnotationCut");
			ExecuteContentsTreeCommand("ContentsCut");
		}

		private void ContentsCopyAnnotation_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAnnotationCopy");
			ExecuteContentsTreeCommand("ContentsCopy");
		}

		private void ContentsDeleteAnnotation_Click(object sender, System.EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAnnotationDelete");
			ExecuteContentsTreeCommand("ContentsDelete");
		}

		private void ContentsRenameAnnotation_Click(object sender, System.EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAnnotationRename");
			ExecuteContentsTreeCommand("ContentsRename");
		}

		////////////////////////////////////////////////////////////////////////////
		// Spotfire Link 
		////////////////////////////////////////////////////////////////////////////

		private void ContentsAddSpotfireLink_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsSpotfireLinkAdd");
			ExecuteContentsTreeCommand("ContentsDoubleClick");
		}

		private void ContentsOpenSpotfireLink_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsOpenSpotfireLink");
			ExecuteContentsTreeCommand("OpenSpotfireLink");
		}

		private void ContentsEditSpotfireLink_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsEditSpotfireLink");
			ExecuteContentsTreeCommand("EditSpotfireLink");
		}

		private void ContentsFavotitesSpotfireLink_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsSpotfireLinkAddToFavorites");
			ExecuteContentsTreeCommand("ContentsAddToFavorites");
		}

		private void ContentsPermissionsSpotfireLink_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsSpotfireLinkPermissions");
			ExecuteContentsTreeCommand("ContentsPermissions");
		}

		private void ContentsCutSpotfireLink_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsSpotfireLinkCut");
			ExecuteContentsTreeCommand("ContentsCut");
		}

		private void ContentsCopySpotfireLink_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsSpotfireLinkCopyCopy");
			ExecuteContentsTreeCommand("ContentsCopy");
		}

		private void ContentsDeleteSpotfireLink_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsSpotfireLinkDelete");
			ExecuteContentsTreeCommand("ContentsDelete");
		}

		private void ContentsRenameSpotfireLink_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsSpotfireLinkRename");
			ExecuteContentsTreeCommand("ContentsRename");
		}

		////////////////////////////////////////////////////////////////////////////
		// Project
		////////////////////////////////////////////////////////////////////////////

		private void menuSelectDefaultProject_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsProjectSelectDefaultProject");
			CommandExec.ExecuteCommandAsynch("SelectDefaultProject " + QbContentsTree.CurrentContentsMetaTreeNode.Target);
		}

		private void ContentsEditProjectDefinition_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsEditProject");
			string path = QbContentsTree.QbContentsTreeCtl.GetMetaTreeNodePath(QbContentsTree.CurrentContentsTreeListNode);
			if (Lex.IsUndefined(path)) path = QbContentsTree.CurrentContentsMetaTreeNode.Name;
			CommandExec.ExecuteCommandAsynch("ContentsEdit " + path);
		}

		private void ContentsPasteProject_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsProjectPaste");
			ExecuteContentsTreeCommand("ContentsPaste");
		}

		private void ContentsAddToFavoritesProject_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsProjectAddToFavorites");
			ExecuteContentsTreeCommand("ContentsAddToFavorites");
		}

		private void menuCreateUserFolder_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsProjectCreateUserFolder");
			ExecuteContentsTreeCommand("CreateUserFolder");
		}

		private void menuRenameProjectAllUsers_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsProjectRenameAllUsers");
			menuRenameTableAllUsers_Click(null, null);
		}

		////////////////////////////////////////////////////////////////////////////
		// Submenu
		////////////////////////////////////////////////////////////////////////////

		private void ContentsPasteSubmenu_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsPasteSubmenu");
			ExecuteContentsTreeCommand("ContentsPaste");
		}

		private void ContentsAddToFavoritesSubmenu_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsFolderAddToFavoritesSubmenu");
			ExecuteContentsTreeCommand("ContentsAddToFavorites");
		}

		private void ContentsCreateUserFolderSubmenu_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsCreateUserFolderSubmenu");
			ExecuteContentsTreeCommand("CreateUserFolder");
		}

		////////////////////////////////////////////////////////////////////////////
		// Calc Field
		////////////////////////////////////////////////////////////////////////////

		private void ContentsAddCalcField_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsCalcFieldAdd");
			ExecuteContentsTreeCommand("ContentsDoubleClick"); // default action
		}

		private void ShowCalcFieldTableDescriptionMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsTableShowDescription");
			ContentsCommandTargetOnly("ShowTableDescription");
		}

		private void ContentsOpenCalcField_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsCalcFieldEdit");
			ExecuteContentsTreeCommand("ContentsOpenCalcField");
		}

		private void ContentsAddToFavoritesCalcField_Click_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsCalcFieldAddToFavorites");
			ExecuteContentsTreeCommand("ContentsAddToFavorites");
		}

		private void ContentsPermissionsCalcField_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsCalcFieldPermissions");
			ExecuteContentsTreeCommand("ContentsPermissions");
		}

		private void ContentsCutCalcField_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsCalcFieldCut");
			ExecuteContentsTreeCommand("ContentsCut");
		}

		private void ContentsCopyCalcField_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsCalcFieldCopy");
			ExecuteContentsTreeCommand("ContentsCopy");
		}

		private void ContentsDeleteCalcField_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsCalcFieldRename");
			ExecuteContentsTreeCommand("ContentsDelete");
		}

		private void ContentsRenameCalcField_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsCalcFieldRename");
			ExecuteContentsTreeCommand("ContentsRename");
		}

		////////////////////////////////////////////////////////////////////////////
		// Cond Format
		////////////////////////////////////////////////////////////////////////////
		
		private void ContentsOpenCondFormat_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsCondFormatEdit");
			ExecuteContentsTreeCommand("ContentsCondFormatEdit");
		}

		private void ContentsAddToFavoritesCondFormat_Click_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsCondFormatAddToFavorites");
			ExecuteContentsTreeCommand("ContentsAddToFavorites");
		}

		private void ContentsPermissionsCondFormat_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsCondFormatPermissions");
			ExecuteContentsTreeCommand("ContentsPermissions");
		}

		private void ContentsCutCondFormat_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsCondFormatCut");
			ExecuteContentsTreeCommand("ContentsCut");
		}

		private void ContentsCopyCondFormat_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsCondFormatCopy");
			ExecuteContentsTreeCommand("ContentsCopy");
		}

		private void ContentsDeleteCondFormat_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsCondFormatRename");
			ExecuteContentsTreeCommand("ContentsDelete");
		}

		private void ContentsRenameCondFormat_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsCondFormatRename");
			ExecuteContentsTreeCommand("ContentsRename");
		}


		////////////////////////////////////////////////////////////////////////////
		// Url
		////////////////////////////////////////////////////////////////////////////

		private void ContentsOpenUrl_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsUrlOpen");
			ExecuteContentsTreeCommand("ContentsDoubleClick"); // default action
		}

		private void ContentsAddToFavoritesUrl_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsUrlAddToFavorites");
			ExecuteContentsTreeCommand("ContentsAddToFavorites");
		}

		////////////////////////////////////////////////////////////////////////////
		// Library
		////////////////////////////////////////////////////////////////////////////

		private void AddLibToCriteriaMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAddLibToCriteria");
			ExecuteContentsTreeCommand("ContentsAddLibToCriteria");
		}

		private void ViewLibAsListMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsViewLibAsList");
			ExecuteContentsTreeCommand("ContentsViewLibAsList");
		}

		////////////////////////////////////////////////////////////////////////////
		// Action
		////////////////////////////////////////////////////////////////////////////

		private void ContentsPerformAction_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsUrlOpen");
			ExecuteContentsTreeCommand("ContentsDoubleClick"); // default action
		}

		private void ContentsAddToFavoritesAction_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsActionAddToFavorites");
			ExecuteContentsTreeCommand("ContentsAddToFavorites");
		}

		////////////////////////////////////////////////////////////////////////////
		// User Db
		////////////////////////////////////////////////////////////////////////////

		private void ContentsOpenUserDatabase_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsUserDatabaseEdit");
			ExecuteContentsTreeCommand("ContentsOpenUserDatabase");
		}

		private void AddToFavoritesUcdb_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsUserDatabaseAddToFavorites");
			ExecuteContentsTreeCommand("ContentsAddToFavorites");
		}

		private void PermissionsUserDatabaseMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAnnotationPermissions");
			ExecuteContentsTreeCommand("ContentsPermissions");
		}

		private void ContentsDeleteUserDatabase_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsUserDatabaseDelete");
			ExecuteContentsTreeCommand("ContentsDelete");
		}

		private void ContentsRenameUserDatabase_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsUserDatabaseRename");
			ExecuteContentsTreeCommand("ContentsRename");
		}

		////////////////////////////////////////////////////////////////////////////
		// User Folder
		////////////////////////////////////////////////////////////////////////////

		private void addToFavoritesUserFolder_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsUserFolderAddToFavorites");
			ExecuteContentsTreeCommand("ContentsAddToFavorites");
		}

		private void ContentsCreateUserFolder_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsUserFolderCreate");
			ExecuteContentsTreeCommand("CreateUserFolder");
		}

		private void PermissionsUserFolder_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsUserFolderPermissions");
			ExecuteContentsTreeCommand("ContentsPermissions");
		}

		private void ContentsPasteUserFolder_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsUserFolderPaste");
			ExecuteContentsTreeCommand("ContentsPaste");
		}

		private void ContentsDeleteUserFolder_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsUserFolderDelete");
			ExecuteContentsTreeCommand("ContentsDelete");
		}

		private void ContentsRenameUserFolder_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsUserFolderRename");
			ExecuteContentsTreeCommand("ContentsRename");
		}

		////////////////////////////////////////////////////////////////////////////
		// Multitable
		////////////////////////////////////////////////////////////////////////////

		private void ContentsAddMultitable_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsMultitableAdd");
			ExecuteContentsTreeCommand("ContentsDoubleClick"); // default action
		}

		private void ContentsAddMultiTableAsMultipleTabs_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsMultitableAddAsMultipleTabs");
			ExecuteContentsTreeCommand("ContentsAddQueryToCurrentQuery");
		}

		private void AddToFavoritesMultitable_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsMultitableAddToFavorites");
			ExecuteContentsTreeCommand("ContentsAddToFavorites");
		}

		private void PermissionsMultiTableMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAnnotationPermissions");
			ExecuteContentsTreeCommand("ContentsPermissions");
		}

		private void ContentsCutMultitable_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsMultitableCut");
			ExecuteContentsTreeCommand("ContentsCut");
		}

		private void ContentsCopyMultitable_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsMultitableCopy");
			ExecuteContentsTreeCommand("ContentsCopy");
		}

		private void ContentsDeleteMultitable_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsMultitableDelete");
			ExecuteContentsTreeCommand("ContentsDelete");
		}

		private void ContentsRenameMultitable_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsMultitableRename");
			ExecuteContentsTreeCommand("ContentsRename");
		}

		////////////////////////////////////////////////////////////////////////////
		// Compound ID List
		////////////////////////////////////////////////////////////////////////////

		private void ContentsOpenList_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsListOpen");
			ExecuteContentsTreeCommand("ContentsDoubleClick");
		}

		private void ContentsCopyListToCurrent_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsListCopyToCurrent");
			ExecuteContentsTreeCommand("ContentsCopyListToCurrent");
		}

		private void ContentsAddList_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsListAddToQueryCriteria");
			ExecuteContentsTreeCommand("ContentsAddList");
		}

		private void ContentsRunQueryList_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsListRunQuery");
			ExecuteContentsTreeCommand("ContentsRunQueryList");
		}

		private void addToFavoritesCnList_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsListAddToFavorites");
			ExecuteContentsTreeCommand("ContentsAddToFavorites");
		}

		private void PermissionsCnListMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAnnotationPermissions");
			ExecuteContentsTreeCommand("ContentsPermissions");
		}

		private void ContentsCutList_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsListCut");
			ExecuteContentsTreeCommand("ContentsCut");
		}

		private void ContentsCopyList_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsListCopy");
			ExecuteContentsTreeCommand("ContentsCopy");
		}

		private void ContentsDeleteList_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsListDelete");
			ExecuteContentsTreeCommand("ContentsDelete");
		}

		private void ContentsRenameList_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsListRename");
			ExecuteContentsTreeCommand("ContentsRename");
		}

		////////////////////////////////////////////////////////////////////////////
		// Query
		////////////////////////////////////////////////////////////////////////////

		private void ContentsOpenQuery_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsQueryOpen");
			ExecuteContentsTreeCommand("ContentsDoubleClick"); // default action
		}

		private void AddToCurrentQueryNormal_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsQueryAddToCurrent");
			ExecuteContentsTreeCommand("ContentsAddQueryToCurrentQuery");
		}

		private void AddToCurrentQuerySingleTab_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsQueryAddToCurrentAsSingleTab");
			ExecuteContentsTreeCommand("ContentsAddQueryToCurrentQuerySingleTab");
		}

		private void ContentsOpenAndRunQuery_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsOpenAndRunQuery");
			ExecuteContentsTreeCommand("ContentsOpenAndRunQuery");
		}

		private void ContentsAddToFavoritesQuery_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsQueryAddToFavorites");
			ExecuteContentsTreeCommand("ContentsAddToFavorites");
		}

		private void PermissionsQueryMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAnnotationPermissions");
			ExecuteContentsTreeCommand("ContentsPermissions");
		}

		private void ContentsCutQuery_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsQueryCut");
			ExecuteContentsTreeCommand("ContentsCut");
		}

		private void ContentsCopyQuery_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsQueryCopy");
			ExecuteContentsTreeCommand("ContentsCopy");
		}

		private void ContentsDeleteQuery_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsQueryRename");
			ExecuteContentsTreeCommand("ContentsDelete");
		}

		private void ContentsRenameQuery_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsQueryRename");
			ExecuteContentsTreeCommand("ContentsRename");
		}

		////////////////////////////////////////////////////////////////////////////
		// Table
		////////////////////////////////////////////////////////////////////////////

		private void menuAddTable_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsTableAdd");
			ExecuteContentsTreeCommand("ContentsDoubleClick"); // default action is to add to query
		}

		private void menuAddSummarizedTable_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsTableAddSummarized");
			ExecuteContentsTreeCommand("AddSummarizedTable");
		}

		private void menuPreviewContentsItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsTablePreview");
			ContentsCommandTargetOnly("RunQuerySingleTablePreview");
		}

		private void ContentsShowTableDescription_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsTableShowDescription");
			ContentsCommandTargetOnly("ShowTableDescription");
		}

		private void addToFavoritesTable_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsTableAddToFavorites");
			ExecuteContentsTreeCommand("ContentsAddToFavorites");
		}

		private void menuRenameTableAllUsers_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsTableRenameAllUsers");
			MetaTreeNode node = QbContentsTree.CurrentContentsMetaTreeNode;
			if (node == null) return;
			if (String.IsNullOrEmpty(node.Target)) return;
			throw new NotImplementedException(); // "RenameContentsItemAllUsers " + node.Target);
		}

		////////////////////////////////////////////////////////////////////////////
		// Favorites context menu - Used when rt-clicking on folder item in Favorites menu
		////////////////////////////////////////////////////////////////////////////

		private void OpenFavoriteMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsFavoritesOpen");

			MetaTreeNode node = QbContentsTree.CurrentContentsMetaTreeNode;
			if (node == null) return;
			if (String.IsNullOrEmpty(node.Target)) return;
			CommandExec.ExecuteCommandAsynch("OpenFavorite " + node.Target);
		}

		private void DeleteFavoriteMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsFavoritesDelete");

			MetaTreeNode node = QbContentsTree.CurrentContentsMetaTreeNode;
			if (node == null) return;
			if (String.IsNullOrEmpty(node.Target)) return;

			DialogResult dr = MessageBoxMx.Show(
				"Are you sure you want to delete: '" + node.Label + "' ?",
				"Confirm Delete", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			if (dr != DialogResult.Yes) return;

			SessionManager.Instance.MainMenuControl.DeleteFavorite(node);
		}

		/// <summary>
		/// This method is called to send a ContentsTree node click command with object type & name
		/// </summary>
		/// <param name="command"></param>

		private void ExecuteContentsTreeCommand(
			string command)
		{
			QbContentsTree.ExecuteContentsTreeCommand(command);
		}

		/// <summary>
		/// Send a MetaTreeNode click command with just the object name
		/// </summary>
		/// <param name="command"></param>

		private void ContentsCommandTargetOnly(
			string command)
		{
			ExecuteContentsTreeCommand(command);
			//string target;

			//MetaTreeNode node = QbContentsTree.CurrentContentsTreeNode;
			//if (node == null) return;
			//if (node.Target == null || node.Target == "") return;

			//string[] sa = node.Target.Split(' ');
			//if (sa.Length == 1) target = sa[0];
			//else if (sa.Length == 2) target = sa[1];
			//else return;

			//CommandExec.Execute(command + " " + target);
		}

		/// <summary>
		/// Get the context menu associated with a metatree node
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>

		public ContextMenuStrip GetSingleNodeContextMenu(
			MetaTreeNode node)
		{
			bool singleNodeSelected = true;
			return GetNodeContextMenu(node, singleNodeSelected);
		}

		/// <summary>
		/// Get the context menu associated with a metatree node checking to see if multiple nodes are selected in the tree
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>

		public ContextMenuStrip GetNodeContextMenu(
			MetaTreeNode node)
		{
			bool singleNodeSelected = QbContentsTree.SingleNodeSelected();
			return GetNodeContextMenu(node, singleNodeSelected);
		}

		/// <summary>
		/// Get the context menu associated with a metatree node
		/// </summary>
		/// <param name="node"></param>
		/// <param name="singleNodeSelected"></param>
		/// <returns></returns>

		public ContextMenuStrip GetNodeContextMenu(
			MetaTreeNode node,
			bool singleNodeSelected)
		{
			ContextMenuStrip menu;
			string target = node.Target;

		  ShowInTreeMenuItem.Visible = singleNodeSelected;

			if (QbContentsTree == null) return null;

			MetaTable mt = null;
			bool descExists = false;

			switch (node.Type)
			{
				case MetaTreeNodeType.Project:
					SetPasteEnable(ContentsPasteProject);
					ContentsEditProjectDefinition.Enabled =
							SS.I.UserInfo.Privileges.HasPrivilege("ContentsTreeEditor") || SS.I.UserInfo.Privileges.HasPrivilege("ProjectEditor");
					return ContentsProjectContextMenu;

				case MetaTreeNodeType.SystemFolder:
					SetPasteEnable(ContentsPasteSubmenu);
					return ContentsSubmenuContextMenu;

				case MetaTreeNodeType.Target:
					return ContentsTargetContextMenu;

				case MetaTreeNodeType.UserFolder:
					if (Lex.StartsWith(node.Name, "MERGED_PUBLIC_")) return null; // can't do anything with merged public folders
					SetPasteEnable(ContentsPasteUserFolder);
					return ContentsUserFolderContextMenu;

				case MetaTreeNodeType.Database:
					if (Lex.StartsWith(node.Name, "MERGED_PUBLIC_")) return null; // can't do anything with merged public folders

					if (Lex.StartsWith(target, "USERDATABASE_")) // show user database ContextMenu
						return ContentsUserDatabaseContextMenu;
					return null;

				case MetaTreeNodeType.MetaTable:
					AddSummarizedTableToQueryMenuItem.Enabled = false;
					ShowTableDescriptionMenuItem.Enabled = false;

					// disable preview if more than one table selected
					PreviewTableMenuItem.Enabled = singleNodeSelected;

					mt = MetaTableCollection.Get(node.Target);
					if (mt != null)
					{ 
						AddSummarizedTableToQueryMenuItem.Enabled = mt.SummarizedExists;
						descExists = !String.IsNullOrEmpty(mt.Description);
					}
					ShowTableDescriptionMenuItem.Enabled = descExists;

					return ContentsTableContextMenu;

				case MetaTreeNodeType.Url:
					return ContentsUrlContextMenu;

				case MetaTreeNodeType.Action:
					return ContentsActionContextMenu;

				case MetaTreeNodeType.Query:
					if (Lex.StartsWith(target, "QUERY_")) // normal query
					{
						menu = ContentsQueryContextMenu;
						QbContentsTree.SetItemEnabledState(node, ContentsCutQuery, ContentsCopyQuery, ContentsDeleteQuery, ContentsRenameQuery);
						ContentsOpenQuery.Enabled = singleNodeSelected;
						ContentsAddToCurrentQuery.Enabled = singleNodeSelected;
						ContentsOpenAndRunQuery.Enabled = singleNodeSelected;
						ContentsAddToFavoritesQuery.Enabled = true; // singleNodeSelected;
						PermissionsQueryMenuItem.Enabled = singleNodeSelected;
						ContentsRenameQuery.Enabled = singleNodeSelected;
						return menu;
					}

					else if (Lex.StartsWith(target, "MULTITABLE_")) // multitable form of query (referenced in root.xml as "MULTITABLE_123")
					{
						menu = ContentsMultitableContextMenu;
						QbContentsTree.SetItemEnabledState(node, ContentsCutMultitable, ContentsCopyMultitable, ContentsDeleteMultitable, ContentsRenameMultitable);
						ContentsAddMultitable.Enabled = singleNodeSelected;
						ContentsAddMultiTableAsMultipleTabs.Enabled = singleNodeSelected;
						AddToFavoritesMultitableMenuItem.Enabled = true; // singleNodeSelected;
						PermissionsMultiTableMenuItem.Enabled = singleNodeSelected;
						ContentsRenameMultitable.Enabled = singleNodeSelected;
						return menu;
					}

					return null;

				case MetaTreeNodeType.CnList:
					menu = ContentsCnListContextMenu;
					QbContentsTree.SetItemEnabledState(node, ContentsCutList, ContentsCopyList, ContentsDeleteList, ContentsRenameList);
					ContentsOpenList.Enabled = singleNodeSelected;
					ContentsAddList.Enabled = singleNodeSelected;
					ContentsCopyListToCurrent.Enabled = singleNodeSelected;
					ContentsRunQueryList.Enabled = singleNodeSelected;
					AddToFavoritesCnListMenuItem.Enabled = true; // singleNodeSelected;
					PermissionsCnListMenuItem.Enabled = singleNodeSelected;
					ContentsRenameList.Enabled = singleNodeSelected;
					return menu;

				case MetaTreeNodeType.CalcField:
					menu = ContentsCalcFieldContextMenu;
					QbContentsTree.SetItemEnabledState(node, ContentsCutCalcField, ContentsCopyCalcField, ContentsDeleteCalcField, ContentsRenameCalcField);
					ContentsOpenCalcField.Enabled = singleNodeSelected;
					ContentsAddCalcField.Enabled = singleNodeSelected;
					ContentsPermissionsCalcField.Enabled = singleNodeSelected;
					ContentsRenameCalcField.Enabled = singleNodeSelected;

					mt = MetaTableCollection.Get(node.Target);
					if (mt != null)
						descExists = !String.IsNullOrEmpty(mt.Description);
					ShowCalcFieldTableDescriptionMenuItem.Enabled = descExists;

					return menu;

				case MetaTreeNodeType.CondFormat:
					menu = ContentsCondFormatContextMenu;
					QbContentsTree.SetItemEnabledState(node, ContentsCutCondFormat, ContentsCopyCondFormat, ContentsDeleteCondFormat, ContentsRenameCondFormat);
					ContentsOpenCalcField.Enabled = singleNodeSelected;
					ContentsAddCalcField.Enabled = singleNodeSelected;
					ContentsPermissionsCalcField.Enabled = singleNodeSelected;
					return menu;

				case MetaTreeNodeType.Annotation:
					ContentsPreviewAnnotation.Visible =
							ContentsOpenAnnotation.Visible =
									PermissionsAnnotationMenuItem.Visible = true;
					menu = ContentsAnnotationContextMenu;
					QbContentsTree.SetItemEnabledState(node, ContentsCutAnnotation, ContentsCopyAnnotation, ContentsDeleteAnnotation, ContentsRenameAnnotation);
					ContentsPreviewAnnotation.Enabled = singleNodeSelected;
					ContentsOpenAnnotation.Enabled = singleNodeSelected;
					PermissionsAnnotationMenuItem.Enabled = singleNodeSelected;
					ContentsAddAnnotation.Enabled = singleNodeSelected;

                    mt = MetaTableCollection.Get(node.Target);
					if (mt != null) 
						descExists = !String.IsNullOrEmpty(mt.Description);
					ShowAnnotationTableDescriptionMenuItem.Enabled = descExists;

					// If Rename is Enabled, disable if there is a multiselection
					if (ContentsRenameAnnotation.Enabled) ContentsRenameAnnotation.Enabled = singleNodeSelected;

					// because this menu could be used to add annotations and tables to the query, we hide items specific to annotations
					if (!singleNodeSelected)
					{
						ContentsPreviewAnnotation.Visible =
								ContentsOpenAnnotation.Visible =
										PermissionsAnnotationMenuItem.Visible = false;
						// if any of the objects in the list do not have read access then the menus are disabled 
						QbContentsTree.SetItemEnabledState(ContentsCutAnnotation, ContentsCopyAnnotation, ContentsDeleteAnnotation, ContentsRenameAnnotation, ContentsAddAnnotation);
					}
					return menu;

				case MetaTreeNodeType.Library:
					menu = ContentsLibraryContextMenu;
					return menu;

				case MetaTreeNodeType.ResultsView:
					menu = ContentsSpotfireLinkContextMenu;
					return menu;

				default:
					return null;
			}
		}

		/// <summary>
		/// Return true if node name is an AFS project node name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static bool IsAfsNodeName(string name)
		{
			int id = GetAfsProjectId(name);
			return id > 0 ? true : false;
		}

		/// <summary>
		/// Get the AFS project id, if any, associated with a project node name
		/// </summary>
		/// <param name="projNodeName"></param>
		/// <returns></returns>

		public static int GetAfsProjectId(string projNodeName)
		{
			int id;

			string result = CommandDao.Execute("ProjectTreeFactory.GetAfsProjectId " + projNodeName);
			if (int.TryParse(result, out id)) return id;
			else return -1;
		}

		/// <summary>
		/// Set Enable for a Paste ToolStripMenuItem based on whether there is a UserObject in the clipboard
		/// </summary>
		/// <param name="mi"></param>

		void SetPasteEnable(ToolStripMenuItem mi)
		{
			bool enable = false;
			try
			{
				string txt = Clipboard.GetText();
				//if (UserObject.Deserialize(txt) != null) enable = true;
				UserObject[] userobjects = UserObjectUtil.Deserialize(txt);
				if (userobjects != null) enable = true;
			}
			catch (Exception ex) { }

			mi.Enabled = enable;
			return;
		}

		private void ShowTargetDescriptionMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsTargetShowDescription");
			ContentsCommandTargetOnly("ShowTargetDescription");
		}

		private void ShowTargetAssayListMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsTargetShowAssayList");
			ContentsCommandTargetOnly("ShowTargetAssayList");
		}

		private void ShowTargetAssayDataMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsShowAllTargetUnsummarizedAssayData");
			ContentsCommandTargetOnly("ShowAllTargetUnsummarizedAssayData");
		}

		private void AddToFavoritesTargetMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsTargetAddToFavorites");
			ContentsCommandTargetOnly("ContentsAddToFavorites");
		}

		private void ShowInTreeMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsShowResultInTree");
			ContentsCommandTargetOnly("ShowResultInTree");
		}
	}
}