using Mobius.Data;
using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mobius.ClientComponents
{
	public class MetaTreeNodeCollection
	{

		/// <summary>
		/// Get a MetaTreeNode from either MetaTree or UserObjectTree
		/// searching both by name and target
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static MetaTreeNode GetNode(
			string name)
		{
			MetaTreeNode mtn = MetaTree.GetNode(name); // check main tree first

			if (mtn == null) // check UserObject tree if not in main tree
				mtn = UserObjectTree.GetNode(name);

			if (mtn == null) // try by main tree target 
				mtn = MetaTree.GetNodeByTarget(name);

			if (mtn == null) // try UserObject tree target
				mtn = UserObjectTree.GetNodeByTarget(name);

			if (mtn != null) return mtn;
			else return null;
		}
	}
}
