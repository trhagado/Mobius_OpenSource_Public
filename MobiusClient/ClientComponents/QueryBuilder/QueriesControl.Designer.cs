namespace Mobius.ClientComponents
{
    partial class QueriesControl
		{

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QueriesControl));
			this.Tabs = new DevExpress.XtraTab.XtraTabControl();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.BrowseMode = new DevExpress.XtraTab.XtraTabPage();
			this.QueryResultsControl = new Mobius.ClientComponents.QueryResultsControl();
			this.BuildMode = new DevExpress.XtraTab.XtraTabPage();
			this.QueryBuilderControl = new Mobius.ClientComponents.QbControl();
			this.QueryContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.QuerySaveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.QuerySaveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
			this.QueryRunMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.QueryOptionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator21 = new System.Windows.Forms.ToolStripSeparator();
			this.DuplicateMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.QueryRemoveAllCriteriaMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.QueryClearMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RenameQueryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.Tabs)).BeginInit();
			this.Tabs.SuspendLayout();
			this.BrowseMode.SuspendLayout();
			this.BuildMode.SuspendLayout();
			this.QueryContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// Tabs
			// 
			this.Tabs.AllowDrop = true;
			this.Tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Tabs.Appearance.BackColor = System.Drawing.Color.WhiteSmoke;
			this.Tabs.Appearance.Options.UseBackColor = true;
			this.Tabs.AppearancePage.HeaderActive.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.Tabs.AppearancePage.HeaderActive.ForeColor = System.Drawing.Color.Navy;
			this.Tabs.AppearancePage.HeaderActive.Options.UseFont = true;
			this.Tabs.AppearancePage.HeaderActive.Options.UseForeColor = true;
			this.Tabs.ClosePageButtonShowMode = DevExpress.XtraTab.ClosePageButtonShowMode.InActiveTabPageHeader;
			this.Tabs.HeaderAutoFill = DevExpress.Utils.DefaultBoolean.False;
			this.Tabs.HeaderButtons = ((DevExpress.XtraTab.TabButtons)(((DevExpress.XtraTab.TabButtons.Prev | DevExpress.XtraTab.TabButtons.Next) 
            | DevExpress.XtraTab.TabButtons.Close)));
			this.Tabs.HeaderButtonsShowMode = DevExpress.XtraTab.TabButtonShowMode.Always;
			this.Tabs.Images = this.Bitmaps16x16;
			this.Tabs.Location = new System.Drawing.Point(0, 2);
			this.Tabs.MultiLine = DevExpress.Utils.DefaultBoolean.False;
			this.Tabs.Name = "Tabs";
			this.Tabs.SelectedTabPage = this.BrowseMode;
			this.Tabs.Size = new System.Drawing.Size(749, 611);
			this.Tabs.TabIndex = 0;
			this.Tabs.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.BuildMode,
            this.BrowseMode});
			this.Tabs.SelectedPageChanged += new DevExpress.XtraTab.TabPageChangedEventHandler(this.Tabs_SelectedPageChanged);
			this.Tabs.CloseButtonClick += new System.EventHandler(this.Tabs_CloseButtonClick);
			this.Tabs.Click += new System.EventHandler(this.Tabs_Click);
			this.Tabs.DragOver += new System.Windows.Forms.DragEventHandler(this.Tabs_DragOver);
			this.Tabs.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Tabs_MouseClick);
			this.Tabs.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Tabs_MouseDown);
			this.Tabs.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Tabs_MouseMove);
			this.Tabs.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Tabs_MouseUp);
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "Query3.bmp");
			// 
			// BrowseMode
			// 
			this.BrowseMode.Controls.Add(this.QueryResultsControl);
			this.BrowseMode.Name = "BrowseMode";
			this.BrowseMode.Size = new System.Drawing.Size(747, 583);
			this.BrowseMode.Text = "BrowseMode";
			// 
			// QueryResultsControl
			// 
			this.QueryResultsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QueryResultsControl.Location = new System.Drawing.Point(0, 0);
			this.QueryResultsControl.Margin = new System.Windows.Forms.Padding(0);
			this.QueryResultsControl.Name = "QueryResultsControl";
			this.QueryResultsControl.Size = new System.Drawing.Size(747, 583);
			this.QueryResultsControl.TabIndex = 0;
			// 
			// BuildMode
			// 
			this.BuildMode.Controls.Add(this.QueryBuilderControl);
			this.BuildMode.ImageOptions.ImageIndex = 0;
			this.BuildMode.Name = "BuildMode";
			this.BuildMode.Size = new System.Drawing.Size(747, 583);
			this.BuildMode.Text = "BuildMode";
			// 
			// QueryBuilderControl
			// 
			this.QueryBuilderControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QueryBuilderControl.Location = new System.Drawing.Point(0, 0);
			this.QueryBuilderControl.Name = "QueryBuilderControl";
			this.QueryBuilderControl.Size = new System.Drawing.Size(747, 583);
			this.QueryBuilderControl.TabIndex = 0;
			// 
			// QueryContextMenu
			// 
			this.QueryContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.QuerySaveMenuItem,
            this.QuerySaveAsMenuItem,
            this.toolStripSeparator20,
            this.QueryRunMenuItem,
            this.QueryOptionsMenuItem,
            this.toolStripSeparator21,
            this.DuplicateMenuItem,
            this.QueryRemoveAllCriteriaMenuItem,
            this.QueryClearMenuItem,
            this.RenameQueryMenuItem});
			this.QueryContextMenu.Name = "RunQueryPlusMenu";
			this.QueryContextMenu.Size = new System.Drawing.Size(185, 192);
			// 
			// QuerySaveMenuItem
			// 
			this.QuerySaveMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Save;
			this.QuerySaveMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.QuerySaveMenuItem.Name = "QuerySaveMenuItem";
			this.QuerySaveMenuItem.Size = new System.Drawing.Size(184, 22);
			this.QuerySaveMenuItem.Text = "&Save";
			this.QuerySaveMenuItem.Click += new System.EventHandler(this.QuerySaveMenuItem_Click);
			// 
			// QuerySaveAsMenuItem
			// 
			this.QuerySaveAsMenuItem.Name = "QuerySaveAsMenuItem";
			this.QuerySaveAsMenuItem.Size = new System.Drawing.Size(184, 22);
			this.QuerySaveAsMenuItem.Text = "Save &As...";
			this.QuerySaveAsMenuItem.Click += new System.EventHandler(this.QuerySaveAsMenuItem_Click);
			// 
			// toolStripSeparator20
			// 
			this.toolStripSeparator20.Name = "toolStripSeparator20";
			this.toolStripSeparator20.Size = new System.Drawing.Size(181, 6);
			// 
			// QueryRunMenuItem
			// 
			this.QueryRunMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.RunQuery;
			this.QueryRunMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.QueryRunMenuItem.Name = "QueryRunMenuItem";
			this.QueryRunMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
			this.QueryRunMenuItem.Size = new System.Drawing.Size(184, 22);
			this.QueryRunMenuItem.Text = "&Run Query";
			this.QueryRunMenuItem.Click += new System.EventHandler(this.QueryRunMenuItem_Click);
			// 
			// QueryOptionsMenuItem
			// 
			this.QueryOptionsMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.QueryOptionsMenuItem.Name = "QueryOptionsMenuItem";
			this.QueryOptionsMenuItem.Size = new System.Drawing.Size(184, 22);
			this.QueryOptionsMenuItem.Text = "Options...";
			this.QueryOptionsMenuItem.Click += new System.EventHandler(this.QueryOptionsMenuItem_Click);
			// 
			// toolStripSeparator21
			// 
			this.toolStripSeparator21.Name = "toolStripSeparator21";
			this.toolStripSeparator21.Size = new System.Drawing.Size(181, 6);
			// 
			// DuplicateMenuItem
			// 
			this.DuplicateMenuItem.Name = "DuplicateMenuItem";
			this.DuplicateMenuItem.Size = new System.Drawing.Size(184, 22);
			this.DuplicateMenuItem.Text = "Duplicate Query";
			this.DuplicateMenuItem.Click += new System.EventHandler(this.DuplicateMenuItem_Click);
			// 
			// QueryRemoveAllCriteriaMenuItem
			// 
			this.QueryRemoveAllCriteriaMenuItem.Name = "QueryRemoveAllCriteriaMenuItem";
			this.QueryRemoveAllCriteriaMenuItem.Size = new System.Drawing.Size(184, 22);
			this.QueryRemoveAllCriteriaMenuItem.Text = "Remove All Criteria...";
			this.QueryRemoveAllCriteriaMenuItem.Click += new System.EventHandler(this.QueryRemoveAllCriteriaMenuItem_Click);
			// 
			// QueryClearMenuItem
			// 
			this.QueryClearMenuItem.Name = "QueryClearMenuItem";
			this.QueryClearMenuItem.Size = new System.Drawing.Size(184, 22);
			this.QueryClearMenuItem.Text = "Remove All Tables...";
			this.QueryClearMenuItem.Click += new System.EventHandler(this.QueryClearMenuItem_Click);
			// 
			// RenameQueryMenuItem
			// 
			this.RenameQueryMenuItem.Name = "RenameQueryMenuItem";
			this.RenameQueryMenuItem.Size = new System.Drawing.Size(184, 22);
			this.RenameQueryMenuItem.Text = "Rename...";
			this.RenameQueryMenuItem.Click += new System.EventHandler(this.RenameQueryMenuItem_Click);
			// 
			// QueriesControl
			// 
			this.Appearance.BackColor = System.Drawing.Color.OldLace;
			this.Appearance.Options.UseBackColor = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.Tabs);
			this.Name = "QueriesControl";
			this.Size = new System.Drawing.Size(749, 613);
			((System.ComponentModel.ISupportInitialize)(this.Tabs)).EndInit();
			this.Tabs.ResumeLayout(false);
			this.BrowseMode.ResumeLayout(false);
			this.BuildMode.ResumeLayout(false);
			this.QueryContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

				public DevExpress.XtraTab.XtraTabPage BuildMode;
				public DevExpress.XtraTab.XtraTabPage BrowseMode;
				public System.Windows.Forms.ImageList Bitmaps16x16;
				public QueryResultsControl QueryResultsControl;
				public DevExpress.XtraTab.XtraTabControl Tabs;
				public QbControl QueryBuilderControl;
				private System.ComponentModel.IContainer components;
		public System.Windows.Forms.ContextMenuStrip QueryContextMenu;
		public System.Windows.Forms.ToolStripMenuItem QuerySaveMenuItem;
		public System.Windows.Forms.ToolStripMenuItem QuerySaveAsMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator20;
		public System.Windows.Forms.ToolStripMenuItem QueryRunMenuItem;
		public System.Windows.Forms.ToolStripMenuItem QueryOptionsMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator21;
		private System.Windows.Forms.ToolStripMenuItem DuplicateMenuItem;
		public System.Windows.Forms.ToolStripMenuItem QueryRemoveAllCriteriaMenuItem;
		public System.Windows.Forms.ToolStripMenuItem QueryClearMenuItem;
		private System.Windows.Forms.ToolStripMenuItem RenameQueryMenuItem;
	}
}
