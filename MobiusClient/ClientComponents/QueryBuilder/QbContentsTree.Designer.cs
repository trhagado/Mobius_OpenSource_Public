namespace Mobius.ClientComponents
{
	partial class QbContentsTree
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QbContentsTree));
			DevExpress.Utils.SuperToolTip superToolTip1 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipTitleItem toolTipTitleItem1 = new DevExpress.Utils.ToolTipTitleItem();
			DevExpress.Utils.ToolTipItem toolTipItem1 = new DevExpress.Utils.ToolTipItem();
			DevExpress.Utils.SuperToolTip superToolTip2 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipTitleItem toolTipTitleItem2 = new DevExpress.Utils.ToolTipTitleItem();
			DevExpress.Utils.ToolTipItem toolTipItem2 = new DevExpress.Utils.ToolTipItem();
			DevExpress.Utils.SuperToolTip superToolTip3 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipTitleItem toolTipTitleItem3 = new DevExpress.Utils.ToolTipTitleItem();
			DevExpress.Utils.ToolTipItem toolTipItem3 = new DevExpress.Utils.ToolTipItem();
			DevExpress.Utils.SuperToolTip superToolTip4 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipTitleItem toolTipTitleItem4 = new DevExpress.Utils.ToolTipTitleItem();
			DevExpress.Utils.ToolTipItem toolTipItem4 = new DevExpress.Utils.ToolTipItem();
			DevExpress.Utils.SuperToolTip superToolTip5 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipTitleItem toolTipTitleItem5 = new DevExpress.Utils.ToolTipTitleItem();
			DevExpress.Utils.ToolTipItem toolTipItem5 = new DevExpress.Utils.ToolTipItem();
			DevExpress.Utils.SuperToolTip superToolTip6 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipTitleItem toolTipTitleItem6 = new DevExpress.Utils.ToolTipTitleItem();
			DevExpress.Utils.ToolTipItem toolTipItem6 = new DevExpress.Utils.ToolTipItem();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.AnnotationButtonContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuNewAnnotation = new System.Windows.Forms.ToolStripMenuItem();
			this.menuExistingAnnotation = new System.Windows.Forms.ToolStripMenuItem();
			this.menuOpenAnnotation = new System.Windows.Forms.ToolStripMenuItem();
			this.CalcFieldButtonContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuNewCalcField = new System.Windows.Forms.ToolStripMenuItem();
			this.menuExistingCalcField = new System.Windows.Forms.ToolStripMenuItem();
			this.menuOpenCalcField = new System.Windows.Forms.ToolStripMenuItem();
			this.UserDatabaseButtonContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.NewUserDatabaseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.OpenUserDatabaseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.AnnotationButton = new DevExpress.XtraEditors.SimpleButton();
			this.UserDatabaseButton = new DevExpress.XtraEditors.SimpleButton();
			this.CalcFieldButton = new DevExpress.XtraEditors.SimpleButton();
			this.AddTableButton = new DevExpress.XtraEditors.SimpleButton();
			this.ContentsFind2 = new DevExpress.XtraEditors.SimpleButton();
			this.ContentsFindReset2 = new DevExpress.XtraEditors.SimpleButton();
			this.ContentsLabel = new DevExpress.XtraEditors.LabelControl();
			this.QbContentsTreeCtl = new Mobius.ClientComponents.ContentsTreeControl();
			this.AnnotationButtonContextMenu.SuspendLayout();
			this.CalcFieldButtonContextMenu.SuspendLayout();
			this.UserDatabaseButtonContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "WorldSearch.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "AnnotationTable.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "CalcField.bmp");
			this.Bitmaps16x16.Images.SetKeyName(3, "TableStruct.bmp");
			this.Bitmaps16x16.Images.SetKeyName(4, "ChevronRt.bmp");
			// 
			// AnnotationButtonContextMenu
			// 
			this.AnnotationButtonContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuNewAnnotation,
            this.menuExistingAnnotation,
            this.menuOpenAnnotation});
			this.AnnotationButtonContextMenu.Name = "AnnotationButtonContextMenu";
			this.AnnotationButtonContextMenu.Size = new System.Drawing.Size(292, 70);
			// 
			// menuNewAnnotation
			// 
			this.menuNewAnnotation.Image = global::Mobius.ClientComponents.Properties.Resources.NewDoc;
			this.menuNewAnnotation.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.menuNewAnnotation.Name = "menuNewAnnotation";
			this.menuNewAnnotation.Size = new System.Drawing.Size(291, 22);
			this.menuNewAnnotation.Text = "Add New Annotation Table to Query...";
			this.menuNewAnnotation.Click += new System.EventHandler(this.menuNewAnnotation_Click);
			// 
			// menuExistingAnnotation
			// 
			this.menuExistingAnnotation.Image = global::Mobius.ClientComponents.Properties.Resources.FileOpen;
			this.menuExistingAnnotation.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.menuExistingAnnotation.Name = "menuExistingAnnotation";
			this.menuExistingAnnotation.Size = new System.Drawing.Size(291, 22);
			this.menuExistingAnnotation.Text = "Add Existing Annotation Table to Query...";
			this.menuExistingAnnotation.Click += new System.EventHandler(this.menuExistingAnnotation_Click);
			// 
			// menuOpenAnnotation
			// 
			this.menuOpenAnnotation.Image = global::Mobius.ClientComponents.Properties.Resources.Edit;
			this.menuOpenAnnotation.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.menuOpenAnnotation.Name = "menuOpenAnnotation";
			this.menuOpenAnnotation.Size = new System.Drawing.Size(291, 22);
			this.menuOpenAnnotation.Text = "Edit Existing Annotation Table...";
			this.menuOpenAnnotation.Click += new System.EventHandler(this.menuOpenAnnotation_Click);
			// 
			// CalcFieldButtonContextMenu
			// 
			this.CalcFieldButtonContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuNewCalcField,
            this.menuExistingCalcField,
            this.menuOpenCalcField});
			this.CalcFieldButtonContextMenu.Name = "CalcFieldButtonContextMenu";
			this.CalcFieldButtonContextMenu.Size = new System.Drawing.Size(290, 70);
			// 
			// menuNewCalcField
			// 
			this.menuNewCalcField.Image = global::Mobius.ClientComponents.Properties.Resources.NewDoc;
			this.menuNewCalcField.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.menuNewCalcField.Name = "menuNewCalcField";
			this.menuNewCalcField.Size = new System.Drawing.Size(289, 22);
			this.menuNewCalcField.Text = "Add New Calculated Field to Query...";
			this.menuNewCalcField.Click += new System.EventHandler(this.menuNewCalcField_Click);
			// 
			// menuExistingCalcField
			// 
			this.menuExistingCalcField.Image = global::Mobius.ClientComponents.Properties.Resources.FileOpen;
			this.menuExistingCalcField.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.menuExistingCalcField.Name = "menuExistingCalcField";
			this.menuExistingCalcField.Size = new System.Drawing.Size(289, 22);
			this.menuExistingCalcField.Text = "Add Existing Calculated Field to Query...";
			this.menuExistingCalcField.Click += new System.EventHandler(this.menuExistingCalcField_Click);
			// 
			// menuOpenCalcField
			// 
			this.menuOpenCalcField.Image = global::Mobius.ClientComponents.Properties.Resources.Edit;
			this.menuOpenCalcField.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.menuOpenCalcField.Name = "menuOpenCalcField";
			this.menuOpenCalcField.Size = new System.Drawing.Size(289, 22);
			this.menuOpenCalcField.Text = "Edit Existing Calculated Field Definition...";
			this.menuOpenCalcField.Click += new System.EventHandler(this.menuOpenCalcField_Click);
			// 
			// UserDatabaseButtonContextMenu
			// 
			this.UserDatabaseButtonContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewUserDatabaseMenuItem,
            this.OpenUserDatabaseMenuItem});
			this.UserDatabaseButtonContextMenu.Name = "UserDatabaseButtonContextMenu";
			this.UserDatabaseButtonContextMenu.Size = new System.Drawing.Size(324, 48);
			// 
			// NewUserDatabaseMenuItem
			// 
			this.NewUserDatabaseMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.NewDoc;
			this.NewUserDatabaseMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.NewUserDatabaseMenuItem.Name = "NewUserDatabaseMenuItem";
			this.NewUserDatabaseMenuItem.Size = new System.Drawing.Size(323, 22);
			this.NewUserDatabaseMenuItem.Text = "New User Compound Database...";
			this.NewUserDatabaseMenuItem.Click += new System.EventHandler(this.NewUserDatabaseMenuItem_Click);
			// 
			// OpenUserDatabaseMenuItem
			// 
			this.OpenUserDatabaseMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.FileOpen;
			this.OpenUserDatabaseMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.OpenUserDatabaseMenuItem.Name = "OpenUserDatabaseMenuItem";
			this.OpenUserDatabaseMenuItem.Size = new System.Drawing.Size(323, 22);
			this.OpenUserDatabaseMenuItem.Text = "View/Edit Existing User Compound Databases...";
			this.OpenUserDatabaseMenuItem.Click += new System.EventHandler(this.OpenUserDatabaseMenuItem_Click);
			// 
			// AnnotationButton
			// 
			this.AnnotationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AnnotationButton.ImageOptions.ImageIndex = 1;
			this.AnnotationButton.ImageOptions.ImageList = this.Bitmaps16x16;
			this.AnnotationButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.AnnotationButton.Location = new System.Drawing.Point(1, 276);
			this.AnnotationButton.Name = "AnnotationButton";
			this.AnnotationButton.Size = new System.Drawing.Size(22, 22);
			toolTipTitleItem1.Text = "Annotation Tables";
			toolTipItem1.LeftIndent = 6;
			toolTipItem1.Text = "Create and edit annotation tables.";
			superToolTip1.Items.Add(toolTipTitleItem1);
			superToolTip1.Items.Add(toolTipItem1);
			this.AnnotationButton.SuperTip = superToolTip1;
			this.AnnotationButton.TabIndex = 192;
			this.AnnotationButton.Click += new System.EventHandler(this.AnnotationButton_Click);
			// 
			// UserDatabaseButton
			// 
			this.UserDatabaseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.UserDatabaseButton.Enabled = false;
			this.UserDatabaseButton.ImageOptions.ImageIndex = 3;
			this.UserDatabaseButton.ImageOptions.ImageList = this.Bitmaps16x16;
			this.UserDatabaseButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.UserDatabaseButton.Location = new System.Drawing.Point(51, 276);
			this.UserDatabaseButton.Name = "UserDatabaseButton";
			this.UserDatabaseButton.Size = new System.Drawing.Size(22, 22);
			toolTipTitleItem2.Text = "User Compound Databases";
			toolTipItem2.LeftIndent = 6;
			toolTipItem2.Text = "Create and edit user compound databases";
			superToolTip2.Items.Add(toolTipTitleItem2);
			superToolTip2.Items.Add(toolTipItem2);
			this.UserDatabaseButton.SuperTip = superToolTip2;
			this.UserDatabaseButton.TabIndex = 193;
			this.UserDatabaseButton.Visible = false;
			this.UserDatabaseButton.Click += new System.EventHandler(this.UserDatabaseButton_Click);
			// 
			// CalcFieldButton
			// 
			this.CalcFieldButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CalcFieldButton.ImageOptions.ImageIndex = 2;
			this.CalcFieldButton.ImageOptions.ImageList = this.Bitmaps16x16;
			this.CalcFieldButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.CalcFieldButton.Location = new System.Drawing.Point(26, 276);
			this.CalcFieldButton.Name = "CalcFieldButton";
			this.CalcFieldButton.Size = new System.Drawing.Size(22, 22);
			toolTipTitleItem3.Text = "Calculated Fields";
			toolTipItem3.LeftIndent = 6;
			toolTipItem3.Text = "Create and edit calculated fields";
			superToolTip3.Items.Add(toolTipTitleItem3);
			superToolTip3.Items.Add(toolTipItem3);
			this.CalcFieldButton.SuperTip = superToolTip3;
			this.CalcFieldButton.TabIndex = 194;
			this.CalcFieldButton.Click += new System.EventHandler(this.CalcFieldButton_Click);
			// 
			// AddTableButton
			// 
			this.AddTableButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AddTableButton.ImageOptions.ImageIndex = 4;
			this.AddTableButton.ImageOptions.ImageList = this.Bitmaps16x16;
			this.AddTableButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleRight;
			this.AddTableButton.Location = new System.Drawing.Point(251, 276);
			this.AddTableButton.Name = "AddTableButton";
			this.AddTableButton.Size = new System.Drawing.Size(48, 22);
			toolTipTitleItem4.Text = "Add Table to Query";
			toolTipItem4.LeftIndent = 6;
			toolTipItem4.Text = "Add the currently selected data table to the query.";
			superToolTip4.Items.Add(toolTipTitleItem4);
			superToolTip4.Items.Add(toolTipItem4);
			this.AddTableButton.SuperTip = superToolTip4;
			this.AddTableButton.TabIndex = 195;
			this.AddTableButton.Text = " Add";
			this.AddTableButton.Click += new System.EventHandler(this.AddTable_Click);
			// 
			// ContentsFind2
			// 
			this.ContentsFind2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ContentsFind2.ImageOptions.ImageIndex = 0;
			this.ContentsFind2.ImageOptions.ImageList = this.Bitmaps16x16;
			this.ContentsFind2.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleLeft;
			this.ContentsFind2.Location = new System.Drawing.Point(174, 2);
			this.ContentsFind2.Name = "ContentsFind2";
			this.ContentsFind2.Size = new System.Drawing.Size(62, 22);
			toolTipTitleItem5.Text = "Find in Database Contents";
			toolTipItem5.LeftIndent = 6;
			toolTipItem5.Text = "Search the Database Contents tree for one or more words, partial words or an assa" +
    "y code.\r\n";
			superToolTip5.Items.Add(toolTipTitleItem5);
			superToolTip5.Items.Add(toolTipItem5);
			this.ContentsFind2.SuperTip = superToolTip5;
			this.ContentsFind2.TabIndex = 196;
			this.ContentsFind2.Text = "Find...";
			this.ContentsFind2.Click += new System.EventHandler(this.ContentsFind_Click);
			// 
			// ContentsFindReset2
			// 
			this.ContentsFindReset2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ContentsFindReset2.ImageOptions.ImageIndex = 0;
			this.ContentsFindReset2.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleLeft;
			this.ContentsFindReset2.Location = new System.Drawing.Point(239, 2);
			this.ContentsFindReset2.Name = "ContentsFindReset2";
			this.ContentsFindReset2.Size = new System.Drawing.Size(60, 22);
			toolTipTitleItem6.Text = "Show Full Database Contents tree";
			toolTipItem6.LeftIndent = 6;
			toolTipItem6.Text = "Clear any current search results and display the full Database Contents tree.";
			superToolTip6.Items.Add(toolTipTitleItem6);
			superToolTip6.Items.Add(toolTipItem6);
			this.ContentsFindReset2.SuperTip = superToolTip6;
			this.ContentsFindReset2.TabIndex = 197;
			this.ContentsFindReset2.Text = "Show Full";
			this.ContentsFindReset2.Click += new System.EventHandler(this.ContentsFindReset_Click);
			// 
			// ContentsLabel
			// 
			this.ContentsLabel.Appearance.Font = new System.Drawing.Font("Arial", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
			this.ContentsLabel.Appearance.Options.UseFont = true;
			this.ContentsLabel.Location = new System.Drawing.Point(5, 5);
			this.ContentsLabel.Name = "ContentsLabel";
			this.ContentsLabel.Size = new System.Drawing.Size(133, 17);
			this.ContentsLabel.TabIndex = 198;
			this.ContentsLabel.Text = "Database Contents";
			// 
			// QbContentsTreeCtl
			// 
			this.QbContentsTreeCtl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.QbContentsTreeCtl.Location = new System.Drawing.Point(1, 27);
			this.QbContentsTreeCtl.Name = "QbContentsTreeCtl";
			this.QbContentsTreeCtl.Size = new System.Drawing.Size(299, 247);
			this.QbContentsTreeCtl.TabIndex = 0;
			this.QbContentsTreeCtl.FocusedNodeChanged += new System.EventHandler(this.ContentsTree_FocusedNodeChanged);
			this.QbContentsTreeCtl.Click += new System.EventHandler(this.ContentsTree_Click);
			this.QbContentsTreeCtl.DoubleClick += new System.EventHandler(this.ContentsTree_DoubleClick);
			this.QbContentsTreeCtl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ContentsTree_MouseDown);
			this.QbContentsTreeCtl.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ContentsTree_MouseClick);
			// 
			// QbContentsTree
			// 
			this.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Appearance.Options.UseBackColor = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ContentsLabel);
			this.Controls.Add(this.UserDatabaseButton);
			this.Controls.Add(this.ContentsFindReset2);
			this.Controls.Add(this.ContentsFind2);
			this.Controls.Add(this.AnnotationButton);
			this.Controls.Add(this.AddTableButton);
			this.Controls.Add(this.CalcFieldButton);
			this.Controls.Add(this.QbContentsTreeCtl);
			this.Name = "QbContentsTree";
			this.Size = new System.Drawing.Size(301, 300);
			this.VisibleChanged += new System.EventHandler(this.QbContentsTree_VisibleChanged);
			this.AnnotationButtonContextMenu.ResumeLayout(false);
			this.CalcFieldButtonContextMenu.ResumeLayout(false);
			this.UserDatabaseButtonContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ImageList Bitmaps16x16;
		public System.Windows.Forms.ContextMenuStrip AnnotationButtonContextMenu;
		public System.Windows.Forms.ToolStripMenuItem menuNewAnnotation;
		public System.Windows.Forms.ToolStripMenuItem menuExistingAnnotation;
		public System.Windows.Forms.ToolStripMenuItem menuOpenAnnotation;
		public System.Windows.Forms.ContextMenuStrip CalcFieldButtonContextMenu;
		public System.Windows.Forms.ToolStripMenuItem menuNewCalcField;
		public System.Windows.Forms.ToolStripMenuItem menuExistingCalcField;
		public System.Windows.Forms.ToolStripMenuItem menuOpenCalcField;
		public System.Windows.Forms.ContextMenuStrip UserDatabaseButtonContextMenu;
		public System.Windows.Forms.ToolStripMenuItem NewUserDatabaseMenuItem;
		public System.Windows.Forms.ToolStripMenuItem OpenUserDatabaseMenuItem;
		public DevExpress.XtraEditors.SimpleButton AnnotationButton;
		public DevExpress.XtraEditors.SimpleButton UserDatabaseButton;
		public DevExpress.XtraEditors.SimpleButton CalcFieldButton;
		public DevExpress.XtraEditors.SimpleButton AddTableButton;
		public DevExpress.XtraEditors.SimpleButton ContentsFind2;
		public DevExpress.XtraEditors.SimpleButton ContentsFindReset2;
		private DevExpress.XtraEditors.LabelControl ContentsLabel;
		public ContentsTreeControl QbContentsTreeCtl;
	}
}
