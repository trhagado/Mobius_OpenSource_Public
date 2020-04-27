using Mobius.ComOps;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Summary description for NewsForm.
	/// </summary>
	public class NewsDialog : XtraForm
	{
		static NewsDialog Instance;

		public DevExpress.XtraEditors.CheckEdit ShowNewsCheckBox;
		public DevExpress.XtraEditors.SimpleButton CloseButton;
		private WebBrowser WebBrowser;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Display any new news if requested
		/// </summary>

		public static bool ShowNewNews()
		{
			try
			{
				string lastNews = Preferences.Get("ShowNews");
				if (lastNews == "false") return false; // user doesn't want to see news

				if (ClientState.IsDeveloper) return false; // debug

				string fileName = ServicesIniFile.Read("NewsFile");
				if (fileName == "") throw new Exception("NewsFile not defined");
				DateTime dt = File.GetLastWriteTime(fileName);

				string fileDate = String.Format("{0,4:0000}{1,2:00}{2,2:00}", dt.Year, dt.Month, dt.Day);

				if (String.Compare(fileDate, lastNews) > 0 || lastNews == "")
				{
					ShowNews(); // show the news
					UserObjectDao.SetUserParameter(SS.I.UserName, "ShowNews", fileDate);
					return true;
				}

				else return false;
			}

			catch (Exception ex)
			{
				//if (ClientState.IsDeveloper)
				// MessageBoxMx.ShowError(ex.Message);
				return false;
			}
		}

		/// <summary>
		/// Display the news file
		/// </summary>

		public static void ShowNews()
		{
			try
			{
				string fileName = ServicesIniFile.Read("NewsFile");
				if (fileName == "") return;

				if (Instance == null)
					Instance = new NewsDialog();

				Instance.ShowNewsCheckBox.Checked = Preferences.GetBool("ShowNews");
				Instance.Show();
				Application.DoEvents(); // allow form to draw before doing navigate
				Instance.WebBrowser.Navigate(fileName);
			}


			catch (Exception ex) { return; }
		}

		/// <summary>
		/// Set user parameter enabling/disabling display of news
		/// </summary>
		/// <param name="enabled"></param>

		public static void SetShowNewsEnabled(
			bool enabled)
		{
			if (enabled)
				UserObjectDao.SetUserParameter(SS.I.UserName, "ShowNews", SS.I.CurrentDate);
			else UserObjectDao.SetUserParameter(SS.I.UserName, "ShowNews", "false");
		}

/// <summary>
/// Constructor
/// </summary>

		public NewsDialog()
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewsDialog));
			this.CloseButton = new DevExpress.XtraEditors.SimpleButton();
			this.ShowNewsCheckBox = new DevExpress.XtraEditors.CheckEdit();
			this.WebBrowser = new System.Windows.Forms.WebBrowser();
			((System.ComponentModel.ISupportInitialize)(this.ShowNewsCheckBox.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CloseButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.CloseButton.Appearance.Options.UseFont = true;
			this.CloseButton.Appearance.Options.UseForeColor = true;
			this.CloseButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = new System.Drawing.Point(528, 386);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CloseButton.Size = new System.Drawing.Size(68, 24);
			this.CloseButton.TabIndex = 25;
			this.CloseButton.Tag = "";
			this.CloseButton.Text = "Close";
			this.CloseButton.Click += new System.EventHandler(this.Close_Click);
			// 
			// ShowNewsCheckBox
			// 
			this.ShowNewsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShowNewsCheckBox.Location = new System.Drawing.Point(4, 389);
			this.ShowNewsCheckBox.Name = "ShowNewsCheckBox";
			this.ShowNewsCheckBox.Properties.Caption = "Show any \"new\" news when Mobius starts";
			this.ShowNewsCheckBox.Size = new System.Drawing.Size(256, 19);
			this.ShowNewsCheckBox.TabIndex = 26;
			this.ShowNewsCheckBox.CheckedChanged += new System.EventHandler(this.ShowNews_CheckedChanged);
			// 
			// WebBrowser
			// 
			this.WebBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.WebBrowser.Location = new System.Drawing.Point(2, 2);
			this.WebBrowser.MinimumSize = new System.Drawing.Size(20, 20);
			this.WebBrowser.Name = "WebBrowser";
			this.WebBrowser.Size = new System.Drawing.Size(594, 380);
			this.WebBrowser.TabIndex = 27;
			// 
			// NewsDialog
			// 
			this.AcceptButton = this.CloseButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.CancelButton = this.CloseButton;
			this.ClientSize = new System.Drawing.Size(598, 413);
			this.Controls.Add(this.WebBrowser);
			this.Controls.Add(this.ShowNewsCheckBox);
			this.Controls.Add(this.CloseButton);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "NewsDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Mobius News";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.NewsForm_Closing);
			((System.ComponentModel.ISupportInitialize)(this.ShowNewsCheckBox.Properties)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void Close_Click(object sender, System.EventArgs e)
		{
			this.Visible = false;
		}

		private void ShowNews_CheckedChanged(object sender, System.EventArgs e)
		{
			if (!Visible) return;
			Preferences.Set("ShowNews", ShowNewsCheckBox.Checked);
		}

		private void NewsForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			this.Visible = false;
		}
	}
}
