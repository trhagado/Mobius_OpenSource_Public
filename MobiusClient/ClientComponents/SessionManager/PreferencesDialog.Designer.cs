namespace Mobius.ClientComponents
{
	partial class PreferencesDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreferencesDialog));
			this.BrowseFolders = new DevExpress.XtraEditors.SimpleButton();
			this.BrowseProjects = new DevExpress.XtraEditors.SimpleButton();
			this.DefaultFolder = new DevExpress.XtraEditors.TextEdit();
			this.PreferredProject = new DevExpress.XtraEditors.TextEdit();
			this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
			this.groupControl3 = new DevExpress.XtraEditors.GroupControl();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.groupControl2 = new DevExpress.XtraEditors.GroupControl();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.DefaultLookAndFeel = new DevExpress.LookAndFeel.DefaultLookAndFeel();
			this.LookAndFeelOption = new DevExpress.XtraEditors.ImageComboBoxEdit();
			this.SkinImages16x16 = new System.Windows.Forms.ImageList();
			this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
			this.GraphicsColumnZoom = new Mobius.ClientComponents.ZoomControl();
			this.TableColumnZoom = new Mobius.ClientComponents.ZoomControl();
			this.ScrollGridByPixel = new DevExpress.XtraEditors.CheckEdit();
			this.ScrollGridByRow = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
			this.RestoreWindowsAtStartup = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl9 = new DevExpress.XtraEditors.LabelControl();
			this.FindRelatedCpdsInQuickSearch = new DevExpress.XtraEditors.CheckEdit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultFolder.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PreferredProject.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.groupControl3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.groupControl2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LookAndFeelOption.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ScrollGridByPixel.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ScrollGridByRow.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RestoreWindowsAtStartup.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FindRelatedCpdsInQuickSearch.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// BrowseFolders
			// 
			this.BrowseFolders.Location = new System.Drawing.Point(308, 93);
			this.BrowseFolders.Name = "BrowseFolders";
			this.BrowseFolders.Size = new System.Drawing.Size(69, 24);
			this.BrowseFolders.TabIndex = 36;
			this.BrowseFolders.Text = "Change...";
			this.BrowseFolders.Click += new System.EventHandler(this.BrowseFolders_Click);
			// 
			// BrowseProjects
			// 
			this.BrowseProjects.Location = new System.Drawing.Point(308, 34);
			this.BrowseProjects.Name = "BrowseProjects";
			this.BrowseProjects.Size = new System.Drawing.Size(69, 24);
			this.BrowseProjects.TabIndex = 35;
			this.BrowseProjects.Text = "Change...";
			this.BrowseProjects.Click += new System.EventHandler(this.BrowseProjects_Click);
			// 
			// DefaultFolder
			// 
			this.DefaultFolder.EditValue = "";
			this.DefaultFolder.Location = new System.Drawing.Point(15, 95);
			this.DefaultFolder.Name = "DefaultFolder";
			this.DefaultFolder.Size = new System.Drawing.Size(283, 20);
			this.DefaultFolder.TabIndex = 34;
			// 
			// PreferredProject
			// 
			this.PreferredProject.AllowDrop = true;
			this.PreferredProject.Location = new System.Drawing.Point(15, 36);
			this.PreferredProject.Name = "PreferredProject";
			this.PreferredProject.Size = new System.Drawing.Size(282, 20);
			this.PreferredProject.TabIndex = 33;
			// 
			// labelControl3
			// 
			this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl3.LineLocation = DevExpress.XtraEditors.LineLocation.Center;
			this.labelControl3.LineVisible = true;
			this.labelControl3.Location = new System.Drawing.Point(7, 17);
			this.labelControl3.Name = "labelControl3";
			this.labelControl3.Size = new System.Drawing.Size(370, 13);
			this.labelControl3.TabIndex = 32;
			this.labelControl3.Text = " My preferred project";
			// 
			// groupControl3
			// 
			this.groupControl3.Location = new System.Drawing.Point(4, 23);
			this.groupControl3.LookAndFeel.SkinName = "Blue";
			this.groupControl3.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Style3D;
			this.groupControl3.LookAndFeel.UseDefaultLookAndFeel = false;
			this.groupControl3.LookAndFeel.UseWindowsXPTheme = true;
			this.groupControl3.Name = "groupControl3";
			this.groupControl3.Size = new System.Drawing.Size(368, 2);
			this.groupControl3.TabIndex = 31;
			// 
			// labelControl2
			// 
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.LineVisible = true;
			this.labelControl2.Location = new System.Drawing.Point(7, 73);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(370, 16);
			this.labelControl2.TabIndex = 30;
			this.labelControl2.Text = " Default folder for importing and exporting files";
			// 
			// groupControl2
			// 
			this.groupControl2.Location = new System.Drawing.Point(3, 82);
			this.groupControl2.LookAndFeel.SkinName = "Blue";
			this.groupControl2.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Style3D;
			this.groupControl2.LookAndFeel.UseDefaultLookAndFeel = false;
			this.groupControl2.LookAndFeel.UseWindowsXPTheme = true;
			this.groupControl2.Name = "groupControl2";
			this.groupControl2.Size = new System.Drawing.Size(368, 2);
			this.groupControl2.TabIndex = 29;
			// 
			// Cancel
			// 
			this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = new System.Drawing.Point(314, 392);
			this.Cancel.Name = "Cancel";
			this.Cancel.Size = new System.Drawing.Size(69, 24);
			this.Cancel.TabIndex = 28;
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.Location = new System.Drawing.Point(235, 392);
			this.OK.Name = "OK";
			this.OK.Size = new System.Drawing.Size(69, 24);
			this.OK.TabIndex = 27;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// labelControl1
			// 
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(4, 218);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(370, 16);
			this.labelControl1.TabIndex = 26;
			this.labelControl1.Text = " Interface look and feel ";
			// 
			// DefaultLookAndFeel
			// 
			this.DefaultLookAndFeel.LookAndFeel.SkinName = "Office 2010 Blue";
			// 
			// LookAndFeelOption
			// 
			this.LookAndFeelOption.Location = new System.Drawing.Point(16, 240);
			this.LookAndFeelOption.Name = "LookAndFeelOption";
			this.LookAndFeelOption.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.LookAndFeelOption.Properties.SmallImages = this.SkinImages16x16;
			this.LookAndFeelOption.Properties.Sorted = true;
			this.LookAndFeelOption.Size = new System.Drawing.Size(282, 20);
			this.LookAndFeelOption.TabIndex = 37;
			this.LookAndFeelOption.SelectedIndexChanged += new System.EventHandler(this.LookAndFeelOption_SelectedIndexChanged);
			// 
			// SkinImages16x16
			// 
			this.SkinImages16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("SkinImages16x16.ImageStream")));
			this.SkinImages16x16.TransparentColor = System.Drawing.Color.Transparent;
			this.SkinImages16x16.Images.SetKeyName(0, "Skin-Black.bmp");
			this.SkinImages16x16.Images.SetKeyName(1, "Skin-Blue.bmp");
			this.SkinImages16x16.Images.SetKeyName(2, "Skin-Blueprint.bmp");
			this.SkinImages16x16.Images.SetKeyName(3, "Skin-Caramel.bmp");
			this.SkinImages16x16.Images.SetKeyName(4, "Skin-Coffee.bmp");
			this.SkinImages16x16.Images.SetKeyName(5, "Skin-Dark-Side.bmp");
			this.SkinImages16x16.Images.SetKeyName(6, "Skin-Darkroom.bmp");
			this.SkinImages16x16.Images.SetKeyName(7, "Skin-DevExpress-2010.bmp");
			this.SkinImages16x16.Images.SetKeyName(8, "Skin-Foggy.bmp");
			this.SkinImages16x16.Images.SetKeyName(9, "Skin-Glass-Oceans.bmp");
			this.SkinImages16x16.Images.SetKeyName(10, "Skin-High-Contrast.bmp");
			this.SkinImages16x16.Images.SetKeyName(11, "Skin-iMaginary.bmp");
			this.SkinImages16x16.Images.SetKeyName(12, "Skin-Lilian.bmp");
			this.SkinImages16x16.Images.SetKeyName(13, "Skin-Liquid-Sky.bmp");
			this.SkinImages16x16.Images.SetKeyName(14, "Skin-London-Liquid-Sky.bmp");
			this.SkinImages16x16.Images.SetKeyName(15, "Skin-McSkin.bmp");
			this.SkinImages16x16.Images.SetKeyName(16, "Skin-Metro.bmp");
			this.SkinImages16x16.Images.SetKeyName(17, "Skin-Money-Twins.bmp");
			this.SkinImages16x16.Images.SetKeyName(18, "Skin-Office-2007-Black.bmp");
			this.SkinImages16x16.Images.SetKeyName(19, "Skin-Office-2007-Blue.bmp");
			this.SkinImages16x16.Images.SetKeyName(20, "Skin-Office-2007-Green.bmp");
			this.SkinImages16x16.Images.SetKeyName(21, "Skin-Office-2007-Pink.bmp");
			this.SkinImages16x16.Images.SetKeyName(22, "Skin-Office-2007-Silver.bmp");
			this.SkinImages16x16.Images.SetKeyName(23, "Skin-Office-2010-Black.bmp");
			this.SkinImages16x16.Images.SetKeyName(24, "Skin-Office-2010-Blue.bmp");
			this.SkinImages16x16.Images.SetKeyName(25, "Skin-Office-2010-Silver.bmp");
			this.SkinImages16x16.Images.SetKeyName(26, "Skin-Pumpkin.bmp");
			this.SkinImages16x16.Images.SetKeyName(27, "Skin-Sharp.bmp");
			this.SkinImages16x16.Images.SetKeyName(28, "Skin-Sharp-Plus.bmp");
			this.SkinImages16x16.Images.SetKeyName(29, "Skin-Springtime.bmp");
			this.SkinImages16x16.Images.SetKeyName(30, "Skin-Stardust.bmp");
			this.SkinImages16x16.Images.SetKeyName(31, "Skin-Summer.bmp");
			this.SkinImages16x16.Images.SetKeyName(32, "Skin-The-Asphalt-World.bmp");
			this.SkinImages16x16.Images.SetKeyName(33, "Skin-Valentine.bmp");
			this.SkinImages16x16.Images.SetKeyName(34, "Skin-VS10.bmp");
			this.SkinImages16x16.Images.SetKeyName(35, "Skin-Whiteprint.bmp");
			this.SkinImages16x16.Images.SetKeyName(36, "Skin-Seven.bmp");
			this.SkinImages16x16.Images.SetKeyName(37, "Skin-Seven-Classic.bmp");
			this.SkinImages16x16.Images.SetKeyName(38, "Skin-Windows Classic.bmp");
			this.SkinImages16x16.Images.SetKeyName(39, "Skin-Xmas.bmp");
			// 
			// labelControl7
			// 
			this.labelControl7.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl7.LineVisible = true;
			this.labelControl7.Location = new System.Drawing.Point(5, 264);
			this.labelControl7.Name = "labelControl7";
			this.labelControl7.Size = new System.Drawing.Size(370, 11);
			this.labelControl7.TabIndex = 43;
			// 
			// labelControl4
			// 
			this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl4.LineOrientation = DevExpress.XtraEditors.LabelLineOrientation.Horizontal;
			this.labelControl4.LineVisible = true;
			this.labelControl4.Location = new System.Drawing.Point(7, 132);
			this.labelControl4.Name = "labelControl4";
			this.labelControl4.Size = new System.Drawing.Size(370, 17);
			this.labelControl4.TabIndex = 216;
			this.labelControl4.Text = "Results grid";
			// 
			// labelControl5
			// 
			this.labelControl5.Location = new System.Drawing.Point(22, 186);
			this.labelControl5.Name = "labelControl5";
			this.labelControl5.Size = new System.Drawing.Size(141, 13);
			this.labelControl5.TabIndex = 218;
			this.labelControl5.Text = "Extra structure column zoom:";
			// 
			// labelControl6
			// 
			this.labelControl6.Location = new System.Drawing.Point(22, 159);
			this.labelControl6.Name = "labelControl6";
			this.labelControl6.Size = new System.Drawing.Size(138, 13);
			this.labelControl6.TabIndex = 219;
			this.labelControl6.Text = "Column width zoom: . . . . . .";
			// 
			// GraphicsColumnZoom
			// 
			this.GraphicsColumnZoom.Location = new System.Drawing.Point(174, 182);
			this.GraphicsColumnZoom.Name = "GraphicsColumnZoom";
			this.GraphicsColumnZoom.Size = new System.Drawing.Size(197, 25);
			this.GraphicsColumnZoom.TabIndex = 220;
			this.GraphicsColumnZoom.ZoomPct = 100;
			this.GraphicsColumnZoom.EditValueChanged += new System.EventHandler(this.GraphicsColumnZoom_EditValueChanged);
			// 
			// TableColumnZoom
			// 
			this.TableColumnZoom.Location = new System.Drawing.Point(174, 153);
			this.TableColumnZoom.Name = "TableColumnZoom";
			this.TableColumnZoom.Size = new System.Drawing.Size(197, 25);
			this.TableColumnZoom.TabIndex = 221;
			this.TableColumnZoom.ZoomPct = 100;
			this.TableColumnZoom.EditValueChanged += new System.EventHandler(this.TableColumnZoom_EditValueChanged);
			// 
			// ScrollGridByPixel
			// 
			this.ScrollGridByPixel.Cursor = System.Windows.Forms.Cursors.Default;
			this.ScrollGridByPixel.Location = new System.Drawing.Point(471, 240);
			this.ScrollGridByPixel.Name = "ScrollGridByPixel";
			this.ScrollGridByPixel.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ScrollGridByPixel.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(57)))), ((int)(((byte)(91)))));
			this.ScrollGridByPixel.Properties.Appearance.Options.UseBackColor = true;
			this.ScrollGridByPixel.Properties.Appearance.Options.UseForeColor = true;
			this.ScrollGridByPixel.Properties.Caption = "By pixel";
			this.ScrollGridByPixel.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ScrollGridByPixel.Properties.RadioGroupIndex = 2;
			this.ScrollGridByPixel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ScrollGridByPixel.Size = new System.Drawing.Size(64, 19);
			this.ScrollGridByPixel.TabIndex = 227;
			this.ScrollGridByPixel.TabStop = false;
			this.ScrollGridByPixel.Visible = false;
			// 
			// ScrollGridByRow
			// 
			this.ScrollGridByRow.Cursor = System.Windows.Forms.Cursors.Default;
			this.ScrollGridByRow.EditValue = true;
			this.ScrollGridByRow.Location = new System.Drawing.Point(401, 240);
			this.ScrollGridByRow.Name = "ScrollGridByRow";
			this.ScrollGridByRow.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ScrollGridByRow.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(57)))), ((int)(((byte)(91)))));
			this.ScrollGridByRow.Properties.Appearance.Options.UseBackColor = true;
			this.ScrollGridByRow.Properties.Appearance.Options.UseForeColor = true;
			this.ScrollGridByRow.Properties.Caption = "By row";
			this.ScrollGridByRow.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ScrollGridByRow.Properties.RadioGroupIndex = 2;
			this.ScrollGridByRow.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ScrollGridByRow.Size = new System.Drawing.Size(64, 19);
			this.ScrollGridByRow.TabIndex = 226;
			this.ScrollGridByRow.Visible = false;
			// 
			// labelControl8
			// 
			this.labelControl8.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Horizontal;
			this.labelControl8.LineOrientation = DevExpress.XtraEditors.LabelLineOrientation.Horizontal;
			this.labelControl8.Location = new System.Drawing.Point(350, 243);
			this.labelControl8.Name = "labelControl8";
			this.labelControl8.Size = new System.Drawing.Size(43, 13);
			this.labelControl8.TabIndex = 225;
			this.labelControl8.Text = "Scrolling:";
			this.labelControl8.Visible = false;
			// 
			// RestoreWindowsAtStartup
			// 
			this.RestoreWindowsAtStartup.Location = new System.Drawing.Point(16, 306);
			this.RestoreWindowsAtStartup.Name = "RestoreWindowsAtStartup";
			this.RestoreWindowsAtStartup.Properties.Caption = "Maintain open queries between sessions";
			this.RestoreWindowsAtStartup.Size = new System.Drawing.Size(232, 19);
			this.RestoreWindowsAtStartup.TabIndex = 228;
			// 
			// labelControl9
			// 
			this.labelControl9.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl9.LineVisible = true;
			this.labelControl9.Location = new System.Drawing.Point(7, 331);
			this.labelControl9.Name = "labelControl9";
			this.labelControl9.Size = new System.Drawing.Size(370, 11);
			this.labelControl9.TabIndex = 229;
			// 
			// FindRelatedCpdsInQuickSearch
			// 
			this.FindRelatedCpdsInQuickSearch.Location = new System.Drawing.Point(16, 281);
			this.FindRelatedCpdsInQuickSearch.Name = "FindRelatedCpdsInQuickSearch";
			this.FindRelatedCpdsInQuickSearch.Properties.Caption = "Find related compounds in Quick Search";
			this.FindRelatedCpdsInQuickSearch.Size = new System.Drawing.Size(232, 19);
			this.FindRelatedCpdsInQuickSearch.TabIndex = 230;
			// 
			// PreferencesDialog
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(388, 421);
			this.Controls.Add(this.FindRelatedCpdsInQuickSearch);
			this.Controls.Add(this.labelControl9);
			this.Controls.Add(this.RestoreWindowsAtStartup);
			this.Controls.Add(this.ScrollGridByPixel);
			this.Controls.Add(this.ScrollGridByRow);
			this.Controls.Add(this.labelControl8);
			this.Controls.Add(this.labelControl5);
			this.Controls.Add(this.labelControl6);
			this.Controls.Add(this.GraphicsColumnZoom);
			this.Controls.Add(this.labelControl4);
			this.Controls.Add(this.labelControl7);
			this.Controls.Add(this.labelControl3);
			this.Controls.Add(this.BrowseFolders);
			this.Controls.Add(this.BrowseProjects);
			this.Controls.Add(this.DefaultFolder);
			this.Controls.Add(this.PreferredProject);
			this.Controls.Add(this.groupControl3);
			this.Controls.Add(this.labelControl2);
			this.Controls.Add(this.groupControl2);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.LookAndFeelOption);
			this.Controls.Add(this.TableColumnZoom);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PreferencesDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Preferences";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PreferencesDialog2_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.DefaultFolder.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PreferredProject.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.groupControl3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.groupControl2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LookAndFeelOption.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ScrollGridByPixel.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ScrollGridByRow.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RestoreWindowsAtStartup.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FindRelatedCpdsInQuickSearch.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DevExpress.XtraEditors.SimpleButton BrowseFolders;
		private DevExpress.XtraEditors.SimpleButton BrowseProjects;
		private DevExpress.XtraEditors.TextEdit DefaultFolder;
		private DevExpress.XtraEditors.TextEdit PreferredProject;
		private DevExpress.XtraEditors.LabelControl labelControl3;
		private DevExpress.XtraEditors.GroupControl groupControl3;
		private DevExpress.XtraEditors.LabelControl labelControl2;
		private DevExpress.XtraEditors.GroupControl groupControl2;
		private DevExpress.XtraEditors.SimpleButton Cancel;
		private DevExpress.XtraEditors.SimpleButton OK;
		private DevExpress.XtraEditors.LabelControl labelControl1;
		private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog;
		public DevExpress.LookAndFeel.DefaultLookAndFeel DefaultLookAndFeel;
		private DevExpress.XtraEditors.ImageComboBoxEdit LookAndFeelOption;
		private System.Windows.Forms.ImageList SkinImages16x16;
		private DevExpress.XtraEditors.LabelControl labelControl7;
		public DevExpress.XtraEditors.LabelControl labelControl4;
		private DevExpress.XtraEditors.LabelControl labelControl5;
		private ZoomControl TableColumnZoom;
		private DevExpress.XtraEditors.LabelControl labelControl6;
		private ZoomControl GraphicsColumnZoom;
		public DevExpress.XtraEditors.CheckEdit ScrollGridByPixel;
		public DevExpress.XtraEditors.CheckEdit ScrollGridByRow;
		public DevExpress.XtraEditors.LabelControl labelControl8;
		public DevExpress.XtraEditors.CheckEdit RestoreWindowsAtStartup;
		private DevExpress.XtraEditors.LabelControl labelControl9;
		public DevExpress.XtraEditors.CheckEdit FindRelatedCpdsInQuickSearch;
	}
}