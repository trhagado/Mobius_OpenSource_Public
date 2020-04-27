namespace Mobius.ClientComponents
{
	partial class QueryEngineStatsForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QueryEngineStatsForm));
			this.Grid = new DevExpress.XtraGrid.GridControl();
			this.GridView = new DevExpress.XtraGrid.Views.Grid.GridView();
			this.MetatableLabel = new DevExpress.XtraGrid.Columns.GridColumn();
			this.MetatableName = new DevExpress.XtraGrid.Columns.GridColumn();
			this.ColumnCount = new DevExpress.XtraGrid.Columns.GridColumn();
			this.ColumnsSelected = new DevExpress.XtraGrid.Columns.GridColumn();
			this.CriteriaCount = new DevExpress.XtraGrid.Columns.GridColumn();
			this.BrokerCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.Multipivot = new DevExpress.XtraGrid.Columns.GridColumn();
			this.NextRowCount = new DevExpress.XtraGrid.Columns.GridColumn();
			this.ReadRowCount = new DevExpress.XtraGrid.Columns.GridColumn();
			this.OracleTime = new DevExpress.XtraGrid.Columns.GridColumn();
			this.ShowSqlButton = new DevExpress.XtraEditors.SimpleButton();
			this.SearchTimeCtl = new DevExpress.XtraEditors.LabelControl();
			this.SearchHitCountCtl = new DevExpress.XtraEditors.LabelControl();
			this.Timer = new System.Windows.Forms.Timer(this.components);
			this.MetatableRowsCtl = new DevExpress.XtraEditors.LabelControl();
			this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
			this.panelControl2 = new DevExpress.XtraEditors.PanelControl();
			this.GridRowsCtl = new DevExpress.XtraEditors.LabelControl();
			this.OracleRowsCtl = new DevExpress.XtraEditors.LabelControl();
			this.RetrievalTimeCtl = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
			this.panelControl1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
			this.panelControl2.SuspendLayout();
			this.SuspendLayout();
			// 
			// Grid
			// 
			this.Grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Grid.Cursor = System.Windows.Forms.Cursors.Default;
			this.Grid.Location = new System.Drawing.Point(12, 12);
			this.Grid.MainView = this.GridView;
			this.Grid.Name = "Grid";
			this.Grid.Size = new System.Drawing.Size(900, 679);
			this.Grid.TabIndex = 210;
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
			this.GridView.ColumnPanelRowHeight = 48;
			this.GridView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.MetatableLabel,
            this.MetatableName,
            this.ColumnCount,
            this.ColumnsSelected,
            this.CriteriaCount,
            this.BrokerCol,
            this.Multipivot,
            this.NextRowCount,
            this.ReadRowCount,
            this.OracleTime});
			this.GridView.GridControl = this.Grid;
			this.GridView.IndicatorWidth = 32;
			this.GridView.Name = "GridView";
			this.GridView.OptionsBehavior.Editable = false;
			this.GridView.OptionsCustomization.AllowColumnMoving = false;
			this.GridView.OptionsCustomization.AllowFilter = false;
			this.GridView.OptionsCustomization.AllowGroup = false;
			this.GridView.OptionsCustomization.AllowRowSizing = true;
			this.GridView.OptionsMenu.EnableColumnMenu = false;
			this.GridView.OptionsMenu.EnableFooterMenu = false;
			this.GridView.OptionsMenu.EnableGroupPanelMenu = false;
			this.GridView.OptionsSelection.MultiSelect = true;
			this.GridView.OptionsView.ColumnAutoWidth = false;
			this.GridView.OptionsView.RowAutoHeight = true;
			this.GridView.OptionsView.ShowGroupPanel = false;
			this.GridView.CustomDrawRowIndicator += new DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventHandler(this.GridView_CustomDrawRowIndicator);
			// 
			// MetatableLabel
			// 
			this.MetatableLabel.Caption = "Table Name";
			this.MetatableLabel.FieldName = "MetatableLabel";
			this.MetatableLabel.Name = "MetatableLabel";
			this.MetatableLabel.Visible = true;
			this.MetatableLabel.VisibleIndex = 0;
			this.MetatableLabel.Width = 238;
			// 
			// MetatableName
			// 
			this.MetatableName.AppearanceHeader.Options.UseTextOptions = true;
			this.MetatableName.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.MetatableName.AppearanceHeader.TextOptions.Trimming = DevExpress.Utils.Trimming.None;
			this.MetatableName.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.MetatableName.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.MetatableName.Caption = "Metatable";
			this.MetatableName.FieldName = "MetatableName";
			this.MetatableName.Name = "MetatableName";
			this.MetatableName.Visible = true;
			this.MetatableName.VisibleIndex = 1;
			this.MetatableName.Width = 117;
			// 
			// ColumnCount
			// 
			this.ColumnCount.Caption = "Columns";
			this.ColumnCount.FieldName = "ColumnCount";
			this.ColumnCount.Name = "ColumnCount";
			this.ColumnCount.Visible = true;
			this.ColumnCount.VisibleIndex = 2;
			this.ColumnCount.Width = 51;
			// 
			// ColumnsSelected
			// 
			this.ColumnsSelected.Caption = "Columns Selected";
			this.ColumnsSelected.FieldName = "ColumnsSelected";
			this.ColumnsSelected.Name = "ColumnsSelected";
			this.ColumnsSelected.Visible = true;
			this.ColumnsSelected.VisibleIndex = 3;
			this.ColumnsSelected.Width = 49;
			// 
			// CriteriaCount
			// 
			this.CriteriaCount.Caption = "Criteria";
			this.CriteriaCount.FieldName = "CriteriaCount";
			this.CriteriaCount.Name = "CriteriaCount";
			this.CriteriaCount.Visible = true;
			this.CriteriaCount.VisibleIndex = 4;
			this.CriteriaCount.Width = 50;
			// 
			// BrokerCol
			// 
			this.BrokerCol.Caption = "Broker Type";
			this.BrokerCol.FieldName = "BrokerCol";
			this.BrokerCol.Name = "BrokerCol";
			this.BrokerCol.Visible = true;
			this.BrokerCol.VisibleIndex = 5;
			// 
			// Multipivot
			// 
			this.Multipivot.Caption = "MP";
			this.Multipivot.FieldName = "Multipivot";
			this.Multipivot.Name = "Multipivot";
			this.Multipivot.Visible = true;
			this.Multipivot.VisibleIndex = 6;
			this.Multipivot.Width = 24;
			// 
			// NextRowCount
			// 
			this.NextRowCount.Caption = "MetaTbl Rows";
			this.NextRowCount.FieldName = "NextRowCount";
			this.NextRowCount.Name = "NextRowCount";
			this.NextRowCount.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum)});
			this.NextRowCount.Visible = true;
			this.NextRowCount.VisibleIndex = 7;
			this.NextRowCount.Width = 50;
			// 
			// ReadRowCount
			// 
			this.ReadRowCount.Caption = "DB Server Rows";
			this.ReadRowCount.FieldName = "ReadRowCount";
			this.ReadRowCount.Name = "ReadRowCount";
			this.ReadRowCount.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum)});
			this.ReadRowCount.Visible = true;
			this.ReadRowCount.VisibleIndex = 8;
			this.ReadRowCount.Width = 50;
			// 
			// OracleTime
			// 
			this.OracleTime.Caption = "DB Server Time (s)";
			this.OracleTime.DisplayFormat.FormatString = "F3";
			this.OracleTime.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
			this.OracleTime.FieldName = "OracleTime";
			this.OracleTime.Name = "OracleTime";
			this.OracleTime.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum)});
			this.OracleTime.Visible = true;
			this.OracleTime.VisibleIndex = 9;
			this.OracleTime.Width = 56;
			// 
			// ShowSqlButton
			// 
			this.ShowSqlButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShowSqlButton.Location = new System.Drawing.Point(17, 697);
			this.ShowSqlButton.Name = "ShowSqlButton";
			this.ShowSqlButton.Size = new System.Drawing.Size(80, 22);
			this.ShowSqlButton.TabIndex = 211;
			this.ShowSqlButton.Text = "Show SQL";
			this.ShowSqlButton.Click += new System.EventHandler(this.ShowSqlButton_Click);
			// 
			// SearchTimeCtl
			// 
			this.SearchTimeCtl.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Horizontal;
			this.SearchTimeCtl.Location = new System.Drawing.Point(5, 6);
			this.SearchTimeCtl.Name = "SearchTimeCtl";
			this.SearchTimeCtl.Size = new System.Drawing.Size(69, 13);
			this.SearchTimeCtl.TabIndex = 212;
			this.SearchTimeCtl.Text = "Search time: 0";
			// 
			// SearchHitCountCtl
			// 
			this.SearchHitCountCtl.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Horizontal;
			this.SearchHitCountCtl.Location = new System.Drawing.Point(116, 6);
			this.SearchHitCountCtl.Name = "SearchHitCountCtl";
			this.SearchHitCountCtl.Size = new System.Drawing.Size(61, 13);
			this.SearchHitCountCtl.TabIndex = 213;
			this.SearchHitCountCtl.Text = "Key count: 0";
			// 
			// Timer
			// 
			this.Timer.Enabled = true;
			this.Timer.Interval = 1000;
			this.Timer.Tick += new System.EventHandler(this.Timer_Tick);
			// 
			// MetatableRowsCtl
			// 
			this.MetatableRowsCtl.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Horizontal;
			this.MetatableRowsCtl.Location = new System.Drawing.Point(229, 6);
			this.MetatableRowsCtl.Name = "MetatableRowsCtl";
			this.MetatableRowsCtl.Size = new System.Drawing.Size(87, 13);
			this.MetatableRowsCtl.TabIndex = 216;
			this.MetatableRowsCtl.Text = "Metatable rows: 0";
			// 
			// panelControl1
			// 
			this.panelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.panelControl1.Controls.Add(this.SearchTimeCtl);
			this.panelControl1.Controls.Add(this.SearchHitCountCtl);
			this.panelControl1.Location = new System.Drawing.Point(190, 696);
			this.panelControl1.Name = "panelControl1";
			this.panelControl1.Size = new System.Drawing.Size(223, 25);
			this.panelControl1.TabIndex = 219;
			// 
			// panelControl2
			// 
			this.panelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.panelControl2.Controls.Add(this.RetrievalTimeCtl);
			this.panelControl2.Controls.Add(this.OracleRowsCtl);
			this.panelControl2.Controls.Add(this.GridRowsCtl);
			this.panelControl2.Controls.Add(this.MetatableRowsCtl);
			this.panelControl2.Location = new System.Drawing.Point(431, 696);
			this.panelControl2.Name = "panelControl2";
			this.panelControl2.Size = new System.Drawing.Size(481, 25);
			this.panelControl2.TabIndex = 220;
			// 
			// GridRowsCtl
			// 
			this.GridRowsCtl.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Horizontal;
			this.GridRowsCtl.Location = new System.Drawing.Point(130, 6);
			this.GridRowsCtl.Name = "GridRowsCtl";
			this.GridRowsCtl.Size = new System.Drawing.Size(58, 13);
			this.GridRowsCtl.TabIndex = 215;
			this.GridRowsCtl.Text = "Grid rows: 0";
			// 
			// OracleRowsCtl
			// 
			this.OracleRowsCtl.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Horizontal;
			this.OracleRowsCtl.Location = new System.Drawing.Point(357, 6);
			this.OracleRowsCtl.Name = "OracleRowsCtl";
			this.OracleRowsCtl.Size = new System.Drawing.Size(70, 13);
			this.OracleRowsCtl.TabIndex = 217;
			this.OracleRowsCtl.Text = "DB Server rows: 0";
			// 
			// RetrievalTimeCtl
			// 
			this.RetrievalTimeCtl.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Horizontal;
			this.RetrievalTimeCtl.Location = new System.Drawing.Point(10, 6);
			this.RetrievalTimeCtl.Name = "RetrievalTimeCtl";
			this.RetrievalTimeCtl.Size = new System.Drawing.Size(79, 13);
			this.RetrievalTimeCtl.TabIndex = 214;
			this.RetrievalTimeCtl.Text = "Retrieval time: 0";
			// 
			// QueryEngineStatsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(919, 725);
			this.Controls.Add(this.panelControl2);
			this.Controls.Add(this.panelControl1);
			this.Controls.Add(this.ShowSqlButton);
			this.Controls.Add(this.Grid);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "QueryEngineStatsForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "Query Engine Statistics";
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
			this.panelControl1.ResumeLayout(false);
			this.panelControl1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
			this.panelControl2.ResumeLayout(false);
			this.panelControl2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraGrid.GridControl Grid;
		private DevExpress.XtraGrid.Views.Grid.GridView GridView;
		private DevExpress.XtraGrid.Columns.GridColumn MetatableName;
		private DevExpress.XtraGrid.Columns.GridColumn MetatableLabel;
		private DevExpress.XtraGrid.Columns.GridColumn ColumnCount;
		private DevExpress.XtraGrid.Columns.GridColumn ColumnsSelected;
		private DevExpress.XtraGrid.Columns.GridColumn CriteriaCount;
		private DevExpress.XtraGrid.Columns.GridColumn RowsRetrieved;
		private DevExpress.XtraGrid.Columns.GridColumn OracleTime;
		private DevExpress.XtraGrid.Columns.GridColumn BrokerCol;
		private DevExpress.XtraGrid.Columns.GridColumn MultipivotCol;
		private DevExpress.XtraGrid.Columns.GridColumn Multipivot;
		private DevExpress.XtraGrid.Columns.GridColumn NextRowCount;
		private DevExpress.XtraGrid.Columns.GridColumn ReadRowCount;
		private DevExpress.XtraEditors.SimpleButton ShowSqlButton;
		private DevExpress.XtraEditors.LabelControl SearchTimeCtl;
		private DevExpress.XtraEditors.LabelControl SearchHitCountCtl;
		private System.Windows.Forms.Timer Timer;
		private DevExpress.XtraEditors.LabelControl MetatableRowsCtl;
		private DevExpress.XtraEditors.PanelControl panelControl1;
		private DevExpress.XtraEditors.PanelControl panelControl2;
		private DevExpress.XtraEditors.LabelControl RetrievalTimeCtl;
		private DevExpress.XtraEditors.LabelControl OracleRowsCtl;
		private DevExpress.XtraEditors.LabelControl GridRowsCtl;
	}
}