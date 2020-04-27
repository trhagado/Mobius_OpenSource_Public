using Mobius.ComOps;

using DevExpress.XtraEditors;

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Summary description for StructurePopup.
	/// </summary>
	public class PopupHtml : XtraForm
	{
		private static int PopupPos = 10; // incremental popup positioning
		private ImageList Bitmaps16x16;
		public WebBrowser WebBrowser;
		private Panel Panel;
		public SimpleButton BackBut;
		public SimpleButton ForwardBut;
		public SimpleButton PrintBut;
		private GroupBox groupBox1;
		private System.ComponentModel.IContainer components;

		public PopupHtml()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			UIMisc.RestoreWindowPlacement(this,"BrowserPopupPlacement");
			Left = PopupPos; // position
			Top = PopupPos;
			PopupPos+=25;
			if (PopupPos>250) PopupPos=10;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PopupHtml));
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.WebBrowser = new System.Windows.Forms.WebBrowser();
			this.Panel = new System.Windows.Forms.Panel();
			this.BackBut = new DevExpress.XtraEditors.SimpleButton();
			this.ForwardBut = new DevExpress.XtraEditors.SimpleButton();
			this.PrintBut = new DevExpress.XtraEditors.SimpleButton();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.SuspendLayout();
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "Previous.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "Next.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "Print.bmp");
			// 
			// WebBrowser
			// 
			this.WebBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.WebBrowser.Location = new System.Drawing.Point(0, 0);
			this.WebBrowser.MinimumSize = new System.Drawing.Size(20, 20);
			this.WebBrowser.Name = "WebBrowser";
			this.WebBrowser.Size = new System.Drawing.Size(458, 330);
			this.WebBrowser.TabIndex = 5;
			this.WebBrowser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.WebBrowser_Navigated);
			this.WebBrowser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.WebBrowser_Navigating);
			// 
			// Panel
			// 
			this.Panel.Location = new System.Drawing.Point(0, 0);
			this.Panel.Name = "Panel";
			this.Panel.Size = new System.Drawing.Size(60, 52);
			this.Panel.TabIndex = 6;
			// 
			// BackBut
			// 
			this.BackBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BackBut.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.BackBut.Appearance.Options.UseBackColor = true;
			this.BackBut.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.BackBut.ImageIndex = 0;
			this.BackBut.ImageList = this.Bitmaps16x16;
			this.BackBut.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.BackBut.Location = new System.Drawing.Point(2, 334);
			this.BackBut.LookAndFeel.SkinName = "Money Twins";
			this.BackBut.Name = "BackBut";
			this.BackBut.Size = new System.Drawing.Size(22, 22);
			this.BackBut.TabIndex = 185;
			this.BackBut.ToolTip = "Previous page";
			this.BackBut.Click += new System.EventHandler(this.BackBut_Click);
			// 
			// ForwardBut
			// 
			this.ForwardBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ForwardBut.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ForwardBut.Appearance.Options.UseBackColor = true;
			this.ForwardBut.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.ForwardBut.ImageIndex = 1;
			this.ForwardBut.ImageList = this.Bitmaps16x16;
			this.ForwardBut.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.ForwardBut.Location = new System.Drawing.Point(28, 334);
			this.ForwardBut.LookAndFeel.SkinName = "Money Twins";
			this.ForwardBut.Name = "ForwardBut";
			this.ForwardBut.Size = new System.Drawing.Size(22, 22);
			this.ForwardBut.TabIndex = 186;
			this.ForwardBut.ToolTip = "Next page";
			this.ForwardBut.Click += new System.EventHandler(this.ForwardBut_Click);
			// 
			// PrintBut
			// 
			this.PrintBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PrintBut.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.PrintBut.Appearance.Options.UseBackColor = true;
			this.PrintBut.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.PrintBut.ImageIndex = 2;
			this.PrintBut.ImageList = this.Bitmaps16x16;
			this.PrintBut.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.PrintBut.Location = new System.Drawing.Point(67, 334);
			this.PrintBut.LookAndFeel.SkinName = "Money Twins";
			this.PrintBut.Name = "PrintBut";
			this.PrintBut.Size = new System.Drawing.Size(22, 22);
			this.PrintBut.TabIndex = 187;
			this.PrintBut.ToolTip = "Print";
			this.PrintBut.Click += new System.EventHandler(this.PrintBut_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Location = new System.Drawing.Point(-1, 330);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(459, 2);
			this.groupBox1.TabIndex = 188;
			this.groupBox1.TabStop = false;
			// 
			// PopupHtml
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.ClientSize = new System.Drawing.Size(456, 357);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.WebBrowser);
			this.Controls.Add(this.ForwardBut);
			this.Controls.Add(this.BackBut);
			this.Controls.Add(this.Panel);
			this.Controls.Add(this.PrintBut);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "PopupHtml";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "Mobius";
			this.Activated += new System.EventHandler(this.PopupHtml_Activated);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.BrowserPopup_Closing);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BrowserPopup_FormClosed);
			this.Load += new System.EventHandler(this.BrowserPopup_Load);
			this.Enter += new System.EventHandler(this.PopupHtml_Enter);
			this.Resize += new System.EventHandler(this.BrowserPopup_Resize);
			this.ResumeLayout(false);

		}
		#endregion

		private void BrowserPopup_Load(object sender, System.EventArgs e)
		{
		}

		private void WebBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
		{
			bool processed = PopupMobiusCommandNavigation.Process(e, Panel);
			return;
		}

		private void WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
		{
			return;
		}

		private void BrowserPopup_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
		}

		private void BrowserPopup_Resize(object sender, System.EventArgs e)
		{
			if (WindowState == FormWindowState.Normal)
				UIMisc.SaveWindowPlacement(this,"BrowserPopupPlacement");
		}

		private void BackBut_Click(object sender, EventArgs e)
		{
			try { WebBrowser.GoBack(); }
			catch (Exception ex) { }
		}

		private void ForwardBut_Click(object sender, EventArgs e)
		{
			try { WebBrowser.GoForward(); }
			catch (Exception ex) {}
		}

		private void PrintBut_Click(object sender, EventArgs e)
		{
			WebBrowser.ShowPrintDialog();
		}

		private void PopupHtml_Enter(object sender, EventArgs e)
		{
			StackTrace st = new StackTrace(true);
			//ClientLog.Message("Enter " + Location.X + st.ToString());
		}

		private void PopupHtml_Activated(object sender, EventArgs e)
		{
			StackTrace st = new StackTrace(true);
			//ClientLog.Message("Activated " + Location.X + st.ToString());
		}

		private void BrowserPopup_FormClosed(object sender, FormClosedEventArgs e)
		{
			return;
		}


	}
}
