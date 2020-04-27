namespace Mobius.SpotfireClient
{
	partial class ColumnSelectorControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColumnSelectorControl));
			this.Bitmaps8x8 = new System.Windows.Forms.ImageList(this.components);
			this.ModelColButton = new DevExpress.XtraEditors.DropDownButton();
			this.BarManager = new DevExpress.XtraBars.BarManager(this.components);
			this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
			this.LayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.AddColButton = new DevExpress.XtraEditors.DropDownButton();
			this.ColumnSelectorContainerPanel = new DevExpress.XtraEditors.PanelControl();
			this.ExpressionRtClickContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.RemoveExpression = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator24 = new System.Windows.Forms.ToolStripSeparator();
			this.MoveExpressionLeftMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MoveExpressionRightMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.DropdownColumnSelector = new Mobius.SpotfireClient.ColumnSelectorDropDown();
			this.DropdownControlContainer = new DevExpress.XtraBars.PopupControlContainer(this.components);
			this.RemoveAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.BarManager)).BeginInit();
			this.LayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ColumnSelectorContainerPanel)).BeginInit();
			this.ColumnSelectorContainerPanel.SuspendLayout();
			this.ExpressionRtClickContextMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DropdownControlContainer)).BeginInit();
			this.DropdownControlContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// Bitmaps8x8
			// 
			this.Bitmaps8x8.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps8x8.ImageStream")));
			this.Bitmaps8x8.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps8x8.Images.SetKeyName(0, "DropDownChevron8x8.bmp");
			// 
			// ModelColButton
			// 
			this.ModelColButton.ActAsDropDown = false;
			this.ModelColButton.Appearance.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ModelColButton.Appearance.Options.UseFont = true;
			this.ModelColButton.AutoSize = true;
			this.ModelColButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.ModelColButton.Location = new System.Drawing.Point(2, 2);
			this.ModelColButton.Margin = new System.Windows.Forms.Padding(2);
			this.ModelColButton.Name = "ModelColButton";
			this.ModelColButton.Size = new System.Drawing.Size(99, 20);
			this.ModelColButton.TabIndex = 4;
			this.ModelColButton.Text = "Sum(T2.Amw)";
			this.ModelColButton.ArrowButtonClick += new System.EventHandler(this.ColButton_ArrowButtonClick);
			this.ModelColButton.Click += new System.EventHandler(this.ColButton_Click);
			this.ModelColButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ColButton_MouseDown);
			// 
			// BarManager
			// 
			this.BarManager.DockControls.Add(this.barDockControlTop);
			this.BarManager.DockControls.Add(this.barDockControlBottom);
			this.BarManager.DockControls.Add(this.barDockControlLeft);
			this.BarManager.DockControls.Add(this.barDockControlRight);
			this.BarManager.Form = this;
			// 
			// barDockControlTop
			// 
			this.barDockControlTop.CausesValidation = false;
			this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
			this.barDockControlTop.Manager = this.BarManager;
			this.barDockControlTop.Size = new System.Drawing.Size(1073, 0);
			// 
			// barDockControlBottom
			// 
			this.barDockControlBottom.CausesValidation = false;
			this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.barDockControlBottom.Location = new System.Drawing.Point(0, 652);
			this.barDockControlBottom.Manager = this.BarManager;
			this.barDockControlBottom.Size = new System.Drawing.Size(1073, 0);
			// 
			// barDockControlLeft
			// 
			this.barDockControlLeft.CausesValidation = false;
			this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
			this.barDockControlLeft.Location = new System.Drawing.Point(0, 0);
			this.barDockControlLeft.Manager = this.BarManager;
			this.barDockControlLeft.Size = new System.Drawing.Size(0, 652);
			// 
			// barDockControlRight
			// 
			this.barDockControlRight.CausesValidation = false;
			this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
			this.barDockControlRight.Location = new System.Drawing.Point(1073, 0);
			this.barDockControlRight.Manager = this.BarManager;
			this.barDockControlRight.Size = new System.Drawing.Size(0, 652);
			// 
			// LayoutPanel
			// 
			this.LayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LayoutPanel.ColumnCount = 2;
			this.LayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.LayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.LayoutPanel.Controls.Add(this.AddColButton, 1, 0);
			this.LayoutPanel.Controls.Add(this.ModelColButton, 0, 0);
			this.LayoutPanel.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddColumns;
			this.LayoutPanel.Location = new System.Drawing.Point(2, 2);
			this.LayoutPanel.Name = "LayoutPanel";
			this.LayoutPanel.RowCount = 1;
			this.LayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.LayoutPanel.Size = new System.Drawing.Size(478, 24);
			this.LayoutPanel.TabIndex = 6;
			// 
			// AddColButton
			// 
			this.AddColButton.Appearance.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AddColButton.Appearance.Options.UseFont = true;
			this.AddColButton.AutoSize = true;
			this.AddColButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.AddColButton.Location = new System.Drawing.Point(105, 2);
			this.AddColButton.Margin = new System.Windows.Forms.Padding(2);
			this.AddColButton.Name = "AddColButton";
			this.AddColButton.Size = new System.Drawing.Size(34, 20);
			this.AddColButton.TabIndex = 6;
			this.AddColButton.Text = "+";
			// 
			// ColumnSelectorContainerPanel
			// 
			this.ColumnSelectorContainerPanel.Appearance.BackColor = System.Drawing.Color.White;
			this.ColumnSelectorContainerPanel.Appearance.BorderColor = System.Drawing.Color.Red;
			this.ColumnSelectorContainerPanel.Appearance.ForeColor = System.Drawing.Color.White;
			this.ColumnSelectorContainerPanel.Appearance.Options.UseBackColor = true;
			this.ColumnSelectorContainerPanel.Appearance.Options.UseBorderColor = true;
			this.ColumnSelectorContainerPanel.Appearance.Options.UseForeColor = true;
			this.ColumnSelectorContainerPanel.Controls.Add(this.LayoutPanel);
			this.ColumnSelectorContainerPanel.Location = new System.Drawing.Point(2, 2);
			this.ColumnSelectorContainerPanel.Name = "ColumnSelectorContainerPanel";
			this.ColumnSelectorContainerPanel.Size = new System.Drawing.Size(482, 42);
			this.ColumnSelectorContainerPanel.TabIndex = 7;
			// 
			// ExpressionRtClickContextMenu
			// 
			this.ExpressionRtClickContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RemoveExpression,
            this.RemoveAllMenuItem,
            this.toolStripSeparator24,
            this.MoveExpressionLeftMenuItem,
            this.MoveExpressionRightMenuItem});
			this.ExpressionRtClickContextMenu.Name = "CriteriaColRtClickContextMenu";
			this.ExpressionRtClickContextMenu.Size = new System.Drawing.Size(181, 120);
			// 
			// RemoveExpression
			// 
			this.RemoveExpression.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.RemoveExpression.Name = "RemoveExpression";
			this.RemoveExpression.Size = new System.Drawing.Size(180, 22);
			this.RemoveExpression.Text = "Remove";
			this.RemoveExpression.Click += new System.EventHandler(this.RemoveExpression_Click);
			// 
			// toolStripSeparator24
			// 
			this.toolStripSeparator24.Name = "toolStripSeparator24";
			this.toolStripSeparator24.Size = new System.Drawing.Size(177, 6);
			// 
			// MoveExpressionLeftMenuItem
			// 
			this.MoveExpressionLeftMenuItem.CheckOnClick = true;
			this.MoveExpressionLeftMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("MoveExpressionLeftMenuItem.Image")));
			this.MoveExpressionLeftMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.MoveExpressionLeftMenuItem.Name = "MoveExpressionLeftMenuItem";
			this.MoveExpressionLeftMenuItem.Size = new System.Drawing.Size(180, 22);
			this.MoveExpressionLeftMenuItem.Text = "Move Left";
			this.MoveExpressionLeftMenuItem.Click += new System.EventHandler(this.MoveExpressionLeftMenuItem_Click);
			// 
			// MoveExpressionRightMenuItem
			// 
			this.MoveExpressionRightMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("MoveExpressionRightMenuItem.Image")));
			this.MoveExpressionRightMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.MoveExpressionRightMenuItem.Name = "MoveExpressionRightMenuItem";
			this.MoveExpressionRightMenuItem.Size = new System.Drawing.Size(180, 22);
			this.MoveExpressionRightMenuItem.Text = "Move Right";
			this.MoveExpressionRightMenuItem.Click += new System.EventHandler(this.MoveExpressionRightMenuItem_Click);
			// 
			// DropdownColumnSelector
			// 
			this.DropdownColumnSelector.Appearance.BorderColor = System.Drawing.Color.Silver;
			this.DropdownColumnSelector.Appearance.Options.UseBorderColor = true;
			this.DropdownColumnSelector.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DropdownColumnSelector.Location = new System.Drawing.Point(0, 0);
			this.DropdownColumnSelector.Name = "DropdownColumnSelector";
			this.DropdownColumnSelector.Size = new System.Drawing.Size(331, 469);
			this.DropdownColumnSelector.TabIndex = 0;
			// 
			// DropDownControlContainer
			// 
			this.DropdownControlContainer.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.DropdownControlContainer.Controls.Add(this.DropdownColumnSelector);
			this.DropdownControlContainer.Location = new System.Drawing.Point(109, 50);
			this.DropdownControlContainer.Manager = this.BarManager;
			this.DropdownControlContainer.Name = "DropDownControlContainer";
			this.DropdownControlContainer.Size = new System.Drawing.Size(331, 469);
			this.DropdownControlContainer.TabIndex = 238;
			this.DropdownControlContainer.Visible = false;
			// 
			// RemoveAllMenuItem
			// 
			this.RemoveAllMenuItem.Name = "RemoveAllMenuItem";
			this.RemoveAllMenuItem.Size = new System.Drawing.Size(180, 22);
			this.RemoveAllMenuItem.Text = "Remove All";
			this.RemoveAllMenuItem.Click += new System.EventHandler(this.RemoveAllMenuItem_Click);
			// 
			// ColumnSelectorControl
			// 
			this.Appearance.BackColor = System.Drawing.Color.White;
			this.Appearance.BorderColor = System.Drawing.Color.Silver;
			this.Appearance.Options.UseBackColor = true;
			this.Appearance.Options.UseBorderColor = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.DropdownControlContainer);
			this.Controls.Add(this.ColumnSelectorContainerPanel);
			this.Controls.Add(this.barDockControlLeft);
			this.Controls.Add(this.barDockControlRight);
			this.Controls.Add(this.barDockControlBottom);
			this.Controls.Add(this.barDockControlTop);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "ColumnSelectorControl";
			this.Size = new System.Drawing.Size(1073, 652);
			((System.ComponentModel.ISupportInitialize)(this.BarManager)).EndInit();
			this.LayoutPanel.ResumeLayout(false);
			this.LayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ColumnSelectorContainerPanel)).EndInit();
			this.ColumnSelectorContainerPanel.ResumeLayout(false);
			this.ExpressionRtClickContextMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.DropdownControlContainer)).EndInit();
			this.DropdownControlContainer.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ImageList Bitmaps8x8;
		private DevExpress.XtraEditors.DropDownButton ModelColButton;
		private System.Windows.Forms.TableLayoutPanel LayoutPanel;
		private DevExpress.XtraEditors.PanelControl ColumnSelectorContainerPanel;
		private DevExpress.XtraEditors.DropDownButton AddColButton;
		private DevExpress.XtraBars.BarManager BarManager;
		private DevExpress.XtraBars.BarDockControl barDockControlTop;
		private DevExpress.XtraBars.BarDockControl barDockControlBottom;
		private DevExpress.XtraBars.BarDockControl barDockControlLeft;
		private DevExpress.XtraBars.BarDockControl barDockControlRight;
		public System.Windows.Forms.ContextMenuStrip ExpressionRtClickContextMenu;
		public System.Windows.Forms.ToolStripMenuItem RemoveExpression;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator24;
		public System.Windows.Forms.ToolStripMenuItem MoveExpressionLeftMenuItem;
		public System.Windows.Forms.ToolStripMenuItem MoveExpressionRightMenuItem;
		private ColumnSelectorDropDown DropdownColumnSelector;
		private DevExpress.XtraBars.PopupControlContainer DropdownControlContainer;
		private System.Windows.Forms.ToolStripMenuItem RemoveAllMenuItem;
	}
}
