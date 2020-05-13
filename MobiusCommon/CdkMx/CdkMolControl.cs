using Mobius.ComOps;
using Mobius.Data;
using Mobius.Helm;

using NCDK;
using NCDK.Depict;

using DevExpress.XtraEditors;
using DevExpress.Utils;
using DevExpress.Data;

using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using DevExpress.Utils.Svg;

namespace Mobius.CdkMx
{
	public partial class CdkMolControl : DevExpress.XtraEditors.XtraUserControl, ICdkMolControl
	{
		CdkMol CdkMol = null;

		public string MolString = "";
		public MoleculeFormat MolFormat = MoleculeFormat.Unknown;

/// <summary>
/// SetMoleculeAndRender
/// </summary>
/// <param name="format"></param>
/// <param name="value"></param>

		public void SetMoleculeAndRender(MoleculeFormat format, string value)
		{
			MolFormat = format;
			MolString = value;

			CdkMol = new CdkMol(MolFormat, MolString);

			RenderMolecule();

			return;
		}

/// <summary>
/// RenderMolecule
/// </summary>

		public void RenderMolecule()
		{
			Rectangle destRect = this.Bounds, boundingRect;
			string svg;
			Svg.SvgDocument svgDoc;
			Bitmap bm;
			int height; // formatted  height in milliinches
			bool markBoundaries = false;
			int fixedHeight = 0, translateType = 0, desiredBondLength = 100, pageHeight = 11000;
			int miWidth;

			if (MolFormat == MoleculeFormat.Unknown || Lex.IsUndefined(MolString))
			{
				ImageCtl.SvgImage = null;
				ImageCtl.Image = null;
				return;
			}

			svg = CdkMol.GetMoleculeSvg(this.Width, this.Height); // get svg 

			if (DebugMx.False) // Create bitmap from CdkMol
			{
				bm = CdkMol.GetMoleculeBitmap(this.Width, this.Height);
				ImageCtl.Image = bm;
				return;
			}

			if (DebugMx.False) // Create svg, then get bitmap 
			{
				bm = SvgUtil.GetBitmapFromSvgXml(svg, this.Width);
				ImageCtl.Image = bm;
				return;
			}

			if (DebugMx.False) // Fit structure, then get bitmap
			{
				int stdBoxWidthPx = Mobius.Data.MoleculeMx.MilliinchesToPixels(Mobius.Data.MoleculeMx.StandardBoxWidth);
				double scale = (float)this.Width / stdBoxWidthPx;
				desiredBondLength = CdkMol.AdjustBondLengthToValidRange((int)(Mobius.Data.MoleculeMx.StandardBondLength * scale)); // (not implemented)
				if (desiredBondLength < 1) desiredBondLength = 1;

				CdkMol.FitStructureIntoRectangle // scale and translate structure into supplied rectangle.
					(ref destRect, desiredBondLength, translateType, fixedHeight, markBoundaries, pageHeight, out boundingRect);

				bm = CdkMol.GetMoleculeBitmap(this.Width, this.Height);
				ImageCtl.Image = bm;
				return;
			}

			if (DebugMx.True) // Create control SvgImage directly from SVG 
			{
				MemoryStream ms = StreamConverter.StringToMemoryStream(svg); // convert SVG to image for control
				SvgImage svgImg = new SvgImage(ms);
				ImageCtl.SvgImage = svgImg;
				return;
			}

			if (DebugMx.True) 
			{
				//miWidth = MoleculeMx.PixelsToMilliinches(this.Width);
				//float inchWidth = miWidth / 1000.0f; // convert width milliinches to inches
				//svgDoc = SvgUtil.AdjustSvgDocumentToFitContent(svg, inchWidth, Svg.SvgUnitType.Inch);

				string newSvg = SvgUtil.AdjustSvgToFitContent(svg, this.Width, Svg.SvgUnitType.Pixel, out svgDoc);
				ImageCtl.Image = svgDoc.Draw(ImageCtl.Width, ImageCtl.Height);

				//bm = SvgUtil.GetBitmapFromSvgXml(newSvg, this.Width);
				//ImageCtl.Image = bm;
				return;
			}
		}

		/// <summary>
		/// Getter/setter for molfile 
		/// </summary>

		public string MolfileString
		{
			get
			{
				if (CdkMol == null) return null;
				string molfile = CdkMol.GetMolfile();
				return molfile; 
			}

			set
			{
				SetMoleculeAndRender(MoleculeFormat.Molfile, value);
			}
		}


		public void GetMolecule(out MoleculeFormat format, out string value)
		{		
			format = MolFormat;
			value = MolString;
			return;
		}

		public DisplayPreferences Preferences = null;

		public event EventHandler StructureChanged;

		public MolEditorReturnedHandler EditorReturnedHandler
		{
			get => _editorReturnedHandler;
			set => value = _editorReturnedHandler;
		}
		MolEditorReturnedHandler _editorReturnedHandler;


		// public event MolEditorReturnedHandler EditorReturnedHandler
		// {
		//add	{	_editorReturnedHandler += value; }
		//remove { _editorReturnedHandler -= value;	}
		// }
		// event MolEditorReturnedHandler _editorReturnedHandler;


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
		/// Set control molecule from molfile string
		/// </summary>

		public DisplayPreferences DisplayPreferences;

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

		public Bitmap PaintMolecule(MoleculeFormat molFormat, string molString, int width, int height)
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// Edit the structure in the specified Renditor
		/// </summary>
		/// <param name="renditor"></param>

		public static void EditStructure(
			CdkMolControl renditor)
		{
			AssertMx.IsNotNull(renditor, "renditor");

			try
			{
				throw new NotImplementedException();
			}

			catch (Exception ex)
			{
				string msg =
					"An error has occurred starting the molecule editor";

				DialogResult dr = MessageBox.Show(msg, "Error starting molecule editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		// Stub class based on HelmControl

		[DefaultValue(CdkMolControlMode.BrowserViewOnly)]
		public CdkMolControlMode HelmMode { get => _helmMode; set => _helmMode = value; }
		CdkMolControlMode _helmMode = CdkMolControlMode.BrowserViewOnly;

		public bool BitmapMode => (_helmMode == CdkMolControlMode.OffScreenBitmap); // rendered off-screen and returned as a bitmap that can then be used anywhere
		public bool SvgMode => (_helmMode == CdkMolControlMode.OffScreenSvg); // rendered off-screen and returned as SVG that can be used anywhere 
		public bool ViewMode => (_helmMode == CdkMolControlMode.BrowserViewOnly); // view helm in browser without edit controls
		public bool EditMode => (HelmMode == CdkMolControlMode.BrowserEditor); // show helm in editor

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

		//MolEditorReturnedHandler EditorReturnedHandler { get; set; }

		public event EventHandler MoleculeChanged; // caller-set event to fire when edit value changes

		public static HelmOffScreenBrowser StaticOffScreenBrowser = null;

		public static bool Debug => JavaScriptManager.Debug;

		/// <summary>
		/// Constructor
		/// </summary>

		public CdkMolControl()
		{
			InitializeComponent();

			if (Debug) DebugLog.Message("CdkMolControl instance created" + IdText); // + "\r\n" + (new StackTrace(true)).ToString());
			return;
		}

		/// <summary>
		/// Initialize the on-screen and/or off-screen rendering controls based on the selected CdkMolControlMode
		/// </summary>

		public void InitializeControl()
		{
#if false
			if (HelmMode == CdkMolControlMode.OffScreenBitmap)
			{
				if (OffScreenBrowserMx != null) return;

				OffScreenBrowserMx = new HelmOffScreenBrowser(); // used for generating the bitmap image of the Helm
				OffScreenBrowserMx.LoadInitialPage();

				SetupOnScreenBitmapImageControl();
			}

			else if (HelmMode == CdkMolControlMode.OffScreenSvg)
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
			if (Debug) DebugLog.Message("CdkMolControl initialized for Mode: " + HelmMode + IdText);
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

			OnScreenImageCtl.Click += new EventHandler(CdkMolControl_Click); // allow edit of controls contents
			OnScreenImageCtl.DoubleClick += new EventHandler(CdkMolControl_DoubleClick);

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

			if (HelmMode != CdkMolControlMode.OffScreenSvg)
				throw new Exception("Expected HelmMode == CdkMolControlMode.OffScreenSvg CdkMolControlMode but HelmMode is " + HelmMode);

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
			if (HelmMode != CdkMolControlMode.OffScreenSvg)
				throw new Exception("Expected HelmMode == CdkMolControlMode.OffScreenSvg CdkMolControlMode but HelmMode is " + HelmMode);

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

		private void CdkMolControl_Click(object sender, EventArgs e)
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

		private void CdkMolControl_DoubleClick(object sender, EventArgs e)
		{
			return;
		}

		/// <summary>
		/// Size changed, adjust underlying image or browser control accordingly
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void CdkMolControl_SizeChanged(object sender, EventArgs e)
		{
			RenderMolecule();

			if (Debug) DebugLog.Message("CdkMolControl_SizeChanged: " + this.Size.Width + ", " + this.Size.Height);

			return;
		}

	}

	/// <summary>
	/// Helm renderer/editor mode
	/// </summary>

	public enum CdkMolControlMode
	{
		OffScreenBitmap = 0, // rendered off-screen and returned as a bitmap that can then be used anywhere
		OffScreenSvg = 1, // rendered off-screen and returned as SVG that can be used anywhere 
		BrowserViewOnly = 2, // rendered on-screen by browser but not editable
		BrowserEditor = 3 // Helm structure with editor controls
	}
}
