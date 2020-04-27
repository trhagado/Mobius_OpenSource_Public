namespace Mobius.ClientComponents
{
	partial class MoleculeSelectorControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MoleculeSelectorControl));
			this.CloseButton = new DevExpress.XtraEditors.SimpleButton();
			this.NewMoleculeMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.NewChemicalStructureMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.NewBiopolymerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.FromDatabaseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SavedStructureMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.FromRecentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.FromFavoriteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.AddToFavoritesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.ClearStructureMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.StructureListControl = new Mobius.ClientComponents.StructureListControl();
			this.NewMoleculeMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = new System.Drawing.Point(560, 714);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(79, 23);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.Text = "Close";
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// NewMoleculeMenu
			// 
			this.NewMoleculeMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewChemicalStructureMenuItem,
            this.NewBiopolymerMenuItem,
            this.toolStripMenuItem1,
            this.FromDatabaseMenuItem,
            this.SavedStructureMenuItem,
            this.ToolStripSeparator1,
            this.FromRecentMenuItem,
            this.FromFavoriteMenuItem,
            this.AddToFavoritesMenuItem,
            this.toolStripSeparator2,
            this.ClearStructureMenuItem});
			this.NewMoleculeMenu.Name = "RetrieveModelMenu";
			this.NewMoleculeMenu.Size = new System.Drawing.Size(321, 220);
			// 
			// NewChemicalStructureMenuItem
			// 
			this.NewChemicalStructureMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("NewChemicalStructureMenuItem.Image")));
			this.NewChemicalStructureMenuItem.Name = "NewChemicalStructureMenuItem";
			this.NewChemicalStructureMenuItem.Size = new System.Drawing.Size(320, 22);
			this.NewChemicalStructureMenuItem.Text = "Edit a New Chemical Structure";
			this.NewChemicalStructureMenuItem.Click += new System.EventHandler(this.NewChemicalStructureMenuItem_Click);
			// 
			// NewBiopolymerMenuItem
			// 
			this.NewBiopolymerMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("NewBiopolymerMenuItem.Image")));
			this.NewBiopolymerMenuItem.Name = "NewBiopolymerMenuItem";
			this.NewBiopolymerMenuItem.Size = new System.Drawing.Size(320, 22);
			this.NewBiopolymerMenuItem.Text = "Edit a New Biopolymer";
			this.NewBiopolymerMenuItem.Click += new System.EventHandler(this.NewBiopolymerMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(317, 6);
			// 
			// FromDatabaseMenuItem
			// 
			this.FromDatabaseMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("FromDatabaseMenuItem.Image")));
			this.FromDatabaseMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.FromDatabaseMenuItem.Name = "FromDatabaseMenuItem";
			this.FromDatabaseMenuItem.Size = new System.Drawing.Size(320, 22);
			this.FromDatabaseMenuItem.Text = "From a Database...";
			this.FromDatabaseMenuItem.Click += new System.EventHandler(this.FromDatabaseMenuItem_Click);
			// 
			// SavedStructureMenuItem
			// 
			this.SavedStructureMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("SavedStructureMenuItem.Image")));
			this.SavedStructureMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.SavedStructureMenuItem.Name = "SavedStructureMenuItem";
			this.SavedStructureMenuItem.Size = new System.Drawing.Size(320, 22);
			this.SavedStructureMenuItem.Text = "From a Saved Molecule File...";
			this.SavedStructureMenuItem.Click += new System.EventHandler(this.SavedStructureMenuItem_Click);
			// 
			// ToolStripSeparator1
			// 
			this.ToolStripSeparator1.Name = "ToolStripSeparator1";
			this.ToolStripSeparator1.Size = new System.Drawing.Size(317, 6);
			// 
			// FromRecentMenuItem
			// 
			this.FromRecentMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("FromRecentMenuItem.Image")));
			this.FromRecentMenuItem.Name = "FromRecentMenuItem";
			this.FromRecentMenuItem.Size = new System.Drawing.Size(320, 22);
			this.FromRecentMenuItem.Text = "From Recent Molecules...";
			this.FromRecentMenuItem.Click += new System.EventHandler(this.FromRecentStructure_Click);
			// 
			// FromFavoriteMenuItem
			// 
			this.FromFavoriteMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("FromFavoriteMenuItem.Image")));
			this.FromFavoriteMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.FromFavoriteMenuItem.Name = "FromFavoriteMenuItem";
			this.FromFavoriteMenuItem.Size = new System.Drawing.Size(320, 22);
			this.FromFavoriteMenuItem.Text = "From Favorite Molecules...";
			this.FromFavoriteMenuItem.Click += new System.EventHandler(this.FromFavoriteMenuItem_Click);
			// 
			// AddToFavoritesMenuItem
			// 
			this.AddToFavoritesMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("AddToFavoritesMenuItem.Image")));
			this.AddToFavoritesMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.AddToFavoritesMenuItem.Name = "AddToFavoritesMenuItem";
			this.AddToFavoritesMenuItem.Size = new System.Drawing.Size(320, 22);
			this.AddToFavoritesMenuItem.Text = "Add the current molecule to the Favorites list...";
			this.AddToFavoritesMenuItem.Click += new System.EventHandler(this.AddToFavoritesMenuItem_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(317, 6);
			// 
			// ClearStructureMenuItem
			// 
			this.ClearStructureMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ClearStructureMenuItem.Image")));
			this.ClearStructureMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ClearStructureMenuItem.Name = "ClearStructureMenuItem";
			this.ClearStructureMenuItem.Size = new System.Drawing.Size(320, 22);
			this.ClearStructureMenuItem.Text = "&Clear the Molecule";
			this.ClearStructureMenuItem.Click += new System.EventHandler(this.ClearStructureMenuItem_Click);
			// 
			// StructureListControl
			// 
			this.StructureListControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StructureListControl.Location = new System.Drawing.Point(0, 0);
			this.StructureListControl.Name = "StructureListControl";
			this.StructureListControl.Size = new System.Drawing.Size(645, 744);
			this.StructureListControl.TabIndex = 1;
			// 
			// MoleculeSelectorListControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.CloseButton;
			this.ClientSize = new System.Drawing.Size(645, 744);
			this.Controls.Add(this.StructureListControl);
			this.Controls.Add(this.CloseButton);
			this.MinimizeBox = false;
			this.Name = "MoleculeSelectorListControl";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Select Desired Molecule";
			this.NewMoleculeMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private StructureListControl StructureListControl;
		private DevExpress.XtraEditors.SimpleButton CloseButton;
		private System.Windows.Forms.ContextMenuStrip NewMoleculeMenu;
		private System.Windows.Forms.ToolStripMenuItem FromDatabaseMenuItem;
		private System.Windows.Forms.ToolStripMenuItem SavedStructureMenuItem;
		private System.Windows.Forms.ToolStripSeparator ToolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem FromRecentMenuItem;
		private System.Windows.Forms.ToolStripMenuItem FromFavoriteMenuItem;
		private System.Windows.Forms.ToolStripMenuItem AddToFavoritesMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem ClearStructureMenuItem;
		private System.Windows.Forms.ToolStripMenuItem NewChemicalStructureMenuItem;
		private System.Windows.Forms.ToolStripMenuItem NewBiopolymerMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
	}
}