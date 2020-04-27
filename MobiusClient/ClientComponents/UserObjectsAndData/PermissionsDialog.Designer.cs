namespace Mobius.ClientComponents
{
	partial class PermissionsDialog
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PermissionsDialog));
			this.repositoryItemCheckEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
			this.AdvancedButton = new DevExpress.XtraEditors.SimpleButton();
			this.Bitmaps8x8 = new System.Windows.Forms.ImageList(this.components);
			this.CancelBut = new DevExpress.XtraEditors.SimpleButton();
			this.OkButton = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.MakePublic = new DevExpress.XtraEditors.SimpleButton();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.MakePrivate = new DevExpress.XtraEditors.SimpleButton();
			this.AdvancedMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.CreateUserGroupMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ViewUserGroupMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.EditUserGroupMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ChangeOwnerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.PermissionsList = new Mobius.ClientComponents.PermissionsList();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit1)).BeginInit();
			this.AdvancedMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// repositoryItemCheckEdit1
			// 
			this.repositoryItemCheckEdit1.AutoHeight = false;
			this.repositoryItemCheckEdit1.Name = "repositoryItemCheckEdit1";
			// 
			// AdvancedButton
			// 
			this.AdvancedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AdvancedButton.ImageIndex = 0;
			this.AdvancedButton.ImageList = this.Bitmaps8x8;
			this.AdvancedButton.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleRight;
			this.AdvancedButton.Location = new System.Drawing.Point(198, 300);
			this.AdvancedButton.Name = "AdvancedButton";
			this.AdvancedButton.Size = new System.Drawing.Size(81, 24);
			this.AdvancedButton.TabIndex = 212;
			this.AdvancedButton.Text = "Advanced";
			this.AdvancedButton.Click += new System.EventHandler(this.AdvancedButton_Click);
			// 
			// Bitmaps8x8
			// 
			this.Bitmaps8x8.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps8x8.ImageStream")));
			this.Bitmaps8x8.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps8x8.Images.SetKeyName(0, "DropDown8x8.bmp");
			// 
			// CancelBut
			// 
			this.CancelBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelBut.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBut.Location = new System.Drawing.Point(418, 300);
			this.CancelBut.Name = "CancelBut";
			this.CancelBut.Size = new System.Drawing.Size(66, 24);
			this.CancelBut.TabIndex = 213;
			this.CancelBut.Text = "Cancel";
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.Location = new System.Drawing.Point(346, 300);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = new System.Drawing.Size(66, 24);
			this.OkButton.TabIndex = 214;
			this.OkButton.Text = "OK";
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// labelControl1
			// 
			this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.Location = new System.Drawing.Point(6, 7);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(497, 13);
			this.labelControl1.TabIndex = 215;
			this.labelControl1.Text = "Groups and users that currently have access:";
			// 
			// labelControl2
			// 
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.LineVisible = true;
			this.labelControl2.Location = new System.Drawing.Point(-2, 290);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(491, 4);
			this.labelControl2.TabIndex = 216;
			// 
			// MakePublic
			// 
			this.MakePublic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.MakePublic.ImageIndex = 1;
			this.MakePublic.ImageList = this.Bitmaps16x16;
			this.MakePublic.Location = new System.Drawing.Point(4, 300);
			this.MakePublic.Name = "MakePublic";
			this.MakePublic.Size = new System.Drawing.Size(88, 24);
			this.MakePublic.TabIndex = 217;
			this.MakePublic.Text = "Make Public";
			this.MakePublic.Click += new System.EventHandler(this.MakePublic_Click);
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "Person.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "People.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "Delete.bmp");
			// 
			// MakePrivate
			// 
			this.MakePrivate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.MakePrivate.ImageIndex = 0;
			this.MakePrivate.ImageList = this.Bitmaps16x16;
			this.MakePrivate.Location = new System.Drawing.Point(98, 300);
			this.MakePrivate.Name = "MakePrivate";
			this.MakePrivate.Size = new System.Drawing.Size(94, 24);
			this.MakePrivate.TabIndex = 218;
			this.MakePrivate.Text = "Make Private";
			this.MakePrivate.Click += new System.EventHandler(this.MakePrivate_Click);
			// 
			// AdvancedMenu
			// 
			this.AdvancedMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CreateUserGroupMenuItem,
            this.EditUserGroupMenuItem,
            this.ViewUserGroupMenuItem,
            this.ChangeOwnerMenuItem});
			this.AdvancedMenu.Name = "SummaryTypeMenu";
			this.AdvancedMenu.Size = new System.Drawing.Size(264, 92);
			// 
			// CreateUserGroupMenuItem
			// 
			this.CreateUserGroupMenuItem.Name = "CreateUserGroupMenuItem";
			this.CreateUserGroupMenuItem.Size = new System.Drawing.Size(263, 22);
			this.CreateUserGroupMenuItem.Text = "Create a New User Group...";
			this.CreateUserGroupMenuItem.Click += new System.EventHandler(this.CreateUserGroupMenuItem_Click);
			// 
			// ViewUserGroupMenuItem
			// 
			this.ViewUserGroupMenuItem.Name = "ViewUserGroupMenuItem";
			this.ViewUserGroupMenuItem.Size = new System.Drawing.Size(263, 22);
			this.ViewUserGroupMenuItem.Text = "View the Members of a User Group...";
			this.ViewUserGroupMenuItem.Click += new System.EventHandler(this.ViewUserGroupMenuItem_Click);
			// 
			// EditUserGroupMenuItem
			// 
			this.EditUserGroupMenuItem.Name = "EditUserGroupMenuItem";
			this.EditUserGroupMenuItem.Size = new System.Drawing.Size(263, 22);
			this.EditUserGroupMenuItem.Text = "Edit a User Group that You Control...";
			this.EditUserGroupMenuItem.Click += new System.EventHandler(this.EditUserGroupMenuItem_Click);
			// 
			// ChangeOwnerMenuItem
			// 
			this.ChangeOwnerMenuItem.Name = "ChangeOwnerMenuItem";
			this.ChangeOwnerMenuItem.Size = new System.Drawing.Size(263, 22);
			this.ChangeOwnerMenuItem.Text = "Change the Owner...";
			this.ChangeOwnerMenuItem.Click += new System.EventHandler(this.ChangeOwnerMenuItem_Click);
			// 
			// PermissionsList
			// 
			this.PermissionsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.PermissionsList.Location = new System.Drawing.Point(4, 26);
			this.PermissionsList.Name = "PermissionsList";
			this.PermissionsList.Size = new System.Drawing.Size(481, 263);
			this.PermissionsList.TabIndex = 219;
			// 
			// PermissionsDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.CancelBut;
			this.ClientSize = new System.Drawing.Size(489, 330);
			this.Controls.Add(this.PermissionsList);
			this.Controls.Add(this.MakePrivate);
			this.Controls.Add(this.MakePublic);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.CancelBut);
			this.Controls.Add(this.AdvancedButton);
			this.Controls.Add(this.labelControl2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PermissionsDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Permissions";
			this.Activated += new System.EventHandler(this.PermissionsDialog_Activated);
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit1)).EndInit();
			this.AdvancedMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraGrid.Columns.GridColumn UserCol;
		private DevExpress.XtraEditors.SimpleButton AdvancedButton;
		private DevExpress.XtraEditors.SimpleButton CancelBut;
		private DevExpress.XtraEditors.SimpleButton OkButton;
		private DevExpress.XtraEditors.LabelControl labelControl1;
		private DevExpress.XtraEditors.LabelControl labelControl2;
		private DevExpress.XtraEditors.SimpleButton MakePublic;
		private DevExpress.XtraEditors.SimpleButton MakePrivate;
		public System.Windows.Forms.ImageList Bitmaps16x16;
		private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repositoryItemCheckEdit1;
		private System.Windows.Forms.ContextMenuStrip AdvancedMenu;
		private System.Windows.Forms.ToolStripMenuItem CreateUserGroupMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ChangeOwnerMenuItem;
		public System.Windows.Forms.ImageList Bitmaps8x8;
		private System.Windows.Forms.ToolStripMenuItem EditUserGroupMenuItem;
		private PermissionsList PermissionsList;
		private System.Windows.Forms.ToolStripMenuItem ViewUserGroupMenuItem;
	}
}