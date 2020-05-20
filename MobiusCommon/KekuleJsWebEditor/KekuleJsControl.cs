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
		public KekuleJsControlMode KekuleJsMode { get => _moleculeStringMode; set => _moleculeStringMode = value; }
		KekuleJsControlMode _moleculeStringMode = KekuleJsControlMode.BrowserViewOnly;

		public bool BitmapMode => (_moleculeStringMode == KekuleJsControlMode.OffScreenBitmap); // rendered off-screen and returned as a bitmap that can then be used anywhere
		public bool SvgMode => (_moleculeStringMode == KekuleJsControlMode.OffScreenSvg); // rendered off-screen and returned as SVG that can be used anywhere 
		public bool ViewMode => (_moleculeStringMode == KekuleJsControlMode.BrowserViewOnly); // view moleculeString in browser without edit controls
		public bool EditMode => (KekuleJsMode == KekuleJsControlMode.BrowserEditor); // show moleculeString in editor

		public KekuleJsWinFormsBrowser WinFormsBrowserMx = null; // underlying Mobius browser wrapper for view-only and editor modes

		public WindowsMessageFilterCallback WindowsMessageFilterCallback;
		public WindowsMessageFilter WindowsMessageFilter;

		public KekuleJsOffScreenBrowser OffScreenBrowserMx = null; // used to generate OffScreen image for use as bitmap or SVG

		public PictureBox OnScreenImageCtl = null; // used to display OffScreen renderer bitmap

		public string MolfileString { get => Molfile; set => Molfile = value; } // most-recently rendered molfile
		string Molfile = ""; 

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

		public DisplayPreferences Preferences = new DisplayPreferences();

		public event EventHandler StructureChanged;

		public MolEditorReturnedHandler EditorReturnedHandler
		{
			get => _editorReturnedHandler;
			set => value = _editorReturnedHandler;
		}
		MolEditorReturnedHandler _editorReturnedHandler;


		/// <summary>
		/// Get current molecule value from control
		/// </summary>
		/// <param name="molFormat"></param>
		/// <param name="molString"></param>

		public void GetMolecule(
			out MoleculeFormat molFormat,
			out string molString)
		{
			if (Lex.IsDefined(Molfile))
			{
				molFormat = MoleculeFormat.Molfile;
				molString = Molfile;
			}

			else
			{
				molFormat = MoleculeFormat.Unknown;
				molString = null;
			}

			return;
		}

		/// <summary>
		/// Set moleculeString string and render the depiction of the moleculeString
		/// </summary>
		/// <param name="molFormat"></param>
		/// <param name="molString"></param>

		public void SetMoleculeAndRender(
			MoleculeFormat molFormat,
			string molString)
		{
			Size size = new Size(this.Size.Width - 20, this.Size.Height); // reduce width a bit to avoid truncating on the right
			SetMoleculeAndRender(molFormat, molString, size);
			return;
		}

		public void SetTag(object tag)
		{
			this.Tag = tag;
			return;
		}

		public object GetTag()
		{
			return this.Tag;
		}

		/// <summary>
		/// Set moleculeString string and render the depiction of the moleculeString (asynch)
		/// </summary>
		/// <param name="moleculeString"></param>
		/// <param name="size"></param>

		public void SetMoleculeAndRenderAsynch(
			string moleculeString,
			Size size)
		{
			object[] parms = new object[] { moleculeString, size };

			DelayedCallback.Schedule(SetMoleculeAndRenderAsynchCallback, parms);
		}

		public void SetMoleculeAndRenderAsynchCallback(
			object parmsObj)
		{
			AssertMx.IsNotNull((parmsObj as object[]), "parmsObj as object[]");

			object[] parms = (object[])parmsObj;
			AssertMx.IsTrue(parms.Length == 3, "parms.Length == 3");

			MoleculeFormat molFormat = (MoleculeFormat)parms[0];
			string molString = (string)parms[1];
			Size size = (Size)parms[2];

			SetMoleculeAndRender(molFormat, molString, size);
			return;
		}

		/// <summary>
		/// Set moleculeString string and render to full size of control
		/// </summary>
		/// <param name="molString"></param>

		public void SetMoleculeAndRender(
			MoleculeFormat molFormat,
			string molString,
			Size size)
		{
			InitializeControl(); // be sure initial page is loaded into browser

			//if (DebugMx.True) return; // debug - disable

			if (Debug) DebugLog.Message("SetMoleculeAndRender Entered: " + molString);

			MoleculeMx molMx = new MoleculeMx(molFormat, molString);

			Molfile =  molMx.GetMolfileString();

			LastRenderSize = size;

			RenderMolecule(size);

			return;
		}

		public void RenderMolecule(
			Size size)
		{
			Stopwatch sw = Stopwatch.StartNew();

			if (BitmapMode)
			{
				if (OnScreenImageCtl == null) throw new NullReferenceException("OnScreenImageCtl == null");

				Bitmap bitmap = OffScreenBrowserMx.GetBitmap(Molfile, this.Size);
				if (OnScreenImageCtl?.Image != null)
					BitmapUtil.DisposeOfBitmap(OnScreenImageCtl.Image);
				OnScreenImageCtl.Image = bitmap;
				OnScreenImageCtl.Refresh();
			}

			else if (SvgMode)
			{
				string svg = OffScreenBrowserMx.GetSvg(Molfile); // get from off-screen control
				WinFormsBrowserMx.LoadSvgString(svg); // display svg in browser control
			}

			else // RenderMode || EditMode
			{
				WinFormsBrowserMx.SetMoleculeAndRender(Molfile, size);
			}

			RenderCount++;

			if (Debug) DebugLog.StopwatchMessage("SetMoleculeAndRender Complete, Time: ", sw);

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
		/// Get current moleculeString
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

			string moleculeString = JavaScriptManager.CallFunction(WinFormsBrowserMx, "getKekuleJsString"); // call function to get the moleculeString

			if (Debug) DebugLog.StopwatchMessage("GetKekuleJs Complete: " + moleculeString + ", Time: ", sw);

			return moleculeString;
		}

		/// <summary>
		/// Get bitmap of current moleculeString in current control size
		/// </summary>
		/// <returns></returns>

		public Bitmap GetBitmap()
		{
			Bitmap bm = null;

			AssertMx.IsNotNull(OffScreenBrowserMx, "OffScreenBrowserMx");

			bm = OffScreenBrowserMx.GetBitmap(this.Molfile, this.Size);

			return bm;
		}

		/// <summary>
		/// Static method to get bitmap trimed to proper height
		/// </summary>
		/// <param name="moleculeString"></param>
		/// <param name="pixWidth"></param>
		/// <returns></returns>

		public static Bitmap GetBitmap(
			string moleculeString,
			int pixWidth)
		{
			if (StaticOffScreenBrowser == null)
				StaticOffScreenBrowser = new KekuleJsOffScreenBrowser();

			lock (StaticOffScreenBrowser) // lock to avoid reentry
			{
				string svg = StaticOffScreenBrowser.GetSvg(moleculeString);
				Bitmap bm = SvgUtil.GetBitmapFromSvgXml(svg, pixWidth);
				return bm;

				//float minViewBoxWidth = 40 * 6; // avoid scaleup for less than 6 moleculeString monomers (todo: implement this)
			}
		}

		/// <summary>
		/// Static method to get GZip compressed svg depiction of KekuleJs
		/// </summary>
		/// <param name="moleculeString"></param>
		/// <returns></returns>
		public static string GetCompressedSvg(
			string moleculeString)
		{
			string svg = GetSvg(moleculeString);
			byte[] svgGZ = GZip.Compress(svg);
			return svg;
		}

		/// <summary>
		/// Static method to get SVG depiction of KekuleJs
		/// </summary>
		/// <param name="moleculeString"></param>
		/// <param name="pixWidth"></param>
		/// <returns></returns>
		/// 

		public static string GetSvg(
			string moleculeString)
		{
			if (StaticOffScreenBrowser == null)
				StaticOffScreenBrowser = new KekuleJsOffScreenBrowser();

			lock (StaticOffScreenBrowser) // lock to avoid reentry
			{
				string svg = StaticOffScreenBrowser.GetSvg(moleculeString);
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

			string svg = OffScreenBrowserMx.GetSvg(this.Molfile, this.Size);
			return svg;
		}

#if false
		/// <summary>
		/// Get SVG of the specified moleculeString 
		/// </summary>
		/// <param name="moleculeString"></param>
		/// <param name="size"></param>
		/// <returns></returns>

		public string GetSvg(
			string moleculeString,
			Size size)
		{
			if (KekuleJsMode != KekuleJsControlMode.OffScreenSvg)
				throw new Exception("Expected KekuleJsMode == KekuleJsControlMode.OffScreenSvgKekuleJsControlMode but KekuleJsMode is " + KekuleJsMode);

			InitializeControl(); // be sure initial page is loaded into browser

			KekuleJs = moleculeString;

			string svg = OffScreenBrowserMx.GetSvg(moleculeString, size);
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

			string newMolfile = editor.Edit(Molfile, ParentForm);
			if (newMolfile != null && !String.Equals(newMolfile, Molfile))
			{
				SetMoleculeAndRender(MoleculeFormat.Molfile, newMolfile); // display new moleculeString

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
			if (WinFormsBrowserMx == null && OffScreenBrowserMx == null) return; // just return if nothing in moleculeString control yet

			if (Debug) DebugLog.Message("KekuleJsControl_SizeChanged: " + this.Size.Width + ", " + this.Size.Height);

			//ResizeRendering(); // redraw the moleculeString at the proper size (being done at browser level now)

			return;
		}

		public bool CanPaste => throw new NotImplementedException();

		public bool CanCopy => throw new NotImplementedException();

		public void PasteFromClipboard()
		{
			throw new NotImplementedException();
		}

		public void CopyToClipboard()
		{
			throw new NotImplementedException();
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
