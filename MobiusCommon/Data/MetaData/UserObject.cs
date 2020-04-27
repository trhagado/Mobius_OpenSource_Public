using Mobius.ComOps;

using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace Mobius.Data
{

/// <summary>
/// Enum of user object types (UserObject.Type)
/// </summary>
	public enum UserObjectType 
	{
		Unknown       =  0,
		CnList        =  1,
		Query         =  2,
		Structure     =  3,
		UserParameter =  4,
		CalcField     =  5,
		Annotation    =  6,
		CondFormat    =  7,
		MetaRename    =  8, // renamed metadata
		Folder        =  9, // user-created folder
		Alert         = 10, // new-data alert
		MultiTable    = 11, // metatable based on a multiple-table query (stored in db as a query)
		UserDatabase  = 12, // user-created compound library with associated annotation & computed data
		ImportState   = 13, // import state for user data (i.e. Annotation tables and UCDBs)
		BackgroundExport = 14, // results of a background export
		Link          = 15, // hyperlink or a link to an existing user object
		UserGroup     = 16,
		SpotfireLink  = 17, // link to Spotfire analysis that retrieves data associated with a query
		MaxValue = 17
	}

/// <summary>
/// Type of folder user object is stored in
/// </summary>

	public enum FolderTypeEnum 
	{
		None   =   0, // used for current list & other objects not in a specific folder
		System = 256, // user object is stored in a system folder from basic non-user part of tree (equiv to MetaTreeNodeType.PersonalFolder)
		User   = 512  // user object is stored in a user created-folder containing user objects (equiv to not MetaTreeNodeType.PersonalFolder)
	}

/// <summary>
/// Enum of user object access levels (UserObject.AccessLevel)
/// </summary>
	public enum UserObjectAccess 
	{
		None          = 0,
		Private       = 1,
		Public        = 2,
		ACL           = 3 // controlled via an Access Control List
	}

	/// <summary>
	/// User objects can be of several types (CnList, Query ...) and are
	/// stored in the MBS_USR_OBJ Oracle table. Each object can have
	/// a ParentFolder which is usually a project identifier or a user folder. 
	/// AccessLevel can be public, private or none.
	/// </summary>

	public class UserObject
	{
		public int Id = 0; // database object identifier in MBS_USR_OBJ.OBJ_ID
		public UserObjectType Type; // CnList, Query, Annotation ...
		public string Owner = ""; // userid of object owner
		public string Name = ""; // name of object
		public string Description = ""; // description
		public string ParentFolder = ""; // id of folder this object is stored in (e.g. KINASE_TIER1, FOLDER_123)
		public FolderTypeEnum ParentFolderType = FolderTypeEnum.None; // type of folder object is stored in (System or User)
		public UserObjectAccess AccessLevel = UserObjectAccess.None; // private or public
		public String ACL = ""; // optional access control list
		public int Count; // number of items in object
		public string Content = ""; // object content
		public DateTime CreationDateTime; // when object was created
		public DateTime UpdateDateTime; // when object last saved

		public UserObject()
		{
		}

		public UserObject( // constructor with type
			UserObjectType type)
		{
			Type = type;
		}

		public UserObject( // constructor with type, owner, name supplied
			UserObjectType type,
			string owner,
			string name)
		{
			Type = type;
			Owner = owner;
			Name = name;
		}

		public UserObject( // constructor with type, owner, folder, name supplied
			UserObjectType type,
			string owner,
			string parentFolder,
			string name)
		{
			Type = type;
			Owner = owner;
			ParentFolder = parentFolder;
			Name = name;
		}

// Prefixes used for userobjects in tree nodes and metatable names (where appropriate)

		public const string TempFolderName = "TEMP_FOLDER";
		public const string TempFolderNameQualified = "TEMP_FOLDER.";

		public const string AnnotationTablePrefix = "ANNOTATION_";
		public const string CalculatedFieldTablePrefix = "CALCFIELD_";
		public const string MultiTablePrefix = "MULTITABLE_";
		public const string SpotfireLinkPrefix = "SPOTFIRELINK_";
		public const string UserDatabasePrefix = "USERDATABASE_";
		public const string UserDatabaseStructurePrefix = "USERDATABASE_STRUCTURE_";

		public const string CompoundIdListNamePrefix = "CNLIST_";

		/// <summary>
/// return true if the specified name is an internal user object name
/// </summary>
/// <param name="internalName"></param>
/// <returns></returns>

		public static bool IsUserObjectInternalName(
			string internalName)
		{
			UserObjectType type;
			int id;

			if (IsUserObjectTableName(internalName))
				return true;

			bool parseResult = ParseObjectTypeAndIdFromInternalName(internalName, out type, out id);
			return parseResult;
		}

/// <summary>
/// Return true if prefixed user database id
/// </summary>
/// <param name="internalName"></param>
/// <returns></returns>

		public static bool IsUserDatabaseId(
			string internalName)
		{
			if (IsUserDatabaseStructureTableName(internalName)) return false; // be sure not structure prefix
			return Lex.StartsWith(internalName, UserDatabasePrefix);
		}

/// <summary>
/// Return true if prefixed user database structure id
/// </summary>
/// <param name="internalName"></param>
/// <returns></returns>

		public static bool IsUserDatabaseStructureTableName(
			string internalName)
		{
			return Lex.StartsWith(internalName, UserDatabaseStructurePrefix);
		}

/// <summary>
/// Return true if prefixed annotation table id
/// </summary>
/// <param name="internalName"></param>
/// <returns></returns>

		public static bool IsAnnotationTableName(
			string internalName)
		{
			return Lex.StartsWith(internalName, AnnotationTablePrefix);
		}

/// <summary>
/// Return true if the MetaTable name appears to be a user object MetaTable
/// </summary>
/// <param name="mtName"></param>
/// <returns></returns>

		public static bool IsUserObjectTableName(string mtName)
		{
			if (Lex.StartsWith(mtName, AnnotationTablePrefix) ||
			 Lex.StartsWith(mtName, CalculatedFieldTablePrefix) ||
			 Lex.StartsWith(mtName, MultiTablePrefix) ||
			 Lex.StartsWith(mtName, SpotfireLinkPrefix) ||
			 Lex.StartsWith(mtName, UserDatabasePrefix) ||
			 Lex.StartsWith(mtName, UserDatabaseStructurePrefix)) return true;
			return false;
		}

	    /// <summary>
/// Return true if name is the correct form for a compound id list
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public static bool IsCompoundIdListName(string name)
		{
			UserObjectType type;
			int id;

			ParseObjectTypeAndIdFromInternalName(name, out type, out id);
			if (type == UserObjectType.CnList) return true;
	        return false;
		}

/// <summary>
/// Return true if folder type object
/// </summary>
/// <param name="type"></param>
/// <returns></returns>

		public static bool IsFolderType(
			UserObjectType type)
        {
            if (type == UserObjectType.Folder ||
				type == UserObjectType.UserDatabase)
				return true;
            return false;
        }

	    /// <summary>
		/// User object types that correspond to metatables
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>

		public static bool IsMetaTableType(
			UserObjectType type)
	    {
	        if (
				type == UserObjectType.Annotation || 
				type == UserObjectType.CalcField) // ||
				// type == UserObjectType.MultiTable)
				return true;
	        return false;
	    }

	    /// <summary>
		/// User object types that are editable
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>

		public static bool IsUserEditableType(
			UserObjectType type)
		{
			if (
				type == UserObjectType.Query ||
				type == UserObjectType.CnList ||
				type == UserObjectType.CalcField ||
				type == UserObjectType.SpotfireLink ||
				type == UserObjectType.CondFormat ||
				type == UserObjectType.Annotation ||
				type == UserObjectType.Folder ||
				type == UserObjectType.MultiTable ||
				type == UserObjectType.UserDatabase)
				return true;

			return false;
		}

		/// <summary>
		/// Return true if this type of UserObject can be copied and pasted
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>

		public static bool CanCopyPaste(UserObjectType type)
		{
			if ( // allowed types for copy
				type == UserObjectType.Annotation ||
				type == UserObjectType.CalcField ||
				type == UserObjectType.SpotfireLink ||
				type == UserObjectType.CnList ||
				type == UserObjectType.MultiTable ||
				type == UserObjectType.Query) return true;
			return false;
		}

		/// <summary>
		/// Return true if this type of UserObject can be copied and pasted
		/// </summary>
		/// <param name="uoArray"></param>
		/// <returns></returns>

		public static bool CanCopyPaste(UserObject[] uoArray)
		{
			bool approved = false;
			foreach (var uo in uoArray)
			{
				if (CanCopyPaste(uo.Type))
					approved = true;
				else return false;
			}

			return approved;
		}

/// <summary>
/// Get the label for an object type
/// </summary>
/// <param name="uot"></param>
/// <returns></returns>

		public static string GetTypeLabel (
			UserObjectType uot)
{
    if (uot == UserObjectType.Query) return "Query";
    if (uot == UserObjectType.CnList) return "List";
		if (uot == UserObjectType.CalcField) return "Calculated Field";
		if (uot == UserObjectType.SpotfireLink) return "Spotfire Link";
		if (uot == UserObjectType.Annotation) return "Annotation";
    if (uot == UserObjectType.CondFormat) return "Conditional Formatting";
    if (uot == UserObjectType.Folder) return "Folder";
    return "";
}

	    /// <summary>
/// Get the plural label for an object type
/// </summary>
/// <param name="uot"></param>
/// <returns></returns>

		public static string GetTypeLabelPlural(
			UserObjectType uot)
		{
			if (uot == UserObjectType.Query) return "Queries";
			if (uot == UserObjectType.CnList) return "Lists";
			if (uot == UserObjectType.CalcField) return "Calculated Fields";
			if (uot == UserObjectType.SpotfireLink) return "Spotfire Links";
			if (uot == UserObjectType.Annotation) return "Annotations";
			if (uot == UserObjectType.CondFormat) return "Conditional Formattings";
			if (uot == UserObjectType.Folder) return "Folders";
			return "";
		}

/// <summary>
/// Get object type from plural of type name
/// </summary>
/// <param name="label"></param>
/// <returns></returns>

		public static UserObjectType GetTypeFromPlural(string label)
		{
			if (Lex.Eq(label, "Queries")) return UserObjectType.Query;
			if (Lex.Eq(label, "Lists")) return UserObjectType.CnList;
			if (Lex.Eq(label, "Calculated Fields")) return UserObjectType.CalcField;
			if (Lex.Eq(label, "Spotfire Links")) return UserObjectType.SpotfireLink;
			if (Lex.Eq(label, "Annotations")) return UserObjectType.Annotation;
			if (Lex.Eq(label, "Conditional Formattings")) return UserObjectType.CondFormat;
			if (Lex.Eq(label, "Folders")) return UserObjectType.Folder;
			return UserObjectType.Unknown;
		}

		public void SetTempObjectInternalName(string tempObjName)
		{
			if (Lex.StartsWith(tempObjName, TempFolderNameQualified))
				tempObjName = tempObjName.Substring(TempFolderNameQualified.Length);

			ParentFolder = TempFolderName;
		}

		/// <summary>
		/// Build text of internal user object name
		/// </summary>
		/// <returns>objectType_objectId (e.g. "QUERY_456") or folderId.name (e.g. "FOLDER_123.My Query")</returns>

		public string InternalName
		{
			get
			{
				string internalName;

				UserObject uo = this;
				if (uo.Type != UserObjectType.Unknown && uo.Id > 0) // if known type & id then build internal name from those
					internalName = uo.Type.ToString().ToUpper() + "_" + uo.Id;

				else if (Lex.Eq(uo.Name, "CURRENT") && uo.Id == 0 && // CURRENT object of type
					uo.Type != UserObjectType.Unknown && String.IsNullOrEmpty(uo.ParentFolder))
					internalName = uo.Type.ToString().ToUpper() + "_" + uo.Id;

				else if (!String.IsNullOrEmpty(uo.ParentFolder) && !String.IsNullOrEmpty(uo.Name))
					internalName = uo.ParentFolder + "." + uo.Name;

				else
				{
					string name = !String.IsNullOrEmpty(uo.Name) ? uo.Name : "(null)";
					string folder = !String.IsNullOrEmpty(uo.ParentFolder) ? uo.ParentFolder : "(null)";
					throw new Exception("Can't assign internal name to UserObject, type: " + uo.Type + ", name: " + name + ", folder: " + folder);
				}

				return internalName;
			}
		}

		/// <summary>
		/// Parse an internal user object name into a UserObject setting type
		/// </summary>
		/// <param name="name">objectType_objectId (e.g. "QUERY_456") or folderId.name (e.g. "FOLDER_123.My Query")</param>
		/// <param name="objType">UserObjectType</param>
		/// <returns></returns>
		/// 
		public static UserObject ParseInternalUserObjectName(
			string objectName,
			UserObjectType objType,
			string userName)
		{
			UserObject uo = ParseInternalUserObjectName(objectName, userName);
			if (uo != null) uo.Type = objType;
			return uo;
		}

		/// <summary>
		/// Parse an internal user object name into a UserObject
		/// </summary>
		/// <param name="name">objectType_objectId (e.g. "QUERY_456") or folderId.name (e.g. "FOLDER_123.My Query")</param>
		/// <returns>UserObject</returns>

		public static UserObject ParseInternalUserObjectName(
			string objectName,
			string userName)
		{
			String owner, name2;
			int i1;

			UserObject uo = new UserObject();

			// Check for special names

			if (Lex.Eq(objectName, "CNLIST_0")) objectName = CidList.CurrentListInternalName; // alias for current list

			if (Lex.StartsWith(objectName, TempFolderNameQualified))
			{
				objectName = objectName.Substring(TempFolderNameQualified.Length);

				uo.ParentFolder = TempFolderName;
				uo.ParentFolderType = 0; // unknown folder type
				uo.Owner = userName;
				uo.Name = objectName;
			}

			else if (objectName.Contains(".")) // assume "FOLDER_123.My Query" format
			{
				i1 = Lex.IndexOf(objectName, ".Folder_"); // see if old form qualified by owner name (e.g. <userName>.FOLDER_49430.bq1)
				if (i1 >= 0)
				{
					userName = objectName.Substring(0, i1);
					objectName = objectName.Substring(i1 + 1); // remove owner name
				}
				i1 = objectName.IndexOf(".");
				uo.Owner = userName;
				uo.ParentFolder = objectName.Substring(0, i1);
				uo.Name = objectName.Substring(i1 + 1);
			}

			else // name containing type and object id (e.g. QUERY_12345)
				ParseObjectTypeAndIdFromInternalName(objectName, out uo.Type, out uo.Id);

			return uo;
		}

/// <summary>
/// Parse a name string of the form QUERY_123 into the object type and id
/// </summary>
/// <param name="internalName"></param>
/// <param name="type"></param>
/// <param name="id"></param>

		public static bool ParseObjectTypeAndIdFromInternalName(
			string internalName, 
			out UserObjectType type, 
			out int id)
		{
			type = UserObjectType.Unknown;
			string idString;
			id = -1;

			int i1 = internalName.IndexOf("_");
			int i2 = internalName.LastIndexOf("_");
			if (i1 >= 0)
			{
				string typeName = internalName.Substring(0, i1);
				type = ParseTypeName(typeName);
				idString = internalName.Substring(i2 + 1);
			}
			else // no type supplied
			{
				type = UserObjectType.Unknown;
				idString = internalName;
			}

			int.TryParse(idString, out id);

			if (type != UserObjectType.Unknown && id >= 0) return true;
            return false;
		}

/// <summary>
/// Convert a user object type name into a UserObjectType
/// </summary>
/// <param name="typeName"></param>
/// <returns></returns>

		public static UserObjectType ParseTypeName(string typeName)
		{
			UserObjectType type;
			typeName = typeName.ToUpper().Trim();

			if (typeName == "CNLIST")
				type = UserObjectType.CnList;
			else if (typeName == "QUERY")
				type = UserObjectType.Query;
			else if (typeName == "STRUCTURE")
				type = UserObjectType.Structure;
			else if (typeName == "USERPARAMETER")
				type = UserObjectType.UserParameter;
			else if (typeName == "CALCFIELD")
				type = UserObjectType.CalcField;
			else if (typeName == "ANNOTATION")
				type = UserObjectType.Annotation;
			else if (typeName == "SPOTFIRELINK")
				type = UserObjectType.SpotfireLink;
			else if (typeName == "CONDFORMAT")
				type = UserObjectType.CondFormat;
			else if (typeName == "METARENAME")
				type = UserObjectType.MetaRename;
			else if (typeName == "FOLDER")
				type = UserObjectType.Folder;
			else if (typeName == "ALERT")
				type = UserObjectType.Alert;
			else if (typeName == "MULTITABLE")
				type = UserObjectType.MultiTable;
			else if (typeName == "USERDATABASE")
				type = UserObjectType.UserDatabase;
			else if (typeName == "IMPORTSTATE")
				type = UserObjectType.ImportState;
			else if (typeName == "BACKGROUNDEXPORT")
				type = UserObjectType.BackgroundExport;
			else type = UserObjectType.Unknown;

			return type;
		}

/// <summary>
/// Parse a user object id from the object name (i.e. ANNOTATION_123)
/// </summary>
/// <param name="internalName"></param>
/// <returns></returns>

		public static int ParseObjectIdFromInternalName(
			string internalName)
		{
			int id;

			if (TryParseObjectIdFromInternalName(internalName, out id))
				return id;

			throw new Exception("Invalid internal name: " + internalName);
		}

/// <summary>
/// Try to parse a user object id from the object name (i.e. ANNOTATION_123)
/// </summary>
/// <param name="internalName"></param>
/// <param name="id"></param>
/// <returns></returns>

		public static bool TryParseObjectIdFromInternalName(
			string internalName,
			out int id)
		{
			id = -1;
			if (Lex.EndsWith(internalName, MetaTable.SummarySuffix)) // remove any summary suffix
				internalName = Lex.Replace(internalName, MetaTable.SummarySuffix, "");
			int i1 = internalName.LastIndexOf("_"); // parse the <Type>_<ObjectId> string
			if (i1 < 0) return false;
			string idString = internalName.Substring(i1 + 1);
			if (!Int32.TryParse(idString, out id)) return false;
			return true;
		}

/// <summary>
/// Parse the object type string from from an internal name
/// </summary>
/// <param name="internalName"></param>
/// <returns></returns>

		public static string ParseObjectTypeStringFromInternalName(
			string internalName)
		{
			UserObjectType type;
			int id;

			ParseObjectTypeAndIdFromInternalName(internalName, out type, out id);
			if (type == UserObjectType.Unknown) 
				throw new Exception("Invalid prefixed id: " + internalName);

			return type.ToString();
		}

/// <summary>
/// Parse the object type string from from an internal name
/// </summary>
/// <param name="internalName"></param>
/// <returns></returns>

		public static UserObjectType ParseObjectTypeFromInternalName(
			string internalName)
		{
			UserObjectType type;
			int id;

			ParseObjectTypeAndIdFromInternalName(internalName, out type, out id);
			if (type == UserObjectType.Unknown) 
				throw new Exception("Invalid prefixed id: " + internalName);

			return type;
		}

/// <summary>
/// Return true if the UserObject has a defined parent folder
/// </summary>

		public bool HasDefinedParentFolder
		{
			get
			{
				return IsDefinedParentFolderName(ParentFolder);
			}
		}

/// <summary>
/// Return true if the UserObject has a defined parent folder
/// </summary>
		
		public bool HasUndefinedParentFolder
		{
			get
			{
				return !IsDefinedParentFolderName(ParentFolder);
			}
		}


/// <summary>
/// Return true if temporary object without parent folder
/// </summary>

		public bool IsTempObject
		{
			get
			{
				return IsTemporaryObjectName(Name, ParentFolder);
			}
		}

/// <summary>
/// Return true if temp object name
/// </summary>
/// <param name="name"></param>
/// <param name="parentFolder"></param>
/// <returns></returns>

		public static bool IsTemporaryObjectName(string name, string parentFolder)
		{
			if (Lex.Eq(parentFolder, TempFolderName)) return true;
			return false;
		}

/// <summary>
/// Return true if "Current" object without parent folder
/// </summary>

		public bool IsCurrentObject
		{
			get
			{
				return IsCurrentObjectName(Name, ParentFolder);
			}
		}

/// <summary>
/// Return true if "Current" object without parent folder
/// </summary>
/// <param name="name"></param>
/// <param name="parentFolder"></param>
/// <returns></returns>

		public static bool IsCurrentObjectName(string name, string parentFolder)
		{
			if ((Lex.Eq(name, "CURRENT") || Lex.Eq(name, "*CURRENT")) && IsUndefinedParentFolderName(parentFolder))
				return true;
			return false;
		}

/// <summary>
/// return true if "Current" object internal name 
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public static bool IsCurrentObjectInternalName(string name)
		{
			if (Lex.Eq(name, "CURRENT") || Lex.Eq(name, "*CURRENT")) 
				return true;

			if (Lex.Eq(name, TempFolderName + ".CURRENT") || Lex.Eq(name, TempFolderName + ".*CURRENT"))
				return true;

			return false;
		}

/// <summary>
/// Return true if the UserObject has a defined parent folder
/// </summary>
/// <param name="parentFolderName"></param>
/// <returns></returns>

		public static bool IsDefinedParentFolderName(string parentFolderName)
		{
			return !IsUndefinedParentFolderName(parentFolderName);
		}

/// <summary>
/// Return true if the UserObject has an undefined parent folder
/// </summary>
/// <param name="parentFolderName"></param>
/// <returns></returns>

		public static bool IsUndefinedParentFolderName(string parentFolderName)
		{
			if (parentFolderName == null || parentFolderName.Trim() == "" || Lex.Eq(parentFolderName, TempFolderName))
				return true;
			return false;
		}

/// <summary>
/// Serialize
/// </summary>
/// <returns></returns>

		public string Serialize()
		{
			XmlMemoryStreamTextWriter mstw = new XmlMemoryStreamTextWriter();
			mstw.Writer.Formatting = Formatting.Indented;
			XmlTextWriter tw = mstw.Writer;

			tw.WriteStartElement("UserObject");

			tw.WriteAttributeString("Id", Id.ToString());
			tw.WriteAttributeString("Type", Type.ToString());
			tw.WriteAttributeString("Owner", Owner);
			tw.WriteAttributeString("Name", Name);
			XmlUtil.WriteAttributeIfDefined(tw, "Description", Description);
			tw.WriteAttributeString("ParentFolder", ParentFolder);
			tw.WriteAttributeString("ParentFolderType", ParentFolderType.ToString());
			tw.WriteAttributeString("AccessLevel", AccessLevel.ToString());
			XmlUtil.WriteAttributeIfDefined(tw, "ACL", ACL);
			tw.WriteAttributeString("Count", Count.ToString());
			if (CreationDateTime != DateTime.MinValue)
				tw.WriteAttributeString("CreationDateTime", DateTimeUS.ToString(CreationDateTime));
			if (UpdateDateTime != DateTime.MinValue)
				tw.WriteAttributeString("UpdateDateTime", DateTimeUS.ToString(UpdateDateTime));

			if (!Lex.IsNullOrEmpty(Content))
			{
				tw.WriteStartElement("Content");
				string cdataClose = "]]>";
				string content = Content;
				while (Lex.Contains(content, cdataClose))
				{
					int p = Lex.IndexOf(content, cdataClose);
					tw.WriteCData(content.Substring(0,p + 2));
					content = content.Substring(p + 2);
				}
				tw.WriteCData(content);
				tw.WriteEndElement(); // end of DataRows
			}

			tw.WriteEndElement();
			string xml = mstw.GetXmlAndClose();
			return xml;
		}

        /// <summary>
        /// Deserialize UserObject XML into an actual UserObject.
        /// </summary>
        /// <param name="userObjectElement"></param>
        /// <returns></returns>
        public static UserObject Deserialize(
        XmlElement userObjectElement)
        {
            UserObject uo = new UserObject();
            if (userObjectElement==null) return uo;
            if (userObjectElement.Name != "UserObject") throw new Exception("UserObject.Deserialize - \"UserObject\" element not found");

            foreach (XmlAttribute attribute in userObjectElement.Attributes)
            {
                switch (attribute.Name)
                {
                    case "Id": uo.Id = int.Parse(attribute.Value);
                        break;
                    case "Name": uo.Name = attribute.Value;
                        break;
                    case "Owner": uo.Owner = attribute.Value;
                        break;
                    case "Description": uo.Description = attribute.Value;
                        break;
                    case "ParentFolder": uo.ParentFolder = attribute.Value;
                        break;
                    case "ACL": uo.ACL = attribute.Value;
                        break;
                    case "Count": uo.Count = int.Parse(attribute.Value);
                        break;
                    case "ParentFolderType": uo.ParentFolderType = (FolderTypeEnum)Enum.Parse(typeof(FolderTypeEnum), attribute.Value);
                        break;
                    case "Type": uo.Type = (UserObjectType)Enum.Parse(typeof(UserObjectType), attribute.Value);
                        break;
                    case "AccessLevel": uo.AccessLevel = (UserObjectAccess)Enum.Parse(typeof(UserObjectAccess), attribute.Value);
                        break;
                    case "CreationDateTime": uo.CreationDateTime = DateTimeUS.Parse(attribute.Value);
                        break;
                    case "UpdateDateTime": uo.UpdateDateTime = DateTimeUS.Parse(attribute.Value);
                        break;
                    default:
                        throw new Exception("UserObject.Deserialize - UserObject property \"" +  attribute.Name + "\" not found");
                }
            }

            // Retrieve Content node
            XmlNode contentElement = userObjectElement.SelectSingleNode("Content");
            if (contentElement == null) return uo;

            // May be multiple CDATA segments. Add these data to the Content Prperty.
            foreach (XmlNode node in contentElement.ChildNodes)
            {
                if (node is XmlCDataSection) uo.Content += node.Value;     
            }

            return uo;
        } 

        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="serializedForm"></param>
        /// <returns></returns>

		public static UserObject Deserialize(
			string serializedForm)
		{
			string txt = null;

			UserObject uo = new UserObject();
			if (Lex.IsNullOrEmpty(serializedForm)) return uo;

			XmlMemoryStreamTextReader mstr = new XmlMemoryStreamTextReader(serializedForm);

			XmlTextReader tr = mstr.Reader;
			tr.Read(); // get CalcField element
			tr.MoveToContent();

			if (!Lex.Eq(tr.Name, "UserObject"))
				throw new Exception("UserObject.Deserialize - \"UserObject\" element not found");

			XmlUtil.GetIntAttribute(tr, "Id", ref uo.Id);
			if (XmlUtil.GetStringAttribute(tr, "Type", ref txt))
				uo.Type = (UserObjectType)Enum.Parse(typeof(UserObjectType), txt);
			XmlUtil.GetStringAttribute(tr, "Owner", ref uo.Owner);
			XmlUtil.GetStringAttribute(tr, "Name", ref uo.Name);
			XmlUtil.GetStringAttribute(tr, "Description", ref uo.Description);
			XmlUtil.GetStringAttribute(tr, "ParentFolder", ref uo.ParentFolder);
			if (XmlUtil.GetStringAttribute(tr, "ParentFolderType", ref txt))
				uo.ParentFolderType = (FolderTypeEnum)Enum.Parse(typeof(FolderTypeEnum), txt);
			if (XmlUtil.GetStringAttribute(tr, "AccessLevel", ref txt))
				uo.AccessLevel = (UserObjectAccess)Enum.Parse(typeof(UserObjectAccess), txt);
			XmlUtil.GetStringAttribute(tr, "ACL", ref uo.ACL);
			XmlUtil.GetIntAttribute(tr, "Count", ref uo.Count);
			if (XmlUtil.GetStringAttribute(tr, "CreationDateTime", ref txt))
				uo.CreationDateTime = DateTimeUS.Parse(txt);
			if (XmlUtil.GetStringAttribute(tr, "UpdateDateTime", ref txt))
				uo.UpdateDateTime = DateTimeUS.Parse(txt);

			tr.Read(); // get any content
			tr.MoveToContent();

			if (Lex.Eq(tr.Name, "Content"))
			{
				tr.Read(); // get any CDATA content
				tr.MoveToContent();
				uo.Content = "";
				while (tr.NodeType == XmlNodeType.CDATA) // may be multiple CDATA segments
				{
					uo.Content += tr.Value;
					tr.Read(); // more CDATA or content end tag
					tr.MoveToContent();
				}
			}

			tr.Read(); // UserObject end tag
			tr.MoveToContent();

			mstr.Close();
			return uo;
		}

		/// <summary>
		/// Serialize a single user object (binary)
		/// </summary>
		/// <returns></returns>

		public byte[] SerializeBinary()
		{
			List<UserObject> list = new List<UserObject>();
			list.Add(this);
			return SerializeListBinary(list);
		}

/// <summary>
/// Serialize a list of user objects (binary)
/// </summary>
/// <param name="objList"></param>
/// <returns></returns>

		public static byte[] SerializeListBinary(
			List<UserObject> objList)
		{
			
			MemoryStream ms = new MemoryStream();
			BinaryWriter bw = new BinaryWriter(ms);

			bw.Write(objList.Count); // count of list elements
			for (int li = 0; li < objList.Count; li++)
			{
				objList[li].SerializeBinary(bw);
			}

			bw.Flush();
			byte[] ba = new byte[ms.Length];
			ms.Seek(0, 0);
			ms.Read(ba, 0, (int)ms.Length);
			bw.Close();

			return ba;
		}

/// <summary>
/// Serialize a single item
/// </summary>
/// <param name="bw"></param>

		void SerializeBinary(
			BinaryWriter bw)
		{
			bw.Write(Id);
			bw.Write((int)Type);
			bw.Write(Lex.S(Owner));
			bw.Write(Lex.S(Name));
			bw.Write(Lex.S(Description));
			bw.Write(Lex.S(ParentFolder));
			bw.Write((int)ParentFolderType);
			bw.Write((int)AccessLevel);
			bw.Write(Lex.S(ACL));
			bw.Write(Lex.S(Content));
			bw.Write(Count);
			bw.Write(CreationDateTime.Ticks);
			bw.Write(UpdateDateTime.Ticks);
		}

/// <summary>
/// Deserialize a single UserObject (binary)
/// </summary>
/// <param name="content"></param>
/// <returns></returns>

		public static UserObject DeserializeBinary(byte[] content)
		{
			List<UserObject> list = DeserializeListBinary(content);
			if (list.Count == 0) return null;
            return list[0];
		}

/// <summary>
/// Deserialize a list of UserObjects (binary)
/// </summary>
/// <param name="content"></param>
/// <returns></returns>

		public static List<UserObject> DeserializeListBinary(byte[] content)
		{
			List<UserObject> list = new List<UserObject>();

			MemoryStream ms = new MemoryStream(content);
			BinaryReader br = new BinaryReader(ms);
			int count = br.ReadInt32();
			//DebugLog.Message("DeserializeListBinary: " + content.Length + ", " + count);
			for (int oi = 0; oi < count; oi++)
			{
				UserObject uo = DeserializeBinary(br);
				list.Add(uo);
			}
			return list;
		}

/// <summary>
/// Deserialize a single item from reader
/// </summary>
/// <param name="br"></param>
/// <returns></returns>

		static UserObject DeserializeBinary(
			BinaryReader br)
		{
			long ticks;

			UserObject uo = new UserObject();

			uo.Id = br.ReadInt32();
			uo.Type = (UserObjectType)br.ReadInt32();
			uo.Owner = br.ReadString();
			uo.Name = br.ReadString();
			uo.Description = br.ReadString();
			uo.ParentFolder = br.ReadString();
			uo.ParentFolderType = (FolderTypeEnum)br.ReadInt32();
			uo.AccessLevel = (UserObjectAccess)br.ReadInt32();
			uo.ACL = br.ReadString();
			uo.Content = br.ReadString();
			uo.Count = br.ReadInt32();
			ticks = br.ReadInt64();
			uo.CreationDateTime = new DateTime(ticks);
			ticks = br.ReadInt64();
			uo.UpdateDateTime = new DateTime(ticks);

			return uo;
		}

/// <summary>
/// Clone UserObject
/// </summary>
/// <returns></returns>

		public UserObject Clone()
		{
			UserObject uo = (UserObject)MemberwiseClone();
			return uo;
		}

	}
}
