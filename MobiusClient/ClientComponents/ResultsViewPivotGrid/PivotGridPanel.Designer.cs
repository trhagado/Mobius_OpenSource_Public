namespace Mobius.ClientComponents
{
	partial class PivotGridPanel
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
			//DevExpress.XtraCharts.SideBySideBarSeriesLabel sideBySideBarSeriesLabel1 = new DevExpress.XtraCharts.SideBySideBarSeriesLabel();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PivotGridPanel));
			this.SplitContainerControl = new DevExpress.XtraEditors.SplitContainerControl();
			this.PivotGrid = new Mobius.ClientComponents.PivotGridControlMx();
			//this.Chart = new DevExpress.XtraCharts.ChartControl();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainerControl)).BeginInit();
			this.SplitContainerControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PivotGrid)).BeginInit();
			//((System.ComponentModel.ISupportInitialize)(this.Chart)).BeginInit();
			//((System.ComponentModel.ISupportInitialize)(sideBySideBarSeriesLabel1)).BeginInit();
			this.SuspendLayout();
			// 
			// SplitContainerControl
			// 
			this.SplitContainerControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainerControl.FixedPanel = DevExpress.XtraEditors.SplitFixedPanel.None;
			this.SplitContainerControl.Horizontal = false;
			this.SplitContainerControl.Location = new System.Drawing.Point(0, 0);
			this.SplitContainerControl.Name = "SplitContainerControl";
			this.SplitContainerControl.Panel1.Controls.Add(this.PivotGrid);
			this.SplitContainerControl.Panel1.Text = "Panel1";
			//this.SplitContainerControl.Panel2.Controls.Add(this.Chart);
			this.SplitContainerControl.Panel2.Text = "Panel2";
			this.SplitContainerControl.Size = new System.Drawing.Size(560, 451);
			this.SplitContainerControl.SplitterPosition = 260;
			this.SplitContainerControl.TabIndex = 2;
			this.SplitContainerControl.Text = "splitContainerControl1";
			// 
			// PivotGrid
			// 
			this.PivotGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PivotGrid.Location = new System.Drawing.Point(0, 0);
			this.PivotGrid.Name = "PivotGrid";
			this.PivotGrid.OptionsCustomization.CustomizationFormStyle = DevExpress.XtraPivotGrid.Customization.CustomizationFormStyle.Excel2007;
			this.PivotGrid.OptionsMenu.EnableFormatRulesMenu = true;
			this.PivotGrid.Size = new System.Drawing.Size(560, 260);
			this.PivotGrid.TabIndex = 0;
			this.PivotGrid.FieldAreaChanging += new DevExpress.XtraPivotGrid.PivotAreaChangingEventHandler(this.Grid_FieldAreaChanging);
			this.PivotGrid.CustomUnboundFieldData += new DevExpress.XtraPivotGrid.CustomFieldDataEventHandler(this.PivotGridControl_CustomUnboundFieldData);
			this.PivotGrid.CustomSummary += new DevExpress.XtraPivotGrid.PivotGridCustomSummaryEventHandler(this.Grid_CustomSummary);
			this.PivotGrid.ShowingCustomizationForm += new DevExpress.XtraPivotGrid.CustomizationFormShowingEventHandler(this.Grid_ShowingCustomizationForm);
			this.PivotGrid.FieldAreaChanged += new DevExpress.XtraPivotGrid.PivotFieldEventHandler(this.Grid_FieldAreaChanged);
			this.PivotGrid.FieldWidthChanged += new DevExpress.XtraPivotGrid.PivotFieldEventHandler(this.Grid_FieldWidthChanged);
			this.PivotGrid.FieldValueDisplayText += new DevExpress.XtraPivotGrid.PivotFieldDisplayTextEventHandler(this.Grid_FieldValueDisplayText);
			this.PivotGrid.CustomGroupInterval += new DevExpress.XtraPivotGrid.PivotCustomGroupIntervalEventHandler(this.Grid_CustomGroupInterval);
			this.PivotGrid.CustomCellDisplayText += new DevExpress.XtraPivotGrid.PivotCellDisplayTextEventHandler(this.Grid_CustomCellDisplayText);
			this.PivotGrid.CellDoubleClick += new DevExpress.XtraPivotGrid.PivotCellEventHandler(this.Grid_CellDoubleClick);
			this.PivotGrid.CellClick += new DevExpress.XtraPivotGrid.PivotCellEventHandler(this.Grid_CellClick);
			this.PivotGrid.PopupMenuShowing += new DevExpress.XtraPivotGrid.PopupMenuShowingEventHandler(this.Grid_PopupMenuShowing);
			this.PivotGrid.CustomAppearance += new DevExpress.XtraPivotGrid.PivotCustomAppearanceEventHandler(this.Grid_CustomAppearance);
			this.PivotGrid.CustomDrawFieldValue += new DevExpress.XtraPivotGrid.PivotCustomDrawFieldValueEventHandler(this.Grid_CustomDrawFieldValue);
			// 
			// Chart
			// 
			//this.Chart.Cursor = System.Windows.Forms.Cursors.Default;
			//this.Chart.Dock = System.Windows.Forms.DockStyle.Fill;
			//this.Chart.Location = new System.Drawing.Point(0, 0);
			//this.Chart.Name = "Chart";
			//this.Chart.SeriesSerializable = new DevExpress.XtraCharts.Series[0];
			//sideBySideBarSeriesLabel1.LineVisibility = DevExpress.Utils.DefaultBoolean.True;
			//this.Chart.SeriesTemplate.Label = sideBySideBarSeriesLabel1;
			//this.Chart.Size = new System.Drawing.Size(560, 185);
			//this.Chart.TabIndex = 0;
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "PivotAreaColumn.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "PivotAreaRow.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "PivotAreaData.bmp");
			this.Bitmaps16x16.Images.SetKeyName(3, "PivotAreaFilter.bmp");
			this.Bitmaps16x16.Images.SetKeyName(4, "PivotFields_16x16.png");
			// 
			// PivotGridPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.SplitContainerControl);
			this.Name = "PivotGridPanel";
			this.Size = new System.Drawing.Size(560, 451);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainerControl)).EndInit();
			this.SplitContainerControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PivotGrid)).EndInit();
			//((System.ComponentModel.ISupportInitialize)(sideBySideBarSeriesLabel1)).EndInit();
			//((System.ComponentModel.ISupportInitialize)(this.Chart)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal PivotGridControlMx PivotGrid;
		internal DevExpress.XtraEditors.SplitContainerControl SplitContainerControl;
		//internal DevExpress.XtraCharts.ChartControl Chart;
		public System.Windows.Forms.ImageList Bitmaps16x16;
	}
}
