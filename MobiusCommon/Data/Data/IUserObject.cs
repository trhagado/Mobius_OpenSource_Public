using System;
using System.Collections.Generic;
using System.Text;

namespace Mobius.Data
{
	/// <summary>
	/// Interface to basic UserObjectDao functions called from lower level assemblies
	/// </summary>

	public interface IUserObjectDao
	{
		/// <summary>
		/// Read header for specified objectId
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>

		UserObject ReadHeader(
			int id);

		/// <summary>
		/// Read UserObject with specified objectId
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>

		UserObject Read(
			int id);

		/// <summary>
		/// Fetch all objects of a given type regardless of who owns it or where it is located.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="includeContent"></param>
		/// <returns>Set of user objects ordered by obj_id</returns>

		List<UserObject> ReadMultiple(
			UserObjectType type,
			bool includeContent);

	}

	/// <summary>
	/// Interface for callbacks for UserObject Insert/Update/Delete
	/// </summary>

	public interface IUserObjectIUD
	{
		/// <summary>
		/// Method to call in UserObjectTree when user object inserted 
		/// </summary>
		/// <param name="uo"></param>

		void UserObjectInserted(
			UserObject uo);

		/// <summary>
		/// Method to call in UserObjectTree when user object updated
		/// </summary>
		/// <param name="uo"></param>

		void UserObjectUpdated(
			UserObject uo);

		/// <summary>
		/// Method to call in UserObjectTree when user object deleted
		/// </summary>
		/// <param name="uo"></param>
		void UserObjectDeleted(
			UserObject uo);
	}

	/// <summary>
	/// Interface for UserObjectTree functions called from lower level assemblies
	/// </summary>

	public interface IUserObjectTree
	{
		/// <summary>
		/// Assure that the user object tree is built
		/// </summary>

		void AssureTreeIsBuilt();

		/// <summary>
		/// Get list of UserObjects by type
		/// </summary>
		/// <param name="uoType"></param>
		/// <returns></returns>

		List<UserObject> GetUserObjectsByType(
			UserObjectType uoType);

			/// <summary>
			/// Find UserObjectTree folder node
			/// </summary>
			/// <param name="folderName"></param>
			/// <returns></returns>

			MetaTreeNode FindUserObjectFolderNode(
			string folderName);

		/// <summary>
		/// Find a node by target within the full set of user objects
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		MetaTreeNode GetUserObjectNodeBytarget(
			string name);


	}
}
