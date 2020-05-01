
using Mobius.ComOps;
using Mobius.CdkMx;
using Mobius.MolLib2;
using Mobius.Helm;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.Utils;
using DevExpress.Data;

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// User control to render a MoleculeMx that is in one of the allowed formats
	/// </summary>

	public partial class MoleculeControl : DevExpress.XtraEditors.XtraUserControl 
	{
		[DefaultValue(false)]
		public bool ShowLargerStructureInTooltip { get => _showLargerStructureInTooltip; set => _showLargerStructureInTooltip = value; }
		bool _showLargerStructureInTooltip = false;

		[DefaultValue(true)]
		public bool ShowEnlargeStructureButton { get => _showEnlargeStructureButton; set => _showEnlargeStructureButton = value; }
		bool _showEnlargeStructureButton = true;

		public MoleculeMx Molecule // underlying molecule
		{
			get => _molecule;
			set => SetupAndRenderMolecule(value);
		}
		private MoleculeMx _molecule = new MoleculeMx(); // underlying molecule for control, init with empty molecule

		public MoleculeRendererType RendererType => GetRendererType(); // type of currently selected render control
		public bool DisplayChem => (RendererType == MoleculeRendererType.Chemistry);
		public bool DisplayHelm => (RendererType == MoleculeRendererType.Helm);
		public Control RenderControl => GetRenderControl(); // current renderer	control

		public string MolfileString => GetMolfileString();

		public string ChimeString => Molecule.GetChimeString();

		public string SmilesString => Molecule.GetSmilesString();

		public string HelmString => Molecule.GetHelmString();

		public string TagString = null;

		public bool AllowEditing = true; // allow molecule to be edited

		internal bool StructurePopupEnabled = true;
		internal Timer StructurePopupTimer; // timer to monitor when to hide structure popup
		string LastPopupMolString = null;
		internal MoleculeFormat LastPopupMolFormat = MoleculeFormat.Unknown;

		internal WindowsMessageFilter RenditorRtClickMessageFilter; // to catch rt-click within chemical renditor

		internal static MoleculeControl StaticChemCopyPasteMoleculeControl = new MoleculeControl(); // used for static copy/paste of molecules

		public bool InSetup = false;

		// Public events

		public event EventHandler MoleculeClicked; // event to fire when control is clicked

		public event EventHandler EditValueChanged; // event to fire when edit value changes

		/// <summary>
		/// Constructor
		/// </summary>

		public MoleculeControl()
		{
			InitializeComponent();

			InSetup = true;

			try
			{
				MolLib1MoleculeControl.Dock = DockStyle.Fill;
				//MolLib1MoleculeControl.BorderStyle = BorderStyle.None;
				CdkMx.MoleculeControl.SetStandardDisplayPreferences(MolLib1MoleculeControl);

				HelmControl.HelmMode = Helm.HelmControlMode.BrowserViewOnly; // setup with rendering directly from Chrome browser (not just a bitmap)
				HelmControl.Dock = DockStyle.Fill;
				HelmControl.BorderStyle = BorderStyle.None;

				RemoveRenderControlsFromMoleculeControl();

				StructurePopupTimer = new System.Windows.Forms.Timer(); // build timer
				StructurePopupTimer.Interval = 500;
				StructurePopupTimer.Tick += new System.EventHandler(this.StructurePopupTimer_Tick);
				StructurePopupTimer.Enabled = false;

				HelmControl.WindowsMessageFilterCallback = // capture rt-click for HelmControl 
					MoleculeControlRightMouseButtonMessageReceived;

				AllowEditing = false; // don't allow molecule editing by default
			}

			finally { InSetup = false; }

			return;
		}

		private MoleculeRendererType GetRendererType()
		{
			if (Controls.Contains(HelmControl))
				return MoleculeRendererType.Helm;

			else if (Controls.Contains(MolLib1MoleculeControl))
				return MoleculeRendererType.Chemistry;

			else return MoleculeRendererType.Unknown;
		}

		/// <summary>
		/// Get current renderer control if any
		/// </summary>
		/// <returns></returns>

		public Control GetRenderControl() // current renderer	control
		{
			if (Controls.Contains(HelmControl))
				return HelmControl;

			else if (Controls.Contains(MolLib1MoleculeControl))
				return MolLib1MoleculeControl;

			else return null;
		}

		/// <summary>
		/// Create a new empty molecule of the specified format and edit it
		/// </summary>
		/// <param name="format"></param>

		public void SetupAndEditNewMoleculeAsynch(MoleculeFormat format)
		{
			_molecule = new MoleculeMx(format);
			DelayedCallback.Schedule(SetupAndEditMolecule); // invoke editor after returning from this event
		}

		/// <summary>
		/// Setup, render and invoke editor for the current molecule
		/// </summary>

		public void SetupAndEditMolecule()
		{
			SetupRendererControl(_molecule);
			RenderMolecule();
			EditMolecule();
		}

		/// <summary>
		/// Set the molecule render it
		/// </summary>
		/// <param name="mol"></param>

		public void SetupAndRenderMolecule(MoleculeMx mol)
		{
			if (mol == null) mol = new MoleculeMx(MoleculeFormat.Chime); // default to chime format

			_molecule = mol;

			SetupRendererControl(mol);
			RenderMolecule();

			return;
		}

		public void SetupRendererControl(MoleculeMx mol)
		{

			//InSetup = true; // prevent structure-changed event from doing anything

			try
			{
				if (mol == null || mol.IsChemStructureFormat || !mol.IsBiopolymerFormat)
					SetupRendererControl(MoleculeRendererType.Chemistry);

				else if (mol.IsBiopolymerFormat)
					SetupRendererControl(MoleculeRendererType.Helm);

				StructurePopupTimer.Enabled = true;

				return;
			}

			finally { InSetup = false; }
		}

		/// <summary>
		/// Setup the proper renderer control as specified
		/// </summary>
		/// <param name="newRenderer"></param>

		private void SetupRendererControl(MoleculeRendererType newRenderer)
		{
			if (!ShowEnlargeStructureButton && Controls.Contains(EnlargeStructureButton))
				Controls.Remove(EnlargeStructureButton);

			if (RendererType == newRenderer) return;

			RemoveRenderControlsFromMoleculeControl();  // remove any existing renderer

			if (newRenderer == MoleculeRendererType.Chemistry)
			{
				Controls.Add(MolLib1MoleculeControl);
				RenditorRtClickMessageFilter = // capture rt-click for Renditor
					WindowsMessageFilter.CreateRightClickMessageFilter(MolLib1MoleculeControl, MoleculeControlRightMouseButtonMessageReceived);
			}

			else if (newRenderer == MoleculeRendererType.Helm)
			{
				Controls.Add(HelmControl);
			}

			else newRenderer = newRenderer; // shouldn't happen

			EnlargeStructureButton.BringToFront();

			return;
		}

		void RemoveRenderControlsFromMoleculeControl()
		{
			if (Controls.Contains(HelmControl))
				Controls.Remove(HelmControl);

			if (Controls.Contains(MolLib1MoleculeControl))
				Controls.Remove(MolLib1MoleculeControl);

			return;
		}

		/// <summary>
		/// Render the current molecule, assume control is already set up
		/// </summary>

		public void RenderMolecule()
		{
			InSetup = true;

			try
			{
				if (Molecule == null) RemoveRenderControlsFromMoleculeControl();

				else if (Molecule.IsChemStructureFormat)
				{
					string molfile = Molecule.GetMolfileString();
					MolLib1MoleculeControl.MolfileString = molfile;
				}

				else if (Molecule.IsBiopolymerFormat)
				{
					string helm = Molecule.GetHelmString();
					HelmControl.SetHelmAndRender(helm);
				}

				else RemoveRenderControlsFromMoleculeControl();

				return;
			}

			finally { InSetup = false; }
		}

		/// <summary>
		/// Set molecule to empty molecule of preferred type and render empty control
		/// </summary>

		public void ClearMolecule()
		{
			MoleculeFormat molFormat = MoleculeFormat.Molfile;

			if (Molecule != null && Molecule.PrimaryFormat != MoleculeFormat.Unknown)
				molFormat = Molecule.PrimaryFormat;

			SetPrimaryTypeAndValue(molFormat, "");

			return;
		}

		/// <summary>
		/// Start the appropriate molecule editor and return
		/// </summary>

		public void EditMolecule()
		{
			HideStructurePopup();

			if (!DisplayChem && !DisplayHelm) // if nothing there use preferred type
				Molecule = new MoleculeMx(MoleculeMx.PreferredMoleculeFormat);

			if (DisplayChem)
				CdkMx.MoleculeControl.EditStructure(MolLib1MoleculeControl);

			else if (DisplayHelm)
				HelmControl.EditMolecule();

			return;
		}

		/// <summary>
		/// Helm molecule changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void HelmControl_MoleculeChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			string helm = HelmControl.Helm;
			SetPrimaryTypeAndValue(MoleculeFormat.Helm, helm);

			if (MoleculeMx.PreferredMoleculeFormat != MoleculeFormat.Helm) // update preference if desired
			{
				MoleculeMx.PreferredMoleculeFormat = MoleculeFormat.Helm;
				Preferences.Set("PreferredMoleculeFormat", MoleculeFormat.Helm.ToString());
			}

			return;
		}

		/// <summary>
		/// Change molecule type and/or value and call any EditValueChanged event handler
		/// </summary>
		/// <param name="molFormatType"></param>
		/// <param name="molString"></param>

		public void SetPrimaryTypeAndValue(
			MoleculeFormat molFormatType,
			string molString,
			bool callEventHandler = true)
		{
			Molecule.SetPrimaryTypeAndValue(molFormatType, molString);

			if (InSetup) return; // need this?

			SetupRendererControl(Molecule); // redisplay
			RenderMolecule();

			if (callEventHandler && EditValueChanged != null)
				EditValueChanged(this, null);

			return;
		}

		/// <summary>
		/// Get molfile form of molecule
		/// </summary>

		public string GetMolfileString()
		{
			string molfile = "";

			if (Lex.IsDefined(Molecule.MolfileString)) // already have?
				molfile = Molecule.MolfileString;

			else if (Molecule.IsChemStructureFormat)
			{
				molfile = Molecule.GetMolfileString(); // chem format, do any necessary conversion from other chem format
			}

			else if (Molecule.IsBiopolymerFormat) // may need to get from JSDraw object in underlying browser
			{
				molfile = HelmControl.WinFormsBrowserMx.GetMolfile();
				Molecule.MolfileString = molfile; // save for reuse
			}

			else molfile = Molecule.GetMolfileString(); // just in case

			return molfile;
		}

		/// <summary>
		/// Get any molecule object from the clipboard and return it
		/// </summary>
		/// <returns></returns>

		public static MoleculeMx GetMoleculeFromClipboard()
		{
			// Try to paste into the static Molecule control and return the molecule if successful

			if (StaticChemCopyPasteMoleculeControl.PasteMoleculeFromClipboard())
				return StaticChemCopyPasteMoleculeControl.Molecule;

			else return null;
		}

		/// <summary>
		/// Paste a molecule from the clipboard into the control
		/// </summary>
		/// <returns></returns>

		public bool PasteMoleculeFromClipboard()
		{
			try
			{
				if (MolLib1MoleculeControl.CanPaste)
				{
					SetupRendererControl(MoleculeRendererType.Chemistry);
					MolLib1MoleculeControl.PasteFromClipboard();

					SetPrimaryTypeAndValue(MoleculeFormat.Molfile, MolLib1MoleculeControl.MolfileString);
					return true;
				}

				else // get text from clipboard and see if recognizable molecule format
				{
					string txt = Clipboard.GetText();
					MoleculeMx mol = new MoleculeMx(txt);
					if (MoleculeMx.IsDefined(mol))
					{
						SetPrimaryTypeAndValue(mol.PrimaryFormat, mol.PrimaryValue);
						return true; ;
					}

				}

				return false;
			}

			catch { return false; }
		}

		/// <summary>
		/// Copy current molecule in control to clipboard
		/// </summary>

		public void CopyMoleculeToClipboard()
		{
			MoleculeMx mol = this.Molecule;

			if (MoleculeMx.IsUndefined(mol)) return;

			if (mol.IsChemStructureFormat)
			{
				if (MolLib1MoleculeControl.CanCopy)
					MolLib1MoleculeControl.CopyToClipboard();
				return;
			}

			else if (mol.IsBiopolymerFormat)
			{
				CopyMoleculeToClipboard(mol);
				return;
			}

			else return; // unexpected format, just return
		}

		/// <summary>
		/// Copy a molecule to the clipboard
		/// </summary>
		/// <param name="mol"></param>

		public static void CopyMoleculeToClipboard(MoleculeMx mol)
		{
			int bitmapWidth = 1024;

			if (MoleculeMx.IsUndefined(mol)) return;

			if (mol.IsChemStructureFormat)
			{
				lock (StaticChemCopyPasteMoleculeControl) // non-reentrant
				{
					MoleculeControl ctl = StaticChemCopyPasteMoleculeControl;
					ctl.MolLib1MoleculeControl.MolfileString = mol.GetMolfileString(); // set and render the molecule

					if (ctl.MolLib1MoleculeControl.CanCopy)
						ctl.MolLib1MoleculeControl.CopyToClipboard();
				}
				return;
			}

			else if (mol.IsBiopolymerFormat)
			{
				if (Lex.IsUndefined(mol.HelmString)) return;

				IDataObject dataObject = new DataObject();

				string helm = mol.HelmString;

				// Put multiple items on the clipboard (SVG, bitmap, Helm)
				// For Word the first item is the default
				// For Excel text is the default
				// Paste Special can be used to select a particular desired format

				if (DebugMx.True) throw new NotImplementedException();

				string svg = "todo"; // HelmControl.GetSvg(helm); // put the SVG in the clipboard DataObject
				if (Lex.IsDefined(svg))
				{
					byte[] bytes = Encoding.UTF8.GetBytes(svg);
					MemoryStream stream = new MemoryStream(bytes);
					dataObject.SetData("image/svg+xml", stream);

					dataObject.SetData(DataFormats.Html, svg); // also put on as SVG since all machines many not recognize the image/svg+xml MIME type
				}

				Bitmap bm = null; // todo  HelmControl.GetBitmap(helm, bitmapWidth); // put the bitmap in the clipboard DataObject
				if (bm != null)
					dataObject.SetData(DataFormats.Bitmap, bm);

				if (Lex.IsDefined(helm)) // put the Helm string in the clipboard DataObject
				{
					dataObject.SetData(DataFormats.Text, helm);

					//dataObject.SetData("Helm", true, helm); // doesn't work to show Helm label in paste special options
				}

				Clipboard.SetDataObject(dataObject); // Place the data in the Clipboard.
				return;
			}
			return;
		}

		/// <summary>
		/// Set temporary molecule tag string
		/// </summary>
		/// <param name="tagString"></param>

		public void SetTemporaryMoleculeTag(string tagString)
		{
			TagString = tagString + "\t" + Molecule.PrimaryValue; // save tag and current molecule value 
		}

		/// <summary>
		/// Get temporary tag string if molecule hasn't changed since tag was set
		/// </summary>
		/// <returns></returns>

		public string GetTemporaryStructureTag()
		{
			string s = TagString?.ToString();

			if (Lex.IsUndefined(TagString)) return "";

			string[] sa = s.Split('\t');
			if (sa.Length != 2) return "";

			string tagString = sa[0];
			if (Lex.IsDefined(tagString) && Lex.Eq(sa[1], Molecule.PrimaryValue))
				return tagString;

			else return "";
		}

		void ShowStructurePopup()
		{
			StructurePopupTimer_Tick(null, null);
			return;
		}

		/// <summary>
		/// Show/hide chemical structure popup if appropriate and mouse is over a compoundId 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void StructurePopupTimer_Tick(object sender, EventArgs e)
		{
			ToolTipController ttc = ToolTipController;

			StructurePopupTimer.Enabled = false; // disable timer during processing

			if (!ShowLargerStructureInTooltip) return;
			if (RtClickContextMenu.Visible) return;

			try
			{

				ContainerControl ctl = RenderControl as ContainerControl;
				if (ctl == null || Molecule == null)
				{
					HideStructurePopup();
					//DebugLog.Message("Hide: No molecule or displaycontrol control");
					return;
				}

				bool inContainingForm = (Form.ActiveForm == ctl.ParentForm);

				Point pm = Cursor.Position; // get mouse position in screen coords

				Rectangle r = ctl.RectangleToScreen(ctl.Bounds);
				bool inControlRect = r.Contains(pm);

				if (!inContainingForm || !inControlRect)
				{
					HideStructurePopup();
					//DebugLog.Message("Hide: Mouse outside of control");
					//UIMisc.Beep();
					return;
				}

				if (Lex.Eq(Molecule.PrimaryValue, LastPopupMolString) && Molecule.PrimaryFormat == LastPopupMolFormat)
					return;

				LastPopupMolString = Molecule.PrimaryValue;
				LastPopupMolFormat = Molecule.PrimaryFormat;

				SuperToolTip stt = null;
				if (Molecule != null)
					stt = Molecule.BuildStructureTooltip();

				if (stt == null)
				{ // just hide if no structure
					HideStructurePopup();
					DebugLog.Message("Hide: No Structure");
					return;
				}

				// Build and display the popup

				ToolTipControllerShowEventArgs ttcArgs = ToolTipUtil.BuildSuperTooltipArgs(stt, ctl);
				ttcArgs.ToolTipLocation = ToolTipLocation.BottomRight;
				ttc.HideHint(); // be sure any existing tooltip is hidden first

				Point screenLoc = ctl.PointToScreen(new Point(ctl.Left, ctl.Bottom - 20)); // screen loc of hint (bot left of ctl)
				ttc.ShowHint(ttcArgs, screenLoc);

				this.Refresh(); // necessary to refresh to prevent ghosting of popup
				Application.DoEvents();

				//DebugLog.Message("Showing");
				//String.Format("Showing: pm.X {0}, pm.Y {1}, p1.X {2}, p1.Y {3}, p2.X {4}, p2.Y {5}",	pm.X, pm.Y, p1.X, p1.Y, p2.X, p2.Y));
			}

			catch (Exception ex)
			{
				return;
			}

			finally // if control is still visible enable timer otherwise just hide popup
			{
				if (Visible)
					StructurePopupTimer.Enabled = true; // reenable timer

				else HideStructurePopup();
			}
		}

		/// <summary>
		/// Check if hint is visible (via DevExpress)
		/// </summary>
		/// <param name="controller"></param>
		/// <returns></returns>

		private bool IsHintVisible(ToolTipController controller)
		{
			System.Reflection.FieldInfo fi = typeof(ToolTipController).GetField("toolWindow", BindingFlags.NonPublic | BindingFlags.Instance);
			DevExpress.Utils.Win.ToolTipControllerBaseWindow window = fi.GetValue(controller) as DevExpress.Utils.Win.ToolTipControllerBaseWindow;
			return window != null && !window.IsDisposed && window.Visible;
		}

		/// <summary>
		/// Hide structure popup control
		/// </summary>

		void HideStructurePopup()
		{
			ToolTipController ttc = ToolTipController;
			if (ttc != null) ttc.HideHint();
			LastPopupMolString = "";
			LastPopupMolFormat = MoleculeFormat.Unknown;
			//DebugLog.Message("Hide Popup");
		}

		/// <summary>
		/// Callback for Rt-click picked up by Windows MessageFilter
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>

		private bool MoleculeControlRightMouseButtonMessageReceived(int msg)
		{
			if (msg == WindowsMessage.WM_RBUTTONUP) // show menu on button up 
				DelayedCallback.Schedule(ShowMoleculeContextMenu, null); // schedule callback

			HideStructurePopup(); // hide any structure popup that may be showing

			return true; // say handled if down or up
		}

		internal void ShowMoleculeContextMenu(object state)
		{
			HideStructurePopup(); // hide any structure popup that may be showing

			EditMoleculeMenuItem.Visible = Separator1.Visible =
				CutMolecule.Visible = PasteMolecule.Visible = DeleteMenuItem.Visible = AllowEditing;

			if (RendererType == MoleculeRendererType.Helm)
				ViewMoleculeInNewWindowMenuItem.Image = Bitmaps.GetImageFromName(Bitmaps.I.MetaColumnTypeImages, "Helm");

			else ViewMoleculeInNewWindowMenuItem.Image = Bitmaps.GetImageFromName(Bitmaps.I.MetaColumnTypeImages, "Structure");

			Point p = WindowsHelper.GetMousePosition();
			RtClickContextMenu.Show(p);
			return;
		}

		private void EditMolecule_Click(object sender, EventArgs e)
		{
			HideStructurePopup();
			EditMolecule();
			return;
		}

		private void CutMolecule_Click(object sender, EventArgs e)
		{
			HideStructurePopup();
			CopyMoleculeToClipboard();
			ClearMolecule();
			return;
		}

		private void CopyMolecule_Click(object sender, EventArgs e)
		{
			HideStructurePopup();
			CopyMoleculeToClipboard();
			return;
		}

		private void PasteMolecule_Click(object sender, EventArgs e)
		{
			HideStructurePopup();
			PasteMoleculeFromClipboard();
			return;
		}

		private void ViewMoleculeInNewWindow_Click(object sender, EventArgs e)
		{
			DelayedCallback.Schedule(ViewMoleculeInNewWindow);
			return;
		}

		void ViewMoleculeInNewWindow()
		{
			this.Focus(); // move focus away from button
			HideStructurePopup();
			string title = Molecule.Id;
			MoleculeViewer.ShowMolecule(Molecule, title);
			return;
		}

		private void DeleteMenuItem_Click(object sender, EventArgs e)
		{
			HideStructurePopup();
			ClearMolecule();
			return;
		}

		private void EnlargeStructureButton_Click(object sender, EventArgs e)
		{
			ViewMoleculeInNewWindow_Click(null, null);
			return;
		}


		private void EnlargeStructureButton_MouseClick(object sender, MouseEventArgs e)
		{
			return;
		}

		private void EnlargeStructureButton_MouseHover(object sender, EventArgs e)
		{
			return;
		}

	}

}
