namespace Mobius.SpotfireClient
{
	partial class SpotfireToolbar
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpotfireToolbar));
			this.separatorControl4 = new DevExpress.XtraEditors.SeparatorControl();
			this.OpenDocMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.OpenFromFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.OpenFromLibraryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.separatorControl3 = new DevExpress.XtraEditors.SeparatorControl();
			this.separatorControl1 = new DevExpress.XtraEditors.SeparatorControl();
			this.separatorControl2 = new DevExpress.XtraEditors.SeparatorControl();
			this.OpenDocumentButton = new DevExpress.XtraEditors.DropDownButton();
			this.ArrangeMaximizeActive = new DevExpress.XtraEditors.SimpleButton();
			this.ArrangeStackedButton = new DevExpress.XtraEditors.SimpleButton();
			this.ArrangeSideBySideButton = new DevExpress.XtraEditors.SimpleButton();
			this.ArrangeEvenlyButton = new DevExpress.XtraEditors.SimpleButton();
			this.FiltersButton = new DevExpress.XtraEditors.SimpleButton();
			this.DetailsOnDemandButton = new DevExpress.XtraEditors.SimpleButton();
			this.VisPropsButton = new DevExpress.XtraEditors.SimpleButton();
			this.TableVisualButton = new DevExpress.XtraEditors.SimpleButton();
			this.TrellisCardButton = new DevExpress.XtraEditors.SimpleButton();
			this.BarChartButton = new DevExpress.XtraEditors.SimpleButton();
			this.HeatMapButton = new DevExpress.XtraEditors.SimpleButton();
			this.ScatterPlotButton = new DevExpress.XtraEditors.SimpleButton();
			this.TreemapButton = new DevExpress.XtraEditors.SimpleButton();
			this.DataButton = new DevExpress.XtraEditors.SimpleButton();
			((System.ComponentModel.ISupportInitialize)(this.separatorControl4)).BeginInit();
			this.OpenDocMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.separatorControl3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.separatorControl1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.separatorControl2)).BeginInit();
			this.SuspendLayout();
			// 
			// separatorControl4
			// 
			this.separatorControl4.LineColor = System.Drawing.Color.Gray;
			this.separatorControl4.LineOrientation = System.Windows.Forms.Orientation.Vertical;
			this.separatorControl4.LineThickness = 1;
			this.separatorControl4.Location = new System.Drawing.Point(59, 115);
			this.separatorControl4.Name = "separatorControl4";
			this.separatorControl4.Padding = new System.Windows.Forms.Padding(0);
			this.separatorControl4.Size = new System.Drawing.Size(8, 22);
			this.separatorControl4.TabIndex = 16;
			// 
			// OpenDocMenu
			// 
			this.OpenDocMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenFromFileMenuItem,
            this.OpenFromLibraryMenuItem});
			this.OpenDocMenu.Name = "OpenDocMenu";
			this.OpenDocMenu.Size = new System.Drawing.Size(184, 48);
			// 
			// OpenFromFileMenuItem
			// 
			this.OpenFromFileMenuItem.Name = "OpenFromFileMenuItem";
			this.OpenFromFileMenuItem.Size = new System.Drawing.Size(183, 22);
			this.OpenFromFileMenuItem.Text = "Open From File...";
			this.OpenFromFileMenuItem.Click += new System.EventHandler(this.OpenFromFile_Click);
			// 
			// OpenFromLibraryMenuItem
			// 
			this.OpenFromLibraryMenuItem.Name = "OpenFromLibraryMenuItem";
			this.OpenFromLibraryMenuItem.Size = new System.Drawing.Size(183, 22);
			this.OpenFromLibraryMenuItem.Text = "Open  from Library...";
			this.OpenFromLibraryMenuItem.Click += new System.EventHandler(this.OpenFromLibraryMenuItem_Click);
			// 
			// separatorControl3
			// 
			this.separatorControl3.LineColor = System.Drawing.Color.Gray;
			this.separatorControl3.LineOrientation = System.Windows.Forms.Orientation.Vertical;
			this.separatorControl3.LineThickness = 1;
			this.separatorControl3.Location = new System.Drawing.Point(271, 115);
			this.separatorControl3.Name = "separatorControl3";
			this.separatorControl3.Padding = new System.Windows.Forms.Padding(0);
			this.separatorControl3.Size = new System.Drawing.Size(8, 22);
			this.separatorControl3.TabIndex = 11;
			// 
			// separatorControl1
			// 
			this.separatorControl1.LineColor = System.Drawing.Color.Gray;
			this.separatorControl1.LineOrientation = System.Windows.Forms.Orientation.Vertical;
			this.separatorControl1.LineThickness = 1;
			this.separatorControl1.Location = new System.Drawing.Point(113, 115);
			this.separatorControl1.Name = "separatorControl1";
			this.separatorControl1.Padding = new System.Windows.Forms.Padding(0);
			this.separatorControl1.Size = new System.Drawing.Size(8, 22);
			this.separatorControl1.TabIndex = 2;
			// 
			// separatorControl2
			// 
			this.separatorControl2.LineColor = System.Drawing.Color.Gray;
			this.separatorControl2.LineOrientation = System.Windows.Forms.Orientation.Vertical;
			this.separatorControl2.LineThickness = 1;
			this.separatorControl2.Location = new System.Drawing.Point(379, 115);
			this.separatorControl2.Name = "separatorControl2";
			this.separatorControl2.Padding = new System.Windows.Forms.Padding(0);
			this.separatorControl2.Size = new System.Drawing.Size(8, 22);
			this.separatorControl2.TabIndex = 9;
			// 
			// OpenDocumentButton
			// 
			this.OpenDocumentButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.OpenDocumentButton.ContextMenuStrip = this.OpenDocMenu;
			this.OpenDocumentButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("OpenDocumentButton.ImageOptions.Image")));
			this.OpenDocumentButton.Location = new System.Drawing.Point(21, 117);
			this.OpenDocumentButton.Name = "OpenDocumentButton";
			this.OpenDocumentButton.Size = new System.Drawing.Size(41, 18);
			this.OpenDocumentButton.TabIndex = 1;
			this.OpenDocumentButton.ToolTip = "Open";
			// 
			// ArrangeMaximizeActive
			// 
			this.ArrangeMaximizeActive.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.ArrangeMaximizeActive.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("ArrangeMaximizeActive.ImageOptions.Image")));
			this.ArrangeMaximizeActive.Location = new System.Drawing.Point(357, 117);
			this.ArrangeMaximizeActive.Name = "ArrangeMaximizeActive";
			this.ArrangeMaximizeActive.Size = new System.Drawing.Size(18, 18);
			this.ArrangeMaximizeActive.TabIndex = 15;
			this.ArrangeMaximizeActive.ToolTip = "Maximize Active";
			this.ArrangeMaximizeActive.Click += new System.EventHandler(this.ArrangeMaximizeActive_Click);
			// 
			// ArrangeStackedButton
			// 
			this.ArrangeStackedButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.ArrangeStackedButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("ArrangeStackedButton.ImageOptions.Image")));
			this.ArrangeStackedButton.Location = new System.Drawing.Point(333, 117);
			this.ArrangeStackedButton.Name = "ArrangeStackedButton";
			this.ArrangeStackedButton.Size = new System.Drawing.Size(18, 18);
			this.ArrangeStackedButton.TabIndex = 14;
			this.ArrangeStackedButton.ToolTip = "Stacked";
			this.ArrangeStackedButton.Click += new System.EventHandler(this.ArrangeStackedButton_Click);
			// 
			// ArrangeSideBySideButton
			// 
			this.ArrangeSideBySideButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.ArrangeSideBySideButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("ArrangeSideBySideButton.ImageOptions.Image")));
			this.ArrangeSideBySideButton.Location = new System.Drawing.Point(309, 117);
			this.ArrangeSideBySideButton.Name = "ArrangeSideBySideButton";
			this.ArrangeSideBySideButton.Size = new System.Drawing.Size(18, 18);
			this.ArrangeSideBySideButton.TabIndex = 13;
			this.ArrangeSideBySideButton.ToolTip = "Side-by-Side";
			this.ArrangeSideBySideButton.Click += new System.EventHandler(this.ArrangeSideBySideButton_Click);
			// 
			// ArrangeEvenlyButton
			// 
			this.ArrangeEvenlyButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.ArrangeEvenlyButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("ArrangeEvenlyButton.ImageOptions.Image")));
			this.ArrangeEvenlyButton.Location = new System.Drawing.Point(285, 117);
			this.ArrangeEvenlyButton.Name = "ArrangeEvenlyButton";
			this.ArrangeEvenlyButton.Size = new System.Drawing.Size(18, 18);
			this.ArrangeEvenlyButton.TabIndex = 12;
			this.ArrangeEvenlyButton.ToolTip = "Evenly";
			this.ArrangeEvenlyButton.Click += new System.EventHandler(this.ArrangeEvenlyButton_Click);
			// 
			// FiltersButton
			// 
			this.FiltersButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.FiltersButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("FiltersButton.ImageOptions.Image")));
			this.FiltersButton.Location = new System.Drawing.Point(71, 117);
			this.FiltersButton.Name = "FiltersButton";
			this.FiltersButton.Size = new System.Drawing.Size(18, 18);
			this.FiltersButton.TabIndex = 0;
			this.FiltersButton.ToolTip = "Filters";
			this.FiltersButton.Click += new System.EventHandler(this.FiltersButton_Click);
			// 
			// DetailsOnDemandButton
			// 
			this.DetailsOnDemandButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.DetailsOnDemandButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("DetailsOnDemandButton.ImageOptions.Image")));
			this.DetailsOnDemandButton.Location = new System.Drawing.Point(93, 117);
			this.DetailsOnDemandButton.Name = "DetailsOnDemandButton";
			this.DetailsOnDemandButton.Size = new System.Drawing.Size(18, 18);
			this.DetailsOnDemandButton.TabIndex = 1;
			this.DetailsOnDemandButton.ToolTip = "Details-on-Demand";
			this.DetailsOnDemandButton.Click += new System.EventHandler(this.DetailsOnDemandButton_Click);
			// 
			// VisPropsButton
			// 
			this.VisPropsButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.VisPropsButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("VisPropsButton.ImageOptions.Image")));
			this.VisPropsButton.Location = new System.Drawing.Point(0, 1);
			this.VisPropsButton.Name = "VisPropsButton";
			this.VisPropsButton.Size = new System.Drawing.Size(114, 18);
			this.VisPropsButton.TabIndex = 10;
			this.VisPropsButton.Text = "Visual Properties";
			this.VisPropsButton.ToolTip = "Visualization Properties";
			this.VisPropsButton.Visible = false;
			this.VisPropsButton.Click += new System.EventHandler(this.VisPropsButton_Click);
			// 
			// TableVisualButton
			// 
			this.TableVisualButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.TableVisualButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("TableVisualButton.ImageOptions.Image")));
			this.TableVisualButton.Location = new System.Drawing.Point(128, 117);
			this.TableVisualButton.Name = "TableVisualButton";
			this.TableVisualButton.Size = new System.Drawing.Size(18, 18);
			this.TableVisualButton.TabIndex = 3;
			this.TableVisualButton.ToolTip = "Table";
			this.TableVisualButton.Click += new System.EventHandler(this.TableVisualButton_Click);
			// 
			// TrellisCardButton
			// 
			this.TrellisCardButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.TrellisCardButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("TrellisCardButton.ImageOptions.Image")));
			this.TrellisCardButton.Location = new System.Drawing.Point(246, 117);
			this.TrellisCardButton.Name = "TrellisCardButton";
			this.TrellisCardButton.Size = new System.Drawing.Size(18, 18);
			this.TrellisCardButton.TabIndex = 8;
			this.TrellisCardButton.ToolTip = "Trellis Card Visualization";
			this.TrellisCardButton.Click += new System.EventHandler(this.TrellisCardButton_Click);
			// 
			// BarChartButton
			// 
			this.BarChartButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.BarChartButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("BarChartButton.ImageOptions.Image")));
			this.BarChartButton.Location = new System.Drawing.Point(151, 117);
			this.BarChartButton.Name = "BarChartButton";
			this.BarChartButton.Size = new System.Drawing.Size(18, 18);
			this.BarChartButton.TabIndex = 4;
			this.BarChartButton.Text = "Bar Chart";
			this.BarChartButton.Click += new System.EventHandler(this.BarChartButton_Click);
			// 
			// HeatMapButton
			// 
			this.HeatMapButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.HeatMapButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("HeatMapButton.ImageOptions.Image")));
			this.HeatMapButton.Location = new System.Drawing.Point(222, 117);
			this.HeatMapButton.Name = "HeatMapButton";
			this.HeatMapButton.Size = new System.Drawing.Size(18, 18);
			this.HeatMapButton.TabIndex = 7;
			this.HeatMapButton.ToolTip = "Heat Map";
			this.HeatMapButton.Click += new System.EventHandler(this.HeatMapButton_Click);
			// 
			// ScatterPlotButton
			// 
			this.ScatterPlotButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.ScatterPlotButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("ScatterPlotButton.ImageOptions.Image")));
			this.ScatterPlotButton.Location = new System.Drawing.Point(175, 117);
			this.ScatterPlotButton.Name = "ScatterPlotButton";
			this.ScatterPlotButton.Size = new System.Drawing.Size(18, 18);
			this.ScatterPlotButton.TabIndex = 5;
			this.ScatterPlotButton.ToolTip = "Scatter Plot";
			this.ScatterPlotButton.Click += new System.EventHandler(this.ScatterPlotButton_Click);
			// 
			// TreemapButton
			// 
			this.TreemapButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.TreemapButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("TreemapButton.ImageOptions.Image")));
			this.TreemapButton.Location = new System.Drawing.Point(198, 117);
			this.TreemapButton.Name = "TreemapButton";
			this.TreemapButton.Size = new System.Drawing.Size(18, 18);
			this.TreemapButton.TabIndex = 6;
			this.TreemapButton.ToolTip = "Treemap";
			this.TreemapButton.Click += new System.EventHandler(this.TreemapButton_Click);
			// 
			// DataButton
			// 
			this.DataButton.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.DataButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("DataButton.ImageOptions.Image")));
			this.DataButton.Location = new System.Drawing.Point(106, 1);
			this.DataButton.Name = "DataButton";
			this.DataButton.Size = new System.Drawing.Size(97, 18);
			this.DataButton.TabIndex = 17;
			this.DataButton.Text = "Data Mapping";
			this.DataButton.ToolTip = "Data Properties";
			this.DataButton.Click += new System.EventHandler(this.DataButton_Click);
			// 
			// SpotfireToolbar
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.DataButton);
			this.Controls.Add(this.separatorControl4);
			this.Controls.Add(this.OpenDocumentButton);
			this.Controls.Add(this.ArrangeMaximizeActive);
			this.Controls.Add(this.ArrangeStackedButton);
			this.Controls.Add(this.separatorControl2);
			this.Controls.Add(this.ArrangeSideBySideButton);
			this.Controls.Add(this.separatorControl1);
			this.Controls.Add(this.ArrangeEvenlyButton);
			this.Controls.Add(this.FiltersButton);
			this.Controls.Add(this.separatorControl3);
			this.Controls.Add(this.DetailsOnDemandButton);
			this.Controls.Add(this.VisPropsButton);
			this.Controls.Add(this.TableVisualButton);
			this.Controls.Add(this.TrellisCardButton);
			this.Controls.Add(this.BarChartButton);
			this.Controls.Add(this.HeatMapButton);
			this.Controls.Add(this.ScatterPlotButton);
			this.Controls.Add(this.TreemapButton);
			this.Name = "SpotfireToolbar";
			this.Size = new System.Drawing.Size(244, 20);
			((System.ComponentModel.ISupportInitialize)(this.separatorControl4)).EndInit();
			this.OpenDocMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.separatorControl3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.separatorControl1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.separatorControl2)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		public DevExpress.XtraEditors.SeparatorControl separatorControl1;
		public DevExpress.XtraEditors.SimpleButton DetailsOnDemandButton;
		public DevExpress.XtraEditors.SimpleButton FiltersButton;
		public DevExpress.XtraEditors.SimpleButton TreemapButton;
		public DevExpress.XtraEditors.SimpleButton ScatterPlotButton;
		public DevExpress.XtraEditors.SimpleButton BarChartButton;
		public DevExpress.XtraEditors.SimpleButton TableVisualButton;
		public DevExpress.XtraEditors.SimpleButton ArrangeMaximizeActive;
		public DevExpress.XtraEditors.SimpleButton ArrangeStackedButton;
		public DevExpress.XtraEditors.SimpleButton ArrangeSideBySideButton;
		public DevExpress.XtraEditors.SimpleButton ArrangeEvenlyButton;
		public DevExpress.XtraEditors.SeparatorControl separatorControl3;
		public DevExpress.XtraEditors.SimpleButton VisPropsButton;
		public DevExpress.XtraEditors.SimpleButton TrellisCardButton;
		public DevExpress.XtraEditors.SimpleButton HeatMapButton;
		public DevExpress.XtraEditors.SeparatorControl separatorControl2;
		public DevExpress.XtraEditors.SeparatorControl separatorControl4;
		public DevExpress.XtraEditors.DropDownButton OpenDocumentButton;
		public System.Windows.Forms.ContextMenuStrip OpenDocMenu;
		public System.Windows.Forms.ToolStripMenuItem OpenFromFileMenuItem;
		public System.Windows.Forms.ToolStripMenuItem OpenFromLibraryMenuItem;
		public DevExpress.XtraEditors.SimpleButton DataButton;
	}
}
