using Mobius.Data;
using Mobius.QueryEngineLibrary;
using Mobius.ComOps;
using Mobius.UAL;

using System;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace Mobius.MetaFactoryNamespace
{
	/// <summary>
	/// MetaTree builder
	/// </summary>

	public partial class MetaTreeFactory : IMetaTreeFactory
	{
		public static string MetaTreeDir; // folder that contains tree xml files
		public static Dictionary<string, MetaTreeNode> Nodes; // map of all tree nodes keyed by name
		static HashSet<string> AlternateLabels = new HashSet<string>();
		public static bool IncludePlatformTargetAssayExtension = true; // include extension for platform, target, assay organization
		public static bool IncludePathwayTargetAssayExtension = true; // include extension for pathway, target, assay organization
		public static bool IncludeTreeExtensions = true; // include hidden extensions from data sources
		public static bool IncludeAssayDbContentOrg = true; // load assay content organization
		public static bool ForceAlternateTableLabels = true; // alternate labels have top priority if true
		public static bool UseAssayMetadataLabels = true; // if just a normal label use the label from the assay metadata database
		public static bool ShowMetaTableStats = true;
		public static bool LogStartupDetails = false;

		public static bool EditingRootXml = false;
		public static int RootXmlSaves = 0;

		static string TreeDirectory { get { return ServicesDirs.MetaDataDir + @"\MetaTrees"; } } // dir containing Root.xml and backups
		static string RootXmlFile { get { return ServicesDirs.MetaDataDir + @"\MetaTrees\Root.xml"; } }
		static string RootEditorLogFileName { get { return ServicesDirs.MetaDataDir + @"\MetaTrees\RootEditorSession.log"; } } // log of edit sessions
		static LogFile RootEditorLogFile = null; // logging object
		static string RootCurrentBackupFile { get { return ServicesDirs.MetaDataDir + @"\MetaTrees\RootCurrentBackupFile.txt"; } } // file contains index of current backup file
		static FileStream RootStream = null; // stream to read & write Root.xml

		static bool BuildAsynch; // if true doing asynchronous build
		static Semaphore AsyncBuildLock;
		static bool BuildComplete; // initial tree build complete
		static Exception BuildTreeException; // any exception from tree build
		static bool QuickBuild = false; // do quick build of tree if true
		const string DefaultCachedTreeFile = "CachedTree.txt";
		static int LabelLookupCount = 0;

/// <summary>
/// Constructor
/// </summary>

		public MetaTreeFactory()
		{
			return;
		}

		/// <summary>
		/// Build & return the full MetaTree
		/// </summary>

		public Dictionary<string, MetaTreeNode> GetMetaTree()
		{
			bool attemptToBuildFromCache = ServicesIniFile.ReadBool("BuildContentsTreeFromCache", true);
			Build(attemptToBuildFromCache);
			WaitForInitializeCompletion(); // be sure complete
			return Nodes;
		}

		/// <summary>
		/// Get the full MetaTree from the specified root file. 
		/// Client does rebuild locally
		/// </summary>
		/// <param name="treeRootPath"></param>
		/// <returns></returns>

		public Dictionary<string, MetaTreeNode> GetMetaTree(string treeRootPath)
		{
			bool attemptToBuildFromCache = false;
			Build(attemptToBuildFromCache, treeRootPath); // force build from specified file
			return Nodes;
		}

		/// <summary>
		/// rebuild & return the full MetaTree
		/// </summary>

		public Dictionary<string, MetaTreeNode> ReloadMetaTree()
		{
			bool attemptToBuildFromCache = false;
			Build(attemptToBuildFromCache);
			WaitForInitializeCompletion(); // be sure complete
			return Nodes;
		}

		/// <summary>
		/// Build initial contents tree
		/// </summary>

		public static void Build()
		{
			bool attemptToBuildFromCache = ServicesIniFile.ReadBool("BuildContentsTreeFromCache", true);
			Build(attemptToBuildFromCache);
			WaitForInitializeCompletion(); // be sure complete
			return;
		}

		/// <summary>
		/// Build initial contents tree, optionally from cache
		/// </summary>

		public static void Build(
			bool attemptToBuildFromCache)
		{
			Build(attemptToBuildFromCache, RootXmlFile);
			return;
		}

		/// <summary>
		/// Build initial contents tree, optionally from cache
		/// </summary>
		
		public static void Build(
			bool attemptToBuildFromCache,
			string treeRootPath)
		{
			bool quickBuild = false;
			Build(attemptToBuildFromCache, quickBuild, treeRootPath);
			return;
		}

		/// <summary>
		/// Build initial contents tree, optionally from cache
		/// </summary>

		public static void Build(
			bool attemptToBuildFromCache,
			bool quickBuild,
			string treeRootPath)
		{
			int t0 = TimeOfDay.Milliseconds();

			BuildTreeException = null;
			BuildComplete = false;
			QuickBuild = quickBuild;

			try
			{
				if (BuildContentsTreeDisabled()) return;

				ForceAlternateTableLabels = // see if we should use alternate labels (1st priority)
					ServicesIniFile.ReadBool("ForceAlternateTableLabels", true);

				UseAssayMetadataLabels = // see if we should use labels from assay metadata (2nd priority)
					ServicesIniFile.ReadBool("UseAssayMetadataLabels", true);

				ShowMetaTableStats = // see if we should include table size & mod date
					ServicesIniFile.ReadBool("IncludeMetaTableStats", true);

				IncludePlatformTargetAssayExtension = // see if we should include extension for platform, target, assay organization
					ServicesIniFile.ReadBool("IncludePlatformTargetAssayExtension", true);

				IncludeTreeExtensions = // see if we should include hidden extensions from data sources
					ServicesIniFile.ReadBool("IncludeTreeExtensions", true);

				IncludeAssayDbContentOrg = // include Assay contents organization
					ServicesIniFile.ReadBool("IncludeAssayDbContentOrg", true);

				LogStartupDetails = // log timing details of startup
				ServicesIniFile.ReadBool("LogStartupDetails", false);

				try // get user preference for showing data source, need to know while building tree
				{
					MetaTableFactory.ShowDataSource = ServicesIniFile.ReadBool("ShowDataSourceDefault", true);
				}
				catch (Exception ex) { }

				if (quickBuild)
				{
					IncludePlatformTargetAssayExtension = false;
					IncludeTreeExtensions = false;
					IncludeAssayDbContentOrg = false;
				}

				bool buildFromCache = attemptToBuildFromCache; // try to build from cache if possible

				if (attemptToBuildFromCache)
				{
					//if the cached copy of the tree isn't stale, then we can build from the cache
					DateTime cacheDate;
					buildFromCache = !IsCachedTreeStale(out cacheDate);
				}

				if (buildFromCache)
				{
					AsyncBuildLock = new Semaphore(0, 1);

					ThreadStart ts = new ThreadStart(BuildFromCache);
					Thread newThread = new Thread(ts);
					newThread.Name = "BuildMetaTreeFomCache";
					newThread.IsBackground = true;
					newThread.SetApartmentState(ApartmentState.STA);
					newThread.Start();

					AsyncBuildLock.WaitOne();
					AsyncBuildLock = null;

					return;
				}

				else // do non-asynch "slow" build of tree from sources
				{
					if (LogStartupDetails)
					{
						DebugLog.Message("Starting BuildFromSources");
						//StackTrace stackTrace = new StackTrace(true);
						//DebugLog.Message(stackTrace.ToString());
					}

					BuildFromSources(quickBuild, treeRootPath); //pass true to ensure that cached version of tree includes "hidden" collaboration data
					BuildComplete = true;

					if (attemptToBuildFromCache) // write new cache if building from it is enabled
					{
						try
						{
							string fileName = // name of cached tree file
								MetaTreeDir + @"\" + ServicesIniFile.Read("CachedTreeFile", DefaultCachedTreeFile);
							WriteTreeCache(Nodes, fileName); // update cache
						}
						catch (Exception ex) { DebugLog.Message(DebugLog.FormatExceptionMessage(ex, "Error writing tree cache")); }
					}

				}
			}

			catch (Exception ex)
			{
				BuildTreeException = ex;
				BuildComplete = true;
			}

			MetaTreeNode rootNode = null;
			if (Nodes.ContainsKey("ROOT")) // debug
				rootNode = Nodes["ROOT"];

			return;
		}

		/// <summary>
		/// Quickly build the metatree from Root.xml without the usual extensions
		/// </summary>
		/// <param name="rootXmlFileName"></param>

		public Dictionary<string, MetaTreeNode> BuildQuickMetaTree(string rootXmlFileName)
		{
			bool quickBuild = true;
			bool attemptToBuildFromCache = false;
			Build(attemptToBuildFromCache, quickBuild, rootXmlFileName); 
			return Nodes;
		}

		/// <summary>
		/// Check source dates to see if the cache was last written after the most recent change
		/// </summary>
		/// <param name="cacheDate"></param>
		/// <returns></returns>

		public static bool IsCachedTreeStale(out DateTime cacheDate)
		{
			bool cacheIsStale = false;

			// 
			string cacheFile = ServicesDirs.MetaDataDir + @"\MetaTrees\" + // see if cache is up to date
					ServicesIniFile.Read("CachedTreeFile", DefaultCachedTreeFile);

			try { cacheDate = File.GetLastWriteTime(cacheFile); }
			catch (Exception ex) { cacheDate = DateTime.MinValue; }

			if (FileNewer(ServicesDirs.MetaDataDir + @"\MetaTrees\Root.xml", cacheDate)) cacheIsStale = true;
			if (FileNewer(ServicesDirs.MetaDataDir + @"\MetaTables\AssayPrecomputedMetadata.txt", cacheDate)) cacheIsStale = true;

			return cacheIsStale;
		}

		/// <summary>
		/// Exposes Reset via the IMetaTreeFactory interface
		/// </summary>
		void IMetaTreeFactory.Reset()
		{
			Reset();
		}

		/// <summary>
		/// Clear metatree data
		/// </summary>

		public static void Reset()
		{
			Nodes = null;
			return;
		}

		/// <summary>
		/// Check to see if tree building disabled (usually for debug) 
		/// </summary>
		/// <returns></returns>

		public static bool BuildContentsTreeDisabled()
		{
			if (Lex.Ne(ServicesIniFile.Read("BuildContentsTree"), "false")) return false;

			MetaTreeFactory.Nodes = new Dictionary<string, MetaTreeNode>();
			MetaTreeNode mtn = new MetaTreeNode();
			mtn.Name = "root";
			mtn.Type = MetaTreeNodeType.Root;
			mtn.Label = "Tree Build Disabled";
			MetaTreeFactory.AddNode(mtn);
			MetaTableCollection.ContentsRoot = MetaTreeFactory.GetNode("root"); // set root pointer

			BuildComplete = true;
			return true;
		}

		/// <summary>
		/// See if tree cache file is older than file comparing to
		/// </summary>
		/// <param name="cacheDate"></param>
		/// <param name="compareFile"></param>
		/// <returns></returns>

		public static bool FileNewer(
			string fileName,
			DateTime compareDate)
		{
			DateTime fileDate;

			try { fileDate = File.GetLastWriteTime(fileName); }
			catch (Exception ex) { return false; }

			if (DateTime.Compare(fileDate, compareDate) > 0) return true;
			else return false;
		}

		/// <summary>
		/// Build tree from cache on new thread
		/// </summary>
		public static void BuildFromCache()
		{
			try
			{ MetaTreeFactory.BuildFromCache(ServicesDirs.MetaDataDir + @"\MetaTrees"); }

			catch (Exception ex) // set uo IOException for debug
			{ BuildTreeException = ex; }

			AsyncBuildLock.Release();
			BuildComplete = true;

			return;
		}

		/// <summary>
		/// Wait for initialization to complete
		/// </summary>

		public static void WaitForInitializeCompletion()
		{
			while (!BuildComplete) // loop til complete
				Thread.Sleep(10);

			if (BuildTreeException != null) throw new Exception(BuildTreeException.Message, BuildTreeException);

			//if (BuildTreeException != null)
			//  MessageBoxMx.ShowError("Error building contents tree: \r\n" +
			//    DebugLog.FormatExceptionMessage(ex));

			//foreach (MetaTreeNode mtn in MetaTreeFactory.Nodes) // (debug)
			//{ // see where the node name doesn't match it's itemId
			//  if (mtn.Name != mtn.Target)
			//    DebugLog.Message("Name: " + mtn.Name + ", ItemId: " + mtn.Target);
			//}

			return;
		}

		/// <summary>
		/// Build the metadata tree
		/// </summary>
		/// <param name="metaTreePath">Path to directory or root file for tree</param>

		public static void BuildFromSources(
			bool quickBuild,
			string metaTreePath)
		{
			MetaTreeNode mtn;
			DateTime rootDate, cacheDate;
			StreamReader sr = null;
			string rootFile, fileName;
			int i1;

			try
			{

				int t0 = TimeOfDay.Milliseconds();

				if (File.Exists(metaTreePath)) // specified path to root file
				{
					rootFile = metaTreePath;
					MetaTreeDir = Path.GetDirectoryName(metaTreePath);
				}

				else // metaTreePath must be the folder we need to look in 
				{
					MetaTreeDir = metaTreePath;
					rootFile = ServicesIniFile.Read("MetaTreeRootFile", "Root.xml"); // root to start at
					if (rootFile.IndexOf(@"\") < 0) rootFile = MetaTreeDir + @"\" + rootFile;
				}

				Nodes = new Dictionary<string, MetaTreeNode>();
				AlternateLabels = new HashSet<string>();

				try
				{
					ParseMetaTreeXml(rootFile);
				}
				catch (Exception ex)
				{
					string msg = "Error parsing MetaTree file " + rootFile;
					DebugLog.Message(DebugLog.FormatExceptionMessage(ex, msg));
					throw new Exception(msg, ex);
				}

				if (LogStartupDetails)
					t0 = DebugLog.TimeMessage("MetaTreeFactory.Build ParseMetaTreeXml, total nodes: " + Nodes.Count + ", time: ", t0);

				// Include Platform, Target, Assay organized section

				if (IncludePlatformTargetAssayExtension)
				{
					BuildPlatformMetaTree();
					if (LogStartupDetails)
						t0 = DebugLog.TimeMessage("MetaTreeFactory.Build Platform, Target, Assay Extension, total nodes: " + Nodes.Count + ", time: ", t0);
				}

				// Include Pathway, Target, Assay organized section

				if (IncludePathwayTargetAssayExtension)
				{
					BuildPathwayMetaTree();
					if (LogStartupDetails)
						t0 = DebugLog.TimeMessage("MetaTreeFactory.Build Pathway, Target, Assay Extension, total nodes: " + Nodes.Count + ", time: ", t0);
				}


				// Include general assay view

				if (IncludeAssayDbContentOrg)
				{
					AssayMetaFactory assayMf = new AssayMetaFactory();
					assayMf.BuildMetaTree();
					if (LogStartupDetails)
						t0 = DebugLog.TimeMessage("MetaTreeFactory.Build Assay Extensions, total nodes: " + Nodes.Count + ", time: ", t0);
				}

				// Include hidden list of all assays for each assay source if flag is set

				if (IncludeTreeExtensions)
				{
					// noop
				} // end of tree extensions

				// Adjust node child references to point to version of node with full data

				foreach (MetaTreeNode mtn0 in Nodes.Values)
				{
					List<MetaTreeNode> children = mtn0.Nodes;
					for (i1 = 0; i1 < children.Count; i1++)
					{
						mtn = children[i1];
						if (MetaTreeFactory.Nodes.ContainsKey(mtn.Name))
							children[i1] = MetaTreeFactory.Nodes[mtn.Name];
						else DebugLog.Message("Child node not in tree: " + mtn.Name);
					}
				}

				if (LogStartupDetails)
					t0 = DebugLog.TimeMessage("MetaTreeFactory.Build Adjust References Time: ", t0);

				// Apply assay metadata labels if requested

				if (UseAssayMetadataLabels)
				{
					int assayTotal = 0; // number of assay tables seen
					int assaysRenamed = 0; // number of assay tables renamed

					foreach (MetaTreeNode mtn0 in Nodes.Values)
					{
						if (!mtn0.IsAssayNode) continue;
						assayTotal++;
						if (AlternateLabels.Contains(mtn0.Label)) continue; // don't use if already has alternate label
						string assayId = mtn0.Target.Substring(4);
						string assayLabel = DictionaryMx.LookupWordByDefinition("mthd_vrsn_rpt_nm", assayId);
						if (assayLabel != null && assayLabel != mtn0.Label)
						{
							mtn0.Label = assayLabel;
							assaysRenamed++; // number of Warehouse tables renamed
						}
					}
					if (LogStartupDetails)
						DebugLog.Message("Assay labels - total metatables: " + assayTotal + ", renamed: " + assaysRenamed);
				}

				// Apply any interactively assigned labels to nodes

				DictionaryMx newDict = DictionaryMx.Get("NewNameDict");
				DictionaryMx originalDict = DictionaryMx.Get("OriginalNameDict");
				if (newDict != null && originalDict != null)
				{
					foreach (MetaTreeNode mtn0 in Nodes.Values)
					{
						string newLabel = newDict.LookupDefinition(mtn0.Name);
						if (newLabel == null) continue;
						originalDict.Add(mtn0.Name, mtn0.Label); // save original label
						mtn0.Label = newLabel;
					}
				}

				if (LogStartupDetails)
					t0 = DebugLog.TimeMessage("MetaTreeFactory.Build rename nodes time", t0);

				ShowHideDataSource(MetaTableFactory.ShowDataSource); // set source in label if requested

				if (LogStartupDetails)
					t0 = DebugLog.TimeMessage("MetaTreeFactory.ShowHideDataSource time", t0);

				MetaTableFactory.LoadMetaTableStats(); // load table stats

				if (ShowMetaTableStats) // show metatable stats if requested
				{
					ShowMetaTableStatsMethod();
					if (LogStartupDetails)
						t0 = DebugLog.TimeMessage("MetaTreeFactory.ShowMetaTableStats time", t0);
				}

				MetaTableCollection.ContentsRoot = GetNode("root"); // set root pointer

				return;
			} // try

			catch (Exception ex)
			{
				DebugLog.Message(ex);
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// Add new node & link in if doesn't already exist
		/// </summary>
		/// <param name="id"></param>
		/// <param name="label"></param>
		/// <param name="type"></param>
		/// <param name="parentId"></param>

		static void AddNodeAsNeeded(
			string id,
			string label,
			int type,
			string parentId)
		{
			if (id == null || id == "") return;
			MetaTreeNode mtn = MetaTreeFactory.GetNode(id); // see if already exists
			if (mtn == null) // create if doesn't exist
			{
				mtn = new MetaTreeNode();
				mtn.Name = id.Trim();
				mtn.Target = id.Trim();
				mtn.Label = label;
				mtn.Type = (MetaTreeNodeType)type;
				MetaTreeFactory.AddNode(mtn);
			}

			MetaTreeNode parent = MetaTreeFactory.GetNode(parentId); // add to parent if needed
			if (parent != null && parent.GetChild(mtn.Name) == null)
				parent.Nodes.Add(mtn);
		}

		/// <summary>
		/// Top level parsing of a metatree XML file
		/// </summary>
		/// <param name="fileName"></param>

		static void ParseMetaTreeXml(
			String fileName)
		{

			XmlDocument doc = new XmlDocument();
			XmlNode node;
			FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

			try
			{
				doc.Load(fs);

				node = doc.SelectSingleNode("metatree");
				if (node == null)
				{
					throw new Exception("No initial element (\"metatree\") found");
				}

				// Parse each first level node recursively
				ParseChildNodes(node, null);
			}

			finally { if (fs != null) fs.Close(); }

		} // end of ParseMetaTreeXml

		/// <summary>
		/// Recursively parse child nodes
		/// </summary>
		/// <param name="node"></param>
		/// <param name="parentNode"></param>

		static void ParseChildNodes(
			XmlNode parentXmlNode,
			MetaTreeNode parentTreeNode)
		{
			XmlNode node;
			MetaTreeNode mtn2;
			int i1;
			string altLabel, nodeName, attName, attValue;

			for (node = parentXmlNode.FirstChild; node != null; node = node.NextSibling)
			{
				if (node.NodeType != XmlNodeType.Element)
					continue;

				nodeName = node.Name.ToLower();
				if (nodeName == "child" || nodeName == "node" || nodeName == "parent")
				{ }	// found an expected node name, safe to go on

				else if (nodeName == "change" || nodeName.StartsWith("deleted"))
					continue; // edited nodes, ignore

				else
				{
					throw new Exception("Expected Node element but saw " + node.Name);
				}

				MetaTreeNode mtn = new MetaTreeNode();
				XmlAttributeCollection atts = node.Attributes;
				altLabel = ""; // init any alternate label
				bool disabled = false;



				for (int i = 0; i < atts.Count; i++) // parse node attributes
				{
					XmlNode att = atts.Item(i);
					attName = att.Name.ToUpper();
					attValue = att.Value.ToUpper();

					if (attName == "TYPE")
					{
						mtn.Type = MetaTreeNode.ParseTypeString(attValue);
						if (mtn.Type == MetaTreeNodeType.Unknown)
							throw new Exception("Unexpected Node Type: " + attValue);
					}

					else if (attName == "N" || attName == "NAME")
					{
						mtn.Name = attValue.ToUpper().Trim(); //since case is changed anyway
						if (mtn.Target == "") mtn.Target = mtn.Name;
					}
					
					else if (attName == "L" || attName == "LABEL")
					{
						mtn.Label = att.Value.Trim(); //to restore the original case
					}
					
					else if (attName == "ITEM" || attName == "ITEMID" || attName == "ITEMSTRING" || attName == "ITEMIDSTRING" ||
						attName == "TARGET")
					{
						if (Lex.IsUri(att.Value)) // use original case if it's a url
							mtn.Target = att.Value.Trim();
						else mtn.Target = attValue.Trim();
					}
					
					else if (attName == "POSITION" || attName == "DISPLAYORDER")
					{
						i1 = Convert.ToInt32(attValue); // obsolete
					}
					
					else if (attName == "C" || attName == "COMMENT")
					{
						altLabel = att.Value; //to restore the original case
					}

					else if (attName == "AL" || attName == "ALABEL")
					{
						altLabel = att.Value.Trim(); //to restore the original case
					}

					else if (attName == "DISABLED") // disabled item
					{
						bool.TryParse(attValue, out disabled);
					}

					//if (Lex.Eq(att.Value,"ACD_Database")) mtn2=mtn; // debug

				} // end of loop over attributes

				if (disabled) continue; // if disabled node, don't add it t

				if (mtn.Name == null)
					throw new Exception("Node name missing for node " + mtn.Label);

				//if (Lex.Eq(mtn.Name, "star_42802")) i1=1; // debug
				//else if (mtn.Name.ToLower().IndexOf("assay_1834") >= 0) i1=1; // debug

				if (ForceAlternateTableLabels && altLabel != "")
				{
					mtn.Label = altLabel; // force use of alternate label
					AlternateLabels.Add(altLabel);
				}

				else if (mtn.Type == MetaTreeNodeType.MetaTable)
				{
					if (mtn.Label == "")
					{ // if no label try to get label from metatable name
						mtn.Label = mtn.Name;

						if (!QuickBuild) 
						{
							MetaTable mt = MetaTableCollection.Get(mtn.Name);
							if (mt != null) mtn.Label = mt.Label;
						}

						LabelLookupCount++;
					}

					// Add size && date modified

					if (ShowMetaTableStats)
					{

					}
				}

				if (mtn.Label == "") // use name as last resort
				{
					if (altLabel != "") mtn.Label = altLabel;
					else mtn.Label = mtn.Name;
				}

				if (Nodes.ContainsKey(mtn.Name))
					mtn2 = Nodes[mtn.Name]; // get any existing info on this node
				else mtn2 = null;

				if (nodeName == "parent") // this is a ref to our parent
				{
					if (mtn2 != null) mtn = mtn2;
					else AddNode(mtn);
					if (parentTreeNode != null)
						mtn.Nodes.Add(parentTreeNode);
				}
				else // this is a normal child or top level node
				{
					if (mtn2 != null) // does a node with this name already exist?
					{
						if (mtn2.Nodes.Count > 0 || // keep prev node if it has children
							(mtn.Name == mtn.Label && mtn2.Label != mtn2.Name)) // or has defined label & this node doesn't
						{
							if (mtn2.Type == MetaTreeNodeType.Unknown) // keep any node type info in new node
								mtn2.Type = mtn.Type;
							mtn = mtn2;
						}
						else AddNode(mtn); // replace existing node with new node
					}
					else AddNode(mtn); // add new node

					if (parentTreeNode != null)
						parentTreeNode.Nodes.Add(mtn);
				}

				// Parse children recursively

				ParseChildNodes(node, mtn);
			}
		} // end of parseChildNodes

		/// <summary>
		/// Add or remove source identifiers in metatree nodes
		/// Todo: This fails to find and change nodes that are in the tree but not in the Hashtable (e.g. assay_1439)
		/// </summary>
		/// <param name="showSource"></param>

		public static void ShowHideDataSource(
			bool showSource)
		{
			int t0 = TimeOfDay.Milliseconds();

			foreach (MetaTreeNode mtn in Nodes.Values)
			{
				if (mtn.Type != MetaTreeNodeType.MetaTable &&
					mtn.Type != MetaTreeNodeType.Annotation &&
					mtn.Type != MetaTreeNodeType.CalcField)
					continue; // just for these node types

				if (showSource) mtn.Label = MetaTableFactory.AddSourceToLabel(mtn.Target, mtn.Label);
				else mtn.Label = MetaTableFactory.RemoveSourceFromLabel(mtn.Target, mtn.Label);
			}

			t0 = TimeOfDay.Milliseconds() - t0;
			return;
		}

		/// <summary>
		/// Add metatable table statistics to node label
		/// </summary>

		public static void ShowMetaTableStatsMethod()
		{
			if (MetaTableFactory.TableStats == null) return; // no table stats

			IDictionaryEnumerator e = Nodes.GetEnumerator();
			foreach (MetaTreeNode mtn in Nodes.Values)
			{
				if (mtn.Type != MetaTreeNodeType.MetaTable) continue;

				string target = mtn.Target;
				MetaTableStats mts = MetaTableFactory.GetStats(target);
				if (mts == null) continue;
				mtn.Size = (int)mts.RowCount;
				mtn.UpdateDateTime = mts.UpdateDateTime;
			}
		}

		/// <summary>
		/// Write the tree to disk in compact format
		/// </summary>

		public static void WriteTreeCache(
			Dictionary<string, MetaTreeNode> nodes,
			string filePath)
		{
			StreamWriter sw = null;

			int t0 = TimeOfDay.Milliseconds();

			try
			{
				StringBuilder sb = new StringBuilder();

				foreach (MetaTreeNode mtn in nodes.Values)
				{
					sb.Append(mtn.Name);
					sb.Append("\t");
					sb.Append(((int)mtn.Type).ToString());
					sb.Append("\t");
					sb.Append(mtn.Label);
					sb.Append("\t");
					if (mtn.Size >= 0) sb.Append(mtn.Size.ToString());
					sb.Append("\t");
					if (mtn.UpdateDateTime != DateTime.MinValue)
						sb.Append(mtn.UpdateDateTime.ToShortDateString()); // store in US-English format (assuming we're running in that mode)
					sb.Append("\t");
					sb.Append(mtn.Target);
					sb.Append("\t");
					foreach (MetaTreeNode child in mtn.Nodes)
					{
						sb.Append(child.Name);
						sb.Append("\t");
					}
					sb.Append("\r\n");
				}

				string dir = Path.GetDirectoryName(filePath);
				string tempExt = Path.GetFileName(filePath);
				string tempFile = TempFile.GetTempFileName(ServicesDirs.TempDir, tempExt);

				DebugLog.Message("Writing metatree cache to temp file: " + tempFile);
				sw = new StreamWriter(tempFile); // write to temp file initially
				sw.Write(sb.ToString());
				sw.Close();

				DebugLog.Message("Copying temp metatree cache file to: " + filePath);

				bool replaceOk = FileUtil.BackupAndReplaceFile(filePath, filePath + ".bak", tempFile, true); // swap in the new file, logging errors
				if (!replaceOk)
				{
					string lockMsg = FileUtil.WhoIsLockingToString(filePath);
					if (Lex.IsDefined(lockMsg)) 
						DebugLog.Message("Metatree cache file " + filePath + " is locked by:\r\n" + lockMsg);
				}

				if (LogStartupDetails)
					DebugLog.TimeMessage("WriteTreeCache time", t0);

				return;
			}

			catch (Exception ex)
			{
				DebugLog.Message(ex);
				FileUtil.CloseStreamWriter(sw); // be sure closed
				throw new Exception(ex.Message, ex);
			}

		}

		/// <summary>
		/// Read compact format tree from disk for performance improvement over normal build
		/// </summary>

		public static void BuildFromCache(
				string metaTreeDir)
		{
			BuildFromCache(metaTreeDir, false);
		}

		/// <summary>
		/// Read compact format tree from disk for performance improvement over normal build
		/// </summary>

		public static void BuildFromCache(
			string metaTreeDir,
			bool showHideLabelDataSources)
		{
			FileStream fs = null;
			MemoryStream ms = null;
			StreamReader sr = null;
			MetaTreeNode mtn, child;
			string cacheFilePath = "", fileText;
			int line = 0;

			int t0 = TimeOfDay.Milliseconds();

			if (LogStartupDetails)
				DebugLog.Message("Started (asynch)");

			try
			{
				MetaTreeDir = metaTreeDir;

				string cacheFileName = DefaultCachedTreeFile;

				if (ServicesIniFile.IniFile != null) // get name of cached tree file
					cacheFileName = ServicesIniFile.Read("CachedTreeFile", DefaultCachedTreeFile);

				cacheFilePath = MetaTreeDir + @"\" + cacheFileName;

// Quickly open, read and close the file

				fs = // open as FileShare.ReadWrite to minimize blocking
					File.Open(cacheFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				sr = new StreamReader(fs);
				fileText = sr.ReadToEnd();
				sr.Close();
				fs.Close();

				ms = new MemoryStream(Encoding.UTF8.GetBytes(fileText));
				sr = new StreamReader(ms);

				Nodes = new Dictionary<string, MetaTreeNode>();
				while (true)
				{
					string rec = sr.ReadLine();
					line++;
					if (rec == null) break;
					if (Lex.IsUndefined(rec)) continue;

					string[] sa = rec.Split('\t');

					if (sa.Length < 6) // must have basic 6 elements
					{
						try // close & delete the corrupted file
						{
							//sr.Close();
							//fs.Close();

							string badFile = cacheFilePath + ".bad"; // try to backup the bad file
							try { File.Delete(badFile); } // delete any old version
							catch (Exception ex) { }

							try { File.Move(cacheFilePath, badFile); } // rename existing file to badFile
							catch (Exception ex) { }
						}
						catch (Exception ex2) { ex2 = ex2; }
						throw new Exception("Corrupted CachedTreeFile " + cacheFilePath + " at line " + line + ", content: " + rec);
					}

					string mtnName = sa[0];
					MetaTreeNodeType mtnType = (MetaTreeNodeType)int.Parse(sa[1]);
					string mtnLabel = sa[2];

					if (Nodes.ContainsKey(sa[0])) mtn = Nodes[sa[0]];
					else
					{
						mtn = new MetaTreeNode();
						Nodes[sa[0]] = mtn;
					}

					mtn.Name = mtnName;
					mtn.Type = mtnType;
					mtn.Label = mtnLabel;

					//				if (mtn.Label.Contains("ACVR1B")) mtn = mtn; // debug
					if (!String.IsNullOrEmpty(sa[3])) mtn.Size = int.Parse(sa[3]);
					if (!String.IsNullOrEmpty(sa[4]) && sa[4] != "1/1/0001" && sa[4] != "1/1/2001")
						mtn.UpdateDateTime = DateTimeUS.ParseDate(sa[4]);
					mtn.Target = sa[5];

					mtn.Nodes = new List<MetaTreeNode>(); // add placeholders for child nodes
					for (int i1 = 6; i1 < sa.Length; i1++)
					{
						if (sa[i1] == null || sa[i1] == "") continue;
						else if (Nodes.ContainsKey(sa[i1])) child = Nodes[sa[i1]];
						else
						{
							child = new MetaTreeNode();
							child.Name = sa[i1];
							Nodes[child.Name] = child;
						}
						mtn.Nodes.Add(child);
					}
				}
			}
			catch (Exception ex)
			{
				try
				{
					DebugLog.Message(ex);
					sr.Close(); // be sure closed
					ms.Close();
				}
				catch (Exception ex2) { ex2 = ex2; }
				throw new Exception(ex.Message, ex);
			}

			sr.Close(); // be sure closed
			ms.Close();

			if (showHideLabelDataSources)
			{
				MetaTreeFactory.ShowHideDataSource(MetaTableFactory.ShowDataSource); // set source in label if requested
			}

			MetaTableCollection.ContentsRoot = GetNode("root"); // set root pointer

			if (LogStartupDetails)
				DebugLog.TimeMessage("Time (asynch)", t0);

			return;
		}

		/// <summary>
		/// Add a node to the metatree
		/// </summary>
		/// <param name="node"></param>

		public static void AddNode(
			MetaTreeNode node)
		{
			if (node == null) return;
			//if (Lex.Eq(node.Target, "star_42802")) node = node; // debug
			node.Name = node.Name.Trim().ToUpper();
			Nodes[node.Name] = node;
			return;
		}

		/// <summary>
		/// Lookup a MetaTreeNode by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static MetaTreeNode GetNode(
			string name)
		{
			MetaTreeNode mt;

			if (Nodes == null) return null;
			name = name.Trim().ToUpper();
			if (!Nodes.ContainsKey(name)) return null;
			mt = Nodes[name];
			return mt;
		}

		/// <summary>
		/// Lookup a MetaTreeNode by label
		/// </summary>
		/// <param name="label"></param>
		/// <returns></returns>

		public static MetaTreeNode GetNodeByLabel(
			string label)
		{
			if (Nodes == null || label == null || label == "") return null;
			foreach (MetaTreeNode mtn in Nodes.Values)
			{
				if (Lex.Eq(label, mtn.Label)) return mtn;
			}
			return null;
		}

		/// <summary>
		/// Lookup a MetaTreeNode by target
		/// </summary>
		/// <param name="label"></param>
		/// <returns></returns>

		public static MetaTreeNode GetNodeByTarget(
			string target)
		{
			if (Nodes == null) return null;
			{
				foreach (MetaTreeNode mtn in Nodes.Values)
				{
					if (Lex.Eq(mtn.Target, target)) return mtn;
				}
			}
			return null;
		}

/// <summary>
/// Get all nodes that are subnodes of the supplied node
/// Include the supplied node in the returned list
/// </summary>
/// <param name="n"></param>
/// <returns></returns>

		public static List<MetaTreeNode> GetSubnodes(
			MetaTreeNode n)
		{
			HashSet<string> set = new HashSet<string>();
			List<MetaTreeNode> list = new List<MetaTreeNode>();

			GetSubnodes(n, set, list); // enter recursive code
			if (list.Count == 0) list = null; // nothing found
			return list;
		}

/// <summary>
/// Recursively get the subnodes
/// </summary>
/// <param name="n"></param>
/// <param name="set"></param>
/// <param name="list"></param>

		public static void GetSubnodes(
			MetaTreeNode n,
			HashSet<string> set,
			List<MetaTreeNode> list)
		{
			if (n == null || set.Contains(n.Name)) return;
			set.Add(n.Name);
			list.Add(n);

			foreach (MetaTreeNode mtn in n.Nodes) // check subnodes
			{
				if (mtn.IsFolderType && !set.Contains(mtn.Name))
					GetSubnodes(mtn, set , list);
			}

			return;
		}

		/// <summary>
		/// Mark the MetaTree cache for rebuilding
		/// </summary>

		public void MarkCacheForRebuild()
		{
			DeleteCacheFile();
		}

		/// <summary>
		/// Delete tree cache file
		/// </summary>

		public static void DeleteCacheFile()
		{
			MetaTreeNode mtn, child;

			if (MetaTreeDir == null) return;
			string fileName = // name of cached tree file
				MetaTreeDir + @"\" + ServicesIniFile.Read("CachedTreeFile", DefaultCachedTreeFile);
			try { File.Delete(fileName); }
			catch (Exception ex) { return; }
			return;
		}

		/// <summary>
		/// Get the xml for the full tree or a subtree
		/// Example node xml:
		///  <node name="root" l="Contents" type="root">
		///   <child name="corp_structures" l="CorpId Structures" type="metatable" />
		///   <child name="url_Mobius_usage"   l="Mobius Usage graphs" type="url" target="http://[server]/SpotfireWeb/..." />    
		///  </node>
		/// </summary>
		/// <param name="rootNodeName"></param>
		/// <param name="includeCurrentUsersObjects"></param>
		/// <param name="includeAllUsersObjects"></param>
		/// <returns></returns>

		public static string GetMetatreeXml(
			string rootNodeName,
			bool includeCurrentUsersObjects,
			bool includeAllUsersObjects)
		{
			MetaTreeNode node, uoNode;

			if (Nodes == null) // be sure basic tree is built
			{
				MetaTreeFactory mtf = new MetaTreeFactory();
				mtf.GetMetaTree(); // get tree if not done yet
			}

			IUserObjectTree iuot = InterfaceRefs.IUserObjectTree;
			bool includeUserObjects = ((iuot != null) && (includeCurrentUsersObjects || includeAllUsersObjects));

			if (includeUserObjects) // be sure the user object tree is built
				iuot.AssureTreeIsBuilt();

			if (Lex.IsDefined(rootNodeName))
				rootNodeName = rootNodeName.ToUpper();
			else rootNodeName = "ROOT";

			MemoryStream ms = new MemoryStream();
			XmlTextWriter tw = new XmlTextWriter(ms, null);
			tw.Formatting = Formatting.Indented;
			tw.WriteStartDocument();
			tw.WriteStartElement("MetaTree");

			LinkedList<string> nodesToDoList = new LinkedList<string>();
			HashSet<string> nodesToDoHash = new HashSet<string>();
			HashSet<string> nodesDoneHash = new HashSet<string>();

			nodesToDoHash.Add(rootNodeName);
			nodesToDoList.AddLast(rootNodeName);

			while (nodesToDoList.Count > 0)
			{
				string nodeName = nodesToDoList.First.Value;
				nodesToDoList.RemoveFirst();
				nodesToDoHash.Remove(nodeName);
				nodesDoneHash.Add(nodeName);

				//if (Lex.Eq(nodeName, "FOLDER_262028")) nodeName = nodeName; // debug

				node = GetNode(nodeName);
				if (node == null) // if not in main tree, see if node is a user object folder
				{
					if (iuot != null)
						node = iuot.FindUserObjectFolderNode(nodeName);
					if (node == null) continue; // just ignore if not found
				}

				tw.WriteStartElement("Node");
				tw.WriteAttributeString("name", nodeName);
				tw.WriteAttributeString("label", node.Label);
				tw.WriteAttributeString("type", node.Type.ToString());

				FormatSubnodes(node, includeCurrentUsersObjects, includeAllUsersObjects, tw, nodesToDoList, nodesToDoHash, nodesDoneHash);

				if (node.IsSystemType && includeUserObjects) // include user objects in system nodes?
				{
					uoNode = iuot.FindUserObjectFolderNode(nodeName);
					if (uoNode != null)
							FormatSubnodes(uoNode, includeCurrentUsersObjects, includeAllUsersObjects, tw, nodesToDoList, nodesToDoHash, nodesDoneHash);
				}

				tw.WriteEndElement(); // end of Node
			}

			tw.WriteEndElement(); // end of MetaTree
			tw.WriteEndDocument();
			tw.Flush();

			byte[] buffer = new byte[ms.Length];
			ms.Seek(0, 0);
			ms.Read(buffer, 0, (int)ms.Length);
			string xml = System.Text.Encoding.ASCII.GetString(buffer, 0, (int)ms.Length);
			tw.Close(); // must close after read

			return xml;
		}

		public static void FormatSubnodes(
			MetaTreeNode node,
			bool includeCurrentUsersObjects,
			bool includeAllUsersObjects,
			XmlTextWriter tw,
			LinkedList<string> toDoList,
			HashSet<string> toDoHash,
			HashSet<string> doneHash)
		{

			HashSet<string> childNodeNames = new HashSet<string>();

			foreach (MetaTreeNode childNode in node.Nodes)
			{
				bool include = childNode.IsSystemType || includeAllUsersObjects || // include node?
					(includeCurrentUsersObjects && Lex.Eq(childNode.Owner, UAL.Security.UserName));
				if (!include) continue; // skip user nodes not to be included

				string childNodeName = childNode.Name.ToUpper();
				if (Lex.Eq(childNodeName, "QUERY_255487")) childNodeName = childNodeName;
				if (childNodeNames.Contains(childNodeName)) continue;
				childNodeNames.Add(childNodeName);

				tw.WriteStartElement("child");
				tw.WriteAttributeString("name", childNodeName);
				tw.WriteAttributeString("label", childNode.Label);
				tw.WriteAttributeString("type", childNode.Type.ToString());
				string t = childNode.Target;
				if (childNode.IsUserObjectType && Lex.IsDefined(childNode.Owner)) // is UserObject include owner and shared attributes
					tw.WriteAttributeString("owner", childNode.Owner);

				if (Lex.IsDefined(t) && Lex.Ne(t, childNodeName)) // write target if different that node name
					tw.WriteAttributeString("Target", t);

				tw.WriteEndElement(); // end of child

				//if (Lex.Eq(childNodeName, "FOLDER_262028"))  childNodeName = childNodeName; // debug
				if (childNode.IsFolderType && !toDoHash.Contains(childNodeName) && !doneHash.Contains(childNodeName))
				{
					toDoHash.Add(childNodeName);
					toDoList.AddLast(childNodeName);
				}
			}
			return;
		}

/// <summary>
/// Locak and read Root.xml for editing
/// </summary>
/// <returns></returns>

		public static string LockAndReadMetatreeXml()
		{
			StreamReader sr;
			StreamWriter sw;

			if (RootStream != null) RootStream.Close(); // if open close

			try // see if we can get exclusive write access to the file
			{
				RootStream = new FileStream(RootXmlFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
			}

			catch (Exception ex) // if not, return message
			{
				string editor = "Unknown unknown";
				try
				{
					sr = new StreamReader(RootEditorLogFileName);
					string txt = sr.ReadToEnd();
					sr.Close();
					int i1 = txt.LastIndexOf("LockAndReadMetatreeXml,");
					txt = txt.Substring(i1);

					i1 = txt.IndexOf("User: ");
					txt = txt.Substring(i1);

					i1 = txt.IndexOf("\n");
					if (i1 > 0) txt = txt.Substring(0, i1);

					editor = txt;

					LogRootEditorMessage("LockAndReadMetatreeXml Exception: " + ex.Message);
				}
				catch (Exception ex2) { ex2 = ex2; }

				throw new UserQueryException("The contents tree is currently being edited by " + editor + "\r\n\r\n" + ex.Message); 
			}

// Have control, read it in

			sr = new StreamReader(RootStream);
			string treeXml = sr.ReadToEnd();
			sr = null; // don't close sr since it closes the underlying RootStream

			EditingRootXml = true;
			RootXmlSaves = 0;

			LogRootEditorMessage("LockAndReadMetatreeXml", treeXml);

			return treeXml;
		}

/// <summary>
/// Backup and save Root.xml
/// </summary>
/// <param name="treeXml"></param>

		public static void SaveMetaTreeXml(string treeXml)
		{
			if (RootXmlSaves == 0) // backup existing root.xml on first save
				BackupRootXmlFile();

			RootStream.SetLength(0);
			StreamWriter sw = new StreamWriter(RootStream);
			sw.Write(treeXml);
			sw.Flush();
			RootStream.Flush();
			sw = null; // don't close sw since it closes underlying RootStream

			RootXmlSaves++;

			LogRootEditorMessage("SaveMetatreeXml", treeXml);

			return;
		}

/// <summary>
/// Release Root.xml from editing
/// </summary>

		public static void ReleaseMetaTreeXml()
		{
			if (RootStream != null)
			{
				RootStream.Close();
				RootStream = null;
			}

			EditingRootXml = false;
			RootXmlSaves = 0;

			LogRootEditorMessage("ReleaseMetatreeXml");

			return;
		}

/// <summary>
/// Backup the current root.xml
/// </summary>
/// <returns></returns>

		static void BackupRootXmlFile()
		{
			const int MaxBackups = 100;
			int backupNumber = 0;

			try // get any existing backup number
			{
				StreamReader sr = new StreamReader(RootCurrentBackupFile);
				string txt = sr.ReadLine();
				sr.Close();
				int.TryParse(txt, out backupNumber);
			}
			catch (Exception ex) { ex = ex; }

			try
			{
				backupNumber += 1;
				if (backupNumber > MaxBackups) backupNumber = 1;

				StreamWriter sw = new StreamWriter(RootCurrentBackupFile);
				sw.WriteLine(backupNumber);
				sw.Close();
				
				string txt = String.Format("{0:000}", backupNumber);
				string backupFilePath = TreeDirectory + @"\Root" + txt + ".xml";

				File.Copy(RootXmlFile, backupFilePath, true); // copy existing root to backup

				return;
			}

			catch (Exception ex) { throw new Exception (ex.Message, ex); }
		}

		/// <summary>
		/// Log editor message
		/// </summary>
		/// <param name="msg"></param>

		static void LogRootEditorMessage(
			string msg,
			string treeXml)
		{
			int nodes = Lex.CountSubstring(treeXml, "<node");
			int children = Lex.CountSubstring(treeXml, "<child");
			msg += ", Parent nodes: " + nodes + ", Child nodes: " + children + ", Length: " + treeXml.Length;
			LogRootEditorMessage(msg);
			return;
		}

		/// <summary>
		/// Log editor message
		/// </summary>
		/// <param name="msg"></param>

		static void LogRootEditorMessage(string msg)
		{
			if (RootEditorLogFile == null)
				RootEditorLogFile = new LogFile(RootEditorLogFileName);

			UserInfo ui = Security.UserInfo;
			msg += ", User: " + ui.FullName + " (" + ui.UserDomainName + @"\" + ui.UserName + "), Process: " + Process.GetCurrentProcess().Id;
			RootEditorLogFile.Message(msg);
			return;
		}

	} // end of class
} // end of namespace
