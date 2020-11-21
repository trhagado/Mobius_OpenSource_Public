namespace Mobius.ClientComponents
{
	partial class ContentsTreeControl
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
			this.TreeList = new DevExpress.XtraTreeList.TreeList();
			this.MetaTreeNodeCol = new DevExpress.XtraTreeList.Columns.TreeListColumn();
			((System.ComponentModel.ISupportInitialize)(this.TreeList)).BeginInit();
			this.SuspendLayout();
			// 
			// TreeList
			// 
			this.TreeList.Appearance.FocusedCell.BackColor = System.Drawing.SystemColors.Highlight;
			this.TreeList.Appearance.FocusedCell.ForeColor = System.Drawing.Color.White;
			this.TreeList.Appearance.FocusedCell.Options.UseBackColor = true;
			this.TreeList.Appearance.FocusedCell.Options.UseForeColor = true;
			this.TreeList.Appearance.FocusedRow.BackColor = System.Drawing.Color.White;
			this.TreeList.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] {
            this.MetaTreeNodeCol});
			this.TreeList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TreeList.FixedLineWidth = 3;
			this.TreeList.HorzScrollStep = 4;
			this.TreeList.ImageIndexFieldName = "";
			this.TreeList.Location = new System.Drawing.Point(0, 0);
			this.TreeList.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.TreeList.MinWidth = 30;
			this.TreeList.Name = "TreeList";
			this.TreeList.OptionsBehavior.Editable = false;
			this.TreeList.OptionsBehavior.EditorShowMode = DevExpress.XtraTreeList.TreeListEditorShowMode.MouseDownFocused;
			this.TreeList.OptionsSelection.KeepSelectedOnClick = false;
			this.TreeList.OptionsView.AutoWidth = false;
			this.TreeList.OptionsView.ShowColumns = false;
			this.TreeList.OptionsView.ShowHorzLines = false;
			this.TreeList.OptionsView.ShowIndicator = false;
			this.TreeList.OptionsView.ShowRoot = false;
			this.TreeList.OptionsView.ShowVertLines = false;
			this.TreeList.Size = new System.Drawing.Size(445, 417);
			this.TreeList.TabIndex = 0;
			this.TreeList.TreeLevelWidth = 27;
			this.TreeList.BeforeExpand += new DevExpress.XtraTreeList.BeforeExpandEventHandler(this.Tree_BeforeExpand);
			this.TreeList.FocusedNodeChanged += new DevExpress.XtraTreeList.FocusedNodeChangedEventHandler(this.Tree_FocusedNodeChanged);
			this.TreeList.CustomDrawNodeCell += new DevExpress.XtraTreeList.CustomDrawNodeCellEventHandler(this.Tree_CustomDrawNodeCell);
			this.TreeList.CustomDrawNodeImages += new DevExpress.XtraTreeList.CustomDrawNodeImagesEventHandler(this.Tree_CustomDrawNodeImages);
			this.TreeList.Click += new System.EventHandler(this.Tree_Click);
			this.TreeList.DoubleClick += new System.EventHandler(this.Tree_DoubleClick);
			this.TreeList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Tree_MouseDown);
			// 
			// MetaTreeNodeCol
			// 
			this.MetaTreeNodeCol.AllowIncrementalSearch = false;
			this.MetaTreeNodeCol.Caption = "MetaTreeNodeCol";
			this.MetaTreeNodeCol.FieldName = "MetaTreeNodeCol";
			this.MetaTreeNodeCol.MinWidth = 30;
			this.MetaTreeNodeCol.Name = "MetaTreeNodeCol";
			this.MetaTreeNodeCol.Visible = true;
			this.MetaTreeNodeCol.VisibleIndex = 0;
			this.MetaTreeNodeCol.Width = 1500;
			// 
			// ContentsTreeControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.TreeList);
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "ContentsTreeControl";
			this.Size = new System.Drawing.Size(445, 417);
			((System.ComponentModel.ISupportInitialize)(this.TreeList)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraTreeList.TreeList TreeList;
		private DevExpress.XtraTreeList.Columns.TreeListColumn MetaTreeNodeCol;

	}
}
