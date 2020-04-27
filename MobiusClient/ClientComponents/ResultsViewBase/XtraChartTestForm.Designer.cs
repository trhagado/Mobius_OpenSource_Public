namespace Mobius.ClientComponents
{
	partial class XtraChartTestForm
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
			DevExpress.XtraCharts.XYDiagram xyDiagram1 = new DevExpress.XtraCharts.XYDiagram();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XtraChartTestForm));
			DevExpress.XtraCharts.Series series1 = new DevExpress.XtraCharts.Series();
			DevExpress.XtraCharts.BubbleSeriesLabel bubbleSeriesLabel1 = new DevExpress.XtraCharts.BubbleSeriesLabel();
			DevExpress.XtraCharts.SeriesPoint seriesPoint1 = new DevExpress.XtraCharts.SeriesPoint("1", new object[] {
            ((object)(2)),
            ((object)(2))});
			DevExpress.XtraCharts.SeriesPoint seriesPoint2 = new DevExpress.XtraCharts.SeriesPoint("3", new object[] {
            ((object)(4)),
            ((object)(4))});
			DevExpress.XtraCharts.SeriesPoint seriesPoint3 = new DevExpress.XtraCharts.SeriesPoint("5", new object[] {
            ((object)(6)),
            ((object)(6))});
			DevExpress.XtraCharts.SeriesPoint seriesPoint4 = new DevExpress.XtraCharts.SeriesPoint("7", new object[] {
            ((object)(8)),
            ((object)(8))});
			DevExpress.XtraCharts.BubbleSeriesView bubbleSeriesView1 = new DevExpress.XtraCharts.BubbleSeriesView();
			DevExpress.XtraCharts.Series series2 = new DevExpress.XtraCharts.Series();
			DevExpress.XtraCharts.BubbleSeriesLabel bubbleSeriesLabel2 = new DevExpress.XtraCharts.BubbleSeriesLabel();
			DevExpress.XtraCharts.BubbleSeriesView bubbleSeriesView2 = new DevExpress.XtraCharts.BubbleSeriesView();
			DevExpress.XtraCharts.BubbleSeriesLabel bubbleSeriesLabel3 = new DevExpress.XtraCharts.BubbleSeriesLabel();
			DevExpress.XtraCharts.BubbleSeriesView bubbleSeriesView3 = new DevExpress.XtraCharts.BubbleSeriesView();
			this.chartControl1 = new DevExpress.XtraCharts.ChartControl();
			((System.ComponentModel.ISupportInitialize)(this.chartControl1)).BeginInit();
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
			// chartControl1
			// 
			this.chartControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.chartControl1.BackImage.Stretch = true;
			////xyDiagram1.AxisX.Range.ScrollingRange.SideMarginsEnabled = true;
			////xyDiagram1.AxisX.Range.SideMarginsEnabled = true;
			xyDiagram1.AxisX.VisibleInPanesSerializable = "-1";
			////xyDiagram1.AxisY.Range.ScrollingRange.SideMarginsEnabled = true;
			////xyDiagram1.AxisY.Range.SideMarginsEnabled = true;
			xyDiagram1.AxisY.VisibleInPanesSerializable = "-1";
			xyDiagram1.DefaultPane.BackImage.Image = ((System.Drawing.Image)(resources.GetObject("resource.Image")));
			xyDiagram1.DefaultPane.BackImage.Stretch = true;
			xyDiagram1.DefaultPane.EnableAxisXScrolling = DevExpress.Utils.DefaultBoolean.True;
			xyDiagram1.DefaultPane.EnableAxisXZooming = DevExpress.Utils.DefaultBoolean.True;
			xyDiagram1.DefaultPane.EnableAxisYScrolling = DevExpress.Utils.DefaultBoolean.True;
			xyDiagram1.DefaultPane.EnableAxisYZooming = DevExpress.Utils.DefaultBoolean.True;
			xyDiagram1.EnableAxisXScrolling = true;
			xyDiagram1.EnableAxisXZooming = true;
			xyDiagram1.EnableAxisYScrolling = true;
			xyDiagram1.EnableAxisYZooming = true;
			xyDiagram1.Margins.Bottom = 0;
			xyDiagram1.Margins.Left = 0;
			xyDiagram1.Margins.Right = 0;
			xyDiagram1.Margins.Top = 0;
			this.chartControl1.Diagram = xyDiagram1;
			this.chartControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chartControl1.Location = new System.Drawing.Point(0, 0);
			this.chartControl1.Name = "chartControl1";
			bubbleSeriesLabel1.LineVisibility = DevExpress.Utils.DefaultBoolean.True;
			series1.Label = bubbleSeriesLabel1;
			series1.Name = "Series 1";
			series1.Points.AddRange(new DevExpress.XtraCharts.SeriesPoint[] {
            seriesPoint1,
            seriesPoint2,
            seriesPoint3,
            seriesPoint4});
			series1.View = bubbleSeriesView1;
			bubbleSeriesLabel2.LineVisibility = DevExpress.Utils.DefaultBoolean.True;
			series2.Label = bubbleSeriesLabel2;
			series2.Name = "Series 2";
			series2.View = bubbleSeriesView2;
			this.chartControl1.SeriesSerializable = new DevExpress.XtraCharts.Series[] {
        series1,
        series2};
			bubbleSeriesLabel3.LineVisibility = DevExpress.Utils.DefaultBoolean.True;
			this.chartControl1.SeriesTemplate.Label = bubbleSeriesLabel3;
			this.chartControl1.SeriesTemplate.View = bubbleSeriesView3;
			this.chartControl1.Size = new System.Drawing.Size(813, 796);
			this.chartControl1.TabIndex = 0;
			// 
			// XtraChartView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(813, 796);
			this.Controls.Add(this.chartControl1);
			this.Name = "XtraChartView";
			this.Text = "XtraChartView";
			((System.ComponentModel.ISupportInitialize)(xyDiagram1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(bubbleSeriesLabel1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(bubbleSeriesView1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(series1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(bubbleSeriesLabel2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(bubbleSeriesView2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(series2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(bubbleSeriesLabel3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(bubbleSeriesView3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.chartControl1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraCharts.ChartControl chartControl1;
	}
}