namespace Mobius.SpotfireClient
{
	partial class DataMapDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataMapDialog));
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.CloseButton = new DevExpress.XtraEditors.SimpleButton();
			this.DataMapCtl = new Mobius.SpotfireClient.DataMapControl();
			this.SuspendLayout();
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "Add.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "Edit.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "ScrollDown16x16.bmp");
			this.Bitmaps16x16.Images.SetKeyName(3, "Delete.bmp");
			this.Bitmaps16x16.Images.SetKeyName(4, "up2.bmp");
			this.Bitmaps16x16.Images.SetKeyName(5, "down2.bmp");
			this.Bitmaps16x16.Images.SetKeyName(6, "Copy.bmp");
			this.Bitmaps16x16.Images.SetKeyName(7, "Paste.bmp");
			this.Bitmaps16x16.Images.SetKeyName(8, "Properties.bmp");
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CloseButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.CloseButton.Appearance.Options.UseFont = true;
			this.CloseButton.Appearance.Options.UseForeColor = true;
			this.CloseButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.CloseButton.Location = new System.Drawing.Point(680, 525);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CloseButton.Size = new System.Drawing.Size(61, 24);
			this.CloseButton.TabIndex = 204;
			this.CloseButton.Text = "Close";
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// DataMapControl
			// 
			this.DataMapCtl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DataMapCtl.CanEditMapping = true;
			this.DataMapCtl.Location = new System.Drawing.Point(3, 3);
			this.DataMapCtl.Name = "DataMapControl";
			this.DataMapCtl.SelectSingleColumn = false;
			this.DataMapCtl.ShowSelectedColumnCheckBoxes = true;
			this.DataMapCtl.Size = new System.Drawing.Size(738, 520);
			this.DataMapCtl.TabIndex = 205;
			// 
			// DataMapDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(747, 553);
			this.Controls.Add(this.DataMapCtl);
			this.Controls.Add(this.CloseButton);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "DataMapDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Mapping of Spotfire Data Tables to Mobius Query Tables";
			this.ResumeLayout(false);

		}

		#endregion
		public System.Windows.Forms.ImageList Bitmaps16x16;
		public DevExpress.XtraEditors.SimpleButton CloseButton;
		public Mobius.SpotfireClient.DataMapControl DataMapCtl = null;
	}
}