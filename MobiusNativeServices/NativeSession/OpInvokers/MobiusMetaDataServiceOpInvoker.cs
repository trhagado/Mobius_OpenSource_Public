using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mobius.Services.Native.ServiceOpCodes;


namespace Mobius.Services.Native.OpInvokers
{
	public class MobiusMetaDataServiceOpInvoker : IInvokeServiceOps
	{
		#region IInvokeServiceOps Members

		object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
		{
			MobiusMetaDataService op = (MobiusMetaDataService)opCode;
			switch (op)
			{
				case MobiusMetaDataService.Build:
					{
						Mobius.MetaFactoryNamespace.MetaTreeFactory.Build();

						Mobius.Data.MetaTree.MetaTreeFactory = new Mobius.MetaFactoryNamespace.MetaTreeFactory(); // factory to call to build metatree
						Mobius.Data.MetaTree.Nodes = Mobius.MetaFactoryNamespace.MetaTreeFactory.Nodes; // be sure nodes are referenced from MetaTree

						return null;
					}

				case MobiusMetaDataService.GetFullTree:
					{
						return Mobius.MetaFactoryNamespace.MetaTreeFactory.Nodes;
					}

				case MobiusMetaDataService.MarkCacheForRebuild:
					{
						if (Mobius.Data.MetaTree.MetaTreeFactory == null)
							Mobius.Data.MetaTree.MetaTreeFactory = new Mobius.MetaFactoryNamespace.MetaTreeFactory();

						Mobius.Data.MetaTree.MetaTreeFactory.MarkCacheForRebuild();
						return null;
					}

				case MobiusMetaDataService.GetAfsSubtree:
					{
						if (Mobius.Data.MetaTree.MetaTreeFactory == null)
							Mobius.Data.MetaTree.MetaTreeFactory = new Mobius.MetaFactoryNamespace.MetaTreeFactory(); 

						string arg = args[0] as string;
						return Mobius.Data.MetaTree.MetaTreeFactory.GetAfsSubtree(arg);
					}

				case MobiusMetaDataService.GetMetaTreeXml:
					{
						string rootNodeName = args[0] as string;
						bool includeCurrentUsersObjects = (bool)args[1];
						bool includeAllUsersObjects = (bool)args[2];
						string treeXml = Mobius.MetaFactoryNamespace.MetaTreeFactory.GetMetatreeXml(rootNodeName, includeCurrentUsersObjects, includeAllUsersObjects);
						return treeXml;
					}

				case MobiusMetaDataService.GetMetaTreeXmlCompressed:
					{
						string rootNodeName = args[0] as string;
						bool includeCurrentUsersObjects = (bool)args[1];
						bool includeAllUsersObjects = (bool)args[2];
						string treeXml = Mobius.MetaFactoryNamespace.MetaTreeFactory.GetMetatreeXml(rootNodeName, includeCurrentUsersObjects, includeAllUsersObjects);
						byte[] ba = Mobius.ComOps.GZip.Compress(treeXml); 
						return ba;
					}

				case MobiusMetaDataService.LockAndReadMetatreeXml:
					{
						string treeXml = Mobius.MetaFactoryNamespace.MetaTreeFactory.LockAndReadMetatreeXml();
						byte[] ba = Mobius.ComOps.GZip.Compress(treeXml);
						return ba;
					}

				case MobiusMetaDataService.SaveMetaTreeXml:
					{
						byte[] ba = args[0] as byte[];
						string treeXml = Mobius.ComOps.GZip.Decompress(ba);
						Mobius.MetaFactoryNamespace.MetaTreeFactory.SaveMetaTreeXml(treeXml);
						return null;
					}

				case MobiusMetaDataService.ReleaseMetaTreeXml:
					{
						Mobius.MetaFactoryNamespace.MetaTreeFactory.ReleaseMetaTreeXml();
						return null;
					}

			}
			return null;
		}

		#endregion
	}
}
