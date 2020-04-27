namespace Mobius.ClientComponents
{
	partial class ZoomControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZoomControl));
			this.Bitmaps16x16 = new DevExpress.Utils.ImageCollection(this.components);
			this.ZoomSlider = new DevExpress.XtraEditors.ZoomTrackBarControl();
			this.ZoomPctTextEdit = new DevExpress.XtraEditors.TextEdit();
			((System.ComponentModel.ISupportInitialize)(this.Bitmaps16x16)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ZoomSlider)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ZoomSlider.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ZoomPctTextEdit.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((DevExpress.Utils.ImageCollectionStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "MouseZoom2.bmp");
			// 
			// ZoomSlider
			// 
			this.ZoomSlider.EditValue = null;
			this.ZoomSlider.Location = new System.Drawing.Point(50, -1);
			this.ZoomSlider.Name = "ZoomSlider";
			this.ZoomSlider.Properties.Maximum = 100;
			this.ZoomSlider.Properties.ScrollThumbStyle = DevExpress.XtraEditors.Repository.ScrollThumbStyle.ArrowDownRight;
			this.ZoomSlider.Size = new System.Drawing.Size(135, 23);
			this.ZoomSlider.TabIndex = 2;
			this.ZoomSlider.ValueChanged += new System.EventHandler(this.ZoomSlider_ValueChanged);
			// 
			// ZoomPctTextEdit
			// 
			this.ZoomPctTextEdit.EditValue = "100%";
			this.ZoomPctTextEdit.Location = new System.Drawing.Point(0, 0);
			this.ZoomPctTextEdit.Name = "ZoomPctTextEdit";
			this.ZoomPctTextEdit.Properties.Appearance.Options.UseTextOptions = true;
			this.ZoomPctTextEdit.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
			this.ZoomPctTextEdit.Size = new System.Drawing.Size(41, 20);
			this.ZoomPctTextEdit.TabIndex = 10;
			this.ZoomPctTextEdit.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ZoomPctTextEdit_KeyDown);
			this.ZoomPctTextEdit.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ZoomPctTextEdit_MouseDown);
			// 
			// ZoomControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ZoomPctTextEdit);
			this.Controls.Add(this.ZoomSlider);
			this.Name = "ZoomControl";
			this.Size = new System.Drawing.Size(190, 20);
			((System.ComponentModel.ISupportInitialize)(this.Bitmaps16x16)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ZoomSlider.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ZoomSlider)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ZoomPctTextEdit.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DevExpress.XtraEditors.ZoomTrackBarControl ZoomSlider;
		private DevExpress.Utils.ImageCollection Bitmaps16x16;
		private DevExpress.XtraEditors.TextEdit ZoomPctTextEdit;
	}
}
