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
	/// Project MetaTree creation and editing members
	/// </summary>

	public partial class MetaTreeFactory
	{

		/// <summary>
		/// Get the a subtree from the Flow Scheme contents tree database
		/// </summary>
		/// <param name="projectNodeName"></param>
		/// <returns></returns>

		public MetaTreeNode GetAfsSubtree(string projectNodeName)
		{
			MetaTreeNode mtn = null;

			try
			{
				Dictionary<string, MetaTreeNode> nodes = BuildAfsProjectNodes(projectNodeName);
				if (nodes == null || nodes.Count == 0) return null;
				foreach (MetaTreeNode mtn0 in nodes.Values) // should be just one node
					mtn = mtn0;

				return mtn;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}

		}

		/// <summary>
		/// Build a set of the therapeutic area, drug hunting team nodes and their underlying Afs project nodes
		/// </summary>
		/// <returns></returns>

		public Dictionary<string, MetaTreeNode> BuildAfsProjectNodes(
			string projectNodeName)
		{
			Dictionary<string, MetaTreeNode> pNodes = new Dictionary<string, MetaTreeNode>();

			BuildAfsProjectNodes(projectNodeName, pNodes); // build basic project nodes

			BuildAfsAssayNodes(projectNodeName, pNodes); // add the assay nodes

			BuildAfsLibraryNodes(projectNodeName, pNodes); // add the library nodes

			return pNodes;
		}

/// <summary>
/// Build the basic projects nodes without assay, library or other assoc info
/// </summary>
/// <param name="projectNodeName"></param>
/// <param name="pNodes"></param>

		public void BuildAfsProjectNodes(
			string projectNodeName,
			Dictionary<string, MetaTreeNode> pNodes)
		{
			MetaTreeNode projNode, assayNode;

			List<AfsProject> projects = AfsProject.Select(projectNodeName);
			foreach (AfsProject p in projects)
			{
				if (!pNodes.ContainsKey(p.MbsProjectName)) // add project node
				{
					projNode = new MetaTreeNode(MetaTreeNodeType.Project);
					projNode.Name = projNode.Target = p.MbsProjectName;
					projNode.Label = p.ProjectLabel;
					projNode.Owner = "AFS";
					pNodes[p.MbsProjectName] = projNode;

					if (Nodes.ContainsKey(p.MbsDhtFolderName)) // link to project parent if exists
						projNode.Parent = Nodes[p.MbsDhtFolderName];
				}
			}

			return;
		}


/// <summary>
/// Build assay nodes
/// </summary>
/// <param name="projectNodeName"></param>
/// <param name="pNodes"></param>

		public void BuildAfsAssayNodes(
			string projectNodeName,
			Dictionary<string, MetaTreeNode> pNodes)
		{
			MetaTreeNode projNode, assayNode;

			List<AfsAssay> assays =  AfsAssay.Select(projectNodeName);
			foreach (AfsAssay a in assays)
			{
				if (!pNodes.ContainsKey(a.MbsProjectName)) continue;

				projNode = pNodes[a.MbsProjectName];

				assayNode = new MetaTreeNode(MetaTreeNodeType.MetaTable);
				assayNode.Name = a.MbsProjectName + "_" + a.AssayDb + "_" + a.AssayId; // assign unique name for project so label with assay use gets passed through

				assayNode.Target = a.AssayDb + "_" + a.AssayId; // target must match a metatable name

				assayNode.Label = a.AssayLabel;

				if (!Lex.IsNullOrEmpty(a.AssayUse)) // append any assay use
					assayNode.Label += " (" + a.AssayUse + ")";

				assayNode.Owner = "AFS";
				projNode.Nodes.Add(assayNode);
			}

			return;
		}

/// <summary>
/// Read assay information
/// </summary>
/// <param name="projectNodeName"></param>
/// <param name="pNodes"></param>

		public void BuildAfsLibraryNodes(
			string projectNodeName,
			Dictionary<string, MetaTreeNode> pNodes)
		{
			MetaTreeNode projNode, libFolderNode, libNode;
			StringBuilder sb = new StringBuilder(64);

			List<AfsLibrary> libs = AfsLibrary.Select(projectNodeName);

			string fileName = MetaTableFactory.MetaTableXmlFolder + @"\LibraryStats.txt";
			foreach (AfsLibrary l in libs)
			{
				if (!pNodes.ContainsKey(l.MbsProjectName)) continue;

				projNode = pNodes[l.MbsProjectName];

				List<MetaTreeNode> projNodes = projNode.Nodes;

				if (projNodes.Count == 0 || projNodes[projNodes.Count-1].Label != "Libraries")
				{ // add libraries folder if doesn't exist
					libFolderNode = new MetaTreeNode(MetaTreeNodeType.SystemFolder);
					libFolderNode.Name = projNode.Name + "_LIBRARIES";
					libFolderNode.Label = "Libraries";
					libFolderNode.Owner = "AFS";
					projNodes.Add(libFolderNode);
				}
				else libFolderNode = projNodes[projNodes.Count-1];

				libNode = new MetaTreeNode(MetaTreeNodeType.Library); // add node for library
				libNode.Name = libNode.Target = "LIBRARY_" + l.LibId;

				libNode.Label = l.LibLabel;
				libNode.Owner = "AFS";
				libFolderNode.Nodes.Add(libNode);
			}

			return;
		}

		/// <summary>
		/// Calculate & persist library stats
		/// </summary>
		/// <returns></returns>

		public static string UpdateLibraryStatistics()
		{
			Dictionary<string, MetaTableStats> stats = new Dictionary<string, MetaTableStats>();
			int libId;
			long cnt;
			DateTime dt;
			string libName, txt;

			try
			{
				DbCommandMx dao = new DbCommandMx();

				string sql = // get count and date (must use crt_timestamp) for each library
				@"
				select 
				 l.lib_id, 
				 count(*),
				 l.crt_timestamp
				from 
				 corp_owner.corp_library l,
				 corp_owner.corp_library_substance ls
				where ls.lib_id (+) = l.lib_id
				group by l.lib_id, l.crt_timestamp 
				order by l.lib_id";

				dao.Prepare(sql);
				dao.ExecuteReader();
				while (dao.Read())
				{
					libId = dao.GetInt(0);
					libName = "LIBRARY_" + libId;

					if (!stats.ContainsKey(libName)) stats[libName] = new MetaTableStats();

					cnt = dao.GetLong(1);
					stats[libName].RowCount = cnt;
					dt = dao.GetDateTime(2);
					stats[libName].UpdateDateTime = dt;
				}

				dao.CloseReader();
				dao.Dispose();

				string fileName = MetaTableFactory.MetaTableXmlFolder + @"\LibraryStats";
				MetaTableFactory.WriteMetaTableStats(stats, fileName);
			}
			catch (Exception ex)
			{
				throw ex;
			}

			return "Updated statistics for " + stats.Count + " libraries";
		}

		/// <summary>
		/// Return true if node name is a mobius AFS project node name
		/// </summary>
		/// <param name="projNodeName"></param>
		/// <returns></returns>

		public static bool IsAfsNodeName(string projNodeName)
		{
			int id = AfsProject.GetAfsProjectId(projNodeName);
			return id > 0 ? true : false;
		}

	}

/// <summary>
/// AfsProject
/// </summary>

	public class AfsProject
	{
		public int ProjId = -1;

		public string MbsProjectName = "";
		public string ProjectLabel = "";
		public string MbsDhtFolderName = "";
		public string DhtFolderLabel = "";

		public string PlatformName = "";
		public string Description = "";
		public string ProjectFlowScheme = "";
		public List<AfsProjMeta> ProjMeta = new List<AfsProjMeta>();
		public List<AfsTarget> Targets = new List<AfsTarget>();
		public List<AfsAssay> Assays = new List<AfsAssay>();
		public List<AfsLibrary> Libraries = new List<AfsLibrary>();

		public const string AfsTableSchema = "DEV_MBS_OWNER";

		/// <summary>
		/// Select basic project info
		/// </summary>
		/// <param name="projId"></param>
		/// <returns></returns>

		public static List<AfsProject> Select(string projNodeName)
		{
			string projCriteria = !Lex.IsNullOrEmpty(projNodeName) ? "MBS_PROJECT_CODE = '" + projNodeName.ToUpper() + "'" : "1 = 1";
			return SelectWithCriteria(projCriteria);
		}

		/// <summary>
		/// Select basic project info for the supplied AFS project ID
		/// </summary>
		/// <param name="projId"></param>
		/// <returns></returns>

		public static List<AfsProject> Select(int projId)
		{
			string projCriteria = "p.PROJ_ID = " + projId;
			return SelectWithCriteria(projCriteria);
		}

		/// <summary>
		/// Select basic project info with criteria
		/// </summary>
		/// <param name="criteria"></param>
		/// <returns></returns>

		public static List<AfsProject> SelectWithCriteria(string criteria)
		{
			MetaTreeNode mtn;

			string sql = @"
				select
					proj_id, 
					mbs_project_code,
					proj_name, 
					mbs_dht_folder_code,
					dht_folder_name, 
					platform_name
				from
					<mbs_owner>.afs_project p
				where
					afs_current = 1
					and mbs_project_code is not null
					and <criteria>
				order by upper(dht_folder_name), upper(proj_name)
			";

			sql = Lex.Replace(sql, "<mbs_owner>", AfsProject.AfsTableSchema);
			sql = Lex.Replace(sql, "<criteria>", criteria);

			List<AfsProject> projects = new List<AfsProject>();

			DbCommandMx dao = new DbCommandMx();
			dao.Prepare(sql);
			dao.ExecuteReader();
			while (dao.Read())
			{
				AfsProject p = new AfsProject();
				p.ProjId = dao.GetInt(0);

				p.MbsProjectName = dao.GetString(1).ToUpper();
				p.ProjectLabel = dao.GetString(2);
				if (Lex.IsNullOrEmpty(p.ProjectLabel))
					p.ProjectLabel = p.MbsProjectName;

				p.MbsDhtFolderName = dao.GetString(3).ToUpper();
				p.DhtFolderLabel = dao.GetString(4).ToUpper();
				if (Lex.IsNullOrEmpty(p.DhtFolderLabel))
					p.DhtFolderLabel = p.MbsDhtFolderName;

				p.PlatformName = dao.GetString(5).ToUpper();

				projects.Add(p);
			}

			dao.CloseReader();

			return projects;
		}

/// <summary>
///  Get the AFS project id, if any, associated with a project node name and return as string
/// </summary>
/// <param name="projNodeName"></param>
/// <returns></returns>

		public static string GetAfsProjectIdString(string projNodeName)
		{
			return GetAfsProjectId(projNodeName).ToString();
		}

		/// <summary>
		/// Get the AFS project id, if any, associated with a project node name
		/// </summary>
		/// <param name="projNodeName"></param>
		/// <returns></returns>

		public static int GetAfsProjectId(string projNodeName)
		{
			string sql = @"
				select proj_id
				from <mbs_owner>.afs_project 
				where afs_current = 1 and MBS_PROJECT_CODE = '" + projNodeName.ToUpper() + "'";

			sql = Lex.Replace(sql, "<mbs_owner>", AfsTableSchema);

			DbCommandMx dao = new DbCommandMx();
			dao.Prepare(sql);
			dao.ExecuteReader();
			if (!dao.Read())
			{
				dao.CloseReader();
				return -1;
			}

			int projId = dao.GetInt(0);

			dao.CloseReader();
			return projId;
		}

		/// <summary>
		/// Get the description for a project in HTML format
		/// </summary>
		/// <param name="projIdString"></param>
		/// <returns></returns>

		public static string GetProjectHtmlDescription(string projIdString)
		{
			string html = "";

			AfsProject p = AfsProject.SelectMetaData(projIdString);

			try
			{
				string templateFile = ServicesDirs.MetaDataDir + @"\MiscConfig\AFSProjectTemplate.htm";
				StreamReader sr = new StreamReader(templateFile);
				html = sr.ReadToEnd();
				sr.Close();
			}
			catch (Exception ex) { return ex.Message; }

			html = Lex.Replace(html, "[Project]", p.ProjectLabel);
			html = Lex.Replace(html, "[Platform]", p.PlatformName);
			html = Lex.Replace(html, "[Description]", p.Description);

			string targets = "";
			foreach (AfsTarget t in p.Targets)
			{
				if (Lex.IsNullOrEmpty(t.TargetName)) continue;

				if (targets != "") targets += " ,";
				targets += t.TargetName;
				if (!Lex.IsNullOrEmpty(t.TargetType))
					targets += " (" + t.TargetType + ")";
			}

			html = Lex.Replace(html, "[Targets]", targets);

			if (!Lex.IsNullOrEmpty(p.ProjectFlowScheme))
			{
				html = Lex.Replace(html, "[LinkText]", "Link");
				html = Lex.Replace(html, "[LinkArg]", p.ProjId.ToString());
			}

			else
			{
				html = Lex.Replace(html, "[LinkText]", "Not Available");
			}

// Project metadata

			string currentCat = "";
			string vals = "";
			string metaHtml = "";
			for (int i1 = 0; i1 < p.ProjMeta.Count; i1++)
			{
				AfsProjMeta m = p.ProjMeta[i1];
				if (Lex.IsNullOrEmpty(m.CategoryName) || Lex.IsNullOrEmpty(m.CategoryValue))
					continue;

				if (m.CategoryName != currentCat) // going to new category
				{
					if (vals != "") // add any vals for current category
						metaHtml += "<strong>" + currentCat + " : </strong>" + vals + "<br>";

					currentCat = m.CategoryName;
					vals = "";
				}

				if (vals != "") vals += ", ";
				vals += m.CategoryValue;
			}

			if (vals != "") // last one
				metaHtml += "<strong>" + currentCat + " : </strong>" + vals + "<br>";

			html = Lex.Replace(html, "[ProjMeta]", metaHtml);

// Assays

			metaHtml = "";
			for (int i1 = 0; i1 < p.Assays.Count; i1++)
			{
				AfsAssay a  = p.Assays[i1];

				string target = a.AssayDb + "_" + a.AssayId;
				string label = a.AssayLabel;
				MetaTreeNode mtn = MetaTree.GetNodeByTarget(target); // try to get better label from contents tree
				if (mtn != null)
				{
					label = mtn.Label + " (";

					if (!Lex.IsNullOrEmpty(a.AssayUse) && !Lex.Contains(label, a.AssayUse)) // append any assay use
						label += "Use: " + a.AssayUse + ", ";

					label += "Id: " + mtn.Target + ", ";

					label += MetaTreeNode.FormatStatistics(mtn);
					label += ")";
				}

				metaHtml += "&nbsp; &nbsp;" + label + "<br>";
			}

			html = Lex.Replace(html, "[AssayMeta]", metaHtml);

// Libraries

			metaHtml = "";
			for (int i1 = 0; i1 < p.Libraries.Count; i1++)
			{
				AfsLibrary l = p.Libraries[i1];
				metaHtml += "&nbsp; &nbsp;" + l.LibLabel + "<br>";
			}

			html = Lex.Replace(html, "[LibraryMeta]", metaHtml);

// Flowscheme if any

			if (!Lex.IsNullOrEmpty(p.ProjectFlowScheme))
				html += "\v" + p.ProjectFlowScheme;

			return html;
		}


		/// <summary>
		/// GetProjectMetaData for the supplied Mobius project name
		/// </summary>
		/// <param name="projIdString"></param>
		/// <returns></returns>

		public static AfsProject SelectMetaData(string projNodeName)
		{
			string projCriteria = !Lex.IsNullOrEmpty(projNodeName) ? "MBS_PROJECT_CODE = '" + projNodeName.ToUpper() + "'" : "1 = 1";
			return SelectMetaDataWithCriteria(projCriteria);
		}

		/// <summary>
		/// GetProjectMetaData for the supplied AFS project ID
		/// </summary>
		/// <param name="projId"></param>
		/// <returns></returns>

		public static AfsProject SelectMetaData(int projId)
		{
			string projCriteria = "p.PROJ_ID = " + projId;
			return SelectMetaDataWithCriteria(projCriteria);
		}

		/// <summary>
		/// GetProjectMetaDataWithCriteria
		/// </summary>
		/// <param name="criteria"></param>
		/// <returns></returns>

		public static AfsProject SelectMetaDataWithCriteria(string criteria)
		{
			string sql = @"
				select *
				from <mbs_owner>.afs_project 
				where afs_current = 1 and <criteria>";

			sql = Lex.Replace(sql, "<mbs_owner>", AfsTableSchema);
			sql = Lex.Replace(sql, "<criteria>", criteria);

			DbCommandMx dao = new DbCommandMx();
			dao.Prepare(sql);
			dao.ExecuteReader();
			if (!dao.Read())
			{
				dao.CloseReader();
				return null;
			}

			AfsProject p = new AfsProject();
			p.ProjId = dao.GetIntByName("PROJ_ID");
			p.ProjectLabel = dao.GetStringByName("PROJ_NAME");
			p.PlatformName = dao.GetStringByName("PLATFORM_NAME");
			p.Description = dao.GetStringByName("DESCRIPTION");
			p.ProjectFlowScheme = dao.GetClobByName("PROJ_FLOW_SCHEME");
			p.MbsDhtFolderName = dao.GetStringByName("MBS_DHT_FOLDER_CODE");
			p.MbsProjectName = dao.GetStringByName("MBS_PROJECT_CODE");

			dao.CloseReader();

			p.ProjMeta = AfsProjMeta.Select(p.ProjId);
			p.Targets = AfsTarget.Select(p.ProjId);
			p.Assays = AfsAssay.Select(p.ProjId);
			p.Libraries = AfsLibrary.Select(p.ProjId);

			return p;
		}
	}

/// <summary>
/// AfsProjMeta
/// </summary>

	public class AfsProjMeta
	{
		public int ProjId = -1;
		public string CategoryName = "";
		public string CategoryValue = "";

/// <summary>
/// Select
/// </summary>
/// <param name="projId"></param>
/// <returns></returns>

		public static List<AfsProjMeta> Select(int projId)
		{
			List<AfsProjMeta> l = new List<AfsProjMeta>();

			string sql = @"
				select *
				from <mbs_owner>.afs_proj_meta 
				where afs_current = 1 and proj_id = " + projId +
				" order by upper(category_name)";													 ;

			sql = Lex.Replace(sql, "<mbs_owner>", AfsProject.AfsTableSchema);

			DbCommandMx dao = new DbCommandMx();
			dao.Prepare(sql);
			dao.ExecuteReader();
			while (dao.Read())
			{
				AfsProjMeta m = new AfsProjMeta();
				m.ProjId = dao.GetIntByName("PROJ_ID");
				m.CategoryName = dao.GetStringByName("CATEGORY_NAME");
				m.CategoryValue = dao.GetStringByName("CATEGORY_VALUE");
				l.Add(m);
			}

			dao.CloseReader();

			return l;
		}
	}

/// <summary>
/// AfsTarget
/// </summary>

	public class AfsTarget
	{
		public int ProjId = -1;
		public string TargetName = "";
		public string TargetType = "";

		/// <summary>
		/// Select
		/// </summary>
		/// <param name="projId"></param>
		/// <returns></returns>

		public static List<AfsTarget> Select(int projId)
		{
			List<AfsTarget> l = new List<AfsTarget>();

			string sql = @"
				select *
				from <mbs_owner>.afs_target
				where afs_current = 1 and proj_id = " + projId +
				" order by upper(target_name)";

			sql = Lex.Replace(sql, "<mbs_owner>", AfsProject.AfsTableSchema);

			DbCommandMx dao = new DbCommandMx();
			dao.Prepare(sql);
			dao.ExecuteReader();
			while (dao.Read())
			{
				AfsTarget t = new AfsTarget();
				t.ProjId = dao.GetIntByName("PROJ_ID");
				t.TargetName = dao.GetStringByName("TARGET_NAME");
				t.TargetType = dao.GetStringByName("TARGET_TYPE");
				l.Add(t);
			}

			dao.CloseReader();

			return l;
		}

	}

/// <summary>
/// AfsAssay
/// </summary>

		public class AfsAssay
		{
			public int ProjId = -1;

			public string MbsProjectName = "";
			public string ProjectLabel = "";
			public string MbsDhtFolderName = "";
			public string DhtFolderLabel = "";
			public string Platform = "";

			public int AssayId = -1;
			public string AssayLabel = "";
			public string AssayDb = "";
			public string AssayUse = "";

/// <summary>
/// Select assays
/// </summary>
/// <param name="projId"></param>
/// <returns></returns>

			public static List<AfsAssay> Select(string projNodeName)
			{
				string projCriteria = !Lex.IsNullOrEmpty(projNodeName) ? "MBS_PROJECT_CODE = '" + projNodeName.ToUpper() + "'" : "1 = 1";
				return SelectWithCriteria(projCriteria);
			}

			/// <summary>
			/// GetProjectMetaData for the supplied AFS project ID
			/// </summary>
			/// <param name="projId"></param>
			/// <returns></returns>

			public static List<AfsAssay> Select(int projId)
			{
				string projCriteria = "p.PROJ_ID = " + projId;
				return SelectWithCriteria(projCriteria);
			}

/// <summary>
/// 
/// </summary>
/// <param name="criteria"></param>
/// <returns></returns>

			public static List<AfsAssay> SelectWithCriteria(string criteria)
			{
				MetaTreeNode mtn;

				string sql = @"
				select
					p.proj_id, 
					p.mbs_project_code,
					proj_name, 
					p.mbs_dht_folder_code,
					dht_folder_name, 
					platform_name, 
					assay_id, 
					assay_name, 
					assay_db,
					assay_use
				from
					<mbs_owner>.afs_project p,
					<mbs_owner>.afs_assay a
				where
					p.afs_current = 1
					and p.mbs_project_code is not null
					and a.afs_current = 1
					and a.proj_id = p.proj_id
					and <criteria>
				order by upper(dht_folder_name), upper(proj_name), upper(assay_name)
			";

				sql = Lex.Replace(sql, "<mbs_owner>", AfsProject.AfsTableSchema);
				sql = Lex.Replace(sql, "<criteria>", criteria);

				List<AfsAssay> assays = new List<AfsAssay>();

				DbCommandMx dao = new DbCommandMx();
				dao.Prepare(sql);
				dao.ExecuteReader();
				while (dao.Read())
				{
					AfsAssay a = new AfsAssay();
					a.ProjId = dao.GetInt(0);
					a.MbsProjectName = dao.GetString(1).ToUpper();
					a.ProjectLabel = dao.GetString(2);
					a.MbsDhtFolderName = dao.GetString(3).ToUpper();
					a.DhtFolderLabel = dao.GetString(4).ToUpper();
					a.Platform = dao.GetString(5).ToUpper();
					a.AssayId = dao.GetInt(6);
					a.AssayLabel = dao.GetString(7);
					a.AssayDb = dao.GetString(8).ToUpper();
					a.AssayUse = dao.GetString(9).ToUpper();

					assays.Add(a);
				}

				dao.CloseReader();

				return assays;
			}

		}

/// <summary>
/// AfsLibrary
/// </summary>

		public class AfsLibrary
		{
			public int ProjId = -1;

			public string MbsProjectName = ""; // internal Mobius node name
			public string ProjectLabel = "";
			public string MbsDhtFolderName = ""; // internal mobius dht folder node name
			public string DhtFolderLabel = "";
			public string Platform = "";

			public int LibId = -1;
			public string LibName = "";
			public string LibLabel = ""; // built up label
			public string LibUse = "";

/// <summary>
/// Select libraries
/// </summary>
/// <param name="projId"></param>
/// <returns></returns>

			public static List<AfsLibrary> Select(string projNodeName)
			{
				string projCriteria = !Lex.IsNullOrEmpty(projNodeName) ? "MBS_PROJECT_CODE = '" + projNodeName.ToUpper() + "'" : "1 = 1";
				return SelectWithCriteria(projCriteria);
			}

/// <summary>
/// Select libraries
/// </summary>
/// <param name="projId"></param>
/// <returns></returns>

			public static List<AfsLibrary> Select(int projId)
			{
				string projCriteria = "p.PROJ_ID = " + projId;
				return SelectWithCriteria(projCriteria);
			}

/// <summary>
/// SelectWithCriteria
/// </summary>
/// <param name="criteria"></param>
/// <returns></returns>

			public static List<AfsLibrary> SelectWithCriteria(string criteria)
			{
				MetaTreeNode tempMtn = new MetaTreeNode(MetaTreeNodeType.Library);
				StringBuilder sb = new StringBuilder(64);

				string sql = @"
				select
					p.proj_id, 
					p.mbs_project_code,
					proj_name, 
					p.mbs_dht_folder_code,
					dht_folder_name, 
					platform_name, 
					lib_id, 
					lib_name, 
					lib_use
				from
					<mbs_owner>.afs_project p,
					<mbs_owner>.afs_lib l
				where
					p.afs_current = 1
					and p.mbs_project_code is not null
					and l.afs_current = 1
					and l.proj_id = p.proj_id
					and <criteria>
				order by upper(dht_folder_name), upper(proj_name), upper(lib_name)
				";

				sql = Lex.Replace(sql, "<mbs_owner>", AfsProject.AfsTableSchema);
				sql = Lex.Replace(sql, "<criteria>", criteria);

				List<AfsLibrary> libs = new List<AfsLibrary>();

				DbCommandMx dao = new DbCommandMx();
				dao.Prepare(sql);
				dao.ExecuteReader();

				string fileName = MetaTableFactory.MetaTableXmlFolder + @"\LibraryStats.txt";
				Dictionary<string, MetaTableStats> libStats = new Dictionary<string, MetaTableStats>();
				int libCount = MetaTableFactory.LoadMetaTableStats(fileName, libStats);

				while (dao.Read())
				{
					AfsLibrary l = new AfsLibrary();
					l.ProjId = dao.GetInt(0);
					l.MbsProjectName = dao.GetString(1).ToUpper();
					l.ProjectLabel = dao.GetString(2);
					l.MbsDhtFolderName = dao.GetString(3).ToUpper();
					l.DhtFolderLabel = dao.GetString(4).ToUpper();
					l.Platform = dao.GetString(5).ToUpper();
					l.LibId = dao.GetInt(6);
					l.LibName = dao.GetString(7);
					l.LibUse = dao.GetString(8).ToUpper();

					string nodeName = "LIBRARY_" + l.LibId;

					sb.Length = 0;
					sb.Append(l.LibName);

					sb.Append(" (");
					if (!Lex.IsNullOrEmpty(l.LibUse))
					{
						sb.Append(l.LibUse);
						sb.Append(", ");
					}
					sb.Append("Id: ");
					sb.Append(l.LibId);

					if (libStats.ContainsKey(nodeName))
					{
						tempMtn.Size = (int)libStats[nodeName].RowCount;
						tempMtn.UpdateDateTime = libStats[nodeName].UpdateDateTime; // really create date
						sb.Append(", ");
						sb.Append(MetaTreeNode.FormatStatistics(tempMtn));
					}

					sb.Append(")");
					l.LibLabel = sb.ToString();

					libs.Add(l);
				}

				dao.CloseReader();
				return libs;
			}

		}

	}
