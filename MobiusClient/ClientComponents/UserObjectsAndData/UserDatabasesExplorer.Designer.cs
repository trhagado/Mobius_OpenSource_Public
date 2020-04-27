namespace Mobius.ClientComponents
{
	partial class UserDatabasesExplorer
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserDatabasesExplorer));
			this.RunDatabaseQuery = new DevExpress.XtraEditors.SimpleButton();
			this.label1 = new DevExpress.XtraEditors.LabelControl();
			this.BuildDatabaseQuery = new DevExpress.XtraEditors.SimpleButton();
			this.DeleteDatabase = new DevExpress.XtraEditors.SimpleButton();
			this.EditDatabase = new DevExpress.XtraEditors.SimpleButton();
			this.NewDatabase = new DevExpress.XtraEditors.SimpleButton();
			this.CloseForm = new DevExpress.XtraEditors.SimpleButton();
			this.Grid = new DevExpress.XtraGrid.GridControl();
			this.GridView = new DevExpress.XtraGrid.Views.Grid.GridView();
			this.UserDatabaseNameCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.PublicCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.repositoryItemCheckEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
			this.MoleculesCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.PredictiveModelsCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.PendingUpdatesCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.CreatedCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.UpdatedCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.OwnerUserNameCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.DatabaseIdCol = new DevExpress.XtraGrid.Columns.GridColumn();
			this.GridContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.EditDatabaseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.DeleteDatabaseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RecalculateModelResultsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RestartPendingUpdatesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.BuildQueryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RunQueryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit1)).BeginInit();
			this.GridContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// RunDatabaseQuery
			// 
			this.RunDatabaseQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RunDatabaseQuery.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RunDatabaseQuery.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.RunDatabaseQuery.Appearance.Options.UseFont = true;
			this.RunDatabaseQuery.Appearance.Options.UseForeColor = true;
			this.RunDatabaseQuery.Cursor = System.Windows.Forms.Cursors.Default;
			this.RunDatabaseQuery.Location = new System.Drawing.Point(297, 349);
			this.RunDatabaseQuery.Name = "RunDatabaseQuery";
			this.RunDatabaseQuery.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RunDatabaseQuery.Size = new System.Drawing.Size(81, 22);
			this.RunDatabaseQuery.TabIndex = 83;
			this.RunDatabaseQuery.Text = "View Data";
			this.toolTip1.SetToolTip(this.RunDatabaseQuery, "Build and run a query to view all data for the selected database");
			this.RunDatabaseQuery.Click += new System.EventHandler(this.RunDatabaseQuery_Click);
			// 
			// label1
			// 
			this.label1.Appearance.Options.UseTextOptions = true;
			this.label1.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.label1.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.label1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.label1.Location = new System.Drawing.Point(5, 6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(617, 45);
			this.label1.TabIndex = 82;
			this.label1.Text = resources.GetString("label1.Text");
			// 
			// BuildDatabaseQuery
			// 
			this.BuildDatabaseQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BuildDatabaseQuery.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.BuildDatabaseQuery.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.BuildDatabaseQuery.Appearance.Options.UseFont = true;
			this.BuildDatabaseQuery.Appearance.Options.UseForeColor = true;
			this.BuildDatabaseQuery.Cursor = System.Windows.Forms.Cursors.Default;
			this.BuildDatabaseQuery.Location = new System.Drawing.Point(210, 349);
			this.BuildDatabaseQuery.Name = "BuildDatabaseQuery";
			this.BuildDatabaseQuery.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.BuildDatabaseQuery.Size = new System.Drawing.Size(81, 22);
			this.BuildDatabaseQuery.TabIndex = 81;
			this.BuildDatabaseQuery.Text = "Build Query";
			this.toolTip1.SetToolTip(this.BuildDatabaseQuery, "Build a query for the selected database");
			this.BuildDatabaseQuery.Click += new System.EventHandler(this.BuildDatabaseQuery_Click);
			// 
			// DeleteDatabase
			// 
			this.DeleteDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DeleteDatabase.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DeleteDatabase.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.DeleteDatabase.Appearance.Options.UseFont = true;
			this.DeleteDatabase.Appearance.Options.UseForeColor = true;
			this.DeleteDatabase.Cursor = System.Windows.Forms.Cursors.Default;
			this.DeleteDatabase.Location = new System.Drawing.Point(141, 349);
			this.DeleteDatabase.Name = "DeleteDatabase";
			this.DeleteDatabase.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.DeleteDatabase.Size = new System.Drawing.Size(63, 22);
			this.DeleteDatabase.TabIndex = 80;
			this.DeleteDatabase.Text = "Delete...";
			this.toolTip1.SetToolTip(this.DeleteDatabase, "Delete the selected user compound database");
			this.DeleteDatabase.Click += new System.EventHandler(this.DeleteDatabase_Click);
			// 
			// EditDatabase
			// 
			this.EditDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.EditDatabase.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EditDatabase.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.EditDatabase.Appearance.Options.UseFont = true;
			this.EditDatabase.Appearance.Options.UseForeColor = true;
			this.EditDatabase.Cursor = System.Windows.Forms.Cursors.Default;
			this.EditDatabase.Location = new System.Drawing.Point(72, 349);
			this.EditDatabase.Name = "EditDatabase";
			this.EditDatabase.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.EditDatabase.Size = new System.Drawing.Size(63, 22);
			this.EditDatabase.TabIndex = 79;
			this.EditDatabase.Text = "Edit";
			this.toolTip1.SetToolTip(this.EditDatabase, "Edit the selected user compound database");
			this.EditDatabase.Click += new System.EventHandler(this.EditDatabase_Click);
			// 
			// NewDatabase
			// 
			this.NewDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.NewDatabase.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NewDatabase.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.NewDatabase.Appearance.Options.UseFont = true;
			this.NewDatabase.Appearance.Options.UseForeColor = true;
			this.NewDatabase.Cursor = System.Windows.Forms.Cursors.Default;
			this.NewDatabase.Location = new System.Drawing.Point(3, 349);
			this.NewDatabase.Name = "NewDatabase";
			this.NewDatabase.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.NewDatabase.Size = new System.Drawing.Size(63, 22);
			this.NewDatabase.TabIndex = 78;
			this.NewDatabase.Text = "New";
			this.toolTip1.SetToolTip(this.NewDatabase, "Create a new user compound database");
			this.NewDatabase.Click += new System.EventHandler(this.NewDatabase_Click);
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
			this.CloseForm.Location = new System.Drawing.Point(560, 349);
			this.CloseForm.Name = "CloseForm";
			this.CloseForm.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CloseForm.Size = new System.Drawing.Size(60, 22);
			this.CloseForm.TabIndex = 77;
			this.CloseForm.Text = "Close";
			// 
			// Grid
			// 
			this.Grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.Grid.Location = new System.Drawing.Point(3, 50);
			this.Grid.MainView = this.GridView;
			this.Grid.Name = "Grid";
			this.Grid.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemCheckEdit1});
			this.Grid.Size = new System.Drawing.Size(613, 294);
			this.Grid.TabIndex = 84;
			this.Grid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.GridView});
			// 
			// GridView
			// 
			this.GridView.ColumnPanelRowHeight = 32;
			this.GridView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.UserDatabaseNameCol,
            this.PublicCol,
            this.MoleculesCol,
            this.PredictiveModelsCol,
            this.PendingUpdatesCol,
            this.CreatedCol,
            this.UpdatedCol,
            this.OwnerUserNameCol,
            this.DatabaseIdCol});
			this.GridView.GridControl = this.Grid;
			this.GridView.Name = "GridView";
			this.GridView.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;
			this.GridView.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False;
			this.GridView.OptionsBehavior.Editable = false;
			this.GridView.OptionsView.ColumnAutoWidth = false;
			this.GridView.OptionsView.RowAutoHeight = true;
			this.GridView.OptionsView.ShowGroupPanel = false;
			this.GridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseDown);
			// 
			// UserDatabaseNameCol
			// 
			this.UserDatabaseNameCol.AppearanceHeader.Options.UseTextOptions = true;
			this.UserDatabaseNameCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.UserDatabaseNameCol.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.UserDatabaseNameCol.Caption = "User Database Name";
			this.UserDatabaseNameCol.FieldName = "UserDatabaseName";
			this.UserDatabaseNameCol.Name = "UserDatabaseNameCol";
			this.UserDatabaseNameCol.OptionsFilter.AllowFilter = false;
			this.UserDatabaseNameCol.Visible = true;
			this.UserDatabaseNameCol.VisibleIndex = 0;
			this.UserDatabaseNameCol.Width = 198;
			// 
			// PublicCol
			// 
			this.PublicCol.AppearanceHeader.Options.UseTextOptions = true;
			this.PublicCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.PublicCol.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.PublicCol.Caption = "Public";
			this.PublicCol.ColumnEdit = this.repositoryItemCheckEdit1;
			this.PublicCol.FieldName = "Public";
			this.PublicCol.Name = "PublicCol";
			this.PublicCol.OptionsFilter.AllowFilter = false;
			this.PublicCol.Visible = true;
			this.PublicCol.VisibleIndex = 1;
			this.PublicCol.Width = 47;
			// 
			// repositoryItemCheckEdit1
			// 
			this.repositoryItemCheckEdit1.AutoHeight = false;
			this.repositoryItemCheckEdit1.Name = "repositoryItemCheckEdit1";
			// 
			// MoleculesCol
			// 
			this.MoleculesCol.AppearanceHeader.Options.UseTextOptions = true;
			this.MoleculesCol.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.MoleculesCol.Caption = "Molecules";
			this.MoleculesCol.FieldName = "Molecules";
			this.MoleculesCol.Name = "MoleculesCol";
			this.MoleculesCol.OptionsFilter.AllowFilter = false;
			this.MoleculesCol.Visible = true;
			this.MoleculesCol.VisibleIndex = 2;
			this.MoleculesCol.Width = 60;
			// 
			// PredictiveModelsCol
			// 
			this.PredictiveModelsCol.AppearanceHeader.Options.UseTextOptions = true;
			this.PredictiveModelsCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.PredictiveModelsCol.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.PredictiveModelsCol.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.PredictiveModelsCol.Caption = "Predictive Models";
			this.PredictiveModelsCol.FieldName = "PredictiveModels";
			this.PredictiveModelsCol.Name = "PredictiveModelsCol";
			this.PredictiveModelsCol.OptionsFilter.AllowFilter = false;
			this.PredictiveModelsCol.Visible = true;
			this.PredictiveModelsCol.VisibleIndex = 3;
			this.PredictiveModelsCol.Width = 60;
			// 
			// PendingUpdatesCol
			// 
			this.PendingUpdatesCol.AppearanceHeader.Options.UseTextOptions = true;
			this.PendingUpdatesCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.PendingUpdatesCol.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.PendingUpdatesCol.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.PendingUpdatesCol.Caption = "Pending Updates";
			this.PendingUpdatesCol.FieldName = "PendingUpdates";
			this.PendingUpdatesCol.Name = "PendingUpdatesCol";
			this.PendingUpdatesCol.OptionsFilter.AllowFilter = false;
			this.PendingUpdatesCol.Visible = true;
			this.PendingUpdatesCol.VisibleIndex = 4;
			// 
			// CreatedCol
			// 
			this.CreatedCol.AppearanceHeader.Options.UseTextOptions = true;
			this.CreatedCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.CreatedCol.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.CreatedCol.Caption = "Created";
			this.CreatedCol.FieldName = "Created";
			this.CreatedCol.Name = "CreatedCol";
			this.CreatedCol.OptionsFilter.AllowFilter = false;
			this.CreatedCol.Visible = true;
			this.CreatedCol.VisibleIndex = 5;
			this.CreatedCol.Width = 70;
			// 
			// UpdatedCol
			// 
			this.UpdatedCol.AppearanceHeader.Options.UseTextOptions = true;
			this.UpdatedCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.UpdatedCol.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.UpdatedCol.Caption = "Updated";
			this.UpdatedCol.FieldName = "Updated";
			this.UpdatedCol.Name = "UpdatedCol";
			this.UpdatedCol.OptionsFilter.AllowFilter = false;
			this.UpdatedCol.Visible = true;
			this.UpdatedCol.VisibleIndex = 6;
			this.UpdatedCol.Width = 70;
			// 
			// OwnerUserNameCol
			// 
			this.OwnerUserNameCol.AppearanceHeader.Options.UseTextOptions = true;
			this.OwnerUserNameCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.OwnerUserNameCol.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.OwnerUserNameCol.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.OwnerUserNameCol.Caption = "Owner UserName";
			this.OwnerUserNameCol.FieldName = "OwnerUserName";
			this.OwnerUserNameCol.Name = "OwnerUserNameCol";
			this.OwnerUserNameCol.OptionsFilter.AllowFilter = false;
			this.OwnerUserNameCol.Visible = true;
			this.OwnerUserNameCol.VisibleIndex = 7;
			this.OwnerUserNameCol.Width = 90;
			// 
			// DatabaseIdCol
			// 
			this.DatabaseIdCol.AppearanceHeader.Options.UseTextOptions = true;
			this.DatabaseIdCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.DatabaseIdCol.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.DatabaseIdCol.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.DatabaseIdCol.Caption = "Database Id";
			this.DatabaseIdCol.FieldName = "DatabaseId";
			this.DatabaseIdCol.Name = "DatabaseIdCol";
			this.DatabaseIdCol.OptionsFilter.AllowFilter = false;
			this.DatabaseIdCol.Visible = true;
			this.DatabaseIdCol.VisibleIndex = 8;
			this.DatabaseIdCol.Width = 58;
			// 
			// GridContextMenu
			// 
			this.GridContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.EditDatabaseMenuItem,
            this.DeleteDatabaseMenuItem,
            this.RecalculateModelResultsMenuItem,
            this.RestartPendingUpdatesMenuItem,
            this.BuildQueryMenuItem,
            this.RunQueryMenuItem});
			this.GridContextMenu.Name = "GridContextMenu";
			this.GridContextMenu.Size = new System.Drawing.Size(210, 136);
			// 
			// EditDatabaseMenuItem
			// 
			this.EditDatabaseMenuItem.Name = "EditDatabaseMenuItem";
			this.EditDatabaseMenuItem.Size = new System.Drawing.Size(209, 22);
			this.EditDatabaseMenuItem.Text = "Edit Database";
			this.EditDatabaseMenuItem.Click += new System.EventHandler(this.EditDatabaseMenuItem_Click);
			// 
			// DeleteDatabaseMenuItem
			// 
			this.DeleteDatabaseMenuItem.Name = "DeleteDatabaseMenuItem";
			this.DeleteDatabaseMenuItem.Size = new System.Drawing.Size(209, 22);
			this.DeleteDatabaseMenuItem.Text = "Delete Database";
			this.DeleteDatabaseMenuItem.Click += new System.EventHandler(this.DeleteDatabaseMenuItem_Click);
			// 
			// RecalculateModelResultsMenuItem
			// 
			this.RecalculateModelResultsMenuItem.Enabled = false;
			this.RecalculateModelResultsMenuItem.Name = "RecalculateModelResultsMenuItem";
			this.RecalculateModelResultsMenuItem.Size = new System.Drawing.Size(209, 22);
			this.RecalculateModelResultsMenuItem.Text = "Recalculate Model Results";
			this.RecalculateModelResultsMenuItem.Visible = false;
			// 
			// RestartPendingUpdatesMenuItem
			// 
			this.RestartPendingUpdatesMenuItem.Enabled = false;
			this.RestartPendingUpdatesMenuItem.Name = "RestartPendingUpdatesMenuItem";
			this.RestartPendingUpdatesMenuItem.Size = new System.Drawing.Size(209, 22);
			this.RestartPendingUpdatesMenuItem.Text = "Restart Pending Updates";
			this.RestartPendingUpdatesMenuItem.Visible = false;
			//this.RestartPendingUpdatesMenuItem.Click += new System.EventHandler(this.RestartPendingUpdatesMenuItem_Click);
			// 
			// BuildQueryMenuItem
			// 
			this.BuildQueryMenuItem.Name = "BuildQueryMenuItem";
			this.BuildQueryMenuItem.Size = new System.Drawing.Size(209, 22);
			this.BuildQueryMenuItem.Text = "Build Query";
			this.BuildQueryMenuItem.Click += new System.EventHandler(this.BuildQueryMenuItem_Click);
			// 
			// RunQueryMenuItem
			// 
			this.RunQueryMenuItem.Name = "RunQueryMenuItem";
			this.RunQueryMenuItem.Size = new System.Drawing.Size(209, 22);
			this.RunQueryMenuItem.Text = "View Data";
			this.RunQueryMenuItem.Click += new System.EventHandler(this.RunQueryMenuItem_Click);
			// 
			// UserDatabasesExplorer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.CloseForm;
			this.ClientSize = new System.Drawing.Size(623, 374);
			this.Controls.Add(this.Grid);
			this.Controls.Add(this.RunDatabaseQuery);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.BuildDatabaseQuery);
			this.Controls.Add(this.DeleteDatabase);
			this.Controls.Add(this.EditDatabase);
			this.Controls.Add(this.NewDatabase);
			this.Controls.Add(this.CloseForm);
			this.MinimizeBox = false;
			this.Name = "UserDatabasesExplorer";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "User Compound Databases";
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit1)).EndInit();
			this.GridContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.SimpleButton RunDatabaseQuery;
		private DevExpress.XtraEditors.LabelControl label1;
		public DevExpress.XtraEditors.SimpleButton BuildDatabaseQuery;
		public DevExpress.XtraEditors.SimpleButton DeleteDatabase;
		public DevExpress.XtraEditors.SimpleButton EditDatabase;
		public DevExpress.XtraEditors.SimpleButton NewDatabase;
		public DevExpress.XtraEditors.SimpleButton CloseForm;
		private DevExpress.XtraGrid.GridControl Grid;
		private DevExpress.XtraGrid.Views.Grid.GridView GridView;
		private DevExpress.XtraGrid.Columns.GridColumn UserDatabaseNameCol;
		private DevExpress.XtraGrid.Columns.GridColumn PublicCol;
		private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repositoryItemCheckEdit1;
		private DevExpress.XtraGrid.Columns.GridColumn MoleculesCol;
		private DevExpress.XtraGrid.Columns.GridColumn PredictiveModelsCol;
		private DevExpress.XtraGrid.Columns.GridColumn PendingUpdatesCol;
		private DevExpress.XtraGrid.Columns.GridColumn CreatedCol;
		private DevExpress.XtraGrid.Columns.GridColumn UpdatedCol;
		private DevExpress.XtraGrid.Columns.GridColumn OwnerUserNameCol;
		private DevExpress.XtraGrid.Columns.GridColumn DatabaseIdCol;
		private System.Windows.Forms.ContextMenuStrip GridContextMenu;
		private System.Windows.Forms.ToolStripMenuItem EditDatabaseMenuItem;
		private System.Windows.Forms.ToolStripMenuItem DeleteDatabaseMenuItem;
		private System.Windows.Forms.ToolStripMenuItem RecalculateModelResultsMenuItem;
		private System.Windows.Forms.ToolStripMenuItem RestartPendingUpdatesMenuItem;
		private System.Windows.Forms.ToolStripMenuItem BuildQueryMenuItem;
		private System.Windows.Forms.ToolStripMenuItem RunQueryMenuItem;
		private System.Windows.Forms.ToolTip toolTip1;
	}
}