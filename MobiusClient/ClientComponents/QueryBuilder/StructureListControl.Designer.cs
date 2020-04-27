namespace Mobius.ClientComponents
{
	partial class StructureListControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StructureListControl));
			this.MoleculeGrid = new Mobius.ClientComponents.MoleculeGridControl();
			this.BandedGridView = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridView();
			this.GridBand = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
			this.NameCol = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
			this.RepositoryItemTextEdit = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
			this.StructureCol = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
			this.DateCol = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
			this.StructureTypeCol = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
			this.RepositoryItemDateEdit = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
			this.bandedGridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
			this.RtClickMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.CopyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RenameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.DeleteRowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.ViewMoleculeInNewWindowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.MoleculeGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BandedGridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RepositoryItemTextEdit)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RepositoryItemDateEdit)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RepositoryItemDateEdit.CalendarTimeProperties)).BeginInit();
			this.RtClickMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// MoleculeGrid
			// 
			this.MoleculeGrid.AutoNumberRows = false;
			this.MoleculeGrid.CheckmarkColumnName = "";
			this.MoleculeGrid.Col = 0;
			this.MoleculeGrid.DataSource = null;
			this.MoleculeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MoleculeGrid.Location = new System.Drawing.Point(0, 0);
			this.MoleculeGrid.MainView = this.BandedGridView;
			this.MoleculeGrid.Name = "MoleculeGrid";
			this.MoleculeGrid.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.RepositoryItemTextEdit,
            this.RepositoryItemDateEdit});
			this.MoleculeGrid.Row = -2147483648;
			this.MoleculeGrid.ShowNewRow = true;
			this.MoleculeGrid.Size = new System.Drawing.Size(861, 568);
			this.MoleculeGrid.TabIndex = 209;
			this.MoleculeGrid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.BandedGridView});
			// 
			// BandedGridView
			// 
			this.BandedGridView.Appearance.HeaderPanel.Options.UseTextOptions = true;
			this.BandedGridView.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.BandedGridView.Appearance.HeaderPanel.TextOptions.Trimming = DevExpress.Utils.Trimming.None;
			this.BandedGridView.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.BandedGridView.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] {
            this.GridBand});
			this.BandedGridView.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] {
            this.NameCol,
            this.StructureCol,
            this.DateCol,
            this.StructureTypeCol});
			this.BandedGridView.GridControl = this.MoleculeGrid;
			this.BandedGridView.IndicatorWidth = 26;
			this.BandedGridView.Name = "BandedGridView";
			this.BandedGridView.OptionsCustomization.AllowColumnMoving = false;
			this.BandedGridView.OptionsCustomization.AllowGroup = false;
			this.BandedGridView.OptionsCustomization.AllowRowSizing = true;
			this.BandedGridView.OptionsMenu.EnableColumnMenu = false;
			this.BandedGridView.OptionsMenu.EnableFooterMenu = false;
			this.BandedGridView.OptionsMenu.EnableGroupPanelMenu = false;
			this.BandedGridView.OptionsSelection.MultiSelect = true;
			this.BandedGridView.OptionsView.ColumnAutoWidth = false;
			this.BandedGridView.OptionsView.RowAutoHeight = true;
			this.BandedGridView.OptionsView.ShowBands = false;
			this.BandedGridView.OptionsView.ShowGroupPanel = false;
			this.BandedGridView.OptionsView.ShowIndicator = false;
			// 
			// GridBand
			// 
			this.GridBand.AppearanceHeader.Options.UseTextOptions = true;
			this.GridBand.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.GridBand.Caption = "---";
			this.GridBand.Columns.Add(this.NameCol);
			this.GridBand.Columns.Add(this.StructureCol);
			this.GridBand.Columns.Add(this.DateCol);
			this.GridBand.Columns.Add(this.StructureTypeCol);
			this.GridBand.Name = "GridBand";
			this.GridBand.VisibleIndex = 0;
			this.GridBand.Width = 616;
			// 
			// NameCol
			// 
			this.NameCol.AppearanceCell.Options.UseTextOptions = true;
			this.NameCol.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.NameCol.AppearanceHeader.Options.UseTextOptions = true;
			this.NameCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.NameCol.AppearanceHeader.TextOptions.Trimming = DevExpress.Utils.Trimming.None;
			this.NameCol.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.NameCol.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.NameCol.Caption = "Name";
			this.NameCol.ColumnEdit = this.RepositoryItemTextEdit;
			this.NameCol.FieldName = "NameCol";
			this.NameCol.Name = "NameCol";
			this.NameCol.OptionsColumn.AllowEdit = false;
			this.NameCol.OptionsColumn.AllowMerge = DevExpress.Utils.DefaultBoolean.False;
			this.NameCol.OptionsColumn.AllowMove = false;
			this.NameCol.OptionsColumn.AllowShowHide = false;
			this.NameCol.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
			this.NameCol.OptionsColumn.ReadOnly = true;
			this.NameCol.Visible = true;
			this.NameCol.Width = 138;
			// 
			// RepositoryItemTextEdit
			// 
			this.RepositoryItemTextEdit.AutoHeight = false;
			this.RepositoryItemTextEdit.Name = "RepositoryItemTextEdit";
			// 
			// StructureCol
			// 
			this.StructureCol.AppearanceHeader.Options.UseTextOptions = true;
			this.StructureCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.StructureCol.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.StructureCol.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.StructureCol.Caption = "Structure";
			this.StructureCol.FieldName = "StructureCol";
			this.StructureCol.Name = "StructureCol";
			this.StructureCol.OptionsColumn.AllowEdit = false;
			this.StructureCol.OptionsColumn.AllowMerge = DevExpress.Utils.DefaultBoolean.False;
			this.StructureCol.OptionsColumn.AllowMove = false;
			this.StructureCol.OptionsColumn.AllowShowHide = false;
			this.StructureCol.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
			this.StructureCol.OptionsColumn.ReadOnly = true;
			this.StructureCol.OptionsFilter.AllowFilter = false;
			this.StructureCol.UnboundType = DevExpress.Data.UnboundColumnType.Object;
			this.StructureCol.Visible = true;
			this.StructureCol.Width = 307;
			// 
			// DateCol
			// 
			this.DateCol.AppearanceCell.Options.UseTextOptions = true;
			this.DateCol.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.DateCol.AppearanceHeader.Options.UseTextOptions = true;
			this.DateCol.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.DateCol.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			this.DateCol.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.DateCol.Caption = "Date";
			this.DateCol.ColumnEdit = this.RepositoryItemTextEdit;
			this.DateCol.DisplayFormat.FormatString = "dd-MMM-yyyy";
			this.DateCol.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
			this.DateCol.FieldName = "DateCol";
			this.DateCol.Name = "DateCol";
			this.DateCol.OptionsColumn.AllowEdit = false;
			this.DateCol.OptionsColumn.AllowMerge = DevExpress.Utils.DefaultBoolean.False;
			this.DateCol.OptionsColumn.AllowMove = false;
			this.DateCol.OptionsColumn.AllowShowHide = false;
			this.DateCol.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
			this.DateCol.OptionsColumn.ReadOnly = true;
			this.DateCol.Visible = true;
			this.DateCol.Width = 81;
			// 
			// StructureTypeCol
			// 
			this.StructureTypeCol.AppearanceCell.Options.UseTextOptions = true;
			this.StructureTypeCol.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.StructureTypeCol.Caption = "Type";
			this.StructureTypeCol.ColumnEdit = this.RepositoryItemTextEdit;
			this.StructureTypeCol.FieldName = "StructureTypeCol";
			this.StructureTypeCol.Name = "StructureTypeCol";
			this.StructureTypeCol.OptionsColumn.AllowEdit = false;
			this.StructureTypeCol.OptionsColumn.AllowMerge = DevExpress.Utils.DefaultBoolean.False;
			this.StructureTypeCol.OptionsColumn.AllowMove = false;
			this.StructureTypeCol.OptionsColumn.AllowShowHide = false;
			this.StructureTypeCol.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
			this.StructureTypeCol.OptionsColumn.ReadOnly = true;
			this.StructureTypeCol.Visible = true;
			this.StructureTypeCol.Width = 90;
			// 
			// RepositoryItemDateEdit
			// 
			this.RepositoryItemDateEdit.AutoHeight = false;
			this.RepositoryItemDateEdit.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.RepositoryItemDateEdit.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.RepositoryItemDateEdit.Name = "RepositoryItemDateEdit";
			// 
			// bandedGridColumn1
			// 
			this.bandedGridColumn1.Caption = "bandedGridColumn1";
			this.bandedGridColumn1.Name = "bandedGridColumn1";
			// 
			// RtClickMenu
			// 
			this.RtClickMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CopyMenuItem,
            this.RenameMenuItem,
            this.DeleteRowMenuItem,
            this.toolStripSeparator2,
            this.ViewMoleculeInNewWindowMenuItem});
			this.RtClickMenu.Name = "RetrieveModelMenu";
			this.RtClickMenu.Size = new System.Drawing.Size(248, 98);
			// 
			// CopyMenuItem
			// 
			this.CopyMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Copy;
			this.CopyMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.CopyMenuItem.Name = "CopyMenuItem";
			this.CopyMenuItem.Size = new System.Drawing.Size(247, 22);
			this.CopyMenuItem.Text = "Copy";
			this.CopyMenuItem.Click += new System.EventHandler(this.CopyMenuItem_Click);
			// 
			// RenameMenuItem
			// 
			this.RenameMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("RenameMenuItem.Image")));
			this.RenameMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.RenameMenuItem.Name = "RenameMenuItem";
			this.RenameMenuItem.Size = new System.Drawing.Size(247, 22);
			this.RenameMenuItem.Text = "Rename...";
			this.RenameMenuItem.Click += new System.EventHandler(this.RenameMenuItem_Click);
			// 
			// DeleteRowMenuItem
			// 
			this.DeleteRowMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("DeleteRowMenuItem.Image")));
			this.DeleteRowMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.DeleteRowMenuItem.Name = "DeleteRowMenuItem";
			this.DeleteRowMenuItem.Size = new System.Drawing.Size(247, 22);
			this.DeleteRowMenuItem.Text = "Delete";
			this.DeleteRowMenuItem.Click += new System.EventHandler(this.DeleteRowMenuItem_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(244, 6);
			// 
			// ViewMoleculeInNewWindowMenuItem
			// 
			this.ViewMoleculeInNewWindowMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Struct;
			this.ViewMoleculeInNewWindowMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ViewMoleculeInNewWindowMenuItem.Name = "ViewMoleculeInNewWindowMenuItem";
			this.ViewMoleculeInNewWindowMenuItem.Size = new System.Drawing.Size(247, 22);
			this.ViewMoleculeInNewWindowMenuItem.Text = "View Molecule in a New Window";
			this.ViewMoleculeInNewWindowMenuItem.Click += new System.EventHandler(this.ViewMoleculeInNewWindowMenuItem_Click);
			// 
			// StructureListControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.MoleculeGrid);
			this.Name = "StructureListControl";
			this.Size = new System.Drawing.Size(861, 568);
			((System.ComponentModel.ISupportInitialize)(this.MoleculeGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BandedGridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RepositoryItemTextEdit)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RepositoryItemDateEdit.CalendarTimeProperties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RepositoryItemDateEdit)).EndInit();
			this.RtClickMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		public MoleculeGridControl MoleculeGrid;
		private DevExpress.XtraGrid.Views.BandedGrid.BandedGridView BandedGridView;
		private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn NameCol;
		private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn StructureCol;
		private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn DateCol;
		private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn StructureTypeCol;
		private DevExpress.XtraGrid.Views.BandedGrid.GridBand GridBand;
		private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn1;
		private System.Windows.Forms.ContextMenuStrip RtClickMenu;
		private System.Windows.Forms.ToolStripMenuItem RenameMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem DeleteRowMenuItem;
		private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit RepositoryItemTextEdit;
		private DevExpress.XtraEditors.Repository.RepositoryItemDateEdit RepositoryItemDateEdit;
		public System.Windows.Forms.ToolStripMenuItem CopyMenuItem;
		public System.Windows.Forms.ToolStripMenuItem ViewMoleculeInNewWindowMenuItem;
	}
}
