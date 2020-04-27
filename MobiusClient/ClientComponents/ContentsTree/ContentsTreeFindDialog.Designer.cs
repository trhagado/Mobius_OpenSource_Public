namespace Mobius.ClientComponents
{
	partial class ContentsTreeFindDialog
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
			this.Prompt = new DevExpress.XtraEditors.LabelControl();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.MetaTables = new DevExpress.XtraEditors.CheckEdit();
			this.AnnotationTables = new DevExpress.XtraEditors.CheckEdit();
			this.CalcFields = new DevExpress.XtraEditors.CheckEdit();
			this.Queries = new DevExpress.XtraEditors.CheckEdit();
			this.CidLists = new DevExpress.XtraEditors.CheckEdit();
			this.HyperLinks = new DevExpress.XtraEditors.CheckEdit();
			this.Folders = new DevExpress.XtraEditors.CheckEdit();
			this.AllButton = new DevExpress.XtraEditors.SimpleButton();
			this.ClearButton = new DevExpress.XtraEditors.SimpleButton();
			this.MyUserObjects = new DevExpress.XtraEditors.CheckEdit();
			this.AllUserObjects = new DevExpress.XtraEditors.CheckEdit();
			this.QueryStringCtl = new DevExpress.XtraEditors.TextEdit();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
			this.DateEdit = new DevExpress.XtraEditors.DateEdit();
			this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
			this.TreeResultDisplayCheckEdit = new DevExpress.XtraEditors.CheckEdit();
			this.ListResultDisplayCheckEdit = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.MetaTables.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AnnotationTables.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CalcFields.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Queries.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CidLists.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HyperLinks.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Folders.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MyUserObjects.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AllUserObjects.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.QueryStringCtl.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DateEdit.Properties.CalendarTimeProperties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DateEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TreeResultDisplayCheckEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ListResultDisplayCheckEdit.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// Prompt
			// 
			this.Prompt.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Prompt.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.Prompt.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.Prompt.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.Prompt.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.Prompt.Location = new System.Drawing.Point(6, 7);
			this.Prompt.Name = "Prompt";
			this.Prompt.Size = new System.Drawing.Size(310, 32);
			this.Prompt.TabIndex = 1;
			this.Prompt.Text = "Enter one or more words, partial words or an assay code that you want to search f" +
    "or in the contents tree.";
			// 
			// Cancel
			// 
			this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = new System.Drawing.Point(254, 457);
			this.Cancel.Name = "Cancel";
			this.Cancel.Size = new System.Drawing.Size(60, 22);
			this.Cancel.TabIndex = 5;
			this.Cancel.Text = "Cancel";
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Location = new System.Drawing.Point(186, 457);
			this.OK.Name = "OK";
			this.OK.Size = new System.Drawing.Size(60, 22);
			this.OK.TabIndex = 4;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// MetaTables
			// 
			this.MetaTables.EditValue = true;
			this.MetaTables.Location = new System.Drawing.Point(18, 186);
			this.MetaTables.Name = "MetaTables";
			this.MetaTables.Properties.Caption = "Data tables and assays (built-in)";
			this.MetaTables.Size = new System.Drawing.Size(205, 19);
			this.MetaTables.TabIndex = 7;
			// 
			// AnnotationTables
			// 
			this.AnnotationTables.EditValue = true;
			this.AnnotationTables.Location = new System.Drawing.Point(18, 210);
			this.AnnotationTables.Name = "AnnotationTables";
			this.AnnotationTables.Properties.Caption = "Annotation tables";
			this.AnnotationTables.Size = new System.Drawing.Size(205, 19);
			this.AnnotationTables.TabIndex = 8;
			// 
			// CalcFields
			// 
			this.CalcFields.EditValue = true;
			this.CalcFields.Location = new System.Drawing.Point(19, 234);
			this.CalcFields.Name = "CalcFields";
			this.CalcFields.Properties.Caption = "Calculated fields";
			this.CalcFields.Size = new System.Drawing.Size(205, 19);
			this.CalcFields.TabIndex = 9;
			// 
			// Queries
			// 
			this.Queries.EditValue = true;
			this.Queries.Location = new System.Drawing.Point(19, 259);
			this.Queries.Name = "Queries";
			this.Queries.Properties.Caption = "Queries";
			this.Queries.Size = new System.Drawing.Size(205, 19);
			this.Queries.TabIndex = 10;
			// 
			// CidLists
			// 
			this.CidLists.EditValue = true;
			this.CidLists.Location = new System.Drawing.Point(19, 284);
			this.CidLists.Name = "CidLists";
			this.CidLists.Properties.Caption = "Compound id lists";
			this.CidLists.Size = new System.Drawing.Size(205, 19);
			this.CidLists.TabIndex = 11;
			// 
			// HyperLinks
			// 
			this.HyperLinks.EditValue = true;
			this.HyperLinks.Location = new System.Drawing.Point(18, 309);
			this.HyperLinks.Name = "HyperLinks";
			this.HyperLinks.Properties.Caption = "Hyperlinks";
			this.HyperLinks.Size = new System.Drawing.Size(144, 19);
			this.HyperLinks.TabIndex = 12;
			// 
			// Folders
			// 
			this.Folders.EditValue = true;
			this.Folders.Location = new System.Drawing.Point(18, 162);
			this.Folders.Name = "Folders";
			this.Folders.Properties.Caption = "Folder names";
			this.Folders.Size = new System.Drawing.Size(205, 19);
			this.Folders.TabIndex = 13;
			// 
			// AllButton
			// 
			this.AllButton.Location = new System.Drawing.Point(225, 308);
			this.AllButton.Name = "AllButton";
			this.AllButton.Size = new System.Drawing.Size(40, 22);
			this.AllButton.TabIndex = 14;
			this.AllButton.Text = "All";
			this.AllButton.Click += new System.EventHandler(this.AllButton_Click);
			// 
			// ClearButton
			// 
			this.ClearButton.Location = new System.Drawing.Point(271, 308);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = new System.Drawing.Size(40, 22);
			this.ClearButton.TabIndex = 15;
			this.ClearButton.Text = "Clear";
			this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
			// 
			// MyUserObjects
			// 
			this.MyUserObjects.Location = new System.Drawing.Point(103, 365);
			this.MyUserObjects.Name = "MyUserObjects";
			this.MyUserObjects.Properties.Caption = "Myself only";
			this.MyUserObjects.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.MyUserObjects.Properties.RadioGroupIndex = 2;
			this.MyUserObjects.Size = new System.Drawing.Size(87, 19);
			this.MyUserObjects.TabIndex = 17;
			this.MyUserObjects.TabStop = false;
			// 
			// AllUserObjects
			// 
			this.AllUserObjects.EditValue = true;
			this.AllUserObjects.Location = new System.Drawing.Point(18, 365);
			this.AllUserObjects.Name = "AllUserObjects";
			this.AllUserObjects.Properties.Caption = "Everyone";
			this.AllUserObjects.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.AllUserObjects.Properties.RadioGroupIndex = 2;
			this.AllUserObjects.Size = new System.Drawing.Size(87, 19);
			this.AllUserObjects.TabIndex = 19;
			// 
			// QueryStringCtl
			// 
			this.QueryStringCtl.Location = new System.Drawing.Point(12, 45);
			this.QueryStringCtl.Name = "QueryStringCtl";
			this.QueryStringCtl.Size = new System.Drawing.Size(304, 20);
			this.QueryStringCtl.TabIndex = 0;
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(6, 137);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(310, 13);
			this.labelControl1.TabIndex = 23;
			this.labelControl1.Text = "Types of items to include in the search";
			// 
			// labelControl2
			// 
			this.labelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.LineVisible = true;
			this.labelControl2.Location = new System.Drawing.Point(6, 345);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(310, 13);
			this.labelControl2.TabIndex = 24;
			this.labelControl2.Text = "Owners of queries, annotation tables, etc. to consider";
			// 
			// labelControl3
			// 
			this.labelControl3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl3.Location = new System.Drawing.Point(6, 87);
			this.labelControl3.Name = "labelControl3";
			this.labelControl3.Size = new System.Drawing.Size(310, 13);
			this.labelControl3.TabIndex = 25;
			this.labelControl3.Text = "Filter tree to include only assays with new data added since:";
			// 
			// DateEdit
			// 
			this.DateEdit.EditValue = null;
			this.DateEdit.Location = new System.Drawing.Point(12, 106);
			this.DateEdit.Name = "DateEdit";
			this.DateEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.DateEdit.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.DateEdit.Size = new System.Drawing.Size(304, 20);
			this.DateEdit.TabIndex = 26;
			// 
			// labelControl4
			// 
			this.labelControl4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl4.LineVisible = true;
			this.labelControl4.Location = new System.Drawing.Point(-3, 439);
			this.labelControl4.Name = "labelControl4";
			this.labelControl4.Size = new System.Drawing.Size(326, 10);
			this.labelControl4.TabIndex = 27;
			// 
			// labelControl5
			// 
			this.labelControl5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl5.Location = new System.Drawing.Point(133, 70);
			this.labelControl5.Name = "labelControl5";
			this.labelControl5.Size = new System.Drawing.Size(48, 13);
			this.labelControl5.TabIndex = 28;
			this.labelControl5.Text = "And / Or";
			// 
			// TreeResultDisplayCheckEdit
			// 
			this.TreeResultDisplayCheckEdit.Location = new System.Drawing.Point(75, 418);
			this.TreeResultDisplayCheckEdit.Name = "TreeResultDisplayCheckEdit";
			this.TreeResultDisplayCheckEdit.Properties.Caption = "Tree";
			this.TreeResultDisplayCheckEdit.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.TreeResultDisplayCheckEdit.Properties.RadioGroupIndex = 3;
			this.TreeResultDisplayCheckEdit.Size = new System.Drawing.Size(87, 19);
			this.TreeResultDisplayCheckEdit.TabIndex = 29;
			this.TreeResultDisplayCheckEdit.TabStop = false;
			this.TreeResultDisplayCheckEdit.CheckedChanged += new System.EventHandler(this.TreeResultDisplayCheckEdit_CheckedChanged);
			// 
			// ListResultDisplayCheckEdit
			// 
			this.ListResultDisplayCheckEdit.EditValue = true;
			this.ListResultDisplayCheckEdit.Location = new System.Drawing.Point(19, 418);
			this.ListResultDisplayCheckEdit.Name = "ListResultDisplayCheckEdit";
			this.ListResultDisplayCheckEdit.Properties.Caption = "List";
			this.ListResultDisplayCheckEdit.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ListResultDisplayCheckEdit.Properties.RadioGroupIndex = 3;
			this.ListResultDisplayCheckEdit.Size = new System.Drawing.Size(87, 19);
			this.ListResultDisplayCheckEdit.TabIndex = 30;
			// 
			// labelControl6
			// 
			this.labelControl6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl6.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl6.LineVisible = true;
			this.labelControl6.Location = new System.Drawing.Point(6, 398);
			this.labelControl6.Name = "labelControl6";
			this.labelControl6.Size = new System.Drawing.Size(310, 13);
			this.labelControl6.TabIndex = 31;
			this.labelControl6.Text = "Format to use for the display of the matching items";
			// 
			// ContentsTreeFindDialog
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(323, 484);
			this.Controls.Add(this.TreeResultDisplayCheckEdit);
			this.Controls.Add(this.ListResultDisplayCheckEdit);
			this.Controls.Add(this.labelControl6);
			this.Controls.Add(this.labelControl5);
			this.Controls.Add(this.labelControl4);
			this.Controls.Add(this.DateEdit);
			this.Controls.Add(this.MyUserObjects);
			this.Controls.Add(this.Folders);
			this.Controls.Add(this.AllUserObjects);
			this.Controls.Add(this.labelControl3);
			this.Controls.Add(this.MetaTables);
			this.Controls.Add(this.labelControl2);
			this.Controls.Add(this.ClearButton);
			this.Controls.Add(this.AllButton);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.AnnotationTables);
			this.Controls.Add(this.CalcFields);
			this.Controls.Add(this.Queries);
			this.Controls.Add(this.CidLists);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.HyperLinks);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Prompt);
			this.Controls.Add(this.QueryStringCtl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ContentsTreeFindDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Find in Contents";
			this.Shown += new System.EventHandler(this.ContentsTreeFindDialog_Shown);
			((System.ComponentModel.ISupportInitialize)(this.MetaTables.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AnnotationTables.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CalcFields.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Queries.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CidLists.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HyperLinks.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Folders.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MyUserObjects.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AllUserObjects.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.QueryStringCtl.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DateEdit.Properties.CalendarTimeProperties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DateEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TreeResultDisplayCheckEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ListResultDisplayCheckEdit.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.LabelControl Prompt;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.SimpleButton OK;
		private DevExpress.XtraEditors.CheckEdit MetaTables;
		private DevExpress.XtraEditors.CheckEdit AnnotationTables;
		private DevExpress.XtraEditors.CheckEdit CalcFields;
		private DevExpress.XtraEditors.CheckEdit Queries;
		private DevExpress.XtraEditors.CheckEdit CidLists;
		private DevExpress.XtraEditors.CheckEdit HyperLinks;
		private DevExpress.XtraEditors.CheckEdit Folders;
		public DevExpress.XtraEditors.SimpleButton AllButton;
		public DevExpress.XtraEditors.SimpleButton ClearButton;
		private DevExpress.XtraEditors.CheckEdit MyUserObjects;
		private DevExpress.XtraEditors.CheckEdit AllUserObjects;
		public DevExpress.XtraEditors.TextEdit QueryStringCtl;
		private DevExpress.XtraEditors.LabelControl labelControl1;
		private DevExpress.XtraEditors.LabelControl labelControl2;
		private DevExpress.XtraEditors.LabelControl labelControl3;
		private DevExpress.XtraEditors.LabelControl labelControl4;
		private DevExpress.XtraEditors.LabelControl labelControl5;
		public DevExpress.XtraEditors.DateEdit DateEdit;
		private DevExpress.XtraEditors.CheckEdit TreeResultDisplayCheckEdit;
		private DevExpress.XtraEditors.CheckEdit ListResultDisplayCheckEdit;
		private DevExpress.XtraEditors.LabelControl labelControl6;
	}
}