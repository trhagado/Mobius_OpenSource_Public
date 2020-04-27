namespace Mobius.ClientComponents
{
	partial class ResultsPagePanel
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResultsPagePanel));
			this.DockManager = new DevExpress.XtraBars.Docking.DockManager(this.components);
			this.DodDockPanel = new DevExpress.XtraBars.Docking.DockPanel();
			this.dockPanel1_Container = new DevExpress.XtraBars.Docking.ControlContainer();
			this.DoDGridPanel = new Mobius.ClientComponents.MoleculeGridPanel();
			this.FiltersDockPanel = new DevExpress.XtraBars.Docking.DockPanel();
			this.dockPanel2_Container = new DevExpress.XtraBars.Docking.ControlContainer();
			this.FilterPanel = new Mobius.ClientComponents.FilterPanel();
			this.SelectFieldMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.Table1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.Field1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.Bitmaps8x8 = new System.Windows.Forms.ImageList(this.components);
			this.ViewsPanel = new Mobius.ClientComponents.ViewsPanel();
			((System.ComponentModel.ISupportInitialize)(this.DockManager)).BeginInit();
			this.DodDockPanel.SuspendLayout();
			this.dockPanel1_Container.SuspendLayout();
			this.FiltersDockPanel.SuspendLayout();
			this.dockPanel2_Container.SuspendLayout();
			this.SelectFieldMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// DockManager
			// 
			this.DockManager.DockMode = DevExpress.XtraBars.Docking.Helpers.DockMode.Standard;
			this.DockManager.Form = this;
			this.DockManager.RootPanels.AddRange(new DevExpress.XtraBars.Docking.DockPanel[] {
            this.DodDockPanel,
            this.FiltersDockPanel});
			this.DockManager.TopZIndexControls.AddRange(new string[] {
            "DevExpress.XtraBars.BarDockControl",
            "DevExpress.XtraBars.StandaloneBarDockControl",
            "System.Windows.Forms.StatusBar",
            "DevExpress.XtraBars.Ribbon.RibbonStatusBar",
            "DevExpress.XtraBars.Ribbon.RibbonControl"});
			this.DockManager.PopupMenuShowing += new DevExpress.XtraBars.Docking.PopupMenuShowingEventHandler(this.DockManager_PopupMenuShowing);
			// 
			// DodDockPanel
			// 
			this.DodDockPanel.Appearance.BackColor = System.Drawing.Color.White;
			this.DodDockPanel.Appearance.Options.UseBackColor = true;
			this.DodDockPanel.Controls.Add(this.dockPanel1_Container);
			this.DodDockPanel.Dock = DevExpress.XtraBars.Docking.DockingStyle.Bottom;
			this.DodDockPanel.FloatVertical = true;
			this.DodDockPanel.ID = new System.Guid("dc540bac-67a1-4c68-b666-5a053c35851a");
			this.DodDockPanel.Location = new System.Drawing.Point(0, 511);
			this.DodDockPanel.Name = "DodDockPanel";
			this.DodDockPanel.Options.AllowDockAsTabbedDocument = false;
			this.DodDockPanel.Options.FloatOnDblClick = false;
			this.DodDockPanel.Options.ShowAutoHideButton = false;
			this.DodDockPanel.Options.ShowMaximizeButton = false;
			this.DodDockPanel.OriginalSize = new System.Drawing.Size(200, 162);
			this.DodDockPanel.Size = new System.Drawing.Size(862, 162);
			this.DodDockPanel.Text = "Selected Rows";
			this.DodDockPanel.ClosedPanel += new DevExpress.XtraBars.Docking.DockPanelEventHandler(this.DetailsOnDemandDockPanel_ClosedPanel);
			// 
			// dockPanel1_Container
			// 
			this.dockPanel1_Container.Controls.Add(this.DoDGridPanel);
			this.dockPanel1_Container.Location = new System.Drawing.Point(4, 23);
			this.dockPanel1_Container.Name = "dockPanel1_Container";
			this.dockPanel1_Container.Size = new System.Drawing.Size(854, 135);
			this.dockPanel1_Container.TabIndex = 0;
			// 
			// DoDGridPanel
			// 
			this.DoDGridPanel.Appearance.BackColor = System.Drawing.Color.White;
			this.DoDGridPanel.Appearance.Options.UseBackColor = true;
			this.DoDGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DoDGridPanel.Location = new System.Drawing.Point(0, 0);
			this.DoDGridPanel.Name = "DoDGridPanel";
			this.DoDGridPanel.Size = new System.Drawing.Size(854, 135);
			this.DoDGridPanel.TabIndex = 0;
			this.DoDGridPanel.Visible = false;
			// 
			// FiltersDockPanel
			// 
			this.FiltersDockPanel.Appearance.BackColor = System.Drawing.Color.White;
			this.FiltersDockPanel.Appearance.Options.UseBackColor = true;
			this.FiltersDockPanel.Controls.Add(this.dockPanel2_Container);
			this.FiltersDockPanel.Dock = DevExpress.XtraBars.Docking.DockingStyle.Right;
			this.FiltersDockPanel.FloatVertical = true;
			this.FiltersDockPanel.ID = new System.Guid("efc294ca-767b-4e1b-9a04-db8d711ba5eb");
			this.FiltersDockPanel.Location = new System.Drawing.Point(668, 0);
			this.FiltersDockPanel.Name = "FiltersDockPanel";
			this.FiltersDockPanel.Options.AllowDockAsTabbedDocument = false;
			this.FiltersDockPanel.Options.FloatOnDblClick = false;
			this.FiltersDockPanel.Options.ShowAutoHideButton = false;
			this.FiltersDockPanel.Options.ShowMaximizeButton = false;
			this.FiltersDockPanel.OriginalSize = new System.Drawing.Size(194, 265);
			this.FiltersDockPanel.Size = new System.Drawing.Size(194, 511);
			this.FiltersDockPanel.Text = "Filters";
			this.FiltersDockPanel.ClosedPanel += new DevExpress.XtraBars.Docking.DockPanelEventHandler(this.FiltersDockPanel_ClosedPanel);
			// 
			// dockPanel2_Container
			// 
			this.dockPanel2_Container.Controls.Add(this.FilterPanel);
			this.dockPanel2_Container.Location = new System.Drawing.Point(4, 23);
			this.dockPanel2_Container.Name = "dockPanel2_Container";
			this.dockPanel2_Container.Size = new System.Drawing.Size(186, 484);
			this.dockPanel2_Container.TabIndex = 0;
			// 
			// FilterPanel
			// 
			this.FilterPanel.Appearance.BackColor = System.Drawing.Color.White;
			this.FilterPanel.Appearance.Options.UseBackColor = true;
			this.FilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterPanel.Location = new System.Drawing.Point(0, 0);
			this.FilterPanel.Name = "FilterPanel";
			this.FilterPanel.Size = new System.Drawing.Size(186, 484);
			this.FilterPanel.TabIndex = 0;
			// 
			// SelectFieldMenu
			// 
			this.SelectFieldMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Table1MenuItem});
			this.SelectFieldMenu.Name = "SelectFieldMenu";
			this.SelectFieldMenu.Size = new System.Drawing.Size(118, 26);
			// 
			// Table1MenuItem
			// 
			this.Table1MenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Field1MenuItem});
			this.Table1MenuItem.Image = ((System.Drawing.Image)(resources.GetObject("Table1MenuItem.Image")));
			this.Table1MenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.Table1MenuItem.Name = "Table1MenuItem";
			this.Table1MenuItem.Size = new System.Drawing.Size(152, 22);
			this.Table1MenuItem.Text = "Table1";
			// 
			// Field1MenuItem
			// 
			this.Field1MenuItem.Name = "Field1MenuItem";
			this.Field1MenuItem.Size = new System.Drawing.Size(113, 22);
			this.Field1MenuItem.Text = "Field1";
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "MouseSelect.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "MouseZoom.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "MouseTranslate.bmp");
			this.Bitmaps16x16.Images.SetKeyName(3, "MouseRotate.bmp");
			this.Bitmaps16x16.Images.SetKeyName(4, "FullScreen.bmp");
			this.Bitmaps16x16.Images.SetKeyName(5, "maximizeWindow.bmp");
			this.Bitmaps16x16.Images.SetKeyName(6, "RestoreWindow.bmp");
			this.Bitmaps16x16.Images.SetKeyName(7, "CloseWindow.bmp");
			// 
			// Bitmaps8x8
			// 
			this.Bitmaps8x8.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps8x8.ImageStream")));
			this.Bitmaps8x8.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps8x8.Images.SetKeyName(0, "DropDown8x8.bmp");
			// 
			// ViewsPanel
			// 
			this.ViewsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ViewsPanel.Location = new System.Drawing.Point(0, 0);
			this.ViewsPanel.Name = "ViewsPanel";
			this.ViewsPanel.Size = new System.Drawing.Size(668, 511);
			this.ViewsPanel.TabIndex = 205;
			this.ViewsPanel.Resize += new System.EventHandler(this.ViewsPanel_Resize);
			// 
			// ResultsPagePanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ViewsPanel);
			this.Controls.Add(this.FiltersDockPanel);
			this.Controls.Add(this.DodDockPanel);
			this.Name = "ResultsPagePanel";
			this.Size = new System.Drawing.Size(862, 673);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.ResultsPagePanel_Paint);
			((System.ComponentModel.ISupportInitialize)(this.DockManager)).EndInit();
			this.DodDockPanel.ResumeLayout(false);
			this.dockPanel1_Container.ResumeLayout(false);
			this.FiltersDockPanel.ResumeLayout(false);
			this.dockPanel2_Container.ResumeLayout(false);
			this.SelectFieldMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraBars.Docking.DockManager DockManager;
		private DevExpress.XtraBars.Docking.ControlContainer dockPanel2_Container;
		private DevExpress.XtraBars.Docking.ControlContainer dockPanel1_Container;
		internal DevExpress.XtraBars.Docking.DockPanel FiltersDockPanel;
		internal DevExpress.XtraBars.Docking.DockPanel DodDockPanel;
		internal FilterPanel FilterPanel;
		internal MoleculeGridPanel DoDGridPanel;
		private System.Windows.Forms.ImageList Bitmaps16x16;
		private System.Windows.Forms.ImageList Bitmaps8x8;
		private System.Windows.Forms.ToolStripMenuItem Table1MenuItem;
		private System.Windows.Forms.ToolStripMenuItem Field1MenuItem;
		private System.Windows.Forms.ContextMenuStrip SelectFieldMenu;
		internal ViewsPanel ViewsPanel;

	}
}
