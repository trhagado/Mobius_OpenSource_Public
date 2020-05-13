namespace Mobius.CdkMx
{
	partial class CdkMolControl
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
			this.toolTipController1 = new DevExpress.Utils.ToolTipController(this.components);
			this.ImageCtl = new DevExpress.XtraEditors.PictureEdit();
			((System.ComponentModel.ISupportInitialize)(this.ImageCtl.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// ImageCtl
			// 
			this.ImageCtl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImageCtl.Location = new System.Drawing.Point(0, 0);
			this.ImageCtl.Name = "ImageCtl";
			this.ImageCtl.Properties.NullText = " ";
			this.ImageCtl.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
			this.ImageCtl.Size = new System.Drawing.Size(361, 267);
			this.ImageCtl.TabIndex = 0;
			// 
			// CdkMolControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ImageCtl);
			this.Margin = new System.Windows.Forms.Padding(2);
			this.Name = "CdkMolControl";
			this.Size = new System.Drawing.Size(361, 267);
			this.SizeChanged += new System.EventHandler(this.CdkMolControl_SizeChanged);
			this.Click += new System.EventHandler(this.CdkMolControl_Click);
			this.DoubleClick += new System.EventHandler(this.CdkMolControl_DoubleClick);
			((System.ComponentModel.ISupportInitialize)(this.ImageCtl.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.Utils.ToolTipController toolTipController1;
		private DevExpress.XtraEditors.PictureEdit ImageCtl;
	}
}
