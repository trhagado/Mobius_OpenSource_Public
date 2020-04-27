namespace Mobius.ClientComponents
{
	partial class QueryResultsControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QueryResultsControl));
			this.Tabs = new DevExpress.XtraTab.XtraTabControl();
			this.ResultsPage1TabPage = new DevExpress.XtraTab.XtraTabPage();
			this.ResultsPageNTabPage = new DevExpress.XtraTab.XtraTabPage();
			this.PageLayoutTabPage = new DevExpress.XtraTab.XtraTabPage();
			this.ResultsLabel = new DevExpress.XtraEditors.LabelControl();
			this.ToolPanel = new DevExpress.XtraEditors.PanelControl();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.PageContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.NewPageOrViewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.NewViewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.ArrangeStackedMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ArrangeSideBySideMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ArrangeEvenlyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ArrangeTabbedMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
			this.ShowFilterPanelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ShowSelectedDataRowsPanelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.RenameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.DuplicatePageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.DeletePageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ResultsPageControlContainer = new DevExpress.XtraEditors.PanelControl();
			this.AddViewContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.TempMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.Tabs)).BeginInit();
			this.Tabs.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ToolPanel)).BeginInit();
			this.ToolPanel.SuspendLayout();
			this.PageContextMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ResultsPageControlContainer)).BeginInit();
			this.AddViewContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// Tabs
			// 
			this.Tabs.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Tabs.Appearance.Options.UseBackColor = true;
			this.Tabs.AppearancePage.Header.BackColor = System.Drawing.Color.Transparent;
			this.Tabs.AppearancePage.Header.Options.UseBackColor = true;
			this.Tabs.AppearancePage.HeaderActive.BackColor = System.Drawing.Color.White;
			this.Tabs.AppearancePage.HeaderActive.BorderColor = System.Drawing.Color.Gray;
			this.Tabs.AppearancePage.HeaderActive.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Tabs.AppearancePage.HeaderActive.ForeColor = System.Drawing.Color.Navy;
			this.Tabs.AppearancePage.HeaderActive.Options.UseBackColor = true;
			this.Tabs.AppearancePage.HeaderActive.Options.UseBorderColor = true;
			this.Tabs.AppearancePage.HeaderActive.Options.UseFont = true;
			this.Tabs.AppearancePage.HeaderActive.Options.UseForeColor = true;
			this.Tabs.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.Tabs.HeaderAutoFill = DevExpress.Utils.DefaultBoolean.False;
			this.Tabs.HeaderButtons = ((DevExpress.XtraTab.TabButtons)((DevExpress.XtraTab.TabButtons.Prev | DevExpress.XtraTab.TabButtons.Next)));
			this.Tabs.HeaderButtonsShowMode = DevExpress.XtraTab.TabButtonShowMode.Never;
			this.Tabs.Location = new System.Drawing.Point(122, 2);
			this.Tabs.LookAndFeel.SkinName = "Blue";
			this.Tabs.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.UltraFlat;
			this.Tabs.LookAndFeel.UseDefaultLookAndFeel = false;
			this.Tabs.Margin = new System.Windows.Forms.Padding(0);
			this.Tabs.MultiLine = DevExpress.Utils.DefaultBoolean.False;
			this.Tabs.Name = "Tabs";
			this.Tabs.SelectedTabPage = this.ResultsPage1TabPage;
			this.Tabs.Size = new System.Drawing.Size(529, 24);
			this.Tabs.TabIndex = 181;
			this.Tabs.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.ResultsPage1TabPage,
            this.ResultsPageNTabPage,
            this.PageLayoutTabPage});
			this.Tabs.SelectedPageChanged += new DevExpress.XtraTab.TabPageChangedEventHandler(this.Tabs_SelectedPageChanged);
			this.Tabs.SelectedPageChanging += new DevExpress.XtraTab.TabPageChangingEventHandler(this.Tabs_SelectedPageChanging);
			this.Tabs.CloseButtonClick += new System.EventHandler(this.Tabs_CloseButtonClick);
			this.Tabs.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.Tabs_ControlAdded);
			this.Tabs.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Tabs_MouseUp);
			// 
			// ResultsPage1TabPage
			// 
			this.ResultsPage1TabPage.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("ResultsPage1TabPage.ImageOptions.Image")));
			this.ResultsPage1TabPage.Name = "ResultsPage1TabPage";
			this.ResultsPage1TabPage.Size = new System.Drawing.Size(529, 0);
			this.ResultsPage1TabPage.Text = "Results Page";
			// 
			// ResultsPageNTabPage
			// 
			this.ResultsPageNTabPage.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("ResultsPageNTabPage.ImageOptions.Image")));
			this.ResultsPageNTabPage.Name = "ResultsPageNTabPage";
			this.ResultsPageNTabPage.Size = new System.Drawing.Size(529, 0);
			this.ResultsPageNTabPage.Text = "Results Page N";
			// 
			// PageLayoutTabPage
			// 
			this.PageLayoutTabPage.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("PageLayoutTabPage.ImageOptions.Image")));
			this.PageLayoutTabPage.Name = "PageLayoutTabPage";
			this.PageLayoutTabPage.Size = new System.Drawing.Size(529, -1);
			this.PageLayoutTabPage.Text = "Add View";
			// 
			// ResultsLabel
			// 
			this.ResultsLabel.Appearance.Font = new System.Drawing.Font("Arial", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
			this.ResultsLabel.Appearance.Options.UseFont = true;
			this.ResultsLabel.Location = new System.Drawing.Point(8, 5);
			this.ResultsLabel.Name = "ResultsLabel";
			this.ResultsLabel.Size = new System.Drawing.Size(105, 17);
			this.ResultsLabel.TabIndex = 199;
			this.ResultsLabel.Text = "Query Results:";
			this.ResultsLabel.Click += new System.EventHandler(this.ResultsLabel_Click);
			// 
			// ToolPanel
			// 
			this.ToolPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ToolPanel.Controls.Add(this.labelControl1);
			this.ToolPanel.Location = new System.Drawing.Point(654, 2);
			this.ToolPanel.Name = "ToolPanel";
			this.ToolPanel.Size = new System.Drawing.Size(143, 24);
			this.ToolPanel.TabIndex = 230;
			// 
			// labelControl1
			// 
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.Location = new System.Drawing.Point(5, 5);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(140, 14);
			this.labelControl1.TabIndex = 0;
			this.labelControl1.Text = "View-specific tools go here";
			// 
			// PageContextMenu
			// 
			this.PageContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewPageOrViewMenuItem,
            this.NewViewMenuItem,
            this.toolStripMenuItem1,
            this.ArrangeStackedMenuItem,
            this.ArrangeSideBySideMenuItem,
            this.ArrangeEvenlyMenuItem,
            this.ArrangeTabbedMenuItem,
            this.toolStripMenuItem5,
            this.ShowFilterPanelMenuItem,
            this.ShowSelectedDataRowsPanelMenuItem,
            this.toolStripMenuItem2,
            this.RenameMenuItem,
            this.DuplicatePageMenuItem,
            this.DeletePageMenuItem});
			this.PageContextMenu.Name = "SortContextMenu";
			this.PageContextMenu.Size = new System.Drawing.Size(241, 264);
			this.PageContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.PageContextMenu_Opening);
			// 
			// NewPageOrViewMenuItem
			// 
			this.NewPageOrViewMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("NewPageOrViewMenuItem.Image")));
			this.NewPageOrViewMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.NewPageOrViewMenuItem.Name = "NewPageOrViewMenuItem";
			this.NewPageOrViewMenuItem.Size = new System.Drawing.Size(240, 22);
			this.NewPageOrViewMenuItem.Text = "New Page";
			this.NewPageOrViewMenuItem.Visible = false;
			this.NewPageOrViewMenuItem.Click += new System.EventHandler(this.NewPageMenuItem_Click);
			// 
			// NewViewMenuItem
			// 
			this.NewViewMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("NewViewMenuItem.Image")));
			this.NewViewMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.NewViewMenuItem.Name = "NewViewMenuItem";
			this.NewViewMenuItem.Size = new System.Drawing.Size(240, 22);
			this.NewViewMenuItem.Text = "New View...";
			this.NewViewMenuItem.Visible = false;
			this.NewViewMenuItem.Click += new System.EventHandler(this.NewViewMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(237, 6);
			this.toolStripMenuItem1.Visible = false;
			// 
			// ArrangeStackedMenuItem
			// 
			this.ArrangeStackedMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ArrangeStackedMenuItem.Image")));
			this.ArrangeStackedMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ArrangeStackedMenuItem.Name = "ArrangeStackedMenuItem";
			this.ArrangeStackedMenuItem.Size = new System.Drawing.Size(240, 22);
			this.ArrangeStackedMenuItem.Text = "Arrange Views Stacked";
			this.ArrangeStackedMenuItem.Visible = false;
			this.ArrangeStackedMenuItem.Click += new System.EventHandler(this.ArrangeStackedMenuItem_Click);
			// 
			// ArrangeSideBySideMenuItem
			// 
			this.ArrangeSideBySideMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ArrangeSideBySideMenuItem.Image")));
			this.ArrangeSideBySideMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ArrangeSideBySideMenuItem.Name = "ArrangeSideBySideMenuItem";
			this.ArrangeSideBySideMenuItem.Size = new System.Drawing.Size(240, 22);
			this.ArrangeSideBySideMenuItem.Text = "Arrange Views Side-by-Side";
			this.ArrangeSideBySideMenuItem.Visible = false;
			this.ArrangeSideBySideMenuItem.Click += new System.EventHandler(this.ArrangeSideBySideMenuItem_Click);
			// 
			// ArrangeEvenlyMenuItem
			// 
			this.ArrangeEvenlyMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ArrangeEvenlyMenuItem.Image")));
			this.ArrangeEvenlyMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ArrangeEvenlyMenuItem.Name = "ArrangeEvenlyMenuItem";
			this.ArrangeEvenlyMenuItem.Size = new System.Drawing.Size(240, 22);
			this.ArrangeEvenlyMenuItem.Text = "Arrange in Rows and Columns";
			this.ArrangeEvenlyMenuItem.Visible = false;
			this.ArrangeEvenlyMenuItem.Click += new System.EventHandler(this.ArrangeEvenlyMenuItem_Click);
			// 
			// ArrangeTabbedMenuItem
			// 
			this.ArrangeTabbedMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ArrangeTabbedMenuItem.Image")));
			this.ArrangeTabbedMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ArrangeTabbedMenuItem.Name = "ArrangeTabbedMenuItem";
			this.ArrangeTabbedMenuItem.Size = new System.Drawing.Size(240, 22);
			this.ArrangeTabbedMenuItem.Text = "Arrange in Tabs";
			this.ArrangeTabbedMenuItem.Visible = false;
			this.ArrangeTabbedMenuItem.Click += new System.EventHandler(this.ArrangeTabbedMenuItem_Click);
			// 
			// toolStripMenuItem5
			// 
			this.toolStripMenuItem5.Name = "toolStripMenuItem5";
			this.toolStripMenuItem5.Size = new System.Drawing.Size(237, 6);
			this.toolStripMenuItem5.Visible = false;
			// 
			// ShowFilterPanelMenuItem
			// 
			this.ShowFilterPanelMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ShowFilterPanelMenuItem.Image")));
			this.ShowFilterPanelMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ShowFilterPanelMenuItem.Name = "ShowFilterPanelMenuItem";
			this.ShowFilterPanelMenuItem.Size = new System.Drawing.Size(240, 22);
			this.ShowFilterPanelMenuItem.Text = "Show Filter Panel";
			this.ShowFilterPanelMenuItem.Visible = false;
			this.ShowFilterPanelMenuItem.Click += new System.EventHandler(this.ShowFilterPanelMenuItem_Click);
			// 
			// ShowSelectedDataRowsPanelMenuItem
			// 
			this.ShowSelectedDataRowsPanelMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ShowSelectedDataRowsPanelMenuItem.Image")));
			this.ShowSelectedDataRowsPanelMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ShowSelectedDataRowsPanelMenuItem.Name = "ShowSelectedDataRowsPanelMenuItem";
			this.ShowSelectedDataRowsPanelMenuItem.Size = new System.Drawing.Size(240, 22);
			this.ShowSelectedDataRowsPanelMenuItem.Text = "Show Selected Data Rows Panel";
			this.ShowSelectedDataRowsPanelMenuItem.Visible = false;
			this.ShowSelectedDataRowsPanelMenuItem.Click += new System.EventHandler(this.ShowSelectedDataRowsPanelMenuItem_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(237, 6);
			this.toolStripMenuItem2.Visible = false;
			// 
			// RenameMenuItem
			// 
			this.RenameMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("RenameMenuItem.Image")));
			this.RenameMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.RenameMenuItem.Name = "RenameMenuItem";
			this.RenameMenuItem.Size = new System.Drawing.Size(240, 22);
			this.RenameMenuItem.Text = "Rename View...";
			this.RenameMenuItem.Click += new System.EventHandler(this.RenameMenuItem_Click);
			// 
			// DuplicatePageMenuItem
			// 
			this.DuplicatePageMenuItem.Enabled = false;
			this.DuplicatePageMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("DuplicatePageMenuItem.Image")));
			this.DuplicatePageMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.DuplicatePageMenuItem.Name = "DuplicatePageMenuItem";
			this.DuplicatePageMenuItem.Size = new System.Drawing.Size(240, 22);
			this.DuplicatePageMenuItem.Text = "Duplicate View";
			this.DuplicatePageMenuItem.Visible = false;
			this.DuplicatePageMenuItem.Click += new System.EventHandler(this.DuplicatePageMenuItem_Click);
			// 
			// DeletePageMenuItem
			// 
			this.DeletePageMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Delete;
			this.DeletePageMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.DeletePageMenuItem.Name = "DeletePageMenuItem";
			this.DeletePageMenuItem.Size = new System.Drawing.Size(240, 22);
			this.DeletePageMenuItem.Text = "Delete View...";
			this.DeletePageMenuItem.Click += new System.EventHandler(this.DeletePageMenuItem_Click);
			// 
			// ResultsPageControlContainer
			// 
			this.ResultsPageControlContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ResultsPageControlContainer.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
			this.ResultsPageControlContainer.Location = new System.Drawing.Point(0, 27);
			this.ResultsPageControlContainer.Name = "ResultsPageControlContainer";
			this.ResultsPageControlContainer.Size = new System.Drawing.Size(798, 624);
			this.ResultsPageControlContainer.TabIndex = 232;
			// 
			// AddViewContextMenu
			// 
			this.AddViewContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TempMenuItem});
			this.AddViewContextMenu.Name = "SortContextMenu";
			this.AddViewContextMenu.Size = new System.Drawing.Size(164, 26);
			// 
			// TempMenuItem
			// 
			this.TempMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("TempMenuItem.Image")));
			this.TempMenuItem.Name = "TempMenuItem";
			this.TempMenuItem.Size = new System.Drawing.Size(163, 22);
			this.TempMenuItem.Text = "Filled at Runtime";
			// 
			// QueryResultsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ResultsPageControlContainer);
			this.Controls.Add(this.ToolPanel);
			this.Controls.Add(this.Tabs);
			this.Controls.Add(this.ResultsLabel);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "QueryResultsControl";
			this.Size = new System.Drawing.Size(798, 651);
			((System.ComponentModel.ISupportInitialize)(this.Tabs)).EndInit();
			this.Tabs.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ToolPanel)).EndInit();
			this.ToolPanel.ResumeLayout(false);
			this.PageContextMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ResultsPageControlContainer)).EndInit();
			this.AddViewContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DevExpress.XtraTab.XtraTabPage ResultsPage1TabPage;
		public DevExpress.XtraTab.XtraTabControl Tabs;
		private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn CheckMarkCol;
		//internal Mobius.ClientComponents.ChartPageControl ChartPageControl = null;
		internal DevExpress.XtraTab.XtraTabPage ResultsPageNTabPage;
		internal DevExpress.XtraTab.XtraTabPage PageLayoutTabPage;
		internal DevExpress.XtraEditors.PanelControl ToolPanel;
		private DevExpress.XtraEditors.LabelControl labelControl1;
		internal System.Windows.Forms.ContextMenuStrip PageContextMenu;
		public System.Windows.Forms.ToolStripMenuItem ArrangeStackedMenuItem;
		public System.Windows.Forms.ToolStripMenuItem ArrangeSideBySideMenuItem;
		public System.Windows.Forms.ToolStripMenuItem ArrangeEvenlyMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ArrangeTabbedMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
		private System.Windows.Forms.ToolStripMenuItem ShowFilterPanelMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ShowSelectedDataRowsPanelMenuItem;
		private System.Windows.Forms.ToolStripMenuItem NewPageOrViewMenuItem;
		private System.Windows.Forms.ToolStripMenuItem NewViewMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem RenameMenuItem;
		private System.Windows.Forms.ToolStripMenuItem DuplicatePageMenuItem;
		private System.Windows.Forms.ToolStripMenuItem DeletePageMenuItem;
		internal DevExpress.XtraEditors.PanelControl ResultsPageControlContainer;
		internal System.Windows.Forms.ContextMenuStrip AddViewContextMenu;
		public DevExpress.XtraEditors.LabelControl ResultsLabel;
		private System.Windows.Forms.ToolStripMenuItem TempMenuItem;
	}
}
