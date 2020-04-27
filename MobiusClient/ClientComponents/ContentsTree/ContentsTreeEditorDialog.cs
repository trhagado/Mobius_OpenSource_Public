using System;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Mobius.ServiceFacade;

namespace Mobius.ClientComponents
{
  /// <summary>
  /// Dialog Window that is used for editing the Contents Tree Control.  This allows users to edit the root.xml file.
  /// </summary>

  public partial class ContentsTreeEditorDialog : DevExpress.XtraEditors.XtraForm
    {

/// <summary>
/// Most-recently saved Root.xml string
/// </summary>
			public string SavedRootXml = null;

        private readonly string _fullNodePath;
        
        public ContentsTreeEditorDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">The path parameter represents the current path and node that the user has selected
        /// in the main tree.  The path is delimited by a "." and has all the parent nodes so we can focus on this node
        /// when the Contents Tree ditor opens.</param>
        public ContentsTreeEditorDialog(string path)
        {
            InitializeComponent();
            _fullNodePath = path;
        }

        /// <summary>
        /// Load the root xml document and call the control's SetData method.  Register for the control's AfterTreeUpdate
        /// method so we can enable/disable various buttons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentsTreeEditorDialog_Load(object sender, EventArgs e)
        {
            XmlDocument rootXml = new XmlDocument();
            string msg = "Begin \r\n\r\n";
            try
            {
                string treeXml = MetaTreeFactory.LockAndReadMetatreeXml();
                msg += string.IsNullOrEmpty(treeXml) ? "treeXml is null" : "treeXml is not null";
                msg += "\r\n\r\n";
                rootXml.LoadXml(treeXml);
                msg += "LoadXml success \r\n\r\n";
                contentsTreeEditorControl1.SetData(rootXml);
                msg += "SetData success \r\n\r\n";
                if (!string.IsNullOrEmpty(_fullNodePath))
                {
                    msg += "_fullNodePath is not null: " + _fullNodePath + " \r\n\r\n";
                    msg += (contentsTreeEditorControl1==null) ? "contentsTreeEditorControl1 is null" : "contentsTreeEditorControl1 is not null";
                    msg += "SetData success \r\n\r\n";
                    contentsTreeEditorControl1.SetCurrentNode(_fullNodePath);
                }
                if (contentsTreeEditorControl1==null) msg += "contentsTreeEditorControl1 is null \r\n\r\n";
                contentsTreeEditorControl1.AfterTreeUpdate += AfterTreeUpdate;

            }
            catch (Exception ex)
            {
                msg += "Unable to open the database contents tree for editing.\r\n\r\n" + ex.Message;
                MessageBoxMx.ShowError(msg);
                Close();
            }
        }

        /// <summary>
        /// We are registered for the AfterTreeUpdate event so we can enable the Apply button.
        /// </summary>
        private void AfterTreeUpdate()
        {
            btnApply.Enabled = true;
        }

        /// <summary>
        /// When the Apply button is clicked, save the root xml document and disable the Apply button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnApply_Click(object sender, EventArgs e)
        {
            if (!SaveRootXml()) return;
            btnApply.Enabled = false;
        }

        /// <summary>
        /// When the OK button is clicked, save the root xml document, disable the Apply button, and close the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!SaveRootXml()) return;
            btnApply.Enabled = false;

						if (SavedRootXml != null)
							DialogResult = System.Windows.Forms.DialogResult.OK;
						else DialogResult = System.Windows.Forms.DialogResult.Cancel; 
            Close();
        }

        /// <summary>
        /// Save the RootXml document after backing up the original.
        /// </summary>
        private bool SaveRootXml()
        {
            // don't do anything if there are no changes.  If someone looks at the tree and makes no changes, 
            // then presses OK, no reason to save a backup and non-existent changes.
            if (!contentsTreeEditorControl1.IsDirty) return true;

            // get the new version of the Xml Data
            XmlDocument rootXml = contentsTreeEditorControl1.GetData();

            // if GetData returned null then we were unable to update the xml from the tree.
            if (rootXml==null) return false;

            // reformat XML so it is pretty and save to the SourceFile location
						StringBuilder rootXmlSb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings {NewLineChars = "\n", Indent = true};
            XmlWriter writer = XmlWriter.Create(rootXmlSb, settings);
            rootXml.WriteTo(writer);
            writer.Close();
						SavedRootXml = rootXmlSb.ToString();
						MetaTreeFactory.SaveMetaTreeXml(SavedRootXml);
           
            return true;
        }

        /// <summary>
        /// When the Cancel button is clicked, close the window without saving any changes to the xml document.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
				{
					if (SavedRootXml != null)
						DialogResult = System.Windows.Forms.DialogResult.OK;
					else DialogResult = System.Windows.Forms.DialogResult.Cancel;

					Close();
				}

        /// <summary>
        /// Always release the XML file when the form closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentsTreeEditorDialog_FormClosed(object sender, FormClosedEventArgs e)
		{
			MetaTreeFactory.ReleaseMetaTreeXml(); // release control of tree
		}

    }
}