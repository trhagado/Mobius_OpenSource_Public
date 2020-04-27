namespace Mobius.ClientComponents
{
	partial class CidListEditor
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CidListEditor));
			Mobius.Data.MoleculeMx moleculeMx1 = new Mobius.Data.MoleculeMx();
			this.ListLabel = new System.Windows.Forms.GroupBox();
			this.CidListCtl = new DevExpress.XtraEditors.MemoEdit();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.CloseButton = new DevExpress.XtraEditors.SimpleButton();
			this.CopyStructure = new DevExpress.XtraEditors.SimpleButton();
			this.DisplayFrame = new System.Windows.Forms.GroupBox();
			this.MolCtl = new Mobius.ClientComponents.MoleculeControl();
			this.Weight = new DevExpress.XtraEditors.TextEdit();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.CidCtl = new DevExpress.XtraEditors.TextEdit();
			this.Formula = new DevExpress.XtraEditors.TextEdit();
			this.HeavyAtoms = new DevExpress.XtraEditors.TextEdit();
			this.RNLabel = new DevExpress.XtraEditors.LabelControl();
			this._Label2_8 = new DevExpress.XtraEditors.LabelControl();
			this._Label2_9 = new DevExpress.XtraEditors.LabelControl();
			this.Timer = new System.Windows.Forms.Timer(this.components);
			this.BarManager = new DevExpress.XtraBars.BarManager(this.components);
			this.MainMenuBar = new DevExpress.XtraBars.Bar();
			this.FileMenu = new DevExpress.XtraBars.BarSubItem();
			this.EditMenu = new DevExpress.XtraBars.BarSubItem();
			this.CutMenuItem = new DevExpress.XtraBars.BarButtonItem();
			this.CopyMenuItem = new DevExpress.XtraBars.BarButtonItem();
			this.CopyAsCsvMenuItem = new DevExpress.XtraBars.BarButtonItem();
			this.PasteMenuItem = new DevExpress.XtraBars.BarButtonItem();
			this.DeleteMenuItem = new DevExpress.XtraBars.BarButtonItem();
			this.ClearListMenuItem = new DevExpress.XtraBars.BarButtonItem();
			this.SortMenu = new DevExpress.XtraBars.BarSubItem();
			this.SortAscendingMenuItem = new DevExpress.XtraBars.BarButtonItem();
			this.SortDescendingMenuItem = new DevExpress.XtraBars.BarButtonItem();
			this.SaltsMenu = new DevExpress.XtraBars.BarSubItem();
			this.InsertSaltsForCurrentCompound = new DevExpress.XtraBars.BarButtonItem();
			this.InsertSaltsForAllCompounds = new DevExpress.XtraBars.BarButtonItem();
			this.GroupSalts = new DevExpress.XtraBars.BarButtonItem();
			this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
			this.ValidateNumbers = new DevExpress.XtraEditors.CheckEdit();
			this.RemoveDuplicates = new DevExpress.XtraEditors.CheckEdit();
			this.StatusMessage = new DevExpress.XtraEditors.LabelControl();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.FileContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.SaveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SaveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SaveAsSavedListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.SaveAsTempListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SaveAsNewTempListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.CloseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SaveAndClose = new DevExpress.XtraEditors.SimpleButton();
			this.SaveMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
			this.FileMenuItemPositionPanel = new DevExpress.XtraEditors.PanelControl();
			this.ListLabel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CidListCtl.Properties)).BeginInit();
			this.DisplayFrame.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Weight.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CidCtl.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Formula.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HeavyAtoms.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BarManager)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ValidateNumbers.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RemoveDuplicates.Properties)).BeginInit();
			this.FileContextMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FileMenuItemPositionPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// ListLabel
			// 
			this.ListLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.ListLabel.Controls.Add(this.CidListCtl);
			this.ListLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ListLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ListLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ListLabel.Location = new System.Drawing.Point(8, 35);
			this.ListLabel.Name = "ListLabel";
			this.ListLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ListLabel.Size = new System.Drawing.Size(217, 459);
			this.ListLabel.TabIndex = 31;
			this.ListLabel.TabStop = false;
			this.ListLabel.Text = " CmpdId List - (Enter one number per line) ";
			// 
			// CidListCtl
			// 
			this.CidListCtl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CidListCtl.Location = new System.Drawing.Point(8, 24);
			this.CidListCtl.Name = "CidListCtl";
			this.CidListCtl.Size = new System.Drawing.Size(203, 374);
			this.CidListCtl.TabIndex = 47;
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "Save.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "FileClose.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "Cut.bmp");
			this.Bitmaps16x16.Images.SetKeyName(3, "Copy.bmp");
			this.Bitmaps16x16.Images.SetKeyName(4, "Paste.bmp");
			this.Bitmaps16x16.Images.SetKeyName(5, "Delete.bmp");
			this.Bitmaps16x16.Images.SetKeyName(6, "SortAscending.bmp");
			this.Bitmaps16x16.Images.SetKeyName(7, "SortDescending.bmp");
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Appearance.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CloseButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.CloseButton.Appearance.Options.UseFont = true;
			this.CloseButton.Appearance.Options.UseForeColor = true;
			this.CloseButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.CloseButton.ImageOptions.ImageList = this.Bitmaps16x16;
			this.CloseButton.Location = new System.Drawing.Point(552, 508);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CloseButton.Size = new System.Drawing.Size(61, 23);
			this.CloseButton.TabIndex = 10;
			this.CloseButton.Tag = "Cancel";
			this.CloseButton.Text = "Close";
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// CopyStructure
			// 
			this.CopyStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CopyStructure.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CopyStructure.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.CopyStructure.Appearance.Options.UseFont = true;
			this.CopyStructure.Appearance.Options.UseForeColor = true;
			this.CopyStructure.Cursor = System.Windows.Forms.Cursors.Default;
			this.CopyStructure.Location = new System.Drawing.Point(104, 355);
			this.CopyStructure.Name = "CopyStructure";
			this.CopyStructure.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CopyStructure.Size = new System.Drawing.Size(171, 23);
			this.CopyStructure.TabIndex = 12;
			this.CopyStructure.Text = "C&opy Structure to Clipboard";
			this.CopyStructure.Click += new System.EventHandler(this.CopyStructure_Click);
			// 
			// DisplayFrame
			// 
			this.DisplayFrame.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DisplayFrame.Controls.Add(this.MolCtl);
			this.DisplayFrame.Controls.Add(this.Weight);
			this.DisplayFrame.Controls.Add(this.labelControl1);
			this.DisplayFrame.Controls.Add(this.CopyStructure);
			this.DisplayFrame.Controls.Add(this.CidCtl);
			this.DisplayFrame.Controls.Add(this.Formula);
			this.DisplayFrame.Controls.Add(this.HeavyAtoms);
			this.DisplayFrame.Controls.Add(this.RNLabel);
			this.DisplayFrame.Controls.Add(this._Label2_8);
			this.DisplayFrame.Controls.Add(this._Label2_9);
			this.DisplayFrame.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DisplayFrame.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DisplayFrame.ForeColor = System.Drawing.SystemColors.ControlText;
			this.DisplayFrame.Location = new System.Drawing.Point(236, 35);
			this.DisplayFrame.Name = "DisplayFrame";
			this.DisplayFrame.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.DisplayFrame.Size = new System.Drawing.Size(372, 384);
			this.DisplayFrame.TabIndex = 32;
			this.DisplayFrame.TabStop = false;
			this.DisplayFrame.Text = "Item 1 of 1000";
			// 
			// MolCtl
			// 
			this.MolCtl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MolCtl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.MolCtl.Location = new System.Drawing.Point(8, 106);
			this.MolCtl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			moleculeMx1.BackColor = System.Drawing.Color.Empty;
			moleculeMx1.DateTimeValue = new System.DateTime(((long)(0)));
			moleculeMx1.DbLink = "";
			moleculeMx1.Filtered = false;
			moleculeMx1.ForeColor = System.Drawing.Color.Black;
			moleculeMx1.FormattedBitmap = null;
			moleculeMx1.FormattedText = null;
			moleculeMx1.Hyperlink = "";
			moleculeMx1.Hyperlinked = false;
			moleculeMx1.IsNonExistant = false;
			moleculeMx1.IsRetrievingValue = false;
			moleculeMx1.Modified = false;
			moleculeMx1.NumericValue = -4194303D;
			this.MolCtl.Molecule = moleculeMx1;
			this.MolCtl.Name = "MolCtl";
			this.MolCtl.Size = new System.Drawing.Size(357, 243);
			this.MolCtl.TabIndex = 218;
			// 
			// Weight
			// 
			this.Weight.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.Weight.Location = new System.Drawing.Point(88, 72);
			this.Weight.Name = "Weight";
			this.Weight.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.Weight.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Weight.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Weight.Properties.Appearance.Options.UseBackColor = true;
			this.Weight.Properties.Appearance.Options.UseFont = true;
			this.Weight.Properties.Appearance.Options.UseForeColor = true;
			this.Weight.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Weight.Size = new System.Drawing.Size(70, 20);
			this.Weight.TabIndex = 69;
			this.Weight.Tag = "MF";
			// 
			// labelControl1
			// 
			this.labelControl1.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl1.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.labelControl1.Appearance.Options.UseFont = true;
			this.labelControl1.Appearance.Options.UseForeColor = true;
			this.labelControl1.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl1.Location = new System.Drawing.Point(171, 75);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(66, 13);
			this.labelControl1.TabIndex = 70;
			this.labelControl1.Text = "Heavy Atoms:";
			// 
			// CidCtl
			// 
			this.CidCtl.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.CidCtl.EditValue = "";
			this.CidCtl.Location = new System.Drawing.Point(88, 23);
			this.CidCtl.Name = "CidCtl";
			this.CidCtl.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.CidCtl.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CidCtl.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.CidCtl.Properties.Appearance.Options.UseBackColor = true;
			this.CidCtl.Properties.Appearance.Options.UseFont = true;
			this.CidCtl.Properties.Appearance.Options.UseForeColor = true;
			this.CidCtl.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CidCtl.Size = new System.Drawing.Size(205, 20);
			this.CidCtl.TabIndex = 20;
			// 
			// Formula
			// 
			this.Formula.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.Formula.Location = new System.Drawing.Point(88, 47);
			this.Formula.Name = "Formula";
			this.Formula.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.Formula.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Formula.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Formula.Properties.Appearance.Options.UseBackColor = true;
			this.Formula.Properties.Appearance.Options.UseFont = true;
			this.Formula.Properties.Appearance.Options.UseForeColor = true;
			this.Formula.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Formula.Size = new System.Drawing.Size(205, 20);
			this.Formula.TabIndex = 19;
			this.Formula.Tag = "MF";
			// 
			// HeavyAtoms
			// 
			this.HeavyAtoms.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.HeavyAtoms.Location = new System.Drawing.Point(248, 72);
			this.HeavyAtoms.Name = "HeavyAtoms";
			this.HeavyAtoms.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.HeavyAtoms.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HeavyAtoms.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.HeavyAtoms.Properties.Appearance.Options.UseBackColor = true;
			this.HeavyAtoms.Properties.Appearance.Options.UseFont = true;
			this.HeavyAtoms.Properties.Appearance.Options.UseForeColor = true;
			this.HeavyAtoms.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.HeavyAtoms.Size = new System.Drawing.Size(45, 20);
			this.HeavyAtoms.TabIndex = 18;
			this.HeavyAtoms.Tag = "MW";
			// 
			// RNLabel
			// 
			this.RNLabel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RNLabel.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.RNLabel.Appearance.Options.UseFont = true;
			this.RNLabel.Appearance.Options.UseForeColor = true;
			this.RNLabel.Appearance.Options.UseTextOptions = true;
			this.RNLabel.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
			this.RNLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.RNLabel.Cursor = System.Windows.Forms.Cursors.Default;
			this.RNLabel.Location = new System.Drawing.Point(8, 26);
			this.RNLabel.Name = "RNLabel";
			this.RNLabel.Size = new System.Drawing.Size(74, 16);
			this.RNLabel.TabIndex = 23;
			this.RNLabel.Text = "CmpdId:";
			// 
			// _Label2_8
			// 
			this._Label2_8.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._Label2_8.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this._Label2_8.Appearance.Options.UseFont = true;
			this._Label2_8.Appearance.Options.UseForeColor = true;
			this._Label2_8.Appearance.Options.UseTextOptions = true;
			this._Label2_8.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
			this._Label2_8.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this._Label2_8.Cursor = System.Windows.Forms.Cursors.Default;
			this._Label2_8.Location = new System.Drawing.Point(9, 75);
			this._Label2_8.Name = "_Label2_8";
			this._Label2_8.Size = new System.Drawing.Size(73, 16);
			this._Label2_8.TabIndex = 22;
			this._Label2_8.Text = "Mol. Wt.:";
			// 
			// _Label2_9
			// 
			this._Label2_9.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._Label2_9.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this._Label2_9.Appearance.Options.UseFont = true;
			this._Label2_9.Appearance.Options.UseForeColor = true;
			this._Label2_9.Appearance.Options.UseTextOptions = true;
			this._Label2_9.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
			this._Label2_9.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this._Label2_9.Cursor = System.Windows.Forms.Cursors.Default;
			this._Label2_9.Location = new System.Drawing.Point(9, 50);
			this._Label2_9.Name = "_Label2_9";
			this._Label2_9.Size = new System.Drawing.Size(73, 16);
			this._Label2_9.TabIndex = 21;
			this._Label2_9.Text = "Mol. Formula:";
			// 
			// Timer
			// 
			this.Timer.Tick += new System.EventHandler(this.Timer_Tick);
			// 
			// BarManager
			// 
			this.BarManager.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.MainMenuBar});
			this.BarManager.DockControls.Add(this.barDockControlTop);
			this.BarManager.DockControls.Add(this.barDockControlBottom);
			this.BarManager.DockControls.Add(this.barDockControlLeft);
			this.BarManager.DockControls.Add(this.barDockControlRight);
			this.BarManager.Form = this;
			this.BarManager.Images = this.Bitmaps16x16;
			this.BarManager.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.FileMenu,
            this.EditMenu,
            this.SortMenu,
            this.SaltsMenu,
            this.CutMenuItem,
            this.CopyMenuItem,
            this.PasteMenuItem,
            this.DeleteMenuItem,
            this.ClearListMenuItem,
            this.SortAscendingMenuItem,
            this.SortDescendingMenuItem,
            this.InsertSaltsForCurrentCompound,
            this.InsertSaltsForAllCompounds,
            this.GroupSalts,
            this.CopyAsCsvMenuItem});
			this.BarManager.MainMenu = this.MainMenuBar;
			this.BarManager.MaxItemId = 28;
			// 
			// MainMenuBar
			// 
			this.MainMenuBar.BarName = "MainMenuBar";
			this.MainMenuBar.DockCol = 0;
			this.MainMenuBar.DockRow = 0;
			this.MainMenuBar.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
			this.MainMenuBar.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.FileMenu),
            new DevExpress.XtraBars.LinkPersistInfo(this.EditMenu),
            new DevExpress.XtraBars.LinkPersistInfo(this.SortMenu),
            new DevExpress.XtraBars.LinkPersistInfo(this.SaltsMenu)});
			this.MainMenuBar.OptionsBar.AllowQuickCustomization = false;
			this.MainMenuBar.OptionsBar.DrawDragBorder = false;
			this.MainMenuBar.OptionsBar.MultiLine = true;
			this.MainMenuBar.OptionsBar.UseWholeRow = true;
			this.MainMenuBar.Text = "MainMenuBar";
			// 
			// FileMenu
			// 
			this.FileMenu.Caption = "&File";
			this.FileMenu.Id = 10;
			this.FileMenu.Name = "FileMenu";
			this.FileMenu.GetItemData += new System.EventHandler(this.FileMenu_GetItemData);
			// 
			// EditMenu
			// 
			this.EditMenu.Caption = "&Edit";
			this.EditMenu.Id = 11;
			this.EditMenu.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.CutMenuItem),
            new DevExpress.XtraBars.LinkPersistInfo(this.CopyMenuItem),
            new DevExpress.XtraBars.LinkPersistInfo(this.CopyAsCsvMenuItem),
            new DevExpress.XtraBars.LinkPersistInfo(this.PasteMenuItem),
            new DevExpress.XtraBars.LinkPersistInfo(this.DeleteMenuItem),
            new DevExpress.XtraBars.LinkPersistInfo(this.ClearListMenuItem, true)});
			this.EditMenu.Name = "EditMenu";
			// 
			// CutMenuItem
			// 
			this.CutMenuItem.Caption = "&Cut";
			this.CutMenuItem.Id = 17;
			this.CutMenuItem.ImageOptions.ImageIndex = 2;
			this.CutMenuItem.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X));
			this.CutMenuItem.Name = "CutMenuItem";
			this.CutMenuItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.CutMenuItem_ItemClick);
			// 
			// CopyMenuItem
			// 
			this.CopyMenuItem.Caption = "&Copy";
			this.CopyMenuItem.Id = 18;
			this.CopyMenuItem.ImageOptions.ImageIndex = 3;
			this.CopyMenuItem.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C));
			this.CopyMenuItem.Name = "CopyMenuItem";
			this.CopyMenuItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.CopyMenuItem_ItemClick);
			// 
			// CopyAsCsvMenuItem
			// 
			this.CopyAsCsvMenuItem.Caption = "Copy as Csv (1, 2, ...)";
			this.CopyAsCsvMenuItem.Id = 27;
			this.CopyAsCsvMenuItem.Name = "CopyAsCsvMenuItem";
			this.CopyAsCsvMenuItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.CopyAsCsvMenuItem_ItemClick);
			// 
			// PasteMenuItem
			// 
			this.PasteMenuItem.Caption = "&Paste";
			this.PasteMenuItem.Id = 19;
			this.PasteMenuItem.ImageOptions.ImageIndex = 4;
			this.PasteMenuItem.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V));
			this.PasteMenuItem.Name = "PasteMenuItem";
			this.PasteMenuItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.PasteMenuItem_ItemClick);
			// 
			// DeleteMenuItem
			// 
			this.DeleteMenuItem.Caption = "&Delete";
			this.DeleteMenuItem.Id = 20;
			this.DeleteMenuItem.ImageOptions.ImageIndex = 5;
			this.DeleteMenuItem.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D));
			this.DeleteMenuItem.Name = "DeleteMenuItem";
			this.DeleteMenuItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.DeleteMenuItem_ItemClick);
			// 
			// ClearListMenuItem
			// 
			this.ClearListMenuItem.Caption = "Clear List...";
			this.ClearListMenuItem.Id = 21;
			this.ClearListMenuItem.Name = "ClearListMenuItem";
			this.ClearListMenuItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.ClearListMenuItem_ItemClick);
			// 
			// SortMenu
			// 
			this.SortMenu.Caption = "Sort";
			this.SortMenu.Id = 12;
			this.SortMenu.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.SortAscendingMenuItem),
            new DevExpress.XtraBars.LinkPersistInfo(this.SortDescendingMenuItem)});
			this.SortMenu.Name = "SortMenu";
			// 
			// SortAscendingMenuItem
			// 
			this.SortAscendingMenuItem.Caption = "&Ascending";
			this.SortAscendingMenuItem.Id = 22;
			this.SortAscendingMenuItem.ImageOptions.ImageIndex = 6;
			this.SortAscendingMenuItem.Name = "SortAscendingMenuItem";
			this.SortAscendingMenuItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.SortAscendingMenuItem_ItemClick);
			// 
			// SortDescendingMenuItem
			// 
			this.SortDescendingMenuItem.Caption = "&Descending";
			this.SortDescendingMenuItem.Id = 23;
			this.SortDescendingMenuItem.ImageOptions.ImageIndex = 7;
			this.SortDescendingMenuItem.Name = "SortDescendingMenuItem";
			this.SortDescendingMenuItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.SortDescendingMenuItem_ItemClick);
			// 
			// SaltsMenu
			// 
			this.SaltsMenu.Caption = "Salts";
			this.SaltsMenu.Id = 13;
			this.SaltsMenu.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.InsertSaltsForCurrentCompound),
            new DevExpress.XtraBars.LinkPersistInfo(this.InsertSaltsForAllCompounds),
            new DevExpress.XtraBars.LinkPersistInfo(this.GroupSalts)});
			this.SaltsMenu.Name = "SaltsMenu";
			// 
			// InsertSaltsForCurrentCompound
			// 
			this.InsertSaltsForCurrentCompound.Caption = "&Insert for current compound";
			this.InsertSaltsForCurrentCompound.Id = 24;
			this.InsertSaltsForCurrentCompound.Name = "InsertSaltsForCurrentCompound";
			this.InsertSaltsForCurrentCompound.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.InsertSaltsForCurrentCompound_ItemClick);
			// 
			// InsertSaltsForAllCompounds
			// 
			this.InsertSaltsForAllCompounds.Caption = "Insert for &all compounds";
			this.InsertSaltsForAllCompounds.Id = 25;
			this.InsertSaltsForAllCompounds.Name = "InsertSaltsForAllCompounds";
			this.InsertSaltsForAllCompounds.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.InsertSaltsForAllCompounds_ItemClick);
			// 
			// GroupSalts
			// 
			this.GroupSalts.Caption = "&Group salts of the same parent structure together";
			this.GroupSalts.Id = 26;
			this.GroupSalts.Name = "GroupSalts";
			this.GroupSalts.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.GroupSalts_ItemClick);
			// 
			// barDockControlTop
			// 
			this.barDockControlTop.CausesValidation = false;
			this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
			this.barDockControlTop.Manager = this.BarManager;
			this.barDockControlTop.Size = new System.Drawing.Size(618, 22);
			// 
			// barDockControlBottom
			// 
			this.barDockControlBottom.CausesValidation = false;
			this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.barDockControlBottom.Location = new System.Drawing.Point(0, 536);
			this.barDockControlBottom.Manager = this.BarManager;
			this.barDockControlBottom.Size = new System.Drawing.Size(618, 0);
			// 
			// barDockControlLeft
			// 
			this.barDockControlLeft.CausesValidation = false;
			this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
			this.barDockControlLeft.Location = new System.Drawing.Point(0, 22);
			this.barDockControlLeft.Manager = this.BarManager;
			this.barDockControlLeft.Size = new System.Drawing.Size(0, 514);
			// 
			// barDockControlRight
			// 
			this.barDockControlRight.CausesValidation = false;
			this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
			this.barDockControlRight.Location = new System.Drawing.Point(618, 22);
			this.barDockControlRight.Manager = this.BarManager;
			this.barDockControlRight.Size = new System.Drawing.Size(0, 514);
			// 
			// ValidateNumbers
			// 
			this.ValidateNumbers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ValidateNumbers.EditValue = true;
			this.ValidateNumbers.Location = new System.Drawing.Point(247, 425);
			this.ValidateNumbers.Name = "ValidateNumbers";
			this.ValidateNumbers.Properties.Caption = "Validate numbers";
			this.ValidateNumbers.Size = new System.Drawing.Size(139, 19);
			this.ValidateNumbers.TabIndex = 115;
			// 
			// RemoveDuplicates
			// 
			this.RemoveDuplicates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RemoveDuplicates.EditValue = true;
			this.RemoveDuplicates.Location = new System.Drawing.Point(247, 449);
			this.RemoveDuplicates.Name = "RemoveDuplicates";
			this.RemoveDuplicates.Properties.Caption = "Remove duplicates";
			this.RemoveDuplicates.Size = new System.Drawing.Size(139, 19);
			this.RemoveDuplicates.TabIndex = 116;
			// 
			// StatusMessage
			// 
			this.StatusMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.StatusMessage.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StatusMessage.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.StatusMessage.Appearance.Options.UseFont = true;
			this.StatusMessage.Appearance.Options.UseForeColor = true;
			this.StatusMessage.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.StatusMessage.Cursor = System.Windows.Forms.Cursors.Default;
			this.StatusMessage.Location = new System.Drawing.Point(9, 510);
			this.StatusMessage.Name = "StatusMessage";
			this.StatusMessage.Size = new System.Drawing.Size(361, 19);
			this.StatusMessage.TabIndex = 121;
			this.StatusMessage.Text = "Status message";
			// 
			// labelControl2
			// 
			this.labelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl2.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl2.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.labelControl2.Appearance.Options.UseFont = true;
			this.labelControl2.Appearance.Options.UseForeColor = true;
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl2.LineVisible = true;
			this.labelControl2.Location = new System.Drawing.Point(0, 495);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(621, 10);
			this.labelControl2.TabIndex = 71;
			// 
			// FileContextMenu
			// 
			this.FileContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.FileContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SaveMenuItem,
            this.SaveAsMenuItem,
            this.CloseMenuItem});
			this.FileContextMenu.Name = "StandardCalcContextMenu";
			this.FileContextMenu.Size = new System.Drawing.Size(119, 82);
			// 
			// SaveMenuItem
			// 
			this.SaveMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("SaveMenuItem.Image")));
			this.SaveMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.SaveMenuItem.Name = "SaveMenuItem";
			this.SaveMenuItem.Size = new System.Drawing.Size(118, 26);
			this.SaveMenuItem.Text = "&Save";
			this.SaveMenuItem.Click += new System.EventHandler(this.SaveMenuItem_Click);
			// 
			// SaveAsMenuItem
			// 
			this.SaveAsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SaveAsSavedListMenuItem,
            this.toolStripMenuItem1,
            this.SaveAsTempListMenuItem,
            this.SaveAsNewTempListMenuItem});
			this.SaveAsMenuItem.Name = "SaveAsMenuItem";
			this.SaveAsMenuItem.Size = new System.Drawing.Size(118, 26);
			this.SaveAsMenuItem.Text = "Save &As";
			this.SaveAsMenuItem.DropDownOpening += new System.EventHandler(this.SaveAsMenuItem_DropDownOpening);
			// 
			// SaveAsSavedListMenuItem
			// 
			this.SaveAsSavedListMenuItem.Name = "SaveAsSavedListMenuItem";
			this.SaveAsSavedListMenuItem.Size = new System.Drawing.Size(188, 22);
			this.SaveAsSavedListMenuItem.Text = "Saved List...";
			this.SaveAsSavedListMenuItem.Click += new System.EventHandler(this.SaveAsSavedListMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(185, 6);
			// 
			// SaveAsTempListMenuItem
			// 
			this.SaveAsTempListMenuItem.Name = "SaveAsTempListMenuItem";
			this.SaveAsTempListMenuItem.Size = new System.Drawing.Size(188, 22);
			this.SaveAsTempListMenuItem.Text = "Current List";
			this.SaveAsTempListMenuItem.Click += new System.EventHandler(this.SaveAsTempListMenuItem_Click);
			// 
			// SaveAsNewTempListMenuItem
			// 
			this.SaveAsNewTempListMenuItem.Name = "SaveAsNewTempListMenuItem";
			this.SaveAsNewTempListMenuItem.Size = new System.Drawing.Size(188, 22);
			this.SaveAsNewTempListMenuItem.Text = "New Temporary List...";
			this.SaveAsNewTempListMenuItem.Click += new System.EventHandler(this.SaveAsNewTempListMenuItem_Click);
			// 
			// CloseMenuItem
			// 
			this.CloseMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.FileClose;
			this.CloseMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.CloseMenuItem.Name = "CloseMenuItem";
			this.CloseMenuItem.Size = new System.Drawing.Size(118, 26);
			this.CloseMenuItem.Text = "&Close";
			this.CloseMenuItem.Click += new System.EventHandler(this.CloseMenuItem_Click);
			// 
			// SaveAndClose
			// 
			this.SaveAndClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveAndClose.Appearance.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SaveAndClose.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SaveAndClose.Appearance.Options.UseFont = true;
			this.SaveAndClose.Appearance.Options.UseForeColor = true;
			this.SaveAndClose.Cursor = System.Windows.Forms.Cursors.Default;
			this.SaveAndClose.ImageOptions.ImageList = this.Bitmaps16x16;
			this.SaveAndClose.Location = new System.Drawing.Point(454, 508);
			this.SaveAndClose.Name = "SaveAndClose";
			this.SaveAndClose.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.SaveAndClose.Size = new System.Drawing.Size(92, 23);
			this.SaveAndClose.TabIndex = 208;
			this.SaveAndClose.Tag = "Cancel";
			this.SaveAndClose.Text = "Save && Close";
			this.SaveAndClose.Click += new System.EventHandler(this.SaveAndClose_Click);
			// 
			// SaveMenuItem2
			// 
			this.SaveMenuItem2.Image = global::Mobius.ClientComponents.Properties.Resources.Save;
			this.SaveMenuItem2.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.SaveMenuItem2.Name = "SaveMenuItem2";
			this.SaveMenuItem2.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.SaveMenuItem2.Size = new System.Drawing.Size(152, 22);
			this.SaveMenuItem2.Text = "&Save";
			this.SaveMenuItem2.Click += new System.EventHandler(this.SaveMenuItem_Click);
			// 
			// FileMenuItemPositionPanel
			// 
			this.FileMenuItemPositionPanel.Location = new System.Drawing.Point(2, 20);
			this.FileMenuItemPositionPanel.Name = "FileMenuItemPositionPanel";
			this.FileMenuItemPositionPanel.Size = new System.Drawing.Size(16, 16);
			this.FileMenuItemPositionPanel.TabIndex = 213;
			this.FileMenuItemPositionPanel.Visible = false;
			// 
			// CidListEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(618, 536);
			this.Controls.Add(this.FileMenuItemPositionPanel);
			this.Controls.Add(this.SaveAndClose);
			this.Controls.Add(this.labelControl2);
			this.Controls.Add(this.StatusMessage);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.RemoveDuplicates);
			this.Controls.Add(this.ValidateNumbers);
			this.Controls.Add(this.ListLabel);
			this.Controls.Add(this.DisplayFrame);
			this.Controls.Add(this.barDockControlLeft);
			this.Controls.Add(this.barDockControlRight);
			this.Controls.Add(this.barDockControlBottom);
			this.Controls.Add(this.barDockControlTop);
			this.MinimizeBox = false;
			this.Name = "CidListEditor";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "List Editor";
			this.Activated += new System.EventHandler(this.CidListEditor_Activated);
			this.Deactivate += new System.EventHandler(this.CidListEditor_Deactivate);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CidListEditor_FormClosing);
			this.ListLabel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CidListCtl.Properties)).EndInit();
			this.DisplayFrame.ResumeLayout(false);
			this.DisplayFrame.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Weight.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CidCtl.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Formula.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HeavyAtoms.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BarManager)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ValidateNumbers.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RemoveDuplicates.Properties)).EndInit();
			this.FileContextMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.FileMenuItemPositionPanel)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public System.Windows.Forms.GroupBox ListLabel;
		public DevExpress.XtraEditors.SimpleButton CloseButton;
		public System.Windows.Forms.ImageList Bitmaps16x16;
		public DevExpress.XtraEditors.SimpleButton CopyStructure;
		public System.Windows.Forms.GroupBox DisplayFrame;
		public DevExpress.XtraEditors.TextEdit CidCtl;
		public DevExpress.XtraEditors.TextEdit Formula;
		public DevExpress.XtraEditors.TextEdit HeavyAtoms;
		public DevExpress.XtraEditors.LabelControl RNLabel;
		public DevExpress.XtraEditors.LabelControl _Label2_8;
		public DevExpress.XtraEditors.LabelControl _Label2_9;
		public System.Windows.Forms.Timer Timer;
		public DevExpress.XtraEditors.MemoEdit CidListCtl;
		private DevExpress.XtraBars.BarManager BarManager;
		private DevExpress.XtraBars.BarDockControl barDockControlTop;
		private DevExpress.XtraBars.BarDockControl barDockControlBottom;
		private DevExpress.XtraBars.BarDockControl barDockControlLeft;
		private DevExpress.XtraBars.BarDockControl barDockControlRight;
		public DevExpress.XtraEditors.CheckEdit RemoveDuplicates;
		public DevExpress.XtraEditors.CheckEdit ValidateNumbers;
		public DevExpress.XtraEditors.TextEdit Weight;
		public DevExpress.XtraEditors.LabelControl labelControl1;
		private DevExpress.XtraBars.Bar MainMenuBar;
		private DevExpress.XtraBars.BarSubItem FileMenu;
		private DevExpress.XtraBars.BarSubItem EditMenu;
		private DevExpress.XtraBars.BarSubItem SortMenu;
		private DevExpress.XtraBars.BarSubItem SaltsMenu;
		public DevExpress.XtraEditors.LabelControl StatusMessage;
		public DevExpress.XtraEditors.LabelControl labelControl2;
		private DevExpress.XtraBars.BarButtonItem CutMenuItem;
		private DevExpress.XtraBars.BarButtonItem CopyMenuItem;
		private DevExpress.XtraBars.BarButtonItem PasteMenuItem;
		private DevExpress.XtraBars.BarButtonItem DeleteMenuItem;
		private DevExpress.XtraBars.BarButtonItem ClearListMenuItem;
		private DevExpress.XtraBars.BarButtonItem SortAscendingMenuItem;
		private DevExpress.XtraBars.BarButtonItem SortDescendingMenuItem;
		private DevExpress.XtraBars.BarButtonItem InsertSaltsForCurrentCompound;
		private DevExpress.XtraBars.BarButtonItem InsertSaltsForAllCompounds;
		private DevExpress.XtraBars.BarButtonItem GroupSalts;
		public System.Windows.Forms.ContextMenuStrip FileContextMenu;
		private System.Windows.Forms.ToolStripMenuItem SaveAsMenuItem;
		private System.Windows.Forms.ToolStripMenuItem SaveAsTempListMenuItem;
		private System.Windows.Forms.ToolStripMenuItem SaveAsNewTempListMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem SaveAsSavedListMenuItem;
		public DevExpress.XtraEditors.SimpleButton SaveAndClose;
		private System.Windows.Forms.ToolStripMenuItem CloseMenuItem;
		private System.Windows.Forms.ToolStripMenuItem SaveMenuItem;
		public System.Windows.Forms.ToolStripMenuItem SaveMenuItem2;
		private DevExpress.XtraEditors.PanelControl FileMenuItemPositionPanel;
		private DevExpress.XtraBars.BarButtonItem CopyAsCsvMenuItem;
		private MoleculeControl MolCtl;
	}
}