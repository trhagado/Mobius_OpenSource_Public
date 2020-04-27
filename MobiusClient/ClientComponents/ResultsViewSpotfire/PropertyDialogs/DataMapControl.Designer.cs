namespace Mobius.SpotfireClient
{
	partial class DataMapControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataMapControl));
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.SourceQueryMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.CurrentQueryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MruListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.FavoritesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SelectFromContentsTreeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.HeaderPanel = new DevExpress.XtraEditors.PanelControl();
			this.LeftArrowHead = new DevExpress.XtraEditors.PictureEdit();
			this.RightArrowHead = new DevExpress.XtraEditors.PictureEdit();
			this.Arrow = new DevExpress.XtraEditors.LabelControl();
			this.SpotfireToMobiusArrowButton = new DevExpress.XtraEditors.SimpleButton();
			this.QtSelectorControl = new Mobius.SpotfireClient.QueryTableSelectorControl();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.MapOptionsButton = new DevExpress.XtraEditors.SimpleButton();
			this.DataTableSelectorControl = new Mobius.SpotfireClient.DataMapsTableSelectorControl();
			this.GridPromptLabel = new DevExpress.XtraEditors.LabelControl();
			this.SpotfireDataTableControlLabel = new DevExpress.XtraEditors.LabelControl();
			this.RepositoryItemCheckEdit = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
			this.repositoryItemTextEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
			this.DataTypeImageRepositoryItemPictureEdit = new DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit();
			this.GridView = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridView();
			this.DataTypeBand = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
			this.DataTypeImageCol = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
			this.SpotfireBand = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
			this.OriginalSpotfireColNameCol = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
			this.SpotfireColNameCol = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
			this.SelectedBand = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
			this.FieldSelectedCol = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
			this.MobiusBand = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
			this.MobiusTableNameCol = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
			this.MobiusColNameCol = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
			this.FieldGrid = new DevExpress.XtraGrid.GridControl();
			this.SourceQueryMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HeaderPanel)).BeginInit();
			this.HeaderPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LeftArrowHead.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RightArrowHead.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RepositoryItemCheckEdit)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DataTypeImageRepositoryItemPictureEdit)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FieldGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "Add.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "Edit.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "ScrollDown16x16.bmp");
			this.Bitmaps16x16.Images.SetKeyName(3, "Delete.bmp");
			this.Bitmaps16x16.Images.SetKeyName(4, "up2.bmp");
			this.Bitmaps16x16.Images.SetKeyName(5, "down2.bmp");
			this.Bitmaps16x16.Images.SetKeyName(6, "Copy.bmp");
			this.Bitmaps16x16.Images.SetKeyName(7, "Paste.bmp");
			this.Bitmaps16x16.Images.SetKeyName(8, "Properties.bmp");
			this.Bitmaps16x16.Images.SetKeyName(9, "Sum.bmp");
			// 
			// SourceQueryMenu
			// 
			this.SourceQueryMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CurrentQueryMenuItem,
            this.MruListMenuItem,
            this.FavoritesMenuItem,
            this.SelectFromContentsTreeMenuItem});
			this.SourceQueryMenu.Name = "SourceQueryMenu";
			this.SourceQueryMenu.Size = new System.Drawing.Size(272, 92);
			// 
			// CurrentQueryMenuItem
			// 
			this.CurrentQueryMenuItem.Name = "CurrentQueryMenuItem";
			this.CurrentQueryMenuItem.Size = new System.Drawing.Size(271, 22);
			this.CurrentQueryMenuItem.Text = "The Current Query";
			// 
			// MruListMenuItem
			// 
			this.MruListMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("MruListMenuItem.Image")));
			this.MruListMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.MruListMenuItem.Name = "MruListMenuItem";
			this.MruListMenuItem.Size = new System.Drawing.Size(271, 22);
			this.MruListMenuItem.Text = "Recent Queries";
			// 
			// FavoritesMenuItem
			// 
			this.FavoritesMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("FavoritesMenuItem.Image")));
			this.FavoritesMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.FavoritesMenuItem.Name = "FavoritesMenuItem";
			this.FavoritesMenuItem.Size = new System.Drawing.Size(271, 22);
			this.FavoritesMenuItem.Text = "Favorites";
			// 
			// SelectFromContentsTreeMenuItem
			// 
			this.SelectFromContentsTreeMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("SelectFromContentsTreeMenuItem.Image")));
			this.SelectFromContentsTreeMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.SelectFromContentsTreeMenuItem.Name = "SelectFromContentsTreeMenuItem";
			this.SelectFromContentsTreeMenuItem.Size = new System.Drawing.Size(271, 22);
			this.SelectFromContentsTreeMenuItem.Text = "Select from Database Contents Tree...";
			// 
			// HeaderPanel
			// 
			this.HeaderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.HeaderPanel.Controls.Add(this.LeftArrowHead);
			this.HeaderPanel.Controls.Add(this.RightArrowHead);
			this.HeaderPanel.Controls.Add(this.Arrow);
			this.HeaderPanel.Controls.Add(this.SpotfireToMobiusArrowButton);
			this.HeaderPanel.Controls.Add(this.QtSelectorControl);
			this.HeaderPanel.Controls.Add(this.labelControl1);
			this.HeaderPanel.Controls.Add(this.MapOptionsButton);
			this.HeaderPanel.Controls.Add(this.DataTableSelectorControl);
			this.HeaderPanel.Controls.Add(this.GridPromptLabel);
			this.HeaderPanel.Controls.Add(this.SpotfireDataTableControlLabel);
			this.HeaderPanel.Location = new System.Drawing.Point(0, 0);
			this.HeaderPanel.Name = "HeaderPanel";
			this.HeaderPanel.Size = new System.Drawing.Size(763, 81);
			this.HeaderPanel.TabIndex = 225;
			// 
			// LeftArrowHead
			// 
			this.LeftArrowHead.EditValue = ((object)(resources.GetObject("LeftArrowHead.EditValue")));
			this.LeftArrowHead.Location = new System.Drawing.Point(108, 13);
			this.LeftArrowHead.Margin = new System.Windows.Forms.Padding(0);
			this.LeftArrowHead.Name = "LeftArrowHead";
			this.LeftArrowHead.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.LeftArrowHead.Properties.Appearance.Options.UseBackColor = true;
			this.LeftArrowHead.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.LeftArrowHead.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
			this.LeftArrowHead.Size = new System.Drawing.Size(6, 10);
			this.LeftArrowHead.TabIndex = 240;
			// 
			// RightArrowHead
			// 
			this.RightArrowHead.EditValue = ((object)(resources.GetObject("RightArrowHead.EditValue")));
			this.RightArrowHead.Location = new System.Drawing.Point(286, 13);
			this.RightArrowHead.Margin = new System.Windows.Forms.Padding(0);
			this.RightArrowHead.Name = "RightArrowHead";
			this.RightArrowHead.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.RightArrowHead.Properties.Appearance.Options.UseBackColor = true;
			this.RightArrowHead.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.RightArrowHead.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
			this.RightArrowHead.Size = new System.Drawing.Size(6, 10);
			this.RightArrowHead.TabIndex = 239;
			// 
			// Arrow
			// 
			this.Arrow.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Arrow.Appearance.Options.UseBackColor = true;
			this.Arrow.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.Arrow.LineColor = System.Drawing.Color.Black;
			this.Arrow.LineVisible = true;
			this.Arrow.Location = new System.Drawing.Point(108, 11);
			this.Arrow.Name = "Arrow";
			this.Arrow.Size = new System.Drawing.Size(180, 10);
			this.Arrow.TabIndex = 237;
			// 
			// SpotfireToMobiusArrowButton
			// 
			this.SpotfireToMobiusArrowButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.SpotfireToMobiusArrowButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("SpotfireToMobiusArrowButton.ImageOptions.Image")));
			this.SpotfireToMobiusArrowButton.Location = new System.Drawing.Point(270, 28);
			this.SpotfireToMobiusArrowButton.Name = "SpotfireToMobiusArrowButton";
			this.SpotfireToMobiusArrowButton.Size = new System.Drawing.Size(26, 21);
			this.SpotfireToMobiusArrowButton.TabIndex = 236;
			// 
			// QtSelectorControl
			// 
			this.QtSelectorControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.QtSelectorControl.Location = new System.Drawing.Point(302, 28);
			this.QtSelectorControl.Name = "QtSelectorControl";
			this.QtSelectorControl.Size = new System.Drawing.Size(456, 21);
			this.QtSelectorControl.TabIndex = 235;
			// 
			// labelControl1
			// 
			this.labelControl1.Location = new System.Drawing.Point(302, 9);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(79, 13);
			this.labelControl1.TabIndex = 229;
			this.labelControl1.Text = "Mobius Table(s):";
			// 
			// MapOptionsButton
			// 
			this.MapOptionsButton.ImageOptions.ImageIndex = 9;
			this.MapOptionsButton.ImageOptions.ImageList = this.Bitmaps16x16;
			this.MapOptionsButton.Location = new System.Drawing.Point(539, 54);
			this.MapOptionsButton.Name = "MapOptionsButton";
			this.MapOptionsButton.Size = new System.Drawing.Size(23, 20);
			this.MapOptionsButton.TabIndex = 228;
			this.MapOptionsButton.Visible = false;
			this.MapOptionsButton.Click += new System.EventHandler(this.MapOptionsButton_Click);
			// 
			// DataTableSelectorControl
			// 
			this.DataTableSelectorControl.Location = new System.Drawing.Point(8, 28);
			this.DataTableSelectorControl.Name = "DataTableSelectorControl";
			this.DataTableSelectorControl.Size = new System.Drawing.Size(260, 23);
			this.DataTableSelectorControl.TabIndex = 227;
			// 
			// GridPromptLabel
			// 
			this.GridPromptLabel.Location = new System.Drawing.Point(8, 57);
			this.GridPromptLabel.Name = "GridPromptLabel";
			this.GridPromptLabel.Size = new System.Drawing.Size(460, 13);
			this.GridPromptLabel.TabIndex = 225;
			this.GridPromptLabel.Text = "Select the Mobius column to assign to each Spotfire column. Rename Spotfire colum" +
    "s as desired.";
			// 
			// SpotfireDataTableControlLabel
			// 
			this.SpotfireDataTableControlLabel.Location = new System.Drawing.Point(8, 9);
			this.SpotfireDataTableControlLabel.Name = "SpotfireDataTableControlLabel";
			this.SpotfireDataTableControlLabel.Size = new System.Drawing.Size(94, 13);
			this.SpotfireDataTableControlLabel.TabIndex = 218;
			this.SpotfireDataTableControlLabel.Text = "Spotfire data table:";
			// 
			// RepositoryItemCheckEdit
			// 
			this.RepositoryItemCheckEdit.AutoHeight = false;
			this.RepositoryItemCheckEdit.Name = "RepositoryItemCheckEdit";
			// 
			// repositoryItemTextEdit1
			// 
			this.repositoryItemTextEdit1.Appearance.Options.UseTextOptions = true;
			this.repositoryItemTextEdit1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.repositoryItemTextEdit1.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.repositoryItemTextEdit1.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.repositoryItemTextEdit1.Name = "repositoryItemTextEdit1";
			// 
			// DataTypeImageRepositoryItemPictureEdit
			// 
			this.DataTypeImageRepositoryItemPictureEdit.Name = "DataTypeImageRepositoryItemPictureEdit";
			// 
			// GridView
			// 
			this.GridView.Appearance.HeaderPanel.Options.UseTextOptions = true;
			this.GridView.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.GridView.Appearance.HeaderPanel.TextOptions.Trimming = DevExpress.Utils.Trimming.None;
			this.GridView.Appearance.HeaderPanel.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.GridView.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.GridView.Appearance.Row.Options.UseTextOptions = true;
			this.GridView.Appearance.Row.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.GridView.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] {
            this.DataTypeBand,
            this.SpotfireBand,
            this.SelectedBand,
            this.MobiusBand});
			this.GridView.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] {
            this.DataTypeImageCol,
            this.OriginalSpotfireColNameCol,
            this.SpotfireColNameCol,
            this.FieldSelectedCol,
            this.MobiusTableNameCol,
            this.MobiusColNameCol});
			this.GridView.GridControl = this.FieldGrid;
			this.GridView.IndicatorWidth = 26;
			this.GridView.Name = "GridView";
			this.GridView.OptionsCustomization.AllowColumnMoving = false;
			this.GridView.OptionsCustomization.AllowFilter = false;
			this.GridView.OptionsCustomization.AllowGroup = false;
			this.GridView.OptionsCustomization.AllowRowSizing = true;
			this.GridView.OptionsCustomization.AllowSort = false;
			this.GridView.OptionsMenu.EnableColumnMenu = false;
			this.GridView.OptionsMenu.EnableFooterMenu = false;
			this.GridView.OptionsMenu.EnableGroupPanelMenu = false;
			this.GridView.OptionsSelection.MultiSelect = true;
			this.GridView.OptionsView.RowAutoHeight = true;
			this.GridView.OptionsView.ShowGroupPanel = false;
			this.GridView.ScrollStyle = DevExpress.XtraGrid.Views.Grid.ScrollStyleFlags.LiveVertScroll;
			this.GridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseDown);
			this.GridView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseUp);
			// 
			// DataTypeBand
			// 
			this.DataTypeBand.Caption = " ";
			this.DataTypeBand.Columns.Add(this.DataTypeImageCol);
			this.DataTypeBand.Name = "DataTypeBand";
			this.DataTypeBand.VisibleIndex = 0;
			this.DataTypeBand.Width = 20;
			// 
			// DataTypeImageCol
			// 
			this.DataTypeImageCol.Caption = " ";
			this.DataTypeImageCol.ColumnEdit = this.DataTypeImageRepositoryItemPictureEdit;
			this.DataTypeImageCol.FieldName = "DataTypeImageField";
			this.DataTypeImageCol.Name = "DataTypeImageCol";
			this.DataTypeImageCol.OptionsColumn.AllowEdit = false;
			this.DataTypeImageCol.Visible = true;
			this.DataTypeImageCol.Width = 20;
			// 
			// SpotfireBand
			// 
			this.SpotfireBand.AppearanceHeader.Options.UseTextOptions = true;
			this.SpotfireBand.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.SpotfireBand.Caption = "Spotfire Column";
			this.SpotfireBand.Columns.Add(this.OriginalSpotfireColNameCol);
			this.SpotfireBand.Columns.Add(this.SpotfireColNameCol);
			this.SpotfireBand.Name = "SpotfireBand";
			this.SpotfireBand.VisibleIndex = 1;
			this.SpotfireBand.Width = 220;
			// 
			// OriginalSpotfireColNameCol
			// 
			this.OriginalSpotfireColNameCol.Caption = "Role";
			this.OriginalSpotfireColNameCol.FieldName = "OriginalSpotfireColNameField";
			this.OriginalSpotfireColNameCol.MinWidth = 28;
			this.OriginalSpotfireColNameCol.Name = "OriginalSpotfireColNameCol";
			this.OriginalSpotfireColNameCol.OptionsColumn.AllowEdit = false;
			this.OriginalSpotfireColNameCol.Visible = true;
			this.OriginalSpotfireColNameCol.Width = 110;
			// 
			// SpotfireColNameCol
			// 
			this.SpotfireColNameCol.Caption = "Name";
			this.SpotfireColNameCol.ColumnEdit = this.repositoryItemTextEdit1;
			this.SpotfireColNameCol.FieldName = "SpotfireColNameField";
			this.SpotfireColNameCol.MinWidth = 28;
			this.SpotfireColNameCol.Name = "SpotfireColNameCol";
			this.SpotfireColNameCol.OptionsColumn.AllowEdit = false;
			this.SpotfireColNameCol.Visible = true;
			this.SpotfireColNameCol.Width = 110;
			// 
			// SelectedBand
			// 
			this.SelectedBand.Caption = " ";
			this.SelectedBand.Columns.Add(this.FieldSelectedCol);
			this.SelectedBand.Name = "SelectedBand";
			this.SelectedBand.VisibleIndex = 2;
			this.SelectedBand.Width = 20;
			// 
			// FieldSelectedCol
			// 
			this.FieldSelectedCol.Caption = " ";
			this.FieldSelectedCol.ColumnEdit = this.RepositoryItemCheckEdit;
			this.FieldSelectedCol.FieldName = "SelectedField";
			this.FieldSelectedCol.ImageOptions.Alignment = System.Drawing.StringAlignment.Center;
			this.FieldSelectedCol.Name = "FieldSelectedCol";
			this.FieldSelectedCol.OptionsColumn.AllowEdit = false;
			this.FieldSelectedCol.Visible = true;
			this.FieldSelectedCol.Width = 20;
			// 
			// MobiusBand
			// 
			this.MobiusBand.AppearanceHeader.Options.UseTextOptions = true;
			this.MobiusBand.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.MobiusBand.Caption = "Mobius";
			this.MobiusBand.Columns.Add(this.MobiusTableNameCol);
			this.MobiusBand.Columns.Add(this.MobiusColNameCol);
			this.MobiusBand.Name = "MobiusBand";
			this.MobiusBand.VisibleIndex = 3;
			this.MobiusBand.Width = 347;
			// 
			// MobiusTableNameCol
			// 
			this.MobiusTableNameCol.AppearanceCell.Options.UseTextOptions = true;
			this.MobiusTableNameCol.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.MobiusTableNameCol.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.MobiusTableNameCol.AppearanceCell.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.MobiusTableNameCol.AppearanceHeader.Options.UseTextOptions = true;
			this.MobiusTableNameCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.MobiusTableNameCol.AppearanceHeader.TextOptions.Trimming = DevExpress.Utils.Trimming.None;
			this.MobiusTableNameCol.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.MobiusTableNameCol.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.MobiusTableNameCol.Caption = "Table/Assay";
			this.MobiusTableNameCol.ColumnEdit = this.repositoryItemTextEdit1;
			this.MobiusTableNameCol.FieldName = "MobiusTableNameField";
			this.MobiusTableNameCol.Name = "MobiusTableNameCol";
			this.MobiusTableNameCol.OptionsColumn.AllowEdit = false;
			this.MobiusTableNameCol.Visible = true;
			this.MobiusTableNameCol.Width = 172;
			// 
			// MobiusColNameCol
			// 
			this.MobiusColNameCol.AppearanceCell.Options.UseTextOptions = true;
			this.MobiusColNameCol.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.MobiusColNameCol.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
			this.MobiusColNameCol.AppearanceCell.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.MobiusColNameCol.AppearanceHeader.Options.UseTextOptions = true;
			this.MobiusColNameCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.MobiusColNameCol.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.MobiusColNameCol.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.MobiusColNameCol.Caption = "Column Name";
			this.MobiusColNameCol.ColumnEdit = this.repositoryItemTextEdit1;
			this.MobiusColNameCol.FieldName = "MobiusColNameField";
			this.MobiusColNameCol.Name = "MobiusColNameCol";
			this.MobiusColNameCol.OptionsColumn.AllowEdit = false;
			this.MobiusColNameCol.Visible = true;
			this.MobiusColNameCol.Width = 175;
			// 
			// FieldGrid
			// 
			this.FieldGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FieldGrid.Location = new System.Drawing.Point(0, 81);
			this.FieldGrid.MainView = this.GridView;
			this.FieldGrid.Name = "FieldGrid";
			this.FieldGrid.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.RepositoryItemCheckEdit,
            this.repositoryItemTextEdit1,
            this.DataTypeImageRepositoryItemPictureEdit});
			this.FieldGrid.Size = new System.Drawing.Size(763, 348);
			this.FieldGrid.TabIndex = 217;
			this.FieldGrid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.GridView});
			// 
			// DataMapControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.HeaderPanel);
			this.Controls.Add(this.FieldGrid);
			this.Name = "DataMapControl";
			this.Size = new System.Drawing.Size(763, 429);
			this.SourceQueryMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.HeaderPanel)).EndInit();
			this.HeaderPanel.ResumeLayout(false);
			this.HeaderPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LeftArrowHead.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RightArrowHead.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RepositoryItemCheckEdit)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DataTypeImageRepositoryItemPictureEdit)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FieldGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.ImageList Bitmaps16x16;
		private System.Windows.Forms.ContextMenuStrip SourceQueryMenu;
		private System.Windows.Forms.ToolStripMenuItem MruListMenuItem;
		private System.Windows.Forms.ToolStripMenuItem FavoritesMenuItem;
		private System.Windows.Forms.ToolStripMenuItem SelectFromContentsTreeMenuItem;
		private System.Windows.Forms.ToolStripMenuItem CurrentQueryMenuItem;
		private DevExpress.XtraEditors.PanelControl HeaderPanel;
		private DevExpress.XtraEditors.LabelControl GridPromptLabel;
		private DataMapsTableSelectorControl DataTableSelectorControl;
		private DevExpress.XtraEditors.SimpleButton MapOptionsButton;
		private DevExpress.XtraEditors.LabelControl labelControl1;
		private DevExpress.XtraEditors.LabelControl SpotfireDataTableControlLabel;
		private QueryTableSelectorControl QtSelectorControl;
		private DevExpress.XtraEditors.SimpleButton SpotfireToMobiusArrowButton;
		private DevExpress.XtraEditors.PictureEdit LeftArrowHead;
		private DevExpress.XtraEditors.PictureEdit RightArrowHead;
		private DevExpress.XtraEditors.LabelControl Arrow;
		private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit RepositoryItemCheckEdit;
		private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit1;
		private DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit DataTypeImageRepositoryItemPictureEdit;
		private DevExpress.XtraGrid.Views.BandedGrid.BandedGridView GridView;
		private DevExpress.XtraGrid.Views.BandedGrid.GridBand DataTypeBand;
		private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn DataTypeImageCol;
		private DevExpress.XtraGrid.Views.BandedGrid.GridBand SpotfireBand;
		private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn OriginalSpotfireColNameCol;
		private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn SpotfireColNameCol;
		private DevExpress.XtraGrid.Views.BandedGrid.GridBand SelectedBand;
		private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn FieldSelectedCol;
		private DevExpress.XtraGrid.Views.BandedGrid.GridBand MobiusBand;
		private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn MobiusTableNameCol;
		private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn MobiusColNameCol;
		public DevExpress.XtraGrid.GridControl FieldGrid;
	}
}
