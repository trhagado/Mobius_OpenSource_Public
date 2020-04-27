namespace Mobius.ClientComponents
{
	partial class CriteriaContentsTreeList
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CriteriaContentsTreeList));
			this.Prompt = new DevExpress.XtraEditors.LabelControl();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.ItemGrid = new DevExpress.XtraGrid.GridControl();
			this.ColGridView = new DevExpress.XtraGrid.Views.Grid.GridView();
			this.LabelColumn = new DevExpress.XtraGrid.Columns.GridColumn();
			this.InternalNameColumn = new DevExpress.XtraGrid.Columns.GridColumn();
			this.repositoryItemComboBox1 = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
			this.repositoryItemCheckEdit6 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
			this.DeleteRow = new DevExpress.XtraEditors.SimpleButton();
			this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
			this.ContentsTreeWithSearch = new Mobius.ClientComponents.ContentsTreeWithSearch();
			this.ContentsLabel = new DevExpress.XtraEditors.LabelControl();
			this.EditButton = new DevExpress.XtraEditors.SimpleButton();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.EditMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.CopyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.CopyAssayNamesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.CopyInternalNamesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.CopyAssayAndInternalNamesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.PasteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.DeleteRowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.DeleteAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.ItemGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ColGridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit6)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
			this.splitContainerControl1.SuspendLayout();
			this.EditMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// Prompt
			// 
			this.Prompt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Prompt.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.Prompt.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.Prompt.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.Prompt.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.Prompt.Location = new System.Drawing.Point(5, 4);
			this.Prompt.Name = "Prompt";
			this.Prompt.Size = new System.Drawing.Size(999, 40);
			this.Prompt.TabIndex = 45;
			this.Prompt.Text = "Use the Contents Tree and QuickSearch line to select items to be added to this li" +
    "st.";
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(878, 705);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(60, 22);
			this.OK.TabIndex = 43;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// Cancel
			// 
			this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Cancel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Cancel.Appearance.Options.UseFont = true;
			this.Cancel.Appearance.Options.UseForeColor = true;
			this.Cancel.Cursor = System.Windows.Forms.Cursors.Default;
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = new System.Drawing.Point(944, 705);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(60, 22);
			this.Cancel.TabIndex = 42;
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// ItemGrid
			// 
			this.ItemGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ItemGrid.Cursor = System.Windows.Forms.Cursors.Default;
			this.ItemGrid.Location = new System.Drawing.Point(0, 28);
			this.ItemGrid.MainView = this.ColGridView;
			this.ItemGrid.Name = "ItemGrid";
			this.ItemGrid.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemComboBox1,
            this.repositoryItemCheckEdit6});
			this.ItemGrid.Size = new System.Drawing.Size(553, 637);
			this.ItemGrid.TabIndex = 209;
			this.ItemGrid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.ColGridView});
			// 
			// ColGridView
			// 
			this.ColGridView.Appearance.HeaderPanel.Options.UseTextOptions = true;
			this.ColGridView.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.ColGridView.Appearance.HeaderPanel.TextOptions.Trimming = DevExpress.Utils.Trimming.None;
			this.ColGridView.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.ColGridView.ColumnPanelRowHeight = 34;
			this.ColGridView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.LabelColumn,
            this.InternalNameColumn});
			this.ColGridView.GridControl = this.ItemGrid;
			this.ColGridView.IndicatorWidth = 26;
			this.ColGridView.Name = "ColGridView";
			this.ColGridView.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.True;
			this.ColGridView.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.True;
			this.ColGridView.OptionsCustomization.AllowColumnMoving = false;
			this.ColGridView.OptionsCustomization.AllowFilter = false;
			this.ColGridView.OptionsCustomization.AllowGroup = false;
			this.ColGridView.OptionsCustomization.AllowRowSizing = true;
			this.ColGridView.OptionsMenu.EnableColumnMenu = false;
			this.ColGridView.OptionsMenu.EnableFooterMenu = false;
			this.ColGridView.OptionsMenu.EnableGroupPanelMenu = false;
			this.ColGridView.OptionsMenu.ShowDateTimeGroupIntervalItems = false;
			this.ColGridView.OptionsMenu.ShowGroupSortSummaryItems = false;
			this.ColGridView.OptionsSelection.MultiSelect = true;
			this.ColGridView.OptionsView.ColumnAutoWidth = false;
			this.ColGridView.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Bottom;
			this.ColGridView.OptionsView.RowAutoHeight = true;
			this.ColGridView.OptionsView.ShowGroupPanel = false;
			// 
			// LabelColumn
			// 
			this.LabelColumn.AppearanceHeader.Options.UseTextOptions = true;
			this.LabelColumn.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.LabelColumn.AppearanceHeader.TextOptions.Trimming = DevExpress.Utils.Trimming.None;
			this.LabelColumn.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.LabelColumn.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.LabelColumn.Caption = "Assay Name";
			this.LabelColumn.FieldName = "LabelColumn";
			this.LabelColumn.Name = "LabelColumn";
			this.LabelColumn.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
			this.LabelColumn.OptionsFilter.AllowFilter = false;
			this.LabelColumn.Visible = true;
			this.LabelColumn.VisibleIndex = 0;
			this.LabelColumn.Width = 426;
			// 
			// InternalNameColumn
			// 
			this.InternalNameColumn.AppearanceHeader.Options.UseTextOptions = true;
			this.InternalNameColumn.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.InternalNameColumn.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.InternalNameColumn.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.InternalNameColumn.Caption = "Internal Name";
			this.InternalNameColumn.FieldName = "InternalNameColumn";
			this.InternalNameColumn.Name = "InternalNameColumn";
			this.InternalNameColumn.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
			this.InternalNameColumn.OptionsFilter.AllowFilter = false;
			this.InternalNameColumn.Visible = true;
			this.InternalNameColumn.VisibleIndex = 1;
			this.InternalNameColumn.Width = 96;
			// 
			// repositoryItemComboBox1
			// 
			this.repositoryItemComboBox1.AutoHeight = false;
			this.repositoryItemComboBox1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.repositoryItemComboBox1.Items.AddRange(new object[] {
            "Number",
            "Text",
            "Date",
            "Compound Identifier",
            "Structure"});
			this.repositoryItemComboBox1.Name = "repositoryItemComboBox1";
			this.repositoryItemComboBox1.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
			// 
			// repositoryItemCheckEdit6
			// 
			this.repositoryItemCheckEdit6.AutoHeight = false;
			this.repositoryItemCheckEdit6.Caption = "Check";
			this.repositoryItemCheckEdit6.Name = "repositoryItemCheckEdit6";
			// 
			// DeleteRow
			// 
			this.DeleteRow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DeleteRow.Appearance.Options.UseTextOptions = true;
			this.DeleteRow.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.DeleteRow.ImageIndex = 3;
			this.DeleteRow.Location = new System.Drawing.Point(451, 705);
			this.DeleteRow.Name = "DeleteRow";
			this.DeleteRow.Size = new System.Drawing.Size(70, 22);
			this.DeleteRow.TabIndex = 210;
			this.DeleteRow.Text = "Delete Row";
			this.DeleteRow.Click += new System.EventHandler(this.DeleteRow_Click);
			// 
			// splitContainerControl1
			// 
			this.splitContainerControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainerControl1.Location = new System.Drawing.Point(5, 34);
			this.splitContainerControl1.Name = "splitContainerControl1";
			this.splitContainerControl1.Panel1.Controls.Add(this.ContentsTreeWithSearch);
			this.splitContainerControl1.Panel1.Text = "Panel1";
			this.splitContainerControl1.Panel2.Controls.Add(this.ContentsLabel);
			this.splitContainerControl1.Panel2.Controls.Add(this.ItemGrid);
			this.splitContainerControl1.Panel2.Text = "Panel2";
			this.splitContainerControl1.Size = new System.Drawing.Size(999, 665);
			this.splitContainerControl1.SplitterPosition = 441;
			this.splitContainerControl1.TabIndex = 211;
			this.splitContainerControl1.Text = "splitContainerControl1";
			// 
			// ContentsTreeWithSearch
			// 
			this.ContentsTreeWithSearch.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ContentsTreeWithSearch.Appearance.Options.UseBackColor = true;
			this.ContentsTreeWithSearch.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContentsTreeWithSearch.Location = new System.Drawing.Point(0, 0);
			this.ContentsTreeWithSearch.Name = "ContentsTreeWithSearch";
			this.ContentsTreeWithSearch.Size = new System.Drawing.Size(441, 665);
			this.ContentsTreeWithSearch.TabIndex = 0;
			// 
			// ContentsLabel
			// 
			this.ContentsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ContentsLabel.Appearance.Font = new System.Drawing.Font("Arial", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
			this.ContentsLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.ContentsLabel.LineVisible = true;
			this.ContentsLabel.Location = new System.Drawing.Point(4, 5);
			this.ContentsLabel.Name = "ContentsLabel";
			this.ContentsLabel.Size = new System.Drawing.Size(549, 17);
			this.ContentsLabel.TabIndex = 210;
			this.ContentsLabel.Text = "Selected Assays";
			// 
			// EditButton
			// 
			this.EditButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.EditButton.ImageIndex = 2;
			this.EditButton.ImageList = this.Bitmaps16x16;
			this.EditButton.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleRight;
			this.EditButton.Location = new System.Drawing.Point(527, 705);
			this.EditButton.Name = "EditButton";
			this.EditButton.Size = new System.Drawing.Size(56, 22);
			this.EditButton.TabIndex = 214;
			this.EditButton.TabStop = false;
			this.EditButton.Text = "Edit";
			this.EditButton.Click += new System.EventHandler(this.EditButton_Click);
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "AddRule.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "EditRule.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "ScrollDown16x16.bmp");
			this.Bitmaps16x16.Images.SetKeyName(3, "Delete.bmp");
			this.Bitmaps16x16.Images.SetKeyName(4, "up2.bmp");
			this.Bitmaps16x16.Images.SetKeyName(5, "down2.bmp");
			this.Bitmaps16x16.Images.SetKeyName(6, "Copy.bmp");
			this.Bitmaps16x16.Images.SetKeyName(7, "Paste.bmp");
			// 
			// EditMenu
			// 
			this.EditMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CopyMenuItem,
            this.PasteMenuItem,
            this.DeleteRowMenuItem,
            this.toolStripMenuItem1,
            this.DeleteAllMenuItem});
			this.EditMenu.Name = "EditMenuStrip";
			this.EditMenu.Size = new System.Drawing.Size(165, 120);
			// 
			// CopyMenuItem
			// 
			this.CopyMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CopyAssayNamesMenuItem,
            this.CopyInternalNamesMenuItem,
            this.CopyAssayAndInternalNamesMenuItem});
			this.CopyMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Copy;
			this.CopyMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.CopyMenuItem.Name = "CopyMenuItem";
			this.CopyMenuItem.Size = new System.Drawing.Size(164, 22);
			this.CopyMenuItem.Text = "Copy";
			// 
			// CopyAssayNamesMenuItem
			// 
			this.CopyAssayNamesMenuItem.Name = "CopyAssayNamesMenuItem";
			this.CopyAssayNamesMenuItem.Size = new System.Drawing.Size(210, 22);
			this.CopyAssayNamesMenuItem.Text = "Assay Names";
			this.CopyAssayNamesMenuItem.Click += new System.EventHandler(this.CopyAssayNamesMenuItem_Click);
			// 
			// CopyInternalNamesMenuItem
			// 
			this.CopyInternalNamesMenuItem.Name = "CopyInternalNamesMenuItem";
			this.CopyInternalNamesMenuItem.Size = new System.Drawing.Size(210, 22);
			this.CopyInternalNamesMenuItem.Text = "Internal Names";
			this.CopyInternalNamesMenuItem.Click += new System.EventHandler(this.CopyInternalNamesMenuItem_Click);
			// 
			// CopyAssayAndInternalNamesMenuItem
			// 
			this.CopyAssayAndInternalNamesMenuItem.Name = "CopyAssayAndInternalNamesMenuItem";
			this.CopyAssayAndInternalNamesMenuItem.Size = new System.Drawing.Size(210, 22);
			this.CopyAssayAndInternalNamesMenuItem.Text = "Assay and Internal Names";
			this.CopyAssayAndInternalNamesMenuItem.Click += new System.EventHandler(this.CopyAssayAndInternalNamesMenuItem_Click);
			// 
			// PasteMenuItem
			// 
			this.PasteMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Paste;
			this.PasteMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.PasteMenuItem.Name = "PasteMenuItem";
			this.PasteMenuItem.Size = new System.Drawing.Size(164, 22);
			this.PasteMenuItem.Text = "&Paste";
			this.PasteMenuItem.Click += new System.EventHandler(this.PasteMenuItem_Click);
			// 
			// DeleteRowMenuItem
			// 
			this.DeleteRowMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Delete;
			this.DeleteRowMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.DeleteRowMenuItem.Name = "DeleteRowMenuItem";
			this.DeleteRowMenuItem.Size = new System.Drawing.Size(164, 22);
			this.DeleteRowMenuItem.Text = "Delete Row";
			this.DeleteRowMenuItem.Click += new System.EventHandler(this.DeleteRowMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(161, 6);
			// 
			// DeleteAllMenuItem
			// 
			this.DeleteAllMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.DeleteAllMenuItem.Name = "DeleteAllMenuItem";
			this.DeleteAllMenuItem.Size = new System.Drawing.Size(164, 22);
			this.DeleteAllMenuItem.Text = "Delete All Rows...";
			this.DeleteAllMenuItem.Click += new System.EventHandler(this.DeleteAllMenuItem_Click);
			// 
			// CriteriaContentsTreeList
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(1008, 730);
			this.Controls.Add(this.EditButton);
			this.Controls.Add(this.splitContainerControl1);
			this.Controls.Add(this.DeleteRow);
			this.Controls.Add(this.Prompt);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.MinimizeBox = false;
			this.Name = "CriteriaContentsTreeList";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Criteria List";
			this.Activated += new System.EventHandler(this.CriteriaContentsTreeList_Activated);
			this.Shown += new System.EventHandler(this.CriteriaContentsTreeList_Shown);
			this.VisibleChanged += new System.EventHandler(this.CriteriaContentsTreeList_VisibleChanged);
			this.Resize += new System.EventHandler(this.CriteriaContentsTreeList_Resize);
			((System.ComponentModel.ISupportInitialize)(this.ItemGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ColGridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit6)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
			this.splitContainerControl1.ResumeLayout(false);
			this.EditMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.LabelControl Prompt;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraGrid.GridControl ItemGrid;
		private DevExpress.XtraGrid.Views.Grid.GridView ColGridView;
		private DevExpress.XtraGrid.Columns.GridColumn LabelColumn;
		private DevExpress.XtraGrid.Columns.GridColumn InternalNameColumn;
		private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox1;
		private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repositoryItemCheckEdit6;
		public DevExpress.XtraEditors.SimpleButton DeleteRow;
		private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
		private ContentsTreeWithSearch ContentsTreeWithSearch;
		private DevExpress.XtraEditors.LabelControl ContentsLabel;
		public DevExpress.XtraEditors.SimpleButton EditButton;
		private System.Windows.Forms.ContextMenuStrip EditMenu;
		private System.Windows.Forms.ToolStripMenuItem CopyMenuItem;
		private System.Windows.Forms.ToolStripMenuItem PasteMenuItem;
		private System.Windows.Forms.ToolStripMenuItem DeleteRowMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		public System.Windows.Forms.ImageList Bitmaps16x16;
		private System.Windows.Forms.ToolStripMenuItem DeleteAllMenuItem;
		private System.Windows.Forms.ToolStripMenuItem CopyAssayNamesMenuItem;
		private System.Windows.Forms.ToolStripMenuItem CopyInternalNamesMenuItem;
		private System.Windows.Forms.ToolStripMenuItem CopyAssayAndInternalNamesMenuItem;
	}
}