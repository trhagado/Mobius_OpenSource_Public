namespace Mobius.SpotfireClient
{
	partial class ColumnsSelectorDropDown
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColumnsSelectorDropDown));
			this.Bitmaps8x8 = new System.Windows.Forms.ImageList(this.components);
			this.SelectFieldMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.Table1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.Field1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.SelectFromDatabaseContentsTreeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.AggregationComboBox = new DevExpress.XtraEditors.ComboBoxEdit();
			this.imageCollection1 = new DevExpress.Utils.ImageCollection(this.components);
			this.DisplayNameTextEdit = new DevExpress.XtraEditors.TextEdit();
			this.DisplayNameLabel = new DevExpress.XtraEditors.LabelControl();
			this.ExpressionTextBox = new DevExpress.XtraEditors.MemoEdit();
			this.CloseButton = new DevExpress.XtraEditors.SimpleButton();
			this.MainPanel = new DevExpress.XtraEditors.PanelControl();
			this.ColumnList = new DevExpress.XtraEditors.CheckedListBoxControl();
			this.BottomPanel = new DevExpress.XtraEditors.PanelControl();
			this.RemoveButton = new DevExpress.XtraEditors.SimpleButton();
			this.TopPanel = new DevExpress.XtraEditors.PanelControl();
			this.TableSelector = new Mobius.SpotfireClient.TableSelectorControlMsx();
			this.ExpressionLabel = new DevExpress.XtraEditors.LabelControl();
			this.SelectFieldMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AggregationComboBox.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.imageCollection1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DisplayNameTextEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExpressionTextBox.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainPanel)).BeginInit();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ColumnList)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BottomPanel)).BeginInit();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopPanel)).BeginInit();
			this.TopPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// Bitmaps8x8
			// 
			this.Bitmaps8x8.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps8x8.ImageStream")));
			this.Bitmaps8x8.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps8x8.Images.SetKeyName(0, "CloseWindow.bmp");
			this.Bitmaps8x8.Images.SetKeyName(1, "ChevronDown.bmp");
			this.Bitmaps8x8.Images.SetKeyName(2, "ChevronUp.bmp");
			// 
			// SelectFieldMenu
			// 
			this.SelectFieldMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Table1MenuItem,
            this.toolStripMenuItem1,
            this.SelectFromDatabaseContentsTreeMenuItem});
			this.SelectFieldMenu.Name = "SelectFieldMenu";
			this.SelectFieldMenu.Size = new System.Drawing.Size(273, 54);
			// 
			// Table1MenuItem
			// 
			this.Table1MenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Field1MenuItem});
			this.Table1MenuItem.Image = ((System.Drawing.Image)(resources.GetObject("Table1MenuItem.Image")));
			this.Table1MenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.Table1MenuItem.Name = "Table1MenuItem";
			this.Table1MenuItem.Size = new System.Drawing.Size(272, 22);
			this.Table1MenuItem.Text = "Table1";
			// 
			// Field1MenuItem
			// 
			this.Field1MenuItem.Name = "Field1MenuItem";
			this.Field1MenuItem.Size = new System.Drawing.Size(105, 22);
			this.Field1MenuItem.Text = "Field1";
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(269, 6);
			// 
			// SelectFromDatabaseContentsTreeMenuItem
			// 
			this.SelectFromDatabaseContentsTreeMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.WorldSearch;
			this.SelectFromDatabaseContentsTreeMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.SelectFromDatabaseContentsTreeMenuItem.Name = "SelectFromDatabaseContentsTreeMenuItem";
			this.SelectFromDatabaseContentsTreeMenuItem.Size = new System.Drawing.Size(272, 22);
			this.SelectFromDatabaseContentsTreeMenuItem.Text = "Select From Database Contents Tree...";
			// 
			// AggregationComboBox
			// 
			this.AggregationComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AggregationComboBox.EditValue = "No aggregation possible";
			this.AggregationComboBox.Enabled = false;
			this.AggregationComboBox.Location = new System.Drawing.Point(5, 35);
			this.AggregationComboBox.Name = "AggregationComboBox";
			this.AggregationComboBox.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.AggregationComboBox.Size = new System.Drawing.Size(321, 20);
			this.AggregationComboBox.TabIndex = 1;
			this.AggregationComboBox.ToolTip = "Aggregation method for selected data";
			// 
			// imageCollection1
			// 
			this.imageCollection1.ImageStream = ((DevExpress.Utils.ImageCollectionStreamer)(resources.GetObject("imageCollection1.ImageStream")));
			this.imageCollection1.Images.SetKeyName(0, "CloseWindow.png");
			this.imageCollection1.Images.SetKeyName(1, "ChevronDown.png");
			this.imageCollection1.Images.SetKeyName(2, "ChevronUp.png");
			// 
			// DisplayNameTextEdit
			// 
			this.DisplayNameTextEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DisplayNameTextEdit.Location = new System.Drawing.Point(78, 333);
			this.DisplayNameTextEdit.Name = "DisplayNameTextEdit";
			this.DisplayNameTextEdit.Size = new System.Drawing.Size(248, 20);
			this.DisplayNameTextEdit.TabIndex = 6;
			// 
			// DisplayNameLabel
			// 
			this.DisplayNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DisplayNameLabel.Location = new System.Drawing.Point(5, 335);
			this.DisplayNameLabel.Name = "DisplayNameLabel";
			this.DisplayNameLabel.Size = new System.Drawing.Size(67, 13);
			this.DisplayNameLabel.TabIndex = 8;
			this.DisplayNameLabel.Text = "Display name:";
			// 
			// ExpressionTextBox
			// 
			this.ExpressionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ExpressionTextBox.Location = new System.Drawing.Point(5, 379);
			this.ExpressionTextBox.Name = "ExpressionTextBox";
			this.ExpressionTextBox.Size = new System.Drawing.Size(322, 54);
			this.ExpressionTextBox.TabIndex = 10;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.CloseButton.Appearance.Options.UseBackColor = true;
			this.CloseButton.ImageOptions.ImageIndex = 0;
			this.CloseButton.ImageOptions.ImageList = this.imageCollection1;
			this.CloseButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.CloseButton.Location = new System.Drawing.Point(305, 3);
			this.CloseButton.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.UltraFlat;
			this.CloseButton.LookAndFeel.UseDefaultLookAndFeel = false;
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(22, 22);
			this.CloseButton.TabIndex = 12;
			this.CloseButton.Text = "simpleButton1";
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.ColumnList);
			this.MainPanel.Controls.Add(this.BottomPanel);
			this.MainPanel.Controls.Add(this.TopPanel);
			this.MainPanel.Controls.Add(this.ExpressionTextBox);
			this.MainPanel.Controls.Add(this.AggregationComboBox);
			this.MainPanel.Controls.Add(this.ExpressionLabel);
			this.MainPanel.Controls.Add(this.DisplayNameTextEdit);
			this.MainPanel.Controls.Add(this.DisplayNameLabel);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = new System.Drawing.Point(0, 0);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = new System.Drawing.Size(331, 469);
			this.MainPanel.TabIndex = 13;
			// 
			// ColumnList
			// 
			this.ColumnList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ColumnList.CheckMode = DevExpress.XtraEditors.CheckMode.Single;
			this.ColumnList.IncrementalSearch = true;
			this.ColumnList.Location = new System.Drawing.Point(0, 60);
			this.ColumnList.Name = "ColumnList";
			this.ColumnList.Size = new System.Drawing.Size(331, 267);
			this.ColumnList.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BottomPanel.Controls.Add(this.RemoveButton);
			this.BottomPanel.Location = new System.Drawing.Point(0, 439);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = new System.Drawing.Size(331, 30);
			this.BottomPanel.TabIndex = 15;
			// 
			// RemoveButton
			// 
			this.RemoveButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RemoveButton.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.RemoveButton.Appearance.Options.UseBackColor = true;
			this.RemoveButton.ImageOptions.ImageIndex = 0;
			this.RemoveButton.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.RemoveButton.Location = new System.Drawing.Point(2, 2);
			this.RemoveButton.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.UltraFlat;
			this.RemoveButton.LookAndFeel.UseDefaultLookAndFeel = false;
			this.RemoveButton.Name = "RemoveButton";
			this.RemoveButton.Size = new System.Drawing.Size(329, 26);
			this.RemoveButton.TabIndex = 13;
			this.RemoveButton.Text = "Remove";
			this.RemoveButton.Click += new System.EventHandler(this.RemoveButton_Click);
			// 
			// TopPanel
			// 
			this.TopPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TopPanel.Controls.Add(this.TableSelector);
			this.TopPanel.Controls.Add(this.CloseButton);
			this.TopPanel.Location = new System.Drawing.Point(0, 0);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = new System.Drawing.Size(331, 30);
			this.TopPanel.TabIndex = 14;
			// 
			// TableSelector
			// 
			this.TableSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TableSelector.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.TableSelector.Appearance.Options.UseBackColor = true;
			this.TableSelector.Location = new System.Drawing.Point(2, 2);
			this.TableSelector.Name = "TableSelector";
			this.TableSelector.Size = new System.Drawing.Size(297, 26);
			this.TableSelector.TabIndex = 13;
			// 
			// ExpressionLabel
			// 
			this.ExpressionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ExpressionLabel.Location = new System.Drawing.Point(5, 359);
			this.ExpressionLabel.Name = "ExpressionLabel";
			this.ExpressionLabel.Size = new System.Drawing.Size(56, 13);
			this.ExpressionLabel.TabIndex = 9;
			this.ExpressionLabel.Text = "Expression:";
			// 
			// ColumnsSelectorDropDown
			// 
			this.Appearance.BorderColor = System.Drawing.Color.Silver;
			this.Appearance.Options.UseBorderColor = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.MainPanel);
			this.Name = "ColumnsSelectorDropDown";
			this.Size = new System.Drawing.Size(331, 469);
			this.SelectFieldMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AggregationComboBox.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.imageCollection1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DisplayNameTextEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ExpressionTextBox.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MainPanel)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ColumnList)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BottomPanel)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TopPanel)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ImageList Bitmaps8x8;
		private System.Windows.Forms.ContextMenuStrip SelectFieldMenu;
		private System.Windows.Forms.ToolStripMenuItem Table1MenuItem;
		private System.Windows.Forms.ToolStripMenuItem Field1MenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem SelectFromDatabaseContentsTreeMenuItem;
		private DevExpress.XtraEditors.ComboBoxEdit AggregationComboBox;
		private DevExpress.Utils.ImageCollection imageCollection1;
		private DevExpress.XtraEditors.TextEdit DisplayNameTextEdit;
		private DevExpress.XtraEditors.LabelControl DisplayNameLabel;
		private DevExpress.XtraEditors.MemoEdit ExpressionTextBox;
		private DevExpress.XtraEditors.SimpleButton CloseButton;
		private DevExpress.XtraEditors.PanelControl MainPanel;
		private DevExpress.XtraEditors.PanelControl BottomPanel;
		private DevExpress.XtraEditors.SimpleButton RemoveButton;
		private DevExpress.XtraEditors.PanelControl TopPanel;
		private Mobius.SpotfireClient.TableSelectorControlMsx TableSelector;
		private DevExpress.XtraEditors.CheckedListBoxControl ColumnList;
		private DevExpress.XtraEditors.LabelControl ExpressionLabel;
	}
}
