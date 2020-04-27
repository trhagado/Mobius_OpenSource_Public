using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.Data
{

/// <summary>
/// Common MetaTree operations
/// </summary>

	public class MetaTree
	{
		public static IMetaTreeFactory MetaTreeFactory; // factory to call to build MetaTree
		public static Dictionary<string, MetaTreeNode> Nodes; // map of all tree nodes keyed by name

		/// <summary>
		/// Clear metatree data
		/// </summary>

		public static void Reset()
		{
			if (MetaTreeFactory == null) throw new Exception("MetaTreeFactory instance is not defined");
			MetaTreeFactory.Reset();
			Nodes = null;
			return;
		}

		/// <summary>
		/// Get the full metatree
		/// </summary>

		public static Dictionary<string, MetaTreeNode> GetMetaTree()
		{
			if (MetaTreeFactory == null) throw new Exception("MetaTreeFactory instance is not defined");
			Nodes = MetaTreeFactory.GetMetaTree();

			return Nodes;
		}

		/// <summary>
		/// Performs an alternate "get" appropriate for reloading the metatree
		/// </summary>
		public static void ReloadMetaTree()
		{
			if (MetaTreeFactory == null) throw new Exception("MetaTreeFactory instance is not defined");
			Nodes = MetaTreeFactory.ReloadMetaTree();
		}

		/// <summary>
		/// Build the metatree from the specified root file
		/// </summary>

		public static Dictionary<string, MetaTreeNode> GetMetaTree (
			string metaTreePath)
		{
			if (MetaTreeFactory == null) throw new Exception("MetaTreeFactory instance is not defined");
			Nodes = MetaTreeFactory.GetMetaTree(metaTreePath);
			return Nodes;
		}

/// <summary>
/// Recursively update nodes dictionary
/// </summary>
/// <param name="nodes"></param>
/// <param name="node"></param>

		public static void UpdateNodesDictForSubtree(
			Dictionary<string, MetaTreeNode> nodes,
			MetaTreeNode node)
		{
			//if (node.Label == "Libraries") node = node; // debug

			if (node.IsUserObjectType) return; // don't include user objects

			//if (!nodes.ContainsKey(node.Name)) // (no, should always update since may be new version with old name

			nodes[node.Name] = node;

			foreach (MetaTreeNode child in node.Nodes)
				UpdateNodesDictForSubtree(nodes, child);
		}

/// <summary>
/// Add a node to the tree following initial build
/// </summary>
/// <param name="node"></param>

		public static void AddNode(MetaTreeNode node)
		{
			if (MetaTreeFactory == null) DebugMx.DataException("MetaTreeFactory instance is not defined");
			if (Nodes == null) DebugMx.DataException("Nodes dictionary is null");
			if (node == null) DebugMx.DataException("Node parameter is null");
			if (Lex.IsNullOrEmpty(node.Name)) DebugMx.DataException("Node name is not defined");

			if (Nodes.ContainsKey(node.Name.ToUpper())) return;
			Nodes.Add(node.Name.ToUpper(), node);
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
			MetaTreeNode mtn;

			if (Nodes == null || name == null) return null;

			name = name.Trim().ToUpper();
			if (!Nodes.ContainsKey(name)) return null;
			mtn = Nodes[name];

			// public static string GetStats()
			if (Math.Abs(1) == 2) // disabled
			{
				Dictionary<MetaTreeNodeType, int> nodeStats = new Dictionary<MetaTreeNodeType, int>();
				foreach (KeyValuePair<string, MetaTreeNode> kv in Nodes)
				{
					string mtnName = kv.Key;
					if (Lex.StartsWith(mtnName, "HIDDEN")) continue;
					MetaTreeNode mtn2 = kv.Value;
					if (!nodeStats.ContainsKey(mtn2.Type))
						nodeStats[mtn2.Type] = 0;

					nodeStats[mtn2.Type]++;
				}
				nodeStats = nodeStats;
			}

			return mtn;
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

			foreach (MetaTreeNode mtn in Nodes.Values) // look for exact match
			{
				if (Lex.Eq(mtn.Label, label)) return mtn;
			}

			foreach (MetaTreeNode mtn in Nodes.Values) // look for label with suffix match
			{
				if (Lex.StartsWith(mtn.Label, label))
				{
					foreach (string suffix in MetaTable.NameSuffixes)
					{
					if (Lex.Eq(mtn.Label, label + suffix)) return mtn;
					}
				}
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
			if (Lex.IsNullOrEmpty(target)) return null;

			foreach (MetaTreeNode mtn in Nodes.Values)
			{
				if (Lex.Eq(mtn.Target, target)) return mtn;
			}

			return null;
		}

/// <summary>
/// Get a list of the parent nodes for the specified node
/// </summary>
/// <param name="mtn"></param>
/// <returns></returns>

		public static List<MetaTreeNode> GetParents (
			MetaTreeNode mtn)
		{
			List<MetaTreeNode> parents = new List<MetaTreeNode>();

			if (Nodes == null) return parents;

			foreach (MetaTreeNode parent in Nodes.Values)
			{
				foreach (MetaTreeNode child in parent.Nodes)
				{
					if (Lex.Eq(mtn.Name, child.Name)) // compare on name not address
					{
						parents.Add(parent);
					}
				}
			}

			return parents;
		}


	}

	/// <summary>
	/// Interface to MetaTreeFactory.
	/// Used by the client to access the factory via the service interface and
	/// by the service to access the factory internally as needed.
	/// </summary>

	public interface IMetaTreeFactory
	{

/// <summary>
/// Get the full MetaTree
/// </summary>
/// <returns></returns>

		Dictionary<string, MetaTreeNode> GetMetaTree();

/// <summary>
/// Get the full MetaTree from the specified root file
/// </summary>
		/// <returns></returns>

		Dictionary<string, MetaTreeNode> GetMetaTree(string rootFileName);

/// <summary>
/// Wipe the metatree clean (in preparation for a reload)
/// </summary>

		void Reset();

/// <summary>
/// Gets the full MetaTree (without relying on a cached version of the metatree)
/// </summary>

		Dictionary<string, MetaTreeNode> ReloadMetaTree();

		/// <summary>
		/// Mark the MetaTree cache for rebuilding
		/// </summary>

		void MarkCacheForRebuild();

/// <summary>
/// Get the a subtree from the Flow Scheme contents tree database
/// </summary>
/// <param name="nodeName"></param>
/// <returns></returns>

		MetaTreeNode GetAfsSubtree(string nodeName);


/// <summary>
/// Quickly build the metatree from Root.xml without the usual extensions
/// </summary>
		/// <param name="rootXmlFileName"></param>
/// <returns></returns>

		Dictionary<string, MetaTreeNode> BuildQuickMetaTree(string rootXmlFileName);

	}

}
