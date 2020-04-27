namespace Mobius.SpotfireClient
{
	partial class ScaleAxisPanel
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScaleAxisPanel));
			this.ReverseScale = new DevExpress.XtraEditors.CheckEdit();
			this.LogScale = new DevExpress.XtraEditors.CheckEdit();
			this.ShowZoomSlider = new DevExpress.XtraEditors.CheckEdit();
			this.IncludeOrigin = new DevExpress.XtraEditors.CheckEdit();
			this.ShowGridLines = new DevExpress.XtraEditors.CheckEdit();
			this.ShowLabels = new DevExpress.XtraEditors.CheckEdit();
			this.HorizontalLabels = new DevExpress.XtraEditors.CheckEdit();
			this.VerticalLabels = new DevExpress.XtraEditors.CheckEdit();
			this.RangeGroupBox = new System.Windows.Forms.GroupBox();
			this.SetToCurrentRangeButton = new DevExpress.XtraEditors.SimpleButton();
			this.RangeMax = new DevExpress.XtraEditors.TextEdit();
			this.RangeMin = new DevExpress.XtraEditors.TextEdit();
			this.MaxLabelControl = new DevExpress.XtraEditors.LabelControl();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.LabelsGroupBox = new System.Windows.Forms.GroupBox();
			this.MaxNumberOfLabels = new DevExpress.XtraEditors.CheckEdit();
			this.MaxNumberOfTicks = new DevExpress.XtraEditors.SpinEdit();
			this.ScalingGroupBox = new System.Windows.Forms.GroupBox();
			this.AxisGroupControl = new DevExpress.XtraEditors.GroupControl();
			this.AxisModeButton = new DevExpress.XtraEditors.SimpleButton();
			this.ColumnSelector = new Mobius.SpotfireClient.ColumnSelectorControl();
			this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
			this.AxisModeMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ContinuousAxisMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.CategoricalAxisMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.ReverseScale.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LogScale.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ShowZoomSlider.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IncludeOrigin.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ShowGridLines.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ShowLabels.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HorizontalLabels.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.VerticalLabels.Properties)).BeginInit();
			this.RangeGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RangeMax.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RangeMin.Properties)).BeginInit();
			this.LabelsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MaxNumberOfLabels.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MaxNumberOfTicks.Properties)).BeginInit();
			this.ScalingGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AxisGroupControl)).BeginInit();
			this.AxisGroupControl.SuspendLayout();
			this.AxisModeMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// ReverseScale
			// 
			this.ReverseScale.Location = new System.Drawing.Point(6, 46);
			this.ReverseScale.Name = "ReverseScale";
			this.ReverseScale.Properties.AutoWidth = true;
			this.ReverseScale.Properties.Caption = "Reverse scale";
			this.ReverseScale.Size = new System.Drawing.Size(89, 19);
			this.ReverseScale.TabIndex = 19;
			this.ReverseScale.EditValueChanged += new System.EventHandler(this.ReverseScale_EditValueChanged);
			// 
			// LogScale
			// 
			this.LogScale.Location = new System.Drawing.Point(6, 21);
			this.LogScale.Name = "LogScale";
			this.LogScale.Properties.AutoWidth = true;
			this.LogScale.Properties.Caption = "Log scale";
			this.LogScale.Size = new System.Drawing.Size(66, 19);
			this.LogScale.TabIndex = 18;
			this.LogScale.EditValueChanged += new System.EventHandler(this.LogScale_EditValueChanged);
			// 
			// ShowZoomSlider
			// 
			this.ShowZoomSlider.Location = new System.Drawing.Point(6, 119);
			this.ShowZoomSlider.Name = "ShowZoomSlider";
			this.ShowZoomSlider.Properties.AutoWidth = true;
			this.ShowZoomSlider.Properties.Caption = "Show zoom slider";
			this.ShowZoomSlider.Size = new System.Drawing.Size(104, 19);
			this.ShowZoomSlider.TabIndex = 16;
			this.ShowZoomSlider.EditValueChanged += new System.EventHandler(this.ShowZoomSlider_EditValueChanged);
			// 
			// IncludeOrigin
			// 
			this.IncludeOrigin.Location = new System.Drawing.Point(6, 94);
			this.IncludeOrigin.Name = "IncludeOrigin";
			this.IncludeOrigin.Properties.AutoWidth = true;
			this.IncludeOrigin.Properties.Caption = "Include origin";
			this.IncludeOrigin.Size = new System.Drawing.Size(86, 19);
			this.IncludeOrigin.TabIndex = 15;
			this.IncludeOrigin.EditValueChanged += new System.EventHandler(this.IncludeOrigin_EditValueChanged);
			// 
			// ShowGridLines
			// 
			this.ShowGridLines.Location = new System.Drawing.Point(6, 144);
			this.ShowGridLines.Name = "ShowGridLines";
			this.ShowGridLines.Properties.AutoWidth = true;
			this.ShowGridLines.Properties.Caption = "Show grid lines";
			this.ShowGridLines.Size = new System.Drawing.Size(93, 19);
			this.ShowGridLines.TabIndex = 22;
			this.ShowGridLines.EditValueChanged += new System.EventHandler(this.ShowGridLines_EditValueChanged);
			// 
			// ShowLabels
			// 
			this.ShowLabels.EditValue = true;
			this.ShowLabels.Location = new System.Drawing.Point(6, 20);
			this.ShowLabels.Name = "ShowLabels";
			this.ShowLabels.Properties.AutoWidth = true;
			this.ShowLabels.Properties.Caption = "Show labels";
			this.ShowLabels.Size = new System.Drawing.Size(78, 19);
			this.ShowLabels.TabIndex = 23;
			this.ShowLabels.EditValueChanged += new System.EventHandler(this.ShowLabels_EditValueChanged);
			// 
			// HorizontalLabels
			// 
			this.HorizontalLabels.EditValue = true;
			this.HorizontalLabels.Location = new System.Drawing.Point(21, 41);
			this.HorizontalLabels.Name = "HorizontalLabels";
			this.HorizontalLabels.Properties.AutoWidth = true;
			this.HorizontalLabels.Properties.Caption = "Horizontally";
			this.HorizontalLabels.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.HorizontalLabels.Properties.RadioGroupIndex = 1;
			this.HorizontalLabels.Size = new System.Drawing.Size(78, 19);
			this.HorizontalLabels.TabIndex = 24;
			this.HorizontalLabels.EditValueChanged += new System.EventHandler(this.HorizontalLabels_EditValueChanged);
			// 
			// VerticalLabels
			// 
			this.VerticalLabels.Location = new System.Drawing.Point(21, 62);
			this.VerticalLabels.Name = "VerticalLabels";
			this.VerticalLabels.Properties.AutoWidth = true;
			this.VerticalLabels.Properties.Caption = "Vertically";
			this.VerticalLabels.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.VerticalLabels.Properties.RadioGroupIndex = 1;
			this.VerticalLabels.Size = new System.Drawing.Size(65, 19);
			this.VerticalLabels.TabIndex = 25;
			this.VerticalLabels.TabStop = false;
			this.VerticalLabels.EditValueChanged += new System.EventHandler(this.VerticalLabels_EditValueChanged);
			// 
			// RangeGroupBox
			// 
			this.RangeGroupBox.Controls.Add(this.ShowGridLines);
			this.RangeGroupBox.Controls.Add(this.SetToCurrentRangeButton);
			this.RangeGroupBox.Controls.Add(this.RangeMax);
			this.RangeGroupBox.Controls.Add(this.RangeMin);
			this.RangeGroupBox.Controls.Add(this.MaxLabelControl);
			this.RangeGroupBox.Controls.Add(this.IncludeOrigin);
			this.RangeGroupBox.Controls.Add(this.labelControl1);
			this.RangeGroupBox.Controls.Add(this.ShowZoomSlider);
			this.RangeGroupBox.Location = new System.Drawing.Point(8, 82);
			this.RangeGroupBox.Name = "RangeGroupBox";
			this.RangeGroupBox.Size = new System.Drawing.Size(250, 169);
			this.RangeGroupBox.TabIndex = 26;
			this.RangeGroupBox.TabStop = false;
			this.RangeGroupBox.Text = "Range";
			// 
			// SetToCurrentRangeButton
			// 
			this.SetToCurrentRangeButton.Location = new System.Drawing.Point(8, 63);
			this.SetToCurrentRangeButton.Name = "SetToCurrentRangeButton";
			this.SetToCurrentRangeButton.Size = new System.Drawing.Size(230, 22);
			this.SetToCurrentRangeButton.TabIndex = 23;
			this.SetToCurrentRangeButton.Text = "Set to Current Range";
			this.SetToCurrentRangeButton.Click += new System.EventHandler(this.SetToCurrentRangeButton_Click);
			// 
			// RangeMax
			// 
			this.RangeMax.Location = new System.Drawing.Point(135, 36);
			this.RangeMax.Name = "RangeMax";
			this.RangeMax.Size = new System.Drawing.Size(104, 20);
			this.RangeMax.TabIndex = 3;
			this.RangeMax.EditValueChanged += new System.EventHandler(this.RangeMax_EditValueChanged);
			// 
			// RangeMin
			// 
			this.RangeMin.Location = new System.Drawing.Point(8, 36);
			this.RangeMin.Name = "RangeMin";
			this.RangeMin.Size = new System.Drawing.Size(104, 20);
			this.RangeMin.TabIndex = 2;
			this.RangeMin.EditValueChanged += new System.EventHandler(this.RangeMin_EditValueChanged);
			// 
			// MaxLabelControl
			// 
			this.MaxLabelControl.Location = new System.Drawing.Point(135, 18);
			this.MaxLabelControl.Name = "MaxLabelControl";
			this.MaxLabelControl.Size = new System.Drawing.Size(48, 13);
			this.MaxLabelControl.TabIndex = 1;
			this.MaxLabelControl.Text = "Maximum:";
			// 
			// labelControl1
			// 
			this.labelControl1.Location = new System.Drawing.Point(8, 18);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(44, 13);
			this.labelControl1.TabIndex = 0;
			this.labelControl1.Text = "Minimum:";
			// 
			// LabelsGroupBox
			// 
			this.LabelsGroupBox.Controls.Add(this.MaxNumberOfLabels);
			this.LabelsGroupBox.Controls.Add(this.MaxNumberOfTicks);
			this.LabelsGroupBox.Controls.Add(this.ShowLabels);
			this.LabelsGroupBox.Controls.Add(this.HorizontalLabels);
			this.LabelsGroupBox.Controls.Add(this.VerticalLabels);
			this.LabelsGroupBox.Location = new System.Drawing.Point(277, 82);
			this.LabelsGroupBox.Name = "LabelsGroupBox";
			this.LabelsGroupBox.Size = new System.Drawing.Size(159, 169);
			this.LabelsGroupBox.TabIndex = 27;
			this.LabelsGroupBox.TabStop = false;
			this.LabelsGroupBox.Text = "Labels";
			// 
			// MaxNumberOfLabels
			// 
			this.MaxNumberOfLabels.Location = new System.Drawing.Point(6, 94);
			this.MaxNumberOfLabels.Name = "MaxNumberOfLabels";
			this.MaxNumberOfLabels.Properties.AutoWidth = true;
			this.MaxNumberOfLabels.Properties.Caption = "Max number of labels";
			this.MaxNumberOfLabels.Size = new System.Drawing.Size(124, 19);
			this.MaxNumberOfLabels.TabIndex = 32;
			this.MaxNumberOfLabels.TabStop = false;
			this.MaxNumberOfLabels.EditValueChanged += new System.EventHandler(this.MaxNumberOfLabels_EditValueChanged);
			// 
			// MaxNumberOfTicks
			// 
			this.MaxNumberOfTicks.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
			this.MaxNumberOfTicks.Location = new System.Drawing.Point(21, 118);
			this.MaxNumberOfTicks.Name = "MaxNumberOfTicks";
			this.MaxNumberOfTicks.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
			this.MaxNumberOfTicks.Properties.IsFloatValue = false;
			this.MaxNumberOfTicks.Properties.Mask.EditMask = "N00";
			this.MaxNumberOfTicks.Size = new System.Drawing.Size(50, 20);
			this.MaxNumberOfTicks.TabIndex = 29;
			this.MaxNumberOfTicks.EditValueChanged += new System.EventHandler(this.MaxNumberOfTicks_EditValueChanged);
			// 
			// ScalingGroupBox
			// 
			this.ScalingGroupBox.Controls.Add(this.LogScale);
			this.ScalingGroupBox.Controls.Add(this.ReverseScale);
			this.ScalingGroupBox.Location = new System.Drawing.Point(8, 261);
			this.ScalingGroupBox.Name = "ScalingGroupBox";
			this.ScalingGroupBox.Size = new System.Drawing.Size(428, 75);
			this.ScalingGroupBox.TabIndex = 28;
			this.ScalingGroupBox.TabStop = false;
			this.ScalingGroupBox.Text = "Scaling";
			// 
			// AxisGroupControl
			// 
			this.AxisGroupControl.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AxisGroupControl.AppearanceCaption.Options.UseFont = true;
			this.AxisGroupControl.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
			this.AxisGroupControl.Controls.Add(this.AxisModeButton);
			this.AxisGroupControl.Controls.Add(this.ColumnSelector);
			this.AxisGroupControl.Controls.Add(this.labelControl5);
			this.AxisGroupControl.Controls.Add(this.RangeGroupBox);
			this.AxisGroupControl.Controls.Add(this.ScalingGroupBox);
			this.AxisGroupControl.Controls.Add(this.LabelsGroupBox);
			this.AxisGroupControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AxisGroupControl.Location = new System.Drawing.Point(0, 0);
			this.AxisGroupControl.Name = "AxisGroupControl";
			this.AxisGroupControl.Size = new System.Drawing.Size(622, 416);
			this.AxisGroupControl.TabIndex = 29;
			this.AxisGroupControl.Text = "Axis";
			// 
			// AxisModeButton
			// 
			this.AxisModeButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("AxisModeButton.ImageOptions.Image")));
			this.AxisModeButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleRight;
			this.AxisModeButton.Location = new System.Drawing.Point(451, 46);
			this.AxisModeButton.Name = "AxisModeButton";
			this.AxisModeButton.Size = new System.Drawing.Size(74, 25);
			this.AxisModeButton.TabIndex = 223;
			this.AxisModeButton.Text = "Axis Mode";
			this.AxisModeButton.Click += new System.EventHandler(this.AxisModeButton_Click);
			// 
			// ColumnSelector
			// 
			this.ColumnSelector.Location = new System.Drawing.Point(8, 46);
			this.ColumnSelector.Margin = new System.Windows.Forms.Padding(0);
			this.ColumnSelector.Name = "ColumnSelector";
			this.ColumnSelector.OptionIncludeNoneItem = false;
			this.ColumnSelector.Size = new System.Drawing.Size(428, 28);
			this.ColumnSelector.TabIndex = 222;
			// 
			// labelControl5
			// 
			this.labelControl5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl5.Location = new System.Drawing.Point(8, 30);
			this.labelControl5.Name = "labelControl5";
			this.labelControl5.Size = new System.Drawing.Size(44, 13);
			this.labelControl5.TabIndex = 221;
			this.labelControl5.Text = "Columns:";
			// 
			// AxisModeMenu
			// 
			this.AxisModeMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ContinuousAxisMenuItem,
            this.CategoricalAxisMenuItem});
			this.AxisModeMenu.Name = "AxisModeMenu";
			this.AxisModeMenu.ShowCheckMargin = true;
			this.AxisModeMenu.Size = new System.Drawing.Size(203, 70);
			// 
			// ContinuousAxisMenuItem
			// 
			this.ContinuousAxisMenuItem.Name = "ContinuousAxisMenuItem";
			this.ContinuousAxisMenuItem.Size = new System.Drawing.Size(202, 22);
			this.ContinuousAxisMenuItem.Text = "Continuous";
			this.ContinuousAxisMenuItem.Click += new System.EventHandler(this.ContinuousAxisMenuItem_Click);
			// 
			// CategoricalAxisMenuItem
			// 
			this.CategoricalAxisMenuItem.Name = "CategoricalAxisMenuItem";
			this.CategoricalAxisMenuItem.Size = new System.Drawing.Size(202, 22);
			this.CategoricalAxisMenuItem.Text = "Categorical";
			this.CategoricalAxisMenuItem.Click += new System.EventHandler(this.CategoricalAxisMenuItem_Click);
			// 
			// ScaleAxisPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.AxisGroupControl);
			this.Name = "ScaleAxisPanel";
			this.Size = new System.Drawing.Size(622, 416);
			((System.ComponentModel.ISupportInitialize)(this.ReverseScale.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LogScale.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ShowZoomSlider.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IncludeOrigin.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ShowGridLines.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ShowLabels.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HorizontalLabels.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.VerticalLabels.Properties)).EndInit();
			this.RangeGroupBox.ResumeLayout(false);
			this.RangeGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RangeMax.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RangeMin.Properties)).EndInit();
			this.LabelsGroupBox.ResumeLayout(false);
			this.LabelsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MaxNumberOfLabels.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MaxNumberOfTicks.Properties)).EndInit();
			this.ScalingGroupBox.ResumeLayout(false);
			this.ScalingGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AxisGroupControl)).EndInit();
			this.AxisGroupControl.ResumeLayout(false);
			this.AxisGroupControl.PerformLayout();
			this.AxisModeMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		internal DevExpress.XtraEditors.CheckEdit ReverseScale;
		internal DevExpress.XtraEditors.CheckEdit LogScale;
		internal DevExpress.XtraEditors.CheckEdit ShowZoomSlider;
		internal DevExpress.XtraEditors.CheckEdit IncludeOrigin;
		internal DevExpress.XtraEditors.CheckEdit ShowGridLines;
		internal DevExpress.XtraEditors.CheckEdit ShowLabels;
		internal DevExpress.XtraEditors.CheckEdit HorizontalLabels;
		internal DevExpress.XtraEditors.CheckEdit VerticalLabels;
		internal System.Windows.Forms.GroupBox RangeGroupBox;
		public DevExpress.XtraEditors.TextEdit RangeMax;
		public DevExpress.XtraEditors.TextEdit RangeMin;
		private DevExpress.XtraEditors.LabelControl MaxLabelControl;
		private DevExpress.XtraEditors.LabelControl labelControl1;
		private System.Windows.Forms.GroupBox LabelsGroupBox;
		private System.Windows.Forms.GroupBox ScalingGroupBox;
		private DevExpress.XtraEditors.SimpleButton SetToCurrentRangeButton;
		private DevExpress.XtraEditors.SpinEdit MaxNumberOfTicks;
		internal DevExpress.XtraEditors.CheckEdit MaxNumberOfLabels;
		private DevExpress.XtraEditors.GroupControl AxisGroupControl;
		private SpotfireClient.ColumnSelectorControl ColumnSelector;
		private DevExpress.XtraEditors.LabelControl labelControl5;
		private DevExpress.XtraEditors.SimpleButton AxisModeButton;
		private System.Windows.Forms.ContextMenuStrip AxisModeMenu;
		private System.Windows.Forms.ToolStripMenuItem ContinuousAxisMenuItem;
		private System.Windows.Forms.ToolStripMenuItem CategoricalAxisMenuItem;
	}
}
