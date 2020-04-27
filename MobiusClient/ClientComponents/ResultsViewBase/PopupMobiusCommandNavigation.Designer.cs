namespace Mobius.ClientComponents
{
	partial class PopupMobiusCommandNavigation
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PopupMobiusCommandNavigation));
			this.TargetContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ShowTargetDescriptionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ShowTargetAssayListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ShowTargetAssayDataMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MetaTableLabelContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuShowDescription = new System.Windows.Forms.ToolStripMenuItem();
			this.menuAddToQuery = new System.Windows.Forms.ToolStripMenuItem();
			this.Timer = new System.Windows.Forms.Timer(this.components);
			this.CidContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.CnPopupStructure = new System.Windows.Forms.ToolStripMenuItem();
			this.CnPopupCopyStructure = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.CnPopupAllData = new System.Windows.Forms.ToolStripMenuItem();
			this.CnPopupShowStbView = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.CnPopupShowCorpIdChangeHistory = new System.Windows.Forms.ToolStripMenuItem();
			this.TargetContextMenu.SuspendLayout();
			this.MetaTableLabelContextMenu.SuspendLayout();
			this.CidContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// TargetContextMenu
			// 
			this.TargetContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ShowTargetDescriptionMenuItem,
            this.ShowTargetAssayListMenuItem,
            this.ShowTargetAssayDataMenuItem});
			this.TargetContextMenu.Name = "TargetContextMenu";
			this.TargetContextMenu.Size = new System.Drawing.Size(236, 70);
			this.TargetContextMenu.Text = "Show List of Assays";
			// 
			// ShowTargetDescriptionMenuItem
			// 
			this.ShowTargetDescriptionMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ShowTargetDescriptionMenuItem.Image")));
			this.ShowTargetDescriptionMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ShowTargetDescriptionMenuItem.Name = "ShowTargetDescriptionMenuItem";
			this.ShowTargetDescriptionMenuItem.Size = new System.Drawing.Size(235, 22);
			this.ShowTargetDescriptionMenuItem.Text = "Show Target Description";
			this.ShowTargetDescriptionMenuItem.Click += new System.EventHandler(this.ShowTargetDescriptionMenuItem_Click);
			// 
			// ShowTargetAssayListMenuItem
			// 
			this.ShowTargetAssayListMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.List;
			this.ShowTargetAssayListMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ShowTargetAssayListMenuItem.Name = "ShowTargetAssayListMenuItem";
			this.ShowTargetAssayListMenuItem.Size = new System.Drawing.Size(235, 22);
			this.ShowTargetAssayListMenuItem.Text = "Show Target Assay List";
			this.ShowTargetAssayListMenuItem.Click += new System.EventHandler(this.ShowTargetAssayListMenuItem_Click);
			// 
			// ShowTargetAssayDataMenuItem
			// 
			this.ShowTargetAssayDataMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ShowTargetAssayDataMenuItem.Image")));
			this.ShowTargetAssayDataMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ShowTargetAssayDataMenuItem.Name = "ShowTargetAssayDataMenuItem";
			this.ShowTargetAssayDataMenuItem.Size = new System.Drawing.Size(235, 22);
			this.ShowTargetAssayDataMenuItem.Text = "Show All Assay Data for Target";
			this.ShowTargetAssayDataMenuItem.Click += new System.EventHandler(this.ShowTargetAssayDataMenuItem_Click_1);
			// 
			// MetaTableLabelContextMenu
			// 
			this.MetaTableLabelContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuShowDescription,
            this.menuAddToQuery});
			this.MetaTableLabelContextMenu.Name = "TableHeaderPopup";
			this.MetaTableLabelContextMenu.Size = new System.Drawing.Size(167, 48);
			// 
			// menuShowDescription
			// 
			this.menuShowDescription.Image = ((System.Drawing.Image)(resources.GetObject("menuShowDescription.Image")));
			this.menuShowDescription.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.menuShowDescription.Name = "menuShowDescription";
			this.menuShowDescription.Size = new System.Drawing.Size(166, 22);
			this.menuShowDescription.Text = "Show Description";
			this.menuShowDescription.Click += new System.EventHandler(this.menuShowDescription_Click);
			// 
			// menuAddToQuery
			// 
			this.menuAddToQuery.Image = global::Mobius.ClientComponents.Properties.Resources.Add;
			this.menuAddToQuery.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.menuAddToQuery.Name = "menuAddToQuery";
			this.menuAddToQuery.Size = new System.Drawing.Size(166, 22);
			this.menuAddToQuery.Text = "Add to Query";
			this.menuAddToQuery.Click += new System.EventHandler(this.menuAddToQuery_Click);
			// 
			// CidContextMenu
			// 
			this.CidContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CnPopupStructure,
            this.CnPopupCopyStructure,
            this.toolStripSeparator1,
            this.CnPopupAllData,
            this.CnPopupShowStbView,
            this.toolStripMenuItem1,
            this.CnPopupShowCorpIdChangeHistory});
			this.CidContextMenu.Name = "CmpdNoPopupMenu";
			this.CidContextMenu.Size = new System.Drawing.Size(398, 148);
			// 
			// CnPopupStructure
			// 
			this.CnPopupStructure.Image = global::Mobius.ClientComponents.Properties.Resources.Struct;
			this.CnPopupStructure.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.CnPopupStructure.Name = "CnPopupStructure";
			this.CnPopupStructure.Size = new System.Drawing.Size(397, 22);
			this.CnPopupStructure.Text = "View Molecule in a New Window";
			this.CnPopupStructure.Click += new System.EventHandler(this.CnPopupStructure_Click);
			// 
			// CnPopupCopyStructure
			// 
			this.CnPopupCopyStructure.Image = global::Mobius.ClientComponents.Properties.Resources.Copy;
			this.CnPopupCopyStructure.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.CnPopupCopyStructure.Name = "CnPopupCopyStructure";
			this.CnPopupCopyStructure.Size = new System.Drawing.Size(397, 22);
			this.CnPopupCopyStructure.Text = "Copy \"Editable\" Structure to Clipboard";
			this.CnPopupCopyStructure.Click += new System.EventHandler(this.CnPopupCopyStructure_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(394, 6);
			// 
			// CnPopupAllData
			// 
			this.CnPopupAllData.Image = global::Mobius.ClientComponents.Properties.Resources.TableDataMultiple;
			this.CnPopupAllData.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.CnPopupAllData.Name = "CnPopupAllData";
			this.CnPopupAllData.Size = new System.Drawing.Size(397, 22);
			this.CnPopupAllData.Text = "Retrieve All Data for the Compound";
			this.CnPopupAllData.Click += new System.EventHandler(this.CnPopupAllData_Click);
			// 
			// CnPopupShowStbView
			// 
			this.CnPopupShowStbView.Image = global::Mobius.ClientComponents.Properties.Resources.Spotfire;
			this.CnPopupShowStbView.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.CnPopupShowStbView.Name = "CnPopupShowStbView";
			this.CnPopupShowStbView.Size = new System.Drawing.Size(397, 22);
			this.CnPopupShowStbView.Text = "Show \"Spill the Beans\" view for the Compound";
			this.CnPopupShowStbView.Click += new System.EventHandler(this.CnPopupShowStbView_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(394, 6);
			this.toolStripMenuItem1.Visible = false;
			// 
			// CnPopupShowCorpIdChangeHistory
			// 
			this.CnPopupShowCorpIdChangeHistory.Name = "CnPopupShowCorpIdChangeHistory";
			this.CnPopupShowCorpIdChangeHistory.Size = new System.Drawing.Size(397, 22);
			this.CnPopupShowCorpIdChangeHistory.Text = "Show Registraton History / CorpId Changes for this Compound";
			this.CnPopupShowCorpIdChangeHistory.Visible = false;
			this.CnPopupShowCorpIdChangeHistory.Click += new System.EventHandler(this.CnPopupShowCorpIdChangeHistory_Click);
			// 
			// PopupMobiusCommandNavigation
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(464, 432);
			this.Name = "PopupMobiusCommandNavigation";
			this.Text = "PopupProcessMobiusCommandNavigation";
			this.TargetContextMenu.ResumeLayout(false);
			this.MetaTableLabelContextMenu.ResumeLayout(false);
			this.CidContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.ContextMenuStrip TargetContextMenu;
		public System.Windows.Forms.ToolStripMenuItem ShowTargetDescriptionMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ShowTargetAssayListMenuItem;
		public System.Windows.Forms.ToolStripMenuItem ShowTargetAssayDataMenuItem;
		private System.Windows.Forms.ContextMenuStrip MetaTableLabelContextMenu;
		private System.Windows.Forms.ToolStripMenuItem menuShowDescription;
		private System.Windows.Forms.ToolStripMenuItem menuAddToQuery;
		private System.Windows.Forms.Timer Timer;
		public System.Windows.Forms.ContextMenuStrip CidContextMenu;
		public System.Windows.Forms.ToolStripMenuItem CnPopupStructure;
		public System.Windows.Forms.ToolStripMenuItem CnPopupCopyStructure;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		public System.Windows.Forms.ToolStripMenuItem CnPopupAllData;
		public System.Windows.Forms.ToolStripMenuItem CnPopupShowStbView;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem CnPopupShowCorpIdChangeHistory;
	}
}