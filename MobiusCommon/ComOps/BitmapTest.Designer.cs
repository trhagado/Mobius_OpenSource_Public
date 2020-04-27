namespace Mobius.ComOps
{
	partial class BitmapTest
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
			this.FieldGrid = new DevExpress.XtraGrid.GridControl();
			this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
			this.RowIdColumn = new DevExpress.XtraGrid.Columns.GridColumn();
			this.IconSetGridColumn = new DevExpress.XtraGrid.Columns.GridColumn();
			this.IconSetImageComboBoxRepositoryItem = new DevExpress.XtraEditors.Repository.RepositoryItemImageComboBox();
			this.ColorSetGridColumn = new DevExpress.XtraGrid.Columns.GridColumn();
			this.ColorSetImageComboBoxRepositoryItem = new DevExpress.XtraEditors.Repository.RepositoryItemImageComboBox();
			this.ColorScaleGridColumn = new DevExpress.XtraGrid.Columns.GridColumn();
			this.ColorScaleImageComboBoxRepositoryItem = new DevExpress.XtraEditors.Repository.RepositoryItemImageComboBox();
			this.DataBarsGridColumn = new DevExpress.XtraGrid.Columns.GridColumn();
			this.DataBarsImageComboBoxRepositoryItem = new DevExpress.XtraEditors.Repository.RepositoryItemImageComboBox();
			this.PictureEditColumn = new DevExpress.XtraGrid.Columns.GridColumn();
			this.BitmapsPictureEditRepositoryItem = new DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit();
			((System.ComponentModel.ISupportInitialize)(this.FieldGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IconSetImageComboBoxRepositoryItem)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorSetImageComboBoxRepositoryItem)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorScaleImageComboBoxRepositoryItem)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DataBarsImageComboBoxRepositoryItem)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BitmapsPictureEditRepositoryItem)).BeginInit();
			this.SuspendLayout();
			// 
			// FieldGrid
			// 
			this.FieldGrid.Dock = System.Windows.Forms.DockStyle.Left;
			this.FieldGrid.Location = new System.Drawing.Point(0, 0);
			this.FieldGrid.MainView = this.gridView1;
			this.FieldGrid.Name = "FieldGrid";
			this.FieldGrid.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.IconSetImageComboBoxRepositoryItem,
            this.BitmapsPictureEditRepositoryItem,
            this.ColorSetImageComboBoxRepositoryItem,
            this.ColorScaleImageComboBoxRepositoryItem,
            this.DataBarsImageComboBoxRepositoryItem});
			this.FieldGrid.Size = new System.Drawing.Size(477, 562);
			this.FieldGrid.TabIndex = 0;
			this.FieldGrid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
			// 
			// gridView1
			// 
			this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.RowIdColumn,
            this.IconSetGridColumn,
            this.ColorSetGridColumn,
            this.ColorScaleGridColumn,
            this.DataBarsGridColumn,
            this.PictureEditColumn});
			this.gridView1.GridControl = this.FieldGrid;
			this.gridView1.Name = "gridView1";
			this.gridView1.OptionsView.ColumnAutoWidth = false;
			this.gridView1.CustomDrawCell += new DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventHandler(this.GridView_CustomDrawCell);
			// 
			// RowIdColumn
			// 
			this.RowIdColumn.Caption = "Int Id";
			this.RowIdColumn.FieldName = "RowIdField";
			this.RowIdColumn.Name = "RowIdColumn";
			this.RowIdColumn.Visible = true;
			this.RowIdColumn.VisibleIndex = 0;
			this.RowIdColumn.Width = 39;
			// 
			// IconSetGridColumn
			// 
			this.IconSetGridColumn.Caption = "Icon Set";
			this.IconSetGridColumn.ColumnEdit = this.IconSetImageComboBoxRepositoryItem;
			this.IconSetGridColumn.FieldName = "IconSetField";
			this.IconSetGridColumn.Name = "IconSetGridColumn";
			this.IconSetGridColumn.Visible = true;
			this.IconSetGridColumn.VisibleIndex = 4;
			this.IconSetGridColumn.Width = 64;
			// 
			// IconSetImageComboBoxRepositoryItem
			// 
			this.IconSetImageComboBoxRepositoryItem.AutoHeight = false;
			this.IconSetImageComboBoxRepositoryItem.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.IconSetImageComboBoxRepositoryItem.Name = "IconSetImageComboBoxRepositoryItem";
			// 
			// ColorSetGridColumn
			// 
			this.ColorSetGridColumn.Caption = "Color Set";
			this.ColorSetGridColumn.ColumnEdit = this.ColorSetImageComboBoxRepositoryItem;
			this.ColorSetGridColumn.FieldName = "ColorSetField";
			this.ColorSetGridColumn.Name = "ColorSetGridColumn";
			this.ColorSetGridColumn.Visible = true;
			this.ColorSetGridColumn.VisibleIndex = 1;
			this.ColorSetGridColumn.Width = 64;
			// 
			// ColorSetImageComboBoxRepositoryItem
			// 
			this.ColorSetImageComboBoxRepositoryItem.AutoHeight = false;
			this.ColorSetImageComboBoxRepositoryItem.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.ColorSetImageComboBoxRepositoryItem.Name = "ColorSetImageComboBoxRepositoryItem";
			// 
			// ColorScaleGridColumn
			// 
			this.ColorScaleGridColumn.Caption = "Color Scale";
			this.ColorScaleGridColumn.ColumnEdit = this.ColorScaleImageComboBoxRepositoryItem;
			this.ColorScaleGridColumn.FieldName = "ColorScaleField";
			this.ColorScaleGridColumn.Name = "ColorScaleGridColumn";
			this.ColorScaleGridColumn.Visible = true;
			this.ColorScaleGridColumn.VisibleIndex = 2;
			this.ColorScaleGridColumn.Width = 64;
			// 
			// ColorScaleImageComboBoxRepositoryItem
			// 
			this.ColorScaleImageComboBoxRepositoryItem.AutoHeight = false;
			this.ColorScaleImageComboBoxRepositoryItem.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.ColorScaleImageComboBoxRepositoryItem.Name = "ColorScaleImageComboBoxRepositoryItem";
			// 
			// DataBarsGridColumn
			// 
			this.DataBarsGridColumn.Caption = "Data Bars";
			this.DataBarsGridColumn.ColumnEdit = this.DataBarsImageComboBoxRepositoryItem;
			this.DataBarsGridColumn.FieldName = "DataBarsField";
			this.DataBarsGridColumn.Name = "DataBarsGridColumn";
			this.DataBarsGridColumn.Visible = true;
			this.DataBarsGridColumn.VisibleIndex = 3;
			this.DataBarsGridColumn.Width = 64;
			// 
			// DataBarsImageComboBoxRepositoryItem
			// 
			this.DataBarsImageComboBoxRepositoryItem.AutoHeight = false;
			this.DataBarsImageComboBoxRepositoryItem.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.DataBarsImageComboBoxRepositoryItem.Name = "DataBarsImageComboBoxRepositoryItem";
			// 
			// PictureEditColumn
			// 
			this.PictureEditColumn.Caption = "Picture Edit";
			this.PictureEditColumn.ColumnEdit = this.BitmapsPictureEditRepositoryItem;
			this.PictureEditColumn.FieldName = "PictureEditField";
			this.PictureEditColumn.Name = "PictureEditColumn";
			this.PictureEditColumn.Visible = true;
			this.PictureEditColumn.VisibleIndex = 5;
			this.PictureEditColumn.Width = 64;
			// 
			// BitmapsPictureEditRepositoryItem
			// 
			this.BitmapsPictureEditRepositoryItem.Name = "BitmapsPictureEditRepositoryItem";
			// 
			// BitmapTest
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(766, 562);
			this.Controls.Add(this.FieldGrid);
			this.Name = "BitmapTest";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "BitmapTest";
			((System.ComponentModel.ISupportInitialize)(this.FieldGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IconSetImageComboBoxRepositoryItem)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorSetImageComboBoxRepositoryItem)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorScaleImageComboBoxRepositoryItem)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DataBarsImageComboBoxRepositoryItem)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BitmapsPictureEditRepositoryItem)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraGrid.GridControl FieldGrid;
		private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
		private DevExpress.XtraGrid.Columns.GridColumn RowIdColumn;
		private DevExpress.XtraGrid.Columns.GridColumn IconSetGridColumn;
		private DevExpress.XtraGrid.Columns.GridColumn PictureEditColumn;
		private DevExpress.XtraGrid.Columns.GridColumn ColorSetGridColumn;
		private DevExpress.XtraGrid.Columns.GridColumn ColorScaleGridColumn;
		private DevExpress.XtraGrid.Columns.GridColumn DataBarsGridColumn;

		private DevExpress.XtraEditors.Repository.RepositoryItemImageComboBox ColorSetImageComboBoxRepositoryItem;

		private DevExpress.XtraEditors.Repository.RepositoryItemImageComboBox ColorScaleImageComboBoxRepositoryItem;

		private DevExpress.XtraEditors.Repository.RepositoryItemImageComboBox DataBarsImageComboBoxRepositoryItem;

		private DevExpress.XtraEditors.Repository.RepositoryItemImageComboBox IconSetImageComboBoxRepositoryItem;

		private DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit BitmapsPictureEditRepositoryItem;
	}
}