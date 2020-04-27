namespace Mobius.ClientComponents
{
	partial class CriteriaDictMultSelect
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CriteriaDictMultSelect));
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.DeselectAll = new DevExpress.XtraEditors.SimpleButton();
			this.SelectAll = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.Prompt = new DevExpress.XtraEditors.LabelControl();
			this.CheckList = new DevExpress.XtraEditors.CheckedListBoxControl();
			this.SearchText = new DevExpress.XtraEditors.TextEdit();
			this.SearchPic = new System.Windows.Forms.Label();
			this.SelectedItemText = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.CheckList)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SearchText.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.Location = new System.Drawing.Point(199, 389);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(68, 22);
			this.OK.TabIndex = 46;
			this.OK.Text = "OK";
			// 
			// DeselectAll
			// 
			this.DeselectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DeselectAll.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DeselectAll.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.DeselectAll.Appearance.Options.UseFont = true;
			this.DeselectAll.Appearance.Options.UseForeColor = true;
			this.DeselectAll.Cursor = System.Windows.Forms.Cursors.Default;
			this.DeselectAll.Location = new System.Drawing.Point(83, 389);
			this.DeselectAll.Name = "DeselectAll";
			this.DeselectAll.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.DeselectAll.Size = new System.Drawing.Size(72, 22);
			this.DeselectAll.TabIndex = 48;
			this.DeselectAll.Text = "Deselect All";
			this.DeselectAll.Click += new System.EventHandler(this.DeselectAll_Click);
			// 
			// SelectAll
			// 
			this.SelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SelectAll.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SelectAll.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SelectAll.Appearance.Options.UseFont = true;
			this.SelectAll.Appearance.Options.UseForeColor = true;
			this.SelectAll.Cursor = System.Windows.Forms.Cursors.Default;
			this.SelectAll.Location = new System.Drawing.Point(5, 389);
			this.SelectAll.Name = "SelectAll";
			this.SelectAll.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.SelectAll.Size = new System.Drawing.Size(72, 22);
			this.SelectAll.TabIndex = 47;
			this.SelectAll.Text = "Select All";
			this.SelectAll.Click += new System.EventHandler(this.SelectAll_Click);
			// 
			// Cancel
			// 
			this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Cancel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Cancel.Appearance.Options.UseFont = true;
			this.Cancel.Appearance.Options.UseForeColor = true;
			this.Cancel.Cursor = System.Windows.Forms.Cursors.Default;
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = new System.Drawing.Point(273, 389);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(68, 22);
			this.Cancel.TabIndex = 45;
			this.Cancel.Text = "Cancel";
			// 
			// Prompt
			// 
			this.Prompt.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Prompt.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Prompt.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Prompt.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.Prompt.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.Prompt.Cursor = System.Windows.Forms.Cursors.Default;
			this.Prompt.Location = new System.Drawing.Point(5, 5);
			this.Prompt.Name = "Prompt";
			this.Prompt.Size = new System.Drawing.Size(318, 24);
			this.Prompt.TabIndex = 44;
			this.Prompt.Tag = "Prompt";
			this.Prompt.Text = "Prompt Prompt Prompt Prompt Prompt Prompt Prompt Prompt Prompt Prompt Prompt Prom" +
    "pt Prompt Prompt";
			// 
			// CheckList
			// 
			this.CheckList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CheckList.CheckOnClick = true;
			this.CheckList.Location = new System.Drawing.Point(5, 67);
			this.CheckList.Name = "CheckList";
			this.CheckList.Size = new System.Drawing.Size(337, 279);
			this.CheckList.TabIndex = 49;
			this.CheckList.ItemCheck += new DevExpress.XtraEditors.Controls.ItemCheckEventHandler(this.CheckList_ItemCheck);
			this.CheckList.SelectedIndexChanged += new System.EventHandler(this.CheckList_SelectedIndexChanged);
			// 
			// SearchText
			// 
			this.SearchText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SearchText.Location = new System.Drawing.Point(5, 40);
			this.SearchText.Name = "SearchText";
			this.SearchText.Size = new System.Drawing.Size(337, 20);
			this.SearchText.TabIndex = 50;
			this.SearchText.EditValueChanged += new System.EventHandler(this.SearchText_EditValueChanged);
			this.SearchText.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SearchText_KeyPress);
			// 
			// SearchPic
			// 
			this.SearchPic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SearchPic.BackColor = System.Drawing.Color.White;
			this.SearchPic.Image = ((System.Drawing.Image)(resources.GetObject("SearchPic.Image")));
			this.SearchPic.Location = new System.Drawing.Point(324, 43);
			this.SearchPic.Name = "SearchPic";
			this.SearchPic.Size = new System.Drawing.Size(14, 14);
			this.SearchPic.TabIndex = 51;
			// 
			// SelectedItemText
			// 
			this.SelectedItemText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectedItemText.Appearance.BackColor = System.Drawing.Color.WhiteSmoke;
			this.SelectedItemText.Appearance.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
			this.SelectedItemText.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.SelectedItemText.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.SelectedItemText.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.SelectedItemText.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.SelectedItemText.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
			this.SelectedItemText.Location = new System.Drawing.Point(5, 352);
			this.SelectedItemText.Name = "SelectedItemText";
			this.SelectedItemText.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.SelectedItemText.Size = new System.Drawing.Size(336, 31);
			this.SelectedItemText.TabIndex = 52;
			this.SelectedItemText.Text = resources.GetString("SelectedItemText.Text");
			// 
			// CriteriaDictMultSelect
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(347, 416);
			this.Controls.Add(this.SelectedItemText);
			this.Controls.Add(this.SearchPic);
			this.Controls.Add(this.SearchText);
			this.Controls.Add(this.CheckList);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.DeselectAll);
			this.Controls.Add(this.SelectAll);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.Prompt);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CriteriaDictMultSelect";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "CriteriaDictMultSelect";
			this.Activated += new System.EventHandler(this.CriteriaDictMultSelect_Activated);
			((System.ComponentModel.ISupportInitialize)(this.CheckList)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SearchText.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton DeselectAll;
		public DevExpress.XtraEditors.SimpleButton SelectAll;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.LabelControl Prompt;
		private DevExpress.XtraEditors.CheckedListBoxControl CheckList;
		private DevExpress.XtraEditors.TextEdit SearchText;
		private System.Windows.Forms.Label SearchPic;
		private DevExpress.XtraEditors.LabelControl SelectedItemText;
	}
}