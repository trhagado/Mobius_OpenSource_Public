using Mobius.ComOps;
using Mobius.Data;
using Mobius.Helm;
using Mobius.MolLib1;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraTab;
using DevExpress.XtraEditors.Controls;

using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using Mobius.ClientComponents;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Miscellaneous client UI methods
	/// </summary>

	public class UIMisc : XtraForm
	{
		static int LogicalPixelsX = -1, LogicalPixelsY = -1;

		public static UIMisc Instance => GetInstance();

		private System.ComponentModel.IContainer Components = null;

		public static string SDFFolder = "";
		public static int FileCount;

		public static int PopupPos = 100; // position for next popup window
		public static int PopupPosDelta = 20; // amount to move for next
		public static int PopupCount = 0; // number of popups shown
		public static int PopupCycleCount = 6; // number of popups to show before returning to initial position

		public static bool DisplayedUncWarningMessage = false;
		public static bool InDoevents = false;

		[DllImport("user32.dll", EntryPoint = "SystemParametersInfo", SetLastError = true)]
		public static extern bool SystemParametersInfoGet(
			uint action, uint param, ref uint vparam, uint init);

		[DllImport("user32.dll", EntryPoint = "SystemParametersInfo", SetLastError = true)]
		static extern bool SystemParametersInfoSet(
			uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

		[DllImport("user32.dll")]
		public static extern IntPtr GetFocus();

		public UIMisc()
		{
			InitializeComponent();

			return;
		}

		public static UIMisc GetInstance()
		{
			if (_instance == null) _instance = new UIMisc();
			return _instance;
		}
		private static UIMisc _instance = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Components != null)
				{
					Components.Dispose();
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
			this.SuspendLayout();
			// 
			// UIMisc
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.ClientSize = new System.Drawing.Size(244, 222);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "UIMisc";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "UIMisc";
			//this.Load += new System.EventHandler(this.UIMisc_Load);
			//this.Shown += new System.EventHandler(this.UIMisc_Shown);
			this.ResumeLayout(false);
		}
		#endregion

		public static void Exit()
		{
			Environment.Exit(-1); // exit immediately
		}

		public static int GetLogicalPixelsX()
		{
			if (LogicalPixelsX < 0)
				try
				{
					Graphics g = Instance.CreateGraphics();
					LogicalPixelsX = (int)g.DpiX;  // Horizontal Resolution
				}
				catch (Exception ex)
				{ LogicalPixelsX = 96; } // default value

			return LogicalPixelsX;
		}

		public static int GetLogicalPixelsY()
		{
			if (LogicalPixelsY < 0)
				try
				{
					Graphics g = Instance.CreateGraphics();
					LogicalPixelsY = (int)g.DpiY;  // Vertical Resolution
				}
				catch (Exception ex)
				{ LogicalPixelsY = 96; } // default value

			return LogicalPixelsY;
		}

		/// <summary>
		/// Set label to link if text defined
		/// </summary>
		/// <param name="ctlObj"></param>
		/// <param name="text"></param>
		public static void SetLabelLink(
			Object ctlObj,
			string text)
		{
			Font f;
			FontStyle fs;

			Label label = (Label)ctlObj;
			label.Tag = text;
			f = label.Font;
			fs = f.Style;
			if (text != "")  // show as link if descriptive text is nonblank
			{
				fs |= FontStyle.Underline;
				label.ForeColor = System.Drawing.Color.Blue;
			}
			else // regular non-link text
			{
				fs &= ~FontStyle.Underline;
				label.ForeColor = System.Drawing.Color.Black;
			}
			label.Font = new Font(f, fs);
		}

		/// <summary>
		/// Set control text to appear as a link if defined
		/// </summary>
		/// <param name="ctlObj"></param>
		/// <param name="text"></param>

		public static void SetLinkText(
			Object ctlObj,
			string text)
		{
			Font f;
			FontStyle fs;

			Label label = (Label)ctlObj;
			label.Tag = text;
			f = label.Font;
			fs = f.Style;
			if (text != "")  // show as link if descriptive text is nonblank
			{
				fs |= FontStyle.Underline;
				label.ForeColor = System.Drawing.Color.Blue;
			}
			else // regular non-link text
			{
				fs &= ~FontStyle.Underline;
				label.ForeColor = System.Drawing.Color.Black;
			}
			label.Font = new Font(f, fs);
		}

		/// <summary>
		/// Set clipboard text
		/// </summary>
		/// <param name="txt"></param>

		public static void SetClipBoardText(
			string txt)
		{
			Clipboard.SetText(txt);
		}

		/// <summary>
		/// Get clipboard text
		/// </summary>
		/// <param name="txt"></param>

		public static string GetClipBoardText()
		{
			string txt = "";
			try { txt = Clipboard.GetText(); }
			catch { }
			return txt;
		}

		/// <summary>
		/// Get name of a file to open
		/// </summary>
		/// <param name="title"></param>
		/// <param name="defaultFile"></param>
		/// <param name="defaultExt"></param>
		/// <returns></returns>

		public static string GetOpenFilename(
			string title,
			string defaultFile,
			string defaultExt)
		{
			string filter = "*" + defaultExt + "|*" + defaultExt;
			return GetFilename(1, title, defaultFile, filter, defaultExt);
		}

		/// <summary>
		/// Get name of a file to open
		/// </summary>
		/// <param name="title"></param>
		/// <param name="defaultFile"></param>
		/// <param name="filter"></param>
		/// <param name="defaultExt"></param>
		/// <returns></returns>

		public static string GetOpenFilename(
			string title,
			string defaultFile,
			string filter,
			string defaultExt)
		{
			return GetFilename(1, title, defaultFile, filter, defaultExt);
		}

		/// <summary>
		/// Get name of a file to save
		/// </summary>
		/// <param name="title"></param>
		/// <param name="defaultFile"></param>
		/// <param name="filter"></param>
		/// <param name="defaultExt"></param>
		/// <returns></returns>

		public static string GetSaveAsFilename(
			string title,
			string defaultFile,
			string filter,
			string defaultExt)
		{
			return GetSaveFilename(title, defaultFile, filter, defaultExt, true);
		}

		/// <summary>
		/// Get name of a file to save
		/// </summary>
		/// <param name="title"></param>
		/// <param name="defaultFile"></param>
		/// <param name="filter"></param>
		/// <param name="defaultExt"></param>
		/// <param name="overwritePrompt"></param>
		/// <returns></returns>

		public static string GetSaveFilename(
			string title,
			string defaultFile,
			string filter,
			string defaultExt,
			bool overwritePrompt)
		{
			int action = 2;
			if (!overwritePrompt) action = 3;
			return GetFilename(action, title, defaultFile, filter, defaultExt);
		}

		/// <summary>
		/// Get the name of a file to Open or Save
		/// </summary>
		/// <param name="action">1=open, 2=save, 3=save without prompt</param>
		/// <param name="title"></param>
		/// <param name="defaultFile"></param>
		/// <param name="filter">filter string, e.g. "Lists (*.lst)|*.lst" </param>
		/// <param name="defaultExtension"></param>
		/// <returns></returns>

		private static string GetFilename(
			int action,
			string title,
			string defaultFile,
			string filter,
			string defaultExt)
		{
			OpenFileDialog openDialog = null;
			SaveFileDialog saveDialog = null;
			FileDialog dialog;
			DialogResult result;
			string initialFolder = "", folder = "", fileName = "", ext = "";

			//ClientLog.Message("GetFileName: action = " + action + ", defaultFile = " + defaultFile); // debug

			if (!String.IsNullOrEmpty(defaultFile)) // split out defaultFile name if specified
			{
				if (Lex.StartsWith(defaultFile, "http")) // if url just use name part
				{
					defaultFile = Path.GetFileName(defaultFile); // just get name part
				}

				ext = Path.GetExtension(defaultFile);
				if (ext != "") // if extension assume file name is present
				{
					initialFolder = Path.GetDirectoryName(defaultFile);
					defaultFile = Path.GetFileName(defaultFile);
				}

				else // if no extension assume just a folder
				{
					initialFolder = defaultFile;
					defaultFile = "";
				}

				//ClientLog.Message("GetFileName modified: initialFolder = " + initialFolder + ", defaultFile = " + defaultFile); // debug
			}

			if (initialFolder == "") // still need initial folder
				initialFolder = DirectoryMx.RemoveTerminalBackSlash(ClientDirs.DefaultMobiusUserDocumentsFolder);

			if (action == 1) // open
			{
				openDialog = new OpenFileDialog();
				dialog = (FileDialog)openDialog;
			}
			else // save
			{
				saveDialog = new SaveFileDialog();
				if (action == 3) saveDialog.OverwritePrompt = false;
				dialog = (FileDialog)saveDialog;
			}

			dialog.Title = title;
			dialog.FileName = defaultFile;
			dialog.InitialDirectory = initialFolder; // if initialFolder doesn't exist the control will use the last folder or the current folder
			dialog.Filter = filter;
			dialog.FilterIndex = 1; // filter show first
			dialog.DefaultExt = defaultExt;

			try { result = dialog.ShowDialog(SessionManager.ActiveForm); }
			catch (Exception ex) // may be invalid default name
			{
				ServicesLog.Message("ShowDialog exception: " + ex.Message);
				dialog.FileName = ""; // clear default name
				result = dialog.ShowDialog(SessionManager.ActiveForm); // try again
			}

			fileName = dialog.FileName;
			dialog.Dispose();

			if (result != DialogResult.OK) return "";

			ext = Path.GetExtension(fileName);
			folder = Path.GetDirectoryName(fileName);

			return fileName;
		}

		/// <summary>
		/// Convert a file name to fully qualified form & check for existance
		/// </summary>
		/// <param name="action">1=open, 2=save</param>
		/// <param name="fileName"></param>
		/// <param name="defaultFolder"></param>
		/// <param name="defaultExt"></param>
		/// <returns></returns>

		public static string CheckFileName(
			int action,
			TextEdit textBox,
			string lastValidatedFileName,
			string defaultFolder,
			string defaultExt)
		{
			string fileName = textBox.Text;

			string fullFileName = "";

			if (fileName == "")
			{
				XtraMessageBox.Show("A File name must be supplied", UmlautMobius.String,
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				textBox.Focus();
				return "";
			}

			fullFileName = fileName;

			string folder = Path.GetDirectoryName(fullFileName);
			if (folder == "") // need to add drive & folder
			{
				if (defaultFolder == "") // do we have a default folder?
				{
					defaultFolder = ClientDirs.DefaultMobiusUserDocumentsFolder;
					if (defaultFolder.Length > 3 && defaultFolder[defaultFolder.Length - 1] == '\\')
						defaultFolder = defaultFolder.Substring(0, defaultFolder.Length - 1);
				}
				fullFileName = defaultFolder + @"\" + fullFileName;
			}

			string extension = Path.GetExtension(fullFileName); // extension without period
			if (extension == "") fullFileName = fullFileName + defaultExt;

			folder = Path.GetDirectoryName(fullFileName);
			defaultFolder = folder; // return folder for file as new default

			if (FileUtil.Exists(fullFileName)) // already exists
			{
				if (action == 1) // open for read?
				{
					string msg = FileUtil.CanReadFile(fullFileName);
					if (Lex.IsDefined(msg))
					{
						XtraMessageBox.Show(msg,
							UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						textBox.Focus();
						return ""; // return blank on failure
					}
				}

				else if (action == 2 && Lex.Ne(fullFileName, lastValidatedFileName)) // open for write & not already checked
				{
					if (XtraMessageBox.Show("File " + fullFileName + " already exists.\n" +
						"Do you want to replace it?", "File Exists",
						MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
					{
						textBox.Focus();
						return "";
					}
				}
			}

			else // file does not exist
			{
				if (action == 1) // open for read?
				{
					XtraMessageBox.Show("File not found: " + fullFileName, UmlautMobius.String,
						MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					textBox.Focus();
					return ""; // return blank on failure
				}
			}

			return fullFileName;
		}

		/// <summary>
		/// See if client file can be opened for reading and optionally display error message if not
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public static bool CanReadFile(
			string fileName,
			bool showErrorIfCantRead)
		{
			string msg = FileUtil.CanReadFile(fileName);
			if (msg == "") return true;

			if (showErrorIfCantRead)
			{
				MessageBoxMx.ShowError(
					"Can't open file: " + fileName + "\n" + msg);
			}
			return false;
		}

		/// <summary>
		/// See if client file can be opened for writing to default dir and optionally display error message if not
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public static bool CanWriteFileToDefaultDir(
			string fileName)
		{
			if (!Directory.Exists(ClientDirs.DefaultMobiusUserDocumentsFolder))
			{
				MessageBoxMx.ShowError(
					"Unable to write to your default import/export folder: \"" + ClientDirs.DefaultMobiusUserDocumentsFolder + "\"\n\n" +
					"You can change your default folder if necessary with the Tools > Preferences command");

				return false;
			}

			string msg = FileUtil.CanWriteFile(fileName);
			if (msg == "") return true;
			MessageBoxMx.ShowError(
				"Unable to write file \"" + fileName + "\"to your Mobius default folder: \"" + ClientDirs.DefaultMobiusUserDocumentsFolder + "\"\n\n" +
				"Error message: " + msg + "\n\n" +
				"You can change your default folder if necessary with the Tools > Preferences command");

			return false;
		}

		/// <summary>
		/// See if client file can be opened for writing and optionally display error message if not
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public static bool CanWriteFile(
			string fileName,
			bool showErrorIfCantWrite)
		{
			string msg = "";

			if (!Lex.StartsWith(fileName, "http"))
			{
				msg = FileUtil.CanWriteFile(fileName);
				if (msg == "") return true;
			}

			else // URL
			{
				return true; // todo: really check
			}

			if (showErrorIfCantWrite)
			{
				MessageBoxMx.ShowError(
					"Can't open file for writing: " + fileName + "\n" + msg);
			}
			return false;
		}

		/// <summary>
		/// Start a new instance of the Mobius client
		/// </summary>
		/// <param name="arg"></param>

		public static void StartNewClientProcess(
			string args)
		{
			string command = Application.ExecutablePath + " " + args;

			try
			{
				Process.Start(command);
			}
			catch (Exception ex)
			{
				ServicesLog.Message("UIMisc.StartNewClientProcess error on: " + command + "\n" +
					ex.Message);
			}
		}

		/// <summary>
		/// Fill a control with an image from a file
		/// </summary>
		/// <param name="ctlObj"></param>
		/// <param name="fileName"></param>

		public static void SetControlImageFromFile(
			Object ctlObj,
			string fileName)
		{
			PictureBox ctl = (PictureBox)ctlObj;
			Image i = Image.FromFile(fileName);
			ctl.Image = i.GetThumbnailImage(ctl.Width, ctl.Height, null, IntPtr.Zero);
			i.Dispose();
		}

		/// <summary>
		/// Set the contents of a checked list box
		/// </summary>
		/// <param name="ctlObj"></param>
		/// <param name="listTxt"></param>

		public static void SetCheckedListBox(
			Object ctlObj,
			string listTxt)
		{
			CheckedListBox ctl = (CheckedListBox)ctlObj;
			ctl.Items.Clear();
			string[] sa = listTxt.Split('\n');
			for (int i1 = 0; i1 < sa.Length; i1++)
			{
				string[] sa2 = sa[i1].Split('\t');
				if (sa2[0] == "") continue;
				ctl.Items.Add(sa2[0]);
				int csInt = Int32.Parse(sa2[1]);
				CheckState cs = (CheckState)csInt;
				ctl.SetItemCheckState(ctl.Items.Count - 1, cs);
			}
		}

		public static string GetCheckedListBox(
			Object ctlObj)
		{
			CheckedListBox ctl = (CheckedListBox)ctlObj;
			string listTxt = "";
			for (int i1 = 0; i1 < ctl.Items.Count; i1++)
			{
				string txt = ctl.Items[i1] + "\t" +
					((int)ctl.GetItemCheckState(i1)).ToString() + "\n";
				listTxt += txt;
			}

			return listTxt;
		}

		/// <summary>
		/// Set the list of items in a list control
		/// </summary>
		/// <param name="ctlObj"></param>
		/// <param name="listTxt"></param>

		public static void SetListControlItems(
			Object ctlObj,
			string listTxt)
		{
			ComboBoxEdit ctl = (ComboBoxEdit)ctlObj;
			string[] sa = listTxt.Split('\n');
			ctl.Properties.Items.Clear();
			if (listTxt == null || listTxt.Length == 0) return;

			ctl.Properties.Items.AddRange(sa);
			return;
		}

		public static void SetWaitCursor()
		{
			Cursor.Current = Cursors.WaitCursor;
		}

		public static void SetDefaultCursor()
		{
			Cursor.Current = Cursors.Default;
		}

		/// <summary>
		/// Save current window state information
		/// </summary>
		/// <param name="form"></param>
		/// <param name="iniEntry"></param>

		public static bool SaveWindowPlacement(
			Form form,
			string iniEntry)
		{
			if (SS.I.UserIniFile == null) return false;
			if (form.WindowState != FormWindowState.Maximized &&
				form.WindowState != FormWindowState.Normal) return false;

			string state =
				((int)form.WindowState).ToString() + "," +
				form.Left.ToString() + "," +
				form.Top.ToString() + "," +
				form.Width.ToString() + "," +
				form.Height.ToString();

			SS.I.UserIniFile.Write(iniEntry, state);
			return true;
		}

		/// <summary>
		/// Restore window state information
		/// </summary>
		/// <param name="form"></param>
		/// <param name="iniEntry"></param>

		public static bool RestoreWindowPlacement(
			Form form,
			string iniEntry)
		{
			try
			{
				if (SS.I.UserIniFile == null) return false;
				string state = SS.I.UserIniFile.Read(iniEntry);
				if (state == "") return false;
				string[] sa = state.Split(',');

				FormWindowState ws = (FormWindowState)Int32.Parse(sa[0]);

				if (ws == FormWindowState.Normal)
				{
					form.Left = Int32.Parse(sa[1]);
					form.Top = Int32.Parse(sa[2]);
					form.Width = Int32.Parse(sa[3]);
					form.Height = Int32.Parse(sa[4]);
				}

				form.WindowState = ws;
			}
			catch (Exception ex)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Ping a URI
		/// </summary>
		/// <param name="uri"></param>

		public static void PingUri(
			string uri)
		{
			try
			{
				WebRequest wrq = WebRequest.Create(uri);
				WebResponse wrs = wrq.GetResponse();
			}
			catch (Exception ex)
			{
			}

			return;
		}

		/// <summary>
		/// Get encrypted domain\username
		/// </summary>
		/// <param name="publicKey"></param>
		/// <returns></returns>
		public static string GetEncryptedDomainUsername(
			string publicKey)
		{
			string txt = SS.I.UserDomainName +
				"\\" + SS.I.UserName;
			return EncryptMx.Encrypt(txt, publicKey);
		}

		/// <summary>
		/// Set font smoothing to Standard or ClearType
		/// </summary>
		/// <param name="arg"></param>

		public static void SetStandardFontSmoothing(
			bool arg)
		{
			try
			{
				bool nResult;
				int t0, t1;

				t0 = TimeOfDay.Milliseconds();

				uint SPI_GETFONTSMOOTHING = 0x004A;
				uint SPI_GETFONTSMOOTHINGTYPE = 0x200A;

				uint SPI_SETFONTSMOOTHING = 0x004B;
				uint SPI_SETFONTSMOOTHINGTYPE = 0x200B;

				uint FE_FONTSMOOTHINGSTANDARD = 0x1;
				uint FE_FONTSMOOTHINGCLEARTYPE = 0x2;
				uint newType;

				uint SPIF_UPDATEINIFILE = 0x1;
				uint SPIF_SENDWININICHANGE = 0x2;

				if (arg) newType = FE_FONTSMOOTHINGSTANDARD;
				else newType = FE_FONTSMOOTHINGCLEARTYPE;

				uint oldType = 0;

				nResult = SystemParametersInfoGet( // get the current smoothing type
					SPI_GETFONTSMOOTHINGTYPE,
					0,
					ref oldType,
					0);

				t1 = TimeOfDay.Milliseconds() - t0;
				if (newType == oldType) return;

				nResult = SystemParametersInfoSet( // set new type, this takes 1-3 secs
					SPI_SETFONTSMOOTHINGTYPE,
					0,
					(IntPtr)newType,
					SPIF_SENDWININICHANGE | SPIF_UPDATEINIFILE);

				t1 = TimeOfDay.Milliseconds() - t0;
				return;
			}

			catch (Exception ex) { return; }
		}


		/// <summary>
		/// Dialog to get a color
		/// </summary>
		/// <param name="title">Not currently supported</param>
		/// <param name="intColor"></param>
		/// <returns></returns>

		public static Color GetColorDialog(
			string title,
			Color color,
			Color customColor)
		{
			ColorDialog cd = new ColorDialog();
			if (customColor != Color.Empty) // custom color supplied?
				cd.CustomColors = new int[] { customColor.ToArgb() };

			cd.Color = color;
			DialogResult dr = cd.ShowDialog(SessionManager.ActiveForm);
			if (dr != DialogResult.OK) return Color.Empty;
			else return cd.Color;
		}

		/// <summary>
		/// Increment setup level
		/// </summary>

		public static void EnteringSetup()
		{
			SS.I.UISetupLevel++;
			return;
		}

		/// <summary>
		/// Decrement setup level
		/// </summary>
		/// 
		public static void LeavingSetup()
		{
			SS.I.UISetupLevel--;
			if (SS.I.UISetupLevel < -1) throw new Exception("EndSetup below lower bound");
			return;
		}

		/// <summary>
		/// Check if in setup
		/// </summary>

		public static bool InSetup
		{
			get
			{
				if (SS.I.UISetupLevel > -1) return true;
				else return false;
			}
		}

		/// <summary>
		/// Setup a dropdown control from a dictionary
		/// </summary>
		/// <param name="items">Control item collection to fill</param>
		/// <param name="dictName">Dictionary name, null to reset control</param>

		public static void SetListControlItemsFromDictionary(
			ComboBoxItemCollection items,
			string dictName,
			bool removeDuplicates)
		{
			items.Clear();
			if (String.IsNullOrEmpty(dictName)) return;

			List<string> words = DictionaryMx.GetWords(dictName, removeDuplicates);
			if (words == null) return;
			items.AddRange(words);
			return;
		}

		/// <summary>
		/// Be sure active form is front window to avoid loss of focus problem
		/// </summary>

		public static void BringActiveFormToFront()
		{
			Form active = System.Windows.Forms.Form.ActiveForm;
			if (active != null) active.BringToFront();
			return;
		}

		/// <summary>
		/// Bring form to front and make active
		/// </summary>
		/// <param name="form"></param>

		public static void BringFormToFront(Form form)
		{
			form.BringToFront();
			form.Activate();
			form.Focus();
			return;
		}

		/// <summary>
		/// Get the current control with focus
		/// </summary>
		/// <returns></returns>

		public static Control GetFocusedControl()
		{
			IntPtr ip = GetFocus(); // see which control has focus
			if (ip == null) return null;
			Control c = Control.FromHandle(ip);
			return c;
		}

		/// <summary>
		/// Get a string listing a control and all of its Parents
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public static string GetControlParentsString(Control c)
		{
			StringBuilder sb = new StringBuilder();

			while (c != null)
			{
				if (sb.Length != 0) sb.Append(" -> ");
				sb.Append(c.GetType().ToString());

				if (!String.IsNullOrWhiteSpace(c.Name))
					sb.Append(" : ").Append(c.Name);

				if (!String.IsNullOrWhiteSpace(c.Text))
					sb.Append(" : ").Append(c.Text);

				c = c.Parent;
			}

			return sb.ToString();
		}

		/// <summary>
		/// Send message back to server to get cnList selection
		/// </summary>
		/// <param name="prompt"></param>
		/// <param name="defaultUo"></param>
		/// <returns></returns>
#if false
		public static UserObject SelectCompoundIdListDialog (
			string prompt,
			UserObject defaultUo)
		{
			string msg = "SelectCompoundIdList";
			if (defaultUo != null)
				msg += "\t" + defaultUo.Id.ToString();

			UIMisc.EnableComTimer(false); // disable timer processing for this loop
			Server.SendMessage(msg);

			ObjectOpen.IsClosed = false;
			while (ObjectOpen.IsClosed == false)
			{ // wait for ObjectOpen to complete
				Application.DoEvents();
				Thread.Sleep(100);
			}

			while (true) // get result
			{
				try
				{
					msg = Server.ReceiveMessage();
					if (!msg.StartsWith("SelectCompoundIdListResult")) continue;
					UIMisc.EnableComTimer(true); // reenable timer
					string[] sa = msg.Split('\t');
					if (sa.Length < 3) return null;
					UserObject uo = new UserObject(UserObjectType.CnList);
					uo.Name = sa[1];
					uo.Id = int.Parse(sa[2]);
					return uo;
				}
				catch (Exception ex) { ex = ex; }
			}

		}
#endif

		/// <summary>
		/// Center form on screen
		/// </summary>
		/// <param name="frm"></param>

		public static void CenterFormOnScreen(Form frm)
		{
			// Get the Width and Height of the form
			int frm_width = frm.Width;
			int frm_height = frm.Height;

			//Get the Width and Height (resolution) of the screen
			System.Windows.Forms.Screen src = System.Windows.Forms.Screen.PrimaryScreen;
			int src_height = src.Bounds.Height;
			int src_width = src.Bounds.Width;

			//put the form in the center
			frm.Left = (src_width - frm_width) / 2;
			frm.Top = (src_height - frm_height) / 2;
			return;
		}

		/// <summary>
		/// Select a list file
		/// </summary>
		/// <param name="prompt"></param>
		/// <param name="defaultName"></param>
		/// <returns></returns>

		public static string SelectListFileDialog(
			string prompt,
			string defaultName)
		{
			string name = UIMisc.GetOpenFilename(prompt, defaultName,
				"Lists (*.lst)|*.lst|All files (*.*)|*.*", "LST");
			return name;
		}

		/// <summary>
		/// Prompt user for structure file name and read in structure
		/// </summary>
		/// <returns></returns>

		public static bool ReadMoleculeFileDialog(
			out MoleculeMx mol,
			out string fileName)
		{
			while (true)
			{
				mol = null;

				string filter ="Molecule Files (*.mol; *.sdf; *.smi; *.helm; *.txt)|*.mol; *.sdf; *.smi; *.helm; *.hlm; *.txt;|All files (*.*)|*.*";
				fileName = UIMisc.GetOpenFilename(
					"Retrieve Model Structure from File", "", filter,	"MOL");

				if (String.IsNullOrEmpty(fileName)) return false;

				try
				{
					if (Lex.EndsWith(fileName, ".mol") || Lex.EndsWith(fileName, ".sdf"))
						mol = MoleculeMx.ReadMolfile(fileName);

					if (Lex.EndsWith(fileName, ".smi"))
						mol = MoleculeMx.ReadSmilesFile(fileName);

					else if (Lex.EndsWith(fileName, ".skc"))
						mol = MoleculeMx.ReadSketchFile(fileName);

					else if (Lex.EndsWith(fileName, ".helm"))
						mol = MoleculeMx.ReadHelmFile(fileName);

					else if (Lex.EndsWith(fileName, ".fasta"))
						mol = MoleculeMx.ReadFastaFile(fileName);

					else if (Lex.EndsWith(fileName, ".seq"))
						mol = MoleculeMx.ReadBiopolymerSequenceFile(fileName);

					else // something else, look at the contents
					{
						string txt = FileUtil.ReadFile(fileName);
						mol = new MoleculeMx(txt);
					}
				}

				catch (Exception ex) // if error clear mol and try again 
				{
					mol = null; 
				}

				if (NullValue.IsNull(mol))
				{
					MessageBoxMx.ShowError("Unable to read molecule file: " + fileName);
					continue; // try again
				}

				return true;
			}
		}

		/// <summary>
		/// Show a help document whose with the specified name
		/// </summary>
		/// <param name="parmName"></param>

		public static bool ShowHelpFile(
				string fileName,
				string title)
		{
			if (!new Uri(fileName).IsFile)
			{
				ShowHtmlPopupFormDocument(fileName, title);
				return true;
			}

			if (!Path.IsPathRooted(fileName))
				fileName = ComOps.CommonConfigInfo.MiscConfigDir + @"\HelpFiles\" + fileName;
			ShowHtmlPopupFormDocument(fileName, title);
			return true;
		}

		/// <summary>
		/// Navigate browser to an html popup form
		/// </summary>
		/// <param name="document">URI/UNC or document content</param>
		/// <param name="title">Title of document</param>

		public static void ShowHtmlPopupFormDocument( // navigate browser to a document
			string htmlOrUrl,
			string title)
		{
			string uri;

			Progress.Hide(); // hide any progress so popup gets focus
											 //Thread.Sleep(100); // small time delay helps to keep popup on top
											 //Application.DoEvents();

			if (Lex.IsUri(htmlOrUrl) || htmlOrUrl.StartsWith(@"\\") ||
			 (htmlOrUrl.Length > 3 && htmlOrUrl.Substring(1, 2) == @":\"))
			{
				uri = htmlOrUrl; // html is the reference
				SystemUtil.StartProcess(uri);
				return;
			}

			else // write file to client & open
			{
				uri = ClientDirs.TempDir + @"\PopupHtml" + PopupCount + ".htm";

				StreamWriter sw = new StreamWriter(uri);
				sw.Write(htmlOrUrl);
				sw.Close();

				SS.I.BrowserPopup = new PopupHtml();
				PopupHtml bp = SS.I.BrowserPopup;
				PositionPopupForm(bp);

				bp.Text = title;
				bp.Show();

				bp.WebBrowser.Navigate(uri);
				return;
			}
		}

		/// <summary>
		/// Position a popup form on the screen
		/// </summary>
		/// <param name="form"></param>

		public static void PositionPopupForm(Form form)
		{
			form.Location = NextPopupPos();
			WindowsHelper.FitFormOnScreen(form);
			return;
		}

		/// <summary>
		/// Get incremental position for next popup
		/// </summary>

		public static Point NextPopupPos()
		{
			int pos = PopupPos + (PopupCount % PopupCycleCount) * PopupPosDelta;
			PopupCount++;
			Point p = new Point(pos, pos);
			return p;
		}

		/// <summary>
		/// Navigate browser to the text
		/// </summary>
		/// <param name="txt">Text to navigate to</param>
		/// <param name="title">Title of document</param>

		public static void ShowTextInBrowserWindow( // navigate browser to the supplied text
			string txt,
			string title)
		{
			string ext;
			if (Lex.Contains(txt, "<?xml version")) ext = ".xml"; // xml format

			else // html format
			{
				ext = ".html";
				txt = txt.Replace("\n", "<br>"); // fix up html format for proper line break
			}

			string fileName = TempFile.GetTempFileName(ClientDirs.TempDir, ext);
			StreamWriter sw = new StreamWriter(fileName);
			sw.Write(txt);
			sw.Close();

			ShowHtmlPopupFormDocument(fileName, title);
		}

		/// <summary>
		/// Write out a Base64Binary string to a file and then open it
		/// The app that opens the file is determined bky the fileName parameter suffix
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="content"></param>

		public static void SaveAndOpenBase64BinaryStringDocument(
			string fileName,
			string base64BinaryString)
		{
			byte[] content = Convert.FromBase64String(base64BinaryString);

			SaveAndOpenBinaryDocument(fileName, content);

			return;
		}

		/// <summary>
		/// Write out a byte array to a file and the open it.
		/// The app that opens the file is determined by the fileName parameter suffix
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="content"></param>

		public static void SaveAndOpenBinaryDocument(
			string fileName,
			byte[] content)
		{
			fileName = Path.GetFileName(fileName);
			if (fileName.IndexOf(".") < 0) fileName = "." + fileName; // just a type?
			if (fileName.StartsWith(".")) fileName = "File" + (++FileCount).ToString() + fileName;
			fileName = ClientDirs.TempDir + @"\" + fileName;

			FileStream fs = new FileStream(fileName, FileMode.Create);
			fs.Write(content, 0, content.Length);
			fs.Close();

			SystemUtil.StartProcess(fileName);
		}

		/// <summary>
		/// If the file is a UNC name check to see if the services can write to it
		/// </summary>
		/// <param name="fileName"></param>

		public static DialogResult CanWriteFileFromServiceAccount(
			string fileName)
		{
			//if (DisplayedUncWarningMessage) return DialogResult.OK;

			if (!fileName.StartsWith(@"\\")) return DialogResult.OK; ;

			Progress.Show("Checking Mobius background write privileges...", UmlautMobius.String, false);
			bool canWrite = ServerFile.CanWriteFileFromServiceAccount(fileName);
			Progress.Hide();
			//canWrite = false; // debug
			if (canWrite) return DialogResult.OK;

			DialogResult dr = MessageBoxMx.Show(
				"The specified file is in a shared Windows network folder.\n" +
				"Mobius can't currently perform a background export directly to this file.\n" +
				"However, if write access to this shared folder is granted to the <mobiusAccount>\n" +
				"account then Mobius will be able to export directly to the specified file.",
				"Mobius", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

			DisplayedUncWarningMessage = true;
			return dr;
		}

		public static DialogResult PathContainsDrive(string fileName)
		{
			// DialogResult dr;
			string root = Path.GetPathRoot(fileName);
			if (string.IsNullOrEmpty(root)) return DialogResult.Cancel;
			if (root.Contains(":"))
			{
				MessageBoxMx.Show(
				"The specified file is trying to write to a drive (" + root + ") mapped on your local machine.\n" +
				"The Mobius server will not be able to find a drive from your local machine.\n" +
				"Please use a designated folder on a server. If you are not entering a path, Mobius is getting\n" +
				"the path from your Preferences. Please change your export folder in your preferences.",
				"Mobius", MessageBoxButtons.OK);
				return DialogResult.Cancel;
			}
			return DialogResult.OK;
		}

		/// <summary>
		/// Perform an Application.DoEvents() avoiding recursion and catching and logging any exceptions
		/// </summary>

		public static void DoEvents()
		{
			if (InDoevents) return; // avoid event recursion (note that this can cause reentry into CustomUnboundColumnData)

			InDoevents = true;
			try { Application.DoEvents(); } // update display
			catch (Exception ex)
			{ ClientLog.Message("DoEvents exception: " + DebugLog.FormatExceptionMessage(ex)); }
			InDoevents = false;
		}

		/// <summary>
		/// Beep Alias
		/// </summary>

		public static void Beep()
		{
			SystemUtil.Beep();
			return;
		}

	} // UIMisc


	/// <summary>
	/// Check for cancel response
	/// </summary>

	public class Response
	{

		public static bool IsCancel(
			string response)
		{
			if (response == null ||
				String.Compare(response, "cancel", true) == 0 ||
				String.Compare(response, "command cancel", true) == 0 ||
				String.Compare(response, "stop", true) == 0 ||
				String.Compare(response, "close", true) == 0)
				return true;
			else return false;
		}

	} // Response


	///////////////////////////////////////////////////////////////////////////////////////////
	//////////////// Form image issue for pre-.Net 4 version of Mobius ////////////////////////
	///////////////////////////////////////////////////////////////////////////////////////////
	// To fix the Visual Studio 2010  error message: "Could not load file or assembly or one of its dependencies.", 
	// which appears to be caused by .Net 4 being used for creating .resx files,
	// do a global replace of   j00 LjAuMC4w to  j0y LjAuMC4w (with spaces removed)

	// From: http://www.beta.microsoft.com/VisualStudio/feedback/details/646980/vs2010-resx-compile-problem

	//    We ran into this problem recently after upgrading to VS2010 and then having to downgrade to .NET 3.5 due tocompatibiility problems
	//		with another application that we integrate with. Affter many hours of researching this I was able to get the below workaround to temporarily bypass this BUG. This is a royal pain for us...
	//
	// Workaround
	// 1.    Open Form in Designer and make needed GUI changes. Close designer and save
	// 2.    Compile project and receive RESX compile error (only forms with Imagelist should have this problem)
	// 3.    Double-click resx compile error to open resx file. 
	// 4.    Scroll to top of imagestream.
	// 5.    Edit the top line of the Image stream:
	// AAEAAAD/////AQAAAAAAAAAMAgAAAFdTeXN0ZW0uV2luZG93cy5Gb3JtcywgVmVyc2lvbj00LjAuMC4w
	// TO
	// AAEAAAD/////AQAAAAAAAAAMAgAAAFdTeXN0ZW0uV2luZG93cy5Gb3JtcywgVmVyc2lvbj0yLjAuMC4w
	// 6.    Close and save resx file and recompile. 
	// **NOTE: the only difference are the characters at end "j00LjAuMC4w' to "j0yLjAuMC4w"
	//
	// This needs to be done EVERY TIME you open the form in Designer mode.


}
