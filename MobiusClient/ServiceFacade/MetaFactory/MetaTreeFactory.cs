using Mobius.ComOps;
using Mobius.Data;
using Mobius.MetaFactoryNamespace;
using Mfn = Mobius.MetaFactoryNamespace;
using Mobius.UAL;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

using ServiceTypes = Mobius.Services.Types;
using Mobius.Services.Native;
using Mobius.Services.Native.ServiceOpCodes;

namespace Mobius.ServiceFacade
{
	/// <summary>
	/// MetaTree builder
	/// </summary>

	public class MetaTreeFactory : IMetaTreeFactory
	{

		Mfn.MetaTreeFactory MetaTreeFactoryInstance; // ref to real factory

		const string DefaultCachedTreeFile = "CachedTree.txt";

		/// <summary>
		/// Constructor
		/// </summary>

		public MetaTreeFactory()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				//no action required...
			}
			else { }; // nothing to do
		}


		/// <summary>
		/// Build & return the full MetaTree, reset cache and retry if fails on 1st attempt
		/// </summary>

		public Dictionary<string, MetaTreeNode> GetMetaTree()
		{
			Dictionary<string, MetaTreeNode> nodes = null;

			if (Mfn.MetaTreeFactory.Nodes != null) // just return nodes if already have
				return Mfn.MetaTreeFactory.Nodes;

			try { nodes = GetMetaTree2(); }
			catch (Exception ex)
			{
				MarkCacheForRebuild(); // reset cache and try again
				nodes = GetMetaTree2();
			}

			Mfn.MetaTreeFactory.FilterNodesForUser(nodes);
			return nodes;
		}

		/// <summary>
		/// Build & return the full MetaTree
		/// </summary>
		/// 
		public Dictionary<string, MetaTreeNode> GetMetaTree2()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusMetaDataService,
					MobiusMetaDataService.Build,
					new object[] { false });

				// Get the cache from the server if newer than the client cache

				string cacheFile = ServicesIniFile.Read("CachedTreeFile", DefaultCachedTreeFile);
				string serverCacheFile = @"<MetaDataDir>\MetaTrees\" + cacheFile;

				string clientCacheFile = ClientDirs.CacheDir + @"\" + cacheFile;
				//ServerFile.Get(serverCacheFile, clientCacheFile); // debug, always get
				bool changed = ServerFile.GetIfChanged(serverCacheFile, clientCacheFile); // just get if changed

				// Build the tree from the local copy of the cache

				MetaFactoryNamespace.MetaTreeFactory.BuildFromCache(ClientDirs.CacheDir, false);

				return Mfn.MetaTreeFactory.Nodes;
			}

			else
			{
				Mfn.MetaTreeFactory.Build();
				return Mfn.MetaTreeFactory.Nodes;
			}
		}

		/// <summary>
		/// Get the full MetaTree from the specified root file. 
		/// Used for development and testing of the tree.
		/// This method operates in the client only and does not affect the server.
		/// Building of tree extensions is turned off for speed and to avoid the need for the client to be set up for Oracle access
		/// </summary>
		/// <param name="treeRootPath"></param>
		/// <returns></returns>

		public Dictionary<string, MetaTreeNode> GetMetaTree(string treeRootPath)
		{
			bool attemptToBuildFromCache = false;
			bool quickBuild = true;
			Mfn.MetaTreeFactory.Build(attemptToBuildFromCache, quickBuild, treeRootPath); // force build from specified file
			return Mfn.MetaTreeFactory.Nodes;
		}

		/// <summary>
		/// Quickly build the metatree from Root.xml without the usual extensions
		/// </summary>
		/// <param name="rootXml"></param>
		/// <returns></returns>

		public Dictionary<string, MetaTreeNode> BuildQuickMetaTree(string rootXmlFileName)
		{
			bool attemptToBuildFromCache = false;
			bool quickBuild = true;
			Mfn.MetaTreeFactory.Build(attemptToBuildFromCache, quickBuild, rootXmlFileName);
			return Mfn.MetaTreeFactory.Nodes;
		}

		/// <summary>
		/// Get MetaTree in xml format
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
			string treeXml = null;

			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusMetaDataService,
					MobiusMetaDataService.GetMetaTreeXml,
					new object[] { rootNodeName, includeCurrentUsersObjects, includeAllUsersObjects });

				if (resultObject != null && resultObject.Value != null)
					treeXml = resultObject.Value.ToString();

				return treeXml;
			}

			else
			{
				treeXml = Mobius.MetaFactoryNamespace.MetaTreeFactory.GetMetatreeXml(rootNodeName, includeCurrentUsersObjects, includeAllUsersObjects);
				return treeXml;
			}
		}

		/// <summary>
		/// Get MetaTree in xml format for editing
		/// </summary>
		/// <returns>Tree in XML</returns>

		public static string LockAndReadMetatreeXml()
		{
			string treeXml = null;

			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusMetaDataService,
					MobiusMetaDataService.LockAndReadMetatreeXml,
					null);

				if (resultObject != null && resultObject.Value != null)
				{
					byte[] ba = resultObject.Value as byte[];
					treeXml = Mobius.ComOps.GZip.Decompress(ba);
				}

				return treeXml;
			}

			else
			{
				treeXml = Mobius.MetaFactoryNamespace.MetaTreeFactory.LockAndReadMetatreeXml();
				return treeXml;
			}
		}

		/// <summary>
		/// Save edited metatree xml
		/// </summary>
		/// <param name="treeXml"></param>

		public static void SaveMetaTreeXml(string treeXml)
		{

			if (ServiceFacade.UseRemoteServices)
			{
				byte[] ba = Mobius.ComOps.GZip.Compress(treeXml);

				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusMetaDataService,
					MobiusMetaDataService.SaveMetaTreeXml,
					new object[] { ba });

				return;
			}

			else
			{
				Mobius.MetaFactoryNamespace.MetaTreeFactory.SaveMetaTreeXml(treeXml);
				return;
			}
		}

		/// <summary>
		/// Release the editing of the metatree xml
		/// </summary>

		public static void ReleaseMetaTreeXml()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusMetaDataService,
					MobiusMetaDataService.ReleaseMetaTreeXml,
					null);

				return;
			}

			else
			{
				Mobius.MetaFactoryNamespace.MetaTreeFactory.ReleaseMetaTreeXml();
				return;
			}
		}


		/// <summary>
		/// Wipe the metatree clean (in preparation for a reload)
		/// </summary>

		void IMetaTreeFactory.Reset()
		{
			Mfn.MetaTreeFactory.Reset();
		}

		/// <summary>
		/// Build & return the full MetaTree
		/// </summary>

		public Dictionary<string, MetaTreeNode> ReloadMetaTree()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusMetaDataService,
					MobiusMetaDataService.GetFullTree,
					null);

				Dictionary<string, MetaTreeNode> metaTree = (resultObject != null) ? (Dictionary<string, MetaTreeNode>)resultObject.Value : null;
				return metaTree;
			}

			else
			{
				if (MetaTreeFactoryInstance == null)
					MetaTreeFactoryInstance = new Mobius.MetaFactoryNamespace.MetaTreeFactory();
				return MetaTreeFactoryInstance.ReloadMetaTree();
			}
		}

		/// <summary>
		/// Mark the MetaTree cache for rebuilding
		/// </summary>

		public void MarkCacheForRebuild()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusMetaDataService,
					MobiusMetaDataService.MarkCacheForRebuild,
					null);
				return;
			}

			else
			{
				if (MetaTreeFactoryInstance == null)
					MetaTreeFactoryInstance = new Mobius.MetaFactoryNamespace.MetaTreeFactory();

				MetaTreeFactoryInstance.MarkCacheForRebuild();
			}
		}

		/// <summary>
		/// Get subtree starting at specified node
		/// </summary>
		/// <param name="rootNodeName"></param>
		/// <returns></returns>

		public MetaTreeNode GetAfsSubtree(string rootNodeName)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
						ServiceCodes.MobiusMetaDataService,
						MobiusMetaDataService.GetAfsSubtree,
						new object[] { rootNodeName });

				if (resultObject == null) return null;
				MetaTreeNode mtn = resultObject.Value as MetaTreeNode;
				return mtn;
			}

			else
			{
				if (MetaTreeFactoryInstance == null)
					MetaTreeFactoryInstance = new Mobius.MetaFactoryNamespace.MetaTreeFactory();
				return MetaTreeFactoryInstance.GetAfsSubtree(rootNodeName);
			}
		}

	}
}
