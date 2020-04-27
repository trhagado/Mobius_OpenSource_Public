namespace Mobius.SpotfireClient
{
	partial class BasicVisualPropertiesDialog
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
			System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Selector list built");
			System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("at runtime...");
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BasicVisualPropertiesDialog));
			this.SplitContainer = new System.Windows.Forms.SplitContainer();
			this.SelectorPanel = new DevExpress.XtraEditors.GroupControl();
			this.TabPageSelector = new System.Windows.Forms.TreeView();
			this.TabsContainerPanel = new DevExpress.XtraEditors.PanelControl();
			this.Tabs = new DevExpress.XtraTab.XtraTabControl();
			this.GeneralTab = new DevExpress.XtraTab.XtraTabPage();
			this.GeneralPropertiesPanel = new Mobius.SpotfireClient.GeneralPropertiesPanel();
			this.DataTab = new DevExpress.XtraTab.XtraTabPage();
			this.DataMapPanel = new Mobius.SpotfireClient.DataMapPanel();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.CloseButton = new DevExpress.XtraEditors.SimpleButton();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SelectorPanel)).BeginInit();
			this.SelectorPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TabsContainerPanel)).BeginInit();
			this.TabsContainerPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Tabs)).BeginInit();
			this.Tabs.SuspendLayout();
			this.GeneralTab.SuspendLayout();
			this.DataTab.SuspendLayout();
			this.buttonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// SplitContainer
			// 
			this.SplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SplitContainer.Location = new System.Drawing.Point(6, 6);
			this.SplitContainer.Name = "SplitContainer";
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.BackColor = System.Drawing.SystemColors.Control;
			this.SplitContainer.Panel1.Controls.Add(this.SelectorPanel);
			this.SplitContainer.Panel1.Padding = new System.Windows.Forms.Padding(1);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
			this.SplitContainer.Panel2.Controls.Add(this.TabsContainerPanel);
			this.SplitContainer.Size = new System.Drawing.Size(629, 513);
			this.SplitContainer.SplitterDistance = 102;
			this.SplitContainer.TabIndex = 13;
			this.SplitContainer.TabStop = false;
			this.SplitContainer.Text = "splitContainer1";
			this.SplitContainer.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.SplitContainer_SplitterMoved);
			// 
			// SelectorPanel
			// 
			this.SelectorPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.SelectorPanel.Controls.Add(this.TabPageSelector);
			this.SelectorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectorPanel.Location = new System.Drawing.Point(1, 1);
			this.SelectorPanel.Name = "SelectorPanel";
			this.SelectorPanel.ShowCaption = false;
			this.SelectorPanel.Size = new System.Drawing.Size(100, 511);
			this.SelectorPanel.TabIndex = 219;
			this.SelectorPanel.Text = "groupControl1";
			// 
			// TabPageSelector
			// 
			this.TabPageSelector.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TabPageSelector.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.TabPageSelector.Font = new System.Drawing.Font("Tahoma", 9F);
			this.TabPageSelector.FullRowSelect = true;
			this.TabPageSelector.HideSelection = false;
			this.TabPageSelector.ItemHeight = 21;
			this.TabPageSelector.Location = new System.Drawing.Point(2, 1);
			this.TabPageSelector.Name = "TabPageSelector";
			treeNode3.Name = "Node1";
			treeNode3.Text = "Selector list built";
			treeNode4.Name = "Node0";
			treeNode4.Text = "at runtime...";
			this.TabPageSelector.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode3,
            treeNode4});
			this.TabPageSelector.ShowLines = false;
			this.TabPageSelector.ShowPlusMinus = false;
			this.TabPageSelector.ShowRootLines = false;
			this.TabPageSelector.Size = new System.Drawing.Size(85, 503);
			this.TabPageSelector.TabIndex = 1;
			this.TabPageSelector.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TabPageSelector_AfterSelect);
			// 
			// TabsContainerPanel
			// 
			this.TabsContainerPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
			this.TabsContainerPanel.Controls.Add(this.Tabs);
			this.TabsContainerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabsContainerPanel.Location = new System.Drawing.Point(0, 0);
			this.TabsContainerPanel.Name = "TabsContainerPanel";
			this.TabsContainerPanel.Size = new System.Drawing.Size(523, 513);
			this.TabsContainerPanel.TabIndex = 0;
			// 
			// Tabs
			// 
			this.Tabs.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
			this.Tabs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Tabs.HeaderLocation = DevExpress.XtraTab.TabHeaderLocation.Right;
			this.Tabs.HeaderOrientation = DevExpress.XtraTab.TabOrientation.Horizontal;
			this.Tabs.Location = new System.Drawing.Point(2, 2);
			this.Tabs.Name = "Tabs";
			this.Tabs.SelectedTabPage = this.GeneralTab;
			this.Tabs.ShowTabHeader = DevExpress.Utils.DefaultBoolean.True;
			this.Tabs.Size = new System.Drawing.Size(519, 509);
			this.Tabs.TabIndex = 0;
			this.Tabs.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.GeneralTab,
            this.DataTab});
			this.Tabs.SelectedPageChanged += new DevExpress.XtraTab.TabPageChangedEventHandler(this.Tabs_SelectedPageChanged);
			this.Tabs.SelectedPageChanging += new DevExpress.XtraTab.TabPageChangingEventHandler(this.Tabs_SelectedPageChanging);
			// 
			// GeneralTab
			// 
			this.GeneralTab.Controls.Add(this.GeneralPropertiesPanel);
			this.GeneralTab.Name = "GeneralTab";
			this.GeneralTab.Size = new System.Drawing.Size(460, 501);
			this.GeneralTab.Text = "General";
			// 
			// GeneralPropertiesPanel
			// 
			this.GeneralPropertiesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GeneralPropertiesPanel.Location = new System.Drawing.Point(0, 0);
			this.GeneralPropertiesPanel.Name = "GeneralPropertiesPanel";
			this.GeneralPropertiesPanel.Size = new System.Drawing.Size(460, 501);
			this.GeneralPropertiesPanel.TabIndex = 0;
			// 
			// DataTab
			// 
			this.DataTab.Controls.Add(this.DataMapPanel);
			this.DataTab.Name = "DataTab";
			this.DataTab.Size = new System.Drawing.Size(460, 501);
			this.DataTab.Text = "Data";
			// 
			// DataMapPanel
			// 
			this.DataMapPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DataMapPanel.Location = new System.Drawing.Point(0, 0);
			this.DataMapPanel.Name = "DataMapPanel";
			this.DataMapPanel.Size = new System.Drawing.Size(460, 501);
			this.DataMapPanel.TabIndex = 0;
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.CloseButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = new System.Drawing.Point(0, 518);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(640, 36);
			this.buttonPanel.TabIndex = 14;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Location = new System.Drawing.Point(573, 5);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(60, 24);
			this.CloseButton.TabIndex = 185;
			this.CloseButton.Text = "Close";
			this.CloseButton.Click += new System.EventHandler(this.OkButton_Click);
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
			// BasicVisualPropertiesDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(640, 554);
			this.Controls.Add(this.buttonPanel);
			this.Controls.Add(this.SplitContainer);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimizeBox = false;
			this.Name = "BasicVisualPropertiesDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Basic Visualization Properties";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GeneralAnalysisProperties_FormClosing);
			this.Shown += new System.EventHandler(this.GeneralAnalysisPropertiesDialog_Shown);
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SelectorPanel)).EndInit();
			this.SelectorPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TabsContainerPanel)).EndInit();
			this.TabsContainerPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Tabs)).EndInit();
			this.Tabs.ResumeLayout(false);
			this.GeneralTab.ResumeLayout(false);
			this.DataTab.ResumeLayout(false);
			this.buttonPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.SplitContainer SplitContainer;
		private System.Windows.Forms.Panel buttonPanel;
		private DevExpress.XtraEditors.GroupControl SelectorPanel;
		private System.Windows.Forms.TreeView TabPageSelector;
		private DevExpress.XtraEditors.PanelControl TabsContainerPanel;
		public DevExpress.XtraEditors.SimpleButton CloseButton;
		public System.Windows.Forms.ImageList Bitmaps16x16;
		private DevExpress.XtraTab.XtraTabControl Tabs;
		private DevExpress.XtraTab.XtraTabPage GeneralTab;
		private DevExpress.XtraTab.XtraTabPage DataTab;
		private DataMapPanel DataMapPanel;
		private GeneralPropertiesPanel GeneralPropertiesPanel;
	}
}