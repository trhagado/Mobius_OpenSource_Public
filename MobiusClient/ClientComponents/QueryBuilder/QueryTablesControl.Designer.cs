namespace Mobius.ClientComponents
{
	partial class QueryTablesControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QueryTablesControl));
			DevExpress.Utils.SuperToolTip superToolTip1 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipTitleItem toolTipTitleItem1 = new DevExpress.Utils.ToolTipTitleItem();
			DevExpress.Utils.ToolTipItem toolTipItem1 = new DevExpress.Utils.ToolTipItem();
			DevExpress.Utils.SuperToolTip superToolTip2 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipTitleItem toolTipTitleItem2 = new DevExpress.Utils.ToolTipTitleItem();
			DevExpress.Utils.ToolTipItem toolTipItem2 = new DevExpress.Utils.ToolTipItem();
			this.Bitmap16x16 = new System.Windows.Forms.ImageList(this.components);
			this.Bitmaps5x5 = new System.Windows.Forms.ImageList(this.components);
			this.RunQueryContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.RunQueryPreviewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RunQuerySingleTableMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RunQueryDetachedMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RunQueryBrowseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.OptionsContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.TableSumPosMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.AlertOptionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MiscOptionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.AdvancedOptionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.QueryIdMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.ShowOpenQueryURLMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ShowRunQueryUrlMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.ShowMqlMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ShowSqlMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ShowQueryXmlMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ShowMetaTableXmlMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.Tabs = new DevExpress.XtraTab.XtraTabControl();
			this.QbCriteriaTabPagePrototype = new DevExpress.XtraTab.XtraTabPage();
			this.CriteriaPanelPrototype = new Mobius.ClientComponents.CriteriaPanel();
			this.QbTableTabPagePrototype = new DevExpress.XtraTab.XtraTabPage();
			this.TableControlPrototype = new Mobius.ClientComponents.QueryTableControl();
			this.RunQueryButton = new DevExpress.XtraEditors.DropDownButton();
			this.OptionsButton = new DevExpress.XtraEditors.DropDownButton();
			this.SelectedDataButton = new DevExpress.XtraEditors.DropDownButton();
			this.RunQueryContextMenu.SuspendLayout();
			this.OptionsContextMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Tabs)).BeginInit();
			this.Tabs.SuspendLayout();
			this.QbCriteriaTabPagePrototype.SuspendLayout();
			this.QbTableTabPagePrototype.SuspendLayout();
			this.SuspendLayout();
			// 
			// Bitmap16x16
			// 
			this.Bitmap16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmap16x16.ImageStream")));
			this.Bitmap16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmap16x16.Images.SetKeyName(0, "Format.bmp");
			this.Bitmap16x16.Images.SetKeyName(1, "ScatterChart.bmp");
			this.Bitmap16x16.Images.SetKeyName(2, "Alert.bmp");
			this.Bitmap16x16.Images.SetKeyName(3, "History.bmp");
			// 
			// Bitmaps5x5
			// 
			this.Bitmaps5x5.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps5x5.ImageStream")));
			this.Bitmaps5x5.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps5x5.Images.SetKeyName(0, "");
			this.Bitmaps5x5.Images.SetKeyName(1, "");
			this.Bitmaps5x5.Images.SetKeyName(2, "");
			// 
			// RunQueryContextMenu
			// 
			this.RunQueryContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RunQueryPreviewMenuItem,
            this.RunQuerySingleTableMenuItem,
            this.RunQueryDetachedMenuItem,
            this.RunQueryBrowseMenuItem});
			this.RunQueryContextMenu.Name = "RunQueryPlusMenu";
			this.RunQueryContextMenu.Size = new System.Drawing.Size(306, 92);
			// 
			// RunQueryPreviewMenuItem
			// 
			this.RunQueryPreviewMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.TablePreviewSingle;
			this.RunQueryPreviewMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.RunQueryPreviewMenuItem.Name = "RunQueryPreviewMenuItem";
			this.RunQueryPreviewMenuItem.Size = new System.Drawing.Size(305, 22);
			this.RunQueryPreviewMenuItem.Text = "Preview all data for this table/assay";
			this.RunQueryPreviewMenuItem.Click += new System.EventHandler(this.RunQueryPreviewMenuItem_Click);
			// 
			// RunQuerySingleTableMenuItem
			// 
			this.RunQuerySingleTableMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.TableSingle;
			this.RunQuerySingleTableMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.RunQuerySingleTableMenuItem.Name = "RunQuerySingleTableMenuItem";
			this.RunQuerySingleTableMenuItem.Size = new System.Drawing.Size(305, 22);
			this.RunQuerySingleTableMenuItem.Text = "Run Query on this table/assay only";
			this.RunQuerySingleTableMenuItem.Click += new System.EventHandler(this.RunQuerySingleTableMenuItem_Click);
			// 
			// RunQueryDetachedMenuItem
			// 
			this.RunQueryDetachedMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Email;
			this.RunQueryDetachedMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.RunQueryDetachedMenuItem.Name = "RunQueryDetachedMenuItem";
			this.RunQueryDetachedMenuItem.Size = new System.Drawing.Size(305, 22);
			this.RunQueryDetachedMenuItem.Text = "Run query in background and E-mail results";
			this.RunQueryDetachedMenuItem.Click += new System.EventHandler(this.RunQueryDetachedMenuItem_Click);
			// 
			// RunQueryBrowseMenuItem
			// 
			this.RunQueryBrowseMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Undo;
			this.RunQueryBrowseMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.RunQueryBrowseMenuItem.Name = "RunQueryBrowseMenuItem";
			this.RunQueryBrowseMenuItem.Size = new System.Drawing.Size(305, 22);
			this.RunQueryBrowseMenuItem.Text = "View previous Run Query results";
			this.RunQueryBrowseMenuItem.Click += new System.EventHandler(this.RunQueryBrowseMenuItem_Click);
			// 
			// OptionsContextMenu
			// 
			this.OptionsContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TableSumPosMenuItem,
            this.AlertOptionsMenuItem,
            this.MiscOptionsMenuItem,
            this.AdvancedOptionsMenuItem,
            this.toolStripMenuItem1,
            this.QueryIdMenuItem,
            this.toolStripMenuItem2,
            this.ShowOpenQueryURLMenuItem,
            this.ShowRunQueryUrlMenuItem,
            this.toolStripSeparator1,
            this.ShowMqlMenuItem,
            this.ShowSqlMenuItem,
            this.ShowQueryXmlMenuItem,
            this.ShowMetaTableXmlMenuItem});
			this.OptionsContextMenu.Name = "RunQueryPlusMenu";
			this.OptionsContextMenu.Size = new System.Drawing.Size(308, 264);
			this.OptionsContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.OptionsContextMenu_Opening);
			// 
			// TableSumPosMenuItem
			// 
			this.TableSumPosMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("TableSumPosMenuItem.Image")));
			this.TableSumPosMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.TableSumPosMenuItem.Name = "TableSumPosMenuItem";
			this.TableSumPosMenuItem.Size = new System.Drawing.Size(307, 22);
			this.TableSumPosMenuItem.Text = "Table Summarization and Position Options...";
			this.TableSumPosMenuItem.Click += new System.EventHandler(this.TableSumPosMenuItem_Click);
			// 
			// AlertOptionsMenuItem
			// 
			this.AlertOptionsMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("AlertOptionsMenuItem.Image")));
			this.AlertOptionsMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.AlertOptionsMenuItem.Name = "AlertOptionsMenuItem";
			this.AlertOptionsMenuItem.Size = new System.Drawing.Size(307, 22);
			this.AlertOptionsMenuItem.Text = "Alert Options...";
			this.AlertOptionsMenuItem.Click += new System.EventHandler(this.AlertOptionsMenuItem_Click);
			// 
			// MiscOptionsMenuItem
			// 
			this.MiscOptionsMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("MiscOptionsMenuItem.Image")));
			this.MiscOptionsMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.MiscOptionsMenuItem.Name = "MiscOptionsMenuItem";
			this.MiscOptionsMenuItem.Size = new System.Drawing.Size(307, 22);
			this.MiscOptionsMenuItem.Text = "Miscellaneous Common Options...";
			this.MiscOptionsMenuItem.Click += new System.EventHandler(this.MiscOptionsMenuItem_Click);
			// 
			// AdvancedOptionsMenuItem
			// 
			this.AdvancedOptionsMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("AdvancedOptionsMenuItem.Image")));
			this.AdvancedOptionsMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.AdvancedOptionsMenuItem.Name = "AdvancedOptionsMenuItem";
			this.AdvancedOptionsMenuItem.Size = new System.Drawing.Size(307, 22);
			this.AdvancedOptionsMenuItem.Text = "Advanced Options...";
			this.AdvancedOptionsMenuItem.Click += new System.EventHandler(this.AdvancedOptionsMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(304, 6);
			// 
			// QueryIdMenuItem
			// 
			this.QueryIdMenuItem.Name = "QueryIdMenuItem";
			this.QueryIdMenuItem.Size = new System.Drawing.Size(307, 22);
			this.QueryIdMenuItem.Text = "=== Query Id: 12345 ===";
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(304, 6);
			// 
			// ShowOpenQueryURLMenuItem
			// 
			this.ShowOpenQueryURLMenuItem.Name = "ShowOpenQueryURLMenuItem";
			this.ShowOpenQueryURLMenuItem.Size = new System.Drawing.Size(307, 22);
			this.ShowOpenQueryURLMenuItem.Text = "URL to Open this Query";
			this.ShowOpenQueryURLMenuItem.Click += new System.EventHandler(this.ShowOpenQueryURLMenuItem_Click);
			// 
			// ShowRunQueryUrlMenuItem
			// 
			this.ShowRunQueryUrlMenuItem.Name = "ShowRunQueryUrlMenuItem";
			this.ShowRunQueryUrlMenuItem.Size = new System.Drawing.Size(307, 22);
			this.ShowRunQueryUrlMenuItem.Text = "URL to Run this Query";
			this.ShowRunQueryUrlMenuItem.Click += new System.EventHandler(this.ShowRunQueryUrlMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(304, 6);
			// 
			// ShowMqlMenuItem
			// 
			this.ShowMqlMenuItem.Name = "ShowMqlMenuItem";
			this.ShowMqlMenuItem.Size = new System.Drawing.Size(307, 22);
			this.ShowMqlMenuItem.Text = "Show MQL for Query";
			this.ShowMqlMenuItem.Click += new System.EventHandler(this.ShowMqlMenuItem_Click);
			// 
			// ShowSqlMenuItem
			// 
			this.ShowSqlMenuItem.Name = "ShowSqlMenuItem";
			this.ShowSqlMenuItem.Size = new System.Drawing.Size(307, 22);
			this.ShowSqlMenuItem.Text = "Show SQL for Query";
			this.ShowSqlMenuItem.Click += new System.EventHandler(this.ShowSqlMenuItem_Click);
			// 
			// ShowQueryXmlMenuItem
			// 
			this.ShowQueryXmlMenuItem.Name = "ShowQueryXmlMenuItem";
			this.ShowQueryXmlMenuItem.Size = new System.Drawing.Size(307, 22);
			this.ShowQueryXmlMenuItem.Text = "Show Query Xml";
			this.ShowQueryXmlMenuItem.Click += new System.EventHandler(this.ShowQueryXmlMenuItem_Click);
			// 
			// ShowMetaTableXmlMenuItem
			// 
			this.ShowMetaTableXmlMenuItem.Name = "ShowMetaTableXmlMenuItem";
			this.ShowMetaTableXmlMenuItem.Size = new System.Drawing.Size(307, 22);
			this.ShowMetaTableXmlMenuItem.Text = "Show Current MetaTable Xml";
			this.ShowMetaTableXmlMenuItem.Click += new System.EventHandler(this.ShowMetaTableXmlMenuItem_Click);
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
			this.Tabs.Location = new System.Drawing.Point(0, 28);
			this.Tabs.LookAndFeel.UseWindowsXPTheme = true;
			this.Tabs.MultiLine = DevExpress.Utils.DefaultBoolean.False;
			this.Tabs.Name = "Tabs";
			this.Tabs.SelectedTabPage = this.QbCriteriaTabPagePrototype;
			this.Tabs.Size = new System.Drawing.Size(543, 522);
			this.Tabs.TabIndex = 181;
			this.Tabs.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.QbCriteriaTabPagePrototype,
            this.QbTableTabPagePrototype});
			this.Tabs.SelectedPageChanged += new DevExpress.XtraTab.TabPageChangedEventHandler(this.QbTabs_SelectedPageChanged);
			this.Tabs.CloseButtonClick += new System.EventHandler(this.Tabs_CloseButtonClick);
			this.Tabs.DragOver += new System.Windows.Forms.DragEventHandler(this.Tabs_DragOver);
			this.Tabs.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Tabs_MouseDown);
			this.Tabs.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Tabs_MouseMove);
			this.Tabs.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Tabs_MouseUp);
			// 
			// QbCriteriaTabPagePrototype
			// 
			this.QbCriteriaTabPagePrototype.Controls.Add(this.CriteriaPanelPrototype);
			this.QbCriteriaTabPagePrototype.Name = "QbCriteriaTabPagePrototype";
			this.QbCriteriaTabPagePrototype.Size = new System.Drawing.Size(541, 497);
			this.QbCriteriaTabPagePrototype.Text = "QbCriteriaTab";
			// 
			// CriteriaPanelPrototype
			// 
			this.CriteriaPanelPrototype.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CriteriaPanelPrototype.Location = new System.Drawing.Point(0, 0);
			this.CriteriaPanelPrototype.Name = "CriteriaPanelPrototype";
			this.CriteriaPanelPrototype.Size = new System.Drawing.Size(541, 497);
			this.CriteriaPanelPrototype.TabIndex = 0;
			// 
			// QbTableTabPagePrototype
			// 
			this.QbTableTabPagePrototype.Controls.Add(this.TableControlPrototype);
			this.QbTableTabPagePrototype.Name = "QbTableTabPagePrototype";
			this.QbTableTabPagePrototype.ShowCloseButton = DevExpress.Utils.DefaultBoolean.True;
			this.QbTableTabPagePrototype.Size = new System.Drawing.Size(541, 497);
			this.QbTableTabPagePrototype.Text = "QbTableTab";
			// 
			// TableControlPrototype
			// 
			this.TableControlPrototype.CanDeselectKeyCol = false;
			this.TableControlPrototype.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableControlPrototype.Location = new System.Drawing.Point(0, 0);
			this.TableControlPrototype.Name = "TableControlPrototype";
			this.TableControlPrototype.QueryTable = null;
			this.TableControlPrototype.SelectOnly = false;
			this.TableControlPrototype.SelectSingle = false;
			this.TableControlPrototype.Size = new System.Drawing.Size(541, 497);
			this.TableControlPrototype.TabIndex = 0;
			// 
			// RunQueryButton
			// 
			this.RunQueryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RunQueryButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("RunQueryButton.ImageOptions.Image")));
			this.RunQueryButton.ImageOptions.ImageIndex = 0;
			this.RunQueryButton.Location = new System.Drawing.Point(374, 2);
			this.RunQueryButton.Name = "RunQueryButton";
			this.RunQueryButton.Size = new System.Drawing.Size(93, 22);
			toolTipTitleItem1.Text = "Run Query";
			toolTipItem1.LeftIndent = 6;
			toolTipItem1.Text = "Clicking this button executes the current query. You can use the small dropdown b" +
    "utton on the right to perform specialized Run Query operations.";
			superToolTip1.Items.Add(toolTipTitleItem1);
			superToolTip1.Items.Add(toolTipItem1);
			this.RunQueryButton.SuperTip = superToolTip1;
			this.RunQueryButton.TabIndex = 1;
			this.RunQueryButton.Text = "Run Query";
			this.RunQueryButton.ArrowButtonClick += new System.EventHandler(this.RunQueryContextMenuButton_Click);
			this.RunQueryButton.Click += new System.EventHandler(this.RunQueryButton_Click);
			// 
			// OptionsButton
			// 
			this.OptionsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OptionsButton.Location = new System.Drawing.Point(474, 2);
			this.OptionsButton.Name = "OptionsButton";
			this.OptionsButton.Size = new System.Drawing.Size(67, 22);
			toolTipTitleItem2.Text = "Query Options";
			toolTipItem2.LeftIndent = 6;
			toolTipItem2.Text = "Define the order and summarization level of the tables/assays in the query, alert" +
    "s and miscellaneous and advanced query options.";
			superToolTip2.Items.Add(toolTipTitleItem2);
			superToolTip2.Items.Add(toolTipItem2);
			this.OptionsButton.SuperTip = superToolTip2;
			this.OptionsButton.TabIndex = 183;
			this.OptionsButton.Text = "Options";
			this.OptionsButton.ArrowButtonClick += new System.EventHandler(this.OptionsButton_ArrowButtonClick);
			this.OptionsButton.Click += new System.EventHandler(this.OptionsButton_Click);
			// 
			// SelectedDataButton
			// 
			this.SelectedDataButton.Appearance.Font = new System.Drawing.Font("Arial", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SelectedDataButton.Appearance.ForeColor = System.Drawing.Color.Black;
			this.SelectedDataButton.Appearance.Options.UseFont = true;
			this.SelectedDataButton.Appearance.Options.UseForeColor = true;
			this.SelectedDataButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.SelectedDataButton.ImageOptions.ImageIndex = 0;
			this.SelectedDataButton.Location = new System.Drawing.Point(2, 3);
			this.SelectedDataButton.Name = "SelectedDataButton";
			this.SelectedDataButton.Size = new System.Drawing.Size(124, 22);
			this.SelectedDataButton.TabIndex = 184;
			this.SelectedDataButton.Text = "Selected Data";
			this.SelectedDataButton.ArrowButtonClick += new System.EventHandler(this.SelectedDataButton_ArrowButtonClick);
			this.SelectedDataButton.Click += new System.EventHandler(this.SelectedDataButton_Click);
			// 
			// QueryTablesControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.SelectedDataButton);
			this.Controls.Add(this.OptionsButton);
			this.Controls.Add(this.Tabs);
			this.Controls.Add(this.RunQueryButton);
			this.Name = "QueryTablesControl";
			this.Size = new System.Drawing.Size(544, 550);
			this.RunQueryContextMenu.ResumeLayout(false);
			this.OptionsContextMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Tabs)).EndInit();
			this.Tabs.ResumeLayout(false);
			this.QbCriteriaTabPagePrototype.ResumeLayout(false);
			this.QbTableTabPagePrototype.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.ImageList Bitmaps5x5;
		public System.Windows.Forms.ImageList Bitmap16x16;
		public System.Windows.Forms.ContextMenuStrip RunQueryContextMenu;
		public System.Windows.Forms.ToolStripMenuItem RunQueryPreviewMenuItem;
		public System.Windows.Forms.ToolStripMenuItem RunQuerySingleTableMenuItem;
		private System.Windows.Forms.ToolStripMenuItem RunQueryDetachedMenuItem;
		private System.Windows.Forms.ToolStripMenuItem RunQueryBrowseMenuItem;
		public System.Windows.Forms.ContextMenuStrip OptionsContextMenu;
		public System.Windows.Forms.ToolStripMenuItem TableSumPosMenuItem;
		private System.Windows.Forms.ToolStripMenuItem AlertOptionsMenuItem;
		private System.Windows.Forms.ToolStripMenuItem MiscOptionsMenuItem;
		private System.Windows.Forms.ToolStripMenuItem AdvancedOptionsMenuItem;
		internal DevExpress.XtraTab.XtraTabPage QbCriteriaTabPagePrototype;
		internal DevExpress.XtraTab.XtraTabPage QbTableTabPagePrototype;
		private DevExpress.XtraEditors.DropDownButton RunQueryButton;
		public DevExpress.XtraTab.XtraTabControl Tabs;
		internal QueryTableControl TableControlPrototype;
		private CriteriaPanel CriteriaPanelPrototype;
		private DevExpress.XtraEditors.DropDownButton OptionsButton;
		private DevExpress.XtraEditors.DropDownButton SelectedDataButton;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem ShowOpenQueryURLMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ShowRunQueryUrlMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ShowMqlMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ShowSqlMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ShowQueryXmlMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ShowMetaTableXmlMenuItem;
		private System.Windows.Forms.ToolStripMenuItem QueryIdMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
	}
}
