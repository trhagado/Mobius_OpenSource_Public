namespace Mobius.ClientComponents
{
	partial class ContentsTreeControlUoMenus
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ContentsTreeControlUoMenus));
			this.TreePopupMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.CreateUserFolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.PermissionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.DeleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RenameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuDivider = new System.Windows.Forms.ToolStripSeparator();
			this.ViewSourceMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.TreePopupMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// TreePopupMenu
			// 
			this.TreePopupMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CreateUserFolderMenuItem,
            this.PermissionsMenuItem,
            this.DeleteMenuItem,
            this.RenameMenuItem,
            this.menuDivider,
            this.ViewSourceMenuItem});
			this.TreePopupMenu.Name = "TreePopupMenu";
			this.TreePopupMenu.Size = new System.Drawing.Size(186, 142);
			// 
			// CreateUserFolderMenuItem
			// 
			this.CreateUserFolderMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.FolderNew;
			this.CreateUserFolderMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.CreateUserFolderMenuItem.Name = "CreateUserFolderMenuItem";
			this.CreateUserFolderMenuItem.Size = new System.Drawing.Size(185, 22);
			this.CreateUserFolderMenuItem.Text = "Create Subfolder...";
			this.CreateUserFolderMenuItem.Click += new System.EventHandler(this.CreateUserFolderMenuItem_Click);
			// 
			// PermissionsMenuItem
			// 
			this.PermissionsMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("PermissionsMenuItem.Image")));
			this.PermissionsMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.PermissionsMenuItem.Name = "PermissionsMenuItem";
			this.PermissionsMenuItem.Size = new System.Drawing.Size(185, 22);
			this.PermissionsMenuItem.Text = "Permissions...";
			this.PermissionsMenuItem.Click += new System.EventHandler(this.PermissionsMenuItem_Click);
			// 
			// DeleteMenuItem
			// 
			this.DeleteMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Delete;
			this.DeleteMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.DeleteMenuItem.Name = "DeleteMenuItem";
			this.DeleteMenuItem.Size = new System.Drawing.Size(185, 22);
			this.DeleteMenuItem.Text = "Delete";
			this.DeleteMenuItem.Click += new System.EventHandler(this.DeleteMenuItem_Click);
			// 
			// RenameMenuItem
			// 
			this.RenameMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.RenameMenuItem.Name = "RenameMenuItem";
			this.RenameMenuItem.Size = new System.Drawing.Size(185, 22);
			this.RenameMenuItem.Text = "Rename";
			this.RenameMenuItem.Click += new System.EventHandler(this.RenameMenuItem_Click);
			// 
			// menuDivider
			// 
			this.menuDivider.Name = "menuDivider";
			this.menuDivider.Size = new System.Drawing.Size(182, 6);
			// 
			// ViewSourceMenuItem
			// 
			this.ViewSourceMenuItem.Name = "ViewSourceMenuItem";
			this.ViewSourceMenuItem.Size = new System.Drawing.Size(185, 22);
			this.ViewSourceMenuItem.Text = "View Internal Format";
			this.ViewSourceMenuItem.Click += new System.EventHandler(this.ViewSourceMenuItem_Click);
			// 
			// ContentsTreeControlUoMenus
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Name = "ContentsTreeControlUoMenus";
			this.TreePopupMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ContextMenuStrip TreePopupMenu;
		private System.Windows.Forms.ToolStripMenuItem CreateUserFolderMenuItem;
		private System.Windows.Forms.ToolStripMenuItem PermissionsMenuItem;
		private System.Windows.Forms.ToolStripMenuItem RenameMenuItem;
		private System.Windows.Forms.ToolStripMenuItem DeleteMenuItem;
		private System.Windows.Forms.ToolStripSeparator menuDivider;
		private System.Windows.Forms.ToolStripMenuItem ViewSourceMenuItem;
	}
}
