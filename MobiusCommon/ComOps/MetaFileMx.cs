using Mobius.ComOps;

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Mobius.ClientComponents
{

	/// <summary>
	/// Utility Mobius MetaFile functions
	/// </summary>

	public class MetaFileMx
	{
		public static IntPtr hDcMetafile;
		public static IntPtr hDcScreen;
		public static int PageWidth, PageHeight; // page dimensions in milliinches
		public static int WindowExtX, WindowExtY; // window extent in logical inches
		public static int ViewExtX, ViewExtY; // viewport extent in device coords

// Below declarations from: http://pinvoke.net/

		[StructLayout(LayoutKind.Sequential)]
			public struct RECT 
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		const int HORZSIZE =  4; // Horizontal size in millimeters 
		const int VERTSIZE =  6; // Vertical size in millimeters   
		const int HORZRES  =  8; // Horizontal width in pixels
		const int VERTRES  = 10; // Vertical height in pixels
		const int LOGPIXELSX = 88; // Logical pixels/inch in X 
		const int LOGPIXELSY = 90; // Logical pixels/inch in Y

		const int MM_ANISOTROPIC = 8;

		[DllImport("user32.dll")]
		public static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern int ReleaseDC(IntPtr hWnd,IntPtr hdc);

		[DllImport("gdi32.dll")] 
		public static extern Int32 GetDeviceCaps(IntPtr hdc, Int32 capindex);

		[DllImport("gdi32.dll")]
		static extern int SetMapMode(IntPtr hdc, int fnMapMode);

		[DllImport("gdi32.dll")]
		static extern bool SetWindowExtEx(IntPtr hdc, int nXExtent, int nYExtent,	
			IntPtr lpSize);

		[DllImport("gdi32.dll")]
		static extern bool SetViewportExtEx(IntPtr hdc, int nXExtent, int nYExtent,
			IntPtr lpSize);

		[DllImport("gdi32.dll")]
		static extern IntPtr CreateEnhMetaFile(IntPtr hdcRef, string lpFilename,
			[In] ref RECT lpRect, string lpDescription);

		[DllImport("gdi32.dll")]
		static extern IntPtr GetEnhMetaFile(string lpszMetaFile);

		[DllImport("gdi32.dll")]
		static extern bool PlayEnhMetaFile(IntPtr hdc, IntPtr hemf, ref RECT lpRect);

		[DllImport("gdi32.dll")]
		static extern IntPtr CloseEnhMetaFile(IntPtr hdc);

		[DllImport("gdi32.dll")]
		static extern bool DeleteEnhMetaFile(IntPtr hemf);

		[DllImport("user32.dll")]
		static extern bool OpenClipboard(IntPtr hWndNewOwner);

		[DllImport("user32.dll")]
		static extern IntPtr GetClipboardData(uint uFormat);

		[DllImport("user32.dll")]
		static extern bool CloseClipboard();

		public MetaFileMx()
		{
			return;
		}

/// <summary>
/// Create a metafile device context (based on the screen) and map it to
/// the current page size.
/// </summary>
/// <param name="metafileName"></param>
/// <param name="bBox"></param>
/// <returns></returns>
		public static Graphics Open (
			string metafileName,
			Rectangle bBox)
		{
			RECT drt;
			float pageWidthInches, pageHeightInches,pageLogPixelsInch;
			float pixelsX, pixelsY;
			float millimetersX, millimetersY;

			PageWidth = bBox.Right - bBox.Left; // page dimensions in milliinches
			PageHeight = bBox.Bottom - bBox.Top;

			pageWidthInches = (bBox.Right - bBox.Left) / 1000.0f;
			pageHeightInches = (bBox.Bottom - bBox.Top) / 1000.0f;
			pageLogPixelsInch = 1000.0f; // logical pixels per inch

			drt.left=(int)(bBox.Left * 2.54); // metafile dest rect in .01mm units
			drt.right=(int)(bBox.Right * 2.54); 
			drt.top=(int)(bBox.Top * 2.54); 
			drt.bottom=(int)(bBox.Bottom * 2.54); 

			IntPtr hDcScreen = GetDC((IntPtr)null); // DC for screen

			pixelsX = (float)GetDeviceCaps( hDcScreen, HORZRES ); // resolution of screen in pixels
			pixelsY = (float)GetDeviceCaps( hDcScreen, VERTRES );
			//ClientLog.Message("PixelsX " + pixelsX.ToString());

			millimetersX = (float)GetDeviceCaps( hDcScreen, HORZSIZE ); // physcial size of screen in millimeters
			millimetersY = (float)GetDeviceCaps( hDcScreen, VERTSIZE );
			//ClientLog.Message("MillimetersX " + millimetersX.ToString());

			hDcMetafile = CreateEnhMetaFile(hDcScreen, metafileName, ref drt, null);
			if (hDcMetafile.ToInt32() == 0)	return null;

			SetMapMode( hDcMetafile, MM_ANISOTROPIC ); // anisotropic mapping mode

			WindowExtX = (int)pageLogPixelsInch;
			WindowExtY = (int)pageLogPixelsInch;
			SetWindowExtEx(   // set the Window extent in "logical" units
				hDcMetafile, WindowExtX, WindowExtY, (IntPtr)0);

			ViewExtX = (int)(25.4f * pixelsX / millimetersX);
			ViewExtY = (int)(25.4f * pixelsY / millimetersY);
			SetViewportExtEx( // set viewport extent in device units
				hDcMetafile, ViewExtX, ViewExtY,(IntPtr)0);   

      Graphics g = Graphics.FromHdc(hDcMetafile);
			return g;
		}

		public static void PlayFromClipboard()
		{
			uint CF_ENHMETAFILE = 14;
			string [] formats;
			IntPtr hmf;
			RECT rt;
			bool rc;
			object o;

//			CloseClipboard();             /

//			IDataObject cb = Clipboard.GetDataObject();
//			if (cb.GetDataPresent("EnhancedMetafile")) 
//				o = cb.GetData("EnhancedMetafile",true);
//			if (cb!=null) formats = cb.GetFormats();

			if (OpenClipboard(IntPtr.Zero)) 
			{
				hmf = GetClipboardData(CF_ENHMETAFILE); 
				if (hmf.ToInt32()==0) return;
				rt.left=rt.top=0; // rectangle to play into in hDcMetafile (units are device units not world (logical))
				rt.right = (int)(PageWidth * ViewExtX / WindowExtX); 
				rt.bottom = (int)(PageHeight * ViewExtY / WindowExtY);
				rc = PlayEnhMetaFile(hDcMetafile, hmf, ref rt); 
				if (!rc) return;
				CloseClipboard();             
			}
			else 
			{
				return;
			}
			return;
		}

		public static void Close ()
		{
			IntPtr hmf;
			hmf=CloseEnhMetaFile(hDcMetafile);
			hDcMetafile=(IntPtr)0;
			ReleaseDC((IntPtr)0, hDcScreen ); // release screen dc
			hDcScreen=(IntPtr)0;
			DeleteEnhMetaFile(hmf);
		}

		public static void TestClipboard()
		{
			string [] formats;
			IDataObject cb = Clipboard.GetDataObject();
			if (cb!=null) formats = cb.GetFormats();
		}


	}
}
