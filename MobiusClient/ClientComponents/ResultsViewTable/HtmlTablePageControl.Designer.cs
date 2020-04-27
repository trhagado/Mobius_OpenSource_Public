namespace Mobius.ClientComponents
{
	partial class HtmlTablePageControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HtmlTablePageControl));
			this.ToolPanel = new DevExpress.XtraEditors.PanelControl();
			this.Bitmaps8x8 = new System.Windows.Forms.ImageList(this.components);
			this.BackBut = new DevExpress.XtraEditors.SimpleButton();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.ForwardBut = new DevExpress.XtraEditors.SimpleButton();
			this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
			this.HtmlPagePanel = new Mobius.ClientComponents.HtmlTablePanel();
			((System.ComponentModel.ISupportInitialize)(this.ToolPanel)).BeginInit();
			this.ToolPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// ToolPanel
			// 
			this.ToolPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ToolPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.ToolPanel.Controls.Add(this.BackBut);
			this.ToolPanel.Controls.Add(this.ForwardBut);
			this.ToolPanel.Controls.Add(this.simpleButton1);
			this.ToolPanel.Location = new System.Drawing.Point(432, 0);
			this.ToolPanel.Name = "ToolPanel";
			this.ToolPanel.Size = new System.Drawing.Size(76, 24);
			this.ToolPanel.TabIndex = 222;
			// 
			// Bitmaps8x8
			// 
			this.Bitmaps8x8.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps8x8.ImageStream")));
			this.Bitmaps8x8.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps8x8.Images.SetKeyName(0, "");
			this.Bitmaps8x8.Images.SetKeyName(1, "");
			this.Bitmaps8x8.Images.SetKeyName(2, "ScrollDown.bmp");
			this.Bitmaps8x8.Images.SetKeyName(3, "ScrollUp.bmp");
			this.Bitmaps8x8.Images.SetKeyName(4, "DropDown8x8.bmp");
			// 
			// BackBut
			// 
			this.BackBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BackBut.ImageIndex = 0;
			this.BackBut.ImageList = this.Bitmaps16x16;
			this.BackBut.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.BackBut.Location = new System.Drawing.Point(2, 1);
			this.BackBut.LookAndFeel.SkinName = "Money Twins";
			this.BackBut.Name = "BackBut";
			this.BackBut.Size = new System.Drawing.Size(22, 22);
			this.BackBut.TabIndex = 223;
			this.BackBut.ToolTip = "Previous page";
			this.BackBut.Click += new System.EventHandler(this.BackBut_Click);
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "Previous.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "Next.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "Print.bmp");
			// 
			// ForwardBut
			// 
			this.ForwardBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ForwardBut.ImageIndex = 1;
			this.ForwardBut.ImageList = this.Bitmaps16x16;
			this.ForwardBut.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.ForwardBut.Location = new System.Drawing.Point(28, 1);
			this.ForwardBut.LookAndFeel.SkinName = "Money Twins";
			this.ForwardBut.Name = "ForwardBut";
			this.ForwardBut.Size = new System.Drawing.Size(22, 22);
			this.ForwardBut.TabIndex = 224;
			this.ForwardBut.ToolTip = "Next page";
			this.ForwardBut.Click += new System.EventHandler(this.ForwardBut_Click);
			// 
			// simpleButton1
			// 
			this.simpleButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.simpleButton1.ImageIndex = 2;
			this.simpleButton1.ImageList = this.Bitmaps16x16;
			this.simpleButton1.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.simpleButton1.Location = new System.Drawing.Point(53, 1);
			this.simpleButton1.LookAndFeel.SkinName = "Money Twins";
			this.simpleButton1.Name = "simpleButton1";
			this.simpleButton1.Size = new System.Drawing.Size(22, 22);
			this.simpleButton1.TabIndex = 225;
			this.simpleButton1.ToolTip = "Print";
			this.simpleButton1.Click += new System.EventHandler(this.PrintBut_Click);
			// 
			// HtmlPagePanel
			// 
			this.HtmlPagePanel.Location = new System.Drawing.Point(3, 29);
			this.HtmlPagePanel.Name = "HtmlPagePanel";
			this.HtmlPagePanel.Size = new System.Drawing.Size(502, 382);
			this.HtmlPagePanel.TabIndex = 0;
			// 
			// HtmlTablePageControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ToolPanel);
			this.Controls.Add(this.HtmlPagePanel);
			this.Name = "HtmlTablePageControl";
			this.Size = new System.Drawing.Size(508, 414);
			((System.ComponentModel.ISupportInitialize)(this.ToolPanel)).EndInit();
			this.ToolPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		internal DevExpress.XtraEditors.PanelControl ToolPanel;
		public DevExpress.XtraEditors.SimpleButton ForwardBut;
		private System.Windows.Forms.ImageList Bitmaps16x16;
		public DevExpress.XtraEditors.SimpleButton BackBut;
		public DevExpress.XtraEditors.SimpleButton simpleButton1;
		internal HtmlTablePanel HtmlPagePanel;
		public System.Windows.Forms.ImageList Bitmaps8x8;
	}
}
