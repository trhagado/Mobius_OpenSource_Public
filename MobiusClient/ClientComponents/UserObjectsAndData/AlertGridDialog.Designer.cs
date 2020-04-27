namespace Mobius.ClientComponents
{
	partial class AlertGridDialog
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
            this.Help = new DevExpress.XtraEditors.SimpleButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.OpenQuery = new DevExpress.XtraEditors.SimpleButton();
            this.DeleteAlert = new DevExpress.XtraEditors.SimpleButton();
            this.EditAlert = new DevExpress.XtraEditors.SimpleButton();
            this.NewAlert = new DevExpress.XtraEditors.SimpleButton();
            this.CloseForm = new DevExpress.XtraEditors.SimpleButton();
            this.Grid = new DevExpress.XtraGrid.GridControl();
            this.GridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.AlertId = new DevExpress.XtraGrid.Columns.GridColumn();
            this.QueryName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.QueryLastModified = new DevExpress.XtraGrid.Columns.GridColumn();
            this.CheckInterval = new DevExpress.XtraGrid.Columns.GridColumn();
            this.AlertLastChecked = new DevExpress.XtraGrid.Columns.GridColumn();
            this.LastCheckThatFoundNewData = new DevExpress.XtraGrid.Columns.GridColumn();
            this.LastCheckExecutionTime = new DevExpress.XtraGrid.Columns.GridColumn();
            this.NewCompounds = new DevExpress.XtraGrid.Columns.GridColumn();
            this.ChangedCompounds = new DevExpress.XtraGrid.Columns.GridColumn();
            this.TotalCompounds = new DevExpress.XtraGrid.Columns.GridColumn();
            this.NewChangedRows = new DevExpress.XtraGrid.Columns.GridColumn();
            this.TotalRows = new DevExpress.XtraGrid.Columns.GridColumn();
            this.SendTo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.AlertOwnerCol2 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.QueryId = new DevExpress.XtraGrid.Columns.GridColumn();
            this.RunAlert = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GridView)).BeginInit();
            this.SuspendLayout();
            // 
            // Help
            // 
            this.Help.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Help.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Help.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Help.Appearance.Options.UseFont = true;
            this.Help.Appearance.Options.UseForeColor = true;
            this.Help.Cursor = System.Windows.Forms.Cursors.Default;
            this.Help.Location = new System.Drawing.Point(764, 502);
            this.Help.Name = "Help";
            this.Help.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Help.Size = new System.Drawing.Size(60, 22);
            this.Help.TabIndex = 52;
            this.Help.Text = "Help";
            this.Help.Click += new System.EventHandler(this.Help_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Location = new System.Drawing.Point(318, 496);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(2, 32);
            this.groupBox1.TabIndex = 51;
            this.groupBox1.TabStop = false;
            // 
            // OpenQuery
            // 
            this.OpenQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OpenQuery.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OpenQuery.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.OpenQuery.Appearance.Options.UseFont = true;
            this.OpenQuery.Appearance.Options.UseForeColor = true;
            this.OpenQuery.Cursor = System.Windows.Forms.Cursors.Default;
            this.OpenQuery.Location = new System.Drawing.Point(326, 502);
            this.OpenQuery.Name = "OpenQuery";
            this.OpenQuery.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.OpenQuery.Size = new System.Drawing.Size(76, 22);
            this.OpenQuery.TabIndex = 50;
            this.OpenQuery.Text = "Open Query";
            this.OpenQuery.Click += new System.EventHandler(this.OpenQuery_Click);
            // 
            // DeleteAlert
            // 
            this.DeleteAlert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DeleteAlert.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DeleteAlert.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.DeleteAlert.Appearance.Options.UseFont = true;
            this.DeleteAlert.Appearance.Options.UseForeColor = true;
            this.DeleteAlert.Cursor = System.Windows.Forms.Cursors.Default;
            this.DeleteAlert.Location = new System.Drawing.Point(238, 502);
            this.DeleteAlert.Name = "DeleteAlert";
            this.DeleteAlert.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.DeleteAlert.Size = new System.Drawing.Size(74, 22);
            this.DeleteAlert.TabIndex = 49;
            this.DeleteAlert.Text = "Delete Alert";
            this.DeleteAlert.Click += new System.EventHandler(this.DeleteAlert_Click);
            // 
            // EditAlert
            // 
            this.EditAlert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.EditAlert.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EditAlert.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.EditAlert.Appearance.Options.UseFont = true;
            this.EditAlert.Appearance.Options.UseForeColor = true;
            this.EditAlert.Cursor = System.Windows.Forms.Cursors.Default;
            this.EditAlert.Location = new System.Drawing.Point(81, 502);
            this.EditAlert.Name = "EditAlert";
            this.EditAlert.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.EditAlert.Size = new System.Drawing.Size(72, 22);
            this.EditAlert.TabIndex = 48;
            this.EditAlert.Text = "Edit Alert...";
            this.EditAlert.Click += new System.EventHandler(this.EditAlert_Click);
            // 
            // NewAlert
            // 
            this.NewAlert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NewAlert.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NewAlert.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.NewAlert.Appearance.Options.UseFont = true;
            this.NewAlert.Appearance.Options.UseForeColor = true;
            this.NewAlert.Cursor = System.Windows.Forms.Cursors.Default;
            this.NewAlert.Location = new System.Drawing.Point(3, 502);
            this.NewAlert.Name = "NewAlert";
            this.NewAlert.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.NewAlert.Size = new System.Drawing.Size(72, 22);
            this.NewAlert.TabIndex = 47;
            this.NewAlert.Text = "New Alert...";
            this.NewAlert.Click += new System.EventHandler(this.NewAlert_Click);
            // 
            // CloseForm
            // 
            this.CloseForm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseForm.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CloseForm.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.CloseForm.Appearance.Options.UseFont = true;
            this.CloseForm.Appearance.Options.UseForeColor = true;
            this.CloseForm.Cursor = System.Windows.Forms.Cursors.Default;
            this.CloseForm.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CloseForm.Location = new System.Drawing.Point(697, 502);
            this.CloseForm.Name = "CloseForm";
            this.CloseForm.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.CloseForm.Size = new System.Drawing.Size(60, 22);
            this.CloseForm.TabIndex = 46;
            this.CloseForm.Text = "Close";
            // 
            // Grid
            // 
            this.Grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Grid.Cursor = System.Windows.Forms.Cursors.Default;
            this.Grid.Location = new System.Drawing.Point(0, 0);
            this.Grid.MainView = this.GridView;
            this.Grid.Name = "Grid";
            this.Grid.Size = new System.Drawing.Size(828, 496);
            this.Grid.TabIndex = 209;
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
            this.AlertId,
            this.QueryName,
            this.QueryLastModified,
            this.CheckInterval,
            this.AlertLastChecked,
            this.LastCheckThatFoundNewData,
            this.LastCheckExecutionTime,
            this.NewCompounds,
            this.ChangedCompounds,
            this.TotalCompounds,
            this.NewChangedRows,
            this.TotalRows,
            this.SendTo,
            this.AlertOwnerCol2,
            this.QueryId});
            this.GridView.GridControl = this.Grid;
            this.GridView.IndicatorWidth = 26;
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
            // 
            // AlertId
            // 
            this.AlertId.AppearanceHeader.Options.UseTextOptions = true;
            this.AlertId.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.AlertId.AppearanceHeader.TextOptions.Trimming = DevExpress.Utils.Trimming.None;
            this.AlertId.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
            this.AlertId.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.AlertId.Caption = "Alert Id";
            this.AlertId.FieldName = "AlertId";
            this.AlertId.Name = "AlertId";
            this.AlertId.Visible = true;
            this.AlertId.VisibleIndex = 0;
            this.AlertId.Width = 54;
            // 
            // QueryName
            // 
            this.QueryName.Caption = "Query Name";
            this.QueryName.FieldName = "QueryName";
            this.QueryName.Name = "QueryName";
            this.QueryName.Visible = true;
            this.QueryName.VisibleIndex = 1;
            this.QueryName.Width = 183;
            // 
            // QueryLastModified
            // 
            this.QueryLastModified.Caption = "Query Last Modified";
            this.QueryLastModified.FieldName = "QueryLastModified";
            this.QueryLastModified.Name = "QueryLastModified";
            this.QueryLastModified.Visible = true;
            this.QueryLastModified.VisibleIndex = 2;
            this.QueryLastModified.Width = 68;
            // 
            // CheckInterval
            // 
            this.CheckInterval.Caption = "Check Interval (Days)";
            this.CheckInterval.FieldName = "CheckInterval";
            this.CheckInterval.Name = "CheckInterval";
            this.CheckInterval.Visible = true;
            this.CheckInterval.VisibleIndex = 3;
            this.CheckInterval.Width = 54;
            // 
            // AlertLastChecked
            // 
            this.AlertLastChecked.Caption = "Alert Last Checked";
            this.AlertLastChecked.FieldName = "AlertLastChecked";
            this.AlertLastChecked.Name = "AlertLastChecked";
            this.AlertLastChecked.Visible = true;
            this.AlertLastChecked.VisibleIndex = 4;
            this.AlertLastChecked.Width = 68;
            // 
            // LastCheckThatFoundNewData
            // 
            this.LastCheckThatFoundNewData.Caption = "Last Check that Found New Data";
            this.LastCheckThatFoundNewData.FieldName = "LastCheckThatFoundNewData";
            this.LastCheckThatFoundNewData.Name = "LastCheckThatFoundNewData";
            this.LastCheckThatFoundNewData.Visible = true;
            this.LastCheckThatFoundNewData.VisibleIndex = 5;
            this.LastCheckThatFoundNewData.Width = 68;
            // 
            // LastCheckExecutionTime
            // 
            this.LastCheckExecutionTime.Caption = "Execution Time";
            this.LastCheckExecutionTime.FieldName = "LastCheckExecutionTime";
            this.LastCheckExecutionTime.Name = "LastCheckExecutionTime";
            this.LastCheckExecutionTime.Visible = true;
            this.LastCheckExecutionTime.VisibleIndex = 6;
            this.LastCheckExecutionTime.Width = 60;
            // 
            // NewCompounds
            // 
            this.NewCompounds.Caption = "New Com- pounds";
            this.NewCompounds.FieldName = "NewCompounds";
            this.NewCompounds.Name = "NewCompounds";
            this.NewCompounds.Visible = true;
            this.NewCompounds.VisibleIndex = 7;
            this.NewCompounds.Width = 54;
            // 
            // ChangedCompounds
            // 
            this.ChangedCompounds.Caption = "Changed Com- pounds";
            this.ChangedCompounds.FieldName = "ChangedCompounds";
            this.ChangedCompounds.Name = "ChangedCompounds";
            this.ChangedCompounds.Visible = true;
            this.ChangedCompounds.VisibleIndex = 8;
            this.ChangedCompounds.Width = 54;
            // 
            // TotalCompounds
            // 
            this.TotalCompounds.Caption = "Total Com- pounds";
            this.TotalCompounds.FieldName = "TotalCompounds";
            this.TotalCompounds.Name = "TotalCompounds";
            this.TotalCompounds.Visible = true;
            this.TotalCompounds.VisibleIndex = 9;
            this.TotalCompounds.Width = 54;
            // 
            // NewChangedRows
            // 
            this.NewChangedRows.Caption = "New Rows";
            this.NewChangedRows.FieldName = "NewChangedRows";
            this.NewChangedRows.Name = "NewChangedRows";
            this.NewChangedRows.Visible = true;
            this.NewChangedRows.VisibleIndex = 10;
            this.NewChangedRows.Width = 54;
            // 
            // TotalRows
            // 
            this.TotalRows.Caption = "Total Rows";
            this.TotalRows.FieldName = "TotalRows";
            this.TotalRows.Name = "TotalRows";
            this.TotalRows.Visible = true;
            this.TotalRows.VisibleIndex = 11;
            this.TotalRows.Width = 54;
            // 
            // SendTo
            // 
            this.SendTo.Caption = "Send To";
            this.SendTo.FieldName = "SendTo";
            this.SendTo.Name = "SendTo";
            this.SendTo.Visible = true;
            this.SendTo.VisibleIndex = 12;
            this.SendTo.Width = 176;
            // 
            // Owner
            // 
            this.AlertOwnerCol2.Caption = "Owner";
            this.AlertOwnerCol2.FieldName = "Owner";
            this.AlertOwnerCol2.Name = "Owner";
            this.AlertOwnerCol2.Visible = true;
            this.AlertOwnerCol2.VisibleIndex = 13;
            this.AlertOwnerCol2.Width = 118;
            // 
            // QueryId
            // 
            this.QueryId.Caption = "Query Id";
            this.QueryId.FieldName = "QueryId";
            this.QueryId.Name = "QueryId";
            this.QueryId.Visible = true;
            this.QueryId.VisibleIndex = 14;
            this.QueryId.Width = 54;
            // 
            // RunAlert
            // 
            this.RunAlert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RunAlert.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RunAlert.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.RunAlert.Appearance.Options.UseFont = true;
            this.RunAlert.Appearance.Options.UseForeColor = true;
            this.RunAlert.Cursor = System.Windows.Forms.Cursors.Default;
            this.RunAlert.Location = new System.Drawing.Point(159, 502);
            this.RunAlert.Name = "RunAlert";
            this.RunAlert.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.RunAlert.Size = new System.Drawing.Size(72, 22);
            this.RunAlert.TabIndex = 210;
            this.RunAlert.Text = "Run Alert";
            this.RunAlert.ToolTip = "Forces the selected alert to run now";
            this.RunAlert.Click += new System.EventHandler(this.RunAlert_Click);
            // 
            // AlertGridDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(828, 529);
            this.Controls.Add(this.RunAlert);
            this.Controls.Add(this.Grid);
            this.Controls.Add(this.Help);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.OpenQuery);
            this.Controls.Add(this.DeleteAlert);
            this.Controls.Add(this.EditAlert);
            this.Controls.Add(this.NewAlert);
            this.Controls.Add(this.CloseForm);
            this.MinimizeBox = false;
            this.Name = "AlertGridDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Alerts";
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GridView)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraGrid.GridControl Grid;
		public DevExpress.XtraEditors.SimpleButton Help;
		private System.Windows.Forms.GroupBox groupBox1;
		public DevExpress.XtraEditors.SimpleButton OpenQuery;
		public DevExpress.XtraEditors.SimpleButton DeleteAlert;
		public DevExpress.XtraEditors.SimpleButton EditAlert;
		public DevExpress.XtraEditors.SimpleButton NewAlert;
		public DevExpress.XtraEditors.SimpleButton CloseForm;
		private DevExpress.XtraGrid.Views.Grid.GridView GridView;
		private DevExpress.XtraGrid.Columns.GridColumn AlertId;
		private DevExpress.XtraGrid.Columns.GridColumn QueryName;
		private DevExpress.XtraGrid.Columns.GridColumn QueryLastModified;
		private DevExpress.XtraGrid.Columns.GridColumn CheckInterval;
		private DevExpress.XtraGrid.Columns.GridColumn AlertLastChecked;
		private DevExpress.XtraGrid.Columns.GridColumn LastCheckThatFoundNewData;
		private DevExpress.XtraGrid.Columns.GridColumn NewCompounds;
		private DevExpress.XtraGrid.Columns.GridColumn TotalCompounds;
		private DevExpress.XtraGrid.Columns.GridColumn NewChangedRows;
		private DevExpress.XtraGrid.Columns.GridColumn TotalRows;
		private DevExpress.XtraGrid.Columns.GridColumn SendTo;
		private DevExpress.XtraGrid.Columns.GridColumn QueryId;
		private DevExpress.XtraGrid.Columns.GridColumn OwnerCol;
		public DevExpress.XtraEditors.SimpleButton RunAlert;
		private DevExpress.XtraGrid.Columns.GridColumn LastCheckExecutionTime;
		private DevExpress.XtraGrid.Columns.GridColumn AlertOwnerCol;
    private DevExpress.XtraGrid.Columns.GridColumn ChangedCompounds;
    private DevExpress.XtraGrid.Columns.GridColumn AlertOwnerCol2;
	}
}