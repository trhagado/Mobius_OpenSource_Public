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
		/// Build platform / target / assay metatree
		/// </summary>

		public static void BuildPlatformMetaTree()
		{
			string assayId, assayDb;
			string sql = @"
				select gene_fmly, gene_symbl, assy_nm, assy_db, assy_id_txt 
				from mbs_owner.cmn_assy_atrbts 
				where gene_fmly is not null and gene_symbl is not null and assy_nm is not null 
				order by lower(gene_fmly), lower(gene_symbl), lower(assy_nm)";

			sql = AssayAttributesDao.AdjustAssayAttrsTableName(sql);

			try
			{
				DbCommandMx drd = new DbCommandMx();
				drd.Prepare(sql);
				drd.ExecuteReader();

				string family = "";
				string targetSymbol = "";
				string assayName = "";

				MetaTreeNode familyMtn = null, targetMtn = null, tsMtn = null, assayMtn = null;

				while (drd.Read())
				{
					string nextFamily = drd.GetString(0);
					if (Lex.Ne(family, nextFamily)) // get current family node
					{
						family = nextFamily;
						string familyNodeName = "GFTA_" + family.Replace(" ", "_"); // build name to match node
						familyMtn = MetaTreeFactory.GetNode(familyNodeName);
					}

					string nextTargetSymbol = drd.GetString(1);
					//					if (Lex.Eq(nextTargetSymbol, "Adr1a")) nextTargetSymbol = nextTargetSymbol; // debug
					if (Lex.Ne(targetSymbol, nextTargetSymbol)) // add new target node if going to new target
					{
						targetSymbol = nextTargetSymbol;
						targetMtn = new MetaTreeNode();
						targetMtn.Type = MetaTreeNodeType.Target;
						targetMtn.Name = "TGT_ASSYS_" + targetSymbol.ToUpper();
						targetMtn.Label = targetSymbol;
						targetMtn.Target = targetMtn.Name;
						MetaTreeFactory.AddNode(targetMtn);
						if (familyMtn != null) familyMtn.Nodes.Add(targetMtn);

						tsMtn = new MetaTreeNode(); // add node for summary view by gene symbol
						tsMtn.Type = MetaTreeNodeType.MetaTable;
						tsMtn.Name = MultiDbAssayDataNames.BasePivotTablePrefix + targetSymbol.ToUpper();
						tsMtn.Label = targetSymbol + " Assay Results Summary";
						tsMtn.Target = tsMtn.Name;

						MetaTreeNode mtn2 = MetaTreeFactory.GetNode(tsMtn.Name); // node with this name already exist?
						if (mtn2 == null) MetaTreeFactory.AddNode(tsMtn);// add to tree if doesn't exist
						else tsMtn = mtn2; // use existing node otherwise since this label doesn't have priority
						if (targetMtn != null) targetMtn.Nodes.Add(tsMtn);
					}

					string nextAssayName = drd.GetString(2);
					if (Lex.Ne(assayName, nextAssayName))
					{
						assayName = nextAssayName;
						assayDb = drd.GetString(3);
						assayId = drd.GetString(4);
						assayMtn = new MetaTreeNode();
						assayMtn.Type = MetaTreeNodeType.MetaTable;
						assayMtn.Name = assayId;
						assayMtn.Target = assayMtn.Name;
						assayMtn.Label = assayName;
						MetaTreeNode mtn2 = MetaTreeFactory.GetNode(assayMtn.Name); // node with this name already exist?
						if (mtn2 == null) MetaTreeFactory.AddNode(assayMtn);// add to tree if doesn't exist
						else assayMtn = mtn2; // use existing node otherwise since this label doesn't have priority
						if (targetMtn != null) targetMtn.Nodes.Add(assayMtn);
					}
				}
				drd.CloseReader();
				drd.Dispose();
			}
			catch (Exception ex)
			{
				DebugLog.Message("TargetAssayMetafactory.Build Error:\r\n" + DebugLog.FormatExceptionMessage(ex));
				return;
			}
		}

		/// <summary>
		/// Build pathway / target / assay metatree
		/// Depends on BuildPlatformMetaTree being called first
		/// and a node named Pathway_View already existing in the tree.
		/// </summary>

		public static void BuildPathwayMetaTree()
		{
			MetaTreeNode pathwayMtn = null, targetMtn;
			StreamReader sr;

			MetaTreeNode pathwayViewRoot = MetaTreeFactory.GetNode("Pathway_View");
			if (pathwayViewRoot == null) return;

			string fileName = TargetMapDao.TargetMapDir + @"\pathway2gene.csv";
			if (!File.Exists(fileName)) return;

			try { sr = new StreamReader(fileName); }
			catch (Exception ex) { return; }

			string rec = sr.ReadLine(); // read header line
			if (rec == null)
			{
				sr.Close();
				return;
			}

			while (true)
			{
				rec = sr.ReadLine();
				if (rec == null) break;

				List<string> toks = // Rec format: pathway, pathway_name, Target_Entrez_Gene_Id, target_gene_symbol, gene_family
						Csv.SplitCsvString(rec);

				string pathwayName = toks[0].Trim();
				string pathwayCaption = toks[1].Trim();
				string targetSymbol = toks[3].Trim();

				string nodeName = "PATHWAY_" + pathwayName;
				pathwayMtn = MetaTreeFactory.GetNode(nodeName); // node with this name already exist?

				if (pathwayMtn == null) // add new pathway node
				{
					pathwayMtn = new MetaTreeNode();
					pathwayMtn.Type = MetaTreeNodeType.SystemFolder;
					pathwayMtn.Name = nodeName;
					pathwayMtn.Label = pathwayCaption + " (" + pathwayName + ")";
					MetaTreeFactory.AddNode(pathwayMtn);
					pathwayViewRoot.Nodes.Add(pathwayMtn);
				}

				nodeName = "TGT_ASSYS_" + targetSymbol.ToUpper();
				targetMtn = MetaTreeFactory.GetNode(nodeName);
				if (targetMtn == null) continue;
				pathwayMtn.Nodes.Add(targetMtn);
			}

			sr.Close();
			return;
		}

		/// <summary>
		/// Filter out the nodes that should be hidden from the user
		/// </summary>
		/// <param name="nodes"></param>

		public static void FilterNodesForUser(Dictionary<string, MetaTreeNode> nodes)
		{
			HashSet<string> allowedMts = ClientState.UserInfo?.RestrictedViewAllowedMetaTables;
			HashSet<string> blockedMts = ClientState.UserInfo?.GenerallyRestrictedMetatables;

			if (!nodes.ContainsKey("ROOT")) return;

			MetaTreeNode root = nodes["ROOT"];

			try
			{
				if (blockedMts != null)
					FilterRestrictedMetatableNodeForUser(root, blockedMts, nodes);

				if (allowedMts != null)
					FilterNodeForUser(root, allowedMts, nodes);
			}

			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}

			return;
		}
		/// <summary>
		/// Filter Restricted Metatable nodes that should be hidden from the user
		/// </summary>
		/// <param name="node"></param>
		/// <param name="blockedMts"></param>
		/// <param name="nodes"></param>
		static void FilterRestrictedMetatableNodeForUser(
				MetaTreeNode node,
				HashSet<string> blockedMts,
				Dictionary<string, MetaTreeNode> nodes)
		{
			MetaTreeNode[] children = node.Nodes.ToArray();
			for (int ci = 0; ci < children.Length; ci++)
			{
				MetaTreeNode child = children[ci];

				if (MetaTreeNode.IsDataTableNodeType(child.Type)) // see if metatable allowed
				{
					string mtName = child.Target.Trim().ToUpper();

					if (blockedMts != null && !blockedMts.Contains(mtName))
					{
						continue;
					}

					else
					{
						node.Nodes.Remove(child);

						if (nodes.ContainsKey(mtName))
							nodes.Remove(mtName);
					}
				}

				else if (!child.IsFolderType) // non-metatable leaf node
				{
					string nodeName = child.Name.Trim().ToUpper(); // see if node name in included list
					if ((blockedMts != null && !blockedMts.Contains(nodeName)))
					{
						continue;
					}

					else
					{
						node.Nodes.Remove(child);

						if (nodes.ContainsKey(child.Name))
							nodes.Remove(child.Name);
					}
				}

				else if (child.IsFolderType) // folder node
				{
					FilterRestrictedMetatableNodeForUser(child, blockedMts, nodes);

					if (!blockedMts.Contains(child.Name))
					{
						continue;
					}
					else
					{
						node.Nodes.Remove(child);
						if (nodes.ContainsKey(child.Name))
							nodes.Remove(child.Name);
					}

				}


			}
		}

		static void FilterNodeForUser(
			MetaTreeNode node,
			HashSet<string> allowedMts,
			Dictionary<string, MetaTreeNode> nodes)
		{
			MetaTreeNode[] children = node.Nodes.ToArray();
			for (int ci = 0; ci < children.Length; ci++)
			{
				MetaTreeNode child = children[ci];

				if (MetaTreeNode.IsDataTableNodeType(child.Type)) // see if metatable allowed
				{
					string mtName = child.Target.Trim().ToUpper();
					if (allowedMts.Contains(mtName))
					{
						continue;
					}

					else
					{
						node.Nodes.Remove(child);
					}
				}

				else if (!child.IsFolderType) // non-metatable leaf node
				{
					string nodeName = child.Name.Trim().ToUpper(); // see if node name in included list
					if (allowedMts.Contains(nodeName))
					{
						continue;
					}

					else
					{
						node.Nodes.Remove(child);
					}
				}

				else if (child.IsFolderType) // folder node
				{
					FilterNodeForUser(child, allowedMts, nodes);

					bool removeChild = true; // remove the folder if only standard two tables in folder

					foreach (MetaTreeNode ccNode in child.Nodes)
					{
						if (Lex.Eq(ccNode.Target, "<standardTable1>") && Lex.Ne(child.Name, "<standardTable1>")) { }

						else if (Lex.Eq(ccNode.Target, "<standardTable2>") && Lex.Ne(child.Name, "<standardTable2>")) { }

						else removeChild = false; // keep the child
					}

					if (removeChild)
					{
						node.Nodes.Remove(child);
						if (nodes.ContainsKey(child.Name))
							nodes.Remove(child.Name);
					}
				}
			}
		}

	} // MetaTreeFactory

}