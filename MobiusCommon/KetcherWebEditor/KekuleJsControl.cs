using Mobius.ComOps;
using Mobius.Data;

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

namespace Mobius.KekuleJs
{
	public partial class KekuleJsControl : DevExpress.XtraEditors.XtraUserControl, INativeMolControl
	{
		[DefaultValue(KekuleJsControlMode.BrowserViewOnly)]
		public KekuleJsControlMode KekuleJsMode { get => _KekuleJsMode; set => _KekuleJsMode = value; }
		KekuleJsControlMode _KekuleJsMode = KekuleJsControlMode.BrowserViewOnly;

		public bool BitmapMode => (_KekuleJsMode == KekuleJsControlMode.OffScreenBitmap); // rendered off-screen and returned as a bitmap that can then be used anywhere
		public bool SvgMode => (_KekuleJsMode == KekuleJsControlMode.OffScreenSvg); // rendered off-screen and returned as SVG that can be used anywhere 
		public bool ViewMode => (_KekuleJsMode == KekuleJsControlMode.BrowserViewOnly); // view KekuleJs in browser without edit controls
		public bool EditMode => (KekuleJsMode == KekuleJsControlMode.BrowserEditor); // show KekuleJs in editor

		public KekuleJsWinFormsBrowser WinFormsBrowserMx = null; // underlying Mobius browser wrapper for view-only and editor modes

		public WindowsMessageFilterCallback WindowsMessageFilterCallback;
		public WindowsMessageFilter WindowsMessageFilter;

		public KekuleJsOffScreenBrowser OffScreenBrowserMx = null; // used to generate OffScreen image for use as bitmap or SVG
			 
		public PictureBox OnScreenImageCtl = null; // used to display OffScreen renderer bitmap

		public string KekuleJs = null; // most-recently rendered KekuleJs
		public Size LastRenderSize = new Size(); // size of most-recently rendered bitmap
		public int RenderCount = 0;
		public int OnPaintCount = 0; // number of times OnPaint called

		public int Id = IdCounter++;
		string IdText => " (HcId: " + Id + ")";
		static int IdCounter = 0;

		ManualResetEvent BrowserInitializedEvent = new ManualResetEvent(false);
		ManualResetEvent PageLoadedEvent = new ManualResetEvent(false);

		public event EventHandler MoleculeChanged; // caller-set event to fire when edit value changes

		public static KekuleJsOffScreenBrowser StaticOffScreenBrowser = null;

		public static bool Debug => JavaScriptManager.Debug;

		/// <summary>
		/// Constructor
		/// </summary>

		public KekuleJsControl()
		{
			InitializeComponent();

			if (Debug) DebugLog.Message("KekuleJsControl instance created" + IdText); // + "\r\n" + (new StackTrace(true)).ToString());
			return;
		}

		/// <summary>
		/// Initialize the on-screen and/or off-screen rendering controls based on the selected KekuleJsControlMode
		/// </summary>

		public void InitializeControl()
		{
			if (KekuleJsMode == KekuleJsControlMode.OffScreenBitmap)
			{
				if (OffScreenBrowserMx != null) return;

				OffScreenBrowserMx = new KekuleJsOffScreenBrowser(); // used for generating the bitmap image of the KekuleJs
				OffScreenBrowserMx.LoadInitialPage();

				SetupOnScreenBitmapImageControl();
			}

			else if (KekuleJsMode == KekuleJsControlMode.OffScreenSvg)
			{
				if (OffScreenBrowserMx != null) return;

				OffScreenBrowserMx = new KekuleJsOffScreenBrowser(); // used for generating the SVG of the KekuleJs
				OffScreenBrowserMx.LoadInitialPage();

				WinFormsBrowserMx = new KekuleJsWinFormsBrowser(KekuleJsMode, this); // used for displaying the SVG for this mode
				WinFormsBrowserMx.Browser.Dock = DockStyle.Fill; // will fill containing control

				Controls.Clear();
				Controls.Add(WinFormsBrowserMx.Browser);
			}

			else // regular render or edit
			{
				if (WinFormsBrowserMx != null) return;

				WinFormsBrowserMx = new KekuleJsWinFormsBrowser(KekuleJsMode, this); 
				WinFormsBrowserMx.LoadInitialPage();

				if (WindowsMessageFilterCallback != null)
					WindowsMessageFilter = WindowsMessageFilter.CreateRightClickMessageFilter(WinFormsBrowserMx.Browser, WindowsMessageFilterCallback);
			}

			if (Debug) DebugLog.Message("KekuleJsControl initialized for Mode: " + KekuleJsMode + IdText);
			return;
		}

		/// <summary>
		/// SetupOnScreenImageControl for displaying bitmap image of KekuleJs
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

			OnScreenImageCtl.Click += new EventHandler(KekuleJsControl_Click); // allow edit of controls contents
			OnScreenImageCtl.DoubleClick += new EventHandler(KekuleJsControl_DoubleClick);

			return;
		}


		/// <summary>
		/// Set KekuleJs string and render the depiction of the KekuleJs
		/// </summary>
		/// <param name="KekuleJs"></param>

		public void SetKekuleJsAndRender(
			string KekuleJs)
		{
			Size size = new Size(this.Size.Width - 20, this.Size.Height); // reduce width a bit to avoid truncating on the right
			SetKekuleJsAndRender(KekuleJs, size);
			return;
		}

		/// <summary>
		/// Set KekuleJs string and render the depiction of the KekuleJs (asynch)
		/// </summary>
		/// <param name="KekuleJs"></param>
		/// <param name="size"></param>

		public void SetKekuleJsAndRenderAsynch(
			string KekuleJs,
			Size size)
		{
			object[] parms = new object[] { KekuleJs, size };

			DelayedCallback.Schedule(SetKekuleJsAndRenderAsynchCallback, parms);
		}

		public void SetKekuleJsAndRenderAsynchCallback(
			object parms)
		{
			string KekuleJs = (string)((object[])parms)[0];
			Size size = (Size)((object[])parms)[1];

			SetKekuleJsAndRender(KekuleJs, size);
			return;
		}
		
		/// <summary>
		/// Set KekuleJs string and render to full size of control
		/// </summary>
		/// <param name="KekuleJs"></param>

		public void SetKekuleJsAndRender(
			string KekuleJs,
			Size size)
		{
			InitializeControl(); // be sure initial page is loaded into browser

			//if (DebugMx.True) return; // debug - disable

			if (Debug) DebugLog.Message("SetKekuleJsAndRender Entered: " + KekuleJs);

			Stopwatch sw = Stopwatch.StartNew();

			KekuleJs = KekuleJs;
			LastRenderSize = size;

			//double scaledCanvasWidth = size.Width * CefMx.MonitorScalingFactor; // scale for high DPI monitor?
			//double scaledCanvasHeight = size.Height * CefMx.MonitorScalingFactor;

			if (BitmapMode)
			{
				if (OnScreenImageCtl == null) throw new NullReferenceException("OnScreenImageCtl == null");

				Bitmap bitmap = OffScreenBrowserMx.GetBitmap(KekuleJs, this.Size);
				if (OnScreenImageCtl?.Image != null) 
					BitmapUtil.DisposeOfBitmap(OnScreenImageCtl.Image);  
				OnScreenImageCtl.Image = bitmap;
				OnScreenImageCtl.Refresh();
			} 

			else if (SvgMode)
			{
				string svg = OffScreenBrowserMx.GetSvg(KekuleJs); // get from off-screen control
				WinFormsBrowserMx.LoadSvgString(svg); // display svg in browser control
			}

			else // RenderMode || EditMode
			{
				WinFormsBrowserMx.SetKekuleJsAndRender(KekuleJs, size);
			}

			RenderCount++;

			if (Debug) DebugLog.StopwatchMessage("SetKekuleJsAndRender Complete, Time: ", sw);

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
		/// Get current KekuleJs
		/// </summary>
		/// <returns></returns>

		public string GetKekuleJs()
		{
			if (Debug) DebugLog.Message("GetKekuleJs Entered");
			 
			Stopwatch sw = Stopwatch.StartNew();

			string script = "";

			if (ViewMode)
				script = "function getKekuleJsString() { return jsd.getKekuleJs(); };"; // define function to get the KekuleJs

			else
				script = "function getKekuleJsString() { return app.canvas.getKekuleJs(); };"; // define function to get the KekuleJs

			JavaScriptManager.ExecuteScript(WinFormsBrowserMx, script);

			string KekuleJs = JavaScriptManager.CallFunction(WinFormsBrowserMx, "getKekuleJsString"); // call function to get the KekuleJs

			if (Debug) DebugLog.StopwatchMessage("GetKekuleJs Complete: " + KekuleJs + ", Time: ", sw);

			return KekuleJs;
		}

		/// <summary>
		/// Get bitmap of current KekuleJs in current control size
		/// </summary>
		/// <returns></returns>

		public Bitmap GetBitmap()
		{
			Bitmap bm = null;

			AssertMx.IsNotNull(OffScreenBrowserMx, "OffScreenBrowserMx");

			bm = OffScreenBrowserMx.GetBitmap(this.KekuleJs, this.Size);

			return bm;
		}

/// <summary>
/// Static method to get bitmap trimed to proper height
/// </summary>
/// <param name="KekuleJs"></param>
/// <param name="pixWidth"></param>
/// <returns></returns>

		public static Bitmap GetBitmap( 
			string KekuleJs,
			int pixWidth)
		{
			if (StaticOffScreenBrowser == null)
				StaticOffScreenBrowser = new KekuleJsOffScreenBrowser();

			lock (StaticOffScreenBrowser) // lock to avoid reentry
			{
				string svg = StaticOffScreenBrowser.GetSvg(KekuleJs);
				Bitmap bm = SvgUtil.GetBitmapFromSvgXml(svg, pixWidth);
				return bm;

				//float minViewBoxWidth = 40 * 6; // avoid scaleup for less than 6 KekuleJs monomers (todo: implement this)
			}
		}

		/// <summary>
		/// Static method to get GZip compressed svg depiction of KekuleJs
		/// </summary>
		/// <param name="KekuleJs"></param>
		/// <returns></returns>
		public static string GetCompressedSvg(
			string KekuleJs)
		{
			string svg = GetSvg(KekuleJs);
			byte[] svgGZ = GZip.Compress(svg);
			return svg;
		}

		/// <summary>
		/// Static method to get SVG depiction of KekuleJs
		/// </summary>
		/// <param name="KekuleJs"></param>
		/// <param name="pixWidth"></param>
		/// <returns></returns>
		/// 

		public static string GetSvg(
			string KekuleJs)
		{
			if (StaticOffScreenBrowser == null)
				StaticOffScreenBrowser = new KekuleJsOffScreenBrowser();

			lock (StaticOffScreenBrowser) // lock to avoid reentry
			{
				string svg = StaticOffScreenBrowser.GetSvg(KekuleJs);
				return svg;
			}
		}

/// <summary>
/// Get SVG depiction of current KekuleJs string
/// </summary>
/// <returns></returns>

		public string GetSvg()
		{
			if (OffScreenBrowserMx == null)
				throw new ArgumentNullException("OffScreenBrowserMx is null");

			if (KekuleJsMode != KekuleJsControlMode.OffScreenSvg)
				throw new Exception("Expected KekuleJsMode == KekuleJsControlMode.OffScreenSvgKekuleJsControlMode but KekuleJsMode is " + KekuleJsMode);

			string svg = OffScreenBrowserMx.GetSvg(this.KekuleJs, this.Size);
			return svg;
		}

#if false
		/// <summary>
		/// Get SVG of the specified KekuleJs 
		/// </summary>
		/// <param name="KekuleJs"></param>
		/// <param name="size"></param>
		/// <returns></returns>

		public string GetSvg(
			string KekuleJs,
			Size size)
		{
			if (KekuleJsMode != KekuleJsControlMode.OffScreenSvg)
				throw new Exception("Expected KekuleJsMode == KekuleJsControlMode.OffScreenSvgKekuleJsControlMode but KekuleJsMode is " + KekuleJsMode);

			InitializeControl(); // be sure initial page is loaded into browser

			KekuleJs = KekuleJs;

			string svg = OffScreenBrowserMx.GetSvg(KekuleJs, size);
			return svg;
		}
#endif

		/// <summary>
		/// Contained control clicked (OffScreen only)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void KekuleJsControl_Click(object sender, EventArgs e)
		{
			DelayedCallback.Schedule(EditMolecule);
			return;
		}

		/// <summary>
		/// Open the KekuleJsEditor for the current KekuleJs string
		/// </summary>

		public void EditMolecule()
		{
			KekuleJsEditorDialog editor = new KekuleJsEditorDialog();

			string newKekuleJs = editor.Edit(KekuleJs, ParentForm);
			if (newKekuleJs != null && !String.Equals(newKekuleJs, KekuleJs))
			{
				SetKekuleJsAndRender(newKekuleJs); // display new KekuleJs

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

		private void KekuleJsControl_DoubleClick(object sender, EventArgs e)
		{
			return;
		}

		/// <summary>
		/// Size changed, adjust underlying image or browser control accordingly
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void KekuleJsControl_SizeChanged(object sender, EventArgs e)
		{
			if (WinFormsBrowserMx == null && OffScreenBrowserMx == null) return; // just return if nothing in KekuleJs control yet

			if (Debug) DebugLog.Message("KekuleJsControl_SizeChanged: " + this.Size.Width + ", " + this.Size.Height);

			//ResizeRendering(); // redraw the KekuleJs at the proper size (being done at browser level now)

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
	/// KekuleJs renderer/editor mode
	/// </summary>

	public enum KekuleJsControlMode
	{
		OffScreenBitmap = 0, // rendered off-screen and returned as a bitmap that can then be used anywhere
		OffScreenSvg = 1, // rendered off-screen and returned as SVG that can be used anywhere 
		BrowserViewOnly = 2, // rendered on-screen by browser but not editable
		BrowserEditor = 3 // KekuleJs structure with editor controls
	}
}
