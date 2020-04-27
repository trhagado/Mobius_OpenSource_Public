using DevExpress.XtraEditors;

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mobius.Client
{
	/// <summary>
	/// Summary description for CriteriaDictionary.
	/// </summary>
	public class CriteriaDictionary : XtraForm
	{
		static CriteriaDictionary Instance;
		public DevExpress.XtraEditors.SimpleButton OK;
		public System.Windows.Forms.ToolTip ToolTip1;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.CheckEdit None;
		public DevExpress.XtraEditors.CheckEdit EQ;
		public DevExpress.XtraEditors.LabelControl Prompt;
		private ComboBoxEdit Value;
		private System.ComponentModel.IContainer components;

		public CriteriaDictionary()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Instance = this;
			DisableLimitFields();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CriteriaDictionary));
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.None = new DevExpress.XtraEditors.CheckEdit();
			this.EQ = new DevExpress.XtraEditors.CheckEdit();
			this.Prompt = new DevExpress.XtraEditors.LabelControl();
			this.Value = new DevExpress.XtraEditors.ComboBoxEdit();
			((System.ComponentModel.ISupportInitialize)(this.None.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EQ.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Value.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// OK
			// 
			this.OK.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(288, 110);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(68, 23);
			this.OK.TabIndex = 11;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// Cancel
			// 
			this.Cancel.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Cancel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Cancel.Appearance.Options.UseFont = true;
			this.Cancel.Appearance.Options.UseForeColor = true;
			this.Cancel.Cursor = System.Windows.Forms.Cursors.Default;
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = new System.Drawing.Point(368, 110);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(68, 23);
			this.Cancel.TabIndex = 10;
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// None
			// 
			this.None.Cursor = System.Windows.Forms.Cursors.Default;
			this.None.Location = new System.Drawing.Point(16, 87);
			this.None.Name = "None";
			this.None.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.None.Properties.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.None.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.None.Properties.Appearance.Options.UseBackColor = true;
			this.None.Properties.Appearance.Options.UseFont = true;
			this.None.Properties.Appearance.Options.UseForeColor = true;
			this.None.Properties.Caption = "&No Criteria";
			this.None.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.None.Properties.RadioGroupIndex = 1;
			this.None.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.None.Size = new System.Drawing.Size(96, 19);
			this.None.TabIndex = 8;
			this.None.TabStop = false;
			this.None.Click += new System.EventHandler(this.None_Click);
			// 
			// EQ
			// 
			this.EQ.Cursor = System.Windows.Forms.Cursors.Default;
			this.EQ.EditValue = true;
			this.EQ.Location = new System.Drawing.Point(16, 57);
			this.EQ.Name = "EQ";
			this.EQ.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.EQ.Properties.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EQ.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.EQ.Properties.Appearance.Options.UseBackColor = true;
			this.EQ.Properties.Appearance.Options.UseFont = true;
			this.EQ.Properties.Appearance.Options.UseForeColor = true;
			this.EQ.Properties.Caption = "=";
			this.EQ.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.EQ.Properties.RadioGroupIndex = 1;
			this.EQ.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.EQ.Size = new System.Drawing.Size(401, 19);
			this.EQ.TabIndex = 7;
			this.EQ.CheckedChanged += new System.EventHandler(this.EQ_CheckedChanged);
			this.EQ.Click += new System.EventHandler(this.EQ_Click);
			// 
			// Prompt
			// 
			this.Prompt.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Prompt.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Prompt.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Prompt.Appearance.Options.UseBackColor = true;
			this.Prompt.Appearance.Options.UseFont = true;
			this.Prompt.Appearance.Options.UseForeColor = true;
			this.Prompt.Appearance.Options.UseTextOptions = true;
			this.Prompt.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.Prompt.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.Prompt.Cursor = System.Windows.Forms.Cursors.Default;
			this.Prompt.Location = new System.Drawing.Point(11, 8);
			this.Prompt.Name = "Prompt";
			this.Prompt.Size = new System.Drawing.Size(426, 44);
			this.Prompt.TabIndex = 9;
			this.Prompt.Tag = "Prompt";
			this.Prompt.Text = resources.GetString("Prompt.Text");
			// 
			// Value
			// 
			this.Value.Location = new System.Drawing.Point(48, 57);
			this.Value.Name = "Value";
			this.Value.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.Value.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
			this.Value.Size = new System.Drawing.Size(272, 20);
			this.Value.TabIndex = 15;
			// 
			// CriteriaDictionary
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(442, 139);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.None);
			this.Controls.Add(this.Prompt);
			this.Controls.Add(this.Value);
			this.Controls.Add(this.EQ);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CriteriaDictionary";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "CriteriaDictionary";
			this.Load += new System.EventHandler(this.CriteriaDictionary_Load);
			this.Activated += new System.EventHandler(this.CriteriaDictionary_Activated);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.CriteriaDictionary_Closing);
			((System.ComponentModel.ISupportInitialize)(this.None.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EQ.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Value.Properties)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void CriteriaDictionary_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
		}

		private void CriteriaDictionary_Load(object sender, System.EventArgs e)
		{
		}

		private void CriteriaDictionary_Activated(object sender, System.EventArgs e)
		{
			DisableLimitFields();
			if (EQ.Checked) 
			{
				Value.Enabled = true;
				if (Visible) Value.Focus();
//				List.Cols[0].Width = List.Width;
//				Value_KeyUp(null,null); // force positioning of list
			}
			else if (None.Checked) None.Focus();
		}

		private void DisableLimitFields()
		{
			Value.Enabled = false;
		}

		private void None_Click(object sender, System.EventArgs e)
		{
			DisableLimitFields();
		}

		private void OK_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void Cancel_Click(object sender, System.EventArgs e)
		{
		}

		private void EQ_CheckedChanged(object sender, System.EventArgs e)
		{
		
		}

		private void EQ_Click(object sender, System.EventArgs e)
		{
			DisableLimitFields();
			Value.Enabled=true;
			if (Value.Visible) Value.Focus();
		}

		private void Value_TextChanged(object sender, System.EventArgs e)
		{
		}

		private void Value_Enter(object sender, System.EventArgs e)
		{
		}

		private void Value_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			try
			{
				Value.Properties.ImmediatePopup = true; // drop automatically when gets focus
			}
			catch (Exception ex) {}
		}

		/// <summary>
		/// Get selected criteria with elements separated by tabs.
		/// </summary>
		/// <returns></returns>

		public static string GetCriteria ()
		{
			if (Instance==null) return "";
			string txt="";

			if (Instance.EQ.Checked) txt = "EQ\t" + Instance.Value.Text;
			else txt = "None";

			return txt;
		}

	}
}
