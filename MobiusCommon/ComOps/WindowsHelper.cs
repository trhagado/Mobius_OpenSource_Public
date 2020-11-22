using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
//using System.Windows.Interop;

namespace Mobius.ComOps
{
	/// <summary>
	/// WindowsHelper implements managed wrappers for unmanaged Win32 APIs.
	/// </summary>
	public class WindowsHelper
	{
		#region structures needed to call into unmanaged Win32 APIs.
		/// <summary>
		/// Point struct used for GetWindowPlacement API.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private class ManagedPt
		{
			public int x = 0;
			public int y = 0;

			public ManagedPt()
			{
			}

			public ManagedPt(int x, int y)
			{
				this.x = x;
				this.y = y;
			}
		}

		/// <summary>
		/// Rect struct used for GetWindowPlacement API.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private class ManagedRect
		{
			public int x = 0;
			public int y = 0;
			public int right = 0;
			public int bottom = 0;

			public ManagedRect()
			{
			}

			public ManagedRect(int x, int y, int right, int bottom)
			{
				this.x = x;
				this.y = y;
				this.right = right;
				this.bottom = bottom;
			}
		}

		/// <summary>
		/// WindowPlacement struct used for GetWindowPlacement API.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private class ManagedWindowPlacement
		{
			public uint length = 0;
			public uint flags = 0;
			public uint showCmd = 0;
			public ManagedPt minPosition = null;
			public ManagedPt maxPosition = null;
			public ManagedRect normalPosition = null;

			public ManagedWindowPlacement()
			{
				this.length = (uint)Marshal.SizeOf(this);
			}
		}

		[StructLayout(LayoutKind.Sequential,
				CharSet = CharSet.Ansi)]
		public struct DEVMODE
		{
			// You can define the following constant
			// but OUTSIDE the structure because you know
			// that size and layout of the structure
			// is very important
			// CCHDEVICENAME = 32 = 0x50
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string dmDeviceName;
			// In addition you can define the last character array
			// as following:
			//[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			//public Char[] dmDeviceName;

			// After the 32-bytes array
			[MarshalAs(UnmanagedType.U2)]
			public UInt16 dmSpecVersion;

			[MarshalAs(UnmanagedType.U2)]
			public UInt16 dmDriverVersion;

			[MarshalAs(UnmanagedType.U2)]
			public UInt16 dmSize;

			[MarshalAs(UnmanagedType.U2)]
			public UInt16 dmDriverExtra;

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 dmFields;

			public POINTL dmPosition;

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 dmDisplayOrientation;

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 dmDisplayFixedOutput;

			[MarshalAs(UnmanagedType.I2)]
			public Int16 dmColor;

			[MarshalAs(UnmanagedType.I2)]
			public Int16 dmDuplex;

			[MarshalAs(UnmanagedType.I2)]
			public Int16 dmYResolution;

			[MarshalAs(UnmanagedType.I2)]
			public Int16 dmTTOption;

			[MarshalAs(UnmanagedType.I2)]
			public Int16 dmCollate;

			// CCHDEVICENAME = 32 = 0x50
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string dmFormName;
			// Also can be defined as
			//[MarshalAs(UnmanagedType.ByValArray,
			//    SizeConst = 32, ArraySubType = UnmanagedType.U1)]
			//public Byte[] dmFormName;

			[MarshalAs(UnmanagedType.U2)]
			public UInt16 dmLogPixels;

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 dmBitsPerPel;

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 dmPelsWidth;

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 dmPelsHeight;

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 dmDisplayFlags;

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 dmDisplayFrequency;

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 dmICMMethod;

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 dmICMIntent;

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 dmMediaType;

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 dmDitherType;

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 dmReserved1;

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 dmReserved2;

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 dmPanningWidth;

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 dmPanningHeight;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct POINTL
		{
			[MarshalAs(UnmanagedType.I4)]
			public int x;
			[MarshalAs(UnmanagedType.I4)]
			public int y;
		}

		#endregion

		// External Win32 APIs that we're calling directly.
		[DllImport("USER32.DLL", SetLastError = true)]
		private static extern uint ShowWindow(uint hwnd, int showCommand);

		[DllImport("USER32.DLL", SetLastError = true)]
		private static extern uint SetForegroundWindow(uint hwnd);

		[DllImport("USER32.DLL", SetLastError = true)]
		private static extern uint GetWindowPlacement(uint hwnd,
			[In, Out]ManagedWindowPlacement lpwndpl);

		[DllImport("USER32.DLL", SetLastError = true)]
		private static extern uint FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll")]
		static extern uint GetForegroundWindow();

		[DllImport("user32.dll")]
		static extern int GetWindowText(uint hWnd, StringBuilder text, int count);

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool MoveWindow(uint hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		static extern IntPtr WindowFromPoint(System.Drawing.Point p);

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool SetProcessDPIAware();

		[DllImport("user32")]
		static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

		[DllImport("user32.dll", SetLastError = false)]
		static extern IntPtr GetDesktopWindow();

		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern Boolean EnumDisplaySettings(
		[param: MarshalAs(UnmanagedType.LPTStr)]
		 string lpszDeviceName,
		[param: MarshalAs(UnmanagedType.U4)]
		 int iModeNum,
		[In, Out]
		 ref DEVMODE lpDevMode);

		const int ENUM_CURRENT_SETTINGS = -1;
		const int ENUM_REGISTRY_SETTINGS = -2;

		const int MONITOR_DEFAULTTONULL = 0x00000000;
		const int MONITOR_DEFAULTTOPRIMARY = 0x00000001;
		const int MONITOR_DEFAULTTONEAREST = 0x00000002;

		[DllImport("user32", CharSet = CharSet.Auto)]
		static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);
		const uint MONITORINFOF_PRIMARY = 0x00000001;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		struct MONITORINFOEX
		{
			public int cbSize;
			public RECT rcMonitor;
			public RECT rcWork;
			public uint dwFlags;  // MONITORINFOF_XXX
			const int CCHDEVICENAME = 32;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
			public string szDevice;
			public const int SIZE = 4 + 4 * 4 + 4 * 4 + 4 + 2 * CCHDEVICENAME;
		}

		struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		MONITORINFOEX info;


		// Windows defined constants.
		public const int WA_CLICKACTIVE = 2;
		public const int SW_SHOWNORMAL = 1;
		public const int SW_SHOWMINIMIZED = 2;
		public const int SW_SHOWMAXIMIZED = 3;
		public const int SW_RESTORE = 9;
		public const int WPF_RESTORETOMAXIMIZED = 2;

		public static void SetDPIAware()
		{
			SetProcessDPIAware();
			return;
		}

		/// <summary>
		/// Get the monitor scaling factor for non-DPI aware apps on high res monitors
		/// </summary>
		/// <returns></returns>

		public static double GetMonitorHighDpiScalingFactor()
		{
			// Basic method idea from: https://stackoverflow.com/questions/33507031/detect-if-non-dpi-aware-application-has-been-scaled-virtualized?rq=1
			// Setup for EnumDisplaySettings from: https://www.codeproject.com/Articles/36664/Changing-Display-Settings-Programmatically

			string result = null;

			// Get the monitor that the window is currently displayed on
			// (where hWnd is a handle to the window of interest).

			IntPtr hMonitor = MonitorFromWindow(GetDesktopWindow(), MONITOR_DEFAULTTONEAREST);

			// Get the logical width and height of the monitor.

			MONITORINFOEX mon_info = new MONITORINFOEX();
			mon_info.cbSize = (int)Marshal.SizeOf(mon_info);
			if (!GetMonitorInfo(hMonitor, ref mon_info)) return 1.0; // return 1.0 if fails
			int cxLogical = (mon_info.rcMonitor.right - mon_info.rcMonitor.left);
			int cyLogical = (mon_info.rcMonitor.bottom - mon_info.rcMonitor.top);

			// Get the physical width and height of the monitor.

			DEVMODE mode = new DEVMODE();
			mode.dmSize = (ushort)Marshal.SizeOf(mode);
			if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref mode)) return 1.0; // return 1.0 if fails

			uint cxPhysical = mode.dmPelsWidth;
			uint cyPhysical = mode.dmPelsHeight;

			// Calculate the scaling factor.

			double horzScale = ((double)cxPhysical / (double)cxLogical);
			double vertScale = ((double)cyPhysical / (double)cyLogical);
			if (horzScale != vertScale) throw new Exception("horzScale != vertScale");

			return horzScale;
		}

		/// <summary>
		/// Get the title of the currently active window
		/// </summary>
		/// <returns></returns>

		public static string GetActiveWindowTitle()
		{
			return GetWindowTitle(GetForegroundWindow());
		}

		/// <summary>
		/// Get title of specified window
		/// </summary>
		/// <param name="hwnd"></param>
		/// <returns></returns>

		public static string GetWindowTitle(uint hwnd)
		{
			const int nChars = 256;
			try
			{
				StringBuilder buf = new StringBuilder(nChars);

				if (GetWindowText(hwnd, buf, nChars) > 0)
				{
					string title = buf.ToString();
					return title;
				}
				else return "";
			}
			catch (Exception ex)
			{ return ""; }
		}

		/// <summary>
		/// Check if a control is contained in a class of the specified type
		/// </summary>
		/// <param name="ctl"></param>
		/// <param name="containerType"></param>
		/// <returns></returns>

		public static bool IsControlContainedInContainer(
			Control ctl,
			Type containerType)
		{
			ContainerControl cc;

			if (FindContainerControl(ctl, containerType, out cc)) return true;
			else return false;
		}


		/// <summary>
		/// Return the ContainerControl of the specified type for the supplied Control, if any.
		/// </summary>
		/// <param name="ctl"></param>
		/// <param name="containerType"></param>
		/// <returns></returns>
		public static bool FindContainerControl(
			Control ctl,
			Type containerType,
			out ContainerControl cc)
		{
			cc = null;
			while (true)
			{
				if (ctl == null) return false;

				else if (ObjectEx.IsSameOrSubclassOf(ctl, containerType))
				{
					cc = ctl as ContainerControl;
					return true;
				}

				else ctl = ctl.Parent;
			}
		}

		/// <summary>
		/// Get current mouse position in screen coordinates
		/// </summary>
		/// <returns></returns>

		public static Point GetMousePosition()
		{
			Point p = Cursor.Position; // get mouse position
			return p;
		}

		/// <summary>
		/// Return true if the mouse cursor is within the specified control
		/// </summary>1
		/// <param name="c"></param>
		/// <returns></returns>
		public static bool IsMouseWithinControl(Control c)
		{
			Point p = Cursor.Position; // get mouse position
			IntPtr windowHandle = WindowFromPoint(p);
			Control c2 = Control.FromHandle(windowHandle);
			if (c2 == null) return false;

			if (c == c2 || c.Contains(c2)) return true;
			else return false;
		}

		/// <summary>
		/// Get window pointer from point on screen
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static IntPtr GetWindowFromPoint(System.Drawing.Point p)
		{
			IntPtr wp = WindowFromPoint(p);
			return wp;
		}

		/// <summary>
		/// Resize all windows for a wide (e.g. 1920 pixel) display
		/// </summary>

		public static void ResizeAllWindows()
		{
			int width = 1600; // size for wide (e.g. 1920) screens
			int x = (Screen.PrimaryScreen.Bounds.Width - width) / 2; // center

			int height = Screen.PrimaryScreen.Bounds.Height - 30; // adjust for taskbar
			int y = 0; // start at top

			ResizeAllWindows(x, y, width, height);
		}

		/// <summary>
		/// Resize all windows
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>

		public static void ResizeAllWindows(
			int x,
			int y,
			int width,
			int height)
		{
			WindowArray wa = new WindowArray();
			foreach (uint hwnd in wa)
			{
				ManagedWindowPlacement placement = new ManagedWindowPlacement();
				GetWindowPlacement(hwnd, placement);

				string title = GetWindowTitle(hwnd);
				if (Lex.Contains(title, "GDI+ Window")) continue; // ignore these

				if (placement.showCmd != SW_SHOWNORMAL)
				{
					ShowWindow(hwnd, SW_SHOWNORMAL);
					Application.DoEvents();
				}

				try
				{
					MoveWindow(hwnd, x, y, width, height, true);
				}
				catch (Exception ex)
				{ string msg = ex.Message; }
			}
		}


		/// <summary>
		/// Activate a window by name
		/// </summary>
		/// <param name="title"></param>
		/// <param name="allowSubstringMatches"></param>
		/// <param name="windowState"></param>
		/// <returns></returns>

		public static bool ActivateWindow(
			string title,
			bool allowSubstringMatches,
			int windowState)
		{
			try
			{
				WindowArray wa = new WindowArray();
				bool success;

				title = title.Trim().ToLower();

				StringBuilder sbTitle = new StringBuilder(256);

				uint matchHwnd = 0;
				uint prefixHwnd = 0;
				uint substringHwnd = 0;
				foreach (uint hwnd in wa)
				{
					GetWindowText(hwnd, sbTitle, sbTitle.Capacity);
					string title2 = sbTitle.ToString().ToLower();
					if (title2 == title)
					{
						matchHwnd = hwnd;
						break;
					}
					else if (title2.StartsWith(title))
						prefixHwnd = hwnd;

					else if (title2.IndexOf(title) > 0)
						substringHwnd = hwnd;
				}

				if (matchHwnd == 0 && allowSubstringMatches)
				{
					if (prefixHwnd != 0) matchHwnd = prefixHwnd;
					else if (substringHwnd != 0) matchHwnd = substringHwnd;
				}

				if (matchHwnd == 0) return false;

				WindowsHelper.ActivateWindow((uint)matchHwnd);
				success = SetForegroundWindow((IntPtr)matchHwnd);
				if (!success) return false;
				if (windowState > 0) // try to set window state?
					ShowWindow((uint)matchHwnd, windowState);
				return true;
			}
			catch (Exception ex) { return false; }
		}

		/// <summary>
		/// Brings the specified window to the foreground.
		/// </summary>
		/// <param name="hwndInstance">Handle of the window to activate.</param>

		public static void ActivateWindow(uint hwndInstance)
		{
			// Get the WindowPlacement, so we can decide the best way to 
			// activate the window correctly.
			ManagedWindowPlacement placement = new ManagedWindowPlacement();
			GetWindowPlacement(hwndInstance, placement);

			if (placement.showCmd == SW_SHOWMINIMIZED) // if minimized restore to prev size
				ShowWindow(hwndInstance, SW_RESTORE);

			// if the window is minimized, then we need to restore it to its
			// previous size.  we also take into account whether it was 
			// previously maximized.
			// The code below doesn't seem totally reliable and was replaced with the SW_RESTORE above
			//			int showCmd = (placement.flags == WPF_RESTORETOMAXIMIZED) ? 
			//			SW_SHOWMAXIMIZED : SW_SHOWNORMAL;
			//			ShowWindow(hwndInstance, showCmd);


			else // if not minimized then just call SetForegroundWindow to bring it to the front.
				SetForegroundWindow(hwndInstance); // (sometimes fails for maximized windows?)

		}

		/// <summary>
		/// Position and size form so that it fits on the screen
		/// </summary>
		/// <param name="form"></param>

		public static void FitFormOnScreen(Form form)
		{
			Rectangle sr = Screen.PrimaryScreen.Bounds;

			if (form.Width > sr.Width) // form too wide?
			{
				form.Left = 0;
				form.Width = sr.Width;
			}

			if (form.Height > sr.Height) // form too tall
			{
				form.Top = 0;
				form.Height = sr.Height;
			}

			if (form.Right > sr.Right) // off on rt side?
				form.Left -= (form.Right - sr.Right);

			if (form.Bottom > sr.Bottom) // off on bottom side?
				form.Top -= (form.Bottom - sr.Bottom);

			return;
		}

		/// <summary>
		/// Get form metrics
		/// </summary>
		/// <param name="form"></param>
		/// <returns></returns>

		public static FormMetricsInfo GetFormMetrics(Form form)
		{
			FormMetricsInfo fmi = new FormMetricsInfo();
			Size fs = fmi.FormSize = form.Size;
			Size cs = fmi.ClientSize = form.ClientSize;

			int fw = form.Width;
			int cw = form.ClientSize.Width;

			fmi.BorderWidth = (fs.Width - cs.Width) / 2;
			fmi.TitleBarHeight = fs.Height - cs.Height - 2 * fmi.BorderWidth;

			return fmi;
		}

		/// <summary>
		/// Get the location to center a rectangle on the screen
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>

		public static Point GetCenteredScreenLocation(Size size)
		{
			Screen screen = null;

			if (Form.ActiveForm != null) // if Active form defined use it to get proper screen in case of multiple monitors of different sizes
				screen = Screen.FromControl(Form.ActiveForm);

			if (screen == null)
				screen = Screen.PrimaryScreen; // default to primary screen

			Rectangle workingArea = screen.WorkingArea;
			Point location = new Point()
			{
				X = Math.Max(workingArea.X, workingArea.X + (workingArea.Width - size.Width) / 2),
				Y = Math.Max(workingArea.Y, workingArea.Y + (workingArea.Height - size.Height) / 2)
			};

			return location;
		}

		public static string GetEventHandlerName(
			Control c,
			string eventName)
		{
			try
			{
				EventHandlerList events = (EventHandlerList)typeof(Component)
						.GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance)
						.GetValue(c, null);

				object key = c.GetType().GetField("EVENT_" + eventName, BindingFlags.NonPublic | BindingFlags.Static)
						.GetValue(null);

				Delegate handlers = events[key];
				foreach (Delegate handler in handlers.GetInvocationList())
				{
					MethodInfo method = handler.Method;
					string name = handler.Target == null ? "" : handler.Target.ToString();
					if (handler.Target is Control) name = ((Control)handler.Target).Name;
					return name;
				}

				return null;
			}

			catch (Exception ex)
			{
				return null;
			}
		}

		/// <summary>
		/// Get list of event handlers for the specified control and event name
		/// </summary>
		/// <param name="b"></param>
		/// <param name="eventName"></param>
		/// <returns></returns>

		public static EventHandlerList GetEventHandlers(
			Control b,
			string eventName)
		{
			FieldInfo f1 = typeof(Control).GetField(eventName,
			BindingFlags.Static | BindingFlags.NonPublic);
			object obj = f1.GetValue(b);
			PropertyInfo pi = b.GetType().GetProperty("Events",
				BindingFlags.NonPublic | BindingFlags.Instance);
			EventHandlerList list = (EventHandlerList)pi.GetValue(b, null);
			return list;
		}

		/// <summary>
		/// Remove all event handlers of a particular type from a Windows control
		/// </summary>
		/// <param name="b"></param>
		/// <param name="handlerName"></param>

		public static void RemoveEventHandlers(
			Control b,
			string handlerName)
		{
			FieldInfo f1 = typeof(Control).GetField(handlerName,
			BindingFlags.Static | BindingFlags.NonPublic);
			object obj = f1.GetValue(b);
			PropertyInfo pi = b.GetType().GetProperty("Events",
				BindingFlags.NonPublic | BindingFlags.Instance);
			EventHandlerList list = (EventHandlerList)pi.GetValue(b, null);
			list.RemoveHandler(obj, list[obj]);
			return;
		}

		/// <summary>
		/// Return true if a control contains a child control of the specified type
		/// </summary>
		/// <param name="c"></param>
		/// <param name="t"></param>
		/// <returns></returns>

		public static bool ControlContainsChildControlOfSpecifiedType(
			Control c,
			Type t)
		{
			if (c == null) return false;

			if (c.GetType().Name == t.Name) return true;

			foreach (Control c0 in c.Controls)
			{
				if (ControlContainsChildControlOfSpecifiedType(c0, t)) return true;
			}
			return false;
		}

		/// <summary>
		/// Dispose of a list of controls 
		/// </summary>
		/// <param name="cList"></param>
		/// <returns></returns>

		public static int DisposeOfChildControls(Control c)
		{
			int dispCnt = 0;
			List<Control> cList = new List<Control>();

			foreach (Control c0 in c.Controls)
			{
				cList.Add(c0);
			}

			foreach (Control c0 in cList)
			{
				try
				{
					dispCnt += DisposeOfControl(c0);
				}

				catch (Exception ex)
				{
					continue;
				}
			}

			c.Controls.Clear();
			return dispCnt;
		}

		/// <summary>
		/// Dispose a control and all its children
		/// </summary>

		public static int DisposeOfControl(
			Control c,
			int depth = 0)
		{
			if (c == null) return 0;

			if (ClientState.IsDeveloper && DebugMx.False)
				DebugLog.Message(new string(' ', depth) + "Disposing of " + depth + " " + c.GetType() + " " + c.Name);

			int dispCnt = DisposeOfChildControlsOf(c, depth);

			c.Parent = null; // remove from parent first
			c.Dispose(); // then dispose

			dispCnt++;
			return dispCnt;
		}

		/// <summary>
		/// Dispose (and remove) all the children of a control
		/// </summary>

		static int DisposeOfChildControlsOf(
			Control c,
			int depth)
		{
			int dispCnt = 0;

			if (c == null || c.Controls == null) return 0;

			depth++;

			while (c.Controls.Count > 0)
			{
				Control child = c.Controls[0];
				dispCnt += DisposeOfControl(child, depth);
			}

			depth--;
			return dispCnt;
		}

		/// <summary>
		/// Perform a managed memory garbage collection
		/// </summary>
		/// <returns></returns>

		public static string GarbageCollect()
		{
			GC.Collect(); // put objects on reachable queue
			GC.WaitForPendingFinalizers(); // finalize garbage objects
			GC.Collect(); // collect garbage objects

			long managedMem = GC.GetTotalMemory(true);
			string mmTxt = MemoryInfo.ToString(managedMem);

			Process p = Process.GetCurrentProcess();
			//long wsMem = p.PagedMemorySize64;
			//long pagedMem = p.WorkingSet64;
			//long privateMem = p.PrivateMemorySize64;
			//long virtualMem = p.VirtualMemorySize64;
			//long peakVirtualMem = p.PeakVirtualMemorySize64;

			string tmTxt = MemoryInfo.GetFormattedMemoryUsageForProcess(p.Id);

			return "Managed memory used: " + mmTxt + ", Total memory used: " + tmTxt;
		}

		/// <summary>
		/// Delegate to allow display of progress messages from non-ui threads
		/// </summary>
		/// <param name="msg"></param>

		public delegate void InvokeShowProgressDelegate(string msg);

		public static InvokeShowProgressDelegate InvokeShowProgressDelegateInstance = null;
	}

	/// <summary>
	/// WinArray: generate ArrayList of top-level windows using EnumWindows.
	/// </summary>

	public class WindowArray : ArrayList
	{
		private delegate bool EnumWindowsCB(uint hwnd, IntPtr param);

		[DllImport("user32")]
		private static extern int EnumWindows(EnumWindowsCB cb,
			IntPtr param);

		private static bool MyEnumWindowsCB(uint hwnd, IntPtr param)
		{
			GCHandle gch = (GCHandle)param;
			WindowArray itw = (WindowArray)gch.Target;
			itw.Add(hwnd);
			return true;
		}

		// This is the only public method, the only one you need to call
		public WindowArray()
		{
			GCHandle gch = GCHandle.Alloc(this);
			EnumWindowsCB ewcb = new EnumWindowsCB(MyEnumWindowsCB);
			EnumWindows(ewcb, (IntPtr)gch);
			gch.Free();
		}

	}

	/// <summary>
	/// MessageSnatcher
	/// </summary>

	public class MessageSnatcher : NativeWindow
	{
		public event EventHandler RightMouseClickOccured = delegate { };
		private readonly Control _control;

		public MessageSnatcher(Control control)
		{
			if (control.Handle != IntPtr.Zero)
				AssignHandle(control.Handle);
			else
				control.HandleCreated += OnHandleCreated;

			control.HandleDestroyed += OnHandleDestroyed;
			_control = control;
		}

		protected override void WndProc(ref Message m)
		{
			Int64 wParam = m.WParam.ToInt64();

			//DebugLog.Message(m.Msg.ToString() + ", " + wParam);

			if (m.Msg == WindowsMessage.WM_PARENTNOTIFY)
			{
				if (wParam == WindowsMessage.WM_RBUTTONDOWN && RightMouseClickOccured != null)
				{
					//DebugLog.Message("WindowsMessage.WM_RBUTTONDOWN");

					RightMouseClickOccured(this, EventArgs.Empty);
					return; // don't pass through (could modify to add "handled" flag so that we could return if handled or pass-through if not
				}

				else if (wParam == WindowsMessage.WM_RBUTTONUP && RightMouseClickOccured != null) // doesn't get this
				{
					//RightMouseClickOccured(this, EventArgs.Empty);
					return; // just ignore
				}
			}

			base.WndProc(ref m); // pass through to base
			return;
		}

		private void OnHandleCreated(object sender, EventArgs e)
		{
			AssignHandle(_control.Handle);
		}

		private void OnHandleDestroyed(object sender, EventArgs e)
		{
			ReleaseHandle();
		}

	}

	/// <summary>
	/// Class to create MessageFilters for particular windows and message lists
	/// </summary>

	public delegate bool WindowsMessageFilterCallback(int message); // form of callback method

	public class WindowsMessageFilter : IMessageFilter
	{
		[DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr GetParent(IntPtr hWnd);

		Control OwnerControl; // the control that owns this filter
		IntPtr OwnerHandle; // associated window handle
		Hashtable Messages = null; // messages to callback for
		event WindowsMessageFilterCallback CallbackMethod; // the callback method

		/// <summary>
		/// Constructor to create a new filter for a control and list of messages and add it to the application
		/// </summary>
		/// <param name="message"></param>
		/// <param name="windowHandle"></param>
		/// <param name="callBack"></param>
		/// <returns></returns>

		public WindowsMessageFilter(
			Control control,
			int[] messages,
			WindowsMessageFilterCallback callBack)
		{
			OwnerControl = control;
			OwnerHandle = control.Handle;
			Messages = new Hashtable();

			for (int c = 0; c < messages.Length; c++)
			{
				Messages[messages[c]] = 0;
			}

			CallbackMethod = callBack;
			Application.AddMessageFilter(this); // add ourselves as the filter
			return;
		}

		/// <summary>
		/// Create a filter for mouse RightClick events
		/// </summary>
		/// <param name="control"></param>
		/// <param name="callBack"></param>
		/// <returns></returns>

		public static WindowsMessageFilter CreateRightClickMessageFilter(
			Control control,
			WindowsMessageFilterCallback callBack)
		{
			int[] messages = new int[] { WindowsMessage.WM_RBUTTONDOWN, WindowsMessage.WM_RBUTTONUP }; // filter for right button down and up
			WindowsMessageFilter filter = new WindowsMessageFilter(control, messages, callBack);
			return filter;
		}

/// <summary>
/// Call on MouseUp events to determine if double click
/// </summary>
/// <returns></returns>

		//public static bool IsDoubleClick()
		//{

		//	float doubleClickStart = 0;

		//	void OnMouseUp()
		//	{
		//		if ((Time.time - doubleClickStart) & lt; 0.3f)
  //    {
		//		this.OnDoubleClick();
		//		doubleClickStart = -1;
		//	}

		//	else
		//	{
		//		doubleClickStart = Time.time;
		//	}
		//}

		//void OnDoubleClick()
		//{
		//	Debug.Log("Double Clicked!");
		//}

		/// <summary>
		/// Call filter routine for defined window and set of events
		/// </summary>
		/// <param name="m">The message to be dispatched. You cannot modify this message.</param>
		/// <returns>	True to filter the message and stop it from being dispatched; false to allow the message to continue to the next filter or control.</returns>

		public bool PreFilterMessage(ref Message m)
		{
			bool handled = false;

			if (!Messages.ContainsKey(m.Msg)) return false; // one of our messages?

			IntPtr handle = m.HWnd;

			while (handle != IntPtr.Zero) // see if our window or child of our window
			{
				if (handle == OwnerHandle)
				{
					handled = CallbackMethod(m.Msg);
					return handled;
				}

				handle = GetParent(handle);
			}

			return false; // not handled
		}

		public void RemoveFilter()
		{
			try
			{
				Application.RemoveMessageFilter(this);
			}
			catch (Exception ex)
			{
				ex = ex;
			}
		}
	}


	public class ClickOnPointTool
	{

		[DllImport("user32.dll")]
		static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

		[DllImport("user32.dll")]
		internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

#pragma warning disable 649
		internal struct INPUT
		{
			public UInt32 Type;
			public MOUSEKEYBDHARDWAREINPUT Data;
		}

		[StructLayout(LayoutKind.Explicit)]
		internal struct MOUSEKEYBDHARDWAREINPUT
		{
			[FieldOffset(0)]
			public MOUSEINPUT Mouse;
		}

		internal struct MOUSEINPUT
		{
			public Int32 X;
			public Int32 Y;
			public UInt32 MouseData;
			public UInt32 Flags;
			public UInt32 Time;
			public IntPtr ExtraInfo;
		}

#pragma warning restore 649


		public static void ClickOnPoint(IntPtr wndHandle, Point clientPoint)
		{
			var oldPos = Cursor.Position;

			/// get screen coordinates
			ClientToScreen(wndHandle, ref clientPoint);

			/// set cursor on coords, and press mouse
			Cursor.Position = new Point(clientPoint.X, clientPoint.Y);

			var inputMouseDown = new INPUT();
			inputMouseDown.Type = 0; /// input type mouse
			inputMouseDown.Data.Mouse.Flags = 0x0002; /// left button down

			var inputMouseUp = new INPUT();
			inputMouseUp.Type = 0; /// input type mouse
			inputMouseUp.Data.Mouse.Flags = 0x0004; /// left button up

			var inputs = new INPUT[] { inputMouseDown, inputMouseUp };
			SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

			/// return mouse 
			Cursor.Position = oldPos;
		}

	}

	/// <summary>
	/// Low-Level Mouse Hook in C#
	/// </summary>

	public class InterceptMouse
	{
		private static LowLevelMouseProc _proc = HookCallback;
		private static IntPtr _hookID = IntPtr.Zero;
		public static void Main()
		{
			_hookID = SetHook(_proc);
			Application.Run();
			UnhookWindowsHookEx(_hookID);
		}

		public static void SetDefaultHook()
		{
			_hookID = SetHook(_proc);
		}

		public static void UnHook()
		{
			if (_hookID != IntPtr.Zero)
				UnhookWindowsHookEx(_hookID);
			return;
		}

		public static IntPtr SetHook(LowLevelMouseProc proc)
		{
			using (Process curProcess = Process.GetCurrentProcess())
			using (ProcessModule curModule = curProcess.MainModule)
			{
				return SetWindowsHookEx(WH_MOUSE_LL, proc,
						GetModuleHandle(curModule.ModuleName), 0);
			}
		}

		public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

		private static IntPtr HookCallback(
				int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0 &&
					((int)wParam == WindowsMessage.WM_RBUTTONDOWN ||
					(int)wParam == WindowsMessage.WM_RBUTTONUP))
			{
				MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
				Console.WriteLine(hookStruct.pt.x + ", " + hookStruct.pt.y);
				return IntPtr.Zero; // ignore
			}

			return CallNextHookEx(_hookID, nCode, wParam, lParam);
		}

		private const int WH_MOUSE_LL = 14;

		[StructLayout(LayoutKind.Sequential)]

		private struct POINT
		{
			public int x;
			public int y;
		}


		[StructLayout(LayoutKind.Sequential)]

		private struct MSLLHOOKSTRUCT
		{
			public POINT pt;
			public uint mouseData;
			public uint flags;
			public uint time;
			public IntPtr dwExtraInfo;
		}


		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

		private static extern IntPtr SetWindowsHookEx(int idHook,

				LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);


		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

		[return: MarshalAs(UnmanagedType.Bool)]

		private static extern bool UnhookWindowsHookEx(IntPtr hhk);


		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,

				IntPtr wParam, IntPtr lParam);


		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]

		private static extern IntPtr GetModuleHandle(string lpModuleName);

	}

	/// <summary>
	/// Form metrics
	/// </summary>

	public class FormMetricsInfo
	{
		public Size FormSize;
		public Size ClientSize;
		public int BorderWidth;
		public int TitleBarHeight;
	}

	public static class WindowsMessage
	{
		public const int WM_NULL = 0x0000;
		public const int WM_CREATE = 0x0001;
		public const int WM_DESTROY = 0x0002;
		public const int WM_MOVE = 0x0003;
		public const int WM_SIZE = 0x0005;
		public const int WM_ACTIVATE = 0x0006;
		public const int WM_SETFOCUS = 0x0007;
		public const int WM_KILLFOCUS = 0x0008;
		public const int WM_ENABLE = 0x000A;
		public const int WM_SETREDRAW = 0x000B;
		public const int WM_SETTEXT = 0x000C;
		public const int WM_GETTEXT = 0x000D;
		public const int WM_GETTEXTLENGTH = 0x000E;
		public const int WM_PAINT = 0x000F;
		public const int WM_CLOSE = 0x0010;
		public const int WM_QUERYENDSESSION = 0x0011;
		public const int WM_QUERYOPEN = 0x0013;
		public const int WM_ENDSESSION = 0x0016;
		public const int WM_QUIT = 0x0012;
		public const int WM_ERASEBKGND = 0x0014;
		public const int WM_SYSCOLORCHANGE = 0x0015;
		public const int WM_SHOWWINDOW = 0x0018;
		public const int WM_WININICHANGE = 0x001A;
		public const int WM_SETTINGCHANGE = 0x001A;  // (same as WM_WININICHANGE)
		public const int WM_DEVMODECHANGE = 0x001B;
		public const int WM_ACTIVATEAPP = 0x001C;
		public const int WM_FONTCHANGE = 0x001D;
		public const int WM_TIMECHANGE = 0x001E;
		public const int WM_CANCELMODE = 0x001F;
		public const int WM_SETCURSOR = 0x0020;
		public const int WM_MOUSEACTIVATE = 0x0021;
		public const int WM_CHILDACTIVATE = 0x0022;
		public const int WM_QUEUESYNC = 0x0023;
		public const int WM_GETMINMAXINFO = 0x0024;
		public const int WM_PAINTICON = 0x0026;
		public const int WM_ICONERASEBKGND = 0x0027;
		public const int WM_NEXTDLGCTL = 0x0028;
		public const int WM_SPOOLERSTATUS = 0x002A;
		public const int WM_DRAWITEM = 0x002B;
		public const int WM_MEASUREITEM = 0x002C;
		public const int WM_DELETEITEM = 0x002D;
		public const int WM_VKEYTOITEM = 0x002E;
		public const int WM_CHARTOITEM = 0x002F;
		public const int WM_SETFONT = 0x0030;
		public const int WM_GETFONT = 0x0031;
		public const int WM_SETHOTKEY = 0x0032;
		public const int WM_GETHOTKEY = 0x0033;
		public const int WM_QUERYDRAGICON = 0x0037;
		public const int WM_COMPAREITEM = 0x0039;
		public const int WM_GETOBJECT = 0x003D;
		public const int WM_COMPACTING = 0x0041;
		public const int WM_COMMNOTIFY = 0x0044;
		public const int WM_WINDOWPOSCHANGING = 0x0046;
		public const int WM_WINDOWPOSCHANGED = 0x0047;
		public const int WM_POWER = 0x0048;
		public const int WM_COPYDATA = 0x004A;
		public const int WM_CANCELJOURNAL = 0x004B;
		public const int WM_NOTIFY = 0x004E;
		public const int WM_INPUTLANGCHANGEREQUEST = 0x0050;
		public const int WM_INPUTLANGCHANGE = 0x0051;
		public const int WM_TCARD = 0x0052;
		public const int WM_HELP = 0x0053;
		public const int WM_USERCHANGED = 0x0054;
		public const int WM_NOTIFYFORMAT = 0x0055;
		public const int WM_CONTEXTMENU = 0x007B;
		public const int WM_STYLECHANGING = 0x007C;
		public const int WM_STYLECHANGED = 0x007D;
		public const int WM_DISPLAYCHANGE = 0x007E;
		public const int WM_GETICON = 0x007F;
		public const int WM_SETICON = 0x0080;
		public const int WM_NCCREATE = 0x0081;
		public const int WM_NCDESTROY = 0x0082;
		public const int WM_NCCALCSIZE = 0x0083;
		public const int WM_NCHITTEST = 0x0084;
		public const int WM_NCPAINT = 0x0085;
		public const int WM_NCACTIVATE = 0x0086;
		public const int WM_GETDLGCODE = 0x0087;
		public const int WM_SYNCPAINT = 0x0088;


		public const int WM_NCMOUSEMOVE = 0x00A0;
		public const int WM_NCLBUTTONDOWN = 0x00A1;
		public const int WM_NCLBUTTONUP = 0x00A2;
		public const int WM_NCLBUTTONDBLCLK = 0x00A3;
		public const int WM_NCRBUTTONDOWN = 0x00A4;
		public const int WM_NCRBUTTONUP = 0x00A5;
		public const int WM_NCRBUTTONDBLCLK = 0x00A6;
		public const int WM_NCMBUTTONDOWN = 0x00A7;
		public const int WM_NCMBUTTONUP = 0x00A8;
		public const int WM_NCMBUTTONDBLCLK = 0x00A9;
		public const int WM_NCXBUTTONDOWN = 0x00AB;
		public const int WM_NCXBUTTONUP = 0x00AC;
		public const int WM_NCXBUTTONDBLCLK = 0x00AD;

		public const int WM_INPUT_DEVICE_CHANGE = 0x00FE;
		public const int WM_INPUT = 0x00FF;

		public const int WM_KEYFIRST = 0x0100;
		public const int WM_KEYDOWN = 0x0100;
		public const int WM_KEYUP = 0x0101;
		public const int WM_CHAR = 0x0102;
		public const int WM_DEADCHAR = 0x0103;
		public const int WM_SYSKEYDOWN = 0x0104;
		public const int WM_SYSKEYUP = 0x0105;
		public const int WM_SYSCHAR = 0x0106;
		public const int WM_SYSDEADCHAR = 0x0107;
		public const int WM_UNICHAR = 0x0109;
		public const int WM_KEYLAST = 0x0109;

		public const int WM_IME_STARTCOMPOSITION = 0x010D;
		public const int WM_IME_ENDCOMPOSITION = 0x010E;
		public const int WM_IME_COMPOSITION = 0x010F;
		public const int WM_IME_KEYLAST = 0x010F;

		public const int WM_INITDIALOG = 0x0110;
		public const int WM_COMMAND = 0x0111;
		public const int WM_SYSCOMMAND = 0x0112;
		public const int WM_TIMER = 0x0113;
		public const int WM_HSCROLL = 0x0114;
		public const int WM_VSCROLL = 0x0115;
		public const int WM_INITMENU = 0x0116;
		public const int WM_INITMENUPOPUP = 0x0117;
		public const int WM_MENUSELECT = 0x011F;
		public const int WM_MENUCHAR = 0x0120;
		public const int WM_ENTERIDLE = 0x0121;
		public const int WM_MENURBUTTONUP = 0x0122;
		public const int WM_MENUDRAG = 0x0123;
		public const int WM_MENUGETOBJECT = 0x0124;
		public const int WM_UNINITMENUPOPUP = 0x0125;
		public const int WM_MENUCOMMAND = 0x0126;

		public const int WM_CHANGEUISTATE = 0x0127;
		public const int WM_UPDATEUISTATE = 0x0128;
		public const int WM_QUERYUISTATE = 0x0129;

		public const int WM_CTLCOLORMSGBOX = 0x0132;
		public const int WM_CTLCOLOREDIT = 0x0133;
		public const int WM_CTLCOLORLISTBOX = 0x0134;
		public const int WM_CTLCOLORBTN = 0x0135;
		public const int WM_CTLCOLORDLG = 0x0136;
		public const int WM_CTLCOLORSCROLLBAR = 0x0137;
		public const int WM_CTLCOLORSTATIC = 0x0138;
		public const int MN_GETHMENU = 0x01E1;

		public const int WM_MOUSEFIRST = 0x0200;
		public const int WM_MOUSEMOVE = 0x0200;
		public const int WM_LBUTTONDOWN = 0x0201;
		public const int WM_LBUTTONUP = 0x0202;
		public const int WM_LBUTTONDBLCLK = 0x0203;
		public const int WM_RBUTTONDOWN = 0x0204;
		public const int WM_RBUTTONUP = 0x0205;
		public const int WM_RBUTTONDBLCLK = 0x0206;
		public const int WM_MBUTTONDOWN = 0x0207;
		public const int WM_MBUTTONUP = 0x0208;
		public const int WM_MBUTTONDBLCLK = 0x0209;
		public const int WM_MOUSEWHEEL = 0x020A;
		public const int WM_XBUTTONDOWN = 0x020B;
		public const int WM_XBUTTONUP = 0x020C;
		public const int WM_XBUTTONDBLCLK = 0x020D;
		public const int WM_MOUSEHWHEEL = 0x020E;

		public const int WM_PARENTNOTIFY = 0x0210;
		public const int WM_ENTERMENULOOP = 0x0211;
		public const int WM_EXITMENULOOP = 0x0212;

		public const int WM_NEXTMENU = 0x0213;
		public const int WM_SIZING = 0x0214;
		public const int WM_CAPTURECHANGED = 0x0215;
		public const int WM_MOVING = 0x0216;

		public const int WM_POWERBROADCAST = 0x0218;

		public const int WM_DEVICECHANGE = 0x0219;

		public const int WM_MDICREATE = 0x0220;
		public const int WM_MDIDESTROY = 0x0221;
		public const int WM_MDIACTIVATE = 0x0222;
		public const int WM_MDIRESTORE = 0x0223;
		public const int WM_MDINEXT = 0x0224;
		public const int WM_MDIMAXIMIZE = 0x0225;
		public const int WM_MDITILE = 0x0226;
		public const int WM_MDICASCADE = 0x0227;
		public const int WM_MDIICONARRANGE = 0x0228;
		public const int WM_MDIGETACTIVE = 0x0229;

		public const int WM_MDISETMENU = 0x0230;
		public const int WM_ENTERSIZEMOVE = 0x0231;
		public const int WM_EXITSIZEMOVE = 0x0232;
		public const int WM_DROPFILES = 0x0233;
		public const int WM_MDIREFRESHMENU = 0x0234;

		public const int WM_IME_SETCONTEXT = 0x0281;
		public const int WM_IME_NOTIFY = 0x0282;
		public const int WM_IME_CONTROL = 0x0283;
		public const int WM_IME_COMPOSITIONFULL = 0x0284;
		public const int WM_IME_SELECT = 0x0285;
		public const int WM_IME_CHAR = 0x0286;
		public const int WM_IME_REQUEST = 0x0288;
		public const int WM_IME_KEYDOWN = 0x0290;
		public const int WM_IME_KEYUP = 0x0291;

		public const int WM_MOUSEHOVER = 0x02A1;
		public const int WM_MOUSELEAVE = 0x02A3;
		public const int WM_NCMOUSEHOVER = 0x02A0;
		public const int WM_NCMOUSELEAVE = 0x02A2;

		public const int WM_WTSSESSION_CHANGE = 0x02B1;

		public const int WM_TABLET_FIRST = 0x02c0;
		public const int WM_TABLET_LAST = 0x02df;

		public const int WM_CUT = 0x0300;
		public const int WM_COPY = 0x0301;
		public const int WM_PASTE = 0x0302;
		public const int WM_CLEAR = 0x0303;
		public const int WM_UNDO = 0x0304;
		public const int WM_RENDERFORMAT = 0x0305;
		public const int WM_RENDERALLFORMATS = 0x0306;
		public const int WM_DESTROYCLIPBOARD = 0x0307;
		public const int WM_DRAWCLIPBOARD = 0x0308;
		public const int WM_PAINTCLIPBOARD = 0x0309;
		public const int WM_VSCROLLCLIPBOARD = 0x030A;
		public const int WM_SIZECLIPBOARD = 0x030B;
		public const int WM_ASKCBFORMATNAME = 0x030C;
		public const int WM_CHANGECBCHAIN = 0x030D;
		public const int WM_HSCROLLCLIPBOARD = 0x030E;
		public const int WM_QUERYNEWPALETTE = 0x030F;
		public const int WM_PALETTEISCHANGING = 0x0310;
		public const int WM_PALETTECHANGED = 0x0311;
		public const int WM_HOTKEY = 0x0312;

		public const int WM_PRINT = 0x0317;
		public const int WM_PRINTCLIENT = 0x0318;

		public const int WM_APPCOMMAND = 0x0319;

		public const int WM_THEMECHANGED = 0x031A;

		public const int WM_CLIPBOARDUPDATE = 0x031D;

		public const int WM_DWMCOMPOSITIONCHANGED = 0x031E;
		public const int WM_DWMNCRENDERINGCHANGED = 0x031F;
		public const int WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320;
		public const int WM_DWMWINDOWMAXIMIZEDCHANGE = 0x0321;

		public const int WM_GETTITLEBARINFOEX = 0x033F;

		public const int WM_HANDHELDFIRST = 0x0358;
		public const int WM_HANDHELDLAST = 0x035F;

		public const int WM_AFXFIRST = 0x0360;
		public const int WM_AFXLAST = 0x037F;

		public const int WM_PENWINFIRST = 0x0380;
		public const int WM_PENWINLAST = 0x038F;

		public const int WM_APP = 0x8000;

		public const int WM_USER = 0x0400;

		public const int WM_REFLECT = 0x0400 + 0x1C00; // (WM_USER + 0x1C00)
	}
}
