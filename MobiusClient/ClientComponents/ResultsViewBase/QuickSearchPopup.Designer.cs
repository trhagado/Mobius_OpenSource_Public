namespace Mobius.ClientComponents
{
	partial class QuickSearchPopup
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
			DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions imageListBoxItemImageOptions1 = new DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions();
			DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions imageListBoxItemImageOptions2 = new DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions();
			DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions imageListBoxItemImageOptions3 = new DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions();
			DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions imageListBoxItemImageOptions4 = new DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions();
			DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions imageListBoxItemImageOptions5 = new DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions();
			DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions imageListBoxItemImageOptions6 = new DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions();
			DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions imageListBoxItemImageOptions7 = new DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions();
			DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions imageListBoxItemImageOptions8 = new DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions();
			DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions imageListBoxItemImageOptions9 = new DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions();
			DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions imageListBoxItemImageOptions10 = new DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions();
			DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions imageListBoxItemImageOptions11 = new DevExpress.XtraEditors.Controls.ImageListBoxItemImageOptions();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickSearchPopup));
			DevExpress.XtraEditors.ButtonsPanelControl.ButtonImageOptions buttonImageOptions1 = new DevExpress.XtraEditors.ButtonsPanelControl.ButtonImageOptions();
			this.OtherCidsList = new DevExpress.XtraEditors.ComboBoxEdit();
			this.Timer = new System.Windows.Forms.Timer(this.components);
			this.ListControl = new DevExpress.XtraEditors.ImageListBoxControl();
			this.ShowAllDataLabel = new DevExpress.XtraEditors.LabelControl();
			this.AllDataButtonStb = new DevExpress.XtraEditors.SimpleButton();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.AllDataButton = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.StructurePanel = new DevExpress.XtraEditors.GroupControl();
			this.RelatedDataButton = new DevExpress.XtraEditors.SimpleButton();
			this.VertDivider = new DevExpress.XtraEditors.LabelControl();
			this.CloseButton = new DevExpress.XtraEditors.SimpleButton();
			this.RelatedStructuresControl = new Mobius.ClientComponents.RelatedStructuresControl();
			((System.ComponentModel.ISupportInitialize)(this.OtherCidsList.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ListControl)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StructurePanel)).BeginInit();
			this.StructurePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// OtherCidsList
			// 
			this.OtherCidsList.EditValue = "100 Other Matches";
			this.OtherCidsList.Location = new System.Drawing.Point(485, 575);
			this.OtherCidsList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.OtherCidsList.Name = "OtherCidsList";
			this.OtherCidsList.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.OtherCidsList.Size = new System.Drawing.Size(148, 22);
			this.OtherCidsList.TabIndex = 207;
			this.OtherCidsList.Visible = false;
			this.OtherCidsList.SelectedIndexChanged += new System.EventHandler(this.OtherCidsList_SelectedIndexChanged);
			this.OtherCidsList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OtherCidsList_KeyDown);
			// 
			// Timer
			// 
			this.Timer.Tick += new System.EventHandler(this.QuickSearchPopupTimer_Tick);
			// 
			// ListControl
			// 
			this.ListControl.HotTrackItems = true;
			this.ListControl.ItemHeight = 18;
			this.ListControl.Items.AddRange(new DevExpress.XtraEditors.Controls.ImageListBoxItem[] {
            new DevExpress.XtraEditors.Controls.ImageListBoxItem(null, "", imageListBoxItemImageOptions1, null),
            new DevExpress.XtraEditors.Controls.ImageListBoxItem(null, "", imageListBoxItemImageOptions2, null),
            new DevExpress.XtraEditors.Controls.ImageListBoxItem(null, "", imageListBoxItemImageOptions3, null),
            new DevExpress.XtraEditors.Controls.ImageListBoxItem(null, "", imageListBoxItemImageOptions4, null),
            new DevExpress.XtraEditors.Controls.ImageListBoxItem(null, "", imageListBoxItemImageOptions5, null),
            new DevExpress.XtraEditors.Controls.ImageListBoxItem(null, "", imageListBoxItemImageOptions6, null),
            new DevExpress.XtraEditors.Controls.ImageListBoxItem(null, "", imageListBoxItemImageOptions7, null),
            new DevExpress.XtraEditors.Controls.ImageListBoxItem(null, "", imageListBoxItemImageOptions8, null),
            new DevExpress.XtraEditors.Controls.ImageListBoxItem(null, "", imageListBoxItemImageOptions9, null),
            new DevExpress.XtraEditors.Controls.ImageListBoxItem(null, "", imageListBoxItemImageOptions10, null),
            new DevExpress.XtraEditors.Controls.ImageListBoxItem(null, "", imageListBoxItemImageOptions11, null)});
			this.ListControl.Location = new System.Drawing.Point(0, 537);
			this.ListControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ListControl.Name = "ListControl";
			this.ListControl.Size = new System.Drawing.Size(455, 250);
			this.ListControl.TabIndex = 209;
			this.ListControl.Visible = false;
			this.ListControl.SelectedIndexChanged += new System.EventHandler(this.ListControl_SelectedIndexChanged);
			this.ListControl.Click += new System.EventHandler(this.ListControl_Click);
			this.ListControl.Enter += new System.EventHandler(this.ListControl_Enter);
			this.ListControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListControl_KeyDown);
			this.ListControl.Leave += new System.EventHandler(this.ListControl_Leave);
			this.ListControl.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ListControl_MouseClick);
			this.ListControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListControl_MouseDown);
			this.ListControl.MouseEnter += new System.EventHandler(this.ListControl_MouseEnter);
			this.ListControl.MouseLeave += new System.EventHandler(this.ListControl_MouseLeave);
			this.ListControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ListControl_MouseUp);
			// 
			// ShowAllDataLabel
			// 
			this.ShowAllDataLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ShowAllDataLabel.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ShowAllDataLabel.Appearance.Options.UseBackColor = true;
			this.ShowAllDataLabel.Location = new System.Drawing.Point(499, 7);
			this.ShowAllDataLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ShowAllDataLabel.Name = "ShowAllDataLabel";
			this.ShowAllDataLabel.Size = new System.Drawing.Size(90, 16);
			this.ShowAllDataLabel.TabIndex = 211;
			this.ShowAllDataLabel.Text = "Additional data:";
			// 
			// AllDataButtonStb
			// 
			this.AllDataButtonStb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AllDataButtonStb.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.AllDataButtonStb.ImageOptions.ImageIndex = 1;
			this.AllDataButtonStb.ImageOptions.ImageList = this.Bitmaps16x16;
			this.AllDataButtonStb.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.AllDataButtonStb.Location = new System.Drawing.Point(656, 2);
			this.AllDataButtonStb.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.AllDataButtonStb.Name = "AllDataButtonStb";
			this.AllDataButtonStb.Size = new System.Drawing.Size(23, 25);
			this.AllDataButtonStb.TabIndex = 210;
			this.AllDataButtonStb.ToolTip = "Spotfire view of data (Spill the Beans)";
			this.AllDataButtonStb.Click += new System.EventHandler(this.AllDataButtonMdbAssay_Click);
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "TableDataMultiple.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "Spotfire.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "TargetSummaryDendogram.bmp");
			this.Bitmaps16x16.Images.SetKeyName(3, "CloseWindow.bmp");
			// 
			// AllDataButton
			// 
			this.AllDataButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AllDataButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.AllDataButton.ImageOptions.ImageIndex = 0;
			this.AllDataButton.ImageOptions.ImageList = this.Bitmaps16x16;
			this.AllDataButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.AllDataButton.Location = new System.Drawing.Point(630, 2);
			this.AllDataButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.AllDataButton.Name = "AllDataButton";
			this.AllDataButton.Size = new System.Drawing.Size(23, 25);
			this.AllDataButton.TabIndex = 209;
			this.AllDataButton.ToolTip = "Table view of all data";
			this.AllDataButton.Click += new System.EventHandler(this.AllDataButton_Click);
			// 
			// labelControl1
			// 
			this.labelControl1.Location = new System.Drawing.Point(22, 794);
			this.labelControl1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(321, 16);
			this.labelControl1.TabIndex = 211;
			this.labelControl1.Text = "This control displays either StructurePanel or ListControl";
			this.labelControl1.Visible = false;
			// 
			// StructurePanel
			// 
			this.StructurePanel.AppearanceCaption.Options.UseTextOptions = true;
			this.StructurePanel.AppearanceCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.StructurePanel.Controls.Add(this.RelatedDataButton);
			this.StructurePanel.Controls.Add(this.VertDivider);
			this.StructurePanel.Controls.Add(this.CloseButton);
			this.StructurePanel.Controls.Add(this.ShowAllDataLabel);
			this.StructurePanel.Controls.Add(this.RelatedStructuresControl);
			this.StructurePanel.Controls.Add(this.AllDataButtonStb);
			this.StructurePanel.Controls.Add(this.AllDataButton);
			this.StructurePanel.CustomHeaderButtons.AddRange(new DevExpress.XtraEditors.ButtonPanel.IBaseButton[] {
            new DevExpress.XtraEditors.ButtonsPanelControl.GroupBoxButton(" ", true, buttonImageOptions1, DevExpress.XtraBars.Docking2010.ButtonStyle.PushButton, "", -1, true, null, true, false, true, null, -1)});
			this.StructurePanel.CustomHeaderButtonsLocation = DevExpress.Utils.GroupElementLocation.AfterText;
			this.StructurePanel.Location = new System.Drawing.Point(0, 0);
			this.StructurePanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.StructurePanel.Name = "StructurePanel";
			this.StructurePanel.Size = new System.Drawing.Size(728, 516);
			this.StructurePanel.TabIndex = 212;
			this.StructurePanel.Text = "Structure and Related Structures";
			// 
			// RelatedDataButton
			// 
			this.RelatedDataButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RelatedDataButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.RelatedDataButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("RelatedDataButton.ImageOptions.Image")));
			this.RelatedDataButton.ImageOptions.ImageIndex = 0;
			this.RelatedDataButton.ImageOptions.ImageList = this.Bitmaps16x16;
			this.RelatedDataButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.RelatedDataButton.Location = new System.Drawing.Point(603, 2);
			this.RelatedDataButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.RelatedDataButton.Name = "RelatedDataButton";
			this.RelatedDataButton.Size = new System.Drawing.Size(23, 25);
			this.RelatedDataButton.TabIndex = 215;
			this.RelatedDataButton.ToolTip = "Retrieve Additional Related Compound Data";
			this.RelatedDataButton.Click += new System.EventHandler(this.RelatedDataButton_Click);
			// 
			// VertDivider
			// 
			this.VertDivider.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.VertDivider.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.VertDivider.Appearance.Options.UseBackColor = true;
			this.VertDivider.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.VertDivider.LineOrientation = DevExpress.XtraEditors.LabelLineOrientation.Vertical;
			this.VertDivider.LineVisible = true;
			this.VertDivider.Location = new System.Drawing.Point(687, 2);
			this.VertDivider.Margin = new System.Windows.Forms.Padding(0);
			this.VertDivider.Name = "VertDivider";
			this.VertDivider.Size = new System.Drawing.Size(7, 25);
			this.VertDivider.TabIndex = 214;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.CloseButton.ImageOptions.ImageIndex = 3;
			this.CloseButton.ImageOptions.ImageList = this.Bitmaps16x16;
			this.CloseButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.CloseButton.Location = new System.Drawing.Point(699, 4);
			this.CloseButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(21, 22);
			this.CloseButton.TabIndex = 213;
			this.CloseButton.ToolTip = "Close";
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// RelatedStructuresControl
			// 
			this.RelatedStructuresControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RelatedStructuresControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.RelatedStructuresControl.Location = new System.Drawing.Point(7, 36);
			this.RelatedStructuresControl.Margin = new System.Windows.Forms.Padding(5);
			this.RelatedStructuresControl.Name = "RelatedStructuresControl";
			this.RelatedStructuresControl.Size = new System.Drawing.Size(714, 476);
			this.RelatedStructuresControl.TabIndex = 212;
			// 
			// QuickSearchPopup
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.ListControl);
			this.Controls.Add(this.OtherCidsList);
			this.Controls.Add(this.StructurePanel);
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "QuickSearchPopup";
			this.Size = new System.Drawing.Size(768, 815);
			((System.ComponentModel.ISupportInitialize)(this.OtherCidsList.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ListControl)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StructurePanel)).EndInit();
			this.StructurePanel.ResumeLayout(false);
			this.StructurePanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public DevExpress.XtraEditors.ComboBoxEdit OtherCidsList;
		private System.Windows.Forms.Timer Timer;
		private DevExpress.XtraEditors.ImageListBoxControl ListControl;
		private DevExpress.XtraEditors.LabelControl labelControl1;
		private DevExpress.XtraEditors.SimpleButton AllDataButton;
		public System.Windows.Forms.ImageList Bitmaps16x16;
		private DevExpress.XtraEditors.LabelControl ShowAllDataLabel;
		private DevExpress.XtraEditors.SimpleButton AllDataButtonStb;
		private RelatedStructuresControl RelatedStructuresControl;
		private DevExpress.XtraEditors.GroupControl StructurePanel;
		private DevExpress.XtraEditors.SimpleButton CloseButton;
		private DevExpress.XtraEditors.LabelControl VertDivider;
		private DevExpress.XtraEditors.SimpleButton RelatedDataButton;
	}
}
