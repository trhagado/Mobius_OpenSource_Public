using Mobius.ComOps;
using Mobius.ClientComponents;

using DevExpress.XtraEditors;

using System;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mobius.Client
{
	/// <summary>
	/// Splash form that is displayed during Mobius startup
	/// </summary>

	public class Splash : XtraForm
	{
		internal static Splash Instance;
		public static bool Unattended = false;
		internal IniFile IniFile; // common client settings
		internal IniFile UserIniFile; // user specific settings
		internal string LookAndFeelName;

		private Bitmap SplashBitmap;
		private Panel Panel;
		public PictureBox Picture;

		private delegate void ScalePictureDelegate();

		[DllImport("user32.dll")]
		public static extern bool MessageBeep(
			int Sound);

		/// <summary>
		/// Required designer variable.
		/// </summary>
		/// 
		private System.ComponentModel.Container components = null;

		public Splash()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Instance = this; // save static ref to self
		}

/// <summary>
/// Create & display splash form
/// </summary>
/// <returns></returns>

		public static Splash ShowForm()
		{
			Splash splash;
			splash = new Splash();

			splash.IniFile = // common client settings
				IniFile.OpenClientIniFile("MobiusClient.ini");

			splash.UserIniFile = // user specific settings
				IniFile.OpenClientIniFile("MobiusClientUser.ini", "Mobius");

			LogWindow.Display = splash.UserIniFile.ReadBool("DisplayLogWindow", false); // 

			if (!Unattended)
			{
				try // set the skin to display 
				{
					string lookAndFeelName = splash.UserIniFile.Read("LookAndFeel", "Blue");
					LookAndFeelMx.SetLookAndFeel(lookAndFeelName, splash.LookAndFeel);
				}
				catch (Exception ex) { ex = ex; }

				splash.Show();
				splash.SetSplashImage();
			}

			return splash;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Splash));
			this.Panel = new System.Windows.Forms.Panel();
			this.Picture = new System.Windows.Forms.PictureBox();
			this.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Picture)).BeginInit();
			this.SuspendLayout();
			// 
			// Panel
			// 
			this.Panel.Controls.Add(this.Picture);
			this.Panel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Panel.Location = new System.Drawing.Point(0, 0);
			this.Panel.Name = "Panel";
			this.Panel.Size = new System.Drawing.Size(412, 269);
			this.Panel.TabIndex = 3;
			// 
			// Picture
			// 
			this.Picture.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Picture.Location = new System.Drawing.Point(0, 0);
			this.Picture.Name = "Picture";
			this.Picture.Size = new System.Drawing.Size(412, 269);
			this.Picture.TabIndex = 1;
			this.Picture.TabStop = false;
			// 
			// Splash
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.ClientSize = new System.Drawing.Size(412, 269);
			this.Controls.Add(this.Panel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Splash";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Mobius - Starting...";
			this.Shown += new System.EventHandler(this.Splash_Shown);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Splash_FormClosed);
			this.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Picture)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion


		public void SetSplashImage()
		{
			string splashImagePath = Application.StartupPath + @"\Splash.png"; // load local image first
			if (File.Exists(splashImagePath))
			{
				Splash.Instance.Picture.Image = new Bitmap(splashImagePath);
				Splash.Instance.Invalidate();
				Application.DoEvents();
			}

			SplashHelper sh = new SplashHelper();
			ThreadStart ts = new ThreadStart(sh.SetSplashImage); // set image on separate thread
			Thread newThread = new Thread(ts);
			newThread.IsBackground = true;
			newThread.SetApartmentState(ApartmentState.STA);
			newThread.Start();
		}

/// <summary>
/// Resize and repaint image in a GUI thread so that the client thread doesn't block this
/// </summary>
/// <param name="image"></param>

		public void SetImage(Image image)
		{
			SplashBitmap = new Bitmap(image);
			Picture.Invoke(new ScalePictureDelegate(this.ScalePicture));
		}

		public void ScalePicture()
		{
			//would like to hold refresh until done, but suspending the layout doesn't avoid flicker
			//this.SuspendLayout();
			//panel1.SuspendLayout();

			//max the picture box
			//Picture.Location = new System.Drawing.Point(0, 0);
			if (Panel == null || Picture == null || SplashBitmap == null) return;

			Picture.Size = new System.Drawing.Size(
					Math.Max(1, Math.Min(Panel.Width, 2 * Picture.Width)),
					Math.Max(1, Math.Min(Panel.Height, 2 * Picture.Height)));

			//determine aspect(s)
			double imageAspect = Convert.ToDouble(SplashBitmap.Width) / Convert.ToDouble(SplashBitmap.Height);
			double pbAspect = Convert.ToDouble(Picture.Width) / Convert.ToDouble(Picture.Height);

			//resize to maintain aspect if too extremely distorted (allow 10% diff)
			if (200 * Math.Abs(pbAspect - imageAspect) / (pbAspect + imageAspect) > 10)
			{
				if (pbAspect > imageAspect)
				{
					//too wide
					int newWidth = Convert.ToInt32(Math.Max(1, Math.Floor(Picture.Height * imageAspect)));
					Picture.Width = newWidth;
				}
				else
				{
					//too high
					int newHeight = Convert.ToInt32(Math.Max(1, Math.Floor(Picture.Width / imageAspect)));
					Picture.Height = newHeight;
				}
			}

			//scale the image
			Bitmap bm = new Bitmap(SplashBitmap, new Size(Picture.Width, Picture.Height));
			Picture.Image = bm;

			//center the image
			Picture.Location = new Point(Math.Max(0, (Panel.Width - Picture.Image.Width) / 2), Math.Max(0, (Panel.Height - Picture.Image.Height) / 2));

			//all done
			//panel1.ResumeLayout();
			//this.ResumeLayout();
		}

        void Splash_Shown(object sender, EventArgs e)
        {
            this.Resize += new System.EventHandler(this.Splash_Resize);
        }

		private void Splash_Resize(object sender, EventArgs e)
		{
			//resize and repaint in a GUI thread so that the client thread doesn't block this
			//really, we wouldn't be handling the resize event if this thread weren't executing, so
			// this isn't strictly necessary.
			try { Picture.Invoke(new ScalePictureDelegate(this.ScalePicture)); }
			catch (Exception ex) { return; } // exception may occur if form not visible
		}


		private void Splash_FormClosed(object sender, FormClosedEventArgs e)
		{
			Environment.Exit(-1);
		}
	}

/// <summary>
/// Splash helper class
/// </summary>

	public class SplashHelper
	{

		/// <summary>
		/// Get splash image from server and display
		/// </summary>

		public void SetSplashImage()
		{

			try
			{
				string splashImageDir = Splash.Instance.IniFile.Read("SplashImageDir");
				//				ClientLog.Message("SplashImageDir: " + splashImageDir); // debug

				string tok = Splash.Instance.IniFile.Read("SplashImageCount");
				//				ClientLog.Message("SplashImageCount: " + tok); // debug
				int splashCount = 0;
				Int32.TryParse(tok, out splashCount);

				int imageIdx = 1;

				tok = Splash.Instance.UserIniFile.Read("SplashImageIndex");
				//				ClientLog.Message("SplashImageIndex: " + tok); // debug

				if (tok != "")
				{
					imageIdx = Int32.Parse(tok) + 1;
					if (imageIdx > splashCount) imageIdx = 1;
				}
				Splash.Instance.UserIniFile.Write("SplashImageIndex", imageIdx.ToString());

				//				int ms = System.DateTime.Now.Millisecond;
				//				int imageIdx = ms % splashCount + 1; // index of image to use 

				string splashImagePath = // name of splash file on server
					splashImageDir + @"\Nature" + imageIdx.ToString() + ".jpg";

				int t0 = TimeOfDay.Milliseconds();
				if (File.Exists(splashImagePath))
				{
					Splash.Instance.SetImage(new Bitmap(splashImagePath));
					Splash.Instance.Invalidate();
					Application.DoEvents();
				}
				t0 = TimeOfDay.Milliseconds() - t0;
			}
			catch (Exception ex)
			{
				ClientLog.Message("Splash display error: " + ex.Message);
			}

		}
	}
}
