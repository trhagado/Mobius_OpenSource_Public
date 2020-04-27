namespace Mobius.ClientComponents
{
	partial class PermissionsList
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PermissionsList));
			this.AddItem = new DevExpress.XtraEditors.SimpleButton();
			this.ItemNameComboBox = new DevExpress.XtraEditors.ComboBoxEdit();
			this.AddPrompt = new DevExpress.XtraEditors.LabelControl();
			this.Grid = new DevExpress.XtraGrid.GridControl();
			this.GridView = new DevExpress.XtraGrid.Views.Grid.GridView();
			this.ItemTypeImageCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.repositoryItemPictureEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit();
			this.ExternalNameCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.ReadCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.WriteCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.RemoveItem = new DevExpress.XtraEditors.SimpleButton();
			((System.ComponentModel.ISupportInitialize)(this.ItemNameComboBox.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemPictureEdit1)).BeginInit();
			this.SuspendLayout();
			// 
			// AddItem
			// 
			this.AddItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AddItem.Location = new System.Drawing.Point(374, 251);
			this.AddItem.Name = "AddItem";
			this.AddItem.Size = new System.Drawing.Size(59, 24);
			this.AddItem.TabIndex = 226;
			this.AddItem.Text = "Add";
			this.AddItem.Click += new System.EventHandler(this.AddGroupUser_Click);
			// 
			// ItemNameComboBox
			// 
			this.ItemNameComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.ItemNameComboBox.Location = new System.Drawing.Point(69, 253);
			this.ItemNameComboBox.Name = "ItemNameComboBox";
			this.ItemNameComboBox.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.ItemNameComboBox.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
			this.ItemNameComboBox.Size = new System.Drawing.Size(299, 20);
			this.ItemNameComboBox.TabIndex = 225;
			this.ItemNameComboBox.Enter += new System.EventHandler(this.GroupUserComboBox_Enter);
			// 
			// AddPrompt
			// 
			this.AddPrompt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AddPrompt.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
			this.AddPrompt.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.AddPrompt.Location = new System.Drawing.Point(4, 256);
			this.AddPrompt.Name = "AddPrompt";
			this.AddPrompt.Size = new System.Drawing.Size(59, 13);
			this.AddPrompt.TabIndex = 224;
			this.AddPrompt.Text = "Group/User:";
			// 
			// Grid
			// 
			this.Grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.Grid.Location = new System.Drawing.Point(0, 0);
			this.Grid.MainView = this.GridView;
			this.Grid.Name = "Grid";
			this.Grid.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemPictureEdit1});
			this.Grid.Size = new System.Drawing.Size(462, 245);
			this.Grid.TabIndex = 223;
			this.Grid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.GridView});
			// 
			// GridView
			// 
			this.GridView.Appearance.HeaderPanel.Options.UseTextOptions = true;
			this.GridView.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.GridView.Appearance.HeaderPanel.TextOptions.Trimming = DevExpress.Utils.Trimming.None;
			this.GridView.Appearance.HeaderPanel.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.GridView.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.GridView.ColumnPanelRowHeight = 20;
			this.GridView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.ItemTypeImageCol,
            this.ExternalNameCol,
            this.ReadCol,
            this.WriteCol});
			this.GridView.GridControl = this.Grid;
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
			// 
			// ItemTypeImageCol
			// 
			this.ItemTypeImageCol.Caption = " ";
			this.ItemTypeImageCol.ColumnEdit = this.repositoryItemPictureEdit1;
			this.ItemTypeImageCol.FieldName = "ItemTypeImageCol";
			this.ItemTypeImageCol.Name = "ItemTypeImageCol";
			this.ItemTypeImageCol.OptionsColumn.AllowEdit = false;
			this.ItemTypeImageCol.Visible = true;
			this.ItemTypeImageCol.VisibleIndex = 0;
			this.ItemTypeImageCol.Width = 20;
			// 
			// repositoryItemPictureEdit1
			// 
			this.repositoryItemPictureEdit1.Name = "repositoryItemPictureEdit1";
			// 
			// ExternalNameCol
			// 
			this.ExternalNameCol.AppearanceHeader.Options.UseTextOptions = true;
			this.ExternalNameCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.ExternalNameCol.AppearanceHeader.TextOptions.Trimming = DevExpress.Utils.Trimming.None;
			this.ExternalNameCol.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.ExternalNameCol.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.ExternalNameCol.Caption = "Group / User";
			this.ExternalNameCol.FieldName = "ExternalNameCol";
			this.ExternalNameCol.Name = "ExternalNameCol";
			this.ExternalNameCol.OptionsColumn.AllowEdit = false;
			this.ExternalNameCol.Visible = true;
			this.ExternalNameCol.VisibleIndex = 1;
			this.ExternalNameCol.Width = 258;
			// 
			// ReadCol
			// 
			this.ReadCol.Caption = "Read";
			this.ReadCol.FieldName = "ReadCol";
			this.ReadCol.Name = "ReadCol";
			this.ReadCol.Visible = true;
			this.ReadCol.VisibleIndex = 2;
			this.ReadCol.Width = 73;
			// 
			// WriteCol
			// 
			this.WriteCol.Caption = "Write";
			this.WriteCol.FieldName = "WriteCol";
			this.WriteCol.Name = "WriteCol";
			this.WriteCol.Visible = true;
			this.WriteCol.VisibleIndex = 3;
			this.WriteCol.Width = 64;
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "Person.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "People.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "Delete.bmp");
			// 
			// RemoveItem
			// 
			this.RemoveItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RemoveItem.Appearance.Options.UseTextOptions = true;
			this.RemoveItem.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.RemoveItem.ImageIndex = 2;
			this.RemoveItem.ImageList = this.Bitmaps16x16;
			this.RemoveItem.Location = new System.Drawing.Point(438, 251);
			this.RemoveItem.Name = "RemoveItem";
			this.RemoveItem.Size = new System.Drawing.Size(24, 24);
			this.RemoveItem.TabIndex = 227;
			this.RemoveItem.ToolTip = "Remove the currently selected row from the grid";
			this.RemoveItem.Click += new System.EventHandler(this.Remove_Click);
			// 
			// PermissionsList
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.RemoveItem);
			this.Controls.Add(this.AddItem);
			this.Controls.Add(this.ItemNameComboBox);
			this.Controls.Add(this.AddPrompt);
			this.Controls.Add(this.Grid);
			this.Name = "PermissionsList";
			this.Size = new System.Drawing.Size(463, 277);
			((System.ComponentModel.ISupportInitialize)(this.ItemNameComboBox.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemPictureEdit1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraGrid.GridControl Grid;
		private DevExpress.XtraGrid.Views.Grid.GridView GridView;
		private DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit repositoryItemPictureEdit1;
		public System.Windows.Forms.ImageList Bitmaps16x16;
		public DevExpress.XtraEditors.SimpleButton AddItem;
		public DevExpress.XtraEditors.ComboBoxEdit ItemNameComboBox;
		public DevExpress.XtraEditors.LabelControl AddPrompt;
		public DevExpress.XtraEditors.SimpleButton RemoveItem;
		public DevExpress.XtraGrid.Columns.GridColumn ItemTypeImageCol;
		public DevExpress.XtraGrid.Columns.GridColumn ExternalNameCol;
		public DevExpress.XtraGrid.Columns.GridColumn ReadCol;
		public DevExpress.XtraGrid.Columns.GridColumn WriteCol;
	}
}
