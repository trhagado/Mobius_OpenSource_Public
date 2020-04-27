namespace Mobius.ClientComponents
{
	partial class CriteriaStructureRangeCtl
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
			this.RangeLabel = new DevExpress.XtraEditors.LabelControl();
			this.TrackBar = new DevExpress.XtraEditors.RangeTrackBarControl();
			this.ToolTipController = new DevExpress.Utils.ToolTipController(this.components);
			this.BarLabel = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.TrackBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TrackBar.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// RangeLabel
			// 
			this.RangeLabel.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.RangeLabel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RangeLabel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.RangeLabel.Cursor = System.Windows.Forms.Cursors.Default;
			this.RangeLabel.Location = new System.Drawing.Point(87, 5);
			this.RangeLabel.Name = "RangeLabel";
			this.RangeLabel.Size = new System.Drawing.Size(38, 13);
			this.RangeLabel.TabIndex = 48;
			this.RangeLabel.Text = "0-10 Up";
			// 
			// TrackBar
			// 
			this.TrackBar.EditValue = new DevExpress.XtraEditors.Repository.TrackBarRange(0, 10);
			this.TrackBar.Location = new System.Drawing.Point(0, -3);
			this.TrackBar.Name = "TrackBar";
			this.TrackBar.Properties.AutoSize = false;
			this.TrackBar.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.TrackBar.Properties.LabelAppearance.Options.UseTextOptions = true;
			this.TrackBar.Properties.LabelAppearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.TrackBar.Properties.LookAndFeel.SkinName = "Seven";
			this.TrackBar.Properties.LookAndFeel.UseDefaultLookAndFeel = false;
			this.TrackBar.Properties.TickFrequency = 10;
			this.TrackBar.Properties.TickStyle = System.Windows.Forms.TickStyle.Both;
			this.TrackBar.Size = new System.Drawing.Size(82, 29);
			this.TrackBar.TabIndex = 46;
			this.TrackBar.Value = new DevExpress.XtraEditors.Repository.TrackBarRange(0, 10);
			this.TrackBar.EditValueChanged += new System.EventHandler(this.Range1_EditValueChanged);
			// 
			// BarLabel
			// 
			this.BarLabel.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.BarLabel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.BarLabel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.BarLabel.AppearanceDisabled.BackColor = System.Drawing.Color.Transparent;
			this.BarLabel.Cursor = System.Windows.Forms.Cursors.Default;
			this.BarLabel.Location = new System.Drawing.Point(29, 5);
			this.BarLabel.Name = "BarLabel";
			this.BarLabel.Size = new System.Drawing.Size(25, 13);
			this.BarLabel.TabIndex = 51;
			this.BarLabel.Text = "Fixed";
			// 
			// CriteriaStructureRangeCtl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.BarLabel);
			this.Controls.Add(this.RangeLabel);
			this.Controls.Add(this.TrackBar);
			this.Name = "CriteriaStructureRangeCtl";
			this.Size = new System.Drawing.Size(133, 24);
			((System.ComponentModel.ISupportInitialize)(this.TrackBar.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TrackBar)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal DevExpress.Utils.ToolTipController ToolTipController;
		internal DevExpress.XtraEditors.LabelControl BarLabel;
		internal DevExpress.XtraEditors.LabelControl RangeLabel;
		internal DevExpress.XtraEditors.RangeTrackBarControl TrackBar;
	}
}
