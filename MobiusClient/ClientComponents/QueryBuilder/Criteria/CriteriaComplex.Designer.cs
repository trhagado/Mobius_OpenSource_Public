namespace Mobius.ClientComponents
{
	partial class CriteriaComplex
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
			this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
			this.TableGrid = new DevExpress.XtraGrid.GridControl();
			this.GridView = new DevExpress.XtraGrid.Views.Grid.GridView();
			this.gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
			this.gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
			this.label1 = new DevExpress.XtraEditors.LabelControl();
			this.AddCriteria = new DevExpress.XtraEditors.SimpleButton();
			this.Criteria = new System.Windows.Forms.RichTextBox();
			this.CriteriaLabel = new DevExpress.XtraEditors.LabelControl();
			this.Help = new DevExpress.XtraEditors.SimpleButton();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.AddCriteriaPopup = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.EQ = new System.Windows.Forms.ToolStripMenuItem();
			this.LT = new System.Windows.Forms.ToolStripMenuItem();
			this.LE = new System.Windows.Forms.ToolStripMenuItem();
			this.GT = new System.Windows.Forms.ToolStripMenuItem();
			this.GE = new System.Windows.Forms.ToolStripMenuItem();
			this.NE = new System.Windows.Forms.ToolStripMenuItem();
			this.In = new System.Windows.Forms.ToolStripMenuItem();
			this.NotIn = new System.Windows.Forms.ToolStripMenuItem();
			this.Between = new System.Windows.Forms.ToolStripMenuItem();
			this.NotBetween = new System.Windows.Forms.ToolStripMenuItem();
			this.Like = new System.Windows.Forms.ToolStripMenuItem();
			this.NotLike = new System.Windows.Forms.ToolStripMenuItem();
			this.IsNull = new System.Windows.Forms.ToolStripMenuItem();
			this.IsNotNull = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.InSavedList = new System.Windows.Forms.ToolStripMenuItem();
			this.StructureSearch = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.And = new System.Windows.Forms.ToolStripMenuItem();
			this.Or = new System.Windows.Forms.ToolStripMenuItem();
			this.Not = new System.Windows.Forms.ToolStripMenuItem();
			this.Parens = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
			this.splitContainerControl1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TableGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GridView)).BeginInit();
			this.AddCriteriaPopup.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainerControl1
			// 
			this.splitContainerControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainerControl1.Horizontal = false;
			this.splitContainerControl1.Location = new System.Drawing.Point(0, 0);
			this.splitContainerControl1.Name = "splitContainerControl1";
			this.splitContainerControl1.Panel1.Controls.Add(this.TableGrid);
			this.splitContainerControl1.Panel1.Controls.Add(this.label1);
			this.splitContainerControl1.Panel1.Text = "Panel1";
			this.splitContainerControl1.Panel2.Controls.Add(this.AddCriteria);
			this.splitContainerControl1.Panel2.Controls.Add(this.Criteria);
			this.splitContainerControl1.Panel2.Controls.Add(this.CriteriaLabel);
			this.splitContainerControl1.Panel2.Text = "Panel2";
			this.splitContainerControl1.Size = new System.Drawing.Size(646, 424);
			this.splitContainerControl1.SplitterPosition = 145;
			this.splitContainerControl1.TabIndex = 0;
			this.splitContainerControl1.Text = "splitContainerControl1";
			// 
			// TableGrid
			// 
			this.TableGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.TableGrid.Location = new System.Drawing.Point(5, 21);
			this.TableGrid.MainView = this.GridView;
			this.TableGrid.Name = "TableGrid";
			this.TableGrid.Size = new System.Drawing.Size(634, 121);
			this.TableGrid.TabIndex = 4;
			this.TableGrid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.GridView});
			// 
			// GridView
			// 
			this.GridView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn1,
            this.gridColumn2});
			this.GridView.GridControl = this.TableGrid;
			this.GridView.IndicatorWidth = 25;
			this.GridView.Name = "GridView";
			this.GridView.OptionsView.ShowGroupPanel = false;
			this.GridView.CustomDrawRowIndicator += new DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventHandler(this.gridView1_CustomDrawRowIndicator);
			this.GridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gridView1_MouseDown);
			// 
			// gridColumn1
			// 
			this.gridColumn1.AppearanceHeader.Options.UseTextOptions = true;
			this.gridColumn1.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.gridColumn1.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
			this.gridColumn1.Caption = "Data Table/Assay Name";
			this.gridColumn1.FieldName = "TableName";
			this.gridColumn1.Name = "gridColumn1";
			this.gridColumn1.OptionsColumn.AllowEdit = false;
			this.gridColumn1.Visible = true;
			this.gridColumn1.VisibleIndex = 0;
			this.gridColumn1.Width = 542;
			// 
			// gridColumn2
			// 
			this.gridColumn2.AppearanceHeader.Options.UseTextOptions = true;
			this.gridColumn2.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.gridColumn2.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
			this.gridColumn2.Caption = "Criteria";
			this.gridColumn2.FieldName = "Criteria";
			this.gridColumn2.Name = "gridColumn2";
			this.gridColumn2.OptionsColumn.AllowEdit = false;
			this.gridColumn2.Visible = true;
			this.gridColumn2.VisibleIndex = 1;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(6, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(71, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Selected Data:";
			// 
			// AddCriteria
			// 
			this.AddCriteria.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AddCriteria.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AddCriteria.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.AddCriteria.Appearance.Options.UseFont = true;
			this.AddCriteria.Appearance.Options.UseForeColor = true;
			this.AddCriteria.Cursor = System.Windows.Forms.Cursors.Default;
			this.AddCriteria.Location = new System.Drawing.Point(566, 3);
			this.AddCriteria.Name = "AddCriteria";
			this.AddCriteria.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.AddCriteria.Size = new System.Drawing.Size(73, 22);
			this.AddCriteria.TabIndex = 44;
			this.AddCriteria.Text = "&Add Criteria";
			this.AddCriteria.Click += new System.EventHandler(this.AddCriteria_Click);
			// 
			// Criteria
			// 
			this.Criteria.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.Criteria.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Criteria.Location = new System.Drawing.Point(5, 28);
			this.Criteria.Name = "Criteria";
			this.Criteria.Size = new System.Drawing.Size(634, 246);
			this.Criteria.TabIndex = 43;
			this.Criteria.Text = "";
			this.Criteria.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Criteria_MouseDown);
			// 
			// CriteriaLabel
			// 
			this.CriteriaLabel.Location = new System.Drawing.Point(6, 7);
			this.CriteriaLabel.Name = "CriteriaLabel";
			this.CriteriaLabel.Size = new System.Drawing.Size(39, 13);
			this.CriteriaLabel.TabIndex = 42;
			this.CriteriaLabel.Text = "Criteria:";
			// 
			// Help
			// 
			this.Help.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Help.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Help.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Help.Appearance.Options.UseFont = true;
			this.Help.Appearance.Options.UseForeColor = true;
			this.Help.Cursor = System.Windows.Forms.Cursors.Default;
			this.Help.Enabled = false;
			this.Help.Location = new System.Drawing.Point(579, 429);
			this.Help.Name = "Help";
			this.Help.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Help.Size = new System.Drawing.Size(60, 23);
			this.Help.TabIndex = 70;
			this.Help.Tag = "Cancel";
			this.Help.Text = "Help";
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(447, 430);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(60, 22);
			this.OK.TabIndex = 69;
			this.OK.Text = "&OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// Cancel
			// 
			this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Cancel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Cancel.Appearance.Options.UseFont = true;
			this.Cancel.Appearance.Options.UseForeColor = true;
			this.Cancel.Cursor = System.Windows.Forms.Cursors.Default;
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = new System.Drawing.Point(513, 430);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(60, 22);
			this.Cancel.TabIndex = 68;
			this.Cancel.Text = "Cancel";
			// 
			// AddCriteriaPopup
			// 
			this.AddCriteriaPopup.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.EQ,
            this.LT,
            this.LE,
            this.GT,
            this.GE,
            this.NE,
            this.In,
            this.NotIn,
            this.Between,
            this.NotBetween,
            this.Like,
            this.NotLike,
            this.IsNull,
            this.IsNotNull,
            this.toolStripSeparator1,
            this.InSavedList,
            this.StructureSearch,
            this.toolStripSeparator2,
            this.And,
            this.Or,
            this.Not,
            this.Parens});
			this.AddCriteriaPopup.Name = "AddCriteriaPopup";
			this.AddCriteriaPopup.Size = new System.Drawing.Size(188, 456);
			// 
			// EQ
			// 
			this.EQ.Name = "EQ";
			this.EQ.Size = new System.Drawing.Size(187, 22);
			this.EQ.Text = "=";
			this.EQ.Click += new System.EventHandler(this.EQ_Click);
			// 
			// LT
			// 
			this.LT.Name = "LT";
			this.LT.Size = new System.Drawing.Size(187, 22);
			this.LT.Text = "<";
			this.LT.Click += new System.EventHandler(this.LT_Click);
			// 
			// LE
			// 
			this.LE.Name = "LE";
			this.LE.Size = new System.Drawing.Size(187, 22);
			this.LE.Text = "<=";
			this.LE.Click += new System.EventHandler(this.LE_Click);
			// 
			// GT
			// 
			this.GT.Name = "GT";
			this.GT.Size = new System.Drawing.Size(187, 22);
			this.GT.Text = ">";
			this.GT.Click += new System.EventHandler(this.GT_Click);
			// 
			// GE
			// 
			this.GE.Name = "GE";
			this.GE.Size = new System.Drawing.Size(187, 22);
			this.GE.Text = ">=";
			this.GE.Click += new System.EventHandler(this.GE_Click);
			// 
			// NE
			// 
			this.NE.Name = "NE";
			this.NE.Size = new System.Drawing.Size(187, 22);
			this.NE.Text = "<>";
			this.NE.Click += new System.EventHandler(this.NE_Click);
			// 
			// In
			// 
			this.In.Name = "In";
			this.In.Size = new System.Drawing.Size(187, 22);
			this.In.Text = "In (...)";
			this.In.Click += new System.EventHandler(this.In_Click);
			// 
			// NotIn
			// 
			this.NotIn.Name = "NotIn";
			this.NotIn.Size = new System.Drawing.Size(187, 22);
			this.NotIn.Text = "Not In (...)";
			this.NotIn.Click += new System.EventHandler(this.NotIn_Click);
			// 
			// Between
			// 
			this.Between.Name = "Between";
			this.Between.Size = new System.Drawing.Size(187, 22);
			this.Between.Text = "Between A and B";
			this.Between.Click += new System.EventHandler(this.Between_Click);
			// 
			// NotBetween
			// 
			this.NotBetween.Name = "NotBetween";
			this.NotBetween.Size = new System.Drawing.Size(187, 22);
			this.NotBetween.Text = "Not Between A and B";
			this.NotBetween.Click += new System.EventHandler(this.NotBetween_Click);
			// 
			// Like
			// 
			this.Like.Name = "Like";
			this.Like.Size = new System.Drawing.Size(187, 22);
			this.Like.Text = "Like";
			this.Like.Click += new System.EventHandler(this.Like_Click);
			// 
			// NotLike
			// 
			this.NotLike.Name = "NotLike";
			this.NotLike.Size = new System.Drawing.Size(187, 22);
			this.NotLike.Text = "Not Like";
			this.NotLike.Click += new System.EventHandler(this.NotLike_Click);
			// 
			// IsNull
			// 
			this.IsNull.Name = "IsNull";
			this.IsNull.Size = new System.Drawing.Size(187, 22);
			this.IsNull.Text = "Is Null";
			this.IsNull.Click += new System.EventHandler(this.IsNull_Click);
			// 
			// IsNotNull
			// 
			this.IsNotNull.Name = "IsNotNull";
			this.IsNotNull.Size = new System.Drawing.Size(187, 22);
			this.IsNotNull.Text = "Is Not Null";
			this.IsNotNull.Click += new System.EventHandler(this.IsNotNull_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(184, 6);
			// 
			// InSavedList
			// 
			this.InSavedList.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.InSavedList.Name = "InSavedList";
			this.InSavedList.Size = new System.Drawing.Size(187, 22);
			this.InSavedList.Text = "Saved List Search";
			this.InSavedList.Click += new System.EventHandler(this.InSavedList_Click);
			// 
			// StructureSearch
			// 
			this.StructureSearch.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.StructureSearch.Name = "StructureSearch";
			this.StructureSearch.Size = new System.Drawing.Size(187, 22);
			this.StructureSearch.Text = "Structure Search";
			this.StructureSearch.Click += new System.EventHandler(this.StructureSearch_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(184, 6);
			// 
			// And
			// 
			this.And.Name = "And";
			this.And.Size = new System.Drawing.Size(187, 22);
			this.And.Text = "And";
			this.And.Click += new System.EventHandler(this.And_Click);
			// 
			// Or
			// 
			this.Or.Name = "Or";
			this.Or.Size = new System.Drawing.Size(187, 22);
			this.Or.Text = "Or";
			this.Or.Click += new System.EventHandler(this.Or_Click);
			// 
			// Not
			// 
			this.Not.Name = "Not";
			this.Not.Size = new System.Drawing.Size(187, 22);
			this.Not.Text = "Not";
			this.Not.Click += new System.EventHandler(this.Not_Click);
			// 
			// Parens
			// 
			this.Parens.Name = "Parens";
			this.Parens.Size = new System.Drawing.Size(187, 22);
			this.Parens.Text = "(...)";
			this.Parens.Click += new System.EventHandler(this.Parens_Click);
			// 
			// CriteriaComplex
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(642, 455);
			this.Controls.Add(this.Help);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.splitContainerControl1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CriteriaComplex";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Advanced Criteria Editor";
			this.Activated += new System.EventHandler(this.CriteriaComplex_Activated);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
			this.splitContainerControl1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TableGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GridView)).EndInit();
			this.AddCriteriaPopup.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
		private DevExpress.XtraEditors.LabelControl label1;
		public DevExpress.XtraEditors.SimpleButton Help;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		private DevExpress.XtraGrid.GridControl TableGrid;
		private DevExpress.XtraGrid.Views.Grid.GridView GridView;
		public DevExpress.XtraEditors.SimpleButton AddCriteria;
		public System.Windows.Forms.RichTextBox Criteria;
		private DevExpress.XtraEditors.LabelControl CriteriaLabel;
		private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
		private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
		private System.Windows.Forms.ContextMenuStrip AddCriteriaPopup;
		private System.Windows.Forms.ToolStripMenuItem EQ;
		private System.Windows.Forms.ToolStripMenuItem LT;
		private System.Windows.Forms.ToolStripMenuItem LE;
		private System.Windows.Forms.ToolStripMenuItem GT;
		private System.Windows.Forms.ToolStripMenuItem GE;
		private System.Windows.Forms.ToolStripMenuItem NE;
		private System.Windows.Forms.ToolStripMenuItem In;
		private System.Windows.Forms.ToolStripMenuItem NotIn;
		private System.Windows.Forms.ToolStripMenuItem Between;
		private System.Windows.Forms.ToolStripMenuItem NotBetween;
		private System.Windows.Forms.ToolStripMenuItem Like;
		private System.Windows.Forms.ToolStripMenuItem NotLike;
		private System.Windows.Forms.ToolStripMenuItem IsNull;
		private System.Windows.Forms.ToolStripMenuItem IsNotNull;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem InSavedList;
		private System.Windows.Forms.ToolStripMenuItem StructureSearch;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem And;
		private System.Windows.Forms.ToolStripMenuItem Or;
		private System.Windows.Forms.ToolStripMenuItem Not;
		private System.Windows.Forms.ToolStripMenuItem Parens;

	}
}