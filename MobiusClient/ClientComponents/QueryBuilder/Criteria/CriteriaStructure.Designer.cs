namespace Mobius.ClientComponents
{
	partial class CriteriaStructure
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CriteriaStructure));
			Mobius.Data.MoleculeMx moleculeMx1 = new Mobius.Data.MoleculeMx();
			this.Full = new DevExpress.XtraEditors.CheckEdit();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.RetrieveModel = new DevExpress.XtraEditors.SimpleButton();
			this.Label1 = new DevExpress.XtraEditors.LabelControl();
			this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.None = new DevExpress.XtraEditors.CheckEdit();
			this.Similarity = new DevExpress.XtraEditors.CheckEdit();
			this.SubStruct = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.BottomDivider = new DevExpress.XtraEditors.LabelControl();
			this.SmallWorld = new DevExpress.XtraEditors.CheckEdit();
			this.ShowOptionsButton = new DevExpress.XtraEditors.SimpleButton();
			this.Bitmaps8x8 = new System.Windows.Forms.ImageList(this.components);
			this.PreviewPanel = new DevExpress.XtraEditors.PanelControl();
			this.PreviewCtl = new Mobius.ClientComponents.CriteriaStructurePreview();
			this.RetrieveRecentButton = new DevExpress.XtraEditors.SimpleButton();
			this.RetrieveFavoritesButton = new DevExpress.XtraEditors.SimpleButton();
			this.AddToFavoritesButton = new DevExpress.XtraEditors.SimpleButton();
			this.EditMoleculeDropdownMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.NewChemicalStructureMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.NewBiopolymerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.QueryMolCtl = new Mobius.ClientComponents.MoleculeControl();
			this.SSOptions = new Mobius.ClientComponents.CriteriaStructureOptionsSS();
			this.SmallWorldOptions = new Mobius.ClientComponents.CriteriaStructureOptionsSW();
			this.SimOptions = new Mobius.ClientComponents.CriteriaStructureOptionsSim();
			this.FSOptions = new Mobius.ClientComponents.CriteriaStructureOptionsFull();
			this.EditStructureButton = new DevExpress.XtraEditors.DropDownButton();
			((System.ComponentModel.ISupportInitialize)(this.Full.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.None.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Similarity.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SubStruct.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SmallWorld.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PreviewPanel)).BeginInit();
			this.PreviewPanel.SuspendLayout();
			this.EditMoleculeDropdownMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// Full
			// 
			this.Full.Cursor = System.Windows.Forms.Cursors.Default;
			this.Full.Location = new System.Drawing.Point(253, 26);
			this.Full.Name = "Full";
			this.Full.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Full.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Full.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Full.Properties.Appearance.Options.UseBackColor = true;
			this.Full.Properties.Appearance.Options.UseFont = true;
			this.Full.Properties.Appearance.Options.UseForeColor = true;
			this.Full.Properties.AppearanceDisabled.ForeColor = System.Drawing.SystemColors.ControlDark;
			this.Full.Properties.AppearanceDisabled.Options.UseForeColor = true;
			this.Full.Properties.Caption = "&Full Structure";
			this.Full.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.Full.Properties.RadioGroupIndex = 1;
			this.Full.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Full.Size = new System.Drawing.Size(92, 19);
			this.Full.TabIndex = 6;
			this.Full.TabStop = false;
			this.Full.CheckedChanged += new System.EventHandler(this.Full_CheckedChanged);
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.Location = new System.Drawing.Point(1159, 760);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(58, 22);
			this.OK.TabIndex = 20;
			this.OK.Tag = "OK";
			this.OK.Text = "OK";
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
			this.Cancel.Location = new System.Drawing.Point(1224, 760);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(59, 22);
			this.Cancel.TabIndex = 21;
			this.Cancel.Tag = "Cancel";
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// RetrieveModel
			// 
			this.RetrieveModel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RetrieveModel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.RetrieveModel.Appearance.Options.UseFont = true;
			this.RetrieveModel.Appearance.Options.UseForeColor = true;
			this.RetrieveModel.Cursor = System.Windows.Forms.Cursors.Default;
			this.RetrieveModel.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("RetrieveModel.ImageOptions.Image")));
			this.RetrieveModel.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.RightCenter;
			this.RetrieveModel.Location = new System.Drawing.Point(8, 361);
			this.RetrieveModel.Name = "RetrieveModel";
			this.RetrieveModel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RetrieveModel.Size = new System.Drawing.Size(73, 22);
			this.RetrieveModel.TabIndex = 18;
			this.RetrieveModel.Tag = "RetrieveModel";
			this.RetrieveModel.Text = "Retrieve";
			this.RetrieveModel.Click += new System.EventHandler(this.RetrieveModel_Click);
			// 
			// Label1
			// 
			this.Label1.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Label1.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label1.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Label1.Appearance.Options.UseBackColor = true;
			this.Label1.Appearance.Options.UseFont = true;
			this.Label1.Appearance.Options.UseForeColor = true;
			this.Label1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.Label1.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label1.LineVisible = true;
			this.Label1.Location = new System.Drawing.Point(5, 6);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(383, 16);
			this.Label1.TabIndex = 22;
			this.Label1.Text = "Structure search type";
			// 
			// None
			// 
			this.None.Cursor = System.Windows.Forms.Cursors.Default;
			this.None.Location = new System.Drawing.Point(147, 57);
			this.None.Name = "None";
			this.None.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.None.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.None.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.None.Properties.Appearance.Options.UseBackColor = true;
			this.None.Properties.Appearance.Options.UseFont = true;
			this.None.Properties.Appearance.Options.UseForeColor = true;
			this.None.Properties.AppearanceDisabled.ForeColor = System.Drawing.SystemColors.ControlDark;
			this.None.Properties.AppearanceDisabled.Options.UseForeColor = true;
			this.None.Properties.Caption = "None";
			this.None.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.None.Properties.RadioGroupIndex = 1;
			this.None.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.None.Size = new System.Drawing.Size(60, 19);
			this.None.TabIndex = 9;
			this.None.TabStop = false;
			this.None.CheckedChanged += new System.EventHandler(this.None_CheckedChanged);
			// 
			// Similarity
			// 
			this.Similarity.Cursor = System.Windows.Forms.Cursors.Default;
			this.Similarity.Location = new System.Drawing.Point(147, 26);
			this.Similarity.Name = "Similarity";
			this.Similarity.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Similarity.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Similarity.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Similarity.Properties.Appearance.Options.UseBackColor = true;
			this.Similarity.Properties.Appearance.Options.UseFont = true;
			this.Similarity.Properties.Appearance.Options.UseForeColor = true;
			this.Similarity.Properties.AppearanceDisabled.ForeColor = System.Drawing.SystemColors.ControlDark;
			this.Similarity.Properties.AppearanceDisabled.Options.UseForeColor = true;
			this.Similarity.Properties.Caption = "Si&milarity";
			this.Similarity.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.Similarity.Properties.RadioGroupIndex = 1;
			this.Similarity.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Similarity.Size = new System.Drawing.Size(82, 19);
			this.Similarity.TabIndex = 8;
			this.Similarity.TabStop = false;
			this.Similarity.CheckedChanged += new System.EventHandler(this.Similarity_CheckedChanged);
			// 
			// SubStruct
			// 
			this.SubStruct.Cursor = System.Windows.Forms.Cursors.Default;
			this.SubStruct.EditValue = true;
			this.SubStruct.Location = new System.Drawing.Point(12, 28);
			this.SubStruct.Name = "SubStruct";
			this.SubStruct.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.SubStruct.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SubStruct.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SubStruct.Properties.Appearance.Options.UseBackColor = true;
			this.SubStruct.Properties.Appearance.Options.UseFont = true;
			this.SubStruct.Properties.Appearance.Options.UseForeColor = true;
			this.SubStruct.Properties.AppearanceDisabled.ForeColor = System.Drawing.SystemColors.ControlDark;
			this.SubStruct.Properties.AppearanceDisabled.Options.UseForeColor = true;
			this.SubStruct.Properties.Caption = "&Substructure";
			this.SubStruct.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.SubStruct.Properties.RadioGroupIndex = 1;
			this.SubStruct.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.SubStruct.Size = new System.Drawing.Size(93, 19);
			this.SubStruct.TabIndex = 7;
			this.SubStruct.CheckedChanged += new System.EventHandler(this.SubStruct_CheckedChanged);
			// 
			// labelControl2
			// 
			this.labelControl2.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl2.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl2.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.labelControl2.Appearance.Options.UseBackColor = true;
			this.labelControl2.Appearance.Options.UseFont = true;
			this.labelControl2.Appearance.Options.UseForeColor = true;
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl2.LineVisible = true;
			this.labelControl2.Location = new System.Drawing.Point(5, 82);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(385, 17);
			this.labelControl2.TabIndex = 27;
			this.labelControl2.Text = "Query structure";
			// 
			// BottomDivider
			// 
			this.BottomDivider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BottomDivider.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.BottomDivider.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.BottomDivider.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.BottomDivider.Appearance.Options.UseBackColor = true;
			this.BottomDivider.Appearance.Options.UseFont = true;
			this.BottomDivider.Appearance.Options.UseForeColor = true;
			this.BottomDivider.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.BottomDivider.Cursor = System.Windows.Forms.Cursors.Default;
			this.BottomDivider.LineVisible = true;
			this.BottomDivider.Location = new System.Drawing.Point(-3, 749);
			this.BottomDivider.Name = "BottomDivider";
			this.BottomDivider.Size = new System.Drawing.Size(1291, 6);
			this.BottomDivider.TabIndex = 26;
			// 
			// SmallWorld
			// 
			this.SmallWorld.Cursor = System.Windows.Forms.Cursors.Default;
			this.SmallWorld.Location = new System.Drawing.Point(12, 57);
			this.SmallWorld.Name = "SmallWorld";
			this.SmallWorld.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.SmallWorld.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SmallWorld.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SmallWorld.Properties.Appearance.Options.UseBackColor = true;
			this.SmallWorld.Properties.Appearance.Options.UseFont = true;
			this.SmallWorld.Properties.Appearance.Options.UseForeColor = true;
			this.SmallWorld.Properties.AppearanceDisabled.ForeColor = System.Drawing.SystemColors.ControlDark;
			this.SmallWorld.Properties.AppearanceDisabled.Options.UseForeColor = true;
			this.SmallWorld.Properties.Caption = "Small World";
			this.SmallWorld.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.SmallWorld.Properties.RadioGroupIndex = 1;
			this.SmallWorld.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.SmallWorld.Size = new System.Drawing.Size(82, 19);
			this.SmallWorld.TabIndex = 28;
			this.SmallWorld.TabStop = false;
			this.SmallWorld.CheckedChanged += new System.EventHandler(this.SmallWorld_CheckedChanged);
			// 
			// ShowOptionsButton
			// 
			this.ShowOptionsButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ShowOptionsButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ShowOptionsButton.Appearance.Options.UseFont = true;
			this.ShowOptionsButton.Appearance.Options.UseForeColor = true;
			this.ShowOptionsButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.ShowOptionsButton.ImageOptions.ImageIndex = 0;
			this.ShowOptionsButton.ImageOptions.ImageList = this.Bitmaps8x8;
			this.ShowOptionsButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleRight;
			this.ShowOptionsButton.Location = new System.Drawing.Point(253, 57);
			this.ShowOptionsButton.Name = "ShowOptionsButton";
			this.ShowOptionsButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ShowOptionsButton.Size = new System.Drawing.Size(105, 22);
			this.ShowOptionsButton.TabIndex = 29;
			this.ShowOptionsButton.Tag = "OK";
			this.ShowOptionsButton.Text = "Search Options";
			this.ShowOptionsButton.Click += new System.EventHandler(this.ShowOptionsButton_Click);
			// 
			// Bitmaps8x8
			// 
			this.Bitmaps8x8.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps8x8.ImageStream")));
			this.Bitmaps8x8.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps8x8.Images.SetKeyName(0, "DropDownChevron8x8.bmp");
			this.Bitmaps8x8.Images.SetKeyName(1, "DropUpChevron8x8.bmp");
			// 
			// PreviewPanel
			// 
			this.PreviewPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
			this.PreviewPanel.Controls.Add(this.PreviewCtl);
			this.PreviewPanel.Location = new System.Drawing.Point(397, 0);
			this.PreviewPanel.Name = "PreviewPanel";
			this.PreviewPanel.Size = new System.Drawing.Size(494, 653);
			this.PreviewPanel.TabIndex = 35;
			// 
			// PreviewCtl
			// 
			this.PreviewCtl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.PreviewCtl.Location = new System.Drawing.Point(0, 0);
			this.PreviewCtl.Margin = new System.Windows.Forms.Padding(4);
			this.PreviewCtl.Name = "PreviewCtl";
			this.PreviewCtl.Size = new System.Drawing.Size(477, 613);
			this.PreviewCtl.TabIndex = 34;
			// 
			// RetrieveRecentButton
			// 
			this.RetrieveRecentButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RetrieveRecentButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.RetrieveRecentButton.Appearance.Options.UseFont = true;
			this.RetrieveRecentButton.Appearance.Options.UseForeColor = true;
			this.RetrieveRecentButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.RetrieveRecentButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("RetrieveRecentButton.ImageOptions.Image")));
			this.RetrieveRecentButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.RetrieveRecentButton.Location = new System.Drawing.Point(88, 361);
			this.RetrieveRecentButton.Name = "RetrieveRecentButton";
			this.RetrieveRecentButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RetrieveRecentButton.Size = new System.Drawing.Size(22, 22);
			this.RetrieveRecentButton.TabIndex = 36;
			this.RetrieveRecentButton.Tag = "";
			this.RetrieveRecentButton.ToolTip = "Select a structure from the recently used molecule list";
			this.RetrieveRecentButton.Click += new System.EventHandler(this.RetrieveRecentButton_Click);
			// 
			// RetrieveFavoritesButton
			// 
			this.RetrieveFavoritesButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RetrieveFavoritesButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.RetrieveFavoritesButton.Appearance.Options.UseFont = true;
			this.RetrieveFavoritesButton.Appearance.Options.UseForeColor = true;
			this.RetrieveFavoritesButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.RetrieveFavoritesButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("RetrieveFavoritesButton.ImageOptions.Image")));
			this.RetrieveFavoritesButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.RetrieveFavoritesButton.Location = new System.Drawing.Point(116, 361);
			this.RetrieveFavoritesButton.Name = "RetrieveFavoritesButton";
			this.RetrieveFavoritesButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RetrieveFavoritesButton.Size = new System.Drawing.Size(22, 22);
			this.RetrieveFavoritesButton.TabIndex = 37;
			this.RetrieveFavoritesButton.Tag = "";
			this.RetrieveFavoritesButton.ToolTip = "Select a structure from the favorites list";
			this.RetrieveFavoritesButton.Click += new System.EventHandler(this.RetrieveFavoritesButton_Click);
			// 
			// AddToFavoritesButton
			// 
			this.AddToFavoritesButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AddToFavoritesButton.Appearance.ForeColor = System.Drawing.Color.Green;
			this.AddToFavoritesButton.Appearance.Options.UseFont = true;
			this.AddToFavoritesButton.Appearance.Options.UseForeColor = true;
			this.AddToFavoritesButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.AddToFavoritesButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.AddToFavoritesButton.Location = new System.Drawing.Point(138, 361);
			this.AddToFavoritesButton.Name = "AddToFavoritesButton";
			this.AddToFavoritesButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.AddToFavoritesButton.Size = new System.Drawing.Size(16, 22);
			this.AddToFavoritesButton.TabIndex = 38;
			this.AddToFavoritesButton.Tag = "";
			this.AddToFavoritesButton.Text = "+";
			this.AddToFavoritesButton.ToolTip = "Add currrent structure to favorites";
			this.AddToFavoritesButton.Click += new System.EventHandler(this.AddToFavoritesButton_Click);
			// 
			// EditMoleculeDropdownMenu
			// 
			this.EditMoleculeDropdownMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewChemicalStructureMenuItem,
            this.NewBiopolymerMenuItem});
			this.EditMoleculeDropdownMenu.Name = "RetrieveModelMenu";
			this.EditMoleculeDropdownMenu.Size = new System.Drawing.Size(203, 70);
			// 
			// NewChemicalStructureMenuItem
			// 
			this.NewChemicalStructureMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("NewChemicalStructureMenuItem.Image")));
			this.NewChemicalStructureMenuItem.Name = "NewChemicalStructureMenuItem";
			this.NewChemicalStructureMenuItem.Size = new System.Drawing.Size(202, 22);
			this.NewChemicalStructureMenuItem.Text = "New Chemical Structure";
			this.NewChemicalStructureMenuItem.Click += new System.EventHandler(this.NewChemicalStructureMenuItem_Click);
			// 
			// NewBiopolymerMenuItem
			// 
			this.NewBiopolymerMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("NewBiopolymerMenuItem.Image")));
			this.NewBiopolymerMenuItem.Name = "NewBiopolymerMenuItem";
			this.NewBiopolymerMenuItem.Size = new System.Drawing.Size(202, 22);
			this.NewBiopolymerMenuItem.Text = "New Biopolymer";
			this.NewBiopolymerMenuItem.Click += new System.EventHandler(this.NewBiopolymerMenuItem_Click);
			// 
			// QueryMolCtl
			// 
			this.QueryMolCtl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.QueryMolCtl.Location = new System.Drawing.Point(8, 105);
			this.QueryMolCtl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
			this.QueryMolCtl.Molecule = moleculeMx1;
			this.QueryMolCtl.Name = "QueryMolCtl";
			this.QueryMolCtl.Size = new System.Drawing.Size(382, 250);
			this.QueryMolCtl.TabIndex = 39;
			// 
			// SSOptions
			// 
			this.SSOptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.SSOptions.Location = new System.Drawing.Point(897, 454);
			this.SSOptions.Margin = new System.Windows.Forms.Padding(4);
			this.SSOptions.Name = "SSOptions";
			this.SSOptions.Size = new System.Drawing.Size(379, 79);
			this.SSOptions.TabIndex = 33;
			this.SSOptions.Visible = false;
			// 
			// SmallWorldOptions
			// 
			this.SmallWorldOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.SmallWorldOptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.SmallWorldOptions.Location = new System.Drawing.Point(6, 391);
			this.SmallWorldOptions.Margin = new System.Windows.Forms.Padding(4);
			this.SmallWorldOptions.Name = "SmallWorldOptions";
			this.SmallWorldOptions.Size = new System.Drawing.Size(386, 360);
			this.SmallWorldOptions.TabIndex = 32;
			this.SmallWorldOptions.Visible = false;
			// 
			// SimOptions
			// 
			this.SimOptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.SimOptions.Location = new System.Drawing.Point(896, 226);
			this.SimOptions.Margin = new System.Windows.Forms.Padding(4);
			this.SimOptions.Name = "SimOptions";
			this.SimOptions.Size = new System.Drawing.Size(380, 213);
			this.SimOptions.TabIndex = 31;
			this.SimOptions.Visible = false;
			// 
			// FSOptions
			// 
			this.FSOptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.FSOptions.Location = new System.Drawing.Point(896, 10);
			this.FSOptions.Margin = new System.Windows.Forms.Padding(4);
			this.FSOptions.Name = "FSOptions";
			this.FSOptions.Size = new System.Drawing.Size(380, 200);
			this.FSOptions.TabIndex = 30;
			this.FSOptions.Visible = false;
			// 
			// EditStructureButton
			// 
			this.EditStructureButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("EditStructureButton.ImageOptions.Image")));
			this.EditStructureButton.Location = new System.Drawing.Point(315, 362);
			this.EditStructureButton.Name = "EditStructureButton";
			this.EditStructureButton.Size = new System.Drawing.Size(74, 22);
			this.EditStructureButton.TabIndex = 42;
			this.EditStructureButton.Text = "Edit";
			this.EditStructureButton.ArrowButtonClick += new System.EventHandler(this.EditStructureButton_ArrowButtonClick);
			this.EditStructureButton.Click += new System.EventHandler(this.EditStructureButton_Click);
			// 
			// CriteriaStructure
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(1288, 790);
			this.Controls.Add(this.EditStructureButton);
			this.Controls.Add(this.QueryMolCtl);
			this.Controls.Add(this.AddToFavoritesButton);
			this.Controls.Add(this.RetrieveFavoritesButton);
			this.Controls.Add(this.RetrieveRecentButton);
			this.Controls.Add(this.PreviewPanel);
			this.Controls.Add(this.SSOptions);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.SmallWorldOptions);
			this.Controls.Add(this.SimOptions);
			this.Controls.Add(this.FSOptions);
			this.Controls.Add(this.ShowOptionsButton);
			this.Controls.Add(this.SmallWorld);
			this.Controls.Add(this.RetrieveModel);
			this.Controls.Add(this.labelControl2);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.None);
			this.Controls.Add(this.Similarity);
			this.Controls.Add(this.SubStruct);
			this.Controls.Add(this.Full);
			this.Controls.Add(this.BottomDivider);
			this.MinimizeBox = false;
			this.Name = "CriteriaStructure";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Structure Search Criteria";
			this.SizeChanged += new System.EventHandler(this.CriteriaStructure_SizeChanged);
			((System.ComponentModel.ISupportInitialize)(this.Full.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.None.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Similarity.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SubStruct.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SmallWorld.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PreviewPanel)).EndInit();
			this.PreviewPanel.ResumeLayout(false);
			this.EditMoleculeDropdownMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.CheckEdit Full;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.SimpleButton RetrieveModel;
		public DevExpress.XtraEditors.LabelControl Label1;
		public System.Windows.Forms.ToolTip ToolTip1;
		public DevExpress.XtraEditors.CheckEdit None;
		public DevExpress.XtraEditors.CheckEdit Similarity;
		public DevExpress.XtraEditors.CheckEdit SubStruct;
		public DevExpress.XtraEditors.LabelControl labelControl2;
		public DevExpress.XtraEditors.LabelControl BottomDivider;
		public DevExpress.XtraEditors.CheckEdit SmallWorld;
		public DevExpress.XtraEditors.SimpleButton ShowOptionsButton;
		private System.Windows.Forms.ImageList Bitmaps8x8;
		private CriteriaStructureOptionsFull FSOptions;
		private CriteriaStructureOptionsSim SimOptions;
		private CriteriaStructureOptionsSW SmallWorldOptions;
		private CriteriaStructureOptionsSS SSOptions;
		internal DevExpress.XtraEditors.PanelControl PreviewPanel;
		internal CriteriaStructurePreview PreviewCtl;
		public DevExpress.XtraEditors.SimpleButton RetrieveRecentButton;
		public DevExpress.XtraEditors.SimpleButton RetrieveFavoritesButton;
		public DevExpress.XtraEditors.SimpleButton AddToFavoritesButton;
		public MoleculeControl QueryMolCtl;
		private System.Windows.Forms.ContextMenuStrip EditMoleculeDropdownMenu;
		private System.Windows.Forms.ToolStripMenuItem NewBiopolymerMenuItem;
		private System.Windows.Forms.ToolStripMenuItem NewChemicalStructureMenuItem;
		private DevExpress.XtraEditors.DropDownButton EditStructureButton;
	}
}