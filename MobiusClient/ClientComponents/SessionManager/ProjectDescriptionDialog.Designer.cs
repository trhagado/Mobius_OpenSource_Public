namespace Mobius.ClientComponents
{
	partial class ProjectDescriptionDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectDescriptionDialog));
			this.Tabs = new DevExpress.XtraTab.XtraTabControl();
			this.FlowSchemeTabPage = new DevExpress.XtraTab.XtraTabPage();
			this.BasicTabPage = new DevExpress.XtraTab.XtraTabPage();
			this.WebBrowser = new System.Windows.Forms.WebBrowser();
			((System.ComponentModel.ISupportInitialize)(this.Tabs)).BeginInit();
			this.Tabs.SuspendLayout();
			this.BasicTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// Tabs
			// 
			this.Tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Tabs.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Tabs.Appearance.Options.UseBackColor = true;
			this.Tabs.Location = new System.Drawing.Point(5, 5);
			this.Tabs.Name = "Tabs";
			this.Tabs.SelectedTabPage = this.FlowSchemeTabPage;
			this.Tabs.Size = new System.Drawing.Size(1066, 829);
			this.Tabs.TabIndex = 204;
			this.Tabs.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.BasicTabPage,
            this.FlowSchemeTabPage});
			this.Tabs.SelectedPageChanged += new DevExpress.XtraTab.TabPageChangedEventHandler(this.Tabs_SelectedPageChanged);
			// 
			// FlowSchemeTabPage
			// 
			this.FlowSchemeTabPage.Appearance.PageClient.BackColor = System.Drawing.Color.Gainsboro;
			this.FlowSchemeTabPage.Appearance.PageClient.Options.UseBackColor = true;
			this.FlowSchemeTabPage.Name = "FlowSchemeTabPage";
			this.FlowSchemeTabPage.Size = new System.Drawing.Size(1060, 801);
			this.FlowSchemeTabPage.Text = "Flow Scheme";
			// 
			// BasicTabPage
			// 
			this.BasicTabPage.Appearance.PageClient.BackColor = System.Drawing.Color.DimGray;
			this.BasicTabPage.Appearance.PageClient.Options.UseBackColor = true;
			this.BasicTabPage.Controls.Add(this.WebBrowser);
			this.BasicTabPage.Name = "BasicTabPage";
			this.BasicTabPage.Size = new System.Drawing.Size(1060, 801);
			this.BasicTabPage.Text = "Project Basics";
			// 
			// WebBrowser
			// 
			this.WebBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.WebBrowser.Location = new System.Drawing.Point(5, 5);
			this.WebBrowser.MinimumSize = new System.Drawing.Size(20, 20);
			this.WebBrowser.Name = "WebBrowser";
			this.WebBrowser.Size = new System.Drawing.Size(1050, 793);
			this.WebBrowser.TabIndex = 6;
			// 
			// ProjectDescriptionDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1076, 839);
			this.Controls.Add(this.Tabs);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ProjectDescriptionDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "Project Description";
			((System.ComponentModel.ISupportInitialize)(this.Tabs)).EndInit();
			this.Tabs.ResumeLayout(false);
			this.BasicTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraTab.XtraTabControl Tabs;
		private DevExpress.XtraTab.XtraTabPage FlowSchemeTabPage;
		private DevExpress.XtraTab.XtraTabPage BasicTabPage;
		public System.Windows.Forms.WebBrowser WebBrowser;
	}
}