namespace Mobius.ClientComponents
{
	public partial class RelatedStructuresControl
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
			Mobius.Data.MoleculeMx moleculeMx1 = new Mobius.Data.MoleculeMx();
			this.RelatedCorpDbStructureLabel
 = new DevExpress.XtraEditors.LabelControl();
			this.SimilarStructure = new DevExpress.XtraEditors.CheckEdit();
			this.MatchedPairs = new DevExpress.XtraEditors.CheckEdit();
			this.AltForms = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.MatchCountsPanel = new System.Windows.Forms.TableLayoutPanel();
			this.SmallWorldLabel = new DevExpress.XtraEditors.LabelControl();
			this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
			this.SmallWorld = new DevExpress.XtraEditors.CheckEdit();
			this.AllMatches = new DevExpress.XtraEditors.CheckEdit();
			this.SearchStatusLabel = new DevExpress.XtraEditors.LabelControl();
			this.ExcludeCidsSeenSoFar = new DevExpress.XtraEditors.CheckEdit();
			this.IncludeQueryStruct = new DevExpress.XtraEditors.CheckEdit();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
			this.DbMatchCountsPanel = new System.Windows.Forms.TableLayoutPanel();
			this.ChemblDB = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.CorpDB = new DevExpress.XtraEditors.CheckEdit();
			this.DatabasesLabel = new DevExpress.XtraEditors.LabelControl();
			this.MoleculeControl = new Mobius.ClientComponents.MoleculeControl();
			this.SplitContainer = new DevExpress.XtraEditors.SplitContainerControl();
			this.MoleculeGridPageControl = new Mobius.ClientComponents.MoleculeGridPageControl();
			((System.ComponentModel.ISupportInitialize)(this.SimilarStructure.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MatchedPairs.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AltForms.Properties)).BeginInit();
			this.MatchCountsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SmallWorld.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AllMatches.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExcludeCidsSeenSoFar.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IncludeQueryStruct.Properties)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.DbMatchCountsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChemblDB.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CorpDB.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// RelatedCorpDbStructureLabel
			// 
			this.RelatedCorpDbStructureLabel
.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RelatedCorpDbStructureLabel
.Appearance.BackColor = System.Drawing.Color.LightSteelBlue;
			this.RelatedCorpDbStructureLabel
.Appearance.Options.UseBackColor = true;
			this.RelatedCorpDbStructureLabel
.Appearance.Options.UseTextOptions = true;
			this.RelatedCorpDbStructureLabel
.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.RelatedCorpDbStructureLabel
.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.RelatedCorpDbStructureLabel
.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.RelatedCorpDbStructureLabel
.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
			this.RelatedCorpDbStructureLabel
.Location = new System.Drawing.Point(426, 0);
			this.RelatedCorpDbStructureLabel
.Margin = new System.Windows.Forms.Padding(0);
			this.RelatedCorpDbStructureLabel
.Name = "RelatedCorpDbStructureLabel";
			this.RelatedCorpDbStructureLabel
.Size = new System.Drawing.Size(144, 20);
			this.RelatedCorpDbStructureLabel
.TabIndex = 241;
			this.RelatedCorpDbStructureLabel
.Text = "Matches by Search Type";
			// 
			// SimilarStructure
			// 
			this.SimilarStructure.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SimilarStructure.EditValue = true;
			this.SimilarStructure.Location = new System.Drawing.Point(100, 67);
			this.SimilarStructure.Name = "SimilarStructure";
			this.SimilarStructure.Properties.Appearance.BackColor = System.Drawing.Color.White;
			this.SimilarStructure.Properties.Appearance.Options.UseBackColor = true;
			this.SimilarStructure.Properties.Caption = "123";
			this.SimilarStructure.Size = new System.Drawing.Size(40, 14);
			this.SimilarStructure.TabIndex = 226;
			this.SimilarStructure.CheckedChanged += new System.EventHandler(this.Similar_CheckedChanged);
			// 
			// MatchedPairs
			// 
			this.MatchedPairs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MatchedPairs.EditValue = true;
			this.MatchedPairs.Location = new System.Drawing.Point(100, 25);
			this.MatchedPairs.Name = "MatchedPairs";
			this.MatchedPairs.Properties.Appearance.BackColor = System.Drawing.Color.White;
			this.MatchedPairs.Properties.Appearance.Options.UseBackColor = true;
			this.MatchedPairs.Properties.Caption = "123";
			this.MatchedPairs.Size = new System.Drawing.Size(40, 14);
			this.MatchedPairs.TabIndex = 227;
			this.MatchedPairs.CheckedChanged += new System.EventHandler(this.MatchedPairs_CheckedChanged);
			// 
			// AltForms
			// 
			this.AltForms.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AltForms.EditValue = true;
			this.AltForms.Location = new System.Drawing.Point(100, 4);
			this.AltForms.Name = "AltForms";
			this.AltForms.Properties.Appearance.BackColor = System.Drawing.Color.White;
			this.AltForms.Properties.Appearance.Options.UseBackColor = true;
			this.AltForms.Properties.Caption = "123";
			this.AltForms.Size = new System.Drawing.Size(40, 14);
			this.AltForms.TabIndex = 224;
			this.AltForms.CheckedChanged += new System.EventHandler(this.AltForms_CheckedChanged);
			// 
			// labelControl3
			// 
			this.labelControl3.Appearance.BackColor = System.Drawing.Color.LightSteelBlue;
			this.labelControl3.Appearance.Options.UseBackColor = true;
			this.labelControl3.Appearance.Options.UseTextOptions = true;
			this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.labelControl3.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelControl3.Location = new System.Drawing.Point(1, 64);
			this.labelControl3.Margin = new System.Windows.Forms.Padding(0);
			this.labelControl3.Name = "labelControl3";
			this.labelControl3.Size = new System.Drawing.Size(95, 20);
			this.labelControl3.TabIndex = 223;
			this.labelControl3.Text = "Similar (ECFP4)";
			// 
			// labelControl5
			// 
			this.labelControl5.Appearance.BackColor = System.Drawing.Color.LightSteelBlue;
			this.labelControl5.Appearance.Options.UseBackColor = true;
			this.labelControl5.Appearance.Options.UseTextOptions = true;
			this.labelControl5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.labelControl5.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelControl5.Location = new System.Drawing.Point(1, 22);
			this.labelControl5.Margin = new System.Windows.Forms.Padding(0);
			this.labelControl5.Name = "labelControl5";
			this.labelControl5.Size = new System.Drawing.Size(95, 20);
			this.labelControl5.TabIndex = 223;
			this.labelControl5.Text = "Matched Pairs";
			// 
			// labelControl1
			// 
			this.labelControl1.Appearance.BackColor = System.Drawing.Color.LightSteelBlue;
			this.labelControl1.Appearance.Options.UseBackColor = true;
			this.labelControl1.Appearance.Options.UseTextOptions = true;
			this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.labelControl1.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelControl1.Location = new System.Drawing.Point(1, 1);
			this.labelControl1.Margin = new System.Windows.Forms.Padding(0);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(95, 20);
			this.labelControl1.TabIndex = 223;
			this.labelControl1.Text = "Salts, Isomers...";
			// 
			// MatchCountsPanel
			// 
			this.MatchCountsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.MatchCountsPanel.BackColor = System.Drawing.Color.White;
			this.MatchCountsPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
			this.MatchCountsPanel.ColumnCount = 2;
			this.MatchCountsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68F));
			this.MatchCountsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32F));
			this.MatchCountsPanel.Controls.Add(this.SmallWorldLabel, 0, 2);
			this.MatchCountsPanel.Controls.Add(this.labelControl6, 0, 4);
			this.MatchCountsPanel.Controls.Add(this.SmallWorld, 1, 2);
			this.MatchCountsPanel.Controls.Add(this.labelControl1, 0, 0);
			this.MatchCountsPanel.Controls.Add(this.labelControl5, 0, 1);
			this.MatchCountsPanel.Controls.Add(this.labelControl3, 0, 3);
			this.MatchCountsPanel.Controls.Add(this.AltForms, 1, 0);
			this.MatchCountsPanel.Controls.Add(this.MatchedPairs, 1, 1);
			this.MatchCountsPanel.Controls.Add(this.SimilarStructure, 1, 3);
			this.MatchCountsPanel.Controls.Add(this.AllMatches, 1, 4);
			this.MatchCountsPanel.Location = new System.Drawing.Point(426, 20);
			this.MatchCountsPanel.Name = "MatchCountsPanel";
			this.MatchCountsPanel.RowCount = 5;
			this.MatchCountsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.MatchCountsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.MatchCountsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.MatchCountsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.MatchCountsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.MatchCountsPanel.Size = new System.Drawing.Size(144, 107);
			this.MatchCountsPanel.TabIndex = 239;
			// 
			// SmallWorldLabel
			// 
			this.SmallWorldLabel.Appearance.BackColor = System.Drawing.Color.LightSteelBlue;
			this.SmallWorldLabel.Appearance.Options.UseBackColor = true;
			this.SmallWorldLabel.Appearance.Options.UseTextOptions = true;
			this.SmallWorldLabel.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.SmallWorldLabel.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.SmallWorldLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.SmallWorldLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SmallWorldLabel.Location = new System.Drawing.Point(1, 43);
			this.SmallWorldLabel.Margin = new System.Windows.Forms.Padding(0);
			this.SmallWorldLabel.Name = "SmallWorldLabel";
			this.SmallWorldLabel.Size = new System.Drawing.Size(95, 20);
			this.SmallWorldLabel.TabIndex = 269;
			this.SmallWorldLabel.Text = "SmallWorld";
			// 
			// labelControl6
			// 
			this.labelControl6.Appearance.BackColor = System.Drawing.Color.LightSteelBlue;
			this.labelControl6.Appearance.Options.UseBackColor = true;
			this.labelControl6.Appearance.Options.UseTextOptions = true;
			this.labelControl6.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.labelControl6.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.labelControl6.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelControl6.Location = new System.Drawing.Point(1, 85);
			this.labelControl6.Margin = new System.Windows.Forms.Padding(0);
			this.labelControl6.Name = "labelControl6";
			this.labelControl6.Size = new System.Drawing.Size(95, 21);
			this.labelControl6.TabIndex = 228;
			this.labelControl6.Text = "All Matches";
			// 
			// SmallWorld
			// 
			this.SmallWorld.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SmallWorld.EditValue = true;
			this.SmallWorld.Location = new System.Drawing.Point(100, 46);
			this.SmallWorld.Name = "SmallWorld";
			this.SmallWorld.Properties.Appearance.BackColor = System.Drawing.Color.White;
			this.SmallWorld.Properties.Appearance.Options.UseBackColor = true;
			this.SmallWorld.Properties.Caption = "123";
			this.SmallWorld.Size = new System.Drawing.Size(40, 14);
			this.SmallWorld.TabIndex = 268;
			this.SmallWorld.CheckedChanged += new System.EventHandler(this.SmallWorld_CheckedChanged);
			// 
			// AllMatches
			// 
			this.AllMatches.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AllMatches.EditValue = true;
			this.AllMatches.Location = new System.Drawing.Point(100, 88);
			this.AllMatches.Name = "AllMatches";
			this.AllMatches.Properties.Appearance.BackColor = System.Drawing.Color.White;
			this.AllMatches.Properties.Appearance.Options.UseBackColor = true;
			this.AllMatches.Properties.Caption = "123";
			this.AllMatches.Size = new System.Drawing.Size(40, 15);
			this.AllMatches.TabIndex = 228;
			this.AllMatches.CheckedChanged += new System.EventHandler(this.AllMatches_CheckedChanged);
			// 
			// SearchStatusLabel
			// 
			this.SearchStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SearchStatusLabel.Appearance.BackColor = System.Drawing.Color.White;
			this.SearchStatusLabel.Appearance.BackColor2 = System.Drawing.Color.White;
			this.SearchStatusLabel.Appearance.ForeColor = System.Drawing.Color.DarkRed;
			this.SearchStatusLabel.Appearance.Options.UseBackColor = true;
			this.SearchStatusLabel.Appearance.Options.UseForeColor = true;
			this.SearchStatusLabel.Appearance.Options.UseTextOptions = true;
			this.SearchStatusLabel.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.SearchStatusLabel.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.SearchStatusLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.SearchStatusLabel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.SearchStatusLabel.Location = new System.Drawing.Point(4, 208);
			this.SearchStatusLabel.Margin = new System.Windows.Forms.Padding(0);
			this.SearchStatusLabel.Name = "SearchStatusLabel";
			this.SearchStatusLabel.Size = new System.Drawing.Size(72, 20);
			this.SearchStatusLabel.TabIndex = 247;
			this.SearchStatusLabel.Text = "Searching...";
			// 
			// ExcludeCidsSeenSoFar
			// 
			this.ExcludeCidsSeenSoFar.Location = new System.Drawing.Point(140, 557);
			this.ExcludeCidsSeenSoFar.Name = "ExcludeCidsSeenSoFar";
			this.ExcludeCidsSeenSoFar.Properties.Caption = "Exclude compound Ids contained in the current search results (Hidden)";
			this.ExcludeCidsSeenSoFar.Size = new System.Drawing.Size(435, 19);
			this.ExcludeCidsSeenSoFar.TabIndex = 246;
			this.ExcludeCidsSeenSoFar.Visible = false;
			// 
			// IncludeQueryStruct
			// 
			this.IncludeQueryStruct.Location = new System.Drawing.Point(100, 2);
			this.IncludeQueryStruct.Margin = new System.Windows.Forms.Padding(3, 1, 0, 0);
			this.IncludeQueryStruct.Name = "IncludeQueryStruct";
			this.IncludeQueryStruct.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.IncludeQueryStruct.Properties.Appearance.Options.UseBackColor = true;
			this.IncludeQueryStruct.Properties.Caption = "1";
			this.IncludeQueryStruct.Size = new System.Drawing.Size(43, 19);
			this.IncludeQueryStruct.TabIndex = 250;
			this.IncludeQueryStruct.CheckedChanged += new System.EventHandler(this.IncludeQueryStruct_CheckedChanged);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32F));
			this.tableLayoutPanel1.Controls.Add(this.IncludeQueryStruct, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.labelControl8, 0, 0);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(426, 212);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(144, 23);
			this.tableLayoutPanel1.TabIndex = 251;
			// 
			// labelControl8
			// 
			this.labelControl8.Appearance.BackColor = System.Drawing.Color.LightSteelBlue;
			this.labelControl8.Appearance.Options.UseBackColor = true;
			this.labelControl8.Appearance.Options.UseTextOptions = true;
			this.labelControl8.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.labelControl8.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.labelControl8.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl8.Location = new System.Drawing.Point(1, 1);
			this.labelControl8.Margin = new System.Windows.Forms.Padding(0);
			this.labelControl8.Name = "labelControl8";
			this.labelControl8.Size = new System.Drawing.Size(95, 21);
			this.labelControl8.TabIndex = 232;
			this.labelControl8.Text = "Include Query";
			// 
			// DbMatchCountsPanel
			// 
			this.DbMatchCountsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DbMatchCountsPanel.BackColor = System.Drawing.Color.White;
			this.DbMatchCountsPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
			this.DbMatchCountsPanel.ColumnCount = 2;
			this.DbMatchCountsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68F));
			this.DbMatchCountsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32F));
			this.DbMatchCountsPanel.Controls.Add(this.ChemblDB, 1, 1);
			this.DbMatchCountsPanel.Controls.Add(this.labelControl7, 0, 1);
			this.DbMatchCountsPanel.Controls.Add(this.labelControl2, 0, 0);
			this.DbMatchCountsPanel.Controls.Add(this.CorpDB, 1, 0);
			this.DbMatchCountsPanel.Location = new System.Drawing.Point(426, 157);
			this.DbMatchCountsPanel.Name = "DbMatchCountsPanel";
			this.DbMatchCountsPanel.RowCount = 2;
			this.DbMatchCountsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.DbMatchCountsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.DbMatchCountsPanel.Size = new System.Drawing.Size(144, 44);
			this.DbMatchCountsPanel.TabIndex = 267;
			// 
			// ChemblDB
			// 
			this.ChemblDB.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChemblDB.Location = new System.Drawing.Point(100, 25);
			this.ChemblDB.Name = "ChemblDB";
			this.ChemblDB.Properties.Appearance.BackColor = System.Drawing.Color.White;
			this.ChemblDB.Properties.Appearance.Options.UseBackColor = true;
			this.ChemblDB.Properties.Caption = "123";
			this.ChemblDB.Size = new System.Drawing.Size(40, 15);
			this.ChemblDB.TabIndex = 269;
			this.ChemblDB.CheckedChanged += new System.EventHandler(this.ChemblDB_CheckedChanged);
			// 
			// labelControl7
			// 
			this.labelControl7.Appearance.BackColor = System.Drawing.Color.LightSteelBlue;
			this.labelControl7.Appearance.Options.UseBackColor = true;
			this.labelControl7.Appearance.Options.UseTextOptions = true;
			this.labelControl7.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.labelControl7.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.labelControl7.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl7.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelControl7.Location = new System.Drawing.Point(1, 22);
			this.labelControl7.Margin = new System.Windows.Forms.Padding(0);
			this.labelControl7.Name = "labelControl7";
			this.labelControl7.Size = new System.Drawing.Size(95, 21);
			this.labelControl7.TabIndex = 271;
			this.labelControl7.Text = "ChEMBL";
			// 
			// labelControl2
			// 
			this.labelControl2.Appearance.BackColor = System.Drawing.Color.LightSteelBlue;
			this.labelControl2.Appearance.Options.UseBackColor = true;
			this.labelControl2.Appearance.Options.UseTextOptions = true;
			this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.labelControl2.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelControl2.Location = new System.Drawing.Point(1, 1);
			this.labelControl2.Margin = new System.Windows.Forms.Padding(0);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(95, 20);
			this.labelControl2.TabIndex = 270;
			this.labelControl2.Text = "CorpDb";
			// 
			// CorpDB
			// 
			this.CorpDB.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CorpDB.EditValue = true;
			this.CorpDB.Location = new System.Drawing.Point(100, 4);
			this.CorpDB.Name = "CorpDB";
			this.CorpDB.Properties.Appearance.BackColor = System.Drawing.Color.White;
			this.CorpDB.Properties.Appearance.Options.UseBackColor = true;
			this.CorpDB.Properties.Caption = "123";
			this.CorpDB.Size = new System.Drawing.Size(40, 14);
			this.CorpDB.TabIndex = 268;
			this.CorpDB.CheckedChanged += new System.EventHandler(this.CorpDB_CheckedChanged);
			// 
			// DatabasesLabel
			// 
			this.DatabasesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DatabasesLabel.Appearance.BackColor = System.Drawing.Color.LightSteelBlue;
			this.DatabasesLabel.Appearance.Options.UseBackColor = true;
			this.DatabasesLabel.Appearance.Options.UseTextOptions = true;
			this.DatabasesLabel.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.DatabasesLabel.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.DatabasesLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.DatabasesLabel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
			this.DatabasesLabel.Location = new System.Drawing.Point(426, 137);
			this.DatabasesLabel.Margin = new System.Windows.Forms.Padding(0);
			this.DatabasesLabel.Name = "DatabasesLabel";
			this.DatabasesLabel.Size = new System.Drawing.Size(144, 20);
			this.DatabasesLabel.TabIndex = 266;
			this.DatabasesLabel.Text = "Matches by Database";
			// 
			// MoleculeControl
			// 
			this.MoleculeControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MoleculeControl.Appearance.BackColor = System.Drawing.Color.White;
			this.MoleculeControl.Appearance.Options.UseBackColor = true;
			this.MoleculeControl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.MoleculeControl.Location = new System.Drawing.Point(0, 0);
			this.MoleculeControl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
			this.MoleculeControl.Molecule = moleculeMx1;
			this.MoleculeControl.Name = "MoleculeControl";
			this.MoleculeControl.Size = new System.Drawing.Size(420, 238);
			this.MoleculeControl.TabIndex = 268;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Horizontal = false;
			this.SplitContainer.Location = new System.Drawing.Point(0, 0);
			this.SplitContainer.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Panel1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
			this.SplitContainer.Panel1.Controls.Add(this.MatchCountsPanel);
			this.SplitContainer.Panel1.Controls.Add(this.SearchStatusLabel);
			this.SplitContainer.Panel1.Controls.Add(this.RelatedCorpDbStructureLabel
);
			this.SplitContainer.Panel1.Controls.Add(this.MoleculeControl);
			this.SplitContainer.Panel1.Controls.Add(this.tableLayoutPanel1);
			this.SplitContainer.Panel1.Controls.Add(this.DbMatchCountsPanel);
			this.SplitContainer.Panel1.Controls.Add(this.DatabasesLabel);
			this.SplitContainer.Panel1.Text = "Panel1";
			this.SplitContainer.Panel2.Controls.Add(this.MoleculeGridPageControl);
			this.SplitContainer.Panel2.Text = "Panel2";
			this.SplitContainer.ShowSplitGlyph = DevExpress.Utils.DefaultBoolean.True;
			this.SplitContainer.Size = new System.Drawing.Size(574, 777);
			this.SplitContainer.SplitterPosition = 244;
			this.SplitContainer.TabIndex = 269;
			// 
			// MoleculeGridPageControl
			// 
			this.MoleculeGridPageControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.MoleculeGridPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MoleculeGridPageControl.Location = new System.Drawing.Point(0, 0);
			this.MoleculeGridPageControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MoleculeGridPageControl.Name = "MoleculeGridPageControl";
			this.MoleculeGridPageControl.Size = new System.Drawing.Size(574, 528);
			this.MoleculeGridPageControl.TabIndex = 249;
			// 
			// RelatedStructuresControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.SplitContainer);
			this.Controls.Add(this.ExcludeCidsSeenSoFar);
			this.Name = "RelatedStructuresControl";
			this.Size = new System.Drawing.Size(574, 777);
			this.SizeChanged += new System.EventHandler(this.RelatedStructuresControl_SizeChanged);
			((System.ComponentModel.ISupportInitialize)(this.SimilarStructure.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MatchedPairs.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AltForms.Properties)).EndInit();
			this.MatchCountsPanel.ResumeLayout(false);
			this.MatchCountsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SmallWorld.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AllMatches.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ExcludeCidsSeenSoFar.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IncludeQueryStruct.Properties)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.DbMatchCountsPanel.ResumeLayout(false);
			this.DbMatchCountsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChemblDB.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CorpDB.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private DevExpress.XtraEditors.CheckEdit SimilarStructure;
		private DevExpress.XtraEditors.CheckEdit MatchedPairs;
		private DevExpress.XtraEditors.CheckEdit AltForms;
		private DevExpress.XtraEditors.LabelControl labelControl3;
		private DevExpress.XtraEditors.LabelControl labelControl5;
		private DevExpress.XtraEditors.LabelControl labelControl1;
		public System.Windows.Forms.TableLayoutPanel MatchCountsPanel;
		public DevExpress.XtraEditors.LabelControl RelatedCorpDbStructureLabel
;
		public DevExpress.XtraEditors.CheckEdit ExcludeCidsSeenSoFar;
		public DevExpress.XtraEditors.LabelControl SearchStatusLabel;
		public MoleculeGridPageControl MoleculeGridPageControl;
		public DevExpress.XtraEditors.CheckEdit IncludeQueryStruct;
		public System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		public System.Windows.Forms.TableLayoutPanel DbMatchCountsPanel;
		private DevExpress.XtraEditors.LabelControl DatabasesLabel;
		private DevExpress.XtraEditors.LabelControl SmallWorldLabel;
		public DevExpress.XtraEditors.CheckEdit SmallWorld;
		private DevExpress.XtraEditors.CheckEdit ChemblDB;
		private DevExpress.XtraEditors.CheckEdit CorpDB;
		private DevExpress.XtraEditors.LabelControl labelControl7;
		private DevExpress.XtraEditors.LabelControl labelControl2;
		private DevExpress.XtraEditors.LabelControl labelControl8;
		private DevExpress.XtraEditors.LabelControl labelControl6;
		public DevExpress.XtraEditors.CheckEdit AllMatches;
		public MoleculeControl MoleculeControl;
		private DevExpress.XtraEditors.SplitContainerControl SplitContainer;
	}
}
