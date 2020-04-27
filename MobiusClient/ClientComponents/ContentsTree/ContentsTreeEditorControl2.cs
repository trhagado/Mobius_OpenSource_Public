using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraTreeList.Nodes.Operations;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraEditors.Controls;
using System.Text.RegularExpressions;
using DevExpress.XtraEditors;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Collections;
using System.Linq;

namespace Mobius.ClientComponents
{

/// <summary>
/// This control is used for editing the root.xml file that drives the Mobius navigation tree. It supports draggig and dropping,
/// inserting, deleting and editing of nodes.
/// </summary>

    public partial class ContentsTreeEditorControl2 : XtraUserControl
    {
        public event Action AfterTreeUpdate;
        public bool IsDirty = false;

        private XmlDocument _rootXml;
        private XmlDocument _searchResultsXml;
        private List<XmlNode> _xmlNodesClipboard;
        private enum TreeListClipBoardStateType { Cut, Copy, };
        private TreeListClipBoardStateType _treeListClipBoardState;
        private Hashtable _nodesForDeleting;
        private string _initialPath;

        public ContentsTreeEditorControl2()
        {
            InitializeComponent();
            treeList1.SelectImageList = Bitmaps.Bitmaps16x16;
            _xmlNodesClipboard = new List<XmlNode>();
        }

        /// <summary>
        /// Create various editors for the TreeList Control and set options.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentsTreeEditorControl_Load(object sender, EventArgs e)
        {
            //treeList1.OptionsBehavior.DragNodes = true;
            treeList1.OptionsDragAndDrop.DragNodesMode = DragNodesMode.Multiple;

            RepositoryItemMemoExEdit commentsEditor = new RepositoryItemMemoExEdit
            {
                AcceptsReturn = false,
                AcceptsTab = true
            };

            treeList1.RepositoryItems.Add(commentsEditor);
            treeList1.Columns["Comments"].ColumnEdit = commentsEditor;

            RepositoryItemButtonEdit nameEditor = new RepositoryItemButtonEdit();
            nameEditor.Buttons[0].Kind = ButtonPredefines.Glyph;
            nameEditor.Buttons[0].Image = Properties.Resources.Find;
            nameEditor.Buttons[0].ToolTip = "Search by assay Id";
            treeList1.Columns["Name"].ColumnEdit = nameEditor;
            nameEditor.ButtonClick += nameEditor_ButtonClick;

            RepositoryItemCheckEdit disabledCheckEdit = new RepositoryItemCheckEdit();
            treeList1.Columns["Disabled"].ColumnEdit = disabledCheckEdit;

            EnableToolsMenuItems();
        }

        /// <summary>
        /// Enable tool menus according to user security.
        /// </summary>
        private void EnableToolsMenuItems()
        {
            projectToolStripMenuItem.Enabled = SS.I.UserInfo.Privileges.CanCreateProjects;

            folderToolStripMenuItem.Enabled =
            databaseToolStripMenuItem.Enabled =
            SS.I.UserInfo.Privileges.CanEditContentsTree;

            metaTableToolStripMenuItem1.Enabled =
            uRLToolStripMenuItem.Enabled =
            annotationToolStripMenuItem.Enabled =
            actionToolStripMenuItem.Enabled =
            SS.I.UserInfo.Privileges.CanEditProjects;
        }

        /// <summary>
        /// When the search button is clicked, remove the "ASSAY_" prefix if they added it, then lookup
        /// the assay.  Afterwards, append the "ASSAY_" prefix.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        void nameEditor_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                ButtonEdit btnEdit = (ButtonEdit)sender;
                string columnValue = btnEdit.Text;
                string assayIdString = Regex.Replace(columnValue, "ASSAY_", "");
                int assayId = Convert.ToInt32(assayIdString);
                var assay = AssayMetaData.GetById(assayId); 
                if (assay == null) throw new Exception();
                treeList1.FocusedNode.SetValue("Label", assay.Name);
                treeList1.FocusedNode.SetValue("Type", "metatable");
                btnEdit.Text = "ASSAY_" + assayIdString;
            }
            catch (Exception)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Unable to find assay.");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Public method allowing the caller to set the UserControl with xml data. 
        /// </summary>
        /// <param name="xmlSourceDocument"></param>
        public void SetData(XmlDocument xmlSourceDocument)
        {
            _rootXml = xmlSourceDocument;
            XmlNodeChangedEventHandler handler = (sender, e) => IsDirty = true;
            _rootXml.NodeChanged += handler;
            _rootXml.NodeInserted += handler;
            _rootXml.NodeRemoved += handler;
            ParseContentsTreeEditorXml();
        }

        /// <summary>
        /// When a user tries to edit a node directly from the Mobius main tree, we open the editor and provide
        /// this method to jump directly to the node they came from.  A string path is sent delimited with a period
        /// for each node.
        /// </summary>
        /// <param name="path"></param>
        public void SetCurrentNode(string path)
        {
            treeList1.ForceInitialize();
            char[] delimeter = { '.' };
            string[] nodes = path.Split(delimeter);

            var treeNodes = treeList1.Nodes;
            TreeListNode finalTreeNode = null;

            foreach (var node in nodes)
            {
                foreach (TreeListNode treeListNode in treeNodes)
                {
                    MetaTreeEditNode mtn = GetMetaTreeEditNode(treeListNode);
                    if (String.Equals(mtn.Name, node, StringComparison.CurrentCultureIgnoreCase))
                    {
                        treeListNode.Expanded = true;
                        treeNodes = treeListNode.Nodes;
                        finalTreeNode = treeListNode;
                        break;
                    }
                }
            }
            treeList1.SetFocusedNode(finalTreeNode);
            treeList1.Selection.Clear();
            finalTreeNode.Selected = true;
        }

        /// <summary>
        /// Update the xml document with any changes made to the tree nodes.  Then return the current root Xml Document to the caller.
        /// </summary>
        /// <returns></returns>
        public XmlDocument GetData()
        {
            bool success = UpdateXmlFromTree();
            return success ? _rootXml : null;
        }

        /// <summary>
        /// Update the root xml document object with any updates from the TreeList.
        /// </summary>
        private bool UpdateXmlFromTree()
        {
            GetCheckedNodesOperation op = new GetCheckedNodesOperation();
            treeList1.NodesIterator.DoOperation(op);

            IEnumerator e = op.CheckedNodes.GetEnumerator();
            while (e.MoveNext())
            {
                XmlNode xmlNode = null;

                TreeListNode currentTreeNode = (TreeListNode)e.Current;
                XmlNode currentXmlNode = (XmlNode)currentTreeNode.Tag;

                bool isNew = (currentXmlNode.Attributes.Count == 0); // new child (placeholder) if no attributes 

                MetaTreeEditNode mtn = GetMetaTreeEditNode(currentTreeNode);

                var colName = currentTreeNode[ColName];
                var colType = currentTreeNode[ColType];
                var colLabel = currentTreeNode[ColLabel];
                var colItem = currentTreeNode[ColItem];
                var colComments = currentTreeNode[ColComments];
                bool isDisabled = (bool)currentTreeNode[ColDisabled];

                string name = colName != null ? colName.ToString().Trim() : "";
                string label = colLabel != null ? colLabel.ToString().Trim() : "";
                string type = colType != null ? colType.ToString().Trim() : "";
                string item = colItem != null ? colItem.ToString().Trim() : "";
                string comments = colComments != null ? colComments.ToString().Trim() : "";
                string disabled = isDisabled.ToString().ToLower();

                mtn.Type = MetaTreeNode.ParseTypeString(type);
                bool isParentNode = !MetaTreeNode.IsLeafNodeType(mtn.Type);

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(label) || string.IsNullOrEmpty(type))
                {
                    MessageBox.Show("Name, Label, and Type are required for all nodes.  Operation canceled.");
                    currentTreeNode.Selected = true;
                    treeList1.SetFocusedNode(currentTreeNode);
                    return false;
                }



                if (isParentNode)
                {
                    XmlNode existingXmlNode = GetNodeElementByName(mtn.Name);
                    if (isNew)
                    {
                        if (existingXmlNode == null)
                        {
                            // Create new XmlNode <node> 
                            xmlNode = _rootXml.CreateElement("node");
                            _rootXml.DocumentElement.AppendChild(xmlNode);

                            // any children that have been added need to be connected to the node element rather than the child element.
                            while (currentXmlNode.ChildNodes.Count > 0)
                            {
                                xmlNode.AppendChild(currentXmlNode.ChildNodes[0]);
                            }
                        }
                        else
                        {
                            if (existingXmlNode.Attributes["name"].Value == name)
                            {
                                MessageBox.Show("A node with the name " + name +
                                                " already exists.  You must select a unique name.");
                                return false;
                            }
                            // any children that have been added need to be connected to the node element rather than the child element.
                            foreach (XmlNode node in existingXmlNode.ChildNodes)
                            {
                                xmlNode.AppendChild(node);
                            }

                        }

                    }
                    else // this is not a new node
                    {
                        xmlNode = existingXmlNode;
                    }

                    // update <node> element with all data because child. is just a pointer
                    if (!string.IsNullOrEmpty(name)) UpdateXmlAttribute(xmlNode, "name", name);
                    if (!string.IsNullOrEmpty(label)) UpdateXmlAttribute(xmlNode, "l", label);
                    if (!string.IsNullOrEmpty(type)) UpdateXmlAttribute(xmlNode, "type", type);
                    if (!string.IsNullOrEmpty(item)) UpdateXmlAttribute(xmlNode, "item", item);
                    if (!string.IsNullOrEmpty(comments)) UpdateXmlAttribute(xmlNode, "comment", comments);
                    if (!string.IsNullOrEmpty(disabled)) UpdateXmlAttribute(xmlNode, "disabled", disabled);
                    if (isNew) UpdateXmlAttribute(xmlNode, "creationDate", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    if (isNew) UpdateXmlAttribute(xmlNode, "createdBy", SS.I.UserName);

                    // Update <child/> node, name only.  Name is the only attribute that should be used on a <child/> node because it it just a pointer.
                    if (!string.IsNullOrEmpty(name)) UpdateXmlAttribute(currentXmlNode, "name", name);
                    if (isNew) UpdateXmlAttribute(currentXmlNode, "creationDate", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    if (isNew) UpdateXmlAttribute(currentXmlNode, "createdBy", SS.I.UserName);

                    // replace <child> with <node> on the treeListNode
                    currentTreeNode.Tag = xmlNode;
                }
                else
                {
                    XmlNode parentNode = GetNodeElementByName(name);
                    if (parentNode != null)
                    {
                        MessageBox.Show("A parent node with the name " + name +
                                                " already exists.  You must select a unique name.");
                        return false;
                    }

                    // <child> is the main element in this scenario
                    if (!string.IsNullOrEmpty(name)) UpdateXmlAttribute(currentXmlNode, "name", name);
                    if (!string.IsNullOrEmpty(label)) UpdateXmlAttribute(currentXmlNode, "l", label);
                    if (!string.IsNullOrEmpty(type)) UpdateXmlAttribute(currentXmlNode, "type", type);
                    if (!string.IsNullOrEmpty(item)) UpdateXmlAttribute(currentXmlNode, "item", item);
                    if (!string.IsNullOrEmpty(comments)) UpdateXmlAttribute(currentXmlNode, "comment", comments);
                    if (!string.IsNullOrEmpty(disabled)) UpdateXmlAttribute(currentXmlNode, "disabled", disabled);
                    if (isNew) UpdateXmlAttribute(currentXmlNode, "creationDate", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    if (isNew) UpdateXmlAttribute(currentXmlNode, "createdBy", SS.I.UserName);

                }
                currentTreeNode.Checked = false;
            }
            return true;
        }

        /// <summary>
        /// Updates an XMl Attribute.
        /// </summary>
        /// <param name="originalNode"></param>
        /// <param name="attributeName"></param>
        /// <param name="newValue"></param>
        private void UpdateXmlAttribute(XmlNode originalNode, string attributeName, string newValue)
        {
            string oldValue = "";
            bool attributeChanged = false;

            if (originalNode.Attributes != null)
            {
                XmlAttribute xmlAttribute = originalNode.Attributes[attributeName];

                if (xmlAttribute == null)
                {
                    XmlAttribute newXxmlAttribute = _rootXml.CreateAttribute(attributeName);
                    newXxmlAttribute.Value = newValue;
                    originalNode.Attributes.Append(newXxmlAttribute);
                }

                if (xmlAttribute != null && xmlAttribute.Value != newValue)
                {
                    oldValue = xmlAttribute.Value;
                    xmlAttribute.Value = newValue;
                    attributeChanged = true;
                }
            }

            if (attributeChanged)
            {
                XmlAttribute userAtt = _rootXml.CreateAttribute("user");
                userAtt.Value = SS.I.UserName;

                XmlNode changeNode = _rootXml.CreateElement("change");
                if (changeNode.Attributes != null)
                {
                    changeNode.Attributes.Append(userAtt);

                    XmlAttribute dateAtt = _rootXml.CreateAttribute("date");
                    dateAtt.Value = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                    changeNode.Attributes.Append(dateAtt);

                    XmlAttribute theAtt = _rootXml.CreateAttribute(attributeName);
                    theAtt.Value = oldValue;
                    changeNode.Attributes.Append(theAtt);
                }

                originalNode.PrependChild(changeNode);
            }
        }

        /// <summary>
        /// Parse root xml document into the TreeList.
        /// </summary>
        private void ParseContentsTreeEditorXml()
        {
            treeList1.BeginUnboundLoad();

            XmlNode metatreeNode = _rootXml.SelectSingleNode("metatree");
            if (metatreeNode == null)
            {
                throw new Exception("No initial element (\"metatree\") found");
            }

            //Build Root Tree Node
            var xmlRootNode = metatreeNode.SelectSingleNode("node[@type='root']");
            if (xmlRootNode != null && xmlRootNode.Attributes != null)
            {
                object[] objectArray = { xmlRootNode.Attributes["l"].Value, "", "", "", "", false };
                TreeListNode rootNode = treeList1.AppendNode(objectArray, null, xmlRootNode);
                var childNodes = metatreeNode.SelectNodes("node[@type='root']/child");
                if (childNodes != null && childNodes.Count > 0) rootNode.HasChildren = true;
            }
            treeList1.EndUnboundLoad();
            treeList1.Nodes.FirstNode.Expanded = true;
        }

        /// <summary>
        /// Operation class that collects all checked nodes.
        /// </summary>
        class GetCheckedNodesOperation : TreeListOperation
        {
            public readonly List<TreeListNode> CheckedNodes = new List<TreeListNode>();

            public override void Execute(TreeListNode node)
            {
                if (node.CheckState != CheckState.Unchecked)
                    CheckedNodes.Add(node);
            }
        }

        /// <summary>
        /// When a Cell has been edited, check the Node so we know it has been edited.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeList1_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            SetNodeCheckState(sender);
        }

        /// <summary>
        /// Raise AfterTreeUpdate event when a cell is being changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeList1_CellValueChanging(object sender, CellValueChangedEventArgs e)
        {
            AfterTreeUpdate();
        }

        /// <summary>
        /// The CheckNode method will set the CheckState to true.  When the tree is saved, we will find all
        /// nodes that have been checked and save them back to the Root XML object. The sender can be either
        /// a TreeList with a Selection or a TreeListNode.
        /// </summary>
        /// <param name="sender"></param>
        private void SetNodeCheckState(object sender)
        {
            var treeList = sender as TreeList;
            var node = sender as TreeListNode;
            if (treeList != null) node = treeList.Selection[0];
            if (node != null) node.Checked = true;
            IsDirty = true;
        }

        /// <summary>
        /// Append nodes to the parent. If the node has no Type attribute, this means it is a parent node that has children.
        /// </summary>
        /// <param name="treeList"></param>
        /// <param name="parentNode"></param>
        /// <param name="childNodes"></param>
        private void AppendNodes(TreeList treeList, TreeListNode parentNode, XmlNodeList childNodes)
        {
            IEnumerator ienum = childNodes.GetEnumerator();
            while (ienum != null && ienum.MoveNext())
            {
                XmlNode child = (XmlNode)ienum.Current;
                MetaTreeEditNode childMtn = GetMetaTreeEditNode(child);
                XmlNode newNode = _rootXml.SelectSingleNode("/metatree/node[translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')=\"" + childMtn.Name.ToLower() + "\"]");

                MetaTreeEditNode mtn = newNode != null ? GetMetaTreeEditNode(newNode) : childMtn;

                TreeListNode existingNode = treeList1.FindNodeByFieldValue("Name", mtn.Name);

                if (existingNode != null && existingNode.ParentNode == parentNode)
                {
                    continue;
                }

                try
                {
                    TreeListNode treeNode = treeList.AppendNode(mtn.ToObjectArray(), parentNode);
                    if (newNode != null)
                    {
                        treeNode.Tag = newNode;
                        var childNodes2 = _rootXml.SelectNodes("/metatree/node[translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='" + mtn.Name.ToLower() + "']/child");
                        if (childNodes2 != null && childNodes2.Count > 0) treeNode.HasChildren = true;
                    }
                    else
                    {
                        treeNode.Tag = child;
                    }
                }
                catch (Exception)
                {
                    throw new Exception("ERROR appending tree nodes. Cannot find node: " + childMtn.Name);
                }
            }
        }

        /// <summary>
        /// Helper method for getting the parent MetaTreeEditNode from a TreeListNode.
        /// </summary>
        /// <param name="treeListNode"></param>
        /// <returns></returns>
        public static MetaTreeEditNode GetParentMetaTreeEditNode(TreeListNode treeListNode)
        {
            XmlNode xmlNode = (XmlNode)treeListNode.Tag;
            if (xmlNode == null) return null;
            XmlNode parentXmlNode = xmlNode.ParentNode;
            if (parentXmlNode == null) return null;
            MetaTreeEditNode metaTreeEditNode = GetMetaTreeEditNode(parentXmlNode);
            return metaTreeEditNode;
        }

        /// <summary>
        /// Helper method fro getting a MetaTreeEditNode from a TreeListNode.
        /// </summary>
        /// <param name="treeListNode"></param>
        /// <returns></returns>
        public static MetaTreeEditNode GetMetaTreeEditNode(TreeListNode treeListNode)
        {
            XmlNode xmlNode = (XmlNode)treeListNode.Tag;
            MetaTreeEditNode metaTreeEditNode = GetMetaTreeEditNode(xmlNode);
            return metaTreeEditNode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static MetaTreeEditNode GetMetaTreeEditNode(XmlNode xmlNode)
        {
            MetaTreeEditNode mtn = new MetaTreeEditNode();
            if (xmlNode == null) return mtn;
            string altLabel = null;
            for (int i = 0; i < xmlNode.Attributes.Count; i++) // parse node attributes
            {
                XmlNode att = xmlNode.Attributes.Item(i);
                string attName = att.Name.ToUpper();
                string attValue = att.Value.ToUpper();

                if (attName == "TYPE")
                {
                    mtn.Type = MetaTreeNode.ParseTypeString(attValue);
                    if (mtn.Type == MetaTreeNodeType.Unknown)
                        throw new Exception("Unexpected Node Type: " + attValue);
                }
                else if (attName == "N" || attName == "NAME")
                {
                    mtn.Name = att.Value.Trim(); //since case is changed anyway
                }
                else if (attName == "L" || attName == "LABEL")
                {
                    mtn.Label = att.Value.Trim(); //to restore the original case
                }
                else if (attName == "DISABLED")
                {
                    mtn.Disabled = att.Value.ToLower() == "true";
                }
                else if (attName == "ITEM" || attName == "ITEMID" || attName == "ITEMSTRING" || attName == "ITEMIDSTRING" ||
                         attName == "TARGET")
                {
                    mtn.Target = Lex.IsUri(att.Value) ? att.Value.Trim() : attValue.Trim();
                }
                else if (attName == "C" || attName == "COMMENT")
                {
                    altLabel = att.Value; //to restore the original case
                    mtn.Comments = att.Value;
                }
                else if (attName == "AL" || attName == "ALABEL")
                {
                    altLabel = att.Value.Trim(); //to restore the original case
                }
            }
            if (String.IsNullOrEmpty(mtn.Label) && altLabel != null) mtn.Label = altLabel;
            return mtn;
        }

        /// <summary>
        /// Before expanding the current Node, go through each child node and populate the childnodes (grandchildren).
        /// This is required so the expand icon is displayed next to any node with a child.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeList1_BeforeExpand(object sender, BeforeExpandEventArgs e)
        {
            TreeListNode currentNode = e.Node;
            if ((currentNode.HasChildren && currentNode.Nodes.Count == 0) || (currentNode.HasChildren && btnResetTree.Enabled))
            {
                // if we got this far, before we rebuild the nodes we need to remove any that were added so we do not duplicate
                //while (currentNode.Nodes.Count>0) currentNode.Nodes.Remove(currentNode.Nodes.LastNode);

                XmlNode xmlNode = (XmlNode)currentNode.Tag;
                MetaTreeEditNode mtn = GetMetaTreeEditNode(xmlNode);

                var childNodes = _rootXml.SelectNodes("/metatree/node[translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='" + mtn.Name.ToLower() + "']/child");
                if (childNodes != null && childNodes.Count > 0)
                {
                    AppendNodes(treeList1, currentNode, childNodes);
                }
            }
        }

        private TreeListMultiSelection GetDragNodes(IDataObject data)
        {
            return data.GetData(typeof(TreeListMultiSelection)) as TreeListMultiSelection;
        }

        private void treeList1_CalcNodeDragImageIndex(object sender, CalcNodeDragImageIndexEventArgs e)
        {
            TreeList tl = sender as TreeList;
            if (GetDragDropEffect(tl, tl.FocusedNode) == DragDropEffects.None)
                e.ImageIndex = -1;  // no icon
            else
                e.ImageIndex = 1;  // the reorder icon (a curved arrow)
        }

        private DragDropEffects GetDragDropEffect(TreeList tl, TreeListNode dragNode)
        {
            Point p = tl.PointToClient(MousePosition);
            TreeListNode targetNode = tl.CalcHitInfo(p).Node;

            if (dragNode != null && targetNode != null
                && dragNode != targetNode)
                return DragDropEffects.Move;
            return DragDropEffects.None;
        }

        private void cutMenuItem_Click(object sender, EventArgs e)
        {
            CutCopy(TreeListClipBoardStateType.Cut);
        }

        private void copyMenuItem_Click(object sender, EventArgs e)
        {
            CutCopy(TreeListClipBoardStateType.Copy);
        }

        private void CutCopy(TreeListClipBoardStateType treeListClipBoardState)
        {
            if (!ValidSelection(treeList1.Selection)) return;
            if (!IsMoveAuthorized(treeList1.Selection, treeList1.Selection[0].ParentNode)) return;
            if (treeList1.AllNodesCount == 0) return;

            _xmlNodesClipboard.Clear();

            treeList1.LockReloadNodes();
            try
            {
                Hashtable sel = GetSelectedNodesHashTable(treeList1.Selection);
                IDictionaryEnumerator nodes = sel.GetEnumerator();
                nodes.Reset();
                while (nodes.MoveNext())
                {
                    if (!UpdateXmlFromTree())
                    {
                        _xmlNodesClipboard.Clear();
                        return;
                    }

                    TreeListNode treeListNode = (TreeListNode)nodes.Value;
                    XmlNode xmlNode = (XmlNode)treeListNode.Tag;
                    XmlAttribute xmlAttribute = (xmlNode.Attributes["name"]);
                    string childName = xmlAttribute != null ? xmlAttribute.Value : null;

                    TreeListNode treeListNodeParent = treeListNode.ParentNode;
                    XmlNode xmlParentNode = (XmlNode)treeListNodeParent.Tag;
                    XmlAttribute xmlParentAttribute = (xmlParentNode.Attributes["name"]);
                    string parentName = xmlParentAttribute != null ? xmlParentAttribute.Value : null;

                    XmlNode xmlChildNode = GetChildElementByName(parentName, childName);

                    // if we found a child than add it.  If we did not find a child this means the treenode was just added and the child has no attributes.
                    // We will pass it along and the Paste function will handle it.
                    _xmlNodesClipboard.Add(xmlChildNode ?? xmlNode);
                    if (treeListClipBoardState == TreeListClipBoardStateType.Cut)
                    {
                        treeListNode.Visible = false;
                        treeList1.DeleteNode(treeListNode);
                    }
                }
            }
            finally { treeList1.UnlockReloadNodes(); }
            _treeListClipBoardState = treeListClipBoardState;
        }

        /// <summary>
        /// Only allow nodes from the same parent to be cut, copied, or dragged. It does not make sense to try and
        /// cut or copy different levels.
        /// </summary>
        /// <param name="treeListMultiSelection"></param>
        /// <returns></returns>
        private bool ValidSelection(TreeListMultiSelection treeListMultiSelection)
        {
            if (btnResetTree.Enabled) // we are in search mode
            {
                foreach (TreeListNode treeListNode in treeListMultiSelection)
                {
                    if (treeListNode.ParentNode != null) continue;
                    MessageBox.Show("Before deleting a node while in search mode, press the Show Parents button so you know which child you are deleting.");
                    return false;
                }
            }

            if (treeListMultiSelection[0].ParentNode == null)
            {
                MessageBox.Show("The root node cannot be modified or moved.");
                return false;
            }

            TreeListNode treelisteNodeParent = treeListMultiSelection[0].ParentNode;
            foreach (TreeListNode node in treeListMultiSelection)
            {
                if (node.ParentNode != treelisteNodeParent || node.ParentNode == null)
                {
                    MessageBox.Show("You can only select nodes that have the same parent.");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Paste items from custom clipboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pasteMenuItem_Click(object sender, EventArgs e)
        {
            if (treeList1.Selection.Count != 1)
            {
                MessageBox.Show("A single node must be selected when pasting.");
                return;
            }

            if (btnShowParents.Enabled)
            {
                MessageBox.Show("You must click on the Expand Parents before pasting.");
                return;
            }

            TreeListNode focusedNode = treeList1.FocusedNode;
            XmlNode xmlFocusedNode = focusedNode.Tag as XmlNode;

            focusedNode.Expanded = true;

            if (!IsMoveAuthorized(_xmlNodesClipboard, focusedNode)) return;

            foreach (XmlNode node in _xmlNodesClipboard)
            {
                MetaTreeEditNode destinationMtn = GetMetaTreeEditNode(focusedNode);
                TreeListNode newTreeListNodeNode;

                XmlNode xmlNodeChild = _treeListClipBoardState == TreeListClipBoardStateType.Copy ? node.Clone() : node;

                XmlNode xmlClipboardNode = node;
                MetaTreeEditNode clipboardMtn = GetMetaTreeEditNode(xmlClipboardNode);

                //bool isLeafNode = clipboardMtn.IsLeafType;

                if (clipboardMtn.Type == MetaTreeNodeType.Unknown)
                {
                    XmlNode nodeElement = GetNodeElementByName(clipboardMtn.Name);
                    clipboardMtn = GetMetaTreeEditNode(nodeElement);
                }

                if (destinationMtn.IsLeafType)
                {
                    newTreeListNodeNode = treeList1.AppendNode(null, focusedNode.ParentNode);
                    treeList1.SetNodeIndex(newTreeListNodeNode, treeList1.GetNodeIndex(focusedNode) + 1);
                    xmlFocusedNode.ParentNode.InsertAfter(xmlNodeChild, xmlFocusedNode);
                }
                else
                {
                    newTreeListNodeNode = treeList1.AppendNode(null, focusedNode);
                    treeList1.SetNodeIndex(newTreeListNodeNode, focusedNode.Nodes.Count);
                    xmlFocusedNode.InsertAfter(xmlNodeChild, xmlFocusedNode.LastChild);
                }

                // Give new TreeListNode the same column values as the node from the clipboard
                newTreeListNodeNode[ColName] = clipboardMtn.Name ?? "";
                newTreeListNodeNode[ColType] = clipboardMtn.Type;
                newTreeListNodeNode[ColLabel] = clipboardMtn.Label ?? "";
                newTreeListNodeNode[ColItem] = clipboardMtn.Target ?? "";
                newTreeListNodeNode[ColDisabled] = clipboardMtn.Disabled;
                newTreeListNodeNode[ColComments] = clipboardMtn.Comments ?? "";

                newTreeListNodeNode.Tag = xmlNodeChild;

                SetNodeCheckState(newTreeListNodeNode);
                treeList1.FocusedNode = newTreeListNodeNode;
                FieldInfo field = typeof(TreeList).GetField("isFocusedNodeDataModified", BindingFlags.Instance | BindingFlags.NonPublic);
                field.SetValue(treeList1, true);

                if (!clipboardMtn.IsLeafType)
                {
                    XmlNode nodeElement = GetNodeElementByName(clipboardMtn.Name);
                    if (nodeElement != null) newTreeListNodeNode.HasChildren = nodeElement.HasChildNodes;
                    newTreeListNodeNode.Expanded = true;
                }

                treeList1.EndUnboundLoad();
            }
            _xmlNodesClipboard.Clear();
            AfterTreeUpdate();
        }

        /// <summary>
        /// Delete nodes from the tree and raie the AfterTreeUpdate event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteMenuItem_Click(object sender, EventArgs e)
        {
            if (!ValidSelection(treeList1.Selection)) return;
            if (treeList1.Selection != null) DeleteNodes(treeList1.Selection);
            AfterTreeUpdate();
        }

        /// <summary>
        /// Delete the selected nodes.
        /// </summary>
        /// <param name="treeListMultiSelection"></param>
        private void DeleteNodes(TreeListMultiSelection treeListMultiSelection)
        {
            if (treeList1.AllNodesCount == 0) return;
            treeList1.LockReloadNodes();
            try
            {
                Hashtable sel = GetSelectedNodesHashTable(treeListMultiSelection);
                IDictionaryEnumerator nodes = sel.GetEnumerator();
                nodes.Reset();
                while (nodes.MoveNext())
                {
                    TreeListNode treeListNode = (TreeListNode)nodes.Value;
                    XmlNode xmlNode = (XmlNode)treeListNode.Tag;
                    DeleteXmlNode(xmlNode, true);
                    treeList1.DeleteNode(treeListNode);
                }
            }
            finally
            {
                treeList1.UnlockReloadNodes();
                _nodesForDeleting = null;
            }
        }

        private void DeleteXmlNode(XmlNode xmlNode, bool softDelete)
        {
            if (xmlNode.Attributes.Count == 0) // new child added but no attributes yet
            {
                xmlNode.ParentNode.RemoveChild(xmlNode);
                return;
            }

            // remove <child> node that is just pointers to <node>
            if (xmlNode.Name == "node")
            {
                MetaTreeEditNode mtn = GetMetaTreeEditNode(xmlNode);
                XmlNode childXmlNode = GetChildElementByName(mtn.Name);
                XmlNode node = childXmlNode.ParentNode.SelectSingleNode("child[@name='" + mtn.Name + "']");
                if (softDelete)
                {
                    MarkNodeDeleted(node);
                }
                else
                {
                    node.ParentNode.RemoveChild(node);
                }
            }
            else if (xmlNode.Name == "child")
            {
                if (softDelete)
                {
                    MarkNodeDeleted(xmlNode);
                }
                else
                {
                    xmlNode.ParentNode.RemoveChild(xmlNode);
                }
            }
        }

        /// <summary>
        /// This performs a soft delete by prepending "delete" to the front of the node name.
        /// Therefore this node will not be seen in Mobius.
        /// </summary>
        /// <param name="xmlNode"></param>
        private void MarkNodeDeleted(XmlNode xmlNode)
        {
            XmlNode newNode = _rootXml.CreateElement("deleted" + xmlNode.Name);
            newNode.InnerXml = xmlNode.InnerXml;
            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                XmlAttribute newAttribute = _rootXml.CreateAttribute(xmlAttribute.Name);
                newAttribute.Value = xmlAttribute.Value;
                newNode.Attributes.Append(newAttribute);
            }

            XmlAttribute xmlDateAttributeAttribute = _rootXml.CreateAttribute("deletionDate");
            xmlDateAttributeAttribute.Value = DateTime.Now.ToString(CultureInfo.InvariantCulture);

            XmlAttribute xmlDeletedAttributeBy = _rootXml.CreateAttribute("deletedBy");
            xmlDeletedAttributeBy.Value = SS.I.UserName;

            newNode.Attributes.Append(xmlDateAttributeAttribute);
            newNode.Attributes.Append(xmlDeletedAttributeBy);

            xmlNode.ParentNode.InsertBefore(newNode, xmlNode);
            xmlNode.ParentNode.RemoveChild(xmlNode);
        }

        /// <summary>
        /// Helper method for deleting nodes.
        /// </summary>
        /// <param name="treeListMultiSelection"></param>
        /// <returns></returns>
        private Hashtable GetSelectedNodesHashTable(TreeListMultiSelection treeListMultiSelection)
        {
            _nodesForDeleting = new Hashtable();
            IEnumerator sel = treeListMultiSelection.GetEnumerator();
            sel.Reset();
            while (sel.MoveNext())
            {
                AddSelectedNode((TreeListNode)sel.Current);
            }
            return _nodesForDeleting;
        }

        /// <summary>
        /// Helper method for deleting nodes.
        /// </summary>
        /// <param name="node"></param>
        private void AddSelectedNode(TreeListNode node)
        {
            TreeListNode pnode = GetSelectedParent(node);
            _nodesForDeleting[pnode] = pnode;
        }

        /// <summary>
        /// Helper method for deleting nodes.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private TreeListNode GetSelectedParent(TreeListNode node)
        {
            TreeListNode pnode = node;
            while (pnode.ParentNode != null && pnode.ParentNode.Selected)
            {
                pnode = pnode.ParentNode;
            }
            return pnode;
        }

        /// <summary>
        /// Validation event for a node.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void treeList1_ValidateNode(object sender, ValidateNodeEventArgs e)
        //{
        //    TreeListNode currentTreeNode = e.Node;
        //    e.Valid = (ValidateNode(currentTreeNode));
        //    if (!e.Valid) e.ErrorText = "Validation Failed. ";
        //}

        /// <summary>
        /// Method for validating a new or updated node.
        /// </summary>
        /// <param name="currentTreeNode"></param>
        /// <returns></returns>
        bool ValidateNode(TreeListNode currentTreeNode)
        {

            bool valid = true;
            const string errorTextShort = "Required Field";
            const string errorTextDuplicate = "Duplicate Name";

            string type = currentTreeNode[ColType].ToString();
            if (type == "Contents") return true;

            string name = currentTreeNode[ColName].ToString();
            string label = currentTreeNode[ColLabel].ToString();

            XmlNode currentXmlNode = (XmlNode)currentTreeNode.Tag;

            MetaTreeNodeType metaTreeNodeType = MetaTreeNode.ParseTypeString(name);

            if (currentXmlNode.Attributes.Count == 0 &&
                !MetaTreeNode.IsLeafNodeType(metaTreeNodeType))

            {
                // new node
                XmlNode xmlNode = _rootXml.SelectSingleNode("/metatree/node[@name='" + name + "']");
                if (xmlNode == null) _rootXml.SelectSingleNode("/metatree/child[@name='" + name + "']");
                if (xmlNode != null)
                {
                    treeList1.SetColumnError(ColName, errorTextDuplicate);
                    valid = false;
                }
            }

            if (string.IsNullOrEmpty(name))
            {
                treeList1.SetColumnError(ColName, errorTextShort);
                valid = false;

            }
            else if (string.IsNullOrEmpty(label))
            {
                treeList1.SetColumnError(ColLabel, errorTextShort);
                valid = false;
            }
            return valid;
        }

        /// <summary>
        /// Method for handling the Search button clicked event.  Will invoke the SearchAndAppendNodes method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchAll_Properties_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            SearchTree(btnSearchAll.Text);
        }

        private void SearchTree(string searchText)
        {
            Cursor.Current = Cursors.WaitCursor;

            UpdateXmlFromTree();

            treeList1.Nodes.Clear();
            //treeList1.OptionsBehavior.DragNodes = false;
            treeList1.OptionsDragAndDrop.DragNodesMode = DragNodesMode.None;

            bool nodesFound = SearchAndAppendNodes(searchText);
            if (nodesFound)
            {
                btnShowParents.Enabled =
                btnResetTree.Enabled = true;
            }
            else
            {
                MessageBox.Show("No data returned.");
                btnResetTree.Enabled = true;
            }

            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// Search and Append nodes to a new tree.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool SearchAndAppendNodes(string text)
        {
            bool nodesFound = false;
            StringBuilder stringBuilder = new StringBuilder();

            string[] words = text.Split(' ');
            stringBuilder.Append("//child");
            foreach (string word in words)
            {
                // check if the name or the label contain any words in the search
                stringBuilder.Append("[contains(translate(@l,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'");
                stringBuilder.Append(word.ToLower());
                stringBuilder.Append("') or contains(translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'");
                stringBuilder.Append(word.ToLower());
                stringBuilder.Append("')]");
            }

            String xPathString = stringBuilder.ToString();
            XmlNodeList xmlNodes = _rootXml.SelectNodes(xPathString);

            if (xmlNodes != null && xmlNodes.Count > 0) nodesFound = true;

            if (xmlNodes != null && xmlNodes.Count > 1000)
            {
                MessageBox.Show("Search returns over 1,000 results.  Please adjust your query to be more specific.");
                return false;
            }

            List<XmlNode> parentNodes = new List<XmlNode>();

            foreach (XmlNode xmlNode in xmlNodes)
            {
                MetaTreeEditNode currentMtn = GetMetaTreeEditNode(xmlNode);
                XmlNode parentNode =
                    _rootXml.SelectSingleNode(
                    "//node[translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='" +
                    currentMtn.Name.ToLower() + "']");

                // collect a list of these nodes for later
                parentNodes.Add(parentNode);

                // if we found a <node>, let's grab the attributes for the tree.  However we will leave the child node
                // as the Tag so we can find the related parent.

                MetaTreeEditNode mtn = GetMetaTreeEditNode(parentNode ?? xmlNode);
                TreeListNode newTreeListNode = treeList1.Nodes.Add(mtn.ToObjectArray());

                XmlAttribute xmlTypeAttribute = _rootXml.CreateAttribute("type");
                xmlTypeAttribute.Value = mtn.Type.ToString();
                xmlNode.Attributes.Append(xmlTypeAttribute);

                newTreeListNode.Tag = xmlNode;
                if (!mtn.IsLeafType)
                {
                    newTreeListNode.HasChildren = parentNode.HasChildNodes;
                }
            }

            // We now have to search <node> elements label attribute.  We already searched <child> name attributes and matched  
            // them to their associated node, so we do not have to search <node> elements by name.
            stringBuilder = new StringBuilder();
            stringBuilder.Append("//node");
            foreach (string word in words)
            {
                // check if the name or the label contain any words in the search
                stringBuilder.Append("[contains(translate(@l,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'");
                stringBuilder.Append(word.ToLower() + "')]");
            }

            xPathString = stringBuilder.ToString();
            xmlNodes = _rootXml.SelectNodes(xPathString);
            if (xmlNodes != null && xmlNodes.Count > 0) nodesFound = true;

            List<XmlNode> newNodes = new List<XmlNode>();

            // discard any we have already found
            foreach (XmlNode xmlNode in xmlNodes)
            {
                if (!parentNodes.Contains(xmlNode)) newNodes.Add(xmlNode);
            }

            // we now have to get the children for these nodes
            stringBuilder = new StringBuilder();
            stringBuilder.Append("//child");

            foreach (XmlNode newNode in newNodes)
            {
                MetaTreeEditNode currentMtn = GetMetaTreeEditNode(newNode);
                XmlNode childNode =
                    _rootXml.SelectSingleNode(
                    "//child[translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='" +
                    currentMtn.Name.ToLower() + "']");

                if (childNode != null)
                {
                    TreeListNode newTreeListNode = treeList1.Nodes.Add(currentMtn.ToObjectArray());
                    newTreeListNode.Tag = childNode;

                    if (!currentMtn.IsLeafType)
                    {
                        newTreeListNode.HasChildren = newNode.HasChildNodes;
                    }

                    XmlAttribute xmlTypeAttribute = _rootXml.CreateAttribute("type");
                    xmlTypeAttribute.Value = currentMtn.Type.ToString();
                    childNode.Attributes.Append(xmlTypeAttribute);
                }
            }
            return nodesFound;
        }


        /// <summary>
        /// After a search is performed, the user may select one or more rows from the search results.
        /// When they press the "Show Parents" button, we reset the TreeList with the data from root.xml
        /// We now want to show the selected nodes and their full path in the tree.  This method appends
        /// the first slected node, then traverses up the parents and expands each parent showing the
        /// full path for the selected search results.
        /// </summary>
        /// <param name="xmlNode">The XmlNOde to be expanded</param>
        /// <returns>Returns the leaf treeListNode</returns>
        public TreeListNode ExpandAllParents(XmlNode xmlNode)
        {
            XmlNode currentNode = null;
            TreeListNode currentTreeListNode = null;
            TreeListNode newTreeListNode = null;

            MetaTreeEditNode currentMtn = GetMetaTreeEditNode(xmlNode);
            if (xmlNode.Name == "child") currentNode = GetNodeElementByName(currentMtn.Name);
            if (currentNode == null) currentNode = xmlNode;

            XmlNode parentXmlNode = null;

            if (currentNode.Name == "child")
            {
                parentXmlNode = currentNode.ParentNode;
            }
            else
            {
                XmlNode childXmlNode = GetChildElementByName(currentMtn.Name);

                if (childXmlNode == null)
                {
                    AddToOrphansFolder(xmlNode);
                }
                else
                {
                    parentXmlNode = childXmlNode.ParentNode;
                }
            }

            TreeListNode treeListNode = GetTreeNodeByTag(currentNode);

            if (parentXmlNode != null && parentXmlNode.Attributes["name"].Value.ToLower() != "root" && treeListNode == null)
                ExpandAllParents(parentXmlNode);

            if (treeListNode == null)
            {
                treeListNode = GetTreeNodeByTag(currentNode);
                treeListNode.HasChildren = currentNode.HasChildNodes;
                treeListNode.Expanded = true;
            }
            else
            {
                treeListNode.HasChildren = currentNode.HasChildNodes;
                treeListNode.Expanded = true;
            }
            return treeListNode;
        }

        private void AddToOrphansFolder(XmlNode currentXmlNode)
        {
            MetaTreeEditNode currentMtn = GetMetaTreeEditNode(currentXmlNode);
            TreeListColumn columnName = treeList1.Columns["Label"];
            TreeListNode orphanTreeListNode = null;

            foreach (TreeListNode node in treeList1.Nodes)
            {
                if (node.GetDisplayText(columnName) == "Orphans") // Orphan node already exists
                {
                    orphanTreeListNode = node;
                    break;
                }
            }

            if (orphanTreeListNode == null)
            {
                object[] childrenArray =
                            {
                                MetaTreeNodeType.SystemFolder.ToString(), "Orphans",
                                "Orphan nodes that are not connected to the main tree"
                            };
                orphanTreeListNode = treeList1.Nodes.Add(childrenArray);
            }

            TreeListNode currentTreeListNode = treeList1.Nodes.Add(currentMtn.ToObjectArray()); // create current tree node
            currentTreeListNode.Tag = currentXmlNode;
            orphanTreeListNode.Nodes.Add(currentTreeListNode);                                  // add current node to orphan tree nodes
            treeList1.MoveNode(currentTreeListNode, orphanTreeListNode);
            orphanTreeListNode.ExpandAll();
        }

        /// <summary>
        /// Helper method fines the node element by the attribute "name".
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public XmlNode GetNodeElementByName(string name)
        {
            XmlNode xmlNode =
                        _rootXml.SelectSingleNode(
                            "/metatree/node[translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='" +
                            name.ToLower() + "']");
            return xmlNode;
        }

        /// <summary>
        /// Helper method fines the child element by the attribute "name".
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public XmlNode GetChildElementByName(string name)
        {
            XmlNode xmlNode =
                        _rootXml.SelectSingleNode("//child[translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='" + name.ToLower() + "']");
            return xmlNode;
        }

        public XmlNode GetChildElementByName(string parentName, string childName)
        {
            if (childName == null || parentName == null) return null;

            string xPath = "/metatree/node[translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='" + parentName.ToLower() + "']" +
                                      "//child[translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='" + childName.ToLower() + "']";

            XmlNode xmlNode =
                        _rootXml.SelectSingleNode(xPath);

            return xmlNode;
        }

        /// <summary>
        /// Helper methods that will find a TreeNode for a given XmlNode.  This method uses three attributes to
        /// determine a match:  Name, Label and Type.
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public TreeListNode GetTreeNodeByTag(XmlNode xmlNode)
        {
            FindNodesOperation2 op = new FindNodesOperation2(xmlNode);
            treeList1.NodesIterator.DoOperation(op);
            return op.FoundNode;
        }

        /// <summary>
        /// This methos shows all the parents for a given node found in a search.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnShowParents_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            //treeList1.OptionsBehavior.DragNodes = true;
            treeList1.OptionsDragAndDrop.DragNodesMode = DragNodesMode.Multiple;

            // Save the current list of selected nodes so we can add them and expand them later
            List<XmlNode> xmlNodeList = new List<XmlNode>();
            TreeListMultiSelection selectedNodes = treeList1.Selection;

            if (selectedNodes == null || selectedNodes.Count == 0)
            {
                MessageBox.Show("Please select a node that you would like to see the parents for.");
                return;
            }

            foreach (TreeListNode treeListNode in selectedNodes)
            {
                XmlNode xmlNode = (XmlNode)treeListNode.Tag;
                xmlNodeList.Add(xmlNode);
            }

            // reset to the full tree
            treeList1.Nodes.Clear();
            ParseContentsTreeEditorXml();

            Dictionary<XmlNode, TreeListNode> d = new Dictionary<XmlNode, TreeListNode>();

            // expand each node we have saved and show all parents.  Save the xmlNode and
            // and the associated leafnode.
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                TreeListNode leafTreelistNode = ExpandAllParents(xmlNode);
                if (leafTreelistNode != null)
                {
                    d.Add(xmlNode, leafTreelistNode);
                    treeList1.SetFocusedNode(leafTreelistNode);
                    leafTreelistNode.Selected = true;
                }
            }

            // Now that the tree has been built successfully we need to replace all leaf child
            // nodes that are pointers to the actual node element. This must be done or cut/paste,
            // drag/drop and edit will not work properly.
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                try
                {
                    TreeListNode treeListNode = d[xmlNode];
                    XmlNode childXmlNode = (XmlNode)treeListNode.Tag;
                    MetaTreeEditNode mtn = GetMetaTreeEditNode(childXmlNode);
                    XmlNode nodeXmlNode = GetNodeElementByName(mtn.Name);
                    if (nodeXmlNode != null) treeListNode.Tag = nodeXmlNode;
                }
                catch (Exception)
                {
                    //this is in case we have an orphan node that did not get loaded into the dictionary
                }
            }
            btnShowParents.Enabled =
            btnResetTree.Enabled = false;

            treeList1.Nodes.FirstNode.Selected = false; // The root node was the last node built, but we do not want it to be selected.

            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// This method is used for displaying the proper icon for a given TreeListNode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeList1_CustomDrawNodeImages(object sender, CustomDrawNodeImagesEventArgs e)
        {
            TreeListNode n = e.Node;
            XmlNode node = (XmlNode)n.Tag;
            MetaTreeEditNode mtn = GetMetaTreeEditNode(node);
            if (mtn == null) return;

            // if the Name is blank, this is a new entry that does not yet have a node attached.
            // Pull the MetaTreeNodeType from the ColType in the treelist.
            //if (mtn.Name == "")
            //{
            mtn = new MetaTreeEditNode();
            object x = n[ColType];
            if (x == null) return;
            string type = n[ColType].ToString();
            mtn.Type = MetaTreeNode.ParseTypeString(type);
            //}

            int ii = mtn.GetImageIndex();

            if ((ii == (int)Bitmaps16x16Enum.Folder || mtn.Type == MetaTreeNodeType.UserFolder) && n.Expanded)
                ii = (int)Bitmaps16x16Enum.FolderOpen;
            e.SelectImageIndex = ii;
        }

        /// <summary>
        /// Rasie the AfterTreeUpdate event after a DragDrop has occurred.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeList1_DragDrop(object sender, DragEventArgs e)
        {
            DXDragEventArgs args = treeList1.GetDXDragEventArgs(e);

            TreeListMultiSelection selectedNodes = treeList1.Selection;
            TreeListNode destNode = args.TargetNode; //treeList1.ViewInfo.GetHitTest(treeList1.PointToClient(new Point(e.X, e.Y))).Node;
            
            if (selectedNodes==null || destNode == null) return;

            // the method below works but does not change the UI, inly the data
            //selectedNodes = TreeListHelper.RestoreNodesSequence(selectedNodes,treeList1);

            DragInsertPosition position = args.DragInsertPosition;

            if (destNode.ParentNode == null)
            {
                MessageBox.Show("You are not allowed to move nodes into the root Contents Node");
                return;
            }

            if (ValidSelection(selectedNodes))
            {
                XmlNode xmlTargetNode = destNode.Tag as XmlNode;

                foreach (TreeListNode node in selectedNodes)
                {
                    XmlNode xmlNode = node.Tag as XmlNode;

                    // for the element being moved we do not want to move the <node> element,
                    // so we need to find the associated <child> element
                    if (xmlNode.Name == "node")
                    {
                        string name = xmlNode.Attributes["name"].Value;
                        TreeListNode parentTreeListNode = node.ParentNode;
                        XmlNode parentXmlNode = (XmlNode)parentTreeListNode.Tag;
                        xmlNode = parentXmlNode.SelectSingleNode("child[@name='" + name + "']");
                    }

                    if (xmlTargetNode != null && xmlNode != null)
                    {
                        if (position == DragInsertPosition.AsChild)
                        {
                            MetaTreeEditNode mtn = ContentsTreeEditorControl2.GetMetaTreeEditNode(xmlTargetNode);
                            if (MetaTreeNode.IsLeafNodeType(mtn.Type))
                            {
                                MessageBox.Show("The target node is a " + mtn.Type + ". This node cannot contain children.");
                                break;
                            }
                            if (ContentsTreeEditorControl2.IsMoveAuthorized(selectedNodes, destNode))
                            {
                                xmlTargetNode.PrependChild(xmlNode);
                                continue;
                            }
                        }

                        // If dragging and dropping as sibblings, select the <child> node rather than the <node>. 
                        else if (xmlTargetNode.Name == "node")
                        {
                            string name = xmlTargetNode.Attributes["name"].Value;
                            TreeListNode parentTreeListNode = destNode.ParentNode;
                            XmlNode parentXmlNode = (XmlNode)parentTreeListNode.Tag;
                            xmlTargetNode = parentXmlNode.SelectSingleNode("child[@name='" + name + "']");
                        }

                        // drag before target node
                        if (position == DragInsertPosition.Before &&
                            ContentsTreeEditorControl2.IsMoveAuthorized(selectedNodes, destNode.ParentNode))
                        {
                            xmlTargetNode.ParentNode.InsertBefore(xmlNode, xmlTargetNode);
                            continue;
                        }

                        // drag after target node
                        if (position == DragInsertPosition.After &&
                        ContentsTreeEditorControl2.IsMoveAuthorized(selectedNodes, destNode.ParentNode))
                        {
                            xmlTargetNode.ParentNode.InsertAfter(xmlNode, xmlTargetNode);
                        }
                    }
                }
            }
            UpdateXmlFromTree();
        }

        /// <summary>
        /// Handle Validation Error.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeList1_InvalidValueException(object sender, InvalidValueExceptionEventArgs e)
        {
            e.ExceptionMode = ExceptionMode.DisplayError;
            e.WindowCaption = "Input Error";
        }

        /// <summary>
        /// The Insert menu is diabled by default.  If ythe focused node is a leaf, and the user
        /// CanEditProject, and the parent is a Project, then enable the Insert Menu.
        /// Or, enable the menu if the user CanEditContentsTree.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contentsTreeEditorMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TreeListNode currentNode = treeList1.FocusedNode;
            MetaTreeEditNode mtn = GetMetaTreeEditNode(currentNode);

            bool isProject = IsProjectInAncestory(treeList1.FocusedNode);

            cutMenuItem.Enabled =
            DeleteMenuItem.Enabled =
            copyMenuItem.Enabled =
            InsertMenuItem.Enabled = (
                                      !btnShowParents.Enabled &&
                                      isProject &&
                                      SS.I.UserInfo.Privileges.CanEditProjects ||
                                      !btnShowParents.Enabled &&
                                      SS.I.UserInfo.Privileges.CanEditContentsTree);

            if (mtn.Name == "root")
            {
                cutMenuItem.Enabled =
                    DeleteMenuItem.Enabled =
                        copyMenuItem.Enabled = false;
            }

            // In order to paste we must have either of the following: 
            //  * CanEditProject privileges, destination node must be a Project or descendant of a project and a non-leaf node
            //  * CanEditContentsTree privileges and and a non-leaf node
            // AND we must have something in the clipboard
            pasteMenuItem.Enabled = (
                                     isProject &&
                                     SS.I.UserInfo.Privileges.CanEditProjects &&
                                     _xmlNodesClipboard.Count > 0)
                                     ||
                                     (SS.I.UserInfo.Privileges.CanEditContentsTree &&
                                     _xmlNodesClipboard.Count > 0);
        }

        /// <summary>
        /// Insert a new node into the tree.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void insertNodeMenuItem_Click(object sender, EventArgs e)
        {
            string type = sender.ToString();
            TreeListNode parentNode = treeList1.FocusedNode;

            if (!ValidateNode(parentNode))
            {
                MessageBox.Show("The parent node is invalid.  The parent must have a valid Name and Label before adding a child.");
                return;
            }

            MetaTreeEditNode parentMtn = GetMetaTreeEditNode(parentNode);
            XmlNode parentXmlNode = (XmlNode)parentNode.Tag;

            string nodeType = parentNode[ColType].ToString();    // in case this element was just added, get the type from the column
            parentMtn.Type = MetaTreeNode.ParseTypeString(nodeType);

            // Append an empty child node after the current node
            treeList1.BeginUnboundLoad();

            XmlNode xmlNodeChild = _rootXml.CreateElement("child");
            TreeListNode newNode;
            //parentNode.Expanded = true;

            if (MetaTreeNode.IsLeafNodeType(parentMtn.Type))
            {
                // if the parent is a leafnode then we are adding a leaf node right underneath
                newNode = treeList1.AppendNode(new object[] { type, "", "", "", "", false }, parentNode.ParentNode);
                treeList1.SetNodeIndex(newNode, treeList1.GetNodeIndex(parentNode) + 1);
                newNode.Tag = xmlNodeChild;
                parentXmlNode.ParentNode.InsertAfter(xmlNodeChild, parentXmlNode);
            }
            else
            {
                newNode = treeList1.AppendNode(new object[] { type, "", "", "", "", false }, parentNode);
                treeList1.SetNodeIndex(newNode, parentNode.Nodes.Count);
                newNode.Tag = xmlNodeChild;
                parentXmlNode.AppendChild(xmlNodeChild);
            }

            SetNodeCheckState(newNode);
            treeList1.FocusedNode = newNode;
            FieldInfo field = typeof(TreeList).GetField("isFocusedNodeDataModified", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(treeList1, true);
            treeList1.EndUnboundLoad();

            //parentNode.Expanded = true;

            if (type == MetaTreeNodeType.MetaTable.ToString())
            {
                treeList1.FocusedColumn = treeList1.Columns[ColName.FieldName];
                RepositoryItemButtonEdit buttonEdit = (RepositoryItemButtonEdit)ColName.ColumnEdit;
                buttonEdit.Buttons[0].Enabled = true;
            }
        }

        /// <summary>
        /// Check the user security to determine if the move is authorized.
        /// </summary>
        /// <param name="sourceNodes"></param>
        /// <param name="destParentNode"></param>
        /// <returns></returns>
        public static bool IsMoveAuthorized(TreeListMultiSelection sourceNodes, TreeListNode destParentNode)
        {
            // If user is an Admin they can do anything
            if (SS.I.UserInfo.Privileges.CanEditContentsTree) return true;

            // If the user does not have CanEditProjects than they cannot drag/drop  items at all
            if (!SS.I.UserInfo.Privileges.CanEditProjects)
            {
                MessageBox.Show("You are only authorized to move items to and from Projects.");
                return false;
            }

            // Only allow items to be moved to and from a Project (anywhere in the ancestory)
            if (!IsProjectInAncestory(destParentNode) || !IsProjectInAncestory(sourceNodes[0]))
            {
                MessageBox.Show("You are only authorized to move items to and from Projects.");
                return false;
            }

            // If user CanEditProjects we will not let them move non leaf items (Projects, Folders, Databases)
            foreach (TreeListNode node in sourceNodes)
            {
                XmlNode xmlNode = (XmlNode)node.Tag;
                MetaTreeEditNode mtn = GetMetaTreeEditNode(xmlNode);
                if (!mtn.IsLeafType) return false;
            }

            return true;
        }

        public bool IsMoveAuthorized(List<XmlNode> sourceNodes, TreeListNode destParentNode)
        {
            // If user is an Admin they can do anything
            if (SS.I.UserInfo.Privileges.CanEditContentsTree) return true;

            // If the user does not have CanEditProjects than they cannot drag/drop  items at all
            if (!SS.I.UserInfo.Privileges.CanEditProjects)
            {
                MessageBox.Show("You are only authorized to move items to and from Projects.");
                return false;
            }

            // Only allow items to be moved to and from a Project (anywhere in the ancestory)
            if (!IsProjectInAncestory(destParentNode) || !IsProjectInAncestory(sourceNodes[0]))
            {
                MessageBox.Show("You are only authorized to move items to and from Projects.");
                return false;
            }

            // If user CanEditProjects we will not let them move non leaf items (Projects, Folders, Databases)
            foreach (XmlNode xmlNode in sourceNodes)
            {
                MetaTreeEditNode mtn = GetMetaTreeEditNode(xmlNode);
                if (!mtn.IsLeafType) return false;
            }

            return true;
        }

        /// <summary>
        /// Helper method that looks up the path of a node to see if there is a project in the ancestory.
        /// </summary>
        /// <param name="treeListNode"></param>
        /// <returns></returns>
        public static bool IsProjectInAncestory(TreeListNode treeListNode)
        {
            while (true)
            {
                MetaTreeEditNode metaTreeEdit = GetMetaTreeEditNode(treeListNode);
                if (metaTreeEdit.Type == MetaTreeNodeType.Project) return true;
                treeListNode = treeListNode.ParentNode;
                if (treeListNode == null) return false;
            }
        }

        /// <summary>
        /// Helper method that looks up the path of a node to see if there is a project in the ancestory.
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public bool IsProjectInAncestory(XmlNode xmlNode)
        {
            while (true)
            {
                MetaTreeEditNode metaTreeEdit = GetMetaTreeEditNode(xmlNode);
                if (metaTreeEdit.Type == MetaTreeNodeType.Project) return true;
                if (metaTreeEdit.IsLeafType)
                {
                    xmlNode = xmlNode.ParentNode;
                }
                else
                {
                    xmlNode = GetChildElementByName(metaTreeEdit.Name);
                    xmlNode = xmlNode.ParentNode;
                }
                if (xmlNode == null) return false;
            }
        }

        /// <summary>
        /// When a node is focused, check to see if it is a leaf node. The name for a LeafNode should not be editable.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeList1_AfterFocusNode(object sender, NodeEventArgs e)
        {
            TreeListNode treeListNode = e.Node;
            MetaTreeEditNode mtn = GetMetaTreeEditNode(treeListNode);

            // If this is the root node disable all columns
            ColDisabled.OptionsColumn.AllowEdit =
                ColComments.OptionsColumn.AllowEdit =
                    ColItem.OptionsColumn.AllowEdit =
                        ColLabel.OptionsColumn.AllowEdit =
                            ColName.OptionsColumn.AllowEdit =
                                (mtn.Name != "root");

            if (treeListNode != null) ColName.OptionsColumn.AllowEdit = MetaTreeNode.IsLeafNodeType(mtn.Type);

            RepositoryItemButtonEdit buttonEdit = (RepositoryItemButtonEdit)ColName.ColumnEdit;
            buttonEdit.Buttons[0].Enabled = mtn.Type == MetaTreeNodeType.MetaTable;

            // if they just inserted the row they need to be able to edit the name
            if (mtn.Name == "") ColName.OptionsColumn.AllowEdit = true;

            string colType = treeListNode[ColType].ToString();
            buttonEdit.Buttons[0].Enabled = (colType == MetaTreeNodeType.MetaTable.ToString());
        }

        /// <summary>
        /// Following a search, this button will Reset the Tree to it's original (full) state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnResetTree_Click(object sender, EventArgs e)
        {
            //treeList1.OptionsBehavior.DragNodes = true;
            treeList1.OptionsDragAndDrop.DragNodesMode = DragNodesMode.Multiple;
            treeList1.Nodes.Clear();
            ParseContentsTreeEditorXml();
            btnShowParents.Enabled =
            btnResetTree.Enabled = false;
        }

        /// <summary>
        /// If the enter key is pressed while the focus is on the seach control, search the tree.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchAll_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13) // enter key
            {
                SearchTree(btnSearchAll.Text);
            }
        }

        private void treeList1_ValidatingEditor(object sender, BaseContainerValidateEditorEventArgs e)
        {
            if (treeList1.FocusedColumn.Name == ColName.Name || treeList1.FocusedColumn.Name == ColLabel.Name)
            {
                if (string.IsNullOrEmpty(e.Value.ToString()))
                { MessageBox.Show(treeList1.FocusedColumn.Caption + " is a required field."); }
            }
        }

        private void treeList1_InvalidNodeException(object sender, InvalidNodeExceptionEventArgs e)
        {
            e.ExceptionMode = ExceptionMode.Ignore;
            MessageBox.Show("You must enter a valid name and label.");
        }
    }

    /// <summary>
    /// Operations method for finding nodes within a tree.
    /// </summary>
    public class FindNodesOperation2 : TreeListOperation
    {
        readonly XmlNode _xmlNode;
        TreeListNode _foundNode;
        public FindNodesOperation2(XmlNode xmlNode)
        {
            _xmlNode = xmlNode;
        }

        public override void Execute(TreeListNode node)
        {
            XmlNode tagNode = (XmlNode)node.Tag;
            MetaTreeEditNode currentMtn = ContentsTreeEditorControl2.GetMetaTreeEditNode(_xmlNode);
            if (tagNode != null)
            {
                MetaTreeEditNode tagMtn = ContentsTreeEditorControl2.GetMetaTreeEditNode(tagNode);

                if (tagMtn.Name == currentMtn.Name &&
                    tagMtn.Label == currentMtn.Label &&
                    tagMtn.Type == currentMtn.Type &&
                    tagNode.ParentNode == _xmlNode.ParentNode)
                    _foundNode = node;
            }
        }

        public TreeListNode FoundNode
        {
            get { return _foundNode; }
        }
    }

    // Save this for now.  This can be used for re-ordering nodes when pasted.  By default they will
    // paste in the order selected.  This method could be used to re-order them in the order they were
    // originally.

    public static class TreeListHelper
    {
        public static TreeListMultiSelection RestoreNodesSequence(TreeListMultiSelection selectedNodes, TreeList tList)
        {
            var nodesDic = new Dictionary<int, TreeListNode>();
            var newNodes = new TreeListMultiSelection(tList);

            foreach (TreeListNode node in selectedNodes)
            {
                var nodeIndex = tList.GetNodeIndex(node);
                nodesDic.Add(nodeIndex, node);
            }

            var sortedDictionary = nodesDic.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            newNodes.Add(sortedDictionary.Values.ToList());
            return newNodes;
        }
    }

}
