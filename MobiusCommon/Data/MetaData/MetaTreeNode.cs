using Mobius.ComOps;

using System;
using System.Text;
using System.Collections.Generic;

namespace Mobius.Data
{

/// <summary>
/// Set of possible node types with one bit assigned to each type
/// </summary>

	[Serializable]
	public enum MetaTreeNodeType 
	{
		// Folder types

		Unknown			=        0,
		Root				=        1,
		Database		=        2,
		DatabaseView	=      4,
		Site				=        8,
		WorkAreaType =      16, // e.g. therapeutic area, hts
		WorkArea		=       32, // e.g. CNS
		Pathway			=       64,
		Platform			=    128, // e.g. Kinase
		Target			=      256, 
		Project			=      512,
		SystemFolder	=   4096, // general system folder 
		UserFolder		=   8192, // user folder containing user objects
		FolderFree =     16384, // next free folder-type bit

		FolderMask = FolderFree - 1, // mask for all folders

// Leaf node types

		MetaTable		 =   131072,
		Url					 =   262144,
		Query				 =   524288,
		CnList			 =  1048576, // compound id list
		Action			 =  2097152, // immediate action
		CalcField	   =  4194304,
		Annotation	 =  8388608,
		CondFormat	 = 16777216,
		Library			 = 33554432,
		ResultsView = 67108864, 
		LeafFree    = 134217728, // next free leaf-type bit

		DataTableTypes = MetaTreeNodeType.MetaTable | MetaTreeNodeType.CalcField | MetaTreeNodeType.Annotation,

		LeafMask		 = (LeafFree - 1) - FolderMask, // mask for all leaves

		All					 = FolderMask | LeafMask
	}

	/// <summary>
	/// Defines a node in the metatree.
	/// </summary>

	[Serializable]
	public class MetaTreeNode : IComparable<MetaTreeNode>
	{
		public string Name = ""; // name of node
		public string Label = ""; // label for node
		public MetaTreeNodeType Type; // Type of node
		public string Target = ""; // target object name or command, defaults to node name
		public int Size = -1; // size of assoc object
		public DateTime UpdateDateTime = DateTime.MinValue; // most-recent update date
		//public object Tag = null; // additional data can be attached here

		public string Owner = "";  // owner if user object - may be changed from UserName to real name
		public bool Shared = true;  // true if shared with the current user

		public MetaTreeNode Parent; // node for which this node is a child (UserObjectTree use only)
		public List<MetaTreeNode> Nodes;  // nodes below this node

/// <summary>
/// Basic constructor
/// </summary>

		public MetaTreeNode()
		{
			Nodes = new List<MetaTreeNode>();
		}

/// <summary>
/// Construct with type 
/// </summary>
/// <param name="type"></param>

		public MetaTreeNode(MetaTreeNodeType type)
		{
			Nodes = new List<MetaTreeNode>();
			Type = type;
			return;
		}

/// <summary>
/// Return true if a folder type node
/// </summary>

		public bool IsFolderType
		{
			get 
			{
				return IsFolderNodeType(Type);		
			}
		}

		public static bool IsFolderNodeType(
			MetaTreeNodeType type)
		{
			bool isFolder = (type & MetaTreeNodeType.FolderMask) != 0;
			//if (!isFolder && type < MetaTreeNodeType.FolderFree) type = type; // debug
			return isFolder;
		}

/// <summary>
/// Return true if leaf type node
/// </summary>

		public bool IsLeafType
		{
			get 
			{
				return IsLeafNodeType(Type);
			}
		}

/// <summary>
/// Return true if leaf type node
/// </summary>
/// <param name="type"></param>
/// <returns></returns>

		public static bool IsLeafNodeType(
			MetaTreeNodeType type)
		{
			bool isLeaf = (type & MetaTreeNodeType.LeafMask) != 0;
			//if (!isLeaf && type >= MetaTreeNodeType.MetaTable) type = type; // debug
			return isLeaf;
		}

/// <summary>
/// Return true if node is a system node type
/// </summary>

		public bool IsSystemType
		{
			get
			{
				return IsSystemNodeType(Type);
			}
		}

/// <summary>
/// Return true if the specified type is a sustem node type
/// </summary>
/// <param name="type"></param>
/// <returns></returns>

		public static bool IsSystemNodeType(
			MetaTreeNodeType type)
		{
			return !IsUserObjectNodeType(type);
		}

/// <summary>
/// Return true if node is a user-object type node
/// </summary>

		public bool IsUserObjectType
		{
			get
			{
				if (Lex.StartsWith(Target, "USERDATABASE_")) return true;
				else return IsUserObjectNodeType(Type);
			}
		}

/// <summary>
/// Return true if the specified type is a user object node type
/// </summary>
/// <param name="type"></param>
/// <returns></returns>
 
		public static bool IsUserObjectNodeType(
			MetaTreeNodeType type)
		{
			switch (type)
			{
				case MetaTreeNodeType.CnList:
				case MetaTreeNodeType.Query:
				case MetaTreeNodeType.Annotation:
				case MetaTreeNodeType.CalcField:
				case MetaTreeNodeType.CondFormat:
				case MetaTreeNodeType.UserFolder:
					return true;

				default:
					return false;
			}
		}

/// <summary>
/// Return true if node is an assay node
/// </summary>

		public bool IsAssayNode
		{
			get
			{
				return IsDataTableNodeType(Type) &&
					(Target.StartsWith("ASSAY_") || Target.Contains("_ASSAY_"));
			}
		}

		/// <summary>
		/// Return true if node is a data table type node
		/// </summary>

		public bool IsDataTableType
		{
			get
			{
				return IsDataTableNodeType(Type);
			}
		}

		public static bool IsDataTableNodeType (
			MetaTreeNodeType type)
		{
			if ((type & MetaTreeNodeType.DataTableTypes) != 0)
				return true;

			else return false;
		}

        public static bool IsMultiSelectableNodeType(
            MetaTreeNodeType type)
        {
            switch (type)
            {
                case MetaTreeNodeType.Annotation:
                case MetaTreeNodeType.CalcField:
                case MetaTreeNodeType.MetaTable:
                case MetaTreeNodeType.CnList:
                case MetaTreeNodeType.Query:
                    return true;

                default:
                    return false;
            }
        }

/// <summary>
/// Return named child node
/// </summary>
/// <param name="childName"></param>
/// <returns></returns>

		public MetaTreeNode GetChild (
			string childName)
		{
			foreach (MetaTreeNode mtn in this.Nodes)
			{
				if (String.Compare(childName,mtn.Name,true) == 0)
					return mtn;
			}
			return null;
		}

/// <summary>
/// Return the target for the node that will go in the client treeview control node
/// </summary>
/// <returns></returns>
		public string GetTarget()
		{
			string target = this.Type.ToString() + " " + this.Target;
			return target;
		}

/// <summary>
/// Convert folder node type to a userobject folder type
/// </summary>
/// <param name="nodeType"></param>
/// <returns></returns>

		public FolderTypeEnum GetUserObjectFolderType ()
		{
			if (this.Type == MetaTreeNodeType.UserFolder) return FolderTypeEnum.User;
			else return FolderTypeEnum.System;
		}

		/// <summary>
		/// Get the image index corresponding to node type
		/// </summary>
		/// <returns></returns>

		public int GetImageIndex()
		{
			return (int)GetImageIndexEnum();
		}

/// <summary>
/// Get the image index corresponding to node type
/// </summary>
/// <returns></returns>

		public Bitmaps16x16Enum GetImageIndexEnum()
		{
			MetaTreeNode mtn = this;

			//if (mtn.Label == "CorpId Structures") mtn = mtn; // debug

			if (IsFolderType)
			{
				switch (Type)
				{
					case MetaTreeNodeType.Root:
						return Bitmaps16x16Enum.World;

					case MetaTreeNodeType.Database:
						return Bitmaps16x16Enum.Books;

					case MetaTreeNodeType.Target:
						return Bitmaps16x16Enum.Target;

					case MetaTreeNodeType.UserFolder:
						if (Lex.Eq(mtn.Label, "Queries"))
							return Bitmaps16x16Enum.QueryFolder;
						else if (Lex.Eq(mtn.Label, "Lists"))
							return Bitmaps16x16Enum.ListFolder;
						else if (Lex.Eq(mtn.Label, "Annotations"))
							return Bitmaps16x16Enum.AnnotationFolder;
						else if (Lex.Eq(mtn.Label, "Calculated Fields"))
							return Bitmaps16x16Enum.CalcFieldFolder;
						else if (Lex.StartsWith(mtn.Label, "Shared ") || Lex.StartsWith(mtn.Label, "Public "))
							return Bitmaps16x16Enum.People;
						else return Bitmaps16x16Enum.Folder;

					case MetaTreeNodeType.Project:
						return Bitmaps16x16Enum.Project;

					default:
						return Bitmaps16x16Enum.Folder;
				}
			}

			// Leaf-type nodes

			switch (Type)
			{
				case MetaTreeNodeType.Url:
					if (Lex.Contains(Target, "Spotfire")) // link to Spotfire webplayer
						return Bitmaps16x16Enum.Spotfire;

					else if (Lex.Contains(Target, "Show") && Lex.Contains(Target, "Description")) // use help image for showing descriptions
						return Bitmaps16x16Enum.Info;

					else return Bitmaps16x16Enum.URL; // generic URL

				case MetaTreeNodeType.Action:
					return Bitmaps16x16Enum.Action;

				case MetaTreeNodeType.CnList:
					if (Shared) return Bitmaps16x16Enum.CidListPublic;
					else return Bitmaps16x16Enum.CidList;

				case MetaTreeNodeType.Query:
					if (Lex.StartsWith(Name, "MULTITABLE_")) // use tables icon for multitable
						return Bitmaps16x16Enum.Tables;
					else
					{
						if (Shared) return Bitmaps16x16Enum.QueryPublic;
						else return Bitmaps16x16Enum.SarTable;
					}

				case MetaTreeNodeType.CalcField:
					if (Shared) return Bitmaps16x16Enum.CalcFieldPublic;
					else return Bitmaps16x16Enum.CalcField;

				case MetaTreeNodeType.Annotation:
					if (Lex.Contains(Label, "STRUCTURES"))  // special case for structure annotation in user databases
						return Bitmaps16x16Enum.Structure;
					else
					{
						if (Shared) return Bitmaps16x16Enum.AnnotationPublic;
						else return Bitmaps16x16Enum.Annotation;
					}

				case MetaTreeNodeType.CondFormat:
					return Bitmaps16x16Enum.CondFormat;

				case MetaTreeNodeType.MetaTable: // must be metatable
					if (Lex.Contains(Name, "_STRUCTURE2") && // special case for struct "plus"
						!Lex.StartsWith(Name, "ORACLE")) // but not in direct Oracle references
						return Bitmaps16x16Enum.StructurePlus;

					else if (Lex.Contains(Name, "_STRUCTURE") && // special case for struct
						!Lex.StartsWith(Name, "ORACLE")) // but not in direct Oracle references
						return Bitmaps16x16Enum.Structure;

					else if (Lex.Eq(Name, MetaTable.SmallWorldMetaTableName))
						return Bitmaps16x16Enum.Structure;

					else return Bitmaps16x16Enum.DataTable;

				case MetaTreeNodeType.Library:
					return Bitmaps16x16Enum.Library;

				case MetaTreeNodeType.ResultsView:
					return Bitmaps16x16Enum.Spotfire;

				default:
					return Bitmaps16x16Enum.World;
			}
		}

		/// <summary>
		/// Parse a node type/name string (e.g. "TABLE QUERY_1234") into a node type & name
		/// </summary>
		/// <param name="typeAndTarget"></param>
		/// <param name="type"></param>
		/// <param name="name"></param>

		public static void ParseTypeAndNameString(
			string typeAndTarget, 
			out MetaTreeNodeType type, 
			out string name)
		{
			type = MetaTreeNodeType.Unknown;
			name = null;

			int i1 = typeAndTarget.IndexOf(" ");
			if (i1 >= 0)
			{
				string typeName = typeAndTarget.Substring(0, i1);
				type = MetaTreeNode.ParseTypeString(typeName);
				name = typeAndTarget.Substring(i1 + 1);
			}
			else // no type supplied
			{
				type = MetaTreeNodeType.Unknown;
				name = typeAndTarget;
			}

			return;
		}

/// <summary>
/// Parse a name string of the form QUERY_123 into the object type and id
/// </summary>
/// <param name="typeAndId"></param>
/// <param name="typeName"></param>
/// <param name="id"></param>

		public static bool ParseTypeNameAndId (
			string typeAndId,
			out string typeName,
			out int id)
		{
			typeName = "";
			string idString = null;
			id = -1;

			int i1 = typeAndId.IndexOf("_");
			if (i1 < 0) return false; // no type supplied

			typeName = typeAndId.Substring(0, i1);
			idString = typeAndId.Substring(i1 + 1);
			return int.TryParse(idString, out id);
		}

/// <summary>
/// Convert a MetaTreeNode type name into a type
/// </summary>
/// <param name="typeName"></param>
/// <returns></returns>

		public static MetaTreeNodeType ParseTypeString(string typeName)
		{
			MetaTreeNodeType type;

			typeName = typeName.ToUpper().Trim();

			if (typeName == "METATABLE" || typeName == "TABLE")
				type = MetaTreeNodeType.MetaTable;
			else if (typeName == "PROJECT")
				type = MetaTreeNodeType.Project;
			else if (typeName == "FOLDER" || typeName == "SYSTEMFOLDER" || typeName == "SUBMENU")
				type = MetaTreeNodeType.SystemFolder;
			else if (typeName == "QUERY")
				type = MetaTreeNodeType.Query;
			else if (typeName == "LIST" || typeName == "CNLIST")
				type = MetaTreeNodeType.CnList;
			else if (typeName == "CALCFIELD")
				type = MetaTreeNodeType.CalcField;
			else if (typeName == "CONDFORMAT")
				type = MetaTreeNodeType.CondFormat;
			else if (typeName == "URL" || typeName == "LINK" || typeName == "URI")
				type = MetaTreeNodeType.Url;
			else if (typeName == "ANNOTATION")
				type = MetaTreeNodeType.Annotation;
			else if (typeName == "LIBRARY")
				type = MetaTreeNodeType.Library;
			else if (typeName == "RESULTSVIEW" || typeName == "SPOTFIRELINK") // (SPOTFIRELINK is old name)
				type = MetaTreeNodeType.ResultsView;
			else if (typeName == "DATABASE")
				type = MetaTreeNodeType.Database;
			else if (typeName == "ROOT")
				type = MetaTreeNodeType.Root;
			else if (typeName == "ACTION")
				type = MetaTreeNodeType.Action;
			else if (typeName == "USERFOLDER")
				type = MetaTreeNodeType.UserFolder;
			else if (typeName == "DATABASEVIEW")
				type = MetaTreeNodeType.DatabaseView;
			else if (typeName == "SITE")
				type = MetaTreeNodeType.Site;
			else if (typeName == "TARGET")
				type = MetaTreeNodeType.Target;
			else if (typeName == "WORKAREATYPE")
				type = MetaTreeNodeType.WorkAreaType;
			else if (typeName == "WORKAREA" || typeName == "TA" || typeName == "THERAPEUTICAREA")
				type = MetaTreeNodeType.WorkArea;
			else type = MetaTreeNodeType.Unknown;

			return type;
		}

		/// <summary>
		/// Compare nodes
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>

		public int CompareTo(MetaTreeNode other)
		{
			int retVal = 0;
			if (other.IsFolderType)
			{
				if (this.IsFolderType)
				{
					retVal = this.Label.CompareTo(other.Label);
				}
				else
				{
					retVal = 1;
				}
			}
			else
			{
				if (this.IsFolderType)
				{
					retVal = -1;
				}
				else
				{
					retVal = this.Label.CompareTo(other.Label);
				}
			}
			return retVal;
		}

		/// <summary>
		/// Format stats for a leaf node
		/// </summary>
		/// <param name="mtn"></param>
		/// <returns></returns>

		public static StringBuilder FormatStatistics(MetaTreeNode mtn)
		{
			StringBuilder sb = new StringBuilder(32);

			if (mtn.IsFolderType) return sb;
			if (mtn.UpdateDateTime == DateTime.MinValue && mtn.Size < 0) return sb;

			if (mtn.UpdateDateTime != DateTime.MinValue)
			{
				if (sb.Length > 0) sb.Append("; ");
				sb.Append(String.Format("Updated: {0:d-MMM-yy}", mtn.UpdateDateTime));
			}

			if (mtn.Size >= 0)
			{
				if (sb.Length > 0) sb.Append("; ");

				if (mtn.Type == MetaTreeNodeType.MetaTable || mtn.Type == MetaTreeNodeType.Annotation) sb.Append("Rows: ");
				else if (mtn.Type == MetaTreeNodeType.Query) sb.Append("Tables: ");
				else if (mtn.Type == MetaTreeNodeType.CnList) sb.Append("Ids: ");
				else if (mtn.Type == MetaTreeNodeType.Library) sb.Append("Compounds: ");

				sb.Append(StringMx.FormatIntegerWithCommas(mtn.Size));
			}

			return sb;
		}

/// <summary>
/// Do a shallow copy of a node to another node
/// </summary>
/// <param name="copyToNode"></param>

		public void MemberwiseCopy(MetaTreeNode copyToNode)
		{
			MetaTreeNode c = copyToNode;

			c.Name = Name; // copy all members except parents & children
			c.Label = Label;
			c.Type = Type;
			c.Target = Target;
			c.Size = Size;
			c.UpdateDateTime = UpdateDateTime;
			c.Owner = Owner;
			c.Shared = Shared;
			c.Parent = Parent;
			c.Nodes = Nodes;
			return;
		}

/// <summary>
/// Clone
/// </summary>
/// <returns></returns>

		public MetaTreeNode Clone()
		{
			return (MetaTreeNode)this.MemberwiseClone();
		}

	} // end of MetaTreeNode

/// <summary>
/// Class for doing simple MetaTreeNode sort compares
/// </summary>

		public class MetaTreeNodeSortComparer : IComparer<MetaTreeNode>
		{
			int IComparer<MetaTreeNode>.Compare(MetaTreeNode mtn1, MetaTreeNode mtn2)
			{
				return mtn1.CompareTo(mtn2);
			}
		}

}
