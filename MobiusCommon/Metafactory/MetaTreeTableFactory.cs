using Mobius.UAL;
using Mobius.ComOps;
using Mobius.Data;
using Mobius.QueryEngineLibrary;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Mobius.MetaFactoryNamespace
{

	public partial class MetaTreeFactory
	{

		/// <summary>
		/// Build a table that contains one row for each child in each folder in the MetaTree
		/// </summary>

		public static string BuildMetaTreeTable()
		{
			List<MetaTreeNodeTableRow> entries = new List<MetaTreeNodeTableRow>();
			HashSet<string> processed = new HashSet<string>();

			foreach (KeyValuePair<string, MetaTreeNode> kv in Nodes)
			{
				MetaTreeNode n = kv.Value;

				if (!n.IsFolderType) continue;
				if (processed.Contains(n.Name)) continue;
				processed.Add(n.Name);

				foreach (MetaTreeNode cn in n.Nodes)
				{
					MetaTreeNodeTableRow ne = new MetaTreeNodeTableRow();

					ne.ParentType = n.Type;
					ne.ParentName = n.Name;
					ne.ParentLabel = n.Label;

					ne.ChildType = cn.Type;
					ne.ChildName = cn.Name;
					ne.ChildLabel = cn.Label;

					entries.Add(ne);
				}
			}

			return "Nodes processed: " + processed.Count + ", Rows stored: " +  entries.Count;
		}

	}

	/// <summary>
	/// Class used to serialize nodes to database table
	/// </summary>

	public class MetaTreeNodeTableRow
	{
		public MetaTreeNodeType ParentType;
		public string ParentName = "";
		public string ParentLabel = "";

		public MetaTreeNodeType ChildType;
		public string ChildName = "";
		public string ChildLabel = "";

	}



}
