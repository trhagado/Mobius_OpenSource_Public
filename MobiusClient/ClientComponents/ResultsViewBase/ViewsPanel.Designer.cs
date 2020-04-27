namespace Mobius.ClientComponents
{
	partial class ViewsPanel
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewsPanel));
			this.DockManager = new DevExpress.XtraBars.Docking.DockManager(this.components);
			this.Bitmaps9x9 = new System.Windows.Forms.ImageList(this.components);
			((System.ComponentModel.ISupportInitialize)(this.DockManager)).BeginInit();
			this.SuspendLayout();
			// 
			// DockManager
			// 
			this.DockManager.Form = this;
			this.DockManager.Images = this.Bitmaps9x9;
			this.DockManager.TopZIndexControls.AddRange(new string[] {
            "DevExpress.XtraBars.BarDockControl",
            "DevExpress.XtraBars.StandaloneBarDockControl",
            "System.Windows.Forms.StatusBar",
            "DevExpress.XtraBars.Ribbon.RibbonStatusBar",
            "DevExpress.XtraBars.Ribbon.RibbonControl"});
			this.DockManager.ActivePanelChanged += new DevExpress.XtraBars.Docking.ActivePanelChangedEventHandler(this.DockManager_ActivePanelChanged);
			this.DockManager.RegisterDockPanel += new DevExpress.XtraBars.Docking.DockPanelEventHandler(this.DockManager_RegisterDockPanel);
			this.DockManager.UnregisterDockPanel += new DevExpress.XtraBars.Docking.DockPanelEventHandler(this.DockManager_UnregisterDockPanel);
			this.DockManager.ClosingPanel += new DevExpress.XtraBars.Docking.DockPanelCancelEventHandler(this.DockManager_ClosingPanel);
			this.DockManager.ClosedPanel += new DevExpress.XtraBars.Docking.DockPanelEventHandler(this.DockManager_ClosedPanel);
			this.DockManager.VisibilityChanged += new DevExpress.XtraBars.Docking.VisibilityChangedEventHandler(this.DockManager_VisibilityChanged);
			this.DockManager.TabbedChanged += new DevExpress.XtraBars.Docking.DockPanelEventHandler(this.DockManager_TabbedChanged);
			this.DockManager.TabsPositionChanged += new DevExpress.XtraBars.Docking.TabsPositionChangedEventHandler(this.DockManager_TabsPositionChanged);
			this.DockManager.ActiveChildChanged += new DevExpress.XtraBars.Docking.DockPanelEventHandler(this.DockManager_ActiveChildChanged);
			this.DockManager.EndDocking += new DevExpress.XtraBars.Docking.EndDockingEventHandler(this.DockManager_EndDocking);
			this.DockManager.EndSizing += new DevExpress.XtraBars.Docking.EndSizingEventHandler(this.DockManager_EndSizing);
			this.DockManager.PopupMenuShowing += new DevExpress.XtraBars.Docking.PopupMenuShowingEventHandler(this.DockManager_PopupMenuShowing);
			// 
			// Bitmaps9x9
			// 
			this.Bitmaps9x9.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps9x9.ImageStream")));
			this.Bitmaps9x9.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps9x9.Images.SetKeyName(0, "MaximizeWindow2.bmp");
			this.Bitmaps9x9.Images.SetKeyName(1, "RestoreWindow2.bmp");
			this.Bitmaps9x9.Images.SetKeyName(2, "CloseWindow2.bmp");
			// 
			// ResultsPageViewsPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Name = "ResultsPageViewsPanel";
			this.Size = new System.Drawing.Size(459, 417);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.ResultsPageViewsPanel_Paint);
			((System.ComponentModel.ISupportInitialize)(this.DockManager)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal DevExpress.XtraBars.Docking.DockManager DockManager;
		private System.Windows.Forms.ImageList Bitmaps9x9;


	}
}
