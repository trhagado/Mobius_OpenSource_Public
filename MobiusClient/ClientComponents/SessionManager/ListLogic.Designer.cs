namespace Mobius.ClientComponents
{
	partial class ListLogic
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ListLogic));
			this.LeftPanel = new DevExpress.XtraEditors.PanelControl();
			this.ListTree1 = new Mobius.ClientComponents.ContentsTreeControl();
			this.ListCaption1 = new DevExpress.XtraEditors.LabelControl();
			this.RightPanel = new DevExpress.XtraEditors.PanelControl();
			this.ListTree2 = new Mobius.ClientComponents.ContentsTreeControl();
			this.ListCaption2 = new DevExpress.XtraEditors.LabelControl();
			this.CenterPanel = new DevExpress.XtraEditors.PanelControl();
			this.ListNotButton = new DevExpress.XtraEditors.SimpleButton();
			this.ImageList = new System.Windows.Forms.ImageList(this.components);
			this.ListOrButton = new DevExpress.XtraEditors.SimpleButton();
			this.ListAndButton = new DevExpress.XtraEditors.SimpleButton();
			this.ListAnd = new DevExpress.XtraEditors.CheckEdit();
			this.ListOr = new DevExpress.XtraEditors.CheckEdit();
			this.ListNot = new DevExpress.XtraEditors.CheckEdit();
			this.Label6 = new DevExpress.XtraEditors.LabelControl();
			this.Label5 = new DevExpress.XtraEditors.LabelControl();
			this.Label4 = new DevExpress.XtraEditors.LabelControl();
			this.SaveCurrent = new DevExpress.XtraEditors.SimpleButton();
			this.Bitmaps8x8 = new System.Windows.Forms.ImageList(this.components);
			this.CloseButton = new DevExpress.XtraEditors.SimpleButton();
			this.Combine = new DevExpress.XtraEditors.SimpleButton();
			this.EditCurrent = new DevExpress.XtraEditors.SimpleButton();
			this.StatusMessage = new DevExpress.XtraBars.BarStaticItem();
			this.bar3 = new DevExpress.XtraBars.Bar();
			this.bar1 = new DevExpress.XtraBars.Bar();
			this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
			this.BarManager = new DevExpress.XtraBars.BarManager(this.components);
			this.label2 = new DevExpress.XtraEditors.LabelControl();
			this.SaveCurrentListContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.SavedListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.SaveCurrentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SaveNewTempListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.LeftPanel)).BeginInit();
			this.LeftPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RightPanel)).BeginInit();
			this.RightPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CenterPanel)).BeginInit();
			this.CenterPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ListAnd.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ListOr.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ListNot.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BarManager)).BeginInit();
			this.SaveCurrentListContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// LeftPanel
			// 
			this.LeftPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)));
			this.LeftPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.LeftPanel.Controls.Add(this.ListTree1);
			this.LeftPanel.Controls.Add(this.ListCaption1);
			this.LeftPanel.Location = new System.Drawing.Point(0, 28);
			this.LeftPanel.Name = "LeftPanel";
			this.LeftPanel.Size = new System.Drawing.Size(383, 589);
			this.LeftPanel.TabIndex = 1;
			// 
			// ListTree1
			// 
			this.ListTree1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.ListTree1.Location = new System.Drawing.Point(3, 29);
			this.ListTree1.Name = "ListTree1";
			this.ListTree1.Size = new System.Drawing.Size(380, 557);
			this.ListTree1.TabIndex = 150;
			// 
			// ListCaption1
			// 
			this.ListCaption1.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ListCaption1.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ListCaption1.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ListCaption1.Cursor = System.Windows.Forms.Cursors.Default;
			this.ListCaption1.Location = new System.Drawing.Point(3, 6);
			this.ListCaption1.Name = "ListCaption1";
			this.ListCaption1.Size = new System.Drawing.Size(31, 13);
			this.ListCaption1.TabIndex = 43;
			this.ListCaption1.Text = " List 1:";
			// 
			// RightPanel
			// 
			this.RightPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.RightPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.RightPanel.Controls.Add(this.ListTree2);
			this.RightPanel.Controls.Add(this.ListCaption2);
			this.RightPanel.Location = new System.Drawing.Point(466, 28);
			this.RightPanel.Name = "RightPanel";
			this.RightPanel.Size = new System.Drawing.Size(383, 589);
			this.RightPanel.TabIndex = 2;
			// 
			// ListTree2
			// 
			this.ListTree2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.ListTree2.Location = new System.Drawing.Point(0, 29);
			this.ListTree2.Name = "ListTree2";
			this.ListTree2.Size = new System.Drawing.Size(380, 557);
			this.ListTree2.TabIndex = 151;
			// 
			// ListCaption2
			// 
			this.ListCaption2.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ListCaption2.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ListCaption2.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ListCaption2.Cursor = System.Windows.Forms.Cursors.Default;
			this.ListCaption2.Location = new System.Drawing.Point(3, 6);
			this.ListCaption2.Name = "ListCaption2";
			this.ListCaption2.Size = new System.Drawing.Size(28, 13);
			this.ListCaption2.TabIndex = 44;
			this.ListCaption2.Text = "List 2:";
			// 
			// CenterPanel
			// 
			this.CenterPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.CenterPanel.Controls.Add(this.ListNotButton);
			this.CenterPanel.Controls.Add(this.ListOrButton);
			this.CenterPanel.Controls.Add(this.ListAndButton);
			this.CenterPanel.Controls.Add(this.ListAnd);
			this.CenterPanel.Controls.Add(this.ListOr);
			this.CenterPanel.Controls.Add(this.ListNot);
			this.CenterPanel.Controls.Add(this.Label6);
			this.CenterPanel.Controls.Add(this.Label5);
			this.CenterPanel.Controls.Add(this.Label4);
			this.CenterPanel.Location = new System.Drawing.Point(384, 28);
			this.CenterPanel.Name = "CenterPanel";
			this.CenterPanel.Size = new System.Drawing.Size(81, 359);
			this.CenterPanel.TabIndex = 3;
			// 
			// ListNotButton
			// 
			this.ListNotButton.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ListNotButton.Appearance.BorderColor = System.Drawing.Color.Transparent;
			this.ListNotButton.Appearance.Options.UseBackColor = true;
			this.ListNotButton.Appearance.Options.UseBorderColor = true;
			this.ListNotButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.ListNotButton.ImageIndex = 2;
			this.ListNotButton.ImageList = this.ImageList;
			this.ListNotButton.Location = new System.Drawing.Point(31, 175);
			this.ListNotButton.Name = "ListNotButton";
			this.ListNotButton.Size = new System.Drawing.Size(36, 36);
			this.ListNotButton.TabIndex = 47;
			this.ListNotButton.Click += new System.EventHandler(this.ListNotButton_Click);
			// 
			// ImageList
			// 
			this.ImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageList.ImageStream")));
			this.ImageList.TransparentColor = System.Drawing.Color.Cyan;
			this.ImageList.Images.SetKeyName(0, "");
			this.ImageList.Images.SetKeyName(1, "");
			this.ImageList.Images.SetKeyName(2, "ListNot.bmp");
			// 
			// ListOrButton
			// 
			this.ListOrButton.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ListOrButton.Appearance.BorderColor = System.Drawing.Color.Transparent;
			this.ListOrButton.Appearance.Options.UseBackColor = true;
			this.ListOrButton.Appearance.Options.UseBorderColor = true;
			this.ListOrButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.ListOrButton.ImageIndex = 1;
			this.ListOrButton.ImageList = this.ImageList;
			this.ListOrButton.Location = new System.Drawing.Point(31, 111);
			this.ListOrButton.Name = "ListOrButton";
			this.ListOrButton.Size = new System.Drawing.Size(36, 36);
			this.ListOrButton.TabIndex = 46;
			this.ListOrButton.Click += new System.EventHandler(this.ListOrButton_Click);
			// 
			// ListAndButton
			// 
			this.ListAndButton.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ListAndButton.Appearance.BorderColor = System.Drawing.Color.Transparent;
			this.ListAndButton.Appearance.Options.UseBackColor = true;
			this.ListAndButton.Appearance.Options.UseBorderColor = true;
			this.ListAndButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.ListAndButton.ImageIndex = 0;
			this.ListAndButton.ImageList = this.ImageList;
			this.ListAndButton.Location = new System.Drawing.Point(31, 51);
			this.ListAndButton.Name = "ListAndButton";
			this.ListAndButton.Size = new System.Drawing.Size(36, 36);
			this.ListAndButton.TabIndex = 45;
			this.ListAndButton.Click += new System.EventHandler(this.ListAndButton_Click);
			// 
			// ListAnd
			// 
			this.ListAnd.Cursor = System.Windows.Forms.Cursors.Default;
			this.ListAnd.EditValue = true;
			this.ListAnd.Location = new System.Drawing.Point(11, 60);
			this.ListAnd.Name = "ListAnd";
			this.ListAnd.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ListAnd.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ListAnd.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ListAnd.Properties.Appearance.Options.UseBackColor = true;
			this.ListAnd.Properties.Appearance.Options.UseFont = true;
			this.ListAnd.Properties.Appearance.Options.UseForeColor = true;
			this.ListAnd.Properties.AutoWidth = true;
			this.ListAnd.Properties.Caption = "";
			this.ListAnd.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ListAnd.Properties.RadioGroupIndex = 1;
			this.ListAnd.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ListAnd.Size = new System.Drawing.Size(23, 19);
			this.ListAnd.TabIndex = 30;
			// 
			// ListOr
			// 
			this.ListOr.Cursor = System.Windows.Forms.Cursors.Default;
			this.ListOr.Location = new System.Drawing.Point(11, 120);
			this.ListOr.Name = "ListOr";
			this.ListOr.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ListOr.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ListOr.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ListOr.Properties.Appearance.Options.UseBackColor = true;
			this.ListOr.Properties.Appearance.Options.UseFont = true;
			this.ListOr.Properties.Appearance.Options.UseForeColor = true;
			this.ListOr.Properties.AutoWidth = true;
			this.ListOr.Properties.Caption = "";
			this.ListOr.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ListOr.Properties.RadioGroupIndex = 1;
			this.ListOr.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ListOr.Size = new System.Drawing.Size(23, 19);
			this.ListOr.TabIndex = 31;
			this.ListOr.TabStop = false;
			// 
			// ListNot
			// 
			this.ListNot.Cursor = System.Windows.Forms.Cursors.Default;
			this.ListNot.Location = new System.Drawing.Point(11, 184);
			this.ListNot.Name = "ListNot";
			this.ListNot.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ListNot.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ListNot.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ListNot.Properties.Appearance.Options.UseBackColor = true;
			this.ListNot.Properties.Appearance.Options.UseFont = true;
			this.ListNot.Properties.Appearance.Options.UseForeColor = true;
			this.ListNot.Properties.AutoWidth = true;
			this.ListNot.Properties.Caption = "";
			this.ListNot.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ListNot.Properties.RadioGroupIndex = 1;
			this.ListNot.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ListNot.Size = new System.Drawing.Size(23, 19);
			this.ListNot.TabIndex = 32;
			this.ListNot.TabStop = false;
			// 
			// Label6
			// 
			this.Label6.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Label6.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label6.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label6.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label6.Location = new System.Drawing.Point(13, 206);
			this.Label6.Name = "Label6";
			this.Label6.Size = new System.Drawing.Size(49, 13);
			this.Label6.TabIndex = 44;
			this.Label6.Text = "Difference";
			// 
			// Label5
			// 
			this.Label5.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Label5.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label5.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label5.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label5.Location = new System.Drawing.Point(22, 142);
			this.Label5.Name = "Label5";
			this.Label5.Size = new System.Drawing.Size(28, 13);
			this.Label5.TabIndex = 43;
			this.Label5.Text = "Union";
			// 
			// Label4
			// 
			this.Label4.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Label4.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label4.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label4.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label4.Location = new System.Drawing.Point(17, 83);
			this.Label4.Name = "Label4";
			this.Label4.Size = new System.Drawing.Size(41, 13);
			this.Label4.TabIndex = 42;
			this.Label4.Text = "Intersect";
			// 
			// SaveCurrent
			// 
			this.SaveCurrent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveCurrent.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SaveCurrent.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SaveCurrent.Appearance.Options.UseFont = true;
			this.SaveCurrent.Appearance.Options.UseForeColor = true;
			this.SaveCurrent.Cursor = System.Windows.Forms.Cursors.Default;
			this.SaveCurrent.ImageIndex = 4;
			this.SaveCurrent.ImageList = this.Bitmaps8x8;
			this.SaveCurrent.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleRight;
			this.SaveCurrent.Location = new System.Drawing.Point(682, 620);
			this.SaveCurrent.Name = "SaveCurrent";
			this.SaveCurrent.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.SaveCurrent.Size = new System.Drawing.Size(106, 22);
			this.SaveCurrent.TabIndex = 56;
			this.SaveCurrent.Text = "Save Current List";
			this.SaveCurrent.Click += new System.EventHandler(this.SaveCurrentList_Click);
			// 
			// Bitmaps8x8
			// 
			this.Bitmaps8x8.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps8x8.ImageStream")));
			this.Bitmaps8x8.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps8x8.Images.SetKeyName(0, "");
			this.Bitmaps8x8.Images.SetKeyName(1, "");
			this.Bitmaps8x8.Images.SetKeyName(2, "ScrollDown.bmp");
			this.Bitmaps8x8.Images.SetKeyName(3, "ScrollUp.bmp");
			this.Bitmaps8x8.Images.SetKeyName(4, "DropDown8x8.bmp");
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CloseButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.CloseButton.Appearance.Options.UseFont = true;
			this.CloseButton.Appearance.Options.UseForeColor = true;
			this.CloseButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = new System.Drawing.Point(796, 620);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CloseButton.Size = new System.Drawing.Size(50, 22);
			this.CloseButton.TabIndex = 55;
			this.CloseButton.Text = "C&lose";
			// 
			// Combine
			// 
			this.Combine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Combine.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Combine.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Combine.Appearance.Options.UseFont = true;
			this.Combine.Appearance.Options.UseForeColor = true;
			this.Combine.Cursor = System.Windows.Forms.Cursors.Default;
			this.Combine.Location = new System.Drawing.Point(517, 620);
			this.Combine.Name = "Combine";
			this.Combine.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Combine.Size = new System.Drawing.Size(59, 22);
			this.Combine.TabIndex = 54;
			this.Combine.Text = "&Combine";
			this.Combine.Click += new System.EventHandler(this.Combine_Click);
			// 
			// EditCurrent
			// 
			this.EditCurrent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.EditCurrent.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EditCurrent.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.EditCurrent.Appearance.Options.UseFont = true;
			this.EditCurrent.Appearance.Options.UseForeColor = true;
			this.EditCurrent.Cursor = System.Windows.Forms.Cursors.Default;
			this.EditCurrent.Location = new System.Drawing.Point(584, 620);
			this.EditCurrent.Name = "EditCurrent";
			this.EditCurrent.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.EditCurrent.Size = new System.Drawing.Size(89, 22);
			this.EditCurrent.TabIndex = 57;
			this.EditCurrent.Text = "Edit Current List";
			this.EditCurrent.Click += new System.EventHandler(this.EditCurrent_Click);
			// 
			// StatusMessage
			// 
			this.StatusMessage.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.StatusMessage.Appearance.Options.UseBackColor = true;
			this.StatusMessage.Border = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.StatusMessage.Id = 0;
			this.StatusMessage.Name = "StatusMessage";
			this.StatusMessage.TextAlignment = System.Drawing.StringAlignment.Near;
			// 
			// bar3
			// 
			this.bar3.BarName = "Status bar";
			this.bar3.CanDockStyle = DevExpress.XtraBars.BarCanDockStyle.Bottom;
			this.bar3.DockCol = 0;
			this.bar3.DockRow = 0;
			this.bar3.DockStyle = DevExpress.XtraBars.BarDockStyle.Bottom;
			this.bar3.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.StatusMessage, DevExpress.XtraBars.BarItemPaintStyle.Standard)});
			this.bar3.OptionsBar.AllowQuickCustomization = false;
			this.bar3.OptionsBar.DrawDragBorder = false;
			this.bar3.OptionsBar.DrawSizeGrip = true;
			this.bar3.OptionsBar.UseWholeRow = true;
			this.bar3.Text = "Status bar";
			// 
			// bar1
			// 
			this.bar1.BarName = "Tools";
			this.bar1.DockCol = 0;
			this.bar1.DockRow = 0;
			this.bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
			this.bar1.Text = "Tools";
			this.bar1.Visible = false;
			// 
			// barDockControlTop
			// 
			this.barDockControlTop.CausesValidation = false;
			this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
			this.barDockControlTop.Size = new System.Drawing.Size(849, 29);
			// 
			// barDockControlBottom
			// 
			this.barDockControlBottom.CausesValidation = false;
			this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.barDockControlBottom.Location = new System.Drawing.Point(0, 644);
			this.barDockControlBottom.Size = new System.Drawing.Size(849, 23);
			// 
			// barDockControlLeft
			// 
			this.barDockControlLeft.CausesValidation = false;
			this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
			this.barDockControlLeft.Location = new System.Drawing.Point(0, 29);
			this.barDockControlLeft.Size = new System.Drawing.Size(0, 615);
			// 
			// barDockControlRight
			// 
			this.barDockControlRight.CausesValidation = false;
			this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
			this.barDockControlRight.Location = new System.Drawing.Point(849, 29);
			this.barDockControlRight.Size = new System.Drawing.Size(0, 615);
			// 
			// BarManager
			// 
			this.BarManager.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.bar1,
            this.bar3});
			this.BarManager.DockControls.Add(this.barDockControlTop);
			this.BarManager.DockControls.Add(this.barDockControlBottom);
			this.BarManager.DockControls.Add(this.barDockControlLeft);
			this.BarManager.DockControls.Add(this.barDockControlRight);
			this.BarManager.Form = this;
			this.BarManager.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.StatusMessage});
			this.BarManager.MaxItemId = 1;
			this.BarManager.StatusBar = this.bar3;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.label2.Appearance.BackColor = System.Drawing.Color.LightSteelBlue;
			this.label2.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.label2.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
			this.label2.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.label2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.label2.Location = new System.Drawing.Point(0, -1);
			this.label2.Name = "label2";
			this.label2.Padding = new System.Windows.Forms.Padding(4);
			this.label2.Size = new System.Drawing.Size(850, 29);
			this.label2.TabIndex = 151;
			this.label2.Text = " Select the two lists to be combined, List 1 and List 2, the combination method a" +
					"nd then click the Combine button. Results appear in the *Current list.";
			// 
			// SaveCurrentListContextMenu
			// 
			this.SaveCurrentListContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SavedListMenuItem,
            this.toolStripMenuItem2,
            this.SaveCurrentMenuItem,
            this.SaveNewTempListMenuItem});
			this.SaveCurrentListContextMenu.Name = "MarkedDataContextMenu";
			this.SaveCurrentListContextMenu.Size = new System.Drawing.Size(193, 76);
			// 
			// SavedListMenuItem
			// 
			this.SavedListMenuItem.Name = "SavedListMenuItem";
			this.SavedListMenuItem.Size = new System.Drawing.Size(192, 22);
			this.SavedListMenuItem.Text = "Saved List..";
			this.SavedListMenuItem.Click += new System.EventHandler(this.SavedListMenuItem_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(189, 6);
			// 
			// SaveCurrentMenuItem
			// 
			this.SaveCurrentMenuItem.Name = "SaveCurrentMenuItem";
			this.SaveCurrentMenuItem.Size = new System.Drawing.Size(192, 22);
			this.SaveCurrentMenuItem.Text = "Current List";
			this.SaveCurrentMenuItem.Click += new System.EventHandler(this.SaveTempListMenuItem_Click);
			// 
			// SaveNewTempListMenuItem
			// 
			this.SaveNewTempListMenuItem.Name = "SaveNewTempListMenuItem";
			this.SaveNewTempListMenuItem.Size = new System.Drawing.Size(192, 22);
			this.SaveNewTempListMenuItem.Text = "New Temporary List...";
			this.SaveNewTempListMenuItem.Click += new System.EventHandler(this.SaveNewTempListMenuItem_Click);
			// 
			// ListLogic
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(849, 667);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.EditCurrent);
			this.Controls.Add(this.SaveCurrent);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.Combine);
			this.Controls.Add(this.CenterPanel);
			this.Controls.Add(this.RightPanel);
			this.Controls.Add(this.LeftPanel);
			this.Controls.Add(this.barDockControlLeft);
			this.Controls.Add(this.barDockControlRight);
			this.Controls.Add(this.barDockControlBottom);
			this.Controls.Add(this.barDockControlTop);
			this.MinimizeBox = false;
			this.Name = "ListLogic";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "List Logic";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ListLogic_FormClosing);
			this.Resize += new System.EventHandler(this.ListLogic_Resize);
			((System.ComponentModel.ISupportInitialize)(this.LeftPanel)).EndInit();
			this.LeftPanel.ResumeLayout(false);
			this.LeftPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RightPanel)).EndInit();
			this.RightPanel.ResumeLayout(false);
			this.RightPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CenterPanel)).EndInit();
			this.CenterPanel.ResumeLayout(false);
			this.CenterPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ListAnd.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ListOr.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ListNot.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BarManager)).EndInit();
			this.SaveCurrentListContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraEditors.PanelControl LeftPanel;
		private DevExpress.XtraEditors.PanelControl RightPanel;
		private DevExpress.XtraEditors.PanelControl CenterPanel;
		public DevExpress.XtraEditors.LabelControl ListCaption1;
		public DevExpress.XtraEditors.LabelControl ListCaption2;
		public DevExpress.XtraEditors.SimpleButton SaveCurrent;
		public DevExpress.XtraEditors.SimpleButton CloseButton;
		public DevExpress.XtraEditors.SimpleButton Combine;
		public DevExpress.XtraEditors.CheckEdit ListAnd;
		public DevExpress.XtraEditors.CheckEdit ListOr;
		public DevExpress.XtraEditors.CheckEdit ListNot;
		public System.Windows.Forms.ImageList ImageList;
		public DevExpress.XtraEditors.LabelControl Label6;
		public DevExpress.XtraEditors.LabelControl Label5;
		public DevExpress.XtraEditors.LabelControl Label4;
		private DevExpress.XtraEditors.SimpleButton ListNotButton;
		private DevExpress.XtraEditors.SimpleButton ListOrButton;
		private DevExpress.XtraEditors.SimpleButton ListAndButton;
		private ContentsTreeControl ListTree1;
		private ContentsTreeControl ListTree2;
		public DevExpress.XtraEditors.SimpleButton EditCurrent;
		private DevExpress.XtraBars.BarStaticItem StatusMessage;
		private DevExpress.XtraBars.Bar bar3;
		private DevExpress.XtraBars.Bar bar1;
		private DevExpress.XtraBars.BarDockControl barDockControlTop;
		private DevExpress.XtraBars.BarDockControl barDockControlBottom;
		private DevExpress.XtraBars.BarDockControl barDockControlLeft;
		private DevExpress.XtraBars.BarDockControl barDockControlRight;
		private DevExpress.XtraBars.BarManager BarManager;
		public DevExpress.XtraEditors.LabelControl label2;
		public System.Windows.Forms.ContextMenuStrip SaveCurrentListContextMenu;
		private System.Windows.Forms.ToolStripMenuItem SavedListMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem SaveCurrentMenuItem;
		private System.Windows.Forms.ToolStripMenuItem SaveNewTempListMenuItem;
		public System.Windows.Forms.ImageList Bitmaps8x8;
	}
}