using DevExpress.XtraEditors;

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class EditHyperlink : XtraForm
	{
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.LabelControl label1;
		public DevExpress.XtraEditors.LabelControl label2;
		public DevExpress.XtraEditors.SimpleButton RemoveLink;
		public DevExpress.XtraEditors.TextEdit Address;
		internal MemoEdit DisplayText;
		public LabelControl labelControl1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public EditHyperlink()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.label1 = new DevExpress.XtraEditors.LabelControl();
			this.label2 = new DevExpress.XtraEditors.LabelControl();
			this.RemoveLink = new DevExpress.XtraEditors.SimpleButton();
			this.Address = new DevExpress.XtraEditors.TextEdit();
			this.DisplayText = new DevExpress.XtraEditors.MemoEdit();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.Address.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DisplayText.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// OK
			// 
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.Location = new System.Drawing.Point(292, 88);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.ShowToolTips = false;
			this.OK.Size = new System.Drawing.Size(68, 23);
			this.OK.TabIndex = 37;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// Cancel
			// 
			this.Cancel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Cancel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Cancel.Appearance.Options.UseFont = true;
			this.Cancel.Appearance.Options.UseForeColor = true;
			this.Cancel.Cursor = System.Windows.Forms.Cursors.Default;
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = new System.Drawing.Point(372, 88);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(68, 23);
			this.Cancel.TabIndex = 36;
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(75, 13);
			this.label1.TabIndex = 38;
			this.label1.Text = "Text to display:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(38, 51);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(43, 13);
			this.label2.TabIndex = 39;
			this.label2.Text = "Address:";
			// 
			// RemoveLink
			// 
			this.RemoveLink.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RemoveLink.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.RemoveLink.Appearance.Options.UseFont = true;
			this.RemoveLink.Appearance.Options.UseForeColor = true;
			this.RemoveLink.Cursor = System.Windows.Forms.Cursors.Default;
			this.RemoveLink.Location = new System.Drawing.Point(354, 49);
			this.RemoveLink.Name = "RemoveLink";
			this.RemoveLink.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RemoveLink.Size = new System.Drawing.Size(86, 20);
			this.RemoveLink.TabIndex = 40;
			this.RemoveLink.Text = "Remove Link";
			this.RemoveLink.Click += new System.EventHandler(this.RemoveLink_Click);
			// 
			// Address
			// 
			this.Address.EditValue = "";
			this.Address.Location = new System.Drawing.Point(88, 49);
			this.Address.Name = "Address";
			this.Address.Size = new System.Drawing.Size(254, 20);
			this.Address.TabIndex = 42;
			// 
			// DisplayText
			// 
			this.DisplayText.Location = new System.Drawing.Point(88, 9);
			this.DisplayText.Name = "DisplayText";
			this.DisplayText.Size = new System.Drawing.Size(254, 33);
			this.DisplayText.TabIndex = 44;
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(1, 73);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(443, 10);
			this.labelControl1.TabIndex = 45;
			// 
			// EditHyperlink
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(446, 117);
			this.Controls.Add(this.Address);
			this.Controls.Add(this.RemoveLink);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.DisplayText);
			this.Controls.Add(this.labelControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EditHyperlink";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Edit Hyperlink";
			this.Activated += new System.EventHandler(this.EditHyperlink_Activated);
			((System.ComponentModel.ISupportInitialize)(this.Address.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DisplayText.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private void RemoveLink_Click(object sender, System.EventArgs e)
		{
			Address.Text = "";
			OK_Click(null,null);
			this.DialogResult = DialogResult.OK;
		}

		private void OK_Click(object sender, System.EventArgs e)
		{
			if (DisplayText.Text == "" && Address.Text != "") // set cell value to hyperlink if no display text
				DisplayText.Text = Address.Text;
		}

		private void Cancel_Click(object sender, System.EventArgs e)
		{
		
		}

		private void EditHyperlink_Activated(object sender, System.EventArgs e)
		{
			if (DisplayText.Text == " ") DisplayText.Text = "";
			Address.Focus();
		}
	}
}
