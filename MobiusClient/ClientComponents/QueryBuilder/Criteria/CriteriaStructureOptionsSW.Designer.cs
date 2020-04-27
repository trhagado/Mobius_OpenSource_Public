namespace Mobius.ClientComponents
{
	partial class CriteriaStructureOptionsSW
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CriteriaStructureOptionsSW));
			DevExpress.Utils.SuperToolTip superToolTip1 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipItem toolTipItem1 = new DevExpress.Utils.ToolTipItem();
			this.PresetsComboBox = new DevExpress.XtraEditors.ComboBoxEdit();
			this.MatchAtomTypes = new DevExpress.XtraEditors.CheckEdit();
			this.ToolTipController = new DevExpress.Utils.ToolTipController();
			this.AdvancedOptionsGroupControl = new DevExpress.XtraEditors.GroupControl();
			this.AdvancedOptionsScrollableControl = new DevExpress.XtraEditors.XtraScrollableControl();
			this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
			this.MutationRangeMajor = new Mobius.ClientComponents.CriteriaStructureRangeCtl();
			this.Identical = new DevExpress.XtraEditors.SimpleButton();
			this.HybridizationRange = new Mobius.ClientComponents.CriteriaStructureRangeCtl();
			this.AlignStructs = new DevExpress.XtraEditors.CheckEdit();
			this.SubstitutionRange = new Mobius.ClientComponents.CriteriaStructureRangeCtl();
			this.ShowColors = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl11 = new DevExpress.XtraEditors.LabelControl();
			this.LinkerRangeUp = new Mobius.ClientComponents.CriteriaStructureRangeCtl();
			this.TerminalRangeUp = new Mobius.ClientComponents.CriteriaStructureRangeCtl();
			this.LinkerRangeDown = new Mobius.ClientComponents.CriteriaStructureRangeCtl();
			this.RingRangeDown = new Mobius.ClientComponents.CriteriaStructureRangeCtl();
			this.RingRangeUp = new Mobius.ClientComponents.CriteriaStructureRangeCtl();
			this.TerminalRangeDown = new Mobius.ClientComponents.CriteriaStructureRangeCtl();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl10 = new DevExpress.XtraEditors.LabelControl();
			this.MutationRangeMinor = new Mobius.ClientComponents.CriteriaStructureRangeCtl();
			this.SavePreferredSettingsButton = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl9 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
			this.Bitmaps8x8 = new System.Windows.Forms.ImageList();
			this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
			this.MaxHits = new DevExpress.XtraEditors.TextEdit();
			this.MaxHits_Label = new DevExpress.XtraEditors.LabelControl();
			this.DistanceRangeLabel = new DevExpress.XtraEditors.LabelControl();
			this.DistanceRange = new DevExpress.XtraEditors.RangeTrackBarControl();
			this.DatabaseComboBox = new DevExpress.XtraEditors.ComboBoxEdit();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.groupControl3 = new DevExpress.XtraEditors.GroupControl();
			this.HelpButton = new DevExpress.XtraEditors.SimpleButton();
			((System.ComponentModel.ISupportInitialize)(this.PresetsComboBox.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MatchAtomTypes.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AdvancedOptionsGroupControl)).BeginInit();
			this.AdvancedOptionsGroupControl.SuspendLayout();
			this.AdvancedOptionsScrollableControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AlignStructs.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ShowColors.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MaxHits.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DistanceRange)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DistanceRange.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DatabaseComboBox.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.groupControl3)).BeginInit();
			this.groupControl3.SuspendLayout();
			this.SuspendLayout();
			// 
			// PresetsComboBox
			// 
			this.PresetsComboBox.EditValue = "SmallWorld";
			this.PresetsComboBox.Location = new System.Drawing.Point(92, 4);
			this.PresetsComboBox.Margin = new System.Windows.Forms.Padding(2);
			this.PresetsComboBox.Name = "PresetsComboBox";
			this.PresetsComboBox.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.PresetsComboBox.Properties.Items.AddRange(new object[] {
            "SmallWorld",
            "Substructure",
            "SuperStructure",
            "Bemis-Murcko Framework",
            "Nq MCS",
            "Element Graph",
            "Custom"});
			this.PresetsComboBox.Size = new System.Drawing.Size(137, 20);
			this.PresetsComboBox.TabIndex = 32;
			this.PresetsComboBox.EditValueChanged += new System.EventHandler(this.PresetsComboBox_EditValueChanged);
			// 
			// MatchAtomTypes
			// 
			this.MatchAtomTypes.EditValue = true;
			this.MatchAtomTypes.Location = new System.Drawing.Point(228, 140);
			this.MatchAtomTypes.Name = "MatchAtomTypes";
			this.MatchAtomTypes.Properties.Caption = "Enable atom matching";
			this.MatchAtomTypes.Properties.GlyphAlignment = DevExpress.Utils.HorzAlignment.Default;
			this.MatchAtomTypes.Size = new System.Drawing.Size(133, 19);
			this.MatchAtomTypes.TabIndex = 115;
			this.MatchAtomTypes.CheckedChanged += new System.EventHandler(this.AtomTypeMatch_CheckedChanged);
			// 
			// ToolTipController
			// 
			this.ToolTipController.AutoPopDelay = 20000;
			this.ToolTipController.CloseOnClick = DevExpress.Utils.DefaultBoolean.True;
			this.ToolTipController.InitialDelay = 1;
			this.ToolTipController.ReshowDelay = 1;
			// 
			// AdvancedOptionsGroupControl
			// 
			this.AdvancedOptionsGroupControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.AdvancedOptionsGroupControl.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.AdvancedOptionsGroupControl.AppearanceCaption.Options.UseFont = true;
			this.AdvancedOptionsGroupControl.Controls.Add(this.AdvancedOptionsScrollableControl);
			this.AdvancedOptionsGroupControl.Location = new System.Drawing.Point(0, 88);
			this.AdvancedOptionsGroupControl.Name = "AdvancedOptionsGroupControl";
			this.AdvancedOptionsGroupControl.Size = new System.Drawing.Size(383, 263);
			this.AdvancedOptionsGroupControl.TabIndex = 129;
			this.AdvancedOptionsGroupControl.Text = "Advanced Search Options";
			this.AdvancedOptionsGroupControl.ToolTipController = this.ToolTipController;
			// 
			// AdvancedOptionsScrollableControl
			// 
			this.AdvancedOptionsScrollableControl.Controls.Add(this.MatchAtomTypes);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.labelControl4);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.MutationRangeMajor);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.Identical);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.HybridizationRange);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.AlignStructs);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.SubstitutionRange);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.ShowColors);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.labelControl3);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.labelControl11);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.LinkerRangeUp);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.TerminalRangeUp);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.LinkerRangeDown);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.RingRangeDown);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.RingRangeUp);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.TerminalRangeDown);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.labelControl2);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.labelControl10);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.MutationRangeMinor);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.SavePreferredSettingsButton);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.labelControl6);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.PresetsComboBox);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.labelControl7);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.labelControl9);
			this.AdvancedOptionsScrollableControl.Controls.Add(this.labelControl8);
			this.AdvancedOptionsScrollableControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdvancedOptionsScrollableControl.Location = new System.Drawing.Point(2, 20);
			this.AdvancedOptionsScrollableControl.Name = "AdvancedOptionsScrollableControl";
			this.AdvancedOptionsScrollableControl.Size = new System.Drawing.Size(379, 241);
			this.AdvancedOptionsScrollableControl.TabIndex = 131;
			// 
			// labelControl4
			// 
			this.labelControl4.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl4.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl4.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.labelControl4.Appearance.Options.UseBackColor = true;
			this.labelControl4.Appearance.Options.UseFont = true;
			this.labelControl4.Appearance.Options.UseForeColor = true;
			this.labelControl4.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl4.Location = new System.Drawing.Point(4, 5);
			this.labelControl4.Name = "labelControl4";
			this.labelControl4.Size = new System.Drawing.Size(79, 13);
			this.labelControl4.TabIndex = 137;
			this.labelControl4.Text = "Search Subtype:";
			// 
			// MutationRangeMajor
			// 
			this.MutationRangeMajor.AccessibleDescription = "StructureSearchOptionsSmallWorld";
			this.MutationRangeMajor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(182)))), ((int)(((byte)(126)))));
			this.MutationRangeMajor.Location = new System.Drawing.Point(219, 162);
			this.MutationRangeMajor.Margin = new System.Windows.Forms.Padding(4);
			this.MutationRangeMajor.Name = "MutationRangeMajor";
			this.MutationRangeMajor.RangeSuffix = "Major";
			this.MutationRangeMajor.Size = new System.Drawing.Size(143, 24);
			this.MutationRangeMajor.TabIndex = 134;
			this.MutationRangeMajor.ToolTipMx = "Set in code (multiline)";
			this.MutationRangeMajor.EditValueChanged += new System.EventHandler(this.MutationRangeMajor_EditValueChanged);
			// 
			// Identical
			// 
			this.Identical.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(190)))), ((int)(((byte)(132)))));
			this.Identical.Appearance.Options.UseBackColor = true;
			this.Identical.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.Identical.Location = new System.Drawing.Point(70, 211);
			this.Identical.Name = "Identical";
			this.Identical.Size = new System.Drawing.Size(115, 24);
			this.Identical.TabIndex = 136;
			this.Identical.Text = "Identical Atom/Bond";
			// 
			// HybridizationRange
			// 
			this.HybridizationRange.AccessibleDescription = "StructureSearchOptionsSmallWorld";
			this.HybridizationRange.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(173)))), ((int)(((byte)(255)))), ((int)(((byte)(171)))));
			this.HybridizationRange.Location = new System.Drawing.Point(219, 186);
			this.HybridizationRange.Margin = new System.Windows.Forms.Padding(4);
			this.HybridizationRange.Name = "HybridizationRange";
			this.HybridizationRange.RangeSuffix = "";
			this.HybridizationRange.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.HybridizationRange.Size = new System.Drawing.Size(114, 24);
			this.HybridizationRange.TabIndex = 51;
			this.HybridizationRange.ToolTipMx = "Atoms with a valence difference or connectivity difference.";
			this.HybridizationRange.EditValueChanged += new System.EventHandler(this.HybridizationRange_EditValueChanged);
			// 
			// AlignStructs
			// 
			this.AlignStructs.EditValue = true;
			this.AlignStructs.Location = new System.Drawing.Point(221, 213);
			this.AlignStructs.Name = "AlignStructs";
			this.AlignStructs.Properties.Caption = "Align Structures";
			this.AlignStructs.Properties.GlyphAlignment = DevExpress.Utils.HorzAlignment.Default;
			this.AlignStructs.Size = new System.Drawing.Size(108, 19);
			this.AlignStructs.TabIndex = 130;
			this.AlignStructs.CheckedChanged += new System.EventHandler(this.AlignStructs_CheckedChanged);
			// 
			// SubstitutionRange
			// 
			this.SubstitutionRange.AccessibleDescription = "StructureSearchOptionsSmallWorld";
			this.SubstitutionRange.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(173)))), ((int)(((byte)(255)))), ((int)(((byte)(171)))));
			this.SubstitutionRange.Location = new System.Drawing.Point(71, 186);
			this.SubstitutionRange.Margin = new System.Windows.Forms.Padding(4);
			this.SubstitutionRange.Name = "SubstitutionRange";
			this.SubstitutionRange.RangeSuffix = "";
			this.SubstitutionRange.Size = new System.Drawing.Size(115, 24);
			this.SubstitutionRange.TabIndex = 50;
			this.SubstitutionRange.ToolTipMx = "Atoms with equal valence and connectivity \"heavy degree difference\".";
			this.SubstitutionRange.EditValueChanged += new System.EventHandler(this.SubstitutionRange_EditValueChanged);
			// 
			// ShowColors
			// 
			this.ShowColors.EditValue = true;
			this.ShowColors.Location = new System.Drawing.Point(10, 213);
			this.ShowColors.Name = "ShowColors";
			this.ShowColors.Properties.Caption = "Color";
			this.ShowColors.Properties.GlyphAlignment = DevExpress.Utils.HorzAlignment.Default;
			this.ShowColors.Size = new System.Drawing.Size(50, 19);
			this.ShowColors.TabIndex = 129;
			this.ShowColors.CheckedChanged += new System.EventHandler(this.ShowColors_CheckedChanged);
			// 
			// labelControl3
			// 
			this.labelControl3.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl3.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl3.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.labelControl3.Appearance.Options.UseBackColor = true;
			this.labelControl3.Appearance.Options.UseFont = true;
			this.labelControl3.Appearance.Options.UseForeColor = true;
			this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl3.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl3.LineVisible = true;
			this.labelControl3.Location = new System.Drawing.Point(4, 137);
			this.labelControl3.Name = "labelControl3";
			this.labelControl3.Size = new System.Drawing.Size(357, 23);
			this.labelControl3.TabIndex = 50;
			this.labelControl3.Text = "Atom type matching variations allowed";
			// 
			// labelControl11
			// 
			this.labelControl11.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl11.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl11.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.labelControl11.Appearance.Options.UseBackColor = true;
			this.labelControl11.Appearance.Options.UseFont = true;
			this.labelControl11.Appearance.Options.UseForeColor = true;
			this.labelControl11.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl11.Location = new System.Drawing.Point(195, 191);
			this.labelControl11.Name = "labelControl11";
			this.labelControl11.Size = new System.Drawing.Size(22, 13);
			this.labelControl11.TabIndex = 135;
			this.labelControl11.Text = "Hyb:";
			// 
			// LinkerRangeUp
			// 
			this.LinkerRangeUp.AccessibleDescription = "StructureSearchOptionsSmallWorld";
			this.LinkerRangeUp.Location = new System.Drawing.Point(74, 100);
			this.LinkerRangeUp.Margin = new System.Windows.Forms.Padding(4);
			this.LinkerRangeUp.Name = "LinkerRangeUp";
			this.LinkerRangeUp.RangeSuffix = "(+) ";
			this.LinkerRangeUp.Size = new System.Drawing.Size(136, 24);
			this.LinkerRangeUp.TabIndex = 48;
			this.LinkerRangeUp.ToolTipMx = "Non-terminal bonds, that do not create or break a ring, added or removed ";
			this.LinkerRangeUp.EditValueChanged += new System.EventHandler(this.LinkerRangeUp_EditValueChanged);
			// 
			// TerminalRangeUp
			// 
			this.TerminalRangeUp.AccessibleDescription = "StructureSearchOptionsSmallWorld";
			this.TerminalRangeUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(190)))), ((int)(((byte)(196)))));
			this.TerminalRangeUp.Location = new System.Drawing.Point(73, 51);
			this.TerminalRangeUp.Margin = new System.Windows.Forms.Padding(4);
			this.TerminalRangeUp.Name = "TerminalRangeUp";
			this.TerminalRangeUp.RangeSuffix = "(+) ";
			this.TerminalRangeUp.Size = new System.Drawing.Size(137, 24);
			this.TerminalRangeUp.TabIndex = 46;
			this.TerminalRangeUp.ToolTipMx = "Terminal acyclic bonds added or removed";
			this.TerminalRangeUp.EditValueChanged += new System.EventHandler(this.TerminalRangeUp_EditValueChanged);
			// 
			// LinkerRangeDown
			// 
			this.LinkerRangeDown.AccessibleDescription = "StructureSearchOptionsSmallWorld";
			this.LinkerRangeDown.Location = new System.Drawing.Point(222, 100);
			this.LinkerRangeDown.Margin = new System.Windows.Forms.Padding(4);
			this.LinkerRangeDown.Name = "LinkerRangeDown";
			this.LinkerRangeDown.RangeSuffix = "(--) ";
			this.LinkerRangeDown.Size = new System.Drawing.Size(139, 24);
			this.LinkerRangeDown.TabIndex = 133;
			this.LinkerRangeDown.ToolTipMx = "Terminal acyclic bonds added or removed";
			this.LinkerRangeDown.EditValueChanged += new System.EventHandler(this.LinkerRangeDown_EditValueChanged);
			// 
			// RingRangeDown
			// 
			this.RingRangeDown.AccessibleDescription = "StructureSearchOptionsSmallWorld";
			this.RingRangeDown.Location = new System.Drawing.Point(222, 76);
			this.RingRangeDown.Margin = new System.Windows.Forms.Padding(4);
			this.RingRangeDown.Name = "RingRangeDown";
			this.RingRangeDown.RangeSuffix = "(--) ";
			this.RingRangeDown.Size = new System.Drawing.Size(136, 24);
			this.RingRangeDown.TabIndex = 132;
			this.RingRangeDown.ToolTipMx = "Terminal acyclic bonds added or removed";
			this.RingRangeDown.EditValueChanged += new System.EventHandler(this.RingRangeDown_EditValueChanged);
			// 
			// RingRangeUp
			// 
			this.RingRangeUp.AccessibleDescription = "StructureSearchOptionsSmallWorld";
			this.RingRangeUp.Location = new System.Drawing.Point(74, 76);
			this.RingRangeUp.Margin = new System.Windows.Forms.Padding(4);
			this.RingRangeUp.Name = "RingRangeUp";
			this.RingRangeUp.RangeSuffix = "(+) ";
			this.RingRangeUp.Size = new System.Drawing.Size(136, 24);
			this.RingRangeUp.TabIndex = 47;
			this.RingRangeUp.ToolTipMx = "Ring bonds added or removed";
			this.RingRangeUp.EditValueChanged += new System.EventHandler(this.RingRangeUp_EditValueChanged);
			// 
			// TerminalRangeDown
			// 
			this.TerminalRangeDown.AccessibleDescription = "StructureSearchOptionsSmallWorld";
			this.TerminalRangeDown.Location = new System.Drawing.Point(222, 51);
			this.TerminalRangeDown.Margin = new System.Windows.Forms.Padding(4);
			this.TerminalRangeDown.Name = "TerminalRangeDown";
			this.TerminalRangeDown.RangeSuffix = "(--) ";
			this.TerminalRangeDown.Size = new System.Drawing.Size(136, 24);
			this.TerminalRangeDown.TabIndex = 131;
			this.TerminalRangeDown.ToolTipMx = "Terminal acyclic bonds added or removed";
			this.TerminalRangeDown.EditValueChanged += new System.EventHandler(this.TerminalRangeDown_EditValueChanged);
			// 
			// labelControl2
			// 
			this.labelControl2.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl2.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl2.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.labelControl2.Appearance.Options.UseBackColor = true;
			this.labelControl2.Appearance.Options.UseFont = true;
			this.labelControl2.Appearance.Options.UseForeColor = true;
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl2.LineVisible = true;
			this.labelControl2.Location = new System.Drawing.Point(4, 31);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(355, 13);
			this.labelControl2.TabIndex = 49;
			this.labelControl2.Text = "Bond additions/breaks allowed";
			// 
			// labelControl10
			// 
			this.labelControl10.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl10.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl10.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.labelControl10.Appearance.Options.UseBackColor = true;
			this.labelControl10.Appearance.Options.UseFont = true;
			this.labelControl10.Appearance.Options.UseForeColor = true;
			this.labelControl10.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl10.Location = new System.Drawing.Point(17, 190);
			this.labelControl10.Name = "labelControl10";
			this.labelControl10.Size = new System.Drawing.Size(41, 13);
			this.labelControl10.TabIndex = 130;
			this.labelControl10.Text = "H-Subst:";
			// 
			// MutationRangeMinor
			// 
			this.MutationRangeMinor.AccessibleDescription = "StructureSearchOptionsSmallWorld";
			this.MutationRangeMinor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(242)))), ((int)(((byte)(119)))));
			this.MutationRangeMinor.Location = new System.Drawing.Point(71, 162);
			this.MutationRangeMinor.Margin = new System.Windows.Forms.Padding(4);
			this.MutationRangeMinor.Name = "MutationRangeMinor";
			this.MutationRangeMinor.RangeSuffix = "Minor";
			this.MutationRangeMinor.Size = new System.Drawing.Size(143, 24);
			this.MutationRangeMinor.TabIndex = 49;
			this.MutationRangeMinor.ToolTipMx = "Set in code (multiline)";
			this.MutationRangeMinor.EditValueChanged += new System.EventHandler(this.MutationRangeMinor_EditValueChanged);
			// 
			// SavePreferredSettingsButton
			// 
			this.SavePreferredSettingsButton.Location = new System.Drawing.Point(257, 4);
			this.SavePreferredSettingsButton.Name = "SavePreferredSettingsButton";
			this.SavePreferredSettingsButton.Size = new System.Drawing.Size(102, 20);
			this.SavePreferredSettingsButton.TabIndex = 116;
			this.SavePreferredSettingsButton.Text = "Save Preferences";
			this.SavePreferredSettingsButton.Click += new System.EventHandler(this.SavePreferredSettings_Click);
			// 
			// labelControl6
			// 
			this.labelControl6.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl6.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl6.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.labelControl6.Appearance.Options.UseBackColor = true;
			this.labelControl6.Appearance.Options.UseFont = true;
			this.labelControl6.Appearance.Options.UseForeColor = true;
			this.labelControl6.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl6.Location = new System.Drawing.Point(26, 57);
			this.labelControl6.Name = "labelControl6";
			this.labelControl6.Size = new System.Drawing.Size(43, 13);
			this.labelControl6.TabIndex = 126;
			this.labelControl6.Text = "Terminal:";
			// 
			// labelControl7
			// 
			this.labelControl7.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl7.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl7.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.labelControl7.Appearance.Options.UseBackColor = true;
			this.labelControl7.Appearance.Options.UseFont = true;
			this.labelControl7.Appearance.Options.UseForeColor = true;
			this.labelControl7.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl7.Location = new System.Drawing.Point(14, 81);
			this.labelControl7.Name = "labelControl7";
			this.labelControl7.Size = new System.Drawing.Size(58, 13);
			this.labelControl7.TabIndex = 127;
			this.labelControl7.Text = "Ring Bonds:";
			// 
			// labelControl9
			// 
			this.labelControl9.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl9.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl9.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.labelControl9.Appearance.Options.UseBackColor = true;
			this.labelControl9.Appearance.Options.UseFont = true;
			this.labelControl9.Appearance.Options.UseForeColor = true;
			this.labelControl9.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl9.Location = new System.Drawing.Point(11, 167);
			this.labelControl9.Name = "labelControl9";
			this.labelControl9.Size = new System.Drawing.Size(49, 13);
			this.labelControl9.TabIndex = 129;
			this.labelControl9.Text = "Mutations:";
			// 
			// labelControl8
			// 
			this.labelControl8.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl8.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl8.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.labelControl8.Appearance.Options.UseBackColor = true;
			this.labelControl8.Appearance.Options.UseFont = true;
			this.labelControl8.Appearance.Options.UseForeColor = true;
			this.labelControl8.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl8.Location = new System.Drawing.Point(7, 105);
			this.labelControl8.Name = "labelControl8";
			this.labelControl8.Size = new System.Drawing.Size(65, 13);
			this.labelControl8.TabIndex = 128;
			this.labelControl8.Text = "Linker Bonds:";
			// 
			// Bitmaps8x8
			// 
			this.Bitmaps8x8.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps8x8.ImageStream")));
			this.Bitmaps8x8.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps8x8.Images.SetKeyName(0, "DropDownChevron8x8.bmp");
			this.Bitmaps8x8.Images.SetKeyName(1, "DropUpChevron8x8.bmp");
			// 
			// labelControl5
			// 
			this.labelControl5.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl5.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl5.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.labelControl5.Appearance.Options.UseBackColor = true;
			this.labelControl5.Appearance.Options.UseFont = true;
			this.labelControl5.Appearance.Options.UseForeColor = true;
			this.labelControl5.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl5.Location = new System.Drawing.Point(5, 32);
			this.labelControl5.Name = "labelControl5";
			this.labelControl5.Size = new System.Drawing.Size(45, 13);
			this.labelControl5.TabIndex = 125;
			this.labelControl5.Text = "Distance:";
			// 
			// MaxHits
			// 
			this.MaxHits.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.MaxHits.EditValue = "100";
			this.MaxHits.Location = new System.Drawing.Point(332, 29);
			this.MaxHits.Name = "MaxHits";
			this.MaxHits.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.MaxHits.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaxHits.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.MaxHits.Properties.Appearance.Options.UseBackColor = true;
			this.MaxHits.Properties.Appearance.Options.UseFont = true;
			this.MaxHits.Properties.Appearance.Options.UseForeColor = true;
			this.MaxHits.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.MaxHits.Size = new System.Drawing.Size(42, 20);
			this.MaxHits.TabIndex = 124;
			this.MaxHits.ToolTip = "Maximum number of hits to retrieve for a search";
			this.MaxHits.EditValueChanged += new System.EventHandler(this.MaxHits_EditValueChanged);
			// 
			// MaxHits_Label
			// 
			this.MaxHits_Label.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.MaxHits_Label.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaxHits_Label.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.MaxHits_Label.Appearance.Options.UseBackColor = true;
			this.MaxHits_Label.Appearance.Options.UseFont = true;
			this.MaxHits_Label.Appearance.Options.UseForeColor = true;
			this.MaxHits_Label.Cursor = System.Windows.Forms.Cursors.Default;
			this.MaxHits_Label.Location = new System.Drawing.Point(282, 32);
			this.MaxHits_Label.Name = "MaxHits_Label";
			this.MaxHits_Label.Size = new System.Drawing.Size(45, 13);
			this.MaxHits_Label.TabIndex = 123;
			this.MaxHits_Label.Text = "Max, hits:";
			// 
			// DistanceRangeLabel
			// 
			this.DistanceRangeLabel.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.DistanceRangeLabel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DistanceRangeLabel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.DistanceRangeLabel.Appearance.Options.UseBackColor = true;
			this.DistanceRangeLabel.Appearance.Options.UseFont = true;
			this.DistanceRangeLabel.Appearance.Options.UseForeColor = true;
			this.DistanceRangeLabel.Cursor = System.Windows.Forms.Cursors.Default;
			this.DistanceRangeLabel.Location = new System.Drawing.Point(230, 32);
			this.DistanceRangeLabel.Name = "DistanceRangeLabel";
			this.DistanceRangeLabel.Size = new System.Drawing.Size(21, 13);
			this.DistanceRangeLabel.TabIndex = 127;
			this.DistanceRangeLabel.Text = "0-25";
			// 
			// DistanceRange
			// 
			this.DistanceRange.EditValue = new DevExpress.XtraEditors.Repository.TrackBarRange(0, 25);
			this.DistanceRange.Location = new System.Drawing.Point(57, 24);
			this.DistanceRange.Name = "DistanceRange";
			this.DistanceRange.Properties.AutoSize = false;
			this.DistanceRange.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.DistanceRange.Properties.LabelAppearance.Options.UseTextOptions = true;
			this.DistanceRange.Properties.LabelAppearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.DistanceRange.Properties.LookAndFeel.SkinName = "Seven";
			this.DistanceRange.Properties.LookAndFeel.UseDefaultLookAndFeel = false;
			this.DistanceRange.Properties.Maximum = 25;
			this.DistanceRange.Properties.TickFrequency = 25;
			this.DistanceRange.Properties.TickStyle = System.Windows.Forms.TickStyle.Both;
			this.DistanceRange.Size = new System.Drawing.Size(169, 29);
			this.DistanceRange.TabIndex = 126;
			this.DistanceRange.ToolTip = "Total allowed distance range for hits";
			this.DistanceRange.Value = new DevExpress.XtraEditors.Repository.TrackBarRange(0, 25);
			this.DistanceRange.EditValueChanged += new System.EventHandler(this.DistanceRange_EditValueChanged);
			// 
			// DatabaseComboBox
			// 
			this.DatabaseComboBox.EditValue = "ChEMBL, PubChem";
			this.DatabaseComboBox.Location = new System.Drawing.Point(63, 59);
			this.DatabaseComboBox.Margin = new System.Windows.Forms.Padding(2);
			this.DatabaseComboBox.Name = "DatabaseComboBox";
			this.DatabaseComboBox.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.DatabaseComboBox.Properties.ReadOnly = true;
			this.DatabaseComboBox.Properties.ShowDropDown = DevExpress.XtraEditors.Controls.ShowDropDown.Never;
			this.DatabaseComboBox.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
			this.DatabaseComboBox.Properties.UseReadOnlyAppearance = false;
			this.DatabaseComboBox.Size = new System.Drawing.Size(311, 20);
			this.DatabaseComboBox.TabIndex = 34;
			this.DatabaseComboBox.ToolTip = "List of databases to search";
			this.DatabaseComboBox.Click += new System.EventHandler(this.DatabaseComboBox_Click);
			// 
			// labelControl1
			// 
			this.labelControl1.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl1.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl1.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.labelControl1.Appearance.Options.UseBackColor = true;
			this.labelControl1.Appearance.Options.UseFont = true;
			this.labelControl1.Appearance.Options.UseForeColor = true;
			this.labelControl1.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl1.Location = new System.Drawing.Point(5, 62);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(54, 13);
			this.labelControl1.TabIndex = 33;
			this.labelControl1.Text = "Databases:";
			// 
			// groupControl3
			// 
			this.groupControl3.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.groupControl3.AppearanceCaption.Options.UseFont = true;
			this.groupControl3.Controls.Add(this.HelpButton);
			this.groupControl3.Controls.Add(this.labelControl1);
			this.groupControl3.Controls.Add(this.DatabaseComboBox);
			this.groupControl3.Controls.Add(this.DistanceRange);
			this.groupControl3.Controls.Add(this.DistanceRangeLabel);
			this.groupControl3.Controls.Add(this.MaxHits_Label);
			this.groupControl3.Controls.Add(this.MaxHits);
			this.groupControl3.Controls.Add(this.labelControl5);
			this.groupControl3.Location = new System.Drawing.Point(0, 1);
			this.groupControl3.Name = "groupControl3";
			this.groupControl3.Size = new System.Drawing.Size(383, 88);
			this.groupControl3.TabIndex = 130;
			this.groupControl3.Text = "SmallWorld Search Options";
			// 
			// HelpButton
			// 
			this.HelpButton.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.HelpButton.Appearance.Options.UseBackColor = true;
			this.HelpButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.HelpButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("HelpButton.ImageOptions.Image")));
			this.HelpButton.Location = new System.Drawing.Point(358, 3);
			this.HelpButton.Name = "HelpButton";
			this.HelpButton.Size = new System.Drawing.Size(18, 14);
			superToolTip1.AllowHtmlText = DevExpress.Utils.DefaultBoolean.True;
			toolTipItem1.Text = resources.GetString("toolTipItem1.Text");
			superToolTip1.Items.Add(toolTipItem1);
			superToolTip1.MaxWidth = 400;
			this.HelpButton.SuperTip = superToolTip1;
			this.HelpButton.TabIndex = 128;
			this.HelpButton.ToolTipController = this.ToolTipController;
			this.HelpButton.Click += new System.EventHandler(this.HelpButton_Click);
			// 
			// CriteriaStructureOptionsSW
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.groupControl3);
			this.Controls.Add(this.AdvancedOptionsGroupControl);
			this.Name = "CriteriaStructureOptionsSW";
			this.Size = new System.Drawing.Size(391, 351);
			((System.ComponentModel.ISupportInitialize)(this.PresetsComboBox.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MatchAtomTypes.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AdvancedOptionsGroupControl)).EndInit();
			this.AdvancedOptionsGroupControl.ResumeLayout(false);
			this.AdvancedOptionsScrollableControl.ResumeLayout(false);
			this.AdvancedOptionsScrollableControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AlignStructs.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ShowColors.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MaxHits.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DistanceRange.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DistanceRange)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DatabaseComboBox.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.groupControl3)).EndInit();
			this.groupControl3.ResumeLayout(false);
			this.groupControl3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private DevExpress.XtraEditors.ComboBoxEdit PresetsComboBox;
		private CriteriaStructureRangeCtl TerminalRangeUp;
		private CriteriaStructureRangeCtl RingRangeUp;
		private CriteriaStructureRangeCtl LinkerRangeUp;
		private CriteriaStructureRangeCtl MutationRangeMinor;
		private CriteriaStructureRangeCtl SubstitutionRange;
		private CriteriaStructureRangeCtl HybridizationRange;
		public DevExpress.XtraEditors.CheckEdit MatchAtomTypes;
		private DevExpress.Utils.ToolTipController ToolTipController;
		private DevExpress.XtraEditors.GroupControl AdvancedOptionsGroupControl;
		private System.Windows.Forms.ImageList Bitmaps8x8;
		public DevExpress.XtraEditors.LabelControl labelControl5;
		public DevExpress.XtraEditors.TextEdit MaxHits;
		public DevExpress.XtraEditors.LabelControl MaxHits_Label;
		public DevExpress.XtraEditors.LabelControl DistanceRangeLabel;
		public DevExpress.XtraEditors.RangeTrackBarControl DistanceRange;
		private DevExpress.XtraEditors.ComboBoxEdit DatabaseComboBox;
		public DevExpress.XtraEditors.LabelControl labelControl1;
		private DevExpress.XtraEditors.GroupControl groupControl3;
		public DevExpress.XtraEditors.LabelControl labelControl2;
		public DevExpress.XtraEditors.LabelControl labelControl3;
		public DevExpress.XtraEditors.SimpleButton SavePreferredSettingsButton;
		private DevExpress.XtraEditors.SimpleButton HelpButton;
		public DevExpress.XtraEditors.LabelControl labelControl6;
		public DevExpress.XtraEditors.LabelControl labelControl11;
		private CriteriaStructureRangeCtl MutationRangeMajor;
		private CriteriaStructureRangeCtl LinkerRangeDown;
		private CriteriaStructureRangeCtl RingRangeDown;
		private CriteriaStructureRangeCtl TerminalRangeDown;
		public DevExpress.XtraEditors.LabelControl labelControl10;
		public DevExpress.XtraEditors.LabelControl labelControl9;
		public DevExpress.XtraEditors.LabelControl labelControl8;
		public DevExpress.XtraEditors.LabelControl labelControl7;
		public DevExpress.XtraEditors.CheckEdit AlignStructs;
		public DevExpress.XtraEditors.CheckEdit ShowColors;
		private DevExpress.XtraEditors.SimpleButton Identical;
		public DevExpress.XtraEditors.LabelControl labelControl4;
		private DevExpress.XtraEditors.XtraScrollableControl AdvancedOptionsScrollableControl;
	}
}
