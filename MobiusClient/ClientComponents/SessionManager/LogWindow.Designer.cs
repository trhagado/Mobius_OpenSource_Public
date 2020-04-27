namespace Mobius.ClientComponents
{
	partial class LogWindow
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
			this.Grid = new DevExpress.XtraGrid.GridControl();
			this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
			this.TimeColumn = new DevExpress.XtraGrid.Columns.GridColumn();
			this.T0PlusColumn = new DevExpress.XtraGrid.Columns.GridColumn();
			this.TDeltaColumn = new DevExpress.XtraGrid.Columns.GridColumn();
			this.MsgColumn = new DevExpress.XtraGrid.Columns.GridColumn();
			this.repositoryItemMemoEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
			this.CopyAll = new DevExpress.XtraEditors.SimpleButton();
			this.ClearGrid = new DevExpress.XtraEditors.SimpleButton();
			this.CloseForm = new DevExpress.XtraEditors.SimpleButton();
			this.Timer = new System.Windows.Forms.Timer(this.components);
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemMemoEdit1)).BeginInit();
			this.SuspendLayout();
			// 
			// Grid
			// 
			this.Grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.Grid.Location = new System.Drawing.Point(5, 6);
			this.Grid.MainView = this.gridView1;
			this.Grid.Name = "Grid";
			this.Grid.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemMemoEdit1});
			this.Grid.Size = new System.Drawing.Size(822, 549);
			this.Grid.TabIndex = 0;
			this.Grid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
			// 
			// gridView1
			// 
			this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.TimeColumn,
            this.T0PlusColumn,
            this.TDeltaColumn,
            this.MsgColumn});
			this.gridView1.GridControl = this.Grid;
			this.gridView1.IndicatorWidth = 40;
			this.gridView1.Name = "gridView1";
			this.gridView1.OptionsView.ShowGroupPanel = false;
			// 
			// TimeColumn
			// 
			this.TimeColumn.AppearanceHeader.Options.UseTextOptions = true;
			this.TimeColumn.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.TimeColumn.Caption = "Time";
			this.TimeColumn.FieldName = "TimeField";
			this.TimeColumn.MaxWidth = 85;
			this.TimeColumn.MinWidth = 10;
			this.TimeColumn.Name = "TimeColumn";
			this.TimeColumn.OptionsColumn.AllowEdit = false;
			this.TimeColumn.OptionsColumn.AllowFocus = false;
			this.TimeColumn.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
			this.TimeColumn.Visible = true;
			this.TimeColumn.VisibleIndex = 0;
			this.TimeColumn.Width = 10;
			// 
			// T0PlusColumn
			// 
			this.T0PlusColumn.AppearanceHeader.Options.UseTextOptions = true;
			this.T0PlusColumn.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.T0PlusColumn.Caption = "T0+Plus";
			this.T0PlusColumn.FieldName = "T0PlusField";
			this.T0PlusColumn.MaxWidth = 85;
			this.T0PlusColumn.MinWidth = 10;
			this.T0PlusColumn.Name = "T0PlusColumn";
			this.T0PlusColumn.OptionsColumn.AllowEdit = false;
			this.T0PlusColumn.OptionsColumn.AllowFocus = false;
			this.T0PlusColumn.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
			this.T0PlusColumn.Visible = true;
			this.T0PlusColumn.VisibleIndex = 1;
			this.T0PlusColumn.Width = 85;
			// 
			// TDeltaColumn
			// 
			this.TDeltaColumn.AppearanceHeader.Options.UseTextOptions = true;
			this.TDeltaColumn.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.TDeltaColumn.Caption = "T-Delta";
			this.TDeltaColumn.FieldName = "TDeltaField";
			this.TDeltaColumn.MaxWidth = 85;
			this.TDeltaColumn.MinWidth = 10;
			this.TDeltaColumn.Name = "TDeltaColumn";
			this.TDeltaColumn.OptionsColumn.AllowEdit = false;
			this.TDeltaColumn.OptionsColumn.AllowFocus = false;
			this.TDeltaColumn.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
			this.TDeltaColumn.Visible = true;
			this.TDeltaColumn.VisibleIndex = 2;
			this.TDeltaColumn.Width = 85;
			// 
			// MsgColumn
			// 
			this.MsgColumn.AppearanceHeader.Options.UseTextOptions = true;
			this.MsgColumn.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.MsgColumn.Caption = "Message";
			this.MsgColumn.ColumnEdit = this.repositoryItemMemoEdit1;
			this.MsgColumn.FieldName = "MessageField";
			this.MsgColumn.Name = "MsgColumn";
			this.MsgColumn.OptionsColumn.AllowEdit = false;
			this.MsgColumn.OptionsColumn.AllowFocus = false;
			this.MsgColumn.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
			this.MsgColumn.Visible = true;
			this.MsgColumn.VisibleIndex = 3;
			this.MsgColumn.Width = 648;
			// 
			// repositoryItemMemoEdit1
			// 
			this.repositoryItemMemoEdit1.Name = "repositoryItemMemoEdit1";
			// 
			// CopyAll
			// 
			this.CopyAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CopyAll.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CopyAll.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.CopyAll.Appearance.Options.UseFont = true;
			this.CopyAll.Appearance.Options.UseForeColor = true;
			this.CopyAll.Cursor = System.Windows.Forms.Cursors.Default;
			this.CopyAll.Location = new System.Drawing.Point(5, 561);
			this.CopyAll.Name = "CopyAll";
			this.CopyAll.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CopyAll.Size = new System.Drawing.Size(72, 24);
			this.CopyAll.TabIndex = 48;
			this.CopyAll.Text = "Copy All";
			this.CopyAll.Click += new System.EventHandler(this.CopyAll_Click);
			// 
			// ClearGrid
			// 
			this.ClearGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ClearGrid.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ClearGrid.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ClearGrid.Appearance.Options.UseFont = true;
			this.ClearGrid.Appearance.Options.UseForeColor = true;
			this.ClearGrid.Cursor = System.Windows.Forms.Cursors.Default;
			this.ClearGrid.Location = new System.Drawing.Point(83, 561);
			this.ClearGrid.Name = "ClearGrid";
			this.ClearGrid.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ClearGrid.Size = new System.Drawing.Size(72, 24);
			this.ClearGrid.TabIndex = 49;
			this.ClearGrid.Text = "Clear";
			this.ClearGrid.Click += new System.EventHandler(this.ClearGrid_Click);
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
			this.CloseForm.Location = new System.Drawing.Point(754, 561);
			this.CloseForm.Name = "CloseForm";
			this.CloseForm.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CloseForm.Size = new System.Drawing.Size(73, 24);
			this.CloseForm.TabIndex = 50;
			this.CloseForm.Text = "Close";
			this.CloseForm.Click += new System.EventHandler(this.CloseForm_Click);
			// 
			// Timer
			// 
			this.Timer.Enabled = true;
			this.Timer.Tick += new System.EventHandler(this.Timer_Tick);
			// 
			// LogWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(831, 590);
			this.Controls.Add(this.CloseForm);
			this.Controls.Add(this.ClearGrid);
			this.Controls.Add(this.CopyAll);
			this.Controls.Add(this.Grid);
			this.Name = "LogWindow";
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "Log Window";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LogWindow_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemMemoEdit1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraGrid.GridControl Grid;
		private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
		private DevExpress.XtraGrid.Columns.GridColumn TimeColumn;
		private DevExpress.XtraGrid.Columns.GridColumn MsgColumn;
		private DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit repositoryItemMemoEdit1;
		private DevExpress.XtraGrid.Columns.GridColumn T0PlusColumn;
		private DevExpress.XtraGrid.Columns.GridColumn TDeltaColumn;
		public DevExpress.XtraEditors.SimpleButton CopyAll;
		public DevExpress.XtraEditors.SimpleButton ClearGrid;
		public DevExpress.XtraEditors.SimpleButton CloseForm;
		private System.Windows.Forms.Timer Timer;
	}
}