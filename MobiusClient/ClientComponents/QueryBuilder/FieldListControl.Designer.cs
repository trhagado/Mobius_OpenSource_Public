namespace Mobius.ClientComponents
{
	partial class FieldListControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FieldListControl));
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList();
			this.FieldGrid = new DevExpress.XtraGrid.GridControl();
			this.GridView = new DevExpress.XtraGrid.Views.Grid.GridView();
			this.FieldSelectedCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.RepositoryItemCheckEdit = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
			this.FieldDatabaseTableCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.repositoryItemMemoEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
			this.FieldDatabaseColumnCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.FieldShortNameCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.repositoryItemComboBox1 = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
			this.repositoryItemPictureEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit();
			this.repositoryItemTextEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
			this.MoveAxisDown = new DevExpress.XtraEditors.SimpleButton();
			this.DeleteFieldBut = new DevExpress.XtraEditors.SimpleButton();
			this.MoveAxisUp = new DevExpress.XtraEditors.SimpleButton();
			this.AddField = new DevExpress.XtraEditors.SimpleButton();
			this.Timer = new System.Windows.Forms.Timer();
			((System.ComponentModel.ISupportInitialize)(this.FieldGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RepositoryItemCheckEdit)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemMemoEdit1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemPictureEdit1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).BeginInit();
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
			// 
			// FieldGrid
			// 
			this.FieldGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FieldGrid.Location = new System.Drawing.Point(0, 0);
			this.FieldGrid.MainView = this.GridView;
			this.FieldGrid.Name = "FieldGrid";
			this.FieldGrid.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemComboBox1,
            this.repositoryItemPictureEdit1,
            this.RepositoryItemCheckEdit,
            this.repositoryItemTextEdit1,
            this.repositoryItemMemoEdit1});
			this.FieldGrid.Size = new System.Drawing.Size(563, 367);
			this.FieldGrid.TabIndex = 217;
			this.FieldGrid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.GridView});
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
			this.GridView.ColumnPanelRowHeight = 22;
			this.GridView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.FieldSelectedCol,
            this.FieldDatabaseTableCol,
            this.FieldDatabaseColumnCol,
            this.FieldShortNameCol});
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
			this.GridView.OptionsView.ColumnAutoWidth = false;
			this.GridView.OptionsView.RowAutoHeight = true;
			this.GridView.OptionsView.ShowGroupPanel = false;
			this.GridView.RowCellClick += new DevExpress.XtraGrid.Views.Grid.RowCellClickEventHandler(this.GridView_RowCellClick);
			this.GridView.CustomDrawRowIndicator += new DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventHandler(this.GridView_CustomDrawRowIndicator);
			this.GridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseDown);
			this.GridView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseUp);
			this.GridView.Click += new System.EventHandler(this.GridView_Click);
			this.GridView.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.GridView_ValidatingEditor);
			// 
			// FieldSelectedCol
			// 
			this.FieldSelectedCol.Caption = "Display";
			this.FieldSelectedCol.ColumnEdit = this.RepositoryItemCheckEdit;
			this.FieldSelectedCol.FieldName = "FieldSelectedCol";
			this.FieldSelectedCol.Name = "FieldSelectedCol";
			this.FieldSelectedCol.Visible = true;
			this.FieldSelectedCol.VisibleIndex = 0;
			this.FieldSelectedCol.Width = 42;
			// 
			// RepositoryItemCheckEdit
			// 
			this.RepositoryItemCheckEdit.AutoHeight = false;
			this.RepositoryItemCheckEdit.Name = "RepositoryItemCheckEdit";
			// 
			// FieldDatabaseTableCol
			// 
			this.FieldDatabaseTableCol.AppearanceCell.Options.UseTextOptions = true;
			this.FieldDatabaseTableCol.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.FieldDatabaseTableCol.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.FieldDatabaseTableCol.AppearanceCell.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.FieldDatabaseTableCol.AppearanceHeader.Options.UseTextOptions = true;
			this.FieldDatabaseTableCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.FieldDatabaseTableCol.AppearanceHeader.TextOptions.Trimming = DevExpress.Utils.Trimming.None;
			this.FieldDatabaseTableCol.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
			this.FieldDatabaseTableCol.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.FieldDatabaseTableCol.Caption = "Table/Assay";
			this.FieldDatabaseTableCol.ColumnEdit = this.repositoryItemMemoEdit1;
			this.FieldDatabaseTableCol.FieldName = "FieldDatabaseTableCol";
			this.FieldDatabaseTableCol.Name = "FieldDatabaseTableCol";
			this.FieldDatabaseTableCol.OptionsColumn.AllowEdit = false;
			this.FieldDatabaseTableCol.Visible = true;
			this.FieldDatabaseTableCol.VisibleIndex = 1;
			this.FieldDatabaseTableCol.Width = 216;
			// 
			// repositoryItemMemoEdit1
			// 
			this.repositoryItemMemoEdit1.Appearance.Options.UseTextOptions = true;
			this.repositoryItemMemoEdit1.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.repositoryItemMemoEdit1.Name = "repositoryItemMemoEdit1";
			// 
			// FieldDatabaseColumnCol
			// 
			this.FieldDatabaseColumnCol.AppearanceCell.Options.UseTextOptions = true;
			this.FieldDatabaseColumnCol.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.FieldDatabaseColumnCol.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
			this.FieldDatabaseColumnCol.AppearanceCell.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.FieldDatabaseColumnCol.AppearanceHeader.Options.UseTextOptions = true;
			this.FieldDatabaseColumnCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.FieldDatabaseColumnCol.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
			this.FieldDatabaseColumnCol.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.FieldDatabaseColumnCol.Caption = "Column";
			this.FieldDatabaseColumnCol.ColumnEdit = this.repositoryItemMemoEdit1;
			this.FieldDatabaseColumnCol.FieldName = "FieldDatabaseColumnCol";
			this.FieldDatabaseColumnCol.Name = "FieldDatabaseColumnCol";
			this.FieldDatabaseColumnCol.OptionsColumn.AllowEdit = false;
			this.FieldDatabaseColumnCol.Visible = true;
			this.FieldDatabaseColumnCol.VisibleIndex = 2;
			this.FieldDatabaseColumnCol.Width = 196;
			// 
			// FieldShortNameCol
			// 
			this.FieldShortNameCol.Caption = "Short Name";
			this.FieldShortNameCol.FieldName = "SpotfireColumnName";
			this.FieldShortNameCol.Name = "SpotfireColumnName";
			this.FieldShortNameCol.Visible = true;
			this.FieldShortNameCol.VisibleIndex = 3;
			this.FieldShortNameCol.Width = 93;
			// 
			// repositoryItemComboBox1
			// 
			this.repositoryItemComboBox1.AutoHeight = false;
			this.repositoryItemComboBox1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.repositoryItemComboBox1.Name = "repositoryItemComboBox1";
			// 
			// repositoryItemPictureEdit1
			// 
			this.repositoryItemPictureEdit1.InitialImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("repositoryItemPictureEdit1.InitialImageOptions.Image")));
			this.repositoryItemPictureEdit1.Name = "repositoryItemPictureEdit1";
			this.repositoryItemPictureEdit1.NullText = " ";
			// 
			// repositoryItemTextEdit1
			// 
			this.repositoryItemTextEdit1.Appearance.Options.UseTextOptions = true;
			this.repositoryItemTextEdit1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.repositoryItemTextEdit1.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.repositoryItemTextEdit1.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.repositoryItemTextEdit1.Name = "repositoryItemTextEdit1";
			// 
			// MoveAxisDown
			// 
			this.MoveAxisDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.MoveAxisDown.ImageOptions.ImageIndex = 5;
			this.MoveAxisDown.ImageOptions.ImageList = this.Bitmaps16x16;
			this.MoveAxisDown.Location = new System.Drawing.Point(195, 371);
			this.MoveAxisDown.Name = "MoveAxisDown";
			this.MoveAxisDown.Size = new System.Drawing.Size(24, 24);
			this.MoveAxisDown.TabIndex = 215;
			this.MoveAxisDown.TabStop = false;
			this.MoveAxisDown.ToolTip = "Move Down";
			this.MoveAxisDown.Click += new System.EventHandler(this.MoveFieldDown_Click);
			// 
			// DeleteFieldBut
			// 
			this.DeleteFieldBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DeleteFieldBut.ImageOptions.ImageIndex = 3;
			this.DeleteFieldBut.ImageOptions.ImageList = this.Bitmaps16x16;
			this.DeleteFieldBut.Location = new System.Drawing.Point(85, 371);
			this.DeleteFieldBut.Name = "DeleteFieldBut";
			this.DeleteFieldBut.Size = new System.Drawing.Size(74, 24);
			this.DeleteFieldBut.TabIndex = 216;
			this.DeleteFieldBut.TabStop = false;
			this.DeleteFieldBut.Text = "&Remove";
			this.DeleteFieldBut.Click += new System.EventHandler(this.DeleteFieldBut_Click);
			// 
			// MoveAxisUp
			// 
			this.MoveAxisUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.MoveAxisUp.ImageOptions.ImageIndex = 4;
			this.MoveAxisUp.ImageOptions.ImageList = this.Bitmaps16x16;
			this.MoveAxisUp.Location = new System.Drawing.Point(165, 371);
			this.MoveAxisUp.Name = "MoveAxisUp";
			this.MoveAxisUp.Size = new System.Drawing.Size(24, 24);
			this.MoveAxisUp.TabIndex = 214;
			this.MoveAxisUp.TabStop = false;
			this.MoveAxisUp.ToolTip = "Move Up";
			this.MoveAxisUp.Click += new System.EventHandler(this.MoveFieldUp_Click);
			// 
			// AddField
			// 
			this.AddField.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AddField.ImageOptions.ImageIndex = 0;
			this.AddField.ImageOptions.ImageList = this.Bitmaps16x16;
			this.AddField.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleLeft;
			this.AddField.Location = new System.Drawing.Point(1, 371);
			this.AddField.Name = "AddField";
			this.AddField.Size = new System.Drawing.Size(78, 24);
			this.AddField.TabIndex = 213;
			this.AddField.TabStop = false;
			this.AddField.Text = "&Add Field";
			this.AddField.Click += new System.EventHandler(this.AddField_Click);
			// 
			// Timer
			// 
			this.Timer.Tick += new System.EventHandler(this.Timer_Tick);
			// 
			// FieldListControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.FieldGrid);
			this.Controls.Add(this.MoveAxisDown);
			this.Controls.Add(this.DeleteFieldBut);
			this.Controls.Add(this.MoveAxisUp);
			this.Controls.Add(this.AddField);
			this.Name = "FieldListControl";
			this.Size = new System.Drawing.Size(563, 396);
			((System.ComponentModel.ISupportInitialize)(this.FieldGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RepositoryItemCheckEdit)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemMemoEdit1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemPictureEdit1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.ImageList Bitmaps16x16;
		public DevExpress.XtraGrid.GridControl FieldGrid;
		private DevExpress.XtraGrid.Views.Grid.GridView GridView;
		private DevExpress.XtraGrid.Columns.GridColumn FieldDatabaseTableCol;
		private DevExpress.XtraGrid.Columns.GridColumn FieldShortNameCol;
		private DevExpress.XtraGrid.Columns.GridColumn FieldDatabaseColumnCol;
		private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit RepositoryItemCheckEdit;
		private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox1;
		private DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit repositoryItemPictureEdit1;
		public DevExpress.XtraEditors.SimpleButton MoveAxisDown;
		public DevExpress.XtraEditors.SimpleButton DeleteFieldBut;
		public DevExpress.XtraEditors.SimpleButton MoveAxisUp;
		public DevExpress.XtraEditors.SimpleButton AddField;
		private DevExpress.XtraGrid.Columns.GridColumn FieldSelectedCol;
		private System.Windows.Forms.Timer Timer;
		private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit1;
		private DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit repositoryItemMemoEdit1;
	}
}
