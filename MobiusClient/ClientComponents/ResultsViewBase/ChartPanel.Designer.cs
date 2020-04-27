namespace Mobius.ClientComponents
{
	partial class ChartPanel
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChartPanel));
			DevExpress.XtraCharts.XYDiagram xyDiagram1 = new DevExpress.XtraCharts.XYDiagram();
			DevExpress.XtraCharts.Series series1 = new DevExpress.XtraCharts.Series();
			DevExpress.XtraCharts.BubbleSeriesLabel bubbleSeriesLabel1 = new DevExpress.XtraCharts.BubbleSeriesLabel();
			DevExpress.XtraCharts.BubbleSeriesView bubbleSeriesView1 = new DevExpress.XtraCharts.BubbleSeriesView();
			DevExpress.XtraCharts.Series series2 = new DevExpress.XtraCharts.Series();
			DevExpress.XtraCharts.BubbleSeriesLabel bubbleSeriesLabel2 = new DevExpress.XtraCharts.BubbleSeriesLabel();
			DevExpress.XtraCharts.BubbleSeriesView bubbleSeriesView2 = new DevExpress.XtraCharts.BubbleSeriesView();
			DevExpress.XtraCharts.BubbleSeriesLabel bubbleSeriesLabel3 = new DevExpress.XtraCharts.BubbleSeriesLabel();
			DevExpress.XtraCharts.BubbleSeriesView bubbleSeriesView3 = new DevExpress.XtraCharts.BubbleSeriesView();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.SelectFieldMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.Table1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.Field1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.AxisClickedTimer = new System.Windows.Forms.Timer(this.components);
			this.Bitmaps8x8 = new System.Windows.Forms.ImageList(this.components);
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.ShowAxesTitlesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ShowAxesScaleLabelsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ChartPropertiesContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ShowFilterPanelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ShowSelectedDataRowsPanelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.ShowLegendMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ShowAxisTitlesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ShowAxisScaleLabelsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.TrellisDividerMenuItem = new System.Windows.Forms.ToolStripSeparator();
			this.ZoomMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ResetZoomMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ZoomDividerMenuItem = new System.Windows.Forms.ToolStripSeparator();
			this.SelectAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MarkedRowsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.PropertiesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolTipController = new DevExpress.Utils.ToolTipController(this.components);
			this.PointPropertiesContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.SelectPointMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MarkPointMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.LegendPropertiesContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.HideLegendMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.LegendPropertiesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MarkerSquareSmallImage = new DevExpress.XtraEditors.PictureEdit();
			this.ChartControl = new Mobius.ClientComponents.ChartControlMx();
			this.TrellisPanel = new Mobius.ClientComponents.TrellisPanel();
			this.CloseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
			this.SelectFieldMenu.SuspendLayout();
			this.ChartPropertiesContextMenu.SuspendLayout();
			this.PointPropertiesContextMenu.SuspendLayout();
			this.LegendPropertiesContextMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MarkerSquareSmallImage.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChartControl)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(xyDiagram1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(series1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(bubbleSeriesLabel1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(bubbleSeriesView1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(series2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(bubbleSeriesLabel2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(bubbleSeriesView2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(bubbleSeriesLabel3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(bubbleSeriesView3)).BeginInit();
			this.SuspendLayout();
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "MouseSelect.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "MouseZoom.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "MouseTranslate.bmp");
			this.Bitmaps16x16.Images.SetKeyName(3, "MouseRotate.bmp");
			this.Bitmaps16x16.Images.SetKeyName(4, "FullScreen.bmp");
			this.Bitmaps16x16.Images.SetKeyName(5, "maximizeWindow.bmp");
			this.Bitmaps16x16.Images.SetKeyName(6, "RestoreWindow.bmp");
			this.Bitmaps16x16.Images.SetKeyName(7, "CloseWindow.bmp");
			// 
			// SelectFieldMenu
			// 
			this.SelectFieldMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Table1MenuItem});
			this.SelectFieldMenu.Name = "SelectFieldMenu";
			this.SelectFieldMenu.Size = new System.Drawing.Size(118, 26);
			// 
			// Table1MenuItem
			// 
			this.Table1MenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Field1MenuItem});
			this.Table1MenuItem.Image = ((System.Drawing.Image)(resources.GetObject("Table1MenuItem.Image")));
			this.Table1MenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.Table1MenuItem.Name = "Table1MenuItem";
			this.Table1MenuItem.Size = new System.Drawing.Size(117, 22);
			this.Table1MenuItem.Text = "Table1";
			// 
			// Field1MenuItem
			// 
			this.Field1MenuItem.Name = "Field1MenuItem";
			this.Field1MenuItem.Size = new System.Drawing.Size(113, 22);
			this.Field1MenuItem.Text = "Field1";
			// 
			// Bitmaps8x8
			// 
			this.Bitmaps8x8.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps8x8.ImageStream")));
			this.Bitmaps8x8.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps8x8.Images.SetKeyName(0, "DropDown8x8.bmp");
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new System.Drawing.Size(218, 6);
			// 
			// ShowAxesTitlesMenuItem
			// 
			this.ShowAxesTitlesMenuItem.Name = "ShowAxesTitlesMenuItem";
			this.ShowAxesTitlesMenuItem.Size = new System.Drawing.Size(221, 22);
			this.ShowAxesTitlesMenuItem.Text = "Show Axes Titles";
			// 
			// ShowAxesScaleLabelsMenuItem
			// 
			this.ShowAxesScaleLabelsMenuItem.Name = "ShowAxesScaleLabelsMenuItem";
			this.ShowAxesScaleLabelsMenuItem.Size = new System.Drawing.Size(221, 22);
			this.ShowAxesScaleLabelsMenuItem.Text = "Show Axes Scale Labels";
			// 
			// ChartPropertiesContextMenu
			// 
			this.ChartPropertiesContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ShowLegendMenuItem,
            this.ShowAxisTitlesMenuItem,
            this.ShowAxisScaleLabelsMenuItem,
            this.TrellisDividerMenuItem,
            this.ZoomMenuItem,
            this.ResetZoomMenuItem,
            this.ZoomDividerMenuItem,
            this.SelectAllMenuItem,
            this.MarkedRowsMenuItem,
            this.toolStripMenuItem2,
            this.ShowFilterPanelMenuItem,
            this.ShowSelectedDataRowsPanelMenuItem,
            this.toolStripMenuItem4,
            this.CloseMenuItem,
            this.toolStripMenuItem1,
            this.PropertiesMenuItem});
			this.ChartPropertiesContextMenu.Name = "SelectFieldMenu";
			this.ChartPropertiesContextMenu.Size = new System.Drawing.Size(240, 298);
			this.ChartPropertiesContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ChartPropertiesContextMenu_Opening);
			// 
			// ShowFilterPanelMenuItem
			// 
			this.ShowFilterPanelMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ShowFilterPanelMenuItem.Image")));
			this.ShowFilterPanelMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ShowFilterPanelMenuItem.Name = "ShowFilterPanelMenuItem";
			this.ShowFilterPanelMenuItem.Size = new System.Drawing.Size(239, 22);
			this.ShowFilterPanelMenuItem.Text = "Show Filter Panel";
			this.ShowFilterPanelMenuItem.Click += new System.EventHandler(this.ShowFilterPanelMenuItem_Click);
			// 
			// ShowSelectedDataRowsPanelMenuItem
			// 
			this.ShowSelectedDataRowsPanelMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ShowSelectedDataRowsPanelMenuItem.Image")));
			this.ShowSelectedDataRowsPanelMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ShowSelectedDataRowsPanelMenuItem.Name = "ShowSelectedDataRowsPanelMenuItem";
			this.ShowSelectedDataRowsPanelMenuItem.Size = new System.Drawing.Size(239, 22);
			this.ShowSelectedDataRowsPanelMenuItem.Text = "Show Selected Data Rows Panel";
			this.ShowSelectedDataRowsPanelMenuItem.Click += new System.EventHandler(this.ShowSelectedDataRowsPanelMenuItem_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(236, 6);
			// 
			// ShowLegendMenuItem
			// 
			this.ShowLegendMenuItem.Name = "ShowLegendMenuItem";
			this.ShowLegendMenuItem.Size = new System.Drawing.Size(239, 22);
			this.ShowLegendMenuItem.Text = "Show Legend";
			this.ShowLegendMenuItem.Click += new System.EventHandler(this.ShowLegendMenuItem_Click);
			// 
			// ShowAxisTitlesMenuItem
			// 
			this.ShowAxisTitlesMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ShowAxisTitlesMenuItem.Name = "ShowAxisTitlesMenuItem";
			this.ShowAxisTitlesMenuItem.Size = new System.Drawing.Size(239, 22);
			this.ShowAxisTitlesMenuItem.Text = "Show Axis Titles";
			this.ShowAxisTitlesMenuItem.Click += new System.EventHandler(this.ShowAxisTitlesMenuItem_Click);
			// 
			// ShowAxisScaleLabelsMenuItem
			// 
			this.ShowAxisScaleLabelsMenuItem.Name = "ShowAxisScaleLabelsMenuItem";
			this.ShowAxisScaleLabelsMenuItem.Size = new System.Drawing.Size(239, 22);
			this.ShowAxisScaleLabelsMenuItem.Text = "Show Axis Scale Labels";
			this.ShowAxisScaleLabelsMenuItem.Click += new System.EventHandler(this.ShowAxisScaleLabelsMenuItem_Click);
			// 
			// TrellisDividerMenuItem
			// 
			this.TrellisDividerMenuItem.Name = "TrellisDividerMenuItem";
			this.TrellisDividerMenuItem.Size = new System.Drawing.Size(236, 6);
			// 
			// ZoomMenuItem
			// 
			this.ZoomMenuItem.Name = "ZoomMenuItem";
			this.ZoomMenuItem.Size = new System.Drawing.Size(239, 22);
			this.ZoomMenuItem.Text = "Zoom...";
			this.ZoomMenuItem.Click += new System.EventHandler(this.ZoomMenuItem_Click);
			// 
			// ResetZoomMenuItem
			// 
			this.ResetZoomMenuItem.Name = "ResetZoomMenuItem";
			this.ResetZoomMenuItem.Size = new System.Drawing.Size(239, 22);
			this.ResetZoomMenuItem.Text = "Reset Zoom";
			this.ResetZoomMenuItem.Click += new System.EventHandler(this.ResetZoomMenuItem_Click);
			// 
			// ZoomDividerMenuItem
			// 
			this.ZoomDividerMenuItem.Name = "ZoomDividerMenuItem";
			this.ZoomDividerMenuItem.Size = new System.Drawing.Size(236, 6);
			// 
			// SelectAllMenuItem
			// 
			this.SelectAllMenuItem.Name = "SelectAllMenuItem";
			this.SelectAllMenuItem.Size = new System.Drawing.Size(239, 22);
			this.SelectAllMenuItem.Text = "Select All";
			this.SelectAllMenuItem.Click += new System.EventHandler(this.SelectAllMenuItem_Click);
			// 
			// MarkedRowsMenuItem
			// 
			this.MarkedRowsMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.CheckOn;
			this.MarkedRowsMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.MarkedRowsMenuItem.Name = "MarkedRowsMenuItem";
			this.MarkedRowsMenuItem.ShowShortcutKeys = false;
			this.MarkedRowsMenuItem.Size = new System.Drawing.Size(239, 22);
			this.MarkedRowsMenuItem.Text = "Marked Rows...";
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(236, 6);
			// 
			// PropertiesMenuItem
			// 
			this.PropertiesMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("PropertiesMenuItem.Image")));
			this.PropertiesMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.PropertiesMenuItem.Name = "PropertiesMenuItem";
			this.PropertiesMenuItem.Size = new System.Drawing.Size(239, 22);
			this.PropertiesMenuItem.Text = "Properties...";
			this.PropertiesMenuItem.Click += new System.EventHandler(this.PropertiesMenuItem_Click);
			// 
			// ToolTipController
			// 
			this.ToolTipController.AutoPopDelay = 1000000;
			this.ToolTipController.InitialDelay = 1;
			this.ToolTipController.ReshowDelay = 1;
			this.ToolTipController.ShowBeak = true;
			this.ToolTipController.ToolTipLocation = DevExpress.Utils.ToolTipLocation.TopCenter;
			// 
			// PointPropertiesContextMenu
			// 
			this.PointPropertiesContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SelectPointMenuItem,
            this.MarkPointMenuItem});
			this.PointPropertiesContextMenu.Name = "PointPropertiesContextMenu";
			this.PointPropertiesContextMenu.Size = new System.Drawing.Size(142, 48);
			// 
			// SelectPointMenuItem
			// 
			this.SelectPointMenuItem.Name = "SelectPointMenuItem";
			this.SelectPointMenuItem.Size = new System.Drawing.Size(141, 22);
			this.SelectPointMenuItem.Text = "Select Point";
			this.SelectPointMenuItem.Click += new System.EventHandler(this.SelectPointMenuItem_Click);
			// 
			// MarkPointMenuItem
			// 
			this.MarkPointMenuItem.Name = "MarkPointMenuItem";
			this.MarkPointMenuItem.Size = new System.Drawing.Size(141, 22);
			this.MarkPointMenuItem.Text = "Mark Point";
			this.MarkPointMenuItem.Click += new System.EventHandler(this.MarkPointMenuItem_Click);
			// 
			// LegendPropertiesContextMenu
			// 
			this.LegendPropertiesContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.HideLegendMenuItem,
            this.toolStripSeparator1,
            this.LegendPropertiesMenuItem});
			this.LegendPropertiesContextMenu.Name = "SelectFieldMenu";
			this.LegendPropertiesContextMenu.Size = new System.Drawing.Size(147, 54);
			// 
			// HideLegendMenuItem
			// 
			this.HideLegendMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.HideLegendMenuItem.Name = "HideLegendMenuItem";
			this.HideLegendMenuItem.Size = new System.Drawing.Size(146, 22);
			this.HideLegendMenuItem.Text = "Hide Legend";
			this.HideLegendMenuItem.Click += new System.EventHandler(this.HideLegendMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(143, 6);
			// 
			// LegendPropertiesMenuItem
			// 
			this.LegendPropertiesMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("LegendPropertiesMenuItem.Image")));
			this.LegendPropertiesMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.LegendPropertiesMenuItem.Name = "LegendPropertiesMenuItem";
			this.LegendPropertiesMenuItem.Size = new System.Drawing.Size(146, 22);
			this.LegendPropertiesMenuItem.Text = "Properties...";
			this.LegendPropertiesMenuItem.Click += new System.EventHandler(this.LegendPropertiesMenuItem_Click);
			// 
			// MarkerSquareSmallImage
			// 
			this.MarkerSquareSmallImage.EditValue = ((object)(resources.GetObject("MarkerSquareSmallImage.EditValue")));
			this.MarkerSquareSmallImage.Location = new System.Drawing.Point(257, 69);
			this.MarkerSquareSmallImage.Name = "MarkerSquareSmallImage";
			this.MarkerSquareSmallImage.Size = new System.Drawing.Size(20, 20);
			this.MarkerSquareSmallImage.TabIndex = 209;
			this.MarkerSquareSmallImage.Visible = false;
			// 
			// ChartControl
			// 
			////xyDiagram1.AxisX.Range.ScrollingRange.SideMarginsEnabled = true;
			////xyDiagram1.AxisX.Range.SideMarginsEnabled = true;
			xyDiagram1.AxisX.VisibleInPanesSerializable = "-1";
			////xyDiagram1.AxisY.Range.ScrollingRange.SideMarginsEnabled = true;
			////xyDiagram1.AxisY.Range.SideMarginsEnabled = true;
			xyDiagram1.AxisY.VisibleInPanesSerializable = "-1";
			this.ChartControl.Diagram = xyDiagram1;
			this.ChartControl.Location = new System.Drawing.Point(15, 13);
			this.ChartControl.Name = "ChartControl";
			bubbleSeriesLabel1.LineVisibility = DevExpress.Utils.DefaultBoolean.True;
			series1.Label = bubbleSeriesLabel1;
			series1.Name = "Series 1";
			series1.View = bubbleSeriesView1;
			bubbleSeriesLabel2.LineVisibility = DevExpress.Utils.DefaultBoolean.True;
			series2.Label = bubbleSeriesLabel2;
			series2.Name = "Series 2";
			series2.View = bubbleSeriesView2;
			this.ChartControl.SeriesSerializable = new DevExpress.XtraCharts.Series[] {
        series1,
        series2};
			bubbleSeriesLabel3.LineVisibility = DevExpress.Utils.DefaultBoolean.True;
			this.ChartControl.SeriesTemplate.Label = bubbleSeriesLabel3;
			this.ChartControl.SeriesTemplate.View = bubbleSeriesView3;
			this.ChartControl.Size = new System.Drawing.Size(298, 253);
			this.ChartControl.TabIndex = 204;
			this.ChartControl.CustomPaint += new DevExpress.XtraCharts.CustomPaintEventHandler(this.ChartControl_CustomPaint);
			this.ChartControl.Paint += new System.Windows.Forms.PaintEventHandler(this.ChartControl_Paint);
			// 
			// TrellisPanel
			// 
			this.TrellisPanel.Appearance.BackColor = System.Drawing.Color.White;
			this.TrellisPanel.Appearance.Options.UseBackColor = true;
			this.TrellisPanel.Location = new System.Drawing.Point(26, 353);
			this.TrellisPanel.Name = "TrellisPanel";
			this.TrellisPanel.Size = new System.Drawing.Size(287, 242);
			this.TrellisPanel.TabIndex = 0;
			// 
			// CloseMenuItem
			// 
			this.CloseMenuItem.Name = "CloseMenuItem";
			this.CloseMenuItem.Size = new System.Drawing.Size(239, 22);
			this.CloseMenuItem.Text = "Close";
			this.CloseMenuItem.Click += new System.EventHandler(this.CloseMenuItem_Click);
			// 
			// toolStripMenuItem4
			// 
			this.toolStripMenuItem4.Name = "toolStripMenuItem4";
			this.toolStripMenuItem4.Size = new System.Drawing.Size(236, 6);
			// 
			// ChartPanel
			// 
			this.Appearance.BackColor = System.Drawing.Color.WhiteSmoke;
			this.Appearance.Options.UseBackColor = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ChartControl);
			this.Controls.Add(this.TrellisPanel);
			this.Controls.Add(this.MarkerSquareSmallImage);
			this.Name = "ChartPanel";
			this.Size = new System.Drawing.Size(744, 611);
			this.SelectFieldMenu.ResumeLayout(false);
			this.ChartPropertiesContextMenu.ResumeLayout(false);
			this.PointPropertiesContextMenu.ResumeLayout(false);
			this.LegendPropertiesContextMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MarkerSquareSmallImage.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(xyDiagram1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(bubbleSeriesLabel1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(bubbleSeriesView1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(series1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(bubbleSeriesLabel2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(bubbleSeriesView2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(series2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(bubbleSeriesLabel3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(bubbleSeriesView3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChartControl)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal TrellisPanel TrellisPanel;
		private System.Windows.Forms.ImageList Bitmaps16x16;
		private System.Windows.Forms.ContextMenuStrip SelectFieldMenu;
		private System.Windows.Forms.ToolStripMenuItem Table1MenuItem;
		private System.Windows.Forms.ToolStripMenuItem Field1MenuItem;
		private System.Windows.Forms.Timer AxisClickedTimer;
		private System.Windows.Forms.ImageList Bitmaps8x8;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.ToolStripMenuItem ShowAxesTitlesMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ShowAxesScaleLabelsMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ShowAxisTitlesMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ShowAxisScaleLabelsMenuItem;
		private System.Windows.Forms.ToolStripSeparator TrellisDividerMenuItem;
		private System.Windows.Forms.ToolStripMenuItem PropertiesMenuItem;
		internal System.Windows.Forms.ContextMenuStrip ChartPropertiesContextMenu;
		internal ChartControlMx ChartControl;
		internal DevExpress.Utils.ToolTipController ToolTipController;
		private System.Windows.Forms.ToolStripMenuItem MarkedRowsMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem ZoomMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ResetZoomMenuItem;
		private System.Windows.Forms.ToolStripSeparator ZoomDividerMenuItem;
		internal System.Windows.Forms.ContextMenuStrip PointPropertiesContextMenu;
		private System.Windows.Forms.ToolStripMenuItem SelectAllMenuItem;
		private System.Windows.Forms.ToolStripMenuItem SelectPointMenuItem;
		private System.Windows.Forms.ToolStripMenuItem MarkPointMenuItem;
		internal System.Windows.Forms.ContextMenuStrip LegendPropertiesContextMenu;
		private System.Windows.Forms.ToolStripMenuItem HideLegendMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem LegendPropertiesMenuItem;
		internal DevExpress.XtraEditors.PictureEdit MarkerSquareSmallImage;
		private System.Windows.Forms.ToolStripMenuItem ShowSelectedDataRowsPanelMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ShowFilterPanelMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem ShowLegendMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
		private System.Windows.Forms.ToolStripMenuItem CloseMenuItem;
	}
}
