﻿namespace Mobius.SpotfireClient
{
	partial class ShapeBySelectorControl
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
			DevExpress.XtraEditors.Repository.RepositoryItemImageComboBox repositoryItemImageComboBox1 = new DevExpress.XtraEditors.Repository.RepositoryItemImageComboBox();
			DevExpress.XtraEditors.Controls.EditorButtonImageOptions editorButtonImageOptions1 = new DevExpress.XtraEditors.Controls.EditorButtonImageOptions();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShapeBySelectorControl));
			DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject1 = new DevExpress.Utils.SerializableAppearanceObject();
			DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject2 = new DevExpress.Utils.SerializableAppearanceObject();
			DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject3 = new DevExpress.Utils.SerializableAppearanceObject();
			DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject4 = new DevExpress.Utils.SerializableAppearanceObject();
			this.ShapeSchemeGrid = new DevExpress.XtraGrid.GridControl();
			this.ShapeSchemeGridView = new DevExpress.XtraGrid.Views.Grid.GridView();
			this.ShapeColumn = new DevExpress.XtraGrid.Columns.GridColumn();
			this.ShapeValue = new DevExpress.XtraGrid.Columns.GridColumn();
			this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
			this.ShapeByColumn = new DevExpress.XtraEditors.CheckEdit();
			this.ShapeByFixedShape = new DevExpress.XtraEditors.CheckEdit();
			this.FixedShapeButton = new DevExpress.XtraEditors.ButtonEdit();
			this.MarkerShapes16x16 = new System.Windows.Forms.ImageList(this.components);
			this.ShapeColumnSelector = new Mobius.SpotfireClient.ColumnSelectorControl();
			this.labelControl12 = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.ShapeSchemeGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ShapeSchemeGridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(repositoryItemImageComboBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ShapeByColumn.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ShapeByFixedShape.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FixedShapeButton.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// ShapeSchemeGrid
			// 
			this.ShapeSchemeGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ShapeSchemeGrid.Location = new System.Drawing.Point(10, 138);
			this.ShapeSchemeGrid.MainView = this.ShapeSchemeGridView;
			this.ShapeSchemeGrid.Name = "ShapeSchemeGrid";
			this.ShapeSchemeGrid.Size = new System.Drawing.Size(565, 344);
			this.ShapeSchemeGrid.TabIndex = 195;
			this.ShapeSchemeGrid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.ShapeSchemeGridView});
			// 
			// ShapeSchemeGridView
			// 
			this.ShapeSchemeGridView.ActiveFilterEnabled = false;
			this.ShapeSchemeGridView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.ShapeColumn,
            this.ShapeValue});
			this.ShapeSchemeGridView.GridControl = this.ShapeSchemeGrid;
			this.ShapeSchemeGridView.Name = "ShapeSchemeGridView";
			this.ShapeSchemeGridView.OptionsFilter.AllowColumnMRUFilterList = false;
			this.ShapeSchemeGridView.OptionsFilter.AllowFilterEditor = false;
			this.ShapeSchemeGridView.OptionsFilter.AllowMRUFilterList = false;
			this.ShapeSchemeGridView.OptionsMenu.EnableColumnMenu = false;
			this.ShapeSchemeGridView.OptionsView.ColumnAutoWidth = false;
			this.ShapeSchemeGridView.OptionsView.ShowGroupPanel = false;
			this.ShapeSchemeGridView.OptionsView.ShowIndicator = false;
			// 
			// ShapeColumn
			// 
			this.ShapeColumn.AppearanceHeader.Options.UseTextOptions = true;
			this.ShapeColumn.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.ShapeColumn.Caption = "Shape";
			repositoryItemImageComboBox1.AutoHeight = false;
			repositoryItemImageComboBox1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			repositoryItemImageComboBox1.Name = "ImageComboBox";
			this.ShapeColumn.ColumnEdit = repositoryItemImageComboBox1;
			this.ShapeColumn.FieldName = "Shape";
			this.ShapeColumn.Name = "ShapeColumn";
			this.ShapeColumn.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
			this.ShapeColumn.OptionsFilter.AllowAutoFilter = false;
			this.ShapeColumn.OptionsFilter.AllowFilter = false;
			this.ShapeColumn.Visible = true;
			this.ShapeColumn.VisibleIndex = 0;
			this.ShapeColumn.Width = 95;
			// 
			// ShapeValue
			// 
			this.ShapeValue.AppearanceHeader.Options.UseTextOptions = true;
			this.ShapeValue.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.ShapeValue.Caption = "Value";
			this.ShapeValue.FieldName = "Value";
			this.ShapeValue.Name = "ShapeValue";
			this.ShapeValue.OptionsFilter.AllowAutoFilter = false;
			this.ShapeValue.OptionsFilter.AllowFilter = false;
			this.ShapeValue.Visible = true;
			this.ShapeValue.VisibleIndex = 1;
			this.ShapeValue.Width = 184;
			// 
			// labelControl8
			// 
			this.labelControl8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl8.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl8.LineVisible = true;
			this.labelControl8.Location = new System.Drawing.Point(5, 108);
			this.labelControl8.Name = "labelControl8";
			this.labelControl8.Size = new System.Drawing.Size(570, 19);
			this.labelControl8.TabIndex = 193;
			this.labelControl8.Text = "Shape scheme";
			// 
			// ShapeByColumn
			// 
			this.ShapeByColumn.EditValue = true;
			this.ShapeByColumn.Location = new System.Drawing.Point(9, 30);
			this.ShapeByColumn.Name = "ShapeByColumn";
			this.ShapeByColumn.Properties.AutoWidth = true;
			this.ShapeByColumn.Properties.Caption = "";
			this.ShapeByColumn.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ShapeByColumn.Properties.RadioGroupIndex = 1;
			this.ShapeByColumn.Size = new System.Drawing.Size(19, 19);
			this.ShapeByColumn.TabIndex = 192;
			this.ShapeByColumn.EditValueChanged += new System.EventHandler(this.ShapeByColumn_EditValueChanged);
			// 
			// ShapeByFixedShape
			// 
			this.ShapeByFixedShape.Location = new System.Drawing.Point(9, 72);
			this.ShapeByFixedShape.Name = "ShapeByFixedShape";
			this.ShapeByFixedShape.Properties.AutoWidth = true;
			this.ShapeByFixedShape.Properties.Caption = "Fixed shape:";
			this.ShapeByFixedShape.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ShapeByFixedShape.Properties.RadioGroupIndex = 1;
			this.ShapeByFixedShape.Size = new System.Drawing.Size(84, 19);
			this.ShapeByFixedShape.TabIndex = 191;
			this.ShapeByFixedShape.TabStop = false;
			this.ShapeByFixedShape.EditValueChanged += new System.EventHandler(this.ShapeByFixedShape_EditValueChanged);
			// 
			// FixedShapeButton
			// 
			this.FixedShapeButton.EditValue = "Item Name";
			this.FixedShapeButton.Location = new System.Drawing.Point(99, 71);
			this.FixedShapeButton.Name = "FixedShapeButton";
			editorButtonImageOptions1.Image = ((System.Drawing.Image)(resources.GetObject("editorButtonImageOptions1.Image")));
			this.FixedShapeButton.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph, "", -1, true, true, true, editorButtonImageOptions1, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), serializableAppearanceObject1, serializableAppearanceObject2, serializableAppearanceObject3, serializableAppearanceObject4, "", null, null, DevExpress.Utils.ToolTipAnchor.Default),
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.DropDown)});
			this.FixedShapeButton.Size = new System.Drawing.Size(113, 22);
			this.FixedShapeButton.TabIndex = 256;
			this.FixedShapeButton.Click += new System.EventHandler(this.FixedShape_Click);
			// 
			// MarkerShapes16x16
			// 
			this.MarkerShapes16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("MarkerShapes16x16.ImageStream")));
			this.MarkerShapes16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.MarkerShapes16x16.Images.SetKeyName(0, "MarkerSquare.bmp");
			this.MarkerShapes16x16.Images.SetKeyName(1, "MarkerDiamond.bmp");
			this.MarkerShapes16x16.Images.SetKeyName(2, "MarkerTriangle.bmp");
			this.MarkerShapes16x16.Images.SetKeyName(3, "MarkerTriangleInverted.bmp");
			this.MarkerShapes16x16.Images.SetKeyName(4, "MarkerCircle.bmp");
			this.MarkerShapes16x16.Images.SetKeyName(5, "MarkerPlus.bmp");
			this.MarkerShapes16x16.Images.SetKeyName(6, "MarkerCross.bmp");
			this.MarkerShapes16x16.Images.SetKeyName(7, "MarkerStar.bmp");
			this.MarkerShapes16x16.Images.SetKeyName(8, "MarkerPentagon.bmp");
			this.MarkerShapes16x16.Images.SetKeyName(9, "MarkerHexagon.bmp");
			// 
			// ShapeColumnSelector
			// 
			this.ShapeColumnSelector.Appearance.BackColor = System.Drawing.Color.White;
			this.ShapeColumnSelector.Appearance.BorderColor = System.Drawing.Color.Silver;
			this.ShapeColumnSelector.Appearance.Options.UseBackColor = true;
			this.ShapeColumnSelector.Appearance.Options.UseBorderColor = true;
			this.ShapeColumnSelector.Location = new System.Drawing.Point(33, 27);
			this.ShapeColumnSelector.Margin = new System.Windows.Forms.Padding(0);
			this.ShapeColumnSelector.Name = "ShapeColumnSelector";
			this.ShapeColumnSelector.OptionIncludeNoneItem = false;
			this.ShapeColumnSelector.Size = new System.Drawing.Size(542, 26);
			this.ShapeColumnSelector.TabIndex = 257;
			// 
			// labelControl12
			// 
			this.labelControl12.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl12.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl12.LineVisible = true;
			this.labelControl12.Location = new System.Drawing.Point(5, 2);
			this.labelControl12.Name = "labelControl12";
			this.labelControl12.Size = new System.Drawing.Size(570, 22);
			this.labelControl12.TabIndex = 197;
			this.labelControl12.Text = "Shape by";
			// 
			// ShapeByBaseProperties
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ShapeColumnSelector);
			this.Controls.Add(this.FixedShapeButton);
			this.Controls.Add(this.labelControl12);
			this.Controls.Add(this.ShapeSchemeGrid);
			this.Controls.Add(this.labelControl8);
			this.Controls.Add(this.ShapeByColumn);
			this.Controls.Add(this.ShapeByFixedShape);
			this.Name = "ShapeByBaseProperties";
			this.Size = new System.Drawing.Size(580, 488);
			((System.ComponentModel.ISupportInitialize)(this.ShapeSchemeGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ShapeSchemeGridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(repositoryItemImageComboBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ShapeByColumn.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ShapeByFixedShape.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FixedShapeButton.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private DevExpress.XtraGrid.GridControl ShapeSchemeGrid;
		private DevExpress.XtraGrid.Views.Grid.GridView ShapeSchemeGridView;
		private DevExpress.XtraGrid.Columns.GridColumn ShapeColumn;
		private DevExpress.XtraGrid.Columns.GridColumn ShapeValue;
		private DevExpress.XtraEditors.LabelControl labelControl8;
		private DevExpress.XtraEditors.CheckEdit ShapeByColumn;
		private DevExpress.XtraEditors.CheckEdit ShapeByFixedShape;
		private DevExpress.XtraEditors.ButtonEdit FixedShapeButton;
		private System.Windows.Forms.ImageList MarkerShapes16x16;
		private ColumnSelectorControl ShapeColumnSelector;
		private DevExpress.XtraEditors.LabelControl labelControl12;
	}
}