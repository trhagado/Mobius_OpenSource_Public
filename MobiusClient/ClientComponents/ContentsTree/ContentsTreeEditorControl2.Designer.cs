namespace Mobius.ClientComponents
{
    partial class ContentsTreeEditorControl2
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
            DevExpress.Utils.SuperToolTip superToolTip1 = new DevExpress.Utils.SuperToolTip();
            DevExpress.Utils.ToolTipTitleItem toolTipTitleItem1 = new DevExpress.Utils.ToolTipTitleItem();
            DevExpress.Utils.ToolTipItem toolTipItem1 = new DevExpress.Utils.ToolTipItem();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject1 = new DevExpress.Utils.SerializableAppearanceObject();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.treeList1 = new DevExpress.XtraTreeList.TreeList();
            this.ColType = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.ColLabel = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.ColName = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.ColItem = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.ColComments = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.ColDisabled = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.contentsTreeEditorMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.InsertMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.folderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.databaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.metaTableToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.uRLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.actionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.annotationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DeleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnResetTree = new DevExpress.XtraEditors.SimpleButton();
            this.btnShowParents = new DevExpress.XtraEditors.SimpleButton();
            this.btnSearchAll = new DevExpress.XtraEditors.ButtonEdit();
            this.panelSearchBar = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeList1)).BeginInit();
            this.contentsTreeEditorMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnSearchAll.Properties)).BeginInit();
            this.panelSearchBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelControl1
            // 
            this.panelControl1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControl1.Controls.Add(this.treeList1);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl1.Location = new System.Drawing.Point(0, 34);
            this.panelControl1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(780, 412);
            this.panelControl1.TabIndex = 9;
            // 
            // treeList1
            // 
            this.treeList1.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] {
            this.ColType,
            this.ColLabel,
            this.ColName,
            this.ColItem,
            this.ColComments,
            this.ColDisabled});
            this.treeList1.ContextMenuStrip = this.contentsTreeEditorMenuStrip;
            this.treeList1.Cursor = System.Windows.Forms.Cursors.Default;
            this.treeList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeList1.Location = new System.Drawing.Point(0, 0);
            this.treeList1.Name = "treeList1";
            this.treeList1.OptionsDragAndDrop.DragNodesMode = DevExpress.XtraTreeList.DragNodesMode.Multiple;
            this.treeList1.OptionsSelection.MultiSelect = true;
            this.treeList1.Size = new System.Drawing.Size(780, 412);
            this.treeList1.TabIndex = 0;
            this.treeList1.BeforeExpand += new DevExpress.XtraTreeList.BeforeExpandEventHandler(this.treeList1_BeforeExpand);
            this.treeList1.AfterFocusNode += new DevExpress.XtraTreeList.NodeEventHandler(this.treeList1_AfterFocusNode);
            this.treeList1.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.treeList1_ValidatingEditor);
            this.treeList1.InvalidValueException += new DevExpress.XtraEditors.Controls.InvalidValueExceptionEventHandler(this.treeList1_InvalidValueException);
            this.treeList1.InvalidNodeException += new DevExpress.XtraTreeList.InvalidNodeExceptionEventHandler(this.treeList1_InvalidNodeException);
            this.treeList1.CustomDrawNodeImages += new DevExpress.XtraTreeList.CustomDrawNodeImagesEventHandler(this.treeList1_CustomDrawNodeImages);
            this.treeList1.CellValueChanging += new DevExpress.XtraTreeList.CellValueChangedEventHandler(this.treeList1_CellValueChanging);
            this.treeList1.CellValueChanged += new DevExpress.XtraTreeList.CellValueChangedEventHandler(this.treeList1_CellValueChanged);
            this.treeList1.Load += new System.EventHandler(this.ContentsTreeEditorControl_Load);
            this.treeList1.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeList1_DragDrop);
            // 
            // ColType
            // 
            this.ColType.Caption = "Type";
            this.ColType.FieldName = "Type";
            this.ColType.Name = "ColType";
            this.ColType.OptionsColumn.AllowEdit = false;
            this.ColType.OptionsColumn.AllowFocus = false;
            this.ColType.OptionsColumn.AllowSort = false;
            this.ColType.OptionsColumn.ReadOnly = true;
            this.ColType.Visible = true;
            this.ColType.VisibleIndex = 0;
            this.ColType.Width = 100;
            // 
            // ColLabel
            // 
            this.ColLabel.Caption = "Label";
            this.ColLabel.FieldName = "Label";
            this.ColLabel.Name = "ColLabel";
            this.ColLabel.OptionsColumn.AllowEdit = false;
            this.ColLabel.OptionsColumn.AllowSort = false;
            this.ColLabel.Visible = true;
            this.ColLabel.VisibleIndex = 1;
            this.ColLabel.Width = 100;
            // 
            // ColName
            // 
            this.ColName.Caption = "ColName";
            this.ColName.FieldName = "Name";
            this.ColName.Name = "ColName";
            this.ColName.OptionsColumn.AllowSort = false;
            this.ColName.Visible = true;
            this.ColName.VisibleIndex = 2;
            this.ColName.Width = 100;
            // 
            // ColItem
            // 
            this.ColItem.Caption = "Item";
            this.ColItem.FieldName = "Item";
            this.ColItem.Name = "ColItem";
            this.ColItem.OptionsColumn.AllowEdit = false;
            this.ColItem.OptionsColumn.AllowSort = false;
            this.ColItem.OptionsColumn.FixedWidth = true;
            this.ColItem.Visible = true;
            this.ColItem.VisibleIndex = 3;
            this.ColItem.Width = 40;
            // 
            // ColComments
            // 
            this.ColComments.Caption = "Notes";
            this.ColComments.FieldName = "Comments";
            this.ColComments.Name = "ColComments";
            this.ColComments.OptionsColumn.AllowEdit = false;
            this.ColComments.OptionsColumn.AllowSort = false;
            this.ColComments.OptionsColumn.FixedWidth = true;
            this.ColComments.Visible = true;
            this.ColComments.VisibleIndex = 4;
            this.ColComments.Width = 50;
            // 
            // ColDisabled
            // 
            this.ColDisabled.Caption = "Disabled";
            this.ColDisabled.FieldName = "Disabled";
            this.ColDisabled.Name = "ColDisabled";
            this.ColDisabled.OptionsColumn.AllowEdit = false;
            this.ColDisabled.OptionsColumn.AllowSort = false;
            this.ColDisabled.OptionsColumn.FixedWidth = true;
            this.ColDisabled.Visible = true;
            this.ColDisabled.VisibleIndex = 5;
            this.ColDisabled.Width = 50;
            // 
            // contentsTreeEditorMenuStrip
            // 
            this.contentsTreeEditorMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contentsTreeEditorMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.InsertMenuItem,
            this.cutMenuItem,
            this.copyMenuItem,
            this.pasteMenuItem,
            this.DeleteMenuItem});
            this.contentsTreeEditorMenuStrip.Name = "contextMenuStrip1";
            this.contentsTreeEditorMenuStrip.Size = new System.Drawing.Size(129, 134);
            this.contentsTreeEditorMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contentsTreeEditorMenuStrip_Opening);
            // 
            // InsertMenuItem
            // 
            this.InsertMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.folderToolStripMenuItem,
            this.databaseToolStripMenuItem,
            this.projectToolStripMenuItem,
            this.metaTableToolStripMenuItem1,
            this.uRLToolStripMenuItem,
            this.actionToolStripMenuItem,
            this.annotationToolStripMenuItem});
            this.InsertMenuItem.Enabled = false;
            this.InsertMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Add;
            this.InsertMenuItem.Name = "InsertMenuItem";
            this.InsertMenuItem.Size = new System.Drawing.Size(128, 26);
            this.InsertMenuItem.Text = "Insert";
            // 
            // folderToolStripMenuItem
            // 
            this.folderToolStripMenuItem.Enabled = false;
            this.folderToolStripMenuItem.Name = "folderToolStripMenuItem";
            this.folderToolStripMenuItem.Size = new System.Drawing.Size(173, 26);
            this.folderToolStripMenuItem.Text = "SystemFolder";
            this.folderToolStripMenuItem.Click += new System.EventHandler(this.insertNodeMenuItem_Click);
            // 
            // databaseToolStripMenuItem
            // 
            this.databaseToolStripMenuItem.Enabled = false;
            this.databaseToolStripMenuItem.Name = "databaseToolStripMenuItem";
            this.databaseToolStripMenuItem.Size = new System.Drawing.Size(173, 26);
            this.databaseToolStripMenuItem.Text = "Database";
            this.databaseToolStripMenuItem.Click += new System.EventHandler(this.insertNodeMenuItem_Click);
            // 
            // projectToolStripMenuItem
            // 
            this.projectToolStripMenuItem.Enabled = false;
            this.projectToolStripMenuItem.Name = "projectToolStripMenuItem";
            this.projectToolStripMenuItem.Size = new System.Drawing.Size(173, 26);
            this.projectToolStripMenuItem.Text = "Project";
            this.projectToolStripMenuItem.Click += new System.EventHandler(this.insertNodeMenuItem_Click);
            // 
            // metaTableToolStripMenuItem1
            // 
            this.metaTableToolStripMenuItem1.Enabled = false;
            this.metaTableToolStripMenuItem1.Name = "metaTableToolStripMenuItem1";
            this.metaTableToolStripMenuItem1.Size = new System.Drawing.Size(173, 26);
            this.metaTableToolStripMenuItem1.Text = "MetaTable";
            this.metaTableToolStripMenuItem1.Click += new System.EventHandler(this.insertNodeMenuItem_Click);
            // 
            // uRLToolStripMenuItem
            // 
            this.uRLToolStripMenuItem.Enabled = false;
            this.uRLToolStripMenuItem.Name = "uRLToolStripMenuItem";
            this.uRLToolStripMenuItem.Size = new System.Drawing.Size(173, 26);
            this.uRLToolStripMenuItem.Text = "Url";
            this.uRLToolStripMenuItem.Click += new System.EventHandler(this.insertNodeMenuItem_Click);
            // 
            // actionToolStripMenuItem
            // 
            this.actionToolStripMenuItem.Enabled = false;
            this.actionToolStripMenuItem.Name = "actionToolStripMenuItem";
            this.actionToolStripMenuItem.Size = new System.Drawing.Size(173, 26);
            this.actionToolStripMenuItem.Text = "Action";
            this.actionToolStripMenuItem.Click += new System.EventHandler(this.insertNodeMenuItem_Click);
            // 
            // annotationToolStripMenuItem
            // 
            this.annotationToolStripMenuItem.Enabled = false;
            this.annotationToolStripMenuItem.Name = "annotationToolStripMenuItem";
            this.annotationToolStripMenuItem.Size = new System.Drawing.Size(173, 26);
            this.annotationToolStripMenuItem.Text = "Annotation";
            this.annotationToolStripMenuItem.Click += new System.EventHandler(this.insertNodeMenuItem_Click);
            // 
            // cutMenuItem
            // 
            this.cutMenuItem.Enabled = false;
            this.cutMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Cut;
            this.cutMenuItem.Name = "cutMenuItem";
            this.cutMenuItem.Size = new System.Drawing.Size(128, 26);
            this.cutMenuItem.Text = "Cut";
            this.cutMenuItem.Click += new System.EventHandler(this.cutMenuItem_Click);
            // 
            // copyMenuItem
            // 
            this.copyMenuItem.Enabled = false;
            this.copyMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Copy;
            this.copyMenuItem.Name = "copyMenuItem";
            this.copyMenuItem.Size = new System.Drawing.Size(128, 26);
            this.copyMenuItem.Text = "Copy";
            this.copyMenuItem.Click += new System.EventHandler(this.copyMenuItem_Click);
            // 
            // pasteMenuItem
            // 
            this.pasteMenuItem.Enabled = false;
            this.pasteMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Paste;
            this.pasteMenuItem.Name = "pasteMenuItem";
            this.pasteMenuItem.Size = new System.Drawing.Size(128, 26);
            this.pasteMenuItem.Text = "Paste";
            this.pasteMenuItem.Click += new System.EventHandler(this.pasteMenuItem_Click);
            // 
            // DeleteMenuItem
            // 
            this.DeleteMenuItem.Enabled = false;
            this.DeleteMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Delete;
            this.DeleteMenuItem.ImageTransparentColor = System.Drawing.Color.White;
            this.DeleteMenuItem.Name = "DeleteMenuItem";
            this.DeleteMenuItem.Size = new System.Drawing.Size(128, 26);
            this.DeleteMenuItem.Text = "Delete";
            this.DeleteMenuItem.Click += new System.EventHandler(this.DeleteMenuItem_Click);
            // 
            // btnResetTree
            // 
            this.btnResetTree.Enabled = false;
            this.btnResetTree.Location = new System.Drawing.Point(690, 4);
            this.btnResetTree.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnResetTree.Name = "btnResetTree";
            this.btnResetTree.Size = new System.Drawing.Size(87, 28);
            this.btnResetTree.TabIndex = 201;
            this.btnResetTree.Text = "Reset Tree";
            this.btnResetTree.Click += new System.EventHandler(this.btnResetTree_Click);
            // 
            // btnShowParents
            // 
            this.btnShowParents.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShowParents.Enabled = false;
            this.btnShowParents.ImageIndex = 0;
            this.btnShowParents.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleLeft;
            this.btnShowParents.Location = new System.Drawing.Point(595, 4);
            this.btnShowParents.LookAndFeel.SkinName = "Money Twins";
            this.btnShowParents.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnShowParents.Name = "btnShowParents";
            this.btnShowParents.Size = new System.Drawing.Size(89, 27);
            toolTipTitleItem1.Text = "Show Full Database Contents tree";
            toolTipItem1.LeftIndent = 6;
            toolTipItem1.Text = "Clear any current search results and display the full Database Contents tree.";
            superToolTip1.Items.Add(toolTipTitleItem1);
            superToolTip1.Items.Add(toolTipItem1);
            this.btnShowParents.SuperTip = superToolTip1;
            this.btnShowParents.TabIndex = 199;
            this.btnShowParents.Text = "Show Parents";
            this.btnShowParents.Click += new System.EventHandler(this.BtnShowParents_Click);
            // 
            // btnSearchAll
            // 
            this.btnSearchAll.Location = new System.Drawing.Point(208, 4);
            this.btnSearchAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSearchAll.Name = "btnSearchAll";
            this.btnSearchAll.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph, "", -1, true, true, false, DevExpress.XtraEditors.ImageLocation.MiddleCenter, global::Mobius.ClientComponents.Properties.Resources.Find, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), serializableAppearanceObject1, "", null, null, true)});
            this.btnSearchAll.Properties.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.btnSearchAll_Properties_ButtonClick);
            this.btnSearchAll.Size = new System.Drawing.Size(381, 24);
            this.btnSearchAll.TabIndex = 200;
            this.btnSearchAll.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.btnSearchAll_KeyPress);
            // 
            // panelSearchBar
            // 
            this.panelSearchBar.Controls.Add(this.btnResetTree);
            this.panelSearchBar.Controls.Add(this.btnShowParents);
            this.panelSearchBar.Controls.Add(this.btnSearchAll);
            this.panelSearchBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSearchBar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.panelSearchBar.Location = new System.Drawing.Point(0, 0);
            this.panelSearchBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelSearchBar.Name = "panelSearchBar";
            this.panelSearchBar.Size = new System.Drawing.Size(780, 34);
            this.panelSearchBar.TabIndex = 8;
            // 
            // ContentsTreeEditorControl2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.panelSearchBar);
            this.Name = "ContentsTreeEditorControl2";
            this.Size = new System.Drawing.Size(780, 446);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeList1)).EndInit();
            this.contentsTreeEditorMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnSearchAll.Properties)).EndInit();
            this.panelSearchBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private System.Windows.Forms.ContextMenuStrip contentsTreeEditorMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem InsertMenuItem;
        private System.Windows.Forms.ToolStripMenuItem folderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem databaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem projectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem metaTableToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem uRLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem actionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem annotationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DeleteMenuItem;
        private DevExpress.XtraEditors.SimpleButton btnResetTree;
        public DevExpress.XtraEditors.SimpleButton btnShowParents;
        private DevExpress.XtraEditors.ButtonEdit btnSearchAll;
        private System.Windows.Forms.FlowLayoutPanel panelSearchBar;
        private DevExpress.XtraTreeList.TreeList treeList1;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColType;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColLabel;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColName;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColItem;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColComments;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColDisabled;
    }
}
