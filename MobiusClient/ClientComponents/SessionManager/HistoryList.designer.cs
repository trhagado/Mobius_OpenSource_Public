namespace Mobius.ClientComponents
{
	partial class HistoryList
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
			this.Grid = new DevExpress.XtraGrid.GridControl();
			this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
			this.QueryCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.HitCountCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.TimeCol = new DevExpress.XtraGrid.Columns.GridColumn();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// Grid
			// 
			this.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid.Location = new System.Drawing.Point(0, 0);
			this.Grid.MainView = this.gridView1;
			this.Grid.Name = "Grid";
			this.Grid.Size = new System.Drawing.Size(323, 387);
			this.Grid.TabIndex = 1;
			this.Grid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
			// 
			// gridView1
			// 
			this.gridView1.Appearance.HeaderPanel.Options.UseTextOptions = true;
			this.gridView1.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.gridView1.Appearance.SelectedRow.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Underline);
			this.gridView1.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Blue;
			this.gridView1.Appearance.SelectedRow.Options.UseFont = true;
			this.gridView1.Appearance.SelectedRow.Options.UseForeColor = true;
			this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.QueryCol,
            this.HitCountCol,
            this.TimeCol});
			this.gridView1.GridControl = this.Grid;
			this.gridView1.IndicatorWidth = 25;
			this.gridView1.Name = "gridView1";
			this.gridView1.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;
			this.gridView1.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False;
			this.gridView1.OptionsBehavior.Editable = false;
			this.gridView1.OptionsCustomization.AllowColumnMoving = false;
			this.gridView1.OptionsCustomization.AllowFilter = false;
			this.gridView1.OptionsCustomization.AllowGroup = false;
			this.gridView1.OptionsCustomization.AllowQuickHideColumns = false;
			this.gridView1.OptionsSelection.MultiSelect = true;
			this.gridView1.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.CellSelect;
			this.gridView1.OptionsView.ColumnAutoWidth = false;
			this.gridView1.OptionsView.ShowGroupExpandCollapseButtons = false;
			this.gridView1.OptionsView.ShowGroupPanel = false;
			this.gridView1.VertScrollVisibility = DevExpress.XtraGrid.Views.Base.ScrollVisibility.Always;
			this.gridView1.CustomDrawRowIndicator += new DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventHandler(this.GridView_CustomDrawRowIndicator);
			this.gridView1.RowCellClick += new DevExpress.XtraGrid.Views.Grid.RowCellClickEventHandler(this.GridView_RowCellClick);
			// 
			// QueryCol
			// 
			this.QueryCol.AppearanceCell.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Underline);
			this.QueryCol.AppearanceCell.ForeColor = System.Drawing.Color.Blue;
			this.QueryCol.AppearanceCell.Options.UseFont = true;
			this.QueryCol.AppearanceCell.Options.UseForeColor = true;
			this.QueryCol.AppearanceCell.Options.UseTextOptions = true;
			this.QueryCol.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.QueryCol.AppearanceHeader.Options.UseTextOptions = true;
			this.QueryCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.QueryCol.Caption = "Query";
			this.QueryCol.FieldName = "QueryCol";
			this.QueryCol.Name = "QueryCol";
			this.QueryCol.Visible = true;
			this.QueryCol.VisibleIndex = 0;
			this.QueryCol.Width = 146;
			// 
			// HitCountCol
			// 
			this.HitCountCol.AppearanceCell.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Underline);
			this.HitCountCol.AppearanceCell.ForeColor = System.Drawing.Color.Blue;
			this.HitCountCol.AppearanceCell.Options.UseFont = true;
			this.HitCountCol.AppearanceCell.Options.UseForeColor = true;
			this.HitCountCol.AppearanceCell.Options.UseTextOptions = true;
			this.HitCountCol.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
			this.HitCountCol.AppearanceHeader.Options.UseTextOptions = true;
			this.HitCountCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.HitCountCol.Caption = "Hit Count";
			this.HitCountCol.FieldName = "HitCountCol";
			this.HitCountCol.Name = "HitCountCol";
			this.HitCountCol.Visible = true;
			this.HitCountCol.VisibleIndex = 1;
			this.HitCountCol.Width = 58;
			// 
			// TimeCol
			// 
			this.TimeCol.AppearanceCell.Options.UseTextOptions = true;
			this.TimeCol.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.TimeCol.AppearanceHeader.Options.UseTextOptions = true;
			this.TimeCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.TimeCol.Caption = "Time";
			this.TimeCol.FieldName = "TimeCol";
			this.TimeCol.Name = "TimeCol";
			this.TimeCol.Visible = true;
			this.TimeCol.VisibleIndex = 2;
			this.TimeCol.Width = 69;
			// 
			// HistoryList
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(323, 387);
			this.ControlBox = false;
			this.Controls.Add(this.Grid);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "HistoryList";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Deactivate += new System.EventHandler(this.HistoryList_Deactivate);
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal DevExpress.XtraGrid.GridControl Grid;
		private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
		private DevExpress.XtraGrid.Columns.GridColumn QueryCol;
		private DevExpress.XtraGrid.Columns.GridColumn HitCountCol;
		private DevExpress.XtraGrid.Columns.GridColumn TimeCol;
	}
}