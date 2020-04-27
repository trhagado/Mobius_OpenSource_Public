using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;
using Mobius.NativeSessionClient;

using DevExpress.XtraEditors;

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Mobius "About" form
	/// </summary>

	public class AboutDialog : XtraForm
	{
		static AboutDialog Instance;

		public System.Windows.Forms.PictureBox MobiusIcon;
		public DevExpress.XtraEditors.LabelControl ClientVersion;
		public DevExpress.XtraEditors.LabelControl ServicesVersion;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.LabelControl Server;
		public LabelControl labelControl1;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AboutDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Text = "About " + UmlautMobius.String;
			return;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutDialog));
			this.ClientVersion = new DevExpress.XtraEditors.LabelControl();
			this.ServicesVersion = new DevExpress.XtraEditors.LabelControl();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Server = new DevExpress.XtraEditors.LabelControl();
			this.MobiusIcon = new System.Windows.Forms.PictureBox();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.MobiusIcon)).BeginInit();
			this.SuspendLayout();
			// 
			// ClientVersion
			// 
			this.ClientVersion.Location = new System.Drawing.Point(69, 9);
			this.ClientVersion.Name = "ClientVersion";
			this.ClientVersion.Size = new System.Drawing.Size(192, 13);
			this.ClientVersion.TabIndex = 1;
			this.ClientVersion.Text = "Mobius Client: 1.2.3.4, January 1, 2010";
			// 
			// ServicesVersion
			// 
			this.ServicesVersion.Location = new System.Drawing.Point(69, 30);
			this.ServicesVersion.Name = "ServicesVersion";
			this.ServicesVersion.Size = new System.Drawing.Size(197, 13);
			this.ServicesVersion.TabIndex = 2;
			this.ServicesVersion.Text = "Mobius Server: 1.2.3.4, January 1, 2010";
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
			this.OK.Location = new System.Drawing.Point(235, 79);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(68, 24);
			this.OK.TabIndex = 24;
			this.OK.Tag = "OK";
			this.OK.Text = "OK";
			// 
			// Server
			// 
			this.Server.Location = new System.Drawing.Point(72, 47);
			this.Server.Name = "Server";
			this.Server.Size = new System.Drawing.Size(76, 13);
			this.Server.TabIndex = 25;
			this.Server.Text = "ServerName (1)";
			// 
			// MobiusIcon
			// 
			this.MobiusIcon.Image = ((System.Drawing.Image)(resources.GetObject("MobiusIcon.Image")));
			this.MobiusIcon.Location = new System.Drawing.Point(10, 9);
			this.MobiusIcon.Name = "MobiusIcon";
			this.MobiusIcon.Size = new System.Drawing.Size(48, 48);
			this.MobiusIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.MobiusIcon.TabIndex = 0;
			this.MobiusIcon.TabStop = false;
			// 
			// labelControl1
			// 
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(-2, 68);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(314, 6);
			this.labelControl1.TabIndex = 26;
			// 
			// AboutDialog
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.CancelButton = this.OK;
			this.ClientSize = new System.Drawing.Size(309, 109);
			this.Controls.Add(this.Server);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.ServicesVersion);
			this.Controls.Add(this.ClientVersion);
			this.Controls.Add(this.MobiusIcon);
			this.Controls.Add(this.labelControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "About Mobius";
			((System.ComponentModel.ISupportInitialize)(this.MobiusIcon)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// Show the About dialog
		/// </summary>

		public static new void Show()
		{
			string clientVersion, servicesVersion, server;

			if (Instance == null) Instance = new AboutDialog();

			ServiceFacade.ServiceFacade.GetClientAndServicesVersions(out clientVersion, out servicesVersion, out server, includeSessionCount: true);

			Instance.ClientVersion.Text = "Mobius Client: " + clientVersion;
			Instance.ServicesVersion.Text = "Mobius Services: " + servicesVersion;
			Instance.Server.Text = server;

			Instance.ShowDialog(SessionManager.ActiveForm); // put up the form
			return;
		}
	}
}
