using Mobius.ComOps;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Summary description for Logon.
	/// </summary>
	public class Logon : XtraForm
	{
		public static Logon Instance;
		UserInfo UserInfo = null;
		int Attempts = 0;

		public DevExpress.XtraEditors.SimpleButton Cancel;
		public System.Windows.Forms.ToolTip ToolTip1;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.TextEdit Password;
		public DevExpress.XtraEditors.TextEdit Username;
		public DevExpress.XtraEditors.LabelControl label;
		public DevExpress.XtraEditors.LabelControl Label2;
		public DevExpress.XtraEditors.LabelControl Label1;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.ComponentModel.IContainer components;

		public Logon()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		public static UserInfo Show (string initialUserName)
		{
			if (Instance == null) Instance = new Logon();

			UserInfo uInf = new UserInfo();

			Instance.Username.Text = initialUserName;
			Instance.Password.Text = "";
			Instance.Attempts = 0;

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return null;
			else return Instance.UserInfo;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Logon));
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Password = new DevExpress.XtraEditors.TextEdit();
			this.Username = new DevExpress.XtraEditors.TextEdit();
			this.label = new DevExpress.XtraEditors.LabelControl();
			this.Label2 = new DevExpress.XtraEditors.LabelControl();
			this.Label1 = new DevExpress.XtraEditors.LabelControl();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.Password.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Username.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// Cancel
			// 
			this.Cancel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Cancel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Cancel.Appearance.Options.UseFont = true;
			this.Cancel.Appearance.Options.UseForeColor = true;
			this.Cancel.Cursor = System.Windows.Forms.Cursors.Default;
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = new System.Drawing.Point(204, 118);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(68, 24);
			this.Cancel.TabIndex = 10;
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// OK
			// 
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(128, 118);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(68, 24);
			this.OK.TabIndex = 9;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// Password
			// 
			this.Password.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.Password.Location = new System.Drawing.Point(88, 86);
			this.Password.Name = "Password";
			this.Password.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.Password.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.Password.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Password.Properties.Appearance.Options.UseBackColor = true;
			this.Password.Properties.Appearance.Options.UseFont = true;
			this.Password.Properties.Appearance.Options.UseForeColor = true;
			this.Password.Properties.PasswordChar = '*';
			this.Password.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Password.Size = new System.Drawing.Size(184, 19);
			this.Password.TabIndex = 8;
			// 
			// Username
			// 
			this.Username.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.Username.Location = new System.Drawing.Point(88, 52);
			this.Username.Name = "Username";
			this.Username.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.Username.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.Username.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Username.Properties.Appearance.Options.UseBackColor = true;
			this.Username.Properties.Appearance.Options.UseFont = true;
			this.Username.Properties.Appearance.Options.UseForeColor = true;
			this.Username.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Username.Size = new System.Drawing.Size(184, 19);
			this.Username.TabIndex = 7;
			// 
			// label
			// 
			this.label.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.label.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.label.Appearance.Options.UseBackColor = true;
			this.label.Appearance.Options.UseFont = true;
			this.label.Appearance.Options.UseForeColor = true;
			this.label.Cursor = System.Windows.Forms.Cursors.Default;
			this.label.Location = new System.Drawing.Point(46, 15);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(149, 13);
			this.label.TabIndex = 13;
			this.label.Text = "Enter user name and password:";
			// 
			// Label2
			// 
			this.Label2.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Label2.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label2.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label2.Appearance.Options.UseBackColor = true;
			this.Label2.Appearance.Options.UseFont = true;
			this.Label2.Appearance.Options.UseForeColor = true;
			this.Label2.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label2.Location = new System.Drawing.Point(24, 86);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(53, 14);
			this.Label2.TabIndex = 12;
			this.Label2.Text = "Password:";
			// 
			// Label1
			// 
			this.Label1.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Label1.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label1.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label1.Appearance.Options.UseBackColor = true;
			this.Label1.Appearance.Options.UseFont = true;
			this.Label1.Appearance.Options.UseForeColor = true;
			this.Label1.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label1.Location = new System.Drawing.Point(24, 52);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(54, 13);
			this.Label1.TabIndex = 11;
			this.Label1.Text = "User name:";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(4, 4);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(32, 32);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 14;
			this.pictureBox1.TabStop = false;
			// 
			// Logon
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(278, 147);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Password);
			this.Controls.Add(this.Username);
			this.Controls.Add(this.label);
			this.Controls.Add(this.Label2);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.Cancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Logon";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Mobius Logon";
			this.Load += new System.EventHandler(this.Logon_Load);
			this.Activated += new System.EventHandler(this.Logon_Activated);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Logon_Closing);
			((System.ComponentModel.ISupportInitialize)(this.Password.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Username.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion


		private void Logon_Load(object sender, System.EventArgs e)
		{
		
		}

		private void OK_Click(object sender, System.EventArgs e)
		{
			if (Security.Authenticate(Username.Text, Password.Text))
				DialogResult = DialogResult.OK;

			Attempts++;

			if (Attempts >= 3)
			{
				MessageBoxMx.Show(
					"The system could not log you on.\n" +
					"Make sure your User name is correct, then type your password again.\n" +
					"Letters in passwords must be typed in the correct case.",
					"Logon Message");

				DialogResult = DialogResult.Cancel;
			}
		}

		private void Cancel_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void Logon_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
		}

		private void Logon_Activated(object sender, System.EventArgs e)
		{
			if (Username.Text=="") Username.Focus();
			else Password.Focus();
		}
	}
}
