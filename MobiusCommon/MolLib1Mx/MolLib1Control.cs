using Mobius.ComOps;
using Mobius.Helm;

using DevExpress.XtraEditors;
using DevExpress.Utils;
using DevExpress.Data;

using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;

namespace Mobius.MolLib1
{
	public partial class MolLib1Control : DevExpress.XtraEditors.XtraUserControl
	{
		[DefaultValue(MolLib1ControlMode.BrowserViewOnly)]
		public MolLib1ControlMode HelmMode { get => _helmMode; set => _helmMode = value; }
		MolLib1ControlMode _helmMode = MolLib1ControlMode.BrowserViewOnly;

		public bool BitmapMode => (_helmMode == MolLib1ControlMode.OffScreenBitmap); // rendered off-screen and returned as a bitmap that can then be used anywhere
		public bool SvgMode => (_helmMode == MolLib1ControlMode.OffScreenSvg); // rendered off-screen and returned as SVG that can be used anywhere 
		public bool ViewMode => (_helmMode == MolLib1ControlMode.BrowserViewOnly); // view helm in browser without edit controls
		public bool EditMode => (HelmMode == MolLib1ControlMode.BrowserEditor); // show helm in editor

		public HelmWinFormsBrowser WinFormsBrowserMx = null; // underlying Mobius browser wrapper for view-only and editor modes

		public WindowsMessageFilterCallback WindowsMessageFilterCallback;
		public WindowsMessageFilter WindowsMessageFilter;

		public HelmOffScreenBrowser OffScreenBrowserMx = null; // used to generate OffScreen image for use as bitmap or SVG
			 
		public PictureBox OnScreenImageCtl = null; // used to display OffScreen renderer bitmap

		public string Helm = null; // most-recently rendered helm
		public Size LastRenderSize = new Size(); // size of most-recently rendered bitmap
		public int RenderCount = 0;
		public int OnPaintCount = 0; // number of times OnPaint called

		public int Id = IdCounter++;
		string IdText => " (HcId: " + Id + ")";
		static int IdCounter = 0;

		ManualResetEvent BrowserInitializedEvent = new ManualResetEvent(false);
		ManualResetEvent PageLoadedEvent = new ManualResetEvent(false);

		public event EventHandler MoleculeChanged; // caller-set event to fire when edit value changes

		public static HelmOffScreenBrowser StaticOffScreenBrowser = null;

		public static bool Debug => JavaScriptManager.Debug;

		/// <summary>
		/// Constructor
		/// </summary>

		public MolLib1Control()
		{
			InitializeComponent();

			if (Debug) DebugLog.Message("MolLib1Control instance created" + IdText); // + "\r\n" + (new StackTrace(true)).ToString());
			return;
		}

		/// <summary>
		/// Initialize the on-screen and/or off-screen rendering controls based on the selected MolLib1ControlMode
		/// </summary>

		public void InitializeControl()
		{
#if false
			if (HelmMode == MolLib1ControlMode.OffScreenBitmap)
			{
				if (OffScreenBrowserMx != null) return;

				OffScreenBrowserMx = new HelmOffScreenBrowser(); // used for generating the bitmap image of the Helm
				OffScreenBrowserMx.LoadInitialPage();

				SetupOnScreenBitmapImageControl();
			}

			else if (HelmMode == MolLib1ControlMode.OffScreenSvg)
			{
				if (OffScreenBrowserMx != null) return;

				OffScreenBrowserMx = new HelmOffScreenBrowser(); // used for generating the SVG of the Helm
				OffScreenBrowserMx.LoadInitialPage();

				WinFormsBrowserMx = new HelmWinFormsBrowser(HelmMode, this); // used for displaying the SVG for this mode
				WinFormsBrowserMx.Browser.Dock = DockStyle.Fill; // will fill containing control

				Controls.Clear();
				Controls.Add(WinFormsBrowserMx.Browser);
			}

			else // regular render or edit
			{
				if (WinFormsBrowserMx != null) return;

				WinFormsBrowserMx = new HelmWinFormsBrowser(HelmMode, this); 
				WinFormsBrowserMx.LoadInitialPage();

				if (WindowsMessageFilterCallback != null)
					WindowsMessageFilter = WindowsMessageFilter.CreateRightClickMessageFilter(WinFormsBrowserMx.Browser, WindowsMessageFilterCallback);
			}

#endif
			if (Debug) DebugLog.Message("MolLib1Control initialized for Mode: " + HelmMode + IdText);
			return;
		}

		/// <summary>
		/// SetupOnScreenImageControl for displaying bitmap image of Helm
		/// </summary>

		void SetupOnScreenBitmapImageControl()
		{
			if (this.Controls.Count == 1 && this.Controls[0] is PictureBox)
			{
				OnScreenImageCtl = Controls[0] as PictureBox;
			}

			else
			{
				OnScreenImageCtl = new PictureBox();
				OnScreenImageCtl.Image = new Bitmap(this.Size.Width, this.Size.Height);

				Controls.Clear();
				Controls.Add(OnScreenImageCtl);
			}

			OnScreenImageCtl.Dock = DockStyle.Fill;

			OnScreenImageCtl.Click += new EventHandler(MolLib1Control_Click); // allow edit of controls contents
			OnScreenImageCtl.DoubleClick += new EventHandler(MolLib1Control_DoubleClick);

			return;
		}


		/// <summary>
		/// Set HELM string and render the depiction of the HELM
		/// </summary>
		/// <param name="helm"></param>

		public void SetHelmAndRender(
			string helm)
		{
			Size size = new Size(this.Size.Width - 20, this.Size.Height); // reduce width a bit to avoid truncating on the right
			SetHelmAndRender(helm, size);
			return;
		}

		/// <summary>
		/// Set HELM string and render the depiction of the HELM (asynch)
		/// </summary>
		/// <param name="helm"></param>
		/// <param name="size"></param>

		public void SetHelmAndRenderAsynch(
			string helm,
			Size size)
		{
			object[] parms = new object[] { helm, size };

			DelayedCallback.Schedule(SetHelmAndRenderAsynchCallback, parms);
		}

		public void SetHelmAndRenderAsynchCallback(
			object parms)
		{
			string helm = (string)((object[])parms)[0];
			Size size = (Size)((object[])parms)[1];

			SetHelmAndRender(helm, size);
			return;
		}
		
		/// <summary>
		/// Set HELM string and render to full size of control
		/// </summary>
		/// <param name="helm"></param>

		public void SetHelmAndRender(
			string helm,
			Size size)
		{
			InitializeControl(); // be sure initial page is loaded into browser

			//if (DebugMx.True) return; // debug - disable

			if (Debug) DebugLog.Message("SetHelmAndRender Entered: " + helm);

			Stopwatch sw = Stopwatch.StartNew();

			Helm = helm;
			LastRenderSize = size;

			//double scaledCanvasWidth = size.Width * CefMx.MonitorScalingFactor; // scale for high DPI monitor?
			//double scaledCanvasHeight = size.Height * CefMx.MonitorScalingFactor;

			if (BitmapMode)
			{
				if (OnScreenImageCtl == null) throw new NullReferenceException("OnScreenImageCtl == null");

				Bitmap bitmap = OffScreenBrowserMx.GetBitmap(Helm, this.Size);
				if (OnScreenImageCtl?.Image != null) 
					BitmapUtil.DisposeOfBitmap(OnScreenImageCtl.Image);  
				OnScreenImageCtl.Image = bitmap;
				OnScreenImageCtl.Refresh();
			} 

			else if (SvgMode)
			{
				string svg = OffScreenBrowserMx.GetSvg(Helm); // get from off-screen control
				WinFormsBrowserMx.LoadSvgString(svg); // display svg in browser control
			}

			else // RenderMode || EditMode
			{
				WinFormsBrowserMx.SetHelmAndRender(helm, size);
			}

			RenderCount++;

			if (Debug) DebugLog.StopwatchMessage("SetHelmAndRender Complete, Time: ", sw);

			return;
		}

		/// <summary>
		/// ResizeRendering
		/// </summary>

		public void ResizeRendering()
		{
			WinFormsBrowserMx.ResizeRendering();
		}

		/// <summary>
		/// Get current HELM
		/// </summary>
		/// <returns></returns>

		public string GetHelm()
		{
			if (Debug) DebugLog.Message("GetHelm Entered");
			 
			Stopwatch sw = Stopwatch.StartNew();

			string script = "";

			if (ViewMode)
				script = "function getHelmString() { return jsd.getHelm(); };"; // define function to get the Helm

			else
				script = "function getHelmString() { return app.canvas.getHelm(); };"; // define function to get the Helm

			JavaScriptManager.ExecuteScript(WinFormsBrowserMx, script);

			string helm = JavaScriptManager.CallFunction(WinFormsBrowserMx, "getHelmString"); // call function to get the helm

			if (Debug) DebugLog.StopwatchMessage("GetHelm Complete: " + helm + ", Time: ", sw);

			return helm;
		}

		/// <summary>
		/// Get bitmap of current helm in current control size
		/// </summary>
		/// <returns></returns>

		public Bitmap GetBitmap()
		{
			Bitmap bm = null;

			AssertMx.IsNotNull(OffScreenBrowserMx, "OffScreenBrowserMx");

			bm = OffScreenBrowserMx.GetBitmap(this.Helm, this.Size);

			return bm;
		}

/// <summary>
/// Static method to get bitmap trimed to proper height
/// </summary>
/// <param name="helm"></param>
/// <param name="pixWidth"></param>
/// <returns></returns>

		public static Bitmap GetBitmap( 
			string helm,
			int pixWidth)
		{
			if (StaticOffScreenBrowser == null)
				StaticOffScreenBrowser = new HelmOffScreenBrowser();

			lock (StaticOffScreenBrowser) // lock to avoid reentry
			{
				string svg = StaticOffScreenBrowser.GetSvg(helm);
				Bitmap bm = SvgUtil.GetBitmapFromSvgXml(svg, pixWidth);
				return bm;

				//float minViewBoxWidth = 40 * 6; // avoid scaleup for less than 6 HELM monomers (todo: implement this)
			}
		}

		/// <summary>
		/// Static method to get GZip compressed svg depiction of Helm
		/// </summary>
		/// <param name="helm"></param>
		/// <returns></returns>
		public static string GetCompressedSvg(
			string helm)
		{
			string svg = GetSvg(helm);
			byte[] svgGZ = GZip.Compress(svg);
			return svg;
		}

		/// <summary>
		/// Static method to get SVG depiction of Helm
		/// </summary>
		/// <param name="helm"></param>
		/// <param name="pixWidth"></param>
		/// <returns></returns>
		/// 

		public static string GetSvg(
			string helm)
		{
			if (StaticOffScreenBrowser == null)
				StaticOffScreenBrowser = new HelmOffScreenBrowser();

			lock (StaticOffScreenBrowser) // lock to avoid reentry
			{
				string svg = StaticOffScreenBrowser.GetSvg(helm);
				return svg;
			}
		}

/// <summary>
/// Get SVG depiction of current Helm string
/// </summary>
/// <returns></returns>

		public string GetSvg()
		{
			if (OffScreenBrowserMx == null)
				throw new ArgumentNullException("OffScreenBrowserMx is null");

			if (HelmMode != MolLib1ControlMode.OffScreenSvg)
				throw new Exception("Expected HelmMode == MolLib1ControlMode.OffScreenSvgMolLib1ControlMode but HelmMode is " + HelmMode);

			string svg = OffScreenBrowserMx.GetSvg(this.Helm, this.Size);
			return svg;
		}

#if false
		/// <summary>
		/// Get SVG of the specified HELM 
		/// </summary>
		/// <param name="helm"></param>
		/// <param name="size"></param>
		/// <returns></returns>

		public string GetSvg(
			string helm,
			Size size)
		{
			if (HelmMode != MolLib1ControlMode.OffScreenSvg)
				throw new Exception("Expected HelmMode == MolLib1ControlMode.OffScreenSvgMolLib1ControlMode but HelmMode is " + HelmMode);

			InitializeControl(); // be sure initial page is loaded into browser

			Helm = helm;

			string svg = OffScreenBrowserMx.GetSvg(helm, size);
			return svg;
		}
#endif

		/// <summary>
		/// Contained control clicked (OffScreen only)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void MolLib1Control_Click(object sender, EventArgs e)
		{
			DelayedCallback.Schedule(EditMolecule);
			return;
		}

		/// <summary>
		/// Open the HelmEditor for the current Helm string
		/// </summary>

		public void EditMolecule()
		{
			HelmEditorDialog editor = new HelmEditorDialog();

			string newHelm = editor.Edit(Helm, ParentForm);
			if (newHelm != null && !String.Equals(newHelm, Helm))
			{
				SetHelmAndRender(newHelm); // display new helm

				if (MoleculeChanged != null) // tell creater if callback defined
					MoleculeChanged(this, null);

				return;
			}
		}

		/// <summary>
		/// Contained control clicked (OffScreen only)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void MolLib1Control_DoubleClick(object sender, EventArgs e)
		{
			return;
		}

		/// <summary>
		/// Size changed, adjust underlying image or browser control accordingly
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void MolLib1Control_SizeChanged(object sender, EventArgs e)
		{
			if (WinFormsBrowserMx == null && OffScreenBrowserMx == null) return; // just return if nothing in HELM control yet

			if (Debug) DebugLog.Message("MolLib1Control_SizeChanged: " + this.Size.Width + ", " + this.Size.Height);

			//ResizeRendering(); // redraw the helm at the proper size (being done at browser level now)

			return;
		}
	}

	class JsHandler
	{
		public void HandleJsCall(int arg)
		{
			MessageBox.Show($"Value Provided From JavaScript: {arg.ToString()}", "C# Method Called");
		}
	}

	/// <summary>
	/// Helm renderer/editor mode
	/// </summary>

	public enum MolLib1ControlMode
	{
		OffScreenBitmap = 0, // rendered off-screen and returned as a bitmap that can then be used anywhere
		OffScreenSvg = 1, // rendered off-screen and returned as SVG that can be used anywhere 
		BrowserViewOnly = 2, // rendered on-screen by browser but not editable
		BrowserEditor = 3 // Helm structure with editor controls
	}
}
