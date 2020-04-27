namespace Mobius.ClientComponents
{
	partial class RelatedCompoundsDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RelatedCompoundsDialog));
			Mobius.Data.MoleculeMx moleculeMx1 = new Mobius.Data.MoleculeMx();
			this.CloseBut = new DevExpress.XtraEditors.SimpleButton();
			this.RunQueryButton = new DevExpress.XtraEditors.SimpleButton();
			this.CurrentCidCheckEdit = new DevExpress.XtraEditors.CheckEdit();
			this.ApplyExistingCriteria = new DevExpress.XtraEditors.CheckEdit();
			this.MarkedCidsCheckEdit = new DevExpress.XtraEditors.CheckEdit();
			this.AllCidsCheckEdit = new DevExpress.XtraEditors.CheckEdit();
			this.RelatedStrsCheckEdit = new DevExpress.XtraEditors.CheckEdit();
			this.AltForms = new DevExpress.XtraEditors.CheckEdit();
			this.MatchedPairs = new DevExpress.XtraEditors.CheckEdit();
			this.SimilarSearch = new DevExpress.XtraEditors.CheckEdit();
			this.SmallWorld = new DevExpress.XtraEditors.CheckEdit();
			this.CidCtl = new DevExpress.XtraEditors.TextEdit();
			this.SelectFromDatabaseContentsButton = new DevExpress.XtraEditors.SimpleButton();
			this.MruListCtl = new DevExpress.XtraEditors.ImageListBoxControl();
			this.FavoritesButton = new DevExpress.XtraEditors.SimpleButton();
			this.ExcludeCurrentResultsCids = new DevExpress.XtraEditors.CheckEdit();
			this.StatusMessage = new DevExpress.XtraEditors.LabelControl();
			this.FavoritesMenu = new System.Windows.Forms.ContextMenuStrip();
			this.ShowTargetDescriptionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ShowTargetAssayListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.AllDataForTargetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.IncludeStructures = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
			this.CompoundIdsToRetrieveGroupControl = new DevExpress.XtraEditors.GroupControl();
			this.QueryMolCtl = new Mobius.ClientComponents.MoleculeControl();
			this.AddToFavoritesButton = new DevExpress.XtraEditors.SimpleButton();
			this.RetrieveFavoritesButton = new DevExpress.XtraEditors.SimpleButton();
			this.RetrieveRecentButton = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.AlignMatches = new DevExpress.XtraEditors.CheckEdit();
			this.RetrieveModel = new DevExpress.XtraEditors.SimpleButton();
			this.EditStructure = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.Substructure = new DevExpress.XtraEditors.CheckEdit();
			this.SelectedDataGroupControl = new DevExpress.XtraEditors.GroupControl();
			this.SplitContainer = new DevExpress.XtraEditors.SplitContainerControl();
			this.QueryBuilderPanel = new DevExpress.XtraEditors.PanelControl();
			this.Timer1 = new System.Windows.Forms.Timer();
			this.ToolTipManager = new System.Windows.Forms.ToolTip();
			((System.ComponentModel.ISupportInitialize)(this.CurrentCidCheckEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ApplyExistingCriteria.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MarkedCidsCheckEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AllCidsCheckEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RelatedStrsCheckEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AltForms.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MatchedPairs.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SimilarSearch.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SmallWorld.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CidCtl.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MruListCtl)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExcludeCurrentResultsCids.Properties)).BeginInit();
			this.FavoritesMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.IncludeStructures.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CompoundIdsToRetrieveGroupControl)).BeginInit();
			this.CompoundIdsToRetrieveGroupControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AlignMatches.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Substructure.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SelectedDataGroupControl)).BeginInit();
			this.SelectedDataGroupControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.QueryBuilderPanel)).BeginInit();
			this.QueryBuilderPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// CloseBut
			// 
			this.CloseBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseBut.Location = new System.Drawing.Point(748, 490);
			this.CloseBut.Name = "CloseBut";
			this.CloseBut.Size = new System.Drawing.Size(76, 25);
			this.CloseBut.TabIndex = 180;
			this.CloseBut.Text = "Close";
			this.CloseBut.Click += new System.EventHandler(this.CloseBut_Click);
			// 
			// RunQueryButton
			// 
			this.RunQueryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RunQueryButton.Location = new System.Drawing.Point(657, 490);
			this.RunQueryButton.Name = "RunQueryButton";
			this.RunQueryButton.Size = new System.Drawing.Size(85, 25);
			this.RunQueryButton.TabIndex = 179;
			this.RunQueryButton.Text = "OK";
			this.RunQueryButton.Click += new System.EventHandler(this.RunQueryButton_Click);
			// 
			// CurrentCidCheckEdit
			// 
			this.CurrentCidCheckEdit.Cursor = System.Windows.Forms.Cursors.Default;
			this.CurrentCidCheckEdit.EditValue = true;
			this.CurrentCidCheckEdit.Location = new System.Drawing.Point(13, 31);
			this.CurrentCidCheckEdit.Name = "CurrentCidCheckEdit";
			this.CurrentCidCheckEdit.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.CurrentCidCheckEdit.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CurrentCidCheckEdit.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.CurrentCidCheckEdit.Properties.Appearance.Options.UseBackColor = true;
			this.CurrentCidCheckEdit.Properties.Appearance.Options.UseFont = true;
			this.CurrentCidCheckEdit.Properties.Appearance.Options.UseForeColor = true;
			this.CurrentCidCheckEdit.Properties.Caption = "Current compound id:";
			this.CurrentCidCheckEdit.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.CurrentCidCheckEdit.Properties.RadioGroupIndex = 1;
			this.CurrentCidCheckEdit.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CurrentCidCheckEdit.Size = new System.Drawing.Size(136, 19);
			this.CurrentCidCheckEdit.TabIndex = 183;
			this.CurrentCidCheckEdit.Tag = "FixedHeightStructs";
			// 
			// ApplyExistingCriteria
			// 
			this.ApplyExistingCriteria.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ApplyExistingCriteria.Location = new System.Drawing.Point(17, 461);
			this.ApplyExistingCriteria.Name = "ApplyExistingCriteria";
			this.ApplyExistingCriteria.Properties.Caption = "Apply any (non-compound id) criteria present in the query";
			this.ApplyExistingCriteria.Size = new System.Drawing.Size(328, 19);
			this.ApplyExistingCriteria.TabIndex = 181;
			// 
			// MarkedCidsCheckEdit
			// 
			this.MarkedCidsCheckEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MarkedCidsCheckEdit.Cursor = System.Windows.Forms.Cursors.Default;
			this.MarkedCidsCheckEdit.Location = new System.Drawing.Point(13, 82);
			this.MarkedCidsCheckEdit.Name = "MarkedCidsCheckEdit";
			this.MarkedCidsCheckEdit.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.MarkedCidsCheckEdit.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MarkedCidsCheckEdit.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.MarkedCidsCheckEdit.Properties.Appearance.Options.UseBackColor = true;
			this.MarkedCidsCheckEdit.Properties.Appearance.Options.UseFont = true;
			this.MarkedCidsCheckEdit.Properties.Appearance.Options.UseForeColor = true;
			this.MarkedCidsCheckEdit.Properties.Caption = "Selected compound Ids (nnn)";
			this.MarkedCidsCheckEdit.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.MarkedCidsCheckEdit.Properties.RadioGroupIndex = 1;
			this.MarkedCidsCheckEdit.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.MarkedCidsCheckEdit.Size = new System.Drawing.Size(181, 19);
			this.MarkedCidsCheckEdit.TabIndex = 184;
			this.MarkedCidsCheckEdit.TabStop = false;
			this.MarkedCidsCheckEdit.Tag = "FixedHeightStructs";
			// 
			// AllCidsCheckEdit
			// 
			this.AllCidsCheckEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AllCidsCheckEdit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.AllCidsCheckEdit.Cursor = System.Windows.Forms.Cursors.Default;
			this.AllCidsCheckEdit.Location = new System.Drawing.Point(13, 57);
			this.AllCidsCheckEdit.Name = "AllCidsCheckEdit";
			this.AllCidsCheckEdit.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.AllCidsCheckEdit.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AllCidsCheckEdit.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.AllCidsCheckEdit.Properties.Appearance.Options.UseBackColor = true;
			this.AllCidsCheckEdit.Properties.Appearance.Options.UseFont = true;
			this.AllCidsCheckEdit.Properties.Appearance.Options.UseForeColor = true;
			this.AllCidsCheckEdit.Properties.Caption = "All ids in the current result set (nnn)";
			this.AllCidsCheckEdit.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.AllCidsCheckEdit.Properties.RadioGroupIndex = 1;
			this.AllCidsCheckEdit.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.AllCidsCheckEdit.Size = new System.Drawing.Size(344, 19);
			this.AllCidsCheckEdit.TabIndex = 185;
			this.AllCidsCheckEdit.TabStop = false;
			this.AllCidsCheckEdit.Tag = "FixedHeightStructs";
			// 
			// RelatedStrsCheckEdit
			// 
			this.RelatedStrsCheckEdit.Cursor = System.Windows.Forms.Cursors.Default;
			this.RelatedStrsCheckEdit.Location = new System.Drawing.Point(13, 105);
			this.RelatedStrsCheckEdit.Name = "RelatedStrsCheckEdit";
			this.RelatedStrsCheckEdit.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.RelatedStrsCheckEdit.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RelatedStrsCheckEdit.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.RelatedStrsCheckEdit.Properties.Appearance.Options.UseBackColor = true;
			this.RelatedStrsCheckEdit.Properties.Appearance.Options.UseFont = true;
			this.RelatedStrsCheckEdit.Properties.Appearance.Options.UseForeColor = true;
			this.RelatedStrsCheckEdit.Properties.Caption = "Compounds structurally related to the current compound:";
			this.RelatedStrsCheckEdit.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.RelatedStrsCheckEdit.Properties.RadioGroupIndex = 1;
			this.RelatedStrsCheckEdit.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RelatedStrsCheckEdit.Size = new System.Drawing.Size(318, 19);
			this.RelatedStrsCheckEdit.TabIndex = 186;
			this.RelatedStrsCheckEdit.TabStop = false;
			this.RelatedStrsCheckEdit.Tag = "FixedHeightStructs";
			// 
			// AltForms
			// 
			this.AltForms.EditValue = true;
			this.AltForms.Location = new System.Drawing.Point(32, 129);
			this.AltForms.Name = "AltForms";
			this.AltForms.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.AltForms.Properties.Appearance.Options.UseBackColor = true;
			this.AltForms.Properties.Caption = "Salts, Isomers...";
			this.AltForms.Size = new System.Drawing.Size(101, 19);
			this.AltForms.TabIndex = 278;
			// 
			// MatchedPairs
			// 
			this.MatchedPairs.EditValue = true;
			this.MatchedPairs.Location = new System.Drawing.Point(139, 129);
			this.MatchedPairs.Name = "MatchedPairs";
			this.MatchedPairs.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.MatchedPairs.Properties.Appearance.Options.UseBackColor = true;
			this.MatchedPairs.Properties.Caption = "Matched Pairs";
			this.MatchedPairs.Size = new System.Drawing.Size(86, 19);
			this.MatchedPairs.TabIndex = 280;
			// 
			// SimilarSearch
			// 
			this.SimilarSearch.EditValue = true;
			this.SimilarSearch.Location = new System.Drawing.Point(313, 129);
			this.SimilarSearch.Name = "SimilarSearch";
			this.SimilarSearch.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.SimilarSearch.Properties.Appearance.Options.UseBackColor = true;
			this.SimilarSearch.Properties.Caption = "Similar";
			this.SimilarSearch.Size = new System.Drawing.Size(55, 19);
			this.SimilarSearch.TabIndex = 279;
			// 
			// SmallWorld
			// 
			this.SmallWorld.EditValue = true;
			this.SmallWorld.Location = new System.Drawing.Point(233, 129);
			this.SmallWorld.Name = "SmallWorld";
			this.SmallWorld.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.SmallWorld.Properties.Appearance.Options.UseBackColor = true;
			this.SmallWorld.Properties.Caption = "SmallWorld";
			this.SmallWorld.Size = new System.Drawing.Size(80, 19);
			this.SmallWorld.TabIndex = 281;
			// 
			// CidCtl
			// 
			this.CidCtl.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.CidCtl.EditValue = "";
			this.CidCtl.Location = new System.Drawing.Point(145, 31);
			this.CidCtl.Name = "CidCtl";
			this.CidCtl.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.CidCtl.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CidCtl.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.CidCtl.Properties.Appearance.Options.UseBackColor = true;
			this.CidCtl.Properties.Appearance.Options.UseFont = true;
			this.CidCtl.Properties.Appearance.Options.UseForeColor = true;
			this.CidCtl.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CidCtl.Size = new System.Drawing.Size(163, 20);
			this.CidCtl.TabIndex = 189;
			this.CidCtl.Click += new System.EventHandler(this.CidCtl_Click);
			this.CidCtl.Enter += new System.EventHandler(this.CidCtl_Enter);
			this.CidCtl.Leave += new System.EventHandler(this.CidCtl_Leave);
			// 
			// SelectFromDatabaseContentsButton
			// 
			this.SelectFromDatabaseContentsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SelectFromDatabaseContentsButton.Location = new System.Drawing.Point(170, 403);
			this.SelectFromDatabaseContentsButton.Name = "SelectFromDatabaseContentsButton";
			this.SelectFromDatabaseContentsButton.Size = new System.Drawing.Size(132, 22);
			this.SelectFromDatabaseContentsButton.TabIndex = 182;
			this.SelectFromDatabaseContentsButton.Text = "Database Contents Tree";
			this.SelectFromDatabaseContentsButton.Click += new System.EventHandler(this.SelectFromDatabaseContentsButton_Click);
			// 
			// MruListCtl
			// 
			this.MruListCtl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MruListCtl.Location = new System.Drawing.Point(17, 29);
			this.MruListCtl.Name = "MruListCtl";
			this.MruListCtl.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.MruListCtl.Size = new System.Drawing.Size(385, 368);
			this.MruListCtl.TabIndex = 282;
			this.MruListCtl.DoubleClick += new System.EventHandler(this.MruListCtl_DoubleClick);
			// 
			// FavoritesButton
			// 
			this.FavoritesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.FavoritesButton.Location = new System.Drawing.Point(87, 403);
			this.FavoritesButton.Name = "FavoritesButton";
			this.FavoritesButton.Size = new System.Drawing.Size(76, 22);
			this.FavoritesButton.TabIndex = 283;
			this.FavoritesButton.Text = "Favorites";
			this.FavoritesButton.Click += new System.EventHandler(this.FavoritesButton_Click);
			// 
			// ExcludeCurrentResultsCids
			// 
			this.ExcludeCurrentResultsCids.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ExcludeCurrentResultsCids.EditValue = true;
			this.ExcludeCurrentResultsCids.Location = new System.Drawing.Point(15, 459);
			this.ExcludeCurrentResultsCids.Name = "ExcludeCurrentResultsCids";
			this.ExcludeCurrentResultsCids.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ExcludeCurrentResultsCids.Properties.Appearance.Options.UseBackColor = true;
			this.ExcludeCurrentResultsCids.Properties.Caption = "Exclude compound ids in the current results list";
			this.ExcludeCurrentResultsCids.Size = new System.Drawing.Size(293, 19);
			this.ExcludeCurrentResultsCids.TabIndex = 284;
			// 
			// StatusMessage
			// 
			this.StatusMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.StatusMessage.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StatusMessage.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.StatusMessage.Appearance.Options.UseFont = true;
			this.StatusMessage.Appearance.Options.UseForeColor = true;
			this.StatusMessage.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Horizontal;
			this.StatusMessage.Cursor = System.Windows.Forms.Cursors.Default;
			this.StatusMessage.Location = new System.Drawing.Point(18, 408);
			this.StatusMessage.Name = "StatusMessage";
			this.StatusMessage.Size = new System.Drawing.Size(59, 13);
			this.StatusMessage.TabIndex = 285;
			this.StatusMessage.Text = "Select from: ";
			// 
			// FavoritesMenu
			// 
			this.FavoritesMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.FavoritesMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ShowTargetDescriptionMenuItem,
            this.ShowTargetAssayListMenuItem,
            this.AllDataForTargetMenuItem});
			this.FavoritesMenu.Name = "FavoritesMenu";
			this.FavoritesMenu.Size = new System.Drawing.Size(239, 82);
			this.FavoritesMenu.Text = "Show List of Assays";
			// 
			// ShowTargetDescriptionMenuItem
			// 
			this.ShowTargetDescriptionMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ShowTargetDescriptionMenuItem.Image")));
			this.ShowTargetDescriptionMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ShowTargetDescriptionMenuItem.Name = "ShowTargetDescriptionMenuItem";
			this.ShowTargetDescriptionMenuItem.Size = new System.Drawing.Size(238, 26);
			this.ShowTargetDescriptionMenuItem.Text = "Show Target Description";
			// 
			// ShowTargetAssayListMenuItem
			// 
			this.ShowTargetAssayListMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.List;
			this.ShowTargetAssayListMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ShowTargetAssayListMenuItem.Name = "ShowTargetAssayListMenuItem";
			this.ShowTargetAssayListMenuItem.Size = new System.Drawing.Size(238, 26);
			this.ShowTargetAssayListMenuItem.Text = "Show Target Assay List";
			// 
			// AllDataForTargetMenuItem
			// 
			this.AllDataForTargetMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("AllDataForTargetMenuItem.Image")));
			this.AllDataForTargetMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.AllDataForTargetMenuItem.Name = "AllDataForTargetMenuItem";
			this.AllDataForTargetMenuItem.Size = new System.Drawing.Size(238, 26);
			this.AllDataForTargetMenuItem.Text = "Show All Assay Data for Target";
			// 
			// IncludeStructures
			// 
			this.IncludeStructures.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.IncludeStructures.Location = new System.Drawing.Point(17, 438);
			this.IncludeStructures.Name = "IncludeStructures";
			this.IncludeStructures.Properties.Caption = "Include structures if not already included in the selected query/data table";
			this.IncludeStructures.Size = new System.Drawing.Size(392, 19);
			this.IncludeStructures.TabIndex = 286;
			// 
			// labelControl4
			// 
			this.labelControl4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl4.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl4.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.labelControl4.Appearance.Options.UseFont = true;
			this.labelControl4.Appearance.Options.UseForeColor = true;
			this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl4.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl4.LineVisible = true;
			this.labelControl4.Location = new System.Drawing.Point(0, 427);
			this.labelControl4.Name = "labelControl4";
			this.labelControl4.Size = new System.Drawing.Size(410, 10);
			this.labelControl4.TabIndex = 287;
			// 
			// CompoundIdsToRetrieveGroupControl
			// 
			this.CompoundIdsToRetrieveGroupControl.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CompoundIdsToRetrieveGroupControl.AppearanceCaption.Options.UseFont = true;
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.QueryMolCtl);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.AddToFavoritesButton);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.RetrieveFavoritesButton);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.RetrieveRecentButton);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.labelControl1);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.AlignMatches);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.RetrieveModel);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.EditStructure);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.MarkedCidsCheckEdit);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.labelControl2);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.ExcludeCurrentResultsCids);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.Substructure);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.MatchedPairs);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.RelatedStrsCheckEdit);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.AllCidsCheckEdit);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.CidCtl);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.SmallWorld);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.AltForms);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.SimilarSearch);
			this.CompoundIdsToRetrieveGroupControl.Controls.Add(this.CurrentCidCheckEdit);
			this.CompoundIdsToRetrieveGroupControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CompoundIdsToRetrieveGroupControl.Location = new System.Drawing.Point(0, 0);
			this.CompoundIdsToRetrieveGroupControl.Name = "CompoundIdsToRetrieveGroupControl";
			this.CompoundIdsToRetrieveGroupControl.Size = new System.Drawing.Size(413, 486);
			this.CompoundIdsToRetrieveGroupControl.TabIndex = 288;
			this.CompoundIdsToRetrieveGroupControl.Text = "Compound Id(s) to retrieve data for";
			// 
			// QueryMolCtl
			// 
			this.QueryMolCtl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.QueryMolCtl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.QueryMolCtl.Location = new System.Drawing.Point(32, 177);
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
			this.QueryMolCtl.Size = new System.Drawing.Size(357, 241);
			this.QueryMolCtl.TabIndex = 295;
			// 
			// AddToFavoritesButton
			// 
			this.AddToFavoritesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AddToFavoritesButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AddToFavoritesButton.Appearance.ForeColor = System.Drawing.Color.Green;
			this.AddToFavoritesButton.Appearance.Options.UseFont = true;
			this.AddToFavoritesButton.Appearance.Options.UseForeColor = true;
			this.AddToFavoritesButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.AddToFavoritesButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.AddToFavoritesButton.Location = new System.Drawing.Point(206, 424);
			this.AddToFavoritesButton.Name = "AddToFavoritesButton";
			this.AddToFavoritesButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.AddToFavoritesButton.Size = new System.Drawing.Size(16, 22);
			this.AddToFavoritesButton.TabIndex = 294;
			this.AddToFavoritesButton.Tag = "";
			this.AddToFavoritesButton.Text = "+";
			this.AddToFavoritesButton.ToolTip = "Add currrent structure to favorites";
			this.AddToFavoritesButton.Click += new System.EventHandler(this.AddToFavoritesButton_Click);
			// 
			// RetrieveFavoritesButton
			// 
			this.RetrieveFavoritesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RetrieveFavoritesButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RetrieveFavoritesButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.RetrieveFavoritesButton.Appearance.Options.UseFont = true;
			this.RetrieveFavoritesButton.Appearance.Options.UseForeColor = true;
			this.RetrieveFavoritesButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.RetrieveFavoritesButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("RetrieveFavoritesButton.ImageOptions.Image")));
			this.RetrieveFavoritesButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.RetrieveFavoritesButton.Location = new System.Drawing.Point(183, 424);
			this.RetrieveFavoritesButton.Name = "RetrieveFavoritesButton";
			this.RetrieveFavoritesButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RetrieveFavoritesButton.Size = new System.Drawing.Size(22, 22);
			this.RetrieveFavoritesButton.TabIndex = 293;
			this.RetrieveFavoritesButton.Tag = "";
			this.RetrieveFavoritesButton.ToolTip = "Select a structure from the favorites list";
			this.RetrieveFavoritesButton.Click += new System.EventHandler(this.RetrieveFavoritesButton_Click);
			// 
			// RetrieveRecentButton
			// 
			this.RetrieveRecentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RetrieveRecentButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RetrieveRecentButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.RetrieveRecentButton.Appearance.Options.UseFont = true;
			this.RetrieveRecentButton.Appearance.Options.UseForeColor = true;
			this.RetrieveRecentButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.RetrieveRecentButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("RetrieveRecentButton.ImageOptions.Image")));
			this.RetrieveRecentButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.RetrieveRecentButton.Location = new System.Drawing.Point(156, 424);
			this.RetrieveRecentButton.Name = "RetrieveRecentButton";
			this.RetrieveRecentButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RetrieveRecentButton.Size = new System.Drawing.Size(22, 22);
			this.RetrieveRecentButton.TabIndex = 292;
			this.RetrieveRecentButton.Tag = "";
			this.RetrieveRecentButton.ToolTip = "Select a structure from the recently used molecule list";
			this.RetrieveRecentButton.Click += new System.EventHandler(this.RetrieveRecentButton_Click);
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl1.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.labelControl1.Appearance.Options.UseFont = true;
			this.labelControl1.Appearance.Options.UseForeColor = true;
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(0, 447);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(409, 10);
			this.labelControl1.TabIndex = 290;
			// 
			// AlignMatches
			// 
			this.AlignMatches.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AlignMatches.Cursor = System.Windows.Forms.Cursors.Default;
			this.AlignMatches.EditValue = true;
			this.AlignMatches.Location = new System.Drawing.Point(298, 425);
			this.AlignMatches.Name = "AlignMatches";
			this.AlignMatches.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.AlignMatches.Properties.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AlignMatches.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.AlignMatches.Properties.Appearance.Options.UseBackColor = true;
			this.AlignMatches.Properties.Appearance.Options.UseFont = true;
			this.AlignMatches.Properties.Appearance.Options.UseForeColor = true;
			this.AlignMatches.Properties.Caption = "Align matches";
			this.AlignMatches.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.AlignMatches.Size = new System.Drawing.Size(94, 19);
			this.AlignMatches.TabIndex = 291;
			this.AlignMatches.ToolTip = "Align matching structures to the query structure";
			// 
			// RetrieveModel
			// 
			this.RetrieveModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RetrieveModel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RetrieveModel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.RetrieveModel.Appearance.Options.UseFont = true;
			this.RetrieveModel.Appearance.Options.UseForeColor = true;
			this.RetrieveModel.Cursor = System.Windows.Forms.Cursors.Default;
			this.RetrieveModel.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("RetrieveModel.ImageOptions.Image")));
			this.RetrieveModel.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.RightCenter;
			this.RetrieveModel.Location = new System.Drawing.Point(33, 424);
			this.RetrieveModel.Name = "RetrieveModel";
			this.RetrieveModel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RetrieveModel.Size = new System.Drawing.Size(117, 22);
			this.RetrieveModel.TabIndex = 287;
			this.RetrieveModel.Tag = "RetrieveModel";
			this.RetrieveModel.Text = "Retrieve Structure";
			this.RetrieveModel.ToolTip = "Retrieve a model structure from a database or file...";
			this.RetrieveModel.Click += new System.EventHandler(this.RetrieveModel_Click);
			// 
			// EditStructure
			// 
			this.EditStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.EditStructure.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EditStructure.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.EditStructure.Appearance.Options.UseFont = true;
			this.EditStructure.Appearance.Options.UseForeColor = true;
			this.EditStructure.Cursor = System.Windows.Forms.Cursors.Default;
			this.EditStructure.Location = new System.Drawing.Point(230, 424);
			this.EditStructure.Name = "EditStructure";
			this.EditStructure.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.EditStructure.Size = new System.Drawing.Size(58, 22);
			this.EditStructure.TabIndex = 288;
			this.EditStructure.Tag = "EditStructure";
			this.EditStructure.Text = "Edit";
			this.EditStructure.ToolTip = "Edit Structure";
			this.EditStructure.Click += new System.EventHandler(this.EditStructure_Click);
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
			this.labelControl2.Location = new System.Drawing.Point(51, 154);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(336, 19);
			this.labelControl2.TabIndex = 286;
			this.labelControl2.Text = "Substructure";
			// 
			// Substructure
			// 
			this.Substructure.EditValue = true;
			this.Substructure.Location = new System.Drawing.Point(32, 153);
			this.Substructure.Name = "Substructure";
			this.Substructure.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Substructure.Properties.Appearance.Options.UseBackColor = true;
			this.Substructure.Properties.Caption = "";
			this.Substructure.Size = new System.Drawing.Size(86, 19);
			this.Substructure.TabIndex = 285;
			// 
			// SelectedDataGroupControl
			// 
			this.SelectedDataGroupControl.Appearance.FontStyleDelta = System.Drawing.FontStyle.Underline;
			this.SelectedDataGroupControl.Appearance.Options.UseFont = true;
			this.SelectedDataGroupControl.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SelectedDataGroupControl.AppearanceCaption.Options.UseFont = true;
			this.SelectedDataGroupControl.Controls.Add(this.MruListCtl);
			this.SelectedDataGroupControl.Controls.Add(this.StatusMessage);
			this.SelectedDataGroupControl.Controls.Add(this.labelControl4);
			this.SelectedDataGroupControl.Controls.Add(this.ApplyExistingCriteria);
			this.SelectedDataGroupControl.Controls.Add(this.IncludeStructures);
			this.SelectedDataGroupControl.Controls.Add(this.FavoritesButton);
			this.SelectedDataGroupControl.Controls.Add(this.SelectFromDatabaseContentsButton);
			this.SelectedDataGroupControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectedDataGroupControl.Location = new System.Drawing.Point(0, 0);
			this.SelectedDataGroupControl.Name = "SelectedDataGroupControl";
			this.SelectedDataGroupControl.Size = new System.Drawing.Size(410, 486);
			this.SelectedDataGroupControl.TabIndex = 285;
			this.SelectedDataGroupControl.Text = "Query or data table(s) to retrieve related data for ";
			// 
			// SplitContainer
			// 
			this.SplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SplitContainer.Location = new System.Drawing.Point(0, 0);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Panel1.Controls.Add(this.CompoundIdsToRetrieveGroupControl);
			this.SplitContainer.Panel1.Text = "Panel1";
			this.SplitContainer.Panel2.Controls.Add(this.SelectedDataGroupControl);
			this.SplitContainer.Panel2.Text = "Panel2";
			this.SplitContainer.Size = new System.Drawing.Size(828, 486);
			this.SplitContainer.SplitterPosition = 413;
			this.SplitContainer.TabIndex = 289;
			this.SplitContainer.Text = "splitContainerControl1";
			// 
			// QueryBuilderPanel
			// 
			this.QueryBuilderPanel.Controls.Add(this.CloseBut);
			this.QueryBuilderPanel.Controls.Add(this.RunQueryButton);
			this.QueryBuilderPanel.Controls.Add(this.SplitContainer);
			this.QueryBuilderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QueryBuilderPanel.Location = new System.Drawing.Point(0, 0);
			this.QueryBuilderPanel.Name = "QueryBuilderPanel";
			this.QueryBuilderPanel.Size = new System.Drawing.Size(831, 520);
			this.QueryBuilderPanel.TabIndex = 290;
			// 
			// Timer1
			// 
			this.Timer1.Tick += new System.EventHandler(this.Timer1_Tick);
			// 
			// RelatedCompoundsDialog
			// 
			this.AcceptButton = this.RunQueryButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(831, 520);
			this.Controls.Add(this.QueryBuilderPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RelatedCompoundsDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Retrieve Related Compound Data";
			this.Shown += new System.EventHandler(this.RelatedCompoundsDialog_Shown);
			((System.ComponentModel.ISupportInitialize)(this.CurrentCidCheckEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ApplyExistingCriteria.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MarkedCidsCheckEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AllCidsCheckEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RelatedStrsCheckEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AltForms.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MatchedPairs.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SimilarSearch.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SmallWorld.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CidCtl.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MruListCtl)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ExcludeCurrentResultsCids.Properties)).EndInit();
			this.FavoritesMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.IncludeStructures.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CompoundIdsToRetrieveGroupControl)).EndInit();
			this.CompoundIdsToRetrieveGroupControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AlignMatches.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Substructure.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SelectedDataGroupControl)).EndInit();
			this.SelectedDataGroupControl.ResumeLayout(false);
			this.SelectedDataGroupControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.QueryBuilderPanel)).EndInit();
			this.QueryBuilderPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.SimpleButton CloseBut;
		public DevExpress.XtraEditors.SimpleButton RunQueryButton;
		public DevExpress.XtraEditors.CheckEdit CurrentCidCheckEdit;
		public DevExpress.XtraEditors.CheckEdit ApplyExistingCriteria;
		public DevExpress.XtraEditors.CheckEdit AllCidsCheckEdit;
		public DevExpress.XtraEditors.CheckEdit MarkedCidsCheckEdit;
		public DevExpress.XtraEditors.CheckEdit RelatedStrsCheckEdit;
		private DevExpress.XtraEditors.CheckEdit AltForms;
		private DevExpress.XtraEditors.CheckEdit MatchedPairs;
		private DevExpress.XtraEditors.CheckEdit SimilarSearch;
		public DevExpress.XtraEditors.CheckEdit SmallWorld;
		public DevExpress.XtraEditors.TextEdit CidCtl;
		public DevExpress.XtraEditors.SimpleButton SelectFromDatabaseContentsButton;
		private DevExpress.XtraEditors.ImageListBoxControl MruListCtl;
		public DevExpress.XtraEditors.SimpleButton FavoritesButton;
		private DevExpress.XtraEditors.CheckEdit ExcludeCurrentResultsCids;
		public DevExpress.XtraEditors.LabelControl StatusMessage;
		public System.Windows.Forms.ContextMenuStrip FavoritesMenu;
		public System.Windows.Forms.ToolStripMenuItem ShowTargetDescriptionMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ShowTargetAssayListMenuItem;
		public System.Windows.Forms.ToolStripMenuItem AllDataForTargetMenuItem;
		public DevExpress.XtraEditors.CheckEdit IncludeStructures;
		public DevExpress.XtraEditors.LabelControl labelControl4;
		private DevExpress.XtraEditors.GroupControl CompoundIdsToRetrieveGroupControl;
		private DevExpress.XtraEditors.CheckEdit Substructure;
		private DevExpress.XtraEditors.GroupControl SelectedDataGroupControl;
		public DevExpress.XtraEditors.LabelControl labelControl2;
		public DevExpress.XtraEditors.SimpleButton RetrieveModel;
		public DevExpress.XtraEditors.SimpleButton EditStructure;
		public DevExpress.XtraEditors.LabelControl labelControl1;
		public DevExpress.XtraEditors.CheckEdit AlignMatches;
		private DevExpress.XtraEditors.SplitContainerControl SplitContainer;
		public DevExpress.XtraEditors.SimpleButton AddToFavoritesButton;
		public DevExpress.XtraEditors.SimpleButton RetrieveFavoritesButton;
		public DevExpress.XtraEditors.SimpleButton RetrieveRecentButton;
		private DevExpress.XtraEditors.PanelControl QueryBuilderPanel;
		public System.Windows.Forms.Timer Timer1;
		private System.Windows.Forms.ToolTip ToolTipManager;
		public MoleculeControl QueryMolCtl;
	}
}