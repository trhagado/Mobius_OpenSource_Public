namespace Mobius.SpotfireClient
{
	partial class SpotfirePanel
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpotfirePanel));
			this.WebBrowser = new System.Windows.Forms.WebBrowser();
			this.WebBrowserRtClickContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.DataMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.VisualPropertiesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.ShowSpotfireApiLogMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SplitContainerControl = new DevExpress.XtraEditors.SplitContainerControl();
			this.SpotfireApiLog = new DevExpress.XtraEditors.MemoEdit();
			this.WebBrowserRtClickContextMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainerControl)).BeginInit();
			this.SplitContainerControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SpotfireApiLog.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// WebBrowser
			// 
			this.WebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WebBrowser.IsWebBrowserContextMenuEnabled = false;
			this.WebBrowser.Location = new System.Drawing.Point(0, 0);
			this.WebBrowser.MinimumSize = new System.Drawing.Size(20, 20);
			this.WebBrowser.Name = "WebBrowser";
			this.WebBrowser.ScrollBarsEnabled = false;
			this.WebBrowser.Size = new System.Drawing.Size(724, 423);
			this.WebBrowser.TabIndex = 228;
			this.WebBrowser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.WebBrowser_Navigated);
			this.WebBrowser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.WebBrowser_Navigating);
			this.WebBrowser.Resize += new System.EventHandler(this.WebBrowser_Resize);
			// 
			// WebBrowserRtClickContextMenu
			// 
			this.WebBrowserRtClickContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.WebBrowserRtClickContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DataMenuItem,
            this.VisualPropertiesMenuItem,
            this.toolStripMenuItem1,
            this.ShowSpotfireApiLogMenuItem});
			this.WebBrowserRtClickContextMenu.Name = "ContextMenu";
			this.WebBrowserRtClickContextMenu.Size = new System.Drawing.Size(196, 88);
			// 
			// DataMenuItem
			// 
			this.DataMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("DataMenuItem.Image")));
			this.DataMenuItem.Name = "DataMenuItem";
			this.DataMenuItem.Size = new System.Drawing.Size(195, 26);
			this.DataMenuItem.Text = "Data Mapping...";
			this.DataMenuItem.Click += new System.EventHandler(this.DataMenuItem_Click);
			// 
			// VisualPropertiesMenuItem
			// 
			this.VisualPropertiesMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("VisualPropertiesMenuItem.Image")));
			this.VisualPropertiesMenuItem.Name = "VisualPropertiesMenuItem";
			this.VisualPropertiesMenuItem.Size = new System.Drawing.Size(195, 26);
			this.VisualPropertiesMenuItem.Text = "Visual Properties...";
			this.VisualPropertiesMenuItem.Visible = false;
			this.VisualPropertiesMenuItem.Click += new System.EventHandler(this.VisualPropertiesMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(192, 6);
			// 
			// ShowSpotfireApiLogMenuItem
			// 
			this.ShowSpotfireApiLogMenuItem.Name = "ShowSpotfireApiLogMenuItem";
			this.ShowSpotfireApiLogMenuItem.Size = new System.Drawing.Size(195, 26);
			this.ShowSpotfireApiLogMenuItem.Text = "Show Spotfire Api Log";
			this.ShowSpotfireApiLogMenuItem.Click += new System.EventHandler(this.ShowSpotfireApiLogMenuItem_Click);
			// 
			// SplitContainerControl
			// 
			this.SplitContainerControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainerControl.FixedPanel = DevExpress.XtraEditors.SplitFixedPanel.Panel2;
			this.SplitContainerControl.Horizontal = false;
			this.SplitContainerControl.Location = new System.Drawing.Point(0, 0);
			this.SplitContainerControl.Name = "SplitContainerControl";
			this.SplitContainerControl.Panel1.Controls.Add(this.WebBrowser);
			this.SplitContainerControl.Panel1.Text = "Panel1";
			this.SplitContainerControl.Panel2.Controls.Add(this.SpotfireApiLog);
			this.SplitContainerControl.Panel2.Text = "Panel2";
			this.SplitContainerControl.Size = new System.Drawing.Size(724, 567);
			this.SplitContainerControl.SplitterPosition = 139;
			this.SplitContainerControl.TabIndex = 230;
			// 
			// SpotfireApiLog
			// 
			this.SpotfireApiLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SpotfireApiLog.Location = new System.Drawing.Point(0, 0);
			this.SpotfireApiLog.Name = "SpotfireApiLog";
			this.SpotfireApiLog.Size = new System.Drawing.Size(724, 139);
			this.SpotfireApiLog.TabIndex = 229;
			// 
			// SpotfirePanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.SplitContainerControl);
			this.Name = "SpotfirePanel";
			this.Size = new System.Drawing.Size(724, 567);
			this.WebBrowserRtClickContextMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainerControl)).EndInit();
			this.SplitContainerControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SpotfireApiLog.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.WebBrowser WebBrowser;
		internal System.Windows.Forms.ContextMenuStrip WebBrowserRtClickContextMenu;
		internal DevExpress.XtraEditors.SplitContainerControl SplitContainerControl;
		internal System.Windows.Forms.ToolStripMenuItem VisualPropertiesMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem ShowSpotfireApiLogMenuItem;
		internal System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		internal System.Windows.Forms.ToolStripMenuItem DataMenuItem;
		public DevExpress.XtraEditors.MemoEdit SpotfireApiLog;
	}
}
