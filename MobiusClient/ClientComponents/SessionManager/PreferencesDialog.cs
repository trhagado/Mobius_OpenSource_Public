using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using DevExpress.LookAndFeel;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraBars.Ribbon.ViewInfo;

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

  /// <summary>
  /// User preferences form
  /// </summary>

  public partial class PreferencesDialog : DevExpress.XtraEditors.XtraForm
  {
    public static PreferencesDialog Instance = null; // single instance of form

    static Bitmap MobiusIcon = null; // ref to original app icon
    static int QuickSearchImageIndex = -1; // image index for QuickSearch lightning bold

    string PreferredProjectId = "";
    bool PreferredProjectChanged = false;
    string CurrentLookAndFeel = "";
    bool ChangingLookAndFeelModes = false;

    bool ZoomChanged = false;
    bool InitialScrollGridByPixel = false;

    bool InSetup = false;

    public PreferencesDialog()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Dialog to get preferences from user
    /// </summary>

    public static void Edit()
    {
      if (Instance == null) Instance = new PreferencesDialog();
      Instance.Setup();
      DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
      return;
    }

    private void Setup()
    {
      InSetup = true;

      // Setup preferred project

      MetaTreeNode mtn = MetaTree.GetNode(SS.I.PreferredProjectId);

      // If not found create folder node
      // The following default folder node should exist in the MetaTree root node:
      //   <child name = "default_folder" l = "Private Queries, Lists..." type = "project" item = "default_folder" />

      if (mtn == null)
      {
        mtn = new MetaTreeNode(MetaTreeNodeType.Project);
        mtn.Name = mtn.Target = "DEFAULT_FOLDER";
        mtn.Label = "Private Queries, Lists...";
      }

      PreferredProject.Text = mtn.Label;
      PreferredProjectId = SS.I.PreferredProjectId;
      PreferredProjectChanged = false;

      // Setup default directory

      DefaultFolder.Text = ClientDirs.DefaultMobiusUserDocumentsFolder;

      // Setup zoom

      TableColumnZoom.ZoomPct = SS.I.TableColumnZoom;   // Setup zoom controls
      GraphicsColumnZoom.ZoomPct = SS.I.GraphicsColumnZoom;
      ZoomChanged = false;

      ScrollGridByRow.Checked = !SS.I.ScrollGridByPixel;
      ScrollGridByPixel.Checked = SS.I.ScrollGridByPixel;
      InitialScrollGridByPixel = SS.I.ScrollGridByPixel;

      // Setup look and feel

      LookAndFeelOption.Properties.Items.Clear();
      List<SkinInfoMx> skins = LookAndFeelMx.GetSkins();
      foreach (SkinInfoMx si in skins)
        LookAndFeelOption.Properties.Items.Add(new ImageComboBoxItem(si.ExternalName, si.ImageIndex));

      // Basic old styles (these cause dialog box to close for some reason)

      CurrentLookAndFeel = SS.I.UserIniFile.Read("LookAndFeel", "Blue");
      LookAndFeelOption.Text = LookAndFeelMx.GetExternalSkinName(CurrentLookAndFeel);
      ChangingLookAndFeelModes = false;
      InSetup = false;

      FindRelatedCpdsInQuickSearch.Checked = SS.I.FindRelatedCpdsInQuickSearch;
      RestoreWindowsAtStartup.Checked = SS.I.RestoreWindowsAtStartup;

      return;
    }

    private void BrowseProjects_Click(object sender, EventArgs e)
    {
      MetaTreeNode mtn = new MetaTreeNode();
      mtn.Name = SS.I.PreferredProjectId;

      mtn = MetaTree.GetNode(SS.I.PreferredProjectId);
      mtn = SelectFromContents.SelectSingleItem(
          "Select Preferred Project",
          "Select your preferred project from the choices given below",
          MetaTreeNodeType.Project, // show project folders and above
          mtn,
          false);

      if (mtn == null) return;

      PreferredProject.Text = mtn.Label;
      PreferredProjectId = mtn.Target;
      PreferredProjectChanged = true;
    }

    private void BrowseFolders_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog fbd = new FolderBrowserDialog();
      fbd.Description = "Default Location for Local PC File Storage";
      fbd.SelectedPath = DefaultFolder.Text;
      DialogResult dr = fbd.ShowDialog(SessionManager.ActiveForm);
      if (dr == DialogResult.OK)
        DefaultFolder.Text = fbd.SelectedPath;
    }

    private void LookAndFeelOption_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (InSetup) return;

      bool newStyleIsSkin, currentStyleIsSkin;

      string newLf = LookAndFeelOption.SelectedItem.ToString();
      newLf = LookAndFeelMx.GetInternalSkinName(newLf);

      if (newLf == "Flat" || newLf == "UltraFlat" || newLf == "Classic Windows 3D Style")
        newStyleIsSkin = false;
      else newStyleIsSkin = true;

      if (GetDefaultLookAndFeelStyle() == LookAndFeelStyle.Skin)
        currentStyleIsSkin = true;
      else currentStyleIsSkin = false;

      if (newStyleIsSkin != currentStyleIsSkin)
      { // changing between a skin and non-skin look and feel causes the dialog to close & reopen so make change permanent now
        SaveLookAndFeel();
        CurrentLookAndFeel = newLf;
        ChangingLookAndFeelModes = true;
      }

      SetLookAndFeel(newLf);

      SessionManager sm = SessionManager.Instance;
      if (sm != null)
      {
        //if (sm.MainMenuControl != null)
        //	sm.MainMenuControl.SetMatchingBackgroundColor();

        if (sm.StatusBarManager != null)
          sm.StatusBarManager.AdjustRetrievalProgressBarLocation();
      }

      return;
    }

    /// <summary>
    /// Set the default look and feel
    /// </summary>
    /// <param name="lookAndFeelName"></param>

    public static void SetLookAndFeel(
      string lookAndFeelName)
    {
      if (Instance == null) Instance = new PreferencesDialog();

      Color mainMenuFontColor = LookAndFeelMx.SetLookAndFeel(lookAndFeelName, Instance.DefaultLookAndFeel.LookAndFeel);

      int mainMenuLeft = 45;

      SessionManager sm = SessionManager.Instance;

      if (sm != null && sm.RibbonCtl != null)
      {
        RibbonControl ribbon = sm.RibbonCtl;
        RibbonViewInfo vi = ribbon.ViewInfo;

        ribbon.Minimized = true; // be sure minimized
        if (MobiusIcon == null) MobiusIcon = ribbon.ApplicationIcon;
        if (QuickSearchImageIndex < 0) QuickSearchImageIndex = sm.QuickSearchControl.ImageIndex;

        if (Lex.Contains(lookAndFeelName, "Office 2010"))
        // || Lex.Contains(lookAndFeelName, "Windows 7"))
        { // use new style ribbon
          ribbon.RibbonStyle = RibbonControlStyle.Office2010;
          ribbon.ShowPageHeadersMode = ShowPageHeadersMode.Hide;
          ribbon.ApplicationIcon = null; // don't show Mobius icon on application button
          ribbon.ApplicationButtonText = "";
          sm.QuickSearchControl.ImageIndex = -1;
        }

        else // Office 2007 ribbon
        {
          ribbon.RibbonStyle = RibbonControlStyle.Office2007;
          ribbon.ApplicationIcon = MobiusIcon;
          sm.QuickSearchControl.ImageIndex = QuickSearchImageIndex;
        }

        // Obsolete
        //if (sm != null && sm.MainMenuControl != null && sm.ShellForm != null)
        //{
        //	MainMenuControl menu = sm.MainMenuControl;

        //	if (ribbon.RibbonStyle == RibbonControlStyle.Office2010)
        //		mainMenuLeft += 14;

        //	menu.Left = mainMenuLeft;

        //	SetMainMenuTopPosition();

        //	menu.SetFontColor(mainMenuFontColor);
        //}
      }

      DevExpress.LookAndFeel.LookAndFeelHelper.ForceDefaultLookAndFeelChanged();
      Application.DoEvents(); // repaint

      return;

      /// Get skin element color
      //DevExpress.Skins.Skin currentSkin = DevExpress.Skins.CommonSkins.GetSkin(PreferencesDialog.Instance.DefaultLookAndFeel.LookAndFeel);
      //string elementName = DevExpress.Skins.CommonSkins.SkinTextBorder;
      //DevExpress.Skins.SkinElement element = currentSkin[elementName];
      //QueryDescDivider.BackColor = element.Border.All;
    }

    /// <summary>
    /// SetMainMenuTopPosition
    /// </summary>

    public static void SetMainMenuTopPosition()
    {
      int mainMenuTop = 36;

      SessionManager sm = SessionManager.Instance;

      if (sm == null || sm.MainMenuControl == null || sm.ShellForm == null) return;

      if (sm.ShellForm.WindowState == FormWindowState.Normal) mainMenuTop = 32;
      else if (sm.ShellForm.WindowState == FormWindowState.Maximized) mainMenuTop = 40;
      else return;

      sm.MainMenuControl.Top = mainMenuTop;
      return;
    }

    /// <summary>
    /// Get current look and feel style type
    /// </summary>
    /// <returns></returns>

    public static LookAndFeelStyle GetDefaultLookAndFeelStyle()
    {
      return Instance.DefaultLookAndFeel.LookAndFeel.Style;
    }

    /// <summary>
    /// Save Look and Feel setting in local preferences
    /// </summary>

    void SaveLookAndFeel()
    {
      string internalName = LookAndFeelMx.GetInternalSkinName(LookAndFeelOption.Text);
      SS.I.UserIniFile.Write("LookAndFeel", internalName);
      //if (SessionManager.Instance != null && SessionManager.Instance.MainMenuControl != null)
      //{ // also save any main menu background color
      //	Color c = SessionManager.Instance.MainMenuControl.BackColor;
      //	string txt = c.R.ToString() + ", " + c.G.ToString() + ", " + c.B.ToString();
      //	SS.I.UserIniFile.Write("MainMenuBackgroundColor", txt);
      //}
    }

    private void OK_Click(object sender, EventArgs e)
    {
      // Save preferred project in server preferences if changed

      if (PreferredProjectChanged)
      {
        SS.I.PreferredProjectId = PreferredProjectId;
        UserObjectDao.SetUserParameter(SS.I.UserName, "PreferredProject", PreferredProjectId);

        SessionManager.Instance.MainContentsControl.ShowNormal(); // redisplay main tree with new selected project open
      }

      // Save default folder info in local preferences

      string folder = DefaultFolder.Text;
      if (folder.EndsWith(@"\") && !folder.EndsWith(@":\"))
        folder = folder.Substring(0, folder.Length - 1);

      if (!System.IO.Directory.Exists(folder))
      {
        XtraMessageBox.Show("Folder does not exist: " + folder, UmlautMobius.String,
          MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        DefaultFolder.Focus();
        return;
      }

      ClientDirs.DefaultMobiusUserDocumentsFolder = folder;
      Preferences.Set("DefaultExportFolder", folder); // also persist

      if (ZoomChanged)
      {
        Preferences.Set("TableColumnZoom", SS.I.TableColumnZoom);
        Preferences.Set("GraphicsColumnZoom", SS.I.GraphicsColumnZoom);
      }

      SS.I.ScrollGridByPixel = ScrollGridByPixel.Checked; // set selected value
      if (SS.I.ScrollGridByPixel != InitialScrollGridByPixel)
        Preferences.Set("ScrollGridByPixel", SS.I.ScrollGridByPixel);

      SaveLookAndFeel();

      if (FindRelatedCpdsInQuickSearch.Checked != SS.I.FindRelatedCpdsInQuickSearch)
      {
        SS.I.FindRelatedCpdsInQuickSearch = !SS.I.FindRelatedCpdsInQuickSearch;
        Preferences.Set("FindRelatedCpdsInQuickSearch", SS.I.FindRelatedCpdsInQuickSearch);
      }

      if (RestoreWindowsAtStartup.Checked != SS.I.RestoreWindowsAtStartup)
      {
        SS.I.RestoreWindowsAtStartup = !SS.I.RestoreWindowsAtStartup;
        Preferences.Set("RestoreWindowsAtStartup", SS.I.RestoreWindowsAtStartup);
      }

      Hide(); // must explicitly hide since closing event is cancelled
    }

    private void Cancel_Click(object sender, EventArgs e)
    {
      if (ZoomChanged) // restore grid options if changed
      {
        SS.I.TableColumnZoom = Preferences.GetInt("TableColumnZoom");
        SS.I.GraphicsColumnZoom = Preferences.GetInt("GraphicsColumnZoom");
        ScaleView(); // restore view
      }

      SS.I.ScrollGridByPixel = InitialScrollGridByPixel;

      if (CurrentLookAndFeel != LookAndFeelMx.GetInternalSkinName(LookAndFeelOption.Text))
      { // restore previous look and feel
        SetLookAndFeel(CurrentLookAndFeel);
      }

      Hide(); // must explicitly hide since closing event is cancelled
    }

    private void PreferencesDialog2_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (ChangingLookAndFeelModes)
      { // cancel since this close caused by switching look and feel modes
        e.Cancel = true;
        ChangingLookAndFeelModes = false;
      }

      else if (DialogResult == DialogResult.None)
        Cancel_Click(sender, e);
    }

    private void TableColumnZoom_EditValueChanged(object sender, EventArgs e)
    {
      SS.I.TableColumnZoom = TableColumnZoom.ZoomPct;
      ScaleView();
      ZoomChanged = true;
      return;
    }

    private void GraphicsColumnZoom_EditValueChanged(object sender, EventArgs e)
    {
      SS.I.GraphicsColumnZoom = GraphicsColumnZoom.ZoomPct;
      ScaleView();
      ZoomChanged = true;
      return;
    }

    /// <summary>
    /// Scale the view
    /// </summary>

    void ScaleView()
    {
      if (QbUtil.CurrentMode != QueryMode.Browse) return;
      Query q = QueriesControl.Instance.CurrentBrowseQuery; // get query being browsed, may differ from CurrentQuery (e.g. preview, transform)
      if (q == null) return;

      QueryManager qm = q.QueryManager as QueryManager;
      if (qm == null || qm.MoleculeGrid == null) return;

      qm.MoleculeGrid.ScaleView(q.ViewScale);
    }

    private void TextColumnZoom_EditValueChanged(object sender, EventArgs e)
    {

    }

  }
}