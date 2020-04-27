namespace Mobius.ClientComponents
{
	partial class SessionMonitorDialog
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
			this.repositoryItemMemoEdit2 = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
			this.repositoryItemPictureEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit();
			this.repositoryItemCheckEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
			this.repositoryItemMemoEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
			this.Label = new DevExpress.XtraEditors.LabelControl();
			this.Grid = new DevExpress.XtraGrid.GridControl();
			this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
			this.SessionIdCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.IsNonNativeCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.UserIdCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.UserNameCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.CreationDtCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.IdleTimeCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.ProcessIdCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.CpuTimeCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.MemoryCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.ThreadsCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.HandlesCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.Timer = new System.Windows.Forms.Timer(this.components);
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemMemoEdit2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemPictureEdit1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemMemoEdit1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// repositoryItemMemoEdit2
			// 
			this.repositoryItemMemoEdit2.Name = "repositoryItemMemoEdit2";
			// 
			// repositoryItemPictureEdit1
			// 
			this.repositoryItemPictureEdit1.Name = "repositoryItemPictureEdit1";
			// 
			// repositoryItemCheckEdit1
			// 
			this.repositoryItemCheckEdit1.AutoHeight = false;
			this.repositoryItemCheckEdit1.Name = "repositoryItemCheckEdit1";
			// 
			// repositoryItemMemoEdit1
			// 
			this.repositoryItemMemoEdit1.Name = "repositoryItemMemoEdit1";
			// 
			// Label
			// 
			this.Label.Location = new System.Drawing.Point(5, 6);
			this.Label.Name = "Label";
			this.Label.Size = new System.Drawing.Size(187, 13);
			this.Label.TabIndex = 0;
			this.Label.Text = "Server: XYZ, Version: 1.2.3, Count: 99";
			// 
			// Grid
			// 
			this.Grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Grid.Location = new System.Drawing.Point(0, 25);
			this.Grid.MainView = this.gridView1;
			this.Grid.Name = "Grid";
			this.Grid.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemMemoEdit2,
            this.repositoryItemPictureEdit1,
            this.repositoryItemCheckEdit1,
            this.repositoryItemMemoEdit1});
			this.Grid.Size = new System.Drawing.Size(722, 489);
			this.Grid.TabIndex = 210;
			this.Grid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
			// 
			// gridView1
			// 
			this.gridView1.Appearance.HeaderPanel.Options.UseTextOptions = true;
			this.gridView1.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.gridView1.Appearance.HeaderPanel.TextOptions.Trimming = DevExpress.Utils.Trimming.None;
			this.gridView1.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.gridView1.ColumnPanelRowHeight = 34;
			this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.SessionIdCol,
            this.IsNonNativeCol,
            this.UserIdCol,
            this.UserNameCol,
            this.CreationDtCol,
            this.IdleTimeCol,
            this.ProcessIdCol,
            this.CpuTimeCol,
            this.MemoryCol,
            this.ThreadsCol,
            this.HandlesCol});
			this.gridView1.GridControl = this.Grid;
			this.gridView1.IndicatorWidth = 26;
			this.gridView1.Name = "gridView1";
			this.gridView1.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;
			this.gridView1.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False;
			this.gridView1.OptionsBehavior.Editable = false;
			this.gridView1.OptionsCustomization.AllowColumnMoving = false;
			this.gridView1.OptionsCustomization.AllowFilter = false;
			this.gridView1.OptionsCustomization.AllowGroup = false;
			this.gridView1.OptionsCustomization.AllowRowSizing = true;
			this.gridView1.OptionsMenu.EnableColumnMenu = false;
			this.gridView1.OptionsMenu.EnableFooterMenu = false;
			this.gridView1.OptionsMenu.EnableGroupPanelMenu = false;
			this.gridView1.OptionsSelection.MultiSelect = true;
			this.gridView1.OptionsView.ColumnAutoWidth = false;
			this.gridView1.OptionsView.RowAutoHeight = true;
			this.gridView1.OptionsView.ShowGroupPanel = false;
			// 
			// SessionIdCol
			// 
			this.SessionIdCol.AppearanceHeader.Options.UseTextOptions = true;
			this.SessionIdCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.SessionIdCol.AppearanceHeader.TextOptions.Trimming = DevExpress.Utils.Trimming.None;
			this.SessionIdCol.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.SessionIdCol.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.SessionIdCol.Caption = "Session Id";
			this.SessionIdCol.ColumnEdit = this.repositoryItemMemoEdit1;
			this.SessionIdCol.FieldName = "SessionIdCol";
			this.SessionIdCol.Name = "SessionIdCol";
			this.SessionIdCol.Visible = true;
			this.SessionIdCol.VisibleIndex = 0;
			this.SessionIdCol.Width = 46;
			// 
			// IsNonNativeCol
			// 
			this.IsNonNativeCol.Caption = " ";
			this.IsNonNativeCol.ColumnEdit = this.repositoryItemMemoEdit1;
			this.IsNonNativeCol.FieldName = "IsNonNativeCol";
			this.IsNonNativeCol.Name = "IsNonNativeCol";
			this.IsNonNativeCol.Visible = true;
			this.IsNonNativeCol.VisibleIndex = 1;
			this.IsNonNativeCol.Width = 20;
			// 
			// UserIdCol
			// 
			this.UserIdCol.Caption = "User Id";
			this.UserIdCol.ColumnEdit = this.repositoryItemMemoEdit1;
			this.UserIdCol.FieldName = "UserIdCol";
			this.UserIdCol.Name = "UserIdCol";
			this.UserIdCol.Visible = true;
			this.UserIdCol.VisibleIndex = 2;
			this.UserIdCol.Width = 80;
			// 
			// UserNameCol
			// 
			this.UserNameCol.Caption = "User Name";
			this.UserNameCol.ColumnEdit = this.repositoryItemMemoEdit1;
			this.UserNameCol.FieldName = "UserNameCol";
			this.UserNameCol.Name = "UserNameCol";
			this.UserNameCol.Visible = true;
			this.UserNameCol.VisibleIndex = 3;
			this.UserNameCol.Width = 138;
			// 
			// CreationDtCol
			// 
			this.CreationDtCol.Caption = "Age";
			this.CreationDtCol.ColumnEdit = this.repositoryItemMemoEdit1;
			this.CreationDtCol.FieldName = "CreationDtCol";
			this.CreationDtCol.Name = "CreationDtCol";
			this.CreationDtCol.Visible = true;
			this.CreationDtCol.VisibleIndex = 4;
			this.CreationDtCol.Width = 65;
			// 
			// IdleTimeCol
			// 
			this.IdleTimeCol.Caption = "Idle Time";
			this.IdleTimeCol.ColumnEdit = this.repositoryItemMemoEdit1;
			this.IdleTimeCol.FieldName = "IdleTimeCol";
			this.IdleTimeCol.Name = "IdleTimeCol";
			this.IdleTimeCol.Visible = true;
			this.IdleTimeCol.VisibleIndex = 5;
			this.IdleTimeCol.Width = 62;
			// 
			// ProcessIdCol
			// 
			this.ProcessIdCol.AppearanceCell.Options.UseTextOptions = true;
			this.ProcessIdCol.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
			this.ProcessIdCol.Caption = "Process Id";
			this.ProcessIdCol.ColumnEdit = this.repositoryItemMemoEdit1;
			this.ProcessIdCol.FieldName = "ProcessIdCol";
			this.ProcessIdCol.Name = "ProcessIdCol";
			this.ProcessIdCol.Visible = true;
			this.ProcessIdCol.VisibleIndex = 6;
			this.ProcessIdCol.Width = 59;
			// 
			// CpuTimeCol
			// 
			this.CpuTimeCol.AppearanceCell.Options.UseTextOptions = true;
			this.CpuTimeCol.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
			this.CpuTimeCol.Caption = "CPU Time (s)";
			this.CpuTimeCol.ColumnEdit = this.repositoryItemMemoEdit1;
			this.CpuTimeCol.FieldName = "CpuTimeCol";
			this.CpuTimeCol.Name = "CpuTimeCol";
			this.CpuTimeCol.Visible = true;
			this.CpuTimeCol.VisibleIndex = 7;
			this.CpuTimeCol.Width = 60;
			// 
			// MemoryCol
			// 
			this.MemoryCol.AppearanceCell.Options.UseTextOptions = true;
			this.MemoryCol.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
			this.MemoryCol.Caption = "Memory (Mb)";
			this.MemoryCol.ColumnEdit = this.repositoryItemMemoEdit1;
			this.MemoryCol.FieldName = "MemoryCol";
			this.MemoryCol.Name = "MemoryCol";
			this.MemoryCol.Visible = true;
			this.MemoryCol.VisibleIndex = 8;
			this.MemoryCol.Width = 58;
			// 
			// ThreadsCol
			// 
			this.ThreadsCol.AppearanceCell.Options.UseTextOptions = true;
			this.ThreadsCol.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
			this.ThreadsCol.Caption = "Threads";
			this.ThreadsCol.ColumnEdit = this.repositoryItemMemoEdit1;
			this.ThreadsCol.FieldName = "ThreadsCol";
			this.ThreadsCol.Name = "ThreadsCol";
			this.ThreadsCol.Visible = true;
			this.ThreadsCol.VisibleIndex = 9;
			this.ThreadsCol.Width = 50;
			// 
			// HandlesCol
			// 
			this.HandlesCol.AppearanceCell.Options.UseTextOptions = true;
			this.HandlesCol.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
			this.HandlesCol.Caption = "Handles";
			this.HandlesCol.ColumnEdit = this.repositoryItemMemoEdit1;
			this.HandlesCol.FieldName = "HandlesCol";
			this.HandlesCol.Name = "HandlesCol";
			this.HandlesCol.Visible = true;
			this.HandlesCol.VisibleIndex = 10;
			this.HandlesCol.Width = 50;
			// 
			// Timer
			// 
			this.Timer.Interval = 60000;
			this.Timer.Tick += new System.EventHandler(this.Timer_Tick);
			// 
			// SessionMonitorDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(722, 515);
			this.Controls.Add(this.Grid);
			this.Controls.Add(this.Label);
			this.MinimizeBox = false;
			this.Name = "SessionMonitorDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Mobius User Sessions";
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemMemoEdit2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemPictureEdit1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemMemoEdit1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DevExpress.XtraEditors.LabelControl Label;
		private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
		private DevExpress.XtraGrid.Columns.GridColumn SessionIdCol;
		private DevExpress.XtraGrid.Columns.GridColumn UserIdCol;
		private DevExpress.XtraGrid.Columns.GridColumn UserNameCol;
		private DevExpress.XtraGrid.Columns.GridColumn CreationDtCol;
		private DevExpress.XtraGrid.GridControl Grid;
		private DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit repositoryItemMemoEdit1;
		private DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit repositoryItemMemoEdit2;
		private DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit repositoryItemPictureEdit1;
		private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repositoryItemCheckEdit1;
		private DevExpress.XtraGrid.Columns.GridColumn ProcessIdCol;
		private DevExpress.XtraGrid.Columns.GridColumn CpuTimeCol;
		private DevExpress.XtraGrid.Columns.GridColumn MemoryCol;
		private DevExpress.XtraGrid.Columns.GridColumn ThreadsCol;
		private DevExpress.XtraGrid.Columns.GridColumn HandlesCol;
		private DevExpress.XtraGrid.Columns.GridColumn IsNonNativeCol;
		private DevExpress.XtraGrid.Columns.GridColumn IdleTimeCol;
		private System.Windows.Forms.Timer Timer;
	}
}